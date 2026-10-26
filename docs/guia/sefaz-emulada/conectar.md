# Conectar a OpenDFe à SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Conectar

Este artigo mostra as três coisas que mudam quando a OpenDFe fala com a SEFAZ emulada em vez da SEFAZ real: o certificado do emitente, a confiança no servidor e as URLs dos Web Services. O resto do código, da montagem da nota à leitura do retorno, é o mesmo da produção.

| O que muda | Na SEFAZ real | Na SEFAZ emulada |
|---|---|---|
| Certificado | O A1 ICP-Brasil do emitente | Qualquer certificado com o CNPJ (ou CPF) do emitente, inclusive autoassinado |
| Confiança no servidor | As ACs da ICP-Brasil e as do sistema (padrão da biblioteca) | Também a AC de teste do emulador (`ac.crt`) |
| URLs | As do leiaute (padrão) | As do emulador, em `ConfiguracaoDFe.Urls` |
| Ambiente | `Homologacao` ou `Producao` | Sempre `Homologacao` |

## Pré-requisitos

- A SEFAZ emulada no ar. Veja [Executar a SEFAZ emulada](executar.md).

## 1. O certificado do emitente

No TLS mútuo, o emulador aceita qualquer certificado de cliente, sem conferir a cadeia. Na assinatura da nota, porém, ele confere o titular contra o emitente, como a SEFAZ: a raiz do CNPJ (8 posições) ou o CPF inteiro. Um titular diferente dá a rejeição 213 (ou 227 no CPF); um certificado sem CNPJ nem CPF, a 292.

Você tem duas opções:

- **Usar um PFX que você já tem**, de qualquer origem, desde que o titular seja o emitente das notas de teste. Abra com `CertificadoDigital.AbrirArquivoPfx(caminho, senha)`.
- **Gerar um certificado autoassinado na hora**, com o CNPJ do emitente. A OpenDFe lê o titular como a ICP-Brasil o grava: o CNPJ no `otherName` 2.16.76.1.3.3 do nome alternativo ou no `serialNumber` do titular. O exemplo abaixo usa o `serialNumber`, que se monta com as classes do .NET:

```csharp
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OpenDFe;

static X509Certificate2 GerarCertificadoTeste(string cnpj, string nome)
{
    using var rsa = RSA.Create(2048);
    var titular = new X500DistinguishedNameBuilder();
    titular.AddCommonName(nome);
    titular.Add("2.5.4.5", cnpj, UniversalTagNumber.PrintableString);   // o CNPJ no serialNumber, como no e-CNPJ
    var pedido = new CertificateRequest(titular.Build(), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    pedido.CertificateExtensions.Add(new X509KeyUsageExtension(
        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyEncipherment, critical: true));
    pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
        [new Oid("1.3.6.1.5.5.7.3.2"), new Oid("1.3.6.1.5.5.7.3.4")], critical: false));
    using var gerado = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

    // Passa pelo PFX para a chave privada ficar utilizável no TLS (no Windows, o SChannel exige).
    return CertificadoDigital.AbrirPfx(gerado.Export(X509ContentType.Pkcs12, "teste"), "teste");
}
```

Confira o titular com `CertificadoDigital.LerTitular(certificado)?.Cnpj`.

> **Observação**
> O emulador identifica o **transmissor** pelo certificado do TLS e o **emitente** pelo certificado da assinatura. Na OpenDFe os dois são o mesmo (`ConfiguracaoDFe.Certificado`). O transmissor importa em dois casos: só ele consulta o recibo de um lote (223), e, na distribuição DF-e, a raiz do CNPJ dele tem de ser a do interessado (593).

## 2. A confiança na AC de teste

O certificado HTTPS do emulador é emitido pela AC de teste que ele gera (`ac.crt`). O handler padrão da OpenDFe não confia nela, e a conexão falha com `FalhaTransmissaoException` do tipo `Tls`.

A forma certa de acrescentar a confiança é a fábrica do `ConexoesSefaz`: ela recebe cada conexão, cria o handler padrão da biblioteca (`ConexaoSefaz.CriarPadrao`, com o certificado do cliente e as regras de TLS da OpenDFe) e aceita, **além** do que o padrão aceita, o servidor cuja cadeia sobe à AC de teste. O nome do servidor continua conferido.

```csharp
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using OpenDFe;

// A AC de teste: pelo painel (ou X509Certificate2.CreateFromPemFile(caminhoDoAcCrt)).
using var painel = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5444/") };
using var ac = X509Certificate2.CreateFromPem(await painel.GetStringAsync("_controle/ac.crt"));

using var conexoes = new ConexoesSefaz(conexao =>
{
    var handler = conexao.CriarPadrao();
    var padrao = handler.SslOptions.RemoteCertificateValidationCallback!;
    handler.SslOptions.RemoteCertificateValidationCallback = (remetente, servidor, cadeia, erros) =>
        padrao(remetente, servidor, cadeia, erros) || EmitidoPelaAc(ac, servidor, erros);
    return handler;
});

static bool EmitidoPelaAc(X509Certificate2 ac, X509Certificate? servidor, SslPolicyErrors erros)
{
    // Só o erro de cadeia pode ser resolvido pela AC de teste; nome divergente nunca passa.
    if (servidor is null || erros != SslPolicyErrors.RemoteCertificateChainErrors)
        return false;
    using var certificadoServidor = X509CertificateLoader.LoadCertificate(servidor.GetRawCertData());
    using var cadeia = new X509Chain();
    cadeia.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
    cadeia.ChainPolicy.CustomTrustStore.Add(ac);
    cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
    return cadeia.Build(certificadoServidor);
}
```

