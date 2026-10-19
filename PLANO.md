# Plano de implementação — OpenDFe

O modelo é o `docs/PROJETO.md`; onde este plano e o documento divergirem, vale o documento. **Proposta de 2026-10-19, aguardando aprovação.**

## Como trabalhar

- **Uma fase por vez, só quando pedido.** Cada fase é dividida em etapas.
- **Uma etapa por sessão** (ou por agente), parando ao fim de cada uma; o estado fica escrito aqui ("Situação"), para a sessão seguinte começar dele.
- **Branch** `fase-<n>-<nome>`; commit ao fim de cada etapa, sem push.
- **Desenho antes de implementar** nas etapas que criam API pública (contratos do núcleo, modelo, parâmetros e retornos): os tipos e as assinaturas são aprovados antes do código, porque depois de publicada cada mudança é versão maior (`PROJETO.md` 12.3).
- **Conferir na fonte, na etapa:** pacote de liberação dos schemas, URLs dos autorizadores, versões do QR Code e regras do MOC mudam por NT. O plano não fixa versões; a etapa confere no Portal Nacional da NF-e e registra em `docs/NTS.md`.
- Decisão que não está no `PROJETO.md`: pergunte.

## Estrutura

```
OpenDFe.slnx
Directory.Build.props        net10.0, nullable, avisos como erro, determinístico, SourceLink,
                             documentação XML, validação de pacote
Directory.Packages.props     versões centralizadas
src/
  OpenDFe.Nucleo/                    pacote OpenDFe.Nucleo
  OpenDFe.NFe.Modelo/                ┐ pacote OpenDFe.NFe (as duas DLLs,
  OpenDFe.Leiaute.NFe.v400/          ┘ mesma versão; PROJETO.md 3.4)
    Schemas/<pacote de liberação>/   XSDs como recurso embutido
  OpenDFe.Impressao.Pdf/             pacote OpenDFe.Impressao.Pdf
tests/
  OpenDFe.Nucleo.Testes/
  OpenDFe.NFe.Testes/
  OpenDFe.Homologacao.Testes/        ligados por variável de ambiente (PROJETO.md 13.3)
  Exemplos/                          XMLs públicos e reais com os dados trocados
docs/  PROJETO.md, REGRAS.md, NTS.md, QUANDO_CAI.md
```

Testes em xUnit v3. API pública rastreada pelo `Microsoft.CodeAnalysis.PublicApiAnalyzers` (`PublicAPI.Shipped.txt` / `Unshipped.txt`): toda mudança de superfície aparece no diff, e é o que decide entre versão menor e maior.

## Riscos, atacados primeiro

1. **Assinatura RSA-SHA1 no Linux.** O OpenSSL 3 é requisito no Linux (`PROJETO.md` 2.8), mas a política de criptografia de algumas distribuições recusa assinatura SHA-1, que a NF-e ainda exige. A prova confere que a distribuição do PDV assina; a que recusa fica documentada como requisito de configuração (a política que libera o SHA-1), sem contorno dentro da lib.
2. **TLS mútuo com a SEFAZ no Linux:** a cadeia ICP-Brasil fora do repositório da máquina, o certificado do cliente no handler e as cifras de cada autorizador.

Os dois são provados na **etapa 1.7**, com uma chamada de status do serviço em homologação nos dois sistemas, antes de investir no modelo e no leiaute.

## Fases

| Fase | Conteúdo | Pronto quando |
|---|---|---|
| 0 | Repositório, licença, CI, estrutura | a CI roda vazia em Linux e Windows |
| 1 | Núcleo: tipos base, chave, certificado, assinatura, XSD, contratos, prova de risco | um XML de exemplo assina, verifica e valida; status de MG respondido nos dois sistemas |
| 2 | Modelo NF-e/NFC-e e leiaute 4.00: escrita, leitura, detector | notas de teste geradas válidas no XSD e lidas de volta iguais |
| 3 | Regras do MOC e callbacks | as rejeições mais comuns barradas antes |
| 4 | Operações e transmissão | NF-e autorizada em homologação, em Windows e Linux |
| 5 | NFC-e: QR Code, CSC, contingência offline | NFC-e online e em contingência autorizadas em homologação |
| 6 | Eventos e inutilização | cancelamento, substituição, CC-e e inutilização em homologação |
| 7 | DANFCE, DANFE e PDF | PDFs conferidos contra o manual |
| 8 | Distribuição DF-e, manifestação, consulta cadastro | notas destinadas baixadas em homologação |

As fases 1 a 5 e a 7 atendem o PDV (fase 5 dele); a 8, a caixa de entrada do ErpCore.

### Fase 0: repositório

