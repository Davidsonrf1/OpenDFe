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
  Compartilhado/                     fonte comum aos testes (o gerador do certificado de teste, 13.1)
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
- **1.7. Prova de risco.** Envelope SOAP 1.2 mínimo, `HttpClient` com o certificado do cliente e as ACs raiz da ICP-Brasil embutidas (`CustomRootTrust`), uma chamada de `NFeStatusServico4` em homologação de MG, com o pedido assinado em SHA-1 para provar o item 1 dos riscos mesmo sem o serviço exigir. Rodada no Windows e no Linux (a distribuição do PDV). Precisa do PFX A1 de homologação (variável de ambiente). **É aqui que entra o certificado real:** até a 1.6, todos os testes usam o autoassinado da 1.3; a 1.7 confere a abertura e a leitura do titular com o PFX ICP-Brasil de verdade (inclusive o perfil da versão 8.2 do DOC-ICP-04 em diante, com o documento no `serialNumber`). O código dessa etapa é descartável: a fase 4 o refaz com o desenho definitivo, aproveitando o que a prova ensinar.

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

**1.3. Certificado. Feita (2026-10-19).**
- **Por instrução do usuário, o certificado é autoassinado:** não há PFX ICP-Brasil disponível. O gerador de teste produz o autoassinado com os campos da ICP-Brasil, e todos os testes desta etapa (e os da 1.4 em diante) trabalham com ele. A abertura e a leitura do titular não exigem a cadeia ICP-Brasil; a cadeia é da 1.7 e da 4.1. **O certificado real fica para quando houver o PFX de homologação (1.7).** `PROJETO.md` 13.1 diz isso agora.
- No `OpenDFe.Nucleo` (namespace `OpenDFe`):
  - `CertificadoDigital`, classe estática: `AbrirPfx(byte[] conteudo, string senha)` e `AbrirArquivoPfx(string caminho, string senha)` → `X509Certificate2`; `Validade(X509Certificate2, DateTimeOffset instante)` → `ValidadeCertificado`; `LerTitular(X509Certificate2)` → `TitularCertificado?`. Interno: `ExigirChavePrivadaRsa`, para o `Nucleo.Criar` da 1.6 conferir o certificado recebido na configuração.
  - `TitularCertificado` (classe selada, construtor interno): `Nome`, `Cnpj` e `Cpf` (exatamente um dos dois preenchido, sem máscara).
  - `ValidadeCertificado`: `Vigente = 1`, `AindaNaoVigente = 2`, `Vencido = 3`.
