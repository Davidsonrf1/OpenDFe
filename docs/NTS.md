# Notas Técnicas e pacotes de schemas

O registro de `PROJETO.md` 12.2: cada pacote de liberação de schemas embutido num leiaute e cada Nota Técnica (NT) que ele implementa, com o que mudou, a vigência e a versão da OpenDFe que a trouxe. Nasceu na etapa 2.1 (2026-10-19). A fonte é sempre o Portal Nacional da NF-e (`https://www.nfe.fazenda.gov.br/portal`): **Documentos > Esquemas XML** (`listaConteudo.aspx?tipoConteudo=BMPFMBoln3w=`) e **Documentos > Notas Técnicas** (`listaConteudo.aspx?tipoConteudo=04BIflQt1aY=`).

## 1. Leiaute NF-e/NFC-e 4.00 (`OpenDFe.Leiaute.NFe.v400`)

### 1.1. Pacote embutido: `PL_010f_v1.04`

- **No portal** (conferido em 2026-10-19, o primeiro da lista "VERSÕES OFICIAIS (em uso)"): "Schemas XML NF-e - Pacote de Liberação nº 010f - NT 2025.002 v.1.50 e NT 2026.007 v.1.00 (Publicado em 31/08/26)". Arquivo `PL_010f_v1.04.zip`, SHA-256 `b8589490a58a09a993a80e6ac4d7ed10f20892061ecfc56719337098d4b95998`.
- **Na OpenDFe:** `src/OpenDFe.Leiaute.NFe.v400/Schemas/PL_010f_v1.04/`, embutidos como recurso (`Schemas/PL_010f_v1.04/<arquivo>`) e carregados pelo `SchemaXml` do núcleo. Os bytes são os do ZIP, sem conversão de fim de linha (`.gitattributes`, `-text`); o teste `CadaArquivoTemOsBytesDoPacoteOficial` confere o SHA-256 de cada arquivo contra a tabela abaixo. Desde a versão 0.1.0 (não publicada).

**Os pacotes da série 010 trazem só a nota.** O 010f (como o 010a, o 010b, o 010c e o 010e) tem os cinco arquivos da `NFe`: `nfe`, `leiauteNFe`, `tiposBasico`, `DFeTiposBasicos` e a assinatura. Os schemas das mensagens dos web services (`enviNFe`, `consStatServ`, `consReciNFe`, `inutNFe`, `procNFe` e os retornos) não mudaram desde então e só existem nos pacotes anteriores, que trazem o conjunto inteiro; os da consulta do protocolo e da inutilização foram republicados no 010d, com o CNPJ alfanumérico. A pasta é o pacote vigente da nota **completado**, arquivo a arquivo, com a versão mais recente de cada mensagem publicada no portal. Cada mensagem inclui o `leiauteNFe` (ou o `leiaute<mensagem>`) pelo caminho relativo, por isso todos ficam na mesma pasta: a mensagem do pacote antigo valida com os tipos do 010f.

