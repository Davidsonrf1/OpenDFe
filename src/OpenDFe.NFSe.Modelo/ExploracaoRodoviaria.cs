namespace OpenDFe.NFSe;

/// <summary>O pedágio (<c>explRod</c>). Só na versão 1.00: a leitura preenche, a escrita da 1.01 não escreve (docs/NFSE.md 7, M10).</summary>
public sealed class ExploracaoRodoviaria
{
    /// <summary>A categoria do veículo.</summary>
    [Tag("categVeic")]
    public CategoriaVeiculo? Categoria { get; set; }

    /// <summary>O número de eixos.</summary>
    [Tag("nEixos")]
    public int? Eixos { get; set; }

    /// <summary>O tipo de rodagem.</summary>
    [Tag("rodagem")]
    public TipoRodagem? Rodagem { get; set; }

    /// <summary>O sentido.</summary>
    [Tag("sentido")]
    public int? Sentido { get; set; }

    /// <summary>A placa do veículo.</summary>
    [Tag("placa")]
    public string? Placa { get; set; }

    /// <summary>O código de acesso gerado pela concessionária.</summary>
    [Tag("codAcessoPed")]
    public string? CodigoAcesso { get; set; }

    /// <summary>O código do contrato.</summary>
    [Tag("codContrato")]
    public string? CodigoContrato { get; set; }
}
