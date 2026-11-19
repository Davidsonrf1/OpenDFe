using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// Um erro da Sefin, como vai no JSON da resposta (docs/NFSE.md 5): o código, a descrição e o complemento.
internal sealed record ErroSefin(string Codigo, string Descricao, string? Complemento = null)
{
    public static ErroSefin Criar(string codigo, string? complemento = null) => new(codigo, MotivosNFSe.Texto(codigo), complemento);
}

// A DPS aberta: o elemento (lido com o espaço em branco preservado, para a assinatura continuar valendo dentro da NFS-e)
// e o texto, como veio.
internal sealed record DpsAberta(XElement Dps, string Texto);

// O que a emissão devolve: o status HTTP, o JSON e a situação para os pedidos recentes; a NFS-e quando gerou.
internal sealed record ResultadoSefin(int Status, object Corpo, string Situacao);

// O POST /nfse da Sefin Nacional (E.7): a abertura do JSON (o Base64, o GZip, o UTF-8, o prefixo, o tipo e o schema, da aba
// RN_RECEPCAO_DPS do Anexo I), as regras da DPS que o emulador confere (docs/SEFAZ_EMULADA.md, "NFS-e") e a NFS-e gerada,
// com os valores calculados pela parametrização do município e assinada pela AC de teste.
internal sealed partial class EmissaoNFSe(Municipios municipios, RepositorioNFSe repositorio, AssinanteNFSe assinante)
{
    public static readonly XNamespace Namespace = "http://www.sped.fazenda.gov.br/nfse";
    private static readonly XNamespace NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    // O campo do JSON do pedido (docs/NTS.md 12.4: de implementação aberta, a conferir no Swagger com o PFX).
    public const string CampoDps = "dpsXmlGZipB64";

    // O verAplic da NFS-e e o versaoAplicativo do JSON (TSVerAplic: até 20).
    public const string VersaoAplicativo = "OpenDFe Emulada 1.0";

    // A mesma tolerância da data de emissão da NF-e no emulador (703).
    private static readonly TimeSpan Tolerancia = TimeSpan.FromMinutes(5);

    private static readonly UTF8Encoding Utf8Estrito = new(false, true);

