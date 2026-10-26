# Usar a SEFAZ emulada em testes automatizados

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Testes automatizados

Este artigo mostra como escrever testes de integração do seu aplicativo contra a SEFAZ emulada: uma fixture xUnit que prepara o certificado, as conexões e as URLs, o isolamento entre os testes e a execução na integração contínua.

## Pré-requisitos

- A SEFAZ emulada no ar, com o painel (`dotnet run` ou Docker; veja [Executar a SEFAZ emulada](executar.md)).
- Um projeto de testes com o xUnit v3 (`xunit.v3`) e os pacotes da OpenDFe.

## A fixture

A fixture faz, uma vez por classe de testes, o que o [artigo de conexão](conectar.md) descreve: gera o certificado de teste, confia na AC do emulador e lê as URLs. Ela também oferece o cliente do painel, para comandar falhas e zerar o estado.

```csharp
using System.Formats.Asn1;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using OpenDFe;
using OpenDFe.NFe;

/// A SEFAZ emulada para os testes: o certificado do emitente, as conexões com a AC de teste e as URLs.
public sealed class SefazEmuladaFixture : IAsyncLifetime
{
    public const string CnpjEmitente = "11222333000181";

    private X509Certificate2? _ac;
    private JsonElement _emulador;

    /// O painel e a API de controle (SEFAZ_EMULADA_PAINEL muda o endereço, por exemplo na CI).
    public HttpClient Painel { get; } = new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("SEFAZ_EMULADA_PAINEL") ?? "http://127.0.0.1:5444/"),
    };

    public LeiauteNFe400 Leiaute { get; } = new();
    public X509Certificate2 Certificado { get; } = GerarCertificadoTeste(CnpjEmitente, "EMPRESA DE TESTE LTDA");
    public ConexoesSefaz Conexoes { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _ac = X509Certificate2.CreateFromPem(await Painel.GetStringAsync("_controle/ac.crt"));
        _emulador = await Painel.GetFromJsonAsync<JsonElement>("_controle/emulador");
        Conexoes = new ConexoesSefaz(conexao =>
        {
            var handler = conexao.CriarPadrao();
            var padrao = handler.SslOptions.RemoteCertificateValidationCallback!;
            handler.SslOptions.RemoteCertificateValidationCallback = (remetente, servidor, cadeia, erros) =>
                padrao(remetente, servidor, cadeia, erros) || EmitidoPelaAc(_ac, servidor, erros);
            return handler;
        })
        {
            TempoResposta = TimeSpan.FromSeconds(10),   // as falhas de tempo esgotado não esperam 60 s
        };
    }

    /// O núcleo do documento, contra o emulador. Os parâmetros opcionais cobrem a contingência e a NFC-e com CSC.
    public DFe Criar(Documento documento, TipoEmissao emissao = TipoEmissao.Normal, ConfiguracaoDocumento? configuracao = null,
        X509Certificate2? certificado = null) =>
        DFe.Criar(Leiaute, documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado ?? Certificado,
            Conexoes = Conexoes,
            Urls = Urls(documento),
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.Now.AddMinutes(-1), "Contingencia de teste na SEFAZ emulada"),
            Documento = configuracao,
        });

    /// Zera o emulador: notas, eventos, distribuição, falhas, cadastros e pedidos.
    public async Task ZerarAsync() => (await Painel.DeleteAsync("_controle")).EnsureSuccessStatusCode();

    /// Comanda uma falha (veja "Simular falhas").
    public async Task ComandarAsync(object falha) => (await Painel.PostAsJsonAsync("_controle/falhas", falha)).EnsureSuccessStatusCode();

    public ValueTask DisposeAsync()
    {
        Conexoes?.Dispose();
        _ac?.Dispose();
        Certificado.Dispose();
        Painel.Dispose();
        return ValueTask.CompletedTask;
    }

    private List<EnderecoServico> Urls(Documento documento)
    {
        var servicos = DeclaracaoLeiaute.Criar(Leiaute, documento).Servicos.Select(s => s.Nome).ToHashSet();
        var urls = new List<EnderecoServico>();
        foreach (var item in _emulador.GetProperty("urls").EnumerateArray())
        {
            var servico = item.GetProperty("servico").GetString()!;
            if (servicos.Contains(servico) && Destinos.TentarLerSigla(item.GetProperty("destino").GetString(), out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, servico, new Uri(item.GetProperty("url").GetString()!)));
        }
        return urls;
    }

    public static X509Certificate2 GerarCertificadoTeste(string cnpj, string nome)
    {
        using var rsa = RSA.Create(2048);
        var titular = new X500DistinguishedNameBuilder();
        titular.AddCommonName(nome);
        titular.Add("2.5.4.5", cnpj, UniversalTagNumber.PrintableString);
        var pedido = new CertificateRequest(titular.Build(), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        pedido.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyEncipherment, critical: true));
        pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            [new Oid("1.3.6.1.5.5.7.3.2"), new Oid("1.3.6.1.5.5.7.3.4")], critical: false));
        using var gerado = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return CertificadoDigital.AbrirPfx(gerado.Export(X509ContentType.Pkcs12, "teste"), "teste");
    }

    private static bool EmitidoPelaAc(X509Certificate2? ac, X509Certificate? servidor, SslPolicyErrors erros)
    {
        if (ac is null || servidor is null || erros != SslPolicyErrors.RemoteCertificateChainErrors)
            return false;
        using var certificadoServidor = X509CertificateLoader.LoadCertificate(servidor.GetRawCertData());
        using var cadeia = new X509Chain();
        cadeia.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        cadeia.ChainPolicy.CustomTrustStore.Add(ac);
        cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return cadeia.Build(certificadoServidor);
    }
}
```

