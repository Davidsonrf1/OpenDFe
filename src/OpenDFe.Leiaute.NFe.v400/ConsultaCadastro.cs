namespace OpenDFe.NFe;

/// <summary>
/// A consulta ao cadastro de contribuintes da UF (<c>CadConsultaCadastro4</c>, MOC 7.0, Visão Geral, 5.6, docs/CONTRATOS.md
/// 24): o <c>ConsCad</c> 2.00 por CNPJ, por CPF ou por inscrição estadual e o <c>retConsCad</c> com os contribuintes
/// achados (<see cref="RetornoCadastro"/>). Só na NF-e.
/// </summary>
/// <remarks>
/// <para>
/// O pedido não é assinado e não leva o ambiente. Vai ao autorizador da UF consultada (<see cref="UF"/>), no atendimento
/// normal, qualquer que seja a UF do núcleo. Só atendem o serviço as 10 UFs com autorizador próprio de NF-e (AM, BA, GO, MG,
/// MS, MT, PE, PR, RS e SP) e, na SVRS, AC, ES, RN, PB e SC: a UF que não atende (a SVAN do MA e as outras da SVRS) é
/// recusada antes de transmitir (<c>NFE-CADASTRO</c>), a menos que o <see cref="ParametroWS.Destino"/> seja informado (quem
/// escolhe o destino sabe o que faz). A SUFRAMA (<c>SU</c>) não é uma UF e não existe aqui.
/// </para>
/// <para>
/// O certificado do núcleo é o do solicitante: o transmissor irregular é recusado pela SEFAZ (257). A consulta é só da
/// NF-e: a NFC-e não a registra nesta versão (o PDV não consulta cadastro).
/// </para>
/// </remarks>
public sealed class ConsultaCadastro : ParametroWS<RetornoCadastro>
{
    private const int MenorInscricao = 2;
    private const int MaiorInscricao = 14;

    private ConsultaCadastro(UF uf, string? documento, string? inscricaoEstadual)
    {
        if (!Enum.IsDefined(uf))
            throw new ArgumentOutOfRangeException(nameof(uf), uf, "O valor não é uma UF.");
        UF = uf;
        Documento = documento;
        InscricaoEstadual = inscricaoEstadual;
    }

    /// <summary>A consulta pelo CNPJ ou pelo CPF.</summary>
    /// <param name="uf">A UF consultada (o cadastro é o dela).</param>
    /// <param name="documento">O CNPJ (numérico ou alfanumérico) ou o CPF, com ou sem máscara; o dígito verificador é conferido.</param>
    /// <exception cref="ArgumentNullException">O documento é nulo.</exception>
    /// <exception cref="ArgumentException">O documento não é um CNPJ nem um CPF válido.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A UF não é conhecida.</exception>
    public ConsultaCadastro(UF uf, string documento)
        : this(uf, Normalizar(documento), null)
    {
    }

    /// <summary>A consulta pela inscrição estadual.</summary>
    /// <param name="uf">A UF consultada (a inscrição é dela).</param>
    /// <param name="inscricaoEstadual">
    /// De 2 a 14 algarismos, sem todos zero e sem máscara; o dígito verificador é de cada UF e a lib não o confere.
    /// </param>
    /// <returns>A consulta.</returns>
    /// <exception cref="ArgumentNullException">A inscrição é nula.</exception>
    /// <exception cref="ArgumentException">A inscrição não tem de 2 a 14 algarismos, é toda zero ou é <c>ISENTO</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A UF não é conhecida.</exception>
    public static ConsultaCadastro Inscricao(UF uf, string inscricaoEstadual)
    {
        ArgumentNullException.ThrowIfNull(inscricaoEstadual);
        if (inscricaoEstadual.Length is < MenorInscricao or > MaiorInscricao || !inscricaoEstadual.All(char.IsAsciiDigit)
            || inscricaoEstadual.All(c => c == '0'))
            throw new ArgumentException("A inscrição estadual tem de 2 a 14 algarismos, sem todos zero.", nameof(inscricaoEstadual));
        return new ConsultaCadastro(uf, null, inscricaoEstadual);
    }

    /// <summary>A UF consultada (<c>UF</c>).</summary>
    public UF UF { get; }

    /// <summary>O CNPJ ou o CPF consultado, sem máscara; nulo na consulta pela inscrição.</summary>
    public string? Documento { get; }

    /// <summary>A inscrição estadual consultada; nula na consulta pelo CNPJ ou pelo CPF.</summary>
    public string? InscricaoEstadual { get; }

    private static string Normalizar(string documento)
    {
        ArgumentNullException.ThrowIfNull(documento);
        if (Cnpj.Valido(documento))
            return Cnpj.RemoverMascara(documento);
        if (Cpf.Valido(documento))
            return Cpf.RemoverMascara(documento);
        throw new ArgumentException("O documento não é um CNPJ nem um CPF válido.", nameof(documento));
    }
}
