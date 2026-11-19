namespace OpenDFe.NFSe;

/// <summary>O vínculo entre as partes no negócio (<c>vincPrest</c>). O valor de cada membro é o do XML.</summary>
public enum VinculoPartes
{
    /// <summary>Sem vínculo (0).</summary>
    Nenhum = 0,

    /// <summary>Controlada (1).</summary>
    Controlada = 1,

    /// <summary>Controladora (2).</summary>
    Controladora = 2,

    /// <summary>Coligada (3).</summary>
    Coligada = 3,

    /// <summary>Matriz (4).</summary>
    Matriz = 4,

    /// <summary>Filial ou sucursal (5).</summary>
    Filial = 5,

    /// <summary>Outro vínculo (6).</summary>
    Outro = 6,

    /// <summary>Desconhecido (não informado na nota de origem) (9).</summary>
    Desconhecido = 9,
}
