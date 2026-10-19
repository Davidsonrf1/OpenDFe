# Plano de implementação — OpenDFe

O modelo é o `docs/PROJETO.md`; onde este plano e o documento divergirem, vale o documento. **Aprovado em 2026-10-19.**

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

- **7.1. QuestPDF.** Decidido (10.2). Página de teste com código de barras e QR Code, o README do pacote com a licença e o erro de licença não definida apontando para ele.
- **7.2. Modelo de impressão e DANFCE.** O modelo neutro no núcleo (blocos, seções, destaque, QR Code, imagem de 1 bit; 10.1) e o DANFCE no leiaute.
- **7.3. DANFE.** O DANFE no leiaute, com as seções do manual.
- **7.4. PDF.** `OpenDFe.Impressao.Pdf`, conferido contra o manual.

### Fase 8: distribuição DF-e

- **8.1. Distribuição.** `DistribuicaoDFe` por último NSU, NSU e chave, destino AN, os documentos compactados (resumo, nota, evento) devolvidos como XML.
- **8.2. Manifestação e cadastro.** Os eventos de manifestação (210200, 210210, 210220, 210240) com destino AN, e a `ConsultaCadastro`.

## Publicação

Pré-lançamentos (`0.x`) no NuGet.org a partir do fim da fase 5, quando o PDV precisa da lib; a `1.0` quando a fase 8 fechar. **A confirmar** antes da primeira publicação.

## Situação

### Fase 0 (branch `fase-0-repositorio`)

**0.1. Solução. Feita (2026-10-19).**
- `OpenDFe.slnx` com os quatro projetos de `src/` e os três de `tests/`, vazios; referências: o modelo, o leiaute e o PDF dependem do núcleo; o leiaute, do modelo; `NFe.Testes` e `Homologacao.Testes`, do leiaute.
- `global.json`: SDK 10.0.100 com `latestFeature` (nesta máquina, 10.0.401) e o runner de testes `Microsoft.Testing.Platform`.
- `Directory.Build.props` (raiz): `net10.0`, nullable, avisos como erro, `AnalysisLevel` `latest-recommended` com o estilo no build, determinístico, metadados do pacote (MIT, autor). `src/Directory.Build.props`: documentação XML (sem comentário, não compila), `PublicApiAnalyzers` com `PublicAPI.Shipped.txt`/`Unshipped.txt` por projeto (conferido: tipo público não declarado é erro RS0016), validação de pacote, símbolos `.snupkg`, o README no pacote. SourceLink vem do SDK.
- Testes: xUnit v3 4.x na Microsoft Testing Platform; o projeto de teste é executável (`OutputType Exe`) e não leva `Microsoft.NET.Test.Sdk` nem `xunit.runner.visualstudio`. `AceitaNenhumTeste` (em `tests/Directory.Build.targets`) aceita o código 8 (nenhum teste rodou): hoje nos três projetos; **sai do `Nucleo.Testes` e do `NFe.Testes` quando entrar o primeiro teste** e fica para sempre no `Homologacao.Testes`.
- `.gitattributes` com LF em tudo e `tests/Exemplos/** -text` (os bytes dos XMLs assinados não podem mudar); `.editorconfig`; `.gitignore` com certificados e pacotes; `LICENSE` (MIT); README com a primeira linha de 14.4, os pacotes, os requisitos (2.8) e a propriedade (14.3).
- Conferido: `dotnet build` sem aviso e `dotnet test` com código 0 (build com `--artifacts-path` fora do repositório).
- **Fica para a 0.2:** o `OpenDFe.NFe` com as duas DLLs (hoje o modelo e o leiaute empacotariam separados), `RepositoryUrl` e `PackageProjectUrl` (quando o repositório do GitHub existir) e a CI.

