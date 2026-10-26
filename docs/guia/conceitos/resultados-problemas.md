# Resultados, problemas e exceções

[Documentação](../../index.md) › Conceitos › Resultados, problemas e exceções

Toda operação termina de um de quatro jeitos. Este artigo mostra como cada um aparece no código e como tratá-los.

| Desfecho | Como aparece | Exemplo |
|---|---|---|
| A SEFAZ aceitou | `ResultadoWS.Processado`, com o `cStat` de sucesso | 100: autorizada |
| A SEFAZ rejeitou | `ResultadoWS.Processado`, com o `cStat` da rejeição | 539: duplicidade |
| A biblioteca recusou antes de transmitir | `ResultadoWS.Situacao == RecusadoLocalmente`, com os `Problemas` | 564: o total não fecha |
| A transmissão falhou | `FalhaTransmissaoException` | Tempo esgotado |

A regra que guia o desenho: **problema não é exceção**. A rejeição e a validação voltam no resultado; exceção é só para falha de infraestrutura e erro de programação.

## O ResultadoWS

```csharp
var resultado = await dfe.ExecutarAsync(new Autorizacao(nota));

if (resultado.Processado)
{
    // A SEFAZ respondeu. Retorno e Motivo não são nulos.
    Console.WriteLine($"{resultado.CStat} {resultado.Motivo}");   // o cStat do lote: 104
    var nota0 = resultado.Retorno.Notas[0];
    Console.WriteLine($"{nota0.Protocolo?.Status} {nota0.Protocolo?.Motivo}");   // o da nota: 100
}
else
{
    // RecusadoLocalmente: nada foi enviado.
    foreach (var problema in resultado.Problemas)
        Console.WriteLine(problema);
}
```

| Membro | Descrição |
|---|---|
| `Situacao` | `Processado` ou `RecusadoLocalmente` |
| `Processado` | `Situacao == Processado`. Com ele verdadeiro, o compilador sabe que `Retorno` e `Motivo` não são nulos |
| `CStat`, `Motivo` | O `cStat` e o `xMotivo` da mensagem. Na autorização, os do lote; o de cada nota está no protocolo dela |
| `Retorno` | O retorno tipado da operação (`RetornoAutorizacao`, `RetornoEvento`...) |
| `Problemas` | Todos os problemas: os da montagem, do schema, das regras, dos callbacks e da leitura. No `Processado`, só avisos |
| `Rastro` | O que foi enviado e recebido |

> **Importante**
> Na autorização, o `cStat` do resultado é o do **lote** (104: lote processado; 103: lote recebido). A situação de cada nota está em `Retorno.Notas[i].Protocolo.Status`, e o atalho `ResultadoNota.Autorizada` diz se é 100 ou 150. Um lote processado pode ter notas rejeitadas.

## O Problema

Cada problema tem um código, uma severidade e uma mensagem em português, e aponta o campo:

| Membro | Descrição | Exemplo |
|---|---|---|
| `Codigo` | O código da rejeição que a regra antecipa, ou um código da biblioteca | `564`, `225`, `NFE-QRCODE` |
| `Severidade` | `Erro` impede a transmissão; `Aviso`, não | `Erro` |
| `Mensagem` | O texto, em português | `Total do Produto / Serviço difere do somatório dos itens (vProd 49,00; soma dos itens 50,00)` |
| `Tag` | A tag do XML | `vProd` |
| `Caminho` | O caminho no XML, com o índice a partir de 1 no elemento que se repete | `NFe/infNFe/total/ICMSTot/vProd` |
| `Propriedade` | A propriedade do modelo | `Total.Icms.ValorProdutos` |
| `Schema` | Nos erros de schema, o detalhe estruturado (`FalhaSchema`) | |

`ToString()` monta `Severidade Codigo: Mensagem (Propriedade)`:

```
Erro 564: Total do Produto / Serviço difere do somatório dos itens (vProd 49,00; soma dos itens 50,00) (Total.Icms.ValorProdutos)
```

Na autorização de várias notas, a propriedade leva o índice da nota no parâmetro: `Notas[1].Itens[2].Produto.Valor`.

### Os erros de schema

O erro de schema traz, em `Problema.Schema`, o que falhou, para você mostrar do seu jeito:

| Membro de `FalhaSchema` | Descrição |
|---|---|
| `Tipo` | O que falhou: `Valor`, `ElementoAusente`, `ElementoInesperado`, `AtributoAusente`, `AtributoInesperado`, `TextoInesperado`, `Outra` |
| `TipoSchema` | O tipo do schema do campo (`TDec_1302`) |
| `Documentacao`, `DocumentacaoTipo` | A documentação do campo e do tipo no XSD |
| `Restricoes` | As facetas violadas, com o valor de cada uma (tamanho, padrão, valores aceitos, casas decimais) |
| `Esperados` | Os elementos que o schema esperava naquele ponto |
| `Valor` | O valor encontrado |
| `Linha`, `Coluna` | A posição no XML |

