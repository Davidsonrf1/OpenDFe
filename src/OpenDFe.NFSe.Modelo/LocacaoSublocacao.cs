namespace OpenDFe.NFSe;

/// <summary>A locação, a sublocação, o arrendamento, o direito de passagem ou a permissão de uso (<c>lsadppu</c>). Só na versão 1.00: a leitura preenche, a escrita da 1.01 não escreve (docs/NFSE.md 7, M10).</summary>
public sealed class LocacaoSublocacao
{
    /// <summary>A categoria do serviço.</summary>
    [Tag("categ")]
    public CategoriaLocacao? Categoria { get; set; }

    /// <summary>O objeto.</summary>
    [Tag("objeto")]
    public ObjetoLocacao? Objeto { get; set; }

    /// <summary>A extensão total da ferrovia, da rodovia, dos cabos, dos dutos ou dos condutos.</summary>
    [Tag("extensao")]
    public int? Extensao { get; set; }

    /// <summary>O número total de postes.</summary>
    [Tag("nPostes")]
    public int? Postes { get; set; }
}
