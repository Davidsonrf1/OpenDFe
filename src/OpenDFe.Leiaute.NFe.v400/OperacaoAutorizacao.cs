using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// A autorização (<c>NFeAutorizacao4</c>, docs/CONTRATOS.md 14): o <c>enviNFe</c> com as notas e o <c>retEnviNFe</c>
/// com o recibo (103) ou o protocolo da síncrona. Só a NF-e nesta versão: a NFC-e entra com o QR Code (decisão 12).
/// </summary>
/// <remarks>
/// <para>
/// A montagem: o <c>idLote</c> (o do parâmetro ou o do relógio), o <c>indSinc</c> pela quantidade (1 com uma nota, 0 com
/// mais) e as notas na ordem, as do modelo sem a assinatura (o núcleo assina cada <c>Id</c>) e as assinadas com os
/// mesmos bytes. Cada nota é um <see cref="DocumentoPedido"/> com o schema da nota (225), a conversão do caminho e o
/// prefixo da propriedade, e o lote vai ao autorizador da chave quando as notas vêm assinadas.
/// </para>
/// <para>
/// A leitura é branda (<see cref="LeituraRetorno"/>); cada <c>protNFe</c> é ligado à nota pela chave, e a nota com o
/// protocolo de autorização ou de denegação ganha o <c>nfeProc</c>, com os bytes da mensagem enviada.
/// </para>
/// </remarks>
internal sealed class OperacaoAutorizacao : IOperacao<Autorizacao, RetornoAutorizacao>
{
    private const string Raiz = "retEnviNFe";

    private static readonly HashSet<string> Grupos = ["infRec", "protNFe"];

    /// <summary>Os status do lote que trazem o resultado das notas (recebido, processado, em processamento).</summary>
    internal static readonly int[] StatusLote = [103, 104, 105];

    public PedidoWS Montar(Autorizacao parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var notas = LoteNFe.Montar(parametro, contexto, problemas);
        var lote = parametro.Lote ?? Lote(contexto.Configuracao.Relogio);
        var xml = new StringBuilder()
            .Append(CultureInfo.InvariantCulture, $"<enviNFe xmlns=\"{EscritaNFe.Namespace}\" versao=\"4.00\">")
            .Append(CultureInfo.InvariantCulture, $"<idLote>{lote}</idLote>")
            .Append(notas.Count == 1 ? "<indSinc>1</indSinc>" : "<indSinc>0</indSinc>");
        foreach (var nota in notas)
            xml.Append(nota.Xml);
        xml.Append("</enviNFe>");

        var assinadas = parametro.Xmls.Count > 0;
        var primeira = assinadas ? notas.Select(n => n.Chave).FirstOrDefault(c => c is not null) : null;
        // A nota repetida (NFE-LOTE, já um erro da montagem) entra uma vez só como documento.
        var documentos = notas.Where(n => n.Chave is not null).DistinctBy(n => n.Chave).ToList();
        return new PedidoWS(AutorizadoresNFe400.Autorizacao, xml.ToString(), SchemasNFe400.Lote)
        {
            IdsAssinatura = assinadas ? [] : [.. documentos.Select(n => "NFe" + n.Chave)],
            Documentos = [.. documentos.Select(n => new DocumentoPedido("NFe" + n.Chave)
            {
                Modelo = n.Modelo,
                Chave = n.Chave,
                Schema = SchemasNFe400.Nota,
                Propriedade = CaminhoModeloNFe.Propriedade,
                PrefixoPropriedade = n.Prefixo,
            })],
            Destino = primeira?.UF.ComoDestino(),
            Emissao = primeira?.TipoEmissao,
            Problemas = problemas,
        };
    }

    public LeituraWS<RetornoAutorizacao> Ler(Autorizacao parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        string? recibo = null;
        TimeSpan? tempoMedio = null;
        var recebimento = raiz.Elements(XName.Get("infRec", EscritaNFe.Namespace)).ToList();
        if (recebimento.Count > 0)
        {
            var caminho = Raiz + "/infRec";
            if (recebimento.Count > 1)
                problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Repetido, Raiz, "infRec", "O grupo infRec do retorno veio repetido: vale o primeiro."));
            var camposRecibo = LeituraRetorno.Campos(recebimento[0], t => t is "nRec" or "tMed", new HashSet<string>(), problemas);
            recibo = camposRecibo.GetValueOrDefault("nRec");
            tempoMedio = LeituraRetorno.Campo(camposRecibo, caminho, "tMed", problemas, "não é um número de segundos", LeituraRetorno.LerSegundos);
        }

        var temProtocolo = raiz.Elements(XName.Get("protNFe", EscritaNFe.Namespace)).Any();
        List<ResultadoNota> notas = [];
        if (temProtocolo || StatusLote.Contains(status))
        {
            var enviadas = Enviadas(parametro, xmlPedido);
            notas = LoteNFe.Resultados(raiz, enviadas, avisarFalta: status == 104, problemas);
        }

        var retorno = new RetornoAutorizacao(status, motivo, notas.AsReadOnly())
        {
            Ambiente = LeituraRetorno.Campo(campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = LeituraRetorno.Campo(campos, Raiz, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            Recebimento = LeituraRetorno.Campo(campos, Raiz, "dhRecbto", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            Recibo = recibo,
            TempoMedio = tempoMedio,
        };
        return new LeituraWS<RetornoAutorizacao>(retorno, status, motivo) { Problemas = problemas };
    }

    // As notas como foram: as assinadas do parâmetro, como entraram no lote; as do modelo, tiradas da mensagem assinada.
    private static List<(string Xml, ChaveAcesso? Chave)> Enviadas(Autorizacao parametro, string xmlPedido)
    {
        var xmls = parametro.Xmls.Count > 0
            ? parametro.Xmls.Select(LoteNFe.Normalizar).ToList()
            : LoteNFe.Enviadas(xmlPedido);
        return [.. xmls.Select(x => (x, LoteNFe.Chave(x)))];
    }

    // O idLote do relógio, em UTC: AAMMDDhhmmssmmm (15 dígitos).
    private static long Lote(TimeProvider relogio) =>
        long.Parse(relogio.GetUtcNow().ToString("yyMMddHHmmssfff", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "cUF" or "dhRecbto";
}
