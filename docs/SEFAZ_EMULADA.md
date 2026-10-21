# SEFAZ emulada

Um servidor local que responde como os autorizadores da NF-e, **só para testes e só em homologação** (`PROJETO.md` 13.5). Ele é a nossa leitura do MOC, não a SEFAZ: não substitui a homologação real (13.3), que é a única que prova a cadeia ICP-Brasil, as cifras e as peculiaridades de cada UF.

Este documento registra o que o emulador implementa: os serviços, as regras com os códigos do MOC e as falhas que dá para comandar. Cada etapa da trilha (`PLANO.md`, "Trilha paralela") acrescenta a sua parte.

Fontes: MOC 7.0, Visão Geral (3.2, 4.2.2, 4.3.1 a 4.3.6, 4.4.1 e 5.1 a 5.11), Anexo I (4.1, grupos A, B, D, DA, DB, E, F e K; 4.2.1, os grupos A, B e ZX, o 1C17, o 2B08 da duplicidade, o 3B08 e o 5E17; 4.4.1 e 4.4.2, os textos dos motivos) e Anexo III (2.1.3, a SVC); a NT 2020.005 v1.21 (a numeração inutilizada na autorização), a NT 2023.002 v1.01 (o emitente CPF na NFC-e), a NT 2024.001 v1.20 (o fim da denegação), a NT 2025.001 v1.02 (QR Code versão 3, o 452 e o prazo de 7 dias), a NT 2026.001 v1.02b (as séries do PAA), a NT 2026.002 v1.11 (emissão offline e DANFE Simplificado Tipo 2), a NT 2014.002 v1.40 (a distribuição DF-e) e a NT 2020.001 v1.60 (a manifestação do destinatário); o Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0 (o hash e a assinatura do QR Code); os XSDs do pacote de liberação embutido no leiaute (`docs/NTS.md`) e a "Relação de Serviços Web" do Portal Nacional, conferidos de 2026-10-19 a 2026-10-21. Quando uma NT muda uma regra do MOC, vale a NT.

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
| `/_controle/notas` | As notas autorizadas: `GET` lista, na ordem (`chave`, `numeracao`, `protocolo`, `recibo`, `recebimento`, `autorizador`, `situacao`, que é o 100 ou o 150 da autorização, e `cancelada`); `DELETE` esquece as notas, os lotes, os eventos, as faixas inutilizadas e a distribuição, com o consumo de cada interessado (204). Só do próprio computador, como as falhas. |
| `/_controle/distribuicao` | `GET`: o que o AN distribui a cada interessado, em ordem: `[ { "interessado": "11222333000181", "documentos": [ { "nsu": 1, "chave": "...", "schema": "resNFe_v1.01.xsd", "recebimento": "..." } ] } ]`. Só do próprio computador. |
| `/_controle/cadastros` | Os contribuintes da consulta cadastro (abaixo, em "CadConsultaCadastro4"). Só do próprio computador. |
| `/_controle/csc` | Os CSCs da NFC-e (abaixo, em "CSC"). Só do próprio computador. |
| `/_controle/irregulares` | Os CNPJs e CPFs em situação irregular (abaixo, em "Irregulares"). Só do próprio computador. |

### Autorizadores

A tabela é do próprio emulador, sem o `Atendimento` da lib: se ele reaproveitasse o que a lib declara, um erro da lib apareceria igual dos dois lados e passaria. O `cUF` da coluna "casa" vai no retorno quando o pedido não chega a dizer qual é a UF (as rejeições antes da leitura).

| Destino | UFs atendidas | Casa |
|---|---|---|
| AM, BA, GO, MG, MS, MT, PE, PR, RS, SP | a própria | a própria |
| SVAN | MA | 91 |
| SVRS | AC, AL, AP, CE, DF, ES, PA, PB, PI, RJ, RN, RO, RR, SC, SE, TO | 43 |
| SVC-AN | AC, AL, AP, CE, DF, ES, MG, PA, PB, PI, RJ, RN, RO, RR, RS, SC, SE, SP, TO | 91 |
| SVC-RS | AM, BA, GO, MA, MS, MT, PE, PR (sem o `NFeInutilizacao4`) | 43 |
| AN | nenhuma: só `NFeDistribuicaoDFe` e `NFeRecepcaoEvento4` (a manifestação) | 91 |

O `CadConsultaCadastro4` está nas 10 UFs (o da própria UF) e na SVRS, que consulta só o cadastro do AC, do ES, do RN, da PB e de SC ("Relação de Serviços Web", conferida em 2026-10-21); a SVAN e as duas SVC não o têm. O emulador responde só os serviços que já implementa. Os outros dão 404, mesmo os que o autorizador tem. A SVC-AN tem o `NFeInutilizacao4` na lista do Portal, mas a SVC não oferece a inutilização (Anexo III, 2.1.3, e): no emulador, 404 nas duas SVC. A EPEC, no `NFeRecepcaoEvento4` do AN, não é implementada (491).

### Envelope SOAP 1.2

O emulador lê e escreve o envelope à mão. A área de dados vem dentro do `nfeDadosMsg`, no namespace do WSDL do serviço (`http://www.portalfiscal.inf.br/nfe/wsdl/<serviço>`), e o retorno vai dentro do `nfeResultMsg`, no mesmo namespace. O espaço em branco da área de dados é guardado como veio, porque faz parte do que a nota assina; para o schema e a assinatura, a área de dados (e cada nota) é tirada do envelope sozinha, sem as declarações de namespace dele, que o C14N levaria para dentro do que é assinado. O `nfeCabecMsg` saiu no leiaute 4.00 e, se vier no `Header`, é ignorado. A resposta sai em `application/soap+xml; charset=utf-8`, em UTF-8 e sem BOM.

O `NFeDistribuicaoDFe` é diferente (NT 2014.002, 5 e 6): o `nfeDadosMsg` vem dentro da operação, `nfeDistDFeInteresse`, no namespace `http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe`, e o retorno vai em `nfeDistDFeInteresseResponse/nfeDistDFeInteresseResult`. O `nfeDadosMsg` direto no `Body` é falha `soap:Sender`. Os exemplos da NT estão em SOAP 1.1; o emulador, como nos outros serviços, só aceita o 1.2.

