using System.Collections.ObjectModel;

namespace OpenDFe.NFe;

/// <summary>
/// O leiaute 4.00 da NF-e (modelo 55) e da NFC-e (modelo 65), os dois documentos no mesmo leiaute e no mesmo
/// modelo de dados (PROJETO.md 3.1): o que se passa a <see cref="DFe.Criar"/>.
/// </summary>
/// <remarks>
/// <para>
/// Sem estado e seguro para uso concorrente: uma instância serve a todos os núcleos, de qualquer emitente e
/// documento. Os schemas são os do pacote de liberação embutido (docs/NTS.md), carregados na primeira vez que
/// uma operação os usa.
/// </para>
/// <para>
/// Esqueleto (etapa 2.1): ainda não declara operações, atendimentos nem URLs, que entram nas fases 4 e 5; até
/// lá, <see cref="DeclaracaoLeiaute.Criar"/> lista vazio e <see cref="DFe.Criar"/> recusa a configuração, porque
/// nenhuma UF tem atendimento.
/// </para>
/// </remarks>
public sealed class LeiauteNFe400 : ILeiaute
{
    private static readonly ReadOnlyCollection<Documento> Atendidos = Array.AsReadOnly([Documento.NFe, Documento.NFCe]);

    /// <summary>A versão do leiaute, como no atributo <c>versao</c> do XML: <c>"4.00"</c>.</summary>
    public string Versao => "4.00";

    /// <summary>A NF-e e a NFC-e.</summary>
    public IReadOnlyList<Documento> Documentos => Atendidos;

    /// <summary>Declara o que o leiaute tem para o documento (hoje, nada: ver as observações da classe).</summary>
    /// <param name="declaracao">Onde declarar.</param>
    /// <exception cref="ArgumentNullException">A declaração é nula.</exception>
    public void Declarar(DeclaracaoLeiaute declaracao)
    {
        ArgumentNullException.ThrowIfNull(declaracao);
    }
}
