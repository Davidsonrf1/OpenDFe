# Impressão: o documento de conformidade da fase 7

O que a lib imprime e **contra qual exigência**. A fase 7 começa pelo modelo de impressão neutro (núcleo, `OpenDFe.Impressao`, `docs/CONTRATOS.md` 20) e pelo DANFCE (etapa 7.2); o DANFE (7.3) e o PDF (7.4) completam este arquivo com as suas seções.

Decisões e fontes:

- **A fonte** é o **Manual de Padrões Técnicos do DANFE NFC-e e QR Code, versão 6.0 (março de 2025)**, do Portal Nacional da NF-e, SHA-256 `bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e` (`docs/NTS.md` 5; baixado de novo na 7.2, o mesmo arquivo, lido inteiro, com as figuras dos exemplos 1, 2 e 3). Onde o arquivo da etapa e o manual divergem, **vale o manual**, e a divergência está na coluna "Onde".
- O modelo é **estrutural**: o DANFCE sai como `DocumentoImpressao` (texto, linhas de células, separador, QR Code, imagem), sem fonte nem posição em pixel; quem renderiza (o PDV em ESC/POS, o PDF da 7.4) escolhe. O manual diz que as **posições** da divisão II não são reguladas (3.1.2) e dá a divisão do documento nas figuras 1A e 1B; a lib segue a ordem da figura 1B (QR Code centralizado, a que serve às bobinas de 58 e de 80 mm).
- **O que o DANFCE imprime vem só do XML** (manual, capítulo 2): nada entra no DANFCE que não conste do XML da NFC-e, a não ser as informações do retorno da autorização (o protocolo e a data da autorização). As exceções são os textos fixos do próprio manual e a via (`OpcoesDanfce`).
- **A lib não decide** quantas vias imprimir, nem se imprime resumido (3.1.2, "a critério da UF"): a escolha da via é de `OpcoesDanfce.Via`, e o DANFCE sai sempre completo.
- `Atende`: sim; `Facultativo`: o manual deixa a critério do emissor ou da UF e a lib **não** imprime; `Fora`: não é do DANFCE da lib, com o motivo.

## 1. DANFCE

A montagem está em `LeiauteNFe400.Danfce` (`src/OpenDFe.Leiaute.NFe.v400/Impressao/`). A coluna "Onde" cita o método de `MontagemDanfce` que cumpre a exigência e o teste que a prova (`NFe.Testes/Impressao/`).

