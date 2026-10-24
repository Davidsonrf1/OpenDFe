# Etapas restantes: regras comuns

Este arquivo vale para **todas** as etapas de `docs/etapas/`. Cada etapa é executada por **um agente isolado, sem supervisão**, que lê este arquivo, o arquivo da etapa e os documentos que o arquivo da etapa manda ler. O agente não pergunta nada ao usuário durante a execução: onde o arquivo não decide, vale a regra "Decisão não coberta" (seção 6).

**Aprovação do usuário (2026-10-24):** o usuário pediu o detalhamento de todas as etapas que faltam, mandou **aceitar sempre a recomendação** e mandou testar com **certificado autoassinado local e a SEFAZ emulada**. Portanto:

- todas as decisões de desenho escritas nos arquivos de etapa estão **aprovadas** (o "desenho para aprovação" do `PLANO.md` está cumprido: o agente só registra a aprovação, com esta data);
- as **regras fora do schema** listadas em cada arquivo de etapa estão **aprovadas** (regra do `PROJETO.md` 2.9). Regra fora do schema que **não** está listada **não se implementa**: vai para "Pendências para o usuário" na situação;
- o "pronto quando" de cada fase vale **contra a SEFAZ emulada** (`PROJETO.md` 17); a homologação real fica como pendente do PFX.

## 1. Ordem, branches e dependências

As etapas se executam **nesta ordem, uma por agente, cada uma só depois que a anterior terminou e foi commitada**:

| Ordem | Etapa | Arquivo | Branch |
|---|---|---|---|
| 1 | 5.1 QR Code e CSC | `5.1-qrcode-csc.md` | `fase-5-nfce` (existe) |
| 2 | 5.2 Contingência offline | `5.2-contingencia-offline.md` | `fase-5-nfce` |
| 3 | 6.1 Eventos: base, cancelamento, inutilização | `6.1-eventos-base-cancelamento-inutilizacao.md` | `fase-6-eventos` (criar a partir da `fase-5-nfce`) |
| 4 | 6.2 Substituição e carta de correção | `6.2-substituicao-carta-correcao.md` | `fase-6-eventos` |
| 5 | 7.1 QuestPDF | `7.1-questpdf.md` | `fase-7-impressao` (criar a partir da `fase-6-eventos`) |
| 6 | 7.2 Modelo de impressão e DANFCE | `7.2-modelo-impressao-danfce.md` | `fase-7-impressao` |
| 7 | 7.3 DANFE | `7.3-danfe.md` | `fase-7-impressao` |
| 8 | 7.4 PDF | `7.4-pdf.md` | `fase-7-impressao` |
| 9 | 8.1 Distribuição DF-e | `8.1-distribuicao-dfe.md` | `fase-8-distribuicao` (criar a partir da `fase-7-impressao`) |
| 10 | 8.2 Manifestação e consulta cadastro | `8.2-manifestacao-consulta-cadastro.md` | `fase-8-distribuicao` |

A cadeia é linear de propósito (sem merge entre as branches de fase). **Não há mais de um agente ao mesmo tempo no repositório.**

## 2. Antes de começar (todo agente)

