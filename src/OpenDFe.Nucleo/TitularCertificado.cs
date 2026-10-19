namespace OpenDFe;

/// <summary>
/// O titular de um certificado digital: o nome e o CNPJ (e-CNPJ) ou o CPF (e-CPF), lidos por
/// <see cref="CertificadoDigital.LerTitular"/>. Exatamente um entre <see cref="Cnpj"/> e <see cref="Cpf"/>
/// está preenchido.
/// </summary>
public sealed class TitularCertificado
{
    internal TitularCertificado(string nome, string? cnpj, string? cpf)
    {
        Nome = nome;
        Cnpj = cnpj;
        Cpf = cpf;
    }

    /// <summary>
    /// O nome do titular, do <c>CN</c> do certificado: a razão social no e-CNPJ, o nome civil no e-CPF. Sem o
    /// sufixo <c>:&lt;CNPJ ou CPF&gt;</c> que os certificados anteriores à versão 8.2 do DOC-ICP-04 levam no
    /// <c>CN</c>; vazio se o certificado não tem <c>CN</c>.
    /// </summary>
    public string Nome { get; }

    /// <summary>O CNPJ do titular, sem máscara (talvez alfanumérico), ou <see langword="null"/> no e-CPF.</summary>
    public string? Cnpj { get; }

    /// <summary>O CPF do titular, sem máscara, ou <see langword="null"/> no e-CNPJ.</summary>
    public string? Cpf { get; }
}
