namespace OpenDFe.NFSe;

/// <summary>O ambiente que gerou a NFS-e (<c>ambGer</c>). O valor de cada membro é o do XML.</summary>
public enum AmbienteGerador
{
    /// <summary>O sistema próprio do município (1).</summary>
    Prefeitura = 1,

    /// <summary>O Sistema Nacional da NFS-e (2).</summary>
    SistemaNacional = 2,
}
