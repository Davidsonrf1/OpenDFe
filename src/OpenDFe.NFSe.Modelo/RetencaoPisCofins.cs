namespace OpenDFe.NFSe;

/// <summary>A retenção do PIS, da COFINS e da CSLL (<c>tpRetPisCofins</c>, NT 007). O valor de cada membro é o do XML.</summary>
public enum RetencaoPisCofins
{
    /// <summary>PIS, COFINS e CSLL não retidos (0).</summary>
    NenhumRetido = 0,

    /// <summary>PIS e COFINS retidos (1).</summary>
    PisCofinsRetidos = 1,

    /// <summary>PIS e COFINS não retidos (2).</summary>
    PisCofinsNaoRetidos = 2,

    /// <summary>PIS, COFINS e CSLL retidos (3).</summary>
    TodosRetidos = 3,

    /// <summary>PIS e COFINS retidos, CSLL não (4).</summary>
    PisCofinsRetidosCsllNao = 4,

    /// <summary>PIS retido, COFINS e CSLL não (5).</summary>
    PisRetido = 5,

    /// <summary>COFINS retida, PIS e CSLL não (6).</summary>
    CofinsRetida = 6,

    /// <summary>COFINS e CSLL retidas, PIS não (7).</summary>
    CofinsCsllRetidas = 7,

    /// <summary>CSLL retida, PIS e COFINS não (8).</summary>
    CsllRetida = 8,

    /// <summary>PIS e CSLL retidos, COFINS não (9).</summary>
    PisCsllRetidos = 9,
}
