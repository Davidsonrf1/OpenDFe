namespace OpenDFe.NFSe;

/// <summary>O local da prestação do serviço (<c>locPrest</c>): o município ou, fora do Brasil, o país.</summary>
public sealed class LocalPrestacao
{
    /// <summary>O código IBGE do município da prestação; um dos dois (município ou país).</summary>
    [Tag("cLocPrestacao")]
    public int? Municipio { get; set; }

    /// <summary>O código do país da prestação (ISO 3166-1, duas letras).</summary>
    [Tag("cPaisPrestacao")]
    public string? Pais { get; set; }
}
