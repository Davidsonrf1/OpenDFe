using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// O preparo (etapa 4.4, CONTRATOS.md 14.2): o DFe.Preparar com o leiaute falso, que monta um eco sozinho. O caminho é o
// do ExecutarAsync até os callbacks, sem transmitir.
public class PreparoTestes
{
    private static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = "11222333000181" }.Gerar();

    private static (DFe Nucleo, TransporteFalso Transporte) Criar(Action<DeclaracaoLeiaute>? declarar = null, ValidacaoCallback? validacao = null)
    {
        var transporte = new TransporteFalso();
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = new ConexoesSefaz(transporte),
            Validacao = validacao,
        };
        return (DFe.Criar(new LeiauteFalso { Acrescimo = declarar }, Documento.NFe, configuracao), transporte);
    }

    [Fact]
    public void ODocumentoSaiAssinadoEValidoSemTransmitir()
    {
        var (dfe, transporte) = Criar();
        var documento = new TextoFalso { Texto = "um" };

        var preparado = dfe.Preparar(documento);

        Assert.True(preparado.Preparado);
        Assert.Empty(preparado.Problemas);
        Assert.Equal(PreparoEcoFalso.Chave, preparado.Chave);
        Assert.StartsWith($"<eco xmlns=\"{LeiauteFalso.Namespace}\"><infEco Id=\"ECO1\"><texto>um</texto></infEco><Signature", preparado.Xml, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(preparado.Xml!, "ECO1", "11222333000181"));
        Assert.Empty(LeiauteFalso.Schema.Validar(preparado.Xml!));
        Assert.Empty(transporte.Envios);
        Assert.Same(dfe.Configuracao, documento.ContextoRecebido!.Configuracao);
        Assert.Equal(Documento.NFe, documento.ContextoRecebido.Documento);
    }

    [Fact]
    public void OErroDaMontagemParaAntesDeAssinar()
    {
        var (dfe, _) = Criar();
        var erro = new Problema("M1", Severidade.Erro, "Falta o texto.");

        var preparado = dfe.Preparar(new TextoFalso { ProblemaMontagem = erro });

        Assert.False(preparado.Preparado);
        Assert.Null(preparado.Xml);
        Assert.Same(erro, Assert.Single(preparado.Problemas));
        Assert.Equal(PreparoEcoFalso.Chave, preparado.Chave);
    }

    [Fact]
    public void OAvisoDaMontagemSegueNoPreparado()
    {
        var (dfe, _) = Criar();

        var preparado = dfe.Preparar(new TextoFalso { ProblemaMontagem = new Problema("M2", Severidade.Aviso, "Campo não suportado.") });

        Assert.True(preparado.Preparado);
        Assert.Equal("M2", Assert.Single(preparado.Problemas).Codigo);
    }

    [Fact]
    public void OErroDoSchemaDevolveOXmlAssinadoENaoChamaAsRegras()
    {
        var chamadas = 0;
        var (dfe, _) = Criar(d => d.DeclararRegras((_, _) => { chamadas++; return []; }));

        var preparado = dfe.Preparar(new TextoFalso { Texto = new string('x', 21) });

        Assert.False(preparado.Preparado);
        var problema = Assert.Single(preparado.Problemas);
        Assert.Equal((LeiauteFalso.CodigoSchema, "eco/infEco/texto"), (problema.Codigo, problema.Caminho));
        Assert.Contains("<Signature", preparado.Xml, StringComparison.Ordinal);
        Assert.Equal(0, chamadas);
    }

    [Fact]
    public void AsRegrasEOsCallbacksRodamNoDocumento()
    {
        var documentos = new List<DocumentoVerificado>();
        var campos = new List<CampoVerificado>();
        var (dfe, _) = Criar(
            d => d.DeclararRegras((documento, _) =>
            {
                documentos.Add(documento);
                return [new Problema("R1", Severidade.Aviso, "Regra.") { Caminho = "eco/infEco/texto", Propriedade = "Texto" }];
            }),
            new ValidacaoCallback
            {
                Campo = c => { campos.Add(c); return []; },
                Documento = d => [new Problema("C1", Severidade.Erro, "Do sistema.")],
            });

        var preparado = dfe.Preparar(new TextoFalso { Texto = "dois" });

        Assert.False(preparado.Preparado);
        Assert.Equal(["R1", "C1"], preparado.Problemas.Select(p => p.Codigo));
        Assert.Equal("Texto", preparado.Problemas[0].Propriedade);
        var documento = Assert.Single(documentos);
        Assert.Equal(("dois", PreparoEcoFalso.Chave), (documento.Modelo, documento.Chave));
        Assert.Contains(campos, c => (c.Tag, c.Propriedade, c.Texto) == ("texto", "Texto", "dois"));
    }

    [Fact]
    public void ORegistroEAsRecusas()
    {
        var (dfe, _) = Criar();

        Assert.Throws<ArgumentNullException>(() => dfe.Preparar<TextoFalso>(null!));
        Assert.Throws<NotSupportedException>(() => dfe.Preparar(new NaoPreparadoFalso()));
        Assert.Throws<InvalidOperationException>(() => dfe.Preparar(new TextoFalso { MontagemNula = true }));
        Assert.Throws<ArgumentException>(() => Criar(d => d.RegistrarPreparo(new PreparoEcoFalso())));
        Assert.Throws<ArgumentNullException>(() => Criar(d => d.RegistrarPreparo<TextoFalso>(null!)));

        var leiaute = new LeiauteFalso();
        DeclaracaoLeiaute.Criar(leiaute, Documento.NFe);
        Assert.Throws<InvalidOperationException>(() => leiaute.Ultima!.RegistrarPreparo(new PreparoEcoFalso()));
    }

    [Fact]
    public void ODocumentoMontadoRecusaONulo()
    {
        var pedido = new DocumentoPedido("ECO1");

        Assert.Throws<ArgumentNullException>(() => new DocumentoMontado(null!, LeiauteFalso.Schema, pedido));
        Assert.Throws<ArgumentNullException>(() => new DocumentoMontado("", null!, pedido));
        Assert.Throws<ArgumentNullException>(() => new DocumentoMontado("", LeiauteFalso.Schema, null!));
        Assert.Throws<ArgumentNullException>(() => new DocumentoMontado("", LeiauteFalso.Schema, pedido) { Problemas = [null!] });
    }
}

