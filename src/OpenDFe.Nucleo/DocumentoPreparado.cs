namespace OpenDFe;

/// <summary>
/// O resultado do <see cref="DFe.Preparar{TDocumento}"/>: o documento assinado e validado, pronto para transmitir com
/// os mesmos bytes (PROJETO.md 8.3), ou os problemas que o impedem. Imutável.
/// </summary>
public sealed class DocumentoPreparado
{
    internal DocumentoPreparado(bool preparado, string? xml, ChaveAcesso? chave, IReadOnlyList<Problema> problemas)
    {
        Preparado = preparado;
        Xml = xml;
        Chave = chave;
        Problemas = problemas;
    }

    /// <summary>
    /// Sem problema de severidade <see cref="Severidade.Erro"/>: o <see cref="Xml"/> está assinado e válido no
    /// schema, nas regras do leiaute e nos callbacks.
    /// </summary>
    public bool Preparado { get; }

    /// <summary>
    /// O XML assinado: o que se transmite e se guarda (PROJETO.md 5.3). Sem <see cref="Preparado"/>, o XML até onde o
    /// caminho chegou (o montado, sem assinatura, quando a montagem tem erro); <see langword="null"/> quando a
    /// montagem não o produziu. Tem dados pessoais: guardar com o cuidado do documento, nunca em log.
    /// </summary>
    public string? Xml { get; }

    /// <summary>A chave de acesso do documento; nula quando a montagem não a fez.</summary>
    public ChaveAcesso? Chave { get; }

    /// <summary>Todos os problemas: os da montagem, do schema, das regras e dos callbacks, inclusive os avisos.</summary>
    public IReadOnlyList<Problema> Problemas { get; }
}