1. `cd C:\Projetos\OpenDFe` (repositório próprio, separado do `ErpCore`; não toque em nada fora dele, salvo o que o arquivo da etapa disser). Ler, nesta ordem: este arquivo, o arquivo da etapa, a seção "Como trabalhar" e a "Situação" do fim da fase em `PLANO.md`, e os documentos do "Ler antes" do arquivo da etapa.
2. `git status` e `git branch --show-current`. **A árvore tem trabalho do usuário sem commit** (a persistência do emulador: `tests/OpenDFe.SefazEmulada/Armazenamento.cs`, `Properties/`, `tests/OpenDFe.SefazEmulada.Testes/PersistenciaTestes.cs`, `docs/OpenDFe.code-workspace` e modificações em `tests/OpenDFe.SefazEmulada/*` e `docs/SEFAZ_EMULADA.md`, mais a linha `Microsoft.Data.Sqlite` em `Directory.Packages.props`). **Não descarte, não reverta, não commite isso.** Os commits da etapa usam `git add <caminhos>` explícitos, nunca `git add -A` nem `git commit -a`. Exceção única: se a etapa precisa editar `Directory.Packages.props` (7.1), o commit leva o arquivo inteiro, com a linha do `Microsoft.Data.Sqlite` dentro (inofensiva), e a situação registra isso. Se o usuário já tiver commitado esse trabalho, ótimo; nada muda.
3. Conferir que a etapa anterior da lista da seção 1 está na "Situação" do `PLANO.md` como **Feita** e commitada (`git log --oneline -5`). Se não estiver, **pare** (seção 7).
4. Criar a branch da fase, quando a etapa for a primeira dela (`git switch -c fase-6-eventos`, etc., a partir da branch anterior). Se `git log HEAD..main --oneline` não for vazio, `git merge main` no início da fase (como nas fases 3 e 4); conflito só se resolve se for trivial (`PLANO.md`, `OpenDFe.slnx`, `Directory.Packages.props`); senão, pare.
5. Build e testes **antes** de mexer: `dotnet build OpenDFe.slnx -c Debug` e `dotnet test --solution OpenDFe.slnx`. Anote as contagens (a 4.5 fechou com 1.754 testes e 18 pulados; a 5.1 e as seguintes conferem o número que a situação da anterior registrou). Se já falha antes de mexer, **pare** e registre (a falha não é sua, e consertá-la mistura o trabalho).

## 3. O que toda etapa entrega

- **Código** em `src/` e testes em `tests/`, no estilo do código vizinho: mesma densidade de comentário (XML em português em todo membro público, comentários só onde o porquê não é óbvio), mesma estrutura de arquivo, mesmo idioma de teste. Olhe a etapa 4.4 e a 4.5 (`OperacaoAutorizacao.cs`, `OperacaoConsultaProtocolo.cs`, `AutorizacaoTestes.cs`, `AutorizacaoEmuladaTestes.cs`, `QuedaEmuladaTestes.cs`) como modelo de como uma operação nova nasce, é testada com um handler falso e depois contra o emulador.
- **API pública rastreada:** todo membro público novo entra em `PublicAPI.Unshipped.txt` do projeto (o analisador `PublicApiAnalyzers` quebra o build; use a correção sugerida ou copie as linhas que ele mostra). Nada é removido do `Shipped.txt`.
- **Documentação:**
  - `docs/CONTRATOS.md`: a seção da etapa (o número sugerido está no arquivo da etapa; confira o próximo livre) com o desenho **como foi implementado**, marcando "Aprovado em 2026-10-24, todas as decisões pela recomendação" e, no fim, "Implementação" com os **ajustes ao desenho** (o que mudou e por quê). Etapa sem API pública nova ainda assim registra o que mudou.
  - `docs/REGRAS.md`: cada regra implementada muda a situação no catálogo (a coluna de situação, como as 4.4 e 4.5 fizeram); o `CatalogoRegrasTestes` cruza o catálogo com o código.
  - `docs/NTS.md`: NT, manual ou tabela conferida na etapa (nome, versão, data, SHA-256 do arquivo baixado), URLs conferidas.
  - `docs/PROJETO.md`: onde o desenho divergir do texto dele, atualize o texto (e liste a divergência na situação).
  - `docs/QUANDO_CAI.md` ou guia novo, quando o arquivo da etapa mandar.
  - `docs/SEFAZ_EMULADA.md`: só se o emulador mudar (seção 5).
- **`PLANO.md`, "Situação":** um bloco da etapa no formato da 4.4/4.5 (seção 8 deste arquivo), trocando a linha "Próxima:" no fim da fase.
- **Um commit ao fim**, sem push (seção 9).

## 4. Regras do projeto que valem sempre (resumo; o texto está em `PLANO.md` e `PROJETO.md`)

