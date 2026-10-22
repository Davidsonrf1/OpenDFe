namespace OpenDFe;

/// <summary>
/// O detalhe de um problema da validação pelo schema (<see cref="Problema.Schema"/>): o que falhou, as regras
/// aplicadas, o valor encontrado e a posição no XML.
/// </summary>
/// <remarks>
/// <see cref="Valor"/> e <see cref="MensagemOriginal"/> podem trazer dados pessoais do documento (CPF, nome,
/// endereço): servem a quem é dono do XML, para mostrar ao usuário, e não devem ir para log nem telemetria.
/// A <see cref="Problema.Mensagem"/> nunca traz o valor.
/// </remarks>
public sealed class FalhaSchema
{
    internal FalhaSchema(TipoFalhaSchema tipo, string mensagemOriginal)
    {
        Tipo = tipo;
        MensagemOriginal = mensagemOriginal;
    }

    /// <summary>O que falhou.</summary>
    public TipoFalhaSchema Tipo { get; }

    /// <summary>
    /// O nome do tipo do schema do elemento ou atributo (<c>"TDec_1302"</c>; os do XSD com o prefixo,
    /// <c>"xs:string"</c>); <see langword="null"/> se o tipo é anônimo ou desconhecido.
    /// </summary>
    public string? TipoSchema { get; internal init; }

    /// <summary>
    /// O texto do <c>xs:documentation</c> da declaração do campo no schema (<c>"Valor bruto do produto ou
    /// serviço."</c>): o do elemento ou atributo que falhou, ou, na falta de um elemento obrigatório (ausente, ou
    /// outro no lugar dele), o do que faltou; <see langword="null"/> quando o schema não documenta o campo ou a falha
    /// não aponta uma declaração.
    /// </summary>
    public string? Documentacao { get; internal init; }

    /// <summary>
    /// O texto do <c>xs:documentation</c> do tipo nomeado do campo (<c>"Tipo Decimal com 15 dígitos, sendo 13 de corpo
    /// e 2 decimais"</c>, o do <see cref="TipoSchema"/>); <see langword="null"/> quando o tipo não é documentado.
    /// </summary>
    public string? DocumentacaoTipo { get; internal init; }

    /// <summary>
    /// O valor encontrado no elemento ou atributo, como está no XML, na falha de <see cref="TipoFalhaSchema.Valor"/>;
    /// <see langword="null"/> nas outras. Pode ser dado pessoal (ver a observação da classe).
    /// </summary>
    public string? Valor { get; internal init; }

    /// <summary>
    /// As restrições do tipo que o valor violou, na falha de <see cref="TipoFalhaSchema.Valor"/>; vazia nas outras
    /// e quando a lib não identifica a restrição.
    /// </summary>
    public IReadOnlyList<RestricaoSchema> Restricoes { get; internal init; } = [];

    /// <summary>
    /// Os nomes (locais, sem prefixo) dos elementos ou atributos esperados, nas falhas de estrutura: os que
    /// podiam vir no lugar do inesperado, os obrigatórios ausentes, os atributos que o elemento declara. Um
    /// <c>"*"</c> representa qualquer elemento (<c>xs:any</c>).
    /// </summary>
    public IReadOnlyList<string> Esperados { get; internal init; } = [];

    /// <summary>A linha no XML (a partir de 1), quando conhecida.</summary>
    public int? Linha { get; internal init; }

    /// <summary>A coluna no XML (a partir de 1), quando conhecida.</summary>
    public int? Coluna { get; internal init; }

    /// <summary>
    /// A mensagem do validador do .NET, no idioma da máquina, para diagnóstico. Pode trazer o valor (ver a
    /// observação da classe).
    /// </summary>
    public string MensagemOriginal { get; }
}