| Pedido | Resposta |
|---|---|
| Destino ou serviço que o emulador não atende | HTTP 404, sem corpo |
| `Content-Type` que não é `application/soap+xml` | HTTP 415, sem corpo |
| Envelope SOAP 1.1 | HTTP 500, falha `soap:VersionMismatch` |
| XML mal formado, com DTD, fora de um `Envelope` SOAP 1.2, sem `Body` ou sem `nfeDadosMsg` no namespace do serviço | HTTP 400, falha `soap:Sender` |
| Corpo inteiro acima de 500 KB (B01) | HTTP 200, `cStat` 214 |
| `nfeDadosMsg` vazio | HTTP 200, `cStat` 516 (565 na autorização) |
| Na distribuição, corpo inteiro acima de 10 KB (NT 2014.002, 3.9, B01) | HTTP 200, `cStat` 214 |

A ligação HTTP das falhas segue o SOAP 1.2, parte 2, 7.5.1: `Sender` é 400; os outros são 500.

## Serviços

Em todo retorno, `tpAmb` é 2 e `verAplic` é a sigla do destino seguida de `-EMULADA` (FR04: começa pela sigla da UF, ou por SVAN ou SVRS). O `dhRecbto` sai no horário de Brasília (`-03:00`), e o `xMotivo` traz o texto do Anexo I, 4.4.1, sem a referência à NT, com o travessão e as aspas curvas do MOC trocados pelo hífen e pelas aspas retas (516, 517, 565, 568, 572, 903): o `TString` dos schemas de retorno só aceita caracteres até o `ÿ`, e o retorno passa no schema (conferido nos testes do evento, da inutilização e da consulta).

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

### NFeAutorizacao4 (E.2, E.3)

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
| GAP03a-3 | 452 | `indSinc` = 0 com uma só nota (NT 2025.001 v1.02, em produção desde 13/10/2025): a nota sozinha vai na resposta síncrona. Vale para a NF-e e a NFC-e. |
| 4.2.1, B | 765 | NF-e e NFC-e no mesmo lote. |
| B03, B04 | 108, 109 | O serviço está paralisado (comandado). |

Passando o lote, cada nota recebe o seu `protNFe`:

- **síncrona** (`indSinc` = 1, uma nota): `cStat` 104 ("Lote processado") com o `protNFe`;
- **assíncrona** (`indSinc` = 0, de 2 a 50 notas): `cStat` 103 ("Lote recebido com sucesso") com o `infRec` (`nRec` e `tMed` = 1); os protocolos ficam com o lote, para o `NFeRetAutorizacao4`. O emulador processa o lote na hora; a fila só aparece quando comandada (`loteProcessando`).

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
| B22-10 (NT 2026.002) | 711 | NF-e com `tpEmis` 9 sem o DANFE Simplificado Tipo 2 (`tpImp` 6). |
| 4.2.1, B | 570 | `tpEmis` 3, 6 ou 7 num autorizador que não é a SVC. |
| 4.2.1, B | 783 | NFC-e na SVC. |
| 4.2.1, B | 713 | Na SVC, `tpEmis` diferente de 6 (SVC-AN) ou de 7 (SVC-RS). |
| 4.2.1, B | 703 | `dhEmi` mais de 5 minutos depois do recebimento. |
| B09-40 (NT 2026.002) | 704 | NFC-e, ou NF-e com `tpImp` 6, com `tpEmis` 1, 3, 6 ou 7 emitida há mais de 5 minutos. A de contingência (4 e 9) não é rejeitada: transmitida depois de 24 horas, é autorizada fora de prazo (150). |
| B09-20 (NT 2025.001, NT 2026.002) | 228 | A outra NF-e, com `tpEmis` 1, 3, 6 ou 7, emitida há mais de 30 dias (a exceção 01, a critério da UF, que o emulador aplica). Com mais de 7 dias, a NF-e é autorizada fora de prazo (150); até 7 dias, 100. |
| ZX | 393 a 903 | O QR Code (abaixo). |
| 1C17-38 (NT 2024.001) | 781 | O emitente irregular: o CNPJ ou o CPF em "Irregulares", ou a marca `EMULADA:irregular:emitente` na própria nota. Na NF-e e na NFC-e. |
| 2B08-10 | 539 | A mesma numeração (modelo, UF, CNPJ ou CPF do emitente, série e número; na NFC-e, também o `tpEmis`) já autorizada com outra chave. O motivo leva `[chNFe:<a chave autorizada>]` e, se ela veio num lote assíncrono, `[nRec:<o recibo dele>]`. |
| 2B08-20 | 204 | A mesma chave já autorizada. O motivo leva `[nRec:...]` como no 539. O emulador não devolve o protocolo da nota igual (a observação 2, a critério da UF). |
| 2B08-30 | 218 | A mesma chave já autorizada e cancelada. O motivo leva `[nRec:...]` como no 539. |
| 3B08-100 (NT 2020.005) | 206 | A numeração inutilizada (modelo, UF, CNPJ ou CPF, série e número), na emissão normal e na SVC (`tpEmis` 1, 6 e 7); em SP, também na FS-IA, na EPEC e na FS-DA (2, 4 e 5). A NFC-e offline (9) não é conferida. |
| 5E17-40 (NT 2024.001) | 302 | Na NF-e, o destinatário (o CNPJ ou o CPF) irregular, em "Irregulares" ou pela marca `EMULADA:irregular:destinatario`. Rejeição, e não mais denegação. |
| | 100, 150 | Autorizada (150: fora de prazo, pela data de emissão acima): o `nProt` e a nota registrada. |

O `protNFe` 4.00 (Tabela 5-6) sai sem o `Id` e sem a `Signature`, que são a critério da UF: `tpAmb`, `verAplic`, `chNFe`, `dhRecbto`, `nProt` (só no 100 e no 150), `digVal` (o `DigestValue` da assinatura da nota, quando há), `cStat` e `xMotivo`. A nota rejeitada não é registrada e pode ser corrigida e enviada de novo. A duplicidade vale entre autorizadores: a base é uma só, como a do país.

