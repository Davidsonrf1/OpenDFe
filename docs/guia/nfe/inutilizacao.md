# Inutilizar numeração

[Documentação](../../index.md) › NF-e e NFC-e › Inutilização

A inutilização (`NFeInutilizacao4`) retira da sequência uma faixa de números que **não foram usados**: um salto de numeração, uma nota que não chegou a sair, a numeração de uma NFC-e rejeitada que não será reemitida. Ela vale para a NF-e e para a NFC-e, conforme o núcleo.

## Inutilizar uma faixa

```csharp
var inutilizacao = new Inutilizacao(
    cnpj: "11.222.333/0001-81",
    serie: 1,
    numeroInicial: 10,
    numeroFinal: 15,
    justificativa: "Salto de numeracao no sistema de vendas");

var resultado = await dfe.ExecutarAsync(inutilizacao);
var retorno = resultado.Retorno!;

if (retorno.Homologada)                       // 102
    Guardar(retorno.XmlProcessado!);          // o ProcInutNFe
else
    Console.WriteLine($"{retorno.Status} {retorno.Motivo}");
```

- O CNPJ é o do emitente, com ou sem máscara. Para uma faixa de um só número, use o mesmo número no início e no fim.
- A justificativa tem de 15 a 255 caracteres.
- O ano (2 dígitos, no `infInut`) sai do relógio da configuração, no fuso de Brasília. Para inutilizar uma faixa do ano anterior, informe: `new Inutilizacao(...) { Ano = 25 }`.
- O modelo e a UF saem do núcleo.

## O retorno

| `cStat` | Significado | O que fazer |
|---|---|---|
| 102 | Inutilização homologada | Guardar o `XmlProcessado` (o `ProcInutNFe`) |
| 563 | A mesma faixa já foi inutilizada | O `Protocolo` traz o da inutilização anterior; guarde-o |
| 256 | Um número da faixa já foi inutilizado em outra faixa | Ajuste a faixa |
| 241 | Um número da faixa já foi usado por uma nota autorizada | Ajuste a faixa: a nota existe |
| 224 | O número inicial maior que o final | Corrija |
| 201 | Mais de 10.000 números | Divida em faixas menores |

`RetornoInutilizacao` traz também a faixa (`Serie`, `NumeroInicial`, `NumeroFinal`), o `Ano`, o `Cnpj`, o `Modelo`, o `Protocolo` e o `Recebimento`.

> **Observação**
> O schema da inutilização só tem o CNPJ: o emitente com CPF não tem inutilização neste leiaute. A SVC não oferece a inutilização: ela vai sempre ao autorizador normal da UF.

## Sem a resposta

Reenvie a mesma faixa. Se a primeira chegou, a SEFAZ responde 563, com o protocolo da primeira; se não chegou, a segunda é homologada (102).

## Prompt para agentes de IA

Peça a um agente que escreva a inutilização dos saltos de numeração. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva uma rotina que detecta os saltos de numeração do meu sistema <descreva onde estão as notas> e os inutiliza com a OpenDFe,
seguindo docs/guia/nfe/inutilizacao.md: new Inutilizacao(cnpj, serie, numeroInicial, numeroFinal, justificativa de 15 a 255),
em faixas de no máximo 10.000 números, só de números que nunca foram transmitidos (nunca os que estão "em dúvida").
Trate 102 (guardar o XmlProcessado), 563 (já inutilizada: guardar o Protocolo), 241 e 256. Na queda, reenvie a mesma faixa.
```

## Próximas etapas

- [Cancelar e corrigir: eventos](eventos.md)
- [Quando a conexão cai](queda-recuperacao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
