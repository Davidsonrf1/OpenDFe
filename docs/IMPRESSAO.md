# Impressão: o documento de conformidade da fase 7

O que a lib imprime e **contra qual exigência**. A fase 7 começa pelo modelo de impressão neutro (núcleo, `OpenDFe.Impressao`, `docs/CONTRATOS.md` 20) e pelo DANFCE (etapa 7.2, seção 1); o DANFE da NF-e (etapa 7.3, `docs/CONTRATOS.md` 21) está na seção 2, e o PDF (7.4), com a coluna "PDF" das exigências de aparência, está na seção 3, e o guia do ESC/POS para o PDV, na 4.

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

## 2. DANFE da NF-e (etapa 7.3)

O DANFE sai de `LeiauteNFe400.Danfe(NotaLida, OpcoesDanfe?)` (`src/OpenDFe.Leiaute.NFe.v400/Impressao/MontagemDanfe*.cs`, um arquivo por grupo de quadros). O `tpImp` da nota escolhe o documento: **1 (retrato) e 2 (paisagem)** saem no A4, com os quadros e os campos do Anexo II; **6 (Simplificado Tipo 2)** sai na bobina, no leiaute do DANFE NFC-e (seção 2.4); 0, 3, 4 e 5 são recusados (`ArgumentException` com a razão). A coluna "Onde" cita o método e o teste (`NFe.Testes/Impressao/`: `DanfeTestes`, golden em `Esperado/Danfe*.txt`; `DanfeConteudoTestes`; `DanfeSimplificadoTestes`; `DanfeEmuladoTestes`).

### 2.1 Fontes e decisões

- **A fonte é o Anexo II do MOC 7.0, "Manual de Especificações Técnicas do DANFE e Código de Barras"** (Portal Nacional da NF-e, Documentos > Manuais; capa "Versão 7.00 - Outubro de 2020", controle de versões "7.00, novembro de 2020"; 28 páginas), SHA-256 `729c04fd1374188c29eb21ca60342741b24a014003d4c2ff1bbb455bb1a6a60e`, lido inteiro, com as páginas dos três leiautes (retrato, paisagem, formulário contínuo) e as figuras dos locais de retirada e de entrega. É o vigente: o portal não traz outra versão. Os acréscimos que as NTs fazem ao DANFE estão aplicados, **cada um só quando o XML tem o dado**: a NT 2018.005 v1.52 (locais de retirada e de entrega, modalidade do frete), a **NT 2026.010 v1.00 (DANFE Reforma Tributária, outubro de 2026, produção a partir de 01/12/2026)** e, para o Tipo 2, a NT 2026.003 v1.00 (seção 2.4). O MOC 7.0 Anexo III (contingência da NF-e) dá a expressão das contingências. As fontes e os SHA-256 estão em `docs/NTS.md` 8.
- **Onde o Anexo II e a NT 2026.010 divergem, vale o Anexo II** (a NT só vigora em 01/12/2026 e diz que o leiaute dela é uma "referência"): a ordem dos quadros (a NT põe o ISSQN antes do transportador; o Anexo II, depois dos produtos), e as colunas dos itens (a NT junta bases, alíquotas e valores em três colunas e tira o código e o NCM da linha; o Anexo II impõe 12 colunas que não se suprimem). A lib imprime **as colunas do Anexo II e acrescenta** as da NT quando há IBS, CBS ou IS. A NT 2026.010 é aplicada no que não colide: o bloco "Total do IBS/CBS/IS", o CRT no quadro do emitente, as colunas da reforma por item (com a alíquota efetiva quando há redução, o grupo `gRed`) e os campos facultativos do total (FCP, DIFAL, monofásicos).
- **O DANFE imprime o que o XML tem** (Anexo II 3.1): campo ausente sai em branco; quadro suprimível (fatura, ISSQN, retirada, entrega, total da reforma) some quando a nota não o usa; o que o leiaute não prevê e o XML tem (valor aproximado dos tributos, ICMS desonerado, FCP, DIFAL, monofásico) só sai quando maior que zero. A lib não confere regra do MOC nem o schema: quem imprime já validou na escrita.
- **A estrutura é neutra** (`CONTRATOS.md` 20 e 21): o DANFE é uma lista de textos, seções (`SecaoImpressao`, linhas de campos rotulados com pesos de largura) e uma tabela (`TabelaImpressao`), o código de barras e a imagem. Os pesos de largura são os centímetros da tabela 3.8.1 do Anexo II (a sugestão; a posição em centímetros é do renderizador, 7.4). Mudar de página, repetir o cabeçalho, numerar a folha, desenhar a marca d'água e o canhoto na vertical é do renderizador (seção 2.6).
- **Os testes conferem o que o golden não confere:** a chave do código de barras é a da nota, com 44 dígitos; os totais exibidos são os do `Total` e a soma da coluna do valor total bate com o total dos produtos; o `tpNF`; a ordem dos quadros; o cabeçalho de continuação; o código de barras dos Dados da NF-e recalculado por outro módulo 11.

### 2.2 DANFE retrato e paisagem: o Anexo II, item a item

`Atende`: sim. `Facultativo`: o Anexo II deixa a critério e a lib não imprime. `Fora`: não é do documento da lib, com o motivo.

