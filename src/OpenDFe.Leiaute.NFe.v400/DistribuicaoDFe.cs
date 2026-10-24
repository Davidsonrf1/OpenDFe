namespace OpenDFe.NFe;

/// <summary>
/// A consulta à distribuição de DF-e do Ambiente Nacional (<c>NFeDistribuicaoDFe</c>, NT 2014.002, docs/CONTRATOS.md 23):
/// o <c>distDFeInt</c> por último NSU, por NSU ou por chave, e o <c>retDistDFeInt</c> com os documentos já descompactados
/// (<see cref="RetornoDistribuicao"/>). Só na NF-e: a NFC-e não é distribuída. Sempre ao AN.
/// </summary>
/// <remarks>
/// <para>
/// O pedido não é assinado. O certificado do núcleo tem de ser o do interessado (a raiz do CNPJ, ou o CPF): a matriz
/// consulta as filiais; outro CNPJ é rejeitado (593, 472).
/// </para>
/// <para>
/// A lib não repete a consulta, não espera e não guarda o NSU (PROJETO.md 8.6). A NT bloqueia por 1 hora o CNPJ que
/// consulta a mais ou fora da sequência (656): o laço que respeita isso está em docs/DISTRIBUICAO.md.
/// </para>
/// </remarks>
public sealed class DistribuicaoDFe : ParametroWS<RetornoDistribuicao>
{
    /// <summary>O maior NSU: 15 algarismos (<c>TNSU</c>).</summary>
    private const long MaiorNsu = 999_999_999_999_999;

    private DistribuicaoDFe(string interessado, TipoConsultaDistribuicao consulta, long? nsu, ChaveAcesso? chave, UF? ufAutor)
    {
        if (ufAutor is { } uf && !Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(ufAutor), uf, "O valor não é uma UF.");
        Interessado = interessado;
        Consulta = consulta;
        NsuConsultado = nsu;
        ChaveConsultada = chave;
        UFAutor = ufAutor;
    }

    /// <summary>
    /// Os documentos a partir do NSU seguinte ao informado (0 na primeira vez): até 50 por resposta, em ordem de NSU.
    /// </summary>
    /// <param name="interessado">O CNPJ (numérico ou alfanumérico) ou o CPF do interessado, com ou sem máscara.</param>
    /// <param name="ultimoNsu">O último NSU que quem usa recebeu; 0 na primeira vez (traz os últimos 90 dias).</param>
    /// <param name="ufAutor">O <c>cUFAutor</c>; nulo é a UF do núcleo.</param>
    /// <returns>A consulta.</returns>
    /// <exception cref="ArgumentException">O interessado não é um CNPJ nem um CPF válido; o NSU é negativo ou passa de 15 algarismos; ou a UF não é conhecida.</exception>
    public static DistribuicaoDFe UltimoNsu(string interessado, long ultimoNsu = 0, UF? ufAutor = null) =>
        new(Normalizar(interessado), TipoConsultaDistribuicao.UltimoNsu, ConferirNsu(ultimoNsu, nameof(ultimoNsu)), null, ufAutor);

    /// <summary>O documento de um NSU: consulta pontual, que conta no limite de 20 por hora.</summary>
    /// <param name="interessado">O CNPJ (numérico ou alfanumérico) ou o CPF do interessado, com ou sem máscara.</param>
    /// <param name="nsu">O NSU consultado.</param>
    /// <param name="ufAutor">O <c>cUFAutor</c>; nulo é a UF do núcleo.</param>
    /// <returns>A consulta.</returns>
    /// <exception cref="ArgumentException">O interessado não é um CNPJ nem um CPF válido; o NSU é negativo ou passa de 15 algarismos; ou a UF não é conhecida.</exception>
    public static DistribuicaoDFe Nsu(string interessado, long nsu, UF? ufAutor = null) =>
        new(Normalizar(interessado), TipoConsultaDistribuicao.Nsu, ConferirNsu(nsu, nameof(nsu)), null, ufAutor);

    /// <summary>O documento de uma chave: consulta pontual, que conta no limite de 20 por hora.</summary>
    /// <param name="interessado">O CNPJ (numérico ou alfanumérico) ou o CPF do interessado, com ou sem máscara.</param>
    /// <param name="chave">A chave de acesso da NF-e (modelo 55).</param>
    /// <param name="ufAutor">O <c>cUFAutor</c>; nulo é a UF do núcleo.</param>
    /// <returns>A consulta.</returns>
    /// <exception cref="ArgumentException">O interessado não é um CNPJ nem um CPF válido, ou a UF não é conhecida.</exception>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    public static DistribuicaoDFe Chave(string interessado, ChaveAcesso chave, UF? ufAutor = null)
    {
        ArgumentNullException.ThrowIfNull(chave);
        return new(Normalizar(interessado), TipoConsultaDistribuicao.Chave, null, chave, ufAutor);
    }

    /// <summary>O CNPJ ou o CPF do interessado, só com os algarismos e as letras (sem máscara).</summary>
    public string Interessado { get; }

    /// <summary>Qual das três consultas é.</summary>
    public TipoConsultaDistribuicao Consulta { get; }

    /// <summary>O NSU: o <c>ultNSU</c> na consulta por último NSU, o <c>NSU</c> na consulta por NSU; nulo na consulta por chave.</summary>
    public long? NsuConsultado { get; }

    /// <summary>A chave da consulta por chave; nula nas outras.</summary>
    public ChaveAcesso? ChaveConsultada { get; }

    /// <summary>O <c>cUFAutor</c>: a UF do autor da consulta; nulo é a UF do núcleo.</summary>
    public UF? UFAutor { get; }

    private static string Normalizar(string interessado)
    {
        ArgumentNullException.ThrowIfNull(interessado);
        if (Cnpj.Valido(interessado))
            return Cnpj.RemoverMascara(interessado);
        if (Cpf.Valido(interessado))
            return Cpf.RemoverMascara(interessado);
        throw new ArgumentException("O interessado não é um CNPJ nem um CPF válido.", nameof(interessado));
    }

    private static long ConferirNsu(long nsu, string parametro)
    {
        if (nsu is < 0 or > MaiorNsu)
            throw new ArgumentOutOfRangeException(parametro, "O NSU tem de ser de 0 a 15 algarismos (999999999999999).");
        return nsu;
    }
}
