# Montar a nota

[Documentação](../../index.md) › NF-e e NFC-e › Montar a nota

A `NotaFiscal` é o modelo da NF-e e da NFC-e: tudo o que vai dentro do `infNFe`, com nomes descritivos em português. Este artigo mostra como o modelo é organizado, o que você preenche e o que a biblioteca preenche, e traz uma NFC-e e uma NF-e completas.

## Como o modelo é organizado

Cada grupo do XML é uma classe, e cada campo é uma propriedade com o atributo `[Tag]`, que diz a tag e o identificador do campo no MOC:

```csharp
public sealed class Emitente
{
    [Tag("CNPJ", "C02")]  public string? Cnpj { get; set; }
    [Tag("xNome", "C03")] public string? Nome { get; set; }
    [Tag("IE", "C17")]    public string? InscricaoEstadual { get; set; }
    // ...
}
```

O IntelliSense mostra a tag e o identificador em cada propriedade, e a [referência do modelo](../../referencia/opendfe-nfe-modelo.md) lista todas as classes com a tag de cada campo: procure pela tag que você conhece do XML para achar a propriedade.

| Grupo do XML | Propriedade de `NotaFiscal` | Classe |
|---|---|---|
| `ide` | `Identificacao` | `Identificacao` |
| `emit` | `Emitente` | `Emitente` |
| `dest` | `Destinatario` | `Destinatario` (opcional na NFC-e) |
| `retirada`, `entrega` | `Retirada`, `Entrega` | `Local` |
| `autXML` | `AutorizadosXml` | lista de `PessoaAutorizada` |
| `det` | `Itens` | lista de `Item` (o `nItem` é a posição na lista) |
| `total` | `Total` | `Total` (`Icms`, `Issqn`, `IbsCbs`, `ImpostoSeletivo`, `ValorTotalNota`) |
| `transp` | `Transporte` | `Transporte` |
| `cobr` | `Cobranca` | `Cobranca` |
| `pag` | `Pagamento` | `Pagamento` (`Formas`, `Troco`) |
| `infIntermed` | `Intermediador` | `Intermediador` |
| `infAdic` | `InformacoesAdicionais` | `InformacoesAdicionais` |
| `exporta`, `compra`, `cana` | `Exportacao`, `Compra`, `Cana` | |
| `infRespTec` | `ResponsavelTecnico` | `ResponsavelTecnico` |

Os grupos obrigatórios já vêm criados (`nota.Identificacao`, `nota.Emitente`, `nota.Total`...); os opcionais são `null` até você criá-los. As listas vêm vazias.

### Escolhas viram tipos

Onde o leiaute permite um grupo entre vários, o modelo tem uma classe base e uma subclasse para cada grupo:

| Campo | Base | Subclasses |
|---|---|---|
| ICMS do item | `Icms` | `Icms00`, `Icms02`, `Icms10`, `Icms15`, `Icms20`, `Icms30`, `Icms40`, `Icms51`, `Icms53`, `Icms60`, `Icms61`, `Icms70`, `Icms90`, `IcmsPartilha`, `IcmsRepasse`, e do Simples Nacional `IcmsSn101`, `IcmsSn102`, `IcmsSn201`, `IcmsSn202`, `IcmsSn500`, `IcmsSn900` |
| PIS e COFINS | `Pis`, `Cofins` | `PisAliquota`, `PisQuantidade`, `PisNaoTributado`, `PisOutros` (e as mesmas da COFINS) |
| Documento referenciado | `DocumentoReferenciado` | `NFeReferenciada`, `NotaModelo1Referenciada`, `NotaProdutorReferenciada`, `CTeReferenciado`, `CupomFiscalReferenciado`, `NFeSigilosaReferenciada` |

```csharp
item.Imposto.Icms = new Icms00
{
    Origem = OrigemMercadoria.Nacional,
    ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao,
    BaseCalculo = 1000m,
    Aliquota = 18m,
    Valor = 180m,
};
```

O CST e o CSOSN que cada grupo admite estão nas enumerações (`CstIcms`, `CsosnIcms`, `CstPisCofins`): o grupo `IcmsSn102`, por exemplo, serve aos CSOSN 102, 103, 300 e 400.

### Os valores

- **Números** em `decimal`. A biblioteca escreve com as casas que o leiaute define para cada campo; o valor com mais casas que o campo aceita é um problema de schema.
- **Datas e horas** em `DateTimeOffset`, com o fuso. O leiaute exige o deslocamento (`2026-10-20T14:30:00-03:00`): use o horário local do emitente.
- **Datas** sem hora em `DateOnly`.
- **Enumerações** com o valor do XML: `MeioPagamento.Dinheiro` é o `tPag` 01; `FormatoImpressao.DanfeNFCe` é o `tpImp` 4.
- **Textos** como vão no XML, sem formatação: CNPJ e CPF só com os dígitos (ou as letras do CNPJ alfanumérico), CEP sem hífen.

