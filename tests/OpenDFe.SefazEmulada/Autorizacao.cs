using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeAutorizacao4 (MOC 7.0, Visão Geral, 5.1; Anexo I, 4.1 e 4.2.1): o enviNFe e o retEnviNFe. No lote, na
// ordem: a raiz (565, 404, 568, 239), o schema do lote com as notas dentro (225), a resposta síncrona com
// mais de uma nota (764), o lote com NF-e e NFC-e (765) e a paralisação (108, 109). Passando o lote, cada
// nota é conferida e recebe o seu protNFe: na resposta síncrona (indSinc=1), o 104 com o protocolo; na
// assíncrona, o 103 com o recibo, e os protocolos ficam para o NFeRetAutorizacao4.
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
        string? protocolo = null;
        if (codigo == Motivos.Autorizado)
        {
            protocolo = repositorio.NovoProtocolo(pedido.Autorizador, Ide(nota, "cUF"), pedido.Recebimento);
            repositorio.Registrar(new NotaRegistrada(chave, Numeracao(nota), protocolo, recibo, pedido.Recebimento));
        }
        return Protocolo(pedido, chave, protocolo, resumo, codigo, Motivos.Texto(codigo) + complemento);
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
    // autorizadora (226) e a do lote (476), o tipo de emissão no autorizador (570, 783, 713), a data de
    // emissão (703, 228, 704) e a duplicidade (539, 204). O complemento vai depois do texto do motivo.
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

        // B09: 5 minutos de tolerância para o relógio do emitente; a NF-e normal até 30 dias atrás; a NFC-e
        // on-line até 5 minutos atrás (a de contingência, 9 e 4, fica para a E.3).
        if (emissao > pedido.Recebimento.AddMinutes(5))
            return (Motivos.EmissaoPosterior, "");
        if (modelo == "55" && tipoEmissao is "1" or "6" or "7" && emissao < pedido.Recebimento.AddDays(-30))
            return (Motivos.EmissaoAtrasada, "");
        if (modelo == "65" && tipoEmissao is "1" or "3" or "6" or "7" && emissao < pedido.Recebimento.AddMinutes(-5))
            return (Motivos.NfceAtrasada, "");

        if (repositorio.NotaNumeracao(Numeracao(nota)) is { } registrada)
        {
            var lote = registrada.Recibo is null ? "" : $"[nRec:{registrada.Recibo}]";
            return registrada.Chave == chave
                ? (Motivos.Duplicidade, lote.Length == 0 ? "" : " " + lote)
                : (Motivos.DuplicidadeChaveDiferente, $" [chNFe:{registrada.Chave}]{lote}");
        }
        return (Motivos.Autorizado, "");
    }

    // O que a duplicidade compara (Anexo I, 2B08-10): modelo, UF, CNPJ ou CPF do emitente, série e número; na
    // NFC-e, também o tipo de emissão.
    private static string Numeracao(XElement nota)
    {
        var emitente = nota.Element(NamespaceNFe + "infNFe")!.Element(NamespaceNFe + "emit")!;
        var documento = (emitente.Element(NamespaceNFe + "CNPJ") ?? emitente.Element(NamespaceNFe + "CPF"))!.Value;
        var modelo = Ide(nota, "mod");
        var numeracao = string.Join('|', modelo, Ide(nota, "cUF"), documento, Ide(nota, "serie"), Ide(nota, "nNF"));
        return modelo == "65" ? numeracao + "|" + Ide(nota, "tpEmis") : numeracao;
    }

    // Um campo do ide, que o schema garante.
    private static string Ide(XElement nota, string campo) =>
        nota.Element(NamespaceNFe + "infNFe")!.Element(NamespaceNFe + "ide")!.Element(NamespaceNFe + campo)!.Value;
}
