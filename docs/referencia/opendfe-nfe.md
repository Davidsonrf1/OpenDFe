# Namespace OpenDFe.NFe: leiaute e operações

O leiaute 4.00 da NF-e e da NFC-e: as operações (parâmetros e retornos), a leitura, a NFC-e, os eventos, a distribuição, o cadastro e a impressão. Pacote NuGet: `OpenDFe.NFe`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`Autorizacao`](#autorizacao) | A autorização de 1 a 50 notas (`NFeAutorizacao4`, docs/CONTRATOS.md 14): uma nota vai na resposta síncrona (`indSinc=1`); de 2 a 50, no lote com recibo, que se consulta com [`ConsultaRecibo`](#consultarecibo). A quantidade decide, porque desde a NT 2025.001 o lote de uma nota é rejeitado (452) e a síncrona com mais de uma também (764). |
| [`Cancelamento`](#cancelamento) | O cancelamento (110111, MOC 7.0, Visão Geral, 5.9): o autor é o emitente da chave. O prazo (501), o protocolo (222) e a situação da nota ficam para a SEFAZ. |
| [`CancelamentoSubstituicao`](#cancelamentosubstituicao) | O cancelamento por substituição (110112, MOC 7.0, Visão Geral, 5.9) de uma NFC-e: cancela a NFC-e e aponta a que a substitui. O autor é o emitente da chave. A janela de 168 horas (501), a substituta que não confere (911 a 919), a nota fora da emissão normal (920, 921) e a situação da nota ficam para a SEFAZ. |
| [`CartaCorrecao`](#cartacorrecao) | A carta de correção (110110, MOC 7.0, Visão Geral, 5.10) de uma NF-e; a NFC-e não tem. O texto da condição de uso é fixo (o schema o enumera). O autor é o emitente da chave. Os prazos, a sequência acima da registrada e o que a carta pode corrigir ficam para a SEFAZ (o 580 de nota cancelada, o 577 de data antes da emissão). |
| [`CienciaOperacao`](#cienciaoperacao) | A ciência da operação (210210): o destinatário declara que soube da nota, e isso libera o download da NF-e completa na distribuição DF-e (docs/CONTRATOS.md 23). A sequência é sempre 1. |
| [`CodigoSeguranca`](#codigoseguranca) | O CSC (Código de Segurança do Contribuinte) que a SEFAZ da UF deu à empresa (Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, 4.6). O código é segredo: o [`ToString`](#codigoseguranca) mostra só o identificador. |
| [`ConfiguracaoNFCe`](#configuracaonfce) | A parte da configuração que é da NFC-e (PROJETO.md 3.2 e 9.1, docs/CONTRATOS.md 16): a versão do QR Code, o CSC e as URLs de consulta. Opcional: sem ela, o QR Code sai na versão 3 com as URLs da tabela do leiaute ([`UrlsConsulta`](#leiautenfe400)). Vale também para a NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6, NT 2026.002), só pelas URLs: o QR Code dela é sempre a versão 3. Imutável. |
| [`ConfirmacaoOperacao`](#confirmacaooperacao) | A confirmação da operação (210200). A sequência é 1, ou 2 na retificação (o Ajuste SINIEF 43/23 permite uma); depois dela, a nota não pode mais ser cancelada (221). |
| [`ConsultaCadastro`](#consultacadastro) | A consulta ao cadastro de contribuintes da UF (`CadConsultaCadastro4`, MOC 7.0, Visão Geral, 5.6, docs/CONTRATOS.md 24): o `ConsCad` 2.00 por CNPJ, por CPF ou por inscrição estadual e o `retConsCad` com os contribuintes achados ([`RetornoCadastro`](#retornocadastro)). Só na NF-e. |
| [`ConsultaProtocolo`](#consultaprotocolo) | A consulta da situação de uma nota pela chave (`NFeConsultaProtocolo4`, docs/CONTRATOS.md 15): o `consSitNFe` e o `retConsSitNFe`, com o protocolo e os eventos da nota. |
| [`ConsultaRecibo`](#consultarecibo) | A consulta do recibo do lote (`NFeRetAutorizacao4`, docs/CONTRATOS.md 14): os protocolos das notas do lote que a [`Autorizacao`](#autorizacao) devolveu com o recibo (103). |
| [`ContribuinteCadastro`](#contribuintecadastro) | Um contribuinte do cadastro da UF (o `infCad`, grupos GR07 a GR29 da Tabela 5-22). Imutável. Tem dados do contribuinte: guardar com cuidado, nunca em log. |
| [`CredenciamentoDFe`](#credenciamentodfe) | O credenciamento do contribuinte para emitir NF-e ou CT-e na SEFAZ consultada (`indCredNFe` e `indCredCTe`, MOC 7.0, Tabela 5-22, GR11 e GR12). Significa só que o contribuinte é credenciado nessa SEFAZ. Aberta: um valor que o MOC não conhece entra como o número, com aviso `NFE-VALOR`. |
| [`DesconhecimentoOperacao`](#desconhecimentooperacao) | O desconhecimento da operação (210220). A sequência é 1, ou 2 na retificação. |
| [`DistribuicaoDFe`](#distribuicaodfe) | A consulta à distribuição de DF-e do Ambiente Nacional (`NFeDistribuicaoDFe`, NT 2014.002, docs/CONTRATOS.md 23): o `distDFeInt` por último NSU, por NSU ou por chave, e o `retDistDFeInt` com os documentos já descompactados ([`RetornoDistribuicao`](#retornodistribuicao)). Só na NF-e: a NFC-e não é distribuída. Sempre ao AN. |
| [`DocumentoDistribuido`](#documentodistribuido) | Um documento da distribuição DF-e (`docZip`, NT 2014.002, 3.2) já descompactado: o XML como veio e, nos resumos, os campos nos tipos do modelo. Imutável. |
| [`Duplicidade`](#duplicidade) | A nota já registrada na SEFAZ que causou a rejeição por duplicidade (204 e 539, PROJETO.md 8.6, docs/CONTRATOS.md 15): no 204, a mesma nota (recuperar o `nfeProc` com a [`RecuperacaoAutorizacao`](#recuperacaoautorizacao)); no 539, outra chave com a mesma numeração (consultar a outra com a [`ConsultaProtocolo`](#consultaprotocolo)). Imutável. |
| [`EnderecoCadastro`](#enderecocadastro) | O endereço de um contribuinte do cadastro (o `ender`, `TEndereco`). Imutável; todos os campos são opcionais. |
| [`EnvioEvento`](#envioevento) | O envio de 1 a 20 eventos (`NFeRecepcaoEvento4`, docs/CONTRATOS.md 18): o `envEvento`, com cada evento assinado à parte, e o `retEnvEvento`. Na NF-e e na NFC-e. |
| [`Evento`](#evento) | Um evento de um documento (docs/CONTRATOS.md 18): o que o [`EnvioEvento`](#envioevento) leva. A base é fechada: só o leiaute deriva (construtor `private protected` e membros internos), para o cancelamento, a substituição, a carta de correção e a manifestação entrarem sem mudar o envio nem a operação. |
| [`Inutilizacao`](#inutilizacao) | A inutilização de uma faixa de numeração (`NFeInutilizacao4`, MOC 7.0, Visão Geral, 5.3; docs/CONTRATOS.md 18): o `inutNFe` assinado. Vai sempre ao autorizador normal da UF do núcleo, com o tipo de emissão normal (a SVC não oferece o serviço). O modelo (55 ou 65) e a UF são os do núcleo. |
| [`LeiauteNFe400`](#leiautenfe400) | O leiaute 4.00 da NF-e (modelo 55) e da NFC-e (modelo 65), os dois documentos no mesmo leiaute e no mesmo modelo de dados (PROJETO.md 3.1): o que se passa a [`Criar`](opendfe.md#dfe). |
| [`Manifestacao`](#manifestacao) | Um evento de manifestação do destinatário (NT 2020.001 v1.60, MOC 7.0, Visão Geral, 5.11, docs/CONTRATOS.md 24): a ciência ([`CienciaOperacao`](#cienciaoperacao)), a confirmação ([`ConfirmacaoOperacao`](#confirmacaooperacao)), o desconhecimento ([`DesconhecimentoOperacao`](#desconhecimentooperacao)) e a operação não realizada ([`OperacaoNaoRealizada`](#operacaonaorealizada)). A base é fechada (construtor `private protected`). |
| [`NotaLida`](#notalida) | O resultado da leitura de uma NF-e ou NFC-e ([`Ler`](#leiautenfe400)): a nota, a chave, o protocolo, o XML recebido e os problemas (docs/LEITURA.md). |
| [`OpcoesDanfce`](#opcoesdanfce) | As opções do DANFCE ([`Danfce`](#leiautenfe400)). |
| [`OpcoesDanfe`](#opcoesdanfe) | As opções do DANFE da NF-e ([`Danfe`](#leiautenfe400)). |
| [`OperacaoNaoRealizada`](#operacaonaorealizada) | A operação não realizada (210240), com a justificativa. A sequência é 1, ou 2 na retificação. |
| [`OrientacaoDanfe`](#orientacaodanfe) | A orientação da folha A4 do DANFE (Anexo II 3.6.1 e 3.6.3). |
| [`RecuperacaoAutorizacao`](#recuperacaoautorizacao) | A recuperação da autorização cuja resposta se perdeu (PROJETO.md 8.6, docs/CONTRATOS.md 15): consulta a chave da nota assinada (`NFeConsultaProtocolo4`) e, achado o protocolo de autorização ou de denegação daquele XML, monta o `nfeProc`. Não reenvia nada. |
| [`ResultadoEvento`](#resultadoevento) | O resultado de um evento do [`RetornoEvento`](#retornoevento) (o `retEvento`). Imutável. |
| [`ResultadoNota`](#resultadonota) | Uma nota do lote no [`RetornoAutorizacao`](#retornoautorizacao): a chave, o XML enviado, o protocolo e o `nfeProc`. |
| [`ResumoEvento`](#resumoevento) | O resumo de um evento de NF-e (`resEvento` 1.01, NT 2014.002, 3.11.2), que o Ambiente Nacional distribui antes da manifestação. Imutável. |
| [`ResumoNFe`](#resumonfe) | O resumo de uma NF-e (`resNFe` 1.01, NT 2014.002, 3.11.1): o que o Ambiente Nacional distribui ao destinatário antes da manifestação, com o necessário para ele se manifestar. Imutável. |
| [`RetornoAutorizacao`](#retornoautorizacao) | O retorno da [`Autorizacao`](#autorizacao) e da [`ConsultaRecibo`](#consultarecibo) (o `retEnviNFe` e o `retConsReciNFe`, docs/CONTRATOS.md 14): os campos do lote e o resultado de cada nota. Imutável. |
| [`RetornoCadastro`](#retornocadastro) | O retorno da [`ConsultaCadastro`](#consultacadastro) (o `retConsCad` 2.00, MOC 7.0, Visão Geral, 5.6, Tabela 5-22, docs/CONTRATOS.md 24): o status e os contribuintes achados. Imutável. |
| [`RetornoDistribuicao`](#retornodistribuicao) | O retorno da [`DistribuicaoDFe`](#distribuicaodfe) (o `retDistDFeInt` 1.01, docs/CONTRATOS.md 23): o status, os documentos já descompactados e a posição do NSU. Imutável. |
| [`RetornoEvento`](#retornoevento) | O retorno do [`EnvioEvento`](#envioevento) (o `retEnvEvento`): o lote e o resultado de cada evento. Imutável. |
| [`RetornoInutilizacao`](#retornoinutilizacao) | O retorno da [`Inutilizacao`](#inutilizacao) (o `retInutNFe`). Imutável. |
| [`RetornoProtocolo`](#retornoprotocolo) | O retorno da [`ConsultaProtocolo`](#consultaprotocolo) (o `retConsSitNFe`, docs/CONTRATOS.md 15): a situação da nota, o protocolo dela e os eventos. Imutável. |
| [`SituacaoCadastral`](#situacaocadastral) | A situação cadastral do contribuinte (`cSit`, MOC 7.0, Tabela 5-22, GR10). Aberta: um valor que o MOC não conhece entra como o número, com aviso `NFE-VALOR`. |
| [`SituacaoResumo`](#situacaoresumo) | A situação da NF-e no resumo (`cSitNFe` do `resNFe`, NT 2014.002, 3.11.1). Aberta, como as outras enumerações do leiaute: um valor que a lib não conhece chega como o número, convertido. |
| [`TipoConsultaDistribuicao`](#tipoconsultadistribuicao) | Qual das três consultas do `distDFeInt` a [`DistribuicaoDFe`](#distribuicaodfe) faz (NT 2014.002, 3.4.1). |
| [`TipoDocumentoDistribuido`](#tipodocumentodistribuido) | O tipo de um documento distribuído, pelo `schema` do `docZip` (NT 2014.002, 3.2): o prefixo do nome do arquivo (`resNFe_`, `procNFe_`, `resEvento_`, `procEventoNFe_`), sem a versão. |
| [`TipoEvento`](#tipoevento) | O tipo de um evento (`tpEvento`, MOC 7.0, Visão Geral, 3.1): o código de 6 dígitos. |
| [`UrlConsultaNFCe`](#urlconsultanfce) | A URL de consulta da NFC-e de uma UF num ambiente: uma linha da tabela do leiaute ([`UrlsConsulta`](#leiautenfe400), docs/NTS.md 4). Imutável. |
| [`VersaoQrCode`](#versaoqrcode) | A versão do QR Code que a lib escreve na NFC-e (docs/CONTRATOS.md 16). A versão 100, a primeira, só se lê: a SEFAZ a recusa (903). |
| [`ViaDanfce`](#viadanfce) | A via do DANFE NFC-e (a contingência exige duas: a do consumidor e a do estabelecimento). |

## Autorizacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoAutorizacao>`](opendfe.md#parametrowstretorno)

A autorização de 1 a 50 notas (`NFeAutorizacao4`, docs/CONTRATOS.md 14): uma nota vai na resposta síncrona (`indSinc=1`); de 2 a 50, no lote com recibo, que se consulta com [`ConsultaRecibo`](#consultarecibo). A quantidade decide, porque desde a NT 2025.001 o lote de uma nota é rejeitado (452) e a síncrona com mais de uma também (764).

As notas vêm do modelo (o leiaute escreve com os valores do núcleo; o núcleo assina, valida e transmite) ou já assinadas (o [`Preparar`](opendfe.md#dfe), o reenvio depois de uma queda), que vão com os mesmos bytes, sem a declaração XML. As do modelo vão ao autorizador da UF e do tipo de emissão do núcleo; as assinadas, ao da UF e do tipo de emissão da chave (PROJETO.md 8.4). O problema de uma nota leva o índice dela no parâmetro na propriedade (`Notas[0].Itens[2].Produto.Valor`, `Xmls[1].Emitente.Cnpj`) e o caminho da nota (`NFe/infNFe/det[3]/prod/vProd`). A nota do modelo não é alterada: executar de novo a mesma nota, com o `cNF` nulo, sorteia outra chave. O reenvio depois de uma queda usa o XML assinado ([`Xml`](#resultadonota), [`XmlPedido`](opendfe.md#rastrows)) ou a nota preparada antes (docs/CONTRATOS.md 14, decisão 13).

### Construtores

| Construtor | Descrição |
|---|---|
| `Autorizacao(params IEnumerable<NotaFiscal> notas)` | A autorização das notas do modelo. **`ArgumentNullException`:** A lista é nula. **`ArgumentException`:** Nenhuma nota, mais de 50, ou uma nula. |
| `Autorizacao(params IEnumerable<string> xmls)` | A autorização das notas já assinadas (o `NFe`), com os mesmos bytes. **`ArgumentNullException`:** A lista é nula. **`ArgumentException`:** Nenhuma nota, mais de 50, ou uma nula ou vazia. |

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `MaximoNotas` | `int` | 50 | O máximo de notas num lote (o `enviNFe`). |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Notas` | `IReadOnlyList<NotaFiscal>` | get | As notas do modelo; vazia quando vieram os XMLs. |
| `Xmls` | `IReadOnlyList<string>` | get | As notas assinadas; vazia quando vieram as notas do modelo. |
| `Lote` | `long?` | get; init | O número do lote (`idLote`, de 1 a 15 dígitos); nulo: do relógio da configuração, em UTC (`AAMMDDhhmmssmmm`). A SEFAZ não exige que seja único. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static NotasEnviadas(string xmlPedido)` | `IReadOnlyList<string>` | As notas assinadas, com os bytes como foram, tiradas da mensagem enviada: o [`XmlPedido`](opendfe.md#rastrows) do resultado ou da [`FalhaTransmissaoException`](opendfe.md#falhatransmissaoexception) (docs/CONTRATOS.md 15, decisão 9). É o XML da nota do modelo para a [`RecuperacaoAutorizacao`](#recuperacaoautorizacao) e o reenvio depois de uma queda. **Retorna:** Os `NFe` do lote, na ordem. **`ArgumentNullException`:** A mensagem é nula. **`FormatException`:** A mensagem não é um `enviNFe` da NF-e. |

## Cancelamento

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Evento`](#evento)

O cancelamento (110111, MOC 7.0, Visão Geral, 5.9): o autor é o emitente da chave. O prazo (501), o protocolo (222) e a situação da nota ficam para a SEFAZ.

### Construtores

| Construtor | Descrição |
|---|---|
| `Cancelamento(ChaveAcesso chave, string protocolo, string justificativa)` | O cancelamento da nota da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** O protocolo não tem 15 dígitos; a justificativa é nula ou vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Protocolo` | `string` | get | O número do protocolo da autorização (`nProt`). |
| `Justificativa` | `string` | get | A justificativa (`xJust`). |

## CancelamentoSubstituicao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Evento`](#evento)

O cancelamento por substituição (110112, MOC 7.0, Visão Geral, 5.9) de uma NFC-e: cancela a NFC-e e aponta a que a substitui. O autor é o emitente da chave. A janela de 168 horas (501), a substituta que não confere (911 a 919), a nota fora da emissão normal (920, 921) e a situação da nota ficam para a SEFAZ.

### Construtores

| Construtor | Descrição |
|---|---|
| `CancelamentoSubstituicao(ChaveAcesso chave, string protocolo, string justificativa, ChaveAcesso chaveSubstituta)` | O cancelamento por substituição da NFC-e da chave. **`ArgumentNullException`:** Alguma das chaves é nula. **`ArgumentException`:** Alguma das chaves não é de NFC-e (modelo 65); as duas chaves são iguais; o protocolo não tem 15 dígitos; a justificativa é nula ou vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Protocolo` | `string` | get | O número do protocolo da autorização da NFC-e cancelada (`nProt`). |
| `Justificativa` | `string` | get | A justificativa (`xJust`). |
| `ChaveSubstituta` | `ChaveAcesso` | get | A chave de acesso da NFC-e substituta (`chNFeRef`). |
| `VersaoAplicativo` | `string` | get; init | O `verAplic` do `detEvento` (1 a 20 caracteres): a versão do aplicativo que emite. Padrão: "OpenDFe " e a versão do pacote, cortada em 20; quem usa troca pela do seu sistema. O que passa de 20 caracteres é cortado; o nulo volta ao padrão. |

## CartaCorrecao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Evento`](#evento)

A carta de correção (110110, MOC 7.0, Visão Geral, 5.10) de uma NF-e; a NFC-e não tem. O texto da condição de uso é fixo (o schema o enumera). O autor é o emitente da chave. Os prazos, a sequência acima da registrada e o que a carta pode corrigir ficam para a SEFAZ (o 580 de nota cancelada, o 577 de data antes da emissão).

### Construtores

| Construtor | Descrição |
|---|---|
| `CartaCorrecao(ChaveAcesso chave, int sequencia, string correcao)` | A carta de correção da NF-e da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** A chave não é de NF-e (modelo 55); a correção é nula ou vazia. **`ArgumentOutOfRangeException`:** A sequência está fora de 1 a 20. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Correcao` | `string` | get | O texto da correção (`xCorrecao`). |

## CienciaOperacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Manifestacao`](#manifestacao)

A ciência da operação (210210): o destinatário declara que soube da nota, e isso libera o download da NF-e completa na distribuição DF-e (docs/CONTRATOS.md 23). A sequência é sempre 1.

### Construtores

| Construtor | Descrição |
|---|---|
| `CienciaOperacao(ChaveAcesso chave, string autor)` | A ciência da nota da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido. |

## CodigoSeguranca

Classe selada · Namespace `OpenDFe.NFe`

O CSC (Código de Segurança do Contribuinte) que a SEFAZ da UF deu à empresa (Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, 4.6). O código é segredo: o [`ToString`](#codigoseguranca) mostra só o identificador.

O manual diz "alfanuméricos", mas o exemplo dele tem hífen: a conferência fica no que quebra o QR Code (o tamanho, o caractere fora do ASCII visível e a barra vertical, que separa os parâmetros), não no alfabeto. Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `CodigoSeguranca(int identificador, string codigo)` | Cria o CSC. **`ArgumentOutOfRangeException`:** O identificador está fora de 1 a 999999. **`ArgumentException`:** O código é nulo, tem menos de 16 ou mais de 36 caracteres, tem caractere fora do ASCII visível ou tem a barra vertical. |

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `MenorIdentificador` | `int` | 1 | O menor identificador (`idCSC`). |
| `MaiorIdentificador` | `int` | 999999 | O maior identificador: o `idCSC` tem até 6 dígitos. |
| `MenorCodigo` | `int` | 16 | O menor tamanho do código. |
| `MaiorCodigo` | `int` | 36 | O maior tamanho do código. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Identificador` | `int` | get | O identificador (`idCSC`), escrito sem zeros à esquerda. |
| `Codigo` | `string` | get | O código do CSC. É segredo: não o registre. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `ToString()` | `string` | Só o identificador (`"CSC 1"`): o código nunca aparece. |

## ConfiguracaoNFCe

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ConfiguracaoDocumento`](opendfe.md#configuracaodocumento)

A parte da configuração que é da NFC-e (PROJETO.md 3.2 e 9.1, docs/CONTRATOS.md 16): a versão do QR Code, o CSC e as URLs de consulta. Opcional: sem ela, o QR Code sai na versão 3 com as URLs da tabela do leiaute ([`UrlsConsulta`](#leiautenfe400)). Vale também para a NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6, NT 2026.002), só pelas URLs: o QR Code dela é sempre a versão 3. Imutável.

### Construtores

| Construtor | Descrição |
|---|---|
| `ConfiguracaoNFCe()` | O QR Code na versão 3, sem CSC. |
| `ConfiguracaoNFCe(CodigoSeguranca csc)` | O QR Code na versão 2, com o CSC da UF. **`ArgumentNullException`:** O CSC é nulo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `QrCode` | `VersaoQrCode` | get | A versão do QR Code: [`V3`](#versaoqrcode) sem CSC, [`V2`](#versaoqrcode) com ele. |
| `Csc` | `CodigoSeguranca?` | get | O CSC; nulo na versão 3. |
| `UrlQrCode` | `Uri?` | get; init | Troca a URL do QR Code da tabela do leiaute, na UF e no ambiente do núcleo (que são os da nota): o endereço sem a query, a que a lib acrescenta o `?p=`. |
| `UrlChave` | `Uri?` | get; init | Troca a URL da consulta pela chave (o `urlChave`) da tabela do leiaute. |

## ConfirmacaoOperacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Manifestacao`](#manifestacao)

A confirmação da operação (210200). A sequência é 1, ou 2 na retificação (o Ajuste SINIEF 43/23 permite uma); depois dela, a nota não pode mais ser cancelada (221).

### Construtores

| Construtor | Descrição |
|---|---|
| `ConfirmacaoOperacao(ChaveAcesso chave, string autor, int sequencia = 1)` | A confirmação da operação da nota da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido. **`ArgumentOutOfRangeException`:** A sequência está fora de 1 a 2. |

## ConsultaCadastro

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoCadastro>`](opendfe.md#parametrowstretorno)

A consulta ao cadastro de contribuintes da UF (`CadConsultaCadastro4`, MOC 7.0, Visão Geral, 5.6, docs/CONTRATOS.md 24): o `ConsCad` 2.00 por CNPJ, por CPF ou por inscrição estadual e o `retConsCad` com os contribuintes achados ([`RetornoCadastro`](#retornocadastro)). Só na NF-e.

O pedido não é assinado e não leva o ambiente. Vai ao autorizador da UF consultada ([`UF`](#consultacadastro)), no atendimento normal, qualquer que seja a UF do núcleo. Só atendem o serviço as 10 UFs com autorizador próprio de NF-e (AM, BA, GO, MG, MS, MT, PE, PR, RS e SP) e, na SVRS, AC, ES, RN, PB e SC: a UF que não atende (a SVAN do MA e as outras da SVRS) é recusada antes de transmitir (`NFE-CADASTRO`), a menos que o [`Destino`](opendfe.md#parametrows) seja informado (quem escolhe o destino sabe o que faz). A SUFRAMA (`SU`) não é uma UF e não existe aqui. O certificado do núcleo é o do solicitante: o transmissor irregular é recusado pela SEFAZ (257). A consulta é só da NF-e: a NFC-e não a registra nesta versão (o PDV não consulta cadastro).

### Construtores

| Construtor | Descrição |
|---|---|
| `ConsultaCadastro(UF uf, string documento)` | A consulta pelo CNPJ ou pelo CPF. **`ArgumentNullException`:** O documento é nulo. **`ArgumentException`:** O documento não é um CNPJ nem um CPF válido. **`ArgumentOutOfRangeException`:** A UF não é conhecida. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `UF` | `UF` | get | A UF consultada (`UF`). |
| `Documento` | `string?` | get | O CNPJ ou o CPF consultado, sem máscara; nulo na consulta pela inscrição. |
| `InscricaoEstadual` | `string?` | get | A inscrição estadual consultada; nula na consulta pelo CNPJ ou pelo CPF. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Inscricao(UF uf, string inscricaoEstadual)` | `ConsultaCadastro` | A consulta pela inscrição estadual. **Retorna:** A consulta. **`ArgumentNullException`:** A inscrição é nula. **`ArgumentException`:** A inscrição não tem de 2 a 14 algarismos, é toda zero ou é `ISENTO`. **`ArgumentOutOfRangeException`:** A UF não é conhecida. |

## ConsultaProtocolo

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoProtocolo>`](opendfe.md#parametrowstretorno)

A consulta da situação de uma nota pela chave (`NFeConsultaProtocolo4`, docs/CONTRATOS.md 15): o `consSitNFe` e o `retConsSitNFe`, com o protocolo e os eventos da nota.

A consulta vai ao autorizador da UF e do tipo de emissão da chave (PROJETO.md 8.4): a nota emitida em SVC-AN é consultada na SVC-AN, que só acha as que ela autorizou; o `Destino` do parâmetro sobrepõe, para consultar no autorizador normal, que acha também as da SVC. A chave de outro modelo que o do núcleo (a NFC-e num núcleo de NF-e) é recusada antes de transmitir (`NFE-CHAVE`, decisão 3).

### Construtores

| Construtor | Descrição |
|---|---|
| `ConsultaProtocolo(ChaveAcesso chave)` | A consulta da nota da chave. **`ArgumentNullException`:** A chave é nula. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Chave` | `ChaveAcesso` | get | A chave de acesso consultada (`chNFe`). |

## ConsultaRecibo

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoAutorizacao>`](opendfe.md#parametrowstretorno)

A consulta do recibo do lote (`NFeRetAutorizacao4`, docs/CONTRATOS.md 14): os protocolos das notas do lote que a [`Autorizacao`](#autorizacao) devolveu com o recibo (103).

Com as notas assinadas do lote em [`Xmls`](#consultarecibo) (as [`Xml`](#resultadonota) do retorno com o recibo), o retorno liga cada protocolo à nota e traz o `nfeProc`, e a consulta vai ao autorizador da chave da primeira nota (PROJETO.md 8.4); sem elas, só os protocolos, e a consulta vai ao autorizador do núcleo. A lib não consulta sozinha: esperar o tempo médio e repetir é de quem usa (decisão 14).

### Construtores

| Construtor | Descrição |
|---|---|
| `ConsultaRecibo(string recibo)` | A consulta do recibo. **`ArgumentNullException`:** O recibo é nulo. **`ArgumentException`:** O recibo não tem 15 dígitos. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Recibo` | `string` | get | O número do recibo (`nRec`). |
| `Xmls` | `IReadOnlyList<string>` | get; init | As notas assinadas do lote, para o `nfeProc`; vazia: só os protocolos. |

## ContribuinteCadastro

Classe selada · Namespace `OpenDFe.NFe`

Um contribuinte do cadastro da UF (o `infCad`, grupos GR07 a GR29 da Tabela 5-22). Imutável. Tem dados do contribuinte: guardar com cuidado, nunca em log.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `InscricaoEstadual` | `string` | get | A inscrição estadual (`IE`): 2 a 14 algarismos ou `ISENTO`. |
| `Cnpj` | `string?` | get | O CNPJ (`CNPJ`); nulo quando o contribuinte é pessoa física ([`Cpf`](#contribuintecadastro)). |
| `Cpf` | `string?` | get | O CPF (`CPF`); nulo quando o contribuinte é pessoa jurídica ([`Cnpj`](#contribuintecadastro)). |
| `UF` | `UF?` | get | A UF de localização (`UF`), que em algumas situações é diferente da consultada (a inscrição de substituto tributário, por exemplo); nula quando não veio ou não se leu. |
| `Situacao` | `SituacaoCadastral?` | get | A situação cadastral (`cSit`); nula quando não veio ou não é um número. |
| `CredenciamentoNFe` | `CredenciamentoDFe?` | get | O credenciamento para emitir NF-e na SEFAZ consultada (`indCredNFe`); nulo quando não veio ou não é um número. |
| `CredenciamentoCTe` | `CredenciamentoDFe?` | get | O credenciamento para emitir CT-e na SEFAZ consultada (`indCredCTe`); nulo quando não veio ou não é um número. |
| `Nome` | `string` | get | A razão social ou o nome (`xNome`); vazia quando não veio. |
| `Fantasia` | `string?` | get | O nome fantasia (`xFant`). |
| `RegimeApuracao` | `string?` | get | O regime de apuração do ICMS (`xRegApur`), como o texto da SEFAZ. |
| `Cnae` | `string?` | get | O CNAE fiscal (`CNAE`), de 6 ou 7 algarismos. |
| `InicioAtividade` | `DateOnly?` | get | A data de início das atividades (`dIniAtiv`). |
| `UltimaSituacao` | `DateOnly?` | get | A data da última modificação da situação cadastral (`dUltSit`). |
| `Baixa` | `DateOnly?` | get | A data da baixa do contribuinte (`dBaixa`). |
| `InscricaoUnica` | `string?` | get | A inscrição estadual única (`IEUnica`). |
| `InscricaoAtual` | `string?` | get | A inscrição estadual atual (`IEAtual`). |
| `Endereco` | `EnderecoCadastro?` | get | O endereço (`ender`); nulo quando o cadastro não o traz. |

## CredenciamentoDFe

Enumeração · Namespace `OpenDFe.NFe`

O credenciamento do contribuinte para emitir NF-e ou CT-e na SEFAZ consultada (`indCredNFe` e `indCredCTe`, MOC 7.0, Tabela 5-22, GR11 e GR12). Significa só que o contribuinte é credenciado nessa SEFAZ. Aberta: um valor que o MOC não conhece entra como o número, com aviso `NFE-VALOR`.

| Membro | Valor | Descrição |
|---|---|---|
| `NaoCredenciado` | 0 | Não credenciado para a emissão (0). |
| `Credenciado` | 1 | Credenciado (1). |
| `ObrigatorioTotal` | 2 | Credenciado com obrigatoriedade para todas as operações (2). |
| `ObrigatorioParcial` | 3 | Credenciado com obrigatoriedade parcial (3). |
| `NaoInformado` | 4 | A SEFAZ não fornece a informação (4). |

## DesconhecimentoOperacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Manifestacao`](#manifestacao)

O desconhecimento da operação (210220). A sequência é 1, ou 2 na retificação.

### Construtores

| Construtor | Descrição |
|---|---|
| `DesconhecimentoOperacao(ChaveAcesso chave, string autor, int sequencia = 1)` | O desconhecimento da operação da nota da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido. **`ArgumentOutOfRangeException`:** A sequência está fora de 1 a 2. |

## DistribuicaoDFe

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoDistribuicao>`](opendfe.md#parametrowstretorno)

A consulta à distribuição de DF-e do Ambiente Nacional (`NFeDistribuicaoDFe`, NT 2014.002, docs/CONTRATOS.md 23): o `distDFeInt` por último NSU, por NSU ou por chave, e o `retDistDFeInt` com os documentos já descompactados ([`RetornoDistribuicao`](#retornodistribuicao)). Só na NF-e: a NFC-e não é distribuída. Sempre ao AN.

O pedido não é assinado. O certificado do núcleo tem de ser o do interessado (a raiz do CNPJ, ou o CPF): a matriz consulta as filiais; outro CNPJ é rejeitado (593, 472). A lib não repete a consulta, não espera e não guarda o NSU (PROJETO.md 8.6). A NT bloqueia por 1 hora o CNPJ que consulta a mais ou fora da sequência (656): o laço que respeita isso está em docs/DISTRIBUICAO.md.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Interessado` | `string` | get | O CNPJ ou o CPF do interessado, só com os algarismos e as letras (sem máscara). |
| `Consulta` | `TipoConsultaDistribuicao` | get | Qual das três consultas é. |
| `NsuConsultado` | `long?` | get | O NSU: o `ultNSU` na consulta por último NSU, o `NSU` na consulta por NSU; nulo na consulta por chave. |
| `ChaveConsultada` | `ChaveAcesso?` | get | A chave da consulta por chave; nula nas outras. |
| `UFAutor` | `UF?` | get | O `cUFAutor`: a UF do autor da consulta; nulo é a UF do núcleo. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Chave(string interessado, ChaveAcesso chave, UF? ufAutor = null)` | `DistribuicaoDFe` | O documento de uma chave: consulta pontual, que conta no limite de 20 por hora. **Retorna:** A consulta. **`ArgumentException`:** O interessado não é um CNPJ nem um CPF válido, ou a UF não é conhecida. **`ArgumentNullException`:** A chave é nula. |
| `static Nsu(string interessado, long nsu, UF? ufAutor = null)` | `DistribuicaoDFe` | O documento de um NSU: consulta pontual, que conta no limite de 20 por hora. **Retorna:** A consulta. **`ArgumentException`:** O interessado não é um CNPJ nem um CPF válido; o NSU é negativo ou passa de 15 algarismos; ou a UF não é conhecida. |
| `static UltimoNsu(string interessado, long ultimoNsu = 0, UF? ufAutor = null)` | `DistribuicaoDFe` | Os documentos a partir do NSU seguinte ao informado (0 na primeira vez): até 50 por resposta, em ordem de NSU. **Retorna:** A consulta. **`ArgumentException`:** O interessado não é um CNPJ nem um CPF válido; o NSU é negativo ou passa de 15 algarismos; ou a UF não é conhecida. |

## DocumentoDistribuido

Classe selada · Namespace `OpenDFe.NFe`

Um documento da distribuição DF-e (`docZip`, NT 2014.002, 3.2) já descompactado: o XML como veio e, nos resumos, os campos nos tipos do modelo. Imutável.

Para a NF-e processada ([`NFeProcessada`](#tipodocumentodistribuido)) e o evento processado ([`EventoProcessado`](#tipodocumentodistribuido)), quem usa lê o [`Xml`](#documentodistribuido) com [`Ler`](#leiautenfe400) (a [`NotaLida`](#notalida)) ou direto: a leitura da nota não é duplicada aqui.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Nsu` | `long?` | get | O NSU do documento (`NSU` do `docZip`); nulo quando não veio, como na consulta pela chave. |
| `Schema` | `string` | get | O `schema` do `docZip` (`"resNFe_v1.01.xsd"`, `"procNFe_v4.00.xsd"`), como veio. |
| `Tipo` | `TipoDocumentoDistribuido` | get | O tipo do documento, pelo [`Schema`](#documentodistribuido). |
| `Xml` | `string` | get | O XML descompactado (gzip), em UTF-8, como veio. |
| `Chave` | `ChaveAcesso?` | get | A chave de acesso da NF-e do documento, lida do XML quando possível; nula no [`Outro`](#tipodocumentodistribuido). |
| `ResumoNFe` | `ResumoNFe?` | get | O resumo da NF-e: só no tipo [`ResumoNFe`](#tipodocumentodistribuido) (e nulo se o XML não se lê nos tipos). |
| `ResumoEvento` | `ResumoEvento?` | get | O resumo do evento: só no tipo [`ResumoEvento`](#tipodocumentodistribuido) (e nulo se o XML não se lê nos tipos). |

## Duplicidade

Classe selada · Namespace `OpenDFe.NFe`

A nota já registrada na SEFAZ que causou a rejeição por duplicidade (204 e 539, PROJETO.md 8.6, docs/CONTRATOS.md 15): no 204, a mesma nota (recuperar o `nfeProc` com a [`RecuperacaoAutorizacao`](#recuperacaoautorizacao)); no 539, outra chave com a mesma numeração (consultar a outra com a [`ConsultaProtocolo`](#consultaprotocolo)). Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Chave` | `ChaveAcesso?` | get | A chave da nota registrada: no 204, a da própria nota; no 539, a do motivo (`[chNFe:...]`), nula quando ele não a traz. |
| `Recibo` | `string?` | get | O recibo do lote em que a nota registrada foi autorizada (`[nRec:...]`), quando o motivo o traz. |
| `MesmaChave` | `bool` | get | A nota registrada tem a mesma chave (204); falso: outra chave com a mesma numeração (539). |

## EnderecoCadastro

Classe selada · Namespace `OpenDFe.NFe`

O endereço de um contribuinte do cadastro (o `ender`, `TEndereco`). Imutável; todos os campos são opcionais.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Logradouro` | `string?` | get | O logradouro (`xLgr`). |
| `Numero` | `string?` | get | O número (`nro`). |
| `Complemento` | `string?` | get | O complemento (`xCpl`). |
| `Bairro` | `string?` | get | O bairro (`xBairro`). |
| `CodigoMunicipio` | `string?` | get | O código IBGE do município (`cMun`), de 7 algarismos. |
| `Municipio` | `string?` | get | O nome do município (`xMun`). |
| `Cep` | `string?` | get | O CEP (`CEP`), de 7 ou 8 algarismos. |

## EnvioEvento

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoEvento>`](opendfe.md#parametrowstretorno)

O envio de 1 a 20 eventos (`NFeRecepcaoEvento4`, docs/CONTRATOS.md 18): o `envEvento`, com cada evento assinado à parte, e o `retEnvEvento`. Na NF-e e na NFC-e.

O destino e o tipo de emissão saem da chave do primeiro evento (PROJETO.md 8.4); só o cancelamento de nota de SVC vai à SVC. O `Destino` do parâmetro sobrepõe, no autorizador normal. Os eventos de um lote vão ao mesmo destino (`NFE-LOTE`), e a chave de outro modelo que o do núcleo é `NFE-CHAVE`.

### Construtores

| Construtor | Descrição |
|---|---|
| `EnvioEvento(params IEnumerable<Evento> eventos)` | O envio dos eventos. **`ArgumentNullException`:** A lista é nula. **`ArgumentException`:** Nenhum evento, mais de 20, ou um nulo. |

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `MaximoEventos` | `int` | 20 | O máximo de eventos num envio. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Eventos` | `IReadOnlyList<Evento>` | get | Os eventos, na ordem do envio. |
| `Lote` | `long?` | get; init | O `idLote` (1 a 15 dígitos); nulo: do relógio da configuração, em UTC (`AAMMDDhhmmssmmm`). |

## Evento

Classe abstrata · Namespace `OpenDFe.NFe`

Um evento de um documento (docs/CONTRATOS.md 18): o que o [`EnvioEvento`](#envioevento) leva. A base é fechada: só o leiaute deriva (construtor `private protected` e membros internos), para o cancelamento, a substituição, a carta de correção e a manifestação entrarem sem mudar o envio nem a operação.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Chave` | `ChaveAcesso` | get | A chave de acesso do documento do evento (`chNFe`). |
| `Tipo` | `TipoEvento` | get | O tipo do evento (`tpEvento`). |
| `Sequencia` | `int` | get | O sequencial do evento (`nSeqEvento`); 1 no cancelamento. |
| `Momento` | `DateTimeOffset?` | get; init | A data e hora do evento (`dhEvento`); nulo: o instante do relógio da configuração, no fuso de Brasília (-03:00). Para reenviar o mesmo evento (mesmos bytes e mesmo `Id`), informe o mesmo valor. |

## Inutilizacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoInutilizacao>`](opendfe.md#parametrowstretorno)

A inutilização de uma faixa de numeração (`NFeInutilizacao4`, MOC 7.0, Visão Geral, 5.3; docs/CONTRATOS.md 18): o `inutNFe` assinado. Vai sempre ao autorizador normal da UF do núcleo, com o tipo de emissão normal (a SVC não oferece o serviço). O modelo (55 ou 65) e a UF são os do núcleo.

O emitente com CPF não tem inutilização neste schema (o `infInut` só tem CNPJ): pendente.

### Construtores

| Construtor | Descrição |
|---|---|
| `Inutilizacao(string cnpj, int serie, int numeroInicial, int numeroFinal, string justificativa)` | A inutilização da faixa. **`ArgumentException`:** CNPJ inválido, série fora de 0 a 999, número inicial menor que 1, final menor que o inicial ou maior que 999999999, justificativa nula ou vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Ano` | `int?` | get; init | O ano da numeração, com 2 dígitos; nulo: o ano do relógio da configuração, no fuso de Brasília. |
| `Cnpj` | `string` | get | O CNPJ do emitente, sem máscara. |
| `Serie` | `int` | get | A série. |
| `NumeroInicial` | `int` | get | O primeiro número da faixa. |
| `NumeroFinal` | `int` | get | O último número da faixa. |
| `Justificativa` | `string` | get | A justificativa. |

## LeiauteNFe400

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ILeiaute`](opendfe.md#ileiaute)

O leiaute 4.00 da NF-e (modelo 55) e da NFC-e (modelo 65), os dois documentos no mesmo leiaute e no mesmo modelo de dados (PROJETO.md 3.1): o que se passa a [`Criar`](opendfe.md#dfe).

Sem estado e seguro para uso concorrente: uma instância serve a todos os núcleos, de qualquer emitente e documento. Os schemas são os do pacote de liberação embutido (docs/NTS.md), carregados na primeira vez que uma operação os usa. Declara os serviços SOAP, quem atende cada UF em cada tipo de emissão e as URLs dos autorizadores nos dois ambientes (etapa 4.2), e as operações, nos dois documentos: o status do serviço ([`ConsultaStatus`](opendfe.md#consultastatus), etapa 4.3); a autorização ([`Autorizacao`](#autorizacao)), a consulta do recibo ([`ConsultaRecibo`](#consultarecibo)) e o preparo da nota ([`Preparar`](opendfe.md#dfe)), etapa 4.4, e a NFC-e com o QR Code que a lib escreve (etapa 5.1, [`ConfiguracaoNFCe`](#configuracaonfce)); a consulta do protocolo ([`ConsultaProtocolo`](#consultaprotocolo)) e a recuperação da autorização ([`RecuperacaoAutorizacao`](#recuperacaoautorizacao)), etapa 4.5; o registro de eventos ([`EnvioEvento`](#envioevento), com o [`Cancelamento`](#cancelamento)) e a inutilização ([`Inutilizacao`](#inutilizacao)), etapa 6.1; e, só na NF-e, a distribuição DF-e ([`DistribuicaoDFe`](#distribuicaodfe), ao AN), etapa 8.1, e a consulta cadastro ([`ConsultaCadastro`](#consultacadastro), à UF consultada), etapa 8.2; as manifestações do destinatário ([`Manifestacao`](#manifestacao)) são eventos do [`EnvioEvento`](#envioevento), ao AN, etapa 8.2. Declara também as formas de XML que lê (o `NFe` e o `nfeProc`), para o [`Detectar`](opendfe.md#dfe), e lê a nota ([`Ler`](#leiautenfe400)) sem núcleo.

### Construtores

| Construtor | Descrição |
|---|---|
| `LeiauteNFe400()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Versao` | `string` | get | A versão do leiaute, como no atributo `versao` do XML: `"4.00"`. |
| `Documentos` | `IReadOnlyList<Documento>` | get | A NF-e e a NFC-e. |
| `UrlsConsulta` | `IReadOnlyList<UrlConsultaNFCe>` | get | A tabela das URLs de consulta da NFC-e (docs/NTS.md 4): a do QR Code e a da consulta pela chave, por UF e ambiente, só as linhas conferidas em fonte primária; a UF que não consta usa a URL da [`ConfiguracaoNFCe`](#configuracaonfce). A NF-e com `tpImp` 6 usa a mesma tabela (NT 2026.002). |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Danfce(NotaLida nota, OpcoesDanfce? opcoes = null)` | `DocumentoImpressao` | Monta o DANFE NFC-e (o DANFCE, a NFC-e impressa em bobina) de uma NFC-e lida: o `nfeProc` autorizado, ou o `NFe` assinado em contingência offline, sem protocolo (etapa 7.2, docs/IMPRESSAO.md 1). **Retorna:** O documento de impressão, para o renderizador do PDV ou do PDF. **`ArgumentNullException`:** A nota lida é nula. **`ArgumentException`:** A leitura não achou a nota; a nota não é NFC-e (modelo 65); não tem a chave; não tem o QR Code e a URL de consulta (`qrCode` e `urlChave`); o protocolo é de outra situação que não 100 ou 150; ou o papel das opções não é a bobina de 80 ou de 58 mm. |
| `Danfe(NotaLida nota, OpcoesDanfe? opcoes = null)` | `DocumentoImpressao` | Monta o DANFE da NF-e (modelo 55) de uma NF-e lida: o `nfeProc` autorizado, ou o `NFe` assinado em contingência, sem protocolo (etapa 7.3, docs/IMPRESSAO.md 2). O `tpImp` da nota escolhe o documento: retrato (1) e paisagem (2) saem no A4, com os quadros e os campos do Anexo II do MOC 7.0 e os acréscimos da NT 2026.010; o Simplificado Tipo 2 (6) sai no leiaute do DANFE NFC-e (NT 2026.003). **Retorna:** O documento de impressão, para o renderizador do PDF. **`ArgumentNullException`:** A nota lida é nula. **`ArgumentException`:** A leitura não achou a nota; a nota não é NF-e (modelo 55); não tem a chave; o protocolo é de outra situação que não 100 ou 150; o `tpImp` é 0 (sem DANFE), 3 (não implementado), 4 ou 5 (DANFE NFC-e), ou não informado sem a orientação nas opções; a orientação ou o papel das opções não cabem no formato; ou o Tipo 2 sem o QR Code. |
| `Declarar(DeclaracaoLeiaute declaracao)` | `void` | Declara o que o leiaute tem para o documento: as operações e o preparo, as formas de XML que ele lê (o `NFe` e o `nfeProc`), iguais para a NF-e e a NFC-e (o modelo do XML diz qual dos dois), as regras do MOC que ele confere antes de transmitir (docs/REGRAS.md) e os autorizadores do documento: os serviços SOAP, os atendimentos e as URLs (docs/NTS.md 3). **`ArgumentNullException`:** A declaração é nula. |
| `Ler(string xml)` | `NotaLida` | Lê uma NF-e ou NFC-e: o `NFe`, ou o `nfeProc` com o protocolo, de qualquer NT do leiaute 4.00 (PROJETO.md 5.2, docs/LEITURA.md). **Retorna:** A nota, a chave, o protocolo, o XML recebido e os problemas. **`ArgumentNullException`:** O XML é nulo. |

## Manifestacao

Classe abstrata · Namespace `OpenDFe.NFe` · Deriva de [`Evento`](#evento)

Um evento de manifestação do destinatário (NT 2020.001 v1.60, MOC 7.0, Visão Geral, 5.11, docs/CONTRATOS.md 24): a ciência ([`CienciaOperacao`](#cienciaoperacao)), a confirmação ([`ConfirmacaoOperacao`](#confirmacaooperacao)), o desconhecimento ([`DesconhecimentoOperacao`](#desconhecimentooperacao)) e a operação não realizada ([`OperacaoNaoRealizada`](#operacaonaorealizada)). A base é fechada (construtor `private protected`).

O autor não é o emitente: é o destinatário da nota, que o chamador informa ([`Autor`](#manifestacao)); a assinatura é do certificado do núcleo, e a conferência da raiz do CNPJ ou do CPF é da SEFAZ. A manifestação vai sempre ao Ambiente Nacional ([`AN`](opendfe.md#destino), `cOrgao` 91), qualquer que seja a UF ou o tipo de emissão da chave, e só existe para a NF-e (modelo 55). Os prazos (596), a sequência (594), a ordem das manifestações (655), a nota cancelada (650, 651) e o destinatário (575) ficam para a SEFAZ.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Autor` | `string` | get | O CNPJ (numérico ou alfanumérico) ou o CPF do destinatário que manifesta, só com os algarismos e as letras. |

## NotaLida

Classe selada · Namespace `OpenDFe.NFe`

O resultado da leitura de uma NF-e ou NFC-e ([`Ler`](#leiautenfe400)): a nota, a chave, o protocolo, o XML recebido e os problemas (docs/LEITURA.md).

Imutável. A nota dentro dela é do modelo, mutável, e passa a ser de quem leu: a lib não guarda referência.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Nota` | `NotaFiscal?` | get | A nota; `null` só quando o XML não é uma NF-e do leiaute 4.00 (`NFE-XML`, `NFE-RAIZ`, `NFE-VERSAO`). |
| `Chave` | `ChaveAcesso?` | get | A chave do `Id` do `infNFe`; `null` quando ausente ou inválida (`NFE-CHAVE`). |
| `Protocolo` | `Protocolo?` | get | O protocolo do `nfeProc`; `null` no `NFe` sozinho. |
| `Xml` | `string` | get | O XML recebido, a mesma instância passada à leitura: é o que se guarda (PROJETO.md 5.3), nunca o regerado, porque a escrita de novo não reproduz os bytes da assinatura. |
| `Problemas` | `IReadOnlyList<Problema>` | get | Os problemas da leitura, na ordem do XML (docs/LEITURA.md 4). |

## OpcoesDanfce

Classe selada · Namespace `OpenDFe.NFe`

As opções do DANFCE ([`Danfce`](#leiautenfe400)).

Imutável. A lib não decide quantas vias imprimir: quem imprime chama [`Danfce`](#leiautenfe400) uma vez por via (docs/IMPRESSAO.md 1).

### Construtores

| Construtor | Descrição |
|---|---|
| `OpcoesDanfce()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Papel` | `PapelImpressao` | get; init | O papel: [`Bobina80`](opendfe-impressao.md#papelimpressao) (padrão) ou [`Bobina58`](opendfe-impressao.md#papelimpressao); outro papel é recusado pelo [`Danfce`](#leiautenfe400). |
| `Logotipo` | `Imagem1Bit?` | get; init | O logotipo do emitente, no topo; `null` sem logotipo. |
| `AssinaturaSoftware` | `Imagem1Bit?` | get; init | A marca do software emissor, no rodapé; `null` sem marca. |
| `TextoRodape` | `string?` | get; init | Uma linha livre depois do conteúdo da NFC-e, como o manual manda para o que não está no XML (por exemplo, `"ERP Core - DLA Tec"`); `null` sem a linha. |
| `Via` | `ViaDanfce` | get; init | A via; [`Consumidor`](#viadanfce) por padrão. |

## OpcoesDanfe

Classe selada · Namespace `OpenDFe.NFe`

As opções do DANFE da NF-e ([`Danfe`](#leiautenfe400)).

Imutável. O [`Papel`](#opcoesdanfe) e a [`Via`](#opcoesdanfe) são só do DANFE Simplificado Tipo 2 (`tpImp` 6, bobina, no leiaute do DANFE NFC-e); o retrato e a paisagem são sempre o A4 da orientação (docs/CONTRATOS.md 21).

### Construtores

| Construtor | Descrição |
|---|---|
| `OpcoesDanfe()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Logotipo` | `Imagem1Bit?` | get; init | O logotipo do emitente, no quadro de identificação; `null` sem logotipo. |
| `Canhoto` | `bool` | get; init | Se o canhoto destacável sai (retrato); verdadeiro por padrão. Só o retrato pode suprimi-lo (Anexo II 3.3.1): na paisagem o canhoto sai sempre. Não vale para o Tipo 2. |
| `Orientacao` | `OrientacaoDanfe?` | get; init | A orientação do A4; `null` segue o `tpImp` da nota (1 retrato, 2 paisagem). Com o `tpImp` 6 (Tipo 2) a orientação não existe e informá-la é recusado. |
| `Cancelada` | `bool` | get; init | Se a NF-e foi cancelada: o documento sai com a marca d'água `CANCELADA`. A situação da nota vem de quem usa (a consulta ou o evento); a lib não a deduz. |
| `TextoRodape` | `string?` | get; init | Uma linha livre ao fim do documento, para o que não está no XML (por exemplo, `"Impresso por ERP Core"`); `null` sem a linha. |
| `Papel` | `PapelImpressao?` | get; init | O papel do DANFE Simplificado Tipo 2 ([`Bobina80`](opendfe-impressao.md#papelimpressao) por padrão; também a bobina de 58 mm ou o A4 em pé). `null` é o padrão; no retrato e na paisagem informá-lo é recusado. |
| `Via` | `ViaDanfce` | get; init | A via do DANFE Simplificado Tipo 2 (contingência offline); [`Consumidor`](#viadanfce) por padrão. |

## OperacaoNaoRealizada

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`Manifestacao`](#manifestacao)

A operação não realizada (210240), com a justificativa. A sequência é 1, ou 2 na retificação.

### Construtores

| Construtor | Descrição |
|---|---|
| `OperacaoNaoRealizada(ChaveAcesso chave, string autor, string justificativa, int sequencia = 1)` | A operação não realizada da nota da chave. **`ArgumentNullException`:** A chave é nula. **`ArgumentException`:** A chave não é de NF-e (modelo 55); o autor não é um CNPJ nem um CPF válido; a justificativa é nula ou vazia. **`ArgumentOutOfRangeException`:** A sequência está fora de 1 a 2. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Justificativa` | `string` | get | A justificativa (`xJust`). |

## OrientacaoDanfe

Enumeração · Namespace `OpenDFe.NFe`

A orientação da folha A4 do DANFE (Anexo II 3.6.1 e 3.6.3).

| Membro | Valor | Descrição |
|---|---|---|
| `Retrato` | 0 | A folha em pé (`tpImp` 1). |
| `Paisagem` | 1 | A folha deitada (`tpImp` 2). |

## RecuperacaoAutorizacao

Classe selada · Namespace `OpenDFe.NFe` · Deriva de [`ParametroWS<RetornoAutorizacao>`](opendfe.md#parametrowstretorno)

A recuperação da autorização cuja resposta se perdeu (PROJETO.md 8.6, docs/CONTRATOS.md 15): consulta a chave da nota assinada (`NFeConsultaProtocolo4`) e, achado o protocolo de autorização ou de denegação daquele XML, monta o `nfeProc`. Não reenvia nada.

O retorno é o da [`Autorizacao`](#autorizacao), para quem usa tratar os dois com o mesmo código: o [`Status`](#retornoautorizacao) é o da consulta (100 ou 150 autorizada, 217 a nota não consta: reenviar o mesmo XML), sem recibo, e uma [`ResultadoNota`](#resultadonota) com o XML, o protocolo e o `nfeProc`. O `nfeProc` só sai se o `digVal` do protocolo é o `DigestValue` do XML (decisão 8): diferente, a base tem outro XML com a mesma chave, e o protocolo vem sem o `nfeProc`, com o aviso `NFE-PROTOCOLO`. A consulta vai ao autorizador da chave; uma nota por recuperação (decisão 7). O guia está em docs/QUANDO\_CAI.md.

### Construtores

| Construtor | Descrição |
|---|---|
| `RecuperacaoAutorizacao(string xml)` | A recuperação da nota assinada. **`ArgumentNullException`:** O XML é nulo. **`ArgumentException`:** O XML é vazio. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Xml` | `string` | get | A nota assinada. |

## ResultadoEvento

Classe selada · Namespace `OpenDFe.NFe`

O resultado de um evento do [`RetornoEvento`](#retornoevento) (o `retEvento`). Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Evento` | `Evento?` | get | O evento enviado, ligado pela chave, o tipo e a sequência (ou pela posição); nulo no que sobrou do retorno. |
| `Chave` | `ChaveAcesso?` | get | O `chNFe` do retorno. |
| `Status` | `int` | get | O `cStat` do evento: 135, 136, 155 ou a rejeição. |
| `Motivo` | `string` | get | O `xMotivo`. |
| `Descricao` | `string?` | get | O `xEvento`. |
| `Protocolo` | `string?` | get | O `nProt` do evento. |
| `Registro` | `DateTimeOffset?` | get | O `dhRegEvento`. |
| `XmlEvento` | `string?` | get | O evento assinado, como foi enviado. Tem dados do documento: guardar com cuidado, nunca em log. |
| `XmlProcessado` | `string?` | get | O `procEventoNFe` (o evento com os bytes enviados e o `retEvento`): só quando registrado; o XML distribuível. |
| `Registrado` | `bool` | get | O evento foi registrado: [`Status`](#resultadoevento) 135 (vinculado), 136 (não vinculado) ou 155 (cancelamento fora de prazo). |

## ResultadoNota

Classe selada · Namespace `OpenDFe.NFe`

Uma nota do lote no [`RetornoAutorizacao`](#retornoautorizacao): a chave, o XML enviado, o protocolo e o `nfeProc`.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Chave` | `ChaveAcesso?` | get | A chave de acesso da nota; nula quando o `chNFe` do protocolo não é uma chave. |
| `Xml` | `string?` | get | O `NFe` assinado, como foi enviado: o que se guarda até ter o protocolo (e para o reenvio). Nulo na consulta do recibo sem os XMLs. Tem dados pessoais: guardar com o cuidado do documento, nunca em log. |
| `Protocolo` | `Protocolo?` | get | O `protNFe` da nota (docs/LEITURA.md 2.3), com o `cStat` dela; nulo enquanto o lote não foi processado, ou quando a resposta não trouxe o da nota. |
| `XmlProcessado` | `string?` | get | O `nfeProc`: a nota com os bytes assinados e o `protNFe`, o XML distribuível (PROJETO.md 8.7). Só com o protocolo de autorização ou de denegação (100, 150, 110, 301, 302) e o [`Xml`](#resultadonota); senão nulo. |
| `Autorizada` | `bool` | get | O protocolo autoriza o uso: `cStat` 100 ou 150 (fora de prazo). |
| `Duplicidade` | `Duplicidade?` | get | Na rejeição por duplicidade (204 e 539), a nota que já está na SEFAZ, com a chave e o recibo do motivo (docs/CONTRATOS.md 15, decisão 10); nula nos outros. |

## ResumoEvento

Classe selada · Namespace `OpenDFe.NFe`

O resumo de um evento de NF-e (`resEvento` 1.01, NT 2014.002, 3.11.2), que o Ambiente Nacional distribui antes da manifestação. Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Orgao` | `int` | get | O código do órgão que recebeu o evento (`cOrgao`: o da UF, ou 91 para o Ambiente Nacional). |
| `Autor` | `string` | get | O CNPJ ou o CPF do autor do evento, sem máscara (a norma o chama de emitente). |
| `Chave` | `ChaveAcesso` | get | A chave de acesso da NF-e (`chNFe`). |
| `Momento` | `DateTimeOffset` | get | A data e a hora do evento (`dhEvento`). |
| `Tipo` | `TipoEvento` | get | O tipo do evento (`tpEvento`): aberto, um tipo que a enumeração não nomeia chega como o número, convertido. |
| `Sequencia` | `int` | get | O número sequencial do evento (`nSeqEvento`). |
| `Descricao` | `string` | get | A descrição do evento (`xEvento`). |
| `Recebimento` | `DateTimeOffset` | get | A data e a hora do registro do evento (`dhRecbto`). |
| `Protocolo` | `string` | get | O número do protocolo do evento (`nProt`). |

## ResumoNFe

Classe selada · Namespace `OpenDFe.NFe`

O resumo de uma NF-e (`resNFe` 1.01, NT 2014.002, 3.11.1): o que o Ambiente Nacional distribui ao destinatário antes da manifestação, com o necessário para ele se manifestar. Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Chave` | `ChaveAcesso` | get | A chave de acesso da NF-e (`chNFe`). |
| `Emitente` | `string` | get | O CNPJ (14 posições, numérico ou alfanumérico) ou o CPF (11) do emitente, sem máscara. |
| `NomeEmitente` | `string` | get | A razão social ou o nome do emitente (`xNome`). |
| `InscricaoEstadual` | `string` | get | A inscrição estadual do emitente (`IE`): os algarismos, `ISENTO` ou vazia (não contribuinte). |
| `Emissao` | `DateTimeOffset` | get | A data e a hora de emissão (`dhEmi`), com o fuso que veio. |
| `TipoNota` | `TipoNota` | get | O tipo de operação (`tpNF`): entrada ou saída. |
| `ValorTotal` | `decimal` | get | O valor total da NF-e (`vNF`). |
| `Digest` | `string?` | get | O `digVal` (o resumo da assinatura da NF-e na base do Ambiente Nacional); nulo se não veio. |
| `Recebimento` | `DateTimeOffset` | get | A data e a hora da autorização (`dhRecbto`). |
| `Protocolo` | `string` | get | O número do protocolo de autorização (`nProt`). |
| `Situacao` | `SituacaoResumo` | get | A situação da NF-e (`cSitNFe`): aberta, pode ser um número que a enumeração não nomeia. |

## RetornoAutorizacao

Classe selada · Namespace `OpenDFe.NFe`

O retorno da [`Autorizacao`](#autorizacao) e da [`ConsultaRecibo`](#consultarecibo) (o `retEnviNFe` e o `retConsReciNFe`, docs/CONTRATOS.md 14): os campos do lote e o resultado de cada nota. Imutável.

O [`Status`](#retornoautorizacao) é o do lote (103 recebido, 104 processado, 105 em processamento, 106 não encontrado, ou a rejeição do lote); o de cada nota está no [`Protocolo`](#resultadonota).

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat` do lote. |
| `Motivo` | `string` | get | O `xMotivo` do lote. |
| `Ambiente` | `Ambiente?` | get | O `tpAmb`; nulo quando não veio ou não se leu. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`. |
| `UF` | `UF?` | get | O `cUF`: a UF do autorizador que atendeu. |
| `Recebimento` | `DateTimeOffset?` | get | O `dhRecbto`. |
| `Recibo` | `string?` | get | O `nRec`: o recibo do lote (103), que se consulta com [`ConsultaRecibo`](#consultarecibo); também quando a síncrona devolve o recibo, e na consulta. |
| `TempoMedio` | `TimeSpan?` | get | O `tMed`: o tempo médio de resposta, a esperar antes de consultar o recibo. |
| `Mensagem` | `MensagemSefaz?` | get | A mensagem da SEFAZ ao emissor sobre o lote (`cMsg` e `xMsg` da consulta do recibo). |
| `Notas` | `IReadOnlyList<ResultadoNota>` | get | Uma por nota enviada, na ordem do parâmetro (ou dos [`Xmls`](#consultarecibo)); na consulta sem os XMLs, uma por `protNFe`, na ordem da resposta. Vazia na rejeição do lote. |

## RetornoCadastro

Classe selada · Namespace `OpenDFe.NFe`

O retorno da [`ConsultaCadastro`](#consultacadastro) (o `retConsCad` 2.00, MOC 7.0, Visão Geral, 5.6, Tabela 5-22, docs/CONTRATOS.md 24): o status e os contribuintes achados. Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat`: 111 (uma ocorrência), 112 (mais de uma), 257 a 265 (as rejeições) ou o do serviço (108, 109). |
| `Motivo` | `string` | get | O `xMotivo`. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`: a versão do Web Service que processou a consulta. |
| `UF` | `UF?` | get | A `UF` consultada, como o retorno a devolve; nula quando não veio ou não é uma UF (a `SU` da SUFRAMA). |
| `UFAtendimento` | `UF?` | get | O `cUF`: a UF do Web Service que atendeu a consulta; nula quando não veio ou não se leu. |
| `Consulta` | `DateTimeOffset?` | get | O `dhCons`: a data e a hora da consulta; nula quando não veio ou não se leu. |
| `Contribuintes` | `IReadOnlyList<ContribuinteCadastro>` | get | Os contribuintes achados (`infCad`), na ordem do retorno; vazia sem ocorrência. |
| `Encontrado` | `bool` | get | Achou: [`Status`](#retornocadastro) 111 (uma ocorrência) ou 112 (mais de uma). |

## RetornoDistribuicao

Classe selada · Namespace `OpenDFe.NFe`

O retorno da [`DistribuicaoDFe`](#distribuicaodfe) (o `retDistDFeInt` 1.01, docs/CONTRATOS.md 23): o status, os documentos já descompactados e a posição do NSU. Imutável.

A lib não repete a consulta, não espera e não guarda o NSU (PROJETO.md 8.6): [`TemMais`](#retornodistribuicao) e [`EsperaSugerida`](#retornodistribuicao) dizem o que a NT manda, e o laço é de quem usa (docs/DISTRIBUICAO.md).

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat`: 137 (nenhum documento), 138 (documentos), 656 (consumo indevido) ou a rejeição. |
| `Motivo` | `string` | get | O `xMotivo`. |
| `Ambiente` | `Ambiente?` | get | O `tpAmb`; nulo quando não veio ou não se leu. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`: a versão do Web Service que processou a consulta. |
| `Resposta` | `DateTimeOffset?` | get | O `dhResp`: a data e a hora da resposta. |
| `UltimoNsu` | `long?` | get | O `ultNSU`: o último NSU pesquisado, de onde a próxima consulta continua. No 656 da distribuição por NSU é o da última consulta que o Ambiente Nacional respondeu para o CNPJ ou CPF (NT 2014.002, 3.11.4). Nulo se não se lê. |
| `MaximoNsu` | `long?` | get | O `maxNSU`: o maior NSU existente no Ambiente Nacional para o CNPJ ou CPF; nulo se não se lê. |
| `Documentos` | `IReadOnlyList<DocumentoDistribuido>` | get | Os documentos da resposta, em ordem de NSU; vazia sem documento (e os `docZip` ilegíveis ficam de fora, com aviso). |
| `NenhumDocumento` | `bool` | get | Nenhum documento localizado ([`Status`](#retornodistribuicao) 137). |
| `ConsumoIndevido` | `bool` | get | O consumo indevido ([`Status`](#retornodistribuicao) 656): o CNPJ ou CPF está bloqueado por 1 hora. |
| `TemMais` | `bool` | get | Há mais a buscar: a distribuição por NSU com 138 e [`UltimoNsu`](#retornodistribuicao) menor que [`MaximoNsu`](#retornodistribuicao). Chame de novo na hora, com o [`UltimoNsu`](#retornodistribuicao) devolvido. Falso nas consultas pontuais, que respondem um documento só. |
| `EsperaSugerida` | `TimeSpan?` | get | A espera que a NT manda antes da próxima consulta (3.5 e 3.11.4): 1 hora na distribuição por NSU que não tem mais o que entregar (o 137, ou o 138 cujo [`UltimoNsu`](#retornodistribuicao) já alcançou o [`MaximoNsu`](#retornodistribuicao)) e no 656 (o tempo recomeça a cada consulta feita no bloqueio); nula nos outros, e quando há [`TemMais`](#retornodistribuicao). A lib não espera: é de quem usa. |

## RetornoEvento

Classe selada · Namespace `OpenDFe.NFe`

O retorno do [`EnvioEvento`](#envioevento) (o `retEnvEvento`): o lote e o resultado de cada evento. Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat` do lote: 128 (processado) ou a rejeição do lote. |
| `Motivo` | `string` | get | O `xMotivo` do lote. |
| `Ambiente` | `Ambiente?` | get | O `tpAmb`; nulo quando não veio ou não se leu. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`. |
| `Orgao` | `int?` | get | O `cOrgao` do retorno. |
| `Lote` | `long?` | get | O `idLote`. |
| `Eventos` | `IReadOnlyList<ResultadoEvento>` | get | Um por evento enviado, na ordem do envio (o que sobrar do retorno, no fim). Vazia na rejeição do lote. |

## RetornoInutilizacao

Classe selada · Namespace `OpenDFe.NFe`

O retorno da [`Inutilizacao`](#inutilizacao) (o `retInutNFe`). Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat`: 102 (homologada), 563 (a faixa já foi inutilizada) ou a rejeição. |
| `Motivo` | `string` | get | O `xMotivo`. |
| `Ambiente` | `Ambiente?` | get | O `tpAmb`. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`. |
| `UF` | `UF?` | get | O `cUF`: a UF que atendeu. |
| `Ano` | `int?` | get | O `ano` (2 dígitos), só no 102 e no 563. |
| `Cnpj` | `string?` | get | O `CNPJ`, só no 102 e no 563. |
| `Modelo` | `Documento?` | get | O `mod`, só no 102 e no 563. |
| `Serie` | `int?` | get | A `serie`, só no 102 e no 563. |
| `NumeroInicial` | `int?` | get | O `nNFIni`, só no 102 e no 563. |
| `NumeroFinal` | `int?` | get | O `nNFFin`, só no 102 e no 563. |
| `Recebimento` | `DateTimeOffset?` | get | O `dhRecbto`. |
| `Protocolo` | `string?` | get | O `nProt`: o da inutilização (no 563, o da anterior). |
| `XmlProcessado` | `string?` | get | O `ProcInutNFe` (o `inutNFe` com os bytes enviados e o `retInutNFe`): só quando homologada (102). |
| `Homologada` | `bool` | get | A inutilização foi homologada: [`Status`](#retornoinutilizacao) 102. |

## RetornoProtocolo

Classe selada · Namespace `OpenDFe.NFe`

O retorno da [`ConsultaProtocolo`](#consultaprotocolo) (o `retConsSitNFe`, docs/CONTRATOS.md 15): a situação da nota, o protocolo dela e os eventos. Imutável.

O [`Status`](#retornoprotocolo) é a situação da nota na base: 100 e 150 autorizada, 101 (151, 155) cancelada, 110, 301 e 302 denegada; ou 217 (não consta), 562 (outro código numérico na base), 561, 613 e as rejeições da consulta.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O `cStat`: a situação da nota, ou a rejeição da consulta. |
| `Motivo` | `string` | get | O `xMotivo`. |
| `Ambiente` | `Ambiente?` | get | O `tpAmb`; nulo quando não veio ou não se leu. |
| `VersaoAplicativo` | `string?` | get | O `verAplic`. |
| `UF` | `UF?` | get | O `cUF`: a UF do autorizador que atendeu. |
| `Recebimento` | `DateTimeOffset?` | get | O `dhRecbto`. |
| `Chave` | `ChaveAcesso?` | get | O `chNFe` da resposta; nulo quando não se lê (nas rejeições da mensagem, vem com zeros). |
| `Protocolo` | `Protocolo?` | get | O `protNFe` da autorização ou da denegação (docs/LEITURA.md 2.3), quando a nota consta. |
| `Eventos` | `IReadOnlyList<string>` | get | Cada `procEventoNFe` como veio (o cancelamento, a carta de correção), na ordem da resposta. O evento lido num tipo próprio vem na fase 6. |
| `ChaveRegistrada` | `ChaveAcesso?` | get | No 562 (a nota da base tem outro código numérico), a chave da nota da base, quando o motivo a traz (`[chNFe:...]`); senão nula. |
| `Autorizada` | `bool` | get | A nota está autorizada: [`Status`](#retornoprotocolo) 100 ou 150 (fora de prazo). |
| `Cancelada` | `bool` | get | A nota foi cancelada: [`Status`](#retornoprotocolo) 101, 151 ou 155 (fora de prazo). |

## SituacaoCadastral

Enumeração · Namespace `OpenDFe.NFe`

A situação cadastral do contribuinte (`cSit`, MOC 7.0, Tabela 5-22, GR10). Aberta: um valor que o MOC não conhece entra como o número, com aviso `NFE-VALOR`.

| Membro | Valor | Descrição |
|---|---|---|
| `NaoHabilitado` | 0 | Não habilitado (0). |
| `Habilitado` | 1 | Habilitado (1). |

## SituacaoResumo

Enumeração · Namespace `OpenDFe.NFe`

A situação da NF-e no resumo (`cSitNFe` do `resNFe`, NT 2014.002, 3.11.1). Aberta, como as outras enumerações do leiaute: um valor que a lib não conhece chega como o número, convertido.

| Membro | Valor | Descrição |
|---|---|---|
| `Autorizada` | 1 | Uso autorizado. |
| `Denegada` | 2 | Uso denegado. |
| `Cancelada` | 3 | NF-e cancelada. |

## TipoConsultaDistribuicao

Enumeração · Namespace `OpenDFe.NFe`

Qual das três consultas do `distDFeInt` a [`DistribuicaoDFe`](#distribuicaodfe) faz (NT 2014.002, 3.4.1).

| Membro | Valor | Descrição |
|---|---|---|
| `UltimoNsu` | 0 | A distribuição a partir do último NSU (`distNSU`): até 50 documentos por resposta, em ordem de NSU. |
| `Nsu` | 1 | A consulta pontual de um NSU (`consNSU`): conta no limite de 20 consultas por hora. |
| `Chave` | 2 | A consulta pontual de uma NF-e pela chave (`consChNFe`): conta no limite de 20 consultas por hora. |

## TipoDocumentoDistribuido

Enumeração · Namespace `OpenDFe.NFe`

O tipo de um documento distribuído, pelo `schema` do `docZip` (NT 2014.002, 3.2): o prefixo do nome do arquivo (`resNFe_`, `procNFe_`, `resEvento_`, `procEventoNFe_`), sem a versão.

| Membro | Valor | Descrição |
|---|---|---|
| `ResumoNFe` | 0 | O resumo da NF-e (`resNFe`), que o destinatário recebe antes de se manifestar. |
| `NFeProcessada` | 1 | A NF-e com o protocolo de autorização (`nfeProc`). |
| `ResumoEvento` | 2 | O resumo de um evento (`resEvento`). |
| `EventoProcessado` | 3 | O evento com o registro (`procEventoNFe`). |
| `Outro` | 4 | Um `schema` que a lib não conhece (o evento de outro documento, um leiaute novo): o XML vem em [`Xml`](#documentodistribuido). |

## TipoEvento

Enumeração · Namespace `OpenDFe.NFe`

O tipo de um evento (`tpEvento`, MOC 7.0, Visão Geral, 3.1): o código de 6 dígitos.

| Membro | Valor | Descrição |
|---|---|---|
| `CartaCorrecao` | 110110 | Carta de correção (110110). |
| `Cancelamento` | 110111 | Cancelamento (110111). |
| `CancelamentoSubstituicao` | 110112 | Cancelamento por substituição, da NFC-e (110112). |
| `ConfirmacaoOperacao` | 210200 | Confirmação da operação, do destinatário (210200). |
| `CienciaOperacao` | 210210 | Ciência da operação, do destinatário (210210). |
| `DesconhecimentoOperacao` | 210220 | Desconhecimento da operação, do destinatário (210220). |
| `OperacaoNaoRealizada` | 210240 | Operação não realizada, do destinatário (210240). |

## UrlConsultaNFCe

Classe selada · Namespace `OpenDFe.NFe`

A URL de consulta da NFC-e de uma UF num ambiente: uma linha da tabela do leiaute ([`UrlsConsulta`](#leiautenfe400), docs/NTS.md 4). Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `UF` | `UF` | get | A UF. |
| `Ambiente` | `Ambiente` | get | O ambiente. |
| `QrCode` | `Uri` | get | O endereço da consulta pelo QR Code, sem a query: a lib acrescenta o `?p=`. |
| `Chave` | `Uri` | get | O endereço da consulta pela chave de acesso, como vai no `urlChave`. |

## VersaoQrCode

Enumeração · Namespace `OpenDFe.NFe`

A versão do QR Code que a lib escreve na NFC-e (docs/CONTRATOS.md 16). A versão 100, a primeira, só se lê: a SEFAZ a recusa (903).

| Membro | Valor | Descrição |
|---|---|---|
| `V2` | 2 | A versão 2, com o CSC da UF: `chave|2|tpAmb|idCSC|hash`. |
| `V3` | 3 | A versão 3, sem CSC: `chave|3|tpAmb`. |

## ViaDanfce

Enumeração · Namespace `OpenDFe.NFe`

A via do DANFE NFC-e (a contingência exige duas: a do consumidor e a do estabelecimento).

| Membro | Valor | Descrição |
|---|---|---|
| `Consumidor` | 0 | A via do consumidor. |
| `Estabelecimento` | 1 | A via do estabelecimento, que fica à disposição do Fisco até a NFC-e ser autorizada. |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
