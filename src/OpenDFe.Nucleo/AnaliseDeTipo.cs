using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace OpenDFe;

// Qual restrição de um tipo simples um valor violou. O .NET não diz de forma estruturada; aqui o tipo é
// percorrido do primitivo do XSD até o do campo, e o primeiro passo da cadeia que recusa o valor (pelo próprio
// validador do .NET, XmlSchemaDatatype.ParseValue, que aplica as facetas daquele passo e das anteriores) é o
// que tem a restrição violada. Dentro desse passo, cada faceta é conferida à parte; o padrão, que é uma
// expressão do XSD, é conferido com o Regex do .NET e, se não der para conferir, por eliminação (o passo
// recusa e as outras facetas dele passam).
internal static class AnaliseDeTipo
{
    private static readonly TimeSpan LimiteDoPadrao = TimeSpan.FromSeconds(1);

    internal sealed record Resultado(IReadOnlyList<RestricaoSchema> Restricoes, IReadOnlyList<string> Frases)
    {
        public static Resultado Vazio { get; } = new([], []);
    }

    // O tipo simples do valor: o próprio, ou a base do complexo de conteúdo simples (simpleContent); null no
    // complexo com elementos.
    public static XmlSchemaSimpleType? TipoSimples(XmlSchemaType? tipo)
    {
        if (tipo is XmlSchemaSimpleType simples)
            return simples;
        if (tipo is XmlSchemaComplexType { ContentType: XmlSchemaContentType.TextOnly } complexo)
        {
            for (XmlSchemaType? t = complexo; t is not null; t = t.BaseXmlSchemaType)
            {
                if (t is XmlSchemaSimpleType s)
                    return s;
            }
        }
        return null;
    }

    public static Resultado Analisar(XmlSchemaSimpleType tipo, string valor, XmlNameTable nomes, IXmlNamespaceResolver espacos, string? tipoDoCampo)
    {
        // A cadeia, do primitivo do XSD (o primeiro tipo embutido) até o tipo do campo.
        var cadeia = new List<XmlSchemaSimpleType>();
        for (XmlSchemaType? t = tipo; t is XmlSchemaSimpleType s; t = s.BaseXmlSchemaType)
        {
            cadeia.Add(s);
            if (s.QualifiedName.Namespace == XmlSchema.Namespace)
                break;
        }
        cadeia.Reverse();
        var primitivo = cadeia[0];

        if (primitivo.QualifiedName.Namespace == XmlSchema.Namespace && !Aceita(primitivo, valor, nomes, espacos))
        {
            var nome = primitivo.QualifiedName.Name;
            var frase = "o valor não é " + Descricao(nome)
                + (tipoDoCampo is null || tipoDoCampo == "xs:" + nome ? "" : $" (tipo {tipoDoCampo})");
            return new([new RestricaoSchema(FacetaSchema.TipoPrimitivo, [nome], "xs:" + nome)], [frase]);
        }

        var normalizado = Normalizar(valor, cadeia);
        foreach (var passo in cadeia.Skip(primitivo.QualifiedName.Namespace == XmlSchema.Namespace ? 1 : 0))
        {
            if (Aceita(passo, valor, nomes, espacos))
                continue;
            if (passo.Content is not XmlSchemaSimpleTypeRestriction restricao)
                return Resultado.Vazio;   // lista ou união: sem detalhe
            return Facetas(restricao, ValidacaoSchema.NomeDoTipo(passo), tipoDoCampo, normalizado, primitivo, nomes, espacos);
        }
        return Resultado.Vazio;
    }

