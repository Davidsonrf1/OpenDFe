# Serviços e códigos de retorno da SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Serviços e códigos de retorno

Este artigo resume o que cada serviço da SEFAZ emulada confere e os códigos de retorno (`cStat`) que ela devolve, para você saber como provocar cada caso num teste. A especificação completa, com a regra do MOC de cada código e a ordem em que as regras são aplicadas, está em [SEFAZ_EMULADA.md](../../SEFAZ_EMULADA.md).

## Comum a todos os serviços

- **Endereço:** `POST https://<host>:<porta>/<autorizador>/<serviço>`, como `/MG/NFeAutorizacao4`. O autorizador e o serviço não distinguem maiúsculas.
- **Envelope:** SOAP 1.2 (`application/soap+xml`), com a mensagem no `nfeDadosMsg`, no namespace do WSDL do serviço. O SOAP 1.1 é recusado (`soap:VersionMismatch`).
- **Só homologação:** `tpAmb` 1 é rejeitado com o 252.
- **No retorno:** `tpAmb` 2, `verAplic` com a sigla do autorizador seguida de `-EMULADA` (`MG-EMULADA`), `dhRecbto` no horário de Brasília e o `xMotivo` com o texto do MOC.

| Pedido | Resposta |
|---|---|
| Autorizador ou serviço que o emulador não atende | HTTP 404 |
| `Content-Type` que não é `application/soap+xml` | HTTP 415 |
| XML mal formado, com DTD, sem `Body` ou sem `nfeDadosMsg` | HTTP 400, falha `soap:Sender` |
| Mensagem acima de 500 KB (10 KB na distribuição) | `cStat` 214 |
| Raiz errada, prefixo de namespace, sem `versao`, outra versão | 516 (565 na autorização), 404, 517 (568), 239 |
| Fora do schema | 215 (225 na autorização) |
| Paralisação comandada | 108 ou 109 |

## Os autorizadores

| Autorizador | UFs atendidas |
|---|---|
| AM, BA, GO, MG, MS, MT, PE, PR, RS, SP | a própria |
| SVAN | MA |
| SVRS | AC, AL, AP, CE, DF, ES, PA, PB, PI, RJ, RN, RO, RR, SC, SE, TO |
| SVC-AN | AC, AL, AP, CE, DF, ES, MG, PA, PB, PI, RJ, RN, RO, RR, RS, SC, SE, SP, TO |
| SVC-RS | AM, BA, GO, MA, MS, MT, PE, PI, PR (sem a inutilização) |
| AN (Ambiente Nacional) | só a distribuição DF-e e a manifestação do destinatário |

A tabela do emulador é dele mesmo, conferida na "Relação de Serviços Web" do Portal Nacional, e não a da biblioteca: um erro da biblioteca não se repete do outro lado.

## NFeStatusServico4

| `cStat` | Quando |
|---|---|
| 107 | Em operação (`tMed` 1) |
| 108, 109 | Paralisado (comandado), com o `xObs` da observação |
| 289 | O autorizador não atende a UF do pedido |

## NFeAutorizacao4

**O lote:** 104 na síncrona (uma nota, `indSinc` 1), com o `protNFe`; 103 no lote (de 2 a 50 notas, `indSinc` 0), com o recibo. Rejeições do lote: 225 (fora do schema, inclusive a nota sem assinatura), 764 (síncrona com mais de uma nota), 452 (lote com uma nota só), 765 (NF-e e NFC-e no mesmo lote).

**Cada nota**, na ordem em que as regras são aplicadas:

