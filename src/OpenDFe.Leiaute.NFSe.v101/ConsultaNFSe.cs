namespace OpenDFe.NFSe;

/// <summary>
/// A consulta da NFS-e pela chave de acesso na Sefin Nacional (<c>GET /nfse/{chave}</c>; docs/NFSE.md 3.3): o XML da NFS-e,
/// se ela existe.
/// </summary>
public sealed class ConsultaNFSe : ParametroWS<RetornoConsultaNFSe>
{
    /// <summary>A consulta da chave.</summary>
    /// <param name="chave">A chave de acesso (50 dígitos).</param>
    /// <exception cref="ArgumentException">A chave é o valor padrão (vazia).</exception>
    public ConsultaNFSe(ChaveNFSe chave)
    {
        if (chave.Valor.Length == 0)
            throw new ArgumentException("A chave da NFS-e é vazia: use ChaveNFSe.Ler.", nameof(chave));
        Chave = chave;
    }

    /// <summary>A chave consultada.</summary>
    public ChaveNFSe Chave { get; }
}

/// <summary>
/// O retorno da consulta pela chave: a NFS-e (<see cref="Encontrada"/>) ou os erros (o 404 da NFS-e que não existe). O
/// <see cref="ResultadoWS{TRetorno}.CStat"/> é o status HTTP.
/// </summary>
public sealed class RetornoConsultaNFSe
{
    internal RetornoConsultaNFSe(ChaveNFSe? chave, string? xml, NotaFiscalServico? nota, IReadOnlyList<MensagemSefin> erros,
        IReadOnlyList<MensagemSefin> alertas)
    {
        Chave = chave;
        Xml = xml;
        Nota = nota;
        Erros = erros;
        Alertas = alertas;
    }

    /// <summary>A NFS-e existe: há a <see cref="Chave"/> e o <see cref="Xml"/>.</summary>
    public bool Encontrada => Chave is not null && Xml is not null;

    /// <summary>A chave da NFS-e encontrada; <see langword="null"/> se não foi.</summary>
    public ChaveNFSe? Chave { get; }

    /// <summary>O XML da NFS-e, descompactado, como veio; <see langword="null"/> se não foi encontrada.</summary>
    public string? Xml { get; }

    /// <summary>A NFS-e lida do <see cref="Xml"/>; <see langword="null"/> se não foi encontrada.</summary>
    public NotaFiscalServico? Nota { get; }

    /// <summary>Os erros da Sefin (o E0044 da NFS-e que não existe, a chave mal formada); vazio se foi encontrada.</summary>
    public IReadOnlyList<MensagemSefin> Erros { get; }

    /// <summary>Os alertas da Sefin.</summary>
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}

internal sealed class OperacaoConsultaNFSe : IOperacaoRest<ConsultaNFSe, RetornoConsultaNFSe>
{
    public PedidoRest Montar(ConsultaNFSe parametro, ContextoOperacao contexto) =>
        new(SefinNacional.Servico, MetodoRest.Get, "nfse/" + parametro.Chave.Valor) { Destino = Destino.SefinNacional };

    public LeituraWS<RetornoConsultaNFSe> Ler(ConsultaNFSe parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var lida = RespostaSefin.Ler(resposta);
        var (chave, xml, nota, problemas) = LeituraNotaSefin.Nota(lida, "consulta da NFS-e");
        var retorno = new RetornoConsultaNFSe(chave, xml, nota, lida.Erros, lida.Alertas);
        return new LeituraWS<RetornoConsultaNFSe>(retorno, resposta.Status, lida.Motivo("NFS-e encontrada")) { Problemas = problemas };
    }
}
