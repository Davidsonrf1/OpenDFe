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
/// Na propriedade que guarda um grupo (<c>NotaFiscal.Destinatario</c>), é a tag do grupo (<c>dest</c>, <c>E01</c>);
/// no atributo do XML, o nome sem <c>@</c> (<c>nItem</c>, <c>H02</c>). Na classe, é a tag da escolha do XML que a
/// subclasse representa (<c>refNF</c>, <c>BA03</c>), quando a escolha é um grupo. Os valores não são conferidos
/// aqui: são constantes do código do modelo, conferidas pelos testes do leiaute.
/// </para>
/// <para>
/// Sem identificador (<see cref="TagAttribute(string)"/>) nos campos de uma classe que o XML usa em mais de um
/// lugar, onde o identificador do MOC muda com o lugar (<c>xLgr</c> é F03 na retirada e G03 na entrega): o
/// identificador do grupo fica na propriedade que o guarda.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TagAttribute : Attribute
{
    /// <summary>Marca o campo com a tag, sem o identificador do MOC.</summary>
    /// <param name="tag">O nome local da tag no XML (<c>"xLgr"</c>), sem prefixo nem namespace.</param>
    public TagAttribute(string tag)
    {
        Tag = tag;
    }

    /// <summary>Marca o campo com a tag e o identificador do campo.</summary>
    /// <param name="tag">O nome local da tag no XML (<c>"xNome"</c>), sem prefixo nem namespace.</param>
    /// <param name="identificador">O identificador do campo no leiaute do MOC (<c>"E04"</c>).</param>
    public TagAttribute(string tag, string identificador)
    {
        Tag = tag;
        Identificador = identificador;
    }

    /// <summary>O nome local da tag no XML (<c>"xNome"</c>).</summary>
    public string Tag { get; }

    /// <summary>
    /// O identificador do campo no leiaute do MOC (<c>"E04"</c>); nulo no campo de uma classe usada em mais de um
    /// lugar.
    /// </summary>
    public string? Identificador { get; }
}
