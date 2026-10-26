# Início rápido: a primeira NFC-e na SEFAZ emulada

[Documentação](../index.md) › Início rápido

Neste tutorial, você autoriza uma NFC-e, consulta a situação dela, cancela e gera o DANFCE em PDF. Tudo roda na sua máquina, contra a [SEFAZ emulada](sefaz-emulada/index.md), sem certificado ICP-Brasil e sem acesso à SEFAZ real.

Você vai:

> - Subir a SEFAZ emulada.
> - Criar um certificado de teste e confiar na AC do emulador.
> - Criar o núcleo da OpenDFe apontando para o emulador.
> - Montar e autorizar uma NFC-e.
> - Consultar, cancelar e gerar o PDF do DANFCE.

## Pré-requisitos

- O SDK do .NET 10.
- O repositório da OpenDFe clonado (`git clone https://github.com/Davidsonrf1/OpenDFe.git`). O emulador roda a partir dele.
- Os pacotes da OpenDFe gerados numa pasta local, como em [Instalação](instalacao.md), ou os projetos referenciados diretamente.

## 1. Subir a SEFAZ emulada

Na raiz do repositório da OpenDFe, rode:

```
dotnet run --project tests/OpenDFe.SefazEmulada
```

O emulador mostra onde está escutando:

```
SEFAZ emulada em https://127.0.0.1:5443/ (só homologação, só para testes).
A AC de teste, para confiar no servidor: C:\Users\voce\AppData\Local\OpenDFe.SefazEmulada\ac.crt
...
O painel, no navegador: http://127.0.0.1:5444/
Ctrl+C para parar.
```

Deixe o terminal aberto. A porta `5443` é a dos Web Services (HTTPS, com TLS mútuo); a `5444` é o [painel](sefaz-emulada/painel.md) e a API de controle (HTTP, só no próprio computador). Abra `http://127.0.0.1:5444/` no navegador para acompanhar os pedidos ao vivo.

> **Dica**
> Prefere Docker? `docker compose -f tests/OpenDFe.SefazEmulada/compose.yaml up -d --build` sobe o mesmo emulador nas mesmas portas. Veja [Executar a SEFAZ emulada](sefaz-emulada/executar.md).

## 2. Criar o projeto

Em outro terminal, crie um aplicativo de console e adicione os pacotes:

```
dotnet new console -n PrimeiraNFCe
cd PrimeiraNFCe
dotnet add package OpenDFe.NFe --version 0.1.0
dotnet add package OpenDFe.Impressao.Pdf --version 0.1.0
```

## 3. Escrever o programa

Substitua o conteúdo de `Program.cs` pelo código abaixo. As seções seguintes explicam cada parte.

