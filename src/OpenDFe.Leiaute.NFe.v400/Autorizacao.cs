namespace OpenDFe.NFe;

/// <summary>
/// A autorização de 1 a 50 notas (<c>NFeAutorizacao4</c>, docs/CONTRATOS.md 14): uma nota vai na resposta síncrona
/// (<c>indSinc=1</c>); de 2 a 50, no lote com recibo, que se consulta com <see cref="ConsultaRecibo"/>. A quantidade
/// decide, porque desde a NT 2025.001 o lote de uma nota é rejeitado (452) e a síncrona com mais de uma também (764).
/// </summary>
/// <remarks>
/// <para>
/// As notas vêm do modelo (o leiaute escreve com os valores do núcleo; o núcleo assina, valida e transmite) ou já
/// assinadas (o <see cref="DFe.Preparar{TDocumento}"/>, o reenvio depois de uma queda), que vão com os mesmos bytes, sem
/// a declaração XML. As do modelo vão ao autorizador da UF e do tipo de emissão do núcleo; as assinadas, ao da UF e do
/// tipo de emissão da chave (PROJETO.md 8.4).
/// </para>
/// <para>
/// O problema de uma nota leva o índice dela no parâmetro na propriedade (<c>Notas[0].Itens[2].Produto.Valor</c>,
/// <c>Xmls[1].Emitente.Cnpj</c>) e o caminho da nota (<c>NFe/infNFe/det[3]/prod/vProd</c>).
/// </para>
/// <para>
/// A nota do modelo não é alterada: executar de novo a mesma nota, com o <c>cNF</c> nulo, sorteia outra chave. O reenvio
/// depois de uma queda usa o XML assinado (<see cref="ResultadoNota.Xml"/>, <see cref="RastroWS.XmlPedido"/>) ou a nota
/// preparada antes (docs/CONTRATOS.md 14, decisão 13).
/// </para>
/// </remarks>
public sealed class Autorizacao : ParametroWS<RetornoAutorizacao>
{
    /// <summary>O máximo de notas num lote (o <c>enviNFe</c>).</summary>
    public const int MaximoNotas = 50;

    private const long MaiorLote = 999_999_999_999_999;

    private readonly long? _lote;

    /// <summary>A autorização das notas do modelo.</summary>
    /// <param name="notas">De 1 a 50 notas.</param>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">Nenhuma nota, mais de 50, ou uma nula.</exception>
    public Autorizacao(params IEnumerable<NotaFiscal> notas)
    {
        ArgumentNullException.ThrowIfNull(notas);
        NotaFiscal[] lista = [.. notas];
        Conferir(lista.Length, nameof(notas));
        if (Array.Exists(lista, n => n is null))
            throw new ArgumentException("Uma das notas é nula.", nameof(notas));
        Notas = lista.AsReadOnly();
        Xmls = [];
    }

    /// <summary>A autorização das notas já assinadas (o <c>NFe</c>), com os mesmos bytes.</summary>
    /// <param name="xmls">De 1 a 50 notas assinadas.</param>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">Nenhuma nota, mais de 50, ou uma nula ou vazia.</exception>
    public Autorizacao(params IEnumerable<string> xmls)
    {
        ArgumentNullException.ThrowIfNull(xmls);
        string[] lista = [.. xmls];
        Conferir(lista.Length, nameof(xmls));
        if (Array.Exists(lista, string.IsNullOrWhiteSpace))
            throw new ArgumentException("Uma das notas assinadas é nula ou vazia.", nameof(xmls));
        Notas = [];
        Xmls = lista.AsReadOnly();
    }

    /// <summary>As notas do modelo; vazia quando vieram os XMLs.</summary>
    public IReadOnlyList<NotaFiscal> Notas { get; }

    /// <summary>As notas assinadas; vazia quando vieram as notas do modelo.</summary>
    public IReadOnlyList<string> Xmls { get; }

    /// <summary>
    /// O número do lote (<c>idLote</c>, de 1 a 15 dígitos); nulo: do relógio da configuração, em UTC
    /// (<c>AAMMDDhhmmssmmm</c>). A SEFAZ não exige que seja único.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Menor que 1 ou com mais de 15 dígitos.</exception>
    public long? Lote
    {
        get => _lote;
        init
        {
            if (value is { } lote && lote is < 1 or > MaiorLote)
                throw new ArgumentOutOfRangeException(nameof(value), lote, "O lote vai de 1 a 15 dígitos.");
            _lote = value;
        }
    }

    /// <summary>
    /// As notas assinadas, com os bytes como foram, tiradas da mensagem enviada: o <see cref="RastroWS.XmlPedido"/> do
    /// resultado ou da <see cref="FalhaTransmissaoException"/> (docs/CONTRATOS.md 15, decisão 9). É o XML da nota do
    /// modelo para a <see cref="RecuperacaoAutorizacao"/> e o reenvio depois de uma queda.
    /// </summary>
    /// <param name="xmlPedido">O <c>enviNFe</c> enviado.</param>
    /// <returns>Os <c>NFe</c> do lote, na ordem.</returns>
    /// <exception cref="ArgumentNullException">A mensagem é nula.</exception>
    /// <exception cref="FormatException">A mensagem não é um <c>enviNFe</c> da NF-e.</exception>
    public static IReadOnlyList<string> NotasEnviadas(string xmlPedido)
    {
        ArgumentNullException.ThrowIfNull(xmlPedido);
        if (!LoteNFe.Lote(xmlPedido))
            throw new FormatException("A mensagem não é o enviNFe da autorização.");
        return LoteNFe.Enviadas(xmlPedido).AsReadOnly();
    }

    private static void Conferir(int quantidade, string nome)
    {
        if (quantidade == 0)
            throw new ArgumentException("A autorização precisa de pelo menos uma nota.", nome);
        if (quantidade > MaximoNotas)
            throw new ArgumentException($"O lote vai até {MaximoNotas} notas: vieram {quantidade}.", nome);
    }
}
