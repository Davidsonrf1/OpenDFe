# Especificação do modelo da NFS-e (docs/NFSE.md 7), na ordem do XSD 1.01.
# Campo: (tipo, tag, propriedade, extra, doc, opcoes)
#   tipo: s texto, d decimal (2 casas), i int, l long, dt DateOnly, dh DateTimeOffset, uf UF, amb Ambiente,
#         e enumeração (extra = (Enum, digitos)), g grupo opcional (extra = Classe), G grupo obrigatório,
#         L lista de grupos (extra = Classe), Ls lista de textos
#   opcoes: v100 (só na 1.00), alias (tags da 1.00 que vão para a mesma propriedade), leitura (só a leitura)

def S(tag, prop, doc, **o): return ('s', tag, prop, None, doc, o)
def D(tag, prop, doc, **o): return ('d', tag, prop, None, doc, o)
def I(tag, prop, doc, **o): return ('i', tag, prop, None, doc, o)
def L(tag, prop, doc, **o): return ('l', tag, prop, None, doc, o)
def DT(tag, prop, doc, **o): return ('dt', tag, prop, None, doc, o)
def DH(tag, prop, doc, **o): return ('dh', tag, prop, None, doc, o)
def UF(tag, prop, doc, **o): return ('uf', tag, prop, None, doc, o)
def AMB(tag, prop, doc, **o): return ('amb', tag, prop, None, doc, o)
def E(tag, prop, enum, dig, doc, **o): return ('e', tag, prop, (enum, dig), doc, o)
def G(tag, prop, cls, doc, **o): return ('g', tag, prop, cls, doc, o)
def GR(tag, prop, cls, doc, **o): return ('G', tag, prop, cls, doc, o)
def LG(tag, prop, cls, doc, **o): return ('L', tag, prop, cls, doc, o)
def LS(tag, prop, doc, **o): return ('Ls', tag, prop, None, doc, o)

CLASSES = []
def C(nome, doc, campos, escolhas=(), observacao=None):
    CLASSES.append(dict(nome=nome, doc=doc, campos=campos, escolhas=list(escolhas), obs=observacao))

ENUMS = []
def EN(nome, doc, membros, observacao=None):
    ENUMS.append(dict(nome=nome, doc=doc, membros=membros, obs=observacao))

def identificacao(alvo, nif=True):
    campos = [
        S('CNPJ', 'Cnpj', f'O CNPJ {alvo}, só os dígitos; um dos documentos (CNPJ, CPF, NIF ou o motivo da ausência do NIF).'),
        S('CPF', 'Cpf', f'O CPF {alvo}, só os dígitos.'),
    ]
    if nif:
        campos += [
            S('NIF', 'Nif', f'O número de identificação fiscal {alvo} no exterior.'),
            E('cNaoNIF', 'MotivoAusenciaNif', 'MotivoAusenciaNif', 0, f'O motivo de não informar o NIF {alvo}.'),
        ]
    return campos

ESCOLHA_DOC = ('Cnpj', 'Cpf', 'Nif', 'MotivoAusenciaNif')

def endereco_resto():
    return [
        S('xLgr', 'Logradouro', 'O tipo e o nome do logradouro.'),
        S('nro', 'Numero', 'O número.'),
        S('xCpl', 'Complemento', 'O complemento; opcional.'),
        S('xBairro', 'Bairro', 'O bairro.'),
    ]

# ---------------------------------------------------------------- DPS

C('Dps', 'A Declaração de Prestação de Serviço: o <c>DPS</c> e o <c>infDPS</c> juntos (docs/NFSE.md 7, M2). O que quem emite preenche, assina e envia, e o que a leitura devolve, também de dentro da NFS-e.',
  [
    AMB('tpAmb', 'Ambiente', 'O ambiente; nulo na escrita = o da configuração do núcleo (M4).'),
    DH('dhEmi', 'Emissao', 'A data e a hora da emissão da DPS.'),
    S('verAplic', 'VersaoAplicativo', 'A versão do aplicativo que gerou a DPS; nulo na escrita = "OpenDFe " e a versão do pacote (M4).'),
    S('serie', 'Serie', 'A série da DPS (até 5 dígitos; 00001 a 49999 para o aplicativo próprio).'),
    L('nDPS', 'Numero', 'O número da DPS (1 a 999.999.999.999.999).'),
    DT('dCompet', 'Competencia', 'A data em que se iniciou a prestação do serviço (a competência).'),
    E('tpEmit', 'Emitente', 'EmitenteDps', 0, 'Quem emite a DPS; a Sefin Nacional só aceita o prestador (regra NFSE-EMITENTE).'),
    E('cMotivoEmisTI', 'MotivoEmissaoTerceiro', 'MotivoEmissaoTerceiro', 0, 'O motivo da emissão pelo tomador ou pelo intermediário; opcional.'),
    S('chNFSeRej', 'ChaveRejeitada', 'A chave da NFS-e rejeitada pelo tomador ou pelo intermediário; opcional.'),
    I('cLocEmi', 'MunicipioEmissor', 'O código IBGE do município emissor.'),
    G('subst', 'Substituicao', 'Substituicao', 'A NFS-e que esta DPS substitui; preenchida, a emissão substitui a NFS-e da chave; opcional.'),
    GR('prest', 'Prestador', 'Prestador', 'O prestador do serviço.'),
    G('toma', 'Tomador', 'Pessoa', 'O tomador do serviço; opcional.'),
    G('interm', 'Intermediario', 'Pessoa', 'O intermediário do serviço; opcional.'),
    GR('serv', 'Servico', 'Servico', 'O serviço prestado.'),
    GR('valores', 'Valores', 'ValoresDps', 'Os valores do serviço e os tributos.'),
    G('IBSCBS', 'IbsCbs', 'IbsCbsDps', 'O IBS e a CBS declarados por quem emite; opcional.'),
  ])

C('Substituicao', 'A NFS-e substituída (<c>subst</c>): a emissão com este grupo cancela a NFS-e da chave por substituição (o evento 105102 é gerado pela Sefin).',
  [
    S('chSubstda', 'Chave', 'A chave de acesso da NFS-e substituída (50 dígitos).'),
    E('cMotivo', 'Motivo', 'MotivoSubstituicao', 2, 'O motivo da substituição.'),
    S('xMotivo', 'Descricao', 'A descrição do motivo; opcional.'),
  ])

C('Prestador', 'O prestador do serviço (<c>prest</c>, <c>TCInfoPrestador</c>). Com a DPS emitida pelo prestador, o nome, o endereço, o NIF e o motivo de ausência do NIF ficam de fora: o ambiente nacional os tira do cadastro (regra NFSE-PRESTADOR).',
  identificacao('do prestador') + [
    S('CAEPF', 'Caepf', 'O CAEPF do prestador pessoa física; opcional.'),
    S('IM', 'InscricaoMunicipal', 'A inscrição municipal; opcional.'),
    S('xNome', 'Nome', 'O nome ou o nome empresarial; opcional.'),
    G('end', 'Endereco', 'Endereco', 'O endereço; opcional.'),
    S('fone', 'Telefone', 'O telefone, com o DDD; opcional.'),
    S('email', 'Email', 'O e-mail; opcional.'),
    GR('regTrib', 'RegimeTributacao', 'RegimeTributacao', 'Os regimes de tributação do prestador.'),
  ], [ESCOLHA_DOC])

C('RegimeTributacao', 'Os regimes de tributação do prestador (<c>regTrib</c>).',
  [
    E('opSimpNac', 'SimplesNacional', 'SituacaoSimplesNacional', 0, 'A situação perante o Simples Nacional.'),
    E('regApTribSN', 'ApuracaoSimples', 'RegimeApuracaoSimples', 0, 'O regime de apuração do optante ME/EPP que ultrapassou um sublimite; opcional.'),
    E('regEspTrib', 'RegimeEspecial', 'RegimeEspecialTributacao', 0, 'O regime especial de tributação.'),
  ])

C('Pessoa', 'Uma pessoa da DPS (<c>TCInfoPessoa</c>): o tomador, o intermediário e o fornecedor da dedução (docs/NFSE.md 7, M8).',
  identificacao('da pessoa') + [
    S('CAEPF', 'Caepf', 'O CAEPF da pessoa física; opcional.'),
    S('IM', 'InscricaoMunicipal', 'A inscrição municipal; opcional.'),
    S('xNome', 'Nome', 'O nome ou o nome empresarial.'),
    G('end', 'Endereco', 'Endereco', 'O endereço; opcional.'),
    S('fone', 'Telefone', 'O telefone, com o DDD; opcional.'),
    S('email', 'Email', 'O e-mail; opcional.'),
  ], [ESCOLHA_DOC])

C('Endereco', 'Um endereço da DPS (<c>TCEndereco</c>): o nacional ou o do exterior, e o logradouro.',
  [
    G('endNac', 'Nacional', 'EnderecoNacional', 'O município e o CEP, no Brasil; um dos dois (nacional ou exterior).'),
    G('endExt', 'Exterior', 'EnderecoExterior', 'O país, o código postal e a cidade, no exterior.'),
  ] + endereco_resto(), [('Nacional', 'Exterior')])

C('EnderecoNacional', 'O município e o CEP do endereço no Brasil (<c>endNac</c>).',
  [
    I('cMun', 'Municipio', 'O código IBGE do município.'),
    S('CEP', 'Cep', 'O CEP, só os dígitos.'),
  ])

C('EnderecoExterior', 'O endereço no exterior (<c>endExt</c>, <c>TCEnderExt</c>).',
  [
    S('cPais', 'Pais', 'O código do país (ISO 3166-1, duas letras).'),
    S('cEndPost', 'CodigoPostal', 'O código de endereçamento postal.'),
    S('xCidade', 'Cidade', 'A cidade.'),
    S('xEstProvReg', 'EstadoProvincia', 'O estado, a província ou a região.'),
  ])

