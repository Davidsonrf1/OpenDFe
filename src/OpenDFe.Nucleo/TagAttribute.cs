namespace OpenDFe;

/// <summary>
/// A tag do XML e o identificador do campo no manual (MOC) que uma propriedade do modelo representa
/// (PROJETO.md 4.1): <c>[Tag("xNome", "E04")]</c> em <c>Destinatario.Nome</c>.
/// </summary>
/// <remarks>
/// <para>
/// Serve à documentação (a tag aparece com a propriedade), ao problema que aponta a propriedade do modelo a
/// partir do caminho no XML (<see cref="Problema.Propriedade"/>) e ao aviso do campo preenchido que o leiaute
/// carregado não escreve (PROJETO.md 4.4). Quem lê o atributo é o leiaute; o núcleo só o define, porque ele vale
/// para o modelo de qualquer documento.
/// </para>
/// <para>
/// Na propriedade que guarda um grupo (<c>Nota.Destinatario</c>), é a tag do grupo (<c>dest</c>, <c>E01</c>); no
/// atributo do XML, o nome sem <c>@</c> (<c>nItem</c>, <c>H02</c>). Os valores não são conferidos aqui: são
/// constantes do código do modelo, conferidas pelos testes do leiaute.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class TagAttribute : Attribute
{
    /// <summary>Marca a propriedade com a tag e o identificador do campo.</summary>
    /// <param name="tag">O nome local da tag no XML (<c>"xNome"</c>), sem prefixo nem namespace.</param>
    /// <param name="identificador">O identificador do campo no leiaute do MOC (<c>"E04"</c>).</param>
    public TagAttribute(string tag, string identificador)
    {
        Tag = tag;
        Identificador = identificador;
    }

    /// <summary>O nome local da tag no XML (<c>"xNome"</c>).</summary>
    public string Tag { get; }

    /// <summary>O identificador do campo no leiaute do MOC (<c>"E04"</c>).</summary>
    public string Identificador { get; }
}
