using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A leitura (etapa 2.5, docs/LEITURA.md): a ida e volta contra a escrita (PROJETO.md 13.2), o nfeProc, a nota de outro
// emissor, cada problema da seção 4, a tolerância da seção 3 e o detector com o leiaute 4.00.
public partial class LeitorNFeTestes
{
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";

    private static readonly LeiauteNFe400 Leiaute = new();

    private static NotaLida Ler(string xml) => Leiaute.Ler(xml);

    private static string Exemplo(string nome) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Exemplos", nome));

    // A NF-e de teste escrita e assinada, com a chave.
    private static (string Xml, ChaveAcesso Chave) Assinada(NotaFiscal? nota = null)
    {
        var escrita = NotasTeste.Escrever(nota ?? NotasTeste.NFe());
        return (NotasTeste.Assinar(escrita), escrita.Chave!);
    }

    private static string Processada(string nfe, string chave, string? chaveProtocolo = null)
    {
        var digest = Regex.Match(nfe, "<DigestValue>([^<]+)</DigestValue>").Groups[1].Value;
        return $"<nfeProc xmlns=\"{Ns}\" versao=\"4.00\">{nfe}<protNFe versao=\"4.00\"><infProt Id=\"ID131260000000001\">"
            + $"<tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic><chNFe>{chaveProtocolo ?? chave}</chNFe><dhRecbto>2026-10-20T14:31:05-03:00</dhRecbto>"
            + $"<nProt>131260000000001</nProt><digVal>{digest}</digVal><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo>"
            + "<cMsg>200</cMsg><xMsg>Mensagem da SEFAZ</xMsg></infProt></protNFe></nfeProc>";
    }

    private static string Trocar(string xml, string de, string para)
    {
        var posicao = xml.IndexOf(de, StringComparison.Ordinal);
        Assert.True(posicao >= 0, $"Falta {de} no XML.");
        return string.Concat(xml.AsSpan(0, posicao), para, xml.AsSpan(posicao + de.Length));
    }

    private static Problema Unico(NotaLida lida, string codigo)
    {
        var problemas = lida.Problemas.Where(p => p.Codigo == codigo).ToList();
        Assert.True(problemas.Count == 1, $"{problemas.Count} problemas {codigo}: {string.Join("; ", lida.Problemas)}");
        return problemas[0];
    }

    // A nota escrita, assinada e lida de novo: sem problema, a mesma chave, o objeto igual (salvo o que a escrita completa)
    // e, escrita outra vez, o mesmo XML.
    private static NotaLida AssertIdaVolta(NotaFiscal nota)
    {
        var escrita = NotasTeste.Escrever(nota);
        var assinado = NotasTeste.Assinar(escrita);

        var lida = Ler(assinado);
        // A lib gera o infNFeSupl da NFC-e (etapa 5.1): o modelo lido o traz, e o de teste não o tinha preenchido.
        if (nota.Suplementares is null && lida.Nota?.Suplementares is { } gerado)
            nota.Suplementares = new InformacoesSuplementares { QrCode = gerado.QrCode, UrlChave = gerado.UrlChave };

        Assert.True(lida.Problemas.Count == 0, string.Join(Environment.NewLine, lida.Problemas.Select(p => $"{p} [{p.Caminho}]")));
        Assert.Same(assinado, lida.Xml);
        Assert.Equal(escrita.Chave, lida.Chave);
        Assert.Null(lida.Protocolo);
        var diferencas = ComparacaoModelo.Diferencas(nota, lida.Nota!);
        Assert.True(diferencas.Count == 0, string.Join(Environment.NewLine, diferencas));
        Assert.Equal(escrita.Xml, NotasTeste.Escrever(lida.Nota!).Xml);
        return lida;
    }

    [Fact]
    public void ANFCeMinimaVaiEVolta()
    {
        var lida = AssertIdaVolta(NotasTeste.NFCe());

        Assert.Equal(Documento.NFCe, lida.Nota!.Identificacao.Modelo);
        Assert.Equal(UF.MG, lida.Nota.Identificacao.UF);
        Assert.Equal(TipoEmissao.Normal, lida.Nota.Identificacao.TipoEmissao);
        Assert.Equal(Ambiente.Homologacao, lida.Nota.Identificacao.Ambiente);
    }

    [Fact]
    public void ANFeComOsGruposDoCabecalhoVaiEVolta() => AssertIdaVolta(NotasTeste.NFe());

    [Theory]
    [MemberData(nameof(EscritorNFeTestes.GruposImposto), MemberType = typeof(EscritorNFeTestes))]
    public void CadaGrupoDoImpostoVaiEVolta(string grupo)
    {
        var lida = AssertIdaVolta(EscritorNFeTestes.NotaGrupoImposto(grupo));

        var imposto = lida.Nota!.Itens[0].Imposto;
        Assert.Contains(grupo, new[] { imposto.Icms?.GetType().Name, imposto.Pis?.GetType().Name, imposto.Cofins?.GetType().Name });
    }

    [Fact]
    public void OCstDoGrupoDeUmValorVoltaPreenchido()
    {
        var nota = NotasTeste.NFCe();
        nota.Itens[0].Imposto.Icms = new Icms00 { Origem = OrigemMercadoria.Nacional, ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao, BaseCalculo = 10m, Aliquota = 18m, Valor = 1.8m };

        var lida = AssertIdaVolta(nota);

        Assert.Equal(CstIcms.Tributada, Assert.IsType<Icms00>(lida.Nota!.Itens[0].Imposto.Icms).Cst);
    }

