using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace OpenDFe;

/// <summary>
/// Assinatura XMLDSig dos documentos fiscais, no padrão da SEFAZ, e a verificação da assinatura de um XML
/// recebido (PROJETO.md 7.2).
/// </summary>
/// <remarks>
/// <para>
/// O padrão (MOC 7.0, Visão Geral, 4.2.4): assinatura <i>enveloped</i> do elemento identificado pelo atributo
/// <c>Id</c> (<c>infNFe</c>, <c>infEvento</c>, <c>infInut</c>...), com a canonicalização C14N
/// (<c>http://www.w3.org/TR/2001/REC-xml-c14n-20010315</c>), RSA com SHA-1, uma única <c>Reference</c> com
/// <c>URI="#&lt;Id&gt;"</c>, as transformações <i>enveloped-signature</i> e C14N, o resumo SHA-1 e o
/// <c>KeyInfo</c> só com o certificado do assinante. A <c>Signature</c> é o último filho do pai do elemento
/// assinado (<c>NFe</c>, <c>evento</c>, <c>inutNFe</c>), com o namespace do XMLDSig declarado nela.
/// </para>
/// <para>
/// O XML entra e sai como texto, e nada fora da assinatura é alterado: <see cref="Assinar"/> devolve o texto
/// recebido com a <c>Signature</c> inserida, e <see cref="Verificar"/> não altera nada. O espaço em branco é
/// preservado, porque faz parte do que é assinado; gravar ou transmitir o XML assinado exige os mesmos
/// caracteres (em UTF-8), sem reformatar. O texto não começa pelo BOM, que é da codificação dos bytes:
/// <see cref="StreamReader"/> e <see cref="File.ReadAllText(string)"/> o tiram, e <c>Encoding.UTF8.GetString</c>
/// não.
/// </para>
/// <para>
/// No Linux, a assinatura e a verificação com SHA-1 dependem da política de criptografia da máquina
/// (PROJETO.md 2.8).
/// </para>
/// </remarks>
public static class AssinaturaDigital
{
    private const string NamespaceAssinatura = SignedXml.XmlDsigNamespaceUrl;
    private const string AtributoId = "Id";

    /// <summary>Assina o elemento identificado pelo <c>Id</c>, no padrão da SEFAZ.</summary>
    /// <param name="xml">O XML do documento ou da mensagem (a nota, o evento, a inutilização, o lote).</param>
    /// <param name="id">
    /// O valor do atributo <c>Id</c> do elemento a assinar, sem o <c>#</c> (<c>"NFe3126..."</c>). Tem de
    /// aparecer uma única vez no XML.
    /// </param>
    /// <param name="certificado">O certificado do assinante, com a chave privada RSA.</param>
    /// <returns>
    /// O XML recebido, sem nenhuma outra mudança, com a <c>Signature</c> inserida como último filho do pai do
    /// elemento assinado.
    /// </returns>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    /// <exception cref="ArgumentException">
    /// O <c>Id</c> está vazio, não aparece no XML ou aparece mais de uma vez; o elemento é a raiz do XML (a
    /// assinatura vai no pai dele); o pai já tem uma <c>Signature</c>.
    /// </exception>
    /// <exception cref="XmlException">O XML não é bem formado ou tem DTD.</exception>
    /// <exception cref="CryptographicException">
    /// O certificado não tem a chave privada, a chave não é RSA, ou a assinatura falhou (no Linux, a política
    /// de criptografia que recusa SHA-1).
    /// </exception>
    public static string Assinar(string xml, string id, X509Certificate2 certificado)
    {
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(certificado);
        CertificadoDigital.ExigirChavePrivadaRsa(certificado);

        var documento = Carregar(xml);
        var elementos = ElementosId(documento, id);
        if (elementos.Count == 0)
            throw new ArgumentException($"Nenhum elemento tem o atributo Id \"{id}\".", nameof(id));
        if (elementos.Count > 1)
            throw new ArgumentException($"O Id \"{id}\" aparece em mais de um elemento.", nameof(id));
        var elemento = elementos[0];
        if (!elemento.HasAttribute(AtributoId))
            throw new ArgumentException($"O elemento {elemento.Name} leva o identificador num atributo que não é \"Id\".", nameof(id));
        if (elemento.ParentNode is not XmlElement pai)
            throw new ArgumentException($"O elemento {elemento.Name} é a raiz do XML; a assinatura vai no pai do elemento assinado.", nameof(id));
        if (Assinaturas(pai).Count > 0)
            throw new ArgumentException($"O elemento {pai.Name} já tem uma assinatura.", nameof(xml));

        string assinatura;
        using (var chave = certificado.GetRSAPrivateKey()!)
        {
            var assinador = new SignedXml(documento) { SigningKey = chave };
            assinador.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
            assinador.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
            var referencia = new Reference("#" + id) { DigestMethod = SignedXml.XmlDsigSHA1Url };
            referencia.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            referencia.AddTransform(new XmlDsigC14NTransform());
            assinador.AddReference(referencia);
            var info = new KeyInfo();
            info.AddClause(new KeyInfoX509Data(certificado));   // só o certificado do assinante (EndCertOnly)
            assinador.KeyInfo = info;
            assinador.ComputeSignature();
            assinatura = assinador.GetXml().OuterXml;
        }

        var assinado = xml.Insert(InicioFechamento(xml, id), assinatura);

        // Conferência do próprio resultado: o XML assinado, lido de novo do texto, tem de verificar (a estrutura
        // e o valor; o uso da chave do certificado é de quem verifica).
        var problemas = new List<Problema>();
        using (VerificarAssinatura(Carregar(assinado), id, conferirCertificado: false, problemas))
        {
            if (problemas.Count > 0)
                throw new CryptographicException("A assinatura gerada não confere: " + problemas[0].Mensagem);
        }
        return assinado;
    }

