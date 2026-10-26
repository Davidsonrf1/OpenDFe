# Referência das rotas de controle da SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Rotas de controle

A API de controle (`/_controle`) comanda e inspeciona a SEFAZ emulada. É o que o painel usa por dentro e o que os testes automatizados chamam.

## Onde fica

| Porta | Endereço | Observação |
|---|---|---|
| Painel (`5444`) | `http://127.0.0.1:5444/_controle/...` | HTTP, sem certificado. O caminho recomendado. |
| Web Services (`5443`) | `https://127.0.0.1:5443/_controle/...` | Passa pelo mesmo TLS mútuo dos Web Services: o cliente precisa apresentar um certificado e confiar na AC de teste. |

As rotas só são aceitas do próprio computador (403 de fora), salvo com `--controle rede` (o modo do contêiner, publicado só no loopback). No painel, o cabeçalho `Host` tem de ser `127.0.0.1`, `localhost` ou `[::1]`. As mudanças são `POST` com JSON e `DELETE`.

## O emulador

| Rota | Resposta |
|---|---|
| `GET /_controle/emulador` | `{ autoridade, endereco, painel, pedidos, urls }`: o caminho do `ac.crt`, o endereço dos Web Services, o do painel, quantos pedidos ficam guardados e as URLs (`{ destino, servico, url }`) |
| `GET /_controle/ac.crt` | O certificado da AC de teste, em PEM |
| `GET /_controle/autorizadores` | Cada autorizador: `sigla`, `casa` (o `cUF`), `tipoEmissao` (na SVC), as UFs `atendidas`, as do `cadastro`, os `servicos` e, no autorizador normal, a `contingencia` (`{ uf, svc }`) |
| `DELETE /_controle` | Zera tudo: falhas, notas, lotes, eventos, faixas, distribuição, CSCs, irregulares, contribuintes e pedidos recentes. 204. |
| `GET /` (só na porta dos Web Services) | `{ autoridade, urls }` |

## Falhas

| Rota | Resposta |
|---|---|
| `POST /_controle/falhas` | Acrescenta uma falha. 201 com a falha e o `Location`, ou 400 com `{ "erro": "..." }`. |
| `GET /_controle/falhas` | As falhas ativas, na ordem em que foram comandadas |
| `DELETE /_controle/falhas/{id}` | Remove uma: 204, ou 404 |
| `DELETE /_controle/falhas` | Remove todas: 204 |

```json
{ "tipo": "paralisacao", "destino": "MG", "servico": "NFeStatusServico4", "vezes": 1, "codigo": "109", "observacao": "Manutenção" }
```

| Campo | Tipo | Descrição |
|---|---|---|
| `tipo` | texto, obrigatório | `atraso`, `tempoEsgotado`, `conexaoDerrubada`, `respostaIlegivel`, `falhaSoap`, `paralisacao` ou `loteProcessando` |
| `destino` | texto | O autorizador (`MG`, `SVRS`, `SVAN`, `SVC-AN`, `SVC-RS`, `AN`); nulo vale para todos |
| `servico` | texto | O serviço (`NFeAutorizacao4`...); nulo vale para todos |
| `vezes` | número | Quantos pedidos a falha atinge (1 ou mais); nulo, até ser removida |
| `milissegundos` | número | Só no `atraso` |
| `depois` | lógico | Só na `conexaoDerrubada`: derruba depois de processar |
| `codigo` | texto | Só na `paralisacao`: `108` (padrão) ou `109` |
| `observacao` | texto | Só na `paralisacao`: o `xObs` |

Veja [Simular falhas](falhas-comandadas.md).

## Notas, eventos e faixas

| Rota | Resposta |
|---|---|
| `GET /_controle/notas` | As notas autorizadas, na ordem: `chave`, `numeracao`, `protocolo`, `recibo`, `recebimento`, `autorizador`, `situacao` (100 ou 150) e `cancelada` |
| `GET /_controle/notas/{chave}` | Uma nota, com o `xml` do `nfeProc` e os `eventos` (cada um com o `xml` do `procEventoNFe`); 404 se não está |
| `DELETE /_controle/notas` | Esquece as notas, os lotes, os eventos, as faixas e a distribuição: 204 |
| `GET /_controle/eventos` | Os eventos registrados: `chave`, `tipo`, `sequencia`, `protocolo`, `registro`, `autorizador` |
| `GET /_controle/inutilizacoes` | As faixas inutilizadas |
| `GET /_controle/distribuicao` | Por interessado, os documentos com `nsu`, `chave`, `schema` e `recebimento` |

