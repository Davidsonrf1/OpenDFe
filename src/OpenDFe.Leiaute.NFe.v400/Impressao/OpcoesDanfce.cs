using OpenDFe.Impressao;

namespace OpenDFe.NFe;

/// <summary>A via do DANFE NFC-e (a contingência exige duas: a do consumidor e a do estabelecimento).</summary>
public enum ViaDanfce
{
    /// <summary>A via do consumidor.</summary>
    Consumidor,

    /// <summary>A via do estabelecimento, que fica à disposição do Fisco até a NFC-e ser autorizada.</summary>
    Estabelecimento,
}

/// <summary>As opções do DANFCE (<see cref="LeiauteNFe400.Danfce"/>).</summary>
/// <remarks>
/// Imutável. A lib não decide quantas vias imprimir: quem imprime chama <see cref="LeiauteNFe400.Danfce"/> uma vez por via
/// (docs/IMPRESSAO.md 1).
/// </remarks>
public sealed class OpcoesDanfce
{
    /// <summary>
    /// O papel: <see cref="PapelImpressao.Bobina80"/> (padrão) ou <see cref="PapelImpressao.Bobina58"/>; outro papel é
    /// recusado pelo <see cref="LeiauteNFe400.Danfce"/>.
    /// </summary>
    public PapelImpressao Papel { get; init; } = PapelImpressao.Bobina80;

    /// <summary>O logotipo do emitente, no topo; <see langword="null"/> sem logotipo.</summary>
    public Imagem1Bit? Logotipo { get; init; }

    /// <summary>A marca do software emissor, no rodapé; <see langword="null"/> sem marca.</summary>
    public Imagem1Bit? AssinaturaSoftware { get; init; }

    /// <summary>
    /// Uma linha livre depois do conteúdo da NFC-e, como o manual manda para o que não está no XML (por exemplo,
    /// <c>"ERP Core - DLA Tec"</c>); <see langword="null"/> sem a linha.
    /// </summary>
    public string? TextoRodape { get; init; }

    /// <summary>A via; <see cref="ViaDanfce.Consumidor"/> por padrão.</summary>
    public ViaDanfce Via { get; init; } = ViaDanfce.Consumidor;
}