C('EnderecoExteriorSimples', 'O endereço no exterior sem o país (<c>endExt</c>, <c>TCEnderExtSimples</c>), da obra, do imóvel e do evento.',
  [
    S('cEndPost', 'CodigoPostal', 'O código de endereçamento postal.'),
    S('xCidade', 'Cidade', 'A cidade.'),
    S('xEstProvReg', 'EstadoProvincia', 'O estado, a província ou a região.'),
  ])

for nome, doc in (('EnderecoObra', 'O endereço da obra e do imóvel (<c>TCEnderObraEvento</c>).'),
                  ('EnderecoEvento', 'O endereço da atividade de evento (<c>TCEnderecoSimples</c>): o mesmo conteúdo do <see cref="EnderecoObra"/>, outro tipo no XSD (docs/NFSE.md 7, M8).')):
    C(nome, doc,
      [
        S('CEP', 'Cep', 'O CEP, só os dígitos; um dos dois (CEP ou exterior).'),
        G('endExt', 'Exterior', 'EnderecoExteriorSimples', 'O endereço no exterior.'),
      ] + endereco_resto(), [('Cep', 'Exterior')])

# ---------------------------------------------------------------- serviço

C('Servico', 'O serviço prestado (<c>serv</c>).',
  [
    GR('locPrest', 'Local', 'LocalPrestacao', 'O local da prestação.'),
    GR('cServ', 'Codigo', 'CodigoServico', 'Os códigos e a descrição do serviço.'),
    G('comExt', 'ComercioExterior', 'ComercioExterior', 'A exportação ou a importação do serviço; opcional.'),
    G('lsadppu', 'Locacao', 'LocacaoSublocacao', 'A locação, a sublocação, o arrendamento, o direito de passagem ou a permissão de uso; só na versão 1.00.', v100=True),
    G('obra', 'Obra', 'Obra', 'O serviço de obra; opcional.'),
    G('atvEvento', 'Evento', 'AtividadeEvento', 'A atividade de evento (artístico, cultural, esportivo); opcional.'),
    G('explRod', 'ExploracaoRodoviaria', 'ExploracaoRodoviaria', 'O pedágio; só na versão 1.00.', v100=True),
    G('infoCompl', 'InformacoesComplementares', 'InformacoesComplementares', 'As informações complementares; opcional.'),
  ])

C('LocalPrestacao', 'O local da prestação do serviço (<c>locPrest</c>): o município ou, fora do Brasil, o país.',
  [
    I('cLocPrestacao', 'Municipio', 'O código IBGE do município da prestação; um dos dois (município ou país).'),
    S('cPaisPrestacao', 'Pais', 'O código do país da prestação (ISO 3166-1, duas letras).'),
  ], [('Municipio', 'Pais')])

C('CodigoServico', 'Os códigos e a descrição do serviço (<c>cServ</c>).',
  [
    S('cTribNac', 'TributacaoNacional', 'O código de tributação nacional do ISSQN (6 dígitos, LC 116/2003).'),
    S('cTribMun', 'TributacaoMunicipal', 'O código de tributação municipal do ISSQN (3 dígitos); opcional.'),
    S('xDescServ', 'Descricao', 'A descrição completa do serviço.'),
    S('cNBS', 'Nbs', 'O código NBS do serviço (9 dígitos); opcional.'),
    S('cIntContrib', 'CodigoInterno', 'O código interno do contribuinte; opcional.'),
  ])

C('ComercioExterior', 'A exportação ou a importação do serviço (<c>comExt</c>).',
  [
    E('mdPrestacao', 'ModoPrestacao', 'ModoPrestacao', 0, 'O modo de prestação.'),
    E('vincPrest', 'Vinculo', 'VinculoPartes', 0, 'O vínculo entre as partes.'),
    S('tpMoeda', 'Moeda', 'O código da moeda da transação (3 dígitos).'),
    D('vServMoeda', 'ValorMoeda', 'O valor do serviço na moeda estrangeira.'),
    E('mecAFComexP', 'MecanismoPrestador', 'MecanismoFomentoPrestador', 2, 'O mecanismo de apoio ou fomento ao comércio exterior do prestador.'),
    E('mecAFComexT', 'MecanismoTomador', 'MecanismoFomentoTomador', 2, 'O mecanismo de apoio ou fomento ao comércio exterior do tomador.'),
    E('movTempBens', 'MovimentacaoBens', 'MovimentacaoTemporariaBens', 0, 'O vínculo com a movimentação temporária de bens.'),
    S('nDI', 'DeclaracaoImportacao', 'O número da declaração de importação; opcional.'),
    S('nRE', 'RegistroExportacao', 'O número do registro de exportação; opcional.'),
    E('mdic', 'CompartilharMdic', 'CompartilhamentoMdic', 0, 'Se a NFS-e é compartilhada com a Secretaria de Comércio Exterior.'),
  ])

C('LocacaoSublocacao', 'A locação, a sublocação, o arrendamento, o direito de passagem ou a permissão de uso (<c>lsadppu</c>). Só na versão 1.00: a leitura preenche, a escrita da 1.01 não escreve (docs/NFSE.md 7, M10).',
  [
    E('categ', 'Categoria', 'CategoriaLocacao', 0, 'A categoria do serviço.'),
    E('objeto', 'Objeto', 'ObjetoLocacao', 0, 'O objeto.'),
    I('extensao', 'Extensao', 'A extensão total da ferrovia, da rodovia, dos cabos, dos dutos ou dos condutos.'),
    I('nPostes', 'Postes', 'O número total de postes.'),
  ])

C('Obra', 'O serviço de obra (<c>obra</c>).',
  [
    S('inscImobFisc', 'InscricaoImobiliaria', 'A inscrição imobiliária fiscal; opcional.'),
    S('cObra', 'Codigo', 'O código da obra (CNO ou CEI); um dos três (código, CIB ou endereço).'),
    S('cCIB', 'Cib', 'O código do Cadastro Imobiliário Brasileiro.'),
    G('end', 'Endereco', 'EnderecoObra', 'O endereço da obra.'),
  ], [('Codigo', 'Cib', 'Endereco')])

C('AtividadeEvento', 'A atividade de evento (<c>atvEvento</c>).',
  [
    S('xNome', 'Nome', 'A descrição do evento (na versão 1.00, a tag <c>desc</c>).', alias=['desc']),
    DT('dtIni', 'Inicio', 'A data de início.'),
    DT('dtFim', 'Fim', 'A data de fim.'),
    S('idAtvEvt', 'Identificacao', 'A identificação do evento pela administração tributária (na versão 1.00, a tag <c>id</c>); um dos dois (identificação ou endereço).', alias=['id']),
    G('end', 'Endereco', 'EnderecoEvento', 'O endereço do evento.'),
  ], [('Identificacao', 'Endereco')])

C('ExploracaoRodoviaria', 'O pedágio (<c>explRod</c>). Só na versão 1.00: a leitura preenche, a escrita da 1.01 não escreve (docs/NFSE.md 7, M10).',
  [
    E('categVeic', 'Categoria', 'CategoriaVeiculo', 2, 'A categoria do veículo.'),
    I('nEixos', 'Eixos', 'O número de eixos.'),
    E('rodagem', 'Rodagem', 'TipoRodagem', 0, 'O tipo de rodagem.'),
    I('sentido', 'Sentido', 'O sentido.'),
    S('placa', 'Placa', 'A placa do veículo.'),
    S('codAcessoPed', 'CodigoAcesso', 'O código de acesso gerado pela concessionária.'),
    S('codContrato', 'CodigoContrato', 'O código do contrato.'),
  ])

C('InformacoesComplementares', 'As informações complementares do serviço (<c>infoCompl</c>).',
  [
    S('idDocTec', 'ResponsabilidadeTecnica', 'O documento de responsabilidade técnica (ART, RRT, DRT); opcional.'),
    S('docRef', 'DocumentoReferencia', 'O documento de referência (chave, número da nota, contrato); opcional.'),
    S('xPed', 'Pedido', 'O número do pedido, da ordem de compra ou de serviço; opcional.'),
    G('gItemPed', 'ItensPedido', 'ItensPedido', 'Os itens do pedido; opcional.'),
    S('xInfComp', 'Texto', 'As informações complementares (até 2.000 caracteres); opcional.'),
  ])

C('ItensPedido', 'Os itens do pedido (<c>gItemPed</c>).',
  [LS('xItemPed', 'Itens', 'Os números dos itens do pedido (1 a 99).')])

# ---------------------------------------------------------------- valores e tributos da DPS

C('ValoresDps', 'Os valores do serviço e os tributos da DPS (<c>valores</c>, <c>TCInfoValores</c>).',
  [
    GR('vServPrest', 'Servico', 'ValorServico', 'O valor do serviço.'),
    G('vDescCondIncond', 'Descontos', 'Descontos', 'Os descontos; opcional.'),
    G('vDedRed', 'DeducaoReducao', 'DeducaoReducao', 'A dedução ou a redução da base de cálculo; opcional.'),
    GR('trib', 'Tributacao', 'Tributacao', 'Os tributos.'),
  ])

C('ValorServico', 'O valor do serviço (<c>vServPrest</c>).',
  [
    D('vReceb', 'ValorRecebido', 'O valor recebido pelo intermediário; opcional.'),
    D('vServ', 'Valor', 'O valor do serviço.'),
  ])

C('Descontos', 'Os descontos condicionado e incondicionado (<c>vDescCondIncond</c>).',
  [
    D('vDescIncond', 'Incondicionado', 'O desconto incondicionado; opcional.'),
    D('vDescCond', 'Condicionado', 'O desconto condicionado; opcional.'),
  ])

