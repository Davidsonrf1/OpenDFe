namespace OpenDFe.NFe;

/// <summary>
/// A modalidade de determinação da base de cálculo do ICMS ST. O valor de cada membro é o do XML (<c>modBCST</c>).
/// </summary>
public enum ModalidadeBaseCalculoSt
{
    /// <summary>Preço tabelado ou máximo sugerido (0).</summary>
    PrecoTabelado = 0,

    /// <summary>Lista negativa (valor) (1).</summary>
    ListaNegativa = 1,

    /// <summary>Lista positiva (valor) (2).</summary>
    ListaPositiva = 2,

    /// <summary>Lista neutra (valor) (3).</summary>
    ListaNeutra = 3,

    /// <summary>Margem de valor agregado (%) (4).</summary>
    MargemValorAgregado = 4,

    /// <summary>Pauta (valor) (5).</summary>
    Pauta = 5,

    /// <summary>Valor da operação (6).</summary>
    ValorOperacao = 6,
}
