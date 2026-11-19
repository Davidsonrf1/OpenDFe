namespace OpenDFe.NFSe;

/// <summary>A NFS-e substituída (<c>subst</c>): a emissão com este grupo cancela a NFS-e da chave por substituição (o evento 105102 é gerado pela Sefin).</summary>
public sealed class Substituicao
{
    /// <summary>A chave de acesso da NFS-e substituída (50 dígitos).</summary>
    [Tag("chSubstda")]
    public string? Chave { get; set; }

    /// <summary>O motivo da substituição.</summary>
    [Tag("cMotivo")]
    public MotivoSubstituicao? Motivo { get; set; }

    /// <summary>A descrição do motivo; opcional.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
