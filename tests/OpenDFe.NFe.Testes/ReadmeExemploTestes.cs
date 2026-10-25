using System.Security.Cryptography.X509Certificates;
using OpenDFe.Impressao.Pdf;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using QuestPDF.Infrastructure;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// O exemplo do <c>README.md</c> do repositório (criar o núcleo, autorizar, imprimir), compilado e executado contra a SEFAZ
/// emulada: o método <see cref="Exemplo"/> tem as mesmas chamadas do trecho do README, só com o certificado, as conexões e as
/// URLs vindos do teste no lugar do PFX e da SEFAZ real.
/// </summary>
public sealed class ReadmeExemploTestes : IClassFixture<EmuladorFixture>
{
    private readonly Emulador _emulador;

    public ReadmeExemploTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    // O trecho do README (as duas primeiras linhas do README abrem o PFX e criam as conexões; aqui chegam prontas).
    private static async Task<byte[]?> Exemplo(X509Certificate2 certificado, ConexoesSefaz conexoes, IReadOnlyList<EnderecoServico> urls, NotaFiscal nota,
        CancellationToken ct)
    {
        var leiaute = new LeiauteNFe400();
        var dfe = DFe.Criar(leiaute, Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
            Urls = urls,
        });

        var resultado = await dfe.ExecutarAsync(new Autorizacao(nota), ct);
        var autorizada = resultado.Retorno!.Notas[0];
        if (!autorizada.Autorizada)
            return null;

        QuestPDF.Settings.License = LicenseType.Community; // a escolha é de quem usa (o README do pacote de PDF)
        return RenderizadorPdf.Gerar(leiaute.Danfe(leiaute.Ler(autorizada.XmlProcessado!)));
    }

    [Fact]
    public async Task OExemploDoReadmeAutorizaEGeraOPdfDoDanfe()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var nota = ApoioEmulado.Nota(Documento.NFe, 1, DateTimeOffset.UtcNow);

        var pdf = await Exemplo(ApoioEmulado.Certificado, conexoes, ApoioEmulado.Urls(_emulador, Documento.NFe), nota, TestContext.Current.CancellationToken);

        Assert.NotNull(pdf);
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdf!, 0, 5), StringComparison.Ordinal);
    }
}
