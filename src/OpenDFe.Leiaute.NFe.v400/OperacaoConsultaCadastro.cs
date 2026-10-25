using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// A consulta cadastro (<c>CadConsultaCadastro4</c>, MOC 7.0, Visão Geral, 5.6, docs/CONTRATOS.md 24): o <c>ConsCad</c> 2.00 e o
/// <c>retConsCad</c> 2.00 com os <c>infCad</c>. Só na NF-e.
/// </summary>
/// <remarks>
/// <para>
/// O pedido não é assinado, passa pelo <c>consCad_v2.00.xsd</c> (215) e vai ao autorizador da UF consultada, no atendimento
/// normal. A UF sem o serviço é recusada na montagem (<c>NFE-CADASTRO</c>) quando o <see cref="ParametroWS.Destino"/> não é
/// informado.
/// </para>
/// <para>
/// A leitura é branda (<see cref="LeituraRetorno"/>, docs/CONTRATOS.md 13, decisão 4): sem o <c>infCons</c>, o <c>cStat</c> ou
/// o <c>xMotivo</c>, a resposta é ilegível (<see cref="FormatException"/>); o campo que não se lê fica nulo, com
/// <c>NFE-VALOR</c>, e o que o leiaute não conhece é <c>NFE-DESCONHECIDO</c>, os dois como aviso. A situação e o
/// credenciamento com um número que o MOC não define entram como o número, com aviso.
/// </para>
/// </remarks>
internal sealed class OperacaoConsultaCadastro : IOperacao<ConsultaCadastro, RetornoCadastro>
{
    private const string Versao = "2.00";
    private const string Raiz = "retConsCad";
    private const string Informacao = "retConsCad/infCons";

    private static readonly HashSet<string> Grupos = ["infCad"];
    private static readonly HashSet<string> GruposContribuinte = ["ender"];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    public PedidoWS Montar(ConsultaCadastro parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        if (parametro.Destino is null && !AutorizadoresNFe400.TemCadastro(parametro.UF))
        {
            problemas.Add(new Problema(CodigosEscrita.Cadastro, Severidade.Erro,
                $"O serviço de cadastro não existe para a UF {parametro.UF.Sigla()}: só atendem AM, BA, GO, MG, MS, MT, PE, PR, RS e SP, e a SVRS, AC, ES, RN, PB e SC. Informe o Destino para escolher o autorizador.")
            {
                Tag = "UF",
                Caminho = "ConsCad/infCons/UF",
                Propriedade = nameof(ConsultaCadastro.UF),
            });
        }

        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("ConsCad", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", Versao);
            xml.WriteStartElement("infCons", EscritaNFe.Namespace);
            xml.WriteElementString("xServ", EscritaNFe.Namespace, "CONS-CAD");
            xml.WriteElementString("UF", EscritaNFe.Namespace, parametro.UF.Sigla());
            if (parametro.InscricaoEstadual is { } inscricao)
                xml.WriteElementString("IE", EscritaNFe.Namespace, inscricao);
            else
                xml.WriteElementString(parametro.Documento!.Length == Cnpj.Tamanho ? "CNPJ" : "CPF", EscritaNFe.Namespace, parametro.Documento);
            xml.WriteEndElement();
            xml.WriteEndElement();
        }
        return new PedidoWS(AutorizadoresNFe400.ConsultaCadastro, texto.ToString(), SchemasNFe400.ConsultaCadastro)
        {
            Destino = parametro.UF.ComoDestino(),
            Emissao = TipoEmissao.Normal,
            Problemas = problemas,
        };
    }

