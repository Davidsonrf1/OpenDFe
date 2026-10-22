namespace OpenDFe.NFe;

/// <summary>
/// O retorno da <see cref="ConsultaProtocolo"/> (o <c>retConsSitNFe</c>, docs/CONTRATOS.md 15): a situação da nota, o
/// protocolo dela e os eventos. Imutável.
/// </summary>
/// <remarks>
/// O <see cref="Status"/> é a situação da nota na base: 100 e 150 autorizada, 101 (151, 155) cancelada, 110, 301 e 302
/// denegada; ou 217 (não consta), 562 (outro código numérico na base), 561, 613 e as rejeições da consulta.
/// </remarks>
public sealed class RetornoProtocolo
{
    internal RetornoProtocolo(int status, string motivo, IReadOnlyList<string> eventos)
    {
        Status = status;
        Motivo = motivo;
        Eventos = eventos;
    }

    /// <summary>O <c>cStat</c>: a situação da nota, ou a rejeição da consulta.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c>.</summary>
    public string Motivo { get; }

    /// <summary>O <c>tpAmb</c>; nulo quando não veio ou não se leu.</summary>
    public Ambiente? Ambiente { get; internal init; }

    /// <summary>O <c>verAplic</c>.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>O <c>cUF</c>: a UF do autorizador que atendeu.</summary>
    public UF? UF { get; internal init; }

    /// <summary>O <c>dhRecbto</c>.</summary>
    public DateTimeOffset? Recebimento { get; internal init; }

    /// <summary>O <c>chNFe</c> da resposta; nulo quando não se lê (nas rejeições da mensagem, vem com zeros).</summary>
    public ChaveAcesso? Chave { get; internal init; }

    /// <summary>O <c>protNFe</c> da autorização ou da denegação (docs/LEITURA.md 2.3), quando a nota consta.</summary>
    public Protocolo? Protocolo { get; internal init; }

    /// <summary>
    /// Cada <c>procEventoNFe</c> como veio (o cancelamento, a carta de correção), na ordem da resposta. O evento lido
    /// num tipo próprio vem na fase 6.
    /// </summary>
    public IReadOnlyList<string> Eventos { get; }

    /// <summary>
    /// No 562 (a nota da base tem outro código numérico), a chave da nota da base, quando o motivo a traz
    /// (<c>[chNFe:...]</c>); senão nula.
    /// </summary>
    public ChaveAcesso? ChaveRegistrada { get; internal init; }

    /// <summary>A nota está autorizada: <see cref="Status"/> 100 ou 150 (fora de prazo).</summary>
    public bool Autorizada => Status is 100 or 150;

    /// <summary>A nota foi cancelada: <see cref="Status"/> 101, 151 ou 155 (fora de prazo).</summary>
    public bool Cancelada => Status is 101 or 151 or 155;
}
