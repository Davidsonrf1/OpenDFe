namespace OpenDFe.NFSe;

/// <summary>
/// A consulta da DPS na Sefin Nacional (<c>GET</c> ou <c>HEAD /dps/{id}</c>; docs/NFSE.md 3.3): se a DPS já gerou a NFS-e e,
/// no GET, a chave dela. É o primeiro passo da <see cref="RecuperacaoEmissao"/>.
/// </summary>
public sealed class ConsultaDps : ParametroWS<RetornoConsultaDps>
{
    /// <summary>A consulta do identificador.</summary>
    /// <param name="id">O identificador da DPS (o <c>Id</c> do <c>infDPS</c>; <see cref="DpsEscrita.Id"/>).</param>
    /// <exception cref="ArgumentException">O identificador é o valor padrão (vazio).</exception>
    public ConsultaDps(IdentificadorDps id)
    {
        if (id.Valor.Length == 0)
            throw new ArgumentException("O identificador da DPS é vazio: use IdentificadorDps.Montar ou Ler.", nameof(id));
        Id = id;
    }

    /// <summary>O identificador consultado.</summary>
    public IdentificadorDps Id { get; }

    /// <summary>
    /// <see langword="true"/>: o HEAD, que só diz se a NFS-e foi gerada (vale qualquer certificado); <see langword="false"/>
    /// (o padrão): o GET, que devolve também a chave (só ao prestador, ao tomador ou ao intermediário da nota).
    /// </summary>
    public bool ApenasExistencia { get; init; }
}

/// <summary>
/// O retorno da consulta da DPS: se a NFS-e foi gerada (<see cref="Existe"/>) e, no GET, a chave. O
/// <see cref="ResultadoWS{TRetorno}.CStat"/> é o status HTTP (200 gerada, 404 não).
/// </summary>
public sealed class RetornoConsultaDps
{
    internal RetornoConsultaDps(bool existe, ChaveNFSe? chave, IReadOnlyList<MensagemSefin> erros)
    {
        Existe = existe;
        Chave = chave;
        Erros = erros;
    }

    /// <summary>A DPS gerou a NFS-e.</summary>
    public bool Existe { get; }

    /// <summary>A chave da NFS-e gerada; <see langword="null"/> no HEAD e se não foi gerada.</summary>
    public ChaveNFSe? Chave { get; }

    /// <summary>Os erros da Sefin (o 404, o identificador mal formado); vazio no HEAD.</summary>
    public IReadOnlyList<MensagemSefin> Erros { get; }
}

internal sealed class OperacaoConsultaDps : IOperacaoRest<ConsultaDps, RetornoConsultaDps>
{
    public PedidoRest Montar(ConsultaDps parametro, ContextoOperacao contexto) =>
        new(SefinNacional.Servico, parametro.ApenasExistencia ? MetodoRest.Head : MetodoRest.Get, "dps/" + parametro.Id.Valor)
        {
            Destino = Destino.SefinNacional,
        };

    public LeituraWS<RetornoConsultaDps> Ler(ConsultaDps parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var lida = RespostaSefin.Ler(resposta);
        ChaveNFSe? chave = null;
        if (lida.Sucesso && !parametro.ApenasExistencia)
        {
            if (!ChaveNFSe.TentarLer(lida.ChaveAcesso, out var lidaChave))
                throw new FormatException($"A resposta da consulta da DPS (HTTP {resposta.Status}) não traz uma chave de acesso válida.");
            chave = lidaChave;
        }
        var retorno = new RetornoConsultaDps(lida.Sucesso, chave, lida.Erros);
        return new LeituraWS<RetornoConsultaDps>(retorno, resposta.Status, lida.Motivo("NFS-e gerada pela DPS"));
    }
}
