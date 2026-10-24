namespace OpenDFe.NFe;

/// <summary>
/// O tipo de um documento distribuído, pelo <c>schema</c> do <c>docZip</c> (NT 2014.002, 3.2): o prefixo do nome do
/// arquivo (<c>resNFe_</c>, <c>procNFe_</c>, <c>resEvento_</c>, <c>procEventoNFe_</c>), sem a versão.
/// </summary>
public enum TipoDocumentoDistribuido
{
    /// <summary>O resumo da NF-e (<c>resNFe</c>), que o destinatário recebe antes de se manifestar.</summary>
    ResumoNFe = 0,

    /// <summary>A NF-e com o protocolo de autorização (<c>nfeProc</c>).</summary>
    NFeProcessada = 1,

    /// <summary>O resumo de um evento (<c>resEvento</c>).</summary>
    ResumoEvento = 2,

    /// <summary>O evento com o registro (<c>procEventoNFe</c>).</summary>
    EventoProcessado = 3,

    /// <summary>Um <c>schema</c> que a lib não conhece (o evento de outro documento, um leiaute novo): o XML vem em <see cref="DocumentoDistribuido.Xml"/>.</summary>
    Outro = 4,
}
