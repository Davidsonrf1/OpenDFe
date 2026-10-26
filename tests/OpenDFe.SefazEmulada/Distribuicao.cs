using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeDistribuicaoDFe do AN (NT 2014.002 v1.40; MOC 7.0, Visão Geral, 5.7): o distDFeInt e o retDistDFeInt, com
// os documentos de Interessados, cada um compactado em gzip no docZip. O pedido vem na operação
// nfeDistDFeInteresse e tem o limite de 10 KB (3.9, B01). As regras, na ordem: a raiz (516, 404, 517, 239), o
// schema (215), o ambiente (H01, 252), o CNPJ (H02, 489) ou o CPF (H03, 490) do interessado, a raiz do CNPJ do
// certificado do TLS contra a do CNPJ consultado (H04, 593) ou o CPF dele contra o CPF consultado (H05, 472), a
// paralisação (108, 109) e o bloqueio do uso indevido (656). Depois, por consulta: o distNSU, o consNSU e o
// consChNFe (Distribuir, ConsultarNsu, ConsultarChave).
internal sealed class Distribuicao : Servico
{
    private const string Versao = "1.01";
    private const int Lote = 50;
    private const int PontuaisHora = 20;

    // O que o AN guarda para distribuir (3: 90 dias depois de recebido) e o bloqueio do uso indevido (3.11.4).
    private static readonly TimeSpan Validade = TimeSpan.FromDays(90);
    private static readonly TimeSpan Bloqueio = TimeSpan.FromHours(1);

    // Os textos que a NT 2014.002 (4) escreve diferente do Anexo I, só neste serviço.
    private const string TextoLocalizados = "Documento(s) localizado(s)";
    private const string TextoInexistente = "Rejeição: NF-e inexistente para a chave de acesso informada";
    private const string TextoModelo = "Rejeição: Chave de Acesso inválida (modelo diferente de 55)";
    private const string AguardarHora = ". Deve ser aguardado 1 hora para efetuar nova solicitação caso não existam mais documentos a serem pesquisados. Tente após 1 hora";
    private const string UsarUltimo = ". Deve ser utilizado o ultNSU nas solicitações subsequentes. Tente após 1 hora";
    private const string LimitePontuais = " - ultrapassou o limite de 20 consultas por hora";

    public override string Nome => NomeDistribuicao;

    public override string? Operacao => "nfeDistDFeInteresse";

