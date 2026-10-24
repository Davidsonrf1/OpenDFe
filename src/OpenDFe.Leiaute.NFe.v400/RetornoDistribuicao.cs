namespace OpenDFe.NFe;

/// <summary>
/// O retorno da <see cref="DistribuicaoDFe"/> (o <c>retDistDFeInt</c> 1.01, docs/CONTRATOS.md 23): o status, os
/// documentos já descompactados e a posição do NSU. Imutável.
/// </summary>
/// <remarks>
/// A lib não repete a consulta, não espera e não guarda o NSU (PROJETO.md 8.6): <see cref="TemMais"/> e
/// <see cref="EsperaSugerida"/> dizem o que a NT manda, e o laço é de quem usa (docs/DISTRIBUICAO.md).
/// </remarks>
public sealed class RetornoDistribuicao
{
    private readonly bool _distribuicaoNsu;

    internal RetornoDistribuicao(int status, string motivo, IReadOnlyList<DocumentoDistribuido> documentos, bool distribuicaoNsu)
    {
        Status = status;
        Motivo = motivo;
        Documentos = documentos;
        _distribuicaoNsu = distribuicaoNsu;
    }

    /// <summary>O <c>cStat</c>: 137 (nenhum documento), 138 (documentos), 656 (consumo indevido) ou a rejeição.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c>.</summary>
    public string Motivo { get; }

    /// <summary>O <c>tpAmb</c>; nulo quando não veio ou não se leu.</summary>
    public Ambiente? Ambiente { get; internal init; }

    /// <summary>O <c>verAplic</c>: a versão do Web Service que processou a consulta.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>O <c>dhResp</c>: a data e a hora da resposta.</summary>
    public DateTimeOffset? Resposta { get; internal init; }

    /// <summary>
    /// O <c>ultNSU</c>: o último NSU pesquisado, de onde a próxima consulta continua. No 656 da distribuição por NSU é o da
    /// última consulta que o Ambiente Nacional respondeu para o CNPJ ou CPF (NT 2014.002, 3.11.4). Nulo se não se lê.
    /// </summary>
    public long? UltimoNsu { get; internal init; }

    /// <summary>O <c>maxNSU</c>: o maior NSU existente no Ambiente Nacional para o CNPJ ou CPF; nulo se não se lê.</summary>
    public long? MaximoNsu { get; internal init; }

    /// <summary>Os documentos da resposta, em ordem de NSU; vazia sem documento (e os <c>docZip</c> ilegíveis ficam de fora, com aviso).</summary>
    public IReadOnlyList<DocumentoDistribuido> Documentos { get; }

    /// <summary>Nenhum documento localizado (<see cref="Status"/> 137).</summary>
    public bool NenhumDocumento => Status == 137;

    /// <summary>O consumo indevido (<see cref="Status"/> 656): o CNPJ ou CPF está bloqueado por 1 hora.</summary>
    public bool ConsumoIndevido => Status == 656;

    /// <summary>
    /// Há mais a buscar: a distribuição por NSU com 138 e <see cref="UltimoNsu"/> menor que <see cref="MaximoNsu"/>. Chame de
    /// novo na hora, com o <see cref="UltimoNsu"/> devolvido. Falso nas consultas pontuais, que respondem um documento só.
    /// </summary>
    public bool TemMais => _distribuicaoNsu && Status == 138 && UltimoNsu is { } ultimo && MaximoNsu is { } maximo && ultimo < maximo;

    /// <summary>
    /// A espera que a NT manda antes da próxima consulta (3.5 e 3.11.4): 1 hora na distribuição por NSU que não tem mais o que
    /// entregar (o 137, ou o 138 cujo <see cref="UltimoNsu"/> já alcançou o <see cref="MaximoNsu"/>) e no 656 (o tempo recomeça
    /// a cada consulta feita no bloqueio); nula nos outros, e quando há <see cref="TemMais"/>. A lib não espera: é de quem usa.
    /// </summary>
    public TimeSpan? EsperaSugerida => Status == 656 || (_distribuicaoNsu && EsgotouEntrega) ? TimeSpan.FromHours(1) : null;

    private bool EsgotouEntrega =>
        Status == 137 || (Status == 138 && UltimoNsu is { } ultimo && MaximoNsu is { } maximo && ultimo >= maximo);
}