| Arquivo | Origem (ZIP do portal) | SHA-256 |
|---|---|---|
| `nfe_v4.00.xsd` | `PL_010f_v1.04.zip` | `adce3646c13ceb54922ec3142fc1dc45bd4fb839ac35ad583e86c733c07d27df` |
| `leiauteNFe_v4.00.xsd` | `PL_010f_v1.04.zip` | `2bace939973916d54184ff3e2740041a932de5d79772f3363504504160f22542` |
| `tiposBasico_v4.00.xsd` | `PL_010f_v1.04.zip` | `772619c85723e598840667ca66e7298a250442df47eeb94b397d2a333ce62047` |
| `DFeTiposBasicos_v1.00.xsd` | `PL_010f_v1.04.zip` | `173577a4e3a9dc1d0deced85b89b955b6064bb5a4ae5a96f6727d2da8a694d09` |
| `xmldsig-core-schema_v1.01.xsd` | `PL_010f_v1.04.zip` | `f56744a5f51c03f027de13f39f869307091781a9ef1d91b1ebe14719ce28e1ac` |
| `consSitNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `44d816a725553a0c53508be919629c9235b42c5d41c358a05be8fdf61945f5c1` |
| `leiauteConsSitNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `646e3a6bd26720b12a1322d5d25181504d82175c3650e14a1120dfb65895b618` |
| `retConsSitNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `8b7188d6fe9e879ca057bfb68600746c7491d2e092e3dc5121a6cd5aa79272d5` |
| `leiauteInutNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `e6aa8f817d445cd7796f5bf448ee462598cc00393608ffa038f3a7539de4b184` |
| `procInutNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `c71ce9ed656e22161923d77af47839421fa41c63703a064655880fed76e20334` |
| `retConsReciNFe_v4.00.xsd` | `PL_010d_v1.03.zip` (`NFe/`) | `5cc8219feac16f43a21e168ec4cbf8d0dcf8dade5dccf3f07a42f538519129e9` |
| `enviNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `9ba17a12e3fd6e153bd8676450526ba3bcfc15b75ecb372823826ee827b8c3ef` |
| `retEnviNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `d58b700f849ea7df9f1d35580ae0de425dfa1e295408c02609b81905b04ce4ce` |
| `consReciNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `0520356a8ac619c8730f793ed2a955418ce5a138b6811f3a6f2c0e72017002c5` |
| `consStatServ_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `20c2cae522ac79ea52353ca66d7595c6616a46a491e561b076145fe8574b8773` |
| `retConsStatServ_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `337ceed7f74c0764edbc8a18feacaf8c33f66aa951c46b4b29b561cda7c2f7fb` |
| `leiauteConsStatServ_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `cdc0813b5bf0087c7c8510f6ea0c2a367c829d9ea4b44c23533d18d3bcf23375` |
| `inutNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `d50dd40f010634d8576ad880552a3d186bd4cfe0a4b554f3097dae2cc5c6a59e` |
| `retInutNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `8b5590f79c157510c5f65a43c7325008c01677ebf24e14eef26e4817ea2673a1` |
| `procNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `b7af3072ff7398aa557c42bc6d8198dd3c006b1a6ecaad66bc839a5c34cbcb84` |

Os ZIPs de origem: `PL_010d_v1.03.zip` ("Pacote de Liberação 010d_v.1.03 (ZIP) CNPJ Alfanumérico - NT 2026.004 v.1.01", publicado em 10/07/2026, em uso), SHA-256 `45ceefe4dfbbfec93958283b650a2f1e1734784f4770d070b9907754de081d9b`; `PL_009q_NT2025_001_v1.00.zip` ("Pacote de Liberação nº 9q (Novo leiaute da NF-e, NT 2025.001 v.1.00)", publicado em 19/05/2025, hoje entre as versões anteriores; o mais recente com o conjunto inteiro), SHA-256 `82a26d04a778fc214bf8c6c6e5c65ea577721e32ed5efa9a11a80098b8f9802f`.

**Conferido (2026-10-19):**
- a sequência 010d → 010e → 010f do `leiauteNFe` é cumulativa (comparados os arquivos): o 010e acrescenta ao 010d o `tpImp` 6, o `cIndOp` e o `ISUFEmit`, entre outros; o 010f muda do 010e só o `IE` do emitente (passa a opcional, NT 2026.007) e o tipo do `vNFTot` (`TDec_1302`). O `tiposBasico_v4.00.xsd` é o mesmo nos três;
- o `TChNFe` aceita o CNPJ alfanumérico (`[0-9]{6}[0-9A-Z]{12}[0-9]{26}`) e o `leiauteNFe` tem os grupos da reforma tributária (`IBSCBS`, `IS`, `IBSCBSTot`, `ISTot`, `vNFTot`), também nos testes;
- dos arquivos tirados do 010d e do 9q, os iguais nos dois pacotes têm o mesmo SHA-256; o `leiauteConsSitNFe` e o `leiauteInutNFe` do 010d só diferem do 9q pelo padrão do `Id` (com o CNPJ alfanumérico); o 010e não desfaz nenhum padrão alfanumérico do `leiauteNFe` do 010d;
- todas as mensagens (`nfe`, `procNFe`, `enviNFe`, `retEnviNFe`, `consReciNFe`, `retConsReciNFe`, `consSitNFe`, `retConsSitNFe`, `consStatServ`, `retConsStatServ`, `inutNFe`, `retInutNFe`, `procInutNFe`) carregam e compilam pelo `SchemaXml` do .NET 10, no Windows e no Linux (Ubuntu 26.04).