| Item do manual | Exigência (resumida) | Obrigatória? | Situação | Onde |
|---|---|---|---|---|
| 2 | O DANFE NFC-e é impresso pelo aplicativo do contribuinte, em impressora comum, com base no XML da NFC-e | Sim | Atende | `Danfce(NotaLida)` lê a `NotaLida` (o XML lido), e o `DocumentoImpressao` é renderizado pelo aplicativo |
| 2 | Não inserir informação que não conste do XML, exceto as do retorno da autorização (protocolo, `cMsg`, `xMsg`) | Sim | Atende | só campos da nota e do `Protocolo`; o teste `NadaVemDeForaDaNotaOuDasOpcoes` |
| 2 | DANFE NFC-e resumido (sem os itens), se a UF permitir e o consumidor pedir | Não (UF) | Facultativo | a lib imprime sempre o completo; o resumido é decisão da UF e do consumidor (a lib não decide) |
| 2 | Envio eletrônico no lugar da impressão, a critério da UF | Não (UF) | Fora | o aplicativo decide; o `DocumentoImpressao` serve ao PDF |
| 2 | Legibilidade e durabilidade do papel por 6 meses | Sim | Fora | do papel e da impressora, não do documento |
| 3.1.1 (I) | CNPJ do emitente, com a máscara `99.999.999/9999-99` (C02) | Sim | Atende | `Cabecalho`; `Cnpj` do `FormatadorImpressao` (a máscara vale para o CNPJ alfanumérico) |
| 3.1.1 (I) | **CPF** do emitente, com a máscara `999.999.999-99` (C02a), março de 2025 | Sim | Atende | `Cabecalho`; `Cpf`; teste do emitente com CPF |
| 3.1.1 (I) | Razão social ou nome do emitente (C03) | Sim | Atende | `Cabecalho` (em negrito, como a figura) |
| 3.1.1 (I) | Endereço completo do emitente, sem a indicação do país | Sim | Atende | `Cabecalho` (logradouro, número, complemento, bairro, município, UF e CEP; sem o país) |
| 3.1.1 (I) | O texto "Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica" | Sim | Atende | `Cabecalho` (o texto do manual; o "Não permite aproveitamento de crédito de ICMS" do arquivo da etapa **não** está no manual 6.0 e não entra) |
| 3.1.1 (I) | Logotipo da empresa ou da NFC-e, no canto esquerdo da divisão | Não | Atende (ajuste) | `OpcoesDanfce.Logotipo`, centralizado acima do cabeçalho: o modelo não compõe imagem ao lado de texto (`CONTRATOS.md` 20, ajustes) |
| 3.1.1 (I) | Nome fantasia do emitente | Não (o manual não pede) | Facultativo | não impresso |
| 3.1.2 (II) | Detalhe dos itens: código (I02), descrição (I04), quantidade (I10), unidade (I09), valor unitário (I10a), valor total (I11) | Sim, se há divisão II | Atende | `Itens`: cabeçalho "Código Descrição / Qtde UN x Vl Unit / Vl Total" e, por item, o código e a descrição numa linha e `qtd un x unitário` com o total à direita na outra (posições não reguladas) |
| 3.1.2 (II) | Valores com vírgula decimal e ponto de milhar | Sim | Atende | `FormatadorImpressao.Dinheiro`, `Quantidade`, `Unitario` (sem `CultureInfo`) |
| 3.1.2 (II) | Descrição longa: a posição e a forma não são reguladas | n/a | Atende | a descrição quebra na largura (`TextoImpressao.Quebrar`); teste de descrição longa |
| 3.1.2 (II) | Valores de frete, seguro, desconto e outras despesas **por item** (I15 a I17a), a critério da UF | Não (UF) | Facultativo | não impresso por item; saem nos totais |
| 3.1.9.1 | Valor total de tributos **por item** (`vTotTrib`, 183a), facultativo | Não | Facultativo | não impresso por item |
| 3.1.3 (III) | Quantidade total de itens (itens distintos, não a soma das quantidades) | Sim | Atende | `Totais` (`Qtde. total de itens`: a contagem dos `det`) |
| 3.1.3 (III) | Valor total R$: somatório dos valores totais dos itens | Sim | Atende | `Totais` (`vProd` do `ICMSTot`) |
| 3.1.3 (III) | Acréscimos (frete, seguro, outras despesas) e desconto: a linha só se existir | Sim | Atende | `Totais`: uma linha por `vDesc`, `vFrete`, `vSeg` e `vOutro` maior que zero, como a figura 2 |
| 3.1.3 (III) | Valor a pagar R$ (W16, `vNF`): só se existir acréscimo ou desconto | Sim | Atende | `Totais`: impresso quando o `vNF` difere do `vProd`; em negrito |
| 3.1.3 (III) | Forma de pagamento (YA02, `tPag`), com o valor parcial de cada uma | Sim | Atende | `Pagamentos`: "FORMA PAGAMENTO" e "VALOR PAGO R$", uma linha por `detPag`; a descrição é a da tabela de meios (`xPag` quando `tPag` é 99) |
| 3.1.3 (III) | Valor pago em cada forma (YA03) | Sim | Atende | `Pagamentos` |
| 3.1.3 (III) | Troco (YA09, `vTroco`) | Sim | Atende | `Pagamentos`: "Troco R$", quando o `vTroco` é maior que zero |
| 3.1.4 (IV) | Texto "Consulte pela Chave de Acesso em" e o endereço da consulta pública (ZX-03, `urlChave`) | Sim | Atende | `Consulta`; a URL é a do `Suplementares.UrlChave`, nunca uma outra |
| 3.1.4 (IV) | A chave de acesso em 11 blocos de 4 dígitos, um espaço entre eles | Sim | Atende | `Consulta`; `FormatadorImpressao.Chave`; teste da chave sem espaços (44 dígitos, a da nota) |
| 3.1.5 (V) | O QR Code do DANFE, de no mínimo 25 x 25 mm (22 mm de conteúdo e 3 mm de margem; acima de 25 mm, margem de 10% do total), à esquerda ou centralizado | Sim | Atende | `Montar`: `QrCodeImpressao` com o texto **igual, byte a byte**, ao `Suplementares.QrCode`; lado de 30 mm, centralizado (figura 5); a margem é do renderizador |
| 3.4, 4.5.2, 4.5.3 | QR Code com correção de erro nível M e texto em UTF-8 | Sim | Atende (do renderizador) | documentado em `QrCodeImpressao`; o PDF (7.4) e o PDV usam M e UTF-8 |
| 4 | O QR Code existe no DANFE da emissão normal e da contingência | Sim | Atende | o `Danfce` recusa a NFC-e sem o QR Code (`ArgumentException`) |
| 3.1.6 (VI) | `CONSUMIDOR CNPJ:`, `CONSUMIDOR CPF:` ou `CONSUMIDOR Id. Estrangeiro:` e o número, em caixa alta, só se constarem do XML | Sim | Atende | `Consumidor`; testes com CPF, CNPJ, estrangeiro |
| 3.1.6 (VI) | Nome e endereço do consumidor: facultativos; **obrigatórios** na entrega em domicílio | Sim (entrega) | Atende | `Consumidor`: nome e endereço do `dest` quando constam; teste de `indPres` 4 |
| 3.1.6 (VI) | "CONSUMIDOR NÃO IDENTIFICADO" quando não há identificação e não é entrega em domicílio | Sim | Atende | `Consumidor`; teste sem identificação |
| 3.1.7 (VII) | Número (B08) e série (B07) da NFC-e | Sim | Atende | `Identificacao`: `NFC-e nº 000000001 Série 001` (9 e 3 dígitos, como as figuras) |
| 3.1.7 (VII) | Data e hora de emissão (B09), **convertida para o horário local** | Sim | Atende (ver decisão) | `Identificacao`: a hora do próprio `dhEmi`, **sem converter** (o `dhEmi` do leiaute 4.00 já é a hora local do emitente, com o `-03:00`); a decisão está no `CONTRATOS.md` 20 |
| 3.1.7 (VII) | "Protocolo de autorização:" e o número (PR09), e a data e hora da autorização (PR08, convertida para o horário local) | Sim | Atende (ver decisão) | `ProtocoloAutorizacao`: `nProt` e `dhRecbto` com o fuso do retorno, sem converter; só com o protocolo autorizado (100 ou 150) |
| 3.1.7 (VII) | Na contingência, a informação do protocolo é suprimida | Sim | Atende | `ProtocoloAutorizacao` não imprime sem o protocolo; a NFC-e offline lida do `NFe` não tem o `protNFe`; com o protocolo (já autorizada), imprime |
| 3.1.8 (VIII) | Mensagem de interesse fiscal: `infAdFisco` (ou, a critério da UF, o `xMsg` do retorno) | Não | Atende / Facultativo | `Mensagens` imprime o `infAdFisco`; o `xMsg` do retorno, que é a critério da UF, **não** |
| 3.1.8 (VIII) | Contingência: "EMITIDA EM CONTINGÊNCIA" e "Pendente de autorização", em destaque, em duas linhas | Sim | Atende | `Contingencia` (chamada pelo `Montar`): `tpEmis` 9 e sem protocolo; centralizado, negrito, a primeira linha em tamanho duplo |
| 3.1.8 (VIII) | O texto da contingência em **dois locais**: abaixo do cabeçalho (centralizado) e abaixo da identificação da NFC-e (VII) | Sim | Atende | `Contingencia` nos dois lugares; teste dos dois |
| 3.1.8 (VIII) | Segunda via do DANFE na contingência (a UF pode dispensar) | Sim (UF) | Atende | `OpcoesDanfce.Via`: o aplicativo chama `Danfce` duas vezes (Consumidor e Estabelecimento); **a lib não decide quantas vias** |
| 3.1.8 (VIII) | A 2ª via identificada ao lado da data e hora com "Via do Estabelecimento" | Sim | Atende | `Identificacao`: a linha "Via do Estabelecimento" logo depois da identificação (a figura 3 a põe abaixo); a via do consumidor leva "Via do Consumidor" na contingência |
| 3.1.8 (VIII) | Guarda eletrônica do XML da contingência no lugar da 2ª via (termo no livro RUDFTO) | Não | Fora | do emitente, não do documento |
| 3.1.8 (VIII) | Ambiente de homologação: "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL", centralizado, em caixa alta | Sim | Atende | `Mensagens`: `tpAmb` 2 da nota, em negrito e tamanho pequeno |
| 3.1.9 (IX) | Mensagem de interesse do contribuinte: `infCpl` (Z03) | Não | Atende | `Mensagens` |
| 3.1.9 | Mensagens institucionais que não estão no XML, **depois** da divisão IX | Não | Atende | `OpcoesDanfce.TextoRodape`, depois do `infCpl` e dos tributos; e a `AssinaturaSoftware` por último |
| 3.1.9.1 | Informações da Lei 12.741/2012 (`vTotTrib` do total, 341a), a critério do emissor | Não | Atende | `Mensagens`: "Tributos Totais Incidentes (Lei Federal 12.741/2012): R$65,62", só quando o `vTotTrib` consta |
| 3.1.9.1 | O detalhamento federal, estadual e municipal em percentual (figura 2) | Não | Fora | o XML não tem os percentuais; o manual não os pede |
| 3.3 | Papel de no mínimo 56 mm de largura; margens laterais de no mínimo 2 mm | Sim | Atende | `PapelImpressao.Bobina58` (margem de 5 mm) e `Bobina80` (4 mm); o A4 é aceito pelo manual, mas **o `Danfce` recusa** o papel que não é bobina (`ArgumentException`: o A4 do DANFCE é decisão do aplicativo e do PDF) |
| 3.3 | Nunca em impressora de ECF, nem como relatório gerencial | Sim | Fora | do aplicativo |
| 3.2 | Exemplos 1, 2 e 3 (normal sem consumidor, entrega com dois pagamentos, contingência com logotipo) | n/a | Atende | os golden `Exemplo1`, `Exemplo2` e `Exemplo3Consumidor` e `Exemplo3Estabelecimento` (os valores, as linhas e os rótulos dos exemplos do manual) |
| 4.7 | A saída de impressão por padrão na tela do frente de caixa, com envio à impressora ou a meio eletrônico | n/a | Fora | do aplicativo |

### 1.1 O que fica fora do DANFCE da lib

- O DANFCE da nota **cancelada** não se imprime (fora da etapa 7.2: a lib não trata; o `Danfce` recusa só o que não é imprimível, sem decidir pela situação fiscal da nota).
- A NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6, modelo 55): é o DANFE da 7.3, não o DANFCE (o `Danfce` recusa o modelo 55).
- ESC/POS e PDF: o PDV e a 7.4 consomem o mesmo `DocumentoImpressao`.

### 1.2 Lacunas do modelo (o que o DANFCE precisaria e a leitura ou o modelo de impressão não traz)

- O logotipo ao lado do cabeçalho (3.1.1): o modelo de impressão empilha elementos; a composição lado a lado fica para o renderizador (a 7.4 pode compor o logotipo e o cabeçalho como quiser a partir dos elementos consecutivos).
- O `xMsg` do retorno da autorização (3.1.8): a `NotaLida` não o traz (o `Protocolo` traz as `Mensagens`, mas o manual deixa a critério da UF e a lib não imprime).