**Sem denegação** (NT 2024.001 v1.20, em produção desde 16/09/2024, pelo Ajuste SINIEF 43/23; a NT 2023.002 já a tirara da NFC-e): a regra do 301 (1C17-40) foi excluída, o emitente irregular é o 781 e o destinatário irregular, o 302 e o 303, agora rejeições. Nenhuma nota nova sai com 110, e o 205 da duplicidade (a nota denegada) não aparece. O 303 (5E17-60, o destinatário vedado na UF) não é aplicado: o emulador não separa a UF do cadastro.

Ainda não: a autorização com alerta (120, NT 2026.002), que só sai de regras de cadastro do destinatário; a NFC-e offline recusada pela UF (712, facultativa); as regras de conteúdo da nota que a lib já confere antes (`PROJETO.md` 6.2), como os totais, a justificativa da contingência (556, 557) e as da NF-e com `tpImp` 6 (705, 706, 707, 715).

### QR Code (E.3)

Na NFC-e e na NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6), o `infNFeSupl` (`qrCode` e `urlChave`), pelas regras do grupo ZX (Anexo I, com a NT 2025.001 e a NT 2026.002) e pelo cálculo do Manual do DANFE NFC-e e QR Code 6.0, em código próprio. Elas vêm depois da data de emissão e antes da duplicidade. O schema já confere a forma de cada versão: a 100 (os parâmetros nomeados), a 2 e a 3, online ou offline pelo `tpEmis` da chave; com isso, o parâmetro inexistente ou fora do formato (396, 400) e a assinatura da versão 3 que sobra na online ou falta na offline (445, 474) são 225.

| Regra | cStat | Motivo |
|---|---|---|
| ZX01-10 | 393 | NF-e sem `tpImp` 6 com o `infNFeSupl`. |
| ZX02-10 | 394 | NFC-e, ou NF-e com `tpImp` 6, sem o `infNFeSupl`. |
| ZX02-15 | 903 | NFC-e com o QR Code na versão 100. |
| ZX02-220 (NT 2026.002) | 672 | NF-e com `tpImp` 6 e o QR Code em outra versão que não a 3. |
| ZX02-222 (NT 2025.001) | 444 | Versão 2 do emitente CPF. |
| ZX02-228, ZX02-244 | 397 | A chave (`[Param: chNFe]`) ou o ambiente (`[Param: tpAmb]`) do QR Code diferem da nota. |
| ZX02-268, ZX02-276 | 397 | Na offline (`tpEmis` 9): o dia da emissão (`[Param: dhEmi]`) ou o valor total (`[Param: vNF]`, comparado como número). |
| (versão 2) | 397 | Na offline, o DigestValue em hexadecimal (`[Param: digVal]`) não é o da assinatura da nota. |
| (versão 2) | 462, 463 | O identificador do CSC não está cadastrado para a UF e a raiz do CNPJ do emitente, ou foi revogado antes do `dhEmi` (abaixo, em "CSC"). |
| (versão 2) | 464 | O hash não confere: o SHA-1, em hexadecimal, dos parâmetros separados por barra vertical (sem o hash) com o CSC no fim (Manual, 4.3.4 e 4.3.5). Sem distinguir maiúsculas. |
| ZX02-338 (NT 2025.001) | 397 | Na versão 3 offline, o tipo e o documento do destinatário (6º e 7º parâmetros: 1 e o CNPJ, 2 e o CPF, 3 e vazio no estrangeiro, os dois vazios sem destinatário) diferem da nota (`[Param: idDest]`). |
| ZX02-338 (NT 2025.001) | 583 | Na versão 3 offline, a assinatura não confere: RSA-SHA1, em Base64, dos parâmetros 1 a 7 separados por barra vertical, com a chave pública do certificado do `KeyInfo` da nota (Manual, 4.4.2). |

O `[Param: ...]` vai depois do texto do motivo; o nome do parâmetro é o da versão 100 (o Manual não nomeia os das versões 2 e 3). A versão 3 online não tem CSC nem assinatura. A URL do site da UF (395) e a de consulta pela chave (878) **não** são conferidas: o schema confere a forma (`http(s)://...?p=`), e a tabela de cada UF é da lib (5.1). O 407 (a UF que não aceita a versão 3) saiu na NT 2025.001 v1.01.

### CSC (E.3)

O CSC é por UF e raiz do CNPJ do emitente (Manual, 4.6). O emulador tem **um CSC padrão**, que vale para qualquer emitente sem cadastro: o identificador `1` e o valor `OPENDFE-SEFAZ-EMULADA-CSC-PADRAO`. A rota `/_controle/csc`, só do próprio computador, cadastra outros e revoga:

| Rota | O que faz |
|---|---|
| `POST /_controle/csc` | Cadastra, em JSON: `{ "uf": "MG", "cnpj": "11222333", "id": "2", "csc": "..." }`. A UF é a sigla; o CNPJ é a raiz (8 posições, também a alfanumérica); o identificador vai sem zeros à esquerda (até 6 algarismos), como no QR Code; o CSC tem de 16 a 36 caracteres ASCII, sem espaço nem barra vertical. O mesmo identificador cadastrado de novo troca o valor e desfaz a revogação. Devolve 201 com o CSC e o `Location`, ou 400 com `{ "erro": "..." }`. |
| `DELETE /_controle/csc/{uf}/{cnpj}/{id}` | Revoga no instante do pedido, também o padrão: 204, ou 404 se não existe. A nota emitida antes da revogação ainda passa; a emitida depois, 463. |
| `GET /_controle/csc` | Os cadastrados e revogados (`uf`, `cnpj`, `id`, `csc`, `revogacao`), sem o padrão que nunca foi mexido. |
| `DELETE /_controle/csc` | Esquece os cadastros e as revogações: fica só o padrão (204). |

### NFeConsultaProtocolo4 (E.3)

Pedido `consSitNFe` 4.00 (`tpAmb`, `xServ` = `CONSULTAR`, `chNFe`), retorno `retConsSitNFe` 4.00 na ordem da Tabela 5-14, com o `cUF` da casa do autorizador. O schema é o `consSitNFe_v4.00.xsd` do leiaute. As regras, na ordem (Visão Geral, Tabela 5-16):