- **Gerador de teste:** `tests/Compartilhado/CertificadoDeTeste.cs`, fonte incluída pelo projeto de teste (`<Compile Include="../Compartilhado/**/*.cs" />`; o `NFe.Testes` e o `Homologacao.Testes` acrescentam a mesma linha quando precisarem). Autoassinado, RSA 2048 (ou ECDSA P-256, para o teste da recusa), `KeyUsage` crítico com `digitalSignature`, `nonRepudiation` e `keyEncipherment`, `BasicConstraints` com `cA=false`, EKU de autenticação de cliente e de e-mail, e o "Subject Alternative Name" com um `rfc822Name` e os `otherName`: no e-CNPJ, 2.16.76.1.3.4 (nascimento, CPF, NIS e RG do responsável), 2.16.76.1.3.2 (nome do responsável), 2.16.76.1.3.3 (CNPJ) e 2.16.76.1.3.7 (CEI); no e-CPF, 2.16.76.1.3.1 (nascimento, CPF, NIS e RG do titular) e 2.16.76.1.3.6 (CEI); em `OCTET STRING` ou `PrintableString`. O titular com o CN `<nome>:<documento>` (perfil até a 8.1) ou só o nome, e o `serialNumber` com o documento (8.2 em diante), à escolha; validade configurável (o padrão é de ontem a daqui a um ano), inclusive vencido e ainda não válido; exporta o PFX com senha, com e sem a chave privada. Os valores não são conferidos, para os testes da leitura com campo inválido.
- **Conferido na fonte** (2026-10-19): DOC-ICP-04 versão 8.3 (Resolução CG ICP-Brasil nº 179, compilada com as nº 211, de 31/10/2024, e nº 212, de 30/01/2025; PDF do gov.br/iti): o 7.1.2.2 (o `otherName` em `OCTET STRING` ou `PRINTABLE STRING`; zeros quando o número não está disponível; só A a Z e 0 a 9, o que comporta o CNPJ alfanumérico nas 14 posições do 2.16.76.1.3.3) e o Anexo I, incluído pela Resolução 211: o titular com `CN = <Nome Civil>` ou `<Razão Social>` e `serialNumber` (2.5.4.5) `= <número CPF>` ou `<número CNPJ>` (ETSI EN 319 412-2 e 412-3), e o "Subject Alternative Name" com o 2.16.76.1.3.1 (8 posições da data de nascimento, `ddmmaaaa`, 11 do CPF, 11 do NIS, 15 do RG, até 10 do órgão expedidor e UF) no e-CPF e o 2.16.76.1.3.3 (as 14 posições do CNPJ) no e-CNPJ, **obrigatórios "com restrição" até 31/12/2028 e opcionais depois**. A versão 8.3 não lista mais o 2.16.76.1.3.4, o .2 e o .7 no e-CNPJ: o formato deles (o 2.16.76.1.3.4 com o mesmo leiaute do .1, para o responsável; o .2, o nome do responsável; o .7, o CEI em 12 posições) e o CN `<nome>:<documento>` foram conferidos numa política de certificado publicada (PC da AC Fecomercio MG RFB A1, v1.1, no repositório do Serpro), que reproduz o 7.1.2.3 das versões anteriores; as versões 6.x e 7.x do DOC-ICP-04 não estavam acessíveis no site do ITI. Do .NET 10 (documentação da Microsoft): `X509CertificateLoader.LoadPkcs12` devolve o primeiro certificado com a chave privada (ou o primeiro, se nenhum tem) e lança `CryptographicException` (e `Pkcs12LoadLimitExceededException`, derivada dela).
- **De memória, não conferido em fonte:** que o SChannel (TLS no Windows) não usa a chave importada com `EphemeralKeySet` (o motivo do `DefaultKeySet`, abaixo); que no Linux a chave do PFX fica em memória com qualquer flag; o formato exato do `serialNumber` nos certificados reais da 8.2 em diante (o Anexo I diz só `<número CPF>`; a ETSI EN 319 412-1, 5.1.3, tem a forma com prefixo, `IDCBR-...`, que a leitura hoje **não** aceita). Os três ficam para a 1.7, com o PFX real, nos dois sistemas.
- **Decisões tomadas sem perguntar** (menor superfície pública, fácil de estender em versão menor). A conferir pelo usuário:
  - **Funções estáticas sobre o `X509Certificate2`, sem tipo próprio de certificado** (nem invólucro `IDisposable`): a lib recebe o `X509Certificate2` (7.3, 3.2), e um invólucro criaria duas formas de passar o certificado e uma dúvida de posse. **A posse é de quem chama:** `AbrirPfx` devolve um certificado novo, que quem chama descarta; as funções que recebem um certificado não o descartam nem o guardam.
  - **Do `X509Certificate2` já carregado** (repositório do Windows, A3) não há função de "abrir": ele já é o que a lib usa, e `LerTitular` e `Validade` valem para ele. A conferência da chave privada RSA é interna agora e entra na 1.6, no `Nucleo.Criar`.
  - **Erros de abertura como `CryptographicException`**, com a mensagem em português e a exceção do .NET como `InnerException`: o conteúdo que não é PFX (conferido pela estrutura de fora, RFC 7292, para separar de "senha errada", que o .NET não distingue), a senha errada ou o PFX corrompido (uma mensagem só: sem o pacote `System.Security.Cryptography.Pkcs`, que não é dependência, não dá para conferir o MAC à parte), o limite de leitura do .NET, o PFX sem chave privada, a chave que não é RSA. Não é `ArgumentException` (a decisão da 1.1 vale para o erro de programação; certificado inválido é falha de certificado, `PROJETO.md` 2.4). Uma exceção própria, derivada de `CryptographicException` e com o motivo, entra depois sem quebrar ninguém. Arquivo inexistente: a `IOException` do .NET, como vem.
  - **`X509KeyStorageFlags.DefaultKeySet`, sem parâmetro de flags:** no Windows, o TLS mútuo com a SEFAZ (SChannel) precisa da chave num contêiner, o que o `EphemeralKeySet` não dá; o contêiner é temporário, no perfil do usuário, e sai no `Dispose` (daí a posse explícita). Nada de `PersistKeySet` (deixaria a chave no disco) nem de `MachineKeySet` (pede permissão de máquina). Quem roda sem perfil carregado (serviço do Windows) carrega o PFX por conta própria, com as flags que quiser, e passa o `X509Certificate2`. No Linux as flags não mudam nada (a chave fica em memória). Um parâmetro de flags entra depois como sobrecarga.
  - **`AbrirPfx` não confere a validade:** o certificado vencido abre, para ser lido (o PDV mostra "vencido em ..."); quem confere é `Validade`. A senha é `string` não nula (vazia, para o PFX sem senha); relaxar para `string?` depois não quebra.
  - **`Validade` recebe o instante (`DateTimeOffset`), não o `TimeProvider`:** é uma função pura, sem relógio escondido (2.3); quem tem o `TimeProvider` passa `GetUtcNow()`, e a verificação de um XML recebido (1.4) confere na data da emissão. Os dois extremos (`notBefore`, `notAfter`) são vigentes (RFC 5280, 4.1.2.5).
  - **Titular ausente é `null`, não erro:** o certificado sem os campos (ou com zeros, dígito errado, máscara, tamanho errado) é lido, e quem precisa do CNPJ decide. A ordem: o `otherName` 2.16.76.1.3.3 (CNPJ), depois o 2.16.76.1.3.1 (o CPF nas posições 9 a 19), depois o `serialNumber` do titular (perfil 8.2 em diante); o `otherName` vale mais que o `serialNumber` se divergirem, e o e-CNPJ é da pessoa jurídica mesmo com o CPF do responsável (o 2.16.76.1.3.4 não é lido). A extensão malformada é ignorada, sem exceção. O `Nome` sai do CN, sem o sufixo `:<documento>` quando ele é o documento lido; vazio se não há CN.
  - **O gerador de teste fica fora da API** (13.1 não pede que seja público): fonte compartilhada, não projeto; público só dentro do assembly de teste, que não é pacote.
