# SEFAZ emulada

Um servidor local que responde como os autorizadores da NF-e, **só para testes e só em homologação** (`PROJETO.md` 13.5). Ele é a nossa leitura do MOC, não a SEFAZ: não substitui a homologação real (13.3), que é a única que prova a cadeia ICP-Brasil, as cifras e as peculiaridades de cada UF.

Este documento registra o que o emulador implementa: os serviços, as regras com os códigos do MOC e as falhas que dá para comandar. Cada etapa da trilha (`PLANO.md`, "Trilha paralela") acrescenta a sua parte.

Fontes: MOC 7.0, Visão Geral (4.2.2, 4.3.1 a 4.3.6, 4.4.1, 5.1, 5.2 e 5.5) e Anexo I (4.1, grupos A, B, D, DA, E, F e K; 4.2.1, os grupos A e B e o 2B08 da duplicidade; 4.4.1 e 4.4.2, os textos dos motivos), os XSDs do pacote de liberação embutido no leiaute (`docs/NTS.md`) e a "Relação de Serviços Web" do Portal Nacional, conferidos em 2026-10-19.

## Como rodar

```
dotnet run --project tests/OpenDFe.SefazEmulada [-- --porta 5443 --pasta <pasta> --endereco 127.0.0.1 --nome 127.0.0.1]
```

| Opção | Padrão | O que é |
|---|---|---|
| `--porta` | 5443 | A porta HTTPS. `0` escolhe uma livre, que é o que os testes usam. |
| `--pasta` | `%LOCALAPPDATA%/OpenDFe.SefazEmulada` (`~/.local/share/...` no Linux) | Onde a AC de teste fica guardada. |
| `--endereco` | `127.0.0.1` | O IP em que o Kestrel escuta. Com outro IP, o emulador atende a rede, mas o `/_controle` continua aceito só do próprio computador. |
| `--nome` | `127.0.0.1` | O nome usado nas URLs listadas, que também entra no certificado do servidor. O padrão é o IP porque, no Windows, `localhost` tenta o `::1` antes, e o emulador não escuta nele. |

Ao subir, o emulador mostra o endereço, o caminho do `ac.crt` e a URL do controle, e registra cada pedido numa linha (`MG/NFeStatusServico4: cStat 107`). Nos testes ele sobe pela classe `Emulador` (`Emulador.IniciarAsync`), numa porta livre e sem registrar nada.

## TLS mútuo

- **A AC de teste** é gerada na primeira partida e guardada na pasta: `ac.pfx` (com a chave, sem senha) e `ac.crt` (só o certificado, em PEM). Ela é reaproveitada enquanto valer por mais de um dia. A validade é de 10 anos, com RSA 2048 e SHA-256.
- **Quem usa o emulador confia no `ac.crt`**, nunca no repositório do sistema: no .NET, isso é `CustomRootTrust` com o `ac.crt` no `CustomTrustStore`. Pela 13.5, do lado da lib a raiz entra pela fábrica do handler, a ser fechada na 4.1.
- **O certificado do servidor** é emitido pela AC a cada partida, com validade de 90 dias e o uso de autenticação de servidor. No nome alternativo vão `localhost`, `127.0.0.1`, `::1` e o `--nome`.
- **O certificado do cliente** é exigido no handshake, como em MG e no PR, e **aceito sem conferir a cadeia**: qualquer um serve, inclusive o autoassinado dos testes (13.1), mesmo vencido. A SEFAZ exige a cadeia ICP-Brasil (Anexo I, A03 a A06); o emulador não. O certificado identifica o **transmissor** (o CNPJ ou o CPF dos campos da ICP-Brasil, ou a impressão digital do certificado sem eles), que não precisa ser o emitente (Visão Geral, 4.3.3): o recibo do lote só é consultado por ele (223). O titular comparado ao emitente (213, 227) é o do certificado da assinatura, na autorização.
- TLS 1.2 e 1.3, só HTTP/1.1.

## Rotas

| Rota | O que faz |
|---|---|
| `POST /<destino>/<serviço>` | O Web Service, como `/MG/NFeStatusServico4`. Destino e serviço não distinguem maiúsculas. |
| `GET /` | Em JSON, o `ac.crt` e as URLs prontas para trocar (`PROJETO.md` 8.4): `{ "autoridade": "...", "urls": [ { "destino": "MG", "servico": "NFeStatusServico4", "url": "https://127.0.0.1:5443/MG/NFeStatusServico4" }, ... ] }`. A lista traz cada serviço implementado em cada autorizador que o tem. |
| `/_controle/falhas` | As falhas comandadas (abaixo). |
| `/_controle/notas` | As notas autorizadas: `GET` lista, na ordem (`chave`, `numeracao`, `protocolo`, `recibo`, `recebimento`); `DELETE` esquece as notas e os lotes (204). Só do próprio computador, como as falhas. |

