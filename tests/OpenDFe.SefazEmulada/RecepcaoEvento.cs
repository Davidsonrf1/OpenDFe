using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeRecepcaoEvento4 (MOC 7.0, Visão Geral, 5.8 a 5.11): o envEvento e o retEnvEvento, com o cancelamento
// (110111), o cancelamento por substituição da NFC-e (110112) e a carta de correção (110110) nos autorizadores, e
// a manifestação do destinatário (210200, 210210, 210220, 210240; NT 2020.001 v1.60) no AN. No lote, na ordem:
// a raiz (516, 404, 517, 239), o schema genérico (215, com o detEvento livre) e a paralisação (108, 109);
// passando, 128, e cada evento recebe o seu retEvento. As regras de cada evento estão em Conferir. A SVC só
// registra o cancelamento das notas que ela autorizou (Anexo III, 2.1.3, c e d). O evento registrado chega aos
// interessados da distribuição (Interessados).
internal sealed class RecepcaoEvento : Servico
{
    private const string Versao = "1.00";
    private const string CartaCorrecao = "110110";
    private const string Cancelamento = "110111";
    private const string Substituicao = "110112";
    private const string Confirmacao = "210200";
    private const string Ciencia = "210210";
    private const string Desconhecimento = "210220";
    private const string NaoRealizada = "210240";

    // O xEvento do retEvento registrado: o do cancelamento é o mesmo nos dois (5.9.2); os da manifestação, da NT
    // 2020.001, 6.3.2 (R20).
    private static readonly Dictionary<string, string> Descricoes = new(StringComparer.Ordinal)
    {
        [CartaCorrecao] = "Carta de Correção registrada",
        [Cancelamento] = "Cancelamento homologado",
        [Substituicao] = "Cancelamento homologado",
        [Confirmacao] = "Confirmacao de Operacao registrada",
        [Ciencia] = "Ciencia da Operacao registrada",
        [Desconhecimento] = "Desconhecimento da Operacao registrada",
        [NaoRealizada] = "Operacao nao Realizada registrada",
    };