    /// <summary>
    /// Verifica a assinatura de um elemento de um XML recebido: o padrão da SEFAZ, a referência ao elemento,
    /// o valor da assinatura e o titular do certificado.
    /// </summary>
    /// <param name="xml">O XML recebido, como foi lido (os caracteres não podem ter mudado).</param>
    /// <param name="id">
    /// O valor do atributo <c>Id</c> do elemento que deve estar assinado, sem o <c>#</c>: o do elemento que
    /// quem chama vai ler (o <c>infNFe</c> da nota), nunca o que a assinatura diz.
    /// </param>
    /// <param name="cnpjOuCpf">
    /// O CNPJ ou o CPF de quem deve ter assinado (o emitente da nota, o autor do evento), com ou sem máscara.
    /// </param>
    /// <returns>
    /// Os problemas encontrados, todos de severidade <see cref="Severidade.Erro"/> e com o código da rejeição
    /// da SEFAZ (MOC 7.0, Anexo I, grupos E e F); vazio se a assinatura é válida.
    /// </returns>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    /// <exception cref="ArgumentException">O <c>Id</c> está vazio, ou o documento não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="XmlException">O XML não é bem formado ou tem DTD.</exception>
    /// <remarks>
    /// <para>
    /// A assinatura verificada é a <c>Signature</c> filha do pai do elemento com o <c>Id</c> (no <c>nfeProc</c>,
    /// a da <c>NFe</c>, não a do protocolo). Para não aceitar uma assinatura de outro elemento (<i>signature
    /// wrapping</i>), o <c>Id</c> tem de aparecer uma única vez no XML, e a assinatura tem uma única
    /// <c>Reference</c>, para ele.
    /// </para>
    /// <para>
    /// O titular é comparado como a SEFAZ compara (Anexo I, F03 e F03A): com um CNPJ, a raiz (as 8 primeiras
    /// posições), porque o certificado de um estabelecimento assina pelos outros da mesma empresa (rejeição
    /// 213); com um CPF, o CPF inteiro (rejeição 227).
    /// </para>
    /// <para>
    /// <b>Não confere</b> a cadeia do certificado (ICP-Brasil), a revogação nem a validade na data da emissão
    /// (rejeições 291, 293 a 296).
    /// </para>
    /// </remarks>
    public static IReadOnlyList<Problema> Verificar(string xml, string id, string cnpjOuCpf)
    {
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(cnpjOuCpf);
        string? cnpj = null, cpf = null;
        if (Cnpj.Valido(cnpjOuCpf))
            cnpj = Cnpj.RemoverMascara(cnpjOuCpf);
        else if (Cpf.Valido(cnpjOuCpf))
            cpf = Cpf.RemoverMascara(cnpjOuCpf);
        else
            throw new ArgumentException("O documento não é um CNPJ nem um CPF válido.", nameof(cnpjOuCpf));

        var problemas = new List<Problema>();
        using var certificado = VerificarAssinatura(Carregar(xml), id, conferirCertificado: true, problemas);
        if (certificado is not null)
            ConferirTitular(certificado, cnpj, cpf, problemas);
        return problemas;
    }

