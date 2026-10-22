namespace OpenDFe.NFe;

/// <summary>
/// A recuperação da autorização (<c>NFeConsultaProtocolo4</c>, PROJETO.md 8.6, docs/CONTRATOS.md 15): a nota assinada
/// lida até a chave (com a assinatura conferida, como na autorização), o <c>consSitNFe</c> da chave e, achado o protocolo
/// de autorização ou de denegação do mesmo XML (o <c>digVal</c>), o <c>nfeProc</c>. Só a NF-e nesta versão, como a
/// autorização (decisão 12).
/// </summary>
internal sealed class OperacaoRecuperacao : IOperacao<RecuperacaoAutorizacao, RetornoAutorizacao>
{
    private const string Prefixo = "Xml.";

    public PedidoWS Montar(RecuperacaoAutorizacao parametro, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var chave = LoteNFe.LerAssinada(LoteNFe.Normalizar(parametro.Xml), Prefixo, problemas);
        if (chave is not null)
            SituacaoNFe.ConferirModelo(chave, contexto.Documento, "NFe/infNFe/@Id", Prefixo.TrimEnd('.'), problemas);
        return SituacaoNFe.Pedido(chave, parametro.Destino, contexto, problemas);
    }

    public LeituraWS<RetornoAutorizacao> Ler(RecuperacaoAutorizacao parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var problemas = new List<Problema>();
        var leitura = SituacaoNFe.Ler(xmlRetorno, "Notas[0].", problemas);
        var (ambiente, versao, uf, recebimento) = SituacaoNFe.Comuns(leitura, problemas);

        var xml = LoteNFe.Normalizar(parametro.Xml);
        var chave = LoteNFe.Chave(xml);
        var protocolo = leitura.Protocolo;
        string? processada = null;
        if (protocolo is not null && protocolo.Chave != chave?.ToString())
        {
            problemas.Add(new Problema(CodigosLeitura.Protocolo, Severidade.Aviso,
                $"O protocolo da resposta é da chave {protocolo.Chave}, não da nota: fica fora.")
            {
                Tag = "chNFe",
                Caminho = "retConsSitNFe/protNFe/infProt/chNFe",
            });
            protocolo = null;
        }
        else if (protocolo is not null && ResultadoNota.Processada(protocolo.Status))
        {
            if (protocolo.Digest is { } digest && digest == LoteNFe.Digest(xml))
                processada = LoteNFe.NotaProcessada(xml, leitura.ElementoProtocolo!);
            else
                problemas.Add(new Problema(CodigosLeitura.Protocolo, Severidade.Aviso, protocolo.Digest is null
                    ? "O protocolo não traz o digVal: não há como provar que é desta nota, e o nfeProc não sai."
                    : "O digVal do protocolo não é o DigestValue da nota: a SEFAZ tem outro XML com a mesma chave, e o nfeProc não sai.")
                {
                    Tag = "digVal",
                    Caminho = "retConsSitNFe/protNFe/infProt/digVal",
                    Propriedade = "Notas[0].Protocolo.Digest",
                });
        }

        var retorno = new RetornoAutorizacao(leitura.Status, leitura.Motivo, [new ResultadoNota(chave, xml, protocolo, processada)])
        {
            Ambiente = ambiente,
            VersaoAplicativo = versao,
            UF = uf,
            Recebimento = recebimento,
        };
        return new LeituraWS<RetornoAutorizacao>(retorno, leitura.Status, leitura.Motivo) { Problemas = problemas };
    }
}
