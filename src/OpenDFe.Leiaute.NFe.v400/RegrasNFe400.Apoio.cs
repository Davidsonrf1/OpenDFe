using System.Globalization;
using System.Reflection;

namespace OpenDFe.NFe;

// O apoio das regras: os caminhos no documento (como os do schema, com o índice só no elemento que se repete), os
// valores do ICMS de cada grupo e o detalhe com os valores.
internal static partial class RegrasNFe400
{
    private const string Raiz = "NFe/infNFe";

    // Propriedades, e não campos: a Lista é montada na inicialização estática, e a ordem dos inicializadores entre os
    // arquivos da classe parcial não é definida.
    private static Documento[] Ambos => [Documento.NFe, Documento.NFCe];
    private static Documento[] SoNFe => [Documento.NFe];

    private static RegraNFe Regra(string identificador, string codigo, Documento[] modelos, AplicacaoRegra aplicacao,
        string mensagem, Func<NotaFiscal, ContextoRegra, IEnumerable<OcorrenciaRegra>> conferir) =>
        new(identificador, codigo, modelos, aplicacao, mensagem, conferir);

    // O passo de um elemento de uma lista: com o índice (a partir de 1) só quando a lista tem mais de um.
    private static string Passo(string nome, int indice, int total) =>
        total > 1 ? string.Create(CultureInfo.InvariantCulture, $"{nome}[{indice + 1}]") : nome;

    private static string Item(NotaFiscal nota, int indice) => $"{Raiz}/{Passo("det", indice, nota.Itens.Count)}";

    // O caminho do grupo do ICMS do item, pelo [Tag] da subclasse (ICMS00, ICMSSN102, ICMSPart, ICMSST).
    private static string GrupoIcms(NotaFiscal nota, int indice, Icms icms) =>
        $"{Item(nota, indice)}/imposto/ICMS/{icms.GetType().GetCustomAttribute<TagAttribute>()!.Tag}";

    // O detalhe dos valores, com vírgula decimal e as casas que o valor tem (mínimo de duas).
    private static string Valor(decimal valor) =>
        valor.ToString("0.00########", CultureInfo.InvariantCulture).Replace('.', ',');

    private static string Comparacao(string campo, decimal informado, decimal calculado, string origem = "soma dos itens") =>
        $"{campo} {Valor(informado)}; {origem} {Valor(calculado)}";

    // O valor do campo que só alguns grupos do ICMS têm; zero nos outros e quando não informado.
    private static decimal BaseIcms(Icms? icms) => icms switch
    {
        Icms00 g => g.BaseCalculo ?? 0,
        Icms10 g => g.BaseCalculo ?? 0,
        Icms20 g => g.BaseCalculo ?? 0,
        Icms51 g => g.BaseCalculo ?? 0,
        Icms70 g => g.BaseCalculo ?? 0,
        Icms90 g => g.BaseCalculo ?? 0,
        IcmsPartilha g => g.BaseCalculo ?? 0,
        IcmsSn900 g => g.BaseCalculo ?? 0,
        _ => 0,
    };

    private static decimal ValorIcms(Icms? icms) => icms switch
    {
        Icms00 g => g.Valor ?? 0,
        Icms10 g => g.Valor ?? 0,
        Icms20 g => g.Valor ?? 0,
        Icms51 g => g.Valor ?? 0,
        Icms70 g => g.Valor ?? 0,
        Icms90 g => g.Valor ?? 0,
        IcmsPartilha g => g.Valor ?? 0,
        IcmsSn900 g => g.Valor ?? 0,
        _ => 0,
    };

    private static decimal DesoneradoIcms(Icms? icms) => icms switch
    {
        Icms20 g => g.ValorDesonerado ?? 0,
        Icms30 g => g.ValorDesonerado ?? 0,
        Icms40 g => g.ValorDesonerado ?? 0,
        Icms70 g => g.ValorDesonerado ?? 0,
        Icms90 g => g.ValorDesonerado ?? 0,
        IcmsPartilha g => g.ValorDesonerado ?? 0,
        _ => 0,
    };

