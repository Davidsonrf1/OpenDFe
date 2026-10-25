using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// Um evento de manifestação do destinatário (NT 2020.001 v1.60, MOC 7.0, Visão Geral, 5.11, docs/CONTRATOS.md 24): a ciência
/// (<see cref="CienciaOperacao"/>), a confirmação (<see cref="ConfirmacaoOperacao"/>), o desconhecimento
/// (<see cref="DesconhecimentoOperacao"/>) e a operação não realizada (<see cref="OperacaoNaoRealizada"/>). A base é fechada
/// (construtor <c>private protected</c>).
/// </summary>
/// <remarks>
/// O autor não é o emitente: é o destinatário da nota, que o chamador informa (<see cref="Autor"/>); a assinatura é do
/// certificado do núcleo, e a conferência da raiz do CNPJ ou do CPF é da SEFAZ. A manifestação vai sempre ao Ambiente
/// Nacional (<see cref="OpenDFe.Destino.AN"/>, <c>cOrgao</c> 91), qualquer que seja a UF ou o tipo de emissão da chave, e só
/// existe para a NF-e (modelo 55). Os prazos (596), a sequência (594), a ordem das manifestações (655), a nota cancelada
/// (650, 651) e o destinatário (575) ficam para a SEFAZ.
/// </remarks>
public abstract class Manifestacao : Evento
{
    /// <summary>O código do Ambiente Nacional no <c>cOrgao</c>.</summary>
    private const int OrgaoNacional = 91;

    private const int MaximoSequencia = 2;

    private protected Manifestacao(ChaveAcesso chave, TipoEvento tipo, string autor, int sequencia)
        : base(chave, tipo, sequencia)
    {
        if (chave.Modelo != Documento.NFe)
            throw new ArgumentException("A manifestação do destinatário é só da NF-e (modelo 55).", nameof(chave));
        if (autor is null || !(Cnpj.Valido(autor) || Cpf.Valido(autor)))
            throw new ArgumentException("O autor da manifestação não é um CNPJ nem um CPF válido.", nameof(autor));
        if (sequencia is < 1 or > MaximoSequencia)
            throw new ArgumentOutOfRangeException(nameof(sequencia), "A sequência da manifestação vai de 1 a 2 (a segunda é a retificação).");
        Autor = Cnpj.Valido(autor) ? Cnpj.RemoverMascara(autor) : Cpf.RemoverMascara(autor);
    }

    /// <summary>O CNPJ (numérico ou alfanumérico) ou o CPF do destinatário que manifesta, só com os algarismos e as letras.</summary>
    public new string Autor { get; }

    internal override string? AutorInformado => Autor;

    internal override int Orgao(ContextoOperacao contexto) => OrgaoNacional;

    internal override Destino Destino(ContextoOperacao contexto) => OpenDFe.Destino.AN;

    internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos => [];

    /// <summary>Os campos do <c>detEvento</c> depois do <c>descEvento</c>; nenhum, salvo na operação não realizada.</summary>
    internal virtual void EscreverCampos(XmlWriter xml)
    {
    }

    internal override void EscreverDetalhe(XmlWriter xml)
    {
        xml.WriteStartElement("detEvento", EscritaNFe.Namespace);
        xml.WriteAttributeString("versao", VersaoDetalhe);
        xml.WriteElementString("descEvento", EscritaNFe.Namespace, Descricao);
        EscreverCampos(xml);
        xml.WriteEndElement();
    }

    internal override string? PropriedadeDetalhe(string passo) => null;
}

/// <summary>
/// A ciência da operação (210210): o destinatário declara que soube da nota, e isso libera o download da NF-e completa na
/// distribuição DF-e (docs/CONTRATOS.md 23). A sequência é sempre 1.
/// </summary>
public sealed class CienciaOperacao : Manifestacao
{
    /// <summary>A ciência da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da NF-e.</param>
    /// <param name="autor">O CNPJ ou o CPF do destinatário, com ou sem máscara.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido.</exception>
    public CienciaOperacao(ChaveAcesso chave, string autor)
        : base(chave, TipoEvento.CienciaOperacao, autor, 1)
    {
    }

    internal override string Descricao => "Ciencia da Operacao";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheCiencia;
}

/// <summary>
/// A confirmação da operação (210200). A sequência é 1, ou 2 na retificação (o Ajuste SINIEF 43/23 permite uma); depois
/// dela, a nota não pode mais ser cancelada (221).
/// </summary>
public sealed class ConfirmacaoOperacao : Manifestacao
{
    /// <summary>A confirmação da operação da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da NF-e.</param>
    /// <param name="autor">O CNPJ ou o CPF do destinatário, com ou sem máscara.</param>
    /// <param name="sequencia">1, ou 2 na retificação.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A sequência está fora de 1 a 2.</exception>
    public ConfirmacaoOperacao(ChaveAcesso chave, string autor, int sequencia = 1)
        : base(chave, TipoEvento.ConfirmacaoOperacao, autor, sequencia)
    {
    }

    internal override string Descricao => "Confirmacao da Operacao";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheConfirmacao;
}

/// <summary>O desconhecimento da operação (210220). A sequência é 1, ou 2 na retificação.</summary>
public sealed class DesconhecimentoOperacao : Manifestacao
{
    /// <summary>O desconhecimento da operação da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da NF-e.</param>
    /// <param name="autor">O CNPJ ou o CPF do destinatário, com ou sem máscara.</param>
    /// <param name="sequencia">1, ou 2 na retificação.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A sequência está fora de 1 a 2.</exception>
    public DesconhecimentoOperacao(ChaveAcesso chave, string autor, int sequencia = 1)
        : base(chave, TipoEvento.DesconhecimentoOperacao, autor, sequencia)
    {
    }

    internal override string Descricao => "Desconhecimento da Operacao";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheDesconhecimento;
}

/// <summary>A operação não realizada (210240), com a justificativa. A sequência é 1, ou 2 na retificação.</summary>
public sealed class OperacaoNaoRealizada : Manifestacao
{
    /// <summary>A operação não realizada da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da NF-e.</param>
    /// <param name="autor">O CNPJ ou o CPF do destinatário, com ou sem máscara.</param>
    /// <param name="justificativa">A justificativa; o tamanho (15 a 255) e a pontuação são do schema, na montagem.</param>
    /// <param name="sequencia">1, ou 2 na retificação.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="ArgumentException">
    /// A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido; a justificativa é nula ou vazia.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">A sequência está fora de 1 a 2.</exception>
    public OperacaoNaoRealizada(ChaveAcesso chave, string autor, string justificativa, int sequencia = 1)
        : base(chave, TipoEvento.OperacaoNaoRealizada, autor, sequencia)
    {
        if (string.IsNullOrEmpty(justificativa))
            throw new ArgumentException("A justificativa da operação não realizada é obrigatória.", nameof(justificativa));
        Justificativa = justificativa;
    }

    /// <summary>A justificativa (<c>xJust</c>).</summary>
    public string Justificativa { get; }

    internal override string Descricao => "Operacao nao Realizada";

    internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheNaoRealizada;

    internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos =>
        [("xJust", nameof(Justificativa), Justificativa)];

    internal override void EscreverCampos(XmlWriter xml) =>
        xml.WriteElementString("xJust", EscritaNFe.Namespace, Justificativa);

    internal override string? PropriedadeDetalhe(string passo) => passo == "xJust" ? nameof(Justificativa) : null;
}
