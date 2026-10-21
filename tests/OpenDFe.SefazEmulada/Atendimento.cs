using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O atendimento de POST /<destino>/<serviço>: as falhas de transporte comandadas, o envelope SOAP 1.2, o
// limite de tamanho (Anexo I, B01, 214) e o serviço. Cada pedido fica nos PedidosRecentes, com o XML do pedido e
// da resposta, também o que o emulador não atende (404, 415) e o que as falhas comandadas derrubam.
internal sealed partial class Atendimento(Controle controle, Repositorio repositorio, CodigosSeguranca codigos, Irregulares irregulares, Cadastros cadastros,
    PedidosRecentes recentes, OpcoesEmulador opcoes, IHostApplicationLifetime vida, ILogger<Atendimento> registro)
{
    // B01: o XML de dados até 500 KB, o padrão dos serviços (Servico.Limite). Medido no corpo inteiro do
    // pedido, com o envelope.
    public const int LimiteDados = 500 * 1024;

    public static IReadOnlyList<Servico> Servicos { get; } = [
        new StatusServico(), new Autorizacao(), new RetAutorizacao(), new ConsultaProtocolo(), new Inutilizacao(), new RecepcaoEvento(),
        new Distribuicao(), new ConsultaCadastro(),
    ];

    public static Servico? Procurar(Autorizador autorizador, string nome) =>
        Servicos.FirstOrDefault(s => string.Equals(s.Nome, nome, StringComparison.OrdinalIgnoreCase)
            && autorizador.Servicos.Contains(s.Nome, StringComparer.Ordinal) && s.Atende(autorizador));

    public async Task AtenderAsync(HttpContext contexto, string destino, string nomeServico)
    {
        var inicio = opcoes.Relogio.GetTimestamp();
        var recebimento = opcoes.Relogio.GetUtcNow();
        var transmissor = Transmissor(contexto.Connection.ClientCertificate);
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(contexto.RequestAborted, vida.ApplicationStopping);
        var ct = cancelamento.Token;
        using var memoria = new MemoryStream();
        await contexto.Request.Body.CopyToAsync(memoria, ct);
        var corpo = memoria.ToArray();
        void Guardar(int? status, byte[]? resposta, string situacao) =>
            recentes.Registrar(destino, nomeServico, recebimento, transmissor, corpo, status, resposta, situacao, opcoes.Relogio.GetElapsedTime(inicio));

        var autorizador = Autorizador.Procurar(destino);
        var servico = autorizador is null ? null : Procurar(autorizador, nomeServico);
        if (autorizador is null || servico is null)
        {
            Guardar(StatusCodes.Status404NotFound, null, "HTTP 404: o emulador não atende o destino ou o serviço");
            contexto.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        (destino, nomeServico) = (autorizador.Sigla, servico.Nome);
        if (!MediaTypeHeaderValue.TryParse(contexto.Request.ContentType, out var tipo)
            || !string.Equals(tipo.MediaType, EnvelopeSoap.TipoConteudo, StringComparison.OrdinalIgnoreCase))
        {
            Guardar(StatusCodes.Status415UnsupportedMediaType, null, "HTTP 415: o Content-Type não é o do SOAP 1.2");
            contexto.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            return;
        }

        // A marca no infCpl passa na frente da falha do controle da mesma espécie (Marca).
        MarcaFalha? marca;
        try
        {
            marca = servico.Nome == Servico.NomeAutorizacao && corpo.Length <= servico.Limite ? Marca.Ler(corpo) : null;
        }
        catch (FalhaSoapException falha)
        {
            var envelope = EnvelopeSoap.Falha(falha.Codigo, falha.Message);
            Registrar(autorizador.Sigla, servico.Nome, "falha SOAP: " + falha.Message);
            Guardar(StatusCodes.Status400BadRequest, envelope, "falha SOAP: " + falha.Message);
            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, envelope, ct);
            return;
        }
        var (transporte, paralisacao, processamento) = controle.Consumir(autorizador.Sigla, servico.Nome,
            transporte: marca?.Transporte is null, paralisacao: marca?.Paralisacao is null);
        transporte = marca?.Transporte ?? transporte;
        paralisacao = marca?.Paralisacao ?? paralisacao;

        switch (transporte)
        {
            case { Tipo: TipoFalha.Atraso, Milissegundos: { } milissegundos }:
                await Task.Delay(TimeSpan.FromMilliseconds(milissegundos), opcoes.Relogio, ct);
                break;
            case { Tipo: TipoFalha.TempoEsgotado }:
                Guardar(null, null, "tempo esgotado comandado: sem resposta");
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
                Guardar(null, null, "conexão derrubada comandada, antes de processar");
                contexto.Abort();
                return;
        }

        var pedido = new Pedido(autorizador, opcoes.Relogio.GetUtcNow(),
            paralisacao is null ? null : new Paralisacao(paralisacao.Codigo!, paralisacao.Observacao),
            Transmissor: transmissor,
            Repositorio: repositorio,
            Processando: processamento is not null,
            Pendentes: marca?.Pendentes ?? 0,
            Codigos: codigos,
            Irregulares: irregulares,
            Cadastros: cadastros);
        var (status, resposta, situacao) = Responder(corpo, servico, pedido);
        switch (transporte?.Tipo)
        {
            case TipoFalha.ConexaoDerrubada:
                Registrar(autorizador.Sigla, servico.Nome, situacao + ", e a conexão derrubada");
                // A resposta que sairia fica guardada: é a que a recuperação precisa achar pela consulta.
                Guardar(null, resposta, situacao + ", e a conexão derrubada antes da resposta");
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
        Guardar(status, resposta, situacao);
        await ResponderAsync(contexto, status, resposta, ct);
    }

    private static async Task ResponderAsync(HttpContext contexto, int status, byte[] resposta, CancellationToken ct)
    {
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = EnvelopeSoap.TipoConteudo + "; charset=utf-8";
        await contexto.Response.Body.WriteAsync(resposta, ct);
    }

    // Quem transmitiu (Visão Geral, 4.3.3): o CNPJ ou o CPF do certificado do TLS, pela leitura da lib (13.5:
    // o que é neutro); sem os campos da ICP-Brasil, a impressão digital, para o recibo continuar sendo só dele.
    private static string Transmissor(X509Certificate2? certificado)
    {
        if (certificado is null)
            return "";
        var titular = CertificadoDigital.LerTitular(certificado);
        return titular?.Cnpj ?? titular?.Cpf ?? certificado.Thumbprint;
    }

    private static (int Status, byte[] Resposta, string Situacao) Responder(byte[] corpo, Servico servico, Pedido pedido)
    {
        try
        {
            XElement retorno;
            if (corpo.Length > servico.Limite)
            {
                retorno = servico.Rejeitar(Motivos.TamanhoExcedido, pedido);
            }
            else
            {
                var dados = EnvelopeSoap.LerDados(corpo, servico.NamespaceWsdl, servico.Operacao).Elements().FirstOrDefault();
                pedido = pedido with { Prefixo = EnvelopeSoap.UsaPrefixo(corpo, servico.NamespaceWsdl) };
                retorno = dados is null ? servico.Rejeitar(servico.RaizAusente, pedido) : servico.Processar(dados, pedido);
            }
            // O cStat da raiz ou, na consulta cadastro, o do infCons.
            var codigo = retorno.Descendants(Servico.NamespaceNFe + "cStat").FirstOrDefault()?.Value;
            return (StatusCodes.Status200OK, EnvelopeSoap.Resultado(servico.NamespaceWsdl, servico.Operacao, retorno), "cStat " + codigo);
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
