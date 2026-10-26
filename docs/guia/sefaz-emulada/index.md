# SEFAZ emulada

[Documentação](../../index.md) › SEFAZ emulada

A SEFAZ emulada é um servidor local que responde como os autorizadores da NF-e e da NFC-e **em homologação**. Ela implementa os Web Services que a OpenDFe usa, com o mesmo envelope SOAP 1.2, o mesmo TLS mútuo, os mesmos schemas e as regras que só a SEFAZ confere (duplicidade, numeração, situação da nota, prazos), com os códigos de retorno do MOC.

Use a SEFAZ emulada para:

- **Desenvolver sem certificado ICP-Brasil.** A homologação real só aceita o certificado A1 ICP-Brasil do emitente no TLS mútuo. O emulador aceita qualquer certificado, inclusive um autoassinado gerado na hora.
- **Testar o que a homologação real não deixa.** Serviço paralisado, tempo esgotado, conexão derrubada depois de autorizar (a queda no meio), resposta cortada, lote preso em processamento, contingência SVC com o autorizador normal fora, contribuinte irregular.
- **Rodar testes automatizados** rápidos e repetíveis, na sua máquina e na integração contínua.
- **Servir de ambiente de desenvolvimento** a um PDV ou a um ERP, com um painel no navegador para acompanhar cada pedido.

O emulador tem duas portas:

```
seu aplicativo + OpenDFe ──HTTPS, TLS mútuo──► SEFAZ emulada :5443  (os Web Services: /MG/NFeAutorizacao4 ...)
navegador, testes        ──HTTP, só local────► painel        :5444  (a página e a API /_controle)
```

## O que a SEFAZ emulada implementa

| Serviço | O que responde |
|---|---|
| `NFeStatusServico4` | O status (107), a paralisação comandada (108, 109) |
| `NFeAutorizacao4` | A autorização síncrona (104) e em lote (103), com as regras do lote e de cada nota, o QR Code da NFC-e e a duplicidade (204, 539) |
| `NFeRetAutorizacao4` | A consulta do recibo (104), o lote em processamento (105) |
| `NFeConsultaProtocolo4` | A situação da nota (100, 101, 217, 562) com os eventos |
| `NFeRecepcaoEvento4` | O cancelamento, o cancelamento por substituição, a carta de correção e, no Ambiente Nacional, a manifestação do destinatário |
| `NFeInutilizacao4` | A inutilização de faixas (102, 563, 241, 256) |
| `NFeDistribuicaoDFe` | A distribuição das notas destinadas, com o NSU, os resumos e o uso indevido (656) |
| `CadConsultaCadastro4` | A consulta cadastro (111, 112) dos contribuintes que você cadastra |

Os autorizadores são os reais: as 10 UFs com autorizador próprio, a SVRS, a SVAN, a SVC-AN, a SVC-RS e o Ambiente Nacional, cada um com as UFs que atende e os serviços que tem. Os detalhes, regra a regra, estão em [Serviços e códigos de retorno](servicos.md).

## O que ela não é

> **Importante**
> A SEFAZ emulada é a leitura que a OpenDFe faz do MOC e das Notas Técnicas, não a SEFAZ. Ela não substitui a homologação real, que é a única que prova a cadeia ICP-Brasil no TLS, as cifras de cada autorizador e as particularidades de cada UF.

- **Só homologação.** Pedido com `tpAmb` 1 (produção) é rejeitado com o 252.
- **Só para testes.** O certificado do cliente não tem a cadeia conferida; a validade, a revogação e a cadeia da assinatura (291, 293 a 296) não são conferidas.
- **Independente da biblioteca.** O emulador lê o pedido e monta o retorno com código próprio, a partir dos manuais, sem usar o leitor nem as regras da OpenDFe. Assim, um erro da biblioteca não aparece igual dos dois lados. Ele usa da biblioteca só o que é neutro: a validação pelo XSD e a verificação da assinatura.
- **As regras de conteúdo que a biblioteca já confere** antes de transmitir (os totais, por exemplo) não são repetidas, salvo onde o teste de uma rejeição precisa.

## Como começar

1. [Execute a SEFAZ emulada](executar.md) com `dotnet run` ou Docker.
2. [Conecte a OpenDFe a ela](conectar.md): o certificado de teste, a AC de teste e as URLs.
3. Siga o [Início rápido](../inicio-rapido.md) para autorizar a primeira NFC-e.
4. Abra o [painel](painel.md) em `http://127.0.0.1:5444/` para ver os pedidos.

## Nesta seção

| Artigo | Conteúdo |
|---|---|
| [Executar a SEFAZ emulada](executar.md) | `dotnet run`, Docker, as opções de linha de comando e a persistência em disco |
| [Conectar a OpenDFe à SEFAZ emulada](conectar.md) | O certificado de teste, a confiança na AC de teste e a troca das URLs |
| [Usar o painel](painel.md) | Acompanhar os pedidos, paralisar autorizadores, ver as notas e a distribuição pelo navegador |
| [Simular falhas](falhas-comandadas.md) | As falhas comandadas pela API de controle e pela marca no `infCpl` da nota |
| [Preparar dados de teste](dados-teste.md) | O CSC da NFC-e, os contribuintes irregulares, o cadastro da consulta cadastro e como zerar o estado |
| [Usar em testes automatizados](testes-automatizados.md) | Uma fixture xUnit, o isolamento entre testes e a integração contínua |
| [Serviços e códigos de retorno](servicos.md) | O que cada serviço confere e responde |
| [Referência das rotas de controle](api-controle.md) | A API HTTP do `/_controle`, rota a rota |

A especificação completa, com cada regra e a fonte no MOC, está em [SEFAZ_EMULADA.md](../../SEFAZ_EMULADA.md).

## Prompt para agentes de IA

Peça a um agente que monte o ambiente de desenvolvimento com a SEFAZ emulada. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Monte o ambiente de desenvolvimento do meu sistema com a SEFAZ emulada da OpenDFe, seguindo docs/guia/sefaz-emulada/index.md:
o emulador por <dotnet run / Docker Compose> a partir do repositório da OpenDFe, o painel em http://127.0.0.1:5444, e uma chave de
configuração no meu aplicativo que troca o certificado (autoassinado), as conexões (confiança na AC de teste) e as URLs
(ConfiguracaoDFe.Urls) entre o emulador e a SEFAZ real. Lembre que o emulador é só homologação e não confere a cadeia ICP-Brasil.
```

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
