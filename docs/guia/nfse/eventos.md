# Cancelar e substituir a NFS-e

[Documentação](../../index.md) › NFS-e › Eventos

Os eventos da NFS-e vão à Sefin Nacional pelo `EnvioEventoNFSe` e se consultam pelo `ConsultaEventosNFSe`. Quem emite envia dois: o **cancelamento** (101101) e a **solicitação de análise fiscal para cancelamento** (101103). A **substituição não é um evento que você envia**: é uma DPS nova com o grupo `Substituicao`, e a Sefin gera sozinha o evento 105102 sobre a nota substituída.

| O quê | Como | Código |
|---|---|---|
| Cancelar | `EnvioEventoNFSe(new CancelamentoNFSe { ... })` | 101101 |
| Pedir a análise fiscal do cancelamento (fora do prazo) | `EnvioEventoNFSe(new AnaliseFiscalCancelamento { ... })` | 101103 |
| Substituir | `EmissaoNFSe(dps)` com `dps.Substituicao` preenchida | 105102, gerado pela Sefin |
| Consultar os eventos | `ConsultaEventosNFSe(chave)`, com `Tipo` e `Sequencia` opcionais | todos, de qualquer autor |

## Cancelar

```csharp
var cancelamento = new CancelamentoNFSe
{
    CnpjAutor = "11222333000181",       // o prestador
    Chave = chave.Valor,
    Motivo = MotivoCancelamento.ErroEmissao,
    Descricao = "Erro no valor do serviço",
};

var envio = await dfe.ExecutarAsync(new EnvioEventoNFSe(cancelamento));
if (envio.Retorno!.Registrado)
    Guardar(envio.Retorno.Evento!.Xml);   // o evento registrado, assinado pela Sefin
else
    foreach (var erro in envio.Retorno.Erros)
        Console.WriteLine(erro);
```

- A data do evento nula vale o relógio da configuração, no horário de Brasília.
- O `EnvioEventoNFSe` recusa no construtor o evento que não é de quem emite (os do município e do fisco só se leem).
- Fora do prazo de cancelamento do município, envie a `AnaliseFiscalCancelamento`: o município defere (105104) ou indefere (105105), e a consulta dos eventos mostra a decisão.

## Substituir

A substituição é a emissão de uma DPS nova com a chave da nota que ela substitui:

```csharp
dpsNova.Substituicao = new Substituicao
{
    Chave = chaveAntiga.Valor,
    Motivo = MotivoSubstituicao.Outros,
    Descricao = "Correção da descrição do serviço",
};
var emissao = await dfe.ExecutarAsync(new EmissaoNFSe(dpsNova));
```

A nota substituída passa a ter o evento 105102, com a chave da substituta. Ela não pode ser cancelada nem substituída de novo.

## Consultar os eventos

```csharp
var consulta = await dfe.ExecutarAsync(new ConsultaEventosNFSe(chave));
foreach (var evento in consulta.Retorno!.Eventos)
    Console.WriteLine($"{evento.Tipo} {evento.Sequencia} {evento.Processamento}");

var cancelada = consulta.Retorno.Eventos.Any(e => e.Tipo == "101101");
var substituida = consulta.Retorno.Eventos.Any(e => e.Tipo == "105102");
```

Com `Tipo` (e `Sequencia`), a consulta traz só aquele evento. É também o que se faz **depois de uma queda no meio do envio**: consulte pelo tipo antes de reenviar; o reenvio de um evento que já entrou é recusado pela Sefin.

> **Dica**
> A situação que a consulta mostra é o que vai no [DANFSe](../impressao/danfse.md): `OpcoesDanfse.Cancelada` e `OpcoesDanfse.Substituida`.

## O que guardar

- O XML de cada evento registrado (`Evento.Xml`).
- Na substituição, a chave da substituta junto da substituída.

## Prompt para agentes de IA

Peça a um agente que escreva o cancelamento e a substituição. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva o cancelamento e a substituição da NFS-e com a OpenDFe, seguindo docs/guia/nfse/eventos.md: EnvioEventoNFSe com
CancelamentoNFSe (autor, chave, motivo, descrição); a substituição como EmissaoNFSe de uma DPS com Substituicao (chave,
motivo, descrição); ConsultaEventosNFSe para saber a situação (101101, 105102) e depois de uma queda. Guarde o XML de cada
evento. Os dados vêm de <descreva>.
```

## Próximas etapas

- [Consultar a parametrização do município](parametrizacao.md)
- [Imprimir o DANFSe](../impressao/danfse.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
