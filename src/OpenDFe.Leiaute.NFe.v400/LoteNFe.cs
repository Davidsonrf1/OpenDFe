using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// O que a autorização e a consulta do recibo têm em comum (docs/CONTRATOS.md 14): a nota do modelo escrita para o lote,
/// a nota assinada lida até a chave, as regras do lote, as notas enviadas tiradas da mensagem, a leitura dos
/// <c>protNFe</c> do retorno ligados às notas e o <c>nfeProc</c>. Sem estado.
/// </summary>
internal static class LoteNFe
{
    private const string Versao = "4.00";

    // As rejeições da assinatura que a lib confere na nota assinada recebida (F02 e afins, decisão 10); o titular
    // (213, 227, 292) fica para a SEFAZ.
    private static readonly HashSet<string> CodigosAssinatura = ["290", "297", "298"];

    private static readonly XmlReaderSettings ConfiguracaoLeitura = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    /// <summary>
    /// As regras do catálogo (docs/REGRAS.md) que a montagem do lote confere (decisão 10), pelo identificador e pelo
    /// código: o teste do catálogo as cruza com as linhas "lib (4.4)".
    /// </summary>
    internal static readonly IReadOnlyList<(string Identificador, string Codigo)> Regras =
        [("B02-20", "476"), ("B06-20", "765"), ("C02-20", "560"), ("C02a-30", "560"), ("F02", "297")];

    /// <summary>Uma nota do lote, já montada: o XML (sem assinatura, se é do modelo), a chave e o prefixo da propriedade.</summary>
    internal sealed record Nota(string? Xml, ChaveAcesso? Chave, string Prefixo, object? Modelo);

    /// <summary>
    /// As notas do lote, na ordem: as do modelo escritas com os valores do núcleo, ou as assinadas lidas até a chave,
    /// com os problemas (as regras do lote inclusive) e o prefixo da propriedade (<c>Notas[k].</c>, <c>Xmls[k].</c>).
    /// </summary>
    internal static List<Nota> Montar(Autorizacao parametro, ContextoOperacao contexto, List<Problema> problemas)
    {
        var notas = new List<Nota>();
        if (parametro.Notas.Count > 0)
        {
            var nucleo = ValoresNucleo.Criar(contexto);
            for (var k = 0; k < parametro.Notas.Count; k++)
            {
                var prefixo = Prefixo("Notas", k);
                var escrita = EscritorNFe.Escrever(parametro.Notas[k], nucleo);
                problemas.AddRange(escrita.Problemas.Select(p => Prefixar(p, prefixo)));
                notas.Add(new Nota(escrita.Xml, escrita.Chave, prefixo, parametro.Notas[k]));
            }
        }
        else
        {
            for (var k = 0; k < parametro.Xmls.Count; k++)
            {
                var prefixo = Prefixo("Xmls", k);
                var xml = Normalizar(parametro.Xmls[k]);
                notas.Add(new Nota(xml, LerAssinada(xml, prefixo, problemas), prefixo, null));
            }
        }
        ConferirLote(notas, contexto.Documento, problemas);
        return notas;
    }

    /// <summary>
    /// A nota assinada como vai no lote: sem o BOM, sem a declaração XML e sem os espaços em volta. O elemento assinado
    /// não muda.
    /// </summary>
    internal static string Normalizar(string xml)
    {
        var texto = xml.TrimStart('﻿').Trim();
        if (texto.StartsWith("<?xml", StringComparison.Ordinal))
        {
            var fim = texto.IndexOf("?>", StringComparison.Ordinal);
            if (fim > 0)
                texto = texto[(fim + 2)..].TrimStart();
        }
        return texto;
    }

