using System.Reflection;

namespace OpenDFe.Testes;

// O atributo [Tag] (PROJETO.md 4.1): o núcleo só o define; quem o lê é o leiaute.
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
    public void ValeSoEmPropriedadeUmaVez()
    {
        var uso = typeof(TagAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        Assert.NotNull(uso);
        Assert.Equal(AttributeTargets.Property, uso.ValidOn);
        Assert.False(uso.AllowMultiple);
        Assert.True(uso.Inherited);
    }
}
