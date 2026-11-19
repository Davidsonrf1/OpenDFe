namespace OpenDFe.NFSe;

/// <summary>A resposta da análise fiscal que indeferiu o cancelamento (<c>cMotivo</c> do evento 105105). O valor de cada membro é o do XML.</summary>
public enum MotivoIndeferimento
{
    /// <summary>Cancelamento extemporâneo indeferido (1).</summary>
    Indeferido = 1,

    /// <summary>Indeferido sem análise de mérito (2).</summary>
    IndeferidoMerito = 2,
}
