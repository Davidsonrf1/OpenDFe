namespace OpenDFe.NFe;

/// <summary>
/// Um documento da distribuição DF-e (<c>docZip</c>, NT 2014.002, 3.2) já descompactado: o XML como veio e, nos resumos, os
/// campos nos tipos do modelo. Imutável.
/// </summary>
/// <remarks>
/// Para a NF-e processada (<see cref="TipoDocumentoDistribuido.NFeProcessada"/>) e o evento processado
/// (<see cref="TipoDocumentoDistribuido.EventoProcessado"/>), quem usa lê o <see cref="Xml"/> com
/// <see cref="LeiauteNFe400.Ler"/> (a <see cref="NotaLida"/>) ou direto: a leitura da nota não é duplicada aqui.
/// </remarks>
public sealed class DocumentoDistribuido
{
    internal DocumentoDistribuido(long? nsu, string schema, TipoDocumentoDistribuido tipo, string xml)
    {
        Nsu = nsu;
        Schema = schema;
        Tipo = tipo;
        Xml = xml;
    }

    /// <summary>O NSU do documento (<c>NSU</c> do <c>docZip</c>); nulo quando não veio, como na consulta pela chave.</summary>
    public long? Nsu { get; }

    /// <summary>O <c>schema</c> do <c>docZip</c> (<c>"resNFe_v1.01.xsd"</c>, <c>"procNFe_v4.00.xsd"</c>), como veio.</summary>
    public string Schema { get; }

    /// <summary>O tipo do documento, pelo <see cref="Schema"/>.</summary>
    public TipoDocumentoDistribuido Tipo { get; }

    /// <summary>O XML descompactado (gzip), em UTF-8, como veio.</summary>
    public string Xml { get; }

    /// <summary>A chave de acesso da NF-e do documento, lida do XML quando possível; nula no <see cref="TipoDocumentoDistribuido.Outro"/>.</summary>
    public ChaveAcesso? Chave { get; internal init; }

    /// <summary>O resumo da NF-e: só no tipo <see cref="TipoDocumentoDistribuido.ResumoNFe"/> (e nulo se o XML não se lê nos tipos).</summary>
    public ResumoNFe? ResumoNFe { get; internal init; }

    /// <summary>O resumo do evento: só no tipo <see cref="TipoDocumentoDistribuido.ResumoEvento"/> (e nulo se o XML não se lê nos tipos).</summary>
    public ResumoEvento? ResumoEvento { get; internal init; }
}
