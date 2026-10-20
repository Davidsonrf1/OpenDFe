using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeConsultaProtocolo4 (MOC 7.0, Visão Geral, 5.4): o consSitNFe e o retConsSitNFe. As regras, na ordem:
// a raiz (516, 404, 517, 239), o schema (215) e as da Tabela 5-16: o ambiente (J01, 252), a UF da chave (J02,
// 226), o DV (J02a, 236), o ano (J02c, 615), o mês (J02d, 616), o CNPJ ou o CPF (J02e, 617), o modelo (J02f,
// 618) e o número (J02g, 619); a paralisação (108, 109); e, na base, a nota pela numeração (J03, 217), o
// código numérico (J04, 562), o mês (J05, 561) e o resto da chave (J06, 613). Achada, a situação da nota (100
// ou 150) com o protNFe da autorização. A SVC só acha as notas que ela autorizou (Anexo III, 2.1.3.4, f); o
// autorizador normal acha também as da SVC, que chegam a ele pelo Ambiente Nacional (h).
internal sealed class ConsultaProtocolo : Servico
{
    private const string Versao = "4.00";
    private const string ChaveVazia = "00000000000000000000000000000000000000000000";

    public override string Nome => NomeConsultaProtocolo;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "consSitNFe", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Consulta.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);
        var chave = dados.Element(NamespaceNFe + "chNFe")!.Value;
        if (dados.Element(NamespaceNFe + "tpAmb")!.Value != "2")
            return Retorno(Motivos.AmbienteDivergente, pedido, chave);
        if (ConferirChave(chave, pedido) is { } invalida)
            return Retorno(invalida, pedido, chave);
        if (pedido.Paralisacao is { } paralisacao)
            return Retorno(paralisacao.Codigo, pedido, chave);

        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
        {
            var nota = repositorio.NotaNumeracao(NotaRegistrada.Numerar(chave));
            if (nota is null || (pedido.Autorizador.TipoEmissao is not null && nota.Autorizador != pedido.Autorizador.Sigla))
                return Retorno(Motivos.NotaInexistente, pedido, chave);
            if (nota.Chave[35..43] != chave[35..43])
                return Retorno(Motivos.CodigoNumericoDivergente, pedido, chave, Complemento(nota, pedido));
            if (nota.Chave[4..6] != chave[4..6])
                return Retorno(Motivos.MesChaveDivergente, pedido, chave);
            if (nota.Chave != chave)
                return Retorno(Motivos.ChaveDivergenteBase, pedido, chave);
            return Retorno(nota.Situacao, pedido, chave, protocolo: nota.ProtocoloXml);
        }
    }

    // Antes de ler a chave, ela vai com zeros (o chNFe é obrigatório no retorno).
    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, ChaveVazia);

    // J02 a J02g. O J02b (614, a UF inválida) não aparece: a UF que não existe também não é atendida (226), que
    // vem antes na tabela. O J02k (526, a chave com mais de 6 meses) é a critério da UF e não é aplicado.
    private static string? ConferirChave(string chave, Pedido pedido)
    {
        if (!pedido.Autorizador.Atende(chave[..2]))
            return Motivos.UFEmitenteDivergente;
        if (Documentos.Modulo11(chave[..43]) != chave[43] - '0')
            return Motivos.ChaveDigitoInvalido;
        var ano = int.Parse(chave[2..4], CultureInfo.InvariantCulture);
        if (ano < 6 || ano > pedido.Recebimento.ToOffset(TimeSpan.FromHours(-3)).Year % 100)
            return Motivos.ChaveAnoInvalido;
        if (chave[4..6] is not ("01" or "02" or "03" or "04" or "05" or "06" or "07" or "08" or "09" or "10" or "11" or "12"))
            return Motivos.ChaveMesInvalido;
        // J02e (NT 2018.001): da série 0 a 909, o CNPJ; da 910 a 969, o CPF (com os três zeros à esquerda).
        var serie = int.Parse(chave[22..25], CultureInfo.InvariantCulture);
        var documento = chave[6..20];
        if ((serie <= 909 && !Documentos.CnpjValido(documento))
            || (serie is >= 910 and <= 969 && (!documento.StartsWith("000", StringComparison.Ordinal) || !Documentos.CpfValido(documento[3..]))))
            return Motivos.ChaveDocumentoInvalido;
        if (chave[20..22] is not ("55" or "65"))
            return Motivos.ChaveModeloInvalido;
        return chave[25..34] == "000000000" ? Motivos.ChaveNumeroInvalido : null;
    }

    // J04: a chave da nota na base, opcional no motivo, quando o transmissor é o emitente (a raiz do CNPJ ou o
    // CPF do certificado do TLS). O emulador não guarda o destinatário nem o transmissor da nota, que a regra
    // também admite.
    private static string Complemento(NotaRegistrada nota, Pedido pedido)
    {
        var documento = nota.Chave[6..20];
        var transmissor = pedido.Transmissor;
        var mesmo = transmissor.Length == 14 ? transmissor[..8] == documento[..8] : transmissor.Length == 11 && "000" + transmissor == documento;
        return mesmo ? $" [chNFe:{nota.Chave}]" : "";
    }

    // O retConsSitNFe (Tabela 5-14), com o cUF da casa do autorizador e o protNFe só na nota achada.
    private static XElement Retorno(string codigo, Pedido pedido, string chave, string complemento = "", XElement? protocolo = null) =>
        new(NamespaceNFe + "retConsSitNFe",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo) + complemento),
            Elemento("cUF", pedido.Autorizador.CodigoCasa),
            Elemento("dhRecbto", DataHora(pedido.Recebimento)),
            Elemento("chNFe", chave),
            protocolo is null ? null : new XElement(protocolo));
}
