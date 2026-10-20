namespace OpenDFe.SefazEmulada;

// O CNPJ (também o alfanumérico), o CPF e a chave de acesso, com o dígito verificador conferido. Código
// próprio, sem a lib (13.5): se o emulador reaproveitasse o cálculo dela, um erro dele apareceria igual dos
// dois lados e passaria.
internal static class Documentos
{
    // O CNPJ de 14 posições: as 12 primeiras com letra maiúscula ou algarismo, os 2 dígitos no fim (NT
    // 2025.001). O valor de cada posição é o código ASCII menos 48, o que dá o próprio algarismo nos números.
    public static bool CnpjValido(string cnpj)
    {
        if (cnpj.Length != 14 || !cnpj[..12].All(c => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c))
            || !char.IsAsciiDigit(cnpj[12]) || !char.IsAsciiDigit(cnpj[13]) || cnpj.All(c => c == cnpj[0]))
            return false;
        return Modulo11(cnpj[..12]) == cnpj[12] - '0' && Modulo11(cnpj[..13]) == cnpj[13] - '0';
    }

    public static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(char.IsAsciiDigit) || cpf.All(c => c == cpf[0]))
            return false;
        return DigitoCpf(cpf[..9]) == cpf[9] - '0' && DigitoCpf(cpf[..10]) == cpf[10] - '0';
    }

    // Se a série é de emitente CNPJ: de 0 a 909 e, desde a NT 2026.001 (PAA), de 980 a 989; as outras são do
    // emitente CPF (NT 2018.001), que vai na chave com três zeros à esquerda.
    public static bool SerieCnpj(int serie) => serie <= 909 || serie is >= 980 and <= 989;

    // O CNPJ ou o CPF da chave, pela série (J02e da consulta; P12-26 do evento, 617), com o DV conferido.
    public static bool DocumentoChaveValido(string chave)
    {
        var documento = chave[6..20];
        return SerieCnpj(int.Parse(chave[22..25], System.Globalization.CultureInfo.InvariantCulture))
            ? CnpjValido(documento)
            : documento.StartsWith("000", StringComparison.Ordinal) && CpfValido(documento[3..]);
    }

    // As regras da chave que a consulta (J02a a J02g) e o evento (P12-10 a P12-34) aplicam iguais: o DV (236), o
    // ano menor que 06 ou maior que o do recebimento (615), o mês (616), o CNPJ ou o CPF pela série (617), o
    // modelo (618) e o número zero (619). Nulo: a chave passa.
    public static string? ConferirChave(string chave, DateTimeOffset recebimento)
    {
        if (Modulo11(chave[..43]) != chave[43] - '0')
            return Motivos.ChaveDigitoInvalido;
        var ano = int.Parse(chave[2..4], System.Globalization.CultureInfo.InvariantCulture);
        if (ano < 6 || ano > recebimento.ToOffset(TimeSpan.FromHours(-3)).Year % 100)
            return Motivos.ChaveAnoInvalido;
        if (chave[4..6] is not ("01" or "02" or "03" or "04" or "05" or "06" or "07" or "08" or "09" or "10" or "11" or "12"))
            return Motivos.ChaveMesInvalido;
        if (!DocumentoChaveValido(chave))
            return Motivos.ChaveDocumentoInvalido;
        if (chave[20..22] is not ("55" or "65"))
            return Motivos.ChaveModeloInvalido;
        return chave[25..34] == "000000000" ? Motivos.ChaveNumeroInvalido : null;
    }

    // A chave de acesso (MOC 7.0, Visão Geral, 2.2.6): cUF, AAMM da emissão, CNPJ ou CPF do emitente (o CPF
    // com zeros à esquerda), modelo, série, número, tipo de emissão, código numérico e o DV.
    public static string Chave(string uf, DateTimeOffset emissao, string emitente, string modelo, string serie, string numero, string tipoEmissao, string codigo, string digito) =>
        uf + emissao.ToString("yyMM", System.Globalization.CultureInfo.InvariantCulture) + emitente.PadLeft(14, '0') + modelo
        + serie.PadLeft(3, '0') + numero.PadLeft(9, '0') + tipoEmissao + codigo + digito;

    // O DV da chave (Visão Geral, 2.2.6.2, sobre as 43 primeiras posições) e do CNPJ: módulo 11, pesos de 2
    // a 9 da direita para a esquerda; resto 0 ou 1 dá 0. As letras valem o código ASCII menos 48.
    public static int Modulo11(string parte)
    {
        var soma = 0;
        var peso = 2;
        for (var i = parte.Length - 1; i >= 0; i--)
        {
            soma += (parte[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int DigitoCpf(string parte)
    {
        var soma = 0;
        for (var i = 0; i < parte.Length; i++)
            soma += (parte[i] - '0') * (parte.Length + 1 - i);
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
