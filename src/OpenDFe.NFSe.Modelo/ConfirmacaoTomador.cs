namespace OpenDFe.NFSe;

/// <summary>A confirmação do tomador (<c>e203202</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e203202")]
public sealed class ConfirmacaoTomador : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"203202"</c>.</summary>
    public override string Tipo => "203202";
}