    // Abre o corpo do pedido até a DPS válida no schema; o erro é o da recepção.
    public static (DpsAberta? Dps, ErroSefin? Erro) Abrir(byte[] corpo)
    {
        ArgumentNullException.ThrowIfNull(corpo);
        string? base64;
        try
        {
            using var json = JsonDocument.Parse(corpo);
            base64 = json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty(CampoDps, out var campo)
                && campo.ValueKind == JsonValueKind.String ? campo.GetString() : null;
        }
        catch (JsonException)
        {
            base64 = null;
        }
        if (base64 is null)
            return (null, ErroSefin.Criar(MotivosNFSe.FalhaBase64, $"O corpo não é um JSON com o campo {CampoDps}."));

        var compactado = new byte[base64.Length];
        if (!Convert.TryFromBase64String(base64, compactado, out var tamanho))
            return (null, ErroSefin.Criar(MotivosNFSe.FalhaBase64));

        byte[] bytes;
        try
        {
            using var entrada = new MemoryStream(compactado, 0, tamanho);
            using var gzip = new GZipStream(entrada, CompressionMode.Decompress);
            using var saida = new MemoryStream();
            gzip.CopyTo(saida);
            bytes = saida.ToArray();
        }
        catch (InvalidDataException)
        {
            return (null, ErroSefin.Criar(MotivosNFSe.EstruturaMalFormada, "O conteúdo não é GZip."));
        }

        string texto;
        try
        {
            texto = Utf8Estrito.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return (null, ErroSefin.Criar(MotivosNFSe.NaoUtf8));
        }
        if (texto.Length > 0 && texto[0] == '\uFEFF')
            texto = texto[1..];
        if (Declaracao().Match(texto) is { Success: true } declaracao && declaracao.Groups["codificacao"].Success
            && !string.Equals(declaracao.Groups["codificacao"].Value, "UTF-8", StringComparison.OrdinalIgnoreCase))
            return (null, ErroSefin.Criar(MotivosNFSe.NaoUtf8));

        XDocument documento;
        try
        {
            if (UsaPrefixo(texto))
                return (null, ErroSefin.Criar(MotivosNFSe.PrefixoNamespace));
            using var leitor = XmlReader.Create(new StringReader(texto), Configuracao());
            documento = XDocument.Load(leitor, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException erro)
        {
            return (null, ErroSefin.Criar(MotivosNFSe.EstruturaMalFormada, erro.Message));
        }
        if (documento.Root is not { } raiz || raiz.Name != Namespace + "DPS")
            return (null, ErroSefin.Criar(MotivosNFSe.TipoNaoTratado, $"A raiz é {documento.Root?.Name.LocalName}; a Sefin recebe a DPS."));

        var problemas = SchemasLeiaute.Dps.Validar(texto);
        if (problemas.Count > 0)
            return (null, ErroSefin.Criar(MotivosNFSe.FalhaSchema, problemas[0].Mensagem));
        return (new DpsAberta(raiz, texto), null);
    }

    // As regras da DPS e a NFS-e, na ordem: a assinatura (E0717), o emitente (E9996), o documento do prestador (E0080,
    // E0096), a assinatura e o titular (E0714, E0718), o Id (E0004), o ambiente (E0006), a versão 1.00 (E0001), a emissão
    // (E0008), a competência (E0015), o município emissor (503 paralisado, E0037, E0038, E0039), a substituição (E.8), a
    // duplicidade (E0014) e a parametrização do município de incidência (E0595, E0177, E0541, E0667, E0670).
    public ResultadoSefin Emitir(DpsAberta aberta, DateTimeOffset recebimento)
    {
        ArgumentNullException.ThrowIfNull(aberta);
        var dps = aberta.Dps;
        var inf = dps.Element(Namespace + "infDPS")!;
        var idDps = inf.Attribute("Id")!.Value;
        ResultadoSefin Rejeitar(string codigo, string? complemento = null) =>
            new(StatusCodes.Status400BadRequest, Erro(recebimento, idDps, ErroSefin.Criar(codigo, complemento)), $"rejeitada: {codigo}");

        if (dps.Element(NamespaceAssinatura + "Signature") is null)
            return Rejeitar(MotivosNFSe.AssinaturaAusente);
        if (Valor(inf, "tpEmit") != "1")
            return Rejeitar(MotivosNFSe.EmitenteNaoPrestador);

        var prestador = inf.Element(Namespace + "prest")!;
        var cnpj = Valor(prestador, "CNPJ");
        var cpf = Valor(prestador, "CPF");
        if (cnpj is null && cpf is null)
            return Rejeitar(MotivosNFSe.IdDivergente, "O prestador que emite não tem CNPJ nem CPF.");
        if (cnpj is not null ? !Documentos.CnpjValido(cnpj) : !Documentos.CpfValido(cpf!))
            return Rejeitar(cnpj is not null ? MotivosNFSe.CnpjPrestadorInvalido : MotivosNFSe.CpfPrestadorInvalido);
        var documento = cnpj ?? cpf!;
        var assinatura = AssinaturaDigital.Verificar(aberta.Texto, idDps, documento);
        if (assinatura.Count > 0)
        {
            var problema = assinatura[0];
            return Rejeitar(problema.Codigo is "213" or "227" ? MotivosNFSe.AssinaturaOutroTitular : MotivosNFSe.AssinaturaInvalida, problema.Mensagem);
        }

        var emissor = int.Parse(Valor(inf, "cLocEmi")!, CultureInfo.InvariantCulture);
        var inscricao = (cnpj is not null ? "2" : "1") + documento.PadLeft(14, '0');
        var composto = "DPS" + emissor.ToString("D7", CultureInfo.InvariantCulture) + inscricao
            + Valor(inf, "serie")!.PadLeft(5, '0') + Valor(inf, "nDPS")!.PadLeft(15, '0');
        if (composto != idDps)
            return Rejeitar(MotivosNFSe.IdDivergente, $"O esperado é {composto}.");
        if (Valor(inf, "tpAmb") != "2")
            return Rejeitar(MotivosNFSe.AmbienteDivergente, "O emulador é a produção restrita (tpAmb 2).");
        if (dps.Attribute("versao")?.Value != "1.01")
            return Rejeitar(MotivosNFSe.VersaoExpirada);
        var emissao = DateTimeOffset.ParseExact(Valor(inf, "dhEmi")!, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        if (emissao > recebimento + Tolerancia)
            return Rejeitar(MotivosNFSe.EmissaoPosterior);
        var competencia = DateOnly.ParseExact(Valor(inf, "dCompet")!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (competencia > DateOnly.FromDateTime(emissao.DateTime))
            return Rejeitar(MotivosNFSe.CompetenciaPosterior);

        var (municipio, _) = municipios.Procurar(emissor);
        if (municipio.Paralisado)
            return new ResultadoSefin(StatusCodes.Status503ServiceUnavailable, Indisponivel($"O município {emissor} está paralisado no emulador."), "município paralisado: 503");
        if (Municipio.Uf(emissor) is null || municipio.Convenio == SituacaoConvenio.Inexistente)
            return Rejeitar(MotivosNFSe.MunicipioInexistente);
        if (municipio.Convenio == SituacaoConvenio.Inativo)
            return Rejeitar(MotivosNFSe.ConvenioInativo);
        if (municipio.Emissor == EmissorMunicipio.Proprio)
            return Rejeitar(MotivosNFSe.EmissorProprio);
        if (inf.Element(Namespace + "subst") is not null)
            return Rejeitar(MotivosNFSe.SubstituicaoNaoEmulada);
        if (repositorio.Dps(idDps) is { } existente)
            return Rejeitar(MotivosNFSe.DpsDuplicada, $"A NFS-e gerada é a {existente.Chave}.");

        // O município de incidência: o do emissor (LC 116, art. 3º, a regra geral); as exceções do artigo não são emuladas.
        var calculo = Calcular(inf, municipio);
        if (calculo.Erro is { } erro)
            return Rejeitar(erro);

        var (gerada, outra) = repositorio.Gerar(idDps, emissor, documento, (numero, numeroDfse, codigo) =>
            Gerar(dps, inf, municipio, inscricao, cnpj, cpf, numero, numeroDfse, codigo, calculo, recebimento));
        if (outra is not null)
            return Rejeitar(MotivosNFSe.DpsDuplicada, $"A NFS-e gerada é a {outra.Chave}.");
        var nota = gerada!;
        return new ResultadoSefin(StatusCodes.Status201Created, new RespostaEmissao(2, VersaoAplicativo, DataHora(recebimento), idDps, nota.Chave,
            Compactar(nota.Xml), null), "NFS-e gerada: " + nota.Chave);
    }

    // O JSON do erro (400, 404): o ambiente, a versão, o processamento, a DPS (quando há) e os erros.
    public static RespostaErro Erro(DateTimeOffset processamento, string? idDps, params ErroSefin[] erros) =>
        new(2, VersaoAplicativo, DataHora(processamento), idDps, erros);

    // O 503 da paralisação, comandada ou do município.
    public static object Indisponivel(string complemento) =>
        new { erros = new[] { new ErroSefin("EMULADA-PARALISACAO", "Serviço indisponível.", complemento) } };

    // O XML em GZip e Base64, como vai e volta no JSON.
    public static string Compactar(string xml)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal))
            gzip.Write(Encoding.UTF8.GetBytes(xml));
        return Convert.ToBase64String(saida.ToArray());
    }