### Autorizadores

A tabela é do próprio emulador, sem o `Atendimento` da lib: se ele reaproveitasse o que a lib declara, um erro da lib apareceria igual dos dois lados e passaria. O `cUF` da coluna "casa" vai no retorno quando o pedido não chega a dizer qual é a UF (as rejeições antes da leitura).

| Destino | UFs atendidas | Casa |
|---|---|---|
| AM, BA, GO, MG, MS, MT, PE, PR, RS, SP | a própria | a própria |
| SVAN | MA | 91 |
| SVRS | AC, AL, AP, CE, DF, ES, PA, PB, PI, RJ, RN, RO, RR, SC, SE, TO | 43 |
| SVC-AN | AC, AL, AP, CE, DF, ES, MG, PA, PB, PI, RJ, RN, RO, RR, RS, SC, SE, SP, TO | 91 |
| SVC-RS | AM, BA, GO, MA, MS, MT, PE, PR (sem o `NFeInutilizacao4`) | 43 |
| AN | nenhuma: só `NFeDistribuicaoDFe` e `NFeRecepcaoEvento4` | 91 |

O emulador responde só os serviços que já implementa. Os outros dão 404, mesmo os que o autorizador tem.

### Envelope SOAP 1.2

O emulador lê e escreve o envelope à mão. A área de dados vem dentro do `nfeDadosMsg`, no namespace do WSDL do serviço (`http://www.portalfiscal.inf.br/nfe/wsdl/<serviço>`), e o retorno vai dentro do `nfeResultMsg`, no mesmo namespace. O espaço em branco da área de dados é guardado como veio, porque faz parte do que a nota assina; para o schema e a assinatura, a área de dados (e cada nota) é tirada do envelope sozinha, sem as declarações de namespace dele, que o C14N levaria para dentro do que é assinado. O `nfeCabecMsg` saiu no leiaute 4.00 e, se vier no `Header`, é ignorado. A resposta sai em `application/soap+xml; charset=utf-8`, em UTF-8 e sem BOM.

| Pedido | Resposta |
|---|---|
| Destino ou serviço que o emulador não atende | HTTP 404, sem corpo |
| `Content-Type` que não é `application/soap+xml` | HTTP 415, sem corpo |
| Envelope SOAP 1.1 | HTTP 500, falha `soap:VersionMismatch` |
| XML mal formado, com DTD, fora de um `Envelope` SOAP 1.2, sem `Body` ou sem `nfeDadosMsg` no namespace do serviço | HTTP 400, falha `soap:Sender` |
| Corpo inteiro acima de 500 KB (B01) | HTTP 200, `cStat` 214 |
| `nfeDadosMsg` vazio | HTTP 200, `cStat` 516 (565 na autorização) |

A ligação HTTP das falhas segue o SOAP 1.2, parte 2, 7.5.1: `Sender` é 400; os outros são 500.

## Serviços

Em todo retorno, `tpAmb` é 2 e `verAplic` é a sigla do destino seguida de `-EMULADA` (FR04: começa pela sigla da UF, ou por SVAN ou SVRS). O `dhRecbto` sai no horário de Brasília (`-03:00`), e o `xMotivo` traz o texto do Anexo I, 4.4.1, sem a referência à NT.

### NFeStatusServico4 (E.1)

Pedido `consStatServ` 4.00 (`tpAmb`, `cUF`, `xServ` = `STATUS`), retorno `retConsStatServ` 4.00 na ordem da Tabela 5-18. As regras, na ordem em que o emulador as aplica:

| Regra | cStat | Motivo |
|---|---|---|
| D01 | 516 | A raiz não é `consStatServ` no namespace da NF-e. |
| D02 | 404 | Algum elemento da área de dados usa prefixo de namespace, mesmo apontando para o namespace da NF-e. |
| D03 | 517 | A raiz não tem o atributo `versao`. |
| B06 | 239 | `versao` diferente de 4.00. |
| (schema) | 215 | A estrutura no lugar do XSD: os três filhos na ordem, nada além deles, sem atributo na raiz além de `versao`, `tpAmb` 1 ou 2, `cUF` válido e `xServ` = `STATUS`. |
| K01 | 252 | `tpAmb` = 1: o emulador é só homologação. |
| K02 | 289 | O autorizador não atende a `cUF` do pedido. |
| K03, K04 | 108, 109 | O serviço está paralisado (comandado; abaixo). O `xObs` sai se o comando trouxer uma observação. |
| | 107 | Em operação, com o `cUF` do pedido e `tMed` = 1. |

