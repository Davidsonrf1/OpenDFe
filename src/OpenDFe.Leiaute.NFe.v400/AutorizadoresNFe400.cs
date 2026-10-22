namespace OpenDFe.NFe;

/// <summary>
/// Os autorizadores da NF-e e da NFC-e (PROJETO.md 8.4): os serviços SOAP, quem atende cada UF em cada tipo de
/// emissão e as URLs de cada autorizador nos dois ambientes, conferidos nas fontes em 2026-10-22 (docs/NTS.md 3).
/// </summary>
/// <remarks>
/// <para>
/// Os serviços seguem a NT 2016.002 v1.61, 2.3: o namespace do WSDL é
/// <c>http://www.portalfiscal.inf.br/nfe/wsdl/</c> mais o nome do serviço, e a ação é o namespace mais o método.
/// </para>
/// <para>
/// A NF-e segue a "Relação de Serviços Web" do Portal Nacional; a NFC-e, a do Portal da SVRS (que não lista MG) e a
/// página de Web Services da SEF-MG. A UF que não tem autorizador próprio vai à SVRS (e o MA, na NF-e, à SVAN).
/// </para>
/// </remarks>
internal static partial class AutorizadoresNFe400
{
    internal const string Autorizacao = "NFeAutorizacao4";
    internal const string RetAutorizacao = "NFeRetAutorizacao4";
    internal const string ConsultaProtocolo = "NFeConsultaProtocolo4";
    internal const string Status = "NFeStatusServico4";
    internal const string Inutilizacao = "NFeInutilizacao4";
    internal const string RecepcaoEvento = "NFeRecepcaoEvento4";
    internal const string ConsultaCadastro = "CadConsultaCadastro4";

    private const string NamespaceWsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/";

    // O serviço e o método do WSDL (NT 2016.002, 2.3). A distribuição DF-e (NFeDistribuicaoDFe, no AN) fica para a
    // fase 8: o nfeDadosMsg dela vai dentro do elemento da operação, que o envelope do núcleo ainda não escreve.
    private static readonly (string Nome, string Metodo)[] Servicos =
    [
        (Autorizacao, "nfeAutorizacaoLote"),
        (RetAutorizacao, "nfeRetAutorizacaoLote"),
        (ConsultaProtocolo, "nfeConsultaNF"),
        (Status, "nfeStatusServicoNF"),
        (Inutilizacao, "nfeInutilizacaoNF"),
        (RecepcaoEvento, "nfeRecepcaoEvento"),
        (ConsultaCadastro, "consultaCadastro"),
    ];

    // As UFs com autorizador próprio; as outras vão à SVRS, menos o MA da NF-e, que vai à SVAN.
    private static readonly UF[] ProprioNFe = [UF.AM, UF.BA, UF.GO, UF.MG, UF.MS, UF.MT, UF.PE, UF.PR, UF.RS, UF.SP];
    private static readonly UF[] ProprioNFCe = [UF.AM, UF.GO, UF.MG, UF.MS, UF.MT, UF.PR, UF.RS, UF.SP];

    // A contingência da NF-e. O PI está nas duas: a produção o põe na SVC-AN e a homologação, na SVC-RS (o Portal
    // diverge em 2026-10-22); quem usa escolhe pelo tipo de emissão (decisão do usuário na 4.2).
    private static readonly UF[] SvcAN =
        [UF.AC, UF.AL, UF.AP, UF.CE, UF.DF, UF.ES, UF.MG, UF.PA, UF.PB, UF.PI, UF.RJ, UF.RN, UF.RO, UF.RR, UF.RS, UF.SC, UF.SE, UF.SP, UF.TO];
    private static readonly UF[] SvcRS = [UF.AM, UF.BA, UF.GO, UF.MA, UF.MS, UF.MT, UF.PE, UF.PI, UF.PR];

    // Os tipos de emissão que vão ao autorizador normal da UF: a nota em formulário de segurança, a do EPEC e a
    // offline são transmitidas depois a ele (o evento do EPEC vai ao AN pelo destino do pedido). O offline na NF-e
    // é o do DANFE Simplificado Tipo 2 (NT 2025.001).
    private static readonly TipoEmissao[] NormalNFe =
        [TipoEmissao.Normal, TipoEmissao.ContingenciaFSIA, TipoEmissao.ContingenciaEpec, TipoEmissao.ContingenciaFSDA, TipoEmissao.ContingenciaOffline];
    private static readonly TipoEmissao[] NormalNFCe = [TipoEmissao.Normal, TipoEmissao.ContingenciaOffline];

    /// <summary>Declara os serviços, os atendimentos e as URLs do documento da declaração.</summary>
    internal static void Declarar(DeclaracaoLeiaute declaracao)
    {
        foreach (var (nome, metodo) in Servicos)
            declaracao.DeclararServico(new ServicoSoap(nome, NamespaceWsdl + nome, NamespaceWsdl + nome + "/" + metodo));

        var nfe = declaracao.Documento == Documento.NFe;
        foreach (var uf in Enum.GetValues<UF>())
        {
            var normal = Normal(uf, nfe);
            foreach (var emissao in nfe ? NormalNFe : NormalNFCe)
                declaracao.DeclararAtendimento(uf, emissao, normal);
            if (!nfe)
                continue;
            if (SvcAN.Contains(uf))
                declaracao.DeclararAtendimento(uf, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN);
            if (SvcRS.Contains(uf))
                declaracao.DeclararAtendimento(uf, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS);
        }

        foreach (var (autorizador, servico, homologacao, producao) in nfe ? UrlsNFe : UrlsNFCe)
        {
            declaracao.DeclararEndereco(new EnderecoServico(autorizador, Ambiente.Homologacao, servico, new Uri(homologacao)));
            declaracao.DeclararEndereco(new EnderecoServico(autorizador, Ambiente.Producao, servico, new Uri(producao)));
        }
    }

    private static Destino Normal(UF uf, bool nfe)
    {
        if ((nfe ? ProprioNFe : ProprioNFCe).Contains(uf))
            return uf.ComoDestino();
        return nfe && uf == UF.MA ? Destino.SVAN : Destino.SVRS;
    }
}
