using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.NFe;

/// <summary>
/// O que a escrita da nota tira do núcleo (MODELO.md, decisão 6): os campos da identificação que a configuração já
/// sabe: os da configuração do núcleo (<see cref="ContextoOperacao"/>), pelo <see cref="Criar"/>.
/// </summary>
internal sealed class ValoresNucleo
{
    /// <summary>O modelo do documento do núcleo (<c>mod</c>).</summary>
    public required Documento Documento { get; init; }

    /// <summary>A UF do emitente (<c>cUF</c>).</summary>
    public required UF UF { get; init; }

    /// <summary>O ambiente (<c>tpAmb</c>).</summary>
    public required Ambiente Ambiente { get; init; }

    /// <summary>O tipo de emissão (<c>tpEmis</c>).</summary>
    public TipoEmissao Emissao { get; init; } = TipoEmissao.Normal;

    /// <summary>A contingência (<c>dhCont</c> e <c>xJust</c>); nula na emissão normal.</summary>
    public Contingencia? Contingencia { get; init; }

    /// <summary>A configuração da NFC-e (o CSC e as URLs de consulta); nula quando não foi configurada.</summary>
    public ConfiguracaoNFCe? NFCe { get; init; }

    /// <summary>
    /// O certificado da configuração do núcleo: a versão 3 do QR Code offline assina os parâmetros com ele (etapa 5.2). Nulo
    /// fora do núcleo (nos testes da escrita).
    /// </summary>
    public X509Certificate2? Certificado { get; init; }

    /// <summary>Os valores da configuração do núcleo da operação.</summary>
    public static ValoresNucleo Criar(ContextoOperacao contexto)
    {
        var configuracao = contexto.Configuracao;
        return new ValoresNucleo
        {
            Documento = contexto.Documento,
            UF = configuracao.UF,
            Ambiente = configuracao.Ambiente,
            Emissao = configuracao.Emissao,
            Contingencia = configuracao.Contingencia,
            NFCe = configuracao.Documento as ConfiguracaoNFCe,
            Certificado = configuracao.Certificado,
        };
    }
}
