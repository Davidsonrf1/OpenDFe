# Namespace OpenDFe.NFe: modelo (classes)

As classes do modelo da NF-e e da NFC-e, com a tag do XML e o identificador do MOC de cada propriedade. Pacote NuGet: `OpenDFe.NFe`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`Adicao`](#adicao) | Uma adição da declaração de importação (`adi`). |
| [`Agropecuario`](#agropecuario) | Os produtos agropecuários (`agropecuario`): os defensivos ou a guia de trânsito. |
| [`AjusteCompetencia`](#ajustecompetencia) | O ajuste de competência do IBS e da CBS (`gAjusteCompet`, CST 811). |
| [`AquisicaoCana`](#aquisicaocana) | A aquisição de cana (`cana`). |
| [`Arma`](#arma) | Uma arma de fogo (`arma`). |
| [`AssinaturaProvedor`](#assinaturaprovedor) | A assinatura do provedor de assinatura e autorização (`PAASignature`). |
| [`CTeReferenciado`](#ctereferenciado) | Um CT-e referenciado pela chave de acesso (`refCTe`). |
| [`Cbs`](#cbs) | A CBS (`gCBS`). |
| [`CbsAreaIncentivada`](#cbsareaincentivada) | A CBS com alíquota zero em área incentivada, ALC ou ZFM (`gALCZFMCBS`). |
| [`ChavePublicaRsa`](#chavepublicarsa) | A chave pública RSA do provedor de assinatura (`RSAKeyValue`). |
| [`Cide`](#cide) | A CIDE dos combustíveis (`CIDE`). |
| [`Cobranca`](#cobranca) | A cobrança (`cobr`). |
| [`Cofins`](#cofins) | A COFINS do item (`COFINS`): exatamente um dos grupos do XML, uma subclasse para cada. |
| [`CofinsAliquota`](#cofinsaliquota) | A COFINS tributada pela alíquota (`COFINSAliq`, CST 01 ou 02). |
| [`CofinsNaoTributado`](#cofinsnaotributado) | A COFINS não tributada (`COFINSNT`, CST 04 a 09). |
| [`CofinsOutros`](#cofinsoutros) | A COFINS de outras operações (`COFINSOutr`, CST 49 a 99). |
| [`CofinsQuantidade`](#cofinsquantidade) | A COFINS tributada por quantidade (`COFINSQtde`, CST 03). |
| [`CofinsSt`](#cofinsst) | A COFINS por substituição tributária (`COFINSST`). |
| [`Combustivel`](#combustivel) | O combustível líquido ou gasoso (`comb`). |
| [`Compra`](#compra) | As informações de compra (`compra`). |
| [`CompraGovernamental`](#compragovernamental) | A compra governamental (`gCompraGov`, NT 2025.002). |
| [`CreditoPresumidoIbsCbs`](#creditopresumidoibscbs) | O crédito presumido de um tributo da reforma (`gIBSCredPres` ou `gCBSCredPres`). |
| [`CreditoPresumidoIcms`](#creditopresumidoicms) | Um crédito presumido do ICMS do produto (`gCred`). |
| [`CreditoPresumidoOperacao`](#creditopresumidooperacao) | O crédito presumido da operação no IBS e na CBS (`gCredPresOper`). |
| [`CreditoPresumidoZfm`](#creditopresumidozfm) | O crédito presumido do IBS na Zona Franca de Manaus (`gCredPresIBSZFM`). |
| [`CupomFiscalReferenciado`](#cupomfiscalreferenciado) | Um cupom fiscal referenciado (`refECF`). |
| [`DeclaracaoImportacao`](#declaracaoimportacao) | Uma declaração de importação do produto (`DI`). |
| [`Deducao`](#deducao) | Uma dedução da aquisição de cana (`deduc`). |
| [`Defensivo`](#defensivo) | Um defensivo agrícola (`defensivo`). |
| [`Destinatario`](#destinatario) | O destinatário (`dest`). |
| [`DetalheExportacao`](#detalheexportacao) | Um detalhe da exportação do produto (`detExport`). |
| [`DevolucaoTributo`](#devolucaotributo) | A devolução de um tributo da reforma (`gDevTrib`). |
| [`Diferimento`](#diferimento) | O diferimento de um tributo da reforma (`gDif`). |
| [`DocumentoReferenciado`](#documentoreferenciado) | Um documento referenciado (`NFref`): exatamente uma das escolhas do XML, uma subclasse para cada. |
| [`Duplicata`](#duplicata) | Uma duplicata (`dup`). |
| [`EmbalagemProduto`](#embalagemproduto) | A embalagem do produto (`infProdEmb`). |
| [`Emitente`](#emitente) | O emitente (`emit`). |
| [`Encerrante`](#encerrante) | O encerrante do abastecimento (`encerrante`). |
| [`EnderecoDestinatario`](#enderecodestinatario) | O endereço do destinatário (`enderDest`, o tipo `TEndereco`): aceita o exterior. |
| [`EnderecoEmitente`](#enderecoemitente) | O endereço do emitente (`enderEmit`, o tipo `TEnderEmi`): só UF do Brasil. |
| [`EstornoCredito`](#estornocredito) | O estorno de crédito do IBS e da CBS (`gEstornoCred`). |
| [`Exportacao`](#exportacao) | A exportação (`exporta`). |
| [`ExportacaoIndireta`](#exportacaoindireta) | A exportação indireta (`exportInd`). |
| [`Fatura`](#fatura) | A fatura (`fat`). |
| [`FiscoEmitente`](#fiscoemitente) | O fisco que emitiu a nota avulsa (`avulsa`). |
| [`FormaPagamento`](#formapagamento) | Uma forma de pagamento (`detPag`). |
| [`FornecimentoDiario`](#fornecimentodiario) | O fornecimento de cana de um dia (`forDia`). |
| [`GuiaTransito`](#guiatransito) | A guia de trânsito de produtos agropecuários (`guiaTransito`). |
| [`IbsCbs`](#ibscbs) | O IBS e a CBS do item (`IBSCBS`, NT 2025.002). |
| [`IbsMunicipio`](#ibsmunicipio) | O IBS do município (`gIBSMun`). |
| [`IbsUF`](#ibsuf) | O IBS da UF (`gIBSUF`). |
| [`Icms`](#icms) | O ICMS do item (`ICMS`): exatamente um dos grupos do XML, uma subclasse para cada. |
| [`Icms00`](#icms00) | O ICMS tributado integralmente (`ICMS00`, CST 00). |
| [`Icms02`](#icms02) | O ICMS monofásico próprio sobre combustíveis (`ICMS02`, CST 02). |
| [`Icms10`](#icms10) | O ICMS tributado e com cobrança por substituição tributária (`ICMS10`, CST 10). |
| [`Icms15`](#icms15) | O ICMS monofásico próprio e com responsabilidade pela retenção sobre combustíveis (`ICMS15`, CST 15). |
| [`Icms20`](#icms20) | O ICMS com redução de base de cálculo (`ICMS20`, CST 20). |
| [`Icms30`](#icms30) | O ICMS isento ou não tributado e com cobrança por substituição tributária (`ICMS30`, CST 30). |
| [`Icms40`](#icms40) | O ICMS isento, não tributado ou suspenso (`ICMS40`, CST 40, 41 ou 50). |
| [`Icms51`](#icms51) | O ICMS com diferimento (`ICMS51`, CST 51). |
| [`Icms53`](#icms53) | O ICMS monofásico sobre combustíveis com recolhimento diferido (`ICMS53`, CST 53). |
| [`Icms60`](#icms60) | O ICMS cobrado anteriormente por substituição tributária (`ICMS60`, CST 60). |
| [`Icms61`](#icms61) | O ICMS monofásico sobre combustíveis cobrado anteriormente (`ICMS61`, CST 61). |
| [`Icms70`](#icms70) | O ICMS com redução de base de cálculo e cobrança por substituição tributária (`ICMS70`, CST 70). |
| [`Icms90`](#icms90) | O ICMS de outras situações (`ICMS90`, CST 90). |
| [`IcmsPartilha`](#icmspartilha) | A partilha do ICMS entre a UF de origem e a de destino, ou a definida na legislação (`ICMSPart`, CST 10 ou 90). |
| [`IcmsRepasse`](#icmsrepasse) | O repasse do ICMS ST retido anteriormente em operações interestaduais (`ICMSST`, CST 41 ou 60). |
| [`IcmsSn101`](#icmssn101) | O ICMS do Simples Nacional com permissão de crédito (`ICMSSN101`, CSOSN 101). |
| [`IcmsSn102`](#icmssn102) | O ICMS do Simples Nacional sem permissão de crédito, isento, imune ou não tributado (`ICMSSN102`, CSOSN 102, 103, 300 ou 400). |
| [`IcmsSn201`](#icmssn201) | O ICMS do Simples Nacional com permissão de crédito e cobrança por substituição tributária (`ICMSSN201`, CSOSN 201). |
| [`IcmsSn202`](#icmssn202) | O ICMS do Simples Nacional sem permissão de crédito e com cobrança por substituição tributária (`ICMSSN202`, CSOSN 202 ou 203). |
| [`IcmsSn500`](#icmssn500) | O ICMS do Simples Nacional cobrado anteriormente por substituição tributária ou por antecipação (`ICMSSN500`, CSOSN 500). |
| [`IcmsSn900`](#icmssn900) | O ICMS do Simples Nacional de outras situações (`ICMSSN900`, CSOSN 900). |
| [`IcmsUFDestino`](#icmsufdestino) | O ICMS de partilha para a UF do destinatário, na operação interestadual com consumidor final não contribuinte (`ICMSUFDest`, EC 87/2015). |
| [`Identificacao`](#identificacao) | A identificação da nota (`ide`). |
| [`Imposto`](#imposto) | Os tributos do item (`imposto`). |
| [`ImpostoDevolvido`](#impostodevolvido) | O imposto devolvido do item (`impostoDevol`). |
| [`ImpostoImportacao`](#impostoimportacao) | O imposto de importação do item (`II`). |
| [`ImpostoSeletivo`](#impostoseletivo) | O imposto seletivo do item (`IS`, NT 2025.002). |
| [`InformacoesAdicionais`](#informacoesadicionais) | As informações adicionais (`infAdic`). |
| [`InformacoesSuplementares`](#informacoessuplementares) | As informações suplementares da NFC-e (`infNFeSupl`). |
| [`Intermediador`](#intermediador) | O intermediador da transação (`infIntermed`): a plataforma de terceiros. |
| [`Ipi`](#ipi) | O IPI do item (`IPI`). |
| [`IpiDevolvido`](#ipidevolvido) | O IPI devolvido (`impostoDevol/IPI`). |
| [`IpiNaoTributado`](#ipinaotributado) | O IPI não tributado (`IPINT`). |
| [`IpiTributado`](#ipitributado) | O IPI tributado (`IPITrib`). |
| [`Issqn`](#issqn) | O ISSQN do item (`ISSQN`). |
| [`Item`](#item) | Um item da nota (`det`): o produto ou serviço, os tributos e o que vem com eles. |
| [`ItemReferenciado`](#itemreferenciado) | O item de outro documento fiscal a que este item se refere (`DFeReferenciado`). |
| [`Lacre`](#lacre) | Um lacre de volume (`lacres`). |
| [`Local`](#local) | Um local de retirada (`retirada`) ou de entrega (`entrega`), o tipo `TLocal`. |
| [`Medicamento`](#medicamento) | O medicamento ou a matéria-prima farmacêutica (`med`). |
| [`MensagemSefaz`](#mensagemsefaz) | Uma mensagem da SEFAZ ao emissor, no protocolo (`cMsg` e `xMsg`). Sem grupo no XML: a sequência dos dois campos se repete dentro do `infProt`. |
| [`MonofasiaCbsAdRem`](#monofasiacbsadrem) | A CBS monofásica ad rem (`gCBSMonoAdRem`). |
| [`MonofasiaCbsAdRemPadrao`](#monofasiacbsadrempadrao) | A tributação monofásica padrão da CBS ad rem (`gMonoPadrao`). |
| [`MonofasiaCbsAdRemRetencao`](#monofasiacbsadremretencao) | A tributação monofásica sujeita à retenção da CBS ad rem (`gMonoReten`). |
| [`MonofasiaCbsAdValorem`](#monofasiacbsadvalorem) | A CBS monofásica ad valorem (`gCBSMonoAdValorem`). |
| [`MonofasiaCbsAdValoremPadrao`](#monofasiacbsadvalorempadrao) | A tributação monofásica padrão da CBS ad valorem (`gMonoPadrao`). |
| [`MonofasiaCbsAdValoremRetencao`](#monofasiacbsadvaloremretencao) | A tributação monofásica sujeita à retenção da CBS ad valorem (`gMonoReten`). |
| [`MonofasiaCbsBiocombustivel`](#monofasiacbsbiocombustivel) | A diferença da CBS pela mistura de biocombustível (EAC) com a gasolina A em percentual diferente do obrigatório (`gpBioDiferenca`). |
| [`MonofasiaCbsRetido`](#monofasiacbsretido) | A CBS monofásica retida anteriormente (`gMonoRet`). |
| [`MonofasiaIbsAdRem`](#monofasiaibsadrem) | O IBS monofásico ad rem (`gIBSMonoAdRem`). |
| [`MonofasiaIbsAdRemPadrao`](#monofasiaibsadrempadrao) | A tributação monofásica padrão do IBS ad rem (`gMonoPadrao`). |
| [`MonofasiaIbsAdRemRetencao`](#monofasiaibsadremretencao) | A tributação monofásica sujeita à retenção do IBS ad rem (`gMonoReten`). |
| [`MonofasiaIbsAdValorem`](#monofasiaibsadvalorem) | O IBS monofásico ad valorem (`gIBSMonoAdValorem`). |
| [`MonofasiaIbsAdValoremPadrao`](#monofasiaibsadvalorempadrao) | A tributação monofásica padrão do IBS ad valorem (`gMonoPadrao`). |
| [`MonofasiaIbsAdValoremRetencao`](#monofasiaibsadvaloremretencao) | A tributação monofásica sujeita à retenção do IBS ad valorem (`gMonoReten`). |
| [`MonofasiaIbsBiocombustivel`](#monofasiaibsbiocombustivel) | A diferença do IBS pela mistura de biocombustível (EAC) com a gasolina A em percentual diferente do obrigatório (`gpBioDiferenca`). |
| [`MonofasiaIbsCbs`](#monofasiaibscbs) | A tributação monofásica de combustíveis no IBS e na CBS (`gIBSCBSMono`). |
| [`MonofasiaIbsRetido`](#monofasiaibsretido) | O IBS monofásico retido anteriormente (`gMonoRet`). |
| [`NFeReferenciada`](#nfereferenciada) | Uma NF-e ou NFC-e referenciada pela chave de acesso (`refNFe`). |
| [`NFeSigilosaReferenciada`](#nfesigilosareferenciada) | Uma NF-e referenciada pela chave com o código numérico zerado, para manter o sigilo dela (`refNFeSig`). |
| [`NotaFiscal`](#notafiscal) | Uma NF-e (modelo 55) ou NFC-e (modelo 65): tudo o que vai dentro do `infNFe` e do `infNFeSupl`. |
| [`NotaModelo1Referenciada`](#notamodelo1referenciada) | Uma nota fiscal modelo 1 ou 2 referenciada (`refNF`). |
| [`NotaProdutorReferenciada`](#notaprodutorreferenciada) | Uma nota fiscal de produtor rural referenciada (`refNFP`). |
| [`Observacao`](#observacao) | Uma observação de uso livre: do contribuinte (`obsCont`) ou do fisco (`obsFisco`), na nota ou no item. |
| [`ObservacoesItem`](#observacoesitem) | As observações do item (`obsItem`). |
| [`OrigemCombustivel`](#origemcombustivel) | Uma origem do combustível (`origComb`). |
| [`Pagamento`](#pagamento) | O pagamento (`pag`). |
| [`PagamentoAntecipado`](#pagamentoantecipado) | As NF-e de antecipação de pagamento que esta abate (`gPagAntecipado`, NT 2025.002). |
| [`PessoaAutorizada`](#pessoaautorizada) | Uma pessoa autorizada a obter o XML da nota (`autXML`). |
| [`Pis`](#pis) | O PIS do item (`PIS`): exatamente um dos grupos do XML, uma subclasse para cada. |
| [`PisAliquota`](#pisaliquota) | O PIS tributado pela alíquota (`PISAliq`, CST 01 ou 02). |
| [`PisNaoTributado`](#pisnaotributado) | O PIS não tributado (`PISNT`, CST 04 a 09). |
| [`PisOutros`](#pisoutros) | O PIS de outras operações (`PISOutr`, CST 49 a 99). |
| [`PisQuantidade`](#pisquantidade) | O PIS tributado por quantidade (`PISQtde`, CST 03). |
| [`PisSt`](#pisst) | O PIS por substituição tributária (`PISST`). |
| [`ProcessoReferenciado`](#processoreferenciado) | Um processo referenciado (`procRef`). |
| [`Produto`](#produto) | O produto ou serviço do item (`prod`). |
| [`ProdutoNff`](#produtonff) | As informações do produto na NFF, nota fiscal fácil (`infProdNFF`). |
| [`Protocolo`](#protocolo) | O protocolo do processamento de uma NF-e pela SEFAZ (`protNFe`, com o `infProt` achatado): autorizada, denegada ou rejeitada. |
| [`ProvedorAssinatura`](#provedorassinatura) | O provedor de assinatura e autorização, PAA (`infPAA`): a nota gerada por ele ([`ProvedorAssinatura`](opendfe-nfe-modelo-enumeracoes.md#processoemissao)). |
| [`Rastro`](#rastro) | Um lote do produto, para a rastreabilidade (`rastro`). |
| [`ReducaoAliquota`](#reducaoaliquota) | A redução da alíquota de um tributo da reforma (`gRed`). |
| [`ResponsavelTecnico`](#responsaveltecnico) | O responsável técnico pelo sistema emissor (`infRespTec`). |
| [`RetencaoIcmsTransporte`](#retencaoicmstransporte) | A retenção do ICMS do transporte (`retTransp`). |
| [`RetencaoTributos`](#retencaotributos) | As retenções de tributos (`retTrib`). |
| [`SolicitacaoNff`](#solicitacaonff) | A solicitação da NFF, nota fiscal fácil (`infSolicNFF`). |
| [`Total`](#total) | Os totais da nota (`total`). |
| [`TotalCbs`](#totalcbs) | Os totais da CBS (`gCBS`). |
| [`TotalIbs`](#totalibs) | Os totais do IBS (`gIBS`). |
| [`TotalIbsCbs`](#totalibscbs) | Os totais do IBS e da CBS (`IBSCBSTot`). |
| [`TotalIbsMunicipio`](#totalibsmunicipio) | Os totais do IBS do município (`gIBSMun`). |
| [`TotalIbsUF`](#totalibsuf) | Os totais do IBS da UF (`gIBSUF`). |
| [`TotalIcms`](#totalicms) | Os totais do ICMS e dos valores da nota (`ICMSTot`). |
| [`TotalImpostoSeletivo`](#totalimpostoseletivo) | O total do imposto seletivo (`ISTot`). |
| [`TotalIssqn`](#totalissqn) | Os totais do ISSQN (`ISSQNtot`). |
| [`TotalMonofasia`](#totalmonofasia) | Os totais da tributação monofásica do IBS e da CBS (`gMono`). |
| [`TransacaoEletronica`](#transacaoeletronica) | Os dados de um pagamento eletrônico (`card`): cartões, PIX, boletos e outros. |
| [`TransferenciaCredito`](#transferenciacredito) | A transferência de crédito do IBS e da CBS (`gTransfCred`, CST 800). |
| [`Transportador`](#transportador) | O transportador (`transporta`). |
| [`Transporte`](#transporte) | O transporte (`transp`). |
| [`TributacaoCompraGovernamental`](#tributacaocompragovernamental) | A composição do IBS e da CBS na compra governamental (`gTribCompraGov`). |
| [`TributacaoIbsCbs`](#tributacaoibscbs) | A tributação do IBS e da CBS (`gIBSCBS`). |
| [`TributacaoRegular`](#tributacaoregular) | A tributação regular, como seria se não cumprida a condição resolutória ou suspensiva (`gTribRegular`). |
| [`Veiculo`](#veiculo) | Um veículo (o tipo `TVeiculo`): o de tração (`veicTransp`) ou um reboque (`reboque`). |
| [`VeiculoNovo`](#veiculonovo) | O veículo novo (`veicProd`). |
| [`Volume`](#volume) | Um volume transportado (`vol`). |

## Adicao

Classe selada · Namespace `OpenDFe.NFe`

Uma adição da declaração de importação (`adi`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Adicao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `int?` | `nAdicao` | I26 | O número da adição; opcional. |
| `Sequencial` | `int?` | `nSeqAdic` | I27 | O número sequencial do item na adição. |
| `CodigoFabricante` | `string?` | `cFabricante` | I28 | O código do fabricante estrangeiro. |
| `Desconto` | `decimal?` | `vDescDI` | I29 | O valor do desconto do item na DI; opcional. |
| `Drawback` | `string?` | `nDraw` | I29a | O número do ato concessório de drawback; opcional. |

## Agropecuario

Classe selada · Namespace `OpenDFe.NFe`

Os produtos agropecuários (`agropecuario`): os defensivos ou a guia de trânsito.

### Construtores

| Construtor | Descrição |
|---|---|
| `Agropecuario()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Defensivos` | `IList<Defensivo>` | `defensivo` | ZF02 | Os defensivos (até 20); exclusivos com a [`GuiaTransito`](#agropecuario). |
| `GuiaTransito` | `GuiaTransito?` | `guiaTransito` | ZF04 | A guia de trânsito; exclusiva com os [`Defensivos`](#agropecuario). |

## AjusteCompetencia

Classe selada · Namespace `OpenDFe.NFe`

O ajuste de competência do IBS e da CBS (`gAjusteCompet`, CST 811).

### Construtores

| Construtor | Descrição |
|---|---|
| `AjusteCompetencia()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Competencia` | `string?` | `competApur` | UB113 | O ano e o mês da competência de apuração, como no XML (`AAAA-MM`). |
| `ValorIbs` | `decimal?` | `vIBS` | UB114 | O valor do IBS do ajuste. |
| `ValorCbs` | `decimal?` | `vCBS` | UB115 | O valor da CBS do ajuste. |

## AquisicaoCana

Classe selada · Namespace `OpenDFe.NFe`

A aquisição de cana (`cana`).

### Construtores

| Construtor | Descrição |
|---|---|
| `AquisicaoCana()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Safra` | `string?` | `safra` | ZC02 | A safra, como no XML (`AAAA` ou `AAAA/AAAA`). |
| `Referencia` | `string?` | `ref` | ZC03 | O mês e o ano de referência, como no XML (`MM/AAAA`). |
| `Fornecimentos` | `IList<FornecimentoDiario>` | `forDia` | ZC04 | O fornecimento diário (de 1 a 31). |
| `QuantidadeMes` | `decimal?` | `qTotMes` | ZC07 | A quantidade total do mês. |
| `QuantidadeAnterior` | `decimal?` | `qTotAnt` | ZC08 | A quantidade total anterior. |
| `QuantidadeGeral` | `decimal?` | `qTotGer` | ZC09 | A quantidade total geral. |
| `Deducoes` | `IList<Deducao>` | `deduc` | ZC10 | As deduções (até 10). |
| `ValorFornecimentos` | `decimal?` | `vFor` | ZC13 | O valor dos fornecimentos. |
| `ValorDeducoes` | `decimal?` | `vTotDed` | ZC14 | O valor total das deduções. |
| `ValorLiquido` | `decimal?` | `vLiqFor` | ZC15 | O valor líquido dos fornecimentos. |

## Arma

Classe selada · Namespace `OpenDFe.NFe`

Uma arma de fogo (`arma`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Arma()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Tipo` | `TipoArma?` | `tpArma` | L02 | O tipo da arma de fogo. |
| `Serie` | `string?` | `nSerie` | L03 | O número de série da arma. |
| `SerieCano` | `string?` | `nCano` | L04 | O número de série do cano. |
| `Descricao` | `string?` | `descr` | L05 | A descrição completa da arma (calibre, marca, capacidade e afins). |

## AssinaturaProvedor

Classe selada · Namespace `OpenDFe.NFe`

A assinatura do provedor de assinatura e autorização (`PAASignature`).

### Construtores

| Construtor | Descrição |
|---|---|
| `AssinaturaProvedor()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Valor` | `string?` | `SignatureValue` | ZG04 | A assinatura, em base64. |
| `ChavePublica` | `ChavePublicaRsa` | `RSAKeyValue` | ZG05 | A chave pública RSA do provedor. |

## CTeReferenciado

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Um CT-e referenciado pela chave de acesso (`refCTe`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CTeReferenciado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Chave` | `string?` | `refCTe` | BA19 | A chave de acesso do CT-e. |

## Cbs

Classe selada · Namespace `OpenDFe.NFe`

A CBS (`gCBS`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Cbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Aliquota` | `decimal?` | `pCBS` | UB56 | A alíquota da CBS. |
| `Diferimento` | `Diferimento?` | `gDif` | UB59 | O diferimento; opcional. |
| `Devolucao` | `DevolucaoTributo?` | `gDevTrib` | UB62 | A devolução de tributo; opcional. |
| `Reducao` | `ReducaoAliquota?` | `gRed` | UB64 | A redução da alíquota; opcional. |
| `AreaIncentivada` | `CbsAreaIncentivada?` | `gALCZFMCBS` | UB66a | A CBS com alíquota zero em área incentivada (ALC ou ZFM); opcional. |
| `Valor` | `decimal?` | `vCBS` | UB67 | O valor da CBS. |

## CbsAreaIncentivada

Classe selada · Namespace `OpenDFe.NFe`

A CBS com alíquota zero em área incentivada, ALC ou ZFM (`gALCZFMCBS`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CbsAreaIncentivada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Tipo` | `AplicacaoAliquotaZeroCbs?` | `tpALCZFMCBS` | UB66b | O tipo de aplicação da alíquota zero da CBS. |
| `ProcessoSuframa` | `string?` | `nProcSuframa` | UB66c | O número do processo na SUFRAMA para o item; opcional. |
| `AliquotaEfetiva` | `decimal?` | `pAliqEfetRegCBS` | UB66d | A alíquota efetiva de referência da CBS fora das áreas incentivadas. |
| `Valor` | `decimal?` | `vTribRegCBS` | UB66e | O valor da CBS calculado como fora das áreas incentivadas. |

## ChavePublicaRsa

Classe selada · Namespace `OpenDFe.NFe`

A chave pública RSA do provedor de assinatura (`RSAKeyValue`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ChavePublicaRsa()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Modulo` | `string?` | `Modulus` | ZG06 | O módulo, em base64. |
| `Expoente` | `string?` | `Exponent` | ZG07 | O expoente, em base64 (em geral `AQAB`). |

## Cide

Classe selada · Namespace `OpenDFe.NFe`

A CIDE dos combustíveis (`CIDE`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Cide()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `qBCProd` | LA08 | A base de cálculo da CIDE: a quantidade comercializada. |
| `Aliquota` | `decimal?` | `vAliqProd` | LA09 | A alíquota da CIDE, em reais por unidade. |
| `Valor` | `decimal?` | `vCIDE` | LA10 | O valor da CIDE. |

## Cobranca

Classe selada · Namespace `OpenDFe.NFe`

A cobrança (`cobr`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Cobranca()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Fatura` | `Fatura?` | `fat` | Y02 | A fatura; opcional. |
| `Duplicatas` | `IList<Duplicata>` | `dup` | Y07 | As duplicatas (até 120). |

## Cofins

Classe abstrata · Namespace `OpenDFe.NFe`

A COFINS do item (`COFINS`): exatamente um dos grupos do XML, uma subclasse para cada.

Hierarquia fechada: só a lib deriva; quem faz `switch` pelo tipo precisa de um caso padrão. O CST está em todos os grupos, com a mesma tag e o mesmo identificador, e fica aqui.

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstPisCofins?` | `CST` | S06 | O CST, o mesmo campo em todos os grupos. Nulo na escrita, no [`CofinsQuantidade`](#cofinsquantidade): o único valor dele (`03`). |

## CofinsAliquota

Classe selada · Tag `COFINSAliq` (S02) · Namespace `OpenDFe.NFe` · Deriva de [`Cofins`](#cofins)

A COFINS tributada pela alíquota (`COFINSAliq`, CST 01 ou 02).

### Construtores

| Construtor | Descrição |
|---|---|
| `CofinsAliquota()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | S07 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pCOFINS` | S08 | A alíquota, em percentual. |
| `Valor` | `decimal?` | `vCOFINS` | S11 | O valor da COFINS. |

## CofinsNaoTributado

Classe selada · Tag `COFINSNT` (S04) · Namespace `OpenDFe.NFe` · Deriva de [`Cofins`](#cofins)

A COFINS não tributada (`COFINSNT`, CST 04 a 09).

Só o [`Cst`](#cofins).

### Construtores

| Construtor | Descrição |
|---|---|
| `CofinsNaoTributado()` |  |

## CofinsOutros

Classe selada · Tag `COFINSOutr` (S05) · Namespace `OpenDFe.NFe` · Deriva de [`Cofins`](#cofins)

A COFINS de outras operações (`COFINSOutr`, CST 49 a 99).

A base e a alíquota ([`BaseCalculo`](#cofinsoutros) e [`Aliquota`](#cofinsoutros)) ou a quantidade e o valor por unidade ([`Quantidade`](#cofinsoutros) e [`AliquotaUnidade`](#cofinsoutros)): escolha do XSD.

### Construtores

| Construtor | Descrição |
|---|---|
| `CofinsOutros()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | S07 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pCOFINS` | S08 | A alíquota, em percentual; opcional. |
| `Quantidade` | `decimal?` | `qBCProd` | S09 | A quantidade vendida; opcional. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | S10 | A alíquota em reais, por unidade; opcional. |
| `Valor` | `decimal?` | `vCOFINS` | S11 | O valor da COFINS. |

## CofinsQuantidade

Classe selada · Tag `COFINSQtde` (S03) · Namespace `OpenDFe.NFe` · Deriva de [`Cofins`](#cofins)

A COFINS tributada por quantidade (`COFINSQtde`, CST 03).

### Construtores

| Construtor | Descrição |
|---|---|
| `CofinsQuantidade()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCProd` | S09 | A quantidade vendida. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | S10 | A alíquota em reais, por unidade. |
| `Valor` | `decimal?` | `vCOFINS` | S11 | O valor da COFINS. |

## CofinsSt

Classe selada · Namespace `OpenDFe.NFe`

A COFINS por substituição tributária (`COFINSST`).

A base e a alíquota ([`BaseCalculo`](#cofinsst) e [`Aliquota`](#cofinsst)) ou a quantidade e o valor por unidade ([`Quantidade`](#cofinsst) e [`AliquotaUnidade`](#cofinsst)): escolha do XSD.

### Construtores

| Construtor | Descrição |
|---|---|
| `CofinsSt()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | T02 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pCOFINS` | T03 | A alíquota, em percentual; opcional. |
| `Quantidade` | `decimal?` | `qBCProd` | T04 | A quantidade vendida; opcional. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | T05 | A alíquota em reais, por unidade; opcional. |
| `Valor` | `decimal?` | `vCOFINS` | T06 | O valor da COFINS. |
| `Totalizacao` | `IndicadorTotal?` | `indSomaCOFINSST` | T07 | Se o valor da COFINS ST compõe o total da nota; opcional. |

## Combustivel

Classe selada · Namespace `OpenDFe.NFe`

O combustível líquido ou gasoso (`comb`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Combustivel()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `CodigoAnp` | `string?` | `cProdANP` | LA02 | O código do produto na ANP. |
| `DescricaoAnp` | `string?` | `descANP` | LA03 | A descrição do produto na ANP. |
| `PercentualGlp` | `decimal?` | `pGLP` | LA03a | O percentual do GLP derivado do petróleo; opcional. |
| `PercentualGasNacional` | `decimal?` | `pGNn` | LA03b | O percentual de gás natural nacional; opcional. |
| `PercentualGasImportado` | `decimal?` | `pGNi` | LA03c | O percentual de gás natural importado; opcional. |
| `ValorPartida` | `decimal?` | `vPart` | LA03d | O valor de partida, por quilograma, sem ICMS; opcional. |
| `Codif` | `string?` | `CODIF` | LA04 | O código de autorização ou registro do CODIF; opcional. |
| `QuantidadeTemperaturaAmbiente` | `decimal?` | `qTemp` | LA05 | A quantidade faturada à temperatura ambiente; opcional. |
| `UFConsumo` | `UF?` | `UFCons` | LA06 | A UF de consumo. |
| `Exterior` | `bool` |  |  | Se a UF de consumo do XML é `EX` (exterior); com ela, [`UFConsumo`](#combustivel) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UFCons`. |
| `Cide` | `Cide?` | `CIDE` | LA07 | A CIDE; opcional. |
| `Encerrante` | `Encerrante?` | `encerrante` | LA11 | O encerrante; opcional. |
| `PercentualBiodiesel` | `decimal?` | `pBio` | LA17 | O percentual do índice de mistura do biodiesel; opcional. |
| `Origens` | `IList<OrigemCombustivel>` | `origComb` | LA18 | As origens do combustível (até 30). |

## Compra

Classe selada · Namespace `OpenDFe.NFe`

As informações de compra (`compra`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Compra()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `NotaEmpenho` | `string?` | `xNEmp` | ZB02 | A nota de empenho (compras públicas); opcional. |
| `Pedido` | `string?` | `xPed` | ZB03 | O pedido; opcional. |
| `Contrato` | `string?` | `xCont` | ZB04 | O contrato; opcional. |

## CompraGovernamental

Classe selada · Namespace `OpenDFe.NFe`

A compra governamental (`gCompraGov`, NT 2025.002).

### Construtores

| Construtor | Descrição |
|---|---|
| `CompraGovernamental()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Ente` | `EnteGovernamental?` | `tpEnteGov` | BB02 | O tipo do ente governamental. |
| `PercentualReducao` | `decimal?` | `pRedutor` | BB03 | O percentual de redução da alíquota. |
| `Operacao` | `OperacaoGovernamental?` | `tpOperGov` | BB04 | O tipo da operação com o ente governamental. |
| `DocumentosAnteriores` | `IList<string>` | `refDFeAnt` | BB05 | As chaves dos documentos anteriores (até 99). |

## CreditoPresumidoIbsCbs

Classe selada · Namespace `OpenDFe.NFe`

O crédito presumido de um tributo da reforma (`gIBSCredPres` ou `gCBSCredPres`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. O [`Valor`](#creditopresumidoibscbs) e o [`ValorCondicaoSuspensiva`](#creditopresumidoibscbs) são uma escolha do XSD: um dos dois.

### Construtores

| Construtor | Descrição |
|---|---|
| `CreditoPresumidoIbsCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Percentual` | `decimal?` | `pCredPres` |  | O percentual do crédito presumido. |
| `Valor` | `decimal?` | `vCredPres` |  | O valor do crédito presumido; ou o [`ValorCondicaoSuspensiva`](#creditopresumidoibscbs) (um dos dois). |
| `ValorCondicaoSuspensiva` | `decimal?` | `vCredPresCondSus` |  | O valor do crédito presumido em condição suspensiva; ou o [`Valor`](#creditopresumidoibscbs) (um dos dois). |

## CreditoPresumidoIcms

Classe selada · Namespace `OpenDFe.NFe`

Um crédito presumido do ICMS do produto (`gCred`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CreditoPresumidoIcms()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Codigo` | `string?` | `cCredPresumido` | I05h | O código de benefício fiscal do crédito presumido. |
| `Percentual` | `decimal?` | `pCredPresumido` | I05i | O percentual do crédito presumido. |
| `Valor` | `decimal?` | `vCredPresumido` | I05j | O valor do crédito presumido. |

## CreditoPresumidoOperacao

Classe selada · Namespace `OpenDFe.NFe`

O crédito presumido da operação no IBS e na CBS (`gCredPresOper`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CreditoPresumidoOperacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCCredPres` | UB121 | A base de cálculo do crédito presumido. |
| `Codigo` | `string?` | `cCredPres` | UB122 | O código de classificação do crédito presumido. |
| `Ibs` | `CreditoPresumidoIbsCbs?` | `gIBSCredPres` | UB123 | O crédito presumido do IBS; opcional. |
| `Cbs` | `CreditoPresumidoIbsCbs?` | `gCBSCredPres` | UB127 | O crédito presumido da CBS; opcional. |

## CreditoPresumidoZfm

Classe selada · Namespace `OpenDFe.NFe`

O crédito presumido do IBS na Zona Franca de Manaus (`gCredPresIBSZFM`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CreditoPresumidoZfm()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Competencia` | `string?` | `competApur` | UB132 | O ano e o mês da competência de apuração, como no XML (`AAAA-MM`). |
| `Classificacao` | `ClassificacaoCreditoPresumidoZfm?` | `tpCredPresIBSZFM` | UB133 | A classificação para o crédito presumido do IBS na Zona Franca de Manaus. |
| `Valor` | `decimal?` | `vCredPresIBSZFM` | UB134 | O valor do crédito presumido do IBS na Zona Franca de Manaus. |

## CupomFiscalReferenciado

Classe selada · Tag `refECF` (BA20) · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Um cupom fiscal referenciado (`refECF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `CupomFiscalReferenciado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Modelo` | `ModeloCupomFiscal?` | `mod` | BA21 | O modelo do cupom (`2B`, `2C` ou `2D`). |
| `NumeroEcf` | `int?` | `nECF` | BA22 | O número de ordem sequencial do ECF. |
| `Coo` | `int?` | `nCOO` | BA23 | O contador de ordem de operação (COO). |

## DeclaracaoImportacao

Classe selada · Namespace `OpenDFe.NFe`

Uma declaração de importação do produto (`DI`).

### Construtores

| Construtor | Descrição |
|---|---|
| `DeclaracaoImportacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `string?` | `nDI` | I19 | O número da declaração de importação (DI, DSI, DIRE, DUImp). |
| `Data` | `DateOnly?` | `dDI` | I20 | A data do registro. |
| `LocalDesembaraco` | `string?` | `xLocDesemb` | I21 | O local do desembaraço aduaneiro. |
| `UFDesembaraco` | `UF?` | `UFDesemb` | I22 | A UF do desembaraço aduaneiro. |
| `DataDesembaraco` | `DateOnly?` | `dDesemb` | I23 | A data do desembaraço aduaneiro. |
| `ViaTransporte` | `ViaTransporteInternacional?` | `tpViaTransp` | I23a | A via de transporte internacional. |
| `ValorAfrmm` | `decimal?` | `vAFRMM` | I23b | O valor do adicional ao frete para renovação da marinha mercante (AFRMM); opcional. |
| `FormaImportacao` | `FormaImportacao?` | `tpIntermedio` | I23c | A forma de importação quanto à intermediação. |
| `Cnpj` | `string?` | `CNPJ` | I23d | O CNPJ do adquirente ou do encomendante; ou o [`Cpf`](#declaracaoimportacao) (um dos dois). |
| `Cpf` | `string?` | `CPF` | I23d1 | O CPF do adquirente ou do encomendante; ou o [`Cnpj`](#declaracaoimportacao) (um dos dois). |
| `UFTerceiro` | `UF?` | `UFTerceiro` | I23e | A UF do adquirente ou do encomendante; opcional. |
| `CodigoExportador` | `string?` | `cExportador` | I24 | O código do exportador. |
| `Adicoes` | `IList<Adicao>` | `adi` | I25 | As adições (de 1 a 999). |

## Deducao

Classe selada · Namespace `OpenDFe.NFe`

Uma dedução da aquisição de cana (`deduc`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Deducao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Descricao` | `string?` | `xDed` | ZC11 | A descrição. |
| `Valor` | `decimal?` | `vDed` | ZC12 | O valor. |

## Defensivo

Classe selada · Namespace `OpenDFe.NFe`

Um defensivo agrícola (`defensivo`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Defensivo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Receituario` | `string?` | `nReceituario` | ZF03 | O número do receituário ou da receita. |
| `CpfResponsavel` | `string?` | `CPFRespTec` | ZF03a | O CPF do responsável técnico pelo receituário. |

## Destinatario

Classe selada · Namespace `OpenDFe.NFe`

O destinatário (`dest`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Destinatario()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | E02 | O CNPJ; ou o [`Cpf`](#destinatario) ou o [`IdEstrangeiro`](#destinatario) (um dos três). |
| `Cpf` | `string?` | `CPF` | E03 | O CPF; ou o [`Cnpj`](#destinatario) ou o [`IdEstrangeiro`](#destinatario) (um dos três). |
| `IdEstrangeiro` | `string?` | `idEstrangeiro` | E03a | O documento do estrangeiro; ou o [`Cnpj`](#destinatario) ou o [`Cpf`](#destinatario) (um dos três). Texto vazio escreve a tag vazia (o estrangeiro sem documento). |
| `Nome` | `string?` | `xNome` | E04 | A razão social ou o nome; opcional. |
| `Endereco` | `EnderecoDestinatario?` | `enderDest` | E05 | O endereço; opcional. |
| `IndicadorIE` | `IndicadorIEDestinatario?` | `indIEDest` | E16a | O indicador da inscrição estadual. |
| `InscricaoEstadual` | `string?` | `IE` | E17 | A inscrição estadual; opcional. |
| `InscricaoSuframa` | `string?` | `ISUF` | E18 | A inscrição na SUFRAMA; opcional. |
| `InscricaoMunicipal` | `string?` | `IM` | E18a | A inscrição municipal do tomador do serviço; opcional. |
| `Email` | `string?` | `email` | E19 | O e-mail; opcional. |

## DetalheExportacao

Classe selada · Namespace `OpenDFe.NFe`

Um detalhe da exportação do produto (`detExport`).

### Construtores

| Construtor | Descrição |
|---|---|
| `DetalheExportacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Drawback` | `string?` | `nDraw` | I51 | O número do ato concessório de drawback; opcional. |
| `Indireta` | `ExportacaoIndireta?` | `exportInd` | I52 | A exportação indireta; opcional. |

## DevolucaoTributo

Classe selada · Namespace `OpenDFe.NFe`

A devolução de um tributo da reforma (`gDevTrib`).

Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e na CBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `DevolucaoTributo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Percentual` | `decimal?` | `pDevTrib` |  | O percentual da devolução; opcional. |
| `Valor` | `decimal?` | `vDevTrib` |  | O valor da devolução. |

## Diferimento

Classe selada · Namespace `OpenDFe.NFe`

O diferimento de um tributo da reforma (`gDif`).

Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e na CBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `Diferimento()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Percentual` | `decimal?` | `pDif` |  | O percentual do diferimento. |
| `Valor` | `decimal?` | `vDif` |  | O valor do diferimento. |

## DocumentoReferenciado

Classe abstrata · Namespace `OpenDFe.NFe`

Um documento referenciado (`NFref`): exatamente uma das escolhas do XML, uma subclasse para cada.

Hierarquia fechada: só a lib deriva. Uma escolha nova entra como subclasse nova em versão menor; quem faz `switch` pelo tipo precisa de um caso padrão.

## Duplicata

Classe selada · Namespace `OpenDFe.NFe`

Uma duplicata (`dup`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Duplicata()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `string?` | `nDup` | Y08 | O número; opcional. |
| `Vencimento` | `DateOnly?` | `dVenc` | Y09 | A data de vencimento; opcional. |
| `Valor` | `decimal?` | `vDup` | Y10 | O valor. |

## EmbalagemProduto

Classe selada · Namespace `OpenDFe.NFe`

A embalagem do produto (`infProdEmb`).

### Construtores

| Construtor | Descrição |
|---|---|
| `EmbalagemProduto()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Descricao` | `string?` | `xEmb` | I87a | A embalagem. |
| `Volume` | `decimal?` | `qVolEmb` | I87b | O volume do produto na embalagem. |
| `Unidade` | `string?` | `uEmb` | I87c | A unidade de medida da embalagem. |

## Emitente

Classe selada · Namespace `OpenDFe.NFe`

O emitente (`emit`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Emitente()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | C02 | O CNPJ; ou o [`Cpf`](#emitente) (um dos dois). |
| `Cpf` | `string?` | `CPF` | C02a | O CPF; ou o [`Cnpj`](#emitente) (um dos dois). |
| `Nome` | `string?` | `xNome` | C03 | A razão social ou o nome. |
| `NomeFantasia` | `string?` | `xFant` | C04 | O nome fantasia; opcional. |
| `Endereco` | `EnderecoEmitente` | `enderEmit` | C05 | O endereço. |
| `InscricaoEstadual` | `string?` | `IE` | C17 | A inscrição estadual, ou `ISENTO`; opcional desde a NT 2026.007. |
| `InscricaoEstadualSubstituto` | `string?` | `IEST` | C18 | A inscrição estadual do substituto tributário; opcional. |
| `InscricaoMunicipal` | `string?` | `IM` | C19 | A inscrição municipal; opcional, com o [`Cnae`](#emitente). |
| `Cnae` | `string?` | `CNAE` | C20 | O CNAE fiscal; opcional, só com a [`InscricaoMunicipal`](#emitente). |
| `RegimeTributario` | `RegimeTributario?` | `CRT` | C21 | O código do regime tributário. |
| `InscricaoSuframa` | `string?` | `ISUFEmit` | C22 | A inscrição na SUFRAMA; opcional. |

## Encerrante

Classe selada · Namespace `OpenDFe.NFe`

O encerrante do abastecimento (`encerrante`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Encerrante()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Bico` | `int?` | `nBico` | LA12 | O número do bico. |
| `Bomba` | `int?` | `nBomba` | LA13 | O número da bomba; opcional. |
| `Tanque` | `int?` | `nTanque` | LA14 | O número do tanque. |
| `Inicial` | `decimal?` | `vEncIni` | LA15 | O encerrante no início do abastecimento. |
| `Final` | `decimal?` | `vEncFin` | LA16 | O encerrante no fim do abastecimento. |

## EnderecoDestinatario

Classe selada · Namespace `OpenDFe.NFe`

O endereço do destinatário (`enderDest`, o tipo `TEndereco`): aceita o exterior.

### Construtores

| Construtor | Descrição |
|---|---|
| `EnderecoDestinatario()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Logradouro` | `string?` | `xLgr` | E06 | O logradouro. |
| `Numero` | `string?` | `nro` | E07 | O número. |
| `Complemento` | `string?` | `xCpl` | E08 | O complemento; opcional. |
| `Bairro` | `string?` | `xBairro` | E09 | O bairro. |
| `CodigoMunicipio` | `int?` | `cMun` | E10 | O código do município (IBGE); 9999999 no exterior. |
| `Municipio` | `string?` | `xMun` | E11 | O nome do município; `EXTERIOR` no exterior. |
| `UF` | `UF?` | `UF` | E12 | A UF; nula no exterior ([`Exterior`](#enderecodestinatario)). |
| `Exterior` | `bool` |  |  | Se a UF do XML é `EX` (exterior); com ela, [`UF`](#enderecodestinatario) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UF`. |
| `Cep` | `string?` | `CEP` | E13 | O CEP; opcional. |
| `CodigoPais` | `int?` | `cPais` | E14 | O código do país; opcional. |
| `Pais` | `string?` | `xPais` | E15 | O nome do país; opcional. |
| `Telefone` | `string?` | `fone` | E16 | O telefone; opcional. |

## EnderecoEmitente

Classe selada · Namespace `OpenDFe.NFe`

O endereço do emitente (`enderEmit`, o tipo `TEnderEmi`): só UF do Brasil.

### Construtores

| Construtor | Descrição |
|---|---|
| `EnderecoEmitente()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Logradouro` | `string?` | `xLgr` | C06 | O logradouro. |
| `Numero` | `string?` | `nro` | C07 | O número. |
| `Complemento` | `string?` | `xCpl` | C08 | O complemento; opcional. |
| `Bairro` | `string?` | `xBairro` | C09 | O bairro. |
| `CodigoMunicipio` | `int?` | `cMun` | C10 | O código do município (IBGE). |
| `Municipio` | `string?` | `xMun` | C11 | O nome do município. |
| `UF` | `UF?` | `UF` | C12 | A UF. |
| `Cep` | `string?` | `CEP` | C13 | O CEP. |
| `CodigoPais` | `int?` | `cPais` | C14 | O código do país (1058, Brasil); opcional. |
| `Pais` | `string?` | `xPais` | C15 | O nome do país; opcional. |
| `Telefone` | `string?` | `fone` | C16 | O telefone; opcional. |

## EstornoCredito

Classe selada · Namespace `OpenDFe.NFe`

O estorno de crédito do IBS e da CBS (`gEstornoCred`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no item e no total.

### Construtores

| Construtor | Descrição |
|---|---|
| `EstornoCredito()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `ValorIbs` | `decimal?` | `vIBSEstCred` |  | O valor do IBS estornado. |
| `ValorCbs` | `decimal?` | `vCBSEstCred` |  | O valor da CBS estornada. |

## Exportacao

Classe selada · Namespace `OpenDFe.NFe`

A exportação (`exporta`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Exportacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `UFSaida` | `UF?` | `UFSaidaPais` | ZA02 | A UF do embarque ou da transposição de fronteira. |
| `LocalEmbarque` | `string?` | `xLocExporta` | ZA03 | O local do embarque ou da transposição de fronteira. |
| `LocalDespacho` | `string?` | `xLocDespacho` | ZA04 | O local do despacho; opcional. |

## ExportacaoIndireta

Classe selada · Namespace `OpenDFe.NFe`

A exportação indireta (`exportInd`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ExportacaoIndireta()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `RegistroExportacao` | `string?` | `nRE` | I53 | O número do registro de exportação. |
| `Chave` | `string?` | `chNFe` | I54 | A chave da NF-e recebida para exportação. |
| `Quantidade` | `decimal?` | `qExport` | I55 | A quantidade do item efetivamente exportada. |

## Fatura

Classe selada · Namespace `OpenDFe.NFe`

A fatura (`fat`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Fatura()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `string?` | `nFat` | Y03 | O número; opcional. |
| `ValorOriginal` | `decimal?` | `vOrig` | Y04 | O valor original; opcional. |
| `Desconto` | `decimal?` | `vDesc` | Y05 | O valor do desconto; opcional. |
| `ValorLiquido` | `decimal?` | `vLiq` | Y06 | O valor líquido; opcional. |

## FiscoEmitente

Classe selada · Namespace `OpenDFe.NFe`

O fisco que emitiu a nota avulsa (`avulsa`).

### Construtores

| Construtor | Descrição |
|---|---|
| `FiscoEmitente()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | D02 | O CNPJ do órgão emitente. |
| `Orgao` | `string?` | `xOrgao` | D03 | O órgão emitente. |
| `MatriculaAgente` | `string?` | `matr` | D04 | A matrícula do agente. |
| `NomeAgente` | `string?` | `xAgente` | D05 | O nome do agente. |
| `Telefone` | `string?` | `fone` | D06 | O telefone; opcional. |
| `UF` | `UF?` | `UF` | D07 | A UF. |
| `NumeroDar` | `string?` | `nDAR` | D08 | O número do documento de arrecadação; opcional. |
| `EmissaoDar` | `DateOnly?` | `dEmi` | D09 | A data da emissão do documento de arrecadação; opcional. |
| `ValorDar` | `decimal?` | `vDAR` | D10 | O valor do documento de arrecadação; opcional. |
| `Reparticao` | `string?` | `repEmi` | D11 | A repartição fiscal emitente. |
| `PagamentoDar` | `DateOnly?` | `dPag` | D12 | A data do pagamento do documento de arrecadação; opcional. |

## FormaPagamento

Classe selada · Namespace `OpenDFe.NFe`

Uma forma de pagamento (`detPag`).

### Construtores

| Construtor | Descrição |
|---|---|
| `FormaPagamento()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Indicador` | `IndicadorPagamento?` | `indPag` | YA01b | À vista ou a prazo; opcional. |
| `Meio` | `MeioPagamento?` | `tPag` | YA02 | O meio de pagamento. |
| `Descricao` | `string?` | `xPag` | YA02a | A descrição do meio de pagamento, no [`Outros`](opendfe-nfe-modelo-enumeracoes.md#meiopagamento); opcional. |
| `Valor` | `decimal?` | `vPag` | YA03 | O valor do pagamento. |
| `Data` | `DateOnly?` | `dPag` | YA03a | A data do pagamento; opcional. |
| `CnpjTransacao` | `string?` | `CNPJPag` | YA03c | O CNPJ do estabelecimento onde o pagamento foi processado, quando diferente do emitente; com a [`UFTransacao`](#formapagamento). |
| `UFTransacao` | `UF?` | `UFPag` | YA03d | A UF do [`CnpjTransacao`](#formapagamento). |
| `Transacao` | `TransacaoEletronica?` | `card` | YA04 | Os dados do pagamento eletrônico (cartões, PIX, boletos); opcional. |

## FornecimentoDiario

Classe selada · Namespace `OpenDFe.NFe`

O fornecimento de cana de um dia (`forDia`).

### Construtores

| Construtor | Descrição |
|---|---|
| `FornecimentoDiario()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Dia` | `int?` | `dia` | ZC05 | O dia (o atributo `dia`). |
| `Quantidade` | `decimal?` | `qtde` | ZC06 | A quantidade. |

## GuiaTransito

Classe selada · Namespace `OpenDFe.NFe`

A guia de trânsito de produtos agropecuários (`guiaTransito`).

### Construtores

| Construtor | Descrição |
|---|---|
| `GuiaTransito()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Tipo` | `TipoGuiaTransito?` | `tpGuia` | ZF05 | O tipo da guia. |
| `UF` | `UF?` | `UFGuia` | ZF06 | A UF de emissão; opcional. |
| `Serie` | `string?` | `serieGuia` | ZF07 | A série; opcional. |
| `Numero` | `int?` | `nGuia` | ZF08 | O número. |

## IbsCbs

Classe selada · Namespace `OpenDFe.NFe`

O IBS e a CBS do item (`IBSCBS`, NT 2025.002).

O CST e a classificação tributária são códigos de tabela publicada à parte e ficam como texto, com os zeros à esquerda. A [`Tributacao`](#ibscbs), a [`Monofasia`](#ibscbs), a [`TransferenciaCredito`](#ibscbs) e o [`AjusteCompetencia`](#ibscbs) são uma escolha do XSD, e o [`CreditoPresumido`](#ibscbs) e o [`CreditoPresumidoZfm`](#ibscbs) outra: em cada uma, no máximo um vai preenchido.

### Construtores

| Construtor | Descrição |
|---|---|
| `IbsCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `string?` | `CST` | UB13 | O CST do IBS e da CBS, como no XML (com os zeros à esquerda). |
| `ClassificacaoTributaria` | `string?` | `cClassTrib` | UB14 | A classificação tributária, como no XML (com os zeros à esquerda). |
| `Doacao` | `IndicadorDoacao?` | `indDoacao` | UB14a | Se a operação é de doação; opcional. |
| `Tributacao` | `TributacaoIbsCbs?` | `gIBSCBS` | UB15 | A tributação do IBS e da CBS; ou a [`Monofasia`](#ibscbs), a [`TransferenciaCredito`](#ibscbs) ou o [`AjusteCompetencia`](#ibscbs) (no máximo um). |
| `Monofasia` | `MonofasiaIbsCbs?` | `gIBSCBSMono` | UB84 | A tributação monofásica de combustíveis; ou a [`Tributacao`](#ibscbs), a [`TransferenciaCredito`](#ibscbs) ou o [`AjusteCompetencia`](#ibscbs) (no máximo um). |
| `TransferenciaCredito` | `TransferenciaCredito?` | `gTransfCred` | UB106 | A transferência de crédito (CST 800); ou a [`Tributacao`](#ibscbs), a [`Monofasia`](#ibscbs) ou o [`AjusteCompetencia`](#ibscbs) (no máximo um). |
| `AjusteCompetencia` | `AjusteCompetencia?` | `gAjusteCompet` | UB112 | O ajuste de competência (CST 811); ou a [`Tributacao`](#ibscbs), a [`Monofasia`](#ibscbs) ou a [`TransferenciaCredito`](#ibscbs) (no máximo um). |
| `EstornoCredito` | `EstornoCredito?` | `gEstornoCred` | UB116 | O estorno de crédito; opcional. |
| `CreditoPresumido` | `CreditoPresumidoOperacao?` | `gCredPresOper` | UB120 | O crédito presumido da operação; ou o [`CreditoPresumidoZfm`](#ibscbs) (no máximo um). |
| `CreditoPresumidoZfm` | `CreditoPresumidoZfm?` | `gCredPresIBSZFM` | UB131 | O crédito presumido do IBS na Zona Franca de Manaus; ou o [`CreditoPresumido`](#ibscbs) (no máximo um). |

## IbsMunicipio

Classe selada · Namespace `OpenDFe.NFe`

O IBS do município (`gIBSMun`).

### Construtores

| Construtor | Descrição |
|---|---|
| `IbsMunicipio()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Aliquota` | `decimal?` | `pIBSMun` | UB37 | A alíquota do IBS do município. |
| `Diferimento` | `Diferimento?` | `gDif` | UB40 | O diferimento; opcional. |
| `Devolucao` | `DevolucaoTributo?` | `gDevTrib` | UB43 | A devolução de tributo; opcional. |
| `Reducao` | `ReducaoAliquota?` | `gRed` | UB45 | A redução da alíquota; opcional. |
| `Valor` | `decimal?` | `vIBSMun` | UB54 | O valor do IBS do município. |

## IbsUF

Classe selada · Namespace `OpenDFe.NFe`

O IBS da UF (`gIBSUF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `IbsUF()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Aliquota` | `decimal?` | `pIBSUF` | UB18 | A alíquota do IBS da UF. |
| `Diferimento` | `Diferimento?` | `gDif` | UB21 | O diferimento; opcional. |
| `Devolucao` | `DevolucaoTributo?` | `gDevTrib` | UB24 | A devolução de tributo; opcional. |
| `Reducao` | `ReducaoAliquota?` | `gRed` | UB26 | A redução da alíquota; opcional. |
| `Valor` | `decimal?` | `vIBSUF` | UB35 | O valor do IBS da UF. |

## Icms

Classe abstrata · Namespace `OpenDFe.NFe`

O ICMS do item (`ICMS`): exatamente um dos grupos do XML, uma subclasse para cada.

Hierarquia fechada: só a lib deriva. Um grupo novo entra como subclasse nova em versão menor; quem faz `switch` pelo tipo precisa de um caso padrão. A [`Origem`](#icms) está em todos os grupos; o CST e o CSOSN ficam nas subclasses.

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Origem` | `OrigemMercadoria?` | `orig` | N11 | A origem da mercadoria. |

## Icms00

Classe selada · Tag `ICMS00` (N02) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS tributado integralmente (`ICMS00`, CST 00).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms00()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`00`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |

## Icms02

Classe selada · Tag `ICMS02` (N02a) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS monofásico próprio sobre combustíveis (`ICMS02`, CST 02).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms02()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`02`). Nulo na escrita: o único valor do grupo. |
| `QuantidadeBaseMonofasico` | `decimal?` | `qBCMono` | N37a | A quantidade tributada no ICMS monofásico próprio; opcional. |
| `AliquotaAdRem` | `decimal?` | `adRemICMS` | N38 | A alíquota ad rem do ICMS monofásico próprio. |
| `ValorMonofasico` | `decimal?` | `vICMSMono` | N39 | O valor do ICMS monofásico próprio. |

## Icms10

Classe selada · Tag `ICMS10` (N03) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS tributado e com cobrança por substituição tributária (`ICMS10`, CST 10).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms10()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`10`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCP` | N17a | A base de cálculo do FCP; opcional. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `ValorStDesonerado` | `decimal?` | `vICMSSTDeson` | N33a | O valor do ICMS ST desonerado; opcional. |
| `MotivoDesoneracaoSt` | `MotivoDesoneracao?` | `motDesICMSST` | N33b | O motivo da desoneração do ICMS ST; opcional. |

## Icms15

Classe selada · Tag `ICMS15` (N03a) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS monofásico próprio e com responsabilidade pela retenção sobre combustíveis (`ICMS15`, CST 15).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms15()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`15`). Nulo na escrita: o único valor do grupo. |
| `QuantidadeBaseMonofasico` | `decimal?` | `qBCMono` | N37a | A quantidade tributada no ICMS monofásico próprio; opcional. |
| `AliquotaAdRem` | `decimal?` | `adRemICMS` | N38 | A alíquota ad rem do ICMS monofásico próprio. |
| `ValorMonofasico` | `decimal?` | `vICMSMono` | N39 | O valor do ICMS monofásico próprio. |
| `QuantidadeBaseRetencao` | `decimal?` | `qBCMonoReten` | N39a | A quantidade tributada no ICMS monofásico sujeito à retenção; opcional. |
| `AliquotaAdRemRetencao` | `decimal?` | `adRemICMSReten` | N40 | A alíquota ad rem do ICMS monofásico sujeito à retenção. |
| `ValorMonofasicoRetencao` | `decimal?` | `vICMSMonoReten` | N41 | O valor do ICMS monofásico sujeito à retenção. |
| `PercentualReducaoAdRem` | `decimal?` | `pRedAdRem` | N47 | O percentual de redução da alíquota ad rem; opcional. |
| `MotivoReducaoAdRem` | `MotivoReducaoAdRem?` | `motRedAdRem` | N48 | O motivo da redução da alíquota ad rem; opcional. |

## Icms20

Classe selada · Tag `ICMS20` (N04) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS com redução de base de cálculo (`ICMS20`, CST 20).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms20()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`20`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCP` | N17a | A base de cálculo do FCP; opcional. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |

## Icms30

Classe selada · Tag `ICMS30` (N05) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS isento ou não tributado e com cobrança por substituição tributária (`ICMS30`, CST 30).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms30()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`30`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |

## Icms40

Classe selada · Tag `ICMS40` (N06) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS isento, não tributado ou suspenso (`ICMS40`, CST 40, 41 ou 50).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms40()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST: `40`, `41` ou `50`. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |

## Icms51

Classe selada · Tag `ICMS51` (N07) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS com diferimento (`ICMS51`, CST 51).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms51()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`51`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo; opcional. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo; opcional. |
| `CodigoBeneficioReducao` | `string?` | `cBenefRBC` | N14a | O código de benefício fiscal da redução da base de cálculo; opcional. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota; opcional. |
| `ValorOperacao` | `decimal?` | `vICMSOp` | N16a | O valor do ICMS da operação; opcional. |
| `PercentualDiferimento` | `decimal?` | `pDif` | N16b | O percentual do diferimento; opcional. |
| `ValorDiferido` | `decimal?` | `vICMSDif` | N16c | O valor do ICMS diferido; opcional. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS; opcional. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCP` | N17a | A base de cálculo do FCP; opcional. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |
| `PercentualDiferimentoFcp` | `decimal?` | `pFCPDif` | N17d | O percentual do diferimento do FCP; opcional. |
| `ValorFcpDiferido` | `decimal?` | `vFCPDif` | N17e | O valor do FCP diferido; opcional. |
| `ValorFcpEfetivo` | `decimal?` | `vFCPEfet` | N17f | O valor efetivo do FCP; opcional. |

## Icms53

Classe selada · Tag `ICMS53` (N07a) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS monofásico sobre combustíveis com recolhimento diferido (`ICMS53`, CST 53).

Na NT 2023.001 v1.60, os identificadores N41a e N42 aparecem duas vezes neste grupo; as propriedades os levam como na NT.

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms53()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`53`). Nulo na escrita: o único valor do grupo. |
| `QuantidadeBaseMonofasico` | `decimal?` | `qBCMono` | N37a | A quantidade tributada no ICMS monofásico próprio; opcional. |
| `AliquotaAdRem` | `decimal?` | `adRemICMS` | N38 | A alíquota ad rem do ICMS monofásico próprio; opcional. |
| `ValorMonofasicoOperacao` | `decimal?` | `vICMSMonoOp` | N41a | O valor do ICMS monofásico da operação; opcional. |
| `PercentualDiferimento` | `decimal?` | `pDif` | N42 | O percentual do diferimento; opcional. |
| `ValorMonofasicoDiferido` | `decimal?` | `vICMSMonoDif` | N43 | O valor do ICMS monofásico diferido; opcional. |
| `ValorMonofasico` | `decimal?` | `vICMSMono` | N39 | O valor do ICMS monofásico próprio; opcional. |
| `QuantidadeBaseDiferida` | `decimal?` | `qBCMonoDif` | N41a | A quantidade tributada no ICMS monofásico diferido; opcional. |
| `AliquotaAdRemDiferida` | `decimal?` | `adRemICMSDif` | N42 | A alíquota ad rem do ICMS monofásico diferido; opcional. |

## Icms60

Classe selada · Tag `ICMS60` (N08) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS cobrado anteriormente por substituição tributária (`ICMS60`, CST 60).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms60()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`60`). Nulo na escrita: o único valor do grupo. |
| `BaseCalculoStRetido` | `decimal?` | `vBCSTRet` | N26 | A base de cálculo do ICMS ST retido anteriormente; opcional. |
| `AliquotaConsumidorFinal` | `decimal?` | `pST` | N26a | A alíquota suportada pelo consumidor final; opcional. |
| `ValorSubstituto` | `decimal?` | `vICMSSubstituto` | N26b | O valor do ICMS próprio do substituto; opcional. |
| `ValorStRetido` | `decimal?` | `vICMSSTRet` | N27 | O valor do ICMS ST retido anteriormente; opcional. |
| `BaseCalculoFcpStRetido` | `decimal?` | `vBCFCPSTRet` | N27a | A base de cálculo do FCP retido anteriormente por substituição tributária; opcional. |
| `PercentualFcpStRetido` | `decimal?` | `pFCPSTRet` | N27b | O percentual do FCP retido anteriormente por substituição tributária; opcional. |
| `ValorFcpStRetido` | `decimal?` | `vFCPSTRet` | N27d | O valor do FCP retido anteriormente por substituição tributária; opcional. |
| `PercentualReducaoBaseEfetiva` | `decimal?` | `pRedBCEfet` | N34 | O percentual de redução da base de cálculo efetiva; opcional. |
| `BaseCalculoEfetiva` | `decimal?` | `vBCEfet` | N35 | A base de cálculo efetiva; opcional. |
| `AliquotaEfetiva` | `decimal?` | `pICMSEfet` | N36 | A alíquota do ICMS efetiva; opcional. |
| `ValorEfetivo` | `decimal?` | `vICMSEfet` | N37 | O valor do ICMS efetivo; opcional. |

## Icms61

Classe selada · Tag `ICMS61` (N08a) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS monofásico sobre combustíveis cobrado anteriormente (`ICMS61`, CST 61).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms61()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`61`). Nulo na escrita: o único valor do grupo. |
| `QuantidadeBaseRetida` | `decimal?` | `qBCMonoRet` | N43a | A quantidade tributada no ICMS monofásico retido anteriormente; opcional. |
| `AliquotaAdRemRetida` | `decimal?` | `adRemICMSRet` | N44 | A alíquota ad rem do ICMS monofásico retido anteriormente. |
| `ValorMonofasicoRetido` | `decimal?` | `vICMSMonoRet` | N45 | O valor do ICMS monofásico retido anteriormente. |

## Icms70

Classe selada · Tag `ICMS70` (N09) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS com redução de base de cálculo e cobrança por substituição tributária (`ICMS70`, CST 70).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms70()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`70`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCP` | N17a | A base de cálculo do FCP; opcional. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |
| `ValorStDesonerado` | `decimal?` | `vICMSSTDeson` | N33a | O valor do ICMS ST desonerado; opcional. |
| `MotivoDesoneracaoSt` | `MotivoDesoneracao?` | `motDesICMSST` | N33b | O motivo da desoneração do ICMS ST; opcional. |

## Icms90

Classe selada · Tag `ICMS90` (N10) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS de outras situações (`ICMS90`, CST 90).

### Construtores

| Construtor | Descrição |
|---|---|
| `Icms90()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST (`90`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo; opcional. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo; opcional. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo; opcional. |
| `CodigoBeneficioReducao` | `string?` | `cBenefRBC` | N14a | O código de benefício fiscal da redução da base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota; opcional. |
| `ValorOperacao` | `decimal?` | `vICMSOp` | N16b | O valor do ICMS da operação; opcional. |
| `PercentualDiferimento` | `decimal?` | `pDif` | N16c | O percentual do diferimento; opcional. |
| `ValorDiferido` | `decimal?` | `vICMSDif` | N16d | O valor do ICMS diferido; opcional. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS; opcional. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCP` | N17a | A base de cálculo do FCP; opcional. |
| `PercentualFcp` | `decimal?` | `pFCP` | N17b | O percentual do FCP (fundo de combate à pobreza); opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | N17c | O valor do FCP; opcional. |
| `PercentualDiferimentoFcp` | `decimal?` | `pFCPDif` | N17d | O percentual do diferimento do FCP; opcional. |
| `ValorFcpDiferido` | `decimal?` | `vFCPDif` | N17e | O valor do FCP diferido; opcional. |
| `ValorFcpEfetivo` | `decimal?` | `vFCPEfet` | N17f | O valor efetivo do FCP; opcional. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST; opcional. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST; opcional. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST; opcional. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST; opcional. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |
| `ValorStDesonerado` | `decimal?` | `vICMSSTDeson` | N33a | O valor do ICMS ST desonerado; opcional. |
| `MotivoDesoneracaoSt` | `MotivoDesoneracao?` | `motDesICMSST` | N33b | O motivo da desoneração do ICMS ST; opcional. |

## IcmsPartilha

Classe selada · Tag `ICMSPart` (N10a) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

A partilha do ICMS entre a UF de origem e a de destino, ou a definida na legislação (`ICMSPart`, CST 10 ou 90).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsPartilha()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST: `10`, `20` ou `90`. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `PercentualBaseOperacao` | `decimal?` | `pBCOp` | N25 | O percentual da base de cálculo da operação própria. |
| `UFSt` | `UF?` | `UFST` | N24 | A UF a que é devido o ICMS ST. |
| `Exterior` | `bool` |  |  | Se a UF do ICMS ST do XML é `EX` (exterior); com ela, [`UFSt`](#icmspartilha) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UFST`. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | N28a | O valor do ICMS desonerado; opcional. |
| `MotivoDesoneracao` | `MotivoDesoneracao?` | `motDesICMS` | N28 | O motivo da desoneração do ICMS; opcional. |
| `DeducaoDesoneracao` | `IndicadorDeducaoDesoneracao?` | `indDeduzDeson` | N28b | Se o ICMS desonerado deduz do valor do item e do total da nota; opcional. |

## IcmsRepasse

Classe selada · Tag `ICMSST` (N10b) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O repasse do ICMS ST retido anteriormente em operações interestaduais (`ICMSST`, CST 41 ou 60).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsRepasse()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIcms?` | `CST` | N12 | O CST: `41` ou `60`. |
| `BaseCalculoStRetido` | `decimal?` | `vBCSTRet` | N26 | A base de cálculo do ICMS ST retido anteriormente. |
| `AliquotaConsumidorFinal` | `decimal?` | `pST` | N26a | A alíquota suportada pelo consumidor final; opcional. |
| `ValorSubstituto` | `decimal?` | `vICMSSubstituto` | N26b | O valor do ICMS próprio do substituto; opcional. |
| `ValorStRetido` | `decimal?` | `vICMSSTRet` | N27 | O valor do ICMS ST retido anteriormente. |
| `BaseCalculoFcpStRetido` | `decimal?` | `vBCFCPSTRet` | N27a | A base de cálculo do FCP retido anteriormente por substituição tributária; opcional. |
| `PercentualFcpStRetido` | `decimal?` | `pFCPSTRet` | N27b | O percentual do FCP retido anteriormente por substituição tributária; opcional. |
| `ValorFcpStRetido` | `decimal?` | `vFCPSTRet` | N27d | O valor do FCP retido anteriormente por substituição tributária; opcional. |
| `BaseCalculoStDestino` | `decimal?` | `vBCSTDest` | N31 | A base de cálculo do ICMS ST da UF de destino. |
| `ValorStDestino` | `decimal?` | `vICMSSTDest` | N32 | O valor do ICMS ST da UF de destino. |
| `PercentualReducaoBaseEfetiva` | `decimal?` | `pRedBCEfet` | N34 | O percentual de redução da base de cálculo efetiva; opcional. |
| `BaseCalculoEfetiva` | `decimal?` | `vBCEfet` | N35 | A base de cálculo efetiva; opcional. |
| `AliquotaEfetiva` | `decimal?` | `pICMSEfet` | N36 | A alíquota do ICMS efetiva; opcional. |
| `ValorEfetivo` | `decimal?` | `vICMSEfet` | N37 | O valor do ICMS efetivo; opcional. |

## IcmsSn101

Classe selada · Tag `ICMSSN101` (N10c) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional com permissão de crédito (`ICMSSN101`, CSOSN 101).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn101()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN (`101`). Nulo na escrita: o único valor do grupo. |
| `AliquotaCredito` | `decimal?` | `pCredSN` | N29 | A alíquota do crédito que pode ser aproveitado (Simples Nacional). |
| `ValorCredito` | `decimal?` | `vCredICMSSN` | N30 | O valor do crédito que pode ser aproveitado (Simples Nacional). |

## IcmsSn102

Classe selada · Tag `ICMSSN102` (N10d) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional sem permissão de crédito, isento, imune ou não tributado (`ICMSSN102`, CSOSN 102, 103, 300 ou 400).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn102()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN: `102`, `103`, `300` ou `400`. |

## IcmsSn201

Classe selada · Tag `ICMSSN201` (N10e) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional com permissão de crédito e cobrança por substituição tributária (`ICMSSN201`, CSOSN 201).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn201()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN (`201`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `AliquotaCredito` | `decimal?` | `pCredSN` | N29 | A alíquota do crédito que pode ser aproveitado (Simples Nacional). |
| `ValorCredito` | `decimal?` | `vCredICMSSN` | N30 | O valor do crédito que pode ser aproveitado (Simples Nacional). |

## IcmsSn202

Classe selada · Tag `ICMSSN202` (N10f) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional sem permissão de crédito e com cobrança por substituição tributária (`ICMSSN202`, CSOSN 202 ou 203).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn202()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN: `202` ou `203`. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |

## IcmsSn500

Classe selada · Tag `ICMSSN500` (N10g) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional cobrado anteriormente por substituição tributária ou por antecipação (`ICMSSN500`, CSOSN 500).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn500()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN (`500`). Nulo na escrita: o único valor do grupo. |
| `BaseCalculoStRetido` | `decimal?` | `vBCSTRet` | N26 | A base de cálculo do ICMS ST retido anteriormente; opcional. |
| `AliquotaConsumidorFinal` | `decimal?` | `pST` | N26a | A alíquota suportada pelo consumidor final; opcional. |
| `ValorSubstituto` | `decimal?` | `vICMSSubstituto` | N26b | O valor do ICMS próprio do substituto; opcional. |
| `ValorStRetido` | `decimal?` | `vICMSSTRet` | N27 | O valor do ICMS ST retido anteriormente; opcional. |
| `BaseCalculoFcpStRetido` | `decimal?` | `vBCFCPSTRet` | N27a | A base de cálculo do FCP retido anteriormente por substituição tributária; opcional. |
| `PercentualFcpStRetido` | `decimal?` | `pFCPSTRet` | N27b | O percentual do FCP retido anteriormente por substituição tributária; opcional. |
| `ValorFcpStRetido` | `decimal?` | `vFCPSTRet` | N27d | O valor do FCP retido anteriormente por substituição tributária; opcional. |
| `PercentualReducaoBaseEfetiva` | `decimal?` | `pRedBCEfet` | N34 | O percentual de redução da base de cálculo efetiva; opcional. |
| `BaseCalculoEfetiva` | `decimal?` | `vBCEfet` | N35 | A base de cálculo efetiva; opcional. |
| `AliquotaEfetiva` | `decimal?` | `pICMSEfet` | N36 | A alíquota do ICMS efetiva; opcional. |
| `ValorEfetivo` | `decimal?` | `vICMSEfet` | N37 | O valor do ICMS efetivo; opcional. |

## IcmsSn900

Classe selada · Tag `ICMSSN900` (N10h) · Namespace `OpenDFe.NFe` · Deriva de [`Icms`](#icms)

O ICMS do Simples Nacional de outras situações (`ICMSSN900`, CSOSN 900).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsSn900()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Csosn` | `CsosnIcms?` | `CSOSN` | N12a | O CSOSN (`900`). Nulo na escrita: o único valor do grupo. |
| `ModalidadeBaseCalculo` | `ModalidadeBaseCalculo?` | `modBC` | N13 | A modalidade de determinação da base de cálculo; opcional. |
| `BaseCalculo` | `decimal?` | `vBC` | N15 | A base de cálculo; opcional. |
| `PercentualReducaoBase` | `decimal?` | `pRedBC` | N14 | O percentual de redução da base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pICMS` | N16 | A alíquota; opcional. |
| `Valor` | `decimal?` | `vICMS` | N17 | O valor do ICMS; opcional. |
| `ModalidadeBaseCalculoSt` | `ModalidadeBaseCalculoSt?` | `modBCST` | N18 | A modalidade de determinação da base de cálculo do ICMS ST; opcional. |
| `PercentualMargemSt` | `decimal?` | `pMVAST` | N19 | O percentual da margem de valor adicionado do ICMS ST; opcional. |
| `PercentualReducaoBaseSt` | `decimal?` | `pRedBCST` | N20 | O percentual de redução da base de cálculo do ICMS ST; opcional. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | N21 | A base de cálculo do ICMS ST; opcional. |
| `AliquotaSt` | `decimal?` | `pICMSST` | N22 | A alíquota do ICMS ST; opcional. |
| `ValorSt` | `decimal?` | `vICMSST` | N23 | O valor do ICMS ST; opcional. |
| `BaseCalculoFcpSt` | `decimal?` | `vBCFCPST` | N23a | A base de cálculo do FCP retido por substituição tributária; opcional. |
| `PercentualFcpSt` | `decimal?` | `pFCPST` | N23b | O percentual do FCP retido por substituição tributária; opcional. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | N23d | O valor do FCP retido por substituição tributária; opcional. |
| `AliquotaCredito` | `decimal?` | `pCredSN` | N29 | A alíquota do crédito que pode ser aproveitado (Simples Nacional); opcional. |
| `ValorCredito` | `decimal?` | `vCredICMSSN` | N30 | O valor do crédito que pode ser aproveitado (Simples Nacional); opcional. |

## IcmsUFDestino

Classe selada · Namespace `OpenDFe.NFe`

O ICMS de partilha para a UF do destinatário, na operação interestadual com consumidor final não contribuinte (`ICMSUFDest`, EC 87/2015).

### Construtores

| Construtor | Descrição |
|---|---|
| `IcmsUFDestino()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCUFDest` | NA03 | A base de cálculo na UF de destino. |
| `BaseCalculoFcp` | `decimal?` | `vBCFCPUFDest` | NA04 | A base de cálculo do FCP na UF de destino; opcional. |
| `PercentualFcp` | `decimal?` | `pFCPUFDest` | NA05 | O percentual do FCP na UF de destino; opcional. |
| `AliquotaInterna` | `decimal?` | `pICMSUFDest` | NA07 | A alíquota interna da UF de destino. |
| `AliquotaInterestadual` | `decimal?` | `pICMSInter` | NA09 | A alíquota interestadual (4, 7 ou 12). |
| `PercentualPartilha` | `decimal?` | `pICMSInterPart` | NA11 | O percentual provisório de partilha do ICMS interestadual. |
| `ValorFcp` | `decimal?` | `vFCPUFDest` | NA13 | O valor do FCP da UF de destino; opcional. |
| `ValorDestino` | `decimal?` | `vICMSUFDest` | NA15 | O valor do ICMS de partilha para a UF de destino. |
| `ValorRemetente` | `decimal?` | `vICMSUFRemet` | NA17 | O valor do ICMS de partilha para a UF do remetente. |

## Identificacao

Classe selada · Namespace `OpenDFe.NFe`

A identificação da nota (`ide`).

Os campos que o núcleo já sabe ([`UF`](#identificacao), [`Modelo`](#identificacao), [`TipoEmissao`](#identificacao), [`Ambiente`](#identificacao), a contingência) e o [`CodigoNumerico`](#identificacao), nulos na escrita, vêm do núcleo; preenchidos e diferentes dele, são erro na montagem. A leitura sempre os preenche. O `cDV` fica fora: é calculado na escrita e conferido na leitura.

### Construtores

| Construtor | Descrição |
|---|---|
| `Identificacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `UF` | `UF?` | `cUF` | B02 | A UF do emitente. Nulo na escrita: o valor do núcleo (a configuração). |
| `CodigoNumerico` | `int?` | `cNF` | B03 | O código numérico que compõe a chave (8 dígitos). Nulo na escrita: sorteado na montagem. |
| `NaturezaOperacao` | `string?` | `natOp` | B04 | A natureza da operação. |
| `Modelo` | `Documento?` | `mod` | B06 | O modelo do documento (55 ou 65). Nulo na escrita: o valor do núcleo (a configuração). |
| `Serie` | `int?` | `serie` | B07 | A série. |
| `Numero` | `int?` | `nNF` | B08 | O número da nota. |
| `Emissao` | `DateTimeOffset?` | `dhEmi` | B09 | A data e a hora da emissão, com o fuso. |
| `SaidaEntrada` | `DateTimeOffset?` | `dhSaiEnt` | B10 | A data e a hora da saída ou da entrada, com o fuso; opcional. |
| `PrevisaoEntrega` | `DateOnly?` | `dPrevEntrega` | B10a | A data prevista da entrega; opcional. |
| `Tipo` | `TipoNota?` | `tpNF` | B11 | Entrada ou saída. |
| `LocalDestino` | `LocalDestino?` | `idDest` | B11a | O local de destino da operação. |
| `MunicipioFatoGerador` | `int?` | `cMunFG` | B12 | O município do fato gerador do ICMS (código do IBGE). |
| `MunicipioFatoGeradorIbsCbs` | `int?` | `cMunFGIBS` | B12a | O município do fato gerador do IBS e da CBS (código do IBGE); opcional. |
| `FormatoImpressao` | `FormatoImpressao?` | `tpImp` | B21 | O formato de impressão do DANFE. |
| `TipoEmissao` | `TipoEmissao?` | `tpEmis` | B22 | O tipo de emissão. Nulo na escrita: o valor do núcleo (a configuração). |
| `Ambiente` | `Ambiente?` | `tpAmb` | B24 | O ambiente. Nulo na escrita: o valor do núcleo (a configuração). |
| `Finalidade` | `FinalidadeEmissao?` | `finNFe` | B25 | A finalidade da emissão. |
| `TipoDebito` | `TipoNotaDebito?` | `tpNFDebito` | B25.1 | O tipo da nota de débito; opcional. |
| `TipoCredito` | `TipoNotaCredito?` | `tpNFCredito` | B25.2 | O tipo da nota de crédito; opcional. |
| `ConsumidorFinal` | `IndicadorConsumidorFinal?` | `indFinal` | B25a | Se a operação é com consumidor final. |
| `Presenca` | `PresencaComprador?` | `indPres` | B25b | A presença do comprador no estabelecimento. |
| `Intermediacao` | `IndicadorIntermediador?` | `indIntermed` | B25c | Se a operação usa plataforma de terceiros (intermediador); opcional. |
| `CodigoLocalOperacao` | `string?` | `cIndOp` | B25d | O código do indicador da operação (6 dígitos); opcional. |
| `Processo` | `ProcessoEmissao?` | `procEmi` | B26 | O processo de emissão. |
| `VersaoAplicativo` | `string?` | `verProc` | B27 | A versão do aplicativo emissor. |
| `InicioContingencia` | `DateTimeOffset?` | `dhCont` | B28 | A data e a hora da entrada em contingência; com a [`JustificativaContingencia`](#identificacao). Nulo na escrita: o valor do núcleo (a configuração). |
| `JustificativaContingencia` | `string?` | `xJust` | B29 | A justificativa da entrada em contingência; com o [`InicioContingencia`](#identificacao). Nulo na escrita: o valor do núcleo (a configuração). |
| `Referencias` | `IList<DocumentoReferenciado>` | `NFref` | BA01 | Os documentos referenciados (até 999). |
| `CompraGovernamental` | `CompraGovernamental?` | `gCompraGov` | BB01 | A compra governamental; opcional. |
| `PagamentoAntecipado` | `PagamentoAntecipado?` | `gPagAntecipado` | BC01 | As notas de antecipação de pagamento que esta abate; opcional. |

## Imposto

Classe selada · Namespace `OpenDFe.NFe`

Os tributos do item (`imposto`).

O ICMS (com o IPI e o II) ou o ISSQN (com o IPI) são uma escolha do XSD: o [`Icms`](#imposto) e o [`Issqn`](#imposto) juntos, ou o [`ImpostoImportacao`](#imposto) com o [`Issqn`](#imposto), são erro na montagem; o [`Ipi`](#imposto) é um só nos dois ramos.

### Construtores

| Construtor | Descrição |
|---|---|
| `Imposto()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `ValorTotalTributos` | `decimal?` | `vTotTrib` | M02 | O valor aproximado total dos tributos (Lei 12.741/2012); opcional. |
| `Icms` | `Icms?` | `ICMS` | N01 | O ICMS; opcional. |
| `Ipi` | `Ipi?` | `IPI` | O01 | O IPI; opcional. |
| `ImpostoImportacao` | `ImpostoImportacao?` | `II` | P01 | O imposto de importação; opcional. |
| `Issqn` | `Issqn?` | `ISSQN` | U01 | O ISSQN; opcional. |
| `Pis` | `Pis?` | `PIS` | Q01 | O PIS; opcional. |
| `PisSt` | `PisSt?` | `PISST` | R01 | O PIS por substituição tributária; opcional. |
| `Cofins` | `Cofins?` | `COFINS` | S01 | A COFINS; opcional. |
| `CofinsSt` | `CofinsSt?` | `COFINSST` | T01 | A COFINS por substituição tributária; opcional. |
| `IcmsUFDestino` | `IcmsUFDestino?` | `ICMSUFDest` | NA01 | O ICMS de partilha para a UF do destinatário; opcional. |
| `ImpostoSeletivo` | `ImpostoSeletivo?` | `IS` | UB01 | O imposto seletivo; opcional. |
| `IbsCbs` | `IbsCbs?` | `IBSCBS` | UB12 | O IBS e a CBS; opcional. |

## ImpostoDevolvido

Classe selada · Namespace `OpenDFe.NFe`

O imposto devolvido do item (`impostoDevol`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ImpostoDevolvido()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Percentual` | `decimal?` | `pDevol` | UA02 | O percentual da mercadoria devolvida. |
| `Ipi` | `IpiDevolvido` | `IPI` | UA03 | O IPI devolvido. |

## ImpostoImportacao

Classe selada · Namespace `OpenDFe.NFe`

O imposto de importação do item (`II`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ImpostoImportacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | P02 | A base de cálculo. |
| `DespesasAduaneiras` | `decimal?` | `vDespAdu` | P03 | As despesas aduaneiras. |
| `Valor` | `decimal?` | `vII` | P04 | O valor do imposto de importação. |
| `Iof` | `decimal?` | `vIOF` | P05 | O valor do IOF. |

## ImpostoSeletivo

Classe selada · Namespace `OpenDFe.NFe`

O imposto seletivo do item (`IS`, NT 2025.002).

O CST e a classificação tributária são códigos de tabela publicada à parte e ficam como texto, com os zeros à esquerda.

### Construtores

| Construtor | Descrição |
|---|---|
| `ImpostoSeletivo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `string?` | `CSTIS` | UB02 | O CST do imposto seletivo, como no XML (com os zeros à esquerda). |
| `ClassificacaoTributaria` | `string?` | `cClassTribIS` | UB03 | A classificação tributária do imposto seletivo, como no XML. |
| `BaseCalculo` | `decimal?` | `vBCIS` | UB05 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pIS` | UB06 | A alíquota, em percentual; opcional. |
| `AliquotaEspecifica` | `decimal?` | `adRemIS` | UB07 | A alíquota específica, por unidade de medida; opcional. |
| `UnidadeTributavel` | `string?` | `uTrib` | UB09 | A unidade de medida tributável do imposto seletivo; opcional. |
| `QuantidadeTributavel` | `decimal?` | `qTrib` | UB10 | A quantidade na unidade tributável do imposto seletivo; opcional. |
| `Valor` | `decimal?` | `vIS` | UB11 | O valor do imposto seletivo; opcional. |

## InformacoesAdicionais

Classe selada · Namespace `OpenDFe.NFe`

As informações adicionais (`infAdic`).

### Construtores

| Construtor | Descrição |
|---|---|
| `InformacoesAdicionais()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Fisco` | `string?` | `infAdFisco` | Z02 | As informações de interesse do fisco; opcional. |
| `Complementares` | `string?` | `infCpl` | Z03 | As informações complementares de interesse do contribuinte; opcional. |
| `ObservacoesContribuinte` | `IList<Observacao>` | `obsCont` | Z04 | As observações de uso livre do contribuinte (até 10). |
| `ObservacoesFisco` | `IList<Observacao>` | `obsFisco` | Z07 | As observações de uso livre do fisco (até 10). |
| `Processos` | `IList<ProcessoReferenciado>` | `procRef` | Z10 | Os processos referenciados (até 100). |

## InformacoesSuplementares

Classe selada · Namespace `OpenDFe.NFe`

As informações suplementares da NFC-e (`infNFeSupl`).

A leitura preenche. Na escrita, a lib gera o grupo na NFC-e e na NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6) a partir da chave, do ambiente e da `ConfiguracaoNFCe` (etapa 5.1): o grupo preenchido igual ao gerado passa, e o diferente é erro (`NFE-NUCLEO`). Na NF-e sem `tpImp` 6, o grupo preenchido sai como está e é a regra 393.

### Construtores

| Construtor | Descrição |
|---|---|
| `InformacoesSuplementares()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `QrCode` | `string?` | `qrCode` | ZX02 | O texto do QR Code. |
| `UrlChave` | `string?` | `urlChave` | ZX03 | A URL da consulta pela chave de acesso. |

## Intermediador

Classe selada · Namespace `OpenDFe.NFe`

O intermediador da transação (`infIntermed`): a plataforma de terceiros.

### Construtores

| Construtor | Descrição |
|---|---|
| `Intermediador()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | YB02 | O CNPJ do intermediador. |
| `Identificador` | `string?` | `idCadIntTran` | YB03 | O identificador do vendedor no cadastro do intermediador. |

## Ipi

Classe selada · Namespace `OpenDFe.NFe`

O IPI do item (`IPI`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Ipi()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `CnpjProdutor` | `string?` | `CNPJProd` | O03 | O CNPJ do produtor, quando diferente do emitente (exportação direta ou indireta); opcional. |
| `CodigoSelo` | `string?` | `cSelo` | O04 | O código do selo de controle do IPI; opcional. |
| `QuantidadeSelo` | `long?` | `qSelo` | O05 | A quantidade de selos de controle; opcional. |
| `CodigoEnquadramento` | `string?` | `cEnq` | O06 | O código de enquadramento legal do IPI. |
| `Tributado` | `IpiTributado?` | `IPITrib` | O07 | O IPI tributado; ou o [`NaoTributado`](#ipi) (um dos dois). |
| `NaoTributado` | `IpiNaoTributado?` | `IPINT` | O08 | O IPI não tributado; ou o [`Tributado`](#ipi) (um dos dois). |

## IpiDevolvido

Classe selada · Namespace `OpenDFe.NFe`

O IPI devolvido (`impostoDevol/IPI`).

### Construtores

| Construtor | Descrição |
|---|---|
| `IpiDevolvido()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Valor` | `decimal?` | `vIPIDevol` | UA04 | O valor do IPI devolvido. |

## IpiNaoTributado

Classe selada · Namespace `OpenDFe.NFe`

O IPI não tributado (`IPINT`).

### Construtores

| Construtor | Descrição |
|---|---|
| `IpiNaoTributado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIpi?` | `CST` | O09 | O CST: `01`, `02`, `03`, `04`, `05`, `51`, `52`, `53`, `54` ou `55`. |

## IpiTributado

Classe selada · Namespace `OpenDFe.NFe`

O IPI tributado (`IPITrib`).

A base e a alíquota ([`BaseCalculo`](#ipitributado) e [`Aliquota`](#ipitributado)) ou a quantidade e o valor por unidade ([`Quantidade`](#ipitributado) e [`ValorUnidade`](#ipitributado)): escolha do XSD.

### Construtores

| Construtor | Descrição |
|---|---|
| `IpiTributado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstIpi?` | `CST` | O09 | O CST: `00`, `49`, `50` ou `99`. |
| `BaseCalculo` | `decimal?` | `vBC` | O10 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pIPI` | O13 | A alíquota; opcional. |
| `Quantidade` | `decimal?` | `qUnid` | O11 | A quantidade total na unidade padrão de tributação; opcional. |
| `ValorUnidade` | `decimal?` | `vUnid` | O12 | O valor por unidade tributável; opcional. |
| `Valor` | `decimal?` | `vIPI` | O14 | O valor do IPI. |

## Issqn

Classe selada · Namespace `OpenDFe.NFe`

O ISSQN do item (`ISSQN`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Issqn()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | U02 | A base de cálculo. |
| `Aliquota` | `decimal?` | `vAliq` | U03 | A alíquota. |
| `Valor` | `decimal?` | `vISSQN` | U04 | O valor do ISSQN. |
| `MunicipioFatoGerador` | `int?` | `cMunFG` | U05 | O município de ocorrência do fato gerador (código do IBGE). |
| `ItemListaServico` | `string?` | `cListServ` | U06 | O item da lista de serviços (LC 116/2003). |
| `Deducao` | `decimal?` | `vDeducao` | U07 | O valor da dedução, para a redução da base de cálculo; opcional. |
| `OutrasRetencoes` | `decimal?` | `vOutro` | U08 | O valor de outras retenções; opcional. |
| `DescontoIncondicionado` | `decimal?` | `vDescIncond` | U09 | O valor do desconto incondicionado; opcional. |
| `DescontoCondicionado` | `decimal?` | `vDescCond` | U10 | O valor do desconto condicionado; opcional. |
| `Retencao` | `decimal?` | `vISSRet` | U11 | O valor da retenção do ISS; opcional. |
| `Exigibilidade` | `ExigibilidadeIss?` | `indISS` | U12 | A exigibilidade do ISS. |
| `CodigoServico` | `string?` | `cServico` | U13 | O código do serviço no município; opcional. |
| `MunicipioIncidencia` | `int?` | `cMun` | U14 | O município de incidência do imposto (código do IBGE); opcional. |
| `CodigoPais` | `int?` | `cPais` | U15 | O país onde o serviço foi prestado (código do BACEN); opcional. |
| `Processo` | `string?` | `nProcesso` | U16 | O número do processo judicial ou administrativo de suspensão da exigibilidade; opcional. |
| `IncentivoFiscal` | `IndicadorIncentivo?` | `indIncentivo` | U17 | Se há incentivo fiscal. |

## Item

Classe selada · Namespace `OpenDFe.NFe`

Um item da nota (`det`): o produto ou serviço, os tributos e o que vem com eles.

O `nItem` do XML não tem propriedade: é a posição do item em [`Itens`](#notafiscal), a partir de 1 (a escrita numera de 1 a n, a leitura confere a sequência).

### Construtores

| Construtor | Descrição |
|---|---|
| `Item()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Produto` | `Produto` | `prod` | I01 | O produto ou serviço. |
| `Imposto` | `Imposto` | `imposto` | M01 | Os tributos do item. |
| `ImpostoDevolvido` | `ImpostoDevolvido?` | `impostoDevol` | UA01 | O imposto devolvido; opcional. |
| `InformacoesAdicionais` | `string?` | `infAdProd` | V01 | As informações adicionais do produto; opcional. |
| `Observacoes` | `ObservacoesItem?` | `obsItem` | VA01 | As observações do item; opcional. |
| `ValorTotal` | `decimal?` | `vItem` | VB01 | O valor total do item, a participação dele no total da nota; opcional. |
| `Referencia` | `ItemReferenciado?` | `DFeReferenciado` | VC01 | O item de outro documento a que este se refere; opcional. |

## ItemReferenciado

Classe selada · Namespace `OpenDFe.NFe`

O item de outro documento fiscal a que este item se refere (`DFeReferenciado`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ItemReferenciado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Chave` | `string?` | `chaveAcesso` | VC02 | A chave de acesso do documento referenciado. |
| `NumeroItem` | `int?` | `nItem` | VC03 | O número do item no documento referenciado; opcional. |

## Lacre

Classe selada · Namespace `OpenDFe.NFe`

Um lacre de volume (`lacres`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Lacre()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `string?` | `nLacre` | X34 | O número do lacre. |

## Local

Classe selada · Namespace `OpenDFe.NFe`

Um local de retirada (`retirada`) ou de entrega (`entrega`), o tipo `TLocal`.

Uma classe para os dois lugares: o [`TagAttribute`](opendfe.md#tagattribute) dos campos leva só a tag, sem o identificador do MOC, que muda com o lugar (`xLgr` é F03 na retirada e G03 na entrega); o identificador do grupo fica na propriedade da [`NotaFiscal`](#notafiscal).

### Construtores

| Construtor | Descrição |
|---|---|
| `Local()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` |  | O CNPJ; ou o [`Cpf`](#local) (um dos dois). |
| `Cpf` | `string?` | `CPF` |  | O CPF; ou o [`Cnpj`](#local) (um dos dois). |
| `Nome` | `string?` | `xNome` |  | A razão social ou o nome do expedidor ou do recebedor; opcional. |
| `Logradouro` | `string?` | `xLgr` |  | O logradouro. |
| `Numero` | `string?` | `nro` |  | O número. |
| `Complemento` | `string?` | `xCpl` |  | O complemento; opcional. |
| `Bairro` | `string?` | `xBairro` |  | O bairro. |
| `CodigoMunicipio` | `int?` | `cMun` |  | O código do município (IBGE); 9999999 no exterior. |
| `Municipio` | `string?` | `xMun` |  | O nome do município; `EXTERIOR` no exterior. |
| `UF` | `UF?` | `UF` |  | A UF; nula no exterior ([`Exterior`](#local)). |
| `Exterior` | `bool` |  |  | Se a UF do XML é `EX` (exterior); com ela, [`UF`](#local) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UF`. |
| `Cep` | `string?` | `CEP` |  | O CEP; opcional. |
| `CodigoPais` | `int?` | `cPais` |  | O código do país; opcional. |
| `Pais` | `string?` | `xPais` |  | O nome do país; opcional. |
| `Telefone` | `string?` | `fone` |  | O telefone; opcional. |
| `Email` | `string?` | `email` |  | O e-mail; opcional. |
| `InscricaoEstadual` | `string?` | `IE` |  | A inscrição estadual; opcional. |

## Medicamento

Classe selada · Namespace `OpenDFe.NFe`

O medicamento ou a matéria-prima farmacêutica (`med`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Medicamento()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `CodigoAnvisa` | `string?` | `cProdANVISA` | K01a | O número do registro na ANVISA, ou `ISENTO`. |
| `MotivoIsencao` | `string?` | `xMotivoIsencao` | K01b | O motivo da isenção do registro na ANVISA; opcional. |
| `PrecoMaximoConsumidor` | `decimal?` | `vPMC` | K06 | O preço máximo ao consumidor. |

## MensagemSefaz

Classe selada · Namespace `OpenDFe.NFe`

Uma mensagem da SEFAZ ao emissor, no protocolo (`cMsg` e `xMsg`). Sem grupo no XML: a sequência dos dois campos se repete dentro do `infProt`.

### Construtores

| Construtor | Descrição |
|---|---|
| `MensagemSefaz()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Codigo` | `int?` | `cMsg` | PR14 | O código da mensagem. |
| `Texto` | `string?` | `xMsg` | PR15 | O texto da mensagem. |

## MonofasiaCbsAdRem

Classe selada · Namespace `OpenDFe.NFe`

A CBS monofásica ad rem (`gCBSMonoAdRem`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdRem()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Padrao` | `MonofasiaCbsAdRemPadrao?` | `gMonoPadrao` | UB96 | A tributação monofásica padrão; opcional. |
| `Retencao` | `MonofasiaCbsAdRemRetencao?` | `gMonoReten` | UB97 | A tributação monofásica sujeita à retenção; opcional. |
| `Retido` | `MonofasiaCbsRetido?` | `gMonoRet` | UB98 | A tributação monofásica retida anteriormente; opcional. |
| `Biocombustivel` | `MonofasiaCbsBiocombustivel?` | `gpBioDiferenca` | UB99 | A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional. |

## MonofasiaCbsAdRemPadrao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica padrão da CBS ad rem (`gMonoPadrao`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdRemPadrao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCMono` | UB96a | A quantidade tributada no ICMS monofásico próprio. |
| `AliquotaAdRem` | `decimal?` | `adRemCBS` | UB96b | A alíquota ad rem da CBS. |
| `Valor` | `decimal?` | `vCBSMono` | UB96c | O valor da CBS monofásica. |

## MonofasiaCbsAdRemRetencao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica sujeita à retenção da CBS ad rem (`gMonoReten`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdRemRetencao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCMonoReten` | UB97a | A quantidade tributada no ICMS monofásico sujeito à retenção. |
| `AliquotaAdRem` | `decimal?` | `adRemCBSReten` | UB97b | A alíquota ad rem da CBS sujeita à retenção. |
| `Valor` | `decimal?` | `vCBSMonoReten` | UB97c | O valor da CBS monofásica sujeita à retenção. |

## MonofasiaCbsAdValorem

Classe selada · Namespace `OpenDFe.NFe`

A CBS monofásica ad valorem (`gCBSMonoAdValorem`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdValorem()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Padrao` | `MonofasiaCbsAdValoremPadrao?` | `gMonoPadrao` | UB101 | A tributação monofásica padrão; opcional. |
| `Retencao` | `MonofasiaCbsAdValoremRetencao?` | `gMonoReten` | UB102 | A tributação monofásica sujeita à retenção; opcional. |
| `Retido` | `MonofasiaCbsRetido?` | `gMonoRet` | UB103 | A tributação monofásica retida anteriormente; opcional. |
| `Biocombustivel` | `MonofasiaCbsBiocombustivel?` | `gpBioDiferenca` | UB104 | A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional. |

## MonofasiaCbsAdValoremPadrao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica padrão da CBS ad valorem (`gMonoPadrao`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdValoremPadrao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCMono` | UB101a | A base de cálculo da tributação monofásica. |
| `Aliquota` | `decimal?` | `pAliqMonoCBS` | UB101b | A alíquota da CBS. |
| `Valor` | `decimal?` | `vCBSMono` | UB101c | O valor da CBS monofásica. |

## MonofasiaCbsAdValoremRetencao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica sujeita à retenção da CBS ad valorem (`gMonoReten`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsAdValoremRetencao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCMonoReten` | UB102a | A base de cálculo da tributação monofásica sujeita à retenção. |
| `Aliquota` | `decimal?` | `pAliqMonoReten` | UB102b | A alíquota da tributação monofásica sujeita à retenção. |
| `Valor` | `decimal?` | `vCBSMonoReten` | UB102c | O valor da CBS monofásica sujeita à retenção. |

## MonofasiaCbsBiocombustivel

Classe selada · Namespace `OpenDFe.NFe`

A diferença da CBS pela mistura de biocombustível (EAC) com a gasolina A em percentual diferente do obrigatório (`gpBioDiferenca`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem da CBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsBiocombustivel()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCBioComb` |  | A quantidade de biocombustível (EAC) a recolher ou a ressarcir. |
| `Valor` | `decimal?` | `vCBSDiferenca` |  | O valor da CBS da diferença. |

## MonofasiaCbsRetido

Classe selada · Namespace `OpenDFe.NFe`

A CBS monofásica retida anteriormente (`gMonoRet`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem da CBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaCbsRetido()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Valor` | `decimal?` | `vCBSMonoRet` |  | O valor da CBS retida anteriormente. |

## MonofasiaIbsAdRem

Classe selada · Namespace `OpenDFe.NFe`

O IBS monofásico ad rem (`gIBSMonoAdRem`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdRem()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Padrao` | `MonofasiaIbsAdRemPadrao?` | `gMonoPadrao` | UB86 | A tributação monofásica padrão; opcional. |
| `Retencao` | `MonofasiaIbsAdRemRetencao?` | `gMonoReten` | UB87 | A tributação monofásica sujeita à retenção; opcional. |
| `Retido` | `MonofasiaIbsRetido?` | `gMonoRet` | UB88 | A tributação monofásica retida anteriormente; opcional. |
| `Biocombustivel` | `MonofasiaIbsBiocombustivel?` | `gpBioDiferenca` | UB89 | A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional. |

## MonofasiaIbsAdRemPadrao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica padrão do IBS ad rem (`gMonoPadrao`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdRemPadrao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCMono` | UB86a | A quantidade tributada no ICMS monofásico próprio. |
| `AliquotaAdRem` | `decimal?` | `adRemIBS` | UB86b | A alíquota ad rem do IBS. |
| `Valor` | `decimal?` | `vIBSMono` | UB86c | O valor do IBS monofásico. |

## MonofasiaIbsAdRemRetencao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica sujeita à retenção do IBS ad rem (`gMonoReten`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdRemRetencao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCMonoReten` | UB87a | A quantidade tributada no ICMS monofásico sujeito à retenção. |
| `AliquotaAdRem` | `decimal?` | `adRemIBSReten` | UB87b | A alíquota ad rem do IBS sujeito à retenção. |
| `Valor` | `decimal?` | `vIBSMonoReten` | UB87c | O valor do IBS monofásico sujeito à retenção. |

## MonofasiaIbsAdValorem

Classe selada · Namespace `OpenDFe.NFe`

O IBS monofásico ad valorem (`gIBSMonoAdValorem`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdValorem()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Padrao` | `MonofasiaIbsAdValoremPadrao?` | `gMonoPadrao` | UB91 | A tributação monofásica padrão; opcional. |
| `Retencao` | `MonofasiaIbsAdValoremRetencao?` | `gMonoReten` | UB92 | A tributação monofásica sujeita à retenção; opcional. |
| `Retido` | `MonofasiaIbsRetido?` | `gMonoRet` | UB93 | A tributação monofásica retida anteriormente; opcional. |
| `Biocombustivel` | `MonofasiaIbsBiocombustivel?` | `gpBioDiferenca` | UB94 | A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional. |

## MonofasiaIbsAdValoremPadrao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica padrão do IBS ad valorem (`gMonoPadrao`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdValoremPadrao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCMono` | UB91a | A base de cálculo da tributação monofásica. |
| `AliquotaUF` | `decimal?` | `pAliqMonoUF` | UB91b | A alíquota do IBS da UF. |
| `ValorUF` | `decimal?` | `vIBSMonoUF` | UB91c | O valor do IBS monofásico da UF. |
| `AliquotaMunicipio` | `decimal?` | `pAliqMonoMun` | UB91d | A alíquota do IBS do município. |
| `ValorMunicipio` | `decimal?` | `vIBSMonoMun` | UB91e | O valor do IBS monofásico do município. |
| `Valor` | `decimal?` | `vIBSMono` | UB91f | O valor do IBS monofásico. |

## MonofasiaIbsAdValoremRetencao

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica sujeita à retenção do IBS ad valorem (`gMonoReten`).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsAdValoremRetencao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCMonoReten` | UB92a | A base de cálculo da tributação monofásica sujeita à retenção. |
| `Aliquota` | `decimal?` | `pAliqMonoReten` | UB92b | A alíquota da tributação monofásica sujeita à retenção. |
| `Valor` | `decimal?` | `vIBSMonoReten` | UB92c | O valor do IBS monofásico sujeito à retenção. |

## MonofasiaIbsBiocombustivel

Classe selada · Namespace `OpenDFe.NFe`

A diferença do IBS pela mistura de biocombustível (EAC) com a gasolina A em percentual diferente do obrigatório (`gpBioDiferenca`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem do IBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsBiocombustivel()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCBioComb` |  | A quantidade de biocombustível (EAC) a recolher ou a ressarcir. |
| `Valor` | `decimal?` | `vIBSDiferenca` |  | O valor do IBS da diferença. |

## MonofasiaIbsCbs

Classe selada · Namespace `OpenDFe.NFe`

A tributação monofásica de combustíveis no IBS e na CBS (`gIBSCBSMono`).

O ad rem e o ad valorem de cada tributo são uma escolha do XSD: no IBS, no máximo um de [`IbsAdRem`](#monofasiaibscbs) e [`IbsAdValorem`](#monofasiaibscbs); na CBS, no máximo um de [`CbsAdRem`](#monofasiaibscbs) e [`CbsAdValorem`](#monofasiaibscbs).

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `IbsAdRem` | `MonofasiaIbsAdRem?` | `gIBSMonoAdRem` | UB85a | O IBS monofásico ad rem; ou o [`IbsAdValorem`](#monofasiaibscbs) (um dos dois). |
| `IbsAdValorem` | `MonofasiaIbsAdValorem?` | `gIBSMonoAdValorem` | UB90 | O IBS monofásico ad valorem; ou o [`IbsAdRem`](#monofasiaibscbs) (um dos dois). |
| `CbsAdRem` | `MonofasiaCbsAdRem?` | `gCBSMonoAdRem` | UB95a | A CBS monofásica ad rem; ou a [`CbsAdValorem`](#monofasiaibscbs) (uma das duas). |
| `CbsAdValorem` | `MonofasiaCbsAdValorem?` | `gCBSMonoAdValorem` | UB100 | A CBS monofásica ad valorem; ou a [`CbsAdRem`](#monofasiaibscbs) (uma das duas). |
| `TotalIbs` | `decimal?` | `vTotIBSMonoItem` | UB105a | O total do IBS monofásico do item. |
| `TotalCbs` | `decimal?` | `vTotCBSMonoItem` | UB105b | O total da CBS monofásica do item. |

## MonofasiaIbsRetido

Classe selada · Namespace `OpenDFe.NFe`

O IBS monofásico retido anteriormente (`gMonoRet`).

Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem do IBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `MonofasiaIbsRetido()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Valor` | `decimal?` | `vIBSMonoRet` |  | O valor do IBS retido anteriormente. |

## NFeReferenciada

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Uma NF-e ou NFC-e referenciada pela chave de acesso (`refNFe`).

### Construtores

| Construtor | Descrição |
|---|---|
| `NFeReferenciada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Chave` | `string?` | `refNFe` | BA02 | A chave de acesso (44 caracteres). |

## NFeSigilosaReferenciada

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Uma NF-e referenciada pela chave com o código numérico zerado, para manter o sigilo dela (`refNFeSig`).

### Construtores

| Construtor | Descrição |
|---|---|
| `NFeSigilosaReferenciada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Chave` | `string?` | `refNFeSig` | BA02a | A chave de acesso, com o `cNF` zerado. |

## NotaFiscal

Classe selada · Namespace `OpenDFe.NFe`

Uma NF-e (modelo 55) ou NFC-e (modelo 65): tudo o que vai dentro do `infNFe` e do `infNFeSupl`.

Preenchida por quem emite e devolvida pela leitura (docs/MODELO.md). Mutável e sem conferência: o que está errado aparece como [`Problema`](opendfe.md#problema) na montagem (schema, regras), nunca como exceção do modelo. Não é segura para alteração concorrente; a lib nunca altera a nota que recebe. Ficam fora do modelo o `Id` e a `versao` do `infNFe` (a chave e a versão do leiaute), o `cDV` e a assinatura.

### Construtores

| Construtor | Descrição |
|---|---|
| `NotaFiscal()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Identificacao` | `Identificacao` | `ide` | B01 | A identificação da nota. |
| `Emitente` | `Emitente` | `emit` | C01 | O emitente. |
| `Avulsa` | `FiscoEmitente?` | `avulsa` | D01 | O fisco que emitiu a nota avulsa (uso exclusivo do fisco); opcional. |
| `Destinatario` | `Destinatario?` | `dest` | E01 | O destinatário; opcional (na NFC-e, o consumidor, quando identificado). |
| `Retirada` | `Local?` | `retirada` | F01 | O local de retirada, quando diferente do endereço do emitente; opcional. |
| `Entrega` | `Local?` | `entrega` | G01 | O local de entrega, quando diferente do endereço do destinatário; opcional. |
| `AutorizadosXml` | `IList<PessoaAutorizada>` | `autXML` | GA01 | As pessoas autorizadas a obter o XML (até 10). |
| `Itens` | `IList<Item>` | `det` | H01 | Os itens da nota (de 1 a 990); o `nItem` é a posição na lista, a partir de 1. |
| `Total` | `Total` | `total` | W01 | Os totais da nota. |
| `Transporte` | `Transporte` | `transp` | X01 | O transporte. |
| `Cobranca` | `Cobranca?` | `cobr` | Y01 | A cobrança (fatura e duplicatas); opcional. |
| `Pagamento` | `Pagamento` | `pag` | YA01 | O pagamento. |
| `Intermediador` | `Intermediador?` | `infIntermed` | YB01 | O intermediador da transação; opcional. |
| `InformacoesAdicionais` | `InformacoesAdicionais?` | `infAdic` | Z01 | As informações adicionais; opcional. |
| `Exportacao` | `Exportacao?` | `exporta` | ZA01 | A exportação; opcional. |
| `Compra` | `Compra?` | `compra` | ZB01 | A compra (nota de empenho, pedido, contrato); opcional. |
| `Cana` | `AquisicaoCana?` | `cana` | ZC01 | A aquisição de cana; opcional. |
| `ResponsavelTecnico` | `ResponsavelTecnico?` | `infRespTec` | ZD01 | O responsável técnico pelo sistema emissor; opcional. |
| `SolicitacaoNff` | `SolicitacaoNff?` | `infSolicNFF` | ZE01 | A solicitação da NFF (nota fiscal fácil); opcional. |
| `Agropecuario` | `Agropecuario?` | `agropecuario` | ZF01 | Os produtos agropecuários (defensivos ou guia de trânsito); opcional. |
| `ProvedorAssinatura` | `ProvedorAssinatura?` | `infPAA` | ZG01 | O provedor de assinatura e autorização (PAA); opcional. |
| `Suplementares` | `InformacoesSuplementares?` | `infNFeSupl` | ZX01 | As informações suplementares da NFC-e (QR Code e URL da consulta). No XML, irmão do `infNFe`, e não filho. |

## NotaModelo1Referenciada

Classe selada · Tag `refNF` (BA03) · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Uma nota fiscal modelo 1 ou 2 referenciada (`refNF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `NotaModelo1Referenciada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `UF` | `UF?` | `cUF` | BA04 | A UF do emitente. |
| `AnoMes` | `string?` | `AAMM` | BA05 | O ano e o mês da emissão, como no XML (`AAMM`). |
| `Cnpj` | `string?` | `CNPJ` | BA06 | O CNPJ do emitente. |
| `Modelo` | `ModeloNotaReferenciada?` | `mod` | BA07 | O modelo (01 ou 02). |
| `Serie` | `int?` | `serie` | BA08 | A série (0 se não houver). |
| `Numero` | `int?` | `nNF` | BA09 | O número. |

## NotaProdutorReferenciada

Classe selada · Tag `refNFP` (BA10) · Namespace `OpenDFe.NFe` · Deriva de [`DocumentoReferenciado`](#documentoreferenciado)

Uma nota fiscal de produtor rural referenciada (`refNFP`).

### Construtores

| Construtor | Descrição |
|---|---|
| `NotaProdutorReferenciada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `UF` | `UF?` | `cUF` | BA11 | A UF do emitente. |
| `AnoMes` | `string?` | `AAMM` | BA12 | O ano e o mês da emissão, como no XML (`AAMM`). |
| `Cnpj` | `string?` | `CNPJ` | BA13 | O CNPJ do emitente; ou o [`Cpf`](#notaprodutorreferenciada) (um dos dois). |
| `Cpf` | `string?` | `CPF` | BA14 | O CPF do emitente; ou o [`Cnpj`](#notaprodutorreferenciada) (um dos dois). |
| `InscricaoEstadual` | `string?` | `IE` | BA15 | A inscrição estadual do emitente, ou `ISENTO`. |
| `Modelo` | `ModeloNotaProdutor?` | `mod` | BA16 | O modelo (01 ou 04). |
| `Serie` | `int?` | `serie` | BA17 | A série (0 se não houver). |
| `Numero` | `int?` | `nNF` | BA18 | O número. |

## Observacao

Classe selada · Namespace `OpenDFe.NFe`

Uma observação de uso livre: do contribuinte (`obsCont`) ou do fisco (`obsFisco`), na nota ou no item.

Uma classe para os quatro lugares, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: na nota, Z05 e Z06 no contribuinte, Z08 e Z09 no fisco; no item (`obsItem`), VA03 e VA04 no contribuinte, VA06 e VA07 no fisco.

### Construtores

| Construtor | Descrição |
|---|---|
| `Observacao()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Campo` | `string?` | `xCampo` |  | A identificação do campo (o atributo `xCampo`). |
| `Texto` | `string?` | `xTexto` |  | O conteúdo. |

## ObservacoesItem

Classe selada · Namespace `OpenDFe.NFe`

As observações do item (`obsItem`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ObservacoesItem()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Contribuinte` | `Observacao?` | `obsCont` | VA02 | A observação do contribuinte; opcional. |
| `Fisco` | `Observacao?` | `obsFisco` | VA05 | A observação do fisco; opcional. |

## OrigemCombustivel

Classe selada · Namespace `OpenDFe.NFe`

Uma origem do combustível (`origComb`).

### Construtores

| Construtor | Descrição |
|---|---|
| `OrigemCombustivel()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Importacao` | `IndicadorImportacao?` | `indImport` | LA19 | Se o combustível é nacional ou importado. |
| `UF` | `UF?` | `cUFOrig` | LA20 | A UF de origem do produtor ou do importador. |
| `Percentual` | `decimal?` | `pOrig` | LA21 | O percentual originário da UF. |

## Pagamento

Classe selada · Namespace `OpenDFe.NFe`

O pagamento (`pag`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Pagamento()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Formas` | `IList<FormaPagamento>` | `detPag` | YA01a | As formas de pagamento (de 1 a 100). |
| `Troco` | `decimal?` | `vTroco` | YA09 | O valor do troco; opcional. |

## PagamentoAntecipado

Classe selada · Namespace `OpenDFe.NFe`

As NF-e de antecipação de pagamento que esta abate (`gPagAntecipado`, NT 2025.002).

### Construtores

| Construtor | Descrição |
|---|---|
| `PagamentoAntecipado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Chaves` | `IList<string>` | `refNFe` | BC02 | As chaves de acesso (de 1 a 99). |

## PessoaAutorizada

Classe selada · Namespace `OpenDFe.NFe`

Uma pessoa autorizada a obter o XML da nota (`autXML`).

### Construtores

| Construtor | Descrição |
|---|---|
| `PessoaAutorizada()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | GA02 | O CNPJ; ou o [`Cpf`](#pessoaautorizada) (um dos dois). |
| `Cpf` | `string?` | `CPF` | GA03 | O CPF; ou o [`Cnpj`](#pessoaautorizada) (um dos dois). |

## Pis

Classe abstrata · Namespace `OpenDFe.NFe`

O PIS do item (`PIS`): exatamente um dos grupos do XML, uma subclasse para cada.

Hierarquia fechada: só a lib deriva; quem faz `switch` pelo tipo precisa de um caso padrão. O CST está em todos os grupos, com a mesma tag e o mesmo identificador, e fica aqui.

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `CstPisCofins?` | `CST` | Q06 | O CST, o mesmo campo em todos os grupos. Nulo na escrita, no [`PisQuantidade`](#pisquantidade): o único valor dele (`03`). |

## PisAliquota

Classe selada · Tag `PISAliq` (Q02) · Namespace `OpenDFe.NFe` · Deriva de [`Pis`](#pis)

O PIS tributado pela alíquota (`PISAliq`, CST 01 ou 02).

### Construtores

| Construtor | Descrição |
|---|---|
| `PisAliquota()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | Q07 | A base de cálculo. |
| `Aliquota` | `decimal?` | `pPIS` | Q08 | A alíquota, em percentual. |
| `Valor` | `decimal?` | `vPIS` | Q09 | O valor do PIS. |

## PisNaoTributado

Classe selada · Tag `PISNT` (Q04) · Namespace `OpenDFe.NFe` · Deriva de [`Pis`](#pis)

O PIS não tributado (`PISNT`, CST 04 a 09).

Só o [`Cst`](#pis).

### Construtores

| Construtor | Descrição |
|---|---|
| `PisNaoTributado()` |  |

## PisOutros

Classe selada · Tag `PISOutr` (Q05) · Namespace `OpenDFe.NFe` · Deriva de [`Pis`](#pis)

O PIS de outras operações (`PISOutr`, CST 49 a 99).

A base e a alíquota ([`BaseCalculo`](#pisoutros) e [`Aliquota`](#pisoutros)) ou a quantidade e o valor por unidade ([`Quantidade`](#pisoutros) e [`AliquotaUnidade`](#pisoutros)): escolha do XSD.

### Construtores

| Construtor | Descrição |
|---|---|
| `PisOutros()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | Q07 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pPIS` | Q08 | A alíquota, em percentual; opcional. |
| `Quantidade` | `decimal?` | `qBCProd` | Q10 | A quantidade vendida; opcional. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | Q11 | A alíquota em reais, por unidade; opcional. |
| `Valor` | `decimal?` | `vPIS` | Q09 | O valor do PIS. |

## PisQuantidade

Classe selada · Tag `PISQtde` (Q03) · Namespace `OpenDFe.NFe` · Deriva de [`Pis`](#pis)

O PIS tributado por quantidade (`PISQtde`, CST 03).

### Construtores

| Construtor | Descrição |
|---|---|
| `PisQuantidade()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `decimal?` | `qBCProd` | Q10 | A quantidade vendida. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | Q11 | A alíquota em reais, por unidade. |
| `Valor` | `decimal?` | `vPIS` | Q09 | O valor do PIS. |

## PisSt

Classe selada · Namespace `OpenDFe.NFe`

O PIS por substituição tributária (`PISST`).

A base e a alíquota ([`BaseCalculo`](#pisst) e [`Aliquota`](#pisst)) ou a quantidade e o valor por unidade ([`Quantidade`](#pisst) e [`AliquotaUnidade`](#pisst)): escolha do XSD.

### Construtores

| Construtor | Descrição |
|---|---|
| `PisSt()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | R02 | A base de cálculo; opcional. |
| `Aliquota` | `decimal?` | `pPIS` | R03 | A alíquota, em percentual; opcional. |
| `Quantidade` | `decimal?` | `qBCProd` | R04 | A quantidade vendida; opcional. |
| `AliquotaUnidade` | `decimal?` | `vAliqProd` | R05 | A alíquota em reais, por unidade; opcional. |
| `Valor` | `decimal?` | `vPIS` | R06 | O valor do PIS. |
| `Totalizacao` | `IndicadorTotal?` | `indSomaPISST` | R07 | Se o valor do PIS ST compõe o total da nota; opcional. |

## ProcessoReferenciado

Classe selada · Namespace `OpenDFe.NFe`

Um processo referenciado (`procRef`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ProcessoReferenciado()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Numero` | `string?` | `nProc` | Z11 | O número do processo, ato concessório ou benefício. |
| `Origem` | `OrigemProcesso?` | `indProc` | Z12 | A origem do processo. |
| `TipoAto` | `TipoAtoConcessorio?` | `tpAto` | Z13 | O tipo do ato concessório; opcional. |

## Produto

Classe selada · Namespace `OpenDFe.NFe`

O produto ou serviço do item (`prod`).

Os grupos específicos ([`VeiculoNovo`](#produto), [`Medicamento`](#produto), [`Armas`](#produto), [`Combustivel`](#produto), [`Recopi`](#produto)) são uma escolha do XSD: no máximo um vai preenchido (a lista de armas não vazia conta como escolhida); mais de um é erro na montagem.

### Construtores

| Construtor | Descrição |
|---|---|
| `Produto()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Codigo` | `string?` | `cProd` | I02 | O código do produto ou serviço (ou `CFOP9999`, sem codificação própria). |
| `Gtin` | `string?` | `cEAN` | I03 | O GTIN do produto (o antigo código EAN), ou `SEM GTIN`. |
| `CodigoBarras` | `string?` | `cBarra` | I03a | O código de barras diferente do padrão GTIN; opcional. |
| `Descricao` | `string?` | `xProd` | I04 | A descrição do produto ou serviço. |
| `Ncm` | `string?` | `NCM` | I05 | O código NCM (8 dígitos, ou o capítulo com 2). |
| `Nve` | `IList<string>` | `NVE` | I05a | As nomenclaturas de valor aduaneiro e estatístico (até 8). |
| `Cest` | `string?` | `CEST` | I05c | O código especificador da substituição tributária (CEST); opcional. |
| `Escala` | `IndicadorEscala?` | `indEscala` | I05d | Se o produto é fabricado em escala relevante; opcional. |
| `CnpjFabricante` | `string?` | `CNPJFab` | I05e | O CNPJ do fabricante da mercadoria, quando produzida em escala não relevante; opcional. |
| `CodigoBeneficio` | `string?` | `cBenef` | I05f | O código de benefício fiscal na UF; opcional. |
| `CreditosPresumidos` | `IList<CreditoPresumidoIcms>` | `gCred` | I05g | Os créditos presumidos do ICMS (até 4). |
| `ClassificacaoZfm` | `ClassificacaoCreditoPresumidoZfm?` | `tpCredPresIBSZFM` | I05k | A classificação para o crédito presumido do IBS na Zona Franca de Manaus; opcional. |
| `ExTipi` | `string?` | `EXTIPI` | I06 | O código EX da TIPI; opcional. |
| `Cfop` | `int?` | `CFOP` | I08 | O CFOP. |
| `UnidadeComercial` | `string?` | `uCom` | I09 | A unidade comercial. |
| `QuantidadeComercial` | `decimal?` | `qCom` | I10 | A quantidade comercial. |
| `ValorUnitarioComercial` | `decimal?` | `vUnCom` | I10a | O valor unitário de comercialização. |
| `Valor` | `decimal?` | `vProd` | I11 | O valor bruto do produto ou serviço. |
| `GtinTributavel` | `string?` | `cEANTrib` | I12 | O GTIN da unidade tributável, ou `SEM GTIN`. |
| `CodigoBarrasTributavel` | `string?` | `cBarraTrib` | I12a | O código de barras da unidade tributável diferente do padrão GTIN; opcional. |
| `UnidadeTributavel` | `string?` | `uTrib` | I13 | A unidade tributável. |
| `QuantidadeTributavel` | `decimal?` | `qTrib` | I14 | A quantidade tributável. |
| `ValorUnitarioTributavel` | `decimal?` | `vUnTrib` | I14a | O valor unitário de tributação. |
| `Frete` | `decimal?` | `vFrete` | I15 | O valor do frete; opcional. |
| `Seguro` | `decimal?` | `vSeg` | I16 | O valor do seguro; opcional. |
| `Desconto` | `decimal?` | `vDesc` | I17 | O valor do desconto; opcional. |
| `OutrasDespesas` | `decimal?` | `vOutro` | I17a | As outras despesas acessórias; opcional. |
| `Totalizacao` | `IndicadorTotal?` | `indTot` | I17b | Se o valor do item compõe o total da nota (`vProd`). |
| `BemMovelUsado` | `IndicadorBemMovelUsado?` | `indBemMovelUsado` | I17c | Se o item é fornecimento de bem móvel usado; opcional. |
| `Importacoes` | `IList<DeclaracaoImportacao>` | `DI` | I18 | As declarações de importação (até 100). |
| `Exportacoes` | `IList<DetalheExportacao>` | `detExport` | I50 | Os detalhes da exportação (até 500). |
| `Pedido` | `string?` | `xPed` | I60 | O número do pedido de compra; opcional. |
| `ItemPedido` | `string?` | `nItemPed` | I61 | O item do pedido de compra; opcional. |
| `NumeroFci` | `string?` | `nFCI` | I70 | O número de controle da FCI (ficha de conteúdo de importação), o GUID em maiúsculas; opcional. |
| `Rastros` | `IList<Rastro>` | `rastro` | I80 | Os lotes do produto, para a rastreabilidade (até 500). |
| `Nff` | `ProdutoNff?` | `infProdNFF` | I86 | As informações do produto na NFF; opcional. |
| `Embalagem` | `EmbalagemProduto?` | `infProdEmb` | I87 | A embalagem do produto; opcional. |
| `VeiculoNovo` | `VeiculoNovo?` | `veicProd` | J01 | O veículo novo; opcional. |
| `Medicamento` | `Medicamento?` | `med` | K01 | O medicamento ou a matéria-prima farmacêutica; opcional. |
| `Armas` | `IList<Arma>` | `arma` | L01 | As armas de fogo (até 500). |
| `Combustivel` | `Combustivel?` | `comb` | LA01 | O combustível; opcional. |
| `Recopi` | `string?` | `nRECOPI` | LB01 | O número do RECOPI (papel imune); opcional. |

## ProdutoNff

Classe selada · Namespace `OpenDFe.NFe`

As informações do produto na NFF, nota fiscal fácil (`infProdNFF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `ProdutoNff()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `CodigoFisco` | `string?` | `cProdFisco` | I86a | O código fiscal do produto. |
| `CodigoOperacao` | `int?` | `cOperNFF` | I86b | O código da operação escolhida na NFF para o item. |

## Protocolo

Classe selada · Namespace `OpenDFe.NFe`

O protocolo do processamento de uma NF-e pela SEFAZ (`protNFe`, com o `infProt` achatado): autorizada, denegada ou rejeitada.

Vem no `nfeProc` (a leitura, docs/LEITURA.md) e, na fase 4, no retorno da autorização, da consulta do recibo e da consulta do protocolo. Segue as regras de forma do modelo (MODELO.md 3): a leitura preenche campo a campo, e o valor ilegível fica nulo. A `versao` do `protNFe` e a `Signature` (opcional) ficam fora, como as da nota.

### Construtores

| Construtor | Descrição |
|---|---|
| `Protocolo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Id` | `string?` | `Id` | PR04 | O identificador do `infProt`, quando a SEFAZ assina a resposta (`ID` + o número do protocolo). Opcional. |
| `Ambiente` | `Ambiente?` | `tpAmb` | PR05 | O ambiente em que a nota foi processada. |
| `VersaoAplicativo` | `string?` | `verAplic` | PR06 | A versão do aplicativo da SEFAZ que processou a nota. |
| `Chave` | `string?` | `chNFe` | PR07 | A chave de acesso da nota, como texto (MODELO.md, decisão 7). |
| `Recebimento` | `DateTimeOffset?` | `dhRecbto` | PR08 | A data e a hora do processamento (também na rejeição). |
| `Numero` | `string?` | `nProt` | PR09 | O número do protocolo (15 dígitos); ausente na rejeição. |
| `Digest` | `string?` | `digVal` | PR10 | O `DigestValue` da nota processada, em base64: confere a integridade da nota original. |
| `Status` | `int?` | `cStat` | PR11 | O código do status da nota: 100 e 150 autorizada; 110, 301 e 302 denegada; ou o da rejeição. |
| `Motivo` | `string?` | `xMotivo` | PR12 | A descrição do status. |
| `Mensagens` | `IList<MensagemSefaz>` |  |  | As mensagens da SEFAZ ao emissor: a sequência `cMsg` + `xMsg` (PR13), até 5. |

## ProvedorAssinatura

Classe selada · Namespace `OpenDFe.NFe`

O provedor de assinatura e autorização, PAA (`infPAA`): a nota gerada por ele ([`ProvedorAssinatura`](opendfe-nfe-modelo-enumeracoes.md#processoemissao)).

### Construtores

| Construtor | Descrição |
|---|---|
| `ProvedorAssinatura()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJPAA` | ZG02 | O CNPJ do provedor. |
| `Assinatura` | `AssinaturaProvedor` | `PAASignature` | ZG03 | A assinatura do provedor. |

## Rastro

Classe selada · Namespace `OpenDFe.NFe`

Um lote do produto, para a rastreabilidade (`rastro`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Rastro()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Lote` | `string?` | `nLote` | I81 | O número do lote. |
| `Quantidade` | `decimal?` | `qLote` | I82 | A quantidade do produto no lote. |
| `Fabricacao` | `DateOnly?` | `dFab` | I83 | A data de fabricação ou produção. |
| `Validade` | `DateOnly?` | `dVal` | I84 | A data de validade. |
| `CodigoAgregacao` | `string?` | `cAgreg` | I85 | O código de agregação; opcional. |

## ReducaoAliquota

Classe selada · Namespace `OpenDFe.NFe`

A redução da alíquota de um tributo da reforma (`gRed`).

Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag: o identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e na CBS.

### Construtores

| Construtor | Descrição |
|---|---|
| `ReducaoAliquota()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Percentual` | `decimal?` | `pRedAliq` |  | O percentual de redução da alíquota. |
| `AliquotaEfetiva` | `decimal?` | `pAliqEfet` |  | A alíquota efetiva, depois da redução. |

## ResponsavelTecnico

Classe selada · Namespace `OpenDFe.NFe`

O responsável técnico pelo sistema emissor (`infRespTec`).

O modelo guarda o que está no XML. A conta do hash, que precisa da chave já montada e do CSRT secreto, fica com a configuração do documento, fora do modelo.

### Construtores

| Construtor | Descrição |
|---|---|
| `ResponsavelTecnico()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | ZD02 | O CNPJ. |
| `Contato` | `string?` | `xContato` | ZD04 | O nome da pessoa de contato. |
| `Email` | `string?` | `email` | ZD05 | O e-mail. |
| `Telefone` | `string?` | `fone` | ZD06 | O telefone. |
| `IdCsrt` | `int?` | `idCSRT` | ZD08 | O identificador do CSRT; com o [`HashCsrt`](#responsaveltecnico). |
| `HashCsrt` | `string?` | `hashCSRT` | ZD09 | O hash do CSRT, em base64, como no XML; com o [`IdCsrt`](#responsaveltecnico). |

## RetencaoIcmsTransporte

Classe selada · Namespace `OpenDFe.NFe`

A retenção do ICMS do transporte (`retTransp`).

### Construtores

| Construtor | Descrição |
|---|---|
| `RetencaoIcmsTransporte()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `ValorServico` | `decimal?` | `vServ` | X12 | O valor do serviço. |
| `BaseCalculo` | `decimal?` | `vBCRet` | X13 | A base de cálculo da retenção. |
| `Aliquota` | `decimal?` | `pICMSRet` | X14 | A alíquota da retenção. |
| `Valor` | `decimal?` | `vICMSRet` | X15 | O valor do ICMS retido. |
| `Cfop` | `int?` | `CFOP` | X16 | O CFOP do serviço de transporte. |
| `MunicipioFatoGerador` | `int?` | `cMunFG` | X17 | O município do fato gerador do ICMS do transporte (IBGE). |

## RetencaoTributos

Classe selada · Namespace `OpenDFe.NFe`

As retenções de tributos (`retTrib`).

### Construtores

| Construtor | Descrição |
|---|---|
| `RetencaoTributos()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Pis` | `decimal?` | `vRetPIS` | W24 | O valor retido do PIS; opcional. |
| `Cofins` | `decimal?` | `vRetCOFINS` | W25 | O valor retido da COFINS; opcional. |
| `Csll` | `decimal?` | `vRetCSLL` | W26 | O valor retido da CSLL; opcional. |
| `BaseCalculoIrrf` | `decimal?` | `vBCIRRF` | W27 | A base de cálculo do IRRF; opcional. |
| `Irrf` | `decimal?` | `vIRRF` | W28 | O valor retido do IRRF; opcional. |
| `BaseCalculoPrevidencia` | `decimal?` | `vBCRetPrev` | W29 | A base de cálculo da retenção da previdência social; opcional. |
| `Previdencia` | `decimal?` | `vRetPrev` | W30 | O valor retido da previdência social; opcional. |

## SolicitacaoNff

Classe selada · Namespace `OpenDFe.NFe`

A solicitação da NFF, nota fiscal fácil (`infSolicNFF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `SolicitacaoNff()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Texto` | `string?` | `xSolic` | ZE02 | A solicitação do pedido de emissão da NFF. |

## Total

Classe selada · Namespace `OpenDFe.NFe`

Os totais da nota (`total`).

Preenchidos por quem emite (ou pela leitura): o modelo não os calcula, e a conferência contra a soma dos itens é regra (rejeições 531 a 535, 564 e as da reforma).

### Construtores

| Construtor | Descrição |
|---|---|
| `Total()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Icms` | `TotalIcms` | `ICMSTot` | W02 | Os totais do ICMS e da nota. |
| `Issqn` | `TotalIssqn?` | `ISSQNtot` | W17 | Os totais do ISSQN; opcional. |
| `Retencao` | `RetencaoTributos?` | `retTrib` | W23 | As retenções de tributos; opcional. |
| `ImpostoSeletivo` | `TotalImpostoSeletivo?` | `ISTot` | W31 | O total do imposto seletivo; opcional. |
| `IbsCbs` | `TotalIbsCbs?` | `IBSCBSTot` | W34 | Os totais do IBS e da CBS; opcional. |
| `ValorTotalNota` | `decimal?` | `vNFTot` | W60 | O valor total da nota com o IBS, a CBS e o IS, que são cobrados por fora; opcional. |

## TotalCbs

Classe selada · Namespace `OpenDFe.NFe`

Os totais da CBS (`gCBS`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Diferimento` | `decimal?` | `vDif` | W53 | O total do diferimento da CBS. |
| `Devolucao` | `decimal?` | `vDevTrib` | W54 | O total da devolução da CBS. |
| `Valor` | `decimal?` | `vCBS` | W56 | O total da CBS. |
| `CreditoPresumido` | `decimal?` | `vCredPres` | W56a | O total do crédito presumido da CBS. |
| `CreditoPresumidoCondicaoSuspensiva` | `decimal?` | `vCredPresCondSus` | W56b | O total do crédito presumido da CBS em condição suspensiva. |

## TotalIbs

Classe selada · Namespace `OpenDFe.NFe`

Os totais do IBS (`gIBS`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `UF` | `TotalIbsUF` | `gIBSUF` | W37 | Os totais do IBS da UF. |
| `Municipio` | `TotalIbsMunicipio` | `gIBSMun` | W42 | Os totais do IBS do município. |
| `Valor` | `decimal?` | `vIBS` | W47 | O total do IBS. |
| `CreditoPresumido` | `decimal?` | `vCredPres` | W48 | O total do crédito presumido do IBS. |
| `CreditoPresumidoCondicaoSuspensiva` | `decimal?` | `vCredPresCondSus` | W49 | O total do crédito presumido do IBS em condição suspensiva. |

## TotalIbsCbs

Classe selada · Namespace `OpenDFe.NFe`

Os totais do IBS e da CBS (`IBSCBSTot`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIbsCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBCIBSCBS` | W35 | O total da base de cálculo do IBS e da CBS. |
| `Ibs` | `TotalIbs?` | `gIBS` | W36 | Os totais do IBS; opcional. |
| `Cbs` | `TotalCbs?` | `gCBS` | W50 | Os totais da CBS; opcional. |
| `Monofasia` | `TotalMonofasia?` | `gMono` | W57 | Os totais da tributação monofásica; opcional. |
| `EstornoCredito` | `EstornoCredito?` | `gEstornoCred` | W59e | Os totais do estorno de crédito; opcional. |

## TotalIbsMunicipio

Classe selada · Namespace `OpenDFe.NFe`

Os totais do IBS do município (`gIBSMun`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIbsMunicipio()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Diferimento` | `decimal?` | `vDif` | W43 | O total do diferimento do IBS do município. |
| `Devolucao` | `decimal?` | `vDevTrib` | W44 | O total da devolução do IBS do município. |
| `Valor` | `decimal?` | `vIBSMun` | W46 | O total do IBS do município. |

## TotalIbsUF

Classe selada · Namespace `OpenDFe.NFe`

Os totais do IBS da UF (`gIBSUF`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIbsUF()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Diferimento` | `decimal?` | `vDif` | W38 | O total do diferimento do IBS da UF. |
| `Devolucao` | `decimal?` | `vDevTrib` | W39 | O total da devolução do IBS da UF. |
| `Valor` | `decimal?` | `vIBSUF` | W41 | O total do IBS da UF. |

## TotalIcms

Classe selada · Namespace `OpenDFe.NFe`

Os totais do ICMS e dos valores da nota (`ICMSTot`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIcms()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | W03 | O total da base de cálculo do ICMS. |
| `Valor` | `decimal?` | `vICMS` | W04 | O total do ICMS. |
| `ValorDesonerado` | `decimal?` | `vICMSDeson` | W04a | O total do ICMS desonerado. |
| `ValorFcpUFDestino` | `decimal?` | `vFCPUFDest` | W04c | O total do FCP da UF de destino; opcional. |
| `ValorIcmsUFDestino` | `decimal?` | `vICMSUFDest` | W04e | O total do ICMS de partilha para a UF de destino; opcional. |
| `ValorIcmsUFRemetente` | `decimal?` | `vICMSUFRemet` | W04g | O total do ICMS de partilha para a UF do remetente; opcional. |
| `ValorFcp` | `decimal?` | `vFCP` | W04h | O total do FCP. |
| `BaseCalculoSt` | `decimal?` | `vBCST` | W05 | O total da base de cálculo do ICMS ST. |
| `ValorSt` | `decimal?` | `vST` | W06 | O total do ICMS ST. |
| `ValorFcpSt` | `decimal?` | `vFCPST` | W06a | O total do FCP retido por substituição tributária. |
| `ValorFcpStRetido` | `decimal?` | `vFCPSTRet` | W06b | O total do FCP retido anteriormente por substituição tributária. |
| `QuantidadeBaseMonofasico` | `decimal?` | `qBCMono` | W06b.1 | O total da quantidade tributada no ICMS monofásico próprio; opcional. |
| `ValorMonofasico` | `decimal?` | `vICMSMono` | W06c | O total do ICMS monofásico próprio; opcional. |
| `QuantidadeBaseRetencao` | `decimal?` | `qBCMonoReten` | W06c.1 | O total da quantidade tributada no ICMS monofásico sujeito à retenção; opcional. |
| `ValorMonofasicoRetencao` | `decimal?` | `vICMSMonoReten` | W06d | O total do ICMS monofásico sujeito à retenção; opcional. |
| `QuantidadeBaseRetida` | `decimal?` | `qBCMonoRet` | W06d.1 | O total da quantidade tributada no ICMS monofásico retido anteriormente; opcional. |
| `ValorMonofasicoRetido` | `decimal?` | `vICMSMonoRet` | W06e | O total do ICMS monofásico retido anteriormente; opcional. |
| `ValorProdutos` | `decimal?` | `vProd` | W07 | O total dos produtos e serviços. |
| `Frete` | `decimal?` | `vFrete` | W08 | O total do frete. |
| `Seguro` | `decimal?` | `vSeg` | W09 | O total do seguro. |
| `Desconto` | `decimal?` | `vDesc` | W10 | O total do desconto. |
| `ValorImpostoImportacao` | `decimal?` | `vII` | W11 | O total do imposto de importação. |
| `ValorIpi` | `decimal?` | `vIPI` | W12 | O total do IPI. |
| `ValorIpiDevolvido` | `decimal?` | `vIPIDevol` | W12a | O total do IPI devolvido. |
| `ValorPis` | `decimal?` | `vPIS` | W13 | O total do PIS. |
| `ValorCofins` | `decimal?` | `vCOFINS` | W14 | O total da COFINS. |
| `OutrasDespesas` | `decimal?` | `vOutro` | W15 | O total das outras despesas acessórias. |
| `ValorNota` | `decimal?` | `vNF` | W16 | O valor total da nota (`vNF`). |
| `ValorTotalTributos` | `decimal?` | `vTotTrib` | W16a | O valor aproximado total dos tributos (Lei 12.741/2012); opcional. |

## TotalImpostoSeletivo

Classe selada · Namespace `OpenDFe.NFe`

O total do imposto seletivo (`ISTot`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalImpostoSeletivo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Valor` | `decimal?` | `vIS` | W33 | O total do imposto seletivo. |

## TotalIssqn

Classe selada · Namespace `OpenDFe.NFe`

Os totais do ISSQN (`ISSQNtot`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalIssqn()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `ValorServicos` | `decimal?` | `vServ` | W18 | O total dos serviços sob não incidência ou não tributados pelo ICMS; opcional. |
| `BaseCalculo` | `decimal?` | `vBC` | W19 | O total da base de cálculo do ISS; opcional. |
| `Valor` | `decimal?` | `vISS` | W20 | O total do ISS; opcional. |
| `ValorPis` | `decimal?` | `vPIS` | W21 | O total do PIS sobre os serviços; opcional. |
| `ValorCofins` | `decimal?` | `vCOFINS` | W22 | O total da COFINS sobre os serviços; opcional. |
| `Competencia` | `DateOnly?` | `dCompet` | W22a | A data da prestação do serviço. |
| `Deducao` | `decimal?` | `vDeducao` | W22b | O total da dedução, para a redução da base de cálculo; opcional. |
| `OutrasRetencoes` | `decimal?` | `vOutro` | W22c | O total de outras retenções; opcional. |
| `DescontoIncondicionado` | `decimal?` | `vDescIncond` | W22d | O total do desconto incondicionado; opcional. |
| `DescontoCondicionado` | `decimal?` | `vDescCond` | W22e | O total do desconto condicionado; opcional. |
| `Retencao` | `decimal?` | `vISSRet` | W22f | O total da retenção do ISS; opcional. |
| `RegimeEspecial` | `RegimeEspecialTributacao?` | `cRegTrib` | W22g | O regime especial de tributação; opcional. |

## TotalMonofasia

Classe selada · Namespace `OpenDFe.NFe`

Os totais da tributação monofásica do IBS e da CBS (`gMono`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TotalMonofasia()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Ibs` | `decimal?` | `vIBSMono` | W58 | O total do IBS monofásico. |
| `Cbs` | `decimal?` | `vCBSMono` | W59 | O total da CBS monofásica. |
| `IbsRetencao` | `decimal?` | `vIBSMonoReten` | W59a | O total do IBS monofásico sujeito à retenção. |
| `CbsRetencao` | `decimal?` | `vCBSMonoReten` | W59b | O total da CBS monofásica sujeita à retenção. |
| `IbsRetido` | `decimal?` | `vIBSMonoRet` | W59c | O total do IBS monofásico retido anteriormente. |
| `CbsRetido` | `decimal?` | `vCBSMonoRet` | W59d | O total da CBS monofásica retida anteriormente. |

## TransacaoEletronica

Classe selada · Namespace `OpenDFe.NFe`

Os dados de um pagamento eletrônico (`card`): cartões, PIX, boletos e outros.

### Construtores

| Construtor | Descrição |
|---|---|
| `TransacaoEletronica()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Integracao` | `TipoIntegracao?` | `tpIntegra` | YA04a | Se o pagamento é integrado ao sistema de automação. |
| `CnpjInstituicao` | `string?` | `CNPJ` | YA05 | O CNPJ da instituição de pagamento, adquirente ou subadquirente; opcional. |
| `Bandeira` | `BandeiraCartao?` | `tBand` | YA06 | A bandeira da operadora do cartão; opcional. |
| `Autorizacao` | `string?` | `cAut` | YA07 | O número da autorização da transação; opcional. |
| `CnpjBeneficiario` | `string?` | `CNPJReceb` | YA07a | O CNPJ do beneficiário do pagamento; opcional. |
| `Terminal` | `string?` | `idTermPag` | YA07b | O identificador do terminal de pagamento; opcional. |

## TransferenciaCredito

Classe selada · Namespace `OpenDFe.NFe`

A transferência de crédito do IBS e da CBS (`gTransfCred`, CST 800).

### Construtores

| Construtor | Descrição |
|---|---|
| `TransferenciaCredito()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `ValorIbs` | `decimal?` | `vIBS` | UB107 | O valor do IBS a transferir. |
| `ValorCbs` | `decimal?` | `vCBS` | UB108 | O valor da CBS a transferir. |

## Transportador

Classe selada · Namespace `OpenDFe.NFe`

O transportador (`transporta`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Transportador()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cnpj` | `string?` | `CNPJ` | X04 | O CNPJ; ou o [`Cpf`](#transportador) (um dos dois, ou nenhum). |
| `Cpf` | `string?` | `CPF` | X05 | O CPF; ou o [`Cnpj`](#transportador) (um dos dois, ou nenhum). |
| `Nome` | `string?` | `xNome` | X06 | A razão social ou o nome; opcional. |
| `InscricaoEstadual` | `string?` | `IE` | X07 | A inscrição estadual, ou `ISENTO`; opcional. |
| `Endereco` | `string?` | `xEnder` | X08 | O endereço completo, em texto; opcional. |
| `Municipio` | `string?` | `xMun` | X09 | O nome do município; opcional. |
| `UF` | `UF?` | `UF` | X10 | A UF; opcional, nula no exterior ([`Exterior`](#transportador)). |
| `Exterior` | `bool` |  |  | Se a UF do XML é `EX` (exterior); com ela, [`UF`](#transportador) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UF`. |

## Transporte

Classe selada · Namespace `OpenDFe.NFe`

O transporte (`transp`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Transporte()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Modalidade` | `ModalidadeFrete?` | `modFrete` | X02 | A modalidade do frete. |
| `Transportador` | `Transportador?` | `transporta` | X03 | O transportador; opcional. |
| `Retencao` | `RetencaoIcmsTransporte?` | `retTransp` | X11 | A retenção do ICMS do transporte; opcional. |
| `Veiculo` | `Veiculo?` | `veicTransp` | X18 | O veículo de tração; opcional, com os [`Reboques`](#transporte). Exclusivo com o [`Vagao`](#transporte) e a [`Balsa`](#transporte). |
| `Reboques` | `IList<Veiculo>` | `reboque` | X22 | Os reboques (até 5). |
| `Vagao` | `string?` | `vagao` | X25a | A identificação do vagão; opcional, exclusivo com o veículo e a balsa. |
| `Balsa` | `string?` | `balsa` | X25b | A identificação da balsa; opcional, exclusiva com o veículo e o vagão. |
| `Volumes` | `IList<Volume>` | `vol` | X26 | Os volumes (até 5000). |

## TributacaoCompraGovernamental

Classe selada · Namespace `OpenDFe.NFe`

A composição do IBS e da CBS na compra governamental (`gTribCompraGov`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TributacaoCompraGovernamental()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `AliquotaIbsUF` | `decimal?` | `pAliqIBSUF` | UB82b | A alíquota do IBS da UF na compra governamental. |
| `ValorIbsUF` | `decimal?` | `vTribIBSUF` | UB82c | O valor do IBS da UF na compra governamental. |
| `AliquotaIbsMunicipio` | `decimal?` | `pAliqIBSMun` | UB82d | A alíquota do IBS do município na compra governamental. |
| `ValorIbsMunicipio` | `decimal?` | `vTribIBSMun` | UB82e | O valor do IBS do município na compra governamental. |
| `AliquotaCbs` | `decimal?` | `pAliqCBS` | UB82f | A alíquota da CBS na compra governamental. |
| `ValorCbs` | `decimal?` | `vTribCBS` | UB82g | O valor da CBS na compra governamental. |

## TributacaoIbsCbs

Classe selada · Namespace `OpenDFe.NFe`

A tributação do IBS e da CBS (`gIBSCBS`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TributacaoIbsCbs()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `BaseCalculo` | `decimal?` | `vBC` | UB16 | A base de cálculo. |
| `IbsUF` | `IbsUF` | `gIBSUF` | UB17 | O IBS da UF. |
| `IbsMunicipio` | `IbsMunicipio` | `gIBSMun` | UB36 | O IBS do município. |
| `ValorIbs` | `decimal?` | `vIBS` | UB54a | O valor do IBS (o da UF mais o do município). |
| `Cbs` | `Cbs` | `gCBS` | UB55 | A CBS. |
| `TributacaoRegular` | `TributacaoRegular?` | `gTribRegular` | UB68 | A tributação regular, se não cumprida a condição resolutória ou suspensiva; opcional. |
| `CompraGovernamental` | `TributacaoCompraGovernamental?` | `gTribCompraGov` | UB82a | A composição do IBS e da CBS na compra governamental; opcional. |

## TributacaoRegular

Classe selada · Namespace `OpenDFe.NFe`

A tributação regular, como seria se não cumprida a condição resolutória ou suspensiva (`gTribRegular`).

### Construtores

| Construtor | Descrição |
|---|---|
| `TributacaoRegular()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Cst` | `string?` | `CSTReg` | UB69 | O CST da tributação regular, como no XML. |
| `ClassificacaoTributaria` | `string?` | `cClassTribReg` | UB70 | A classificação tributária da tributação regular, como no XML. |
| `AliquotaIbsUF` | `decimal?` | `pAliqEfetRegIBSUF` | UB71 | A alíquota efetiva do IBS da UF na tributação regular. |
| `ValorIbsUF` | `decimal?` | `vTribRegIBSUF` | UB72 | O valor do IBS da UF na tributação regular. |
| `AliquotaIbsMunicipio` | `decimal?` | `pAliqEfetRegIBSMun` | UB72a | A alíquota efetiva do IBS do município na tributação regular. |
| `ValorIbsMunicipio` | `decimal?` | `vTribRegIBSMun` | UB72b | O valor do IBS do município na tributação regular. |
| `AliquotaCbs` | `decimal?` | `pAliqEfetRegCBS` | UB72c | A alíquota efetiva da CBS na tributação regular. |
| `ValorCbs` | `decimal?` | `vTribRegCBS` | UB72d | O valor da CBS na tributação regular. |

## Veiculo

Classe selada · Namespace `OpenDFe.NFe`

Um veículo (o tipo `TVeiculo`): o de tração (`veicTransp`) ou um reboque (`reboque`).

Uma classe para os dois lugares, com o [`TagAttribute`](opendfe.md#tagattribute) só com a tag (X19 a X21 no de tração, X23 a X25 no reboque).

### Construtores

| Construtor | Descrição |
|---|---|
| `Veiculo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Placa` | `string?` | `placa` |  | A placa. |
| `UF` | `UF?` | `UF` |  | A UF; opcional, nula no exterior ([`Exterior`](#veiculo)). |
| `Exterior` | `bool` |  |  | Se a UF do XML é `EX` (exterior); com ela, [`UF`](#veiculo) fica nulo. Sem [`TagAttribute`](opendfe.md#tagattribute): é outro valor do mesmo campo `UF`. |
| `Rntc` | `string?` | `RNTC` |  | O registro nacional de transportador de carga (ANTT); opcional. |

## VeiculoNovo

Classe selada · Namespace `OpenDFe.NFe`

O veículo novo (`veicProd`).

Os códigos das tabelas do RENAVAM e do DENATRAN e os campos que o XSD tipa como texto (potência, cilindrada, pesos) ficam como texto, como no XML.

### Construtores

| Construtor | Descrição |
|---|---|
| `VeiculoNovo()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `TipoOperacao` | `TipoOperacaoVeiculo?` | `tpOp` | J02 | O tipo da operação. |
| `Chassi` | `string?` | `chassi` | J03 | O chassi (VIN). |
| `CodigoCor` | `string?` | `cCor` | J04 | O código da cor (de cada montadora). |
| `Cor` | `string?` | `xCor` | J05 | A descrição da cor. |
| `Potencia` | `string?` | `pot` | J06 | A potência máxima do motor, em cavalos-vapor, como no XML. |
| `Cilindrada` | `string?` | `cilin` | J07 | A cilindrada, em centímetros cúbicos, como no XML. |
| `PesoLiquido` | `string?` | `pesoL` | J08 | O peso líquido, como no XML. |
| `PesoBruto` | `string?` | `pesoB` | J09 | O peso bruto, como no XML. |
| `Serie` | `string?` | `nSerie` | J10 | O serial (série). |
| `TipoCombustivel` | `string?` | `tpComb` | J11 | O tipo de combustível (tabela do RENAVAM). |
| `Motor` | `string?` | `nMotor` | J12 | O número do motor. |
| `CapacidadeTracao` | `string?` | `CMT` | J13 | A capacidade máxima de tração, em toneladas, como no XML. |
| `DistanciaEixos` | `string?` | `dist` | J14 | A distância entre eixos, como no XML. |
| `AnoModelo` | `int?` | `anoMod` | J16 | O ano do modelo. |
| `AnoFabricacao` | `int?` | `anoFab` | J17 | O ano de fabricação. |
| `TipoPintura` | `string?` | `tpPint` | J18 | O tipo de pintura. |
| `TipoVeiculo` | `string?` | `tpVeic` | J19 | O tipo de veículo (tabela do RENAVAM). |
| `EspecieVeiculo` | `int?` | `espVeic` | J20 | A espécie de veículo (tabela do RENAVAM). |
| `Vin` | `CondicaoVin?` | `VIN` | J21 | Se o VIN (chassi) é remarcado. |
| `Condicao` | `CondicaoVeiculo?` | `condVeic` | J22 | A condição do veículo. |
| `MarcaModelo` | `string?` | `cMod` | J23 | O código de marca e modelo (tabela do RENAVAM). |
| `CorDenatran` | `string?` | `cCorDENATRAN` | J24 | O código da cor (tabela do DENATRAN). |
| `Lotacao` | `int?` | `lota` | J25 | A lotação: a quantidade máxima de passageiros sentados, com o motorista. |
| `Restricao` | `RestricaoVeiculo?` | `tpRest` | J26 | A restrição. |

## Volume

Classe selada · Namespace `OpenDFe.NFe`

Um volume transportado (`vol`).

### Construtores

| Construtor | Descrição |
|---|---|
| `Volume()` |  |

### Propriedades

| Propriedade | Tipo | Tag | MOC | Descrição |
|---|---|---|---|---|
| `Quantidade` | `long?` | `qVol` | X27 | A quantidade de volumes; opcional. |
| `Especie` | `string?` | `esp` | X28 | A espécie; opcional. |
| `Marca` | `string?` | `marca` | X29 | A marca; opcional. |
| `Numeracao` | `string?` | `nVol` | X30 | A numeração; opcional. |
| `PesoLiquido` | `decimal?` | `pesoL` | X31 | O peso líquido, em kg; opcional. |
| `PesoBruto` | `decimal?` | `pesoB` | X32 | O peso bruto, em kg; opcional. |
| `Lacres` | `IList<Lacre>` | `lacres` | X33 | Os lacres. |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
