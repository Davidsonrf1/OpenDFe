using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O cancelamento (110111, MOC 7.0, Visão Geral, 5.9): o autor é o emitente da chave. O prazo (501), o protocolo (222) e
/// a situação da nota ficam para a SEFAZ.
/// </summary>
public sealed class Cancelamento : Evento
{
    private const int TamanhoProtocolo = 15;

    /// <summary>O cancelamento da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da nota.</param>
    /// <param name="protocolo">O <c>nProt</c> da autorização (<see cref="NFe.Protocolo.Numero"/>), 15 dígitos.</param>
    /// <param name="justificativa">A justificativa; o tamanho (15 a 255) e a pontuação são do schema, na montagem.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">O protocolo não tem 15 dígitos; a justificativa é nula ou vazia.</exception>
    public Cancelamento(ChaveAcesso chave, string protocolo, string justificativa)
        : base(chave, TipoEvento.Cancelamento, 1)
    {
        if (protocolo is null || protocolo.Length != TamanhoProtocolo || !protocolo.All(char.IsAsciiDigit))
            throw new ArgumentException("O protocolo da autorização tem 15 dígitos.", nameof(protocolo));
        if (string.IsNullOrEmpty(justificativa))
            throw new ArgumentException("A justificativa do cancelamento é obrigatória.", nameof(justificativa));
        Protocolo = protocolo;
        Justificativa = justificativa;
    }

    /// <summary>O número do protocolo da autorização (<c>nProt</c>).</summary>
    public string Protocolo { get; }

    /// <summary>A justificativa (<c>xJust</c>).</summary>
    public string Justificativa { get; }

    internal override string Descricao => "Cancelamento";

    // O autor é o emitente da chave: o CNPJ, ou o CPF (a chave o guarda com três zeros à esquerda).
    internal override string Autor
    {
        get
        {
            var emitente = Chave.Emitente;
            return Cnpj.Valido(emitente) || !emitente.StartsWith("000", StringComparison.Ordinal) ? emitente : emitente[3..];
        }
    }

    internal override bool AceitaContingencia => true;

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheCancelamento;

    internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos =>
        [("xJust", nameof(Justificativa), Justificativa)];

    internal override void EscreverDetalhe(XmlWriter xml)
    {
        xml.WriteStartElement("detEvento", EscritaNFe.Namespace);
        xml.WriteAttributeString("versao", VersaoDetalhe);
        xml.WriteElementString("descEvento", EscritaNFe.Namespace, Descricao);
        xml.WriteElementString("nProt", EscritaNFe.Namespace, Protocolo);
        xml.WriteElementString("xJust", EscritaNFe.Namespace, Justificativa);
        xml.WriteEndElement();
    }

    internal override string? PropriedadeDetalhe(string passo) => passo switch
    {
        "nProt" => nameof(Protocolo),
        "xJust" => nameof(Justificativa),
        _ => null,
    };
}
