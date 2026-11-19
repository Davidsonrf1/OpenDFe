namespace OpenDFe;

/// <summary>
/// O pedido montado por <see cref="IOperacaoRest{TParametro, TRetorno}.Montar"/> (docs/NFSE.md, D4): o método, o caminho
/// relativo à base do serviço, o XML que vai compactado no JSON (com o schema, o campo e o que assinar), o destino e os
/// problemas da montagem.
/// </summary>
/// <remarks>
/// O núcleo confere a coerência no <see cref="DFe.ExecutarAsync{TRetorno}"/> (erro de programação do leiaute,
/// <see cref="InvalidOperationException"/>): o POST leva o <see cref="Xml"/>, com o <see cref="Schema"/> e o
/// <see cref="CampoXml"/>; o GET e o HEAD não levam; o <see cref="Destino"/> é obrigatório, se o parâmetro não o traz.
/// </remarks>
public sealed class PedidoRest
{
    private readonly IReadOnlyList<string> _idsAssinatura = [];
    private readonly IReadOnlyList<Problema> _problemas = [];
    private readonly IReadOnlyList<DocumentoPedido> _documentos = [];
    private readonly Destino? _destino;
    private readonly string? _campoXml;

    /// <summary>Cria o pedido.</summary>
    /// <param name="servico">O nome da base na tabela de URLs (<c>"SefinNacional"</c>).</param>
    /// <param name="metodo">O método HTTP.</param>
    /// <param name="caminho">
    /// O caminho relativo à base, já com os valores (<c>"nfse"</c>, <c>"dps/DPS3106..."</c>), sem a barra do começo.
    /// </param>
    /// <exception cref="ArgumentException">O serviço ou o caminho são nulos ou vazios, ou o caminho é absoluto.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O método não é conhecido.</exception>
    public PedidoRest(string servico, MetodoRest metodo, string caminho)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(servico);
        ArgumentException.ThrowIfNullOrWhiteSpace(caminho);
        if (!Enum.IsDefined(metodo))
            throw new ArgumentOutOfRangeException(nameof(metodo), metodo, "O valor não é um método.");
        if (caminho.StartsWith('/') || caminho.Contains("://", StringComparison.Ordinal) || caminho.Contains('?') || caminho.Contains('#'))
            throw new ArgumentException($"O caminho {caminho} não é relativo à base do serviço (sem a barra do começo, o esquema, a consulta e o fragmento).", nameof(caminho));
        Servico = servico;
        Metodo = metodo;
        Caminho = caminho;
    }

    /// <summary>O nome da base na tabela de URLs (<c>"SefinNacional"</c>).</summary>
    public string Servico { get; }

    /// <summary>O método HTTP.</summary>
    public MetodoRest Metodo { get; }

    /// <summary>O caminho relativo à base.</summary>
    public string Caminho { get; }

    /// <summary>
    /// O documento que vai no corpo (a DPS, o <c>pedRegEvento</c>), sem a assinatura; <see langword="null"/> = sem corpo
    /// (GET, HEAD). Com um problema de severidade <see cref="Severidade.Erro"/> em <see cref="Problemas"/>, pode estar
    /// incompleto.
    /// </summary>
    public string? Xml { get; init; }

    /// <summary>O schema do <see cref="Xml"/>, validado depois de assinar.</summary>
    public SchemaXml? Schema { get; init; }

    /// <summary>
    /// O campo do JSON que leva o <see cref="Xml"/> assinado, em GZip e Base64 (<c>"dpsXmlGZipB64"</c>).
    /// </summary>
    /// <exception cref="ArgumentException">O nome é vazio.</exception>
    public string? CampoXml
    {
        get => _campoXml;
        init
        {
            if (value is not null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("O nome do campo do JSON é vazio.", nameof(value));
            _campoXml = value;
        }
    }

    /// <summary>Os <c>Id</c> dos elementos que o núcleo assina, na ordem (o <c>infDPS</c>, o <c>infPedReg</c>).</summary>
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
    /// O destino (<see cref="OpenDFe.Destino.SefinNacional"/>): obrigatório, porque não há UF para cair. O
    /// <see cref="ParametroWS.Destino"/> sobrepõe este.
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
    /// Os problemas da montagem. Com um de severidade <see cref="Severidade.Erro"/>, nada é assinado nem enviado.
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista ou um dos problemas são nulos.</exception>
    public IReadOnlyList<Problema> Problemas
    {
        get => _problemas;
        init => _problemas = PedidoWS.Copiar(value);
    }

    /// <summary>
    /// Os documentos do <see cref="Xml"/>, para as regras do leiaute e os callbacks de quem usa, como no
    /// <see cref="PedidoWS.Documentos"/>; vazio quando o pedido não leva documento.
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
}
