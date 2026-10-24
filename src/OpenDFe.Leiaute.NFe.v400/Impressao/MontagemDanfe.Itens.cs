using System.Globalization;
using OpenDFe.Impressao;

namespace OpenDFe.NFe;

// O quadro "Dados dos produtos/serviços" (Anexo II 3.1.7, 3.2 e 3.8; NT 2026.010 4.3): uma linha por item, na ordem das
// colunas do Anexo II, com a descrição e os dados do item em linhas dentro da célula.
internal static partial class MontagemDanfe
{
    private const string Quebra = "\n";

    // Os dados do ICMS que as colunas pedem, de qualquer das tributações: o código (origem e CST ou CSOSN), as bases, a
    // alíquota e os valores do ICMS próprio e da substituição tributária.
    private readonly record struct DadosIcms(
        string? Codigo, decimal? BaseCalculo, decimal? Aliquota, decimal? Valor, decimal? BaseCalculoSt, decimal? ValorSt);

    private static TabelaImpressao Itens(NotaFiscal nota)
    {
        var reforma = nota.Itens.Any(i => i.Imposto.IbsCbs is not null || i.Imposto.ImpostoSeletivo is not null);
        var colunas = new List<ColunaImpressao>
        {
            new("CÓDIGO", 1.4),
            new("DESCRIÇÃO DOS PRODUTOS/SERVIÇOS", 4.6),
            new("NCM/SH", 1.5),
            new("O/CST", 1.0),
            new("CFOP", 0.9),
            new("UNID.", 0.9),
            new("QUANT.", 1.4, AlinhamentoImpressao.Direita),
            new("VALOR UNITÁRIO", 1.5, AlinhamentoImpressao.Direita),
            new("DESCONTO", 1.1, AlinhamentoImpressao.Direita),
            new("VALOR TOTAL", 1.5, AlinhamentoImpressao.Direita),
            new("B.CÁLC. ICMS", 1.5, AlinhamentoImpressao.Direita),
            new("B.CÁLC. ICMS ST", 1.5, AlinhamentoImpressao.Direita),
            new("VALOR ICMS", 1.3, AlinhamentoImpressao.Direita),
            new("VALOR ICMS ST", 1.3, AlinhamentoImpressao.Direita),
            new("VALOR IPI", 1.2, AlinhamentoImpressao.Direita),
            new("ALÍQ. ICMS", 0.9, AlinhamentoImpressao.Direita),
            new("ALÍQ. IPI", 0.9, AlinhamentoImpressao.Direita),
        };
        if (reforma)
        {
            colunas.Add(new("B.CÁLC. IBS/CBS/IS", 1.5, AlinhamentoImpressao.Direita));
            colunas.Add(new("ALÍQ. IBS/CBS/IS (%)", 1.6, AlinhamentoImpressao.Direita));
            colunas.Add(new("VALOR IBS/CBS/IS", 1.5, AlinhamentoImpressao.Direita));
        }

        var linhas = new List<IReadOnlyList<string>>();
        foreach (var item in nota.Itens)
            linhas.Add(LinhaItem(item, reforma));

        return new TabelaImpressao("DADOS DOS PRODUTOS / SERVIÇOS", colunas, linhas)
        {
            // O item em várias linhas não se divide entre páginas, e um destaque o separa do seguinte (3.1.7).
            LinhasIndivisiveis = true,
            DivisorLinhas = true,
        };
    }

    private static string[] LinhaItem(Item item, bool reforma)
    {
        var produto = item.Produto;
        var icms = LerIcms(item.Imposto.Icms);
        var ipi = item.Imposto.Ipi?.Tributado;
        var celulas = new List<string>
        {
            Vazio(produto.Codigo),
            Descricao(item),
            Vazio(produto.Ncm),
            Vazio(icms.Codigo),
            produto.Cfop is { } cfop ? cfop.ToString("0000", CultureInfo.InvariantCulture) : string.Empty,
            Vazio(produto.UnidadeComercial),
            produto.QuantidadeComercial is { } quantidade ? FormatadorImpressao.Unitario(quantidade) : string.Empty,
            produto.ValorUnitarioComercial is { } unitario ? FormatadorImpressao.Unitario(unitario) : string.Empty,
            Dinheiro(produto.Desconto),
            Dinheiro(produto.Valor),
            Dinheiro(icms.BaseCalculo),
            Dinheiro(icms.BaseCalculoSt),
            Dinheiro(icms.Valor),
            Dinheiro(icms.ValorSt),
            Dinheiro(ipi?.Valor),
            Dinheiro(icms.Aliquota),
            Dinheiro(ipi?.Aliquota),
        };
        if (reforma)
            celulas.AddRange(Reforma(item.Imposto));
        return [.. celulas];
    }

