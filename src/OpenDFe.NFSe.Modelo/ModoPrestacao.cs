namespace OpenDFe.NFSe;

/// <summary>O modo de prestação do serviço ao exterior (<c>mdPrestacao</c>). O valor de cada membro é o do XML.</summary>
public enum ModoPrestacao
{
    /// <summary>Desconhecido (não informado na nota de origem) (0).</summary>
    Desconhecido = 0,

    /// <summary>Transfronteiriço (1).</summary>
    Transfronteirico = 1,

    /// <summary>Consumo no Brasil (2).</summary>
    ConsumoBrasil = 2,

    /// <summary>Presença comercial no exterior (3).</summary>
    PresencaComercialExterior = 3,

    /// <summary>Movimento temporário de pessoas físicas (4).</summary>
    MovimentoTemporarioPessoas = 4,
}
