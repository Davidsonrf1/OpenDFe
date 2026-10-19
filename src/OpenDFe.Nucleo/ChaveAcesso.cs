using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;

namespace OpenDFe;

/// <summary>
/// Chave de acesso do documento fiscal eletrônico: as 44 posições que o identificam (MOC 7.0, Visão
/// Geral, 2.2.6; NT 2025.001, item 5). Imutável; duas chaves são iguais quando as 44 posições são iguais.
/// </summary>
/// <remarks>
/// <para>
/// A composição, da esquerda para a direita: código IBGE da UF do emitente (2), ano e mês da emissão,
/// <c>AAMM</c> (4), CNPJ ou CPF do emitente (14), modelo (2), série (3), número (9), tipo de emissão (1),
/// código numérico, <c>cNF</c> (8), e o dígito verificador (1).
/// </para>
/// <para>
/// O CNPJ pode ser alfanumérico (NT 2025.001): as 12 primeiras posições do CNPJ aceitam letras
/// maiúsculas, e todas as outras posições da chave são dígitos. O emitente pessoa física (produtor rural,
/// NT 2018.001 e NT 2023.002) entra com o CPF precedido de três zeros, completando as 14 posições; a chave
/// sozinha não diz se o emitente é CPF ou CNPJ (quem diz é a série e o XML), por isso
/// <see cref="Emitente"/> devolve as 14 posições como estão.
/// </para>
/// <para>
/// O dígito verificador é o módulo 11 das 43 primeiras posições, com os pesos de 2 a 9 da direita para a
/// esquerda e cada caractere valendo o código ASCII menos 48 (o mesmo cálculo do CNPJ alfanumérico).
/// </para>
/// </remarks>
public sealed class ChaveAcesso : IEquatable<ChaveAcesso>
{
    /// <summary>Quantidade de posições da chave de acesso.</summary>
    public const int Tamanho = 44;

    private const int MaiorSerie = 999;
    private const int MaiorNumero = 999_999_999;
    private const int LimiteCodigoNumerico = 100_000_000;
    private const int PrimeiroAno = 2000;
    private const int UltimoAno = 2099;

    // MOC 7.0, Anexo I, regra B09-20 (rejeição 897, NT 2019.001): o cNF não pode ser uma destas
    // sequências, nem igual ao número do documento.
    private static readonly int[] CodigosNumericosRecusados =
    [
        00000000, 11111111, 22222222, 33333333, 44444444, 55555555, 66666666, 77777777, 88888888, 99999999,
        12345678, 23456789, 34567890, 45678901, 56789012, 67890123, 78901234, 89012345, 90123456, 01234567,
    ];

    private readonly string _valor;

    private ChaveAcesso(string valor, UF uf, int ano, int mes, Documento modelo, int serie, int numero,
        TipoEmissao tipoEmissao, int codigoNumerico)
    {
        _valor = valor;
        UF = uf;
        Ano = ano;
        Mes = mes;
        Modelo = modelo;
        Serie = serie;
        Numero = numero;
        TipoEmissao = tipoEmissao;
        CodigoNumerico = codigoNumerico;
    }

    /// <summary>A UF do emitente (<c>cUF</c>).</summary>
    public UF UF { get; }

    /// <summary>O ano da emissão, com quatro dígitos (a chave leva os dois últimos: <c>26</c> é 2026).</summary>
    public int Ano { get; }

    /// <summary>O mês da emissão, de 1 a 12.</summary>
    public int Mes { get; }

    /// <summary>
    /// O CNPJ ou o CPF do emitente como está na chave: 14 posições, o CNPJ sem máscara (talvez alfanumérico)
    /// ou o CPF precedido de três zeros (<c>00052998224725</c>).
    /// </summary>
    public string Emitente => _valor.Substring(6, 14);

    /// <summary>O modelo do documento (<c>mod</c>).</summary>
    public Documento Modelo { get; }

