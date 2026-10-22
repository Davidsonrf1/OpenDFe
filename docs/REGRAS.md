# Regras do MOC e callbacks (fase 3)

Desenho do motor de regras (`PROJETO.md` 6.2 e 6.3, etapa 3.1): onde as regras do leiaute e os callbacks de quem usa entram no caminho do `ExecutarAsync`, o que o núcleo ganha de público para isso e a forma do catálogo das rejeições. **Aprovado em 2026-10-22** (as doze decisões da seção 5, todas pela recomendação) e **implementado na mesma data** (etapa 3.1), com os ajustes da seção 9. O catálogo das rejeições está na seção 10; o lote de regras da etapa 3.2, com a decisão de ficar no schema mais um lote pequeno de regras, na seção 11.

O critério é o do `CONTRATOS.md`: a menor superfície pública que atende as fases 3 a 8, fácil de estender em versão menor (`PROJETO.md` 12.3), com a regra de nomes sem preposição. As regras em si (a conta do total, o DV do CPF) ficam **internas** no leiaute; o público é só a costura no núcleo.

**Conferido na fonte (2026-10-22):** o MOC 7.0, Anexo I, versão 7.03 (o mesmo PDF das etapas 1.4 a 2.3, com o SHA-256 na seção 10.1), seções 4.1 (regras gerais, grupos A, B, D, E e F) e 4.2.1 (autorização, grupos A a ZX, cerca de 495 regras com 426 códigos distintos); a coluna "Aplic." (`Obrig.`, `Facul.`) e o "Efeito" (`Rej.`, `Den.`). O Anexo 7.03 consolida as NTs até a 2020.006; as regras das NTs seguintes (2021.002 a 2026.009, as do `NTS.md`) acrescentam cerca de 400. O levantamento (seção 10) conferiu o Anexo e as 25 NTs vigentes com regras da autorização, baixados de novo nesta etapa.

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

- **`RegraNFe`:** o código da rejeição (`"564"`), o identificador do MOC ou da NT (`"W07-10"`, `"UB12-10"`), os modelos em que vale (55, 65 ou os dois), a aplicação (obrigatória ou facultativa), a mensagem do MOC (sem o "Rejeição: ") e a conferência (`Func<NotaFiscal, ContextoRegra, IEnumerable<...>>`, com o que a regra achou: o caminho, a propriedade e o detalhe).
- **`RegrasNFe400`:** a lista, e a função entregue a `DeclararRegras`: lê o `Modelo` como `NotaFiscal` (ou, nulo, lê o XML pelo `LeitorNFe` da 2.5) e roda as regras do documento do núcleo.
- **O problema da regra:** `Codigo` = o código da rejeição; `Mensagem` = a do MOC, mais o detalhe com os valores que não são dado pessoal ("vProd 100,00; soma dos itens 99,00"; o CPF errado não vai na mensagem, `PROJETO.md` 6.4); `Tag`, `Caminho` (do documento, como o do callback) e `Propriedade` do campo que a regra aponta. Regra obrigatória = `Erro`; facultativa = `Aviso` (decisão 6).
- **Robustez:** as regras rodam sobre uma nota que passou no schema, mas leem o modelo, e o campo nulo é tratado como "não se aplica" (o schema já apontou o obrigatório).
- **O relógio** (as regras de data: emissão no futuro, NFC-e atrasada) é o `ConfiguracaoDFe.Relogio` no instante da validação, com as tolerâncias e as exceções de contingência do MOC.

## 5. Decisões (aprovadas em 2026-10-22, todas pela recomendação)

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

## 6. O catálogo (a forma; o levantamento está na seção 10)

Uma seção por grupo do MOC (as regras gerais, 4.1, grupos A, B, D, E e F; a autorização, 4.2.1, grupos A a ZX; e as das NTs no grupo do campo), uma tabela por seção:

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| W07-10 | 55/65 | 564 | Obrig. | Total do Produto / Serviço difere do somatório dos itens | NT 2025.002 v1.52 | lib (3.2) |

A **situação**:

- **lib (etapa):** implementada (`RegrasNFe400`), com o teste;
- **a fazer (etapa):** cabe na lib e ainda não foi feita (a etapa prevista: 3.2, 3.3, ou a fase da operação);
- **schema:** o XSD do pacote embutido já confere (tamanho, padrão, enumeração, obrigatoriedade);
- **escrita:** a montagem já impede (a chave montada pelos campos, a escolha do XSD, o campo do núcleo; os `NFE-*`);
- **SEFAZ:** só o autorizador confere (cadastro do contribuinte, duplicidade, numeração, situação de outra nota, a UF do web service, o certificado do transporte); **SEFAZ (tabela):** precisa de uma tabela oficial que a lib não embute (município, NCM, CEST);
- **outra operação (fase):** é de uma mensagem que não é a nota (lote, evento, inutilização, consulta), com a fase dela;
- **excluída:** a NT que a republica a marca como excluída.

Cada linha cita a fonte (o MOC 7.03 ou a NT e a versão), e a regra que uma NT alterou ou revogou aparece com a versão vigente. O **teste do catálogo** lê as tabelas e confere, nos dois sentidos, que toda `RegraNFe` está marcada "lib" com o mesmo código, modelo e aplicação, e que toda linha "lib" tem a sua `RegraNFe`.

## 7. Testes previstos

- **No núcleo, com o leiaute falso:** a ordem das etapas e a parada (o `Erro` do schema não chama as regras; o das regras não chama os callbacks); todos os documentos e todas as funções rodam; o `RegrasComoAviso` rebaixa só o código dele e não toca o schema nem os callbacks; o `DocumentoVerificado` com o modelo, a chave e o XML assinado do documento (o pai do `Id`, com a `Signature`); o `Campo` em cada elemento e atributo, fora da `Signature`, com o caminho do schema (índice só no repetido, `@` no atributo) e a propriedade da conversão; sem `Validacao`, nada é percorrido; sem documentos, nada é chamado; o `Id` que não existe, a lista nula, o problema nulo e a exceção da função; o `Id` repetido no `PedidoWS`; o `DeclararRegras` fora do `Declarar` e nulo; o código vazio no `RegrasComoAviso`; chamadas em paralelo.
- **No leiaute:** a `RegrasNFe400` declarada com uma lista de teste (a função lê o modelo; com o modelo nulo, lê o XML); o problema com o código, a mensagem, o caminho e a propriedade; a facultativa como aviso; o teste do catálogo.

## 8. Fora desta etapa

- As regras em si: o primeiro lote (3.2: chave e DV, CPF e CNPJ, totais, CST × grupo, CFOP × `idDest`, `indIEDest` × IE, datas, `cClassTrib`) e as da NFC-e (3.3).
- As regras do lote (476, 765) e o caminho da nota no lote (4.4; feitas, seção 13); as dos eventos e da inutilização (fase 6), com o `DocumentoPedido` do evento.
- Um `Validar` público (validar uma nota sem transmitir) e as regras na leitura (`LeiauteNFe400.Ler`): fora, como na 2.5 (decisão 4); entram em versão menor se pedidos.

## 9. Implementação (etapa 3.1, 2026-10-22)

- **No núcleo (público, 14 símbolos novos no `PublicAPI.Unshipped.txt`):** `DocumentoPedido` (`Id`, `Modelo`, `Chave`, `Propriedade`), `PedidoWS.Documentos`, `DeclaracaoLeiaute.DeclararRegras` e `ConfiguracaoDFe.RegrasComoAviso`, como na seção 2. O `DFe.Criar` recusa a lista nula e o código nulo ou vazio (`ArgumentException`) e copia os códigos.
- **No núcleo (interno):** `MotorRegras`, com os passos 5 e 6 do `ExecutarAsync` (seção 3): acha os documentos no XML assinado (uma leitura, sem DTD nem entidade externa), roda as funções de regras, rebaixa os códigos de `RegrasComoAviso` e chama os callbacks, percorrendo os campos só quando o `Campo` existe.
- **No leiaute (interno):** `RegraNFe`, `AplicacaoRegra`, `OcorrenciaRegra` (o caminho, o detalhe e, se a regra quiser, a propriedade), `ContextoRegra` (os `ValoresNucleo`, a chave e o instante pelo relógio) e `RegrasNFe400`, com a lista vazia (as regras começam na 3.2) e a função declarada pelo `LeiauteNFe400.Declarar` nos dois documentos. A propriedade do problema sai do caminho pela `CaminhoModeloNFe` (2.6), e a tag, do último passo.
- **Ajustes ao desenho** (a conferir pelo usuário):
  - o `Id` que aparece mais de uma vez na mensagem, ou que é da raiz (sem pai), também é `InvalidOperationException`, como o que não existe;
  - o `DocumentoVerificado.Xml` é o documento sozinho, com a declaração do namespace que ele herdava da mensagem (`<NFe xmlns="...">`): é XML válido e a assinatura continua conferindo (o C14N já incluía o namespace herdado);
  - no callback `Campo`, os atributos de um elemento vêm antes dos filhos dele (a ordem do texto), e o elemento que só tem a `Signature` dentro não é campo;
  - a mensagem do problema da regra junta o detalhe entre parênteses ("Total ... difere do somatório dos itens (vProd 100,00; soma dos itens 99,00)").
- **O catálogo** (seção 10): 1.068 linhas, do Anexo I 7.03 e de 25 NTs, levantadas por um script (fora do repositório, no padrão do gerador da 2.4) e com a situação numa primeira classificação, que as etapas 3.2 e 3.3 revisam.
- **Testes:** `Nucleo.Testes` com 475 (19 novos, `MotorRegrasTestes`, com o leiaute falso, que ganhou os documentos no eco) e `NFe.Testes` com 294 (14 novos: `RegrasNFe400Testes` e `CatalogoRegrasTestes`, que lê o `REGRAS.md` copiado para a saída). Duas mutações provadas: uma linha "lib" sem regra no catálogo (acusada pela contagem e pelo cruzamento) e a parada depois das regras tirada do `DFe` (três testes acusam).

## 10. Catálogo das rejeições

O levantamento da etapa 3.1: as regras de validação da autorização da NF-e e da NFC-e no Anexo I do MOC 7.0 (versão 7.03) e nas NTs vigentes que as criam ou alteram, uma linha por regra e código, com a situação na lib (seção 6). As tabelas são lidas pelo teste do catálogo (`CatalogoRegrasTestes`): a linha "lib" e a `RegraNFe` andam juntas. Quem implementar uma regra troca a situação da linha para "lib (etapa)"; a NT nova acrescenta ou republica linhas com a fonte dela.

### 10.1. Fontes

Baixadas em 2026-10-22: as NTs do Portal Nacional da NF-e (Documentos > Notas Técnicas; a lista das vigentes era a mesma de 01/10/2026), o Anexo I do portal da SVRS (Documentos > Manuais, `moc7-anexo-i-leiaute-e-rv.pdf`), igual ao baixado nas etapas anteriores. O SHA-256 é o do PDF baixado.

| Fonte | Versão | Publicada | Assunto | SHA-256 |
|---|---|---|---|---|
| MOC 7.0, Anexo I | 7.03 | 16/12/2020 | leiaute e regras de validação da NF-e e da NFC-e (consolida as NTs até a 2020.006) | `5eb4cf2010b10b0b62f78197c4eb64025f24535d4c6e61158bc7806dd008f55d` |
| NT 2020.005 | 1.21 | 15/10/2021 | atualização de regras de validação | `3ce019a6ea73083d65a86123faa5ecb5ec4e76a32dd143e732079862682d7468` |
| NT 2020.006 | 1.31 | 26/09/2022 | intermediador da transação | `449c1816a67555082891be24f509c57b8e71096f3f8efaf9a71fcb79c2640600` |
| NT 2021.004 | 1.35 | 01/11/2022 | atualização de regras de validação | `e33f067c25a024e3cddd99162baa337eaa7c090f4476c44af394059de3e3ae16` |
| NT 2022.003 | 1.11 | 25/01/2023 | novos campos e regras de validação | `5eda1dbc0be00922e46972836fc81fa605772298ca6f80a96bac5210ba134e31` |
| NT 2022.004 | 1.10 | 14/02/2023 | regra do ISSQN | `58c4bfdbb0878a8240024317f376df57da595aa6cf726ef175ee4f4a097ea05d` |
| NT 2024.001 | 1.20 | 29/08/2024 | CRT 4 (MEI) | `fc5e648cac11ed84e1107f273893776af2b9cfa748a0a5332748100e82e05b86` |
| NT 2022.005 | 1.11 | 11/10/2024 | ICMS na venda interestadual a consumidor final | `e6f12d52ceafb77d366769db9b59fbb15b1600c1b2ed58face5d6fa16a3295db` |
| NT 2023.004 | 1.20 | 07/10/2024 | inclusão e alteração de campos e regras | `1c6db64a5c32b176f8e60baf2ebbfed24632169ddfcd80632c1b5cc65b9ac71d` |
| NT 2021.002 | 1.12 | 28/01/2025 | Nota Fiscal Fácil (NFF) | `a67d9e70cff61bdf6a0913bb5c7ca6b30d82332c0204d443e4671a8dd293d64e` |
| NT 2023.001 | 1.60 | 09/06/2025 | tributação monofásica dos combustíveis | `2b18d99fdb4989a12044cd491c176a2199ab19bb2462406a6ac03376b7701fe5` |
| NT 2019.001 | 1.70 | 18/08/2025 | criação e atualização de regras | `8c03e1d53893f211b4967a5f6da0a1a5f9c46b0862e23ee5b63816657e79ff00` |
| NT 2025.001 | 1.03 | 29/09/2025 | alteração de regras (QR Code, lote) | `333b69f71debe5040585b677b574bb13b39362aa7934b904edc59cdf83cded02` |
| NT 2023.002 | 1.01 | 18/11/2025 | NFC-e do produtor rural, fim da denegação na NFC-e | `b0cb9528bfaa6e0b5ac2986c879898eebcbb9457e97fcf0af985a00659649031` |
| NT 2022.002 | 1.30a | 26/03/2026 | equiparação à exportação e outras | `45b96f11d192afafba1d061b4f08eb1047c2609a60b940217a8a37921348b54f` |
| NT 2024.003 | 1.10 | 11/05/2026 | alteração de regras (agrotóxicos) | `d7609f48772db2ccbff6784b3e51a138433733ddff3deba84640622f82397966` |
| NT 2026.004 | 1.01 | 08/06/2026 | CNPJ alfanumérico (só schema; sem regra de rejeição) | `5f24a25351e790692754b07bbefac42ac67e70167a62a7806395d880f56675be` |
| NT 2026.001 | 1.02b | 31/07/2026 | Provedor de Assinatura e Autorização (PAA) | `703310f95bff855aad9a4a7004515ce29ffff343334c53f0c45e9b32390296cd` |
| NT 2026.006 | 1.00 | 25/08/2026 | vinculação com a transação de pagamento (split payment) | `47a65abde315fed1e6a8c6085a67ca483935cb57877ae5715d365377a725e05d` |
| NT 2026.009 | 1.00 | 09/09/2026 | correção de regra | `50b7dec40bba8928b4d544feb4fdbd8c8bac16e37c99d926ddb2834cf246f15d` |
| NT 2021.003 | 1.50 | 25/09/2026 | regras do GTIN | `28b7e26b1a515cdd8180084a71c6b8bf4eb6fa7745ecafba878fbeb75547b363` |
| NT 2023.003 | 1.40 | 25/09/2026 | alteração de regras | `18b8ce0d3f5fdf46b32bce5588f9f7bef88e123b07ac3bb1adf855339971f7f8` |
| NT 2025.002 | 1.52 | 01/10/2026 | Reforma Tributária do Consumo (IBS, CBS e IS) | `54ed893327040a20a4a3c0562d342953542a1dc98e55d73dbe607d6c13d3d416` |
| NT 2026.002 | 1.11 | 01/10/2026 | DANFE Simplificado Tipo 2 | `72c033b2c7490710f61488a14d0bb3d7bb54e5345aa8d33d6ff2c270c228f046` |
| NT 2026.007 | 1.10 | 01/10/2026 | LCC-RFB e CCC; contribuinte exclusivo do IBS/CBS | `4a9d9de64504f4091c55e3e8a2d499116d4bac1a2009a874a7b9ce26d9fe8b10` |
| NT 2026.008 | 1.00 | 01/10/2026 | valor líquido do produto; ICMS previsto no pagamento antecipado | `bc56e76ae5f3354f7ae636d87908840c5c685284f0f8f32bb29579f8124e44cd` |

### 10.2. Método e situação

- **Extração:** o texto de cada PDF pelo `pdftotext -raw`, uma linha por regra (o identificador, o modelo, o texto da regra, a aplicação, o código e a descrição do erro); as palavras que a extração colou separadas pelo vocabulário da extração `-layout`. Dez regras com a aplicação e o código em linhas separadas foram conferidas e preenchidas à mão (YA03 a YA06 da NT 2025.001, BA02a-10 da NT 2022.002, ZG02-36 e ZG02-40 da NT 2026.001).
- **Mescla:** uma linha por regra e código. Primeiro o Anexo I 7.03; depois as NTs, na ordem da publicação da versão vigente (10.1). A regra que uma NT republica substitui a do Anexo (ou a da NT anterior) com o mesmo identificador e o mesmo código; a regra que uma NT marca como excluída fica com a situação "excluída".
- **A mensagem** é a descrição do erro na fonte, sem o "Rejeição: " e sem as observações; os marcadores como `[nItem:nnn]` ficam como estão.
- **A situação** é uma primeira classificação, por critérios explícitos sobre o texto da regra (o script fica fora do repositório, como o do escritor da 2.4): "SEFAZ" para o banco de dados do autorizador (cadastro, duplicidade, outra nota, credenciamento), a denegação, o limite que a SEFAZ define e o PAA; "SEFAZ (tabela)" para a tabela oficial que a lib não embute (município, NCM, CEST); "escrita" para o que a montagem já garante (o `Id` montado pelos campos, o namespace sem prefixo, o UTF-8, o modelo e a UF do núcleo, a assinatura); "schema" para a 215 e a 225; "outra operação" para o lote (4.4) e os eventos (6); "a fazer (3.3)" para as do modelo 65 sozinho; "a fazer (3.2)" para as do primeiro lote do plano (chave e DV, CPF e CNPJ, totais contra a soma dos itens, CST × grupo, CFOP × `idDest`, `indIEDest` × IE, datas, `cClassTrib`); "a fazer" para o resto que cabe na lib, sem etapa. **As etapas 3.2 e 3.3 revisam a situação das regras que tocam**, e a classificação errada se corrige na linha.
- **Fora do catálogo:** as regras de consumo indevido (Anexo I, 4.3, código 656), só da SEFAZ; e as NTs só de evento, DANFE ou outro serviço (EPEC 2014.001, manifestação 2020.001, comprovante de entrega 2021.001, ator interessado 2020.007, conciliação financeira 2024.002, insucesso na entrega 2023.005, distribuição 2014.002, DANFE 2020.004, 2026.003 e 2026.010), que entram com as fases delas.

| Situação | Regras |
|---|---|
| lib | 104 |
| a fazer (5.1) | 36 |
| a fazer | 640 |
| schema | 2 |
| escrita | 17 |
| SEFAZ | 193 |
| SEFAZ (tabela) | 15 |
| outra operação (6) | 53 |
| excluída | 8 |
| **total** | **1068** |

### 10.3. Regras gerais (Anexo I, 4.1)

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| A01 | 55/65 | 280 | Obrig. | Certificado Transmissor inválido | MOC 7.03 | SEFAZ |
| A02 | 55/65 | 281 | Obrig. | Certificado Transmissor Data Validade | MOC 7.03 | SEFAZ |
| A03 | 55/65 | 283 | Obrig. | Certificado Transmissor - erro Cadeia de Certificação | MOC 7.03 | SEFAZ |
| A04 | 55/65 | 286 | Obrig. | Certificado Transmissor erro no acesso a LCR | MOC 7.03 | SEFAZ |
| A05 | 55/65 | 284 | Obrig. | Certificado Transmissor revogado | MOC 7.03 | SEFAZ |
| A06 | 55/65 | 285 | Obrig. | Certificado Transmissor difere ICP-Brasil | MOC 7.03 | SEFAZ |
| A07 | 55/65 | 282 | Obrig. | Certificado Transmissor sem CNPJ/CPF | MOC 7.03 | SEFAZ |
| B01 | 55/65 | 214 | Obrig. | Tamanho da mensagemexcedeuo limite estabelecido | MOC 7.03 | SEFAZ |
| B02 | 55/65 | 243 | Facul. | XML Mal Formado | MOC 7.03 | escrita |
| B03 | 55/65 | 108 | Obrig. | Serviço Paralisado Momentaneamente (curto prazo) | MOC 7.03 | SEFAZ |
| B04 | 55/65 | 109 | Obrig. | Serviço Paralisado sem Previsão | MOC 7.03 | SEFAZ |
| B05 | 55/65 | 410 | Obrig. | UF informada no campo cUF não é atendida pelo WebService | MOC 7.03 | escrita |
| B06 | 55/65 | 239 | Obrig. | Versão do arquivo XML não suportada | MOC 7.03 | escrita |
| D01 | 55/65 | 215 | Obrig. | Falha no schema XML | MOC 7.03 | schema |
| D01 | 55/65 | 225 | Obrig. | Falha no Schema XML do lote de NFe | MOC 7.03 | schema |
| D01a | 55/65 | 516 | Facul. | Falha no schema XML – inexiste a tag raiz esperada para a mensagem | MOC 7.03 | escrita |
| D01a | 55/65 | 565 | Facul. | Falha no schema XML – inexiste a tag raiz esperada para o lote de NF-e | MOC 7.03 | escrita |
| D01b | 55/65 | 517 | Facul. | Falha no schema XML – inexiste atributo versao na tag raiz da mensagem | MOC 7.03 | escrita |
| D01b | 55/65 | 568 | Facul. | Falha no schema XML – inexiste atributo versao na tag raiz do lote de NF-e | MOC 7.03 | escrita |
| D01d | 55/65 | 587 | Facul. | Usar somente o namespace padrão da NF-e | MOC 7.03 | escrita |
| D01e | 55/65 | 588 | Facul. | Não é permitida a presença de caracteres de edição no início/fim da mensagem ou entre as tags da mensagem | MOC 7.03 | escrita |
| D02 | 55/65 | 404 | Obrig. | Uso de prefixo de namespace não permitido | MOC 7.03 | escrita |
| D03 | 55/65 | 402 | Obrig. | XML da área de dados com codificação diferente de UTF-8 | MOC 7.03 | escrita |
| D04 | 55/65 | 491 | Obrig. | O tpEvento informado inválido | MOC 7.03 | outra operação (6) |
| D05 | 55/65 | 492 | Obrig. | O verEvento informado inválido | MOC 7.03 | outra operação (6) |
| D06 | 55/65 | 493 | Obrig. | Evento não atende o Schema XML específico | MOC 7.03 | outra operação (6) |
| E01 | 55/65 | 290 | Obrig. | Certificado Assinatura inválido | MOC 7.03 | SEFAZ |
| E02 | 55/65 | 291 | Obrig. | Certificado Assinatura Data Validade | MOC 7.03 | SEFAZ |
| E03 | 55/65 | 292 | Obrig. | Certificado de Assinatura sem CNPJ/CPF | MOC 7.03 | SEFAZ |
| E04 | 55/65 | 293 | Obrig. | Certificado Assinatura - erro Cadeia de Certificação | MOC 7.03 | SEFAZ |
| E05 | 55/65 | 296 | Obrig. | Certificado Assinatura erro no acesso a LCR | MOC 7.03 | SEFAZ |
| E06 | 55/65 | 294 | Obrig. | Certificado Assinatura revogado | MOC 7.03 | SEFAZ |
| E07 | 55/65 | 295 | Obrig. | Certificado Assinatura difere ICP-Brasil | MOC 7.03 | SEFAZ |
| F01 | 55/65 | 298 | Obrig. | Assinatura difere do padrão do Sistema | MOC 7.03 | escrita |
| F02 | 55/65 | 297 | Obrig. | Assinatura difere do calculado | MOC 7.03 | lib (4.4) |
| F03 | 55/65 | 227 | Obrig. | CPF do Emitente difere do CPF do Certificado Digital | MOC 7.03 | a fazer |
| GAP03a-1 | 55/65 | 764 | Obrig. | Solicitada resposta síncrona para Lote com mais de uma NF-e (indSinc=1) | MOC 7.03 | escrita |
| GAP03a-2 | 55/65 | 776 | Facul. | Solicitada resposta síncrona para UF que não disponibiliza este atendimento(indSinc=1) | MOC 7.03 | SEFAZ |