    // O dhProc e o dataHoraProcessamento: o horário de Brasília (TSDateTimeUTC, com o fuso em horas inteiras).
    public static string DataHora(DateTimeOffset instante) =>
        instante.ToOffset(TimeSpan.FromHours(-3)).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

    // Os valores da NFS-e (TCValoresNFSe), pela parametrização do município: a dedução, o benefício, a base, a alíquota, o
    // ISS, as retenções e o líquido. Simplificado: o ISS só no tributável (tribISSQN 1) e fora do MEI (opSimpNac 2); a
    // dedução por documentos e o PIS e a COFINS retidos não entram.
    private static Calculo Calcular(XElement inf, Municipio municipio)
    {
        var valores = inf.Element(Namespace + "valores")!;
        var tributacao = valores.Element(Namespace + "trib")!.Element(Namespace + "tribMun")!;
        var regime = inf.Element(Namespace + "prest")!.Element(Namespace + "regTrib")!;
        var servico = Decimal(valores, "vServPrest", "vServ") ?? 0;
        var incondicionado = Decimal(valores, "vDescCondIncond", "vDescIncond") ?? 0;
        var condicionado = Decimal(valores, "vDescCondIncond", "vDescCond") ?? 0;
        decimal? deducao = valores.Element(Namespace + "vDedRed") is { } reducao
            ? Decimal(reducao, "vDR") ?? (Decimal(reducao, "pDR") is { } percentual ? Arredondar(servico * percentual / 100) : null)
            : null;
        var tributavel = Valor(tributacao, "tribISSQN") == "1" && Valor(regime, "opSimpNac") != "2";
        var retencao = Valor(tributacao, "tpRetISSQN");
        var informada = Decimal(tributacao, "pAliq");

        if (informada > 5)
            return new Calculo(MotivosNFSe.AliquotaSuperior);
        var especial = Valor(regime, "regEspTrib");
        if (especial is not (null or "0") && !(municipio.RegimesEspeciais ?? []).Contains(int.Parse(especial, CultureInfo.InvariantCulture)))
            return new Calculo(MotivosNFSe.RegimeNaoAdmitido);
        BeneficioMunicipio? beneficio = null;
        if (tributacao.Element(Namespace + "BM") is { } bm)
        {
            beneficio = (municipio.Beneficios ?? []).FirstOrDefault(b => b.Numero == Valor(bm, "nBM"));
            if (beneficio is null)
                return new Calculo(MotivosNFSe.BeneficioInexistente);
        }
        if (retencao == "2" && Valor(inf, "toma", "CPF") is { } tomador && !(municipio.Retencoes ?? []).Contains(tomador))
            return new Calculo(MotivosNFSe.RetencaoCpfTomador);
        if (retencao == "3" && Valor(inf, "interm", "CPF") is { } intermediario && !(municipio.Retencoes ?? []).Contains(intermediario))
            return new Calculo(MotivosNFSe.RetencaoCpfIntermediario);

        var federais = valores.Element(Namespace + "trib")!.Element(Namespace + "tribFed");
        var retidosFederais = (federais is null ? 0 : (Decimal(federais, "vRetCP") ?? 0) + (Decimal(federais, "vRetIRRF") ?? 0) + (Decimal(federais, "vRetCSLL") ?? 0));
        if (!tributavel)
        {
            var total = retidosFederais > 0 ? retidosFederais : (decimal?)null;
            return new Calculo(null, deducao, null, null, null, null, null, total, servico - incondicionado - condicionado - retidosFederais);
        }

        var aliquota = informada ?? municipio.Aliquota(Valor(inf, "serv", "cServ", "cTribNac")!);
        decimal? reducaoBeneficio = null;
        var isento = false;
        switch (beneficio?.Tipo)
        {
            case 1:
                isento = true;
                break;
            case 2:
                reducaoBeneficio = Decimal(tributacao, "BM", "pRedBCBM") is { } reducaoPercentual ? Arredondar((servico - incondicionado - (deducao ?? 0)) * reducaoPercentual / 100) : null;
                break;
            case 3:
                reducaoBeneficio = Decimal(tributacao, "BM", "vRedBCBM");
                break;
            case 4:
                aliquota = beneficio.Aliquota!.Value;
                break;
        }
        var baseCalculo = Math.Max(0, servico - incondicionado - (deducao ?? 0) - (reducaoBeneficio ?? 0));
        var issqn = isento ? 0 : Arredondar(baseCalculo * aliquota / 100);
        var retido = retencao is "2" or "3" ? issqn : 0;
        var retencoes = retido + retidosFederais;
        return new Calculo(null, deducao, beneficio?.Tipo, reducaoBeneficio, baseCalculo, aliquota, issqn, retencoes > 0 ? retencoes : null,
            servico - incondicionado - condicionado - retencoes);
    }

