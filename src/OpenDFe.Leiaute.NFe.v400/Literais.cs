using System.Globalization;

namespace OpenDFe.NFe;

/// <summary>
/// Os valores do XML que não são número (MODELO.md, decisões 8 e 31): o membro da enumeração vira o texto do leiaute. O
/// valor fora da lista sai como o número, e o schema aponta.
/// </summary>
internal static class Literais
{
    internal static string? ModeloCupomFiscal(ModeloCupomFiscal? valor) => valor switch
    {
        null => null,
        NFe.ModeloCupomFiscal.MaquinaRegistradora => "2B",
        NFe.ModeloCupomFiscal.Pdv => "2C",
        NFe.ModeloCupomFiscal.Ecf => "2D",
        _ => Numero((int)valor),
    };

    internal static string? IndicadorEscala(IndicadorEscala? valor) => valor switch
    {
        null => null,
        NFe.IndicadorEscala.Relevante => "S",
        NFe.IndicadorEscala.NaoRelevante => "N",
        _ => Numero((int)valor),
    };

    internal static string? CondicaoVin(CondicaoVin? valor) => valor switch
    {
        null => null,
        NFe.CondicaoVin.Remarcado => "R",
        NFe.CondicaoVin.Normal => "N",
        _ => Numero((int)valor),
    };

    // A leitura (docs/LEITURA.md 3): o texto do leiaute vira o membro; o que não é da tradução fica nulo, e a leitura
    // tenta o número.
    internal static ModeloCupomFiscal? LerModeloCupomFiscal(string texto) => texto switch
    {
        "2B" => NFe.ModeloCupomFiscal.MaquinaRegistradora,
        "2C" => NFe.ModeloCupomFiscal.Pdv,
        "2D" => NFe.ModeloCupomFiscal.Ecf,
        _ => null,
    };

    internal static IndicadorEscala? LerIndicadorEscala(string texto) => texto switch
    {
        "S" => NFe.IndicadorEscala.Relevante,
        "N" => NFe.IndicadorEscala.NaoRelevante,
        _ => null,
    };

    internal static CondicaoVin? LerCondicaoVin(string texto) => texto switch
    {
        "R" => NFe.CondicaoVin.Remarcado,
        "N" => NFe.CondicaoVin.Normal,
        _ => null,
    };

    private static string Numero(int valor) => valor.ToString(CultureInfo.InvariantCulture);
}
