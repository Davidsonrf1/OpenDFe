using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// As NFC-e dos casos do DANFCE (etapa 7.2), montadas à mão como se tivessem sido lidas: os valores dos exemplos do
/// Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0 (item 3.2) e os casos de borda. O QR Code e a URL de consulta
/// são fixos, para o golden não depender de CSC nem de hash.
/// </summary>
internal static class NotasDanfce
{
    internal const string Cnpj = "11222333000181";
    internal const string UrlConsulta = "www.fazenda.mg.gov.br/nfce/consulta";
    internal const string QrCodeFixo = "https://nfce.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml?p=CHAVE|2|1|1|ABCDEF0123456789ABCDEF0123456789ABCDEF01";

    // A data do exemplo do manual: 10/03/2015 15:03:53, no -03:00 (a hora local do emitente).
    internal static readonly DateTimeOffset Emissao = new(2015, 3, 10, 15, 3, 53, TimeSpan.FromHours(-3));

    internal static Imagem1Bit Logotipo { get; } = new(120, 40, new byte[15 * 40]);

    internal static Imagem1Bit Assinatura { get; } = new(96, 24, new byte[12 * 24]);

    // A NFC-e base: emitente em Belo Horizonte, produção, emissão normal, pagamento em dinheiro, sem consumidor, e a chave
    // montada com o número e a série dela. Sem itens nem totais: cada caso põe os seus (Item e Totalizar).
    internal static NotaFiscal Base(int numero = 1, int serie = 1)
    {
        var nota = NotasTeste.NFCe();
        nota.Itens.Clear();
        nota.Pagamento.Formas.Clear();
        nota.Identificacao.Numero = numero;
        nota.Identificacao.Serie = serie;
        nota.Identificacao.Emissao = Emissao;
        nota.Identificacao.Ambiente = Ambiente.Producao;
        nota.Identificacao.TipoEmissao = TipoEmissao.Normal;
        nota.Identificacao.Modelo = Documento.NFCe;
        nota.Emitente = new Emitente
        {
            Cnpj = Cnpj, Nome = "Razão Social da Empresa", InscricaoEstadual = "0010000000000", RegimeTributario = RegimeTributario.SimplesNacional,
            Endereco =
            {
                Logradouro = "Av da Tecnologia", Numero = "030", Bairro = "Centro", Municipio = "Belo Horizonte", UF = UF.MG, Cep = "30110000",
            },
        };
        nota.Suplementares = new InformacoesSuplementares { QrCode = QrCodeFixo, UrlChave = UrlConsulta };
        return nota;
    }

    internal static void Item(this NotaFiscal nota, string codigo, string descricao, decimal quantidade, string unidade, decimal unitario,
        decimal? total = null)
    {
        nota.Itens.Add(new Item
        {
            Produto =
            {
                Codigo = codigo, Descricao = descricao, UnidadeComercial = unidade, QuantidadeComercial = quantidade,
                ValorUnitarioComercial = unitario, Valor = total ?? Math.Round(quantidade * unitario, 2, MidpointRounding.AwayFromZero),
            },
        });
    }

    internal static void Pagar(this NotaFiscal nota, MeioPagamento meio, decimal valor, string? descricao = null) =>
        nota.Pagamento.Formas.Add(new FormaPagamento { Meio = meio, Valor = valor, Descricao = descricao });

    // O total: o vProd é a soma dos itens, e o vNF soma os acréscimos e tira o desconto.
    internal static void Totalizar(this NotaFiscal nota, decimal desconto = 0m, decimal frete = 0m, decimal seguro = 0m,
        decimal outras = 0m, decimal? tributos = null)
    {
        var produtos = nota.Itens.Sum(i => i.Produto.Valor ?? 0m);
        var total = nota.Total.Icms;
        total.ValorProdutos = produtos;
        total.Desconto = desconto;
        total.Frete = frete;
        total.Seguro = seguro;
        total.OutrasDespesas = outras;
        total.ValorNota = produtos - desconto + frete + seguro + outras;
        total.ValorTotalTributos = tributos;
    }

