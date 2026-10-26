# Executar a SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Executar

A SEFAZ emulada é um aplicativo ASP.NET Core do repositório da OpenDFe, em `tests/OpenDFe.SefazEmulada`. Ela não é publicada como pacote: você a executa a partir do código-fonte, com o `dotnet run` ou com Docker.

## Pré-requisitos

- O repositório da OpenDFe clonado: `git clone https://github.com/Davidsonrf1/OpenDFe.git`.
- O SDK do .NET 10, para o `dotnet run`; ou o Docker com o Compose, para o contêiner.

## Executar com o dotnet run

Na raiz do repositório:

```
dotnet run --project tests/OpenDFe.SefazEmulada
```

Ao subir, o emulador mostra o endereço dos Web Services, o caminho da AC de teste e o endereço do painel, e registra cada pedido numa linha:

```
SEFAZ emulada em https://127.0.0.1:5443/ (só homologação, só para testes).
A AC de teste, para confiar no servidor: C:\Users\voce\AppData\Local\OpenDFe.SefazEmulada\ac.crt
As URLs: GET https://127.0.0.1:5443/. O controle das falhas: https://127.0.0.1:5443/_controle/falhas.
O painel, no navegador: http://127.0.0.1:5444/
Ctrl+C para parar.
MG/NFeStatusServico4: cStat 107
```

Para passar opções, use `--` depois do projeto:

```
dotnet run --project tests/OpenDFe.SefazEmulada -- --porta 6443 --painel 6444 --persistir sim
```

### Opções

| Opção | Padrão | Descrição |
|---|---|---|
| `--porta` | `5443` | A porta HTTPS dos Web Services. `0` escolhe uma porta livre. |
| `--painel` | `5444` | A porta do painel e da API de controle, em HTTP e só no loopback. `0` desliga o painel. |
| `--pasta` | `%LOCALAPPDATA%\OpenDFe.SefazEmulada` no Windows, `~/.local/share/OpenDFe.SefazEmulada` no Linux | Onde ficam a AC de teste e, com `--persistir sim`, o estado. |
| `--endereco` | `127.0.0.1` | O IP em que o servidor escuta. Com outro IP, o emulador atende a rede, mas a API de controle continua aceita só do próprio computador. |
| `--nome` | `127.0.0.1` | O nome usado nas URLs listadas, que também entra no certificado do servidor. |
| `--pedidos` | `100` | Quantos pedidos recentes o emulador guarda, com o XML, para o painel. |
| `--controle` | `local` | De onde a API de controle e o painel são aceitos: `local`, só do próprio computador; `rede`, de qualquer endereço (o modo do contêiner). |
| `--persistir` | `nao` | `sim` guarda o estado em disco e o recarrega na partida seguinte. |

> **Observação**
> Use `127.0.0.1`, não `localhost`. No Windows, `localhost` tenta o IPv6 (`::1`) antes, e o emulador escuta só no IPv4.

## Executar com Docker

O repositório traz o `Dockerfile` e o `compose.yaml` do emulador. Na raiz do repositório:

```
docker compose -f tests/OpenDFe.SefazEmulada/compose.yaml up -d --build
```

A imagem (`opendfe/sefaz-emulada:dev`) é construída localmente, a partir do código. O Compose publica as duas portas só no loopback: `127.0.0.1:5443` (os Web Services) e `127.0.0.1:5444` (o painel). Para quem usa, não há diferença em relação ao `dotnet run`: as URLs e o certificado do servidor são os mesmos.

| Variável | Padrão | Descrição |
|---|---|---|
| `SEFAZ_EMULADA_PORTA` | `5443` | A porta dos Web Services no computador. |
| `SEFAZ_EMULADA_PAINEL` | `5444` | A porta do painel no computador. |
| `SEFAZ_EMULADA_PASTA` | `tests/OpenDFe.SefazEmulada/dados` | A pasta da AC de teste no computador, montada em `/dados` no contêiner. |
| `SEFAZ_EMULADA_PERSISTIR` | `nao` | `sim` guarda o estado em `/dados/persistencia` (veja "Persistência", abaixo). |

Para usar a mesma AC do `dotnet run`, aponte a pasta para a dele:

```
SEFAZ_EMULADA_PASTA=$LOCALAPPDATA/OpenDFe.SefazEmulada docker compose -f tests/OpenDFe.SefazEmulada/compose.yaml up -d --build
```

Para parar: `docker compose -f tests/OpenDFe.SefazEmulada/compose.yaml down`.

### Acessar de outro contêiner

Outro contêiner na mesma rede do Compose chega ao emulador pelo nome do serviço, `sefaz-emulada:5443`. O certificado do servidor, porém, só leva `localhost`, `127.0.0.1`, `::1` e o `--nome`. Suba o emulador com `--nome sefaz-emulada` (acrescente ao `command` do serviço no `compose.yaml`) para que o TLS aceite esse nome.

## A AC de teste

Na primeira partida, o emulador gera uma AC de teste e a guarda na pasta: `ac.pfx` (com a chave, sem senha) e `ac.crt` (o certificado, em PEM). A AC vale 10 anos e é reaproveitada nas partidas seguintes. O certificado HTTPS do servidor é emitido por ela a cada partida.

O cliente precisa confiar nessa AC. Você pega o `ac.crt` de três formas:

- pelo caminho que o emulador mostra ao subir;
- pelo painel, aba **Conexão**;
- pela API: `GET http://127.0.0.1:5444/_controle/ac.crt`.

[Conectar a OpenDFe à SEFAZ emulada](conectar.md) mostra como confiar nela no código.

## Persistência

Por padrão, o emulador guarda tudo em memória e começa vazio a cada partida. Com `--persistir sim`, ele grava o estado em `<pasta>/persistencia/` e o recarrega na partida seguinte:

- `controle.db` (SQLite): as notas autorizadas e canceladas, os lotes, os eventos, as faixas inutilizadas, a sequência dos recibos e protocolos, a distribuição, os CSCs, os irregulares e os contribuintes da consulta cadastro;
- `xml/<autorizador>/`: os XMLs como foram recebidos (`nfe/<chave>.xml`, `eventos/<chave>-<tipo>-<seq>.xml`, `lotes/<recibo>.xml`) e, em `AN/distribuicao/`, o que o Ambiente Nacional distribui.

As falhas comandadas e os pedidos recentes não persistem: são do teste em curso. O botão **Zerar tudo** do painel (ou `DELETE /_controle`) esquece também o que está em disco.

> **Observação**
> Não há migração entre versões do emulador. Se o formato do banco mudar, apague a pasta `persistencia`.

No contêiner, defina `SEFAZ_EMULADA_PERSISTIR=sim`; o estado fica no volume `/dados`.

## Prompt para agentes de IA

Peça a um agente que automatize a subida do emulador. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva um script <PowerShell / Bash / docker compose> que sobe a SEFAZ emulada da OpenDFe, seguindo docs/guia/sefaz-emulada/executar.md,
com a porta <5443> dos Web Services, o painel na <5444>, a pasta da AC em <...> e a persistência <ligada / desligada>
(--persistir sim ou SEFAZ_EMULADA_PERSISTIR=sim). Espere o emulador ficar pronto consultando GET http://127.0.0.1:5444/_controle/emulador
e mostre o caminho do ac.crt. Use 127.0.0.1, não localhost.
```

## Próximas etapas

- [Conectar a OpenDFe à SEFAZ emulada](conectar.md)
- [Usar o painel](painel.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