    private NFSeRegistrada Gerar(XElement dps, XElement inf, Municipio municipio, string inscricao, string? cnpj, string? cpf,
        long numero, long numeroDfse, int codigo, Calculo calculo, DateTimeOffset processamento)
    {
        var emissor = municipio.Codigo;
        var brasilia = processamento.ToOffset(TimeSpan.FromHours(-3));
        var parte = emissor.ToString("D7", CultureInfo.InvariantCulture) + "2" + inscricao + numero.ToString("D13", CultureInfo.InvariantCulture)
            + brasilia.ToString("yyMM", CultureInfo.InvariantCulture) + RepositorioNFSe.Codigo(codigo);
        var chave = parte + Documentos.Modulo11(parte).ToString(CultureInfo.InvariantCulture);
        var prestacao = Valor(inf, "serv", "locPrest", "cLocPrestacao");
        var nomePrestacao = prestacao is null
            ? "País " + Valor(inf, "serv", "locPrest", "cPaisPrestacao")
            : municipios.Procurar(int.Parse(prestacao, CultureInfo.InvariantCulture)).Municipio.Nome!;
        var prestador = inf.Element(Namespace + "prest")!;
        var tributacaoNacional = Valor(inf, "serv", "cServ", "cTribNac")!;

        var informacoes = new XElement(Namespace + "infNFSe", new XAttribute("Id", "NFS" + chave),
            Elemento("xLocEmi", municipio.Nome),
            Elemento("xLocPrestacao", nomePrestacao),
            Elemento("nNFSe", numero.ToString(CultureInfo.InvariantCulture)),
            calculo.BaseCalculo is null ? null : Elemento("cLocIncid", emissor.ToString(CultureInfo.InvariantCulture)),
            calculo.BaseCalculo is null ? null : Elemento("xLocIncid", municipio.Nome),
            Elemento("xTribNac", $"Serviço do código de tributação nacional {tributacaoNacional} (emulado)"),
            Elemento("verAplic", VersaoAplicativo),
            Elemento("ambGer", "2"),
            Elemento("tpEmis", "1"),
            Elemento("procEmi", "1"),
            Elemento("cStat", "100"),
            Elemento("dhProc", DataHora(processamento)),
            Elemento("nDFSe", numeroDfse.ToString(CultureInfo.InvariantCulture)),
            new XElement(Namespace + "emit",
                cnpj is not null ? Elemento("CNPJ", cnpj) : Elemento("CPF", cpf),
                Valor(prestador, "IM") is { } im ? Elemento("IM", im) : null,
                Elemento("xNome", $"CONTRIBUINTE EMULADO {cnpj ?? cpf}"),
                new XElement(Namespace + "enderNac",
                    Elemento("xLgr", "LOGRADOURO EMULADO"),
                    Elemento("nro", "S/N"),
                    Elemento("xBairro", "CENTRO"),
                    Elemento("cMun", emissor.ToString(CultureInfo.InvariantCulture)),
                    Elemento("UF", Municipio.Uf(emissor)),
                    Elemento("CEP", "00000000"))),
            new XElement(Namespace + "valores",
                Monetario("vCalcDR", calculo.Deducao),
                calculo.TipoBeneficio is { } tipo ? Elemento("tpBM", tipo.ToString(CultureInfo.InvariantCulture)) : null,
                Monetario("vCalcBM", calculo.ReducaoBeneficio),
                Monetario("vBC", calculo.BaseCalculo),
                Monetario("pAliqAplic", calculo.Aliquota),
                Monetario("vISSQN", calculo.Issqn),
                Monetario("vTotalRet", calculo.Retencoes),
                Monetario("vLiq", calculo.Liquido)),
            new XElement(dps));
        var xml = new XElement(Namespace + "NFSe", new XAttribute("versao", "1.01"), informacoes).ToString(SaveOptions.DisableFormatting);
        xml = AssinaturaDigital.Assinar(xml, "NFS" + chave, assinante.Certificado);
        return new NFSeRegistrada(chave, inf.Attribute("Id")!.Value, numero, numeroDfse, emissor, cnpj ?? cpf!, processamento, xml);
    }

