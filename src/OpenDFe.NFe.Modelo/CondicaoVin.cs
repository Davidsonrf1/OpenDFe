namespace OpenDFe.NFe;

/// <summary>Se o VIN (chassi) do veículo é remarcado (<c>VIN</c>).</summary>
/// <remarks>O valor do XML não é número: o leiaute traduz o membro (1 e 2) para <c>R</c> e <c>N</c>.</remarks>
public enum CondicaoVin
{
    /// <summary>Remarcado; <c>R</c> no XML.</summary>
    Remarcado = 1,

    /// <summary>Normal; <c>N</c> no XML.</summary>
    Normal = 2,
}
