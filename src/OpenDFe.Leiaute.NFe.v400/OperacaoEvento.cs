using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// O registro de eventos (<c>NFeRecepcaoEvento4</c>, docs/CONTRATOS.md 18): o <c>envEvento</c> com de 1 a 20 eventos,
/// cada um assinado no <c>infEvento</c>, e o <c>retEnvEvento</c>, com o <c>procEventoNFe</c> de cada evento registrado.
/// Vale para a NF-e e para a NFC-e. Não conhece o tipo de evento: tudo o que muda de um tipo a outro está no
/// <see cref="Evento"/>.
/// </summary>
internal sealed class OperacaoEvento : IOperacao<EnvioEvento, RetornoEvento>
{
    private const string Raiz = "retEnvEvento";
    private const string Versao = "1.00";

    private static readonly HashSet<string> Grupos = ["retEvento"];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    public PedidoWS Montar(EnvioEvento parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var eventos = parametro.Eventos;
        for (var k = 0; k < eventos.Count; k++)
            ConferirEvento(eventos[k], k, eventos.Count, contexto, problemas);
        ConferirLote(eventos, contexto, problemas);

        var texto = new StringBuilder();
        var ids = new List<string>();
        if (!problemas.Any(p => p.Severidade == Severidade.Erro))
        {
            var lote = parametro.Lote ?? Lote(contexto.Configuracao.Relogio);
            using var xml = XmlWriter.Create(texto, ConfiguracaoEscrita);
            xml.WriteStartElement("envEvento", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", Versao);
            xml.WriteElementString("idLote", EscritaNFe.Namespace, lote.ToString(CultureInfo.InvariantCulture));
            foreach (var evento in eventos)
            {
                var id = Id(evento);
                ids.Add(id);
                Escrever(xml, evento, id, contexto);
            }
            xml.WriteEndElement();
        }

        var primeiro = eventos[0];
        var documentos = eventos.Select((e, k) => (Evento: e, Indice: k, Id: Id(e))).DistinctBy(e => e.Id)
            .Select(e => new DocumentoPedido(e.Id)
            {
                Modelo = e.Evento,
                Chave = e.Evento.Chave,
                Propriedade = e.Evento.Propriedade,
                PrefixoPropriedade = Passo(e.Indice) + ".",
            });
        return new PedidoWS(AutorizadoresNFe400.RecepcaoEvento, texto.ToString(), SchemasNFe400.Evento)
        {
            IdsAssinatura = ids.Distinct(StringComparer.Ordinal).ToList(),
            Documentos = [.. documentos],
            Destino = primeiro.Destino(contexto),
            Emissao = parametro.Destino is null ? primeiro.EmissaoPedido : TipoEmissao.Normal,
            Problemas = problemas,
        };
    }

    public LeituraWS<RetornoEvento> Ler(EnvioEvento parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        var resultados = new List<ResultadoEvento>();
        if (status == 128)
            resultados = Resultados(parametro, xmlPedido, raiz, problemas);

        var retorno = new RetornoEvento(status, motivo, resultados.AsReadOnly())
        {
            Ambiente = LeituraRetorno.Campo(campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            Orgao = LeituraRetorno.Campo(campos, Raiz, "cOrgao", problemas, "não é um número", LerInteiro),
            Lote = LeituraRetorno.Campo(campos, Raiz, "idLote", problemas, "não é um número", LerLote),
        };
        return new LeituraWS<RetornoEvento>(retorno, status, motivo) { Problemas = problemas };
    }

    // O Id do infEvento: "ID" + tpEvento + chave + nSeqEvento com 2 posições.
    private static string Id(Evento evento) =>
        string.Create(CultureInfo.InvariantCulture, $"ID{(int)evento.Tipo}{evento.Chave}{evento.Sequencia:D2}");

    private static string Passo(int indice) => string.Create(CultureInfo.InvariantCulture, $"Eventos[{indice}]");

    // O caminho do campo do evento k no envEvento (o índice só quando o elemento se repete).
    private static string Caminho(int indice, int quantidade, string resto) =>
        quantidade > 1
            ? string.Create(CultureInfo.InvariantCulture, $"envEvento/evento[{indice + 1}]/infEvento/{resto}")
            : $"envEvento/evento/infEvento/{resto}";

    private static void Escrever(XmlWriter xml, Evento evento, string id, ContextoOperacao contexto)
    {
        var momento = evento.Momento ?? contexto.Configuracao.Relogio.GetUtcNow().ToOffset(TimeSpan.FromHours(-3));
        var autor = evento.Autor;
        xml.WriteStartElement("evento", EscritaNFe.Namespace);
        xml.WriteAttributeString("versao", Versao);
        xml.WriteStartElement("infEvento", EscritaNFe.Namespace);
        xml.WriteAttributeString("Id", id);
        xml.WriteElementString("cOrgao", EscritaNFe.Namespace, evento.Orgao(contexto).ToString(CultureInfo.InvariantCulture));
        xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)contexto.Configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
        xml.WriteElementString(autor.Length == Cpf.Tamanho ? "CPF" : "CNPJ", EscritaNFe.Namespace, autor);
        xml.WriteElementString("chNFe", EscritaNFe.Namespace, evento.Chave.ToString());
        xml.WriteElementString("dhEvento", EscritaNFe.Namespace, FormatadorInvariavel.DataHora(momento));
        xml.WriteElementString("tpEvento", EscritaNFe.Namespace, ((int)evento.Tipo).ToString(CultureInfo.InvariantCulture));
        xml.WriteElementString("nSeqEvento", EscritaNFe.Namespace, evento.Sequencia.ToString(CultureInfo.InvariantCulture));
        xml.WriteElementString("verEvento", EscritaNFe.Namespace, evento.VersaoDetalhe);
        evento.EscreverDetalhe(xml);
        xml.WriteEndElement();
        xml.WriteEndElement();
    }

    // As recusas de um evento: o modelo da chave (NFE-CHAVE), os caracteres do texto (NFE-CARACTERE) e o detEvento no
    // schema do tipo (493), com a propriedade do evento k.
    private static void ConferirEvento(Evento evento, int indice, int quantidade, ContextoOperacao contexto, List<Problema> problemas)
    {
        var prefixo = Passo(indice) + ".";
        SituacaoNFe.ConferirModelo(evento.Chave, contexto.Documento, Caminho(indice, quantidade, "chNFe"), prefixo + nameof(Evento.Chave),
            problemas, "o evento");

        var caracteres = false;
        foreach (var (tag, propriedade, valor) in evento.Textos)
        {
            if (PosicaoInvalida(valor) is not { } posicao)
                continue;
            caracteres = true;
            problemas.Add(new Problema(CodigosEscrita.Caractere, Severidade.Erro,
                string.Create(CultureInfo.InvariantCulture,
                    $"O campo {tag} tem um caractere que o XML não aceita (U+{(int)valor[posicao]:X4}, na posição {posicao + 1})."))
            {
                Tag = tag,
                Caminho = Caminho(indice, quantidade, "detEvento/" + tag),
                Propriedade = prefixo + propriedade,
            });
        }
        if (caracteres)
            return;

        var fragmento = new StringBuilder();
        using (var xml = XmlWriter.Create(fragmento, ConfiguracaoEscrita))
            evento.EscreverDetalhe(xml);
        foreach (var problema in evento.SchemaDetalhe.Validar(fragmento.ToString()))
        {
            problemas.Add(new Problema(problema.Codigo, problema.Severidade, problema.Mensagem)
            {
                Tag = problema.Tag,
                Caminho = problema.Caminho is { } c ? Caminho(indice, quantidade, c) : null,
                Propriedade = problema.Propriedade is { } p ? prefixo + p : prefixo.TrimEnd('.'),
                Schema = problema.Schema,
            });
        }
    }

    // A posição do primeiro caractere que o XML 1.0 não aceita; nula se não há.
    private static int? PosicaoInvalida(string valor)
    {
        for (var i = 0; i < valor.Length; i++)
        {
            var c = valor[i];
            if (XmlConvert.IsXmlChar(c))
                continue;
            if (char.IsHighSurrogate(c) && i + 1 < valor.Length && char.IsLowSurrogate(valor[i + 1]))
            {
                i++;
                continue;
            }
            return i;
        }
        return null;
    }

    // As regras do lote (NFE-LOTE): o mesmo destino e tipo de emissão em todos os eventos, e nenhum evento repetido.
    private static void ConferirLote(IReadOnlyList<Evento> eventos, ContextoOperacao contexto, List<Problema> problemas)
    {
        var vistos = new HashSet<(TipoEvento, ChaveAcesso, int)>();
        var primeiro = eventos[0];
        for (var k = 0; k < eventos.Count; k++)
        {
            var evento = eventos[k];
            var propriedade = Passo(k);
            if (!vistos.Add((evento.Tipo, evento.Chave, evento.Sequencia)))
                problemas.Add(new Problema(CodigosEscrita.Lote, Severidade.Erro,
                    "O mesmo evento (o mesmo tipo, a mesma chave e a mesma sequência) está duas vezes no envio.")
                {
                    Tag = "infEvento",
                    Caminho = Caminho(k, eventos.Count, "@Id"),
                    Propriedade = propriedade,
                });
            if (k > 0 && (evento.Destino(contexto) != primeiro.Destino(contexto) || evento.EmissaoPedido != primeiro.EmissaoPedido))
                problemas.Add(new Problema(CodigosEscrita.Lote, Severidade.Erro,
                    "O evento iria a outro destino ou tipo de emissão que o do primeiro do envio: o lote é processado por um órgão só.")
                {
                    Tag = "chNFe",
                    Caminho = Caminho(k, eventos.Count, "chNFe"),
                    Propriedade = propriedade + "." + nameof(Evento.Chave),
                });
        }
    }

    // Um por evento enviado, ligado ao retEvento pela chave, o tipo e a sequência; sem esses campos no retEvento, pela
    // posição quando a quantidade é a mesma; o que sobra do retorno, com Evento nulo e o aviso NFE-EVENTO.
    private static List<ResultadoEvento> Resultados(EnvioEvento parametro, string xmlPedido, XElement raiz, List<Problema> problemas)
    {
        var enviados = Enviados(xmlPedido);
        var retornos = raiz.Elements(XName.Get("retEvento", EscritaNFe.Namespace)).ToList();
        var lidos = new List<Lido>();
        for (var i = 0; i < retornos.Count; i++)
            lidos.Add(Ler(retornos[i], string.Create(CultureInfo.InvariantCulture, $"retEnvEvento/retEvento[{i + 1}]"), problemas));

        var ligacao = new int[lidos.Count];
        Array.Fill(ligacao, -1);
        var usados = new bool[parametro.Eventos.Count];
        for (var i = 0; i < lidos.Count; i++)
        {
            if (lidos[i].Chave is not { } chave || lidos[i].Tipo is not { } tipo || lidos[i].Sequencia is not { } sequencia)
                continue;
            var k = Enumerable.Range(0, usados.Length).FirstOrDefault(
                j => !usados[j] && parametro.Eventos[j].Chave == chave && (int)parametro.Eventos[j].Tipo == tipo && parametro.Eventos[j].Sequencia == sequencia,
                -1);
            if (k >= 0)
            {
                (ligacao[i], usados[k]) = (k, true);
            }
        }
        if (lidos.Count == parametro.Eventos.Count)
        {
            for (var i = 0; i < lidos.Count; i++)
            {
                if (ligacao[i] < 0 && !usados[i] && !TemChave(lidos[i]))
                    (ligacao[i], usados[i]) = (i, true);
            }
        }

        var resultados = new List<(int Ordem, ResultadoEvento Resultado)>();
        for (var i = 0; i < lidos.Count; i++)
        {
            var lido = lidos[i];
            var k = ligacao[i];
            string? xmlEvento = k >= 0 && k < enviados.Count ? enviados[k] : null;
            var resultado = new ResultadoEvento(lido.Status, lido.Motivo)
            {
                Evento = k >= 0 ? parametro.Eventos[k] : null,
                Chave = lido.Chave,
                Descricao = lido.Descricao,
                Protocolo = lido.Protocolo,
                Registro = lido.Registro,
                XmlEvento = xmlEvento,
                XmlProcessado = xmlEvento is not null && lido.Status is 135 or 136 or 155
                    ? ProcEvento(xmlEvento, lido.Elemento)
                    : null,
            };
            if (k < 0)
                problemas.Add(LeituraRetorno.Aviso(CodigosEvento, "retEnvEvento", "retEvento",
                    "Um retEvento do retorno não é de nenhum dos eventos enviados: fica sem o evento."));
            resultados.Add((k >= 0 ? k : int.MaxValue - lidos.Count + i, resultado));
        }
        for (var k = 0; k < usados.Length; k++)
        {
            if (!usados[k])
                problemas.Add(new Problema(CodigosEvento, Severidade.Aviso, "O evento enviado não veio com o retEvento na resposta.")
                {
                    Caminho = "retEnvEvento",
                    Propriedade = Passo(k),
                });
        }
        return [.. resultados.OrderBy(r => r.Ordem).Select(r => r.Resultado)];
    }

    private static bool TemChave(Lido lido) => lido.Chave is not null && lido.Tipo is not null && lido.Sequencia is not null;

    /// <summary>O <c>procEventoNFe</c>: o evento com os bytes como foi enviado e o <c>retEvento</c> da resposta.</summary>
    internal static string ProcEvento(string evento, XElement retEvento) =>
        $"<procEventoNFe xmlns=\"{EscritaNFe.Namespace}\" versao=\"{Versao}\">{evento}{retEvento.ToString(SaveOptions.DisableFormatting)}</procEventoNFe>";

    private sealed record Lido(int Status, string Motivo, ChaveAcesso? Chave, int? Tipo, int? Sequencia, string? Descricao,
        string? Protocolo, DateTimeOffset? Registro, XElement Elemento);

    private const string CodigosEvento = "NFE-EVENTO";

    private static Lido Ler(XElement retEvento, string caminho, List<Problema> problemas)
    {
        var info = retEvento.Element(XName.Get("infEvento", EscritaNFe.Namespace))
            ?? throw new FormatException("O retEvento da resposta não tem o infEvento.");
        var locais = new List<Problema>();
        var campos = LeituraRetorno.Campos(info, ConhecidoEvento, new HashSet<string>(), locais);
        foreach (var problema in locais)
            problemas.Add(new Problema(problema.Codigo, problema.Severidade, problema.Mensagem)
            {
                Tag = problema.Tag,
                Caminho = caminho + "/" + problema.Caminho,
            });
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        ChaveAcesso? chave = null;
        if (campos.TryGetValue("chNFe", out var textoChave) && !ChaveAcesso.TentarLer(textoChave, out chave))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, caminho + "/infEvento", "chNFe", "O campo chNFe do retorno não é uma chave de acesso: fica nulo."));
        var tipo = LeituraRetorno.Campo(campos, caminho + "/infEvento", "tpEvento", problemas, "não é um número", LerInteiro);
        var sequencia = LeituraRetorno.Campo(campos, caminho + "/infEvento", "nSeqEvento", problemas, "não é um número", LerInteiro);
        var registro = LeituraRetorno.Campo(campos, caminho + "/infEvento", "dhRegEvento", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora);
        return new Lido(status, motivo, chave, tipo, sequencia, campos.GetValueOrDefault("xEvento"), campos.GetValueOrDefault("nProt"), registro, retEvento);
    }

    // Os eventos como foram enviados, tirados da mensagem assinada (envEvento), na ordem: os bytes de cada evento.
    internal static List<string> Enviados(string xmlPedido)
    {
        var eventos = new List<string>();
        var inicio = xmlPedido.IndexOf("</idLote>", StringComparison.Ordinal);
        while (inicio >= 0)
        {
            var abre = xmlPedido.IndexOf("<evento ", inicio, StringComparison.Ordinal);
            if (abre < 0)
                break;
            var fecha = xmlPedido.IndexOf("</evento>", abre, StringComparison.Ordinal);
            if (fecha < 0)
                break;
            fecha += "</evento>".Length;
            eventos.Add(xmlPedido[abre..fecha]);
            inicio = fecha;
        }
        return eventos;
    }

    // O idLote do relógio, em UTC: AAMMDDhhmmssmmm (15 dígitos).
    private static long Lote(TimeProvider relogio) =>
        long.Parse(relogio.GetUtcNow().ToString("yyMMddHHmmssfff", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

    private static int? LerInteiro(string texto) =>
        texto.Length <= 9 && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static long? LerLote(string texto) =>
        texto.Length <= 15 && long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static bool Conhecido(string tag) =>
        tag is "idLote" or "tpAmb" or "verAplic" or "cOrgao" or "cStat" or "xMotivo";

    private static bool ConhecidoEvento(string tag) =>
        tag is "tpAmb" or "verAplic" or "cOrgao" or "cStat" or "xMotivo" or "chNFe" or "tpEvento" or "xEvento" or "nSeqEvento"
            or "cOrgaoAutor" or "CNPJDest" or "CPFDest" or "emailDest" or "dhRegEvento" or "nProt";
}