| Regra | cStat | Motivo |
|---|---|---|
| D01b, D02, D01e, B06 | 516, 404, 517, 239 | A raiz, como no status. |
| D01 | 215 | O pedido não passa no schema. |
| J01 | 252 | `tpAmb` = 1. |
| J02 | 226 | O autorizador não atende a UF da chave (também a que não existe: o J02b, 614, não chega a aparecer). |
| J02a | 236 | O DV da chave não confere. |
| J02c, J02d | 615, 616 | O ano da chave menor que 06 ou maior que o atual; o mês fora de 1 a 12. |
| J02e (NT 2018.001, NT 2026.001) | 617 | Das séries 0 a 909 e 980 a 989 (as do PAA com CNPJ), o CNPJ da chave inválido ou zerado; das outras, o CPF (depois de `000`). |
| J02f, J02g | 618, 619 | O modelo diferente de 55 e 65; o número zero. |
| B03, B04 | 108, 109 | O serviço está paralisado (comandado). |
| J03 | 217 | Nenhuma nota com a numeração da chave (a da duplicidade: modelo, UF, emitente, série, número e, na NFC-e, o `tpEmis`). Na SVC, só contam as notas que ela autorizou. |
| J04 | 562 | A nota da base tem outro `cNF`. O motivo leva `[chNFe:<a chave da base>]` quando o transmissor (o certificado do TLS) é o emitente: a raiz do CNPJ ou o CPF. |
| J05 | 561 | A nota da base tem outro mês na chave. |
| J06 | 613 | A chave difere da da base em outra posição (na NF-e, o `tpEmis`). |
| | 100, 150, 101 | Achada: a situação da autorização (101 depois do cancelamento, pelo 110111 ou pelo 110112), o `protNFe` que a autorização devolveu e um `procEventoNFe` por evento registrado, na ordem do registro (5.4.3: o cancelamento e a CC-e). |

O `chNFe` do retorno é o do pedido; quando não dá para lê-lo (as rejeições até o 215), vai com zeros. O J02k (526, a chave com mais de 6 meses) é a critério da UF e não é aplicado. O `retCancNFe` (o cancelamento de antes dos eventos) não sai, nem o 151 (o cancelamento fora de prazo, que o emulador não aceita: 501).

**Divergência entre os schemas do Portal:** o `procEventoNFe` vai como foi registrado, e o do cancelamento por substituição traz o `cOrgaoAutor` no `retEvento` (R22), que o `retEnvEvento_v1.00` e o `procEventoNFe_v1.00` preveem; o `retConsSitNFe_v4.00` do PL_010d tem o seu próprio `TProcEvento`, sem ele. Quem valida o retorno da consulta pelo schema acha essa falha só nesse caso.

**A consulta na SVC** (Anexo III, 2.1.3.4, f e h): a SVC só acha as notas que ela mesma autorizou; o autorizador normal acha também as da SVC, que chegam a ele pelo Ambiente Nacional. No emulador, a nota da SVC já está no normal na hora.

### Contingência SVC (E.3)

Em homologação, a SVC fica sempre ativa para todas as UFs (Anexo III, 2.1.3.2): o emulador não tem o 113 nem o 114 (a SVC desabilitada), e a SVC-AN e a SVC-RS respondem 107 no status e autorizam com o `tpEmis` delas (6 e 7) também com o autorizador normal paralisado. O caso de teste é o do Anexo III, 2.1.3.5: a paralisação comandada em MG (108 ou 109 no status e na autorização), o status da SVC-AN com 107 para MG, a nota gerada de novo com o `tpEmis` 6 e autorizada na SVC-AN, e a consulta na SVC-AN e, depois, em MG.

### NFeInutilizacao4 (E.4)

Pedido `inutNFe` 4.00 (`infInut` com `tpAmb`, `xServ` = `INUTILIZAR`, `cUF`, `ano`, `CNPJ`, `mod`, `serie`, `nNFIni`, `nNFFin` e `xJust`, assinado), retorno `retInutNFe` 4.00 na ordem da Tabela 5-10, sem o `Id` (a resposta não é assinada) e com o `cUF` da casa do autorizador. O schema é o `inutNFe_v4.00.xsd` do leiaute. A SVC não oferece o serviço (acima, em "Autorizadores"). As regras, na ordem (Visão Geral, Tabelas 5-11 e 5-12):

| Regra | cStat | Motivo |
|---|---|---|
| D01b, D02, D01e, B06 | 516, 404, 517, 239 | A raiz, como no status. |
| D01 | 215 | O pedido não passa no schema. |
| I05 | 203 | O CNPJ com o DV errado: a Tabela 5-12 não tem regra própria, e o CNPJ que não existe não está no cadastro. |
| E, F | 290, 292, 297, 298, 213 | A assinatura e o titular contra o CNPJ do pedido (a raiz: a matriz inutiliza pela filial), pela verificação da lib. |
| I01 | 252 | `tpAmb` = 1. |
| I02 | 250 | O autorizador não atende o `cUF`. |
| I02a, I02d (NT 2026.001) | 266 | A série do emitente CPF (910 a 969) ou a reservada ao PAA (970 a 989). |
| I02b, I02c | 453, 454 | O ano depois do atual (horário de Brasília) ou antes de 06. |
| I03 | 224 | O número inicial maior que o final. |
| I04 | 201 | Mais de 10.000 números. |
| I04.a | 502 | O `Id` não é `ID` + `cUF` + `ano` + `CNPJ` + `mod` + a série com 3 posições + o início e o fim com 9. |
| B03, B04 | 108, 109 | O serviço está paralisado (comandado). |
| I06 | 240 | O emitente irregular (em "Irregulares"). |
| I07 | 563 | A mesma faixa (UF, CNPJ, modelo, série, início e fim) já inutilizada: a faixa e o `nProt` da anterior vão no retorno (5.3.5). |
| I07a | 256 | Um número da faixa já inutilizado em outra faixa. |
| I08 | 241 | Um número da faixa já usado por uma nota autorizada (também a cancelada), pela chave. |
| | 102 | Homologada: a faixa, o `dhRecbto` e o `nProt` (formado como o da nota). |

