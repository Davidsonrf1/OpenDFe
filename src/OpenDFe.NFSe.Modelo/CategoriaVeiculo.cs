namespace OpenDFe.NFSe;

/// <summary>A categoria do veículo no pedágio (<c>categVeic</c>, só na versão 1.00). O XML leva dois dígitos. O valor de cada membro é o do XML.</summary>
public enum CategoriaVeiculo
{
    /// <summary>Não informada na nota de origem (0).</summary>
    NaoInformada = 0,

    /// <summary>Automóvel, caminhonete e furgão (1).</summary>
    Automovel = 1,

    /// <summary>Caminhão leve, ônibus, caminhão trator e furgão (2).</summary>
    CaminhaoLeve = 2,

    /// <summary>Automóvel e caminhonete com semirreboque (3).</summary>
    AutomovelSemirreboque = 3,

    /// <summary>Caminhão, caminhão-trator com semirreboque e ônibus (4).</summary>
    Caminhao = 4,

    /// <summary>Automóvel e caminhonete com reboque (5).</summary>
    AutomovelReboque = 5,

    /// <summary>Caminhão com reboque (6).</summary>
    CaminhaoReboque = 6,

    /// <summary>Caminhão trator com semirreboque (7).</summary>
    CaminhaoTratorSemirreboque = 7,

    /// <summary>Motocicletas, motonetas e bicicletas motorizadas (8).</summary>
    Motocicleta = 8,

    /// <summary>Veículo especial (9).</summary>
    Especial = 9,

    /// <summary>Veículo isento (10).</summary>
    Isento = 10,
}
