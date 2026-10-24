# OpenDFe.Impressao.Pdf

> **Licença: leia antes de usar.** Este pacote gera o PDF com o **QuestPDF**, que **não** é MIT: tem licença própria (fonte disponível, não aprovada pela OSI), com uma versão gratuita (Community) e licenças pagas (Professional e Enterprise). **A escolha e a obrigação são de quem usa o `OpenDFe.Impressao.Pdf`, não da OpenDFe.** O resto da OpenDFe (núcleo, NF-e, leiaute) é MIT e não depende do QuestPDF.
>
> Conferido no site oficial em **2026-10-24** (Community License e License Selection Guide, versão 3.0, de 6 de julho de 2026; o texto também vem no pacote NuGet `QuestPDF` 2026.9.1): a Community é gratuita, inclusive para uso comercial, para pessoa física com receita bruta anual abaixo de USD 1.000.000, **organização com receita bruta anual abaixo de USD 1.000.000** no último exercício (consolidada entre as entidades sob controle comum), projeto open source sob licença aprovada pela OSI, entidade de caridade e instituição acadêmica. **Órgãos públicos e empresas de capital aberto não são elegíveis, qualquer que seja a receita.** Quem não se enquadra precisa de licença paga antes do uso em produção. Quem só usa uma biblioteca que depende do QuestPDF, sem chamar a API dele, pode se enquadrar como "usuário de dependência transitiva", **exceto** se essa biblioteca for basicamente um invólucro do QuestPDF: se este pacote vale como invólucro ou não é questão do texto da licença, que **você** deve ler e, na dúvida, perguntar ao QuestPDF (contact@questpdf.com). Os termos valem na versão do QuestPDF que você instala; releia ao atualizar.
>
> Textos oficiais: <https://www.questpdf.com/license/> e <https://www.questpdf.com/pricing>. Este aviso é um resumo, não aconselhamento jurídico; vale o texto da licença.

## O que o pacote faz

Gera o PDF do DANFE e do DANFCE a partir do modelo de impressão da OpenDFe (o modelo e o renderizador chegam nas etapas 7.2 a 7.4). Nesta versão há só o diagnóstico, `RenderizadorPdf.GerarPaginaTeste()`: uma página A4 com texto acentuado, um código de barras Code 128, um QR Code e uma imagem de 1 bit, para conferir no ambiente que o pacote, a licença e as fontes funcionam.

O código de barras (Code 128, conjunto C), o QR Code (nível M, UTF-8, como o manual do DANFE NFC-e manda) e as imagens de 1 bit vão ao PDF como **vetor** (SVG), nítidos em qualquer resolução. A biblioteca não grava nada em disco nem guarda estado.

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

Sem registrar nada, o QuestPDF usa a fonte embutida Lato (SIL OFL 1.1), que cobre o português. O pacote **não registra fonte nenhuma** (o `FontManager` do QuestPDF é global). Para usar outra, o aplicativo a registra uma vez, na partida e antes do primeiro PDF, como a documentação do QuestPDF mostra:

```csharp
QuestPDF.Drawing.FontManager.RegisterFontFromFile("Fontes/MinhaFonte-Regular.ttf");
```

## Terceiros

| Componente | Licença | Link |
|---|---|---|
| QuestPDF (gera o PDF) | Community, Professional ou Enterprise do QuestPDF (acima) | <https://www.questpdf.com/license/> |
| QRCoder (a matriz do QR Code) | MIT | <https://github.com/codebude/QRCoder> |
| Lato, embutida no QuestPDF | SIL OFL 1.1 | <https://www.latofonts.com> |

O repositório da OpenDFe: <https://github.com/Davidsonrf1/OpenDFe> (licença MIT para o código da OpenDFe).
