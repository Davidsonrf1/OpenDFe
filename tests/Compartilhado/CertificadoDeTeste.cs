using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OpenDFe.Testes;

/// <summary>Como o valor de cada <c>otherName</c> é codificado (DOC-ICP-04 7.1.2.2 a: um dos dois).</summary>
public enum CodificacaoOtherName
{
    OctetString,
    PrintableString,
}

/// <summary>
/// Gerador do certificado de teste (PROJETO.md 13.1): <b>autoassinado</b>, RSA 2048, com os campos que um
/// e-CNPJ ou e-CPF A1 da ICP-Brasil leva (DOC-ICP-04 8.3, 7.1.2.2 e Anexo I): no "Subject Alternative Name",
/// os <c>otherName</c> 2.16.76.1.3.3 (CNPJ), 2.16.76.1.3.4 (dados do responsável), 2.16.76.1.3.2 (nome do
/// responsável) e 2.16.76.1.3.7 (CEI) no e-CNPJ, e 2.16.76.1.3.1 (dados do titular) e 2.16.76.1.3.6 (CEI) no
/// e-CPF; o uso de chave para assinatura digital; e a validade configurável, inclusive vencido ou ainda não
/// válido. Gerado na hora, nunca gravado no repositório.
/// </summary>
/// <remarks>
/// Código dos testes, fora da API pública da lib (público só no assembly de teste, que não é pacote): cada projeto de teste o inclui como fonte
/// (<c>&lt;Compile Include="../Compartilhado/**/*.cs" /&gt;</c>). Os valores não são conferidos (um CNPJ com
/// o dígito errado entra como está), para os testes da leitura.
/// </remarks>
public sealed class CertificadoDeTeste
{
    private const string OidNomeAlternativo = "2.5.29.17";

    /// <summary>O nome civil (e-CPF) ou a razão social (e-CNPJ), no CN.</summary>
    public string Nome { get; init; } = "EMPRESA DE TESTE LTDA";

    /// <summary>O CNPJ do titular (e-CNPJ), no otherName 2.16.76.1.3.3; nulo no e-CPF.</summary>
    public string? Cnpj { get; init; }

    /// <summary>O CPF do titular (e-CPF), no otherName 2.16.76.1.3.1; nulo no e-CNPJ.</summary>
    public string? Cpf { get; init; }

    /// <summary>O CPF do responsável pelo e-CNPJ, no otherName 2.16.76.1.3.4.</summary>
    public string CpfResponsavel { get; init; } = "52998224725";

    /// <summary>O nome do responsável pelo e-CNPJ, no otherName 2.16.76.1.3.2.</summary>
    public string NomeResponsavel { get; init; } = "RESPONSAVEL DE TESTE";

    /// <summary>A data de nascimento do titular (e-CPF) ou do responsável (e-CNPJ).</summary>
    public DateOnly Nascimento { get; init; } = new(1980, 1, 15);

    /// <summary>Se o "Subject Alternative Name" leva os otherName da ICP-Brasil.</summary>
    public bool ComOtherName { get; init; } = true;

    /// <summary>Como o valor de cada otherName é codificado.</summary>
    public CodificacaoOtherName Codificacao { get; init; } = CodificacaoOtherName.OctetString;

    /// <summary>
    /// Perfil anterior à versão 8.2 do DOC-ICP-04: o CN é <c>&lt;nome&gt;:&lt;CNPJ ou CPF&gt;</c>. Falso: o CN
    /// é só o nome.
    /// </summary>
    public bool DocumentoNoNomeComum { get; init; } = true;

    /// <summary>Perfil da versão 8.2 em diante: o <c>serialNumber</c> (2.5.4.5) do titular com o CNPJ ou CPF.</summary>
    public bool DocumentoNoNumeroDeSerie { get; init; }

    /// <summary>Início da validade; o padrão é um dia antes de agora.</summary>
    public DateTimeOffset NaoAntes { get; init; } = DateTimeOffset.UtcNow.AddDays(-1);

    /// <summary>Fim da validade; o padrão é um ano depois de agora.</summary>
    public DateTimeOffset NaoDepois { get; init; } = DateTimeOffset.UtcNow.AddYears(1);

    /// <summary>Chave ECDSA P-256 em vez de RSA 2048 (para o teste da recusa).</summary>
    public bool ChaveEcdsa { get; init; }