## Os testes

O estado do emulador (as notas autorizadas, as falhas) é um só para todos os testes. Para um teste não ver o que outro deixou, coloque os testes do emulador numa **coleção sem paralelismo** e zere o estado no começo de cada um:

```csharp
using OpenDFe;
using OpenDFe.NFe;

[CollectionDefinition("SEFAZ emulada", DisableParallelization = true)]
public sealed class ColecaoSefazEmulada : ICollectionFixture<SefazEmuladaFixture>;

[Collection("SEFAZ emulada")]
public sealed class EmissaoTestes : IAsyncLifetime
{
    private readonly SefazEmuladaFixture _sefaz;

    public EmissaoTestes(SefazEmuladaFixture sefaz) => _sefaz = sefaz;

    public async ValueTask InitializeAsync() => await _sefaz.ZerarAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task AutorizaANFCe()
    {
        var dfe = _sefaz.Criar(Documento.NFCe);

        var resultado = await dfe.ExecutarAsync(new Autorizacao(MinhasNotas.NFCe(numero: 1)), TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado, string.Join("; ", resultado.Problemas));
        Assert.Equal(100, resultado.Retorno.Notas[0].Protocolo?.Status);
        Assert.NotNull(resultado.Retorno.Notas[0].XmlProcessado);
    }

    [Fact]
    public async Task RecuperaANotaQuandoAConexaoCaiDepoisDeAutorizar()
    {
        var dfe = _sefaz.Criar(Documento.NFCe);
        await _sefaz.ComandarAsync(new { tipo = "conexaoDerrubada", servico = "NFeAutorizacao4", vezes = 1, depois = true });

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => dfe.ExecutarAsync(new Autorizacao(MinhasNotas.NFCe(numero: 2)), TestContext.Current.CancellationToken));
        Assert.True(falha.PodeTerSidoProcessado);

        var xml = Autorizacao.NotasEnviadas(falha.Rastro.XmlPedido!)[0];
        var recuperada = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), TestContext.Current.CancellationToken);

        Assert.Equal(100, recuperada.Retorno!.Status);
        Assert.NotNull(recuperada.Retorno.Notas[0].XmlProcessado);
    }

    [Fact]
    public async Task OStatusMostraOServicoParalisado()
    {
        var dfe = _sefaz.Criar(Documento.NFCe);
        await _sefaz.ComandarAsync(new { tipo = "paralisacao", destino = "MG", servico = "NFeStatusServico4", vezes = 1, codigo = "109" });

        var status = await dfe.ExecutarAsync(new ConsultaStatus(), TestContext.Current.CancellationToken);

        Assert.Equal(109, status.CStat);
        Assert.False(status.Retorno!.Operando);
    }
}
```

