using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O que o Ambiente Nacional distribui (NT 2014.002 v1.40, 3 e 3.11.3): para cada interessado, pelo CNPJ (as 14
// posições: cada estabelecimento tem a sua sequência) ou pelo CPF, os documentos com o NSU, sem lacunas, na ordem
// em que chegaram. Os papéis, pela tabela do item 3, só na NF-e (a NFC-e não é distribuída):
//   - o destinatário (dest): o resNFe na autorização; o evento de cancelamento sempre, como resEvento até a
//     manifestação que libera o download e como procEventoNFe depois; a liberação (a ciência, a confirmação ou a
//     operação não realizada; o desconhecimento não libera) traz o nfeProc e as cartas de correção já
//     registradas, e as seguintes chegam como procEventoNFe;
//   - o transportador (transporta) e os autorizados no autXML: o nfeProc na autorização e o procEventoNFe do
//     cancelamento e da carta de correção; os autorizados, também o da manifestação;
//   - o emitente: o procEventoNFe da manifestação do destinatário.
// O documento que é também o destinatário da nota (o transportador ou o autorizado) recebe como destinatário
// (3.4: "não é gerado o NSU, até que o destinatário tenha realizado a manifestação"). Guarda também o consumo de
// cada CNPJ ou CPF da requisição, para o uso indevido (3.11.4, 656). Chamado com a Trava do repositório.
internal sealed class Interessados
{
    private const string Cancelamento = "110111";
    private const string CartaCorrecao = "110110";
    private const string Desconhecimento = "210220";

    private readonly Dictionary<string, List<DocumentoDistribuido>> _documentos = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Consumo> _consumos = new(StringComparer.Ordinal);
    private readonly HashSet<string> _liberadas = new(StringComparer.Ordinal);

    // Os documentos do interessado, na ordem do NSU.
    public IReadOnlyList<DocumentoDistribuido> Documentos(string interessado) =>
        _documentos.TryGetValue(interessado, out var documentos) ? documentos : [];

    // O maior NSU do interessado (zero sem documentos).
    public long Maximo(string interessado) => Documentos(interessado).Count;

    // Se o destinatário da nota já se manifestou de um jeito que libera o download.
    public bool Liberada(string chave) => _liberadas.Contains(chave);

    public Consumo Consumo(string documento)
    {
        if (!_consumos.TryGetValue(documento, out var consumo))
            _consumos[documento] = consumo = new Consumo();
        return consumo;
    }

    // A NF-e autorizada: o resNFe ao destinatário e o nfeProc ao transportador e aos autorizados.
    public void Autorizada(NotaRegistrada nota, DateTimeOffset recebimento)
    {
        var papeis = Atores(nota);
        if (papeis is null)
            return;
        if (papeis.Destinatario is { } destinatario)
            Acrescentar(destinatario, nota.Chave, "resNFe_v1.01.xsd", Resumo(nota), recebimento);
        foreach (var terceiro in papeis.Terceiros())
            Acrescentar(terceiro, nota.Chave, "procNFe_v4.00.xsd", Processada(nota), recebimento);
    }

    // O evento registrado (135), pelo tipo.
    public void Registrado(NotaRegistrada nota, EventoRegistrado evento, IReadOnlyList<EventoRegistrado> eventos)
    {
        var papeis = Atores(nota);
        if (papeis is null)
            return;
        var processado = new XElement(evento.Processado);
        switch (evento.Tipo)
        {
            case Cancelamento:
            case CartaCorrecao:
                if (papeis.Destinatario is { } destinatario)
                {
                    if (Liberada(nota.Chave))
                        Acrescentar(destinatario, nota.Chave, "procEventoNFe_v1.00.xsd", processado, evento.Registro);
                    else if (evento.Tipo == Cancelamento)
                        Acrescentar(destinatario, nota.Chave, "resEvento_v1.01.xsd", ResumoEvento(evento), evento.Registro);
                }
                foreach (var terceiro in papeis.Terceiros())
                    Acrescentar(terceiro, nota.Chave, "procEventoNFe_v1.00.xsd", new XElement(processado), evento.Registro);
                break;
            default:
                // A manifestação do destinatário: ao emitente e aos autorizados; a que libera, o download ao destinatário.
                Acrescentar(papeis.Emitente, nota.Chave, "procEventoNFe_v1.00.xsd", processado, evento.Registro);
                foreach (var autorizado in papeis.Autorizados)
                    Acrescentar(autorizado, nota.Chave, "procEventoNFe_v1.00.xsd", new XElement(processado), evento.Registro);
                if (evento.Tipo != Desconhecimento && papeis.Destinatario is { } liberado && _liberadas.Add(nota.Chave))
                {
                    Acrescentar(liberado, nota.Chave, "procNFe_v4.00.xsd", Processada(nota), evento.Registro);
                    foreach (var carta in eventos.Where(e => e.Tipo == CartaCorrecao))
                        Acrescentar(liberado, nota.Chave, "procEventoNFe_v1.00.xsd", new XElement(carta.Processado), evento.Registro);
                }
                break;
        }
    }

    // Os interessados com os documentos, para o GET /_controle/distribuicao, em ordem.
    public IReadOnlyList<DistribuicaoInteressado> Listar() =>
        [.. _documentos.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new DistribuicaoInteressado(p.Key,
            [.. p.Value.Select(d => new DocumentoListado(d.Nsu, d.Chave, d.Schema, d.Recebimento))]))];

    public void Limpar()
    {
        _documentos.Clear();
        _consumos.Clear();
        _liberadas.Clear();
    }

    // O nfeProc (a NF-e com o protNFe), o que o destinatário baixa e o transportador e os autorizados recebem.
    public static XElement Processada(NotaRegistrada nota) =>
        new(Servico.NamespaceNFe + "nfeProc", new XAttribute("versao", "4.00"), new XElement(nota.Xml), new XElement(nota.ProtocoloXml));

    // O resNFe (Tabela 5-30, C01 a C14), com o digVal e o nProt do protNFe e o horário da autorização. Sem a IE do
    // emitente (opcional desde a NT 2026.007), ISENTO, que o schema aceita.
    public static XElement Resumo(NotaRegistrada nota)
    {
        var informacoes = nota.Xml.Element(Servico.NamespaceNFe + "infNFe")!;
        var emitente = informacoes.Element(Servico.NamespaceNFe + "emit")!;
        var ide = informacoes.Element(Servico.NamespaceNFe + "ide")!;
        var protocolo = nota.ProtocoloXml.Element(Servico.NamespaceNFe + "infProt")!;
        var documento = emitente.Elements().First(e => e.Name.LocalName is "CNPJ" or "CPF");
        XElement Campo(string nome, string valor) => new(Servico.NamespaceNFe + nome, valor);
        return new XElement(Servico.NamespaceNFe + "resNFe",
            new XAttribute("versao", "1.01"),
            Campo("chNFe", nota.Chave),
            Campo(documento.Name.LocalName, documento.Value),
            Campo("xNome", emitente.Element(Servico.NamespaceNFe + "xNome")!.Value),
            Campo("IE", emitente.Element(Servico.NamespaceNFe + "IE")?.Value ?? "ISENTO"),
            Campo("dhEmi", ide.Element(Servico.NamespaceNFe + "dhEmi")!.Value),
            Campo("tpNF", ide.Element(Servico.NamespaceNFe + "tpNF")!.Value),
            Campo("vNF", informacoes.Element(Servico.NamespaceNFe + "total")!.Element(Servico.NamespaceNFe + "ICMSTot")!.Element(Servico.NamespaceNFe + "vNF")!.Value),
            protocolo.Element(Servico.NamespaceNFe + "digVal") is { } resumo ? Campo("digVal", resumo.Value) : null,
            Campo("dhRecbto", protocolo.Element(Servico.NamespaceNFe + "dhRecbto")!.Value),
            Campo("nProt", nota.Protocolo),
            Campo("cSitNFe", nota.Cancelada ? "3" : "1"));
    }

    // O resEvento (Tabela 5-31, D01 a D12), do evento e do retEvento guardados no procEventoNFe: o autor do evento
    // e o registro.
    public static XElement ResumoEvento(EventoRegistrado evento)
    {
        var informacao = evento.Processado.Element(Servico.NamespaceNFe + "evento")!.Element(Servico.NamespaceNFe + "infEvento")!;
        var retorno = evento.Processado.Element(Servico.NamespaceNFe + "retEvento")!.Element(Servico.NamespaceNFe + "infEvento")!;
        var autor = informacao.Elements().First(e => e.Name.LocalName is "CNPJ" or "CPF");
        XElement Campo(string nome, string valor) => new(Servico.NamespaceNFe + nome, valor);
        return new XElement(Servico.NamespaceNFe + "resEvento",
            new XAttribute("versao", "1.01"),
            Campo("cOrgao", informacao.Element(Servico.NamespaceNFe + "cOrgao")!.Value),
            Campo(autor.Name.LocalName, autor.Value),
            Campo("chNFe", evento.Chave),
            Campo("dhEvento", informacao.Element(Servico.NamespaceNFe + "dhEvento")!.Value),
            Campo("tpEvento", evento.Tipo),
            Campo("nSeqEvento", evento.Sequencia.ToString(CultureInfo.InvariantCulture)),
            Campo("xEvento", retorno.Element(Servico.NamespaceNFe + "xEvento")!.Value),
            Campo("dhRecbto", retorno.Element(Servico.NamespaceNFe + "dhRegEvento")!.Value),
            Campo("nProt", evento.Protocolo));
    }

    // Os atores da NF-e (NT 2014.002, 2): o emitente (C01), o destinatário (E01), o transportador (X03) e os
    // autorizados (GA01), pelo CNPJ ou pelo CPF. Nulo na NFC-e.
    public static Papeis? Atores(NotaRegistrada nota)
    {
        if (nota.Chave[20..22] != "55")
            return null;
        var informacoes = nota.Xml.Element(Servico.NamespaceNFe + "infNFe")!;
        static string? Documento(XElement? grupo) =>
            grupo?.Elements().FirstOrDefault(e => e.Name.LocalName is "CNPJ" or "CPF")?.Value;
        var emitente = Documento(informacoes.Element(Servico.NamespaceNFe + "emit"))!;
        var destinatario = Documento(informacoes.Element(Servico.NamespaceNFe + "dest"));
        var transportador = Documento(informacoes.Element(Servico.NamespaceNFe + "transp")?.Element(Servico.NamespaceNFe + "transporta"));
        var autorizados = informacoes.Elements(Servico.NamespaceNFe + "autXML").Select(a => Documento(a)!)
            .Where(d => d != destinatario).Distinct(StringComparer.Ordinal).ToList();
        return new Papeis(emitente, destinatario, transportador == destinatario ? null : transportador, autorizados);
    }

    private void Acrescentar(string interessado, string chave, string schema, XElement xml, DateTimeOffset recebimento)
    {
        if (!_documentos.TryGetValue(interessado, out var documentos))
            _documentos[interessado] = documentos = [];
        documentos.Add(new DocumentoDistribuido(documentos.Count + 1, chave, schema, xml, recebimento));
    }
}

