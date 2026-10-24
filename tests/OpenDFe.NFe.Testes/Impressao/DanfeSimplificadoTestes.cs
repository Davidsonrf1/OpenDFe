using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFE Simplificado Tipo 2 da NF-e (<c>tpImp</c> 6, NT 2026.003, etapa 7.3): o leiaute do DANFE NFC-e com o cabeçalho
/// "DANFE Simplificado Tipo 2", o "NF-e nº" e a divisão III-A dos novos tributos. Os golden são conferidos contra as
/// figuras 4, 5 e 6 da NT (docs/IMPRESSAO.md 2.2).
/// </summary>
public sealed class DanfeSimplificadoTestes
{
    private static readonly LeiauteNFe400 Leiaute = new();

    private static string Texto(NotaLida lida, OpcoesDanfe? opcoes = null) => TextoPlano.Renderizar(Leiaute.Danfe(lida, opcoes));

    // A NF-e do Tipo 2: modelo 55, tpImp 6, os dois itens do exemplo 2 da NT (a cadeira e a mesa), o QR Code e a URL de consulta.
    private static NotaFiscal Tipo2(bool tributos = false)
    {
        var nota = NotasDanfe.Base(formato: FormatoImpressao.DanfeSimplificadoTipo2);
        nota.Identificacao.ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal;
        nota.Destinatario = new Destinatario { Cnpj = "99999999000191", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte };
        nota.ItemDanfe("003277", "Cadeira Est. Comtemp.", 2m, "Peça", 500m);
        nota.ItemDanfe("085273", "Mesa", 1m, "Peça", 1500m);
        nota.TotalizarDanfe(desconto: 500m, frete: 50m);
        nota.Suplementares = new InformacoesSuplementares { QrCode = NotasDanfe.QrCodeFixo, UrlChave = NotasDanfe.UrlConsulta };
        if (tributos)
        {
            nota.Total.IbsCbs = new TotalIbsCbs { Ibs = new TotalIbs { Valor = 2.05m }, Cbs = new TotalCbs { Valor = 18.45m } };
            nota.Total.ImpostoSeletivo = new TotalImpostoSeletivo { Valor = 2m };
            nota.Total.Icms.ValorNota += 18.45m + 2.05m + 2m;
        }

        return nota;
    }

    private static NotaFiscal Contingencia()
    {
        var nota = Tipo2();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        nota.Identificacao.InicioContingencia = NotasDanfe.Emissao.AddMinutes(-30);
        nota.Identificacao.JustificativaContingencia = "Sem comunicacao com a SEFAZ";
        return nota;
    }

    [Fact]
    public void ONormalSemOsNovosTributos() =>
        ArquivoEsperado.Conferir("Danfe2Normal", Texto(NotasDanfe.Autorizada(Tipo2())));

    [Fact]
    public void ONormalComADivisaoIIIADosNovosTributos() =>
        ArquivoEsperado.Conferir("Danfe2NovosTributos", Texto(NotasDanfe.Autorizada(Tipo2(tributos: true)), new OpcoesDanfe { Logotipo = NotasDanfe.Logotipo }));

    [Fact]
    public void AContingenciaOfflineViaConsumidor() =>
        ArquivoEsperado.Conferir("Danfe2ContingenciaConsumidor", Texto(NotasDanfe.Lida(Contingencia()), new OpcoesDanfe { Via = ViaDanfce.Consumidor }));

    [Fact]
    public void AContingenciaOfflineViaEstabelecimento() =>
        ArquivoEsperado.Conferir("Danfe2ContingenciaEstabelecimento", Texto(NotasDanfe.Lida(Contingencia()), new OpcoesDanfe { Via = ViaDanfce.Estabelecimento }));

    [Fact]
    public void NaBobinaDe58mm() =>
        ArquivoEsperado.Conferir("Danfe2Bobina58", Texto(NotasDanfe.Autorizada(Tipo2(tributos: true)), new OpcoesDanfe { Papel = PapelImpressao.Bobina58 }));

    [Fact]
    public void OPapelPadraoEABobinaDe80mmEOA4EmPeSaiSemQuebrarOsPesos()
    {
        Assert.Same(PapelImpressao.Bobina80, Leiaute.Danfe(NotasDanfe.Autorizada(Tipo2())).Papel);
        var a4 = Leiaute.Danfe(NotasDanfe.Autorizada(Tipo2(tributos: true)), new OpcoesDanfe { Papel = PapelImpressao.A4Retrato });
        Assert.Same(PapelImpressao.A4Retrato, a4.Papel);
        Assert.NotEmpty(a4.Elementos);
    }

