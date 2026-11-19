namespace OpenDFe.NFSe;

/// <summary>As NFS-e referenciadas (<c>gRefNFSe</c>).</summary>
public sealed class NFSeReferenciadas
{
    /// <summary>As chaves das NFS-e referenciadas (1 a 99).</summary>
    [Tag("refNFSe")]
    public IList<string> Chaves { get; } = new List<string>();
}
