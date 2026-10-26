# Namespace OpenDFe.Impressao.Pdf

O renderizador PDF do modelo de impressão. Pacote NuGet: `OpenDFe.Impressao.Pdf`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`OpcoesPdf`](#opcoespdf) | As opções do PDF: os metadados do arquivo e a família de fonte. O conteúdo e o papel vêm do [`DocumentoImpressao`](opendfe-impressao.md#documentoimpressao); as opções não mudam o que é impresso. |
| [`RenderizadorPdf`](#renderizadorpdf) | O renderizador PDF do modelo de impressão: [`Gerar`](#renderizadorpdf) desenha qualquer [`DocumentoImpressao`](opendfe-impressao.md#documentoimpressao) (o DANFCE e o DANFE) e [`GerarPaginaTeste`](#renderizadorpdf) é o diagnóstico do ambiente. Síncrono, sem estado e seguro em paralelo; não toca nenhuma configuração global do QuestPDF (docs/CONTRATOS.md 22). |

## OpcoesPdf

Classe selada · Namespace `OpenDFe.Impressao.Pdf`

As opções do PDF: os metadados do arquivo e a família de fonte. O conteúdo e o papel vêm do [`DocumentoImpressao`](opendfe-impressao.md#documentoimpressao); as opções não mudam o que é impresso.

### Construtores

| Construtor | Descrição |
|---|---|
| `OpcoesPdf()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Titulo` | `string?` | get; init | O título do PDF; `null` usa o [`Titulo`](opendfe-impressao.md#documentoimpressao) do documento. |
| `Autor` | `string?` | get; init | O autor do PDF; `null` não grava autor. |
| `Criador` | `string` | get; init | O criador do PDF (o programa que o gerou). |
| `Fonte` | `string?` | get; init | A família de fonte registrada pelo aplicativo (por exemplo `Times New Roman`, que o Anexo II do MOC pede para o DANFE), ou `null` para a fonte embutida do QuestPDF (Lato). O pacote não registra fonte: o aplicativo a registra uma vez, na partida, antes do primeiro PDF (README do pacote). Se a família pedida não estiver registrada (nem ativada em `QuestPDF.Settings.UseSystemFonts`), o PDF sai em Lato: a emissão não quebra por falta de fonte. |

## RenderizadorPdf

Classe estática · Namespace `OpenDFe.Impressao.Pdf`

O renderizador PDF do modelo de impressão: [`Gerar`](#renderizadorpdf) desenha qualquer [`DocumentoImpressao`](opendfe-impressao.md#documentoimpressao) (o DANFCE e o DANFE) e [`GerarPaginaTeste`](#renderizadorpdf) é o diagnóstico do ambiente. Síncrono, sem estado e seguro em paralelo; não toca nenhuma configuração global do QuestPDF (docs/CONTRATOS.md 22).

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Gerar(DocumentoImpressao documento, OpcoesPdf? opcoes = null)` | `byte[]` | Renderiza o documento de impressão em PDF, em memória: a bobina sai com a largura do papel e altura contínua; a folha A4 sai paginada, com a margem do papel (docs/CONTRATOS.md 22). **Retorna:** Os bytes do PDF. **`ArgumentNullException`:** O documento é nulo. **`InvalidOperationException`:** A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote. |
| `static Gerar(DocumentoImpressao documento, Stream destino, OpcoesPdf? opcoes = null)` | `void` | Renderiza o documento de impressão em PDF, no fluxo dado (o fluxo não é fechado nem reposicionado). **`ArgumentNullException`:** O documento ou o fluxo é nulo. **`InvalidOperationException`:** A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote. |
| `static GerarPaginaTeste()` | `byte[]` | Gera um PDF de uma página A4 com texto acentuado, um Code 128, um QR Code e uma imagem de 1 bit: serve para conferir que o pacote, a licença do QuestPDF e as fontes funcionam no ambiente. **Retorna:** Os bytes do PDF. **`InvalidOperationException`:** A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote. |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