| `cStat` | Quando |
|---|---|
| 207, 401 | O CNPJ ou o CPF do emitente com o dígito errado |
| 290, 292, 297, 298 | A assinatura: o certificado, o titular sem CNPJ nem CPF, o valor que não confere, a estrutura |
| 213, 227 | O titular do certificado da assinatura não é o emitente (a raiz do CNPJ ou o CPF) |
| 502, 253 | O `Id` não é a chave dos campos; o dígito da chave |
| 252, 226, 476 | Produção; a UF que o autorizador não atende; outra UF no mesmo lote |
| 711 | NF-e com `tpEmis` 9 sem o DANFE Simplificado Tipo 2 |
| 570, 783, 713 | Tipo de emissão de SVC fora da SVC; NFC-e na SVC; o `tpEmis` que não é o da SVC |
| 703 | A emissão mais de 5 minutos no futuro |
| 704 | NFC-e (ou NF-e com `tpImp` 6) emitida há mais de 5 minutos, fora de contingência |
| 228, 150 | NF-e emitida há mais de 30 dias (228); há mais de 7 dias, autorizada fora de prazo (150) |
| 393 a 903 | O QR Code (abaixo) |
| 781 | O emitente irregular |
| 539, 204, 218 | A duplicidade: a mesma numeração com outra chave; a mesma chave autorizada; a mesma chave cancelada. O motivo leva `[chNFe:...]` e, se veio de lote, `[nRec:...]` |
| 206 | A numeração inutilizada |
| 302 | O destinatário irregular (NF-e) |
| 100, 150 | Autorizada; autorizada fora de prazo (contingência transmitida depois de 24 horas, ou a NF-e com mais de 7 dias) |

Não há denegação (NT 2024.001): nenhuma nota sai com 110.

**O QR Code** (NFC-e e NF-e com `tpImp` 6): 393 (o grupo na NF-e sem `tpImp` 6), 394 (sem o grupo), 903 (a versão 100), 672 (NF-e com `tpImp` 6 fora da versão 3), 444 (versão 2 do emitente CPF), 397 (um parâmetro que não confere com a nota), 462 e 463 (o CSC não cadastrado ou revogado), 464 (o hash), 583 (a assinatura da versão 3 offline). As URLs do QR Code e da consulta não são conferidas.

## NFeRetAutorizacao4

| `cStat` | Quando |
|---|---|
| 104 | Processado, com os protocolos na ordem do lote (a consulta pode ser repetida) |
| 105 | Em processamento (comandado: `loteProcessando`) |
| 106 | O autorizador não recebeu esse lote (cada autorizador tem a sua fila) |
| 223 | O transmissor da consulta não é o do lote |
| 248, 553 | O recibo não é deste autorizador |

## NFeConsultaProtocolo4

| `cStat` | Quando |
|---|---|
| 100, 150 | Autorizada, com o `protNFe` e os eventos |
| 101 | Cancelada (pelo 110111 ou pelo 110112), com o `procEventoNFe` |
| 217 | Nenhuma nota com essa numeração |
| 562 | A base tem a numeração com outro `cNF`; o motivo leva a chave da base quando o transmissor é o emitente |
| 561, 613 | Outro mês na chave; outra posição da chave diferente |
| 236, 614 a 619 | A chave inválida (dígito, UF, ano, mês, documento, modelo, número) |

A SVC só acha as notas que ela autorizou; o autorizador normal acha também as da SVC.

## NFeRecepcaoEvento4

O lote responde 128, com um `retEvento` por evento, na ordem.

**Parte geral:** 491 (tipo de evento não aceito), 492 (versão), 493 (o detalhe fora do schema), 489 e 490 (o autor com o dígito errado), 572 (o `Id`), 250 (o órgão), 574 (o autor não é o emitente), 578 (o evento no futuro), 240 (o autor irregular), 573 (o evento já registrado).

| Evento | Regras e códigos |
|---|---|
| Cancelamento (110111) | 594 (sequência diferente de 1), 494 (nota não está na base), 501 (mais de 24 horas), 580 (já cancelada), 577 e 579 (data), 222 (protocolo errado), 221 (a confirmação do destinatário impede); 135 registrado |
| Cancelamento por substituição (110112) | 920 (a cancelada não é da emissão normal), 921 (a substituta é da emissão normal), 455, 910, 911, 912 a 919 (a substituta: inexistente, cancelada, mais de 2 horas depois, valor, ICMS, destinatário, itens), 501 (mais de 168 horas); 135 registrado |
| Carta de correção (110110) | 594 (sequência acima de 20), 784 (NFC-e), 494, 580, 577; 135 registrado. A carta nova substitui a anterior |
| Manifestação do destinatário (no AN) | 494, 575 (o autor não é o destinatário), 577, 579, 594 (sequência), 596 (fora do prazo: 10 dias na ciência, 90 nas conclusivas), 650, 651, 655, 496; 135 registrado |