### 10.4. Autorização da NF-e e da NFC-e, por grupo (Anexo I, 4.2.1, e as NTs)

#### Grupo A: Dados da NF-e

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| A03-10 | 55/65 | 502 | Obrig. | Erro na Chave de Acesso - Campo Id não corresponde à concatenação dos campos correspondentes | MOC 7.03 | escrita |
| A08 | 55/65 | 818 | Obrig. | CNPJ incorreto na assinatura da NFF | NT 2021.002 v1.12 | a fazer |
| A08 | 55/65 | 832 | Obrig. | CNPJ incorreto na transmissão da NFF | NT 2021.002 v1.12 | a fazer |

#### Grupo B: Identificação da NF-e

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| B02-10 | 55/65 | 226 | Obrig. | Código da UF do Emitente diverge da UF autorizadora | NT 2026.007 v1.10 | SEFAZ |
| B02-20 | 55/65 | 476 | Obrig. | Código da UF diverge da UF da primeira NF-e do Lote | MOC 7.03 | lib (4.4) |
| B03-10 | 55/65 | 897 | Obrig. | Código numérico em formato inválido | NT 2019.001 v1.70 | a fazer |
| B06-10 | 65 | 702 | Obrig. | NFC-e não é aceita pela UFdo Emitente | MOC 7.03 | SEFAZ |
| B06-20 | 55/65 | 765 | Obrig. | Lote só poderá conter NF-e ou NFC-e | MOC 7.03 | lib (4.4) |
| B06-30 | 55 | 450 | Facul. | Modelo da NF-e diferente de 55 | MOC 7.03 | escrita |
| B06-40 | 65 | 775 | Facul. | Modelo da NFC-e diferente de 65 | MOC 7.03 | escrita |
| B07-10 | 65 | 450 | Obrig. | Modelo da NF-e diferente de 55 | NT 2026.001 v1.02b | escrita |
| B09-10 | 55/65 | 703 | Obrig. | Data-Hora de Emissão posterior ao horário de recebimento | MOC 7.03 | lib (3.2) |
| B09-20 | 55 | 228 | Obrig. | Data de Emissão muito atrasada | NT 2026.002 v1.11 | SEFAZ |
| B09-30 | 55 | 315 | Obrig. | Data de Emissão anterior ao início da autorização de Nota Fiscal na UF | MOC 7.03 | SEFAZ |
| B09-40 | 55/65 | 704 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com Data-Hora de emissão atrasada | NT 2026.002 v1.11 | a fazer |
| B09-50 | 65 | 315 | Obrig. | Data de Emissão anterior ao início da autorização de Nota Fiscal na UF | MOC 7.03 | SEFAZ |
| B10-10 | 55/65 | 705 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com data de entrada/saída | NT 2026.002 v1.11 | lib (3.3) |
| B10-20 | 55 | 504 | Facul. | Data de Entrada/Saída posterior ao permitido | MOC 7.03 | a fazer |
| B10-30 | 55 | 505 | Facul. | Data de Entrada/Saída anterior ao permitido | MOC 7.03 | a fazer |
| B10-40 | 55 | 506 | Facul. | Data de Saída menor que a Data de Emissão | MOC 7.03 | lib (3.2) |
| B10a-10 | 65 | 1153 | Obrig. | NFC-e com data de previsão de entrega | NT 2025.002 v1.52 | lib (3.3) |
| B10a-20 | 55 | 1154 | Obrig. | Data de previsão de entrega posterior ao permitido | NT 2025.002 v1.52 | a fazer |
| B10a-30 | 55 | 1155 | Obrig. | Data de previsão de entrega anterior ao permitido | NT 2025.002 v1.52 | a fazer |
| B10a-40 | 55 | 1156 | Obrig. | Data de previsão de entrega não permitida para a finalidade de emissão informada | NT 2025.002 v1.52 | a fazer |
| B10a-50 | 55 | 1157 | Obrig. | Data de previsão de entrega não permitida para a modalidade de frete informada | NT 2025.002 v1.52 | a fazer |
| B11-10 | 55/65 | 706 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 para operação de entrada | NT 2026.002 v1.11 | lib (3.3) |
| B11a-10 | 55/65 | 707 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 para operação interestadual ou com o exterior | NT 2026.002 v1.11 | lib (3.3) |
| B12-10 | 55/65 | 270 | Obrig. | Código Município do Fato Gerador de ICMS inexistente | MOC 7.03 | SEFAZ (tabela) |
| B12-20 | 55/65 | 271 | Obrig. | Código Município do Fato Gerador: difere da UF do emitente | MOC 7.03 | a fazer |
| B21-10 | 65 | 709 | Obrig. | NFC-e com formato de DANFE inválido | MOC 7.03 | lib (3.3) |
| B21-20 | 55 | 710 | Obrig. | NF-e com formato de DANFE inválido | MOC 7.03 | a fazer |
| B22-10 | 55 | 711 | Obrig. | NF-e com contingência off-line em operação não permitida | NT 2026.002 v1.11 | a fazer |
| B22-20 | 65 | 712 | Facul. | NFC-e com contingência off-line para a UF | MOC 7.03 | SEFAZ |
| B22-30 | 55/65 | 570 | Obrig. | Tipo de Emissão 3, 6 ou 7 só é válido nas contingências SCAN/SVC | NT 2021.002 v1.12 | a fazer |
| B22-34 | 65 | 714 | Facul. | NFC-e com contingência inválida (tpEmis=2, 4 (a critério da UF) ou 5) | MOC 7.03 | SEFAZ |
| B22-60 | 55/65 | 713 | Obrig. | Tipo de Emissão diferente de 6 ou 7 para contingência da SVC acessada | MOC 7.03 | a fazer |
| B22-70 | 65 | 783 | Obrig. | NFC-e não é autorizada pela SVC | MOC 7.03 | SEFAZ |
| B23-10 | 55/65 | 253 | Obrig. | Digito Verificador da chave de acesso composta inválida | MOC 7.03 | a fazer |
| B24-10 | 55/65 | 252 | Obrig. | Ambiente informado diverge do Ambiente de recebimento | MOC 7.03 | SEFAZ |
| B25.1-10 | 55 | 1139 | Obrig. | Tipo de nota de débito informado indevidamente | NT 2025.002 v1.52 | a fazer |
| B25.1-20 | 55 | 1009 | Obrig. | Não informado tipo de nota de débito | NT 2025.002 v1.52 | a fazer |
| B25.2-10 | 55 | 1163 | Obrig. | Tipo de nota de crédito informado indevidamente | NT 2025.002 v1.52 | a fazer |
| B25.2-20 | 55 | 1164 | Obrig. | Não informado tipo de nota de crédito | NT 2025.002 v1.52 | a fazer |
| B25.2-30 | 55 | 1145 | Obrig. | Tipo de nota de crédito só pode ser usado a partir de janeiro/2029 | NT 2025.002 v1.52 | a fazer |
| B25.2-40 | 55 | 1152 | Obrig. | Tipo de Operação incompatível com NF-e de Crédito do tipo Retorno | NT 2025.002 v1.52 | a fazer |
| B25-20 | 55/65 | 715 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com finalidade inválida | NT 2026.002 v1.11 | lib (3.3) |
| B25-30 | 55 | 254 | Obrig. | NF-e não possui NF referenciada | NT 2025.002 v1.52 | a fazer |
| B25-40 | 55 | 255 | Obrig. | NF-e possui mais de uma NF referenciada | NT 2025.002 v1.52 | a fazer |
| B25-50 | 55 | 269 | Obrig. | CNPJ/CPF Emitente da NF-e difere do CNPJ/CPF da NF Referenciada | NT 2025.002 v1.52 | a fazer |
| B25-60 | 55 | 678 | Obrig. | NF referenciada com UF diferente desta NF-e | NT 2025.002 v1.52 | a fazer |
| B25-65 | 55 | 1027 | Obrig. | NF referenciada informada indevidamente | NT 2025.002 v1.52 | a fazer |
| B25-70 | 55 | 321 | Obrig. | NF-e de devolução de mercadoria não possui documento fiscal referenciado | MOC 7.03 | a fazer |
| B25-80 | 55 | 1001 | Obrig. | NF-e com finalidade de débito ou crédito somente para IBS/CBS | NT 2026.008 v1.00 | a fazer |
| B25-90 | 55/65 | 1002 | Obrig. | NF-e sem informação de ICMS / ISSQN | NT 2026.007 v1.10 | a fazer |
| B25-100 | 55 | 1003 | Obrig. | NF-e de crédito faz referência a documento fiscal diferente de NF-e modelo 55 | NT 2025.002 v1.52 | a fazer |
| B25-110 | 55 | 1161 | Obrig. | Tipo de Operação incompatível com NF-e de Crédito | NT 2025.002 v1.52 | a fazer |
| B25-120 | 55 | 1162 | Obrig. | Tipo de Operação incompatível com NF-e de Débito | NT 2025.002 v1.52 | a fazer |
| B25a-10 | 55/65 | 716 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 em operação não destinada a consumidor final | NT 2026.002 v1.11 | lib (3.3) |
| B25b-10 | 55 | 794 | Obrig. | NF-e com indicativo de NFC-e com entregaa domicílio | MOC 7.03 | a fazer |
| B25b-20 | 55/65 | 717 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 em operação não permitida | NT 2026.002 v1.11 | a fazer |
| B25b-30 | 65 | 785 | Obrig. | NFC-e com entregaa domicílio não permitida pela UF | MOC 7.03 | SEFAZ |
| B25b-40 | 55 | 864 | Obrig. | NF-e com indicativo de Operação presencial, fora do estabelecimentoe não informada NF referenciada | MOC 7.03 | a fazer |
| B25b-60 | 65 | 1000 | Obrig. | Município do fato gerador do IBS informado indevidamente | NT 2025.002 v1.52 | SEFAZ |
| B25b-70 | 65 | 1005 | Obrig. | Operação presencial fora do estabelecimento sem informação do município do fato gerador do IBS | NT 2025.002 v1.52 | SEFAZ |
| B25c-10 | 55/65 | 434 | Obrig. | NF-e sem indicativo do intermediador | NT 2020.006 v1.31 | a fazer |
| B25c-20 | 55/65 | 435 | Obrig. | NF-e não pode ter o indicativo do intermediador | NT 2020.006 v1.31 | a fazer |
| B25d-10 | 65 | 1099 | Obrig. | Não é permitido o uso do código indicador do local da operação de fornecimento na NFC-e modelo 65 | NT 2025.002 v1.52 | a fazer |
| B25d-20 | 55 | 1102 | Obrig. | Código indicador do local de operação de fornecimento não permitido neste modelo de DFe | NT 2025.002 v1.52 | a fazer |
| B25d-30 | 55 | 1110 | Obrig. | Local de Retirada não informado | NT 2025.002 v1.52 | a fazer |
| B26-10 | 55/65 | 244 | Obrig. | Processo de Emissão pelo Contribuinte incompatível com a Série da NF | NT 2026.001 v1.02b | a fazer |
| B26-20 | 55/65 | 451 | Obrig. | Processo de Emissão pelo Fisco incompatível com a Série da NF | NT 2026.001 v1.02b | a fazer |
| B26-22 | 55 | 631 | Obrig. | Ambiente de autorização não suporta esta operação | NT 2026.001 v1.02b | SEFAZ |
| B26-24 | 55 | 667 | Obrig. | Processo de Emissão pelo PAA incompatível com a Série da NF | NT 2026.001 v1.02b | SEFAZ |
| B26-26 | 55 | 867 | Obrig. | Grupo de informações do Provedor de Assinatura e Autorização não informado | NT 2026.001 v1.02b | SEFAZ |
| B26-28 | 55 | 893 | Obrig. | Grupo de informações do Provedor de Assinatura e Autorização informado indevidamente | NT 2026.001 v1.02b | SEFAZ |
| B26-30 | 55/65 | 370 | Obrig. | Processo de emissão pelo Fisco com Tipo de Emissão inválido | NT 2023.002 v1.01 | a fazer |
| B26-40 | 55/65 | 571 | Obrig. | Processo de emissão pelo Fisco com Certificado de Transmissão incompatível | MOC 7.03 | a fazer |
| B26-50 | 65 | 957 | Obrig. | Tipo de emissão incompatível com o Processo de Emissão | NT 2023.002 v1.01 | lib (3.3) |
| B28-10 | 55/65 | 556 | Obrig. | Justificativa de entrada em contingência não deve ser informada para tipo de emissão normal | MOC 7.03 | a fazer |
| B28-20 | 55/65 | 557 | Obrig. | A Justificativa de entrada em contingência deve ser informada | MOC 7.03 | a fazer |
| B28-30 | 55/65 | 558 | Facul. | Data de entrada em contingência posterior a data de recebimento | MOC 7.03 | a fazer |
| B28-40 | 55/65 | 569 | Facul. | Data de entrada em contingência muito atrasada | MOC 7.03 | a fazer |

