using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;

namespace OpenDFe.Testes;

// O que os testes dos eventos e da inutilização contra a SEFAZ emulada repartem (etapa 6.1): o certificado, a conexão com a
// AC do emulador, o núcleo com as URLs trocadas, as notas emitidas agora e o relógio que avança.
internal static class ApoioEmulado
{
    internal static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    internal static ConexoesSefaz Conexoes(Emulador emulador) =>
        new(ConexaoTeste.Fabrica(emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };

    internal static DFe Criar(Emulador emulador, ConexoesSefaz conexoes, Documento documento = Documento.NFe,
        TipoEmissao emissao = TipoEmissao.Normal, TimeProvider? relogio = null) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal
                ? null
                : new Contingencia(DateTimeOffset.UtcNow.AddMinutes(-10), "Teste na SEFAZ emulada"),
            Relogio = relogio ?? TimeProvider.System,
            Urls = Urls(emulador, documento),
        });

    // As URLs do emulador para os serviços que o leiaute declara.
    internal static List<EnderecoServico> Urls(Emulador emulador, Documento documento)
    {
        var servicos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Servicos.Select(s => s.Nome).ToHashSet(StringComparer.Ordinal);
        var urls = new List<EnderecoServico>();
        foreach (var url in emulador.Urls)
        {
            if (servicos.Contains(url.Servico) && Destinos.TentarLerSigla(url.Destino, out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, url.Servico, url.Url));
        }
        return urls;
    }

    // A nota emitida um minuto antes de <paramref name="agora"/>, com o número do caso e o cNF fixo.
    internal static NotaFiscal Nota(Documento documento, int numero, DateTimeOffset agora)
    {
        var nota = documento == Documento.NFe ? NotasTeste.NFe() : NotasTeste.NFCe();
        var emissao = agora.AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        emissao = emissao.AddTicks(-(emissao.Ticks % TimeSpan.TicksPerSecond));
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        nota.Identificacao.Emissao = emissao;
        if (documento == Documento.NFe)
            nota.Identificacao.SaidaEntrada = emissao;
        return nota;
    }

    // O relógio do emulador e da lib, que avança sem esperar (as 24 horas do cancelamento).
    internal sealed class RelogioAvancavel : TimeProvider
    {
        private TimeSpan _avanco;

        public override DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow() + _avanco;

        public void Avancar(TimeSpan tempo) => _avanco += tempo;
    }
}
