namespace OpenDFe.NFSe;

/// <summary>
/// As regras fora do schema que o leiaute 1.01 confere na DPS antes de transmitir (docs/NFSE.md 4, aprovadas em
/// 2026-11-19), declaradas ao núcleo por <see cref="DeclaracaoLeiaute.DeclararRegras"/>. O catálogo é o
/// <c>docs/REGRAS.md</c> 20.
/// </summary>
/// <remarks>
/// <para>
/// Cinco das seis aprovadas: a <c>NFSE-CHAVE</c> caiu, porque a etapa 9.1 não confirmou o cálculo do dígito verificador da
/// chave da NFS-e (<see cref="ChaveNFSe"/>), como a 9.0 previu. O código do problema é o local (<c>NFSE-...</c>), com a
/// rejeição da Sefin que ele antecipa na mensagem; todas são erro.
/// </para>
/// <para>
/// O núcleo só as chama com o schema sem erro; mesmo assim, o campo nulo é "não se aplica" (o schema já apontou o
/// obrigatório). Sem estado e seguro em paralelo.
/// </para>
/// </remarks>
internal static class RegrasNFSe101
{
    /// <summary>A tolerância do relógio na emissão, a mesma da regra de datas da NF-e (REGRAS.md, B09-10).</summary>
    internal static TimeSpan ToleranciaRelogio => TimeSpan.FromMinutes(5);

    /// <summary>As regras, na ordem do catálogo.</summary>
    internal static IReadOnlyList<RegraNFSe> Lista { get; } =
    [
        new("NFSE-DOCUMENTO", "O CPF ou o CNPJ não é válido (rejeições E0082, E0098, E0190, E0207 e E0250)", Documentos),
        new("NFSE-COMPETENCIA", "A data da competência é posterior à data da emissão (rejeição E0015)", Competencia),
        new("NFSE-EMISSAO", "A data da emissão é posterior à data e à hora atuais (rejeição E0008)", Emissao),
        new("NFSE-EMITENTE", "A Sefin Nacional só aceita a DPS emitida pelo prestador (rejeição E9996)", Emitente),
        new("NFSE-PRESTADOR", "Na DPS emitida pelo prestador, o nome, o endereço, o NIF e o motivo da ausência do NIF do prestador ficam de fora: o ambiente nacional os tira do cadastro (rejeições E0121, E0128, E0112 e E0114)", Prestador),
    ];

    /// <summary>A função declarada ao núcleo: as regras sobre a DPS do documento (o modelo, ou o XML lido).</summary>
    internal static IEnumerable<Problema> Conferir(DocumentoVerificado documento, ContextoOperacao contexto)
    {
        var dps = documento.Modelo switch
        {
            Dps d => d,
            null => LeitorNFSe.Ler(documento.Xml).Dps,
            _ => null,   // o evento não é DPS: as regras da DPS não se aplicam
        };
        return dps is null ? [] : Conferir(dps, contexto.Configuracao.Relogio.GetUtcNow());
    }

    /// <summary>As regras sobre a <paramref name="dps"/>, com o relógio em <paramref name="agora"/>.</summary>
    internal static List<Problema> Conferir(Dps dps, DateTimeOffset agora)
    {
        var problemas = new List<Problema>();
        foreach (var regra in Lista)
            foreach (var (caminho, propriedade, detalhe) in regra.Conferir(dps, agora))
                problemas.Add(new Problema(regra.Codigo, Severidade.Erro, detalhe is null ? regra.Mensagem : $"{regra.Mensagem} ({detalhe})")
                {
                    Tag = caminho[(caminho.LastIndexOf('/') + 1)..],
                    Caminho = caminho,
                    Propriedade = propriedade,
                });
        return problemas;
    }

    private static IEnumerable<Ocorrencia> Documentos(Dps dps, DateTimeOffset agora)
    {
        foreach (var (grupo, tag, cnpj, cpf, propriedade) in new[]
        {
            ("prest", "prestador", dps.Prestador?.Cnpj, dps.Prestador?.Cpf, "Prestador"),
            ("toma", "tomador", dps.Tomador?.Cnpj, dps.Tomador?.Cpf, "Tomador"),
            ("interm", "intermediário", dps.Intermediario?.Cnpj, dps.Intermediario?.Cpf, "Intermediario"),
        })
        {
            if (!string.IsNullOrEmpty(cnpj) && !Cnpj.Valido(cnpj))
                yield return new($"DPS/infDPS/{grupo}/CNPJ", propriedade + ".Cnpj", $"o CNPJ do {tag}");
            if (!string.IsNullOrEmpty(cpf) && !Cpf.Valido(cpf))
                yield return new($"DPS/infDPS/{grupo}/CPF", propriedade + ".Cpf", $"o CPF do {tag}");
        }
    }

    private static IEnumerable<Ocorrencia> Competencia(Dps dps, DateTimeOffset agora)
    {
        if (dps.Competencia is { } competencia && dps.Emissao is { } emissao && competencia > DateOnly.FromDateTime(emissao.DateTime))
            yield return new("DPS/infDPS/dCompet", "Competencia", null);
    }

    private static IEnumerable<Ocorrencia> Emissao(Dps dps, DateTimeOffset agora)
    {
        if (dps.Emissao is { } emissao && emissao > agora + ToleranciaRelogio)
            yield return new("DPS/infDPS/dhEmi", "Emissao", null);
    }

    private static IEnumerable<Ocorrencia> Emitente(Dps dps, DateTimeOffset agora)
    {
        if (dps.Emitente is { } emitente && emitente != EmitenteDps.Prestador)
            yield return new("DPS/infDPS/tpEmit", "Emitente", null);
    }

    private static IEnumerable<Ocorrencia> Prestador(Dps dps, DateTimeOffset agora)
    {
        if (dps.Emitente != EmitenteDps.Prestador || dps.Prestador is not { } prestador)
            yield break;
        if (!string.IsNullOrEmpty(prestador.Nome))
            yield return new("DPS/infDPS/prest/xNome", "Prestador.Nome", "o nome");
        if (prestador.Endereco is not null)
            yield return new("DPS/infDPS/prest/end", "Prestador.Endereco", "o endereço");
        if (!string.IsNullOrEmpty(prestador.Nif))
            yield return new("DPS/infDPS/prest/NIF", "Prestador.Nif", "o NIF");
        if (prestador.MotivoAusenciaNif is not null)
            yield return new("DPS/infDPS/prest/cNaoNIF", "Prestador.MotivoAusenciaNif", "o motivo da ausência do NIF");
    }
}

/// <summary>Uma regra fora do schema da DPS: o código local, a mensagem e a conferência.</summary>
internal sealed record RegraNFSe(string Codigo, string Mensagem, Func<Dps, DateTimeOffset, IEnumerable<Ocorrencia>> Conferir);

/// <summary>Onde a regra achou o problema: o caminho no XML, a propriedade e o detalhe da mensagem.</summary>
internal sealed record Ocorrencia(string Caminho, string Propriedade, string? Detalhe);
