namespace OpenDFe.NFe;

/// <summary>
/// O CSOSN, o código de situação da operação no Simples Nacional. O valor de cada membro é o do XML (<c>CSOSN</c>).
/// </summary>
/// <remarks>
/// Uma enumeração para todos os grupos do Simples Nacional: cada grupo aceita só uma parte dos códigos, e o schema
/// aponta o código que o grupo não aceita.
/// </remarks>
public enum CsosnIcms
{
    /// <summary>Tributada com permissão de crédito (101).</summary>
    PermiteCredito = 101,

    /// <summary>Tributada sem permissão de crédito (102).</summary>
    NaoPermiteCredito = 102,

    /// <summary>Isenção do ICMS para a faixa de receita bruta (103).</summary>
    IsencaoFaixaReceita = 103,

    /// <summary>Tributada com permissão de crédito e com cobrança do ICMS por substituição tributária (201).</summary>
    PermiteCreditoSubstituicao = 201,

    /// <summary>Tributada sem permissão de crédito e com cobrança do ICMS por substituição tributária (202).</summary>
    NaoPermiteCreditoSubstituicao = 202,

    /// <summary>
    /// Isenção do ICMS para a faixa de receita bruta e com cobrança do ICMS por substituição tributária (203).
    /// </summary>
    IsencaoFaixaReceitaSubstituicao = 203,

    /// <summary>Imune (300).</summary>
    Imune = 300,

    /// <summary>Não tributada pelo Simples Nacional (400).</summary>
    NaoTributada = 400,

    /// <summary>ICMS cobrado anteriormente por substituição tributária ou por antecipação (500).</summary>
    CobradoAnteriormente = 500,

    /// <summary>Outros (900).</summary>
    Outros = 900,
}
