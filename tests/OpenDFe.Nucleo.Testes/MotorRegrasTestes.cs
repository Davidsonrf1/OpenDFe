using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// O motor de regras (REGRAS.md 3 e 7): os passos 5 e 6 do ExecutarAsync com o leiaute falso, cada eco como um
// documento (o eco, pai do infEco assinado).
public class MotorRegrasTestes
{
    private static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = "11222333000181" }.Gerar();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (DFe Nucleo, TransporteFalso Transporte) Criar(
        Action<DeclaracaoLeiaute>? declarar = null, ValidacaoCallback? validacao = null, IReadOnlyCollection<string>? comoAviso = null)
    {
        var transporte = new TransporteFalso();
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = new ConexoesSefaz(transporte),
            Validacao = validacao,
            RegrasComoAviso = comoAviso ?? [],
        };
        return (DFe.Criar(new LeiauteFalso { Acrescimo = declarar }, Documento.NFe, configuracao), transporte);
    }

    private static Problema Erro(string codigo, string mensagem = "Regra.") => new(codigo, Severidade.Erro, mensagem);

    private static Problema Aviso(string codigo, string mensagem = "Regra.") => new(codigo, Severidade.Aviso, mensagem);

    // As regras do leiaute

    [Fact]
    public async Task AsRegrasDoLeiauteRecebemCadaDocumentoAssinado()
    {
        var recebidos = new List<(DocumentoVerificado Documento, ContextoOperacao Contexto)>();
        var (dfe, transporte) = Criar(d => d.DeclararRegras((documento, contexto) =>
        {
            recebidos.Add((documento, contexto));
            return [];
        }));

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["um", "dois"], MontarDocumentos = true }, Ct);

        Assert.True(resultado.Processado);
        Assert.Single(transporte.Envios);
        Assert.Equal(2, recebidos.Count);
        Assert.Equal(["um", "dois"], recebidos.Select(r => r.Documento.Modelo));
        Assert.All(recebidos, r => Assert.Equal(Documento.NFe, r.Documento.Documento));
        Assert.All(recebidos, r => Assert.Null(r.Documento.Chave));
        Assert.All(recebidos, r => Assert.Equal(Documento.NFe, r.Contexto.Documento));
        Assert.All(recebidos, r => Assert.Same(dfe.Configuracao, r.Contexto.Configuracao));

        // O documento é o eco, sozinho, com o namespace declarado e a assinatura dele.
        var primeiro = recebidos[0].Documento.Xml;
        Assert.StartsWith($"<eco xmlns=\"{LeiauteFalso.Namespace}\"><infEco Id=\"ECO1\"><texto>um</texto></infEco><Signature", primeiro, StringComparison.Ordinal);
        Assert.DoesNotContain("ECO2", primeiro, StringComparison.Ordinal);
        Assert.Contains("<Reference URI=\"#ECO2\"", recebidos[1].Documento.Xml, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(primeiro, "ECO1", "11222333000181"));
    }

    [Fact]
    public async Task TodasAsRegrasDeTodosOsDocumentosRodamNaOrdem()
    {
        var (dfe, transporte) = Criar(d =>
        {
            d.DeclararRegras((documento, _) => [Erro("1", $"primeira {documento.Modelo}")]);
            d.DeclararRegras((documento, _) => [Erro("2", $"segunda {documento.Modelo}")]);
        });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["a", "b"], MontarDocumentos = true }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal(["primeira a", "segunda a", "primeira b", "segunda b"], resultado.Problemas.Select(p => p.Mensagem));
        Assert.Empty(transporte.Envios);
        Assert.Contains("<Signature", resultado.Rastro.XmlPedido, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OErroDoSchemaNaoChamaAsRegrasNemOsCallbacks()
    {
        var chamadas = 0;
        var (dfe, transporte) = Criar(
            d => d.DeclararRegras((_, _) => { chamadas++; return []; }),
            new ValidacaoCallback
            {
                Campo = _ => { chamadas++; return []; },
                Documento = _ => { chamadas++; return []; },
            });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = [new string('x', 21)], MontarDocumentos = true }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal(LeiauteFalso.CodigoSchema, Assert.Single(resultado.Problemas).Codigo);
        Assert.Equal(0, chamadas);
        Assert.Empty(transporte.Envios);
    }

    [Fact]
    public async Task OErroDasRegrasNaoChamaOsCallbacks()
    {
        var chamadas = 0;
        var (dfe, _) = Criar(
            d => d.DeclararRegras((_, _) => [Erro("564")]),
            new ValidacaoCallback
            {
                Campo = _ => { chamadas++; return []; },
                Documento = _ => { chamadas++; return []; },
            });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal(0, chamadas);
    }

    [Fact]
    public async Task OsAvisosDeCadaEtapaSeguemNaOrdem()
    {
        var (dfe, _) = Criar(
            d => d.DeclararRegras((_, _) => [Aviso("R")]),
            new ValidacaoCallback { Documento = _ => [Aviso("C")] });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { MontarDocumentos = true, ProblemaMontagem = Aviso("M") }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(["M", "R", "C"], resultado.Problemas.Select(p => p.Codigo));
    }

    [Fact]
    public async Task SemDocumentosNadaEChamado()
    {
        var chamadas = 0;
        var (dfe, transporte) = Criar(
            d => d.DeclararRegras((_, _) => { chamadas++; return [Erro("1")]; }),
            new ValidacaoCallback
            {
                Campo = _ => { chamadas++; return [Erro("2")]; },
                Documento = _ => { chamadas++; return [Erro("3")]; },
            });

        Assert.True((await dfe.ExecutarAsync(new EcoFalso(), Ct)).Processado);
        Assert.True((await dfe.ExecutarAsync(new StatusFalso(), Ct)).Processado);
        Assert.Equal(0, chamadas);
        Assert.Equal(2, transporte.Envios.Count);
    }

    // RegrasComoAviso

    [Fact]
    public async Task RegrasComoAvisoRebaixaSoOCodigoDela()
    {
        var (dfe, transporte) = Criar(d => d.DeclararRegras((_, _) =>
            [new Problema("564", Severidade.Erro, "Total.") { Tag = "vProd", Caminho = "eco/infEco/texto", Propriedade = "Texto" }]),
            comoAviso: ["564"]);

        var resultado = await dfe.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct);

        Assert.True(resultado.Processado);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("564", Severidade.Aviso, "Total.", "vProd", "eco/infEco/texto", "Texto"),
            (problema.Codigo, problema.Severidade, problema.Mensagem, problema.Tag, problema.Caminho, problema.Propriedade));
        Assert.Single(transporte.Envios);

        var (outro, _) = Criar(d => d.DeclararRegras((_, _) => [Erro("564"), Erro("999")]), comoAviso: ["564"]);
        var recusado = await outro.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, recusado.Situacao);
        Assert.Equal([Severidade.Aviso, Severidade.Erro], recusado.Problemas.Select(p => p.Severidade));
    }

    [Fact]
    public async Task RegrasComoAvisoNaoValeParaOSchemaNemParaOsCallbacks()
    {
        var (dfe, _) = Criar(
            validacao: new ValidacaoCallback { Documento = _ => [Erro("564")] },
            comoAviso: ["564", LeiauteFalso.CodigoSchema]);

        var schema = await dfe.ExecutarAsync(new EcoFalso { Textos = [new string('x', 21)], MontarDocumentos = true }, Ct);
        Assert.Equal(Severidade.Erro, Assert.Single(schema.Problemas).Severidade);

        var callback = await dfe.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, callback.Situacao);
        Assert.Equal(Severidade.Erro, Assert.Single(callback.Problemas).Severidade);
    }

    [Fact]
    public void OCriarRecusaARegraComoAvisoNulaOuVazia()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        ConfiguracaoDFe Configurar(IReadOnlyCollection<string> codigos) => new()
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, RegrasComoAviso = codigos,
        };

        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar(null!)));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar(["564", ""])));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar([null!])));
    }

    [Fact]
    public async Task OCriarCopiaAsRegrasComoAviso()
    {
        var codigos = new List<string> { "564" };
        var transporte = new TransporteFalso();
        var dfe = DFe.Criar(new LeiauteFalso { Acrescimo = d => d.DeclararRegras((_, _) => [Erro("564")]) }, Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = new ConexoesSefaz(transporte), RegrasComoAviso = codigos,
        });
        codigos.Clear();

        Assert.True((await dfe.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct)).Processado);
    }

    // Os callbacks

    [Fact]
    public async Task OCallbackDeCampoRecebeCadaCampoForaDaAssinaturaEDepoisODeDocumento()
    {
        var chamadas = new List<string>();
        var campos = new List<CampoVerificado>();
        var (dfe, _) = Criar(validacao: new ValidacaoCallback
        {
            Campo = c => { campos.Add(c); chamadas.Add("campo " + c.Caminho); return []; },
            Documento = d => { chamadas.Add("documento " + d.Modelo); return []; },
        });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["um", "dois"], MontarDocumentos = true }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(
            ["campo eco/infEco/@Id", "campo eco/infEco/texto", "documento um", "campo eco/infEco/@Id", "campo eco/infEco/texto", "documento dois"],
            chamadas);
        Assert.Equal(
            [("Id", "Id", "ECO1"), ("texto", "Texto", "um"), ("Id", "Id", "ECO2"), ("texto", "Texto", "dois")],
            campos.Select(c => (c.Tag, c.Propriedade, c.Texto)));
        Assert.All(campos, c => Assert.Equal(Documento.NFe, c.Documento));
    }

    [Fact]
    public async Task OErroDoCallbackRecusaEOAvisoPassa()
    {
        var (recusa, transporte) = Criar(validacao: new ValidacaoCallback
        {
            Campo = c => c.Tag == "texto" && c.Texto == "TESTE" ? [new Problema("PDV-01", Severidade.Erro, "Teste.") { Caminho = c.Caminho }] : [],
        });
        var recusado = await recusa.ExecutarAsync(new EcoFalso { Textos = ["ok", "TESTE"], MontarDocumentos = true }, Ct);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, recusado.Situacao);
        Assert.Equal("eco/infEco/texto", Assert.Single(recusado.Problemas).Caminho);
        Assert.Empty(transporte.Envios);

        var (passa, _) = Criar(validacao: new ValidacaoCallback { Documento = _ => [Aviso("PDV-02")] });
        var processado = await passa.ExecutarAsync(new EcoFalso { Textos = ["a", "b"], MontarDocumentos = true }, Ct);
        Assert.True(processado.Processado);
        Assert.Equal(["PDV-02", "PDV-02"], processado.Problemas.Select(p => p.Codigo));
    }

    [Fact]
    public void OsCamposSaemComOCaminhoDoSchema()
    {
        const string Xml =
            "<lote xmlns=\"urn:a\"><doc versao=\"1\"><inf Id=\"A\"><x>1</x><x>2</x><y a=\"b\"></y><z></z><g><w>3</w></g></inf>"
            + "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><v>s</v></Signature></doc></lote>";
        var pedido = new DocumentoPedido("A") { Propriedade = c => c == "doc/inf/x[2]" ? "X[1]" : null };

        var achado = Assert.Single(MotorRegras.Achar(Xml, [pedido], Documento.NFCe));
        var campos = MotorRegras.Campos(achado).ToList();

        Assert.Equal(
            [
                ("versao", "doc/@versao", null, "1"),
                ("Id", "doc/inf/@Id", null, "A"),
                ("x", "doc/inf/x[1]", null, "1"),
                ("x", "doc/inf/x[2]", "X[1]", "2"),
                ("a", "doc/inf/y/@a", null, "b"),
                ("y", "doc/inf/y", null, ""),
                ("z", "doc/inf/z", null, ""),
                ("w", "doc/inf/g/w", null, "3"),
            ],
            campos.Select(c => (c.Tag, c.Caminho, c.Propriedade, c.Texto)));
        Assert.All(campos, c => Assert.Equal(Documento.NFCe, c.Documento));
        Assert.StartsWith("<doc versao=\"1\" xmlns=\"urn:a\">", achado.Verificado.Xml, StringComparison.Ordinal);
    }

    // Recusas e exceções

    [Fact]
    public async Task ODocumentoQueNaoEstaNaMensagemEErroDoLeiaute()
    {
        var (dfe, transporte) = Criar();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dfe.ExecutarAsync(new EcoFalso { DocumentosPedido = [new DocumentoPedido("NAO-EXISTE")] }, Ct));
        Assert.Empty(transporte.Envios);
        Assert.Throws<InvalidOperationException>(() =>
            MotorRegras.Achar("<raiz Id=\"A\"></raiz>", [new DocumentoPedido("A")], Documento.NFe));
        Assert.Throws<InvalidOperationException>(() =>
            MotorRegras.Achar("<r><a><b Id=\"A\"/></a><a><b Id=\"A\"/></a></r>", [new DocumentoPedido("A")], Documento.NFe));
    }

    [Fact]
    public async Task AListaNulaOuComProblemaNuloEErro()
    {
        var (nula, _) = Criar(d => d.DeclararRegras((_, _) => null!));
        await Assert.ThrowsAsync<InvalidOperationException>(() => nula.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct));

        var (problemaNulo, _) = Criar(d => d.DeclararRegras((_, _) => [null!]));
        await Assert.ThrowsAsync<InvalidOperationException>(() => problemaNulo.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct));

        var (callbackNulo, _) = Criar(validacao: new ValidacaoCallback { Campo = _ => null! });
        await Assert.ThrowsAsync<InvalidOperationException>(() => callbackNulo.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct));
    }

    [Fact]
    public async Task AExcecaoDasRegrasEDosCallbacksSobeComoVeio()
    {
        var (regra, _) = Criar(d => d.DeclararRegras((_, _) => throw new FormatException("regra")));
        Assert.Equal("regra", (await Assert.ThrowsAsync<FormatException>(() => regra.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct))).Message);

        var (callback, _) = Criar(validacao: new ValidacaoCallback { Documento = _ => throw new ArithmeticException("callback") });
        Assert.Equal("callback", (await Assert.ThrowsAsync<ArithmeticException>(() => callback.ExecutarAsync(new EcoFalso { MontarDocumentos = true }, Ct))).Message);
    }

    [Fact]
    public void OPedidoRecusaDocumentoNuloOuRepetido()
    {
        static PedidoWS Pedido(IReadOnlyList<DocumentoPedido> documentos) =>
            new("EcoFalso", "<x/>", LeiauteFalso.Schema) { Documentos = documentos };

        Assert.Throws<ArgumentNullException>(() => Pedido(null!));
        Assert.Throws<ArgumentNullException>(() => Pedido([null!]));
        Assert.Throws<ArgumentException>(() => Pedido([new DocumentoPedido("A"), new DocumentoPedido("A")]));
        Assert.Equal(["A", "B"], Pedido([new DocumentoPedido("A"), new DocumentoPedido("B")]).Documentos.Select(d => d.Id));
        Assert.Empty(new PedidoWS("EcoFalso", "<x/>", LeiauteFalso.Schema).Documentos);
        Assert.ThrowsAny<ArgumentException>(() => new DocumentoPedido(""));
        Assert.ThrowsAny<ArgumentException>(() => new DocumentoPedido(null!));
    }

    [Fact]
    public void ADeclaracaoRecusaARegraNulaEForaDoDeclarar()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DeclaracaoLeiaute.Criar(new LeiauteFalso { Acrescimo = d => d.DeclararRegras(null!) }, Documento.NFe));

        var leiaute = new LeiauteFalso();
        DeclaracaoLeiaute.Criar(leiaute, Documento.NFe);
        Assert.Throws<InvalidOperationException>(() => leiaute.Ultima!.DeclararRegras((_, _) => []));
    }

    [Fact]
    public async Task AtendeChamadasEmParalelo()
    {
        var regras = 0;
        var campos = 0;
        var (dfe, transporte) = Criar(
            d => d.DeclararRegras((_, _) => { Interlocked.Increment(ref regras); return [Aviso("R")]; }),
            new ValidacaoCallback { Campo = _ => { Interlocked.Increment(ref campos); return []; } });

        var resultados = await Task.WhenAll(Enumerable.Range(0, 20).Select(i =>
            dfe.ExecutarAsync(new EcoFalso { Textos = ["a" + i, "b" + i], MontarDocumentos = true }, Ct)));

        Assert.All(resultados, r => Assert.True(r.Processado));
        Assert.All(resultados, r => Assert.Equal(2, r.Problemas.Count));
        Assert.Equal(40, regras);
        Assert.Equal(80, campos);
        Assert.Equal(20, transporte.Envios.Count);
    }
}