    /// <summary>O certificado com a chave privada. É de quem chama (descarte).</summary>
    public X509Certificate2 Gerar()
    {
        var titular = NomeDoTitular();
        if (ChaveEcdsa)
        {
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var pedidoEc = new CertificateRequest(titular, ecdsa, HashAlgorithmName.SHA256);
            AcrescentarExtensoes(pedidoEc);
            return pedidoEc.CreateSelfSigned(NaoAntes, NaoDepois);
        }

        using var rsa = RSA.Create(2048);
        var pedido = new CertificateRequest(titular, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        AcrescentarExtensoes(pedido);
        return pedido.CreateSelfSigned(NaoAntes, NaoDepois);
    }

    /// <summary>O PFX com a chave privada, protegido pela senha (a cifra padrão do <c>Export</c>).</summary>
    public byte[] GerarPfx(string senha)
    {
        using var certificado = Gerar();
        return certificado.Export(X509ContentType.Pkcs12, senha);
    }

    /// <summary>O PFX só com o certificado, sem a chave privada.</summary>
    public byte[] GerarPfxSemChavePrivada(string senha)
    {
        using var certificado = Gerar();
        using var publico = X509CertificateLoader.LoadCertificate(certificado.RawData);
        return publico.Export(X509ContentType.Pkcs12, senha);
    }

    private X500DistinguishedName NomeDoTitular()
    {
        var documento = Cnpj ?? Cpf;
        var nome = new X500DistinguishedNameBuilder();
        nome.AddCountryOrRegion("BR");
        nome.AddOrganizationName("ICP-Brasil");
        nome.AddOrganizationalUnitName("Certificado de teste autoassinado");
        nome.AddCommonName(DocumentoNoNomeComum && documento is not null ? $"{Nome}:{documento}" : Nome);
        if (DocumentoNoNumeroDeSerie && documento is not null)
            nome.Add("2.5.4.5", documento, UniversalTagNumber.PrintableString);
        return nome.Build();
    }

    private void AcrescentarExtensoes(CertificateRequest pedido)
    {
        pedido.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        pedido.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyEncipherment,
            critical: true));
        pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            [new Oid("1.3.6.1.5.5.7.3.2"), new Oid("1.3.6.1.5.5.7.3.4")], critical: false));
        pedido.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(pedido.PublicKey, critical: false));
        pedido.CertificateExtensions.Add(new X509Extension(OidNomeAlternativo, NomeAlternativo(), critical: false));
    }

    // GeneralNames com os otherName da ICP-Brasil e um rfc822Name (que a leitura tem de pular).
    private byte[] NomeAlternativo()
    {
        var escritor = new AsnWriter(AsnEncodingRules.DER);
        using (escritor.PushSequence())
        {
            escritor.WriteCharacterString(UniversalTagNumber.IA5String, "teste@exemplo.com.br",
                new Asn1Tag(TagClass.ContextSpecific, 1));
            if (ComOtherName)
            {
                foreach (var (oid, valor) in OutrosNomes())
                    EscreverOutroNome(escritor, oid, valor);
            }
        }
        return escritor.Encode();
    }

    private IEnumerable<(string Oid, string Valor)> OutrosNomes()
    {
        var nascimento = Nascimento.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
        const string Nis = "00000000000";
        const string Rg = "000000000000000";
        const string Cei = "000000000000";
        if (Cnpj is not null)
        {
            yield return ("2.16.76.1.3.4", nascimento + CpfResponsavel + Nis + Rg);
            yield return ("2.16.76.1.3.2", NomeResponsavel);
            yield return ("2.16.76.1.3.3", Cnpj);
            yield return ("2.16.76.1.3.7", Cei);
        }
        else if (Cpf is not null)
        {
            yield return ("2.16.76.1.3.1", nascimento + Cpf + Nis + Rg);
            yield return ("2.16.76.1.3.6", Cei);
        }
    }

    private void EscreverOutroNome(AsnWriter escritor, string oid, string valor)
    {
        var contexto0 = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        using (escritor.PushSequence(contexto0))
        {
            escritor.WriteObjectIdentifier(oid);
            using (escritor.PushSequence(contexto0))
            {
                if (Codificacao == CodificacaoOtherName.OctetString)
                    escritor.WriteOctetString(Encoding.ASCII.GetBytes(valor));
                else
                    escritor.WriteCharacterString(UniversalTagNumber.PrintableString, valor);
            }
        }
    }
}
