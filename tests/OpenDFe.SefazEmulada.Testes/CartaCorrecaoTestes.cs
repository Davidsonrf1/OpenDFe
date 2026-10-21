using System.Xml.Linq;

namespace OpenDFe.Testes;

// A carta de correção (110110; MOC 7.0, Visão Geral, 5.10, Tabela 5-40): registrada e vinculada à nota, a
// sequência, a NFC-e, a nota cancelada e as cartas na consulta.
public sealed class CartaCorrecaoTestes(EmuladorFixture fixture) : IClassFixture<EmuladorFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(NotaTeste Nota, string Protocolo)> AutorizarAsync(NotaTeste? nota = null)
    {
        nota ??= new NotaTeste();
        var retorno = await fixture.Http.AutorizarAsync(fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal("100", protocolo.Campo("cStat"));
        return (nota, protocolo.Campo("nProt"));
    }

    private async Task<XElement> EnviarAsync(EventoTeste evento) =>
        (await fixture.Http.EnviarEventosAsync(fixture.Url("MG", Servico.NomeRecepcaoEvento), [evento.Gerar()], Ct))
            .Element(Mensagens.NFe + "retEvento")!.Element(Mensagens.NFe + "infEvento")!;

    private static EventoTeste Carta(NotaTeste nota, int sequencia = 1) =>
        new() { Chave = nota.Chave, Tipo = EventoTeste.CartaCorrecao, Sequencia = sequencia };

    [Fact]
    public async Task RegistraACartaDeCorrecao()
    {
        var (nota, _) = await AutorizarAsync(new NotaTeste { Destinatario = "98765432000198" });

        var resultado = await EnviarAsync(Carta(nota));

        Assert.Equal(["tpAmb", "verAplic", "cOrgao", "cStat", "xMotivo", "chNFe", "tpEvento", "xEvento", "nSeqEvento", "dhRegEvento", "nProt"],
            resultado.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("135", "110110", "Carta de Correção registrada", "1"),
            (resultado.Campo("cStat"), resultado.Campo("tpEvento"), resultado.Campo("xEvento"), resultado.Campo("nSeqEvento")));
    }

    [Fact]
    public async Task AceitaADescricaoComAcento()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("135", (await EnviarAsync(Carta(nota) with { DescricaoCarta = "Carta de Correção" })).Campo("cStat"));
    }

    [Fact]
    public async Task ACartaNovaVemDepoisDaAnteriorNaConsulta()
    {
        var (nota, _) = await AutorizarAsync();
        Assert.Equal("135", (await EnviarAsync(Carta(nota))).Campo("cStat"));
        Assert.Equal("135", (await EnviarAsync(Carta(nota, 2) with { Correcao = "SEGUNDA CORRECAO DO TESTE" })).Campo("cStat"));

        var consulta = await fixture.Http.ConsultarProtocoloAsync(fixture.Url("MG", Servico.NomeConsultaProtocolo), nota.Chave, Ct);

        Assert.Equal("100", consulta.Campo("cStat"));
        Assert.Equal(["1", "2"], consulta.Elements(Mensagens.NFe + "procEventoNFe")
            .Select(p => p.Element(Mensagens.NFe + "evento")!.Element(Mensagens.NFe + "infEvento")!.Campo("nSeqEvento")));
        Assert.Empty(Esquemas.Validar("retConsSitNFe_v4.00.xsd", consulta));
    }

    [Fact]
    public async Task RecusaASequenciaRepetidaEAcimaDeVinte()
    {
        var (nota, _) = await AutorizarAsync();
        Assert.Equal("135", (await EnviarAsync(Carta(nota, 20))).Campo("cStat"));

        Assert.Equal("573", (await EnviarAsync(Carta(nota, 20))).Campo("cStat"));
        var acima = await EnviarAsync(Carta(nota, 21));
        Assert.Equal(("594", "Rejeição: O número de sequencia do evento informado é maior que o permitido"), (acima.Campo("cStat"), acima.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaANfce()
    {
        var (nota, _) = await AutorizarAsync(new NotaTeste { Modelo = "65" });

        var resultado = await EnviarAsync(Carta(nota));

        Assert.Equal(("784", "Rejeição: NFC-e não permite o evento de Carta de Correção"), (resultado.Campo("cStat"), resultado.Campo("xMotivo")));
    }

    [Fact]
    public async Task RecusaANotaInexistenteECancelada()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", (await EnviarAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo })).Campo("cStat"));

        Assert.Equal("494", (await EnviarAsync(Carta(new NotaTeste()))).Campo("cStat"));
        var cancelada = await EnviarAsync(Carta(nota));
        Assert.Equal(("580", "Rejeição: O evento exige uma NF-e autorizada"), (cancelada.Campo("cStat"), cancelada.Campo("xMotivo")));
    }

    [Fact]
    public async Task CancelaDepoisDaCartaEAConsultaTrazOsDois()
    {
        var (nota, protocolo) = await AutorizarAsync();
        Assert.Equal("135", (await EnviarAsync(Carta(nota))).Campo("cStat"));
        Assert.Equal("135", (await EnviarAsync(new EventoTeste { Chave = nota.Chave, Protocolo = protocolo })).Campo("cStat"));

        var consulta = await fixture.Http.ConsultarProtocoloAsync(fixture.Url("MG", Servico.NomeConsultaProtocolo), nota.Chave, Ct);

        Assert.Equal("101", consulta.Campo("cStat"));
        Assert.Equal(["110110", "110111"], consulta.Elements(Mensagens.NFe + "procEventoNFe")
            .Select(p => p.Element(Mensagens.NFe + "retEvento")!.Element(Mensagens.NFe + "infEvento")!.Campo("tpEvento")));
    }

    [Fact]
    public async Task RecusaAEmissaoDepoisDoEvento()
    {
        var (nota, _) = await AutorizarAsync();

        Assert.Equal("577", (await EnviarAsync(Carta(nota) with { Data = NotaTeste.Agora().AddMinutes(-2) })).Campo("cStat"));
    }
}
