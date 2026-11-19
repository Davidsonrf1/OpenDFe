namespace OpenDFe.NFSe;

/// <summary>A operação com bem imóvel, exceto obra (<c>imovel</c>).</summary>
public sealed class Imovel
{
    /// <summary>A inscrição imobiliária fiscal; opcional.</summary>
    [Tag("inscImobFisc")]
    public string? InscricaoImobiliaria { get; set; }

    /// <summary>O código do Cadastro Imobiliário Brasileiro; um dos dois (CIB ou endereço).</summary>
    [Tag("cCIB")]
    public string? Cib { get; set; }

    /// <summary>O endereço do imóvel.</summary>
    [Tag("end")]
    public EnderecoObra? Endereco { get; set; }
}
