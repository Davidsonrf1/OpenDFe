# Ler um XML recebido

[Documentação](../../index.md) › NF-e e NFC-e › Ler um XML

A leitura transforma o XML de uma NF-e ou NFC-e (o `NFe` ou o `nfeProc`) no modelo `NotaFiscal`, com a chave e o protocolo. É o caminho da caixa de entrada (as notas de compra que os fornecedores enviam), da reimpressão e do DANFE. Ela não precisa de certificado nem de núcleo.

## Ler

```csharp
using OpenDFe;
using OpenDFe.NFe;

var leiaute = new LeiauteNFe400();
NotaLida lida = leiaute.Ler(xml);

if (lida.Problemas.Any(p => p.Severidade == Severidade.Erro))
{
    foreach (var problema in lida.Problemas)
        Console.WriteLine(problema);
}

var nota = lida.Nota!;
Console.WriteLine(nota.Emitente.Nome);
Console.WriteLine(lida.Chave);                         // a chave do Id
Console.WriteLine(lida.Protocolo?.Status);             // 100 no nfeProc autorizado
foreach (var item in nota.Itens)
    Console.WriteLine($"{item.Produto.Codigo} {item.Produto.Descricao} {item.Produto.QuantidadeComercial} {item.Produto.Valor}");
```

| Membro de `NotaLida` | Descrição |
|---|---|
| `Nota` | A `NotaFiscal`; nula só quando o XML não é uma NF-e do leiaute 4.00 |
| `Chave` | A `ChaveAcesso` do `Id` do `infNFe`; nula quando ausente ou inválida |
| `Protocolo` | O protocolo do `nfeProc`; nulo no `NFe` sozinho |
| `Xml` | O XML recebido, a mesma instância que você passou |
| `Problemas` | O que a leitura não reconheceu ou não converteu, na ordem do XML |

## Como a leitura se comporta

- **Aceita o que a SEFAZ aceitou.** O `NFe` e o `nfeProc` de qualquer Nota Técnica do leiaute 4.00, desde 2017. Os campos que saíram do schema numa NT posterior viram o aviso `NFE-RETIRADO` e ficam no XML original.
- **Não valida pelo schema nem confere as regras.** A nota recebida já passou pelo schema da época. A leitura lê o que reconhece e aponta o que não reconhece.
- **Não lança pelo conteúdo.** XML mal formado, outra raiz ou outra versão viram problemas (`NFE-XML`, `NFE-RAIZ`, `NFE-VERSAO`), com a `Nota` nula.
- **Tolera a forma.** XML indentado, com BOM, com comentários, com os atributos em outra ordem ou com prefixo de namespace lê igual.
- **É segura.** Sem DTD nem entidade externa: o XML vem de fora.
- **Não confere a assinatura.** Para isso, use `AssinaturaDigital.Verificar` (veja [Certificado digital](../conceitos/certificado.md)).

| Código | Severidade | Quando |
|---|---|---|
| `NFE-XML` | Erro | O texto não é XML bem formado |
| `NFE-RAIZ` | Erro | A raiz não é `NFe` nem `nfeProc` |
| `NFE-VERSAO` | Erro | A versão não é 4.00 |
| `NFE-CHAVE` | Erro | O `Id` falta ou a chave é inválida |
| `NFE-PROTOCOLO` | Erro | O protocolo é de outra nota |
| `NFE-VALOR` | Erro | Um valor que não converte (a propriedade fica nula; o valor não vai na mensagem) |
| `NFE-RETIRADO` | Aviso | Um campo de NT anterior que saiu do schema |
| `NFE-DESCONHECIDO` | Aviso | Um elemento que o leiaute não conhece |
| `NFE-REPETIDO` | Aviso | Um elemento repetido onde o schema aceita um (vale o primeiro) |
| `NFE-ENUMERACAO` | Aviso | Um código fora da enumeração (entra como o número) |
| `NFE-ITEM` | Aviso | O `nItem` fora da sequência |

> **Importante**
> Na caixa de entrada, confira o `NFE-PROTOCOLO`. Um `nfeProc` com o protocolo de outra nota é um caso que o `Protocolo.Status == 100` sozinho não pega.

## Guardar o XML original

**Guarde o `lida.Xml`, nunca um XML gerado de novo a partir da `Nota`.** Escrever a nota de novo não reproduz os bytes que a assinatura cobre: o XML regerado não é a nota fiscal.

## Detectar o documento

Quando você não sabe de antemão o que o XML é (a caixa de entrada recebe NF-e, NFC-e, de versões diferentes), use o detector. Ele lê só a raiz, a versão e o modelo, e diz qual dos leiautes que você registrou sabe ler o XML:

```csharp
var leiaute = new LeiauteNFe400();

if (DFe.Detectar(xml, leiaute) is { Leiaute: LeiauteNFe400 nfe } deteccao)
{
    Console.WriteLine($"{deteccao.Documento} {deteccao.Raiz} {deteccao.Versao}");   // NFe nfeProc 4.00
    var lida = nfe.Ler(xml);
}
else
{
    // Nenhum leiaute lê este XML (outra versão, outro documento, ou não é XML).
}
```

`Detectar` devolve `null` quando nenhum leiaute lê o XML, inclusive quando ele não é XML bem formado, e não lança pelo conteúdo. Com vários leiautes, vale o primeiro que lê, na ordem em que você os passou.

## Exemplo: a caixa de entrada

```csharp
var leiaute = new LeiauteNFe400();
var lida = leiaute.Ler(xmlRecebido);

if (lida.Nota is null || lida.Problemas.Any(p => p.Severidade == Severidade.Erro))
    return Recusar(lida.Problemas);

if (lida.Protocolo?.Status is not (100 or 150))
    return Recusar("A nota não está autorizada.");

if (lida.Nota.Destinatario?.Cnpj != meuCnpj)
    return Recusar("A nota não é destinada a esta empresa.");

var problemasAssinatura = AssinaturaDigital.Verificar(lida.Xml, "NFe" + lida.Chave, lida.Nota.Emitente.Cnpj ?? lida.Nota.Emitente.Cpf!);
if (problemasAssinatura.Count > 0)
    return Recusar(problemasAssinatura);

Guardar(lida.Chave!, lida.Xml);
```

Para confirmar a situação atual da nota na SEFAZ (ela pode ter sido cancelada depois), use a [consulta do protocolo](consulta-protocolo.md) ou a [distribuição DF-e](distribuicao-dfe.md).

## Prompt para agentes de IA

Peça a um agente que escreva a importação das notas de compra. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva a importação de XMLs de NF-e recebidos de fornecedores com a OpenDFe, seguindo docs/guia/nfe/leitura-xml.md:
DFe.Detectar para saber se o leiaute lê o XML; LeiauteNFe400.Ler; recusar com os Problemas de severidade Erro (inclusive NFE-PROTOCOLO);
aceitar só Protocolo.Status 100 ou 150 e o destinatário <meu CNPJ>; verificar a assinatura com AssinaturaDigital.Verificar contra o emitente;
guardar lida.Xml (o XML recebido, nunca um regerado). Mapeie para o meu modelo de entrada <descreva>: itens, quantidades, valores, lotes.
```

## Próximas etapas

- [Baixar as notas destinadas: distribuição DF-e](distribuicao-dfe.md)
- [Modelo de impressão: DANFE e DANFCE](../impressao/modelo-impressao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
