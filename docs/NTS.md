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
| `envEvento_v1.00.xsd` | `PL_010d_v1.03.zip` (`Evento/`) | `e4d61ba81582b5f1f9a2a4b135d33f4c1ec1c8f9138a2343a9abfffa02bb2777` |
| `leiauteEvento_v1.00.xsd` | `PL_010d_v1.03.zip` (`Evento/`) | `9a48df8603f685653dfde27dbda72e0f47b5d3c13be0090952f1e82f8d2bc476` |
| `retEnvEvento_v1.00.xsd` | `PL_010d_v1.03.zip` (`Evento/`) | `5eb93a244c5b26c983c08f725f8eafaa1923664a992181b9d78f60cc8523fa72` |
| `procEventoNFe_v1.00.xsd` | `PL_010d_v1.03.zip` (`Evento/`) | `4263ee5d12e7a731efab126310aba4edb25612e5986a1c5dde0c87e6c6173798` |
| `tiposBasico_v1.03.xsd` | `PL_010d_v1.03.zip` (`Evento/`) | `d89d4fe1136f89abc7878b440d91ac0e770fbc128dd35b483284c961d6c3fd5d` |
| `e110111_v1.00.xsd` | `Evento_Canc_PL_v1.01_NT_2018_004.zip` | `56fcdad336b281a9de5e7aae27850bc2c5bd7714e267c891e1b4d0dc227c65da` |
| `e110112_v1.00.xsd` | `Evento_CancSubst_v1.01_NT_2018_004.zip` | `73c555577c21a10d9f572efaafe56ad52314b86e98d9ba3a8d679a2750ee944e` |
| `e110110_v1.00.xsd` | `Evento_CCe_PL_v1.01.zip` | `0a56d7efa780579eb82f28c2069748bad1bf0fdbae57f3c59b0a198cecf3b718` |
| `retConsStatServ_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `337ceed7f74c0764edbc8a18feacaf8c33f66aa951c46b4b29b561cda7c2f7fb` |
| `leiauteConsStatServ_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `cdc0813b5bf0087c7c8510f6ea0c2a367c829d9ea4b44c23533d18d3bcf23375` |
| `inutNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `d50dd40f010634d8576ad880552a3d186bd4cfe0a4b554f3097dae2cc5c6a59e` |
| `retInutNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `8b5590f79c157510c5f65a43c7325008c01677ebf24e14eef26e4817ea2673a1` |
| `procNFe_v4.00.xsd` | `PL_009q_NT2025_001_v1.00.zip` | `b7af3072ff7398aa557c42bc6d8198dd3c006b1a6ecaad66bc839a5c34cbcb84` |
| `e210200_v1.00.xsd` | `Evento_ManifestaDest_PL_v1.01.zip` | `afe48b729cdefbdf122d1dee81382aa7832b9bdf1596ed4de6c5451070071b98` |
| `e210210_v1.00.xsd` | `Evento_ManifestaDest_PL_v1.01.zip` | `ebc9171b9b64192599270cafdf51e13756b09cebce83d2e78ac0140dee4003e7` |
| `e210220_v1.00.xsd` | `Evento_ManifestaDest_PL_v1.01.zip` | `636e742e2f6784b8626a8f3b284d3eac01e2ea28a1fb8a56d9d847e5b92abcfd` |
| `e210240_v1.00.xsd` | `Evento_ManifestaDest_PL_v1.01.zip` | `e7df7243f0a6d5b7b590e5251b83f34a52d0ec3a68a8e6b398bcb4cfc862b14e` |
| `distDFeInt_v1.01.xsd` | `PL_NFeDistDFe_104.zip` | `2d95df1430b0f0e2b04c6f29cf6bb69cdb5a392ac52102987653902b9a34ff7a` |
| `retDistDFeInt_v1.01.xsd` | `PL_NFeDistDFe_104.zip` | `059c4183e190282851337e80725c6ac4e996736a85077246d8711b3b31406236` |
| `resNFe_v1.01.xsd` | `PL_NFeDistDFe_104.zip` | `6ca072e27d72d3a74065d3414af669f7ad525f8883a22af89d22da96095856fa` |
| `resEvento_v1.01.xsd` | `PL_NFeDistDFe_104.zip` | `77bd5c05e336fa2854f8ab71316e07fe30759c5efedf493bb684a77c26af9d55` |
| `tiposDistDFe_v1.01.xsd` | `PL_NFeDistDFe_104.zip` | `0bc215b9bda6422e37c2c28b80590b4c5ba3041f0db85ca17ebba61edadd7547` |
| `CadConsultaCadastro/consCad_v2.00.xsd` | `PL_010d_v1.03.zip` (`CadConsultaCadastro/`) | `8e5cfa3386c1e61999fc290ad0c3cf166258125447c53a342bea5036524b0fc0` |
| `CadConsultaCadastro/leiauteConsultaCadastro_v2.00.xsd` | `PL_010d_v1.03.zip` (`CadConsultaCadastro/`) | `85e613e63109398d79561bd2d7c372c62fdd033bde537c2c74cd2abaed9b5d9d` |
| `CadConsultaCadastro/retConsCad_v2.00.xsd` | `PL_010d_v1.03.zip` (`CadConsultaCadastro/`) | `2662459d90461de342e9aeee5b9b9505d25f698e773e1d5c3a0207d7eb2fc9e2` |
| `CadConsultaCadastro/tiposBasico_v1.03.xsd` | `PL_010d_v1.03.zip` (`CadConsultaCadastro/`) | `0822f1b36b535dc090df244409cd9d8b86628950236e79f5b716ec2b1a208c1a` |

Os ZIPs de origem: `PL_010d_v1.03.zip` ("Pacote de Liberação 010d_v.1.03 (ZIP) CNPJ Alfanumérico - NT 2026.004 v.1.01", publicado em 10/07/2026, em uso), SHA-256 `45ceefe4dfbbfec93958283b650a2f1e1734784f4770d070b9907754de081d9b`; `PL_009q_NT2025_001_v1.00.zip` ("Pacote de Liberação nº 9q (Novo leiaute da NF-e, NT 2025.001 v.1.00)", publicado em 19/05/2025, hoje entre as versões anteriores; o mais recente com o conjunto inteiro), SHA-256 `82a26d04a778fc214bf8c6c6e5c65ea577721e32ed5efa9a11a80098b8f9802f`.

**Os eventos** (desde a etapa E.4 da trilha da SEFAZ emulada, 2026-10-20, por decisão do usuário, adiantando a parte da fase 6): o evento genérico da pasta `Evento/` do `PL_010d_v1.03.zip` (o `envEvento`, o `retEnvEvento` e o `procEventoNFe`, com o CNPJ alfanumérico), que deixa o `detEvento` livre (`xs:any`), e o `detEvento` de cada evento, validado à parte (MOC 7.0, Anexo I, D06): o do cancelamento (`e110111`), o do cancelamento por substituição (`e110112`) e o da carta de correção (`e110110`), cada um do pacote do evento em uso no portal: "Esquemas XML NF-e / NFC-e - Evento Cancelamento (Atualizado em 21/12/2018)" (`Evento_Canc_PL_v1.01_NT_2018_004.zip`, SHA-256 `71ccd5fcc48f9604a2a8965146df45e3b73e96e87942ab3391be0d55cc0df718`), "Esquemas XML NFC-e - Evento Cancelamento por Substituição (Atualizado em 21/12/2018)" (`Evento_CancSubst_v1.01_NT_2018_004.zip`, `49a32c2c4dc64413f6ef47ed4756b54121ec56514d5352d6931b9bf43a0706a1`) e "Esquemas XML NF-e - Pacote de Liberação Evento CCe v1.01 (30/05/2014)" (`Evento_CCe_PL_v1.01.zip`, `c300695ae3344dcb0c000ec806559e7392c54f7be99599274965050112771f77`). Os pacotes de cada evento trazem o evento inteiro (`envEventoCancNFe`, `leiauteEventoCancNFe` e afins), mas com um `tiposBasico_v1.03.xsd` próprio e anterior ao CNPJ alfanumérico; deles entra só o `detEvento`, que inclui o `tiposBasico_v1.03` da mesma pasta, o do 010d.

