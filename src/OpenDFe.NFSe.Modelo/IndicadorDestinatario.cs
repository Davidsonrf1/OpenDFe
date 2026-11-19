namespace OpenDFe.NFSe;

/// <summary>Se o destinatário do serviço é o próprio tomador (<c>indDest</c>). O valor de cada membro é o do XML.</summary>
public enum IndicadorDestinatario
{
    /// <summary>O destinatário é o tomador (0).</summary>
    Tomador = 0,

    /// <summary>O destinatário não é o tomador (1).</summary>
    Outro = 1,
}