    /// <summary>A série (<c>serie</c>), de 0 a 999.</summary>
    public int Serie { get; }

    /// <summary>O número do documento (<c>nNF</c>), de 1 a 999.999.999.</summary>
    public int Numero { get; }

    /// <summary>O tipo de emissão (<c>tpEmis</c>).</summary>
    public TipoEmissao TipoEmissao { get; }

    /// <summary>O código numérico (<c>cNF</c>), de 0 a 99.999.999; no XML, com 8 dígitos.</summary>
    public int CodigoNumerico { get; }

    /// <summary>O dígito verificador (<c>cDV</c>), de 0 a 9.</summary>
    public int DigitoVerificador => _valor[Tamanho - 1] - '0';

    /// <summary>
    /// Gera a chave de um documento novo, com o código numérico (<c>cNF</c>) sorteado por
    /// <see cref="RandomNumberGenerator"/>: nunca igual ao número do documento, nem uma das sequências que a
    /// SEFAZ recusa (<c>00000000</c>, <c>11111111</c>, ..., <c>12345678</c>, ..., <c>01234567</c>; MOC 7.0,
    /// regra B09-20, rejeição 897).
    /// </summary>
    /// <param name="uf">A UF do emitente.</param>
    /// <param name="emissao">A data e hora da emissão (<c>dhEmi</c>); vale o ano e o mês no deslocamento do próprio valor.</param>
    /// <param name="emitente">O CNPJ (inclusive alfanumérico) ou o CPF do emitente, sem máscara ou com a máscara inteira.</param>
    /// <param name="modelo">O modelo do documento.</param>
    /// <param name="serie">A série, de 0 a 999.</param>
    /// <param name="numero">O número do documento, de 1 a 999.999.999.</param>
    /// <param name="tipoEmissao">O tipo de emissão.</param>
    /// <returns>A chave de acesso.</returns>
    /// <exception cref="ArgumentException">O emitente não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A UF, o modelo ou o tipo de emissão não são valores conhecidos; o ano da emissão está fora de 2000 a
    /// 2099; a série ou o número estão fora da faixa.
    /// </exception>
    public static ChaveAcesso Gerar(UF uf, DateTimeOffset emissao, string emitente, Documento modelo, int serie,
        int numero, TipoEmissao tipoEmissao) =>
        Gerar(uf, emissao, emitente, modelo, serie, numero, tipoEmissao,
            static () => RandomNumberGenerator.GetInt32(LimiteCodigoNumerico));

    /// <summary>
    /// Gera a chave sorteando o código numérico com <paramref name="sortear"/> até sair um aceito. Os testes
    /// usam para provar a recusa do número e das sequências.
    /// </summary>
    internal static ChaveAcesso Gerar(UF uf, DateTimeOffset emissao, string emitente, Documento modelo,
        int serie, int numero, TipoEmissao tipoEmissao, Func<int> sortear)
    {
        // Confere os argumentos antes de sortear, para o erro não depender do sorteio.
        _ = Montar(uf, emissao, emitente, modelo, serie, numero, tipoEmissao, codigoNumerico: 0);
        int codigo;
        do
            codigo = sortear();
        while (codigo is < 0 or >= LimiteCodigoNumerico || codigo == numero
            || Array.IndexOf(CodigosNumericosRecusados, codigo) >= 0);
        return Montar(uf, emissao, emitente, modelo, serie, numero, tipoEmissao, codigo);
    }