**A manifestação do destinatário, a distribuição DF-e e a consulta cadastro** (desde a etapa E.5 da trilha da SEFAZ emulada, 2026-10-21, adiantando a parte da fase 8, como na E.4): o `detEvento` dos quatro eventos da manifestação, do pacote "Esquemas XML NF-e - Pacote de Liberação Evento Manifesta Destinatário v1.01 (30/05/2014)" (`Evento_ManifestaDest_PL_v1.01.zip`, SHA-256 `05877dd4a4a42ea7e1b251a2c2ecc365d3c73f3a1a9794760486db7cbe2a116e`), sem includes, validados à parte do evento genérico como os do cancelamento; a distribuição inteira do "Esquemas XML NF-e - Pacote de Liberação Distribuição de DF-e v.1.04. Publicado em 03/07/2026" (`PL_NFeDistDFe_104.zip`, `9bd6d478dd04770016783a914111f3e2cc794c6a2f2b62ac7243fb101c138740`; o pedido, o retorno e os dois resumos do `docZip`, com os tipos próprios, que importam o `xmldsig-core-schema_v1.01.xsd` da pasta); e a pasta `CadConsultaCadastro/` do `PL_010d_v1.03.zip` (o mesmo ZIP baixado de novo, com o mesmo SHA-256), **numa subpasta com o mesmo nome**: o `tiposBasico_v1.03.xsd` dela não é o dos eventos (o `TStat` só com 3 algarismos e só comentários a mais), e os dois não cabem na mesma pasta. O `SchemaXml` resolve o include pela pasta do recurso, e o nome do recurso leva a subpasta (`Schemas/PL_010f_v1.04/CadConsultaCadastro/consCad_v2.00.xsd`).

**Conferido (2026-10-21), na E.5:**
- o pacote da manifestação traz o evento inteiro (`envConfRecebto`, `leiauteConfRecebto`, `retEnvConfRecebto`, `procConfRecebtoNFe`) com um `tiposBasico_v1.03.xsd` e um `xmldsig-core-schema_v1.01.xsd` próprios e mais antigos; dele entra só o `detEvento` (`e210200` a `e210240`), que não inclui nada. O `e210240` exige o `xJust` (a justificativa da operação não realizada, que o MOC, H01, rejeita com 595); o `e210220` o aceita como opcional;
- o `retDistDFeInt_v1.01` tem o `ultNSU` e o `maxNSU` obrigatórios (o MOC diz 0-1) e o `NSU` do `docZip` opcional (facultativo desde a NT 2014.002 v1.15); o `resNFe` exige a IE do emitente (`TIe`: algarismos ou `ISENTO`). O `CNPJ` do pedido e dos resumos é o alfanumérico (a v1.04, da NT 2026.004);
- o `retConsCad_v2.00` exige o argumento da consulta (IE, CNPJ ou CPF) também na rejeição, e o `dhCons` é `xs:dateTime`;
- todos (os quatro `detEvento`, o `distDFeInt`, o `retDistDFeInt`, o `resNFe`, o `resEvento`, o `consCad` e o `retConsCad`) carregam e compilam pelo `SchemaXml` do .NET 10, no Windows e no Linux (Ubuntu 26.04).

**Conferido (2026-10-20), nos eventos:**
- o `tiposBasico_v1.03.xsd` muda de pacote para pacote: o do 010d v1.03 difere do 010d v1.01 ("Eventos e ws de Consulta CCC", de 08/06/2026, o `PL_Evento.zip` de dentro) só em comentários e numa documentação; os do cancelamento (32.400 bytes) e da CC-e (30.140) são outros, mais antigos. O `envEvento`, o `leiauteEvento`, o `retEnvEvento` e o `procEventoNFe` são iguais no 010d v1.01 e no v1.03;
- a pasta `Evento/` do 010d traz também um `tiposBasico_v4.00.xsd`, diferente do do 010f, e o `xmldsig-core-schema_v1.01.xsd`, igual ao do 010f; o `leiauteEvento` não inclui o primeiro, que não entra;
- o `e110111`, o `e110112` e o `e110110` compilam com o `tiposBasico_v1.03` do 010d (o `TProt`, o `TJust`, o `TChNFe` alfanumérico, o `TCodUfIBGE` e o `TVerAplic` estão nele); o `e110110` não inclui nada;
- o `retConsSitNFe_v4.00` (PL_010d) tem o seu próprio `TProcEvento`, cujo `retEvento` não tem o `cOrgaoAutor` (R22 do MOC, do cancelamento por substituição) que o `leiauteEvento` tem.

**Conferido (2026-10-19):**
- a sequência 010d → 010e → 010f do `leiauteNFe` é cumulativa (comparados os arquivos): o 010e acrescenta ao 010d o `tpImp` 6, o `cIndOp` e o `ISUFEmit`, entre outros; o 010f muda do 010e só o `IE` do emitente (passa a opcional, NT 2026.007) e o tipo do `vNFTot` (`TDec_1302`). O `tiposBasico_v4.00.xsd` é o mesmo nos três;
- o `TChNFe` aceita o CNPJ alfanumérico (`[0-9]{6}[0-9A-Z]{12}[0-9]{26}`) e o `leiauteNFe` tem os grupos da reforma tributária (`IBSCBS`, `IS`, `IBSCBSTot`, `ISTot`, `vNFTot`), também nos testes;
- dos arquivos tirados do 010d e do 9q, os iguais nos dois pacotes têm o mesmo SHA-256; o `leiauteConsSitNFe` e o `leiauteInutNFe` do 010d só diferem do 9q pelo padrão do `Id` (com o CNPJ alfanumérico); o 010e não desfaz nenhum padrão alfanumérico do `leiauteNFe` do 010d;
- todas as mensagens (`nfe`, `procNFe`, `enviNFe`, `retEnviNFe`, `consReciNFe`, `retConsReciNFe`, `consSitNFe`, `retConsSitNFe`, `consStatServ`, `retConsStatServ`, `inutNFe`, `retInutNFe`, `procInutNFe` e, desde a E.4, `envEvento`, `retEnvEvento`, `procEventoNFe`, `e110110`, `e110111` e `e110112`) carregam e compilam pelo `SchemaXml` do .NET 10, no Windows e no Linux (Ubuntu 26.04).

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

As NTs que mudam as regras do MOC 7.0 sem mudar o schema embutido, conferidas na SEFAZ emulada (`docs/SEFAZ_EMULADA.md`, E.3 e E.4) e a conferir de novo nas regras da lib (fase 3) e nas operações (fases 4 a 6).

