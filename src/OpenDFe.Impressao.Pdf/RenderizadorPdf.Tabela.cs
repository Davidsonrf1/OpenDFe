using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// A tabela (os itens do DANFE): o título do bloco, o cabeçalho das colunas repetido a cada página e as linhas com as
// larguras proporcionais aos pesos (Anexo II 3.1.7, 3.5 e 3.7.2; docs/CONTRATOS.md 22.2).
public static partial class RenderizadorPdf
{
    private static void Tabela(IContainer item, TabelaImpressao tabela, Contexto contexto)
    {
        item.Column(coluna =>
        {
            if (!string.IsNullOrWhiteSpace(tabela.Titulo))
                Escrever(coluna.Item().PaddingTop(0.8f, Unit.Millimetre), tabela.Titulo, Medidas.FolhaTituloQuadro, true, AlinhamentoImpressao.Esquerda);

            coluna.Item().Table(quadro =>
            {
                quadro.ColumnsDefinition(colunas =>
                {
                    foreach (var definicao in tabela.Colunas)
                        colunas.RelativeColumn((float)definicao.Peso);
                });

                // O cabeçalho do QuestPDF se repete no alto de cada página em que a tabela continua.
                quadro.Header(cabecalho =>
                {
                    for (var j = 0; j < tabela.Colunas.Count; j++)
                    {
                        var definicao = tabela.Colunas[j];
                        var celula = Moldura(cabecalho.Cell(), j == 0, topo: true, divisor: true, escuro: false);
                        Escrever(celula, definicao.Titulo, Medidas.FolhaCabecalhoTabela, true, AlinhamentoImpressao.Centro);
                    }
                });

                foreach (var linha in tabela.Linhas)
                {
                    for (var j = 0; j < linha.Count; j++)
                    {
                        var celula = Moldura(quadro.Cell(), j == 0, topo: false, divisor: true, escuro: tabela.DivisorLinhas);
                        // A linha indivisível: o conteúdo de uma célula não se divide entre duas páginas (3.1.7).
                        if (tabela.LinhasIndivisiveis)
                            celula = celula.ShowEntire();
                        Escrever(celula, linha[j], Medidas.FolhaCelulaTabela, false, tabela.Colunas[j].Alinhamento);
                    }
                }
            });
        });
    }

    // A moldura da célula: a linha vertical entre as colunas e a horizontal entre as linhas (mais escura no destaque
    // divisório). O contorno externo vem da primeira coluna (esquerda) e do cabeçalho (topo).
    private static IContainer Moldura(IContainer celula, bool primeira, bool topo, bool divisor, bool escuro)
    {
        var moldura = celula;
        if (topo)
            moldura = moldura.BorderTop(Medidas.BordaPt);
        if (primeira)
            moldura = moldura.BorderLeft(Medidas.BordaPt);
        if (divisor)
            moldura = moldura.BorderRight(Medidas.BordaPt).BorderBottom(escuro ? Medidas.BordaPt : Medidas.BordaPt / 2);
        return moldura.PaddingVertical(Medidas.QuadroRespiroMm / 2, Unit.Millimetre).PaddingHorizontal(Medidas.CelulaRespiroHorizontalMm, Unit.Millimetre);
    }
}