> **Importante**
> Monte essa fábrica só no código de teste ou de desenvolvimento. Não a use contra a SEFAZ real, e nunca troque a validação por `return true`: isso aceitaria qualquer servidor.

> **Dica**
> Não instale o `ac.crt` no repositório de certificados do sistema. A fábrica acima limita a confiança às conexões da OpenDFe, sem afetar o resto da máquina.

## 3. As URLs do emulador

O emulador atende todos os autorizadores no mesmo servidor, com o caminho `/<autorizador>/<serviço>`: `https://127.0.0.1:5443/MG/NFeAutorizacao4`, `https://127.0.0.1:5443/SVRS/NFeStatusServico4`, `https://127.0.0.1:5443/AN/NFeDistribuicaoDFe`. A lista pronta sai do painel, em `GET /_controle/emulador`:

```json
{
  "autoridade": "C:\\Users\\voce\\AppData\\Local\\OpenDFe.SefazEmulada\\ac.crt",
  "endereco": "https://127.0.0.1:5443/",
  "painel": "http://127.0.0.1:5444/",
  "pedidos": 100,
  "urls": [
    { "destino": "MG", "servico": "NFeAutorizacao4", "url": "https://127.0.0.1:5443/MG/NFeAutorizacao4" },
    { "destino": "SVC-AN", "servico": "NFeAutorizacao4", "url": "https://127.0.0.1:5443/SVC-AN/NFeAutorizacao4" }
  ]
}
```

Converta a lista em `EnderecoServico` e passe em `ConfiguracaoDFe.Urls`. Filtre pelos serviços que o leiaute declara para o documento: o núcleo recusa (`ArgumentException`) uma URL de serviço que o documento não tem, como a distribuição DF-e num núcleo de NFC-e.

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using OpenDFe;
using OpenDFe.NFe;

static async Task<List<EnderecoServico>> UrlsEmulador(HttpClient painel, LeiauteNFe400 leiaute, Documento documento)
{
    var servicos = DeclaracaoLeiaute.Criar(leiaute, documento).Servicos.Select(s => s.Nome).ToHashSet();
    var emulador = await painel.GetFromJsonAsync<JsonElement>("_controle/emulador");
    var urls = new List<EnderecoServico>();
    foreach (var item in emulador.GetProperty("urls").EnumerateArray())
    {
        var servico = item.GetProperty("servico").GetString()!;
        if (servicos.Contains(servico) && Destinos.TentarLerSigla(item.GetProperty("destino").GetString(), out var destino))
            urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, servico, new Uri(item.GetProperty("url").GetString()!)));
    }
    return urls;
}
```

> **Observação**
> As URLs são de homologação. O ambiente do núcleo tem de ser `Ambiente.Homologacao`: as URLs de produção continuam as da SEFAZ, e o emulador rejeita `tpAmb` 1 com o 252.

## 4. O núcleo

Com as três partes, o núcleo é criado como em produção:

```csharp
var leiaute = new LeiauteNFe400();
var dfe = DFe.Criar(leiaute, Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = GerarCertificadoTeste("11222333000181", "EMPRESA DE TESTE LTDA"),
    Conexoes = conexoes,
    Urls = await UrlsEmulador(painel, leiaute, Documento.NFe),
});

var status = await dfe.ExecutarAsync(new ConsultaStatus());
Console.WriteLine($"{status.CStat} {status.Motivo}");   // 107 Serviço em Operação
```

Qualquer UF serve: o emulador tem os autorizadores de todas. A NFC-e de MG gera o QR Code com a URL de consulta da tabela da biblioteca; em outra UF, informe as URLs de consulta na `ConfiguracaoNFCe` (veja [QR Code e CSC](../nfce/qrcode-csc.md)). O emulador não confere a URL do QR Code.

## Alternar entre o emulador e a SEFAZ real

Deixe a escolha na configuração do seu aplicativo. Só três itens mudam, e o resto do código é o mesmo:

```csharp
ConfiguracaoDFe Configuracao(bool emulador, X509Certificate2 certificado, ConexoesSefaz conexoes, IReadOnlyList<EnderecoServico> urlsEmulador) =>
    new()
    {
        Ambiente = Ambiente.Homologacao,
        UF = UF.MG,
        Certificado = certificado,                       // o PFX real na SEFAZ; o de teste no emulador
        Conexoes = conexoes,                             // com a fábrica da AC de teste só no emulador
        Urls = emulador ? urlsEmulador : [],             // vazio: as URLs da SEFAZ, do leiaute
    };
```

## Próximas etapas

- [Usar o painel](painel.md)
- [Simular falhas](falhas-comandadas.md)
- [Usar em testes automatizados](testes-automatizados.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
