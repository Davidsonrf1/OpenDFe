namespace OpenDFe.NFSe;

/// <summary>A confirmação tácita (<c>e205204</c>), do ADN: a lib só a lê.</summary>
[Tag("e205204")]
public sealed class ConfirmacaoTacita : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"205204"</c>.</summary>
    public override string Tipo => "205204";
}
