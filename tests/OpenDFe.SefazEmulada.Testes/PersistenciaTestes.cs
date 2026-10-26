namespace OpenDFe.Testes;

// A persistência (--persistir sim): o que o emulador guardou numa partida vale na seguinte, com os dados de
// controle no SQLite e os XMLs em arquivos por autorizador. Sem ela, cada partida começa vazia.
public sealed class PersistenciaTestes : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "OpenDFe.SefazEmulada.Testes", Guid.NewGuid().ToString("N"));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private Task<Emulador> IniciarAsync(bool persistir) =>
        Emulador.IniciarAsync(new OpcoesEmulador { Porta = 0, Pasta = _pasta, Persistir = persistir }, Ct);

    private static HttpClient Cliente(Emulador emulador) => Clientes.Criar(emulador.Autoridade, Clientes.CertificadoCliente);

    private static Uri Url(Emulador emulador, string servico, string destino = "MG") => new(emulador.Endereco, $"{destino}/{servico}");

    [Fact]
    public async Task AsNotasOsLotesOsEventosECadastrosValemNaPartidaSeguinte()
    {
        var nota = new NotaTeste();
        var duas = new[] { new NotaTeste(), new NotaTeste() };
        string recibo;
        string protocolo;
        await using (var emulador = await IniciarAsync(persistir: true))
        {
            using var http = Cliente(emulador);
            var sincrona = await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
            protocolo = sincrona.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt");
            var lote = await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), false, [.. duas.Select(n => n.Gerar())], Ct);
            recibo = lote.Element(Mensagens.NFe + "infRec")!.Campo("nRec");
            emulador.Codigos.Cadastrar(new PedidoCsc("MG", "11222333", "2", "CSC-PERSISTIDO-0123456789"));
            emulador.Irregulares.Cadastrar("12345678000195");
            emulador.Cadastros.Cadastrar(new Contribuinte("MG", "0623079040081", Cnpj: "11222333000181", Nome: "EMPRESA DE TESTE LTDA"));
        }

        // Os XMLs em arquivos, por autorizador.
        var xml = Path.Combine(_pasta, "persistencia", "xml", "MG");
        Assert.True(File.Exists(Path.Combine(xml, "nfe", nota.Chave + ".xml")));
        Assert.True(File.Exists(Path.Combine(xml, "lotes", recibo + ".xml")));
        Assert.True(File.Exists(Path.Combine(_pasta, "persistencia", "controle.db")));

        await using (var emulador = await IniciarAsync(persistir: true))
        {
            using var http = Cliente(emulador);
            var consulta = await http.ConsultarProtocoloAsync(Url(emulador, Servico.NomeConsultaProtocolo), nota.Chave, Ct);
            Assert.Equal("100", consulta.Campo("cStat"));
            Assert.Equal(protocolo, consulta.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt"));

            // A mesma nota de novo: a duplicidade vê a de antes.
            var repetida = await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
            Assert.Equal("204", repetida.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat"));

            // O recibo do lote continua consultável.
            var retorno = await http.ConsultarReciboAsync(Url(emulador, Servico.NomeRetAutorizacao), recibo, Ct);
            Assert.Equal("104", retorno.Campo("cStat"));
            Assert.Equal(2, retorno.Elements(Mensagens.NFe + "protNFe").Count());

            // A sequência segue: o protocolo novo não repete o de antes.
            var outra = await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), true, [new NotaTeste().Gerar()], Ct);
            Assert.NotEqual(protocolo, outra.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt"));

            Assert.Equal(4, emulador.Repositorio.Notas().Count);
            Assert.Contains(emulador.Codigos.Listar(), c => c.Id == "2" && c.Csc == "CSC-PERSISTIDO-0123456789");
            Assert.Equal(["12345678000195"], emulador.Irregulares.Listar());
            Assert.Single(emulador.Cadastros.Listar());
        }
    }

    [Fact]
    public async Task SemAPersistenciaACadaPartidaComecaVazia()
    {
        var nota = new NotaTeste();
        await using (var emulador = await IniciarAsync(persistir: false))
        {
            using var http = Cliente(emulador);
            await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        }

        Assert.False(Directory.Exists(Path.Combine(_pasta, "persistencia")));
        await using (var emulador = await IniciarAsync(persistir: true))
            Assert.Empty(emulador.Repositorio.Notas());
    }

    [Fact]
    public async Task ZerarEsqueceOsDadosEOsArquivos()
    {
        var nota = new NotaTeste();
        await using (var emulador = await IniciarAsync(persistir: true))
        {
            using var http = Cliente(emulador);
            await http.AutorizarAsync(Url(emulador, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
            emulador.Repositorio.Limpar();
        }

        Assert.False(File.Exists(Path.Combine(_pasta, "persistencia", "xml", "MG", "nfe", nota.Chave + ".xml")));
        await using (var emulador = await IniciarAsync(persistir: true))
            Assert.Empty(emulador.Repositorio.Notas());
    }
}
