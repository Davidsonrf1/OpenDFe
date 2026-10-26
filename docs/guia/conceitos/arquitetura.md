# Arquitetura: núcleo, leiaute e pacotes

[Documentação](../../index.md) › Conceitos › Arquitetura

Este artigo explica como a OpenDFe é organizada: o núcleo, o leiaute, o documento e o caminho que toda operação percorre. Entender essas peças ajuda a ler os resultados, a trocar o que precisa ser trocado e a usar a biblioteca em sistemas com muitos emitentes.

## Núcleo e leiaute

A OpenDFe separa o que é comum a todo documento fiscal do que é de um documento numa versão do leiaute.

| Peça | Tipo | Responsabilidade |
|---|---|---|
| **Núcleo** | `DFe` (pacote `OpenDFe.Nucleo`) | A assinatura XMLDSig, a validação pelo schema, o envelope SOAP, o TLS, a escolha da URL, os callbacks de validação. Não conhece nenhum documento. |
| **Leiaute** | `LeiauteNFe400` (pacote `OpenDFe.NFe`) | O mapeamento do modelo para as tags, os schemas, as regras do MOC, as operações, os autorizadores e as URLs padrão, o QR Code, o DANFE e o DANFCE. Implementa `ILeiaute`. |
| **Modelo** | `NotaFiscal` e os grupos (pacote `OpenDFe.NFe`) | Os dados da nota, com nomes descritivos. Um modelo só para todas as versões do leiaute. |
| **Documento** | `Documento.NFe` ou `Documento.NFCe` | Qual documento o leiaute atende num núcleo. NF-e e NFC-e são documentos distintos no mesmo leiaute, com o mesmo modelo. |

O leiaute é entregue pelo tipo: você referencia o pacote e passa a instância (`new LeiauteNFe400()`). O leiaute não tem estado e pode ser compartilhado por todos os núcleos.

## Um núcleo, uma configuração

Um núcleo é criado com um leiaute, um documento e uma configuração:

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.SP,
    Certificado = certificado,
    Conexoes = conexoes,
});
```

A configuração é imutável. Outro documento, outro emitente, outro ambiente ou a entrada em contingência é **outro núcleo**, e criar um núcleo é barato. Num sistema com vários emitentes, crie um núcleo por emitente e documento, todos com o mesmo `ConexoesSefaz`. Veja [O núcleo e a configuração](nucleo-configuracao.md).

## Uma porta para todas as operações

Toda operação com a SEFAZ é `dfe.ExecutarAsync(parametro)`. O parâmetro deriva de `ParametroWS<TRetorno>`, e o C# deduz o tipo do retorno:

```csharp
ResultadoWS<RetornoStatus> status = await dfe.ExecutarAsync(new ConsultaStatus());
ResultadoWS<RetornoAutorizacao> autorizacao = await dfe.ExecutarAsync(new Autorizacao(nota));
ResultadoWS<RetornoEvento> evento = await dfe.ExecutarAsync(new EnvioEvento(cancelamento));
```

| Operação | Parâmetro | Retorno |
|---|---|---|
| Status do serviço | `ConsultaStatus` | `RetornoStatus` |
| Autorização | `Autorizacao` | `RetornoAutorizacao` |
| Consulta do recibo | `ConsultaRecibo` | `RetornoAutorizacao` |
| Recuperação da autorização | `RecuperacaoAutorizacao` | `RetornoAutorizacao` |
| Consulta da nota | `ConsultaProtocolo` | `RetornoProtocolo` |
| Eventos | `EnvioEvento` | `RetornoEvento` |
| Inutilização | `Inutilizacao` | `RetornoInutilizacao` |
| Distribuição DF-e (só NF-e) | `DistribuicaoDFe` | `RetornoDistribuicao` |
| Consulta cadastro (só NF-e) | `ConsultaCadastro` | `RetornoCadastro` |

Para saber o que um núcleo suporta, use `dfe.Suporta<DistribuicaoDFe>()` ou percorra `dfe.Operacoes`.

Há também uma operação local, sem a SEFAZ: `dfe.Preparar(nota)` gera, assina e valida a nota sem transmitir.

## O caminho de uma operação

```
ExecutarAsync(parametro)
  1. o leiaute monta a mensagem                      problema de Erro → RecusadoLocalmente
  2. o núcleo assina (se o serviço pede)
  3. o núcleo valida pelo schema                     problema de Erro → RecusadoLocalmente
  4. as regras do leiaute (o MOC)                    problema de Erro → RecusadoLocalmente
  5. os seus callbacks de validação                  problema de Erro → RecusadoLocalmente
  6. a escolha da URL (destino × ambiente × tipo de emissão × serviço)
  7. SOAP 1.2 com TLS mútuo                          falha de rede → FalhaTransmissaoException
  8. o leiaute lê o retorno                          resposta ilegível → FalhaTransmissaoException
  9. Processado, com o cStat
