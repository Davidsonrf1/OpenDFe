# Namespace OpenDFe.NFe: modelo (enumerações)

As enumerações do modelo da NF-e e da NFC-e. O valor de cada membro é o do XML. Pacote NuGet: `OpenDFe.NFe`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`AplicacaoAliquotaZeroCbs`](#aplicacaoaliquotazerocbs) | O tipo de aplicação da alíquota zero da CBS em área incentivada (ALC ou ZFM). O valor de cada membro é o do XML (`tpALCZFMCBS`). |
| [`BandeiraCartao`](#bandeiracartao) | A bandeira da operadora do cartão. O valor de cada membro é o do XML (`tBand`). |
| [`ClassificacaoCreditoPresumidoZfm`](#classificacaocreditopresumidozfm) | A classificação para o crédito presumido do IBS na Zona Franca de Manaus (art. 450, § 1º, da LC 214/2025). O valor de cada membro é o do XML (`tpCredPresIBSZFM`). |
| [`CondicaoVeiculo`](#condicaoveiculo) | A condição do veículo. O valor de cada membro é o do XML (`condVeic`). |
| [`CondicaoVin`](#condicaovin) | Se o VIN (chassi) do veículo é remarcado (`VIN`). |
| [`CsosnIcms`](#csosnicms) | O CSOSN, o código de situação da operação no Simples Nacional. O valor de cada membro é o do XML (`CSOSN`). |
| [`CstIcms`](#csticms) | O CST do ICMS. O valor de cada membro é o do XML (`CST`). |
| [`CstIpi`](#cstipi) | O CST do IPI. O valor de cada membro é o do XML (`CST`). |
| [`CstPisCofins`](#cstpiscofins) | O CST do PIS e da COFINS (os mesmos códigos). O valor de cada membro é o do XML (`CST`). |
| [`EnteGovernamental`](#entegovernamental) | O tipo do ente governamental da compra governamental (NT 2025.002). O valor de cada membro é o do XML (`tpEnteGov`). |
| [`ExigibilidadeIss`](#exigibilidadeiss) | A exigibilidade do ISS. O valor de cada membro é o do XML (`indISS`). |
| [`FinalidadeEmissao`](#finalidadeemissao) | A finalidade da emissão. O valor de cada membro é o do XML (`finNFe`). |
| [`FormaImportacao`](#formaimportacao) | A forma de importação quanto à intermediação. O valor de cada membro é o do XML (`tpIntermedio`). |
| [`FormatoImpressao`](#formatoimpressao) | O formato de impressão do DANFE. O valor de cada membro é o do XML (`tpImp`). |
| [`IndicadorBemMovelUsado`](#indicadorbemmovelusado) | O fornecimento de bem móvel usado. O valor de cada membro é o do XML (`indBemMovelUsado`). |
| [`IndicadorConsumidorFinal`](#indicadorconsumidorfinal) | Se a operação é com consumidor final. O valor de cada membro é o do XML (`indFinal`). |
| [`IndicadorDeducaoDesoneracao`](#indicadordeducaodesoneracao) | Se o ICMS desonerado deduz do valor do item e do total da nota. O valor de cada membro é o do XML (`indDeduzDeson`). |
| [`IndicadorDoacao`](#indicadordoacao) | A operação de doação. O valor de cada membro é o do XML (`indDoacao`). |
| [`IndicadorEscala`](#indicadorescala) | Se o produto é fabricado em escala relevante (`indEscala`). |
| [`IndicadorIEDestinatario`](#indicadoriedestinatario) | O indicador da inscrição estadual do destinatário. O valor de cada membro é o do XML (`indIEDest`). |
| [`IndicadorImportacao`](#indicadorimportacao) | Se o combustível é nacional ou importado. O valor de cada membro é o do XML (`indImport`). |
| [`IndicadorIncentivo`](#indicadorincentivo) | Se há incentivo fiscal no ISSQN. O valor de cada membro é o do XML (`indIncentivo`). |
| [`IndicadorIntermediador`](#indicadorintermediador) | Se a operação usa plataforma própria ou de terceiros. O valor de cada membro é o do XML (`indIntermed`). |
| [`IndicadorPagamento`](#indicadorpagamento) | À vista ou a prazo. O valor de cada membro é o do XML (`indPag`). |
| [`IndicadorTotal`](#indicadortotal) | Se o valor compõe o total da nota. O valor de cada membro é o do XML (`indTot, indSomaPISST, indSomaCOFINSST`). |
| [`LocalDestino`](#localdestino) | O local de destino da operação. O valor de cada membro é o do XML (`idDest`). |
| [`MeioPagamento`](#meiopagamento) | O meio de pagamento. O valor de cada membro é o do XML (`tPag`). |
| [`ModalidadeBaseCalculo`](#modalidadebasecalculo) | A modalidade de determinação da base de cálculo do ICMS. O valor de cada membro é o do XML (`modBC`). |
| [`ModalidadeBaseCalculoSt`](#modalidadebasecalculost) | A modalidade de determinação da base de cálculo do ICMS ST. O valor de cada membro é o do XML (`modBCST`). |
| [`ModalidadeFrete`](#modalidadefrete) | A modalidade do frete. O valor de cada membro é o do XML (`modFrete`). |
| [`ModeloCupomFiscal`](#modelocupomfiscal) | O modelo do cupom fiscal referenciado (`refECF/mod`). |
| [`ModeloNotaProdutor`](#modelonotaprodutor) | O modelo da nota de produtor referenciada (`refNFP/mod`, `01` ou `04`). |
| [`ModeloNotaReferenciada`](#modelonotareferenciada) | O modelo da nota fiscal modelo 1 ou 2 referenciada (`refNF/mod`, `01` ou `02`). |
| [`MotivoDesoneracao`](#motivodesoneracao) | O motivo da desoneração do ICMS ou do ICMS ST (`motDesICMS`, `motDesICMSST`). O valor de cada membro é o do XML. |
| [`MotivoReducaoAdRem`](#motivoreducaoadrem) | O motivo da redução da alíquota ad rem do ICMS monofásico. O valor de cada membro é o do XML (`motRedAdRem`). |
| [`OperacaoGovernamental`](#operacaogovernamental) | O tipo da operação com o ente governamental (NT 2025.002). O valor de cada membro é o do XML (`tpOperGov`). |
| [`OrigemMercadoria`](#origemmercadoria) | A origem da mercadoria. O valor de cada membro é o do XML (`orig`). |
| [`OrigemProcesso`](#origemprocesso) | A origem de um processo referenciado. O valor de cada membro é o do XML (`indProc`). |
| [`PresencaComprador`](#presencacomprador) | A presença do comprador no estabelecimento no momento da operação. O valor de cada membro é o do XML (`indPres`). |
| [`ProcessoEmissao`](#processoemissao) | O processo de emissão da nota. O valor de cada membro é o do XML (`procEmi`). |
| [`RegimeEspecialTributacao`](#regimeespecialtributacao) | O regime especial de tributação do ISSQN. O valor de cada membro é o do XML (`cRegTrib`). |
| [`RegimeTributario`](#regimetributario) | O código do regime tributário do emitente. O valor de cada membro é o do XML (`CRT`). |
| [`RestricaoVeiculo`](#restricaoveiculo) | A restrição do veículo. O valor de cada membro é o do XML (`tpRest`). |
| [`TipoArma`](#tipoarma) | O tipo da arma de fogo. O valor de cada membro é o do XML (`tpArma`). |
| [`TipoAtoConcessorio`](#tipoatoconcessorio) | O tipo do ato concessório de um processo referenciado (NT 2023.004). O valor de cada membro é o do XML (`tpAto`). |
| [`TipoGuiaTransito`](#tipoguiatransito) | O tipo da guia de trânsito de produtos agropecuários (NT 2024.003). O valor de cada membro é o do XML (`tpGuia`). |
| [`TipoIntegracao`](#tipointegracao) | Se o pagamento é integrado ao sistema de automação da empresa. O valor de cada membro é o do XML (`tpIntegra`). |
| [`TipoNota`](#tiponota) | Entrada ou saída. O valor de cada membro é o do XML (`tpNF`). |
| [`TipoNotaCredito`](#tiponotacredito) | O tipo da nota de crédito (NT 2025.002). O valor de cada membro é o do XML (`tpNFCredito`). |
| [`TipoNotaDebito`](#tiponotadebito) | O tipo da nota de débito (NT 2025.002). O valor de cada membro é o do XML (`tpNFDebito`). |
| [`TipoOperacaoVeiculo`](#tipooperacaoveiculo) | O tipo da operação com o veículo novo. O valor de cada membro é o do XML (`tpOp`). |
| [`ViaTransporteInternacional`](#viatransporteinternacional) | A via de transporte internacional da declaração de importação. O valor de cada membro é o do XML (`tpViaTransp`). |

## AplicacaoAliquotaZeroCbs

Enumeração · Namespace `OpenDFe.NFe`

O tipo de aplicação da alíquota zero da CBS em área incentivada (ALC ou ZFM). O valor de cada membro é o do XML (`tpALCZFMCBS`).

| Membro | Valor | Descrição |
|---|---|---|
| `AreaIncentivada` | 1 | Fornecedor e destinatário em área incentivada, sem operação industrial com processo aprovado na SUFRAMA para o item (1). |
| `ProcessoSuframa` | 2 | Fornecedor e destinatário em área incentivada, em operação industrial com processo aprovado na SUFRAMA para o item (2). |

## BandeiraCartao

Enumeração · Namespace `OpenDFe.NFe`

A bandeira da operadora do cartão. O valor de cada membro é o do XML (`tBand`).

A lista é a do MOC 7.0 (Anexo I). Desde a NT 2020.006 vale a tabela de códigos das operadoras publicada no Portal Nacional, que pode ter mais bandeiras: a leitura de um código que não está aqui devolve o número, e a escrita escreve o número que estiver.

| Membro | Valor | Descrição |
|---|---|---|
| `Visa` | 1 | Visa (1). |
| `Mastercard` | 2 | Mastercard (2). |
| `AmericanExpress` | 3 | American Express (3). |
| `Sorocred` | 4 | Sorocred (4). |
| `DinersClub` | 5 | Diners Club (5). |
| `Elo` | 6 | Elo (6). |
| `Hipercard` | 7 | Hipercard (7). |
| `Aura` | 8 | Aura (8). |
| `Cabal` | 9 | Cabal (9). |
| `Outros` | 99 | Outros (99). |

## ClassificacaoCreditoPresumidoZfm

Enumeração · Namespace `OpenDFe.NFe`

A classificação para o crédito presumido do IBS na Zona Franca de Manaus (art. 450, § 1º, da LC 214/2025). O valor de cada membro é o do XML (`tpCredPresIBSZFM`).

| Membro | Valor | Descrição |
|---|---|---|
| `Nenhum` | 0 | Sem crédito presumido (0). |
| `BensConsumoFinal` | 1 | Bens de consumo final (1). |
| `BensCapital` | 2 | Bens de capital (2). |
| `BensIntermediarios` | 3 | Bens intermediários (3). |
| `BensInformatica` | 4 | Bens de informática e outros definidos em legislação (4). |

## CondicaoVeiculo

Enumeração · Namespace `OpenDFe.NFe`

A condição do veículo. O valor de cada membro é o do XML (`condVeic`).

| Membro | Valor | Descrição |
|---|---|---|
| `Acabado` | 1 | Acabado (1). |
| `Inacabado` | 2 | Inacabado (2). |
| `SemiAcabado` | 3 | Semiacabado (3). |

## CondicaoVin

Enumeração · Namespace `OpenDFe.NFe`

Se o VIN (chassi) do veículo é remarcado (`VIN`).

O valor do XML não é número: o leiaute traduz o membro (1 e 2) para `R` e `N`.

| Membro | Valor | Descrição |
|---|---|---|
| `Remarcado` | 1 | Remarcado; `R` no XML. |
| `Normal` | 2 | Normal; `N` no XML. |

## CsosnIcms

Enumeração · Namespace `OpenDFe.NFe`

O CSOSN, o código de situação da operação no Simples Nacional. O valor de cada membro é o do XML (`CSOSN`).

Uma enumeração para todos os grupos do Simples Nacional: cada grupo aceita só uma parte dos códigos, e o schema aponta o código que o grupo não aceita.

| Membro | Valor | Descrição |
|---|---|---|
| `PermiteCredito` | 101 | Tributada com permissão de crédito (101). |
| `NaoPermiteCredito` | 102 | Tributada sem permissão de crédito (102). |
| `IsencaoFaixaReceita` | 103 | Isenção do ICMS para a faixa de receita bruta (103). |
| `PermiteCreditoSubstituicao` | 201 | Tributada com permissão de crédito e com cobrança do ICMS por substituição tributária (201). |
| `NaoPermiteCreditoSubstituicao` | 202 | Tributada sem permissão de crédito e com cobrança do ICMS por substituição tributária (202). |
| `IsencaoFaixaReceitaSubstituicao` | 203 | Isenção do ICMS para a faixa de receita bruta e com cobrança do ICMS por substituição tributária (203). |
| `Imune` | 300 | Imune (300). |
| `NaoTributada` | 400 | Não tributada pelo Simples Nacional (400). |
| `CobradoAnteriormente` | 500 | ICMS cobrado anteriormente por substituição tributária ou por antecipação (500). |
| `Outros` | 900 | Outros (900). |

## CstIcms

Enumeração · Namespace `OpenDFe.NFe`

O CST do ICMS. O valor de cada membro é o do XML (`CST`).

Uma enumeração para todos os grupos: cada grupo aceita só uma parte dos códigos, e o schema aponta o código que o grupo não aceita. O leiaute escreve o valor com os dois dígitos.

| Membro | Valor | Descrição |
|---|---|---|
| `Tributada` | 0 | Tributada integralmente (`00`). |
| `MonofasicaPropria` | 2 | Tributação monofásica própria sobre combustíveis (`02`). |
| `TributadaSubstituicao` | 10 | Tributada e com cobrança do ICMS por substituição tributária (`10`). |
| `MonofasicaPropriaRetencao` | 15 | Tributação monofásica própria e com responsabilidade pela retenção sobre combustíveis (`15`). |
| `ReducaoBase` | 20 | Com redução de base de cálculo (`20`). |
| `IsentaSubstituicao` | 30 | Isenta ou não tributada e com cobrança do ICMS por substituição tributária (`30`). |
| `Isenta` | 40 | Isenta (`40`). |
| `NaoTributada` | 41 | Não tributada (`41`). |
| `Suspensao` | 50 | Suspensão (`50`). |
| `Diferimento` | 51 | Diferimento (`51`). |
| `MonofasicaDiferida` | 53 | Tributação monofásica sobre combustíveis com recolhimento diferido (`53`). |
| `CobradoAnteriormente` | 60 | ICMS cobrado anteriormente por substituição tributária (`60`). |
| `MonofasicaCobradaAnteriormente` | 61 | Tributação monofásica sobre combustíveis cobrada anteriormente (`61`). |
| `ReducaoBaseSubstituicao` | 70 | Com redução de base de cálculo e cobrança do ICMS por substituição tributária (`70`). |
| `Outras` | 90 | Outras (`90`). |

## CstIpi

Enumeração · Namespace `OpenDFe.NFe`

O CST do IPI. O valor de cada membro é o do XML (`CST`).

O [`IpiTributado`](opendfe-nfe-modelo.md#ipitributado) e o [`IpiNaoTributado`](opendfe-nfe-modelo.md#ipinaotributado) aceitam cada um uma parte dos códigos, e o schema aponta o código que o grupo não aceita. O leiaute escreve o valor com os dois dígitos.

| Membro | Valor | Descrição |
|---|---|---|
| `EntradaRecuperacaoCredito` | 0 | Entrada com recuperação de crédito (`00`). |
| `EntradaAliquotaZero` | 1 | Entrada tributada com alíquota zero (`01`). |
| `EntradaIsenta` | 2 | Entrada isenta (`02`). |
| `EntradaNaoTributada` | 3 | Entrada não tributada (`03`). |
| `EntradaImune` | 4 | Entrada imune (`04`). |
| `EntradaSuspensao` | 5 | Entrada com suspensão (`05`). |
| `OutrasEntradas` | 49 | Outras entradas (`49`). |
| `SaidaTributada` | 50 | Saída tributada (`50`). |
| `SaidaAliquotaZero` | 51 | Saída tributada com alíquota zero (`51`). |
| `SaidaIsenta` | 52 | Saída isenta (`52`). |
| `SaidaNaoTributada` | 53 | Saída não tributada (`53`). |
| `SaidaImune` | 54 | Saída imune (`54`). |
| `SaidaSuspensao` | 55 | Saída com suspensão (`55`). |
| `OutrasSaidas` | 99 | Outras saídas (`99`). |

## CstPisCofins

Enumeração · Namespace `OpenDFe.NFe`

O CST do PIS e da COFINS (os mesmos códigos). O valor de cada membro é o do XML (`CST`).

Cada grupo do PIS e da COFINS aceita só uma parte dos códigos, e o schema aponta o código que o grupo não aceita. O leiaute escreve o valor com os dois dígitos.

| Membro | Valor | Descrição |
|---|---|---|
| `AliquotaBasica` | 1 | Operação tributável com alíquota básica (`01`). |
| `AliquotaDiferenciada` | 2 | Operação tributável com alíquota diferenciada (`02`). |
| `AliquotaUnidade` | 3 | Operação tributável com alíquota por unidade de medida de produto (`03`). |
| `MonofasicaRevendaAliquotaZero` | 4 | Operação tributável monofásica, revenda a alíquota zero (`04`). |
| `SubstituicaoTributaria` | 5 | Operação tributável por substituição tributária (`05`). |
| `AliquotaZero` | 6 | Operação tributável a alíquota zero (`06`). |
| `Isenta` | 7 | Operação isenta da contribuição (`07`). |
| `NaoIncidencia` | 8 | Operação sem incidência da contribuição (`08`). |
| `Suspensao` | 9 | Operação com suspensão da contribuição (`09`). |
| `OutrasSaidas` | 49 | Outras operações de saída (`49`). |
| `CreditoReceitaTributada` | 50 | Com direito a crédito, vinculada exclusivamente a receita tributada no mercado interno (`50`). |
| `CreditoReceitaNaoTributada` | 51 | Com direito a crédito, vinculada exclusivamente a receita não tributada no mercado interno (`51`). |
| `CreditoReceitaExportacao` | 52 | Com direito a crédito, vinculada exclusivamente a receita de exportação (`52`). |
| `CreditoReceitasMercadoInterno` | 53 | Com direito a crédito, vinculada a receitas tributadas e não tributadas no mercado interno (`53`). |
| `CreditoReceitasTributadaExportacao` | 54 | Com direito a crédito, vinculada a receitas tributadas no mercado interno e de exportação (`54`). |
| `CreditoReceitasNaoTributadaExportacao` | 55 | Com direito a crédito, vinculada a receitas não tributadas no mercado interno e de exportação (`55`). |
| `CreditoReceitasTodas` | 56 | Com direito a crédito, vinculada a receitas tributadas e não tributadas no mercado interno e de exportação (`56`). |
| `CreditoPresumidoReceitaTributada` | 60 | Crédito presumido, aquisição vinculada exclusivamente a receita tributada no mercado interno (`60`). |
| `CreditoPresumidoReceitaNaoTributada` | 61 | Crédito presumido, aquisição vinculada exclusivamente a receita não tributada no mercado interno (`61`). |
| `CreditoPresumidoReceitaExportacao` | 62 | Crédito presumido, aquisição vinculada exclusivamente a receita de exportação (`62`). |
| `CreditoPresumidoReceitasMercadoInterno` | 63 | Crédito presumido, aquisição vinculada a receitas tributadas e não tributadas no mercado interno (`63`). |
| `CreditoPresumidoReceitasTributadaExportacao` | 64 | Crédito presumido, aquisição vinculada a receitas tributadas no mercado interno e de exportação (`64`). |
| `CreditoPresumidoReceitasNaoTributadaExportacao` | 65 | Crédito presumido, aquisição vinculada a receitas não tributadas no mercado interno e de exportação (`65`). |
| `CreditoPresumidoReceitasTodas` | 66 | Crédito presumido, aquisição vinculada a receitas tributadas e não tributadas no mercado interno e de exportação (`66`). |
| `CreditoPresumidoOutras` | 67 | Crédito presumido, outras operações (`67`). |
| `AquisicaoCreditoVedado` | 70 | Aquisição sem direito a crédito (`70`). |
| `AquisicaoIsencao` | 71 | Aquisição com isenção (`71`). |
| `AquisicaoSuspensao` | 72 | Aquisição com suspensão (`72`). |
| `AquisicaoAliquotaZero` | 73 | Aquisição a alíquota zero (`73`). |
| `AquisicaoNaoIncidencia` | 74 | Aquisição sem incidência da contribuição (`74`). |
| `AquisicaoSubstituicao` | 75 | Aquisição por substituição tributária (`75`). |
| `OutrasEntradas` | 98 | Outras operações de entrada (`98`). |
| `Outras` | 99 | Outras operações (`99`). |

## EnteGovernamental

Enumeração · Namespace `OpenDFe.NFe`

O tipo do ente governamental da compra governamental (NT 2025.002). O valor de cada membro é o do XML (`tpEnteGov`).

| Membro | Valor | Descrição |
|---|---|---|
| `Uniao` | 1 | União (1). |
| `Estado` | 2 | Estado (2). |
| `DistritoFederal` | 3 | Distrito Federal (3). |
| `Municipio` | 4 | Município (4). |
| `ConsorcioPublico` | 5 | Consórcio público (5). |
| `ComiteGestorIbs` | 6 | Comitê Gestor do IBS (6). |

## ExigibilidadeIss

Enumeração · Namespace `OpenDFe.NFe`

A exigibilidade do ISS. O valor de cada membro é o do XML (`indISS`).

| Membro | Valor | Descrição |
|---|---|---|
| `Exigivel` | 1 | Exigível (1). |
| `NaoIncidencia` | 2 | Não incidência (2). |
| `Isencao` | 3 | Isenção (3). |
| `Exportacao` | 4 | Exportação (4). |
| `Imunidade` | 5 | Imunidade (5). |
| `SuspensaJudicial` | 6 | Exigibilidade suspensa por decisão judicial (6). |
| `SuspensaAdministrativa` | 7 | Exigibilidade suspensa por processo administrativo (7). |

## FinalidadeEmissao

Enumeração · Namespace `OpenDFe.NFe`

A finalidade da emissão. O valor de cada membro é o do XML (`finNFe`).

| Membro | Valor | Descrição |
|---|---|---|
| `Normal` | 1 | Nota normal (1). |
| `Complementar` | 2 | Nota complementar (2). |
| `Ajuste` | 3 | Nota de ajuste (3). |
| `Devolucao` | 4 | Devolução de mercadoria (4). |
| `NotaCredito` | 5 | Nota de crédito (5). |
| `NotaDebito` | 6 | Nota de débito (6). |

## FormaImportacao

Enumeração · Namespace `OpenDFe.NFe`

A forma de importação quanto à intermediação. O valor de cada membro é o do XML (`tpIntermedio`).

| Membro | Valor | Descrição |
|---|---|---|
| `ContaPropria` | 1 | Por conta própria (1). |
| `ContaOrdem` | 2 | Por conta e ordem (2). |
| `Encomenda` | 3 | Por encomenda (3). |

## FormatoImpressao

Enumeração · Namespace `OpenDFe.NFe`

O formato de impressão do DANFE. O valor de cada membro é o do XML (`tpImp`).

| Membro | Valor | Descrição |
|---|---|---|
| `Nenhum` | 0 | Sem geração de DANFE (0). |
| `DanfeRetrato` | 1 | DANFE normal, retrato (1). |
| `DanfePaisagem` | 2 | DANFE normal, paisagem (2). |
| `DanfeSimplificado` | 3 | DANFE simplificado (3). |
| `DanfeNFCe` | 4 | DANFE NFC-e (4). |
| `DanfeNFCeMensagem` | 5 | DANFE NFC-e em mensagem eletrônica (5). |
| `DanfeSimplificadoTipo2` | 6 | DANFE simplificado, tipo 2 (6). |

## IndicadorBemMovelUsado

Enumeração · Namespace `OpenDFe.NFe`

O fornecimento de bem móvel usado. O valor de cada membro é o do XML (`indBemMovelUsado`).

O XSD aceita só o 1; sem o indicador, a propriedade fica nula. Enumeração, e não `Boolean`, para ganhar valor em versão menor.

| Membro | Valor | Descrição |
|---|---|---|
| `Usado` | 1 | Bem móvel usado (1). |

## IndicadorConsumidorFinal

Enumeração · Namespace `OpenDFe.NFe`

Se a operação é com consumidor final. O valor de cada membro é o do XML (`indFinal`).

| Membro | Valor | Descrição |
|---|---|---|
| `Normal` | 0 | Normal (0). |
| `ConsumidorFinal` | 1 | Consumidor final (1). |

## IndicadorDeducaoDesoneracao

Enumeração · Namespace `OpenDFe.NFe`

Se o ICMS desonerado deduz do valor do item e do total da nota. O valor de cada membro é o do XML (`indDeduzDeson`).

| Membro | Valor | Descrição |
|---|---|---|
| `NaoDeduz` | 0 | O ICMS desonerado não deduz do valor do item (`vProd`) nem do total da nota. |
| `Deduz` | 1 | O ICMS desonerado deduz do valor do item (`vProd`) e do total da nota. |

## IndicadorDoacao

Enumeração · Namespace `OpenDFe.NFe`

A operação de doação. O valor de cada membro é o do XML (`indDoacao`).

O XSD aceita só o 1; sem o indicador, a propriedade fica nula. Enumeração, e não `Boolean`, para ganhar valor em versão menor.

| Membro | Valor | Descrição |
|---|---|---|
| `Doacao` | 1 | Doação (1). |

## IndicadorEscala

Enumeração · Namespace `OpenDFe.NFe`

Se o produto é fabricado em escala relevante (`indEscala`).

O valor do XML não é número: o leiaute traduz o membro (1 e 2) para `S` e `N`.

| Membro | Valor | Descrição |
|---|---|---|
| `Relevante` | 1 | Produzido em escala relevante; `S` no XML. |
| `NaoRelevante` | 2 | Produzido em escala não relevante; `N` no XML. |

## IndicadorIEDestinatario

Enumeração · Namespace `OpenDFe.NFe`

O indicador da inscrição estadual do destinatário. O valor de cada membro é o do XML (`indIEDest`).

| Membro | Valor | Descrição |
|---|---|---|
| `Contribuinte` | 1 | Contribuinte do ICMS, com a inscrição estadual (1). |
| `ContribuinteIsento` | 2 | Contribuinte isento de inscrição estadual (2). |
| `NaoContribuinte` | 9 | Não contribuinte, que pode ou não ter inscrição estadual (9). |

## IndicadorImportacao

Enumeração · Namespace `OpenDFe.NFe`

Se o combustível é nacional ou importado. O valor de cada membro é o do XML (`indImport`).

| Membro | Valor | Descrição |
|---|---|---|
| `Nacional` | 0 | Nacional (0). |
| `Importado` | 1 | Importado (1). |

## IndicadorIncentivo

Enumeração · Namespace `OpenDFe.NFe`

Se há incentivo fiscal no ISSQN. O valor de cada membro é o do XML (`indIncentivo`).

| Membro | Valor | Descrição |
|---|---|---|
| `Sim` | 1 | Sim (1). |
| `Nao` | 2 | Não (2). |

## IndicadorIntermediador

Enumeração · Namespace `OpenDFe.NFe`

Se a operação usa plataforma própria ou de terceiros. O valor de cada membro é o do XML (`indIntermed`).

| Membro | Valor | Descrição |
|---|---|---|
| `PlataformaPropria` | 0 | Operação sem intermediador (no site ou plataforma própria) (0). |
| `PlataformaTerceiros` | 1 | Operação em site ou plataforma de terceiros (intermediador) (1). |

## IndicadorPagamento

Enumeração · Namespace `OpenDFe.NFe`

À vista ou a prazo. O valor de cada membro é o do XML (`indPag`).

| Membro | Valor | Descrição |
|---|---|---|
| `Vista` | 0 | Pagamento à vista (0). |
| `Prazo` | 1 | Pagamento a prazo (1). |

## IndicadorTotal

Enumeração · Namespace `OpenDFe.NFe`

Se o valor compõe o total da nota. O valor de cada membro é o do XML (`indTot, indSomaPISST, indSomaCOFINSST`).

| Membro | Valor | Descrição |
|---|---|---|
| `NaoCompoe` | 0 | Não compõe o total (0). |
| `Compoe` | 1 | Compõe o total (1). |

## LocalDestino

Enumeração · Namespace `OpenDFe.NFe`

O local de destino da operação. O valor de cada membro é o do XML (`idDest`).

| Membro | Valor | Descrição |
|---|---|---|
| `Interna` | 1 | Operação interna (1). |
| `Interestadual` | 2 | Operação interestadual (2). |
| `Exterior` | 3 | Operação com o exterior (3). |

## MeioPagamento

Enumeração · Namespace `OpenDFe.NFe`

O meio de pagamento. O valor de cada membro é o do XML (`tPag`).

A lista é a da Tabela de Meios de Pagamento do Portal Nacional da NF-e (IT 2024.002 v1.11, publicada em 04/03/2026), que muda por informe técnico: a leitura de um código que não está aqui devolve o número (`(MeioPagamento)25`), e a escrita escreve o número que estiver.

| Membro | Valor | Descrição |
|---|---|---|
| `Dinheiro` | 1 | Dinheiro (1). |
| `Cheque` | 2 | Cheque (2). |
| `CartaoCredito` | 3 | Cartão de crédito (3). |
| `CartaoDebito` | 4 | Cartão de débito (4). |
| `CartaoLoja` | 5 | Cartão da loja (private label), crediário digital e outros crediários (5). |
| `ValeAlimentacao` | 10 | Vale alimentação (10). |
| `ValeRefeicao` | 11 | Vale refeição (11). |
| `ValePresente` | 12 | Vale presente (12). |
| `ValeCombustivel` | 13 | Vale combustível (13). |
| `DuplicataMercantil` | 14 | Duplicata mercantil (14). |
| `BoletoBancario` | 15 | Boleto bancário (15). |
| `DepositoBancario` | 16 | Depósito bancário (16). |
| `PixDinamico` | 17 | Pagamento instantâneo (PIX) com QR Code ou URL dinâmicos (17). |
| `Ted` | 18 | TED (transferência eletrônica disponível), entre contas de bancos diferentes (18). |
| `ProgramaFidelidade` | 19 | Programa de fidelidade, cashback, crédito virtual (19). |
| `PixEstatico` | 20 | Pagamento instantâneo (PIX) com QR Code estático, chave PIX ou agência e conta (20). |
| `CreditoLoja` | 21 | Crédito em loja (valor pago antes, devolução de mercadoria) (21). |
| `PagamentoEletronicoNaoInformado` | 22 | Pagamento eletrônico não informado, por falha de hardware do sistema emissor (22). |
| `PixAutomatico` | 23 | Pagamento instantâneo (PIX) automático, recorrente (23). |
| `TefBookTransfer` | 24 | TEF "book transfer", entre contas do mesmo banco (24). |
| `Nenhum` | 90 | Sem pagamento (90). |
| `PagamentoPosterior` | 91 | Pagamento posterior à emissão (91). |
| `Outros` | 99 | Outros (99). |

## ModalidadeBaseCalculo

Enumeração · Namespace `OpenDFe.NFe`

A modalidade de determinação da base de cálculo do ICMS. O valor de cada membro é o do XML (`modBC`).

| Membro | Valor | Descrição |
|---|---|---|
| `MargemValorAgregado` | 0 | Margem de valor agregado (%) (0). |
| `Pauta` | 1 | Pauta (valor) (1). |
| `PrecoTabeladoMaximo` | 2 | Preço tabelado máximo (valor) (2). |
| `ValorOperacao` | 3 | Valor da operação (3). |

## ModalidadeBaseCalculoSt

Enumeração · Namespace `OpenDFe.NFe`

A modalidade de determinação da base de cálculo do ICMS ST. O valor de cada membro é o do XML (`modBCST`).

| Membro | Valor | Descrição |
|---|---|---|
| `PrecoTabelado` | 0 | Preço tabelado ou máximo sugerido (0). |
| `ListaNegativa` | 1 | Lista negativa (valor) (1). |
| `ListaPositiva` | 2 | Lista positiva (valor) (2). |
| `ListaNeutra` | 3 | Lista neutra (valor) (3). |
| `MargemValorAgregado` | 4 | Margem de valor agregado (%) (4). |
| `Pauta` | 5 | Pauta (valor) (5). |
| `ValorOperacao` | 6 | Valor da operação (6). |

## ModalidadeFrete

Enumeração · Namespace `OpenDFe.NFe`

A modalidade do frete. O valor de cada membro é o do XML (`modFrete`).

| Membro | Valor | Descrição |
|---|---|---|
| `Remetente` | 0 | Contratação do frete por conta do remetente (CIF) (0). |
| `Destinatario` | 1 | Contratação do frete por conta do destinatário (FOB) (1). |
| `Terceiros` | 2 | Contratação do frete por conta de terceiros (2). |
| `ProprioRemetente` | 3 | Transporte próprio por conta do remetente (3). |
| `ProprioDestinatario` | 4 | Transporte próprio por conta do destinatário (4). |
| `NenhumTransporte` | 9 | Sem ocorrência de transporte (9). |

## ModeloCupomFiscal

Enumeração · Namespace `OpenDFe.NFe`

O modelo do cupom fiscal referenciado (`refECF/mod`).

O único domínio com valor do XML que não é número: o leiaute traduz o membro (1 a 3) para `2B`, `2C` e `2D`.

| Membro | Valor | Descrição |
|---|---|---|
| `MaquinaRegistradora` | 1 | Cupom fiscal emitido por máquina registradora, não ECF; `2B` no XML. |
| `Pdv` | 2 | Cupom fiscal PDV; `2C` no XML. |
| `Ecf` | 3 | Cupom fiscal emitido por ECF; `2D` no XML. |

## ModeloNotaProdutor

Enumeração · Namespace `OpenDFe.NFe`

O modelo da nota de produtor referenciada (`refNFP/mod`, `01` ou `04`).

| Membro | Valor | Descrição |
|---|---|---|
| `NotaAvulsa` | 1 | Nota fiscal avulsa (modelo 01) (1). |
| `NotaProdutor` | 4 | Nota fiscal de produtor (modelo 04) (4). |

## ModeloNotaReferenciada

Enumeração · Namespace `OpenDFe.NFe`

O modelo da nota fiscal modelo 1 ou 2 referenciada (`refNF/mod`, `01` ou `02`).

| Membro | Valor | Descrição |
|---|---|---|
| `Modelo1` | 1 | Modelo 01 (1). |
| `Modelo2` | 2 | Modelo 02 (2). |

## MotivoDesoneracao

Enumeração · Namespace `OpenDFe.NFe`

O motivo da desoneração do ICMS ou do ICMS ST (`motDesICMS`, `motDesICMSST`). O valor de cada membro é o do XML.

Uma enumeração para todos os grupos, com a união dos valores: cada grupo aceita só uma parte, e o schema aponta o valor que o grupo não aceita.

| Membro | Valor | Descrição |
|---|---|---|
| `Taxi` | 1 | Táxi (1). |
| `Agropecuaria` | 3 | Uso na agropecuária (no `ICMS40`, produtor agropecuário). |
| `FrotistaLocadora` | 4 | Frotista ou locadora (4). |
| `DiplomaticoConsular` | 5 | Diplomático ou consular (5). |
| `UtilitariosMotocicletasAmazonia` | 6 | Utilitários e motocicletas da Amazônia Ocidental e das áreas de livre comércio (6). |
| `Suframa` | 7 | SUFRAMA (7). |
| `OrgaoPublico` | 8 | Venda a órgão público (8). |
| `Outros` | 9 | Outros (9). |
| `DeficienteCondutor` | 10 | Deficiente condutor (10). |
| `DeficienteNaoCondutor` | 11 | Deficiente não condutor (11). |
| `FomentoAgropecuario` | 12 | Órgão de fomento e desenvolvimento agropecuário (12). |
| `OlimpiadasRio2016` | 16 | Olimpíadas Rio 2016 (16). |
| `SolicitadoFisco` | 90 | Solicitado pelo fisco (90). |

## MotivoReducaoAdRem

Enumeração · Namespace `OpenDFe.NFe`

O motivo da redução da alíquota ad rem do ICMS monofásico. O valor de cada membro é o do XML (`motRedAdRem`).

| Membro | Valor | Descrição |
|---|---|---|
| `TransporteColetivo` | 1 | Transporte coletivo de passageiros (1). |
| `Outros` | 9 | Outros (9). |

## OperacaoGovernamental

Enumeração · Namespace `OpenDFe.NFe`

O tipo da operação com o ente governamental (NT 2025.002). O valor de cada membro é o do XML (`tpOperGov`).

| Membro | Valor | Descrição |
|---|---|---|
| `FornecimentoPagamentoPosterior` | 1 | Fornecimento com pagamento posterior (1). |
| `RecebimentoFornecimentoRealizado` | 2 | Recebimento do pagamento com fornecimento já realizado (2). |
| `FornecimentoPagamentoRealizado` | 3 | Fornecimento com pagamento já realizado (3). |
| `RecebimentoFornecimentoPosterior` | 4 | Recebimento do pagamento com fornecimento posterior (4). |

## OrigemMercadoria

Enumeração · Namespace `OpenDFe.NFe`

A origem da mercadoria. O valor de cada membro é o do XML (`orig`).

| Membro | Valor | Descrição |
|---|---|---|
| `Nacional` | 0 | Nacional, exceto as dos códigos 3, 4, 5 e 8 (0). |
| `EstrangeiraImportacaoDireta` | 1 | Estrangeira, importação direta, exceto a do código 6 (1). |
| `EstrangeiraMercadoInterno` | 2 | Estrangeira, adquirida no mercado interno, exceto a do código 7 (2). |
| `NacionalConteudoImportacaoMedio` | 3 | Nacional, com conteúdo de importação superior a 40% e inferior ou igual a 70% (3). |
| `NacionalProcessoProdutivoBasico` | 4 | Nacional, produzida conforme os processos produtivos básicos (4). |
| `NacionalConteudoImportacaoBaixo` | 5 | Nacional, com conteúdo de importação inferior ou igual a 40% (5). |
| `EstrangeiraImportacaoDiretaCamex` | 6 | Estrangeira, importação direta, sem similar nacional, constante em lista da CAMEX (6). |
| `EstrangeiraMercadoInternoCamex` | 7 | Estrangeira, adquirida no mercado interno, sem similar nacional, constante em lista da CAMEX (7). |
| `NacionalConteudoImportacaoAlto` | 8 | Nacional, com conteúdo de importação superior a 70% (8). |

## OrigemProcesso

Enumeração · Namespace `OpenDFe.NFe`

A origem de um processo referenciado. O valor de cada membro é o do XML (`indProc`).

| Membro | Valor | Descrição |
|---|---|---|
| `Sefaz` | 0 | SEFAZ (0). |
| `JusticaFederal` | 1 | Justiça Federal (1). |
| `JusticaEstadual` | 2 | Justiça Estadual (2). |
| `SecexRfb` | 3 | SECEX/RFB (3). |
| `Confaz` | 4 | CONFAZ (4). |
| `Outros` | 9 | Outros (9). |

## PresencaComprador

Enumeração · Namespace `OpenDFe.NFe`

A presença do comprador no estabelecimento no momento da operação. O valor de cada membro é o do XML (`indPres`).

| Membro | Valor | Descrição |
|---|---|---|
| `NaoAplicavel` | 0 | Não se aplica (nota complementar ou de ajuste, por exemplo) (0). |
| `Presencial` | 1 | Operação presencial (1). |
| `NaoPresencialInternet` | 2 | Operação não presencial, pela internet (2). |
| `NaoPresencialTeleatendimento` | 3 | Operação não presencial, teleatendimento (3). |
| `NaoPresencialEntrega` | 4 | NFC-e em operação com entrega a domicílio (4). |
| `PresencialExterno` | 5 | Operação presencial, fora do estabelecimento (5). |
| `NaoPresencialOutros` | 9 | Operação não presencial, outros (9). |

## ProcessoEmissao

Enumeração · Namespace `OpenDFe.NFe`

O processo de emissão da nota. O valor de cada membro é o do XML (`procEmi`).

| Membro | Valor | Descrição |
|---|---|---|
| `AplicativoContribuinte` | 0 | Emissão com aplicativo do contribuinte (0). |
| `AvulsaFisco` | 1 | Emissão avulsa pelo fisco (1). |
| `AvulsaContribuinteSite` | 2 | Emissão avulsa pelo contribuinte, com o certificado digital, pelo site do fisco (2). |
| `AplicativoFisco` | 3 | Emissão pelo contribuinte com aplicativo fornecido pelo fisco (3). |
| `ProvedorAssinatura` | 4 | Emissão por provedor de assinatura e autorização (PAA) (4). |

## RegimeEspecialTributacao

Enumeração · Namespace `OpenDFe.NFe`

O regime especial de tributação do ISSQN. O valor de cada membro é o do XML (`cRegTrib`).

| Membro | Valor | Descrição |
|---|---|---|
| `MicroempresaMunicipal` | 1 | Microempresa municipal (1). |
| `Estimativa` | 2 | Estimativa (2). |
| `SociedadeProfissionais` | 3 | Sociedade de profissionais (3). |
| `Cooperativa` | 4 | Cooperativa (4). |
| `Mei` | 5 | Microempreendedor individual (MEI) (5). |
| `MicroempresaPequenoPorte` | 6 | Microempresa ou empresa de pequeno porte (ME ou EPP) (6). |

## RegimeTributario

Enumeração · Namespace `OpenDFe.NFe`

O código do regime tributário do emitente. O valor de cada membro é o do XML (`CRT`).

| Membro | Valor | Descrição |
|---|---|---|
| `SimplesNacional` | 1 | Simples Nacional (1). |
| `SimplesExcessoSublimite` | 2 | Simples Nacional, excesso de sublimite da receita bruta (2). |
| `Normal` | 3 | Regime normal (3). |
| `Mei` | 4 | Simples Nacional, microempreendedor individual (MEI) (4). |

## RestricaoVeiculo

Enumeração · Namespace `OpenDFe.NFe`

A restrição do veículo. O valor de cada membro é o do XML (`tpRest`).

| Membro | Valor | Descrição |
|---|---|---|
| `Nenhuma` | 0 | Não há (0). |
| `AlienacaoFiduciaria` | 1 | Alienação fiduciária (1). |
| `ArrendamentoMercantil` | 2 | Arrendamento mercantil (2). |
| `ReservaDominio` | 3 | Reserva de domínio (3). |
| `PenhorVeiculos` | 4 | Penhor de veículos (4). |
| `Outras` | 9 | Outras (9). |

## TipoArma

Enumeração · Namespace `OpenDFe.NFe`

O tipo da arma de fogo. O valor de cada membro é o do XML (`tpArma`).

| Membro | Valor | Descrição |
|---|---|---|
| `UsoPermitido` | 0 | Uso permitido (0). |
| `UsoRestrito` | 1 | Uso restrito (1). |

## TipoAtoConcessorio

Enumeração · Namespace `OpenDFe.NFe`

O tipo do ato concessório de um processo referenciado (NT 2023.004). O valor de cada membro é o do XML (`tpAto`).

| Membro | Valor | Descrição |
|---|---|---|
| `TermoAcordo` | 8 | Termo de acordo (8). |
| `RegimeEspecial` | 10 | Regime especial (10). |
| `AutorizacaoEspecifica` | 12 | Autorização específica (12). |
| `AjusteSinief` | 14 | Ajuste SINIEF (14). |
| `ConvenioIcms` | 15 | Convênio ICMS (15). |

## TipoGuiaTransito

Enumeração · Namespace `OpenDFe.NFe`

O tipo da guia de trânsito de produtos agropecuários (NT 2024.003). O valor de cada membro é o do XML (`tpGuia`).

| Membro | Valor | Descrição |
|---|---|---|
| `Gta` | 1 | GTA, guia de trânsito animal (1). |
| `Tta` | 2 | TTA, termo de trânsito animal (2). |
| `Dta` | 3 | DTA, documento de transferência animal (3). |
| `Atv` | 4 | ATV, autorização de trânsito vegetal (4). |
| `Ptv` | 5 | PTV, permissão de trânsito vegetal (5). |
| `Gtv` | 6 | GTV, guia de trânsito vegetal (6). |
| `GuiaFlorestal` | 7 | Guia florestal (DOF, SisFlora PA e MT, SIAM MG) (7). |

## TipoIntegracao

Enumeração · Namespace `OpenDFe.NFe`

Se o pagamento é integrado ao sistema de automação da empresa. O valor de cada membro é o do XML (`tpIntegra`).

| Membro | Valor | Descrição |
|---|---|---|
| `Integrado` | 1 | Pagamento integrado (TEF, comércio eletrônico, POS integrado) (1). |
| `NaoIntegrado` | 2 | Pagamento não integrado (POS simples) (2). |

## TipoNota

Enumeração · Namespace `OpenDFe.NFe`

Entrada ou saída. O valor de cada membro é o do XML (`tpNF`).

| Membro | Valor | Descrição |
|---|---|---|
| `Entrada` | 0 | Entrada (0). |
| `Saida` | 1 | Saída (1). |

## TipoNotaCredito

Enumeração · Namespace `OpenDFe.NFe`

O tipo da nota de crédito (NT 2025.002). O valor de cada membro é o do XML (`tpNFCredito`).

| Membro | Valor | Descrição |
|---|---|---|
| `MultaJuros` | 1 | Multa e juros (1). |
| `CreditoPresumidoZfm` | 2 | Apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM (2). |
| `RetornoRecusaEntrega` | 3 | Retorno por recusa na entrega ou por não localização do destinatário (3). |
| `ReducaoValores` | 4 | Redução de valores (4). |
| `TransferenciaCreditoSucessao` | 5 | Transferência de crédito na sucessão (5). |
| `RetornoRecusaParcial` | 6 | Retorno de mercadoria por recusa parcial (6). |

## TipoNotaDebito

Enumeração · Namespace `OpenDFe.NFe`

O tipo da nota de débito (NT 2025.002). O valor de cada membro é o do XML (`tpNFDebito`).

| Membro | Valor | Descrição |
|---|---|---|
| `TransferenciaCreditoCooperativa` | 1 | Transferência de créditos para cooperativas (1). |
| `AnulacaoCreditoSaidaImune` | 2 | Anulação de crédito por saídas imunes ou isentas (2). |
| `DebitoNaoProcessado` | 3 | Débitos de notas fiscais não processadas na apuração (3). |
| `MultaJuros` | 4 | Multa e juros (4). |
| `TransferenciaCreditoSucessao` | 5 | Transferência de crédito na sucessão (5). |
| `PagamentoAntecipado` | 6 | Pagamento antecipado (6). |
| `PerdaEstoque` | 7 | Perda em estoque (7). |
| `DesenquadramentoSimples` | 8 | Desenquadramento do Simples Nacional (8). |

## TipoOperacaoVeiculo

Enumeração · Namespace `OpenDFe.NFe`

O tipo da operação com o veículo novo. O valor de cada membro é o do XML (`tpOp`).

| Membro | Valor | Descrição |
|---|---|---|
| `Outros` | 0 | Outros (0). |
| `VendaConcessionaria` | 1 | Venda concessionária (1). |
| `FaturamentoDireto` | 2 | Faturamento direto para consumidor final (2). |
| `VendaDireta` | 3 | Venda direta para grandes consumidores (frotista, governo e afins) (3). |

## ViaTransporteInternacional

Enumeração · Namespace `OpenDFe.NFe`

A via de transporte internacional da declaração de importação. O valor de cada membro é o do XML (`tpViaTransp`).

| Membro | Valor | Descrição |
|---|---|---|
| `Maritima` | 1 | Marítima (1). |
| `Fluvial` | 2 | Fluvial (2). |
| `Lacustre` | 3 | Lacustre (3). |
| `Aerea` | 4 | Aérea (4). |
| `Postal` | 5 | Postal (5). |
| `Ferroviaria` | 6 | Ferroviária (6). |
| `Rodoviaria` | 7 | Rodoviária (7). |
| `Conduto` | 8 | Conduto ou rede de transmissão (8). |
| `MeiosProprios` | 9 | Meios próprios (9). |
| `Ficta` | 10 | Entrada ou saída ficta (10). |
| `Courier` | 11 | Courier (11). |
| `EntregaMaos` | 12 | Em mãos (12). |
| `Reboque` | 13 | Por reboque (13). |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
