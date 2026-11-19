namespace OpenDFe.NFSe;

/// <summary>O processo de emissão da DPS (<c>procEmi</c>). O valor de cada membro é o do XML.</summary>
public enum ProcessoEmissao
{
    /// <summary>Aplicativo do contribuinte (web service) (1).</summary>
    Contribuinte = 1,

    /// <summary>Aplicativo do fisco (web) (2).</summary>
    FiscoWeb = 2,

    /// <summary>Aplicativo do fisco (app) (3).</summary>
    FiscoApp = 3,
}
