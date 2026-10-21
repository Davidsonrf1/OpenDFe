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

## 2. Como atualizar

1. Na lista de Esquemas XML do portal, o primeiro pacote de "VERSÕES OFICIAIS (em uso)" que traz a nota; ler o título (as NTs) e a data.
2. Baixar o ZIP numa pasta vazia, conferir os arquivos e comparar com a pasta atual (o que mudou e se é cumulativo).
3. Criar a pasta nova em `Schemas/` com os arquivos do pacote e os que ele não republica (a mensagem mais recente publicada de cada um), apagar a antiga e trocar `SchemasNFe400.Pacote`.
4. Atualizar as tabelas daqui (arquivos, SHA-256, NTs, vigência, versão da OpenDFe) e a tabela do teste `SchemasNFe400Testes`.
5. NT nova sem quebra é versão menor (`PROJETO.md` 12.3).