O `tMed` (FR09) só sai no 107, porque é o tempo médio do serviço em operação. O emulador responde em menos de 1 segundo, e abaixo de 1 o manual manda informar 1 (4.3.6). Nas rejeições, o `cUF` do retorno é o da casa do autorizador; no 107 e na paralisação, é o do pedido.

### NFeAutorizacao4 (E.2)

Pedido `enviNFe` 4.00 (`idLote`, `indSinc`, de 1 a 50 `NFe`), retorno `retEnviNFe` 4.00 na ordem da Tabela 5-2, com o `cUF` da casa do autorizador (o da UF que atendeu). O schema é o `enviNFe_v4.00.xsd` do leiaute, que valida as notas dentro do lote, pelo `SchemaXml` da lib: uma nota fora do schema rejeita o lote inteiro (Anexo I, 4.1.3).

As regras do lote, na ordem:

| Regra | cStat | Motivo |
|---|---|---|
| D01b | 565 | A raiz não é `enviNFe` no namespace da NF-e. |
| D02 | 404 | Algum elemento usa prefixo de namespace. |
| D01e | 568 | A raiz não tem o atributo `versao`. |
| B06 | 239 | `versao` diferente de 4.00. |
| D01 | 225 | O lote, com as notas, não passa no schema (inclusive a nota sem a `Signature`, e a mesma nota duas vezes, porque o `Id` é `xs:ID`). |
| GAP03a-1 | 764 | `indSinc` = 1 com mais de uma nota. |
| 4.2.1, B | 765 | NF-e e NFC-e no mesmo lote. |
| B03, B04 | 108, 109 | O serviço está paralisado (comandado). |

Passando o lote, cada nota recebe o seu `protNFe`:

- **síncrona** (`indSinc` = 1, uma nota): `cStat` 104 ("Lote processado") com o `protNFe`;
- **assíncrona**: `cStat` 103 ("Lote recebido com sucesso") com o `infRec` (`nRec` e `tMed` = 1); os protocolos ficam com o lote, para o `NFeRetAutorizacao4`. O emulador processa o lote na hora; a fila só aparece quando comandada (`loteProcessando`).

As regras de cada nota, na ordem:

| Regra (Anexo I) | cStat | Motivo |
|---|---|---|
| C02-20, C02a-30 | 207, 401 | O CNPJ (também o alfanumérico) ou o CPF do emitente com o DV errado. Conferido com código próprio, sem a lib. |
| E01, E03, F01, F02 | 290, 292, 298, 297 | A assinatura: o certificado (uso de chave, versão), o titular sem CNPJ nem CPF, a estrutura fora do padrão (Visão Geral, 4.2.4) e o valor que não confere. Pela verificação da lib (`AssinaturaDigital.Verificar`). A cadeia, a validade e a revogação (291, 293 a 296) **não** são conferidas: o autoassinado dos testes passa. |
| F03, F03A | 213, 227 | O titular do certificado da assinatura contra o emitente: a raiz do CNPJ (a matriz assina pela filial) ou o CPF inteiro. |
| A03-10 | 502 | O `Id` não é a concatenação dos campos da chave (cUF, AAMM do `dhEmi`, CNPJ ou CPF com zeros à esquerda, modelo, série, número, `tpEmis`, `cNF` e `cDV`). |
| 4.2.1, B | 253 | O DV da chave composta não confere (módulo 11; a letra vale o código ASCII menos 48). |
| 4.2.1, B | 252 | `tpAmb` = 1 na nota. |
| 4.2.1, B | 226 | O autorizador não atende o `cUF` da nota. |
| 4.2.1, B | 476 | O `cUF` difere do da primeira nota do lote. |
| 4.2.1, B | 570 | `tpEmis` 3, 6 ou 7 num autorizador que não é a SVC. |
| 4.2.1, B | 783 | NFC-e na SVC. |
| 4.2.1, B | 713 | Na SVC, `tpEmis` diferente de 6 (SVC-AN) ou de 7 (SVC-RS). |
| 4.2.1, B | 703 | `dhEmi` mais de 5 minutos depois do recebimento. |
| 4.2.1, B | 228 | NF-e com `tpEmis` 1, 6 ou 7 emitida há mais de 30 dias. |
| 4.2.1, B | 704 | NFC-e com `tpEmis` 1, 3, 6 ou 7 emitida há mais de 5 minutos. A de contingência (9 e 4) não é rejeitada; o 150 ("autorização fora de prazo") da NFC-e offline depois de 24 horas entra na E.3. |
| 2B08-10 | 539 | A mesma numeração (modelo, UF, CNPJ ou CPF do emitente, série e número; na NFC-e, também o `tpEmis`) já autorizada com outra chave. O motivo leva `[chNFe:<a chave autorizada>]` e, se ela veio num lote assíncrono, `[nRec:<o recibo dele>]`. |
| 2B08-20 | 204 | A mesma chave já autorizada. O motivo leva `[nRec:...]` como no 539. O emulador não devolve o protocolo da nota igual (a observação 2, a critério da UF). |
| | 100 | Autorizada: o `nProt` e a nota registrada. |

