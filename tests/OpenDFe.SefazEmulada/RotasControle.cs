using System.Net;

namespace OpenDFe.SefazEmulada;

// A rota /_controle (PROJETO.md 13.5), aceita só do próprio computador:
//   GET    /_controle/falhas        as falhas ativas
//   POST   /_controle/falhas        acrescenta uma (PedidoFalha em JSON); 201 com a falha, 400 com o motivo
//   DELETE /_controle/falhas/{id}   remove uma; 404 se não existe
//   DELETE /_controle/falhas        remove todas
//   GET    /_controle/notas         as notas autorizadas, na ordem
//   DELETE /_controle/notas         esquece as notas e os lotes
internal static class RotasControle
{
    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup("/_controle").AddEndpointFilter(async (contexto, proximo) =>
            Local(contexto.HttpContext.Connection.RemoteIpAddress)
                ? await proximo(contexto)
                : Results.StatusCode(StatusCodes.Status403Forbidden));

        grupo.MapGet("/falhas", (Controle controle) => controle.Listar());
        grupo.MapPost("/falhas", (PedidoFalha pedido, Controle controle) =>
        {
            try
            {
                var falha = controle.Acrescentar(pedido);
                return Results.Created($"/_controle/falhas/{falha.Id}", falha);
            }
            catch (ArgumentException erro)
            {
                return Results.BadRequest(new { erro = MensagemLimpa(erro) });
            }
        });
        grupo.MapDelete("/falhas/{id:int}", (int id, Controle controle) =>
            controle.Remover(id) ? Results.NoContent() : Results.NotFound());
        grupo.MapDelete("/falhas", (Controle controle) =>
        {
            controle.Limpar();
            return Results.NoContent();
        });
        grupo.MapGet("/notas", (Repositorio repositorio) => repositorio.Notas());
        grupo.MapDelete("/notas", (Repositorio repositorio) =>
        {
            repositorio.Limpar();
            return Results.NoContent();
        });
    }

    // O próprio computador: o loopback, também o IPv4 mapeado em IPv6.
    public static bool Local(IPAddress? endereco) =>
        endereco is not null && IPAddress.IsLoopback(endereco.IsIPv4MappedToIPv6 ? endereco.MapToIPv4() : endereco);

    // A mensagem da ArgumentException sem o " (Parameter 'pedido')" que o .NET acrescenta.
    private static string MensagemLimpa(ArgumentException erro) =>
        erro.ParamName is null ? erro.Message : erro.Message.Replace($" (Parameter '{erro.ParamName}')", "", StringComparison.Ordinal);
}
