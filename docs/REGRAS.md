# Regras do MOC e callbacks (fase 3)

Desenho do motor de regras (`PROJETO.md` 6.2 e 6.3, etapa 3.1): onde as regras do leiaute e os callbacks de quem usa entram no caminho do `ExecutarAsync`, o que o núcleo ganha de público para isso e a forma do catálogo das rejeições. **Desenho para aprovação (2026-10-22).** O catálogo em si (o levantamento, seção 6) é feito na implementação da 3.1, depois de aprovada a forma.

O critério é o do `CONTRATOS.md`: a menor superfície pública que atende as fases 3 a 8, fácil de estender em versão menor (`PROJETO.md` 12.3), com a regra de nomes sem preposição. As regras em si (a conta do total, o DV do CPF) ficam **internas** no leiaute; o público é só a costura no núcleo.

**Conferido na fonte (2026-10-22):** o MOC 7.0, Anexo I, versão 7.03 (o mesmo PDF das etapas 1.4 a 2.3, SHA-256 a registrar no levantamento), seções 4.1 (regras gerais, grupos A, B, D, E e F) e 4.2.1 (autorização, grupos A a ZX, cerca de 495 regras com 426 códigos distintos); a coluna "Aplic." (`Obrig.`, `Facul.`) e o "Efeito" (`Rej.`, `Den.`). O Anexo 7.03 consolida as NTs até a 2020.006; as regras das NTs seguintes (2021.002 a 2026.009, as do `NTS.md`) acrescentam cerca de 400, a conferir uma a uma no levantamento.

## 1. Visão geral

```
dfe.ExecutarAsync(parametro)
  1. o handler pelo tipo exato
  2. Montar → PedidoWS { Xml, Schema, IdsAssinatura, Documentos (novo), Problemas }
       Erro → RecusadoLocalmente
  3. assinar cada Id
  4. schema (6.1)                                  Erro → RecusadoLocalmente
  5. regras do leiaute (6.2), por documento (novo) Erro → RecusadoLocalmente
       as declaradas por DeclaracaoLeiaute.DeclararRegras, com o DocumentoVerificado
       o código em ConfiguracaoDFe.RegrasComoAviso rebaixado a Aviso
  6. callbacks de quem usa (6.3), por documento (novo)  Erro → RecusadoLocalmente
       Campo: cada campo do XML assinado do documento (CampoVerificado)
       Documento: o documento inteiro (DocumentoVerificado)
  7. a URL; 8. transmitir; 9. ler; 10. Processado
```

| Tipo ou membro | Onde | Papel |
|---|---|---|
| `DocumentoPedido` | núcleo, novo | um documento dentro da mensagem: o `Id` do elemento assinado, o modelo, a chave e a conversão do caminho na propriedade |
| `PedidoWS.Documentos` | núcleo, novo | os documentos da mensagem (uma nota por nota do lote); vazio no status, na consulta |
| `DeclaracaoLeiaute.DeclararRegras` | núcleo, novo | o leiaute entrega a função das regras do documento; o núcleo a chama no passo 5 |
| `ConfiguracaoDFe.RegrasComoAviso` | núcleo, novo | os códigos de regra que quem usa rebaixa a aviso (a válvula para a regra que a lib aplica errado) |
| `ValidacaoCallback`, `CampoVerificado`, `DocumentoVerificado` | núcleo, já existem (1.6) | passam a ser chamados; a forma não muda |
| `RegraNFe`, `RegrasNFe400` | leiaute, internos | a tabela das regras: código, identificador do MOC, modelos, aplicação, mensagem e a conferência sobre a nota |

## 2. Tipos públicos (no núcleo)

