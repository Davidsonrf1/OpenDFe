namespace OpenDFe.NFe;

/// <summary>Uma nota do lote no <see cref="RetornoAutorizacao"/>: a chave, o XML enviado, o protocolo e o <c>nfeProc</c>.</summary>
public sealed class ResultadoNota
{
    private static readonly int[] StatusProcessada = [100, 150, 110, 301, 302];

    internal ResultadoNota(ChaveAcesso? chave, string? xml, Protocolo? protocolo, string? xmlProcessado)
    {
        Chave = chave;
        Xml = xml;
        Protocolo = protocolo;
        XmlProcessado = xmlProcessado;
    }

    /// <summary>A chave de acesso da nota; nula quando o <c>chNFe</c> do protocolo não é uma chave.</summary>
    public ChaveAcesso? Chave { get; }

    /// <summary>
    /// O <c>NFe</c> assinado, como foi enviado: o que se guarda até ter o protocolo (e para o reenvio). Nulo na consulta
    /// do recibo sem os XMLs. Tem dados pessoais: guardar com o cuidado do documento, nunca em log.
    /// </summary>
    public string? Xml { get; }

    /// <summary>
    /// O <c>protNFe</c> da nota (docs/LEITURA.md 2.3), com o <c>cStat</c> dela; nulo enquanto o lote não foi
    /// processado, ou quando a resposta não trouxe o da nota.
    /// </summary>
    public Protocolo? Protocolo { get; }

    /// <summary>
    /// O <c>nfeProc</c>: a nota com os bytes assinados e o <c>protNFe</c>, o XML distribuível (PROJETO.md 8.7). Só com
    /// o protocolo de autorização ou de denegação (100, 150, 110, 301, 302) e o <see cref="Xml"/>; senão nulo.
    /// </summary>
    public string? XmlProcessado { get; }

    /// <summary>O protocolo autoriza o uso: <c>cStat</c> 100 ou 150 (fora de prazo).</summary>
    public bool Autorizada => Protocolo?.Status is 100 or 150;

    internal static bool Processada(int? status) => status is { } valor && StatusProcessada.Contains(valor);
}
