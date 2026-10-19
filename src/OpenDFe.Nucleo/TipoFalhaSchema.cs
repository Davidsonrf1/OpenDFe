namespace OpenDFe;

/// <summary>O que falhou na validação pelo schema (<see cref="FalhaSchema.Tipo"/>).</summary>
public enum TipoFalhaSchema
{
    /// <summary>
    /// O valor de um elemento ou atributo não atende o tipo simples do schema (tamanho, padrão, valores aceitos,
    /// dígitos, faixa ou o tipo primitivo); as restrições violadas estão em <see cref="FalhaSchema.Restricoes"/>.
    /// </summary>
    Valor = 1,

    /// <summary>Falta um elemento obrigatório; os esperados estão em <see cref="FalhaSchema.Esperados"/>.</summary>
    ElementoAusente = 2,

    /// <summary>
    /// Um elemento que não era esperado naquela posição (fora de ordem, a mais, não declarado ou dentro de um
    /// elemento que não aceita filhos); os esperados estão em <see cref="FalhaSchema.Esperados"/>.
    /// </summary>
    ElementoInesperado = 3,

    /// <summary>Falta um atributo obrigatório; os ausentes estão em <see cref="FalhaSchema.Esperados"/>.</summary>
    AtributoAusente = 4,

    /// <summary>Um atributo que o elemento não declara; os declarados estão em <see cref="FalhaSchema.Esperados"/>.</summary>
    AtributoInesperado = 5,

    /// <summary>Texto num elemento que só aceita elementos filhos.</summary>
    TextoInesperado = 6,

    /// <summary>Outra falha, que a lib não classifica; o detalhe está em <see cref="FalhaSchema.MensagemOriginal"/>.</summary>
    Outra = 7,
}
