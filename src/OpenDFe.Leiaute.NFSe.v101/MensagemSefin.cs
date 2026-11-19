namespace OpenDFe.NFSe;

/// <summary>
/// Um erro ou um alerta da resposta da Sefin Nacional (os itens de <c>erros</c> e <c>alertas</c> do JSON): o código do Anexo I
/// (<c>"E0014"</c>), a descrição e o complemento.
/// </summary>
public sealed class MensagemSefin
{
    internal MensagemSefin(string codigo, string descricao, string? complemento)
    {
        Codigo = codigo;
        Descricao = descricao;
        Complemento = complemento;
    }

    /// <summary>O código (<c>"E0014"</c>, <c>"E1235"</c>); vazio se a resposta não o traz.</summary>
    public string Codigo { get; }

    /// <summary>A descrição; vazia se a resposta não a traz.</summary>
    public string Descricao { get; }

    /// <summary>O complemento (o detalhe do schema, a chave da NFS-e que já existe); <see langword="null"/> se não veio.</summary>
    public string? Complemento { get; }

    /// <summary>O código, a descrição e o complemento numa linha.</summary>
    /// <returns>O texto (<c>"E0014: Conjunto de Série... (A NFS-e gerada é a 3106...)"</c>).</returns>
    public override string ToString() =>
        (Codigo.Length == 0 ? Descricao : $"{Codigo}: {Descricao}") + (Complemento is null ? "" : $" ({Complemento})");
}
