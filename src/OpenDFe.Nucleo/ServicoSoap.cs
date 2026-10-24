namespace OpenDFe;

/// <summary>
/// O que o envelope SOAP 1.2 de um serviço leva (PROJETO.md 8.2): o namespace do WSDL, que é o do
/// <c>nfeDadosMsg</c> e do <c>nfeResultMsg</c>, a ação, no <c>Content-Type</c>, e, no serviço que a tem no <c>Body</c>, a
/// operação (<see cref="Operacao"/>). O leiaute declara um por serviço
/// (<see cref="DeclaracaoLeiaute.DeclararServico"/>); o núcleo não conhece os serviços de nenhum documento.
/// </summary>
public sealed class ServicoSoap
{
    /// <summary>Cria a declaração de um serviço.</summary>
    /// <param name="nome">O nome do serviço, o mesmo da tabela de URLs (<c>"NFeStatusServico4"</c>).</param>
    /// <param name="namespaceWsdl">
    /// O namespace do WSDL (<c>"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4"</c>).
    /// </param>
    /// <param name="acao">
    /// A ação SOAP (<c>"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4/nfeStatusServicoNF"</c>).
    /// </param>
    /// <exception cref="ArgumentException">Um dos valores é nulo, vazio ou só espaços.</exception>
    public ServicoSoap(string nome, string namespaceWsdl, string acao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(namespaceWsdl);
        ArgumentException.ThrowIfNullOrWhiteSpace(acao);
        Nome = nome;
        NamespaceWsdl = namespaceWsdl;
        Acao = acao;
    }

    /// <summary>O nome do serviço (<c>"NFeStatusServico4"</c>).</summary>
    public string Nome { get; }

    /// <summary>O namespace do WSDL, o do <c>nfeDadosMsg</c> e do <c>nfeResultMsg</c>.</summary>
    public string NamespaceWsdl { get; }

    /// <summary>A ação SOAP, no parâmetro <c>action</c> do <c>Content-Type</c>.</summary>
    public string Acao { get; }

    /// <summary>
    /// O nome da operação, nos serviços cujo <c>Body</c> a tem como elemento (<c>"nfeDistDFeInteresse"</c>, da
    /// distribuição DF-e, NT 2014.002). Quando não nulo, o <c>nfeDadosMsg</c> vai dentro de
    /// <c>&lt;Operacao xmlns="namespaceWsdl"&gt;</c>, e a resposta é lida em <c>&lt;Operacao&gt;Response/&lt;Operacao&gt;Result</c>,
    /// em vez do <c>nfeResultMsg</c> direto no <c>Body</c>. Nulo (o padrão) é o envelope dos outros serviços, sem mudança.
    /// </summary>
    /// <exception cref="ArgumentException">O valor é vazio, só espaços ou não é um nome XML simples (letras, algarismos e sublinhado).</exception>
    public string? Operacao
    {
        get;
        init
        {
            if (value is not null && (string.IsNullOrWhiteSpace(value) || !value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_') || char.IsAsciiDigit(value[0])))
                throw new ArgumentException("A operação tem de ser um nome XML simples (letras, algarismos e sublinhado).", nameof(value));
            field = value;
        }
    }
}
