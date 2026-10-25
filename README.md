# OpenDFe

Biblioteca .NET aberta para **documentos fiscais eletrônicos brasileiros** (NF-e e NFC-e da SEFAZ; CT-e e MDF-e depois): geração do XML, validação pelas regras da SEFAZ, assinatura, transmissão, leitura e eventos. Não tem relação com outros projetos chamados "DFe".

**Estado (2026-10-24):** as fases 0 a 8 do [`PLANO.md`](PLANO.md) estão implementadas e conferidas **contra a SEFAZ emulada** (`tests/OpenDFe.SefazEmulada`), no Windows e no Linux. A homologação real está **pendente do certificado A1 ICP-Brasil**: os testes dela estão escritos e são pulados sem o PFX. **Nenhum pacote foi publicado ainda.** A arquitetura está em [`docs/PROJETO.md`](docs/PROJETO.md).

## O que a biblioteca faz

NF-e (modelo 55) e NFC-e (modelo 65), no leiaute 4.00:

- escrita, leitura e validação do XML (schema primeiro, e só as regras do MOC que o schema não vê), com os problemas em português e apontando o campo do modelo;
- assinatura XMLDSig, transmissão SOAP com TLS mútuo e a escolha do autorizador por UF e tipo de emissão;
- autorização síncrona e em lote, consulta do recibo e da nota, recuperação da autorização depois de uma queda;
- NFC-e: QR Code e CSC, contingência offline (e a NF-e com o DANFE Simplificado Tipo 2);
- eventos (cancelamento, cancelamento por substituição, carta de correção) e inutilização;
- DANFCE e DANFE em modelo de impressão neutro e em PDF;
- distribuição DF-e (as notas destinadas), manifestação do destinatário (ciência, confirmação, desconhecimento, operação não realizada) e consulta cadastro;
- uma SEFAZ emulada para testar tudo isso sem certificado real.

A lib não grava nada, não reenvia sozinha e não calcula tributo: quem usa guarda o XML e decide o imposto.

## Exemplo

Criar o núcleo, autorizar e imprimir (o trecho compila e roda contra o emulador em `tests/OpenDFe.NFe.Testes/ReadmeExemploTestes.cs`):

```csharp
using var certificado = CertificadoDigital.AbrirArquivoPfx("emitente.pfx", senha);
using var conexoes = new ConexoesSefaz();
var leiaute = new LeiauteNFe400();
var dfe = DFe.Criar(leiaute, Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes,
});

var resultado = await dfe.ExecutarAsync(new Autorizacao(nota));   // nota: a NotaFiscal com os valores prontos
var autorizada = resultado.Retorno!.Notas[0];
if (autorizada.Autorizada)
{
    QuestPDF.Settings.License = LicenseType.Community;            // escolha de quem usa (README do pacote de PDF)
    byte[] pdf = RenderizadorPdf.Gerar(leiaute.Danfe(leiaute.Ler(autorizada.XmlProcessado!)));
}
```

## Pacotes

| Pacote | Conteúdo |
|---|---|
| `OpenDFe.Nucleo` | contratos, certificado, assinatura XMLDSig, SOAP, TLS ICP-Brasil, validação pelo XSD |
| `OpenDFe.NFe` | modelo da NF-e e da NFC-e e o leiaute 4.00 |
| `OpenDFe.Impressao.Pdf` | DANFE e DANFCE em PDF |

> **Licença do PDF:** o `OpenDFe.Impressao.Pdf` usa o **QuestPDF**, que **não** é MIT (licença própria, com uma versão gratuita Community e licenças pagas). A escolha e a obrigação são de quem usa esse pacote; leia o [README do pacote](src/OpenDFe.Impressao.Pdf/README.md) antes de usar. O resto da OpenDFe é MIT e não depende dele.

## Requisitos

- .NET 10.
- Windows ou Linux. No Linux, OpenSSL 3, com a política de criptografia permitindo a assinatura RSA-SHA1 que a NF-e exige (`docs/PROJETO.md` 2.8).

## Licença e propriedade

[MIT](LICENSE). A OpenDFe é projeto pessoal de Davidson Faria desde o primeiro commit. Produtos que a usam, inclusive os comerciais, o fazem sob a licença MIT, sem transferência de propriedade.
