# Contingência offline da NFC-e

[Documentação](../../index.md) › NFC-e › Contingência offline

Quando a rede ou a SEFAZ cai, a NFC-e pode ser emitida em **contingência offline** (`tpEmis` 9): a nota é gerada e assinada na máquina, impressa com o QR Code offline e transmitida quando a conexão voltar. Este artigo mostra os três passos. Vale também para a NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6).

> **Importante**
> A biblioteca **não guarda fila e não conta prazo**. Ela devolve o XML pronto; guardar, imprimir e transmitir é do seu aplicativo. A regra de sempre vale aqui com mais força: **a nota é o XML assinado**. Guarde-o antes de qualquer outra coisa.

## 1. Emitir offline

Crie um núcleo com o tipo de emissão offline e a entrada em contingência (o início e o motivo, de 15 a 256 caracteres). O `dhCont` e o `xJust` da nota saem dela.

```csharp
var offline = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,                 // o mesmo da emissão normal
    Conexoes = conexoes,                       // não é usado para preparar
    Emissao = TipoEmissao.ContingenciaOffline,
    Contingencia = new Contingencia(DateTimeOffset.Now, "Sem comunicacao com a SEFAZ (queda do link)"),
    Documento = new ConfiguracaoNFCe(),        // versão 3 do QR Code; com o CodigoSeguranca, a versão 2
});

var preparado = offline.Preparar(nota);
if (!preparado.Preparado)
    return Mostrar(preparado.Problemas);
Guardar(preparado.Chave!, preparado.Xml!);     // antes de imprimir
```

O `Preparar` gera, assina e valida sem enviar. O XML sai com `tpEmis` 9, o `dhCont`, o `xJust` e o `infNFeSupl` com o QR Code offline:

| Versão | Parâmetros | O que prova a nota |
|---|---|---|
| 3 | `chave\|3\|tpAmb\|dia\|vNF\|tipo do destinatário\|documento\|assinatura` | A assinatura RSA-SHA1 dos sete primeiros parâmetros, com o certificado da nota |
| 2 | `chave\|2\|tpAmb\|dia\|vNF\|digVal\|idCSC\|hash` | O `DigestValue` da assinatura da nota (`digVal`) e o hash com o CSC |

> **Importante**
> Não altere o XML depois do `Preparar`, nem um espaço. O `digVal` e a assinatura do QR Code foram calculados sobre a nota como está e deixariam de conferir (397 e 583).

- A versão 3 offline precisa do certificado com a chave privada (`NFE-QRCODE` sem ela).
- A NF-e com `tpEmis` 9 sem `tpImp` 6 é rejeitada (711): a contingência offline da NF-e é só com o DANFE Simplificado Tipo 2.

## 2. Imprimir

O DANFCE da nota offline leva a legenda "EMITIDA EM CONTINGÊNCIA" e "Pendente de autorização", e o QR Code offline. Monte-o a partir do XML guardado:

```csharp
var leiaute = new LeiauteNFe400();
var danfce = leiaute.Danfce(leiaute.Ler(preparado.Xml!), new OpcoesDanfce { Via = ViaDanfce.Consumidor });
```

Na contingência, imprima as vias que a legislação da UF pede (chame `Danfce` uma vez por via). Veja [Modelo de impressão](../impressao/modelo-impressao.md).

## 3. Transmitir quando a conexão voltar

A norma manda transmitir a nota offline **dentro de 24 horas da emissão**. A biblioteca não impõe esse prazo: depois dele, a SEFAZ autoriza **fora de prazo** (`cStat` 150, no lugar do 100), e o retorno já traz o código. Transmita o quanto antes.

A transmissão é a autorização normal do XML guardado, por um núcleo **normal**: o destino sai da chave, e o autorizador é o normal da UF.

```csharp
var normal = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
});

// Uma nota: síncrona.
var r = await normal.ExecutarAsync(new Autorizacao(xmlGuardado), ct);
var nota = r.Retorno!.Notas[0];
if (nota.Autorizada)                                   // 100, ou 150 (fora de prazo)
    Guardar(nota.XmlProcessado!);                      // o nfeProc

// Várias: em lote (de 2 a 50), com a consulta do recibo.
var lote = await normal.ExecutarAsync(new Autorizacao(xmlsGuardados), ct);
var consulta = await normal.ExecutarAsync(new ConsultaRecibo(lote.Retorno!.Recibo!) { Xmls = xmlsGuardados }, ct);
```

A queda no meio da transmissão é a de sempre: recupere, não reenvie às cegas. Veja [Quando a conexão cai](../nfe/queda-recuperacao.md).

## O que pode dar errado

| Situação | O que fazer |
|---|---|
| A mesma nota transmitida duas vezes (204) | A SEFAZ já tem a nota: recupere o `nfeProc` com `RecuperacaoAutorizacao`. Não gere a nota de novo |
| O QR Code que não confere (397, 464, 583) | O XML foi alterado depois do `Preparar`, ou o CSC não é o que a SEFAZ tem (464), ou foi revogado antes da emissão (463) |
| A nota offline rejeitada | A rejeição não gera nota autorizada: não há o que cancelar. Corrija o dado, emita outra nota com nova numeração e [inutilize](../nfe/inutilizacao.md) a numeração da rejeitada |
| A UF sem URL de consulta | Informe `UrlQrCode` e `UrlChave` na `ConfiguracaoNFCe` (`NFE-QRCODE`) |

## A substituição de uma NFC-e

Uma venda já autorizada que precisa ser refeita pode ser substituída por uma NFC-e emitida em contingência offline, com o [cancelamento por substituição](cancelamento-substituicao.md).

> **Dica**
> Na [SEFAZ emulada](../sefaz-emulada/index.md), a NFC-e offline transmitida até 24 horas depois da emissão é autorizada com o 100; com mais de 24 horas, com o 150. Use um `TimeProvider` falso na configuração para testar o prazo sem esperar.

## Prompt para agentes de IA

Peça a um agente que implemente a contingência offline do PDV. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Implemente a contingência offline da NFC-e com a OpenDFe no meu PDV, seguindo docs/guia/nfce/contingencia-offline.md:
detectar a queda; criar o DFe com Emissao = ContingenciaOffline e Contingencia (início e justificativa); Preparar cada venda e gravar
o XML numa fila local <descreva> antes de imprimir; imprimir o DANFCE com a legenda de contingência (Danfce com OpcoesDanfce.Via);
quando a rede voltar, transmitir a fila pelo DFe normal (síncrona ou lote de até 50, com ConsultaRecibo), dentro de 24 horas
(depois disso vem o 150). Não altere o XML depois do Preparar. Trate o 204 como recuperação e a rejeição com nova numeração e inutilização.
```

## Próximas etapas

- [Cancelamento por substituição](cancelamento-substituicao.md)
- [QR Code e CSC](qrcode-csc.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
