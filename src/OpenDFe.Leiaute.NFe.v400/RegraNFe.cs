namespace OpenDFe.NFe;

/// <summary>
/// Uma regra de validação do MOC ou de uma NT (PROJETO.md 6.2, REGRAS.md 4): a rejeição que ela antecipa, onde está na
/// fonte, em que modelos vale, se é obrigatória e a conferência sobre a nota. Sem estado e segura em paralelo.
/// </summary>
internal sealed class RegraNFe
{
    /// <summary>Cria a regra.</summary>
    /// <param name="identificador">O identificador na fonte, com a sequência (<c>"W16-10"</c>, <c>"UB12-10"</c>).</param>
    /// <param name="codigo">O código da rejeição (<c>"564"</c>).</param>
    /// <param name="modelos">Os modelos em que vale (55, 65 ou os dois).</param>
    /// <param name="aplicacao">Obrigatória (erro) ou facultativa da UF (aviso).</param>
    /// <param name="mensagem">A descrição do erro na fonte, sem o "Rejeição: ".</param>
    /// <param name="conferir">A conferência: cada ocorrência vira um problema.</param>
    internal RegraNFe(
        string identificador, string codigo, IReadOnlyList<Documento> modelos, AplicacaoRegra aplicacao, string mensagem,
        Func<NotaFiscal, ContextoRegra, IEnumerable<OcorrenciaRegra>> conferir)
    {
        Identificador = identificador;
        Codigo = codigo;
        Modelos = modelos;
        Aplicacao = aplicacao;
        Mensagem = mensagem;
        Conferir = conferir;
    }

    /// <summary>O identificador na fonte, com a sequência (<c>"W16-10"</c>).</summary>
    internal string Identificador { get; }

    /// <summary>O código da rejeição (<c>"564"</c>), o <see cref="Problema.Codigo"/>.</summary>
    internal string Codigo { get; }

    /// <summary>Os modelos em que a regra vale.</summary>
    internal IReadOnlyList<Documento> Modelos { get; }

    /// <summary>Obrigatória ou facultativa.</summary>
    internal AplicacaoRegra Aplicacao { get; }

    /// <summary>A descrição do erro na fonte, sem o "Rejeição: ".</summary>
    internal string Mensagem { get; }

    /// <summary>A conferência sobre a nota.</summary>
    internal Func<NotaFiscal, ContextoRegra, IEnumerable<OcorrenciaRegra>> Conferir { get; }

    /// <summary>A severidade do problema: erro na obrigatória, aviso na facultativa (REGRAS.md, decisão 6).</summary>
    internal Severidade Severidade => Aplicacao == AplicacaoRegra.Obrigatoria ? Severidade.Erro : Severidade.Aviso;
}

/// <summary>A coluna "Aplic." do MOC.</summary>
internal enum AplicacaoRegra
{
    /// <summary><c>Obrig.</c>: toda UF aplica.</summary>
    Obrigatoria = 1,

    /// <summary><c>Facul.</c>: a UF escolhe se aplica.</summary>
    Facultativa = 2,
}

/// <summary>
/// O que a regra achou: o campo, pelo caminho no documento (<c>"NFe/infNFe/total/ICMSTot/vProd"</c>, como o do schema),
/// e o detalhe com os valores, sem dado pessoal (PROJETO.md 6.4). A propriedade sai do caminho
/// (<see cref="CaminhoModeloNFe"/>), salvo quando a regra a dá.
/// </summary>
internal sealed record OcorrenciaRegra(string? Caminho, string? Detalhe = null, string? Propriedade = null);

/// <summary>O que as regras sabem além da nota: os valores do núcleo, a chave e o instante da validação.</summary>
internal sealed class ContextoRegra
{
    internal ContextoRegra(ValoresNucleo nucleo, ChaveAcesso? chave, DateTimeOffset agora)
    {
        Nucleo = nucleo;
        Chave = chave;
        Agora = agora;
    }

    /// <summary>O modelo, a UF, o ambiente, o tipo de emissão e a contingência do núcleo.</summary>
    internal ValoresNucleo Nucleo { get; }

    /// <summary>A chave de acesso do documento, quando ele tem uma.</summary>
    internal ChaveAcesso? Chave { get; }

    /// <summary>O instante da validação, pelo relógio da configuração (as regras de data).</summary>
    internal DateTimeOffset Agora { get; }
}
