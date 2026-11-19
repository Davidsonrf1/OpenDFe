using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.NFSe;

/// <summary>
/// Os campos do mapa de uma classe <typeparamref name="T"/> do modelo, um método por tipo de valor (docs/NFSE.md 7,
/// M5). Os valores vão ao XML como o leiaute da NF-e os escreve: os decimais com as casas, sem arredondar; as datas com o
/// deslocamento do valor; as enumerações pelo número, com os zeros do campo.
/// </summary>
internal static class Campo<T> where T : class
{
    private static readonly string[] FormatosDataHora =
    [
        "yyyy-MM-dd'T'HH:mm:sszzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", "yyyy-MM-dd'T'HH:mm:ss'Z'", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
    ];

    /// <summary>O texto como está; nulo e vazio omitem a tag.</summary>
    internal static CampoMapa Texto(string tag, string propriedade, Func<T, string?> obter, Action<T, string?> definir,
        bool versao100 = false, string[]? outras = null) =>
        new CampoTexto<T>(tag, propriedade, obter, definir, versao100, outras);

    /// <summary>O decimal com <paramref name="casas"/> casas, sem arredondar.</summary>
    internal static CampoMapa Decimal(string tag, string propriedade, Func<T, decimal?> obter, Action<T, decimal?> definir,
        int casas, bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, decimal>(tag, propriedade, obter, definir, v => FormatadorInvariavel.Numero(v, casas), LerDecimal,
            versao100, outras);

    /// <summary>O inteiro, sem zeros à esquerda.</summary>
    internal static CampoMapa Inteiro(string tag, string propriedade, Func<T, int?> obter, Action<T, int?> definir,
        bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, int>(tag, propriedade, obter, definir, v => v.ToString(CultureInfo.InvariantCulture), LerInteiro,
            versao100, outras);

    /// <summary>O inteiro longo, sem zeros à esquerda.</summary>
    internal static CampoMapa Longo(string tag, string propriedade, Func<T, long?> obter, Action<T, long?> definir,
        bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, long>(tag, propriedade, obter, definir, v => v.ToString(CultureInfo.InvariantCulture), LerLongo,
            versao100, outras);

    /// <summary>A data, <c>AAAA-MM-DD</c>.</summary>
    internal static CampoMapa Data(string tag, string propriedade, Func<T, DateOnly?> obter, Action<T, DateOnly?> definir,
        bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, DateOnly>(tag, propriedade, obter, definir, FormatadorInvariavel.Data, LerData, versao100, outras);

    /// <summary>A data e a hora, com o deslocamento do próprio valor.</summary>
    internal static CampoMapa DataHora(string tag, string propriedade, Func<T, DateTimeOffset?> obter,
        Action<T, DateTimeOffset?> definir, bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, DateTimeOffset>(tag, propriedade, obter, definir, FormatadorInvariavel.DataHora, LerDataHora,
            versao100, outras);

    /// <summary>A sigla da UF.</summary>
    internal static CampoMapa Sigla(string tag, string propriedade, Func<T, UF?> obter, Action<T, UF?> definir,
        bool versao100 = false, string[]? outras = null) =>
        new CampoValor<T, UF>(tag, propriedade, obter, definir,
            u => Enum.IsDefined(u) ? u.ToString() : ((int)u).ToString(CultureInfo.InvariantCulture), LerSigla, versao100, outras);

    /// <summary>
    /// A enumeração pelo número, com zeros à esquerda até <paramref name="digitos"/> (0 = sem zeros). Na leitura, o número
    /// fora da lista entra como está, com aviso (MODELO.md, decisão 8); com <paramref name="textoLivre100"/>, o texto que
    /// não é número numa NFS-e 1.00 fica nulo com aviso, porque a 1.00 tinha ali texto livre (M10).
    /// </summary>
    internal static CampoMapa Enumeracao<TEnum>(string tag, string propriedade, Func<T, TEnum?> obter, Action<T, TEnum?> definir,
        int digitos, bool versao100 = false, string[]? outras = null, bool textoLivre100 = false) where TEnum : struct, Enum =>
        new CampoValor<T, TEnum>(tag, propriedade, obter, definir,
            v => Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(digitos > 0 ? "D" + digitos.ToString(CultureInfo.InvariantCulture) : "D", CultureInfo.InvariantCulture),
            LerEnumeracao, versao100, outras, textoLivre100);

    /// <summary>Um grupo; o nulo omite o grupo.</summary>
    internal static CampoMapa Grupo<TGrupo>(string tag, string propriedade, Func<T, TGrupo?> obter, Action<T, TGrupo> definir,
        Func<MapaGrupo> mapa, bool versao100 = false) where TGrupo : class, new() =>
        new CampoGrupo<T, TGrupo>(tag, propriedade, obter, definir, mapa, versao100);

    /// <summary>Uma lista de grupos: o elemento se repete.</summary>
    internal static CampoMapa Lista<TGrupo>(string tag, string propriedade, Func<T, IList<TGrupo>> obter, Func<MapaGrupo> mapa,
        bool versao100 = false) where TGrupo : class, new() =>
        new CampoLista<T, TGrupo>(tag, propriedade, obter, mapa, versao100);

    /// <summary>Uma lista de textos: o elemento de texto se repete.</summary>
    internal static CampoMapa Textos(string tag, string propriedade, Func<T, IList<string>> obter, bool versao100 = false) =>
        new CampoTextos<T>(tag, propriedade, obter, versao100);

    /// <summary>
    /// Um campo da versão 1.00 que mudou de grupo na 1.01 (o <c>xOutInf</c> dos valores da NFS-e): a leitura entrega o texto
    /// a <paramref name="ler"/>, e a escrita não o tem.
    /// </summary>
    internal static CampoMapa Desvio(string tag, Action<LeituraNFSe, string> ler) => new CampoDesvio(tag, ler);

    /// <summary>
    /// A DPS dentro da NFS-e (<c>DPS</c>, com o <c>infDPS</c> no meio, que a <see cref="NotaFiscalServico.Dps"/> achata), lida pelo leitor da
    /// DPS. A lib não escreve a NFS-e.
    /// </summary>
    internal static CampoMapa Dps(string tag, string propriedade, Action<T, Dps> definir) =>
        new CampoDps<T>(tag, propriedade, definir);

    /// <summary>O pedido de registro dentro do evento registrado (<c>pedRegEvento</c>), lido pelo leitor dos eventos.</summary>
    internal static CampoMapa Pedido(string tag, string propriedade, Action<T, EventoNFSe?> definir) =>
        new CampoPedido<T>(tag, propriedade, definir);

    private static ResultadoConversao LerDecimal(string texto, out decimal valor) =>
        decimal.TryParse(texto, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out valor)
            ? ResultadoConversao.Valido : ResultadoConversao.Invalido;

    private static ResultadoConversao LerInteiro(string texto, out int valor) =>
        int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out valor) ? ResultadoConversao.Valido : ResultadoConversao.Invalido;

    private static ResultadoConversao LerLongo(string texto, out long valor) =>
        long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out valor) ? ResultadoConversao.Valido : ResultadoConversao.Invalido;

    private static ResultadoConversao LerData(string texto, out DateOnly valor) =>
        DateOnly.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out valor)
            ? ResultadoConversao.Valido : ResultadoConversao.Invalido;

    private static ResultadoConversao LerDataHora(string texto, out DateTimeOffset valor) =>
        DateTimeOffset.TryParseExact(texto, FormatosDataHora, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out valor)
            ? ResultadoConversao.Valido : ResultadoConversao.Invalido;

    private static ResultadoConversao LerSigla(string texto, out UF valor)
    {
        valor = default;
        if (texto.Length != 2 || !char.IsAsciiLetterUpper(texto[0]) || !char.IsAsciiLetterUpper(texto[1]))
            return ResultadoConversao.Invalido;
        return Enum.TryParse(texto, ignoreCase: false, out valor) && Enum.IsDefined(valor)
            ? ResultadoConversao.Valido : ResultadoConversao.Invalido;
    }

    private static ResultadoConversao LerEnumeracao<TEnum>(string texto, out TEnum valor) where TEnum : struct, Enum
    {
        valor = default;
        if (!long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) || numero > int.MaxValue)
            return ResultadoConversao.Invalido;
        valor = (TEnum)Enum.ToObject(typeof(TEnum), numero);
        return Enum.IsDefined(valor) ? ResultadoConversao.Valido : ResultadoConversao.Desconhecido;
    }
}

/// <summary>O resultado de converter o texto de um campo.</summary>
internal enum ResultadoConversao
{
    /// <summary>O valor serve.</summary>
    Valido,

