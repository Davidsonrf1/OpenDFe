# Imprimir em impressora térmica (ESC/POS)

[Documentação](../../index.md) › Impressão › ESC/POS

O PDV imprime o DANFCE na impressora térmica por comandos ESC/POS, a partir do mesmo [modelo de impressão](modelo-impressao.md) que gera o PDF. A OpenDFe **não implementa** o ESC/POS: quem conhece a marca e o modelo da impressora é o aplicativo. Este guia mostra como percorrer o modelo e quais comandos usar para cada elemento.

Quem imprime por ESC/POS não precisa do pacote `OpenDFe.Impressao.Pdf` nem do QuestPDF.

> **Observação**
> Os comandos abaixo são os do conjunto ESC/POS comum (Epson TM, Bematech, Elgin, Daruma e compatíveis). Confira no manual da sua impressora.

## O papel e as colunas

`papel.Colunas` dá a largura da linha em caracteres na fonte normal: 48 na bobina de 80 mm e 32 na de 58 mm (fonte A, 12 × 24 pontos a 203 dpi). `papel.ColunasTamanho(tamanho)` dá a largura em cada tamanho: `Pequeno` é `Colunas * 4 / 3` (a fonte B) e `Duplo` é `Colunas / 2`. A área impressa é de 576 pontos (72 mm) na bobina de 80 mm e 384 pontos (48 mm) na de 58 mm.

## Cada elemento

| Elemento | Em ESC/POS |
|---|---|
| `TextoImpressao` | Quebre com `texto.Quebrar(papel.ColunasTamanho(texto.Tamanho))` e imprima uma linha por vez, com o alinhamento (`ESC a 0/1/2`), o negrito (`ESC E 1` / `ESC E 0`), o tamanho (`Pequeno`: `ESC M 1`; `Normal`: `ESC M 0` e `GS ! 0x00`; `Duplo`: `GS ! 0x11`) e o invertido (`GS B 1` / `GS B 0`). Termine a linha com `LF` |
| `LinhaImpressao` | `linha.Larguras(papel.Colunas)` reparte as colunas (a soma é sempre `Colunas`). Quebre cada célula com `celula.Quebrar(larguras[i])`; a linha ocupa tantas linhas de impressão quanto a célula mais alta. Em cada uma, preencha as células com espaços até a largura, no alinhamento de cada célula, e junte numa só linha |
| `SeparadorImpressao` | Uma linha de `Colunas` caracteres: `-` no `Tracejado`, `=` no `Cheio` |
| `EspacoImpressao` | `Linhas` linhas em branco (`LF`) |
| `QrCodeImpressao` | O comando nativo `GS ( k`: o modelo 2 (`fn 165`), o tamanho do módulo (`fn 167`), o nível de correção M (`fn 169`, valor 49), os dados (`fn 180`) e a impressão (`fn 181`). O `LadoMm` inclui a margem de silêncio: o símbolo ocupa cerca de 80% dele. Sem o comando nativo, gere a matriz com um gerador de QR Code (nível M, UTF-8, o mesmo texto) e imprima como raster |
| `CodigoBarrasImpressao` | O comando nativo `GS k 73` (Code 128) com o conjunto C (`{C` e os pares de dígitos), `GS h` para a altura (`AlturaMm * 8` pontos), `GS w` para o módulo e `GS H 0` sem o texto; ou raster |
| `ImagemImpressao` | Raster: `GS v 0`, com a largura em bytes (`(Largura + 7) / 8`) e a altura. Os `Bits` da `Imagem1Bit` (o mais significativo primeiro, 1 = preto, cada linha em bytes inteiros) já estão no formato do comando. Centralize com `ESC a 1` |
| `QuebraPaginaImpressao` | O corte: `GS V 66 0` (avança o papel e corta). O que vem depois é outro bilhete (a outra via) |
| `SecaoImpressao`, `TabelaImpressao` | São do DANFE em A4; o DANFCE não os usa |

## Os caracteres

A impressora térmica não é Unicode. Escolha a página de código **850** (`ESC t 2`, ou a 860, conforme a impressora) e converta o texto antes de enviar, com o `Encoding.GetEncoding(850)` do pacote `System.Text.Encoding.CodePages`. Troque antes o que a página não tem (o travessão e as aspas tipográficas, por exemplo).

```csharp
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
var cp850 = System.Text.Encoding.GetEncoding(850);
```

## O percurso

O esqueleto abaixo percorre o DANFCE. `impressora` e `Preencher` são do seu aplicativo: a primeira escreve os comandos na porta serial, USB ou de rede da impressora; a segunda junta, numa linha de `papel.Colunas` caracteres, a linha `y` de cada célula, preenchida e alinhada na largura que `Larguras` deu.

```csharp
using OpenDFe.Impressao;
using OpenDFe.NFe;

DocumentoImpressao danfce = new LeiauteNFe400().Danfce(lida, new OpcoesDanfce { Papel = PapelImpressao.Bobina80 });
var papel = danfce.Papel;

foreach (var elemento in danfce.Elementos)
{
    switch (elemento)
    {
        case TextoImpressao texto:
            foreach (var linha in texto.Quebrar(papel.ColunasTamanho(texto.Tamanho)))
                impressora.Linha(linha, texto.Alinhamento, texto.Negrito, texto.Tamanho, texto.Invertido);
            break;
        case LinhaImpressao linha:
            var larguras = linha.Larguras(papel.Colunas);
            var blocos = linha.Celulas.Select((c, i) => c.Quebrar(larguras[i])).ToList();
            for (var y = 0; y < blocos.Max(b => b.Count); y++)
                impressora.Linha(Preencher(linha, blocos, larguras, y), AlinhamentoImpressao.Esquerda, linha.Celulas.Any(c => c.Negrito));
            break;
        case SeparadorImpressao separador:
            impressora.Linha(new string(separador.Estilo == EstiloSeparador.Cheio ? '=' : '-', papel.Colunas));
            break;
        case EspacoImpressao espaco:
            impressora.LinhasEmBranco(espaco.Linhas);
            break;
        case QrCodeImpressao qr:
            impressora.QrCode(qr.Texto, qr.LadoMm, qr.Alinhamento);   // GS ( k, ou raster
            break;
        case ImagemImpressao imagem:
            impressora.Raster(imagem.Imagem.Largura, imagem.Imagem.Altura, imagem.Imagem.Bits.Span, imagem.Alinhamento);
            break;
        case QuebraPaginaImpressao:
            impressora.Cortar();                                       // GS V 66 0
            break;
    }
}
impressora.Cortar();
```

## As vias

A biblioteca não decide quantas vias imprimir. Na contingência offline, chame `Danfce` uma vez por via (`OpcoesDanfce.Via`) e imprima cada uma, com o corte entre elas.

## Próximas etapas

- [Modelo de impressão](modelo-impressao.md)
- [Contingência offline](../nfce/contingencia-offline.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