| NT | O que muda nas regras | Homologação | Produção |
|---|---|---|---|
| 2025.001 v1.02 (simplificação operacional; setembro de 2025) | o QR Code versão 3 da NFC-e (sem CSC; a offline assinada em RSA-SHA1), o 444 (versão 2 do emitente CPF), o **452 (lote assíncrono com uma só nota, GAP03a-3)** e o prazo da NF-e (até 7 dias, 100; depois, 150) | até 08/09/2025 | 13/10/2025 (o 452) |
| 2026.002 v1.11 (emissão offline, autorização com alerta e DANFE Simplificado Tipo 2; setembro de 2026) | o `tpEmis` 9 na NF-e com `tpImp` 6 (711 sem ele), o QR Code também nessa NF-e (só a versão 3, 672), o prazo das 24 horas da offline (150) e o 120 (autorização com alerta) | 01/09/2026 | 05/10/2026 |
| 2020.005 v1.21 (criação e atualização de regras; outubro de 2021) | a numeração inutilizada na autorização (3B08-100, 206) só na emissão normal e na SVC (em SP, também na FS-IA, na EPEC e na FS-DA); os eventos que impedem o cancelamento (4P15-30 e 4P15-34, 939 e 940) | 01/07/2021 | 01/09/2021 (v1.21: 25/10/2021) |
| 2023.002 v1.01 (emitente CPF na NFC-e) | a NFC-e do produtor rural pessoa física, com o seu cancelamento (o autor CPF no modelo 65), e o fim da denegação na NFC-e | 24/07/2023 | 04/09/2023 (v1.01: 02/02/2026) |
| 2024.001 v1.20 (CRT 4, MEI, e denegação; agosto de 2024) | **o fim da denegação também na NF-e** (Ajuste SINIEF 43/23): excluída a 1C17-40 (301); o emitente irregular é rejeitado com 781 (1C17-38); o 302 e o 303 do destinatário passam a rejeição | 03/06/2024 | 16/09/2024 |
| 2026.001 v1.02b (PAA, provedor de assinatura e autorização; julho de 2026) | as séries 970 a 989 reservadas ao PAA (I02d, 266 na inutilização) e o CNPJ também nas séries 980 a 989 (J02e da consulta, P12-26 e P12-44 do evento) | 03/08/2026 | 05/10/2026 |
| 2020.001 v1.60 (manifestação do destinatário; abril de 2026; substitui a 2012.002 e a 2013.001) | as regras da manifestação (6.3.7 e 6.3.8: 575, 594, 596, 650, 651, 655, 496); até 2 eventos de cada manifestação conclusiva (v1.50, Ajuste SINIEF 43/23); o prazo da manifestação conclusiva de 90 dias da autorização (v1.60, Ajuste SINIEF 14/26; era 180), e o da ciência, de 10 dias | 15/05/2026 (o prazo de 90 dias) | 01/06/2026 (o prazo de 90 dias) |
| 2014.002 v1.40 (Web Service de Distribuição de DF-e; julho de 2026) | o que cada ator recebe (a tabela do item 3), o NSU por interessado, o `consChNFe` sem o NSU (v1.15), o uso indevido com o 656 e o `ultNSU` na rejeição (3.11.4), o limite de 10 KB do pedido e os textos dos motivos (137 "Nenhum documento localizado", 138 "Documento(s) localizado(s)"); o CNPJ alfanumérico (v1.40, com o pacote 1.04) | — | 08/07/2026 (a v1.40) |

Os PDFs da 2020.001 v1.60 (`NT_2020.001 v1.60 - Manifestação do destinatário.pdf`, SHA-256 `f610878ad54708086aaa20dc668d714adf89cf109b8afbea687f69ef733efe46`) e da 2014.002 v1.40 (`NT2014.002_v1.40 - WsNFeDistribuicaoDFe CNPJ alfa.pdf`, `156dcb170e205c076a6da86aedf56a04afcec3aef8e373dd7fbf7ccb068da029`) vieram do portal na E.5 (2026-10-21). Os textos da 2020.005 v1.21, da 2023.002 v1.01, da 2024.001 v1.20 e da 2026.001 v1.02b foram lidos na E.4 (2026-10-20) em cópias baixadas em sessões anteriores, sem o SHA-256 registrado: a conferir de novo no portal na fase 3. O PDF da 2025.001 v1.02 veio de cópia pública (SHA-256 `44b71f16bf1196f35efe0f8645d7992305b37506ffd2e44fe7fb3350616a930d`); o da 2026.002 v1.11 (`72c033b2c7490710f61488a14d0bb3d7bb54e5345aa8d33d6ff2c270c228f046`), do portal. Do portal também, em 2026-10-19: o Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, de março de 2025 (`bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e`), e o MOC 7.0, Anexo III (`e1cfb9b6c5adc6a67062f66fd1699495fdec5dfdb93025768f87fe3782918dde`). O Anexo I baixado de novo é o mesmo arquivo da E.1 (`5eb4cf2010b10b0b62f78197c4eb64025f24535d4c6e61158bc7806dd008f55d`).

### 1.4. Fora do pacote embutido

Os schemas de outras mensagens, em pacotes próprios no portal, entram com as etapas que os usam, conferidos de novo nelas:
- **eventos** (fase 6): o detalhe da EPEC, do pacote do evento (o genérico, o cancelamento, o cancelamento por substituição e a CC-e estão embutidos, acima, desde a E.4; a manifestação, desde a E.5); os eventos da reforma ("Schema dos eventos da NT 2025.002 v.1.40 - RTC", `Eventos_RTC.zip`, publicado em 27/07/2026).

A distribuição DF-e e a consulta cadastro, que a fase 8 usa, estão embutidas desde a E.5 (acima); a fase 8 as confere de novo.

### 1.5. Tabelas de domínio fora do schema

Campos que o XSD declara só pelo padrão (`[0-9]{2}`), com a lista numa tabela do Portal Nacional que muda por informe técnico, sem pacote de schemas novo. No modelo, são enumerações abertas (`docs/MODELO.md`, decisão 8): o código fora da lista é lido como o número e escrito como está; o código novo entra como membro novo, em versão menor.

| Campo | Enumeração | Fonte | Conferido |
|---|---|---|---|
| `tPag` (YA02) | `MeioPagamento` | "Tabela de Meios de Pagamento", publicada em 04/03/2026 (`Tabela_de_meios_de_pagamento - 04032026.xlsx`, no Portal Nacional e no portal da SVRS), e o IT 2024.002 v1.11: os códigos 01 a 05, 10 a 24, 90, 91 e 99. A v1.10 incluiu o 91 (produção em 03/11/2025); a v1.11, o 23 e o 24 (produção em 04/05/2026) | 2026-10-19 (etapa 2.2); o teste `MeioPagamentoTemOsCodigosDaTabelaDoPortal` |
| `tBand` (YA06) | `BandeiraCartao` | o MOC 7.0, Anexo I: 01 a 09 e 99. A NT 2020.006 manda usar a "Tabela de Códigos das Operadoras" publicada no Portal Nacional, que **não foi achada** nas páginas de documentos do portal nem no da SVRS | 2026-10-19 (etapa 2.2), só a lista do MOC |

**Pendente:** a tabela das bandeiras do Portal Nacional. Achada, os códigos que faltam entram em `BandeiraCartao` (versão menor) e esta tabela ganha a fonte.

### 1.6. Campos que saíram do schema do 4.00

