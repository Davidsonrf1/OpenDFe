# Modelo de impressão: DANFE e DANFCE

[Documentação](../../index.md) › Impressão › Modelo de impressão

A OpenDFe monta o DANFE (da NF-e) e o DANFCE (da NFC-e) num **modelo de impressão neutro**: uma lista de elementos (textos, linhas, separadores, QR Code, código de barras, imagens, seções e tabelas) já formatados, sem fonte nem coordenada. O mesmo modelo serve ao [renderizador PDF](pdf.md) e à [impressora térmica](escpos.md).

```
nfeProc (XML) ── LeiauteNFe400.Ler ──► NotaLida ── Danfe / Danfce ──► DocumentoImpressao
                                                                          ├─► RenderizadorPdf.Gerar  → PDF
                                                                          └─► o seu código ESC/POS   → térmica
```

O modelo fica no núcleo (namespace `OpenDFe.Impressao`) e não conhece a NF-e; o DANFE e o DANFCE ficam no leiaute.

## O DANFCE

```csharp
var leiaute = new LeiauteNFe400();
NotaLida nota = leiaute.Ler(xmlProcessado);              // o nfeProc autorizado, ou o NFe assinado em contingência

DocumentoImpressao danfce = leiaute.Danfce(nota, new OpcoesDanfce
{
    Papel = PapelImpressao.Bobina80,                     // ou Bobina58
    Logotipo = logotipo,                                 // Imagem1Bit, no topo
    TextoRodape = "Obrigado pela preferência!",
});
```

| Opção (`OpcoesDanfce`) | Padrão | Descrição |
|---|---|---|
| `Papel` | `Bobina80` | `PapelImpressao.Bobina80` (48 colunas) ou `Bobina58` (32 colunas) |
| `Logotipo` | nenhum | O logotipo do emitente, em 1 bit, acima do cabeçalho |
| `AssinaturaSoftware` | nenhuma | A imagem do software emissor, em 1 bit, no fim |
| `TextoRodape` | nenhum | Uma linha livre depois do conteúdo da NFC-e |
| `Via` | `Consumidor` | `ViaDanfce.Consumidor` ou `Estabelecimento`. A biblioteca não decide quantas vias imprimir: chame `Danfce` uma vez por via |

O DANFCE segue a ordem e os textos do Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0: o cabeçalho, os itens, os totais, as formas de pagamento, a consulta pela chave, o consumidor, a identificação, o protocolo, o QR Code e as mensagens. O QR Code é o texto do `infNFeSupl` da nota, igual byte a byte. Na contingência offline sem protocolo, saem a legenda "EMITIDA EM CONTINGÊNCIA" e "Pendente de autorização", nos dois lugares que o manual pede, e a via.

`Danfce` lança `ArgumentException` (a mensagem cita o campo, nunca o valor) quando a leitura não tem a nota, a nota não é NFC-e, falta a chave ou o QR Code, o protocolo não é de autorização (100 ou 150), ou o papel não é uma bobina.

## O DANFE

```csharp
DocumentoImpressao danfe = leiaute.Danfe(nota, new OpcoesDanfe
{
    Logotipo = logotipo,
    TextoRodape = "Impresso pelo MeuERP",
});
```

| Opção (`OpcoesDanfe`) | Padrão | Descrição |
|---|---|---|
| `Orientacao` | pelo `tpImp` da nota | `OrientacaoDanfe.Retrato` ou `Paisagem`, por cima do `tpImp` |
| `Canhoto` | `true` | O canhoto; só o retrato o suprime |
| `Logotipo` | nenhum | O logotipo, no quadro do emitente |
| `Cancelada` | `false` | A marca d'água "CANCELADA". A situação é você quem informa |
| `TextoRodape` | nenhum | Uma linha livre ao fim |
| `Papel`, `Via` | | Só do DANFE Simplificado Tipo 2 (`tpImp` 6): a bobina ou o A4 retrato, e a via na contingência |

O `tpImp` da nota escolhe o documento:

