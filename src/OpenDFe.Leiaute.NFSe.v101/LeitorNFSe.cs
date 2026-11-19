using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFSe;

/// <summary>
/// A leitura da NFS-e, da DPS e dos eventos, nas versões 1.00 e 1.01 (docs/NFSE.md 7, D8 e M10), pelo mapa do modelo
/// (<see cref="MapasNFSe"/>).
/// </summary>
/// <remarks>
/// <para>
/// Sem o schema: pelo nome de cada filho, sem conferir a ordem, a obrigatoriedade nem as facetas. O que reconhece entra no
/// objeto; o que não reconhece é aviso; o valor que não converte fica nulo, com erro. Nunca lança pelo conteúdo: XML mal
/// formado, outra raiz ou outra versão são problemas, com o objeto nulo.
/// </para>
/// <para>Sem DTD nem entidade externa: o XML vem de fora. Sem estado e seguro em paralelo.</para>
/// </remarks>
internal static class LeitorNFSe
{
    private static readonly XmlReaderSettings Configuracao = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    private static readonly string[] Versoes = ["1.00", "1.01"];

    /// <summary>Lê a NFS-e (<c>NFSe</c>) ou a DPS sozinha (<c>DPS</c>).</summary>
    internal static NFSeLida Ler(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        var l = new LeituraNFSe();
        if (Carregar(xml, l, ["NFSe", "DPS"], "a NFS-e (NFSe) e a DPS (DPS)") is not { } raiz)
            return new NFSeLida(null, null, null, null, null, xml, l.Problemas.ToArray());

        if (raiz.Name.LocalName == "DPS")
        {
            l.Entrar("DPS", null);
            var (dps, id) = LerDps(l, raiz);
            l.Sair();
            return new NFSeLida(null, dps, null, id, l.Versao, xml, l.Problemas.ToArray());
        }

        l.Entrar("NFSe", null);
        NotaFiscalServico? nota = null;
        ChaveNFSe? chave = null;
        IdentificadorDps? idDps = null;
        foreach (var filho in raiz.Elements())
        {
            if (filho.Name == XName.Get("infNFSe", EscritaNFSe.Namespace) && nota is null)
            {
                l.Entrar("infNFSe", null);
                chave = LerChave(l, (string?)filho.Attribute("Id"));
                nota = new NotaFiscalServico();
                LeituraMapa.Ler(l, MapasNFSe.MapaNotaFiscalServico, nota, filho);
                nota.InformacoesFisco ??= l.InformacoesFisco100;
                idDps = (string?)filho.Element(XName.Get("DPS", EscritaNFSe.Namespace))?.Element(XName.Get("infDPS", EscritaNFSe.Namespace))?.Attribute("Id")
                    is { } textoId && IdentificadorDps.TentarLer(textoId, out var lido) ? lido : null;
                l.Sair();
            }
            else if (filho.Name.LocalName != "Signature" || filho.Name.NamespaceName != LeituraNFSe.NamespaceAssinatura)
            {
                l.Registrar(CodigosNFSe.Desconhecido, Severidade.Aviso, $"O campo {filho.Name.LocalName} não é do leiaute: ficou de fora do objeto, e está no XML.",
                    filho.Name.LocalName, null);
            }
        }
        l.Sair();
        if (nota is null)
        {
            l.Registrar(CodigosNFSe.Raiz, Severidade.Erro, "Falta o infNFSe dentro do NFSe.", null, null);
            return new NFSeLida(null, null, null, null, l.Versao, xml, l.Problemas.ToArray());
        }
        return new NFSeLida(nota, nota.Dps, chave, idDps, l.Versao, xml, l.Problemas.ToArray());
    }

