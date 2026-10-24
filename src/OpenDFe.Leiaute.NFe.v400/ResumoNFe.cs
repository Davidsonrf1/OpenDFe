namespace OpenDFe.NFe;

/// <summary>
/// O resumo de uma NF-e (<c>resNFe</c> 1.01, NT 2014.002, 3.11.1): o que o Ambiente Nacional distribui ao destinatário
/// antes da manifestação, com o necessário para ele se manifestar. Imutável.
/// </summary>
public sealed class ResumoNFe
{
    internal ResumoNFe(ChaveAcesso chave, string emitente, string nomeEmitente, string inscricaoEstadual, DateTimeOffset emissao,
        TipoNota tipoNota, decimal valorTotal, DateTimeOffset recebimento, string protocolo, SituacaoResumo situacao)
    {
        Chave = chave;
        Emitente = emitente;
        NomeEmitente = nomeEmitente;
        InscricaoEstadual = inscricaoEstadual;
        Emissao = emissao;
        TipoNota = tipoNota;
        ValorTotal = valorTotal;
        Recebimento = recebimento;
        Protocolo = protocolo;
        Situacao = situacao;
    }

    /// <summary>A chave de acesso da NF-e (<c>chNFe</c>).</summary>
    public ChaveAcesso Chave { get; }

    /// <summary>O CNPJ (14 posições, numérico ou alfanumérico) ou o CPF (11) do emitente, sem máscara.</summary>
    public string Emitente { get; }

    /// <summary>A razão social ou o nome do emitente (<c>xNome</c>).</summary>
    public string NomeEmitente { get; }

    /// <summary>A inscrição estadual do emitente (<c>IE</c>): os algarismos, <c>ISENTO</c> ou vazia (não contribuinte).</summary>
    public string InscricaoEstadual { get; }

    /// <summary>A data e a hora de emissão (<c>dhEmi</c>), com o fuso que veio.</summary>
    public DateTimeOffset Emissao { get; }

    /// <summary>O tipo de operação (<c>tpNF</c>): entrada ou saída.</summary>
    public TipoNota TipoNota { get; }

    /// <summary>O valor total da NF-e (<c>vNF</c>).</summary>
    public decimal ValorTotal { get; }

    /// <summary>O <c>digVal</c> (o resumo da assinatura da NF-e na base do Ambiente Nacional); nulo se não veio.</summary>
    public string? Digest { get; internal init; }

    /// <summary>A data e a hora da autorização (<c>dhRecbto</c>).</summary>
    public DateTimeOffset Recebimento { get; }

    /// <summary>O número do protocolo de autorização (<c>nProt</c>).</summary>
    public string Protocolo { get; }

    /// <summary>A situação da NF-e (<c>cSitNFe</c>): aberta, pode ser um número que a enumeração não nomeia.</summary>
    public SituacaoResumo Situacao { get; }
}
