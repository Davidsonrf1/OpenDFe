using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// A inutilização (<c>NFeInutilizacao4</c>, docs/CONTRATOS.md 18): o <c>inutNFe</c> assinado no <c>infInut</c> e o
/// <c>retInutNFe</c>, com o <c>ProcInutNFe</c> quando homologada. Sempre ao autorizador normal da UF do núcleo, no tipo de
/// emissão normal. Vale para a NF-e e para a NFC-e (o modelo é o do núcleo).
/// </summary>
internal sealed class OperacaoInutilizacao : IOperacao<Inutilizacao, RetornoInutilizacao>
{
    private const string Raiz = "retInutNFe";
    private const string Versao = "4.00";

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    public PedidoWS Montar(Inutilizacao parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var configuracao = contexto.Configuracao;
        var ano = parametro.Ano ?? configuracao.Relogio.GetUtcNow().ToOffset(TimeSpan.FromHours(-3)).Year % 100;
        var uf = configuracao.UF.Codigo().ToString(CultureInfo.InvariantCulture);
        var modelo = ((int)contexto.Documento).ToString(CultureInfo.InvariantCulture);
        var anoTexto = ano.ToString("D2", CultureInfo.InvariantCulture);
        var id = string.Create(CultureInfo.InvariantCulture,
            $"ID{uf}{anoTexto}{parametro.Cnpj}{modelo}{parametro.Serie:D3}{parametro.NumeroInicial:D9}{parametro.NumeroFinal:D9}");

        if (PosicaoInvalida(parametro.Justificativa) is { } posicao)
        {
            problemas.Add(new Problema(CodigosEscrita.Caractere, Severidade.Erro,
                string.Create(CultureInfo.InvariantCulture,
                    $"O campo xJust tem um caractere que o XML não aceita (U+{(int)parametro.Justificativa[posicao]:X4}, na posição {posicao + 1})."))
            {
                Tag = "xJust",
                Caminho = "inutNFe/infInut/xJust",
                Propriedade = nameof(Inutilizacao.Justificativa),
            });
            return Pedido("", id, problemas);
        }

        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("inutNFe", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", Versao);
            xml.WriteStartElement("infInut", EscritaNFe.Namespace);
            xml.WriteAttributeString("Id", id);
            xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("xServ", EscritaNFe.Namespace, "INUTILIZAR");
            xml.WriteElementString("cUF", EscritaNFe.Namespace, uf);
            xml.WriteElementString("ano", EscritaNFe.Namespace, anoTexto);
            xml.WriteElementString("CNPJ", EscritaNFe.Namespace, parametro.Cnpj);
            xml.WriteElementString("mod", EscritaNFe.Namespace, modelo);
            xml.WriteElementString("serie", EscritaNFe.Namespace, parametro.Serie.ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("nNFIni", EscritaNFe.Namespace, parametro.NumeroInicial.ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("nNFFin", EscritaNFe.Namespace, parametro.NumeroFinal.ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("xJust", EscritaNFe.Namespace, parametro.Justificativa);
            xml.WriteEndElement();
            xml.WriteEndElement();
        }
        return Pedido(texto.ToString(), id, problemas);
    }

    public LeituraWS<RetornoInutilizacao> Ler(Inutilizacao parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var info = raiz.Element(XName.Get("infInut", EscritaNFe.Namespace))
            ?? throw new FormatException("A resposta não tem o infInut.");
        var campos = LeituraRetorno.Campos(info, Conhecido, new HashSet<string>(), problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);
        const string Caminho = Raiz + "/infInut";

        var retorno = new RetornoInutilizacao(status, motivo)
        {
            Ambiente = LeituraRetorno.Campo(campos, Caminho, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = LeituraRetorno.Campo(campos, Caminho, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            Ano = LeituraRetorno.Campo(campos, Caminho, "ano", problemas, "não é um número", LerInteiro),
            Cnpj = campos.GetValueOrDefault("CNPJ"),
            Modelo = LeituraRetorno.Campo(campos, Caminho, "mod", problemas, "não é 55 nem 65", LerModelo),
            Serie = LeituraRetorno.Campo(campos, Caminho, "serie", problemas, "não é um número", LerInteiro),
            NumeroInicial = LeituraRetorno.Campo(campos, Caminho, "nNFIni", problemas, "não é um número", LerInteiro),
            NumeroFinal = LeituraRetorno.Campo(campos, Caminho, "nNFFin", problemas, "não é um número", LerInteiro),
            Recebimento = LeituraRetorno.Campo(campos, Caminho, "dhRecbto", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            Protocolo = campos.GetValueOrDefault("nProt"),
            XmlProcessado = status == 102 ? ProcInutilizacao(LoteNFe.Normalizar(xmlPedido), raiz) : null,
        };
        return new LeituraWS<RetornoInutilizacao>(retorno, status, motivo) { Problemas = problemas };
    }

    /// <summary>O <c>ProcInutNFe</c>: o <c>inutNFe</c> com os bytes como foi enviado e o <c>retInutNFe</c> da resposta.</summary>
    internal static string ProcInutilizacao(string inutNFe, XElement retInutNFe) =>
        $"<ProcInutNFe xmlns=\"{EscritaNFe.Namespace}\" versao=\"{Versao}\">{inutNFe}{retInutNFe.ToString(SaveOptions.DisableFormatting)}</ProcInutNFe>";

    private static PedidoWS Pedido(string xml, string id, List<Problema> problemas) =>
        new(AutorizadoresNFe400.Inutilizacao, xml, SchemasNFe400.Inutilizacao)
        {
            IdsAssinatura = [id],
            Emissao = TipoEmissao.Normal,
            Problemas = problemas,
        };

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

    private static int? LerInteiro(string texto) =>
        texto.Length <= 9 && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static Documento? LerModelo(string texto) => texto switch
    {
        "55" => Documento.NFe,
        "65" => Documento.NFCe,
        _ => null,
    };

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "cUF" or "ano" or "CNPJ" or "mod" or "serie" or "nNFIni"
            or "nNFFin" or "dhRecbto" or "nProt";
}