A leitura aceita a nota de qualquer NT do 4.00 (`PROJETO.md` 5.2). Em 2026-10-21 (etapa 2.5), os 43 pacotes do 4.00 listados no portal (do `PL_009` da NT 2016.002 v1.10, de 10/04/2017, ao 010f) foram baixados e comparados por script com o embutido: nenhum valor de enumeração saiu, e os campos que saíram estão em `docs/LEITURA.md` 5 (o `clEnq` do IPI, o trio `cCredPresumido` do 9m e as versões preliminares dos grupos da reforma, do 010a ao 010e). A leitura os reconhece como `NFE-RETIRADO`, pela tabela do `LeitorNFe`. **Ao trocar o pacote** (seção 2), comparar o novo com o anterior: o campo que sair entra na tabela, com a NT.

## 2. Como atualizar

1. Na lista de Esquemas XML do portal, o primeiro pacote de "VERSÕES OFICIAIS (em uso)" que traz a nota; ler o título (as NTs) e a data.
2. Baixar o ZIP numa pasta vazia, conferir os arquivos e comparar com a pasta atual (o que mudou e se é cumulativo).
3. Criar a pasta nova em `Schemas/` com os arquivos do pacote e os que ele não republica (a mensagem mais recente publicada de cada um), apagar a antiga e trocar `SchemasNFe400.Pacote`.
4. Atualizar as tabelas daqui (arquivos, SHA-256, NTs, vigência, versão da OpenDFe) e a tabela do teste `SchemasNFe400Testes`.
5. NT nova sem quebra é versão menor (`PROJETO.md` 12.3).

## 3. Autorizadores, serviços e URLs (`AutorizadoresNFe400`)

Conferidos em 2026-10-22 (etapa 4.2). A tabela fica em `src/OpenDFe.Leiaute.NFe.v400/AutorizadoresNFe400.cs` (os serviços SOAP e os atendimentos) e `AutorizadoresNFe400.Urls.cs` (as URLs), e o teste `AutorizadoresNFe400Testes` confere a forma dela.

### 3.1. Fontes

| O quê | Fonte |
|---|---|
| Serviços SOAP: nome do serviço e método | NT 2016.002 v1.61 (10/09/2018), 2.3, do portal (SHA-256 `4fa4bb19dbedf7653f00768ed418216395ba3fd5a9cf189f473688556d6987f0`). O namespace do WSDL é `http://www.portalfiscal.inf.br/nfe/wsdl/<serviço>`, e a ação, o namespace mais `/<método>` |
| NF-e: autorizadores, UFs de cada um e URLs de produção | "Relação de Serviços Web" do Portal Nacional (`www.nfe.fazenda.gov.br/portal/webServices.aspx?tipoConteudo=OUC/YVNWZfo=`) |
| NF-e: URLs de homologação | a mesma página no portal de homologação (`hom.nfe.fazenda.gov.br/portal/webServices.aspx?tipoConteudo=OUC/YVNWZfo=`) |
| NFC-e: autorizadores e URLs dos dois ambientes | "Relação de Serviços Web" do Portal da NFC-e na SVRS (`dfe-portal.svrs.rs.gov.br/Nfce/Servicos`): AM, GO, MS, MT, PR, RS, SP e SVRS |
| NFC-e de MG, que a SVRS não lista | "Web Services" da SEF-MG (`portalsped.fazenda.mg.gov.br/spedmg/nfce/web-services/`) |

Os WSDLs mesmo não foram abertos: todos os autorizadores pedem o certificado do cliente já para o `?wsdl` (HTTP 403 sem ele). O Portal da NFC-e da ENCAT (`nfce.encat.org`) estava fora do ar.

### 3.2. O que a tabela diz

- **NF-e, normal:** AM, BA, GO, MG, MS, MT, PE, PR, RS e SP com autorizador próprio; MA na SVAN; as outras 16 UFs na SVRS. A mesma tabela vale para os tipos de emissão que transmitem depois ao autorizador normal: FS-IA (2), EPEC (4, a nota; o evento vai ao AN pelo destino do pedido), FS-DA (5) e o offline (9, o do DANFE Simplificado Tipo 2, NT 2025.001).
- **NF-e, contingência SVC:** SVC-AN (6) para AC, AL, AP, CE, DF, ES, MG, PA, PB, PI, RJ, RN, RO, RR, RS, SC, SE, SP e TO; SVC-RS (7) para AM, BA, GO, MA, MS, MT, PE, PR e PI. **O PI está nas duas** (decisão do usuário na 4.2): o portal de produção o põe na SVC-AN, e o de homologação, na SVC-RS; quem usa escolhe pelo tipo de emissão.
- **NFC-e:** AM, GO, MG, MS, MT, PR, RS e SP com autorizador próprio; as outras 19 UFs na SVRS (BA, PE e MA inclusive: nenhuma das fontes lista autorizador próprio de NFC-e para elas). Só o normal e o offline (9): sem SVC nem EPEC.
- **As URLs:** as das páginas, sem o `?wsdl` que GO, MT, PE e PR trazem na lista (é o endereço do WSDL; o serviço é o mesmo caminho sem ele). A SVC-AN tem os endereços da SVAN, e a SVC-RS os da SVRS, sem a inutilização. A consulta cadastro na NF-e nas 10 UFs e na SVRS (que só consulta AC, ES, RN, PB e SC; quem confere a UF é a operação, na 8.2); na NFC-e, só onde a página dela a lista (GO, MS, PR). O AN, só com o `NFeRecepcaoEvento4` (manifestação e EPEC).
- **A distribuição DF-e (etapa 8.1, 2026-10-24):** `NFeDistribuicaoDFe`, só na NF-e e só no AN: `https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx` (produção) e `https://hom1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx` (homologação), conferidas nas duas páginas da "Relação de Serviços Web" (seção "Ambiente Nacional"; serviço `NFeDistribuicaoDFe`, versão 1.00 do serviço) baixadas em 2026-10-24: produção SHA-256 `b1107dfee8f54525e0302368e884bb1b32117dbfb25b2e13f0f341f880c10f22`, homologação `4453c5cce7d391795b0460d2b15248a18a482e7e95fda0e902b29d16ef71b572`. O `nfeDadosMsg` vai dentro do elemento da operação (`nfeDistDFeInteresse`, NT 2014.002, 5 e 6), que o `ServicoSoap.Operacao` escreve (`CONTRATOS.md` 23). A NFC-e não é distribuída. As cifras de TLS por autorizador (`DeclararCifras`) continuam vazias: sem o PFX, não há como conferir o que cada autorizador negocia.

### 3.3. Como atualizar

Baixar de novo as páginas da 3.1 (o Portal Nacional pede cookie: `curl -c` e `-b` com o mesmo arquivo), comparar com a tabela e trocar as linhas que mudaram. URL trocada é versão menor (`PROJETO.md` 12.3). Até a versão nova sair, quem usa troca a URL na configuração (`ConfiguracaoDFe.Urls`).

## 4. URLs de consulta da NFC-e (`UrlsConsultaNFCe`)

Conferidas em 2026-10-24 (etapa 5.1). A tabela fica em `src/OpenDFe.Leiaute.NFe.v400/UrlsConsultaNFCe.cs` e é listada por `LeiauteNFe400.UrlsConsulta`; o teste `UrlsConsultaTestes` confere a forma dela e a lista das UFs que faltam. Cada linha tem a URL do QR Code (sem a query: a lib acrescenta o `?p=`) e a da consulta pela chave (o `urlChave`). A NF-e com o DANFE Simplificado Tipo 2 usa a mesma tabela (NT 2026.002, ZX02-20, observação 3).

