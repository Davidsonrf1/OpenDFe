using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// O que a consulta do protocolo e a recuperação da autorização têm em comum (docs/CONTRATOS.md 15): o <c>consSitNFe</c>
/// da chave, ao autorizador da chave, a recusa da chave de outro modelo (decisão 3) e a leitura branda do
/// <c>retConsSitNFe</c>. Sem estado.
/// </summary>
internal static class SituacaoNFe
{
    private const string Raiz = "retConsSitNFe";

    private static readonly HashSet<string> Grupos = ["protNFe", "retCancNFe", "procEventoNFe"];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    /// <summary>O retorno lido: os campos, o protocolo (com o elemento, para o <c>nfeProc</c>) e os eventos como texto.</summary>
    internal sealed record Leitura(int Status, string Motivo, Dictionary<string, string> Campos, ChaveAcesso? Chave,
        Protocolo? Protocolo, XElement? ElementoProtocolo, IReadOnlyList<string> Eventos);

    /// <summary>
    /// O <c>consSitNFe</c> da chave, ao autorizador da UF e do tipo de emissão dela (PROJETO.md 8.4); com o
    /// <paramref name="destino"/> do parâmetro, ao autorizador normal dele (a nota da SVC consultada na UF, decisão 4).
    /// Sem a chave (a nota assinada que não se leu), a mensagem vai com o <c>chNFe</c> vazio: os problemas já a recusam.
    /// </summary>
    internal static PedidoWS Pedido(ChaveAcesso? chave, Destino? destino, ContextoOperacao contexto, List<Problema> problemas)
    {
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("consSitNFe", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", "4.00");
            xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)contexto.Configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("xServ", EscritaNFe.Namespace, "CONSULTAR");
            xml.WriteElementString("chNFe", EscritaNFe.Namespace, chave?.ToString() ?? "");
            xml.WriteEndElement();
        }
        return new PedidoWS(AutorizadoresNFe400.ConsultaProtocolo, texto.ToString(), SchemasNFe400.Situacao)
        {
            Destino = chave?.UF.ComoDestino(),
            Emissao = destino is null ? chave?.TipoEmissao : TipoEmissao.Normal,
            Problemas = problemas,
        };
    }

    /// <summary>
    /// A chave de outro modelo que o do núcleo (a NFC-e num núcleo de NF-e e o contrário) é recusada: os web services
    /// são outros em várias UFs, e a resposta seria enganosa (decisão 3).
    /// </summary>
    internal static void ConferirModelo(ChaveAcesso chave, Documento documento, string caminho, string propriedade, List<Problema> problemas)
    {
        if (chave.Modelo != documento)
            problemas.Add(new Problema(CodigosLeitura.Chave, Severidade.Erro,
                $"A chave é de {chave.Modelo}, e o núcleo é de {documento}: a consulta iria ao web service do outro documento.")
            {
                Tag = caminho[(caminho.LastIndexOf('/') + 1)..].TrimStart('@'),
                Caminho = caminho,
                Propriedade = propriedade,
            });
    }

    /// <summary>
    /// O <c>retConsSitNFe</c>, brando como o status (CONTRATOS.md 13, decisão 4): o <c>protNFe</c> pelo leitor do
    /// protocolo (os problemas dele como aviso, com a propriedade a partir de <paramref name="prefixo"/>), cada
    /// <c>procEventoNFe</c> como texto, o <c>retCancNFe</c> ignorado.
    /// </summary>
    internal static Leitura Ler(string xmlRetorno, string prefixo, List<Problema> problemas)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        ChaveAcesso? chave = null;
        if (campos.TryGetValue("chNFe", out var textoChave) && !ChaveAcesso.TentarLer(textoChave, out chave)
            && textoChave.Any(c => c != '0'))
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Valor, Raiz, "chNFe", "O campo chNFe do retorno não é uma chave de acesso: fica nulo."));

        Protocolo? protocolo = null;
        var elementos = raiz.Elements(XName.Get("protNFe", EscritaNFe.Namespace)).ToList();
        if (elementos.Count > 1)
            problemas.Add(LeituraRetorno.Aviso(CodigosLeitura.Repetido, Raiz, "protNFe", "O grupo protNFe do retorno veio repetido: vale o primeiro."));
        if (elementos.Count > 0)
        {
            (protocolo, var lidos) = LeitorNFe.LerProtocolo(elementos[0]);
            foreach (var problema in lidos)
                problemas.Add(new Problema(problema.Codigo, Severidade.Aviso, problema.Mensagem)
                {
                    Tag = problema.Tag,
                    Caminho = problema.Caminho is { } c && c.StartsWith("protNFe", StringComparison.Ordinal) ? Raiz + "/" + c : problema.Caminho,
                    Propriedade = problema.Propriedade is { } p ? prefixo + p : null,
                });
        }

        List<string> eventos = [.. raiz.Elements(XName.Get("procEventoNFe", EscritaNFe.Namespace))
            .Select(e => e.ToString(SaveOptions.DisableFormatting))];
        return new Leitura(status, motivo, campos, chave, protocolo, elementos.FirstOrDefault(), eventos.AsReadOnly());
    }

    /// <summary>Os campos comuns do retorno (o ambiente, a versão, a UF e o recebimento), com o aviso do que não se lê.</summary>
    internal static (Ambiente? Ambiente, string? Versao, UF? UF, DateTimeOffset? Recebimento) Comuns(Leitura leitura, List<Problema> problemas) =>
        (LeituraRetorno.Campo(leitura.Campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            leitura.Campos.GetValueOrDefault("verAplic"),
            LeituraRetorno.Campo(leitura.Campos, Raiz, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            LeituraRetorno.Campo(leitura.Campos, Raiz, "dhRecbto", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora));

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "cUF" or "dhRecbto" or "chNFe";
}
