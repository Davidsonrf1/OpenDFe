# Solução de problemas

[Documentação](../index.md) › Solução de problemas

Este artigo reúne os problemas mais comuns e o que fazer com cada um.

## Conexão e TLS

### FalhaTransmissaoException do tipo Tls

O servidor não foi aceito ou a negociação do TLS falhou.

- **Contra a SEFAZ emulada:** falta a confiança na AC de teste. Monte o `ConexoesSefaz` com a fábrica de [Conectar a OpenDFe à SEFAZ emulada](sefaz-emulada/conectar.md).
- **Contra a SEFAZ real, no Linux:** o autorizador usa uma AC pública que a imagem não tem. Instale o pacote `ca-certificates` da distribuição. As raízes da ICP-Brasil já vêm na biblioteca.
- **Nome divergente:** a URL não corresponde ao certificado do servidor (`localhost` em vez de `127.0.0.1`, um nome de contêiner). Use o nome que o certificado leva.

### FalhaTransmissaoException do tipo CertificadoRecusado

O servidor recusou o certificado do cliente (o HTTP 403 sem envelope de alguns autorizadores). Confira que o certificado é o A1 ICP-Brasil do emitente, vigente, e que o PFX tem a chave privada.

### FalhaTransmissaoException do tipo TempoEsgotado

O `TempoResposta` (60 segundos por padrão) acabou. A SEFAZ pode ter processado: na autorização, recupere a nota (veja [Quando a conexão cai](nfe/queda-recuperacao.md)). Se acontece sempre, confira a rede e aumente o `TempoResposta`.

### Na SEFAZ emulada, a conexão com localhost falha

No Windows, `localhost` tenta o IPv6 antes, e o emulador escuta só no IPv4. Use `127.0.0.1`.

## Certificado e assinatura

### CryptographicException ao abrir o PFX

A mensagem diz a causa: o arquivo não é um PFX, a senha está errada, o PFX não tem a chave privada ou a chave não é RSA.

### CryptographicException ao assinar, no Linux

A política de criptografia do sistema bloqueia o SHA-1 (família RHEL 9). Libere com `sudo update-crypto-policies --set DEFAULT:SHA1`. Veja [Instalação](instalacao.md).

### Rejeição 213 ou 227

O titular do certificado não é o emitente: a raiz do CNPJ (8 posições) do certificado é outra que a do emitente, ou o CPF é outro. Use o certificado da empresa emitente. Na SEFAZ emulada, gere o certificado de teste com o CNPJ do emitente.

### Rejeição 292 na SEFAZ emulada

O certificado não tem o CNPJ nem o CPF nos campos que a ICP-Brasil usa. O certificado de teste precisa do CNPJ no `serialNumber` ou no `otherName` 2.16.76.1.3.3. Veja [Conectar a OpenDFe à SEFAZ emulada](sefaz-emulada/conectar.md).

## Configuração do núcleo

### ArgumentException no DFe.Criar: "O leiaute não declara o envelope SOAP do serviço..."

Uma URL em `ConfiguracaoDFe.Urls` é de um serviço que o documento não tem (a distribuição DF-e num núcleo de NFC-e, por exemplo). Filtre as URLs pelos serviços da declaração: `DeclaracaoLeiaute.Criar(leiaute, documento).Servicos`.

### ArgumentException no DFe.Criar sobre a contingência

O tipo de emissão diferente de `Normal` exige a `Contingencia` (o início e a justificativa), e o `Normal` a recusa.

### NotSupportedException no ExecutarAsync

O leiaute não registrou a operação para o documento (a distribuição DF-e na NFC-e), ou não há URL para o autorizador, o ambiente e o serviço (o status no Ambiente Nacional). Confira com `dfe.Suporta<TParametro>()` e `dfe.Enderecos`.

## Montagem e validação

### RecusadoLocalmente com 225

