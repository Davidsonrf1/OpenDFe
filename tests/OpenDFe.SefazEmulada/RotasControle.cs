using System.Net;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// A rota /_controle (PROJETO.md 13.5), aceita só do próprio computador, na porta dos Web Services e na do painel
// (E.6). Toda mudança (o que não é GET) avisa o painel pelo fluxo. As rotas:
//   GET    /_controle/emulador      o ac.crt, o endereço, o painel, as URLs e quantos pedidos ficam guardados
//   GET    /_controle/ac.crt        o certificado da AC de teste, em PEM
//   GET    /_controle/autorizadores os autorizadores: as UFs, os serviços, a consulta cadastro e a SVC de cada UF
//   DELETE /_controle               zera tudo: as falhas, as notas e a distribuição, os CSCs, os irregulares, os
//                                   contribuintes e os pedidos recentes
//   GET    /_controle/pedidos       os pedidos recentes, sem o XML, do mais antigo ao mais novo (?depois=<id>)
//   GET    /_controle/pedidos/{id}  um pedido, com o XML do pedido e da resposta; 404 se já saiu da fila
//   GET    /_controle/pedidos/fluxo os avisos ao vivo (text/event-stream): pedido (com o resumo) e estado
//   DELETE /_controle/pedidos       esquece os pedidos recentes
//   GET    /_controle/eventos       os eventos registrados, na ordem
//   GET    /_controle/inutilizacoes as faixas inutilizadas, na ordem
//   GET    /_controle/notas/{chave} a nota (o nfeProc) com os eventos (o procEventoNFe de cada um); 404 se não está
//   GET    /_controle/falhas        as falhas ativas
//   POST   /_controle/falhas        acrescenta uma (PedidoFalha em JSON); 201 com a falha, 400 com o motivo
//   DELETE /_controle/falhas/{id}   remove uma; 404 se não existe
//   DELETE /_controle/falhas        remove todas
//   GET    /_controle/notas         as notas autorizadas, na ordem
//   DELETE /_controle/notas         esquece as notas, os lotes, os eventos, as faixas inutilizadas e a distribuição
//   GET    /_controle/distribuicao  os documentos que o AN distribui a cada interessado, com o NSU
//   GET    /_controle/csc           os CSCs cadastrados e revogados
//   POST   /_controle/csc           cadastra um (PedidoCsc em JSON); 201 com o CSC, 400 com o motivo
//   DELETE /_controle/csc/{uf}/{cnpj}/{id}   revoga um; 404 se não existe
//   DELETE /_controle/csc           esquece os cadastrados e as revogações (fica só o padrão)
//   GET    /_controle/irregulares   os CNPJs e CPFs em situação irregular
//   POST   /_controle/irregulares   cadastra um (PedidoIrregular em JSON); 201 com o documento, 400 com o motivo
//   DELETE /_controle/irregulares/{documento}   tira um; 404 se não está
//   DELETE /_controle/irregulares   tira todos
//   GET    /_controle/cadastros     os contribuintes da consulta cadastro
//   POST   /_controle/cadastros     cadastra um (Contribuinte em JSON) ou troca o da mesma UF e IE; 201 ou 200 com
//                                   ele, 400 com o motivo
//   DELETE /_controle/cadastros/{uf}/{ie}   tira um; 404 se não está
//   DELETE /_controle/cadastros     tira todos
internal static class RotasControle
{
    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup("/_controle").AddEndpointFilter(async (contexto, proximo) =>
        {
            var http = contexto.HttpContext;
            if (!Local(http.Connection.RemoteIpAddress))
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            var resultado = await proximo(contexto);
            if (!HttpMethods.IsGet(http.Request.Method))
                http.RequestServices.GetRequiredService<PedidosRecentes>().AvisarEstado();
            return resultado;
        });

