using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// A carta de correção (110110, MOC 7.0, Visão Geral, 5.10) de uma NF-e; a NFC-e não tem. O texto da condição de uso é
/// fixo (o schema o enumera). O autor é o emitente da chave. Os prazos, a sequência acima da registrada e o que a carta
/// pode corrigir ficam para a SEFAZ (o 580 de nota cancelada, o 577 de data antes da emissão).
/// </summary>
public sealed class CartaCorrecao : Evento
{
    // A última sequência que a SEFAZ aceita (nSeqEvento de 1 a 20).
    private const int MaximoSequencia = 20;

    // O texto de e110110_v1.00.xsd, na forma sem acentos (a que o MOC mostra); um teste o confere com o schema.
    internal const string CondicaoUso =
        "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.";

    /// <summary>A carta de correção da NF-e da chave.</summary>
    /// <param name="chave">A chave de acesso da NF-e.</param>
    /// <param name="sequencia">
    /// O número da carta nesta nota, de 1 a 20 (<c>nSeqEvento</c>): cada carta nova tem o número seguinte. A lib não sabe
    /// qual é o próximo; conte as cartas que a consulta da nota traz.
    /// </param>
    /// <param name="correcao">O texto da correção; o tamanho (15 a 1000) e os caracteres são do schema, na montagem.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">A chave não é de NF-e (modelo 55); a correção é nula ou vazia.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A sequência está fora de 1 a 20.</exception>
    public CartaCorrecao(ChaveAcesso chave, int sequencia, string correcao)
        : base(chave, TipoEvento.CartaCorrecao, sequencia)
    {
        if (chave.Modelo != Documento.NFe)
            throw new ArgumentException("A carta de correção é só da NF-e (modelo 55).", nameof(chave));
        if (sequencia is < 1 or > MaximoSequencia)
            throw new ArgumentOutOfRangeException(nameof(sequencia), "A sequência da carta de correção vai de 1 a 20.");
        if (string.IsNullOrEmpty(correcao))
            throw new ArgumentException("A correção é obrigatória.", nameof(correcao));
        Correcao = correcao;
    }

    /// <summary>O texto da correção (<c>xCorrecao</c>).</summary>
    public string Correcao { get; }

    internal override string Descricao => "Carta de Correcao";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheCartaCorrecao;

    internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos =>
        [("xCorrecao", nameof(Correcao), Correcao)];

    internal override void EscreverDetalhe(XmlWriter xml)
    {
        xml.WriteStartElement("detEvento", EscritaNFe.Namespace);
        xml.WriteAttributeString("versao", VersaoDetalhe);
        xml.WriteElementString("descEvento", EscritaNFe.Namespace, Descricao);
        xml.WriteElementString("xCorrecao", EscritaNFe.Namespace, Correcao);
        xml.WriteElementString("xCondUso", EscritaNFe.Namespace, CondicaoUso);
        xml.WriteEndElement();
    }

    internal override string? PropriedadeDetalhe(string passo) => passo == "xCorrecao" ? nameof(Correcao) : null;
}
