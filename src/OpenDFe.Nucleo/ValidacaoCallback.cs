namespace OpenDFe;

/// <summary>
/// As regras de negócio de quem usa, na configuração (<see cref="ConfiguracaoDFe.Validacao"/>, PROJETO.md 6.3):
/// um callback por campo escrito e um por documento inteiro. Cada um devolve os problemas encontrados (vazio =
/// nenhum), com o código e a severidade de quem usa.
/// </summary>
/// <remarks>
/// <para>
/// Os callbacks podem ser chamados em paralelo (o núcleo é compartilhado) e devem ser seguros para isso.
/// Exceção num callback sobe como veio: é erro de programação.
/// </para>
/// <para>
/// Quem os chama, e em que ponto do caminho (depois do schema e das regras do leiaute), é do motor de regras
/// (etapa 3.1 do plano): por ora, a configuração só os guarda.
/// </para>
/// </remarks>
public sealed class ValidacaoCallback
{
    /// <summary>Chamado por campo escrito no XML.</summary>
    public Func<CampoVerificado, IEnumerable<Problema>>? Campo { get; init; }

    /// <summary>Chamado por documento inteiro (cada nota do lote).</summary>
    public Func<DocumentoVerificado, IEnumerable<Problema>>? Documento { get; init; }
}
