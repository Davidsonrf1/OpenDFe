namespace OpenDFe.Testes;

public class DestinosTestes
{
    [Fact]
    public void AsUFsSaoDestinosComOMesmoCodigoEASigla()
    {
        foreach (var uf in Enum.GetValues<UF>())
        {
            var destino = uf.ComoDestino();
            Assert.Equal((int)uf, (int)destino);
            Assert.Equal(uf.Sigla(), destino.Sigla());
            Assert.True(destino.TentarUF(out var lida));
            Assert.Equal(uf, lida);
        }
        Assert.Equal(27, Enum.GetValues<UF>().Length);
        Assert.Equal(27 + 5, Enum.GetValues<Destino>().Length);
    }

    [Theory]
    [InlineData(Destino.AN, 91, "AN")]
    [InlineData(Destino.SVRS, 101, "SVRS")]
    [InlineData(Destino.SVAN, 102, "SVAN")]
    [InlineData(Destino.SvcAN, 103, "SVC-AN")]
    [InlineData(Destino.SvcRS, 104, "SVC-RS")]
    public void OsAutorizadoresNaoSaoUF(Destino destino, int valor, string sigla)
    {
        Assert.Equal(valor, (int)destino);
        Assert.Equal(sigla, destino.Sigla());
        Assert.False(destino.TentarUF(out var uf));
        Assert.Equal(default, uf);
    }

    [Fact]
    public void LeTodasAsSiglas()
    {
        foreach (var destino in Enum.GetValues<Destino>())
        {
            Assert.True(Destinos.TentarLerSigla(destino.Sigla(), out var lido));
            Assert.Equal(destino, lido);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("mg")]
    [InlineData("svrs")]
    [InlineData("SVCAN")]
    [InlineData("SvcAN")]
    [InlineData("XX")]
    [InlineData(" MG")]
    public void RecusaASiglaQueNaoEDeDestino(string? sigla)
    {
        Assert.False(Destinos.TentarLerSigla(sigla, out var destino));
        Assert.Equal(default, destino);
    }

    [Fact]
    public void RecusaOValorDesconhecido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Destino)99).Sigla());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((UF)99).ComoDestino());
        Assert.False(((Destino)99).TentarUF(out _));
    }
}
