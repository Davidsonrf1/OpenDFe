namespace OpenDFe.NFe;

/// <summary>
/// Se o ICMS desonerado deduz do valor do item e do total da nota. O valor de cada membro é o do XML
/// (<c>indDeduzDeson</c>).
/// </summary>
public enum IndicadorDeducaoDesoneracao
{
    /// <summary>O ICMS desonerado não deduz do valor do item (<c>vProd</c>) nem do total da nota.</summary>
    NaoDeduz = 0,

    /// <summary>O ICMS desonerado deduz do valor do item (<c>vProd</c>) e do total da nota.</summary>
    Deduz = 1,
}