    /// <summary>
    /// A chave do <c>Id</c> do <c>infNFe</c> da nota assinada, com a assinatura conferida (297; o titular fica para a
    /// SEFAZ). O XML que não é um <c>NFe</c> com a chave é erro, e a nota fica sem chave.
    /// </summary>
    internal static ChaveAcesso? LerAssinada(string xml, string prefixo, List<Problema> problemas)
    {
        XElement raiz;
        try
        {
            using var texto = new StringReader(xml);
            using var leitor = XmlReader.Create(texto, ConfiguracaoLeitura);
            raiz = XElement.Load(leitor);
        }
        catch (XmlException erro)
        {
            problemas.Add(new Problema(CodigosLeitura.Xml, Severidade.Erro, string.Create(CultureInfo.InvariantCulture,
                $"A nota {prefixo.TrimEnd('.')} não é XML bem formado (linha {erro.LineNumber}, coluna {erro.LinePosition}).")));
            return null;
        }
        if (raiz.Name != XName.Get("NFe", EscritaNFe.Namespace))
        {
            problemas.Add(new Problema(CodigosLeitura.Raiz, Severidade.Erro,
                $"A nota {prefixo.TrimEnd('.')} não é um NFe assinado: a raiz é {raiz.Name.LocalName}.") { Caminho = raiz.Name.LocalName });
            return null;
        }

        var id = (string?)raiz.Element(XName.Get("infNFe", EscritaNFe.Namespace))?.Attribute("Id");
        if (id is null || !id.StartsWith("NFe", StringComparison.Ordinal) || !ChaveAcesso.TentarLer(id[3..], out var chave))
        {
            problemas.Add(Problema(CodigosLeitura.Chave, Severidade.Erro, "O Id do infNFe não tem uma chave de acesso válida.",
                "NFe/infNFe/@Id", prefixo));
            return null;
        }

        var emitente = chave.Emitente;
        var documento = Cnpj.Valido(emitente) ? emitente : emitente[3..];
        if (!Cnpj.Valido(documento) && !Cpf.Valido(documento))
        {
            problemas.Add(Problema(CodigosLeitura.Chave, Severidade.Erro, "A chave de acesso não tem um CNPJ nem um CPF válido do emitente.",
                "NFe/infNFe/@Id", prefixo));
            return chave;
        }
        foreach (var problema in AssinaturaDigital.Verificar(xml, id, documento))
        {
            if (CodigosAssinatura.Contains(problema.Codigo))
                problemas.Add(new Problema(problema.Codigo, problema.Severidade, problema.Mensagem)
                {
                    Tag = problema.Tag,
                    Caminho = "NFe/" + (problema.Tag is "Signature" or null ? "Signature" : "Signature/" + problema.Tag),
                });
        }
        return chave;
    }

    /// <summary>
    /// As regras do lote (decisão 10): o modelo do núcleo (765), a UF da primeira nota (476), o CNPJ base ou o CPF da
    /// primeira nota (560, facultativa: aviso), a mesma chave duas vezes e o tipo de emissão da primeira nota
    /// (<c>NFE-LOTE</c>). Só as notas com chave entram.
    /// </summary>
    internal static void ConferirLote(IReadOnlyList<Nota> notas, Documento documento, List<Problema> problemas)
    {
        ChaveAcesso? primeira = null;
        var vistas = new HashSet<ChaveAcesso>();
        foreach (var nota in notas)
        {
            if (nota.Chave is not { } chave)
                continue;
            if (chave.Modelo != documento)
                problemas.Add(Problema("765", Severidade.Erro,
                    $"Rejeição: Lote só poderá conter NF-e ou NFC-e (a nota é {chave.Modelo} e o núcleo, {documento}).", "NFe/infNFe/ide/mod", nota.Prefixo));
            if (!vistas.Add(chave))
                problemas.Add(Problema(CodigosEscrita.Lote, Severidade.Erro, "A mesma nota (a mesma chave de acesso) está duas vezes no lote.",
                    "NFe/infNFe/@Id", nota.Prefixo));
            if (primeira is null)
            {
                primeira = chave;
                continue;
            }
            if (chave.UF != primeira.UF)
                problemas.Add(Problema("476", Severidade.Erro, "Rejeição: Código da UF diverge da UF da primeira NF-e do Lote.",
                    "NFe/infNFe/ide/cUF", nota.Prefixo));
            if (chave.TipoEmissao != primeira.TipoEmissao)
                problemas.Add(Problema(CodigosEscrita.Lote, Severidade.Erro,
                    "O tipo de emissão difere do da primeira nota do lote: as duas iriam a autorizadores diferentes.", "NFe/infNFe/ide/tpEmis", nota.Prefixo));
            if (Base(chave.Emitente) != Base(primeira.Emitente))
                problemas.Add(Problema("560", Severidade.Aviso,
                    "Rejeição: CNPJ base/CPF do emitente difere do CNPJ base/CPF da primeira NF-e do lote recebido.", "NFe/infNFe/emit", nota.Prefixo));
        }
    }

