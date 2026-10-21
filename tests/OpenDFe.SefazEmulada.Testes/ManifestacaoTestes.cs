using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// A manifestação do destinatário no NFeRecepcaoEvento4 do AN (NT 2020.001 v1.60; MOC 7.0, Visão Geral, 5.11): as
// regras da parte geral que valem no AN, as do evento (594, 596, 650, 651, 655, 496), o 221 do cancelamento da nota
// confirmada e a nota com o evento na consulta. A nota é de outra empresa (o OutroCertificado), para o destinatário
// ser o certificado dos testes.
public sealed class ManifestacaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private const string Emitente = "12345678000195";
    private const string Destinatario = "11222333000181";

    private readonly EmuladorFixture _fixture;

    public ManifestacaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => Limpar();

    private void Limpar()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Relogio.Avanco = TimeSpan.Zero;
    }

    // A filial do destinatário (a mesma raiz, o DV pelo módulo 11, como o do CNPJ).
    private static string Filial()
    {
        var parte = "112223330002";
        var primeiro = NotaTeste.Modulo11(parte);
        return parte + primeiro + NotaTeste.Modulo11(parte + primeiro);
    }

    private static NotaTeste Nota(string? destinatario = Destinatario) =>
        new() { Cnpj = Emitente, Certificado = Clientes.OutroCertificado, Destinatario = destinatario };

    // A nota autorizada em MG (100) e o protocolo.
    private async Task<(NotaTeste Nota, string Protocolo)> AutorizarAsync(NotaTeste? nota = null)
    {
        nota ??= Nota();
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal("100", protocolo.Campo("cStat"));
        return (nota, protocolo.Campo("nProt"));
    }

    private static EventoTeste Manifestacao(NotaTeste nota, string tipo = EventoTeste.Confirmacao, int sequencia = 1) =>
        new() { Chave = nota.Chave, Tipo = tipo, Sequencia = sequencia, Justificativa = tipo == EventoTeste.NaoRealizada ? "MERCADORIA RECUSADA NA ENTREGA" : null };

    // O cancelamento pelo emitente, em MG.
    private static EventoTeste Cancelamento(NotaTeste nota, string protocolo) =>
        new() { Chave = nota.Chave, Protocolo = protocolo, Cnpj = Emitente, Certificado = Clientes.OutroCertificado };

    private Task<XElement> EnviarAsync(string destino, params EventoTeste[] eventos) =>
        _fixture.Http.EnviarEventosAsync(_fixture.Url(destino, Servico.NomeRecepcaoEvento), eventos.Select(e => e.Gerar()), Ct);

    private static XElement Resultado(XElement retorno, int posicao = 0) =>
        retorno.Elements(Mensagens.NFe + "retEvento").ElementAt(posicao).Element(Mensagens.NFe + "infEvento")!;

    private async Task<string> CodigoAsync(EventoTeste evento, string destino = "AN") =>
        Resultado(await EnviarAsync(destino, evento)).Campo("cStat");

    private async Task<(string Codigo, string Motivo)> MotivoAsync(EventoTeste evento)
    {
        var resultado = Resultado(await EnviarAsync("AN", evento));
        return (resultado.Campo("cStat"), resultado.Campo("xMotivo"));
    }

    // Registrada

    [Fact]
    public async Task RegistraAConfirmacaoNoAmbienteNacional()
    {
        var (nota, _) = await AutorizarAsync();

        var retorno = await EnviarAsync("AN", Manifestacao(nota));

        Assert.Equal(("7", "AN-EMULADA", "91", "128"), (retorno.Campo("idLote"), retorno.Campo("verAplic"), retorno.Campo("cOrgao"), retorno.Campo("cStat")));
        var resultado = Resultado(retorno);
        Assert.Equal(["tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "chNFe", "tpEvento", "xEvento", "nSeqEvento", "dhRegEvento", "nProt"],
            resultado.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("91", "135", "Evento registrado e vinculado a NF-e", nota.Chave, "210200", "Confirmacao de Operacao registrada", "1"),
            (resultado.Campo("cOrgao"), resultado.Campo("cStat"), resultado.Campo("xMotivo"), resultado.Campo("chNFe"), resultado.Campo("tpEvento"),
                resultado.Campo("xEvento"), resultado.Campo("nSeqEvento")));
        Assert.Matches("^291" + NotaTeste.Agora().ToString("yy", CultureInfo.InvariantCulture) + "[0-9]{10}$", resultado.Campo("nProt"));
        Assert.Empty(Esquemas.Validar("retEnvEvento_v1.00.xsd", retorno));
        Assert.False(Assert.Single(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave).Cancelada);
    }

    [Theory]
    [InlineData(EventoTeste.Confirmacao, "Confirmacao de Operacao registrada")]
    [InlineData(EventoTeste.Ciencia, "Ciencia da Operacao registrada")]
    [InlineData(EventoTeste.Desconhecimento, "Desconhecimento da Operacao registrada")]
    [InlineData(EventoTeste.NaoRealizada, "Operacao nao Realizada registrada")]
    public async Task RegistraCadaManifestacaoComADescricao(string tipo, string descricao)
    {
        var (nota, _) = await AutorizarAsync();

        var resultado = Resultado(await EnviarAsync("AN", Manifestacao(nota, tipo)));

        Assert.Equal(("135", tipo, descricao), (resultado.Campo("cStat"), resultado.Campo("tpEvento"), resultado.Campo("xEvento")));
    }

    [Fact]
    public async Task ODesconhecimentoAceitaAJustificativa()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.Desconhecimento) with { Justificativa = "NAO RECONHECEMOS ESTA OPERACAO" }));
    }

    [Fact]
    public async Task AConsultaDaNotaTrazAManifestacao()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var evento = Manifestacao(nota, EventoTeste.Ciencia);
        Assert.Equal("135", await CodigoAsync(evento));

        var retorno = await _fixture.Http.ConsultarProtocoloAsync(_fixture.Url("MG", Servico.NomeConsultaProtocolo), nota.Chave, Ct);

        Assert.Equal("100", retorno.Campo("cStat"));
        Assert.Equal(protocolo, retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt"));
        var processado = Assert.Single(retorno.Elements(Mensagens.NFe + "procEventoNFe"));
        Assert.Equal(XElement.Parse(evento.Gerar()).ToString(), processado.Element(Mensagens.NFe + "evento")!.ToString());
        Assert.Empty(Esquemas.Validar("retConsSitNFe_v4.00.xsd", retorno));
    }

    [Fact]
    public async Task AFilialManifestaComOCertificadoDaMatriz()
    {
        var filial = Filial();
        var (nota, _) = await AutorizarAsync(Nota(filial));

        Assert.Equal("135", await CodigoAsync(Manifestacao(nota) with { Cnpj = filial }));
    }

    [Fact]
    public async Task ODestinatarioPessoaFisicaManifestaComOCpf()
    {
        var certificado = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(
            new CertificadoTeste { Cpf = "52998224725", Nome = "PESSOA DE TESTE" }.GerarPfx("teste"), "teste");
        var (nota, _) = await AutorizarAsync(Nota("52998224725"));

        Assert.Equal("135", await CodigoAsync(Manifestacao(nota) with { Cnpj = null, Cpf = "52998224725", Certificado = certificado }));
    }

    [Fact]
    public async Task ACienciaEDepoisAConfirmacao()
    {
        var (nota, _) = await AutorizarAsync();

        var retorno = await EnviarAsync("AN", Manifestacao(nota, EventoTeste.Ciencia), Manifestacao(nota));

        Assert.Equal(["135", "135"], retorno.Elements(Mensagens.NFe + "retEvento").Select(e => e.Element(Mensagens.NFe + "infEvento")!.Campo("cStat")));
    }

    // As regras da parte geral, no AN

    [Fact]
    public async Task OAnSoRecebeAManifestacaoEAUfNaoARecebe()
    {
        var (nota, protocolo) = await AutorizarAsync();

        Assert.Equal("491", await CodigoAsync(Cancelamento(nota, protocolo)));
        Assert.Equal("491", await CodigoAsync(Manifestacao(nota) with { Orgao = "31" }, "MG"));
        Assert.Equal("491", await CodigoAsync(Manifestacao(nota), "SVRS"));
    }

    [Fact]
    public async Task RecusaOOrgaoQueNaoEOAmbienteNacional()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("250", await CodigoAsync(Manifestacao(nota) with { Orgao = "31" }));
    }

    [Fact]
    public async Task RecusaASchemaDoEventoEAOperacaoNaoRealizadaSemJustificativa()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("493", await CodigoAsync(Manifestacao(nota, EventoTeste.NaoRealizada) with { Justificativa = null }));
        Assert.Equal("493", await CodigoAsync(Manifestacao(nota) with
        {
            Detalhe = "<detEvento versao=\"1.00\"><descEvento>Ciencia da Operacao</descEvento></detEvento>",
        }));
    }

    [Fact]
    public async Task RecusaAAssinaturaDeOutraEmpresa()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("213", await CodigoAsync(Manifestacao(nota) with { Certificado = Clientes.OutroCertificado }));
    }

    [Fact]
    public async Task ODuplicadoE573()
    {
        var (nota, _) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota)));

        Assert.Equal("573", await CodigoAsync(Manifestacao(nota)));
    }

    // As regras do evento

    [Fact]
    public async Task ANotaQueNaoEstaNaBaseE494()
    {
        Assert.Equal("494", await CodigoAsync(Manifestacao(Nota())));
    }

    [Fact]
    public async Task OAutorQueNaoEODestinatarioE575()
    {
        var (outro, _) = await AutorizarAsync(Nota("98765432000198"));
        var (semDestinatario, _) = await AutorizarAsync(Nota(null));

        Assert.Equal(("575", "Rejeição: O autor do evento diverge do destinatário da NF-e"), await MotivoAsync(Manifestacao(outro)));
        Assert.Equal("575", await CodigoAsync(Manifestacao(semDestinatario)));
    }

    [Fact]
    public async Task ADataDoEventoAntesDaEmissaoE577EAntesDaAutorizacaoE579()
    {
        var (nota, _) = await AutorizarAsync(Nota() with { Emissao = NotaTeste.Agora().AddHours(-2) });

        Assert.Equal("577", await CodigoAsync(Manifestacao(nota) with { Data = nota.Emissao.AddMinutes(-1) }));
        Assert.Equal("579", await CodigoAsync(Manifestacao(nota) with { Data = nota.Emissao.AddMinutes(30) }));
    }

    [Fact]
    public async Task ACienciaSoTemASequencia1EAsConclusivasAte2()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal(("594", "Rejeição: O número de sequencia do evento informado é maior que o permitido"),
            await MotivoAsync(Manifestacao(nota, EventoTeste.Ciencia, 2)));
        Assert.Equal("594", await CodigoAsync(Manifestacao(nota, EventoTeste.Confirmacao, 3)));
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.Confirmacao)));
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.Confirmacao, 2)));
    }

    [Fact]
    public async Task OPrazoDaCienciaEDe10DiasEODasConclusivasDe90()
    {
        var (nota, _) = await AutorizarAsync();

        _fixture.Relogio.Avanco = TimeSpan.FromDays(11);
        Assert.Equal(("596", "Rejeição: Evento apresentado fora do prazo: 10 dias"), await MotivoAsync(Manifestacao(nota, EventoTeste.Ciencia)));
        _fixture.Relogio.Avanco = TimeSpan.FromDays(89);
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.Desconhecimento)));
        _fixture.Relogio.Avanco = TimeSpan.FromDays(91);
        Assert.Equal(("596", "Rejeição: Evento apresentado fora do prazo: 90 dias"), await MotivoAsync(Manifestacao(nota, EventoTeste.Confirmacao)));
    }

    [Fact]
    public async Task ACienciaEODesconhecimentoDaNotaCanceladaSaoRecusados()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Cancelamento(nota, protocolo), "MG"));

        Assert.Equal(("650", "Rejeição: Evento de \"Ciência da Emissão\" para NF-e Cancelada ou Denegada"),
            await MotivoAsync(Manifestacao(nota, EventoTeste.Ciencia)));
        Assert.Equal(("651", "Rejeição: Evento de \"Desconhecimento da Operação\" para NF-e Cancelada ou Denegada"),
            await MotivoAsync(Manifestacao(nota, EventoTeste.Desconhecimento)));
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.NaoRealizada)));
    }

    [Theory]
    [InlineData(EventoTeste.Confirmacao)]
    [InlineData(EventoTeste.Desconhecimento)]
    [InlineData(EventoTeste.NaoRealizada)]
    public async Task ACienciaDepoisDaManifestacaoConclusivaE655(string conclusiva)
    {
        var (nota, _) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, conclusiva)));

        Assert.Equal(("655", "Rejeição: Evento de Ciência da Emissão informado após a manifestação final do destinatário"),
            await MotivoAsync(Manifestacao(nota, EventoTeste.Ciencia)));
    }

    [Fact]
    public async Task AChaveDaEmissaoOfflineE496()
    {
        var (nota, _) = await AutorizarAsync(Nota() with { TipoEmissao = "9", TipoImpressao = "6" });

        Assert.Equal("496", await CodigoAsync(Manifestacao(nota)));
    }

    [Fact]
    public async Task AParalisacaoDoAnRespondeNoLote()
    {
        var (nota, _) = await AutorizarAsync();
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "AN", Servico.NomeRecepcaoEvento, Vezes: 1));

        var retorno = await EnviarAsync("AN", Manifestacao(nota));

        Assert.Equal(("108", "91"), (retorno.Campo("cStat"), retorno.Campo("cOrgao")));
        Assert.Empty(retorno.Elements(Mensagens.NFe + "retEvento"));
    }

    // O cancelamento da nota confirmada (4P15-14)

    [Fact]
    public async Task ANotaConfirmadaNaoECancelada()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota)));

        var resultado = Resultado(await EnviarAsync("MG", Cancelamento(nota, protocolo)));

        Assert.Equal(("221", "Rejeição: Confirmado o recebimento da NF-e pelo destinatário"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Theory]
    [InlineData(EventoTeste.Desconhecimento)]
    [InlineData(EventoTeste.NaoRealizada)]
    public async Task AConfirmacaoDesfeitaDeixaCancelar(string depois)
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota)));
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, depois)));

        Assert.Equal("135", await CodigoAsync(Cancelamento(nota, protocolo), "MG"));
    }

    [Fact]
    public async Task ACienciaNaoImpedeOCancelamento()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(Manifestacao(nota, EventoTeste.Ciencia)));

        Assert.Equal("135", await CodigoAsync(Cancelamento(nota, protocolo), "MG"));
    }
}