**0.2. CI e pacotes. Feita (2026-10-19).**
- O `OpenDFe.Leiaute.NFe.v400` sai como pacote `OpenDFe.NFe` (`PackageId`), com o modelo por `ProjectReference` `PrivateAssets="all"` e o alvo `IncluirModeloNoPacote` (`TargetsForTfmSpecificBuildOutput`): a DLL e o XML do modelo em `lib/`, o PDB no `.snupkg`; o `OpenDFe.Nucleo` continua dependência de pacote. O modelo tem `IsPackable` e `EnablePackageValidation` desligados. No `pack --no-build`, o leiaute não recompila as referências (`BuildProjectReferences` falso com `NoBuild`; sem isso, NETSDK1085).
- `Directory.Build.props`: `VersionPrefix` 0.1.0 (0.x até a fase 8), `RepositoryUrl` e `PackageProjectUrl` (`https://github.com/Davidsonrf1/OpenDFe`, público).
- `.github/workflows/ci.yml`: `ubuntu-latest` e `windows-latest` (sem `fail-fast`), SDK pelo `global.json`; restore, build, `dotnet test --solution` (os de homologação passam vazios), `pack --no-build` e `eng/ConferirPacotes.ps1`, que abre cada `.nupkg`/`.snupkg` e confere o `lib/` contra o esperado (os três pacotes, nenhum a mais; o `OpenDFe.NFe` com as duas DLLs). Os pacotes do Ubuntu sobem como artefato. Roda em push no `main` e nas `fase-*`, e em PR.
- Conferido localmente (Windows), na sequência da CI: build sem aviso, testes com código 0, os três pacotes e o script; o script falha (código 1) com os `.snupkg` ausentes. **A CI no GitHub ainda não rodou:** sobe com o push da branch, que é do usuário. O "pronto quando" da fase 0 (a CI vazia nos dois sistemas) fecha nessa primeira execução.
- **Do usuário, pendente:** o push e a reserva do prefixo `OpenDFe.*` no NuGet.org (14.1, antes da primeira publicação).

**Próxima: 1.1. Tipos base** (fase 1, branch `fase-1-nucleo` a partir desta). Ao entrar o primeiro teste no `Nucleo.Testes`, tirar o `AceitaNenhumTeste` dele.

### Fase 1 (branch `fase-1-nucleo`)

**1.1. Tipos base. Feita (2026-10-19).**
- No `OpenDFe.Nucleo`, todos no namespace `OpenDFe` (`RootNamespace`; o `Nucleo.Criar` de 3.2 pede a classe `Nucleo`, que colidiria com um namespace `OpenDFe.Nucleo`):
  - `UF`: enumeração com o código IBGE como valor (`MG = 31`) e a sigla como nome, as 27; `UnidadesFederativas` com `Sigla(this UF)`, `Codigo(this UF)`, `TentarPelaSigla(string?, out UF)` (só maiúsculas) e `TentarPeloCodigo(int, out UF)`. AN, SVRS e afins não são UF: ficam para o `Destino` (1.6).
  - `Ambiente` (`Producao = 1`, `Homologacao = 2`), `TipoEmissao` (`Normal = 1`, `ContingenciaFSIA = 2`, `ContingenciaEpec = 4`, `ContingenciaFSDA = 5`, `ContingenciaSvcAN = 6`, `ContingenciaSvcRS = 7`, `ContingenciaOffline = 9`) e `Documento` (`NFe = 55`, `NFCe = 65`): o valor de cada membro é o do XML (`tpAmb`, `tpEmis`, `mod`; 4.3).
  - `Cpf` e `Cnpj`, classes estáticas: `Valido(string?)`, `SemMascara(string)`, `Formatar(string)`, `CalcularDigitos(string base)` e a constante `Tamanho`. O CNPJ aceita as 12 primeiras posições com dígitos ou letras maiúsculas e os DVs numéricos, por módulo 11 sobre o ASCII − 48 (conferido com o exemplo da Receita, `12.ABC.345/01DE-35`); o módulo 11 com pesos de 2 a 9 (`Modulo11`, interno) é o mesmo que a chave de acesso vai usar na 1.2.
  - `Problema` (classe selada: `Codigo`, `Severidade` e `Mensagem` no construtor; `Tag`, `Caminho` e `Propriedade` opcionais, `init`) e `Severidade` (`Aviso = 1`, `Erro = 2`).
  - `FormatadorInvariavel`: `Numero(decimal, casas)`, `DataHora(DateTimeOffset)` (`yyyy-MM-ddTHH:mm:ss±hh:mm`, o deslocamento do próprio valor) e `Data(DateOnly)`, sempre com a cultura invariável.