// Um interessado com os seus documentos, sem o XML (o GET /_controle/distribuicao).
internal sealed record DistribuicaoInteressado(string Interessado, IReadOnlyList<DocumentoListado> Documentos);

internal sealed record DocumentoListado(long Nsu, string Chave, string Schema, DateTimeOffset Recebimento);

// Os atores de uma NF-e. Terceiros: o transportador e os autorizados, sem repetir.
internal sealed record Papeis(string Emitente, string? Destinatario, string? Transportador, IReadOnlyList<string> Autorizados)
{
    public IEnumerable<string> Terceiros() =>
        (Transportador is null ? Autorizados : Autorizados.Prepend(Transportador)).Distinct(StringComparer.Ordinal);

    // Se o CNPJ (pela raiz, as 8 primeiras posições) ou o CPF do interessado é o do documento (3.7, H16).
    public static bool Mesmo(string interessado, string? documento) =>
        documento is not null && (interessado.Length == 14
            ? documento.Length == 14 && documento[..8] == interessado[..8]
            : documento == interessado);
}

// Um documento distribuído a um interessado: o NSU, a chave da nota, o schema (o atributo do docZip), o XML e o
// recebimento pelo Ambiente Nacional (o que conta para os 90 dias).
internal sealed record DocumentoDistribuido(long Nsu, string Chave, string Schema, XElement Xml, DateTimeOffset Recebimento);

// O consumo de um CNPJ (as 14 posições) ou CPF da requisição (3.11.4): o último ultNSU devolvido no distNSU, quando
// ele respondeu 137, as consultas pontuais da última hora e o bloqueio, com o motivo.
internal sealed class Consumo
{
    public long? UltimoNsu { get; set; }

    public DateTimeOffset? Nenhum { get; set; }

    public List<DateTimeOffset> Pontuais { get; } = [];

    public DateTimeOffset? Bloqueio { get; set; }

    public string Motivo { get; set; } = "";
}
