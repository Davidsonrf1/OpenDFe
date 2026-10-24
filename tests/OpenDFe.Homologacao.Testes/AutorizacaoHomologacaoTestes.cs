using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A autorização da NF-e pela lib inteira na homologação real (etapa 4.4), e a consulta e a recuperação dela (4.5): uma
// nota do Simples Nacional, de venda a consumidor final em MG, com o emitente do PFX e a inscrição estadual em
// OPENDFE_EMITENTE_IE. Pulado sem o PFX: até ele chegar, as etapas fecham na SEFAZ emulada (AutorizacaoEmuladaTestes,
// QuedaEmuladaTestes, PROJETO.md 17). Outro regime ou outra UF pedem o item e o endereço ajustados.
//
// A NFC-e (etapa 5.1) em MG, com o QR Code na versão 3 (nada a configurar) e na versão 2, esta com o CSC de homologação
// da empresa em OPENDFE_CSC_ID e OPENDFE_CSC (PROJETO.md 13.3); pulados sem o PFX, e a versão 2 também sem o CSC.
public class AutorizacaoHomologacaoTestes
{
    private const string VariavelInscricao = "OPENDFE_EMITENTE_IE";
    private const string VariavelIdCsc = "OPENDFE_CSC_ID";
    private const string VariavelCsc = "OPENDFE_CSC";

