namespace OpenDFe.NFe;

// As combinações da operação (CFOP × idDest, indIEDest × IE) e as datas (REGRAS.md 11).
internal static partial class RegrasNFe400
{
    // A tolerância do relógio na emissão (B09-10): o servidor da empresa e o da SEFAZ podem diferir.
    private static TimeSpan ToleranciaRelogio => TimeSpan.FromMinutes(5);

    private static IEnumerable<RegraNFe> Operacao() =>
    [
        // Exceção: com o combustível consumido no exterior (UFCons=EX), o CFOP iniciado por 7 vale com outro idDest.
        Regra("I08-30", "731", SoNFe, AplicacaoRegra.Obrigatoria, "CFOP de operação com Exterior e idDest <> 3",
            (n, _) => CfopIdDest(n, [3, 7], LocalDestino.Exterior,
                item => item.Produto.Cfop / 1000 == 7 && item.Produto.Combustivel?.Exterior == true)),
        Regra("I08-40", "732", SoNFe, AplicacaoRegra.Obrigatoria, "CFOP de operação interestadual e idDest <> 2",
            (n, _) => CfopIdDest(n, [2, 6], LocalDestino.Interestadual, _ => false)),
        Regra("I08-50", "733", SoNFe, AplicacaoRegra.Obrigatoria, "CFOP de operação interna e idDest <> 1",
            (n, _) => CfopIdDest(n, [1, 5], LocalDestino.Interna, _ => false)),

        Regra("E16a-40", "696", SoNFe, AplicacaoRegra.Obrigatoria,
            "Operação com não contribuinte deve indicar operação com consumidor final",
            (n, _) => n.Destinatario?.IndicadorIE == IndicadorIEDestinatario.NaoContribuinte
                && n.Identificacao.ConsumidorFinal == IndicadorConsumidorFinal.Normal
                && n.Identificacao.Tipo == TipoNota.Saida
                && n.Identificacao.LocalDestino is { } destino && destino != LocalDestino.Exterior
                    ? [new OcorrenciaRegra($"{Raiz}/dest/indIEDest")]
                    : []),
        Regra("E17-20", "728", SoNFe, AplicacaoRegra.Obrigatoria, "NF-e sem informação da IE do destinatário",
            (n, _) => n.Destinatario is { IndicadorIE: IndicadorIEDestinatario.Contribuinte } d && string.IsNullOrEmpty(d.InscricaoEstadual)
                ? [new OcorrenciaRegra($"{Raiz}/dest/indIEDest")]
                : []),
        Regra("E17-30", "791", SoNFe, AplicacaoRegra.Obrigatoria,
            "NF-e com indicação de destinatário isento de IE, com a informação da IE do destinatário",
            (n, _) => n.Destinatario is { IndicadorIE: IndicadorIEDestinatario.ContribuinteIsento } d && !string.IsNullOrEmpty(d.InscricaoEstadual)
                ? [new OcorrenciaRegra($"{Raiz}/dest/IE")]
                : []),
        Regra("E17-40", "792", SoNFe, AplicacaoRegra.Obrigatoria,
            "Informada a IE do destinatário para operação com destinatário no Exterior",
            (n, _) => n.Destinatario is { Endereco.Exterior: true } d && !string.IsNullOrEmpty(d.InscricaoEstadual)
                ? [new OcorrenciaRegra($"{Raiz}/dest/IE")]
                : []),

        Regra("B09-10", "703", Ambos, AplicacaoRegra.Obrigatoria, "Data-Hora de Emissão posterior ao horário de recebimento",
            (n, c) => n.Identificacao.Emissao is { } emissao && emissao > c.Agora + ToleranciaRelogio
                ? [new OcorrenciaRegra($"{Raiz}/ide/dhEmi")]
                : []),
        Regra("B10-40", "506", SoNFe, AplicacaoRegra.Facultativa, "Data de Saída menor que a Data de Emissão",
            (n, _) => n.Identificacao is { Tipo: TipoNota.Saida, SaidaEntrada: { } saida, Emissao: { } emissao } && saida < emissao
                ? [new OcorrenciaRegra($"{Raiz}/ide/dhSaiEnt")]
                : []),
        Regra("I83-10", "877", Ambos, AplicacaoRegra.Obrigatoria, "Data de fabricação maior que a data de processamento",
            (n, c) => Rastros(n).Where(r => r.Rastro.Fabricacao > Dia(c.Agora, n))
                .Select(r => new OcorrenciaRegra($"{r.Caminho}/dFab"))),
        Regra("I84-10", "870", Ambos, AplicacaoRegra.Obrigatoria, "Data de validade incompatível com data de fabricação",
            (n, _) => Rastros(n).Where(r => r.Rastro.Validade < r.Rastro.Fabricacao)
                .Select(r => new OcorrenciaRegra($"{r.Caminho}/dVal"))),
        Regra("Y09-20", "900", SoNFe, AplicacaoRegra.Obrigatoria,
            "Data de vencimento da parcela não informada ou menor que Data de Emissão",
            (n, _) => Duplicatas(n).Where(d => n.Identificacao.Emissao is { } emissao
                    && (d.Duplicata.Vencimento is not { } vencimento || vencimento < DateOnly.FromDateTime(emissao.DateTime)))
                .Select(d => new OcorrenciaRegra($"{d.Caminho}/dVenc"))),
        Regra("Y09-30", "850", SoNFe, AplicacaoRegra.Obrigatoria,
            "Data de vencimento da parcela não informada ou menor que a Data de vencimento da parcela anterior",
            (n, _) => Duplicatas(n).Skip(1).Zip(Duplicatas(n))
                .Where(x => x.First.Duplicata.Vencimento is not { } vencimento
                    || (x.Second.Duplicata.Vencimento is { } anterior && vencimento < anterior))
                .Select(x => new OcorrenciaRegra($"{x.First.Caminho}/dVenc"))),
    ];

    // O CFOP do item começa por um dos dígitos e o idDest é outro (a exceção à parte).
    private static IEnumerable<OcorrenciaRegra> CfopIdDest(NotaFiscal nota, int[] inicios, LocalDestino esperado, Func<Item, bool> excecao)
    {
        if (nota.Identificacao.LocalDestino is not { } destino || destino == esperado)
            yield break;
        for (var i = 0; i < nota.Itens.Count; i++)
        {
            var item = nota.Itens[i];
            if (item.Produto.Cfop is { } cfop && inicios.Contains(cfop / 1000) && !excecao(item))
                yield return new OcorrenciaRegra($"{Item(nota, i)}/prod/CFOP", $"CFOP {cfop}; idDest {(int)destino}");
        }
    }

    private static IEnumerable<(Rastro Rastro, string Caminho)> Rastros(NotaFiscal nota) =>
        nota.Itens.SelectMany((item, i) => item.Produto.Rastros.Select((r, j) =>
            (r, $"{Item(nota, i)}/prod/{Passo("rastro", j, item.Produto.Rastros.Count)}")));

    private static List<(Duplicata Duplicata, string Caminho)> Duplicatas(NotaFiscal nota)
    {
        var duplicatas = nota.Cobranca?.Duplicatas ?? [];
        return [.. duplicatas.Select((d, i) => (d, $"{Raiz}/cobr/{Passo("dup", i, duplicatas.Count)}"))];
    }

    // O dia do instante no fuso da emissão (o dia de quem emite), ou no UTC sem a emissão.
    private static DateOnly Dia(DateTimeOffset instante, NotaFiscal nota) =>
        DateOnly.FromDateTime(instante.ToOffset(nota.Identificacao.Emissao?.Offset ?? TimeSpan.Zero).DateTime);
}
