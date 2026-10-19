namespace OpenDFe.Testes;

public class ProblemaTestes
{
    [Fact]
    public void GuardaOsCampos()
    {
        var problema = new Problema("564", Severidade.Erro, "Total do Produto / Serviço difere do somatório dos itens")
        {
            Tag = "vProd",
            Caminho = "total/ICMSTot/vProd",
            Propriedade = "Total.Icms.ValorProdutos",
        };

        Assert.Equal("564", problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("Total do Produto / Serviço difere do somatório dos itens", problema.Mensagem);
        Assert.Equal("vProd", problema.Tag);
        Assert.Equal("total/ICMSTot/vProd", problema.Caminho);
        Assert.Equal("Total.Icms.ValorProdutos", problema.Propriedade);
    }

    [Fact]
    public void OndeEhOpcional()
    {
        var problema = new Problema("X1", Severidade.Aviso, "Mensagem");

        Assert.Null(problema.Tag);
        Assert.Null(problema.Caminho);
        Assert.Null(problema.Propriedade);
        Assert.Equal("Aviso X1: Mensagem", problema.ToString());
    }

    [Fact]
    public void TextoMostraAPropriedadeQuandoHa()
    {
        var problema = new Problema("564", Severidade.Erro, "Total difere") { Tag = "vProd", Propriedade = "Total.Icms.ValorProdutos" };
        Assert.Equal("Erro 564: Total difere (Total.Icms.ValorProdutos)", problema.ToString());
    }

    [Fact]
    public void CodigoEMensagemSaoObrigatorios()
    {
        Assert.Throws<ArgumentException>(() => new Problema("", Severidade.Erro, "Mensagem"));
        Assert.Throws<ArgumentException>(() => new Problema("564", Severidade.Erro, " "));
        Assert.Throws<ArgumentNullException>(() => new Problema(null!, Severidade.Erro, "Mensagem"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Problema("564", (Severidade)0, "Mensagem"));
    }
}
