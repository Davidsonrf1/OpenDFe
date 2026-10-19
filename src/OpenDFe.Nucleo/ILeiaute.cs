namespace OpenDFe;

/// <summary>
/// Um documento numa versão do leiaute (PROJETO.md 3.1): o que um leiaute implementa para o núcleo. Tudo o que
/// ele tem (operações, atendimentos, URLs, a configuração do documento) é declarado em
/// <see cref="Declarar"/>.
/// </summary>
/// <remarks>
/// Sem estado e seguro para uso concorrente: a mesma instância serve a vários núcleos.
/// </remarks>
public interface ILeiaute
{
    /// <summary>A versão do leiaute, como no atributo <c>versao</c> do XML (<c>"4.00"</c>).</summary>
    string Versao { get; }

    /// <summary>Os documentos que o leiaute atende (<c>[NFe, NFCe]</c>).</summary>
    IReadOnlyList<Documento> Documentos { get; }

    /// <summary>
    /// Declara tudo o que o leiaute tem para o documento de <see cref="DeclaracaoLeiaute.Documento"/>. Chamado
    /// por <see cref="DFe.Criar"/> e <see cref="DeclaracaoLeiaute.Criar"/>; a mesma entrada dá sempre a mesma
    /// declaração.
    /// </summary>
    /// <param name="declaracao">Onde declarar; vale só durante a chamada.</param>
    void Declarar(DeclaracaoLeiaute declaracao);
}
