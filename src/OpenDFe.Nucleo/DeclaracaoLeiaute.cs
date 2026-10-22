using System.Net.Security;

namespace OpenDFe;

/// <summary>
/// O que um leiaute declara para um documento: as operações, os atendimentos (UF × tipo de emissão →
/// autorizador), as URLs (autorizador × ambiente × serviço), os serviços SOAP, as cifras de cada autorizador, a
/// configuração do documento, as formas de leitura e as regras. Criada pelo núcleo,
/// que a passa a <see cref="ILeiaute.Declarar"/>.
/// </summary>
/// <remarks>
/// Os métodos <c>Registrar</c> e <c>Declarar</c> só valem durante <see cref="ILeiaute.Declarar"/> (depois,
/// <see cref="InvalidOperationException"/>); as propriedades são a listagem, na ordem da declaração. Depois do
/// <see cref="ILeiaute.Declarar"/>, a instância é imutável e segura para uso concorrente.
/// </remarks>
public sealed class DeclaracaoLeiaute
{
    private readonly List<ExecutorOperacao> _executores = [];
    private readonly Dictionary<Type, ExecutorOperacao> _executoresTipo = [];
    private readonly List<Atendimento> _atendimentos = [];
    private readonly Dictionary<(UF, TipoEmissao), Atendimento> _atendimentosChave = [];
    private readonly List<EnderecoServico> _enderecos = [];
    private readonly HashSet<(Destino, Ambiente, string)> _enderecosChave = [];
    private readonly List<ServicoSoap> _servicos = [];
    private readonly Dictionary<string, ServicoSoap> _servicosNome = new(StringComparer.Ordinal);
    private readonly Dictionary<Destino, IReadOnlyList<TlsCipherSuite>> _cifras = [];
    private readonly List<FormaLeitura> _leituras = [];
    private readonly List<Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>>> _regras = [];
    private OperacaoSuportada[] _operacoes = [];
    private bool _aberta = true;
    private bool _configuracaoDeclarada;

    private DeclaracaoLeiaute(Documento documento)
    {
        Documento = documento;
    }

    /// <summary>
    /// A declaração de um leiaute para um documento, sem criar o núcleo: a listagem antes de haver certificado
    /// (a tela de configuração).
    /// </summary>
    /// <param name="leiaute">O leiaute.</param>
    /// <param name="documento">O documento.</param>
    /// <returns>A declaração, já fechada.</returns>
    /// <exception cref="ArgumentNullException">O leiaute é nulo.</exception>
    /// <exception cref="ArgumentException">O leiaute não atende o documento.</exception>
    /// <remarks>
    /// As exceções de <see cref="ILeiaute.Declarar"/> (uma declaração repetida, um argumento inválido) sobem como
    /// vieram: são erro de programação do leiaute.
    /// </remarks>
    public static DeclaracaoLeiaute Criar(ILeiaute leiaute, Documento documento)
    {
        ArgumentNullException.ThrowIfNull(leiaute);
        if (leiaute.Documentos is not { } documentos || !documentos.Contains(documento))
            throw new ArgumentException($"O leiaute {leiaute.Versao} não atende o documento {documento}.", nameof(documento));

        var declaracao = new DeclaracaoLeiaute(documento);
        try
        {
            leiaute.Declarar(declaracao);
        }
        finally
        {
            declaracao.Fechar();
        }
        return declaracao;
    }

    /// <summary>O documento da declaração.</summary>
    public Documento Documento { get; }

    /// <summary>As operações registradas, na ordem do registro.</summary>
    public IReadOnlyList<OperacaoSuportada> Operacoes => _aberta ? [.. _executores.Select(e => e.Operacao)] : _operacoes;

    /// <summary>Os atendimentos declarados, na ordem da declaração.</summary>
    public IReadOnlyList<Atendimento> Atendimentos => _atendimentos.AsReadOnly();

    /// <summary>As URLs declaradas, na ordem da declaração.</summary>
    public IReadOnlyList<EnderecoServico> Enderecos => _enderecos.AsReadOnly();

    /// <summary>Os serviços SOAP declarados, na ordem da declaração.</summary>
    public IReadOnlyList<ServicoSoap> Servicos => _servicos.AsReadOnly();

    /// <summary>As cifras de TLS declaradas por autorizador (só os que não usam o padrão da máquina).</summary>
    public IReadOnlyDictionary<Destino, IReadOnlyList<TlsCipherSuite>> Cifras => _cifras.AsReadOnly();

    /// <summary>As formas de XML que o leiaute lê para o documento (<see cref="DFe.Detectar"/>), na ordem da declaração.</summary>
    public IReadOnlyList<FormaLeitura> Leituras => _leituras.AsReadOnly();

    /// <summary>
    /// O tipo da configuração do documento (<c>typeof(ConfiguracaoNFCe)</c>); <see langword="null"/> se o
    /// leiaute não declara nenhuma.
    /// </summary>
    public Type? TipoConfiguracaoDocumento { get; private set; }

