namespace OpenDFe.NFSe;

/// <summary>
/// Um evento da NFS-e: o pedido de registro (<c>pedRegEvento</c>, o <c>infPedReg</c>), com o detalhe na subclasse do tipo
/// (docs/NFSE.md 7.5, M11). O Id (<c>PRE</c> + a chave + o tipo) e o texto fixo do tipo (<c>xDesc</c>) ficam fora: o leiaute
/// os escreve e a leitura os confere.
/// </summary>
/// <remarks>
/// A base é fechada: só a lib deriva, uma subclasse por tipo do XSD. Quem faz <c>switch</c> pelo tipo precisa de um
/// <c>default</c>, porque um tipo novo entra em versão menor (MODELO.md, decisão 17). O envio (etapa 9.3) aceita só os de
/// quem emite (<see cref="CancelamentoNFSe"/>, <see cref="AnaliseFiscalCancelamento"/>); os outros a lib só lê.
/// </remarks>
public abstract class EventoNFSe
{
    private protected EventoNFSe()
    {
    }

    /// <summary>O ambiente; nulo na escrita = o da configuração do núcleo (M4).</summary>
    [Tag("tpAmb")]
    public Ambiente? Ambiente { get; set; }

    /// <summary>A versão do aplicativo que gerou o pedido; nulo na escrita = "OpenDFe " e a versão do pacote (M4).</summary>
    [Tag("verAplic")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>A data e a hora do evento.</summary>
    [Tag("dhEvento")]
    public DateTimeOffset? Data { get; set; }

    /// <summary>O CNPJ do autor do evento, só os dígitos; um dos dois (CNPJ ou CPF).</summary>
    [Tag("CNPJAutor")]
    public string? CnpjAutor { get; set; }

    /// <summary>O CPF do autor do evento, só os dígitos.</summary>
    [Tag("CPFAutor")]
    public string? CpfAutor { get; set; }

    /// <summary>A chave de acesso da NFS-e do evento (50 dígitos).</summary>
    [Tag("chNFSe")]
    public string? Chave { get; set; }

    /// <summary>O código do tipo do evento (<c>"101101"</c>), o da tag do detalhe sem o <c>e</c>.</summary>
    public abstract string Tipo { get; }
}
