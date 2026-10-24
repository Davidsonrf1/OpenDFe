using OpenDFe.Impressao;

namespace OpenDFe.NFe;

/// <summary>A orientação da folha A4 do DANFE (Anexo II 3.6.1 e 3.6.3).</summary>
public enum OrientacaoDanfe
{
    /// <summary>A folha em pé (<c>tpImp</c> 1).</summary>
    Retrato,

    /// <summary>A folha deitada (<c>tpImp</c> 2).</summary>
    Paisagem,
}

/// <summary>As opções do DANFE da NF-e (<see cref="LeiauteNFe400.Danfe"/>).</summary>
/// <remarks>
/// Imutável. O <see cref="Papel"/> e a <see cref="Via"/> são só do DANFE Simplificado Tipo 2 (<c>tpImp</c> 6, bobina, no
/// leiaute do DANFE NFC-e); o retrato e a paisagem são sempre o A4 da orientação (docs/CONTRATOS.md 21).
/// </remarks>
public sealed class OpcoesDanfe
{
    /// <summary>O logotipo do emitente, no quadro de identificação; <see langword="null"/> sem logotipo.</summary>
    public Imagem1Bit? Logotipo { get; init; }

    /// <summary>
    /// Se o canhoto destacável sai (retrato); verdadeiro por padrão. Só o retrato pode suprimi-lo (Anexo II 3.3.1): na
    /// paisagem o canhoto sai sempre. Não vale para o Tipo 2.
    /// </summary>
    public bool Canhoto { get; init; } = true;

    /// <summary>
    /// A orientação do A4; <see langword="null"/> segue o <c>tpImp</c> da nota (1 retrato, 2 paisagem). Com o <c>tpImp</c> 6
    /// (Tipo 2) a orientação não existe e informá-la é recusado.
    /// </summary>
    public OrientacaoDanfe? Orientacao { get; init; }

    /// <summary>
    /// Se a NF-e foi cancelada: o documento sai com a marca d'água <c>CANCELADA</c>. A situação da nota vem de quem usa (a
    /// consulta ou o evento); a lib não a deduz.
    /// </summary>
    public bool Cancelada { get; init; }

    /// <summary>
    /// Uma linha livre ao fim do documento, para o que não está no XML (por exemplo, <c>"Impresso por ERP Core"</c>);
    /// <see langword="null"/> sem a linha.
    /// </summary>
    public string? TextoRodape { get; init; }

    /// <summary>
    /// O papel do DANFE Simplificado Tipo 2 (<see cref="PapelImpressao.Bobina80"/> por padrão; também a bobina de 58 mm ou o
    /// A4 em pé). <see langword="null"/> é o padrão; no retrato e na paisagem informá-lo é recusado.
    /// </summary>
    public PapelImpressao? Papel { get; init; }

    /// <summary>A via do DANFE Simplificado Tipo 2 (contingência offline); <see cref="ViaDanfce.Consumidor"/> por padrão.</summary>
    public ViaDanfce Via { get; init; }
}
