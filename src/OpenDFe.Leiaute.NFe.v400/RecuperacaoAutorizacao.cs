namespace OpenDFe.NFe;

/// <summary>
/// A recuperação da autorização cuja resposta se perdeu (PROJETO.md 8.6, docs/CONTRATOS.md 15): consulta a chave da
/// nota assinada (<c>NFeConsultaProtocolo4</c>) e, achado o protocolo de autorização ou de denegação daquele XML, monta
/// o <c>nfeProc</c>. Não reenvia nada.
/// </summary>
/// <remarks>
/// <para>
/// O retorno é o da <see cref="Autorizacao"/>, para quem usa tratar os dois com o mesmo código: o
/// <see cref="RetornoAutorizacao.Status"/> é o da consulta (100 ou 150 autorizada, 217 a nota não consta: reenviar o
/// mesmo XML), sem recibo, e uma <see cref="ResultadoNota"/> com o XML, o protocolo e o <c>nfeProc</c>.
/// </para>
/// <para>
/// O <c>nfeProc</c> só sai se o <c>digVal</c> do protocolo é o <c>DigestValue</c> do XML (decisão 8): diferente, a base
/// tem outro XML com a mesma chave, e o protocolo vem sem o <c>nfeProc</c>, com o aviso <c>NFE-PROTOCOLO</c>. A
/// consulta vai ao autorizador da chave; uma nota por recuperação (decisão 7). O guia está em docs/QUANDO_CAI.md.
/// </para>
/// </remarks>
public sealed class RecuperacaoAutorizacao : ParametroWS<RetornoAutorizacao>
{
    /// <summary>A recuperação da nota assinada.</summary>
    /// <param name="xml">
    /// O <c>NFe</c> assinado, como foi enviado (o <see cref="ResultadoNota.Xml"/>, o <see cref="DocumentoPreparado.Xml"/>
    /// ou uma das <see cref="Autorizacao.NotasEnviadas"/> do rastro).
    /// </param>
    /// <exception cref="ArgumentNullException">O XML é nulo.</exception>
    /// <exception cref="ArgumentException">O XML é vazio.</exception>
    public RecuperacaoAutorizacao(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("A nota assinada é vazia.", nameof(xml));
        Xml = xml;
    }

    /// <summary>A nota assinada.</summary>
    public string Xml { get; }
}
