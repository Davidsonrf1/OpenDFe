using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O problema até a propriedade (2.6, docs/MODELO.md 14): o caminho no XML convertido na propriedade do modelo pelo
// [Tag], e o problema do schema da nota e do nfeProc com a propriedade.
public class CaminhoModeloNFeTestes
{
    private static readonly XNamespace Ns = EscritaNFe.Namespace;
    private static readonly XNamespace NsAssinatura = LeituraNFe.NamespaceAssinatura;
    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    // Os caminhos sem propriedade: a chave e a versão do leiaute, o cDV calculado e o nItem, que é a posição.
    private static readonly string[] PassosNulos = ["@Id", "@versao", "cDV", "@nItem"];

    [Theory]
    [InlineData("NFe/infNFe/det[3]/prod/vProd", "Itens[2].Produto.Valor")]
    [InlineData("NFe/infNFe/det/prod/vProd", "Itens[0].Produto.Valor")]
    [InlineData("NFe/infNFe/det[1]/prod[1]/vProd[1]", "Itens[0].Produto.Valor")]
    [InlineData("NFe/infNFe/ide/cUF", "Identificacao.UF")]
    [InlineData("NFe/infNFe/ide", "Identificacao")]
    [InlineData("NFe/infNFe/emit/enderEmit/xLgr", "Emitente.Endereco.Logradouro")]
    [InlineData("NFe/infNFe/dest/enderDest/UF", "Destinatario.Endereco.UF")]
    [InlineData("NFe/infNFe/entrega/xLgr", "Entrega.Logradouro")]
    [InlineData("NFe/infNFe/retirada/xLgr", "Retirada.Logradouro")]
    [InlineData("NFe/infNFe/ide/NFref[2]/refNF/cUF", "Identificacao.Referencias[1].UF")]
    [InlineData("NFe/infNFe/ide/NFref/refNFe", "Identificacao.Referencias[0].Chave")]
    [InlineData("NFe/infNFe/ide/NFref[4]/refCTe", "Identificacao.Referencias[3].Chave")]
    [InlineData("NFe/infNFe/ide/NFref[5]/refECF/nCOO", "Identificacao.Referencias[4].Coo")]
    [InlineData("NFe/infNFe/det[2]/imposto/ICMS/ICMS00/vBC", "Itens[1].Imposto.Icms.BaseCalculo")]
    [InlineData("NFe/infNFe/det[2]/imposto/ICMS/ICMS00/orig", "Itens[1].Imposto.Icms.Origem")]
    [InlineData("NFe/infNFe/det[2]/imposto/ICMS/ICMS00", "Itens[1].Imposto.Icms")]
    [InlineData("NFe/infNFe/det/imposto/PIS/PISAliq/CST", "Itens[0].Imposto.Pis.Cst")]
    [InlineData("NFe/infNFe/det/prod/NVE[2]", "Itens[0].Produto.Nve[1]")]
    [InlineData("NFe/infNFe/transp/vol[2]/lacres[3]/nLacre", "Transporte.Volumes[1].Lacres[2].Numero")]
    [InlineData("NFe/infNFe/total/ICMSTot/vNF", "Total.Icms.ValorNota")]
    [InlineData("NFe/infNFeSupl/qrCode", "Suplementares.QrCode")]
    [InlineData("nfeProc/NFe/infNFe/det[3]/prod/vProd", "Itens[2].Produto.Valor")]
    [InlineData("nfeProc/protNFe/infProt/cStat", "Protocolo.Status")]
    [InlineData("nfeProc/protNFe/infProt/@Id", "Protocolo.Id")]
    [InlineData("nfeProc/protNFe/infProt/cMsg", "Protocolo.Mensagens[0].Codigo")]
    [InlineData("nfeProc/protNFe/infProt/xMsg[2]", "Protocolo.Mensagens[1].Texto")]
    [InlineData("nfeProc/protNFe/infProt", "Protocolo")]
    [InlineData("nfeProc/protNFe", "Protocolo")]
    public void ConverteOCaminho(string caminho, string propriedade) =>
        Assert.Equal(propriedade, CaminhoModeloNFe.Propriedade(caminho));

