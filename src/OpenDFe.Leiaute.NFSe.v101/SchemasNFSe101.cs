namespace OpenDFe.NFSe;

/// <summary>
/// Os schemas da NFS-e, do pacote de produção embutido em <c>Schemas/</c> (docs/NTS.md 12.1; a D2 da etapa 9.1), nas pastas
/// <c>1.00</c> e <c>1.01</c>: cada um carregado e compilado na primeira vez que é usado e guardado (o
/// <see cref="SchemaXml"/> é imutável e seguro em paralelo).
/// </summary>
/// <remarks>
/// O código do problema é o da rejeição da Sefin que a falha antecipa: E1235, "Falha no esquema XML do DF-e" (Anexo I, aba
/// <c>RN_RECEPCAO_DPS</c>). O problema sai com a propriedade do modelo (<see cref="CaminhoModeloNFSe"/>).
/// </remarks>
internal static class SchemasNFSe101
{
    /// <summary>A pasta do pacote em <c>Schemas/</c>.</summary>
    internal const string Pacote = "NFSe-ESQUEMAS_XSD-v1.01-20260209";

    /// <summary>Falha no esquema XML do DF-e.</summary>
    internal const string Codigo = "E1235";

    private static readonly Lazy<SchemaXml> SchemaDps = Preparar("1.01", "DPS_v1.01.xsd");
    private static readonly Lazy<SchemaXml> SchemaNota = Preparar("1.01", "NFSe_v1.01.xsd");
    private static readonly Lazy<SchemaXml> SchemaPedido = Preparar("1.01", "pedRegEvento_v1.01.xsd");
    private static readonly Lazy<SchemaXml> SchemaEvento = Preparar("1.01", "evento_v1.01.xsd");
    private static readonly Lazy<SchemaXml> SchemaDps100 = Preparar("1.00", "DPS_v1.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaNota100 = Preparar("1.00", "NFSe_v1.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaPedido100 = Preparar("1.00", "pedRegEvento_v1.00.xsd");
    private static readonly Lazy<SchemaXml> SchemaEvento100 = Preparar("1.00", "evento_v1.00.xsd");

    /// <summary>A DPS 1.01, a que a lib escreve.</summary>
    internal static SchemaXml Dps => SchemaDps.Value;

    /// <summary>A NFS-e 1.01.</summary>
    internal static SchemaXml Nota => SchemaNota.Value;

    /// <summary>O pedido de registro de evento 1.01.</summary>
    internal static SchemaXml Pedido => SchemaPedido.Value;

    /// <summary>O evento registrado 1.01.</summary>
    internal static SchemaXml Evento => SchemaEvento.Value;

    /// <summary>A DPS 1.00.</summary>
    internal static SchemaXml Dps100 => SchemaDps100.Value;

    /// <summary>A NFS-e 1.00.</summary>
    internal static SchemaXml Nota100 => SchemaNota100.Value;

    /// <summary>O pedido de registro de evento 1.00.</summary>
    internal static SchemaXml Pedido100 => SchemaPedido100.Value;

    /// <summary>O evento registrado 1.00.</summary>
    internal static SchemaXml Evento100 => SchemaEvento100.Value;

    /// <summary>O nome do recurso de um arquivo do pacote (<c>"Schemas/NFSe-.../1.01/DPS_v1.01.xsd"</c>).</summary>
    internal static string Recurso(string versao, string arquivo) => $"Schemas/{Pacote}/{versao}/{arquivo}";

    private static Lazy<SchemaXml> Preparar(string versao, string arquivo) =>
        new(() => SchemaXml.Carregar(typeof(SchemasNFSe101).Assembly, Recurso(versao, arquivo), Codigo, CaminhoModeloNFSe.Propriedade));
}
