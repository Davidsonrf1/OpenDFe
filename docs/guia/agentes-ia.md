# Usar a OpenDFe com agentes de IA

[Documentação](../index.md) › Agentes de IA

Agentes de IA (Claude, GitHub Copilot, ChatGPT e outros) escrevem código melhor com a OpenDFe quando recebem o contexto certo: as regras da biblioteca, os tipos principais e as armadilhas do domínio fiscal. Este artigo traz um prompt de contexto para começar qualquer conversa, e cada página da documentação tem, no fim, uma seção **Prompt para agentes de IA** com um pedido pronto sobre o tema dela.

## Como usar os prompts

1. Cole o **prompt de contexto** abaixo no começo da conversa, ou no arquivo de instruções do agente (`CLAUDE.md`, `.github/copilot-instructions.md`, `AGENTS.md`).
2. Copie o prompt da página do tema e complete os trechos entre `<...>` com o seu caso.
3. Peça ao agente que confira os nomes dos tipos e membros na [referência da API](../referencia/index.md) ou no código-fonte, e que rode o código contra a [SEFAZ emulada](sefaz-emulada/index.md) antes de você confiar nele.

> **Importante**
> O agente pode inventar membros que não existem ou regras fiscais que não se aplicam. Revise o código gerado, rode os testes contra a SEFAZ emulada e, em caso de dúvida, consulte o código-fonte da biblioteca.

## Prompt de contexto

```text
Você vai escrever código C# (.NET 10) que usa a biblioteca OpenDFe para NF-e (modelo 55) e NFC-e (modelo 65), leiaute 4.00.
Documentação: https://github.com/Davidsonrf1/OpenDFe/tree/main/docs. Confira nomes de tipos e membros em docs/referencia antes de usá-los; não invente APIs.

Regras da biblioteca:
- Namespaces: OpenDFe (núcleo: DFe, ConfiguracaoDFe, ConexoesSefaz, CertificadoDigital, ChaveAcesso, Problema),
  OpenDFe.NFe (LeiauteNFe400, NotaFiscal e o modelo, operações), OpenDFe.Impressao (modelo de impressão), OpenDFe.Impressao.Pdf.
- Crie o núcleo com DFe.Criar(new LeiauteNFe400(), Documento.NFe ou Documento.NFCe, new ConfiguracaoDFe { Ambiente, UF, Certificado, Conexoes }).
  A configuração é imutável: outro emitente, documento ou contingência é outro núcleo. Um ConexoesSefaz por processo, compartilhado.
- Toda operação é await dfe.ExecutarAsync(parametro): ConsultaStatus, Autorizacao, ConsultaRecibo, RecuperacaoAutorizacao,
  ConsultaProtocolo, EnvioEvento (Cancelamento, CartaCorrecao, CancelamentoSubstituicao, manifestação), Inutilizacao,
  DistribuicaoDFe, ConsultaCadastro. dfe.Preparar(nota) gera, assina e valida sem transmitir.
- Problema não é exceção: confira resultado.Processado. RecusadoLocalmente traz resultado.Problemas (nada foi enviado).
  Na autorização, o cStat do resultado é o do lote; o da nota está em Retorno.Notas[i].Protocolo.Status (Autorizada = 100 ou 150).
- FalhaTransmissaoException é falha de rede/TLS/SOAP. Se PodeTerSidoProcessado for verdadeiro, NÃO reenvie a NotaFiscal:
  tire o XML com Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido) e use RecuperacaoAutorizacao(xml).
- A nota é o XML assinado: guarde o XML (Preparar ou ResultadoNota.Xml) e o nfeProc (ResultadoNota.XmlProcessado).
  Nunca gere a NotaFiscal de novo para reenviar (o cNF é sorteado e a chave muda: rejeição 539).
- A biblioteca não calcula tributo nem totais, não grava nada, não reenvia sozinha e não espera. Preencha valores prontos em decimal.
- Não preencha no modelo os campos que o núcleo sabe (UF, modelo, ambiente, tipo de emissão, contingência, chave, QR Code).
- Datas em DateTimeOffset com o fuso; nada depende da cultura da máquina.
- Para testes, use a SEFAZ emulada (tests/OpenDFe.SefazEmulada no repositório): só homologação, certificado autoassinado com
  o CNPJ do emitente, confiança na AC de teste pela fábrica do ConexoesSefaz, URLs em ConfiguracaoDFe.Urls filtradas pelos
  serviços de DeclaracaoLeiaute.Criar(leiaute, documento).Servicos.
- Nunca desligue a validação do certificado do servidor (return true). Não registre em log os XMLs nem o CSC.
```

## Boas práticas ao pedir código

- **Diga o documento e a UF.** NF-e e NFC-e têm operações diferentes (a carta de correção e a distribuição são só da NF-e), e a UF decide o autorizador e as URLs de consulta da NFC-e.
- **Diga o ambiente.** Desenvolvimento e testes contra a SEFAZ emulada; homologação real; produção.
- **Peça o tratamento de erro junto.** "Inclua o tratamento de `RecusadoLocalmente`, da rejeição e da `FalhaTransmissaoException`, seguindo o guia Quando a conexão cai."
- **Peça o teste junto.** "Escreva também um teste xUnit contra a SEFAZ emulada, com a fixture da documentação."
- **Peça que o agente cite a fonte.** "Para cada regra fiscal que você aplicar, diga de onde ela vem (a documentação da OpenDFe, o MOC ou uma NT)."

## Próximas etapas

- [Início rápido: a primeira NFC-e na SEFAZ emulada](inicio-rapido.md)
- [Usar a SEFAZ emulada em testes automatizados](sefaz-emulada/testes-automatizados.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
