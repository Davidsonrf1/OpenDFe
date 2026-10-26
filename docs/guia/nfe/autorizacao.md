# Autorizar notas

[Documentação](../../index.md) › NF-e e NFC-e › Autorizar notas

A autorização (`NFeAutorizacao4`) envia de 1 a 50 notas à SEFAZ. Este artigo mostra a autorização síncrona e em lote, a consulta do recibo, o preparo da nota antes de transmitir e o que guardar.

## Autorizar uma nota

Uma nota vai sempre na autorização **síncrona**: a resposta já traz o protocolo.

```csharp
var resultado = await dfe.ExecutarAsync(new Autorizacao(nota));

if (!resultado.Processado)
{
    // Recusada antes de transmitir: os problemas apontam o campo.
    foreach (var problema in resultado.Problemas)
        Console.WriteLine(problema);
    return;
}

var autorizada = resultado.Retorno.Notas[0];
if (autorizada.Autorizada)
{
    Guardar(autorizada.Chave!, autorizada.XmlProcessado!);   // o nfeProc: guarde este XML
    Console.WriteLine($"Protocolo {autorizada.Protocolo!.Numero}");
}
else
{
    Console.WriteLine($"Rejeitada: {autorizada.Protocolo?.Status} {autorizada.Protocolo?.Motivo}");
}
```

## O retorno

`RetornoAutorizacao` traz os dados do lote e uma `ResultadoNota` por nota, na ordem em que você as passou:

| Membro de `RetornoAutorizacao` | Descrição |
|---|---|
| `Status`, `Motivo` | O `cStat` do lote: 104 (processado), 103 (recebido, no lote), 105 (em processamento), 106, ou a rejeição do lote |
| `Recibo` | O número do recibo (`nRec`), no lote |
| `TempoMedio` | O tempo médio de processamento (`tMed`) |
| `Notas` | Uma `ResultadoNota` por nota |
| `Ambiente`, `UF`, `Recebimento`, `VersaoAplicativo`, `Mensagem` | Os outros campos da resposta |

| Membro de `ResultadoNota` | Descrição |
|---|---|
| `Chave` | A chave de acesso da nota |
| `Xml` | O `NFe` assinado, como foi enviado: o que você guarda até ter o protocolo |
| `Protocolo` | O protocolo (`protNFe`): `Status`, `Motivo`, `Numero`, `Recebimento`, `Digest`; nulo enquanto o lote não foi processado |
| `XmlProcessado` | O `nfeProc` (a nota com o protocolo), quando autorizada (100, 150) ou denegada |
| `Autorizada` | `Protocolo.Status` é 100 ou 150 |
| `Duplicidade` | No 204 e no 539, a nota que já está na base (veja [Quando a conexão cai](queda-recuperacao.md)) |

> **Importante**
> **Guarde o `XmlProcessado`.** O `nfeProc` é o XML fiscal da nota: é ele que você distribui ao destinatário, imprime e guarda pelos prazos legais. A biblioteca não grava nada.

## Autorizar um lote

De 2 a 50 notas vão sempre no **lote assíncrono**: a resposta (103) traz o recibo, e os protocolos saem da consulta do recibo.

```csharp
var lote = await dfe.ExecutarAsync(new Autorizacao(nota1, nota2, nota3));
// lote.Retorno.Status == 103; lote.Retorno.Recibo == "311000000000006"

// Guarde os XMLs assinados do lote antes de qualquer outra coisa.
var xmls = lote.Retorno!.Notas.Select(n => n.Xml!).ToList();
Guardar(lote.Retorno.Recibo!, xmls);

// Espere o tempo médio e consulte o recibo.
await Task.Delay(lote.Retorno.TempoMedio ?? TimeSpan.FromSeconds(1));
var consulta = await dfe.ExecutarAsync(new ConsultaRecibo(lote.Retorno.Recibo!) { Xmls = xmls });

if (consulta.Retorno!.Status == 105)
{
    // Ainda em processamento: espere e consulte de novo.
}
foreach (var nota in consulta.Retorno.Notas.Where(n => n.Autorizada))
    Guardar(nota.Chave!, nota.XmlProcessado!);
```