C('DeducaoReducao', 'A dedução ou a redução da base de cálculo (<c>vDedRed</c>): o percentual, o valor ou os documentos.',
  [
    D('pDR', 'Percentual', 'O percentual de dedução ou redução; um dos três (percentual, valor ou documentos).'),
    D('vDR', 'Valor', 'O valor de dedução ou redução.'),
    G('documentos', 'Documentos', 'DocumentosDeducao', 'Os documentos da dedução ou da redução.'),
  ], [('Percentual', 'Valor', 'Documentos')])

C('DocumentosDeducao', 'Os documentos da dedução ou da redução (<c>documentos</c>).',
  [LG('docDedRed', 'Itens', 'DocumentoDeducao', 'Os documentos (1 a 1.000).')])

C('DocumentoDeducao', 'Um documento da dedução ou da redução (<c>docDedRed</c>).',
  [
    S('chNFSe', 'ChaveNFSe', 'A chave da NFS-e nacional; um dos seis documentos.'),
    S('chNFe', 'ChaveNFe', 'A chave da NF-e.'),
    G('NFSeMun', 'NFSeMunicipal', 'NFSeMunicipal', 'A NFS-e do padrão municipal anterior.'),
    G('NFNFS', 'NotaPapel', 'NotaPapel', 'A NF ou a NFS em papel.'),
    S('nDocFisc', 'NumeroDocumentoFiscal', 'O número de outro documento fiscal.'),
    S('nDoc', 'NumeroDocumento', 'O número de um documento não fiscal.'),
    E('tpDedRed', 'Tipo', 'TipoDeducao', 0, 'O tipo da dedução ou da redução.'),
    S('xDescOutDed', 'DescricaoOutra', 'A descrição, quando o tipo é "outras deduções"; opcional.'),
    DT('dtEmiDoc', 'Emissao', 'A data da emissão do documento.'),
    D('vDedutivelRedutivel', 'ValorDedutivel', 'O valor total dedutível ou redutível do documento.'),
    D('vDeducaoReducao', 'ValorUtilizado', 'O valor usado nesta NFS-e.'),
    G('fornec', 'Fornecedor', 'Pessoa', 'O fornecedor; opcional.'),
  ], [('ChaveNFSe', 'ChaveNFe', 'NFSeMunicipal', 'NotaPapel', 'NumeroDocumentoFiscal', 'NumeroDocumento')])

C('NFSeMunicipal', 'A NFS-e do padrão municipal anterior (<c>NFSeMun</c>).',
  [
    I('cMunNFSeMun', 'Municipio', 'O código IBGE do município emissor.'),
    S('nNFSeMun', 'Numero', 'O número da nota (15 dígitos).'),
    S('cVerifNFSeMun', 'CodigoVerificacao', 'O código de verificação.'),
  ])

C('NotaPapel', 'A NF ou a NFS em papel (<c>NFNFS</c>).',
  [
    S('nNFS', 'Numero', 'O número da nota (7 dígitos).'),
    S('modNFS', 'Modelo', 'O modelo da nota.'),
    S('serieNFS', 'Serie', 'A série da nota.'),
  ])

C('Tributacao', 'Os tributos do serviço (<c>trib</c>).',
  [
    GR('tribMun', 'Municipal', 'TributacaoMunicipal', 'O ISSQN.'),
    G('tribFed', 'Federal', 'TributacaoFederal', 'Os tributos federais; opcional.'),
    GR('totTrib', 'Total', 'TotalTributos', 'O total aproximado dos tributos (Lei 12.741/2012).'),
  ])

C('TributacaoMunicipal', 'O ISSQN (<c>tribMun</c>).',
  [
    E('tribISSQN', 'Tributacao', 'TributacaoIssqn', 0, 'A tributação do ISSQN; na versão 1.00 o XSD dá outro texto para os códigos 2 a 4, e a leitura guarda o número como veio (docs/NFSE.md 7, M12).'),
    S('cPaisResult', 'PaisResultado', 'O país do resultado da prestação, na exportação; opcional.'),
    E('tpImunidade', 'Imunidade', 'TipoImunidade', 0, 'O tipo da imunidade; opcional.'),
    G('exigSusp', 'ExigibilidadeSuspensa', 'ExigibilidadeSuspensa', 'A suspensão da exigibilidade; opcional.'),
    G('BM', 'Beneficio', 'BeneficioMunicipal', 'O benefício municipal; opcional.'),
    E('tpRetISSQN', 'Retencao', 'RetencaoIssqn', 0, 'A retenção do ISSQN.'),
    D('pAliq', 'Aliquota', 'A alíquota do ISSQN (%); opcional.'),
  ])

C('ExigibilidadeSuspensa', 'A suspensão da exigibilidade do ISSQN (<c>exigSusp</c>).',
  [
    E('tpSusp', 'Tipo', 'TipoSuspensao', 0, 'O tipo da suspensão.'),
    S('nProcesso', 'Processo', 'O número do processo (30 dígitos).'),
  ])

C('BeneficioMunicipal', 'O benefício municipal do ISSQN (<c>BM</c>).',
  [
    E('tpBM', 'Tipo', 'TipoBeneficioLei', 0, 'O tipo do benefício; só na versão 1.00.', v100=True),
    S('nBM', 'Identificador', 'O identificador do benefício parametrizado pelo município (14 dígitos).'),
    D('vRedBCBM', 'ValorReducao', 'O valor de redução da base de cálculo; um dos dois (valor ou percentual), opcional.'),
    D('pRedBCBM', 'PercentualReducao', 'O percentual de redução da base de cálculo.'),
  ], [('ValorReducao', 'PercentualReducao')])

C('TributacaoFederal', 'Os tributos federais (<c>tribFed</c>).',
  [
    G('piscofins', 'PisCofins', 'PisCofins', 'O PIS e a COFINS; opcional.'),
    D('vRetCP', 'RetencaoCp', 'A contribuição previdenciária retida; opcional.'),
    D('vRetIRRF', 'RetencaoIrrf', 'O IRRF retido; opcional.'),
    D('vRetCSLL', 'RetencaoCsll', 'A CSLL retida; opcional.'),
  ])

C('PisCofins', 'O PIS e a COFINS (<c>piscofins</c>).',
  [
    E('CST', 'Cst', 'CstPisCofinsServico', 2, 'O código de situação tributária.'),
    D('vBCPisCofins', 'BaseCalculo', 'A base de cálculo; opcional.'),
    D('pAliqPis', 'AliquotaPis', 'A alíquota do PIS (%); opcional.'),
    D('pAliqCofins', 'AliquotaCofins', 'A alíquota da COFINS (%); opcional.'),
    D('vPis', 'ValorPis', 'O valor do PIS; opcional.'),
    D('vCofins', 'ValorCofins', 'O valor da COFINS; opcional.'),
    E('tpRetPisCofins', 'Retencao', 'RetencaoPisCofins', 0, 'A retenção do PIS, da COFINS e da CSLL; opcional.'),
  ])

C('TotalTributos', 'O total aproximado dos tributos (<c>totTrib</c>): os valores, os percentuais, o indicador de não informar ou o percentual do Simples Nacional.',
  [
    G('vTotTrib', 'Valores', 'TotalTributosValor', 'Os valores; um dos quatro.'),
    G('pTotTrib', 'Percentuais', 'TotalTributosPercentual', 'Os percentuais.'),
    E('indTotTrib', 'Indicador', 'IndicadorTotalTributos', 0, 'O indicador de não informar o total (Decreto 8.264/2014).'),
    D('pTotTribSN', 'PercentualSimples', 'O percentual aproximado do total dos tributos da alíquota do Simples Nacional.'),
  ], [('Valores', 'Percentuais', 'Indicador', 'PercentualSimples')])

C('TotalTributosValor', 'Os valores aproximados dos tributos (<c>vTotTrib</c>).',
  [
    D('vTotTribFed', 'Federal', 'Os tributos federais.'),
    D('vTotTribEst', 'Estadual', 'Os tributos estaduais.'),
    D('vTotTribMun', 'Municipal', 'Os tributos municipais.'),
  ])

C('TotalTributosPercentual', 'Os percentuais aproximados dos tributos (<c>pTotTrib</c>).',
  [
    D('pTotTribFed', 'Federal', 'Os tributos federais (%).'),
    D('pTotTribEst', 'Estadual', 'Os tributos estaduais (%).'),
    D('pTotTribMun', 'Municipal', 'Os tributos municipais (%).'),
  ])

# ---------------------------------------------------------------- IBS e CBS da DPS

C('IbsCbsDps', 'O IBS e a CBS declarados por quem emite (<c>IBSCBS</c>, <c>TCRTCInfoIBSCBS</c>).',
  [
    E('finNFSe', 'Finalidade', 'FinalidadeNFSe', 0, 'A finalidade da emissão.'),
    E('indFinal', 'ConsumoPessoal', 'ConsumoPessoal', 0, 'Se a operação é de uso ou consumo pessoal; opcional.'),
    S('cIndOp', 'CodigoOperacao', 'O código indicador da operação (6 dígitos, Anexo C).'),
    E('tpOper', 'TipoOperacao', 'TipoOperacaoGoverno', 0, 'O tipo de operação com ente governamental ou sobre bens imóveis; opcional.'),
    G('gRefNFSe', 'Referencias', 'NFSeReferenciadas', 'As NFS-e referenciadas; opcional.'),
    E('tpEnteGov', 'EnteGovernamental', 'EnteGovernamental', 0, 'O tipo de ente governamental; opcional.'),
    E('indDest', 'IndicadorDestinatario', 'IndicadorDestinatario', 0, 'Se o destinatário é o próprio tomador.'),
    G('dest', 'Destinatario', 'Destinatario', 'O destinatário, quando não é o tomador; opcional.'),
    G('imovel', 'Imovel', 'Imovel', 'A operação com bem imóvel, exceto obra; opcional.'),
    GR('valores', 'Valores', 'ValoresIbsCbs', 'Os valores e os tributos do IBS e da CBS.'),
  ])

