namespace OpenDFe.NFe;

/// <summary>As informações suplementares da NFC-e (<c>infNFeSupl</c>).</summary>
/// <remarks>
/// A leitura preenche. Na escrita, a lib gera o grupo na NFC-e e na NF-e com o DANFE Simplificado Tipo 2 (<c>tpImp</c> 6)
/// a partir da chave, do ambiente e da <c>ConfiguracaoNFCe</c> (etapa 5.1): o grupo preenchido igual ao gerado passa, e
/// o diferente é erro (<c>NFE-NUCLEO</c>). Na NF-e sem <c>tpImp</c> 6, o grupo preenchido sai como está e é a regra 393.
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
