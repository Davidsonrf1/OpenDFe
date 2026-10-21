using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeAutorizacao4 (MOC 7.0, Visão Geral, 5.1; Anexo I, 4.1 e 4.2.1): o enviNFe e o retEnviNFe. No lote, na
// ordem: a raiz (565, 404, 568, 239), o schema do lote com as notas dentro (225), a resposta síncrona com
// mais de uma nota (764) e a assíncrona com uma só (452, NT 2025.001), o lote com NF-e e NFC-e (765) e a
// paralisação (108, 109). Passando o lote, cada nota é conferida e recebe o seu protNFe: na resposta síncrona
// (indSinc=1), o 104 com o protocolo; na assíncrona, o 103 com o recibo, e os protocolos ficam para o
// NFeRetAutorizacao4.
internal sealed class Autorizacao : Servico
{
    private const string Versao = "4.00";

    private static readonly XNamespace NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    public override string Nome => NomeAutorizacao;

    public override string RaizAusente => Motivos.RaizLoteInexistente;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "enviNFe", Versao, Motivos.RaizLoteInexistente, Motivos.VersaoLoteInexistente) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Lote.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchemaLote, pedido);
        var notas = dados.Elements(NamespaceNFe + "NFe").ToList();
        var sincrono = dados.Element(NamespaceNFe + "indSinc")!.Value == "1";
        if (sincrono && notas.Count > 1)
            return Rejeitar(Motivos.SincronoVariasNotas, pedido);
        if (!sincrono && notas.Count == 1)
            return Rejeitar(Motivos.AssincronoUmaNota, pedido);
        if (notas.Select(n => Ide(n, "mod")).Distinct(StringComparer.Ordinal).Count() > 1)
            return Rejeitar(Motivos.LoteMisturado, pedido);
        if (pedido.Paralisacao is { } paralisacao)
            return Rejeitar(paralisacao.Codigo, pedido);

        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
        {
            var recibo = sincrono ? null : repositorio.NovoRecibo(pedido.Autorizador);
            var primeira = Ide(notas[0], "cUF");
            var protocolos = notas.Select(n => Autorizar(n, primeira, recibo, pedido, repositorio)).ToList();
            if (sincrono)
                return Retorno(Motivos.LoteProcessado, pedido, protocolos[0]);
            repositorio.Registrar(new LoteRecebido(pedido.Autorizador.Sigla, recibo!, pedido.Transmissor, protocolos, pedido.Pendentes));
            return Retorno(Motivos.LoteRecebido, pedido,
                Elemento("infRec", new object[] { Elemento("nRec", recibo), Elemento("tMed", "1") }));
        }
    }

    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, null);

    // O retEnviNFe (Tabela 5-2), com o cUF da casa do autorizador: o da UF que atendeu.
    private static XElement Retorno(string codigo, Pedido pedido, XElement? resultado) =>
        new(NamespaceNFe + "retEnviNFe",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo)),
            Elemento("cUF", pedido.Autorizador.CodigoCasa),
            Elemento("dhRecbto", DataHora(pedido.Recebimento)),
            resultado);

    // Uma nota do lote: as regras, a autorização (com o protocolo e o registro) e o protNFe.
    private static XElement Autorizar(XElement nota, string primeira, string? recibo, Pedido pedido, Repositorio repositorio)
    {
        var chave = nota.Element(NamespaceNFe + "infNFe")!.Attribute("Id")!.Value[3..];
        var resumo = nota.Element(NamespaceAssinatura + "Signature")?.Element(NamespaceAssinatura + "SignedInfo")
            ?.Element(NamespaceAssinatura + "Reference")?.Element(NamespaceAssinatura + "DigestValue")?.Value;
        var (codigo, complemento) = Conferir(nota, chave, primeira, pedido, repositorio);
        if (codigo is not (Motivos.Autorizado or Motivos.AutorizadoForaPrazo))
            return Protocolo(pedido, chave, null, resumo, codigo, Motivos.Texto(codigo) + complemento);
        var protocolo = repositorio.NovoProtocolo(pedido.Autorizador, Ide(nota, "cUF"), pedido.Recebimento);
        var retorno = Protocolo(pedido, chave, protocolo, resumo, codigo, Motivos.Texto(codigo));
        var registrada = new NotaRegistrada(chave, NotaRegistrada.Numerar(chave), protocolo, recibo, pedido.Recebimento,
            pedido.Autorizador.Sigla, codigo, new XElement(retorno), new XElement(nota));
        repositorio.Registrar(registrada);
        repositorio.Interessados.Autorizada(registrada, pedido.Recebimento);
        return retorno;
    }

    // O protNFe (Tabela 5-6, PR01 a PR12), sem o Id e sem a assinatura, que são a critério da UF.
    public static XElement Protocolo(Pedido pedido, string chave, string? protocolo, string? resumo, string codigo, string motivo) =>
        new(NamespaceNFe + "protNFe",
            new XAttribute("versao", Versao),
            Elemento("infProt", new object?[]
            {
                Elemento("tpAmb", "2"),
                Elemento("verAplic", VersaoAplicativo(pedido)),
                Elemento("chNFe", chave),
                Elemento("dhRecbto", DataHora(pedido.Recebimento)),
                protocolo is null ? null : Elemento("nProt", protocolo),
                resumo is null ? null : Elemento("digVal", resumo),
                Elemento("cStat", codigo),
                Elemento("xMotivo", motivo),
            }));

    // As regras da nota, na ordem: o documento do emitente (207, 401), a assinatura e o titular (grupos E e F:
    // 290, 292, 297, 298, 213, 227), a chave contra os campos (502) e o DV (253), o ambiente (252), a UF
    // autorizadora (226) e a do lote (476), o tipo de emissão (711, 570, 783, 713), a data de emissão (703,
    // 228, 704 e o 150 da autorização fora de prazo), o QR Code (grupo ZX), o emitente irregular (1C17-38, 781),
    // a duplicidade (539, 204 e 218 da nota cancelada), a numeração inutilizada (3B08-100, 206) e o destinatário
    // irregular da NF-e (5E17-40, 302). O complemento vai depois do texto do motivo.
    private static (string Codigo, string Complemento) Conferir(XElement nota, string chave, string primeira, Pedido pedido, Repositorio repositorio)
    {
        var emitente = nota.Element(NamespaceNFe + "infNFe")!.Element(NamespaceNFe + "emit")!;
        var cnpj = emitente.Element(NamespaceNFe + "CNPJ")?.Value;
        var documento = cnpj ?? emitente.Element(NamespaceNFe + "CPF")!.Value;
        if (cnpj is not null ? !Documentos.CnpjValido(cnpj) : !Documentos.CpfValido(documento))
            return (cnpj is not null ? Motivos.CnpjEmitenteInvalido : Motivos.CpfEmitenteInvalido, "");

        IReadOnlyList<Problema> assinatura;
        try
        {
            assinatura = AssinaturaDigital.Verificar(Texto(nota), "NFe" + chave, documento);
        }
        catch (ArgumentException)
        {
            // O documento que a lib não aceita como CNPJ ou CPF.
            return (cnpj is not null ? Motivos.CnpjEmitenteInvalido : Motivos.CpfEmitenteInvalido, "");
        }
        if (assinatura.Count > 0)
            return (assinatura[0].Codigo, "");

        var uf = Ide(nota, "cUF");
        var modelo = Ide(nota, "mod");
        var tipoEmissao = Ide(nota, "tpEmis");
        var emissao = DateTimeOffset.ParseExact(Ide(nota, "dhEmi"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        var composta = Documentos.Chave(uf, emissao, documento, modelo, Ide(nota, "serie"), Ide(nota, "nNF"), tipoEmissao, Ide(nota, "cNF"), Ide(nota, "cDV"));
        if (composta != chave)
            return (Motivos.ChaveDivergenteCampos, "");
        if (Documentos.Modulo11(chave[..43]) != chave[43] - '0')
            return (Motivos.DigitoChaveInvalido, "");
        if (Ide(nota, "tpAmb") != "2")
            return (Motivos.AmbienteDivergente, "");
        if (!pedido.Autorizador.Atende(uf))
            return (Motivos.UFEmitenteDivergente, "");
        if (uf != primeira)
            return (Motivos.UFPrimeiraNotaDivergente, "");

        // B22-10 (NT 2026.002): a NF-e offline só com o DANFE Simplificado Tipo 2.
        var simplificada = modelo == "65" || Ide(nota, "tpImp") == "6";
        if (modelo == "55" && tipoEmissao == "9" && !simplificada)
            return (Motivos.NfeOffline, "");
        if (pedido.Autorizador.TipoEmissao is null)
        {
            if (tipoEmissao is "3" or "6" or "7")
                return (Motivos.TipoEmissaoContingencia, "");
        }
        else if (modelo == "65")
        {
            return (Motivos.NfceSvc, "");
        }
        else if (tipoEmissao != pedido.Autorizador.TipoEmissao)
        {
            return (Motivos.TipoEmissaoSvc, "");
        }

        // B09 (NT 2025.001 e NT 2026.002): 5 minutos de tolerância para o relógio do emitente. A NFC-e e a NF-e
        // com DANFE Simplificado Tipo 2 (B09-40): até 5 minutos atrás, salvo na contingência (4 e 9), que
        // depois de 24 horas é autorizada fora de prazo. A outra NF-e (B09-20): até 7 dias, 100; depois, 150; e,
        // a critério da UF (o emulador aplica), depois de 30 dias só a de contingência (2, 4, 5, 9).
        if (emissao > pedido.Recebimento.AddMinutes(5))
            return (Motivos.EmissaoPosterior, "");
        var situacao = Motivos.Autorizado;
        if (simplificada)
        {
            if (tipoEmissao is "1" or "3" or "6" or "7" && emissao < pedido.Recebimento.AddMinutes(-5))
                return (Motivos.NfceAtrasada, "");
            if (emissao < pedido.Recebimento.AddHours(-24))
                situacao = Motivos.AutorizadoForaPrazo;
        }
        else
        {
            if (tipoEmissao is "1" or "3" or "6" or "7" && emissao < pedido.Recebimento.AddDays(-30))
                return (Motivos.EmissaoAtrasada, "");
            if (emissao < pedido.Recebimento.AddDays(-7))
                situacao = Motivos.AutorizadoForaPrazo;
        }

        if (QrCode.Conferir(nota, chave, pedido.Codigos!) is { } qrCode)
            return qrCode;

        // Os irregulares (NT 2024.001: sem denegação), pelo cadastro ou pela marca da própria nota.
        var marca = Marca.Irregular(nota);
        if (marca.Emitente || pedido.Irregulares!.Contem(documento))
            return (Motivos.EmissorNaoHabilitado, "");

        if (repositorio.NotaNumeracao(NotaRegistrada.Numerar(chave)) is { } registrada)
        {
            var lote = registrada.Recibo is null ? "" : $"[nRec:{registrada.Recibo}]";
            if (registrada.Chave != chave)
                return (Motivos.DuplicidadeChaveDiferente, $" [chNFe:{registrada.Chave}]{lote}");
            return (registrada.Cancelada ? Motivos.NotaCancelada : Motivos.Duplicidade, lote.Length == 0 ? "" : " " + lote);
        }

        // 3B08-100 (NT 2020.005): a numeração inutilizada, na emissão normal e na SVC; em SP, também na FS-IA, na
        // EPEC e na FS-DA. A NFC-e offline (9) não é conferida.
        var numero = long.Parse(Ide(nota, "nNF"), CultureInfo.InvariantCulture);
        if ((tipoEmissao is "1" or "6" or "7" || (uf == "35" && tipoEmissao is "2" or "4" or "5"))
            && repositorio.Inutilizado(uf, chave[6..20], modelo, int.Parse(Ide(nota, "serie"), CultureInfo.InvariantCulture), numero, numero))
            return (Motivos.NumeracaoInutilizada, "");

        // 5E17-40 (NT 2024.001): o destinatário irregular da NF-e, rejeitado, e não mais denegado.
        var destinatario = nota.Element(NamespaceNFe + "infNFe")!.Element(NamespaceNFe + "dest");
        var documentoDestinatario = destinatario?.Element(NamespaceNFe + "CNPJ")?.Value ?? destinatario?.Element(NamespaceNFe + "CPF")?.Value;
        if (modelo == "55" && (marca.Destinatario || pedido.Irregulares!.Contem(documentoDestinatario)))
            return (Motivos.DestinatarioIrregular, "");
        return (situacao, "");
    }

    // Um campo do ide, que o schema garante.
    private static string Ide(XElement nota, string campo) =>
        nota.Element(NamespaceNFe + "infNFe")!.Element(NamespaceNFe + "ide")!.Element(NamespaceNFe + campo)!.Value;
}
