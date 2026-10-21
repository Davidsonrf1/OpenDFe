# Leitura e detector (etapa 2.5)

Desenho dos tipos públicos da leitura da NF-e e da NFC-e (`PROJETO.md` 5.2 e 5.3) e do detector (`PROJETO.md` 3.3). **Aprovado em 2026-10-21:** todas as decisões da seção 7 pela recomendação (a recomendação está em negrito em cada uma; na 15, a (c): os XMLs da escrita e dos manuais já, e os reais quando o usuário os passar).

O critério é o do `CONTRATOS.md` e do `MODELO.md`: a menor superfície pública que atende a caixa de entrada do ErpCore (a nota recebida de um fornecedor) e a autorização da fase 4 (o `nfeProc` e o protocolo), fácil de estender em versão menor (`PROJETO.md` 12.3), com a regra de nomes sem preposição. Ao contrário da escrita (2.4), que ficou interna até a 4.4, **a leitura é pública já nesta etapa**: a caixa de entrada não passa pela SEFAZ e precisa dela sem a fase 4.

**Conferido na fonte (2026-10-21):** os 43 pacotes de schemas do leiaute 4.00 publicados no Portal Nacional da NF-e (Documentos > Esquemas XML), do primeiro (`PL_009`, NT 2016.002 v1.10, 10/04/2017) ao 010f, baixados e comparados por script com o `leiauteNFe_v4.00.xsd` embutido: os caminhos de elemento e atributo a partir do `NFe` e os valores das enumerações. O resultado está na seção 5. O `TNfeProc` e o `TProtNFe` pelo XSD embutido.

## 1. Visão geral

```
quem usa                              núcleo (OpenDFe)                      leiaute (OpenDFe.NFe)
--------                              ----------------                      ---------------------
DFe.Detectar(xml, leiautes) ────────► para cada leiaute e documento: ─────► Declarar(declaracao)
                                        as FormaLeitura declaradas ◄─────── DeclararLeitura(FormaLeitura)
                                        raiz, versão e modelo do XML
DeteccaoDFe { Leiaute, Documento, ◄──── a primeira que bate
              Raiz, Versao }

leiaute.Ler(xml) ──────────────────────────────────────────────────────────► LeitorNFe (interno)
NotaLida { Nota, Chave, Protocolo, ◄──────────────────────────────────────── XmlReader, um método por grupo
           Xml, Problemas }
```

| Tipo | Onde | Papel |
|---|---|---|
| `DFe.Detectar` | núcleo | o membro estático que o `CONTRATOS.md` 2.1 deixou para cá: qual leiaute, entre os de quem usa, lê o XML |
| `DeteccaoDFe` | núcleo | o resultado do detector: o leiaute, o documento, a raiz e a versão |
| `FormaLeitura` | núcleo | uma forma de XML que o leiaute lê: a raiz e onde estão a versão e o modelo |
| `DeclaracaoLeiaute.DeclararLeitura`, `.Leituras` | núcleo | o leiaute declara as formas; quem usa pode listá-las |
| `LeiauteNFe400.Ler` | leiaute | a leitura do `NFe` e do `nfeProc` |
| `NotaLida` | leiaute | o resultado da leitura: a nota, a chave, o protocolo, o XML original e os problemas |
| `Protocolo`, `MensagemSefaz` | modelo | o `protNFe` (`infProt`) e as mensagens da SEFAZ ao emissor (`cMsg`, `xMsg`); servem também à autorização (4.4) |

## 2. Tipos públicos

Os blocos mostram só os membros públicos, sem corpo, com a documentação essencial.

### 2.1. O detector, no núcleo