    [Fact]
    public void OCabecalhoEAIdentificacaoSaoDoTipo2()
    {
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(Tipo2()));
        var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.Contains("DANFE Simplificado Tipo 2", textos);
        Assert.DoesNotContain(textos, t => t.StartsWith("Documento Auxiliar da Nota Fiscal de Consumidor", StringComparison.Ordinal));
        Assert.Contains(textos, t => t.StartsWith("NF-e nº 000000123 Série 001 14/10/2026 09:30:00", StringComparison.Ordinal));
        Assert.DoesNotContain(textos, t => t.StartsWith("NFC-e", StringComparison.Ordinal));
        Assert.Equal("DANFE Simplificado Tipo 2 000000123 série 001", documento.Titulo);
    }

    [Fact]
    public void OQrCodeEAChaveSaoOsDaNota()
    {
        var lida = NotasDanfe.Autorizada(Tipo2());
        var documento = Leiaute.Danfe(lida);

        Assert.Equal(NotasDanfe.QrCodeFixo, Assert.Single(documento.Elementos.OfType<QrCodeImpressao>()).Texto);
        var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Contains(textos, t => t.Replace(" ", string.Empty, StringComparison.Ordinal) == lida.Chave!.ToString());
        Assert.Contains(NotasDanfe.UrlConsulta, textos);
        Assert.Contains("Protocolo de autorização: 131260000012345", textos);
    }

    [Fact]
    public void ADivisaoIIIASoTemOQueExisteEOISSoQuandoMaiorQueZero()
    {
        string[] Tributos(NotaFiscal nota) => Leiaute.Danfe(NotasDanfe.Autorizada(nota)).Elementos.OfType<LinhaImpressao>()
            .Select(l => l.Celulas[0].Texto).Where(t => t.StartsWith("(+)", StringComparison.Ordinal)).ToArray();

        Assert.Empty(Tributos(Tipo2()));
        Assert.Equal(["(+) CBS R$", "(+) IBS R$", "(+) IS R$"], Tributos(Tipo2(tributos: true)));

        var somenteIbs = Tipo2(tributos: true);
        somenteIbs.Total.ImpostoSeletivo = new TotalImpostoSeletivo { Valor = 0m };
        Assert.Equal(["(+) CBS R$", "(+) IBS R$"], Tributos(somenteIbs));

        var soCbs = Tipo2();
        soCbs.Total.IbsCbs = new TotalIbsCbs { Cbs = new TotalCbs { Valor = 9m } };
        Assert.Equal(["(+) CBS R$"], Tributos(soCbs));
    }

    [Fact]
    public void ADivisaoIIIAVemEntreOsTotaisEOValorAPagar()
    {
        var elementos = Leiaute.Danfe(NotasDanfe.Autorizada(Tipo2(tributos: true))).Elementos;
        var rotulos = elementos.OfType<LinhaImpressao>().Select(l => l.Celulas[0].Texto).ToList();

        Assert.True(rotulos.IndexOf("Frete R$") < rotulos.IndexOf("(+) CBS R$"));
        Assert.True(rotulos.IndexOf("(+) IS R$") < rotulos.IndexOf("Valor a Pagar R$"));
        var pagar = elementos.OfType<LinhaImpressao>().Single(l => l.Celulas[0].Texto == "Valor a Pagar R$");
        Assert.Equal("2.072,50", pagar.Celulas[1].Texto);
    }

    [Fact]
    public void AContingenciaTemAsDuasLegendasEAViaDoEstabelecimento()
    {
        var consumidor = Leiaute.Danfe(NotasDanfe.Lida(Contingencia()), new OpcoesDanfe { Via = ViaDanfce.Consumidor });
        var estabelecimento = Leiaute.Danfe(NotasDanfe.Lida(Contingencia()), new OpcoesDanfe { Via = ViaDanfce.Estabelecimento });

        Assert.Equal(2, consumidor.Elementos.OfType<TextoImpressao>().Count(t => t.Texto == "EMITIDA EM CONTINGÊNCIA"));
        Assert.Contains(consumidor.Elementos.OfType<TextoImpressao>(), t => t.Texto == "Via do Consumidor");
        Assert.Contains(estabelecimento.Elementos.OfType<TextoImpressao>(), t => t.Texto == "Via do Estabelecimento");
        Assert.DoesNotContain(consumidor.Elementos.OfType<TextoImpressao>(), t => t.Texto.StartsWith("Protocolo de autorização", StringComparison.Ordinal));
    }

    [Fact]
    public void OHomologacaoTemAFraseDoManual()
    {
        var nota = Tipo2();
        nota.Identificacao.Ambiente = Ambiente.Homologacao;

        var textos = Leiaute.Danfe(NotasDanfe.Autorizada(nota)).Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.Contains("EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL", textos);
    }

    [Fact]
    public void RecusaOTipo2SemOQrCodeEComOrientacaoOuPapelInvalido()
    {
        var notaFaltaQr = Tipo2();
        notaFaltaQr.Suplementares!.QrCode = null;
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Autorizada(notaFaltaQr)));

        var lida = NotasDanfe.Autorizada(Tipo2());
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida, new OpcoesDanfe { Orientacao = OrientacaoDanfe.Retrato }));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida, new OpcoesDanfe { Papel = PapelImpressao.A4Paisagem }));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida, new OpcoesDanfe { Via = (ViaDanfce)9 }));
    }

    [Fact]
    public void ODanfceDaNFCeContinuaIgualEORecusaAANFe()
    {
        // O Tipo 2 reaproveita a montagem do DANFCE: a NFC-e não muda (os golden do DANFCE) e o DANFCE segue recusando a NF-e.
        var lida = NotasDanfe.Autorizada(Tipo2());

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida));

        Assert.Contains("modelo 65", erro.Message, StringComparison.Ordinal);
    }
}
