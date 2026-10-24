using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf.Testes;

public class LicencaQuestPdfTestes
{
    [Fact]
    public void SemLicencaAMensagemExplicaOCodigoEApontaOReadme()
    {
        var erro = Assert.Throws<InvalidOperationException>(() => LicencaQuestPdf.Exigir(null));
        Assert.Contains("QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;", erro.Message);
        Assert.Contains("https://github.com/Davidsonrf1/OpenDFe/blob/main/src/OpenDFe.Impressao.Pdf/README.md", erro.Message);
        Assert.Contains("exige", erro.Message);
    }

    [Theory]
    [InlineData(LicenseType.Community)]
    [InlineData(LicenseType.Professional)]
    [InlineData(LicenseType.Enterprise)]
    public void ComLicencaNaoLanca(LicenseType licenca) => LicencaQuestPdf.Exigir(licenca);

    [Fact]
    public void OReadmeDoPacoteTemALicencaNoPrimeiroBlocoEOEnderecoDaMensagem()
    {
        var readme = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "README.Pdf.md"));
        var linhas = readme.Split('\n');
        Assert.StartsWith("# OpenDFe.Impressao.Pdf", linhas[0]);
        Assert.StartsWith("> **Licença", linhas[2]);          // o primeiro bloco depois do título é o aviso de licença
        Assert.Contains("questpdf.com/license", readme);
    }
}
