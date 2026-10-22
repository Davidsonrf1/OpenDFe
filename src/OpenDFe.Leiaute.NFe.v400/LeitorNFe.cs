using System.Globalization;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// A leitura da NF-e e da NFC-e no leiaute 4.00 (PROJETO.md 5.2, docs/LEITURA.md): o <c>NFe</c> ou o <c>nfeProc</c> vira
/// o <see cref="NotaFiscal"/>, com <see cref="XmlReader"/> à mão, um método por grupo, espelho do <see cref="EscritorNFe"/>.
/// </summary>
/// <remarks>
/// <para>
/// Sem o schema: pelo nome de cada filho, sem conferir a ordem, a obrigatoriedade nem as facetas. O que reconhece entra
/// no objeto; o que não reconhece é aviso; o valor que não converte fica nulo, com erro. Os campos de NT anterior que
/// saíram do schema (docs/LEITURA.md 5) são avisos à parte. Nunca lança pelo conteúdo.
/// </para>
/// <para>Sem DTD nem entidade externa: o XML vem de fora. Sem estado e seguro em paralelo.</para>
/// </remarks>
internal static partial class LeitorNFe
{
    private const string Versao = "4.00";

    private static readonly XmlReaderSettings Configuracao = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    /// <summary>Lê o <c>NFe</c> ou o <c>nfeProc</c>.</summary>
    internal static NotaLida Ler(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        using var texto = new StringReader(xml);
        if (texto.Peek() == '﻿')
            texto.Read();   // o BOM que sobrou da decodificação
        using var leitor = XmlReader.Create(texto, Configuracao);
        var estado = new Estado(new LeituraNFe(leitor));
        try
        {
            LerRaiz(estado, leitor);
            while (leitor.Read())
            {
                // o que vier depois da raiz só é conferido como XML bem formado
            }
        }
        catch (XmlException erro)
        {
            estado.Leitura.Problemas.Add(new Problema(CodigosLeitura.Xml, Severidade.Erro, string.Create(CultureInfo.InvariantCulture,
                $"O texto não é XML bem formado (linha {erro.LineNumber}, coluna {erro.LinePosition}).")));
            estado.Valida = false;
        }
        catch (LeituraInterrompidaException)
        {
            estado.Valida = false;
        }
        if (estado.Valida && estado.Nota is not null)
            Conferir(estado);
        return new NotaLida(estado.Valida ? estado.Nota : null, estado.Chave, estado.Protocolo, xml, estado.Leitura.Problemas.ToArray());
    }

    private static void LerRaiz(Estado estado, XmlReader leitor)
    {
        var l = estado.Leitura;
        leitor.MoveToContent();
        if (leitor.NodeType != XmlNodeType.Element || leitor.NamespaceURI != EscritaNFe.Namespace
            || leitor.LocalName is not ("NFe" or "nfeProc"))
            Interromper(l, CodigosLeitura.Raiz, $"A raiz {leitor.Name} não é uma NF-e: o leiaute 4.00 lê o NFe e o nfeProc, no namespace {EscritaNFe.Namespace}.");

        if (leitor.LocalName == "NFe")
        {
            LerNFe(estado);
        }
        else
        {
            estado.Processada = true;
            l.Entrar(null);
            ConferirVersao(l, l.TextoAtributo("versao", null), "nfeProc");
            while (l.Filho())
            {
                switch (l.Nome)
                {
                    case "NFe":
                        if (l.Unico())
                            LerNFe(estado);
                        break;
                    case "protNFe":
                        if (l.Unico())
                            estado.Protocolo = LerProtocolo(l);
                        break;
                    default:
                        l.Desconhecido();
                        break;
                }
            }
            l.Sair();
        }
        if (estado.Nota is null)
            Interromper(l, CodigosLeitura.Raiz, "Falta a nota: o infNFe dentro do NFe.");
    }