C('NFSeReferenciadas', 'As NFS-e referenciadas (<c>gRefNFSe</c>).',
  [LS('refNFSe', 'Chaves', 'As chaves das NFS-e referenciadas (1 a 99).')])

C('Destinatario', 'O destinatário do serviço (<c>dest</c>, <c>TCRTCInfoDest</c>).',
  identificacao('do destinatário') + [
    S('xNome', 'Nome', 'O nome ou o nome empresarial.'),
    G('end', 'Endereco', 'Endereco', 'O endereço; opcional.'),
    S('fone', 'Telefone', 'O telefone, com o DDD; opcional.'),
    S('email', 'Email', 'O e-mail; opcional.'),
  ], [ESCOLHA_DOC])

C('Imovel', 'A operação com bem imóvel, exceto obra (<c>imovel</c>).',
  [
    S('inscImobFisc', 'InscricaoImobiliaria', 'A inscrição imobiliária fiscal; opcional.'),
    S('cCIB', 'Cib', 'O código do Cadastro Imobiliário Brasileiro; um dos dois (CIB ou endereço).'),
    G('end', 'Endereco', 'EnderecoObra', 'O endereço do imóvel.'),
  ], [('Cib', 'Endereco')])

C('ValoresIbsCbs', 'Os valores e os tributos do IBS e da CBS declarados (<c>IBSCBS/valores</c>).',
  [
    G('gReeRepRes', 'ReembolsoRepasse', 'ReembolsoRepasse', 'Os reembolsos, repasses e ressarcimentos fora da base de cálculo; opcional.'),
    GR('trib', 'Tributos', 'TributosIbsCbs', 'Os tributos.'),
  ])

C('ReembolsoRepasse', 'Os reembolsos, repasses e ressarcimentos (<c>gReeRepRes</c>).',
  [LG('documentos', 'Documentos', 'DocumentoReembolso', 'Os documentos (1 a 1.000).')])

C('DocumentoReembolso', 'Um documento de reembolso, repasse ou ressarcimento (<c>gReeRepRes/documentos</c>).',
  [
    G('dFeNacional', 'DFeNacional', 'DFeNacional', 'O documento fiscal eletrônico do repositório nacional; um dos três documentos.'),
    G('docFiscalOutro', 'DocumentoFiscal', 'DocumentoFiscalOutro', 'O documento fiscal fora do repositório nacional.'),
    G('docOutro', 'DocumentoNaoFiscal', 'DocumentoOutro', 'O documento não fiscal.'),
    G('fornec', 'Fornecedor', 'FornecedorDocumento', 'O fornecedor; opcional.'),
    DT('dtEmiDoc', 'Emissao', 'A data da emissão do documento.'),
    DT('dtCompDoc', 'Competencia', 'A data da competência do documento.'),
    E('tpReeRepRes', 'Tipo', 'TipoReembolso', 2, 'O tipo do valor.'),
    S('xTpReeRepRes', 'DescricaoTipo', 'A descrição, quando o tipo é "outros"; opcional.'),
    D('vlrReeRepRes', 'Valor', 'O valor fora da base de cálculo.'),
  ], [('DFeNacional', 'DocumentoFiscal', 'DocumentoNaoFiscal')])

C('DFeNacional', 'O documento fiscal eletrônico do repositório nacional (<c>dFeNacional</c>).',
  [
    E('tipoChaveDFe', 'Tipo', 'TipoDFe', 0, 'O tipo do documento.'),
    S('xTipoChaveDFe', 'DescricaoTipo', 'A descrição do documento, quando o tipo é "outro"; opcional.'),
    S('chaveDFe', 'Chave', 'A chave do documento.'),
  ])

C('DocumentoFiscalOutro', 'O documento fiscal fora do repositório nacional (<c>docFiscalOutro</c>).',
  [
    I('cMunDocFiscal', 'Municipio', 'O código IBGE do município emissor.'),
    S('nDocFiscal', 'Numero', 'O número do documento.'),
    S('xDocFiscal', 'Descricao', 'A descrição do documento.'),
  ])

C('DocumentoOutro', 'O documento não fiscal (<c>docOutro</c>).',
  [
    S('nDoc', 'Numero', 'O número do documento.'),
    S('xDoc', 'Descricao', 'A descrição do documento.'),
  ])

C('FornecedorDocumento', 'O fornecedor do documento de reembolso (<c>fornec</c>, <c>TCRTCListaDocFornec</c>).',
  identificacao('do fornecedor') + [S('xNome', 'Nome', 'O nome ou a razão social.')], [ESCOLHA_DOC])

C('TributosIbsCbs', 'Os tributos IBS e CBS declarados (<c>IBSCBS/valores/trib</c>).',
  [GR('gIBSCBS', 'Classificacao', 'ClassificacaoIbsCbs', 'A situação e a classificação tributária.')])

C('ClassificacaoIbsCbs', 'A situação e a classificação tributária do IBS e da CBS (<c>gIBSCBS</c>).',
  [
    S('CST', 'Cst', 'O código de situação tributária (3 dígitos).'),
    S('cClassTrib', 'ClassificacaoTributaria', 'O código de classificação tributária (6 dígitos).'),
    S('cCredPres', 'CreditoPresumido', 'O código do crédito presumido (2 dígitos); opcional.'),
    G('gTribRegular', 'TributacaoRegular', 'TributacaoRegularIbsCbs', 'A tributação regular; opcional.'),
    G('gDif', 'Diferimento', 'DiferimentoIbsCbs', 'O diferimento; opcional.'),
  ])

C('TributacaoRegularIbsCbs', 'A tributação regular do IBS e da CBS (<c>gTribRegular</c> da DPS).',
  [
    S('CSTReg', 'Cst', 'O código de situação tributária da tributação regular.'),
    S('cClassTribReg', 'ClassificacaoTributaria', 'O código de classificação tributária da tributação regular.'),
  ])

C('DiferimentoIbsCbs', 'O diferimento do IBS e da CBS (<c>gDif</c>).',
  [
    D('pDifUF', 'PercentualUF', 'O percentual de diferimento do IBS estadual.'),
    D('pDifMun', 'PercentualMunicipio', 'O percentual de diferimento do IBS municipal.'),
    D('pDifCBS', 'PercentualCbs', 'O percentual de diferimento da CBS.'),
  ])

# ---------------------------------------------------------------- NFS-e

C('NotaFiscalServico', 'A NFS-e do padrão nacional: o <c>NFSe</c> e o <c>infNFSe</c> juntos (docs/NFSE.md 7, M3), com a DPS de dentro. Devolvida pela leitura; quem a gera é o ambiente nacional ou o município. A chave fica no resultado da leitura.',
  [
    S('xLocEmi', 'NomeMunicipioEmissor', 'O nome do município emissor.'),
    S('xLocPrestacao', 'NomeLocalPrestacao', 'O nome do local da prestação.'),
    L('nNFSe', 'Numero', 'O número da NFS-e, sequencial por emitente.'),
    I('cLocIncid', 'MunicipioIncidencia', 'O código IBGE do município de incidência do ISSQN; opcional.'),
    S('xLocIncid', 'NomeMunicipioIncidencia', 'O nome do município de incidência do ISSQN; opcional.'),
    S('xTribNac', 'DescricaoTributacaoNacional', 'A descrição do código de tributação nacional.'),
    S('xTribMun', 'DescricaoTributacaoMunicipal', 'A descrição do código de tributação municipal; opcional.'),
    S('xNBS', 'DescricaoNbs', 'A descrição do código NBS; opcional.'),
    S('verAplic', 'VersaoAplicativo', 'A versão do aplicativo que gerou a NFS-e.'),
    E('ambGer', 'AmbienteGerador', 'AmbienteGerador', 0, 'O ambiente que gerou a NFS-e.'),
    E('tpEmis', 'TipoEmissao', 'TipoEmissaoNFSe', 0, 'O tipo de emissão.'),
    E('procEmi', 'ProcessoEmissao', 'ProcessoEmissao', 0, 'O processo de emissão da DPS; opcional.'),
    E('cStat', 'Situacao', 'SituacaoNFSe', 0, 'A situação da NFS-e.'),
    DH('dhProc', 'Processamento', 'A data e a hora do processamento (a geração da NFS-e).'),
    L('nDFSe', 'NumeroDfse', 'O número do documento no ambiente gerador.'),
    GR('emit', 'Emitente', 'EmitenteNFSe', 'O emitente da NFS-e.'),
    GR('valores', 'Valores', 'ValoresNFSe', 'Os valores calculados pelo ambiente gerador.'),
    S('xOutInf', 'InformacoesFisco', 'O texto de uso da administração tributária municipal (na versão 1.00, dentro de <c>valores</c>); opcional.'),
    G('IBSCBS', 'IbsCbs', 'IbsCbsNFSe', 'O IBS e a CBS calculados pelo ambiente gerador; opcional.'),
    GR('DPS', 'Dps', 'Dps', 'A DPS que gerou a NFS-e.'),
  ])

C('EmitenteNFSe', 'O emitente da NFS-e (<c>emit</c>).',
  [
    S('CNPJ', 'Cnpj', 'O CNPJ; um dos dois (CNPJ ou CPF).'),
    S('CPF', 'Cpf', 'O CPF.'),
    S('IM', 'InscricaoMunicipal', 'A inscrição municipal; opcional.'),
    S('xNome', 'Nome', 'O nome ou a razão social.'),
    S('xFant', 'NomeFantasia', 'O nome fantasia; opcional.'),
    GR('enderNac', 'Endereco', 'EnderecoEmitente', 'O endereço nacional.'),
    S('fone', 'Telefone', 'O telefone; opcional.'),
    S('email', 'Email', 'O e-mail; opcional.'),
  ], [('Cnpj', 'Cpf')])