### 4.1. Fontes

| O quê | Fonte | Conferido |
|---|---|---|
| Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0 (março de 2025) | Portal Nacional da NF-e (`exibirArquivo.aspx?conteudo=k/IuuaW4YiY%3D`), SHA-256 `bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e` (baixado de novo, confere com o da E.3) | 2026-10-24: o parâmetro de cada forma (4.3 e 4.4), o hash (4.3.4 e 4.3.5) e o CSC (4.6). Os dois exemplos de hash do manual conferem (`DC6AE2C2B9A992BE59679AC365E29922DE6B7511` e `4615A93BB0D7C4E780F8D30EE77EDD5BA55C7D66`, com o CSC `SEU-CODIGO-CSC-CONTRIBUINTE-36-CARACTERES`, de 41 caracteres). **O manual não traz as URLs**: manda para o Portal da NFC-e da ENCAT (`nfce.encat.org/desenvolvedor/qrcode/`) |
| NT 2025.001 v1.02 (QR Code versão 3, 444) e NT 2026.002 v1.11 (NF-e com `tpImp` 6, 672) | Portal Nacional | já conferidas (1.3a); só citadas |
| Manual 6.0, 4.3.2/4.3.5 e 4.4.2, as formas offline do QR Code (etapa 5.2); NT 2026.002 v1.11 (B22-10, 711) | os mesmos arquivos, já com SHA-256 acima | 2026-10-24: o desenho e o código seguem as leituras da E.3 e da 5.1 (`digVal` = o `DigestValue` em Base64 com cada caractere em hexadecimal; `dia` e `vNF` da nota; a assinatura da versão 3 sobre os sete primeiros parâmetros). **O manual não foi baixado de novo na 5.2** (a nova tentativa de download foi barrada pelo controle de permissão): a conferência do 4.3.5 (passo 1) com o texto do manual fica para o usuário (`CONTRATOS.md` 17.2) |
| Portal da NFC-e da ENCAT, as URLs por UF (QR Code e consulta pela chave) | `nfce.encat.org/desenvolvedor/qrcode/` | **fora do ar** em 2026-10-24 (nome não resolve; a 4.2 já o achou assim) |
| Wayback Machine da página da ENCAT | `web.archive.org` | **fora do ar** em 2026-10-24 ("Temporarily Offline"; a API de disponibilidade só devolvia um retrato de 15/10/2022, velho demais para valer) |
| Portal de cada UF | o site da SEFAZ | só o de MG deu as duas URLs de forma clara (4.2) |

### 4.2. As linhas da tabela

| UF | Ambiente | URL do QR Code | URL da consulta pela chave | Fonte |
|---|---|---|---|---|
| MG | produção | `https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml` | `https://portalsped.fazenda.mg.gov.br/portalnfce` | "Web Services" da SEF-MG (`portalsped.fazenda.mg.gov.br/spedmg/nfce/web-services/`), 2026-10-24 |
| MG | homologação | `https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml` | `https://hportalsped.fazenda.mg.gov.br/portalnfce` | a mesma página, 2026-10-24 |

A página da SEF-MG lista o **mesmo endereço do QR Code nos dois ambientes**, e só o da consulta pela chave muda. Um fórum (ACBr) relata que a ENCAT e a SEF-MG já divergiram na homologação; a linha segue a página da SEF-MG (fonte primária) e fica **a conferir com o PFX** (uma divergência seria a rejeição 395 ou 878, só na homologação).

### 4.3. As UFs que ficaram fora (nenhuma fonte primária conferida)

AC, AL, AM, AP, BA, CE, DF, ES, GO, MA, MS, MT, PA, PB, PE, PI, PR, RJ, RN, RO, RR, RS, SC, SE, SP e TO, nos dois ambientes: a ENCAT e o Wayback Machine estavam fora do ar, e o portal de cada UF ou não foi achado, ou não traz as duas URLs, ou bloqueia o acesso (o do RJ), ou a página é de 2016 e o endereço do QR Code não consta (GO). Nada foi tirado de blog nem de fórum: a regra é não inventar URL. Quem emite nessas UFs informa as duas na `ConfiguracaoNFCe` (`UrlQrCode` e `UrlChave`); sem elas, a montagem recusa a nota com `NFE-QRCODE` (o comportamento seguro). O SC não emite NFC-e (a lista `UFsPendentes` do teste o inclui só porque a tabela não o tem).

O que as fontes consultadas disseram e **não** entrou na tabela, para quem completar:

- **PR:** o portal da SEFA-PR (`sped.fazenda.pr.gov.br/NFCe/Pagina/QR-Code`) dá o QR Code `http://www.fazenda.pr.gov.br/nfce/qrcode`, o mesmo na produção e na homologação (o ambiente vai no `tpAmb` do parâmetro); falta a URL da consulta pela chave.
- **GO:** os comunicados de 2016 (`goias.gov.br/economia/...`) trazem só `https://nfe.sefaz.go.gov.br/nfeweb/sites/nfce/danfeNFCe` (produção) e `https://homolog.sefaz.go.gov.br/nfeweb/sites/nfce/danfeNFCe` (homologação), de 2016; a SEFAZ-GO trocou a URL aceita depois (relato de 2021), e não há fonte atual.
- **ES:** o portal (`sefaz.es.gov.br/qr-code` e `/url-dos-web-services`) deu as duas URLs de forma truncada ou ambígua (o QR Code como `www2.sefaz.es.gov.br/nfce/consulta` e a consulta pela chave com o domínio repetido); não foi usado.
- **RJ:** a página `portal.fazenda.rj.gov.br/dfe` bloqueou o acesso. Pelo resumo de uma busca, o RJ trocou o QR Code (produção e homologação) para `https://consultadfe.fazenda.rj.gov.br/consultaNFCe/QRCode` em 19/12/2023, com o endereço antigo (`www4.fazenda.rj.gov.br`) valendo até 02/09/2024; **não conferido na página**.
- **RN:** a SEFAZ-RN avisou em 18/05/2026 que o domínio da SET deixou de existir e que os links do QR Code mudaram (só do RN); o aviso (`sefaz.rn.gov.br/materia/atencao-desenvolvedores-nfc-e/`) respondeu 503, e as URLs novas não foram vistas.
- **GO, MG, RJ, RN e SP** são as UFs que o texto da página da ENCAT (salva em 2026-10-19, que não está mais no repositório) dizia ter data de troca de endereço; as datas não foram confirmadas, e por isso o `docs/NTS.md` não as traz. Uma URL trocada vale versão menor da lib (`PROJETO.md` 12.3).

### 4.4. Como completar

Quando a ENCAT voltar, ou para cada UF: abrir a página, copiar as duas URLs de cada ambiente (a do QR Code sem a query), acrescentar as linhas em `UrlsConsultaNFCe.cs`, tirar a UF da lista `UFsPendentes` do `UrlsConsultaTestes` (o teste quebra se a lista e a tabela divergirem), e registrar aqui a fonte e a data.

## 5. Eventos e inutilização (etapa 6.1, 2026-10-24)

