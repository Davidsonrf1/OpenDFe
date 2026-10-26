# Quando a conexão cai: recuperação e duplicidade

[Documentação](../../index.md) › NF-e e NFC-e › Quando a conexão cai

Este artigo cobre os casos em que a autorização não termina de um jeito claro: a conexão caiu, a resposta não veio, a SEFAZ respondeu "duplicidade". A biblioteca não reenvia nem consulta nada sozinha: o próximo passo é seu, e este guia diz qual é. Vale para a NF-e e para a NFC-e.

> **Importante**
> **A nota é o XML assinado, não o objeto `NotaFiscal`.** Depois que uma nota saiu para a SEFAZ, mesmo sem resposta, guarde e reenvie sempre aquele XML. Não gere a nota de novo: o código numérico (`cNF`) é sorteado a cada escrita, e a nota gerada de novo teria outra chave para a mesma numeração.

## Antes de enviar: guarde o XML

O caminho mais seguro é preparar a nota, gravar o XML e só depois transmitir:

```csharp
var preparado = dfe.Preparar(nota);
if (!preparado.Preparado)
    return Mostrar(preparado.Problemas);
Guardar(preparado.Chave!, preparado.Xml!);                 // no banco, antes de transmitir

var resultado = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), ct);
```

Se você transmite a nota do modelo direto (`new Autorizacao(nota)`), o XML assinado fica no rastro, e `Autorizacao.NotasEnviadas` o tira de lá com os mesmos bytes:

```csharp
var xmls = Autorizacao.NotasEnviadas(resultado.Rastro.XmlPedido!);   // ou falha.Rastro.XmlPedido
```

## A falha que não chegou à SEFAZ

`FalhaTransmissaoException` com `PodeTerSidoProcessado == false`: DNS, conexão recusada, TLS, o certificado recusado pelo servidor. O pedido não saiu inteiro, e a SEFAZ não viu a nota.

**O que fazer:** corrija a causa (rede, certificado) e reenvie o mesmo XML. Não há risco de duplicidade.

## A falha que pode ter chegado

`FalhaTransmissaoException` com `PodeTerSidoProcessado == true`: tempo esgotado, conexão caída depois do envio, HTTP 5xx, resposta ilegível. A SEFAZ pode ter autorizado a nota sem que a resposta chegasse.

**O que fazer:** recupere, não reenvie às cegas.

```csharp
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    var xml = Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido!)[0];
    Guardar(xml);                                          // antes de qualquer outra coisa

    var r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), ct);
    var nota = r.Retorno!.Notas[0];
    switch (r.Retorno.Status)
    {
        case 100 or 150:                                   // autorizada: o nfeProc está pronto
            Guardar(nota.XmlProcessado!);
            break;
        case 217:                                          // não consta: a nota não chegou
            await dfe.ExecutarAsync(new Autorizacao(xml), ct);   // o MESMO xml
            break;
        default:                                           // 101 (cancelada) e outros: consulte a situação
            break;
    }
}
```

A `RecuperacaoAutorizacao` consulta a chave do XML (`NFeConsultaProtocolo4`) e só monta o `nfeProc` se o protocolo é daquele XML: o `digVal` do protocolo tem de ser o `DigestValue` da nota. Se não for, o `Protocolo` vem, o `XmlProcessado` fica nulo e sai o aviso `NFE-PROTOCOLO`: a SEFAZ tem outro XML com a mesma chave.

- **Se a recuperação também cair:** espere e tente de novo, com o mesmo XML. Enquanto não houver resposta, a nota está "em dúvida", e a numeração dela não pode ser usada por outra nota.
- **No lote (2 a 50 notas):** a recuperação é por nota. Tire as notas da mensagem com `NotasEnviadas` e recupere cada uma.

## A rejeição por duplicidade

A resposta chegou, mas a nota veio rejeitada por duplicidade. `ResultadoNota.Duplicidade` diz qual nota já está na base.

**204 (`Duplicidade.MesmaChave == true`):** a mesma nota já foi autorizada, quase sempre por um envio anterior que caiu no meio. Recupere o `nfeProc`:

```csharp
if (nota.Duplicidade is { MesmaChave: true })
    r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(nota.Xml!), ct);
```

**539 (`MesmaChave == false`):** outra nota, com outra chave, já usou a mesma numeração (modelo, série e número do emitente). Em geral é a nota gerada de novo depois de uma queda, com outro `cNF`. Consulte a outra chave:

```csharp
if (nota.Duplicidade is { MesmaChave: false, Chave: { } outra })
{
    var situacao = await dfe.ExecutarAsync(new ConsultaProtocolo(outra), ct);
}
```