### 1.2. As NTs que o pacote implementa

As que o título dos pacotes cita e as de schema publicadas desde o 010d. As datas são do "Histórico de Alterações / Cronograma" de cada NT, na versão vigente no portal em 2026-10-19.

| NT | O que muda no schema | Homologação | Produção | Pacote |
|---|---|---|---|---|
| 2025.002 v1.50 (RTC: IBS, CBS e IS; vigente v1.52) | os grupos do IBS, da CBS e do IS na nota e nos totais (desde a v1.20); na v1.50, a reformulação da tributação monofásica de combustíveis. As v1.51 e v1.52 só mudam regras de validação | até 01/09/2026 (a v1.50) | 03/11/2026 (a v1.50) | 010f |
| 2026.007 v1.00 (contribuinte exclusivo do IBS/CBS; vigente v1.10) | a IE do emitente opcional. A v1.10 só muda regras | 01/09/2026 | 03/11/2026 | 010f |
| 2026.002 (DANFE Simplificado Tipo 2, emissão offline, autorização com alerta; vigente v1.11) | o domínio de campos para a NF-e com DANFE Simplificado Tipo 2 (`tpImp` 6; `tpEmis` 9 também na NF-e) | 01/09/2026 | 05/10/2026 | 010e |
| 2026.003 v1.00 (DANFE Simplificado Tipo 2) | citada no título do 010e; não aberta nesta etapa | — | — | 010e |
| 2026.004 v1.01 (CNPJ alfanumérico; complementa a NT Conjunta 2025.001) | o CNPJ e a chave de acesso alfanuméricos nos schemas da NF-e e das mensagens | até 15/06/2026 | 01/07/2026 | 010d |

### 1.3. Pendente: NTs vigentes em homologação sem pacote de schemas publicado

Em 2026-10-19 o portal não tem pacote de schemas posterior ao 010f, e o 010f não tem os campos destas duas (conferido pela busca das tags nos arquivos). Quando o pacote sair, a etapa que o embutir troca a pasta e atualiza esta seção.

| NT | O que muda no schema | Homologação | Produção |
|---|---|---|---|
| 2026.008 v1.00 (RTC, valor líquido do produto; publicada em 01/10/2026) | a sequência `I11a` com `vUnComLiq` e `vProdLiq` no item (0-1, "futuramente obrigatória"); o grupo `ICMSPrevistoPagtoAntecip` (NB01, `vICMSPrevisto`); o `vProdLiqTot` nos totais (W01a, 0-1) | 05/10/2026 | 03/11/2026 |
| 2026.006 v1.00 (vinculação com a transação de pagamento, *split payment*; publicada em 25/08/2026) | o grupo `gPgtoVinc` (YC01, 0-1) e o evento 110300; "não há exigência de preenchimento ou uso dos campos de split payment em 2026" | 05/10/2026 | 03/11/2026 |

### 1.3a. NTs de regras de validação já aplicadas

As NTs que mudam as regras do MOC 7.0 sem mudar o schema embutido, conferidas na SEFAZ emulada (`docs/SEFAZ_EMULADA.md`, E.3) e a conferir de novo nas regras da lib (fase 3) e nas operações (fases 4 e 5).

| NT | O que muda nas regras | Homologação | Produção |
|---|---|---|---|
| 2025.001 v1.02 (simplificação operacional; setembro de 2025) | o QR Code versão 3 da NFC-e (sem CSC; a offline assinada em RSA-SHA1), o 444 (versão 2 do emitente CPF), o **452 (lote assíncrono com uma só nota, GAP03a-3)** e o prazo da NF-e (até 7 dias, 100; depois, 150) | até 08/09/2025 | 13/10/2025 (o 452) |
| 2026.002 v1.11 (emissão offline, autorização com alerta e DANFE Simplificado Tipo 2; setembro de 2026) | o `tpEmis` 9 na NF-e com `tpImp` 6 (711 sem ele), o QR Code também nessa NF-e (só a versão 3, 672), o prazo das 24 horas da offline (150) e o 120 (autorização com alerta) | 01/09/2026 | 05/10/2026 |