- **MOC 7.0, Visão Geral** ("Versão 7.00 - Novembro de 2020", 150 páginas), do Portal Nacional (`listaConteudo.aspx?tipoConteudo=ndIjl+iEFdE=`, cookie com `curl -c` e `-b` e a página de origem no `-e`; `exibirArquivo.aspx?conteudo=LrBx7WT9PuA=`), conferido em 2026-10-24: SHA-256 `f664dcf94b77cabb32311620d85a7eb02cdf86adb2d4632ce178af2572dd2ad1`. Lidas as seções 5.3 (a inutilização: Tabelas 5-9 a 5-12, o `ProcInutNFe` do schema), 5.8 (a parte geral do evento: Tabelas 5-32 a 5-36, o 128, o 135 e o 136 do 5.8.5) e 5.9 (o cancelamento: Tabelas 5-37 e 5-38; o **155**, "Cancelamento homologado fora de prazo", no 5.9.4).
- **Etapa 6.2 (2026-10-24):** o MOC 7.0, Visão Geral, baixado de novo pelo mesmo caminho, com o **mesmo SHA-256** (`f664dcf94b77cabb32311620d85a7eb02cdf86adb2d4632ce178af2572dd2ad1`). Lidas as seções 5.9.1 (o leiaute do 110112: `descEvento`, `cOrgaoAutor`, `tpAutor`, `verAplic`, `nProt`, `xJust`, `chNFeRef`), 5.9.3 (Tabela 5-38: 920, 921, 455, 910, 911, 912 a 919, 501, 580 e 577) e 5.10 (a carta de correção: Tabelas 5-39 e 5-40, a condição de uso nas duas formas, o `xEvento` "Carta de Correção registrada", a correção mais recente substituindo as anteriores). **Conferido no texto: a regra 5P31-44 (921) manda a substituta ser de contingência (`tpEmis` diferente de 1), e a P12-48 (920) manda a cancelada ser da emissão normal.** Os pacotes de schema do cancelamento por substituição (`Evento_CancSubst_v1.01_NT_2018_004.zip`) e da CC-e (`Evento_CCe_PL_v1.01.zip`) são os da seção 1.1 (SHA-256 acima); o `e110110` enumera as duas formas do `descEvento` e do `xCondUso` (com e sem acento), e a lib escreve a forma **sem acento** (as duas valem no schema e no MOC 5.10; a sem acento não depende da codificação do texto; um teste confere o texto contra o XSD embutido). A NT do cancelamento por substituição da NFC-e é a 2018.004 (a do pacote acima); o texto da CC-e está no MOC 7.0, 5.10, e a NT dela não foi baixada separadamente.
- O Anexo I (4.1 e 4.4) não foi baixado de novo: vale o arquivo da E.1 (`5eb4cf2010b10b0b62f78197c4eb64025f24535d4c6e61158bc7806dd008f55d`, seção 1.3a).
- **Schemas** (já embutidos, seção 1.1): `envEvento`, `leiauteEvento`, `procEventoNFe`, `retEnvEvento`, `e110111`, `inutNFe`, `leiauteInutNFe`, `retInutNFe` e `procInutNFe`. O elemento raiz do `procInutNFe_v4.00.xsd` é `ProcInutNFe` (com o P maiúsculo), e a lib escreve esse nome.

## 6. Impressão em PDF: pacotes e licença (etapa 7.1, 2026-10-24)

Não há NT nesta etapa; as fontes são os pacotes NuGet e o texto de licença do QuestPDF.

- **Versões fixas** (`Directory.Packages.props`; a última estável de cada uma com **pelo menos 14 dias** de publicada em 2026-10-24, pela API do NuGet):

| Pacote | Versão | Publicada | Licença | Onde |
|---|---|---|---|---|
| `QuestPDF` | 2026.9.1 | 2026-09-25 | Community, Professional ou Enterprise do QuestPDF (abaixo) | `OpenDFe.Impressao.Pdf` |
| `QRCoder` | 1.8.0 | 2026-04-04 | MIT | `OpenDFe.Impressao.Pdf` |
| `PdfPig` (namespace `UglyToad.PdfPig`) | 0.1.16 | 2026-08-22 | Apache-2.0 | só `OpenDFe.Impressao.Pdf.Testes` |

  Passaram na frente por data: o `QRCoder` 1.8.1 (2026-10-13, 11 dias) e as pré-versões do `PdfPig` (alfa diário, 0.1.17). O `QuestPDF` 2026.9.1 no `net10.0` não tem dependência de pacote; traz o Skia e o qpdf nativos em `runtimes/` (win-x64, win-x86, win-arm64, linux-x64, linux-arm64, linux-musl-x64, osx-x64, osx-arm64). SHA-256 do `questpdf.2026.9.1.nupkg` baixado: `83e78c3323ad95f56e36a10f293683918fdeaba923b4f3533f27e6adad4ffcf6`. As bibliotecas do Linux dependem só de `libstdc++`, `libm`, `libgcc_s`, `libc` e `libz` (`readelf -d`, na 2026.9.1); a documentação não pede `fontconfig`.
- **Licença do QuestPDF** (conferida em 2026-10-24, em `https://www.questpdf.com/license/`, `/license/community.html` e `/license/guide.html`, e no `LICENSE.md` do próprio pacote 2026.9.1, **Effective Date 6 July 2026, Version 3.0**):
  - É "source-available", **não** é MIT nem OSI. A Community vale, sem custo e também para uso comercial, para: pessoa física com receita bruta anual abaixo do limite; aprendizado e avaliação; entidade de caridade; instituição acadêmica sem fins lucrativos ou pública; **projeto open source sob licença OSI**; **organização com receita bruta anual abaixo de USD 1.000.000** no último exercício (consolidada entre entidades sob controle comum); e o "usuário de dependência transitiva" (a Community não vale se a biblioteca intermediária for basicamente um invólucro do QuestPDF: aí a licença se determina como se o código chamasse o QuestPDF direto).
  - **Órgãos públicos e empresas de capital aberto não são elegíveis à Community, qualquer que seja a receita.** Transição de 90 dias ao deixar de se enquadrar. "Desenvolvedor", para a licença paga, é quem escreve código que chama o QuestPDF (a CI e o AI de código contam como a organização); as pagas cobrem número ilimitado deles.
  - O README do pacote `OpenDFe.Impressao.Pdf` copia o resumo, com a data e o endereço. **Divergência com o texto anterior do `PROJETO.md` 10.2** ("a comunitária (MIT)"): corrigida na 7.1.
- **Code 128:** a tabela dos 107 padrões foi conferida contra a do `python-barcode` (`barcode/charsets/code128.py`, 2026-10-24: os 107 padrões em módulos, convertidos em larguras de barra e espaço, idênticos aos da lib) e contra a soma de 11 módulos por símbolo (13 no fim); o teste decodifica de volta.

## 7. Impressão: o Manual do DANFE NFC-e (etapa 7.2, 2026-10-24)

