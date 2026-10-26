# Gerar o PDF do DANFE e do DANFCE

[Documentação](../../index.md) › Impressão › PDF

O pacote `OpenDFe.Impressao.Pdf` desenha o [modelo de impressão](modelo-impressao.md) em PDF: o DANFE em A4 paginado e o DANFCE na largura da bobina, com altura contínua. O código de barras, o QR Code e as imagens vão como vetor, nítidos em qualquer resolução.

## Licença do QuestPDF

> **Importante**
> O `OpenDFe.Impressao.Pdf` usa o **QuestPDF**, que **não** é MIT: tem licença própria, com uma versão gratuita (Community) e licenças pagas (Professional e Enterprise). **A escolha e a obrigação são de quem usa o pacote.** O resto da OpenDFe é MIT e não depende do QuestPDF.

Pelo texto conferido em 24/10/2026 (licença 3.0, de 6 de julho de 2026), a Community é gratuita, inclusive para uso comercial, para pessoas físicas e organizações com receita bruta anual abaixo de USD 1.000.000, projetos open source sob licença aprovada pela OSI, entidades de caridade e instituições acadêmicas. Órgãos públicos e empresas de capital aberto não são elegíveis, qualquer que seja a receita. Leia o texto oficial em <https://www.questpdf.com/license/> e o [README do pacote](https://github.com/Davidsonrf1/OpenDFe/blob/main/src/OpenDFe.Impressao.Pdf/README.md) antes de usar.

O pacote não define a licença: é estado global do QuestPDF e escolha do aplicativo. Defina uma vez, na partida do aplicativo, antes do primeiro PDF:

```csharp
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
// ou LicenseType.Professional / LicenseType.Enterprise
```

Sem isso, `RenderizadorPdf.Gerar` lança `InvalidOperationException` com a explicação.

## Gerar

```csharp
using OpenDFe.Impressao.Pdf;
using OpenDFe.NFe;

var leiaute = new LeiauteNFe400();
var nota = leiaute.Ler(xmlProcessado);

// O DANFE, em memória.
byte[] pdf = RenderizadorPdf.Gerar(leiaute.Danfe(nota));

// O DANFCE, direto num arquivo, com os metadados do PDF.
using var arquivo = File.Create($"{nota.Chave}.pdf");
RenderizadorPdf.Gerar(leiaute.Danfce(nota), arquivo, new OpcoesPdf { Titulo = $"NFC-e {nota.Chave}", Autor = "Loja Exemplo Ltda" });
```

`Gerar` é síncrono, sem estado e seguro para uso concorrente. O fluxo não é fechado nem reposicionado.

| Opção (`OpcoesPdf`) | Padrão | Descrição |
|---|---|---|
| `Titulo` | o `Titulo` do documento | O título do PDF |
| `Autor` | nenhum | O autor do PDF |
| `Criador` | `OpenDFe` | O aplicativo criador |
| `Fonte` | Lato (embutida no QuestPDF) | A família de fonte, registrada pelo aplicativo |

## Fontes

Sem registrar nada, o PDF usa a **Lato**, embutida no QuestPDF, que cobre o português. O Anexo II do MOC pede Times New Roman ou Courier New no DANFE; a biblioteca não embute essas fontes. Para usá-las, registre a fonte no QuestPDF e informe o nome:

```csharp
using QuestPDF.Drawing;

FontManager.RegisterFont(File.OpenRead("/usr/share/fonts/truetype/liberation/LiberationSerif-Regular.ttf"));
var pdf = RenderizadorPdf.Gerar(danfe, new OpcoesPdf { Fonte = "Liberation Serif" });
```

A Liberation Serif tem as mesmas medidas da Times New Roman. Uma família não registrada cai na Lato. Nenhum caractere derruba a emissão: o que a fonte não desenha vira o equivalente ASCII ou `?`.

## Conferir o ambiente

`RenderizadorPdf.GerarPaginaTeste()` gera uma página A4 com texto acentuado, um código de barras, um QR Code e uma imagem de 1 bit. Use-a para conferir, no servidor, que o pacote, a licença e as fontes funcionam.

## Linux

O QuestPDF traz as bibliotecas nativas (Skia e qpdf) no pacote, para `linux-x64`, `linux-arm64` e `linux-musl-x64`. Elas dependem só de `libstdc++` e `zlib`. Em imagem de contêiner mínima, confira que esses dois estão instalados.

## Quem não precisa deste pacote

O aplicativo que imprime só em impressora térmica (ESC/POS) não precisa do `OpenDFe.Impressao.Pdf` nem da licença do QuestPDF: o modelo de impressão já está no núcleo. Veja [Imprimir em impressora térmica](escpos.md).

## Limitações conhecidas

As diferenças do PDF em relação ao Anexo II do MOC estão em [IMPRESSAO.md](../../IMPRESSAO.md): o quadro de identificação empilhado em vez de lado a lado, o nome do emitente em 8 pt (o anexo pede 12 pt) e o canhoto da paisagem na horizontal.

## Prompt para agentes de IA

Peça a um agente que gere e entregue o PDF. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Gere o PDF do DANFE/DANFCE com o OpenDFe.Impressao.Pdf, seguindo docs/guia/impressao/pdf.md, e <envie por e-mail / devolva num
endpoint ASP.NET / grave em arquivo>. Defina QuestPDF.Settings.License uma vez na partida (a escolha da licença é minha: <Community /
Professional>); use RenderizadorPdf.Gerar com Stream para não carregar tudo em memória; OpcoesPdf com Titulo e Autor; fonte
<padrão Lato / Liberation Serif registrada com FontManager.RegisterFont>. A geração é thread-safe e síncrona.
```

## Próximas etapas

- [Imprimir em impressora térmica (ESC/POS)](escpos.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
