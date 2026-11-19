namespace OpenDFe.NFSe;

/// <summary>O tipo da dedução ou da redução (<c>tpDedRed</c>). O valor de cada membro é o do XML.</summary>
public enum TipoDeducao
{
    /// <summary>Alimentação e bebidas, frigobar (1).</summary>
    AlimentacaoBebidas = 1,

    /// <summary>Materiais (2).</summary>
    Materiais = 2,

    /// <summary>Produção externa (3).</summary>
    ProducaoExterna = 3,

    /// <summary>Reembolso de despesas (4).</summary>
    ReembolsoDespesas = 4,

    /// <summary>Repasse consorciado (5).</summary>
    RepasseConsorciado = 5,

    /// <summary>Repasse plano de saúde (6).</summary>
    RepassePlanoSaude = 6,

    /// <summary>Serviços (7).</summary>
    Servicos = 7,

    /// <summary>Subempreitada de mão de obra (8).</summary>
    Subempreitada = 8,

    /// <summary>Profissional parceiro (9).</summary>
    ProfissionalParceiro = 9,

    /// <summary>Outras deduções (99).</summary>
    Outras = 99,
}
