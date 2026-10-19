using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OpenDFe;

/// <summary>
/// Utilitários do certificado digital A1 (PFX): abrir, conferir a validade e ler o titular (PROJETO.md 7.3).
/// </summary>
/// <remarks>
/// <para>
/// A lib trabalha com o <see cref="X509Certificate2"/> com a chave privada RSA; de onde ele vem é de quem usa.
/// <see cref="AbrirPfx"/> e <see cref="AbrirArquivoPfx"/> abrem o PFX; o certificado do repositório do
/// Windows (inclusive o A3, pelo driver do cartão ou token) é usado como já vem, e
/// <see cref="LerTitular"/> e <see cref="Validade"/> valem para qualquer <see cref="X509Certificate2"/>.
/// </para>
/// <para>
/// Nada aqui confere a cadeia do certificado: abrir e ler o titular funcionam com qualquer certificado,
/// inclusive o autoassinado dos testes. A cadeia ICP-Brasil é conferida por quem usa o certificado (o TLS
/// e a verificação da assinatura).
/// </para>
/// </remarks>
public static class CertificadoDigital
{
    private const string OidNomeAlternativo = "2.5.29.17";
    private const string OidNomeComum = "2.5.4.3";
    private const string OidNumeroDeSerie = "2.5.4.5";

    // DOC-ICP-04, Anexo I e 7.1.2.2: os campos otherName do "Subject Alternative Name".
    private const string OidDadosPessoaFisica = "2.16.76.1.3.1";
    private const string OidCnpj = "2.16.76.1.3.3";

    // No 2.16.76.1.3.1: a data de nascimento (8 posições), o CPF (11), o NIS (11), o RG (15) e o órgão
    // expedidor com a UF (até 10).
    private const int InicioCpfNosDadosPessoaFisica = 8;

    /// <summary>Abre um certificado A1 a partir do conteúdo de um PFX (PKCS#12).</summary>
    /// <param name="conteudo">Os bytes do PFX.</param>
    /// <param name="senha">A senha do PFX (vazia, se ele não tem senha).</param>
    /// <returns>
    /// O certificado com a chave privada RSA. É de quem chama: descarte-o (<see cref="IDisposable.Dispose"/>)
    /// quando não for mais usado; no Windows, a chave fica num contêiner temporário do perfil do usuário até o
    /// descarte.
    /// </returns>
    /// <exception cref="ArgumentNullException">O conteúdo ou a senha são nulos.</exception>
    /// <exception cref="CryptographicException">
    /// O conteúdo não é um PFX; a senha está errada (ou o PFX está corrompido); o PFX não tem a chave privada;
    /// a chave não é RSA. A mensagem diz qual.
    /// </exception>
    /// <remarks>
    /// Se o PFX tem vários certificados (a cadeia junto), vale o primeiro com a chave privada. A validade não é
    /// conferida aqui (um certificado vencido abre, para ser lido): use <see cref="Validade"/>.
    /// </remarks>
    public static X509Certificate2 AbrirPfx(byte[] conteudo, string senha)
    {
        ArgumentNullException.ThrowIfNull(conteudo);
        ArgumentNullException.ThrowIfNull(senha);
        return Abrir(conteudo, senha);
    }

    /// <summary>Abre um certificado A1 a partir de um arquivo PFX (PKCS#12).</summary>
    /// <param name="caminho">O caminho do arquivo <c>.pfx</c> ou <c>.p12</c>.</param>
    /// <param name="senha">A senha do PFX (vazia, se ele não tem senha).</param>
    /// <returns>O certificado com a chave privada RSA, de quem chama (ver <see cref="AbrirPfx"/>).</returns>
    /// <exception cref="ArgumentNullException">A senha é nula.</exception>
    /// <exception cref="ArgumentException">O caminho é nulo ou vazio.</exception>
    /// <exception cref="IOException">O arquivo não existe ou não pode ser lido.</exception>
    /// <exception cref="UnauthorizedAccessException">Sem permissão para ler o arquivo.</exception>
    /// <exception cref="CryptographicException">Os mesmos casos de <see cref="AbrirPfx"/>.</exception>
    public static X509Certificate2 AbrirArquivoPfx(string caminho, string senha)
    {
        ArgumentException.ThrowIfNullOrEmpty(caminho);
        ArgumentNullException.ThrowIfNull(senha);
        return Abrir(File.ReadAllBytes(caminho), senha);
    }

