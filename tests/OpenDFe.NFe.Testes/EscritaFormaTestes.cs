using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A forma do XML escrito e os problemas da montagem (2.4; MODELO.md 6 e 11.13).
public class EscritaFormaTestes
{
    [Fact]
    public void ODestinatarioNoExteriorEAContingenciaPassamNoSchema()
    {
        var nota = NotasTeste.NFe();
        nota.Identificacao.LocalDestino = LocalDestino.Exterior;
        nota.Destinatario = new Destinatario
        {
            IdEstrangeiro = "", Nome = "CLIENTE NO EXTERIOR", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
            Endereco = new EnderecoDestinatario
            {
                Logradouro = "MAIN STREET", Numero = "1", Bairro = "DOWNTOWN", CodigoMunicipio = 9999999, Municipio = "EXTERIOR",
                Exterior = true, CodigoPais = 2496, Pais = "ESTADOS UNIDOS",
            },
        };
        nota.Entrega = null;
        nota.Exportacao = new Exportacao { UFSaida = UF.SP, LocalEmbarque = "PORTO DE SANTOS" };
        var nucleo = NotasTeste.Nucleo(Documento.NFe, TipoEmissao.ContingenciaSvcAN,
            new Contingencia(NotasTeste.Emissao.AddMinutes(-30), "SEFAZ FORA DO AR DESDE AS 14H"));
        NotasTeste.AssertValida(nota, nucleo);

        var xml = NotasTeste.Escrever(nota, nucleo).Xml;
        Assert.Contains("<idEstrangeiro></idEstrangeiro>", xml, StringComparison.Ordinal);
        Assert.Contains("<UF>EX</UF>", xml, StringComparison.Ordinal);
        Assert.Contains("<tpEmis>6</tpEmis>", xml, StringComparison.Ordinal);
        Assert.Contains("<dhCont>2026-10-20T14:00:00-03:00</dhCont><xJust>SEFAZ FORA DO AR DESDE AS 14H</xJust>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OXmlSaiSemDeclaracaoSemPrefixoESemEspacos()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe());

        Assert.StartsWith("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\" Id=\"NFe", escrita.Xml, StringComparison.Ordinal);
        // A NFC-e leva o infNFeSupl que a lib gera (etapa 5.1), depois do infNFe.
        Assert.Contains("</infNFe><infNFeSupl><qrCode>", escrita.Xml, StringComparison.Ordinal);
        Assert.EndsWith("</urlChave></infNFeSupl></NFe>", escrita.Xml, StringComparison.Ordinal);
        Assert.DoesNotContain("> <", escrita.Xml, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", escrita.Xml, StringComparison.Ordinal);
        Assert.DoesNotContain(" />", escrita.Xml, StringComparison.Ordinal);
        Assert.Contains("<det nItem=\"1\"><prod>", escrita.Xml, StringComparison.Ordinal);
        Assert.Contains("<pag><detPag><tPag>17</tPag><vPag>50.00</vPag><card><tpIntegra>2</tpIntegra></card></detPag></pag>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AChaveSaiDaNotaEDoNucleo()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.CodigoNumerico = 1234;
        var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo());

        var chave = escrita.Chave!;
        Assert.Equal("NFe" + chave, escrita.Id);
        Assert.Equal(UF.MG, chave.UF);
        Assert.Equal(Documento.NFCe, chave.Modelo);
        Assert.Equal(NotasTeste.CnpjEmitente, chave.Emitente);
        Assert.Equal((1, 123, 1234), (chave.Serie, chave.Numero, chave.CodigoNumerico));
        Assert.Equal(TipoEmissao.Normal, chave.TipoEmissao);
        Assert.Contains($"Id=\"NFe{chave}\"", escrita.Xml, StringComparison.Ordinal);
        Assert.Contains("<cUF>31</cUF><cNF>00001234</cNF>", escrita.Xml, StringComparison.Ordinal);
        Assert.Contains("<mod>65</mod>", escrita.Xml, StringComparison.Ordinal);
        Assert.Contains($"<tpEmis>1</tpEmis><cDV>{chave.DigitoVerificador}</cDV><tpAmb>2</tpAmb>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OCodigoNumericoNuloESorteadoSemAlterarANota()
    {
        var nota = NotasTeste.NFCe();
        var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo());

        Assert.Null(nota.Identificacao.CodigoNumerico);
        Assert.Null(nota.Identificacao.UF);
        Assert.Null(nota.Identificacao.Modelo);
        Assert.NotEqual(nota.Identificacao.Numero, escrita.Chave!.CodigoNumerico);
        Assert.Contains($"<cNF>{escrita.Chave.CodigoNumerico:D8}</cNF>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OCampoDiferenteDoNucleoEProblemaESaiOValorDoNucleo()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.Ambiente = Ambiente.Producao;
        nota.Identificacao.UF = UF.MG;
        var escrita = NotasTeste.Escrever(nota);

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal(("NFE-NUCLEO", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.Equal("tpAmb", problema.Tag);
        Assert.Equal("NFe/infNFe/ide/tpAmb", problema.Caminho);
        Assert.Equal("Identificacao.Ambiente", problema.Propriedade);
        Assert.Contains("<tpAmb>2</tpAmb>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AContingenciaNaNotaComONucleoNormalEProblema()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.InicioContingencia = NotasTeste.Emissao;
        nota.Identificacao.JustificativaContingencia = "SEFAZ FORA DO AR DESDE AS 14H";
        var escrita = NotasTeste.Escrever(nota);

        Assert.Equal(["Identificacao.InicioContingencia", "Identificacao.JustificativaContingencia"], escrita.Problemas.Select(p => p.Propriedade));
        Assert.All(escrita.Problemas, p => Assert.Equal("NFE-NUCLEO", p.Codigo));
        Assert.DoesNotContain("<dhCont>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AContingenciaIgualANucleoNaoEProblema()
    {
        var contingencia = new Contingencia(NotasTeste.Emissao, "SEFAZ FORA DO AR DESDE AS 14H");
        var nota = NotasTeste.NFCe();
        nota.Identificacao.InicioContingencia = NotasTeste.Emissao.ToUniversalTime();
        nota.Identificacao.JustificativaContingencia = contingencia.Justificativa;
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        var escrita = NotasTeste.Escrever(nota, NotasTeste.Nucleo(Documento.NFCe, TipoEmissao.ContingenciaOffline, contingencia));

        // A contingência é a do núcleo e não diverge; o único problema é o QR Code offline, que entra na 5.2 (CONTRATOS.md 16, decisão 9).
        Assert.Equal(["NFE-QRCODE"], escrita.Problemas.Select(p => p.Codigo));
        Assert.Equal(TipoEmissao.ContingenciaOffline, escrita.Chave!.TipoEmissao);
        Assert.Contains("<dhCont>2026-10-20T14:30:00-03:00</dhCont>", escrita.Xml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Emissao", "NFe/infNFe/ide/dhEmi", "Identificacao.Emissao")]
    [InlineData("Numero", "NFe/infNFe/ide/nNF", "Identificacao.Numero")]
    [InlineData("Serie", "NFe/infNFe/ide/serie", "Identificacao.Serie")]
    [InlineData("Cnpj", "NFe/infNFe/emit/CNPJ", "Emitente.Cnpj")]
    [InlineData("CnpjInvalido", "NFe/infNFe/emit/CNPJ", "Emitente.Cnpj")]
    [InlineData("CpfInvalido", "NFe/infNFe/emit/CPF", "Emitente.Cpf")]
    [InlineData("Ano", "NFe/infNFe/ide/dhEmi", "Identificacao.Emissao")]
    [InlineData("SerieGrande", "NFe/infNFe/ide/serie", "Identificacao.Serie")]
    [InlineData("CodigoNumerico", "NFe/infNFe/ide/cNF", "Identificacao.CodigoNumerico")]
    public void SemUmDadoDaChaveANotaSaiSemIdESemDigito(string falta, string caminho, string propriedade)
    {
        var nota = NotasTeste.NFCe();
        switch (falta)
        {
            case "Emissao": nota.Identificacao.Emissao = null; break;
            case "Numero": nota.Identificacao.Numero = null; break;
            case "Serie": nota.Identificacao.Serie = null; break;
            case "Cnpj": nota.Emitente.Cnpj = null; break;
            case "CnpjInvalido": nota.Emitente.Cnpj = "11222333000182"; break;
            case "CpfInvalido": nota.Emitente.Cnpj = null; nota.Emitente.Cpf = "52998224726"; break;
            case "Ano": nota.Identificacao.Emissao = new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero); break;
            case "SerieGrande": nota.Identificacao.Serie = 1000; break;
            case "CodigoNumerico": nota.Identificacao.CodigoNumerico = 100_000_000; break;
        }
        var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo());

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal(("NFE-CHAVE", caminho, propriedade), (problema.Codigo, problema.Caminho, problema.Propriedade));
        Assert.Null(escrita.Chave);
        Assert.Null(escrita.Id);
        Assert.StartsWith("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\"><ide>", escrita.Xml, StringComparison.Ordinal);
        Assert.DoesNotContain("<cDV>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OEmitenteCpfEntraNaChave()
    {
        var nota = NotasTeste.NFCe();
        nota.Emitente.Cnpj = null;
        nota.Emitente.Cpf = "52998224725";
        var escrita = NotasTeste.Escrever(nota);

        Assert.Empty(escrita.Problemas);
        Assert.Equal("00052998224725", escrita.Chave!.Emitente);
    }

    [Fact]
    public void DuasAlternativasDeUmaEscolhaSaoProblema()
    {
        var nota = NotasTeste.NFe();
        nota.Destinatario!.Cpf = "52998224725";
        nota.Itens[1].Imposto.Icms = new Icms00();
        nota.Itens[0].Produto.Medicamento = new Medicamento();
        nota.Itens[0].Produto.Recopi = "12345678901234567890";
        nota.Retirada!.Exterior = true;
        var escrita = NotasTeste.Escrever(nota);

        Assert.All(escrita.Problemas, p => Assert.Equal(("NFE-EXCLUSIVO", Severidade.Erro), (p.Codigo, p.Severidade)));
        Assert.Equal(
            [
                ("NFe/infNFe/dest", "Destinatario.Cpf"),
                ("NFe/infNFe/retirada", "Retirada.UF"),
                ("NFe/infNFe/det[1]/prod", "Itens[0].Produto.Recopi"),
                ("NFe/infNFe/det[2]/imposto", "Itens[1].Imposto.Issqn"),
            ],
            escrita.Problemas.Select(p => (p.Caminho, p.Propriedade)));
        // A escrita segue com todas, para apontar o resto; o problema impede o envio.
        Assert.Contains("<CNPJ>11444777000161</CNPJ><CPF>52998224725</CPF>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OCaractereQueOXmlNaoAceitaEProblemaEFicaDeFora()
    {
        var nota = NotasTeste.NFe();
        nota.Emitente.Nome = "LOJA\u0001";
        nota.Itens[1].Produto.Descricao = "INSTALA\u0007CAO";
        nota.Itens[0].Produto.Nve[0] = "A\uD800";
        var escrita = NotasTeste.Escrever(nota);

        Assert.All(escrita.Problemas, p => Assert.Equal("NFE-CARACTERE", p.Codigo));
        Assert.Equal(
            [
                ("NFe/infNFe/emit/xNome", "Emitente.Nome", "xNome"),
                ("NFe/infNFe/det[1]/prod/NVE", "Itens[0].Produto.Nve[0]", "NVE"),
                ("NFe/infNFe/det[2]/prod/xProd", "Itens[1].Produto.Descricao", "xProd"),
            ],
            escrita.Problemas.Select(p => (p.Caminho, p.Propriedade, p.Tag)));
        Assert.Contains("U+0001, na posição 5", escrita.Problemas[0].Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("<xProd>INSTALA", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OTextoSaiComoEstaComOsCaracteresEspeciaisEscapados()
    {
        var nota = NotasTeste.NFe();
        nota.InformacoesAdicionais!.Complementares = "LINHA 1\nLINHA 2 & \"3\" <4> \U0001F600";
        nota.Emitente.Nome = " COM ESPACOS ";
        var xml = NotasTeste.Escrever(nota).Xml;

        Assert.Contains("<infCpl>LINHA 1\nLINHA 2 &amp; \"3\" &lt;4&gt; \U0001F600</infCpl>", xml, StringComparison.Ordinal);
        Assert.Contains("<xNome> COM ESPACOS </xNome>", xml, StringComparison.Ordinal);
        Assert.Contains("<obsCont xCampo=\"pedido\"><xTexto>99</xTexto></obsCont>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OTextoVazioENuloOmitemATag()
    {
        var nota = NotasTeste.NFCe();
        nota.Emitente.NomeFantasia = "";
        nota.Emitente.InscricaoMunicipal = null;
        var xml = NotasTeste.Escrever(nota).Xml;

        Assert.DoesNotContain("<xFant>", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("<IM>", xml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("50", "50.00")]
    [InlineData("10.123", "10.123")]
    [InlineData("10.1000", "10.10")]
    [InlineData("0", "0.00")]
    public void ODecimalSaiComAsCasasDoCampoSemArredondar(string valor, string esperado)
    {
        var nota = NotasTeste.NFCe();
        nota.Total.Icms.ValorNota = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Contains($"<vNF>{esperado}</vNF>", NotasTeste.Escrever(nota).Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AsCasasVemDoTipoDoCampo()
    {
        var xml = NotasTeste.Escrever(NotasTeste.NFe()).Xml;

        Assert.Contains("<qCom>1.00</qCom><vUnCom>1000.00</vUnCom>", xml, StringComparison.Ordinal);   // TDec_1104v e TDec_1110v
        Assert.Contains("<pPIS>1.65</pPIS>", xml, StringComparison.Ordinal);                         // TDec_0302a04
        Assert.Contains("<pesoL>10.500</pesoL><pesoB>11.250</pesoB>", xml, StringComparison.Ordinal); // TDec_1203
        Assert.Contains("<pIBSUF>0.10</pIBSUF>", xml, StringComparison.Ordinal);                     // TDec_0302_04RTC
    }

    [Fact]
    public void OCstNuloDoGrupoDeUmValorSaiComOValorDoGrupo()
    {
        var xml = NotasTeste.Escrever(NotasTeste.NFe()).Xml;
        Assert.Contains("<ICMS00><orig>0</orig><CST>00</CST><modBC>3</modBC>", xml, StringComparison.Ordinal);

        var nota = NotasTeste.NFCe();
        nota.Itens[0].Imposto.Icms = new IcmsSn500 { Origem = OrigemMercadoria.Nacional };
        Assert.Contains("<ICMSSN500><orig>0</orig><CSOSN>500</CSOSN></ICMSSN500>", NotasTeste.Escrever(nota).Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OsValoresQueNaoSaoNumeroSaoTraduzidos()
    {
        var nota = NotasTeste.NFe();
        nota.Itens[0].Produto.Escala = IndicadorEscala.NaoRelevante;
        var xml = NotasTeste.Escrever(nota).Xml;

        Assert.Contains("<indEscala>N</indEscala>", xml, StringComparison.Ordinal);
        Assert.Contains("<refECF><mod>2D</mod><nECF>1</nECF><nCOO>123</nCOO></refECF>", xml, StringComparison.Ordinal);

        nota.Itens[0].Produto.Escala = (IndicadorEscala)7;
        Assert.Contains("<indEscala>7</indEscala>", NotasTeste.Escrever(nota).Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OValorForaDaEnumeracaoSaiComoONumero()
    {
        var nota = NotasTeste.NFCe();
        nota.Pagamento.Formas[0].Meio = (MeioPagamento)77;
        nota.Pagamento.Formas[0].Transacao!.Bandeira = (BandeiraCartao)5;
        var xml = NotasTeste.Escrever(nota).Xml;

        Assert.Contains("<tPag>77</tPag>", xml, StringComparison.Ordinal);
        Assert.Contains("<tBand>05</tBand>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OsItensSaoNumeradosPelaPosicao()
    {
        var xml = NotasTeste.Escrever(NotasTeste.NFe()).Xml;
        Assert.Contains("<det nItem=\"1\">", xml, StringComparison.Ordinal);
        Assert.Contains("<det nItem=\"2\">", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AsSuplementaresSaemDepoisDoInfNFe()
    {
        var nota = NotasTeste.NFCe();
        var escrita = NotasTeste.Escrever(nota);
        var qrCode = $"https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml?p={escrita.Chave}|3|2";
        Assert.EndsWith($"</infNFe><infNFeSupl><qrCode>{qrCode}</qrCode><urlChave>https://hportalsped.fazenda.mg.gov.br/portalnfce</urlChave></infNFeSupl></NFe>",
            escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OCampoNaoSuportadoPreenchidoViraAviso()
    {
        var nota = NotasTeste.NFe();
        nota.Emitente.NomeFantasia = "EXEMPLO";
        nota.Itens[1].Produto.Cest = "0100100";
        nota.Destinatario!.Email = "";
        var naoSuportados = new[]
        {
            typeof(Emitente).GetProperty(nameof(Emitente.NomeFantasia))!,
            typeof(Produto).GetProperty(nameof(Produto.Cest))!,
            typeof(Destinatario).GetProperty(nameof(Destinatario.Email))!,
            typeof(Emitente).GetProperty(nameof(Emitente.InscricaoMunicipal))!,
        };
        var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFe), naoSuportados);

        Assert.All(escrita.Problemas, p => Assert.Equal(("NFE-NAO-SUPORTADO", Severidade.Aviso), (p.Codigo, p.Severidade)));
        Assert.Equal(["Emitente.NomeFantasia", "Itens[0].Produto.Cest", "Itens[1].Produto.Cest"], escrita.Problemas.Select(p => p.Propriedade));
        Assert.Equal("xFant", escrita.Problemas[0].Tag);
    }
}