    /// <summary>O número de enumeração fora da lista: entra como está, com aviso.</summary>
    Desconhecido,

    /// <summary>O texto não converte: o campo fica nulo, com erro.</summary>
    Invalido,
}

/// <summary>Converte o texto de um campo no valor.</summary>
internal delegate ResultadoConversao Conversao<TValor>(string texto, out TValor valor);

internal sealed class CampoTexto<T>(string tag, string propriedade, Func<T, string?> obter, Action<T, string?> definir,
    bool versao100, IReadOnlyList<string>? outras) : CampoMapa(tag, propriedade, versao100, outras) where T : class
{
    internal override bool Preenchido(object dono) => !string.IsNullOrEmpty(obter((T)dono));

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        var valor = obter((T)dono);
        if (!string.IsNullOrEmpty(valor))
            e.Texto(Tag, Propriedade, valor);
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        if (l.TextoSimples(elemento, Propriedade) is { } texto)
            definir((T)dono, texto);
    }
}

internal sealed class CampoValor<T, TValor>(string tag, string propriedade, Func<T, TValor?> obter, Action<T, TValor?> definir,
    Func<TValor, string> formatar, Conversao<TValor> converter, bool versao100, IReadOnlyList<string>? outras,
    bool textoLivre100 = false) : CampoMapa(tag, propriedade, versao100, outras) where T : class where TValor : struct
{
    internal override bool Preenchido(object dono) => obter((T)dono) is not null;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        if (obter((T)dono) is { } valor)
            e.Valor(Tag, formatar(valor));
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        if (l.TextoSimples(elemento, Propriedade) is not { } texto)
            return;
        switch (converter(texto, out var valor))
        {
            case ResultadoConversao.Valido:
                definir((T)dono, valor);
                break;
            case ResultadoConversao.Desconhecido:
                definir((T)dono, valor);
                l.Registrar(CodigosNFSe.Enumeracao, Severidade.Aviso,
                    $"O campo {elemento.Name.LocalName} tem um código fora da lista do leiaute: entrou como está.", elemento.Name.LocalName, Propriedade);
                break;
            default:
                if (textoLivre100 && l.Versao == "1.00")
                    l.Registrar(CodigosNFSe.Valor, Severidade.Aviso,
                        $"O campo {elemento.Name.LocalName} da versão 1.00 é texto livre, sem o código da 1.01: ficou nulo, e o texto está no XML.",
                        elemento.Name.LocalName, Propriedade);
                else
                    l.Registrar(CodigosNFSe.Valor, Severidade.Erro,
                        $"O campo {elemento.Name.LocalName} não tem um valor válido.", elemento.Name.LocalName, Propriedade);
                break;
        }
    }
}

