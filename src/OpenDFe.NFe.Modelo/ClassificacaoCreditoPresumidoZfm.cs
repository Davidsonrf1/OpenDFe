namespace OpenDFe.NFe;

/// <summary>
/// A classificação para o crédito presumido do IBS na Zona Franca de Manaus (art. 450, § 1º, da LC 214/2025). O valor
/// de cada membro é o do XML (<c>tpCredPresIBSZFM</c>).
/// </summary>
public enum ClassificacaoCreditoPresumidoZfm
{
    /// <summary>Sem crédito presumido (0).</summary>
    Nenhum = 0,

    /// <summary>Bens de consumo final (1).</summary>
    BensConsumoFinal = 1,

    /// <summary>Bens de capital (2).</summary>
    BensCapital = 2,

    /// <summary>Bens intermediários (3).</summary>
    BensIntermediarios = 3,

    /// <summary>Bens de informática e outros definidos em legislação (4).</summary>
    BensInformatica = 4,
}