    public override int Limite => 10 * 1024;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "distDFeInt", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Distribuicao.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);
        if (dados.Element(NamespaceNFe + "tpAmb")!.Value != "2")
            return Rejeitar(Motivos.AmbienteDivergente, pedido);
        var cnpj = dados.Element(NamespaceNFe + "CNPJ")?.Value;
        var interessado = cnpj ?? dados.Element(NamespaceNFe + "CPF")!.Value;
        if (cnpj is not null ? !Documentos.CnpjValido(cnpj) : !Documentos.CpfValido(interessado))
            return Rejeitar(cnpj is not null ? Motivos.CnpjInvalido : Motivos.CpfInvalido, pedido);
        var transmissor = pedido.Transmissor;
        if (cnpj is not null && (transmissor.Length != 14 || transmissor[..8] != cnpj[..8]))
            return Rejeitar(Motivos.CnpjBaseCertificadoDivergente, pedido);
        if (cnpj is null && transmissor != interessado)
            return Rejeitar(Motivos.CpfCertificadoDivergente, pedido);
        if (pedido.Paralisacao is { } paralisacao)
            return Rejeitar(paralisacao.Codigo, pedido);

        var consulta = dados.Elements().Last();
        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
        {
            var interessados = repositorio.Interessados;
            var consumo = interessados.Consumo(interessado);
            var maximo = interessados.Maximo(interessado);
            var agora = pedido.Recebimento;
            if (consumo.Bloqueio > agora)
            {
                // Quem consulta no bloqueio recomeça a hora (3.11.4.2, a observação); o distNSU leva o último NSU.
                consumo.Bloqueio = agora + Bloqueio;
                interessados.SalvarConsumo(interessado);
                var ultimo = consulta.Name.LocalName == "distNSU" ? consumo.UltimoNsu ?? 0 : 0;
                return Retorno(Motivos.ConsumoIndevido, Motivos.Texto(Motivos.ConsumoIndevido) + consumo.Motivo, pedido, ultimo, maximo);
            }
            var resposta = consulta.Name.LocalName switch
            {
                "distNSU" => Distribuir(Numero(consulta), interessado, consumo, maximo, pedido, interessados),
                "consNSU" => ConsultarNsu(Numero(consulta), interessado, consumo, maximo, pedido, interessados),
                _ => ConsultarChave(consulta.Element(NamespaceNFe + "chNFe")!.Value, interessado, consumo, maximo, pedido, repositorio),
            };
            interessados.SalvarConsumo(interessado);
            return resposta;
        }
    }

    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, Motivos.Texto(codigo), pedido, 0, 0);

    // O distNSU (3.5): o NSU acima do maior (H06, 589), o uso indevido (3.11.4.1: a nova consulta em menos de uma hora
    // depois do 137, ou com outro ultNSU que não o último devolvido, 656, com o bloqueio de uma hora) e os
    // documentos depois do ultNSU, até 50, recebidos há menos de 90 dias: 138 com o lote, ou 137.
    private static XElement Distribuir(long ultimo, string interessado, Consumo consumo, long maximo, Pedido pedido, Interessados interessados)
    {
        var agora = pedido.Recebimento;
        if (ultimo > maximo)
            return Retorno(Motivos.NsuSuperior, Motivos.Texto(Motivos.NsuSuperior), pedido, maximo, maximo);
        if (consumo.Nenhum is { } nenhum && agora - nenhum < Bloqueio)
            return Bloquear(consumo, AguardarHora, pedido, maximo);
        if (consumo.UltimoNsu is { } devolvido && ultimo != devolvido)
            return Bloquear(consumo, UsarUltimo, pedido, maximo);

        var documentos = interessados.Documentos(interessado).Where(d => d.Nsu > ultimo && agora - d.Recebimento <= Validade).Take(Lote).ToList();
        if (documentos.Count == 0)
        {
            consumo.Nenhum = agora;
            consumo.UltimoNsu = maximo;
            return Retorno(Motivos.NenhumDocumento, Motivos.Texto(Motivos.NenhumDocumento), pedido, maximo, maximo);
        }
        consumo.Nenhum = null;
        consumo.UltimoNsu = documentos[^1].Nsu;
        return Retorno(Motivos.DocumentoLocalizado, TextoLocalizados, pedido, documentos[^1].Nsu, maximo,
            [.. documentos.Select(d => Compactar(d.Xml, d.Schema, d.Nsu))]);
    }

    // O consNSU (3.6): o limite das consultas pontuais (656), o NSU acima do maior (589) e o documento daquele NSU,
    // se ainda guardado (138; 137 sem ele).
    private static XElement ConsultarNsu(long nsu, string interessado, Consumo consumo, long maximo, Pedido pedido, Interessados interessados)
    {
        if (Pontual(consumo, pedido) is { } excedido)
            return excedido;
        if (nsu > maximo)
            return Retorno(Motivos.NsuSuperior, Motivos.Texto(Motivos.NsuSuperior), pedido, maximo, maximo);
        var documento = interessados.Documentos(interessado).FirstOrDefault(d => d.Nsu == nsu && pedido.Recebimento - d.Recebimento <= Validade);
        if (documento is null)
            return Retorno(Motivos.NenhumDocumento, Motivos.Texto(Motivos.NenhumDocumento), pedido, nsu, maximo);
        return Retorno(Motivos.DocumentoLocalizado, Motivos.Texto(Motivos.DocumentoLocalizado), pedido, nsu, maximo,
            [Compactar(documento.Xml, documento.Schema, documento.Nsu)]);
    }

    // O consChNFe (3.7), na ordem: o limite das consultas pontuais (656), a chave (H07 a H13: 236, 614, 615, 616,
    // 617, 618 e 619, aqui só o modelo 55), a nota na base (H14, 217), o prazo de 90 dias (H15, 632), o emitente
    // (H17, 641, antes do H16, que senão o cobriria), o interessado na nota (H16, 640: o destinatário, o
    // transportador ou um autorizado, pela raiz do CNPJ ou pelo CPF) e a nota cancelada (H18, 653). Achada, só a
    // NF-e, sem os eventos: o nfeProc ou, ao destinatário que ainda não se manifestou, o resNFe; sem o NSU no docZip
    // (facultativo desde a v1.15) e com o ultNSU zerado.
    private static XElement ConsultarChave(string chave, string interessado, Consumo consumo, long maximo, Pedido pedido, Repositorio repositorio)
    {
        if (Pontual(consumo, pedido) is { } excedido)
            return excedido;
        var invalida = Documentos.ConferirChave(chave, pedido.Recebimento);
        if (invalida == Motivos.ChaveDigitoInvalido)
            return Retorno(invalida, Motivos.Texto(invalida), pedido, 0, maximo);
        if (!UnidadesFederativas.Existe(chave[..2]))
            return Retorno(Motivos.ChaveUFInvalida, Motivos.Texto(Motivos.ChaveUFInvalida), pedido, 0, maximo);
        if (invalida == Motivos.ChaveModeloInvalido || (invalida is null && chave[20..22] != "55"))
            return Retorno(Motivos.ChaveModeloInvalido, TextoModelo, pedido, 0, maximo);
        if (invalida is not null)
            return Retorno(invalida, Motivos.Texto(invalida), pedido, 0, maximo);

        if (repositorio.Nota(chave) is not { } nota)
            return Retorno(Motivos.NotaInexistente, TextoInexistente, pedido, 0, maximo);
        if (pedido.Recebimento - nota.Recebimento > Validade)
            return Retorno(Motivos.DownloadForaPrazo, Motivos.Texto(Motivos.DownloadForaPrazo), pedido, 0, maximo);
        var papeis = Interessados.Atores(nota)!;
        var destinatario = Papeis.Mesmo(interessado, papeis.Destinatario);
        var terceiro = Papeis.Mesmo(interessado, papeis.Transportador) || papeis.Autorizados.Any(a => Papeis.Mesmo(interessado, a));
        if (!destinatario && !terceiro)
        {
            var codigo = Papeis.Mesmo(interessado, papeis.Emitente) ? Motivos.IndisponivelEmitente : Motivos.InteressadoNaoPermitido;
            return Retorno(codigo, Motivos.Texto(codigo), pedido, 0, maximo);
        }
        if (nota.Cancelada)
            return Retorno(Motivos.DownloadNotaCancelada, Motivos.Texto(Motivos.DownloadNotaCancelada), pedido, 0, maximo);
        var (xml, schema) = destinatario && !terceiro && !repositorio.Interessados.Liberada(chave)
            ? (Interessados.Resumo(nota), "resNFe_v1.01.xsd")
            : (Interessados.Processada(nota), "procNFe_v4.00.xsd");
        return Retorno(Motivos.DocumentoLocalizado, Motivos.Texto(Motivos.DocumentoLocalizado), pedido, 0, maximo, [Compactar(xml, schema, null)]);
    }

    // As consultas pontuais (consNSU e consChNFe) da última hora (3.11.4.2): a 21ª é uso indevido (656).
    private static XElement? Pontual(Consumo consumo, Pedido pedido)
    {
        var agora = pedido.Recebimento;
        consumo.Pontuais.RemoveAll(p => agora - p >= Bloqueio);
        if (consumo.Pontuais.Count >= PontuaisHora)
            return Bloquear(consumo, LimitePontuais, pedido, 0);
        consumo.Pontuais.Add(agora);
        return null;
    }

    // O uso indevido: o bloqueio de uma hora, com o motivo, que se repete enquanto durar.
    private static XElement Bloquear(Consumo consumo, string motivo, Pedido pedido, long maximo)
    {
        consumo.Bloqueio = pedido.Recebimento + Bloqueio;
        consumo.Motivo = motivo;
        return Retorno(Motivos.ConsumoIndevido, Motivos.Texto(Motivos.ConsumoIndevido) + motivo, pedido, consumo.UltimoNsu ?? 0, maximo);
    }

    // O docZip (B11): o documento em UTF-8, compactado em gzip e em base64, com o NSU (quando há) e o schema.
    private static XElement Compactar(XElement xml, string schema, long? nsu)
    {
        using var compactado = new MemoryStream();
        using (var gzip = new GZipStream(compactado, CompressionLevel.Optimal, leaveOpen: true))
        using (var escritor = XmlWriter.Create(gzip, new XmlWriterSettings { Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false }))
            xml.Save(escritor);
        return Elemento("docZip", new object?[]
        {
            nsu is null ? null : new XAttribute("NSU", Nsu(nsu.Value)),
            new XAttribute("schema", schema),
            Convert.ToBase64String(compactado.ToArray()),
        });
    }

    // O retDistDFeInt (Tabela 5-27; o ultNSU e o maxNSU obrigatórios no schema 1.01), com o motivo pronto.
    private static XElement Retorno(string codigo, string motivo, Pedido pedido, long ultimo, long maximo, IReadOnlyList<XElement>? documentos = null) =>
        new(NamespaceNFe + "retDistDFeInt",
            new XAttribute("versao", Versao),
            Elemento("tpAmb", "2"),
            Elemento("verAplic", VersaoAplicativo(pedido)),
            Elemento("cStat", codigo),
            Elemento("xMotivo", motivo),
            Elemento("dhResp", DataHora(pedido.Recebimento)),
            Elemento("ultNSU", Nsu(ultimo)),
            Elemento("maxNSU", Nsu(maximo)),
            documentos is null ? null : Elemento("loteDistDFeInt", documentos));

    private static long Numero(XElement consulta) => long.Parse(consulta.Elements().Single().Value, CultureInfo.InvariantCulture);

    private static string Nsu(long numero) => numero.ToString("D15", CultureInfo.InvariantCulture);
}
