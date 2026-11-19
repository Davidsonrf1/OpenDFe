using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace OpenDFe.Testes;

// Um pedido de registro de evento da NFS-e (pedRegEvento 1.01) mínimo, escrito em texto e assinado com o SignedXml do
// .NET (NotaTeste.Assinar), sem a lib, como a DpsTeste (E.8). O detalhe segue o tipo: o cancelamento e a análise fiscal
// com o motivo e a descrição; os outros só com o xDesc (os da manifestação), para as recusas.
internal sealed record PedidoEventoTeste
{
    private static readonly Dictionary<string, string> Descricoes = new(StringComparer.Ordinal)
    {
        ["101101"] = "Cancelamento de NFS-e",
        ["101103"] = "Solicitação de Análise Fiscal para Cancelamento de NFS-e",
        ["202201"] = "Manifestação de NFS-e - Confirmação do Prestador",
    };

    public required string Chave { get; init; }

    public string Tipo { get; init; } = "101101";

    public string? Cnpj { get; init; } = "11222333000181";

    public string? Cpf { get; init; }

    public string Ambiente { get; init; } = "2";

    public DateTimeOffset Data { get; init; } = NotaTeste.Agora().AddMinutes(-1);

    public string Motivo { get; init; } = "1";

    public string Descricao { get; init; } = "Erro na emissão da nota de teste";

    // Nulo: PRE + a chave + o tipo.
    public string? Id { get; init; }

    public X509Certificate2? Certificado { get; init; } = Clientes.CertificadoCliente;

    public string IdComposto => "PRE" + Chave + Tipo;

    // O XML assinado (sem a assinatura, com o Certificado nulo).
    public string Gerar()
    {
        var id = Id ?? IdComposto;
        var autor = Cnpj is not null ? $"<CNPJAutor>{Cnpj}</CNPJAutor>" : $"<CPFAutor>{Cpf}</CPFAutor>";
        var detalhe = Tipo is "101101" or "101103"
            ? $"<xDesc>{Descricoes[Tipo]}</xDesc><cMotivo>{Motivo}</cMotivo><xMotivo>{Descricao}</xMotivo>"
            : $"<xDesc>{Descricoes[Tipo]}</xDesc>";
        var xml = $"<pedRegEvento xmlns=\"{DpsTeste.Namespace}\" versao=\"1.01\"><infPedReg Id=\"{id}\">"
            + $"<tpAmb>{Ambiente}</tpAmb><verAplic>TESTE 1.0</verAplic>"
            + $"<dhEvento>{Data.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)}</dhEvento>{autor}<chNFSe>{Chave}</chNFSe>"
            + $"<e{Tipo}>{detalhe}</e{Tipo}></infPedReg></pedRegEvento>";
        return Certificado is null ? xml : NotaTeste.Assinar(xml, id, Certificado);
    }

    // O corpo do POST /nfse/{chave}/eventos: o XML em GZip e Base64 no JSON.
    public static string Corpo(string xml) =>
        JsonSerializer.Serialize(new Dictionary<string, string> { ["pedidoRegistroEventoXmlGZipB64"] = DpsTeste.Compactar(Encoding.UTF8.GetBytes(xml)) });
}
