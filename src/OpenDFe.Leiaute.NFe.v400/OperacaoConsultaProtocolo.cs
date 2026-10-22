namespace OpenDFe.NFe;

/// <summary>
/// A consulta do protocolo (<c>NFeConsultaProtocolo4</c>, docs/CONTRATOS.md 15): o <c>consSitNFe</c> da chave e o
/// <c>retConsSitNFe</c> com a situação, o protocolo e os eventos. Na NF-e e na NFC-e (decisão 12).
/// </summary>
internal sealed class OperacaoConsultaProtocolo : IOperacao<ConsultaProtocolo, RetornoProtocolo>
{
    public PedidoWS Montar(ConsultaProtocolo parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        SituacaoNFe.ConferirModelo(parametro.Chave, contexto.Documento, "consSitNFe/chNFe", "Chave", problemas);
        return SituacaoNFe.Pedido(parametro.Chave, parametro.Destino, contexto, problemas);
    }

    public LeituraWS<RetornoProtocolo> Ler(ConsultaProtocolo parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var leitura = SituacaoNFe.Ler(xmlRetorno, "", problemas);
        var (ambiente, versao, uf, recebimento) = SituacaoNFe.Comuns(leitura, problemas);
        var retorno = new RetornoProtocolo(leitura.Status, leitura.Motivo, leitura.Eventos)
        {
            Ambiente = ambiente,
            VersaoAplicativo = versao,
            UF = uf,
            Recebimento = recebimento,
            Chave = leitura.Chave,
            Protocolo = leitura.Protocolo,
            ChaveRegistrada = leitura.Status == 562 ? MotivoSefaz.Chave(leitura.Motivo) : null,
        };
        return new LeituraWS<RetornoProtocolo>(retorno, leitura.Status, leitura.Motivo) { Problemas = problemas };
    }
}