| Item do Anexo II | Exigência (resumida) | Obrigatória? | Situação | Onde |
|---|---|---|---|---|
| 2 | Código de barras CODE-128C da chave de acesso (44 dígitos), altura da barra mínima de 0,8 cm | Sim | Atende | `Identificacao`: `CodigoBarrasImpressao` com os 44 dígitos da chave (`AlturaMm` 10); `ACodigoDeBarrasTemAChaveDaNotaCom44Digitos` |
| 2 | Largura total mínima de 6 cm (laser) e do módulo de 0,02 cm; margem clara; DV módulo 103 | Sim | Atende (do renderizador) | o `CodigoBarras128` do pacote de PDF (7.1) calcula o Code 128 C; a largura é do renderizador |
| 2 (b), 3.1.2, 3.9.2 | Contingência com formulário de segurança (FS-IA 2, FS-DA 5): **dois** códigos de barras, o da chave e o dos "Dados da NF-e" (36 dígitos: cUF, tpEmis, CNPJ ou CPF do destinatário, vNF em centavos, ICMS próprio, ICMS ST, dia, DV módulo 11), com a representação em 9 blocos de 4 | Sim (FS) | Atende | `DadosNota`; `OsDadosDaNotaDoFormularioDeSegurancaSaoOsDoAnexoII` (o DV por um segundo módulo 11), exterior (99 e zeros) e CPF; golden `DanfeContingenciaFsda` e `Fsia`; o CNPJ alfanumérico do destinatário **não gera** o código (só dígitos) e recusa (`ArgumentException`) |
| 3 | Homologação: a frase "SEM VALOR FISCAL" no quadro das informações complementares **ou** em marca d'água | Sim | Atende | as duas: `Adicionais` ("EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL") e `DocumentoImpressao.MarcaAgua`; golden `DanfeHomologacao` |
| 3 (NF-e cancelada) | (o manual não trata) a nota cancelada | n/a | Atende (opção) | `OpcoesDanfe.Cancelada` põe a marca d'água `CANCELADA` (a situação é de quem usa); `DanfeCancelada` |
| 3 | Contingência em destaque (Anexo III 2.1: "DANFE em Contingência - impresso em decorrência de problemas técnicos", com o motivo, a data e a hora; EPEC: "DANFE impresso em contingência - EPEC regularmente recebida pela Receita Federal do Brasil") | Sim | Atende | `Identificacao`, no cabeçalho (repete nas páginas seguintes); só sem protocolo; `DanfeContingenciaFsda`, `Fsia`, `Epec`; a SVC (6 e 7) sai como a emissão normal (3.9.1) |
| 3 | "Valor Aproximado dos Tributos" em campo próprio do quadro do imposto (opcional) | Não | Atende | `Imposto`: o `vTotTrib` do total, quando maior que zero; por item, **Facultativo** (o `infAdProd`/`infCpl` são do emitente) |
| 3 | DANFE em uma única via, em papel comum | n/a | Fora | do aplicativo (a lib não decide vias) |
| 3.1 | O campo representa a tag do XML; não se imprime o que não consta do arquivo | Sim | Atende | só campos da nota e do protocolo; o destinatário ausente sai em branco (`ODestinatarioAusenteSaiComOsCamposEmBranco`); os textos fixos são os do manual |
| 3.1 | O DANFE tem todos os campos do modelo adotado | Sim | Atende | os quadros sempre completos (transportador e destinatário com os campos em branco quando faltam); só somem os suprimíveis (3.3) |
| 3.1.1 | A chave em 11 blocos de 4 dígitos, com um espaço entre eles | Sim | Atende | `FormatadorImpressao.Chave`; o teste confere os 11 blocos |
| 3.1.2, 3.9.1 | Campo 1: "Consulta de autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora"; campo 2: "Protocolo de autorização de uso" com o número, a data e a hora | Sim | Atende | `Identificacao`: o protocolo e a data do `protNFe`, sem converter o fuso; o protocolo some sem o `nProt`. **Sem protocolo na emissão normal** o Anexo II não diz o que imprimir: o campo sai em branco (registrado na situação) |
| 3.1.3 | O emitente: nome ou razão social, endereço completo (logradouro, número, complemento, bairro, município, UF, CEP) e **telefone**; logotipo opcional | Sim | Atende | `Identificacao` (negrito); o telefone `(31) 3333-4444`; `OpcoesDanfe.Logotipo` |
| 3.1.4 e NT 2018.005 5.2 | Local de retirada (grupo F), em área específica | Não (facultado) | Atende | `Local("INFORMAÇÕES DO LOCAL DE RETIRADA")`, só se o grupo F existe; `DanfeRetiradaEntrega` |
| 3.1.5 e NT 2018.005 5.3 | Local de entrega (grupo G), em área específica | Não (facultado) | Atende | `Local("INFORMAÇÕES DO LOCAL DE ENTREGA")`, só se o grupo G existe |
| 3.1.6, 3.3.2 | Fatura e duplicatas (o grupo de cobrança); o quadro se suprime se não é usado | Não | Atende | `Cobranca`: a fatura (número, valores) e as duplicatas, três por linha; some sem cobrança; `DanfeDuplicatas` |
| 3.1.7 | O `infAdProd` logo abaixo do item ao qual se refere | Sim | Atende | `Descricao`: depois do nome e do EAN; `DanfeInfAdProd` |
| 3.1.7 | O item de várias linhas com **destaque divisório** entre um item e outro | Sim | Atende | `TabelaImpressao.DivisorLinhas` (o renderizador escolhe linha, espaço ou sombreamento) e `LinhasIndivisiveis` |
| 3.1.7 | Valor unitário comercial diferente do tributável: as duas informações, identificadas | Sim | Atende | `Descricao`: a linha `Trib.: qtd UN x valor` quando a unidade, a quantidade ou o valor tributáveis diferem |
| 3.1.7 | As colunas que não se suprimem: código, descrição, NCM, CST, CFOP, unidade, quantidade, valor unitário, valor total, base e valor e alíquota do ICMS próprio | Sim | Atende | `Tabela`: as 17 colunas do 3.8.1, na ordem do Anexo II ("Colunas apresentadas na ordem descrita"): código, descrição, NCM/SH, O/CST, CFOP, unidade, quantidade, valor unitário, desconto, valor total, base do ICMS, base do ICMS ST, valor do ICMS, valor do ICMS ST, valor do IPI, alíquota do ICMS, alíquota do IPI |
| 3.1.7 e 3.8 | O **CST** (e o CSOSN) com a origem: `O/CST` | Sim | Atende | `LerIcms`: origem + CST de 2 dígitos ou CSOSN de 3 (`000`, `0102`); `ACstDaColunaTemAOrigemNaFrente` e `ACsosnDaColunaTemAOrigemNaFrente`; `DanfeIcmsGrupos` (todos os grupos do ICMS) |
| 3.1.7 | O FCP por item no `infAdProd` | Sim | Fora | do emitente (o campo é dele); o `infAdProd` sai como veio |
| 3.1.7 | Colunas do grupo H e as do interesse da empresa, à direita da descrição | Não | Atende | os detalhes do grupo H saem dentro da descrição (EAN, lote, medicamento, combustível, veículo, arma); as demais, **Facultativo** (seção 2.5) |
| 3.1.8 | As informações complementares trazem o `infAdFisco` e o `infCpl`; o `obsCont` é facultado | Sim | Atende / Facultativo | `Adicionais`; as observações do contribuinte e do fisco **não** saem; o texto longo vai inteiro (a continuação em outra folha é do renderizador) |
| 3.1.8 | O DIFAL e o FCP totais no `infAdFisco`/`infCpl` (NT 2015.003) | Sim | Fora | do emitente; o DANFE imprime o que está nesses campos |
| 3.1.9 | O quadro "Reservado ao Fisco" não se preenche (exceto a critério da UF, o retorno da autorização) | Sim | Atende | em branco; o `xMsg` do retorno **não** sai (`NotaLida` não o traz) |
| 3.1.10 e NT 2018.005 5.1 | O quadro do transportador, com a modalidade do frete e os códigos 0, 1, 2, 3, 4 e 9 | Sim | Atende | `Transporte`: um volume por linha; a modalidade `0 - Remetente (CIF)` a `9 - Sem Ocorrência de Transporte`; o código ANTT é o RNTC do veículo; `DanfeTransporte` |
| 3.2 | Uma coluna para dois campos (código com NCM, CST com CFOP, e outros), em duas linhas por item | Não | Facultativo | a lib usa **uma coluna por campo** (o modelo é estrutural); o renderizador pode empilhar as colunas |
| 3.3.1 | O canhoto pode ser suprimido, **só no retrato** | Não | Atende | `OpcoesDanfe.Canhoto` (padrão sim); na paisagem sai sempre (`ApaisagemNaoSuprimeOCanhoto`); é o primeiro elemento do documento |
| 3.3.3 | O quadro do ISSQN se suprime se não se aplica | Não | Atende | `Issqn`: some sem o total do ISSQN; `DanfeServicoIssqn` |
| 3.4 | O verso da folha, com "CONTINUA NO VERSO" | Não | Fora | a lib continua na **próxima folha** (3.5), nunca no verso; a 7.4 não escreve "CONTINUA NO VERSO" |
| 3.5 | As folhas adicionais repetem, no alto: o emitente, "DANFE" e "Documento Auxiliar", número, série, tipo de operação, folhas, o código de barras, a natureza da operação, a chave, a IE, a IE do substituto e o CNPJ; as colunas dos itens com a mesma disposição | Sim | Atende | `DocumentoImpressao.CabecalhoContinuacao` (o mesmo bloco da primeira folha); a tabela repete o cabeçalho das colunas a cada página; `OCabecalhoDeContinuacaoTemOQueOAnexoIIManda` |
| 3.6.1 | Formulário A4, até Ofício II; o excedente aumenta a largura dos campos e a altura dos itens e dos adicionais | Sim | Atende (A4) | `PapelImpressao.A4Retrato` e `A4Paisagem`; outros tamanhos não existem no modelo |
| 3.6.2 | Margem de 0,2 a 0,8 cm em cada lado | Sim | Atende | as folhas A4 passaram a **8 mm** (eram 10 mm; `CONTRATOS.md` 21); `AMargemDoA4CabeNosLimitesDoAnexoIIDoDanfe` |
| 3.6.3 | Os modelos permitidos: A4 retrato e paisagem, folhas soltas ou formulário contínuo | Sim | Atende (folhas soltas) | o formulário contínuo (Anexos III.03 e III.05) é **Fora** (impressão matricial, fora da etapa) |
| 3.7 | Fonte Times New Roman ou Courier New; os tamanhos mínimos (5, 6, 8, 10 e 12 pontos) e o negrito de cada bloco | Sim | Atende (no modelo) / Fora (os pontos) | o modelo marca negrito e o tamanho duplo do "DANFE" e do emitente; os pontos e a fonte são do renderizador (7.4) |
| 3.7.4 | "DANFE" em negrito (12 pt), número, série, folha e o tipo de operação (a descrição "ENTRADA"/"SAÍDA") em negrito, em caixa alta | Sim | Atende | `Identificacao`: `0 - ENTRADA / 1 - SAÍDA` com o dígito do `tpNF`; `OTipoDeOperacaoTemOTextoDoManualEODigitoDoTpNF` |
| 3.8.1 e 3.8.2 | O tamanho e a posição de cada campo (sugestão: as larguras não são obrigatórias) | Não | Atende (larguras) | os pesos de largura de cada campo são os centímetros da tabela do retrato; a posição absoluta, do renderizador |
| 3.9.3 | Contingência EPEC: o campo 2 com o protocolo de autorização **do EPEC** | Sim | **Parcial** | o rótulo "PROTOCOLO DE AUTORIZAÇÃO DO EPEC" sai com o valor em branco: a `NotaLida` não traz o evento EPEC (o recibo é da fora-de-escopo "DANFE de contingência EPEC com o recibo"); `DanfeContingenciaEpec` |
| 3.10.1 | Marca d'água, desde que não prejudique a legibilidade | Não | Atende | `DocumentoImpressao.MarcaAgua` |
| 3.10.2 | O número da folha e o total, em toda folha, inclusive na primeira | Sim | Atende | `FOLHA {pagina}/{paginas}` (as fichas de `TextoPaginado`); o renderizador troca página a página |
| 3.10.3 | A redução da altura do quadro dos produtos por limitação da impressora | Não | Fora | do renderizador |
| 3.10.4 | Código de barras de dados de interesse do emissor | Não | Facultativo | não impresso |
| 3.10.5 | O `vICMSDeson` copiado no `infCpl` | Sim | Atende | o emitente o põe no `infCpl`; e o DANFE imprime o total em campo próprio (`VALOR DO ICMS DESONERADO`) quando maior que zero |
| NT 2026.010 4.1 | O bloco "Total do IBS/CBS/IS" depois do bloco do ICMS e do IPI: IBS UF, IBS município, CBS, IS e as quatro monofasias | Sim (a partir de 01/12/2026) | Atende | `TotalReforma`: só os campos que o XML tem (o destaque do IBS e da CBS mesmo zero; as monofasias só maior que zero); `DanfeIbsCbs` |
| NT 2026.010 4.2 | O CRT no quadro do emitente; o "Tipo de regime de apuração do IBS e da CBS" reservado | Sim / não | Atende / Fora | `RegimeTexto` (`3 - REGIME NORMAL`); o tipo de regime **não sai**: a NT manda não imprimir enquanto não houver a tag |
| NT 2026.010 4.3 | Os campos da reforma por item: classificação tributária, base, alíquotas e valores do IBS UF, IBS município, CBS e IS; a **alíquota efetiva** quando há redução (`gRed`) | Sim | Atende | três colunas só quando algum item tem IBS, CBS ou IS (`Reforma`), e `cClassTrib` na descrição; a alíquota é a efetiva (`pAliqEfet`) quando `gRed` existe |
| NT 2026.010 4.4 | Campos facultativos: FCP, FCP ST, DIFAL, FCP de destino, ICMS monofásico (base e valor, e por retenção) | Não | Atende | `Imposto`: cada um, só quando maior que zero |
| NT 2026.010 4.5 | O QR Code na NF-e retrato e paisagem (a NT 2025/2026 ainda vai fixar as regras) | Não | Atende | `QrCodeImpressao` ao fim, quando a nota traz o `qrCode` (a lib não forma nem confere o QR Code do DANFE: é o do XML) |

