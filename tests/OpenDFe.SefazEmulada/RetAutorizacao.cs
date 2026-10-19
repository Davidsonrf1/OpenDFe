using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeRetAutorizacao4 (MOC 7.0, Visão Geral, 5.2): o consReciNFe e o retConsReciNFe. As regras, na ordem: a
// raiz (516, 404, 517, 239), o schema (215), o ambiente (E01, 252), a UF do recibo (E02, 248) e o tipo de
// autorizador (E02a, 553), a paralisação (108, 109), o lote (E03, 106), o transmissor (E05, 223) e o lote
// ainda em processamento (E04, 105, comandado). Passando tudo, 104 com os protocolos das notas.
internal sealed class RetAutorizacao : Servico
{
    private const string Versao = "4.00";
    private const string ReciboVazio = "000000000000000";

    public override string Nome => NomeRetAutorizacao;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "consReciNFe", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Recibo.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);
        var recibo = dados.Element(NamespaceNFe + "nRec")!.Value;
        if (dados.Element(NamespaceNFe + "tpAmb")!.Value != "2")
            return Retorno(Motivos.AmbienteDivergente, pedido, recibo);
        if (recibo[..2] != pedido.Autorizador.CodigoCasa)
            return Retorno(Motivos.UFReciboDivergente, pedido, recibo);
        if (recibo[2] != pedido.Autorizador.Tipo)
            return Retorno(Motivos.TipoAutorizadorDivergente, pedido, recibo);
        if (pedido.Paralisacao is { } paralisacao)
            return Retorno(paralisacao.Codigo, pedido, recibo);

        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
        {
            var lote = repositorio.Lote(pedido.Autorizador.Sigla, recibo);
            if (lote is null)
                return Retorno(Motivos.LoteNaoLocalizado, pedido, recibo);
            if (lote.Transmissor != pedido.Transmissor)
                return Retorno(Motivos.TransmissorDivergente, pedido, recibo);
            if (pedido.Processando)
                return Retorno(Motivos.LoteProcessando, pedido, recibo);
            if (lote.Pendentes > 0)
            {
                lote.Pendentes--;
                return Retorno(Motivos.LoteProcessando, pedido, recibo);
            }
            return Retorno(Motivos.LoteProcessado, pedido, recibo, lote.Protocolos);
        }
    }

    // BR06: o nRec com zeros quando não dá para obtê-lo do pedido.
    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, ReciboVazio);

    // O retConsReciNFe (Tabela 5-6), com o cUF da casa do autorizador. Os protocolos só no 104.
    private static XElement Retorno(string codigo, Pedido pedido, string recibo, IReadOnlyList<XElement>? protocolos = null) =>
        new(NamespaceNFe + "retConsReciNFe",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("nRec", recibo),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo)),
            Elemento("cUF", pedido.Autorizador.CodigoCasa),
            Elemento("dhRecbto", DataHora(pedido.Recebimento)),
            protocolos?.Select(p => new XElement(p)));
}