- **Nomes sem preposição nem contração** nos identificadores (`RegraNFe`, nunca `RegrasDaNFe`): `De`, `Da`, `Do`, `Das`, `Dos`, `Para`, `Pelo`, `Pela`, `Por`, `Com`, `Sem`, `No`, `Na`, `Nos`, `Nas`, `Em`, `Ao`. Vale para tipos, membros, parâmetros, variáveis, campos, constantes, propriedades MSBuild e nomes de arquivo `.cs`; não vale para os nomes dos métodos de teste (frases) nem para texto em português. Conferência no fim: `grep -rhoE "\b[A-Za-z_]*[a-z](De|Da|Do|Das|Dos|Para|Pelo|Pela|Por|Com|Sem|No|Na|Nos|Nas|Em|Ao)[A-Z][A-Za-z]*\b" src tests eng` mais os que começam pela preposição (`SemMascara`), descontados os métodos de teste e o `NoBuild`/`NoCheck`; não pode sobrar nada novo seu.
- **Validação: schema primeiro, regra à mão só a aprovada.** Toda regra fora do schema tem código do MOC (ou `NFE-...` da lib), severidade e linha no catálogo (`REGRAS.md`).
- **Sem estado global, sem I/O fora do transporte, sem reenvio automático** (`PROJETO.md` 2.3, 8.6). A lib não grava nada.
- **A lib não calcula tributo nem decide fiscalmente** (`PROJETO.md` 1.1).
- **Nada de dado pessoal em log/telemetria/mensagem de exceção** (texto digitado, e-mail, documento): mensagens de erro citam o campo, não o valor pessoal. O CSC nunca aparece em `ToString`, mensagem, rastro de teste ou log.
- **Concorrência:** operações e leiaute sem estado, seguros em paralelo.
- **`net10.0`, nullable, avisos como erro, documentação XML obrigatória**; `Directory.Build.props` manda. Build sem aviso em Debug **e** Release.
- **Não há `Protegidos/`** neste repositório; a regra de testes protegidos do `ErpCore` não se aplica aqui. Mesmo assim: **nunca altere um teste existente para fazê-lo passar**. Se um teste antigo falha por causa da sua mudança, o certo é o código; só mude o teste quando a mudança de comportamento é a própria etapa (a situação diz qual teste mudou e por quê).

## 5. Como testar (certificado autoassinado e SEFAZ emulada)

