using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// As DPS de teste (PROJETO.md 13.2): quatro variantes que, juntas, preenchem cada propriedade do modelo da DPS e cada
// alternativa das escolhas do XSD (docs/NFSE.md 7, M7). Todas válidas no XSD 1.01; os valores seguem os padrões dele.
internal static class DpsTeste
{
    internal const string CnpjPrestador = "11222333000181";
    internal const string CnpjTomador = "11444777000161";
    internal const string Cpf = "52998224725";
    internal const int Municipio = 3106200;
    internal const string Chave = "31062001251235800000112230000000173023019580208160";
    internal const string ChaveNFe = "31261111222333000181550010000000421000000425";

    internal static readonly DateTimeOffset Emissao = new(2026, 11, 19, 10, 30, 0, TimeSpan.FromHours(-3));

    // A mínima: o prestador CPF emite, sem tomador, com o indicador do total dos tributos.
    internal static Dps Minima() => new()
    {
        Ambiente = Ambiente.Homologacao,
        Emissao = Emissao,
        VersaoAplicativo = "ERP 1.0",
        Serie = "1",
        Numero = 1,
        Competencia = new DateOnly(2026, 11, 19),
        Emitente = EmitenteDps.Prestador,
        MunicipioEmissor = Municipio,
        Prestador =
        {
            Cpf = Cpf,
            RegimeTributacao = { SimplesNacional = SituacaoSimplesNacional.NaoOptante, RegimeEspecial = RegimeEspecialTributacao.Nenhum },
        },
        Servico =
        {
            Local = { Municipio = Municipio },
            Codigo = { TributacaoNacional = "010101", Descricao = "Desenvolvimento de software" },
        },
        Valores =
        {
            Servico = { Valor = 1000.00m },
            Tributacao =
            {
                Municipal = { Tributacao = TributacaoIssqn.Tributavel, Retencao = RetencaoIssqn.NaoRetido },
                Total = { Indicador = IndicadorTotalTributos.NaoInformado },
            },
        },
    };

