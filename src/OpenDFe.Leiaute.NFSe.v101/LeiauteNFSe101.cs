using System.Collections.ObjectModel;

namespace OpenDFe.NFSe;

/// <summary>
/// O leiaute 1.01 da NFS-e do padrão nacional (docs/NFSE.md, PROJETO.md 18): o que se passa a <see cref="DFe.Criar"/>
/// com <see cref="Documento.NFSe"/> (D1).
/// </summary>
/// <remarks>
/// <para>
/// Sem estado e seguro para uso concorrente: uma instância serve a todos os núcleos. Os schemas são os do pacote de
/// produção embutido (docs/NTS.md 12.1), carregados na primeira vez que são usados.
/// </para>
/// <para>
/// Declara as formas de XML que lê (a <c>DPS</c>, a <c>NFSe</c>, o <c>pedRegEvento</c> e o <c>evento</c>, nas versões 1.00
/// e 1.01) para o <see cref="DFe.Detectar"/>, e as regras fora do schema da DPS; escreve a DPS (<see cref="Escrever"/>) e lê
/// a NFS-e, a DPS e os eventos (<see cref="Ler"/>, <see cref="LerEvento"/>) sem núcleo. Desde a 9.2, as operações na Sefin
/// Nacional: a emissão, a consulta pela chave e pela DPS e a <see cref="RecuperacaoEmissao"/>; desde a 9.3, o registro dos
/// eventos de quem emite (<see cref="EnvioEventoNFSe"/>) e a consulta dos eventos (<see cref="ConsultaEventosNFSe"/>); desde a
/// 9.4, a parametrização municipal no ADN (<see cref="ConsultaConvenio"/>, <see cref="ConsultaAliquota"/>,
/// <see cref="ConsultaRegimesEspeciais"/>, <see cref="ConsultaRetencoes"/> e <see cref="ConsultaBeneficio"/>).
/// </para>
/// </remarks>
public sealed class LeiauteNFSe101 : ILeiaute
{
    private static readonly ReadOnlyCollection<Documento> Atendidos = Array.AsReadOnly([Documento.NFSe]);

    private static readonly string[] Anteriores = ["1.00"];

    private static readonly FormaLeitura[] Formas =
    [
        new(EscritaNFSe.Namespace, "DPS", "@versao", Documento.NFSe) { OutrasVersoes = Anteriores },
        new(EscritaNFSe.Namespace, "NFSe", "@versao", Documento.NFSe) { OutrasVersoes = Anteriores },
        new(EscritaNFSe.Namespace, "pedRegEvento", "@versao", Documento.NFSe) { OutrasVersoes = Anteriores },
        new(EscritaNFSe.Namespace, "evento", "@versao", Documento.NFSe) { OutrasVersoes = Anteriores },
    ];

    private LeiauteNFSe101()
    {
    }

    /// <summary>A instância única (o leiaute não tem estado).</summary>
    public static LeiauteNFSe101 Instancia { get; } = new();

    /// <summary>A versão que o leiaute escreve, como no atributo <c>versao</c> do XML: <c>"1.01"</c>. Lê também a 1.00.</summary>
    public string Versao => EscritorNFSe.Versao;

    /// <summary>A NFS-e.</summary>
    public IReadOnlyList<Documento> Documentos => Atendidos;

