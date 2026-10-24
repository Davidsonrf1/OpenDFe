using OpenDFe.Impressao;

namespace OpenDFe.NFe;

/// <summary>
/// Monta o DANFE NFC-e (<see cref="DocumentoImpressao"/>) a partir da NFC-e lida, na ordem das divisões do Manual de
/// Padrões Técnicos do DANFE NFC-e e QR Code 6.0 (docs/IMPRESSAO.md 1). Só lê a nota: não confere regra do MOC nem o schema.
/// </summary>
internal static class MontagemDanfce
{
    private const string TextoContingencia = "EMITIDA EM CONTINGÊNCIA";
    private const string TextoPendente = "Pendente de autorização";
    private const string TextoHomologacao = "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";

    // As colunas do valor à direita: o total e a quantidade (até 9.999.999,99) e o "VALOR PAGO R$" do cabeçalho.
    private const int ColunasValor = 12;
    private const int ColunasValorPago = 14;

    private const double LadoQrCodeMm = 30;

    internal static DocumentoImpressao Montar(NotaLida lida, OpcoesDanfce? opcoes)
    {
        ArgumentNullException.ThrowIfNull(lida);
        opcoes ??= new OpcoesDanfce();
        var papel = opcoes.Papel ?? throw new ArgumentException("O papel do DANFCE não foi informado.", nameof(opcoes));
        if (!ReferenceEquals(papel, PapelImpressao.Bobina80) && !ReferenceEquals(papel, PapelImpressao.Bobina58))
            throw new ArgumentException("O DANFCE é impresso na bobina de 80 mm ou de 58 mm.", nameof(opcoes));
        if (!Enum.IsDefined(opcoes.Via))
            throw new ArgumentException("A via do DANFCE não é uma das definidas.", nameof(opcoes));

        var nota = lida.Nota ?? throw new ArgumentException("A leitura não achou a nota: não há o que imprimir.", nameof(lida));
        var chave = lida.Chave ?? throw new ArgumentException("A nota não tem a chave de acesso (Id do infNFe) válida.", nameof(lida));
        var ide = nota.Identificacao;
        if ((ide.Modelo ?? chave.Modelo) != Documento.NFCe)
            throw new ArgumentException("O DANFCE é da NFC-e (modelo 65); a nota não é.", nameof(lida));
        var qrCode = nota.Suplementares?.QrCode;
        var urlChave = nota.Suplementares?.UrlChave;
        if (string.IsNullOrEmpty(qrCode) || string.IsNullOrWhiteSpace(urlChave))
            throw new ArgumentException("A NFC-e sem o QR Code (qrCode) e a URL de consulta (urlChave) das informações suplementares não é imprimível.", nameof(lida));

        var protocolo = lida.Protocolo;
        var autorizada = !string.IsNullOrWhiteSpace(protocolo?.Numero);
        if (autorizada && protocolo!.Status is { } status && status is not (100 or 150))
            throw new ArgumentException($"O protocolo da nota é de situação {status}: o DANFCE só se imprime da NFC-e autorizada (100 ou 150).", nameof(lida));
        var contingencia = ide.TipoEmissao == TipoEmissao.ContingenciaOffline && !autorizada;

        var elementos = new List<ElementoImpressao>();
        if (opcoes.Logotipo is { } logotipo)
            elementos.Add(new ImagemImpressao(logotipo));
        Cabecalho(elementos, nota);
        if (contingencia)
            Contingencia(elementos, papel);
        elementos.Add(Tracejado());
        Itens(elementos, nota, papel);
        elementos.Add(Tracejado());
        Totais(elementos, nota, papel);
        Pagamentos(elementos, nota, papel);
        elementos.Add(Tracejado());
        Consulta(elementos, chave, urlChave);
        Consumidor(elementos, nota);
        Identificacao(elementos, nota, opcoes.Via, contingencia);
        ProtocoloAutorizacao(elementos, protocolo, autorizada);
        if (contingencia)
            Contingencia(elementos, papel);
        elementos.Add(new QrCodeImpressao(qrCode) { LadoMm = LadoQrCodeMm });
        elementos.Add(Tracejado());
        Mensagens(elementos, nota, opcoes);
        if (opcoes.AssinaturaSoftware is { } assinatura)
            elementos.Add(new ImagemImpressao(assinatura));

        return new DocumentoImpressao(papel, elementos)
        {
            Titulo = $"DANFE NFC-e {FormatadorImpressao.Zeros(ide.Numero ?? 0, 9)} série {FormatadorImpressao.Zeros(ide.Serie ?? 0, 3)}",
        };
    }

