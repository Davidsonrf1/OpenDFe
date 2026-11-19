namespace OpenDFe.NFSe;

/// <summary>A confirmação do intermediário (<c>e204203</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e204203")]
public sealed class ConfirmacaoIntermediario : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"204203"</c>.</summary>
    public override string Tipo => "204203";
}