```csharp
/// Um documento dentro da mensagem do pedido (a nota do lote; na fase 6, o evento). O leiaute cria; o núcleo o
/// acha no XML assinado pelo Id e entrega às regras do leiaute e aos callbacks.
public sealed class DocumentoPedido
{
    /// id: o Id do elemento assinado ("NFe" + chave, do infNFe). O documento é o elemento pai dele (o NFe), como na
    /// assinatura enveloped de todo DF-e. ArgumentException: id nulo ou vazio.
    public DocumentoPedido(string id);
    public string Id { get; }
    /// O modelo do leiaute (a NotaFiscal); null quando o documento veio só como XML (o preparado, 4.4).
    public object? Modelo { get; init; }
    public ChaveAcesso? Chave { get; init; }
    /// A conversão do caminho do documento ("NFe/infNFe/det[3]/prod/xProd") na propriedade do modelo, a mesma da
    /// 2.6; null = os campos saem sem propriedade.
    public Func<string, string?>? Propriedade { get; init; }
}

public sealed class PedidoWS
{
    // novo
    /// Os documentos da mensagem, na ordem; vazio quando a mensagem não leva documento (status, consulta).
    /// ArgumentException: Id repetido. Um Id que não existe no XML assinado é erro de programação do leiaute
    /// (InvalidOperationException no ExecutarAsync, como o Id da assinatura).
    public IReadOnlyList<DocumentoPedido> Documentos { get; init; } = [];
}

public sealed class DeclaracaoLeiaute
{
    // novo
    /// As regras do leiaute (PROJETO.md 6.2) para os documentos dos pedidos: chamadas uma vez por documento, depois
    /// do schema, com o mesmo DocumentoVerificado dos callbacks. Pode ser chamado mais de uma vez (as funções rodam na
    /// ordem). Só durante o ILeiaute.Declarar. A função é segura para uso concorrente; exceção nela sobe como veio.
    public void DeclararRegras(Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>> regras);
}

public sealed class ConfiguracaoDFe
{
    // novo
    /// Códigos das regras do leiaute ("564") cujo problema sai como Aviso em vez de Erro: para seguir emitindo quando
    /// a lib aplica uma regra que o autorizador não aplica, até a versão corrigida. Não vale para o schema (6.1) nem
    /// para os callbacks. O Criar recusa código nulo ou vazio.
    public IReadOnlyCollection<string> RegrasComoAviso { get; init; } = [];
}
```

Os tipos da 1.6 ficam como estão: `DocumentoVerificado` (`Documento`, `Modelo`, `Xml`, `Chave`) é montado pelo núcleo a partir do `DocumentoPedido` e do XML assinado, e serve às regras do leiaute e ao callback `Documento`; `CampoVerificado` (`Documento`, `Tag`, `Caminho`, `Propriedade`, `Texto`), ao callback `Campo`. Os comentários de documentação deles e do `ValidacaoCallback` ("por ora, a configuração só os guarda") são atualizados.

## 3. O caminho (o que muda no `CONTRATOS.md` 3)

- **Passo 5, regras do leiaute.** Só com o schema sem `Erro`. Para cada `DocumentoPedido`, na ordem: o núcleo acha o elemento do `Id` no XML assinado, monta o `DocumentoVerificado` (o `Xml` é o elemento pai, com a `Signature`) e chama cada função de `DeclararRegras`. Os problemas com o código em `RegrasComoAviso` saem com `Severidade.Aviso` (o resto do problema igual). Todas as regras de todos os documentos rodam; com algum `Erro`, `RecusadoLocalmente`.
- **Passo 6, callbacks.** Só sem `Erro` até aqui. Para cada documento: o `Campo` em cada campo do documento (abaixo), depois o `Documento` uma vez. Com algum `Erro`, `RecusadoLocalmente`.
- **Os campos do callback `Campo`:** cada elemento do documento sem filhos elementos (com o texto, inclusive vazio) e cada atributo (salvo as declarações de namespace), na ordem do documento, **fora da `Signature`** e do que vem dentro dela. O `Caminho` é o do documento, com o índice só no elemento que se repete e `@` no atributo, como o do schema (`NFe/infNFe/det[3]/@nItem`); a `Tag` é o nome local (sem `@`); a `Propriedade`, a da conversão do `DocumentoPedido`.
- **Sem documentos** (status, consulta), os passos 5 e 6 não fazem nada. Sem `Validacao` (ou com o delegado nulo), o callback não é chamado e o XML não é percorrido.
- **Os problemas** no `ResultadoWS.Problemas`, na ordem das etapas: montagem, schema, regras, callbacks, leitura (os avisos seguem até o `Processado`).
- **Exceções:** a das funções de regra e dos callbacks sobe como veio (erro de programação, `CONTRATOS.md` 2.10); a lista nula ou com problema nulo devolvida por elas é `InvalidOperationException`, como o pedido nulo.

