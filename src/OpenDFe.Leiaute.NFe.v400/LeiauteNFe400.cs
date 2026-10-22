using System.Collections.ObjectModel;

namespace OpenDFe.NFe;

/// <summary>
/// O leiaute 4.00 da NF-e (modelo 55) e da NFC-e (modelo 65), os dois documentos no mesmo leiaute e no mesmo
/// modelo de dados (PROJETO.md 3.1): o que se passa a <see cref="DFe.Criar"/>.
/// </summary>
/// <remarks>
/// <para>
/// Sem estado e seguro para uso concorrente: uma instância serve a todos os núcleos, de qualquer emitente e
/// documento. Os schemas são os do pacote de liberação embutido (docs/NTS.md), carregados na primeira vez que
/// uma operação os usa.
/// </para>
/// <para>
/// Ainda não declara operações, atendimentos nem URLs, que entram nas fases 4 e 5; até lá, <see cref="DFe.Criar"/>
/// recusa a configuração, porque nenhuma UF tem atendimento. Declara as formas de XML que lê (o <c>NFe</c> e o
/// <c>nfeProc</c>), para o <see cref="DFe.Detectar"/>, e lê a nota (<see cref="Ler"/>) sem núcleo.
/// </para>
/// </remarks>
public sealed class LeiauteNFe400 : ILeiaute
{
    private const string Namespace = "http://www.portalfiscal.inf.br/nfe";

    private static readonly ReadOnlyCollection<Documento> Atendidos = Array.AsReadOnly([Documento.NFe, Documento.NFCe]);

    private static readonly FormaLeitura FormaNFe = new(Namespace, "NFe", "infNFe/@versao", "infNFe/ide/mod");

    private static readonly FormaLeitura FormaNfeProc = new(Namespace, "nfeProc", "@versao", "NFe/infNFe/ide/mod");

    /// <summary>A versão do leiaute, como no atributo <c>versao</c> do XML: <c>"4.00"</c>.</summary>
    public string Versao => "4.00";

    /// <summary>A NF-e e a NFC-e.</summary>
    public IReadOnlyList<Documento> Documentos => Atendidos;

    /// <summary>
    /// Declara o que o leiaute tem para o documento: hoje, só as formas de XML que ele lê (o <c>NFe</c> e o
    /// <c>nfeProc</c>), iguais para a NF-e e a NFC-e; o modelo do XML diz qual dos dois.
    /// </summary>
    /// <param name="declaracao">Onde declarar.</param>
    /// <exception cref="ArgumentNullException">A declaração é nula.</exception>
    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        ArgumentNullException.ThrowIfNull(declaracao);
        declaracao.DeclararLeitura(FormaNFe);
        declaracao.DeclararLeitura(FormaNfeProc);
    }

    /// <summary>
    /// Lê uma NF-e ou NFC-e: o <c>NFe</c>, ou o <c>nfeProc</c> com o protocolo, de qualquer NT do leiaute 4.00
    /// (PROJETO.md 5.2, docs/LEITURA.md).
    /// </summary>
    /// <param name="xml">O XML recebido.</param>
    /// <returns>A nota, a chave, o protocolo, o XML recebido e os problemas.</returns>
    /// <exception cref="ArgumentNullException">O XML é nulo.</exception>
    /// <remarks>
    /// <para>
    /// Não valida pelo schema nem confere as regras do MOC: lê o que reconhece, pelo nome de cada campo, e aponta o que
    /// não reconhece. O campo de NT anterior que saiu do schema é aviso (<c>NFE-RETIRADO</c>), e o valor fica no XML.
    /// </para>
    /// <para>
    /// Não lança pelo conteúdo: XML mal formado, outra raiz ou outra versão são problemas, com a nota nula. Sem DTD nem
    /// entidade externa. Sem estado e seguro em paralelo.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "A leitura é do leiaute, que quem usa recebe do DFe.Detectar (docs/LEITURA.md, decisão 1).")]
    public NotaLida Ler(string xml) => LeitorNFe.Ler(xml);
}