    /// <summary>
    /// As notas como foram enviadas, tiradas da mensagem assinada (<c>enviNFe</c>), na ordem: os bytes de cada
    /// <c>NFe</c>, com a assinatura.
    /// </summary>
    internal static List<string> Enviadas(string xmlPedido)
    {
        var notas = new List<string>();
        var inicio = xmlPedido.IndexOf("</indSinc>", StringComparison.Ordinal);
        while (inicio >= 0)
        {
            var abre = xmlPedido.IndexOf("<NFe", inicio, StringComparison.Ordinal);
            if (abre < 0)
                break;
            var fecha = xmlPedido.IndexOf("</NFe>", abre, StringComparison.Ordinal);
            if (fecha < 0)
                break;
            fecha += "</NFe>".Length;
            notas.Add(xmlPedido[abre..fecha]);
            inicio = fecha;
        }
        return notas;
    }

    /// <summary>A chave do <c>Id</c> do <c>infNFe</c> de uma nota enviada; nula quando não há.</summary>
    internal static ChaveAcesso? Chave(string nota)
    {
        try
        {
            using var texto = new StringReader(nota);
            using var leitor = XmlReader.Create(texto, ConfiguracaoLeitura);
            var id = (string?)XElement.Load(leitor).Element(XName.Get("infNFe", EscritaNFe.Namespace))?.Attribute("Id");
            return id is not null && id.StartsWith("NFe", StringComparison.Ordinal) && ChaveAcesso.TentarLer(id[3..], out var chave)
                ? chave
                : null;
        }
        catch (XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// O resultado de cada nota: as <paramref name="enviadas"/> (o XML e a chave), na ordem, com o <c>protNFe</c> da
    /// chave dela; sem notas enviadas, uma por <c>protNFe</c>. O <c>protNFe</c> de uma chave fora do lote é aviso e fica
    /// fora; a nota sem protocolo, com <paramref name="avisarFalta"/>, também é aviso. Os problemas do protocolo saem
    /// como aviso, com o caminho a partir da <paramref name="raiz"/> e a propriedade a partir do retorno.
    /// </summary>
    internal static List<ResultadoNota> Resultados(XElement raiz, IReadOnlyList<(string Xml, ChaveAcesso? Chave)> enviadas,
        bool avisarFalta, List<Problema> problemas)
    {
        var nomeRaiz = raiz.Name.LocalName;
        var elementos = raiz.Elements(XName.Get("protNFe", EscritaNFe.Namespace)).ToList();
        var lidos = new List<(Protocolo Protocolo, XElement Elemento, IReadOnlyList<Problema> Problemas, string Passo)>();
        for (var i = 0; i < elementos.Count; i++)
        {
            var (protocolo, problemasProtocolo) = LeitorNFe.LerProtocolo(elementos[i]);
            var passo = elementos.Count > 1 ? string.Create(CultureInfo.InvariantCulture, $"protNFe[{i + 1}]") : "protNFe";
            lidos.Add((protocolo, elementos[i], problemasProtocolo, passo));
        }

        var resultados = new List<ResultadoNota>();
        if (enviadas.Count == 0)
        {
            foreach (var lido in lidos)
            {
                AcrescentarProblemas(problemas, lido.Problemas, nomeRaiz, lido.Passo, resultados.Count);
                var chave = ChaveAcesso.TentarLer(lido.Protocolo.Chave, out var lida) ? lida : null;
                resultados.Add(new ResultadoNota(chave, null, lido.Protocolo, null));
            }
            return resultados;
        }

        var usados = new bool[lidos.Count];
        foreach (var (xml, chave) in enviadas)
        {
            var indice = chave is null ? -1 : lidos.FindIndex(l => l.Protocolo.Chave == chave.ToString());
            if (indice < 0)
            {
                if (avisarFalta)
                    problemas.Add(new Problema(CodigosLeitura.Protocolo, Severidade.Aviso,
                        $"A nota {chave} do lote não veio com o protocolo na resposta.")
                    {
                        Caminho = nomeRaiz,
                        Propriedade = Passo("Notas", resultados.Count),
                    });
                resultados.Add(new ResultadoNota(chave, xml, null, null));
                continue;
            }
            usados[indice] = true;
            var lido = lidos[indice];
            AcrescentarProblemas(problemas, lido.Problemas, nomeRaiz, lido.Passo, resultados.Count);
            var processada = ResultadoNota.Processada(lido.Protocolo.Status) ? NotaProcessada(xml, lido.Elemento) : null;
            resultados.Add(new ResultadoNota(chave, xml, lido.Protocolo, processada));
        }
        for (var i = 0; i < lidos.Count; i++)
        {
            if (!usados[i])
                problemas.Add(new Problema(CodigosLeitura.Protocolo, Severidade.Aviso,
                    $"O protocolo da chave {lidos[i].Protocolo.Chave} não é de nenhuma nota do lote: fica fora.")
                {
                    Tag = "chNFe",
                    Caminho = $"{nomeRaiz}/{lidos[i].Passo}/infProt/chNFe",
                });
        }
        return resultados;
    }

    /// <summary>O <c>nfeProc</c>: a nota com os bytes como foi enviada e o <c>protNFe</c> da resposta.</summary>
    internal static string NotaProcessada(string nota, XElement protNFe) =>
        $"<nfeProc xmlns=\"{EscritaNFe.Namespace}\" versao=\"{Versao}\">{nota}{protNFe.ToString(SaveOptions.DisableFormatting)}</nfeProc>";

    private static void AcrescentarProblemas(List<Problema> problemas, IReadOnlyList<Problema> lidos, string raiz, string passo, int nota)
    {
        foreach (var problema in lidos)
        {
            var caminho = problema.Caminho is { } c && c.StartsWith("protNFe", StringComparison.Ordinal)
                ? raiz + "/" + passo + c["protNFe".Length..]
                : problema.Caminho;
            problemas.Add(new Problema(problema.Codigo, Severidade.Aviso, problema.Mensagem)
            {
                Tag = problema.Tag,
                Caminho = caminho,
                Propriedade = problema.Propriedade is { } p ? Passo("Notas", nota) + "." + p : null,
            });
        }
    }

    private static string Base(string emitente) => Cnpj.Valido(emitente) ? emitente[..8] : emitente;

    private static string Prefixo(string lista, int indice) => Passo(lista, indice) + ".";

    private static string Passo(string lista, int indice) =>
        string.Create(CultureInfo.InvariantCulture, $"{lista}[{indice}]");

    private static Problema Prefixar(Problema problema, string prefixo) =>
        problema.Propriedade is null
            ? problema
            : new Problema(problema.Codigo, problema.Severidade, problema.Mensagem)
            {
                Tag = problema.Tag,
                Caminho = problema.Caminho,
                Propriedade = prefixo + problema.Propriedade,
                Schema = problema.Schema,
            };

    private static Problema Problema(string codigo, Severidade severidade, string mensagem, string caminho, string prefixo)
    {
        var propriedade = CaminhoModeloNFe.Propriedade(caminho);
        return new Problema(codigo, severidade, mensagem)
        {
            Tag = caminho[(caminho.LastIndexOf('/') + 1)..].TrimStart('@'),
            Caminho = caminho,
            Propriedade = propriedade is null ? null : prefixo + propriedade,
        };
    }
}
