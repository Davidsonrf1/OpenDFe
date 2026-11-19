namespace OpenDFe.NFSe;

/// <summary>O serviço de obra (<c>obra</c>).</summary>
public sealed class Obra
{
    /// <summary>A inscrição imobiliária fiscal; opcional.</summary>
    [Tag("inscImobFisc")]
    public string? InscricaoImobiliaria { get; set; }

    /// <summary>O código da obra (CNO ou CEI); um dos três (código, CIB ou endereço).</summary>
    [Tag("cObra")]
    public string? Codigo { get; set; }

    /// <summary>O código do Cadastro Imobiliário Brasileiro.</summary>
    [Tag("cCIB")]
    public string? Cib { get; set; }

    /// <summary>O endereço da obra.</summary>
    [Tag("end")]
    public EnderecoObra? Endereco { get; set; }
}
