namespace OpenDFe.SefazEmulada;

// A página do painel (E.6): HTML, CSS e JS embutidos no assembly (Painel/), sem npm nem build, servidos só na porta do
// painel, a única sem TLS. Nela vale só o que a página precisa: ela mesma e o /_controle; a porta do TLS não serve a
// página. O Host do pedido tem de ser o loopback (contra o DNS rebinding: um site de fora que aponta o próprio nome
// para o 127.0.0.1 não lê nem comanda o emulador), e as respostas levam a CSP que só aceita o próprio painel.
internal static class PaginaPainel
{
    private static readonly Dictionary<string, (byte[] Conteudo, string Tipo)> Arquivos = Carregar();

    // Os hosts do loopback com que o navegador chega ao painel.
    private static readonly string[] HostsLocais = ["127.0.0.1", "localhost", "[::1]"];

    public static Task Separar(HttpContext contexto, RequestDelegate proximo)
    {
        var caminho = contexto.Request.Path;
        var arquivo = Arquivos.ContainsKey(caminho.Value?.TrimStart('/') ?? "");
        if (contexto.Request.IsHttps)
        {
            if (arquivo)
                contexto.Response.StatusCode = StatusCodes.Status404NotFound;
            return arquivo ? Task.CompletedTask : proximo(contexto);
        }

        if (!HostsLocais.Contains(contexto.Request.Host.Host, StringComparer.OrdinalIgnoreCase))
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        if (!(caminho == "/" || arquivo || caminho.StartsWithSegments("/_controle")))
        {
            contexto.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
        var cabecalhos = contexto.Response.Headers;
        cabecalhos.ContentSecurityPolicy = "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self' data:; "
            + "base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
        cabecalhos.XContentTypeOptions = "nosniff";
        cabecalhos.CacheControl = "no-store";
        cabecalhos["Referrer-Policy"] = "no-referrer";
        return proximo(contexto);
    }

    // As rotas dos arquivos da página; o painel.html é o GET / da porta do painel.
    public static void Mapear(WebApplication app)
    {
        foreach (var nome in Arquivos.Keys.Where(n => n != "painel.html"))
            app.MapGet("/" + nome, () => Pagina(nome));
    }

    public static IResult Pagina(string nome)
    {
        var (conteudo, tipo) = Arquivos[nome];
        return Results.Bytes(conteudo, tipo);
    }

    private static Dictionary<string, (byte[], string)> Carregar()
    {
        var assembly = typeof(PaginaPainel).Assembly;
        var arquivos = new Dictionary<string, (byte[], string)>(StringComparer.Ordinal);
        foreach (var recurso in assembly.GetManifestResourceNames().Where(n => n.StartsWith("Painel/", StringComparison.Ordinal)))
        {
            using var fluxo = assembly.GetManifestResourceStream(recurso)!;
            using var memoria = new MemoryStream();
            fluxo.CopyTo(memoria);
            var nome = recurso["Painel/".Length..];
            var tipo = Path.GetExtension(nome) switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "text/javascript; charset=utf-8",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream",
            };
            arquivos[nome] = (memoria.ToArray(), tipo);
        }
        return arquivos;
    }
}
