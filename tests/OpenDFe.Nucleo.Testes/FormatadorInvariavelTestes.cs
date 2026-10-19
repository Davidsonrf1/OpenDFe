using System.Globalization;

namespace OpenDFe.Testes;

public class FormatadorInvariavelTestes
{
    // pt-BR usa vírgula decimal e ponto de milhar; th-TH, o calendário budista; ar-SA, o Um Al-Qura.
    public static TheoryData<string> Culturas => new() { "", "pt-BR", "th-TH", "ar-SA", "de-CH" };

    [Theory]
    [InlineData("1234.5", 2, "1234.50")]
    [InlineData("1234567.891", 4, "1234567.8910")]
    [InlineData("0", 2, "0.00")]
    [InlineData("0", 0, "0")]
    [InlineData("1", 0, "1")]
    [InlineData("10.1000", 2, "10.10")]        // zeros além das casas do campo saem
    [InlineData("10.123", 2, "10.123")]        // nunca arredonda: o schema aponta o campo
    [InlineData("1.5", 0, "1.5")]
    [InlineData("-3.2", 2, "-3.20")]
    [InlineData("0.0000000001", 10, "0.0000000001")]
    [InlineData("79228162514264337593543950335", 2, "79228162514264337593543950335.00")]
    public void Numero(string valor, int casas, string esperado) =>
        Assert.Equal(esperado, FormatadorInvariavel.Numero(decimal.Parse(valor, CultureInfo.InvariantCulture), casas));

    [Fact]
    public void ZeroNegativoSaiSemSinal() =>
        Assert.Equal("0.00", FormatadorInvariavel.Numero(decimal.Negate(0.000m), 2));

    [Theory]
    [InlineData(-1)]
    [InlineData(29)]
    public void CasasForaDoLimite(int casas) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => FormatadorInvariavel.Numero(1m, casas));

    [Theory]
    [MemberData(nameof(Culturas))]
    public void NumeroNaoDependeDaCultura(string cultura)
    {
        using var _ = new CulturaTemporaria(cultura);
        Assert.Equal("1234567.89", FormatadorInvariavel.Numero(1234567.89m, 2));
        Assert.Equal("-0.5000", FormatadorInvariavel.Numero(-0.5m, 4));
    }

    [Fact]
    public void DataHoraComOFuso()
    {
        var valor = new DateTimeOffset(2026, 10, 19, 14, 30, 5, 987, TimeSpan.FromHours(-3));
        Assert.Equal("2026-10-19T14:30:05-03:00", FormatadorInvariavel.DataHora(valor));
    }

    [Theory]
    [InlineData(0, "2026-01-05T08:00:00+00:00")]
    [InlineData(-4, "2026-01-05T08:00:00-04:00")]
    [InlineData(-5, "2026-01-05T08:00:00-05:00")]
    [InlineData(1, "2026-01-05T08:00:00+01:00")]
    public void DataHoraUsaODeslocamentoDoValor(int horas, string esperado) =>
        Assert.Equal(esperado, FormatadorInvariavel.DataHora(new DateTimeOffset(2026, 1, 5, 8, 0, 0, TimeSpan.FromHours(horas))));

    [Theory]
    [MemberData(nameof(Culturas))]
    public void DataEHoraNaoDependemDaCultura(string cultura)
    {
        using var _ = new CulturaTemporaria(cultura);
        var valor = new DateTimeOffset(2026, 10, 19, 23, 59, 59, TimeSpan.FromHours(-3));
        Assert.Equal("2026-10-19T23:59:59-03:00", FormatadorInvariavel.DataHora(valor));
        Assert.Equal("2026-02-28", FormatadorInvariavel.Data(new DateOnly(2026, 2, 28)));
    }

    [Fact]
    public void Data() => Assert.Equal("2026-01-05", FormatadorInvariavel.Data(new DateOnly(2026, 1, 5)));

    private sealed class CulturaTemporaria : IDisposable
    {
        private readonly CultureInfo _cultura = CultureInfo.CurrentCulture;
        private readonly CultureInfo _culturaInterface = CultureInfo.CurrentUICulture;

        public CulturaTemporaria(string nome)
        {
            var cultura = CultureInfo.GetCultureInfo(nome);
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _cultura;
            CultureInfo.CurrentUICulture = _culturaInterface;
        }
    }
}