    // A: o prestador CNPJ emite; o tomador CPF no exterior; o intermediário com NIF; a primeira alternativa de cada escolha.
    internal static Dps CompletaA()
    {
        var dps = Minima();
        dps.Numero = 42;
        dps.Substituicao = new Substituicao { Chave = Chave, Motivo = MotivoSubstituicao.Outros, Descricao = "Correção de valores" };
        dps.Prestador = new Prestador
        {
            Cnpj = CnpjPrestador,
            Caepf = "12345678901234",
            InscricaoMunicipal = "12345",
            Nome = "PRESTADOR DE TESTE LTDA",
            Endereco = new Endereco
            {
                Nacional = new EnderecoNacional { Municipio = Municipio, Cep = "30110000" },
                Logradouro = "RUA DOS TESTES",
                Numero = "100",
                Complemento = "SALA 2",
                Bairro = "CENTRO",
            },
            Telefone = "31999990000",
            Email = "prestador@exemplo.com.br",
            RegimeTributacao =
            {
                SimplesNacional = SituacaoSimplesNacional.MeEpp,
                ApuracaoSimples = RegimeApuracaoSimples.Simples,
                RegimeEspecial = RegimeEspecialTributacao.Nenhum,
            },
        };
        dps.Tomador = new Pessoa
        {
            Cpf = Cpf,
            Caepf = "98765432109876",
            InscricaoMunicipal = "999",
            Nome = "TOMADOR DE TESTE",
            Endereco = new Endereco
            {
                Exterior = new EnderecoExterior { Pais = "US", CodigoPostal = "10001", Cidade = "NEW YORK", EstadoProvincia = "NY" },
                Logradouro = "5TH AVENUE",
                Numero = "1",
                Bairro = "MANHATTAN",
            },
            Telefone = "12125550000",
            Email = "tomador@exemplo.com",
        };
        dps.Intermediario = new Pessoa { Nif = "123456789", Nome = "INTERMEDIARIO DE TESTE" };
        dps.Servico = new Servico
        {
            Local = { Municipio = Municipio },
            Codigo =
            {
                TributacaoNacional = "010101",
                TributacaoMunicipal = "001",
                Descricao = "Desenvolvimento de software\nsob encomenda",
                Nbs = "115021000",
                CodigoInterno = "SRV01",
            },
            ComercioExterior = new ComercioExterior
            {
                ModoPrestacao = ModoPrestacao.Transfronteirico,
                Vinculo = VinculoPartes.Nenhum,
                Moeda = "220",
                ValorMoeda = 200.00m,
                MecanismoPrestador = MecanismoFomentoPrestador.Nenhum,
                MecanismoTomador = MecanismoFomentoTomador.Zpe,
                MovimentacaoBens = MovimentacaoTemporariaBens.Nao,
                DeclaracaoImportacao = "123",
                RegistroExportacao = "456",
                CompartilharMdic = CompartilhamentoMdic.Nao,
            },
            Obra = new Obra { InscricaoImobiliaria = "IMOB1", Codigo = "CNO123456" },
            Evento = new AtividadeEvento { Nome = "FESTIVAL", Inicio = new DateOnly(2026, 11, 1), Fim = new DateOnly(2026, 11, 3), Identificacao = "EVT1" },
            InformacoesComplementares = new InformacoesComplementares
            {
                ResponsabilidadeTecnica = "ART1",
                DocumentoReferencia = "CONTRATO 7",
                Pedido = "PED1",
                ItensPedido = new ItensPedido { Itens = { "1", "2" } },
                Texto = "Informações complementares",
            },
        };
        dps.Valores = new ValoresDps
        {
            Servico = { ValorRecebido = 10.00m, Valor = 1000.00m },
            Descontos = new Descontos { Incondicionado = 5.00m, Condicionado = 2.50m },
            DeducaoReducao = new DeducaoReducao { Percentual = 10.00m },
            Tributacao =
            {
                Municipal =
                {
                    Tributacao = TributacaoIssqn.Tributavel,
                    PaisResultado = "US",
                    Imunidade = TipoImunidade.NaoInformada,
                    ExigibilidadeSuspensa = new ExigibilidadeSuspensa { Tipo = TipoSuspensao.DecisaoJudicial, Processo = "123456789012345678901234567890" },
                    Beneficio = new BeneficioMunicipal { Identificador = "12345678901234", ValorReducao = 1.00m },
                    Retencao = RetencaoIssqn.Tomador,
                    Aliquota = 2.00m,
                },
                Federal = new TributacaoFederal
                {
                    PisCofins = new PisCofins
                    {
                        Cst = CstPisCofinsServico.AliquotaBasica,
                        BaseCalculo = 1000.00m,
                        AliquotaPis = 0.65m,
                        AliquotaCofins = 3.00m,
                        ValorPis = 6.50m,
                        ValorCofins = 30.00m,
                        Retencao = RetencaoPisCofins.PisCofinsRetidos,
                    },
                    RetencaoCp = 1.00m,
                    RetencaoIrrf = 15.00m,
                    RetencaoCsll = 10.00m,
                },
                Total = { Valores = new TotalTributosValor { Federal = 100.00m, Estadual = 0.00m, Municipal = 20.00m } },
            },
        };
        dps.IbsCbs = new IbsCbsDps
        {
            Finalidade = FinalidadeNFSe.Regular,
            ConsumoPessoal = ConsumoPessoal.Nao,
            CodigoOperacao = "100301",
            TipoOperacao = TipoOperacaoGoverno.FornecimentoPagamentoPosterior,
            Referencias = new NFSeReferenciadas { Chaves = { Chave } },
            EnteGovernamental = EnteGovernamental.Municipio,
            IndicadorDestinatario = IndicadorDestinatario.Outro,
            Destinatario = new Destinatario
            {
                Cnpj = CnpjTomador,
                Nome = "DESTINATARIO DE TESTE",
                Endereco = new Endereco { Nacional = new EnderecoNacional { Municipio = 3550308, Cep = "01310100" }, Logradouro = "AV PAULISTA", Numero = "1000", Bairro = "BELA VISTA" },
                Telefone = "11999990000",
                Email = "destinatario@exemplo.com.br",
            },
            Imovel = new Imovel { InscricaoImobiliaria = "IMOB2", Cib = "12345678" },
            Valores =
            {
                ReembolsoRepasse = new ReembolsoRepasse
                {
                    Documentos =
                    {
                        new DocumentoReembolso
                        {
                            DFeNacional = new DFeNacional { Tipo = TipoDFe.NFe, Chave = ChaveNFe },
                            Fornecedor = new FornecedorDocumento { Cnpj = CnpjTomador, Nome = "FORNECEDOR" },
                            Emissao = new DateOnly(2026, 11, 10),
                            Competencia = new DateOnly(2026, 11, 10),
                            Tipo = TipoReembolso.RepasseTurismo,
                            Valor = 50.00m,
                        },
                    },
                },
                Tributos =
                {
                    Classificacao =
                    {
                        Cst = "000",
                        ClassificacaoTributaria = "000001",
                        CreditoPresumido = "01",
                        TributacaoRegular = new TributacaoRegularIbsCbs { Cst = "000", ClassificacaoTributaria = "000001" },
                        Diferimento = new DiferimentoIbsCbs { PercentualUF = 10.00m, PercentualMunicipio = 20.00m, PercentualCbs = 30.00m },
                    },
                },
            },
        };
        return dps;
    }

