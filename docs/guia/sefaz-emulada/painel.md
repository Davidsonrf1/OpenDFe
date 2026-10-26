# Usar o painel da SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Painel

O painel é uma página no navegador para acompanhar e comandar a SEFAZ emulada. Ele mostra cada pedido que chega, com o XML do pedido e da resposta, e deixa paralisar autorizadores, comandar falhas, ver as notas autorizadas e cadastrar dados de teste, sem escrever código.

## Abrir o painel

Com o emulador no ar, abra `http://127.0.0.1:5444/` no navegador.

O painel tem a sua própria porta, em **HTTP sem TLS e só no loopback**: o navegador não precisaria de certificado de cliente, e ninguém de fora do computador alcança a página. A porta dos Web Services (`5443`) e o TLS mútuo não mudam.

> **Observação**
> O painel só responde quando o endereço do pedido é `127.0.0.1`, `localhost` ou `[::1]` (403 nos outros), para que um site de fora não consiga ler nem comandar o emulador. Para desligar o painel, suba o emulador com `--painel 0`.

## As abas

| Aba | O que mostra e o que faz |
|---|---|
| **Autorizadores** | Cada autorizador (as UFs, a SVRS, a SVAN, as duas SVC e o Ambiente Nacional), com as UFs que atende e os serviços que tem. Paralise um autorizador com o 108 ou o 109 num clique e libere depois. Com o autorizador de uma UF paralisado, o painel mostra a SVC que atende a contingência. |
| **Pedidos** | Os pedidos ao vivo, do mais novo ao mais antigo, com o filtro por autorizador, serviço e situação. Cada pedido abre o XML do pedido e da resposta, formatado. Na distribuição DF-e, os `docZip` aparecem já descompactados. |
| **Falhas** | As falhas comandadas ativas, com quantas vezes ainda vão atingir um pedido, e um formulário para cada tipo de falha. |
| **Notas** | As notas autorizadas, com a situação, os eventos e o XML de cada uma, e as faixas inutilizadas. |
| **Distribuição** | O que o Ambiente Nacional distribui a cada interessado, com o NSU e o tipo do documento. |
| **Cadastros** | Os contribuintes da consulta cadastro, os CNPJs e CPFs irregulares e os CSCs da NFC-e. |
| **Conexão** | O `ac.crt` para baixar e as URLs dos Web Services para copiar. |

O botão **Zerar tudo** pede confirmação e esquece o estado inteiro: as falhas, as notas (com os lotes, os eventos, as faixas e a distribuição), os CSCs, os irregulares, os contribuintes e os pedidos recentes.

A aba aberta fica no endereço (`http://127.0.0.1:5444/#pedidos`), para você guardar o link.

## Os pedidos recentes

O emulador guarda os últimos pedidos em memória (100 por padrão; `--pedidos` muda). Cada um fica com:

- o XML do pedido, com o envelope SOAP, e o XML da resposta;
- o transmissor (o CNPJ ou o CPF do certificado do TLS);
- o status HTTP, a situação (o `cStat` ou o que aconteceu) e a duração.

Entram também os pedidos que o emulador não atende (404, 415) e os que uma falha comandada interrompeu: o tempo esgotado e a conexão derrubada antes de processar aparecem sem resposta; a conexão derrubada **depois** de processar aparece com a resposta que teria saído, que é a que a recuperação da autorização precisa achar.

## Usar o painel a partir do código

As ações do painel são as rotas da API de controle (`/_controle`), na mesma porta. Tudo o que você faz clicando também se faz por HTTP, o que é o caminho dos testes automatizados:

```csharp
using System.Net.Http.Json;

using var painel = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5444/") };

// Paralisar MG no status do serviço, uma vez, com o 109.
await painel.PostAsJsonAsync("_controle/falhas",
    new { tipo = "paralisacao", destino = "MG", servico = "NFeStatusServico4", vezes = 1, codigo = "109" });

// As notas autorizadas.
var notas = await painel.GetStringAsync("_controle/notas");

// Zerar tudo.
await painel.DeleteAsync("_controle");
```

A lista completa está em [Referência das rotas de controle](api-controle.md).

## Próximas etapas

- [Simular falhas](falhas-comandadas.md)
- [Preparar dados de teste](dados-teste.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
