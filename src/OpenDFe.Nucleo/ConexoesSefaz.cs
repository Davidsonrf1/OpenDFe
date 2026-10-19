namespace OpenDFe;

/// <summary>
/// O pool de conexões com os autorizadores: um <see cref="HttpMessageHandler"/> por certificado (PROJETO.md
/// 8.5), com as ACs raiz da ICP-Brasil. Um por processo (ou por contêiner de injeção de dependência),
/// compartilhado por todos os núcleos (<see cref="ConfiguracaoDFe.Conexoes"/>).
/// </summary>
/// <remarks>
/// <para>
/// É de quem cria, que o descarta depois do último núcleo: o núcleo só o referencia. Descartá-lo com um núcleo
/// em uso é erro de quem chama (<see cref="ObjectDisposedException"/> na chamada seguinte). Seguro para uso
/// concorrente.
/// </para>
/// <para>
/// A transmissão SOAP 1.2 com o TLS da ICP-Brasil entra na etapa 4.1 do plano: até lá, toda transmissão por
/// esta instância falha com <see cref="FalhaTransmissaoException"/> (a causa, uma
/// <see cref="NotSupportedException"/>, em <see cref="Exception.InnerException"/>).
/// </para>
/// </remarks>
public sealed class ConexoesSefaz : IDisposable
{
    private readonly ITransporteSefaz _transporte;
    private int _descartado;

    /// <summary>Cria o pool, vazio: as conexões são abertas na primeira transmissão de cada certificado.</summary>
    public ConexoesSefaz()
        : this(new TransportePendente())
    {
    }

    // A costura do transporte: os testes injetam um transporte falso (até a 4.1, o único que transmite).
    internal ConexoesSefaz(ITransporteSefaz transporte)
    {
        _transporte = transporte;
    }

    internal bool Descartado => Volatile.Read(ref _descartado) != 0;

    /// <summary>Fecha as conexões. Chamar de novo não faz nada.</summary>
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

    // Até a 4.1: nada transmite.
    private sealed class TransportePendente : ITransporteSefaz
    {
        public Task<string> EnviarAsync(EnvioSefaz envio, CancellationToken cancellationToken) =>
            Task.FromException<string>(new NotSupportedException("A transmissão SOAP com a SEFAZ entra na etapa 4.1 da OpenDFe."));
    }
}