    // A estrutura, o certificado e o valor da assinatura. Devolve o certificado do KeyInfo (de quem chama),
    // ou null se a estrutura ou o certificado não permitem continuar.
    private static X509Certificate2? VerificarAssinatura(XmlDocument documento, string id, bool conferirCertificado, List<Problema> problemas)
    {
        var elementos = ElementosId(documento, id);
        if (elementos.Count == 0)
        {
            problemas.Add(Padrao($"Nenhum elemento tem o atributo Id \"{id}\".", AtributoId));
            return null;
        }
        if (elementos.Count > 1)
        {
            problemas.Add(Padrao($"O Id \"{id}\" aparece em mais de um elemento.", AtributoId));
            return null;
        }
        if (elementos[0].ParentNode is not XmlElement pai)
        {
            problemas.Add(Padrao($"O elemento {elementos[0].Name} é a raiz do XML e não tem a assinatura ao lado.", "Signature"));
            return null;
        }

        var assinaturas = Assinaturas(pai);
        if (assinaturas.Count != 1)
        {
            problemas.Add(Padrao(assinaturas.Count == 0
                ? $"O elemento {elementos[0].Name} não está assinado: falta a Signature em {pai.Name}."
                : $"O elemento {pai.Name} tem mais de uma Signature.", "Signature"));
            return null;
        }
        var assinatura = assinaturas[0];

        var textoCertificado = ConferirEstrutura(assinatura, id, problemas);
        if (textoCertificado is null)
            return null;

        var certificado = LerCertificado(textoCertificado, conferirCertificado, problemas);
        if (certificado is null)
            return null;

        bool confere;
        try
        {
            using var chave = certificado.GetRSAPublicKey()!;
            var verificador = new SignedXml(documento);
            verificador.LoadXml(assinatura);
            confere = verificador.CheckSignature(chave);
        }
        catch (CryptographicException)
        {
            confere = false;
        }
        if (!confere)
        {
            problemas.Add(new Problema("297", Severidade.Erro,
                "Assinatura difere do calculado: o conteúdo assinado ou o valor da assinatura foram alterados.")
            { Tag = "SignatureValue" });
        }
        return certificado;
    }

