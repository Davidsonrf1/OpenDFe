namespace OpenDFe.NFe;

/// <summary>
/// O ICMS do Simples Nacional sem permissão de crédito, isento, imune ou não tributado (<c>ICMSSN102</c>, CSOSN 102,
/// 103, 300 ou 400).
/// </summary>
[Tag("ICMSSN102", "N10d")]
public sealed class IcmsSn102 : Icms
{
    /// <summary>O CSOSN: <c>102</c>, <c>103</c>, <c>300</c> ou <c>400</c>.</summary>
    [Tag("CSOSN", "N12a")]
    public CsosnIcms? Csosn { get; set; }
}