        grupo.MapGet("/emulador", (SituacaoEmulador situacao, OpcoesEmulador opcoes) => new
        {
            autoridade = situacao.ArquivoAutoridade,
            endereco = situacao.Endereco,
            painel = situacao.Painel,
            pedidos = opcoes.Pedidos,
            urls = situacao.Urls,
        });
        grupo.MapGet("/ac.crt", (SituacaoEmulador situacao) =>
            Results.File(situacao.ArquivoAutoridade, "application/x-pem-file", AutoridadeTeste.ArquivoCertificado));
        grupo.MapGet("/autorizadores", () => Autorizador.Todos.Select(a => new
        {
            sigla = a.Sigla,
            casa = a.CodigoCasa,
            tipoEmissao = a.TipoEmissao,
            atendidas = a.Atendidas.Select(c => UnidadesFederativas.Sigla(c)),
            cadastro = a.Cadastro.Select(c => UnidadesFederativas.Sigla(c)),
            servicos = Atendimento.Servicos.Where(s => Atendimento.Procurar(a, s.Nome) is not null).Select(s => s.Nome),
            // A SVC que atende cada UF do autorizador normal, quando ele está paralisado (Anexo III, 2.1.3).
            contingencia = a.Atendidas.Where(_ => a.TipoEmissao is null)
                .Select(c => new { uf = UnidadesFederativas.Sigla(c), svc = Autorizador.Todos.FirstOrDefault(s => s.TipoEmissao is not null && s.Atende(c))?.Sigla })
                .Where(p => p.svc is not null),
        }));
        grupo.MapDelete("", (Controle controle, Repositorio repositorio, CodigosSeguranca codigos, Irregulares irregulares, Cadastros cadastros, PedidosRecentes recentes) =>
        {
            controle.Limpar();
            repositorio.Limpar();
            codigos.Limpar();
            irregulares.Limpar();
            cadastros.Limpar();
            recentes.Limpar();
            return Results.NoContent();
        });
        grupo.MapGet("/pedidos", (long? depois, PedidosRecentes recentes) => recentes.Listar(depois));
        grupo.MapGet("/pedidos/{id:long}", (long id, PedidosRecentes recentes) =>
            recentes.Ler(id) is { } pedido ? Results.Ok(pedido) : Results.NotFound());
        grupo.MapGet("/pedidos/fluxo", (HttpContext contexto, PedidosRecentes recentes, IHostApplicationLifetime vida) => FluxoAsync(contexto, recentes, vida));
        grupo.MapDelete("/pedidos", (PedidosRecentes recentes) =>
        {
            recentes.Limpar();
            return Results.NoContent();
        });
        grupo.MapGet("/eventos", (Repositorio repositorio) => repositorio.EventosRegistrados());
        grupo.MapGet("/inutilizacoes", (Repositorio repositorio) => repositorio.Faixas());
        grupo.MapGet("/notas/{chave}", (string chave, Repositorio repositorio) =>
        {
            if (repositorio.Detalhe(chave) is not { } detalhe)
                return Results.NotFound();
            var (nota, eventos) = detalhe;
            return Results.Ok(new
            {
                nota,
                xml = Interessados.Processada(nota).ToString(),
                eventos = eventos.Select(e => new { evento = e, xml = e.Processado.ToString() }),
            });
        });

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
        grupo.MapGet("/distribuicao", (Repositorio repositorio) => repositorio.Distribuidos());
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
        grupo.MapGet("/cadastros", (Cadastros cadastros) => cadastros.Listar());
        grupo.MapPost("/cadastros", (Contribuinte contribuinte, Cadastros cadastros) =>
        {
            try
            {
                var novo = cadastros.Cadastrar(contribuinte);
                var gravado = cadastros.Procurar(contribuinte.Uf!.ToUpperInvariant(), contribuinte.Ie, null, null).First(c => c.Ie == contribuinte.Ie);
                return novo ? Results.Created($"/_controle/cadastros/{gravado.Uf}/{gravado.Ie}", gravado) : Results.Ok(gravado);
            }
            catch (ArgumentException erro)
            {
                return Results.BadRequest(new { erro = MensagemLimpa(erro) });
            }
        });
        grupo.MapDelete("/cadastros/{uf}/{ie}", (string uf, string ie, Cadastros cadastros) =>
            cadastros.Remover(uf, ie) ? Results.NoContent() : Results.NotFound());
        grupo.MapDelete("/cadastros", (Cadastros cadastros) =>
        {
            cadastros.Limpar();
            return Results.NoContent();
        });
    }

    // O fluxo de avisos (Server-Sent Events): um evento "pedido" com o resumo de cada pedido novo, um "estado" a cada
    // mudança pelo controle e um comentário a cada 15 segundos, para a conexão parada não cair. Vai até o navegador
    // fechar ou o emulador parar.
    private static async Task FluxoAsync(HttpContext contexto, PedidosRecentes recentes, IHostApplicationLifetime vida)
    {
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(contexto.RequestAborted, vida.ApplicationStopping);
        var ct = cancelamento.Token;
        var leitor = recentes.Ouvir(ct);
        contexto.Response.ContentType = "text/event-stream; charset=utf-8";
        contexto.Response.Headers.CacheControl = "no-store";
        var opcoes = contexto.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>().Value.SerializerOptions;
        try
        {
            await contexto.Response.WriteAsync(": aberto\n\n", ct);
            await contexto.Response.Body.FlushAsync(ct);
            while (!ct.IsCancellationRequested)
            {
                using var espera = CancellationTokenSource.CreateLinkedTokenSource(ct);
                espera.CancelAfter(TimeSpan.FromSeconds(15));
                string texto;
                try
                {
                    var aviso = await leitor.ReadAsync(espera.Token);
                    texto = $"event: {aviso.Tipo}\ndata: {(aviso.Pedido is null ? "{}" : JsonSerializer.Serialize(aviso.Pedido, opcoes))}\n\n";
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    texto = ": ping\n\n";
                }
                await contexto.Response.WriteAsync(texto, ct);
                await contexto.Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (System.Threading.Channels.ChannelClosedException)
        {
        }
    }

    // O próprio computador: o loopback, também o IPv4 mapeado em IPv6.
    public static bool Local(IPAddress? endereco) =>
        endereco is not null && IPAddress.IsLoopback(endereco.IsIPv4MappedToIPv6 ? endereco.MapToIPv4() : endereco);

    // A mensagem da ArgumentException sem o " (Parameter 'pedido')" que o .NET acrescenta.
    private static string MensagemLimpa(ArgumentException erro) =>
        erro.ParamName is null ? erro.Message : erro.Message.Replace($" (Parameter '{erro.ParamName}')", "", StringComparison.Ordinal);
}