    // MOC 7.0, Visão Geral, 4.2.4 (XS01 a XS21), e o xmldsig-core-schema_v1.01.xsd. Devolve o texto do
    // X509Certificate, ou null com o problema.
    private static string? ConferirEstrutura(XmlElement assinatura, string id, List<Problema> problemas)
    {
        var filhos = Filhos(assinatura);
        if (filhos.Count != 3 || !E(filhos[0], "SignedInfo") || !E(filhos[1], "SignatureValue") || !E(filhos[2], "KeyInfo"))
        {
            problemas.Add(Padrao("A Signature tem de ter SignedInfo, SignatureValue e KeyInfo, nesta ordem, e nada mais.", "Signature"));
            return null;
        }

        var info = Filhos(filhos[0]);
        if (info.Count != 3 || !E(info[0], "CanonicalizationMethod") || !E(info[1], "SignatureMethod") || !E(info[2], "Reference"))
        {
            problemas.Add(Padrao("O SignedInfo tem de ter CanonicalizationMethod, SignatureMethod e uma única Reference.", "SignedInfo"));
            return null;
        }
        if (info[0].GetAttribute("Algorithm") != SignedXml.XmlDsigC14NTransformUrl)
        {
            problemas.Add(Padrao($"A canonicalização tem de ser C14N ({SignedXml.XmlDsigC14NTransformUrl}).", "CanonicalizationMethod"));
            return null;
        }
        if (info[1].GetAttribute("Algorithm") != SignedXml.XmlDsigRSASHA1Url)
        {
            problemas.Add(Padrao($"O método de assinatura tem de ser RSA com SHA-1 ({SignedXml.XmlDsigRSASHA1Url}).", "SignatureMethod"));
            return null;
        }

        var referencia = info[2];
        if (referencia.GetAttribute("URI") != "#" + id)
        {
            problemas.Add(Padrao($"A Reference tem de apontar o elemento assinado (URI=\"#{id}\").", "Reference"));
            return null;
        }
        var partes = Filhos(referencia);
        if (partes.Count != 3 || !E(partes[0], "Transforms") || !E(partes[1], "DigestMethod") || !E(partes[2], "DigestValue"))
        {
            problemas.Add(Padrao("A Reference tem de ter Transforms, DigestMethod e DigestValue.", "Reference"));
            return null;
        }
        var transformacoes = Filhos(partes[0]);
        // As duas, em qualquer ordem (o schema da SEFAZ pede o Algorithm único, não a ordem), sem XPath.
        var algoritmos = transformacoes.Select(t => t.GetAttribute("Algorithm")).ToHashSet(StringComparer.Ordinal);
        if (transformacoes.Count != 2 || !transformacoes.All(t => E(t, "Transform") && !t.HasChildNodes)
            || !algoritmos.SetEquals([SignedXml.XmlDsigEnvelopedSignatureTransformUrl, SignedXml.XmlDsigC14NTransformUrl]))
        {
            problemas.Add(Padrao("As transformações têm de ser exatamente a enveloped-signature e a C14N.", "Transforms"));
            return null;
        }
        if (partes[1].GetAttribute("Algorithm") != SignedXml.XmlDsigSHA1Url)
        {
            problemas.Add(Padrao($"O resumo tem de ser SHA-1 ({SignedXml.XmlDsigSHA1Url}).", "DigestMethod"));
            return null;
        }

        var chave = Filhos(filhos[2]);
        var dados = chave.Count == 1 && E(chave[0], "X509Data") ? Filhos(chave[0]) : null;
        if (dados is null || dados.Count != 1 || !E(dados[0], "X509Certificate"))
        {
            problemas.Add(Padrao("O KeyInfo tem de ter só o X509Data com um único X509Certificate, o do assinante.", "KeyInfo"));
            return null;
        }
        return dados[0].InnerText;
    }

    // MOC 7.0, Anexo I, E01: versão 3, não é de AC, uso de chave com assinatura digital e não recusa.
    private static X509Certificate2? LerCertificado(string base64, bool conferirUso, List<Problema> problemas)
    {
        X509Certificate2 certificado;
        try
        {
            certificado = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(base64.Trim()));
        }
        catch (Exception e) when (e is FormatException or CryptographicException)
        {
            problemas.Add(CertificadoInvalido("O X509Certificate não é um certificado legível."));
            return null;
        }