## O que você não preenche

| Campo | De onde vem |
|---|---|
| `cUF`, `mod`, `tpAmb`, `tpEmis` | Da configuração do núcleo (a UF, o documento, o ambiente, o tipo de emissão) |
| `dhCont`, `xJust` (a contingência) | Da `Contingencia` da configuração |
| A chave de acesso (`Id`) e o `cDV` | Calculados na escrita |
| `cNF` (`Identificacao.CodigoNumerico`) | Opcional: sem ele, a biblioteca sorteia um a cada escrita |
| O QR Code (`infNFeSupl`) | Gerado pela biblioteca na NFC-e e na NF-e com o DANFE Simplificado Tipo 2 |
| A assinatura | Feita pelo núcleo |

Esses campos existem no modelo porque a leitura os preenche. Na escrita, deixe-os nulos: se você preencher com um valor diferente do que o núcleo sabe, a montagem acusa `NFE-NUCLEO`.

> **Importante**
> O `cNF` sorteado faz cada escrita gerar uma **chave diferente** para a mesma nota. Por isso o reenvio depois de uma falha usa sempre o XML assinado, nunca a `NotaFiscal` de novo. Veja [Quando a conexão cai](queda-recuperacao.md).

## O que a biblioteca não calcula

A biblioteca **não calcula tributo nem totais**. Você informa a base, a alíquota e o valor de cada imposto, e os totais da nota. O que a biblioteca faz é conferir a coerência que a SEFAZ confere antes de transmitir: o total dos produtos contra a soma dos itens (564), o total da nota contra a soma dos componentes (610), o grupo do ICMS contra o CST. Veja [Validação](../conceitos/validacao.md).

## Uma NFC-e

Uma venda a consumidor final de um emitente do Simples Nacional, paga em dinheiro:

```csharp
using OpenDFe;
using OpenDFe.NFe;

var nota = new NotaFiscal
{
    Identificacao =
    {
        NaturezaOperacao = "VENDA",
        Serie = 1,
        Numero = 123,
        Emissao = DateTimeOffset.Now,
        Tipo = TipoNota.Saida,
        LocalDestino = LocalDestino.Interna,
        MunicipioFatoGerador = 3106200,
        FormatoImpressao = FormatoImpressao.DanfeNFCe,
        Finalidade = FinalidadeEmissao.Normal,
        ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal,
        Presenca = PresencaComprador.Presencial,
        Processo = ProcessoEmissao.AplicativoContribuinte,
        VersaoAplicativo = "MeuPDV 1.0",
    },
    Emitente =
    {
        Cnpj = "11222333000181",
        Nome = "LOJA EXEMPLO LTDA",
        InscricaoEstadual = "0010000000000",
        RegimeTributario = RegimeTributario.SimplesNacional,
        Endereco =
        {
            Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200,
            Municipio = "BELO HORIZONTE", UF = UF.MG, Cep = "30110000",
        },
    },
    Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
    Pagamento = { Formas = { new FormaPagamento { Meio = MeioPagamento.Dinheiro, Valor = 50.00m } } },
};

nota.Itens.Add(new Item
{
    Produto =
    {
        Codigo = "123", Gtin = "SEM GTIN", Descricao = "CAFE TORRADO 500G", Ncm = "09012100", Cfop = 5102,
        UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m, Valor = 50.00m,
        GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m, ValorUnitarioTributavel = 25.00m,
        Totalizacao = IndicadorTotal.Compoe,
    },
    Imposto =
    {
        Icms = new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito },
        Pis = new PisOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
        Cofins = new CofinsOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
    },
});

var total = nota.Total.Icms;
total.BaseCalculo = 0m; total.Valor = 0m; total.ValorDesonerado = 0m; total.ValorFcp = 0m;
total.BaseCalculoSt = 0m; total.ValorSt = 0m; total.ValorFcpSt = 0m; total.ValorFcpStRetido = 0m;
total.ValorProdutos = 50.00m; total.Frete = 0m; total.Seguro = 0m; total.Desconto = 0m;
total.ValorImpostoImportacao = 0m; total.ValorIpi = 0m; total.ValorIpiDevolvido = 0m;
total.ValorPis = 0m; total.ValorCofins = 0m; total.OutrasDespesas = 0m; total.ValorNota = 50.00m;
```

Na NFC-e, o destinatário é opcional. Para identificar o consumidor:

```csharp
nota.Destinatario = new Destinatario
{
    Cpf = "52998224725",
    Nome = "CONSUMIDOR EXEMPLO",
    IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
};
```

## Uma NF-e

A mesma venda, agora entre empresas, com o destinatário contribuinte do ICMS:

