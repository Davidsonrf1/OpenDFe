using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// Os eventos da NFS-e na Sefin emulada (E.8), do Anexo II (ANEXO_II-SEFIN_ADN-PEDREGEVT_EVT-SNNFSe-v1.01-20260122,
// docs/NTS.md 12.1) e do manual da API do emissor público (1.5):
//   - o POST /nfse/{chave}/eventos, com o pedido de quem emite: o cancelamento (101101) e a solicitação de análise fiscal
//     para cancelamento (101103), os dois de autor "Emite" na aba TIPO EVENTOS; as regras da aba RN_EVENTO_PED.REG.EVENTO
//     que o emulador confere, e a matriz da aba RN_EVENTOSxEVENTOS para os eventos que ele conhece;
//   - o cancelamento por substituição (105102), que a Sefin gera sozinha quando a DPS traz o subst (manual 1.3.2);
//   - os eventos do município emissor (o deferimento 105104 e o indeferimento 105105 da análise fiscal, e o cancelamento
//     por ofício 305101), pela rota de controle, para quem usa a lib ter o que ler na consulta dos eventos.
// O evento gerado é o TCEvento (evento_v1.01.xsd): o infEvento com o pedido dentro, assinado pela AC de teste no papel
// da Sefin, como a NFS-e.
internal sealed partial class EventosNFSe(Municipios municipios, RepositorioNFSe repositorio, AssinanteNFSe assinante)
{
    // O campo do JSON do pedido (docs/NTS.md 12.4: de implementação aberta, a conferir no Swagger com o PFX).
    public const string CampoPedido = "pedidoRegistroEventoXmlGZipB64";

    public const string Cancelamento = "101101";
    public const string Substituicao = "105102";
    public const string AnaliseFiscal = "101103";
    public const string Deferimento = "105104";
    public const string Indeferimento = "105105";
    public const string CancelamentoOficio = "305101";

    // A mesma tolerância da data de emissão da DPS.
    private static readonly TimeSpan Tolerancia = TimeSpan.FromMinutes(5);

    private static readonly XNamespace Namespace = EmissaoNFSe.Namespace;
    private static readonly XNamespace NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    // O xDesc de cada evento que o emulador conhece (o texto fixo do tiposEventos_v1.01.xsd), que é também o nome que
    // vai no lugar de <nome_evento_vinculado_a_NFS-e> das mensagens da matriz.
    private static readonly Dictionary<string, string> Descricoes = new(StringComparer.Ordinal)
    {
        [Cancelamento] = "Cancelamento de NFS-e",
        [Substituicao] = "Cancelamento de NFS-e por Substituição",
        [AnaliseFiscal] = "Solicitação de Análise Fiscal para Cancelamento de NFS-e",
        [Deferimento] = "Cancelamento de NFS-e Deferido por Análise Fiscal",
        [Indeferimento] = "Cancelamento de NFS-e Indeferido por Análise Fiscal",
        [CancelamentoOficio] = "Cancelamento de NFS-e por Ofício",
    };

    // O código da matriz quando um evento anterior impede o novo, por tipo do novo.
    private static readonly Dictionary<string, string> Impedimentos = new(StringComparer.Ordinal)
    {
        [Cancelamento] = MotivosNFSe.CancelamentoImpedido,
        [Substituicao] = MotivosNFSe.SubstituicaoImpedida,
        [AnaliseFiscal] = MotivosNFSe.AnaliseImpedida,
        [Deferimento] = MotivosNFSe.DeferimentoImpedido,
        [Indeferimento] = MotivosNFSe.IndeferimentoImpedido,
        [CancelamentoOficio] = MotivosNFSe.CancelamentoOficioImpedido,
    };

    // Os eventos que o município manda pela rota de controle.
    public static IReadOnlyList<string> TiposMunicipio { get; } = [Deferimento, Indeferimento, CancelamentoOficio];

