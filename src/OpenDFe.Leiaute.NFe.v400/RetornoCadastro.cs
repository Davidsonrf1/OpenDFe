namespace OpenDFe.NFe;

/// <summary>
/// O retorno da <see cref="ConsultaCadastro"/> (o <c>retConsCad</c> 2.00, MOC 7.0, Visão Geral, 5.6, Tabela 5-22,
/// docs/CONTRATOS.md 24): o status e os contribuintes achados. Imutável.
/// </summary>
public sealed class RetornoCadastro
{
    internal RetornoCadastro(int status, string motivo, IReadOnlyList<ContribuinteCadastro> contribuintes)
    {
        Status = status;
        Motivo = motivo;
        Contribuintes = contribuintes;
    }

    /// <summary>O <c>cStat</c>: 111 (uma ocorrência), 112 (mais de uma), 257 a 265 (as rejeições) ou o do serviço (108, 109).</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c>.</summary>
    public string Motivo { get; }

    /// <summary>O <c>verAplic</c>: a versão do Web Service que processou a consulta.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>A <c>UF</c> consultada, como o retorno a devolve; nula quando não veio ou não é uma UF (a <c>SU</c> da SUFRAMA).</summary>
    public UF? UF { get; internal init; }

    /// <summary>O <c>cUF</c>: a UF do Web Service que atendeu a consulta; nula quando não veio ou não se leu.</summary>
    public UF? UFAtendimento { get; internal init; }

    /// <summary>O <c>dhCons</c>: a data e a hora da consulta; nula quando não veio ou não se leu.</summary>
    public DateTimeOffset? Consulta { get; internal init; }

    /// <summary>Os contribuintes achados (<c>infCad</c>), na ordem do retorno; vazia sem ocorrência.</summary>
    public IReadOnlyList<ContribuinteCadastro> Contribuintes { get; }

    /// <summary>Achou: <see cref="Status"/> 111 (uma ocorrência) ou 112 (mais de uma).</summary>
    public bool Encontrado => Status is 111 or 112;
}

/// <summary>
/// Um contribuinte do cadastro da UF (o <c>infCad</c>, grupos GR07 a GR29 da Tabela 5-22). Imutável. Tem dados do
/// contribuinte: guardar com cuidado, nunca em log.
/// </summary>
public sealed class ContribuinteCadastro
{
    internal ContribuinteCadastro(string inscricaoEstadual, string nome)
    {
        InscricaoEstadual = inscricaoEstadual;
        Nome = nome;
    }

    /// <summary>A inscrição estadual (<c>IE</c>): 2 a 14 algarismos ou <c>ISENTO</c>.</summary>
    public string InscricaoEstadual { get; }

    /// <summary>O CNPJ (<c>CNPJ</c>); nulo quando o contribuinte é pessoa física (<see cref="Cpf"/>).</summary>
    public string? Cnpj { get; internal init; }

    /// <summary>O CPF (<c>CPF</c>); nulo quando o contribuinte é pessoa jurídica (<see cref="Cnpj"/>).</summary>
    public string? Cpf { get; internal init; }

    /// <summary>
    /// A UF de localização (<c>UF</c>), que em algumas situações é diferente da consultada (a inscrição de substituto
    /// tributário, por exemplo); nula quando não veio ou não se leu.
    /// </summary>
    public UF? UF { get; internal init; }

    /// <summary>A situação cadastral (<c>cSit</c>); nula quando não veio ou não é um número.</summary>
    public SituacaoCadastral? Situacao { get; internal init; }

    /// <summary>O credenciamento para emitir NF-e na SEFAZ consultada (<c>indCredNFe</c>); nulo quando não veio ou não é um número.</summary>
    public CredenciamentoDFe? CredenciamentoNFe { get; internal init; }

    /// <summary>O credenciamento para emitir CT-e na SEFAZ consultada (<c>indCredCTe</c>); nulo quando não veio ou não é um número.</summary>
    public CredenciamentoDFe? CredenciamentoCTe { get; internal init; }

