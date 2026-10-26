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

## Documentação

A documentação completa começa em [docs/index.md](docs/index.md). Comece pelo [início rápido](docs/guia/inicio-rapido.md): a primeira NFC-e autorizada na SEFAZ emulada, sem certificado real.

- [Visão geral](docs/guia/visao-geral.md)
- [Instalação](docs/guia/instalacao.md)
- [Início rápido: a primeira NFC-e na SEFAZ emulada](docs/guia/inicio-rapido.md)

**SEFAZ emulada**

- [Visão geral da SEFAZ emulada](docs/guia/sefaz-emulada/index.md): o que ela é, o que ela confere e o que ela não substitui.
- [Executar a SEFAZ emulada](docs/guia/sefaz-emulada/executar.md): `dotnet run`, Docker, opções e persistência.
- [Conectar a OpenDFe à SEFAZ emulada](docs/guia/sefaz-emulada/conectar.md): o certificado de teste, a AC de teste e as URLs.
- [Usar o painel](docs/guia/sefaz-emulada/painel.md): acompanhar os pedidos e comandar o emulador pelo navegador.
- [Simular falhas](docs/guia/sefaz-emulada/falhas-comandadas.md): paralisação, tempo esgotado, queda no meio, lote em processamento.
- [Preparar dados de teste](docs/guia/sefaz-emulada/dados-teste.md): CSC, contribuintes irregulares, cadastro e persistência.
- [Usar em testes automatizados](docs/guia/sefaz-emulada/testes-automatizados.md): xUnit, isolamento entre testes e CI.
- [Serviços e códigos de retorno](docs/guia/sefaz-emulada/servicos.md): o que cada serviço responde.
- [Referência das rotas de controle](docs/guia/sefaz-emulada/api-controle.md): a API HTTP do `/_controle`.

**Conceitos**

- [Arquitetura: núcleo, leiaute e pacotes](docs/guia/conceitos/arquitetura.md)
- [O núcleo e a configuração](docs/guia/conceitos/nucleo-configuracao.md)
- [Certificado digital](docs/guia/conceitos/certificado.md)
- [Conexões, TLS e tempos](docs/guia/conceitos/conexoes-tls.md)
- [Resultados, problemas e exceções](docs/guia/conceitos/resultados-problemas.md)
- [Validação: schema, regras e regras próprias](docs/guia/conceitos/validacao.md)
- [Autorizadores, URLs e contingência SVC](docs/guia/conceitos/autorizadores-urls.md)

**NF-e e NFC-e**

- [Montar a nota](docs/guia/nfe/montar-nota.md)
- [Consultar o status do serviço](docs/guia/nfe/status-servico.md)
- [Autorizar notas](docs/guia/nfe/autorizacao.md)
- [Consultar a situação de uma nota](docs/guia/nfe/consulta-protocolo.md)
- [Quando a conexão cai: recuperação e duplicidade](docs/guia/nfe/queda-recuperacao.md)
- [Cancelar e corrigir: eventos](docs/guia/nfe/eventos.md)
- [Inutilizar numeração](docs/guia/nfe/inutilizacao.md)
- [Ler um XML recebido](docs/guia/nfe/leitura-xml.md)
- [Baixar as notas destinadas: distribuição DF-e](docs/guia/nfe/distribuicao-dfe.md)
- [Manifestação do destinatário](docs/guia/nfe/manifestacao.md)
- [Consulta cadastro](docs/guia/nfe/consulta-cadastro.md)

**Específico da NFC-e**

- [QR Code e CSC](docs/guia/nfce/qrcode-csc.md)
- [Contingência offline](docs/guia/nfce/contingencia-offline.md)
- [Cancelamento por substituição](docs/guia/nfce/cancelamento-substituicao.md)

**Impressão**

- [Modelo de impressão: DANFE e DANFCE](docs/guia/impressao/modelo-impressao.md)
- [Gerar o PDF](docs/guia/impressao/pdf.md)
- [Imprimir em impressora térmica (ESC/POS)](docs/guia/impressao/escpos.md)

**Da homologação à produção**

- [Passar para a homologação real e para a produção](docs/guia/homologacao-producao.md)
- [Solução de problemas](docs/guia/solucao-problemas.md)
- [Glossário](docs/guia/glossario.md)
- [Usar a OpenDFe com agentes de IA](docs/guia/agentes-ia.md): o prompt de contexto e, em cada página, um prompt pronto sobre o tema.

**Referência**

- [Referência da API](docs/referencia/index.md): todos os tipos e membros públicos, por namespace.
- [Catálogo de regras e rejeições](docs/REGRAS.md): as regras do MOC que a biblioteca confere antes de transmitir e as que ficam para a SEFAZ.
- [Notas Técnicas acompanhadas](docs/NTS.md): as NTs, os schemas e as tabelas que cada versão implementa.

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