```csharp
public sealed class DFe
{
    // ... os membros da 1.6

    /// Qual dos leiautes lê o XML (PROJETO.md 3.3): o primeiro, na ordem recebida, com uma forma declarada
    /// (FormaLeitura) para um dos seus documentos que bate com a raiz, a versão e o modelo do XML.
    /// Lê só o necessário: a raiz e os dois caminhos da forma. Sem DTD nem entidade externa.
    /// null: nenhum leiaute lê o XML, inclusive quando ele não é XML bem formado. Não lança pelo conteúdo.
    /// ArgumentNullException: o XML ou a lista nulos, ou um leiaute nulo na lista.
    public static DeteccaoDFe? Detectar(string xml, params IEnumerable<ILeiaute> leiautes);
}

/// O que o detector achou. Construtor interno.
public sealed class DeteccaoDFe
{
    public ILeiaute Leiaute { get; }
    public Documento Documento { get; }      // pelo modelo do XML (ide/mod)
    public string Raiz { get; }              // o nome local da raiz: "NFe", "nfeProc"
    public string Versao { get; }            // a versão do XML, igual à do leiaute: "4.00"
}

/// Uma forma de XML que o leiaute lê, para um documento. Os caminhos são os nomes locais separados por "/", a
/// partir da raiz (sem ela), no espaço de nomes da raiz; o último pode ser um atributo ("@versao"). Vazio = a
/// própria raiz, para o atributo dela ("@versao").
public sealed class FormaLeitura
{
    /// ArgumentException: algum texto vazio; caminho que não termina num atributo (versão) ou que tem passo vazio.
    public FormaLeitura(string espacoNomes, string raiz, string caminhoVersao, string caminhoModelo);
    public string EspacoNomes { get; }       // "http://www.portalfiscal.inf.br/nfe"
    public string Raiz { get; }              // "NFe"
    public string CaminhoVersao { get; }     // "infNFe/@versao"
    public string CaminhoModelo { get; }     // "infNFe/ide/mod"
}

public sealed class DeclaracaoLeiaute
{
    // ... os membros da 1.6

    /// O leiaute lê XMLs desta forma para o documento da declaração. Só durante o ILeiaute.Declarar.
    /// ArgumentException: a mesma raiz no mesmo espaço de nomes já declarada.
    public void DeclararLeitura(FormaLeitura forma);

    /// As formas declaradas, para a listagem.
    public IReadOnlyList<FormaLeitura> Leituras { get; }
}
```

O detector confere, para cada leiaute e cada documento de `leiaute.Documentos`, as formas da declaração (`DeclaracaoLeiaute.Criar`): a raiz e o espaço de nomes iguais; o texto do `CaminhoVersao` igual a `leiaute.Versao`; o texto do `CaminhoModelo` igual ao número do documento (`55`, `65`). O leiaute 4.00 declara duas formas por documento:

| Raiz | Versão | Modelo |
|---|---|---|
| `NFe` | `infNFe/@versao` | `infNFe/ide/mod` |
| `nfeProc` | `@versao` | `NFe/infNFe/ide/mod` |

Os eventos (`procEventoNFe`, fase 6) e os documentos da distribuição (`resNFe`, `resEvento`, fase 8) entram como formas novas nas fases deles; o modelo do evento vem da chave (`chNFe`), o que pede um caminho de chave na `FormaLeitura` (membro novo, versão menor).

### 2.2. A leitura, no leiaute

```csharp
public sealed class LeiauteNFe400 : ILeiaute
{
    // ... os membros da 2.1

    /// Lê uma NF-e ou NFC-e: o NFe ou o nfeProc (a nota com o protocolo), de qualquer NT do leiaute 4.00
    /// (PROJETO.md 5.2). Não valida pelo schema nem confere regras: lê o que reconhece, aponta o que não
    /// reconhece e devolve o XML recebido como veio (5.3). Sem DTD nem entidade externa. Sem estado e seguro em
    /// paralelo. Não lança pelo conteúdo: XML mal formado, outra raiz ou outra versão são problemas.
    /// ArgumentNullException: xml nulo.
    public NotaLida Ler(string xml);
}

/// O resultado da leitura. Construtor interno; imutável (a nota dentro dela é do modelo, mutável, e passa a ser
/// de quem leu).
public sealed class NotaLida
{
    /// A nota; null só quando o XML não é uma NF-e do leiaute 4.00 (NFE-XML, NFE-RAIZ, NFE-VERSAO).
    public NotaFiscal? Nota { get; }

    /// A chave do Id do infNFe; null quando ausente ou inválida (NFE-CHAVE).
    public ChaveAcesso? Chave { get; }

    /// O protocolo do nfeProc; null no NFe sozinho.
    public Protocolo? Protocolo { get; }

    /// O XML recebido, a mesma instância passada ao Ler: é o que se guarda (5.3), nunca o regerado.
    public string Xml { get; }

    /// Os problemas da leitura (seção 4), na ordem do XML.
    public IReadOnlyList<Problema> Problemas { get; }
}
```