> **Observação**
> O valor encontrado pode ser dado pessoal (um CPF, um e-mail). Ele fica só em `FalhaSchema.Valor`, nunca na mensagem: a mensagem pode ir para um log.

## O rastro

`ResultadoWS.Rastro` (e `FalhaTransmissaoException.Rastro`) guarda o que foi enviado e recebido:

| Membro | Descrição |
|---|---|
| `XmlPedido` | A mensagem como foi enviada, assinada (o conteúdo do `nfeDadosMsg`) |
| `XmlRetorno` | A resposta (o conteúdo do `nfeResultMsg`); nulo sem resposta |
| `Autorizador`, `Url` | Para onde foi |
| `Inicio`, `Duracao` | Pelo relógio da configuração |
| `DuracaoTransmissao` | Só a ida e volta HTTPS; nulo se não chegou a transmitir |

> **Importante**
> Os XMLs do rastro têm dados pessoais e fiscais. Guarde-os com o mesmo cuidado do documento e nunca os escreva em log.

## A FalhaTransmissaoException

Lançada quando o pedido foi montado e validado, mas a transmissão falhou:

```csharp
try
{
    var resultado = await dfe.ExecutarAsync(new Autorizacao(nota));
}
catch (FalhaTransmissaoException ex) when (!ex.PodeTerSidoProcessado)
{
    // Não chegou à SEFAZ (DNS, conexão recusada, TLS): corrija e reenvie o mesmo XML.
}
catch (FalhaTransmissaoException ex)
{
    // Pode ter chegado: não reenvie às cegas. Recupere (veja "Quando a conexão cai").
    var xml = Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido!)[0];
}
```

| Membro | Descrição |
|---|---|
| `Tipo` | A espécie da falha (`TipoFalhaTransmissao`): `Conexao`, `Tls`, `CertificadoRecusado`, `TempoEsgotado`, `Http`, `FalhaSoap`, `RespostaIlegivel` |
| `PodeTerSidoProcessado` | O pedido saiu e a resposta não veio (ou não se leu): a SEFAZ pode ter processado |
| `Rastro` | O que foi enviado (e o que chegou, se chegou) |
| `InnerException` | A causa original |

## As outras exceções

| Exceção | Quando |
|---|---|
| `ArgumentException` (e derivadas) | Parâmetro inválido: uma chave nula, um protocolo que não tem 15 dígitos, mais de 50 notas num lote |
| `NotSupportedException` | O leiaute não registrou a operação para o documento, ou não há URL para o autorizador, o ambiente e o serviço |
| `OperationCanceledException` | O `CancellationToken` foi cancelado |
| `CryptographicException` | O certificado sem chave privada RSA, no `DFe.Criar` |

## Os códigos da biblioteca

Além dos códigos de rejeição do MOC, a biblioteca usa códigos próprios, com o prefixo `NFE-`:

| Código | Quando |
|---|---|
| `NFE-XML`, `NFE-RAIZ`, `NFE-VERSAO` | O XML não é bem formado, não é uma NF-e, ou é de outro leiaute |
| `NFE-CHAVE` | A chave ausente ou inválida; a chave de outro modelo que o do núcleo |
| `NFE-PROTOCOLO` | O protocolo é de outra nota (ou, na recuperação, de outro XML com a mesma chave) |
| `NFE-NUCLEO` | A nota contradiz o núcleo (um campo que o núcleo já sabe, preenchido diferente) |
| `NFE-LOTE` | A mesma nota duas vezes no lote; tipos de emissão ou destinos diferentes no mesmo envio |
| `NFE-QRCODE` | A NFC-e de uma UF sem URL de consulta na tabela e sem a troca na configuração |
| `NFE-CADASTRO` | A consulta cadastro de uma UF que não tem o serviço |
| `NFE-VALOR`, `NFE-DESCONHECIDO`, `NFE-REPETIDO` | Na leitura: o valor que não converte, o campo desconhecido, o repetido |

O catálogo completo das regras, com as que a biblioteca confere e as que ficam para a SEFAZ, está em [REGRAS.md](../../REGRAS.md).

## Próximas etapas

- [Validação: schema, regras e regras próprias](validacao.md)
- [Quando a conexão cai: recuperação e duplicidade](../nfe/queda-recuperacao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
