using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O que o emulador guarda entre os pedidos (PROJETO.md 13.5): as notas autorizadas, para a duplicidade, os
// lotes recebidos, para a consulta do recibo, os eventos registrados, as faixas inutilizadas e o que o Ambiente
// Nacional distribui a cada interessado (Interessados). Em memória,
// vazio a cada partida. Quem processa um lote segura
// a Trava do começo ao fim, para que as notas do mesmo lote e de lotes simultâneos se vejam na ordem.
internal sealed class Repositorio
{
    private readonly Dictionary<string, NotaRegistrada> _notas = new(StringComparer.Ordinal);
    private readonly List<NotaRegistrada> _ordem = [];
    private readonly Dictionary<string, string> _numeracoes = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Autorizador, string Recibo), LoteRecebido> _lotes = [];
    private readonly Dictionary<string, List<EventoRegistrado>> _eventos = new(StringComparer.Ordinal);
    private readonly List<FaixaInutilizada> _faixas = [];
    private long _sequencia;

    public Lock Trava { get; } = new();

    // Os documentos distribuídos e o consumo de cada interessado (NFeDistribuicaoDFe).
    public Interessados Interessados { get; } = new();

    // As consultas abaixo são chamadas com a Trava.

    public NotaRegistrada? Nota(string chave) => _notas.GetValueOrDefault(chave);

    public NotaRegistrada? NotaNumeracao(string numeracao) =>
        _numeracoes.TryGetValue(numeracao, out var chave) ? _notas[chave] : null;

    public void Registrar(NotaRegistrada nota)
    {
        _notas.Add(nota.Chave, nota);
        _numeracoes.Add(nota.Numeracao, nota.Chave);
        _ordem.Add(nota);
    }

    // A nota passa a cancelada (o evento de cancelamento registrado).
    public void Cancelar(string chave)
    {
        var cancelada = _notas[chave] with { Cancelada = true };
        _notas[chave] = cancelada;
        _ordem[_ordem.FindIndex(n => n.Chave == chave)] = cancelada;
    }

    // Os eventos da nota, na ordem do registro.
    public IReadOnlyList<EventoRegistrado> Eventos(string chave) =>
        _eventos.TryGetValue(chave, out var eventos) ? eventos : [];

    public void Registrar(EventoRegistrado evento)
    {
        if (!_eventos.TryGetValue(evento.Chave, out var eventos))
            _eventos[evento.Chave] = eventos = [];
        eventos.Add(evento);
    }

    // A faixa com a mesma numeração (I07: CNPJ, modelo, série, início e fim, na UF).
    public FaixaInutilizada? Faixa(string uf, string cnpj, string modelo, int serie, long inicio, long fim) =>
        _faixas.FirstOrDefault(f => f.Uf == uf && f.Cnpj == cnpj && f.Modelo == modelo && f.Serie == serie && f.Inicio == inicio && f.Fim == fim);

    // Se algum número entre o início e o fim já está numa faixa inutilizada (I07a, e 3B08-100 com um número só).
    public bool Inutilizado(string uf, string documento, string modelo, int serie, long inicio, long fim) =>
        _faixas.Any(f => f.Uf == uf && f.Cnpj == documento && f.Modelo == modelo && f.Serie == serie && f.Inicio <= fim && inicio <= f.Fim);

    // Se alguma nota autorizada usa um número entre o início e o fim (I08), tirado da chave.
    public bool Utilizado(string uf, string documento, string modelo, int serie, long inicio, long fim) =>
        _ordem.Any(n => n.Chave[..2] == uf && n.Chave[6..20] == documento.PadLeft(14, '0') && n.Chave[20..22] == modelo
            && int.Parse(n.Chave[22..25], CultureInfo.InvariantCulture) == serie
            && long.Parse(n.Chave[25..34], CultureInfo.InvariantCulture) is var numero && numero >= inicio && numero <= fim);

    public void Registrar(FaixaInutilizada faixa) => _faixas.Add(faixa);

    public LoteRecebido? Lote(string autorizador, string recibo) => _lotes.GetValueOrDefault((autorizador, recibo));

    public void Registrar(LoteRecebido lote) => _lotes.Add((lote.Autorizador, lote.Recibo), lote);

    // O número do recibo (Visão Geral, 4.3.4): o cUF de onde o lote foi entregue, o tipo de autorizador e 12
    // posições sequenciais.
    public string NovoRecibo(Autorizador autorizador) =>
        autorizador.CodigoCasa + autorizador.Tipo + Proximo().ToString("D12", CultureInfo.InvariantCulture);

    // O número do protocolo (4.3.5): o tipo de autorizador, o cUF da nota, o ano e 10 posições sequenciais.
    public string NovoProtocolo(Autorizador autorizador, string uf, DateTimeOffset recebimento) =>
        autorizador.Tipo + uf + recebimento.ToOffset(TimeSpan.FromHours(-3)).ToString("yy", CultureInfo.InvariantCulture)
        + Proximo().ToString("D10", CultureInfo.InvariantCulture);

    // As notas, para o GET /_controle/notas, na ordem da autorização.
    public IReadOnlyList<NotaRegistrada> Notas()
    {
        lock (Trava)
            return [.. _ordem];
    }

    // O que o AN distribui a cada interessado, para o GET /_controle/distribuicao.
    public IReadOnlyList<DistribuicaoInteressado> Distribuidos()
    {
        lock (Trava)
            return Interessados.Listar();
    }

    public void Limpar()
    {
        lock (Trava)
        {
            _notas.Clear();
            _numeracoes.Clear();
            _ordem.Clear();
            _lotes.Clear();
            _eventos.Clear();
            _faixas.Clear();
            Interessados.Limpar();
        }
    }

    private long Proximo() => ++_sequencia;
}

