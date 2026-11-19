using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// As descrições das enumerações que o DANFSe imprime, tiradas do <c>xs:documentation</c> do XSD 1.01 (docs/NFSE.md 12,
/// decisão G10): a NT 008/2026 manda "utilizar a descrição destas opções". O valor que a lista não tem sai como o código.
/// </summary>
internal static class TextosDanfse
{
    internal static string AmbienteGerador(AmbienteGerador? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        NFSe.AmbienteGerador.Prefeitura => "Prefeitura",
        NFSe.AmbienteGerador.SistemaNacional => "Sistema Nacional da NFS-e",
        _ => Codigo((int)valor),
    };

    internal static string Ambiente(Ambiente? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        OpenDFe.Ambiente.Producao => "Produção",
        OpenDFe.Ambiente.Homologacao => "Homologação",
        _ => Codigo((int)valor),
    };

    internal static string Emitente(EmitenteDps? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        EmitenteDps.Prestador => "Prestador",
        EmitenteDps.Tomador => "Tomador",
        EmitenteDps.Intermediario => "Intermediário",
        _ => Codigo((int)valor),
    };

    internal static string Situacao(SituacaoNFSe? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        SituacaoNFSe.Gerada => "NFS-e Gerada",
        SituacaoNFSe.DecisaoJudicial => "NFS-e de Decisão Judicial",
        SituacaoNFSe.Avulsa => "NFS-e Avulsa",
        SituacaoNFSe.Mei => "NFS-e MEI",
        // O 101 só existe na versão 1.00 (o TStat dela), e a 1.01 o tirou: a enumeração não tem o membro.
        (SituacaoNFSe)101 => "NFS-e de Substituição Gerada",
        _ => Codigo((int)valor),
    };

    internal static string Finalidade(FinalidadeNFSe? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        FinalidadeNFSe.Regular => "NFS-e regular",
        _ => Codigo((int)valor),
    };

    internal static string Simples(SituacaoSimplesNacional? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        SituacaoSimplesNacional.NaoOptante => "Não Optante",
        SituacaoSimplesNacional.Mei => "Optante - Microempreendedor Individual (MEI)",
        SituacaoSimplesNacional.MeEpp => "Optante - Microempresa ou Empresa de Pequeno Porte (ME/EPP)",
        _ => Codigo((int)valor),
    };

    internal static string ApuracaoSimples(RegimeApuracaoSimples? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        RegimeApuracaoSimples.Simples => "Regime de apuração dos tributos federais e municipal pelo SN",
        RegimeApuracaoSimples.IssqnFora => "Regime de apuração dos tributos federais pelo SN e ISSQN por fora do SN conforme respectiva legislação municipal do tributo",
        RegimeApuracaoSimples.TodosFora => "Regime de apuração dos tributos federais e municipal por fora do SN conforme respectivas legislações federal e municipal de cada tributo",
        _ => Codigo((int)valor),
    };

    internal static string TributacaoIssqn(TributacaoIssqn? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        NFSe.TributacaoIssqn.Tributavel => "Operação tributável",
        NFSe.TributacaoIssqn.Imunidade => "Imunidade",
        NFSe.TributacaoIssqn.Exportacao => "Exportação de serviço",
        NFSe.TributacaoIssqn.NaoIncidencia => "Não Incidência",
        _ => Codigo((int)valor),
    };

    internal static string RegimeEspecial(RegimeEspecialTributacao? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        RegimeEspecialTributacao.Nenhum => "Nenhum",
        RegimeEspecialTributacao.AtoCooperado => "Ato Cooperado (Cooperativa)",
        RegimeEspecialTributacao.Estimativa => "Estimativa",
        RegimeEspecialTributacao.MicroempresaMunicipal => "Microempresa Municipal",
        RegimeEspecialTributacao.NotarioRegistrador => "Notário ou Registrador",
        RegimeEspecialTributacao.ProfissionalAutonomo => "Profissional Autônomo",
        RegimeEspecialTributacao.SociedadeProfissionais => "Sociedade de Profissionais",
        RegimeEspecialTributacao.Outros => "Outros",
        _ => Codigo((int)valor),
    };

