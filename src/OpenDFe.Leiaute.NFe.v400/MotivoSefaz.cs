namespace OpenDFe.NFe;

/// <summary>
/// O complemento que a SEFAZ põe no fim do <c>xMotivo</c> de algumas rejeições (o 204, o 539, o 562): a chave da nota
/// registrada (<c>[chNFe:...]</c>) e o recibo do lote (<c>[nRec:...]</c>), com ou sem espaço depois dos dois-pontos. O
/// MOC o faz opcional: ausente ou fora do formato, nulo, sem aviso. Sem estado.
/// </summary>
internal static class MotivoSefaz
{
    /// <summary>A chave de <c>[chNFe:...]</c>; nula sem ele, ou se não é uma chave válida.</summary>
    internal static ChaveAcesso? Chave(string? motivo) =>
        Marcador(motivo, "chNFe", ChaveAcesso.Tamanho) is { } texto && ChaveAcesso.TentarLer(texto, out var chave) ? chave : null;

    /// <summary>O recibo de <c>[nRec:...]</c> (15 dígitos); nulo sem ele.</summary>
    internal static string? Recibo(string? motivo) => Marcador(motivo, "nRec", 15);

    // Os dígitos de [nome:...], com os espaços em volta, quando são exatamente o tamanho.
    private static string? Marcador(string? motivo, string nome, int tamanho)
    {
        if (motivo is null)
            return null;
        var inicio = motivo.IndexOf("[" + nome + ":", StringComparison.Ordinal);
        if (inicio < 0)
            return null;
        inicio += nome.Length + 2;
        var fim = motivo.IndexOf(']', inicio);
        if (fim < 0)
            return null;
        var texto = motivo[inicio..fim].Trim();
        return texto.Length == tamanho && texto.All(char.IsAsciiDigit) ? texto : null;
    }
}
