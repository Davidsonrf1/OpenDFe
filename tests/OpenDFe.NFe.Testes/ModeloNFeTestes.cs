using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O modelo da etapa 2.2 (docs/MODELO.md) contra o XSD do pacote embutido no leiaute: cada [Tag] existe no lugar
// certo, todo campo do XSD tem propriedade (fora do que fica para a 2.3 e do que não é do modelo), a ocorrência do
// XSD bate com o grupo inicializado ou nulo, o Exterior está onde a UF aceita EX e as enumerações têm os valores
// do XSD. E as regras de forma (MODELO.md 3).
public class ModeloNFeTestes
{
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";

    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    private static readonly Lazy<XmlSchemaSet> Schemas = new(Compilar);

    // As enumerações cujo campo o XSD declara só pelo padrão ([0-9]{2}): a lista vem de tabela do Portal Nacional.
    private static readonly string[] EnumeracoesTabela = [nameof(MeioPagamento), nameof(BandeiraCartao)];

    // O valor do XML que não é número (MODELO.md, decisão 8).
    private static readonly Dictionary<int, string> ModelosCupom = new() { [1] = "2B", [2] = "2C", [3] = "2D" };

    private sealed record Filho(string Nome, XmlSchemaType? Tipo, bool Atributo, bool Opcional, bool Repete);

    private sealed class Conferencia
    {
        public List<string> Falhas { get; } = [];

        public HashSet<Type> Visitadas { get; } = [];

        public List<string> EnumeracaoAusente { get; } = [];
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
                Assert.False(filhos.ContainsKey(nome), $"A tag {nome} aparece duas vezes no mesmo grupo do XSD.");
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

    // Confere a classe contra os filhos do grupo do XSD; devolve as tags que ela mapeou.
    private static HashSet<string> Conferir(Type classe, Dictionary<string, Filho> filhos, string caminho, Conferencia conferencia)
    {
        conferencia.Visitadas.Add(classe);
        var usadas = new HashSet<string>();
        var instancia = classe.IsAbstract ? null : Activator.CreateInstance(classe);
        foreach (var propriedade in Propriedades(classe))
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
        if (tipo == typeof(Item) || tipo == typeof(Total))
        {
            conferencia.Visitadas.Add(tipo);   // o conteúdo é da 2.3
            return;
        }
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
        Sobras(filhos, usadas, tipo == typeof(Identificacao) ? ["cDV"] : [], caminho, conferencia);
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
        var valores = Enum.GetValues(tipo).Cast<object>().Select(Convert.ToInt32)
            .Select(v => tipo == typeof(ModeloCupomFiscal) ? ModelosCupom[v] : v.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var xsd = enumeracao.Select(v => tipo == typeof(ModeloCupomFiscal) ? v : int.Parse(v, System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture));
        var faltam = xsd.Except(valores).ToList();
        var sobram = valores.Except(xsd).ToList();
        if (faltam.Count > 0 || sobram.Count > 0)
            conferencia.Falhas.Add($"{caminho}: {tipo.Name} diverge do XSD (faltam {string.Join(",", faltam)}; sobram {string.Join(",", sobram)}).");
    }

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
    public void IdentificadoresDoMocNaoSeRepetem()
    {
        var identificadores = Modelo.GetExportedTypes()
            .SelectMany(t => Propriedades(t).Select(p => p.GetCustomAttribute<TagAttribute>()).Append(t.GetCustomAttribute<TagAttribute>()))
            .Select(t => t?.Identificador)
            .OfType<string>()
            .ToList();

        Assert.Empty(identificadores.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key));
    }

    [Fact]
    public void SoAsClassesCompartilhadasTemTagSemIdentificador()
    {
        var semIdentificador = Modelo.GetExportedTypes()
            .Where(t => Propriedades(t).Any(p => p.GetCustomAttribute<TagAttribute>() is { Identificador: null }))
            .Select(t => t.Name)
            .Order();

        Assert.Equal([nameof(Local), nameof(Observacao), nameof(Veiculo)], semIdentificador);
        Assert.All(new[] { typeof(Local), typeof(Observacao), typeof(Veiculo) }, t =>
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

    [Fact]
    public void ADocumentoReferenciadoEFechada()
    {
        var construtores = typeof(DocumentoReferenciado).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.True(typeof(DocumentoReferenciado).IsAbstract);
        var construtor = Assert.Single(construtores);
        Assert.True(construtor.IsFamilyAndAssembly);   // private protected: só a lib deriva
        Assert.Equal(6, Modelo.GetExportedTypes().Count(t => t.BaseType == typeof(DocumentoReferenciado)));
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
            foreach (var propriedade in Propriedades(classe).Where(p => ItemLista(p.PropertyType) is null))
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
