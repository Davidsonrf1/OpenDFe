using System.Security.Cryptography;
using System.Xml;

namespace OpenDFe;

/// <summary>
/// O núcleo de um leiaute, um documento e uma configuração (PROJETO.md 3.2): a porta das operações
/// (<see cref="ExecutarAsync{TRetorno}"/>) e a listagem do que o leiaute declara.
/// </summary>
/// <remarks>
/// <para>
/// Imutável e seguro para uso concorrente: um núcleo atende chamadas em paralelo. Criar é barato (o leiaute não
/// tem estado e os schemas são dele); entrar em contingência é criar outro núcleo com as mesmas
/// <see cref="ConfiguracaoDFe.Conexoes"/>.
/// </para>
/// <para>
/// Não é <see cref="IDisposable"/>: o certificado e as conexões são de quem os cria, e o núcleo só os referencia.
/// </para>
/// </remarks>
public sealed class DFe
{
    private readonly DeclaracaoLeiaute _declaracao;
    private readonly Dictionary<(Destino, Ambiente, string), EnderecoServico> _enderecosChave;
    private readonly ContextoOperacao _contexto;

    private DFe(ILeiaute leiaute, Documento documento, ConfiguracaoDFe configuracao, DeclaracaoLeiaute declaracao, EnderecoServico[] enderecos)
    {
        Leiaute = leiaute;
        Documento = documento;
        Configuracao = configuracao;
        _declaracao = declaracao;
        Enderecos = enderecos;
        _enderecosChave = enderecos.ToDictionary(e => e.Chave);
        _contexto = new ContextoOperacao(documento, configuracao);
    }

    /// <summary>
    /// Cria o núcleo: chama <see cref="ILeiaute.Declarar"/> e confere a configuração contra a declaração.
    /// </summary>
    /// <param name="leiaute">O leiaute.</param>
    /// <param name="documento">O documento, entre os que o leiaute atende.</param>
    /// <param name="configuracao">A configuração.</param>
    /// <returns>O núcleo.</returns>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    /// <exception cref="ArgumentException">
    /// O leiaute não atende o documento; um membro obrigatório da configuração é nulo ou tem valor desconhecido;
    /// a configuração do documento falta (quando o leiaute a declara obrigatória), é de outro tipo ou não é
    /// declarada pelo leiaute; a UF não tem atendimento no tipo de emissão; o tipo de emissão é diferente de
    /// <see cref="TipoEmissao.Normal"/> sem a <see cref="ConfiguracaoDFe.Contingencia"/>, ou é
    /// <see cref="TipoEmissao.Normal"/> com ela; uma URL trocada é nula ou repetida.
    /// </exception>
    /// <exception cref="CryptographicException">O certificado não tem a chave privada, ou ela não é RSA.</exception>
    /// <exception cref="ObjectDisposedException">As conexões já foram descartadas.</exception>
    /// <remarks>
    /// Não confere a validade nem o titular do certificado: um núcleo vive mais que um instante, e quem usa
    /// confere com <see cref="CertificadoDigital.Validade"/> e <see cref="CertificadoDigital.LerTitular"/>.
    /// </remarks>
    public static DFe Criar(ILeiaute leiaute, Documento documento, ConfiguracaoDFe configuracao)
    {
        ArgumentNullException.ThrowIfNull(leiaute);
        ArgumentNullException.ThrowIfNull(configuracao);
        ConferirConfiguracao(configuracao);
        var declaracao = DeclaracaoLeiaute.Criar(leiaute, documento);

        ConferirDocumento(declaracao, configuracao);
        if (!declaracao.TentarAtendimento(configuracao.UF, configuracao.Emissao, out _))
        {
            throw new ArgumentException(
                $"O leiaute não declara quem atende a UF {configuracao.UF} no tipo de emissão {configuracao.Emissao}.",
                nameof(configuracao));
        }
        var enderecos = Sobrepor(declaracao.Enderecos, configuracao);
        CertificadoDigital.ExigirChavePrivadaRsa(configuracao.Certificado);
        ObjectDisposedException.ThrowIf(configuracao.Conexoes.Descartado, configuracao.Conexoes);

        return new DFe(leiaute, documento, configuracao, declaracao, enderecos);
    }

    /// <summary>O leiaute.</summary>
    public ILeiaute Leiaute { get; }

    /// <summary>O documento.</summary>
    public Documento Documento { get; }

    /// <summary>A configuração recebida em <see cref="Criar"/>.</summary>
    public ConfiguracaoDFe Configuracao { get; }