O `ano`, o `CNPJ`, o `mod`, a `serie`, o `nNFIni`, o `nNFFin` e o `nProt` vão só no 102 e no 563; o `dhRecbto`, sempre. O I09 (241 pela EPEC) chega com a EPEC, no AN.

### NFeRecepcaoEvento4 (E.4)

Pedido `envEvento` 1.00 (`idLote` e de 1 a 20 `evento`, cada um assinado no `infEvento`), retorno `retEnvEvento` 1.00 na ordem da Tabela 5-33, com o `cOrgao` do primeiro evento (R06: idem a entrada; antes de ler o lote, a casa do autorizador e o `idLote` 0). O schema é o `envEvento_v1.00.xsd` genérico do leiaute (o do PL_010d, com o CNPJ alfanumérico), que deixa o `detEvento` livre; o detalhe de cada evento é conferido à parte, no schema do tipo (`e110110`, `e110111`, `e110112`).

As regras do lote: a raiz (516, 404, 517, 239), o schema (215; também mais de 20 eventos) e a paralisação (108, 109). Passando, 128 ("Lote de Evento Processado"), e cada evento recebe o seu `retEvento`, na ordem do lote; os eventos do mesmo lote se veem (o cancelamento depois da CC-e da mesma nota).

Os eventos: o **cancelamento** (110111), o **cancelamento por substituição** (110112, da NFC-e) e a **carta de correção** (110110). A SVC registra só o cancelamento, e só das notas que ela autorizou (Anexo III, 2.1.3, c e d); o autorizador normal cancela também as da SVC, que a consulta dele já acha. As regras de cada evento, na ordem:

| Regra | cStat | Motivo |
|---|---|---|
| D04 | 491 | O `tpEvento` não é um dos três (na SVC, não é o 110111). |
| D05 | 492 | O `verEvento` não é 1.00. |
| D06 | 493 | O `detEvento` não passa no schema do tipo (também o `tpAutor` diferente de 1, P21-10, 466, que o schema confere). |
| P10-10, P11-10 | 489, 490 | O CNPJ ou o CPF do autor com o DV errado. |
| E, F | 290, 292, 297, 298, 213, 227 | A assinatura e o titular contra o autor. |
| P07-10 | 572 | O `Id` não é `ID` + `tpEvento` + `chNFe` + `nSeqEvento` com 2 posições. |
| P08-10 | 250 | O `cOrgao` não é a UF da chave, ou o autorizador não a atende. Cobre o 249 (P12-40) e o 614 (P12-14), que viriam depois. |
| P09-10 | 252 | `tpAmb` = 1. |
| P12-10 a P12-34 | 236, 615 a 619 | A chave, como na consulta (o documento pela série, com a NT 2026.001). |
| P12-44 | 574 | O CNPJ ou o CPF do autor não é o da chave (o documento inteiro: a filial cancela a sua nota, com o certificado da matriz). |
| P13-10 | 578 | O `dhEvento` mais de 5 minutos depois do recebimento. |
| 1P10-20 | 240 | O autor irregular (em "Irregulares"). |
| 3P15-10 | 573 | O mesmo evento (`tpEvento`, chave e `nSeqEvento`) já registrado. |

Depois, as do cancelamento e do cancelamento por substituição (Tabela 5-38):

| Regra | cStat | Motivo |
|---|---|---|
| P12-48 | 920 | No 110112, a nota cancelada não é da emissão normal (`tpEmis` 1). |
| P15-10 | 594 | O `nSeqEvento` não é 1. |
| P20-10 | 455 | No 110112, o `cOrgaoAutor` não é a UF da chave. |
| P31-10 a P31-34 | 910 | No 110112, a chave da substituta inválida, com o campo no motivo: `(Dígito)`, `(Código UF)`, `(Ano)`, `(Mês)`, `(CNPJ/CPF)`, `(Modelo)`, `(Número)`. |
| P31-38 a P31-52 | 911 | No 110112, a chave da substituta incorreta para a cancelada: `(mesma Chave de Acesso)`, `(Código da UF)`, `(CNPJ/CPF)`, `(Ano-Mes)` (depois do mês da cancelada ou antes do anterior), `(Modelo)`. |
| 2P12-10 | 494 | A nota não está na base (pela numeração, como a consulta; na SVC, só as dela). Achada com outra chave, o motivo leva `[chNFe:<a chave da base>]`. |
| 2P12-14, 2P12-18 | 501 | Mais de 24 horas (110111) ou de 168 horas (110112) desde a autorização. O 155 (o cancelamento fora de prazo, a critério da UF) não é aplicado. |
| 2P12-22 | 580 | A nota já cancelada. |
| 2P13-10 | 577 | O `dhEvento` antes da emissão. |
| 2P13-14 | 579 | Na nota da emissão normal, o `dhEvento` mais de 5 minutos antes da autorização. |
| 2P23-10 | 222 | O `nProt` não é o da autorização. |
| 4P15-14 | 221 | Na NF-e, a última manifestação conclusiva do destinatário (abaixo) é a confirmação (210200). Depois dela, o desconhecimento ou a operação não realizada deixam cancelar; a ciência não impede. |
| 5P31-10 a 5P31-40 | 912 a 919 | No 110112, a substituta (pela chave): inexistente (912), cancelada (913), emitida mais de 2 horas depois da cancelada (914), com outro valor total (915), outro ICMS (916), outro destinatário (917: o CNPJ, o CPF ou o `idEstrangeiro`, e a IE), outra quantidade de itens (918) ou um item diferente (919: `cProd`, `cEAN`, `xProd`, `NCM`, `CFOP`, `uCom`, `indTot` como texto; `qCom`, `vUnCom`, `vProd` como número). |
| 5P31-44 | 921 | No 110112, a substituta da emissão normal (`tpEmis` 1). |
| | 135 | Registrado e vinculado à nota: a nota passa a cancelada. |

E as da carta de correção (Tabela 5-40): a sequência acima de 20 (GA03, 594), a NFC-e (GA03a, 784), a nota que não está na base (494, como acima), a nota cancelada (GA01, 580) e o `dhEvento` antes da emissão (577); passando, 135. A carta nova substitui a anterior (5.10), e a consulta devolve todas, na ordem.