    // O tipo do evento (o nome do elemento e<tipo> do infPedReg).
    public static string? Tipo(XElement pedido) =>
        pedido.Element(Namespace + "infPedReg")?.Elements().FirstOrDefault(e => e.Name.Namespace == Namespace && ElementoTipo().IsMatch(e.Name.LocalName))
            ?.Name.LocalName[1..];

    // A NFS-e encerrada por um evento: o cancelamento, a substituição, o deferimento e o cancelamento por ofício.
    public static bool Encerra(string tipo) => tipo is Cancelamento or Substituicao or Deferimento or CancelamentoOficio;

    // A solicitação de análise fiscal sem o deferimento nem o indeferimento.
    public static bool AnalisePendente(IReadOnlyList<EventoNFSeRegistrado> eventos) =>
        eventos.Any(e => e.Tipo == AnaliseFiscal) && !eventos.Any(e => e.Tipo is Deferimento or Indeferimento);

    // A situação da NFS-e pelos eventos, para o controle: normal, cancelada, substituída ou em análise.
    public static string Situacao(IReadOnlyList<EventoNFSeRegistrado> eventos) =>
        eventos.Any(e => e.Tipo == Substituicao) ? "substituida"
        : eventos.Any(e => Encerra(e.Tipo)) ? "cancelada"
        : AnalisePendente(eventos) ? "emAnalise"
        : "normal";

    // A matriz da aba RN_EVENTOSxEVENTOS, restrita aos eventos que o emulador conhece: sem evento, valem o cancelamento,
    // a substituição, a análise fiscal e o ofício; o deferimento e o indeferimento pedem a análise pendente (E0853,
    // E0856). Depois da análise, só o deferimento e o indeferimento; depois do indeferimento, só o ofício; o
    // cancelamento, a substituição, o deferimento e o ofício encerram a NFS-e.
    public static ErroSefin? Conferir(string tipo, IReadOnlyList<EventoNFSeRegistrado> eventos)
    {
        ArgumentNullException.ThrowIfNull(eventos);
        if (tipo == Deferimento && !AnalisePendente(eventos))
            return ErroSefin.Criar(MotivosNFSe.DeferimentoSemAnalise);
        if (tipo == Indeferimento && !AnalisePendente(eventos))
            return ErroSefin.Criar(MotivosNFSe.IndeferimentoSemAnalise);
        var pendente = AnalisePendente(eventos);
        foreach (var anterior in eventos)
        {
            // A linha da análise fiscal vale enquanto ela está pendente: depois do indeferimento, vale a linha dele (que
            // permite o ofício, que a da análise não permite).
            if (anterior.Tipo == AnaliseFiscal && !pendente)
                continue;
            var permitido = anterior.Tipo switch
            {
                AnaliseFiscal => tipo is Deferimento or Indeferimento,
                Indeferimento => tipo is CancelamentoOficio,
                _ => false,
            };
            if (!permitido)
            {
                var codigo = Impedimentos[tipo];
                return new ErroSefin(codigo, MotivosNFSe.Texto(codigo, Descricoes[anterior.Tipo]));
            }
        }
        return null;
    }

    // A conferência da substituta sobre os eventos da substituída (Anexo I, RN DPS_NFS-e, subst/chSubstda): a cancelada
    // ou já substituída (E0046), a análise fiscal pendente (E0068), e o resto da matriz (E0845).
    public static ErroSefin? ConferirSubstituida(IReadOnlyList<EventoNFSeRegistrado> eventos)
    {
        if (eventos.Any(e => Encerra(e.Tipo)))
            return ErroSefin.Criar(MotivosNFSe.SubstituidaCancelada);
        if (AnalisePendente(eventos))
            return ErroSefin.Criar(MotivosNFSe.SubstituidaAnalisePendente);
        return Conferir(Substituicao, eventos);
    }

