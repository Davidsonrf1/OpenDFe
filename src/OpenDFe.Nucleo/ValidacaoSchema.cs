using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace OpenDFe;

// Uma validação de um XML por um XmlSchemaSet compilado (SchemaXml.Validar). Lê o XML com um XmlReader sem
// validação e conduz o XmlSchemaValidator nó a nó: assim cada evento do validador chega sabendo qual nó o
// causou (o elemento, o atributo, o texto, o fim do elemento), com o caminho e o tipo do schema dele, e a
// lista do que era esperado ali. A mensagem do .NET não é lida: a classificação vem do contexto, e a mensagem
// em português, das regras levantadas do tipo (AnaliseTipo).
//
// Uma instância por chamada: o XmlSchemaSet compilado é só lido (nunca alterado), e o NameTable é o do leitor
// desta chamada, nunca o do XmlSchemaSet, que não é seguro para escrita concorrente.
internal sealed class ValidacaoSchema
{
    private readonly XmlSchemaSet _schemas;
    private readonly string _codigo;
    private readonly Func<string, string?>? _propriedade;
    private readonly List<Pendente> _pendentes = [];
    private readonly List<ValidationEventArgs> _eventos = [];
    private readonly Stack<Quadro> _pilha = new();
    private XmlReader _leitor = null!;
    private XmlSchemaValidator _validador = null!;
    private bool _raizDesconhecida;

    public ValidacaoSchema(XmlSchemaSet schemas, string codigo, Func<string, string?>? propriedade)
    {
        _schemas = schemas;
        _codigo = codigo;
        _propriedade = propriedade;
    }

    public IReadOnlyList<Problema> Executar(string xml)
    {
        var configuracao = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
        };
        using (_leitor = XmlReader.Create(new StringReader(xml), configuracao))
        {
            _validador = new XmlSchemaValidator(_leitor.NameTable, _schemas, (IXmlNamespaceResolver)_leitor,
                XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings)
            {
                XmlResolver = null,
                LineInfoProvider = _leitor as IXmlLineInfo,
            };
            _validador.ValidationEventHandler += (_, e) => _eventos.Add(e);
            _validador.Initialize();

            while (_leitor.Read())
            {
                switch (_leitor.NodeType)
                {
                    case XmlNodeType.Element:
                        AbrirElemento();
                        break;
                    case XmlNodeType.EndElement:
                        FecharElemento();
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        Texto(_leitor.Value, espaco: false);
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        if (_pilha.Count > 0)
                            Texto(_leitor.Value, espaco: true);
                        break;
                }
            }
            _validador.EndValidation();
            Descartar();
        }

