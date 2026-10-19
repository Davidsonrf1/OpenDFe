namespace OpenDFe;

/// <summary>Conversões do <see cref="Destino"/> de e para a <see cref="UF"/> e a sigla.</summary>
public static class Destinos
{
    /// <summary>O destino da UF (<see cref="Destino.MG"/> para <see cref="UF.MG"/>).</summary>
    /// <param name="uf">A UF.</param>
    /// <returns>O destino com o mesmo código IBGE.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é uma UF.</exception>
    public static Destino ComoDestino(this UF uf)
    {
        if (!Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(uf), uf, "O valor não é uma UF.");
        return (Destino)(int)uf;
    }

    /// <summary>A UF do destino, quando o destino é uma UF.</summary>
    /// <param name="destino">O destino.</param>
    /// <param name="uf">A UF, quando o destino é uma.</param>
    /// <returns>Se o destino é uma UF (não o AN nem um autorizador virtual).</returns>
    public static bool TentarUF(this Destino destino, out UF uf)
    {
        uf = (UF)(int)destino;
        if (Enum.IsDefined(destino) && Enum.IsDefined(uf))
            return true;
        uf = default;
        return false;
    }

    /// <summary>
    /// A sigla do destino: a da UF (<c>"MG"</c>), <c>"AN"</c>, <c>"SVRS"</c>, <c>"SVAN"</c>, <c>"SVC-AN"</c>
    /// ou <c>"SVC-RS"</c>.
    /// </summary>
    /// <param name="destino">O destino.</param>
    /// <returns>A sigla, em maiúsculas.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é um destino.</exception>
    public static string Sigla(this Destino destino) => destino switch
    {
        Destino.SvcAN => "SVC-AN",
        Destino.SvcRS => "SVC-RS",
        _ when Enum.IsDefined(destino) => destino.ToString(),
        _ => throw new ArgumentOutOfRangeException(nameof(destino), destino, "O valor não é um destino."),
    };

    /// <summary>Obtém o destino pela sigla, em maiúsculas (<c>"MG"</c>, <c>"AN"</c>, <c>"SVC-AN"</c>).</summary>
    /// <param name="sigla">A sigla, como <see cref="Sigla"/> a devolve.</param>
    /// <param name="destino">O destino, quando a sigla é de um.</param>
    /// <returns>Se a sigla é de um destino.</returns>
    public static bool TentarLerSigla(string? sigla, out Destino destino)
    {
        destino = default;
        switch (sigla)
        {
            case null:
                return false;
            case "SVC-AN":
                destino = Destino.SvcAN;
                return true;
            case "SVC-RS":
                destino = Destino.SvcRS;
                return true;
            case "AN" or "SVRS" or "SVAN":
                destino = Enum.Parse<Destino>(sigla);
                return true;
        }
        if (UnidadesFederativas.TentarLerSigla(sigla, out var uf))
        {
            destino = uf.ComoDestino();
            return true;
        }
        return false;
    }
}