### 2.3. O protocolo, no modelo

Pelas regras de forma do `MODELO.md` 3 (classe selada, `get; set;`, tudo anulável, lista inicializada, `[Tag]` em cada propriedade), porque a leitura o preenche campo a campo e o valor ilegível fica nulo. Os identificadores do MOC (grupo `PR`) são conferidos no Anexo I na implementação.

```csharp
/// O protNFe: o resultado do processamento de uma NF-e pela SEFAZ (autorizada, denegada ou rejeitada). Vem no
/// nfeProc e, na fase 4, no retorno da autorização, da consulta do recibo e da consulta do protocolo.
[Tag("protNFe")]
public sealed class Protocolo
{
    [Tag("Id")]        public string? Id { get; set; }                        // o atributo infProt/@Id, opcional
    [Tag("tpAmb")]     public Ambiente? Ambiente { get; set; }
    [Tag("verAplic")]  public string? VersaoAplicativo { get; set; }
    [Tag("chNFe")]     public string? Chave { get; set; }                     // texto, como as chaves referenciadas (MODELO.md decisão 7)
    [Tag("dhRecbto")]  public DateTimeOffset? Recebimento { get; set; }
    [Tag("nProt")]     public string? Numero { get; set; }                    // 15 dígitos; ausente na rejeição
    [Tag("digVal")]    public string? Digest { get; set; }                    // base64, o DigestValue da nota processada
    [Tag("cStat")]     public int? Status { get; set; }                       // 100, 150, 110, 301, 302, ou a rejeição
    [Tag("xMotivo")]   public string? Motivo { get; set; }
    /// As mensagens da SEFAZ ao emissor (a sequência cMsg + xMsg, até 5).
    public IList<MensagemSefaz> Mensagens { get; } = new List<MensagemSefaz>();
    // a Signature do protNFe (opcional) fica fora, como a da nota (MODELO.md decisão 20)
}

/// Uma mensagem da SEFAZ ao emissor (cMsg, xMsg). Sem grupo próprio no XML: a sequência repetida.
public sealed class MensagemSefaz
{
    [Tag("cMsg")] public int? Codigo { get; set; }
    [Tag("xMsg")] public string? Texto { get; set; }
}
```

### 2.4. Exemplo de uso

```csharp
// a caixa de entrada: um XML recebido, de versão desconhecida
var leiaute = new LeiauteNFe400();
if (DFe.Detectar(xml, leiaute) is { Leiaute: LeiauteNFe400 nfe } deteccao)
{
    var lida = nfe.Ler(xml);
    if (lida.Problemas.Any(p => p.Severidade == Severidade.Erro)) { /* mostra os problemas */ }
    if (lida.Protocolo?.Status is not (100 or 150)) { /* não autorizada: não entra */ }
    var nota = lida.Nota!;
    var fornecedor = nota.Emitente.Cnpj ?? nota.Emitente.Cpf;
    foreach (var item in nota.Itens) { /* item.Produto.Codigo, .Quantidade, .Valor, .Rastros ... */ }
    guardar(lida.Chave, lida.Xml);                       // o XML recebido, não o regerado
}
```

## 3. Como a leitura funciona