```csharp
using System.Formats.Asn1;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using OpenDFe;
using OpenDFe.Impressao.Pdf;
using OpenDFe.NFe;
using QuestPDF.Infrastructure;

const string CnpjEmitente = "11222333000181";

// O painel da SEFAZ emulada: de lá vêm a AC de teste e as URLs dos Web Services.
using var painel = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5444/") };

// 1. O certificado do emitente: autoassinado, com o CNPJ do emitente.
using var certificado = GerarCertificadoTeste(CnpjEmitente, "LOJA EXEMPLO LTDA");

// 2. As conexões, confiando na AC de teste do emulador.
using var ac = X509Certificate2.CreateFromPem(await painel.GetStringAsync("_controle/ac.crt"));
using var conexoes = new ConexoesSefaz(conexao =>
{
    var handler = conexao.CriarPadrao();
    var padrao = handler.SslOptions.RemoteCertificateValidationCallback!;
    handler.SslOptions.RemoteCertificateValidationCallback = (remetente, servidor, cadeia, erros) =>
        padrao(remetente, servidor, cadeia, erros) || EmitidoPelaAc(ac, servidor, erros);
    return handler;
});

// 3. O núcleo da NFC-e, com as URLs do emulador no lugar das da SEFAZ.
var leiaute = new LeiauteNFe400();
var dfe = DFe.Criar(leiaute, Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Urls = await UrlsEmulador(painel, leiaute, Documento.NFCe),
});

// 4. O status do serviço.
var status = await dfe.ExecutarAsync(new ConsultaStatus());
Console.WriteLine($"Status: {status.CStat} {status.Motivo}");

// 5. A autorização.
var resultado = await dfe.ExecutarAsync(new Autorizacao(NovaNFCe(numero: 1)));
if (!resultado.Processado)
{
    foreach (var problema in resultado.Problemas)
        Console.WriteLine(problema);
    return;
}
var nota = resultado.Retorno.Notas[0];
Console.WriteLine($"Autorização: {nota.Protocolo?.Status} {nota.Protocolo?.Motivo}, protocolo {nota.Protocolo?.Numero}");
if (!nota.Autorizada)
    return;
File.WriteAllText($"{nota.Chave}-nfeProc.xml", nota.XmlProcessado);

// 6. O DANFCE em PDF.
QuestPDF.Settings.License = LicenseType.Community;   // a escolha da licença é sua: veja "Gerar o PDF"
var danfce = leiaute.Danfce(leiaute.Ler(nota.XmlProcessado!));
File.WriteAllBytes($"{nota.Chave}.pdf", RenderizadorPdf.Gerar(danfce));

// 7. A consulta e o cancelamento.
var cancelamento = new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, "Cancelamento de teste do tutorial");
var evento = await dfe.ExecutarAsync(new EnvioEvento(cancelamento));
Console.WriteLine($"Cancelamento: {evento.Retorno!.Eventos[0].Status} {evento.Retorno.Eventos[0].Motivo}");

var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!));
Console.WriteLine($"Consulta: {consulta.Retorno!.Status} {consulta.Retorno.Motivo}");

// ---------------------------------------------------------------------------------------------------------------

static NotaFiscal NovaNFCe(int numero)
{
    var nota = new NotaFiscal
    {
        Identificacao =
        {
            NaturezaOperacao = "VENDA",
            Serie = 1,
            Numero = numero,
            Emissao = DateTimeOffset.Now,
            Tipo = TipoNota.Saida,
            LocalDestino = LocalDestino.Interna,
            MunicipioFatoGerador = 3106200,
            FormatoImpressao = FormatoImpressao.DanfeNFCe,
            Finalidade = FinalidadeEmissao.Normal,
            ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal,
            Presenca = PresencaComprador.Presencial,
            Processo = ProcessoEmissao.AplicativoContribuinte,
            VersaoAplicativo = "PrimeiraNFCe 1.0",
        },
        Emitente =
        {
            Cnpj = CnpjEmitente,
            Nome = "LOJA EXEMPLO LTDA",
            InscricaoEstadual = "0010000000000",
            RegimeTributario = RegimeTributario.SimplesNacional,
            Endereco =
            {
                Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200,
                Municipio = "BELO HORIZONTE", UF = UF.MG, Cep = "30110000",
            },
        },
        Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
        Pagamento = { Formas = { new FormaPagamento { Meio = MeioPagamento.Dinheiro, Valor = 50.00m } } },
    };

    nota.Itens.Add(new Item
    {
        Produto =
        {
            Codigo = "123", Gtin = "SEM GTIN", Descricao = "CAFE TORRADO 500G", Ncm = "09012100", Cfop = 5102,
            UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m, Valor = 50.00m,
            GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m,
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
    total.BaseCalculo = 0m; total.Valor = 0m; total.ValorDesonerado = 0m; total.ValorFcp = 0m;
    total.BaseCalculoSt = 0m; total.ValorSt = 0m; total.ValorFcpSt = 0m; total.ValorFcpStRetido = 0m;
    total.ValorProdutos = 50.00m; total.Frete = 0m; total.Seguro = 0m; total.Desconto = 0m;
    total.ValorImpostoImportacao = 0m; total.ValorIpi = 0m; total.ValorIpiDevolvido = 0m;
    total.ValorPis = 0m; total.ValorCofins = 0m; total.OutrasDespesas = 0m; total.ValorNota = 50.00m;
    return nota;
}

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
    return CertificadoDigital.AbrirPfx(gerado.Export(X509ContentType.Pkcs12, "teste"), "teste");
}

static bool EmitidoPelaAc(X509Certificate2 ac, X509Certificate? servidor, SslPolicyErrors erros)
{
    if (servidor is null || erros != SslPolicyErrors.RemoteCertificateChainErrors)
        return false;
    using var certificadoServidor = X509CertificateLoader.LoadCertificate(servidor.GetRawCertData());
    using var cadeia = new X509Chain();
    cadeia.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
    cadeia.ChainPolicy.CustomTrustStore.Add(ac);
    cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
    return cadeia.Build(certificadoServidor);
}

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

## 4. Executar

```
dotnet run
```

A saída se parece com esta:

```
Status: 107 Serviço em Operação
Autorização: 100 Autorizado o uso da NF-e, protocolo 131260000000001
Cancelamento: 135 Evento registrado e vinculado a NF-e
Consulta: 101 Cancelamento de NF-e homologado
```

A pasta do projeto recebe o `nfeProc` (a nota assinada com o protocolo) e o PDF do DANFCE. No painel, a aba **Pedidos** mostra cada pedido e cada resposta, com o XML.

> **Observação**
> O número da nota precisa mudar a cada execução enquanto o emulador estiver no ar: a mesma numeração com outra chave é rejeitada com o `cStat` 539 (duplicidade). Troque o `numero` ou zere o emulador com o botão **Zerar tudo** do painel.

## O que o código faz

**O certificado de teste.** A SEFAZ emulada aceita qualquer certificado no TLS mútuo, mas confere que o titular do certificado que assina a nota é o emitente (como a SEFAZ faz, pela raiz do CNPJ). Por isso o certificado de teste leva o CNPJ do emitente no campo `serialNumber`, como o e-CNPJ da ICP-Brasil. Na SEFAZ real, você usa o PFX do emitente com `CertificadoDigital.AbrirArquivoPfx`. Veja [Certificado digital](conceitos/certificado.md).

**A AC de teste.** O certificado HTTPS do emulador é emitido por uma AC que ele mesmo gera. O handler padrão da biblioteca (`ConexaoSefaz.CriarPadrao`) não confia nela, e a fábrica do `ConexoesSefaz` acrescenta essa confiança sem desligar nenhuma outra verificação: o nome do servidor continua conferido. Veja [Conectar a OpenDFe à SEFAZ emulada](sefaz-emulada/conectar.md).

**As URLs.** `ConfiguracaoDFe.Urls` troca as URLs da SEFAZ pelas do emulador. A lista vem do painel e é filtrada pelos serviços que o leiaute declara para o documento: o núcleo recusa uma URL de serviço que o documento não tem (a NFC-e não tem a distribuição DF-e, por exemplo).

**O núcleo.** `DFe.Criar` junta o leiaute, o documento e a configuração. O núcleo é imutável, barato de criar e seguro para uso concorrente. Veja [O núcleo e a configuração](conceitos/nucleo-configuracao.md).

**A nota.** A `NotaFiscal` traz os valores prontos: a biblioteca não calcula tributo. Os campos que o núcleo já sabe (a UF, o modelo, o ambiente, o tipo de emissão) e a chave de acesso saem da configuração. O QR Code da NFC-e (versão 3) é gerado pela biblioteca. Veja [Montar a nota](nfe/montar-nota.md).

**O resultado.** `ExecutarAsync` devolve um `ResultadoWS`. `Processado` diz que a SEFAZ respondeu, inclusive com rejeição; o `cStat` de cada nota está em `Protocolo.Status`. Problemas encontrados antes de transmitir (schema, regras) voltam como `RecusadoLocalmente`, com a lista de `Problemas`. Veja [Resultados, problemas e exceções](conceitos/resultados-problemas.md).

## Experimente

- **Uma rejeição.** Mude `total.ValorProdutos` para `49.00m`. A nota não sai do computador: `resultado.Problemas` traz o `564` ("Total do Produto / Serviço difere do somatório dos itens"), apontando `Total.Icms.ValorProdutos`.
- **Um serviço paralisado.** No painel, aba **Autorizadores**, paralise MG com o 109 e rode de novo: o status volta `109`. Veja [Simular falhas](sefaz-emulada/falhas-comandadas.md).
- **A queda no meio.** Escreva `EMULADA:conexaoDerrubada:depois` nas informações complementares da nota (`nota.InformacoesAdicionais = new InformacoesAdicionais { Complementares = "EMULADA:conexaoDerrubada:depois" }`). O emulador autoriza e derruba a conexão: você recebe uma `FalhaTransmissaoException` com `PodeTerSidoProcessado` verdadeiro, e o guia [Quando a conexão cai](nfe/queda-recuperacao.md) mostra como recuperar a nota.

## Próximas etapas

- [SEFAZ emulada: visão geral](sefaz-emulada/index.md)
- [Autorizar notas: síncrona, lote e preparo](nfe/autorizacao.md)
- [Montar a nota](nfe/montar-nota.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
