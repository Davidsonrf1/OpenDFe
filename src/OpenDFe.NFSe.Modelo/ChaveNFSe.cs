using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// A chave de acesso da NFS-e (50 dígitos), gerada pelo ambiente que gera a nota: o município (7), o ambiente gerador (1),
/// o tipo de inscrição (1: CPF, 2: CNPJ), a inscrição (14, o CPF com 000 à esquerda), o número da NFS-e (13), o ano e o mês
/// da emissão (4), o código numérico (9) e o dígito verificador (1). É o Id do <c>infNFSe</c> sem o <c>NFS</c>.
/// </summary>
/// <remarks>
/// <para>
/// A leitura confere o tamanho, os dígitos, o ambiente gerador, o tipo de inscrição (e os zeros do CPF), o mês e o número
/// diferente de zero. <b>Não confere o dígito verificador</b>: a documentação diz só "dígito verificador do código
/// numérico", e nenhum cálculo conhecido bate com a chave de exemplo do guia oficial do emissor web e com uma chave de
/// exemplo de implementação aberta ao mesmo tempo (etapa 9.1, docs/NFSE.md 4; a regra <c>NFSE-CHAVE</c> caiu). Também
/// não confere o CPF nem o CNPJ: a chave é de quem gerou a nota, e a leitura de uma chave de terceiro não deve falhar.
/// </para>
/// <para>
/// A instância padrão (<see langword="default"/>) não é uma chave: <see cref="Valor"/> é vazio e as outras propriedades
/// lançam.
/// </para>
/// </remarks>
public readonly record struct ChaveNFSe
{
    /// <summary>O tamanho da chave: 50 dígitos.</summary>
    public const int Tamanho = 50;

    private readonly string? _valor;

    private ChaveNFSe(string valor)
    {
        _valor = valor;
    }

    /// <summary>A chave, os 50 dígitos.</summary>
    public string Valor => _valor ?? "";

    /// <summary>O código IBGE do município (as 7 primeiras posições).</summary>
    public int Municipio => Parte(0, 7);

    /// <summary>O ambiente que gerou a NFS-e (a 8ª posição).</summary>
    public AmbienteGerador AmbienteGerador => (AmbienteGerador)Parte(7, 1);

    /// <summary>Se a inscrição é um CPF (tipo 1); senão, é um CNPJ (tipo 2).</summary>
    public bool InscricaoCpf => Parte(8, 1) == 1;

    /// <summary>A inscrição do emitente: o CPF (11 dígitos, sem os zeros da chave) ou o CNPJ (14).</summary>
    public string Inscricao => InscricaoCpf ? Valor.Substring(12, 11) : Valor.Substring(9, 14);

    /// <summary>O número da NFS-e.</summary>
    public long Numero => long.Parse(Valor.AsSpan(23, 13), NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>O ano da emissão (2000 + as duas posições da chave).</summary>
    public int Ano => 2000 + Parte(36, 2);

    /// <summary>O mês da emissão.</summary>
    public int Mes => Parte(38, 2);

    /// <summary>O código numérico (9 posições), gerado pelo ambiente gerador.</summary>
    public int CodigoNumerico => Parte(40, 9);

    /// <summary>O dígito verificador, como está na chave (não conferido).</summary>
    public int DigitoVerificador => Parte(49, 1);

    /// <summary>Lê a chave.</summary>
    /// <param name="chave">Os 50 dígitos.</param>
    /// <returns>A chave.</returns>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    /// <exception cref="FormatException">
    /// O tamanho não é 50; há caractere que não é dígito; o ambiente gerador ou o tipo de inscrição não é 1 nem 2; o CPF
    /// não vem precedido de 000; o mês não vai de 1 a 12; o número é zero.
    /// </exception>
    public static ChaveNFSe Ler(string chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        return Conferir(chave) is { } motivo ? throw new FormatException(motivo) : new ChaveNFSe(chave);
    }

    /// <summary>Lê a chave, sem lançar.</summary>
    /// <param name="chave">Os 50 dígitos.</param>
    /// <param name="resultado">A chave, quando válida.</param>
    /// <returns>Se a chave é válida (as conferências de <see cref="Ler"/>).</returns>
    public static bool TentarLer([NotNullWhen(true)] string? chave, out ChaveNFSe resultado)
    {
        if (chave is not null && Conferir(chave) is null)
        {
            resultado = new ChaveNFSe(chave);
            return true;
        }
        resultado = default;
        return false;
    }

    /// <summary>Os 50 dígitos.</summary>
    /// <returns>A chave.</returns>
    public override string ToString() => Valor;

    private int Parte(int inicio, int tamanho) =>
        int.Parse(Valor.AsSpan(inicio, tamanho), NumberStyles.None, CultureInfo.InvariantCulture);

    // O motivo de a chave não servir, sem o valor (pode ter CPF); nulo quando serve.
    private static string? Conferir(string chave)
    {
        if (chave.Length != Tamanho)
            return "A chave da NFS-e tem 50 dígitos.";
        foreach (var c in chave)
            if (c is < '0' or > '9')
                return "A chave da NFS-e só tem dígitos.";
        if (chave[7] is not ('1' or '2'))
            return "O ambiente gerador da chave da NFS-e é 1 ou 2.";
        if (chave[8] is not ('1' or '2'))
            return "O tipo de inscrição da chave da NFS-e é 1 (CPF) ou 2 (CNPJ).";
        if (chave[8] == '1' && !chave.AsSpan(9, 3).SequenceEqual("000"))
            return "O CPF da chave da NFS-e vem precedido de 000.";
        var mes = int.Parse(chave.AsSpan(38, 2), NumberStyles.None, CultureInfo.InvariantCulture);
        if (mes is < 1 or > 12)
            return "O mês da chave da NFS-e vai de 1 a 12.";
        if (chave.AsSpan(23, 13).IndexOfAnyExcept('0') < 0)
            return "O número da NFS-e na chave não é zero.";
        return null;
    }
}