- **`XmlReader` à mão, um método por classe do modelo** (`PROJETO.md` 5.1), espelho do `EscritorNFe`: `LeitorNFe` (interno) com um método por grupo e a `LeituraNFe` com o leitor, o caminho aberto (no XML e no modelo, para o problema já sair com a propriedade, sem esperar a 2.6) e os problemas. Gerado por script a partir do XSD e do modelo, como a escrita, e revisado.
- **Pelo nome, não pela ordem:** cada filho é reconhecido pelo nome local, no espaço de nomes da NF-e; a ordem do XSD não é conferida, nem a obrigatoriedade, nem as facetas (tamanho, padrão). O que reconhece entra no objeto; o grupo ausente fica nulo (o obrigatório fica como o construtor deixa, inicializado e vazio).
- **As escolhas** pelo elemento: o grupo do ICMS pelo nome (`ICMS00` → `Icms00`), o `NFref` pelo filho; as propriedades exclusivas (CNPJ e CPF) recebem o que vier, as duas se vierem as duas (o schema e as regras apontam, `MODELO.md` decisão 10).
- **Os valores**, pela forma do XSD mas sem as facetas: decimal invariável (`.` como separador, sem milhar; as casas que vierem); `DateTimeOffset` só com o deslocamento (`TDateTimeUTC`); `DateOnly` como `AAAA-MM-DD`; inteiros sem sinal; enumerações pelo valor do XML, com as traduções da escrita ao contrário (`S`/`N` do `indEscala`, `R`/`N` do `VIN`, `2B`/`2C`/`2D` do `refECF`). O texto como está (espaços e quebras de linha preservados). O valor que não converte fica nulo, com problema.
- **Os campos do núcleo** (`cUF`, `mod`, `tpEmis`, `tpAmb`, `dhCont`, `xJust`) e o `cNF` são lidos para o modelo (`MODELO.md` decisão 6); o `cDV` é conferido contra a chave do `Id`.
- **O `nItem`:** os itens ficam na ordem em que vieram; a sequência 1 a n é conferida (`MODELO.md` decisão 21).
- **O que fica fora sem problema:** a `Signature` da nota e a do protocolo (decisão 20 do `MODELO.md`), as declarações de espaço de nomes, a declaração XML, comentários, instruções de processamento e o espaço em branco entre as tags (o XML indentado lê igual). O BOM no começo do texto é aceito.
- **Segurança:** `DtdProcessing.Prohibit`, `XmlResolver = null`; o XML vem de fora.

## 4. Os problemas da leitura

Com o prefixo `NFE-`, como os da escrita (`CodigosEscrita`), e sempre com o `Caminho` (com o índice a partir de 1 no repetido) e, quando há, a `Propriedade`.

| Código | Severidade | Quando | O que acontece |
|---|---|---|---|
| `NFE-XML` | Erro | o texto não é XML bem formado | `Nota` nula; a mensagem com a linha e a coluna |
| `NFE-RAIZ` | Erro | a raiz não é `NFe` nem `nfeProc` no espaço de nomes da NF-e | `Nota` nula |
| `NFE-VERSAO` | Erro | a `versao` do `infNFe` (ou do `nfeProc`) não é `4.00` | `Nota` nula: é outro leiaute (`PROJETO.md` 4.5) |
| `NFE-CHAVE` | Erro | o `Id` falta, não é `NFe` + 44 posições, ou a chave é inválida (`ChaveAcesso.TentarLer`); o `cDV` difere do dígito da chave | `Chave` nula no primeiro caso; a nota é lida |
| `NFE-PROTOCOLO` | Erro | o `chNFe` do protocolo difere da chave da nota | o protocolo vem, mas não é desta nota |
| `NFE-VALOR` | Erro | o valor não converte ao tipo da propriedade (decimal com vírgula, data sem fuso, letra num número) | a propriedade fica nula; o valor encontrado não vai na mensagem (pode ser dado pessoal, `PROJETO.md` 6.4) |
| `NFE-RETIRADO` | Aviso | um campo de uma NT anterior que saiu do schema (seção 5) | fica fora do objeto, no XML original |
| `NFE-DESCONHECIDO` | Aviso | um elemento ou atributo que o leiaute não conhece | ignorado, com o que tiver dentro; fica no XML original |
| `NFE-REPETIDO` | Aviso | um elemento que o XSD aceita uma vez, repetido | vale o primeiro |
| `NFE-ENUMERACAO` | Aviso | o valor numérico fora da lista da enumeração (`MODELO.md` decisão 8) | entra como o número (`(MeioPagamento)77`) |
| `NFE-ITEM` | Aviso | o `nItem` fora da sequência 1 a n, repetido ou ausente | o item fica na posição em que veio |

## 5. NTs anteriores: o que saiu do schema do leiaute 4.00

Comparados os 43 pacotes do 4.00 com o 010f (seção de abertura). **O leiaute é cumulativo**: nenhum valor de enumeração saiu, e os campos que saíram são poucos.