    private static Resultado Facetas(XmlSchemaSimpleTypeRestriction restricao, string? tipoDoPasso, string? tipoDoCampo,
        string valor, XmlSchemaSimpleType primitivo, XmlNameTable nomes, IXmlNamespaceResolver espacos)
    {
        var restricoes = new List<RestricaoSchema>();
        var frases = new List<string>();
        var doTipo = DoTipo(tipoDoPasso, tipoDoCampo);
        var padroes = new List<string>();
        var enumeracao = new List<string>();

        void Violou(FacetaSchema faceta, IReadOnlyList<string> valores, string frase)
        {
            restricoes.Add(new RestricaoSchema(faceta, valores, tipoDoPasso));
            frases.Add(frase);
        }

        var tamanho = valor.Length;
        var digitos = Digitos(valor);
        foreach (var faceta in restricao.Facets.OfType<XmlSchemaFacet>())
        {
            var limite = faceta.Value ?? "";
            switch (faceta)
            {
                case XmlSchemaLengthFacet when Inteiro(limite) is { } n && tamanho != n:
                    Violou(FacetaSchema.Tamanho, [limite], $"o valor tem {Caracteres(tamanho)}, e o tamanho deve ser exatamente {limite}{doTipo}");
                    break;
                case XmlSchemaMinLengthFacet when Inteiro(limite) is { } n && tamanho < n:
                    Violou(FacetaSchema.TamanhoMinimo, [limite], $"o valor tem {Caracteres(tamanho)}, abaixo do mínimo de {limite}{doTipo}");
                    break;
                case XmlSchemaMaxLengthFacet when Inteiro(limite) is { } n && tamanho > n:
                    Violou(FacetaSchema.TamanhoMaximo, [limite], $"o valor tem {Caracteres(tamanho)}, acima do máximo de {limite}{doTipo}");
                    break;
                case XmlSchemaTotalDigitsFacet when digitos is { } d && Inteiro(limite) is { } n && d.Total > n:
                    Violou(FacetaSchema.TotalDigitos, [limite], $"o valor tem {d.Total} dígitos, acima do total de {limite}{doTipo}");
                    break;
                case XmlSchemaFractionDigitsFacet when digitos is { } d && Inteiro(limite) is { } n && d.Casas > n:
                    Violou(FacetaSchema.CasasDecimais, [limite], $"o valor tem {d.Casas} casas decimais, acima do máximo de {limite}{doTipo}");
                    break;
                case XmlSchemaMinInclusiveFacet when Comparar(valor, limite, primitivo, nomes, espacos) is < 0:
                    Violou(FacetaSchema.MinimoInclusivo, [limite], $"o valor é menor que o mínimo de {limite}{doTipo}");
                    break;
                case XmlSchemaMaxInclusiveFacet when Comparar(valor, limite, primitivo, nomes, espacos) is > 0:
                    Violou(FacetaSchema.MaximoInclusivo, [limite], $"o valor é maior que o máximo de {limite}{doTipo}");
                    break;
                case XmlSchemaMinExclusiveFacet when Comparar(valor, limite, primitivo, nomes, espacos) is <= 0:
                    Violou(FacetaSchema.MinimoExclusivo, [limite], $"o valor tem de ser maior que {limite}{doTipo}");
                    break;
                case XmlSchemaMaxExclusiveFacet when Comparar(valor, limite, primitivo, nomes, espacos) is >= 0:
                    Violou(FacetaSchema.MaximoExclusivo, [limite], $"o valor tem de ser menor que {limite}{doTipo}");
                    break;
                case XmlSchemaPatternFacet:
                    padroes.Add(limite);
                    break;
                case XmlSchemaEnumerationFacet:
                    enumeracao.Add(limite);
                    break;
            }
        }

        if (enumeracao.Count > 0 && !enumeracao.Any(e => Igual(valor, e, primitivo, nomes, espacos)))
            Violou(FacetaSchema.Enumeracao, enumeracao, $"o valor não está entre os aceitos{DoTipo(tipoDoPasso, tipoDoCampo, "pelo")}: {ValidacaoSchema.Lista(enumeracao)}");

        if (padroes.Count > 0)
        {
            var atende = padroes.Select(p => Atende(valor, p)).ToList();
            // Sem nenhuma outra explicação, o padrão é o que falhou (o .NET recusou o passo), mesmo que o
            // Regex não tenha conseguido conferir (expressão do XSD que o .NET lê diferente).
            if (atende.All(a => a == false) || (restricoes.Count == 0 && !atende.Contains(true)))
                Violou(FacetaSchema.Padrao, padroes, $"o valor não segue o padrão{doTipo} ({string.Join(" ou ", padroes)})");
        }

        return new(restricoes, frases);
    }

    private static bool Aceita(XmlSchemaSimpleType tipo, string valor, XmlNameTable nomes, IXmlNamespaceResolver espacos)
    {
        if (tipo.Datatype is null)
            return true;
        try
        {
            tipo.Datatype.ParseValue(valor, nomes, espacos);
            return true;
        }
        catch (XmlSchemaException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    // O valor como as facetas o veem: o whiteSpace mais derivado da cadeia, senão o do primitivo.
    private static string Normalizar(string valor, List<XmlSchemaSimpleType> cadeia)
    {
        string? espaco = null;
        foreach (var passo in cadeia)
        {
            if (passo.Content is XmlSchemaSimpleTypeRestriction r && r.Facets.OfType<XmlSchemaWhiteSpaceFacet>().LastOrDefault() is { } f)
                espaco = f.Value;
        }
        espaco ??= cadeia[0].TypeCode switch
        {
            XmlTypeCode.String => "preserve",
            XmlTypeCode.NormalizedString => "replace",
            _ => "collapse",
        };
        return espaco switch
        {
            "preserve" => valor,
            "replace" => valor.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' '),
            _ => string.Join(' ', valor.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)),
        };
    }

