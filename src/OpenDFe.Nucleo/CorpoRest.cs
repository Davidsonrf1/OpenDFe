using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace OpenDFe;

/// <summary>
/// O corpo do pedido REST (docs/NFSE.md, D4): o XML assinado em UTF-8, compactado em GZip e codificado em Base64, num JSON
/// de um campo só (<c>{"dpsXmlGZipB64": "H4sI..."}</c>).
/// </summary>
internal static class CorpoRest
{
    /// <summary>O JSON com o XML compactado no <paramref name="campo"/>.</summary>
    internal static string Montar(string campo, string xml)
    {
        using var saida = new MemoryStream();
        using (var escritor = new Utf8JsonWriter(saida))
        {
            escritor.WriteStartObject();
            escritor.WriteString(campo, Compactar(xml));
            escritor.WriteEndObject();
        }
        return Encoding.UTF8.GetString(saida.ToArray());
    }

    /// <summary>O texto em UTF-8 (sem BOM), em GZip e Base64.</summary>
    internal static string Compactar(string texto)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal, leaveOpen: true))
        {
            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(texto);
            gzip.Write(bytes);
        }
        return Convert.ToBase64String(saida.ToArray());
    }
}
