namespace OpenDFe.SefazEmulada;

// As falhas que a rota /_controle comanda (PROJETO.md 13.5).
internal enum TipoFalha
{
    // Espera e depois responde normalmente.
    Atraso = 1,

    // Nunca responde: segura o pedido até o cliente desistir (ou o emulador parar).
    TempoEsgotado = 2,

    // Derruba a conexão sem resposta: antes de processar ou, com Depois, depois de processar.
    ConexaoDerrubada = 3,

    // HTTP 200 com o envelope cortado ao meio.
    RespostaIlegivel = 4,

    // HTTP 500 com a falha SOAP (env:Receiver).
    FalhaSoap = 5,

    // O serviço paralisado: 108 (o padrão) ou 109 no cStat (Anexo I, B03 e B04).
    Paralisacao = 6,

    // A consulta do recibo responde 105, como se o lote ainda estivesse na fila (NFeRetAutorizacao4, E04).
    LoteProcessando = 7,
}

// O pedido de falha, como chega em JSON no POST /_controle/falhas. Destino e serviço nulos valem para
// todos; vezes nulo vale até a falha ser removida.
internal sealed record PedidoFalha(
    TipoFalha Tipo,
    string? Destino = null,
    string? Servico = null,
    int? Milissegundos = null,
    int? Vezes = null,
    string? Codigo = null,
    string? Observacao = null,
    bool Depois = false);

// Uma falha ativa. Restantes nulo: até ser removida.
internal sealed record FalhaComandada(
    int Id, TipoFalha Tipo, string? Destino, string? Servico, int? Milissegundos, int? Restantes, string? Codigo, string? Observacao, bool Depois)
{
    public bool Vale(string destino, string servico) =>
        (Destino is null || string.Equals(Destino, destino, StringComparison.OrdinalIgnoreCase))
        && (Servico is null || string.Equals(Servico, servico, StringComparison.OrdinalIgnoreCase));
}

// O estado comandado pelos testes: as falhas ativas. Em memória, vazio a cada partida; seguro para uso
// concorrente.
internal sealed class Controle
{
    private readonly Lock _trava = new();
    private readonly List<FalhaComandada> _falhas = [];
    private int _ultimoId;

    // Confere o pedido (ArgumentException com o que está errado) e acrescenta a falha.
    public FalhaComandada Acrescentar(PedidoFalha pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        if (!Enum.IsDefined(pedido.Tipo))
            throw new ArgumentException("Tipo de falha desconhecido.", nameof(pedido));
        if (pedido.Destino is not null && Autorizador.Procurar(pedido.Destino) is null)
            throw new ArgumentException($"Destino desconhecido: {pedido.Destino}.", nameof(pedido));
        if (pedido.Servico is not null && !Autorizador.Todos.Any(a => a.Servicos.Contains(pedido.Servico, StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException($"Serviço desconhecido: {pedido.Servico}.", nameof(pedido));
        if (pedido.Vezes is < 1)
            throw new ArgumentException("Vezes deve ser ao menos 1 (ou omitido, para valer até a remoção).", nameof(pedido));
        if (pedido.Tipo == TipoFalha.Atraso && pedido.Milissegundos is not > 0)
            throw new ArgumentException("O atraso precisa de milissegundos maior que zero.", nameof(pedido));
        if (pedido.Tipo != TipoFalha.Atraso && pedido.Milissegundos is not null)
            throw new ArgumentException("Milissegundos só vale para o atraso.", nameof(pedido));
        if (pedido.Depois && pedido.Tipo != TipoFalha.ConexaoDerrubada)
            throw new ArgumentException("Depois só vale para a conexão derrubada.", nameof(pedido));
        if (pedido.Tipo == TipoFalha.LoteProcessando && pedido.Servico is not null
            && !string.Equals(pedido.Servico, Servico.NomeRetAutorizacao, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"O lote em processamento só vale para o {Servico.NomeRetAutorizacao}.", nameof(pedido));
        var codigo = pedido.Codigo;
        if (pedido.Tipo == TipoFalha.Paralisacao)
        {
            codigo ??= Motivos.ParalisadoMomentaneamente;
            if (codigo is not (Motivos.ParalisadoMomentaneamente or Motivos.ParalisadoIndefinidamente))
                throw new ArgumentException("A paralisação é 108 ou 109.", nameof(pedido));
        }
        else if (codigo is not null || pedido.Observacao is not null)
        {
            throw new ArgumentException("Código e observação só valem para a paralisação.", nameof(pedido));
        }

        lock (_trava)
        {
            var falha = new FalhaComandada(++_ultimoId, pedido.Tipo, pedido.Destino, pedido.Servico, pedido.Milissegundos,
                pedido.Vezes, codigo, pedido.Observacao, pedido.Depois);
            _falhas.Add(falha);
            return falha;
        }
    }

    public IReadOnlyList<FalhaComandada> Listar()
    {
        lock (_trava)
            return [.. _falhas];
    }

    public bool Remover(int id)
    {
        lock (_trava)
            return _falhas.RemoveAll(f => f.Id == id) > 0;
    }

    public void Limpar()
    {
        lock (_trava)
            _falhas.Clear();
    }

    // As falhas que valem para o pedido, já descontadas: a primeira de transporte, a primeira paralisação e,
    // na consulta do recibo, o primeiro lote em processamento, na ordem em que foram comandadas. Transporte ou
    // paralisação falsos: o pedido já tem a sua (a marca no infCpl), e a do controle fica para o próximo.
    public (FalhaComandada? Transporte, FalhaComandada? Paralisacao, FalhaComandada? Processamento) Consumir(
        string destino, string servico, bool transporte = true, bool paralisacao = true)
    {
        lock (_trava)
        {
            return (
                transporte ? Tirar(f => f.Tipo is not (TipoFalha.Paralisacao or TipoFalha.LoteProcessando) && f.Vale(destino, servico)) : null,
                paralisacao ? Tirar(f => f.Tipo == TipoFalha.Paralisacao && f.Vale(destino, servico)) : null,
                servico == Servico.NomeRetAutorizacao ? Tirar(f => f.Tipo == TipoFalha.LoteProcessando && f.Vale(destino, servico)) : null);
        }
    }

    private FalhaComandada? Tirar(Func<FalhaComandada, bool> criterio)
    {
        var indice = _falhas.FindIndex(f => criterio(f));
        if (indice < 0)
            return null;
        var falha = _falhas[indice];
        if (falha.Restantes is { } restantes)
        {
            if (restantes <= 1)
                _falhas.RemoveAt(indice);
            else
                _falhas[indice] = falha with { Restantes = restantes - 1 };
        }
        return falha;
    }
}