    // O POST /nfse/{chave}/eventos, com o pedido já aberto e válido no schema. As regras, na ordem: a assinatura (E1989),
    // o autor contra o certificado da assinatura (E0812, E0815) e a assinatura que não confere (E1980), o Id (E1827), o
    // ambiente (E1845), a data (E1843), a chave do caminho (EMULADA-CHAVE), o tipo que a Sefin recebe de quem emite
    // (EMULADA-TIPO), a NFS-e (E1831), o autor contra o prestador (E0813, E0816), o prazo do cancelamento (E0822) e a
    // matriz (E0840, E0848).
    public ResultadoSefin Registrar(DocumentoAberto aberto, string chaveCaminho, DateTimeOffset recebimento)
    {
        ArgumentNullException.ThrowIfNull(aberto);
        var pedido = aberto.Raiz;
        var inf = pedido.Element(Namespace + "infPedReg")!;
        var idPedido = inf.Attribute("Id")!.Value;
        ResultadoSefin Rejeitar(ErroSefin erro) =>
            new(StatusCodes.Status400BadRequest, EmissaoNFSe.Erro(recebimento, null, erro), $"evento rejeitado: {erro.Codigo}");
        ResultadoSefin RejeitarCodigo(string codigo, string? complemento = null) => Rejeitar(ErroSefin.Criar(codigo, complemento));

        if (pedido.Element(NamespaceAssinatura + "Signature") is null)
            return RejeitarCodigo(MotivosNFSe.EventoAssinaturaAusente);
        var cnpj = EmissaoNFSe.Valor(inf, "CNPJAutor");
        var cpf = EmissaoNFSe.Valor(inf, "CPFAutor");
        var autor = cnpj ?? cpf!;
        var assinatura = AssinaturaDigital.Verificar(aberto.Texto, idPedido, autor);
        if (assinatura.Count > 0)
        {
            var problema = assinatura[0];
            var codigo = problema.Codigo is "213" or "227"
                ? cnpj is not null ? MotivosNFSe.EventoCnpjAutorCertificado : MotivosNFSe.EventoCpfAutorCertificado
                : MotivosNFSe.EventoAssinaturaInvalida;
            return RejeitarCodigo(codigo, problema.Mensagem);
        }

        var chave = EmissaoNFSe.Valor(inf, "chNFSe")!;
        var tipo = Tipo(pedido)!;
        if (idPedido != "PRE" + chave + tipo)
            return RejeitarCodigo(MotivosNFSe.IdPedidoDivergente, $"O esperado é PRE{chave}{tipo}.");
        if (EmissaoNFSe.Valor(inf, "tpAmb") != "2")
            return RejeitarCodigo(MotivosNFSe.EventoAmbienteDivergente, "O emulador é a produção restrita (tpAmb 2).");
        var data = DateTimeOffset.ParseExact(EmissaoNFSe.Valor(inf, "dhEvento")!, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        if (data > recebimento + Tolerancia)
            return RejeitarCodigo(MotivosNFSe.EventoEmissaoPosterior);
        if (chave != chaveCaminho)
            return RejeitarCodigo(MotivosNFSe.EventoChaveDivergente, $"O caminho traz {chaveCaminho}; o pedido, {chave}.");
        if (tipo is not (Cancelamento or AnaliseFiscal))
            return RejeitarCodigo(MotivosNFSe.EventoNaoAceito, $"O tipo {tipo}.");

        var (evento, erro) = repositorio.RegistrarEvento(chave, (nota, eventos) =>
        {
            if (nota.Prestador != autor)
                return ErroSefin.Criar(cnpj is not null ? MotivosNFSe.EventoCnpjAutorTipo : MotivosNFSe.EventoCpfAutorTipo,
                    "O autor do cancelamento e da análise fiscal é o prestador da NFS-e.");
            if (tipo == Cancelamento && municipios.Procurar(nota.Municipio).Municipio.PrazoCancelamento is { } prazo
                && recebimento > nota.Processamento.AddDays(prazo))
                return ErroSefin.Criar(MotivosNFSe.CancelamentoForaPrazo, $"O prazo do município {nota.Municipio} é de {prazo} dias.");
            return Conferir(tipo, eventos);
        }, (nota, numeroDfse, sequencia) => Gerar(pedido, chave, tipo, sequencia(tipo), numeroDfse, recebimento));
        if (erro is not null)
            return Rejeitar(erro);
        return new ResultadoSefin(StatusCodes.Status201Created, Resposta(evento!, recebimento), $"evento {tipo} registrado: {evento!.Id}");
    }

    // O 105102 que a Sefin gera sobre a substituída, com a substituta (manual 1.3.2): o pedido montado aqui, sem a
    // assinatura (o autor é o município emissor, e a assinatura do pedido não é obrigatória, aba TIPO EVENTOS), com o
    // autor sendo o prestador (o emulador não tem o CNPJ do município) e o motivo do subst da DPS.
    public EventoNFSeRegistrado GerarSubstituicao(string substituida, NFSeRegistrada substituta, string motivo, string? descricao, long numeroDfse,
        DateTimeOffset processamento)
    {
        ArgumentNullException.ThrowIfNull(substituta);
        var detalhe = new List<XElement> { Elemento("cMotivo", motivo) };
        if (descricao is not null)
            detalhe.Add(Elemento("xMotivo", descricao));
        detalhe.Add(Elemento("chSubstituta", substituta.Chave));
        var pedido = Pedido(substituida, Substituicao, substituta.Prestador, processamento, detalhe);
        return Gerar(pedido, substituida, Substituicao, 1, numeroDfse, processamento);
    }

    // Um evento do município (105104, 105105, 305101), pela rota de controle: o pedido montado com o CPF do agente como
    // autor, conferido no schema (ArgumentException com a mensagem dele) e registrado com a matriz.
    public (EventoNFSeRegistrado? Evento, ErroSefin? Erro) RegistrarMunicipio(string chave, PedidoEventoMunicipio pedido, DateTimeOffset processamento)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        if (pedido.Tipo is null || !TiposMunicipio.Contains(pedido.Tipo))
            throw new ArgumentException($"O tipo é {string.Join(", ", TiposMunicipio)}.", nameof(pedido));
        if (pedido.CpfAgente is null || !Documentos.CpfValido(pedido.CpfAgente))
            throw new ArgumentException("O cpfAgente é o CPF do agente tributário, com 11 dígitos e o DV.", nameof(pedido));
        var detalhe = new List<XElement> { Elemento("CPFAgTrib", pedido.CpfAgente) };
        if (pedido.Tipo == CancelamentoOficio)
        {
            detalhe.Add(Elemento("nProcAdm", pedido.Processo ?? ""));
            detalhe.Add(Elemento("xProcAdm", pedido.Descricao ?? ""));
        }
        else
        {
            if (pedido.Processo is not null)
                detalhe.Add(Elemento("nProcAdm", pedido.Processo));
            detalhe.Add(Elemento("cMotivo", pedido.Motivo ?? "1"));
            detalhe.Add(Elemento("xMotivo", pedido.Descricao ?? ""));
        }
        var xml = Pedido(chave, pedido.Tipo, pedido.CpfAgente, processamento, detalhe);
        var problemas = SchemasLeiaute.PedidoEventoNFSe.Validar(xml.ToString(SaveOptions.DisableFormatting));
        if (problemas.Count > 0)
            throw new ArgumentException(problemas[0].Mensagem, nameof(pedido));
        return repositorio.RegistrarEvento(chave, (_, eventos) => Conferir(pedido.Tipo, eventos),
            (_, numeroDfse, sequencia) => Gerar(xml, chave, pedido.Tipo, sequencia(pedido.Tipo), numeroDfse, processamento));
    }

