# Simular falhas na SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Simular falhas

A SEFAZ real não deixa você escolher quando ela cai. A emulada deixa: você comanda uma falha e ela atinge os próximos pedidos que combinam com ela. Assim você testa o tratamento de cada caso, inclusive o mais perigoso, a queda no meio: a SEFAZ autoriza a nota e a resposta não chega.

Há duas formas de comandar uma falha:

| Forma | Quando usar |
|---|---|
| **A API de controle** (`POST /_controle/falhas`, ou a aba **Falhas** do painel) | Nos testes automatizados e quando a falha deve atingir vários pedidos ou um serviço que não é a autorização |
| **A marca no `infCpl` da nota** (`EMULADA:<tipo>`) | A partir do próprio aplicativo (o PDV, o ERP), sem acesso ao controle: a falha atinge só o pedido de autorização que leva a nota |

## Os tipos de falha

| Tipo | Efeito | O que a OpenDFe devolve |
|---|---|---|
| `paralisacao` | O `cStat` 108 (padrão) ou 109 no lugar da resposta normal, depois das rejeições do próprio pedido | `Processado`, com o `CStat` 108 ou 109 (no status, `RetornoStatus.Operando` falso) |
| `atraso` | Espera os milissegundos e responde normalmente | O resultado normal; com o atraso maior que o `TempoResposta`, `FalhaTransmissaoException` do tipo `TempoEsgotado` |
| `tempoEsgotado` | Nunca responde: segura o pedido até o cliente desistir | `FalhaTransmissaoException` do tipo `TempoEsgotado`, com `PodeTerSidoProcessado` verdadeiro |
| `conexaoDerrubada` | Derruba a conexão sem resposta, **antes** de processar: a nota não é autorizada | `FalhaTransmissaoException` com `PodeTerSidoProcessado` verdadeiro (o pedido saiu) |
| `conexaoDerrubada` com `depois` | Processa o pedido (autoriza a nota) e derruba a conexão sem mandar a resposta: a queda no meio | `FalhaTransmissaoException` com `PodeTerSidoProcessado` verdadeiro; a nota está autorizada no emulador |
| `respostaIlegivel` | HTTP 200 com o envelope cortado ao meio | `FalhaTransmissaoException` do tipo `RespostaIlegivel` |
| `falhaSoap` | HTTP 500 com a falha `soap:Receiver` | `FalhaTransmissaoException` do tipo `FalhaSoap` |
| `loteProcessando` | A consulta do recibo responde 105, como se o lote ainda estivesse na fila (só no `NFeRetAutorizacao4`) | `Processado`, com o `Status` 105 no `RetornoAutorizacao` |

## Comandar pela API de controle

A falha é um JSON enviado ao painel:

```http
POST http://127.0.0.1:5444/_controle/falhas
Content-Type: application/json

{ "tipo": "paralisacao", "destino": "MG", "servico": "NFeStatusServico4", "vezes": 1, "codigo": "109", "observacao": "Manutenção" }
```

| Campo | Descrição |
|---|---|
| `tipo` | Obrigatório. Um dos tipos da tabela acima. |
| `destino`, `servico` | A que pedidos a falha vale: o autorizador (`MG`, `SVRS`, `SVC-AN`, `AN`...) e o serviço (`NFeAutorizacao4`...). Nulo vale para todos. |
| `vezes` | Quantos pedidos a falha atinge (1 ou mais). Nulo: até ser removida. |
| `milissegundos` | Só no `atraso` (maior que zero). |
| `depois` | Só na `conexaoDerrubada`: `true` derruba depois de processar. |
| `codigo`, `observacao` | Só na `paralisacao`: o código (`108` ou `109`) e a observação, que vai no `xObs` do status. |

A resposta é `201 Created` com a falha e o `Location` dela, ou `400` com `{ "erro": "..." }`. `GET /_controle/falhas` lista as ativas; `DELETE /_controle/falhas/{id}` remove uma; `DELETE /_controle/falhas` remove todas.

Cada pedido consome a **primeira** falha de transporte que vale para ele (atraso, tempo esgotado, conexão derrubada, resposta ilegível, falha SOAP), a primeira paralisação e, na consulta do recibo, o primeiro lote em processamento, na ordem em que foram comandadas. Elas podem valer juntas: um atraso seguido do 109, por exemplo. O `vezes` é descontado a cada pedido atingido, e a falha some quando chega a zero.

## Comandar pela marca no infCpl

Escreva `EMULADA:<tipo>[:<valor>]` nas informações complementares (`infCpl`) de uma nota do lote. A marca vale só para o pedido de autorização que a traz e passa na frente da falha comandada pelo controle.

```csharp
nota.InformacoesAdicionais = new InformacoesAdicionais
{
    Complementares = "Pedido 123 EMULADA:conexaoDerrubada:depois",
};
```

