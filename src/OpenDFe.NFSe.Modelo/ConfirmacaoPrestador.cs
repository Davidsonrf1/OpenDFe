namespace OpenDFe.NFSe;

/// <summary>A confirmação do prestador (<c>e202201</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e202201")]
public sealed class ConfirmacaoPrestador : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"202201"</c>.</summary>
    public override string Tipo => "202201";
}
