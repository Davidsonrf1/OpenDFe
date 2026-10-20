namespace OpenDFe.NFe;

/// <summary>O processo de emissão da nota. O valor de cada membro é o do XML (<c>procEmi</c>).</summary>
public enum ProcessoEmissao
{
    /// <summary>Emissão com aplicativo do contribuinte (0).</summary>
    AplicativoContribuinte = 0,

    /// <summary>Emissão avulsa pelo fisco (1).</summary>
    AvulsaFisco = 1,

    /// <summary>Emissão avulsa pelo contribuinte, com o certificado digital, pelo site do fisco (2).</summary>
    AvulsaContribuinteSite = 2,

    /// <summary>Emissão pelo contribuinte com aplicativo fornecido pelo fisco (3).</summary>
    AplicativoFisco = 3,

    /// <summary>Emissão por provedor de assinatura e autorização (PAA) (4).</summary>
    ProvedorAssinatura = 4,
}
