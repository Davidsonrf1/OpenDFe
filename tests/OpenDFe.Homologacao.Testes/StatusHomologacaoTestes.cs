using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O status do serviço pela lib inteira na homologação real (etapa 4.3): o leiaute de verdade, as URLs dele e o PFX
// A1 ICP-Brasil, com as intermediárias no handshake (como o ClienteSefaz da prova). Pulado sem o PFX: até ele chegar,
// a etapa fecha na SEFAZ emulada (StatusEmuladoTestes, PROJETO.md 17).
public class StatusHomologacaoTestes
{
    [Theory]
    [InlineData(Documento.NFe, UF.MG, Destino.MG)]
    [InlineData(Documento.NFe, UF.SC, Destino.SVRS)]
    [InlineData(Documento.NFe, UF.SP, Destino.SP)]
    [InlineData(Documento.NFCe, UF.MG, Destino.MG)]
    public async Task ConsultaOStatusEmHomologacao(Documento documento, UF uf, Destino autorizador)
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = uf,
            Certificado = certificado,
            Conexoes = conexoes,
        });

        var resultado = await dfe.ExecutarAsync(new ConsultaStatus(), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado);
        Assert.Contains(resultado.CStat!.Value, (int[])[107, 108, 109]);
        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        Assert.Equal(Ambiente.Homologacao, resultado.Retorno.Ambiente);
        Assert.Empty(resultado.Problemas);
    }
}
