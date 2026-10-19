namespace OpenDFe.Testes;

// O valor de cada membro é o que vai no XML (PROJETO.md 4.3).
public class EnumeracoesTestes
{
    [Theory]
    [InlineData(Ambiente.Producao, 1)]
    [InlineData(Ambiente.Homologacao, 2)]
    public void AmbienteTpAmb(Ambiente ambiente, int tpAmb) => Assert.Equal(tpAmb, (int)ambiente);

    [Fact]
    public void AmbienteTemDoisValores() => Assert.Equal(2, Enum.GetValues<Ambiente>().Length);

    [Theory]
    [InlineData(TipoEmissao.Normal, 1)]
    [InlineData(TipoEmissao.ContingenciaFSIA, 2)]
    [InlineData(TipoEmissao.ContingenciaEpec, 4)]
    [InlineData(TipoEmissao.ContingenciaFSDA, 5)]
    [InlineData(TipoEmissao.ContingenciaSvcAN, 6)]
    [InlineData(TipoEmissao.ContingenciaSvcRS, 7)]
    [InlineData(TipoEmissao.ContingenciaOffline, 9)]
    public void TipoEmissaoTpEmis(TipoEmissao tipo, int tpEmis) => Assert.Equal(tpEmis, (int)tipo);

    [Fact]
    public void TipoEmissaoTemSeteValores() => Assert.Equal(7, Enum.GetValues<TipoEmissao>().Length);

    [Theory]
    [InlineData(Documento.NFe, 55)]
    [InlineData(Documento.NFCe, 65)]
    public void DocumentoModelo(Documento documento, int mod) => Assert.Equal(mod, (int)documento);

    [Fact]
    public void SeveridadeErroEhMaiorQueAviso() => Assert.True(Severidade.Erro > Severidade.Aviso);
}
