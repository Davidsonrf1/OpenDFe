namespace OpenDFe;

/// <summary>
/// O pool de conexões com os autorizadores (PROJETO.md 8.5): um <see cref="HttpMessageHandler"/> por certificado e
/// política de cifras, que transmite em SOAP 1.2 com TLS mútuo. Um por processo (ou por contêiner de injeção de
/// dependência), compartilhado por todos os núcleos (<see cref="ConfiguracaoDFe.Conexoes"/>).
/// </summary>
/// <remarks>
/// <para>
/// É de quem cria, que o descarta depois do último núcleo: o núcleo só o referencia. Descartá-lo com um núcleo
/// em uso é erro de quem chama (<see cref="ObjectDisposedException"/> na chamada seguinte). Seguro para uso
/// concorrente.
/// </para>
/// <para>
/// O handler é criado na primeira transmissão de cada certificado (pela impressão SHA-256) e descartado depois de
/// <see cref="TempoOcioso"/> sem uso, conferido a cada transmissão, sem timer; um handler em uso nunca é descartado.
/// </para>
/// </remarks>
public sealed class ConexoesSefaz : IDisposable
{
    private readonly ITransporteSefaz _transporte;
    private readonly TimeSpan _tempoConexao = TimeSpan.FromSeconds(15);
    private readonly TimeSpan _tempoResposta = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _tempoOcioso = TimeSpan.FromMinutes(30);
    private int _descartado;

    /// <summary>Cria o pool, vazio, com o handler da lib (<see cref="ConexaoSefaz.CriarPadrao"/>).</summary>
    public ConexoesSefaz()
        : this(c => c.CriarPadrao())
    {
    }

    /// <summary>Cria o pool, vazio, com a fábrica de handlers de quem usa (PROJETO.md 2.3).</summary>
    /// <param name="fabrica">
    /// A fábrica: recebe o certificado e as cifras e devolve o handler, que passa a ser do pool (descartado por ele).
    /// Chamada uma vez por certificado e política de cifras, dentro da trava do pool: rápida e sem I/O.
    /// </param>
    /// <exception cref="ArgumentNullException">A fábrica é nula.</exception>
    public ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler> fabrica)
        : this(fabrica, TimeProvider.System)
    {
    }

    // O relógio do tempo ocioso e do tempo de resposta: os testes o trocam.
    internal ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler> fabrica, TimeProvider relogio)
    {
        ArgumentNullException.ThrowIfNull(fabrica);
        _transporte = new TransporteSoap(this, fabrica, relogio);
    }

    // A costura do transporte: os testes do núcleo injetam um transporte falso.
    internal ConexoesSefaz(ITransporteSefaz transporte)
    {
        _transporte = transporte;
    }

    /// <summary>O tempo para abrir a conexão, com o handshake; o padrão é 15 segundos.</summary>
    /// <exception cref="ArgumentOutOfRangeException">O tempo não é positivo.</exception>
    public TimeSpan TempoConexao
    {
        get => _tempoConexao;
        init => _tempoConexao = Positivo(value);
    }

    /// <summary>
    /// O tempo de cada transmissão, do envio ao fim da resposta (com a conexão); o padrão é 60 segundos. Esgotado,
    /// <see cref="FalhaTransmissaoException"/> com <see cref="TipoFalhaTransmissao.TempoEsgotado"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O tempo não é positivo.</exception>
    public TimeSpan TempoResposta
    {
        get => _tempoResposta;
        init => _tempoResposta = Positivo(value);
    }

    /// <summary>
    /// Depois de quanto tempo sem uso o handler de um certificado é descartado; o padrão é 30 minutos. No ERP com
    /// muitos certificados, é o que libera os que não transmitem mais.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O tempo não é positivo.</exception>
    public TimeSpan TempoOcioso
    {
        get => _tempoOcioso;
        init => _tempoOcioso = Positivo(value);
    }

    internal bool Descartado => Volatile.Read(ref _descartado) != 0;

    // O transporte real, para os testes do pool (também depois do descarte); nulo com o transporte falso.
    internal TransporteSoap? Soap => _transporte as TransporteSoap;

    /// <summary>Fecha as conexões e descarta os handlers. Chamar de novo não faz nada.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _descartado, 1) != 0)
            return;
        (_transporte as IDisposable)?.Dispose();
    }

    // O transporte, para o núcleo transmitir; ObjectDisposedException depois do descarte.
    internal ITransporteSefaz Transporte()
    {
        ObjectDisposedException.ThrowIf(Descartado, this);
        return _transporte;
    }

    private static TimeSpan Positivo(TimeSpan valor, [System.Runtime.CompilerServices.CallerMemberName] string nome = "")
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(valor, TimeSpan.Zero, nome);
        return valor;
    }
}