O `retEvento` (Tabela 5-33, R10 a R31) sai sem o `Id` e sem a assinatura (a critério da UF): `tpAmb`, `verAplic`, `cOrgao` (o do evento), `cStat`, `xMotivo`, `chNFe`, `tpEvento`, `xEvento` (só no 135: "Cancelamento homologado" ou "Carta de Correção registrada"), `nSeqEvento`, o `cOrgaoAutor` (só no 110112 registrado), o `CNPJDest` ou o `CPFDest` e o `emailDest` da nota (só no 110111 registrado, quando a nota os tem), `dhRegEvento` (o recebimento, também na rejeição) e `nProt` (só no 135, formado como o da nota, com o cUF da chave). O evento registrado guarda o `procEventoNFe` (o `evento` como chegou e o `retEvento`), que a consulta devolve.

Não aplicadas: o 408 (P11-20, o autor CPF na NFC-e), porque a NT 2023.002 permite a NFC-e do emitente CPF e o seu cancelamento; o 203 e o 240 do cadastro além do autor irregular; o 690, o 219, o 304, o 939, o 940 e o 811, que dependem de eventos de outros autores (o CT-e, o MDF-e, a passagem, a SUFRAMA, a averbação, os financeiros, a prorrogação), que o emulador não registra.

### Manifestação do destinatário (E.5)

No `NFeRecepcaoEvento4` do **AN**, pela NT 2020.001 v1.60 (que juntou a 2012.002 e a 2013.001; MOC 7.0, Visão Geral, 3.2 e 5.11): a **confirmação da operação** (210200), a **ciência da emissão** (210210), o **desconhecimento da operação** (210220) e a **operação não realizada** (210240, com o `xJust`). O autor é o destinatário da nota, pelo CNPJ ou pelo CPF, e a assinatura é de um certificado com a raiz do CNPJ ou o CPF dele: a filial manifesta com o certificado da matriz. O lote e o retorno são os mesmos dos outros eventos (acima), com o `cOrgao` 91. Os autorizadores das UFs não recebem a manifestação (491), e o AN só recebe ela (491 para o cancelamento e a CC-e; a EPEC também não está implementada).

Valem as regras da parte geral acima, com três diferenças: o `cOrgao` tem de ser 91 (250), o autor não é comparado ao emitente da chave (sem o 574) e o autor irregular não é recusado (sem o 240, que a NT não lista). Depois, na ordem:

| Regra | cStat | Motivo |
|---|---|---|
| | 494 | A nota não está na base, pela chave. |
| 2P21-10 | 575 | O CNPJ ou o CPF do autor não é o do destinatário da nota (o documento inteiro), ou a nota não tem destinatário. |
| 2P13-10 | 577 | O `dhEvento` antes da emissão. |
| 2P13-14 | 579 | Na nota da emissão normal, o `dhEvento` mais de 5 minutos antes da autorização. |
| H02 | 594 | A ciência com o `nSeqEvento` diferente de 1, ou a manifestação conclusiva acima de 2 (o Ajuste SINIEF 43/23 permite retificar uma vez). |
| H03 | 596 | Fora do prazo, contado da autorização: 10 dias na ciência e 90 nas conclusivas (Ajuste SINIEF 14/26, que mudou de 180 para 90). O motivo leva o prazo: `Rejeição: Evento apresentado fora do prazo: 90 dias`. |
| H04 | 650 | A ciência da nota cancelada. |
| H05 | 651 | O desconhecimento da nota cancelada. A confirmação e a operação não realizada da nota cancelada são registradas. |
| H06 | 655 | A ciência depois de uma manifestação conclusiva. |
| H08 | 496 | A chave com o `tpEmis` fora de 1 a 7 (o 9 da NF-e offline). |
| | 135 | Registrada e vinculada à nota. |

O H01 (595, a operação não realizada sem justificativa) não sai: o `e210240` exige o `xJust`, e a falta dele é 493. O H07 (658, a UF do destinatário contra a do Web Service) não vale no AN.

O `retEvento` do 135 leva o `xEvento` da NT (`Confirmacao de Operacao registrada`, `Ciencia da Operacao registrada`, `Desconhecimento da Operacao registrada`, `Operacao nao Realizada registrada`), sem o `CNPJDest`, o `CPFDest` e o `emailDest`, e o `nProt` com o tipo de autorizador 2 e o 91 no lugar da UF (`291` + ano + sequência). A manifestação vale para o cancelamento (o 221 acima, só da confirmação, valendo a última conclusiva), para a consulta da nota (o `procEventoNFe` dela vem com os outros) e para a distribuição (abaixo).

### NFeDistribuicaoDFe (E.5)

No **AN**, pela NT 2014.002 v1.40 (MOC 7.0, Visão Geral, 5.7). Pedido `distDFeInt` 1.01 (`tpAmb`, `cUFAutor` opcional, o `CNPJ` ou o `CPF` do interessado e uma das três consultas), retorno `retDistDFeInt` 1.01 na ordem da Tabela 5-27: `tpAmb`, `verAplic`, `cStat`, `xMotivo`, `dhResp`, `ultNSU`, `maxNSU` (os dois obrigatórios no schema) e o `loteDistDFeInt` com até 50 `docZip`. Cada `docZip` é um documento em UTF-8, compactado em gzip e em base64, com o `schema` (`resNFe_v1.01.xsd`, `procNFe_v4.00.xsd`, `resEvento_v1.01.xsd` ou `procEventoNFe_v1.00.xsd`) e o `NSU`.

**O que cada um recebe**, só da NF-e (a NFC-e não é distribuída), pela tabela do item 3 da NT, cada interessado com a sua sequência de NSU, sem lacunas, na ordem em que o AN recebeu (cada estabelecimento, pelas 14 posições do CNPJ, tem a sua):

| Ator | Recebe |
|---|---|
| Destinatário (`dest`) | Na autorização, o `resNFe`. O cancelamento sempre: como `resEvento` antes da manifestação e como `procEventoNFe` depois. A ciência, a confirmação ou a operação não realizada (o desconhecimento não) liberam o download: o `nfeProc` (a nota com o `protNFe`) e as cartas de correção já registradas, como `procEventoNFe`; as seguintes chegam do mesmo jeito. |
| Transportador (`transporta`) e autorizados (`autXML`) | Na autorização, o `nfeProc`; o `procEventoNFe` do cancelamento e da carta de correção. Os autorizados, também o da manifestação. O que também é o destinatário da nota recebe só como destinatário. |
| Emitente | O `procEventoNFe` da manifestação do destinatário. A própria nota, não. |

