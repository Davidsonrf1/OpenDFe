namespace OpenDFe;

/// <summary>
/// Dígito verificador por módulo 11, com os pesos de 2 a 9 da direita para a esquerda, recomeçando em 2
/// (CNPJ, inclusive o alfanumérico, e chave de acesso). Cada caractere vale o código ASCII menos 48:
/// os dígitos valem o próprio número, e as letras maiúsculas, de 17 (A) a 42 (Z).
/// </summary>
internal static class Modulo11
{
    /// <summary>O dígito de <paramref name="valores"/>, que só pode ter dígitos e letras maiúsculas.</summary>
    public static int Calcular(ReadOnlySpan<char> valores)
    {
        var soma = 0;
        var peso = 2;
        for (var i = valores.Length - 1; i >= 0; i--)
        {
            soma += (valores[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
