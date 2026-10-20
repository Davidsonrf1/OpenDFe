namespace OpenDFe.NFe;

/// <summary>
/// A modalidade de determinação da base de cálculo do ICMS. O valor de cada membro é o do XML (<c>modBC</c>).
/// </summary>
public enum ModalidadeBaseCalculo
{
    /// <summary>Margem de valor agregado (%) (0).</summary>
    MargemValorAgregado = 0,

    /// <summary>Pauta (valor) (1).</summary>
    Pauta = 1,

    /// <summary>Preço tabelado máximo (valor) (2).</summary>
    PrecoTabeladoMaximo = 2,

    /// <summary>Valor da operação (3).</summary>
    ValorOperacao = 3,
}