O `protNFe` 4.00 (Tabela 5-6) sai sem o `Id` e sem a `Signature`, que são a critério da UF: `tpAmb`, `verAplic`, `chNFe`, `dhRecbto`, `nProt` (só no 100), `digVal` (o `DigestValue` da assinatura da nota, quando há), `cStat` e `xMotivo`. A nota rejeitada não é registrada e pode ser corrigida e enviada de novo. A duplicidade vale entre autorizadores: a base é uma só, como a do país.

Ainda não: a denegação (110, 301, 302) e as notas canceladas ou denegadas na duplicidade (218, 205), que chegam com os eventos (E.4); as regras de conteúdo da nota que a lib já confere antes (`PROJETO.md` 6.2), como os totais.

### NFeRetAutorizacao4 (E.2)

Pedido `consReciNFe` 4.00 (`tpAmb`, `nRec`), retorno `retConsReciNFe` 4.00 na ordem da Tabela 5-6, com o `cUF` da casa do autorizador. O schema é o `consReciNFe_v4.00.xsd` do leiaute. As regras, na ordem:

| Regra | cStat | Motivo |
|---|---|---|
| D01b, D02, D01e, B06 | 516, 404, 517, 239 | A raiz, como no status. |
| D01 | 215 | O pedido não passa no schema. |
| E01 | 252 | `tpAmb` = 1. |
| E02 | 248 | As 2 primeiras posições do recibo não são a casa do autorizador. |
| E02a | 553 | A 3ª posição não é o tipo do autorizador. |
| B03, B04 | 108, 109 | O serviço está paralisado (comandado). |
| E03 | 106 | O autorizador não recebeu esse lote. Cada autorizador tem a sua fila: o recibo da SVRS não é achado na SVC-RS, mesmo com a mesma casa e o mesmo tipo. |
| E05 | 223 | O transmissor da consulta (o certificado do TLS) não é o do lote. |
| E04 | 105 | O lote ainda em processamento (comandado: `loteProcessando`, pelo controle ou pela marca). |
| | 104 | Processado, com os `protNFe` das notas na ordem do lote. A consulta pode ser repetida. |

O `nRec` do retorno é o do pedido; quando não dá para lê-lo (as rejeições até o 215), vai com zeros (BR06).

### Recibo e protocolo

| Número | Formação | Exemplos |
|---|---|---|
| Recibo (`nRec`, Visão Geral, 4.3.4) | cUF da casa do autorizador + tipo de autorizador + 12 posições sequenciais | `311000000000001` (MG), `433...` (SVRS), `914...` (SVAN), `912...` (SVC-AN) |
| Protocolo (`nProt`, 4.3.5) | tipo de autorizador + cUF da nota + ano (horário de Brasília) + 10 posições sequenciais | `131260000000002` (MG), `342...` (SC na SVRS) |

O tipo de autorizador é 1 nos autorizadores de UF, 2 na SVC-AN (a Receita, que tinha o SCAN), 3 na SVRS e na SVC-RS (o mesmo órgão) e 4 na SVAN. A sequência é uma só no emulador, para o recibo e o protocolo.

### O que o emulador guarda

Em memória, vazio a cada partida (ou depois do `DELETE /_controle/notas`): as notas autorizadas (a chave, a numeração da duplicidade, o protocolo, o recibo do lote e o recebimento) e os lotes assíncronos (o recibo, o transmissor, os protocolos e as consultas que ainda respondem 105). Um lote é processado de uma vez, sem outro no meio: as notas do mesmo lote e de lotes simultâneos se veem na ordem.

## Falhas comandadas

Elas são comandadas pela rota `/_controle`, aceita só do próprio computador (loopback, também o IPv4 mapeado em IPv6; de fora, HTTP 403). A rota passa pelo mesmo TLS mútuo. O estado fica em memória e volta vazio a cada partida.