// Uma nota autorizada. Numeracao: o que a duplicidade compara (Numerar). Recibo: o do lote assíncrono que a
// autorizou; nulo na autorização síncrona. Autorizador: a sigla de quem autorizou (a SVC só consulta as suas).
// Situacao: o cStat da autorização, 100 ou 150. ProtocoloXml: o protNFe como saiu, para a consulta. Xml: a NFe
// como chegou, para os eventos (o destinatário, os valores e os itens do cancelamento por substituição).
// Cancelada: o evento de cancelamento registrado (a consulta responde 101; a duplicidade, 218).
internal sealed record NotaRegistrada(
    string Chave,
    string Numeracao,
    string Protocolo,
    string? Recibo,
    DateTimeOffset Recebimento,
    string Autorizador,
    string Situacao,
    [property: JsonIgnore] XElement ProtocoloXml,
    [property: JsonIgnore] XElement Xml,
    bool Cancelada = false)
{
    // O que a duplicidade compara (Anexo I, 2B08-10) e a consulta procura (Visão Geral, 5.4.4, J03): modelo,
    // UF, CNPJ ou CPF do emitente, série e número; na NFC-e, também o tipo de emissão. Tirado da chave, que a
    // autorização já conferiu contra os campos (502); o CPF vem com os zeros à esquerda, como na chave.
    public static string Numerar(string chave)
    {
        var modelo = chave[20..22];
        var numeracao = string.Join('|', modelo, chave[..2], chave[6..20], chave[22..25], chave[25..34]);
        return modelo == "65" ? numeracao + "|" + chave[34] : numeracao;
    }
}

// Um lote assíncrono: os protocolos das notas, prontos para a consulta, o transmissor (só ele consulta,
// regra E05 do NFeRetAutorizacao4) e as consultas que ainda respondem 105.
internal sealed class LoteRecebido(string autorizador, string recibo, string transmissor, IReadOnlyList<XElement> protocolos, int pendentes)
{
    public string Autorizador { get; } = autorizador;

    public string Recibo { get; } = recibo;

    public string Transmissor { get; } = transmissor;

    public IReadOnlyList<XElement> Protocolos { get; } = protocolos;

    // Mudado com a Trava do repositório.
    public int Pendentes { get; set; } = pendentes;
}

// Um evento registrado (135). Processado: o procEventoNFe, com o evento como chegou e o retEvento, para a
// consulta do protocolo.
internal sealed record EventoRegistrado(
    string Chave,
    string Tipo,
    int Sequencia,
    string Protocolo,
    DateTimeOffset Registro,
    string Autorizador,
    XElement Processado);

// Uma faixa inutilizada (102): a UF, o CNPJ, o modelo, a série, o início e o fim, com o protocolo.
internal sealed record FaixaInutilizada(
    string Uf,
    string Cnpj,
    string Modelo,
    int Serie,
    long Inicio,
    long Fim,
    string Protocolo,
    DateTimeOffset Registro,
    string Autorizador);