## 4. As regras no leiaute (interno)

- **`RegraNFe`:** o código da rejeição (`"564"`), o identificador do MOC ou da NT (`"W16-10"`, `"UB12-10"`), os modelos em que vale (55, 65 ou os dois), a aplicação (obrigatória ou facultativa), a mensagem do MOC (sem o "Rejeição: ") e a conferência (`Func<NotaFiscal, ContextoRegra, IEnumerable<...>>`, com o que a regra achou: o caminho, a propriedade e o detalhe).
- **`RegrasNFe400`:** a lista, e a função entregue a `DeclararRegras`: lê o `Modelo` como `NotaFiscal` (ou, nulo, lê o XML pelo `LeitorNFe` da 2.5) e roda as regras do documento do núcleo.
- **O problema da regra:** `Codigo` = o código da rejeição; `Mensagem` = a do MOC, mais o detalhe com os valores que não são dado pessoal ("vProd 100,00; soma dos itens 99,00"; o CPF errado não vai na mensagem, `PROJETO.md` 6.4); `Tag`, `Caminho` (do documento, como o do callback) e `Propriedade` do campo que a regra aponta. Regra obrigatória = `Erro`; facultativa = `Aviso` (decisão 6).
- **Robustez:** as regras rodam sobre uma nota que passou no schema, mas leem o modelo, e o campo nulo é tratado como "não se aplica" (o schema já apontou o obrigatório).
- **O relógio** (as regras de data: emissão no futuro, NFC-e atrasada) é o `ConfiguracaoDFe.Relogio` no instante da validação, com as tolerâncias e as exceções de contingência do MOC.

## 5. Decisões (para aprovação)