    // B: o tomador CNPJ emite (com o motivo e a chave rejeitada); o prestador com NIF no exterior; as segundas alternativas.
    internal static Dps CompletaB()
    {
        var dps = Minima();
        dps.Numero = 43;
        dps.Emitente = EmitenteDps.Tomador;
        dps.MotivoEmissaoTerceiro = MotivoEmissaoTerceiro.ImportacaoServico;
        dps.ChaveRejeitada = Chave;
        dps.Prestador = new Prestador
        {
            Nif = "987654321",
            Nome = "FOREIGN PROVIDER INC",
            Endereco = new Endereco
            {
                Exterior = new EnderecoExterior { Pais = "PT", CodigoPostal = "1000001", Cidade = "LISBOA", EstadoProvincia = "LISBOA" },
                Logradouro = "RUA AUGUSTA",
                Numero = "10",
                Bairro = "BAIXA",
            },
            RegimeTributacao = { SimplesNacional = SituacaoSimplesNacional.NaoOptante, RegimeEspecial = RegimeEspecialTributacao.Outros },
        };
        dps.Tomador = new Pessoa { Cnpj = CnpjTomador, Nome = "TOMADOR LTDA" };
        dps.Intermediario = new Pessoa { MotivoAusenciaNif = MotivoAusenciaNif.Dispensado, Nome = "INTERMEDIARIO" };
        dps.Servico.Local = new LocalPrestacao { Pais = "PT" };
        dps.Servico.Obra = new Obra { Cib = "87654321" };
        dps.Servico.Evento = new AtividadeEvento
        {
            Nome = "CONGRESSO",
            Inicio = new DateOnly(2026, 11, 5),
            Fim = new DateOnly(2026, 11, 6),
            Endereco = new EnderecoEvento { Cep = "30110000", Logradouro = "AV AFONSO PENA", Numero = "1", Complemento = "TEATRO", Bairro = "CENTRO" },
        };
        dps.Valores.DeducaoReducao = new DeducaoReducao { Valor = 100.00m };
        dps.Valores.Tributacao.Municipal.Beneficio = new BeneficioMunicipal { Identificador = "12345678901234", PercentualReducao = 5.00m };
        dps.Valores.Tributacao.Total = new TotalTributos { Percentuais = new TotalTributosPercentual { Federal = 10.00m, Estadual = 0.00m, Municipal = 2.00m } };
        dps.IbsCbs = new IbsCbsDps
        {
            Finalidade = FinalidadeNFSe.Regular,
            CodigoOperacao = "100301",
            IndicadorDestinatario = IndicadorDestinatario.Outro,
            Destinatario = new Destinatario { Cpf = Cpf, Nome = "DESTINATARIO PF" },
            Imovel = new Imovel
            {
                Endereco = new EnderecoObra
                {
                    Exterior = new EnderecoExteriorSimples { CodigoPostal = "1000001", Cidade = "LISBOA", EstadoProvincia = "LISBOA" },
                    Logradouro = "RUA DO OURO",
                    Numero = "5",
                    Bairro = "BAIXA",
                },
            },
            Valores =
            {
                ReembolsoRepasse = new ReembolsoRepasse
                {
                    Documentos =
                    {
                        new DocumentoReembolso
                        {
                            DocumentoFiscal = new DocumentoFiscalOutro { Municipio = Municipio, Numero = "123", Descricao = "NOTA MUNICIPAL" },
                            Emissao = new DateOnly(2026, 11, 10),
                            Competencia = new DateOnly(2026, 11, 10),
                            Tipo = TipoReembolso.RepasseCorretores,
                            Valor = 50.00m,
                        },
                    },
                },
                Tributos = { Classificacao = { Cst = "000", ClassificacaoTributaria = "000001" } },
            },
        };
        return dps;
    }

