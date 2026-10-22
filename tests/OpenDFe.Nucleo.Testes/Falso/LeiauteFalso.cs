using System.Globalization;
using System.Security;
using System.Xml.Linq;

namespace OpenDFe.Testes;

/// <summary>
/// Um leiaute falso, só para os testes dos contratos (CONTRATOS.md 10): duas operações (o status e um eco
/// assinado), o envelope SOAP dos dois, os atendimentos de MG e do AC, as URLs de quatro autorizadores nos dois
/// ambientes e, na NFC-e, uma configuração do documento obrigatória. As mensagens seguem o
/// <c>Schemas/Falso/eco_v1.00.xsd</c>.
/// </summary>
internal sealed class LeiauteFalso : ILeiaute
{
    public const string Namespace = "urn:opendfe:teste:eco";
    public const string ServicoStatus = "StatusFalso";
    public const string ServicoEco = "EcoFalso";
    public const string CodigoSchema = "215";
    public const string NamespaceWsdl = "urn:opendfe:teste:wsdl";

    public static readonly ServicoSoap SoapStatus = new(ServicoStatus, NamespaceWsdl, NamespaceWsdl + "/status");
    public static readonly ServicoSoap SoapEco = new(ServicoEco, NamespaceWsdl, NamespaceWsdl + "/eco");

    public static readonly SchemaXml Schema =
        SchemaXml.Carregar(typeof(LeiauteFalso).Assembly, "Schemas/Falso/eco_v1.00.xsd", CodigoSchema);

    public static readonly Destino[] Autorizadores = [Destino.MG, Destino.SVRS, Destino.SvcAN, Destino.AN];

    public string Versao => "1.00";

    public IReadOnlyList<Documento> Documentos { get; init; } = [Documento.NFe, Documento.NFCe];

    /// <summary>Uma declaração a mais, depois da padrão (para as recusas da declaração).</summary>
    public Action<DeclaracaoLeiaute>? Acrescimo { get; init; }

    /// <summary>A última declaração recebida (para a recusa do registro depois do Declarar).</summary>
    public DeclaracaoLeiaute? Ultima { get; private set; }

    public static Uri Url(Destino autorizador, Ambiente ambiente, string servico) =>
        new($"https://{autorizador.Sigla().ToLowerInvariant()}.{(ambiente == Ambiente.Homologacao ? "hom" : "prod")}.exemplo/{servico}");

    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        Ultima = declaracao;
        declaracao.RegistrarOperacao(new OperacaoStatusFalsa());
        declaracao.RegistrarOperacao(new OperacaoEcoFalsa());
        declaracao.RegistrarPreparo(new PreparoEcoFalso());
        if (declaracao.Documento == Documento.NFCe)
            declaracao.DeclararConfiguracaoDocumento<ConfiguracaoFalsa>(obrigatoria: true);

        declaracao.DeclararServico(SoapStatus);
        declaracao.DeclararServico(SoapEco);
        declaracao.DeclararAtendimento(UF.MG, TipoEmissao.Normal, Destino.MG);
        declaracao.DeclararAtendimento(UF.MG, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN);
        declaracao.DeclararAtendimento(UF.AC, TipoEmissao.Normal, Destino.SVRS);
        declaracao.DeclararAtendimento(UF.AC, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN);
        foreach (var autorizador in Autorizadores)
        {
            foreach (var ambiente in (Ambiente[])[Ambiente.Homologacao, Ambiente.Producao])
            {
                declaracao.DeclararEndereco(new EnderecoServico(autorizador, ambiente, ServicoStatus, Url(autorizador, ambiente, ServicoStatus)));
                declaracao.DeclararEndereco(new EnderecoServico(autorizador, ambiente, ServicoEco, Url(autorizador, ambiente, ServicoEco)));
            }
        }
        Acrescimo?.Invoke(declaracao);
    }
}

internal sealed class ConfiguracaoFalsa : ConfiguracaoDocumento
{
    public string Csc { get; init; } = "CSC-TESTE";
}

internal sealed class OutraConfiguracaoFalsa : ConfiguracaoDocumento
{
}

public sealed class RetornoStatusFalso
{
    public required string Host { get; init; }
}

public sealed class RetornoEcoFalso
{
    public required IReadOnlyList<string> Textos { get; init; }
}

/// <summary>O status falso: sem assinatura.</summary>
public sealed class StatusFalso : ParametroWS<RetornoStatusFalso>
{
}