- **Manual de Padrões Técnicos do DANFE NFC-e e QR Code, versão 6.0 (março de 2025)**, Portal Nacional da NF-e (`exibirArquivo.aspx?conteudo=k/IuuaW4YiY%3D`, que pede o cookie: `curl -L -c cj -b cj`), baixado de novo, com o **mesmo SHA-256** da seção 4.1 (`bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e`, 1.887.904 bytes, 40 páginas). Lido inteiro, com as figuras dos exemplos 1, 2 e 3 (itens 3.1 a 3.4 e 4.5 a 4.8 para o DANFCE): as nove divisões do DANFE NFC-e, o texto fixo de cada uma, a contingência (as duas legendas e a segunda via), a homologação, o consumidor, a Lei 12.741, o papel (56 mm, margens de 2 mm) e o QR Code (25 x 25 mm, nível M, UTF-8). O checklist, exigência por exigência, está em `docs/IMPRESSAO.md` 1.
- **O que o manual 6.0 não traz** (e o arquivo da etapa trazia): "Não permite aproveitamento de crédito de ICMS" e o prefixo "DANFE NFC-e - " do título; a lib segue o manual.
- **A conversão da data para o horário local** que o manual pede (3.1.7) vem do tempo do `dhEmi` em UTC; no leiaute 4.00 o `dhEmi` e o `dhRecbto` já trazem o horário local e o deslocamento, e a lib imprime sem converter (decisão do arquivo da etapa, `CONTRATOS.md` 20.3).
- A NT 2025.001 v1.02 e a NT 2026.002 v1.11 **não foram lidas de novo** nesta etapa: são NTs de regras de validação (seção 1.3a), e as cópias não estão no repositório; o que elas dizem da NFC-e (o `tpEmis` 9, a regra dos 5 minutos e a offline depois de 24 horas) já está implementado e não altera o que o DANFCE imprime. O manual 6.0 (março de 2025) é posterior à NT 2025.001 e a cobre.

## 8. Impressão: o DANFE da NF-e (etapa 7.3, 2026-10-24)

Todos baixados do Portal Nacional da NF-e em 2026-10-24 (`https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=<id>`, que pede o cookie da página de origem: `curl -L -c cj -b cj -A "Mozilla/5.0" -e <página da lista>`), convertidos com `pdftotext -layout` e lidos; as figuras dos leiautes foram vistas como imagem. As páginas de lista são `listaConteudo.aspx?tipoConteudo=ndIjl+iEFdE=` (os manuais do MOC) e `listaConteudo.aspx?tipoConteudo=04BIflQt1aY=` (as NTs).

| Documento | Identificador (`conteudo=`) | Páginas | SHA-256 | O que foi lido |
|---|---|---|---|---|
| **MOC 7.0, Anexo II: Manual de Especificações Técnicas do DANFE e Código de Barras** (capa "Versão 7.00 - Outubro de 2020"; controle de versões "7.00, novembro de 2020") | `f%20NhsSn3/5M%3D` | 28 | `729c04fd1374188c29eb21ca60342741b24a014003d4c2ff1bbb455bb1a6a60e` | inteiro: o código de barras (2), os campos do DANFE (3.1), as supressões (3.3), o verso e as folhas adicionais (3.4 e 3.5), o formulário (3.6), as fontes (3.7), a tabela de tamanhos do retrato e da paisagem (3.8), os campos de conteúdo variável (3.9), a marca d'água, a folha e o código de barras (3.10), o DANFE Simplificado e a Etiqueta (3.11 e 3.12), o Code 128 C (Anexo III.01) e as figuras dos leiautes (III.02 a III.05) |
| **MOC 7.0, Anexo III: Manual de Contingência da NF-e** | `cMjGAyYrb/s%3D` | 21 | `e1cfb9b6c5adc6a67062f66fd1699495fdec5dfdb93025768f87fe3782918dde` (o mesmo da E.5) | as expressões do DANFE em contingência (2.1 e 2.2): "DANFE em Contingência - impresso em decorrência de problemas técnicos" e "DANFE impresso em contingência - EPEC regularmente recebida pela Receita Federal do Brasil" |
| **NT 2026.010 v1.00, DANFE Reforma Tributária** (outubro de 2026; produção em 01/12/2026) | `ugn4MYF5PLg%3D` | 9 | `eadc5246235f37a55f8140ddcf84595c8ce77f2d760f111f7b85ddf389b8f484` | inteira, com os dois modelos (vertical e horizontal): o bloco "Total do IBS/CBS/IS", o CRT, as colunas da reforma, os campos facultativos, o QR Code |
| **NT 2026.003 v1.00, DANFE Simplificado Tipo 2** (maio de 2026; produção em 03/08/2026) | `2YdaBTIql%20k%3D` | 25 | `2ef826027e0e8423ba9dc2f53ee7197f8bfdcc399a30459b72da977528600f13` | inteira, com as figuras 1 a 6 e os três exemplos |
| **NT 2026.002 v1.11** (a NF-e com `tpImp` 6, a contingência offline) | `m8aACnFLK/Y%3D` | 24 | `72c033b2c7490710f61488a14d0bb3d7bb54e5345aa8d33d6ff2c270c228f046` (o mesmo da seção 1.3a) | só as linhas do `tpImp` (B21) e do QR Code da NF-e com o Tipo 2 |
| **NT 2020.004 v1.10, DANFE Simplificado - Etiqueta** (agosto de 2024) | `6VJK0RTUooA%3D` | 5 | `f2c422ed3490793d674475a7edee7cfc34bd3819c4b880f2d3ae39c16a409f8a` | inteira: os campos mínimos, sem leiaute (a Etiqueta **não está implementada**, `IMPRESSAO.md` 2.3) |
| **NT 2018.005 v1.52** (locais de retirada e de entrega, modalidade do frete) | `vZguLua3oPM%3D` | 28 | `c2c299ee59397a495c88b7b83826f39ca44e0d070bfeebbb9d9ff61d32a58ec7` | o capítulo 5, "Alteração do DANFE" (o quadro do transportador e as figuras dos locais) |
| **NT 2025.002 v1.52** (Reforma Tributária do Consumo) | `HXPO8VLbh4o%3D` | 94 | `54ed893327040a20a4a3c0562d342953542a1dc98e55d73dbe607d6c13d3d416` | só o capítulo 9, "DANFE": diz que as alterações do DANFE "estão em estudo" e serão publicadas à parte; a NT 2026.010 as publica |

- **O vigente:** o portal tem um só Anexo II (o MOC 7.0), e a NT 2026.010 é a única que altera o leiaute dele para o IBS, a CBS e o IS. A NT é de outubro de 2026 e **vigora em 01/12/2026**; a lib aplica os acréscimos **só quando o XML tem o dado**, então um DANFE de uma nota sem IBS/CBS/IS é o do Anexo II puro, e o de uma nota com eles já traz o que a NT manda (`IMPRESSAO.md` 2.1). **Divergência entre as duas fontes:** a ordem dos quadros (a NT põe o ISSQN antes do transportador) e as colunas dos itens (a NT junta e tira colunas que o Anexo II não deixa suprimir); a lib segue o Anexo II (o texto da NT chama o leiaute dela de "referência" e remete ao manual para os quadros suprimíveis).
- **Não lidas** (não alteram o que o DANFE imprime): a NT 2026.007 v1.10 e a 2026.008 v1.00 (regras de validação e leiaute de campos), a 2026.001 (PAA), a 2026.006 (split payment). A NT 2012.001 (a origem do DANFE Simplificado) está no portal e não foi aberta (o Anexo II a absorveu).
- **Ferramenta:** o `pdftotext` do poppler (`-enc UTF-8 -layout`) e o PyMuPDF para renderizar as páginas das figuras; nenhuma entra no repositório.

## 9. Impressão: o PDF (etapa 7.4, 2026-10-24)