- A quantidade decide o modo: desde a NT 2025.001, o lote de uma nota só é rejeitado (452) e a síncrona com mais de uma também (764). Não há como escolher.
- Passe os XMLs do lote em `ConsultaRecibo.Xmls`: com eles, o retorno liga cada protocolo à nota e monta o `nfeProc`. Sem eles, vêm só os protocolos.
- A biblioteca não consulta o recibo sozinha nem espera: o próximo passo é seu.
- A consulta do recibo não muda nada na SEFAZ e pode ser repetida.
- As notas de um lote são do mesmo modelo (NF-e e NFC-e não se misturam: 765) e da mesma UF.

O `idLote` sai do relógio da configuração (`AAMMDDhhmmssmmm`). Para informar o seu: `new Autorizacao(notas) { Lote = 123 }`.

## Preparar antes de transmitir

`dfe.Preparar(nota)` gera, assina e valida a nota sem transmitir. É o caminho mais seguro de emitir: você guarda o XML assinado e só depois transmite os mesmos bytes.

```csharp
var preparado = dfe.Preparar(nota);
if (!preparado.Preparado)
{
    Mostrar(preparado.Problemas);
    return;
}
Guardar(preparado.Chave!, preparado.Xml!);       // antes de transmitir

var resultado = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!));
```

Com o XML guardado antes, uma queda no meio da transmissão nunca deixa você sem a nota: veja [Quando a conexão cai](queda-recuperacao.md).

## Autorizar XMLs já assinados

O segundo construtor da `Autorizacao` recebe os XMLs assinados (o `NFe`), que vão com os mesmos bytes. É o caminho do preparo, do reenvio depois de uma falha e da NFC-e emitida em contingência offline.

```csharp
var resultado = await dfe.ExecutarAsync(new Autorizacao(xmlAssinado1, xmlAssinado2));
```

- O destino e o tipo de emissão saem da chave de cada XML, não do núcleo.
- A biblioteca confere a assinatura de cada XML recebido (297): o XML alterado depois de assinado é recusado antes de transmitir.
- Não se misturam notas do modelo e XMLs no mesmo lote.

## As regras do lote

Antes de transmitir, a biblioteca confere:

| Código | Severidade | O que confere |
|---|---|---|
| 476 | Erro | As notas do lote com a mesma UF |
| 765 | Erro | As notas com o modelo do núcleo |
| 560 | Aviso | O emitente igual ao da primeira nota |
| 297 | Erro | A assinatura de cada XML recebido |
| `NFE-LOTE` | Erro | A mesma nota duas vezes; tipos de emissão diferentes no lote |

Além delas, cada nota passa pelo schema e pelas regras do MOC (veja [Validação](../conceitos/validacao.md)).

## Códigos comuns da nota

| `cStat` | Significado | O que fazer |
|---|---|---|
| 100 | Autorizado o uso | Guardar o `nfeProc` |
| 150 | Autorizado fora de prazo | Guardar o `nfeProc` (é uma nota autorizada) |
| 204 | Duplicidade (a mesma chave) | Recuperar o `nfeProc` ([Quando a conexão cai](queda-recuperacao.md)) |
| 539 | Duplicidade (a mesma numeração, outra chave) | Consultar a outra chave ([Quando a conexão cai](queda-recuperacao.md)) |
| 2xx, 3xx, 4xx, 5xx, 7xx | Rejeição | Corrigir o dado e enviar de novo; a nota rejeitada não fica registrada |

## Prompt para agentes de IA

Peça a um agente que escreva o fluxo de emissão. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva o fluxo de emissão de <NF-e/NFC-e> com a OpenDFe, seguindo docs/guia/nfe/autorizacao.md e docs/guia/nfe/queda-recuperacao.md:
1) dfe.Preparar(nota) e, sem Preparado, mostrar os Problemas; 2) gravar a chave e o XML assinado no meu banco <descreva> antes de transmitir;
3) ExecutarAsync(new Autorizacao(preparado.Xml)); 4) com Retorno.Notas[0].Autorizada, gravar o XmlProcessado (nfeProc);
5) rejeição: mostrar Protocolo.Status e Motivo; 204 e 539 pela Duplicidade; 6) FalhaTransmissaoException com PodeTerSidoProcessado:
RecuperacaoAutorizacao com o mesmo XML. Para <lotes de até 50 notas>, use o 103 com o Recibo e ConsultaRecibo { Xmls }, tratando o 105.
Nunca gere a NotaFiscal de novo para reenviar.
```

## Próximas etapas

- [Consultar a situação de uma nota](consulta-protocolo.md)
- [Quando a conexão cai: recuperação e duplicidade](queda-recuperacao.md)
- [Gerar o PDF do DANFE](../impressao/pdf.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
