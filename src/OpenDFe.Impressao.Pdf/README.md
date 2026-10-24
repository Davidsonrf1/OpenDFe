# OpenDFe.Impressao.Pdf

> **Licença: leia antes de usar.** Este pacote gera o PDF com o **QuestPDF**, que **não** é MIT: tem licença própria (fonte disponível, não aprovada pela OSI), com uma versão gratuita (Community) e licenças pagas (Professional e Enterprise). **A escolha e a obrigação são de quem usa o `OpenDFe.Impressao.Pdf`, não da OpenDFe.** O resto da OpenDFe (núcleo, NF-e, leiaute) é MIT e não depende do QuestPDF.
>
> Conferido no site oficial em **2026-10-24** (Community License e License Selection Guide, versão 3.0, de 6 de julho de 2026; o texto também vem no pacote NuGet `QuestPDF` 2026.9.1): a Community é gratuita, inclusive para uso comercial, para pessoa física com receita bruta anual abaixo de USD 1.000.000, **organização com receita bruta anual abaixo de USD 1.000.000** no último exercício (consolidada entre as entidades sob controle comum), projeto open source sob licença aprovada pela OSI, entidade de caridade e instituição acadêmica. **Órgãos públicos e empresas de capital aberto não são elegíveis, qualquer que seja a receita.** Quem não se enquadra precisa de licença paga antes do uso em produção. Quem só usa uma biblioteca que depende do QuestPDF, sem chamar a API dele, pode se enquadrar como "usuário de dependência transitiva", **exceto** se essa biblioteca for basicamente um invólucro do QuestPDF: se este pacote vale como invólucro ou não é questão do texto da licença, que **você** deve ler e, na dúvida, perguntar ao QuestPDF (contact@questpdf.com). Os termos valem na versão do QuestPDF que você instala; releia ao atualizar.
>
> Textos oficiais: <https://www.questpdf.com/license/> e <https://www.questpdf.com/pricing>. Este aviso é um resumo, não aconselhamento jurídico; vale o texto da licença.

## O que o pacote faz

Renderiza em PDF o **modelo de impressão** da OpenDFe (`DocumentoImpressao`): o DANFE da NF-e (A4, retrato e paisagem) e o DANFCE da NFC-e (bobina de 80 e de 58 mm), do DANFE Simplificado Tipo 2 à contingência com duas vias. O modelo vem do leiaute (`LeiauteNFe400.Danfe` e `Danfce`); o pacote só o **desenha**, sem tomar decisão fiscal e sem mexer no conteúdo: a bobina sai com a largura do papel e altura contínua, e a folha A4 sai paginada, com o cabeçalho da tabela e o de continuação repetidos, a marca d'água e a numeração das folhas. O mapeamento, elemento a elemento, está em `docs/CONTRATOS.md` 22 do repositório, e a conferência contra o manual, em `docs/IMPRESSAO.md` 3.

O código de barras (Code 128, conjunto C), o QR Code (nível M, UTF-8, como o manual do DANFE NFC-e manda, com a margem de silêncio) e as imagens de 1 bit vão ao PDF como **vetor** (SVG), nítidos em qualquer resolução. A biblioteca não grava nada em disco nem guarda estado, e gera em paralelo sem problema.

`RenderizadorPdf.GerarPaginaTeste()` é o diagnóstico: uma página A4 com texto acentuado, um código de barras, um QR Code e uma imagem de 1 bit, para conferir no ambiente que o pacote, a licença e as fontes funcionam.

## Como usar

```csharp
// Na partida do aplicativo (uma vez): a licença do QuestPDF (abaixo).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// A NF-e autorizada (o nfeProc) lida de volta, o DANFE montado e o PDF gerado.
var leiaute = new LeiauteNFe400();
NotaLida nota = leiaute.Ler(xmlProcessado);
DocumentoImpressao danfe = leiaute.Danfe(nota);                 // A4, retrato ou paisagem pelo tpImp da nota
byte[] pdf = RenderizadorPdf.Gerar(danfe);                      // em memória

// Ou direto num arquivo, com o título e o autor do PDF.
using var arquivo = File.Create("danfe.pdf");
RenderizadorPdf.Gerar(danfe, arquivo, new OpcoesPdf { Autor = "Minha Empresa Ltda" });

// O DANFCE da NFC-e: bobina de 80 mm (ou OpcoesDanfce { Papel = PapelImpressao.Bobina58 }).
byte[] bilhete = RenderizadorPdf.Gerar(leiaute.Danfce(nota));
```