- `Nucleo.Testes` com 249 testes (42 novos): o PFX gerado abre (com senha, sem senha, de bytes e de arquivo temporário apagado no fim), a chave aberta assina e verifica, a senha errada, o conteúdo que não é PFX (vazio, lixo, versão errada, o certificado em DER), o PFX sem chave privada, a chave ECDSA, o vencido e o ainda não vigente abrem, os argumentos nulos, a validade nos extremos e em outro fuso, o titular CNPJ (numérico e alfanumérico, em `OCTET STRING` e em `PrintableString`, com o CPF do responsável junto), o CPF, o perfil novo (só o `serialNumber`), o CN com `:` que não é o documento, o certificado sem os campos (`null`), os campos inválidos (zeros, dígito errado, 13 posições, máscara) e o certificado sem chave privada (o de um XML recebido).
- Conferido (Windows): build sem aviso, `dotnet test --solution OpenDFe.slnx` com código 0 (249 testes), `dotnet pack -c Release` e `eng/ConferirPacotes.ps1` aprovando os três pacotes (com `--artifacts-path` fora do repositório). **Só a CI no Linux confirma** os 42 testes no OpenSSL 3 do `ubuntu-latest` (a abertura com `DefaultKeySet`, o PFX exportado pelo `Export` do gerador, a senha errada vindo como `CryptographicException`). **Fica para a 1.7, com o PFX real:** o PFX das ACs cifrado com RC2 ou 3DES no OpenSSL 3 (o RC2 está no provedor *legacy*), o `DefaultKeySet` no TLS mútuo do Windows e o `serialNumber` dos certificados novos.

**Próxima: 1.4. Assinatura e verificação.**
