using System.Globalization;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O formatador do DANFE (etapa 7.2): pt-BR sem depender da cultura do sistema (num Linux enxuto o <c>pt-BR</c> pode faltar),
/// a data e hora como a nota traz, as máscaras de CNPJ, CPF e CEP e a chave em blocos de quatro.
/// </summary>
public sealed class FormatadorImpressaoTestes
{
    [Theory]
    [InlineData("0", "0,00")]
    [InlineData("0.5", "0,50")]
    [InlineData("1", "1,00")]
    [InlineData("27.64", "27,64")]
    [InlineData("999.99", "999,99")]
    [InlineData("1000", "1.000,00")]
    [InlineData("1234.56", "1.234,56")]
    [InlineData("1234567.89", "1.234.567,89")]
    [InlineData("-8", "-8,00")]
    [InlineData("-1234.5", "-1.234,50")]
    [InlineData("0.005", "0,01")]
    [InlineData("0.004", "0,00")]
    public void ODinheiroTemVirgulaDecimalPontoDeMilharEDuasCasas(string valor, string esperado) =>
        Assert.Equal(esperado, FormatadorImpressao.Dinheiro(decimal.Parse(valor, CultureInfo.InvariantCulture)));

    [Theory]
    [InlineData("1", "1")]
    [InlineData("1.0000", "1")]
    [InlineData("2.5000", "2,5")]
    [InlineData("1.125", "1,125")]
    [InlineData("1.2350", "1,235")]
    [InlineData("0.0001", "0,0001")]
    [InlineData("1000", "1.000")]
    [InlineData("1234.5", "1.234,5")]
    [InlineData("0", "0")]
    public void AQuantidadeNaoTemZerosSobrandoADireita(string valor, string esperado) =>
        Assert.Equal(esperado, FormatadorImpressao.Quantidade(decimal.Parse(valor, CultureInfo.InvariantCulture)));

    [Theory]
    [InlineData("27.6400", "27,64")]
    [InlineData("1", "1,00")]
    [InlineData("25.5", "25,50")]
    [InlineData("0.123456", "0,123456")]
    [InlineData("0.1234567890", "0,123456789")]
    [InlineData("12345.6", "12.345,60")]
    public void OValorUnitarioTemAoMenosDuasCasasENaoArredonda(string valor, string esperado) =>
        Assert.Equal(esperado, FormatadorImpressao.Unitario(decimal.Parse(valor, CultureInfo.InvariantCulture)));

    [Fact]
    public void NadaDependeDaCulturaDoSistema()
    {
        var antes = CultureInfo.CurrentCulture;
        try
        {
            foreach (var cultura in new[] { "en-US", "de-DE", "fr-FR", "ar-SA", "th-TH", "" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
                Assert.Equal("1.234,56", FormatadorImpressao.Dinheiro(1234.56m));
                Assert.Equal("1.234,5", FormatadorImpressao.Quantidade(1234.5m));
                Assert.Equal("0,123456", FormatadorImpressao.Unitario(0.123456m));
                Assert.Equal("10/03/2015 15:03:53", FormatadorImpressao.DataHora(new DateTimeOffset(2015, 3, 10, 15, 3, 53, TimeSpan.FromHours(-3))));
                Assert.Equal("000000007", FormatadorImpressao.Zeros(7, 9));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = antes;
        }
    }

    [Fact]
    public void ADataEHoraSaemComoANotaTrazSemConverterOFuso()
    {
        Assert.Equal("10/03/2015 15:03:53", FormatadorImpressao.DataHora(new DateTimeOffset(2015, 3, 10, 15, 3, 53, TimeSpan.FromHours(-3))));
        // Em outro fuso, a hora é a da nota, não a convertida.
        Assert.Equal("10/03/2015 15:03:53", FormatadorImpressao.DataHora(new DateTimeOffset(2015, 3, 10, 15, 3, 53, TimeSpan.FromHours(9))));
        Assert.Equal("10/03/2015 15:03:53", FormatadorImpressao.DataHora(new DateTimeOffset(2015, 3, 10, 15, 3, 53, TimeSpan.Zero)));
        // A virada do dia em UTC não muda o dia impresso.
        Assert.Equal("24/10/2026 23:59:59", FormatadorImpressao.DataHora(new DateTimeOffset(2026, 10, 24, 23, 59, 59, TimeSpan.FromHours(-3))));
        // As frações de segundo não saem.
        Assert.Equal("01/01/2026 00:00:00", FormatadorImpressao.DataHora(new DateTimeOffset(2026, 1, 1, 0, 0, 0, 999, TimeSpan.FromHours(-3))));
    }

    [Fact]
    public void AsMascarasDeDocumentoECep()
    {
        Assert.Equal("11.222.333/0001-81", FormatadorImpressao.Cnpj("11222333000181"));
        Assert.Equal("12.ABC.345/01DE-35", FormatadorImpressao.Cnpj("12ABC34501DE35"));
        Assert.Equal("123", FormatadorImpressao.Cnpj("123"));
        Assert.Equal("529.982.247-25", FormatadorImpressao.Cpf("52998224725"));
        Assert.Equal("123", FormatadorImpressao.Cpf("123"));
        Assert.Equal("30.110-000", FormatadorImpressao.Cep("30110000"));
        Assert.Equal("3011", FormatadorImpressao.Cep("3011"));
    }

    [Fact]
    public void AChaveSaiEmOnzeBlocosDeQuatroDigitos()
    {
        var chave = ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2015, 3, 10, 15, 3, 53, TimeSpan.FromHours(-3)), "11222333000181", Documento.NFCe, 1, 1,
            TipoEmissao.Normal, 12345678).ToString();

        var formatada = FormatadorImpressao.Chave(chave);

        Assert.Equal(54, formatada.Length);
        Assert.Equal(11, formatada.Split(' ').Length);
        Assert.Equal(chave, formatada.Replace(" ", string.Empty, StringComparison.Ordinal));
        Assert.StartsWith("3115 0311 2223 3300 0181 6500 1000 0000 0111", formatada, StringComparison.Ordinal);
        Assert.Equal("1234 56", FormatadorImpressao.Chave("123456"));
    }

    [Fact]
    public void OsZerosAEsquerda()
    {
        Assert.Equal("000000001", FormatadorImpressao.Zeros(1, 9));
        Assert.Equal("001", FormatadorImpressao.Zeros(1, 3));
        Assert.Equal("1234", FormatadorImpressao.Zeros(1234, 3));
    }
}
