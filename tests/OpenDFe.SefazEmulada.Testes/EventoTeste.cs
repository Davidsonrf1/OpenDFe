using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// Um evento da NF-e (MOC 7.0, Visão Geral, 5.8 a 5.10), escrito em texto e assinado no infEvento pelo SignedXml
// do .NET, sem a lib, como a NotaTeste: o cancelamento (110111, o padrão), o cancelamento por substituição
// (110112) ou a carta de correção (110110), com os campos que os testes trocam.
internal sealed record EventoTeste
{
    public const string Cancelamento = "110111";
    public const string Substituicao = "110112";
    public const string CartaCorrecao = "110110";

    // O xCondUso sem acentuação, como o schema e110110 o enumera.
    public const string CondicaoUso = "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro "
        + "de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja "
        + "relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, "
        + "quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do "
        + "destinatario; III - a data de emissao ou de saida.";

    public required string Chave { get; init; }

    public string Tipo { get; init; } = Cancelamento;

    // Nulo: a UF da chave.
    public string? Orgao { get; init; }

    public string Ambiente { get; init; } = "2";

    public string? Cnpj { get; init; } = "11222333000181";

    public string? Cpf { get; init; }

    public DateTimeOffset Data { get; init; } = NotaTeste.Agora();

    public int Sequencia { get; init; } = 1;

    public string VersaoEvento { get; init; } = "1.00";

    // O nProt da nota, no cancelamento.
    public string Protocolo { get; init; } = "000000000000000";

    public string Justificativa { get; init; } = "CANCELAMENTO DE TESTE DO EMULADOR";

    // O chNFeRef e o cOrgaoAutor (nulo: a UF da chave) do cancelamento por substituição.
    public string? Referencia { get; init; }

    public string? OrgaoAutor { get; init; }

    public string Correcao { get; init; } = "CORRECAO DE TESTE DO EMULADOR";

    public string DescricaoCarta { get; init; } = "Carta de Correcao";

    // O detEvento inteiro, no lugar do que o tipo monta.
    public string? Detalhe { get; init; }

    // O Id trocado (572).
    public string? Id { get; init; }

    public X509Certificate2 Certificado { get; init; } = Clientes.CertificadoCliente;

    // O XML assinado, como sai do autor.
    public string Gerar()
    {
        var id = Id ?? "ID" + Tipo + Chave + Sequencia.ToString("D2", CultureInfo.InvariantCulture);
        var autor = Cnpj is not null ? $"<CNPJ>{Cnpj}</CNPJ>" : $"<CPF>{Cpf}</CPF>";
        var detalhe = Detalhe ?? Tipo switch
        {
            CartaCorrecao => $"<detEvento versao=\"1.00\"><descEvento>{DescricaoCarta}</descEvento><xCorrecao>{Correcao}</xCorrecao><xCondUso>{CondicaoUso}</xCondUso></detEvento>",
            Substituicao => "<detEvento versao=\"1.00\"><descEvento>Cancelamento por substituicao</descEvento>"
                + $"<cOrgaoAutor>{OrgaoAutor ?? Chave[..2]}</cOrgaoAutor><tpAutor>1</tpAutor><verAplic>TESTE 1.0</verAplic>"
                + $"<nProt>{Protocolo}</nProt><xJust>{Justificativa}</xJust><chNFeRef>{Referencia}</chNFeRef></detEvento>",
            _ => $"<detEvento versao=\"1.00\"><descEvento>Cancelamento</descEvento><nProt>{Protocolo}</nProt><xJust>{Justificativa}</xJust></detEvento>",
        };
        var xml = $"<evento xmlns=\"{Mensagens.NamespaceNFe}\" versao=\"1.00\"><infEvento Id=\"{id}\"><cOrgao>{Orgao ?? Chave[..2]}</cOrgao>"
            + $"<tpAmb>{Ambiente}</tpAmb>{autor}<chNFe>{Chave}</chNFe>"
            + $"<dhEvento>{Data.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)}</dhEvento>"
            + $"<tpEvento>{Tipo}</tpEvento><nSeqEvento>{Sequencia}</nSeqEvento><verEvento>{VersaoEvento}</verEvento>{detalhe}</infEvento></evento>";
        return NotaTeste.Assinar(xml, id, Certificado);
    }
}

// O pedido de inutilização (Visão Geral, Tabela 5-9), escrito em texto e assinado no infInut. Cada um sai com
// uma faixa nova, longe dos números da NotaTeste, para os testes que dividem o emulador não se cruzarem.
internal sealed record InutilizacaoTeste
{
    private static long _ultimo = 1_000_000;

    public string Uf { get; init; } = "31";

    public string Ano { get; init; } = NotaTeste.Agora().ToString("yy", CultureInfo.InvariantCulture);

    public string Cnpj { get; init; } = "11222333000181";

    public string Modelo { get; init; } = "55";

    public string Serie { get; init; } = "1";

    public long Inicio { get; init; }

    public long Fim { get; init; }

    public string Ambiente { get; init; } = "2";

    public string Justificativa { get; init; } = "NUMERACAO PULADA NO TESTE";

    public string? Id { get; init; }

    public X509Certificate2 Certificado { get; init; } = Clientes.CertificadoCliente;

    // Uma faixa nova com a quantidade de números.
    public static InutilizacaoTeste Nova(int quantidade = 1)
    {
        var fim = Interlocked.Add(ref _ultimo, Math.Max(quantidade, 1_000));
        return new InutilizacaoTeste { Inicio = fim - quantidade + 1, Fim = fim };
    }

    public string Gerar()
    {
        var id = Id ?? "ID" + Uf + Ano + Cnpj + Modelo + Serie.PadLeft(3, '0')
            + Inicio.ToString("D9", CultureInfo.InvariantCulture) + Fim.ToString("D9", CultureInfo.InvariantCulture);
        var xml = $"<inutNFe xmlns=\"{Mensagens.NamespaceNFe}\" versao=\"4.00\"><infInut Id=\"{id}\"><tpAmb>{Ambiente}</tpAmb>"
            + $"<xServ>INUTILIZAR</xServ><cUF>{Uf}</cUF><ano>{Ano}</ano><CNPJ>{Cnpj}</CNPJ><mod>{Modelo}</mod><serie>{Serie}</serie>"
            + $"<nNFIni>{Inicio}</nNFIni><nNFFin>{Fim}</nNFFin><xJust>{Justificativa}</xJust></infInut></inutNFe>";
        return NotaTeste.Assinar(xml, id, Certificado);
    }
}

// Os retornos do emulador contra os XSDs do leiaute, pelo SchemaXml da lib.
internal static class Esquemas
{
    public static IReadOnlyList<Problema> Validar(string arquivo, System.Xml.Linq.XElement xml)
    {
        var assembly = typeof(OpenDFe.NFe.LeiauteNFe400).Assembly;
        var recurso = assembly.GetManifestResourceNames().Single(n => n.Replace('\\', '/').EndsWith("/" + arquivo, StringComparison.Ordinal));
        return SchemaXml.Carregar(assembly, recurso, "215").Validar(xml.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
    }
}