### 2.3 Não implementado: o DANFE Simplificado (3) e a Etiqueta

- **DANFE Simplificado (`tpImp` 3, Anexo II 3.11) e DANFE Simplificado - Etiqueta (NT 2020.004 v1.10, Anexo II 3.12):** **não implementados.** O Anexo II e a NT dão só os **campos mínimos** (o código de barras da chave, o protocolo, o emitente, os dados gerais, o destinatário, os itens (no Simplificado) e o valor total) e o papel (55 mm de largura), **sem a ordem, os quadros nem as dimensões**, e o código de barras "em qualquer sentido, no canto superior direito"; o `tpImp` 3 também não separa um do outro. Sem leiaute de referência que o golden possa conferir, a lib recusa o `tpImp` 3 (`ArgumentException`). **Fontes tentadas:** Anexo II 3.11 e 3.12 (lidos), NT 2020.004 v1.10 (lida inteira, 5 páginas) e a NT 2012.001 (a origem do Simplificado, listada no portal, não aberta: é de 2012 e o Anexo II a absorveu). Se o usuário trouxer um modelo, é uma etapa à parte.

### 2.4 DANFE Simplificado Tipo 2 (`tpImp` 6, NT 2026.003)

A NF-e modelo 55 com o DANFE Simplificado Tipo 2 (Ajuste SINIEF 13/2026) sai no **leiaute do DANFE NFC-e** (a seção 1), com três diferenças da NT 2026.003 v1.00: o texto "DANFE Simplificado Tipo 2" no lugar do "Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica", o "NF-e nº" no lugar do "NFC-e nº", e a **divisão III-A** (os novos tributos). `Danfe` delega a `MontagemDanfce` (o mesmo código do DANFCE, parametrizado), então tudo o que a seção 1 diz vale; o que a NT acrescenta:

