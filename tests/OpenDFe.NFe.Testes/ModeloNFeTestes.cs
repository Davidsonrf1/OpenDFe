using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O modelo das etapas 2.2 e 2.3 (docs/MODELO.md) contra o XSD do pacote embutido no leiaute: cada [Tag] existe no
// lugar certo, todo campo do XSD tem propriedade (fora do que não é do modelo), a ocorrência do XSD bate com o grupo
// inicializado ou nulo, o Exterior está onde a UF aceita EX e as enumerações têm os valores do XSD. E as regras de
// forma (MODELO.md 3 e 11.9).
public class ModeloNFeTestes
{
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";

    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    private static readonly Lazy<XmlSchemaSet> Schemas = new(Compilar);

    // As enumerações cujo campo o XSD declara só pelo padrão ([0-9]{2}): a lista vem de tabela do Portal Nacional.
    private static readonly string[] EnumeracoesTabela = [nameof(MeioPagamento), nameof(BandeiraCartao)];

    // Os valores do XML que não são número (MODELO.md, decisões 8 e 31).
    private static readonly Dictionary<Type, Dictionary<int, string>> NaoNumericos = new()
    {
        [typeof(ModeloCupomFiscal)] = new() { [1] = "2B", [2] = "2C", [3] = "2D" },
        [typeof(IndicadorEscala)] = new() { [1] = "S", [2] = "N" },
        [typeof(CondicaoVin)] = new() { [1] = "R", [2] = "N" },
    };

    // As classes compartilhadas, com o [Tag] só com a tag nos campos (MODELO.md, decisões 12 e 29).
    private static readonly Type[] Compartilhadas =
    [
        typeof(CreditoPresumidoIbsCbs), typeof(DevolucaoTributo), typeof(Diferimento), typeof(EstornoCredito), typeof(Local),
        typeof(MonofasiaCbsBiocombustivel), typeof(MonofasiaCbsRetido), typeof(MonofasiaIbsBiocombustivel),
        typeof(MonofasiaIbsRetido), typeof(Observacao), typeof(ReducaoAliquota), typeof(Veiculo),
    ];

    // As alternativas de uma escolha que são propriedades exclusivas de um pai e repetem o identificador do MOC
    // (MODELO.md, decisão 30); as subclasses de uma base fechada entram sem lista.
    private static readonly Type[][] AlternativasExclusivas = [[typeof(IpiTributado), typeof(IpiNaoTributado)]];

    // A repetição dentro de um grupo que a NT 2023.001 v1.60 traz no ICMS53 (MODELO.md, decisão 30).
    private static readonly string[] RepetidosIcms53 = ["N41a", "N42"];

    private sealed record Filho(string Nome, XmlSchemaType? Tipo, bool Atributo, bool Opcional, bool Repete);

    private sealed class Conferencia
    {
        public List<string> Falhas { get; } = [];

        public HashSet<Type> Visitadas { get; } = [];

        public List<string> EnumeracaoAusente { get; } = [];

        // Os valores do XSD de cada enumeração, somados em todos os lugares em que ela aparece.
        public Dictionary<Type, HashSet<string>> ValoresXsd { get; } = [];
    }

    // O leiaute embute os XSDs; o teste os copia para uma pasta e os compila pelo XmlSchemaSet, sem o SchemaXml
    // (que não expõe o conjunto), para percorrer os tipos.
    private static XmlSchemaSet Compilar()
    {
        var pasta = Directory.CreateTempSubdirectory("OpenDFe.Modelo.");
        var assembly = typeof(LeiauteNFe400).Assembly;
        foreach (var nome in assembly.GetManifestResourceNames())
        {
            using var origem = assembly.GetManifestResourceStream(nome)!;
            using var destino = File.Create(Path.Combine(pasta.FullName, Path.GetFileName(nome.Replace('\\', '/'))));
            origem.CopyTo(destino);
        }
        var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemas.Add(Ns, Path.Combine(pasta.FullName, "nfe_v4.00.xsd"));
        schemas.Compile();
        return schemas;
    }

