using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O leiaute (etapas 2.1 e 2.5): a versão, os documentos e a declaração, com as formas de leitura (os autorizadores,
// da 4.2, em AutorizadoresNFe400Testes).
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

    public static TheoryData<Documento, Type[]> OperacoesDocumento => new()
    {
        // a NFC-e ganha a autorização e a recuperação com o QR Code, na 5.1 (CONTRATOS.md 14 e 15, decisão 12); só a NF-e tem a distribuição DF-e (8.1)
        { Documento.NFe, [typeof(ConsultaStatus), typeof(ConsultaProtocolo), typeof(Autorizacao), typeof(ConsultaRecibo), typeof(RecuperacaoAutorizacao), typeof(EnvioEvento), typeof(Inutilizacao), typeof(DistribuicaoDFe), typeof(OpenDFe.NFe.ConsultaCadastro)] },
        { Documento.NFCe, [typeof(ConsultaStatus), typeof(ConsultaProtocolo), typeof(Autorizacao), typeof(ConsultaRecibo), typeof(RecuperacaoAutorizacao), typeof(EnvioEvento), typeof(Inutilizacao)] },
    };

    [Theory]
    [MemberData(nameof(OperacoesDocumento))]
    public void ADeclaracaoTemAsFormasDeLeituraEAsOperacoesDoDocumento(Documento documento, Type[] parametros)
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);

        Assert.Equal(documento, declaracao.Documento);
        Assert.Equal(parametros, declaracao.Operacoes.Select(o => o.Parametro));
        Assert.Equal(typeof(RetornoStatus), declaracao.Operacoes[0].Retorno);
        Assert.Equal(typeof(RetornoProtocolo), declaracao.Operacoes[1].Retorno);
        Assert.All(declaracao.Operacoes.Skip(2).Take(3), o => Assert.Equal(typeof(RetornoAutorizacao), o.Retorno));
        Assert.Equal(typeof(RetornoEvento), declaracao.Operacoes[5].Retorno);   // os eventos e a inutilização entram na 6.1
        Assert.Equal(typeof(RetornoInutilizacao), declaracao.Operacoes[6].Retorno);
        Assert.Equal(documento == Documento.NFe, declaracao.Operacoes.Any(o => o.Retorno == typeof(RetornoDistribuicao)));
        Assert.NotEmpty(declaracao.Atendimentos);
        Assert.NotEmpty(declaracao.Enderecos);
        Assert.Equal(typeof(ConfiguracaoNFCe), declaracao.TipoConfiguracaoDocumento);
        Assert.False(declaracao.ConfiguracaoDocumentoObrigatoria);
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
