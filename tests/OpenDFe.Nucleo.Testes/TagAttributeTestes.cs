using System.Reflection;

namespace OpenDFe.Testes;

// O atributo [Tag] (PROJETO.md 4.1; docs/MODELO.md, decisões 11 e 12): o núcleo só o define; quem o lê é o leiaute.
public class TagAttributeTestes
{
    private class GrupoExemplo
    {
        [Tag("xNome", "E04")]
        public virtual string? Nome { get; set; }

        [Tag("nItem", "H02")]
        public int Numero { get; set; }

        public string? Livre { get; set; }
    }

    private sealed class GrupoDerivado : GrupoExemplo
    {
        public override string? Nome { get; set; }
    }

    [Tag("refNF", "BA03")]
    private sealed class EscolhaExemplo
    {
    }

    private static TagAttribute? Ler(Type tipo, string propriedade) =>
        tipo.GetProperty(propriedade)!.GetCustomAttribute<TagAttribute>(inherit: true);

    [Fact]
    public void GuardaATagEOIdentificador()
    {
        var atributo = new TagAttribute("xNome", "E04");

        Assert.Equal("xNome", atributo.Tag);
        Assert.Equal("E04", atributo.Identificador);
    }

    [Fact]
    public void GuardaSoATagSemIdentificador()
    {
        var atributo = new TagAttribute("xLgr");

        Assert.Equal("xLgr", atributo.Tag);
        Assert.Null(atributo.Identificador);
    }

    [Fact]
    public void ELidoDaClasse()
    {
        var atributo = typeof(EscolhaExemplo).GetCustomAttribute<TagAttribute>();

        Assert.Equal(("refNF", "BA03"), (atributo?.Tag, atributo?.Identificador));
    }

    [Fact]
    public void ELidoDaPropriedade()
    {
        var nome = Ler(typeof(GrupoExemplo), nameof(GrupoExemplo.Nome));
        var numero = Ler(typeof(GrupoExemplo), nameof(GrupoExemplo.Numero));

        Assert.Equal(("xNome", "E04"), (nome?.Tag, nome?.Identificador));
        Assert.Equal(("nItem", "H02"), (numero?.Tag, numero?.Identificador));
        Assert.Null(Ler(typeof(GrupoExemplo), nameof(GrupoExemplo.Livre)));
    }

    [Fact]
    public void VemDaPropriedadeSobrescrita()
    {
        var nome = Attribute.GetCustomAttribute(typeof(GrupoDerivado).GetProperty(nameof(GrupoDerivado.Nome))!, typeof(TagAttribute), inherit: true);

        Assert.Equal("xNome", Assert.IsType<TagAttribute>(nome).Tag);
    }

    [Fact]
    public void ValeEmPropriedadeEClasseUmaVez()
    {
        var uso = typeof(TagAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        Assert.NotNull(uso);
        Assert.Equal(AttributeTargets.Property | AttributeTargets.Class, uso.ValidOn);
        Assert.False(uso.AllowMultiple);
        Assert.True(uso.Inherited);
    }
}