| Rota | O que faz |
|---|---|
| `POST /_controle/falhas` | Acrescenta uma falha. Devolve HTTP 201 com a falha e o `Location`, ou 400 com `{ "erro": "..." }`. |
| `GET /_controle/falhas` | As falhas ativas, na ordem em que foram comandadas. |
| `DELETE /_controle/falhas/{id}` | Remove uma falha: 204, ou 404 se ela não existe. |
| `DELETE /_controle/falhas` | Remove todas: 204. |

O pedido, em JSON:

```json
{ "tipo": "paralisacao", "destino": "MG", "servico": "NFeStatusServico4", "vezes": 1, "codigo": "109", "observacao": "Manutenção" }
```

| Campo | O que é |
|---|---|
| `tipo` | Obrigatório. Os valores estão na tabela abaixo. |
| `destino`, `servico` | A que pedidos a falha vale. Nulo vale para todos. O serviço pode ser um que o emulador ainda não implementa. |
| `vezes` | Quantos pedidos a falha atinge (≥ 1). Nulo: até ser removida. |
| `milissegundos` | Só no `atraso` (> 0). |
| `depois` | Só na `conexaoDerrubada`. |
| `codigo`, `observacao` | Só na `paralisacao`: o código é 108 (padrão) ou 109, e a observação vai no `xObs`. |

| Tipo | Efeito |
|---|---|
| `atraso` | Espera os milissegundos e depois responde normalmente. |
| `tempoEsgotado` | Nunca responde: segura o pedido até o cliente desistir (ou o emulador parar) e derruba a conexão. |
| `conexaoDerrubada` | Derruba a conexão sem resposta, antes de processar o pedido. Com `depois`, derruba depois de processar: é o caso da `RecuperacaoAutorizacao` (8.6), que passa a importar na E.2, quando processar muda o estado. |
| `respostaIlegivel` | HTTP 200 com o envelope cortado ao meio. |
| `falhaSoap` | HTTP 500 com a falha `soap:Receiver`. |
| `paralisacao` | O `cStat` 108 ou 109 no lugar do 107, depois das rejeições do próprio pedido. Na autorização e no recibo, também depois das rejeições do pedido, antes das notas e do lote. |
| `loteProcessando` | A consulta do recibo responde 105, como se o lote ainda estivesse na fila (E04). Só no `NFeRetAutorizacao4` (o `servico` nulo ou ele); o 106 e o 223 continuam na frente. |

Cada pedido consome **a primeira falha de transporte** que vale para ele, **a primeira paralisação** e, na consulta do recibo, **o primeiro lote em processamento**, na ordem em que foram comandadas. Elas podem valer juntas: um atraso seguido do 109, por exemplo. O `vezes` é descontado a cada pedido atingido, e a falha some quando chega a zero.

A `conexaoDerrubada` com `depois` na autorização é a queda no meio: a nota fica autorizada e a resposta não chega; o reenvio da mesma nota é 204, e a recuperação consulta pela chave (o `NFeConsultaProtocolo4`, na E.3).

### A marca no `infCpl`

Para provocar a falha a partir do PDV ou do ERP, sem a rota de controle: o texto `EMULADA:<tipo>[:<valor>]` no `infCpl` de uma nota do lote, no `NFeAutorizacao4`. A marca vale só para o pedido que a traz e passa na frente da falha do controle da mesma espécie, que fica para o pedido seguinte.

| Marca | Efeito |
|---|---|
| `EMULADA:atraso:<milissegundos>` | O `atraso`. |
| `EMULADA:tempoEsgotado` | O `tempoEsgotado`. |
| `EMULADA:conexaoDerrubada` | Derruba antes de processar: a nota não é autorizada. |
| `EMULADA:conexaoDerrubada:depois` | Autoriza e derruba: a queda no meio. |
| `EMULADA:respostaIlegivel` | A `respostaIlegivel`. |
| `EMULADA:falhaSoap` | A `falhaSoap`. |
| `EMULADA:paralisacao`, `EMULADA:paralisacao:109` | A `paralisacao` (108 se omitido). |
| `EMULADA:loteProcessando[:<n>]` | No lote assíncrono, as `n` primeiras consultas do recibo respondem 105 (1 se omitido). Na síncrona, não faz nada. |

O resto do texto fica como está (`Pedido 123 EMULADA:paralisacao:109`). Uma marca por espécie no lote (transporte, paralisação, lote em processamento). A marca que não serve (tipo desconhecido, valor que o tipo não aceita, duas da mesma espécie) responde HTTP 400 com a falha `soap:Sender` e a explicação, sem processar o lote: o erro de digitação não passa calado.
