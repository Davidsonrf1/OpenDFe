namespace OpenDFe.NFSe;

/// <summary>O valor do serviço (<c>vServPrest</c>).</summary>
public sealed class ValorServico
{
    /// <summary>O valor recebido pelo intermediário; opcional.</summary>
    [Tag("vReceb")]
    public decimal? ValorRecebido { get; set; }

    /// <summary>O valor do serviço.</summary>
    [Tag("vServ")]
    public decimal? Valor { get; set; }
}
