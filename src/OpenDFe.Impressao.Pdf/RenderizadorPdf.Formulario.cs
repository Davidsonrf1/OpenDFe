using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// O formulário de posição fixa (o DANFSe, NT 008/2026): cada quadro numa camada, na posição e no tamanho pedidos, a partir
// do canto da área útil; a borda da área útil e a marca d'água atrás (docs/NFSE.md 12).
public static partial class RenderizadorPdf
{
    // A folga que a altura do formulário deixa à área útil: sem ela, o arredondamento de ponto flutuante do QuestPDF pode achar
    // que o formulário não cabe na página e lançar a exceção de leiaute.
    private const double FolgaFormularioMm = 0.05;

    private static void Formulario(IContainer item, FormularioImpressao formulario, Contexto contexto)
    {
        if (!contexto.Folha)
            throw new ArgumentException("O formulário de posição fixa só se desenha na folha, não na bobina.", nameof(item));
        var largura = contexto.LarguraUtilMm;
        var altura = contexto.AlturaUtilMm - FolgaFormularioMm;
        for (var i = 0; i < formulario.Quadros.Count; i++)
        {
            var quadro = formulario.Quadros[i];
            if (quadro.XMm + quadro.LarguraMm > largura + 0.001 || quadro.YMm + quadro.AlturaMm > altura + 0.001)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "O quadro {0} do formulário passa da área útil do papel ({1:0.##} x {2:0.##} mm).", i, largura, altura), nameof(formulario));
            }
        }

        var caixa = item.Width((float)largura, Unit.Millimetre).Height((float)altura, Unit.Millimetre);
        caixa.Layers(camadas =>
        {
            // A camada principal é a borda da área útil (1 pt no DANFSe), e ela dá o tamanho ao conjunto.
            var principal = camadas.PrimaryLayer();
            if (formulario.BordaPt > 0)
                principal.Border((float)formulario.BordaPt);
            else
                principal.Text(string.Empty);

            if (formulario.MarcaAgua is { } marca)
                MarcaFormulario(camadas.Layer(), marca, largura, altura);

            foreach (var quadro in formulario.Quadros)
            {
                var lugar = camadas.Layer()
                    .PaddingLeft((float)quadro.XMm, Unit.Millimetre).PaddingTop((float)quadro.YMm, Unit.Millimetre)
                    .AlignLeft().AlignTop()
                    .Width((float)quadro.LarguraMm, Unit.Millimetre).Height((float)quadro.AlturaMm, Unit.Millimetre);
                Quadro(lugar, quadro);
            }
        });
    }

    private static void Quadro(IContainer lugar, QuadroFormulario quadro)
    {
        var fundo = quadro.Sombreado ? lugar.Background(Color.FromHex(Medidas.FormularioSombra)) : lugar;
        var bordas = quadro.Bordas;
        if (bordas.HasFlag(BordaQuadro.Cima))
            fundo = fundo.BorderTop(Medidas.BordaPt);
        if (bordas.HasFlag(BordaQuadro.Baixo))
            fundo = fundo.BorderBottom(Medidas.BordaPt);
        if (bordas.HasFlag(BordaQuadro.Esquerda))
            fundo = fundo.BorderLeft(Medidas.BordaPt);
        if (bordas.HasFlag(BordaQuadro.Direita))
            fundo = fundo.BorderRight(Medidas.BordaPt);

        // O conteúdo ocupa o quadro inteiro: a borda do QuestPDF acompanha o tamanho do filho, e o quadro vazio (a linha
        // divisória) teria tamanho zero.
        fundo = fundo.Extend();
        switch (quadro)
        {
            case TextoFormulario texto:
                TextoQuadro(fundo, texto);
                break;
            case QrCodeFormulario qr:
                fundo.Svg(QrCodeSvg.Gerar(qr.Texto)).FitArea();
                break;
            case ImagemFormulario imagem:
                fundo.AlignCenter().AlignMiddle().Image(imagem.Png.ToArray()).FitArea();
                break;
            default:
                throw new NotSupportedException($"O renderizador PDF não trata o quadro {quadro.GetType().Name}.");
        }
    }

    // O rótulo numa linha e o valor nas linhas que cabem na altura do quadro: o que passa sai cortado com reticências (a NT
    // 008/2026, 2.1, permite as reticências); o formulário nunca cresce. No quadro justo, o respiro vertical encolhe antes da
    // fonte: o tamanho pedido é o mínimo da norma. A redução ao caber só age se nem a primeira linha couber no quadro (o quadro
    // pequeno demais de quem monta), e evita a exceção de leiaute.
    private static void TextoQuadro(IContainer fundo, TextoFormulario texto)
    {
        var cor = texto.Cor == CorImpressao.Vermelho ? Color.FromHex(Medidas.FormularioVermelho) : Colors.Black;
        var tamanhoRotulo = (float)texto.TamanhoRotuloPt;
        var tamanhoValor = (float)texto.TamanhoValorPt;
        var rotulo = string.IsNullOrEmpty(texto.Rotulo) ? null : TextoSeguro.Limpar(texto.Rotulo);
        var valor = string.IsNullOrEmpty(texto.Valor) ? null : TextoSeguro.Limpar(texto.Valor);
        var alturaRotulo = rotulo is null ? 0 : tamanhoRotulo * Medidas.Entrelinha;
        var linhaValor = tamanhoValor * Medidas.Entrelinha;
        var quadroPt = texto.AlturaMm * Medidas.PontosMilimetro;
        var primeira = alturaRotulo + (valor is null ? 0 : linhaValor);
        var respiro = (float)Math.Clamp((quadroPt - primeira) / 2 - Medidas.FormularioFolgaPt, 0, Medidas.FormularioRespiroVerticalMm * Medidas.PontosMilimetro);
        var linhas = Math.Max(1, (int)Math.Floor((quadroPt - 2 * respiro - alturaRotulo - Medidas.FormularioFolgaPt) / linhaValor));

        fundo.PaddingHorizontal(Medidas.FormularioRespiroHorizontalMm, Unit.Millimetre)
            .PaddingVertical(respiro)
            .ScaleToFit()
            .Column(coluna =>
            {
                if (rotulo is not null)
                {
                    coluna.Item().Text(descritor =>
                    {
                        descritor.DefaultTextStyle(estilo => Estilo(estilo, tamanhoRotulo, texto.RotuloNegrito, cor));
                        Alinhar(descritor, texto.Alinhamento);
                        descritor.ClampLines(1, Medidas.FormularioReticencias);
                        descritor.Span(rotulo);
                    });
                }

                if (valor is not null)
                {
                    coluna.Item().Text(descritor =>
                    {
                        descritor.DefaultTextStyle(estilo => Estilo(estilo, tamanhoValor, texto.ValorNegrito, cor));
                        Alinhar(descritor, texto.Alinhamento);
                        descritor.ClampLines(linhas, Medidas.FormularioReticencias);
                        descritor.Span(valor);
                    });
                }
            });
    }

    private static TextStyle Estilo(TextStyle estilo, float tamanho, bool negrito, Color cor)
    {
        var novo = estilo.FontSize(tamanho).LineHeight(Medidas.Entrelinha).FontColor(cor);
        return negrito ? novo.Bold() : novo;
    }

    // A marca d'água do formulário (NT 008/2026 2.5): em diagonal, sem negrito, cinza K35, com 50 pontos no mínimo e até o
    // tamanho que cabe na largura do formulário, atrás dos quadros.
    private static void MarcaFormulario(IContainer camada, string marca, double larguraMm, double alturaMm)
    {
        var texto = TextoSeguro.Limpar(marca);
        var letras = Math.Max(1, texto.Length);
        var larguraPt = (float)(Math.Min(larguraMm, alturaMm) * Medidas.PontosMilimetro);
        var cabe = larguraPt / (letras * Medidas.FormularioMarcaLarguraLetra);
        // O mínimo da NT (50 pt) vale enquanto o texto cabe; o texto longo demais encolhe para não sair do formulário.
        var tamanho = Math.Min(cabe, Math.Clamp(cabe, Medidas.FormularioMarcaMinimo, Medidas.FormularioMarcaMaximo));
        camada.AlignCenter().AlignMiddle()
            .Width(letras * tamanho * Medidas.FormularioMarcaLarguraLetra).Height(tamanho * 1.4f)
            .Rotate(Medidas.MarcaAnguloGraus).AlignCenter().AlignMiddle()
            .Text(texto).FontSize(tamanho).FontColor(Color.FromHex(Medidas.FormularioMarcaCor));
    }
}