`Gerar` lança `ArgumentNullException` com o documento nulo e `InvalidOperationException` se a licença do QuestPDF não foi definida. Não há estado entre as chamadas: dá para gerar vários PDFs ao mesmo tempo.

## Como definir a licença

O pacote **não** define a licença: é estado global do QuestPDF e escolha do aplicativo. Defina uma vez, na partida do aplicativo, antes de gerar o primeiro PDF:

```csharp
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
// ou LicenseType.Professional / LicenseType.Enterprise, se a licença paga for a que se aplica
```

Sem isso, `RenderizadorPdf` lança `InvalidOperationException` com uma mensagem que explica a escolha e aponta para este README.

## Quem não precisa deste pacote

O aplicativo que imprime em impressora térmica por ESC/POS (o PDV) **não** precisa do `OpenDFe.Impressao.Pdf`: carrega só o núcleo, o leiaute e o modelo de impressão, sem o QuestPDF e sem a licença dele.

## Linux

O QuestPDF traz as bibliotecas nativas dele (Skia e qpdf) no próprio pacote, para `linux-x64`, `linux-arm64` e `linux-musl-x64` (Alpine), além de Windows e macOS. Na versão 2026.9.1 elas dependem só de `libstdc++`, `libm`, `libgcc_s`, `libc` e `libz` (conferido com `readelf -d`); a documentação do QuestPDF não pede mais nenhum pacote do sistema (nem `libfontconfig`). Em imagem mínima, confira que `libstdc++` e `zlib` estão instalados.

## Fontes

Sem registrar nada, o QuestPDF usa a fonte embutida Lato (SIL OFL 1.1), que cobre o português: todos os caracteres do Latin-1 (os que o schema da NF-e admite), o Latin Extended-A e B e a pontuação geral. **Nenhum caractere derruba a emissão:** o que a Lato não desenha vira o equivalente ASCII ou `?` (`docs/CONTRATOS.md` 22.4).

O Anexo II do MOC pede o DANFE em **Times New Roman** ou **Courier New** (3.7), que não são livres para embutir no pacote. O pacote **não registra fonte nenhuma** (o `FontManager` do QuestPDF é global): o aplicativo que quer uma delas (ou uma metricamente compatível, como a Liberation Serif) a registra uma vez, na partida e antes do primeiro PDF, e passa o nome da família nas opções:

```csharp
QuestPDF.Drawing.FontManager.RegisterFontFromFile("Fontes/LiberationSerif-Regular.ttf");
QuestPDF.Drawing.FontManager.RegisterFontFromFile("Fontes/LiberationSerif-Bold.ttf");

byte[] pdf = RenderizadorPdf.Gerar(danfe, new OpcoesPdf { Fonte = "Liberation Serif" });
```

Se a família pedida não estiver registrada, o PDF sai em Lato (a emissão não quebra por falta de fonte). Os tamanhos em pontos são os do Anexo II (mínimos de 5, 6, 8, 10 e 12 pt); numa fonte mais estreita que a Lato o texto ocupa menos linhas.

## Terceiros

| Componente | Licença | Link |
|---|---|---|
| QuestPDF (gera o PDF) | Community, Professional ou Enterprise do QuestPDF (acima) | <https://www.questpdf.com/license/> |
| QRCoder (a matriz do QR Code) | MIT | <https://github.com/codebude/QRCoder> |
| Lato, embutida no QuestPDF | SIL OFL 1.1 | <https://www.latofonts.com> |

O repositório da OpenDFe: <https://github.com/Davidsonrf1/OpenDFe> (licença MIT para o código da OpenDFe).