- **Certificado:** o autoassinado gerado na hora por `tests/Compartilhado/CertificadoTeste.cs` (RSA 2048, campos de titular da ICP-Brasil, CNPJ ou CPF configurável). Nenhum PFX real, nenhum segredo no repositório.
- **SEFAZ emulada:** `tests/OpenDFe.SefazEmulada`, usada **por dentro dos testes** (`Emulador.IniciarAsync`, porta livre, sem registrar nada), com as URLs dela trocadas pela configuração (`EnderecoServico`) e a AC dela confiada pela fábrica de teste `tests/Compartilhado/ConexaoTeste.cs`. O modelo é `tests/OpenDFe.NFe.Testes/AutorizacaoEmuladaTestes.cs` e `QuedaEmuladaTestes.cs` (a fixture do emulador está duplicada em cada projeto de teste que a usa; copie o padrão). `docs/SEFAZ_EMULADA.md` lista o que cada serviço responde, com os códigos; **leia a seção do serviço da sua etapa antes de escrever o teste**, e escreva o teste contra o que o emulador faz.
- **O emulador é independente da lib** (`PROJETO.md` 13.5): ele não usa o leitor nem as regras da lib. Se o teste da lib contra o emulador falhar, descubra de que lado está o erro lendo o **MOC/NT** (a fonte), não o outro lado. Erro do emulador: corrija-o (é código de teste, parte do repositório), cubra com teste em `OpenDFe.SefazEmulada.Testes` e registre na situação ("emulador corrigido: ..."). Não adapte a lib ao erro do emulador.
- **Homologação real** (`tests/OpenDFe.Homologacao.Testes`): estenda com o caso da etapa no padrão dos existentes (ligado por variável de ambiente, **pulado sem o PFX**). Não rode: não há PFX. A situação registra "pendente até o PFX".
- **Relógio:** os testes usam `ConfiguracaoDFe.Relogio` falso para datas (prazo, `dhEmi`, `dhEvento`), nunca `Thread.Sleep`.
- **Linux:** o repositório roda a CI em Linux e Windows (`PROJETO.md` 13.4), e as etapas anteriores conferiram o Linux pelo WSL. Se o WSL com .NET 10 existir (`wsl -l -v`; `wsl dotnet --version`), rode `Nucleo.Testes`, `NFe.Testes` e `SefazEmulada.Testes` (e os de impressão, na fase 7) lá, com `--artifacts-path` fora da pasta do repositório para não misturar `bin/obj` dos dois sistemas. Se não existir, a situação diz **"Linux não conferido (sem WSL)"** e o `ci.yml` cobre depois.
- **Mutação provada:** para 3 a 5 comportamentos-chave da etapa, quebre o código de propósito (inverta a condição, tire a regra), rode o teste que deveria acusar, confirme que ele falha, desfaça. A situação lista o que foi provado, como nas etapas 4.4 e 4.5.
- **Todo XML gerado passa pelo XSD** nos testes (`PROJETO.md` 13.2), e a leitura de volta reproduz o objeto quando houver leitura.

## 6. Decisão não coberta

O arquivo da etapa pode não prever tudo. Regra, na ordem:

1. A fonte primária manda (MOC, NT, schema, manual), conferida na etapa e registrada em `docs/NTS.md`. Fonte fora do ar: tente o espelho/Wayback Machine, o portal da UF, a cópia que já está em `docs/NTS.md`; registre qual foi usada.
2. Entre duas leituras possíveis da fonte, a **mais conservadora** (a que recusa antes de transmitir só o que a SEFAZ certamente recusa; a que não inventa campo).
3. Siga o padrão das etapas 4.4, 4.5 e 5.1 (o que elas fizeram em caso parecido).
4. **Não pergunte.** Escolha, implemente e registre em "Decisões tomadas sem perguntar" na situação (o que o desenho não cobria, o que escolheu, por quê), para o usuário conferir depois.
5. Regra fora do schema que apareceu e não está aprovada: **não implemente**; "Pendências para o usuário".
6. Mudança que quebraria API **já usada por etapa anterior** (todas ainda estão em `Unshipped`, então é permitido, mas só se necessário): faça, atualize os testes **por consequência direta da etapa** e registre em "Ajustes ao desenho".

## 7. Quando parar (bloqueio)

Pare **sem commitar código quebrado** quando: o build ou os testes anteriores já falham (seção 2.5); a fonte primária indispensável não pôde ser obtida por nenhum meio e a decisão não tem saída conservadora; o emulador não implementa algo de que a etapa depende e consertar passaria de ~1 dia de trabalho; ou os testes da etapa não fecham verdes depois de uma investigação séria.

Como parar sem sujar o repositório:

1. `git switch -c bloqueio-<etapa>` (por exemplo `bloqueio-5.2`) e commite o trabalho parcial lá, com a mensagem `Etapa <n>: parcial, bloqueada (não passa)`.
2. Volte à branch da fase (`git switch fase-...`), que fica no último commit verde, com a árvore limpa (exceto o trabalho do usuário da seção 2.2).
3. Crie/atualize `docs/etapas/BLOQUEIOS.md` com: a etapa, a data, o que foi feito (e a branch `bloqueio-...` onde está), o que bloqueou (com a mensagem/saída exata), o que foi tentado e o que seria preciso. Commite só esse arquivo na branch da fase: `Etapa <n>: bloqueio registrado`.
4. **Não comece a etapa seguinte.** Termine com o relatório da seção 10, dizendo "BLOQUEADA".