## CSC

| Rota | Resposta |
|---|---|
| `POST /_controle/csc` | Cadastra `{ uf, cnpj, id, csc }` (a raiz do CNPJ, o identificador sem zeros, o código de 16 a 36 caracteres). O mesmo identificador de novo troca o código e desfaz a revogação. 201 ou 400. |
| `GET /_controle/csc` | Os cadastrados e revogados: `uf`, `cnpj`, `id`, `csc`, `revogacao` |
| `DELETE /_controle/csc/{uf}/{cnpj}/{id}` | Revoga no instante do pedido (também o padrão): 204, ou 404 |
| `DELETE /_controle/csc` | Volta só ao CSC padrão (`1`, `OPENDFE-SEFAZ-EMULADA-CSC-PADRAO`): 204 |

## Irregulares

| Rota | Resposta |
|---|---|
| `POST /_controle/irregulares` | Cadastra `{ documento }` (CNPJ ou CPF, sem máscara, com o dígito conferido). 201 ou 400. |
| `GET /_controle/irregulares` | Os documentos |
| `DELETE /_controle/irregulares/{documento}` | Tira um: 204, ou 404 |
| `DELETE /_controle/irregulares` | Tira todos: 204 |

## Cadastros (consulta cadastro)

| Rota | Resposta |
|---|---|
| `POST /_controle/cadastros` | Cadastra ou troca o contribuinte da mesma UF e IE. 201 (com o `Location`, como `/_controle/cadastros/MG/0623079040081`), 200 quando trocou, ou 400. |
| `GET /_controle/cadastros` | Os contribuintes, por UF e IE |
| `DELETE /_controle/cadastros/{uf}/{ie}` | Tira um: 204, ou 404 |
| `DELETE /_controle/cadastros` | Tira todos: 204 |

| Campo | Obrigatório | Descrição |
|---|---|---|
| `uf` | sim | A sigla da UF |
| `ie` | sim | De 2 a 14 algarismos, não toda zero |
| `cnpj` ou `cpf` | um dos dois | Com o dígito certo |
| `nome` | sim | Até 60 caracteres |
| `fantasia`, `regimeApuracao`, `cnae` | não | Texto |
| `inicioAtividade`, `ultimaSituacao`, `baixa` | não | Data `AAAA-MM-DD` |
| `ieUnica`, `ieAtual` | não | A IE única e a atual |
| `endereco` | não | `{ logradouro, numero, complemento, bairro, codigoMunicipio, municipio, cep }` |
| `habilitado` | não | `true` (padrão) ou `false` |
| `credenciadoNFe`, `credenciadoCTe` | não | O indicador de credenciamento (`1` e `4` quando omitidos) |

## Pedidos recentes

| Rota | Resposta |
|---|---|
| `GET /_controle/pedidos[?depois=<id>]` | Os pedidos, sem o XML, do mais antigo ao mais novo: `id`, `recebimento`, `destino`, `servico`, `transmissor`, `status`, `situacao`, `milissegundos`, `tamanhoPedido`, `tamanhoResposta`. Com `depois`, só os mais novos que o id. |
| `GET /_controle/pedidos/{id}` | Um pedido, com o `pedido` e a `resposta` em texto; 404 se já saiu da fila |
| `GET /_controle/pedidos/fluxo` | Os avisos ao vivo, em `text/event-stream`: `event: pedido` a cada pedido novo e `event: estado` a cada mudança pelo controle |
| `DELETE /_controle/pedidos` | Esquece os pedidos: 204 |

## Prompt para agentes de IA

Peça a um agente que escreva um utilitário de linha de comando para o controle. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva um utilitário de linha de comando (C# ou PowerShell) para a API de controle da SEFAZ emulada da OpenDFe, seguindo
docs/guia/sefaz-emulada/api-controle.md: subcomandos para paralisar/liberar (POST /_controle/falhas com tipo paralisacao), listar as
notas (GET /_controle/notas), mostrar uma nota com o XML (GET /_controle/notas/{chave}), acompanhar os pedidos ao vivo
(GET /_controle/pedidos/fluxo, text/event-stream) e zerar (DELETE /_controle). Endereço padrão http://127.0.0.1:5444.
```

## Próximas etapas

- [Usar em testes automatizados](testes-automatizados.md)
- [Visão geral da SEFAZ emulada](index.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
