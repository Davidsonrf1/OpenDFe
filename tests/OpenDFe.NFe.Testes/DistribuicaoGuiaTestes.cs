using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using DocumentoDistribuido = OpenDFe.NFe.DocumentoDistribuido;

namespace OpenDFe.Testes;

// O laço do guia docs/DISTRIBUICAO.md (seção 7), exatamente como está lá, rodando contra a SEFAZ emulada: o código do guia
// compila e faz o que o texto diz (a sincronização em lotes, a hora de espera sem consultar, a resposta perdida e a lacuna
// recuperada pelo consNSU). Se este arquivo mudar, o guia muda junto.

// --- o código do guia ---

/// <summary>A caixa de entrada do aplicativo: guarda o NSU e os documentos. Quem usa implementa (banco, arquivo).</summary>
public interface ICaixaEntrada
{
    /// <summary>O último NSU recebido e gravado; 0 na primeira vez.</summary>
    long UltimoNsu { get; }

    /// <summary>Até quando não consultar (a hora de espera da NT); nulo se pode consultar.</summary>
    DateTimeOffset? NaoConsultarAte { get; set; }

    /// <summary>Grava o documento se ainda não está (a chave de idempotência é o NSU; sem NSU, a chave). Devolve se era novo.</summary>
    bool Gravar(DocumentoDistribuido documento);

    /// <summary>Avança o NSU, depois de gravar os documentos até ele. Nunca volta.</summary>
    void AvancarNsu(long ultimoNsu);
}

public static class SincronizadorDistribuicao
{
    /// <summary>Busca tudo o que há de novo e respeita a hora de espera. Devolve quantos documentos novos gravou.</summary>
    public static async Task<int> SincronizarAsync(DFe dfe, string interessado, ICaixaEntrada caixa, TimeProvider relogio, CancellationToken ct)
    {
        if (caixa.NaoConsultarAte is { } ate && relogio.GetUtcNow() < ate)
            return 0;

        var novos = 0;
        while (true)
        {
            var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(interessado, caixa.UltimoNsu), ct);
            if (!resultado.Processado)
                throw new InvalidOperationException("A consulta foi recusada antes de transmitir: " + string.Join("; ", resultado.Problemas));

            var retorno = resultado.Retorno;
            foreach (var documento in retorno.Documentos)
            {
                if (caixa.Gravar(documento))
                    novos++;
            }

            // Só avança no 137 e no 138, e nunca para trás (o 137 do primeiro acesso traz o ultNSU zerado).
            if (retorno.Status is 137 or 138 && retorno.UltimoNsu is { } ultimo && ultimo > caixa.UltimoNsu)
                caixa.AvancarNsu(ultimo);

            // 137, 138 que alcançou o maxNSU e 656: esperar 1 hora, sem consultar de novo antes.
            if (retorno.EsperaSugerida is { } espera)
            {
                caixa.NaoConsultarAte = relogio.GetUtcNow() + espera;
                return novos;
            }

            // Sem mais lotes (ou uma rejeição, que o chamador examina pelo resultado): para.
            if (!retorno.TemMais)
                return novos;
        }
    }

    /// <summary>Busca um a um os NSU da lacuna (a resposta perdida), no limite de 20 consultas por hora.</summary>
    public static async Task<int> RecuperarLacunaAsync(DFe dfe, string interessado, ICaixaEntrada caixa, long ateNsu, CancellationToken ct)
    {
        var novos = 0;
        for (var nsu = caixa.UltimoNsu + 1; nsu <= ateNsu; nsu++)
        {
            var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.Nsu(interessado, nsu), ct);
            if (!resultado.Processado || resultado.Retorno.ConsumoIndevido)
                break; // o limite de 20 por hora: recomeça da lacuna que ficou, depois da espera

            foreach (var documento in resultado.Retorno.Documentos)
            {
                if (caixa.Gravar(documento))
                    novos++;
            }
            caixa.AvancarNsu(nsu); // o 137 do NSU que o Ambiente Nacional não gerou também fecha a lacuna
        }
        return novos;
    }
}

// --- fim do código do guia ---

/// <summary>A caixa de entrada em memória do teste.</summary>
internal sealed class CaixaMemoria : ICaixaEntrada
{
    private readonly HashSet<string> _gravados = [];

    public List<DocumentoDistribuido> Documentos { get; } = [];

    public long UltimoNsu { get; private set; }

    public DateTimeOffset? NaoConsultarAte { get; set; }

