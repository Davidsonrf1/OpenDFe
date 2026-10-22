namespace OpenDFe.NFe;

/// <summary>
/// As regras do MOC e das NTs que o leiaute 4.00 confere antes de transmitir (PROJETO.md 6.2, REGRAS.md 4), declaradas ao
/// núcleo por <see cref="DeclaracaoLeiaute.DeclararRegras"/>. O catálogo (docs/REGRAS.md) marca cada uma como "lib".
/// </summary>
/// <remarks>
/// O núcleo só as chama com o schema sem erro; mesmo assim, as regras leem o modelo, e o campo nulo é "não se aplica" (o
/// schema já apontou o obrigatório). Sem estado e seguro em paralelo.
/// </remarks>
internal static partial class RegrasNFe400
{
    /// <summary>
    /// As regras: o lote da etapa 3.2 (REGRAS.md 11), o CPF e o CNPJ, o DV das chaves referenciadas, os totais, as
    /// combinações da operação e as datas; e as da NFC-e, da etapa 3.3 (REGRAS.md 12).
    /// </summary>
    internal static IReadOnlyList<RegraNFe> Lista { get; } = [.. Documentos(), .. Totais(), .. Operacao(), .. NFCe()];

    /// <summary>A função declarada ao núcleo: as regras da <see cref="Lista"/> sobre o documento.</summary>
    internal static IEnumerable<Problema> Conferir(DocumentoVerificado documento, ContextoOperacao contexto)
    {
        var nucleo = ValoresNucleo.Criar(contexto);
        return Conferir(Lista, documento.Modelo, documento.Xml, new ContextoRegra(nucleo, documento.Chave, contexto.Configuracao.Relogio.GetUtcNow()));
    }

    /// <summary>
    /// As regras sobre a nota (o <paramref name="modelo"/>, ou, nulo, o <paramref name="xml"/> lido), só as do modelo do
    /// núcleo, na ordem da lista.
    /// </summary>
    /// <exception cref="InvalidOperationException">O modelo não é uma <see cref="NotaFiscal"/>.</exception>
    internal static List<Problema> Conferir(IReadOnlyList<RegraNFe> regras, object? modelo, string xml, ContextoRegra contexto)
    {
        var nota = modelo switch
        {
            NotaFiscal n => n,
            null => LeitorNFe.Ler(xml).Nota,
            _ => throw new InvalidOperationException($"O modelo do documento é {modelo.GetType().Name}, não NotaFiscal."),
        };
        var problemas = new List<Problema>();
        if (nota is null)
            return problemas;

        foreach (var regra in regras)
        {
            if (!regra.Modelos.Contains(contexto.Nucleo.Documento))
                continue;
            foreach (var ocorrencia in regra.Conferir(nota, contexto))
                problemas.Add(Problema(regra, ocorrencia));
        }
        return problemas;
    }

    private static Problema Problema(RegraNFe regra, OcorrenciaRegra ocorrencia)
    {
        var mensagem = ocorrencia.Detalhe is null ? regra.Mensagem : $"{regra.Mensagem} ({ocorrencia.Detalhe})";
        var caminho = ocorrencia.Caminho;
        return new Problema(regra.Codigo, regra.Severidade, mensagem)
        {
            Tag = caminho is null ? null : Tag(caminho),
            Caminho = caminho,
            Propriedade = ocorrencia.Propriedade ?? (caminho is null ? null : CaminhoModeloNFe.Propriedade(caminho)),
        };
    }

    // O último passo do caminho, sem o índice e sem o @ do atributo.
    private static string Tag(string caminho)
    {
        var passo = caminho[(caminho.LastIndexOf('/') + 1)..].TrimStart('@');
        var colchete = passo.IndexOf('[', StringComparison.Ordinal);
        return colchete < 0 ? passo : passo[..colchete];
    }
}