- `Nucleo.Testes` com 168 testes (CPF e CNPJ válidos e inválidos, inclusive alfanuméricos e o exemplo da Receita; formatação; os códigos IBGE das 27 UFs; os valores do XML; o formatador em pt-BR, th-TH, ar-SA e de-CH). O `AceitaNenhumTeste` saiu dele.
- **Decisões tomadas sem perguntar** (a etapa rodou sem o usuário; sempre a menor superfície pública, fácil de estender em versão menor). A conferir pelo usuário:
  - UF, ambiente, tipo de emissão, documento e severidade como `enum` com o valor do XML, sem tipo próprio para "valor do XML" (o leiaute escreve o inteiro).
  - `TipoEmissao` sem o `3` (Regime Especial da Nota Fiscal Fácil, NT 2021.002), que só algumas UFs adotam e é da NF-e gerada pelo aplicativo NFF; entra em versão menor se for preciso. A leitura de um XML com `tpEmis=3` fica para a 2.5.
  - `Cpf` e `Cnpj` como funções estáticas sobre `string`, não como tipos de valor; o modelo guarda o documento como texto.
  - Máscara aceita só inteira (`12.ABC.345/01DE-35`) ou nenhuma; letras minúsculas, espaços e máscara parcial são recusados (relaxar depois não quebra ninguém). Documento com todas as posições iguais (`111.111.111-11`, `00000000000000`) é inválido, embora passe no cálculo.
  - `Formatar` e `SemMascara` lançam `ArgumentException` com documento inválido (erro de programação: quem chama confere antes com `Valido`).
  - `Problema.Codigo` é texto (as regras do MOC usam o número da rejeição, mas o schema e os callbacks precisam de outros códigos). As restrições do schema (6.4) ainda não estão no `Problema`: entram na 1.5, como propriedade nova.
  - Severidade só com `Aviso` e `Erro` (o "bloqueante" de 8.2 é o `Erro`).
  - O formatador **nunca arredonda**: valor com mais casas significativas que o campo sai com todas, e o schema aponta o campo (2.4: o problema volta como resultado, e o valor não muda calado). Zeros à direita além das casas são descartados. Frações de segundo da data e hora são descartadas. O método é `Numero`, não `Decimal` (CA1720: identificador com nome de tipo).
- Conferido: build sem aviso, `dotnet test --solution OpenDFe.slnx` com código 0 (168 testes), `dotnet pack -c Release` e `eng/ConferirPacotes.ps1` aprovando os três pacotes (com `--artifacts-path` fora do repositório).

