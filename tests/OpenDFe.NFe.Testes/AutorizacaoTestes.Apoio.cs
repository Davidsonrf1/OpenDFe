using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O apoio dos testes da autorização: as notas, o núcleo com o relógio fixo, o handler que responde conforme o pedido e
// os retornos montados à mão (na ordem do leiaute do retEnviNFe e do retConsReciNFe).
public partial class AutorizacaoTestes
{
    private const string CodigoLote = "NFE-LOTE";
    private const string Recebimento = "2026-10-20T15:30:01-03:00";

    private sealed record Envio(Uri Url, string Mensagem);

    // A NF-e de teste com o número e o cNF fixos (a mesma nota dá a mesma chave).
    private static NotaFiscal Nota(int numero = 1)
    {
        var nota = NotasTeste.NFe();
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        return nota;
    }

    // A nota escrita e assinada fora do núcleo, numa UF e num tipo de emissão.
    private static string Assinada(NotaFiscal nota, TipoEmissao emissao = TipoEmissao.Normal, UF uf = UF.MG) =>
        NotasTeste.Assinar(EscritorNFe.Escrever(nota, new ValoresNucleo
        {
            Documento = Documento.NFe,
            UF = uf,
            Ambiente = Ambiente.Homologacao,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(NotasTeste.Emissao, "Teste da autorizacao em contingencia"),
        }));

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes) => new()
    {
        Ambiente = Ambiente.Homologacao,
        UF = UF.MG,
        Certificado = Certificado.Value,
        Conexoes = conexoes,
        Relogio = new RelogioFixo(Agora),
    };

    private static async Task<(ResultadoWS<TRetorno> Resultado, Envio? Envio)> Executar<TRetorno>(ParametroWS<TRetorno> parametro,
        Func<string, string> responder, Documento documento = Documento.NFe)
        where TRetorno : class
    {
        var envios = new List<Envio>();
        using var conexoes = new ConexoesSefaz(_ => new HandlerResposta(responder, envios));
        var dfe = DFe.Criar(new LeiauteNFe400(), documento, Configuracao(conexoes));

        var resultado = await dfe.ExecutarAsync(parametro, Ct);

        return (resultado, envios.SingleOrDefault());
    }

    // A resposta síncrona: o lote processado (104) com o protocolo da nota do pedido, no status do caso.
    private static Func<string, string> Sincrona(int status) => pedido =>
    {
        var nota = Enviadas(pedido)[0];
        return Retorno(104, "Lote processado", Protocolo(Chave(nota), Digest(nota), status));
    };

    // O lote recebido (103), com o recibo.
    private static string Recebido(string pedido) =>
        Retorno(103, "Lote recebido com sucesso", "<infRec><nRec>311000000000001</nRec><tMed>3</tMed></infRec>");

    private static string Retorno(int status, string motivo, string resto = "") =>
        $"<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic>"
        + $"<cStat>{status}</cStat><xMotivo>{motivo}</xMotivo><cUF>31</cUF><dhRecbto>{Recebimento}</dhRecbto>{resto}</retEnviNFe>";

    private static string RetornoRecibo(int status, string motivo, params string[] resto) =>
        $"<retConsReciNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic>"
        + $"<nRec>311000000000001</nRec><cStat>{status}</cStat><xMotivo>{motivo}</xMotivo><cUF>31</cUF><dhRecbto>{Recebimento}</dhRecbto>"
        + string.Concat(resto.Where(r => r.StartsWith("<cMsg", StringComparison.Ordinal)))
        + string.Concat(resto.Where(r => !r.StartsWith("<cMsg", StringComparison.Ordinal)))
        + "</retConsReciNFe>";

    private static string Protocolo(string chave, string digest, int status, string? motivo = null)
    {
        var autorizada = status is 100 or 150 or 110 or 301 or 302;
        return "<protNFe versao=\"4.00\"><infProt><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic>"
            + $"<chNFe>{chave}</chNFe><dhRecbto>{Recebimento}</dhRecbto>"
            + (autorizada ? "<nProt>131260000000001</nProt>" : "")
            + (digest.Length > 0 ? $"<digVal>{digest}</digVal>" : "")
            + $"<cStat>{status}</cStat><xMotivo>{motivo ?? "Motivo " + status.ToString(CultureInfo.InvariantCulture)}</xMotivo></infProt></protNFe>";
    }

    // A chave e o DigestValue da primeira nota de um XML.
    private static string Chave(string xml) => Regex.Match(xml, "Id=\"NFe([0-9]{44})\"").Groups[1].Value;

    private static string Digest(string xml) => Regex.Match(xml, "<DigestValue>([^<]+)</DigestValue>").Groups[1].Value;

    // As notas do enviNFe, com os bytes da mensagem.
    private static List<string> Enviadas(string mensagem) =>
        [.. Regex.Matches(mensagem, "<NFe .*?</NFe>", RegexOptions.Singleline).Select(m => m.Value)];

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora.ToUniversalTime();
    }

    // O handler que guarda a URL e a mensagem (o conteúdo do nfeDadosMsg) e responde dentro do nfeResultMsg.
    private sealed class HandlerResposta(Func<string, string> responder, List<Envio> envios) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // O núcleo põe a mensagem no envelope como veio: os bytes dela estão no corpo, dentro do nfeDadosMsg.
            var corpo = await request.Content!.ReadAsStringAsync(cancellationToken);
            var inicio = corpo.IndexOf('>', corpo.IndexOf("<nfeDadosMsg", StringComparison.Ordinal)) + 1;
            var mensagem = corpo[inicio..corpo.LastIndexOf("</nfeDadosMsg>", StringComparison.Ordinal)];
            lock (envios)
                envios.Add(new Envio(request.RequestUri!, mensagem));
            var envelope = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
                + "<nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4\">" + responder(mensagem)
                + "</nfeResultMsg></soap:Body></soap:Envelope>";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(envelope, Encoding.UTF8, "application/soap+xml"),
            };
        }
    }
}
