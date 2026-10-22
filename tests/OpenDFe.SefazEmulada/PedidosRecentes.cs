using System.Text;
using System.Threading.Channels;

namespace OpenDFe.SefazEmulada;

// Os últimos pedidos aos Web Services, com o XML do pedido e da resposta como foram e voltaram (o painel, E.6),
// numa fila dos últimos N (OpcoesEmulador.Pedidos): o mais antigo sai quando chega o novo. Também avisa quem ouve
// (o GET /_controle/pedidos/fluxo, que o painel abre) de cada pedido novo e de cada mudança pela rota de controle.
// Em memória, vazio a cada partida; seguro para uso concorrente.
internal sealed class PedidosRecentes(OpcoesEmulador opcoes)
{
    private readonly Lock _trava = new();
    private readonly LinkedList<PedidoRegistrado> _pedidos = new();
    private readonly List<Channel<Aviso>> _ouvintes = [];
    private long _ultimoId;

    // Guarda o pedido e avisa. Status nulo: a conexão caiu ou nunca respondeu. Resposta: o que saiu (ou o que sairia,
    // na conexão derrubada depois de processar).
    public PedidoRegistrado Registrar(string destino, string servico, DateTimeOffset recebimento, string transmissor, byte[] pedido,
        int? status, byte[]? resposta, string situacao, TimeSpan duracao)
    {
        PedidoRegistrado registrado;
        lock (_trava)
        {
            registrado = new PedidoRegistrado(++_ultimoId, recebimento, destino, servico, transmissor, status, situacao,
                Math.Round(duracao.TotalMilliseconds, 1), Texto(pedido), resposta is null ? null : Texto(resposta));
            _pedidos.AddLast(registrado);
            while (_pedidos.Count > Math.Max(opcoes.Pedidos, 1))
                _pedidos.RemoveFirst();
        }
        Avisar(new Aviso("pedido", registrado.Resumo()));
        return registrado;
    }

    // Os resumos, do mais antigo ao mais novo; com depois, só os de id maior.
    public IReadOnlyList<PedidoResumo> Listar(long? depois = null)
    {
        lock (_trava)
            return [.. _pedidos.Where(p => depois is null || p.Id > depois).Select(p => p.Resumo())];
    }

    public PedidoRegistrado? Ler(long id)
    {
        lock (_trava)
            return _pedidos.FirstOrDefault(p => p.Id == id);
    }

    public void Limpar()
    {
        lock (_trava)
            _pedidos.Clear();
        Avisar(new Aviso("estado", null));
    }

    // A mudança feita pela rota de controle (uma falha, uma nota esquecida, um cadastro).
    public void AvisarEstado() => Avisar(new Aviso("estado", null));

    // Um ouvinte novo, até o cancelamento. Quem não lê a tempo perde os avisos mais antigos, nunca trava o emulador.
    public ChannelReader<Aviso> Ouvir(CancellationToken cancellationToken)
    {
        var canal = Channel.CreateBounded<Aviso>(new BoundedChannelOptions(256) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (_trava)
            _ouvintes.Add(canal);
        cancellationToken.Register(() =>
        {
            lock (_trava)
                _ouvintes.Remove(canal);
            canal.Writer.TryComplete();
        });
        return canal.Reader;
    }

    private void Avisar(Aviso aviso)
    {
        lock (_trava)
        {
            foreach (var ouvinte in _ouvintes)
                ouvinte.Writer.TryWrite(aviso);
        }
    }

    // O XML como texto, em UTF-8 (o pedido que não é UTF-8 sai com o caractere de substituição).
    private static string Texto(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}

// Um pedido guardado, com o XML do pedido (o corpo inteiro, com o envelope) e da resposta.
internal sealed record PedidoRegistrado(
    long Id, DateTimeOffset Recebimento, string Destino, string Servico, string Transmissor, int? Status, string Situacao,
    double Milissegundos, string Pedido, string? Resposta)
{
    public PedidoResumo Resumo() => new(Id, Recebimento, Destino, Servico, Transmissor, Status, Situacao, Milissegundos, Pedido.Length, Resposta?.Length);
}

// O pedido sem o XML, para a lista: os tamanhos em caracteres.
internal sealed record PedidoResumo(
    long Id, DateTimeOffset Recebimento, string Destino, string Servico, string Transmissor, int? Status, string Situacao,
    double Milissegundos, int TamanhoPedido, int? TamanhoResposta);

// O que o fluxo manda: o tipo do evento (pedido ou estado) e o resumo do pedido.
internal sealed record Aviso(string Tipo, PedidoResumo? Pedido);
