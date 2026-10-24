using OpenDFe.Impressao;

namespace OpenDFe.NFe;

/// <summary>
/// Monta o DANFE da NF-e (<see cref="DocumentoImpressao"/>, A4 retrato ou paisagem) a partir da NF-e lida, quadro a quadro na
/// ordem do Anexo II do MOC 7.0 (docs/IMPRESSAO.md 2), com os acréscimos da NT 2026.010 (IBS, CBS e IS). Só lê a nota: não
/// confere regra do MOC nem o schema, e não imprime o que o XML não tem (Anexo II 3.1).
/// </summary>
/// <remarks>Um arquivo por grupo de quadros: a identificação, as partes, os totais, o transporte, os itens e os adicionais.</remarks>
internal static partial class MontagemDanfe
{
    private const string TextoHomologacao = "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
    private const string MarcaCancelada = "CANCELADA";
    private const string MarcaHomologacao = "SEM VALOR FISCAL";

    internal static DocumentoImpressao Montar(NotaLida lida, OpcoesDanfe? opcoes)
    {
        ArgumentNullException.ThrowIfNull(lida);
        opcoes ??= new OpcoesDanfe();
        if (opcoes.Orientacao is { } escolhida && !Enum.IsDefined(escolhida))
            throw new ArgumentException("A orientação do DANFE não é uma das definidas.", nameof(opcoes));

        var nota = lida.Nota ?? throw new ArgumentException("A leitura não achou a nota: não há o que imprimir.", nameof(lida));
        var chave = lida.Chave ?? throw new ArgumentException("A nota não tem a chave de acesso (Id do infNFe) válida.", nameof(lida));
        var ide = nota.Identificacao;
        if ((ide.Modelo ?? chave.Modelo) != Documento.NFe)
            throw new ArgumentException("O DANFE é da NF-e (modelo 55); a nota não é (a NFC-e tem o DANFCE).", nameof(lida));

        var protocolo = lida.Protocolo;
        var autorizada = !string.IsNullOrWhiteSpace(protocolo?.Numero);
        if (autorizada && protocolo!.Status is { } status && status is not (100 or 150))
            throw new ArgumentException($"O protocolo da nota é de situação {status}: o DANFE só se imprime da NF-e autorizada (100 ou 150).", nameof(lida));

        var formato = ide.FormatoImpressao;
        switch (formato)
        {
            case FormatoImpressao.Nenhum:
                throw new ArgumentException("O formato de impressão da nota (tpImp) é 0, sem geração de DANFE.", nameof(lida));
            case FormatoImpressao.DanfeSimplificado:
                throw new ArgumentException(
                    "O DANFE Simplificado (tpImp 3) não está implementado: o Anexo II só fixa os campos mínimos, sem ordem nem dimensões (docs/IMPRESSAO.md 2.3).",
                    nameof(lida));
            case FormatoImpressao.DanfeNFCe or FormatoImpressao.DanfeNFCeMensagem:
                throw new ArgumentException("O formato de impressão da nota (tpImp 4 ou 5) é o do DANFE NFC-e, que não é da NF-e.", nameof(lida));
            case FormatoImpressao.DanfeSimplificadoTipo2:
                if (opcoes.Orientacao is not null)
                    throw new ArgumentException("O DANFE Simplificado Tipo 2 não tem orientação: ela é do retrato e da paisagem.", nameof(opcoes));
                return MontagemDanfce.MontarTipo2(lida, nota, chave, opcoes);
        }

        if (opcoes.Papel is not null)
            throw new ArgumentException("O papel das opções é do DANFE Simplificado Tipo 2; o retrato e a paisagem são o A4 da orientação.", nameof(opcoes));
        var orientacao = opcoes.Orientacao ?? formato switch
        {
            FormatoImpressao.DanfeRetrato => OrientacaoDanfe.Retrato,
            FormatoImpressao.DanfePaisagem => OrientacaoDanfe.Paisagem,
            _ => throw new ArgumentException(
                "A nota não informa o formato de impressão (tpImp) e as opções não informam a orientação.", nameof(lida)),
        };

        var paisagem = orientacao == OrientacaoDanfe.Paisagem;
        var papel = paisagem ? PapelImpressao.A4Paisagem : PapelImpressao.A4Retrato;
        var cabecalho = Identificacao(nota, chave, protocolo, autorizada, opcoes);

        var elementos = new List<ElementoImpressao>();
        // O canhoto só se suprime no retrato (Anexo II 3.3.1); na paisagem o renderizador o põe na vertical, à esquerda.
        if (opcoes.Canhoto || paisagem)
            elementos.AddRange(Canhoto(nota, chave));
        elementos.AddRange(cabecalho);
        elementos.Add(Destinatario(nota));
        if (nota.Retirada is { } retirada)
            elementos.Add(Local("INFORMAÇÕES DO LOCAL DE RETIRADA", retirada));
        if (nota.Entrega is { } entrega)
            elementos.Add(Local("INFORMAÇÕES DO LOCAL DE ENTREGA", entrega));
        if (Cobranca(nota) is { } cobranca)
            elementos.Add(cobranca);
        elementos.Add(Imposto(nota));
        if (TotalReforma(nota) is { } reforma)
            elementos.Add(reforma);
        elementos.Add(Transporte(nota));
        elementos.Add(Itens(nota));
        if (Issqn(nota) is { } issqn)
            elementos.Add(issqn);
        elementos.Add(Adicionais(nota));
        if (!string.IsNullOrEmpty(nota.Suplementares?.QrCode))
            elementos.Add(new QrCodeImpressao(nota.Suplementares.QrCode));
        if (!string.IsNullOrWhiteSpace(opcoes.TextoRodape))
            elementos.Add(new TextoImpressao(opcoes.TextoRodape) { Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno });

        var marca = opcoes.Cancelada ? MarcaCancelada : ide.Ambiente == Ambiente.Homologacao ? MarcaHomologacao : null;
        return new DocumentoImpressao(papel, elementos)
        {
            Titulo = $"DANFE NF-e {FormatadorImpressao.Zeros(ide.Numero ?? 0, 9)} série {FormatadorImpressao.Zeros(ide.Serie ?? 0, 3)}",
            CabecalhoContinuacao = cabecalho,
            MarcaAgua = marca,
        };
    }

