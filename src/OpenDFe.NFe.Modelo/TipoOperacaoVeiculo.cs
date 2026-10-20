namespace OpenDFe.NFe;

/// <summary>O tipo da operação com o veículo novo. O valor de cada membro é o do XML (<c>tpOp</c>).</summary>
public enum TipoOperacaoVeiculo
{
    /// <summary>Outros (0).</summary>
    Outros = 0,

    /// <summary>Venda concessionária (1).</summary>
    VendaConcessionaria = 1,

    /// <summary>Faturamento direto para consumidor final (2).</summary>
    FaturamentoDireto = 2,

    /// <summary>Venda direta para grandes consumidores (frotista, governo e afins) (3).</summary>
    VendaDireta = 3,
}