    public LeituraWS<RetornoCadastro> Ler(ConsultaCadastro parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var informacao = raiz.Element(XName.Get("infCons", EscritaNFe.Namespace))
            ?? throw new FormatException("A resposta não tem o infCons.");
        var problemas = new List<Problema>();
        foreach (var filho in raiz.Elements().Where(e => e != informacao))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Desconhecido, Raiz, filho.Name.LocalName,
                $"O campo {filho.Name.LocalName} do retorno não é conhecido do leiaute: ignorado."));

        var campos = LeituraRetorno.Campos(informacao, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        var contribuintes = new List<ContribuinteCadastro>();
        var k = 0;
        foreach (var inf in informacao.Elements(XName.Get("infCad", EscritaNFe.Namespace)))
            contribuintes.Add(LerContribuinte(inf, string.Create(CultureInfo.InvariantCulture, $"{Informacao}/infCad[{++k}]"), problemas));

        var retorno = new RetornoCadastro(status, motivo, contribuintes.AsReadOnly())
        {
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = LeituraRetorno.Campo(campos, Informacao, "UF", problemas, "não é a sigla de uma UF", LerSigla),
            UFAtendimento = LeituraRetorno.Campo(campos, Informacao, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            Consulta = LeituraRetorno.Campo(campos, Informacao, "dhCons", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
        };
        return new LeituraWS<RetornoCadastro>(retorno, status, motivo) { Problemas = problemas };
    }

    private static bool Conhecido(string tag) =>
        tag is "verAplic" or "cStat" or "xMotivo" or "UF" or "IE" or "CNPJ" or "CPF" or "dhCons" or "cUF";

    private static bool ConhecidoContribuinte(string tag) =>
        tag is "IE" or "CNPJ" or "CPF" or "UF" or "cSit" or "indCredNFe" or "indCredCTe" or "xNome" or "xFant" or "xRegApur"
            or "CNAE" or "dIniAtiv" or "dUltSit" or "dBaixa" or "IEUnica" or "IEAtual";

    private static bool ConhecidoEndereco(string tag) =>
        tag is "xLgr" or "nro" or "xCpl" or "xBairro" or "cMun" or "xMun" or "CEP";

    private static ContribuinteCadastro LerContribuinte(XElement inf, string caminho, List<Problema> problemas)
    {
        var campos = Campos(inf, ConhecidoContribuinte, GruposContribuinte, caminho, problemas);
        if (!campos.ContainsKey("IE"))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, caminho, "IE", "O campo IE do contribuinte não veio: fica vazio."));
        if (!campos.ContainsKey("xNome"))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, caminho, "xNome", "O campo xNome do contribuinte não veio: fica vazio."));
        return new ContribuinteCadastro(campos.GetValueOrDefault("IE") ?? "", campos.GetValueOrDefault("xNome") ?? "")
        {
            Cnpj = campos.GetValueOrDefault("CNPJ"),
            Cpf = campos.GetValueOrDefault("CPF"),
            UF = LeituraRetorno.Campo(campos, caminho, "UF", problemas, "não é a sigla de uma UF", LerSigla),
            Situacao = Enumeracao<SituacaoCadastral>(campos, caminho, "cSit", problemas),
            CredenciamentoNFe = Enumeracao<CredenciamentoDFe>(campos, caminho, "indCredNFe", problemas),
            CredenciamentoCTe = Enumeracao<CredenciamentoDFe>(campos, caminho, "indCredCTe", problemas),
            Fantasia = campos.GetValueOrDefault("xFant"),
            RegimeApuracao = campos.GetValueOrDefault("xRegApur"),
            Cnae = campos.GetValueOrDefault("CNAE"),
            InicioAtividade = LeituraRetorno.Campo(campos, caminho, "dIniAtiv", problemas, "não é uma data", LerData),
            UltimaSituacao = LeituraRetorno.Campo(campos, caminho, "dUltSit", problemas, "não é uma data", LerData),
            Baixa = LeituraRetorno.Campo(campos, caminho, "dBaixa", problemas, "não é uma data", LerData),
            InscricaoUnica = campos.GetValueOrDefault("IEUnica"),
            InscricaoAtual = campos.GetValueOrDefault("IEAtual"),
            Endereco = LerEndereco(inf, caminho, problemas),
        };
    }

    private static EnderecoCadastro? LerEndereco(XElement inf, string caminho, List<Problema> problemas)
    {
        var endereco = inf.Elements(XName.Get("ender", EscritaNFe.Namespace)).FirstOrDefault();
        if (endereco is null)
            return null;
        var dados = Campos(endereco, ConhecidoEndereco, new HashSet<string>(), caminho + "/ender", problemas);
        return new EnderecoCadastro
        {
            Logradouro = dados.GetValueOrDefault("xLgr"),
            Numero = dados.GetValueOrDefault("nro"),
            Complemento = dados.GetValueOrDefault("xCpl"),
            Bairro = dados.GetValueOrDefault("xBairro"),
            CodigoMunicipio = dados.GetValueOrDefault("cMun"),
            Municipio = dados.GetValueOrDefault("xMun"),
            Cep = dados.GetValueOrDefault("CEP"),
        };
    }

    // Os campos simples de um grupo repetido, como LeituraRetorno.Campos, mas com o caminho do grupo (com o índice) nos avisos.
    private static Dictionary<string, string> Campos(XElement grupo, Func<string, bool> conhecido, IReadOnlySet<string> grupos, string caminho,
        List<Problema> problemas)
    {
        var locais = new List<Problema>();
        var campos = LeituraRetorno.Campos(grupo, conhecido, grupos, locais);
        foreach (var problema in locais)
            problemas.Add(new Problema(problema.Codigo, problema.Severidade, problema.Mensagem)
            {
                Tag = problema.Tag,
                Caminho = caminho + "/" + problema.Tag,
            });
        return campos;
    }

    // Uma enumeração aberta: o número que o MOC define; o que não define entra como o número, com aviso; o que não é número, nulo.
    private static T? Enumeracao<T>(Dictionary<string, string> campos, string caminho, string tag, List<Problema> problemas)
        where T : struct, Enum
    {
        if (!campos.TryGetValue(tag, out var texto))
            return null;
        if (texto.Length is < 1 or > 9 || !int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
        {
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, caminho, tag, $"O campo {tag} do retorno não é um número: fica nulo."));
            return null;
        }
        var valor = (T)Enum.ToObject(typeof(T), numero);
        if (!Enum.IsDefined(valor))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, caminho, tag,
                $"O campo {tag} do retorno tem o valor {numero}, que o MOC não define: fica como o número."));
        return valor;
    }

    private static UF? LerSigla(string texto) => UnidadesFederativas.TentarLerSigla(texto, out var uf) ? uf : null;

    private static DateOnly? LerData(string texto) =>
        DateOnly.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data) ? data : null;
}
