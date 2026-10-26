# Namespace OpenDFe.Impressao

O modelo de impressão neutro: o documento, o papel e os elementos que o DANFE e o DANFCE usam. Pacote NuGet: `OpenDFe.Nucleo`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`AlinhamentoImpressao`](#alinhamentoimpressao) | O alinhamento do texto na largura disponível. |
| [`CampoImpressao`](#campoimpressao) | Um campo rotulado de uma [`LinhaCamposImpressao`](#linhacamposimpressao) (a grade do DANFE). |
| [`CelulaImpressao`](#celulaimpressao) | Uma célula de uma [`LinhaImpressao`](#linhaimpressao): o texto e o peso da largura. |
| [`CodigoBarrasImpressao`](#codigobarrasimpressao) | O código de barras Code 128, conjunto C (pares de dígitos): a chave de acesso de 44 dígitos no DANFE. |
| [`ColunaImpressao`](#colunaimpressao) | Uma coluna de uma [`TabelaImpressao`](#tabelaimpressao): o título, o peso da largura e o alinhamento. |
| [`DocumentoImpressao`](#documentoimpressao) | O documento de impressão: o papel e a lista de elementos, de cima para baixo. |
| [`ElementoImpressao`](#elementoimpressao) | Um elemento do documento de impressão: o texto, a linha de células, o separador, o espaço, o QR Code, o código de barras, a imagem, a seção, a tabela e a quebra de página. |
| [`EspacoImpressao`](#espacoimpressao) | Um espaço em branco de algumas linhas de texto. |
| [`EstiloSeparador`](#estiloseparador) | O estilo do separador. |
| [`Imagem1Bit`](#imagem1bit) | A imagem de 1 bit por pixel (o logotipo do emitente e a assinatura do software emissor): a impressora térmica só imprime preto. |
| [`ImagemImpressao`](#imagemimpressao) | Uma imagem de 1 bit no documento. |
| [`LinhaCamposImpressao`](#linhacamposimpressao) | Uma linha de campos rotulados, lado a lado, com larguras proporcionais aos pesos. |
| [`LinhaImpressao`](#linhaimpressao) | Uma linha de células com larguras proporcionais (os itens da NFC-e: código, descrição, quantidade, valores). |
| [`PapelImpressao`](#papelimpressao) | O papel do documento de impressão. As instâncias são as fábricas estáticas: não há como criar outro papel. |
| [`QrCodeImpressao`](#qrcodeimpressao) | O QR Code. O renderizador o desenha com a correção de erro nível M e o texto em UTF-8 (o Manual de Padrões Técnicos do DANFE NFC-e e QR Code, item 4.5). |
| [`QuebraPaginaImpressao`](#quebrapaginaimpressao) | A quebra de página (só na folha; na bobina não tem efeito). |
| [`SecaoImpressao`](#secaoimpressao) | Uma seção com título e linhas de campos rotulados, em grade (o DANFE, etapa 7.3). |
| [`SeparadorImpressao`](#separadorimpressao) | Uma linha horizontal na largura do papel que separa as partes do documento. |
| [`TabelaImpressao`](#tabelaimpressao) | Uma tabela com o cabeçalho repetido a cada página (os itens do DANFE, etapa 7.3). |
| [`TamanhoTexto`](#tamanhotexto) | O tamanho do texto. |
| [`TextoImpressao`](#textoimpressao) | Um texto, que o renderizador quebra na largura do papel. |
| [`TextoPaginado`](#textopaginado) | As fichas de paginação que o texto de um elemento pode levar (`FOLHA {pagina}/{paginas}` no DANFE) e a troca delas pelos números de cada página. |
| [`TipoPapel`](#tipopapel) | O tipo do papel: a bobina (altura contínua) ou a folha (altura fixa, várias páginas). |

## AlinhamentoImpressao

Enumeração · Namespace `OpenDFe.Impressao`

O alinhamento do texto na largura disponível.

| Membro | Valor | Descrição |
|---|---|---|
| `Esquerda` | 0 | À esquerda. |
| `Centro` | 1 | Centralizado. |
| `Direita` | 2 | À direita. |

## CampoImpressao

Classe selada · Namespace `OpenDFe.Impressao`

Um campo rotulado de uma [`LinhaCamposImpressao`](#linhacamposimpressao) (a grade do DANFE).

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `CampoImpressao(string rotulo, string valor, double peso = 1)` | Cria o campo. **`ArgumentNullException`:** O rótulo ou o valor é nulo. **`ArgumentOutOfRangeException`:** O peso não é um número maior que zero. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Rotulo` | `string` | get | O rótulo. |
| `Valor` | `string` | get | O valor. |
| `Peso` | `double` | get | O peso da largura em relação aos outros campos da linha. |
| `Alinhamento` | `AlinhamentoImpressao` | get; init | O alinhamento do valor; à esquerda por padrão. |
| `Negrito` | `bool` | get; init | Se o valor está em negrito. |

## CelulaImpressao

Classe selada · Namespace `OpenDFe.Impressao`

Uma célula de uma [`LinhaImpressao`](#linhaimpressao): o texto e o peso da largura.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `CelulaImpressao(string texto, double peso = 1)` | Cria a célula. **`ArgumentNullException`:** O texto é nulo. **`ArgumentOutOfRangeException`:** O peso não é um número maior que zero. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Texto` | `string` | get | O texto. |
| `Peso` | `double` | get | O peso da largura em relação às outras células da linha. |
| `Alinhamento` | `AlinhamentoImpressao` | get; init | O alinhamento do texto dentro da célula; à esquerda por padrão. |
| `Negrito` | `bool` | get; init | Se o texto está em negrito. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Quebrar(int colunas)` | `IReadOnlyList<string>` | Quebra o texto da célula como [`Quebrar`](#textoimpressao). **Retorna:** As linhas do texto na largura. **`ArgumentOutOfRangeException`:** As colunas são menores que 1. |

## CodigoBarrasImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

O código de barras Code 128, conjunto C (pares de dígitos): a chave de acesso de 44 dígitos no DANFE.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `CodigoBarrasImpressao(string digitos)` | Cria o código de barras. **`ArgumentNullException`:** Os dígitos são nulos. **`ArgumentException`:** Vazio, em quantidade ímpar ou com caractere que não é dígito de 0 a 9. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Digitos` | `string` | get | Os dígitos. |
| `AlturaMm` | `double` | get; init | A altura das barras, em milímetros; 10 por padrão. |

## ColunaImpressao

Classe selada · Namespace `OpenDFe.Impressao`

Uma coluna de uma [`TabelaImpressao`](#tabelaimpressao): o título, o peso da largura e o alinhamento.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `ColunaImpressao(string titulo, double peso, AlinhamentoImpressao alinhamento = AlinhamentoImpressao.Esquerda)` | Cria a coluna. **`ArgumentNullException`:** O título é nulo. **`ArgumentOutOfRangeException`:** O peso não é um número maior que zero. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Titulo` | `string` | get | O título do cabeçalho. |
| `Peso` | `double` | get | O peso da largura em relação às outras colunas. |
| `Alinhamento` | `AlinhamentoImpressao` | get | O alinhamento das células da coluna. |

## DocumentoImpressao

Classe selada · Namespace `OpenDFe.Impressao`

O documento de impressão: o papel e a lista de elementos, de cima para baixo.

Imutável: a lista passada é copiada. O modelo é neutro (não conhece NF-e) e estrutural, sem fonte nem coordenada: quem monta o documento formata os textos (data, dinheiro, CNPJ) e quem renderiza só desenha (docs/CONTRATOS.md 20). O [`CabecalhoContinuacao`](#documentoimpressao) e a [`MarcaAgua`](#documentoimpressao) (docs/CONTRATOS.md 21) são opcionais para quem renderiza: um renderizador que os ignora ainda gera um documento válido.

### Construtores

| Construtor | Descrição |
|---|---|
| `DocumentoImpressao(PapelImpressao papel, IReadOnlyList<ElementoImpressao> elementos)` | Cria o documento. **`ArgumentNullException`:** O papel, a lista ou um elemento é nulo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Papel` | `PapelImpressao` | get | O papel. |
| `Elementos` | `IReadOnlyList<ElementoImpressao>` | get | Os elementos, de cima para baixo. |
| `Titulo` | `string?` | get; init | Metadado: o título do documento (o título do PDF); `null` sem título. |
| `CabecalhoContinuacao` | `IReadOnlyList<ElementoImpressao>` | get; init | O que se repete no topo das páginas seguintes à primeira, depois do corte do documento em páginas (por exemplo, no DANFE, o quadro do emitente resumido e a chave, Anexo II 3.5); vazio quando as páginas seguintes não repetem nada. A lista é copiada. Os textos podem levar as fichas de [`TextoPaginado`](#textopaginado) (`FOLHA 2/3`). |
| `MarcaAgua` | `string?` | get; init | O texto em diagonal, em todas as páginas, atrás do conteúdo (`CANCELADA`, `SEM VALOR FISCAL`), desde que não prejudique a legibilidade; `null` sem marca. |

## ElementoImpressao

Classe abstrata · Namespace `OpenDFe.Impressao`

Um elemento do documento de impressão: o texto, a linha de células, o separador, o espaço, o QR Code, o código de barras, a imagem, a seção, a tabela e a quebra de página.

O conjunto é fechado: só a biblioteca deriva (o construtor é `private protected`), então quem renderiza trata todos os tipos conhecidos. Os elementos são imutáveis e seguros em paralelo.

## EspacoImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Um espaço em branco de algumas linhas de texto.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `EspacoImpressao(int linhas)` | Cria o espaço. **`ArgumentOutOfRangeException`:** As linhas são menores que 1. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Linhas` | `int` | get | A altura em linhas de texto normal. |

## EstiloSeparador

Enumeração · Namespace `OpenDFe.Impressao`

O estilo do separador.

| Membro | Valor | Descrição |
|---|---|---|
| `Tracejado` | 0 | A linha tracejada. |
| `Cheio` | 1 | A linha cheia. |

## Imagem1Bit

Classe selada · Namespace `OpenDFe.Impressao`

A imagem de 1 bit por pixel (o logotipo do emitente e a assinatura do software emissor): a impressora térmica só imprime preto.

Imutável: os bits passados são copiados. Os bits são empacotados com o primeiro pixel da linha no bit mais significativo do byte, cada linha em bytes inteiros (os bits que sobram no fim da linha são ignorados), e 1 é preto.

### Construtores

| Construtor | Descrição |
|---|---|
| `Imagem1Bit(int largura, int altura, ReadOnlyMemory<byte> bits)` | Cria a imagem. **`ArgumentOutOfRangeException`:** A largura ou a altura é menor que 1. **`ArgumentException`:** O tamanho dos bits não é o da imagem. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Largura` | `int` | get | A largura em pixels. |
| `Altura` | `int` | get | A altura em pixels. |
| `Bits` | `ReadOnlyMemory<byte>` | get | Os bits empacotados (o primeiro pixel no bit mais significativo; 1 é preto). |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Limiarizar(ReadOnlySpan<byte> cinza, int largura, int altura, byte limiar = 128)` | `Imagem1Bit` | Converte uma imagem de 8 bits por pixel (tons de cinza) em 1 bit: o pixel mais escuro que o limiar fica preto. **Retorna:** A imagem de 1 bit. **`ArgumentOutOfRangeException`:** A largura ou a altura é menor que 1. **`ArgumentException`:** O tamanho dos pixels não é o da imagem. |
| `Preto(int x, int y)` | `bool` | Se o pixel é preto. **Retorna:** `true` se o bit é 1. **`ArgumentOutOfRangeException`:** O pixel está fora da imagem. |

## ImagemImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Uma imagem de 1 bit no documento.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `ImagemImpressao(Imagem1Bit imagem)` | Cria o elemento. **`ArgumentNullException`:** A imagem é nula. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Imagem` | `Imagem1Bit` | get | A imagem. |
| `Alinhamento` | `AlinhamentoImpressao` | get; init | O alinhamento; centralizado por padrão. |
| `LarguraMm` | `double?` | get; init | A largura impressa, em milímetros; `null` é o tamanho natural a 203 dpi. |

## LinhaCamposImpressao

Classe selada · Namespace `OpenDFe.Impressao`

Uma linha de campos rotulados, lado a lado, com larguras proporcionais aos pesos.

Imutável: a lista passada é copiada.

### Construtores

| Construtor | Descrição |
|---|---|
| `LinhaCamposImpressao(IReadOnlyList<CampoImpressao> campos)` | Cria a linha. **`ArgumentNullException`:** A lista ou um campo é nulo. **`ArgumentException`:** A lista é vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Campos` | `IReadOnlyList<CampoImpressao>` | get | Os campos, da esquerda para a direita. |

## LinhaImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Uma linha de células com larguras proporcionais (os itens da NFC-e: código, descrição, quantidade, valores).

Imutável: a lista passada é copiada.

### Construtores

| Construtor | Descrição |
|---|---|
| `LinhaImpressao(IReadOnlyList<CelulaImpressao> celulas)` | Cria a linha. **`ArgumentNullException`:** A lista ou uma célula é nula. **`ArgumentException`:** A lista é vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Celulas` | `IReadOnlyList<CelulaImpressao>` | get | As células, da esquerda para a direita. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Larguras(int colunas)` | `IReadOnlyList<int>` | As larguras inteiras de cada célula, em colunas, que somam exatamente `colunas` (o método do maior resto: cada célula recebe a parte inteira da sua cota e as colunas que sobram vão às maiores frações, a primeira célula nos empates). Com `colunas` maior ou igual ao número de células, nenhuma fica com largura zero (a coluna que falta sai da célula mais larga); com menos, as de menor peso podem ficar com zero. **Retorna:** As larguras, uma por célula, na ordem das células. **`ArgumentOutOfRangeException`:** As colunas são menores que 1. |

## PapelImpressao

Classe selada · Namespace `OpenDFe.Impressao`

O papel do documento de impressão. As instâncias são as fábricas estáticas: não há como criar outro papel.

Imutável. Na bobina, as colunas são as de caracteres da fonte A da impressora térmica (12 pontos de largura a 203 dpi): 48 em 80 mm e 32 em 58 mm.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `static Bobina80` | `PapelImpressao` | get | A bobina de 80 mm: 48 colunas de caracteres, margem lateral de 4 mm (72 mm de área impressa). |
| `static Bobina58` | `PapelImpressao` | get | A bobina de 58 mm: 32 colunas de caracteres, margem lateral de 5 mm (48 mm de área impressa). |
| `static A4Retrato` | `PapelImpressao` | get | A folha A4 em pé, 210 x 297 mm, com margem de 8 mm (o máximo que o Anexo II 3.6.2 permite ao DANFE; o mínimo é 2 mm). |
| `static A4Paisagem` | `PapelImpressao` | get | A folha A4 deitada, 297 x 210 mm, com margem de 8 mm (o máximo que o Anexo II 3.6.2 permite ao DANFE; o mínimo é 2 mm). |
| `Tipo` | `TipoPapel` | get | Se é bobina ou folha. |
| `LarguraMm` | `double` | get | A largura do papel, em milímetros. |
| `AlturaMm` | `double?` | get | A altura do papel, em milímetros; `null` na bobina. |
| `Colunas` | `int` | get | Na bobina, as colunas de caracteres da fonte normal; na folha, 0. |
| `MargemMm` | `double` | get | A margem lateral, em milímetros. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `ColunasTamanho(TamanhoTexto tamanho)` | `int` | As colunas de caracteres que cabem numa linha no tamanho do texto: a fonte normal tem [`Colunas`](#papelimpressao); a pequena (fonte B, 9 pontos de largura) tem 4/3 delas, arredondado para baixo; a dupla (largura dobrada) tem a metade. **Retorna:** As colunas; 0 na folha, que não tem colunas. **`ArgumentOutOfRangeException`:** O tamanho não é um dos definidos. |

## QrCodeImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

O QR Code. O renderizador o desenha com a correção de erro nível M e o texto em UTF-8 (o Manual de Padrões Técnicos do DANFE NFC-e e QR Code, item 4.5).

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `QrCodeImpressao(string texto)` | Cria o QR Code. **`ArgumentNullException`:** O texto é nulo. **`ArgumentException`:** O texto é vazio. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Texto` | `string` | get | O texto codificado. |
| `LadoMm` | `double` | get; init | O lado do quadrado, em milímetros, com a margem de silêncio dentro dele (o manual da NFC-e pede no mínimo 25 mm, dos quais 22 mm de conteúdo e 3 mm de margem); 25 por padrão. |
| `Alinhamento` | `AlinhamentoImpressao` | get; init | O alinhamento; centralizado por padrão. |

## QuebraPaginaImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

A quebra de página (só na folha; na bobina não tem efeito).

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `QuebraPaginaImpressao()` |  |

## SecaoImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Uma seção com título e linhas de campos rotulados, em grade (o DANFE, etapa 7.3).

Imutável: a lista passada é copiada.

### Construtores

| Construtor | Descrição |
|---|---|
| `SecaoImpressao(string? titulo, IReadOnlyList<LinhaCamposImpressao> linhas)` | Cria a seção. **`ArgumentNullException`:** A lista ou uma linha é nula. **`ArgumentException`:** A lista é vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Titulo` | `string?` | get | O título; `null` na seção sem título. |
| `Linhas` | `IReadOnlyList<LinhaCamposImpressao>` | get | As linhas de campos, de cima para baixo. |

## SeparadorImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Uma linha horizontal na largura do papel que separa as partes do documento.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `SeparadorImpressao()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Estilo` | `EstiloSeparador` | get; init | O estilo; tracejado por padrão. |

## TabelaImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Uma tabela com o cabeçalho repetido a cada página (os itens do DANFE, etapa 7.3).

Imutável: as listas passadas são copiadas, linha por linha.

### Construtores

| Construtor | Descrição |
|---|---|
| `TabelaImpressao(string? titulo, IReadOnlyList<ColunaImpressao> colunas, IReadOnlyList<IReadOnlyList<string>> linhas)` | Cria a tabela. **`ArgumentNullException`:** Uma lista, uma coluna, uma linha ou um texto é nulo. **`ArgumentException`:** Não há colunas, ou uma linha não tem um texto por coluna. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Titulo` | `string?` | get | O título; `null` sem título. |
| `Colunas` | `IReadOnlyList<ColunaImpressao>` | get | As colunas. |
| `Linhas` | `IReadOnlyList<IReadOnlyList<string>>` | get | As linhas, cada uma com um texto por coluna. |
| `LinhasIndivisiveis` | `bool` | get; init | Se uma linha da tabela nunca é dividida entre duas páginas (o item do DANFE com a descrição e as informações adicionais em várias linhas, Anexo II 3.1.7); falso por padrão. A quebra de linha (o caractere de nova linha) no texto de uma célula é uma quebra de linha dentro da célula. |
| `DivisorLinhas` | `bool` | get; init | Se há um divisor entre uma linha e a seguinte (o "destaque divisório" que distingue um item do outro, Anexo II 3.1.7: linha, espaço duplo ou sombreamento, a escolha é de quem renderiza); falso por padrão. |

## TamanhoTexto

Enumeração · Namespace `OpenDFe.Impressao`

O tamanho do texto.

| Membro | Valor | Descrição |
|---|---|---|
| `Pequeno` | 0 | Menor que o normal (a fonte B da térmica). |
| `Normal` | 1 | O normal (a fonte A da térmica). |
| `Duplo` | 2 | O dobro do normal em largura e altura. |

## TextoImpressao

Classe selada · Namespace `OpenDFe.Impressao` · Deriva de [`ElementoImpressao`](#elementoimpressao)

Um texto, que o renderizador quebra na largura do papel.

Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `TextoImpressao(string texto)` | Cria o texto. **`ArgumentNullException`:** O texto é nulo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Texto` | `string` | get | O texto (`\n` é quebra explícita). |
| `Alinhamento` | `AlinhamentoImpressao` | get; init | O alinhamento; à esquerda por padrão. |
| `Negrito` | `bool` | get; init | Se está em negrito. |
| `Tamanho` | `TamanhoTexto` | get; init | O tamanho; [`Normal`](#tamanhotexto) por padrão. |
| `Invertido` | `bool` | get; init | Se é texto claro em fundo preto (o destaque de um título). |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Quebrar(int colunas)` | `IReadOnlyList<string>` | Quebra o texto em linhas de no máximo `colunas` caracteres, em espaço, sem cortar a palavra que cabe numa linha; a palavra maior que a linha é cortada na largura. `\n` (e `\r\n`) quebra a linha, e a linha vazia é mantida. Os espaços do fim de cada linha e do começo das linhas de continuação são descartados. **Retorna:** As linhas, sem `\n`; ao menos uma (vazia, para o texto vazio). **`ArgumentOutOfRangeException`:** As colunas são menores que 1. |

## TextoPaginado

Classe estática · Namespace `OpenDFe.Impressao`

As fichas de paginação que o texto de um elemento pode levar (`FOLHA {pagina}/{paginas}` no DANFE) e a troca delas pelos números de cada página.

O modelo é estrutural e não sabe em quantas páginas o documento será cortado: quem monta o documento escreve a ficha [`Pagina`](#textopaginado) onde vai o número da página e [`Paginas`](#textopaginado) onde vai o total, e quem renderiza chama [`Resolver`](#textopaginado) com os números de cada página, depois de cortar o documento. Vale para o texto de um [`TextoImpressao`](#textoimpressao), o valor de um [`CampoImpressao`](#campoimpressao) e o texto de uma célula, de uma linha ou de uma tabela, incluindo o [`CabecalhoContinuacao`](#documentoimpressao). Um renderizador que não pagina deixa o texto como está ou resolve com a página 1 de 1. Sem estado, seguro em paralelo (docs/CONTRATOS.md 21).

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `Pagina` | `string` | {pagina} | A ficha do número da página: `{pagina}`. |
| `Paginas` | `string` | {paginas} | A ficha do total de páginas: `{paginas}`. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Resolver(string texto, int pagina, int paginas)` | `string` | Troca as fichas pelo número da página e pelo total, em algarismos sem zeros à esquerda e sem cultura. **Retorna:** O texto com cada ficha trocada; o texto sem fichas volta igual. **`ArgumentNullException`:** O texto é nulo. **`ArgumentOutOfRangeException`:** A página é menor que 1, ou o total é menor que a página. |

## TipoPapel

Enumeração · Namespace `OpenDFe.Impressao`

O tipo do papel: a bobina (altura contínua) ou a folha (altura fixa, várias páginas).

| Membro | Valor | Descrição |
|---|---|---|
| `Bobina` | 0 | A bobina da impressora térmica: a largura é fixa e a altura acompanha o conteúdo. |
| `Folha` | 1 | A folha, de largura e altura fixas, com quebra em páginas. |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
