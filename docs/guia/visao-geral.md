# Visão geral da OpenDFe

[Documentação](../index.md) › Visão geral

A OpenDFe é uma biblioteca .NET para emitir, transmitir e receber a NF-e (modelo 55) e a NFC-e (modelo 65) no leiaute 4.00. Este artigo descreve o que a biblioteca faz, o que ela deixa para o seu sistema e como as partes se encaixam.

## O que a biblioteca faz

| Área | O que a OpenDFe oferece |
|---|---|
| **Documento** | Um modelo de dados em C# com nomes descritivos (`Destinatario.Nome`, `Produto.Valor`), que segue o XSD vigente e já inclui os grupos da reforma tributária (IBS, CBS e IS). |
| **XML** | Escrita e leitura do XML exatamente como o leiaute pede, com a assinatura XMLDSig. A leitura aceita o `NFe` e o `nfeProc` de qualquer Nota Técnica do leiaute 4.00. |
| **Validação** | O schema XSD sempre, as regras do MOC que pegam os erros mais comuns antes da transmissão, e regras de negócio suas, por callback. Os problemas vêm em português e apontam o campo do modelo. |
| **Transmissão** | SOAP 1.2 com TLS mútuo, a escolha do autorizador por UF e tipo de emissão, e o pool de conexões por certificado. |
| **Operações** | Status do serviço, autorização síncrona e em lote, consulta do recibo, consulta da nota, recuperação da autorização perdida, eventos, inutilização, distribuição DF-e, manifestação do destinatário e consulta cadastro. |
| **NFC-e** | QR Code (versões 2 e 3) e CSC, e a contingência offline. |
| **Impressão** | O DANFE e o DANFCE num modelo de impressão neutro, e o renderizador PDF. |
| **Testes** | A [SEFAZ emulada](sefaz-emulada/index.md), um servidor local que responde como os autorizadores em homologação. |

## O que fica com o seu sistema

A OpenDFe cuida do documento, não da decisão fiscal nem do armazenamento. Por desenho:

- **A biblioteca não calcula tributo.** Você informa os valores prontos (CST, CFOP, bases, alíquotas, totais). A biblioteca confere a coerência que a SEFAZ confere (o total é a soma dos itens, o grupo do ICMS combina com o CST), porque esse é o mesmo trabalho da rejeição, feito antes de transmitir.
- **A biblioteca não grava nada.** Quem guarda o XML assinado, o `nfeProc`, o protocolo e o NSU da distribuição é o seu sistema.
- **A biblioteca não reenvia sozinha.** Depois de uma falha de rede, o próximo passo é seu, e o guia [Quando a conexão cai](nfe/queda-recuperacao.md) diz qual é.
- **A biblioteca não imprime na impressora.** Ela entrega o modelo de impressão e o PDF; o envio para a impressora térmica (ESC/POS) é do aplicativo.

## Princípios

- **Sem estado global e sem I/O escondido.** O certificado, as conexões e o relógio entram pela configuração. Nada é lido de arquivo ou de configuração estática.
- **Problema não é exceção.** A validação e a rejeição da SEFAZ voltam como resultado, com a lista de problemas ou o `cStat`. Exceção é só para falha de infraestrutura (rede, certificado, resposta ilegível) e para erro de programação.
- **Determinística e invariável à cultura.** Valores em `decimal`, datas em `DateTimeOffset` com o fuso, nada depende da cultura da máquina.
- **Segura para uso concorrente.** O núcleo, o leiaute e os resultados são imutáveis; um núcleo atende chamadas em paralelo.
- **Tabelas oficiais embutidas e substituíveis.** Os schemas, as URLs dos autorizadores e as URLs de consulta da NFC-e vêm na biblioteca, e você troca a que mudar sem esperar uma versão nova.

## Como as partes se encaixam

```
seu sistema
   │  NotaFiscal (o modelo), parâmetros (Autorizacao, ConsultaProtocolo, EnvioEvento...)
   ▼
DFe (o núcleo)  ──────────────►  LeiauteNFe400 (o leiaute 4.00)
   │  assina, valida pelo schema,     monta o XML, aplica as regras do MOC,
   │  escolhe a URL, transmite        lê o retorno, monta o DANFE e o DANFCE
   ▼
SEFAZ (ou a SEFAZ emulada)
```

- O **núcleo** (`DFe`) não conhece nenhum documento: ele tem a assinatura, o SOAP, o TLS, a validação pelo schema e a escolha da URL.
- O **leiaute** (`LeiauteNFe400`) implementa um documento numa versão do leiaute: o mapeamento do modelo para as tags, os schemas, as regras, as operações e as URLs padrão.
- Toda operação passa pela mesma porta, `dfe.ExecutarAsync(parametro)`, e o tipo do parâmetro diz o tipo do retorno.

O artigo [Arquitetura](conceitos/arquitetura.md) detalha esse desenho.

## Pacotes

| Pacote | Conteúdo | Licença |
|---|---|---|
| `OpenDFe.Nucleo` | Contratos, certificado, assinatura XMLDSig, SOAP, TLS com a cadeia ICP-Brasil, validação pelo XSD, modelo de impressão | MIT |
| `OpenDFe.NFe` | O modelo da NF-e e da NFC-e e o leiaute 4.00 | MIT |
| `OpenDFe.Impressao.Pdf` | O DANFE e o DANFCE em PDF | MIT, com a dependência do QuestPDF, que tem [licença própria](impressao/pdf.md) |

## Plataformas

- .NET 10.
- Windows e Linux. No Linux, o OpenSSL 3 é requisito, com a política de criptografia permitindo a assinatura RSA-SHA1 que a NF-e exige.
- Certificado A1 (PFX). O A3 funciona no Windows pelo repositório de certificados do sistema.

## Próximas etapas

- [Instalar a OpenDFe](instalacao.md)
- [Autorizar a primeira NFC-e na SEFAZ emulada](inicio-rapido.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