    /// <summary>
    /// Monta a chave com um código numérico conhecido (o <c>cNF</c> de um XML, por exemplo) e calcula o
    /// dígito verificador. Não confere as regras do código numérico (<see cref="Gerar(UF, DateTimeOffset, string, Documento, int, int, TipoEmissao)"/>
    /// gera um aceito): essas são regras do MOC, conferidas na validação do documento.
    /// </summary>
    /// <param name="uf">A UF do emitente.</param>
    /// <param name="emissao">A data e hora da emissão (<c>dhEmi</c>); vale o ano e o mês no deslocamento do próprio valor.</param>
    /// <param name="emitente">O CNPJ (inclusive alfanumérico) ou o CPF do emitente, sem máscara ou com a máscara inteira.</param>
    /// <param name="modelo">O modelo do documento.</param>
    /// <param name="serie">A série, de 0 a 999.</param>
    /// <param name="numero">O número do documento, de 1 a 999.999.999.</param>
    /// <param name="tipoEmissao">O tipo de emissão.</param>
    /// <param name="codigoNumerico">O código numérico, de 0 a 99.999.999.</param>
    /// <returns>A chave de acesso.</returns>
    /// <exception cref="ArgumentException">O emitente não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A UF, o modelo ou o tipo de emissão não são valores conhecidos; o ano da emissão está fora de 2000 a
    /// 2099; a série, o número ou o código numérico estão fora da faixa.
    /// </exception>
    public static ChaveAcesso Montar(UF uf, DateTimeOffset emissao, string emitente, Documento modelo, int serie,
        int numero, TipoEmissao tipoEmissao, int codigoNumerico)
    {
        ArgumentNullException.ThrowIfNull(emitente);
        if (!Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(uf), uf, "O valor não é uma UF.");
        if (emissao.Year is < PrimeiroAno or > UltimoAno)
            throw new ArgumentOutOfRangeException(nameof(emissao), emissao, "O ano da emissão vai de 2000 a 2099.");
        if (!Enum.IsDefined(modelo))
            throw new ArgumentOutOfRangeException(nameof(modelo), modelo, "Modelo de documento desconhecido.");
        ArgumentOutOfRangeException.ThrowIfNegative(serie);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(serie, MaiorSerie);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numero);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numero, MaiorNumero);
        if (!Enum.IsDefined(tipoEmissao))
            throw new ArgumentOutOfRangeException(nameof(tipoEmissao), tipoEmissao, "Tipo de emissão desconhecido.");
        ArgumentOutOfRangeException.ThrowIfNegative(codigoNumerico);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(codigoNumerico, LimiteCodigoNumerico);

        string documento;
        if (Cnpj.Valido(emitente))
            documento = Cnpj.RemoverMascara(emitente);
        else if (Cpf.Valido(emitente))
            documento = "000" + Cpf.RemoverMascara(emitente);
        else
            throw new ArgumentException("O emitente não é um CNPJ nem um CPF válido.", nameof(emitente));

        var i = CultureInfo.InvariantCulture;
        var corpo = string.Concat(
            ((int)uf).ToString("D2", i),
            (emissao.Year % 100).ToString("D2", i),
            emissao.Month.ToString("D2", i),
            documento,
            ((int)modelo).ToString("D2", i),
            serie.ToString("D3", i),
            numero.ToString("D9", i),
            ((int)tipoEmissao).ToString(i),
            codigoNumerico.ToString("D8", i));
        var valor = corpo + (char)('0' + Modulo11.Calcular(corpo));
        return new ChaveAcesso(valor, uf, emissao.Year, emissao.Month, modelo, serie, numero, tipoEmissao,
            codigoNumerico);
    }

    /// <summary>
    /// Lê uma chave de acesso: 44 posições, sem espaços nem prefixo (o <c>NFe</c> do atributo <c>Id</c>
    /// fica de fora).
    /// </summary>
    /// <remarks>
    /// Confere o tamanho e os caracteres (letras maiúsculas só nas 12 primeiras posições do CNPJ), a UF, o
    /// mês, o CNPJ ou o CPF do emitente, o modelo, o número (não pode ser zero), o tipo de emissão e o
    /// dígito verificador. Não confere as regras do código numérico, nem a série do emitente CPF, que são
    /// regras do MOC sobre o documento. A chave de leiaute anterior ao 2.00 (com o <c>cNF</c> de 9 dígitos
    /// e sem o tipo de emissão) é recusada quando a posição do tipo de emissão não tem um tipo conhecido.
    /// </remarks>
    /// <param name="valor">A chave, com 44 posições.</param>
    /// <param name="chave">A chave lida, quando válida.</param>
    /// <returns>Se o valor é uma chave de acesso válida.</returns>
    public static bool TentarLer(string? valor, [NotNullWhen(true)] out ChaveAcesso? chave)
    {
        chave = null;
        if (valor is not { Length: Tamanho } || !CaracteresValidos(valor))
            return false;

        var v = valor.AsSpan();
        if (!UnidadesFederativas.TentarLerCodigo(Inteiro(v[..2]), out var uf))
            return false;
        var mes = Inteiro(v[4..6]);
        if (mes is < 1 or > 12)
            return false;
        var emitente = valor.Substring(6, 14);
        if (!Cnpj.Valido(emitente) && !(emitente.StartsWith("000", StringComparison.Ordinal) && Cpf.Valido(emitente[3..])))
            return false;
        var modelo = (Documento)Inteiro(v[20..22]);
        if (!Enum.IsDefined(modelo))
            return false;
        var numero = Inteiro(v[25..34]);
        if (numero == 0)
            return false;
        var tipoEmissao = (TipoEmissao)(v[34] - '0');
        if (!Enum.IsDefined(tipoEmissao))
            return false;
        if (Modulo11.Calcular(v[..(Tamanho - 1)]) != v[Tamanho - 1] - '0')
            return false;

        chave = new ChaveAcesso(valor, uf, PrimeiroAno + Inteiro(v[2..4]), mes, modelo, Inteiro(v[22..25]), numero,
            tipoEmissao, Inteiro(v[35..43]));
        return true;
    }

    /// <summary>Lê uma chave de acesso que se sabe válida; as regras são as de <see cref="TentarLer"/>.</summary>
    /// <param name="valor">A chave, com 44 posições.</param>
    /// <returns>A chave lida.</returns>
    /// <exception cref="ArgumentException">O valor não é uma chave de acesso válida.</exception>
    public static ChaveAcesso Ler(string valor)
    {
        ArgumentNullException.ThrowIfNull(valor);
        if (!TentarLer(valor, out var chave))
            throw new ArgumentException("O valor não é uma chave de acesso válida.", nameof(valor));
        return chave;
    }

    /// <summary>As 44 posições da chave, como vão no XML.</summary>
    /// <returns>A chave.</returns>
    public override string ToString() => _valor;

    /// <inheritdoc/>
    public bool Equals(ChaveAcesso? other) => other is not null && string.Equals(_valor, other._valor, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ChaveAcesso);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_valor);

    /// <summary>Se as duas chaves têm as mesmas 44 posições.</summary>
    /// <param name="esquerda">Uma chave.</param>
    /// <param name="direita">A outra chave.</param>
    /// <returns>Se são iguais.</returns>
    public static bool operator ==(ChaveAcesso? esquerda, ChaveAcesso? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    /// <summary>Se as duas chaves diferem em alguma posição.</summary>
    /// <param name="esquerda">Uma chave.</param>
    /// <param name="direita">A outra chave.</param>
    /// <returns>Se são diferentes.</returns>
    public static bool operator !=(ChaveAcesso? esquerda, ChaveAcesso? direita) => !(esquerda == direita);

    // NT 2025.001, item 5: [0-9]{6}[A-Z0-9]{12}[0-9]{26}.
    private static bool CaracteresValidos(string valor)
    {
        for (var i = 0; i < valor.Length; i++)
        {
            var c = valor[i];
            if (!char.IsAsciiDigit(c) && !(i is >= 6 and < 18 && char.IsAsciiLetterUpper(c)))
                return false;
        }
        return true;
    }

    private static int Inteiro(ReadOnlySpan<char> digitos) => int.Parse(digitos, NumberStyles.None, CultureInfo.InvariantCulture);
}
