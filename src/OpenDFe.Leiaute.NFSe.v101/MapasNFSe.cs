// Gerado por eng/GerarModeloNFSe (docs/NFSE.md 7): o modelo e o mapa do leiaute saem da mesma especificação.

namespace OpenDFe.NFSe;

/// <summary>
/// O mapa do modelo da NFS-e no leiaute 1.01 (docs/NFSE.md 7): para cada classe, os campos na ordem do XSD, com a tag, a
/// propriedade, o tipo e as escolhas. A escrita, a leitura e o caminho do problema andam pelo mesmo mapa. O grupo aponta
/// o mapa dele por uma função, porque os campos estáticos se inicializam na ordem do arquivo; o <c>!</c> diz ao
/// compilador que, quando a função roda, todos já estão prontos.
/// </summary>
internal static class MapasNFSe
{
    internal static readonly MapaGrupo MapaDps = MapaGrupo.Criar<Dps>(
        [
            Campo<Dps>.Enumeracao<Ambiente>("tpAmb", "Ambiente", g => g.Ambiente, (g, v) => g.Ambiente = v, 0),
            Campo<Dps>.DataHora("dhEmi", "Emissao", g => g.Emissao, (g, v) => g.Emissao = v),
            Campo<Dps>.Texto("verAplic", "VersaoAplicativo", g => g.VersaoAplicativo, (g, v) => g.VersaoAplicativo = v),
            Campo<Dps>.Texto("serie", "Serie", g => g.Serie, (g, v) => g.Serie = v),
            Campo<Dps>.Longo("nDPS", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<Dps>.Data("dCompet", "Competencia", g => g.Competencia, (g, v) => g.Competencia = v),
            Campo<Dps>.Enumeracao<EmitenteDps>("tpEmit", "Emitente", g => g.Emitente, (g, v) => g.Emitente = v, 0),
            Campo<Dps>.Enumeracao<MotivoEmissaoTerceiro>("cMotivoEmisTI", "MotivoEmissaoTerceiro", g => g.MotivoEmissaoTerceiro, (g, v) => g.MotivoEmissaoTerceiro = v, 0),
            Campo<Dps>.Texto("chNFSeRej", "ChaveRejeitada", g => g.ChaveRejeitada, (g, v) => g.ChaveRejeitada = v),
            Campo<Dps>.Inteiro("cLocEmi", "MunicipioEmissor", g => g.MunicipioEmissor, (g, v) => g.MunicipioEmissor = v),
            Campo<Dps>.Grupo<Substituicao>("subst", "Substituicao", g => g.Substituicao, (g, v) => g.Substituicao = v, () => MapaSubstituicao!),
            Campo<Dps>.Grupo<Prestador>("prest", "Prestador", g => g.Prestador, (g, v) => g.Prestador = v, () => MapaPrestador!),
            Campo<Dps>.Grupo<Pessoa>("toma", "Tomador", g => g.Tomador, (g, v) => g.Tomador = v, () => MapaPessoa!),
            Campo<Dps>.Grupo<Pessoa>("interm", "Intermediario", g => g.Intermediario, (g, v) => g.Intermediario = v, () => MapaPessoa!),
            Campo<Dps>.Grupo<Servico>("serv", "Servico", g => g.Servico, (g, v) => g.Servico = v, () => MapaServico!),
            Campo<Dps>.Grupo<ValoresDps>("valores", "Valores", g => g.Valores, (g, v) => g.Valores = v, () => MapaValoresDps!),
            Campo<Dps>.Grupo<IbsCbsDps>("IBSCBS", "IbsCbs", g => g.IbsCbs, (g, v) => g.IbsCbs = v, () => MapaIbsCbsDps!),
        ],
        []);

    internal static readonly MapaGrupo MapaSubstituicao = MapaGrupo.Criar<Substituicao>(
        [
            Campo<Substituicao>.Texto("chSubstda", "Chave", g => g.Chave, (g, v) => g.Chave = v),
            Campo<Substituicao>.Enumeracao<MotivoSubstituicao>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 2),
            Campo<Substituicao>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaPrestador = MapaGrupo.Criar<Prestador>(
        [
            Campo<Prestador>.Texto("CNPJ", "Cnpj", g => g.Cnpj, (g, v) => g.Cnpj = v),
            Campo<Prestador>.Texto("CPF", "Cpf", g => g.Cpf, (g, v) => g.Cpf = v),
            Campo<Prestador>.Texto("NIF", "Nif", g => g.Nif, (g, v) => g.Nif = v),
            Campo<Prestador>.Enumeracao<MotivoAusenciaNif>("cNaoNIF", "MotivoAusenciaNif", g => g.MotivoAusenciaNif, (g, v) => g.MotivoAusenciaNif = v, 0),
            Campo<Prestador>.Texto("CAEPF", "Caepf", g => g.Caepf, (g, v) => g.Caepf = v),
            Campo<Prestador>.Texto("IM", "InscricaoMunicipal", g => g.InscricaoMunicipal, (g, v) => g.InscricaoMunicipal = v),
            Campo<Prestador>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v),
            Campo<Prestador>.Grupo<Endereco>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEndereco!),
            Campo<Prestador>.Texto("fone", "Telefone", g => g.Telefone, (g, v) => g.Telefone = v),
            Campo<Prestador>.Texto("email", "Email", g => g.Email, (g, v) => g.Email = v),
            Campo<Prestador>.Grupo<RegimeTributacao>("regTrib", "RegimeTributacao", g => g.RegimeTributacao, (g, v) => g.RegimeTributacao = v, () => MapaRegimeTributacao!),
        ],
        [["Cnpj", "Cpf", "Nif", "MotivoAusenciaNif"]]);

    internal static readonly MapaGrupo MapaRegimeTributacao = MapaGrupo.Criar<RegimeTributacao>(
        [
            Campo<RegimeTributacao>.Enumeracao<SituacaoSimplesNacional>("opSimpNac", "SimplesNacional", g => g.SimplesNacional, (g, v) => g.SimplesNacional = v, 0),
            Campo<RegimeTributacao>.Enumeracao<RegimeApuracaoSimples>("regApTribSN", "ApuracaoSimples", g => g.ApuracaoSimples, (g, v) => g.ApuracaoSimples = v, 0),
            Campo<RegimeTributacao>.Enumeracao<RegimeEspecialTributacao>("regEspTrib", "RegimeEspecial", g => g.RegimeEspecial, (g, v) => g.RegimeEspecial = v, 0),
        ],
        []);