    // C: o intermediário CPF emite; o prestador com o motivo da ausência do NIF; a obra pelo endereço; os documentos da
    // dedução, um de cada; o percentual do Simples; o documento não fiscal e o DF-e "outro" no reembolso.
    internal static Dps CompletaC()
    {
        var dps = Minima();
        dps.Numero = 44;
        dps.Emitente = EmitenteDps.Intermediario;
        dps.MotivoEmissaoTerceiro = MotivoEmissaoTerceiro.ObrigacaoLegal;
        dps.Prestador = new Prestador
        {
            MotivoAusenciaNif = MotivoAusenciaNif.NaoInformado,
            Nome = "PRESTADOR EXTERIOR",
            RegimeTributacao = { SimplesNacional = SituacaoSimplesNacional.NaoOptante, RegimeEspecial = RegimeEspecialTributacao.Nenhum },
        };
        dps.Tomador = new Pessoa { MotivoAusenciaNif = MotivoAusenciaNif.NaoExigido, Nome = "TOMADOR" };
        dps.Intermediario = new Pessoa { Cpf = Cpf, Nome = "INTERMEDIARIO PF" };
        dps.Servico.Obra = new Obra
        {
            Endereco = new EnderecoObra { Cep = "30110000", Logradouro = "RUA DA OBRA", Numero = "S/N", Bairro = "CENTRO" },
        };
        dps.Servico.Evento = new AtividadeEvento
        {
            Nome = "FEIRA",
            Inicio = new DateOnly(2026, 10, 1),
            Fim = new DateOnly(2026, 10, 2),
            Endereco = new EnderecoEvento
            {
                Exterior = new EnderecoExteriorSimples { CodigoPostal = "75001", Cidade = "PARIS", EstadoProvincia = "IDF" },
                Logradouro = "RUE DE RIVOLI",
                Numero = "1",
                Bairro = "LOUVRE",
            },
        };
        var fornecedor = new Pessoa { Cnpj = CnpjTomador, Nome = "FORNECEDOR" };
        dps.Valores.DeducaoReducao = new DeducaoReducao
        {
            Documentos = new DocumentosDeducao
            {
                Itens =
                {
                    Deducao(d => d.ChaveNFSe = Chave, TipoDeducao.Materiais, fornecedor),
                    Deducao(d => d.ChaveNFe = ChaveNFe, TipoDeducao.Servicos),
                    Deducao(d => d.NFSeMunicipal = new NFSeMunicipal { Municipio = Municipio, Numero = "000000000000123", CodigoVerificacao = "ABC123" }, TipoDeducao.Subempreitada),
                    Deducao(d => d.NotaPapel = new NotaPapel { Numero = "0000123", Modelo = "000000000000001", Serie = "A1" }, TipoDeducao.ProducaoExterna),
                    Deducao(d => d.NumeroDocumentoFiscal = "DF-1", TipoDeducao.ReembolsoDespesas),
                    Deducao(d => { d.NumeroDocumento = "RECIBO-1"; d.DescricaoOutra = "Outra dedução"; }, TipoDeducao.Outras),
                },
            },
        };
        dps.Valores.Tributacao.Total = new TotalTributos { PercentualSimples = 6.00m };
        dps.IbsCbs = new IbsCbsDps
        {
            Finalidade = FinalidadeNFSe.Regular,
            CodigoOperacao = "100301",
            IndicadorDestinatario = IndicadorDestinatario.Outro,
            Destinatario = new Destinatario { Nif = "XY987", Nome = "DESTINATARIO EXTERIOR" },
            Valores =
            {
                ReembolsoRepasse = new ReembolsoRepasse
                {
                    Documentos =
                    {
                        new DocumentoReembolso
                        {
                            DocumentoNaoFiscal = new DocumentoOutro { Numero = "R-1", Descricao = "RECIBO" },
                            Emissao = new DateOnly(2026, 11, 10),
                            Competencia = new DateOnly(2026, 11, 10),
                            Tipo = TipoReembolso.Outros,
                            DescricaoTipo = "Outro reembolso",
                            Valor = 5.00m,
                        },
                        new DocumentoReembolso
                        {
                            DFeNacional = new DFeNacional { Tipo = TipoDFe.Outro, DescricaoTipo = "BP-e", Chave = ChaveNFe },
                            Fornecedor = new FornecedorDocumento { Cpf = Cpf, Nome = "FORNECEDOR PF" },
                            Emissao = new DateOnly(2026, 11, 11),
                            Competencia = new DateOnly(2026, 11, 11),
                            Tipo = TipoReembolso.ReembolsoMidia,
                            Valor = 7.00m,
                        },
                        new DocumentoReembolso
                        {
                            DFeNacional = new DFeNacional { Tipo = TipoDFe.NFSe, Chave = Chave },
                            Fornecedor = new FornecedorDocumento { Nif = "AB123", Nome = "FORNECEDOR EXTERIOR" },
                            Emissao = new DateOnly(2026, 11, 12),
                            Competencia = new DateOnly(2026, 11, 12),
                            Tipo = TipoReembolso.ReembolsoProducaoExterna,
                            Valor = 8.00m,
                        },
                        new DocumentoReembolso
                        {
                            DFeNacional = new DFeNacional { Tipo = TipoDFe.CTe, Chave = ChaveNFe },
                            Fornecedor = new FornecedorDocumento { MotivoAusenciaNif = MotivoAusenciaNif.Dispensado, Nome = "FORNECEDOR SEM NIF" },
                            Emissao = new DateOnly(2026, 11, 13),
                            Competencia = new DateOnly(2026, 11, 13),
                            Tipo = TipoReembolso.RepasseTurismo,
                            Valor = 9.00m,
                        },
                    },
                },
                Tributos = { Classificacao = { Cst = "000", ClassificacaoTributaria = "000001" } },
            },
        };
        return dps;
    }

