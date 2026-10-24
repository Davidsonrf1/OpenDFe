using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A distribuição DF-e pela lib inteira na homologação real (etapa 8.1): o leiaute de verdade, a URL do Ambiente Nacional
// (hom1.nfe.fazenda.gov.br) e o PFX A1 ICP-Brasil, com as intermediárias no handshake. Consulta a distribuição do CNPJ do PFX
// a partir do NSU 0, uma vez (a NT bloqueia por 1 hora quem consulta de novo sem esperar: 656). Pulado sem o PFX: até ele
// chegar, a etapa fecha na SEFAZ emulada (DistribuicaoEmuladaTestes, PROJETO.md 17).
public class DistribuicaoHomologacaoTestes
{
    [Fact]
    public async Task ADistribuicaoDoCnpjDoCertificadoDevolveOsDocumentosOuNenhumDocumento()
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

        var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(cnpj!, 0), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        Assert.Equal(Destino.AN, resultado.Rastro.Autorizador);
        Assert.Equal("hom1.nfe.fazenda.gov.br", resultado.Rastro.Url!.Host);
        // 138 com documentos, 137 sem nenhum (o primeiro acesso, ou o CNPJ sem notas), ou o 656 de quem consultou há menos de 1 hora.
        Assert.Contains(resultado.CStat!.Value, (int[])[137, 138, 656]);
        Assert.Equal(Ambiente.Homologacao, resultado.Retorno.Ambiente);
        Assert.DoesNotContain(resultado.Problemas, p => p.Severidade == Severidade.Erro);
        if (resultado.CStat == 138)
            Assert.NotEmpty(resultado.Retorno.Documentos);
    }
}