**1.2. Chave de acesso. Feita (2026-10-19).**
- `ChaveAcesso` (classe selada e imutável, no namespace `OpenDFe`): `Gerar(uf, emissao, emitente, modelo, serie, numero, tipoEmissao)` (o `cNF` sorteado por `RandomNumberGenerator`), `Montar(..., codigoNumerico)` (o `cNF` conhecido, o do XML), `TentarLer(string?, out ChaveAcesso?)`, `Ler(string)`, as partes (`UF`, `Ano` com quatro dígitos, `Mes`, `Emitente`, `Modelo`, `Serie`, `Numero`, `TipoEmissao`, `CodigoNumerico`, `DigitoVerificador`), a constante `Tamanho` (44), `ToString()` com as 44 posições e igualdade de valor (`IEquatable`, `==`, `!=`) pelas 44 posições. O DV é o `Modulo11` interno da 1.1, sobre as 43 posições com o ASCII − 48. O `Nucleo` passou a ter `InternalsVisibleTo` para o `Nucleo.Testes` (o teste do cálculo do DV com o exemplo do manual e o sorteio substituível do `cNF`).
- **Conferido na fonte** (2026-10-19): o leiaute da chave, o DV e o CPF na chave no MOC 7.0, Visão Geral, 2.2.6 (e o exemplo do DV, soma 644, DV 5); o CPF precedido de zeros e as séries do emitente CPF na NT 2023.002, 2.1 e 4.1 (890-899, 910-969, NFF por `tpEmis=3`); a regra do `cNF` no MOC 7.0, Anexo I, B09-20 (rejeição 897, NT 2019.001: não pode ser `00000000` a `99999999` com um dígito repetido, nem as dez sequências crescentes circulares `12345678` ... `01234567`, nem igual ao `nNF`); o CNPJ alfanumérico na chave na NT 2025.001, item 5 (expressão `[0-9]{6}[A-Z0-9]{12}[0-9]{26}`, DV com o ASCII − 48 nas 44 posições, implantação em 06/04/2026 na homologação e 06/07/2026 na produção). Os manuais baixados do portal da SVRS; a NT 2025.001 e a NT 2023.002, de cópias públicas do PDF. `docs/NTS.md` ainda não existe: nasce na 2.1, com o pacote de schemas.
- `Nucleo.Testes` com 207 testes (39 novos): as chaves de exemplo do MOC 7.0 (Visão Geral 2.2.10.3, NF-e do PR; Anexo I, ZX02, NFC-e do RS) lidas e montadas de volta, o DV do exemplo de 2.2.6.2, ida e volta nas 27 UFs, cada DV errado recusado, a chave inválida por cada motivo (com o DV recalculado, para recusar só pelo motivo do caso), o CNPJ alfanumérico (o exemplo da Receita, com o DV conferido à parte pela regra da NT), o emitente CPF, 10.000 gerações sem o `cNF` igual ao número nem recusado, o sorteio repetido com um sorteador falso, e a igualdade.
- **Decisões tomadas sem perguntar** (menor superfície pública, fácil de estender em versão menor). A conferir pelo usuário:
  - **Classe selada, não `struct`:** o `default` de uma `struct` pública seria uma chave inválida que ninguém consegue impedir, e o "sem chave" do modelo é `null` (`ChaveAcesso?`). Não é `record`: o `record` publica `with`, `Deconstruct`, `EqualityContract` e `PrintMembers`, superfície que não se tira depois. A igualdade de valor entra agora porque acrescentá-la depois mudaria o `==` de quem já usa (de referência para valor).
  - **`Emitente` devolve as 14 posições como estão**, sem dizer se é CPF ou CNPJ: a chave sozinha não decide (`000` + CPF pode também ser um CNPJ válido; a série 890-899 aceita os dois). Quem precisa sabe pelo XML.
  - **A leitura confere** tamanho, caracteres (letras maiúsculas só nas 12 primeiras posições do CNPJ, como a expressão da NT), UF, mês de 1 a 12, CNPJ válido ou `000` + CPF válido, modelo conhecido (55, 65), número diferente de zero, tipo de emissão conhecido e DV. **Não confere** as regras do `cNF` (a B09-20 é regra da nota; o Anexo I do MOC 7.0 a aplica aos modelos 55 e 65, e resumos de terceiros dizem que a NT 2019.001 v1.40 a tirou do 65: conferir na fase 3), nem a série do emitente CPF (rejeição 495): são regras do MOC, da fase 3. O ano é sempre 2000 + `AA`.
  - **Tipo de emissão fora do `TipoEmissao` é recusado**, inclusive o `3` (NFF, decisão da 1.1) e o `0` das chaves do leiaute 1.10 (2006 a 2010, com o `cNF` de 9 dígitos e sem `tpEmis`). Aceitá-los depois é versão menor; a leitura de XML antigo é da 2.5.
  - **`Gerar` sempre evita** o número e as 20 sequências, nos dois modelos: custa nada e não depende de qual versão da NT a SEFAZ aplica. `Montar` não confere o `cNF`, para reproduzir a chave de um XML existente.
  - `Gerar` e `Montar` recebem a emissão como `DateTimeOffset` (o `dhEmi`), valendo o ano e o mês no deslocamento do próprio valor; ano de 2000 a 2099. O `RandomNumberGenerator` estático não fere o 2.3 (sem estado, sem I/O). Argumento inválido lança `ArgumentOutOfRangeException` (UF, modelo, tipo de emissão, faixas) ou `ArgumentException` (emitente), e `Ler` lança `ArgumentException`, como os `SemMascara` da 1.1.
  - Sem `Valida(string)` (é `TentarLer(valor, out _)`), sem formatação da chave em grupos de 4 (é do DANFE, fase 7), sem `CalcularDigito` público, sem aceitar o prefixo `NFe` do `Id` (o leiaute o tira).
- Conferido: build sem aviso, `dotnet test --solution OpenDFe.slnx` com código 0 (207 testes), `dotnet pack -c Release` e `eng/ConferirPacotes.ps1` aprovando os três pacotes (com `--artifacts-path` fora do repositório).

**Próxima: 1.3. Certificado.**