    private static XmlSchemaComplexType TipoNFe() =>
        (XmlSchemaComplexType)((XmlSchemaElement)Schemas.Value.GlobalElements[new XmlQualifiedName("NFe", Ns)]!).ElementSchemaType!;

    private static Dictionary<string, Filho> Filhos(XmlSchemaType? tipo)
    {
        var filhos = new Dictionary<string, Filho>();
        if (tipo is not XmlSchemaComplexType complexo)
            return filhos;
        foreach (XmlSchemaAttribute atributo in complexo.AttributeUses.Values)
            filhos[atributo.QualifiedName.Name] = new(atributo.QualifiedName.Name, atributo.AttributeSchemaType, true, atributo.Use != XmlSchemaUse.Required, false);
        Percorrer(complexo.ContentTypeParticle, false, filhos);
        return filhos;
    }

    private static void Percorrer(XmlSchemaParticle? particula, bool opcional, Dictionary<string, Filho> filhos)
    {
        switch (particula)
        {
            case XmlSchemaElement elemento:
                var nome = elemento.QualifiedName.Name;
                if (filhos.TryGetValue(nome, out var repetido))
                {
                    // O IPI está nos dois ramos da escolha do imposto (ICMS ou ISSQN): o mesmo elemento, opcional.
                    Assert.True(repetido.Tipo == elemento.ElementSchemaType, $"A tag {nome} aparece duas vezes no mesmo grupo do XSD, com tipos diferentes.");
                    filhos[nome] = repetido with { Opcional = true };
                    break;
                }
                filhos[nome] = new(nome, elemento.ElementSchemaType, false, opcional || elemento.MinOccurs == 0, elemento.MaxOccurs > 1);
                break;
            case XmlSchemaSequence sequencia:
                foreach (XmlSchemaParticle item in sequencia.Items)
                    Percorrer(item, opcional || sequencia.MinOccurs == 0, filhos);
                break;
            case XmlSchemaChoice escolha:
                foreach (XmlSchemaParticle item in escolha.Items)
                    Percorrer(item, opcional || escolha.MinOccurs == 0 || escolha.Items.Count > 1, filhos);
                break;
        }
    }

    private static bool EClasseModelo(Type tipo) => tipo.IsClass && tipo.Assembly == Modelo;

    private static Type? ItemLista(Type tipo) =>
        tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(IList<>) ? tipo.GetGenericArguments()[0] : null;

    private static PropertyInfo[] Propriedades(Type classe) =>
        classe.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    // Com as herdadas da base fechada (a Origem do ICMS, o CST do PIS e da COFINS).
    private static PropertyInfo[] PropriedadesTodas(Type classe) =>
        classe.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    // Confere a classe contra os filhos do grupo do XSD; devolve as tags que ela mapeou.
    private static HashSet<string> Conferir(Type classe, Dictionary<string, Filho> filhos, string caminho, Conferencia conferencia)
    {
        conferencia.Visitadas.Add(classe);
        var usadas = new HashSet<string>();
        var instancia = classe.IsAbstract ? null : Activator.CreateInstance(classe);
        foreach (var propriedade in PropriedadesTodas(classe))
        {
            var tag = propriedade.GetCustomAttribute<TagAttribute>();
            if (tag is null)
                continue;
            var aqui = $"{caminho}/{tag.Tag} ({classe.Name}.{propriedade.Name})";
            if (!filhos.TryGetValue(tag.Tag, out var filho))
            {
                conferencia.Falhas.Add($"{aqui}: a tag não existe no XSD.");
                continue;
            }
            usadas.Add(tag.Tag);

            var item = ItemLista(propriedade.PropertyType);
            var tipo = item ?? Nullable.GetUnderlyingType(propriedade.PropertyType) ?? propriedade.PropertyType;
            if ((item is not null) != filho.Repete)
                conferencia.Falhas.Add($"{aqui}: lista no modelo e no XSD não batem (repete no XSD: {filho.Repete}).");

            if (EClasseModelo(tipo))
            {
                if (item is null && instancia is not null)
                {
                    var inicializado = propriedade.GetValue(instancia) is not null;
                    if (inicializado == filho.Opcional)
                        conferencia.Falhas.Add($"{aqui}: grupo {(filho.Opcional ? "opcional" : "obrigatório")} no XSD e {(inicializado ? "inicializado" : "nulo")} no modelo.");
                }
                ConferirGrupo(tipo, filho, aqui, conferencia);
            }
            else
            {
                ConferirValor(classe, propriedade, tipo, filho, aqui, conferencia);
            }
        }
        return usadas;
    }