#### Grupo BA: Documento Fiscal Referenciado

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| BA01-10 | 55/65 | 708 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 não pode referenciar documento fiscal | NT 2026.002 v1.11 | lib (3.3) |
| BA02-10 | 55 | 547 | Facul. | Chave de Acesso referenciada com Dígito Verificador inválido[nOcor:nnn] | MOC 7.03 | lib (3.2) |
| BA02-14 | 55 | 522 | Facul. | Chave de Acesso referenciada com UF inválida[nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-20 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-24 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-30 | 55 | 552 | Facul. | Chave de Acesso referenciada com CNPJ/CPF inválido [nOcor:nnn] | NT 2026.001 v1.02b | a fazer |
| BA02-34 | 55 | 679 | Facul. | Chave de Acesso referenciada com Modelo inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-35 | 55 | 679 | Obrig. | Chave de Acesso referenciada com Modelo inválido [nOcor:nnn] | NT 2026.002 v1.11 | a fazer |
| BA02-40 | 55 | 683 | Facul. | Chave de Acesso referenciada com Número inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-44 | 55 | 680 | Facul. | Chave de Acesso referenciada em duplicidade na NF-e [nOcor:nnn] | MOC 7.03 | SEFAZ |
| BA02-50 | 55 | 316 | Obrig. | Chave de Acesso referenciada com a mesma Chave de Acesso da Nota Fiscal atual [nOcor:nnn] | MOC 7.03 | a fazer |
| BA02-60 | 55 | 953 | Facul. | Chave de Acesso referenciada com tipo de emissão inválido [nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02-70 | 55 | 954 | Facul. | Chave de Acesso referenciada não permitida para esta operação[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02-80 | 55 | 954 | Facul. | Chave de Acesso referenciada não permitida para esta operação[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-10 | 55 | 951 | Facul. | Chave de Acesso referenciada com código numérico zerado não permitida para finalidade diferente de normal | NT 2022.002 v1.30a | a fazer |
| BA02a-20 | 55 | 522 | Facul. | Chave de Acesso referenciada com UF inválida[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-30 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-40 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-50 | 55 | 552 | Facul. | Chave de Acesso referenciada com CNPJ/CPF inválido [nOcor:nnn] | NT 2026.001 v1.02b | a fazer |
| BA02a-60 | 55 | 679 | Facul. | Chave de Acesso referenciada com Modelo inválido[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-70 | 55 | 683 | Facul. | Chave de Acesso referenciada com Número inválido[nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-74 | 55 | 955 | Facul. | Chave de Acesso referenciada com código numérico não zerado [nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-80 | 55 | 680 | Facul. | Chave de Acesso referenciada em duplicidade na NF-e [nOcor:nnn] | NT 2022.003 v1.11 | SEFAZ |
| BA02a-90 | 55 | 680 | Facul. | Chave de Acesso referenciada em duplicidade na NF-e [nOcor:nnn] | NT 2022.003 v1.11 | SEFAZ |
| BA02a-100 | 55 | 952 | Facul. | Chave de Acesso referenciada com a mesma Chave Natural da Nota Fiscal atual [nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-110 | 55 | 953 | Facul. | Chave de Acesso referenciada com tipo de emissão inválido [nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA02a-120 | 55 | 956 | Facul. | Chave de Acesso referenciada com código numérico zerado não permitida na UF [nOcor:nnn] | NT 2022.003 v1.11 | a fazer |
| BA03-10 | 55 | 681 | Obrig. | Proibido informar NF referenciada [nOcor: nnn] | NT 2026.002 v1.11 | SEFAZ |
| BA05-10 | 55 | 317 | Facul. | NF modelo 1 referenciada com data de emissão inválida [nOcor:nnn] | NT 2026.002 v1.11 | excluída |
| BA06-10 | 55 | 548 | Facul. | NF modelo 1 referenciada com data de emissão inválida [nOcor:nnn] | NT 2026.002 v1.11 | excluída |
| BA10-10 | 55 | 682 | Obrig. | Proibido referenciar NF de Produtor [nOcor: 999] | NT 2026.002 v1.11 | SEFAZ |
| BA10-20 | 55 | 318 | Facul. | Contranota de Produtor sem Nota Fiscal referenciada | NT 2026.002 v1.11 | a fazer |
| BA10-30 | 55 | 319 | Facul. | Contranota de Produtor não pode referenciar somente Nota Fiscal de entrada | NT 2026.002 v1.11 | a fazer |
| BA10-40 | 55 | 320 | Facul. | Contranota de Produtor referencia somente NF de outro emitente | NT 2026.002 v1.11 | a fazer |
| BA10-50 | 55 | 922 | Facul. | Contranota de Produtor só pode referenciar NF-e ou NF de Produtor Modelo 4 | NT 2026.002 v1.11 | a fazer |
| BA12-10 | 55 | 322 | Facul. | NF de produtor referenciada com data de emissão inválida [nOcor:nnn] | NT 2026.002 v1.11 | excluída |
| BA13-10 | 55 | 549 | Facul. | CNPJ da NF referenciada de produtor inválido [nOcor: 999] | NT 2026.002 v1.11 | excluída |
| BA14-10 | 55 | 550 | Facul. | CPF da NF referenciada de produtor inválido [nOcor: 999] | NT 2026.002 v1.11 | excluída |
| BA15-10 | 55 | 551 | Facul. | IE da NF referenciada de produtor inválido | NT 2026.002 v1.11 | a fazer |
| BA19-10 | 55 | 547 | Facul. | Chave de Acesso referenciada com Dígito Verificador inválido[nOcor:nnn] | MOC 7.03 | lib (3.2) |
| BA19-14 | 55 | 522 | Facul. | Chave de Acesso referenciada com UF inválida[nOcor:nnn] | MOC 7.03 | a fazer |
| BA19-20 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA19-24 | 55 | 524 | Facul. | Chave de Acesso referenciada com Ano-Mês inválido [nOcor: 999] | MOC 7.03 | a fazer |
| BA19-30 | 55 | 552 | Facul. | Chave de Acesso referenciada com CNPJ inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA19-34 | 55 | 679 | Facul. | Chave de Acesso referenciada com Modelo inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA19-40 | 55 | 683 | Facul. | Chave de Acesso referenciada com Número inválido[nOcor:nnn] | MOC 7.03 | a fazer |
| BA19-44 | 55 | 680 | Facul. | Chave de Acesso referenciada em duplicidade na NF-e [nOcor:nnn] | MOC 7.03 | SEFAZ |
| BA20-10 | 55 | 684 | Facul. | Duplicidade de Cupom Fiscal referenciado (Modelo, Número de Ordem e COO) [nOcor: 999] | NT 2026.002 v1.11 | SEFAZ |
| BA20-20 | 55 | 923 | Facul. | Referenciado documento de operação interna em operação interestadual. [nOcor: 999] | NT 2026.002 v1.11 | a fazer |
| BA20-30 | 55 | 924 | Obrig. | Proibido referenciar Cupom Fisca [nOcor: 999] | NT 2026.002 v1.11 | a fazer |

#### Grupo BB: Compras Governamentais

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| BB01-10 | 65 | 1006 | Obrig. | NFCe com grupo de compra governamental | NT 2025.002 v1.52 | lib (3.3) |
| BB02-10 | 55 | 1008 | Obrig. | Nota de compra governamental e alíquota dos outros entes informada incorretamente | NT 2025.002 v1.52 | a fazer |
| BB05-10 | 55 | 1195 | Obrig. | Referenciado indevidamente DFe anterior em operação com ente governamental na operação de fornecimento com pagamento posterior | NT 2025.002 v1.52 | a fazer |
| BB05-20 | 55 | 1196 | Obrig. | Não referenciado DFe anterior em operação com ente governamental na operação de recebimento do pagamento com fornecimento já realizado | NT 2025.002 v1.52 | a fazer |
| BB05-30 | 55 | 1197 | Obrig. | Referenciado mais de um DFe anterior em operação com ente governamental na operação de recebimento do pagamento com fornecimento já realizado | NT 2025.002 v1.52 | a fazer |
| BB05-40 | 55 | 1198 | Obrig. | Não referenciado DFe anterior em operação com ente governamental na operação de fornecimento com pagamento já realizado | NT 2025.002 v1.52 | a fazer |
| BB05-50 | 55 | 1199 | Obrig. | Referenciado indevidamente DFe anterior em operação com ente governamental na operação de recebimento do pagamento com fornecimento posterior | NT 2025.002 v1.52 | a fazer |
| BB05-60 | 55 | 1203 | Obrig. | DFe referenciado em operação com ente governamental com tamanho inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-70 | 55 | 1204 | Obrig. | DFe referenciado em operação com ente governamental com Dígito Verificador inválido [nOcor:nnn] | NT 2025.002 v1.52 | lib (3.2) |
| BB05-80 | 55 | 1205 | Obrig. | DFe referenciado em operação com ente governamental com UF inválida [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-90 | 55 | 1206 | Obrig. | DFe referenciado em operação com ente governamental com Ano inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-100 | 55 | 1207 | Obrig. | DFe referenciado em operação com ente governamental com Mês inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-110 | 55 | 1208 | Obrig. | DFe referenciado em operação com ente governamental com CNPJ/CPF inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-120 | 55 | 1209 | Obrig. | DFe referenciado em operação com ente governamental com Modelo inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-130 | 55 | 1210 | Obrig. | DFe referenciado em operação com ente governamental com Número inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-140 | 55 | 1211 | Obrig. | DFe referenciado em operação com ente governamental em duplicidade [nOcor:nnn] | NT 2025.002 v1.52 | SEFAZ |
| BB05-150 | 55 | 1212 | Obrig. | DFe referenciado em operação com ente governamental com a mesma Chave de Acesso da Nota Fiscal atual [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-160 | 55 | 1213 | Obrig. | DFe referenciado em operação com ente governamental com tipo de emissão inválido [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-170 | 55 | 1214 | Obrig. | DFe referenciado em operação com ente governamental deve existir e não estar cancelada [nOcor:nnn] | NT 2025.002 v1.52 | SEFAZ |
| BB05-180 | 55 | 1215 | Obrig. | DFe referenciado em operação com ente governamental deve possuir o mesmo CNPJ base ou CPF do emitente do documento [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-190 | 55 | 1216 | Obrig. | DFe referenciado em operação com ente governamental deve ser de fornecimento com pagamento posterior [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |
| BB05-200 | 55 | 1217 | Obrig. | DFe referenciado em operação com ente governamental deve ser de recebimento do pagamento com fornecimento posterior [nOcor:nnn] | NT 2025.002 v1.52 | a fazer |

#### Grupo C: Identificação do Emitente

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| C02-10 | 55/65 | 207 | Obrig. | CNPJ do emitente inválido | MOC 7.03 | lib (3.2) |
| C02-20 | 55/65 | 560 | Facul. | CNPJ base/CPFdo emitente difere do CNPJ base/CPFda primeira NF-e do lote recebido | MOC 7.03 | lib (4.4) |
| C02-30 | 55/65 | 503 | Obrig. | CNPJ do emitente com Série incompatível | NT 2026.001 v1.02b | a fazer |
| C02a-04 | 65 | 337 | Obrig. | NFC-e para emitente pessoa física | NT 2021.002 v1.12 | SEFAZ |
| C02a-08 | 55 | 652 | Obrig. | NF-e para emitente pessoa física | NT 2022.003 v1.11 | a fazer |
| C02a-10 | 55/65 | 495 | Obrig. | CPF do Emitente com Série incompatível | NT 2026.001 v1.02b | a fazer |
| C02a-14 | 55 | 407 | Obrig. | CPF do Emitente somente no serviço de Nota Fiscal Avulsa no site do Fisco | NT 2021.002 v1.12 | a fazer |
| C02a-20 | 55/65 | 401 | Obrig. | CPF do emitente inválido | NT 2023.002 v1.01 | lib (3.2) |
| C02a-30 | 55/65 | 560 | Facul. | CNPJ Base/CPF do emitente difere do CNPJ Base/CPF da primeira NF-e do lote recebido | NT 2023.002 v1.01 | lib (4.4) |
| C10-10 | 55/65 | 272 | Obrig. | Código Município do Emitente inexistente | MOC 7.03 | SEFAZ (tabela) |
| C10-20 | 55/65 | 273 | Obrig. | Código Município do Emitente: difere da UF do emitente | MOC 7.03 | a fazer |
| C12-10 | 55/65 | 247 | Obrig. | Sigla da UF do Emitente diverge da UF autorizadora | NT 2026.007 v1.10 | SEFAZ |
| C17-10 | 55/65 | 229 | Obrig. | IE do emitente não informada | NT 2026.007 v1.10 | excluída |
| C17-11 | 55 | 166 | Obrig. | UF de autorização não permitida para contribuinte exclusivo do IBS/CBS | NT 2026.007 v1.10 | SEFAZ |
| C17-20 | 55/65 | 209 | Obrig. | IE do emitente inválida | NT 2026.007 v1.10 | a fazer |
| C17-30 | 55/65 | 554 | Obrig. | IE do Emitente informada como ISENTO indevidamente | MOC 7.03 | a fazer |
| C17-42 | 65 | 156 | Obrig. | NFC-e não pode ser emitida por contribuinte exclusivo do IBS/CBS | NT 2026.007 v1.10 | SEFAZ |
| C17-43 | 55 | 157 | Obrig. | Obrigatório informar CNPJ do emitente para contribuinte exclusivo do IBS/CBS | NT 2026.007 v1.10 | a fazer |
| C18-10 | 55/65 | 718 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 não deve informar IE de Substituto Tributário | NT 2026.002 v1.11 | lib (3.3) |
| C18-14 | 55 | 347 | Obrig. | Informada IE do substituto tributário em operação que não é interestadual | MOC 7.03 | a fazer |
| C18-20 | 55 | 478 | Obrig. | Local da entrega não informado para faturamento direto de veículos novos | MOC 7.03 | a fazer |
| C18-30 | 55 | 211 | Obrig. | IE do substituto inválida | MOC 7.03 | a fazer |
| C18-40 | 55 | 363 | Obrig. | IE do substituto tributário idêntica à IE do emitente ou do destinatário | MOC 7.03 | a fazer |
| C18-50 | 55 | 158 | Obrig. | Proibido informar IEST para contribuinte exclusivo do IBS/CBS | NT 2026.007 v1.10 | a fazer |
| C21-10 | 55/65 | 812 | Facul. | Regime Tributário SN,com excesso de sublimite não é permitido para Emitentes desta UF | MOC 7.03 | a fazer |
| C21-20 | 55/65 | 1178 | Obrig. | Utilização de PAA não permitida para contribuinte enquadrado no regime normal | NT 2026.001 v1.02b | SEFAZ |
| C22-10 | 55/65 | 1185 | Obrig. | Município do Emitente não pertence à área incentivada - CBS | NT 2025.002 v1.52 | a fazer |
| C22-20 | 55/65 | 1186 | Obrig. | Inscrição do emitente na Suframa com dígito verificador inválido | NT 2025.002 v1.52 | a fazer |

#### Grupo D: Identificação do Fisco Emitente (NF-e Avulsa)

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| D01-10 | 55/65 | 403 | Obrig. | O grupo de informações da NF-e avulsa é de uso exclusivo do Fisco | MOC 7.03 | a fazer |
| D01-20 | 55/65 | 369 | Obrig. | Não informado o grupo avulsa na emissão pelo Fisco | MOC 7.03 | a fazer |

#### Grupo E: Identificação do Destinatário

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| E01-10 | 55 | 719 | Obrig. | NF-e sem a identificação do destinatário | MOC 7.03 | a fazer |
| E01-20 | 55/65 | 787 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 (tag:tpImp=6) em operação não presencial sem a identificação do destinatário | NT 2026.002 v1.11 | lib (3.3) |
| E02-10 | 55/65 | 208 | Obrig. | CNPJ do destinatário inválido | MOC 7.03 | lib (3.2) |
| E02-20 | 65 | 220 | Obrig. | Destinatário com identificação igual à identificação do emitente | MOC 7.03 | lib (3.3) |
| E03-10 | 55/65 | 237 | Obrig. | CPF do destinatário inválido | MOC 7.03 | lib (3.2) |
| E03a-10 | 55 | 720 | Obrig. | Na operação com Exterior deve ser informada tag idEstrangeiro | NT 2022.002 v1.30a | a fazer |
| E03a-20 | 55 | 721 | Obrig. | Informado idEstrangeiro e Operação não é com consumidor final | MOC 7.03 | a fazer |
| E03a-30 | 55/65 | 925 | Obrig. | NF-e com identificação de estrangeiro e inscrição estadual informada para destinatário | NT 2019.001 v1.70 | a fazer |
| E03a-60 | 55/65 | 372 | Obrig. | Destinatário com identificação de estrangeiro com caracteres inválidos | MOC 7.03 | a fazer |
| E04-10 | 55 | 724 | Obrig. | NF-e sem o nome do destinatário | MOC 7.03 | a fazer |
| E04-20 | 55/65 | 598 | Obrig. | NF-e emitida em ambiente de homologação com Razão Social do destinatário diferente de NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL | NT 2021.002 v1.12 | a fazer |
| E05-10 | 55 | 726 | Obrig. | NF-e sem a informação de endereço do destinatário | MOC 7.03 | a fazer |
| E05-20 | 65 | 788 | Obrig. | NFC-e de entregaa domicílio sem o endereço do destinatário | MOC 7.03 | lib (3.3) |
| E10-10 | 55/65 | 274 | Obrig. | Código Município do Destinatário inexistente | MOC 7.03 | SEFAZ (tabela) |
| E10-20 | 55/65 | 275 | Obrig. | Código Município do Destinatário: difere da UF do Destinatário | MOC 7.03 | a fazer |
| E10-30 | 55 | 509 | Obrig. | Informado código de município diferente de “9999999” para operação com o exterior | MOC 7.03 | a fazer |
| E12-10 | 55 | 727 | Obrig. | Operação com Exterior e UF diferente de EX | NT 2022.002 v1.30a | a fazer |
| E12-30 | 55 | 772 | Obrig. | Operação Interestadual e UF de destino igual à UF de origem | MOC 7.03 | a fazer |
| E12-40 | 55 | 773 | Obrig. | Operação Interna e UF de destino difere da UF de origem | MOC 7.03 | a fazer |
| E12-50 | 55 | 772 | Obrig. | Operação Interestadual e UF de destino igual à UFde origem | MOC 7.03 | a fazer |
| E12-60 | 55 | 773 | Obrig. | Operação Interna e UF de destino difere da UF de origem | MOC 7.03 | a fazer |
| E14-04 | 55/65 | 377 | Obrig. | Código de País do destinatário inexistente | MOC 7.03 | a fazer |
| E14-10 | 55 | 510 | Facul. | Operação com Exterior e Código País destinatário é 1058 (Brasil) ou não informado | NT 2022.002 v1.30a | a fazer |
| E14-20 | 55/65 | 511 | Facul. | Não é de Operação com Exterior e Código País destinatário difere de 1058 (Brasil) | MOC 7.03 | a fazer |
| E14-30 | 55/65 | 926 | Obrig. | Operação com Exterior e país de destino igual a Brasil | NT 2019.001 v1.70 | a fazer |
| E16a-10 | 65 | 789 | Obrig. | NFC-e para destinatário contribuinte de ICMS | MOC 7.03 | lib (3.3) |
| E16a-20 | 55 | 790 | Obrig. | Operação com Exterior para destinatário Contribuinte de ICMS | NT 2022.002 v1.30a | a fazer |
| E16a-30 | 55 | 805 | Obrig. | A SEFAZ do destinatário não permite Contribuinte Isento de Inscrição Estadual | NT 2025.001 v1.03 | a fazer |
| E16a-35 | 55 | 805 | Facul. | A SEFAZ do destinatário não permite Contribuinte Isento de IE | NT 2025.001 v1.03 | a fazer |
| E16a-40 | 55 | 696 | Obrig. | Operação com não contribuinte deve indicar operação com consumidor final | NT 2019.001 v1.70 | lib (3.2) |
| E17-10 | 65 | 729 | Obrig. | NFC-e com informação da IE do destinatário | MOC 7.03 | lib (3.3) |
| E17-20 | 55 | 728 | Obrig. | NF-e sem informação da IE do destinatário | MOC 7.03 | lib (3.2) |
| E17-30 | 55 | 791 | Obrig. | NF-e com indicação de destinatário isento de IE, com a informação da IE do destinatário | MOC 7.03 | lib (3.2) |
| E17-40 | 55 | 792 | Obrig. | Informada a IE do destinatário para operação com destinatário no Exterior | MOC 7.03 | lib (3.2) |
| E17-50 | 55 | 210 | Obrig. | IE do destinatário inválida | MOC 7.03 | a fazer |
| E18-10 | 65 | 730 | Obrig. | NFC-e com Inscrição Suframa | MOC 7.03 | lib (3.3) |
| E18-20 | 55 | 235 | Obrig. | Inscrição SUFRAMA inválida | MOC 7.03 | a fazer |
| E18-30 | 55 | 251 | Obrig. | UF/Município destinatário não pertence a SUFRAMA | NT 2025.002 v1.52 | a fazer |

#### Grupo F: Local da Retirada

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| F01-10 | 55/65 | 669 | Obrig. | Local de retirada informado indevidamente | NT 2026.002 v1.11 | a fazer |
| F02-10 | 55/65 | 512 | Facul. | CNPJ do Local de Retirada inválido | MOC 7.03 | lib (3.2) |
| F02a-10 | 55/65 | 540 | Facul. | CPF do Local de Retirada inválido | MOC 7.03 | lib (3.2) |
| F03 | 55/65 | 213 | Obrig. | CNPJ-Base do Emitente difere do CNPJ-Base do Certificado Digital | NT 2026.001 v1.02b | SEFAZ |
| F03A | 55/65 | 227 | Obrig. | CPF do Emitente difere do CPF do Certificado Digital | NT 2021.002 v1.12 | a fazer |
| F07-10 | 55/65 | 513 | Obrig. | Código Município do Local de Retirada deve ser 9999999 para UF retirada = “EX” | MOC 7.03 | a fazer |
| F07-20 | 55/65 | 276 | Obrig. | Código Município do Local de Retirada inexistente | MOC 7.03 | SEFAZ (tabela) |
| F07-30 | 55/65 | 277 | Obrig. | Código Município do Local de Retirada: difere da UF do Local de Retirada | MOC 7.03 | a fazer |
| F11-10 | 55 | 970 | Obrig. | Código de País inexistente [local de retirada/entrega] | MOC 7.03 | a fazer |
| F15-10 | 55 | 971 | Obrig. | IE inválida [local de retirada/entrega] | MOC 7.03 | a fazer |

#### Grupo G: Local da Entrega

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| G01-10 | 55/65 | 670 | Obrig. | Local de entrega informado indevidamente | NT 2026.002 v1.11 | a fazer |
| G02-10 | 55/65 | 514 | Facul. | CNPJ do Local de Entrega inválido | MOC 7.03 | lib (3.2) |
| G02a-10 | 55/65 | 541 | Facul. | CPF do Local de Entrega inválido | MOC 7.03 | lib (3.2) |
| G07-10 | 55/65 | 515 | Obrig. | Código Município do Local de Entrega deve ser 9999999 para UF entrega = “EX” | MOC 7.03 | a fazer |
| G07-20 | 55/65 | 278 | Obrig. | Código Município do Local de Entrega inexistente | MOC 7.03 | SEFAZ (tabela) |
| G07-30 | 55/65 | 279 | Obrig. | Código Município do Local de Entrega: difere da UF do Local de Entrega | MOC 7.03 | a fazer |
| G11-10 | 55 | 970 | Obrig. | Código de País inexistente [local de retirada/entrega] | MOC 7.03 | a fazer |
| G15-10 | 55 | 971 | Obrig. | IE inválida [local de retirada/entrega] | MOC 7.03 | a fazer |

#### Grupo GA: Autorização para obter o XML

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| GA02-10 | 55/65 | 323 | Obrig. | CNPJ autorizado para download inválido | MOC 7.03 | lib (3.2) |
| GA02-20 | 55/65 | 324 | Obrig. | CNPJ do destinatário já autorizado para download | MOC 7.03 | SEFAZ |
| GA03-10 | 55/65 | 325 | Obrig. | CPF autorizado para download inválido | MOC 7.03 | lib (3.2) |
| GA03-20 | 55/65 | 326 | Obrig. | CPF do destinatário já autorizado para download | MOC 7.03 | SEFAZ |

#### Grupo H: Detalhamento Produtos e Serviços

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| H02-10 | 55/65 | 927 | Obrig. | Número do item fora da ordem sequencial | NT 2019.001 v1.70 | a fazer |

#### Grupo I: Produtos e Serviços

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| I03-10 | 55/65 | 611 | Obrig. | GTIN (cEAN) inválido [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I03-20 | 55/65 | 882 | Obrig. | GTIN (cEAN) com prefixo inválido [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I03-30 | 55/65 | 883 | Obrig. | GTIN (cEAN) sem informação [nItem: 999] | NT 2021.003 v1.50 | a fazer |
| I04-10 | 55/65 | 373 | Obrig. | Descrição do primeiro item diferente de NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| I05-10 | 55/65 | 777 | Obrig. | Obrigatória a informação do NCM completo (redação dada pela NT 2013/005 v 1.20) [nItem: 999] | NT 2024.001 v1.20 | a fazer |
| I05-20 | 55/65 | 778 | Obrig. | Informado NCM inexistente[nItem:nnn] | MOC 7.03 | SEFAZ (tabela) |
| I05-24 | 55/65 | 471 | Obrig. | Informado NCM=00 indevidamente (NT 2014/004) [nItem:999] | MOC 7.03 | a fazer |
| I05-30 | 65 | 779 | Obrig. | NFC-e com NCM incompatível [nItem:999] | MOC 7.03 | SEFAZ |
| I05c-10 | 55/65 | 446 | Obrig. | Informado CEST inexistente [nItem:999] | NT 2020.005 v1.21 | SEFAZ (tabela) |
| I05e-10 | 55/65 | 879 | Obrig. | Informado item “Produzido em Escala NÃO Relevante”e não informado CNPJ do Fabricante [nItem:999] | MOC 7.03 | a fazer |
| I05e-20 | 55/65 | 489 | Obrig. | CNPJ informado inválido (DV ou zeros) [nItem:999] | MOC 7.03 | lib (3.2) |
| I05f-10 | 55/65 | 928 | Facul. | Informado código de benefício fiscal para CST sem benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| I05f-20 | 55/65 | 931 | Facul. | CST não corresponde ao tipo de código de benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| I05f-30 | 55/65 | 934 | Facul. | Não informado valor do ICMS desonerado ou o Motivo de desoneração [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| I05g-10 | 55/65 | 507 | Facul. | Grupo de informações sobre o Crédito Presumido não permitido | NT 2019.001 v1.70 | a fazer |
| I05h-10 | 55/65 | 664 | Facul. | Informado código de crédito presumido (cCredPresumido) incorreto, inexistente ou incompatível na UF [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| I05k-10 | 65 | 1165 | Obrig. | Não é permitido informar a classificação para subapuração do IBS na ZFM na NFC-e modelo 65 [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| I05k-20 | 55 | 1166 | Obrig. | Classificação para subapuração do IBS na ZFM informado indevidamente. [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| I05k-30 | 55 | 1167 | Obrig. | Classificação para subapuração do IBS na ZFM não informada [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| I08-04 | 55/65 | 770 | Obrig. | CFOP Inexistente [nItem:nnn] | MOC 7.03 | a fazer |
| I08-10 | 55 | 518 | Facul. | CFOP de entrada para NF-e de saída | MOC 7.03 | a fazer |
| I08-20 | 55 | 519 | Facul. | CFOP de saída para NF-e de entrada | MOC 7.03 | a fazer |
| I08-30 | 55 | 731 | Obrig. | CFOP de operação com Exterior e idDest <> 3 | MOC 7.03 | lib (3.2) |
| I08-40 | 55 | 732 | Obrig. | CFOP de operação interestadual e idDest <> 2 | MOC 7.03 | lib (3.2) |
| I08-50 | 55 | 733 | Obrig. | CFOP de operação interna e idDest <> 1 | MOC 7.03 | lib (3.2) |
| I08-60 | 55 | 520 | Facul. | CFOP de Operação com Exterior e UF destinatário difere de “EX” | MOC 7.03 | a fazer |
| I08-70 | 55 | 521 | Facul. | Operação Interna e UF do emitente difere da UF do destinatário/remetente contribuinte do ICMS | MOC 7.03 | a fazer |
| I08-90 | 55 | 523 | Facul. | CFOP não é de Operação Estadual e UF emitente igual à UF destinatário [nItem: 999] | NT 2020.006 v1.31 | a fazer |
| I08-94 | 55 | 771 | Facul. | Informado idEstrangeiro em operação interestadual | NT 2024.003 v1.10 | a fazer |
| I08-110 | 55 | 525 | Facul. | CFOP de Importação e não informado dados da DI [nItem:999] | MOC 7.03 | a fazer |
| I08-120 | 55 | 597 | Facul. | CFOP de Importação e não informado dados de IPI | MOC 7.03 | a fazer |
| I08-130 | 55 | 599 | Facul. | CFOP de Importação e não informado dados de II [nItem:999] | MOC 7.03 | a fazer |
| I08-140 | 55 | 327 | Obrig. | CFOP inválido para Nota Fiscal de devolução ou de retorno de mercadoria [nItem:nnn] | NT 2025.002 v1.52 | a fazer |
| I08-141 | 55 | 1179 | Obrig. | CFOP inválido para Nota Fiscal de devolução ou de retorno de mercadoria emitida por MEI [nItem:nnn] | NT 2025.002 v1.52 | a fazer |
| I08-144 | 55 | 328 | Obrig. | CFOP de devolução de mercadoria para NF-e que não é de devolução ou de retorno de mercadoria [nItem:nnn] | NT 2025.002 v1.52 | a fazer |
| I08-150 | 55/65 | 725 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com CFOP inválido[nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| I08-160 | 55/65 | 374 | Obrig. | CFOP incompatível com o grupo de tributação [nItem:999] | NT 2019.001 v1.70 | a fazer |
| I08-170 | 55/65 | 374 | Obrig. | CFOP incompatível com o grupo de tributação [nItem:999] | NT 2019.001 v1.70 | a fazer |
| I08-171 | 55 | 374 | Obrig. | CFOP incompatível com o grupo de tributação [nItem:nnn] | NT 2019.001 v1.70 | a fazer |
| I08-180 | 55 | 375 | Obrig. | NF-e com lançamento relativo a Cupom Fiscal referencia uma NFC-e [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| I08-184 | 55 | 701 | Obrig. | Não informado Nota Fiscal referenciada (Lançamento relativo a Cupom Fiscal) [nItem:nnn] | MOC 7.03 | a fazer |
| I08-186 | 55 | 953 | Obrig. | Informado ECF referenciado para CFOP 5.929 em UF que não permite essa referência | NT 2026.002 v1.11 | a fazer |
| I08-190 | 55 | 701 | Obrig. | Não informado Nota Fiscal referenciada (CFOP de Exportação Indireta) [nItem:nnn] | MOC 7.03 | a fazer |
| I08-191 | 55 | 159 | Obrig. | Operação não permitida para contribuinte exclusivo do IBS/CBS. [nItem:999] | NT 2026.007 v1.10 | SEFAZ (tabela) |
| I08-194 | 55 | 475 | Facul. | Operação não permitida para não contribuinte [nItem:999] | NT 2022.003 v1.11 | SEFAZ |
| I08-198 | 55 | 475 | Facul. | Operação não permitida para não contribuinte [nItem:999] | NT 2022.003 v1.11 | SEFAZ |
| I09-10 | 65 | 734 | Obrig. | NFC-e com Unidade de Comercialização inválida [nItem:999] | MOC 7.03 | SEFAZ |
| I11-10 | 55/65 | 629 | Facul. | Valor do Produto difere do produto Valor Unitário de Comercialização e Quantidade Comercial [nItem:999] | MOC 7.03 | a fazer |
| I11-20 | 55/65 | 630 | Facul. | Valor do Produto difere do produto Valor Unitário de Tributação e Quantidade Tributável [nItem:999] | MOC 7.03 | a fazer |
| I11c-10 | 55/65 | 1278 | Obrig. | Valor Líquido do Produto não informado [nItem: 999] | NT 2026.008 v1.00 | a fazer |
| I11c-20 | 55/65 | 1279 | Obrig. | Valor Líquido do Produto difere do calculado [nItem: 999] | NT 2026.008 v1.00 | a fazer |
| I12-10 | 55/65 | 612 | Obrig. | GTIN da unidade tributável (cEANTrib) inválido [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I12-20 | 55/65 | 884 | Obrig. | GTIN da unidade tributável (cEANTrib) com prefixo inválido [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I12-30 | 55/65 | 885 | Obrig. | GTIN informado, mas não informado o GTIN da unidade tributável [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I12-40 | 55/65 | 886 | Obrig. | GTIN da unidade tributável informado, mas não informado o GTIN [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I12-60 | 55/65 | 888 | Obrig. | GTIN da unidade tributável (cEANTrib) sem informação [nItem:999] | NT 2021.003 v1.50 | a fazer |
| I13-10 | 65 | 735 | Obrig. | NFC-e com Unidade de Tributação inválida [nItem:999] | MOC 7.03 | SEFAZ |
| I13-20 | 55/65 | 854 | Obrig. | Unidade Tributável (tag:uTrib) incompatível com produto informado [nItem: 999] | NT 2023.001 v1.60 | a fazer |
| I14-10 | 55 | 817 | Obrig. | Unidade Tributável incompatível com o NCM informado na operação com Comércio Exterior [nItem:999] | MOC 7.03 | SEFAZ (tabela) |
| I17-10 | 55/65 | 483 | Obrig. | Valor do desconto maior que valor do produto [nItem:nnn] | MOC 7.03 | a fazer |
| I17b-10 | 55/65 | 774 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 (tag:tpImp=6) com indicador de item não participante do total [nItem: 999] | NT 2026.002 v1.11 | a fazer |
| I19-10 | 55 | 329 | Obrig. | Número da DI /DSI inválido | MOC 7.03 | a fazer |
| I23-10 | 55 | 376 | Obrig. | Data do Desembaraço Aduaneiro inválida [nItem:nnn] | MOC 7.03 | a fazer |
| I23b-10 | 55 | 330 | Obrig. | Informar o Valor da AFRMM na importação por via marítima | MOC 7.03 | a fazer |
| I23d1-10 | 55 | 332 | Obrig. | CNPJ/CPF do adquirente ou do encomendante da importação inválido | NT 2023.004 v1.20 | lib (3.2) |
| I23d-10 | 55 | 331 | Obrig. | Informar o CNPJ ou CPF do adquirente ou do encomendante nesta forma de importação | NT 2023.004 v1.20 | a fazer |
| I23d-20 | 55 | 332 | Obrig. | CNPJ/CPF do adquirente ou do encomendante da importação inválido | NT 2023.004 v1.20 | lib (3.2) |
| I23e-10 | 55 | 333 | Obrig. | Informar a UF do adquirente ou do encomendante nesta forma de importação | MOC 7.03 | a fazer |
| I29a-10 | 55 | 334 | Obrig. | Número do processo de drawback não informado na importação | MOC 7.03 | a fazer |
| I29a-20 | 55 | 335 | Obrig. | Número do processo de drawback na importação inválido | MOC 7.03 | a fazer |
| I50-10 | 55/65 | 336 | Obrig. | Informado o grupo de exportação no item em operação que não é com exterior[nItem:nnn] | MOC 7.03 | a fazer |
| I51-10 | 55 | 338 | Obrig. | Número do processo de drawback não informado na exportação | MOC 7.03 | a fazer |
| I51-20 | 55 | 339 | Obrig. | Número do processo de drawback na exportação inválido | MOC 7.03 | a fazer |
| I52-10 | 55 | 340 | Facul. | Não informado o grupo de exportação indireta no item | MOC 7.03 | a fazer |
| I53-10 | 55 | 341 | Obrig. | Número do registro de exportação inválido | MOC 7.03 | a fazer |
| I54-10 | 55 | 342 | Facul. | Chave de Acesso informada na Exportação Indireta com DV inválido | MOC 7.03 | lib (3.2) |
| I54-20 | 55 | 343 | Facul. | Modelo da NF-e informada na Exportação Indireta diferente de 55 | MOC 7.03 | a fazer |
| I54-30 | 55 | 344 | Facul. | Duplicidade de NF-e informada na Exportação Indireta (Chave de Acesso informada mais de uma vez) | MOC 7.03 | SEFAZ |
| I54-40 | 55 | 345 | Facul. | Chave de Acesso informada na Exportação Indireta não consta como NF-e referenciada | MOC 7.03 | a fazer |
| I55-10 | 55 | 346 | Facul. | Somatório das quantidades informadas na Exportação Indireta não corresponde a quantidade total do item | MOC 7.03 | a fazer |
| I70-10 | 55/65 | 465 | Facul. | Número de Controle da FCI inexistente | MOC 7.03 | SEFAZ |
| I83-10 | 55/65 | 877 | Obrig. | Data de fabricação maior que a data de processamento[nItem:999] | MOC 7.03 | lib (3.2) |
| I84-10 | 55/65 | 870 | Obrig. | Data de validade incompatível com data de fabricação [nItem:999] | MOC 7.03 | lib (3.2) |
| I86-10 | 55/65 | 820 | Obrig. | Informado produto fiscal de NFF | NT 2021.002 v1.12 | a fazer |
| I87-10 | 55/65 | 833 | Obrig. | Informada embalagem do produto | NT 2021.002 v1.12 | a fazer |

#### Grupo J: Item / Veículos Novos

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| J01-10 | 55/65 | 736 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de Veículos novos | NT 2026.002 v1.11 | lib (3.3) |
| J19-10 | 55 | 841 | Obrig. | Código do Tipo de Veículo Inexistente [nItem:nnn] | NT 2021.004 v1.35 | a fazer |
| J20-10 | 55 | 842 | Obrig. | Código da espécie de Veículo Inexistente [nItem:nnn] | NT 2021.004 v1.35 | a fazer |
| J20-20 | 55 | 843 | Obrig. | Código da espécie de Veículo incompatível com o tipo do Veículo. [nItem:nnn] | NT 2021.004 v1.35 | a fazer |

#### Grupo K: Item / Medicamentos

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| K01-10 | 55 | 840 | Facul. | NCM de medicamento e não informado o grupo de medicamento (med) [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| K01-20 | 55 | 873 | Obrig. | Operação com medicamentos e não informado os campos de rastreabilidade [nItem: 999] | NT 2026.002 v1.11 | a fazer |

#### Grupo L: Item / Armamentos

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| L01-10 | 55/65 | 738 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de Armamentos | NT 2026.002 v1.11 | lib (3.3) |

#### Grupo LA: Item / Combustível

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| LA01-20 | 55/65 | 660 | Facul. | CFOP de Combustível e não informado grupo de combustível da NF-e [nItem:nnn] | MOC 7.03 | a fazer |
| LA01-30 | 55/65 | 1106 | Obrig. | Não informado grupo de combustível para cClassTrib de Combustível [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| LA02-10 | 55/65 | 761 | Obrig. | Código de Produto ANP inexistente [nItem:999] | MOC 7.03 | SEFAZ (tabela) |
| LA03c-10 | 55/65 | 461 | Obrig. | Informado campos de percentual de GLP e/ouGLGNne/ou GLGNi para produto diferente de GLP [nItem:999] | MOC 7.03 | a fazer |
| LA03c-20 | 55/65 | 855 | Obrig. | Somatório percentuais de GLP derivado do petróleo,GLGNne GLGNi diferente de 100 [nItem: 999] | MOC 7.03 | a fazer |
| LA03d-10 | 55 | 856 | Obrig. | Campo valor de partida não preenchido para produto GLP [nItem: 999] | NT 2023.001 v1.60 | a fazer |
| LA11-10 | 55/65 | 378 | Facul. | Grupo de Combustível sem a informação de Encerrante [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| LA11-20 | 55 | 379 | Obrig. | Grupo de Encerrante na NF-e (modelo 55) para CFOP diferente de venda de combustível para consumidor final [nItem:nnn] | MOC 7.03 | a fazer |
| LA16-10 | 55/65 | 380 | Obrig. | Valor do Encerrante final não é superior ao Encerrante inicial [nItem:nnn] | MOC 7.03 | a fazer |
| LA17-10 | 55/65 | 907 | Obrig. | Grupo de combustível não pode ter o índice de mistura do Biocombustível. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| LA17-20 | 55 | 908 | Obrig. | Obrigatório o preenchimento do índice de mistura do Biocombustível. [nItem:999] | NT 2026.002 v1.11 | a fazer |
| LA18-10 | 55 | 909 | Obrig. | Obrigatório o preenchimento do grupo de UF de origem do combustível [nItem:999] | NT 2026.002 v1.11 | a fazer |
| LA18-20 | 55 | 909 | Obrig. | Obrigatório o preenchimento do grupo de UF de origem | NT 2026.002 v1.11 | a fazer |
| LA18-30 | 55/65 | 747 | Obrig. | Não permitido o preenchimento do grupo de UF de origem do combustível [nItem:999] | NT 2023.001 v1.60 | a fazer |
| LA21-10 | 55/65 | 958 | Obrig. | Somatório dos percentuais originários para a UF do combustível diverge de 100. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| LA21-20 | 55/65 | 958 | Obrig. | Somatório dos percentuais originários para a UF do combustível diverge de 100. [nItem:999] | NT 2023.001 v1.60 | a fazer |

#### Grupo LB: Item / Papel Imune

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| LB01-10 | 55/65 | 348 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo RECOPI | NT 2026.002 v1.11 | lib (3.3) |
| LB01-20 | 55 | 349 | Facul. | Número RECOPI não informado | MOC 7.03 | a fazer |
| LB01-30 | 55 | 350 | Facul. | Número RECOPI inválido | MOC 7.03 | a fazer |

#### Grupo N: Item / Tributo: ICMS

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| N01-10 | 55 | 161 | Obrig. | Proibido informar ICMS para contribuinte exclusivo do IBS/CBS [nItem:999] | NT 2026.007 v1.10 | a fazer |
| N07-10 | 55 | 929 | Facul. | Informado CST de diferimento sem as informações de diferimento [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| N08-10 | 55 | 858 | Obrig. | Grupo de Tributação informado indevidamente [nItem:999] | MOC 7.03 | a fazer |
| N11-10 | 55/65 | 966 | Obrig. | Obrigatório o preenchimento da origem da mercadoria [nItem:nnn] | NT 2025.002 v1.52 | a fazer |
| N12-10 | 55 | 527 | Facul. | Operação de Exportação com informação de ICMS incompatível | MOC 7.03 | a fazer |
| N12-20 | 55/65 | 590 | Facul. | Informado CST para emissor do Simples Nacional (CRT=1) [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N12-30 | 55/65 | 766 | Obrig. | Item com CST indevido [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N12-34 | 55/65 | 381 | Obrig. | Grupo de tributação ICMS90, informando dados do ICMS-ST [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N12-40 | 55/65 | 382 | Obrig. | CFOP não permitido para o CST informado [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N12-44 | 55/65 | 382 | Obrig. | CFOP não permitido para o CST informado [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N12-50 | 55/65 | 741 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com Partilha de ICMS entre UF | NT 2026.002 v1.11 | lib (3.3) |
| N12-60 | 55/65 | 740 | Obrig. | Item com Repasse de ICMS retido por Substituto Tributário [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12-70 | 55 | 508 | Obrig. | CST incompatível na operação com Não Contribuinte [nItem: 999] | NT 2023.003 v1.40 | a fazer |
| N12-80 | 55 | 529 | Obrig. | CST incompatível na operação com Contribuinte Isento de Inscrição Estadual [nItem: 999] | NT 2020.005 v1.21 | a fazer |
| N12-81 | 55/65 | 938 | Facul. | Não informada BCST, pST e ICMSST retido na operação anterior [nItem:999] | MOC 7.03 | a fazer |
| N12-82 | 55/65 | 906 | Facul. | Não informados os campos para informações do ICMS Efetivo. [nItem:nnn] | MOC 7.03 | a fazer |
| N12-84 | 55/65 | 930 | Facul. | CST com benefício fiscal e não informado o código de benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-85 | 55/65 | 930 | Facul. | CST com benefício fiscal e não informado o código de benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-86 | 55/65 | 928 | Facul. | Informado código de benefício fiscal para CST sem benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-88 | 55/65 | 931 | Facul. | CST não corresponde ao tipo de código de benefício fiscal [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-90 | 55/65 | 934 | Facul. | Não informado valor do ICMS desonerado ou o Motivo de desoneração [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-94 | 55/65 | 931 | Facul. | Informado código de benefício fiscal incompatível com CST e UF [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-97 | 55 | 929 | Facul. | Informado CST de diferimento sem as informações de diferimento [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| N12-98 | 55/65 | 946 | Facul. | Informado código de benefício fiscal incorreto ou inexistente na UF [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N12-100 | 55/65 | 959 | Obrig. | NF-e não pode ter preenchimento de Grupo de Tributação do ICMS monofásica sobre combustíveis. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N12-110 | 55/65 | 960 | Obrig. | Obrigatório o preenchimento de Grupo de Tributação do ICMS monofásica sobre combustíveis. [nItem:999] | NT 2025.002 v1.52 | a fazer |
| N12a-10 | 55/65 | 591 | Facul. | Informado CSOSN para emissor que não é do Simples Nacional (CRT diferente de 1 ou 4) [nItem:nnn] | NT 2024.001 v1.20 | a fazer |
| N12a-20 | 55/65 | 383 | Obrig. | Item com CSOSN indevido [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-30 | 55/65 | 384 | Obrig. | CSOSN não permitido para a UF [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-34 | 55/65 | 385 | Obrig. | Grupo de tributação ICMSSN900, informando dados do ICMS-ST [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-40 | 55/65 | 386 | Obrig. | CFOP não permitido para o CSOSN informado [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-44 | 55/65 | 386 | Obrig. | CFOP não permitido para o CSOSN informado [nItem: nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-50 | 55/65 | 938 | Facul. | Não informada BCST, pST e ICMSST retido na operação anterior [nItem:999] | MOC 7.03 | a fazer |
| N12a-60 | 55/65 | 906 | Facul. | Não informados os campos para informações do ICMS Efetivo. [nItem:nnn] | MOC 7.03 | a fazer |
| N12a-70 | 55 | 600 | Obrig. | CSOSN incompatível na operação com Não Contribuinte [nItem: 999] | NT 2024.003 v1.10 | a fazer |
| N12a-80 | 55 | 782 | Obrig. | CSOSN inválido para emitente MEI (CRT=4) [nItem:nnn] | NT 2024.001 v1.20 | a fazer |
| N12a-81 | 55/65 | 782 | Obrig. | CSOSN inválido para emitente MEI (CRT=4) [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N12a-90 | 55 | 337 | Obrig. | CFOP inválido para emitente MEI (CRT=4) [nItem:nnn] | NT 2024.001 v1.20 | a fazer |
| N12a-91 | 55/65 | 337 | Obrig. | CFOP inválido para emitente MEI (CRT=4) [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| N14a-10 | 55 | 665 | Facul. | Não informado código de benefício fiscal de redução de BC (cBenefRBC) quando percentual de redução de BC for maior que zero | NT 2019.001 v1.70 | a fazer |
| N14a-20 | 55 | 666 | Facul. | Informado código de benefício fiscal de redução de BC (cBenefRBC) incorreto, inexistente ou incompatível na UF [nItem: nnn] | NT 2019.001 v1.70 | SEFAZ |
| N16-04 | 55 | 663 | Facul. | Alíquota do ICMS com valor superior a 4 por cento na operação de saída interestadual com produtos importados [nItem:999] | MOC 7.03 | a fazer |
| N16-20 | 55 | 693 | Obrig. | Alíquota de ICMS superior à definida para a operação interestadual [nItem:999] | MOC 7.03 | a fazer |
| N16a-10 | 55 | 351 | Facul. | Valor do ICMS da Operação no CST=51 difere do produto BC e Alíquota [nItem:999] | MOC 7.03 | a fazer |
| N16c-10 | 55 | 352 | Facul. | Valor do ICMS Diferido no CST=51 difere do produto Valor ICMS Operação e percentual diferimento [nItem:999] | MOC 7.03 | a fazer |
| N17-10 | 55 | 353 | Facul. | Valor do ICMS no CST=51 não corresponde a diferença do ICMS operação e ICMS diferido [nItem: 999] | MOC 7.03 | a fazer |
| N17-20 | 55/65 | 528 | Facul. | Valor do ICMS difere do produto BC e Alíquota [nItem:999] | MOC 7.03 | a fazer |
| N17b-10 | 55/65 | 880 | Obrig. | Percentual de FCP igual a zero [nItem:999] | MOC 7.03 | a fazer |
| N17b-20 | 55/65 | 874 | Obrig. | Percentual de FCP inválido [nItem:999] | MOC 7.03 | SEFAZ |
| N17c-10 | 55/65 | 860 | Obrig. | Valor do FCP informado difere de base de cálculo*alíquota [nItem: 999] | NT 2020.005 v1.21 | a fazer |
| N17c-20 | 55 | 876 | Obrig. | Operação interestadual para Consumidor Final e valor do FCP informado em campo diferente de vFCPUFDest(id:NA13) [nItem:999] | MOC 7.03 | a fazer |
| N17c-30 | 55 | 474 | Obrig. | FCP não deve ser destacado na NF-e conforme legislação estadual [nItem:999] | NT 2024.001 v1.20 | a fazer |
| N18-10 | 55 | 932 | Facul. | Informada modalidade de determinação da BC da ST como MVA e não informado o campo pMVAST [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| N18-20 | 55 | 933 | Facul. | Informada modalidade de determinação da BC da ST diferente de MVA e informado o campo pMVAST [nItem: nnn] | NT 2019.001 v1.70 | a fazer |
| N23-10 | 55 | 806 | Obrig. | Operação com ICMS-ST sem informação do CEST [nItem: 999] | MOC 7.03 | a fazer |
| N23b-10 | 55 | 881 | Obrig. | Percentual de FCPST igual a zero [nItem:999] | MOC 7.03 | a fazer |
| N23b-20 | 55 | 875 | Obrig. | Percentual de FCPST inválido [nItem:999] | MOC 7.03 | SEFAZ |
| N23d-10 | 55 | 860 | Obrig. | Valor do FCP informado difere de base de cálculo*alíquota [nItem: 999] | MOC 7.03 | a fazer |
| N27b-10 | 55/65 | 881 | Obrig. | Percentual de FCPST igual a zero [nItem:999] | MOC 7.03 | a fazer |
| N27b-20 | 55/65 | 875 | Obrig. | Percentual de FCPST inválido [nItem:999] | MOC 7.03 | SEFAZ |
| N27d-10 | 55/65 | 860 | Obrig. | Valor do FCP informado difere de base de cálculo*alíquota [nItem: 999] | MOC 7.03 | a fazer |
| N28-10 | 55/65 | 625 | Facul. | Inscrição SUFRAMA deve ser informada na venda com isenção para ZFM [nItem:999] | MOC 7.03 | a fazer |
| N28-12 | 55 | 652 | Obrig. | informado indicador de desoneração inválido para a ZFM [nItem: 999] | NT 2023.004 v1.20 | a fazer |
| N28-20 | 55 | 626 | Facul. | CFOP de operação isenta para ZFM diferente do previsto [nItem: 999] | NT 2019.001 v1.70 | a fazer |
| N28-30 | 55/65 | 627 | Facul. | O valor do ICMS desonerado deve ser informado [nItem:999] | MOC 7.03 | a fazer |
| N33-10 | 55/65 | 906 | Facul. | Não informados os campos do grupo opcional para informações do ICMS Efetivo, Obrigo quando CST = 60 ou CSOSN=500 e operação com consumidor final [nItem:nnn] | MOC 7.03 | a fazer |
| N37a-10 | 55 | 767 | Obrig. | Obrigatório o preenchimento da Quantidade tributada [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N38-10 | 55 | 961 | Obrig. | Alíquota adrem do imposto difere do definido na legislação para o produto. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N39-10 | 55/65 | 962 | Obrig. | Valor do ICMS próprio difere do calculado. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N39a-10 | 55 | 768 | Obrig. | Obrigatório o preenchimento da Quantidade tributada sujeita a retenção [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N40-10 | 55 | 963 | Obrig. | Alíquota adrem do imposto com retenção difere do definido na legislação para o produto. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N41-10 | 55/65 | 964 | Obrig. | Valor do ICMS com retenção difere do calculado. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N41-20 | 55 | 964 | Obrig. | Valor do ICMS com retenção difere do calculado. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N43a-10 | 55/65 | 769 | Obrig. | Obrigatório o preenchimento da Quantidade tributada retida anteriormente [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N44-10 | 55/65 | 965 | Obrig. | Alíquota adrem do imposto retido anteriormente difere do definido na legislação para o produto. [nItem:999] | NT 2023.001 v1.60 | a fazer |
| N45-10 | 55/65 | 966 | Obrig. | Valor do ICMS retido anteriormente difere do calculado. [nItem:999] | NT 2023.001 v1.60 | a fazer |

#### Grupo NA: Item / ICMS para a UF de Destino

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| NA01-10 | 55/65 | 807 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo de ICMS para a UF do destinatário | NT 2026.002 v1.11 | lib (3.3) |
| NA01-20 | 55 | 694 | Obrig. | Não informado o grupo de ICMS para a UF de destino [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| NA01-30 | 55 | 695 | Obrig. | Informado indevidamente o grupo de ICMS para a UF de destino [nItem:999] | MOC 7.03 | a fazer |
| NA09-10 | 55 | 697 | Obrig. | Alíquota interestadual do ICMS com origem diferente do previsto [nItem:999] | MOC 7.03 | a fazer |
| NA09-20 | 55 | 697 | Obrig. | Alíquota interestadual do ICMS com origem diferente do previsto [nItem:999] | MOC 7.03 | a fazer |
| NA09-30 | 55 | 698 | Obrig. | Alíquota interestadual do ICMS incompatível com as UF envolvidas na operação [nItem:999] | MOC 7.03 | a fazer |
| NA11-10 | 55 | 699 | Obrig. | Percentual do ICMS Interestadual para a UF de destino difere do previsto para o ano da Data de Emissão [nItem:999] | MOC 7.03 | a fazer |
| NA13-10 | 55 | 793 | Obrig. | Valor do ICMS relativo ao Fundo de Combate à Pobreza na UF de destino difere do calculado [nItem: 999] | MOC 7.03 | a fazer |
| NA15-10 | 55 | 815 | Obrig. | Valor do ICMS Interestadual para UF de Destino difere do calculado [nItem: 999] | NT 2020.005 v1.21 | a fazer |
| NA17-10 | 55 | 816 | Obrig. | Valor do ICMS Interestadual para UF do Remetente difere do calculado [nItem: 999] | NT 2020.005 v1.21 | a fazer |

#### Grupo NB: ICMS Previsto no Pagamento Antecipado

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| NB01-10 | 65 | 1280 | Obrig. | Grupo do ICMS Previsto no Pagamento Antecipado informado indevidamente [nItem: 999] | NT 2026.008 v1.00 | SEFAZ |
| NB01-20 | 55 | 1280 | Obrig. | Grupo do ICMS Previsto no Pagamento Antecipado informado indevidamente [nItem: 999] | NT 2026.008 v1.00 | a fazer |
| NB01-30 | 55 | 1281 | Obrig. | Grupo do ICMS Previsto no Pagamento Antecipado não informado [nItem: 999] | NT 2026.008 v1.00 | a fazer |

#### Grupo O: Item / Tributo: IPI

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| O01-10 | 55/65 | 742 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do IPI | NT 2026.002 v1.11 | lib (3.3) |
| O06-10 | 55 | 387 | Obrig. | Código de Enquadramento Legal do IPI inválido [nItem:nnn] | MOC 7.03 | a fazer |
| O09-10 | 55 | 388 | Obrig. | Código de Situação Tributária do IPI incompatível com o Código de Enquadramento Legal do IPI [nItem:nnn] | MOC 7.03 | a fazer |

#### Grupo P: Item / Tributo: II

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| P01-10 | 55/65 | 743 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do II | NT 2026.002 v1.11 | lib (3.3) |
| P21-10 | 55/65 | 466 | Obrig. | Evento com Tipo de Autor incompatível | NT 2026.006 v1.00 | SEFAZ |
| P26-10 | 55/65 | 1273 | Obrig. | Meio de pagamento inválido | NT 2026.006 v1.00 | a fazer |
| P27-10 | 55/65 | 1274 | Obrig. | CNPJ do recebedor do pagamento inválido | NT 2026.006 v1.00 | a fazer |

#### Grupo Q: Item / Tributo: PIS

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| Q01-20 | 55 | 745 | Obrig. | NF-e sem grupo do PIS [nItem:nnn] | NT 2025.002 v1.52 | a fazer |

#### Grupo R: Item / Tributo: PIS ST

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| R01-10 | 55/65 | 746 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo do PIS-ST | NT 2026.002 v1.11 | lib (3.3) |

#### Grupo S: Item / Tributo: COFINS

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| S01-20 | 55 | 748 | Obrig. | NF-e sem grupo da COFINS [nItem:nnn] | NT 2025.002 v1.52 | a fazer |

#### Grupo T: Item / Tributo: COFINS ST

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| T01-10 | 55/65 | 749 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com grupo da COFINS-ST | NT 2026.002 v1.11 | lib (3.3) |

#### Grupo U: Item / Tributo: ISSQN

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| U01-10 | 55/65 | 530 | Facul. | Operação com tributação de ISSQN sem informar a Inscrição Municipal | MOC 7.03 | a fazer |
| U01-20 | 55/65 | 592 | Facul. | A NF-e deve ter pelo menos um item de produto sujeito ao ICMS | NT 2022.004 v1.10 | a fazer |
| U01-30 | 55/65 | 887 | Obrig. | Item de Serviço e informado GTIN diferente de SEM GTIN [nItem:999] | NT 2021.003 v1.50 | a fazer |
| U05-10 | 55/65 | 287 | Obrig. | Código Município do Fato Gerador de ISSQN inexistente [nItem:nnn] | MOC 7.03 | SEFAZ (tabela) |
| U06-10 | 55/65 | 844 | Obrig. | Código de Item da Lista de Serviços inexistente. [nItem:nnn] | NT 2021.004 v1.35 | a fazer |
| U14-10 | 55/65 | 389 | Obrig. | Código Município ISSQN inexistente [nItem:nnn] | MOC 7.03 | SEFAZ (tabela) |
| U15-10 | 55/65 | 739 | Obrig. | Código de País do ISSQN Inexistente | MOC 7.03 | a fazer |

#### Grupo UA: Item / Devolução de Tributos

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| UA01-10 | 55 | 354 | Obrig. | Informado grupo de devolução de tributos para NF-e que não tem finalidade de devolução de mercadoria | MOC 7.03 | a fazer |
| UA01-20 | 55/65 | 390 | Obrig. | Nota Fiscal com grupo de devolução de Tributos [nItem: nnn] | NT 2026.002 v1.11 | a fazer |

#### Grupo UB: Informações dos tributos IBS, CBS e IS

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| UB01-10 | 55/65 | 1011 | Obrig. | Não é permitido uso do Imposto Seletivo para esta classificação da operação [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB01-20 | 55/65 | 1012 | Obrig. | É exigido o uso do Imposto Seletivo para esta classificação da operação [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB01-30 | 55/65 | 1013 | Obrig. | É exigido o uso do Imposto Seletivo para esta classificação da operação para este NCM [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB02-10 | 55/65 | 1014 | Obrig. | CST do Imposto Seletivo informado inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB03-10 | 55/65 | 1015 | Obrig. | Classificação Tributária do Imposto Seletivo informada inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB05-10 | 55/65 | 1103 | Obrig. | Valor da Base de cálculo do Imposto Seletivo difere do somatório dos valores que a compõem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB06-10 | 55/65 | 1016 | Obrig. | CST do Imposto Seletivo obriga informação de alíquota de Imposto Seletivo [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB07-10 | 55 | 1017 | Obrig. | Obrigatório informação de alíquota específica de Imposto Seletivo [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB08-10 | 55/65 | 1018 | Obrig. | Unidade tributável (uTrib) e Quantidade tributável (qTrib) do imposto seletivo não informados [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB11-10 | 55/65 | 1019 | Obrig. | Valor do Imposto Seletivo difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB12-10 | 55/65 | 1115 | Obrig. | Grupo IBSCBS não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB12-11 | 55 | 162 | Obrig. | Grupo IBS/CBS obrigatório para contribuinte exclusivo do IBS/CBS [nItem:999] | NT 2026.007 v1.10 | a fazer |
| UB13-10 | 55/65 | 1020 | Obrig. | CST do IBS/CBS informado inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-20 | 55/65 | 1021 | Obrig. | Grupo gIBSCBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-30 | 55/65 | 1022 | Obrig. | Grupo gIBSCBS não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-39 | 55/65 | 1151 | Obrig. | Grupo gIBSCBSMono informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-40 | 55/65 | 1116 | Obrig. | Grupo gIBSCBSMono não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-44 | 55/65 | 1131 | Obrig. | Grupo de transferência de crédito informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB13-45 | 55/65 | 1132 | Obrig. | Grupo de transferência de crédito não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-10 | 55/65 | 1023 | Obrig. | Classificação Tributária do IBS/CBS informada inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-20 | 55/65 | 1024 | Obrig. | Rejeição: Classificação Tributária do IBS e da CBS incompatível com o CST informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-25 | 55/65 | 1025 | Obrig. | cClassTrib do IBS/CBS não permitido neste modelo de DFe [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-30 | 55 | 1043 | Obrig. | Índice de mistura do Biocombustível inferior ao obrigatório para esta Classificação Tributária do IBS e CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-50 | 55 | 1059 | Obrig. | Índice de mistura do Biocombustível superior ao obrigatório para esta Classificação Tributária do IBS e CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-60 | 55 | 1202 | Obrig. | cClassTrib incompatível com a finalidade da nota ou com o tipo de Nota de Débito ou Crédito [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-70 | 55 | 1200 | Obrig. | cClassTrib incompatível com o tipo de Nota de Débito [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB14-80 | 55 | 1201 | Obrig. | cClassTrib incompatível com o tipo de Nota de Crédito [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB16-10 | 55/65 | 1104 | Obrig. | Valor da Base de cálculo do IBS e CBS difere do somatório dos valores que a compõem [nItem: 999] | NT 2026.008 v1.00 | a fazer |
| UB18-10 | 55/65 | 1026 | Obrig. | Alíquota do IBS da UF inválida [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB22-10 | 55/65 | 1029 | Obrig. | CST do IBS/CBS informado não permite informação de diferimento Estadual [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB22-20 | 55/65 | 1030 | Obrig. | CST do IBS/CBS informado obriga informação de diferimento Estadual [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB23-10 | 55/65 | 1031 | Obrig. | Valor do Diferimento da UF difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB24-10 | 55/65 | 1111 | Obrig. | Grupo de Devolução do IBS da UF informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB26-10 | 55/65 | 1032 | Obrig. | CST do IBS/CBS informado não permite informação de redução de alíquota Estadual [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB26-20 | 55/65 | 1033 | Obrig. | Não informado o grupo de redução de alíquota Estadual [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB27-10 | 55/65 | 1034 | Obrig. | Percentual de redução de alíquota da UF não é válido para este cClassTrib [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB28-10 | 55/65 | 1035 | Obrig. | Valor da Alíquota Efetiva do IBS da UF calculado incorretamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB35-10 | 55/65 | 1041 | Obrig. | Valor do IBS da UF difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB37-10 | 55/65 | 1036 | Obrig. | Alíquota do IBS do Município inválida [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB40-10 | 55/65 | 1044 | Obrig. | CST do IBS/CBS informado obriga informação de diferimento Municipal [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB40-20 | 55/65 | 1083 | Obrig. | CST do IBS/CBS informado não permite informação de diferimento Municipal [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB42-10 | 55/65 | 1045 | Obrig. | Valor do Diferimento do Município difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB43-10 | 55/65 | 1112 | Obrig. | Grupo de Devolução do IBS do Município informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB45-10 | 55/65 | 1007 | Obrig. | CST do IBS/CBS informado não permite informação de redução de alíquota municipal [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB45-20 | 55/65 | 1074 | Obrig. | Não informado o grupo de redução de alíquota Municipal [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB46-10 | 55/65 | 1046 | Obrig. | Percentual de redução de alíquota do Município não é válido para este cClassTrib [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB47-10 | 55/65 | 1047 | Obrig. | Valor da Alíquota Efetiva do IBS do Município calculado incorretamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB54-10 | 55/65 | 1052 | Obrig. | Valor do IBS Municipal difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB54a-10 | 55/65 | 1150 | Obrig. | Valor do IBS do Item (vIBS) difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB56-10 | 55/65 | 1037 | Obrig. | Alíquota da CBS inválida [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB56-20 | 55/65 | 1037 | Obrig. | Alíquota da CBS inválida [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB59-10 | 55/65 | 1061 | Obrig. | CST do IBS/CBS informado obriga informação de diferimento da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB59-20 | 55/65 | 1090 | Obrig. | CST do IBS/CBS informado não permite informação de diferimento da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB61-10 | 55/65 | 1062 | Obrig. | Valor do Diferimento da CBS difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB62-10 | 65 | 1187 | Obrig. | Grupo de Devolução da CBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB62a-10 | 55 | 1188 | Obrig. | Percentual de devolução da CBS não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB63-10 | 55 | 1189 | Obrig. | Valor da CBS devolvida difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB64-10 | 55/65 | 1028 | Obrig. | CST do IBS/CBS informado não permite informação de redução de alíquota da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB64-20 | 55/65 | 1079 | Obrig. | Não informado o grupo de redução de alíquota da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB65-10 | 55/65 | 1063 | Obrig. | Percentual de redução de alíquota da CBS não é válido para este cClassTrib [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB66-10 | 55/65 | 1064 | Obrig. | Valor da Alíquota Efetiva da CBS calculado incorretamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB66a-10 | 55/65 | 1190 | Obrig. | Inscrição do emitente na Suframa não informado | NT 2025.002 v1.52 | a fazer |
| UB66a-20 | 55/65 | 1191 | Obrig. | Grupo de operações em áreas incentivadas (ALC/ZFM) - CBS (alíquota zero) não permitido [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB66c-10 | 55/65 | 1192 | Obrig. | Número do processo na Suframa não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB66e-10 | 55/65 | 1218 | Obrig. | Valor da CBS para operação fora de áreas ou regimes incentivado difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB67-10 | 55/65 | 1069 | Obrig. | Valor da CBS difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB68-10 | 55/65 | 1065 | Obrig. | Classificação Tributária do IBS e da CBS informada obriga informação da tributação regular [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB68-11 | 55/65 | 1114 | Obrig. | Classificação Tributária do IBS e da CBS informada não permite informação da tributação regular [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB69-10 | 55/65 | 1066 | Obrig. | CST do IBS e da CBS informado no grupo de tributação regular inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB70-10 | 55/65 | 1067 | Obrig. | Classificação Tributária do IBS e da CBS informada no grupo de tributação regular inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB72-10 | 55/65 | 1040 | Obrig. | Valor do Tributo Regular da UF difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB72b-10 | 55/65 | 1051 | Obrig. | Valor do Tributo Regular do Município difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB72d-10 | 55/65 | 1068 | Obrig. | Valor do Tributo Regular da CBS difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB82a-10 | 55 | 1141 | Obrig. | Grupo de informações da composição do valor do IBS e da CBS em compras governamentais não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB82a-20 | 55 | 1142 | Obrig. | Soma dos valores de IBS e CBS em compras governamentais divergente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB82a-30 | 55 | 1144 | Obrig. | Grupo de informações da composição do valor do IBS e da CBS em compras governamentais informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB85a-10 | 55/65 | 1219 | Obrig. | Tributação Monofásica Ad Rem do IBS informada indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB85a-20 | 55/65 | 1220 | Obrig. | Não informada Tributação Monofásica Ad Rem do IBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB86-10 | 55/65 | 1125 | Obrig. | Não informada a Tributação Monofásica Padrão de Combustível do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB86-20 | 55/65 | 1126 | Obrig. | Informada indevidamente a Tributação Monofásica Padrão de Combustível do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB86c-10 | 55/65 | 1221 | Obrig. | Valor do IBS Monofásico Padrão Ad Rem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB87-10 | 55/65 | 1127 | Obrig. | Não informada a Tributação Monofásica de Combustível com Retenção do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB87-20 | 55/65 | 1128 | Obrig. | Informada indevidamente a Tributação Monofásica de Combustível com Retenção do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB87c-10 | 55/65 | 1222 | Obrig. | Valor do IBS Monofásico Ad Rem com Retenção difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB88-10 | 55/65 | 1108 | Obrig. | Não informada a Tributação Monofásica Retida Anteriormente do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB88-20 | 55/65 | 1109 | Obrig. | Informada indevidamente a Tributação Monofásica Retida Anteriormente do IBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB88a-10 | 55/65 | 1223 | Obrig. | Valor do IBS Monofásico Ad Rem retido anteriormente difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB89-10 | 55/65 | 1224 | Obrig. | Não informado o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório do IBS Monofásico Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB89-20 | 55/65 | 1225 | Obrig. | Informado indevidamente o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório do IBS Monofásico Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB89a-10 | 55/65 | 1226 | Obrig. | Quantidade de Biocombustível (EAC) a recolher ou a ressarcir do IBS Monofásico Ad Rem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB89b-10 | 55/65 | 1227 | Obrig. | Valor do IBS Monofásico Ad Rem correspondente a diferença em relação ao pBioObrigatorio difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB90-10 | 55/65 | 1228 | Obrig. | Tributação Monofásica Ad Valorem do IBS informada indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB90-20 | 55/65 | 1229 | Obrig. | Não informada Tributação Monofásica Ad Valorem do IBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB91-10 | 55/65 | 1230 | Obrig. | Não informada a Tributação Monofásica Padrão de Combustível do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB91-20 | 55/65 | 1231 | Obrig. | Informada indevidamente a Tributação Monofásica Padrão de Combustível do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB91c-10 | 55/65 | 1232 | Obrig. | Valor do IBS Monofásico Padrão Ad Valorem Estadual difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB91e-10 | 55/65 | 1233 | Obrig. | Valor do IBS Monofásico Padrão Ad Valorem Municipal difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB91f-10 | 55/65 | 1234 | Obrig. | Valor do IBS Monofásico Padrão Ad Valorem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB92-10 | 55/65 | 1235 | Obrig. | Não informada a Tributação Monofásica de Combustível com Retenção do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB92-20 | 55/65 | 1236 | Obrig. | Informada indevidamente a Tributação Monofásica de Combustível com Retenção do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB92c-10 | 55/65 | 1237 | Obrig. | Valor do IBS Monofásico Ad Valorem com Retenção difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB93-10 | 55/65 | 1238 | Obrig. | Não informada a Tributação Monofásica Retida Anteriormente do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB93-20 | 55/65 | 1239 | Obrig. | Informada indevidamente a Tributação Monofásica Retida Anteriormente do IBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB93a-10 | 55/65 | 1240 | Obrig. | Valor do IBS Monofásico Ad Valorem retido anteriormente difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB94-10 | 55/65 | 1241 | Obrig. | Não informado o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório do IBS Monofásico Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB94-20 | 55/65 | 1242 | Obrig. | Informado indevidamente o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório do IBS Monofásico Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB94a-10 | 55/65 | 1243 | Obrig. | Quantidade de Biocombustível (EAC) a recolher ou a ressarcir do IBS Monofásico Ad Valorem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB94b-10 | 55/65 | 1244 | Obrig. | Valor do IBS Monofásico Ad Valorem correspondente a diferença em relação ao pBioObrigatorio difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB95a-10 | 55/65 | 1245 | Obrig. | Tributação Monofásica Ad Rem da CBS informada indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB95a-20 | 55/65 | 1246 | Obrig. | Não informada Tributação Monofásica Ad Rem da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB96-10 | 55/65 | 1247 | Obrig. | Não informada a Tributação Monofásica Padrão de Combustível da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB96-20 | 55/65 | 1248 | Obrig. | Informada indevidamente a Tributação Monofásica Padrão de Combustível da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB96c-10 | 55/65 | 1249 | Obrig. | Valor da CBS Monofásica Padrão Ad Rem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB97-10 | 55/65 | 1250 | Obrig. | Não informada a Tributação Monofásica de Combustível com Retenção da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB97-20 | 55/65 | 1251 | Obrig. | Informada indevidamente a Tributação Monofásica de Combustível com Retenção da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB97c-10 | 55/65 | 1252 | Obrig. | Valor da CBS Monofásica Ad Rem com Retenção difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB98-10 | 55/65 | 1253 | Obrig. | Não informada a Tributação Monofásica Retida Anteriormente da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB98-20 | 55/65 | 1254 | Obrig. | Informada indevidamente a Tributação Monofásica Retida Anteriormente da CBS Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB98a-10 | 55/65 | 1255 | Obrig. | Valor da CBS Monofásica Ad Rem retido anteriormente difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB99-10 | 55/65 | 1148 | Obrig. | Não informado o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório da CBS Monofásica Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB99-20 | 55/65 | 1149 | Obrig. | Informado indevidamente o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório da CBS Monofásica Ad Rem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB99a-10 | 55/65 | 1256 | Obrig. | Quantidade de Biocombustível (EAC) a recolher ou a ressarcir da CBS Monofásica Ad Rem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB99b-10 | 55/65 | 1257 | Obrig. | Valor da CBS Monofásica Ad Rem correspondente a diferença em relação ao pBioObrigatorio difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB100-10 | 55/65 | 1258 | Obrig. | Tributação Monofásica Ad Valorem da CBS informada indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB100-20 | 55/65 | 1259 | Obrig. | Não informada Tributação Monofásica Ad Valorem da CBS [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB101-10 | 55/65 | 1260 | Obrig. | Não informada a Tributação Monofásica Padrão de Combustível da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB101-20 | 55/65 | 1261 | Obrig. | Informada indevidamente a Tributação Monofásica Padrão de Combustível da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB101c-10 | 55/65 | 1262 | Obrig. | Valor da CBS Monofásica Padrão Ad Valorem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB102-10 | 55/65 | 1263 | Obrig. | Não informada a Tributação Monofásica de Combustível com Retenção da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB102-20 | 55/65 | 1264 | Obrig. | Informada indevidamente a Tributação Monofásica de Combustível com Retenção da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB102c-10 | 55/65 | 1265 | Obrig. | Valor da CBS Monofásica Ad Valorem com Retenção difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB103-10 | 55/65 | 1266 | Obrig. | Não informada a Tributação Monofásica Retida Anteriormente da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB103-20 | 55/65 | 1267 | Obrig. | Informada indevidamente a Tributação Monofásica Retida Anteriormente da CBS Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB103a-10 | 55/65 | 1268 | Obrig. | Valor da CBS Monofásica Ad Valorem retido anteriormente difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB104-10 | 55/65 | 1269 | Obrig. | Não informado o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório da CBS Monofásica Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB104-20 | 55/65 | 1270 | Obrig. | Informado indevidamente o grupo de informações sobre mistura de EAC com gasolina A em percentual inferior ou superior ao obrigatório da CBS Monofásica Ad Valorem [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB104a-10 | 55/65 | 1271 | Obrig. | Quantidade de Biocombustível (EAC) a recolher ou a ressarcir da CBS Monofásica Ad Valorem difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB104b-10 | 55/65 | 1272 | Obrig. | Valor da CBS Monofásica Ad Valorem correspondente a diferença em relação ao pBioObrigatorio difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB105a-10 | 55/65 | 1070 | Obrig. | Valor do IBS monofásico difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB105b-10 | 55/65 | 1071 | Obrig. | Valor da CBS monofásico difere do calculado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB106-30 | 55 | 1133 | Obrig. | Finalidade de Emissão da Nota Fiscal incompatível com o CST [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB106-31 | 55 | 1168 | Obrig. | Tipo de Nota de Débito incompatível com o CST [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB106-40 | 55 | 1129 | Obrig. | Valor do IBS ou da CBS deve ser maior que zero na transferência de crédito. [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB112-10 | 55/65 | 1169 | Obrig. | Grupo de Ajuste de Competência informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB112-20 | 55 | 1170 | Obrig. | Grupo de Ajuste de Competência não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB112-30 | 55 | 1171 | Obrig. | Valor do IBS ou da CBS deve ser maior que zero no ajuste de competência [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB116-10 | 55/65 | 1172 | Obrig. | Grupo de Estorno de Crédito informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB116-20 | 55 | 1173 | Obrig. | Grupo de Estorno de Crédito não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB116-30 | 55 | 1174 | Obrig. | Valor do IBS ou da CBS deve ser maior que zero no estorno de crédito [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB120-10 | 65 | 1049 | Obrig. | Não é permitido o uso de Crédito Presumido na NFC-e modelo 65 [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB120-20 | 55 | 1175 | Obrig. | Grupo de Crédito Presumido na Operação informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB122-10 | 55 | 1055 | Obrig. | Código de Crédito Presumido (cCredPres) da Operação inexistente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB123-10 | 55 | 1053 | Obrig. | Crédito Presumido para o IBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB123-20 | 55 | 1054 | Obrig. | Crédito Presumido para o IBS não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB125-10 | 55 | 1107 | Obrig. | Valor do Crédito Presumido do IBS não pode ser superior a vProd [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB126-10 | 55 | 1056 | Obrig. | Valor do Crédito Presumido em condição suspensiva do IBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB127-10 | 55 | 1050 | Obrig. | Crédito Presumido para a CBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB127-20 | 55 | 1058 | Obrig. | Crédito Presumido para a CBS não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB129-10 | 55 | 1124 | Obrig. | Valor do Crédito Presumido da CBS não pode ser superior a vProd [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB130-10 | 55 | 1060 | Obrig. | Valor do Crédito Presumido da CBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB131-10 | 65 | 1138 | Obrig. | Não é permitido o uso de Crédito Presumido ZFM na NFC-e modelo 65 [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB131-20 | 55 | 1134 | Obrig. | CST do IBS/CBS informado não permite informação do grupo para apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB131-30 | 55 | 1135 | Obrig. | CST do IBS/CBS informado exige a informação do grupo para apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB131-40 | 55 | 1158 | Obrig. | Tipo de Nota de Crédito não permite o grupo para apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB131-50 | 55 | 1159 | Obrig. | Tipo de Nota de Crédito exige o grupo para apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB132-10 | 55 | 1160 | Obrig. | Ano e mês referência do período de apuração superior ao ano e mês atual [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| UB133-10 | 55 | 1136 | Obrig. | Não é possível repetir o Tipo de Classificação para o cálculo do crédito presumido na ZFM neste DFe [nItem: 999] | NT 2025.002 v1.52 | a fazer |

#### Grupo VB: Total do item

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| VB01-05 | 55/65 | 1137 | Obrig. | Valor total do Item (vItem) não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VB01-10 | 55/65 | 1105 | Obrig. | Valor total do Item (vItem) difere do somatório dos valores que o compõem [nItem: 999] | NT 2026.008 v1.00 | a fazer |
| VB01-20 | 55 | 1105 | Obrig. | Valor total do Item (vItem) difere do somatório dos valores que o compõem [nItem: 999] | NT 2026.008 v1.00 | a fazer |

#### Grupo VC: Referenciamento de item de outro documento

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| VC02-04 | 55/65 | 708 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 não pode referenciar documento fiscal | NT 2026.002 v1.11 | a fazer |
| VC02-05 | 55 | 1010 | Obrig. | NF-e com referenciamento de documento a nível de nota e a nível de item [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC02-07 | 55 | 1042 | Obrig. | NF-e com referenciamento a nível de item informado indevidamente | NT 2025.002 v1.52 | a fazer |
| VC02-10 | 55 | 1038 | Obrig. | DFeReferenciado não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC02-14 | 55 | 321 | Obrig. | NF-e de devolução de mercadoria não possui documento fiscal referenciado por item [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC02-20 | 55 | 1072 | Obrig. | DFe/Item referenciado em duplicidade [nItem: 999] | NT 2025.002 v1.52 | SEFAZ |
| VC02-30 | 55 | 1130 | Obrig. | Mais de um documento fiscal referenciado [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC02-40 | 55 | 679 | Obrig. | Chave de Acesso referenciada com Modelo inválido [nItem:nnn] | NT 2026.002 v1.11 | a fazer |
| VC02-40 | 55 | 1193 | Obrig. | Emitente das NFe Referenciadas deve ser o mesmo em todos os itens [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC02-50 | 55 | 708 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 não pode referenciar documento fiscal | NT 2026.002 v1.11 | a fazer |
| VC02-50 | 55 | 1194 | Obrig. | Destintário da NF-e deve ser o igual ao Emitente da NF referenciada [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC03-10 | 55 | 1039 | Obrig. | nItem do DFeReferenciado informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | a fazer |
| VC03-20 | 55 | 1048 | Obrig. | nItem do DFeReferenciado não informado [nItem: 999] | NT 2025.002 v1.52 | a fazer |

#### Grupo W: Total da NF-e

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| W01a-10 | 55/65 | 1282 | Obrig. | Total Líquido não informado | NT 2026.008 v1.00 | a fazer |
| W01a-20 | 55/65 | 1283 | Obrig. | Total Líquido difere do somatório dos itens | NT 2026.008 v1.00 | a fazer |
| W03-10 | 55/65 | 531 | Facul. | Total da BC ICMS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W03-20 | 55/65 | 935 | Facul. | Valor total da Base de Cálculo superior ao valor limite estabelecido [Valor Limite: R$ XXX.XXX,XX] (valor definido pela UF) | NT 2019.001 v1.70 | SEFAZ |
| W04-10 | 55/65 | 532 | Facul. | Total do ICMS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W04-20 | 55/65 | 417 | Facul. | Total do ICMS superior ao valor limite estabelecido | MOC 7.03 | SEFAZ |
| W04a-10 | 55/65 | 795 | Facul. | Total do ICMS desonerado difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W04b-10 | 55/65 | 861 | Obrig. | Total do FCP difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W04c-10 | 55 | 798 | Obrig. | Valor total do ICMS relativo Fundo de Combate à Pobreza (FCP) da UF de destino difere do somatório do valor dos itens | MOC 7.03 | lib (3.2) |
| W04e-10 | 55 | 799 | Obrig. | Valor total do ICMS Interestadual da UF de destino difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W04g-10 | 55 | 800 | Obrig. | Valor total do ICMS Interestadual da UF do remetente difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W05-10 | 55/65 | 533 | Facul. | Total da BC ICMS-ST difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W06-10 | 55/65 | 534 | Facul. | Total do ICMS-ST difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W06-20 | 55/65 | 418 | Facul. | Total do ICMS ST superior ao valor limite estabelecido | MOC 7.03 | SEFAZ |
| W06a-10 | 55 | 862 | Obrig. | Total do FCP ST difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W06b.1-10 | 55 | 700 | Facul. | Total da quantidade tributada do ICMS monofásico próprio difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W06b-10 | 55 | 859 | Obrig. | Total do FCP retido anteriormente por Substituição Tributária difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W06c.1-10 | 55 | 723 | Facul. | Total da quantidade tributada do ICMS monofásico sujeito a retenção difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W06c-10 | 55 | 967 | Facul. | Total do ICMS monofásico próprio difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W06d.1-10 | 55/65 | 744 | Facul. | Total da quantidade tributada do ICMS monofásico retido anteriormente difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W06d-10 | 55 | 968 | Facul. | Total do ICMS monofásico sujeito a retenção difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W06e-10 | 55/65 | 969 | Facul. | Total do ICMS monofásico retido anteriormente difere do somatório dos itens | NT 2023.001 v1.60 | a fazer |
| W07-10 | 55/65 | 564 | Obrig. | Total do Produto / Serviço difere do somatório dos itens | NT 2025.002 v1.52 | lib (3.2) |
| W08-10 | 55/65 | 535 | Facul. | Total do Frete difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W09-10 | 55/65 | 536 | Facul. | Total do Seguro difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W10-10 | 55/65 | 537 | Facul. | Total do Desconto difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W11-10 | 55 | 601 | Facul. | Total do II difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W12-10 | 55 | 538 | Facul. | Total do IPI difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W12a-10 | 55 | 863 | Facul. | Total do IPI devolvido difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W13-10 | 55/65 | 602 | Facul. | Total do PIS difere do somatório dos itens sujeitos ao ICMS | MOC 7.03 | lib (3.2) |
| W14-10 | 55/65 | 603 | Facul. | Total da COFINS difere do somatório dos itens sujeitos ao ICMS | MOC 7.03 | lib (3.2) |
| W15-10 | 55/65 | 604 | Facul. | Total do vOutro difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W16-10 | 55/65 | 610 | Facul. | Total da NF difere do somatório dos Valores que compõe o valor Total da NF | NT 2023.001 v1.60 | lib (3.2) |
| W16-20 | 55 | 628 | Facul. | Total da NF superior ao valor limite estabelecido pela SEFAZ [Limite] | MOC 7.03 | SEFAZ |
| W16-30 | 55/65 | 780 | Obrig. | Total da NFC-e ou NF-e com DANFE Simplificado Tipo 2 superior ao valor limite estabelecido pela SEFAZ [Limite] | NT 2026.002 v1.11 | SEFAZ |
| W16-40 | 55/65 | 750 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com valor total superior ao permitido para destinatário não identificado (Código) [Limite] | NT 2026.002 v1.11 | a fazer |
| W16-50 | 55/65 | 751 | Facul. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com valor total superior ao permitido para destinatário não identificado (Nome) [Limite] | NT 2026.002 v1.11 | a fazer |
| W16-60 | 55/65 | 752 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com valor total superior ao permitido para destinatário não identificado (Endereço) [Limite] | NT 2026.002 v1.11 | a fazer |
| W16a-10 | 55/65 | 685 | Facul. | Total do Valor Aproximado dos Tributos difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W18-10 | 55/65 | 605 | Facul. | Total do vServ difere do somatório do vProd dos itens sujeitos ao ISSQN | MOC 7.03 | lib (3.2) |
| W19-10 | 55/65 | 606 | Facul. | Total do vBC do ISS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W20-10 | 55/65 | 607 | Facul. | Total do ISS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W21-10 | 55/65 | 608 | Facul. | Total do PIS difere do somatório dos itens sujeitos ao ISSQN | MOC 7.03 | lib (3.2) |
| W22-10 | 55/65 | 609 | Facul. | Total da COFINS difere do somatório dos itens sujeitos ao ISSQN | MOC 7.03 | lib (3.2) |
| W22b-10 | 55/65 | 364 | Obrig. | Total do valor da dedução do ISS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W22c-10 | 55/65 | 365 | Obrig. | Total de outras retenções difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W22d-10 | 55/65 | 366 | Obrig. | Total do desconto incondicionado ISS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W22e-10 | 55/65 | 367 | Obrig. | Total do desconto condicionado ISS difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W22f-10 | 55/65 | 368 | Obrig. | Total de ISS retido difere do somatório dos itens | MOC 7.03 | lib (3.2) |
| W31-10 | 55/65 | 1073 | Obrig. | Total de IS informado indevidamente | NT 2025.002 v1.52 | a fazer |
| W31-20 | 55/65 | 1117 | Obrig. | Total de IS não informado | NT 2025.002 v1.52 | a fazer |
| W33-10 | 55/65 | 1075 | Obrig. | Total do Imposto Seletivo difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W34-10 | 55/65 | 1118 | Obrig. | Total de IBS e CBS informado indevidamente | NT 2025.002 v1.52 | a fazer |
| W34-20 | 55/65 | 1119 | Obrig. | Total de IBS e CBS não informado | NT 2025.002 v1.52 | a fazer |
| W35-10 | 55/65 | 1076 | Obrig. | Total da BC do IBS e da CBS difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W38-10 | 55/65 | 1077 | Obrig. | Total de Diferimento do IBS UF difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W39-10 | 55/65 | 1078 | Obrig. | Total Devolvido do IBS UF difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W41-10 | 55/65 | 1080 | Obrig. | Total de IBS UF difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W43-10 | 55/65 | 1081 | Obrig. | Total de Diferimento do IBS Municipal difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W44-10 | 55/65 | 1082 | Obrig. | Total Devolvido do IBS Municipal difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W46-10 | 55/65 | 1084 | Obrig. | Total de IBS Municipal difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W47-10 | 55/65 | 1085 | Obrig. | Total do IBS difere da soma do vIBS dos itens | NT 2025.002 v1.52 | a fazer |
| W48-10 | 55/65 | 1086 | Obrig. | Total de Crédito Presumido do IBS UF difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W53-10 | 55/65 | 1088 | Obrig. | Total de Diferimento da CBS difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W54-10 | 55/65 | 1089 | Obrig. | Total Devolvido da CBS difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W56-10 | 55/65 | 1091 | Obrig. | Total de CBS difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W56a-10 | 55/65 | 1087 | Obrig. | Total de Crédito Presumido da CBS difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W58-10 | 55/65 | 1092 | Obrig. | Total do IBS monofásico difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59-10 | 55/65 | 1093 | Obrig. | Total da CBS monofásica difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59a-10 | 55/65 | 1120 | Obrig. | Total do IBS monofásico sujeito à retenção difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59b-10 | 55/65 | 1121 | Obrig. | Total da CBS monofásica sujeita à retenção difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59c-10 | 55/65 | 1122 | Obrig. | Total do IBS monofásico retido anteriormente difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59d-10 | 55/65 | 1123 | Obrig. | Total da CBS monofásica retida anteriormente difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59f-10 | 55/65 | 1176 | Obrig. | Total do IBS estornado difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W59g-10 | 55/65 | 1177 | Obrig. | Total da CBS estornada difere da soma dos itens | NT 2025.002 v1.52 | a fazer |
| W60-05 | 55/65 | 1004 | Obrig. | Valor total da NF-e com IBS / CBS / IS (vNFTot) não informado | NT 2025.002 v1.52 | a fazer |
| W60-10 | 55/65 | 1094 | Obrig. | Total da NFe difere da soma do total dos itens da Nota | NT 2025.002 v1.52 | a fazer |

#### Grupo X: Transporte da NF-e

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| X02-10 | 55/65 | 753 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 (tag:tpImp=6) com Frete | NT 2026.002 v1.11 | a fazer |
| X02-20 | 55 | 868 | Obrig. | Grupos Veiculo Transporte e Reboque não devem ser informados | MOC 7.03 | a fazer |
| X03-10 | 65 | 754 | Obrig. | NFC-e com dados do Transportador | NT 2023.004 v1.20 | lib (3.3) |
| X03-20 | 65 | 786 | Obrig. | NFC-e de entrega a domicílio sem dados do Transportador | NT 2023.004 v1.20 | lib (3.3) |
| X03-30 | 55 | 845 | Obrig. | O Grupo Transportador não pode ser preenchido para Modalidade do frete informada | NT 2021.004 v1.35 | a fazer |
| X04-10 | 55 | 362 | Facul. | Venda de combustível sem informação do Transportador | NT 2022.002 v1.30a | a fazer |
| X04-20 | 55/65 | 542 | Obrig. | CNPJ do Transportador inválido | MOC 7.03 | lib (3.2) |
| X04-30 | 55 | 846 | Obrig. | Transporte próprio por conta do Remetente e CNPJ Base ou CPF do Transportador difere do CNPJ Base ou CPF do Remetente | NT 2021.004 v1.35 | a fazer |
| X04-40 | 55 | 846 | Obrig. | Transporte próprio por conta do Remetente e CNPJ Base ou CPF do Transportador difere do CNPJ Base ou CPF do Remetente | NT 2021.004 v1.35 | a fazer |
| X04-50 | 55 | 847 | Obrig. | Transporte não é próprio por conta do Remetente e CNPJ Base ou CPF do Transportador igual ao CNPJ Base ou CPF do Remetente | NT 2021.004 v1.35 | a fazer |
| X04-60 | 55 | 847 | Obrig. | Transporte não é próprio por conta do Remetente e CNPJ Base ou CPF do Transportador igual ao CNPJ Base ou CPF do Remetente | NT 2021.004 v1.35 | a fazer |
| X04-70 | 55 | 848 | Obrig. | Transporte próprio por conta do Destinatário e CNPJ Base ou CPF do Transportador difere do CNPJ Base ou CPF do Destinatário | NT 2021.004 v1.35 | a fazer |
| X04-80 | 55 | 848 | Obrig. | Transporte próprio por conta do Destinatário e CNPJ Base ou CPF do Transportador difere do CNPJ Base ou CPF do Destinatário | NT 2021.004 v1.35 | a fazer |
| X04-90 | 55 | 849 | Obrig. | Transporte não é próprio por conta do Destinatário e CNPJ Base ou CPF do Transportador igual ao CNPJ Base ou CPF do Destinatário | NT 2021.004 v1.35 | a fazer |
| X04-100 | 55 | 849 | Obrig. | Transporte não é próprio por conta do Destinatário e CNPJ Base ou CPF do Transportador igual ao CNPJ Base ou CPF do Destinatário | NT 2021.004 v1.35 | a fazer |
| X05-10 | 55/65 | 543 | Obrig. | CPF do Transportador inválido | MOC 7.03 | lib (3.2) |
| X07-10 | 55/65 | 559 | Obrig. | UF do Transportador não informada | MOC 7.03 | a fazer |
| X07-20 | 55/65 | 544 | Obrig. | IE do Transportador inválida | MOC 7.03 | a fazer |
| X11-10 | 55/65 | 755 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados de Retenção do ICMS no Transporte | NT 2026.002 v1.11 | a fazer |
| X16-10 | 55 | 722 | Obrig. | CFOP de Transporte Inexistente | MOC 7.03 | a fazer |
| X17-10 | 55 | 288 | Obrig. | Código Município do Fato Gerador do Transporte inexistente | MOC 7.03 | SEFAZ (tabela) |
| X18-10 | 55/65 | 756 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados do veículo de Transporte | NT 2026.002 v1.11 | a fazer |
| X22-10 | 55/65 | 757 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados de Reboque do veículo de Transporte | NT 2026.002 v1.11 | a fazer |
| X25a-10 | 55/65 | 758 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados do Vagão de Transporte | NT 2026.002 v1.11 | a fazer |
| X25b-10 | 55/65 | 759 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados da Balsa de Transporte | NT 2026.002 v1.11 | a fazer |

#### Grupo Y: Dados de Cobrança

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| Y01-10 | 55/65 | 760 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados de cobrança (Fatura, Duplicata) | NT 2026.002 v1.11 | a fazer |
| Y01-20 | 55 | 905 | Obrig. | Campos do grupo Fatura não informados | MOC 7.03 | a fazer |
| Y05-10 | 55 | 901 | Obrig. | Valor do Desconto da Fatura maior que Valor Original da Fatura | MOC 7.03 | a fazer |
| Y06-10 | 55 | 902 | Obrig. | Valor Liquido da Fatura difere do Valor Original menos o Valor do Desconto | MOC 7.03 | a fazer |
| Y08-10 | 55 | 852 | Obrig. | Número da parcela inválido ou não informado [nOcor: 999] | MOC 7.03 | a fazer |
| Y09-20 | 55 | 900 | Obrig. | Data de vencimento da parcela não informada ou menor que Data de Emissão [nOcor: 999] | MOC 7.03 | lib (3.2) |
| Y09-30 | 55 | 850 | Obrig. | Data de vencimento da parcela não informada ou menor que a Data de vencimento da parcela anterior [nOcor: 999] | MOC 7.03 | lib (3.2) |
| Y09-40 | 55 | 853 | Obrig. | Dados de cobrança não devem ser informados para pagamento à vista | NT 2025.001 v1.03 | a fazer |
| Y09-50 | 55 | 797 | Obrig. | Data de vencimento da parcela superior a 10 anos da data atual [nOcor: 999] | NT 2025.001 v1.03 | a fazer |
| Y10-10 | 55 | 851 | Obrig. | Soma do valor das parcelas difere do Valor Líquido da Fatura | MOC 7.03 | a fazer |

#### Grupo YA: Formas de Pagamento

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| YA02-04 | 55 | 871 | Obrig. | O campo Meio de Pagamento deve ser preenchido com a opção Sem Pagamento | MOC 7.03 | a fazer |
| YA02-10 | 65 | 857 | Obrig. | Informado Duplicata Mercantil como Forma de Pagamento | MOC 7.03 | lib (3.3) |
| YA02-40 | 65 | 899 | Obrig. | Informado incorretamente o campo meio de pagamento | MOC 7.03 | lib (3.3) |
| YA02-50 | 55/65 | 436 | Obrig. | Informado 99-Outros como meio de pagamento | NT 2020.006 v1.31 | a fazer |
| YA02-60 | 55/65 | 436 | Obrig. | Código do meio de pagamento inexistente | NT 2020.006 v1.31 | a fazer |
| YA02a-10 | 55/65 | 441 | Obrig. | Descrição do pagamento obrigatória para meio de pagamento 99- outros | NT 2020.006 v1.31 | a fazer |
| YA02a-20 | 55/65 | 442 | Obrig. | Descrição do pagamento não permitida | NT 2020.006 v1.31 | a fazer |
| YA03-10 | 55/65 | 865 | Facul. (55), Obrig. (65) | Total dos pagamentos menor que o total da nota | NT 2025.001 v1.03 | a fazer |
| YA03-20 | 55/65 | 866 | Facul. (55), Obrig. (65) | Ausência de troco quando o valor dos pagamentos informados for maior que o total da nota | NT 2025.001 v1.03 | a fazer |
| YA03-30 | 55/65 | 904 | Facul. (55), Obrig. (65) | Informado indevidamente campo valor de pagamento | NT 2025.001 v1.03 | a fazer |
| YA03a-10 | 55/65 | 657 | Obrig. | Data de Pagamento inválida [nOcor:999] | NT 2023.004 v1.20 | a fazer |
| YA03c-10 | 55/65 | 961 | Obrig. | CNPJ transacional do pagamento inválido [nOcor:999] | NT 2023.004 v1.20 | lib (3.2) |
| YA04-10 | 55/65 | 391 | Facul. (55), Obrig. (65) | Não informados os dados do cartão de crédito / débito nas Formas de Pagamento da Nota Fiscal | NT 2025.001 v1.03 | a fazer |
| YA04-20 | 55/65 | 796 | Obrig. | CNPJ recebedor do pagamento inválido [nOcor:999] | NT 2023.004 v1.20 | a fazer |
| YA04-20 | 55/65 | 963 | Obrig. | Tipo de pagamento não aceita o grupo de cartões ou boletos. [nOcor:999] | NT 2023.004 v1.20 | a fazer |
| YA04a-20 | 55/65 | 737 | Facul. | Pagamento com cartão de crédito em sistema de automação não integrado | MOC 7.03 | a fazer |
| YA05-10 | 55/65 | 392 | Facul. (55), Obrig. (65) | Não informados os dados da operação de pagamento por cartão de crédito / débito | NT 2025.001 v1.03 | SEFAZ |
| YA05-20 | 55/65 | 437 | Facul. (55), Obrig. (65) | CNPJ da instituição de pagamento inválido | NT 2025.001 v1.03 | a fazer |
| YA06-10 | 55/65 | 443 | Facul. (55), Obrig. (65) | Código da bandeira de cartão de crédito e/ou débito inexistente | NT 2025.001 v1.03 | a fazer |
| YA09-10 | 55/65 | 869 | Obrig. | Valor do troco incorreto | MOC 7.03 | a fazer |
| YA09-20 | 55/65 | 965 | Obrig. | Valor do troco acima do permitido | NT 2023.004 v1.20 | a fazer |

#### Grupo YB: Informações do Intermediador da Transação

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| YB01-10 | 55/65 | 438 | Obrig. | Obrigatória as informações do intermediador da transação para operação por site de terceiros | NT 2020.006 v1.31 | a fazer |
| YB01-20 | 55/65 | 439 | Obrig. | Informações do intermediador da transação para operação por site de terceiros preenchido indevidamente | NT 2020.006 v1.31 | a fazer |
| YB02-10 | 55/65 | 440 | Obrig. | CNPJ do intermediador da transação inválido | NT 2020.006 v1.31 | lib (3.2) |

#### Grupo YC: Vinculação com a transação de pagamento

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| YC03-10 | 55/65 | 215 | Obrig. | Falha no esquema XML | NT 2026.006 v1.00 | a fazer |
| YC04-20 | 55/65 | 215 | Obrig. | Falha no esquema XML | NT 2026.006 v1.00 | a fazer |
| YC05-10 | 55/65 | 1273 | Obrig. | Meio de pagamento inválido [nPag: 999] | NT 2026.006 v1.00 | a fazer |
| YC06-10 | 55/65 | 1274 | Obrig. | CNPJ do recebedor do pagamento inválido [nPag: 999] | NT 2026.006 v1.00 | a fazer |

#### Grupo Z: Informação Adicional da NF-e

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| Z02-10 | 65 | 949 | Facul. | NFC-e sem preenchimento das Informações Adicionais de Interesse do Fisco | NT 2021.004 v1.35 | SEFAZ |
| Z02-20 | 65 | 950 | Facul. | Informações Adicionais de Interesse do Fisco abaixo do tamanho mínimo exigido pela UF | NT 2021.004 v1.35 | SEFAZ |
| Z13-10 | 55/65 | 941 | Facul. | Número do Regime especial inválido. 4.6 | NT 2021.004 v1.35 | SEFAZ |

#### Grupo ZA: Comércio Exterior

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZA01-10 | 55 | 355 | Obrig. | Informar o local de saída do Pais no caso da exportação | MOC 7.03 | a fazer |
| ZA01-20 | 55 | 356 | Obrig. | Informar o local de saída do Pais somente no caso da exportação | MOC 7.03 | a fazer |
| ZA01-30 | 65 | 814 | Obrig. | Nota Fiscal com grupo de comércio exterior | MOC 7.03 | lib (3.3) |

#### Grupo ZB: Informação de Compra

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZB01-10 | 55/65 | 762 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados de compras (Empenho, Pedido, Contrato) | NT 2026.002 v1.11 | a fazer |
| ZB02-10 | 55 | 359 | Facul. | NF-e de vendaa Órgão Público sem informar a Nota de Empenho | MOC 7.03 | a fazer |
| ZB02-20 | 55 | 360 | Facul. | NF-e com Nota de Empenho inválida para a UF | MOC 7.03 | a fazer |
| ZB02-30 | 55 | 361 | Facul. | NF-e com Nota de Empenho inexistente na UF | MOC 7.03 | a fazer |

#### Grupo ZC: Informações do Registro de Aquisição de Cana

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZC01-10 | 55/65 | 763 | Obrig. | NFC-e ou NF-e com DANFE Simplificado Tipo 2 com dados de aquisição de Cana | NT 2026.002 v1.11 | a fazer |

#### Grupo ZD: Informações do Responsável Técnico

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZD01-10 | 55/65 | 972 | Facul. | Obrigatória as informações do responsável técnico | NT 2021.002 v1.12 | a fazer |
| ZD02-10 | 55/65 | 973 | Facul. | CNPJ do responsável técnico inválido | NT 2021.002 v1.12 | lib (3.2) |
| ZD07-10 | 55/65 | 975 | Facul. | Obrigatória a informação do identificador do CSRT e do Hash do CSRT | NT 2021.002 v1.12 | a fazer |

#### Grupo ZE: Nota Fiscal Fácil (NFF)

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZE01-10 | 55/65 | 819 | Obrig. | Informação de Solicitação de NFF não pode estar preenchido | NT 2021.002 v1.12 | a fazer |
| ZE01-20 | 55/65 | 834 | Obrig. | Informação de Solicitação de NFF não preenchida Grupo | NT 2021.002 v1.12 | a fazer |

#### Grupo ZF: Produtos da agricultura, pecuária e produção florestal

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZF02-10 | 55 | 835 | Facul. | Em algum item da NF-e foi informado produto agrotóxico e não informado número da receita do defensivo agrícola. [nItem: 999] | NT 2024.003 v1.10 | a fazer |
| ZF02-20 | 55 | 309 | Facul. | Nenhum item da NF-e é defensivo agrícola e informada receita de agrotóxico | NT 2024.003 v1.10 | a fazer |
| ZF03a-10 | 55 | 308 | Obrig. | CPF inválido para Responsável Técnico do de agrotóxico. receituário de agrotóxico | NT 2024.003 v1.10 | a fazer |
| ZF05-10 | 55 | 836 | Facul. | Não informada Guia de Trânsito Animal [nItem: 999] | NT 2024.003 v1.10 | SEFAZ (tabela) |
| ZF05-14 | 55 | 310 | Facul. | Informação indevida de Guia de Trânsito Animal | NT 2024.003 v1.10 | a fazer |
| ZF05-20 | 55 | 311 | Facul. | Não Informada Guia de Trânsito Vegetal [nItem: 999] | NT 2024.003 v1.10 | SEFAZ (tabela) |
| ZF05-24 | 55 | 312 | Facul. | Informação Indevida de Guia de Trânsito Vegetal | NT 2024.003 v1.10 | a fazer |
| ZF05-30 | 55 | 839 | Facul. | Madeira sem documento de origem [nItem: documento de origem florestal. (tpGuia=07) ​ | NT 2024.003 v1.10 | a fazer |

#### Grupo ZG: Provedor de Assinatura e Autorização (PAA)

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZG01-10 | 55/65 | 1179 | Obrig. | Ambiente de autorização inválido para emissão pelo PAA | NT 2026.001 v1.02b | SEFAZ |
| ZG02-10 | 55/65 | 1180 | Obrig. | CNPJ do PAA inválido | NT 2026.001 v1.02b | SEFAZ |
| ZG02-20 | 55/65 | 1181 | Obrig. | Provedor de Assinatura e Autorização não existe na base da SEFAZ | NT 2026.001 v1.02b | SEFAZ |
| ZG02-30 | 55/65 | 1182 | Obrig. | Emitente não associado ao PAA | NT 2026.001 v1.02b | SEFAZ |
| ZG02-34 | 55/65 | 872 | Obrig. | Série da NF difere da estipulada para este Emitente no PAA | NT 2026.001 v1.02b | SEFAZ |
| ZG02-36 | 55/65 | 895 | Obrig. | Chave do Emitente para o PAA inválida | NT 2026.001 v1.02b | SEFAZ |
| ZG02-40 | 55/65 | 1183 | Obrig. | Emissão por PAA deve ser assinada pelo CNPJ do Provedor de Assinatura | NT 2026.001 v1.02b | SEFAZ |
| ZG04-10 | 55/65 | 1184 | Obrig. | Emissão por PAA com Assinatura RSA inválida | NT 2026.001 v1.02b | SEFAZ |

#### Grupo ZX: Informações Suplementares da Nota Fiscal

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| ZX01-10 | 55 | 393 | Obrig. | NF-e com o grupo de Informações Suplementares | MOC 7.03 | a fazer |
| ZX02-10 | 55/65 | 394 | Obrig. | Nota Fiscal sem a informação do QR-Code | NT 2026.002 v1.11 | a fazer |
| ZX02-15 | 65 | 903 | Obrig. | Versão informada no QR-Code (“100”) não é mais válida para a data de emissão | MOC 7.03 | a fazer (5.1) |
| ZX02-20 | 55/65 | 395 | Obrig. | Endereço do site da UF da Consulta via QR- Code diverge do previsto | NT 2026.002 v1.11 | a fazer |
| ZX02-22 | 65 | 813 | Obrig. | QR-Code com sequência de escape para o e-comercial. Usar CDATA | MOC 7.03 | a fazer (5.1) |
| ZX02-24 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (chAcesso) | MOC 7.03 | a fazer (5.1) |
| ZX02-28 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (chAcesso) | MOC 7.03 | a fazer (5.1) |
| ZX02-32 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (nVersao) | MOC 7.03 | a fazer (5.1) |
| ZX02-40 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (tpAmp) | MOC 7.03 | a fazer (5.1) |
| ZX02-44 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (tpAmb) | MOC 7.03 | a fazer (5.1) |
| ZX02-48 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (cDest) | MOC 7.03 | a fazer (5.1) |
| ZX02-52 | 65 | 399 | Obrig. | Parâmetro de Identificação do destinatário no QR-Code para Nota Fiscal sem identificação do destinatário | MOC 7.03 | a fazer (5.1) |
| ZX02-56 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (cDest) | MOC 7.03 | a fazer (5.1) |
| ZX02-60 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (dhEmi) | MOC 7.03 | a fazer (5.1) |
| ZX02-64 | 65 | 400 | Obrig. | Parâmetro do QR-Code não está no formato hexadecimal (dhEmi) | MOC 7.03 | a fazer (5.1) |
| ZX02-68 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (dhEmi) | MOC 7.03 | a fazer (5.1) |
| ZX02-72 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (vNF) | MOC 7.03 | a fazer (5.1) |
| ZX02-76 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (vNF) | MOC 7.03 | a fazer (5.1) |
| ZX02-80 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (vICMS) | MOC 7.03 | a fazer (5.1) |
| ZX02-84 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (vICMS) | MOC 7.03 | a fazer (5.1) |
| ZX02-88 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (digVal) | MOC 7.03 | a fazer (5.1) |
| ZX02-92 | 65 | 400 | Obrig. | Parâmetro do QR-Code não está no formato hexadecimal (digVal) | MOC 7.03 | a fazer (5.1) |
| ZX02-96 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (digVal) | MOC 7.03 | a fazer (5.1) |
| ZX02-100 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (cIdToken) | MOC 7.03 | a fazer (5.1) |
| ZX02-104 | 65 | 462 | Obrig. | Código Identificador do CSC no QR-Code não cadastrado na SEFAZ | MOC 7.03 | SEFAZ |
| ZX02-108 | 65 | 463 | Obrig. | Código Identificador do CSC no QR-Code foi revogado pela empresa | MOC 7.03 | a fazer (5.1) |
| ZX02-112 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente (cHashQRCode) | MOC 7.03 | a fazer (5.1) |
| ZX02-116 | 65 | 400 | Obrig. | Parâmetro do QR-Code não está no formato hexadecimal (cHashQRCode) | MOC 7.03 | a fazer (5.1) |
| ZX02-120 | 65 | 464 | Obrig. | Código de Hash no QR-Code difere do calculado | MOC 7.03 | a fazer (5.1) |
| ZX02-220 | 65 | 407 | Obrig. | NFC-e com qrCode na versão 3 | NT 2025.001 v1.03 | a fazer (5.1) |
| ZX02-220 | 55 | 672 | Obrig. | NF-e com DANFE Simplificado Tipo 2 com versão de qrCode não permitida | NT 2026.002 v1.11 | a fazer |
| ZX02-222 | 65 | 444 | Obrig. | NFC-e com qrCode na versão 2 para Pessoa Física | NT 2025.001 v1.03 | a fazer (5.1) |
| ZX02-224 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx)] | NT 2026.002 v1.11 | a fazer |
| ZX02-228 | 55/65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal [Param: xxx)] | NT 2026.002 v1.11 | a fazer |
| ZX02-232 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx)] | NT 2026.002 v1.11 | a fazer |
| ZX02-236 | 55/65 | 398 | Obrig. | Parâmetro Versão informada no QR-Code diverge do previsto [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-240 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-244 | 55/65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-260 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-268 | 55/65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-272 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-276 | 55/65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-288 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente:[Param: xxx) | MOC 7.03 | a fazer (5.1) |
| ZX02-292 | 65 | 400 | Obrig. | Parâmetro Digest Value no QR-Code não está no formato hexadecimal (Caracteres: “0-9”, “a-f”,“A-F”) | MOC 7.03 | a fazer (5.1) |
| ZX02-296 | 65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal: [Param: xxx)] | MOC 7.03 | a fazer (5.1) |
| ZX02-300 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2025.001 v1.03 | a fazer (5.1) |
| ZX02-304 | 65 | 462 | Obrig. | Parâmetro Código Identificador do CSC no QR-Code não cadastrado na SEFAZ | MOC 7.03 | SEFAZ |
| ZX02-308 | 65 | 463 | Obrig. | Parâmetro Código Identificador do CSC no QR-Code foi revogado pela empresa anteriormente a Data de Emissão | MOC 7.03 | a fazer (5.1) |
| ZX02-312 | 65 | 396 | Obrig. | Parâmetro do QR-Code inexistente:[Param: xxx) | MOC 7.03 | a fazer (5.1) |
| ZX02-316 | 65 | 400 | Obrig. | Parâmetro Hash no QR-Code não está no formato hexadecimal (Caracteres: “0-9”, “a-f”,“A-F”) | MOC 7.03 | a fazer (5.1) |
| ZX02-320 | 65 | 464 | Obrig. | Parâmetro Hash no QR-Code diverge do calculado | MOC 7.03 | a fazer (5.1) |
| ZX02-324 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-326 | 55/65 | 396 | Obrig. | Parâmetro do QR-Code inexistente: [Param: xxx] | NT 2026.002 v1.11 | a fazer |
| ZX02-328 | 55/65 | 397 | Obrig. | Parâmetro do QR-Code divergente da Nota Fiscal (idDest) | NT 2026.002 v1.11 | a fazer |
| ZX02-330 | 55/65 | 445 | Obrig. | Parâmetro assinatura não deve ser informado no qrCode | NT 2026.002 v1.11 | a fazer |
| ZX02-334 | 55/65 | 474 | Obrig. | Parâmetro assinatura deve ser informado no qrCode | NT 2026.002 v1.11 | a fazer |
| ZX02-338 | 55/65 | 583 | Obrig. | Valor da assinatura do qrCode difere do valor calculado | NT 2026.002 v1.11 | a fazer |
| ZX03-20 | 65 | 878 | Facul. | Endereço do site da UFda Consulta por chave de acesso diverge do previsto | MOC 7.03 | a fazer (5.1) |

### 10.5. Banco de dados do autorizador (Anexo I, 4.2.1, e as NTs)

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| 1C03-10 | 55/65 | 935 | Facul. | Razão Social do emitente diverge do informado no cadastro da SEFAZ | MOC 7.03 | SEFAZ |
| 1C03-10 | 55/65 | 936 | Facul. | Razão Social do emitente diverge do informado no cadastro da SEFAZ | NT 2019.001 v1.70 | SEFAZ |
| 1C17-02 | 55 | 163 | Obrig. | Emitente possui inscrição estadual ativa no cadastro da UF | NT 2026.007 v1.10 | SEFAZ |
| 1C17-04 | 55 | 164 | Obrig. | Emitente com situação irregular na UF | NT 2026.007 v1.10 | SEFAZ |
| 1C17-10 | 55/65 | 230 | Obrig. | IE do emitente não cadastrada | NT 2021.002 v1.12 | SEFAZ |
| 1C17-20 | 55/65 | 231 | Obrig. | IE do emitente não vinculada ao CNPJ | NT 2021.002 v1.12 | SEFAZ |
| 1C17-30 | 55/65 | 622 | Obrig. | IE emitente não vinculada ao CPF | NT 2021.002 v1.12 | SEFAZ |
| 1C17-34 | 55 | 203 | Obrig. | Emissor não habilitado para emissão da NF-e | NT 2021.002 v1.12 | SEFAZ |
| 1C17-38 | 65 | 781 | Obrig. | Emissor não habilitado para emissão da NFC-e | NT 2023.002 v1.01 | SEFAZ |
| 1C17-40 | 55/65 | 301 | Obrig. | Irregularidade fiscal do emitente | NT 2023.002 v1.01 | SEFAZ |
| 1C17-50 | 55 | 230 | Obrig. | IE do emitente não cadastrada | MOC 7.03 | excluída |
| 1C17-50 | 55 | 307 | Obrig. | Uso Denegado Rejeição: Emitente bloqueado pela UF de destino, em operação com consumidor final | NT 2024.001 v1.20 | SEFAZ |
| 1C17-60 | 55/65 | 245 | Facul. | CNPJ Emitente não cadastrado | NT 2021.002 v1.12 | SEFAZ |
| 1C17-70 | 55 | 621 | Facul. | CPF Emitente não cadastrado | NT 2021.002 v1.12 | SEFAZ |
| 1P10-30 | 55/65 | 187 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Autor de Evento não cadastrado na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 1P10-32 | 55/65 | 170 | Obrig. | CNPJ do Autor de Evento com situação irregular na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 1P10-40 | 55 | 188 | Obrig. | Evento de NF-e de contribuinte exclusivo do IBS/CBS deve ser autorizado na SVRS | NT 2026.007 v1.10 | SEFAZ |
| 2AB08-10 | 55 | 692 | Obrig. | Existe EPEC registrado para esta Série e Número[Chave EPEC: xxxxxxxxxxx] | MOC 7.03 | SEFAZ |
| 2AB08-20 | 55 | 691 | Obrig. | Chave de Acesso da NF-e diverge da Chave de Acesso do EPEC [Chave EPEC: xxxxxxxxx] | MOC 7.03 | SEFAZ |
| 2AB08-30 | 55 | 467 | Obrig. | Dados da NF-e divergentesdo EPEC [tag: xxxx] | MOC 7.03 | SEFAZ |
| 2AB08-40 | 55 | 468 | Obrig. | NF-e com Tipo Emissão = 4, sem EPEC correspondente | MOC 7.03 | SEFAZ |
| 2B08-10 | 55/65 | 539 | Facul. | Duplicidade de NF-e com diferença na Chave de Acesso [chNFe: 99999999999999999999999999999999999999999999][nRec:9999999999999 99] | MOC 7.03 | SEFAZ |
| 2B08-20 | 55/65 | 204 | Obrig. | Duplicidade de NF-e [nRec:999999999999999] | MOC 7.03 | SEFAZ |
| 2B08-30 | 55/65 | 218 | Obrig. | NF-e já está cancelada na base de dados da SEFAZ [nRec:999999999999999] | MOC 7.03 | SEFAZ |
| 2B08-40 | 55/65 | 205 | Obrig. | NF-e está denegada na base de dados da SEFAZ [nRec:999999999999999] | MOC 7.03 | SEFAZ |
| 2B08-50 | 55/65 | 635 | Facul. | NF-e com mesmo número e série já transmitida e aguardando processamento | MOC 7.03 | SEFAZ |
| 3BA02-10 | 55 | 267 | Obrig. | Chave de Acesso referenciada inexistente [nRef: xxx] | NT 2025.002 v1.52 | SEFAZ |
| 3BA02-20 | 55 | 268 | Facul. | NF Complementar referencia uma outra NF-e Complementar | MOC 7.03 | SEFAZ |
| 3BA02-30 | 55 | 686 | Facul. | NF Complementar referencia uma NF-e cancelada | MOC 7.03 | SEFAZ |
| 3BA02-40 | 55 | 687 | Facul. | NF Complementar referencia uma NF-e denegada | MOC 7.03 | SEFAZ |
| 3BA02-50 | 55 | 545 | Obrig. | NF-e de devolução com valor total superior a NF-e devolvida | NT 2022.005 v1.11 | SEFAZ |
| 3BA02-54 | 55 | 546 | Obrig. | NF-e de devolução com valor do ICMS superior a NF-e devolvida Sistema | NT 2022.005 v1.11 | SEFAZ |
| 3BA02-58 | 55 | 566 | Obrig. | NF-e de devolução com valor do FCP superior a NF-e devolvida | NT 2022.005 v1.11 | SEFAZ |
| 3BA02-64 | 55 | 567 | Obrig. | NF-e de devolução com valor do ICMS da UF Destino superior a NF-e devolvida Sistema Autorização. Nesses casos, a SEFAZ Autorizadora poderá não aplicar essa | NT 2022.005 v1.11 | SEFAZ |
| 3BA02-68 | 55 | 581 | Obrig. | NF-e de devolução com valor do FCP da UF Destino superior a NF-e devolvida | NT 2022.005 v1.11 | SEFAZ |
| 3BA02-70 | 55 | 1095 | Obrig. | Chave de acesso referenciada deve existir e não estar cancelada [nRef: xxx] | NT 2025.002 v1.52 | SEFAZ |
| 3BA02a-10 | 55 | 267 | Facul. | Chave de Acesso referenciada inexistente [nRef: xxx] | NT 2022.003 v1.11 | SEFAZ |
| 3BA15-10 | 55 | 688 | Facul. | NF referenciada de Produtor com IE inexistente [nRef:xxx] | MOC 7.03 | SEFAZ |
| 3BA15-20 | 55 | 689 | Facul. | NF referenciada de Produtor com IE não vinculada ao CNPJ/CPF informado [nRef:xxx] | MOC 7.03 | SEFAZ |
| 4I54-10 | 55 | 357 | Facul. | Chave de Acesso do grupo de Exportação Indireta inexistente [nRef: xxx] | MOC 7.03 | SEFAZ |
| 4I54-20 | 55 | 358 | Facul. | Chave de Acesso do grupo de Exportação Indireta cancelada ou denegada[nRef:xxx] | MOC 7.03 | SEFAZ |
| 5AF15-10 | 55 | 942 | Obrig. | IE do local de retirada não cadastrada | NT 2026.007 v1.10 | SEFAZ |
| 5AF15-11 | 55 | 165 | Obrig. | IE do Local de Retirada com situação irregular na UF | NT 2026.007 v1.10 | SEFAZ |
| 5AF15-12 | 55 | 167 | Obrig. | IE do local de retirada não está ativa na UF | NT 2026.007 v1.10 | SEFAZ |
| 5AF15-20 | 55 | 943 | Obrig. | IE do local de retirada não vinculada ao CNPJ | NT 2021.004 v1.35 | SEFAZ |
| 5AF15-30 | 55 | 944 | Obrig. | IE do local de retirada não vinculada ao CPF | NT 2021.004 v1.35 | SEFAZ |
| 5AF17-10 | 55 | 168 | Obrig. | IE do local de retirada não informada | NT 2026.007 v1.10 | SEFAZ |
| 5AF17-20 | 55 | 169 | Obrig. | CNPJ do Local de Retirada com situação irregular na UF | NT 2026.007 v1.10 | SEFAZ |
| 5BG15-10 | 55 | 171 | Obrig. | IE do local de entrega não cadastrada | NT 2026.007 v1.10 | SEFAZ |
| 5BG15-10 | 55 | 945 | Obrig. | IE do local de entrega não cadastrada | NT 2021.004 v1.35 | SEFAZ |
| 5BG15-11 | 55 | 173 | Obrig. | IE do Local de Entrega com situação irregular na UF | NT 2026.007 v1.10 | SEFAZ |
| 5BG15-12 | 55 | 175 | Obrig. | IE do local de Entrega não está ativa na UF | NT 2026.007 v1.10 | SEFAZ |
| 5BG15-20 | 55 | 947 | Obrig. | IE do local de entrega não vinculada ao CNPJ | NT 2021.004 v1.35 | SEFAZ |
| 5BG15-30 | 55 | 948 | Obrig. | IE do local de entrega não vinculada ao CPF | NT 2021.004 v1.35 | SEFAZ |
| 5BG17-10 | 55 | 176 | Obrig. | IE do local de Entrega não informada | NT 2026.007 v1.10 | SEFAZ |
| 5BG17-20 | 55 | 177 | Obrig. | CNPJ do Local de Entrega com situação irregular na UF 5.10Banco de Dados: Validação Cadastro LCC-RFB | NT 2026.007 v1.10 | SEFAZ |
| 5E17-10 | 55 | 233 | Obrig. | IE do destinatário não cadastrada | NT 2019.001 v1.70 | SEFAZ |
| 5E17-12 | 55 | 300 | Obrig. | Tipo da IE do Destinatário difere de Não Contribuinte no cadastro da UF | NT 2025.001 v1.03 | SEFAZ |
| 5E17-20 | 55 | 234 | Obrig. | IE do destinatário não vinculada ao CNPJ | NT 2019.001 v1.70 | SEFAZ |
| 5E17-30 | 55 | 624 | Obrig. | IE Destinatário não vinculada ao CPF | NT 2019.001 v1.70 | SEFAZ |
| 5E17-40 | 55 | 302 | Obrig. | Irregularidade fiscal do destinatário | NT 2019.001 v1.70 | SEFAZ |
| 5E17-43 | 55 | 305 | Obrig. | Destinatário bloqueado na UF | NT 2019.001 v1.70 | SEFAZ |
| 5E17-46 | 55 | 306 | Obrig. | IE do destinatário não está ativa na UF | NT 2019.001 v1.70 | SEFAZ |
| 5E17-50 | 55 | 232 | Obrig. | IE do destinatário não informada | NT 2019.001 v1.70 | SEFAZ |
| 5E17-60 | 55 | 303 | Obrig. | Destinatário não habilitado a operar na UF | NT 2019.001 v1.70 | SEFAZ |
| 5E17-63 | 55 | 305 | Obrig. | Destinatário bloqueado na UF | NT 2019.001 v1.70 | SEFAZ |
| 5E17-65 | 65 | 172 | Obrig. | Alerta Alerta: Situação do CNPJ destinatário inabilitado no momento da autorização | NT 2026.002 v1.11 | SEFAZ |
| 5E17-70 | 55 | 246 | Obrig. | CNPJ Destinatário não cadastrado | NT 2026.007 v1.10 | excluída |
| 5E17-80 | 55 | 623 | Facul. | CPF Destinatário não cadastrado | NT 2019.001 v1.70 | SEFAZ |
| 7B09-10 | 55/65 | 479 | Facul. | Data de Emissão anterior a data de credenciamento ou anterior a Data de Abertura do estabelecimento | MOC 7.03 | SEFAZ |
| 7C10-10 | 55/65 | 480 | Facul. | Código Município do Emitente diverge do cadastrado na UF | MOC 7.03 | SEFAZ |
| 7C21-10 | 55/65 | 481 | Facul. | Código Regime Tributário do emitente diverge do cadastro na SEFAZ | NT 2024.001 v1.20 | SEFAZ |
| 7E10-10 | 55/65 | 482 | Facul. | Código do Município do Destinatário diverge do cadastrado na UF | MOC 7.03 | SEFAZ |
| 7GA01-10 | 55 | 486 | Facul. | Não informado o Grupo de Autorização para UF que exige a identificação do Escritório de Contabilidade na Nota Fiscal | MOC 7.03 | SEFAZ |
| 7GA01-20 | 55 | 487 | Facul. | Escritório de Contabilidade não cadastrado na SEFAZ | MOC 7.03 | SEFAZ |
| 7I03-10 | 55/65 | 889 | Obrig. | Obrigatória a informação do GTIN para o produto [nItem: 999] | NT 2021.003 v1.50 | SEFAZ |
| 7ZD02-10 | 55/65 | 974 | Facul. | CNPJ do responsável técnico diverge do cadastrado | MOC 7.03 | SEFAZ |
| 7ZD08-10 | 55/65 | 976 | Facul. | Identificador do CSRT não cadastrado na SEFAZ | MOC 7.03 | SEFAZ |
| 7ZD08-20 | 55/65 | 977 | Facul. | Identificador do CSRT revogado | MOC 7.03 | SEFAZ |
| 7ZD09-10 | 55/65 | 978 | Facul. | Hash do CSRT diverge do calculado | MOC 7.03 | SEFAZ |
| 8C02-10 | 55 | 488 | Facul. | Vendas do Emitente incompatíveis com o Porte da Empresa | MOC 7.03 | SEFAZ |
| 9I03-10 | 55/65 | 890 | Obrig. | GTIN inexistente no Cadastro Centralizado de GTIN (CCG) [nItem:999] | NT 2021.003 v1.50 | SEFAZ |
| 9I03-20 | 55/65 | 891 | Obrig. | GTIN incompatível com a NCM [nItem:999; NCM esperada: 99999999] | NT 2021.003 v1.50 | SEFAZ |
| 9I03-30 | 55/65 | 892 | Obrig. | GTIN incompatível com CEST [nItem:999; CEST esperado: 9999999] | NT 2021.003 v1.50 | SEFAZ |
| 9I03-40 | 55/65 | 893 | Obrig. | GTIN da unidade tributável diverge do GTIN Contido cadastrado no CCG [nItem:999; GTIN Contido esperado: 99999999999999] | NT 2021.003 v1.50 | SEFAZ |
| 9I12-10 | 55/65 | 894 | Obrig. | GTIN da unidade tributável inexistente no Cadastro Centralizado de GTIN (CCG) [nItem:999] | NT 2021.003 v1.50 | SEFAZ |
| 9I12-20 | 55/65 | 895 | Obrig. | GTIN da unidade tributável incompatível com a NCM [nItem:999; NCM esperada: 99999999] | NT 2021.003 v1.50 | SEFAZ |
| 9I12-30 | 55/65 | 896 | Obrig. | GTIN da unidade tributável incompatível com CEST [nItem:999; CEST esperado: 9999999] | NT 2021.003 v1.50 | SEFAZ |
| 10ZF04-10 | 55 | 837 | Facul. | Guia de trânsito inválida | NT 2024.003 v1.10 | SEFAZ |
| 10ZF04-20 | 55 | 838 | Facul. | Guia de trânsito já utilizada | NT 2024.003 v1.10 | SEFAZ |
| 11BC01-10 | 65 | 1146 | Obrig. | NF-e referenciada de pagamento antecipado informada indevidamente | NT 2025.002 v1.52 | SEFAZ |
| 11BC01-20 | 55 | 1147 | Obrig. | NF-e referenciada de pagamento antecipado inexistente | NT 2025.002 v1.52 | SEFAZ |
| 11BC01-30 | 55 | 1143 | Obrig. | NF-e referenciada de pagamento antecipado deve ser do tipo débito, pagamento antecipado | NT 2025.002 v1.52 | SEFAZ |
| 12C02-10 | 55/65 | 178 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do emitente não cadastrado na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12C02-20 | 55/65 | 179 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do emitente com situação irregular na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12C21-20 | 55/65 | 180 | Obrig. | Código Regime Tributário do emitente diverge do cadastro na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12E02-10 | 55/65 | 181 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do destinatário não cadastrado na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12E02-20 | 55/65 | 182 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Destinatário com situação irregular na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12F02-10 | 55 | 183 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Local de Retirada não cadastrado na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12F02-20 | 55 | 184 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Local de Retirada com situação irregular na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12G02-10 | 55 | 185 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Local de Entrega não cadastrado na Receita Federal | NT 2026.007 v1.10 | SEFAZ |
| 12G02-20 | 55 | 186 | Obrig. | CNPJ [XX.XXX.XXX/XXXX-DV] do Local de Entrega com situação irregular na Receita Federal | NT 2026.007 v1.10 | SEFAZ |

### 10.6. Regras de evento nas NTs da autorização (fase 6)

As NTs 2019.001 (EPEC), 2025.002 (os eventos da RTC), 2026.001 (o cancelamento pelo PAA) e 2026.007 trazem regras de evento junto com as da nota. Ficam aqui, com a fonte, para a fase 6.

| Regra | Modelo | Código | Aplic. | Mensagem | Fonte | Situação |
|---|---|---|---|---|---|---|
| 1P06-10 | 55/65 | 459 | Obrig. | Cancelamento de Evento inexistente | NT 2025.002 v1.52 | outra operação (6) |
| 1P10-10 | 55/65 | 203 | Obrig. | Emissor não habilitado para emissão de NF-e 1P10-20  Verificar situação fiscal do emitente | NT 2021.002 v1.12 | outra operação (6) |
| 1P10-10 | 55/65 | 240 | Obrig. | Irregularidade fiscal do emitente 2P12-14  Se tpEvento=110111 (Cancelamento Normal): verificar se NF-e autorizada há mais de 1 dia (24 horas) | NT 2021.002 v1.12 | outra operação (6) |
| 1P10-10 | 55/65 | 501 | Obrig. | Prazo de cancelamento superior ao previsto na Legislação | NT 2021.002 v1.12 | outra operação (6) |
| 1P10-10 | 55/65 | 1113 | Obrig. | Autor do Evento de Cancelamento diverge do Autor do Evento a ser cancelado | NT 2025.002 v1.52 | outra operação (6) |
| 1P20-10 | 55/65 | 1178 | Obrig. | Órgão do Autor do Evento de Cancelamento diverge do órgão do Autor do Evento a ser cancelado | NT 2025.002 v1.52 | outra operação (6) |
| 1P24-10 | 55/65 | 460 | Obrig. | Protocolo do Evento difere do cadastrado | NT 2025.002 v1.52 | outra operação (6) |
| 2P12-10 | 55/65 | 1049 | Obrig. | Não é permitido o uso de Crédito Presumido na NFC-e modelo 65 | NT 2025.002 v1.52 | outra operação (6) |
| 2P21-05 | 55/65 | 574 | Obrig. | Autor do evento diverge do emitente da NF-e | NT 2025.002 v1.52 | outra operação (6) |
| 2P21-10 | 55/65 | 575 | Obrig. | Autor do evento diverge do destinatário da NF-e | NT 2025.002 v1.52 | outra operação (6) |
| 2P24-10 | 55/65 | 1096 | Obrig. | Número de item não existe na NFe | NT 2025.002 v1.52 | outra operação (6) |
| 2P25-10 | 55/65 | 1097 | Obrig. | O valor do crédito de IBS do item não pode ser maior que o valor do IBS do respectivo item na NFe | NT 2025.002 v1.52 | outra operação (6) |
| 2P25a-10 | 55/65 | 1055 | Obrig. | Código de Crédito Presumido (cCredPres) da Operação inexistente [nItem: 999] | NT 2025.002 v1.52 | outra operação (6) |
| 2P26-10 | 55/65 | 1053 | Obrig. | Crédito Presumido para o IBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | outra operação (6) |
| 2P26-10 | 55/65 | 1098 | Obrig. | O valor do crédito de CBS do item não pode ser maior que o valor da CBS do respectivo item na NFe | NT 2025.002 v1.52 | outra operação (6) |
| 2P26-20 | 55/65 | 1054 | Obrig. | Crédito Presumido para o IBS não informado [nItem: 999] | NT 2025.002 v1.52 | outra operação (6) |
| 2P28-10 | 55/65 | 1101 | Obrig. | A quantidade do item a ser imobilizado consumido não pode ser maior que a quantidade do respectivo item na NFe (qCom) | NT 2025.002 v1.52 | outra operação (6) |
| 2P29-10 | 55/65 | 1100 | Obrig. | A quantidade de consumo imobilizada não pode ser maior que a quantidade do respectivo item na NFe (qCom) | NT 2025.002 v1.52 | outra operação (6) |
| 2P30-10 | 55/65 | 1050 | Obrig. | Crédito Presumido para a CBS informado indevidamente [nItem: 999] | NT 2025.002 v1.52 | outra operação (6) |
| 2P30-20 | 55/65 | 1058 | Obrig. | Crédito Presumido para a CBS não informado [nItem: 999] | NT 2025.002 v1.52 | outra operação (6) |
| 2P81-10 | 55 | 889 | Obrig. | Emissão por PAA deve ser assinada pelo CNPJ do Provedor de Assinatura | NT 2026.001 v1.02b | outra operação (6) |
| 3B08-100 | 55/65 | 206 | Obrig. | Numeração da NF-e já está inutilizada na Base de Dados da SEFAZ | NT 2020.005 v1.21 | outra operação (6) |
| 6P31-10 | 55 | 233 | Obrig. | IE do destinatário não cadastrada | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-20 | 55 | 234 | Obrig. | IE do destinatário não vinculada ao CNPJ | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-30 | 55 | 624 | Obrig. | IE Destinatário não vinculada ao CPF | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-40 | 55 | 302 | Obrig. | Irregularidade fiscal do destinatário | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-43 | 55 | 305 | Obrig. | Destinatário bloqueado na UF | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-46 | 55 | 306 | Obrig. | IE do destinatário não está ativa na UF | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-50 | 55 | 232 | Obrig. | IE do destinatário não informada | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-60 | 55 | 303 | Obrig. | Destinatário não habilitado a operar na UF | NT 2019.001 v1.70 | outra operação (6) |
| 6P31-63 | 55 | 305 | Obrig. | Destinatário bloqueado na UF | NT 2019.001 v1.70 | outra operação (6) |
| P09-10 | 55/65 | 252 | Obrig. | Ambiente informado diverge do Ambiente de recebimento | NT 2026.001 v1.02b | outra operação (6) |
| P12-26 | 55/65 | 617 | Obrig. | Chave de Acesso inválida (CNPJ/CPF zerado ou dígito inválido) | NT 2026.001 v1.02b | outra operação (6) |
| P12-32 | 65 | 450 | Obrig. | Modelo da NF-e diferente de 55 | NT 2026.001 v1.02b | outra operação (6) |
| P12-40 | 55 | 249 | Obrig. | UF da Chave de Acesso diverge da UF autorizadora | NT 2026.007 v1.10 | outra operação (6) |
| P12-44 | 55/65 | 574 | Obrig. | Autor do evento diverge do emissor da NF-e | NT 2026.001 v1.02b | outra operação (6) |
| P14-10 | 55 | 669 | Obrig. | Ambiente de autorização inválido para emissão pelo PAA | NT 2026.001 v1.02b | outra operação (6) |
| P14-20 | 55 | 669 | Obrig. | Ambiente de autorização inválido para emissão pelo PAA | NT 2026.001 v1.02b | outra operação (6) |
| P31-26 | 55/65 | 910 | Obrig. | Chave de Acesso NF-e Substituta inválida (CNPJ/CPF) | NT 2026.001 v1.02b | outra operação (6) |
| P31-46 | 55/65 | 911 | Obrig. | Chave de Acesso NF-e Substituta incorreta (CNPJ/CPF) | NT 2026.001 v1.02b | outra operação (6) |
| P80-10 | 55 | 893 | Obrig. | Grupo de informações do Provedor de Assinatura e Autorização informado indevidamente | NT 2026.001 v1.02b | outra operação (6) |
| P80-20 | 55 | 893 | Obrig. | Grupo de informações do Provedor de Assinatura e Autorização informado indevidamente | NT 2026.001 v1.02b | outra operação (6) |
| P80-30 | 55 | 867 | Obrig. | Grupo de informações do Provedor de Assinatura e Autorização não informado | NT 2026.001 v1.02b | outra operação (6) |
| P80-40 | 55 | 491 | Obrig. | O tpEvento informado inválido Rejeição: Tipo de Evento inválido para o | NT 2026.001 v1.02b | outra operação (6) |
| P81-10 | 55 | 670 | Obrig. | CNPJ do PAA inválido | NT 2026.001 v1.02b | outra operação (6) |
| P81-20 | 55 | 671 | Obrig. | Provedor de Assinatura e Autorização não existe na base da SEFAZ | NT 2026.001 v1.02b | outra operação (6) |
| P81-30 | 55 | 672 | Obrig. | Emitente não associado ao PAA | NT 2026.001 v1.02b | outra operação (6) |
| P81-40 | 55 | 895 | Obrig. | Chave do Emitente para o PAA inválida | NT 2026.001 v1.02b | outra operação (6) |
| P81-50 | 55 | 776 | Obrig. | Emissão por PAA deve ser assinada pelo CNPJ do Provedor de Assinatura | NT 2026.001 v1.02b | outra operação (6) |
| P83-10 | 55 | 856 | Obrig. | Emissão por PAA com Assinatura RSA inválida | NT 2026.001 v1.02b | outra operação (6) |

## 11. O lote da etapa 3.2 (2026-10-22)

**Decisão do usuário (2026-10-22):** a validação se apoia no **schema**, que confere a forma de cada campo e agora devolve também o `xs:documentation` do campo e do tipo (`FalhaSchema.Documentacao` e `DocumentacaoTipo`, `PROJETO.md` 6.4). Das regras do MOC, a lib implementa só **um lote pequeno**: as que pegam o erro comum de quem emite, que o schema não vê, e que quase não mudam com as NTs. O resto do catálogo fica como referência do que a SEFAZ confere. Foi a opção 2 de três (a 1, só o schema; a 3, as 296 regras marcadas para a 3.2). Com isso, a tabela do `cClassTrib` **não é embutida** nesta etapa (a escolha anterior da mesma sessão, de embuti-la, valia para as regras do grupo UB, que ficaram de fora).

- **As 68 regras implementadas** (situação "lib (3.2)"; a lista aprovada pelo usuário em 2026-10-22, `PROJETO.md` 2.9), em `RegrasNFe400` (um arquivo por assunto):
  - **CPF e CNPJ** (`RegrasNFe400.Documentos.cs`): o emitente, o destinatário, a retirada, a entrega, os autorizados a baixar o XML, o fabricante, o adquirente da importação, o transportador, o CNPJ do pagamento, o intermediador e o responsável técnico (com zeros, repetidos ou com o DV errado, pelo `Cnpj.Valido` e o `Cpf.Valido` do núcleo; o alfanumérico incluído). O documento errado não vai na mensagem.
  - **DV das chaves informadas** (no mesmo arquivo): a NF-e e o CT-e referenciados, a exportação indireta e o documento anterior da compra governamental. Só o DV; o resto da chave referenciada (UF, mês, modelo) ficou fora.
  - **Totais** (`RegrasNFe400.Totais.cs`): cada campo do `ICMSTot` contra a soma dos itens (W03 a W16a) e do `ISSQNtot` contra a soma dos itens com ISSQN (W18 a W22f). Os valores vão no detalhe (`vProd 999,50; soma dos itens 1000,00`). O campo não informado vale zero; a soma é exata, salvo o `vNF` (W16-10), com a tolerância de R$ 0,50 do MOC e as três exceções (faturamento direto de veículo, importação, ICMS desonerado não descontado). O W07-10 soma os itens com `indTot=1` sem o grupo ISSQN (os sujeitos ao ICMS ou ao IBS, à CBS e ao IS, NT 2025.002).
  - **Combinações e datas** (`RegrasNFe400.Operacao.cs`): CFOP × `idDest` (I08-30, I08-40, I08-50, com a exceção do combustível consumido no exterior); `indIEDest` × IE e consumidor final (E16a-40, E17-20, E17-30, E17-40); a emissão no futuro além de 5 minutos (B09-10, pelo `ConfiguracaoDFe.Relogio`), a saída antes da emissão (B10-40), a fabricação e a validade do rastro (I83-10, I84-10) e o vencimento das duplicatas (Y09-20, Y09-30).
- **As outras 223 linhas que estavam "a fazer (3.2)"** voltaram para "a fazer", sem etapa. Ficam para a SEFAZ conferir, ou para uma etapa futura, se pedida. Entre elas: as do IBS e da CBS que dependem das tabelas de CST e `cClassTrib`; as que dependem da tabela de CFOP (`indRetor`, `indComb`), da de combustíveis monofásicos ou da de biocombustível; a IE por UF (210); as alíquotas do ICMS interestadual e da partilha; as da NFC-e e do DANFE Simplificado Tipo 2 que estavam neste lote; as das NTs 2026.006 e 2026.008, que ainda não têm pacote de schemas (`vProdLiq`, `gPgtoVinc`); as marcadas como implementação futura na fonte; e a F03 (227), que é da verificação da assinatura (`AssinaturaDigital.Verificar`, 1.4), na autorização (4.4).
- **Correção do levantamento da 3.1:** a extração do texto dos PDFs não via o tachado. A NT 2026.002 v1.11 exclui as regras BA05-10, BA06-10, BA12-10, BA13-10 e BA14-10 (conferido nas páginas 9 e 10 do PDF), agora "excluída". Outras linhas do catálogo podem ter o mesmo problema; quem implementar uma regra confere o PDF.
- **Testes** (`RegrasLoteTestes`): as duas notas de teste passam sem problema nenhum; um caso por regra, com a nota que a SEFAZ rejeitaria (a mudança na NF-e de teste, o código e o caminho do problema); a lista e os casos conferidos nos dois sentidos; o detalhe dos valores, o CPF fora da mensagem, a tolerância e as exceções do `vNF`, a do CFOP de exportação e as regras só da NF-e. As chaves referenciadas da NF-e de teste estavam com o DV errado e foram corrigidas.


## 12. As regras da NFC-e, etapa 3.3 (2026-10-22)

**Aprovado pelo usuário em 2026-10-22** (`PROJETO.md` 2.9): a lista proposta pela recomendação, sem as regras do IBS, da CBS e da ZFM; com as regras de 55/65 da NT 2026.002 que valem para a NFC-e; as que dependem da UF, do cadastro ou de tabela ficam para a SEFAZ; as do QR Code vão para a 5.1.

- **As 31 regras implementadas** (situação "lib (3.3)"), em `RegrasNFe400.NFCe.cs`, cada uma conferida no PDF da fonte:
  - **NT 2026.002 v1.11, para a NFC-e e para a NF-e com o DANFE Simplificado Tipo 2 (`tpImp=6`)**, como a NT as escreve (modelo 55/65 no catálogo): sem `dhSaiEnt` (B10-10, 705); só saída (B11-10, 706); só operação interna (B11a-10, 707); só `finNFe=1` (B25-20, 715); só consumidor final (B25a-10, 716); sem `NFref` (BA01-10, 708); sem `IEST` (C18-10, 718); na entrega em domicílio (`indPres=4`), com o destinatário (E01-20, 787); e sem os grupos do item: veículo novo (J01-10, 736), arma (L01-10, 738), RECOPI (LB01-10, 348), `ICMSPart` (N12-50, 741), `ICMSUFDest` (NA01-10, 807), IPI (O01-10, 742), II (P01-10, 743), PIS-ST (R01-10, 746) e COFINS-ST (T01-10, 749).
  - **Só a NFC-e:** sem `dPrevEntrega` (B10a-10, 1153, NT 2025.002); `tpImp` 4 ou 5 (B21-10, 709); `procEmi=0` fora da NFF, com a exceção de SC para a nota avulsa (B26-50, 957, NT 2023.002); sem `gCompraGov` (BB01-10, 1006, NT 2025.002); o CNPJ do destinatário diferente do emitente (E02-20, 220); na entrega em domicílio, o destinatário com o endereço (E05-20, 788); `indIEDest=9` (E16a-10, 789); a IE do destinatário na operação com o exterior (E17-10, 729); sem `ISUF` (E18-10, 730); o transportador só na entrega em domicílio, e obrigatório nela (X03-10, 754, e X03-20, 786, NT 2023.004); sem `tPag` 14 (YA02-10, 857) nem 90 (YA02-40, 899); sem `exporta` (ZA01-30, 814).
- **Conferido no PDF e diferente da proposta:**
  - **C02a-04 (337, "NFC-e para emitente pessoa física") não entrou.** A NT 2023.002 v1.01 (18/11/2025), posterior à NT 2021.002 v1.12, permite a NFC-e do produtor rural com CPF (seção 2, a série 920-969 e o e-CPF) e republica a C02a-10 para 55/65; a regra da NT 2021.002 recusaria uma nota hoje válida. Fica "SEFAZ".
  - **E17-10:** o Anexo I condiciona a regra à operação com o exterior (`idDest=3`), e assim ela foi escrita, mesmo que a B11a-10 já recuse essa operação na NFC-e. A extração `-layout` mistura as colunas nessa página; a `-raw` dá a condição.
  - **YA02-40 (899):** a regra é o `tPag=90` ("Sem pagamento") na NFC-e, e não o meio de pagamento em geral.
  - **E05-20:** só com o destinatário informado; a NFC-e de entrega sem destinatário é da E01-20.
- **As outras 39 linhas que estavam "a fazer (3.3)":**
  - **"a fazer (5.1)" (36):** as do QR Code (ZX02, 396, 397, 399, 400, 407, 444, 463, 464) e a URL de consulta (ZX03-20, 878). A lib vai montar o QR Code na 5.1, que decide quais viram "escrita".
  - **"SEFAZ" (14):** a configuração da UF (B06-10, B22-20, B25b-30, Z02-10, Z02-20), o autorizador (B22-70), o cadastro (C17-42), as tabelas (I05-30 NCM, I09-10 e I13-10 unidades), o município do fato gerador do IBS (B25b-60, B25b-70), a NB01-10 (NT 2026.008, ainda sem pacote de schemas) e a C02a-04.
  - **"escrita" (1):** B07-10 (450), o modelo vem do núcleo.
  - **"a fazer" (5):** as do IBS, da CBS e da ZFM (B25d-10, I05k-10, UB62-10, UB120-10, UB131-10), que mudam com as NTs da reforma.
- **Ficaram fora da proposta**, e continuam "a fazer": outras regras da NT 2026.002 do mesmo tipo (B25b-20 717 `indPres`, F01-10 669 retirada, I17b-10 774 `indTot=0`, UA01-20 `impostoDevol`, X02-10 753 frete, X11-10 a X25b-10 755 a 759 veículo e reboque, Y01-10 760 cobrança, N12-30 a N12a-44 CST e CSOSN), a B09-40 (704, a emissão atrasada, que depende do momento do recebimento) e a I08-150 (725, a lista de CFOPs). Entram só com uma nova aprovação.
- **Testes** (`RegrasNFCeTestes`, 52): um caso por regra sobre a NFC-e de teste (o código, o caminho, a severidade, a mensagem e a propriedade); as 17 da NT 2026.002 também na NF-e com `tpImp=6` e nunca na NF-e com o DANFE retrato; as só da NFC-e nunca na NF-e; a NFC-e de entrega completa passa limpa; as exceções da B26-50 (SC e NFF); o grupo no segundo item com o índice. O `CadaRegraTemOSeuCaso` do `RegrasLoteTestes` passa a somar os casos dos dois arquivos. Uma mutação provada: sem a condição do `tpImp=6`, 17 testes acusam.

## 13. O lote da autorização, etapa 4.4 (2026-10-22)

**Aprovadas pelo usuário em 2026-10-22** (`CONTRATOS.md` 14, decisão 10), conferidas na montagem do `enviNFe` (`LoteNFe`, no leiaute) como problemas da montagem, e não na `RegrasNFe400`: são do lote, não da nota. O teste do catálogo cruza a lista `LoteNFe.Regras` com as linhas "lib (4.4)".

| Regra | Código | Severidade | O que confere |
|---|---|---|---|
| B02-20 | 476 | Erro | a UF da chave de cada nota igual à da primeira (só as notas assinadas podem divergir: as do modelo levam a UF do núcleo) |
| B06-20 | 765 | Erro | o modelo da chave de cada nota igual ao documento do núcleo |
| C02-20, C02a-30 | 560 | Aviso (facultativa) | o CNPJ base, ou o CPF, do emitente da chave igual ao da primeira nota |
| F02 | 297 | Erro | a assinatura de cada nota assinada recebida (`AssinaturaDigital.Verificar`), com as rejeições da estrutura (298) e do certificado (290) que a verificação dá; o titular (213, 227, 292) fica para a SEFAZ |
| (lib) | `NFE-LOTE` | Erro | a mesma chave duas vezes no lote; o tipo de emissão diferente do da primeira nota |

O resto do que era "a fazer (4.4)" e "outra operação (4.4)": o 214 (tamanho da mensagem) e o 776 (a UF sem a síncrona) ficam para a SEFAZ; o 764 (síncrona com mais de uma nota) a montagem evita, porque a quantidade decide o `indSinc` (escrita); o YA03-20 (866, troco) é regra da nota, não do lote, e volta a "a fazer".

## 14. A consulta do protocolo e a recuperação, etapa 4.5 (2026-10-22)

**Aprovada pelo usuário em 2026-10-22** (`CONTRATOS.md` 15, decisão 3), conferida na montagem do `consSitNFe` (`SituacaoNFe`, no leiaute). Não é regra do MOC, e o catálogo não tem linha para ela.

| Regra | Código | Severidade | O que confere |
|---|---|---|---|
| (lib) | `NFE-CHAVE` | Erro | a chave consultada (ou a do XML recuperado) é do modelo do núcleo: a de NFC-e num núcleo de NF-e, e o contrário, iria ao web service do outro documento |

As regras J01 a J06 da consulta (Tabela 5-16) ficam com a SEFAZ: o DV, a UF, o ano, o mês, o CNPJ ou o CPF, o modelo e o número da chave (236, 614 a 619) a `ChaveAcesso` já garante antes de existir; o ambiente (252) é o do núcleo. A recuperação confere a assinatura do XML como a autorização confere a da nota recebida (seção 13, F02).