    /// <summary>Se o núcleo recusa a configuração sem a parte do documento.</summary>
    public bool ConfiguracaoDocumentoObrigatoria { get; private set; }

    /// <summary>Registra o handler de uma operação, pelo tipo do parâmetro.</summary>
    /// <typeparam name="TParametro">O parâmetro da operação: um tipo concreto (o núcleo despacha pelo tipo exato).</typeparam>
    /// <typeparam name="TRetorno">O retorno da operação.</typeparam>
    /// <param name="operacao">O handler.</param>
    /// <exception cref="ArgumentNullException">O handler é nulo.</exception>
    /// <exception cref="ArgumentException">Já há handler para <typeparamref name="TParametro"/>, ou ele é abstrato.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void RegistrarOperacao<TParametro, TRetorno>(IOperacao<TParametro, TRetorno> operacao)
        where TParametro : ParametroWS<TRetorno>
        where TRetorno : class
    {
        ConferirAberta();
        ArgumentNullException.ThrowIfNull(operacao);
        if (typeof(TParametro).IsAbstract)
            throw new ArgumentException($"O parâmetro {typeof(TParametro).Name} é abstrato; a operação é registrada pelo tipo concreto.", nameof(operacao));
        if (_executoresTipo.ContainsKey(typeof(TParametro)))
            throw new ArgumentException($"Já há uma operação registrada para {typeof(TParametro).Name}.", nameof(operacao));

        var executor = new ExecutorOperacao<TParametro, TRetorno>(operacao);
        _executores.Add(executor);
        _executoresTipo.Add(typeof(TParametro), executor);
    }

    /// <summary>
    /// Declara que a UF, no tipo de emissão, é atendida pelo autorizador (AC, normal → SVRS; MG, SVC-AN → SVC-AN).
    /// </summary>
    /// <param name="uf">A UF.</param>
    /// <param name="emissao">O tipo de emissão.</param>
    /// <param name="autorizador">O autorizador.</param>
    /// <exception cref="ArgumentOutOfRangeException">Um dos valores não é conhecido.</exception>
    /// <exception cref="ArgumentException">A UF já tem atendimento declarado nesse tipo de emissão.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void DeclararAtendimento(UF uf, TipoEmissao emissao, Destino autorizador)
    {
        ConferirAberta();
        if (!Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(uf), uf, "O valor não é uma UF.");
        if (!Enum.IsDefined(emissao))
            throw new ArgumentOutOfRangeException(nameof(emissao), emissao, "O valor não é um tipo de emissão.");
        if (!Enum.IsDefined(autorizador))
            throw new ArgumentOutOfRangeException(nameof(autorizador), autorizador, "O valor não é um destino.");
        var atendimento = new Atendimento(uf, emissao, autorizador);
        if (!_atendimentosChave.TryAdd((uf, emissao), atendimento))
            throw new ArgumentException($"A UF {uf} já tem atendimento declarado no tipo de emissão {emissao}.", nameof(uf));
        _atendimentos.Add(atendimento);
    }

    /// <summary>Declara a URL de um serviço num autorizador e num ambiente.</summary>
    /// <param name="endereco">O endereço.</param>
    /// <exception cref="ArgumentNullException">O endereço é nulo.</exception>
    /// <exception cref="ArgumentException">Já há URL declarada para o autorizador, o ambiente e o serviço.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void DeclararEndereco(EnderecoServico endereco)
    {
        ConferirAberta();
        ArgumentNullException.ThrowIfNull(endereco);
        if (!_enderecosChave.Add(endereco.Chave))
            throw new ArgumentException($"Já há URL declarada para {endereco.Servico} em {endereco.Autorizador.Sigla()}, {endereco.Ambiente}.", nameof(endereco));
        _enderecos.Add(endereco);
    }

    /// <summary>
    /// Declara o envelope SOAP de um serviço: o namespace do WSDL e a ação (PROJETO.md 8.2). Toda URL, do leiaute
    /// ou trocada por quem usa, precisa do serviço dela declarado (<see cref="DFe.Criar"/> confere).
    /// </summary>
    /// <param name="servico">O serviço.</param>
    /// <exception cref="ArgumentNullException">O serviço é nulo.</exception>
    /// <exception cref="ArgumentException">O serviço, pelo nome, já foi declarado.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void DeclararServico(ServicoSoap servico)
    {
        ConferirAberta();
        ArgumentNullException.ThrowIfNull(servico);
        if (!_servicosNome.TryAdd(servico.Nome, servico))
            throw new ArgumentException($"O serviço {servico.Nome} já foi declarado.", nameof(servico));
        _servicos.Add(servico);
    }

