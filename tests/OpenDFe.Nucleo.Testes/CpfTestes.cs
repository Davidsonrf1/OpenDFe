namespace OpenDFe.Testes;

public class CpfTestes
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    [InlineData("111.444.777-35")]
    [InlineData("00000000191")]
    public void AceitaCpfValido(string valor) => Assert.True(Cpf.Valido(valor));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("52998224724")]         // segundo dígito errado
    [InlineData("52998224715")]         // primeiro dígito errado
    [InlineData("5299822472")]          // 10 dígitos
    [InlineData("529982247250")]        // 12 dígitos
    [InlineData("00000000000")]         // passa no cálculo, todos os dígitos iguais
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    [InlineData("529.982.24725")]       // máscara pela metade
    [InlineData("529-982-247.25")]      // máscara errada
    [InlineData("5299822472A")]
    [InlineData("52998224725 ")]
    public void RecusaCpfInvalido(string? valor) => Assert.False(Cpf.Valido(valor));

    [Theory]
    [InlineData("529982247", "25")]
    [InlineData("111444777", "35")]
    [InlineData("000000001", "91")]
    public void CalculaDigitos(string @base, string digitos) => Assert.Equal(digitos, Cpf.CalcularDigitos(@base));

    [Theory]
    [InlineData("")]
    [InlineData("52998224")]
    [InlineData("5299822470")]
    [InlineData("52998224A")]
    public void CalcularDigitosRecusaBaseInvalida(string @base) =>
        Assert.Throws<ArgumentException>(() => Cpf.CalcularDigitos(@base));

    [Theory]
    [InlineData("52998224725", "529.982.247-25")]
    [InlineData("529.982.247-25", "529.982.247-25")]
    public void Formata(string valor, string esperado) => Assert.Equal(esperado, Cpf.Formatar(valor));

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    public void TiraAMascara(string valor, string esperado) => Assert.Equal(esperado, Cpf.SemMascara(valor));

    [Theory]
    [InlineData("52998224724")]
    [InlineData("11111111111")]
    [InlineData("")]
    public void FormatarRecusaCpfInvalido(string valor)
    {
        Assert.Throws<ArgumentException>(() => Cpf.Formatar(valor));
        Assert.Throws<ArgumentException>(() => Cpf.SemMascara(valor));
    }
}
