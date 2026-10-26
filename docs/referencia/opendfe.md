# Namespace OpenDFe

Os contratos do núcleo: o `DFe`, a configuração, as conexões, o certificado, a assinatura, os resultados, os problemas, a chave de acesso e os documentos. Pacote NuGet: `OpenDFe.Nucleo`.

> **Observação**
> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.

[Voltar à referência da API](index.md)

## Tipos

| Tipo | Descrição |
|---|---|
| [`Ambiente`](#ambiente) | Ambiente de emissão. O valor de cada membro é o que vai no XML (`tpAmb`). |
| [`AssinaturaDigital`](#assinaturadigital) | Assinatura XMLDSig dos documentos fiscais, no padrão da SEFAZ, e a verificação da assinatura de um XML recebido (PROJETO.md 7.2). |
| [`Atendimento`](#atendimento) | Qual autorizador atende uma UF num tipo de emissão (MG, normal → MG; AC, normal → SVRS; MG, SVC-AN → SVC-AN), como o leiaute declara ([`Atendimentos`](#dfe)). Criado pelo núcleo. |
| [`CampoVerificado`](#campoverificado) | Um campo, como foi escrito no XML, entregue ao callback [`Campo`](#validacaocallback). Criado pelo núcleo. |
| [`CertificadoDigital`](#certificadodigital) | Utilitários do certificado digital A1 (PFX): abrir, conferir a validade e ler o titular (PROJETO.md 7.3). |
| [`ChaveAcesso`](#chaveacesso) | Chave de acesso do documento fiscal eletrônico: as 44 posições que o identificam (MOC 7.0, Visão Geral, 2.2.6; NT 2025.001, item 5). Imutável; duas chaves são iguais quando as 44 posições são iguais. |
| [`Cnpj`](#cnpj) | CNPJ, numérico ou alfanumérico (regra de 2026): validação, formatação e dígitos verificadores. |
| [`ConexaoSefaz`](#conexaosefaz) | O que a fábrica de handlers do [`ConexoesSefaz`](#conexoessefaz) recebe: o certificado do TLS mútuo e as cifras do autorizador. O pool chama a fábrica uma vez para cada certificado e política de cifras (PROJETO.md 8.5). Criada pelo núcleo. |
| [`ConexoesSefaz`](#conexoessefaz) | O pool de conexões com os autorizadores (PROJETO.md 8.5): um `HttpMessageHandler` por certificado e política de cifras, que transmite em SOAP 1.2 com TLS mútuo. Um por processo (ou por contêiner de injeção de dependência), compartilhado por todos os núcleos ([`Conexoes`](#configuracaodfe)). |
| [`ConfiguracaoDFe`](#configuracaodfe) | A configuração de um núcleo ([`Criar`](#dfe), PROJETO.md 3.2): o ambiente, a UF do emitente, o certificado, as conexões, o tipo de emissão da nota nova, as URLs trocadas, a parte do documento, os callbacks de validação, as regras rebaixadas a aviso e o relógio. |
| [`ConfiguracaoDocumento`](#configuracaodocumento) | A base da parte da configuração que é de um documento (o CSC da NFC-e). O leiaute deriva o tipo (`ConfiguracaoNFCe`) e o declara ([`DeclararConfiguracaoDocumento`](#declaracaoleiaute)); [`Criar`](#dfe) confere que a configuração recebida é dele. |
| [`ConsultaStatus`](#consultastatus) | O status do serviço do autorizador (PROJETO.md 8.3): se ele está operando, paralisado por um tempo ou sem previsão. Operação comum aos documentos (PROJETO.md 3.4): cada leiaute registra o seu handler, e `dfe.ExecutarAsync(new ConsultaStatus())` devolve o [`RetornoStatus`](#retornostatus). |
| [`ContextoOperacao`](#contextooperacao) | O que o handler de uma operação ([`IOperacao`](#ioperacaotparametro-tretorno)) enxerga do núcleo. Criado pelo núcleo. |
| [`Contingencia`](#contingencia) | A entrada em contingência: o início (`dhCont`) e a justificativa (`xJust`) de toda nota emitida fora do tipo de emissão [`Normal`](#tipoemissao) ([`Contingencia`](#configuracaodfe)). |
| [`Cpf`](#cpf) | CPF: validação, formatação e dígitos verificadores. |
| [`DFe`](#dfe) | O núcleo de um leiaute, um documento e uma configuração (PROJETO.md 3.2): a porta das operações ([`ExecutarAsync`](#dfe)) e a listagem do que o leiaute declara. |
| [`DeclaracaoLeiaute`](#declaracaoleiaute) | O que um leiaute declara para um documento: as operações, os atendimentos (UF × tipo de emissão → autorizador), as URLs (autorizador × ambiente × serviço), os serviços SOAP, as cifras de cada autorizador, a configuração do documento, as formas de leitura e as regras. Criada pelo núcleo, que a passa a [`Declarar`](#ileiaute). |
| [`Destino`](#destino) | Para onde vai uma operação: uma UF (o autorizador sai do [`Atendimento`](#atendimento) que o leiaute declara) ou um autorizador (o Ambiente Nacional e os autorizadores virtuais). |
| [`Destinos`](#destinos) | Conversões do [`Destino`](#destino) de e para a [`UF`](#uf) e a sigla. |
| [`DeteccaoDFe`](#deteccaodfe) | O que [`Detectar`](#dfe) achou num XML: o leiaute que o lê, o documento, a raiz e a versão. Criado pelo núcleo. |
| [`Documento`](#documento) | Documento fiscal eletrônico. O valor de cada membro é o modelo, o que vai no XML (`mod`). |
| [`DocumentoMontado`](#documentomontado) | O documento montado pelo [`IPreparo`](#ipreparotdocumento), sem assinar: o XML, o schema dele e o documento que o núcleo assina e entrega às regras e aos callbacks. |
| [`DocumentoPedido`](#documentopedido) | Um documento dentro da mensagem do pedido ([`Documentos`](#pedidows)): a nota do lote. O núcleo o acha no XML assinado pelo [`Id`](#documentopedido) e o entrega às regras do leiaute ([`DeclararRegras`](#declaracaoleiaute)) e aos callbacks de quem usa ([`Validacao`](#configuracaodfe)), como um [`DocumentoVerificado`](#documentoverificado). |
| [`DocumentoPreparado`](#documentopreparado) | O resultado do [`Preparar`](#dfe): o documento assinado e validado, pronto para transmitir com os mesmos bytes (PROJETO.md 8.3), ou os problemas que o impedem. Imutável. |
| [`DocumentoVerificado`](#documentoverificado) | Um documento inteiro (cada nota do lote), entregue ao callback [`Documento`](#validacaocallback). Criado pelo núcleo. |
| [`EnderecoServico`](#enderecoservico) | A URL de um serviço (`"NFeStatusServico4"`) num autorizador e num ambiente. O leiaute declara as padrão ([`DeclararEndereco`](#declaracaoleiaute)); quem usa troca as que mudaram ([`Urls`](#configuracaodfe)). |
| [`FacetaSchema`](#facetaschema) | A restrição de um tipo simples do schema (a faceta do XSD) que um valor violou. |
| [`FalhaSchema`](#falhaschema) | O detalhe de um problema da validação pelo schema ([`Schema`](#problema)): o que falhou, as regras aplicadas, o valor encontrado e a posição no XML. |
| [`FalhaTransmissaoException`](#falhatransmissaoexception) | Falha de infraestrutura depois de montado e validado o pedido: rede, TLS, tempo esgotado, HTTP diferente de 200, falha SOAP, resposta ilegível (PROJETO.md 2.4). A espécie está em [`Tipo`](#falhatransmissaoexception); a causa, em `InnerException`. Criada pelo núcleo. |
| [`FormaLeitura`](#formaleitura) | Uma forma de XML que o leiaute lê, para um documento (PROJETO.md 3.3): a raiz e onde estão a versão e o modelo. O leiaute a declara em [`DeclararLeitura`](#declaracaoleiaute), e [`Detectar`](#dfe) a confere. |
| [`FormatadorInvariavel`](#formatadorinvariavel) | Formata os valores como os leiautes os escrevem no XML, sempre na cultura invariável: nada depende de `CurrentCulture`. |
| [`ILeiaute`](#ileiaute) | Um documento numa versão do leiaute (PROJETO.md 3.1): o que um leiaute implementa para o núcleo. Tudo o que ele tem (operações, atendimentos, URLs, a configuração do documento) é declarado em [`Declarar`](#ileiaute). |
| [`IOperacao<TParametro, TRetorno>`](#ioperacaotparametro-tretorno) | O handler de uma operação, no leiaute: monta o pedido e lê o retorno. Registrado por [`RegistrarOperacao`](#declaracaoleiaute). |
| [`IPreparo<TDocumento>`](#ipreparotdocumento) | O preparo de um documento, no leiaute: monta o XML que o [`Preparar`](#dfe) assina e valida sem transmitir (a nota preparada, a NFC-e offline). Registrado por [`RegistrarPreparo`](#declaracaoleiaute). |
| [`LeituraWS<TRetorno>`](#leiturawstretorno) | O retorno lido por [`Ler`](#ioperacaotparametro-tretorno): o objeto do leiaute, o `cStat`, o motivo e os problemas da leitura. |
| [`OperacaoSuportada`](#operacaosuportada) | Uma operação que o leiaute registrou ([`Operacoes`](#dfe)): o tipo do parâmetro e o do retorno. Criada pelo núcleo. |
| [`ParametroWS`](#parametrows) | A base de todo parâmetro de operação. Só [`ParametroWS`](#parametrowstretorno) deriva dela: existe para a restrição de [`Suporta`](#dfe). |
| [`ParametroWS<TRetorno>`](#parametrowstretorno) | O parâmetro de uma operação que devolve `TRetorno` (`DFe.ExecutarAsync(new ConsultaStatus())` é `Task<ResultadoWS<RetornoStatus>>`). Os leiautes derivam classes seladas, uma por operação. |
| [`PedidoWS`](#pedidows) | O pedido montado por [`Montar`](#ioperacaotparametro-tretorno): a mensagem, o serviço, o schema, o que assinar, o destino e os problemas da montagem. |
| [`Problema`](#problema) | Um problema encontrado na validação (schema, regras do leiaute ou de quem usa) ou na leitura de um documento. Problema não é exceção: volta como resultado. |
| [`RastroWS`](#rastrows) | O que foi enviado e recebido numa operação, para quem quiser guardar (PROJETO.md 8.7): no [`Rastro`](#resultadowstretorno) e na [`Rastro`](#falhatransmissaoexception). Criado pelo núcleo. |
| [`RestricaoSchema`](#restricaoschema) | Uma restrição de um tipo simples do schema que o valor violou ([`Restricoes`](#falhaschema)). |
| [`ResultadoWS<TRetorno>`](#resultadowstretorno) | O resultado de uma operação ([`ExecutarAsync`](#dfe)): processada pelo autorizador (com o `cStat`, inclusive a rejeição) ou recusada localmente, com os problemas e o rastro. Criado pelo núcleo. |
| [`RetornoStatus`](#retornostatus) | O retorno do status do serviço ([`ConsultaStatus`](#consultastatus)): o `cStat`, o motivo e os outros campos da resposta, comuns aos documentos. |
| [`SchemaXml`](#schemaxml) | O schema XSD de uma mensagem (a nota, o lote, o pedido de evento), carregado dos recursos embutidos de um assembly e compilado uma vez, e a validação de um XML por ele (PROJETO.md 6.1 e 6.4). |
| [`ServicoSoap`](#servicosoap) | O que o envelope SOAP 1.2 de um serviço leva (PROJETO.md 8.2): o namespace do WSDL, que é o do `nfeDadosMsg` e do `nfeResultMsg`, a ação, no `Content-Type`, e, no serviço que a tem no `Body`, a operação ([`Operacao`](#servicosoap)). O leiaute declara um por serviço ([`DeclararServico`](#declaracaoleiaute)); o núcleo não conhece os serviços de nenhum documento. |
| [`Severidade`](#severidade) | Severidade de um [`Problema`](#problema). |
| [`SituacaoResultado`](#situacaoresultado) | O que aconteceu com uma operação ([`Situacao`](#resultadowstretorno)). |
| [`TagAttribute`](#tagattribute) | A tag do XML e o identificador do campo no manual (MOC) que uma propriedade do modelo representa (PROJETO.md 4.1): `[Tag("xNome", "E04")]` em `Destinatario.Nome`. |
| [`TipoEmissao`](#tipoemissao) | Tipo (forma) de emissão do documento. O valor de cada membro é o que vai no XML (`tpEmis`). |
| [`TipoFalhaSchema`](#tipofalhaschema) | O que falhou na validação pelo schema ([`Tipo`](#falhaschema)). |
| [`TipoFalhaTransmissao`](#tipofalhatransmissao) | A espécie de uma [`FalhaTransmissaoException`](#falhatransmissaoexception): para quem chama decidir se repete (a conexão e o tempo esgotado) ou se tem algo a corrigir (o certificado recusado). |
| [`TitularCertificado`](#titularcertificado) | O titular de um certificado digital: o nome e o CNPJ (e-CNPJ) ou o CPF (e-CPF), lidos por [`LerTitular`](#certificadodigital). Exatamente um entre [`Cnpj`](#titularcertificado) e [`Cpf`](#titularcertificado) está preenchido. |
| [`UF`](#uf) | Unidade federativa do Brasil. O valor de cada membro é o código IBGE da UF, o mesmo que vai no XML (`cUF`); o nome é a sigla. |
| [`UnidadesFederativas`](#unidadesfederativas) | Conversões da [`UF`](#uf) de e para a sigla e o código IBGE. |
| [`ValidacaoCallback`](#validacaocallback) | As regras de negócio de quem usa, na configuração ([`Validacao`](#configuracaodfe), PROJETO.md 6.3): um callback por campo escrito e um por documento inteiro. Cada um devolve os problemas encontrados (vazio = nenhum), com o código e a severidade de quem usa. |
| [`ValidadeCertificado`](#validadecertificado) | A situação de um certificado num instante, pelo período de validade (`notBefore` e `notAfter`). |

## Ambiente

Enumeração · Namespace `OpenDFe`

Ambiente de emissão. O valor de cada membro é o que vai no XML (`tpAmb`).

| Membro | Valor | Descrição |
|---|---|---|
| `Producao` | 1 | Produção (1): o documento tem validade jurídica. |
| `Homologacao` | 2 | Homologação (2): testes, sem validade jurídica. |

## AssinaturaDigital

Classe estática · Namespace `OpenDFe`

Assinatura XMLDSig dos documentos fiscais, no padrão da SEFAZ, e a verificação da assinatura de um XML recebido (PROJETO.md 7.2).

O padrão (MOC 7.0, Visão Geral, 4.2.4): assinatura *enveloped* do elemento identificado pelo atributo `Id` (`infNFe`, `infEvento`, `infInut`...), com a canonicalização C14N (`http://www.w3.org/TR/2001/REC-xml-c14n-20010315`), RSA com SHA-1, uma única `Reference` com `URI="#<Id>"`, as transformações *enveloped-signature* e C14N, o resumo SHA-1 e o `KeyInfo` só com o certificado do assinante. A `Signature` é o último filho do pai do elemento assinado (`NFe`, `evento`, `inutNFe`), com o namespace do XMLDSig declarado nela. O XML entra e sai como texto, e nada fora da assinatura é alterado: [`Assinar`](#assinaturadigital) devolve o texto recebido com a `Signature` inserida, e [`Verificar`](#assinaturadigital) não altera nada. O espaço em branco é preservado, porque faz parte do que é assinado; gravar ou transmitir o XML assinado exige os mesmos caracteres (em UTF-8), sem reformatar. O texto não começa pelo BOM, que é da codificação dos bytes: `StreamReader` e `ReadAllText` o tiram, e `Encoding.UTF8.GetString` não. No Linux, a assinatura e a verificação com SHA-1 dependem da política de criptografia da máquina (PROJETO.md 2.8).

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Assinar(string xml, string id, X509Certificate2 certificado)` | `string` | Assina o elemento identificado pelo `Id`, no padrão da SEFAZ. **Retorna:** O XML recebido, sem nenhuma outra mudança, com a `Signature` inserida como último filho do pai do elemento assinado. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** O `Id` está vazio, não aparece no XML ou aparece mais de uma vez; o elemento é a raiz do XML (a assinatura vai no pai dele); o pai já tem uma `Signature`. **`XmlException`:** O XML não é bem formado ou tem DTD. **`CryptographicException`:** O certificado não tem a chave privada, a chave não é RSA, ou a assinatura falhou (no Linux, a política de criptografia que recusa SHA-1). |
| `static Verificar(string xml, string id, string cnpjOuCpf)` | `IReadOnlyList<Problema>` | Verifica a assinatura de um elemento de um XML recebido: o padrão da SEFAZ, a referência ao elemento, o valor da assinatura e o titular do certificado. **Retorna:** Os problemas encontrados, todos de severidade [`Erro`](#severidade) e com o código da rejeição da SEFAZ (MOC 7.0, Anexo I, grupos E e F); vazio se a assinatura é válida. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** O `Id` está vazio, ou o documento não é um CNPJ nem um CPF válido. **`XmlException`:** O XML não é bem formado ou tem DTD. |

## Atendimento

Classe selada · Namespace `OpenDFe`

Qual autorizador atende uma UF num tipo de emissão (MG, normal → MG; AC, normal → SVRS; MG, SVC-AN → SVC-AN), como o leiaute declara ([`Atendimentos`](#dfe)). Criado pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `UF` | `UF` | get | A UF. |
| `Emissao` | `TipoEmissao` | get | O tipo de emissão. |
| `Autorizador` | `Destino` | get | O autorizador: a própria UF, ou um virtual (SVRS, SVAN, SVC-AN, SVC-RS). |

## CampoVerificado

Classe selada · Namespace `OpenDFe`

Um campo, como foi escrito no XML, entregue ao callback [`Campo`](#validacaocallback). Criado pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Documento` | `Documento` | get | O documento (NF-e, NFC-e). |
| `Tag` | `string` | get | A tag do campo (`"xProd"`). |
| `Caminho` | `string` | get | O caminho do campo no XML (`"NFe/infNFe/det[3]/prod/xProd"`), como o [`Caminho`](#problema) da validação pelo schema. |
| `Propriedade` | `string?` | get | A propriedade do modelo (`"Itens[2].Produto.Descricao"`), quando o leiaute a conhece. |
| `Texto` | `string` | get | O valor como está no XML. Pode ser dado pessoal. |

## CertificadoDigital

Classe estática · Namespace `OpenDFe`

Utilitários do certificado digital A1 (PFX): abrir, conferir a validade e ler o titular (PROJETO.md 7.3).

A lib trabalha com o `X509Certificate2` com a chave privada RSA; de onde ele vem é de quem usa. [`AbrirPfx`](#certificadodigital) e [`AbrirArquivoPfx`](#certificadodigital) abrem o PFX; o certificado do repositório do Windows (inclusive o A3, pelo driver do cartão ou token) é usado como já vem, e [`LerTitular`](#certificadodigital) e [`Validade`](#certificadodigital) valem para qualquer `X509Certificate2`. Nada aqui confere a cadeia do certificado: abrir e ler o titular funcionam com qualquer certificado, inclusive o autoassinado dos testes. A cadeia ICP-Brasil é conferida por quem usa o certificado (o TLS e a verificação da assinatura).

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static AbrirArquivoPfx(string caminho, string senha)` | `X509Certificate2` | Abre um certificado A1 a partir de um arquivo PFX (PKCS#12). **Retorna:** O certificado com a chave privada RSA, de quem chama (ver [`AbrirPfx`](#certificadodigital)). **`ArgumentNullException`:** A senha é nula. **`ArgumentException`:** O caminho é nulo ou vazio. **`IOException`:** O arquivo não existe ou não pode ser lido. **`UnauthorizedAccessException`:** Sem permissão para ler o arquivo. **`CryptographicException`:** Os mesmos casos de [`AbrirPfx`](#certificadodigital). |
| `static AbrirPfx(byte[] conteudo, string senha)` | `X509Certificate2` | Abre um certificado A1 a partir do conteúdo de um PFX (PKCS#12). **Retorna:** O certificado com a chave privada RSA. É de quem chama: descarte-o (`Dispose`) quando não for mais usado; no Windows, a chave fica num contêiner temporário do perfil do usuário até o descarte. **`ArgumentNullException`:** O conteúdo ou a senha são nulos. **`CryptographicException`:** O conteúdo não é um PFX; a senha está errada (ou o PFX está corrompido); o PFX não tem a chave privada; a chave não é RSA. A mensagem diz qual. |
| `static LerTitular(X509Certificate2 certificado)` | `TitularCertificado?` | O titular do certificado: o CNPJ (e-CNPJ) ou o CPF (e-CPF) pelos campos da ICP-Brasil, e o nome. **Retorna:** O titular, ou `null` se o certificado não tem um CNPJ nem um CPF válido nos campos da ICP-Brasil. **`ArgumentNullException`:** O certificado é nulo. |
| `static Validade(X509Certificate2 certificado, DateTimeOffset instante)` | `ValidadeCertificado` | A situação do certificado num instante, pelo período de validade. **Retorna:** Se o certificado está vigente, ainda não vigente ou vencido no instante. **`ArgumentNullException`:** O certificado é nulo. |

## ChaveAcesso

Classe selada · Namespace `OpenDFe` · Deriva de `IEquatable<ChaveAcesso>`

Chave de acesso do documento fiscal eletrônico: as 44 posições que o identificam (MOC 7.0, Visão Geral, 2.2.6; NT 2025.001, item 5). Imutável; duas chaves são iguais quando as 44 posições são iguais.

A composição, da esquerda para a direita: código IBGE da UF do emitente (2), ano e mês da emissão, `AAMM` (4), CNPJ ou CPF do emitente (14), modelo (2), série (3), número (9), tipo de emissão (1), código numérico, `cNF` (8), e o dígito verificador (1). O CNPJ pode ser alfanumérico (NT 2025.001): as 12 primeiras posições do CNPJ aceitam letras maiúsculas, e todas as outras posições da chave são dígitos. O emitente pessoa física (produtor rural, NT 2018.001 e NT 2023.002) entra com o CPF precedido de três zeros, completando as 14 posições; a chave sozinha não diz se o emitente é CPF ou CNPJ (quem diz é a série e o XML), por isso [`Emitente`](#chaveacesso) devolve as 14 posições como estão. O dígito verificador é o módulo 11 das 43 primeiras posições, com os pesos de 2 a 9 da direita para a esquerda e cada caractere valendo o código ASCII menos 48 (o mesmo cálculo do CNPJ alfanumérico).

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `Tamanho` | `int` | 44 | Quantidade de posições da chave de acesso. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `UF` | `UF` | get | A UF do emitente (`cUF`). |
| `Ano` | `int` | get | O ano da emissão, com quatro dígitos (a chave leva os dois últimos: `26` é 2026). |
| `Mes` | `int` | get | O mês da emissão, de 1 a 12. |
| `Emitente` | `string` | get | O CNPJ ou o CPF do emitente como está na chave: 14 posições, o CNPJ sem máscara (talvez alfanumérico) ou o CPF precedido de três zeros (`00052998224725`). |
| `Modelo` | `Documento` | get | O modelo do documento (`mod`). |
| `Serie` | `int` | get | A série (`serie`), de 0 a 999. |
| `Numero` | `int` | get | O número do documento (`nNF`), de 1 a 999.999.999. |
| `TipoEmissao` | `TipoEmissao` | get | O tipo de emissão (`tpEmis`). |
| `CodigoNumerico` | `int` | get | O código numérico (`cNF`), de 0 a 99.999.999; no XML, com 8 dígitos. |
| `DigitoVerificador` | `int` | get | O dígito verificador (`cDV`), de 0 a 9. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Equals(ChaveAcesso? other)` | `bool` |  |
| `Equals(object? obj)` | `bool` |  |
| `static Gerar(UF uf, DateTimeOffset emissao, string emitente, Documento modelo, int serie, int numero, TipoEmissao tipoEmissao)` | `ChaveAcesso` | Gera a chave de um documento novo, com o código numérico (`cNF`) sorteado por `RandomNumberGenerator`: nunca igual ao número do documento, nem uma das sequências que a SEFAZ recusa (`00000000`, `11111111`, ..., `12345678`, ..., `01234567`; MOC 7.0, regra B09-20, rejeição 897). **Retorna:** A chave de acesso. **`ArgumentException`:** O emitente não é um CNPJ nem um CPF válido. **`ArgumentOutOfRangeException`:** A UF, o modelo ou o tipo de emissão não são valores conhecidos; o ano da emissão está fora de 2000 a 2099; a série ou o número estão fora da faixa. |
| `GetHashCode()` | `int` |  |
| `static Ler(string valor)` | `ChaveAcesso` | Lê uma chave de acesso que se sabe válida; as regras são as de [`TentarLer`](#chaveacesso). **Retorna:** A chave lida. **`ArgumentException`:** O valor não é uma chave de acesso válida. |
| `static Montar(UF uf, DateTimeOffset emissao, string emitente, Documento modelo, int serie, int numero, TipoEmissao tipoEmissao, int codigoNumerico)` | `ChaveAcesso` | Monta a chave com um código numérico conhecido (o `cNF` de um XML, por exemplo) e calcula o dígito verificador. Não confere as regras do código numérico ([`Gerar`](#chaveacesso) gera um aceito): essas são regras do MOC, conferidas na validação do documento. **Retorna:** A chave de acesso. **`ArgumentException`:** O emitente não é um CNPJ nem um CPF válido. **`ArgumentOutOfRangeException`:** A UF, o modelo ou o tipo de emissão não são valores conhecidos; o ano da emissão está fora de 2000 a 2099; a série, o número ou o código numérico estão fora da faixa. |
| `static TentarLer(string? valor, out ChaveAcesso? chave)` | `bool` | Lê uma chave de acesso: 44 posições, sem espaços nem prefixo (o `NFe` do atributo `Id` fica de fora). **Retorna:** Se o valor é uma chave de acesso válida. |
| `ToString()` | `string` | As 44 posições da chave, como vão no XML. **Retorna:** A chave. |
| `static operator ==(ChaveAcesso? esquerda, ChaveAcesso? direita)` | `bool` | Se as duas chaves têm as mesmas 44 posições. **Retorna:** Se são iguais. |
| `static operator !=(ChaveAcesso? esquerda, ChaveAcesso? direita)` | `bool` | Se as duas chaves diferem em alguma posição. **Retorna:** Se são diferentes. |

## Cnpj

Classe estática · Namespace `OpenDFe`

CNPJ, numérico ou alfanumérico (regra de 2026): validação, formatação e dígitos verificadores.

O CNPJ tem 14 posições: as 12 primeiras (raiz e ordem) são dígitos ou letras maiúsculas, e as duas últimas, os dígitos verificadores, sempre numéricos. Os dígitos verificadores são calculados por módulo 11, com cada caractere valendo o código ASCII menos 48 (no CNPJ numérico, o resultado é o da regra antiga). Aceito sem máscara (`12ABC34501DE35`) ou com a máscara inteira (`12.ABC.345/01DE-35`); letras minúsculas não são aceitas, nem as 14 posições iguais.

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `Tamanho` | `int` | 14 | Quantidade de posições do CNPJ sem máscara. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static CalcularDigitos(string base)` | `string` | Os dois dígitos verificadores da base (raiz e ordem) de um CNPJ. **Retorna:** Os dois dígitos verificadores (`"35"` para `12ABC34501DE`). **`ArgumentException`:** A base não tem 12 posições, dígitos ou letras maiúsculas. |
| `static Formatar(string valor)` | `string` | O CNPJ com a máscara (`12.ABC.345/01DE-35`). **Retorna:** O CNPJ com a máscara. **`ArgumentException`:** O valor não é um CNPJ válido. |
| `static RemoverMascara(string valor)` | `string` | O CNPJ sem máscara (`12ABC34501DE35`), como vai no XML. **Retorna:** As 14 posições, sem máscara. **`ArgumentException`:** O valor não é um CNPJ válido. |
| `static Valido(string? valor)` | `bool` | Se o valor é um CNPJ válido, com ou sem máscara. **Retorna:** Se o formato e os dígitos verificadores estão certos. |

## ConexaoSefaz

Classe selada · Namespace `OpenDFe`

O que a fábrica de handlers do [`ConexoesSefaz`](#conexoessefaz) recebe: o certificado do TLS mútuo e as cifras do autorizador. O pool chama a fábrica uma vez para cada certificado e política de cifras (PROJETO.md 8.5). Criada pelo núcleo.

[`CriarPadrao`](#conexaosefaz) devolve o handler que a lib usaria; a fábrica de quem usa o ajusta (outra validação do servidor, um proxy, as intermediárias do certificado) ou monta outro.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Certificado` | `X509Certificate2` | get | O certificado do cliente no TLS mútuo: uma cópia do pool, que vive enquanto o handler viver (quem usa pode descartar o seu). Não o descarte. |
| `Cifras` | `IReadOnlyList<TlsCipherSuite>?` | get | As cifras que o leiaute declara para o autorizador ([`DeclararCifras`](#declaracaoleiaute)); `null` = o padrão da máquina. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `CriarPadrao()` | `SocketsHttpHandler` | O handler da lib: TLS 1.2 e 1.3, o certificado do cliente no handshake (sem buscar as intermediárias na rede), o servidor aceito pelo sistema ou pelas raízes ICP-Brasil embutidas, sem conferir a revogação, as [`Cifras`](#conexaosefaz) (só no Linux: o .NET não as aplica no Windows), o [`TempoConexao`](#conexoessefaz), sem redirecionamento nem cookies. **Retorna:** Um handler novo, que é de quem o recebe. |

## ConexoesSefaz

Classe selada · Namespace `OpenDFe` · Deriva de `IDisposable`

O pool de conexões com os autorizadores (PROJETO.md 8.5): um `HttpMessageHandler` por certificado e política de cifras, que transmite em SOAP 1.2 com TLS mútuo. Um por processo (ou por contêiner de injeção de dependência), compartilhado por todos os núcleos ([`Conexoes`](#configuracaodfe)).

É de quem cria, que o descarta depois do último núcleo: o núcleo só o referencia. Descartá-lo com um núcleo em uso é erro de quem chama (`ObjectDisposedException` na chamada seguinte). Seguro para uso concorrente. O handler é criado na primeira transmissão de cada certificado (pela impressão SHA-256) e descartado depois de [`TempoOcioso`](#conexoessefaz) sem uso, conferido a cada transmissão, sem timer; um handler em uso nunca é descartado.

### Construtores

| Construtor | Descrição |
|---|---|
| `ConexoesSefaz()` | Cria o pool, vazio, com o handler da lib ([`CriarPadrao`](#conexaosefaz)). |
| `ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler> fabrica)` | Cria o pool, vazio, com a fábrica de handlers de quem usa (PROJETO.md 2.3). **`ArgumentNullException`:** A fábrica é nula. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `TempoConexao` | `TimeSpan` | get; init | O tempo para abrir a conexão, com o handshake; o padrão é 15 segundos. |
| `TempoResposta` | `TimeSpan` | get; init | O tempo de cada transmissão, do envio ao fim da resposta (com a conexão); o padrão é 60 segundos. Esgotado, [`FalhaTransmissaoException`](#falhatransmissaoexception) com [`TempoEsgotado`](#tipofalhatransmissao). |
| `TempoOcioso` | `TimeSpan` | get; init | Depois de quanto tempo sem uso o handler de um certificado é descartado; o padrão é 30 minutos. No ERP com muitos certificados, é o que libera os que não transmitem mais. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Dispose()` | `void` | Fecha as conexões e descarta os handlers. Chamar de novo não faz nada. |

## ConfiguracaoDFe

Classe selada · Namespace `OpenDFe`

A configuração de um núcleo ([`Criar`](#dfe), PROJETO.md 3.2): o ambiente, a UF do emitente, o certificado, as conexões, o tipo de emissão da nota nova, as URLs trocadas, a parte do documento, os callbacks de validação, as regras rebaixadas a aviso e o relógio.

[`Criar`](#dfe) confere a configuração e tira uma cópia das listas: mudar a lista depois não muda o núcleo. Outra configuração (entrar em contingência, outro certificado) é outro núcleo, com as mesmas [`Conexoes`](#configuracaodfe).

### Construtores

| Construtor | Descrição |
|---|---|
| `ConfiguracaoDFe()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Ambiente` | `Ambiente` | get; init (obrigatória) | O ambiente (homologação ou produção). |
| `UF` | `UF` | get; init (obrigatória) | A UF do emitente: o destino natural das operações sem documento nem chave (PROJETO.md 8.4). |
| `Certificado` | `X509Certificate2` | get; init (obrigatória) | O certificado A1 ou A3 com a chave privada RSA. É de quem chama: o núcleo não o descarta nem o copia, e descartá-lo com o núcleo em uso é erro de quem chama. |
| `Conexoes` | `ConexoesSefaz` | get; init (obrigatória) | O pool de conexões, compartilhado entre os núcleos (um por processo). |
| `Emissao` | `TipoEmissao` | get; init | O tipo de emissão da nota nova (PROJETO.md 8.4); o padrão é [`Normal`](#tipoemissao). Diferente de [`Normal`](#tipoemissao) exige a [`Contingencia`](#configuracaodfe). |
| `Contingencia` | `Contingencia?` | get; init | A entrada em contingência, obrigatória quando [`Emissao`](#configuracaodfe) não é [`Normal`](#tipoemissao) (e recusada quando é). |
| `Urls` | `IReadOnlyList<EnderecoServico>` | get; init | Só as URLs que mudam: cada uma substitui a do leiaute com o mesmo autorizador, ambiente e serviço, ou se acrescenta. O resultado está em [`Enderecos`](#dfe). |
| `Documento` | `ConfiguracaoDocumento?` | get; init | A parte que é do documento, do tipo que o leiaute declara (`ConfiguracaoNFCe`, com o CSC); `null` quando o leiaute não declara nenhuma ou ela é opcional. |
| `Validacao` | `ValidacaoCallback?` | get; init | Os callbacks de validação de quem usa (PROJETO.md 6.3). |
| `RegrasComoAviso` | `IReadOnlyCollection<string>` | get; init | Os códigos das regras do leiaute (`"564"`) cujo problema sai como [`Aviso`](#severidade) em vez de [`Erro`](#severidade): para seguir emitindo quando a lib aplica uma regra que o autorizador não aplica, até a versão corrigida. O problema continua no resultado. Não vale para o schema (PROJETO.md 6.1) nem para os callbacks; as regras que dividem um código são rebaixadas juntas. |
| `Relogio` | `TimeProvider` | get; init | O relógio do rastro, dos tempos e do instante que o leiaute usar (PROJETO.md 2.3); o padrão é `System`. |

## ConfiguracaoDocumento

Classe abstrata · Namespace `OpenDFe`

A base da parte da configuração que é de um documento (o CSC da NFC-e). O leiaute deriva o tipo (`ConfiguracaoNFCe`) e o declara ([`DeclararConfiguracaoDocumento`](#declaracaoleiaute)); [`Criar`](#dfe) confere que a configuração recebida é dele.

O tipo derivado deve ser imutável: o núcleo o usa em paralelo.

## ConsultaStatus

Classe selada · Namespace `OpenDFe` · Deriva de [`ParametroWS<RetornoStatus>`](#parametrowstretorno)

O status do serviço do autorizador (PROJETO.md 8.3): se ele está operando, paralisado por um tempo ou sem previsão. Operação comum aos documentos (PROJETO.md 3.4): cada leiaute registra o seu handler, e `dfe.ExecutarAsync(new ConsultaStatus())` devolve o [`RetornoStatus`](#retornostatus).

Vai ao autorizador da UF do núcleo no tipo de emissão do núcleo: o núcleo em contingência SVC consulta a SVC. O [`Destino`](#parametrows) sobrepõe: `new ConsultaStatus { Destino = Destino.SvcAN }` consulta a SVC-AN com o núcleo em emissão normal, e `Destino = Destino.SP`, o autorizador de SP. A UF consultada (o `cUF` do pedido) é a do destino, quando ele é uma UF; nos autorizadores virtuais (SVRS, SVAN, SVC), a do núcleo.

### Construtores

| Construtor | Descrição |
|---|---|
| `ConsultaStatus()` |  |

## ContextoOperacao

Classe selada · Namespace `OpenDFe`

O que o handler de uma operação ([`IOperacao`](#ioperacaotparametro-tretorno)) enxerga do núcleo. Criado pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Documento` | `Documento` | get | O documento do núcleo. |
| `Configuracao` | `ConfiguracaoDFe` | get | A configuração do núcleo. |

## Contingencia

Classe selada · Namespace `OpenDFe`

A entrada em contingência: o início (`dhCont`) e a justificativa (`xJust`) de toda nota emitida fora do tipo de emissão [`Normal`](#tipoemissao) ([`Contingencia`](#configuracaodfe)).

### Construtores

| Construtor | Descrição |
|---|---|
| `Contingencia(DateTimeOffset inicio, string justificativa)` | Cria a entrada em contingência. **`ArgumentException`:** A justificativa é nula ou vazia. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Inicio` | `DateTimeOffset` | get | O instante da entrada em contingência (`dhCont`). |
| `Justificativa` | `string` | get | A justificativa (`xJust`). |

## Cpf

Classe estática · Namespace `OpenDFe`

CPF: validação, formatação e dígitos verificadores.

Aceito sem máscara (`52998224725`) ou com a máscara inteira (`529.982.247-25`). Os números com todos os dígitos iguais (`111.111.111-11`) passam no cálculo, mas não são CPFs válidos.

### Campos

| Campo | Tipo | Valor | Descrição |
|---|---|---|---|
| `Tamanho` | `int` | 11 | Quantidade de dígitos do CPF sem máscara. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static CalcularDigitos(string base)` | `string` | Os dois dígitos verificadores da base de um CPF. **Retorna:** Os dois dígitos verificadores (`"25"` para `529982247`). **`ArgumentException`:** A base não tem 9 dígitos. |
| `static Formatar(string valor)` | `string` | O CPF com a máscara (`529.982.247-25`). **Retorna:** O CPF com a máscara. **`ArgumentException`:** O valor não é um CPF válido. |
| `static RemoverMascara(string valor)` | `string` | O CPF sem máscara (`52998224725`), como vai no XML. **Retorna:** Os 11 dígitos, sem máscara. **`ArgumentException`:** O valor não é um CPF válido. |
| `static Valido(string? valor)` | `bool` | Se o valor é um CPF válido, com ou sem máscara. **Retorna:** Se o formato e os dígitos verificadores estão certos. |

## DFe

Classe selada · Namespace `OpenDFe`

O núcleo de um leiaute, um documento e uma configuração (PROJETO.md 3.2): a porta das operações ([`ExecutarAsync`](#dfe)) e a listagem do que o leiaute declara.

Imutável e seguro para uso concorrente: um núcleo atende chamadas em paralelo. Criar é barato (o leiaute não tem estado e os schemas são dele); entrar em contingência é criar outro núcleo com as mesmas [`Conexoes`](#configuracaodfe). Não é `IDisposable`: o certificado e as conexões são de quem os cria, e o núcleo só os referencia.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Leiaute` | `ILeiaute` | get | O leiaute. |
| `Documento` | `Documento` | get | O documento. |
| `Configuracao` | `ConfiguracaoDFe` | get | A configuração recebida em [`Criar`](#dfe). |
| `Operacoes` | `IReadOnlyList<OperacaoSuportada>` | get | As operações que o leiaute registrou para o documento, na ordem do registro. |
| `Atendimentos` | `IReadOnlyList<Atendimento>` | get | Qual autorizador atende cada UF em cada tipo de emissão (a tabela do leiaute). |
| `Enderecos` | `IReadOnlyList<EnderecoServico>` | get | As URLs efetivas: as do leiaute, na ordem dele, com as trocadas na configuração no lugar; depois, as acrescentadas pela configuração. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Criar(ILeiaute leiaute, Documento documento, ConfiguracaoDFe configuracao)` | `DFe` | Cria o núcleo: chama [`Declarar`](#ileiaute) e confere a configuração contra a declaração. **Retorna:** O núcleo. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** O leiaute não atende o documento; um membro obrigatório da configuração é nulo ou tem valor desconhecido; a configuração do documento falta (quando o leiaute a declara obrigatória), é de outro tipo ou não é declarada pelo leiaute; a UF não tem atendimento no tipo de emissão; o tipo de emissão é diferente de [`Normal`](#tipoemissao) sem a [`Contingencia`](#configuracaodfe), ou é [`Normal`](#tipoemissao) com ela; uma URL trocada é nula ou repetida; a lista das regras rebaixadas a aviso é nula ou tem um código nulo ou vazio; uma URL, do leiaute ou trocada, é de um serviço sem o envelope SOAP declarado ([`DeclararServico`](#declaracaoleiaute)). **`CryptographicException`:** O certificado não tem a chave privada, ou ela não é RSA. **`ObjectDisposedException`:** As conexões já foram descartadas. |
| `static Detectar(string xml, params IEnumerable<ILeiaute> leiautes)` | `DeteccaoDFe?` | Qual dos leiautes lê o XML (PROJETO.md 3.3): o primeiro, na ordem recebida, com uma forma declarada ([`DeclararLeitura`](#declaracaoleiaute)) para um dos seus documentos que bate com a raiz, a versão e o modelo do XML. É o caminho da caixa de entrada, onde a versão só aparece dentro do XML. **Retorna:** O leiaute, o documento, a raiz e a versão; `null` quando nenhum leiaute lê o XML, inclusive quando ele não é XML bem formado. A leitura do leiaute diz o porquê, com os problemas. **`ArgumentNullException`:** O XML ou a lista são nulos, ou há um leiaute nulo na lista. |
| `ExecutarAsync<TRetorno>(ParametroWS<TRetorno> parametro, CancellationToken cancellationToken = default)` | `Task<ResultadoWS<TRetorno>>` | Executa uma operação: montar → recusa local → assinar → validar pelo schema → regras do leiaute → callbacks de quem usa → transmitir → ler. **Retorna:** O resultado: [`Processado`](#situacaoresultado), com o `cStat` (inclusive a rejeição), ou [`RecusadoLocalmente`](#situacaoresultado), sem transmitir; com os problemas e o rastro. **`ArgumentNullException`:** O parâmetro é nulo. **`NotSupportedException`:** O leiaute não registrou a operação, ou não há URL para o autorizador, o ambiente e o serviço (nem atendimento para a UF no tipo de emissão). **`FalhaTransmissaoException`:** Falha de rede, TLS, HTTP, SOAP, tempo esgotado ou resposta ilegível, com o rastro e se o pedido pode ter sido processado. **`OperationCanceledException`:** Cancelado (sem o rastro). **`ObjectDisposedException`:** As conexões já foram descartadas. |
| `Preparar<TDocumento>(TDocumento documento)` | `DocumentoPreparado` | Prepara um documento sem transmitir (PROJETO.md 8.3; a NFC-e offline, 9.2): o caminho do [`ExecutarAsync`](#dfe) até os callbacks, sem a URL nem o SOAP. **Retorna:** O documento assinado e validado ([`Preparado`](#documentopreparado)), ou os problemas que o impedem, com o XML até onde o caminho chegou. **`ArgumentNullException`:** O documento é nulo. **`NotSupportedException`:** O leiaute não registrou o preparo de `TDocumento` para o documento do núcleo. |
| `Suporta<TParametro>()` | `bool` | Se o leiaute registrou um handler para este tipo de parâmetro (o tipo exato). **Retorna:** Se a operação é suportada. |

## DeclaracaoLeiaute

Classe selada · Namespace `OpenDFe`

O que um leiaute declara para um documento: as operações, os atendimentos (UF × tipo de emissão → autorizador), as URLs (autorizador × ambiente × serviço), os serviços SOAP, as cifras de cada autorizador, a configuração do documento, as formas de leitura e as regras. Criada pelo núcleo, que a passa a [`Declarar`](#ileiaute).

Os métodos `Registrar` e `Declarar` só valem durante [`Declarar`](#ileiaute) (depois, `InvalidOperationException`); as propriedades são a listagem, na ordem da declaração. Depois do [`Declarar`](#ileiaute), a instância é imutável e segura para uso concorrente.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Documento` | `Documento` | get | O documento da declaração. |
| `Operacoes` | `IReadOnlyList<OperacaoSuportada>` | get | As operações registradas, na ordem do registro. |
| `Atendimentos` | `IReadOnlyList<Atendimento>` | get | Os atendimentos declarados, na ordem da declaração. |
| `Enderecos` | `IReadOnlyList<EnderecoServico>` | get | As URLs declaradas, na ordem da declaração. |
| `Servicos` | `IReadOnlyList<ServicoSoap>` | get | Os serviços SOAP declarados, na ordem da declaração. |
| `Cifras` | `IReadOnlyDictionary<Destino, IReadOnlyList<TlsCipherSuite>>` | get | As cifras de TLS declaradas por autorizador (só os que não usam o padrão da máquina). |
| `Leituras` | `IReadOnlyList<FormaLeitura>` | get | As formas de XML que o leiaute lê para o documento ([`Detectar`](#dfe)), na ordem da declaração. |
| `TipoConfiguracaoDocumento` | `Type?` | get | O tipo da configuração do documento (`typeof(ConfiguracaoNFCe)`); `null` se o leiaute não declara nenhuma. |
| `ConfiguracaoDocumentoObrigatoria` | `bool` | get | Se o núcleo recusa a configuração sem a parte do documento. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Criar(ILeiaute leiaute, Documento documento)` | `DeclaracaoLeiaute` | A declaração de um leiaute para um documento, sem criar o núcleo: a listagem antes de haver certificado (a tela de configuração). **Retorna:** A declaração, já fechada. **`ArgumentNullException`:** O leiaute é nulo. **`ArgumentException`:** O leiaute não atende o documento. |
| `DeclararAtendimento(UF uf, TipoEmissao emissao, Destino autorizador)` | `void` | Declara que a UF, no tipo de emissão, é atendida pelo autorizador (AC, normal → SVRS; MG, SVC-AN → SVC-AN). **`ArgumentOutOfRangeException`:** Um dos valores não é conhecido. **`ArgumentException`:** A UF já tem atendimento declarado nesse tipo de emissão. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `DeclararCifras(Destino autorizador, IEnumerable<TlsCipherSuite> cifras)` | `void` | Declara as cifras de TLS que o autorizador negocia, quando ele só aceita cifras que a máquina não oferece por padrão (PROJETO.md 8.5): o pool usa um handler próprio para ele, sem rebaixar o padrão da máquina inteira. Só vale no Linux; no Windows, o .NET não aplica a política de cifras e vale o padrão do sistema. **`ArgumentOutOfRangeException`:** O autorizador não é conhecido. **`ArgumentNullException`:** A lista é nula. **`ArgumentException`:** A lista está vazia, ou o autorizador já tem cifras declaradas. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `DeclararConfiguracaoDocumento<TConfiguracao>(bool obrigatoria)` | `void` | Declara o tipo da configuração do documento (`ConfiguracaoNFCe`). **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute), ou a configuração do documento já foi declarada. |
| `DeclararEndereco(EnderecoServico endereco)` | `void` | Declara a URL de um serviço num autorizador e num ambiente. **`ArgumentNullException`:** O endereço é nulo. **`ArgumentException`:** Já há URL declarada para o autorizador, o ambiente e o serviço. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `DeclararLeitura(FormaLeitura forma)` | `void` | Declara uma forma de XML que o leiaute lê para o documento (PROJETO.md 3.3). **`ArgumentNullException`:** A forma é nula. **`ArgumentException`:** A mesma raiz, no mesmo namespace, já foi declarada. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `DeclararRegras(Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>> regras)` | `void` | Declara as regras do leiaute (PROJETO.md 6.2) para os documentos dos pedidos ([`Documentos`](#pedidows)): o núcleo chama a função uma vez por documento, depois da validação pelo schema e só sem erro nela, com o mesmo [`DocumentoVerificado`](#documentoverificado) que os callbacks de quem usa recebem. **`ArgumentNullException`:** A função é nula. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `DeclararServico(ServicoSoap servico)` | `void` | Declara o envelope SOAP de um serviço: o namespace do WSDL e a ação (PROJETO.md 8.2). Toda URL, do leiaute ou trocada por quem usa, precisa do serviço dela declarado ([`Criar`](#dfe) confere). **`ArgumentNullException`:** O serviço é nulo. **`ArgumentException`:** O serviço, pelo nome, já foi declarado. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `RegistrarOperacao<TParametro, TRetorno>(IOperacao<TParametro, TRetorno> operacao)` | `void` | Registra o handler de uma operação, pelo tipo do parâmetro. **`ArgumentNullException`:** O handler é nulo. **`ArgumentException`:** Já há handler para `TParametro`, ou ele é abstrato. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |
| `RegistrarPreparo<TDocumento>(IPreparo<TDocumento> preparo)` | `void` | Registra o preparo de um documento ([`Preparar`](#dfe)), pelo tipo do documento. **`ArgumentNullException`:** O preparo é nulo. **`ArgumentException`:** Já há preparo para `TDocumento`. **`InvalidOperationException`:** Fora de [`Declarar`](#ileiaute). |

## Destino

Enumeração · Namespace `OpenDFe`

Para onde vai uma operação: uma UF (o autorizador sai do [`Atendimento`](#atendimento) que o leiaute declara) ou um autorizador (o Ambiente Nacional e os autorizadores virtuais).

As UFs têm o código IBGE, o mesmo valor de [`UF`](#uf); o [`AN`](#destino), o 91 do `cOrgao`. Os virtuais ([`SVRS`](#destino), [`SVAN`](#destino), [`SvcAN`](#destino), [`SvcRS`](#destino)) não têm código oficial: o valor deles nunca é escrito no XML. Conversões e siglas: [`Destinos`](#destinos).

| Membro | Valor | Descrição |
|---|---|---|
| `RO` | 11 | Rondônia (11). |
| `AC` | 12 | Acre (12). |
| `AM` | 13 | Amazonas (13). |
| `RR` | 14 | Roraima (14). |
| `PA` | 15 | Pará (15). |
| `AP` | 16 | Amapá (16). |
| `TO` | 17 | Tocantins (17). |
| `MA` | 21 | Maranhão (21). |
| `PI` | 22 | Piauí (22). |
| `CE` | 23 | Ceará (23). |
| `RN` | 24 | Rio Grande do Norte (24). |
| `PB` | 25 | Paraíba (25). |
| `PE` | 26 | Pernambuco (26). |
| `AL` | 27 | Alagoas (27). |
| `SE` | 28 | Sergipe (28). |
| `BA` | 29 | Bahia (29). |
| `MG` | 31 | Minas Gerais (31). |
| `ES` | 32 | Espírito Santo (32). |
| `RJ` | 33 | Rio de Janeiro (33). |
| `SP` | 35 | São Paulo (35). |
| `PR` | 41 | Paraná (41). |
| `SC` | 42 | Santa Catarina (42). |
| `RS` | 43 | Rio Grande do Sul (43). |
| `MS` | 50 | Mato Grosso do Sul (50). |
| `MT` | 51 | Mato Grosso (51). |
| `GO` | 52 | Goiás (52). |
| `DF` | 53 | Distrito Federal (53). |
| `AN` | 91 | Ambiente Nacional (91, o `cOrgao`): distribuição DF-e, manifestação, EPEC. |
| `SVRS` | 101 | SEFAZ Virtual do Rio Grande do Sul, SVRS (sem código oficial). |
| `SVAN` | 102 | SEFAZ Virtual do Ambiente Nacional, SVAN (sem código oficial). |
| `SvcAN` | 103 | SEFAZ Virtual de Contingência do Ambiente Nacional, SVC-AN (sem código oficial). |
| `SvcRS` | 104 | SEFAZ Virtual de Contingência do Rio Grande do Sul, SVC-RS (sem código oficial). |

## Destinos

Classe estática · Namespace `OpenDFe`

Conversões do [`Destino`](#destino) de e para a [`UF`](#uf) e a sigla.

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static ComoDestino(this UF uf)` | `Destino` | O destino da UF ([`MG`](#destino) para [`MG`](#uf)). **Retorna:** O destino com o mesmo código IBGE. **`ArgumentOutOfRangeException`:** O valor não é uma UF. |
| `static Sigla(this Destino destino)` | `string` | A sigla do destino: a da UF (`"MG"`), `"AN"`, `"SVRS"`, `"SVAN"`, `"SVC-AN"` ou `"SVC-RS"`. **Retorna:** A sigla, em maiúsculas. **`ArgumentOutOfRangeException`:** O valor não é um destino. |
| `static TentarLerSigla(string? sigla, out Destino destino)` | `bool` | Obtém o destino pela sigla, em maiúsculas (`"MG"`, `"AN"`, `"SVC-AN"`). **Retorna:** Se a sigla é de um destino. |
| `static TentarUF(this Destino destino, out UF uf)` | `bool` | A UF do destino, quando o destino é uma UF. **Retorna:** Se o destino é uma UF (não o AN nem um autorizador virtual). |

## DeteccaoDFe

Classe selada · Namespace `OpenDFe`

O que [`Detectar`](#dfe) achou num XML: o leiaute que o lê, o documento, a raiz e a versão. Criado pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Leiaute` | `ILeiaute` | get | O leiaute que lê o XML, a instância recebida pelo detector. |
| `Documento` | `Documento` | get | O documento, pelo modelo do XML. |
| `Raiz` | `string` | get | O nome local da raiz (`NFe`, `nfeProc`). |
| `Versao` | `string` | get | A versão do XML, igual à do leiaute (`4.00`). |

## Documento

Enumeração · Namespace `OpenDFe`

Documento fiscal eletrônico. O valor de cada membro é o modelo, o que vai no XML (`mod`).

| Membro | Valor | Descrição |
|---|---|---|
| `NFe` | 55 | Nota fiscal eletrônica, NF-e (modelo 55). |
| `NFCe` | 65 | Nota fiscal de consumidor eletrônica, NFC-e (modelo 65). |

## DocumentoMontado

Classe selada · Namespace `OpenDFe`

O documento montado pelo [`IPreparo`](#ipreparotdocumento), sem assinar: o XML, o schema dele e o documento que o núcleo assina e entrega às regras e aos callbacks.

### Construtores

| Construtor | Descrição |
|---|---|
| `DocumentoMontado(string xml, SchemaXml schema, DocumentoPedido documento)` | Cria o documento montado. **`ArgumentNullException`:** Um dos argumentos é nulo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Xml` | `string` | get | O XML do documento, sem a assinatura. |
| `Schema` | `SchemaXml` | get | O schema do documento. |
| `Documento` | `DocumentoPedido` | get | O documento: o `Id` assinado, o modelo, a chave e a conversão. |
| `Problemas` | `IReadOnlyList<Problema>` | get; init | Os problemas da montagem (regras da escrita, campo não suportado). Com Erro, nada é assinado. |

## DocumentoPedido

Classe selada · Namespace `OpenDFe`

Um documento dentro da mensagem do pedido ([`Documentos`](#pedidows)): a nota do lote. O núcleo o acha no XML assinado pelo [`Id`](#documentopedido) e o entrega às regras do leiaute ([`DeclararRegras`](#declaracaoleiaute)) e aos callbacks de quem usa ([`Validacao`](#configuracaodfe)), como um [`DocumentoVerificado`](#documentoverificado).

### Construtores

| Construtor | Descrição |
|---|---|
| `DocumentoPedido(string id)` | Cria o documento. **`ArgumentException`:** O `Id` é nulo ou vazio. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Id` | `string` | get | O `Id` do elemento assinado; o documento é o pai dele. |
| `Modelo` | `object?` | get; init | O modelo do leiaute (no OpenDFe.NFe, a nota); `null` quando o documento veio só como XML. |
| `Chave` | `ChaveAcesso?` | get; init | A chave de acesso, quando o documento tem uma. |
| `Propriedade` | `Func<string, string?>?` | get; init | A conversão do caminho de um campo do documento (`"NFe/infNFe/det[3]/prod/xProd"`) na propriedade do modelo (`"Itens[2].Produto.Descricao"`), ou `null` quando o caminho não tem propriedade; a mesma do [`SchemaXml`](#schemaxml). Nula: os campos saem sem propriedade. |
| `Schema` | `SchemaXml?` | get; init | O schema do documento sozinho (a nota, 225), validado pelo núcleo antes do schema da mensagem (o lote, 215): o problema sai com o caminho do documento (`"NFe/infNFe/..."`) e a propriedade do schema (ou, sem ela, a da [`Propriedade`](#documentopedido)). Só sem erro em todos os documentos a mensagem é validada. Nulo: o documento é validado só dentro da mensagem. |
| `PrefixoPropriedade` | `string?` | get; init | O que o núcleo põe antes da propriedade dos problemas deste documento (os do [`Schema`](#documentopedido), das regras do leiaute e dos callbacks): qual documento do parâmetro (`"Notas[2]."`, para `"Notas[2].Itens[0].Produto.Valor"`). Os callbacks recebem a propriedade sem ele, como no [`Preparar`](#dfe). Nulo: a propriedade fica como vem. |

## DocumentoPreparado

Classe selada · Namespace `OpenDFe`

O resultado do [`Preparar`](#dfe): o documento assinado e validado, pronto para transmitir com os mesmos bytes (PROJETO.md 8.3), ou os problemas que o impedem. Imutável.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Preparado` | `bool` | get | Sem problema de severidade [`Erro`](#severidade): o [`Xml`](#documentopreparado) está assinado e válido no schema, nas regras do leiaute e nos callbacks. |
| `Xml` | `string?` | get | O XML assinado: o que se transmite e se guarda (PROJETO.md 5.3). Sem [`Preparado`](#documentopreparado), o XML até onde o caminho chegou (o montado, sem assinatura, quando a montagem tem erro); `null` quando a montagem não o produziu. Tem dados pessoais: guardar com o cuidado do documento, nunca em log. |
| `Chave` | `ChaveAcesso?` | get | A chave de acesso do documento; nula quando a montagem não a fez. |
| `Problemas` | `IReadOnlyList<Problema>` | get | Todos os problemas: os da montagem, do schema, das regras e dos callbacks, inclusive os avisos. |

## DocumentoVerificado

Classe selada · Namespace `OpenDFe`

Um documento inteiro (cada nota do lote), entregue ao callback [`Documento`](#validacaocallback). Criado pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Documento` | `Documento` | get | O documento (NF-e, NFC-e). |
| `Modelo` | `object?` | get | O modelo do leiaute (no OpenDFe.NFe, a nota), para quem usa converter para o tipo que conhece; `null` quando o documento veio só como XML. |
| `Xml` | `string` | get | O XML do documento. Tem dados pessoais. |
| `Chave` | `ChaveAcesso?` | get | A chave de acesso, quando o documento tem uma. |

## EnderecoServico

Classe selada · Namespace `OpenDFe`

A URL de um serviço (`"NFeStatusServico4"`) num autorizador e num ambiente. O leiaute declara as padrão ([`DeclararEndereco`](#declaracaoleiaute)); quem usa troca as que mudaram ([`Urls`](#configuracaodfe)).

### Construtores

| Construtor | Descrição |
|---|---|
| `EnderecoServico(Destino autorizador, Ambiente ambiente, string servico, Uri url)` | Cria o endereço de um serviço. **`ArgumentNullException`:** O serviço ou a URL são nulos. **`ArgumentOutOfRangeException`:** O autorizador ou o ambiente não são valores conhecidos. **`ArgumentException`:** O serviço está vazio; a URL não é absoluta ou não é HTTPS. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Autorizador` | `Destino` | get | O autorizador. |
| `Ambiente` | `Ambiente` | get | O ambiente. |
| `Servico` | `string` | get | O nome do serviço (`"NFeStatusServico4"`). |
| `Url` | `Uri` | get | A URL. |

## FacetaSchema

Enumeração · Namespace `OpenDFe`

A restrição de um tipo simples do schema (a faceta do XSD) que um valor violou.

| Membro | Valor | Descrição |
|---|---|---|
| `TipoPrimitivo` | 1 | O tipo primitivo do XSD (`xs:decimal`, `xs:date`...): o valor nem chega a ser um valor dele. |
| `Tamanho` | 2 | O tamanho exato (`length`). |
| `TamanhoMinimo` | 3 | O tamanho mínimo (`minLength`). |
| `TamanhoMaximo` | 4 | O tamanho máximo (`maxLength`). |
| `Padrao` | 5 | O padrão, uma expressão regular do XSD (`pattern`). |
| `Enumeracao` | 6 | Os valores aceitos (`enumeration`). |
| `TotalDigitos` | 7 | O total de dígitos (`totalDigits`). |
| `CasasDecimais` | 8 | As casas decimais (`fractionDigits`). |
| `MinimoInclusivo` | 9 | O mínimo, inclusive (`minInclusive`). |
| `MaximoInclusivo` | 10 | O máximo, inclusive (`maxInclusive`). |
| `MinimoExclusivo` | 11 | O mínimo, exclusive (`minExclusive`). |
| `MaximoExclusivo` | 12 | O máximo, exclusive (`maxExclusive`). |

## FalhaSchema

Classe selada · Namespace `OpenDFe`

O detalhe de um problema da validação pelo schema ([`Schema`](#problema)): o que falhou, as regras aplicadas, o valor encontrado e a posição no XML.

[`Valor`](#falhaschema) e [`MensagemOriginal`](#falhaschema) podem trazer dados pessoais do documento (CPF, nome, endereço): servem a quem é dono do XML, para mostrar ao usuário, e não devem ir para log nem telemetria. A [`Mensagem`](#problema) nunca traz o valor.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Tipo` | `TipoFalhaSchema` | get | O que falhou. |
| `TipoSchema` | `string?` | get | O nome do tipo do schema do elemento ou atributo (`"TDec_1302"`; os do XSD com o prefixo, `"xs:string"`); `null` se o tipo é anônimo ou desconhecido. |
| `Documentacao` | `string?` | get | O texto do `xs:documentation` da declaração do campo no schema (`"Valor bruto do produto ou serviço."`): o do elemento ou atributo que falhou, ou, na falta de um elemento obrigatório (ausente, ou outro no lugar dele), o do que faltou; `null` quando o schema não documenta o campo ou a falha não aponta uma declaração. |
| `DocumentacaoTipo` | `string?` | get | O texto do `xs:documentation` do tipo nomeado do campo (`"Tipo Decimal com 15 dígitos, sendo 13 de corpo e 2 decimais"`, o do [`TipoSchema`](#falhaschema)); `null` quando o tipo não é documentado. |
| `Valor` | `string?` | get | O valor encontrado no elemento ou atributo, como está no XML, na falha de [`Valor`](#tipofalhaschema); `null` nas outras. Pode ser dado pessoal (ver a observação da classe). |
| `Restricoes` | `IReadOnlyList<RestricaoSchema>` | get | As restrições do tipo que o valor violou, na falha de [`Valor`](#tipofalhaschema); vazia nas outras e quando a lib não identifica a restrição. |
| `Esperados` | `IReadOnlyList<string>` | get | Os nomes (locais, sem prefixo) dos elementos ou atributos esperados, nas falhas de estrutura: os que podiam vir no lugar do inesperado, os obrigatórios ausentes, os atributos que o elemento declara. Um `"*"` representa qualquer elemento (`xs:any`). |
| `Linha` | `int?` | get | A linha no XML (a partir de 1), quando conhecida. |
| `Coluna` | `int?` | get | A coluna no XML (a partir de 1), quando conhecida. |
| `MensagemOriginal` | `string` | get | A mensagem do validador do .NET, no idioma da máquina, para diagnóstico. Pode trazer o valor (ver a observação da classe). |

## FalhaTransmissaoException

Exceção · Namespace `OpenDFe` · Deriva de `Exception`

Falha de infraestrutura depois de montado e validado o pedido: rede, TLS, tempo esgotado, HTTP diferente de 200, falha SOAP, resposta ilegível (PROJETO.md 2.4). A espécie está em [`Tipo`](#falhatransmissaoexception); a causa, em `InnerException`. Criada pelo núcleo.

A mensagem não traz o XML; o [`Rastro`](#falhatransmissaoexception) traz, com os dados pessoais do documento: não o registre em log.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Rastro` | `RastroWS` | get | O que foi enviado (e recebido, se a resposta veio e não se leu: na falha SOAP e no HTTP sem envelope, o corpo como veio), a URL e os tempos. |
| `Tipo` | `TipoFalhaTransmissao` | get | A espécie da falha. |
| `PodeTerSidoProcessado` | `bool` | get | O pedido saiu inteiro e a resposta não veio (ou não se leu): o autorizador pode tê-lo processado. Na autorização, é o caso da recuperação pela chave (PROJETO.md 8.6). `false`: com certeza não foi processado (DNS, conexão recusada, TLS, certificado recusado, falha SOAP do remetente, HTTP 4xx). |

## FormaLeitura

Classe selada · Namespace `OpenDFe`

Uma forma de XML que o leiaute lê, para um documento (PROJETO.md 3.3): a raiz e onde estão a versão e o modelo. O leiaute a declara em [`DeclararLeitura`](#declaracaoleiaute), e [`Detectar`](#dfe) a confere.

Os caminhos são os nomes locais dos elementos separados por `/`, a partir da raiz (sem ela), no namespace da raiz; o último passo pode ser um atributo sem namespace (`@versao`). O caminho da versão termina sempre num atributo (`infNFe/@versao`; `@versao`, o da própria raiz); o do modelo, num elemento ou num atributo (`infNFe/ide/mod`), cujo texto é o número do documento (`55`).

### Construtores

| Construtor | Descrição |
|---|---|
| `FormaLeitura(string espacoNomes, string raiz, string caminhoVersao, string caminhoModelo)` | Cria a forma. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** Um texto vazio; um caminho com passo vazio ou com atributo antes do fim; o caminho da versão sem atributo no fim. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `EspacoNomes` | `string` | get | O namespace da raiz. |
| `Raiz` | `string` | get | O nome local da raiz. |
| `CaminhoVersao` | `string` | get | O caminho do atributo da versão. |
| `CaminhoModelo` | `string` | get | O caminho do modelo. |

## FormatadorInvariavel

Classe estática · Namespace `OpenDFe`

Formata os valores como os leiautes os escrevem no XML, sempre na cultura invariável: nada depende de `CurrentCulture`.

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Data(DateOnly valor)` | `string` | A data, sem hora (`"2026-10-19"`). **Retorna:** O valor formatado. |
| `static DataHora(DateTimeOffset valor)` | `string` | A data e hora com o deslocamento do fuso, no formato do leiaute (`"2026-10-19T14:30:00-03:00"`). Frações de segundo são descartadas. **Retorna:** O valor formatado. |
| `static Numero(decimal valor, int casas)` | `string` | O decimal com as casas do campo, ponto como separador e sem separador de milhar (`1234.5m` com 2 casas: `"1234.50"`). **Retorna:** O valor formatado. **`ArgumentOutOfRangeException`:** `casas` fora de 0 a 28. |

## ILeiaute

Interface · Namespace `OpenDFe`

Um documento numa versão do leiaute (PROJETO.md 3.1): o que um leiaute implementa para o núcleo. Tudo o que ele tem (operações, atendimentos, URLs, a configuração do documento) é declarado em [`Declarar`](#ileiaute).

Sem estado e seguro para uso concorrente: a mesma instância serve a vários núcleos.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Versao` | `string` | get | A versão do leiaute, como no atributo `versao` do XML (`"4.00"`). |
| `Documentos` | `IReadOnlyList<Documento>` | get | Os documentos que o leiaute atende (`[NFe, NFCe]`). |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Declarar(DeclaracaoLeiaute declaracao)` | `void` | Declara tudo o que o leiaute tem para o documento de [`Documento`](#declaracaoleiaute). Chamado por [`Criar`](#dfe) e [`Criar`](#declaracaoleiaute); a mesma entrada dá sempre a mesma declaração. |

## `IOperacao<TParametro, TRetorno>`

Interface · Namespace `OpenDFe`

O handler de uma operação, no leiaute: monta o pedido e lê o retorno. Registrado por [`RegistrarOperacao`](#declaracaoleiaute).

Sem estado e seguro para uso concorrente. Não faz I/O: o núcleo assina, valida pelo schema, escolhe a URL e transmite (o caminho de [`ExecutarAsync`](#dfe)).

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Ler(TParametro parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)` | `LeituraWS<TRetorno>` | Lê o retorno (o conteúdo do `nfeResultMsg`). O XML que não se lê é `XmlException` ou `FormatException`: o núcleo a embrulha numa [`FalhaTransmissaoException`](#falhatransmissaoexception) com o rastro. Outra exceção é erro de programação e sobe como veio. **Retorna:** O retorno lido. |
| `Montar(TParametro parametro, ContextoOperacao contexto)` | `PedidoWS` | Monta a mensagem (o conteúdo do `nfeDadosMsg`), sem assinar. Os problemas da montagem vão no [`Problemas`](#pedidows); exceção aqui é erro de programação e sobe como veio. **Retorna:** O pedido. |

## `IPreparo<TDocumento>`

Interface · Namespace `OpenDFe`

O preparo de um documento, no leiaute: monta o XML que o [`Preparar`](#dfe) assina e valida sem transmitir (a nota preparada, a NFC-e offline). Registrado por [`RegistrarPreparo`](#declaracaoleiaute).

Sem estado e seguro para uso concorrente. Não faz I/O.

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `Montar(TDocumento documento, ContextoOperacao contexto)` | `DocumentoMontado` | Monta o documento, sem assinar. Os problemas da montagem vão no [`Problemas`](#documentomontado); exceção aqui é erro de programação e sobe como veio. **Retorna:** O documento montado. |

## `LeituraWS<TRetorno>`

Classe selada · Namespace `OpenDFe`

O retorno lido por [`Ler`](#ioperacaotparametro-tretorno): o objeto do leiaute, o `cStat`, o motivo e os problemas da leitura.

### Construtores

| Construtor | Descrição |
|---|---|
| `LeituraWS(TRetorno retorno, int cStat, string motivo)` | Cria o retorno lido. **`ArgumentNullException`:** O retorno ou o motivo são nulos. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Retorno` | `TRetorno` | get | O retorno. |
| `CStat` | `int` | get | O `cStat` da mensagem. |
| `Motivo` | `string` | get | O `xMotivo` da mensagem. |
| `Problemas` | `IReadOnlyList<Problema>` | get; init | Os problemas da leitura (um campo que o leiaute não reconhece, por exemplo). |

## OperacaoSuportada

Classe selada · Namespace `OpenDFe`

Uma operação que o leiaute registrou ([`Operacoes`](#dfe)): o tipo do parâmetro e o do retorno. Criada pelo núcleo.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Parametro` | `Type` | get | O tipo do parâmetro (`typeof(ConsultaStatus)`). |
| `Retorno` | `Type` | get | O tipo do retorno (`typeof(RetornoStatus)`). |

## ParametroWS

Classe abstrata · Namespace `OpenDFe`

A base de todo parâmetro de operação. Só [`ParametroWS`](#parametrowstretorno) deriva dela: existe para a restrição de [`Suporta`](#dfe).

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Destino` | `Destino?` | get; init | Para onde a operação vai; `null` = o destino natural dela (PROJETO.md 8.3 e 8.4). Sobrepõe o destino do pedido e o do núcleo; o tipo de emissão continua o do pedido (ou o do núcleo). |

## `ParametroWS<TRetorno>`

Classe abstrata · Namespace `OpenDFe` · Deriva de [`ParametroWS`](#parametrows)

O parâmetro de uma operação que devolve `TRetorno` (`DFe.ExecutarAsync(new ConsultaStatus())` é `Task<ResultadoWS<RetornoStatus>>`). Os leiautes derivam classes seladas, uma por operação.

## PedidoWS

Classe selada · Namespace `OpenDFe`

O pedido montado por [`Montar`](#ioperacaotparametro-tretorno): a mensagem, o serviço, o schema, o que assinar, o destino e os problemas da montagem.

### Construtores

| Construtor | Descrição |
|---|---|
| `PedidoWS(string servico, string xml, SchemaXml schema)` | Cria o pedido. **`ArgumentNullException`:** O XML ou o schema são nulos. **`ArgumentException`:** O serviço é nulo ou vazio. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Servico` | `string` | get | O nome do serviço (`"NFeStatusServico4"`). |
| `Xml` | `string` | get | A mensagem, sem assinatura. |
| `Schema` | `SchemaXml` | get | O schema da mensagem. |
| `IdsAssinatura` | `IReadOnlyList<string>` | get; init | Os `Id` dos elementos que o núcleo assina, na ordem (um `infNFe` por nota do lote, um `infEvento` por evento); vazio quando a mensagem não é assinada ou já vem assinada. |
| `Documentos` | `IReadOnlyList<DocumentoPedido>` | get; init | Os documentos da mensagem, na ordem (uma nota por nota do lote), para as regras do leiaute e os callbacks de quem usa; vazio quando a mensagem não leva documento (status, consulta). Um `Id` que não está no XML assinado é erro de programação do leiaute (`InvalidOperationException` no [`ExecutarAsync`](#dfe)). |
| `Destino` | `Destino?` | get; init | O destino que sai do documento ou da chave (PROJETO.md 8.4); `null` = o do núcleo (a UF). O [`Destino`](#parametrows) sobrepõe este. |
| `Emissao` | `TipoEmissao?` | get; init | O tipo de emissão que sai do documento ou da chave (PROJETO.md 8.4); `null` = o do núcleo. |
| `Problemas` | `IReadOnlyList<Problema>` | get; init | Os problemas da montagem (regras do leiaute, campo não suportado). Com um de severidade [`Erro`](#severidade), nada é assinado nem enviado. |

## Problema

Classe selada · Namespace `OpenDFe`

Um problema encontrado na validação (schema, regras do leiaute ou de quem usa) ou na leitura de um documento. Problema não é exceção: volta como resultado.

### Construtores

| Construtor | Descrição |
|---|---|
| `Problema(string codigo, Severidade severidade, string mensagem)` | Cria um problema. **`ArgumentException`:** O código ou a mensagem estão vazios. **`ArgumentOutOfRangeException`:** A severidade não é um valor de [`Severidade`](#severidade). |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Codigo` | `string` | get | O código do problema; nas regras do MOC, o da rejeição que ela antecipa (`"564"`). |
| `Severidade` | `Severidade` | get | A severidade. |
| `Mensagem` | `string` | get | A mensagem, em português. |
| `Tag` | `string?` | get; init | A tag do XML (`"vProd"`), quando o problema é de um campo. |
| `Caminho` | `string?` | get; init | O caminho do elemento no XML (`"det[3]/prod/vProd"`), quando conhecido. |
| `Propriedade` | `string?` | get; init | A propriedade do modelo (`"Itens[2].Produto.Valor"`), quando conhecida. |
| `Schema` | `FalhaSchema?` | get; init | O detalhe do problema da validação pelo schema ([`Validar`](#schemaxml)): as regras aplicadas, o valor encontrado e a posição; `null` nos outros problemas. |

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `ToString()` | `string` | O problema em uma linha: severidade, código, mensagem e, se houver, onde. **Retorna:** O texto do problema. |

## RastroWS

Classe selada · Namespace `OpenDFe`

O que foi enviado e recebido numa operação, para quem quiser guardar (PROJETO.md 8.7): no [`Rastro`](#resultadowstretorno) e na [`Rastro`](#falhatransmissaoexception). Criado pelo núcleo.

Os XMLs têm dados pessoais: guarde-os com o cuidado do documento, nunca em log nem telemetria.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `XmlPedido` | `string?` | get | A mensagem (o conteúdo do `nfeDadosMsg`) como foi, ou seria, enviada, assinada; na recusa da montagem, como o leiaute a montou; `null` se a montagem não a produziu. |
| `XmlRetorno` | `string?` | get | A resposta (o conteúdo do `nfeResultMsg`); `null` se não houve resposta. |
| `Autorizador` | `Destino?` | get | O autorizador escolhido; `null` se a operação parou antes da escolha. |
| `Url` | `Uri?` | get | A URL escolhida; `null` se a operação parou antes da escolha. |
| `Inicio` | `DateTimeOffset` | get | O início da operação, pelo [`Relogio`](#configuracaodfe). |
| `Duracao` | `TimeSpan` | get | Do início da operação ao resultado (ou à falha). |
| `DuracaoTransmissao` | `TimeSpan?` | get | Só a ida e volta ao autorizador; `null` se não chegou a transmitir. |

## RestricaoSchema

Classe selada · Namespace `OpenDFe`

Uma restrição de um tipo simples do schema que o valor violou ([`Restricoes`](#falhaschema)).

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Faceta` | `FacetaSchema` | get | A restrição. |
| `Valores` | `IReadOnlyList<string>` | get | O valor da restrição, como está no schema: um item (`"60"` no tamanho máximo, a expressão no padrão, o nome do tipo primitivo, `"decimal"`), ou todos os aceitos na enumeração e todos os padrões alternativos do mesmo tipo (basta atender um). |
| `Tipo` | `string?` | get | O nome do tipo do schema que declara a restrição (`"TString"`, quando herdada pelo tipo do campo); `null` se ela está num tipo anônimo, declarado dentro do elemento. |

## `ResultadoWS<TRetorno>`

Classe selada · Namespace `OpenDFe`

O resultado de uma operação ([`ExecutarAsync`](#dfe)): processada pelo autorizador (com o `cStat`, inclusive a rejeição) ou recusada localmente, com os problemas e o rastro. Criado pelo núcleo.

A falha de infraestrutura (rede, TLS, resposta ilegível) não é resultado: é [`FalhaTransmissaoException`](#falhatransmissaoexception) (PROJETO.md 2.4).

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Situacao` | `SituacaoResultado` | get | O que aconteceu. |
| `Processado` | `bool` | get | [`Situacao`](#resultadowstretorno) é [`Processado`](#situacaoresultado); com ela, [`Retorno`](#resultadowstretorno), [`CStat`](#resultadowstretorno) e [`Motivo`](#resultadowstretorno) não são nulos. |
| `CStat` | `int?` | get | O `cStat` da resposta; `null` na recusa local. |
| `Motivo` | `string?` | get | O `xMotivo` da resposta; `null` na recusa local. |
| `Retorno` | `TRetorno?` | get | O retorno lido pelo leiaute; `null` na recusa local. |
| `Problemas` | `IReadOnlyList<Problema>` | get | Todos os problemas: os da montagem, os do schema e os da leitura, na ordem do caminho; no processado, só avisos. |
| `Rastro` | `RastroWS` | get | O que foi enviado e recebido, a URL e os tempos. |

## RetornoStatus

Classe selada · Namespace `OpenDFe`

O retorno do status do serviço ([`ConsultaStatus`](#consultastatus)): o `cStat`, o motivo e os outros campos da resposta, comuns aos documentos.

O leiaute cria. A leitura é branda: só o `cStat` e o `xMotivo` são exigidos (sem eles, a resposta é ilegível); o campo ausente fica nulo, e o que não se lê também, com um aviso no resultado. O [`Status`](#retornostatus) e o [`Motivo`](#retornostatus) são os mesmos do [`ResultadoWS`](#resultadowstretorno): repetidos aqui para o retorno valer sozinho.

### Construtores

| Construtor | Descrição |
|---|---|
| `RetornoStatus(int status, string motivo)` | Cria o retorno. **`ArgumentNullException`:** O motivo é nulo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Status` | `int` | get | O código do status (`cStat`): 107 em operação; 108 paralisado momentaneamente; 109 paralisado sem previsão; ou o da rejeição do pedido. |
| `Motivo` | `string` | get | A descrição do status (`xMotivo`). |
| `Operando` | `bool` | get | Se o serviço está em operação ([`Status`](#retornostatus) 107). |
| `Ambiente` | `Ambiente?` | get; init | O ambiente que respondeu (`tpAmb`). |
| `VersaoAplicativo` | `string?` | get; init | A versão do aplicativo do autorizador (`verAplic`). |
| `UF` | `UF?` | get; init | A UF responsável pelo serviço (`cUF`). |
| `Recebimento` | `DateTimeOffset?` | get; init | A data e a hora do recebimento da consulta (`dhRecbto`). |
| `TempoMedio` | `TimeSpan?` | get; init | O tempo médio de resposta do serviço nos últimos 5 minutos (`tMed`, em segundos). |
| `PrevisaoRetorno` | `DateTimeOffset?` | get; init | A data e a hora previstas para a volta do serviço paralisado (`dhRetorno`). |
| `Observacao` | `string?` | get; init | A observação do autorizador ao contribuinte (`xObs`). |

## SchemaXml

Classe selada · Namespace `OpenDFe`

O schema XSD de uma mensagem (a nota, o lote, o pedido de evento), carregado dos recursos embutidos de um assembly e compilado uma vez, e a validação de um XML por ele (PROJETO.md 6.1 e 6.4).

A instância é imutável e segura para validar em paralelo: carregue uma vez (o leiaute a guarda) e valide quantas vezes quiser. O `XmlSchemaSet` compilado fica dentro dela, sem ser exposto, para ninguém o alterar. A carga nunca toca o disco nem a rede: o schema principal e os que ele inclui ou importa (`xs:include`, `xs:import`) vêm só dos recursos do assembly, e DTD é proibido. A validação não segue o `xsi:schemaLocation` do XML validado.

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Carregar(Assembly assembly, string recurso, string codigo)` | `SchemaXml` | Carrega e compila o schema de uma mensagem a partir dos recursos embutidos de um assembly. **Retorna:** O schema compilado. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** O recurso ou o código estão vazios, ou o recurso não existe no assembly. **`XmlSchemaException`:** Um schema é inválido, não compila, inclui ou importa um recurso que não existe ou um endereço externo. **`XmlException`:** Um schema não é XML bem formado ou tem DTD. |
| `static Carregar(Assembly assembly, string recurso, string codigo, Func<string, string?> propriedade)` | `SchemaXml` | Carrega e compila o schema de uma mensagem, como [`Carregar`](#schemaxml), com a conversão do caminho do problema na propriedade do modelo (PROJETO.md 6.4): o [`Validar`](#schemaxml) preenche a [`Propriedade`](#problema) de cada problema com o que ela devolve para o [`Caminho`](#problema). **Retorna:** O schema compilado. **`ArgumentNullException`:** Um dos argumentos é nulo. **`ArgumentException`:** O recurso ou o código estão vazios, ou o recurso não existe no assembly. **`XmlSchemaException`:** Um schema é inválido, não compila, inclui ou importa um recurso que não existe ou um endereço externo. **`XmlException`:** Um schema não é XML bem formado ou tem DTD. |
| `Validar(string xml)` | `IReadOnlyList<Problema>` | Valida um XML pelo schema, juntando todos os problemas (não para no primeiro). **Retorna:** Os problemas, na ordem do XML; vazio se o XML é válido. Cada um tem o código informado na carga, a severidade ([`Erro`](#severidade), ou [`Aviso`](#severidade) nos avisos do validador), a mensagem em português, a [`Tag`](#problema) (o nome local do elemento ou atributo), o [`Caminho`](#problema), a [`Propriedade`](#problema) quando o schema foi carregado com a conversão e o detalhe em [`Schema`](#problema). **`ArgumentNullException`:** O XML é nulo. **`XmlException`:** O XML não é bem formado ou tem DTD. |

## ServicoSoap

Classe selada · Namespace `OpenDFe`

O que o envelope SOAP 1.2 de um serviço leva (PROJETO.md 8.2): o namespace do WSDL, que é o do `nfeDadosMsg` e do `nfeResultMsg`, a ação, no `Content-Type`, e, no serviço que a tem no `Body`, a operação ([`Operacao`](#servicosoap)). O leiaute declara um por serviço ([`DeclararServico`](#declaracaoleiaute)); o núcleo não conhece os serviços de nenhum documento.

### Construtores

| Construtor | Descrição |
|---|---|
| `ServicoSoap(string nome, string namespaceWsdl, string acao)` | Cria a declaração de um serviço. **`ArgumentException`:** Um dos valores é nulo, vazio ou só espaços. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Nome` | `string` | get | O nome do serviço (`"NFeStatusServico4"`). |
| `NamespaceWsdl` | `string` | get | O namespace do WSDL, o do `nfeDadosMsg` e do `nfeResultMsg`. |
| `Acao` | `string` | get | A ação SOAP, no parâmetro `action` do `Content-Type`. |
| `Operacao` | `string?` | get; init | O nome da operação, nos serviços cujo `Body` a tem como elemento (`"nfeDistDFeInteresse"`, da distribuição DF-e, NT 2014.002). Quando não nulo, o `nfeDadosMsg` vai dentro de `<Operacao xmlns="namespaceWsdl">`, e a resposta é lida em `<Operacao>Response/<Operacao>Result`, em vez do `nfeResultMsg` direto no `Body`. Nulo (o padrão) é o envelope dos outros serviços, sem mudança. |

## Severidade

Enumeração · Namespace `OpenDFe`

Severidade de um [`Problema`](#problema).

| Membro | Valor | Descrição |
|---|---|---|
| `Aviso` | 1 | Não impede o envio, mas precisa ser visto (por exemplo, um campo que o leiaute não suporta). |
| `Erro` | 2 | Impede o envio: o documento seria rejeitado. |

## SituacaoResultado

Enumeração · Namespace `OpenDFe`

O que aconteceu com uma operação ([`Situacao`](#resultadowstretorno)).

| Membro | Valor | Descrição |
|---|---|---|
| `Processado` | 1 | O autorizador respondeu, inclusive com rejeição: o [`CStat`](#resultadowstretorno) diz qual (PROJETO.md 2.4). |
| `RecusadoLocalmente` | 2 | Um problema de severidade [`Erro`](#severidade) antes de transmitir (na montagem ou no schema): nada foi enviado. |

## TagAttribute

Classe selada · Namespace `OpenDFe` · Deriva de `Attribute`

A tag do XML e o identificador do campo no manual (MOC) que uma propriedade do modelo representa (PROJETO.md 4.1): `[Tag("xNome", "E04")]` em `Destinatario.Nome`.

Serve à documentação (a tag aparece com a propriedade), ao problema que aponta a propriedade do modelo a partir do caminho no XML ([`Propriedade`](#problema)) e ao aviso do campo preenchido que o leiaute carregado não escreve (PROJETO.md 4.4). Quem lê o atributo é o leiaute; o núcleo só o define, porque ele vale para o modelo de qualquer documento. Na propriedade que guarda um grupo (`NotaFiscal.Destinatario`), é a tag do grupo (`dest`, `E01`); no atributo do XML, o nome sem `@` (`nItem`, `H02`). Na classe, é a tag da escolha do XML que a subclasse representa (`refNF`, `BA03`), quando a escolha é um grupo. Os valores não são conferidos aqui: são constantes do código do modelo, conferidas pelos testes do leiaute. Sem identificador ([`construtor`](#tagattribute)) nos campos de uma classe que o XML usa em mais de um lugar, onde o identificador do MOC muda com o lugar (`xLgr` é F03 na retirada e G03 na entrega): o identificador do grupo fica na propriedade que o guarda.

### Construtores

| Construtor | Descrição |
|---|---|
| `TagAttribute(string tag)` | Marca o campo com a tag, sem o identificador do MOC. |
| `TagAttribute(string tag, string identificador)` | Marca o campo com a tag e o identificador do campo. |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Tag` | `string` | get | O nome local da tag no XML (`"xNome"`). |
| `Identificador` | `string?` | get | O identificador do campo no leiaute do MOC (`"E04"`); nulo no campo de uma classe usada em mais de um lugar. |

## TipoEmissao

Enumeração · Namespace `OpenDFe`

Tipo (forma) de emissão do documento. O valor de cada membro é o que vai no XML (`tpEmis`).

É independente do [`Ambiente`](#ambiente): a contingência também se testa em homologação.

| Membro | Valor | Descrição |
|---|---|---|
| `Normal` | 1 | Emissão normal (1). |
| `ContingenciaFSIA` | 2 | Contingência em formulário de segurança, FS-IA (2). |
| `ContingenciaEpec` | 4 | Contingência pelo evento prévio de emissão, EPEC (4). |
| `ContingenciaFSDA` | 5 | Contingência em formulário de segurança para impressão de documento auxiliar, FS-DA (5). |
| `ContingenciaSvcAN` | 6 | Contingência pela SEFAZ Virtual do Ambiente Nacional, SVC-AN (6). |
| `ContingenciaSvcRS` | 7 | Contingência pela SEFAZ Virtual do Rio Grande do Sul, SVC-RS (7). |
| `ContingenciaOffline` | 9 | Contingência off-line da NFC-e (9). |

## TipoFalhaSchema

Enumeração · Namespace `OpenDFe`

O que falhou na validação pelo schema ([`Tipo`](#falhaschema)).

| Membro | Valor | Descrição |
|---|---|---|
| `Valor` | 1 | O valor de um elemento ou atributo não atende o tipo simples do schema (tamanho, padrão, valores aceitos, dígitos, faixa ou o tipo primitivo); as restrições violadas estão em [`Restricoes`](#falhaschema). |
| `ElementoAusente` | 2 | Falta um elemento obrigatório; os esperados estão em [`Esperados`](#falhaschema). |
| `ElementoInesperado` | 3 | Um elemento que não era esperado naquela posição (fora de ordem, a mais, não declarado ou dentro de um elemento que não aceita filhos); os esperados estão em [`Esperados`](#falhaschema). |
| `AtributoAusente` | 4 | Falta um atributo obrigatório; os ausentes estão em [`Esperados`](#falhaschema). |
| `AtributoInesperado` | 5 | Um atributo que o elemento não declara; os declarados estão em [`Esperados`](#falhaschema). |
| `TextoInesperado` | 6 | Texto num elemento que só aceita elementos filhos. |
| `Outra` | 7 | Outra falha, que a lib não classifica; o detalhe está em [`MensagemOriginal`](#falhaschema). |

## TipoFalhaTransmissao

Enumeração · Namespace `OpenDFe`

A espécie de uma [`FalhaTransmissaoException`](#falhatransmissaoexception): para quem chama decidir se repete (a conexão e o tempo esgotado) ou se tem algo a corrigir (o certificado recusado).

| Membro | Valor | Descrição |
|---|---|---|
| `Conexao` | 1 | A rede: o nome que não resolve, a conexão recusada ou derrubada, a resposta que parou no meio, ou outra falha do transporte. |
| `Tls` | 2 | O TLS: o handshake falhou ou o certificado do servidor não é confiável. Inclui o autorizador que aborta o handshake por não aceitar o certificado do cliente (MG, PR). |
| `CertificadoRecusado` | 3 | O autorizador respondeu HTTP 403 sem o envelope SOAP: recusou o certificado do cliente depois do handshake (o IIS da SVRS e de SP). Nada foi processado. |
| `TempoEsgotado` | 4 | O tempo esgotou: o de conexão ([`TempoConexao`](#conexoessefaz)) ou o de resposta ([`TempoResposta`](#conexoessefaz)). |
| `Http` | 5 | Uma resposta HTTP diferente de 200, sem o envelope SOAP. |
| `FalhaSoap` | 6 | A falha SOAP (`Fault`); o motivo vem na mensagem da exceção. |
| `RespostaIlegivel` | 7 | A resposta veio e não se leu: não é um envelope SOAP 1.2, não tem o `nfeResultMsg` ou o leiaute não a entendeu. |

## TitularCertificado

Classe selada · Namespace `OpenDFe`

O titular de um certificado digital: o nome e o CNPJ (e-CNPJ) ou o CPF (e-CPF), lidos por [`LerTitular`](#certificadodigital). Exatamente um entre [`Cnpj`](#titularcertificado) e [`Cpf`](#titularcertificado) está preenchido.

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Nome` | `string` | get | O nome do titular, do `CN` do certificado: a razão social no e-CNPJ, o nome civil no e-CPF. Sem o sufixo `:<CNPJ ou CPF>` que os certificados anteriores à versão 8.2 do DOC-ICP-04 levam no `CN`; vazio se o certificado não tem `CN`. |
| `Cnpj` | `string?` | get | O CNPJ do titular, sem máscara (talvez alfanumérico), ou `null` no e-CPF. |
| `Cpf` | `string?` | get | O CPF do titular, sem máscara, ou `null` no e-CNPJ. |

## UF

Enumeração · Namespace `OpenDFe`

Unidade federativa do Brasil. O valor de cada membro é o código IBGE da UF, o mesmo que vai no XML (`cUF`); o nome é a sigla.

Os ambientes nacionais (AN, SVRS, SVAN, SVC) não são UFs: são destinos de transmissão. Para converter de e para a sigla e o código, use [`UnidadesFederativas`](#unidadesfederativas).

| Membro | Valor | Descrição |
|---|---|---|
| `RO` | 11 | Rondônia (11). |
| `AC` | 12 | Acre (12). |
| `AM` | 13 | Amazonas (13). |
| `RR` | 14 | Roraima (14). |
| `PA` | 15 | Pará (15). |
| `AP` | 16 | Amapá (16). |
| `TO` | 17 | Tocantins (17). |
| `MA` | 21 | Maranhão (21). |
| `PI` | 22 | Piauí (22). |
| `CE` | 23 | Ceará (23). |
| `RN` | 24 | Rio Grande do Norte (24). |
| `PB` | 25 | Paraíba (25). |
| `PE` | 26 | Pernambuco (26). |
| `AL` | 27 | Alagoas (27). |
| `SE` | 28 | Sergipe (28). |
| `BA` | 29 | Bahia (29). |
| `MG` | 31 | Minas Gerais (31). |
| `ES` | 32 | Espírito Santo (32). |
| `RJ` | 33 | Rio de Janeiro (33). |
| `SP` | 35 | São Paulo (35). |
| `PR` | 41 | Paraná (41). |
| `SC` | 42 | Santa Catarina (42). |
| `RS` | 43 | Rio Grande do Sul (43). |
| `MS` | 50 | Mato Grosso do Sul (50). |
| `MT` | 51 | Mato Grosso (51). |
| `GO` | 52 | Goiás (52). |
| `DF` | 53 | Distrito Federal (53). |

## UnidadesFederativas

Classe estática · Namespace `OpenDFe`

Conversões da [`UF`](#uf) de e para a sigla e o código IBGE.

### Métodos

| Método | Retorno | Descrição |
|---|---|---|
| `static Codigo(this UF uf)` | `int` | O código IBGE da UF (`31` para MG), como vai no XML. **Retorna:** O código de dois dígitos. **`ArgumentOutOfRangeException`:** O valor não é uma UF. |
| `static Sigla(this UF uf)` | `string` | A sigla da UF, em maiúsculas (`"MG"`), como vai no XML. **Retorna:** A sigla de duas letras. **`ArgumentOutOfRangeException`:** O valor não é uma UF. |
| `static TentarLerCodigo(int codigo, out UF uf)` | `bool` | Obtém a UF pelo código IBGE (`31` para MG). **Retorna:** Se o código é de uma UF. |
| `static TentarLerSigla(string? sigla, out UF uf)` | `bool` | Obtém a UF pela sigla, em maiúsculas (`"MG"`). **Retorna:** Se a sigla é de uma UF. |

## ValidacaoCallback

Classe selada · Namespace `OpenDFe`

As regras de negócio de quem usa, na configuração ([`Validacao`](#configuracaodfe), PROJETO.md 6.3): um callback por campo escrito e um por documento inteiro. Cada um devolve os problemas encontrados (vazio = nenhum), com o código e a severidade de quem usa.

Os callbacks podem ser chamados em paralelo (o núcleo é compartilhado) e devem ser seguros para isso. Exceção num callback sobe como veio: é erro de programação. O núcleo os chama no [`ExecutarAsync`](#dfe), para cada documento do pedido ([`Documentos`](#pedidows)), depois do schema e das regras do leiaute e só sem erro neles: o [`Campo`](#validacaocallback) em cada campo do documento assinado (cada elemento sem filhos e cada atributo, fora da `Signature`), na ordem do XML, e depois o [`Documento`](#validacaocallback). Com um problema de severidade [`Erro`](#severidade), nada é enviado.

### Construtores

| Construtor | Descrição |
|---|---|
| `ValidacaoCallback()` |  |

### Propriedades

| Propriedade | Tipo | Acesso | Descrição |
|---|---|---|---|
| `Campo` | `Func<CampoVerificado, IEnumerable<Problema>>?` | get; init | Chamado por campo escrito no XML. |
| `Documento` | `Func<DocumentoVerificado, IEnumerable<Problema>>?` | get; init | Chamado por documento inteiro (cada nota do lote). |

## ValidadeCertificado

Enumeração · Namespace `OpenDFe`

A situação de um certificado num instante, pelo período de validade (`notBefore` e `notAfter`).

| Membro | Valor | Descrição |
|---|---|---|
| `Vigente` | 1 | O instante está dentro do período de validade, com os dois extremos incluídos. |
| `AindaNaoVigente` | 2 | O instante é anterior ao início da validade (`notBefore`). |
| `Vencido` | 3 | O instante é posterior ao fim da validade (`notAfter`). |


---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
