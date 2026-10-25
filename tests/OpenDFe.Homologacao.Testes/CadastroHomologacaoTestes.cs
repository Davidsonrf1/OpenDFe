using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A consulta cadastro pela lib inteira na homologação real (etapa 8.2): o leiaute de verdade, a URL do autorizador de MG
// (hnfe.fazenda.mg.gov.br) e o PFX A1 ICP-Brasil, com as intermediárias no handshake. Consulta o CNPJ do PFX em MG: o
// contribuinte pode não ter inscrição lá (259), então o teste aceita o 111, o 112 e o 259. Pulado sem o PFX: até ele chegar, a
// etapa fecha na SEFAZ emulada (CadastroEmuladoTestes, PROJETO.md 17).
public class CadastroHomologacaoTestes
{
    [Fact]
    public async Task AConsultaDoCnpjDoCertificadoEmMgDevolveOContribuinteOuNaoCadastrado()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var cnpj = CertificadoDigital.LerTitular(certificado)?.Cnpj;
        Assert.SkipWhen(cnpj is null, "O PFX não é de um CNPJ.");
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
        });

        var resultado = await dfe.ExecutarAsync(new ConsultaCadastro(UF.MG, cnpj!), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.Equal("hnfe.fazenda.mg.gov.br", resultado.Rastro.Url!.Host);
        Assert.Contains(resultado.CStat!.Value, (int[])[111, 112, 259]);
        Assert.Equal(resultado.CStat is 111 or 112, resultado.Retorno.Encontrado);
        Assert.DoesNotContain(resultado.Problemas, p => p.Severidade == Severidade.Erro);
    }
}