/// <summary>Uma operação que o leiaute falso não registra.</summary>
public sealed class NaoRegistradoFalso : ParametroWS<RetornoStatusFalso>
{
}

/// <summary>Um parâmetro abstrato, que o registro recusa.</summary>
public abstract class AbstratoFalso : ParametroWS<RetornoStatusFalso>
{
}

/// <summary>O eco falso: cada texto vira um <c>eco</c> assinado; os outros membros mudam a montagem.</summary>
public sealed class EcoFalso : ParametroWS<RetornoEcoFalso>
{
    public IReadOnlyList<string> Textos { get; init; } = ["ola"];

    public bool Assinar { get; init; } = true;

    public Problema? ProblemaMontagem { get; init; }

    public bool MensagemVazia { get; init; }

    public Exception? ExcecaoMontagem { get; init; }

    public Destino? DestinoPedido { get; init; }

    public TipoEmissao? EmissaoPedido { get; init; }

    /// <summary>
    /// Um documento por eco (o <c>eco</c>, pai do <c>infEco</c> assinado), com o texto como modelo e a conversão do
    /// caminho do <see cref="OperacaoEcoFalsa.Propriedade"/>.
    /// </summary>
    public bool MontarDocumentos { get; init; }

    /// <summary>Com <see cref="MontarDocumentos"/>, cada documento declara o schema do eco sozinho (4.4).</summary>
    public bool SchemaDocumentos { get; init; }

    /// <summary>Com <see cref="MontarDocumentos"/>, o prefixo da propriedade de cada documento: <c>Ecos[i].</c> (4.4).</summary>
    public bool PrefixarDocumentos { get; init; }

    /// <summary>Os documentos do pedido como estão (para as recusas); sobrepõe <see cref="MontarDocumentos"/>.</summary>
    public IReadOnlyList<DocumentoPedido>? DocumentosPedido { get; init; }

    /// <summary>O contexto recebido na montagem.</summary>
    public ContextoOperacao? ContextoRecebido { get; set; }
}

internal sealed class OperacaoStatusFalsa : IOperacao<StatusFalso, RetornoStatusFalso>
{
    public PedidoWS Montar(StatusFalso parametro, ContextoOperacao contexto) =>
        new(LeiauteFalso.ServicoStatus,
            $"<consStatus xmlns=\"{LeiauteFalso.Namespace}\" versao=\"1.00\"><tpAmb>{(int)contexto.Configuracao.Ambiente}</tpAmb></consStatus>",
            LeiauteFalso.Schema);

    public LeituraWS<RetornoStatusFalso> Ler(StatusFalso parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = XDocument.Parse(xmlRetorno).Root!;
        var retorno = new RetornoStatusFalso { Host = (string?)raiz.Element("host") ?? "" };
        return new LeituraWS<RetornoStatusFalso>(retorno, LerCStat(raiz), (string?)raiz.Element("xMotivo") ?? "");
    }

    internal static int LerCStat(XElement raiz) =>
        int.Parse((string?)raiz.Element("cStat") ?? throw new FormatException("A resposta não tem cStat."), CultureInfo.InvariantCulture);
}

internal sealed class OperacaoAbstrataFalsa : IOperacao<AbstratoFalso, RetornoStatusFalso>
{
    public PedidoWS Montar(AbstratoFalso parametro, ContextoOperacao contexto) => throw new NotSupportedException();

    public LeituraWS<RetornoStatusFalso> Ler(AbstratoFalso parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto) =>
        throw new NotSupportedException();
}

internal sealed class OperacaoEcoFalsa : IOperacao<EcoFalso, RetornoEcoFalso>
{
    public static string Id(int indice) => "ECO" + (indice + 1).ToString(CultureInfo.InvariantCulture);