    public bool Gravar(DocumentoDistribuido documento)
    {
        var chave = documento.Nsu is { } nsu ? "nsu:" + nsu : "chave:" + documento.Chave;
        if (!_gravados.Add(chave))
            return false;
        Documentos.Add(documento);
        return true;
    }

    public void AvancarNsu(long ultimoNsu) => UltimoNsu = Math.Max(UltimoNsu, ultimoNsu);
}

/// <summary>O laço do guia contra a SEFAZ emulada (etapa 8.1).</summary>
public sealed class DistribuicaoGuiaTestes
{
    private const string CnpjA = NotasTeste.CnpjEmitente;
    private const string CnpjB = "11444777000161";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static X509Certificate2 Certificado(string cnpj) =>
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = cnpj }.GerarPfx("teste"), "teste");

    private static DFe Criar(Emulador emulador, ConexoesSefaz conexoes, X509Certificate2 certificado, TimeProvider relogio) =>
        DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado,
            Conexoes = conexoes,
            Relogio = relogio,
            Urls = ApoioEmulado.Urls(emulador, Documento.NFe),
        });

    private static async Task<Emulador> Iniciar(TimeProvider relogio) =>
        await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);

    [Fact]
    public async Task OLacoBuscaTudoEmLotesEEsperaAHoraSemConsultar()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Iniciar(relogio);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        using var certificadoA = Certificado(CnpjA);
        using var certificadoB = Certificado(CnpjB);
        var emitente = Criar(emulador, conexoes, certificadoA, relogio);
        var destinatario = Criar(emulador, conexoes, certificadoB, relogio);
        for (var i = 1; i <= 55; i++)
            await ApoioEmulado.Autorizar(emitente, Documento.NFe, i, relogio);
        var caixa = new CaixaMemoria();

        var primeira = await SincronizadorDistribuicao.SincronizarAsync(destinatario, CnpjB, caixa, relogio, Ct);
        var ultimoPedido = emulador.Pedidos.Listar()[^1].Id;
        var imediata = await SincronizadorDistribuicao.SincronizarAsync(destinatario, CnpjB, caixa, relogio, Ct);

        Assert.Equal(55, primeira);
        Assert.Equal(55L, caixa.UltimoNsu);
        Assert.Equal(Enumerable.Range(1, 55).Select(i => (long?)i), caixa.Documentos.Select(d => d.Nsu));
        Assert.NotNull(caixa.NaoConsultarAte);
        Assert.Equal(0, imediata);
        Assert.Equal(ultimoPedido, emulador.Pedidos.Listar()[^1].Id); // a espera não consulta

        relogio.Avancar(TimeSpan.FromMinutes(61));
        await ApoioEmulado.Autorizar(emitente, Documento.NFe, 56, relogio);
        var depois = await SincronizadorDistribuicao.SincronizarAsync(destinatario, CnpjB, caixa, relogio, Ct);

        Assert.Equal(1, depois);
        Assert.Equal(56L, caixa.UltimoNsu);
    }

    [Fact]
    public async Task ARespostaPerdidaDeixaUmaLacunaQueOConsNsuFecha()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Iniciar(relogio);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        using var certificadoA = Certificado(CnpjA);
        using var certificadoB = Certificado(CnpjB);
        var emitente = Criar(emulador, conexoes, certificadoA, relogio);
        var destinatario = Criar(emulador, conexoes, certificadoB, relogio);
        for (var i = 1; i <= 3; i++)
            await ApoioEmulado.Autorizar(emitente, Documento.NFe, i, relogio);
        var caixa = new CaixaMemoria();
        emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "AN", Vezes: 1, Depois: true));

        await Assert.ThrowsAsync<FalhaTransmissaoException>(() => SincronizadorDistribuicao.SincronizarAsync(destinatario, CnpjB, caixa, relogio, Ct));
        var repetida = await SincronizadorDistribuicao.SincronizarAsync(destinatario, CnpjB, caixa, relogio, Ct);

        Assert.Equal(0, repetida); // o 656: o servidor já entregou o lote que se perdeu
        Assert.Equal(0L, caixa.UltimoNsu);
        Assert.NotNull(caixa.NaoConsultarAte);

        relogio.Avancar(TimeSpan.FromMinutes(61));
        var recuperados = await SincronizadorDistribuicao.RecuperarLacunaAsync(destinatario, CnpjB, caixa, 3, Ct);

        Assert.Equal(3, recuperados);
        Assert.Equal(3L, caixa.UltimoNsu);
        Assert.Equal([1L, 2L, 3L], caixa.Documentos.Select(d => d.Nsu));
    }
}
