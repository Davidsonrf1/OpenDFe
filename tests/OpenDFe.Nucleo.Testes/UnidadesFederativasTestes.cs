namespace OpenDFe.Testes;

public class UnidadesFederativasTestes
{
    // Códigos IBGE das 27 UFs.
    public static TheoryData<UF, string, int> Todas => new()
    {
        { UF.RO, "RO", 11 }, { UF.AC, "AC", 12 }, { UF.AM, "AM", 13 }, { UF.RR, "RR", 14 },
        { UF.PA, "PA", 15 }, { UF.AP, "AP", 16 }, { UF.TO, "TO", 17 },
        { UF.MA, "MA", 21 }, { UF.PI, "PI", 22 }, { UF.CE, "CE", 23 }, { UF.RN, "RN", 24 },
        { UF.PB, "PB", 25 }, { UF.PE, "PE", 26 }, { UF.AL, "AL", 27 }, { UF.SE, "SE", 28 },
        { UF.BA, "BA", 29 },
        { UF.MG, "MG", 31 }, { UF.ES, "ES", 32 }, { UF.RJ, "RJ", 33 }, { UF.SP, "SP", 35 },
        { UF.PR, "PR", 41 }, { UF.SC, "SC", 42 }, { UF.RS, "RS", 43 },
        { UF.MS, "MS", 50 }, { UF.MT, "MT", 51 }, { UF.GO, "GO", 52 }, { UF.DF, "DF", 53 },
    };

    [Theory]
    [MemberData(nameof(Todas))]
    public void SiglaECodigoIbge(UF uf, string sigla, int codigo)
    {
        Assert.Equal(sigla, uf.Sigla());
        Assert.Equal(codigo, uf.Codigo());
        Assert.True(UnidadesFederativas.TentarLerSigla(sigla, out var lidaSigla));
        Assert.Equal(uf, lidaSigla);
        Assert.True(UnidadesFederativas.TentarLerCodigo(codigo, out var lidaCodigo));
        Assert.Equal(uf, lidaCodigo);
    }

    [Fact]
    public void SaoVinteESete() => Assert.Equal(27, Enum.GetValues<UF>().Length);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("mg")]
    [InlineData("Mg")]
    [InlineData("XX")]
    [InlineData("EX")]
    [InlineData("AN")]
    [InlineData("31")]
    [InlineData("MGS")]
    [InlineData(" MG")]
    public void RecusaSiglaQueNaoEhDeUF(string? sigla)
    {
        Assert.False(UnidadesFederativas.TentarLerSigla(sigla, out var uf));
        Assert.Equal(default, uf);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(34)]
    [InlineData(91)]
    [InlineData(-31)]
    public void RecusaCodigoQueNaoEhDeUF(int codigo)
    {
        Assert.False(UnidadesFederativas.TentarLerCodigo(codigo, out var uf));
        Assert.Equal(default, uf);
    }

    [Fact]
    public void ValorForaDaEnumeracaoEhErroDeProgramacao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((UF)99).Sigla());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((UF)99).Codigo());
    }
}