        return _pendentes.ConvertAll(p => p.Problema(_codigo, _propriedade));
    }

    private void AbrirElemento()
    {
        var nome = _leitor.LocalName;
        var espaco = _leitor.NamespaceURI;
        _pilha.TryPeek(out var pai);
        var segmento = new Segmento(nome, pai?.ContarFilho(nome) ?? 1, pai?.Segmento, pai?.Filhos, atributo: false);

        string? xsiTipo = null, xsiNulo = null;
        var atributos = new List<(string Nome, string Espaco, string Valor)>();
        if (_leitor.MoveToFirstAttribute())
        {
            do
            {
                if (_leitor.NamespaceURI == XmlNamespaces.Xmlns)
                    continue;
                if (_leitor.NamespaceURI == XmlSchema.InstanceNamespace)
                {
                    if (_leitor.LocalName == "type")
                        xsiTipo = _leitor.Value;
                    else if (_leitor.LocalName == "nil")
                        xsiNulo = _leitor.Value;
                }
                atributos.Add((_leitor.LocalName, _leitor.NamespaceURI, _leitor.Value));
            }
            while (_leitor.MoveToNextAttribute());
            _leitor.MoveToElement();
        }
        var vazio = _leitor.IsEmptyElement;

        // O que o pai esperava nesta posição: depois da chamada, o estado já é o do elemento novo.
        var esperados = pai is null || _raizDesconhecida ? [] : _validador.GetExpectedParticles();
        var quadro = new Quadro(segmento);
        _pilha.Push(quadro);

        _validador.ValidateElement(nome, espaco, quadro.Info, xsiTipo, xsiNulo, null, null);
        if (pai is null && quadro.Info.SchemaElement is null)
        {
            _raizDesconhecida = true;
            var globais = _schemas.GlobalElements.Names.Cast<XmlQualifiedName>().Select(n => n.Name).ToList();
            var mensagemOriginal = _eventos.Count > 0 ? _eventos[0].Message : "";
            Adicionar(Severidade.Erro, nome, segmento,
                $"{nome}: o elemento raiz não está declarado no schema{DescreverNamespace(espaco)}; esperado: {Lista(globais)}.",
                new FalhaSchema(TipoFalhaSchema.ElementoInesperado, mensagemOriginal) { Esperados = globais, Linha = Linha(), Coluna = Coluna() });
        }
        if (_raizDesconhecida)
        {
            // Sem a raiz, nada mais tem declaração: o resto é só conduzido até o fim, sem problemas.
            _validador.ValidateEndOfAttributes(null);
            _eventos.Clear();
            if (vazio)
                FecharElemento();
            return;
        }
        foreach (var evento in Tomar())
        {
            if (evento.Severity == XmlSeverityType.Warning && quadro.Info.SchemaElement is null)
            {
                // Aceito sem declaração (xs:any lax): um aviso só, no elemento de cima; o conteúdo não é validado.
                if (!_pilha.Any(q => q.Falhou || q.NaoDeclarado))
                {
                    Adicionar(Severidade.Aviso, nome, segmento,
                        $"{nome}: o elemento não tem declaração no schema e não foi validado (o schema o aceita sem conferir).",
                        new FalhaSchema(TipoFalhaSchema.Outra, evento.Message) { Linha = Linha(evento), Coluna = Coluna(evento) });
                }
                quadro.NaoDeclarado = true;
                continue;
            }
            if (evento.Severity == XmlSeverityType.Warning)
            {
                Generico(evento, nome, segmento);
                continue;
            }
            quadro.Falhou = true;
            Inesperado(evento, nome, segmento, pai, esperados);
        }

        foreach (var (nomeAtributo, espacoAtributo, valor) in atributos)
        {
            var info = new XmlSchemaInfo();
            _validador.ValidateAttribute(nomeAtributo, espacoAtributo, valor, info);
            var segmentoAtributo = new Segmento(nomeAtributo, 1, segmento, null, atributo: true);
            foreach (var evento in Tomar())
            {
                if (evento.Severity == XmlSeverityType.Warning)
                    Generico(evento, nomeAtributo, segmentoAtributo);
                else if (info.SchemaAttribute is null)
                    AtributoInesperado(evento, nomeAtributo, segmentoAtributo, quadro);
                else
                    ValorInvalido(evento, nomeAtributo, segmentoAtributo, info.SchemaAttribute, info.SchemaType, valor);
            }
        }

        _validador.ValidateEndOfAttributes(null);
        foreach (var evento in Tomar())
        {
            if (evento.Severity == XmlSeverityType.Warning)
            {
                Generico(evento, nome, segmento);
                continue;
            }
            var declaracoes = AtributosObrigatorios(quadro.Info.SchemaType)
                .Where(a => !atributos.Any(x => x.Nome == a.QualifiedName.Name && x.Espaco == a.QualifiedName.Namespace))
                .ToList();
            var ausentes = declaracoes.ConvertAll(a => a.QualifiedName.Name);
            if (ausentes.Count == 0)
            {
                Generico(evento, nome, segmento);
                continue;
            }
            Adicionar(Severidade.Erro, ausentes.Count == 1 ? ausentes[0] : nome,
                ausentes.Count == 1 ? new Segmento(ausentes[0], 1, segmento, null, atributo: true) : segmento,
                ausentes.Count == 1
                    ? $"{nome}: falta o atributo obrigatório {ausentes[0]}."
                    : $"{nome}: faltam os atributos obrigatórios {Lista(ausentes)}.",
                new FalhaSchema(TipoFalhaSchema.AtributoAusente, evento.Message)
                {
                    Esperados = ausentes,
                    TipoSchema = NomeTipo(quadro.Info.SchemaType),
                    Documentacao = Documentacao(ausentes.Count == 1 ? declaracoes[0] : quadro.Info.SchemaElement),
                    DocumentacaoTipo = DocumentacaoTipo(quadro.Info.SchemaType),
                    Linha = Linha(evento),
                    Coluna = Coluna(evento),
                });
        }

        if (vazio)
            FecharElemento();
    }

    private void FecharElemento()
    {
        var quadro = _pilha.Peek();
        if (_raizDesconhecida)
        {
            _validador.ValidateEndElement(null);
            _eventos.Clear();
            _pilha.Pop();
            return;
        }

        var tipoValor = AnaliseTipo.TipoSimples(quadro.Info.SchemaType);
        var esperados = tipoValor is null ? _validador.GetExpectedParticles() : [];
        _validador.ValidateEndElement(null);
        var nome = quadro.Segmento.Nome;
        foreach (var evento in Tomar())
        {
            if (evento.Severity == XmlSeverityType.Warning)
                Generico(evento, nome, quadro.Segmento);
            else if (quadro.Falhou)
                continue;   // o .NET às vezes repete no fim a falha do início do mesmo elemento
            else if (tipoValor is not null)
                ValorInvalido(evento, nome, quadro.Segmento, quadro.Info.SchemaElement, quadro.Info.SchemaType, quadro.Texto.ToString());
            else
                Ausente(evento, nome, quadro, esperados);
        }
        _pilha.Pop();
    }

    private void Texto(string valor, bool espaco)
    {
        var quadro = _pilha.Peek();
        quadro.Texto.Append(valor);
        if (espaco)
            _validador.ValidateWhitespace(valor);
        else
            _validador.ValidateText(valor);
        if (_raizDesconhecida)
        {
            _eventos.Clear();
            return;
        }
        foreach (var evento in Tomar())
        {
            var nome = quadro.Segmento.Nome;
            if (evento.Severity == XmlSeverityType.Warning || AnaliseTipo.TipoSimples(quadro.Info.SchemaType) is not null)
            {
                Generico(evento, nome, quadro.Segmento);
                continue;
            }
            Adicionar(Severidade.Erro, nome, quadro.Segmento,
                $"{nome}: o elemento não aceita texto, só elementos filhos.",
                new FalhaSchema(TipoFalhaSchema.TextoInesperado, evento.Message)
                {
                    TipoSchema = NomeTipo(quadro.Info.SchemaType),
                    Documentacao = Documentacao(quadro.Info.SchemaElement),
                    DocumentacaoTipo = DocumentacaoTipo(quadro.Info.SchemaType),
                    Linha = Linha(evento),
                    Coluna = Coluna(evento),
                });
        }
    }

    // Classificação

    private void Inesperado(ValidationEventArgs evento, string nome, Segmento segmento, Quadro? pai, XmlSchemaParticle[] particulas)
    {
        var esperados = Nomes(particulas);
        // Com um só elemento obrigatório esperado no lugar (o que faltou), a documentação é a dele.
        var obrigatorios = particulas.OfType<XmlSchemaElement>().Where(p => p.MinOccurs > 0).ToList();
        var tipoPai = pai?.Info.SchemaType;
        string mensagem;
        if (tipoPai is not null && AnaliseTipo.TipoSimples(tipoPai) is not null)
            mensagem = $"{nome}: o elemento {pai!.Segmento.Nome} não aceita elementos filhos.";
        else if (esperados.Count == 0)
            mensagem = $"{nome}: o elemento não era esperado; nenhum outro elemento era esperado em {pai?.Segmento.Nome}.";
        else if (tipoPai is XmlSchemaComplexType complexo && Declara(complexo.ContentTypeParticle, nome))
            mensagem = $"{nome}: o elemento está fora de ordem ou repetido; esperado: {Lista(esperados)}.";
        else
            mensagem = $"{nome}: o elemento não era esperado; esperado: {Lista(esperados)}.";
        Adicionar(Severidade.Erro, nome, segmento, mensagem,
            new FalhaSchema(TipoFalhaSchema.ElementoInesperado, evento.Message)
            {
                Esperados = esperados,
                Documentacao = obrigatorios.Count == 1 ? Documentacao(obrigatorios[0]) : null,
                Linha = Linha(evento),
                Coluna = Coluna(evento),
            });
    }

    private void Ausente(ValidationEventArgs evento, string nome, Quadro quadro, XmlSchemaParticle[] particulas)
    {
        var esperados = Nomes(particulas);
        var obrigatorias = particulas.Where(p => p.MinOccurs > 0).ToArray();
        var obrigatorios = Nomes(obrigatorias);
        if (esperados.Count == 0)
        {
            Generico(evento, nome, quadro.Segmento);
            return;
        }
        var falha = new FalhaSchema(TipoFalhaSchema.ElementoAusente, evento.Message)
        {
            Esperados = obrigatorios.Count > 0 ? obrigatorios : esperados,
            TipoSchema = NomeTipo(quadro.Info.SchemaType),
            Documentacao = obrigatorios.Count == 1
                ? Documentacao(obrigatorias.OfType<XmlSchemaElement>().FirstOrDefault())
                : Documentacao(quadro.Info.SchemaElement),
            DocumentacaoTipo = DocumentacaoTipo(quadro.Info.SchemaType),
            Linha = Linha(evento),
            Coluna = Coluna(evento),
        };
        if (obrigatorios.Count == 1 && obrigatorios[0] != "*")
        {
            Adicionar(Severidade.Erro, obrigatorios[0], new Segmento(obrigatorios[0], 1, quadro.Segmento, null, atributo: false),
                $"{nome}: falta o elemento obrigatório {obrigatorios[0]}.", falha);
        }
        else if (obrigatorios.Count > 1)
        {
            Adicionar(Severidade.Erro, nome, quadro.Segmento, $"{nome}: falta um dos elementos {Lista(obrigatorios)}.", falha);
        }
        else
        {
            Adicionar(Severidade.Erro, nome, quadro.Segmento, $"{nome}: o conteúdo está incompleto; esperado: {Lista(esperados)}.", falha);
        }
    }

    private void AtributoInesperado(ValidationEventArgs evento, string nome, Segmento segmento, Quadro quadro)
    {
        var declarados = AtributosDeclarados(quadro.Info.SchemaType).Select(a => a.QualifiedName.Name).ToList();
        var dono = quadro.Segmento.Nome;
        Adicionar(Severidade.Erro, nome, segmento,
            declarados.Count == 0
                ? $"{nome}: o elemento {dono} não aceita atributos."
                : $"{nome}: o elemento {dono} não aceita este atributo; aceitos: {Lista(declarados)}.",
            new FalhaSchema(TipoFalhaSchema.AtributoInesperado, evento.Message) { Esperados = declarados, Linha = Linha(evento), Coluna = Coluna(evento) });
    }

    private void ValorInvalido(ValidationEventArgs evento, string nome, Segmento segmento, XmlSchemaAnnotated? declaracao,
        XmlSchemaType? tipo, string valor)
    {
        var simples = AnaliseTipo.TipoSimples(tipo);
        var nomeTipo = NomeTipo(tipo);
        var analise = simples is null
            ? AnaliseTipo.Resultado.Vazio
            : AnaliseTipo.Analisar(simples, valor, _leitor.NameTable, (IXmlNamespaceResolver)_leitor, nomeTipo);
        var mensagem = analise.Frases.Count > 0
            ? $"{nome}: {string.Join("; ", analise.Frases)}."
            : $"{nome}: o valor é inválido{(nomeTipo is null ? "" : " para o tipo " + nomeTipo)}.";
        Adicionar(Severidade.Erro, nome, segmento, mensagem,
            new FalhaSchema(TipoFalhaSchema.Valor, evento.Message)
            {
                TipoSchema = nomeTipo,
                Documentacao = Documentacao(declaracao),
                DocumentacaoTipo = DocumentacaoTipo(tipo),
                Valor = valor,
                Restricoes = analise.Restricoes,
                Linha = Linha(evento),
                Coluna = Coluna(evento),
            });
    }

    private void Generico(ValidationEventArgs evento, string nome, Segmento segmento)
    {
        var aviso = evento.Severity == XmlSeverityType.Warning;
        // Dentro de um elemento que já falhou ou não tem declaração, o .NET valida o conteúdo sem schema e avisa
        // a cada nó: ruído.
        if (aviso && _pilha.Any(q => q.Falhou || q.NaoDeclarado))
            return;
        Adicionar(aviso ? Severidade.Aviso : Severidade.Erro, nome, segmento,
            aviso
                ? $"{nome}: o validador do schema avisou sobre este ponto do XML."
                : $"{nome}: o XML não segue o schema neste ponto.",
            new FalhaSchema(TipoFalhaSchema.Outra, evento.Message) { Linha = Linha(evento), Coluna = Coluna(evento) });
    }

    // Apoio

    private List<ValidationEventArgs> Tomar()
    {
        if (_eventos.Count == 0)
            return [];
        var eventos = _eventos.ToList();
        _eventos.Clear();
        return eventos;
    }

    private void Descartar()
    {
        foreach (var evento in Tomar())
        {
            if (!_raizDesconhecida)
                Adicionar(evento.Severity == XmlSeverityType.Warning ? Severidade.Aviso : Severidade.Erro, null, null,
                    "O XML não segue o schema.", new FalhaSchema(TipoFalhaSchema.Outra, evento.Message) { Linha = Linha(evento), Coluna = Coluna(evento) });
        }
    }

    private void Adicionar(Severidade severidade, string? tag, Segmento? segmento, string mensagem, FalhaSchema falha) =>
        _pendentes.Add(new Pendente(severidade, tag, segmento, mensagem, falha));

    private static List<string> Nomes(XmlSchemaParticle[] particulas)
    {
        var nomes = new List<string>();
        foreach (var particula in particulas)
        {
            var nome = particula switch
            {
                XmlSchemaElement elemento => elemento.QualifiedName.Name,
                XmlSchemaAny => "*",
                _ => null,
            };
            if (nome is not null && !nomes.Contains(nome))
                nomes.Add(nome);
        }
        return nomes;
    }

    private static bool Declara(XmlSchemaParticle? particula, string nome) => particula switch
    {
        XmlSchemaElement elemento => elemento.QualifiedName.Name == nome,
        XmlSchemaGroupBase grupo => grupo.Items.OfType<XmlSchemaParticle>().Any(p => Declara(p, nome)),
        XmlSchemaGroupRef referencia => Declara(referencia.Particle, nome),
        _ => false,
    };

    private static IEnumerable<XmlSchemaAttribute> AtributosDeclarados(XmlSchemaType? tipo) =>
        tipo is XmlSchemaComplexType complexo
            ? complexo.AttributeUses.Values.Cast<XmlSchemaAttribute>().OrderBy(a => a.QualifiedName.Name, StringComparer.Ordinal)
            : [];

    private static IEnumerable<XmlSchemaAttribute> AtributosObrigatorios(XmlSchemaType? tipo) =>
        AtributosDeclarados(tipo).Where(a => a.Use == XmlSchemaUse.Required);

    // O texto dos xs:documentation da anotação, com o espaço em branco normalizado; nulo sem documentação.
    internal static string? Documentacao(XmlSchemaAnnotated? item)
    {
        if (item?.Annotation is null)
            return null;
        var partes = item.Annotation.Items.OfType<XmlSchemaDocumentation>()
            .SelectMany(d => d.Markup ?? [])
            .Select(n => n?.InnerText)
            .Where(t => !string.IsNullOrWhiteSpace(t));
        var texto = string.Join(" ", string.Join(" ", partes).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return texto.Length == 0 ? null : texto;
    }

    // A documentação do tipo nomeado; a do tipo anônimo fica com a declaração do campo.
    private static string? DocumentacaoTipo(XmlSchemaType? tipo) =>
        tipo is null || tipo.QualifiedName.IsEmpty ? null : Documentacao(tipo);

    internal static string? NomeTipo(XmlSchemaType? tipo)
    {
        if (tipo is null || tipo.QualifiedName.IsEmpty)
            return null;
        return tipo.QualifiedName.Namespace == XmlSchema.Namespace ? "xs:" + tipo.QualifiedName.Name : tipo.QualifiedName.Name;
    }

    private static string DescreverNamespace(string espaco) => espaco.Length == 0 ? " (sem namespace)" : $" (namespace {espaco})";

    internal static string Lista(IReadOnlyList<string> nomes)
    {
        const int Maximo = 10;
        return nomes.Count <= Maximo
            ? string.Join(", ", nomes)
            : string.Join(", ", nomes.Take(Maximo)) + string.Create(CultureInfo.InvariantCulture, $" e mais {nomes.Count - Maximo}");
    }

    private int? Linha() => _leitor is IXmlLineInfo { } info && info.HasLineInfo() && info.LineNumber > 0 ? info.LineNumber : null;

    private int? Coluna() => _leitor is IXmlLineInfo { } info && info.HasLineInfo() && info.LinePosition > 0 ? info.LinePosition : null;

    private static int? Linha(ValidationEventArgs evento) => evento.Exception.LineNumber > 0 ? evento.Exception.LineNumber : null;

    private static int? Coluna(ValidationEventArgs evento) => evento.Exception.LinePosition > 0 ? evento.Exception.LinePosition : null;

    private static class XmlNamespaces
    {
        public const string Xmlns = "http://www.w3.org/2000/xmlns/";
    }

    // Um elemento aberto: o caminho, os filhos já vistos por nome (para o índice) e o texto.
    private sealed class Quadro(Segmento segmento)
    {
        public Segmento Segmento { get; } = segmento;
        public Dictionary<string, int> Filhos { get; } = new(StringComparer.Ordinal);
        public XmlSchemaInfo Info { get; } = new();
        public StringBuilder Texto { get; } = new();
        public bool Falhou { get; set; }
        public bool NaoDeclarado { get; set; }

        public int ContarFilho(string nome)
        {
            Filhos[nome] = Filhos.GetValueOrDefault(nome) + 1;
            return Filhos[nome];
        }
    }

    // Um passo do caminho. O índice só é escrito no fim da leitura, quando já se sabe se o elemento se repete
    // entre os irmãos (a contagem final do pai).
    private sealed class Segmento(string nome, int indice, Segmento? pai, Dictionary<string, int>? irmaos, bool atributo)
    {
        public string Nome { get; } = nome;

        public string Caminho()
        {
            var passo = atributo
                ? "@" + Nome
                : irmaos is not null && irmaos.GetValueOrDefault(Nome) > 1
                    ? string.Create(CultureInfo.InvariantCulture, $"{Nome}[{indice}]")
                    : Nome;
            return pai is null ? passo : pai.Caminho() + "/" + passo;
        }
    }

    private sealed record Pendente(Severidade Severidade, string? Tag, Segmento? Segmento, string Mensagem, FalhaSchema Falha)
    {
        public Problema Problema(string codigo, Func<string, string?>? propriedade)
        {
            var caminho = Segmento?.Caminho();
            return new(codigo, Severidade, Mensagem)
            {
                Tag = Tag,
                Caminho = caminho,
                Propriedade = caminho is not null && propriedade is not null ? propriedade(caminho) : null,
                Schema = Falha,
            };
        }
    }
}