    internal static string Imunidade(TipoImunidade? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        TipoImunidade.NaoInformada => "Imunidade (tipo não informado na nota de origem)",
        TipoImunidade.Reciproca => "Patrimônio, renda ou serviços, uns dos outros (CF88, Art 150, VI, a)",
        TipoImunidade.Templos => "Templos de qualquer culto (CF88, Art 150, VI, b)",
        TipoImunidade.PartidosEntidades => "Patrimônio, renda ou serviços dos partidos políticos, inclusive suas fundações, das entidades sindicais dos trabalhadores, das instituições de educação e de assistência social, sem fins lucrativos, atendidos os requisitos da lei (CF88, Art 150, VI, c)",
        TipoImunidade.LivrosJornais => "Livros, jornais, periódicos e o papel destinado a sua impressão (CF88, Art 150, VI, d)",
        TipoImunidade.Fonogramas => "Fonogramas e videofonogramas musicais produzidos no Brasil contendo obras musicais ou literomusicais de autores brasileiros e/ou obras em geral interpretadas por artistas brasileiros bem como os suportes materiais ou arquivos digitais que os contenham, salvo na etapa de replicação industrial de mídias ópticas de leitura a laser. (CF88, Art 150, VI, e)",
        _ => Codigo((int)valor),
    };

    internal static string Suspensao(TipoSuspensao? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        TipoSuspensao.DecisaoJudicial => "Exigibilidade Suspensa por Decisão Judicial",
        TipoSuspensao.ProcessoAdministrativo => "Exigibilidade Suspensa por Processo Administrativo",
        _ => Codigo((int)valor),
    };

    internal static string Beneficio(TipoBeneficio? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        TipoBeneficio.Isencao => "Isenção",
        TipoBeneficio.ReducaoPercentual => "Redução da BC em %",
        TipoBeneficio.ReducaoValor => "Redução da BC em R$",
        TipoBeneficio.AliquotaDiferenciada => "Alíquota Diferenciada",
        _ => Codigo((int)valor),
    };

    internal static string RetencaoIssqn(RetencaoIssqn? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        NFSe.RetencaoIssqn.NaoRetido => "Não Retido",
        NFSe.RetencaoIssqn.Tomador => "Retido pelo Tomador",
        NFSe.RetencaoIssqn.Intermediario => "Retido pelo Intermediário",
        _ => Codigo((int)valor),
    };

    internal static string RetencaoPisCofins(RetencaoPisCofins? valor) => valor switch
    {
        null => FormatadorDanfse.Ausente,
        NFSe.RetencaoPisCofins.NenhumRetido => "PIS/COFINS/CSLL Não Retidos",
        NFSe.RetencaoPisCofins.PisCofinsRetidos => "PIS/COFINS Retidos",
        NFSe.RetencaoPisCofins.PisCofinsNaoRetidos => "PIS/COFINS Não Retidos",
        NFSe.RetencaoPisCofins.TodosRetidos => "PIS/COFINS/CSLL Retidos",
        NFSe.RetencaoPisCofins.PisCofinsRetidosCsllNao => "PIS/COFINS Retidos, CSLL Não Retido",
        NFSe.RetencaoPisCofins.PisRetido => "PIS Retido, COFINS/CSLL Não Retido",
        NFSe.RetencaoPisCofins.CofinsRetida => "COFINS Retido, PIS/CSLL Não Retido",
        NFSe.RetencaoPisCofins.CofinsCsllRetidas => "PIS Não Retido, COFINS/CSLL Retidos",
        NFSe.RetencaoPisCofins.CsllRetida => "PIS/COFINS Não Retidos, CSLL Retido",
        NFSe.RetencaoPisCofins.PisCsllRetidos => "COFINS Não Retido, PIS/CSLL Retidos",
        _ => Codigo((int)valor),
    };

    private static string Codigo(int valor) => valor.ToString(CultureInfo.InvariantCulture);
}
