using System.Net;

namespace OpenDFe.SefazEmulada;

// A rota /_controle (PROJETO.md 13.5), aceita só do próprio computador:
//   GET    /_controle/falhas        as falhas ativas
//   POST   /_controle/falhas        acrescenta uma (PedidoFalha em JSON); 201 com a falha, 400 com o motivo
//   DELETE /_controle/falhas/{id}   remove uma; 404 se não existe
//   DELETE /_controle/falhas        remove todas
//   GET    /_controle/notas         as notas autorizadas, na ordem
//   DELETE /_controle/notas         esquece as notas, os lotes, os eventos e as faixas inutilizadas
//   GET    /_controle/csc           os CSCs cadastrados e revogados
//   POST   /_controle/csc           cadastra um (PedidoCsc em JSON); 201 com o CSC, 400 com o motivo
//   DELETE /_controle/csc/{uf}/{cnpj}/{id}   revoga um; 404 se não existe
//   DELETE /_controle/csc           esquece os cadastrados e as revogações (fica só o padrão)
//   GET    /_controle/irregulares   os CNPJs e CPFs em situação irregular
//   POST   /_controle/irregulares   cadastra um (PedidoIrregular em JSON); 201 com o documento, 400 com o motivo
//   DELETE /_controle/irregulares/{documento}   tira um; 404 se não está
//   DELETE /_controle/irregulares   tira todos
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
        grupo.MapGet("/csc", (CodigosSeguranca codigos) => codigos.Listar());
        grupo.MapPost("/csc", (PedidoCsc pedido, CodigosSeguranca codigos) =>
        {
            try
            {
                var csc = codigos.Cadastrar(pedido);
                return Results.Created($"/_controle/csc/{csc.Uf}/{csc.Cnpj}/{csc.Id}", csc);
            }
            catch (ArgumentException erro)
            {
                return Results.BadRequest(new { erro = MensagemLimpa(erro) });
            }
        });
        grupo.MapDelete("/csc/{uf}/{cnpj}/{id}", (string uf, string cnpj, string id, CodigosSeguranca codigos, OpcoesEmulador opcoes) =>
        {
            try
            {
                return codigos.Revogar(uf, cnpj, id, opcoes.Relogio.GetUtcNow()) ? Results.NoContent() : Results.NotFound();
            }
            catch (ArgumentException erro)
            {
                return Results.BadRequest(new { erro = MensagemLimpa(erro) });
            }
        });
        grupo.MapDelete("/csc", (CodigosSeguranca codigos) =>
        {
            codigos.Limpar();
            return Results.NoContent();
        });
        grupo.MapGet("/irregulares", (Irregulares irregulares) => irregulares.Listar());
        grupo.MapPost("/irregulares", (PedidoIrregular pedido, Irregulares irregulares) =>
        {
            try
            {
                irregulares.Cadastrar(pedido.Documento);
                return Results.Created($"/_controle/irregulares/{pedido.Documento}", new { documento = pedido.Documento });
            }
            catch (ArgumentException erro)
            {
                return Results.BadRequest(new { erro = MensagemLimpa(erro) });
            }
        });
        grupo.MapDelete("/irregulares/{documento}", (string documento, Irregulares irregulares) =>
            irregulares.Remover(documento) ? Results.NoContent() : Results.NotFound());
        grupo.MapDelete("/irregulares", (Irregulares irregulares) =>
        {
            irregulares.Limpar();
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
