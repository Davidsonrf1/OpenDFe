namespace OpenDFe.Testes;

public class CnpjTestes
{
    [Theory]
    [InlineData("12ABC34501DE35")]      // exemplo oficial da Receita Federal (CNPJ alfanumérico)
    [InlineData("12.ABC.345/01DE-35")]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    [InlineData("00000000000191")]
    [InlineData("00.000.000/0001-91")]
    public void AceitaCnpjValido(string valor) => Assert.True(Cnpj.Valido(valor));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12ABC34501DE36")]      // segundo dígito errado
    [InlineData("12ABC34501DE45")]      // primeiro dígito errado
    [InlineData("12abc34501de35")]      // minúsculas
    [InlineData("12ABC34501DE3A")]      // dígito verificador com letra
    [InlineData("11222333000182")]
    [InlineData("1122233300018")]       // 13 posições
    [InlineData("112223330001811")]     // 15 posições
    [InlineData("00000000000000")]      // passa no cálculo, todas as posições iguais
    [InlineData("11111111111111")]
    [InlineData("12.ABC.345/01DE.35")]  // máscara errada
    [InlineData("12ABC345/01DE-35")]    // máscara pela metade
    [InlineData(" 12ABC34501DE35")]
    [InlineData("12ABC345-1DE35")]
    [InlineData("12ÁBC34501DE35")]
    public void RecusaCnpjInvalido(string? valor) => Assert.False(Cnpj.Valido(valor));

    [Theory]
    [InlineData("12ABC34501DE", "35")]
    [InlineData("112223330001", "81")]
    [InlineData("000000000001", "91")]
    public void CalculaDigitos(string @base, string digitos) => Assert.Equal(digitos, Cnpj.CalcularDigitos(@base));

    [Theory]
    [InlineData("A1B2C3D4E5F6")]
    [InlineData("ZZZZZZZZ0001")]
    [InlineData("98765432ABCD")]
    public void BaseComDigitosCalculadosEhValida(string @base) =>
        Assert.True(Cnpj.Valido(@base + Cnpj.CalcularDigitos(@base)));

    [Fact]
    public void LetraValeAsciiMenos48()
    {
        // 'A' vale 17: a base "00000000000A" tem soma 17 × 2 = 34 no primeiro dígito (resto 1 → 0)
        // e 17 × 3 = 51 no segundo (resto 7 → 4).
        Assert.Equal("04", Cnpj.CalcularDigitos("00000000000A"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12ABC34501D")]
    [InlineData("12ABC34501DEF")]
    [InlineData("12abc34501de")]
    [InlineData("12.ABC.345/0")]
    public void CalcularDigitosRecusaBaseInvalida(string @base) =>
        Assert.Throws<ArgumentException>(() => Cnpj.CalcularDigitos(@base));

    [Theory]
    [InlineData("12ABC34501DE35", "12.ABC.345/01DE-35")]
    [InlineData("12.ABC.345/01DE-35", "12.ABC.345/01DE-35")]
    [InlineData("11222333000181", "11.222.333/0001-81")]
    public void Formata(string valor, string esperado) => Assert.Equal(esperado, Cnpj.Formatar(valor));

    [Theory]
    [InlineData("12.ABC.345/01DE-35", "12ABC34501DE35")]
    [InlineData("12ABC34501DE35", "12ABC34501DE35")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    public void TiraAMascara(string valor, string esperado) => Assert.Equal(esperado, Cnpj.RemoverMascara(valor));

    [Theory]
    [InlineData("12ABC34501DE36")]
    [InlineData("12abc34501de35")]
    [InlineData("")]
    public void FormatarRecusaCnpjInvalido(string valor)
    {
        Assert.Throws<ArgumentException>(() => Cnpj.Formatar(valor));
        Assert.Throws<ArgumentException>(() => Cnpj.RemoverMascara(valor));
    }
}
