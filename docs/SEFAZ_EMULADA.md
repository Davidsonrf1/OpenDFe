# SEFAZ emulada

Um servidor local que responde como os autorizadores da NF-e, **só para testes e só em homologação** (`PROJETO.md` 13.5). Ele é a nossa leitura do MOC, não a SEFAZ: não substitui a homologação real (13.3), que é a única que prova a cadeia ICP-Brasil, as cifras e as peculiaridades de cada UF.

Este documento registra o que o emulador implementa: os serviços, as regras com os códigos do MOC e as falhas que dá para comandar. Cada etapa da trilha (`PLANO.md`, "Trilha paralela") acrescenta a sua parte.

Fontes: MOC 7.0, Visão Geral (4.2.2, 4.3.6, 4.4.1 e 5.5) e Anexo I (4.1, grupos A, B, D e K; 4.4.1, os textos dos motivos), e a "Relação de Serviços Web" do Portal Nacional, conferida em 2026-10-19.

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
- **O certificado do cliente** é exigido no handshake, como em MG e no PR, e **aceito sem conferir a cadeia**: qualquer um serve, inclusive o autoassinado dos testes (13.1), mesmo vencido. A SEFAZ exige a cadeia ICP-Brasil (Anexo I, A03 a A06); o emulador não. O titular contra o emitente (213) entra na E.2.
- TLS 1.2 e 1.3, só HTTP/1.1.

## Rotas

| Rota | O que faz |
|---|---|
| `POST /<destino>/<serviço>` | O Web Service, como `/MG/NFeStatusServico4`. Destino e serviço não distinguem maiúsculas. |
| `GET /` | Em JSON, o `ac.crt` e as URLs prontas para trocar (`PROJETO.md` 8.4): `{ "autoridade": "...", "urls": [ { "destino": "MG", "servico": "NFeStatusServico4", "url": "https://127.0.0.1:5443/MG/NFeStatusServico4" }, ... ] }`. A lista traz cada serviço implementado em cada autorizador que o tem. |
| `/_controle/falhas` | As falhas comandadas (abaixo). |

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

O emulador lê e escreve o envelope à mão. A área de dados vem dentro do `nfeDadosMsg`, no namespace do WSDL do serviço (`http://www.portalfiscal.inf.br/nfe/wsdl/<serviço>`), e o retorno vai dentro do `nfeResultMsg`, no mesmo namespace. O `nfeCabecMsg` saiu no leiaute 4.00 e, se vier no `Header`, é ignorado. A resposta sai em `application/soap+xml; charset=utf-8`, em UTF-8 e sem BOM.

| Pedido | Resposta |
|---|---|
| Destino ou serviço que o emulador não atende | HTTP 404, sem corpo |
| `Content-Type` que não é `application/soap+xml` | HTTP 415, sem corpo |
| Envelope SOAP 1.1 | HTTP 500, falha `soap:VersionMismatch` |
| XML mal formado, com DTD, fora de um `Envelope` SOAP 1.2, sem `Body` ou sem `nfeDadosMsg` no namespace do serviço | HTTP 400, falha `soap:Sender` |
| Corpo inteiro acima de 500 KB (B01) | HTTP 200, `cStat` 214 |
| `nfeDadosMsg` vazio | HTTP 200, `cStat` 516 |

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
| `paralisacao` | O `cStat` 108 ou 109 no lugar do 107, depois das rejeições do próprio pedido. |

Cada pedido consome **a primeira falha de transporte** que vale para ele e **a primeira paralisação**, na ordem em que foram comandadas. As duas podem valer juntas: um atraso seguido do 109, por exemplo. O `vezes` é descontado a cada pedido atingido, e a falha some quando chega a zero.

A marca no `infCpl` da nota, para provocar a falha a partir do PDV ou do ERP, entra com a autorização (E.2).
