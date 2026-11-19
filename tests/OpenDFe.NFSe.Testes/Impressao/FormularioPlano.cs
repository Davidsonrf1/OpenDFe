using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// O formulário do DANFSe em texto, um quadro por linha na ordem do desenho, para os golden (<c>Impressao/Esperado/*.txt</c>):
/// o tipo, a posição e o tamanho em milímetros, o fundo e as bordas, os tamanhos em pontos com o negrito, a cor e o
/// alinhamento, e o rótulo e o valor. Só de teste.
/// </summary>
internal static class FormularioPlano
{
    internal static string Renderizar(DocumentoImpressao documento)
    {
        var formulario = Assert.IsType<FormularioImpressao>(Assert.Single(documento.Elementos));
        var texto = new StringBuilder();
        texto.Append(CultureInfo.InvariantCulture, $"DOCUMENTO \"{documento.Titulo}\" papel={documento.Papel.LarguraMm}x{documento.Papel.AlturaMm} margem={documento.Papel.MargemMm}");
        texto.Append('\n');
        texto.Append(CultureInfo.InvariantCulture, $"FORMULARIO borda={formulario.BordaPt} marca={formulario.MarcaAgua ?? "-"}\n");
        foreach (var quadro in formulario.Quadros)
        {
            texto.Append(Tipo(quadro)).Append(' ').Append(Medida(quadro.XMm)).Append(',').Append(Medida(quadro.YMm)).Append(' ')
                .Append(Medida(quadro.LarguraMm)).Append('x').Append(Medida(quadro.AlturaMm));
            if (quadro.Sombreado)
                texto.Append(" sombra");
            if (quadro.Bordas != BordaQuadro.Nenhuma)
                texto.Append(" borda=").Append(quadro.Bordas.ToString().Replace(", ", "+", StringComparison.Ordinal));
            switch (quadro)
            {
                case TextoFormulario t:
                    texto.Append(' ').Append(Estilo(t));
                    if (t.Rotulo is not null)
                        texto.Append(CultureInfo.InvariantCulture, $" r{t.TamanhoRotuloPt}{(t.RotuloNegrito ? "B" : "")}=\"{t.Rotulo}\"");
                    if (t.Valor is not null)
                        texto.Append(CultureInfo.InvariantCulture, $" v{t.TamanhoValorPt}{(t.ValorNegrito ? "B" : "")}=\"{t.Valor.Replace("\n", "\\n", StringComparison.Ordinal)}\"");
                    break;
                case QrCodeFormulario qr:
                    texto.Append(" \"").Append(qr.Texto).Append('"');
                    break;
                case ImagemFormulario imagem:
                    texto.Append(" png sha256=").Append(Convert.ToHexStringLower(SHA256.HashData(imagem.Png.Span)));
                    break;
            }

            texto.Append('\n');
        }

        return texto.ToString();
    }

    private static string Tipo(QuadroFormulario quadro) => quadro switch
    {
        TextoFormulario => "T",
        QrCodeFormulario => "Q",
        ImagemFormulario => "I",
        _ => "?",
    };

    private static string Estilo(TextoFormulario texto)
    {
        var partes = new List<string>();
        if (texto.Alinhamento != AlinhamentoImpressao.Esquerda)
            partes.Add(texto.Alinhamento.ToString().ToLowerInvariant());
        if (texto.Cor != CorImpressao.Preto)
            partes.Add(texto.Cor.ToString().ToLowerInvariant());
        return partes.Count == 0 ? "-" : string.Join('+', partes);
    }

    private static string Medida(double valor) => valor.ToString("0.###", CultureInfo.InvariantCulture);
}