    /// <summary>A razão social ou o nome (<c>xNome</c>); vazia quando não veio.</summary>
    public string Nome { get; }

    /// <summary>O nome fantasia (<c>xFant</c>).</summary>
    public string? Fantasia { get; internal init; }

    /// <summary>O regime de apuração do ICMS (<c>xRegApur</c>), como o texto da SEFAZ.</summary>
    public string? RegimeApuracao { get; internal init; }

    /// <summary>O CNAE fiscal (<c>CNAE</c>), de 6 ou 7 algarismos.</summary>
    public string? Cnae { get; internal init; }

    /// <summary>A data de início das atividades (<c>dIniAtiv</c>).</summary>
    public DateOnly? InicioAtividade { get; internal init; }

    /// <summary>A data da última modificação da situação cadastral (<c>dUltSit</c>).</summary>
    public DateOnly? UltimaSituacao { get; internal init; }

    /// <summary>A data da baixa do contribuinte (<c>dBaixa</c>).</summary>
    public DateOnly? Baixa { get; internal init; }

    /// <summary>A inscrição estadual única (<c>IEUnica</c>).</summary>
    public string? InscricaoUnica { get; internal init; }

    /// <summary>A inscrição estadual atual (<c>IEAtual</c>).</summary>
    public string? InscricaoAtual { get; internal init; }

    /// <summary>O endereço (<c>ender</c>); nulo quando o cadastro não o traz.</summary>
    public EnderecoCadastro? Endereco { get; internal init; }
}

/// <summary>O endereço de um contribuinte do cadastro (o <c>ender</c>, <c>TEndereco</c>). Imutável; todos os campos são opcionais.</summary>
public sealed class EnderecoCadastro
{
    internal EnderecoCadastro()
    {
    }

    /// <summary>O logradouro (<c>xLgr</c>).</summary>
    public string? Logradouro { get; internal init; }

    /// <summary>O número (<c>nro</c>).</summary>
    public string? Numero { get; internal init; }

    /// <summary>O complemento (<c>xCpl</c>).</summary>
    public string? Complemento { get; internal init; }

    /// <summary>O bairro (<c>xBairro</c>).</summary>
    public string? Bairro { get; internal init; }

    /// <summary>O código IBGE do município (<c>cMun</c>), de 7 algarismos.</summary>
    public string? CodigoMunicipio { get; internal init; }

    /// <summary>O nome do município (<c>xMun</c>).</summary>
    public string? Municipio { get; internal init; }

    /// <summary>O CEP (<c>CEP</c>), de 7 ou 8 algarismos.</summary>
    public string? Cep { get; internal init; }
}

/// <summary>
/// A situação cadastral do contribuinte (<c>cSit</c>, MOC 7.0, Tabela 5-22, GR10). Aberta: um valor que o MOC não conhece
/// entra como o número, com aviso <c>NFE-VALOR</c>.
/// </summary>
public enum SituacaoCadastral
{
    /// <summary>Não habilitado (0).</summary>
    NaoHabilitado = 0,

    /// <summary>Habilitado (1).</summary>
    Habilitado = 1,
}

/// <summary>
/// O credenciamento do contribuinte para emitir NF-e ou CT-e na SEFAZ consultada (<c>indCredNFe</c> e <c>indCredCTe</c>, MOC
/// 7.0, Tabela 5-22, GR11 e GR12). Significa só que o contribuinte é credenciado nessa SEFAZ. Aberta: um valor que o MOC não
/// conhece entra como o número, com aviso <c>NFE-VALOR</c>.
/// </summary>
public enum CredenciamentoDFe
{
    /// <summary>Não credenciado para a emissão (0).</summary>
    NaoCredenciado = 0,

    /// <summary>Credenciado (1).</summary>
    Credenciado = 1,

    /// <summary>Credenciado com obrigatoriedade para todas as operações (2).</summary>
    ObrigatorioTotal = 2,

    /// <summary>Credenciado com obrigatoriedade parcial (3).</summary>
    ObrigatorioParcial = 3,

    /// <summary>A SEFAZ não fornece a informação (4).</summary>
    NaoInformado = 4,
}