| Campos | Nos pacotes | O que houve |
|---|---|---|
| `det/imposto/IPI/clEnq` | do `PL_009` da NT 2016.002 v1.10 (10/04/2017) ao da v1.30 (14/09/2017) | retirado na v1.40 (24/10/2017) |
| `det/prod/cCredPresumido`, `pCredPresumido`, `vCredPresumido` | só no 9m (NT 2023.004 v1.01, 07/02/2024) | substituídos pelo grupo `gCred` no 9n (19/03/2024) |
| `IBSCBS/gIBSCBS/gCBS/gDif/vCBSOp` e `gIBSMun/gDif/vCBSOp` | só no 010a (NT 2025.002 v1.01, 15/04/2025) | retirados no 010b |
| `IBSCBS/gIBSCBS/gIBSCredPres` e `gCBSCredPres` (com `cCredPres`, `pCredPres`, `vCredPres`, `vCredPresCondSus`) | do 010a ao 010b da v1.20 (20/08/2025) | substituídos no 010b da v1.30 |
| `IBSCBS/gIBSCBSMono` com os campos soltos (`qBCMono`, `adRemIBS`, `vIBSMono`, `vCBSMonoDif`...: 16 campos) | do 010a ao 010b da v1.10 (09/06/2025) | agrupados no 010b da v1.20 |
| `IBSCBS/gIBSCBSMono/gMonoPadrao`, `gMonoReten`, `gMonoRet`, `gMonoDif` (com os campos de cada um) | do 010b da v1.20 (30/07/2025) ao 010e (26/06/2026) | a monofasia reformulada na v1.50 (010f): até ela entrar em produção (03/11/2026), é o schema de produção |
| `det/imposto/IS/pISEspec` | do 010a ao 010e | retirado no 010f |

Fora isso, um XML de qualquer NT do 4.00 desde 2017 tem só campos que o 010f conhece. O leiaute 3.10 (2014 a 2018) é outro leiaute (`NFE-VERSAO`).

## 6. A migração do leitor do ErpCore

O `ErpCore.Fiscal/Nfe/LeitorDeNfe.cs` (lido nesta etapa) lê, do `NFe` ou do `nfeProc` (sem conferir a assinatura): a chave do `Id`, com o DV; `mod` (só 55), `nNF`, `serie`, `dhEmi`, `tpNF`; `emit` e `dest` com `CNPJ` ou `CPF`, `xNome` e `IE` (só os dígitos, nula se `ISENTO`); cada `det` com o `nItem`, `cProd`, `xProd`, `uCom`, `qCom`, `vProd`, `vFrete`, `vSeg`, `vOutro`, `vDesc`, os `rastro` (`nLote`, `qLote`, `dFab`, `dVal`), o `vICMSST` e o `vFCPST` do grupo do ICMS e o `vIPI` do `IPITrib`; o `vNF` do `ICMSTot`; e o `cStat` do `protNFe`. Recusa com mensagem o XML que não é XML, a versão diferente de 4.00, a chave inválida, o modelo diferente de 55, a nota sem itens e o campo obrigatório que falta.

Tudo isso está no modelo (2.2 e 2.3) e na `NotaLida`; o que é regra da caixa de entrada (só modelo 55, só autorizada, `IE` sem `ISENTO`) fica com o ErpCore. A tabela campo a campo entra aqui na implementação, e um teste lê com a lib a NF-e que o teste do ErpCore monta (`ErpCore.Api.Testes/NfeDeTeste.cs`).

## 7. Decisões (aprovadas em 2026-10-21)