```csharp
var nfe = new NotaFiscal
{
    Identificacao =
    {
        NaturezaOperacao = "VENDA DE MERCADORIA",
        Serie = 1,
        Numero = 4567,
        Emissao = DateTimeOffset.Now,
        SaidaEntrada = DateTimeOffset.Now,
        Tipo = TipoNota.Saida,
        LocalDestino = LocalDestino.Interna,
        MunicipioFatoGerador = 3106200,
        FormatoImpressao = FormatoImpressao.DanfeRetrato,
        Finalidade = FinalidadeEmissao.Normal,
        ConsumidorFinal = IndicadorConsumidorFinal.Normal,
        Presenca = PresencaComprador.NaoAplicavel,
        Processo = ProcessoEmissao.AplicativoContribuinte,
        VersaoAplicativo = "MeuERP 1.0",
    },
    Emitente = { /* como na NFC-e */ },
    Destinatario = new Destinatario
    {
        Cnpj = "11444777000161",
        Nome = "CLIENTE EXEMPLO SA",
        IndicadorIE = IndicadorIEDestinatario.Contribuinte,
        InscricaoEstadual = "0620000000000",
        Endereco = new EnderecoDestinatario
        {
            Logradouro = "AV AFONSO PENA", Numero = "1000", Bairro = "CENTRO", CodigoMunicipio = 3106200,
            Municipio = "BELO HORIZONTE", UF = UF.MG, Cep = "30130000", CodigoPais = 1058, Pais = "BRASIL",
        },
    },
    Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
    Pagamento = { Formas = { new FormaPagamento { Meio = MeioPagamento.Dinheiro, Valor = 50.00m } } },
};
// Os itens e os totais como na NFC-e.
```

As diferenças da NF-e para a NFC-e estão na identificação (`FormatoImpressao.DanfeRetrato` ou `DanfePaisagem`, a data de saída, a presença), no destinatário obrigatório e nos grupos que a NFC-e não tem (transporte com transportador e volumes, cobrança, exportação). O núcleo diz o modelo: a mesma `NotaFiscal` vira NF-e num núcleo de `Documento.NFe` e NFC-e num de `Documento.NFCe`.

## A reforma tributária: IBS, CBS e IS

Os grupos do IBS, da CBS e do Imposto Seletivo já estão no modelo:

| Onde | Propriedade | Tag |
|---|---|---|
| Item | `Imposto.IbsCbs` (`IbsCbs`: o CST, a classificação tributária e a tributação) | `IBSCBS` |
| Item | `Imposto.ImpostoSeletivo` | `IS` |
| Totais | `Total.IbsCbs`, `Total.ImpostoSeletivo`, `Total.ValorTotalNota` | `IBSCBSTot`, `ISTot`, `vNFTot` |
| Identificação | `Identificacao.MunicipioFatoGeradorIbsCbs` | `cMunFGIBS` |

```csharp
item.Imposto.IbsCbs = new IbsCbs
{
    Cst = "000",
    ClassificacaoTributaria = "000001",
    Tributacao = new TributacaoIbsCbs
    {
        BaseCalculo = 1000m,
        ValorIbs = 1m,
        IbsUF = { Aliquota = 0.1m, Valor = 1m },
        IbsMunicipio = { Aliquota = 0m, Valor = 0m },
        Cbs = { Aliquota = 0.9m, Valor = 9m },
    },
};
```

O CST e a `cClassTrib` vêm das tabelas oficiais da reforma; a biblioteca não os confere (a SEFAZ confere). Um campo de uma Nota Técnica ainda não vigente só é escrito se você o preencher.

## Campos que o leiaute não suporta

Um campo preenchido no modelo que o leiaute carregado não conhece (de uma versão mais nova do modelo, por exemplo) fica fora do XML e vira um problema de severidade aviso. Ele nunca some calado.

## Prompt para agentes de IA

Peça a um agente que converta os seus dados numa NotaFiscal. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva um método que converte o meu pedido de venda em NotaFiscal da OpenDFe (namespace OpenDFe.NFe), seguindo docs/guia/nfe/montar-nota.md.
O meu modelo: <cole as classes de pedido, cliente, produto e os impostos já calculados>. Documento: <NF-e/NFC-e>. Regime: <...>.
Regras: os grupos de ICMS, PIS e COFINS são subclasses (escolha pelo CST/CSOSN que eu informar); a biblioteca não calcula tributo
nem totais, então preencha Total.Icms com as somas dos itens; valores em decimal; datas em DateTimeOffset com o fuso local;
CNPJ, CPF e CEP só com os dígitos; não preencha UF, Modelo, Ambiente, TipoEmissao, a chave nem o Suplementares (vêm do núcleo).
Confira cada propriedade em docs/referencia/opendfe-nfe-modelo.md pela tag do XML e não invente propriedades.
```

## Próximas etapas

- [Autorizar notas](autorizacao.md)
- [Validação: schema, regras e regras próprias](../conceitos/validacao.md)
- [Referência do modelo](../../referencia/opendfe-nfe-modelo.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