    [Fact]
    public void ONfeProcTrazOProtocolo()
    {
        var (nfe, chave) = Assinada();
        var xml = Processada(nfe, chave.ToString());
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(xml));   // o nfeProc de teste é válido no schema

        var lida = Ler(xml);

        Assert.Empty(lida.Problemas);
        Assert.Equal(chave, lida.Chave);
        var protocolo = Assert.IsType<Protocolo>(lida.Protocolo);
        Assert.Equal("ID131260000000001", protocolo.Id);
        Assert.Equal(Ambiente.Homologacao, protocolo.Ambiente);
        Assert.Equal("MG-TESTE", protocolo.VersaoAplicativo);
        Assert.Equal(chave.ToString(), protocolo.Chave);
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 14, 31, 5, TimeSpan.FromHours(-3)), protocolo.Recebimento);
        Assert.Equal("131260000000001", protocolo.Numero);
        Assert.False(string.IsNullOrEmpty(protocolo.Digest));
        Assert.Equal(100, protocolo.Status);
        Assert.Equal("Autorizado o uso da NF-e", protocolo.Motivo);
        var mensagem = Assert.Single(protocolo.Mensagens);
        Assert.Equal((200, "Mensagem da SEFAZ"), (mensagem.Codigo, mensagem.Texto));
        Assert.Empty(ComparacaoModelo.Diferencas(Ler(nfe).Nota!, lida.Nota!));
    }

    [Fact]
    public void AsMensagensDaSefazSaoUmaPorCMsg()
    {
        var (nfe, chave) = Assinada();
        var xml = Trocar(Processada(nfe, chave.ToString()), "<cMsg>200</cMsg><xMsg>Mensagem da SEFAZ</xMsg>",
            "<cMsg>1</cMsg><xMsg>Um</xMsg><cMsg>2</cMsg><xMsg>Dois</xMsg><xMsg>Sem código</xMsg>");

        var mensagens = Ler(xml).Protocolo!.Mensagens;

        Assert.Equal([(1, "Um"), (2, "Dois"), (null, "Sem código")], mensagens.Select(m => (m.Codigo, m.Texto)));
    }

    // A NF-e de entrada do teste do ErpCore (ErpCore.Api.Testes/NfeDeTeste.cs): indentada, com o xmlns repetido no NFe e
    // uma Signature de exemplo. Os valores são os que o LeitorDeNfe do ErpCore usa (docs/LEITURA.md 6).
    [Fact]
    public void LeANotaDeEntradaDoErpCoreComOQueOLeitorDeleUsa()
    {
        var lida = Ler(Exemplo("erpcore-nfeproc-entrada.xml"));

        Assert.Empty(lida.Problemas);
        Assert.Equal("31261011222333000181550010000001231009740371", lida.Chave?.ToString());
        var nota = lida.Nota!;
        Assert.Equal(Documento.NFe, nota.Identificacao.Modelo);
        Assert.Equal((123, 1), (nota.Identificacao.Numero, nota.Identificacao.Serie));
        Assert.Equal(new DateTimeOffset(2026, 10, 14, 9, 30, 0, TimeSpan.FromHours(-3)), nota.Identificacao.Emissao);
        Assert.Equal(TipoNota.Saida, nota.Identificacao.Tipo);
        Assert.Equal(("11222333000181", "Distribuidora Fornecedor Ltda", "0623079040081"),
            (nota.Emitente.Cnpj, nota.Emitente.Nome, nota.Emitente.InscricaoEstadual));
        Assert.Equal(("34028316000103", "Loja Centro Ltda", null), (nota.Destinatario!.Cnpj, nota.Destinatario.Nome, nota.Destinatario.InscricaoEstadual));
        Assert.Equal(180.00m, nota.Total.Icms.ValorNota);
        Assert.Equal(100, lida.Protocolo!.Status);

        var caneta = nota.Itens[0];
        Assert.Equal(("CAN-001", "Caneta azul", "CX", 10m, 150m, 5m), (caneta.Produto.Codigo, caneta.Produto.Descricao,
            caneta.Produto.UnidadeComercial, caneta.Produto.QuantidadeComercial, caneta.Produto.Valor, caneta.Produto.Frete));
        var icms = Assert.IsType<Icms10>(caneta.Imposto.Icms);
        Assert.Equal((12.30m, 1.20m), (icms.ValorSt, icms.ValorFcpSt));
        Assert.Equal(7.50m, caneta.Imposto.Ipi!.Tributado!.Valor);
        Assert.Equal([("L2026A", 6m, new DateOnly(2026, 1, 10), new DateOnly(2028, 1, 10)), ("L2026B", 4m, new DateOnly(2026, 2, 15), new DateOnly(2028, 2, 15))],
            caneta.Produto.Rastros.Select(r => (r.Lote, r.Quantidade, r.Fabricacao, r.Validade)));

        var lapis = nota.Itens[1];
        Assert.Equal(("LAP-002", 0.50m, null), (lapis.Produto.Codigo, lapis.Produto.Desconto, lapis.Imposto.Ipi));
        Assert.IsType<Icms00>(lapis.Imposto.Icms);
    }

    [Fact]
    public void RecusaOXmlNulo() => Assert.Throws<ArgumentNullException>(() => Ler(null!));
}
