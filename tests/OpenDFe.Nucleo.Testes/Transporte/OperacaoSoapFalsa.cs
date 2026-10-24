using System.Xml.Linq;

namespace OpenDFe.Testes;

/// <summary>O status falso de um serviço com a operação no <c>Body</c> (<see cref="ServicoSoap.Operacao"/>).</summary>
public sealed class StatusOperacaoFalso : ParametroWS<RetornoStatusFalso>
{
}

/// <summary>
/// O serviço falso com a operação no <c>Body</c>, que o <see cref="LeiauteFalso.Acrescimo"/> declara: o mesmo status do leiaute
/// falso, sob o serviço <see cref="Servico"/>.
/// </summary>
internal sealed class OperacaoEnvelopeFalsa : IOperacao<StatusOperacaoFalso, RetornoStatusFalso>
{
    public const string Servico = "OperacaoFalsa";
    public const string Operacao = "nfeOperacaoFalsa";

    public static readonly ServicoSoap Soap = new(Servico, LeiauteFalso.NamespaceWsdl, LeiauteFalso.NamespaceWsdl + "/operacao") { Operacao = Operacao };

    /// <summary>A declaração a mais do leiaute falso: o serviço, a URL no AN e a operação.</summary>
    public static void Declarar(DeclaracaoLeiaute declaracao)
    {
        declaracao.RegistrarOperacao(new OperacaoEnvelopeFalsa());
        declaracao.DeclararServico(Soap);
        foreach (var ambiente in (Ambiente[])[Ambiente.Homologacao, Ambiente.Producao])
            declaracao.DeclararEndereco(new EnderecoServico(Destino.AN, ambiente, Servico, LeiauteFalso.Url(Destino.AN, ambiente, Servico)));
    }

    /// <summary>O envelope da resposta com <c>&lt;Operacao&gt;Response/&lt;Operacao&gt;Result</c>.</summary>
    public static string Envelope(string mensagem) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
        + $"<r:{Operacao}Response xmlns:r=\"{LeiauteFalso.NamespaceWsdl}\"><r:{Operacao}Result>{mensagem}</r:{Operacao}Result></r:{Operacao}Response>"
        + "</soap:Body></soap:Envelope>";

    public PedidoWS Montar(StatusOperacaoFalso parametro, ContextoOperacao contexto) =>
        new(Servico,
            $"<consStatus xmlns=\"{LeiauteFalso.Namespace}\" versao=\"1.00\"><tpAmb>{(int)contexto.Configuracao.Ambiente}</tpAmb></consStatus>",
            LeiauteFalso.Schema)
        {
            Destino = Destino.AN,
        };

    public LeituraWS<RetornoStatusFalso> Ler(StatusOperacaoFalso parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = XDocument.Parse(xmlRetorno).Root!;
        return new LeituraWS<RetornoStatusFalso>(new RetornoStatusFalso { Host = (string?)raiz.Element("host") ?? "" },
            OperacaoStatusFalsa.LerCStat(raiz), (string?)raiz.Element("xMotivo") ?? "");
    }
}