    [Fact]
    public async Task UmaNFeEAutorizadaConsultadaERecuperadaEmHomologacao()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var inscricao = Environment.GetEnvironmentVariable(VariavelInscricao);
        Assert.SkipWhen(string.IsNullOrEmpty(inscricao), $"Sem {VariavelInscricao}: a nota precisa da inscrição estadual do emitente.");
        var cnpj = CertificadoDigital.LerTitular(certificado)?.Cnpj;
        Assert.SkipWhen(cnpj is null, "O PFX não é de um CNPJ.");
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
        });

        var resultado = await dfe.ExecutarAsync(new Autorizacao(Nota(cnpj!, inscricao!)), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.True(nota.Autorizada, $"{nota.Protocolo?.Status} {nota.Protocolo?.Motivo}");
        Assert.NotNull(nota.XmlProcessado);

        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), TestContext.Current.CancellationToken);
        Assert.True(consulta.Processado, string.Join(Environment.NewLine, consulta.Problemas));
        Assert.True(consulta.Retorno.Autorizada, $"{consulta.CStat} {consulta.Motivo}");
        Assert.Equal(nota.Protocolo!.Numero, consulta.Retorno.Protocolo?.Numero);

        var recuperada = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(nota.Xml!), TestContext.Current.CancellationToken);
        Assert.True(recuperada.Processado, string.Join(Environment.NewLine, recuperada.Problemas));
        Assert.NotNull(Assert.Single(recuperada.Retorno.Notas).XmlProcessado);
    }

    // O cancelamento da nota que o teste acima autoriza e a inutilização de uma faixa (etapa 6.1): pulados sem o PFX.
    [Fact]
    public async Task UmaNFeAutorizadaECanceladaEmHomologacao()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var inscricao = Environment.GetEnvironmentVariable(VariavelInscricao);
        Assert.SkipWhen(string.IsNullOrEmpty(inscricao), $"Sem {VariavelInscricao}: a nota precisa da inscrição estadual do emitente.");
        var cnpj = CertificadoDigital.LerTitular(certificado)?.Cnpj;
        Assert.SkipWhen(cnpj is null, "O PFX não é de um CNPJ.");
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
        });
        var autorizacao = await dfe.ExecutarAsync(new Autorizacao(Nota(cnpj!, inscricao!)), TestContext.Current.CancellationToken);
        var nota = Assert.Single(autorizacao.Retorno!.Notas);
        Assert.True(nota.Autorizada, $"{nota.Protocolo?.Status} {nota.Protocolo?.Motivo}");

        var cancelamento = await dfe.ExecutarAsync(
            new EnvioEvento(new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, "Cancelamento do teste de homologacao")),
            TestContext.Current.CancellationToken);

        Assert.True(cancelamento.Processado, string.Join(Environment.NewLine, cancelamento.Problemas));
        var evento = Assert.Single(cancelamento.Retorno!.Eventos);
        Assert.True(evento.Registrado, $"{evento.Status} {evento.Motivo}");
        Assert.NotNull(evento.XmlProcessado);
    }

    [Fact]
    public async Task UmaFaixaEInutilizadaEmHomologacao()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var cnpj = CertificadoDigital.LerTitular(certificado)?.Cnpj;
        Assert.SkipWhen(cnpj is null, "O PFX não é de um CNPJ.");
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
        });
        // A faixa sorteada (a homologação guarda as inutilizações de todas as execuções) na série 99.
        var inicio = Random.Shared.Next(1, 900_000_000);

        var resultado = await dfe.ExecutarAsync(
            new Inutilizacao(cnpj!, 99, inicio, inicio + 1, "Inutilizacao do teste de homologacao"), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        Assert.True(resultado.Retorno!.Homologada, $"{resultado.Retorno.Status} {resultado.Retorno.Motivo}");
        Assert.NotNull(resultado.Retorno.XmlProcessado);
    }

    [Fact]
    public Task UmaNFCeEAutorizadaEmHomologacaoNaVersao3() => AutorizarNFCe(versao2: false);

    [Fact]
    public Task UmaNFCeEAutorizadaEmHomologacaoNaVersao2ComOCsc() => AutorizarNFCe(versao2: true);

    // A NFC-e emitida offline (tpEmis 9) por um núcleo, guardada como XML e transmitida depois por outro, o normal (etapa 5.2).
    [Fact]
    public Task UmaNFCeOfflineETransmitidaDepoisEAutorizadaEmHomologacaoNaVersao3() => AutorizarNFCe(versao2: false, offline: true);

    [Fact]
    public Task UmaNFCeOfflineETransmitidaDepoisEAutorizadaEmHomologacaoNaVersao2ComOCsc() => AutorizarNFCe(versao2: true, offline: true);

    private static async Task AutorizarNFCe(bool versao2, bool offline = false)
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var inscricao = Environment.GetEnvironmentVariable(VariavelInscricao);
        Assert.SkipWhen(string.IsNullOrEmpty(inscricao), $"Sem {VariavelInscricao}: a nota precisa da inscrição estadual do emitente.");
        var idCsc = Environment.GetEnvironmentVariable(VariavelIdCsc);
        var csc = Environment.GetEnvironmentVariable(VariavelCsc);
        Assert.SkipWhen(versao2 && (!int.TryParse(idCsc, out _) || string.IsNullOrEmpty(csc)),
            $"Sem {VariavelIdCsc} e {VariavelCsc}: a versão 2 do QR Code precisa do CSC de homologação da empresa.");
        var cnpj = CertificadoDigital.LerTitular(certificado)?.Cnpj;
        Assert.SkipWhen(cnpj is null, "O PFX não é de um CNPJ.");
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        using var conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            return handler;
        });
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
            Documento = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(int.Parse(idCsc!, System.Globalization.CultureInfo.InvariantCulture), csc!)) : null,
        });

        var nota = NotaNFCe(cnpj!, inscricao!);
        ParametroWS<RetornoAutorizacao> parametro = new Autorizacao(nota);
        if (offline)
        {
            var nucleoOffline = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
            {
                Ambiente = Ambiente.Homologacao,
                UF = UF.MG,
                Certificado = certificado,
                Conexoes = conexoes,
                Emissao = TipoEmissao.ContingenciaOffline,
                Contingencia = new Contingencia(DateTimeOffset.UtcNow.AddHours(-2), "Teste de homologacao da contingencia offline"),
                Documento = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(int.Parse(idCsc!, System.Globalization.CultureInfo.InvariantCulture), csc!)) : null,
            });
            var preparado = nucleoOffline.Preparar(nota);
            Assert.True(preparado.Preparado, string.Join(Environment.NewLine, preparado.Problemas));
            parametro = new Autorizacao(preparado.Xml!);
        }

        var resultado = await dfe.ExecutarAsync(parametro, TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join(Environment.NewLine, resultado.Problemas));
        var autorizada = Assert.Single(resultado.Retorno.Notas);
        Assert.True(autorizada.Autorizada, $"{autorizada.Protocolo?.Status} {autorizada.Protocolo?.Motivo}");
        Assert.NotNull(autorizada.XmlProcessado);
    }

    // A NFC-e de venda a consumidor final: a nota da NF-e sem a saída, o destinatário e o endereço, no formato do DANFE NFC-e.
    private static NotaFiscal NotaNFCe(string cnpj, string inscricao)
    {
        var nota = Nota(cnpj, inscricao);
        nota.Identificacao.SaidaEntrada = null;
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeNFCe;
        nota.Identificacao.VersaoAplicativo = "OpenDFe 5.1";
        nota.Destinatario = null;
        return nota;
    }

    // A venda de um item a um consumidor final de MG, no Simples Nacional, com o número sorteado (a homologação guarda
    // as notas de todas as execuções).
    private static NotaFiscal Nota(string cnpj, string inscricao)
    {
        var agora = DateTimeOffset.UtcNow.AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        agora = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        var nota = new NotaFiscal
        {
            Identificacao =
            {
                NaturezaOperacao = "VENDA", Serie = 1, Numero = Random.Shared.Next(1, 999_999_999), Emissao = agora, SaidaEntrada = agora,
                Tipo = TipoNota.Saida, LocalDestino = LocalDestino.Interna, MunicipioFatoGerador = 3106200,
                FormatoImpressao = FormatoImpressao.DanfeRetrato, Finalidade = FinalidadeEmissao.Normal,
                ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal, Presenca = PresencaComprador.Presencial,
                Processo = ProcessoEmissao.AplicativoContribuinte, VersaoAplicativo = "OpenDFe 4.5",
            },
            Emitente = new Emitente
            {
                Cnpj = cnpj, Nome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL", InscricaoEstadual = inscricao,
                RegimeTributario = RegimeTributario.SimplesNacional,
                Endereco =
                {
                    Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200, Municipio = "BELO HORIZONTE",
                    UF = UF.MG, Cep = "30110000",
                },
            },
            Destinatario = new Destinatario
            {
                Cpf = "52998224725", Nome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL",
                IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
                Endereco = new EnderecoDestinatario
                {
                    Logradouro = "RUA B", Numero = "2", Bairro = "CENTRO", CodigoMunicipio = 3106200, Municipio = "BELO HORIZONTE",
                    UF = UF.MG, Cep = "30110000", CodigoPais = 1058, Pais = "BRASIL",
                },
            },
            Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
            Pagamento = { Formas = { new FormaPagamento { Meio = MeioPagamento.Dinheiro, Valor = 50.00m } } },
        };
        nota.Itens.Add(new Item
        {
            Produto =
            {
                Codigo = "123", Gtin = "SEM GTIN", Descricao = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL",
                Ncm = "09012100", Cfop = 5102, UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m,
                Valor = 50.00m, GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m,
                ValorUnitarioTributavel = 25.00m, Totalizacao = IndicadorTotal.Compoe,
            },
            Imposto =
            {
                Icms = new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito },
                Pis = new PisOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
                Cofins = new CofinsOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
            },
        });
        var total = nota.Total.Icms;
        (total.BaseCalculo, total.Valor, total.ValorDesonerado, total.ValorFcp, total.BaseCalculoSt, total.ValorSt) = (0m, 0m, 0m, 0m, 0m, 0m);
        (total.ValorFcpSt, total.ValorFcpStRetido, total.Frete, total.Seguro, total.Desconto) = (0m, 0m, 0m, 0m, 0m);
        (total.ValorImpostoImportacao, total.ValorIpi, total.ValorIpiDevolvido, total.ValorPis, total.ValorCofins) = (0m, 0m, 0m, 0m, 0m);
        (total.ValorProdutos, total.OutrasDespesas, total.ValorNota) = (50.00m, 0m, 50.00m);
        return nota;
    }
}