        string? motivo = null;
        using (var rsa = certificado.GetRSAPublicKey())
        {
            if (rsa is null)
                motivo = "A chave do certificado não é RSA.";
        }
        if (!conferirUso && motivo is null)
            return certificado;
        if (motivo is null && certificado.Version != 3)
            motivo = "O certificado não é da versão 3.";
        if (motivo is null && certificado.Extensions.OfType<X509BasicConstraintsExtension>().Any(b => b.CertificateAuthority))
            motivo = "O certificado é de uma autoridade certificadora, não de um usuário final.";
        if (motivo is null && certificado.Extensions.OfType<X509KeyUsageExtension>().FirstOrDefault() is { } usos
            && !(usos.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature) && usos.KeyUsages.HasFlag(X509KeyUsageFlags.NonRepudiation)))
            motivo = "O uso da chave do certificado não prevê a assinatura digital e o não repúdio.";

        if (motivo is null)
            return certificado;
        certificado.Dispose();
        problemas.Add(CertificadoInvalido(motivo));
        return null;
    }

    private static void ConferirTitular(X509Certificate2 certificado, string? cnpj, string? cpf, List<Problema> problemas)
    {
        var titular = CertificadoDigital.LerTitular(certificado);
        if (titular is null)
        {
            problemas.Add(new Problema("292", Severidade.Erro,
                "Certificado de assinatura sem CNPJ/CPF: o certificado não tem o CNPJ nem o CPF do titular nos campos da ICP-Brasil.")
            { Tag = "X509Certificate" });
            return;
        }
        if (cnpj is not null && (titular.Cnpj is null || !titular.Cnpj.AsSpan(0, 8).SequenceEqual(cnpj.AsSpan(0, 8))))
        {
            problemas.Add(new Problema("213", Severidade.Erro,
                $"CNPJ-Base do emitente difere do CNPJ-Base do certificado digital: esperado {cnpj[..8]}, o certificado é de {titular.Cnpj ?? "um CPF"}.")
            { Tag = "X509Certificate" });
        }
        else if (cpf is not null && titular.Cpf != cpf)
        {
            problemas.Add(new Problema("227", Severidade.Erro,
                $"CPF do emitente difere do CPF do certificado digital: esperado {cpf}, o certificado é de {titular.Cpf ?? "um CNPJ"}.")
            { Tag = "X509Certificate" });
        }
    }

    // Sem DTD nem resolução de entidades externas; o espaço em branco faz parte do que é assinado.
    private static XmlDocument Carregar(string xml)
    {
        var documento = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using var leitor = XmlReader.Create(new StringReader(xml), Configuracao());
        documento.Load(leitor);
        return documento;
    }

    private static XmlReaderSettings Configuracao() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    // Os mesmos nomes de atributo que o SignedXml aceita como identificador (Id, id, ID): um Id repetido em
    // qualquer um deles tornaria a referência ambígua.
    private static List<XmlElement> ElementosId(XmlDocument documento, string id)
    {
        var encontrados = new List<XmlElement>();
        foreach (XmlElement elemento in documento.GetElementsByTagName("*"))
        {
            if (elemento.GetAttribute("Id") == id || elemento.GetAttribute("id") == id || elemento.GetAttribute("ID") == id)
                encontrados.Add(elemento);
        }
        return encontrados;
    }

    private static List<XmlElement> Assinaturas(XmlElement pai) =>
        pai.ChildNodes.OfType<XmlElement>().Where(e => E(e, "Signature")).ToList();

    private static List<XmlElement> Filhos(XmlElement elemento) => elemento.ChildNodes.OfType<XmlElement>().ToList();

    private static bool E(XmlElement elemento, string nome) =>
        elemento.LocalName == nome && elemento.NamespaceURI == NamespaceAssinatura;

    private static Problema Padrao(string detalhe, string tag) =>
        new("298", Severidade.Erro, "Assinatura difere do padrão do sistema: " + detalhe) { Tag = tag };

    private static Problema CertificadoInvalido(string detalhe) =>
        new("290", Severidade.Erro, "Certificado de assinatura inválido: " + detalhe) { Tag = "X509Certificate" };

    // A posição, no texto, do "</" do fechamento do pai do elemento com o Id: a Signature entra ali, e o resto
    // do texto fica como veio. A linha e a coluna vêm do XmlReader (que conta "\r\n", "\r" e "\n" como fim de
    // linha), e a posição é conferida contra o nome do elemento.
    private static int InicioFechamento(string xml, string id)
    {
        using var leitor = XmlReader.Create(new StringReader(xml), Configuracao());
        var linhas = (IXmlLineInfo)leitor;
        var profundidadePai = -1;
        while (leitor.Read())
        {
            if (profundidadePai < 0)
            {
                if (leitor.NodeType == XmlNodeType.Element && leitor.GetAttribute(AtributoId) == id)
                    profundidadePai = leitor.Depth - 1;
            }
            else if (leitor.NodeType == XmlNodeType.EndElement && leitor.Depth == profundidadePai)
            {
                var posicao = Posicao(xml, linhas.LineNumber, linhas.LinePosition) - 2;
                if (posicao >= 0 && xml.AsSpan(posicao).StartsWith("</" + leitor.Name, StringComparison.Ordinal))
                    return posicao;
                break;
            }
        }
        throw new InvalidOperationException("Não foi possível localizar no texto o fechamento do elemento que recebe a assinatura.");
    }

    private static int Posicao(string texto, int linha, int coluna)
    {
        var inicio = 0;
        for (var atual = 1; atual < linha; atual++)
        {
            var fim = texto.AsSpan(inicio).IndexOfAny('\r', '\n');
            if (fim < 0)
                return -1;
            inicio += fim;
            inicio += texto[inicio] == '\r' && inicio + 1 < texto.Length && texto[inicio + 1] == '\n' ? 2 : 1;
        }
        return inicio + coluna - 1;
    }
}
