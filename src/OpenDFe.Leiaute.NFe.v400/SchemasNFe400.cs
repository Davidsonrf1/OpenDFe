namespace OpenDFe.NFe;

/// <summary>
/// Os schemas das mensagens do leiaute 4.00, do pacote de liberação embutido em <c>Schemas/</c> (docs/NTS.md):
/// cada um carregado e compilado na primeira vez que é usado e guardado (o <see cref="SchemaXml"/> é imutável e
/// seguro em paralelo).
/// </summary>
/// <remarks>
/// Um schema por mensagem, com o código da rejeição que a falha antecipa (1.5): na nota, 225 ("Falha no Schema
/// XML da NFe"); nas outras mensagens, 215 ("Falha no schema XML"). Na nota e na nota processada, o problema sai
/// também com a propriedade do modelo (<see cref="CaminhoModeloNFe"/>). Os códigos vêm de resumos do MOC, não do
/// Anexo I em si (conferido na 3.1).
/// </remarks>
internal static class SchemasNFe400
{
    /// <summary>A pasta do pacote de liberação em <c>Schemas/</c>.</summary>
    internal const string Pacote = "PL_010f_v1.04";

    /// <summary>Falha no Schema XML da NFe.</summary>
    internal const string CodigoNota = "225";

    /// <summary>Falha no schema XML (da mensagem).</summary>
    internal const string CodigoMensagem = "215";

    private static readonly Lazy<SchemaXml> SchemaNota = Preparar("nfe_v4.00.xsd", CodigoNota, CaminhoModeloNFe.Propriedade);
    private static readonly Lazy<SchemaXml> SchemaLote = Preparar("enviNFe_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaRecibo = Preparar("consReciNFe_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaSituacao = Preparar("consSitNFe_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaStatus = Preparar("consStatServ_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaInutilizacao = Preparar("inutNFe_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaNotaProcessada = Preparar("procNFe_v4.00.xsd", CodigoMensagem, CaminhoModeloNFe.Propriedade);

    /// <summary>A nota sozinha (<c>NFe</c>): a que o <c>Preparar</c> gera e a que se lê.</summary>
    internal static SchemaXml Nota => SchemaNota.Value;

    /// <summary>O lote da autorização (<c>enviNFe</c>, <c>NFeAutorizacao4</c>).</summary>
    internal static SchemaXml Lote => SchemaLote.Value;

    /// <summary>A consulta do recibo (<c>consReciNFe</c>, <c>NFeRetAutorizacao4</c>).</summary>
    internal static SchemaXml Recibo => SchemaRecibo.Value;

    /// <summary>A consulta do protocolo (<c>consSitNFe</c>, <c>NFeConsultaProtocolo4</c>).</summary>
    internal static SchemaXml Situacao => SchemaSituacao.Value;

    /// <summary>O status do serviço (<c>consStatServ</c>, <c>NFeStatusServico4</c>).</summary>
    internal static SchemaXml Status => SchemaStatus.Value;

    /// <summary>A inutilização (<c>inutNFe</c>, <c>NFeInutilizacao4</c>).</summary>
    internal static SchemaXml Inutilizacao => SchemaInutilizacao.Value;

    /// <summary>A nota com o protocolo (<c>nfeProc</c>), a distribuível.</summary>
    internal static SchemaXml NotaProcessada => SchemaNotaProcessada.Value;

    /// <summary>O nome do recurso de um arquivo do pacote (<c>"Schemas/PL_010f_v1.04/nfe_v4.00.xsd"</c>).</summary>
    internal static string Recurso(string arquivo) => $"Schemas/{Pacote}/{arquivo}";

    private static Lazy<SchemaXml> Preparar(string arquivo, string codigo) =>
        new(() => SchemaXml.Carregar(typeof(SchemasNFe400).Assembly, Recurso(arquivo), codigo));

    // A nota e a nota processada: o problema do schema aponta a propriedade do modelo (docs/MODELO.md 14).
    private static Lazy<SchemaXml> Preparar(string arquivo, string codigo, Func<string, string?> propriedade) =>
        new(() => SchemaXml.Carregar(typeof(SchemasNFe400).Assembly, Recurso(arquivo), codigo, propriedade));
}