    public override string Nome => NomeRecepcaoEvento;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "envEvento", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Evento.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);
        var lote = dados.Element(NamespaceNFe + "idLote")!.Value;
        var eventos = dados.Elements(NamespaceNFe + "evento").ToList();
        var orgao = eventos[0].Element(NamespaceNFe + "infEvento")!.Element(NamespaceNFe + "cOrgao")!.Value;
        if (pedido.Paralisacao is { } paralisacao)
            return Retorno(paralisacao.Codigo, pedido, lote, orgao, null);

        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
            return Retorno(Motivos.LoteEventoProcessado, pedido, lote, orgao, [.. eventos.Select(e => Registrar(e, pedido, repositorio))]);
    }

    // Antes de ler o lote, o idLote vai com zero e o órgão é a casa do autorizador.
    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, "0", pedido.Autorizador.CodigoCasa, null);

    // O retEnvEvento (Tabela 5-33), com o órgão do primeiro evento (R06: idem a mensagem de entrada).
    private static XElement Retorno(string codigo, Pedido pedido, string lote, string orgao, IReadOnlyList<XElement>? eventos) =>
        new(NamespaceNFe + "retEnvEvento",
            new XAttribute("versao", Versao),
            Elemento("idLote", lote),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cOrgao", orgao),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo)),
            eventos);

    // Um evento do lote: as regras, o registro (com o protocolo e o procEventoNFe) e o retEvento.
    private static XElement Registrar(XElement evento, Pedido pedido, Repositorio repositorio)
    {
        var lido = Lido.Ler(evento);
        var (codigo, complemento, nota) = Conferir(lido, evento, pedido, repositorio);
        if (codigo != Motivos.EventoVinculado)
            return RetornoEvento(pedido, lido, codigo, Motivos.Texto(codigo) + complemento, null, null);

        // O protocolo do AN leva o 91 no lugar da UF (R31: o código do órgão).
        var protocolo = repositorio.NovoProtocolo(pedido.Autorizador, Nacional(pedido) ? pedido.Autorizador.CodigoCasa : lido.Chave[..2], pedido.Recebimento);
        var retorno = RetornoEvento(pedido, lido, codigo, Motivos.Texto(codigo), protocolo, nota);
        var processado = new XElement(NamespaceNFe + "procEventoNFe", new XAttribute("versao", Versao), new XElement(evento), new XElement(retorno));
        var registrado = new EventoRegistrado(lido.Chave, lido.Tipo, lido.Sequencia, protocolo, pedido.Recebimento, pedido.Autorizador.Sigla, processado);
        repositorio.Registrar(registrado);
        if (lido.Tipo is Cancelamento or Substituicao)
            repositorio.Cancelar(lido.Chave);
        repositorio.Interessados.Registrado(nota!, registrado, repositorio.Eventos(lido.Chave));
        return retorno;
    }

    // As regras de um evento, na ordem. Primeiro, as do Anexo I que separam o evento do lote: o tpEvento (D04,
    // 491; na SVC, só o cancelamento; no AN, só a manifestação), o verEvento (D05, 492) e o detEvento no schema do
    // tipo (D06, 493). Depois, as da parte geral (Tabela 5-35 e NT 2020.001, 6.3.7): o documento do autor (P10-10,
    // 489; P11-10, 490), que a assinatura precisa, a assinatura e o titular contra o autor (grupos E e F), o Id
    // (P07-10, 572), o órgão (P08-10, 250: a UF da chave, atendida pelo autorizador; no AN, o 91), o ambiente
    // (P09-10, 252), a chave (P12-10 a P12-34: 236, 615 a 619), o autor contra o emitente da chave (P12-44, 574;
    // não no AN, onde o autor é o destinatário), a data do evento (P13-10, 578), o autor irregular (1P10-20, 240;
    // não no AN, que a NT 2020.001 não lista) e a duplicidade do evento (3P15-10, 573). Por último, as do tipo
    // (ConferirCancelamento, ConferirCarta, ConferirManifestacao). O complemento vai depois do texto do motivo.
    private static (string Codigo, string Complemento, NotaRegistrada? Nota) Conferir(Lido lido, XElement evento, Pedido pedido, Repositorio repositorio)
    {
        var svc = pedido.Autorizador.TipoEmissao is not null;
        var nacional = Nacional(pedido);
        var aceito = nacional ? lido.Tipo is Confirmacao or Ciencia or Desconhecimento or NaoRealizada
            : svc ? lido.Tipo == Cancelamento : lido.Tipo is CartaCorrecao or Cancelamento or Substituicao;
        if (!aceito)
            return (Motivos.TipoEventoInvalido, "", null);
        if (lido.VersaoEvento != Versao)
            return (Motivos.VersaoEventoInvalida, "", null);
        if (SchemasLeiaute.Detalhe(lido.Tipo).Validar(Texto(lido.Detalhe)).Count > 0)
            return (Motivos.SchemaEventoInvalido, "", null);
        var autorInvalido = lido.AutorCnpj ? Motivos.CnpjInvalido : Motivos.CpfInvalido;
        if (lido.AutorCnpj ? !Documentos.CnpjValido(lido.Autor) : !Documentos.CpfValido(lido.Autor))
            return (autorInvalido, "", null);

        IReadOnlyList<Problema> assinatura;
        try
        {
            assinatura = AssinaturaDigital.Verificar(Texto(evento), lido.Id, lido.Autor);
        }
        catch (ArgumentException)
        {
            // O documento que a lib não aceita como CNPJ ou CPF.
            return (autorInvalido, "", null);
        }
        if (assinatura.Count > 0)
            return (assinatura[0].Codigo, "", null);

        var chave = lido.Chave;
        if (lido.Id != "ID" + lido.Tipo + chave + lido.Sequencia.ToString("D2", CultureInfo.InvariantCulture))
            return (Motivos.IdEventoDivergente, "", null);
        // O 249 (P12-40, a UF da chave não atendida) e o 614 (P12-14, a UF que não existe) ficam cobertos por
        // este, que vem antes na tabela.
        if (nacional ? lido.Orgao != pedido.Autorizador.CodigoCasa : lido.Orgao != chave[..2] || !pedido.Autorizador.Atende(lido.Orgao))
            return (Motivos.OrgaoDivergente, "", null);
        if (lido.Ambiente != "2")
            return (Motivos.AmbienteDivergente, "", null);
        if (Documentos.ConferirChave(chave, pedido.Recebimento) is { } invalida)
            return (invalida, "", null);
        if (!nacional && lido.Autor.PadLeft(14, '0') != chave[6..20])
            return (Motivos.AutorDivergente, "", null);
        if (lido.Data > pedido.Recebimento.AddMinutes(5))
            return (Motivos.EventoPosterior, "", null);
        if (!nacional && pedido.Irregulares!.Contem(lido.Autor))
            return (Motivos.EmitenteIrregular, "", null);
        if (repositorio.Eventos(chave).Any(e => e.Tipo == lido.Tipo && e.Sequencia == lido.Sequencia))
            return (Motivos.EventoDuplicado, "", null);
        if (nacional)
            return ConferirManifestacao(lido, pedido, repositorio);
        return lido.Tipo == CartaCorrecao ? ConferirCarta(lido, pedido, repositorio) : ConferirCancelamento(lido, pedido, repositorio);
    }

    // O Ambiente Nacional, que não autoriza nota de UF nenhuma.
    private static bool Nacional(Pedido pedido) => pedido.Autorizador.Atendidas.Count == 0;

    // A manifestação do destinatário (NT 2020.001 v1.60, 6.3.7 e 6.3.8), na ordem: na base, a nota pela chave (494,
    // a regra geral do evento que exige a NF-e), o autor contra o destinatário (2P21-10, 575: o CNPJ ou o CPF
    // inteiro), a data do evento contra a emissão (2P13-10, 577) e contra a autorização (2P13-14, 579); depois, as
    // do tipo: a sequência (H02, 594: 1 na ciência; até 2 nas conclusivas, pelo Ajuste SINIEF 43/23), o prazo desde
    // a autorização (H03, 596: 10 dias na ciência e 90 nas conclusivas, pelo Ajuste SINIEF 14/26), a ciência e o
    // desconhecimento da nota cancelada (H04, 650; H05, 651), a ciência depois de uma manifestação conclusiva (H06,
    // 655) e o tipo de emissão da chave (H08, 496: de 1 a 7). A justificativa da operação não realizada (H01, 595)
    // é o schema que confere (493). O H07 (658) não vale no AN.
    private static (string, string, NotaRegistrada?) ConferirManifestacao(Lido lido, Pedido pedido, Repositorio repositorio)
    {
        if (repositorio.Nota(lido.Chave) is not { } nota)
            return (Motivos.ChaveInexistente, "", null);
        var destinatario = Informacoes(nota).Element(NamespaceNFe + "dest")?.Elements().FirstOrDefault(e => e.Name.LocalName is "CNPJ" or "CPF");
        if (destinatario is null || destinatario.Value != lido.Autor || destinatario.Name.LocalName != (lido.AutorCnpj ? "CNPJ" : "CPF"))
            return (Motivos.AutorDestinatarioDivergente, "", null);
        if (lido.Data < Emissao(nota))
            return (Motivos.EventoAntesEmissao, "", null);
        if (lido.Chave[34] == '1' && lido.Data < nota.Recebimento.AddMinutes(-5))
            return (Motivos.EventoAntesAutorizacao, "", null);
        var ciencia = lido.Tipo == Ciencia;
        if (lido.Sequencia > (ciencia ? 1 : 2))
            return (Motivos.SequenciaEventoInvalida, "", null);
        var prazo = ciencia ? 10 : 90;
        if (pedido.Recebimento - nota.Recebimento > TimeSpan.FromDays(prazo))
            return (Motivos.EventoForaPrazo, string.Create(CultureInfo.InvariantCulture, $": {prazo} dias"), null);
        if (nota.Cancelada && ciencia)
            return (Motivos.CienciaNotaCancelada, "", null);
        if (nota.Cancelada && lido.Tipo == Desconhecimento)
            return (Motivos.DesconhecimentoNotaCancelada, "", null);
        if (ciencia && UltimaManifestacao(repositorio, lido.Chave) is not null)
            return (Motivos.CienciaDepoisManifestacao, "", null);
        if (lido.Chave[34] is < '1' or > '7')
            return (Motivos.TipoEmissaoChaveInvalido, "", null);
        return (Motivos.EventoVinculado, "", nota);
    }

    // A última manifestação conclusiva registrada (a que vale, 3.2.1.5): a confirmação, o desconhecimento ou a
    // operação não realizada. Nula sem nenhuma.
    private static string? UltimaManifestacao(Repositorio repositorio, string chave) =>
        repositorio.Eventos(chave).LastOrDefault(e => e.Tipo is Confirmacao or Desconhecimento or NaoRealizada)?.Tipo;

    // O cancelamento e o cancelamento por substituição (Tabela 5-38), na ordem: o tipo de emissão da nota
    // cancelada por substituição (P12-48, 920), a sequência (P15-10, 594), o órgão do autor (P20-10, 455), a
    // chave da substituta (P31-10 a P31-52, 910 e 911); na base, a nota (2P12-10, 494), o prazo (2P12-14 e
    // 2P12-18, 501), a nota já cancelada (2P12-22, 580), a data do evento contra a emissão (2P13-10, 577) e
    // contra a autorização (2P13-14, 579), o protocolo (2P23-10, 222), a NF-e com a operação confirmada pelo
    // destinatário (4P15-14, 221: a última manifestação conclusiva é a confirmação) e a substituta (5P31-10 a
    // 5P31-44: 912 a 919 e 921). O tipo do autor (P21-10, 466) é o schema que confere (493).
    private static (string, string, NotaRegistrada?) ConferirCancelamento(Lido lido, Pedido pedido, Repositorio repositorio)
    {
        var chave = lido.Chave;
        var substituicao = lido.Tipo == Substituicao;
        string Detalhe(string nome) => lido.Detalhe.Element(NamespaceNFe + nome)!.Value;
        if (substituicao && chave[34] != '1')
            return (Motivos.SubstituicaoTipoEmissao, "", null);
        if (lido.Sequencia != 1)
            return (Motivos.SequenciaEventoInvalida, "", null);
        if (substituicao)
        {
            if (Detalhe("cOrgaoAutor") != chave[..2])
                return (Motivos.OrgaoAutorDivergente, "", null);
            if (ConferirReferencia(Detalhe("chNFeRef"), chave, pedido.Recebimento) is { } referencia)
                return (referencia.Codigo, $" ({referencia.Campo})", null);
        }

        if (Procurar(chave, pedido, repositorio) is not { Nota: { } nota } achada)
            return (Motivos.ChaveInexistente, "", null);
        if (achada.Complemento.Length > 0)
            return (Motivos.ChaveInexistente, achada.Complemento, null);
        if (pedido.Recebimento - nota.Recebimento > TimeSpan.FromHours(substituicao ? 168 : 24))
            return (Motivos.PrazoCancelamento, "", null);
        if (nota.Cancelada)
            return (Motivos.NotaNaoAutorizada, "", null);
        if (lido.Data < Emissao(nota))
            return (Motivos.EventoAntesEmissao, "", null);
        if (chave[34] == '1' && lido.Data < nota.Recebimento.AddMinutes(-5))
            return (Motivos.EventoAntesAutorizacao, "", null);
        if (Detalhe("nProt") != nota.Protocolo)
            return (Motivos.ProtocoloDivergente, "", null);
        if (chave[20..22] == "55" && UltimaManifestacao(repositorio, chave) == Confirmacao)
            return (Motivos.RecebimentoConfirmado, "", null);
        if (substituicao && ConferirSubstituta(nota, Detalhe("chNFeRef"), repositorio) is { } substituta)
            return (substituta, "", null);
        return (Motivos.EventoVinculado, "", nota);
    }

    // A carta de correção (Tabela 5-40), na ordem: a sequência de 1 a 20 (GA03, 594), a NFC-e (GA03a, 784) e, na
    // base, a nota (494, regra geral do evento que exige a NF-e), a nota cancelada (GA01, 580) e a data do evento
    // contra a emissão (577, regra geral). A nova carta substitui a anterior: a consulta devolve todas.
    private static (string, string, NotaRegistrada?) ConferirCarta(Lido lido, Pedido pedido, Repositorio repositorio)
    {
        if (lido.Sequencia > 20)
            return (Motivos.SequenciaEventoInvalida, "", null);
        if (lido.Chave[20..22] == "65")
            return (Motivos.CartaCorrecaoNfce, "", null);
        if (Procurar(lido.Chave, pedido, repositorio) is not { Nota: { } nota } achada)
            return (Motivos.ChaveInexistente, "", null);
        if (achada.Complemento.Length > 0)
            return (Motivos.ChaveInexistente, achada.Complemento, null);
        if (nota.Cancelada)
            return (Motivos.NotaNaoAutorizada, "", null);
        if (lido.Data < Emissao(nota))
            return (Motivos.EventoAntesEmissao, "", null);
        return (Motivos.EventoVinculado, "", nota);
    }

    // A nota do evento pela numeração (2P12-10), como a consulta: na SVC, só as que ela autorizou. Achada com
    // outra chave, o complemento do 494 leva a chave da base (o autor já é o emitente, pelo 574).
    private static (NotaRegistrada? Nota, string Complemento) Procurar(string chave, Pedido pedido, Repositorio repositorio)
    {
        var nota = repositorio.NotaNumeracao(NotaRegistrada.Numerar(chave));
        if (nota is null || (pedido.Autorizador.TipoEmissao is not null && nota.Autorizador != pedido.Autorizador.Sigla))
            return (null, "");
        return (nota, nota.Chave == chave ? "" : $" [chNFe:{nota.Chave}]");
    }

    // A chave da NFC-e substituta (P31-10 a P31-52), com o nome do campo no motivo: inválida em si (910) ou
    // incorreta para a nota cancelada (911).
    private static (string Codigo, string Campo)? ConferirReferencia(string referencia, string chave, DateTimeOffset recebimento)
    {
        switch (Documentos.ConferirChave(referencia, recebimento))
        {
            case Motivos.ChaveDigitoInvalido:
                return (Motivos.SubstitutaInvalida, "Dígito");
            case Motivos.ChaveAnoInvalido:
                return (Motivos.SubstitutaInvalida, "Ano");
            case Motivos.ChaveMesInvalido:
                return (Motivos.SubstitutaInvalida, "Mês");
            case Motivos.ChaveDocumentoInvalido:
                return (Motivos.SubstitutaInvalida, "CNPJ/CPF");
            case Motivos.ChaveModeloInvalido:
                return (Motivos.SubstitutaInvalida, "Modelo");
            case Motivos.ChaveNumeroInvalido:
                return (Motivos.SubstitutaInvalida, "Número");
        }
        if (!UnidadesFederativas.Existe(referencia[..2]))
            return (Motivos.SubstitutaInvalida, "Código UF");
        if (referencia == chave)
            return (Motivos.SubstitutaIncorreta, "mesma Chave de Acesso");
        if (referencia[..2] != chave[..2])
            return (Motivos.SubstitutaIncorreta, "Código da UF");
        if (referencia[6..20] != chave[6..20])
            return (Motivos.SubstitutaIncorreta, "CNPJ/CPF");
        // O ano e o mês da substituta: o mesmo da cancelada ou o anterior.
        static int Meses(string valor) => (int.Parse(valor[2..4], CultureInfo.InvariantCulture) * 12) + int.Parse(valor[4..6], CultureInfo.InvariantCulture);
        if (Meses(referencia) > Meses(chave) || Meses(referencia) < Meses(chave) - 1)
            return (Motivos.SubstitutaIncorreta, "Ano-Mes");
        if (referencia[20..22] != chave[20..22])
            return (Motivos.SubstitutaIncorreta, "Modelo");
        return null;
    }

    // A substituta na base (5P31-10 a 5P31-44), contra a nota cancelada: a existência (912), a situação (913), a
    // emissão até 2 horas depois (914), o valor total (915), o ICMS (916), o destinatário (917), a quantidade de
    // itens (918), cada item (919) e o tipo de emissão de contingência (921).
    private static string? ConferirSubstituta(NotaRegistrada nota, string referencia, Repositorio repositorio)
    {
        if (repositorio.Nota(referencia) is not { } substituta)
            return Motivos.SubstitutaInexistente;
        if (substituta.Cancelada)
            return Motivos.SubstitutaCancelada;
        if (Emissao(substituta) > Emissao(nota).AddHours(2))
            return Motivos.SubstitutaEmissao;
        if (Total(substituta, "vNF") != Total(nota, "vNF"))
            return Motivos.SubstitutaValor;
        if (Total(substituta, "vICMS") != Total(nota, "vICMS"))
            return Motivos.SubstitutaIcms;
        if (Destinatario(substituta) != Destinatario(nota))
            return Motivos.SubstitutaDestinatario;
        var itens = Itens(nota);
        var itensSubstituta = Itens(substituta);
        if (itensSubstituta.Count != itens.Count)
            return Motivos.SubstitutaQuantidadeItens;
        if (itens.Zip(itensSubstituta).Any(par => !Iguais(par.First, par.Second)))
            return Motivos.SubstitutaItem;
        return substituta.Chave[34] == '1' ? Motivos.SubstitutaTipoEmissao : null;
    }

    // Os campos do item comparados no 919: os de texto como estão, os de quantidade e valor como número.
    private static bool Iguais(XElement produto, XElement outro)
    {
        string? Texto(XElement p, string nome) => p.Element(NamespaceNFe + nome)?.Value;
        decimal Numero(XElement p, string nome) => decimal.Parse(p.Element(NamespaceNFe + nome)!.Value, CultureInfo.InvariantCulture);
        return ((string[])["cProd", "cEAN", "xProd", "NCM", "CFOP", "uCom", "indTot"]).All(c => Texto(produto, c) == Texto(outro, c))
            && ((string[])["qCom", "vUnCom", "vProd"]).All(c => Numero(produto, c) == Numero(outro, c));
    }

    private static XElement Informacoes(NotaRegistrada nota) => nota.Xml.Element(NamespaceNFe + "infNFe")!;

    private static DateTimeOffset Emissao(NotaRegistrada nota) =>
        DateTimeOffset.ParseExact(Informacoes(nota).Element(NamespaceNFe + "ide")!.Element(NamespaceNFe + "dhEmi")!.Value,
            "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

    private static decimal Total(NotaRegistrada nota, string campo) =>
        decimal.Parse(Informacoes(nota).Element(NamespaceNFe + "total")!.Element(NamespaceNFe + "ICMSTot")!.Element(NamespaceNFe + campo)!.Value,
            CultureInfo.InvariantCulture);

    // A identificação do destinatário (5P31-32): o CNPJ, o CPF ou o idEstrangeiro, e a IE.
    private static (string?, string?) Destinatario(NotaRegistrada nota)
    {
        var destinatario = Informacoes(nota).Element(NamespaceNFe + "dest");
        var documento = destinatario?.Elements().FirstOrDefault(e => e.Name.LocalName is "CNPJ" or "CPF" or "idEstrangeiro");
        return (documento is null ? null : documento.Name.LocalName + ":" + documento.Value, destinatario?.Element(NamespaceNFe + "IE")?.Value);
    }

    private static List<XElement> Itens(NotaRegistrada nota) =>
        [.. Informacoes(nota).Elements(NamespaceNFe + "det").Select(d => d.Element(NamespaceNFe + "prod")!)];

    // O retEvento (Tabela 5-33, R10 a R31), sem o Id e sem a assinatura (a critério da UF). Registrado (135): o
    // xEvento, o protocolo e, conforme o tipo, o cOrgaoAutor (110112) ou o destinatário da nota (110111; na
    // manifestação, nenhum dos dois: NT 2020.001, R22 a R25). O dhRegEvento é o recebimento, também na rejeição.
    private static XElement RetornoEvento(Pedido pedido, Lido lido, string codigo, string motivo, string? protocolo, NotaRegistrada? nota)
    {
        var registrado = protocolo is not null;
        var destinatario = registrado && lido.Tipo == Cancelamento ? Informacoes(nota!).Element(NamespaceNFe + "dest") : null;
        var documento = destinatario?.Elements().FirstOrDefault(e => e.Name.LocalName is "CNPJ" or "CPF");
        return new(NamespaceNFe + "retEvento",
            new XAttribute("versao", Versao),
            Elemento("infEvento", new object?[]
            {
                Elemento("tpAmb", "2"),
                Elemento("verAplic", VersaoAplicativo(pedido)),
                Elemento("cOrgao", lido.Orgao),
                Elemento("cStat", codigo),
                Elemento("xMotivo", motivo),
                Elemento("chNFe", lido.Chave),
                Elemento("tpEvento", lido.Tipo),
                registrado ? Elemento("xEvento", Descricoes[lido.Tipo]) : null,
                Elemento("nSeqEvento", lido.Sequencia.ToString(CultureInfo.InvariantCulture)),
                registrado && lido.Tipo == Substituicao ? Elemento("cOrgaoAutor", lido.Detalhe.Element(NamespaceNFe + "cOrgaoAutor")!.Value) : null,
                documento is null ? null : Elemento(documento.Name.LocalName + "Dest", documento.Value),
                destinatario?.Element(NamespaceNFe + "email") is { } email ? Elemento("emailDest", email.Value) : null,
                Elemento("dhRegEvento", DataHora(pedido.Recebimento)),
                protocolo is null ? null : Elemento("nProt", protocolo),
            }));
    }

    // O evento como o schema genérico garante que veio (Tabela 5-32).
    private sealed record Lido(
        string Id, string Orgao, string Ambiente, string Autor, bool AutorCnpj, string Chave, DateTimeOffset Data,
        string Tipo, int Sequencia, string VersaoEvento, XElement Detalhe)
    {
        public static Lido Ler(XElement evento)
        {
            var informacao = evento.Element(NamespaceNFe + "infEvento")!;
            string Campo(string nome) => informacao.Element(NamespaceNFe + nome)!.Value;
            var cnpj = informacao.Element(NamespaceNFe + "CNPJ")?.Value;
            return new Lido(
                informacao.Attribute("Id")!.Value,
                Campo("cOrgao"),
                Campo("tpAmb"),
                cnpj ?? Campo("CPF"),
                cnpj is not null,
                Campo("chNFe"),
                DateTimeOffset.ParseExact(Campo("dhEvento"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                Campo("tpEvento"),
                int.Parse(Campo("nSeqEvento"), CultureInfo.InvariantCulture),
                Campo("verEvento"),
                informacao.Element(NamespaceNFe + "detEvento")!);
        }
    }
}
