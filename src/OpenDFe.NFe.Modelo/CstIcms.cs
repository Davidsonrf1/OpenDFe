namespace OpenDFe.NFe;

/// <summary>O CST do ICMS. O valor de cada membro é o do XML (<c>CST</c>).</summary>
/// <remarks>
/// Uma enumeração para todos os grupos: cada grupo aceita só uma parte dos códigos, e o schema aponta o código que o
/// grupo não aceita. O leiaute escreve o valor com os dois dígitos.
/// </remarks>
public enum CstIcms
{
    /// <summary>Tributada integralmente (<c>00</c>).</summary>
    Tributada = 0,

    /// <summary>Tributação monofásica própria sobre combustíveis (<c>02</c>).</summary>
    MonofasicaPropria = 2,

    /// <summary>Tributada e com cobrança do ICMS por substituição tributária (<c>10</c>).</summary>
    TributadaSubstituicao = 10,

    /// <summary>
    /// Tributação monofásica própria e com responsabilidade pela retenção sobre combustíveis (<c>15</c>).
    /// </summary>
    MonofasicaPropriaRetencao = 15,

    /// <summary>Com redução de base de cálculo (<c>20</c>).</summary>
    ReducaoBase = 20,

    /// <summary>Isenta ou não tributada e com cobrança do ICMS por substituição tributária (<c>30</c>).</summary>
    IsentaSubstituicao = 30,

    /// <summary>Isenta (<c>40</c>).</summary>
    Isenta = 40,

    /// <summary>Não tributada (<c>41</c>).</summary>
    NaoTributada = 41,

    /// <summary>Suspensão (<c>50</c>).</summary>
    Suspensao = 50,

    /// <summary>Diferimento (<c>51</c>).</summary>
    Diferimento = 51,

    /// <summary>Tributação monofásica sobre combustíveis com recolhimento diferido (<c>53</c>).</summary>
    MonofasicaDiferida = 53,

    /// <summary>ICMS cobrado anteriormente por substituição tributária (<c>60</c>).</summary>
    CobradoAnteriormente = 60,

    /// <summary>Tributação monofásica sobre combustíveis cobrada anteriormente (<c>61</c>).</summary>
    MonofasicaCobradaAnteriormente = 61,

    /// <summary>Com redução de base de cálculo e cobrança do ICMS por substituição tributária (<c>70</c>).</summary>
    ReducaoBaseSubstituicao = 70,

    /// <summary>Outras (<c>90</c>).</summary>
    Outras = 90,
}