    /// <summary>Lê o pedido de registro de evento (<c>pedRegEvento</c>) ou o evento registrado (<c>evento</c>).</summary>
    internal static EventoLido LerEvento(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        var l = new LeituraNFSe();
        if (Carregar(xml, l, ["pedRegEvento", "evento"], "o pedido de registro de evento (pedRegEvento) e o evento (evento)") is not { } raiz)
            return new EventoLido(null, null, null, null, xml, l.Problemas.ToArray());

        if (raiz.Name.LocalName == "pedRegEvento")
        {
            l.Entrar("pedRegEvento", null);
            var pedido = LerPedido(l, raiz, out var idPedido);
            l.Sair();
            return new EventoLido(pedido, null, idPedido, l.Versao, xml, l.Problemas.ToArray());
        }

        l.Entrar("evento", null);
        EventoNFSeRegistrado? registrado = null;
        string? id = null;
        if (raiz.Element(XName.Get("infEvento", EscritaNFSe.Namespace)) is { } inf)
        {
            l.Entrar("infEvento", null);
            id = (string?)inf.Attribute("Id");
            if (id is null || id.Length != 62 || !id.StartsWith("EVT", StringComparison.Ordinal) || id.AsSpan(3).IndexOfAnyExceptInRange('0', '9') >= 0)
                l.Registrar(CodigosNFSe.Chave, Severidade.Erro, "O Id do infEvento não é EVT seguido da chave, do tipo e da sequência (59 dígitos).", "Id", null);
            registrado = new EventoNFSeRegistrado();
            LeituraMapa.Ler(l, MapasNFSe.MapaEventoNFSeRegistrado, registrado, inf);
            l.Sair();
        }
        else
        {
            l.Registrar(CodigosNFSe.Raiz, Severidade.Erro, "Falta o infEvento dentro do evento.", null, null);
        }
        l.Sair();
        return new EventoLido(registrado?.Pedido, registrado, id, l.Versao, xml, l.Problemas.ToArray());
    }

    /// <summary>Lê o <c>DPS</c> (o grupo em que a leitura já entrou): a versão, o Id do <c>infDPS</c> e os campos.</summary>
    internal static (Dps? Dps, IdentificadorDps? Id) LerDps(LeituraNFSe l, XElement dpsXml)
    {
        if (l.Versao is null)
            l.Versao = (string?)dpsXml.Attribute("versao");
        var inf = dpsXml.Element(XName.Get("infDPS", EscritaNFSe.Namespace));
        if (inf is null)
        {
            l.Registrar(CodigosNFSe.Raiz, Severidade.Erro, "Falta o infDPS dentro do DPS.", null, null);
            return (null, null);
        }
        l.Entrar("infDPS", null);
        IdentificadorDps? id = null;
        if ((string?)inf.Attribute("Id") is { } texto && IdentificadorDps.TentarLer(texto, out var lido))
            id = lido;
        else
            l.Registrar(CodigosNFSe.Chave, Severidade.Erro, "O Id do infDPS não é um identificador de DPS válido (DPS seguido de 42 dígitos).", "Id", null);
        var dps = new Dps();
        LeituraMapa.Ler(l, MapasNFSe.MapaDps, dps, inf);
        l.Sair();
        return (dps, id);
    }

    /// <summary>
    /// Lê o <c>pedRegEvento</c> (o grupo em que a leitura já entrou): o <c>infPedReg</c>, com o detalhe na subclasse do
    /// tipo. O tipo desconhecido é erro, sem o evento.
    /// </summary>
    internal static EventoNFSe? LerPedido(LeituraNFSe l, XElement pedido) => LerPedido(l, pedido, out _);

