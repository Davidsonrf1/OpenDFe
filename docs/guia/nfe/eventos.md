# Cancelar e corrigir: eventos

[Documentação](../../index.md) › NF-e e NFC-e › Eventos

Os eventos registram fatos sobre uma nota já autorizada: o cancelamento, a carta de correção, o cancelamento por substituição da NFC-e e a manifestação do destinatário. Todos vão pelo mesmo parâmetro, `EnvioEvento`, e voltam em `RetornoEvento`. Este artigo cobre o cancelamento e a carta de correção.

| Evento | Classe | Código | Documento |
|---|---|---|---|
| Cancelamento | `Cancelamento` | 110111 | NF-e e NFC-e |
| Carta de correção | `CartaCorrecao` | 110110 | Só NF-e |
| Cancelamento por substituição | `CancelamentoSubstituicao` | 110112 | Só NFC-e ([artigo próprio](../nfce/cancelamento-substituicao.md)) |
| Manifestação do destinatário | `CienciaOperacao`, `ConfirmacaoOperacao`, `DesconhecimentoOperacao`, `OperacaoNaoRealizada` | 2102xx | Só NF-e ([artigo próprio](manifestacao.md)) |

## Cancelar

O cancelamento vale para a NF-e e para a NFC-e, **até 24 horas depois da autorização**. Ele precisa do protocolo da autorização (`nProt`, 15 dígitos) e de uma justificativa de 15 a 255 caracteres.

```csharp
var autorizada = resultado.Retorno!.Notas[0];

var cancelamento = new Cancelamento(autorizada.Chave!, autorizada.Protocolo!.Numero!, "Cancelamento por erro de digitacao")
{
    Momento = DateTimeOffset.Now,   // guarde o evento com o Momento: o reenvio sai com os mesmos bytes
};

var envio = await dfe.ExecutarAsync(new EnvioEvento(cancelamento));
var registrado = envio.Retorno!.Eventos[0];
if (registrado.Registrado)                    // 135, 136 ou 155
    Guardar(registrado.XmlProcessado!);       // o procEventoNFe
else
    Console.WriteLine($"{registrado.Status} {registrado.Motivo}");
```

| `cStat` | Significado |
|---|---|
| 135 | Evento registrado e vinculado à nota |
| 501 | Fora do prazo de 24 horas (o cancelamento extemporâneo, quando a UF o aceita, volta como 155) |
| 580 | A nota já está cancelada |
| 573 | O mesmo evento já foi registrado (o reenvio de um evento que chegou) |
| 222 | O protocolo não é o da autorização |
| 221 | Na NF-e, o destinatário já confirmou a operação |

O cancelamento vai ao autorizador da chave: a nota autorizada em SVC é cancelada na SVC. Com o `Destino` do parâmetro, ele vai ao autorizador normal, que também conhece a nota.

> **Dica**
> Na NFC-e fora das 24 horas, use o [cancelamento por substituição](../nfce/cancelamento-substituicao.md), que vale até 168 horas.

## Corrigir com a carta de correção

A carta de correção (CC-e) corrige um dado da NF-e que não muda o imposto nem as partes. Ela **não existe para a NFC-e** (o construtor recusa a chave de NFC-e). O texto da correção tem de 15 a 1000 caracteres; a biblioteca acrescenta sozinha a condição de uso, que é o texto fixo do schema.

```csharp
var carta = new CartaCorrecao(nota.Chave!, sequencia: 1, "Correcao do endereco de entrega: Rua B, 2");
var envio = await dfe.ExecutarAsync(new EnvioEvento(carta));
```

- **A sequência é sua.** A primeira carta é 1, a seguinte 2, até 20. A biblioteca não sabe qual é a próxima: conte as cartas que a [consulta da nota](consulta-protocolo.md) traz. A mesma sequência de novo é 573.
- **A carta mais recente substitui as anteriores.** Cada carta nova deve repetir o que continua valendo das anteriores.
- **O que a carta não pode corrigir:** as variáveis que determinam o valor do imposto (base de cálculo, alíquota, diferença de preço, quantidade, valor da operação); os dados cadastrais que mudam o remetente ou o destinatário; a data de emissão ou de saída. A SEFAZ não confere o conteúdo: o erro é de quem emite.
- Não cabe em nota cancelada (580). A carta vai sempre ao autorizador normal da UF: a SVC registra só o cancelamento.

## Enviar vários eventos

Um `EnvioEvento` leva de 1 a 20 eventos, cada um assinado, e o retorno traz um `ResultadoEvento` por evento, na ordem.

```csharp
var envio = await dfe.ExecutarAsync(new EnvioEvento(carta, cancelamento));
foreach (var evento in envio.Retorno!.Eventos)
    Console.WriteLine($"{evento.Chave} {evento.Status} {evento.Motivo}");
```

Os eventos de um envio vão ao mesmo autorizador: não se misturam, por exemplo, uma manifestação (que vai ao Ambiente Nacional) e um cancelamento (que vai à UF). O mesmo evento duas vezes no envio também é recusado (`NFE-LOTE`).

## O resultado

| Membro de `ResultadoEvento` | Descrição |
|---|---|
| `Registrado` | O evento foi registrado (135, 136 ou 155) |
| `Status`, `Motivo` | O `cStat` e o `xMotivo` do evento |
| `Protocolo`, `Registro` | O protocolo e o momento do registro |
| `Descricao` | O `xEvento` ("Cancelamento homologado", "Carta de Correção registrada") |
| `XmlEvento` | O evento assinado, como foi enviado |
| `XmlProcessado` | O `procEventoNFe`: o evento com o registro da SEFAZ. É o que se guarda e se distribui |
| `Evento`, `Chave` | O evento do parâmetro e a chave |

## O que guardar

- O `XmlProcessado` de cada evento registrado: ele prova o evento.
- O evento como você o montou, com o `Momento`, para o reenvio depois de uma queda (os mesmos bytes e o mesmo `Id`). Veja [Quando a conexão cai](queda-recuperacao.md).

## Prompt para agentes de IA

Peça a um agente que escreva o cancelamento e a carta de correção. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva o cancelamento e a carta de correção com a OpenDFe, seguindo docs/guia/nfe/eventos.md:
Cancelamento(chave, protocolo de 15 dígitos, justificativa de 15 a 255) até 24 horas da autorização; CartaCorrecao(chave, sequencia,
texto de 15 a 1000) só na NF-e, com a sequência calculada pelas cartas que a ConsultaProtocolo traz. Informe o Momento e guarde o
evento para o reenvio. Trate Registrado (135, 136, 155), 501, 573, 580 e 221. Guarde o XmlProcessado (procEventoNFe).
Na queda, consulte a chave antes de reenviar. Os dados vêm de <descreva>.
```

## Próximas etapas

- [Inutilizar numeração](inutilizacao.md)
- [Cancelamento por substituição da NFC-e](../nfce/cancelamento-substituicao.md)
- [Manifestação do destinatário](manifestacao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