O `resNFe` (Tabela 5-30) leva a chave, o CNPJ ou o CPF, o nome e a IE do emitente (`ISENTO` sem a IE, que o schema exige), o `dhEmi`, o `tpNF`, o `vNF`, o `digVal` e o `dhRecbto` e o `nProt` da autorização, e o `cSitNFe` 1. O `resEvento` (Tabela 5-31) leva o `cOrgao`, o autor, a chave, o `dhEvento`, o tipo, a sequência, o `xEvento`, o registro e o protocolo.

As regras do pedido, na ordem: a raiz (516, 404, 517, 239), o schema (215), o ambiente (H01, 252), o CNPJ (H02, 489) ou o CPF (H03, 490) do interessado, a raiz do CNPJ do certificado do TLS contra a do CNPJ consultado (H04, 593: a matriz consulta as filiais) ou o CPF dele contra o CPF consultado (H05, 472), a paralisação (108, 109) e o bloqueio do uso indevido (656, abaixo). Depois, por consulta:

| Consulta | Regras e resposta |
|---|---|
| `distNSU` (`ultNSU`) | O NSU acima do maior do interessado (H06, 589). O uso indevido (abaixo). Os documentos com NSU acima do `ultNSU`, até 50, recebidos há menos de 90 dias: 138 "Documento(s) localizado(s)", com o `ultNSU` do último do lote e o `maxNSU`; nenhum: 137 "Nenhum documento localizado", com o `ultNSU` e o `maxNSU` iguais ao maior. |
| `consNSU` (`NSU`) | O limite das consultas pontuais (656, abaixo), o NSU acima do maior (589) e o documento daquele NSU, se ainda guardado: 138 "Documento localizado" com ele, ou 137. O `ultNSU` é o NSU pedido. |
| `consChNFe` (`chNFe`) | O limite das consultas pontuais; a chave (H07 a H13: 236, 614, 615, 616, 617, 618, aqui só o modelo 55, com o texto da NT, e 619); a nota na base (H14, 217, "NF-e inexistente para a chave de acesso informada"); o prazo de 90 dias da autorização (H15, 632); o emitente (H17, 641, conferido antes do H16, que senão o cobriria); o interessado que não é o destinatário, o transportador nem um autorizado, pela raiz do CNPJ ou pelo CPF (H16, 640); a nota cancelada (H18, 653). Achada, 138 com a nota: o `nfeProc` ou, ao destinatário que ainda não liberou o download, o `resNFe`; sem os eventos, sem o `NSU` no `docZip` (facultativo desde a v1.15) e com o `ultNSU` zerado. |

**O uso indevido** (3.11.4), por CNPJ (as 14 posições) ou CPF da requisição, com o bloqueio de uma hora; quem consulta no bloqueio recomeça a hora e recebe o 656 de novo (no `distNSU`, com o `ultNSU` do último devolvido):

| Situação | `xMotivo` |
|---|---|
| Um `distNSU` em menos de uma hora depois do 137 | `Rejeição: Consumo Indevido. Deve ser aguardado 1 hora para efetuar nova solicitação caso não existam mais documentos a serem pesquisados. Tente após 1 hora` |
| Um `distNSU` com outro `ultNSU` que não o último devolvido (também o zero, recomeçando) | `Rejeição: Consumo Indevido. Deve ser utilizado o ultNSU nas solicitações subsequentes. Tente após 1 hora` |
| A 21ª consulta pontual (`consNSU` ou `consChNFe`) na última hora | `Rejeição: Consumo Indevido - ultrapassou o limite de 20 consultas por hora` |

Não aplicados: o 654 (a nota denegada, que não existe mais desde a NT 2024.001) e a geração do NSU só para quem usou o serviço nos últimos 60 dias (3.4, a partir da v1.10): no emulador, todo interessado ganha o NSU desde a primeira nota. O "Ator Interessado" (110150), a EPEC e os eventos do fisco, da SUFRAMA, do CT-e e do MDF-e não são distribuídos, porque o emulador não os registra.

### CadConsultaCadastro4 (E.5)

Pelo MOC 7.0, Visão Geral, 5.6. Pedido `ConsCad` 2.00 (`xServ` `CONS-CAD`, a `UF` e a `IE`, o `CNPJ` ou o `CPF`; sem o `tpAmb`), retorno `retConsCad` 2.00 na ordem da Tabela 5-22: o `infCons` com o `verAplic`, o `cStat`, o `xMotivo`, a `UF` e o argumento como vieram, o `dhCons` (no horário de Brasília) e o `cUF` da casa do autorizador, e um `infCad` por contribuinte achado. O schema é o `consCad_v2.00.xsd` do leiaute, da subpasta `CadConsultaCadastro/` (`docs/NTS.md`). Na rejeição antes de ler o pedido, a UF é a da casa e o argumento vai como um CNPJ zerado (o schema do retorno exige um).

As regras, na ordem: a raiz (516, 404, 517, 239), o schema (215), a UF da consulta que não é uma das do Web Service (K01, 265; também o `SU` da SUFRAMA), a paralisação (108, 109), o solicitante (K02, 257: o transmissor, pelo certificado do TLS, em situação irregular, em "Irregulares") e, pelo argumento: o CNPJ com o DV errado (K03, 258) ou que não está no cadastro da UF (K04, 259); a IE que não tem de 2 a 14 algarismos, toda zero ou `ISENTO` (K05, 260; o DV, que cada UF calcula do seu jeito, não é conferido) ou que não está no cadastro (K06, 261); o CPF com o DV errado ou com menos de 11 algarismos (K08, 263) ou que não está no cadastro (K09, 264). Achado, 111 "Consulta cadastro com uma ocorrência" ou 112 "Consulta cadastro com mais de uma ocorrência": pelo CNPJ ou pelo CPF, cada IE dele na UF; pela IE, o contribuinte dela e os que a têm como IE única. O K07 (262, a UF que não consulta pelo CPF) não sai: todas consultam.