1. **Onde fica a leitura.** (a) **método público de instância `LeiauteNFe400.Ler(string)`, ao lado da interface**; (b) classe estática pública `LeitorNFe.Ler`; (c) operação local do núcleo (`dfe.Ler(xml)`). Recomendo (a): ler XML de 4.00 é do leiaute 4.00 (o 5.00, se vier, terá o seu), o `Detectar` devolve o leiaute e quem usa chega ao `Ler` por ele; (c) exigiria criar um núcleo, com certificado e conexões, só para ler um arquivo.
2. **A entrada.** (a) **`string`**; (b) `string` e `Stream`; (c) `byte[]`. Recomendo (a): a NF-e é UTF-8 (MOC), quem tem bytes decodifica (`Encoding.UTF8.GetString`), e a `NotaLida.Xml` devolve exatamente o que entrou (5.3). O `Stream` e os bytes entram depois, em versão menor, se pedidos.
3. **O resultado.** (a) **`NotaLida` imutável, com a `Nota` nula só quando o XML não é uma NF-e 4.00, e nunca exceção pelo conteúdo**; (b) a nota sempre presente, vazia no XML que não é nota; (c) exceção (`FormatException`) no XML mal formado, na raiz ou na versão, como o leitor do ErpCore. Recomendo (a): `PROJETO.md` 2.4 (problema não é exceção; o arquivo que o fornecedor mandou é dado, não erro de programação); (b) esconderia o "não é nota" atrás de um objeto vazio.
4. **Tolerância.** (a) **ler sem o XSD: pelo nome, sem conferir ordem, obrigatoriedade nem facetas; o que não reconhece é aviso, o valor que não converte é erro com a propriedade nula**; (b) validar pelo XSD do 010f antes e recusar o inválido; (c) ler e juntar os problemas do schema aos da leitura. Recomendo (a): a nota autorizada já passou pelo schema da época, e o 010f recusa o que uma NT anterior aceitava (seção 5), o que fere o `PROJETO.md` 5.2 ("aceita o que a SEFAZ aceita"); quem quiser validar pede à parte (um `Validar` público entra em versão menor, se pedido).
5. **Os campos retirados (seção 5).** (a) **reconhecidos e fora do objeto, com o aviso `NFE-RETIRADO`, e o valor no XML original**; (b) propriedades no modelo, marcadas `[Obsolete]`, lidas e nunca escritas; (c) tratados como desconhecidos (`NFE-DESCONHECIDO`). Recomendo (a): são poucos, quase todos de versões preliminares da reforma, com valores só informativos em 2026 (ano de teste); (b) quebraria a regra de que o modelo é o XSD vigente (o teste da 2.2) e deixaria na API pública campos que nenhuma nota nova usa; (c) diria "não conheço" de um campo que a SEFAZ aceitou.
6. **O protocolo.** (a) **`Protocolo` e `MensagemSefaz` no modelo, pelas regras de forma do `MODELO.md` 3, com `[Tag]`; o mesmo tipo serve à fase 4**; (b) tipo de saída imutável, com construtor interno, no leiaute. Recomendo (a): a leitura o preenche campo a campo, o valor ilegível fica nulo, e a 2.6 aponta a propriedade pelo `[Tag]`; o retorno da autorização (4.4) só o reaproveita. Os nomes: `Status` para o `cStat` (o `ResultadoWS.CStat` do núcleo é o do lote, não o da nota), `Numero` para o `nProt`, `Digest` para o `digVal`.
7. **O protocolo de outra nota.** (a) **`NFE-PROTOCOLO`, erro, com o protocolo no resultado**; (b) o protocolo descartado (`Protocolo` nulo); (c) só aviso. Recomendo (a): na caixa de entrada, um `nfeProc` com o protocolo de outra nota é o caso de fraude que o `Status == 100` sozinho não pega; erro deixa claro, e o objeto continua inspecionável.
8. **A chave.** (a) **a `ChaveAcesso` do `Id`, pela `TentarLer` da 1.1; ausente ou inválida, `NFE-CHAVE` e nula; o `cDV` diferente do dígito, `NFE-CHAVE`; as outras partes contra os campos (UF, série, número, emitente) são regra da fase 3**; (b) conferir já todas as partes. Recomendo (a): a 2.5 lê; a coerência entre a chave e a nota é regra do MOC, com código de rejeição (3.2).
9. **A chave da NFF** (`tpEmis` 3). A `ChaveAcesso.TentarLer` recusa o tipo de emissão fora do enum, e o `TipoEmissao` não tem o 3 (decisão da 1.1): a NF-e emitida pela Nota Fiscal Fácil (o MEI, o produtor rural) é lida com `Chave` nula e `NFE-CHAVE`. (a) **a `TentarLer` passa a aceitar o tipo de emissão 3, com `(TipoEmissao)3` na chave, como a leitura já faz no `Identificacao.TipoEmissao` (`MODELO.md` 2.11)**; (b) manter, e a `NotaLida` ganha o `Id` como texto; (c) manter, e a NFF fica sem chave. Recomendo (a): a caixa de entrada do ErpCore recebe nota de produtor rural, e a chave é o identificador que se guarda; muda uma regra da 1.1, sem mudar a API.
10. **A forma do detector.** (a) **declarativa: o leiaute declara a raiz e os caminhos da versão e do modelo (`FormaLeitura`), e o núcleo confere**; (b) o leiaute registra um reconhecedor (um delegado sobre o `XmlReader`); (c) o núcleo confere só a raiz e o espaço de nomes e devolve os leiautes candidatos, sem o documento. Recomendo (a): sem código do leiaute no detector (testável no núcleo com um leiaute falso, listável numa tela, sem reflexão), e com o documento, que o `PROJETO.md` 3.3 pede ("o modelo e a versão"); custa uma sintaxe de caminho mínima, documentada no tipo. (b) é mais flexível, mas cada leiaute de terceiro reescreveria a leitura segura do XML; (c) não diz se é NF-e ou NFC-e.
11. **A assinatura do `Detectar`.** (a) **`Detectar(string xml, params IEnumerable<ILeiaute> leiautes)`, `null` quando nenhum lê (inclusive o XML mal formado), o primeiro na ordem quando mais de um lê**; (b) um registro de leiautes de quem usa (`RegistroLeiautes`), com o `Detectar` nele; (c) exceção quando nenhum lê. Recomendo (a): `PROJETO.md` 3.3 ("entre os registrados por quem usa") sem estado novo; o "por que não leu" é da leitura, que dá os problemas.
12. **Os códigos dos problemas** (seção 4), com o prefixo `NFE-`, como os da escrita. (a) **os onze da tabela**; (b) menos códigos (um `NFE-LEITURA` para tudo, com a mensagem diferenciando). Recomendo (a): quem trata a caixa de entrada filtra pelo código (o `NFE-RETIRADO` e o `NFE-DESCONHECIDO` são de ignorar; o `NFE-PROTOCOLO`, não).
13. **O elemento repetido onde o XSD aceita um.** (a) **vale o primeiro, com `NFE-REPETIDO`**; (b) vale o último. Recomendo (a): é o que um leitor por caminho (o do ErpCore, `Element()`) já faz.
14. **A migração do leitor do ErpCore.** (a) **nesta etapa, a lib cobre tudo o que ele lê (seção 6), com a tabela e o teste; a troca no ErpCore é tarefa do ErpCore, quando a lib for publicada e entrar na lista de pacotes do backend (`PROJETO.md` 15)**; (b) trocar já, por referência de projeto ao repositório da lib. Recomendo (a): são repositórios separados, e o ErpCore só usa pacote publicado.
15. **XMLs reais para o teste** (`PLANO.md`: "XMLs reais leem sem problema"; `tests/Exemplos/` está na estrutura e não existe). (a) **você me passa notas reais (compra de fornecedor e NFC-e, idealmente de emissores diferentes, uma de combustível e uma com IBS/CBS) numa pasta fora do repositório; um script troca os dados pessoais e de empresa (nomes, CNPJ, CPF, IE, endereços, telefones, e-mails, chave com o DV recalculado), e só o resultado entra em `tests/Exemplos/`, com a assinatura que deixa de valer**; (b) só XMLs montados pela escrita da 2.4 e exemplos de manuais; (c) as duas coisas. Recomendo (c), com (a) se houver notas: a ida e volta da 2.4 prova o leitor contra o escritor, mas só XML de outros emissores prova a tolerância (indentação, ordem de atributos, prefixos, campos de NT anterior).

