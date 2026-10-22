using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// De onde veio a confiança no certificado do servidor, na prova. Público: é parâmetro de teste.
public enum OrigemProva
{
    Recusado = 0,
    Sistema = 1,      // a AC raiz está no repositório da máquina (MG e o Ambiente Nacional: ACs públicas)
    IcpBrasil = 2,    // a cadeia sobe a uma das raízes ICP-Brasil embutidas no núcleo (SVRS, SP, PR e a maioria)
    Emulador = 3,     // a cadeia sobe à AC de teste da SEFAZ emulada (PROJETO.md 13.5), só quando informada
}

// O cliente da prova, desde a 4.1 sobre o transporte da lib (ConexoesSefaz): a fábrica ajusta o handler padrão
// com as intermediárias do PFX no handshake e com a AC do emulador, quando informada, e guarda de onde veio a
// confiança (a avaliação do núcleo, ConfiancaServidor).
internal sealed class ClienteSefaz : IDisposable
{
    private readonly ConexoesSefaz _conexoes;
    private readonly X509Certificate2 _certificado;
    private volatile int _origem;

    public ClienteSefaz(X509Certificate2 certificado, X509Certificate2Collection intermediarias, X509Certificate2? raizEmulador = null)
    {
        _certificado = certificado;
        _conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            handler.SslOptions.ClientCertificateContext = SslStreamCertificateContext.Create(conexao.Certificado, intermediarias, offline: true);
            handler.SslOptions.RemoteCertificateValidationCallback = (_, servidor, cadeia, erros) =>
            {
                var origem = raizEmulador is not null && ConexaoTeste.SobeAutoridade(raizEmulador, servidor, erros)
                    ? OrigemProva.Emulador
                    : (OrigemProva)(int)ConfiancaServidor.Avaliar(servidor, cadeia, erros);
                _origem = (int)origem;
                return origem != OrigemProva.Recusado;
            };
            return handler;
        });
    }

    // A confiança da última conexão aberta.
    public OrigemProva UltimaOrigem => (OrigemProva)_origem;

    // A mensagem do nfeResultMsg; a falha, como o transporte a classifica (FalhaTransporte).
    public Task<string> EnviarAsync(Uri url, string mensagem, CancellationToken cancellationToken) =>
        _conexoes.Transporte().EnviarAsync(new EnvioSefaz(_certificado, Destino.MG, url, StatusServico.Servico, mensagem), cancellationToken);

    public void Dispose() => _conexoes.Dispose();
}
