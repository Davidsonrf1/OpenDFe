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
/// Declara os serviços SOAP, quem atende cada UF em cada tipo de emissão e as URLs dos autorizadores nos dois
/// ambientes (etapa 4.2), e as operações: o status do serviço (<see cref="ConsultaStatus"/>, etapa 4.3), nos dois
/// documentos; a autorização (<see cref="NFe.Autorizacao"/>), a consulta do recibo (<see cref="ConsultaRecibo"/>) e o
/// preparo da nota (<see cref="DFe.Preparar{TDocumento}"/>), etapa 4.4, só na NF-e (a NFC-e entra com o QR Code, na
/// 5.1); a consulta do protocolo (<see cref="ConsultaProtocolo"/>), nos dois documentos, e a recuperação da
/// autorização (<see cref="RecuperacaoAutorizacao"/>), só na NF-e, etapa 4.5. Declara também as formas de XML que lê (o
/// <c>NFe</c> e o <c>nfeProc</c>), para o <see cref="DFe.Detectar"/>, e lê a nota (<see cref="Ler"/>) sem núcleo.
/// </para>
/// </remarks>
public sealed class LeiauteNFe400 : ILeiaute
{
    private const string Namespace = "http://www.portalfiscal.inf.br/nfe";

    private static readonly ReadOnlyCollection<Documento> Atendidos = Array.AsReadOnly([Documento.NFe, Documento.NFCe]);

    private static readonly FormaLeitura FormaNFe = new(Namespace, "NFe", "infNFe/@versao", "infNFe/ide/mod");

    private static readonly FormaLeitura FormaNfeProc = new(Namespace, "nfeProc", "@versao", "NFe/infNFe/ide/mod");

    private static readonly OperacaoStatus Status = new();

    private static readonly OperacaoAutorizacao Autorizacao = new();

    private static readonly OperacaoRecibo Recibo = new();

    private static readonly PreparoNFe Preparo = new();

    private static readonly OperacaoConsultaProtocolo Consulta = new();

    private static readonly OperacaoRecuperacao Recuperacao = new();

    /// <summary>A versão do leiaute, como no atributo <c>versao</c> do XML: <c>"4.00"</c>.</summary>
    public string Versao => "4.00";

    /// <summary>A NF-e e a NFC-e.</summary>
    public IReadOnlyList<Documento> Documentos => Atendidos;

    /// <summary>
    /// Declara o que o leiaute tem para o documento: as operações e o preparo, as formas de XML que ele lê (o <c>NFe</c> e o <c>nfeProc</c>),
    /// iguais para a NF-e e a NFC-e (o modelo do XML diz qual dos dois), as regras do MOC que ele confere antes de
    /// transmitir (docs/REGRAS.md) e os autorizadores do documento: os serviços SOAP, os atendimentos e as URLs
    /// (docs/NTS.md 3).
    /// </summary>
    /// <param name="declaracao">Onde declarar.</param>
    /// <exception cref="ArgumentNullException">A declaração é nula.</exception>
    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        ArgumentNullException.ThrowIfNull(declaracao);
        declaracao.RegistrarOperacao(Status);
        declaracao.RegistrarOperacao(Consulta);
        if (declaracao.Documento == Documento.NFe)
        {
            declaracao.RegistrarOperacao(Autorizacao);
            declaracao.RegistrarOperacao(Recibo);
            declaracao.RegistrarOperacao(Recuperacao);
            declaracao.RegistrarPreparo(Preparo);
        }
        declaracao.DeclararLeitura(FormaNFe);
        declaracao.DeclararLeitura(FormaNfeProc);
        declaracao.DeclararRegras(RegrasNFe400.Conferir);
        AutorizadoresNFe400.Declarar(declaracao);
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
