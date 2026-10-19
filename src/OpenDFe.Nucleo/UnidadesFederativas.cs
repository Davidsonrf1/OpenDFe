namespace OpenDFe;

/// <summary>Conversões da <see cref="UF"/> de e para a sigla e o código IBGE.</summary>
public static class UnidadesFederativas
{
    /// <summary>A sigla da UF, em maiúsculas (<c>"MG"</c>), como vai no XML.</summary>
    /// <param name="uf">A UF.</param>
    /// <returns>A sigla de duas letras.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é uma UF.</exception>
    public static string Sigla(this UF uf)
    {
        Conferir(uf);
        return uf.ToString();
    }

    /// <summary>O código IBGE da UF (<c>31</c> para MG), como vai no XML.</summary>
    /// <param name="uf">A UF.</param>
    /// <returns>O código de dois dígitos.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é uma UF.</exception>
    public static int Codigo(this UF uf)
    {
        Conferir(uf);
        return (int)uf;
    }

    /// <summary>Obtém a UF pela sigla, em maiúsculas (<c>"MG"</c>).</summary>
    /// <param name="sigla">A sigla de duas letras maiúsculas.</param>
    /// <param name="uf">A UF, quando a sigla é de uma.</param>
    /// <returns>Se a sigla é de uma UF.</returns>
    public static bool TentarPelaSigla(string? sigla, out UF uf)
    {
        uf = default;
        if (sigla is not { Length: 2 } || !char.IsAsciiLetterUpper(sigla[0]) || !char.IsAsciiLetterUpper(sigla[1]))
            return false;
        if (Enum.TryParse(sigla, ignoreCase: false, out uf) && Enum.IsDefined(uf))
            return true;
        uf = default;
        return false;
    }

    /// <summary>Obtém a UF pelo código IBGE (<c>31</c> para MG).</summary>
    /// <param name="codigo">O código IBGE da UF.</param>
    /// <param name="uf">A UF, quando o código é de uma.</param>
    /// <returns>Se o código é de uma UF.</returns>
    public static bool TentarPeloCodigo(int codigo, out UF uf)
    {
        uf = (UF)codigo;
        if (Enum.IsDefined(uf))
            return true;
        uf = default;
        return false;
    }

    private static void Conferir(UF uf)
    {
        if (!Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(uf), uf, "O valor não é uma UF.");
    }
}
