namespace OpenDFe;

/// <summary>
/// Qual autorizador atende uma UF num tipo de emissão (MG, normal → MG; AC, normal → SVRS; MG, SVC-AN → SVC-AN),
/// como o leiaute declara (<see cref="DFe.Atendimentos"/>). Criado pelo núcleo.
/// </summary>
public sealed class Atendimento
{
    internal Atendimento(UF uf, TipoEmissao emissao, Destino autorizador)
    {
        UF = uf;
        Emissao = emissao;
        Autorizador = autorizador;
    }

    /// <summary>A UF.</summary>
    public UF UF { get; }

    /// <summary>O tipo de emissão.</summary>
    public TipoEmissao Emissao { get; }

    /// <summary>O autorizador: a própria UF, ou um virtual (SVRS, SVAN, SVC-AN, SVC-RS).</summary>
    public Destino Autorizador { get; }
}