    internal static Protocolo ProtocoloAutorizado(string numero = "131260000000019") => new()
    {
        Ambiente = Ambiente.Producao, Chave = "", Numero = numero, Status = 100, Motivo = "Autorizado o uso da NF-e",
        Recebimento = Emissao.AddSeconds(5),
    };

    // A nota como a leitura a devolve: com a chave do número e da série, e o protocolo quando passado.
    internal static NotaLida Lida(NotaFiscal nota, Protocolo? protocolo = null)
    {
        var ide = nota.Identificacao;
        var chave = ChaveAcesso.Montar(UF.MG, ide.Emissao ?? Emissao, nota.Emitente.Cnpj ?? Cnpj, Documento.NFCe, ide.Serie ?? 1,
            ide.Numero ?? 1, ide.TipoEmissao ?? TipoEmissao.Normal, 12345678);
        if (protocolo is not null)
            protocolo.Chave = chave.ToString();
        return new NotaLida(nota, chave, protocolo, string.Empty, []);
    }

    internal static NotaLida Autorizada(NotaFiscal nota) => Lida(nota, ProtocoloAutorizado());

    // Os quatro itens, o desconto de 8,00 e o troco de 20,00 do exemplo 1 do manual (3.2).
    internal static NotaFiscal Exemplo1()
    {
        var nota = Base();
        nota.Item("003277", "PRODUTO", 1m, "CX", 27.64m);
        nota.Item("085273", "PRODUTO", 3m, "LT", 22.00m);
        nota.Item("807194", "PRODUTO", 1m, "CX", 15.10m);
        nota.Item("046281", "PRODUTO", 1m, "LT", 30.00m);
        nota.Totalizar(desconto: 8m, tributos: 65.62m);
        nota.Pagar(MeioPagamento.Dinheiro, 150.74m);
        nota.Pagamento.Troco = 20m;
        return nota;
    }

    // Dois itens, dois pagamentos no cartão, desconto, frete e a entrega em domicílio com o CPF do consumidor (exemplo 2).
    internal static NotaFiscal Exemplo2()
    {
        var nota = Base();
        nota.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
        nota.Item("003277", "Cadeira Est. Comtemp.", 2m, "Peça", 500m);
        nota.Item("085273", "Mesa", 1m, "Peça", 1500m);
        nota.Totalizar(desconto: 500m, frete: 50m, tributos: 200m);
        nota.Pagar(MeioPagamento.CartaoCredito, 1050m);
        nota.Pagar(MeioPagamento.CartaoCredito, 1000m);
        nota.Destinatario = new Destinatario
        {
            Cpf = "09999999999",
            Endereco = new EnderecoDestinatario
            {
                Logradouro = "Rua Dois de Fevereiro", Numero = "30", Bairro = "Icaraí", Municipio = "Niterói", UF = UF.RJ, Cep = "88999999",
            },
        };
        return nota;
    }

    // A contingência offline do exemplo 3: dois itens, dois pagamentos, desconto e o CNPJ do consumidor.
    internal static NotaFiscal Exemplo3()
    {
        var nota = Base();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        nota.Identificacao.InicioContingencia = Emissao.AddMinutes(-30);
        nota.Identificacao.JustificativaContingencia = "Sem comunicacao com a SEFAZ";
        nota.Item("003277", "Tablet", 2m, "Peça", 500m);
        nota.Item("085273", "Smartphone", 1m, "Peça", 1500m);
        nota.Totalizar(desconto: 500m, tributos: 200m);
        nota.Pagar(MeioPagamento.CartaoCredito, 1000m);
        nota.Pagar(MeioPagamento.CartaoCredito, 1000m);
        nota.Destinatario = new Destinatario { Cnpj = "99999999000199" };
        return nota;
    }

    // Um item só, pago em dinheiro com troco.
    internal static NotaFiscal UmItem()
    {
        var nota = Base();
        nota.Item("123", "CAFE TORRADO 500G", 2m, "UN", 25m);
        nota.Totalizar();
        nota.Pagar(MeioPagamento.Dinheiro, 100m);
        nota.Pagamento.Troco = 50m;
        return nota;
    }
}