    /// <summary>
    /// Declara as cifras de TLS que o autorizador negocia, quando ele só aceita cifras que a máquina não oferece por
    /// padrão (PROJETO.md 8.5): o pool usa um handler próprio para ele, sem rebaixar o padrão da máquina inteira.
    /// Só vale no Linux; no Windows, o .NET não aplica a política de cifras e vale o padrão do sistema.
    /// </summary>
    /// <param name="autorizador">O autorizador.</param>
    /// <param name="cifras">As cifras, na ordem de preferência.</param>
    /// <exception cref="ArgumentOutOfRangeException">O autorizador não é conhecido.</exception>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">A lista está vazia, ou o autorizador já tem cifras declaradas.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void DeclararCifras(Destino autorizador, IEnumerable<TlsCipherSuite> cifras)
    {
        ConferirAberta();
        if (!Enum.IsDefined(autorizador))
            throw new ArgumentOutOfRangeException(nameof(autorizador), autorizador, "O valor não é um destino.");
        ArgumentNullException.ThrowIfNull(cifras);
        TlsCipherSuite[] lista = [.. cifras];
        if (lista.Length == 0)
            throw new ArgumentException("A lista de cifras está vazia.", nameof(cifras));
        if (!_cifras.TryAdd(autorizador, lista.AsReadOnly()))
            throw new ArgumentException($"O autorizador {autorizador.Sigla()} já tem cifras declaradas.", nameof(autorizador));
    }

    /// <summary>Declara uma forma de XML que o leiaute lê para o documento (PROJETO.md 3.3).</summary>
    /// <param name="forma">A forma: a raiz e os caminhos da versão e do modelo.</param>
    /// <exception cref="ArgumentNullException">A forma é nula.</exception>
    /// <exception cref="ArgumentException">A mesma raiz, no mesmo namespace, já foi declarada.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    public void DeclararLeitura(FormaLeitura forma)
    {
        ConferirAberta();
        ArgumentNullException.ThrowIfNull(forma);
        if (_leituras.Any(f => f.EspacoNomes == forma.EspacoNomes && f.Raiz == forma.Raiz))
            throw new ArgumentException($"A raiz {forma.Raiz} já foi declarada no namespace {forma.EspacoNomes}.", nameof(forma));
        _leituras.Add(forma);
    }

    /// <summary>Declara o tipo da configuração do documento (<c>ConfiguracaoNFCe</c>).</summary>
    /// <typeparam name="TConfiguracao">O tipo, derivado de <see cref="ConfiguracaoDocumento"/>.</typeparam>
    /// <param name="obrigatoria">Se o núcleo recusa a configuração sem ela.</param>
    /// <exception cref="InvalidOperationException">
    /// Fora de <see cref="ILeiaute.Declarar"/>, ou a configuração do documento já foi declarada.
    /// </exception>
    public void DeclararConfiguracaoDocumento<TConfiguracao>(bool obrigatoria)
        where TConfiguracao : ConfiguracaoDocumento
    {
        ConferirAberta();
        if (_configuracaoDeclarada)
            throw new InvalidOperationException("A configuração do documento já foi declarada.");
        _configuracaoDeclarada = true;
        TipoConfiguracaoDocumento = typeof(TConfiguracao);
        ConfiguracaoDocumentoObrigatoria = obrigatoria;
    }

    /// <summary>
    /// Declara as regras do leiaute (PROJETO.md 6.2) para os documentos dos pedidos (<see cref="PedidoWS.Documentos"/>):
    /// o núcleo chama a função uma vez por documento, depois da validação pelo schema e só sem erro nela, com o mesmo
    /// <see cref="DocumentoVerificado"/> que os callbacks de quem usa recebem.
    /// </summary>
    /// <param name="regras">
    /// A função: o documento e o contexto entram, os problemas saem (vazio = nenhum). Segura para uso concorrente; a
    /// exceção dela sobe do <see cref="DFe.ExecutarAsync{TRetorno}"/> como veio.
    /// </param>
    /// <exception cref="ArgumentNullException">A função é nula.</exception>
    /// <exception cref="InvalidOperationException">Fora de <see cref="ILeiaute.Declarar"/>.</exception>
    /// <remarks>Pode ser chamado mais de uma vez: as funções rodam na ordem da declaração.</remarks>
    public void DeclararRegras(Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>> regras)
    {
        ConferirAberta();
        ArgumentNullException.ThrowIfNull(regras);
        _regras.Add(regras);
    }

    /// <summary>As funções de regras declaradas, na ordem.</summary>
    internal IReadOnlyList<Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>>> Regras => _regras;

    internal bool TentarExecutor(Type parametro, out ExecutorOperacao? executor) =>
        _executoresTipo.TryGetValue(parametro, out executor);

    internal bool TentarServico(string nome, out ServicoSoap? servico) =>
        _servicosNome.TryGetValue(nome, out servico);

    internal IReadOnlyList<TlsCipherSuite>? CifrasAutorizador(Destino autorizador) =>
        _cifras.GetValueOrDefault(autorizador);

    internal bool TentarAtendimento(UF uf, TipoEmissao emissao, out Atendimento? atendimento) =>
        _atendimentosChave.TryGetValue((uf, emissao), out atendimento);

    private void Fechar()
    {
        _operacoes = [.. _executores.Select(e => e.Operacao)];
        _aberta = false;
    }

    private void ConferirAberta()
    {
        if (!_aberta)
            throw new InvalidOperationException("A declaração do leiaute já foi encerrada: só vale durante o ILeiaute.Declarar.");
    }
}