    private static EventoNFSe? LerPedido(LeituraNFSe l, XElement pedido, out string? id)
    {
        id = null;
        if (l.Versao is null)
            l.Versao = (string?)pedido.Attribute("versao");
        var inf = pedido.Element(XName.Get("infPedReg", EscritaNFSe.Namespace));
        if (inf is null)
        {
            l.Registrar(CodigosNFSe.Raiz, Severidade.Erro, "Falta o infPedReg dentro do pedRegEvento.", null, null);
            return null;
        }
        l.Entrar("infPedReg", null);
        id = (string?)inf.Attribute("Id");
        TipoEventoNFSe? tipo = null;
        XElement? detalhe = null;
        foreach (var filho in inf.Elements())
            if (filho.Name.NamespaceName == EscritaNFSe.Namespace && MapasNFSe.Eventos.FirstOrDefault(t => t.Tag == filho.Name.LocalName) is { } achado)
                (tipo, detalhe) = tipo is null ? (achado, filho) : (tipo, detalhe);
        if (tipo is null || detalhe is null)
        {
            l.Registrar(CodigosNFSe.Evento, Severidade.Erro, "O infPedReg não tem o detalhe de um tipo de evento do leiaute (e101101 e os outros).", null, null);
            l.Sair();
            return null;
        }
        if (id is null || id.Length != 59 || !id.StartsWith("PRE", StringComparison.Ordinal) || id.AsSpan(3).IndexOfAnyExceptInRange('0', '9') >= 0)
            l.Registrar(CodigosNFSe.Chave, Severidade.Erro, "O Id do infPedReg não é PRE seguido da chave e do tipo (56 dígitos).", "Id", null);
        else if (!id.EndsWith(tipo.Tipo, StringComparison.Ordinal))
            l.Registrar(CodigosNFSe.Evento, Severidade.Erro, string.Create(CultureInfo.InvariantCulture,
                $"O tipo no fim do Id do infPedReg não é o do detalhe ({tipo.Tipo})."), "Id", null);

        var evento = tipo.Criar();
        LeituraMapa.Ler(l, MapasNFSe.MapaEventoNFSe, evento, inf, filho => filho == detalhe);
        l.Entrar(tipo.Tag, null);
        var esperado = l.Versao == "1.00" ? tipo.Descricao100 : tipo.Descricao101;
        var descricao = (string?)detalhe.Element(XName.Get("xDesc", EscritaNFSe.Namespace));
        if (descricao != esperado)
            l.Registrar(CodigosNFSe.Evento, Severidade.Aviso,
                $"O xDesc não é o texto do evento {tipo.Tipo} na versão {l.Versao}.", "xDesc", null);
        LeituraMapa.Ler(l, tipo.Mapa(), evento, detalhe, filho => filho.Name.LocalName is "xDesc" or "infRej" or "infAnRej");
        // na 1.00, a rejeição e a anulação da rejeição têm os campos dentro de um grupo (infRej, infAnRej), que a 1.01 tirou
        foreach (var grupo in detalhe.Elements().Where(f => f.Name.NamespaceName == EscritaNFSe.Namespace && f.Name.LocalName is "infRej" or "infAnRej"))
        {
            l.Entrar(grupo.Name.LocalName, null);
            LeituraMapa.Ler(l, tipo.Mapa(), evento, grupo);
            l.Sair();
        }
        l.Sair();
        l.Sair();
        return evento;
    }

    // O XML, a raiz e a versão: o que não serve é problema, com a leitura parada (nulo).
    private static XElement? Carregar(string xml, LeituraNFSe l, string[] raizes, string descricao)
    {
        XDocument documento;
        try
        {
            using var texto = new StringReader(xml);
            if (texto.Peek() == '﻿')
                texto.Read();
            using var leitor = XmlReader.Create(texto, Configuracao);
            documento = XDocument.Load(leitor, LoadOptions.None);
        }
        catch (XmlException erro)
        {
            l.Registrar(CodigosNFSe.Xml, Severidade.Erro, string.Create(CultureInfo.InvariantCulture,
                $"O texto não é XML bem formado (linha {erro.LineNumber}, coluna {erro.LinePosition})."), null, null);
            return null;
        }
        var raiz = documento.Root!;
        if (raiz.Name.NamespaceName != EscritaNFSe.Namespace || !raizes.Contains(raiz.Name.LocalName))
        {
            l.Registrar(CodigosNFSe.Raiz, Severidade.Erro,
                $"A raiz {raiz.Name.LocalName} não é do leiaute: ele lê {descricao}, no namespace {EscritaNFSe.Namespace}.", null, null);
            return null;
        }
        var versao = (string?)raiz.Attribute("versao");
        if (versao is null || !Versoes.Contains(versao))
        {
            l.Registrar(CodigosNFSe.Versao, Severidade.Erro, versao is null
                ? $"Falta a versão do {raiz.Name.LocalName}."
                : $"A versão do {raiz.Name.LocalName} não é a 1.00 nem a 1.01: é de outro leiaute.", null, null);
            return null;
        }
        l.Versao = versao;
        return raiz;
    }

    // A chave do Id do infNFSe (NFS + 50 dígitos), pela ChaveNFSe.
    private static ChaveNFSe? LerChave(LeituraNFSe l, string? id)
    {
        if (id is { Length: 53 } && id.StartsWith("NFS", StringComparison.Ordinal) && ChaveNFSe.TentarLer(id[3..], out var chave))
            return chave;
        l.Registrar(CodigosNFSe.Chave, Severidade.Erro, id is null
            ? "Falta o Id do infNFSe, com a chave de acesso."
            : "O Id do infNFSe não é NFS seguido de uma chave de acesso válida (50 dígitos).", "Id", null);
        return null;
    }
}