Bloqueio **parcial** (um item pequeno e isolado que não fecha, mas o resto está verde): não é bloqueio. Feche a etapa sem o item, registre-o em "Pendências" com o motivo exato e o que falta, e siga o commit normal.

## 8. Formato da situação em `PLANO.md`

Sob a seção da fase (crie `### Fase 6 (branch fase-6-eventos)` etc. quando for a primeira etapa dela, com "Criada a partir da `fase-...`"), um bloco como o da 4.5:

```
**<n>. <Nome>. Feita (<data>).** O detalhe está em `docs/CONTRATOS.md` <seção>.
- **API pública nova** (nos `PublicAPI.Unshipped.txt` ...): ...
- **Interno:** ...
- **Ajustes ao desenho** (a conferir pelo usuário): ...
- **Decisões tomadas sem perguntar:** ...
- **Testes** (<total> na solução, <n> novos): ... (quais classes e o que cada uma cobre)
- **Mutações provadas:** ...
- **Emulador corrigido:** ... (só se mudou)
- **Conferido:** build sem aviso (Debug e Release); `dotnet test --solution OpenDFe.slnx` com código 0 (<total> testes, <n> pulados, os de homologação); Linux: <conferido ou não>; `dotnet pack -c Release` com os mesmos pacotes e o `eng/ConferirPacotes.ps1` aprovando; a regra de nomes não acusa nada novo.
- **Pendente até o PFX** (`PROJETO.md` 17): ...
- **Pendências para o usuário:** ...  (regra não aprovada, fonte que não deu, etc.)

**Próxima: <n+1>. <Nome>** (<uma linha>), só quando pedida.
```

Troque o "Próxima" anterior (apague a linha antiga) e atualize a tabela de fases se a fase fechar (`PLANO.md` "Fases" não muda; a situação diz que a fase fechou e o "pronto quando" cumprido **no emulador**).

## 9. Commit

- Mensagem em português, no estilo do histórico (`git log --oneline`): `Etapa 5.1: QR Code e CSC da NFC-e, a tabela de URLs, o infNFeSupl e a NFC-e nas operações da autorização` (uma frase com o que entrou).
- **Sem `Co-Authored-By` e sem nenhuma atribuição ao Claude/Anthropic** na mensagem (regra do usuário, vale em todos os repositórios). **Sem push.** Sem `--no-verify`.
- `git add` com caminhos explícitos (código, testes, docs, `PublicAPI.Unshipped.txt`, `PLANO.md`, `docs/etapas/` se mudou). Antes de commitar, `git status` confere que **nada do trabalho do usuário** (seção 2.2) está na área de preparação.
- Um commit por etapa. Se a etapa teve correção de emulador, ela vai no mesmo commit (a situação diz).

## 10. Relatório final (a última mensagem do agente)

Curto, factual: etapa, **FEITA** ou **BLOQUEADA**, o hash do commit, contagem de testes (total, novos, pulados), o que ficou pendente (PFX, itens parciais, fonte que não deu), as "Decisões tomadas sem perguntar" e os "Ajustes ao desenho" mais relevantes, e se o Linux foi conferido. Não repita o diff.

## 11. Verificações finais (todo agente, nesta ordem)

1. `dotnet build OpenDFe.slnx -c Debug` e `-c Release`: sem aviso.
2. `dotnet test --solution OpenDFe.slnx`: código 0.
3. Linux (seção 5), se houver.
4. `dotnet pack OpenDFe.slnx -c Release -o <pasta fora do repositório>` e `./eng/ConferirPacotes.ps1 -Pasta <pasta>`: aprovando.
5. A conferência de nomes (seção 4).
6. `git status`: só os arquivos da etapa na preparação; o trabalho do usuário intacto.
7. Releia o arquivo da etapa, item por item de "Pronto quando": cada um tem teste ou registro na situação.
