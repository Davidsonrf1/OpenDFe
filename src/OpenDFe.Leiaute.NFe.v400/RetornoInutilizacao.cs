namespace OpenDFe.NFe;

/// <summary>O retorno da <see cref="Inutilizacao"/> (o <c>retInutNFe</c>). Imutável.</summary>
public sealed class RetornoInutilizacao
{
    internal RetornoInutilizacao(int status, string motivo)
    {
        Status = status;
        Motivo = motivo;
    }

    /// <summary>O <c>cStat</c>: 102 (homologada), 563 (a faixa já foi inutilizada) ou a rejeição.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c>.</summary>
    public string Motivo { get; }

    /// <summary>O <c>tpAmb</c>.</summary>
    public Ambiente? Ambiente { get; internal init; }

    /// <summary>O <c>verAplic</c>.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>O <c>cUF</c>: a UF que atendeu.</summary>
    public UF? UF { get; internal init; }

    /// <summary>O <c>ano</c> (2 dígitos), só no 102 e no 563.</summary>
    public int? Ano { get; internal init; }

    /// <summary>O <c>CNPJ</c>, só no 102 e no 563.</summary>
    public string? Cnpj { get; internal init; }

    /// <summary>O <c>mod</c>, só no 102 e no 563.</summary>
    public Documento? Modelo { get; internal init; }

    /// <summary>A <c>serie</c>, só no 102 e no 563.</summary>
    public int? Serie { get; internal init; }

    /// <summary>O <c>nNFIni</c>, só no 102 e no 563.</summary>
    public int? NumeroInicial { get; internal init; }

    /// <summary>O <c>nNFFin</c>, só no 102 e no 563.</summary>
    public int? NumeroFinal { get; internal init; }

    /// <summary>O <c>dhRecbto</c>.</summary>
    public DateTimeOffset? Recebimento { get; internal init; }

    /// <summary>O <c>nProt</c>: o da inutilização (no 563, o da anterior).</summary>
    public string? Protocolo { get; internal init; }

    /// <summary>O <c>ProcInutNFe</c> (o <c>inutNFe</c> com os bytes enviados e o <c>retInutNFe</c>): só quando homologada (102).</summary>
    public string? XmlProcessado { get; internal init; }

    /// <summary>A inutilização foi homologada: <see cref="Status"/> 102.</summary>
    public bool Homologada => Status == 102;
}