O PDF da 2025.001 v1.02 veio de cópia pública (SHA-256 `44b71f16bf1196f35efe0f8645d7992305b37506ffd2e44fe7fb3350616a930d`); o da 2026.002 v1.11 (`72c033b2c7490710f61488a14d0bb3d7bb54e5345aa8d33d6ff2c270c228f046`), do portal. Do portal também, em 2026-10-19: o Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, de março de 2025 (`bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e`), e o MOC 7.0, Anexo III (`e1cfb9b6c5adc6a67062f66fd1699495fdec5dfdb93025768f87fe3782918dde`). O Anexo I baixado de novo é o mesmo arquivo da E.1 (`5eb4cf2010b10b0b62f78197c4eb64025f24535d4c6e61158bc7806dd008f55d`).

### 1.4. Fora do pacote embutido

Os schemas de outras mensagens, em pacotes próprios no portal, entram com as etapas que os usam, conferidos de novo nelas:
- **eventos** (fase 6): o `envEvento` genérico e os específicos (cancelamento, cancelamento por substituição, CC-e, EPEC, manifestação), com a pasta `Evento/` do `PL_010d_v1.03.zip` (CNPJ alfanumérico) e os pacotes de cada evento; os eventos da reforma ("Schema dos eventos da NT 2025.002 v.1.40 - RTC", `Eventos_RTC.zip`, publicado em 27/07/2026);
- **distribuição DF-e** (fase 8): "Pacote de Liberação Distribuição de DF-e v.1.04", publicado em 03/07/2026;
- **consulta cadastro** (fase 8): a pasta `CadConsultaCadastro/` do `PL_010d_v1.03.zip`.

### 1.5. Tabelas de domínio fora do schema

Campos que o XSD declara só pelo padrão (`[0-9]{2}`), com a lista numa tabela do Portal Nacional que muda por informe técnico, sem pacote de schemas novo. No modelo, são enumerações abertas (`docs/MODELO.md`, decisão 8): o código fora da lista é lido como o número e escrito como está; o código novo entra como membro novo, em versão menor.

| Campo | Enumeração | Fonte | Conferido |
|---|---|---|---|
| `tPag` (YA02) | `MeioPagamento` | "Tabela de Meios de Pagamento", publicada em 04/03/2026 (`Tabela_de_meios_de_pagamento - 04032026.xlsx`, no Portal Nacional e no portal da SVRS), e o IT 2024.002 v1.11: os códigos 01 a 05, 10 a 24, 90, 91 e 99. A v1.10 incluiu o 91 (produção em 03/11/2025); a v1.11, o 23 e o 24 (produção em 04/05/2026) | 2026-10-19 (etapa 2.2); o teste `MeioPagamentoTemOsCodigosDaTabelaDoPortal` |
| `tBand` (YA06) | `BandeiraCartao` | o MOC 7.0, Anexo I: 01 a 09 e 99. A NT 2020.006 manda usar a "Tabela de Códigos das Operadoras" publicada no Portal Nacional, que **não foi achada** nas páginas de documentos do portal nem no da SVRS | 2026-10-19 (etapa 2.2), só a lista do MOC |

**Pendente:** a tabela das bandeiras do Portal Nacional. Achada, os códigos que faltam entram em `BandeiraCartao` (versão menor) e esta tabela ganha a fonte.

## 2. Como atualizar

1. Na lista de Esquemas XML do portal, o primeiro pacote de "VERSÕES OFICIAIS (em uso)" que traz a nota; ler o título (as NTs) e a data.
2. Baixar o ZIP numa pasta vazia, conferir os arquivos e comparar com a pasta atual (o que mudou e se é cumulativo).
3. Criar a pasta nova em `Schemas/` com os arquivos do pacote e os que ele não republica (a mensagem mais recente publicada de cada um), apagar a antiga e trocar `SchemasNFe400.Pacote`.
4. Atualizar as tabelas daqui (arquivos, SHA-256, NTs, vigência, versão da OpenDFe) e a tabela do teste `SchemasNFe400Testes`.
5. NT nova sem quebra é versão menor (`PROJETO.md` 12.3).
