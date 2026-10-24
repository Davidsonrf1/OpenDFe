using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// A distribuição DF-e (<c>NFeDistribuicaoDFe</c>, NT 2014.002 v1.40, docs/CONTRATOS.md 23): o <c>distDFeInt</c> 1.01 ao
/// Ambiente Nacional e o <c>retDistDFeInt</c> 1.01 com cada <c>docZip</c> descompactado. Só na NF-e.
/// </summary>
/// <remarks>
/// <para>
/// O pedido não é assinado, vai sempre ao AN (<see cref="Destino.AN"/>) e passa pelo <c>distDFeInt_v1.01.xsd</c> (215). Os
/// argumentos já foram conferidos nos métodos de <see cref="DistribuicaoDFe"/>; a chave de outro modelo é recusada antes de
/// transmitir (<c>NFE-CHAVE</c>).
/// </para>
/// <para>
/// A leitura é branda (<see cref="LeituraRetorno"/>, docs/CONTRATOS.md 13, decisão 4): o <c>docZip</c> ilegível (base64
/// inválido, gzip corrompido, mais de 16 MB descompactado, XML que não é bem formado) é <c>NFE-DISTRIBUICAO</c>, com o NSU, e
/// o documento sai da lista, sem derrubar os outros; o <c>ultNSU</c> e o <c>maxNSU</c> que não se leem ficam nulos, com
/// <c>NFE-VALOR</c>. Os avisos nunca viram recusa: a resposta já foi processada.
/// </para>
/// </remarks>
internal sealed class OperacaoDistribuicao : IOperacao<DistribuicaoDFe, RetornoDistribuicao>
{
    private const string Versao = "1.01";
    private const string Raiz = "retDistDFeInt";
    private const string CaminhoDocumento = "retDistDFeInt/loteDistDFeInt/docZip";

    // A descompressão tem teto: um gzip pequeno que expande sem limite derrubaria a memória (bomba de descompressão).
    private const int LimiteDescompactado = 16 * 1024 * 1024;

    private static readonly HashSet<string> Grupos = ["loteDistDFeInt"];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    private static readonly XmlReaderSettings ConfiguracaoLeitura = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    private static readonly UTF8Encoding Utf8Estrito = new(false, true);

    public PedidoWS Montar(DistribuicaoDFe parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        if (parametro.ChaveConsultada is { } chave)
            SituacaoNFe.ConferirModelo(chave, contexto.Documento, "distDFeInt/consChNFe/chNFe", "ChaveConsultada", problemas, "a distribuição");

        var uf = parametro.UFAutor ?? contexto.Configuracao.UF;
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("distDFeInt", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", Versao);
            xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)contexto.Configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("cUFAutor", EscritaNFe.Namespace, uf.Codigo().ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString(parametro.Interessado.Length == Cnpj.Tamanho ? "CNPJ" : "CPF", EscritaNFe.Namespace, parametro.Interessado);
            switch (parametro.Consulta)
            {
                case TipoConsultaDistribuicao.UltimoNsu:
                    xml.WriteStartElement("distNSU", EscritaNFe.Namespace);
                    xml.WriteElementString("ultNSU", EscritaNFe.Namespace, Nsu(parametro.NsuConsultado!.Value));
                    xml.WriteEndElement();
                    break;
                case TipoConsultaDistribuicao.Nsu:
                    xml.WriteStartElement("consNSU", EscritaNFe.Namespace);
                    xml.WriteElementString("NSU", EscritaNFe.Namespace, Nsu(parametro.NsuConsultado!.Value));
                    xml.WriteEndElement();
                    break;
                default:
                    xml.WriteStartElement("consChNFe", EscritaNFe.Namespace);
                    xml.WriteElementString("chNFe", EscritaNFe.Namespace, parametro.ChaveConsultada!.ToString());
                    xml.WriteEndElement();
                    break;
            }
            xml.WriteEndElement();
        }
        return new PedidoWS(AutorizadoresNFe400.DistribuicaoDFe, texto.ToString(), SchemasNFe400.DistribuicaoPedido)
        {
            Destino = Destino.AN,
            Emissao = TipoEmissao.Normal,
            Problemas = problemas,
        };
    }