    private static CampoImpressao Campo(string rotulo, string? valor, double peso = 1, bool negrito = false,
        AlinhamentoImpressao alinhamento = AlinhamentoImpressao.Esquerda) =>
        new(rotulo, valor ?? string.Empty, peso) { Negrito = negrito, Alinhamento = alinhamento };

    private static CampoImpressao Valor(string rotulo, decimal? valor, double peso = 1, bool negrito = false) =>
        Campo(rotulo, Dinheiro(valor), peso, negrito, AlinhamentoImpressao.Direita);

    private static LinhaCamposImpressao Linha(params CampoImpressao[] campos) => new(campos);

    private static SecaoImpressao Secao(string? titulo, params LinhaCamposImpressao[] linhas) => new(titulo, linhas);

    private static string Dinheiro(decimal? valor) => valor is { } v ? FormatadorImpressao.Dinheiro(v) : string.Empty;

    private static string Vazio(string? texto) => texto ?? string.Empty;

    // Só as partes que existem, separadas pelo separador dado: o endereço sem vírgulas sobrando.
    private static string Juntar(string separador, params string?[] partes) =>
        string.Join(separador, partes.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static string DocumentoPessoa(string? cnpj, string? cpf) =>
        !string.IsNullOrEmpty(cnpj) ? FormatadorImpressao.Cnpj(cnpj) : !string.IsNullOrEmpty(cpf) ? FormatadorImpressao.Cpf(cpf) : string.Empty;

    private static string Sigla(UF? uf, bool exterior) => exterior ? "EX" : uf?.Sigla() ?? string.Empty;
}
