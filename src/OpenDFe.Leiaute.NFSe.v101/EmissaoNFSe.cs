namespace OpenDFe.NFSe;

/// <summary>
/// A emissão da NFS-e na Sefin Nacional (<c>POST /nfse</c>; docs/NFSE.md 3.3): a DPS escrita, assinada, validada pelo schema e
/// pelas regras fora dele, compactada no JSON e enviada; a Sefin devolve a NFS-e gerada ou os erros. A substituição é esta
/// mesma emissão, com a <see cref="Dps.Substituicao"/> preenchida (docs/NFSE.md 1, item 2).
/// </summary>
/// <remarks>
/// Uma DPS por emissão. A Sefin só aceita a DPS do prestador (regra <c>NFSE-EMITENTE</c>). Se a resposta se perder
/// (<see cref="FalhaTransmissaoException"/> com <see cref="FalhaTransmissaoException.PodeTerSidoProcessado"/>), a
/// <see cref="RecuperacaoEmissao"/> descobre se a NFS-e foi gerada, pelo <see cref="IdentificadorDps"/> (o mesmo da
/// <see cref="DpsEscrita.Id"/>); reenviar a mesma DPS antes disso pode dar a rejeição E0014 (a DPS já tem NFS-e).
/// </remarks>
public sealed class EmissaoNFSe : ParametroWS<RetornoEmissaoNFSe>
{
    /// <summary>A emissão da DPS.</summary>
    /// <param name="dps">A DPS, sem assinatura. O <c>tpAmb</c> nulo vale o do núcleo.</param>
    /// <exception cref="ArgumentNullException">A DPS é nula.</exception>
    public EmissaoNFSe(Dps dps)
    {
        ArgumentNullException.ThrowIfNull(dps);
        Dps = dps;
    }

    /// <summary>A DPS.</summary>
    public Dps Dps { get; }
}

/// <summary>
/// O retorno da emissão: a NFS-e gerada (<see cref="Emitida"/>, com a chave, o XML e a nota lida) ou os erros da Sefin. O
/// <see cref="ResultadoWS{TRetorno}.CStat"/> é o status HTTP (201 gerada, 400 rejeitada).
/// </summary>
public sealed class RetornoEmissaoNFSe
{
    internal RetornoEmissaoNFSe(ChaveNFSe? chave, string? xml, NotaFiscalServico? nota, IReadOnlyList<MensagemSefin> erros,
        IReadOnlyList<MensagemSefin> alertas)
    {
        Chave = chave;
        Xml = xml;
        Nota = nota;
        Erros = erros;
        Alertas = alertas;
    }

    /// <summary>A Sefin gerou a NFS-e: há a <see cref="Chave"/> e o <see cref="Xml"/>.</summary>
    public bool Emitida => Chave is not null && Xml is not null;

    /// <summary>A chave de acesso da NFS-e gerada; <see langword="null"/> na rejeição.</summary>
    public ChaveNFSe? Chave { get; }

    /// <summary>
    /// O XML da NFS-e gerada (com a DPS dentro), descompactado, como veio: o que se guarda; <see langword="null"/> na
    /// rejeição.
    /// </summary>
    public string? Xml { get; }

    /// <summary>
    /// A NFS-e lida do <see cref="Xml"/> pelo leiaute (<see cref="LeiauteNFSe101.Ler"/>); os problemas da leitura vão nos
    /// problemas do resultado. <see langword="null"/> na rejeição.
    /// </summary>
    public NotaFiscalServico? Nota { get; }

    /// <summary>Os erros da Sefin (a rejeição), na ordem; vazio na NFS-e gerada.</summary>
    public IReadOnlyList<MensagemSefin> Erros { get; }

    /// <summary>Os alertas da Sefin, na ordem.</summary>
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}

/// <summary>O handler da emissão: a DPS no POST, a resposta lida (docs/NFSE.md, D4 e D5).</summary>
internal sealed class OperacaoEmissaoNFSe : IOperacaoRest<EmissaoNFSe, RetornoEmissaoNFSe>
{
    public PedidoRest Montar(EmissaoNFSe parametro, ContextoOperacao contexto)
    {
        var escrita = EscritorNFSe.Escrever(parametro.Dps, contexto.Configuracao.Ambiente);
        var id = escrita.Id?.Valor;
        return new PedidoRest(SefinNacional.Servico, MetodoRest.Post, "nfse")
        {
            Xml = escrita.Xml,
            Schema = SchemasNFSe101.Dps,
            CampoXml = SefinNacional.CampoDps,
            IdsAssinatura = id is null ? [] : [id],
            Destino = Destino.SefinNacional,
            Problemas = escrita.Problemas,
            Documentos = id is null ? [] : [new DocumentoPedido(id) { Modelo = parametro.Dps, Propriedade = CaminhoModeloNFSe.Propriedade }],
        };
    }

    public LeituraWS<RetornoEmissaoNFSe> Ler(EmissaoNFSe parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var lida = RespostaSefin.Ler(resposta);
        var (chave, xml, nota, problemas) = LeituraNotaSefin.Nota(lida, "emissão");
        var retorno = new RetornoEmissaoNFSe(chave, xml, nota, lida.Erros, lida.Alertas);
        return new LeituraWS<RetornoEmissaoNFSe>(retorno, resposta.Status, lida.Motivo("NFS-e gerada")) { Problemas = problemas };
    }
}

/// <summary>A NFS-e que a resposta da emissão e da consulta pela chave traz no sucesso: a chave, o XML e a nota lida.</summary>
internal static class LeituraNotaSefin
{
    /// <summary>
    /// No 2xx, a chave e o XML são obrigatórios (sem eles, <see cref="FormatException"/>); fora dele, não há nota.
    /// </summary>
    internal static (ChaveNFSe? Chave, string? Xml, NotaFiscalServico? Nota, IReadOnlyList<Problema> Problemas) Nota(RespostaSefin lida, string operacao)
    {
        if (!lida.Sucesso)
            return (null, null, null, []);
        if (lida.ChaveAcesso is null || lida.Xml is null)
            throw new FormatException($"A resposta da {operacao} (HTTP {lida.Status}) não traz a chave de acesso e a NFS-e.");
        if (!ChaveNFSe.TentarLer(lida.ChaveAcesso, out var chave))
            throw new FormatException($"A chave de acesso da resposta da {operacao} não é válida: {lida.ChaveAcesso}.");
        var nota = LeitorNFSe.Ler(lida.Xml);
        return (chave, lida.Xml, nota.Nota, nota.Problemas);
    }
}