    /// <summary>As operações que o leiaute registrou para o documento, na ordem do registro.</summary>
    public IReadOnlyList<OperacaoSuportada> Operacoes => _declaracao.Operacoes;

    /// <summary>Qual autorizador atende cada UF em cada tipo de emissão (a tabela do leiaute).</summary>
    public IReadOnlyList<Atendimento> Atendimentos => _declaracao.Atendimentos;

    /// <summary>
    /// As URLs efetivas: as do leiaute, na ordem dele, com as trocadas na configuração no lugar; depois, as
    /// acrescentadas pela configuração.
    /// </summary>
    public IReadOnlyList<EnderecoServico> Enderecos { get; }

    /// <summary>Se o leiaute registrou um handler para este tipo de parâmetro (o tipo exato).</summary>
    /// <typeparam name="TParametro">O parâmetro da operação.</typeparam>
    /// <returns>Se a operação é suportada.</returns>
    public bool Suporta<TParametro>()
        where TParametro : ParametroWS =>
        _declaracao.TentarExecutor(typeof(TParametro), out _);

    /// <summary>
    /// Executa uma operação: montar → recusa local → assinar → validar pelo schema → transmitir → ler.
    /// </summary>
    /// <typeparam name="TRetorno">O retorno, deduzido do parâmetro.</typeparam>
    /// <param name="parametro">
    /// O parâmetro da operação (<c>ExecutarAsync(new ConsultaStatus())</c> é
    /// <c>Task&lt;ResultadoWS&lt;RetornoStatus&gt;&gt;</c>).
    /// </param>
    /// <param name="cancellationToken">O cancelamento.</param>
    /// <returns>
    /// O resultado: <see cref="SituacaoResultado.Processado"/>, com o <c>cStat</c> (inclusive a rejeição), ou
    /// <see cref="SituacaoResultado.RecusadoLocalmente"/>, sem transmitir; com os problemas e o rastro.
    /// </returns>
    /// <exception cref="ArgumentNullException">O parâmetro é nulo.</exception>
    /// <exception cref="NotSupportedException">
    /// O leiaute não registrou a operação, ou não há URL para o autorizador, o ambiente e o serviço (nem
    /// atendimento para a UF no tipo de emissão).
    /// </exception>
    /// <exception cref="FalhaTransmissaoException">
    /// Falha de rede, TLS, HTTP, SOAP, tempo esgotado ou resposta ilegível, com o rastro e se o pedido pode ter
    /// sido processado.
    /// </exception>
    /// <exception cref="OperationCanceledException">Cancelado (sem o rastro).</exception>
    /// <exception cref="ObjectDisposedException">As conexões já foram descartadas.</exception>
    /// <remarks>
    /// <para>O caminho:</para>
    /// <list type="number">
    /// <item>o handler é achado pelo tipo exato do parâmetro;</item>
    /// <item>
    /// <see cref="IOperacao{TParametro, TRetorno}.Montar"/>; com um problema de severidade
    /// <see cref="Severidade.Erro"/>, <see cref="SituacaoResultado.RecusadoLocalmente"/>;
    /// </item>
    /// <item>cada <see cref="PedidoWS.IdsAssinatura"/> é assinado com o certificado, na ordem;</item>
    /// <item>
    /// a mensagem assinada é validada pelo <see cref="PedidoWS.Schema"/>; com erro,
    /// <see cref="SituacaoResultado.RecusadoLocalmente"/>;
    /// </item>
    /// <item>
    /// o destino é o <see cref="ParametroWS.Destino"/>, ou o <see cref="PedidoWS.Destino"/>, ou a UF da
    /// configuração; o tipo de emissão, o <see cref="PedidoWS.Emissao"/> ou o da configuração; uma UF vai ao
    /// autorizador do <see cref="Atendimento"/>; a URL é a do autorizador, do ambiente e do serviço, em
    /// <see cref="Enderecos"/>;
    /// </item>
    /// <item>a mensagem é transmitida pelas <see cref="ConfiguracaoDFe.Conexoes"/>;</item>
    /// <item><see cref="IOperacao{TParametro, TRetorno}.Ler"/> lê a resposta;</item>
    /// <item><see cref="SituacaoResultado.Processado"/>, com os avisos da montagem, do schema e da leitura.</item>
    /// </list>
    /// <para>
    /// As exceções de <see cref="IOperacao{TParametro, TRetorno}.Montar"/>, da assinatura (um <c>Id</c> que não
    /// existe na mensagem) e as de <see cref="IOperacao{TParametro, TRetorno}.Ler"/> que não são de leitura
    /// sobem como vieram: são erro de programação do leiaute. Cancelar depois de o pedido sair é, para o
    /// autorizador, a mesma queda no meio (PROJETO.md 8.6).
    /// </para>
    /// </remarks>
    public async Task<ResultadoWS<TRetorno>> ExecutarAsync<TRetorno>(
        ParametroWS<TRetorno> parametro, CancellationToken cancellationToken = default)
        where TRetorno : class
    {
        ArgumentNullException.ThrowIfNull(parametro);
        var tipo = parametro.GetType();
        if (!_declaracao.TentarExecutor(tipo, out var encontrado) || encontrado is not ExecutorOperacao<TRetorno> executor)
            throw new NotSupportedException($"O leiaute {Leiaute.Versao} não registrou a operação {tipo.Name} para {Documento}.");
        cancellationToken.ThrowIfCancellationRequested();

        var relogio = Configuracao.Relogio;
        var inicio = relogio.GetUtcNow();
        var marca = relogio.GetTimestamp();

        // 2. Montar; a recusa da montagem.
        var pedido = executor.Montar(parametro, _contexto)
            ?? throw new InvalidOperationException($"A operação {tipo.Name} montou um pedido nulo.");
        var problemas = new List<Problema>(pedido.Problemas);
        if (TemErro(pedido.Problemas))
            return Recusado(pedido.Xml.Length == 0 ? null : pedido.Xml);

        // 3. Assinar; 4. validar a mensagem assinada.
        var xml = pedido.Xml;
        foreach (var id in pedido.IdsAssinatura)
            xml = AssinaturaDigital.Assinar(xml, id, Configuracao.Certificado);
        var problemasSchema = pedido.Schema.Validar(xml);
        problemas.AddRange(problemasSchema);
        if (TemErro(problemasSchema))
            return Recusado(xml);

        // 5. Escolher a URL; 6. transmitir.
        var (autorizador, url) = EscolherUrl(parametro.Destino ?? pedido.Destino, pedido.Emissao, pedido.Servico);
        cancellationToken.ThrowIfCancellationRequested();
        var transporte = Configuracao.Conexoes.Transporte();
        var envio = new EnvioSefaz(Configuracao.Certificado, autorizador, url, pedido.Servico, xml);
        var marcaTransmissao = relogio.GetTimestamp();
        string xmlRetorno;
        try
        {
            xmlRetorno = await transporte.EnviarAsync(envio, cancellationToken).ConfigureAwait(false);
            if (xmlRetorno is null)
                throw new InvalidDataException("O transporte não devolveu a resposta.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            var rastroFalha = Rastro(xml, null, autorizador, url, relogio.GetElapsedTime(marcaTransmissao));
            throw new FalhaTransmissaoException(
                $"A transmissão de {pedido.Servico} a {autorizador.Sigla()} falhou: {e.Message}", rastroFalha, envio.Enviado, e);
        }
        var duracaoTransmissao = relogio.GetElapsedTime(marcaTransmissao);

        // 7. Ler; 8. o resultado.
        LeituraWS<TRetorno> leitura;
        try
        {
            leitura = executor.Ler(parametro, xmlRetorno, _contexto)
                ?? throw new InvalidOperationException($"A operação {tipo.Name} leu um retorno nulo.");
        }
        catch (Exception e) when (e is XmlException or FormatException)
        {
            throw new FalhaTransmissaoException(
                $"A resposta de {pedido.Servico} de {autorizador.Sigla()} não pôde ser lida: {e.Message}",
                Rastro(xml, xmlRetorno, autorizador, url, duracaoTransmissao), podeTerSidoProcessado: true, e);
        }
        problemas.AddRange(leitura.Problemas);
        return ResultadoWS<TRetorno>.CriarProcessado(leitura, problemas.ToArray(), Rastro(xml, xmlRetorno, autorizador, url, duracaoTransmissao));

        ResultadoWS<TRetorno> Recusado(string? xmlPedido) =>
            ResultadoWS<TRetorno>.CriarRecusado(problemas.ToArray(), Rastro(xmlPedido, null, null, null, null));

        RastroWS Rastro(string? xmlPedido, string? retorno, Destino? autorizadorEscolhido, Uri? urlEscolhida, TimeSpan? transmissao) =>
            new(xmlPedido, retorno, autorizadorEscolhido, urlEscolhida, inicio, relogio.GetElapsedTime(marca), transmissao);
    }

    // O passo 5: destino (parâmetro, pedido, UF do núcleo) × tipo de emissão (pedido, núcleo) → autorizador → URL.
    private (Destino Autorizador, Uri Url) EscolherUrl(Destino? destinoEscolhido, TipoEmissao? emissaoPedido, string servico)
    {
        var destino = destinoEscolhido ?? Configuracao.UF.ComoDestino();
        var emissao = emissaoPedido ?? Configuracao.Emissao;
        var autorizador = destino;
        if (destino.TentarUF(out var uf))
        {
            if (!_declaracao.TentarAtendimento(uf, emissao, out var atendimento))
                throw new NotSupportedException($"O leiaute não declara quem atende a UF {uf} no tipo de emissão {emissao}.");
            autorizador = atendimento!.Autorizador;
        }
        if (!_enderecosChave.TryGetValue((autorizador, Configuracao.Ambiente, servico), out var endereco))
            throw new NotSupportedException($"Não há URL de {servico} em {autorizador.Sigla()}, {Configuracao.Ambiente}.");
        return (autorizador, endereco.Url);
    }

    private static bool TemErro(IReadOnlyList<Problema> problemas) =>
        problemas.Any(p => p.Severidade == Severidade.Erro);

    private static void ConferirConfiguracao(ConfiguracaoDFe configuracao)
    {
        const string Nome = nameof(configuracao);
        if (!Enum.IsDefined(configuracao.Ambiente))
            throw new ArgumentException($"O ambiente {configuracao.Ambiente} não é conhecido.", Nome);
        if (!Enum.IsDefined(configuracao.UF))
            throw new ArgumentException($"A UF {configuracao.UF} não é conhecida.", Nome);
        if (!Enum.IsDefined(configuracao.Emissao))
            throw new ArgumentException($"O tipo de emissão {configuracao.Emissao} não é conhecido.", Nome);
        if (configuracao.Certificado is null)
            throw new ArgumentException("A configuração não tem o certificado.", Nome);
        if (configuracao.Conexoes is null)
            throw new ArgumentException("A configuração não tem as conexões.", Nome);
        if (configuracao.Relogio is null)
            throw new ArgumentException("A configuração não tem o relógio.", Nome);
        if (configuracao.Urls is null)
            throw new ArgumentException("A lista de URLs da configuração é nula.", Nome);
        if (configuracao.Emissao != TipoEmissao.Normal && configuracao.Contingencia is null)
            throw new ArgumentException($"O tipo de emissão {configuracao.Emissao} exige a entrada em contingência (Contingencia).", Nome);
        if (configuracao.Emissao == TipoEmissao.Normal && configuracao.Contingencia is not null)
            throw new ArgumentException("A entrada em contingência (Contingencia) só vale com um tipo de emissão de contingência.", Nome);
    }

    private static void ConferirDocumento(DeclaracaoLeiaute declaracao, ConfiguracaoDFe configuracao)
    {
        const string Nome = nameof(configuracao);
        var documento = configuracao.Documento;
        var tipo = declaracao.TipoConfiguracaoDocumento;
        if (documento is null)
        {
            if (tipo is not null && declaracao.ConfiguracaoDocumentoObrigatoria)
                throw new ArgumentException($"O documento {declaracao.Documento} exige a configuração do documento ({tipo.Name}).", Nome);
            return;
        }
        if (tipo is null)
            throw new ArgumentException($"O leiaute não declara configuração para o documento {declaracao.Documento}.", Nome);
        if (!tipo.IsInstanceOfType(documento))
            throw new ArgumentException($"A configuração do documento {declaracao.Documento} é {tipo.Name}, não {documento.GetType().Name}.", Nome);
    }

    // As URLs do leiaute, com as trocadas no lugar e as acrescentadas no fim.
    private static EnderecoServico[] Sobrepor(IReadOnlyList<EnderecoServico> leiaute, ConfiguracaoDFe configuracao)
    {
        const string Nome = nameof(configuracao);
        var efetivas = new List<EnderecoServico>(leiaute);
        var indices = new Dictionary<(Destino, Ambiente, string), int>();
        for (var i = 0; i < efetivas.Count; i++)
            indices[efetivas[i].Chave] = i;

        var vistas = new HashSet<(Destino, Ambiente, string)>();
        foreach (var endereco in configuracao.Urls.ToArray())
        {
            if (endereco is null)
                throw new ArgumentException("Uma das URLs trocadas é nula.", Nome);
            if (!vistas.Add(endereco.Chave))
            {
                throw new ArgumentException(
                    $"A URL de {endereco.Servico} em {endereco.Autorizador.Sigla()}, {endereco.Ambiente}, está repetida na configuração.",
                    Nome);
            }
            if (indices.TryGetValue(endereco.Chave, out var indice))
                efetivas[indice] = endereco;
            else
                efetivas.Add(endereco);
        }
        return [.. efetivas];
    }
}
