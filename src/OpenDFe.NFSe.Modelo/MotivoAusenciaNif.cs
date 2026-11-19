namespace OpenDFe.NFSe;

/// <summary>O motivo de não informar o NIF (<c>cNaoNIF</c>). O valor de cada membro é o do XML.</summary>
public enum MotivoAusenciaNif
{
    /// <summary>Não informado na nota de origem (0).</summary>
    NaoInformado = 0,

    /// <summary>Dispensado do NIF (1).</summary>
    Dispensado = 1,

    /// <summary>Não exigência do NIF (2).</summary>
    NaoExigido = 2,
}
