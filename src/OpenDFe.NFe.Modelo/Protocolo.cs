namespace OpenDFe.NFe;

/// <summary>
/// O protocolo do processamento de uma NF-e pela SEFAZ (<c>protNFe</c>, com o <c>infProt</c> achatado): autorizada,
/// denegada ou rejeitada.
/// </summary>
/// <remarks>
/// Vem no <c>nfeProc</c> (a leitura, docs/LEITURA.md) e, na fase 4, no retorno da autorização, da consulta do recibo e
/// da consulta do protocolo. Segue as regras de forma do modelo (MODELO.md 3): a leitura preenche campo a campo, e o
/// valor ilegível fica nulo. A <c>versao</c> do <c>protNFe</c> e a <c>Signature</c> (opcional) ficam fora, como as da
/// nota.
/// </remarks>
public sealed class Protocolo
{
    /// <summary>O identificador do <c>infProt</c>, quando a SEFAZ assina a resposta (<c>ID</c> + o número do protocolo). Opcional.</summary>
    [Tag("Id", "PR04")]
    public string? Id { get; set; }

    /// <summary>O ambiente em que a nota foi processada.</summary>
    [Tag("tpAmb", "PR05")]
    public Ambiente? Ambiente { get; set; }

    /// <summary>A versão do aplicativo da SEFAZ que processou a nota.</summary>
    [Tag("verAplic", "PR06")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>A chave de acesso da nota, como texto (MODELO.md, decisão 7).</summary>
    [Tag("chNFe", "PR07")]
    public string? Chave { get; set; }

    /// <summary>A data e a hora do processamento (também na rejeição).</summary>
    [Tag("dhRecbto", "PR08")]
    public DateTimeOffset? Recebimento { get; set; }

    /// <summary>O número do protocolo (15 dígitos); ausente na rejeição.</summary>
    [Tag("nProt", "PR09")]
    public string? Numero { get; set; }

    /// <summary>O <c>DigestValue</c> da nota processada, em base64: confere a integridade da nota original.</summary>
    [Tag("digVal", "PR10")]
    public string? Digest { get; set; }

    /// <summary>O código do status da nota: 100 e 150 autorizada; 110, 301 e 302 denegada; ou o da rejeição.</summary>
    [Tag("cStat", "PR11")]
    public int? Status { get; set; }

    /// <summary>A descrição do status.</summary>
    [Tag("xMotivo", "PR12")]
    public string? Motivo { get; set; }

    /// <summary>As mensagens da SEFAZ ao emissor: a sequência <c>cMsg</c> + <c>xMsg</c> (PR13), até 5.</summary>
    public IList<MensagemSefaz> Mensagens { get; } = new List<MensagemSefaz>();
}
