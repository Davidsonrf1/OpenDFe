using System.Net.Security;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A autorização da NF-e pela lib inteira na homologação real (etapa 4.4): uma nota do Simples Nacional, de venda a
// consumidor final em MG, com o emitente do PFX e a inscrição estadual em OPENDFE_EMITENTE_IE. Pulado sem o PFX: até
// ele chegar, a etapa fecha na SEFAZ emulada (AutorizacaoEmuladaTestes, PROJETO.md 17). Outro regime ou outra UF
// pedem o item e o endereço ajustados.
public class AutorizacaoHomologacaoTestes
{
    private const string VariavelInscricao = "OPENDFE_EMITENTE_IE";

    [Fact]
    public async Task UmaNFeEAutorizadaEmHomologacao()
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
                Processo = ProcessoEmissao.AplicativoContribuinte, VersaoAplicativo = "OpenDFe 4.4",
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