    // O JSON do evento registrado (201) e de cada evento da consulta.
    public static RespostaEvento Resposta(EventoNFSeRegistrado evento, DateTimeOffset processamento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        return new(2, EmissaoNFSe.VersaoAplicativo, EmissaoNFSe.DataHora(processamento), evento.Chave, evento.Tipo, evento.Sequencia,
            EmissaoNFSe.Compactar(evento.Xml));
    }

    // O pedRegEvento que o emulador monta (o 105102 e os do município), sem a assinatura.
    private static XElement Pedido(string chave, string tipo, string autor, DateTimeOffset processamento, IEnumerable<XElement> detalhe) =>
        new(Namespace + "pedRegEvento", new XAttribute("versao", "1.01"),
            new XElement(Namespace + "infPedReg", new XAttribute("Id", "PRE" + chave + tipo),
                Elemento("tpAmb", "2"),
                Elemento("verAplic", EmissaoNFSe.VersaoAplicativo),
                Elemento("dhEvento", EmissaoNFSe.DataHora(processamento)),
                Elemento(autor.Length == 11 ? "CPFAutor" : "CNPJAutor", autor),
                Elemento("chNFSe", chave),
                new XElement(Namespace + ("e" + tipo), Elemento("xDesc", Descricoes[tipo]), detalhe)));