| Marca | Efeito |
|---|---|
| `EMULADA:atraso:<milissegundos>` | O atraso. |
| `EMULADA:tempoEsgotado` | O tempo esgotado. |
| `EMULADA:conexaoDerrubada` | Derruba antes de processar: a nota não é autorizada. |
| `EMULADA:conexaoDerrubada:depois` | Autoriza e derruba: a queda no meio. |
| `EMULADA:respostaIlegivel` | A resposta ilegível. |
| `EMULADA:falhaSoap` | A falha SOAP. |
| `EMULADA:paralisacao`, `EMULADA:paralisacao:109` | A paralisação (108 se o código for omitido). |
| `EMULADA:loteProcessando[:<n>]` | No lote assíncrono, as `n` primeiras consultas do recibo respondem 105 (1 se omitido). |
| `EMULADA:irregular:emitente` | Só essa nota: o emitente irregular (781). |
| `EMULADA:irregular:destinatario` | Só essa nota (NF-e): o destinatário irregular (302). |

O resto do texto fica como está. Vale uma marca por espécie no lote (transporte, paralisação, lote em processamento); a do irregular é de cada nota. Uma marca que não serve (tipo desconhecido, valor que o tipo não aceita, duas da mesma espécie) responde HTTP 400 com a falha `soap:Sender` e a explicação, sem processar o lote: o erro de digitação não passa calado.

> **Importante**
> A marca fica no XML da nota. Use-a só em homologação contra o emulador e nunca deixe o texto em notas reais.

## Cenários

### Serviço paralisado e contingência SVC

Em homologação, a SVC fica sempre ativa: com o autorizador da UF paralisado, a SVC-AN (ou a SVC-RS, conforme a UF) responde 107 e autoriza as notas com o tipo de emissão dela.

```csharp
// MG paralisado em todos os serviços, até ser liberado.
await painel.PostAsJsonAsync("_controle/falhas", new { tipo = "paralisacao", destino = "MG", codigo = "108" });

var status = await dfe.ExecutarAsync(new ConsultaStatus());
// status.CStat == 108; status.Retorno.Operando == false

// O status da SVC-AN, com o mesmo núcleo.
var svc = await dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.SvcAN });
// svc.CStat == 107
```

O passo a passo da contingência está em [Autorizadores, URLs e contingência SVC](../conceitos/autorizadores-urls.md).

### A queda no meio

```csharp
await painel.PostAsJsonAsync("_controle/falhas",
    new { tipo = "conexaoDerrubada", destino = "MG", servico = "NFeAutorizacao4", vezes = 1, depois = true });

try
{
    await dfe.ExecutarAsync(new Autorizacao(nota));
}
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    var xml = Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido!)[0];      // guarde antes de tudo
    var recuperada = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml));
    // recuperada.Retorno.Status == 100: a nota foi autorizada; Notas[0].XmlProcessado é o nfeProc

    var reenvio = await dfe.ExecutarAsync(new Autorizacao(xml));
    // reenvio.Retorno.Notas[0].Protocolo.Status == 204; Duplicidade.MesmaChave == true
}
```

O guia [Quando a conexão cai](../nfe/queda-recuperacao.md) explica cada caso.

### O lote em processamento

```csharp
await painel.PostAsJsonAsync("_controle/falhas", new { tipo = "loteProcessando", vezes = 2 });

var lote = await dfe.ExecutarAsync(new Autorizacao(nota1, nota2));         // 103, com o recibo
var xmls = lote.Retorno!.Notas.Select(n => n.Xml!).ToList();
var consulta = await dfe.ExecutarAsync(new ConsultaRecibo(lote.Retorno.Recibo!) { Xmls = xmls });
// consulta.Retorno.Status == 105 nas duas primeiras consultas; 104 na terceira
```

### O tempo esgotado

Reduza o `TempoResposta` do `ConexoesSefaz` no teste para não esperar o padrão de 60 segundos:

```csharp
using var conexoes = new ConexoesSefaz(fabrica) { TempoResposta = TimeSpan.FromSeconds(3) };
await painel.PostAsJsonAsync("_controle/falhas", new { tipo = "tempoEsgotado", servico = "NFeAutorizacao4", vezes = 1 });
// ExecutarAsync lança FalhaTransmissaoException { Tipo: TempoEsgotado, PodeTerSidoProcessado: true }
```

## Prompt para agentes de IA

Peça a um agente que escreva os testes de falha. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva testes xUnit que provocam falhas na SEFAZ emulada da OpenDFe e conferem o tratamento do meu código <descreva a classe de emissão>,
seguindo docs/guia/sefaz-emulada/falhas-comandadas.md. Um teste por caso: paralisação 108/109 (e a SVC respondendo 107), tempoEsgotado
(com TempoResposta curto), conexaoDerrubada antes (não autoriza) e depois (autoriza: recuperar e o reenvio dá 204), respostaIlegivel,
falhaSoap e loteProcessando (105 e depois 104). Comande pela API (POST /_controle/falhas, com vezes = 1) e zere com DELETE /_controle
no começo de cada teste. Rode numa coleção sem paralelismo.
```

## Próximas etapas

- [Preparar dados de teste](dados-teste.md)
- [Quando a conexão cai: recuperação e duplicidade](../nfe/queda-recuperacao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
