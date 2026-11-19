namespace OpenDFe.NFSe;

/// <summary>O código de situação tributária do PIS e da COFINS na NFS-e (<c>CST</c> do <c>piscofins</c>). O XML leva dois dígitos. O valor de cada membro é o do XML.</summary>
public enum CstPisCofinsServico
{
    /// <summary>Nenhum (0).</summary>
    Nenhum = 0,

    /// <summary>Tributável, alíquota básica (1).</summary>
    AliquotaBasica = 1,

    /// <summary>Tributável, alíquota diferenciada (2).</summary>
    AliquotaDiferenciada = 2,

    /// <summary>Tributável, alíquota por unidade (3).</summary>
    AliquotaUnidade = 3,

    /// <summary>Monofásica, revenda a alíquota zero (4).</summary>
    MonofasicaRevendaAliquotaZero = 4,

    /// <summary>Substituição tributária (5).</summary>
    SubstituicaoTributaria = 5,

    /// <summary>Alíquota zero (6).</summary>
    AliquotaZero = 6,

    /// <summary>Isenta (7).</summary>
    Isenta = 7,

    /// <summary>Não incidência (8).</summary>
    NaoIncidencia = 8,

    /// <summary>Suspensão (9).</summary>
    Suspensao = 9,

    /// <summary>Outras operações de saída (49).</summary>
    OutrasSaidas = 49,

    /// <summary>Crédito, receita tributada no mercado interno (50).</summary>
    CreditoReceitaTributada = 50,

    /// <summary>Crédito, receita não tributada no mercado interno (51).</summary>
    CreditoReceitaNaoTributada = 51,

    /// <summary>Crédito, receita de exportação (52).</summary>
    CreditoReceitaExportacao = 52,

    /// <summary>Crédito, receitas tributadas e não tributadas no mercado interno (53).</summary>
    CreditoReceitasMercadoInterno = 53,

    /// <summary>Crédito, receitas tributadas no mercado interno e de exportação (54).</summary>
    CreditoReceitasTributadaExportacao = 54,

    /// <summary>Crédito, receitas não tributadas no mercado interno e de exportação (55).</summary>
    CreditoReceitasNaoTributadaExportacao = 55,

    /// <summary>Crédito, receitas tributadas e não tributadas no mercado interno e de exportação (56).</summary>
    CreditoReceitasTodas = 56,

    /// <summary>Crédito presumido, receita tributada no mercado interno (60).</summary>
    CreditoPresumidoReceitaTributada = 60,

    /// <summary>Crédito presumido, receita não tributada no mercado interno (61).</summary>
    CreditoPresumidoReceitaNaoTributada = 61,

    /// <summary>Crédito presumido, receita de exportação (62).</summary>
    CreditoPresumidoReceitaExportacao = 62,

    /// <summary>Crédito presumido, receitas tributadas e não tributadas no mercado interno (63).</summary>
    CreditoPresumidoReceitasMercadoInterno = 63,

    /// <summary>Crédito presumido, receitas tributadas no mercado interno e de exportação (64).</summary>
    CreditoPresumidoReceitasTributadaExportacao = 64,

    /// <summary>Crédito presumido, receitas não tributadas no mercado interno e de exportação (65).</summary>
    CreditoPresumidoReceitasNaoTributadaExportacao = 65,

    /// <summary>Crédito presumido, receitas tributadas e não tributadas no mercado interno e de exportação (66).</summary>
    CreditoPresumidoReceitasTodas = 66,

    /// <summary>Crédito presumido, outras operações (67).</summary>
    CreditoPresumidoOutras = 67,

    /// <summary>Aquisição sem direito a crédito (70).</summary>
    AquisicaoCreditoVedado = 70,

    /// <summary>Aquisição com isenção (71).</summary>
    AquisicaoIsencao = 71,

    /// <summary>Aquisição com suspensão (72).</summary>
    AquisicaoSuspensao = 72,

    /// <summary>Aquisição a alíquota zero (73).</summary>
    AquisicaoAliquotaZero = 73,

    /// <summary>Aquisição sem incidência (74).</summary>
    AquisicaoNaoIncidencia = 74,

    /// <summary>Aquisição por substituição tributária (75).</summary>
    AquisicaoSubstituicao = 75,

    /// <summary>Outras operações de entrada (98).</summary>
    OutrasEntradas = 98,

    /// <summary>Outras operações (99).</summary>
    Outras = 99,
}
