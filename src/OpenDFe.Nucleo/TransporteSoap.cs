using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace OpenDFe;

/// <summary>
/// O transporte real do <see cref="ConexoesSefaz"/> (PROJETO.md 8.2 e 8.5): o pool de handlers por certificado e
/// política de cifras, o POST do envelope SOAP 1.2 e a leitura da resposta, com cada falha classificada
/// (<see cref="FalhaTransporte"/>).
/// </summary>
internal sealed class TransporteSoap : ITransporteSefaz, IDisposable
{
    private readonly ConexoesSefaz _dono;
    private readonly Func<ConexaoSefaz, HttpMessageHandler> _fabrica;
    private readonly TimeProvider _relogio;
    private readonly Lock _trava = new();
    private readonly Dictionary<string, Entrada> _entradas = new(StringComparer.Ordinal);
    private bool _descartado;

    public TransporteSoap(ConexoesSefaz dono, Func<ConexaoSefaz, HttpMessageHandler> fabrica, TimeProvider relogio)
    {
        _dono = dono;
        _fabrica = fabrica;
        _relogio = relogio;
    }

    /// <summary>Quantos handlers o pool tem agora (para os testes).</summary>
    internal int Handlers
    {
        get
        {
            lock (_trava)
                return _entradas.Count;
        }
    }

    public async Task<string> EnviarAsync(EnvioSefaz envio, CancellationToken cancellationToken)
    {
        var entrada = Pegar(envio.Certificado, envio.Cifras);
        try
        {
            using var tempo = new CancellationTokenSource(_dono.TempoResposta, _relogio);
            using var ligado = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, tempo.Token);
            using var pedido = new HttpRequestMessage(HttpMethod.Post, envio.Url)
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                Content = new ConteudoSoap(envio),
            };

