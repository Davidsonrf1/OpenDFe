using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// As NF-e dos casos do DANFE (etapa 7.3), montadas à mão como se tivessem sido lidas: emitente em Belo Horizonte, produção,
/// emissão normal, retrato, a data de 14/10/2026, e os totais somados dos itens. Os valores são fixos para o golden não
/// depender de relógio nem de assinatura.
/// </summary>
internal static class NotasDanfe
{
    internal const string Cnpj = "11222333000181";
    internal const string CnpjDestinatario = "34028316000103";
    internal const string UrlConsulta = "www.fazenda.mg.gov.br/nfe/consulta";
    internal const string QrCodeFixo = "https://nfe.fazenda.mg.gov.br/portalnfe/sistema/qrcode.xhtml?p=CHAVE|3|1";

    internal static readonly DateTimeOffset Emissao = new(2026, 10, 14, 9, 30, 0, TimeSpan.FromHours(-3));

    internal static Imagem1Bit Logotipo { get; } = new(120, 40, new byte[15 * 40]);

    // A NF-e base: VENDA DE MERCADORIA para o cliente de Belo Horizonte, saída, sem itens nem totais (cada caso põe os seus).
    internal static NotaFiscal Base(int numero = 123, int serie = 1, FormatoImpressao formato = FormatoImpressao.DanfeRetrato)
    {
        var nota = new NotaFiscal
        {
            Identificacao =
            {
                NaturezaOperacao = "VENDA DE MERCADORIA", Modelo = Documento.NFe, Serie = serie, Numero = numero, Emissao = Emissao,
                SaidaEntrada = new DateTimeOffset(2026, 10, 14, 11, 45, 10, TimeSpan.FromHours(-3)), Tipo = TipoNota.Saida,
                LocalDestino = LocalDestino.Interna, FormatoImpressao = formato, TipoEmissao = TipoEmissao.Normal,
                Ambiente = Ambiente.Producao, Finalidade = FinalidadeEmissao.Normal,
            },
            Emitente = new Emitente
            {
                Cnpj = Cnpj, Nome = "Distribuidora Fornecedor Ltda", InscricaoEstadual = "0623079040081",
                InscricaoEstadualSubstituto = "0623079040999", InscricaoMunicipal = "1234567", RegimeTributario = RegimeTributario.Normal,
                Endereco =
                {
                    Logradouro = "Av. Amazonas", Numero = "100", Complemento = "Sala 2", Bairro = "Centro", Municipio = "Belo Horizonte",
                    UF = UF.MG, Cep = "30180001", Telefone = "3133334444",
                },
            },
            Destinatario = new Destinatario
            {
                Cnpj = CnpjDestinatario, Nome = "Loja Centro Ltda", InscricaoEstadual = "110042490114",
                IndicadorIE = IndicadorIEDestinatario.Contribuinte,
                Endereco = new EnderecoDestinatario
                {
                    Logradouro = "Rua A", Numero = "1", Complemento = "Loja 3", Bairro = "Centro", Municipio = "Belo Horizonte",
                    UF = UF.MG, Cep = "30130010", Telefone = "31988887777",
                },
            },
        };
        nota.Transporte.Modalidade = ModalidadeFrete.Remetente;
        nota.Suplementares = new InformacoesSuplementares { UrlChave = UrlConsulta };
        return nota;
    }

    // O item de mercadoria, com o ICMS dado (o ICMS00 de 18% por padrão) e o IPI opcional.
    internal static Item ItemDanfe(this NotaFiscal nota, string codigo, string descricao, decimal quantidade, string unidade,
        decimal unitario, Icms? icms = null, Ipi? ipi = null, string ncm = "96091000", int cfop = 5102, string? adicional = null)
    {
        var valor = Math.Round(quantidade * unitario, 2, MidpointRounding.AwayFromZero);
        icms ??= new Icms00
        {
            Origem = OrigemMercadoria.Nacional, Cst = CstIcms.Tributada, ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao,
            BaseCalculo = valor, Aliquota = 18m, Valor = Math.Round(valor * 0.18m, 2, MidpointRounding.AwayFromZero),
        };
        var item = new Item
        {
            Produto =
            {
                Codigo = codigo, Gtin = "SEM GTIN", Descricao = descricao, Ncm = ncm, Cfop = cfop, UnidadeComercial = unidade,
                QuantidadeComercial = quantidade, ValorUnitarioComercial = unitario, Valor = valor, GtinTributavel = "SEM GTIN",
                UnidadeTributavel = unidade, QuantidadeTributavel = quantidade, ValorUnitarioTributavel = unitario,
                Totalizacao = IndicadorTotal.Compoe,
            },
            Imposto = { Icms = icms, Ipi = ipi },
            InformacoesAdicionais = adicional,
        };
        nota.Itens.Add(item);
        return item;
    }

