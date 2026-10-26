# Manifestação do destinatário

[Documentação](../../index.md) › NF-e e NFC-e › Manifestação do destinatário

A manifestação do destinatário é o evento com que a empresa que recebeu uma NF-e diz o que sabe dela: tomou ciência, confirma a operação, desconhece a operação ou a operação não se realizou. Além do valor fiscal, a manifestação **libera o download** da nota inteira na [distribuição DF-e](distribuicao-dfe.md): sem ela, o destinatário só recebe o resumo. Só a NF-e tem manifestação.

## Os quatro eventos

| Evento | Classe | Código | Quando usar |
|---|---|---|---|
| Ciência da emissão | `CienciaOperacao` | 210210 | Você sabe que a nota existe, mas ainda não confirma a operação. Libera o download |
| Confirmação da operação | `ConfirmacaoOperacao` | 210200 | A operação aconteceu como a nota diz. Libera o download e impede o emitente de cancelar a nota |
| Desconhecimento da operação | `DesconhecimentoOperacao` | 210220 | Você não reconhece a operação (uso indevido do seu CNPJ) |
| Operação não realizada | `OperacaoNaoRealizada` | 210240 | A operação foi combinada, mas não aconteceu (devolução, recusa). Exige justificativa. Libera o download |

A ciência é provisória; as outras três são **conclusivas** e podem ser retificadas uma vez (sequência 2).

## Manifestar

O autor é o destinatário da nota: o CNPJ (ou o CPF) dele, com ou sem máscara. O núcleo assina com o certificado dele, e a SEFAZ confere a raiz do CNPJ do certificado contra o autor.

```csharp
var chave = documento.Chave!;   // do resumo recebido na distribuição

var ciencia = await dfe.ExecutarAsync(new EnvioEvento(new CienciaOperacao(chave, "11444777000161")));
Console.WriteLine($"{ciencia.Retorno!.Eventos[0].Status} {ciencia.Retorno.Eventos[0].Motivo}");   // 135

// Depois de conferir a mercadoria:
var confirmacao = new ConfirmacaoOperacao(chave, "11444777000161");
await dfe.ExecutarAsync(new EnvioEvento(confirmacao));

// Ou, se a operação não aconteceu:
var naoRealizada = new OperacaoNaoRealizada(chave, "11444777000161", "Mercadoria recusada no recebimento por avaria");
await dfe.ExecutarAsync(new EnvioEvento(naoRealizada));
```

- A manifestação vai sempre ao **Ambiente Nacional**, qualquer que seja a UF ou o tipo de emissão da nota.
- A justificativa da operação não realizada tem de 15 a 255 caracteres.
- Para retificar uma manifestação conclusiva, envie outra com `sequencia: 2`.
- Não envie a manifestação junto com eventos que vão à UF (um cancelamento) no mesmo `EnvioEvento`: os destinos são outros (`NFE-LOTE`).

## Os prazos e as regras

| `cStat` | Quando |
|---|---|
| 135 | Registrada e vinculada à nota |
| 596 | Fora do prazo, contado da autorização: 10 dias para a ciência e 90 para as conclusivas |
| 594 | A sequência: a ciência só tem a 1; a conclusiva, até a 2 |
| 655 | A ciência depois de uma manifestação conclusiva |
| 650, 651 | A ciência ou o desconhecimento de uma nota cancelada |
| 575 | O autor não é o destinatário da nota |
| 494 | A nota não está na base |

## O efeito na distribuição

Depois da ciência, da confirmação ou da operação não realizada, a próxima sincronização da distribuição traz o `nfeProc` da nota (e as cartas de correção já registradas) em NSU novos. A consulta por chave (`DistribuicaoDFe.Chave`) também passa a devolver o `nfeProc` em vez do resumo. O emitente recebe o evento de manifestação na distribuição dele.

```csharp
// Resumo recebido → ciência → nota inteira.
if (documento.Tipo == TipoDocumentoDistribuido.ResumoNFe)
{
    await dfe.ExecutarAsync(new EnvioEvento(new CienciaOperacao(documento.Chave!, meuCnpj)));
    var nota = await dfe.ExecutarAsync(DistribuicaoDFe.Chave(meuCnpj, documento.Chave!));
    // nota.Retorno.Documentos[0].Tipo == TipoDocumentoDistribuido.NFeProcessada
}
```

> **Observação**
> A consulta por chave conta no limite de 20 consultas pontuais por hora. Num volume grande de notas, prefira esperar a próxima sincronização, que traz o `nfeProc` sem custo extra.

## Prompt para agentes de IA

Peça a um agente que escreva a manifestação das notas recebidas. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva a manifestação do destinatário com a OpenDFe, seguindo docs/guia/nfe/manifestacao.md: ciência automática (CienciaOperacao)
para cada resumo recebido na distribuição, dentro de 10 dias; e, pela tela do usuário, ConfirmacaoOperacao, DesconhecimentoOperacao
ou OperacaoNaoRealizada (justificativa de 15 a 255), dentro de 90 dias. O autor é o meu CNPJ <...>; o núcleo usa o certificado dele.
Não misture manifestação e cancelamento no mesmo EnvioEvento. Trate 135, 596, 594, 655, 650, 651 e 575.
```

## Próximas etapas

- [Baixar as notas destinadas: distribuição DF-e](distribuicao-dfe.md)
- [Consulta cadastro](consulta-cadastro.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
