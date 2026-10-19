namespace OpenDFe;

/// <summary>A restrição de um tipo simples do schema (a faceta do XSD) que um valor violou.</summary>
public enum FacetaSchema
{
    /// <summary>
    /// O tipo primitivo do XSD (<c>xs:decimal</c>, <c>xs:date</c>...): o valor nem chega a ser um valor dele.
    /// </summary>
    TipoPrimitivo = 1,

    /// <summary>O tamanho exato (<c>length</c>).</summary>
    Tamanho = 2,

    /// <summary>O tamanho mínimo (<c>minLength</c>).</summary>
    TamanhoMinimo = 3,

    /// <summary>O tamanho máximo (<c>maxLength</c>).</summary>
    TamanhoMaximo = 4,

    /// <summary>O padrão, uma expressão regular do XSD (<c>pattern</c>).</summary>
    Padrao = 5,

    /// <summary>Os valores aceitos (<c>enumeration</c>).</summary>
    Enumeracao = 6,

    /// <summary>O total de dígitos (<c>totalDigits</c>).</summary>
    TotalDigitos = 7,

    /// <summary>As casas decimais (<c>fractionDigits</c>).</summary>
    CasasDecimais = 8,

    /// <summary>O mínimo, inclusive (<c>minInclusive</c>).</summary>
    MinimoInclusivo = 9,

    /// <summary>O máximo, inclusive (<c>maxInclusive</c>).</summary>
    MaximoInclusivo = 10,

    /// <summary>O mínimo, exclusive (<c>minExclusive</c>).</summary>
    MinimoExclusivo = 11,

    /// <summary>O máximo, exclusive (<c>maxExclusive</c>).</summary>
    MaximoExclusivo = 12,
}