A etapa não adotou fonte nova: conferiu os **itens de aparência** (dimensões, fontes, QR Code, código de barras, margens) das duas já usadas, **baixadas de novo** do Portal Nacional da NF-e (`curl -L -c cj -b cj`, como nas seções 7 e 8), convertidas com `pdftotext -layout` e lidas nos itens citados:

| Documento | Identificador (`conteudo=`) | SHA-256 | O que foi conferido na 7.4 |
|---|---|---|---|
| **Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0** (março de 2025) | `k/IuuaW4YiY%3D` | `bf906cc212f1edd19b1df7d1cdf4fbf5e73c3fec77567b6e69fa74453c95db5e` (1.887.904 bytes, o mesmo das seções 4.1 e 7) | 3.1.5 e 3.4 (o QR Code de no mínimo 25 x 25 mm, 22 mm de conteúdo e 3 mm de margem segura; acima de 25 mm, 10% da dimensão total), 3.3 (papel de no mínimo 56 mm, margens laterais de no mínimo 2 mm, o A4 permitido, nunca em ECF) e 4.2 (a imagem do QR Code de no mínimo 25 x 25 mm); **o manual não fixa fonte nem posição** (3.1.2) |
| **MOC 7.0, Anexo II: Manual de Especificações Técnicas do DANFE e Código de Barras** | `f%20NhsSn3/5M%3D` | `729c04fd1374188c29eb21ca60342741b24a014003d4c2ff1bbb455bb1a6a60e` (1.345.906 bytes, o mesmo da seção 8) | 2 (o código de barras: largura mínima de 6 cm, altura mínima da barra de 0,8 cm, módulo mínimo de 0,02 cm, margem clara de 10 módulos, as 297 posições), 3.6 (o papel e a margem de 0,2 a 0,8 cm) e 3.7 (as fontes: Times New Roman ou Courier New e os tamanhos mínimos de 5, 6, 8, 10 e 12 pt por bloco) |

- **A fonte do PDF** é a **Lato** (SIL OFL 1.1), embutida no QuestPDF 2026.9.1 (seção 6): o Anexo II pede Times New Roman ou Courier New (3.7), que não se embute (licença). O aplicativo que as tem as registra e passa `OpcoesPdf.Fonte` (README do pacote). A cobertura da Lato foi conferida caractere a caractere (`CONTRATOS.md` 22.4): o Latin-1 inteiro, que é o que o `TString` do schema da NF-e admite.
- **Nenhum pacote novo** entrou (QuestPDF, QRCoder e PdfPig são os da seção 6). A licença do QuestPDF **não foi relida** (o texto da 7.1, versão 3.0 de 6 de julho de 2026, segue o vigente no pacote 2026.9.1 usado).
- **Ferramentas de conferência** (fora do repositório): o `pdftotext` do poppler para ler os manuais e o PyMuPDF para renderizar os PDFs gerados em imagem e abri-los.

## 10. Distribuição DF-e (etapa 8.1, 2026-10-24)

- **NT 2014.002 v1.40** ("Web Service de Distribuição de DF-e de Interesse dos Atores da NF-e (PF ou PJ)", julho de 2026, 18 páginas, publicada em 03/07/2026), baixada de novo do Portal Nacional em 2026-10-24 (`listaConteudo.aspx?tipoConteudo=BMPFMBoln3w=`, `exibirArquivo.aspx?conteudo=uWO2d/gTuWg=`, cookie com `curl -c` e `-b`): SHA-256 `156dcb170e205c076a6da86aedf56a04afcec3aef8e373dd7fbf7ccb068da029`, **o mesmo da E.5** (seção 1.3a). Lida inteira. O que a 8.1 usa dela: o pedido `distDFeInt` (3.1: o `cUFAutor` opcional, o `CNPJ` ou o `CPF`, o `distNSU`, o `consNSU` e o `consChNFe`) e o retorno `retDistDFeInt` (3.2: `ultNSU` e `maxNSU` com ocorrência 0-1 na NT e obrigatórios no schema 1.01, o `docZip` de 1 a 50, o `NSU` facultativo); o gzip por documento (3.3); a tabela do item 3 (quem recebe o quê); a sequência de NSU sem lacunas e por CNPJ de 14 posições (3.4); o `distNSU` (3.5: lotes de até 50 em ordem de NSU, 90 dias, "aguardar um tempo mínimo de uma hora" no 137); o `consNSU` (3.6: o 137 é o NSU que o Ambiente Nacional não gerou); o `consChNFe` (3.7); o certificado pela raiz do CNPJ (3, 3.8); o limite de 10 KB do pedido (3.9); as regras H01 a H19 (3.10.2) e os leiautes resumidos (3.11.1 e 3.11.2: o `cSitNFe` 1, 2 ou 3; a `IE` "vazio, ISENTO ou a IE"); o uso indevido (3.11.4, abaixo); a tabela de códigos (4) e os exemplos (5 e 6, em SOAP 1.1: o envelope da lib é o 1.2).
- **O uso indevido, lido no texto (3.11.4):** (1) o 137 e nova consulta em menos de 1 hora é o 656, com o CNPJ bloqueado por 1 hora; (2) o `distNSU` fora da sequência, com outro `ultNSU` que não o retornado na consulta anterior, é o 656 e o mesmo bloqueio ("deve usar os valores do ultNSU retornados pelo serviço nas chamadas subsequentes"); várias aplicações do mesmo CNPJ de 14 posições têm de seguir **a mesma sequência**; (3) a observação da v1.14: o `ultNSU` da última consulta do CNPJ de 14 posições vai **no XML do 656** quando a consulta é o `distNSU`, "assim, caso o usuário não conheça o número do ultNSU, pode realizar a consulta a partir deste número"; (4) `consNSU` e `consChNFe`: o limite de 20 consultas por hora, a 21ª é o 656 ("ultrapassou o limite de 20 consultas por hora"); (5) em qualquer dos três tipos, **quem consulta antes de completar a hora do 656 zera o tempo**. Disto sai a recomendação da queda no meio em `docs/DISTRIBUICAO.md` 6.
- **Divergências entre a NT e o schema, conferidas** (a lib segue o texto da NT onde ele é mais largo, para não perder documento): `ultNSU` e `maxNSU` 0-1 na NT, obrigatórios no `retDistDFeInt_v1.01.xsd` (lidos como nulos se faltam, sem erro); a `IE` vazia no `resNFe` (C07) que o `TIe` não aceita; o `xNome` de 3 a 60 na NT e de 2 a 60 no schema; o exemplo 3 da NT escreve `loteDistDFInt` (sem o "e"), erro do texto: o schema e a lib usam `loteDistDFeInt`; os exemplos 2 e 3 do pedido fecham o `NSU` e o `chNFe` com `</ultNSU>`, também erro do texto; o pacote de schemas citado na NT é o `PL_NFeDistDFe_102`, e o embutido é o `PL_NFeDistDFe_104` (3 de julho de 2026, seção 1.1).
- **Schemas** (já embutidos, seção 1.1): `distDFeInt_v1.01.xsd` (valida o pedido na lib), `retDistDFeInt_v1.01.xsd`, `tiposDistDFe_v1.01.xsd`, `resNFe_v1.01.xsd` e `resEvento_v1.01.xsd` (só os testes os usam para conferir os documentos de exemplo; a leitura do retorno é branda, sem o schema), `procNFe_v4.00.xsd` e `procEventoNFe_v1.00.xsd`.
- **URLs do AN:** seção 3.2 (a "Relação de Serviços Web", produção e homologação, 2026-10-24).
