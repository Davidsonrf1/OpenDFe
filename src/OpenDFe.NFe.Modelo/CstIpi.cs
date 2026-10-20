namespace OpenDFe.NFe;

/// <summary>O CST do IPI. O valor de cada membro é o do XML (<c>CST</c>).</summary>
/// <remarks>
/// O <see cref="IpiTributado"/> e o <see cref="IpiNaoTributado"/> aceitam cada um uma parte dos códigos, e o schema
/// aponta o código que o grupo não aceita. O leiaute escreve o valor com os dois dígitos.
/// </remarks>
public enum CstIpi
{
    /// <summary>Entrada com recuperação de crédito (<c>00</c>).</summary>
    EntradaRecuperacaoCredito = 0,

    /// <summary>Entrada tributada com alíquota zero (<c>01</c>).</summary>
    EntradaAliquotaZero = 1,

    /// <summary>Entrada isenta (<c>02</c>).</summary>
    EntradaIsenta = 2,

    /// <summary>Entrada não tributada (<c>03</c>).</summary>
    EntradaNaoTributada = 3,

    /// <summary>Entrada imune (<c>04</c>).</summary>
    EntradaImune = 4,

    /// <summary>Entrada com suspensão (<c>05</c>).</summary>
    EntradaSuspensao = 5,

    /// <summary>Outras entradas (<c>49</c>).</summary>
    OutrasEntradas = 49,

    /// <summary>Saída tributada (<c>50</c>).</summary>
    SaidaTributada = 50,

    /// <summary>Saída tributada com alíquota zero (<c>51</c>).</summary>
    SaidaAliquotaZero = 51,

    /// <summary>Saída isenta (<c>52</c>).</summary>
    SaidaIsenta = 52,

    /// <summary>Saída não tributada (<c>53</c>).</summary>
    SaidaNaoTributada = 53,

    /// <summary>Saída imune (<c>54</c>).</summary>
    SaidaImune = 54,

    /// <summary>Saída com suspensão (<c>55</c>).</summary>
    SaidaSuspensao = 55,

    /// <summary>Outras saídas (<c>99</c>).</summary>
    OutrasSaidas = 99,
}
