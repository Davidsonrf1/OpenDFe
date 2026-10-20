namespace OpenDFe.NFe;

/// <summary>O veículo novo (<c>veicProd</c>).</summary>
/// <remarks>
/// Os códigos das tabelas do RENAVAM e do DENATRAN e os campos que o XSD tipa como texto (potência, cilindrada, pesos)
/// ficam como texto, como no XML.
/// </remarks>
public sealed class VeiculoNovo
{
    /// <summary>O tipo da operação.</summary>
    [Tag("tpOp", "J02")]
    public TipoOperacaoVeiculo? TipoOperacao { get; set; }

    /// <summary>O chassi (VIN).</summary>
    [Tag("chassi", "J03")]
    public string? Chassi { get; set; }

    /// <summary>O código da cor (de cada montadora).</summary>
    [Tag("cCor", "J04")]
    public string? CodigoCor { get; set; }

    /// <summary>A descrição da cor.</summary>
    [Tag("xCor", "J05")]
    public string? Cor { get; set; }

    /// <summary>A potência máxima do motor, em cavalos-vapor, como no XML.</summary>
    [Tag("pot", "J06")]
    public string? Potencia { get; set; }

    /// <summary>A cilindrada, em centímetros cúbicos, como no XML.</summary>
    [Tag("cilin", "J07")]
    public string? Cilindrada { get; set; }

    /// <summary>O peso líquido, como no XML.</summary>
    [Tag("pesoL", "J08")]
    public string? PesoLiquido { get; set; }

    /// <summary>O peso bruto, como no XML.</summary>
    [Tag("pesoB", "J09")]
    public string? PesoBruto { get; set; }

    /// <summary>O serial (série).</summary>
    [Tag("nSerie", "J10")]
    public string? Serie { get; set; }

    /// <summary>O tipo de combustível (tabela do RENAVAM).</summary>
    [Tag("tpComb", "J11")]
    public string? TipoCombustivel { get; set; }

    /// <summary>O número do motor.</summary>
    [Tag("nMotor", "J12")]
    public string? Motor { get; set; }

    /// <summary>A capacidade máxima de tração, em toneladas, como no XML.</summary>
    [Tag("CMT", "J13")]
    public string? CapacidadeTracao { get; set; }

    /// <summary>A distância entre eixos, como no XML.</summary>
    [Tag("dist", "J14")]
    public string? DistanciaEixos { get; set; }

    /// <summary>O ano do modelo.</summary>
    [Tag("anoMod", "J16")]
    public int? AnoModelo { get; set; }

    /// <summary>O ano de fabricação.</summary>
    [Tag("anoFab", "J17")]
    public int? AnoFabricacao { get; set; }

    /// <summary>O tipo de pintura.</summary>
    [Tag("tpPint", "J18")]
    public string? TipoPintura { get; set; }

    /// <summary>O tipo de veículo (tabela do RENAVAM).</summary>
    [Tag("tpVeic", "J19")]
    public string? TipoVeiculo { get; set; }

    /// <summary>A espécie de veículo (tabela do RENAVAM).</summary>
    [Tag("espVeic", "J20")]
    public int? EspecieVeiculo { get; set; }

    /// <summary>Se o VIN (chassi) é remarcado.</summary>
    [Tag("VIN", "J21")]
    public CondicaoVin? Vin { get; set; }

    /// <summary>A condição do veículo.</summary>
    [Tag("condVeic", "J22")]
    public CondicaoVeiculo? Condicao { get; set; }

    /// <summary>O código de marca e modelo (tabela do RENAVAM).</summary>
    [Tag("cMod", "J23")]
    public string? MarcaModelo { get; set; }

    /// <summary>O código da cor (tabela do DENATRAN).</summary>
    [Tag("cCorDENATRAN", "J24")]
    public string? CorDenatran { get; set; }

    /// <summary>A lotação: a quantidade máxima de passageiros sentados, com o motorista.</summary>
    [Tag("lota", "J25")]
    public int? Lotacao { get; set; }

    /// <summary>A restrição.</summary>
    [Tag("tpRest", "J26")]
    public RestricaoVeiculo? Restricao { get; set; }
}