    /// <summary>A situação do certificado num instante, pelo período de validade.</summary>
    /// <param name="certificado">O certificado.</param>
    /// <param name="instante">
    /// O instante a conferir: agora (<c>TimeProvider.GetUtcNow()</c>), ou a emissão do documento, na
    /// verificação de um XML recebido.
    /// </param>
    /// <returns>Se o certificado está vigente, ainda não vigente ou vencido no instante.</returns>
    /// <exception cref="ArgumentNullException">O certificado é nulo.</exception>
    public static ValidadeCertificado Validade(X509Certificate2 certificado, DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(certificado);
        var utc = instante.UtcDateTime;
        if (utc < certificado.NotBefore.ToUniversalTime())
            return ValidadeCertificado.AindaNaoVigente;
        if (utc > certificado.NotAfter.ToUniversalTime())
            return ValidadeCertificado.Vencido;
        return ValidadeCertificado.Vigente;
    }

    /// <summary>
    /// O titular do certificado: o CNPJ (e-CNPJ) ou o CPF (e-CPF) pelos campos da ICP-Brasil, e o nome.
    /// </summary>
    /// <param name="certificado">O certificado; a chave privada e a cadeia não importam.</param>
    /// <returns>
    /// O titular, ou <see langword="null"/> se o certificado não tem um CNPJ nem um CPF válido nos campos da
    /// ICP-Brasil.
    /// </returns>
    /// <exception cref="ArgumentNullException">O certificado é nulo.</exception>
    /// <remarks>
    /// <para>
    /// O documento é procurado, nesta ordem (DOC-ICP-04 8.3, 7.1.2.2 e Anexo I): no "Subject Alternative
    /// Name", o <c>otherName</c> 2.16.76.1.3.3 (as 14 posições do CNPJ, inclusive alfanumérico) e o
    /// 2.16.76.1.3.1 (o CPF nas posições 9 a 19, depois da data de nascimento); e o atributo
    /// <c>serialNumber</c> (2.5.4.5) do titular, onde os certificados da versão 8.2 em diante levam o CNPJ ou
    /// o CPF (o <c>otherName</c> fica obrigatório até 31/12/2028). O CPF do responsável num e-CNPJ
    /// (2.16.76.1.3.4) não é o titular. Campo preenchido com zeros, com o dígito errado ou malformado é
    /// ignorado.
    /// </para>
    /// <para>
    /// A cadeia não é conferida: um certificado autoassinado com os mesmos campos é lido do mesmo jeito.
    /// </para>
    /// </remarks>
    public static TitularCertificado? LerTitular(X509Certificate2 certificado)
    {
        ArgumentNullException.ThrowIfNull(certificado);

        LerNomeAlternativo(certificado, out var cnpj, out var cpf);
        LerNomeDoTitular(certificado, out var nomeComum, out var numeroDeSerie);

        if (cnpj is null && cpf is null && numeroDeSerie is not null)
        {
            if (numeroDeSerie.Length == Cnpj.Tamanho && Cnpj.Valido(numeroDeSerie))
                cnpj = numeroDeSerie;
            else if (numeroDeSerie.Length == Cpf.Tamanho && Cpf.Valido(numeroDeSerie))
                cpf = numeroDeSerie;
        }
        if (cnpj is null && cpf is null)
            return null;

        return new TitularCertificado(Nome(nomeComum, cnpj ?? cpf!), cnpj, cpf);
    }

    /// <summary>
    /// Confere que o certificado tem a chave privada e que ela é RSA (a NF-e assina com RSA). Usado ao abrir o
    /// PFX e, na 1.6, ao receber o certificado na configuração do núcleo.
    /// </summary>
    internal static void ExigirChavePrivadaRsa(X509Certificate2 certificado)
    {
        using (var publica = certificado.GetRSAPublicKey())
        {
            if (publica is null)
                throw new CryptographicException("A chave do certificado não é RSA; a assinatura dos documentos fiscais exige RSA.");
        }
        if (!certificado.HasPrivateKey)
            throw new CryptographicException("O certificado não tem a chave privada; a assinatura exige o PFX com a chave (certificado A1).");
        using var privada = certificado.GetRSAPrivateKey()
            ?? throw new CryptographicException("A chave privada do certificado não pôde ser lida como RSA.");
    }

    private static X509Certificate2 Abrir(byte[] conteudo, string senha)
    {
        if (!ParecePfx(conteudo))
            throw new CryptographicException("O conteúdo não é um PFX (PKCS#12).");

        X509Certificate2 certificado;
        try
        {
            // DefaultKeySet, não EphemeralKeySet: no Windows, o TLS (SChannel) não usa a chave efêmera, e o
            // mesmo certificado vai ao TLS mútuo com a SEFAZ. No Linux, a chave fica em memória de qualquer jeito.
            certificado = X509CertificateLoader.LoadPkcs12(conteudo, senha, X509KeyStorageFlags.DefaultKeySet);
        }
        catch (Pkcs12LoadLimitExceededException e)
        {
            throw new CryptographicException("O PFX excede os limites de leitura do .NET (tamanho, iterações ou quantidade de itens).", e);
        }
        catch (CryptographicException e)
        {
            throw new CryptographicException("O PFX não pôde ser aberto: a senha está errada ou o arquivo está corrompido.", e);
        }

        try
        {
            ExigirChavePrivadaRsa(certificado);
        }
        catch
        {
            certificado.Dispose();
            throw;
        }
        return certificado;
    }

