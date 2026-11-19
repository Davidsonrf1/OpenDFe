namespace OpenDFe.NFSe;

/// <summary>O serviço prestado (<c>serv</c>).</summary>
public sealed class Servico
{
    /// <summary>O local da prestação.</summary>
    [Tag("locPrest")]
    public LocalPrestacao Local { get; set; } = new();

    /// <summary>Os códigos e a descrição do serviço.</summary>
    [Tag("cServ")]
    public CodigoServico Codigo { get; set; } = new();

    /// <summary>A exportação ou a importação do serviço; opcional.</summary>
    [Tag("comExt")]
    public ComercioExterior? ComercioExterior { get; set; }

    /// <summary>A locação, a sublocação, o arrendamento, o direito de passagem ou a permissão de uso; só na versão 1.00.</summary>
    [Tag("lsadppu")]
    public LocacaoSublocacao? Locacao { get; set; }

    /// <summary>O serviço de obra; opcional.</summary>
    [Tag("obra")]
    public Obra? Obra { get; set; }

    /// <summary>A atividade de evento (artístico, cultural, esportivo); opcional.</summary>
    [Tag("atvEvento")]
    public AtividadeEvento? Evento { get; set; }

    /// <summary>O pedágio; só na versão 1.00.</summary>
    [Tag("explRod")]
    public ExploracaoRodoviaria? ExploracaoRodoviaria { get; set; }

    /// <summary>As informações complementares; opcional.</summary>
    [Tag("infoCompl")]
    public InformacoesComplementares? InformacoesComplementares { get; set; }
}
