namespace OpenDFe.NFe;

// O CNPJ e o CPF de cada pessoa da nota (com zeros, repetidos ou com o DV errado) e o DV das chaves referenciadas
// (REGRAS.md 11). O documento errado não vai no detalhe: é dado pessoal (PROJETO.md 6.4).
internal static partial class RegrasNFe400
{
    private static IEnumerable<RegraNFe> Documentos() =>
    [
        Regra("C02-10", "207", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ do emitente inválido",
            (n, _) => CnpjInvalido(n.Emitente.Cnpj, $"{Raiz}/emit/CNPJ")),
        Regra("C02a-20", "401", Ambos, AplicacaoRegra.Obrigatoria, "CPF do emitente inválido",
            (n, _) => CpfInvalido(n.Emitente.Cpf, $"{Raiz}/emit/CPF")),
        Regra("E02-10", "208", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ do destinatário inválido",
            (n, _) => CnpjInvalido(n.Destinatario?.Cnpj, $"{Raiz}/dest/CNPJ")),
        Regra("E03-10", "237", Ambos, AplicacaoRegra.Obrigatoria, "CPF do destinatário inválido",
            (n, _) => CpfInvalido(n.Destinatario?.Cpf, $"{Raiz}/dest/CPF")),
        Regra("F02-10", "512", Ambos, AplicacaoRegra.Facultativa, "CNPJ do Local de Retirada inválido",
            (n, _) => CnpjInvalido(n.Retirada?.Cnpj, $"{Raiz}/retirada/CNPJ")),
        Regra("F02a-10", "540", Ambos, AplicacaoRegra.Facultativa, "CPF do Local de Retirada inválido",
            (n, _) => CpfInvalido(n.Retirada?.Cpf, $"{Raiz}/retirada/CPF")),
        Regra("G02-10", "514", Ambos, AplicacaoRegra.Facultativa, "CNPJ do Local de Entrega inválido",
            (n, _) => CnpjInvalido(n.Entrega?.Cnpj, $"{Raiz}/entrega/CNPJ")),
        Regra("G02a-10", "541", Ambos, AplicacaoRegra.Facultativa, "CPF do Local de Entrega inválido",
            (n, _) => CpfInvalido(n.Entrega?.Cpf, $"{Raiz}/entrega/CPF")),
        Regra("GA02-10", "323", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ autorizado para download inválido",
            (n, _) => n.AutorizadosXml.SelectMany((a, i) =>
                CnpjInvalido(a.Cnpj, $"{Raiz}/{Passo("autXML", i, n.AutorizadosXml.Count)}/CNPJ"))),
        Regra("GA03-10", "325", Ambos, AplicacaoRegra.Obrigatoria, "CPF autorizado para download inválido",
            (n, _) => n.AutorizadosXml.SelectMany((a, i) =>
                CpfInvalido(a.Cpf, $"{Raiz}/{Passo("autXML", i, n.AutorizadosXml.Count)}/CPF"))),
        Regra("I05e-20", "489", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ informado inválido (DV ou zeros)",
            (n, _) => n.Itens.SelectMany((item, i) =>
                CnpjInvalido(item.Produto.CnpjFabricante, $"{Item(n, i)}/prod/CNPJFab"))),
        Regra("I23d-20", "332", SoNFe, AplicacaoRegra.Obrigatoria,
            "CNPJ/CPF do adquirente ou do encomendante da importação inválido",
            (n, _) => Importacoes(n).SelectMany(d => CnpjInvalido(d.Declaracao.Cnpj, $"{d.Caminho}/CNPJ"))),
        Regra("I23d1-10", "332", SoNFe, AplicacaoRegra.Obrigatoria,
            "CNPJ/CPF do adquirente ou do encomendante da importação inválido",
            (n, _) => Importacoes(n).SelectMany(d => CpfInvalido(d.Declaracao.Cpf, $"{d.Caminho}/CPF"))),
        Regra("X04-20", "542", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ do Transportador inválido",
            (n, _) => CnpjInvalido(n.Transporte.Transportador?.Cnpj, $"{Raiz}/transp/transporta/CNPJ")),
        Regra("X05-10", "543", Ambos, AplicacaoRegra.Obrigatoria, "CPF do Transportador inválido",
            (n, _) => CpfInvalido(n.Transporte.Transportador?.Cpf, $"{Raiz}/transp/transporta/CPF")),
        Regra("YA03c-10", "961", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ transacional do pagamento inválido",
            (n, _) => n.Pagamento.Formas.SelectMany((f, i) =>
                CnpjInvalido(f.CnpjTransacao, $"{Raiz}/pag/{Passo("detPag", i, n.Pagamento.Formas.Count)}/CNPJPag"))),
        Regra("YB02-10", "440", Ambos, AplicacaoRegra.Obrigatoria, "CNPJ do intermediador da transação inválido",
            (n, _) => CnpjInvalido(n.Intermediador?.Cnpj, $"{Raiz}/infIntermed/CNPJ")),
        Regra("ZD02-10", "973", Ambos, AplicacaoRegra.Facultativa, "CNPJ do responsável técnico inválido",
            (n, _) => CnpjInvalido(n.ResponsavelTecnico?.Cnpj, $"{Raiz}/infRespTec/CNPJ")),

        Regra("BA02-10", "547", SoNFe, AplicacaoRegra.Facultativa,
            "Chave de Acesso referenciada com Dígito Verificador inválido",
            (n, _) => Referencias<NFeReferenciada>(n).Where(r => !DigitoChaveValido(r.Referencia.Chave ?? ""))
                .Select(r => new OcorrenciaRegra($"{r.Caminho}/refNFe"))),
        Regra("BA19-10", "547", SoNFe, AplicacaoRegra.Facultativa,
            "Chave de Acesso referenciada com Dígito Verificador inválido",
            (n, _) => Referencias<CTeReferenciado>(n).Where(r => !DigitoChaveValido(r.Referencia.Chave ?? ""))
                .Select(r => new OcorrenciaRegra($"{r.Caminho}/refCTe"))),
        Regra("I54-10", "342", SoNFe, AplicacaoRegra.Facultativa,
            "Chave de Acesso informada na Exportação Indireta com DV inválido",
            (n, _) => n.Itens.SelectMany((item, i) => item.Produto.Exportacoes.Select((e, j) => (e, j))
                .Where(x => x.e.Indireta?.Chave is { } chave && !DigitoChaveValido(chave))
                .Select(x => new OcorrenciaRegra(
                    $"{Item(n, i)}/prod/{Passo("detExport", x.j, item.Produto.Exportacoes.Count)}/exportInd/chNFe")))),
        Regra("BB05-70", "1204", SoNFe, AplicacaoRegra.Obrigatoria,
            "DFe referenciado em operação com ente governamental com Dígito Verificador inválido",
            (n, _) => n.Identificacao.CompraGovernamental is { } compra
                ? compra.DocumentosAnteriores.Select((chave, i) => (chave, i))
                    .Where(x => !DigitoChaveValido(x.chave))
                    .Select(x => new OcorrenciaRegra(
                        $"{Raiz}/ide/gCompraGov/{Passo("refDFeAnt", x.i, compra.DocumentosAnteriores.Count)}"))
                : []),
    ];

