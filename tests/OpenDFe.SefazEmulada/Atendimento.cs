using System.Net.Http.Headers;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O atendimento de POST /<destino>/<serviço>: as falhas de transporte comandadas, o envelope SOAP 1.2, o
// limite de tamanho (Anexo I, B01, 214) e o serviço.
internal sealed partial class Atendimento(Controle controle, OpcoesEmulador opcoes, IHostApplicationLifetime vida, ILogger<Atendimento> registro)
{
    // B01: o XML de dados até 500 KB. Medido no corpo inteiro do pedido, com o envelope.
    public const int LimiteDados = 500 * 1024;

    public static IReadOnlyList<Servico> Servicos { get; } = [new StatusServico()];

    public static Servico? Procurar(Autorizador autorizador, string nome) =>
        Servicos.FirstOrDefault(s => string.Equals(s.Nome, nome, StringComparison.OrdinalIgnoreCase)
            && autorizador.Servicos.Contains(s.Nome, StringComparer.Ordinal));

    public async Task AtenderAsync(HttpContext contexto, string destino, string nomeServico)
    {
        var autorizador = Autorizador.Procurar(destino);
        var servico = autorizador is null ? null : Procurar(autorizador, nomeServico);
        if (autorizador is null || servico is null)
        {
            contexto.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        if (!MediaTypeHeaderValue.TryParse(contexto.Request.ContentType, out var tipo)
            || !string.Equals(tipo.MediaType, EnvelopeSoap.TipoConteudo, StringComparison.OrdinalIgnoreCase))
        {
            contexto.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            return;
        }

        var (transporte, paralisacao) = controle.Consumir(autorizador.Sigla, servico.Nome);
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(contexto.RequestAborted, vida.ApplicationStopping);
        var ct = cancelamento.Token;
        switch (transporte)
        {
            case { Tipo: TipoFalha.Atraso, Milissegundos: { } milissegundos }:
                await Task.Delay(TimeSpan.FromMilliseconds(milissegundos), opcoes.Relogio, ct);
                break;
            case { Tipo: TipoFalha.TempoEsgotado }:
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, opcoes.Relogio, ct);
                }
                catch (OperationCanceledException)
                {
                }
                contexto.Abort();
                return;
            case { Tipo: TipoFalha.ConexaoDerrubada, Depois: false }:
                contexto.Abort();
                return;
        }

        using var memoria = new MemoryStream();
        await contexto.Request.Body.CopyToAsync(memoria, ct);
        var (status, resposta, situacao) = Responder(memoria.ToArray(), autorizador, servico, paralisacao);
        switch (transporte?.Tipo)
        {
            case TipoFalha.ConexaoDerrubada:
                Registrar(autorizador.Sigla, servico.Nome, situacao + ", e a conexão derrubada");
                contexto.Abort();
                return;
            case TipoFalha.RespostaIlegivel:
                resposta = resposta[..(resposta.Length / 2)];
                situacao += ", com a resposta cortada";
                break;
            case TipoFalha.FalhaSoap:
                status = StatusCodes.Status500InternalServerError;
                resposta = EnvelopeSoap.Falha(CodigoFalha.Receptor, "Falha comandada pelo controle do emulador.");
                situacao = "falha SOAP comandada";
                break;
        }

        Registrar(autorizador.Sigla, servico.Nome, situacao);
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = EnvelopeSoap.TipoConteudo + "; charset=utf-8";
        await contexto.Response.Body.WriteAsync(resposta, ct);
    }

    private (int Status, byte[] Resposta, string Situacao) Responder(byte[] corpo, Autorizador autorizador, Servico servico, FalhaComandada? paralisacao)
    {
        var pedido = new Pedido(autorizador, opcoes.Relogio.GetUtcNow(),
            paralisacao is null ? null : new Paralisacao(paralisacao.Codigo!, paralisacao.Observacao));
        try
        {
            XElement retorno;
            if (corpo.Length > LimiteDados)
            {
                retorno = servico.Rejeitar(Motivos.TamanhoExcedido, pedido);
            }
            else
            {
                var dados = EnvelopeSoap.LerDados(corpo, servico.NamespaceWsdl).Elements().FirstOrDefault();
                pedido = pedido with { Prefixo = EnvelopeSoap.UsaPrefixo(corpo, servico.NamespaceWsdl) };
                retorno = dados is null ? servico.Rejeitar(Motivos.RaizInexistente, pedido) : servico.Processar(dados, pedido);
            }
            var codigo = retorno.Element(Servico.NamespaceNFe + "cStat")?.Value;
            return (StatusCodes.Status200OK, EnvelopeSoap.Resultado(servico.NamespaceWsdl, retorno), "cStat " + codigo);
        }
        catch (FalhaSoapException falha)
        {
            var status = falha.Codigo == CodigoFalha.Remetente ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
            return (status, EnvelopeSoap.Falha(falha.Codigo, falha.Message), "falha SOAP: " + falha.Message);
        }
    }

    private void Registrar(string destino, string servico, string situacao)
    {
        if (opcoes.Registrar)
            RegistrarPedido(registro, destino, servico, situacao);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Destino}/{Servico}: {Situacao}")]
    private static partial void RegistrarPedido(ILogger registro, string destino, string servico, string situacao);
}