    private static void ConferirGrupo(Type tipo, Filho filho, string caminho, Conferencia conferencia)
    {
        var filhos = Filhos(filho.Tipo);
        var usadas = new HashSet<string>();
        if (tipo.IsAbstract)
        {
            conferencia.Visitadas.Add(tipo);
            foreach (var subclasse in Modelo.GetTypes().Where(t => t.BaseType == tipo))
            {
                var tagClasse = subclasse.GetCustomAttribute<TagAttribute>();
                if (tagClasse is null)
                {
                    usadas.UnionWith(Conferir(subclasse, filhos, caminho, conferencia));
                }
                else if (!filhos.TryGetValue(tagClasse.Tag, out var escolha))
                {
                    conferencia.Falhas.Add($"{caminho}/{tagClasse.Tag} ({subclasse.Name}): a tag da classe não existe no XSD.");
                }
                else
                {
                    usadas.Add(tagClasse.Tag);
                    Sobras(Filhos(escolha.Tipo), Conferir(subclasse, Filhos(escolha.Tipo), $"{caminho}/{tagClasse.Tag}", conferencia), [], $"{caminho}/{tagClasse.Tag}", conferencia);
                }
            }
        }
        else
        {
            usadas = Conferir(tipo, filhos, caminho, conferencia);
        }
        // O cDV é calculado (decisão 6); o nItem é a posição na lista (decisão 21).
        string[] fora = tipo == typeof(Identificacao) ? ["cDV"] : tipo == typeof(Item) ? ["nItem"] : [];
        Sobras(filhos, usadas, fora, caminho, conferencia);
    }

    private static void Sobras(Dictionary<string, Filho> filhos, HashSet<string> usadas, string[] fora, string caminho, Conferencia conferencia)
    {
        foreach (var nome in filhos.Keys.Except(usadas).Except(fora))
            conferencia.Falhas.Add($"{caminho}/{nome}: o campo do XSD não tem propriedade no modelo.");
    }

