using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeRecepcaoEvento4 (MOC 7.0, Visão Geral, 5.8 e 5.9): o lote de eventos, as regras da parte geral (Tabela
// 5-35) e o cancelamento (Tabela 5-38), com a nota cancelada na consulta (101) e na duplicidade (218).
public sealed class EventoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public EventoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => Limpar();

    private void Limpar()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Irregulares.Limpar();
        _fixture.Relogio.Avanco = TimeSpan.Zero;
    }

    // A nota autorizada (100) e o protocolo.
    private async Task<(NotaTeste Nota, string Protocolo)> AutorizarAsync(NotaTeste? nota = null, string destino = "MG")
    {
        nota ??= new NotaTeste();
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url(destino, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal("100", protocolo.Campo("cStat"));
        return (nota, protocolo.Campo("nProt"));
    }

    private Task<XElement> EnviarAsync(string destino, params EventoTeste[] eventos) =>
        _fixture.Http.EnviarEventosAsync(_fixture.Url(destino, Servico.NomeRecepcaoEvento), eventos.Select(e => e.Gerar()), Ct);

    // O lote escrito à mão (o envEvento ou outra raiz).
    private async Task<XElement> EnviarAsync(string destino, string lote)
    {
        using var resposta = await _fixture.Http.EnviarAsync(_fixture.Url(destino, Servico.NomeRecepcaoEvento), Mensagens.Envelope(lote, Servico.NomeRecepcaoEvento), Ct);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));
    }

    // O infEvento do retEvento.
    private static XElement Resultado(XElement retorno, int posicao = 0) =>
        retorno.Elements(Mensagens.NFe + "retEvento").ElementAt(posicao).Element(Mensagens.NFe + "infEvento")!;

    private async Task<string> CodigoAsync(EventoTeste evento, string destino = "MG") =>
        Resultado(await EnviarAsync(destino, evento)).Campo("cStat");

    private Task<XElement> ConsultarAsync(string chave, string destino = "MG") =>
        _fixture.Http.ConsultarProtocoloAsync(_fixture.Url(destino, Servico.NomeConsultaProtocolo), chave, Ct);

    // Cancelamento registrado

    [Fact]
    public async Task CancelaANotaAutorizada()
    {
        var (nota, protocolo) = await AutorizarAsync(new NotaTeste { Destinatario = "98765432000198" });
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await EnviarAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = protocolo });

        Assert.Equal(Mensagens.NFe + "retEnvEvento", retorno.Name);
        Assert.Equal("1.00", retorno.Attribute("versao")?.Value);
        Assert.Equal(["idLote", "tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "retEvento"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("7", "2", "MG-EMULADA", "31", "128", "Lote de Evento Processado"),
            (retorno.Campo("idLote"), retorno.Campo("tpAmb"), retorno.Campo("verAplic"), retorno.Campo("cOrgao"), retorno.Campo("cStat"), retorno.Campo("xMotivo")));
        var resultado = Resultado(retorno);
        Assert.Equal("1.00", resultado.Parent!.Attribute("versao")?.Value);
        Assert.Null(resultado.Attribute("Id"));
        Assert.Equal(["tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "chNFe", "tpEvento", "xEvento", "nSeqEvento", "CNPJDest", "dhRegEvento", "nProt"],
            resultado.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("31", "135", "Evento registrado e vinculado a NF-e", nota.Chave, "110111", "Cancelamento homologado", "1", "98765432000198"),
            (resultado.Campo("cOrgao"), resultado.Campo("cStat"), resultado.Campo("xMotivo"), resultado.Campo("chNFe"), resultado.Campo("tpEvento"),
                resultado.Campo("xEvento"), resultado.Campo("nSeqEvento"), resultado.Campo("CNPJDest")));
        Assert.Matches("^131" + NotaTeste.Agora().ToString("yy", CultureInfo.InvariantCulture) + "[0-9]{10}$", resultado.Campo("nProt"));
        var registro = DateTimeOffset.ParseExact(resultado.Campo("dhRegEvento"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.InRange(registro, antes, DateTimeOffset.UtcNow.AddSeconds(1));
        Assert.Empty(Esquemas.Validar("retEnvEvento_v1.00.xsd", retorno));
        Assert.True(Assert.Single(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave).Cancelada);
    }

    [Fact]
    public async Task AConsultaDaNotaCanceladaResponde101ComOEvento()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var evento = new EventoTeste { Chave = nota.Chave, Protocolo = protocolo };
        var registrado = Resultado(await EnviarAsync("MG", evento));

        var retorno = await ConsultarAsync(nota.Chave);

        Assert.Equal(("101", "Cancelamento de NF-e homologado"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto", "chNFe", "protNFe", "procEventoNFe"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(protocolo, retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt"));
        var processado = retorno.Element(Mensagens.NFe + "procEventoNFe")!;
        Assert.Equal("1.00", processado.Attribute("versao")?.Value);
        Assert.Equal(["evento", "retEvento"], processado.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(XElement.Parse(evento.Gerar()).ToString(), processado.Element(Mensagens.NFe + "evento")!.ToString());
        Assert.Equal(registrado.Campo("nProt"), processado.Element(Mensagens.NFe + "retEvento")!.Element(Mensagens.NFe + "infEvento")!.Campo("nProt"));
        Assert.Empty(Esquemas.Validar("retConsSitNFe_v4.00.xsd", retorno));
        Assert.Empty(Esquemas.Validar("procEventoNFe_v1.00.xsd", processado));
    }

    [Fact]
    public async Task ANotaCanceladaReenviadaE218EOCancelamentoRepetidoE573()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var evento = new EventoTeste { Chave = nota.Chave, Protocolo = protocolo };
        Assert.Equal("135", await CodigoAsync(evento));

        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var repetido = Resultado(await EnviarAsync("MG", evento));

        Assert.Equal(("218", "Rejeição: NF-e já está cancelada na base de dados da SEFAZ"),
            (retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat"), retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("xMotivo")));
        Assert.Equal(("573", "Rejeição: Duplicidade de Evento"), (repetido.Campo("cStat"), repetido.Campo("xMotivo")));
        Assert.Equal(["tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "chNFe", "tpEvento", "nSeqEvento", "dhRegEvento"], repetido.Elements().Select(e => e.Name.LocalName));
        Assert.Single(_fixture.Emulador.Repositorio.Eventos(nota.Chave));
    }

    [Fact]
    public async Task ANotaComOutraChaveNaMesmaNumeracaoContinua539()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo }));

        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [(nota with { Codigo = "56473829" }).Gerar()], Ct);

        Assert.Equal("539", retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat"));
    }

    [Fact]
    public async Task ProcessaOsEventosDoLoteNaOrdem()
    {
        var (primeira, protocolo) = await AutorizarAsync();
        var (segunda, _) = await AutorizarAsync(new NotaTeste { Destinatario = "52998224725" });

        var retorno = await EnviarAsync("MG",
            new EventoTeste { Chave = primeira.Chave, Protocolo = protocolo },
            new EventoTeste { Chave = segunda.Chave, Protocolo = "131000000000000" });

        Assert.Equal("128", retorno.Campo("cStat"));
        Assert.Equal(2, retorno.Elements(Mensagens.NFe + "retEvento").Count());
        Assert.Equal(("135", primeira.Chave), (Resultado(retorno).Campo("cStat"), Resultado(retorno).Campo("chNFe")));
        Assert.Equal(("222", segunda.Chave), (Resultado(retorno, 1).Campo("cStat"), Resultado(retorno, 1).Campo("chNFe")));
        Assert.Empty(Esquemas.Validar("retEnvEvento_v1.00.xsd", retorno));
    }

    [Fact]
    public async Task OCancelamentoLevaOCpfDoDestinatarioESemDestinatarioNada()
    {
        var (comCpf, protocoloCpf) = await AutorizarAsync(new NotaTeste { Destinatario = "52998224725" });
        var (sem, protocoloSem) = await AutorizarAsync();

        var retorno = await EnviarAsync("MG", new EventoTeste { Chave = comCpf.Chave, Protocolo = protocoloCpf }, new EventoTeste { Chave = sem.Chave, Protocolo = protocoloSem });

        Assert.Equal("52998224725", Resultado(retorno).Campo("CPFDest"));
        Assert.DoesNotContain(Resultado(retorno, 1).Elements(), e => e.Name.LocalName.EndsWith("Dest", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AFilialCancelaComOCertificadoDaMatriz()
    {
        var (nota, protocolo) = await AutorizarAsync(new NotaTeste { Cnpj = "11222333000262" });

        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Cnpj = "11222333000262" }));
    }

    [Fact]
    public async Task OEmitenteCpfCancelaANfce()
    {
        // A NFC-e do produtor rural (NT 2023.002): o autor CPF no modelo 65 (o 408 do MOC não se aplica).
        var certificado = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(
            new CertificadoTeste { Cpf = "52998224725", Nome = "PRODUTOR DE TESTE" }.GerarPfx("teste"), "teste");
        var (nota, protocolo) = await AutorizarAsync(new NotaTeste { Cnpj = null, Cpf = "52998224725", Modelo = "65", Serie = "920", Certificado = certificado });

        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Cnpj = null, Cpf = "52998224725", Certificado = certificado }));
    }

    // As regras do lote

    [Fact]
    public async Task RecusaARaizEOSchemaDoLote()
    {
        var evento = new EventoTeste { Chave = new NotaTeste().Chave }.Gerar();

        var raiz = await EnviarAsync("MG", Mensagens.Status());
        var versao = await EnviarAsync("MG", Mensagens.Eventos(evento).Replace("<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"", "<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"", StringComparison.Ordinal));
        var vinteEUm = await EnviarAsync("MG", Mensagens.Eventos([.. Enumerable.Range(0, 21).Select(i => new EventoTeste { Chave = new NotaTeste().Chave }.Gerar())]));

        Assert.Equal(("516", "0", "31"), (raiz.Campo("cStat"), raiz.Campo("idLote"), raiz.Campo("cOrgao")));
        Assert.Equal("239", versao.Campo("cStat"));
        Assert.Equal("215", vinteEUm.Campo("cStat"));
        Assert.Empty(raiz.Elements(Mensagens.NFe + "retEvento"));
        Assert.Empty(Esquemas.Validar("retEnvEvento_v1.00.xsd", raiz));
    }

    [Fact]
    public async Task RespondeAParalisacaoDoLote()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Servico.NomeRecepcaoEvento));

        var retorno = await EnviarAsync("MG", new EventoTeste { Chave = new NotaTeste().Chave });

        Assert.Equal(("108", "31"), (retorno.Campo("cStat"), retorno.Campo("cOrgao")));
        Assert.Empty(retorno.Elements(Mensagens.NFe + "retEvento"));
    }

    // As regras de cada evento (Anexo I, D04 a D06; Tabela 5-35)

    [Fact]
    public async Task RecusaOTipoEAVersaoDoEventoQueNaoAtende()
    {
        var chave = new NotaTeste().Chave;

        var tipo = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = chave, Tipo = "210200", Detalhe = "<detEvento versao=\"1.00\"><descEvento>Confirmacao da Operacao</descEvento></detEvento>" }));
        var versao = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = chave, VersaoEvento = "2.00" }));

        Assert.Equal(("491", "Rejeição: O tpEvento informado inválido"), (tipo.Campo("cStat"), tipo.Campo("xMotivo")));
        Assert.Equal(("492", "Rejeição: O verEvento informado inválido"), (versao.Campo("cStat"), versao.Campo("xMotivo")));
    }

    [Theory]
    [InlineData("<detEvento versao=\"1.00\"><descEvento>Cancelamento</descEvento><nProt>131260000000001</nProt></detEvento>")]
    [InlineData("<detEvento versao=\"1.00\"><descEvento>Cancelar</descEvento><nProt>131260000000001</nProt><xJust>CANCELAMENTO DE TESTE</xJust></detEvento>")]
    [InlineData("<detEvento versao=\"1.01\"><descEvento>Cancelamento</descEvento><nProt>131260000000001</nProt><xJust>CANCELAMENTO DE TESTE</xJust></detEvento>")]
    [InlineData("<detEvento versao=\"1.00\"><descEvento>Cancelamento</descEvento><nProt>131260000000001</nProt><xJust>CURTA</xJust></detEvento>")]
    public async Task RecusaODetalheForaDoSchemaDoTipo(string detalhe)
    {
        var resultado = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = new NotaTeste().Chave, Detalhe = detalhe }));

        Assert.Equal(("493", "Rejeição: Evento não atende o Schema XML específico"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaODocumentoDoAutorInvalido()
    {
        var chave = new NotaTeste().Chave;

        Assert.Equal("489", await CodigoAsync(new EventoTeste { Chave = chave, Cnpj = "11222333000180" }));
        Assert.Equal("490", await CodigoAsync(new EventoTeste { Chave = chave, Cnpj = null, Cpf = "52998224726" }));
    }

    [Fact]
    public async Task ConfereAAssinaturaEOTitularContraOAutor()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var evento = new EventoTeste { Chave = nota.Chave, Protocolo = protocolo };
        var alterado = evento.Gerar().Replace("CANCELAMENTO DE TESTE DO EMULADOR", "CANCELAMENTO ALTERADO NO CAMINHO", StringComparison.Ordinal);

        var assinatura = Resultado(await EnviarAsync("MG", Mensagens.Eventos(alterado)));

        Assert.Equal("297", assinatura.Campo("cStat"));
        Assert.Equal("213", await CodigoAsync(evento with { Certificado = Clientes.OutroCertificado }));
    }

    [Fact]
    public async Task RecusaOIdOOrgaoEOAmbiente()
    {
        var chave = new NotaTeste().Chave;

        Assert.Equal("572", await CodigoAsync(new EventoTeste { Chave = chave, Id = "ID110111" + chave + "02" }));
        Assert.Equal("250", await CodigoAsync(new EventoTeste { Chave = chave, Orgao = "35" }));
        Assert.Equal("250", await CodigoAsync(new EventoTeste { Chave = chave, Orgao = "91" }));
        Assert.Equal("250", await CodigoAsync(new EventoTeste { Chave = new NotaTeste { Uf = "42" }.Chave }));
        Assert.Equal("252", await CodigoAsync(new EventoTeste { Chave = chave, Ambiente = "1" }));
    }

    [Fact]
    public async Task RecusaAChaveInvalida()
    {
        static string Completar(string parte) => parte + NotaTeste.Modulo11(parte);
        var chave = new NotaTeste().Chave;
        var digito = chave[..43] + ((chave[43] - '0' + 1) % 10).ToString(CultureInfo.InvariantCulture);

        Assert.Equal("236", await CodigoAsync(new EventoTeste { Chave = digito }));
        Assert.Equal("615", await CodigoAsync(new EventoTeste { Chave = Completar("3105" + chave[4..43]) }));
        Assert.Equal("616", await CodigoAsync(new EventoTeste { Chave = Completar(chave[..4] + "13" + chave[6..43]) }));
        Assert.Equal("617", await CodigoAsync(new EventoTeste { Chave = Completar(chave[..6] + "11222333000180" + chave[20..43]) }));
        Assert.Equal("618", await CodigoAsync(new EventoTeste { Chave = Completar(chave[..20] + "57" + chave[22..43]) }));
        Assert.Equal("619", await CodigoAsync(new EventoTeste { Chave = Completar(chave[..25] + "000000000" + chave[34..43]) }));
    }

    [Fact]
    public async Task RecusaOAutorQueNaoEOEmitenteDaChave()
    {
        // A filial, com o certificado da matriz (o 213 passa): o CNPJ inteiro do autor difere do da chave.
        var resultado = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = new NotaTeste().Chave, Cnpj = "11222333000262" }));

        Assert.Equal(("574", "Rejeição: O autor do evento diverge do emissor da NF-e"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaOEventoNoFuturoEOAutorIrregular()
    {
        var (nota, protocolo) = await AutorizarAsync();

        Assert.Equal("578", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Data = NotaTeste.Agora().AddMinutes(10) }));
        // A tolerância de 5 minutos para o relógio do autor.
        var (outra, protocoloOutra) = await AutorizarAsync();
        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = outra.Chave, Protocolo = protocoloOutra, Data = NotaTeste.Agora().AddMinutes(4) }));
        _fixture.Emulador.Irregulares.Cadastrar("11222333000181");
        var irregular = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = protocolo }));
        Assert.Equal(("240", "Rejeição: Irregularidade Fiscal do Emitente"), (irregular.Campo("cStat"), irregular.Campo("xMotivo")));
    }

    // As regras do cancelamento (Tabela 5-38)

    [Fact]
    public async Task RecusaASequenciaDiferenteDeUm()
    {
        var (nota, protocolo) = await AutorizarAsync();

        Assert.Equal("594", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Sequencia = 2 }));
    }

    [Fact]
    public async Task RecusaANotaQueNaoEstaNaBase()
    {
        var nota = new NotaTeste();
        var (autorizada, protocolo) = await AutorizarAsync(nota with { Codigo = "56473829" });

        var inexistente = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = new NotaTeste().Chave }));
        var outraChave = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = protocolo }));

        Assert.Equal(("494", "Rejeição: Chave de Acesso inexistente"), (inexistente.Campo("cStat"), inexistente.Campo("xMotivo")));
        Assert.Equal(("494", $"Rejeição: Chave de Acesso inexistente [chNFe:{autorizada.Chave}]"), (outraChave.Campo("cStat"), outraChave.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaOCancelamentoDepoisDe24Horas()
    {
        var (nota, protocolo) = await AutorizarAsync();
        _fixture.Relogio.Avanco = TimeSpan.FromHours(24) + TimeSpan.FromMinutes(1);

        var resultado = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Data = NotaTeste.Agora() + _fixture.Relogio.Avanco }));

        Assert.Equal(("501", "Rejeição: Prazo de cancelamento superior ao previsto na Legislação"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Fact]
    public async Task CancelaAte24Horas()
    {
        var (nota, protocolo) = await AutorizarAsync();
        _fixture.Relogio.Avanco = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59);

        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Data = NotaTeste.Agora() + _fixture.Relogio.Avanco }));
    }

    [Fact]
    public async Task RecusaAsDatasAntesDaEmissaoEDaAutorizacao()
    {
        var (emitida, protocoloEmitida) = await AutorizarAsync();
        var (antiga, protocoloAntiga) = await AutorizarAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddHours(-1) });

        Assert.Equal("577", await CodigoAsync(new EventoTeste { Chave = emitida.Chave, Protocolo = protocoloEmitida, Data = NotaTeste.Agora().AddMinutes(-3) }));
        Assert.Equal("579", await CodigoAsync(new EventoTeste { Chave = antiga.Chave, Protocolo = protocoloAntiga, Data = NotaTeste.Agora().AddMinutes(-30) }));
        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = antiga.Chave, Protocolo = protocoloAntiga, Data = NotaTeste.Agora().AddMinutes(-4) }));
    }

    [Fact]
    public async Task RecusaOProtocoloQueNaoEODaNota()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var outro = protocolo[..^1] + (protocolo[^1] == '9' ? '8' : '9');

        var resultado = Resultado(await EnviarAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = outro }));

        Assert.Equal(("222", "Rejeição: Protocolo de Autorização de Uso difere do cadastrado"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    // A contingência SVC (Anexo III, 2.1.3, c e d)

    [Fact]
    public async Task ASvcCancelaSoAsNotasQueAutorizou()
    {
        var (svc, protocoloSvc) = await AutorizarAsync(new NotaTeste { TipoEmissao = "6" }, "SVC-AN");
        var (normal, protocoloNormal) = await AutorizarAsync();

        Assert.Equal("494", await CodigoAsync(new EventoTeste { Chave = normal.Chave, Protocolo = protocoloNormal }, "SVC-AN"));
        Assert.Equal("491", await CodigoAsync(new EventoTeste { Chave = svc.Chave, Tipo = EventoTeste.CartaCorrecao }, "SVC-AN"));
        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = svc.Chave, Protocolo = protocoloSvc }, "SVC-AN"));
        Assert.Equal("101", (await ConsultarAsync(svc.Chave, "SVC-AN")).Campo("cStat"));
    }

    [Fact]
    public async Task OAutorizadorNormalCancelaANotaDaSvc()
    {
        var (svc, protocolo) = await AutorizarAsync(new NotaTeste { TipoEmissao = "6" }, "SVC-AN");

        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = svc.Chave, Protocolo = protocolo }));
    }
}
