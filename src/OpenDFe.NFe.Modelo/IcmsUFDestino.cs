namespace OpenDFe.NFe;

/// <summary>
/// O ICMS de partilha para a UF do destinatário, na operação interestadual com consumidor final não contribuinte
/// (<c>ICMSUFDest</c>, EC 87/2015).
/// </summary>
public sealed class IcmsUFDestino
{
    /// <summary>A base de cálculo na UF de destino.</summary>
    [Tag("vBCUFDest", "NA03")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A base de cálculo do FCP na UF de destino; opcional.</summary>
    [Tag("vBCFCPUFDest", "NA04")]
    public decimal? BaseCalculoFcp { get; set; }

    /// <summary>O percentual do FCP na UF de destino; opcional.</summary>
    [Tag("pFCPUFDest", "NA05")]
    public decimal? PercentualFcp { get; set; }

    /// <summary>A alíquota interna da UF de destino.</summary>
    [Tag("pICMSUFDest", "NA07")]
    public decimal? AliquotaInterna { get; set; }

    /// <summary>A alíquota interestadual (4, 7 ou 12).</summary>
    [Tag("pICMSInter", "NA09")]
    public decimal? AliquotaInterestadual { get; set; }

    /// <summary>O percentual provisório de partilha do ICMS interestadual.</summary>
    [Tag("pICMSInterPart", "NA11")]
    public decimal? PercentualPartilha { get; set; }

    /// <summary>O valor do FCP da UF de destino; opcional.</summary>
    [Tag("vFCPUFDest", "NA13")]
    public decimal? ValorFcp { get; set; }

    /// <summary>O valor do ICMS de partilha para a UF de destino.</summary>
    [Tag("vICMSUFDest", "NA15")]
    public decimal? ValorDestino { get; set; }

    /// <summary>O valor do ICMS de partilha para a UF do remetente.</summary>
    [Tag("vICMSUFRemet", "NA17")]
    public decimal? ValorRemetente { get; set; }
}
