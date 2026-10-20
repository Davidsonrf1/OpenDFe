namespace OpenDFe.NFe;

/// <summary>As informações suplementares da NFC-e (<c>infNFeSupl</c>).</summary>
/// <remarks>
/// A leitura preenche; a escrita escreve como está até a etapa 5.1, quando o leiaute passa a gerá-las a partir do CSC
/// da configuração.
/// </remarks>
public sealed class InformacoesSuplementares
{
    /// <summary>O texto do QR Code.</summary>
    [Tag("qrCode", "ZX02")]
    public string? QrCode { get; set; }

    /// <summary>A URL da consulta pela chave de acesso.</summary>
    [Tag("urlChave", "ZX03")]
    public string? UrlChave { get; set; }
}
