namespace OpenDFe.NFe;

// As regras da NFC-e (REGRAS.md 12): a operação (venda interna, de saída, a consumidor final), o destinatário, os grupos
// que a NFC-e não usa, o transporte e o pagamento. As da NT 2026.002 valem também para a NF-e com o DANFE Simplificado
// Tipo 2 (tpImp=6), como a NT as escreve.
internal static partial class RegrasNFe400
{
    private static Documento[] SoNFCe => [Documento.NFCe];

    private static IEnumerable<RegraNFe> NFCe() =>
    [
        Regra("B10-10", "705", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com data de entrada/saída",
            (n, c) => Simplificada(n, c) && n.Identificacao.SaidaEntrada is not null ? [new OcorrenciaRegra($"{Raiz}/ide/dhSaiEnt")] : []),
        Regra("B10a-10", "1153", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com data de previsão de entrega",
            (n, _) => n.Identificacao.PrevisaoEntrega is not null ? [new OcorrenciaRegra($"{Raiz}/ide/dPrevEntrega")] : []),
        Regra("B11-10", "706", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 para operação de entrada",
            (n, c) => Simplificada(n, c) && n.Identificacao.Tipo == TipoNota.Entrada ? [new OcorrenciaRegra($"{Raiz}/ide/tpNF")] : []),
        Regra("B11a-10", "707", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 para operação interestadual ou com o exterior",
            (n, c) => Simplificada(n, c) && n.Identificacao.LocalDestino is { } destino && destino != LocalDestino.Interna
                ? [new OcorrenciaRegra($"{Raiz}/ide/idDest", $"idDest {(int)destino}")]
                : []),
        Regra("B21-10", "709", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com formato de DANFE inválido",
            (n, _) => n.Identificacao.FormatoImpressao is { } formato
                    && formato is not (FormatoImpressao.DanfeNFCe or FormatoImpressao.DanfeNFCeMensagem)
                ? [new OcorrenciaRegra($"{Raiz}/ide/tpImp", $"tpImp {(int)formato}")]
                : []),
        Regra("B25-20", "715", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com finalidade inválida",
            (n, c) => Simplificada(n, c) && n.Identificacao.Finalidade is { } finalidade && finalidade != FinalidadeEmissao.Normal
                ? [new OcorrenciaRegra($"{Raiz}/ide/finNFe", $"finNFe {(int)finalidade}")]
                : []),
        Regra("B25a-10", "716", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 em operação não destinada a consumidor final",
            (n, c) => Simplificada(n, c) && n.Identificacao.ConsumidorFinal == IndicadorConsumidorFinal.Normal
                ? [new OcorrenciaRegra($"{Raiz}/ide/indFinal")]
                : []),
        // Exceção 1: em SC, a NFC-e pelo aplicativo da nota avulsa (procEmi 1 ou 2).
        Regra("B26-50", "957", SoNFCe, AplicacaoRegra.Obrigatoria, "Tipo de emissão incompatível com o Processo de Emissão",
            (n, c) => (int)(n.Identificacao.TipoEmissao ?? c.Nucleo.Emissao) != 3
                    && n.Identificacao.Processo is { } processo && processo != ProcessoEmissao.AplicativoContribuinte
                    && !(c.Nucleo.UF == UF.SC && processo is ProcessoEmissao.AvulsaFisco or ProcessoEmissao.AvulsaContribuinteSite)
                ? [new OcorrenciaRegra($"{Raiz}/ide/procEmi", $"procEmi {(int)processo}")]
                : []),
        Regra("BA01-10", "708", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 não pode referenciar documento fiscal",
            (n, c) => Simplificada(n, c) && n.Identificacao.Referencias.Count > 0
                ? [new OcorrenciaRegra($"{Raiz}/ide/{Passo("NFref", 0, n.Identificacao.Referencias.Count)}")]
                : []),
        Regra("BB01-10", "1006", SoNFCe, AplicacaoRegra.Obrigatoria, "NFCe com grupo de compra governamental",
            (n, _) => n.Identificacao.CompraGovernamental is not null ? [new OcorrenciaRegra($"{Raiz}/ide/gCompraGov")] : []),

        Regra("C18-10", "718", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 não deve informar IE de Substituto Tributário",
            (n, c) => Simplificada(n, c) && !string.IsNullOrEmpty(n.Emitente.InscricaoEstadualSubstituto)
                ? [new OcorrenciaRegra($"{Raiz}/emit/IEST")]
                : []),

        Regra("E01-20", "787", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 (tag:tpImp=6) em operação não presencial sem a identificação do destinatário",
            (n, c) => Simplificada(n, c) && n.Identificacao.Presenca == PresencaComprador.NaoPresencialEntrega && n.Destinatario is null
                ? [new OcorrenciaRegra($"{Raiz}/ide/indPres")]
                : []),
        Regra("E02-20", "220", SoNFCe, AplicacaoRegra.Obrigatoria,
            "Destinatário com identificação igual à identificação do emitente",
            (n, _) => n.Destinatario?.Cnpj is { Length: > 0 } cnpj && cnpj == n.Emitente.Cnpj
                ? [new OcorrenciaRegra($"{Raiz}/dest/CNPJ")]
                : []),
        // Sem o destinatário, a falta é dele (E01-20); aqui, o destinatário sem o endereço.
        Regra("E05-20", "788", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e de entrega a domicílio sem o endereço do destinatário",
            (n, _) => n.Identificacao.Presenca == PresencaComprador.NaoPresencialEntrega && n.Destinatario is { Endereco: null }
                ? [new OcorrenciaRegra($"{Raiz}/dest")]
                : []),
        Regra("E16a-10", "789", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e para destinatário contribuinte de ICMS",
            (n, _) => n.Destinatario?.IndicadorIE is { } indicador && indicador != IndicadorIEDestinatario.NaoContribuinte
                ? [new OcorrenciaRegra($"{Raiz}/dest/indIEDest", $"indIEDest {(int)indicador}")]
                : []),
        // O texto do Anexo I condiciona a regra à operação com o exterior (idDest=3), que a B11a-10 já recusa na NFC-e.
        Regra("E17-10", "729", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com informação da IE do destinatário",
            (n, _) => n.Identificacao.LocalDestino == LocalDestino.Exterior && !string.IsNullOrEmpty(n.Destinatario?.InscricaoEstadual)
                ? [new OcorrenciaRegra($"{Raiz}/dest/IE")]
                : []),
        Regra("E18-10", "730", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com Inscrição Suframa",
            (n, _) => !string.IsNullOrEmpty(n.Destinatario?.InscricaoSuframa) ? [new OcorrenciaRegra($"{Raiz}/dest/ISUF")] : []),

        Regra("J01-10", "736", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de Veículos novos",
            (n, c) => GrupoItem(n, c, item => item.Produto.VeiculoNovo is not null ? "prod/veicProd" : null)),
        Regra("L01-10", "738", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de Armamentos",
            (n, c) => GrupoItem(n, c, item => item.Produto.Armas.Count > 0 ? $"prod/{Passo("arma", 0, item.Produto.Armas.Count)}" : null)),
        Regra("LB01-10", "348", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo RECOPI",
            (n, c) => GrupoItem(n, c, item => !string.IsNullOrEmpty(item.Produto.Recopi) ? "prod/nRECOPI" : null)),
        Regra("N12-50", "741", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com Partilha de ICMS entre UF",
            (n, c) => GrupoItem(n, c, item => item.Imposto.Icms is IcmsPartilha ? "imposto/ICMS/ICMSPart" : null)),
        Regra("NA01-10", "807", Ambos, AplicacaoRegra.Obrigatoria,
            "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de ICMS para a UF do destinatário",
            (n, c) => GrupoItem(n, c, item => item.Imposto.IcmsUFDestino is not null ? "imposto/ICMSUFDest" : null)),
        Regra("O01-10", "742", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do IPI",
            (n, c) => GrupoItem(n, c, item => item.Imposto.Ipi is not null ? "imposto/IPI" : null)),
        Regra("P01-10", "743", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do II",
            (n, c) => GrupoItem(n, c, item => item.Imposto.ImpostoImportacao is not null ? "imposto/II" : null)),
        Regra("R01-10", "746", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do PIS-ST",
            (n, c) => GrupoItem(n, c, item => item.Imposto.PisSt is not null ? "imposto/PISST" : null)),
        Regra("T01-10", "749", Ambos, AplicacaoRegra.Obrigatoria, "NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo da COFINS-ST",
            (n, c) => GrupoItem(n, c, item => item.Imposto.CofinsSt is not null ? "imposto/COFINSST" : null)),

        Regra("X03-10", "754", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com dados do Transportador",
            (n, _) => n.Transporte.Transportador is not null && n.Identificacao.Presenca != PresencaComprador.NaoPresencialEntrega
                ? [new OcorrenciaRegra($"{Raiz}/transp/transporta")]
                : []),
        Regra("X03-20", "786", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e de entrega a domicílio sem dados do Transportador",
            (n, _) => n.Transporte.Transportador is null && n.Identificacao.Presenca == PresencaComprador.NaoPresencialEntrega
                ? [new OcorrenciaRegra($"{Raiz}/transp")]
                : []),
        Regra("YA02-10", "857", SoNFCe, AplicacaoRegra.Obrigatoria, "Informado Duplicata Mercantil como Forma de Pagamento",
            (n, _) => MeiosPagamento(n, MeioPagamento.DuplicataMercantil)),
        Regra("YA02-40", "899", SoNFCe, AplicacaoRegra.Obrigatoria, "Informado incorretamente o campo meio de pagamento",
            (n, _) => MeiosPagamento(n, MeioPagamento.Nenhum)),
        // O QR Code (docs/CONTRATOS.md 16, decisão 10): as três que a montagem não evita, porque dependem do que quem usa
        // configurou ou preencheu. As outras do grupo ZX a lib evita gerando o grupo (escrita) ou deixa para a SEFAZ.
        Regra("ZX01-10", "393", SoNFe, AplicacaoRegra.Obrigatoria, "NF-e com o grupo de Informações Suplementares",
            (n, _) => n.Suplementares is not null && n.Identificacao.FormatoImpressao != FormatoImpressao.DanfeSimplificadoTipo2
                ? [new OcorrenciaRegra("NFe/infNFeSupl", null, "Suplementares")]
                : []),
        Regra("ZX02-222", "444", SoNFCe, AplicacaoRegra.Obrigatoria, "NFC-e com qrCode na versão 2 para Pessoa Física",
            (n, c) => c.Nucleo.NFCe is { QrCode: VersaoQrCode.V2 } && !string.IsNullOrEmpty(n.Emitente.Cpf)
                ? [new OcorrenciaRegra($"{Raiz}/emit/CPF")]
                : []),
        Regra("ZX02-220", "672", SoNFe, AplicacaoRegra.Obrigatoria, "NF-e com DANFE Simplificado Tipo 2 com versão de qrCode não permitida",
            (n, c) => n.Identificacao.FormatoImpressao == FormatoImpressao.DanfeSimplificadoTipo2 && c.Nucleo.NFCe is { QrCode: VersaoQrCode.V2 }
                ? [new OcorrenciaRegra("NFe/infNFeSupl/qrCode", "versão 2", "Suplementares.QrCode")]
                : []),
        // A contingência offline e a justificativa (docs/CONTRATOS.md 17, decisão 5): o tipo de emissão é o da nota ou, nulo,
        // o do núcleo; a data e a justificativa, as da nota ou, nulas, as da contingência do núcleo (o que a escrita grava).
        Regra("B22-10", "711", SoNFe, AplicacaoRegra.Obrigatoria, "NF-e com contingência off-line em operação não permitida",
            (n, c) => (n.Identificacao.TipoEmissao ?? c.Nucleo.Emissao) == TipoEmissao.ContingenciaOffline
                    && n.Identificacao.FormatoImpressao != FormatoImpressao.DanfeSimplificadoTipo2
                ? [new OcorrenciaRegra($"{Raiz}/ide/tpEmis", "tpEmis 9")]
                : []),
        Regra("B28-10", "556", Ambos, AplicacaoRegra.Obrigatoria,
            "Justificativa de entrada em contingência não deve ser informada para tipo de emissão normal",
            (n, c) => (n.Identificacao.TipoEmissao ?? c.Nucleo.Emissao) == TipoEmissao.Normal ? Contingencia(n, c, preenchida: true) : []),
        Regra("B28-20", "557", Ambos, AplicacaoRegra.Obrigatoria, "A Justificativa de entrada em contingência deve ser informada",
            (n, c) => (n.Identificacao.TipoEmissao ?? c.Nucleo.Emissao) != TipoEmissao.Normal ? Contingencia(n, c, preenchida: false) : []),
        Regra("ZA01-30", "814", SoNFCe, AplicacaoRegra.Obrigatoria, "Nota Fiscal com grupo de comércio exterior",
            (n, _) => n.Exportacao is not null ? [new OcorrenciaRegra($"{Raiz}/exporta")] : []),
    ];

