using System.Security.Cryptography.X509Certificates;
using OpenDFe.Impressao;
using OpenDFe.Impressao.Pdf;
using OpenDFe.Impressao.Pdf.Testes;
using OpenDFe.NFSe;
using Emulador = OpenDFe.SefazEmulada.Emulador;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFSe ponta a ponta (etapa 9.5): a NFS-e emitida pela Sefin emulada (E.7), lida pelo leiaute e impressa em PDF, com o
/// QR Code da chave que a Sefin gerou; e a cancelada, com a situação tirada da consulta dos eventos (9.3), como o guia manda.
/// </summary>
public sealed class DanfseEmuladoTestes : IClassFixture<SefinEmuladaFixture>, IDisposable
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = DpsTeste.CnpjPrestador }.GerarPfx("teste"), "teste");

    private static readonly TimeSpan Brasilia = TimeSpan.FromHours(-3);

    private static int _numero = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100_000) * 100;

    private readonly Emulador _emulador;
    private readonly ConexoesSefaz _conexoes;
    private readonly DFe _dfe;

    public DanfseEmuladoTestes(SefinEmuladaFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Municipios.Limpar();
        _conexoes = new ConexoesSefaz(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };
        _dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = _conexoes,
            Urls = [new EnderecoServico(Destino.SefinNacional, Ambiente.Homologacao, "SefinNacional", _emulador.SefinNacional)],
        });
    }

    public void Dispose() => _conexoes.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(ChaveNFSe Chave, string Xml)> EmitirAsync()
    {
        var agora = DateTimeOffset.UtcNow.ToOffset(Brasilia).AddMinutes(-1);
        var dps = DpsTeste.Minima();
        dps.Numero = Interlocked.Increment(ref _numero);
        dps.Emissao = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        dps.Competencia = DateOnly.FromDateTime(dps.Emissao.Value.DateTime);
        dps.Prestador.Cpf = null;
        dps.Prestador.Cnpj = DpsTeste.CnpjPrestador;
        var emissao = await _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct);
        Assert.True(emissao.Retorno!.Emitida, emissao.Motivo);
        return (emissao.Retorno.Chave!.Value, emissao.Retorno.Xml!);
    }

    [Fact]
    public async Task ANFSeEmitidaPelaSefinEmuladaViraODanfseComOQrCodeDaChave()
    {
        var (chave, xml) = await EmitirAsync();

        var lida = LeiauteNFSe101.Instancia.Ler(xml);
        var documento = LeiauteNFSe101.Instancia.Danfse(lida);
        var pdf = RenderizadorPdf.Gerar(documento);
        AmostrasPdf.Gravar("danfse-emulada", pdf);
        using var leitor = new PdfLeitor(pdf);

        Assert.Equal(1, leitor.Paginas);
        var texto = leitor.TextoCompacto(1);
        Assert.Contains(chave.Valor, texto, StringComparison.Ordinal);
        // A produção restrita do emulador e o município que a Sefin pôs na NFS-e (o padrão do emulador).
        Assert.Contains(PdfLeitor.Compactar("NFS-e SEM VALIDADE JURÍDICA"), texto, StringComparison.Ordinal);
        Assert.Contains(PdfLeitor.Compactar("Município: " + lida.Nota!.NomeMunicipioEmissor), texto, StringComparison.Ordinal);
        var url = "https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=" + chave.Valor;
        var esperada = PdfVetores.MatrizQrCode(url);
        var caminho = leitor.Pagina(1).Paths.First(c => c.IsFilled && c.Count > 50);
        Assert.Equal(esperada, PdfVetores.MatrizQrCodePdf(leitor, 1, esperada.GetLength(0), caminho).Matriz);
    }

    [Fact]
    public async Task ACanceladaPelaConsultaDosEventosSaiComAMarcaDAgua()
    {
        var (chave, xml) = await EmitirAsync();
        var cancelamento = await _dfe.ExecutarAsync(new EnvioEventoNFSe(new CancelamentoNFSe
        {
            CnpjAutor = DpsTeste.CnpjPrestador,
            Chave = chave.Valor,
            Motivo = MotivoCancelamento.ErroEmissao,
            Descricao = "Erro no valor do serviço",
        }), Ct);
        Assert.True(cancelamento.Retorno!.Registrado, cancelamento.Motivo);

        // Quem imprime tira a situação dos eventos (a lib não a deduz): o 101101 é a cancelada.
        var eventos = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave), Ct);
        var cancelada = eventos.Retorno!.Eventos.Any(e => e.Tipo == "101101");
        var documento = LeiauteNFSe101.Instancia.Danfse(LeiauteNFSe101.Instancia.Ler(xml), new OpcoesDanfse { Cancelada = cancelada });

        Assert.True(cancelada);
        Assert.Equal("CANCELADA", Assert.IsType<FormularioImpressao>(documento.Elementos[0]).MarcaAgua);
        using var leitor = new PdfLeitor(RenderizadorPdf.Gerar(documento));
        Assert.Equal("CANCELADA", string.Concat(leitor.Pagina(1).Letters.Where(l => l.PointSize > 20).Select(l => l.Value)));
    }
}
