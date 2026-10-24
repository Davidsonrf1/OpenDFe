using System.Globalization;
using System.Reflection;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O cancelamento por substituição (110112, MOC 7.0, Visão Geral, 5.9) de uma NFC-e: cancela a NFC-e e aponta a que a
/// substitui. O autor é o emitente da chave. A janela de 168 horas (501), a substituta que não confere (911 a 919), a
/// nota fora da emissão normal (920, 921) e a situação da nota ficam para a SEFAZ.
/// </summary>
public sealed class CancelamentoSubstituicao : Evento
{
    private const int TamanhoProtocolo = 15;
    private const int TamanhoVersao = 20;

    private static readonly string VersaoPadrao = Cortar("OpenDFe " + VersaoPacote());

    private readonly string _versaoAplicativo = VersaoPadrao;

    /// <summary>O cancelamento por substituição da NFC-e da chave.</summary>
    /// <param name="chave">A chave de acesso da NFC-e cancelada.</param>
    /// <param name="protocolo">O <c>nProt</c> da autorização dela (<see cref="NFe.Protocolo.Numero"/>), 15 dígitos.</param>
    /// <param name="justificativa">A justificativa; o tamanho (15 a 255) e a pontuação são do schema, na montagem.</param>
    /// <param name="chaveSubstituta">A chave de acesso da NFC-e que substitui a cancelada (<c>chNFeRef</c>).</param>
    /// <exception cref="ArgumentNullException">Alguma das chaves é nula.</exception>
    /// <exception cref="ArgumentException">
    /// Alguma das chaves não é de NFC-e (modelo 65); as duas chaves são iguais; o protocolo não tem 15 dígitos; a
    /// justificativa é nula ou vazia.
    /// </exception>
    public CancelamentoSubstituicao(ChaveAcesso chave, string protocolo, string justificativa, ChaveAcesso chaveSubstituta)
        : base(chave, TipoEvento.CancelamentoSubstituicao, 1)
    {
        ArgumentNullException.ThrowIfNull(chaveSubstituta);
        if (chave.Modelo != Documento.NFCe)
            throw new ArgumentException("O cancelamento por substituição é só da NFC-e (modelo 65).", nameof(chave));
        if (chaveSubstituta.Modelo != Documento.NFCe)
            throw new ArgumentException("A nota substituta é uma NFC-e (modelo 65).", nameof(chaveSubstituta));
        if (chave == chaveSubstituta)
            throw new ArgumentException("A nota substituta não pode ser a própria nota cancelada.", nameof(chaveSubstituta));
        if (protocolo is null || protocolo.Length != TamanhoProtocolo || !protocolo.All(char.IsAsciiDigit))
            throw new ArgumentException("O protocolo da autorização tem 15 dígitos.", nameof(protocolo));
        if (string.IsNullOrEmpty(justificativa))
            throw new ArgumentException("A justificativa do cancelamento é obrigatória.", nameof(justificativa));
        Protocolo = protocolo;
        Justificativa = justificativa;
        ChaveSubstituta = chaveSubstituta;
    }

    /// <summary>O número do protocolo da autorização da NFC-e cancelada (<c>nProt</c>).</summary>
    public string Protocolo { get; }

    /// <summary>A justificativa (<c>xJust</c>).</summary>
    public string Justificativa { get; }

    /// <summary>A chave de acesso da NFC-e substituta (<c>chNFeRef</c>).</summary>
    public ChaveAcesso ChaveSubstituta { get; }

    /// <summary>
    /// O <c>verAplic</c> do <c>detEvento</c> (1 a 20 caracteres): a versão do aplicativo que emite. Padrão: "OpenDFe " e a
    /// versão do pacote, cortada em 20; quem usa troca pela do seu sistema. O que passa de 20 caracteres é cortado; o
    /// nulo volta ao padrão.
    /// </summary>
    public string VersaoAplicativo
    {
        get => _versaoAplicativo;
        init => _versaoAplicativo = value is null ? VersaoPadrao : Cortar(value);
    }

    private static string Cortar(string valor) => valor.Length > TamanhoVersao ? valor[..TamanhoVersao] : valor;

    // A versão do pacote (a informacional sem o "+<commit>"; a do assembly, se não houver).
    private static string VersaoPacote()
    {
        var assembly = typeof(CancelamentoSubstituicao).Assembly;
        var informacional = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrEmpty(informacional))
            return informacional.Split('+')[0];
        return assembly.GetName().Version?.ToString(3) ?? "0";
    }

    internal override string Descricao => "Cancelamento por substituicao";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheSubstituicao;

    internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos =>
        [("xJust", nameof(Justificativa), Justificativa), ("verAplic", nameof(VersaoAplicativo), VersaoAplicativo)];

    internal override void EscreverDetalhe(XmlWriter xml)
    {
        xml.WriteStartElement("detEvento", EscritaNFe.Namespace);
        xml.WriteAttributeString("versao", VersaoDetalhe);
        xml.WriteElementString("descEvento", EscritaNFe.Namespace, Descricao);
        xml.WriteElementString("cOrgaoAutor", EscritaNFe.Namespace, Chave.UF.Codigo().ToString(CultureInfo.InvariantCulture));
        xml.WriteElementString("tpAutor", EscritaNFe.Namespace, "1");
        xml.WriteElementString("verAplic", EscritaNFe.Namespace, VersaoAplicativo);
        xml.WriteElementString("nProt", EscritaNFe.Namespace, Protocolo);
        xml.WriteElementString("xJust", EscritaNFe.Namespace, Justificativa);
        xml.WriteElementString("chNFeRef", EscritaNFe.Namespace, ChaveSubstituta.ToString());
        xml.WriteEndElement();
    }

    internal override string? PropriedadeDetalhe(string passo) => passo switch
    {
        "verAplic" => nameof(VersaoAplicativo),
        "nProt" => nameof(Protocolo),
        "xJust" => nameof(Justificativa),
        "chNFeRef" => nameof(ChaveSubstituta),
        _ => null,
    };
}