    public PedidoWS Montar(EcoFalso parametro, ContextoOperacao contexto)
    {
        parametro.ContextoRecebido = contexto;
        if (parametro.ExcecaoMontagem is not null)
            throw parametro.ExcecaoMontagem;

        var xml = parametro.MensagemVazia
            ? ""
            : $"<ecoLote xmlns=\"{LeiauteFalso.Namespace}\">"
              + string.Concat(parametro.Textos.Select((t, i) => $"<eco><infEco Id=\"{Id(i)}\"><texto>{SecurityElement.Escape(t)}</texto></infEco></eco>"))
              + "</ecoLote>";
        return new PedidoWS(LeiauteFalso.ServicoEco, xml, LeiauteFalso.Schema)
        {
            IdsAssinatura = parametro.Assinar ? [.. parametro.Textos.Select((_, i) => Id(i))] : [],
            Destino = parametro.DestinoPedido,
            Emissao = parametro.EmissaoPedido,
            Problemas = parametro.ProblemaMontagem is { } problema ? [problema] : [],
            Documentos = parametro.DocumentosPedido
                ?? (parametro.MontarDocumentos
                    ? [.. parametro.Textos.Select((t, i) => new DocumentoPedido(Id(i))
                    {
                        Modelo = t,
                        Propriedade = Propriedade,
                        Schema = parametro.SchemaDocumentos ? LeiauteFalso.Schema : null,
                        PrefixoPropriedade = parametro.PrefixarDocumentos ? $"Ecos[{i}]." : null,
                    })]
                    : []),
        };
    }

    /// <summary>A conversão do caminho do eco na propriedade do "modelo" (só o texto e o Id têm).</summary>
    public static string? Propriedade(string caminho) => caminho switch
    {
        "eco/infEco/texto" => "Texto",
        "eco/infEco/@Id" => "Id",
        _ => null,
    };

    public LeituraWS<RetornoEcoFalso> Ler(EcoFalso parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = XDocument.Parse(xmlRetorno).Root!;
        var retorno = new RetornoEcoFalso { Textos = [.. raiz.Elements("texto").Select(e => e.Value)] };
        var avisos = raiz.Elements("aviso").Select(e => new Problema("L01", Severidade.Aviso, e.Value)).ToArray();
        return new LeituraWS<RetornoEcoFalso>(retorno, OperacaoStatusFalsa.LerCStat(raiz), (string?)raiz.Element("xMotivo") ?? "")
        {
            Problemas = avisos,
        };
    }

    /// <summary>A resposta do autorizador falso ao eco: os textos de volta, na ordem.</summary>
    public static string Responder(string xmlPedido)
    {
        XNamespace ns = LeiauteFalso.Namespace;
        var textos = XDocument.Parse(xmlPedido).Descendants(ns + "texto").Select(e => SecurityElement.Escape(e.Value));
        return "<retEco><cStat>100</cStat><xMotivo>Eco processado</xMotivo>" + string.Concat(textos.Select(t => $"<texto>{t}</texto>")) + "</retEco>";
    }
}

/// <summary>O documento do preparo falso: um eco sozinho, com o texto.</summary>
public sealed class TextoFalso
{
    public string Texto { get; init; } = "ola";

    public Problema? ProblemaMontagem { get; init; }

    /// <summary>A montagem devolve nulo (erro de programação do leiaute).</summary>
    public bool MontagemNula { get; init; }

    /// <summary>O contexto recebido na montagem.</summary>
    public ContextoOperacao? ContextoRecebido { get; set; }
}

/// <summary>Um documento que o leiaute falso não sabe preparar.</summary>
public sealed class NaoPreparadoFalso
{
}

/// <summary>O preparo falso: o eco sozinho, com o Id ECO1, a chave de teste e a conversão do eco.</summary>
internal sealed class PreparoEcoFalso : IPreparo<TextoFalso>
{
    public static readonly ChaveAcesso Chave = ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.FromHours(-3)),
        "11222333000181", Documento.NFe, 1, 1, TipoEmissao.Normal, 12345678);

    public DocumentoMontado Montar(TextoFalso documento, ContextoOperacao contexto)
    {
        documento.ContextoRecebido = contexto;
        if (documento.MontagemNula)
            return null!;
        var xml = documento.ProblemaMontagem is { Severidade: Severidade.Erro }
            ? ""
            : $"<eco xmlns=\"{LeiauteFalso.Namespace}\"><infEco Id=\"ECO1\"><texto>{SecurityElement.Escape(documento.Texto)}</texto></infEco></eco>";
        return new DocumentoMontado(xml, LeiauteFalso.Schema, new DocumentoPedido("ECO1")
        {
            Modelo = documento.Texto,
            Chave = Chave,
            Propriedade = OperacaoEcoFalsa.Propriedade,
            Schema = LeiauteFalso.Schema,
        })
        {
            Problemas = documento.ProblemaMontagem is { } problema ? [problema] : [],
        };
    }
}