C('EnderecoEmitente', 'O endereço nacional do emitente da NFS-e (<c>enderNac</c>).',
  endereco_resto() + [
    I('cMun', 'Municipio', 'O código IBGE do município.'),
    UF('UF', 'UF', 'A UF.'),
    S('CEP', 'Cep', 'O CEP, só os dígitos.'),
  ])

C('ValoresNFSe', 'Os valores calculados pelo ambiente gerador (<c>valores</c>, <c>TCValoresNFSe</c>).',
  [
    D('vCalcDR', 'DeducaoReducao', 'O valor de dedução ou redução da base de cálculo; opcional.'),
    E('tpBM', 'TipoBeneficio', 'TipoBeneficio', 0, 'O tipo do benefício municipal (na versão 1.00, texto livre: a leitura aproveita só os códigos 1 a 4, docs/NFSE.md 7, M10); opcional.'),
    D('vCalcBM', 'ReducaoBeneficio', 'O valor da redução pelo benefício municipal; opcional.'),
    D('vBC', 'BaseCalculo', 'A base de cálculo do ISSQN; opcional.'),
    D('pAliqAplic', 'AliquotaAplicada', 'A alíquota aplicada (%); opcional.'),
    D('vISSQN', 'Issqn', 'O valor do ISSQN; opcional.'),
    D('vTotalRet', 'TotalRetencoes', 'O total das retenções; opcional.'),
    D('vLiq', 'Liquido', 'O valor líquido.'),
  ])

C('IbsCbsNFSe', 'O IBS e a CBS calculados pelo ambiente gerador (<c>IBSCBS</c>, <c>TCRTCIBSCBS</c>).',
  [
    I('cLocalidadeIncid', 'MunicipioIncidencia', 'O código IBGE da localidade de incidência.'),
    S('xLocalidadeIncid', 'NomeMunicipioIncidencia', 'O nome da localidade de incidência.'),
    D('pRedutor', 'PercentualRedutor', 'O percentual de redução de alíquota em compra governamental; opcional.'),
    GR('valores', 'Valores', 'ValoresIbsCbsNFSe', 'A base de cálculo e as alíquotas.'),
    GR('totCIBS', 'Totais', 'TotaisIbsCbs', 'Os totais.'),
  ])

C('ValoresIbsCbsNFSe', 'A base de cálculo e as alíquotas do IBS e da CBS (<c>IBSCBS/valores</c> da NFS-e).',
  [
    D('vBC', 'BaseCalculo', 'A base de cálculo.'),
    D('vCalcReeRepRes', 'ReembolsoRepasse', 'O total de reembolsos, repasses e ressarcimentos; opcional.'),
    GR('uf', 'UF', 'AliquotaIbsUF', 'O IBS estadual.'),
    GR('mun', 'Municipio', 'AliquotaIbsMunicipio', 'O IBS municipal.'),
    GR('fed', 'Cbs', 'AliquotaCbs', 'A CBS.'),
  ])

for nome, alvo, sfx in (('AliquotaIbsUF', 'do IBS estadual', 'UF'), ('AliquotaIbsMunicipio', 'do IBS municipal', 'Mun'), ('AliquotaCbs', 'da CBS', 'CBS')):
    t = {'UF': ('pIBSUF', 'pRedAliqUF', 'pAliqEfetUF'), 'Mun': ('pIBSMun', 'pRedAliqMun', 'pAliqEfetMun'), 'CBS': ('pCBS', 'pRedAliqCBS', 'pAliqEfetCBS')}[sfx]
    C(nome, f'As alíquotas {alvo}.',
      [
        D(t[0], 'Aliquota', 'A alíquota (%).'),
        D(t[1], 'Reducao', 'O percentual de redução da alíquota; opcional.'),
        D(t[2], 'Efetiva', 'A alíquota efetiva (%).'),
      ])

C('TotaisIbsCbs', 'Os totais do IBS e da CBS (<c>totCIBS</c>).',
  [
    D('vTotNF', 'TotalNota', 'O valor total da nota com o IBS e a CBS por fora.'),
    GR('gIBS', 'Ibs', 'TotalIbs', 'O IBS.'),
    GR('gCBS', 'Cbs', 'TotalCbs', 'A CBS.'),
    G('gTribRegular', 'TributacaoRegular', 'TotalTributacaoRegular', 'A tributação regular; opcional.'),
    G('gTribCompraGov', 'CompraGovernamental', 'TotalCompraGovernamental', 'A composição em compra governamental; opcional.'),
  ])

C('TotalIbs', 'Os totais do IBS (<c>gIBS</c>).',
  [
    D('vIBSTot', 'Total', 'O IBS total (estadual mais municipal).'),
    G('gIBSCredPres', 'CreditoPresumido', 'CreditoPresumidoIbs', 'O crédito presumido; opcional.'),
    GR('gIBSUFTot', 'UF', 'TotalIbsUF', 'O IBS estadual.'),
    GR('gIBSMunTot', 'Municipio', 'TotalIbsMunicipio', 'O IBS municipal.'),
  ])

C('CreditoPresumidoIbs', 'O crédito presumido do IBS (<c>gIBSCredPres</c>).',
  [D('pCredPresIBS', 'Aliquota', 'A alíquota (%).'), D('vCredPresIBS', 'Valor', 'O valor.')])
C('TotalIbsUF', 'O total do IBS estadual (<c>gIBSUFTot</c>).',
  [D('vDifUF', 'Diferimento', 'O diferimento; opcional.'), D('vIBSUF', 'Valor', 'O valor.')])
C('TotalIbsMunicipio', 'O total do IBS municipal (<c>gIBSMunTot</c>).',
  [D('vDifMun', 'Diferimento', 'O diferimento; opcional.'), D('vIBSMun', 'Valor', 'O valor.')])

C('TotalCbs', 'Os totais da CBS (<c>gCBS</c>).',
  [
    G('gCBSCredPres', 'CreditoPresumido', 'CreditoPresumidoCbs', 'O crédito presumido; opcional.'),
    D('vDifCBS', 'Diferimento', 'O diferimento; opcional.'),
    D('vCBS', 'Valor', 'O valor.'),
  ])

C('CreditoPresumidoCbs', 'O crédito presumido da CBS (<c>gCBSCredPres</c>).',
  [D('pCredPresCBS', 'Aliquota', 'A alíquota (%).'), D('vCredPresCBS', 'Valor', 'O valor.')])

C('TotalTributacaoRegular', 'A tributação regular calculada (<c>gTribRegular</c> da NFS-e).',
  [
    D('pAliqEfeRegIBSUF', 'AliquotaIbsUF', 'A alíquota efetiva do IBS estadual (%).'),
    D('vTribRegIBSUF', 'ValorIbsUF', 'O valor do IBS estadual.'),
    D('pAliqEfeRegIBSMun', 'AliquotaIbsMunicipio', 'A alíquota efetiva do IBS municipal (%).'),
    D('vTribRegIBSMun', 'ValorIbsMunicipio', 'O valor do IBS municipal.'),
    D('pAliqEfeRegCBS', 'AliquotaCbs', 'A alíquota efetiva da CBS (%).'),
    D('vTribRegCBS', 'ValorCbs', 'O valor da CBS.'),
  ])

C('TotalCompraGovernamental', 'A composição do IBS e da CBS em compra governamental (<c>gTribCompraGov</c>).',
  [
    D('pIBSUF', 'AliquotaIbsUF', 'A alíquota do IBS estadual (%).'),
    D('vIBSUF', 'ValorIbsUF', 'O valor do IBS estadual.'),
    D('pIBSMun', 'AliquotaIbsMunicipio', 'A alíquota do IBS municipal (%).'),
    D('vIBSMun', 'ValorIbsMunicipio', 'O valor do IBS municipal.'),
    D('pCBS', 'AliquotaCbs', 'A alíquota da CBS (%).'),
    D('vCBS', 'ValorCbs', 'O valor da CBS.'),
  ])

