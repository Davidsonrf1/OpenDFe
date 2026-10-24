using System.Globalization;
using System.Xml.Linq;
using OpenDFe.Impressao;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFE de uma NF-e produzida pela lib inteira (etapa 7.3): a nota do modelo, a autorização contra a SEFAZ emulada, o
/// <c>nfeProc</c> lido de volta e impresso (retrato, SVC e o Simplificado Tipo 2, normal e offline), e a NF-e de verdade do
/// ERP Core (<c>tests/Exemplos/erpcore-nfeproc-entrada.xml</c>) impressa sem exceção e com os campos certos.
/// </summary>
public sealed class DanfeEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    private static readonly LeiauteNFe400 Leiaute = new();

    private readonly Emulador _emulador;

    public DanfeEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string Exemplo(string nome) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Exemplos", nome));

    private static IEnumerable<CampoImpressao> Campos(DocumentoImpressao documento) =>
        documento.Elementos.OfType<SecaoImpressao>().SelectMany(s => s.Linhas).SelectMany(l => l.Campos);

    private static string Valor(DocumentoImpressao documento, string rotulo) => Campos(documento).First(c => c.Rotulo == rotulo).Valor;

    private static decimal Numero(string texto) =>
        decimal.Parse(texto.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.'), CultureInfo.InvariantCulture);

    [Fact]
    public async Task ANFeAutorizadaPelaLibEImpressaNoRetrato()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var autorizada = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        Assert.Empty(lida.Problemas);

        var documento = Leiaute.Danfe(lida);

        Assert.Same(PapelImpressao.A4Retrato, documento.Papel);
        Assert.Equal(autorizada.Chave!.ToString(), documento.Elementos.OfType<CodigoBarrasImpressao>().Single().Digitos);
        Assert.Equal(autorizada.Chave.ToString(), Valor(documento, "CHAVE DE ACESSO").Replace(" ", string.Empty, StringComparison.Ordinal));
        Assert.StartsWith(autorizada.Protocolo!.Numero! + " ", Valor(documento, "PROTOCOLO DE AUTORIZAÇÃO DE USO"), StringComparison.Ordinal);
        Assert.Equal("SEM VALOR FISCAL", documento.MarcaAgua);
        Assert.Equal("LOJA EXEMPLO LTDA", documento.Elementos.OfType<TextoImpressao>().Skip(1).First().Texto);

        // Os totais exibidos são os da nota que a lib escreveu e o emulador aceitou.
        Assert.Equal(lida.Nota!.Total.Icms.ValorNota, Numero(Valor(documento, "VALOR TOTAL DA NOTA")));
        Assert.Equal(lida.Nota.Total.Icms.ValorProdutos, Numero(Valor(documento, "VALOR TOTAL DOS PRODUTOS")));
        var tabela = documento.Elementos.OfType<TabelaImpressao>().Single();
        Assert.Equal(lida.Nota.Itens.Count, tabela.Linhas.Count);
        Assert.Contains(documento.Elementos.OfType<SecaoImpressao>(), s => s.Titulo == "CÁLCULO DO ISSQN");
        Assert.Contains(documento.Elementos.OfType<SecaoImpressao>(), s => s.Titulo == "TOTAL DO IBS / CBS / IS");
    }

    [Fact]
    public async Task ANFeEmContingenciaSvcAnSaiComoAEmissaoNormal()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var svc = ApoioEmulado.Criar(_emulador, conexoes, emissao: TipoEmissao.ContingenciaSvcAN);
        var autorizada = await ApoioEmulado.Autorizar(svc, Documento.NFe, 1);

        var documento = Leiaute.Danfe(Leiaute.Ler(autorizada.XmlProcessado!));

        Assert.Equal(TipoEmissao.ContingenciaSvcAN, autorizada.Chave!.TipoEmissao);
        Assert.Single(documento.Elementos.OfType<CodigoBarrasImpressao>());
        Assert.DoesNotContain(documento.Elementos.OfType<TextoImpressao>(), t => t.Texto.Contains("Contingência", StringComparison.Ordinal));
        Assert.StartsWith(autorizada.Protocolo!.Numero!, Valor(documento, "PROTOCOLO DE AUTORIZAÇÃO DE USO"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OSimplificadoTipo2AutorizadoEImpressoComOQrCodeDaNota()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var autorizada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1, ajustar: Tipo2);
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        Assert.Empty(lida.Problemas);

        var documento = Leiaute.Danfe(lida);

        Assert.Same(PapelImpressao.Bobina80, documento.Papel);
        var qrXml = XElement.Parse(autorizada.Xml!).Descendants(Nfe + "qrCode").Single().Value;
        Assert.Equal(qrXml, Assert.Single(documento.Elementos.OfType<QrCodeImpressao>()).Texto);
        Assert.EndsWith("|3|2", qrXml, StringComparison.Ordinal);
        var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Contains("DANFE Simplificado Tipo 2", textos);
        Assert.Contains("Protocolo de autorização: " + autorizada.Protocolo!.Numero, textos);
        Assert.Contains(textos, t => t.Replace(" ", string.Empty, StringComparison.Ordinal) == autorizada.Chave!.ToString());
        Assert.DoesNotContain(textos, t => t.Contains("CONTINGÊNCIA", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OSimplificadoTipo2OfflineImpressoSemProtocoloEDepoisAutorizado()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var normal = ApoioEmulado.Criar(_emulador, conexoes);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, emissao: TipoEmissao.ContingenciaOffline);
        var nota = ApoioEmulado.Nota(Documento.NFCe, 1, DateTimeOffset.UtcNow.AddHours(-2));
        Tipo2(nota);
        var preparada = offline.Preparar(nota);
        Assert.True(preparada.Preparado);

        // Sem protocolo: as duas legendas, a via, e o QR Code da versão 3 que a lib escreveu.
        var antes = Leiaute.Danfe(Leiaute.Ler(preparada.Xml!), new OpcoesDanfe { Via = ViaDanfce.Estabelecimento });
        var textosAntes = antes.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Equal(2, textosAntes.Count(t => t == "EMITIDA EM CONTINGÊNCIA"));
        Assert.Contains("Via do Estabelecimento", textosAntes);
        Assert.DoesNotContain(textosAntes, t => t.StartsWith("Protocolo de autorização", StringComparison.Ordinal));
        Assert.Single(antes.Elementos.OfType<QrCodeImpressao>());

        // Depois de autorizada, com o protocolo, a legenda some.
        var resultado = await normal.ExecutarAsync(new Autorizacao(preparada.Xml!), Ct);
        var autorizada = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(autorizada.Autorizada);
        var depois = Leiaute.Danfe(Leiaute.Ler(autorizada.XmlProcessado!));
        var textosDepois = depois.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.DoesNotContain(textosDepois, t => t.Contains("CONTINGÊNCIA", StringComparison.Ordinal));
        Assert.Contains("Protocolo de autorização: " + autorizada.Protocolo!.Numero, textosDepois);
    }

    // A NFC-e de teste emitida como NF-e com o DANFE Simplificado Tipo 2: o núcleo da NF-e escreve o modelo 55.
    private static void Tipo2(NotaFiscal nota) => nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;

    [Fact]
    public void ANFeDeVerdadeDoErpCoreImpressaSemExcecaoEComOsCamposCertos()
    {
        var xml = Exemplo("erpcore-nfeproc-entrada.xml");
        var lida = Leiaute.Ler(xml);
        Assert.Empty(lida.Problemas);

        var documento = Leiaute.Danfe(lida);

        Assert.Same(PapelImpressao.A4Retrato, documento.Papel);
        Assert.Equal("31261011222333000181550010000001231009740371", lida.Chave!.ToString());
        Assert.Equal(lida.Chave.ToString(), documento.Elementos.OfType<CodigoBarrasImpressao>().Single().Digitos);
        Assert.Equal("131260000012345 14/10/2026 09:30:00", Valor(documento, "PROTOCOLO DE AUTORIZAÇÃO DE USO"));
        Assert.Equal("VENDA DE MERCADORIA", Valor(documento, "NATUREZA DA OPERAÇÃO"));
        Assert.Equal("11.222.333/0001-81", Valor(documento, "CNPJ / CPF"));
        Assert.Equal("0623079040081", Valor(documento, "INSCRIÇÃO ESTADUAL"));
        Assert.Equal("3 - REGIME NORMAL", Valor(documento, "CÓDIGO DO REGIME TRIBUTÁRIO"));
        Assert.Equal("Loja Centro Ltda", Valor(documento, "NOME / RAZÃO SOCIAL"));
        Assert.Equal("180,00", Valor(documento, "VALOR TOTAL DA NOTA"));
        Assert.Equal("154,50", Valor(documento, "VALOR TOTAL DOS PRODUTOS"));
        Assert.Equal("12,30", Valor(documento, "VALOR DO ICMS ST"));
        Assert.Equal("7,50", Valor(documento, "VALOR DO IPI"));
        Assert.Equal("0 - Remetente (CIF)", Valor(documento, "FRETE POR CONTA"));
        Assert.Equal("SEM VALOR FISCAL", documento.MarcaAgua);

        var tabela = documento.Elementos.OfType<TabelaImpressao>().Single();
        Assert.Equal(2, tabela.Linhas.Count);
        var primeira = tabela.Linhas[0];
        Assert.Equal("CAN-001", primeira[0]);
        Assert.StartsWith("Caneta azul\n", primeira[1], StringComparison.Ordinal);
        Assert.Contains("Lote: L2026A Qtd: 6 Fab: 10/01/2026 Val: 10/01/2028", primeira[1], StringComparison.Ordinal);
        Assert.Contains("Lote: L2026B Qtd: 4", primeira[1], StringComparison.Ordinal);
        Assert.Equal("010", primeira[3]);
        Assert.Equal("5102", primeira[4]);
        Assert.Equal("10,00", primeira[6]);
        Assert.Equal("15,00", primeira[7]);
        Assert.Equal("150,00", primeira[9]);
        Assert.Equal("210,00", primeira[11]);
        Assert.Equal("27,00", primeira[12]);
        Assert.Equal("12,30", primeira[13]);
        Assert.Equal("7,50", primeira[14]);
        Assert.Equal("18,00", primeira[15]);
        Assert.Equal("5,00", primeira[16]);
        Assert.Equal("0,50", tabela.Linhas[1][8]);
        Assert.Equal("000", tabela.Linhas[1][3]);
        Assert.Contains("Documento de teste.", Valor(documento, "INFORMAÇÕES COMPLEMENTARES"), StringComparison.Ordinal);
    }
}
