using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O cancelamento por substituição da NFC-e (110112; MOC 7.0, Visão Geral, 5.9, Tabela 5-38): a NFC-e online
// cancelada pela offline que a substitui, a chave da substituta e a substituta contra a cancelada (912 a 921).
public sealed class SubstituicaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public SubstituicaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Relogio.Avanco = TimeSpan.Zero;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Relogio.Avanco = TimeSpan.Zero;

    // A NFC-e autorizada (100) e o protocolo.
    private async Task<string> AutorizarAsync(NotaTeste nota)
    {
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal("100", protocolo.Campo("cStat"));
        return protocolo.Campo("nProt");
    }

    // A NFC-e online autorizada e a offline que a substitui, autorizada também, com o que o teste trocar nela.
    private async Task<(NotaTeste Online, string Protocolo, NotaTeste Offline)> PrepararAsync(Func<NotaTeste, NotaTeste>? trocar = null)
    {
        var online = new NotaTeste { Modelo = "65", Destinatario = "52998224725" };
        var protocolo = await AutorizarAsync(online);
        var offline = new NotaTeste { Modelo = "65", TipoEmissao = "9", Destinatario = "52998224725" };
        offline = trocar?.Invoke(offline) ?? offline;
        await AutorizarAsync(offline);
        return (online, protocolo, offline);
    }

    private static EventoTeste Evento(NotaTeste online, string protocolo, string referencia) =>
        new() { Chave = online.Chave, Tipo = EventoTeste.Substituicao, Protocolo = protocolo, Referencia = referencia };

    private async Task<XElement> EnviarAsync(EventoTeste evento) =>
        (await _fixture.Http.EnviarEventosAsync(_fixture.Url("MG", Servico.NomeRecepcaoEvento), [evento.Gerar()], Ct))
            .Element(Mensagens.NFe + "retEvento")!.Element(Mensagens.NFe + "infEvento")!;

    private async Task<string> CodigoAsync(EventoTeste evento) => (await EnviarAsync(evento)).Campo("cStat");

    private static string Completar(string parte) => parte + NotaTeste.Modulo11(parte);

    [Fact]
    public async Task CancelaANfceOnlinePelaOfflineQueASubstitui()
    {
        var (online, protocolo, offline) = await PrepararAsync();

        var resultado = await EnviarAsync(Evento(online, protocolo, offline.Chave));

        Assert.Equal(["tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "chNFe", "tpEvento", "xEvento", "nSeqEvento", "cOrgaoAutor", "dhRegEvento", "nProt"],
            resultado.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("135", "110112", "Cancelamento homologado", "31"), (resultado.Campo("cStat"), resultado.Campo("tpEvento"), resultado.Campo("xEvento"), resultado.Campo("cOrgaoAutor")));
        var consulta = await _fixture.Http.ConsultarProtocoloAsync(_fixture.Url("MG", Servico.NomeConsultaProtocolo), online.Chave, Ct);
        Assert.Equal("101", consulta.Campo("cStat"));
        // O procEventoNFe vai como foi registrado, com o cOrgaoAutor (R22), que o schema genérico prevê; o
        // retConsSitNFe do PL_010d não o prevê no retEvento (docs/SEFAZ_EMULADA.md).
        var processado = consulta.Element(Mensagens.NFe + "procEventoNFe")!;
        Assert.Empty(Esquemas.Validar("procEventoNFe_v1.00.xsd", processado));
        Assert.Equal("cOrgaoAutor", Assert.Single(Esquemas.Validar("retConsSitNFe_v4.00.xsd", consulta)).Tag);
        Assert.Equal("100", (await _fixture.Http.ConsultarProtocoloAsync(_fixture.Url("MG", Servico.NomeConsultaProtocolo), offline.Chave, Ct)).Campo("cStat"));
    }

    [Fact]
    public async Task RecusaANotaCanceladaQueNaoFoiEmitidaOnline()
    {
        var (_, _, offline) = await PrepararAsync();

        var resultado = await EnviarAsync(Evento(offline, "131000000000000", new NotaTeste { Modelo = "65" }.Chave));

        Assert.Equal(("920", "Rejeição: Tipo de Emissão inválido no Cancelamento por Substituição"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaOOrgaoDoAutorDeOutraUf()
    {
        var (online, protocolo, offline) = await PrepararAsync();

        Assert.Equal("455", await CodigoAsync(Evento(online, protocolo, offline.Chave) with { OrgaoAutor = "35" }));
    }

    [Fact]
    public async Task RecusaAChaveDaSubstitutaInvalida()
    {
        var (online, protocolo, offline) = await PrepararAsync();
        var chave = offline.Chave;
        var digito = chave[..43] + ((chave[43] - '0' + 1) % 10).ToString(CultureInfo.InvariantCulture);

        async Task Conferir(string referencia, string campo)
        {
            var resultado = await EnviarAsync(Evento(online, protocolo, referencia));
            Assert.Equal(("910", $"Rejeição: Chave de Acesso NF-e Substituta inválida ({campo})"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
        }

        await Conferir(digito, "Dígito");
        await Conferir(Completar("99" + chave[2..43]), "Código UF");
        await Conferir(Completar(chave[..2] + "05" + chave[4..43]), "Ano");
        await Conferir(Completar(chave[..4] + "13" + chave[6..43]), "Mês");
        await Conferir(Completar(chave[..6] + "11222333000180" + chave[20..43]), "CNPJ/CPF");
        await Conferir(Completar(chave[..20] + "57" + chave[22..43]), "Modelo");
        await Conferir(Completar(chave[..25] + "000000000" + chave[34..43]), "Número");
    }

    [Fact]
    public async Task RecusaAChaveDaSubstitutaIncorretaParaACancelada()
    {
        var (online, protocolo, offline) = await PrepararAsync();
        var chave = offline.Chave;
        var mes = online.Emissao.AddMonths(-2).ToString("yyMM", CultureInfo.InvariantCulture);

        async Task Conferir(string referencia, string campo)
        {
            var resultado = await EnviarAsync(Evento(online, protocolo, referencia));
            Assert.Equal(("911", $"Rejeição: Chave de Acesso NF-e Substituta incorreta ({campo})"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
        }

        await Conferir(online.Chave, "mesma Chave de Acesso");
        await Conferir(Completar("42" + chave[2..43]), "Código da UF");
        await Conferir(Completar(chave[..6] + "11222333000262" + chave[20..43]), "CNPJ/CPF");
        await Conferir(Completar(chave[..2] + mes + chave[6..43]), "Ano-Mes");
        await Conferir(Completar(chave[..20] + "55" + chave[22..43]), "Modelo");
    }

    [Fact]
    public async Task ASubstitutaDoMesAnteriorPassaNaChave()
    {
        // P31-50: o ano e o mês da substituta podem ser os da cancelada menos um; como ela não existe, 912.
        var (online, protocolo, offline) = await PrepararAsync();
        var mes = online.Emissao.AddMonths(-1).ToString("yyMM", CultureInfo.InvariantCulture);

        Assert.Equal("912", await CodigoAsync(Evento(online, protocolo, Completar(offline.Chave[..2] + mes + offline.Chave[6..43]))));
    }

    [Fact]
    public async Task RecusaASubstitutaInexistenteOuCancelada()
    {
        var (online, protocolo, offline) = await PrepararAsync();
        var outra = new NotaTeste { Modelo = "65", TipoEmissao = "9" };

        var inexistente = await EnviarAsync(Evento(online, protocolo, outra.Chave));
        var protocoloOffline = (await _fixture.Http.ConsultarProtocoloAsync(_fixture.Url("MG", Servico.NomeConsultaProtocolo), offline.Chave, Ct))
            .Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt");
        Assert.Equal("135", await CodigoAsync(new EventoTeste { Chave = offline.Chave, Protocolo = protocoloOffline }));
        var cancelada = await EnviarAsync(Evento(online, protocolo, offline.Chave));

        Assert.Equal(("912", "Rejeição: NF-e Substituta inexistente"), (inexistente.Campo("cStat"), inexistente.Campo("xMotivo")));
        Assert.Equal(("913", "Rejeição: NF-e Substituta Denegada ou Cancelada"), (cancelada.Campo("cStat"), cancelada.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaASubstitutaEmitidaMaisDeDuasHorasDepois()
    {
        var online = new NotaTeste { Modelo = "65", Destinatario = "52998224725" };
        var protocolo = await AutorizarAsync(online);
        _fixture.Relogio.Avanco = TimeSpan.FromHours(2) + TimeSpan.FromMinutes(10);
        var offline = new NotaTeste { Modelo = "65", TipoEmissao = "9", Destinatario = "52998224725", Emissao = NotaTeste.Agora() + _fixture.Relogio.Avanco };
        await AutorizarAsync(offline);

        var resultado = await EnviarAsync(Evento(online, protocolo, offline.Chave) with { Data = NotaTeste.Agora() + _fixture.Relogio.Avanco });

        Assert.Equal("914", resultado.Campo("cStat"));
    }

    [Fact]
    public async Task RecusaOCancelamentoPorSubstituicaoDepoisDeSeteDias()
    {
        var (online, protocolo, offline) = await PrepararAsync();
        _fixture.Relogio.Avanco = TimeSpan.FromHours(168) + TimeSpan.FromMinutes(1);

        Assert.Equal("501", await CodigoAsync(Evento(online, protocolo, offline.Chave) with { Data = NotaTeste.Agora() + _fixture.Relogio.Avanco }));
    }

    [Theory]
    [InlineData("915")]
    [InlineData("916")]
    [InlineData("917")]
    [InlineData("918")]
    [InlineData("919")]
    public async Task RecusaASubstitutaQueDifereDaCancelada(string codigo)
    {
        var (online, protocolo, offline) = await PrepararAsync(codigo switch
        {
            "915" => n => n with { ValorItem = 20.00m },
            "916" => n => n with { AliquotaIcms = 12.00m },
            "917" => n => n with { Destinatario = "98765432000198" },
            // O mesmo valor e o mesmo ICMS em dois itens.
            "918" => n => n with { Itens = 2, ValorItem = 5.00m },
            _ => n => n with { Produto = "OUTRO PRODUTO" },
        });

        Assert.Equal(codigo, await CodigoAsync(Evento(online, protocolo, offline.Chave)));
    }

    [Fact]
    public async Task RecusaASubstitutaEmitidaOnline()
    {
        var (online, protocolo, _) = await PrepararAsync();
        var substituta = new NotaTeste { Modelo = "65", Destinatario = "52998224725" };
        await AutorizarAsync(substituta);

        var resultado = await EnviarAsync(Evento(online, protocolo, substituta.Chave));

        Assert.Equal(("921", "Rejeição: Tipo de emissão da NF-e substituta deve ser de contingência"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }
}