    private static void Cabecalho(List<ElementoImpressao> elementos, NotaFiscal nota)
    {
        var emitente = nota.Emitente;
        var documento = !string.IsNullOrEmpty(emitente.Cnpj)
            ? "CNPJ: " + FormatadorImpressao.Cnpj(emitente.Cnpj)
            : !string.IsNullOrEmpty(emitente.Cpf) ? "CPF: " + FormatadorImpressao.Cpf(emitente.Cpf) : string.Empty;
        var identificacao = string.Join(' ', new[] { documento, emitente.Nome }.Where(t => !string.IsNullOrEmpty(t)));
        elementos.Add(new TextoImpressao(identificacao) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });

        var endereco = emitente.Endereco;
        var texto = Endereco(endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Municipio,
            endereco.UF?.Sigla(), endereco.Cep);
        if (texto.Length > 0)
            elementos.Add(new TextoImpressao(texto) { Alinhamento = AlinhamentoImpressao.Centro });
        elementos.Add(new TextoImpressao("Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica")
        {
            Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno,
        });
    }

    // "Av da Tecnologia, 030, Centro, Rio de Janeiro, RJ, CEP: 88.999-999", sem o país (manual, 3.1.1).
    private static string Endereco(string? logradouro, string? numero, string? complemento, string? bairro, string? municipio,
        string? uf, string? cep)
    {
        var partes = new List<string?> { logradouro, numero, complemento, bairro, municipio, uf };
        var texto = string.Join(", ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
        if (!string.IsNullOrEmpty(cep))
            texto += (texto.Length > 0 ? ", " : string.Empty) + "CEP: " + FormatadorImpressao.Cep(cep);
        return texto;
    }

    // Em destaque, em duas linhas, centralizado (manual, 3.1.8): a primeira em tamanho duplo quando cabe na bobina.
    private static void Contingencia(List<ElementoImpressao> elementos, PapelImpressao papel)
    {
        var duplo = papel.ColunasTamanho(TamanhoTexto.Duplo) >= TextoContingencia.Length;
        elementos.Add(new TextoImpressao(TextoContingencia)
        {
            Alinhamento = AlinhamentoImpressao.Centro, Negrito = true, Tamanho = duplo ? TamanhoTexto.Duplo : TamanhoTexto.Normal,
        });
        elementos.Add(new TextoImpressao(TextoPendente) { Alinhamento = AlinhamentoImpressao.Centro });
    }

    private static void Itens(List<ElementoImpressao> elementos, NotaFiscal nota, PapelImpressao papel)
    {
        elementos.Add(new TextoImpressao("Código Descrição") { Negrito = true });
        elementos.Add(Duas("Qtde UN x Vl Unit", "Vl Total", papel.Colunas, ColunasValor, negrito: true));
        foreach (var item in nota.Itens)
        {
            var produto = item.Produto;
            elementos.Add(new TextoImpressao($"{produto.Codigo} {produto.Descricao}".Trim()));
            var quantidade = produto.QuantidadeComercial is { } q ? FormatadorImpressao.Quantidade(q) : string.Empty;
            var unitario = produto.ValorUnitarioComercial is { } u ? FormatadorImpressao.Unitario(u) : string.Empty;
            var total = produto.Valor is { } v ? FormatadorImpressao.Dinheiro(v) : string.Empty;
            var detalhe = string.Join(' ', new[] { quantidade, produto.UnidadeComercial }.Where(t => !string.IsNullOrEmpty(t)))
                + (unitario.Length > 0 ? " x " + unitario : string.Empty);
            elementos.Add(Duas(detalhe, total, papel.Colunas, ColunasValor));
        }
    }

    private static void Totais(List<ElementoImpressao> elementos, NotaFiscal nota, PapelImpressao papel)
    {
        var total = nota.Total.Icms;
        var produtos = total.ValorProdutos ?? nota.Itens.Sum(i => i.Produto.Valor ?? 0m);
        elementos.Add(Duas("Qtde. total de itens", nota.Itens.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), papel.Colunas, ColunasValor));
        elementos.Add(Duas("Valor total R$", FormatadorImpressao.Dinheiro(produtos), papel.Colunas, ColunasValor));
        Acrescimo(elementos, "Desconto R$", total.Desconto, papel);
        Acrescimo(elementos, "Frete R$", total.Frete, papel);
        Acrescimo(elementos, "Seguro R$", total.Seguro, papel);
        Acrescimo(elementos, "Outras despesas R$", total.OutrasDespesas, papel);
        // O valor a pagar só quando há acréscimo ou desconto (manual, 3.1.3): o vNF difere do total dos itens.
        if (total.ValorNota is { } valorNota && valorNota != produtos)
            elementos.Add(Duas("Valor a Pagar R$", FormatadorImpressao.Dinheiro(valorNota), papel.Colunas, ColunasValor, negrito: true));
    }

    // A linha só se o valor existir (manual, 3.1.3).
    private static void Acrescimo(List<ElementoImpressao> elementos, string rotulo, decimal? valor, PapelImpressao papel)
    {
        if (valor is > 0m)
            elementos.Add(Duas(rotulo, FormatadorImpressao.Dinheiro(valor.Value), papel.Colunas, ColunasValor));
    }

    private static void Pagamentos(List<ElementoImpressao> elementos, NotaFiscal nota, PapelImpressao papel)
    {
        elementos.Add(Duas("FORMA PAGAMENTO", "VALOR PAGO R$", papel.Colunas, ColunasValorPago));
        foreach (var forma in nota.Pagamento.Formas)
        {
            elementos.Add(Duas(DescricaoPagamento(forma), forma.Valor is { } v ? FormatadorImpressao.Dinheiro(v) : string.Empty,
                papel.Colunas, ColunasValorPago));
        }

        if (nota.Pagamento.Troco is > 0m)
            elementos.Add(Duas("Troco R$", FormatadorImpressao.Dinheiro(nota.Pagamento.Troco.Value), papel.Colunas, ColunasValorPago));
    }

    // O nome do meio de pagamento: o xPag do "Outros" (99) ou o da tabela do Portal Nacional (IT 2024.002).
    internal static string DescricaoPagamento(FormaPagamento forma)
    {
        if (!string.IsNullOrWhiteSpace(forma.Descricao))
            return forma.Descricao;
        return forma.Meio switch
        {
            MeioPagamento.Dinheiro => "Dinheiro",
            MeioPagamento.Cheque => "Cheque",
            MeioPagamento.CartaoCredito => "Cartão de Crédito",
            MeioPagamento.CartaoDebito => "Cartão de Débito",
            MeioPagamento.CartaoLoja => "Cartão da Loja",
            MeioPagamento.ValeAlimentacao => "Vale Alimentação",
            MeioPagamento.ValeRefeicao => "Vale Refeição",
            MeioPagamento.ValePresente => "Vale Presente",
            MeioPagamento.ValeCombustivel => "Vale Combustível",
            MeioPagamento.DuplicataMercantil => "Duplicata Mercantil",
            MeioPagamento.BoletoBancario => "Boleto Bancário",
            MeioPagamento.DepositoBancario => "Depósito Bancário",
            MeioPagamento.PixDinamico => "PIX Dinâmico",
            MeioPagamento.Ted => "Transferência (TED)",
            MeioPagamento.ProgramaFidelidade => "Programa de Fidelidade",
            MeioPagamento.PixEstatico => "PIX Estático",
            MeioPagamento.CreditoLoja => "Crédito em Loja",
            MeioPagamento.PagamentoEletronicoNaoInformado => "Pagamento Eletrônico Não Informado",
            MeioPagamento.PixAutomatico => "PIX Automático",
            MeioPagamento.TefBookTransfer => "TEF Book Transfer",
            MeioPagamento.Nenhum => "Sem Pagamento",
            MeioPagamento.PagamentoPosterior => "Pagamento Posterior",
            MeioPagamento.Outros => "Outros",
            null => string.Empty,
            { } outro => "Outros (" + ((int)outro).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")",
        };
    }

    private static void Consulta(List<ElementoImpressao> elementos, ChaveAcesso chave, string urlChave)
    {
        elementos.Add(new TextoImpressao("Consulte pela Chave de Acesso em") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        elementos.Add(new TextoImpressao(urlChave.Trim()) { Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno });
        elementos.Add(new TextoImpressao(FormatadorImpressao.Chave(chave.ToString())) { Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno });
    }

    // Manual, 3.1.6: o documento só se consta do XML; nome e endereço na entrega em domicílio (indPres 4).
    private static void Consumidor(List<ElementoImpressao> elementos, NotaFiscal nota)
    {
        var destinatario = nota.Destinatario;
        string? identificacao = null;
        if (!string.IsNullOrEmpty(destinatario?.Cnpj))
            identificacao = "CONSUMIDOR CNPJ: " + FormatadorImpressao.Cnpj(destinatario.Cnpj);
        else if (!string.IsNullOrEmpty(destinatario?.Cpf))
            identificacao = "CONSUMIDOR CPF: " + FormatadorImpressao.Cpf(destinatario.Cpf);
        else if (!string.IsNullOrEmpty(destinatario?.IdEstrangeiro))
            identificacao = "CONSUMIDOR Id. Estrangeiro: " + destinatario.IdEstrangeiro;

        var entrega = nota.Identificacao.Presenca == PresencaComprador.NaoPresencialEntrega;
        var endereco = destinatario?.Endereco;
        var enderecoTexto = endereco is null
            ? string.Empty
            : Endereco(endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Municipio,
                endereco.UF?.Sigla(), endereco.Cep);

        if (identificacao is null && !entrega)
        {
            elementos.Add(new TextoImpressao("CONSUMIDOR NÃO IDENTIFICADO") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
            return;
        }

        var partes = new[] { identificacao, destinatario?.Nome, enderecoTexto }.Where(p => !string.IsNullOrWhiteSpace(p));
        elementos.Add(new TextoImpressao(string.Join(" - ", partes)) { Alinhamento = AlinhamentoImpressao.Centro });
    }

    // Manual, 3.1.7 e 3.1.8: número, série e a data e hora de emissão (a da nota, sem converter), e a via.
    private static void Identificacao(List<ElementoImpressao> elementos, NotaFiscal nota, ViaDanfce via, bool contingencia)
    {
        var ide = nota.Identificacao;
        var emissao = ide.Emissao is { } e ? " " + FormatadorImpressao.DataHora(e) : string.Empty;
        elementos.Add(new TextoImpressao(
            $"NFC-e nº {FormatadorImpressao.Zeros(ide.Numero ?? 0, 9)} Série {FormatadorImpressao.Zeros(ide.Serie ?? 0, 3)}{emissao}")
        {
            Alinhamento = AlinhamentoImpressao.Centro, Negrito = true,
        });

        if (via == ViaDanfce.Estabelecimento)
            elementos.Add(new TextoImpressao("Via do Estabelecimento") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        else if (contingencia)
            elementos.Add(new TextoImpressao("Via do Consumidor") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
    }

    // Manual, 3.1.7: o protocolo e a data da autorização; na contingência (sem protocolo) a informação é suprimida.
    private static void ProtocoloAutorizacao(List<ElementoImpressao> elementos, Protocolo? protocolo, bool autorizada)
    {
        if (!autorizada || protocolo is null)
            return;
        elementos.Add(new TextoImpressao("Protocolo de autorização: " + protocolo.Numero) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        if (protocolo.Recebimento is { } recebimento)
        {
            elementos.Add(new TextoImpressao("Data de autorização " + FormatadorImpressao.DataHora(recebimento))
            {
                Alinhamento = AlinhamentoImpressao.Centro,
            });
        }
    }

    // As divisões VIII e IX (manual, 3.1.8 e 3.1.9) e o que o aplicativo põe depois delas.
    private static void Mensagens(List<ElementoImpressao> elementos, NotaFiscal nota, OpcoesDanfce opcoes)
    {
        var adicionais = nota.InformacoesAdicionais;
        if (!string.IsNullOrWhiteSpace(adicionais?.Fisco))
            elementos.Add(new TextoImpressao(adicionais.Fisco));
        if (nota.Identificacao.Ambiente == Ambiente.Homologacao)
            elementos.Add(new TextoImpressao(TextoHomologacao)
            {
                Alinhamento = AlinhamentoImpressao.Centro, Negrito = true, Tamanho = TamanhoTexto.Pequeno,
            });
        if (!string.IsNullOrWhiteSpace(adicionais?.Complementares))
            elementos.Add(new TextoImpressao(adicionais.Complementares));
        if (nota.Total.Icms.ValorTotalTributos is { } tributos)
        {
            elementos.Add(new TextoImpressao("Tributos Totais Incidentes (Lei Federal 12.741/2012): R$" + FormatadorImpressao.Dinheiro(tributos))
            {
                Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno,
            });
        }

        if (!string.IsNullOrWhiteSpace(opcoes.TextoRodape))
            elementos.Add(new TextoImpressao(opcoes.TextoRodape) { Alinhamento = AlinhamentoImpressao.Centro });
    }

    private static SeparadorImpressao Tracejado() => new();

    // Uma linha com o rótulo à esquerda e o valor à direita, o valor na largura em colunas dada.
    private static LinhaImpressao Duas(string esquerda, string direita, int colunas, int colunasDireita, bool negrito = false) =>
        new([
            new CelulaImpressao(esquerda, colunas - colunasDireita) { Negrito = negrito },
            new CelulaImpressao(direita, colunasDireita) { Alinhamento = AlinhamentoImpressao.Direita, Negrito = negrito },
        ]);
}