    [Theory]
    [InlineData("NFe")]
    [InlineData("NFe/infNFe")]
    [InlineData("NFe/infNFe/@Id")]
    [InlineData("NFe/infNFe/@versao")]
    [InlineData("NFe/infNFe/ide/cDV")]
    [InlineData("NFe/infNFe/det[2]/@nItem")]
    [InlineData("NFe/Signature/SignatureValue")]
    [InlineData("NFe/Signature")]
    [InlineData("NFe/infNFe/det/prod/xYz")]
    [InlineData("NFe/infNFe/det/prod/@xYz")]
    [InlineData("NFe/infNFe/ide[2]/cUF")]
    [InlineData("NFe/infNFe/det[0]/prod")]
    [InlineData("NFe/infNFe/det[a]/prod")]
    [InlineData("NFe/infNFe/det[/prod")]
    [InlineData("NFe/infNFe/det/@prod[1]")]
    [InlineData("NFe/infNFe/det/imposto/ICMS/ICMS00[2]/vBC")]
    [InlineData("NFe/infNFe/det/imposto/ICMS/PISAliq/CST")]
    [InlineData("NFe/infNFe/ide/NFref/refNFe/x")]
    [InlineData("NFe/infNFe//ide")]
    [InlineData("")]
    [InlineData("enviNFe/NFe/infNFe/ide/cUF")]
    [InlineData("infNFe/ide/cUF")]
    [InlineData("nfeProc")]
    [InlineData("nfeProc/@versao")]
    [InlineData("nfeProc/protNFe/@versao")]
    [InlineData("nfeProc/protNFe/Signature")]
    [InlineData("nfeProc/NFe/Signature")]
    [InlineData("nfeProc/infProt/cStat")]
    public void OCaminhoSemPropriedadeDaNulo(string caminho) => Assert.Null(CaminhoModeloNFe.Propriedade(caminho));

    // A cobertura: cada elemento e atributo das notas preenchidas por reflexão (uma rodada por subclasse de escolha, com
    // um segundo item), com o caminho no estilo do schema (índice só no que se repete) e no da leitura (índice em todo
    // passo), convertido e avaliado na própria nota: o campo tem o valor e a tag do elemento. Toda propriedade com [Tag]
    // do modelo é alcançada.
    [Fact]
    public void ConverteOCaminhoDeTodoCampoDoModelo()
    {
        var vistas = new HashSet<(Type, string)>();
        foreach (var (nota, raiz) in NotasCobertura())
        {
            foreach (var elemento in raiz.DescendantsAndSelf())
            {
                Conferir(nota, elemento, Caminho(elemento, todos: false), elemento.Name.LocalName, elemento.HasElements, vistas);
                Conferir(nota, elemento, Caminho(elemento, todos: true), elemento.Name.LocalName, elemento.HasElements, vistas);
                foreach (var atributo in elemento.Attributes().Where(a => !a.IsNamespaceDeclaration))
                    Conferir(nota, elemento, Caminho(elemento, todos: false) + "/@" + atributo.Name.LocalName, atributo.Name.LocalName, false, vistas);
            }
        }

        var faltam = Modelo.GetTypes()
            .Where(t => t.IsClass && t.IsPublic && t != typeof(Protocolo) && t != typeof(MensagemSefaz))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(p => p.GetCustomAttribute<TagAttribute>() is not null)
            .Where(p => !vistas.Contains((p.DeclaringType!, p.Name)))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();
        Assert.True(faltam.Count == 0, "Campos do modelo que a conversão não alcança: " + string.Join(", ", faltam));
    }

    // O protocolo: cada campo do infProt (o Id como atributo) e a sequência cMsg + xMsg, repetida.
    [Fact]
    public void ConverteOCaminhoDeTodoCampoDoProtocolo()
    {
        foreach (var propriedade in typeof(Protocolo).GetProperties().Where(p => p.GetCustomAttribute<TagAttribute>() is not null))
        {
            var tag = propriedade.GetCustomAttribute<TagAttribute>()!.Tag;
            var passo = tag == "Id" ? "@Id" : tag;
            Assert.Equal($"Protocolo.{propriedade.Name}", CaminhoModeloNFe.Propriedade($"nfeProc/protNFe/infProt/{passo}"));
        }
        foreach (var propriedade in typeof(MensagemSefaz).GetProperties())
        {
            var tag = propriedade.GetCustomAttribute<TagAttribute>()!.Tag;
            Assert.Equal($"Protocolo.Mensagens[2].{propriedade.Name}", CaminhoModeloNFe.Propriedade($"nfeProc/protNFe/infProt/{tag}[3]"));
        }
    }