// O schema de cada documento e o prefixo da propriedade (etapa 4.4, CONTRATOS.md 14.5, decisão 9): o eco validado
// sozinho antes do lote, e o problema de cada documento com o índice dele.
public class SchemaDocumentoTestes
{
    private static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = "11222333000181" }.Gerar();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (DFe Nucleo, TransporteFalso Transporte) Criar(Action<DeclaracaoLeiaute>? declarar = null, ValidacaoCallback? validacao = null)
    {
        var transporte = new TransporteFalso();
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = new ConexoesSefaz(transporte),
            Validacao = validacao,
        };
        return (DFe.Criar(new LeiauteFalso { Acrescimo = declarar }, Documento.NFe, configuracao), transporte);
    }

    [Fact]
    public async Task OSchemaDoDocumentoApontaOCaminhoDeleEOPrefixo()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new EcoFalso
        {
            Textos = ["ok", new string('x', 21)],
            MontarDocumentos = true,
            SchemaDocumentos = true,
            PrefixarDocumentos = true,
        }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal((LeiauteFalso.CodigoSchema, "eco/infEco/texto", "Ecos[1].Texto"), (problema.Codigo, problema.Caminho, problema.Propriedade));
        Assert.Empty(transporte.Envios);
    }

    [Fact]
    public async Task SemOSchemaDoDocumentoOCaminhoEODaMensagem()
    {
        var (dfe, _) = Criar();

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["ok", new string('x', 21)], MontarDocumentos = true }, Ct);

        Assert.Equal("ecoLote/eco[2]/infEco/texto", Assert.Single(resultado.Problemas).Caminho);
    }

    [Fact]
    public async Task ComOsDocumentosValidosAMensagemEValidadaETransmitida()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["um", "dois"], MontarDocumentos = true, SchemaDocumentos = true }, Ct);

        Assert.True(resultado.Processado);
        Assert.Single(transporte.Envios);
    }

    [Fact]
    public async Task OPrefixoVaiNasRegrasENosCallbacksMasNaoNoCampoVerificado()
    {
        var campos = new List<CampoVerificado>();
        var (dfe, _) = Criar(
            d => d.DeclararRegras((documento, _) =>
                [new Problema("R1", Severidade.Aviso, "Regra.") { Propriedade = "Texto" }, new Problema("R2", Severidade.Aviso, "Sem propriedade.")]),
            new ValidacaoCallback
            {
                Campo = c =>
                {
                    campos.Add(c);
                    return c.Tag == "texto" ? [new Problema("C1", Severidade.Aviso, "Do sistema.") { Propriedade = c.Propriedade }] : [];
                },
            });

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["um", "dois"], MontarDocumentos = true, PrefixarDocumentos = true }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(
            [("R1", "Ecos[0].Texto"), ("R2", null), ("R1", "Ecos[1].Texto"), ("R2", null), ("C1", "Ecos[0].Texto"), ("C1", "Ecos[1].Texto")],
            resultado.Problemas.Select(p => (p.Codigo, p.Propriedade)));
        Assert.All(campos.Where(c => c.Tag == "texto"), c => Assert.Equal("Texto", c.Propriedade));
    }
}
