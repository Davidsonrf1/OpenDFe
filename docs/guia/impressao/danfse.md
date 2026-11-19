# Imprimir o DANFSe

[Documentação](../../index.md) › Impressão › DANFSe

O DANFSe é o documento auxiliar da NFS-e nacional. Desde 03/08/2026 a API do Ambiente de Dados Nacional que gerava o DANFSe está suspensa: **quem emite gera o DANFSe**, no leiaute da NT SE/CGNFS-e 008/2026 (v1.02). A OpenDFe o monta a partir do XML da NFS-e, e o pacote `OpenDFe.Impressao.Pdf` o desenha em PDF, numa folha A4 só.

## Gerar

```csharp
using OpenDFe.Impressao.Pdf;
using OpenDFe.NFSe;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;  // escolha de quem usa (veja "Gerar o PDF")

var leiaute = LeiauteNFSe101.Instancia;
var nota = leiaute.Ler(xmlNFSe);                 // o XML que a emissão ou a consulta devolveu (1.00 ou 1.01)
var documento = leiaute.Danfse(nota);            // o DocumentoImpressao
byte[] pdf = RenderizadorPdf.Gerar(documento);
```

O `xmlNFSe` é o `Xml` do retorno da [emissão](../nfse/emissao.md) ou da consulta pela chave. A DPS sozinha não tem DANFSe: `Danfse` recusa a leitura sem a NFS-e (`ArgumentException`).

## Opções

| Opção | Padrão | O que faz |
|---|---|---|
| `Canhoto` | `true` | O canhoto no pé da folha (data de cientificação, identificação e assinatura, número e chave). Sem ele, a altura vai às informações complementares. |
| `Cancelada` | `false` | A marca d'água "CANCELADA" em diagonal. |
| `Substituida` | `false` | A marca d'água "SUBSTITUÍDA" em diagonal. |

**A situação vem de você.** A biblioteca não sabe se a nota foi cancelada ou substituída depois de emitida: consulte os eventos e passe a opção.

```csharp
var eventos = await dfe.ExecutarAsync(new ConsultaEventosNFSe(chave));
var tipos = eventos.Retorno!.Eventos.Select(e => e.Tipo).ToHashSet();
var documento = leiaute.Danfse(nota, new OpcoesDanfse
{
    Cancelada = tipos.Contains("101101"),
    Substituida = tipos.Contains("105102"),
});
```

As duas juntas são recusadas.

## O que sai no DANFSe

O formulário segue o Anexo I da NT 008/2026, com as posições e os tamanhos da tabela do item 2.4.5:

- o cabeçalho com a logomarca da NFS-e, "DANFSe v2.0", o município do emitente, o ambiente gerador e o tipo de ambiente;
- os dados da NFS-e (chave, número, competência, datas, DPS, emitente, situação, finalidade) e o QR Code da consulta pública (`https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=...`);
- o prestador, o tomador, o destinatário e o intermediário; o serviço; a tributação municipal, federal e do IBS/CBS; os totais; as informações complementares, com a linha dos totais aproximados dos tributos (Lei 12.741/2012);
- na produção restrita (`tpAmb` 2), "NFS-e SEM VALIDADE JURÍDICA" em vermelho, abaixo do título.

Regras que a biblioteca aplica sozinha:

- **O bloco sem dados vira uma frase** ("TOMADOR/ADQUIRENTE DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e", "O DESTINATÁRIO É O PRÓPRIO TOMADOR/ADQUIRENTE DA OPERAÇÃO", "TRIBUTAÇÃO MUNICIPAL (ISSQN) - OPERAÇÃO NÃO SUJEITA AO ISSQN"), e a altura que sobra vai à descrição do serviço e às informações complementares.
- **O texto longo é cortado com reticências**, nos tamanhos da NT (1.300 caracteres na descrição, 2.000 nas informações complementares); a linha dos tributos nunca se corta. O DANFSe nunca passa de uma folha.
- **O campo sem informação no XML sai com um traço.**
- **O nome do município** do prestador, do tomador e dos outros vem de uma tabela do IBGE embutida: o XML só traz o código.
- **O nome e o endereço do prestador** vêm do emitente da NFS-e quando a DPS não os traz (o ambiente nacional os tira do cadastro).
- A linha do PIS e da COFINS de apuração própria só sai com a competência até dezembro de 2026.

> **Atenção: fontes**
> A NT pede Arial nos rótulos e Microsoft Sans Serif nos conteúdos. O PDF usa a Lato, embutida no QuestPDF. Para outra família, registre a fonte no aplicativo e passe `new OpcoesPdf { Fonte = "Arial" }` (uma família para tudo). Os tamanhos em pontos são os da NT.

## O que guardar

- O XML da NFS-e: ele é o documento. O DANFSe se gera de novo a qualquer momento a partir dele.

## Prompt para agentes de IA

Peça a um agente que gere o DANFSe. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Gere o PDF do DANFSe com a OpenDFe, seguindo docs/guia/impressao/danfse.md: LeiauteNFSe101.Instancia.Ler(xml da NFS-e),
LeiauteNFSe101.Instancia.Danfse(nota, new OpcoesDanfse { Canhoto, Cancelada, Substituida }) e RenderizadorPdf.Gerar.
A situação vem da ConsultaEventosNFSe (101101 cancelada, 105102 substituída). Defina a licença do QuestPDF na partida.
O XML vem de <descreva>.
```

## Próximas etapas

- [Gerar o PDF](pdf.md)
- [Emitir a NFS-e](../nfse/emissao.md)
- [Cancelar e substituir a NFS-e](../nfse/eventos.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