| Item da NT 2026.003 | Exigência (resumida) | Obrigatória? | Situação | Onde |
|---|---|---|---|---|
| 3.1.1 | O cabeçalho: CNPJ ou CPF e nome do emitente, o endereço sem o país, o texto "DANFE Simplificado Tipo 2"; logotipo opcional à esquerda | Sim | Atende | `Cabecalho`; o logotipo vai acima (como no DANFCE) |
| 3.1.2 | O detalhe dos itens: código, descrição, quantidade, unidade, valor unitário, valor total | Sim | Atende | igual ao DANFCE |
| 3.1.3 | Os totais: quantidade de itens, valor total, desconto e acréscimos (a linha só se existir), valor a pagar (só se há acréscimo ou desconto), forma e valor de cada pagamento, troco (obrigatório) | Sim | Atende | igual ao DANFCE (o troco sai quando maior que zero, como no DANFCE) |
| 3.1.4 | **Divisão III-A:** "(+) CBS R$", "(+) IBS R$" (UF e município somados) e "(+) IS R$" (só se houver imposto seletivo), entre os totais e o valor a pagar | Sim | Atende | `NovosTributos`: a CBS e o IBS quando o total os traz (mesmo zero), o IS só maior que zero, entre dois tracejados; `ADivisaoIIIASoTemOQueExisteEOISSoQuandoMaiorQueZero` e a ordem; golden `Danfe2NovosTributos` |
| 3.1.5 | "Consulte pela Chave de Acesso em", a URL do ZX-03 e a chave em 11 blocos | Sim | Atende | igual ao DANFCE |
| 3.1.6 e 4 | O QR Code de 25 x 25 mm no mínimo, nível M, UTF-8, na emissão normal e na contingência | Sim | Atende | o `qrCode` da nota, igual byte a byte (a versão 3, 4.3); o Tipo 2 **sem** QR Code ou sem `urlChave` é recusado |
| 3.1.7 | O consumidor: "CONSUMIDOR CNPJ:" e o documento; o nome e o endereço de entrega obrigatórios nas operações não presenciais | Sim | Atende | igual ao DANFCE (CPF e o ID estrangeiro também, como o manual do DANFCE; a NT cita só o CNPJ) |
| 3.1.8 | Número, série, data e hora da emissão e o protocolo e a data da autorização, "convertidas para o horário local"; sem protocolo na contingência | Sim | Atende (ver decisão) | como na seção 1: a hora do próprio `dhEmi`, sem converter (o `dhEmi` do 4.00 já é local) |
| 3.1.9 | A contingência: "EMITIDA EM CONTINGÊNCIA / Pendente de autorização" em **dois locais**, a 2ª via com "Via do Estabelecimento" ao lado da data; a homologação: "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL" | Sim | Atende | `OpcoesDanfe.Via`; a lib não decide quantas vias imprimir (o aplicativo chama duas vezes); `Danfe2ContingenciaConsumidor` e `Estabelecimento`; a homologação em `OHomologacaoTemAFraseDoManual` |
| 3.1.10 | O `infCpl` e a Lei 12.741 (`vTotTrib`) | Não | Atende | igual ao DANFCE |
| 3.3 | Papel de 56 mm no mínimo, margens laterais de 2 mm; A4 permitido; nunca em ECF | Sim | Atende | `OpcoesDanfe.Papel`: `Bobina80` (padrão), `Bobina58` ou `A4Retrato` (a paisagem é recusada); o ECF é **Fora** |

### 2.5 Lacunas do modelo (o que o Anexo II ou as NTs pedem, ou a nota traz, e o DANFE não imprime)

- **O protocolo de autorização do EPEC** (3.9.3): a `NotaLida` é da nota; o evento EPEC e o recibo ficam fora (a etapa dispensa o "DANFE de contingência EPEC com o recibo"). O campo sai em branco.
- **Sem protocolo na emissão normal** (`tpEmis` 1 e `Protocolo` ausente): o Anexo II não diz o que imprimir; o campo do protocolo sai em branco, e a lib não recusa (quem imprime uma pré-visualização pode querer o documento).
- **O tipo de regime de apuração do IBS e da CBS** (NT 2026.010 4.2): sem tag; não impresso.
- **Por item, não impressos:** o frete, o seguro e as outras despesas rateados (só o desconto tem coluna), o valor total de tributos (`vTotTrib`, facultativo), os grupos de importação (DI e adições) e de exportação, o pedido (`xPed`), o FCI, a monofasia do ICMS e do IBS/CBS (ficam nos totais), a retenção, o crédito presumido, o ICMS de partilha para a UF de destino (só no total).
- **Da nota, não impressos** (não são do leiaute do Anexo II; o emitente os põe no `infCpl` se quiser): as referências (`NFref`), o intermediador, os autorizados a baixar o XML, o responsável técnico, a compra (nota de empenho, pedido, contrato), a exportação (UF de saída, local de embarque e de despacho), a cana, o agropecuário, a transação eletrônica do pagamento, e as formas de pagamento (o quadro de pagamento não está no DANFE da NF-e); o veículo do transporte além da placa, da UF e do RNTC (os reboques, o vagão, a balsa, os lacres) e a retenção do ICMS do transporte.
- **As observações do contribuinte e do fisco** (`obsCont`, `obsFisco`) e o `xMsg` do retorno: facultativos (3.1.8 e 3.1.9).