    // Os campos da contingência (dhCont e xJust) que estão preenchidos (556) ou ausentes (557), pelo valor que a escrita grava.
    private static IEnumerable<OcorrenciaRegra> Contingencia(NotaFiscal nota, ContextoRegra contexto, bool preenchida)
    {
        var ide = nota.Identificacao;
        var inicio = ide.InicioContingencia ?? contexto.Nucleo.Contingencia?.Inicio;
        var justificativa = ide.JustificativaContingencia ?? contexto.Nucleo.Contingencia?.Justificativa;
        if ((inicio is not null) == preenchida)
            yield return new OcorrenciaRegra($"{Raiz}/ide/dhCont", null, "Identificacao.InicioContingencia");
        if (!string.IsNullOrEmpty(justificativa) == preenchida)
            yield return new OcorrenciaRegra($"{Raiz}/ide/xJust", null, "Identificacao.JustificativaContingencia");
    }

    // A NFC-e, ou a NF-e com o DANFE Simplificado Tipo 2: as regras da NT 2026.002 valem para as duas.
    private static bool Simplificada(NotaFiscal nota, ContextoRegra contexto) =>
        contexto.Nucleo.Documento == Documento.NFCe || nota.Identificacao.FormatoImpressao == FormatoImpressao.DanfeSimplificadoTipo2;

    // O grupo que o item não pode ter: o caminho dele dentro do det, ou nulo quando o item não o tem.
    private static IEnumerable<OcorrenciaRegra> GrupoItem(NotaFiscal nota, ContextoRegra contexto, Func<Item, string?> grupo)
    {
        if (!Simplificada(nota, contexto))
            yield break;
        for (var i = 0; i < nota.Itens.Count; i++)
        {
            if (grupo(nota.Itens[i]) is { } caminho)
                yield return new OcorrenciaRegra($"{Item(nota, i)}/{caminho}");
        }
    }

    private static IEnumerable<OcorrenciaRegra> MeiosPagamento(NotaFiscal nota, MeioPagamento meio) =>
        nota.Pagamento.Formas.Select((f, i) => (f.Meio, i)).Where(f => f.Meio == meio)
            .Select(f => new OcorrenciaRegra($"{Raiz}/pag/{Passo("detPag", f.i, nota.Pagamento.Formas.Count)}/tPag"));
}
