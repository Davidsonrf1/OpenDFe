namespace OpenDFe;

/// <summary>
/// A base da parte da configuração que é de um documento (o CSC da NFC-e). O leiaute deriva o tipo
/// (<c>ConfiguracaoNFCe</c>) e o declara (<see cref="DeclaracaoLeiaute.DeclararConfiguracaoDocumento{TConfiguracao}"/>);
/// <see cref="DFe.Criar"/> confere que a configuração recebida é dele.
/// </summary>
/// <remarks>O tipo derivado deve ser imutável: o núcleo o usa em paralelo.</remarks>
public abstract class ConfiguracaoDocumento
{
    /// <summary>Construtor do tipo derivado.</summary>
    protected ConfiguracaoDocumento()
    {
    }
}
