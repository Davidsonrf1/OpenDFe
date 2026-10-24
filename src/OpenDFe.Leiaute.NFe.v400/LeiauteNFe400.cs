using System.Collections.ObjectModel;
using OpenDFe.Impressao;

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
/// ambientes (etapa 4.2), e as operações, nos dois documentos: o status do serviço (<see cref="ConsultaStatus"/>, etapa
/// 4.3); a autorização (<see cref="NFe.Autorizacao"/>), a consulta do recibo (<see cref="ConsultaRecibo"/>) e o preparo
/// da nota (<see cref="DFe.Preparar{TDocumento}"/>), etapa 4.4, e a NFC-e com o QR Code que a lib escreve (etapa 5.1,
/// <see cref="ConfiguracaoNFCe"/>); a consulta do protocolo (<see cref="ConsultaProtocolo"/>) e a recuperação da
/// autorização (<see cref="RecuperacaoAutorizacao"/>), etapa 4.5; o registro de eventos (<see cref="EnvioEvento"/>, com o
/// <see cref="Cancelamento"/>) e a inutilização (<see cref="Inutilizacao"/>), etapa 6.1; e, só na NF-e, a distribuição DF-e (<see cref="DistribuicaoDFe"/>, ao AN), etapa 8.1. Declara também as formas de XML que lê (o
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

    private static readonly OperacaoEvento Eventos = new();

    private static readonly OperacaoInutilizacao Inutilizar = new();

    private static readonly OperacaoDistribuicao Distribuicao = new();

    /// <summary>A versão do leiaute, como no atributo <c>versao</c> do XML: <c>"4.00"</c>.</summary>
    public string Versao => "4.00";

    /// <summary>A NF-e e a NFC-e.</summary>
    public IReadOnlyList<Documento> Documentos => Atendidos;

    /// <summary>
    /// A tabela das URLs de consulta da NFC-e (docs/NTS.md 4): a do QR Code e a da consulta pela chave, por UF e ambiente,
    /// só as linhas conferidas em fonte primária; a UF que não consta usa a URL da <see cref="ConfiguracaoNFCe"/>. A
    /// NF-e com <c>tpImp</c> 6 usa a mesma tabela (NT 2026.002).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "A tabela é do leiaute, que quem usa recebe do DFe.Detectar ou cria; docs/CONTRATOS.md 16.1.")]
    public IReadOnlyList<UrlConsultaNFCe> UrlsConsulta => UrlsConsultaNFCe.Todas;

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
        declaracao.RegistrarOperacao(Autorizacao);
        declaracao.RegistrarOperacao(Recibo);
        declaracao.RegistrarOperacao(Recuperacao);
        declaracao.RegistrarOperacao(Eventos);
        declaracao.RegistrarOperacao(Inutilizar);
        if (declaracao.Documento == Documento.NFe)
            declaracao.RegistrarOperacao(Distribuicao);
        declaracao.RegistrarPreparo(Preparo);
        declaracao.DeclararConfiguracaoDocumento<ConfiguracaoNFCe>(obrigatoria: false);
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

    /// <summary>
    /// Monta o DANFE NFC-e (o DANFCE, a NFC-e impressa em bobina) de uma NFC-e lida: o <c>nfeProc</c> autorizado, ou o
    /// <c>NFe</c> assinado em contingência offline, sem protocolo (etapa 7.2, docs/IMPRESSAO.md 1).
    /// </summary>
    /// <remarks>
    /// <para>
    /// As divisões, o texto fixo e a ordem são os do Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0. O QR Code do
    /// documento é o das informações suplementares da nota (<c>qrCode</c>), sem alteração; a data e a hora são as que a nota
    /// traz, sem conversão de fuso. Não confere o schema nem as regras do MOC, e a lib não decide quantas vias imprimir:
    /// na contingência, quem imprime chama o método uma vez por via (<see cref="OpcoesDanfce.Via"/>). A NFC-e cancelada
    /// não é tratada. Sem estado e seguro em paralelo.
    /// </para>
    /// </remarks>
    /// <param name="nota">A nota lida (<see cref="Ler"/>).</param>
    /// <param name="opcoes">O papel, o logotipo, a assinatura do software, o rodapé e a via; <see langword="null"/> é o padrão.</param>
    /// <returns>O documento de impressão, para o renderizador do PDV ou do PDF.</returns>
    /// <exception cref="ArgumentNullException">A nota lida é nula.</exception>
    /// <exception cref="ArgumentException">
    /// A leitura não achou a nota; a nota não é NFC-e (modelo 65); não tem a chave; não tem o QR Code e a URL de consulta
    /// (<c>qrCode</c> e <c>urlChave</c>); o protocolo é de outra situação que não 100 ou 150; ou o papel das opções não é a
    /// bobina de 80 ou de 58 mm.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "O DANFCE é do leiaute, que quem usa recebe do DFe.Detectar ou cria (docs/CONTRATOS.md 20).")]
    public DocumentoImpressao Danfce(NotaLida nota, OpcoesDanfce? opcoes = null) => MontagemDanfce.Montar(nota, opcoes);

    /// <summary>
    /// Monta o DANFE da NF-e (modelo 55) de uma NF-e lida: o <c>nfeProc</c> autorizado, ou o <c>NFe</c> assinado em
    /// contingência, sem protocolo (etapa 7.3, docs/IMPRESSAO.md 2). O <c>tpImp</c> da nota escolhe o documento: retrato (1)
    /// e paisagem (2) saem no A4, com os quadros e os campos do Anexo II do MOC 7.0 e os acréscimos da NT 2026.010; o
    /// Simplificado Tipo 2 (6) sai no leiaute do DANFE NFC-e (NT 2026.003).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Imprime o que a nota tem, sem converter o fuso das datas, e não confere o schema nem as regras do MOC. O QR Code e a
    /// URL de consulta só saem quando a nota os traz (o Tipo 2 os exige). O documento leva o cabeçalho de continuação das
    /// páginas seguintes, as fichas de página (<see cref="TextoPaginado"/>) e a marca d'água do ambiente de homologação ou da
    /// nota cancelada (<see cref="OpcoesDanfe.Cancelada"/>). O DANFE Simplificado (3) não está implementado. Sem estado e
    /// seguro em paralelo.
    /// </para>
    /// </remarks>
    /// <param name="nota">A nota lida (<see cref="Ler"/>).</param>
    /// <param name="opcoes">O logotipo, o canhoto, a orientação, a situação de cancelada e o rodapé; <see langword="null"/> é o padrão.</param>
    /// <returns>O documento de impressão, para o renderizador do PDF.</returns>
    /// <exception cref="ArgumentNullException">A nota lida é nula.</exception>
    /// <exception cref="ArgumentException">
    /// A leitura não achou a nota; a nota não é NF-e (modelo 55); não tem a chave; o protocolo é de outra situação que não 100
    /// ou 150; o <c>tpImp</c> é 0 (sem DANFE), 3 (não implementado), 4 ou 5 (DANFE NFC-e), ou não informado sem a orientação
    /// nas opções; a orientação ou o papel das opções não cabem no formato; ou o Tipo 2 sem o QR Code.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "O DANFE é do leiaute, que quem usa recebe do DFe.Detectar ou cria (docs/CONTRATOS.md 21).")]
    public DocumentoImpressao Danfe(NotaLida nota, OpcoesDanfe? opcoes = null) => MontagemDanfe.Montar(nota, opcoes);
}
