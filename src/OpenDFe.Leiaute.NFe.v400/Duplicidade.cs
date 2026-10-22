namespace OpenDFe.NFe;

/// <summary>
/// A nota já registrada na SEFAZ que causou a rejeição por duplicidade (204 e 539, PROJETO.md 8.6, docs/CONTRATOS.md
/// 15): no 204, a mesma nota (recuperar o <c>nfeProc</c> com a <see cref="RecuperacaoAutorizacao"/>); no 539, outra
/// chave com a mesma numeração (consultar a outra com a <see cref="ConsultaProtocolo"/>). Imutável.
/// </summary>
public sealed class Duplicidade
{
    private Duplicidade(ChaveAcesso? chave, string? recibo, bool mesmaChave)
    {
        Chave = chave;
        Recibo = recibo;
        MesmaChave = mesmaChave;
    }

    /// <summary>
    /// A chave da nota registrada: no 204, a da própria nota; no 539, a do motivo (<c>[chNFe:...]</c>), nula quando ele
    /// não a traz.
    /// </summary>
    public ChaveAcesso? Chave { get; }

    /// <summary>O recibo do lote em que a nota registrada foi autorizada (<c>[nRec:...]</c>), quando o motivo o traz.</summary>
    public string? Recibo { get; }

    /// <summary>A nota registrada tem a mesma chave (204); falso: outra chave com a mesma numeração (539).</summary>
    public bool MesmaChave { get; }

    /// <summary>A duplicidade do protocolo de uma nota, no 204 e no 539; nula nos outros.</summary>
    internal static Duplicidade? Ler(Protocolo? protocolo, ChaveAcesso? chave) => protocolo?.Status switch
    {
        204 => new Duplicidade(chave ?? MotivoSefaz.Chave(protocolo.Motivo), MotivoSefaz.Recibo(protocolo.Motivo), mesmaChave: true),
        539 => new Duplicidade(MotivoSefaz.Chave(protocolo.Motivo), MotivoSefaz.Recibo(protocolo.Motivo), mesmaChave: false),
        _ => null,
    };
}