O `infCad` (GR07 a GR29) leva a IE, o CNPJ ou o CPF, a UF, o `cSit` (1 habilitado; 0 no contribuinte cadastrado como não habilitado e no CNPJ ou CPF irregular, como a autorização o trata), o `indCredNFe` e o `indCredCTe`, o `xNome` e, quando cadastrados, o `xFant`, o `xRegApur`, o `CNAE`, as datas (`dIniAtiv`, `dUltSit`, `dBaixa`), a `IEUnica`, a `IEAtual` e o `ender`.

O cadastro, pela rota de controle, um contribuinte por UF e IE:

| Rota | O que faz |
|---|---|
| `POST /_controle/cadastros` | Cadastra, em JSON, ou troca o da mesma UF e IE. Devolve 201 com o contribuinte e o `Location` (`/_controle/cadastros/MG/0623079040081`), 200 quando trocou, ou 400 com `{ "erro": "..." }`. |
| `GET /_controle/cadastros` | Os contribuintes, por UF e IE. |
| `DELETE /_controle/cadastros/{uf}/{ie}` | Tira um: 204, ou 404 se não está. |
| `DELETE /_controle/cadastros` | Tira todos: 204. |

```json
{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "CONTRIBUINTE DE TESTE LTDA",
  "fantasia": "TESTE", "regimeApuracao": "NORMAL", "cnae": "4751201", "inicioAtividade": "2010-03-01",
  "ultimaSituacao": null, "baixa": null, "ieUnica": null, "ieAtual": null,
  "endereco": { "logradouro": "RUA DE TESTE", "numero": "100", "complemento": null, "bairro": "CENTRO",
                "codigoMunicipio": "3106200", "municipio": "BELO HORIZONTE", "cep": "30110000" },
  "habilitado": true, "credenciadoNFe": "1", "credenciadoCTe": "4" }
```

Obrigatórios: a `uf` (a sigla, sem o `SU`), a `ie` (de 2 a 14 algarismos, não toda zero), o `cnpj` ou o `cpf` (um dos dois, com o DV certo) e o `nome` (até 60). Os outros são opcionais; o `habilitado` é `true`, o `credenciadoNFe` é 1 e o `credenciadoCTe` é 4 (a SEFAZ não fornece) quando omitidos.

### Irregulares (E.4)

O pedaço do cadastro da SEFAZ que as regras consultam: os CNPJs (também os alfanuméricos) e os CPFs em situação irregular, pelo documento inteiro (o estabelecimento, e não a raiz) e em qualquer UF. Como emitente, rejeitam a autorização com 781, o evento com 240 e a inutilização com 240; como destinatário da NF-e, a autorização com 302.

| Rota | O que faz |
|---|---|
| `POST /_controle/irregulares` | Cadastra, em JSON: `{ "documento": "11222333000181" }`, sem máscara, com o DV conferido. Devolve 201 com o documento e o `Location`, ou 400 com `{ "erro": "..." }`. |
| `GET /_controle/irregulares` | Os documentos, em ordem. |
| `DELETE /_controle/irregulares/{documento}` | Tira um: 204, ou 404 se não está. |
| `DELETE /_controle/irregulares` | Tira todos: 204. |

A marca `EMULADA:irregular:emitente` ou `EMULADA:irregular:destinatario` no `infCpl` faz o mesmo só para a nota que a traz (abaixo).

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

Em memória, vazio a cada partida (ou depois do `DELETE /_controle/notas`): as notas autorizadas (a chave, a numeração da duplicidade, o protocolo, o recibo do lote, o recebimento, o autorizador, a situação, o `protNFe`, para a consulta, a nota como chegou, para os eventos, e se está cancelada), os lotes assíncronos (o recibo, o transmissor, os protocolos e as consultas que ainda respondem 105), os eventos registrados (o `procEventoNFe` de cada um) e as faixas inutilizadas (com o protocolo). Um lote é processado de uma vez, sem outro no meio: as notas do mesmo lote e de lotes simultâneos se veem na ordem. A distribuição (os documentos de cada interessado, com o NSU, e o consumo de cada CNPJ ou CPF da requisição) fica com as notas e é esquecida com elas. Os CSCs cadastrados e revogados, os irregulares e os contribuintes da consulta cadastro ficam à parte, também em memória (`DELETE /_controle/csc`, `DELETE /_controle/irregulares`, `DELETE /_controle/cadastros`).

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
| `paralisacao` | O `cStat` 108 ou 109 no lugar do 107, depois das rejeições do próprio pedido. Na autorização, no recibo e na consulta, também depois das rejeições do pedido, antes das notas, do lote e da base. |
| `loteProcessando` | A consulta do recibo responde 105, como se o lote ainda estivesse na fila (E04). Só no `NFeRetAutorizacao4` (o `servico` nulo ou ele); o 106 e o 223 continuam na frente. |

Cada pedido consome **a primeira falha de transporte** que vale para ele, **a primeira paralisação** e, na consulta do recibo, **o primeiro lote em processamento**, na ordem em que foram comandadas. Elas podem valer juntas: um atraso seguido do 109, por exemplo. O `vezes` é descontado a cada pedido atingido, e a falha some quando chega a zero.

A `conexaoDerrubada` com `depois` na autorização é a queda no meio: a nota fica autorizada e a resposta não chega; o reenvio da mesma nota é 204, e a recuperação consulta pela chave no `NFeConsultaProtocolo4`, que devolve o `protNFe` da autorização.

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
| `EMULADA:irregular:emitente`, `EMULADA:irregular:destinatario` | Só a nota que a traz: o emitente irregular (781) ou, na NF-e, o destinatário irregular (302), como em "Irregulares". As outras notas do lote seguem normais. |

O resto do texto fica como está (`Pedido 123 EMULADA:paralisacao:109`). Uma marca por espécie no lote (transporte, paralisação, lote em processamento); a do irregular é da nota. A marca que não serve (tipo desconhecido, valor que o tipo não aceita, duas da mesma espécie) responde HTTP 400 com a falha `soap:Sender` e a explicação, sem processar o lote: o erro de digitação não passa calado.