    // O schema da nota: os problemas da validação de verdade, nas notas de teste estragadas depois de assinadas.

    [Fact]
    public void OValorDoTerceiroItem()
    {
        var problema = Estragar(NotaTresItens(), r => Det(r, 3).Element(Ns + "prod")!.Element(Ns + "vProd")!.Value = "12,5", "vProd");

        Assert.Equal("NFe/infNFe/det[3]/prod/vProd", problema.Caminho);
        Assert.Equal("Itens[2].Produto.Valor", problema.Propriedade);
        Assert.Equal("225", problema.Codigo);
    }

    [Fact]
    public void OCampoDoGrupoDoIcms()
    {
        var problema = Estragar(EscritorNFeTestes.NotaGrupoImposto(nameof(Icms00)),
            r => Det(r, 1).Descendants(Ns + "ICMS00").Single().Element(Ns + "vBC")!.Value = "abc", "vBC");

        Assert.Equal("NFe/infNFe/det/imposto/ICMS/ICMS00/vBC", problema.Caminho);
        Assert.Equal("Itens[0].Imposto.Icms.BaseCalculo", problema.Propriedade);
    }

    [Fact]
    public void OCampoHerdadoDaBaseDoIcms()
    {
        var problema = Estragar(EscritorNFeTestes.NotaGrupoImposto(nameof(Icms00)),
            r => Det(r, 1).Descendants(Ns + "orig").Single().Value = "9", "orig");

        Assert.Equal("Itens[0].Imposto.Icms.Origem", problema.Propriedade);
    }

    [Fact]
    public void OCampoDaNotaReferenciada()
    {
        var problemas = Validar(NotasTeste.NFe(), r =>
        {
            var referencias = r.Descendants(Ns + "NFref").ToList();
            referencias[0].Element(Ns + "refNFe")!.Value = "123";
            referencias[1].Element(Ns + "refNF")!.Element(Ns + "cUF")!.Value = "99";
        });

        Assert.Equal("Identificacao.Referencias[0].Chave", Assert.Single(problemas, p => p.Tag == "refNFe").Propriedade);
        Assert.Equal("Identificacao.Referencias[1].UF", Assert.Single(problemas, p => p.Tag == "cUF").Propriedade);
        Assert.Equal("NFe/infNFe/ide/NFref[2]/refNF/cUF", Assert.Single(problemas, p => p.Tag == "cUF").Caminho);
    }

    [Fact]
    public void AUFDoDestinatario()
    {
        var problema = Estragar(NotasTeste.NFe(), r => r.Descendants(Ns + "enderDest").Single().Element(Ns + "UF")!.Value = "XX", "UF");

        Assert.Equal("Destinatario.Endereco.UF", problema.Propriedade);
    }

    [Fact]
    public void OCampoDaClasseCompartilhadaApontaOLugar()
    {
        var problema = Estragar(NotasTeste.NFe(), r => r.Descendants(Ns + "entrega").Single().Element(Ns + "xLgr")!.Value = "", "xLgr");

        Assert.Equal("Entrega.Logradouro", problema.Propriedade);
    }

    // O obrigatório que falta no fim do grupo: o caminho é o que ele teria, e a propriedade a dele.
    [Fact]
    public void OObrigatorioAusente()
    {
        var problemas = Validar(NotaTresItens(), r => Det(r, 2).Element(Ns + "prod")!.Element(Ns + "indTot")!.Remove());

        var problema = Assert.Single(problemas);
        Assert.Equal("NFe/infNFe/det[2]/prod/indTot", problema.Caminho);
        Assert.Equal("Itens[1].Produto." + nameof(Produto.Totalizacao), problema.Propriedade);
    }