### 2.6 O que o renderizador (7.4) faz com este documento

O documento do DANFE traz a **estrutura**; o PDF (7.4, feito: seção 3) a **compõe** em páginas A4:

- **Paginar** a tabela dos itens: o cabeçalho das colunas se repete a cada página, a linha de um item (`LinhasIndivisiveis`) não se divide entre duas páginas, e o `DivisorLinhas` pede o destaque divisório entre os itens. A continuação das informações complementares é na folha seguinte (nunca no verso; sem "CONTINUA NO VERSO").
- **Repetir** o `DocumentoImpressao.CabecalhoContinuacao` no alto de cada página depois da primeira, e trocar as fichas `{pagina}` e `{paginas}` (`TextoPaginado.Resolver`) em toda página, inclusive na primeira.
- **Desenhar** a `MarcaAgua` em diagonal, atrás do conteúdo, sem prejudicar a leitura; o canhoto (o primeiro elemento do documento: destacável em cima, no retrato; na vertical, à esquerda, na paisagem); os tamanhos mínimos de fonte do 3.7; a largura mínima do código de barras (6 cm); a margem do papel.
- Um renderizador que ignora a `MarcaAgua` ou o `CabecalhoContinuacao` ainda gera um documento válido (o DANFE de homologação leva a frase também nas informações complementares).

## 3. O PDF (etapa 7.4)