    private static void LerNFe(Estado estado)
    {
        var l = estado.Leitura;
        l.Entrar(null);
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "infNFe":
                    if (!l.Unico())
                        break;
                    l.Entrar(null);
                    ConferirVersao(l, l.TextoAtributo("versao", null), "infNFe");
                    LerChave(estado, l.TextoAtributo("Id", null));
                    estado.Nota ??= new NotaFiscal();
                    Ler(l, estado.Nota);
                    l.Sair();
                    break;
                case "infNFeSupl":
                    estado.Nota ??= new NotaFiscal();
                    estado.Nota.Suplementares = l.Grupo(estado.Nota.Suplementares, "Suplementares", Ler);
                    break;
                default:
                    if (l.Assinatura)
                        l.Pular();
                    else
                        l.Desconhecido();
                    break;
            }
        }
        l.Sair();
    }

    private static void ConferirVersao(LeituraNFe l, string? versao, string grupo)
    {
        if (versao != Versao)
            Interromper(l, CodigosLeitura.Versao, versao is null
                ? $"Falta a versão do {grupo}."
                : $"A versão do {grupo} não é a {Versao}: é de outro leiaute.");
    }

    // A chave do Id (NFe + 44 posições), pela ChaveAcesso (docs/LEITURA.md, decisão 8).
    private static void LerChave(Estado estado, string? id)
    {
        var l = estado.Leitura;
        estado.ChaveTexto = id is { Length: 47 } && id.StartsWith("NFe", StringComparison.Ordinal) ? id[3..] : null;
        if (estado.ChaveTexto is null)
        {
            l.Registrar(CodigosLeitura.Chave, Severidade.Erro,
                id is null ? "Falta o Id do infNFe, com a chave de acesso." : "O Id do infNFe não é NFe seguido das 44 posições da chave.",
                "Id", null);
            return;
        }
        if (ChaveAcesso.TentarLer(estado.ChaveTexto, out var chave))
            estado.Chave = chave;
        else
            l.Registrar(CodigosLeitura.Chave, Severidade.Erro, "A chave de acesso do Id do infNFe não é válida.", "Id", null);
    }

    // O item, com o nItem conferido contra a posição (MODELO.md, decisão 21).
    private static void LerItem(LeituraNFe l, IList<Item> itens)
    {
        var item = new Item();
        l.Entrar("Itens", itens.Count);
        var numero = l.InteiroAtributo("nItem", null);
        if (numero != itens.Count + 1)
            l.Registrar(CodigosLeitura.Item, Severidade.Aviso, string.Create(CultureInfo.InvariantCulture,
                $"O nItem do {itens.Count + 1}º item não é {itens.Count + 1}: os itens ficam na ordem em que vieram."), "nItem", null);
        Ler(l, item);
        l.Sair();
        itens.Add(item);
    }

    private static Protocolo LerProtocolo(LeituraNFe l)
    {
        var protocolo = new Protocolo();
        l.Entrar("Protocolo");
        l.TextoAtributo("versao", null);
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "infProt":
                    if (!l.Unico())
                        break;
                    l.Entrar(null);
                    protocolo.Id = l.TextoAtributo("Id", "Id");
                    Ler(l, protocolo);
                    l.Sair();
                    break;
                default:
                    if (l.Assinatura)
                        l.Pular();
                    else
                        l.Desconhecido();
                    break;
            }
        }
        l.Sair();
        return protocolo;
    }

    private static void Ler(LeituraNFe l, Protocolo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpAmb":
                    g.Ambiente = l.Enumeracao(g.Ambiente, "Ambiente");
                    break;
                case "verAplic":
                    g.VersaoAplicativo = l.Texto(g.VersaoAplicativo, "VersaoAplicativo");
                    break;
                case "chNFe":
                    g.Chave = l.Texto(g.Chave, "Chave");
                    break;
                case "dhRecbto":
                    g.Recebimento = l.DataHora(g.Recebimento, "Recebimento");
                    break;
                case "nProt":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "digVal":
                    g.Digest = l.Texto(g.Digest, "Digest");
                    break;
                case "cStat":
                    g.Status = l.Inteiro(g.Status, "Status");
                    break;
                case "xMotivo":
                    g.Motivo = l.Texto(g.Motivo, "Motivo");
                    break;
                case "cMsg":
                    // a sequência cMsg + xMsg se repete: cada cMsg abre uma mensagem
                    g.Mensagens.Add(new MensagemSefaz { Codigo = l.Inteiro(Mensagem(g.Mensagens.Count, "Codigo")) });
                    break;
                case "xMsg":
                    if (g.Mensagens.Count == 0 || g.Mensagens[^1].Texto is not null)
                        g.Mensagens.Add(new MensagemSefaz());
                    g.Mensagens[^1].Texto = l.Texto(Mensagem(g.Mensagens.Count - 1, "Texto"));
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static string Mensagem(int indice, string propriedade) =>
        string.Create(CultureInfo.InvariantCulture, $"Mensagens[{indice}].{propriedade}");

    // Depois da nota: o cDV contra a chave (MODELO.md, decisão 6) e a chave do protocolo contra a da nota.
    private static void Conferir(Estado estado)
    {
        var problemas = estado.Leitura.Problemas;
        if (estado.Chave is { } chave && estado.Leitura.DigitoVerificador is { } digito && digito != chave.DigitoVerificador)
            problemas.Add(new Problema(CodigosLeitura.Chave, Severidade.Erro, "O cDV difere do dígito verificador da chave de acesso do Id.")
            {
                Tag = "cDV",
                Caminho = estado.Caminho("ide/cDV"),
            });
        if (estado.Protocolo?.Chave is { } chaveProtocolo && estado.ChaveTexto is { } chaveNota && chaveProtocolo != chaveNota)
            problemas.Add(new Problema(CodigosLeitura.Protocolo, Severidade.Erro,
                "A chave do protocolo difere da chave da nota: o protocolo não é desta nota.")
            {
                Tag = "chNFe",
                Caminho = "nfeProc/protNFe/infProt/chNFe",
                Propriedade = "Protocolo.Chave",
            });
    }

    private static void Interromper(LeituraNFe l, string codigo, string mensagem)
    {
        l.Registrar(codigo, Severidade.Erro, mensagem, null, null);
        throw new LeituraInterrompidaException();
    }

    // O que a leitura junta fora do modelo: a raiz, a chave e o protocolo.
    private sealed class Estado(LeituraNFe leitura)
    {
        public LeituraNFe Leitura { get; } = leitura;

        public NotaFiscal? Nota { get; set; }

        public ChaveAcesso? Chave { get; set; }

        public string? ChaveTexto { get; set; }

        public Protocolo? Protocolo { get; set; }

        public bool Valida { get; set; } = true;

        public bool Processada { get; set; }

        public string Caminho(string dentro) => (Processada ? "nfeProc/" : "") + "NFe/infNFe/" + dentro;
    }

    // A leitura que não tem como seguir (a raiz, a versão): o problema já está registrado.
    private sealed class LeituraInterrompidaException : Exception;
}