```

Nada sai do computador com um problema de severidade `Erro`. A rejeição da SEFAZ é `Processado`, com o `cStat` da rejeição. Veja [Resultados, problemas e exceções](resultados-problemas.md).

## O destino de uma operação

A URL sai de quatro coisas: o destino, o ambiente, o tipo de emissão e o serviço.

- **Quando a operação tem um documento ou uma chave, o destino e o tipo de emissão saem dele**, não da configuração. A NFC-e emitida offline é transmitida depois, por um núcleo já normal; a consulta e os eventos de uma nota emitida em SVC seguem o `tpEmis` da chave.
- **Sem documento nem chave** (o status, a inutilização), vale a UF e o tipo de emissão do núcleo.
- **O `Destino` do parâmetro sobrepõe tudo** (`new ConsultaStatus { Destino = Destino.SvcAN }`).

Veja [Autorizadores, URLs e contingência SVC](autorizadores-urls.md).

## Pacotes e namespaces

| Pacote | Namespace | Depende de |
|---|---|---|
| `OpenDFe.Nucleo` | `OpenDFe`, `OpenDFe.Impressao` | `System.Security.Cryptography.Xml` |
| `OpenDFe.NFe` | `OpenDFe.NFe` | `OpenDFe.Nucleo` |
| `OpenDFe.Impressao.Pdf` | `OpenDFe.Impressao.Pdf` | `OpenDFe.Nucleo`, QuestPDF, QRCoder |

O modelo e o leiaute de um documento saem no mesmo pacote e na mesma versão: o leiaute nunca roda contra um modelo de outra versão. Quem não gera PDF não carrega o QuestPDF.

## Extensibilidade

- **Leiautes de terceiros.** `ILeiaute` é uma interface mínima (`Versao`, `Documentos`, `Declarar`); tudo o que o leiaute tem, ele declara numa `DeclaracaoLeiaute`. Um leiaute novo registra operações (`IOperacao<TParametro, TRetorno>`), preparos, atendimentos, URLs, serviços SOAP, regras e formas de leitura.
- **Listagem sem núcleo.** `DeclaracaoLeiaute.Criar(leiaute, documento)` lista as operações, os autorizadores, as URLs e os serviços antes de haver certificado, por exemplo para uma tela de configuração.

## Prompt para agentes de IA

Peça a um agente que desenhe a integração. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Desenhe a integração da OpenDFe no meu sistema <descreva: camadas, injeção de dependência, multiempresa>, seguindo
docs/guia/conceitos/arquitetura.md. Um LeiauteNFe400 e um ConexoesSefaz compartilhados (singletons); um DFe por emitente e documento,
criado sob demanda e reaproveitado; a contingência como outro DFe. Todas as operações por dfe.ExecutarAsync(parametro), com o
CancellationToken. Mostre as classes e onde cada uma é registrada no contêiner.
```

## Próximas etapas

- [O núcleo e a configuração](nucleo-configuracao.md)
- [Resultados, problemas e exceções](resultados-problemas.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
