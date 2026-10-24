using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// A seção: o título do bloco e as linhas de campos em quadros de borda fina, o rótulo pequeno em cima e o valor embaixo
// (Anexo II 3.7: 5 pt no título, 6 pt no rótulo e 10 pt no valor; docs/CONTRATOS.md 22.2).
public static partial class RenderizadorPdf
{
    private static void Secao(IContainer item, SecaoImpressao secao, Contexto contexto)
    {
        // O quadro pode ser mais alto que a página (as informações complementares de 5.000 caracteres): ele continua na folha
        // seguinte (Anexo II 3.5). O título não fica sozinho no pé de uma página: o EnsureSpace leva o quadro à seguinte se o
        // título e o começo da primeira linha não cabem, sem exigir que o quadro inteiro caiba (o que o ShowEntire exigiria).
        item.EnsureSpace(Medidas.QuadroEspacoMinimoPt).Column(coluna =>
        {
            if (!string.IsNullOrWhiteSpace(secao.Titulo))
                Escrever(coluna.Item().PaddingTop(0.8f, Unit.Millimetre), secao.Titulo, Medidas.FolhaTituloQuadro, true, AlinhamentoImpressao.Esquerda);

            foreach (var linha in secao.Linhas)
            {
                // A linha de campos é uma tabela de uma fileira: as células de uma fileira têm a mesma altura (o campo que
                // quebra em duas linhas estica os vizinhos), o que a Row do QuestPDF não faz.
                coluna.Item().Table(fileira =>
                {
                    fileira.ColumnsDefinition(colunas =>
                    {
                        foreach (var campo in linha.Campos)
                            colunas.RelativeColumn((float)campo.Peso);
                    });
                    foreach (var campo in linha.Campos)
                        Campo(fileira.Cell(), campo, contexto);
                });
            }
        });
    }

    // O campo: a borda fina, o rótulo e o valor.
    private static void Campo(IContainer container, CampoImpressao campo, Contexto contexto)
    {
        container.Border(Medidas.BordaPt).Padding(Medidas.QuadroRespiroMm, Unit.Millimetre).Column(coluna =>
        {
            if (!string.IsNullOrEmpty(campo.Rotulo))
                Escrever(coluna.Item(), campo.Rotulo, Medidas.FolhaRotuloCampo, false, AlinhamentoImpressao.Esquerda);
            Escrever(coluna.Item(), campo.Valor, TamanhoValor(campo, contexto), campo.Negrito, campo.Alinhamento);
        });
    }

    // O tamanho do valor do campo: 10 pt nos campos em geral (3.7.9); o campo de texto livre das informações complementares,
    // 6 pt (3.7.8). Na bobina, o tamanho normal.
    private static float TamanhoValor(CampoImpressao campo, Contexto contexto)
    {
        if (!contexto.Folha)
            return contexto.Tamanho(TamanhoTexto.Normal);
        return campo.Rotulo == Medidas.RotuloTextoLivre ? Medidas.FolhaTextoLivre : Medidas.FolhaValorCampo;
    }
}