    private static void ConferirValor(Type classe, PropertyInfo propriedade, Type tipo, Filho filho, string caminho, Conferencia conferencia)
    {
        if (filho.Tipo is XmlSchemaComplexType { ContentType: not XmlSchemaContentType.TextOnly })
        {
            conferencia.Falhas.Add($"{caminho}: é grupo no XSD e valor simples no modelo.");
            return;
        }
        var enumeracao = Enumeracao(filho.Tipo);

        // UF com EX: o Exterior ao lado (MODELO.md, decisão 9).
        if (tipo == typeof(UF))
        {
            var aceitaExterior = enumeracao.Contains("EX");
            var temExterior = classe.GetProperty("Exterior") is not null;
            if (aceitaExterior != temExterior)
                conferencia.Falhas.Add($"{caminho}: a UF {(aceitaExterior ? "aceita" : "não aceita")} EX no XSD e a classe {(temExterior ? "tem" : "não tem")} Exterior.");
        }

        if (!tipo.IsEnum || tipo.Assembly != Modelo)
            return;
        if (enumeracao.Count == 0)
        {
            conferencia.EnumeracaoAusente.Add(tipo.Name);
            return;
        }
        // Uma enumeração serve a vários grupos (o CST, o motivo da desoneração: decisão 24): cada lugar aceita uma
        // parte, que precisa estar nela; o que ela tem a mais que a soma dos lugares é conferido no fim.
        var xsd = enumeracao.Select(v => NaoNumericos.ContainsKey(tipo) ? v : int.Parse(v, System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList();
        var faltam = xsd.Except(ValoresEnumeracao(tipo)).ToList();
        if (faltam.Count > 0)
            conferencia.Falhas.Add($"{caminho}: {tipo.Name} não tem os valores {string.Join(",", faltam)} do XSD.");
        if (!conferencia.ValoresXsd.TryGetValue(tipo, out var soma))
            conferencia.ValoresXsd[tipo] = soma = [];
        soma.UnionWith(xsd);
    }

    private static IEnumerable<string> ValoresEnumeracao(Type tipo) =>
        Enum.GetValues(tipo).Cast<object>().Select(Convert.ToInt32)
            .Select(v => NaoNumericos.TryGetValue(tipo, out var texto) ? texto[v] : v.ToString(System.Globalization.CultureInfo.InvariantCulture));

    // Os valores enumerados do tipo simples, subindo pelos tipos base até achar.
    private static HashSet<string> Enumeracao(XmlSchemaType? tipo)
    {
        for (var atual = tipo; atual is not null; atual = atual.BaseXmlSchemaType)
        {
            if (atual is XmlSchemaSimpleType { Content: XmlSchemaSimpleTypeRestriction restricao })
            {
                var valores = restricao.Facets.OfType<XmlSchemaEnumerationFacet>().Select(f => f.Value!).ToHashSet();
                if (valores.Count > 0)
                    return valores;
            }
        }
        return [];
    }

    private static Conferencia ConferirNota()
    {
        var conferencia = new Conferencia();
        var nfe = TipoNFe();
        var filhosNFe = Filhos(nfe);
        var infNFe = filhosNFe["infNFe"];
        // A raiz achatada (decisão 2): o NFe e o infNFe juntos; o Id, a versao e a Signature ficam fora.
        var filhos = Filhos(infNFe.Tipo);
        foreach (var (nome, filho) in filhosNFe)
            filhos.Add(nome, filho);
        var usadas = Conferir(typeof(NotaFiscal), filhos, "NFe", conferencia);
        Sobras(filhos, usadas, ["infNFe", "Id", "versao", "Signature"], "NFe", conferencia);
        foreach (var (tipo, soma) in conferencia.ValoresXsd)
        {
            var sobram = ValoresEnumeracao(tipo).Except(soma).ToList();
            if (sobram.Count > 0)
                conferencia.Falhas.Add($"{tipo.Name}: os valores {string.Join(",", sobram)} não estão em nenhum lugar do XSD.");
        }
        return conferencia;
    }

    [Fact]
    public void CadaTagExisteNoXsdETodoCampoTemPropriedade()
    {
        var conferencia = ConferirNota();

        // Uma falha por linha: a coleção do Assert.Empty sai truncada.
        Assert.True(conferencia.Falhas.Count == 0, string.Join(Environment.NewLine, conferencia.Falhas));
    }

    [Fact]
    public void TodaClasseDoModeloEstaNaArvoreDaNota()
    {
        var conferencia = ConferirNota();
        var classes = Modelo.GetExportedTypes().Where(t => t.IsClass).ToHashSet();

        Assert.Empty(classes.Except(conferencia.Visitadas).Select(t => t.Name));
    }

    [Fact]
    public void SoMeioPagamentoEBandeiraNaoTemEnumeracaoNoXsd()
    {
        var conferencia = ConferirNota();

        Assert.Equal(EnumeracoesTabela.Order(), conferencia.EnumeracaoAusente.Distinct().Order());
    }

    [Fact]
    public void MeioPagamentoTemOsCodigosDaTabelaDoPortal()
    {
        // A Tabela de Meios de Pagamento do Portal Nacional, de 04/03/2026 (IT 2024.002 v1.11).
        int[] tabela = [1, 2, 3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 90, 91, 99];

        Assert.Equal(tabela, Enum.GetValues<MeioPagamento>().Select(m => (int)m));
    }

    [Fact]
    public void IdentificadoresDoMocSoSeRepetemEntreAlternativas()
    {
        // O MOC repete o identificador do mesmo campo nas alternativas de uma escolha (o N15 em 13 grupos de ICMS, o
        // O09 no IPITrib e no IPINT); fora disso, cada identificador é de um campo só (decisão 30).
        var repetidos = Modelo.GetExportedTypes()
            .SelectMany(t => Propriedades(t).Select(p => (Dono: t, Tag: p.GetCustomAttribute<TagAttribute>()))
                .Append((Dono: t, Tag: t.GetCustomAttribute<TagAttribute>())))
            .Where(u => u.Tag?.Identificador is not null)
            .GroupBy(u => u.Tag!.Identificador!)
            .Where(g => g.Count() > 1);

        var indevidos = new List<string>();
        foreach (var grupo in repetidos)
        {
            var donos = grupo.Select(u => u.Dono).ToList();
            var distintos = donos.Distinct().Count() == donos.Count;
            var subclasses = donos.All(d => d.BaseType is { IsAbstract: true } && d.BaseType == donos[0].BaseType);
            var exclusivas = AlternativasExclusivas.Any(a => donos.All(a.Contains));
            var icms53 = RepetidosIcms53.Contains(grupo.Key) && donos.Count == 2 && donos.All(d => d == typeof(Icms53));
            if (!(distintos && (subclasses || exclusivas)) && !icms53)
                indevidos.Add($"{grupo.Key}: {string.Join(", ", donos.Select(d => d.Name))}");
        }

        Assert.Empty(indevidos);
    }

    [Fact]
    public void SoAsClassesCompartilhadasTemTagSemIdentificador()
    {
        var semIdentificador = Modelo.GetExportedTypes()
            .Where(t => Propriedades(t).Any(p => p.GetCustomAttribute<TagAttribute>() is { Identificador: null }))
            .Select(t => t.Name)
            .Order();

        Assert.Equal(Compartilhadas.Select(t => t.Name).Order(), semIdentificador);
        Assert.All(Compartilhadas, t =>
            Assert.All(Propriedades(t), p => Assert.Null(p.GetCustomAttribute<TagAttribute>()?.Identificador)));
    }

    [Fact]
    public void AsClassesSaoSeladasComConstrutorPublicoSemParametros()
    {
        foreach (var classe in Modelo.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            Assert.True(classe.IsSealed, classe.Name);
            Assert.NotNull(classe.GetConstructor(Type.EmptyTypes));
        }
    }

    [Theory]
    [InlineData(typeof(DocumentoReferenciado), 6)]
    [InlineData(typeof(Icms), 21)]
    [InlineData(typeof(Pis), 4)]
    [InlineData(typeof(Cofins), 4)]
    public void ABaseDeEscolhaEFechada(Type tipo, int subclasses)
    {
        var construtores = tipo.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.True(tipo.IsAbstract);
        var construtor = Assert.Single(construtores);
        Assert.True(construtor.IsFamilyAndAssembly);   // private protected: só a lib deriva
        Assert.Equal(subclasses, Modelo.GetExportedTypes().Count(t => t.BaseType == tipo));
    }

    [Fact]
    public void SoAsEscolhasPorSubclasseSaoAbstratas()
    {
        var abstratas = Modelo.GetExportedTypes().Where(t => t.IsClass && t.IsAbstract).Select(t => t.Name).Order();

        Assert.Equal([nameof(Cofins), nameof(DocumentoReferenciado), nameof(Icms), nameof(Pis)], abstratas);
    }

    [Fact]
    public void AsListasSaoSoLeituraEComecamVazias()
    {
        foreach (var classe in Modelo.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            var instancia = Activator.CreateInstance(classe);
            foreach (var propriedade in Propriedades(classe).Where(p => ItemLista(p.PropertyType) is not null))
            {
                Assert.Null(propriedade.SetMethod);
                var lista = Assert.IsAssignableFrom<IList>(propriedade.GetValue(instancia));
                Assert.Empty(lista);
                Assert.False(lista.IsReadOnly, $"{classe.Name}.{propriedade.Name}");
            }
        }
    }

    [Fact]
    public void TodoValorEAnulavelETemSet()
    {
        var nulidade = new NullabilityInfoContext();
        foreach (var classe in Modelo.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            foreach (var propriedade in PropriedadesTodas(classe).Where(p => ItemLista(p.PropertyType) is null))
            {
                var nome = $"{classe.Name}.{propriedade.Name}";
                Assert.True(propriedade.SetMethod?.IsPublic, nome);
                if (propriedade.PropertyType == typeof(bool))
                {
                    Assert.Equal("Exterior", propriedade.Name);   // a única sem [Tag] (decisão 9)
                    Assert.Null(propriedade.GetCustomAttribute<TagAttribute>());
                    continue;
                }
                Assert.NotNull(propriedade.GetCustomAttribute<TagAttribute>());
                if (propriedade.PropertyType.IsValueType)
                    Assert.True(Nullable.GetUnderlyingType(propriedade.PropertyType) is not null, nome);
                else if (!EClasseModelo(propriedade.PropertyType) || propriedade.GetValue(Activator.CreateInstance(classe)) is null)
                    Assert.True(nulidade.Create(propriedade).WriteState == NullabilityState.Nullable, nome);
            }
        }
    }

    [Fact]
    public void ANotaNovaTemSoOsGruposObrigatorios()
    {
        var nota = new NotaFiscal();

        Assert.NotNull(nota.Identificacao);
        Assert.NotNull(nota.Emitente.Endereco);
        Assert.NotNull(nota.Total);
        Assert.NotNull(nota.Transporte);
        Assert.NotNull(nota.Pagamento);
        Assert.Null(nota.Destinatario);
        Assert.Null(nota.Cobranca);
        Assert.Null(nota.Suplementares);
        Assert.Null(nota.Identificacao.Numero);
        Assert.Null(nota.Identificacao.UF);
        Assert.Empty(nota.Itens);
        Assert.NotNull(nota.Total.Icms);
        Assert.Null(nota.Total.IbsCbs);
    }

    [Fact]
    public void OItemNovoTemSoOsGruposObrigatorios()
    {
        var item = new Item();

        Assert.NotNull(item.Produto);
        Assert.NotNull(item.Imposto);
        Assert.Null(item.Imposto.Icms);
        Assert.Null(item.Imposto.Pis);
        Assert.Null(item.Imposto.IbsCbs);
        Assert.Null(item.ImpostoDevolvido);
        Assert.Empty(item.Produto.Armas);
        Assert.Null(item.Produto.Combustivel);
        Assert.NotNull(new TributacaoIbsCbs().IbsUF);
        Assert.NotNull(new ImpostoDevolvido().Ipi);
    }

    [Fact]
    public void MontaOItemDoExemploDoDesenho()
    {
        // O exemplo de docs/MODELO.md 11.11.
        var nota = new NotaFiscal();
        var item = new Item
        {
            Produto =
            {
                Codigo = "123", Gtin = "SEM GTIN", Descricao = "CAFE TORRADO 500G", Ncm = "09012100", Cfop = 5102,
                UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m, Valor = 50.00m,
                GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m, ValorUnitarioTributavel = 25.00m,
                Totalizacao = IndicadorTotal.Compoe,
            },
            Imposto =
            {
                Icms = new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito },
                Pis = new PisOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
                Cofins = new CofinsOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
                IbsCbs = new IbsCbs
                {
                    Cst = "000", ClassificacaoTributaria = "000001",
                    Tributacao = new TributacaoIbsCbs
                    {
                        BaseCalculo = 50.00m, ValorIbs = 0.05m,
                        IbsUF = { Aliquota = 0.10m, Valor = 0.05m },
                        IbsMunicipio = { Aliquota = 0m, Valor = 0m },
                        Cbs = { Aliquota = 0.90m, Valor = 0.45m },
                    },
                },
            },
        };
        nota.Itens.Add(item);
        nota.Total.Icms.ValorProdutos = 50.00m;
        nota.Total.Icms.ValorNota = 50.00m;

        var lido = Assert.Single(nota.Itens);
        var baseIcms = lido.Imposto.Icms switch
        {
            Icms00 i => i.BaseCalculo,
            Icms20 i => i.BaseCalculo,
            IcmsSn900 i => i.BaseCalculo,
            _ => null,
        };

        Assert.Equal(OrigemMercadoria.Nacional, lido.Imposto.Icms?.Origem);   // na base, sem switch
        Assert.Equal(CstPisCofins.OutrasSaidas, lido.Imposto.Pis?.Cst);
        Assert.Null(baseIcms);
        Assert.Equal(0.45m, lido.Imposto.IbsCbs?.Tributacao?.Cbs.Valor);
        Assert.Null(new Icms00().Cst);   // nulo: o único valor do grupo, na escrita (decisão 24)
    }

    [Fact]
    public void MontaANotaDoExemploDoDesenho()
    {
        // O exemplo de docs/MODELO.md 5, com o inicializador nas listas e nos grupos obrigatórios.
        var emissao = new DateTimeOffset(2026, 10, 19, 10, 30, 0, TimeSpan.FromHours(-3));
        var nota = new NotaFiscal
        {
            Identificacao =
            {
                NaturezaOperacao = "VENDA", Serie = 1, Numero = 123,
                Emissao = emissao, Tipo = TipoNota.Saida, LocalDestino = LocalDestino.Interna,
                MunicipioFatoGerador = 3106200, FormatoImpressao = FormatoImpressao.DanfeNFCe,
                Finalidade = FinalidadeEmissao.Normal, ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal,
                Presenca = PresencaComprador.Presencial, Processo = ProcessoEmissao.AplicativoContribuinte,
                VersaoAplicativo = "PDV 1.0",
            },
            Emitente =
            {
                Cnpj = "12ABC34501DE35", Nome = "LOJA EXEMPLO LTDA", InscricaoEstadual = "0010000000000",
                RegimeTributario = RegimeTributario.SimplesNacional,
                Endereco =
                {
                    Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200,
                    Municipio = "BELO HORIZONTE", UF = UF.MG, Cep = "30110000",
                },
            },
            Destinatario = new Destinatario { Cpf = "52998224725", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte },
            Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
            Pagamento =
            {
                Formas =
                {
                    new FormaPagamento
                    {
                        Meio = MeioPagamento.PixDinamico, Valor = 50.00m,
                        Transacao = new TransacaoEletronica { Integracao = TipoIntegracao.NaoIntegrado },
                    },
                },
            },
        };
        nota.Identificacao.Referencias.Add(new NFeReferenciada { Chave = "31261012345678000195650010000001231000001230" });
        nota.Identificacao.Referencias.Add(new CupomFiscalReferenciado { Modelo = ModeloCupomFiscal.Ecf, NumeroEcf = 1, Coo = 42 });

        var textos = nota.Identificacao.Referencias.Select(r => r switch
        {
            NFeReferenciada n => n.Chave,
            CupomFiscalReferenciado c => $"ECF {c.NumeroEcf} COO {c.Coo}",
            _ => "outro",
        });

        Assert.Equal(["31261012345678000195650010000001231000001230", "ECF 1 COO 42"], textos);
        Assert.Equal(UF.MG, nota.Emitente.Endereco.UF);
        Assert.Equal(50.00m, Assert.Single(nota.Pagamento.Formas).Valor);
        Assert.Null(nota.Identificacao.Modelo);   // do núcleo, na escrita (decisão 6)
    }

    [Fact]
    public void OValorForaDaListaFicaComONumero()
    {
        // A leitura guarda o código que a lista não tem (decisão 8); o modelo não recusa.
        var forma = new FormaPagamento { Meio = (MeioPagamento)77 };

        Assert.Equal(77, (int)forma.Meio!);
        Assert.False(Enum.IsDefined(forma.Meio.Value));
    }
}
