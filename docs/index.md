# Documentação da OpenDFe

A OpenDFe é uma biblioteca .NET aberta para **documentos fiscais eletrônicos brasileiros**: a NF-e (modelo 55) e a NFC-e (modelo 65), no leiaute 4.00. Ela gera o XML, valida pelo schema e pelas regras da SEFAZ, assina, transmite, lê o retorno, trata os eventos e imprime o DANFE e o DANFCE. A OpenDFe não tem relação com outros projetos chamados "DFe".

Junto com a biblioteca vem a **SEFAZ emulada**: um servidor local que responde como os autorizadores da SEFAZ em homologação. Com ela, você desenvolve e testa a emissão inteira (autorização, contingência, eventos, distribuição) sem certificado ICP-Brasil e sem depender da homologação real. Também dá para provocar as falhas que a SEFAZ real não deixa: serviço paralisado, tempo esgotado, conexão derrubada depois de autorizar.

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
});

var resultado = await dfe.ExecutarAsync(new Autorizacao(nota));
var autorizada = resultado.Retorno!.Notas[0];
if (autorizada.Autorizada)
    Guardar(autorizada.XmlProcessado);   // o nfeProc: a nota com o protocolo
```

> **Importante**
> A OpenDFe está em pré-lançamento. As operações estão implementadas e conferidas contra a SEFAZ emulada, no Windows e no Linux. A conferência contra a homologação real da SEFAZ ainda depende de um certificado A1 ICP-Brasil, e os pacotes ainda não estão publicados no NuGet.org. Veja [Instalação](guia/instalacao.md).

## Comece aqui

| Se você quer... | Leia |
|---|---|
| Entender o que a biblioteca faz e como ela é organizada | [Visão geral](guia/visao-geral.md) |
| Instalar e configurar o ambiente | [Instalação](guia/instalacao.md) |
| Autorizar a primeira NFC-e em minutos, sem certificado real | [Início rápido: a primeira NFC-e na SEFAZ emulada](guia/inicio-rapido.md) |
| Desenvolver e testar contra um servidor local | [SEFAZ emulada](guia/sefaz-emulada/index.md) |
| Consultar a assinatura de um tipo ou membro | [Referência da API](referencia/index.md) |
| Usar um agente de IA (Claude, Copilot) para escrever o código | [Usar a OpenDFe com agentes de IA](guia/agentes-ia.md) |

## SEFAZ emulada

Um servidor HTTPS local, com TLS mútuo, que implementa os Web Services da NF-e e da NFC-e como o MOC os descreve. É a forma recomendada de desenvolver e de rodar testes automatizados.

- [Visão geral da SEFAZ emulada](guia/sefaz-emulada/index.md): o que ela é, o que ela confere e o que ela não substitui.
- [Executar a SEFAZ emulada](guia/sefaz-emulada/executar.md): `dotnet run`, Docker, opções e persistência.
- [Conectar a OpenDFe à SEFAZ emulada](guia/sefaz-emulada/conectar.md): o certificado de teste, a AC de teste e as URLs.
- [Usar o painel](guia/sefaz-emulada/painel.md): acompanhar os pedidos e comandar o emulador pelo navegador.
- [Simular falhas](guia/sefaz-emulada/falhas-comandadas.md): paralisação, tempo esgotado, queda no meio, lote em processamento.
- [Preparar dados de teste](guia/sefaz-emulada/dados-teste.md): CSC, contribuintes irregulares, cadastro e persistência.
- [Usar em testes automatizados](guia/sefaz-emulada/testes-automatizados.md): xUnit, isolamento entre testes e CI.
- [Serviços e códigos de retorno](guia/sefaz-emulada/servicos.md): o que cada serviço responde.
- [Referência das rotas de controle](guia/sefaz-emulada/api-controle.md): a API HTTP do `/_controle`.

## Conceitos

- [Arquitetura: núcleo, leiaute e pacotes](guia/conceitos/arquitetura.md)
- [O núcleo e a configuração](guia/conceitos/nucleo-configuracao.md)
- [Certificado digital](guia/conceitos/certificado.md)
- [Conexões, TLS e tempos](guia/conceitos/conexoes-tls.md)
- [Resultados, problemas e exceções](guia/conceitos/resultados-problemas.md)
- [Validação: schema, regras e regras próprias](guia/conceitos/validacao.md)
- [Autorizadores, URLs e contingência SVC](guia/conceitos/autorizadores-urls.md)

## NF-e e NFC-e

- [Montar a nota](guia/nfe/montar-nota.md)
- [Consultar o status do serviço](guia/nfe/status-servico.md)
- [Autorizar notas](guia/nfe/autorizacao.md)
- [Consultar a situação de uma nota](guia/nfe/consulta-protocolo.md)
- [Quando a conexão cai: recuperação e duplicidade](guia/nfe/queda-recuperacao.md)
- [Cancelar e corrigir: eventos](guia/nfe/eventos.md)
- [Inutilizar numeração](guia/nfe/inutilizacao.md)
- [Ler um XML recebido](guia/nfe/leitura-xml.md)
- [Baixar as notas destinadas: distribuição DF-e](guia/nfe/distribuicao-dfe.md)
- [Manifestação do destinatário](guia/nfe/manifestacao.md)
- [Consulta cadastro](guia/nfe/consulta-cadastro.md)

## Específico da NFC-e

- [QR Code e CSC](guia/nfce/qrcode-csc.md)
- [Contingência offline](guia/nfce/contingencia-offline.md)
- [Cancelamento por substituição](guia/nfce/cancelamento-substituicao.md)

## Impressão

- [Modelo de impressão: DANFE e DANFCE](guia/impressao/modelo-impressao.md)
- [Gerar o PDF](guia/impressao/pdf.md)
- [Imprimir em impressora térmica (ESC/POS)](guia/impressao/escpos.md)

## Da homologação à produção

- [Passar para a homologação real e para a produção](guia/homologacao-producao.md)
- [Solução de problemas](guia/solucao-problemas.md)
- [Glossário](guia/glossario.md)
- [Usar a OpenDFe com agentes de IA](guia/agentes-ia.md): o prompt de contexto e, em cada página, um prompt pronto sobre o tema.

## Referência

- [Referência da API](referencia/index.md): todos os tipos e membros públicos, por namespace.
- [Catálogo de regras e rejeições](REGRAS.md): as regras do MOC que a biblioteca confere antes de transmitir e as que ficam para a SEFAZ.
- [Notas Técnicas acompanhadas](NTS.md): as NTs, os schemas e as tabelas que cada versão implementa.

## Documentos de projeto

Os documentos abaixo registram o desenho e as decisões da biblioteca. São úteis para quem contribui com o código ou quer entender o porquê de um comportamento.

| Documento | Conteúdo |
|---|---|
| [PROJETO.md](PROJETO.md) | Escopo, princípios e arquitetura |
| [CONTRATOS.md](CONTRATOS.md) | O desenho de cada tipo público, com as decisões |
| [MODELO.md](MODELO.md) | O desenho do modelo da NF-e |
| [LEITURA.md](LEITURA.md) | A leitura do XML e o detector |
| [IMPRESSAO.md](IMPRESSAO.md) | A conformidade do DANFE e do DANFCE com os manuais |
| [SEFAZ_EMULADA.md](SEFAZ_EMULADA.md) | A especificação completa da SEFAZ emulada |

## Licença

A OpenDFe é distribuída sob a [licença MIT](https://github.com/Davidsonrf1/OpenDFe/blob/main/LICENSE). O pacote `OpenDFe.Impressao.Pdf` depende do QuestPDF, que tem licença própria: leia [Gerar o PDF](guia/impressao/pdf.md) antes de usá-lo.

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