A SVC registra só o cancelamento das notas que ela autorizou. A manifestação vai só ao Ambiente Nacional; os autorizadores das UFs a recusam com 491. A EPEC não está implementada.

## NFeInutilizacao4

| `cStat` | Quando |
|---|---|
| 102 | Homologada, com a faixa e o protocolo |
| 563 | A mesma faixa já inutilizada (o retorno traz o protocolo da anterior) |
| 256 | Um número da faixa já inutilizado em outra faixa |
| 241 | Um número da faixa já usado por uma nota autorizada |
| 224, 201 | O início maior que o fim; mais de 10.000 números |
| 266, 453, 454 | A série do emitente CPF ou do PAA; o ano no futuro ou antes de 2006 |
| 203, 213, 240, 250, 502 | O CNPJ, o titular, o emitente irregular, a UF, o `Id` |

A SVC não oferece a inutilização (404).

## NFeDistribuicaoDFe

No Ambiente Nacional. O emulador gera um NSU por interessado (cada estabelecimento, pelas 14 posições do CNPJ), sem lacunas, e entrega o que a NT 2014.002 manda:

| Quem é na nota | O que recebe |
|---|---|
| Destinatário | O `resNFe` na autorização; o cancelamento; o `nfeProc` e as cartas depois da ciência, da confirmação ou da operação não realizada |
| Transportador e autorizados (`autXML`) | O `nfeProc` e os eventos |
| Emitente | Os eventos de manifestação do destinatário |

| `cStat` | Quando |
|---|---|
| 138 | Documentos localizados (até 50 por resposta) |
| 137 | Nenhum documento novo |
| 656 | Uso indevido: consulta antes de 1 hora depois do 137, `ultNSU` fora da sequência, mais de 20 consultas pontuais por hora. O bloqueio dura 1 hora e recomeça a cada consulta feita nele |
| 589 | O NSU acima do maior do interessado |
| 593, 472 | O certificado do TLS não é do interessado (a raiz do CNPJ, ou o CPF) |
| 632, 640, 641, 653 | Na consulta por chave: mais de 90 dias, interessado que não é ator da nota, o emitente, a nota cancelada |

## CadConsultaCadastro4

| `cStat` | Quando |
|---|---|
| 111, 112 | Uma ou mais ocorrências |
| 259, 261, 264 | O CNPJ, a IE ou o CPF fora do cadastro |
| 258, 260, 263 | O CNPJ, a IE ou o CPF inválidos |
| 265 | A UF que não é do Web Service |
| 257 | O solicitante irregular |

O cadastro é o que você cadastra em `/_controle/cadastros` (veja [Preparar dados de teste](dados-teste.md)). O serviço existe nas 10 UFs com autorizador próprio e na SVRS (para AC, ES, RN, PB e SC).

## Prompt para agentes de IA

Peça a um agente que monte os casos de teste de uma rejeição. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Quero testar que o meu sistema trata a rejeição <cStat> da operação <...>. Usando docs/guia/sefaz-emulada/servicos.md e
docs/SEFAZ_EMULADA.md, diga como provocar esse cStat na SEFAZ emulada (o dado da nota, a ordem das operações, a falha comandada ou o
cadastro na API de controle) e escreva o teste xUnit, conferindo o Protocolo.Status (na autorização) ou o Status do retorno.
```

## Próximas etapas

- [Simular falhas](falhas-comandadas.md)
- [Referência das rotas de controle](api-controle.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