    internal static Ipi IpiTributado(decimal baseCalculo, decimal aliquota) => new()
    {
        CodigoEnquadramento = "999",
        Tributado = new IpiTributado
        {
            Cst = CstIpi.SaidaTributada, BaseCalculo = baseCalculo, Aliquota = aliquota,
            Valor = Math.Round(baseCalculo * aliquota / 100m, 2, MidpointRounding.AwayFromZero),
        },
    };

    // O total: soma os itens (o ICMS, a ST, o IPI e o valor dos produtos) e o vNF soma os acréscimos e tira o desconto.
    internal static void TotalizarDanfe(this NotaFiscal nota, decimal desconto = 0m, decimal frete = 0m, decimal seguro = 0m,
        decimal outras = 0m, decimal? tributos = null)
    {
        var total = nota.Total.Icms;
        total.BaseCalculo = nota.Itens.Sum(i => BaseIcms(i.Imposto.Icms));
        total.Valor = nota.Itens.Sum(i => ValorIcms(i.Imposto.Icms));
        total.BaseCalculoSt = nota.Itens.Sum(i => i.Imposto.Icms is Icms10 s ? s.BaseCalculoSt ?? 0m : 0m);
        total.ValorSt = nota.Itens.Sum(i => i.Imposto.Icms is Icms10 s ? s.ValorSt ?? 0m : 0m);
        total.ValorIpi = nota.Itens.Sum(i => i.Imposto.Ipi?.Tributado?.Valor ?? 0m);
        total.ValorProdutos = nota.Itens.Sum(i => i.Produto.Valor ?? 0m);
        total.Frete = frete;
        total.Seguro = seguro;
        total.Desconto = desconto;
        total.OutrasDespesas = outras;
        total.ValorNota = total.ValorProdutos - desconto + frete + seguro + outras + total.ValorSt + total.ValorIpi;
        total.ValorTotalTributos = tributos;
        nota.Pagamento.Formas.Clear();
        nota.Pagamento.Formas.Add(new FormaPagamento { Meio = MeioPagamento.BoletoBancario, Valor = total.ValorNota });
    }

    private static decimal BaseIcms(Icms? icms) => icms switch
    {
        Icms00 i => i.BaseCalculo ?? 0m,
        Icms10 i => i.BaseCalculo ?? 0m,
        Icms20 i => i.BaseCalculo ?? 0m,
        IcmsSn900 i => i.BaseCalculo ?? 0m,
        _ => 0m,
    };

    private static decimal ValorIcms(Icms? icms) => icms switch
    {
        Icms00 i => i.Valor ?? 0m,
        Icms10 i => i.Valor ?? 0m,
        Icms20 i => i.Valor ?? 0m,
        IcmsSn900 i => i.Valor ?? 0m,
        _ => 0m,
    };

    internal static Protocolo ProtocoloAutorizado(string numero = "131260000012345") => new()
    {
        Ambiente = Ambiente.Producao, Chave = "", Numero = numero, Status = 100, Motivo = "Autorizado o uso da NF-e",
        Recebimento = Emissao.AddSeconds(5),
    };

    // A nota como a leitura a devolve: com a chave do número, da série e do tipo de emissão, e o protocolo quando passado.
    internal static NotaLida Lida(NotaFiscal nota, Protocolo? protocolo = null)
    {
        var ide = nota.Identificacao;
        var chave = ChaveAcesso.Montar(UF.MG, ide.Emissao ?? Emissao, nota.Emitente.Cnpj ?? Cnpj, ide.Modelo ?? Documento.NFe,
            ide.Serie ?? 1, ide.Numero ?? 1, ide.TipoEmissao ?? TipoEmissao.Normal, 12345678);
        if (protocolo is not null)
            protocolo.Chave = chave.ToString();
        return new NotaLida(nota, chave, protocolo, string.Empty, []);
    }

    internal static NotaLida Autorizada(NotaFiscal nota) => Lida(nota, ProtocoloAutorizado());

    // Um item de caneta de 10 caixas a 15,00 e os totais.
    internal static NotaFiscal AcrescentarUmItem(this NotaFiscal nota)
    {
        nota.ItemDanfe("CAN-001", "Caneta azul", 10m, "CX", 15m);
        nota.TotalizarDanfe();
        return nota;
    }

    // A NF-e simples dos casos: um item com o ICMS de 18% e o desconto, autorizada.
    internal static NotaFiscal Simples()
    {
        var nota = Base();
        nota.ItemDanfe("CAN-001", "Caneta azul", 10m, "CX", 15m);
        nota.TotalizarDanfe();
        return nota;
    }
}