## 8. Testes previstos

- **Ida e volta** (`PROJETO.md` 13.2): toda nota dos testes da escrita (as válidas no schema e as preenchidas por reflexão da cobertura) escrita, lida e comparada com a original propriedade a propriedade, salvo os campos do núcleo e o `cNF` (a escrita os completa); os 29 grupos de ICMS, PIS e COFINS; o `nfeProc` montado com um protocolo.
- **Cobertura:** toda propriedade com `[Tag]` do modelo é lida (o espelho do `OLeiauteEscreveTodoCampoDoModelo`).
- **Tolerância:** o XML indentado, com BOM, com comentários, com os atributos em outra ordem, com prefixo de espaço de nomes; cada campo retirado da seção 5; o elemento desconhecido com filhos; o repetido.
- **Cada problema** da seção 4, com o caminho e a propriedade.
- **Detector:** NF-e, NFC-e, `NFe` e `nfeProc`; a versão 3.10, outro espaço de nomes, outra raiz, XML mal formado (nulo); dois leiautes (o primeiro vale); e um leiaute falso no `Nucleo.Testes`, para o detector sem a NF-e.
- **O ErpCore:** a nota do `NfeDeTeste` lida com os valores que o `LeitorDeNfe` devolve.
- **Exemplos reais** (decisão 15), quando houver.
