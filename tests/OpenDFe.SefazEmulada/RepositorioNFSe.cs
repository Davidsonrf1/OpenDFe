using System.Globalization;
using System.Security.Cryptography;

namespace OpenDFe.SefazEmulada;

// Uma NFS-e gerada pela Sefin emulada: a chave, o Id da DPS que a originou, o número, o processamento e o XML assinado
// (o NFSe, com a DPS dentro).
internal sealed record NFSeRegistrada(
    string Chave, string IdDps, long Numero, long NumeroDfse, int Municipio, string Prestador, DateTimeOffset Processamento, string Xml);

// Um evento vinculado a uma NFS-e (E.8): a chave, o tipo (101101, ...), o nSeqEvento, o Id (EVT ...), o nDFSe, o
// processamento e o XML assinado (o evento, com o pedRegEvento dentro).
internal sealed record EventoNFSeRegistrado(
    string Chave, string Tipo, int Sequencia, string Id, long NumeroDfse, DateTimeOffset Processamento, string Xml);

// A substituição pedida numa DPS (E.8): a chave da substituída, a conferência dos eventos dela (o código do erro, ou
// nulo) e o 105102 que a Sefin gera sobre ela com a substituta, dado o nDFSe.
internal sealed record SubstituicaoPedida(
    string Chave, Func<IReadOnlyList<EventoNFSeRegistrado>, ErroSefin?> Conferir, Func<NFSeRegistrada, long, EventoNFSeRegistrado> Evento);

// As NFS-e geradas, pela chave e pelo Id da DPS, os eventos de cada uma, e as numerações: o nNFSe por município e
// inscrição, o nDFSe do ambiente (as NFS-e e os eventos são DF-e, na mesma sequência) e o código numérico da chave. Em
// memória, vazio a cada partida (a persistência é da E.9); seguro para uso concorrente. O Gerar devolve a NFS-e que já
// existia para a DPS, se outra chegou antes (a duplicidade, E0014).
internal sealed class RepositorioNFSe
{
    private readonly Lock _trava = new();
    private readonly Dictionary<string, NFSeRegistrada> _chaves = new(StringComparer.Ordinal);
    private readonly Dictionary<string, NFSeRegistrada> _dps = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<EventoNFSeRegistrado>> _eventos = new(StringComparer.Ordinal);
    private readonly List<NFSeRegistrada> _ordem = [];
    private readonly Dictionary<(int, string), long> _numeros = [];
    private long _documentos;

    public NFSeRegistrada? Chave(string chave)
    {
        lock (_trava)
            return _chaves.GetValueOrDefault(chave);
    }

    public NFSeRegistrada? Dps(string id)
    {
        lock (_trava)
            return _dps.GetValueOrDefault(id);
    }

    // Os eventos da NFS-e, na ordem do registro; vazio sem eventos ou sem a nota.
    public IReadOnlyList<EventoNFSeRegistrado> Eventos(string chave)
    {
        lock (_trava)
            return _eventos.TryGetValue(chave, out var eventos) ? [.. eventos] : [];
    }

    // Gera a NFS-e com os números reservados aqui, debaixo da trava, para duas DPS iguais ao mesmo tempo não gerarem duas
    // notas: a segunda recebe a primeira, como Existente. Com a substituição, confere os eventos da substituída e registra
    // o 105102 sobre ela na mesma trava: duas substituições da mesma nota não passam as duas.
    public (NFSeRegistrada? Gerada, NFSeRegistrada? Existente, ErroSefin? Erro) Gerar(string idDps, int municipio, string prestador,
        Func<long, long, int, NFSeRegistrada> gerar, SubstituicaoPedida? substituicao = null)
    {
        ArgumentNullException.ThrowIfNull(gerar);
        lock (_trava)
        {
            if (_dps.TryGetValue(idDps, out var existente))
                return (null, existente, null);
            if (substituicao is not null)
            {
                if (!_chaves.ContainsKey(substituicao.Chave))
                    return (null, null, ErroSefin.Criar(MotivosNFSe.NFSeInexistente));
                if (substituicao.Conferir(EventosSemTrava(substituicao.Chave)) is { } erro)
                    return (null, null, erro);
            }
            var numero = _numeros.GetValueOrDefault((municipio, prestador)) + 1;
            var nota = gerar(numero, _documentos + 1, RandomNumberGenerator.GetInt32(1, 1_000_000_000));
            _numeros[(municipio, prestador)] = numero;
            _documentos++;
            _chaves[nota.Chave] = nota;
            _dps[idDps] = nota;
            _ordem.Add(nota);
            if (substituicao is not null)
                Acrescentar(substituicao.Evento(nota, ++_documentos));
            return (nota, null, null);
        }
    }

    // Registra um evento sobre a NFS-e: confere os eventos que ela já tem (o erro, ou nulo) e gera o novo com o nDFSe e o
    // nSeqEvento (o seguinte do mesmo tipo), na mesma trava. Erro E1831 sem a nota.
    public (EventoNFSeRegistrado? Evento, ErroSefin? Erro) RegistrarEvento(string chave,
        Func<NFSeRegistrada, IReadOnlyList<EventoNFSeRegistrado>, ErroSefin?> conferir, Func<NFSeRegistrada, long, Func<string, int>, EventoNFSeRegistrado> gerar)
    {
        ArgumentNullException.ThrowIfNull(conferir);
        ArgumentNullException.ThrowIfNull(gerar);
        lock (_trava)
        {
            if (!_chaves.TryGetValue(chave, out var nota))
                return (null, ErroSefin.Criar(MotivosNFSe.EventoNFSeInexistente));
            var eventos = EventosSemTrava(chave);
            if (conferir(nota, eventos) is { } erro)
                return (null, erro);
            var evento = gerar(nota, _documentos + 1, tipo => eventos.Count(e => e.Tipo == tipo) + 1);
            _documentos++;
            Acrescentar(evento);
            return (evento, null);
        }
    }

    public IReadOnlyList<NFSeRegistrada> Listar()
    {
        lock (_trava)
            return [.. _ordem];
    }

    // Esquece as notas e os eventos; as numerações continuam, como a sequência dos protocolos da NF-e.
    public void Limpar()
    {
        lock (_trava)
        {
            _chaves.Clear();
            _dps.Clear();
            _eventos.Clear();
            _ordem.Clear();
        }
    }

    // O código numérico de 9 posições, com zeros à esquerda.
    public static string Codigo(int codigo) => codigo.ToString("D9", CultureInfo.InvariantCulture);

    private List<EventoNFSeRegistrado> EventosSemTrava(string chave) => _eventos.TryGetValue(chave, out var eventos) ? [.. eventos] : [];

    private void Acrescentar(EventoNFSeRegistrado evento)
    {
        if (!_eventos.TryGetValue(evento.Chave, out var eventos))
            _eventos[evento.Chave] = eventos = [];
        eventos.Add(evento);
    }
}