    private static bool? Atende(string valor, string padrao)
    {
        try
        {
            return Regex.IsMatch(valor, "^(?:" + padrao + ")$", RegexOptions.CultureInvariant, LimiteDoPadrao);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }
    }

    private static object? Tipado(string valor, XmlSchemaSimpleType primitivo, XmlNameTable nomes, IXmlNamespaceResolver espacos)
    {
        try
        {
            return primitivo.Datatype?.ParseValue(valor, nomes, espacos);
        }
        catch (Exception e) when (e is XmlSchemaException or FormatException or OverflowException)
        {
            return null;
        }
    }

    // A comparação pelo primitivo (número com número, data com data); null quando não dá para comparar.
    private static int? Comparar(string valor, string limite, XmlSchemaSimpleType primitivo, XmlNameTable nomes, IXmlNamespaceResolver espacos)
    {
        var a = Tipado(valor, primitivo, nomes, espacos);
        var b = Tipado(limite, primitivo, nomes, espacos);
        if (a is IComparable comparavel && b is not null && a.GetType() == b.GetType())
            return Math.Sign(comparavel.CompareTo(b));
        return null;
    }

    private static bool Igual(string valor, string aceito, XmlSchemaSimpleType primitivo, XmlNameTable nomes, IXmlNamespaceResolver espacos)
    {
        if (valor == aceito)
            return true;
        if (primitivo.TypeCode is XmlTypeCode.String or XmlTypeCode.NormalizedString or XmlTypeCode.Token)
            return false;
        var a = Tipado(valor, primitivo, nomes, espacos);
        return a is not null && a.Equals(Tipado(aceito, primitivo, nomes, espacos));
    }

    // Os dígitos de um número decimal do XSD (sinal opcional, inteiros, ponto, casas): o total sem os zeros à
    // esquerda nem os zeros à direita das casas, como o totalDigits e o fractionDigits contam.
    private static (int Total, int Casas)? Digitos(string valor)
    {
        var texto = valor.TrimStart('+', '-');
        var ponto = texto.IndexOf('.', StringComparison.Ordinal);
        var inteiros = (ponto < 0 ? texto : texto[..ponto]).TrimStart('0');
        var casas = ponto < 0 ? "" : texto[(ponto + 1)..].TrimEnd('0');
        if (texto.Length == 0 || !inteiros.All(char.IsAsciiDigit) || !casas.All(char.IsAsciiDigit))
            return null;
        if (ponto >= 0 && texto[(ponto + 1)..].Any(c => !char.IsAsciiDigit(c)))
            return null;
        return (Math.Max(inteiros.Length + casas.Length, 1), casas.Length);
    }

    private static int? Inteiro(string texto) =>
        int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static string Caracteres(int n) =>
        string.Create(CultureInfo.InvariantCulture, $"{n} {(n == 1 ? "caractere" : "caracteres")}");

    private static string DoTipo(string? tipoDoPasso, string? tipoDoCampo, string preposicao = "do")
    {
        if (tipoDoPasso is null)
            return "";
        return tipoDoCampo is null || tipoDoCampo == tipoDoPasso
            ? $" {preposicao} tipo {tipoDoPasso}"
            : $" {preposicao} tipo {tipoDoPasso} (herdado por {tipoDoCampo})";
    }

    private static string Descricao(string primitivo) => primitivo switch
    {
        "decimal" => "um número decimal (com ponto, sem separador de milhar)",
        "integer" or "int" or "long" or "short" or "byte" or "nonNegativeInteger" or "positiveInteger"
            or "unsignedInt" or "unsignedLong" or "unsignedShort" or "unsignedByte" or "negativeInteger" or "nonPositiveInteger"
            => $"um número inteiro (xs:{primitivo})",
        "date" => "uma data (aaaa-mm-dd)",
        "dateTime" => "uma data e hora (aaaa-mm-ddThh:mm:ss)",
        "time" => "uma hora (hh:mm:ss)",
        "boolean" => "um booliano (true, false, 1 ou 0)",
        "base64Binary" => "um conteúdo em base64",
        "hexBinary" => "um conteúdo em hexadecimal",
        "anyURI" => "um endereço (URI)",
        _ => $"um valor do tipo xs:{primitivo}",
    };
}
