using System.Net;

namespace OpenDFe.Testes;

// O estado das falhas comandadas, sem o servidor: as conferências do pedido e o consumo.
public class ControleTestes
{
    [Theory]
    [InlineData((int)TipoFalha.Atraso, null)]
    [InlineData((int)TipoFalha.Atraso, 0)]
    [InlineData((int)TipoFalha.Paralisacao, 10)]
    public void ConfereOsMilissegundos(int tipo, int? milissegundos)
    {
        var controle = new Controle();

        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha((TipoFalha)tipo, Milissegundos: milissegundos)));
    }

    [Fact]
    public void RecusaOPedidoForaDoQueValeParaOTipo()
    {
        var controle = new Controle();

        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha((TipoFalha)99)));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Destino: "XX")));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Servico: "NFeInexistente")));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Vezes: 0)));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Depois: true)));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Codigo: "108")));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.RespostaIlegivel, Observacao: "x")));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, Codigo: "107")));
        Assert.Empty(controle.Listar());
    }

    [Fact]
    public void AceitaOServicoQueOEmuladorAindaNaoImplementa()
    {
        var controle = new Controle();

        var falha = controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", "NFeInutilizacao4"));

        Assert.Equal("108", falha.Codigo);
    }

    [Fact]
    public void ConsomeAPrimeiraQueValeDeCadaGrupoNaOrdem()
    {
        var controle = new Controle();
        var sp = controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Destino: "SP"));
        var atraso = controle.Acrescentar(new PedidoFalha(TipoFalha.Atraso, Milissegundos: 5, Vezes: 1));
        var derrubada = controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, Vezes: 1));
        var paralisacao = controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, Servico: "nfestatusservico4", Vezes: 2));

        var primeira = controle.Consumir("mg", Servico.NomeStatus);
        var segunda = controle.Consumir("MG", Servico.NomeStatus);
        var terceira = controle.Consumir("MG", Servico.NomeStatus);

        Assert.Equal(atraso.Id, primeira.Transporte?.Id);
        Assert.Equal(paralisacao.Id, primeira.Paralisacao?.Id);
        Assert.Equal(derrubada.Id, segunda.Transporte?.Id);
        Assert.Equal(paralisacao.Id, segunda.Paralisacao?.Id);
        Assert.Equal((null, null, null), terceira);
        Assert.Equal([sp.Id], controle.Listar().Select(f => f.Id));
        Assert.Equal(sp.Id, controle.Consumir("SP", "NFeAutorizacao4").Transporte?.Id);
        Assert.Equal(sp.Id, controle.Consumir("SP", Servico.NomeStatus).Transporte?.Id);
    }

    [Fact]
    public void ConsomeOLoteProcessandoSoNaConsultaDoRecibo()
    {
        var controle = new Controle();
        var lote = controle.Acrescentar(new PedidoFalha(TipoFalha.LoteProcessando, Destino: "MG", Vezes: 1));
        var soap = controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Vezes: 1));

        var autorizacao = controle.Consumir("MG", Servico.NomeAutorizacao);
        var recibo = controle.Consumir("MG", Servico.NomeRetAutorizacao);

        Assert.Equal((soap.Id, null, null), (autorizacao.Transporte?.Id, autorizacao.Paralisacao, autorizacao.Processamento));
        Assert.Equal((null, null, lote.Id), (recibo.Transporte, recibo.Paralisacao, recibo.Processamento?.Id));
        Assert.Empty(controle.Listar());
    }

    [Fact]
    public void DeixaParaOProximoOGrupoQueOPedidoJaTem()
    {
        var controle = new Controle();
        var soap = controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Vezes: 1));
        var paralisacao = controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, Vezes: 1));

        var primeira = controle.Consumir("MG", Servico.NomeAutorizacao, transporte: false);
        var segunda = controle.Consumir("MG", Servico.NomeAutorizacao, paralisacao: false);

        Assert.Equal((null, paralisacao.Id), (primeira.Transporte, primeira.Paralisacao?.Id));
        Assert.Equal((soap.Id, null), (segunda.Transporte?.Id, segunda.Paralisacao));
    }

    [Fact]
    public void RecusaOLoteProcessandoEmOutroServico()
    {
        var controle = new Controle();

        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.LoteProcessando, Servico: Servico.NomeStatus)));
        Assert.Throws<ArgumentException>(() => controle.Acrescentar(new PedidoFalha(TipoFalha.LoteProcessando, Codigo: "105")));
        var falha = controle.Acrescentar(new PedidoFalha(TipoFalha.LoteProcessando, Servico: "nferetautorizacao4"));
        Assert.Null(falha.Codigo);
    }

    [Fact]
    public void DescontaAsVezesRestantes()
    {
        var controle = new Controle();
        controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, Vezes: 3));

        controle.Consumir("MG", Servico.NomeStatus);

        Assert.Equal(2, Assert.Single(controle.Listar()).Restantes);
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.0.0.2", true)]
    [InlineData("::1", true)]
    [InlineData("::ffff:127.0.0.1", true)]
    [InlineData("192.168.0.10", false)]
    [InlineData("::ffff:192.168.0.10", false)]
    [InlineData("fe80::1", false)]
    public void AceitaOControleSoDoProprioComputador(string endereco, bool local)
    {
        Assert.Equal(local, RotasControle.Local(IPAddress.Parse(endereco)));
    }

    [Fact]
    public void RecusaOControleSemEndereco()
    {
        Assert.False(RotasControle.Local(null));
    }
}