    // A descrição e, logo abaixo, o que o Anexo II manda imprimir junto do item (3.1.7): as informações adicionais do produto
    // (infAdProd) e, "no campo de informações adicionais", o que o grupo H detalha (obs. 2 do 3.8.1): o EAN, a unidade
    // tributável quando difere da comercial, o lote, o medicamento, o combustível, o veículo e a arma.
    private static string Descricao(Item item)
    {
        var produto = item.Produto;
        var linhas = new List<string> { Vazio(produto.Descricao) };

        if (!string.IsNullOrWhiteSpace(produto.Gtin) && produto.Gtin.All(char.IsAsciiDigit))
            linhas.Add("EAN: " + produto.Gtin);

        var tributavel = produto.UnidadeTributavel is not null
            && (!string.Equals(produto.UnidadeTributavel, produto.UnidadeComercial, StringComparison.Ordinal)
                || produto.QuantidadeTributavel != produto.QuantidadeComercial
                || produto.ValorUnitarioTributavel != produto.ValorUnitarioComercial);
        if (tributavel)
        {
            linhas.Add("Trib.: " + Juntar(" ",
                produto.QuantidadeTributavel is { } q ? FormatadorImpressao.Unitario(q) : null,
                produto.UnidadeTributavel,
                produto.ValorUnitarioTributavel is { } v ? "x " + FormatadorImpressao.Unitario(v) : null));
        }

        if (!string.IsNullOrWhiteSpace(item.InformacoesAdicionais))
            linhas.Add(item.InformacoesAdicionais);

        foreach (var rastro in produto.Rastros)
        {
            linhas.Add("Lote: " + Juntar(" ", rastro.Lote,
                rastro.Quantidade is { } quantidade ? "Qtd: " + FormatadorImpressao.Quantidade(quantidade) : null,
                rastro.Fabricacao is { } fabricacao ? "Fab: " + FormatadorImpressao.Data(fabricacao) : null,
                rastro.Validade is { } validade ? "Val: " + FormatadorImpressao.Data(validade) : null));
        }

        if (produto.Medicamento is { } medicamento)
        {
            linhas.Add("Medicamento: " + Juntar(" ", medicamento.CodigoAnvisa is { } anvisa ? "Reg. ANVISA " + anvisa : null,
                medicamento.MotivoIsencao is { } motivo ? "Isenção: " + motivo : null,
                medicamento.PrecoMaximoConsumidor is { } preco ? "PMC R$ " + FormatadorImpressao.Dinheiro(preco) : null));
        }

        if (produto.Combustivel is { } combustivel)
            linhas.Add("Combustível: " + Juntar(" ", combustivel.CodigoAnp is { } anp ? "ANP " + anp : null, combustivel.DescricaoAnp));

        if (produto.VeiculoNovo is { } veiculo)
        {
            linhas.Add("Veículo: " + Juntar(" ", veiculo.Chassi is { } chassi ? "Chassi " + chassi : null,
                veiculo.Cor is { } cor ? "Cor " + cor : null, veiculo.MarcaModelo));
        }

        foreach (var arma in produto.Armas)
            linhas.Add("Arma: " + Juntar(" ", arma.Serie is { } serie ? "Série " + serie : null, arma.SerieCano is { } cano ? "Cano " + cano : null, arma.Descricao));

        if (item.Imposto.IbsCbs?.ClassificacaoTributaria is { } classificacao)
            linhas.Add("cClassTrib " + classificacao);

        return string.Join(Quebra, linhas);
    }