    // No meio do grupo, o validador do .NET aponta o elemento que veio no lugar do que falta (SchemaXml.Validar): a
    // propriedade é a do caminho do problema.
    [Fact]
    public void OObrigatorioAusenteNoMeioApontaOQueVeioNoLugar()
    {
        var problemas = Validar(NotaTresItens(), r => Det(r, 2).Element(Ns + "prod")!.Element(Ns + "xProd")!.Remove());

        var problema = Assert.Single(problemas);
        Assert.Equal("NFe/infNFe/det[2]/prod/NCM", problema.Caminho);
        Assert.Equal("Itens[1].Produto.Ncm", problema.Propriedade);
    }

    [Fact]
    public void OElementoInesperadoNaoTemPropriedade()
    {
        var problema = Assert.Single(Validar(NotasTeste.NFCe(),
            r => Det(r, 1).Element(Ns + "prod")!.Element(Ns + "xProd")!.AddAfterSelf(new XElement(Ns + "xYz", "1"))));

        Assert.Equal("NFe/infNFe/det/prod/xYz", problema.Caminho);
        Assert.Null(problema.Propriedade);
    }

    [Fact]
    public void ONumeroDoItemNaoTemPropriedade()
    {
        var problema = Estragar(NotaTresItens(), r => Det(r, 2).Attribute("nItem")!.Value = "0", "nItem");

        Assert.Equal("NFe/infNFe/det[2]/@nItem", problema.Caminho);
        Assert.Null(problema.Propriedade);
    }

    [Fact]
    public void AAssinaturaNaoTemPropriedade()
    {
        var problema = Estragar(NotasTeste.NFCe(), r => r.Descendants(NsAssinatura + "SignatureValue").Single().Value = "!!", "SignatureValue");

        Assert.Equal("NFe/Signature/SignatureValue", problema.Caminho);
        Assert.Null(problema.Propriedade);
    }