`MinhasNotas.NFCe` é o código do seu aplicativo que monta a nota (veja [Montar a nota](../nfe/montar-nota.md)). Use a `Emissao` atual (`DateTimeOffset.Now`): o emulador rejeita a NFC-e emitida há mais de 5 minutos (704).

> **Dica**
> Prefere rodar os testes em paralelo? Dispense o `ZerarAsync` e dê a cada teste uma numeração própria (série ou número que nenhum outro teste usa). A duplicidade é a única colisão entre notas diferentes; as falhas comandadas, porém, valem para o próximo pedido que combinar, então teste as falhas numa coleção sem paralelismo.

## Na integração contínua

O emulador não está publicado como imagem: a CI clona o repositório da OpenDFe e sobe o contêiner a partir dele. Um exemplo com o GitHub Actions:

```yaml
jobs:
  testes:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Baixar a OpenDFe (para a SEFAZ emulada)
        uses: actions/checkout@v4
        with:
          repository: Davidsonrf1/OpenDFe
          path: opendfe

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Subir a SEFAZ emulada
        run: docker compose -f opendfe/tests/OpenDFe.SefazEmulada/compose.yaml up -d --build

      - name: Esperar o emulador
        run: |
          for i in $(seq 1 60); do
            curl -sf http://127.0.0.1:5444/_controle/emulador > /dev/null && exit 0
            sleep 2
          done
          exit 1

      - run: dotnet test

      - name: Logs do emulador (se falhar)
        if: failure()
        run: docker compose -f opendfe/tests/OpenDFe.SefazEmulada/compose.yaml logs
```

Sem Docker, o mesmo vale com `dotnet run --project opendfe/tests/OpenDFe.SefazEmulada &` no lugar do Compose.

## Os testes do repositório da OpenDFe

Os testes da própria biblioteca (`tests/OpenDFe.NFe.Testes` e os demais) não precisam de nada disso: eles sobem o emulador dentro do processo de teste, numa porta livre, a cada classe. Basta `dotnet test --solution OpenDFe.slnx`.

Os testes de homologação (`tests/OpenDFe.Homologacao.Testes`) aceitam um emulador já em execução pelas variáveis `OPENDFE_EMULADOR` (o endereço dos Web Services) e `OPENDFE_EMULADOR_AC` (o caminho do `ac.crt`):

```powershell
$env:OPENDFE_EMULADOR = "https://127.0.0.1:5443"
$env:OPENDFE_EMULADOR_AC = "$env:LOCALAPPDATA\OpenDFe.SefazEmulada\ac.crt"
dotnet test --project tests/OpenDFe.Homologacao.Testes
```

## Prompt para agentes de IA

Peça a um agente que crie o projeto de testes de integração. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Crie o projeto de testes de integração do meu sistema contra a SEFAZ emulada da OpenDFe, seguindo docs/guia/sefaz-emulada/testes-automatizados.md:
xUnit v3, a SefazEmuladaFixture do guia (certificado, AC, URLs, ZerarAsync, ComandarAsync), uma coleção sem paralelismo e testes para
<as operações do meu sistema>. Use DateTimeOffset.Now na emissão (o emulador rejeita a NFC-e com mais de 5 minutos) e uma numeração
por teste. Acrescente o workflow do GitHub Actions que clona Davidsonrf1/OpenDFe, sobe o emulador com docker compose, espera o painel
e roda dotnet test.
```

## Próximas etapas

- [Serviços e códigos de retorno](servicos.md)
- [Referência das rotas de controle](api-controle.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
