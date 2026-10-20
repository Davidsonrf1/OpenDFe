namespace OpenDFe.NFe;

/// <summary>A modalidade do frete. O valor de cada membro é o do XML (<c>modFrete</c>).</summary>
public enum ModalidadeFrete
{
    /// <summary>Contratação do frete por conta do remetente (CIF) (0).</summary>
    Remetente = 0,

    /// <summary>Contratação do frete por conta do destinatário (FOB) (1).</summary>
    Destinatario = 1,

    /// <summary>Contratação do frete por conta de terceiros (2).</summary>
    Terceiros = 2,

    /// <summary>Transporte próprio por conta do remetente (3).</summary>
    ProprioRemetente = 3,

    /// <summary>Transporte próprio por conta do destinatário (4).</summary>
    ProprioDestinatario = 4,

    /// <summary>Sem ocorrência de transporte (9).</summary>
    NenhumTransporte = 9,
}
