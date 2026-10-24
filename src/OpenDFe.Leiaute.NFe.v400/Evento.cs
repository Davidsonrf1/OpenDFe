using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// Um evento de um documento (docs/CONTRATOS.md 18): o que o <see cref="EnvioEvento"/> leva. A base é fechada: só o
/// leiaute deriva (construtor <c>private protected</c> e membros internos), para o cancelamento, a substituição, a carta
/// de correção e a manifestação entrarem sem mudar o envio nem a operação.
/// </summary>
public abstract class Evento
{
    private protected Evento(ChaveAcesso chave, TipoEvento tipo, int sequencia)
    {
        ArgumentNullException.ThrowIfNull(chave);
        Chave = chave;
        Tipo = tipo;
        Sequencia = sequencia;
    }

    /// <summary>A chave de acesso do documento do evento (<c>chNFe</c>).</summary>
    public ChaveAcesso Chave { get; }

    /// <summary>O tipo do evento (<c>tpEvento</c>).</summary>
    public TipoEvento Tipo { get; }

    /// <summary>O sequencial do evento (<c>nSeqEvento</c>); 1 no cancelamento.</summary>
    public int Sequencia { get; }

    /// <summary>
    /// A data e hora do evento (<c>dhEvento</c>); nulo: o instante do relógio da configuração, no fuso de Brasília (-03:00).
    /// Para reenviar o mesmo evento (mesmos bytes e mesmo <c>Id</c>), informe o mesmo valor.
    /// </summary>
    public DateTimeOffset? Momento { get; init; }

    /// <summary>A descrição do evento (<c>descEvento</c>).</summary>
    internal abstract string Descricao { get; }

    /// <summary>
    /// O CNPJ (14 posições) ou o CPF (11) do autor: o emitente da chave (o CPF vem na chave com três zeros à esquerda).
    /// </summary>
    internal virtual string Autor
    {
        get
        {
            var emitente = Chave.Emitente;
            return Cnpj.Valido(emitente) || !emitente.StartsWith("000", StringComparison.Ordinal) ? emitente : emitente[3..];
        }
    }

    /// <summary>A versão do detalhe (<c>verEvento</c> e o <c>versao</c> do <c>detEvento</c>).</summary>
    internal virtual string VersaoDetalhe => "1.00";

    /// <summary>Só o cancelamento pode ir à SVC (Anexo III, 2.1.3, c e d).</summary>
    internal virtual bool AceitaContingencia => false;

    /// <summary>O schema do <c>detEvento</c> do tipo (os problemas saem com o 493 e a propriedade).</summary>
    internal abstract SchemaXml SchemaDetalhe { get; }

    /// <summary>Os textos livres do detalhe, para a conferência dos caracteres que o XML não aceita: a tag e a propriedade.</summary>
    internal abstract IEnumerable<(string Tag, string Propriedade, string Valor)> Textos { get; }

    /// <summary>O código do órgão de recepção (<c>cOrgao</c>): a UF da chave.</summary>
    internal virtual int Orgao(ContextoOperacao contexto) => Chave.UF.Codigo();

    /// <summary>O destino natural: o autorizador da UF da chave.</summary>
    internal virtual Destino Destino(ContextoOperacao contexto) => Chave.UF.ComoDestino();

    /// <summary>O tipo de emissão do pedido: o da chave só onde a SVC registra o evento; senão, o normal.</summary>
    internal virtual TipoEmissao EmissaoPedido => AceitaContingencia ? Chave.TipoEmissao : TipoEmissao.Normal;

    /// <summary>Escreve o <c>detEvento</c> inteiro, com o namespace da NF-e.</summary>
    internal abstract void EscreverDetalhe(XmlWriter xml);

    /// <summary>A propriedade do campo do detalhe (<c>"xJust"</c> vira <c>"Justificativa"</c>); nula sem propriedade.</summary>
    internal abstract string? PropriedadeDetalhe(string passo);

    /// <summary>A propriedade do campo pelo caminho a partir do <c>evento</c> (<c>"evento/infEvento/chNFe"</c>).</summary>
    internal string? Propriedade(string caminho)
    {
        var passos = caminho.Split('/');
        if (passos.Length < 3 || passos[0] != "evento" || passos[1] != "infEvento")
            return null;
        var campo = passos[2].TrimStart('@');
        if (campo != "detEvento")
        {
            return campo switch
            {
                "chNFe" => nameof(Chave),
                "tpEvento" => nameof(Tipo),
                "nSeqEvento" => nameof(Sequencia),
                "dhEvento" => nameof(Momento),
                _ => null,
            };
        }
        return passos.Length > 3 ? PropriedadeDetalhe(passos[3]) : null;
    }
}