    /// <summary>
    /// Declara o que o leiaute tem para a NFS-e: as operações REST na Sefin Nacional (a <see cref="EmissaoNFSe"/>, a
    /// <see cref="ConsultaNFSe"/>, a <see cref="ConsultaDps"/>, o <see cref="EnvioEventoNFSe"/> e a
    /// <see cref="ConsultaEventosNFSe"/>; D4), as cinco da parametrização municipal no ADN (docs/NFSE.md 11), as bases da Sefin e do ADN nos dois ambientes, as quatro formas de
    /// XML que ele lê (D7), com a versão 1.00 além da 1.01, e as regras fora do schema da DPS (docs/NFSE.md 4). Sem
    /// atendimento por UF: o destino é sempre a Sefin (D3).
    /// </summary>
    /// <param name="declaracao">Onde declarar.</param>
    /// <exception cref="ArgumentNullException">A declaração é nula.</exception>
    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        ArgumentNullException.ThrowIfNull(declaracao);
        declaracao.RegistrarOperacao(new OperacaoEmissaoNFSe());
        declaracao.RegistrarOperacao(new OperacaoConsultaNFSe());
        declaracao.RegistrarOperacao(new OperacaoConsultaDps());
        declaracao.RegistrarOperacao(new OperacaoEnvioEventoNFSe());
        declaracao.RegistrarOperacao(new OperacaoConsultaEventosNFSe());
        declaracao.RegistrarOperacao(new OperacaoConsultaConvenio());
        declaracao.RegistrarOperacao(new OperacaoConsultaAliquota());
        declaracao.RegistrarOperacao(new OperacaoConsultaRegimesEspeciais());
        declaracao.RegistrarOperacao(new OperacaoConsultaRetencoes());
        declaracao.RegistrarOperacao(new OperacaoConsultaBeneficio());
        SefinNacional.Declarar(declaracao);
        Parametrizacao.Declarar(declaracao);
        foreach (var forma in Formas)
            declaracao.DeclararLeitura(forma);
        declaracao.DeclararRegras(RegrasNFSe101.Conferir);
    }

    /// <summary>
    /// Escreve a DPS 1.01 sem transmitir: o XML para guardar, assinar fora ou mandar ao emulador, com o identificador e os
    /// problemas da montagem (docs/NFSE.md 3.3).
    /// </summary>
    /// <param name="dps">A DPS.</param>
    /// <returns>O XML sem a assinatura, o identificador e os problemas.</returns>
    /// <exception cref="ArgumentNullException">A DPS é nula.</exception>
    /// <remarks>
    /// Sem núcleo, o ambiente é o da DPS (nulo, o <c>tpAmb</c> não sai, e o schema aponta); a versão do aplicativo nula
    /// vale a padrão. Não confere o schema nem as regras (a emissão, na 9.2, confere). Não altera a DPS.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "A escrita é do leiaute, que quem usa recebe do DFe.Detectar ou da Instancia (docs/NFSE.md 3.3).")]
    public DpsEscrita Escrever(Dps dps) => EscritorNFSe.Escrever(dps, ambienteNucleo: null);

    /// <summary>Lê uma NFS-e (<c>NFSe</c>) ou uma DPS (<c>DPS</c>), das versões 1.00 e 1.01 (D8).</summary>
    /// <param name="xml">O XML recebido.</param>
    /// <returns>A nota, a DPS, a chave, o identificador, a versão, o XML recebido e os problemas.</returns>
    /// <exception cref="ArgumentNullException">O XML é nulo.</exception>
    /// <remarks>
    /// Não valida pelo schema nem confere as regras: lê o que reconhece, pelo nome de cada campo, e aponta o que não
    /// reconhece. Não lança pelo conteúdo. Os campos da 1.00 que a 1.01 tirou entram nas propriedades marcadas "só na
    /// versão 1.00" (docs/NFSE.md 7, M10).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "A leitura é do leiaute, que quem usa recebe do DFe.Detectar (docs/LEITURA.md, decisão 1).")]
    public NFSeLida Ler(string xml) => LeitorNFSe.Ler(xml);

    /// <summary>
    /// Lê um evento da NFS-e: o pedido de registro (<c>pedRegEvento</c>) ou o evento registrado (<c>evento</c>), das versões
    /// 1.00 e 1.01, de qualquer autor (D9).
    /// </summary>
    /// <param name="xml">O XML recebido.</param>
    /// <returns>O pedido, o evento registrado, o Id, a versão, o XML recebido e os problemas.</returns>
    /// <exception cref="ArgumentNullException">O XML é nulo.</exception>
    /// <remarks>Como <see cref="Ler"/>: sem o schema, sem lançar pelo conteúdo.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "A leitura é do leiaute, que quem usa recebe do DFe.Detectar (docs/LEITURA.md, decisão 1).")]
    public EventoLido LerEvento(string xml) => LeitorNFSe.LerEvento(xml);
}