    // O evento (TCEvento): o Id é EVT + o Id do pedido sem o PRE (a chave e o tipo, 56) + o nSeqEvento (3), Anexo II,
    // RN_EVENTO_PED.REG.EVENTO, linha 4; o ambiente gerador 2 (a Sefin); assinado pela AC de teste.
    private EventoNFSeRegistrado Gerar(XElement pedido, string chave, string tipo, int sequencia, long numeroDfse, DateTimeOffset processamento)
    {
        var id = "EVT" + chave + tipo + sequencia.ToString("D3", CultureInfo.InvariantCulture);
        var xml = new XElement(Namespace + "evento", new XAttribute("versao", "1.01"),
            new XElement(Namespace + "infEvento", new XAttribute("Id", id),
                Elemento("verAplic", EmissaoNFSe.VersaoAplicativo),
                Elemento("ambGer", "2"),
                Elemento("nSeqEvento", sequencia.ToString(CultureInfo.InvariantCulture)),
                Elemento("dhProc", EmissaoNFSe.DataHora(processamento)),
                Elemento("nDFSe", numeroDfse.ToString(CultureInfo.InvariantCulture)),
                new XElement(pedido))).ToString(SaveOptions.DisableFormatting);
        xml = AssinaturaDigital.Assinar(xml, id, assinante.Certificado);
        return new EventoNFSeRegistrado(chave, tipo, sequencia, id, numeroDfse, processamento, xml);
    }

    private static XElement Elemento(string nome, string valor) => new(Namespace + nome, valor);

    [GeneratedRegex("^e[0-9]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex ElementoTipo();
}

// O evento do município pela rota de controle (POST /_controle/nfse/{chave}/eventos):
//   Tipo       105104 (deferimento), 105105 (indeferimento) ou 305101 (cancelamento por ofício)
//   CpfAgente  o CPF do agente tributário (CPFAgTrib), que é também o autor do pedido
//   Processo   o nProcAdm (só dígitos, até 30; obrigatório no 305101)
//   Motivo     o cMotivo (105104: 1; 105105: 1 ou 2); o padrão é 1
//   Descricao  o xMotivo (105104, 105105) ou o xProcAdm (305101), de 15 a 255 caracteres
internal sealed record PedidoEventoMunicipio(string? Tipo, string? CpfAgente, string? Processo = null, string? Motivo = null, string? Descricao = null);

// O JSON do evento registrado (POST, 201) e de cada evento na consulta: a chave, o tipo, o nSeqEvento e o evento em
// GZip e Base64 (o nome eventoXmlGZipB64 é a nossa leitura, como o nfseXmlGZipB64; docs/SEFAZ_EMULADA.md).
internal sealed record RespostaEvento(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string ChaveAcesso, string TipoEvento,
    int NumSeqEvento, string EventoXmlGZipB64);

// O JSON da consulta dos eventos (GET /nfse/{chave}/eventos[/{tipo}[/{seq}]]).
internal sealed record RespostaEventos(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string ChaveAcesso,
    IReadOnlyList<RespostaEvento> Eventos);
