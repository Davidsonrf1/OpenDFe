namespace OpenDFe.Testes;

// O status do serviço, operação comum do núcleo (etapa 4.3): o parâmetro e o retorno, sem leiaute. A montagem e a
// leitura são do leiaute (OperacaoStatusTestes, no NFe.Testes).
public class RetornoStatusTestes
{
    [Fact]
    public void OParametroVaiAoDestinoNaturalSemDestino()
    {
        Assert.Null(new ConsultaStatus().Destino);
        Assert.Equal(Destino.SvcAN, new ConsultaStatus { Destino = Destino.SvcAN }.Destino);
        Assert.IsAssignableFrom<ParametroWS<RetornoStatus>>(new ConsultaStatus());
    }

    [Fact]
    public void ORetornoSoComOStatusEOMotivo()
    {
        var retorno = new RetornoStatus(107, "Servico em Operacao");

        Assert.Equal(107, retorno.Status);
        Assert.Equal("Servico em Operacao", retorno.Motivo);
        Assert.True(retorno.Operando);
        Assert.Null(retorno.Ambiente);
        Assert.Null(retorno.VersaoAplicativo);
        Assert.Null(retorno.UF);
        Assert.Null(retorno.Recebimento);
        Assert.Null(retorno.TempoMedio);
        Assert.Null(retorno.PrevisaoRetorno);
        Assert.Null(retorno.Observacao);
    }

    [Theory]
    [InlineData(107, true)]
    [InlineData(108, false)]
    [InlineData(109, false)]
    [InlineData(252, false)]
    public void SoO107EstaOperando(int status, bool operando)
    {
        Assert.Equal(operando, new RetornoStatus(status, "motivo").Operando);
    }

    [Fact]
    public void RecusaOMotivoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => new RetornoStatus(107, null!));
    }
}
