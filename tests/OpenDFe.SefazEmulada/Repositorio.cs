using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O que o emulador guarda entre os pedidos (PROJETO.md 13.5): as notas autorizadas, para a duplicidade, e os
// lotes recebidos, para a consulta do recibo. Em memória, vazio a cada partida. Quem processa um lote segura
// a Trava do começo ao fim, para que as notas do mesmo lote e de lotes simultâneos se vejam na ordem.
internal sealed class Repositorio
{
    private readonly Dictionary<string, NotaRegistrada> _notas = new(StringComparer.Ordinal);
    private readonly List<NotaRegistrada> _ordem = [];
    private readonly Dictionary<string, string> _numeracoes = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Autorizador, string Recibo), LoteRecebido> _lotes = [];
    private long _sequencia;

    public Lock Trava { get; } = new();

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

    public void Limpar()
    {
        lock (Trava)
        {
            _notas.Clear();
            _numeracoes.Clear();
            _ordem.Clear();
            _lotes.Clear();
        }
    }

    private long Proximo() => ++_sequencia;
}

// Uma nota autorizada. Numeracao: o que a duplicidade compara (Numerar). Recibo: o do lote assíncrono que a
// autorizou; nulo na autorização síncrona. Autorizador: a sigla de quem autorizou (a SVC só consulta as suas).
// Situacao: o cStat da autorização, 100 ou 150. ProtocoloXml: o protNFe como saiu, para a consulta.
internal sealed record NotaRegistrada(
    string Chave,
    string Numeracao,
    string Protocolo,
    string? Recibo,
    DateTimeOffset Recebimento,
    string Autorizador,
    string Situacao,
    [property: JsonIgnore] XElement ProtocoloXml)
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
