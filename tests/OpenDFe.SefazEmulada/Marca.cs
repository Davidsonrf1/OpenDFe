using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// A marca no infCpl de uma nota do lote (PROJETO.md 13.5): a falha comandada de dentro da própria nota, para
// provocá-la a partir do PDV ou do ERP, sem a rota /_controle. Vale só para o pedido que a traz, no
// NFeAutorizacao4, e passa na frente da falha do controle da mesma espécie. As formas:
//   EMULADA:atraso:<milissegundos>     EMULADA:tempoEsgotado        EMULADA:conexaoDerrubada[:depois]
//   EMULADA:respostaIlegivel           EMULADA:falhaSoap            EMULADA:paralisacao[:108|:109]
//   EMULADA:loteProcessando[:<n>]  (as n primeiras consultas do recibo respondem 105; 1 se omitido)
//   EMULADA:irregular:emitente|destinatario  (só a nota que a traz: 781 ou 302, como os Irregulares)
// Uma por espécie no lote (transporte, paralisação, lote em processamento); a do irregular é da nota, e não do
// lote. Marca que não serve vira falha SOAP (soap:Sender), para o erro de digitação não passar calado.
internal static partial class Marca
{
    private const string Emitente = "emitente";
    private const string Destinatario = "destinatario";

    public static MarcaFalha? Ler(byte[] corpo)
    {
        XDocument documento;
        try
        {
            using var fluxo = new MemoryStream(corpo);
            using var leitor = XmlReader.Create(fluxo, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            documento = XDocument.Load(leitor);
        }
        catch (XmlException)
        {
            // O envelope responde pelo XML mal formado.
            return null;
        }

        FalhaComandada? transporte = null, paralisacao = null;
        int? pendentes = null;
        foreach (var texto in documento.Descendants(Servico.NamespaceNFe + "infCpl").Select(e => e.Value))
        {
            foreach (Match marca in Padrao().Matches(texto))
            {
                var tipo = marca.Groups["tipo"].Value;
                var valor = marca.Groups["valor"].Success ? marca.Groups["valor"].Value : null;
                switch (tipo)
                {
                    case "paralisacao":
                        if (paralisacao is not null)
                            throw Repetida(tipo);
                        if (valor is not (null or Motivos.ParalisadoMomentaneamente or Motivos.ParalisadoIndefinidamente))
                            throw Invalida(marca.Value, "a paralisação é 108 ou 109");
                        paralisacao = Falha(TipoFalha.Paralisacao, codigo: valor ?? Motivos.ParalisadoMomentaneamente);
                        break;
                    case "irregular":
                        // Da nota: quem a aplica é a autorização (Irregular), nota a nota.
                        if (valor is not (Emitente or Destinatario))
                            throw Invalida(marca.Value, "o irregular é o emitente ou o destinatario");
                        break;
                    case "loteProcessando":
                        if (pendentes is not null)
                            throw Repetida(tipo);
                        pendentes = valor is null ? 1 : Numero(marca.Value, valor);
                        break;
                    default:
                        if (transporte is not null)
                            throw Repetida(tipo);
                        transporte = Transporte(marca.Value, tipo, valor);
                        break;
                }
            }
        }
        return transporte is null && paralisacao is null && pendentes is null ? null : new MarcaFalha(transporte, paralisacao, pendentes ?? 0);
    }

    // A marca na DPS da NFS-e (E.7), no infDPS/serv/infoCompl/xInfComp (docs/NFSE.md 5): as mesmas formas de transporte e a
    // paralisação, sem o código (a Sefin responde 503). O lote em processamento e o irregular não valem na NFS-e. Marca que
    // não serve é FalhaSoapException, que a Sefin emulada devolve como 400 com o erro EMULADA-MARCA.
    public static MarcaFalha? LerNFSe(XElement dps)
    {
        ArgumentNullException.ThrowIfNull(dps);
        const string Campo = "xInfComp";
        FalhaComandada? transporte = null, paralisacao = null;
        foreach (var texto in dps.Descendants(EmissaoNFSe.Namespace + Campo).Select(e => e.Value))
        {
            foreach (Match marca in Padrao().Matches(texto))
            {
                var tipo = marca.Groups["tipo"].Value;
                var valor = marca.Groups["valor"].Success ? marca.Groups["valor"].Value : null;
                switch (tipo)
                {
                    case "paralisacao":
                        if (paralisacao is not null)
                            throw Repetida(tipo, "na DPS");
                        if (valor is not null)
                            throw Invalida(marca.Value, "a paralisação da NFS-e não tem código", Campo);
                        paralisacao = Falha(TipoFalha.Paralisacao);
                        break;
                    case "irregular" or "loteProcessando":
                        throw Invalida(marca.Value, "não vale na NFS-e", Campo);
                    default:
                        if (transporte is not null)
                            throw Repetida(tipo, "na DPS");
                        transporte = Transporte(marca.Value, tipo, valor, Campo);
                        break;
                }
            }
        }
        return transporte is null && paralisacao is null ? null : new MarcaFalha(transporte, paralisacao, 0);
    }

    // Os papéis que a marca do irregular põe em situação irregular nesta nota (o Ler já conferiu o valor).
    public static (bool Emitente, bool Destinatario) Irregular(XElement nota)
    {
        bool emitente = false, destinatario = false;
        foreach (var texto in nota.Descendants(Servico.NamespaceNFe + "infCpl").Select(e => e.Value))
        {
            foreach (Match marca in Padrao().Matches(texto))
            {
                if (marca.Groups["tipo"].Value != "irregular")
                    continue;
                emitente |= marca.Groups["valor"].Value == Emitente;
                destinatario |= marca.Groups["valor"].Value == Destinatario;
            }
        }
        return (emitente, destinatario);
    }

    private static FalhaComandada Transporte(string marca, string tipo, string? valor, string campo = "infCpl") => tipo switch
    {
        "atraso" => Falha(TipoFalha.Atraso, milissegundos: valor is null ? throw Invalida(marca, "o atraso precisa dos milissegundos", campo) : Numero(marca, valor, campo)),
        "tempoEsgotado" when valor is null => Falha(TipoFalha.TempoEsgotado),
        "conexaoDerrubada" when valor is null or "depois" => Falha(TipoFalha.ConexaoDerrubada, depois: valor is not null),
        "respostaIlegivel" when valor is null => Falha(TipoFalha.RespostaIlegivel),
        "falhaSoap" when valor is null => Falha(TipoFalha.FalhaSoap),
        "tempoEsgotado" or "conexaoDerrubada" or "respostaIlegivel" or "falhaSoap" => throw Invalida(marca, "valor que o tipo não aceita", campo),
        _ => throw Invalida(marca, "tipo desconhecido", campo),
    };

    private static FalhaComandada Falha(TipoFalha tipo, int? milissegundos = null, string? codigo = null, bool depois = false) =>
        new(0, tipo, null, null, milissegundos, 1, codigo, null, depois);

    private static int Numero(string marca, string valor, string campo = "infCpl") =>
        int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) && numero > 0
            ? numero
            : throw Invalida(marca, "o valor é um número maior que zero", campo);

    private static FalhaSoapException Invalida(string marca, string motivo, string campo = "infCpl") =>
        new(CodigoFalha.Remetente, $"Marca do emulador inválida no {campo} ({marca}): {motivo}.");

    private static FalhaSoapException Repetida(string tipo, string onde = "no lote") =>
        new(CodigoFalha.Remetente, $"Mais de uma marca do emulador da mesma espécie {onde} ({tipo}).");

    [GeneratedRegex("EMULADA:(?<tipo>[A-Za-z]+)(?::(?<valor>[0-9A-Za-z]+))?", RegexOptions.CultureInvariant)]
    private static partial Regex Padrao();
}

// O que a marca comanda: uma falha de transporte, uma paralisação e as consultas do recibo que respondem 105.
internal sealed record MarcaFalha(FalhaComandada? Transporte, FalhaComandada? Paralisacao, int Pendentes);
