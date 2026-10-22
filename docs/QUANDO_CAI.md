# O que fazer quando cai

Este guia é para quem emite NF-e com o OpenDFe. Ele cobre o caso em que a autorização não termina de um jeito claro: a conexão caiu, a resposta não veio, a SEFAZ respondeu "duplicidade". A lib não reenvia nem consulta nada sozinha (`PROJETO.md` 8.6). Quem usa a lib decide o próximo passo, e este guia diz qual é.

Há uma regra que vale para todos os casos: **a nota é o XML assinado, não o objeto `NotaFiscal`.** Depois que uma nota saiu para a SEFAZ, mesmo sem resposta, guarde e reenvie sempre aquele XML. Não gere a nota de novo (veja "Por que não gerar a nota de novo", no fim).

## Antes de enviar: guarde o XML

O caminho mais seguro é preparar a nota, gravar o XML e só depois transmitir:

```csharp
var preparado = dfe.Preparar(nota);
if (!preparado.Preparado)
    return mostrar(preparado.Problemas);
guardar(preparado.Chave, preparado.Xml);                 // no banco, antes de transmitir

var r = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), ct);
```

Se você transmite a nota do modelo direto (`new Autorizacao(nota)`), o XML assinado fica no rastro, e `Autorizacao.NotasEnviadas` o tira de lá com os mesmos bytes:

```csharp
var xmls = Autorizacao.NotasEnviadas(resultado.Rastro.XmlPedido!);   // ou falha.Rastro.XmlPedido
```

## 1. A falha que não chegou à SEFAZ

`FalhaTransmissaoException` com `PodeTerSidoProcessado == false`: DNS, conexão recusada, TLS, o certificado recusado pelo servidor. O pedido não saiu inteiro, e a SEFAZ não viu a nota.

**O que fazer:** corrigir a causa (rede, certificado) e reenviar o mesmo XML. Sem risco de duplicidade.

## 2. A falha que pode ter chegado

`FalhaTransmissaoException` com `PodeTerSidoProcessado == true`: tempo esgotado, conexão caída depois do envio, HTTP 5xx, resposta ilegível. A SEFAZ pode ter autorizado a nota sem que a resposta chegasse.

**O que fazer:** recuperar, não reenviar às cegas.

```csharp
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    var xml = Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido!)[0];
    guardar(xml);                                         // antes de qualquer outra coisa

    var r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), ct);
    var nota = r.Retorno!.Notas[0];
    switch (r.Retorno.Status)
    {
        case 100 or 150:                                  // autorizada: o nfeProc está pronto
            guardar(nota.XmlProcessado);
            break;
        case 110 or 301 or 302:                           // denegada: guarde o nfeProc também
            guardar(nota.XmlProcessado);
            break;
        case 217:                                         // não consta: a nota não chegou
            await dfe.ExecutarAsync(new Autorizacao(xml), ct);   // o MESMO xml
            break;
        default:                                          // 101 (cancelada) e outros: ver a seção 6
            break;
    }
}
```

A recuperação consulta a chave (`NFeConsultaProtocolo4`) e só monta o `nfeProc` se o protocolo é daquele XML: o `digVal` do protocolo tem de ser o `DigestValue` da nota. Se não for, o `Protocolo` vem, o `XmlProcessado` fica nulo e sai o aviso `NFE-PROTOCOLO`: a SEFAZ tem outro XML com a mesma chave (veja a seção 7).

**Se a recuperação também cair:** espere e tente de novo, com o mesmo XML. Enquanto não houver resposta, a nota está "em dúvida", e a numeração dela não pode ser usada por outra nota.

**O lote (2 a 50 notas):** a recuperação é por nota. Tire as notas da mensagem com `NotasEnviadas` e recupere cada uma.

## 3. A rejeição por duplicidade

A resposta chegou, mas a nota veio rejeitada por duplicidade. O `ResultadoNota.Duplicidade` diz qual nota já está na base.

- **204 (`Duplicidade.MesmaChave == true`)**: a mesma nota já foi autorizada, quase sempre por um envio anterior que caiu no meio. Recupere o `nfeProc`:

  ```csharp
  if (nota.Duplicidade is { MesmaChave: true })
      r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(nota.Xml!), ct);
  ```

- **539 (`MesmaChave == false`)**: outra nota, com outra chave, já usou a mesma numeração (modelo, série e número do emitente). Em geral é a nota gerada de novo depois de uma queda, com outro `cNF`. Consulte a outra chave para ver a situação dela:

  ```csharp
  if (nota.Duplicidade is { MesmaChave: false, Chave: { } outra })
  {
      var situacao = await dfe.ExecutarAsync(new ConsultaProtocolo(outra), ct);
  }
  ```

  Se a outra está autorizada, ela é a nota que vale. Procure o XML dela no seu banco (é a nota que você enviou antes) e recupere o `nfeProc` com ele. Se você não tem esse XML, a numeração está em uso por uma nota que o seu sistema perdeu, e o caso é de suporte, não de reenvio. Quando o motivo não traz a chave (`Duplicidade.Chave` nulo), é a mesma situação, sem a chave para consultar.