            HttpStatusCode status;
            string corpo;
            try
            {
                using var resposta = await entrada.Invocador.SendAsync(pedido, ligado.Token).ConfigureAwait(false);
                status = resposta.StatusCode;
                corpo = await resposta.Content.ReadAsStringAsync(ligado.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException e) when (tempo.IsCancellationRequested)
            {
                throw new FalhaTransporte(TipoFalhaTransmissao.TempoEsgotado, envio.Enviado,
                    $"o autorizador não respondeu em {_dono.TempoResposta.TotalSeconds:0.###} s", e);
            }
            catch (OperationCanceledException e)
            {
                // O ConnectTimeout do handler.
                throw new FalhaTransporte(TipoFalhaTransmissao.TempoEsgotado, envio.Enviado, "a conexão não abriu a tempo", e);
            }
            catch (HttpRequestException e)
            {
                throw Classificar(e, envio);
            }
            catch (IOException e)
            {
                throw new FalhaTransporte(TipoFalhaTransmissao.Conexao, envio.Enviado, e.Message, e);
            }
            return Interpretar(status, corpo, envio);
        }
        finally
        {
            Devolver(entrada);
        }
    }

    public void Dispose()
    {
        List<Entrada> descartar;
        lock (_trava)
        {
            _descartado = true;
            descartar = [.. _entradas.Values.Where(e => e.Ativos == 0)];
            foreach (var entrada in _entradas.Values)
                entrada.Removida = true;
            _entradas.Clear();
        }
        foreach (var entrada in descartar)
            entrada.Dispose();
    }

    // A resposta HTTP: a mensagem do nfeResultMsg no 200, ou a falha classificada.
    private static string Interpretar(HttpStatusCode status, string corpo, EnvioSefaz envio)
    {
        var codigo = (int)status;
        if (EnvelopeSoap.PareceEnvelope(corpo))
        {
            try
            {
                var mensagem = EnvelopeSoap.Ler(corpo, envio.Soap);
                if (status == HttpStatusCode.OK)
                    return mensagem;
            }
            catch (FalhaSoapException e)
            {
                throw new FalhaTransporte(TipoFalhaTransmissao.FalhaSoap, !e.Remetente && envio.Enviado, e.Message, e, corpo);
            }
            catch (Exception e) when (e is XmlException or InvalidDataException && status == HttpStatusCode.OK)
            {
                throw new FalhaTransporte(TipoFalhaTransmissao.RespostaIlegivel, envio.Enviado, e.Message, e, corpo);
            }
            catch (Exception e) when (e is XmlException or InvalidDataException)
            {
                // Não 200 e o envelope ilegível: vale o HTTP, abaixo.
            }
        }
        else if (status == HttpStatusCode.OK)
        {
            throw new FalhaTransporte(TipoFalhaTransmissao.RespostaIlegivel, envio.Enviado,
                "a resposta não é um envelope SOAP 1.2", new InvalidDataException("A resposta não é um envelope SOAP 1.2."), corpo);
        }

        var causa = new HttpRequestException($"HTTP {codigo} ({status}).", null, status);
        if (status == HttpStatusCode.Forbidden)
        {
            throw new FalhaTransporte(TipoFalhaTransmissao.CertificadoRecusado, false,
                "HTTP 403: o autorizador recusou o certificado do cliente", causa, corpo);
        }
        // 4xx: o autorizador recusou o pedido sem processá-lo; 5xx e os outros podem ter sido processados.
        var processado = codigo is < 400 or > 499 && envio.Enviado;
        throw new FalhaTransporte(TipoFalhaTransmissao.Http, processado, $"HTTP {codigo} ({status}) sem o envelope SOAP", causa, corpo);
    }

    private static FalhaTransporte Classificar(HttpRequestException e, EnvioSefaz envio)
    {
        if (e.InnerException is TimeoutException)
            return new FalhaTransporte(TipoFalhaTransmissao.TempoEsgotado, envio.Enviado, e.Message, e);
        return e.HttpRequestError switch
        {
            HttpRequestError.NameResolutionError => new FalhaTransporte(TipoFalhaTransmissao.Conexao, false, e.Message, e),
            HttpRequestError.SecureConnectionError => new FalhaTransporte(TipoFalhaTransmissao.Tls, false, e.Message, e),
            HttpRequestError.InvalidResponse => new FalhaTransporte(TipoFalhaTransmissao.RespostaIlegivel, envio.Enviado, e.Message, e),
            _ => new FalhaTransporte(TipoFalhaTransmissao.Conexao, envio.Enviado, e.Message, e),
        };
    }

    // O handler do certificado e das cifras, criado se não há; antes, descarta os ociosos.
    private Entrada Pegar(X509Certificate2 certificado, IReadOnlyList<TlsCipherSuite>? cifras)
    {
        var chave = certificado.GetCertHashString(HashAlgorithmName.SHA256)
            + "|" + (cifras is null ? "" : string.Join(',', cifras.Select(c => (int)c)));
        List<Entrada> ociosas = [];
        Entrada? entrada;
        try
        {
            lock (_trava)
            {
                ObjectDisposedException.ThrowIf(_descartado, typeof(ConexoesSefaz));
                var agora = _relogio.GetTimestamp();
                foreach (var (outra, ociosa) in _entradas.Where(p => p.Value.Ativos == 0 && _relogio.GetElapsedTime(p.Value.UltimoUso, agora) > _dono.TempoOcioso).ToList())
                {
                    _entradas.Remove(outra);
                    ociosas.Add(ociosa);
                }
                if (!_entradas.TryGetValue(chave, out entrada))
                {
                    entrada = Criar(certificado, cifras);
                    _entradas.Add(chave, entrada);
                }
                entrada.Ativos++;
            }
        }
        finally
        {
            foreach (var ociosa in ociosas)
                ociosa.Dispose();
        }
        return entrada;
    }

    private Entrada Criar(X509Certificate2 certificado, IReadOnlyList<TlsCipherSuite>? cifras)
    {
        // A cópia vive com o handler: quem usa pode descartar o seu certificado.
        var copia = new X509Certificate2(certificado);
        try
        {
            var handler = _fabrica(new ConexaoSefaz(copia, cifras, _dono.TempoConexao))
                ?? throw new InvalidOperationException("A fábrica de handlers do ConexoesSefaz devolveu nulo.");
            return new Entrada(new HttpMessageInvoker(handler, disposeHandler: true), copia);
        }
        catch
        {
            copia.Dispose();
            throw;
        }
    }

    private void Devolver(Entrada entrada)
    {
        bool descartar;
        lock (_trava)
        {
            entrada.Ativos--;
            entrada.UltimoUso = _relogio.GetTimestamp();
            descartar = entrada.Removida && entrada.Ativos == 0;
        }
        if (descartar)
            entrada.Dispose();
    }

    private sealed class Entrada(HttpMessageInvoker invocador, X509Certificate2 certificado) : IDisposable
    {
        public HttpMessageInvoker Invocador { get; } = invocador;

        // Sob a trava do transporte.
        public int Ativos { get; set; }

        public long UltimoUso { get; set; }

        public bool Removida { get; set; }

        public void Dispose()
        {
            Invocador.Dispose();
            certificado.Dispose();
        }
    }

    // O envelope em UTF-8, com a ação no Content-Type; marca o envio quando o corpo foi escrito inteiro.
    private sealed class ConteudoSoap : HttpContent
    {
        private readonly EnvioSefaz _envio;
        private readonly byte[] _bytes;

        public ConteudoSoap(EnvioSefaz envio)
        {
            _envio = envio;
            _bytes = Encoding.UTF8.GetBytes(EnvelopeSoap.Montar(envio.Soap, envio.Xml));
            var tipo = new MediaTypeHeaderValue("application/soap+xml") { CharSet = "utf-8" };
            tipo.Parameters.Add(new NameValueHeaderValue("action", $"\"{envio.Soap.Acao}\""));
            Headers.ContentType = tipo;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            await stream.WriteAsync(_bytes, cancellationToken).ConfigureAwait(false);
            _envio.MarcarEnviado();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _bytes.Length;
            return true;
        }
    }
}

/// <summary>
/// Uma falha do <see cref="TransporteSoap"/>, já classificada; o núcleo a transforma na
/// <see cref="FalhaTransmissaoException"/>, com o corpo recebido no rastro.
/// </summary>
internal sealed class FalhaTransporte(
    TipoFalhaTransmissao tipo, bool podeTerSidoProcessado, string mensagem, Exception causa, string? resposta = null)
    : Exception(mensagem, causa)
{
    public TipoFalhaTransmissao Tipo { get; } = tipo;

    public bool PodeTerSidoProcessado { get; } = podeTerSidoProcessado;

    /// <summary>O corpo da resposta, quando veio.</summary>
    public string? Resposta { get; } = resposta;
}
