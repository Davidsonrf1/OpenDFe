namespace OpenDFe.NFe;

/// <summary>
/// A URL de consulta da NFC-e de uma UF num ambiente: uma linha da tabela do leiaute
/// (<see cref="LeiauteNFe400.UrlsConsulta"/>, docs/NTS.md 4). Imutável.
/// </summary>
public sealed class UrlConsultaNFCe
{
    internal UrlConsultaNFCe(UF uf, Ambiente ambiente, Uri qrCode, Uri chave)
    {
        UF = uf;
        Ambiente = ambiente;
        QrCode = qrCode;
        Chave = chave;
    }

    /// <summary>A UF.</summary>
    public UF UF { get; }

    /// <summary>O ambiente.</summary>
    public Ambiente Ambiente { get; }

    /// <summary>O endereço da consulta pelo QR Code, sem a query: a lib acrescenta o <c>?p=</c>.</summary>
    public Uri QrCode { get; }

    /// <summary>O endereço da consulta pela chave de acesso, como vai no <c>urlChave</c>.</summary>
    public Uri Chave { get; }
}
