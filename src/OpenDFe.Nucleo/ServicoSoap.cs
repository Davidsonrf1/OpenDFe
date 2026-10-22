namespace OpenDFe;

/// <summary>
/// O que o envelope SOAP 1.2 de um serviço leva (PROJETO.md 8.2): o namespace do WSDL, que é o do
/// <c>nfeDadosMsg</c> e do <c>nfeResultMsg</c>, e a ação, no <c>Content-Type</c>. O leiaute declara um por serviço
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
}
