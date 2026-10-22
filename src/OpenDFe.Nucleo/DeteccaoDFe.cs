namespace OpenDFe;

/// <summary>
/// O que <see cref="DFe.Detectar"/> achou num XML: o leiaute que o lê, o documento, a raiz e a versão. Criado pelo
/// núcleo.
/// </summary>
public sealed class DeteccaoDFe
{
    internal DeteccaoDFe(ILeiaute leiaute, Documento documento, string raiz, string versao)
    {
        Leiaute = leiaute;
        Documento = documento;
        Raiz = raiz;
        Versao = versao;
    }

    /// <summary>O leiaute que lê o XML, a instância recebida pelo detector.</summary>
    public ILeiaute Leiaute { get; }

    /// <summary>O documento, pelo modelo do XML.</summary>
    public Documento Documento { get; }

    /// <summary>O nome local da raiz (<c>NFe</c>, <c>nfeProc</c>).</summary>
    public string Raiz { get; }

    /// <summary>A versão do XML, igual à do leiaute (<c>4.00</c>).</summary>
    public string Versao { get; }
}
