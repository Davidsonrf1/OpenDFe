namespace OpenDFe.NFe;

/// <summary>O resultado de um evento do <see cref="RetornoEvento"/> (o <c>retEvento</c>). Imutável.</summary>
public sealed class ResultadoEvento
{
    internal ResultadoEvento(int status, string motivo)
    {
        Status = status;
        Motivo = motivo;
    }

    /// <summary>O evento enviado, ligado pela chave, o tipo e a sequência (ou pela posição); nulo no que sobrou do retorno.</summary>
    public Evento? Evento { get; internal init; }

    /// <summary>O <c>chNFe</c> do retorno.</summary>
    public ChaveAcesso? Chave { get; internal init; }

    /// <summary>O <c>cStat</c> do evento: 135, 136, 155 ou a rejeição.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c>.</summary>
    public string Motivo { get; }

    /// <summary>O <c>xEvento</c>.</summary>
    public string? Descricao { get; internal init; }

    /// <summary>O <c>nProt</c> do evento.</summary>
    public string? Protocolo { get; internal init; }

    /// <summary>O <c>dhRegEvento</c>.</summary>
    public DateTimeOffset? Registro { get; internal init; }

    /// <summary>O evento assinado, como foi enviado. Tem dados do documento: guardar com cuidado, nunca em log.</summary>
    public string? XmlEvento { get; internal init; }

    /// <summary>O <c>procEventoNFe</c> (o evento com os bytes enviados e o <c>retEvento</c>): só quando registrado; o XML distribuível.</summary>
    public string? XmlProcessado { get; internal init; }

    /// <summary>O evento foi registrado: <see cref="Status"/> 135 (vinculado), 136 (não vinculado) ou 155 (cancelamento fora de prazo).</summary>
    public bool Registrado => Status is 135 or 136 or 155;
}
