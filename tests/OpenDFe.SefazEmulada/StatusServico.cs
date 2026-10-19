using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeStatusServico4 (MOC 7.0, Visão Geral, 5.5): o consStatServ e o retConsStatServ. As regras, na ordem:
// a raiz (516, 404, 517, 239), a estrutura no lugar do XSD (215), o ambiente (K01, 252: o emulador é
// homologação), a UF atendida (K02, 289), a paralisação (K03 e K04, 108 e 109) e, passando tudo, 107.
internal sealed class StatusServico : Servico
{
    private const string Versao = "4.00";

    public override string Nome => NomeStatus;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "consStatServ", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (Campos(dados, "tpAmb", "cUF", "xServ") is not [var ambiente, var uf, var servico]
            || ambiente is not ("1" or "2") || !UnidadesFederativas.Existe(uf) || servico != "STATUS")
            return Rejeitar(Motivos.FalhaSchema, pedido);
        if (ambiente != "2")
            return Rejeitar(Motivos.AmbienteDivergente, pedido);
        if (!pedido.Autorizador.Atende(uf))
            return Rejeitar(Motivos.UFDivergente, pedido);
        if (pedido.Paralisacao is { } paralisacao)
            return Retorno(paralisacao.Codigo, pedido, uf, paralisacao.Observacao);
        return Retorno(Motivos.Operando, pedido, uf, null);
    }

    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, pedido.Autorizador.CodigoCasa, null);

    // O retConsStatServ (Tabela 5-18). O tMed (FR09) só no 107: o tempo médio de resposta é do serviço em
    // operação, e o emulador responde em menos de 1 segundo (4.3.6: abaixo de 1, informa 1).
    private static XElement Retorno(string codigo, Pedido pedido, string uf, string? observacao)
    {
        var retorno = new XElement(NamespaceNFe + "retConsStatServ",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cStat", codigo),
            Elemento("xMotivo", Motivos.Texto(codigo)),
            Elemento("cUF", uf),
            Elemento("dhRecbto", DataHora(pedido.Recebimento)));
        if (codigo == Motivos.Operando)
            retorno.Add(Elemento("tMed", "1"));
        if (observacao is not null)
            retorno.Add(Elemento("xObs", observacao));
        return retorno;
    }
}