- **0.1. Solução.** Os projetos vazios da estrutura, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitattributes` (fim de linha fixo; hoje o Git avisa LF → CRLF), `LICENSE` (MIT), README com a primeira linha de 14.4 e a nota de propriedade de 14.3.
- **0.2. CI e pacotes.** GitHub Actions em `ubuntu-latest` e `windows-latest`: build, testes, `dotnet pack` e a validação de pacote. O `OpenDFe.NFe` empacota as duas DLLs (o modelo como referência privada do leiaute, incluído no pacote) — conferido abrindo o `.nupkg`.
- **Do usuário, antes ou durante:** criar o repositório público no GitHub e reservar o prefixo `OpenDFe.*` no NuGet.org (14.1).

### Fase 1: núcleo

- **1.1. Tipos base.** `UF` (sigla e código IBGE), `Ambiente`, `TipoEmissao` (com o valor do XML), `Documento` (modelo 55, 65), CPF e CNPJ (inclusive alfanumérico: validação, formatação, DV), `Problema` e `Severidade`, e o formatador invariável que os leiautes usam (decimal com as casas do campo, `DateTimeOffset` com o fuso; 2.5).
- **1.2. Chave de acesso.** Geração (`cNF` por `RandomNumberGenerator`, nunca igual ao número; DV módulo 11, com o CNPJ alfanumérico), leitura das partes e validação. Testes com chaves reais.
- **1.3. Certificado.** Abrir o PFX, validade, titular (CNPJ ou CPF pelos OIDs da ICP-Brasil), e o gerador de certificado de teste que os outros testes usam (13.1).
- **1.4. Assinatura e verificação.** XMLDSig enveloped, C14N, RSA-SHA1, referência `#<Id>`, sobre o elemento indicado (a nota, o evento, a inutilização). Verificação da assinatura e do certificado do emitente contra o CNPJ do XML. Testes com os XMLs de exemplo, e a recusa do XML alterado depois de assinado.
- **1.5. Validação pelo XSD.** Carga do `XmlSchemaSet` a partir de recursos embutidos (com os `include`), validação que junta todos os erros, com a tag e o caminho do elemento, as restrições do tipo simples (tamanho, padrão, enumeração, casas) e a mensagem em português (6.4). Testes com XSDs de exemplo, sem depender do leiaute.
- **1.6. Contratos.** *Desenho aprovado antes.* `ILeiaute`, `IOperacao<TParametro, TRetorno>`, `ParametroWS<T>`, `ResultadoWS<T>`, `Destino`, `ConfiguracaoNucleo` (com a parte do documento), `Nucleo.Criar`, o registro das operações, `Suporta<T>()`, `Operacoes`, a declaração de destinos e URLs com a listagem, e a assinatura dos callbacks de regras (6.3). Testes com um leiaute falso.
- **1.7. Prova de risco.** Envelope SOAP 1.2 mínimo, `HttpClient` com o certificado do cliente e as ACs raiz da ICP-Brasil embutidas (`CustomRootTrust`), uma chamada de `NFeStatusServico4` em homologação de MG, com o pedido assinado em SHA-1 para provar o item 1 dos riscos mesmo sem o serviço exigir. Rodada no Windows e no Linux (a distribuição do PDV). Precisa do PFX A1 de homologação (variável de ambiente). O código dessa etapa é descartável: a fase 4 o refaz com o desenho definitivo, aproveitando o que a prova ensinar.

### Fase 2: modelo e leiaute 4.00

- **2.1. Esqueleto do leiaute.** O atributo `[Tag]` (no núcleo), `LeiauteNFe400 : ILeiaute` com NF-e e NFC-e, os schemas do pacote de liberação vigente (conferido; 4.6: com IBS, CBS e IS) e o registro em `docs/NTS.md`.
- **2.2. Modelo, cabeçalho e partes.** *Desenho aprovado antes.* Identificação, emitente, destinatário, retirada e entrega, autorizados a baixar o XML, transporte, cobrança, pagamento, intermediador, informações adicionais, exportação, compra, responsável técnico. Nomes descritivos com `[Tag]` (4.1).
- **2.3. Modelo, itens e tributos.** *Desenho aprovado antes.* Produto e os grupos específicos (combustível, medicamento, veículo, arma, rastreabilidade), ICMS por CST e CSOSN como subclasses (4.3), IPI, II, PIS, COFINS, ICMS de partilha, IBS, CBS e IS, e os totais.
- **2.4. Escrita.** `XmlWriter` à mão, um método por grupo (5.1); o aviso de campo preenchido e não suportado (4.4), pelo levantamento das propriedades `[Tag]` que o leiaute mapeia. Testes: toda nota gerada passa pelo XSD (13.2).
- **2.5. Leitura e detector.** `XmlReader`, `NFe` e `nfeProc`, NTs anteriores, problemas do que não reconhece, o XML original preservado (5.2, 5.3); migração do leitor do ErpCore (`ErpCore.Fiscal`, `Nfe/LeitorDeNfe.cs`), lido nesta etapa. `Nucleo.Detectar` (3.3). Testes: ida e volta reproduz o objeto; XMLs reais leem sem problema.
- **2.6. Problema até a propriedade.** O caminho do erro do XSD convertido no caminho do modelo pelo `[Tag]` (`det[3]/prod/vProd` → `Itens[2].Produto.Valor`).