internal sealed class CampoGrupo<T, TGrupo>(string tag, string propriedade, Func<T, TGrupo?> obter, Action<T, TGrupo> definir,
    Func<MapaGrupo> mapa, bool versao100) : CampoMapa(tag, propriedade, versao100, null) where T : class where TGrupo : class, new()
{
    internal override MapaGrupo Subgrupo => mapa();

    internal override bool Preenchido(object dono) => obter((T)dono) is not null;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        if (obter((T)dono) is not { } grupo)
            return;
        e.Abrir(Tag, Propriedade);
        EscritaMapa.Escrever(e, mapa(), grupo);
        e.Fechar();
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        var grupo = new TGrupo();
        l.Entrar(Tag, Propriedade);
        LeituraMapa.Ler(l, mapa(), grupo, elemento);
        l.Sair();
        definir((T)dono, grupo);
    }
}

internal sealed class CampoLista<T, TGrupo>(string tag, string propriedade, Func<T, IList<TGrupo>> obter, Func<MapaGrupo> mapa,
    bool versao100) : CampoMapa(tag, propriedade, versao100, null) where T : class where TGrupo : class, new()
{
    internal override MapaGrupo Subgrupo => mapa();

    internal override bool Repete => true;

    internal override bool Preenchido(object dono) => obter((T)dono).Count > 0;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        var lista = obter((T)dono);
        for (var i = 0; i < lista.Count; i++)
        {
            e.Abrir(Tag, Propriedade, i, lista.Count);
            if (lista[i] is { } grupo)
                EscritaMapa.Escrever(e, mapa(), grupo);
            e.Fechar();
        }
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        var lista = obter((T)dono);
        var grupo = new TGrupo();
        l.Entrar(Tag, Propriedade, lista.Count);
        LeituraMapa.Ler(l, mapa(), grupo, elemento);
        l.Sair();
        lista.Add(grupo);
    }
}