    internal static readonly MapaGrupo MapaPessoa = MapaGrupo.Criar<Pessoa>(
        [
            Campo<Pessoa>.Texto("CNPJ", "Cnpj", g => g.Cnpj, (g, v) => g.Cnpj = v),
            Campo<Pessoa>.Texto("CPF", "Cpf", g => g.Cpf, (g, v) => g.Cpf = v),
            Campo<Pessoa>.Texto("NIF", "Nif", g => g.Nif, (g, v) => g.Nif = v),
            Campo<Pessoa>.Enumeracao<MotivoAusenciaNif>("cNaoNIF", "MotivoAusenciaNif", g => g.MotivoAusenciaNif, (g, v) => g.MotivoAusenciaNif = v, 0),
            Campo<Pessoa>.Texto("CAEPF", "Caepf", g => g.Caepf, (g, v) => g.Caepf = v),
            Campo<Pessoa>.Texto("IM", "InscricaoMunicipal", g => g.InscricaoMunicipal, (g, v) => g.InscricaoMunicipal = v),
            Campo<Pessoa>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v),
            Campo<Pessoa>.Grupo<Endereco>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEndereco!),
            Campo<Pessoa>.Texto("fone", "Telefone", g => g.Telefone, (g, v) => g.Telefone = v),
            Campo<Pessoa>.Texto("email", "Email", g => g.Email, (g, v) => g.Email = v),
        ],
        [["Cnpj", "Cpf", "Nif", "MotivoAusenciaNif"]]);

    internal static readonly MapaGrupo MapaEndereco = MapaGrupo.Criar<Endereco>(
        [
            Campo<Endereco>.Grupo<EnderecoNacional>("endNac", "Nacional", g => g.Nacional, (g, v) => g.Nacional = v, () => MapaEnderecoNacional!),
            Campo<Endereco>.Grupo<EnderecoExterior>("endExt", "Exterior", g => g.Exterior, (g, v) => g.Exterior = v, () => MapaEnderecoExterior!),
            Campo<Endereco>.Texto("xLgr", "Logradouro", g => g.Logradouro, (g, v) => g.Logradouro = v),
            Campo<Endereco>.Texto("nro", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<Endereco>.Texto("xCpl", "Complemento", g => g.Complemento, (g, v) => g.Complemento = v),
            Campo<Endereco>.Texto("xBairro", "Bairro", g => g.Bairro, (g, v) => g.Bairro = v),
        ],
        [["Nacional", "Exterior"]]);

    internal static readonly MapaGrupo MapaEnderecoNacional = MapaGrupo.Criar<EnderecoNacional>(
        [
            Campo<EnderecoNacional>.Inteiro("cMun", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v),
            Campo<EnderecoNacional>.Texto("CEP", "Cep", g => g.Cep, (g, v) => g.Cep = v),
        ],
        []);

    internal static readonly MapaGrupo MapaEnderecoExterior = MapaGrupo.Criar<EnderecoExterior>(
        [
            Campo<EnderecoExterior>.Texto("cPais", "Pais", g => g.Pais, (g, v) => g.Pais = v),
            Campo<EnderecoExterior>.Texto("cEndPost", "CodigoPostal", g => g.CodigoPostal, (g, v) => g.CodigoPostal = v),
            Campo<EnderecoExterior>.Texto("xCidade", "Cidade", g => g.Cidade, (g, v) => g.Cidade = v),
            Campo<EnderecoExterior>.Texto("xEstProvReg", "EstadoProvincia", g => g.EstadoProvincia, (g, v) => g.EstadoProvincia = v),
        ],
        []);

    internal static readonly MapaGrupo MapaEnderecoExteriorSimples = MapaGrupo.Criar<EnderecoExteriorSimples>(
        [
            Campo<EnderecoExteriorSimples>.Texto("cEndPost", "CodigoPostal", g => g.CodigoPostal, (g, v) => g.CodigoPostal = v),
            Campo<EnderecoExteriorSimples>.Texto("xCidade", "Cidade", g => g.Cidade, (g, v) => g.Cidade = v),
            Campo<EnderecoExteriorSimples>.Texto("xEstProvReg", "EstadoProvincia", g => g.EstadoProvincia, (g, v) => g.EstadoProvincia = v),
        ],
        []);

    internal static readonly MapaGrupo MapaEnderecoObra = MapaGrupo.Criar<EnderecoObra>(
        [
            Campo<EnderecoObra>.Texto("CEP", "Cep", g => g.Cep, (g, v) => g.Cep = v),
            Campo<EnderecoObra>.Grupo<EnderecoExteriorSimples>("endExt", "Exterior", g => g.Exterior, (g, v) => g.Exterior = v, () => MapaEnderecoExteriorSimples!),
            Campo<EnderecoObra>.Texto("xLgr", "Logradouro", g => g.Logradouro, (g, v) => g.Logradouro = v),
            Campo<EnderecoObra>.Texto("nro", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<EnderecoObra>.Texto("xCpl", "Complemento", g => g.Complemento, (g, v) => g.Complemento = v),
            Campo<EnderecoObra>.Texto("xBairro", "Bairro", g => g.Bairro, (g, v) => g.Bairro = v),
        ],
        [["Cep", "Exterior"]]);

    internal static readonly MapaGrupo MapaEnderecoEvento = MapaGrupo.Criar<EnderecoEvento>(
        [
            Campo<EnderecoEvento>.Texto("CEP", "Cep", g => g.Cep, (g, v) => g.Cep = v),
            Campo<EnderecoEvento>.Grupo<EnderecoExteriorSimples>("endExt", "Exterior", g => g.Exterior, (g, v) => g.Exterior = v, () => MapaEnderecoExteriorSimples!),
            Campo<EnderecoEvento>.Texto("xLgr", "Logradouro", g => g.Logradouro, (g, v) => g.Logradouro = v),
            Campo<EnderecoEvento>.Texto("nro", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<EnderecoEvento>.Texto("xCpl", "Complemento", g => g.Complemento, (g, v) => g.Complemento = v),
            Campo<EnderecoEvento>.Texto("xBairro", "Bairro", g => g.Bairro, (g, v) => g.Bairro = v),
        ],
        [["Cep", "Exterior"]]);

    internal static readonly MapaGrupo MapaServico = MapaGrupo.Criar<Servico>(
        [
            Campo<Servico>.Grupo<LocalPrestacao>("locPrest", "Local", g => g.Local, (g, v) => g.Local = v, () => MapaLocalPrestacao!),
            Campo<Servico>.Grupo<CodigoServico>("cServ", "Codigo", g => g.Codigo, (g, v) => g.Codigo = v, () => MapaCodigoServico!),
            Campo<Servico>.Grupo<ComercioExterior>("comExt", "ComercioExterior", g => g.ComercioExterior, (g, v) => g.ComercioExterior = v, () => MapaComercioExterior!),
            Campo<Servico>.Grupo<LocacaoSublocacao>("lsadppu", "Locacao", g => g.Locacao, (g, v) => g.Locacao = v, () => MapaLocacaoSublocacao!, versao100: true),
            Campo<Servico>.Grupo<Obra>("obra", "Obra", g => g.Obra, (g, v) => g.Obra = v, () => MapaObra!),
            Campo<Servico>.Grupo<AtividadeEvento>("atvEvento", "Evento", g => g.Evento, (g, v) => g.Evento = v, () => MapaAtividadeEvento!),
            Campo<Servico>.Grupo<ExploracaoRodoviaria>("explRod", "ExploracaoRodoviaria", g => g.ExploracaoRodoviaria, (g, v) => g.ExploracaoRodoviaria = v, () => MapaExploracaoRodoviaria!, versao100: true),
            Campo<Servico>.Grupo<InformacoesComplementares>("infoCompl", "InformacoesComplementares", g => g.InformacoesComplementares, (g, v) => g.InformacoesComplementares = v, () => MapaInformacoesComplementares!),
        ],
        []);

    internal static readonly MapaGrupo MapaLocalPrestacao = MapaGrupo.Criar<LocalPrestacao>(
        [
            Campo<LocalPrestacao>.Inteiro("cLocPrestacao", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v),
            Campo<LocalPrestacao>.Texto("cPaisPrestacao", "Pais", g => g.Pais, (g, v) => g.Pais = v),
        ],
        [["Municipio", "Pais"]]);

    internal static readonly MapaGrupo MapaCodigoServico = MapaGrupo.Criar<CodigoServico>(
        [
            Campo<CodigoServico>.Texto("cTribNac", "TributacaoNacional", g => g.TributacaoNacional, (g, v) => g.TributacaoNacional = v),
            Campo<CodigoServico>.Texto("cTribMun", "TributacaoMunicipal", g => g.TributacaoMunicipal, (g, v) => g.TributacaoMunicipal = v),
            Campo<CodigoServico>.Texto("xDescServ", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
            Campo<CodigoServico>.Texto("cNBS", "Nbs", g => g.Nbs, (g, v) => g.Nbs = v),
            Campo<CodigoServico>.Texto("cIntContrib", "CodigoInterno", g => g.CodigoInterno, (g, v) => g.CodigoInterno = v),
        ],
        []);

    internal static readonly MapaGrupo MapaComercioExterior = MapaGrupo.Criar<ComercioExterior>(
        [
            Campo<ComercioExterior>.Enumeracao<ModoPrestacao>("mdPrestacao", "ModoPrestacao", g => g.ModoPrestacao, (g, v) => g.ModoPrestacao = v, 0),
            Campo<ComercioExterior>.Enumeracao<VinculoPartes>("vincPrest", "Vinculo", g => g.Vinculo, (g, v) => g.Vinculo = v, 0),
            Campo<ComercioExterior>.Texto("tpMoeda", "Moeda", g => g.Moeda, (g, v) => g.Moeda = v),
            Campo<ComercioExterior>.Decimal("vServMoeda", "ValorMoeda", g => g.ValorMoeda, (g, v) => g.ValorMoeda = v, 2),
            Campo<ComercioExterior>.Enumeracao<MecanismoFomentoPrestador>("mecAFComexP", "MecanismoPrestador", g => g.MecanismoPrestador, (g, v) => g.MecanismoPrestador = v, 2),
            Campo<ComercioExterior>.Enumeracao<MecanismoFomentoTomador>("mecAFComexT", "MecanismoTomador", g => g.MecanismoTomador, (g, v) => g.MecanismoTomador = v, 2),
            Campo<ComercioExterior>.Enumeracao<MovimentacaoTemporariaBens>("movTempBens", "MovimentacaoBens", g => g.MovimentacaoBens, (g, v) => g.MovimentacaoBens = v, 0),
            Campo<ComercioExterior>.Texto("nDI", "DeclaracaoImportacao", g => g.DeclaracaoImportacao, (g, v) => g.DeclaracaoImportacao = v),
            Campo<ComercioExterior>.Texto("nRE", "RegistroExportacao", g => g.RegistroExportacao, (g, v) => g.RegistroExportacao = v),
            Campo<ComercioExterior>.Enumeracao<CompartilhamentoMdic>("mdic", "CompartilharMdic", g => g.CompartilharMdic, (g, v) => g.CompartilharMdic = v, 0),
        ],
        []);

    internal static readonly MapaGrupo MapaLocacaoSublocacao = MapaGrupo.Criar<LocacaoSublocacao>(
        [
            Campo<LocacaoSublocacao>.Enumeracao<CategoriaLocacao>("categ", "Categoria", g => g.Categoria, (g, v) => g.Categoria = v, 0),
            Campo<LocacaoSublocacao>.Enumeracao<ObjetoLocacao>("objeto", "Objeto", g => g.Objeto, (g, v) => g.Objeto = v, 0),
            Campo<LocacaoSublocacao>.Inteiro("extensao", "Extensao", g => g.Extensao, (g, v) => g.Extensao = v),
            Campo<LocacaoSublocacao>.Inteiro("nPostes", "Postes", g => g.Postes, (g, v) => g.Postes = v),
        ],
        []);

    internal static readonly MapaGrupo MapaObra = MapaGrupo.Criar<Obra>(
        [
            Campo<Obra>.Texto("inscImobFisc", "InscricaoImobiliaria", g => g.InscricaoImobiliaria, (g, v) => g.InscricaoImobiliaria = v),
            Campo<Obra>.Texto("cObra", "Codigo", g => g.Codigo, (g, v) => g.Codigo = v),
            Campo<Obra>.Texto("cCIB", "Cib", g => g.Cib, (g, v) => g.Cib = v),
            Campo<Obra>.Grupo<EnderecoObra>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEnderecoObra!),
        ],
        [["Codigo", "Cib", "Endereco"]]);

    internal static readonly MapaGrupo MapaAtividadeEvento = MapaGrupo.Criar<AtividadeEvento>(
        [
            Campo<AtividadeEvento>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v, outras: ["desc"]),
            Campo<AtividadeEvento>.Data("dtIni", "Inicio", g => g.Inicio, (g, v) => g.Inicio = v),
            Campo<AtividadeEvento>.Data("dtFim", "Fim", g => g.Fim, (g, v) => g.Fim = v),
            Campo<AtividadeEvento>.Texto("idAtvEvt", "Identificacao", g => g.Identificacao, (g, v) => g.Identificacao = v, outras: ["id"]),
            Campo<AtividadeEvento>.Grupo<EnderecoEvento>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEnderecoEvento!),
        ],
        [["Identificacao", "Endereco"]]);

    internal static readonly MapaGrupo MapaExploracaoRodoviaria = MapaGrupo.Criar<ExploracaoRodoviaria>(
        [
            Campo<ExploracaoRodoviaria>.Enumeracao<CategoriaVeiculo>("categVeic", "Categoria", g => g.Categoria, (g, v) => g.Categoria = v, 2),
            Campo<ExploracaoRodoviaria>.Inteiro("nEixos", "Eixos", g => g.Eixos, (g, v) => g.Eixos = v),
            Campo<ExploracaoRodoviaria>.Enumeracao<TipoRodagem>("rodagem", "Rodagem", g => g.Rodagem, (g, v) => g.Rodagem = v, 0),
            Campo<ExploracaoRodoviaria>.Inteiro("sentido", "Sentido", g => g.Sentido, (g, v) => g.Sentido = v),
            Campo<ExploracaoRodoviaria>.Texto("placa", "Placa", g => g.Placa, (g, v) => g.Placa = v),
            Campo<ExploracaoRodoviaria>.Texto("codAcessoPed", "CodigoAcesso", g => g.CodigoAcesso, (g, v) => g.CodigoAcesso = v),
            Campo<ExploracaoRodoviaria>.Texto("codContrato", "CodigoContrato", g => g.CodigoContrato, (g, v) => g.CodigoContrato = v),
        ],
        []);

    internal static readonly MapaGrupo MapaInformacoesComplementares = MapaGrupo.Criar<InformacoesComplementares>(
        [
            Campo<InformacoesComplementares>.Texto("idDocTec", "ResponsabilidadeTecnica", g => g.ResponsabilidadeTecnica, (g, v) => g.ResponsabilidadeTecnica = v),
            Campo<InformacoesComplementares>.Texto("docRef", "DocumentoReferencia", g => g.DocumentoReferencia, (g, v) => g.DocumentoReferencia = v),
            Campo<InformacoesComplementares>.Texto("xPed", "Pedido", g => g.Pedido, (g, v) => g.Pedido = v),
            Campo<InformacoesComplementares>.Grupo<ItensPedido>("gItemPed", "ItensPedido", g => g.ItensPedido, (g, v) => g.ItensPedido = v, () => MapaItensPedido!),
            Campo<InformacoesComplementares>.Texto("xInfComp", "Texto", g => g.Texto, (g, v) => g.Texto = v),
        ],
        []);

    internal static readonly MapaGrupo MapaItensPedido = MapaGrupo.Criar<ItensPedido>(
        [
            Campo<ItensPedido>.Textos("xItemPed", "Itens", g => g.Itens),
        ],
        []);

    internal static readonly MapaGrupo MapaValoresDps = MapaGrupo.Criar<ValoresDps>(
        [
            Campo<ValoresDps>.Grupo<ValorServico>("vServPrest", "Servico", g => g.Servico, (g, v) => g.Servico = v, () => MapaValorServico!),
            Campo<ValoresDps>.Grupo<Descontos>("vDescCondIncond", "Descontos", g => g.Descontos, (g, v) => g.Descontos = v, () => MapaDescontos!),
            Campo<ValoresDps>.Grupo<DeducaoReducao>("vDedRed", "DeducaoReducao", g => g.DeducaoReducao, (g, v) => g.DeducaoReducao = v, () => MapaDeducaoReducao!),
            Campo<ValoresDps>.Grupo<Tributacao>("trib", "Tributacao", g => g.Tributacao, (g, v) => g.Tributacao = v, () => MapaTributacao!),
        ],
        []);

    internal static readonly MapaGrupo MapaValorServico = MapaGrupo.Criar<ValorServico>(
        [
            Campo<ValorServico>.Decimal("vReceb", "ValorRecebido", g => g.ValorRecebido, (g, v) => g.ValorRecebido = v, 2),
            Campo<ValorServico>.Decimal("vServ", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaDescontos = MapaGrupo.Criar<Descontos>(
        [
            Campo<Descontos>.Decimal("vDescIncond", "Incondicionado", g => g.Incondicionado, (g, v) => g.Incondicionado = v, 2),
            Campo<Descontos>.Decimal("vDescCond", "Condicionado", g => g.Condicionado, (g, v) => g.Condicionado = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaDeducaoReducao = MapaGrupo.Criar<DeducaoReducao>(
        [
            Campo<DeducaoReducao>.Decimal("pDR", "Percentual", g => g.Percentual, (g, v) => g.Percentual = v, 2),
            Campo<DeducaoReducao>.Decimal("vDR", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
            Campo<DeducaoReducao>.Grupo<DocumentosDeducao>("documentos", "Documentos", g => g.Documentos, (g, v) => g.Documentos = v, () => MapaDocumentosDeducao!),
        ],
        [["Percentual", "Valor", "Documentos"]]);

    internal static readonly MapaGrupo MapaDocumentosDeducao = MapaGrupo.Criar<DocumentosDeducao>(
        [
            Campo<DocumentosDeducao>.Lista<DocumentoDeducao>("docDedRed", "Itens", g => g.Itens, () => MapaDocumentoDeducao!),
        ],
        []);

    internal static readonly MapaGrupo MapaDocumentoDeducao = MapaGrupo.Criar<DocumentoDeducao>(
        [
            Campo<DocumentoDeducao>.Texto("chNFSe", "ChaveNFSe", g => g.ChaveNFSe, (g, v) => g.ChaveNFSe = v),
            Campo<DocumentoDeducao>.Texto("chNFe", "ChaveNFe", g => g.ChaveNFe, (g, v) => g.ChaveNFe = v),
            Campo<DocumentoDeducao>.Grupo<NFSeMunicipal>("NFSeMun", "NFSeMunicipal", g => g.NFSeMunicipal, (g, v) => g.NFSeMunicipal = v, () => MapaNFSeMunicipal!),
            Campo<DocumentoDeducao>.Grupo<NotaPapel>("NFNFS", "NotaPapel", g => g.NotaPapel, (g, v) => g.NotaPapel = v, () => MapaNotaPapel!),
            Campo<DocumentoDeducao>.Texto("nDocFisc", "NumeroDocumentoFiscal", g => g.NumeroDocumentoFiscal, (g, v) => g.NumeroDocumentoFiscal = v),
            Campo<DocumentoDeducao>.Texto("nDoc", "NumeroDocumento", g => g.NumeroDocumento, (g, v) => g.NumeroDocumento = v),
            Campo<DocumentoDeducao>.Enumeracao<TipoDeducao>("tpDedRed", "Tipo", g => g.Tipo, (g, v) => g.Tipo = v, 0),
            Campo<DocumentoDeducao>.Texto("xDescOutDed", "DescricaoOutra", g => g.DescricaoOutra, (g, v) => g.DescricaoOutra = v),
            Campo<DocumentoDeducao>.Data("dtEmiDoc", "Emissao", g => g.Emissao, (g, v) => g.Emissao = v),
            Campo<DocumentoDeducao>.Decimal("vDedutivelRedutivel", "ValorDedutivel", g => g.ValorDedutivel, (g, v) => g.ValorDedutivel = v, 2),
            Campo<DocumentoDeducao>.Decimal("vDeducaoReducao", "ValorUtilizado", g => g.ValorUtilizado, (g, v) => g.ValorUtilizado = v, 2),
            Campo<DocumentoDeducao>.Grupo<Pessoa>("fornec", "Fornecedor", g => g.Fornecedor, (g, v) => g.Fornecedor = v, () => MapaPessoa!),
        ],
        [["ChaveNFSe", "ChaveNFe", "NFSeMunicipal", "NotaPapel", "NumeroDocumentoFiscal", "NumeroDocumento"]]);

    internal static readonly MapaGrupo MapaNFSeMunicipal = MapaGrupo.Criar<NFSeMunicipal>(
        [
            Campo<NFSeMunicipal>.Inteiro("cMunNFSeMun", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v),
            Campo<NFSeMunicipal>.Texto("nNFSeMun", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<NFSeMunicipal>.Texto("cVerifNFSeMun", "CodigoVerificacao", g => g.CodigoVerificacao, (g, v) => g.CodigoVerificacao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaNotaPapel = MapaGrupo.Criar<NotaPapel>(
        [
            Campo<NotaPapel>.Texto("nNFS", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<NotaPapel>.Texto("modNFS", "Modelo", g => g.Modelo, (g, v) => g.Modelo = v),
            Campo<NotaPapel>.Texto("serieNFS", "Serie", g => g.Serie, (g, v) => g.Serie = v),
        ],
        []);

    internal static readonly MapaGrupo MapaTributacao = MapaGrupo.Criar<Tributacao>(
        [
            Campo<Tributacao>.Grupo<TributacaoMunicipal>("tribMun", "Municipal", g => g.Municipal, (g, v) => g.Municipal = v, () => MapaTributacaoMunicipal!),
            Campo<Tributacao>.Grupo<TributacaoFederal>("tribFed", "Federal", g => g.Federal, (g, v) => g.Federal = v, () => MapaTributacaoFederal!),
            Campo<Tributacao>.Grupo<TotalTributos>("totTrib", "Total", g => g.Total, (g, v) => g.Total = v, () => MapaTotalTributos!),
        ],
        []);

    internal static readonly MapaGrupo MapaTributacaoMunicipal = MapaGrupo.Criar<TributacaoMunicipal>(
        [
            Campo<TributacaoMunicipal>.Enumeracao<TributacaoIssqn>("tribISSQN", "Tributacao", g => g.Tributacao, (g, v) => g.Tributacao = v, 0),
            Campo<TributacaoMunicipal>.Texto("cPaisResult", "PaisResultado", g => g.PaisResultado, (g, v) => g.PaisResultado = v),
            Campo<TributacaoMunicipal>.Enumeracao<TipoImunidade>("tpImunidade", "Imunidade", g => g.Imunidade, (g, v) => g.Imunidade = v, 0),
            Campo<TributacaoMunicipal>.Grupo<ExigibilidadeSuspensa>("exigSusp", "ExigibilidadeSuspensa", g => g.ExigibilidadeSuspensa, (g, v) => g.ExigibilidadeSuspensa = v, () => MapaExigibilidadeSuspensa!),
            Campo<TributacaoMunicipal>.Grupo<BeneficioMunicipal>("BM", "Beneficio", g => g.Beneficio, (g, v) => g.Beneficio = v, () => MapaBeneficioMunicipal!),
            Campo<TributacaoMunicipal>.Enumeracao<RetencaoIssqn>("tpRetISSQN", "Retencao", g => g.Retencao, (g, v) => g.Retencao = v, 0),
            Campo<TributacaoMunicipal>.Decimal("pAliq", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaExigibilidadeSuspensa = MapaGrupo.Criar<ExigibilidadeSuspensa>(
        [
            Campo<ExigibilidadeSuspensa>.Enumeracao<TipoSuspensao>("tpSusp", "Tipo", g => g.Tipo, (g, v) => g.Tipo = v, 0),
            Campo<ExigibilidadeSuspensa>.Texto("nProcesso", "Processo", g => g.Processo, (g, v) => g.Processo = v),
        ],
        []);

    internal static readonly MapaGrupo MapaBeneficioMunicipal = MapaGrupo.Criar<BeneficioMunicipal>(
        [
            Campo<BeneficioMunicipal>.Enumeracao<TipoBeneficioLei>("tpBM", "Tipo", g => g.Tipo, (g, v) => g.Tipo = v, 0, versao100: true),
            Campo<BeneficioMunicipal>.Texto("nBM", "Identificador", g => g.Identificador, (g, v) => g.Identificador = v),
            Campo<BeneficioMunicipal>.Decimal("vRedBCBM", "ValorReducao", g => g.ValorReducao, (g, v) => g.ValorReducao = v, 2),
            Campo<BeneficioMunicipal>.Decimal("pRedBCBM", "PercentualReducao", g => g.PercentualReducao, (g, v) => g.PercentualReducao = v, 2),
        ],
        [["ValorReducao", "PercentualReducao"]]);

    internal static readonly MapaGrupo MapaTributacaoFederal = MapaGrupo.Criar<TributacaoFederal>(
        [
            Campo<TributacaoFederal>.Grupo<PisCofins>("piscofins", "PisCofins", g => g.PisCofins, (g, v) => g.PisCofins = v, () => MapaPisCofins!),
            Campo<TributacaoFederal>.Decimal("vRetCP", "RetencaoCp", g => g.RetencaoCp, (g, v) => g.RetencaoCp = v, 2),
            Campo<TributacaoFederal>.Decimal("vRetIRRF", "RetencaoIrrf", g => g.RetencaoIrrf, (g, v) => g.RetencaoIrrf = v, 2),
            Campo<TributacaoFederal>.Decimal("vRetCSLL", "RetencaoCsll", g => g.RetencaoCsll, (g, v) => g.RetencaoCsll = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaPisCofins = MapaGrupo.Criar<PisCofins>(
        [
            Campo<PisCofins>.Enumeracao<CstPisCofinsServico>("CST", "Cst", g => g.Cst, (g, v) => g.Cst = v, 2),
            Campo<PisCofins>.Decimal("vBCPisCofins", "BaseCalculo", g => g.BaseCalculo, (g, v) => g.BaseCalculo = v, 2),
            Campo<PisCofins>.Decimal("pAliqPis", "AliquotaPis", g => g.AliquotaPis, (g, v) => g.AliquotaPis = v, 2),
            Campo<PisCofins>.Decimal("pAliqCofins", "AliquotaCofins", g => g.AliquotaCofins, (g, v) => g.AliquotaCofins = v, 2),
            Campo<PisCofins>.Decimal("vPis", "ValorPis", g => g.ValorPis, (g, v) => g.ValorPis = v, 2),
            Campo<PisCofins>.Decimal("vCofins", "ValorCofins", g => g.ValorCofins, (g, v) => g.ValorCofins = v, 2),
            Campo<PisCofins>.Enumeracao<RetencaoPisCofins>("tpRetPisCofins", "Retencao", g => g.Retencao, (g, v) => g.Retencao = v, 0),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalTributos = MapaGrupo.Criar<TotalTributos>(
        [
            Campo<TotalTributos>.Grupo<TotalTributosValor>("vTotTrib", "Valores", g => g.Valores, (g, v) => g.Valores = v, () => MapaTotalTributosValor!),
            Campo<TotalTributos>.Grupo<TotalTributosPercentual>("pTotTrib", "Percentuais", g => g.Percentuais, (g, v) => g.Percentuais = v, () => MapaTotalTributosPercentual!),
            Campo<TotalTributos>.Enumeracao<IndicadorTotalTributos>("indTotTrib", "Indicador", g => g.Indicador, (g, v) => g.Indicador = v, 0),
            Campo<TotalTributos>.Decimal("pTotTribSN", "PercentualSimples", g => g.PercentualSimples, (g, v) => g.PercentualSimples = v, 2),
        ],
        [["Valores", "Percentuais", "Indicador", "PercentualSimples"]]);

    internal static readonly MapaGrupo MapaTotalTributosValor = MapaGrupo.Criar<TotalTributosValor>(
        [
            Campo<TotalTributosValor>.Decimal("vTotTribFed", "Federal", g => g.Federal, (g, v) => g.Federal = v, 2),
            Campo<TotalTributosValor>.Decimal("vTotTribEst", "Estadual", g => g.Estadual, (g, v) => g.Estadual = v, 2),
            Campo<TotalTributosValor>.Decimal("vTotTribMun", "Municipal", g => g.Municipal, (g, v) => g.Municipal = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalTributosPercentual = MapaGrupo.Criar<TotalTributosPercentual>(
        [
            Campo<TotalTributosPercentual>.Decimal("pTotTribFed", "Federal", g => g.Federal, (g, v) => g.Federal = v, 2),
            Campo<TotalTributosPercentual>.Decimal("pTotTribEst", "Estadual", g => g.Estadual, (g, v) => g.Estadual = v, 2),
            Campo<TotalTributosPercentual>.Decimal("pTotTribMun", "Municipal", g => g.Municipal, (g, v) => g.Municipal = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaIbsCbsDps = MapaGrupo.Criar<IbsCbsDps>(
        [
            Campo<IbsCbsDps>.Enumeracao<FinalidadeNFSe>("finNFSe", "Finalidade", g => g.Finalidade, (g, v) => g.Finalidade = v, 0),
            Campo<IbsCbsDps>.Enumeracao<ConsumoPessoal>("indFinal", "ConsumoPessoal", g => g.ConsumoPessoal, (g, v) => g.ConsumoPessoal = v, 0),
            Campo<IbsCbsDps>.Texto("cIndOp", "CodigoOperacao", g => g.CodigoOperacao, (g, v) => g.CodigoOperacao = v),
            Campo<IbsCbsDps>.Enumeracao<TipoOperacaoGoverno>("tpOper", "TipoOperacao", g => g.TipoOperacao, (g, v) => g.TipoOperacao = v, 0),
            Campo<IbsCbsDps>.Grupo<NFSeReferenciadas>("gRefNFSe", "Referencias", g => g.Referencias, (g, v) => g.Referencias = v, () => MapaNFSeReferenciadas!),
            Campo<IbsCbsDps>.Enumeracao<EnteGovernamental>("tpEnteGov", "EnteGovernamental", g => g.EnteGovernamental, (g, v) => g.EnteGovernamental = v, 0),
            Campo<IbsCbsDps>.Enumeracao<IndicadorDestinatario>("indDest", "IndicadorDestinatario", g => g.IndicadorDestinatario, (g, v) => g.IndicadorDestinatario = v, 0),
            Campo<IbsCbsDps>.Grupo<Destinatario>("dest", "Destinatario", g => g.Destinatario, (g, v) => g.Destinatario = v, () => MapaDestinatario!),
            Campo<IbsCbsDps>.Grupo<Imovel>("imovel", "Imovel", g => g.Imovel, (g, v) => g.Imovel = v, () => MapaImovel!),
            Campo<IbsCbsDps>.Grupo<ValoresIbsCbs>("valores", "Valores", g => g.Valores, (g, v) => g.Valores = v, () => MapaValoresIbsCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaNFSeReferenciadas = MapaGrupo.Criar<NFSeReferenciadas>(
        [
            Campo<NFSeReferenciadas>.Textos("refNFSe", "Chaves", g => g.Chaves),
        ],
        []);

    internal static readonly MapaGrupo MapaDestinatario = MapaGrupo.Criar<Destinatario>(
        [
            Campo<Destinatario>.Texto("CNPJ", "Cnpj", g => g.Cnpj, (g, v) => g.Cnpj = v),
            Campo<Destinatario>.Texto("CPF", "Cpf", g => g.Cpf, (g, v) => g.Cpf = v),
            Campo<Destinatario>.Texto("NIF", "Nif", g => g.Nif, (g, v) => g.Nif = v),
            Campo<Destinatario>.Enumeracao<MotivoAusenciaNif>("cNaoNIF", "MotivoAusenciaNif", g => g.MotivoAusenciaNif, (g, v) => g.MotivoAusenciaNif = v, 0),
            Campo<Destinatario>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v),
            Campo<Destinatario>.Grupo<Endereco>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEndereco!),
            Campo<Destinatario>.Texto("fone", "Telefone", g => g.Telefone, (g, v) => g.Telefone = v),
            Campo<Destinatario>.Texto("email", "Email", g => g.Email, (g, v) => g.Email = v),
        ],
        [["Cnpj", "Cpf", "Nif", "MotivoAusenciaNif"]]);

    internal static readonly MapaGrupo MapaImovel = MapaGrupo.Criar<Imovel>(
        [
            Campo<Imovel>.Texto("inscImobFisc", "InscricaoImobiliaria", g => g.InscricaoImobiliaria, (g, v) => g.InscricaoImobiliaria = v),
            Campo<Imovel>.Texto("cCIB", "Cib", g => g.Cib, (g, v) => g.Cib = v),
            Campo<Imovel>.Grupo<EnderecoObra>("end", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEnderecoObra!),
        ],
        [["Cib", "Endereco"]]);

    internal static readonly MapaGrupo MapaValoresIbsCbs = MapaGrupo.Criar<ValoresIbsCbs>(
        [
            Campo<ValoresIbsCbs>.Grupo<ReembolsoRepasse>("gReeRepRes", "ReembolsoRepasse", g => g.ReembolsoRepasse, (g, v) => g.ReembolsoRepasse = v, () => MapaReembolsoRepasse!),
            Campo<ValoresIbsCbs>.Grupo<TributosIbsCbs>("trib", "Tributos", g => g.Tributos, (g, v) => g.Tributos = v, () => MapaTributosIbsCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaReembolsoRepasse = MapaGrupo.Criar<ReembolsoRepasse>(
        [
            Campo<ReembolsoRepasse>.Lista<DocumentoReembolso>("documentos", "Documentos", g => g.Documentos, () => MapaDocumentoReembolso!),
        ],
        []);

    internal static readonly MapaGrupo MapaDocumentoReembolso = MapaGrupo.Criar<DocumentoReembolso>(
        [
            Campo<DocumentoReembolso>.Grupo<DFeNacional>("dFeNacional", "DFeNacional", g => g.DFeNacional, (g, v) => g.DFeNacional = v, () => MapaDFeNacional!),
            Campo<DocumentoReembolso>.Grupo<DocumentoFiscalOutro>("docFiscalOutro", "DocumentoFiscal", g => g.DocumentoFiscal, (g, v) => g.DocumentoFiscal = v, () => MapaDocumentoFiscalOutro!),
            Campo<DocumentoReembolso>.Grupo<DocumentoOutro>("docOutro", "DocumentoNaoFiscal", g => g.DocumentoNaoFiscal, (g, v) => g.DocumentoNaoFiscal = v, () => MapaDocumentoOutro!),
            Campo<DocumentoReembolso>.Grupo<FornecedorDocumento>("fornec", "Fornecedor", g => g.Fornecedor, (g, v) => g.Fornecedor = v, () => MapaFornecedorDocumento!),
            Campo<DocumentoReembolso>.Data("dtEmiDoc", "Emissao", g => g.Emissao, (g, v) => g.Emissao = v),
            Campo<DocumentoReembolso>.Data("dtCompDoc", "Competencia", g => g.Competencia, (g, v) => g.Competencia = v),
            Campo<DocumentoReembolso>.Enumeracao<TipoReembolso>("tpReeRepRes", "Tipo", g => g.Tipo, (g, v) => g.Tipo = v, 2),
            Campo<DocumentoReembolso>.Texto("xTpReeRepRes", "DescricaoTipo", g => g.DescricaoTipo, (g, v) => g.DescricaoTipo = v),
            Campo<DocumentoReembolso>.Decimal("vlrReeRepRes", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        [["DFeNacional", "DocumentoFiscal", "DocumentoNaoFiscal"]]);

    internal static readonly MapaGrupo MapaDFeNacional = MapaGrupo.Criar<DFeNacional>(
        [
            Campo<DFeNacional>.Enumeracao<TipoDFe>("tipoChaveDFe", "Tipo", g => g.Tipo, (g, v) => g.Tipo = v, 0),
            Campo<DFeNacional>.Texto("xTipoChaveDFe", "DescricaoTipo", g => g.DescricaoTipo, (g, v) => g.DescricaoTipo = v),
            Campo<DFeNacional>.Texto("chaveDFe", "Chave", g => g.Chave, (g, v) => g.Chave = v),
        ],
        []);

    internal static readonly MapaGrupo MapaDocumentoFiscalOutro = MapaGrupo.Criar<DocumentoFiscalOutro>(
        [
            Campo<DocumentoFiscalOutro>.Inteiro("cMunDocFiscal", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v),
            Campo<DocumentoFiscalOutro>.Texto("nDocFiscal", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<DocumentoFiscalOutro>.Texto("xDocFiscal", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaDocumentoOutro = MapaGrupo.Criar<DocumentoOutro>(
        [
            Campo<DocumentoOutro>.Texto("nDoc", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<DocumentoOutro>.Texto("xDoc", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaFornecedorDocumento = MapaGrupo.Criar<FornecedorDocumento>(
        [
            Campo<FornecedorDocumento>.Texto("CNPJ", "Cnpj", g => g.Cnpj, (g, v) => g.Cnpj = v),
            Campo<FornecedorDocumento>.Texto("CPF", "Cpf", g => g.Cpf, (g, v) => g.Cpf = v),
            Campo<FornecedorDocumento>.Texto("NIF", "Nif", g => g.Nif, (g, v) => g.Nif = v),
            Campo<FornecedorDocumento>.Enumeracao<MotivoAusenciaNif>("cNaoNIF", "MotivoAusenciaNif", g => g.MotivoAusenciaNif, (g, v) => g.MotivoAusenciaNif = v, 0),
            Campo<FornecedorDocumento>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v),
        ],
        [["Cnpj", "Cpf", "Nif", "MotivoAusenciaNif"]]);

    internal static readonly MapaGrupo MapaTributosIbsCbs = MapaGrupo.Criar<TributosIbsCbs>(
        [
            Campo<TributosIbsCbs>.Grupo<ClassificacaoIbsCbs>("gIBSCBS", "Classificacao", g => g.Classificacao, (g, v) => g.Classificacao = v, () => MapaClassificacaoIbsCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaClassificacaoIbsCbs = MapaGrupo.Criar<ClassificacaoIbsCbs>(
        [
            Campo<ClassificacaoIbsCbs>.Texto("CST", "Cst", g => g.Cst, (g, v) => g.Cst = v),
            Campo<ClassificacaoIbsCbs>.Texto("cClassTrib", "ClassificacaoTributaria", g => g.ClassificacaoTributaria, (g, v) => g.ClassificacaoTributaria = v),
            Campo<ClassificacaoIbsCbs>.Texto("cCredPres", "CreditoPresumido", g => g.CreditoPresumido, (g, v) => g.CreditoPresumido = v),
            Campo<ClassificacaoIbsCbs>.Grupo<TributacaoRegularIbsCbs>("gTribRegular", "TributacaoRegular", g => g.TributacaoRegular, (g, v) => g.TributacaoRegular = v, () => MapaTributacaoRegularIbsCbs!),
            Campo<ClassificacaoIbsCbs>.Grupo<DiferimentoIbsCbs>("gDif", "Diferimento", g => g.Diferimento, (g, v) => g.Diferimento = v, () => MapaDiferimentoIbsCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaTributacaoRegularIbsCbs = MapaGrupo.Criar<TributacaoRegularIbsCbs>(
        [
            Campo<TributacaoRegularIbsCbs>.Texto("CSTReg", "Cst", g => g.Cst, (g, v) => g.Cst = v),
            Campo<TributacaoRegularIbsCbs>.Texto("cClassTribReg", "ClassificacaoTributaria", g => g.ClassificacaoTributaria, (g, v) => g.ClassificacaoTributaria = v),
        ],
        []);

    internal static readonly MapaGrupo MapaDiferimentoIbsCbs = MapaGrupo.Criar<DiferimentoIbsCbs>(
        [
            Campo<DiferimentoIbsCbs>.Decimal("pDifUF", "PercentualUF", g => g.PercentualUF, (g, v) => g.PercentualUF = v, 2),
            Campo<DiferimentoIbsCbs>.Decimal("pDifMun", "PercentualMunicipio", g => g.PercentualMunicipio, (g, v) => g.PercentualMunicipio = v, 2),
            Campo<DiferimentoIbsCbs>.Decimal("pDifCBS", "PercentualCbs", g => g.PercentualCbs, (g, v) => g.PercentualCbs = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaNotaFiscalServico = MapaGrupo.Criar<NotaFiscalServico>(
        [
            Campo<NotaFiscalServico>.Texto("xLocEmi", "NomeMunicipioEmissor", g => g.NomeMunicipioEmissor, (g, v) => g.NomeMunicipioEmissor = v),
            Campo<NotaFiscalServico>.Texto("xLocPrestacao", "NomeLocalPrestacao", g => g.NomeLocalPrestacao, (g, v) => g.NomeLocalPrestacao = v),
            Campo<NotaFiscalServico>.Longo("nNFSe", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<NotaFiscalServico>.Inteiro("cLocIncid", "MunicipioIncidencia", g => g.MunicipioIncidencia, (g, v) => g.MunicipioIncidencia = v),
            Campo<NotaFiscalServico>.Texto("xLocIncid", "NomeMunicipioIncidencia", g => g.NomeMunicipioIncidencia, (g, v) => g.NomeMunicipioIncidencia = v),
            Campo<NotaFiscalServico>.Texto("xTribNac", "DescricaoTributacaoNacional", g => g.DescricaoTributacaoNacional, (g, v) => g.DescricaoTributacaoNacional = v),
            Campo<NotaFiscalServico>.Texto("xTribMun", "DescricaoTributacaoMunicipal", g => g.DescricaoTributacaoMunicipal, (g, v) => g.DescricaoTributacaoMunicipal = v),
            Campo<NotaFiscalServico>.Texto("xNBS", "DescricaoNbs", g => g.DescricaoNbs, (g, v) => g.DescricaoNbs = v),
            Campo<NotaFiscalServico>.Texto("verAplic", "VersaoAplicativo", g => g.VersaoAplicativo, (g, v) => g.VersaoAplicativo = v),
            Campo<NotaFiscalServico>.Enumeracao<AmbienteGerador>("ambGer", "AmbienteGerador", g => g.AmbienteGerador, (g, v) => g.AmbienteGerador = v, 0),
            Campo<NotaFiscalServico>.Enumeracao<TipoEmissaoNFSe>("tpEmis", "TipoEmissao", g => g.TipoEmissao, (g, v) => g.TipoEmissao = v, 0),
            Campo<NotaFiscalServico>.Enumeracao<ProcessoEmissao>("procEmi", "ProcessoEmissao", g => g.ProcessoEmissao, (g, v) => g.ProcessoEmissao = v, 0),
            Campo<NotaFiscalServico>.Enumeracao<SituacaoNFSe>("cStat", "Situacao", g => g.Situacao, (g, v) => g.Situacao = v, 0),
            Campo<NotaFiscalServico>.DataHora("dhProc", "Processamento", g => g.Processamento, (g, v) => g.Processamento = v),
            Campo<NotaFiscalServico>.Longo("nDFSe", "NumeroDfse", g => g.NumeroDfse, (g, v) => g.NumeroDfse = v),
            Campo<NotaFiscalServico>.Grupo<EmitenteNFSe>("emit", "Emitente", g => g.Emitente, (g, v) => g.Emitente = v, () => MapaEmitenteNFSe!),
            Campo<NotaFiscalServico>.Grupo<ValoresNFSe>("valores", "Valores", g => g.Valores, (g, v) => g.Valores = v, () => MapaValoresNFSe!),
            Campo<NotaFiscalServico>.Texto("xOutInf", "InformacoesFisco", g => g.InformacoesFisco, (g, v) => g.InformacoesFisco = v),
            Campo<NotaFiscalServico>.Grupo<IbsCbsNFSe>("IBSCBS", "IbsCbs", g => g.IbsCbs, (g, v) => g.IbsCbs = v, () => MapaIbsCbsNFSe!),
            Campo<NotaFiscalServico>.Dps("DPS", "Dps", (g, v) => g.Dps = v),
        ],
        []);

    internal static readonly MapaGrupo MapaEmitenteNFSe = MapaGrupo.Criar<EmitenteNFSe>(
        [
            Campo<EmitenteNFSe>.Texto("CNPJ", "Cnpj", g => g.Cnpj, (g, v) => g.Cnpj = v),
            Campo<EmitenteNFSe>.Texto("CPF", "Cpf", g => g.Cpf, (g, v) => g.Cpf = v),
            Campo<EmitenteNFSe>.Texto("IM", "InscricaoMunicipal", g => g.InscricaoMunicipal, (g, v) => g.InscricaoMunicipal = v),
            Campo<EmitenteNFSe>.Texto("xNome", "Nome", g => g.Nome, (g, v) => g.Nome = v),
            Campo<EmitenteNFSe>.Texto("xFant", "NomeFantasia", g => g.NomeFantasia, (g, v) => g.NomeFantasia = v),
            Campo<EmitenteNFSe>.Grupo<EnderecoEmitente>("enderNac", "Endereco", g => g.Endereco, (g, v) => g.Endereco = v, () => MapaEnderecoEmitente!),
            Campo<EmitenteNFSe>.Texto("fone", "Telefone", g => g.Telefone, (g, v) => g.Telefone = v),
            Campo<EmitenteNFSe>.Texto("email", "Email", g => g.Email, (g, v) => g.Email = v),
        ],
        [["Cnpj", "Cpf"]]);

    internal static readonly MapaGrupo MapaEnderecoEmitente = MapaGrupo.Criar<EnderecoEmitente>(
        [
            Campo<EnderecoEmitente>.Texto("xLgr", "Logradouro", g => g.Logradouro, (g, v) => g.Logradouro = v),
            Campo<EnderecoEmitente>.Texto("nro", "Numero", g => g.Numero, (g, v) => g.Numero = v),
            Campo<EnderecoEmitente>.Texto("xCpl", "Complemento", g => g.Complemento, (g, v) => g.Complemento = v),
            Campo<EnderecoEmitente>.Texto("xBairro", "Bairro", g => g.Bairro, (g, v) => g.Bairro = v),
            Campo<EnderecoEmitente>.Inteiro("cMun", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v),
            Campo<EnderecoEmitente>.Sigla("UF", "UF", g => g.UF, (g, v) => g.UF = v),
            Campo<EnderecoEmitente>.Texto("CEP", "Cep", g => g.Cep, (g, v) => g.Cep = v),
        ],
        []);

    internal static readonly MapaGrupo MapaValoresNFSe = MapaGrupo.Criar<ValoresNFSe>(
        [
            Campo<ValoresNFSe>.Decimal("vCalcDR", "DeducaoReducao", g => g.DeducaoReducao, (g, v) => g.DeducaoReducao = v, 2),
            Campo<ValoresNFSe>.Enumeracao<TipoBeneficio>("tpBM", "TipoBeneficio", g => g.TipoBeneficio, (g, v) => g.TipoBeneficio = v, 0, textoLivre100: true),
            Campo<ValoresNFSe>.Decimal("vCalcBM", "ReducaoBeneficio", g => g.ReducaoBeneficio, (g, v) => g.ReducaoBeneficio = v, 2),
            Campo<ValoresNFSe>.Decimal("vBC", "BaseCalculo", g => g.BaseCalculo, (g, v) => g.BaseCalculo = v, 2),
            Campo<ValoresNFSe>.Decimal("pAliqAplic", "AliquotaAplicada", g => g.AliquotaAplicada, (g, v) => g.AliquotaAplicada = v, 2),
            Campo<ValoresNFSe>.Decimal("vISSQN", "Issqn", g => g.Issqn, (g, v) => g.Issqn = v, 2),
            Campo<ValoresNFSe>.Decimal("vTotalRet", "TotalRetencoes", g => g.TotalRetencoes, (g, v) => g.TotalRetencoes = v, 2),
            Campo<ValoresNFSe>.Decimal("vLiq", "Liquido", g => g.Liquido, (g, v) => g.Liquido = v, 2),
            Campo<ValoresNFSe>.Desvio("xOutInf", (l, texto) => l.InformacoesFisco100 = texto),
        ],
        []);

    internal static readonly MapaGrupo MapaIbsCbsNFSe = MapaGrupo.Criar<IbsCbsNFSe>(
        [
            Campo<IbsCbsNFSe>.Inteiro("cLocalidadeIncid", "MunicipioIncidencia", g => g.MunicipioIncidencia, (g, v) => g.MunicipioIncidencia = v),
            Campo<IbsCbsNFSe>.Texto("xLocalidadeIncid", "NomeMunicipioIncidencia", g => g.NomeMunicipioIncidencia, (g, v) => g.NomeMunicipioIncidencia = v),
            Campo<IbsCbsNFSe>.Decimal("pRedutor", "PercentualRedutor", g => g.PercentualRedutor, (g, v) => g.PercentualRedutor = v, 2),
            Campo<IbsCbsNFSe>.Grupo<ValoresIbsCbsNFSe>("valores", "Valores", g => g.Valores, (g, v) => g.Valores = v, () => MapaValoresIbsCbsNFSe!),
            Campo<IbsCbsNFSe>.Grupo<TotaisIbsCbs>("totCIBS", "Totais", g => g.Totais, (g, v) => g.Totais = v, () => MapaTotaisIbsCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaValoresIbsCbsNFSe = MapaGrupo.Criar<ValoresIbsCbsNFSe>(
        [
            Campo<ValoresIbsCbsNFSe>.Decimal("vBC", "BaseCalculo", g => g.BaseCalculo, (g, v) => g.BaseCalculo = v, 2),
            Campo<ValoresIbsCbsNFSe>.Decimal("vCalcReeRepRes", "ReembolsoRepasse", g => g.ReembolsoRepasse, (g, v) => g.ReembolsoRepasse = v, 2),
            Campo<ValoresIbsCbsNFSe>.Grupo<AliquotaIbsUF>("uf", "UF", g => g.UF, (g, v) => g.UF = v, () => MapaAliquotaIbsUF!),
            Campo<ValoresIbsCbsNFSe>.Grupo<AliquotaIbsMunicipio>("mun", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v, () => MapaAliquotaIbsMunicipio!),
            Campo<ValoresIbsCbsNFSe>.Grupo<AliquotaCbs>("fed", "Cbs", g => g.Cbs, (g, v) => g.Cbs = v, () => MapaAliquotaCbs!),
        ],
        []);

    internal static readonly MapaGrupo MapaAliquotaIbsUF = MapaGrupo.Criar<AliquotaIbsUF>(
        [
            Campo<AliquotaIbsUF>.Decimal("pIBSUF", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
            Campo<AliquotaIbsUF>.Decimal("pRedAliqUF", "Reducao", g => g.Reducao, (g, v) => g.Reducao = v, 2),
            Campo<AliquotaIbsUF>.Decimal("pAliqEfetUF", "Efetiva", g => g.Efetiva, (g, v) => g.Efetiva = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaAliquotaIbsMunicipio = MapaGrupo.Criar<AliquotaIbsMunicipio>(
        [
            Campo<AliquotaIbsMunicipio>.Decimal("pIBSMun", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
            Campo<AliquotaIbsMunicipio>.Decimal("pRedAliqMun", "Reducao", g => g.Reducao, (g, v) => g.Reducao = v, 2),
            Campo<AliquotaIbsMunicipio>.Decimal("pAliqEfetMun", "Efetiva", g => g.Efetiva, (g, v) => g.Efetiva = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaAliquotaCbs = MapaGrupo.Criar<AliquotaCbs>(
        [
            Campo<AliquotaCbs>.Decimal("pCBS", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
            Campo<AliquotaCbs>.Decimal("pRedAliqCBS", "Reducao", g => g.Reducao, (g, v) => g.Reducao = v, 2),
            Campo<AliquotaCbs>.Decimal("pAliqEfetCBS", "Efetiva", g => g.Efetiva, (g, v) => g.Efetiva = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotaisIbsCbs = MapaGrupo.Criar<TotaisIbsCbs>(
        [
            Campo<TotaisIbsCbs>.Decimal("vTotNF", "TotalNota", g => g.TotalNota, (g, v) => g.TotalNota = v, 2),
            Campo<TotaisIbsCbs>.Grupo<TotalIbs>("gIBS", "Ibs", g => g.Ibs, (g, v) => g.Ibs = v, () => MapaTotalIbs!),
            Campo<TotaisIbsCbs>.Grupo<TotalCbs>("gCBS", "Cbs", g => g.Cbs, (g, v) => g.Cbs = v, () => MapaTotalCbs!),
            Campo<TotaisIbsCbs>.Grupo<TotalTributacaoRegular>("gTribRegular", "TributacaoRegular", g => g.TributacaoRegular, (g, v) => g.TributacaoRegular = v, () => MapaTotalTributacaoRegular!),
            Campo<TotaisIbsCbs>.Grupo<TotalCompraGovernamental>("gTribCompraGov", "CompraGovernamental", g => g.CompraGovernamental, (g, v) => g.CompraGovernamental = v, () => MapaTotalCompraGovernamental!),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalIbs = MapaGrupo.Criar<TotalIbs>(
        [
            Campo<TotalIbs>.Decimal("vIBSTot", "Total", g => g.Total, (g, v) => g.Total = v, 2),
            Campo<TotalIbs>.Grupo<CreditoPresumidoIbs>("gIBSCredPres", "CreditoPresumido", g => g.CreditoPresumido, (g, v) => g.CreditoPresumido = v, () => MapaCreditoPresumidoIbs!),
            Campo<TotalIbs>.Grupo<TotalIbsUF>("gIBSUFTot", "UF", g => g.UF, (g, v) => g.UF = v, () => MapaTotalIbsUF!),
            Campo<TotalIbs>.Grupo<TotalIbsMunicipio>("gIBSMunTot", "Municipio", g => g.Municipio, (g, v) => g.Municipio = v, () => MapaTotalIbsMunicipio!),
        ],
        []);

    internal static readonly MapaGrupo MapaCreditoPresumidoIbs = MapaGrupo.Criar<CreditoPresumidoIbs>(
        [
            Campo<CreditoPresumidoIbs>.Decimal("pCredPresIBS", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
            Campo<CreditoPresumidoIbs>.Decimal("vCredPresIBS", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalIbsUF = MapaGrupo.Criar<TotalIbsUF>(
        [
            Campo<TotalIbsUF>.Decimal("vDifUF", "Diferimento", g => g.Diferimento, (g, v) => g.Diferimento = v, 2),
            Campo<TotalIbsUF>.Decimal("vIBSUF", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalIbsMunicipio = MapaGrupo.Criar<TotalIbsMunicipio>(
        [
            Campo<TotalIbsMunicipio>.Decimal("vDifMun", "Diferimento", g => g.Diferimento, (g, v) => g.Diferimento = v, 2),
            Campo<TotalIbsMunicipio>.Decimal("vIBSMun", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalCbs = MapaGrupo.Criar<TotalCbs>(
        [
            Campo<TotalCbs>.Grupo<CreditoPresumidoCbs>("gCBSCredPres", "CreditoPresumido", g => g.CreditoPresumido, (g, v) => g.CreditoPresumido = v, () => MapaCreditoPresumidoCbs!),
            Campo<TotalCbs>.Decimal("vDifCBS", "Diferimento", g => g.Diferimento, (g, v) => g.Diferimento = v, 2),
            Campo<TotalCbs>.Decimal("vCBS", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaCreditoPresumidoCbs = MapaGrupo.Criar<CreditoPresumidoCbs>(
        [
            Campo<CreditoPresumidoCbs>.Decimal("pCredPresCBS", "Aliquota", g => g.Aliquota, (g, v) => g.Aliquota = v, 2),
            Campo<CreditoPresumidoCbs>.Decimal("vCredPresCBS", "Valor", g => g.Valor, (g, v) => g.Valor = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalTributacaoRegular = MapaGrupo.Criar<TotalTributacaoRegular>(
        [
            Campo<TotalTributacaoRegular>.Decimal("pAliqEfeRegIBSUF", "AliquotaIbsUF", g => g.AliquotaIbsUF, (g, v) => g.AliquotaIbsUF = v, 2),
            Campo<TotalTributacaoRegular>.Decimal("vTribRegIBSUF", "ValorIbsUF", g => g.ValorIbsUF, (g, v) => g.ValorIbsUF = v, 2),
            Campo<TotalTributacaoRegular>.Decimal("pAliqEfeRegIBSMun", "AliquotaIbsMunicipio", g => g.AliquotaIbsMunicipio, (g, v) => g.AliquotaIbsMunicipio = v, 2),
            Campo<TotalTributacaoRegular>.Decimal("vTribRegIBSMun", "ValorIbsMunicipio", g => g.ValorIbsMunicipio, (g, v) => g.ValorIbsMunicipio = v, 2),
            Campo<TotalTributacaoRegular>.Decimal("pAliqEfeRegCBS", "AliquotaCbs", g => g.AliquotaCbs, (g, v) => g.AliquotaCbs = v, 2),
            Campo<TotalTributacaoRegular>.Decimal("vTribRegCBS", "ValorCbs", g => g.ValorCbs, (g, v) => g.ValorCbs = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaTotalCompraGovernamental = MapaGrupo.Criar<TotalCompraGovernamental>(
        [
            Campo<TotalCompraGovernamental>.Decimal("pIBSUF", "AliquotaIbsUF", g => g.AliquotaIbsUF, (g, v) => g.AliquotaIbsUF = v, 2),
            Campo<TotalCompraGovernamental>.Decimal("vIBSUF", "ValorIbsUF", g => g.ValorIbsUF, (g, v) => g.ValorIbsUF = v, 2),
            Campo<TotalCompraGovernamental>.Decimal("pIBSMun", "AliquotaIbsMunicipio", g => g.AliquotaIbsMunicipio, (g, v) => g.AliquotaIbsMunicipio = v, 2),
            Campo<TotalCompraGovernamental>.Decimal("vIBSMun", "ValorIbsMunicipio", g => g.ValorIbsMunicipio, (g, v) => g.ValorIbsMunicipio = v, 2),
            Campo<TotalCompraGovernamental>.Decimal("pCBS", "AliquotaCbs", g => g.AliquotaCbs, (g, v) => g.AliquotaCbs = v, 2),
            Campo<TotalCompraGovernamental>.Decimal("vCBS", "ValorCbs", g => g.ValorCbs, (g, v) => g.ValorCbs = v, 2),
        ],
        []);

    internal static readonly MapaGrupo MapaEventoNFSe = MapaGrupo.Criar<EventoNFSe>(
        [
            Campo<EventoNFSe>.Enumeracao<Ambiente>("tpAmb", "Ambiente", g => g.Ambiente, (g, v) => g.Ambiente = v, 0),
            Campo<EventoNFSe>.Texto("verAplic", "VersaoAplicativo", g => g.VersaoAplicativo, (g, v) => g.VersaoAplicativo = v),
            Campo<EventoNFSe>.DataHora("dhEvento", "Data", g => g.Data, (g, v) => g.Data = v),
            Campo<EventoNFSe>.Texto("CNPJAutor", "CnpjAutor", g => g.CnpjAutor, (g, v) => g.CnpjAutor = v),
            Campo<EventoNFSe>.Texto("CPFAutor", "CpfAutor", g => g.CpfAutor, (g, v) => g.CpfAutor = v),
            Campo<EventoNFSe>.Texto("chNFSe", "Chave", g => g.Chave, (g, v) => g.Chave = v),
        ],
        [["CnpjAutor", "CpfAutor"]]);

    internal static readonly MapaGrupo MapaEventoNFSeRegistrado = MapaGrupo.Criar<EventoNFSeRegistrado>(
        [
            Campo<EventoNFSeRegistrado>.Texto("verAplic", "VersaoAplicativo", g => g.VersaoAplicativo, (g, v) => g.VersaoAplicativo = v),
            Campo<EventoNFSeRegistrado>.Enumeracao<AmbienteGeradorEvento>("ambGer", "AmbienteGerador", g => g.AmbienteGerador, (g, v) => g.AmbienteGerador = v, 0),
            Campo<EventoNFSeRegistrado>.Inteiro("nSeqEvento", "Sequencia", g => g.Sequencia, (g, v) => g.Sequencia = v),
            Campo<EventoNFSeRegistrado>.DataHora("dhProc", "Processamento", g => g.Processamento, (g, v) => g.Processamento = v),
            Campo<EventoNFSeRegistrado>.Longo("nDFSe", "NumeroDfse", g => g.NumeroDfse, (g, v) => g.NumeroDfse = v),
            Campo<EventoNFSeRegistrado>.Pedido("pedRegEvento", "Pedido", (g, v) => g.Pedido = v),
        ],
        []);

    internal static readonly MapaGrupo MapaCancelamentoNFSe = MapaGrupo.Criar<CancelamentoNFSe>(
        [
            Campo<CancelamentoNFSe>.Enumeracao<MotivoCancelamento>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<CancelamentoNFSe>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaCancelamentoSubstituicao = MapaGrupo.Criar<CancelamentoSubstituicao>(
        [
            Campo<CancelamentoSubstituicao>.Enumeracao<MotivoSubstituicao>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 2),
            Campo<CancelamentoSubstituicao>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
            Campo<CancelamentoSubstituicao>.Texto("chSubstituta", "ChaveSubstituta", g => g.ChaveSubstituta, (g, v) => g.ChaveSubstituta = v),
        ],
        []);

    internal static readonly MapaGrupo MapaAnaliseFiscalCancelamento = MapaGrupo.Criar<AnaliseFiscalCancelamento>(
        [
            Campo<AnaliseFiscalCancelamento>.Enumeracao<MotivoAnaliseFiscal>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<AnaliseFiscalCancelamento>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaCancelamentoDeferido = MapaGrupo.Criar<CancelamentoDeferido>(
        [
            Campo<CancelamentoDeferido>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<CancelamentoDeferido>.Texto("nProcAdm", "ProcessoAdministrativo", g => g.ProcessoAdministrativo, (g, v) => g.ProcessoAdministrativo = v),
            Campo<CancelamentoDeferido>.Enumeracao<MotivoDeferimento>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<CancelamentoDeferido>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaCancelamentoIndeferido = MapaGrupo.Criar<CancelamentoIndeferido>(
        [
            Campo<CancelamentoIndeferido>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<CancelamentoIndeferido>.Texto("nProcAdm", "ProcessoAdministrativo", g => g.ProcessoAdministrativo, (g, v) => g.ProcessoAdministrativo = v),
            Campo<CancelamentoIndeferido>.Enumeracao<MotivoIndeferimento>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<CancelamentoIndeferido>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaConfirmacaoPrestador = MapaGrupo.Criar<ConfirmacaoPrestador>(
        [],
        []);

    internal static readonly MapaGrupo MapaConfirmacaoTomador = MapaGrupo.Criar<ConfirmacaoTomador>(
        [],
        []);

    internal static readonly MapaGrupo MapaConfirmacaoIntermediario = MapaGrupo.Criar<ConfirmacaoIntermediario>(
        [],
        []);

    internal static readonly MapaGrupo MapaConfirmacaoTacita = MapaGrupo.Criar<ConfirmacaoTacita>(
        [],
        []);

    internal static readonly MapaGrupo MapaRejeicaoPrestador = MapaGrupo.Criar<RejeicaoPrestador>(
        [
            Campo<RejeicaoPrestador>.Enumeracao<MotivoRejeicao>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<RejeicaoPrestador>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaRejeicaoTomador = MapaGrupo.Criar<RejeicaoTomador>(
        [
            Campo<RejeicaoTomador>.Enumeracao<MotivoRejeicao>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<RejeicaoTomador>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaRejeicaoIntermediario = MapaGrupo.Criar<RejeicaoIntermediario>(
        [
            Campo<RejeicaoIntermediario>.Enumeracao<MotivoRejeicao>("cMotivo", "Motivo", g => g.Motivo, (g, v) => g.Motivo = v, 0),
            Campo<RejeicaoIntermediario>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaAnulacaoRejeicao = MapaGrupo.Criar<AnulacaoRejeicao>(
        [
            Campo<AnulacaoRejeicao>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<AnulacaoRejeicao>.Texto("idEvManifRej", "EventoRejeicao", g => g.EventoRejeicao, (g, v) => g.EventoRejeicao = v),
            Campo<AnulacaoRejeicao>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaCancelamentoOficio = MapaGrupo.Criar<CancelamentoOficio>(
        [
            Campo<CancelamentoOficio>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<CancelamentoOficio>.Texto("nProcAdm", "ProcessoAdministrativo", g => g.ProcessoAdministrativo, (g, v) => g.ProcessoAdministrativo = v),
            Campo<CancelamentoOficio>.Texto("xProcAdm", "DescricaoProcesso", g => g.DescricaoProcesso, (g, v) => g.DescricaoProcesso = v),
        ],
        []);

    internal static readonly MapaGrupo MapaBloqueioOficio = MapaGrupo.Criar<BloqueioOficio>(
        [
            Campo<BloqueioOficio>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<BloqueioOficio>.Texto("codEvento", "EventoBloqueado", g => g.EventoBloqueado, (g, v) => g.EventoBloqueado = v),
            Campo<BloqueioOficio>.Texto("xMotivo", "Descricao", g => g.Descricao, (g, v) => g.Descricao = v),
        ],
        []);

    internal static readonly MapaGrupo MapaDesbloqueioOficio = MapaGrupo.Criar<DesbloqueioOficio>(
        [
            Campo<DesbloqueioOficio>.Texto("CPFAgTrib", "CpfAgente", g => g.CpfAgente, (g, v) => g.CpfAgente = v),
            Campo<DesbloqueioOficio>.Texto("idBloqOfic", "Bloqueio", g => g.Bloqueio, (g, v) => g.Bloqueio = v),
        ],
        []);

    /// <summary>Os tipos de evento do XSD 1.01, com o texto fixo do <c>xDesc</c> nas duas versões.</summary>
    internal static readonly TipoEventoNFSe[] Eventos =
    [
        new("101101", "e101101", typeof(CancelamentoNFSe), () => new CancelamentoNFSe(), () => MapaCancelamentoNFSe!, "Cancelamento de NFS-e", "Cancelamento de NFS-e", true),
        new("105102", "e105102", typeof(CancelamentoSubstituicao), () => new CancelamentoSubstituicao(), () => MapaCancelamentoSubstituicao!, "Cancelamento de NFS-e por Substituição", "Cancelamento de NFS-e por Substituicao", false),
        new("101103", "e101103", typeof(AnaliseFiscalCancelamento), () => new AnaliseFiscalCancelamento(), () => MapaAnaliseFiscalCancelamento!, "Solicitação de Análise Fiscal para Cancelamento de NFS-e", "Solicitacao de Analise Fiscal para Cancelamento de NFS-e", true),
        new("105104", "e105104", typeof(CancelamentoDeferido), () => new CancelamentoDeferido(), () => MapaCancelamentoDeferido!, "Cancelamento de NFS-e Deferido por Análise Fiscal", "Cancelamento de NFS-e Deferido por Análise Fiscal", false),
        new("105105", "e105105", typeof(CancelamentoIndeferido), () => new CancelamentoIndeferido(), () => MapaCancelamentoIndeferido!, "Cancelamento de NFS-e Indeferido por Análise Fiscal", "Cancelamento de NFS-e Indeferido por Análise Fiscal", false),
        new("202201", "e202201", typeof(ConfirmacaoPrestador), () => new ConfirmacaoPrestador(), () => MapaConfirmacaoPrestador!, "Manifestação de NFS-e - Confirmação do Prestador", "Confirmação do Prestador", false),
        new("203202", "e203202", typeof(ConfirmacaoTomador), () => new ConfirmacaoTomador(), () => MapaConfirmacaoTomador!, "Manifestação de NFS-e - Confirmação do Tomador", "Confirmação do Tomador", false),
        new("204203", "e204203", typeof(ConfirmacaoIntermediario), () => new ConfirmacaoIntermediario(), () => MapaConfirmacaoIntermediario!, "Manifestação de NFS-e - Confirmação do Intermediário", "Confirmação do Intermediário", false),
        new("205204", "e205204", typeof(ConfirmacaoTacita), () => new ConfirmacaoTacita(), () => MapaConfirmacaoTacita!, "Manifestação de NFS-e - Confirmação Tácita", "Confirmação Tácita", false),
        new("202205", "e202205", typeof(RejeicaoPrestador), () => new RejeicaoPrestador(), () => MapaRejeicaoPrestador!, "Manifestação de NFS-e - Rejeição do Prestador", "Rejeição do Prestador", false),
        new("203206", "e203206", typeof(RejeicaoTomador), () => new RejeicaoTomador(), () => MapaRejeicaoTomador!, "Manifestação de NFS-e - Rejeição do Tomador", "Rejeição do Tomador", false),
        new("204207", "e204207", typeof(RejeicaoIntermediario), () => new RejeicaoIntermediario(), () => MapaRejeicaoIntermediario!, "Manifestação de NFS-e - Rejeição do Intermediário", "Rejeição do Intermediário", false),
        new("205208", "e205208", typeof(AnulacaoRejeicao), () => new AnulacaoRejeicao(), () => MapaAnulacaoRejeicao!, "Manifestação de NFS-e - Anulação da Rejeição", "Anulação da Rejeição", false),
        new("305101", "e305101", typeof(CancelamentoOficio), () => new CancelamentoOficio(), () => MapaCancelamentoOficio!, "Cancelamento de NFS-e por Ofício", "Cancelamento de NFS-e por Ofício", false),
        new("305102", "e305102", typeof(BloqueioOficio), () => new BloqueioOficio(), () => MapaBloqueioOficio!, "Bloqueio de NFS-e por Ofício", "Bloqueio de NFS-e por Ofício", false),
        new("305103", "e305103", typeof(DesbloqueioOficio), () => new DesbloqueioOficio(), () => MapaDesbloqueioOficio!, "Desbloqueio de NFS-e por Ofício", "Desbloqueio de NFS-e por Ofício", false),
    ];
}