    private static IEnumerable<OcorrenciaRegra> CnpjInvalido(string? cnpj, string caminho) =>
        cnpj is null || Cnpj.Valido(cnpj) ? [] : [new OcorrenciaRegra(caminho)];

    private static IEnumerable<OcorrenciaRegra> CpfInvalido(string? cpf, string caminho) =>
        cpf is null || Cpf.Valido(cpf) ? [] : [new OcorrenciaRegra(caminho)];

    private static IEnumerable<(DeclaracaoImportacao Declaracao, string Caminho)> Importacoes(NotaFiscal nota) =>
        nota.Itens.SelectMany((item, i) => item.Produto.Importacoes.Select((d, j) =>
            (d, $"{Item(nota, i)}/prod/{Passo("DI", j, item.Produto.Importacoes.Count)}")));

    // As referências de um tipo, com o caminho do NFref delas (o índice conta todos os NFref).
    private static IEnumerable<(T Referencia, string Caminho)> Referencias<T>(NotaFiscal nota)
        where T : DocumentoReferenciado
    {
        var referencias = nota.Identificacao.Referencias;
        for (var i = 0; i < referencias.Count; i++)
        {
            if (referencias[i] is T referencia)
                yield return (referencia, $"{Raiz}/ide/{Passo("NFref", i, referencias.Count)}");
        }
    }
}
