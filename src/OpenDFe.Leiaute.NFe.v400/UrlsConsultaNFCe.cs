using System.Collections.ObjectModel;

namespace OpenDFe.NFe;

/// <summary>
/// A tabela das URLs de consulta da NFC-e por UF e ambiente (a do QR Code, sem a query, e a da consulta pela chave, o
/// <c>urlChave</c>). Só as linhas conferidas em fonte primária entram (docs/NTS.md 4): a UF e o ambiente que não
/// constam não têm URL aqui, e quem usa a informa na <see cref="ConfiguracaoNFCe"/>; a montagem recusa a nota que
/// ficaria sem ela (<c>NFE-QRCODE</c>). Copiada das fontes em 2026-10-24.
/// </summary>
internal static class UrlsConsultaNFCe
{
    // MG: "Web Services" da SEF-MG (portalsped.fazenda.mg.gov.br/spedmg/nfce/web-services/), conferido em 2026-10-24.
    // A página lista o mesmo endereço do QR Code nos dois ambientes e o da consulta pela chave de cada um.
    private static readonly (UF UF, Ambiente Ambiente, string QrCode, string Chave)[] Linhas =
    [
        (UF.MG, Ambiente.Producao,
            "https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml",
            "https://portalsped.fazenda.mg.gov.br/portalnfce"),
        (UF.MG, Ambiente.Homologacao,
            "https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml",
            "https://hportalsped.fazenda.mg.gov.br/portalnfce"),
    ];

    /// <summary>A tabela, na ordem das linhas.</summary>
    internal static ReadOnlyCollection<UrlConsultaNFCe> Todas { get; } = Array.AsReadOnly(
        [.. Linhas.Select(l => new UrlConsultaNFCe(l.UF, l.Ambiente, new Uri(l.QrCode), new Uri(l.Chave)))]);

    /// <summary>A linha da UF e do ambiente; nula quando a tabela não a tem.</summary>
    internal static UrlConsultaNFCe? Procurar(UF uf, Ambiente ambiente) =>
        Todas.FirstOrDefault(l => l.UF == uf && l.Ambiente == ambiente);
}
