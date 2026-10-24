namespace OpenDFe.Impressao;

/// <summary>
/// Um elemento do documento de impressão: o texto, a linha de células, o separador, o espaço, o QR Code, o código de
/// barras, a imagem, a seção, a tabela e a quebra de página.
/// </summary>
/// <remarks>
/// O conjunto é fechado: só a biblioteca deriva (o construtor é <see langword="private protected"/>), então quem renderiza
/// trata todos os tipos conhecidos. Os elementos são imutáveis e seguros em paralelo.
/// </remarks>
public abstract class ElementoImpressao
{
    private protected ElementoImpressao()
    {
    }
}
