namespace OpenDFe.NFe;

/// <summary>O modelo da nota de produtor referenciada (<c>refNFP/mod</c>, <c>01</c> ou <c>04</c>).</summary>
public enum ModeloNotaProdutor
{
    /// <summary>Nota fiscal avulsa (modelo 01) (1).</summary>
    NotaAvulsa = 1,

    /// <summary>Nota fiscal de produtor (modelo 04) (4).</summary>
    NotaProdutor = 4,
}