internal sealed class CampoTextos<T>(string tag, string propriedade, Func<T, IList<string>> obter, bool versao100)
    : CampoMapa(tag, propriedade, versao100, null) where T : class
{
    internal override bool Repete => true;

    internal override bool Preenchido(object dono) => obter((T)dono).Count > 0;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        var lista = obter((T)dono);
        for (var i = 0; i < lista.Count; i++)
            if (!string.IsNullOrEmpty(lista[i]))
                e.Texto(Tag, Propriedade, lista[i], i, lista.Count);
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        var lista = obter((T)dono);
        if (l.TextoSimples(elemento, Propriedade, lista.Count) is { } texto)
            lista.Add(texto);
    }
}

internal sealed class CampoDesvio(string tag, Action<LeituraNFSe, string> ler) : CampoMapa(tag, "", versao100: true, null)
{
    internal override bool Preenchido(object dono) => false;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        // o campo é só da leitura da 1.00: na 1.01 ele está em outro grupo, com a sua propriedade
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        if (l.TextoSimples(elemento, null) is { } texto)
            ler(l, texto);
    }
}

internal sealed class CampoDps<T>(string tag, string propriedade, Action<T, Dps> definir)
    : CampoMapa(tag, propriedade, versao100: false, null) where T : class
{
    internal override MapaGrupo Subgrupo => MapasNFSe.MapaDps;

    internal override bool Preenchido(object dono) => false;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        // a lib não escreve a NFS-e: quem a gera é o ambiente nacional ou o município
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        l.Entrar(Tag, Propriedade);
        if (LeitorNFSe.LerDps(l, elemento).Dps is { } dps)
            definir((T)dono, dps);
        l.Sair();
    }
}

internal sealed class CampoPedido<T>(string tag, string propriedade, Action<T, EventoNFSe?> definir)
    : CampoMapa(tag, propriedade, versao100: false, null) where T : class
{
    internal override bool Preenchido(object dono) => false;

    internal override void Escrever(EscritaNFSe e, object dono)
    {
        // a lib não escreve o evento registrado: quem o gera é a Sefin, o município ou o ADN
    }

    internal override void Ler(LeituraNFSe l, object dono, XElement elemento)
    {
        l.Entrar(Tag, Propriedade);
        definir((T)dono, LeitorNFSe.LerPedido(l, elemento));
        l.Sair();
    }
}
