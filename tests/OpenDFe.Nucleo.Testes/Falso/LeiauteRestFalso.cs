using System.Security;
using System.Text.Json;

namespace OpenDFe.Testes;

/// <summary>
/// Um leiaute REST falso (a 9.2; docs/NFSE.md, D4), para a NFS-e: uma operação que manda o eco assinado num POST ou consulta
/// num GET ou HEAD, sem atendimento declarado, com a base <c>BaseFalsa</c> na Sefin Nacional dos dois ambientes.
/// </summary>
internal sealed class LeiauteRestFalso : ILeiaute
{
    public const string Servico = "BaseFalsa";
    public const string Campo = "ecoXmlGZipB64";

    public string Versao => "1.00";

    public IReadOnlyList<Documento> Documentos => [Documento.NFSe];

    /// <summary>Uma declaração a mais, depois da padrão.</summary>
    public Action<DeclaracaoLeiaute>? Acrescimo { get; init; }

    public static Uri Url(Ambiente ambiente) =>
        new($"https://sefin.{(ambiente == Ambiente.Homologacao ? "hom" : "prod")}.exemplo/Base");

    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        declaracao.RegistrarOperacao(new OperacaoRestFalsa());
        foreach (var ambiente in (Ambiente[])[Ambiente.Homologacao, Ambiente.Producao])
            declaracao.DeclararEndereco(new EnderecoServico(Destino.SefinNacional, ambiente, Servico, Url(ambiente)));
        Acrescimo?.Invoke(declaracao);
    }
}

public sealed class RetornoRestFalso
{
    public required int Status { get; init; }

    public required string? Corpo { get; init; }

    public required string? XmlPedido { get; init; }
}

/// <summary>O pedido REST falso: o eco num POST (por padrão), ou a consulta sem corpo; os outros membros mudam a montagem.</summary>
public sealed class RestFalso : ParametroWS<RetornoRestFalso>
{
    public MetodoRest Metodo { get; init; } = MetodoRest.Post;

    public string Texto { get; init; } = "ola";

    public string Caminho { get; init; } = "eco";

    /// <summary>O XML como está, no lugar do eco (para a recusa do schema).</summary>
    public string? XmlBruto { get; init; }

    /// <summary>O XML mesmo no GET e no HEAD, ou nenhum no POST (para a coerência).</summary>
    public bool? IncluirXml { get; init; }

    public bool OmitirSchema { get; init; }

    public bool OmitirCampo { get; init; }

    public bool OmitirDestino { get; init; }

    public Problema? ProblemaMontagem { get; init; }

    /// <summary>O documento do eco, para as regras e os callbacks.</summary>
    public bool MontarDocumento { get; init; }

    /// <summary>A leitura lança esta exceção (a resposta ilegível ou o erro de programação).</summary>
    public Exception? ExcecaoLeitura { get; init; }
}

internal sealed class OperacaoRestFalsa : IOperacaoRest<RestFalso, RetornoRestFalso>
{
    public PedidoRest Montar(RestFalso parametro, ContextoOperacao contexto)
    {
        var comXml = parametro.IncluirXml ?? parametro.Metodo == MetodoRest.Post;
        var xml = parametro.XmlBruto
            ?? $"<eco xmlns=\"{LeiauteFalso.Namespace}\"><infEco Id=\"ECO1\"><texto>{SecurityElement.Escape(parametro.Texto)}</texto></infEco></eco>";
        return new PedidoRest(LeiauteRestFalso.Servico, parametro.Metodo, parametro.Caminho)
        {
            Xml = comXml ? xml : null,
            Schema = comXml && !parametro.OmitirSchema ? LeiauteFalso.Schema : null,
            CampoXml = comXml && !parametro.OmitirCampo ? LeiauteRestFalso.Campo : null,
            IdsAssinatura = comXml && parametro.XmlBruto is null ? ["ECO1"] : [],
            Destino = parametro.OmitirDestino ? null : Destino.SefinNacional,
            Problemas = parametro.ProblemaMontagem is { } problema ? [problema] : [],
            Documentos = parametro.MontarDocumento
                ? [new DocumentoPedido("ECO1") { Modelo = parametro.Texto, Propriedade = OperacaoEcoFalsa.Propriedade }]
                : [],
        };
    }

    public LeituraWS<RetornoRestFalso> Ler(RestFalso parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        if (parametro.ExcecaoLeitura is not null)
            throw parametro.ExcecaoLeitura;
        if (resposta.Corpo is not null)
            using (JsonDocument.Parse(resposta.Corpo))
            {
            }
        var retorno = new RetornoRestFalso { Status = resposta.Status, Corpo = resposta.Corpo, XmlPedido = xmlPedido };
        return new LeituraWS<RetornoRestFalso>(retorno, resposta.Status, resposta.Status < 300 ? "ok" : "erro");
    }
}