1. **Onde as regras do leiaute entram.** (a) **declaradas na `DeclaracaoLeiaute` (`DeclararRegras`) e chamadas pelo núcleo, por documento, depois do schema**; (b) no `Montar`, nos `Problemas` do pedido (nada novo no núcleo, mas as regras rodam antes do schema e de assinar); (c) uma função por pedido (`PedidoWS.Regras`). Recomendo (a): é a ordem do `PROJETO.md` 6 (schema, regras, callbacks), o núcleo fica dono do caminho, como na 1.6 (decisão 2), e a função é registrada uma vez, como as operações. (b) diverge do 6 e faz cada operação lembrar de chamar as regras; (c) espalha a mesma função por todo pedido.
2. **Quando cada etapa roda.** (a) **cada etapa só roda se a anterior não deu `Erro`; dentro da etapa, todas as regras (e todos os documentos) rodam**; (b) todas as etapas sempre, juntando tudo. Recomendo (a): é o que a SEFAZ faz (a 225 vem antes de qualquer regra), as regras podem contar com o XML válido, e não sai o mesmo campo duas vezes (o CNPJ fora do padrão no schema e de novo no DV). Quem corrige vê primeiro os erros de forma e depois os de regra; dentro de cada etapa, vê todos de uma vez (a SEFAZ para no primeiro).
3. **Os documentos do pedido.** (a) **`PedidoWS.Documentos`, com `DocumentoPedido` (o `Id`, o modelo, a chave, a conversão); o documento é o pai do elemento do `Id`**; (b) só o `IdsAssinatura`, e o resto pelo leiaute numa função. Recomendo (a): o XML preparado (4.4) já vem assinado, com `IdsAssinatura` vazio, e ainda tem documento; o pai do elemento assinado é o documento em todo DF-e (NF-e, evento, CT-e, MDF-e).
4. **Os campos do callback `Campo`.** (a) **os elementos sem filhos e os atributos, fora da `Signature`, com o caminho como o do schema**; (b) só os elementos. Recomendo (a): "cada campo escrito" inclui o `xCampo` da observação e os atributos do modelo com `[Tag]` (`@dia`, `@nItem`); quem não quer filtra pela `Tag`.
5. **A propriedade no callback.** (a) **a conversão no `DocumentoPedido` (`Func<string, string?>`, a mesma da 2.6)**; (b) a conversão do `pedido.Schema` (só serve quando a mensagem é o próprio documento, e a autorização manda o `enviNFe`); (c) sem propriedade no callback. Recomendo (a): segue a decisão 2 da 2.6 (função, sem tipo novo), funciona no lote e no evento.
6. **As regras facultativas do MOC** (`Facul.`: a UF escolhe se aplica). (a) **implementadas como `Aviso`**; (b) como `Erro`; (c) fora da lib, só no catálogo. Recomendo (a): avisa do que pode ser rejeitado sem impedir a nota que a UF aceitaria. Uma tabela de quais UFs aplicam cada facultativa não existe no MOC.
7. **A válvula `RegrasComoAviso`.** (a) **na configuração, por código, só para as regras do leiaute: o problema continua, como `Aviso`**; (b) nada agora; (c) desligar a regra (o problema some). Recomendo (a): as regras são a leitura que a lib faz do MOC; se uma estiver errada, o PDV não pode parar de vender até a versão nova. (c) esconde o problema (`PROJETO.md` 4.4: nada some calado). O código é o do MOC, e as regras que dividem um código são rebaixadas juntas.
8. **O identificador da regra no problema.** (a) **não: o `Codigo` (a rejeição) e a mensagem; o identificador do MOC fica no catálogo e no código**; (b) `Problema.Regra` (`"W16-10"`). Recomendo (a): menor superfície; entra em versão menor se pedido.
9. **A listagem das regras.** (a) **internas, sem listagem pública; o catálogo é o `REGRAS.md`**; (b) `DeclaracaoLeiaute.Regras` com código e mensagem, para uma tela. Recomendo (a), pelo mesmo motivo.
10. **O que a 3.1 implementa.** (a) **o motor no núcleo (os passos 5 e 6, os membros novos), a estrutura interna no leiaute (`RegraNFe`, `RegrasNFe400` declarada, sem regra ainda), o catálogo e o teste que cruza o catálogo com o código; as regras começam na 3.2**; (b) também algumas regras já. Recomendo (a): a 3.2 (primeiro lote) e a 3.3 (NFC-e) são as etapas das regras, cada uma com o teste do XML que a SEFAZ rejeitaria.
11. **O catálogo** (seção 6). (a) **uma linha por regra do Anexo I 7.03, 4.1 e 4.2.1, e das NTs posteriores registradas no `NTS.md`, com a situação na lib**; (b) só as que a lib implementa. Recomendo (a): é o que o `PROJETO.md` 6.2 pede ("com as implementadas e as que só a SEFAZ confere"), e o que não está lá é a lista do que falta.
12. **A nota do lote no problema.** (a) **as regras e os callbacks apontam o caminho do documento (`NFe/infNFe/...`); dizer qual nota do lote fica para a 4.4, junto com o caminho do schema no `enviNFe`** (a decisão 4 da 2.6); (b) o núcleo prefixa o caminho com o da mensagem (`enviNFe/NFe[2]/...`) já. Recomendo (a): a 4.4 decide as duas coisas juntas, quando o parâmetro da autorização tiver forma.

