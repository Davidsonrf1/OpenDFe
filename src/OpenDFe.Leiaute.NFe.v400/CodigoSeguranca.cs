using System.Globalization;

namespace OpenDFe.NFe;

/// <summary>
/// O CSC (Código de Segurança do Contribuinte) que a SEFAZ da UF deu à empresa (Manual de Padrões Técnicos do DANFE
/// NFC-e e QR Code 6.0, 4.6). O código é segredo: o <see cref="ToString"/> mostra só o identificador.
/// </summary>
/// <remarks>
/// O manual diz "alfanuméricos", mas o exemplo dele tem hífen: a conferência fica no que quebra o QR Code (o tamanho, o
/// caractere fora do ASCII visível e a barra vertical, que separa os parâmetros), não no alfabeto. Imutável.
/// </remarks>
public sealed class CodigoSeguranca
{
    /// <summary>O menor identificador (<c>idCSC</c>).</summary>
    public const int MenorIdentificador = 1;

    /// <summary>O maior identificador: o <c>idCSC</c> tem até 6 dígitos.</summary>
    public const int MaiorIdentificador = 999_999;

    /// <summary>O menor tamanho do código.</summary>
    public const int MenorCodigo = 16;

    /// <summary>O maior tamanho do código.</summary>
    public const int MaiorCodigo = 36;

    /// <summary>Cria o CSC.</summary>
    /// <param name="identificador">O identificador (<c>idCSC</c>), de 1 a 999999; no QR Code vai sem zeros à esquerda.</param>
    /// <param name="codigo">O código, de 16 a 36 caracteres ASCII visíveis, sem a barra vertical.</param>
    /// <exception cref="ArgumentOutOfRangeException">O identificador está fora de 1 a 999999.</exception>
    /// <exception cref="ArgumentException">
    /// O código é nulo, tem menos de 16 ou mais de 36 caracteres, tem caractere fora do ASCII visível ou tem a barra vertical.
    /// </exception>
    public CodigoSeguranca(int identificador, string codigo)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(identificador, MenorIdentificador);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(identificador, MaiorIdentificador);
        ArgumentNullException.ThrowIfNull(codigo);
        if (codigo.Length is < MenorCodigo or > MaiorCodigo)
            throw new ArgumentException(string.Create(CultureInfo.InvariantCulture,
                $"O código do CSC tem de {MenorCodigo} a {MaiorCodigo} caracteres."), nameof(codigo));
        foreach (var c in codigo)
        {
            if (c is < '!' or > '~' || c == '|')
                throw new ArgumentException(
                    "O código do CSC só aceita caracteres ASCII visíveis, sem espaço e sem a barra vertical.", nameof(codigo));
        }
        Identificador = identificador;
        Codigo = codigo;
    }

    /// <summary>O identificador (<c>idCSC</c>), escrito sem zeros à esquerda.</summary>
    public int Identificador { get; }

    /// <summary>O código do CSC. É segredo: não o registre.</summary>
    public string Codigo { get; }

    /// <summary>Só o identificador (<c>"CSC 1"</c>): o código nunca aparece.</summary>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"CSC {Identificador}");
}
