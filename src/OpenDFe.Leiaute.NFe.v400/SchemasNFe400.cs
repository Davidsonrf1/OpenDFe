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
    private static readonly Lazy<SchemaXml> SchemaInutilizacao = Preparar("inutNFe_v4.00.xsd", CodigoMensagem, PropriedadeInutilizacao);
    private static readonly Lazy<SchemaXml> SchemaInutilizacaoProcessada = Preparar("procInutNFe_v4.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaEvento = Preparar("envEvento_v1.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaEventoProcessado = Preparar("procEventoNFe_v1.00.xsd", CodigoMensagem);
    private static readonly Lazy<SchemaXml> SchemaDetalheCancelamento =
        Preparar("e110111_v1.00.xsd", CodigoDetalhe, c => Detalhe(c, "nProt", "Protocolo", "xJust", "Justificativa"));
    private static readonly Lazy<SchemaXml> SchemaDetalheSubstituicao =
        Preparar("e110112_v1.00.xsd", CodigoDetalhe, c => Detalhe(c, "verAplic", "VersaoAplicativo", "nProt", "Protocolo", "xJust", "Justificativa", "chNFeRef", "ChaveSubstituta"));
    private static readonly Lazy<SchemaXml> SchemaDetalheCartaCorrecao =
        Preparar("e110110_v1.00.xsd", CodigoDetalhe, c => Detalhe(c, "xCorrecao", "Correcao"));
    private static readonly Lazy<SchemaXml> SchemaDistribuicao = Preparar("distDFeInt_v1.01.xsd", CodigoMensagem);
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

    /// <summary>O pedido da distribuição DF-e (<c>distDFeInt</c>, <c>NFeDistribuicaoDFe</c>).</summary>
    internal static SchemaXml DistribuicaoPedido => SchemaDistribuicao.Value;

    /// <summary>A nota com o protocolo (<c>nfeProc</c>), a distribuível.</summary>
    internal static SchemaXml NotaProcessada => SchemaNotaProcessada.Value;

    /// <summary>Falha no schema do detalhe do evento (<c>detEvento</c>, D06).</summary>
    internal const string CodigoDetalhe = "493";

    /// <summary>O pedido de eventos (<c>envEvento</c>, <c>NFeRecepcaoEvento4</c>).</summary>
    internal static SchemaXml Evento => SchemaEvento.Value;

    /// <summary>O evento com o registro (<c>procEventoNFe</c>), o distribuível.</summary>
    internal static SchemaXml EventoProcessado => SchemaEventoProcessado.Value;

    /// <summary>O <c>detEvento</c> do cancelamento (110111).</summary>
    internal static SchemaXml DetalheCancelamento => SchemaDetalheCancelamento.Value;

    /// <summary>O <c>detEvento</c> do cancelamento por substituição (110112).</summary>
    internal static SchemaXml DetalheSubstituicao => SchemaDetalheSubstituicao.Value;

    /// <summary>O <c>detEvento</c> da carta de correção (110110).</summary>
    internal static SchemaXml DetalheCartaCorrecao => SchemaDetalheCartaCorrecao.Value;

    /// <summary>A inutilização com o registro (<c>ProcInutNFe</c>), a distribuível.</summary>
    internal static SchemaXml InutilizacaoProcessada => SchemaInutilizacaoProcessada.Value;

    /// <summary>O nome do recurso de um arquivo do pacote (<c>"Schemas/PL_010f_v1.04/nfe_v4.00.xsd"</c>).</summary>
    internal static string Recurso(string arquivo) => $"Schemas/{Pacote}/{arquivo}";

    private static Lazy<SchemaXml> Preparar(string arquivo, string codigo) =>
        new(() => SchemaXml.Carregar(typeof(SchemasNFe400).Assembly, Recurso(arquivo), codigo));

    // A nota e a nota processada: o problema do schema aponta a propriedade do modelo (docs/MODELO.md 14).
    private static Lazy<SchemaXml> Preparar(string arquivo, string codigo, Func<string, string?> propriedade) =>
        new(() => SchemaXml.Carregar(typeof(SchemasNFe400).Assembly, Recurso(arquivo), codigo, propriedade));

    // O detEvento sozinho: "detEvento/xJust" vira a propriedade do evento.
    private static string? Detalhe(string caminho, params string[] tagsEPropriedades)
    {
        var passos = caminho.Split('/');
        if (passos.Length != 2 || passos[0] != "detEvento")
            return null;
        for (var i = 0; i < tagsEPropriedades.Length; i += 2)
            if (passos[1] == tagsEPropriedades[i])
                return tagsEPropriedades[i + 1];
        return null;
    }

    // O inutNFe: o campo do pedido na propriedade da Inutilizacao.
    private static string? PropriedadeInutilizacao(string caminho)
    {
        var passos = caminho.Split('/');
        if (passos.Length != 3 || passos[0] != "inutNFe" || passos[1] != "infInut")
            return null;
        return passos[2] switch
        {
            "ano" => "Ano",
            "CNPJ" => "Cnpj",
            "serie" => "Serie",
            "nNFIni" => "NumeroInicial",
            "nNFFin" => "NumeroFinal",
            "xJust" => "Justificativa",
            _ => null,
        };
    }
}