A nota não passa no schema. O `Problema.Propriedade` aponta o campo do modelo, e `Problema.Schema` traz o que o schema esperava (o tipo, o tamanho, o padrão, os valores aceitos). Os campos obrigatórios sem valor aparecem como `ElementoAusente`.

### RecusadoLocalmente com 564 ou 610

Os totais não fecham com os itens. A biblioteca não calcula os totais: some os itens e preencha `Total.Icms`. A mensagem traz os dois valores.

### NFE-QRCODE

A NFC-e de uma UF sem URL de consulta na tabela da biblioteca. Informe `UrlQrCode` e `UrlChave` na `ConfiguracaoNFCe`. Veja [QR Code e CSC](nfce/qrcode-csc.md).

### NFE-NUCLEO

Um campo que o núcleo já sabe (a UF, o modelo, o ambiente, o tipo de emissão, o QR Code) foi preenchido na nota com outro valor. Deixe-o nulo.

### Uma regra da biblioteca que o autorizador não aplica

Rebaixe o código a aviso com `ConfiguracaoDFe.RegrasComoAviso` enquanto a versão corrigida não sai, e [abra uma issue](https://github.com/Davidsonrf1/OpenDFe/issues). Veja [Validação](conceitos/validacao.md).

## Retornos da SEFAZ

### 539 (duplicidade, outra chave)

A mesma numeração já foi autorizada com outra chave, em geral porque a nota foi gerada de novo depois de uma queda. Não gere a nota de novo: reenvie o XML assinado. Veja [Quando a conexão cai](nfe/queda-recuperacao.md).

### 704 (NFC-e emitida há mais de 5 minutos)

A data de emissão da NFC-e normal ficou velha. Emita com `DateTimeOffset.Now` e transmita logo. A nota emitida em contingência offline não tem esse limite.

### 656 na distribuição DF-e

Uso indevido: o CNPJ está bloqueado por uma hora. Não consulte de novo antes: cada consulta no bloqueio recomeça a hora. Veja [Distribuição DF-e](nfe/distribuicao-dfe.md).

### 252 na SEFAZ emulada

O núcleo está em `Ambiente.Producao`. O emulador só emula a homologação.

## PDF

### InvalidOperationException ao gerar o PDF

A licença do QuestPDF não foi definida. Defina `QuestPDF.Settings.License` na partida do aplicativo. Veja [Gerar o PDF](impressao/pdf.md).

### A fonte não muda

A família informada em `OpcoesPdf.Fonte` não está registrada no QuestPDF, e o PDF caiu na Lato. Registre a fonte com `FontManager.RegisterFont` antes de gerar.

## Ainda com dúvida?

- Consulte a [referência da API](../referencia/index.md) para o comportamento exato de cada membro.
- Leia o código-fonte no [repositório](https://github.com/Davidsonrf1/OpenDFe): os testes contra a SEFAZ emulada (`tests/OpenDFe.NFe.Testes`) mostram cada operação em uso.
- Abra uma [issue no GitHub](https://github.com/Davidsonrf1/OpenDFe/issues).

## Prompt para agentes de IA

Peça a um agente que diagnostique um erro. Use junto com o [prompt de contexto](agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Estou usando a OpenDFe e recebi este erro: <cole a exceção, o Problema.ToString() ou o cStat e o xMotivo>.
O contexto: documento <NF-e/NFC-e>, UF <...>, ambiente <SEFAZ emulada / homologação / produção>, sistema <Windows/Linux>.
Use docs/guia/solucao-problemas.md e docs/guia/conceitos/resultados-problemas.md para diagnosticar. Diga se o erro é de configuração,
de montagem da nota (RecusadoLocalmente), uma rejeição da SEFAZ (Processado com cStat) ou de transmissão (FalhaTransmissaoException,
e se PodeTerSidoProcessado). Proponha a correção e, se a nota pode ter sido autorizada, o passo de recuperação.
```

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
