using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A consulta cadastro (etapa 8.2, CONTRATOS.md 24), pelo ExecutarAsync com o handler que responde conforme o pedido: o ConsCad
// por CNPJ, CPF e inscrição, as recusas do construtor, o destino por UF, o NFE-CADASTRO e a leitura do retConsCad. Contra a
// SEFAZ emulada, em CadastroEmuladoTestes.
public partial class AutorizacaoTestes
{
    private const string CnpjContribuinte = "98765432000198";
    private const string InscricaoContribuinte = "0623079040081";

    private static readonly Lazy<SchemaXml> SchemaRetornoCadastro = new(() =>
        SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("CadConsultaCadastro/retConsCad_v2.00.xsd"), "215"));

    // O retConsCad na ordem da Tabela 5-22.
    private static string RetornoCadastro(int status, string motivo, string uf = "MG", string argumento = $"<CNPJ>{CnpjContribuinte}</CNPJ>",
        string contribuintes = "", string orgao = "31") =>
        "<retConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"><infCons><verAplic>MG-TESTE</verAplic>"
        + $"<cStat>{status}</cStat><xMotivo>{motivo}</xMotivo><UF>{uf}</UF>{argumento}<dhCons>2026-10-20T15:30:02-03:00</dhCons>"
        + $"<cUF>{orgao}</cUF>{contribuintes}</infCons></retConsCad>";

    private static string ContribuinteCompleto(string inscricao = InscricaoContribuinte, string cnpj = CnpjContribuinte) =>
        $"<infCad><IE>{inscricao}</IE><CNPJ>{cnpj}</CNPJ><UF>MG</UF><cSit>1</cSit><indCredNFe>1</indCredNFe><indCredCTe>4</indCredCTe>"
        + "<xNome>CONTRIBUINTE DE TESTE LTDA</xNome><xFant>TESTE</xFant><xRegApur>NORMAL</xRegApur><CNAE>4751201</CNAE>"
        + "<dIniAtiv>2010-03-01</dIniAtiv><dUltSit>2020-05-10</dUltSit><dBaixa>2026-01-31</dBaixa><IEUnica>0623079040999</IEUnica>"
        + "<IEAtual>0623079040082</IEAtual><ender><xLgr>RUA DE TESTE</xLgr><nro>100</nro><xCpl>SALA 2</xCpl><xBairro>CENTRO</xBairro>"
        + "<cMun>3106200</cMun><xMun>BELO HORIZONTE</xMun><CEP>30110000</CEP></ender></infCad>";

    private static string ContribuinteMinimo(string inscricao = InscricaoContribuinte, string documento = $"<CNPJ>{CnpjContribuinte}</CNPJ>") =>
        $"<infCad><IE>{inscricao}</IE>{documento}<UF>MG</UF><cSit>1</cSit><indCredNFe>1</indCredNFe><indCredCTe>4</indCredCTe>"
        + "<xNome>CONTRIBUINTE DE TESTE LTDA</xNome></infCad>";

    private static Func<string, string> RespostaCadastro(string retorno) => _ => retorno;

    // O pedido

    [Fact]
    public void AConsultaCadastroValeSoNaNFe()
    {
        using var conexoes = new ConexoesSefaz();
        var nfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));
        var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configuracao(conexoes));

        Assert.True(nfe.Suporta<ConsultaCadastro>());
        Assert.False(nfce.Suporta<ConsultaCadastro>());
    }

    [Fact]
    public async Task OConsCadPorCnpjSaiNoSchemaSemAssinaturaENemAmbiente()
    {
        var (resultado, envio) = await Executar(new ConsultaCadastro(UF.MG, "98.765.432/0001-98"), RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: ContribuinteMinimo())));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Contains("CadConsultaCadastro4", envio!.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(SchemasNFe400.ConsultaCadastro.Validar(envio.Mensagem));
        Assert.Equal("<ConsCad versao=\"2.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infCons><xServ>CONS-CAD</xServ><UF>MG</UF>"
            + $"<CNPJ>{CnpjContribuinte}</CNPJ></infCons></ConsCad>", envio.Mensagem);
        Assert.DoesNotContain("Signature", envio.Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("tpAmb", envio.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OConsCadPorCpfEPorInscricaoLevamOArgumentoCerto()
    {
        var resposta = RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia"));

        var (cpf, envioCpf) = await Executar(new ConsultaCadastro(UF.SP, "529.982.247-25"), resposta);
        var (ie, envioIe) = await Executar(ConsultaCadastro.Inscricao(UF.SP, InscricaoContribuinte), resposta);

        Assert.True(cpf.Processado && ie.Processado);
        Assert.Contains("<UF>SP</UF><CPF>52998224725</CPF>", envioCpf!.Mensagem, StringComparison.Ordinal);
        Assert.Contains($"<UF>SP</UF><IE>{InscricaoContribuinte}</IE>", envioIe!.Mensagem, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.ConsultaCadastro.Validar(envioCpf.Mensagem));
        Assert.Empty(SchemasNFe400.ConsultaCadastro.Validar(envioIe.Mensagem));
    }

    [Fact]
    public async Task OCnpjAlfanumericoVaiSemMascara()
    {
        var (resultado, envio) = await Executar(new ConsultaCadastro(UF.MG, "12.ABC.345/01DE-35"), RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia")));

        Assert.True(resultado.Processado);
        Assert.Contains("<CNPJ>12ABC34501DE35</CNPJ>", envio!.Mensagem, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.ConsultaCadastro.Validar(envio.Mensagem));
    }

    [Fact]
    public void OsParametrosDaConsultaCadastroRecusamOQueASefazRejeitaria()
    {
        Assert.Throws<ArgumentNullException>(() => new ConsultaCadastro(UF.MG, null!));
        Assert.Throws<ArgumentNullException>(() => ConsultaCadastro.Inscricao(UF.MG, null!));
        // o DV, o tamanho e a mistura de CNPJ com CPF
        foreach (var documento in new[] { "", "98765432000199", "9876543200019", "529.982.247-26", "5299822472", "abc", "00000000000000" })
            Assert.Throws<ArgumentException>(() => new ConsultaCadastro(UF.MG, documento));
        // a inscrição: 2 a 14 algarismos, nem toda zero, nem ISENTO, nem com máscara
        foreach (var inscricao in new[] { "", "1", "123456789012345", "00", "00000000000000", "ISENTO", "062.307.904/0081", "06230790A0081" })
            Assert.Throws<ArgumentException>(() => ConsultaCadastro.Inscricao(UF.MG, inscricao));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaCadastro((UF)99, CnpjContribuinte));
        Assert.Throws<ArgumentOutOfRangeException>(() => ConsultaCadastro.Inscricao((UF)99, InscricaoContribuinte));
        Assert.Equal("12", ConsultaCadastro.Inscricao(UF.MG, "12").InscricaoEstadual);
        Assert.Equal(new string('9', 14), ConsultaCadastro.Inscricao(UF.MG, new string('9', 14)).InscricaoEstadual);
    }

    [Fact]
    public void ADuasFormasDaConsultaGuardamOArgumentoEAUf()
    {
        var porCnpj = new ConsultaCadastro(UF.GO, "98.765.432/0001-98");
        var porIe = ConsultaCadastro.Inscricao(UF.BA, InscricaoContribuinte);

        Assert.Equal((UF.GO, CnpjContribuinte, null), (porCnpj.UF, porCnpj.Documento, porCnpj.InscricaoEstadual));
        Assert.Equal((UF.BA, null, InscricaoContribuinte), (porIe.UF, porIe.Documento, porIe.InscricaoEstadual));
    }

    // O destino

    [Theory]
    [InlineData(UF.MG, Destino.MG, "hnfe.fazenda.mg.gov.br")]
    [InlineData(UF.SP, Destino.SP, "homologacao.nfe.fazenda.sp.gov.br")]
    [InlineData(UF.AM, Destino.AM, "homnfe.sefaz.am.gov.br")]
    [InlineData(UF.ES, Destino.SVRS, "cad-homologacao.svrs.rs.gov.br")]
    [InlineData(UF.AC, Destino.SVRS, "cad-homologacao.svrs.rs.gov.br")]
    [InlineData(UF.RN, Destino.SVRS, "cad-homologacao.svrs.rs.gov.br")]
    [InlineData(UF.PB, Destino.SVRS, "cad-homologacao.svrs.rs.gov.br")]
    [InlineData(UF.SC, Destino.SVRS, "cad-homologacao.svrs.rs.gov.br")]
    public async Task AConsultaVaiAoAutorizadorDaUfConsultadaEOpcaoDoNucleoNaoImporta(UF uf, Destino autorizador, string servidor)
    {
        var (resultado, envio) = await Executar(new ConsultaCadastro(uf, CnpjContribuinte), RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", uf.Sigla())));

        Assert.True(resultado.Processado);
        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        Assert.Equal(servidor, envio!.Url.Host);
    }

    [Theory]
    [InlineData(UF.MA)]
    [InlineData(UF.RJ)]
    [InlineData(UF.CE)]
    [InlineData(UF.DF)]
    [InlineData(UF.PA)]
    [InlineData(UF.TO)]
    public async Task AUfSemOServicoDeCadastroERecusadaComNFeCadastro(UF uf)
    {
        var (resultado, envio) = await Executar(new ConsultaCadastro(uf, CnpjContribuinte), RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia")));

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CADASTRO", Severidade.Erro, "UF", "UF"), (problema.Codigo, problema.Severidade, problema.Tag, problema.Propriedade));
        Assert.Equal("ConsCad/infCons/UF", problema.Caminho);
        Assert.Contains(uf.Sigla(), problema.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ComODestinoInformadoANFeCadastroNaoVale()
    {
        var consulta = new ConsultaCadastro(UF.RJ, CnpjContribuinte) { Destino = Destino.SVRS };

        var (resultado, envio) = await Executar(consulta, RespostaCadastro(RetornoCadastro(265, "Rejeicao: UF nao atendida", "RJ")));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(Destino.SVRS, resultado.Rastro.Autorizador);
        Assert.Contains("<UF>RJ</UF>", envio!.Mensagem, StringComparison.Ordinal);
        Assert.Equal(265, resultado.Retorno!.Status);
    }

    // O retorno

    [Fact]
    public async Task O111TrazOContribuinteComTodosOsCamposEOEndereco()
    {
        var retorno = RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: ContribuinteCompleto());
        Assert.Empty(SchemaRetornoCadastro.Value.Validar(retorno));

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte), RespostaCadastro(retorno));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(111, resultado.CStat);
        var r = resultado.Retorno!;
        Assert.Equal((111, "Consulta cadastro com uma ocorrencia", "MG-TESTE", UF.MG, UF.MG, true),
            (r.Status, r.Motivo, r.VersaoAplicativo, r.UF, r.UFAtendimento, r.Encontrado));
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 15, 30, 2, TimeSpan.FromHours(-3)), r.Consulta);
        var c = Assert.Single(r.Contribuintes);
        Assert.Equal((InscricaoContribuinte, CnpjContribuinte, (string?)null, UF.MG), (c.InscricaoEstadual, c.Cnpj, c.Cpf, c.UF));
        Assert.Equal((SituacaoCadastral.Habilitado, CredenciamentoDFe.Credenciado, CredenciamentoDFe.NaoInformado),
            (c.Situacao, c.CredenciamentoNFe, c.CredenciamentoCTe));
        Assert.Equal(("CONTRIBUINTE DE TESTE LTDA", "TESTE", "NORMAL", "4751201"), (c.Nome, c.Fantasia, c.RegimeApuracao, c.Cnae));
        Assert.Equal((new DateOnly(2010, 3, 1), new DateOnly(2020, 5, 10), new DateOnly(2026, 1, 31)), (c.InicioAtividade, c.UltimaSituacao, c.Baixa));
        Assert.Equal(("0623079040999", "0623079040082"), (c.InscricaoUnica, c.InscricaoAtual));
        var e = c.Endereco!;
        Assert.Equal(("RUA DE TESTE", "100", "SALA 2", "CENTRO", "3106200", "BELO HORIZONTE", "30110000"),
            (e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.CodigoMunicipio, e.Municipio, e.Cep));
    }

    [Fact]
    public async Task O112TrazMaisDeUmContribuinteNaOrdemDoRetorno()
    {
        var retorno = RetornoCadastro(112, "Consulta cadastro com mais de uma ocorrencia",
            contribuintes: ContribuinteMinimo("0623079040081") + ContribuinteMinimo("0623079040155"));
        Assert.Empty(SchemaRetornoCadastro.Value.Validar(retorno));

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte), RespostaCadastro(retorno));

        var r = resultado.Retorno!;
        Assert.Equal((112, true), (r.Status, r.Encontrado));
        Assert.Equal(["0623079040081", "0623079040155"], r.Contribuintes.Select(c => c.InscricaoEstadual));
    }

    [Fact]
    public async Task OContribuinteComCpfEIsentoELidoSemOsCamposOpcionais()
    {
        var retorno = RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", argumento: "<CPF>52998224725</CPF>",
            contribuintes: ContribuinteMinimo("ISENTO", "<CPF>52998224725</CPF>"));
        Assert.Empty(SchemaRetornoCadastro.Value.Validar(retorno));

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, "52998224725"), RespostaCadastro(retorno));

        Assert.Empty(resultado.Problemas);
        var c = Assert.Single(resultado.Retorno!.Contribuintes);
        Assert.Equal(("ISENTO", (string?)null, "52998224725"), (c.InscricaoEstadual, c.Cnpj, c.Cpf));
        Assert.Null(c.Fantasia);
        Assert.Null(c.RegimeApuracao);
        Assert.Null(c.Cnae);
        Assert.Null(c.InicioAtividade);
        Assert.Null(c.UltimaSituacao);
        Assert.Null(c.Baixa);
        Assert.Null(c.InscricaoUnica);
        Assert.Null(c.InscricaoAtual);
        Assert.Null(c.Endereco);
    }

    [Fact]
    public async Task OEnderecoPodeVirSoComAlgunsCampos()
    {
        var contribuinte = ContribuinteMinimo().Replace("</infCad>", "<ender><xLgr>RUA A</xLgr><CEP>3011000</CEP></ender></infCad>", StringComparison.Ordinal);

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte),
            RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: contribuinte)));

        Assert.Empty(resultado.Problemas);
        var e = Assert.Single(resultado.Retorno!.Contribuintes).Endereco!;
        Assert.Equal(("RUA A", "3011000"), (e.Logradouro, e.Cep));
        Assert.Null(e.Numero);
        Assert.Null(e.Municipio);
    }

    [Theory]
    [InlineData(257)]
    [InlineData(258)]
    [InlineData(259)]
    [InlineData(260)]
    [InlineData(261)]
    [InlineData(262)]
    [InlineData(263)]
    [InlineData(264)]
    [InlineData(265)]
    [InlineData(108)]
    [InlineData(109)]
    public async Task AsRejeicoesVoltamSemContribuintes(int status)
    {
        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte), RespostaCadastro(RetornoCadastro(status, "Rejeicao " + status)));

        Assert.True(resultado.Processado);
        Assert.Equal(status, resultado.CStat);
        var r = resultado.Retorno!;
        Assert.Equal((status, false), (r.Status, r.Encontrado));
        Assert.Empty(r.Contribuintes);
    }

    [Fact]
    public async Task OValorQueOMocNaoDefineEntraComoONumeroComAviso()
    {
        var contribuinte = ContribuinteMinimo().Replace("<cSit>1</cSit>", "<cSit>7</cSit>", StringComparison.Ordinal)
            .Replace("<indCredNFe>1</indCredNFe>", "<indCredNFe>9</indCredNFe>", StringComparison.Ordinal);

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte),
            RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: contribuinte)));

        Assert.True(resultado.Processado);
        var c = Assert.Single(resultado.Retorno!.Contribuintes);
        Assert.Equal(7, (int)c.Situacao!.Value);
        Assert.Equal(9, (int)c.CredenciamentoNFe!.Value);
        Assert.Equal(CredenciamentoDFe.NaoInformado, c.CredenciamentoCTe);
        Assert.Equal(2, resultado.Problemas.Count(p => p.Codigo == "NFE-VALOR" && p.Severidade == Severidade.Aviso));
        Assert.Contains(resultado.Problemas, p => p.Tag == "cSit" && p.Caminho == "retConsCad/infCons/infCad[1]/cSit");
    }

    [Fact]
    public async Task ACampoQueNaoSeLeFicaNuloComAvisoEOQueOLeiauteNaoConheceTambem()
    {
        var contribuinte = ContribuinteMinimo().Replace("<cSit>1</cSit>", "<cSit>x</cSit><novo>1</novo>", StringComparison.Ordinal)
            .Replace("</infCad>", "<dIniAtiv>01/03/2010</dIniAtiv></infCad>", StringComparison.Ordinal);
        var retorno = RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: contribuinte)
            .Replace("<dhCons>2026-10-20T15:30:02-03:00</dhCons>", "<dhCons>ontem</dhCons><extra>1</extra>", StringComparison.Ordinal);

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte), RespostaCadastro(retorno));

        Assert.True(resultado.Processado);
        Assert.All(resultado.Problemas, p => Assert.Equal(Severidade.Aviso, p.Severidade));
        var r = resultado.Retorno!;
        Assert.Null(r.Consulta);
        var c = Assert.Single(r.Contribuintes);
        Assert.Null(c.Situacao);
        Assert.Null(c.InicioAtividade);
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-VALOR", Tag: "dhCons" });
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-VALOR", Tag: "cSit" });
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-VALOR", Tag: "dIniAtiv" });
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-DESCONHECIDO", Tag: "novo" });
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-DESCONHECIDO", Tag: "extra" });
    }

    [Theory]
    [InlineData("<retConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"/>")]
    [InlineData("<retConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"><infCons><verAplic>X</verAplic></infCons></retConsCad>")]
    [InlineData("<retConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"><infCons><cStat>111</cStat></infCons></retConsCad>")]
    [InlineData("<retConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infCons>")]
    [InlineData("<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>")]
    public async Task ARespostaIlegivelDaConsultaCadastroELancada(string retorno)
    {
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte), RespostaCadastro(retorno)));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
    }

    [Fact]
    public async Task OContribuinteSemIeENomeVemVazioComAviso()
    {
        var contribuinte = "<infCad><CNPJ>" + CnpjContribuinte + "</CNPJ><UF>MG</UF><cSit>1</cSit></infCad>";

        var (resultado, _) = await Executar(new ConsultaCadastro(UF.MG, CnpjContribuinte),
            RespostaCadastro(RetornoCadastro(111, "Consulta cadastro com uma ocorrencia", contribuintes: contribuinte)));

        var c = Assert.Single(resultado.Retorno!.Contribuintes);
        Assert.Equal(("", ""), (c.InscricaoEstadual, c.Nome));
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-VALOR", Tag: "IE", Severidade: Severidade.Aviso });
        Assert.Contains(resultado.Problemas, p => p is { Codigo: "NFE-VALOR", Tag: "xNome" });
        Assert.Equal(CnpjContribuinte, c.Cnpj);
    }
}