    // As três colunas da reforma (NT 2026.010 4.3), uma linha por tributo: a base, a alíquota (a efetiva quando há redução de
    // alíquota ou compra governamental, o grupo gRed) e o valor do IBS UF, do IBS municipal, da CBS e do IS.
    private static string[] Reforma(Imposto imposto)
    {
        var bases = new List<string>();
        var aliquotas = new List<string>();
        var valores = new List<string>();
        if (imposto.IbsCbs?.Tributacao is { } tributacao)
        {
            if (tributacao.BaseCalculo is { } baseCalculo)
                bases.Add("IBS/CBS: " + FormatadorImpressao.Dinheiro(baseCalculo));
            Tributo("IBS UF", tributacao.IbsUF.Aliquota, tributacao.IbsUF.Reducao, tributacao.IbsUF.Valor, aliquotas, valores);
            Tributo("IBS MUN", tributacao.IbsMunicipio.Aliquota, tributacao.IbsMunicipio.Reducao, tributacao.IbsMunicipio.Valor, aliquotas, valores);
            Tributo("CBS", tributacao.Cbs.Aliquota, tributacao.Cbs.Reducao, tributacao.Cbs.Valor, aliquotas, valores);
        }

        if (imposto.ImpostoSeletivo is { } seletivo)
        {
            if (seletivo.BaseCalculo is { } baseSeletivo)
                bases.Add("IS: " + FormatadorImpressao.Dinheiro(baseSeletivo));
            Tributo("IS", seletivo.Aliquota, null, seletivo.Valor, aliquotas, valores);
        }

        return [string.Join(Quebra, bases), string.Join(Quebra, aliquotas), string.Join(Quebra, valores)];
    }

    private static void Tributo(string nome, decimal? aliquota, ReducaoAliquota? reducao, decimal? valor, List<string> aliquotas,
        List<string> valores)
    {
        var efetiva = reducao?.AliquotaEfetiva ?? aliquota;
        if (efetiva is { } percentual)
            aliquotas.Add($"{nome}: {FormatadorImpressao.Dinheiro(percentual)}");
        if (valor is { } montante)
            valores.Add($"{nome}: {FormatadorImpressao.Dinheiro(montante)}");
    }

    // O código do CST ou do CSOSN, com a origem na frente: "000" é origem 0 e CST 00; "0102" é origem 0 e CSOSN 102.
    private static DadosIcms LerIcms(Icms? icms)
    {
        static string? Cst(Icms i, CstIcms? cst) => cst is { } c ? Codigo(i, ((int)c).ToString("00", CultureInfo.InvariantCulture)) : null;
        static string? Csosn(Icms i, CsosnIcms? csosn) => csosn is { } c ? Codigo(i, ((int)c).ToString("000", CultureInfo.InvariantCulture)) : null;

        return icms switch
        {
            Icms00 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, null, null),
            Icms02 i => new(Cst(i, i.Cst), null, null, null, null, null),
            Icms10 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, i.BaseCalculoSt, i.ValorSt),
            Icms15 i => new(Cst(i, i.Cst), null, null, null, null, null),
            Icms20 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, null, null),
            Icms30 i => new(Cst(i, i.Cst), null, null, null, i.BaseCalculoSt, i.ValorSt),
            Icms40 i => new(Cst(i, i.Cst), null, null, null, null, null),
            Icms51 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, null, null),
            Icms53 i => new(Cst(i, i.Cst), null, null, null, null, null),
            Icms60 i => new(Cst(i, i.Cst), null, null, null, i.BaseCalculoStRetido, i.ValorStRetido),
            Icms61 i => new(Cst(i, i.Cst), null, null, null, null, null),
            Icms70 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, i.BaseCalculoSt, i.ValorSt),
            Icms90 i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, i.BaseCalculoSt, i.ValorSt),
            IcmsPartilha i => new(Cst(i, i.Cst), i.BaseCalculo, i.Aliquota, i.Valor, i.BaseCalculoSt, i.ValorSt),
            IcmsRepasse i => new(Cst(i, i.Cst), null, null, null, i.BaseCalculoStRetido, i.ValorStRetido),
            IcmsSn101 i => new(Csosn(i, i.Csosn), null, null, null, null, null),
            IcmsSn102 i => new(Csosn(i, i.Csosn), null, null, null, null, null),
            IcmsSn201 i => new(Csosn(i, i.Csosn), null, null, null, i.BaseCalculoSt, i.ValorSt),
            IcmsSn202 i => new(Csosn(i, i.Csosn), null, null, null, i.BaseCalculoSt, i.ValorSt),
            IcmsSn500 i => new(Csosn(i, i.Csosn), null, null, null, i.BaseCalculoStRetido, i.ValorStRetido),
            IcmsSn900 i => new(Csosn(i, i.Csosn), i.BaseCalculo, i.Aliquota, i.Valor, i.BaseCalculoSt, i.ValorSt),
            _ => default,
        };
    }

    private static string Codigo(Icms icms, string cst) =>
        (icms.Origem is { } origem ? ((int)origem).ToString(CultureInfo.InvariantCulture) : string.Empty) + cst;
}