### Fase 3: regras

- **3.1. Motor e catálogo.** As três etapas na ordem (6.1 a 6.3): schema, regras do leiaute, callbacks por campo e por documento. Regra com código da rejeição e mensagem do MOC. `docs/REGRAS.md`: levantamento das rejeições do MOC vigente, marcando as que a lib confere e as que só a SEFAZ confere.
- **3.2. Primeiro lote.** Chave e DV, CPF e CNPJ, totais contra a soma dos itens, CST × grupo, CFOP × `idDest`, `indIEDest` × IE, datas, `cClassTrib` contra a tabela. Um teste por regra, com o XML que a SEFAZ rejeitaria.
- **3.3. Regras da NFC-e.** As que só valem para o modelo 65 (destinatário, limites, formas de pagamento, `idDest`).

### Fase 4: operações e transmissão

- **4.1. Transporte.** SOAP 1.2 na base (envelope, `nfeDadosMsg`, ação, falha SOAP como exceção de infraestrutura), TLS da ICP-Brasil, `CipherSuitesPolicy` por destino, pool de `HttpMessageHandler` por certificado e a fábrica substituível (8.5, 2.3). Testes contra um Kestrel local com TLS mútuo.
- **4.2. Destinos e URLs.** A tabela do leiaute (cada UF no seu autorizador, SVRS, SVAN, AN, SVC-AN, SVC-RS; homologação e produção), conferida nos portais; substituição e listagem; a escolha da URL, com destino e tipo de emissão vindos da chave ou do documento quando houver (8.4). Testes unitários da escolha, caso a caso.
- **4.3. O caminho da operação e o status.** `ExecutarAsync`: montar → validar → assinar → SOAP → ler; `RecusadoLocalmente`; `ResultadoWS` com o rastro. `ConsultaStatus` em homologação.
- **4.4. Autorização.** Síncrona e por lote, `ConsultaRecibo`, `nfeProc`, `Preparar` e a autorização do XML já assinado. NF-e autorizada em homologação.
- **4.5. Queda no meio.** `ConsultaProtocolo` (destino pela chave), `RecuperacaoAutorizacao`, a duplicidade identificada com a chave, e o guia `docs/QUANDO_CAI.md` (8.6).

### Fase 5: NFC-e

- **5.1. QR Code e CSC.** A configuração do documento declarada pelo leiaute (3.2), o QR Code nas versões que a NT vigente aceita (conferidas), `infNFeSupl` e a URL de consulta por UF (9.1). Autorização online em homologação de MG.
- **5.2. Contingência offline.** Núcleo com `tpEmis=9`, justificativa e entrada em contingência, QR Code offline, `Preparar` e a transmissão posterior pelo núcleo normal (9.2, 8.4). Autorizada em homologação, nos dois sistemas.

### Fase 6: eventos

- **6.1. Base e cancelamento.** `EnvioEvento` (1 a 20, assinatura por evento, `procEventoNFe`), o detalhe por subclasse, `Cancelamento`; e a `Inutilizacao`.
- **6.2. Substituição e CC-e.** `CancelamentoPorSubstituicao` (NFC-e) e `CartaCorrecao`. Os quatro em homologação.

### Fase 7: impressão

- **7.1. Biblioteca de PDF.** Decidida no início da fase (10.2), pela licença e pelo resultado de uma página de teste com código de barras e QR Code.
- **7.2. Modelo de impressão e DANFCE.** O modelo neutro no núcleo (blocos, seções, destaque, QR Code, imagem de 1 bit; 10.1) e o DANFCE no leiaute.
- **7.3. DANFE.** O DANFE no leiaute, com as seções do manual.
- **7.4. PDF.** `OpenDFe.Impressao.Pdf`, conferido contra o manual.

### Fase 8: distribuição DF-e

- **8.1. Distribuição.** `DistribuicaoDFe` por último NSU, NSU e chave, destino AN, os documentos compactados (resumo, nota, evento) devolvidos como XML.
- **8.2. Manifestação e cadastro.** Os eventos de manifestação (210200, 210210, 210220, 210240) com destino AN, e a `ConsultaCadastro`.

## Publicação

Pré-lançamentos (`0.x`) no NuGet.org a partir do fim da fase 5, quando o PDV precisa da lib; a `1.0` quando a fase 8 fechar. **A confirmar** antes da primeira publicação.

## Situação

Nenhuma etapa começou.
