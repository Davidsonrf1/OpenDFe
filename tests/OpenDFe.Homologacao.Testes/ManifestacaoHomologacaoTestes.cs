using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A manifestação do destinatário pela lib inteira na homologação real (etapa 8.2): o leiaute de verdade, a URL do Ambiente
// Nacional (hom1.nfe.fazenda.gov.br) e o PFX A1 ICP-Brasil, com as intermediárias no handshake. Pega, pela distribuição do CNPJ
// do PFX, uma nota destinada a ele e manifesta a ciência. Pulado sem o PFX, e sem nota destinada ao CNPJ: até o PFX chegar, a
// etapa fecha na SEFAZ emulada (ManifestacaoEmuladaTestes, PROJETO.md 17).
public class ManifestacaoHomologacaoTestes
{
    [Fact]
    public async Task AAscienciaDeUmaNotaDestinadaAoCnpjDoCertificadoERegistradaOuRecusadaPeloPrazoOuPelaOrdem()
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
        var ct = TestContext.Current.CancellationToken;

        var distribuicao = await dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(cnpj!, 0), ct);
        Assert.True(distribuicao.Processado, string.Join(Environment.NewLine, distribuicao.Problemas));
        var resumo = distribuicao.Retorno.Documentos.FirstOrDefault(d => d.ResumoNFe is not null)?.ResumoNFe;
        Assert.SkipWhen(resumo is null, "Não há nota destinada ao CNPJ do PFX na distribuição (ou o AN respondeu 656): nada a manifestar.");

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(new CienciaOperacao(resumo!.Chave, cnpj!)), ct);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        Assert.Equal(Destino.AN, resultado.Rastro.Autorizador);
        Assert.Equal("hom1.nfe.fazenda.gov.br", resultado.Rastro.Url!.Host);
        Assert.Equal(128, resultado.CStat);
        var evento = Assert.Single(resultado.Retorno.Eventos);
        // 135 registrada; 596 (a nota passou de 10 dias), 655 (já houve conclusiva), 650 (cancelada) ou 573 (já registrada).
        Assert.Contains(evento.Status, (int[])[135, 573, 596, 650, 655]);
        Assert.DoesNotContain(resultado.Problemas, p => p.Severidade == Severidade.Erro);
    }
}