    // O schema do nfeProc: a nota igual e o protocolo como Protocolo, como a leitura.
    [Fact]
    public void ONfeProcApontaANotaEOProtocolo()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe());
        var nfe = NotasTeste.Assinar(escrita);
        var digest = Regex.Match(nfe, "<DigestValue>([^<]+)</DigestValue>").Groups[1].Value;
        var xml = $"<nfeProc xmlns=\"{Ns}\" versao=\"4.00\">{nfe}<protNFe versao=\"4.00\"><infProt Id=\"ID131260000000001\">"
            + $"<tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic><chNFe>{escrita.Chave}</chNFe><dhRecbto>2026-10-20T14:31:05-03:00</dhRecbto>"
            + $"<nProt>131260000000001</nProt><digVal>{digest}</digVal><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo>"
            + "<cMsg>200</cMsg><xMsg>Mensagem da SEFAZ</xMsg></infProt></protNFe></nfeProc>";
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(xml));

        var estragado = XElement.Parse(xml, LoadOptions.PreserveWhitespace);
        estragado.Descendants(Ns + "vProd").First().Value = "12,5";
        estragado.Descendants(Ns + "cStat").Single().Value = "abc";
        estragado.Descendants(Ns + "cMsg").Single().Value = "x";
        var problemas = SchemasNFe400.NotaProcessada.Validar(estragado.ToString(SaveOptions.DisableFormatting));

        Assert.Equal("Itens[0].Produto.Valor", Assert.Single(problemas, p => p.Tag == "vProd").Propriedade);
        Assert.Equal("Protocolo.Status", Assert.Single(problemas, p => p.Tag == "cStat").Propriedade);
        Assert.Equal("Protocolo.Mensagens[0].Codigo", Assert.Single(problemas, p => p.Tag == "cMsg").Propriedade);
    }

    // As outras mensagens não têm modelo do [Tag] e ficam sem a conversão (o lote, na 4.4: docs/MODELO.md 14.3, decisão 4).
    [Fact]
    public void OLoteAindaNaoApontaAPropriedade()
    {
        var nfe = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()));
        var lote = $"<enviNFe xmlns=\"{Ns}\" versao=\"4.00\"><idLote>1</idLote><indSinc>1</indSinc>{nfe}</enviNFe>";
        Assert.Empty(SchemasNFe400.Lote.Validar(lote));

        var problema = Assert.Single(SchemasNFe400.Lote.Validar(new Regex("<vProd>[^<]+</vProd>").Replace(lote, "<vProd>12,5</vProd>", 1)));

        Assert.Equal("enviNFe/NFe/infNFe/det/prod/vProd", problema.Caminho);
        Assert.Null(problema.Propriedade);
    }

    // O teste cruzado (docs/MODELO.md 14.3, decisão 5): a leitura e a escrita apontam a propriedade pelo caminho que elas
    // mesmas abrem; a conversão do caminho delas dá a mesma.

    [Fact]
    public void ALeituraEAConversaoApontamAMesmaPropriedade()
    {
        var conferidos = 0;
        foreach (var (_, raiz) in NotasCobertura())
        {
            // todo valor de elemento ilegível: os que não são texto dão NFE-VALOR, com a propriedade
            foreach (var folha in raiz.Descendants().Where(e => !e.HasElements && e.Name.LocalName != "Signature").ToList())
                folha.Value = "#";

            var lida = new LeiauteNFe400().Ler(raiz.ToString(SaveOptions.DisableFormatting));

            foreach (var problema in lida.Problemas.Where(p => p.Propriedade is not null))
            {
                Assert.Equal(problema.Propriedade, CaminhoModeloNFe.Propriedade(problema.Caminho!));
                conferidos++;
            }
        }
        Assert.True(conferidos > 1000, $"Só {conferidos} problemas da leitura com a propriedade.");
    }

    [Fact]
    public void AEscritaEAConversaoApontamAMesmaPropriedade()
    {
        var conferidos = 0;
        foreach (var (nota, _) in NotasCobertura())
        {
            // todo texto com um caractere que o XML não aceita: NFE-CARACTERE em cada um, com a propriedade
            Envenenar(nota);
            var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFe));

            // só os de campo: o da escolha (NFE-EXCLUSIVO) não tem tag, aponta o grupo no caminho e a alternativa na propriedade
            foreach (var problema in escrita.Problemas.Where(p => p.Tag is not null && p.Propriedade is not null && p.Caminho is not null))
            {
                Assert.Equal(problema.Propriedade, CaminhoModeloNFe.Propriedade(problema.Caminho!));
                conferidos++;
            }
        }
        Assert.True(conferidos > 1000, $"Só {conferidos} problemas da escrita com a propriedade.");
    }

    // As notas da cobertura da escrita (CoberturaEscritaTestes.Preencher), com um segundo item, e o XML delas.
    private static IEnumerable<(NotaFiscal Nota, XElement Raiz)> NotasCobertura()
    {
        var rodadas = Modelo.GetTypes().Where(t => t.IsAbstract && t.IsClass && t.Namespace == "OpenDFe.NFe")
            .Max(b => Modelo.GetTypes().Count(t => t.IsSubclassOf(b) && !t.IsAbstract));
        for (var rodada = 0; rodada < rodadas; rodada++)
        {
            var nota = (NotaFiscal)CoberturaEscritaTestes.Preencher(typeof(NotaFiscal), rodada);
            nota.Itens.Add((Item)CoberturaEscritaTestes.Preencher(typeof(Item), rodada + 1));
            var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFe, TipoEmissao.ContingenciaSvcAN,
                new Contingencia(NotasTeste.Emissao, "SEFAZ FORA DO AR DESDE AS 14H")));
            yield return (nota, XElement.Parse(escrita.Xml));
        }
    }

    // O caminho de um elemento como o schema o escreve (índice só no que se repete) ou com o índice em todo passo.
    private static string Caminho(XElement elemento, bool todos)
    {
        var passos = new List<string>();
        for (var e = elemento; e is not null; e = e.Parent)
        {
            var irmaos = e.Parent?.Elements(e.Name).ToList() ?? [e];
            passos.Add(todos || irmaos.Count > 1
                ? string.Create(CultureInfo.InvariantCulture, $"{e.Name.LocalName}[{irmaos.IndexOf(e) + 1}]")
                : e.Name.LocalName);
        }
        passos.Reverse();
        return string.Join('/', passos);
    }

    private static void Conferir(NotaFiscal nota, XElement elemento, string caminho, string nome, bool grupo, HashSet<(Type, string)> vistas)
    {
        var propriedade = CaminhoModeloNFe.Propriedade(caminho);
        if (PassosNulos.Contains(caminho.Split('/')[^1].Split('[')[0]) || elemento.Name.LocalName is "NFe" or "infNFe")
        {
            if (!grupo || elemento.Name.LocalName is "NFe" or "infNFe")
                Assert.True(propriedade is null, $"{caminho}: {propriedade}, e não nulo.");
            return;
        }
        Assert.True(propriedade is not null, $"{caminho}: sem propriedade.");

        var (valor, ultima) = Avaliar(nota, propriedade!);
        Assert.True(valor is not null, $"{caminho} → {propriedade}: sem valor na nota.");
        vistas.Add((ultima.DeclaringType!, ultima.Name));
        if (!grupo)
            Assert.True(ultima.GetCustomAttribute<TagAttribute>()!.Tag == nome, $"{caminho} → {propriedade}: a tag não é {nome}.");
    }

    // O valor da propriedade na nota, passo a passo (o índice na lista), e a última propriedade do caminho.
    private static (object? Valor, PropertyInfo Ultima) Avaliar(object raiz, string caminho)
    {
        object? atual = raiz;
        PropertyInfo? ultima = null;
        foreach (var parte in caminho.Split('.'))
        {
            var abre = parte.IndexOf('[', StringComparison.Ordinal);
            var nome = abre < 0 ? parte : parte[..abre];
            ultima = atual!.GetType().GetProperty(nome, BindingFlags.Public | BindingFlags.Instance);
            Assert.True(ultima is not null, $"{caminho}: {atual.GetType().Name} não tem {nome}.");
            atual = ultima!.GetValue(atual);
            if (abre >= 0)
            {
                var indice = int.Parse(parte[(abre + 1)..^1], CultureInfo.InvariantCulture);
                var lista = (IList)atual!;
                Assert.True(indice < lista.Count, $"{caminho}: {nome} tem {lista.Count} itens.");
                atual = lista[indice];
            }
        }
        return (atual, ultima!);
    }

    // Todo texto do modelo (também nas listas de texto) com um caractere de controle.
    private static void Envenenar(object grupo)
    {
        foreach (var propriedade in grupo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propriedade.GetCustomAttribute<TagAttribute>() is null)
                continue;
            var valor = propriedade.GetValue(grupo);
            switch (valor)
            {
                case string:
                    propriedade.SetValue(grupo, "A\u0001");
                    break;
                case IList<string> textos:
                    for (var i = 0; i < textos.Count; i++)
                        textos[i] = "A\u0001";
                    break;
                case IList lista:
                    foreach (var item in lista)
                        Envenenar(item!);
                    break;
                case not null when valor.GetType().Assembly == Modelo && valor.GetType().IsClass:
                    Envenenar(valor);
                    break;
            }
        }
    }

    private static NotaFiscal NotaTresItens()
    {
        var nota = NotasTeste.NFCe();
        while (nota.Itens.Count < 3)
            nota.Itens.Add(NotasTeste.ItemSimples());
        return nota;
    }

    private static XElement Det(XElement raiz, int numero) =>
        raiz.Descendants(Ns + "det").Single(d => d.Attribute("nItem")!.Value == numero.ToString(CultureInfo.InvariantCulture));

    private static IReadOnlyList<Problema> Validar(NotaFiscal nota, Action<XElement> estrago)
    {
        var raiz = XElement.Parse(NotasTeste.Assinar(NotasTeste.Escrever(nota)), LoadOptions.PreserveWhitespace);
        estrago(raiz);
        return SchemasNFe400.Nota.Validar(raiz.ToString(SaveOptions.DisableFormatting));
    }

    private static Problema Estragar(NotaFiscal nota, Action<XElement> estrago, string tag)
    {
        var problemas = Validar(nota, estrago);
        Assert.True(problemas.Count(p => p.Tag == tag) == 1, string.Join("; ", problemas.Select(p => $"{p} [{p.Caminho}]")));
        return problemas.Single(p => p.Tag == tag);
    }
}
