using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>
/// A configuração de um núcleo (<see cref="DFe.Criar"/>, PROJETO.md 3.2): o ambiente, a UF do emitente, o
/// certificado, as conexões, o tipo de emissão da nota nova, as URLs trocadas, a parte do documento, os
/// callbacks de validação e o relógio.
/// </summary>
/// <remarks>
/// <see cref="DFe.Criar"/> confere a configuração e tira uma cópia das listas: mudar a lista depois não muda o
/// núcleo. Outra configuração (entrar em contingência, outro certificado) é outro núcleo, com as mesmas
/// <see cref="Conexoes"/>.
/// </remarks>
public sealed class ConfiguracaoDFe
{
    /// <summary>O ambiente (homologação ou produção).</summary>
    public required Ambiente Ambiente { get; init; }

    /// <summary>
    /// A UF do emitente: o destino natural das operações sem documento nem chave (PROJETO.md 8.4).
    /// </summary>
    public required UF UF { get; init; }

    /// <summary>
    /// O certificado A1 ou A3 com a chave privada RSA. É de quem chama: o núcleo não o descarta nem o copia, e
    /// descartá-lo com o núcleo em uso é erro de quem chama.
    /// </summary>
    public required X509Certificate2 Certificado { get; init; }

    /// <summary>O pool de conexões, compartilhado entre os núcleos (um por processo).</summary>
    public required ConexoesSefaz Conexoes { get; init; }

    /// <summary>
    /// O tipo de emissão da nota nova (PROJETO.md 8.4); o padrão é <see cref="TipoEmissao.Normal"/>. Diferente
    /// de <see cref="TipoEmissao.Normal"/> exige a <see cref="Contingencia"/>.
    /// </summary>
    public TipoEmissao Emissao { get; init; } = TipoEmissao.Normal;

    /// <summary>
    /// A entrada em contingência, obrigatória quando <see cref="Emissao"/> não é <see cref="TipoEmissao.Normal"/>
    /// (e recusada quando é).
    /// </summary>
    public Contingencia? Contingencia { get; init; }

    /// <summary>
    /// Só as URLs que mudam: cada uma substitui a do leiaute com o mesmo autorizador, ambiente e serviço, ou se
    /// acrescenta. O resultado está em <see cref="DFe.Enderecos"/>.
    /// </summary>
    public IReadOnlyList<EnderecoServico> Urls { get; init; } = [];

    /// <summary>
    /// A parte que é do documento, do tipo que o leiaute declara (<c>ConfiguracaoNFCe</c>, com o CSC);
    /// <see langword="null"/> quando o leiaute não declara nenhuma ou ela é opcional.
    /// </summary>
    public ConfiguracaoDocumento? Documento { get; init; }

    /// <summary>Os callbacks de validação de quem usa (PROJETO.md 6.3).</summary>
    public ValidacaoCallback? Validacao { get; init; }

    /// <summary>
    /// O relógio do rastro, dos tempos e do instante que o leiaute usar (PROJETO.md 2.3); o padrão é
    /// <see cref="TimeProvider.System"/>.
    /// </summary>
    public TimeProvider Relogio { get; init; } = TimeProvider.System;
}
