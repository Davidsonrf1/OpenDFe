using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeConsultaProtocolo4 (MOC 7.0, Visão Geral, 5.4): o consSitNFe e o retConsSitNFe. As regras, na ordem:
// a raiz (516, 404, 517, 239), o schema (215) e as da Tabela 5-16: o ambiente (J01, 252), a UF da chave (J02,
// 226), o DV (J02a, 236), o ano (J02c, 615), o mês (J02d, 616), o CNPJ ou o CPF (J02e, 617), o modelo (J02f,
// 618) e o número (J02g, 619); a paralisação (108, 109); e, na base, a nota pela numeração (J03, 217), o
// código numérico (J04, 562), o mês (J05, 561) e o resto da chave (J06, 613). Achada, a situação da nota (100
// ou 150; 101 depois do cancelamento) com o protNFe da autorização e o procEventoNFe de cada evento registrado
// (5.4.3: o cancelamento e a CC-e). A SVC só acha as notas que ela autorizou (Anexo III, 2.1.3.4, f); o
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
            return Retorno(nota.Cancelada ? Motivos.Cancelado : nota.Situacao, pedido, chave, protocolo: nota.ProtocoloXml,
                eventos: repositorio.Eventos(nota.Chave));
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
        return Documentos.ConferirChave(chave, pedido.Recebimento);
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

    // O retConsSitNFe (Tabela 5-14), com o cUF da casa do autorizador e o protNFe e os eventos só na nota achada. O
    // retCancNFe, do cancelamento anterior aos eventos, não sai.
    private static XElement Retorno(string codigo, Pedido pedido, string chave, string complemento = "", XElement? protocolo = null,
        IReadOnlyList<EventoRegistrado>? eventos = null) =>
        new(NamespaceNFe + "retConsSitNFe",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo) + complemento),
            Elemento("cUF", pedido.Autorizador.CodigoCasa),
            Elemento("dhRecbto", DataHora(pedido.Recebimento)),
            Elemento("chNFe", chave),
            protocolo is null ? null : new XElement(protocolo),
            eventos?.Select(e => new XElement(e.Processado)));
}