    // D: o destinatário sem o NIF e o imóvel pelo endereço nacional com o complemento.
    internal static Dps CompletaD()
    {
        var dps = Minima();
        dps.Numero = 45;
        dps.IbsCbs = new IbsCbsDps
        {
            Finalidade = FinalidadeNFSe.Regular,
            CodigoOperacao = "100301",
            IndicadorDestinatario = IndicadorDestinatario.Outro,
            Destinatario = new Destinatario { MotivoAusenciaNif = MotivoAusenciaNif.NaoInformado, Nome = "DESTINATARIO SEM NIF" },
            Imovel = new Imovel
            {
                Endereco = new EnderecoObra { Cep = "30110000", Logradouro = "RUA DO IMOVEL", Numero = "10", Complemento = "APTO 101", Bairro = "CENTRO" },
            },
            Valores = { Tributos = { Classificacao = { Cst = "000", ClassificacaoTributaria = "000001" } } },
        };
        return dps;
    }

    internal static IEnumerable<(string Nome, Func<Dps> Criar)> Todas() =>
    [
        ("Minima", Minima), ("CompletaA", CompletaA), ("CompletaB", CompletaB), ("CompletaC", CompletaC), ("CompletaD", CompletaD),
    ];

    private static DocumentoDeducao Deducao(Action<DocumentoDeducao> documento, TipoDeducao tipo, Pessoa? fornecedor = null)
    {
        var deducao = new DocumentoDeducao
        {
            Tipo = tipo,
            Emissao = new DateOnly(2026, 11, 1),
            ValorDedutivel = 100.00m,
            ValorUtilizado = 10.00m,
            Fornecedor = fornecedor,
        };
        documento(deducao);
        return deducao;
    }
}
