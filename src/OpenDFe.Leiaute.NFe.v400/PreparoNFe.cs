namespace OpenDFe.NFe;

/// <summary>
/// O preparo da nota (<see cref="DFe.Preparar{TDocumento}"/>, docs/CONTRATOS.md 14): a nota escrita com os valores do
/// núcleo, para o núcleo assinar e validar sem transmitir. A propriedade dos problemas é a da nota, sem prefixo. Só a
/// NF-e nesta versão: a NFC-e entra com o QR Code (decisão 12).
/// </summary>
internal sealed class PreparoNFe : IPreparo<NotaFiscal>
{
    public DocumentoMontado Montar(NotaFiscal documento, ContextoOperacao contexto)
    {
        var escrita = EscritorNFe.Escrever(documento, ValoresNucleo.Criar(contexto));
        // Sem a chave, a escrita já deu erro: o Id é só para o documento existir, e nada é assinado.
        var id = escrita.Id ?? "NFe";
        return new DocumentoMontado(escrita.Xml, SchemasNFe400.Nota, new DocumentoPedido(id)
        {
            Modelo = documento,
            Chave = escrita.Chave,
            Propriedade = CaminhoModeloNFe.Propriedade,
        })
        {
            Problemas = escrita.Problemas,
        };
    }
}
