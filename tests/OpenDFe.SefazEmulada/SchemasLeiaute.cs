using OpenDFe.NFe;

namespace OpenDFe.SefazEmulada;

// Os XSDs das mensagens, do pacote de liberação embutido no leiaute 4.00 (docs/NTS.md), pelo SchemaXml da lib
// (13.5: da lib, só o que é neutro). O recurso é achado pelo nome do arquivo, sem o pacote, para a troca do
// pacote no leiaute não pedir mudança aqui. O código do problema não importa: quem valida escolhe o cStat.
internal static class SchemasLeiaute
{
    private static readonly Lazy<SchemaXml> SchemaLote = Preparar("enviNFe_v4.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaRecibo = Preparar("consReciNFe_v4.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaConsulta = Preparar("consSitNFe_v4.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaInutilizacao = Preparar("inutNFe_v4.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaEvento = Preparar("envEvento_v1.00.xsd");

    // O detEvento de cada tipo de evento (Anexo I, D06), pelo tpEvento.
    private static readonly Dictionary<string, Lazy<SchemaXml>> SchemasDetalhe = new(StringComparer.Ordinal)
    {
        ["110110"] = Preparar("e110110_v1.00.xsd"),
        ["110111"] = Preparar("e110111_v1.00.xsd"),
        ["110112"] = Preparar("e110112_v1.00.xsd"),
    };

    // O enviNFe, com as notas dentro (NFeAutorizacao4).
    public static SchemaXml Lote => SchemaLote.Value;

    // O consReciNFe (NFeRetAutorizacao4).
    public static SchemaXml Recibo => SchemaRecibo.Value;

    // O consSitNFe (NFeConsultaProtocolo4).
    public static SchemaXml Consulta => SchemaConsulta.Value;

    // O inutNFe (NFeInutilizacao4).
    public static SchemaXml Inutilizacao => SchemaInutilizacao.Value;

    // O envEvento genérico, com o detEvento livre (NFeRecepcaoEvento4).
    public static SchemaXml Evento => SchemaEvento.Value;

    // O detEvento do tipo de evento (e110110, e110111, e110112).
    public static SchemaXml Detalhe(string tipo) => SchemasDetalhe[tipo].Value;

    private static Lazy<SchemaXml> Preparar(string arquivo) => new(() =>
    {
        var assembly = typeof(LeiauteNFe400).Assembly;
        var recurso = assembly.GetManifestResourceNames().Single(n => n.EndsWith("/" + arquivo, StringComparison.Ordinal)
            || n.EndsWith("\\" + arquivo, StringComparison.Ordinal));
        return SchemaXml.Carregar(assembly, recurso, Motivos.FalhaSchema);
    });
}