O `Duplicidade.Recibo` traz o recibo do lote em que a nota registrada foi autorizada, quando a SEFAZ o informa.

## 4. O lote sem a resposta do recibo

- **A resposta do envio (103) caiu:** você não tem o recibo. Recupere nota a nota, como na seção 2.
- **A resposta da consulta do recibo caiu:** consulte o recibo de novo. A consulta não muda nada na SEFAZ e pode ser repetida.

```csharp
var consulta = await dfe.ExecutarAsync(new ConsultaRecibo(recibo) { Xmls = xmlsDoLote }, ct);
```

O 105 (lote em processamento) não é falha: espere o `TempoMedio` e consulte de novo.

## 5. A contingência com uma nota em dúvida

Quando a SEFAZ da UF cai e o emitente passa para a SVC, a nota que ficou em dúvida no autorizador normal ainda pode estar autorizada lá. Emitir a mesma numeração em SVC gera outra chave (o `tpEmis` faz parte dela), e as duas colidem: a segunda é rejeitada com 539.

**O que fazer:** resolver a nota em dúvida antes de reusar a numeração dela. Enquanto o autorizador normal está fora, emita as notas novas em SVC com números novos e deixe a numeração em dúvida reservada. Quando ele voltar, recupere essa nota. Se ela não consta (217), transmita o mesmo XML, ou inutilize o número se a venda não vai mais acontecer.

A consulta segue a chave: a nota emitida em SVC-AN é consultada na SVC-AN. A SVC só acha as notas que ela autorizou, e o autorizador normal acha também as da SVC. Para consultar no normal, informe o destino:

```csharp
await dfe.ExecutarAsync(new ConsultaProtocolo(chave) { Destino = Destino.MG }, ct);
```

## 6. A situação de uma nota

A `ConsultaProtocolo` responde à pergunta "como está esta nota agora?":

```csharp
var r = await dfe.ExecutarAsync(new ConsultaProtocolo(chave), ct);
r.Retorno!.Autorizada;   // 100 ou 150
r.Retorno.Cancelada;     // 101, 151 ou 155
r.Retorno.Protocolo;     // o protocolo da autorização (ou da denegação)
r.Retorno.Eventos;       // cada procEventoNFe como veio (o cancelamento, a carta de correção)
```

A recuperação de uma nota já cancelada devolve `Status` 101 e, ainda assim, o `nfeProc` com o protocolo da autorização. A nota existiu, e o cancelamento está nos eventos da consulta.

No 562, a base tem a mesma numeração com outro `cNF`, e `RetornoProtocolo.ChaveRegistrada` traz a chave da nota da base quando a SEFAZ a informa.

## 7. Por que não gerar a nota de novo

A chave de acesso tem o código numérico (`cNF`). Quando a nota não informa um, a lib sorteia um a cada escrita (`CONTRATOS.md` 14, decisão 13). Gerar de novo a mesma `NotaFiscal` produz outra chave para a mesma numeração. Se a primeira tinha sido autorizada, a segunda é rejeitada com 539, e você fica com uma nota autorizada cujo XML não guardou.

Por isso:

- guarde o XML assinado antes de transmitir (o `Preparar`) ou logo depois de uma falha (o `NotasEnviadas`);
- reenvie sempre esse XML, com os mesmos bytes;
- só gere uma nota nova (outro número) depois de ter certeza, pela recuperação, de que a anterior não consta.

## Resumo

| Situação | O que fazer |
|---|---|
| `FalhaTransmissaoException`, `PodeTerSidoProcessado == false` | Reenviar o mesmo XML |
| `FalhaTransmissaoException`, `PodeTerSidoProcessado == true` | `RecuperacaoAutorizacao(xml)` |
| Recuperação com 100/150 (ou 110/301/302) | Guardar o `nfeProc` (`XmlProcessado`) |
| Recuperação com 217 | Reenviar o mesmo XML |
| Recuperação com aviso `NFE-PROTOCOLO` | A base tem outro XML com essa chave: procurar o XML que foi autorizado |
| A recuperação também caiu | Esperar e tentar de novo; não usar a numeração |
| 204 | `RecuperacaoAutorizacao(nota.Xml)` |
| 539 | `ConsultaProtocolo(nota.Duplicidade.Chave)` e procurar o XML da outra nota |
| Lote: o 103 caiu | Recuperar nota a nota |
| Lote: a consulta do recibo caiu | Consultar o recibo de novo |
| Contingência com nota em dúvida | Não reusar a numeração dela até recuperar |

A NFC-e ganha a autorização e a recuperação na etapa 5.1, com o QR Code. A consulta do protocolo já vale para ela.