Se a outra está autorizada, ela é a nota que vale. Procure o XML dela no seu banco (é a nota que você enviou antes) e recupere o `nfeProc` com ele. Se você não tem esse XML, a numeração está em uso por uma nota que o seu sistema perdeu, e o caso é de suporte, não de reenvio. Quando o motivo não traz a chave (`Duplicidade.Chave` nulo), é a mesma situação, sem a chave para consultar.

`Duplicidade.Recibo` traz o recibo do lote em que a nota registrada foi autorizada, quando a SEFAZ o informa.

## O lote sem a resposta do recibo

- **A resposta do envio (103) caiu:** você não tem o recibo. Recupere nota a nota.
- **A resposta da consulta do recibo caiu:** consulte o recibo de novo. A consulta não muda nada na SEFAZ e pode ser repetida.

O 105 (lote em processamento) não é falha: espere o `TempoMedio` e consulte de novo.

## A contingência com uma nota em dúvida

Quando a SEFAZ da UF cai e o emitente passa para a SVC, a nota que ficou em dúvida no autorizador normal ainda pode estar autorizada lá. Emitir a mesma numeração em SVC gera outra chave (o `tpEmis` faz parte dela), e as duas colidem: a segunda é rejeitada com 539.

**O que fazer:** resolva a nota em dúvida antes de reusar a numeração dela. Enquanto o autorizador normal está fora, emita as notas novas em SVC com números novos e deixe a numeração em dúvida reservada. Quando ele voltar, recupere essa nota. Se ela não consta (217), transmita o mesmo XML, ou [inutilize](inutilizacao.md) o número se a venda não vai mais acontecer.

## Cancelamento e inutilização sem a resposta

Os eventos e a inutilização não têm recuperação própria: a consulta da chave diz o que a SEFAZ registrou, e o reenvio é seguro porque a SEFAZ recusa o repetido.

```csharp
try
{
    await dfe.ExecutarAsync(new EnvioEvento(cancelamento), ct);
}
catch (FalhaTransmissaoException falha) when (falha.PodeTerSidoProcessado)
{
    var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(cancelamento.Chave), ct);
    if (consulta.Retorno!.Cancelada)
    {
        // 101: já cancelada. O procEventoNFe está em consulta.Retorno.Eventos: guarde-o.
    }
    else if (consulta.Retorno.Autorizada)
    {
        // 100: o evento não chegou. Reenvie o MESMO evento (o mesmo Momento); 573 diz que já foi registrado.
        await dfe.ExecutarAsync(new EnvioEvento(cancelamento), ct);
    }
}
```

- Informe o `Momento` do evento e **guarde o evento**: o reenvio sai com os mesmos bytes e o mesmo `Id`. Sem o `Momento`, o `dhEvento` vem do relógio e muda a cada tentativa.
- **Inutilização sem a resposta:** reenvie a mesma faixa. Se a primeira chegou, a SEFAZ responde 563 (a faixa já foi inutilizada), com o protocolo da primeira; se não chegou, a segunda é homologada (102).

## Resumo

| Situação | O que fazer |
|---|---|
| `FalhaTransmissaoException`, `PodeTerSidoProcessado == false` | Reenviar o mesmo XML |
| `FalhaTransmissaoException`, `PodeTerSidoProcessado == true` | `RecuperacaoAutorizacao(xml)` |
| Recuperação com 100 ou 150 | Guardar o `nfeProc` (`XmlProcessado`) |
| Recuperação com 217 | Reenviar o mesmo XML |
| Recuperação com o aviso `NFE-PROTOCOLO` | A base tem outro XML com essa chave: procurar o XML que foi autorizado |
| A recuperação também caiu | Esperar e tentar de novo; não usar a numeração |
| 204 | `RecuperacaoAutorizacao(nota.Xml)` |
| 539 | `ConsultaProtocolo(nota.Duplicidade.Chave)` e procurar o XML da outra nota |
| Lote: o 103 caiu | Recuperar nota a nota |
| Lote: a consulta do recibo caiu | Consultar o recibo de novo |
| Contingência com nota em dúvida | Não reusar a numeração dela até recuperar |
| Cancelamento sem a resposta | `ConsultaProtocolo(chave)`: 101, já cancelada; 100, reenviar o mesmo evento |
| Inutilização sem a resposta | Reenviar a mesma faixa (563 diz que já foi) |

> **Dica**
> Teste cada linha desta tabela na [SEFAZ emulada](../sefaz-emulada/falhas-comandadas.md): a falha `conexaoDerrubada` com `depois` é a queda no meio, e o reenvio do mesmo XML depois dela dá o 204.

## Próximas etapas

- [Simular falhas na SEFAZ emulada](../sefaz-emulada/falhas-comandadas.md)
- [Cancelar e corrigir: eventos](eventos.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
