namespace OpenDFe.NFSe;

/// <summary>O tipo do valor de reembolso, repasse ou ressarcimento (<c>tpReeRepRes</c>). O XML leva dois dígitos. O valor de cada membro é o do XML.</summary>
public enum TipoReembolso
{
    /// <summary>Repasse de remuneração por intermediação de imóveis a outros corretores (1).</summary>
    RepasseCorretores = 1,

    /// <summary>Repasse a fornecedor por agência de turismo (2).</summary>
    RepasseTurismo = 2,

    /// <summary>Reembolso a agência de propaganda por produção externa (3).</summary>
    ReembolsoProducaoExterna = 3,

    /// <summary>Reembolso a agência de propaganda por mídia (4).</summary>
    ReembolsoMidia = 4,

    /// <summary>Outros reembolsos ou ressarcimentos (99).</summary>
    Outros = 99,
}
