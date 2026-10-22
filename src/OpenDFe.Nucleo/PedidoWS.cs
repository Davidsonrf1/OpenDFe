namespace OpenDFe;

/// <summary>
/// O pedido montado por <see cref="IOperacao{TParametro, TRetorno}.Montar"/>: a mensagem, o serviço, o schema,
/// o que assinar, o destino e os problemas da montagem.
/// </summary>
public sealed class PedidoWS
{
    private readonly IReadOnlyList<string> _idsAssinatura = [];
    private readonly IReadOnlyList<Problema> _problemas = [];
    private readonly IReadOnlyList<DocumentoPedido> _documentos = [];
    private readonly Destino? _destino;
    private readonly TipoEmissao? _emissao;

    /// <summary>Cria o pedido.</summary>
    /// <param name="servico">O nome do serviço na tabela de URLs (<c>"NFeStatusServico4"</c>).</param>
    /// <param name="xml">
    /// A mensagem, sem assinatura. Com um problema de severidade <see cref="Severidade.Erro"/> em
    /// <see cref="Problemas"/>, pode estar incompleta ou vazia.
    /// </param>
    /// <param name="schema">O schema da mensagem, validado depois de assinar.</param>
    /// <exception cref="ArgumentNullException">O XML ou o schema são nulos.</exception>
    /// <exception cref="ArgumentException">O serviço é nulo ou vazio.</exception>
    public PedidoWS(string servico, string xml, SchemaXml schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(servico);
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentNullException.ThrowIfNull(schema);
        Servico = servico;
        Xml = xml;
        Schema = schema;
    }

    /// <summary>O nome do serviço (<c>"NFeStatusServico4"</c>).</summary>
    public string Servico { get; }

    /// <summary>A mensagem, sem assinatura.</summary>
    public string Xml { get; }

    /// <summary>O schema da mensagem.</summary>
    public SchemaXml Schema { get; }

    /// <summary>
    /// Os <c>Id</c> dos elementos que o núcleo assina, na ordem (um <c>infNFe</c> por nota do lote, um
    /// <c>infEvento</c> por evento); vazio quando a mensagem não é assinada ou já vem assinada.
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">Um <c>Id</c> é nulo ou vazio.</exception>
    public IReadOnlyList<string> IdsAssinatura
    {
        get => _idsAssinatura;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            var copia = value.ToArray();
            if (copia.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Um dos Id a assinar é nulo ou vazio.", nameof(value));
            _idsAssinatura = copia;
        }
    }

    /// <summary>
    /// Os documentos da mensagem, na ordem (uma nota por nota do lote), para as regras do leiaute e os callbacks de
    /// quem usa; vazio quando a mensagem não leva documento (status, consulta). Um <c>Id</c> que não está no XML
    /// assinado é erro de programação do leiaute (<see cref="InvalidOperationException"/> no
    /// <see cref="DFe.ExecutarAsync{TRetorno}"/>).
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista ou um dos documentos são nulos.</exception>
    /// <exception cref="ArgumentException">Dois documentos com o mesmo <c>Id</c>.</exception>
    public IReadOnlyList<DocumentoPedido> Documentos
    {
        get => _documentos;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            var copia = value.ToArray();
            if (Array.Exists(copia, d => d is null))
                throw new ArgumentNullException(nameof(value), "Um dos documentos é nulo.");
            if (copia.Select(d => d.Id).Distinct(StringComparer.Ordinal).Count() != copia.Length)
                throw new ArgumentException("Dois documentos com o mesmo Id.", nameof(value));
            _documentos = copia;
        }
    }

    /// <summary>
    /// O destino que sai do documento ou da chave (PROJETO.md 8.4); <see langword="null"/> = o do núcleo (a UF).
    /// O <see cref="ParametroWS.Destino"/> sobrepõe este.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é um destino.</exception>
    public Destino? Destino
    {
        get => _destino;
        init
        {
            if (value is { } destino && !Enum.IsDefined(destino))
                throw new ArgumentOutOfRangeException(nameof(value), destino, "O valor não é um destino.");
            _destino = value;
        }
    }

    /// <summary>
    /// O tipo de emissão que sai do documento ou da chave (PROJETO.md 8.4); <see langword="null"/> = o do núcleo.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é um tipo de emissão.</exception>
    public TipoEmissao? Emissao
    {
        get => _emissao;
        init
        {
            if (value is { } emissao && !Enum.IsDefined(emissao))
                throw new ArgumentOutOfRangeException(nameof(value), emissao, "O valor não é um tipo de emissão.");
            _emissao = value;
        }
    }

    /// <summary>
    /// Os problemas da montagem (regras do leiaute, campo não suportado). Com um de severidade
    /// <see cref="Severidade.Erro"/>, nada é assinado nem enviado.
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista ou um dos problemas são nulos.</exception>
    public IReadOnlyList<Problema> Problemas
    {
        get => _problemas;
        init => _problemas = Copiar(value);
    }

    internal static Problema[] Copiar(IReadOnlyList<Problema> problemas)
    {
        ArgumentNullException.ThrowIfNull(problemas);
        var copia = problemas.ToArray();
        if (Array.Exists(copia, p => p is null))
            throw new ArgumentNullException(nameof(problemas), "Um dos problemas é nulo.");
        return copia;
    }
}