### Divergências com o `PROJETO.md` e o `CONTRATOS.md`

- `CONTRATOS.md` 3: o caminho ganha os passos 5 e 6 (seção 3); a numeração das etapas seguintes muda.
- `PROJETO.md` 6.2: as facultativas como aviso (decisão 6) e a válvula `RegrasComoAviso` (decisão 7) não estão no documento; entram nele na implementação.

## 6. O catálogo (forma; o levantamento é da implementação)

Uma seção por grupo do MOC (as regras gerais, 4.1, grupos A, B, D, E e F; a autorização, 4.2.1, grupos A a ZX; e as das NTs no grupo do campo), uma tabela por seção:

| Regra | Modelo | Código | Aplic. | Mensagem | Situação |
|---|---|---|---|---|---|
| W16-10 | 55/65 | 564 | Obrig. | Total do Produto / Serviço difere do somatório dos itens | lib (3.2) |

A **situação**:

- **lib (etapa):** implementada (`RegrasNFe400`), com o teste;
- **a fazer (etapa):** cabe na lib e ainda não foi feita (a etapa prevista: 3.2, 3.3, ou a fase da operação);
- **schema:** o XSD do pacote embutido já confere (tamanho, padrão, enumeração, obrigatoriedade);
- **escrita:** a montagem já impede (a chave montada pelos campos, a escolha do XSD, o campo do núcleo; os `NFE-*`);
- **SEFAZ:** só o autorizador confere (cadastro do contribuinte, duplicidade, numeração, situação de outra nota, a UF do web service, o certificado do transporte, tabelas que a lib não tem);
- **outra operação:** é de uma mensagem que não é a nota (lote, evento, inutilização, consulta), com a fase dela.

Cada linha cita a fonte (o MOC 7.03 ou a NT e a versão), e a regra que uma NT alterou ou revogou aparece com a versão vigente. O **teste do catálogo** lê as tabelas e confere, nos dois sentidos, que toda `RegraNFe` está marcada "lib" com o mesmo código, modelo e aplicação, e que toda linha "lib" tem a sua `RegraNFe`.

## 7. Testes previstos

- **No núcleo, com o leiaute falso:** a ordem das etapas e a parada (o `Erro` do schema não chama as regras; o das regras não chama os callbacks); todos os documentos e todas as funções rodam; o `RegrasComoAviso` rebaixa só o código dele e não toca o schema nem os callbacks; o `DocumentoVerificado` com o modelo, a chave e o XML assinado do documento (o pai do `Id`, com a `Signature`); o `Campo` em cada elemento e atributo, fora da `Signature`, com o caminho do schema (índice só no repetido, `@` no atributo) e a propriedade da conversão; sem `Validacao`, nada é percorrido; sem documentos, nada é chamado; o `Id` que não existe, a lista nula, o problema nulo e a exceção da função; o `Id` repetido no `PedidoWS`; o `DeclararRegras` fora do `Declarar` e nulo; o código vazio no `RegrasComoAviso`; chamadas em paralelo.
- **No leiaute:** a `RegrasNFe400` declarada com uma lista de teste (a função lê o modelo; com o modelo nulo, lê o XML); o problema com o código, a mensagem, o caminho e a propriedade; a facultativa como aviso; o teste do catálogo.

## 8. Fora desta etapa

- As regras em si: o primeiro lote (3.2: chave e DV, CPF e CNPJ, totais, CST × grupo, CFOP × `idDest`, `indIEDest` × IE, datas, `cClassTrib`) e as da NFC-e (3.3).
- As regras do lote (476, 765) e o caminho da nota no lote (4.4); as dos eventos e da inutilização (fase 6), com o `DocumentoPedido` do evento.
- Um `Validar` público (validar uma nota sem transmitir) e as regras na leitura (`LeiauteNFe400.Ler`): fora, como na 2.5 (decisão 4); entram em versão menor se pedidos.