    private static decimal FcpIcms(Icms? icms) => icms switch
    {
        Icms00 g => g.ValorFcp ?? 0,
        Icms10 g => g.ValorFcp ?? 0,
        Icms20 g => g.ValorFcp ?? 0,
        Icms51 g => g.ValorFcp ?? 0,
        Icms70 g => g.ValorFcp ?? 0,
        Icms90 g => g.ValorFcp ?? 0,
        _ => 0,
    };

    private static decimal BaseStIcms(Icms? icms) => icms switch
    {
        Icms10 g => g.BaseCalculoSt ?? 0,
        Icms30 g => g.BaseCalculoSt ?? 0,
        Icms70 g => g.BaseCalculoSt ?? 0,
        Icms90 g => g.BaseCalculoSt ?? 0,
        IcmsPartilha g => g.BaseCalculoSt ?? 0,
        IcmsSn201 g => g.BaseCalculoSt ?? 0,
        IcmsSn202 g => g.BaseCalculoSt ?? 0,
        IcmsSn900 g => g.BaseCalculoSt ?? 0,
        _ => 0,
    };

    private static decimal StIcms(Icms? icms) => icms switch
    {
        Icms10 g => g.ValorSt ?? 0,
        Icms30 g => g.ValorSt ?? 0,
        Icms70 g => g.ValorSt ?? 0,
        Icms90 g => g.ValorSt ?? 0,
        IcmsPartilha g => g.ValorSt ?? 0,
        IcmsSn201 g => g.ValorSt ?? 0,
        IcmsSn202 g => g.ValorSt ?? 0,
        IcmsSn900 g => g.ValorSt ?? 0,
        _ => 0,
    };

    private static decimal FcpStIcms(Icms? icms) => icms switch
    {
        Icms10 g => g.ValorFcpSt ?? 0,
        Icms30 g => g.ValorFcpSt ?? 0,
        Icms70 g => g.ValorFcpSt ?? 0,
        Icms90 g => g.ValorFcpSt ?? 0,
        IcmsPartilha g => g.ValorFcpSt ?? 0,
        IcmsSn201 g => g.ValorFcpSt ?? 0,
        IcmsSn202 g => g.ValorFcpSt ?? 0,
        IcmsSn900 g => g.ValorFcpSt ?? 0,
        _ => 0,
    };

    private static decimal FcpStRetidoIcms(Icms? icms) => icms switch
    {
        Icms60 g => g.ValorFcpStRetido ?? 0,
        IcmsRepasse g => g.ValorFcpStRetido ?? 0,
        IcmsSn500 g => g.ValorFcpStRetido ?? 0,
        _ => 0,
    };

    private static decimal ValorPis(Pis? pis) => pis switch
    {
        PisAliquota g => g.Valor ?? 0,
        PisQuantidade g => g.Valor ?? 0,
        PisOutros g => g.Valor ?? 0,
        _ => 0,
    };

    private static decimal ValorCofins(Cofins? cofins) => cofins switch
    {
        CofinsAliquota g => g.Valor ?? 0,
        CofinsQuantidade g => g.Valor ?? 0,
        CofinsOutros g => g.Valor ?? 0,
        _ => 0,
    };

    // O DV da chave de acesso (módulo 11, cada posição valendo o código ASCII menos 48, como no CNPJ alfanumérico);
    // a chave fora da forma (tamanho, caracteres) é do schema e não é conferida aqui.
    private static bool DigitoChaveValido(string chave)
    {
        if (chave.Length != ChaveAcesso.Tamanho || !char.IsAsciiDigit(chave[^1]))
            return true;
        var soma = 0;
        var peso = 2;
        for (var i = chave.Length - 2; i >= 0; i--)
        {
            if (!char.IsAsciiDigit(chave[i]) && !char.IsAsciiLetterUpper(chave[i]))
                return true;
            soma += (chave[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        return (resto < 2 ? 0 : 11 - resto) == chave[^1] - '0';
    }
}
