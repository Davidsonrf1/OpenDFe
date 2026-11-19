namespace OpenDFe.NFSe;

/// <summary>A Declaração de Prestação de Serviço: o <c>DPS</c> e o <c>infDPS</c> juntos (docs/NFSE.md 7, M2). O que quem emite preenche, assina e envia, e o que a leitura devolve, também de dentro da NFS-e.</summary>
public sealed class Dps
{
    /// <summary>O ambiente; nulo na escrita = o da configuração do núcleo (M4).</summary>
    [Tag("tpAmb")]
    public Ambiente? Ambiente { get; set; }

    /// <summary>A data e a hora da emissão da DPS.</summary>
    [Tag("dhEmi")]
    public DateTimeOffset? Emissao { get; set; }

    /// <summary>A versão do aplicativo que gerou a DPS; nulo na escrita = "OpenDFe " e a versão do pacote (M4).</summary>
    [Tag("verAplic")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>A série da DPS (até 5 dígitos; 00001 a 49999 para o aplicativo próprio).</summary>
    [Tag("serie")]
    public string? Serie { get; set; }

    /// <summary>O número da DPS (1 a 999.999.999.999.999).</summary>
    [Tag("nDPS")]
    public long? Numero { get; set; }

    /// <summary>A data em que se iniciou a prestação do serviço (a competência).</summary>
    [Tag("dCompet")]
    public DateOnly? Competencia { get; set; }

    /// <summary>Quem emite a DPS; a Sefin Nacional só aceita o prestador (regra NFSE-EMITENTE).</summary>
    [Tag("tpEmit")]
    public EmitenteDps? Emitente { get; set; }

    /// <summary>O motivo da emissão pelo tomador ou pelo intermediário; opcional.</summary>
    [Tag("cMotivoEmisTI")]
    public MotivoEmissaoTerceiro? MotivoEmissaoTerceiro { get; set; }

    /// <summary>A chave da NFS-e rejeitada pelo tomador ou pelo intermediário; opcional.</summary>
    [Tag("chNFSeRej")]
    public string? ChaveRejeitada { get; set; }

    /// <summary>O código IBGE do município emissor.</summary>
    [Tag("cLocEmi")]
    public int? MunicipioEmissor { get; set; }

    /// <summary>A NFS-e que esta DPS substitui; preenchida, a emissão substitui a NFS-e da chave; opcional.</summary>
    [Tag("subst")]
    public Substituicao? Substituicao { get; set; }

    /// <summary>O prestador do serviço.</summary>
    [Tag("prest")]
    public Prestador Prestador { get; set; } = new();

    /// <summary>O tomador do serviço; opcional.</summary>
    [Tag("toma")]
    public Pessoa? Tomador { get; set; }

    /// <summary>O intermediário do serviço; opcional.</summary>
    [Tag("interm")]
    public Pessoa? Intermediario { get; set; }

    /// <summary>O serviço prestado.</summary>
    [Tag("serv")]
    public Servico Servico { get; set; } = new();

    /// <summary>Os valores do serviço e os tributos.</summary>
    [Tag("valores")]
    public ValoresDps Valores { get; set; } = new();

    /// <summary>O IBS e a CBS declarados por quem emite; opcional.</summary>
    [Tag("IBSCBS")]
    public IbsCbsDps? IbsCbs { get; set; }
}