# ---------------------------------------------------------------- eventos (as subclasses de EventoNFSe)
# (classe, tipo, tag, doc, campos, texto 1.01, texto 1.00, de quem emite)
EVENTOS = [
  ('CancelamentoNFSe', '101101', 'O cancelamento da NFS-e por quem a emitiu (<c>e101101</c>).',
   [E('cMotivo', 'Motivo', 'MotivoCancelamento', 0, 'O motivo do cancelamento.'), S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Cancelamento de NFS-e', 'Cancelamento de NFS-e', True),
  ('CancelamentoSubstituicao', '105102', 'O cancelamento por substituição (<c>e105102</c>), gerado pela Sefin Nacional na emissão da DPS com a substituição: a lib só o lê.',
   [E('cMotivo', 'Motivo', 'MotivoSubstituicao', 2, 'O motivo da substituição.'), S('xMotivo', 'Descricao', 'A descrição do motivo; opcional.'), S('chSubstituta', 'ChaveSubstituta', 'A chave da NFS-e substituta.')],
   'Cancelamento de NFS-e por Substituição', 'Cancelamento de NFS-e por Substituicao', False),
  ('AnaliseFiscalCancelamento', '101103', 'A solicitação de análise fiscal para cancelamento (<c>e101103</c>), fora do prazo do cancelamento.',
   [E('cMotivo', 'Motivo', 'MotivoAnaliseFiscal', 0, 'O motivo da solicitação.'), S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Solicitação de Análise Fiscal para Cancelamento de NFS-e', 'Solicitacao de Analise Fiscal para Cancelamento de NFS-e', True),
  ('CancelamentoDeferido', '105104', 'O cancelamento deferido por análise fiscal (<c>e105104</c>), do município: a lib só o lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('nProcAdm', 'ProcessoAdministrativo', 'O número do processo administrativo; opcional.'),
    E('cMotivo', 'Motivo', 'MotivoDeferimento', 0, 'A resposta da análise.'), S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Cancelamento de NFS-e Deferido por Análise Fiscal', 'Cancelamento de NFS-e Deferido por Análise Fiscal', False),
  ('CancelamentoIndeferido', '105105', 'O cancelamento indeferido por análise fiscal (<c>e105105</c>), do município: a lib só o lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('nProcAdm', 'ProcessoAdministrativo', 'O número do processo administrativo; opcional.'),
    E('cMotivo', 'Motivo', 'MotivoIndeferimento', 0, 'A resposta da análise.'), S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Cancelamento de NFS-e Indeferido por Análise Fiscal', 'Cancelamento de NFS-e Indeferido por Análise Fiscal', False),
  ('ConfirmacaoPrestador', '202201', 'A confirmação do prestador (<c>e202201</c>), manifestação ao ADN: a lib só a lê.', [],
   'Manifestação de NFS-e - Confirmação do Prestador', 'Confirmação do Prestador', False),
  ('ConfirmacaoTomador', '203202', 'A confirmação do tomador (<c>e203202</c>), manifestação ao ADN: a lib só a lê.', [],
   'Manifestação de NFS-e - Confirmação do Tomador', 'Confirmação do Tomador', False),
  ('ConfirmacaoIntermediario', '204203', 'A confirmação do intermediário (<c>e204203</c>), manifestação ao ADN: a lib só a lê.', [],
   'Manifestação de NFS-e - Confirmação do Intermediário', 'Confirmação do Intermediário', False),
  ('ConfirmacaoTacita', '205204', 'A confirmação tácita (<c>e205204</c>), do ADN: a lib só a lê.', [],
   'Manifestação de NFS-e - Confirmação Tácita', 'Confirmação Tácita', False),
  ('RejeicaoPrestador', '202205', 'A rejeição do prestador (<c>e202205</c>), manifestação ao ADN: a lib só a lê.',
   [E('cMotivo', 'Motivo', 'MotivoRejeicao', 0, 'O motivo da rejeição.'), S('xMotivo', 'Descricao', 'A descrição do motivo; opcional.')],
   'Manifestação de NFS-e - Rejeição do Prestador', 'Rejeição do Prestador', False),
  ('RejeicaoTomador', '203206', 'A rejeição do tomador (<c>e203206</c>), manifestação ao ADN: a lib só a lê.',
   [E('cMotivo', 'Motivo', 'MotivoRejeicao', 0, 'O motivo da rejeição.'), S('xMotivo', 'Descricao', 'A descrição do motivo; opcional.')],
   'Manifestação de NFS-e - Rejeição do Tomador', 'Rejeição do Tomador', False),
  ('RejeicaoIntermediario', '204207', 'A rejeição do intermediário (<c>e204207</c>), manifestação ao ADN: a lib só a lê.',
   [E('cMotivo', 'Motivo', 'MotivoRejeicao', 0, 'O motivo da rejeição.'), S('xMotivo', 'Descricao', 'A descrição do motivo; opcional.')],
   'Manifestação de NFS-e - Rejeição do Intermediário', 'Rejeição do Intermediário', False),
  ('AnulacaoRejeicao', '205208', 'A anulação da rejeição (<c>e205208</c>), do município: a lib só a lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('idEvManifRej', 'EventoRejeicao', 'O identificador do evento de rejeição anulado (59 dígitos).'),
    S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Manifestação de NFS-e - Anulação da Rejeição', 'Anulação da Rejeição', False),
  ('CancelamentoOficio', '305101', 'O cancelamento por ofício (<c>e305101</c>), do município: a lib só o lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('nProcAdm', 'ProcessoAdministrativo', 'O número do processo administrativo.'),
    S('xProcAdm', 'DescricaoProcesso', 'A descrição do processo.')],
   'Cancelamento de NFS-e por Ofício', 'Cancelamento de NFS-e por Ofício', False),
  ('BloqueioOficio', '305102', 'O bloqueio por ofício (<c>e305102</c>), do município: a lib só o lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('codEvento', 'EventoBloqueado', 'O evento bloqueado (<c>e101101</c>, <c>e105102</c>, <c>e105104</c>, <c>e105105</c> ou <c>e305101</c>).'),
    S('xMotivo', 'Descricao', 'A descrição do motivo.')],
   'Bloqueio de NFS-e por Ofício', 'Bloqueio de NFS-e por Ofício', False),
  ('DesbloqueioOficio', '305103', 'O desbloqueio por ofício (<c>e305103</c>), do município: a lib só o lê.',
   [S('CPFAgTrib', 'CpfAgente', 'O CPF do agente da administração tributária.'), S('idBloqOfic', 'Bloqueio', 'O identificador do bloqueio desfeito (59 dígitos).')],
   'Desbloqueio de NFS-e por Ofício', 'Desbloqueio de NFS-e por Ofício', False),
]

# ---------------------------------------------------------------- enumerações
# (nome, doc, [(valor, Membro, doc)])

EN('EmitenteDps', 'Quem emite a DPS (<c>tpEmit</c>).', [(1, 'Prestador', 'O prestador'), (2, 'Tomador', 'O tomador'), (3, 'Intermediario', 'O intermediário')])
EN('MotivoEmissaoTerceiro', 'O motivo da emissão da DPS pelo tomador ou pelo intermediário (<c>cMotivoEmisTI</c>).',
   [(1, 'ImportacaoServico', 'Importação de serviço'), (2, 'ObrigacaoLegal', 'Tomador ou intermediário obrigado a emitir pela legislação municipal'),
    (3, 'RecusaPrestador', 'Emissão por recusa do prestador'), (4, 'RejeicaoNFSe', 'Emissão por rejeitar a NFS-e emitida pelo prestador')])
EN('MotivoSubstituicao', 'O motivo da substituição da NFS-e (<c>cMotivo</c> do <c>subst</c> e do evento 105102). O XML leva dois dígitos.',
   [(1, 'DesenquadramentoSimples', 'Desenquadramento do Simples Nacional'), (2, 'EnquadramentoSimples', 'Enquadramento no Simples Nacional'),
    (3, 'InclusaoImunidadeIsencao', 'Inclusão retroativa de imunidade ou isenção'), (4, 'ExclusaoImunidadeIsencao', 'Exclusão retroativa de imunidade ou isenção'),
    (5, 'RejeicaoTomador', 'Rejeição pelo tomador ou pelo intermediário responsável pelo recolhimento'), (99, 'Outros', 'Outros')])
EN('MotivoAusenciaNif', 'O motivo de não informar o NIF (<c>cNaoNIF</c>).',
   [(0, 'NaoInformado', 'Não informado na nota de origem'), (1, 'Dispensado', 'Dispensado do NIF'), (2, 'NaoExigido', 'Não exigência do NIF')])
EN('SituacaoSimplesNacional', 'A situação perante o Simples Nacional (<c>opSimpNac</c>).',
   [(1, 'NaoOptante', 'Não optante'), (2, 'Mei', 'Optante, microempreendedor individual (MEI)'), (3, 'MeEpp', 'Optante, microempresa ou empresa de pequeno porte (ME/EPP)')])
EN('RegimeApuracaoSimples', 'O regime de apuração do optante ME/EPP que ultrapassou um sublimite (<c>regApTribSN</c>).',
   [(1, 'Simples', 'Tributos federais e municipal pelo Simples Nacional'), (2, 'IssqnFora', 'Federais pelo Simples Nacional e o ISSQN por fora'),
    (3, 'TodosFora', 'Federais e municipal por fora do Simples Nacional')])
EN('RegimeEspecialTributacao', 'O regime especial de tributação (<c>regEspTrib</c>).',
   [(0, 'Nenhum', 'Nenhum'), (1, 'AtoCooperado', 'Ato cooperado (cooperativa)'), (2, 'Estimativa', 'Estimativa'), (3, 'MicroempresaMunicipal', 'Microempresa municipal'),
    (4, 'NotarioRegistrador', 'Notário ou registrador'), (5, 'ProfissionalAutonomo', 'Profissional autônomo'), (6, 'SociedadeProfissionais', 'Sociedade de profissionais'), (9, 'Outros', 'Outros')])
EN('ModoPrestacao', 'O modo de prestação do serviço ao exterior (<c>mdPrestacao</c>).',
   [(0, 'Desconhecido', 'Desconhecido (não informado na nota de origem)'), (1, 'Transfronteirico', 'Transfronteiriço'), (2, 'ConsumoBrasil', 'Consumo no Brasil'),
    (3, 'PresencaComercialExterior', 'Presença comercial no exterior'), (4, 'MovimentoTemporarioPessoas', 'Movimento temporário de pessoas físicas')])
EN('VinculoPartes', 'O vínculo entre as partes no negócio (<c>vincPrest</c>).',
   [(0, 'Nenhum', 'Sem vínculo'), (1, 'Controlada', 'Controlada'), (2, 'Controladora', 'Controladora'), (3, 'Coligada', 'Coligada'), (4, 'Matriz', 'Matriz'),
    (5, 'Filial', 'Filial ou sucursal'), (6, 'Outro', 'Outro vínculo'), (9, 'Desconhecido', 'Desconhecido (não informado na nota de origem)')])
EN('MecanismoFomentoPrestador', 'O mecanismo de apoio ou fomento ao comércio exterior do prestador (<c>mecAFComexP</c>). O XML leva dois dígitos.',
   [(0, 'Desconhecido', 'Desconhecido (não informado na nota de origem)'), (1, 'Nenhum', 'Nenhum'), (2, 'Acc', 'ACC, adiantamento sobre contrato de câmbio'),
    (3, 'Ace', 'ACE, adiantamento sobre cambiais entregues'), (4, 'BndesEximPosEmbarque', 'BNDES-Exim pós-embarque, serviços'),
    (5, 'BndesEximPreEmbarque', 'BNDES-Exim pré-embarque, serviços'), (6, 'Fge', 'FGE, Fundo de Garantia à Exportação'), (7, 'ProexEqualizacao', 'PROEX, equalização'),
    (8, 'ProexFinanciamento', 'PROEX, financiamento')])
EN('MecanismoFomentoTomador', 'O mecanismo de apoio ou fomento ao comércio exterior do tomador (<c>mecAFComexT</c>). O XML leva dois dígitos.',
   [(0, 'Desconhecido', 'Desconhecido (não informado na nota de origem)'), (1, 'Nenhum', 'Nenhum'), (2, 'AdministracaoPublica', 'Administração pública e representação internacional'),
    (3, 'AlugueisArrendamento', 'Aluguéis e arrendamento mercantil de máquinas, equipamentos, embarcações e aeronaves'),
    (4, 'ArrendamentoAeronave', 'Arrendamento mercantil de aeronave para empresa de transporte aéreo público'),
    (5, 'ComissaoAgentes', 'Comissão a agentes externos na exportação'), (6, 'DespesasCargaExterior', 'Despesas de armazenagem, movimentação e transporte de carga no exterior'),
    (7, 'EventosFifaSubsidiaria', 'Eventos FIFA (subsidiária)'), (8, 'EventosFifa', 'Eventos FIFA'), (9, 'Fretes', 'Fretes, arrendamentos de embarcações ou aeronaves e outros'),
    (10, 'MaterialAeronautico', 'Material aeronáutico'), (11, 'PromocaoBens', 'Promoção de bens no exterior'), (12, 'PromocaoDestinosTuristicos', 'Promoção de destinos turísticos brasileiros'),
    (13, 'PromocaoBrasil', 'Promoção do Brasil no exterior'), (14, 'PromocaoServicos', 'Promoção de serviços no exterior'), (15, 'Recine', 'RECINE'), (16, 'Recopa', 'RECOPA'),
    (17, 'MarcasPatentes', 'Registro e manutenção de marcas, patentes e cultivares'), (18, 'Reicomp', 'REICOMP'), (19, 'Reidi', 'REIDI'), (20, 'Repenec', 'REPENEC'),
    (21, 'Repes', 'REPES'), (22, 'Retaero', 'RETAERO'), (23, 'Retid', 'RETID'), (24, 'Royalties', 'Royalties, assistência técnica, científica e assemelhados'),
    (25, 'AvaliacaoConformidade', 'Serviços de avaliação da conformidade vinculados aos acordos da OMC'), (26, 'Zpe', 'ZPE')])
EN('MovimentacaoTemporariaBens', 'O vínculo da operação à movimentação temporária de bens (<c>movTempBens</c>).',
   [(0, 'Desconhecido', 'Desconhecido (não informado na nota de origem)'), (1, 'Nao', 'Não'), (2, 'VinculadaImportacao', 'Vinculada à declaração de importação'),
    (3, 'VinculadaExportacao', 'Vinculada à declaração de exportação')])
EN('CompartilhamentoMdic', 'Se as informações da NFS-e vão para a Secretaria de Comércio Exterior (<c>mdic</c>).', [(0, 'Nao', 'Não enviar'), (1, 'Sim', 'Enviar')])
EN('CategoriaLocacao', 'A categoria do serviço de locação (<c>categ</c>, só na versão 1.00).',
   [(1, 'Locacao', 'Locação'), (2, 'Sublocacao', 'Sublocação'), (3, 'Arrendamento', 'Arrendamento'), (4, 'DireitoPassagem', 'Direito de passagem'), (5, 'PermissaoUso', 'Permissão de uso')])
EN('ObjetoLocacao', 'O objeto da locação (<c>objeto</c>, só na versão 1.00).',
   [(1, 'Ferrovia', 'Ferrovia'), (2, 'Rodovia', 'Rodovia'), (3, 'Postes', 'Postes'), (4, 'Cabos', 'Cabos'), (5, 'Dutos', 'Dutos'), (6, 'Condutos', 'Condutos de qualquer natureza')])
EN('CategoriaVeiculo', 'A categoria do veículo no pedágio (<c>categVeic</c>, só na versão 1.00). O XML leva dois dígitos.',
   [(0, 'NaoInformada', 'Não informada na nota de origem'), (1, 'Automovel', 'Automóvel, caminhonete e furgão'), (2, 'CaminhaoLeve', 'Caminhão leve, ônibus, caminhão trator e furgão'),
    (3, 'AutomovelSemirreboque', 'Automóvel e caminhonete com semirreboque'), (4, 'Caminhao', 'Caminhão, caminhão-trator com semirreboque e ônibus'),
    (5, 'AutomovelReboque', 'Automóvel e caminhonete com reboque'), (6, 'CaminhaoReboque', 'Caminhão com reboque'), (7, 'CaminhaoTratorSemirreboque', 'Caminhão trator com semirreboque'),
    (8, 'Motocicleta', 'Motocicletas, motonetas e bicicletas motorizadas'), (9, 'Especial', 'Veículo especial'), (10, 'Isento', 'Veículo isento')])
EN('TipoRodagem', 'O tipo de rodagem no pedágio (<c>rodagem</c>, só na versão 1.00).', [(1, 'Simples', 'Simples'), (2, 'Dupla', 'Dupla')])
EN('TipoDeducao', 'O tipo da dedução ou da redução (<c>tpDedRed</c>).',
   [(1, 'AlimentacaoBebidas', 'Alimentação e bebidas, frigobar'), (2, 'Materiais', 'Materiais'), (3, 'ProducaoExterna', 'Produção externa'), (4, 'ReembolsoDespesas', 'Reembolso de despesas'),
    (5, 'RepasseConsorciado', 'Repasse consorciado'), (6, 'RepassePlanoSaude', 'Repasse plano de saúde'), (7, 'Servicos', 'Serviços'), (8, 'Subempreitada', 'Subempreitada de mão de obra'),
    (9, 'ProfissionalParceiro', 'Profissional parceiro'), (99, 'Outras', 'Outras deduções')])
EN('TributacaoIssqn', 'A tributação do ISSQN (<c>tribISSQN</c>), pela versão 1.01 e o Anexo I. O XSD 1.00 dá outro texto para os códigos 2 a 4 (2 exportação, 3 não incidência, 4 imunidade): a leitura da 1.00 guarda o número como veio, sem traduzir (docs/NFSE.md 7, M12).',
   [(1, 'Tributavel', 'Operação tributável'), (2, 'Imunidade', 'Imunidade'), (3, 'Exportacao', 'Exportação de serviço'), (4, 'NaoIncidencia', 'Não incidência')])
EN('TipoImunidade', 'O tipo da imunidade do ISSQN (<c>tpImunidade</c>).',
   [(0, 'NaoInformada', 'Imunidade, tipo não informado na nota de origem'), (1, 'Reciproca', 'Patrimônio, renda ou serviços, uns dos outros (CF, art. 150, VI, a)'),
    (2, 'Templos', 'Templos de qualquer culto (CF, art. 150, VI, b)'), (3, 'PartidosEntidades', 'Partidos, sindicatos de trabalhadores, educação e assistência social sem fins lucrativos (CF, art. 150, VI, c)'),
    (4, 'LivrosJornais', 'Livros, jornais, periódicos e o papel da impressão (CF, art. 150, VI, d)'), (5, 'Fonogramas', 'Fonogramas e videofonogramas musicais brasileiros (CF, art. 150, VI, e)')])
EN('TipoSuspensao', 'O tipo da suspensão da exigibilidade do ISSQN (<c>tpSusp</c>).', [(1, 'DecisaoJudicial', 'Por decisão judicial'), (2, 'ProcessoAdministrativo', 'Por processo administrativo')])
EN('TipoBeneficioLei', 'O tipo do benefício municipal na DPS (<c>BM/tpBM</c>, só na versão 1.00).', [(1, 'AliquotaDiferenciada', 'Alíquota diferenciada'), (2, 'ReducaoBaseCalculo', 'Redução da base de cálculo'), (3, 'Isencao', 'Isenção')])
EN('RetencaoIssqn', 'A retenção do ISSQN (<c>tpRetISSQN</c>).', [(1, 'NaoRetido', 'Não retido'), (2, 'Tomador', 'Retido pelo tomador'), (3, 'Intermediario', 'Retido pelo intermediário')])
EN('CstPisCofinsServico', 'O código de situação tributária do PIS e da COFINS na NFS-e (<c>CST</c> do <c>piscofins</c>). O XML leva dois dígitos.',
   [(0, 'Nenhum', 'Nenhum'), (1, 'AliquotaBasica', 'Tributável, alíquota básica'), (2, 'AliquotaDiferenciada', 'Tributável, alíquota diferenciada'), (3, 'AliquotaUnidade', 'Tributável, alíquota por unidade'),
    (4, 'MonofasicaRevendaAliquotaZero', 'Monofásica, revenda a alíquota zero'), (5, 'SubstituicaoTributaria', 'Substituição tributária'), (6, 'AliquotaZero', 'Alíquota zero'),
    (7, 'Isenta', 'Isenta'), (8, 'NaoIncidencia', 'Não incidência'), (9, 'Suspensao', 'Suspensão'), (49, 'OutrasSaidas', 'Outras operações de saída'),
    (50, 'CreditoReceitaTributada', 'Crédito, receita tributada no mercado interno'), (51, 'CreditoReceitaNaoTributada', 'Crédito, receita não tributada no mercado interno'),
    (52, 'CreditoReceitaExportacao', 'Crédito, receita de exportação'), (53, 'CreditoReceitasMercadoInterno', 'Crédito, receitas tributadas e não tributadas no mercado interno'),
    (54, 'CreditoReceitasTributadaExportacao', 'Crédito, receitas tributadas no mercado interno e de exportação'), (55, 'CreditoReceitasNaoTributadaExportacao', 'Crédito, receitas não tributadas no mercado interno e de exportação'),
    (56, 'CreditoReceitasTodas', 'Crédito, receitas tributadas e não tributadas no mercado interno e de exportação'),
    (60, 'CreditoPresumidoReceitaTributada', 'Crédito presumido, receita tributada no mercado interno'), (61, 'CreditoPresumidoReceitaNaoTributada', 'Crédito presumido, receita não tributada no mercado interno'),
    (62, 'CreditoPresumidoReceitaExportacao', 'Crédito presumido, receita de exportação'), (63, 'CreditoPresumidoReceitasMercadoInterno', 'Crédito presumido, receitas tributadas e não tributadas no mercado interno'),
    (64, 'CreditoPresumidoReceitasTributadaExportacao', 'Crédito presumido, receitas tributadas no mercado interno e de exportação'),
    (65, 'CreditoPresumidoReceitasNaoTributadaExportacao', 'Crédito presumido, receitas não tributadas no mercado interno e de exportação'),
    (66, 'CreditoPresumidoReceitasTodas', 'Crédito presumido, receitas tributadas e não tributadas no mercado interno e de exportação'),
    (67, 'CreditoPresumidoOutras', 'Crédito presumido, outras operações'), (70, 'AquisicaoCreditoVedado', 'Aquisição sem direito a crédito'), (71, 'AquisicaoIsencao', 'Aquisição com isenção'),
    (72, 'AquisicaoSuspensao', 'Aquisição com suspensão'), (73, 'AquisicaoAliquotaZero', 'Aquisição a alíquota zero'), (74, 'AquisicaoNaoIncidencia', 'Aquisição sem incidência'),
    (75, 'AquisicaoSubstituicao', 'Aquisição por substituição tributária'), (98, 'OutrasEntradas', 'Outras operações de entrada'), (99, 'Outras', 'Outras operações')])
EN('RetencaoPisCofins', 'A retenção do PIS, da COFINS e da CSLL (<c>tpRetPisCofins</c>, NT 007).',
   [(0, 'NenhumRetido', 'PIS, COFINS e CSLL não retidos'), (1, 'PisCofinsRetidos', 'PIS e COFINS retidos'), (2, 'PisCofinsNaoRetidos', 'PIS e COFINS não retidos'),
    (3, 'TodosRetidos', 'PIS, COFINS e CSLL retidos'), (4, 'PisCofinsRetidosCsllNao', 'PIS e COFINS retidos, CSLL não'), (5, 'PisRetido', 'PIS retido, COFINS e CSLL não'),
    (6, 'CofinsRetida', 'COFINS retida, PIS e CSLL não'), (7, 'CofinsCsllRetidas', 'COFINS e CSLL retidas, PIS não'), (8, 'CsllRetida', 'CSLL retida, PIS e COFINS não'),
    (9, 'PisCsllRetidos', 'PIS e CSLL retidos, COFINS não')])
EN('IndicadorTotalTributos', 'O indicador de não informar o total aproximado dos tributos (<c>indTotTrib</c>). O XSD só aceita o 0; o enum fica aberto às NTs (docs/NFSE.md 7, M6).', [(0, 'NaoInformado', 'Não informar nenhum valor estimado (Decreto 8.264/2014)')])
EN('FinalidadeNFSe', 'A finalidade da emissão da NFS-e (<c>finNFSe</c>). O XSD em produção só aceita o 0; a NT 009 acrescenta as notas de ajuste (docs/NFSE.md 7, M6).', [(0, 'Regular', 'NFS-e regular')])
EN('ConsumoPessoal', 'Se a operação é de uso ou consumo pessoal (<c>indFinal</c>, LC 214/2025, art. 57).', [(0, 'Nao', 'Não'), (1, 'Sim', 'Sim')])
EN('TipoOperacaoGoverno', 'O tipo de operação com ente governamental ou sobre bens imóveis (<c>tpOper</c>).',
   [(1, 'FornecimentoPagamentoPosterior', 'Fornecimento com pagamento posterior'), (2, 'PagamentoFornecimentoRealizado', 'Recebimento do pagamento com fornecimento já realizado'),
    (3, 'FornecimentoPagamentoRealizado', 'Fornecimento com pagamento já realizado'), (4, 'PagamentoFornecimentoPosterior', 'Recebimento do pagamento com fornecimento posterior'),
    (5, 'Concomitantes', 'Fornecimento e recebimento do pagamento concomitantes')])
EN('EnteGovernamental', 'O tipo de ente governamental (<c>tpEnteGov</c>).', [(1, 'Uniao', 'União'), (2, 'Estado', 'Estado'), (3, 'DistritoFederal', 'Distrito Federal'), (4, 'Municipio', 'Município')])
EN('IndicadorDestinatario', 'Se o destinatário do serviço é o próprio tomador (<c>indDest</c>).', [(0, 'Tomador', 'O destinatário é o tomador'), (1, 'Outro', 'O destinatário não é o tomador')])
EN('TipoReembolso', 'O tipo do valor de reembolso, repasse ou ressarcimento (<c>tpReeRepRes</c>). O XML leva dois dígitos.',
   [(1, 'RepasseCorretores', 'Repasse de remuneração por intermediação de imóveis a outros corretores'), (2, 'RepasseTurismo', 'Repasse a fornecedor por agência de turismo'),
    (3, 'ReembolsoProducaoExterna', 'Reembolso a agência de propaganda por produção externa'), (4, 'ReembolsoMidia', 'Reembolso a agência de propaganda por mídia'),
    (99, 'Outros', 'Outros reembolsos ou ressarcimentos')])
EN('TipoDFe', 'O tipo do documento fiscal eletrônico do repositório nacional (<c>tipoChaveDFe</c>).', [(1, 'NFSe', 'NFS-e'), (2, 'NFe', 'NF-e'), (3, 'CTe', 'CT-e'), (9, 'Outro', 'Outro')])
EN('AmbienteGerador', 'O ambiente que gerou a NFS-e (<c>ambGer</c>).', [(1, 'Prefeitura', 'O sistema próprio do município'), (2, 'SistemaNacional', 'O Sistema Nacional da NFS-e')])
EN('TipoEmissaoNFSe', 'O tipo de emissão da NFS-e (<c>tpEmis</c>).', [(1, 'Normal', 'Emissão normal no modelo nacional'), (2, 'Transcricao', 'Emissão no leiaute próprio do município, transcrita para o nacional')])
EN('ProcessoEmissao', 'O processo de emissão da DPS (<c>procEmi</c>).', [(1, 'Contribuinte', 'Aplicativo do contribuinte (web service)'), (2, 'FiscoWeb', 'Aplicativo do fisco (web)'), (3, 'FiscoApp', 'Aplicativo do fisco (app)')])
EN('SituacaoNFSe', 'A situação da NFS-e (<c>cStat</c>).', [(100, 'Gerada', 'NFS-e gerada'), (102, 'DecisaoJudicial', 'NFS-e de decisão judicial'), (103, 'Avulsa', 'NFS-e avulsa'), (107, 'Mei', 'NFS-e do MEI')])
EN('TipoBeneficio', 'O tipo do benefício municipal calculado (<c>tpBM</c> dos valores da NFS-e).',
   [(1, 'Isencao', 'Isenção'), (2, 'ReducaoPercentual', 'Redução da base de cálculo em percentual'), (3, 'ReducaoValor', 'Redução da base de cálculo em valor'), (4, 'AliquotaDiferenciada', 'Alíquota diferenciada')])
EN('MotivoCancelamento', 'O motivo do cancelamento da NFS-e (<c>cMotivo</c> do evento 101101).', [(1, 'ErroEmissao', 'Erro na emissão'), (2, 'ServicoNaoPrestado', 'Serviço não prestado'), (9, 'Outros', 'Outros')])
EN('MotivoAnaliseFiscal', 'O motivo da solicitação de análise fiscal para cancelamento (<c>cMotivo</c> do evento 101103).', [(1, 'ErroEmissao', 'Erro na emissão'), (2, 'ServicoNaoPrestado', 'Serviço não prestado'), (9, 'Outros', 'Outros')])
EN('MotivoDeferimento', 'A resposta da análise fiscal que deferiu o cancelamento (<c>cMotivo</c> do evento 105104).', [(1, 'Deferido', 'Cancelamento extemporâneo deferido')])
EN('MotivoIndeferimento', 'A resposta da análise fiscal que indeferiu o cancelamento (<c>cMotivo</c> do evento 105105).', [(1, 'Indeferido', 'Cancelamento extemporâneo indeferido'), (2, 'IndeferidoMerito', 'Indeferido sem análise de mérito')])
EN('MotivoRejeicao', 'O motivo da rejeição da NFS-e na manifestação (<c>cMotivo</c> dos eventos 202205, 203206 e 204207).',
   [(1, 'Duplicidade', 'NFS-e em duplicidade'), (2, 'EmitidaTomador', 'NFS-e já emitida pelo tomador'), (3, 'FatoGeradorInexistente', 'Não ocorrência do fato gerador'),
    (4, 'ResponsabilidadeTributaria', 'Erro quanto à responsabilidade tributária'), (5, 'ErroValores', 'Erro no valor do serviço, das deduções, no serviço ou na data do fato gerador'), (9, 'Outros', 'Outros')])
EN('AmbienteGeradorEvento', 'O ambiente que gerou o evento (<c>ambGer</c> do <c>infEvento</c>).', [(1, 'Prefeitura', 'O sistema próprio do município'), (2, 'SefinNacional', 'A Sefin Nacional'), (3, 'AmbienteNacional', 'O Ambiente de Dados Nacional')])