O `OpenDFe.Impressao.Pdf` desenha qualquer `DocumentoImpressao` em PDF (`RenderizadorPdf.Gerar`, `docs/CONTRATOS.md` 22): a bobina com a largura do papel e altura contínua, a folha A4 paginada. Esta seção é a **coluna "PDF"** dos checklists das seções 1 e 2: de cada exigência de **aparência** (dimensão, fonte, QR Code, código de barras, margem, paginação, marca d'água), o que o PDF faz, como e que teste prova. As exigências de **conteúdo** (o que se imprime, a ordem, os textos) são do modelo e já estão conferidas nas seções 1 e 2 pelos golden de texto; o PDF as leva inteiras: o teste `OsDanfcesViramPdfComOsDadosDaNota` e o `OsDanfesViramPdfComOsDadosDaNota` conferem, para cada caso do catálogo, que **todo texto que o modelo manda está no PDF** (rótulos, valores, células, títulos), ao lado da chave, do protocolo, do total, da legenda de contingência e da marca d'água.

`Atende`: sim. `Parcial`: atende em parte, com o que falta. `Fora`: não é do renderizador, com o motivo. Os testes citados estão em `Impressao.Pdf.Testes` (elementos, `RenderizadorPdfElementosTestes` e `TextoSeguroTestes`) e em `NFe.Testes/Impressao` (documentos completos, `PdfDocumentosTestes`, e ponta a ponta com o emulador, `PdfEmuladoTestes`).

### 3.1 Fontes e como se conferiu

- **As fontes** são as da seção 1 (Manual do DANFE NFC-e 6.0, SHA-256 `bf906cc2...db5e`) e da seção 2 (Anexo II do MOC 7.0, SHA-256 `729c04fd...6a60e`), **baixadas de novo** na 7.4 com os mesmos SHA-256 (`docs/NTS.md` 9), e lidas nos itens de aparência (3.2 a 3.8 do Anexo II; 3.1.5, 3.3 e 3.4 do manual).
- **O PDF é lido de volta** pelo PdfPig: o texto (e a ordem), as fontes e os tamanhos em pontos, a posição de cada letra, os caminhos vetoriais do QR Code (a matriz de módulos reconstruída e comparada com a do QRCoder) e do código de barras (cada barra na posição e na largura do Code 128 da chave), o tamanho de cada página.
- **Inspeção visual:** o PDF de cada caso foi renderizado em imagem (PyMuPDF, fora do repositório; `OPENDFE_AMOSTRAS=<pasta>` grava os PDFs) e **aberto e olhado** página a página. A situação da etapa no `PLANO.md` lista o que a inspeção achou e foi corrigido.

### 3.2 DANFCE (bobina de 80 e de 58 mm)

| Item do manual | Exigência de aparência | Situação (PDF) | Como | Teste |
|---|---|---|---|---|
| 3.1.5 e 3.4 | O QR Code de no mínimo **25 x 25 mm**, sendo 22 mm de conteúdo e 3 mm de margem segura (quiet zone); acima de 25 mm, margem de 10% da dimensão total | Atende | o lado de 30 mm é o **total com a margem dentro**: o símbolo de 23,5 mm e a margem de 3,24 mm (a maior entre os 3 mm e 10% do manual e os 4 módulos do ISO), nunca menor que o manual | `OQrCodeTem30mmComAMargemDeSilencioDentroEFicaCentrado`, `AMargemDeSilencioSegueOManual...`; a margem medida no PDF de cada caso em `OsDanfcesViramPdfComOsDadosDaNota` |
| 4 e 4.2 | O QR Code no padrão ISO/IEC 18004, **nível M, UTF-8**, impresso com os padrões da impressora (térmica, laser ou jato) | Atende | vetor (SVG) do QRCoder, nível M, UTF-8: nítido em qualquer resolução | `OQrCodeDoPdfTemAMatrizDoQrCoderNoNivelM` (3 textos, um com acentos): a matriz do PDF é a do QRCoder, módulo a módulo |
| 4 | O QR Code do DANFCE é o `qrCode` da nota | Atende | o `Suplementares.QrCode` da nota, sem tocar | `PdfEmuladoTestes` (NFC-e v2 e v3 autorizadas pelo emulador, e a offline antes e depois da autorização): a matriz do PDF é a do `qrCode` do XML autorizado |
| 3.1.5 | O QR Code à esquerda das divisões VI e VII (figura 4) ou **centralizado** (figura 5) | Atende | centralizado (`Alinhamento` do modelo; esquerda e direita também funcionam) | `OAlinhamentoDoQrCodeLevaOBlocoParaAEsquerdaOuADireita`, `OQrCodeTem30mm...FicaCentrado` |
| 3.3 | Papel com **largura mínima de 56 mm** | Atende | a bobina de 80 mm (226,77 pt) e a de 58 mm (164,41 pt) | `ABobinaTemALarguraDoPapelEAlturaQueCresceComOConteudo` |
| 3.3 | **Margens laterais de no mínimo 2 mm** | Atende | 4 mm na bobina de 80 e 5 mm na de 58 (a área impressa da térmica: 72 e 48 mm) | `AFolhaTemAMargemDoPapelEABobinaAsLateraisDoPapel` |
| 3.3 | A altura é a do conteúdo (o papel é bobina; a nota não tem "folha") | Atende | página de **altura contínua** (`ContinuousSize`): 6 mm de margem vertical mais as linhas | `ABobinaTemALargura...` (a altura é 6 mm + 9,2 pt por linha), `ANfceDe300ItensSaiNumBilheteSoDeAlturaContinua` (300 itens em um bilhete) |
| 3.3 | A impressão em A4 (210 x 297 mm) é permitida | Fora | o `Danfce` só aceita a bobina de 80 e de 58 mm; o **Simplificado Tipo 2** da NF-e aceita o A4 em pé, e o PDF o desenha | `OSimplificadoTipo2NoA4SaiNaFolhaComOsMesmosDados` |
| 3.1.2 | As posições das divisões não são reguladas (a figura 1B é a de referência); o tamanho da fonte não é fixado | Atende | a ordem do modelo (a da figura 1B); fonte de 8 pt (normal), 6 pt (pequeno) e 12 pt (duplo), a escala da térmica de 203 dpi (23.3) | `ONegritoEOTamanhoDoTextoNaBobinaSaoOsDaEscalaDaTermica` |
| 3.1.9 (NT 2026.003) e 3.1 | A legenda "EMITIDA EM CONTINGÊNCIA / Pendente de autorização" em destaque, **nos dois locais** | Atende | em negrito, tamanho duplo (12 pt) e centralizada, na mesma linha; o tamanho duplo só quando a frase cabe (decisão do modelo) | `OsDanfcesViramPdfComOsDadosDaNota` (a legenda 2 vezes em cada contingência, nenhuma na autorizada) e `PdfEmuladoTestes` |
| 3.1.8 e 3.1.9 | As duas vias na contingência, a "Via do Estabelecimento" | Atende | o aplicativo gera o PDF **duas vezes** (uma por via, `OpcoesDanfce.Via`); cada via é um PDF de um bilhete | `ADanfceEmDuasViasTemAViaCertaEAMesmaChave` |
| 3.1.1 a 3.1.9 | A legibilidade e a durabilidade (contraste) | Atende | preto sobre branco; o texto invertido (fundo preto, letra branca) quando o modelo pede | `OTextoInvertidoTemFundoPretoELetraBranca` |
| 3.1.2 | O logotipo e a assinatura do software (imagem de 1 bit) | Atende | vetor da imagem na largura pedida ou no tamanho natural a 203 dpi, nunca maior que a linha | `AImagemDe1BitSaiComOsPixelsNaLarguraPedida`, `AImagemSemLarguraTemOTamanhoNaturalA203Dpi`, `AImagemMaiorQueALinhaEncolhe...` |
| 3.1 | Texto longo (a descrição do produto, o `infCpl`) não pode ser cortado | Atende | quebra em palavras, e a palavra maior que a linha quebra na letra | `ATextoLongoQuebraENuncaSeCorta`, `ApalavraMaiorQueALinhaQuebraNaLetra...`, `DescricaoEInfCplLongos` |

### 3.3 DANFE (A4 retrato e paisagem)

| Item do Anexo II | Exigência de aparência | Situação (PDF) | Como | Teste |
|---|---|---|---|---|
| 2 | Código de barras **CODE-128C** da chave (44 dígitos): as barras na **largura mínima de 6 cm** e módulo mínimo de 0,02 cm (0,2 mm) | Atende | as barras de **11,5 cm** (a do 3.8.1) e módulo de 0,415 mm, as 277 posições (22 pares de 11 módulos, o início C, o verificador e o fim) | `OCodigoDeBarrasDaChaveTem277Modulos...` |
| 2 | **Altura mínima da barra de 0,8 cm** | Atende | 10 mm (o modelo: `AlturaMm`) | o mesmo; `OsDanfesViramPdfComOsDadosDaNota` (altura >= 8 mm em cada folha) |
| 2 | **Margem clara** de no mínimo 10 módulos | Atende | 10 módulos de cada lado, fora das barras | `OCodigoDeBarrasTemAAlturaPedidaEAMargemClaraDeDezModulos` |
| 2 | O código de barras **é o da chave** (DV módulo 103, as barras do padrão) | Atende | cada barra na posição e largura do Code 128 da chave da nota (o mesmo `CodigoBarras128` conferido na 7.1) | `PdfVetores.ConferirBarras` em cada folha de cada DANFE do catálogo e de `PdfEmuladoTestes` (a chave do XML autorizado) |
| 2 (FS-IA, FS-DA) | O segundo código de barras, o dos **Dados da NF-e**, na contingência com formulário de segurança | Atende | as barras de 36 dígitos, conferidas pelo mesmo caminho | `OsDanfesViramPdfComOsDadosDaNota` (`ContingenciaFsda` e `ContingenciaFsia`) |
| 3.6.1 e 3.6.3 | A4 **retrato e paisagem**, folhas soltas | Atende | 595,28 x 841,89 pt e 841,89 x 595,28 pt | `OA4TemAsDimensoesDoPapelNoRetratoENaPaisagem`, `OsDanfesViramPdfComOsDadosDaNota` (as dimensões de cada folha) |
| 3.6.2 | **Margem de 0,2 a 0,8 cm** em cada lado (inclusive no alto e embaixo) | Atende | 8 mm (o papel, `CONTRATOS.md` 21.1) | `AFolhaTemAMargemDoPapel...` |
| 3.7 | **Times New Roman ou Courier New** | **Parcial** | a fonte padrão é a **Lato** (SIL OFL), embutida no QuestPDF; o aplicativo registra a Times New Roman (ou a Liberation Serif) e passa `OpcoesPdf.Fonte`; a família não registrada cai na Lato | `AFonteDasOpcoesEAFamiliaPedidaComALatoDeReserva`, `ComUmaFamiliaQueNaoEstaRegistrada...` (não há como embutir a Times New Roman: licença) |
| 3.7.1 | O descritivo dos blocos: **5 pt**, negrito, caixa alta | Atende | 5 pt em negrito (o modelo manda a caixa alta) | `ASecaoTemOTituloOsRotulosPequenosEOsValoresMaioresEmQuadros` |
| 3.7.2 | O descritivo dos campos do quadro dos produtos: **5 pt** | Atende | 5 pt em negrito | `AsColunasDaTabelaTemLarguraProporcionalAoPesoEOAlinhamentoDaColuna` |
| 3.7.3 | O descritivo dos demais campos: **6 pt** | Atende | 6 pt | `ASecaoTemOTitulo...` |
| 3.7.4 | "DANFE" em negrito, **12 pt**; "DOCUMENTO AUXILIAR..." de **8 pt**; número, série, folha e tipo de operação em negrito de **10 pt** | Atende | o texto duplo da folha é 12 pt em negrito; o normal, 8 pt; os campos do quadro de identificação (negrito) em 10 pt | `OTamanhoDoTextoNaFolhaTemOsMinimosDoAnexoIIDoDanfe`, `ASecaoTemOTitulo...` |
| 3.7.5 | A chave de acesso em negrito | Atende | o campo é negrito no modelo (10 pt) | `OsDanfesViramPdf...` (o texto da chave); `OTextoNaoNegrito...` |
| 3.7.6 | O quadro do emitente em negrito; a razão social de **12 pt** e os demais dados de **8 pt** | **Parcial** | em negrito; os dados de 8 pt; **a razão social sai em 8 pt**: o modelo põe o nome como texto normal em negrito, igual ao endereço, e o renderizador não distingue (lacuna 3.4) | `OTamanhoDoTextoNaFolha...` |
| 3.7.7 | O conteúdo dos campos do quadro dos produtos: **6 pt** | Atende | 6 pt | `AsColunasDaTabela...` |
| 3.7.8 | O conteúdo das informações complementares: **6 pt** | Atende | 6 pt (o campo é reconhecido pelo rótulo do Anexo II) | `AsInformacoesComplementaresDe5000Caracteres...` |
| 3.7.9 | O conteúdo dos demais campos: **10 pt** | Atende | 10 pt | `ASecaoTemOTitulo...` |
| 3.8.1 e 3.8.2 | O tamanho e a **posição** de cada campo (sugestão: as larguras não são obrigatórias) | **Parcial** | as larguras de cada campo são **proporcionais aos centímetros** da tabela (os pesos do modelo) e a altura, a natural do conteúdo (a do Anexo II: 0,85 cm); a **posição** é a do empilhamento do modelo, e **não a grade** do Anexo II (o quadro do emitente, o "DANFE" e o código de barras da chave lado a lado) | `ASecaoTemOTitulo...` (3:1), `AsColunasDaTabela...` (pesos) |
| 3.1.7 e 3.5 | O quadro dos itens com as **colunas que não se suprimem** e o descritivo das colunas repetido nas folhas adicionais | Atende | a tabela do QuestPDF com o cabeçalho **repetido a cada folha** | `ATabelaDe150LinhasPassaDeUmaPaginaERepeteOCabecalhoEmCadaUma`, `ANfeDe300ItensPaginaComOCabecalhoDaTabelaEmCadaFolhaEOsItensNaOrdem` |
| 3.1.7 | O item de várias linhas com **destaque divisório**, e a linha do item **não se divide** entre duas folhas | Atende | a linha entre os itens (mais escura com o `DivisorLinhas`) e a célula com `ShowEntire` quando o modelo pede `LinhasIndivisiveis` | `ALinhaIndivisivelNaoSeDivideEntreDuasPaginas` |
| 3.1.7 | O `infAdProd` abaixo do item; a descrição longa | Atende | as linhas da mesma célula; a descrição de 500 caracteres quebra na célula e a linha cresce | `ADescricaoDoItemDe500CaracteresQuebraNaCelulaENaoSeCorta`, `OTextoDeUmaCelulaDaTabelaComQuebraDeLinha...` |
| 3.3.1 | O **canhoto**: o recibo de entrega, destacável | **Parcial** | o primeiro elemento do documento, no alto da folha 1 (retrato e paisagem), com a linha tracejada de destaque; **na paisagem o Anexo II o põe na vertical, à esquerda** e o PDF o põe na horizontal (lacuna 3.4) | `OCanhotoSoSaiNoDanfeQueOTemEFicaNoAltoDaPrimeiraPagina` |
| 3.4 e 3.5 | O que passa de uma folha **continua na folha seguinte** (nunca no verso, sem "continua no verso"); as folhas adicionais repetem o emitente, "DANFE", número, série, folha, o código de barras, a natureza da operação, a chave, as IEs e o CNPJ | Atende | o `CabecalhoContinuacao` no alto de cada folha depois da primeira, e o quadro maior que a folha continua na seguinte | `OCabecalhoDeContinuacaoSaiSoDaSegundaPaginaEmDiante`, `AsInformacoesComplementaresDe5000CaracteresContinuamNaFolhaSeguinteComACabecalho` |
| 3.10.2 | O **número da folha e o total** em toda folha, inclusive a primeira | Atende | `FOLHA {pagina}/{paginas}` com o número da página e o total do QuestPDF | `AsFichasDePaginaTrocamParaONumeroDaPaginaEOTotal...`, `OsDanfesViramPdf...` (`FOLHA k/n` em cada folha de cada DANFE) |
| 3.10.1 e 3 | A **marca d'água** ("SEM VALOR FISCAL" na homologação, "CANCELADA"), desde que **não prejudique a legibilidade** | Atende | texto em negrito, cinza `#E6E6E6`, a 45 graus, centrado, **atrás do conteúdo**, em todas as folhas | `AMarcaDAguaSaiEmTodasAsPaginasEmDiagonalEEmCinzaClaro`, `AMarcaDAguaDoA4SaiInteiraEmDiagonal` (as duas frases), `OsDanfesViramPdf...` (a marca em cada folha de `Homologacao` e `Cancelada`) |
| 3 | O contraste para a leitura dos códigos de barras (papel que não seja jornal) | Atende | preto sobre branco; o código de barras nunca fica sob a marca d'água clara | a inspeção visual |
| 3.6.1 | Folhas maiores que o A4 (até o Ofício II) | Fora | o modelo só tem o A4 | n/a |
| 3.10.3 | A redução da altura do quadro dos produtos por limitação da impressora | Fora | não há limitação num PDF | n/a |

### 3.4 Lacunas do PDF (o que o Anexo II pede e o PDF não faz)

- **A grade do Anexo II (3.8)**: o PDF **empilha** os elementos na ordem do modelo (o quadro do emitente, o "DANFE", o código de barras e a chave, um sobre o outro), em vez de pôr o emitente, o "DANFE" e o código de barras lado a lado como no leiaute do Anexo II. Os campos, as larguras proporcionais e as fontes são os do Anexo II, mas a folha fica mais alta: **uma nota de um item no retrato cabe numa folha; na paisagem (210 mm de altura) o quadro dos dados adicionais passa para a folha 2.** Compor o cabeçalho lado a lado pede um elemento novo no modelo (uma linha de blocos) e fica para quando houver pedido.
- **A razão social do emitente em 12 pt (3.7.6)**: o modelo manda o nome como texto normal em negrito, e o renderizador dá 8 pt ao texto normal (o mínimo dos dados do emitente e de "DOCUMENTO AUXILIAR"). Para o nome sair em 12 pt, o modelo precisa de um tamanho entre o normal e o duplo (ou o renderizador de um papel para o nome), o que muda os golden de texto da 7.3.
- **O canhoto na vertical (paisagem, 3.3.1)**: o PDF o põe na horizontal, no alto.
- **Times New Roman ou Courier New (3.7)**: a Lato é a padrão; as fontes do Anexo II exigem o aplicativo as registrar (README do pacote).
- **O texto em colunas de largura fixa** (a térmica): a Lato é proporcional, e uma frase que cabe em 48 colunas de térmica pode quebrar em duas linhas no PDF de 80 mm ("FORMA PAGAMENTO" na bobina de 58 mm). Nada é cortado.
- **PDF/A, assinatura do PDF e e-mail** estão fora da etapa.

## 4. O ESC/POS para o PDV (guia, sem implementar)

O PDV imprime o DANFCE em impressora térmica por **ESC/POS**, a partir do **mesmo** `DocumentoImpressao` (`LeiauteNFe400.Danfce`), **sem usar o `OpenDFe.Impressao.Pdf`** nem o QuestPDF (e sem a licença dele). A lib **não implementa** o ESC/POS (`PROJETO.md` 10.3: é do aplicativo, que conhece a marca e o modelo da impressora); este guia diz como percorrer o modelo. Os comandos abaixo são os do conjunto ESC/POS comum (Epson TM, Bematech, Elgin, Daruma e compatíveis); confira no manual da sua impressora.

**O papel e as colunas.** `papel.Colunas` (48 na bobina de 80 mm e 32 na de 58 mm, fonte A de 12 x 24 pontos a 203 dpi) e `papel.ColunasTamanho(tamanho)` (`Pequeno` = `Colunas * 4 / 3`, a fonte B de 9 x 17; `Duplo` = `Colunas / 2`) dão a largura de uma linha em caracteres. A área impressa é de 576 pontos (72 mm) na de 80 mm e 384 pontos (48 mm) na de 58 mm.

| Elemento | ESC/POS |
|---|---|
| `TextoImpressao` | quebre com `texto.Quebrar(papel.ColunasTamanho(texto.Tamanho))` e imprima uma linha por vez, com o alinhamento (`ESC a 0/1/2`: esquerda, centro, direita), o negrito (`ESC E 1` e `ESC E 0`), o tamanho (`Pequeno`: `ESC M 1`, a fonte B; `Normal`: `ESC M 0` e `GS ! 0x00`; `Duplo`: `GS ! 0x11`, o dobro na largura e na altura) e o invertido (`GS B 1` e `GS B 0`); termine a linha com `LF` |
| `LinhaImpressao` | `var larguras = linha.Larguras(papel.Colunas)` (a soma é sempre `Colunas`); quebre cada célula com `celula.Quebrar(larguras[i])`; a linha tem tantas linhas de impressão quanto a célula mais alta; em cada uma, preencha cada célula com espaços até a largura dela, alinhando à esquerda, ao centro ou à direita (`CelulaImpressao.Alinhamento`), e junte as células numa só linha de texto |
| `SeparadorImpressao` | uma linha de `Colunas` caracteres: `-` para `Tracejado`, `=` para `Cheio` |
| `EspacoImpressao` | `Linhas` linhas em branco (`LF`) |
| `QrCodeImpressao` | o comando **nativo** da impressora: `GS ( k` com o modelo 2 (`fn 165`), o tamanho do módulo (`fn 167`), o nível de correção M (`fn 169`, valor 49), os dados (`fn 180`) e a impressão (`fn 181`); o tamanho do módulo vem de `LadoMm`: **o lado de 30 mm inclui a margem de silêncio**, então o símbolo ocupa cerca de 80% (3 mm de margem de cada lado) e o módulo em pontos é `símbolo em mm * 8 / número de módulos` (versão 3: 29 módulos). Se a impressora não tem o comando nativo, gere a matriz com um gerador de QR Code (nível M, UTF-8, o mesmo texto) e imprima como **raster** |
| `CodigoBarrasImpressao` | o comando nativo `GS k 73` (Code 128) com o conjunto C (`{C` e os pares de dígitos), `GS h` para a altura (`AlturaMm * 8` pontos), `GS w` para o módulo e `GS H 0` sem o texto; ou raster |
| `ImagemImpressao` | **raster**: `GS v 0` com a largura em bytes (`(Largura + 7) / 8`) e a altura; os `Bits` do `Imagem1Bit` (MSB primeiro, 1 = preto, linha em bytes inteiros) **já estão no formato do comando**; centralize com `ESC a 1` |
| `QuebraPaginaImpressao` | o **corte**: `GS V 66 0` (avança o papel e corta parcialmente), e o que vem depois é outro bilhete (a outra via) |
| `SecaoImpressao` e `TabelaImpressao` | são do DANFE (A4); o PDV não os recebe do `Danfce` |

**Os caracteres.** A térmica não é Unicode: escolha a **página de código 850** (`ESC t 2`, ou a 860 do português do Brasil, conforme a impressora) e converta o texto antes de enviar (o `Encoding.GetEncoding(850)` do `System.Text.Encoding.CodePages`). O que a página não tem (o travessão `–` e as aspas tipográficas, por exemplo) o aplicativo troca antes: a homologação do `Danfce` já usa o hífen ASCII (`-`) por isso (`CONTRATOS.md` 20.3).

**Um exemplo curto**, a escala do percurso (o ESC/POS de cada comando é do aplicativo):

```csharp
DocumentoImpressao nota = new LeiauteNFe400().Danfce(lida);
var papel = nota.Papel;
foreach (var elemento in nota.Elementos)
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
        case SeparadorImpressao s:
            impressora.Linha(new string(s.Estilo == EstiloSeparador.Cheio ? '=' : '-', papel.Colunas));
            break;
        case EspacoImpressao e:
            impressora.LinhasEmBranco(e.Linhas);
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

`impressora` e `Preencher` são do aplicativo: a primeira escreve os comandos no fluxo da porta serial, USB ou de rede da impressora; a segunda junta, numa linha de `papel.Colunas` caracteres, a linha `y` de cada célula, preenchida e alinhada na largura que `Larguras` deu.
