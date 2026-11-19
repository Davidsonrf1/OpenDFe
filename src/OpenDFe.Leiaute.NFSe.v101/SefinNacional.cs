namespace OpenDFe.NFSe;

/// <summary>
/// A Sefin Nacional na tabela de URLs do leiaute (docs/NTS.md 12.4): a base de cada ambiente, com o nome do serviço
/// <c>"SefinNacional"</c>, que as operações completam com o caminho (<c>nfse</c>, <c>nfse/{chave}</c>, <c>dps/{id}</c>).
/// </summary>
/// <remarks>
/// A base da produção restrita aparece com e sem <c>/API/</c> nas fontes (a página de APIs documenta
/// <c>.../API/SefinNacional</c>; as implementações abertas que emitem lá usam <c>.../SefinNacional</c>). O leiaute usa a das
/// implementações; a outra entra pela <see cref="ConfiguracaoDFe.Urls"/>. O PFX decide (docs/NFSE.md 6, item 1).
/// </remarks>
internal static class SefinNacional
{
    /// <summary>O nome do serviço na tabela de URLs.</summary>
    internal const string Servico = "SefinNacional";

    /// <summary>O campo do JSON da emissão que leva a DPS assinada, em GZip e Base64.</summary>
    internal const string CampoDps = "dpsXmlGZipB64";

    /// <summary>O campo do JSON do registro de evento que leva o pedido assinado, em GZip e Base64 (9.3).</summary>
    internal const string CampoEvento = "pedidoRegistroEventoXmlGZipB64";

    internal static readonly Uri Producao = new("https://sefin.nfse.gov.br/SefinNacional");

    internal static readonly Uri ProducaoRestrita = new("https://sefin.producaorestrita.nfse.gov.br/SefinNacional");

    /// <summary>Declara as duas bases.</summary>
    internal static void Declarar(DeclaracaoLeiaute declaracao)
    {
        declaracao.DeclararEndereco(new EnderecoServico(Destino.SefinNacional, Ambiente.Producao, Servico, Producao));
        declaracao.DeclararEndereco(new EnderecoServico(Destino.SefinNacional, Ambiente.Homologacao, Servico, ProducaoRestrita));
    }
}
