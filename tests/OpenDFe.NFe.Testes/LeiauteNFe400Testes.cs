using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O leiaute (etapas 2.1 e 2.5): a versão, os documentos e a declaração, que só tem as formas de leitura.
public class LeiauteNFe400Testes
{
    [Fact]
    public void AVersaoEAdoAtributoDoXml()
    {
        Assert.Equal("4.00", new LeiauteNFe400().Versao);
    }

    [Fact]
    public void AtendeANFeEANFCe()
    {
        Assert.Equal([Documento.NFe, Documento.NFCe], new LeiauteNFe400().Documentos);
    }

    [Fact]
    public void OsDocumentosNaoSeAlteramDeFora()
    {
        var documentos = new LeiauteNFe400().Documentos;

        var lista = Assert.IsAssignableFrom<IList<Documento>>(documentos);
        Assert.Throws<NotSupportedException>(() => lista[0] = Documento.NFCe);
        Assert.Equal([Documento.NFe, Documento.NFCe], new LeiauteNFe400().Documentos);
    }

    [Theory]
    [InlineData(Documento.NFe)]
    [InlineData(Documento.NFCe)]
    public void ADeclaracaoSoTemAsFormasDeLeitura(Documento documento)
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);

        Assert.Equal(documento, declaracao.Documento);
        Assert.Empty(declaracao.Operacoes);
        Assert.Empty(declaracao.Atendimentos);
        Assert.Empty(declaracao.Enderecos);
        Assert.Null(declaracao.TipoConfiguracaoDocumento);
        Assert.Collection(declaracao.Leituras,
            f => Assert.Equal(("http://www.portalfiscal.inf.br/nfe", "NFe", "infNFe/@versao", "infNFe/ide/mod"),
                (f.EspacoNomes, f.Raiz, f.CaminhoVersao, f.CaminhoModelo)),
            f => Assert.Equal(("http://www.portalfiscal.inf.br/nfe", "nfeProc", "@versao", "NFe/infNFe/ide/mod"),
                (f.EspacoNomes, f.Raiz, f.CaminhoVersao, f.CaminhoModelo)));
    }

    [Fact]
    public void RecusaADeclaracaoNula()
    {
        Assert.Throws<ArgumentNullException>(() => new LeiauteNFe400().Declarar(null!));
    }
}