| `tpImp` (`FormatoImpressao`) | Documento |
|---|---|
| 1 (`DanfeRetrato`), 2 (`DanfePaisagem`) | O DANFE A4 do Anexo II do MOC 7.0, com os quadros do IBS, da CBS e do IS quando a nota os tem |
| 6 (`DanfeSimplificadoTipo2`) | O DANFE Simplificado Tipo 2 (NT 2026.003), no leiaute do DANFCE, com o QR Code |
| 3 (simplificado), 0, 4, 5 | Não suportados: `ArgumentException` |

O DANFE paginado repete o cabeçalho da tabela de itens e o cabeçalho de continuação em cada folha, e numera as folhas (`FOLHA 1/2`).

## Imagens de 1 bit

O logotipo e a assinatura do software são `Imagem1Bit`: a impressora térmica só imprime preto. Converta uma imagem em tons de cinza com `Limiarizar`:

```csharp
// cinza: um byte por pixel (0 = preto, 255 = branco), linha a linha.
Imagem1Bit logotipo = Imagem1Bit.Limiarizar(cinza, largura, altura, limiar: 128);
```

Os bits ficam com o mais significativo primeiro, cada linha em bytes inteiros, 1 = preto: o formato do comando raster das térmicas.

## Os elementos

`DocumentoImpressao` tem o `Papel`, os `Elementos` de cima para baixo e, no DANFE, o `CabecalhoContinuacao` e a `MarcaAgua`. Os elementos formam um conjunto fechado:

| Elemento | Conteúdo |
|---|---|
| `TextoImpressao` | Um texto, com o alinhamento, o negrito, o tamanho (`Pequeno`, `Normal`, `Duplo`) e o invertido. `Quebrar(colunas)` quebra nas colunas do papel |
| `LinhaImpressao` | Uma linha de células com peso. `Larguras(colunas)` reparte as colunas entre as células |
| `SeparadorImpressao` | Uma linha tracejada ou cheia |
| `EspacoImpressao` | Linhas em branco |
| `QrCodeImpressao` | O texto do QR Code e o lado em milímetros |
| `CodigoBarrasImpressao` | Um código de barras Code 128 C (só dígitos) |
| `ImagemImpressao` | Uma `Imagem1Bit` |
| `SecaoImpressao` | Um quadro com campos (rótulo e valor), no DANFE |
| `TabelaImpressao` | Uma tabela com colunas e linhas, no DANFE |
| `QuebraPaginaImpressao` | O fim de uma página (ou, na bobina, o corte) |

| Papel | Largura | Colunas |
|---|---|---|
| `PapelImpressao.Bobina80` | 80 mm | 48 |
| `PapelImpressao.Bobina58` | 58 mm | 32 |
| `PapelImpressao.A4Retrato`, `A4Paisagem` | 210 mm, 297 mm | |

Tudo é imutável e seguro para uso concorrente. A lista completa de membros está na [referência do namespace OpenDFe.Impressao](../../referencia/opendfe-impressao.md).

## Conformidade com os manuais

A conferência, exigência por exigência, contra o Manual do DANFE NFC-e 6.0 e o Anexo II do MOC 7.0 está em [IMPRESSAO.md](../../IMPRESSAO.md), com as lacunas conhecidas.

## Prompt para agentes de IA

Peça a um agente que gere o DANFE ou o DANFCE. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Gere o <DANFE / DANFCE> de uma nota autorizada com a OpenDFe, seguindo docs/guia/impressao/modelo-impressao.md:
LeiauteNFe400.Ler(nfeProc) e LeiauteNFe400.Danfe ou Danfce, com <logotipo convertido por Imagem1Bit.Limiarizar, texto de rodapé,
bobina de 80/58 mm, via, marca de cancelada>. A situação de cancelada é minha (OpcoesDanfe.Cancelada). Trate a ArgumentException
(nota que não é do modelo, sem protocolo de autorização, tpImp não suportado) e não invente opções que não estão em OpcoesDanfe/OpcoesDanfce.
```

## Próximas etapas

- [Gerar o PDF](pdf.md)
- [Imprimir em impressora térmica (ESC/POS)](escpos.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