    // A estrutura de fora do PFX (RFC 7292, 4): SEQUENCE { version INTEGER (3), authSafe ContentInfo, ... }.
    // Separa "não é um PFX" de "senha errada", que o .NET não distingue.
    private static bool ParecePfx(byte[] conteudo)
    {
        try
        {
            var pfx = new AsnReader(conteudo, AsnEncodingRules.BER).ReadSequence();
            if (!pfx.TryReadInt32(out var versao) || versao != 3)
                return false;
            pfx.ReadSequence().ReadObjectIdentifier();
            return true;
        }
        catch (AsnContentException)
        {
            return false;
        }
    }

    private static void LerNomeAlternativo(X509Certificate2 certificado, out string? cnpj, out string? cpf)
    {
        cnpj = null;
        cpf = null;
        foreach (var extensao in certificado.Extensions)
        {
            if (extensao.Oid?.Value != OidNomeAlternativo)
                continue;
            try
            {
                LerOutrosNomes(extensao.RawData, ref cnpj, ref cpf);
            }
            catch (AsnContentException)
            {
                // Extensão malformada: o titular sai do serialNumber, se houver.
            }
            return;
        }
    }

    // GeneralNames ::= SEQUENCE OF GeneralName; otherName [0] IMPLICIT SEQUENCE { type-id OID,
    // value [0] EXPLICIT ANY } (RFC 5280, 4.2.1.6). Os outros tipos de nome são pulados.
    private static void LerOutrosNomes(byte[] extensao, ref string? cnpj, ref string? cpf)
    {
        var outroNome = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        var nomes = new AsnReader(extensao, AsnEncodingRules.BER).ReadSequence();
        while (nomes.HasData)
        {
            if (nomes.PeekTag() != outroNome)
            {
                nomes.ReadEncodedValue();
                continue;
            }
            var nome = nomes.ReadSequence(outroNome);
            var tipo = nome.ReadObjectIdentifier();
            var valor = LerTexto(nome.ReadSequence(outroNome));
            if (valor is null)
                continue;

            if (tipo == OidCnpj && cnpj is null && valor.Length == Cnpj.Tamanho && Cnpj.Valido(valor))
            {
                cnpj = valor;
            }
            else if (tipo == OidDadosPessoaFisica && cpf is null
                && valor.Length >= InicioCpfNosDadosPessoaFisica + Cpf.Tamanho)
            {
                var numero = valor.Substring(InicioCpfNosDadosPessoaFisica, Cpf.Tamanho);
                if (Cpf.Valido(numero))
                    cpf = numero;
            }
        }
        // O e-CNPJ é da pessoa jurídica, mesmo que traga também um CPF.
        if (cnpj is not null)
            cpf = null;
    }

    // DOC-ICP-04 7.1.2.2 a): OCTET STRING ou PRINTABLE STRING; os outros tipos de texto, por tolerância.
    private static string? LerTexto(AsnReader valor)
    {
        var tag = valor.PeekTag();
        if (tag.TagClass != TagClass.Universal)
            return null;
        return (UniversalTagNumber)tag.TagValue switch
        {
            UniversalTagNumber.OctetString => Encoding.ASCII.GetString(valor.ReadOctetString()),
            UniversalTagNumber.PrintableString => valor.ReadCharacterString(UniversalTagNumber.PrintableString),
            UniversalTagNumber.UTF8String => valor.ReadCharacterString(UniversalTagNumber.UTF8String),
            UniversalTagNumber.IA5String => valor.ReadCharacterString(UniversalTagNumber.IA5String),
            _ => null,
        };
    }

    private static void LerNomeDoTitular(X509Certificate2 certificado, out string? nomeComum, out string? numeroDeSerie)
    {
        nomeComum = null;
        numeroDeSerie = null;
        foreach (var rdn in certificado.SubjectName.EnumerateRelativeDistinguishedNames())
        {
            if (rdn.HasMultipleElements)
                continue;
            var tipo = rdn.GetSingleElementType().Value;
            if (tipo == OidNomeComum)
                nomeComum ??= rdn.GetSingleElementValue();
            else if (tipo == OidNumeroDeSerie)
                numeroDeSerie ??= rdn.GetSingleElementValue();
        }
    }

    // Até a versão 8.1 do DOC-ICP-04, o CN é "<nome>:<CNPJ ou CPF>"; da 8.2 em diante, só o nome.
    private static string Nome(string? nomeComum, string documento)
    {
        if (nomeComum is null)
            return string.Empty;
        var sufixo = ":" + documento;
        var nome = nomeComum.EndsWith(sufixo, StringComparison.Ordinal) ? nomeComum[..^sufixo.Length] : nomeComum;
        return nome.Trim();
    }
}
