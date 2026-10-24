namespace OpenDFe.NFe;

/// <summary>
/// O resumo de um evento de NF-e (<c>resEvento</c> 1.01, NT 2014.002, 3.11.2), que o Ambiente Nacional distribui antes da
/// manifestação. Imutável.
/// </summary>
public sealed class ResumoEvento
{
    internal ResumoEvento(int orgao, string autor, ChaveAcesso chave, DateTimeOffset momento, TipoEvento tipo, int sequencia,
        string descricao, DateTimeOffset recebimento, string protocolo)
    {
        Orgao = orgao;
        Autor = autor;
        Chave = chave;
        Momento = momento;
        Tipo = tipo;
        Sequencia = sequencia;
        Descricao = descricao;
        Recebimento = recebimento;
        Protocolo = protocolo;
    }

    /// <summary>O código do órgão que recebeu o evento (<c>cOrgao</c>: o da UF, ou 91 para o Ambiente Nacional).</summary>
    public int Orgao { get; }

    /// <summary>O CNPJ ou o CPF do autor do evento, sem máscara (a norma o chama de emitente).</summary>
    public string Autor { get; }

    /// <summary>A chave de acesso da NF-e (<c>chNFe</c>).</summary>
    public ChaveAcesso Chave { get; }

    /// <summary>A data e a hora do evento (<c>dhEvento</c>).</summary>
    public DateTimeOffset Momento { get; }

    /// <summary>O tipo do evento (<c>tpEvento</c>): aberto, um tipo que a enumeração não nomeia chega como o número, convertido.</summary>
    public TipoEvento Tipo { get; }

    /// <summary>O número sequencial do evento (<c>nSeqEvento</c>).</summary>
    public int Sequencia { get; }

    /// <summary>A descrição do evento (<c>xEvento</c>).</summary>
    public string Descricao { get; }

    /// <summary>A data e a hora do registro do evento (<c>dhRecbto</c>).</summary>
    public DateTimeOffset Recebimento { get; }

    /// <summary>O número do protocolo do evento (<c>nProt</c>).</summary>
    public string Protocolo { get; }
}
