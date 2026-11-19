namespace OpenDFe.NFSe;

/// <summary>O vínculo da operação à movimentação temporária de bens (<c>movTempBens</c>). O valor de cada membro é o do XML.</summary>
public enum MovimentacaoTemporariaBens
{
    /// <summary>Desconhecido (não informado na nota de origem) (0).</summary>
    Desconhecido = 0,

    /// <summary>Não (1).</summary>
    Nao = 1,

    /// <summary>Vinculada à declaração de importação (2).</summary>
    VinculadaImportacao = 2,

    /// <summary>Vinculada à declaração de exportação (3).</summary>
    VinculadaExportacao = 3,
}
