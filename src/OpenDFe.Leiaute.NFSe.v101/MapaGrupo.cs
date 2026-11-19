namespace OpenDFe.NFSe;

/// <summary>
/// O mapa de uma classe do modelo (<see cref="MapasNFSe"/>): os campos na ordem do XSD e as escolhas (docs/NFSE.md 7, M7).
/// A escrita (<see cref="EscritaMapa"/>), a leitura (<see cref="LeituraMapa"/>) e o caminho do problema
/// (<see cref="CaminhoModeloNFSe"/>) andam pelo mesmo mapa. Imutável e seguro em paralelo.
/// </summary>
internal sealed class MapaGrupo
{
    private readonly Dictionary<string, CampoMapa> _tags;

    private MapaGrupo(Type tipo, CampoMapa[] campos, string[][] escolhas)
    {
        Tipo = tipo;
        Campos = campos;
        Escolhas = escolhas;
        _tags = new Dictionary<string, CampoMapa>(StringComparer.Ordinal);
        foreach (var campo in campos)
        {
            _tags.Add(campo.Tag, campo);
            foreach (var outra in campo.Outras)
                _tags.Add(outra, campo);
        }
    }

    /// <summary>A classe do modelo.</summary>
    internal Type Tipo { get; }

    /// <summary>Os campos, na ordem do XSD 1.01 (os da 1.00 no lugar em que estavam).</summary>
    internal IReadOnlyList<CampoMapa> Campos { get; }

    /// <summary>As escolhas do XSD: em cada uma, só uma das propriedades pode estar preenchida.</summary>
    internal IReadOnlyList<string[]> Escolhas { get; }

    /// <summary>Cria o mapa da classe.</summary>
    internal static MapaGrupo Criar<T>(CampoMapa[] campos, string[][] escolhas) where T : class =>
        new(typeof(T), campos, escolhas);

    /// <summary>O campo da tag (ou de uma tag da versão 1.00 que vai para a mesma propriedade); nulo se não há.</summary>
    internal CampoMapa? Campo(string tag) => _tags.GetValueOrDefault(tag);
}

/// <summary>
/// Um campo do mapa: a tag, a propriedade e como o valor vai e volta do XML. As subclasses são os tipos de valor
/// (<see cref="Campo{T}"/> as cria).
/// </summary>
internal abstract class CampoMapa
{
    private protected CampoMapa(string tag, string propriedade, bool versao100, IReadOnlyList<string>? outras)
    {
        Tag = tag;
        Propriedade = propriedade;
        Versao100 = versao100;
        Outras = outras ?? [];
    }

    /// <summary>A tag no XML.</summary>
    internal string Tag { get; }

    /// <summary>O nome da propriedade no modelo.</summary>
    internal string Propriedade { get; }

    /// <summary>Se o campo é só da versão 1.00: a leitura preenche, a escrita da 1.01 não escreve (M10).</summary>
    internal bool Versao100 { get; }

    /// <summary>As tags da versão 1.00 que a leitura põe na mesma propriedade (<c>desc</c> → <c>xNome</c>).</summary>
    internal IReadOnlyList<string> Outras { get; }

    /// <summary>O mapa do grupo, nos campos que guardam um grupo ou uma lista de grupos.</summary>
    internal virtual MapaGrupo? Subgrupo => null;

    /// <summary>Se o elemento se repete (lista).</summary>
    internal virtual bool Repete => false;

    /// <summary>Se o campo está preenchido no objeto (o texto vazio e a lista vazia não contam).</summary>
    internal abstract bool Preenchido(object dono);

    /// <summary>Escreve o campo do objeto; o nulo não sai.</summary>
    internal abstract void Escrever(EscritaNFSe e, object dono);

    /// <summary>Lê um elemento do campo para o objeto.</summary>
    internal abstract void Ler(LeituraNFSe l, object dono, System.Xml.Linq.XElement elemento);
}
