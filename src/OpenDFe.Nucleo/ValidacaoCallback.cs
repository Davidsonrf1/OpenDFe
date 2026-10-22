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
/// O núcleo os chama no <see cref="DFe.ExecutarAsync{TRetorno}"/>, para cada documento do pedido
/// (<see cref="PedidoWS.Documentos"/>), depois do schema e das regras do leiaute e só sem erro neles: o
/// <see cref="Campo"/> em cada campo do documento assinado (cada elemento sem filhos e cada atributo, fora da
/// <c>Signature</c>), na ordem do XML, e depois o <see cref="Documento"/>. Com um problema de severidade
/// <see cref="Severidade.Erro"/>, nada é enviado.
/// </para>
/// </remarks>
public sealed class ValidacaoCallback
{
    /// <summary>Chamado por campo escrito no XML.</summary>
    public Func<CampoVerificado, IEnumerable<Problema>>? Campo { get; init; }

    /// <summary>Chamado por documento inteiro (cada nota do lote).</summary>
    public Func<DocumentoVerificado, IEnumerable<Problema>>? Documento { get; init; }
}