    // O texto do elemento no caminho, a partir de um elemento da DPS; nulo se não há.
    public static string? Valor(XElement origem, params string[] caminho)
    {
        XElement? atual = origem;
        foreach (var nome in caminho)
            atual = atual?.Element(Namespace + nome);
        return atual?.Value;
    }

    private static decimal? Decimal(XElement origem, params string[] caminho) =>
        Valor(origem, caminho) is { } texto ? decimal.Parse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) : null;

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static XElement? Elemento(string nome, string? valor) => valor is null ? null : new XElement(Namespace + nome, valor);

    private static XElement? Monetario(string nome, decimal? valor) =>
        valor is null ? null : new XElement(Namespace + nome, valor.Value.ToString("0.00", CultureInfo.InvariantCulture));

    // O prefixo em algum elemento ou atributo do namespace da NFS-e (E1228); XML mal formado fica para a carga.
    private static bool UsaPrefixo(string texto)
    {
        using var leitor = XmlReader.Create(new StringReader(texto), Configuracao());
        while (leitor.Read())
        {
            if (leitor.NodeType != XmlNodeType.Element)
                continue;
            if (leitor.NamespaceURI == Namespace.NamespaceName && leitor.Prefix.Length > 0)
                return true;
        }
        return false;
    }

    private static XmlReaderSettings Configuracao() => new() { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

    [GeneratedRegex("^\\s*<\\?xml(?:[^>]*?\\bencoding\\s*=\\s*[\"'](?<codificacao>[^\"']+)[\"'])?[^>]*\\?>", RegexOptions.CultureInvariant)]
    private static partial Regex Declaracao();

    private sealed record Calculo(string? Erro, decimal? Deducao = null, int? TipoBeneficio = null, decimal? ReducaoBeneficio = null,
        decimal? BaseCalculo = null, decimal? Aliquota = null, decimal? Issqn = null, decimal? Retencoes = null, decimal? Liquido = null);
}

// O JSON da DPS que gerou a NFS-e (201).
internal sealed record RespostaEmissao(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string IdDps, string ChaveAcesso,
    string NfseXmlGZipB64, IReadOnlyList<ErroSefin>? Alertas);

// O JSON da rejeição (400) e da consulta que não acha (404).
internal sealed record RespostaErro(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string? IdDps, IReadOnlyList<ErroSefin> Erros);

// O JSON da consulta pela chave (GET /nfse/{chave}).
internal sealed record RespostaNFSe(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string ChaveAcesso, string NfseXmlGZipB64);

// O JSON da consulta pela DPS (GET /dps/{id}).
internal sealed record RespostaDps(int TipoAmbiente, string VersaoAplicativo, string DataHoraProcessamento, string IdDps, string ChaveAcesso);

// A AC de teste, com a chave, no papel da Sefin que assina a NFS-e (docs/NFSE.md 5).
internal sealed class AssinanteNFSe(X509Certificate2 certificado)
{
    public X509Certificate2 Certificado { get; } = certificado;
}
