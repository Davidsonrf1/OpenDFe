namespace OpenDFe.SefazEmulada;

// Os códigos de resultado (cStat) que o emulador usa e o texto do xMotivo, como o MOC 7.0, Anexo I, 4.4.1
// os escreve (sem a referência à NT entre parênteses).
internal static class Motivos
{
    public const string Operando = "107";
    public const string ParalisadoMomentaneamente = "108";
    public const string ParalisadoIndefinidamente = "109";
    public const string TamanhoExcedido = "214";
    public const string FalhaSchema = "215";
    public const string VersaoNaoSuportada = "239";
    public const string AmbienteDivergente = "252";
    public const string UFDivergente = "289";
    public const string PrefixoNamespace = "404";
    public const string RaizInexistente = "516";
    public const string VersaoInexistente = "517";

    private static readonly Dictionary<string, string> Textos = new(StringComparer.Ordinal)
    {
        [Operando] = "Serviço em Operação",
        [ParalisadoMomentaneamente] = "Serviço Paralisado Momentaneamente (curto prazo)",
        [ParalisadoIndefinidamente] = "Serviço Paralisado sem Previsão",
        [TamanhoExcedido] = "Rejeição: Tamanho da mensagem excedeu o limite estabelecido",
        [FalhaSchema] = "Rejeição: Falha no schema XML",
        [VersaoNaoSuportada] = "Rejeição: Versão do arquivo XML não suportada",
        [AmbienteDivergente] = "Rejeição: Ambiente informado diverge do Ambiente de recebimento",
        [UFDivergente] = "Rejeição: Código da UF informada diverge da UF solicitada",
        [PrefixoNamespace] = "Rejeição: Uso de prefixo de namespace não permitido",
        [RaizInexistente] = "Rejeição: Falha no schema XML – inexiste a tag raiz esperada para a mensagem",
        [VersaoInexistente] = "Rejeição: Falha no schema XML – inexiste atributo versao na tag raiz da mensagem",
    };

    public static string Texto(string codigo) => Textos[codigo];
}