    public LeituraWS<RetornoDistribuicao> Ler(DistribuicaoDFe parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        var documentos = new List<DocumentoDistribuido>();
        var lotes = raiz.Elements(XName.Get("loteDistDFeInt", EscritaNFe.Namespace)).ToList();
        if (lotes.Count > 1)
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Repetido, Raiz, "loteDistDFeInt", "O grupo loteDistDFeInt do retorno veio repetido: vale o primeiro."));
        if (lotes.Count > 0)
        {
            foreach (var filho in lotes[0].Elements())
            {
                if (filho.Name != XName.Get("docZip", EscritaNFe.Namespace))
                    problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Desconhecido, Raiz + "/loteDistDFeInt", filho.Name.LocalName,
                        $"O elemento {filho.Name.LocalName} do lote não é conhecido do leiaute: ignorado."));
                else if (LerDocumento(filho, problemas) is { } documento)
                    documentos.Add(documento);
            }
        }

        var retorno = new RetornoDistribuicao(status, motivo, documentos.AsReadOnly(), parametro.Consulta == TipoConsultaDistribuicao.UltimoNsu)
        {
            Ambiente = LeituraRetorno.Campo(campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            Resposta = LeituraRetorno.Campo(campos, Raiz, "dhResp", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            UltimoNsu = LeituraRetorno.Campo(campos, Raiz, "ultNSU", problemas, "não é um NSU de até 15 algarismos", LerNsu),
            MaximoNsu = LeituraRetorno.Campo(campos, Raiz, "maxNSU", problemas, "não é um NSU de até 15 algarismos", LerNsu),
        };
        return new LeituraWS<RetornoDistribuicao>(retorno, status, motivo) { Problemas = problemas };
    }

    private static string Nsu(long valor) => valor.ToString("D15", CultureInfo.InvariantCulture);

    private static long? LerNsu(string texto) =>
        texto.Length is >= 1 and <= 15 && long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "dhResp" or "ultNSU" or "maxNSU";

    // Um docZip: o NSU, o schema, o gzip em base64 e o XML. Nulo quando o conteúdo não se lê (com o aviso).
    private static DocumentoDistribuido? LerDocumento(XElement docZip, List<Problema> problemas)
    {
        long? nsu = null;
        var textoNsu = docZip.Attribute("NSU")?.Value;
        if (textoNsu is not null && (nsu = LerNsu(textoNsu)) is null)
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, "retDistDFeInt/loteDistDFeInt", "docZip/@NSU", "O NSU do docZip não é um NSU de até 15 algarismos: fica nulo."));
        var identificacao = nsu is { } n ? $"NSU {n}" : "sem NSU";

        var schema = docZip.Attribute("schema")?.Value;
        if (string.IsNullOrWhiteSpace(schema))
        {
            problemas.Add(AvisoDocumento($"O docZip ({identificacao}) não tem o atributo schema: o tipo fica Outro."));
            schema = "";
        }

        var motivo = Descompactar(docZip.Value, out var xml);
        if (motivo is not null)
        {
            problemas.Add(AvisoDocumento($"O docZip ({identificacao}) não se lê e foi omitido: {motivo}."));
            return null;
        }

        XElement conteudo;
        try
        {
            using var texto = new StringReader(xml!);
            using var leitor = XmlReader.Create(texto, ConfiguracaoLeitura);
            conteudo = XElement.Load(leitor);
        }
        catch (XmlException)
        {
            problemas.Add(AvisoDocumento($"O docZip ({identificacao}) não se lê e foi omitido: o XML descompactado não é bem formado."));
            return null;
        }

        var tipo = ClassificarSchema(schema);
        return tipo switch
        {
            TipoDocumentoDistribuido.ResumoNFe => new DocumentoDistribuido(nsu, schema, tipo, xml!)
            {
                ResumoNFe = LerResumoNFe(conteudo, identificacao, problemas),
                Chave = ChaveDe(conteudo, "chNFe"),
            },
            TipoDocumentoDistribuido.ResumoEvento => new DocumentoDistribuido(nsu, schema, tipo, xml!)
            {
                ResumoEvento = LerResumoEvento(conteudo, identificacao, problemas),
                Chave = ChaveDe(conteudo, "chNFe"),
            },
            TipoDocumentoDistribuido.NFeProcessada => new DocumentoDistribuido(nsu, schema, tipo, xml!) { Chave = ChaveNota(conteudo) },
            TipoDocumentoDistribuido.EventoProcessado => new DocumentoDistribuido(nsu, schema, tipo, xml!) { Chave = ChaveEvento(conteudo) },
            _ => new DocumentoDistribuido(nsu, schema, tipo, xml!),
        };
    }

    private static Problema AvisoDocumento(string mensagem) =>
        new(CodigosLeitura.Distribuicao, Severidade.Aviso, mensagem) { Tag = "docZip", Caminho = CaminhoDocumento };

    // O tipo pelo prefixo do nome do schema, sem a versão (resNFe_v1.01.xsd, procEventoNFe_v1.00.xsd).
    private static TipoDocumentoDistribuido ClassificarSchema(string schema)
    {
        var nome = schema.Trim();
        if (nome.StartsWith("resNFe_", StringComparison.Ordinal))
            return TipoDocumentoDistribuido.ResumoNFe;
        if (nome.StartsWith("procNFe_", StringComparison.Ordinal))
            return TipoDocumentoDistribuido.NFeProcessada;
        if (nome.StartsWith("resEvento_", StringComparison.Ordinal))
            return TipoDocumentoDistribuido.ResumoEvento;
        if (nome.StartsWith("procEventoNFe_", StringComparison.Ordinal))
            return TipoDocumentoDistribuido.EventoProcessado;
        return TipoDocumentoDistribuido.Outro;
    }

    // O base64 do gzip em texto UTF-8 (sem o BOM), com o teto de LimiteDescompactado. O motivo do que não deu, ou nulo.
    private static string? Descompactar(string base64, out string? xml)
    {
        xml = null;
        byte[] compactado;
        try
        {
            compactado = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return "o conteúdo não é base64";
        }

        try
        {
            using var origem = new MemoryStream(compactado);
            using var gzip = new GZipStream(origem, CompressionMode.Decompress);
            using var destino = new MemoryStream();
            var bloco = new byte[81920];
            int lidos;
            while ((lidos = gzip.Read(bloco, 0, bloco.Length)) > 0)
            {
                if (destino.Length + lidos > LimiteDescompactado)
                    return "o conteúdo descompactado passa de 16 MB";
                destino.Write(bloco, 0, lidos);
            }
            var bytes = destino.GetBuffer().AsSpan(0, (int)destino.Length);
            if (bytes.StartsWith(Encoding.UTF8.Preamble))
                bytes = bytes[Encoding.UTF8.Preamble.Length..];
            xml = Utf8Estrito.GetString(bytes);
            return null;
        }
        catch (Exception e) when (e is InvalidDataException or IOException)
        {
            return "o conteúdo não é um gzip válido";
        }
        catch (DecoderFallbackException)
        {
            return "o conteúdo não é UTF-8";
        }
    }

    private static string? Texto(XElement elemento, string nome) =>
        elemento.Elements().FirstOrDefault(e => e.Name.LocalName == nome)?.Value.Trim();

    private static ChaveAcesso? ChaveDe(XElement elemento, string nome) =>
        ChaveAcesso.TentarLer(Texto(elemento, nome), out var chave) ? chave : null;

    // A chave do nfeProc: o Id do infNFe sem o "NFe"; sem ele, a do protocolo.
    private static ChaveAcesso? ChaveNota(XElement raiz)
    {
        var inf = raiz.Descendants().FirstOrDefault(e => e.Name.LocalName == "infNFe");
        if (inf?.Attribute("Id")?.Value is { } id && id.StartsWith("NFe", StringComparison.Ordinal)
            && ChaveAcesso.TentarLer(id[3..], out var chave))
            return chave;
        var protocolo = raiz.Descendants().FirstOrDefault(e => e.Name.LocalName == "infProt");
        return protocolo is null ? null : ChaveDe(protocolo, "chNFe");
    }

    // A chave do procEventoNFe: a do evento; sem ela, a do registro.
    private static ChaveAcesso? ChaveEvento(XElement raiz)
    {
        foreach (var nome in new[] { "evento", "retEvento" })
        {
            var inf = raiz.Descendants().FirstOrDefault(e => e.Name.LocalName == "infEvento"
                && e.Parent?.Name.LocalName == nome);
            if (inf is not null && ChaveDe(inf, "chNFe") is { } chave)
                return chave;
        }
        return null;
    }

    private static ResumoNFe? LerResumoNFe(XElement raiz, string identificacao, List<Problema> problemas)
    {
        var chave = ChaveDe(raiz, "chNFe");
        var emitente = AutorDocumento(raiz);
        var nome = Texto(raiz, "xNome");
        var ie = Texto(raiz, "IE") ?? "";
        var emissao = DataHora(raiz, "dhEmi");
        var tipo = Texto(raiz, "tpNF") switch { "0" => TipoNota.Entrada, "1" => TipoNota.Saida, _ => (TipoNota?)null };
        var valor = Texto(raiz, "vNF") is { } v && decimal.TryParse(v, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) && d >= 0 ? d : (decimal?)null;
        var recebimento = DataHora(raiz, "dhRecbto");
        var protocolo = Texto(raiz, "nProt");
        var situacao = Texto(raiz, "cSitNFe") is { } s && int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var cod) ? (SituacaoResumo)cod : (SituacaoResumo?)null;

        var faltou = chave is null ? "chNFe"
            : emitente is null ? "CNPJ/CPF"
            : nome is null ? "xNome"
            : emissao is null ? "dhEmi"
            : tipo is null ? "tpNF"
            : valor is null ? "vNF"
            : recebimento is null ? "dhRecbto"
            : protocolo is null ? "nProt"
            : situacao is null ? "cSitNFe"
            : null;
        if (faltou is not null)
        {
            problemas.Add(AvisoDocumento($"O resNFe ({identificacao}) não se lê nos tipos (o campo {faltou} falta ou é ilegível): ResumoNFe fica nulo, e o XML segue em Xml."));
            return null;
        }
        return new ResumoNFe(chave!, emitente!, nome!, ie, emissao!.Value, tipo!.Value, valor!.Value, recebimento!.Value, protocolo!, situacao!.Value)
        {
            Digest = Texto(raiz, "digVal"),
        };
    }

    private static ResumoEvento? LerResumoEvento(XElement raiz, string identificacao, List<Problema> problemas)
    {
        var orgao = Inteiro(raiz, "cOrgao", 2);
        var autor = AutorDocumento(raiz);
        var chave = ChaveDe(raiz, "chNFe");
        var momento = DataHora(raiz, "dhEvento");
        var tipo = Inteiro(raiz, "tpEvento", 6);
        var sequencia = Inteiro(raiz, "nSeqEvento", 2);
        var descricao = Texto(raiz, "xEvento");
        var recebimento = DataHora(raiz, "dhRecbto");
        var protocolo = Texto(raiz, "nProt");

        var faltou = orgao is null ? "cOrgao"
            : autor is null ? "CNPJ/CPF"
            : chave is null ? "chNFe"
            : momento is null ? "dhEvento"
            : tipo is null ? "tpEvento"
            : sequencia is null ? "nSeqEvento"
            : descricao is null ? "xEvento"
            : recebimento is null ? "dhRecbto"
            : protocolo is null ? "nProt"
            : null;
        if (faltou is not null)
        {
            problemas.Add(AvisoDocumento($"O resEvento ({identificacao}) não se lê nos tipos (o campo {faltou} falta ou é ilegível): ResumoEvento fica nulo, e o XML segue em Xml."));
            return null;
        }
        return new ResumoEvento(orgao!.Value, autor!, chave!, momento!.Value, (TipoEvento)tipo!.Value, sequencia!.Value, descricao!, recebimento!.Value, protocolo!);
    }

    // O CNPJ ou o CPF do emitente (do autor), como veio, se tem o tamanho de um deles; nulo se falta.
    private static string? AutorDocumento(XElement raiz)
    {
        var texto = Texto(raiz, "CNPJ") ?? Texto(raiz, "CPF");
        return texto is { Length: Cnpj.Tamanho or 11 } ? texto : null;
    }

    private static DateTimeOffset? DataHora(XElement raiz, string nome) =>
        Texto(raiz, nome) is { } texto ? LeituraRetorno.LerDataHora(texto) : null;

    private static int? Inteiro(XElement raiz, string nome, int digitos) =>
        Texto(raiz, nome) is { } texto && texto.Length <= digitos
        && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor) ? valor : null;
}
