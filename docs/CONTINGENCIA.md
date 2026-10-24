# Contingência offline: emitir sem a SEFAZ e transmitir depois

Este guia é para quem emite NFC-e (e NF-e com o DANFE Simplificado Tipo 2, `tpImp` 6) com o OpenDFe e precisa continuar vendendo quando a rede ou a SEFAZ caiu. É a contingência offline, `tpEmis` 9: a nota é emitida e assinada na máquina, impressa com o QR Code offline, e transmitida quando a conexão voltar.

A lib **não guarda fila e não conta prazo**: ela devolve o XML pronto, e guardar, imprimir e transmitir é do aplicativo (`PROJETO.md` 9.2). Vale a regra do `QUANDO_CAI.md`: **a nota é o XML assinado**; guarde-o antes de qualquer outra coisa.

## 1. Emitir offline

Crie um núcleo com o tipo de emissão offline e a entrada em contingência (o início e o motivo). Sem a `Contingencia`, o núcleo nem é criado. O `dhCont` e o `xJust` da nota saem dela, e a nota não os traz.

```csharp
var offline = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,                 // o mesmo da emissão normal
    Conexoes = conexoes,                       // não é usada para preparar
    Emissao = TipoEmissao.ContingenciaOffline,
    Contingencia = new Contingencia(DateTimeOffset.Now, "Sem comunicacao com a SEFAZ (queda do link)"),
    Documento = new ConfiguracaoNFCe(),        // versão 3 do QR Code; com CodigoSeguranca, a versão 2
});

var preparado = offline.Preparar(nota);
if (!preparado.Preparado)
    return mostrar(preparado.Problemas);
guardar(preparado.Chave, preparado.Xml);       // antes de imprimir
```

O `Preparar` gera, assina e valida sem enviar. O XML sai com `tpEmis` 9, o `dhCont`, o `xJust` e o `infNFeSupl` com o QR Code offline. A justificativa tem de ter de 15 a 256 caracteres (o schema confere).

**Qual versão do QR Code.** A que a configuração diz, como na emissão normal: `new ConfiguracaoNFCe()` (ou nenhuma configuração) gera a **versão 3**; `new ConfiguracaoNFCe(csc)`, a **versão 2**. Nas duas, o QR Code offline leva o dia da emissão, o valor total e a prova de que a nota é aquela:

| Versão | Parâmetros | O que garante a nota |
|---|---|---|
| 3 | `chave\|3\|tpAmb\|dia\|vNF\|tipo do destinatário\|documento\|assinatura` | a assinatura RSA-SHA1 dos sete primeiros, com o certificado da nota (por isso o núcleo offline precisa do certificado com a chave privada) |
| 2 | `chave\|2\|tpAmb\|dia\|vNF\|digVal\|idCSC\|hash` | o `DigestValue` da assinatura da nota (`digVal`) e o hash com o CSC |

A NF-e com `tpImp` 6 só tem a versão 3. A NF-e com `tpEmis` 9 **sem** `tpImp` 6 é rejeitada (711): a contingência offline da NF-e é só com o DANFE Simplificado Tipo 2.

A lib calcula o `digVal` e a assinatura com a nota **como está**: não altere o XML depois do `Preparar` (nem um espaço), porque o `digVal` e a assinatura deixariam de conferir (397 e 583).

## 2. Imprimir

O DANFCE da nota offline leva a legenda de contingência e o QR Code; a impressão é da fase 7. O QR Code é o texto do `infNFeSupl/qrCode` do XML guardado.

## 3. Transmitir quando voltar

A norma manda transmitir a nota offline **dentro de 24 horas da emissão**. A lib **não impõe** esse prazo: depois dele a SEFAZ autoriza **fora de prazo** (`cStat` 150, no lugar do 100), e o `RetornoAutorizacao` já traz o código. Transmita o quanto antes.

A transmissão é a autorização normal do XML preparado, por um núcleo **normal**: o destino sai da chave, e o autorizador é o normal da UF.

```csharp
var normal = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes,
});

// uma nota: síncrona
var r = await normal.ExecutarAsync(new Autorizacao(xmlGuardado), ct);
var nota = r.Retorno!.Notas[0];
if (nota.Autorizada)                                   // 100, ou 150 (fora de prazo)
    guardar(nota.XmlProcessado);                       // o nfeProc

// várias: em lote (de 2 a 50), com a consulta do recibo
var lote = await normal.ExecutarAsync(new Autorizacao(xmls), ct);          // cStat 103, o recibo
var consulta = await normal.ExecutarAsync(new ConsultaRecibo(lote.Retorno!.Recibo!) { Xmls = xmls }, ct);
```

O lote tem de 2 a 50 notas do mesmo modelo; uma nota só vai pela síncrona. A queda no meio da transmissão (a conexão caiu depois de enviar) é a do `QUANDO_CAI.md`: recupere, não reenvie às cegas.

## 4. O que pode dar errado

- **A mesma nota transmitida duas vezes (204).** A SEFAZ já tem a nota: o `ResultadoNota.Duplicidade` traz a chave; recupere o `nfeProc` com `RecuperacaoAutorizacao` (veja o `QUANDO_CAI.md`). Não gere a nota de novo.
- **O QR Code que não confere (397, 464, 583).** O XML foi alterado depois do `Preparar`, ou o CSC da versão 2 não é o que a SEFAZ tem (464), ou foi revogado antes da emissão (463). Corrija a causa e transmita de novo, se a nota ainda não foi rejeitada de forma definitiva.
- **A nota offline que a SEFAZ rejeita.** Rejeição não gera nota autorizada, então não há o que cancelar. Corrija o dado, emita **outra nota com nova numeração** e, quando a numeração da rejeitada não for mais usada, **inutilize-a** (a inutilização é da fase 6).
- **A UF sem URL de consulta.** A tabela da lib tem só as UFs conferidas (`docs/NTS.md` 4): nas outras, informe `UrlQrCode` e `UrlChave` na `ConfiguracaoNFCe`; sem elas a montagem recusa a nota (`NFE-QRCODE`), offline também.
- **Sem o certificado com chave privada**, a versão 3 offline não é gerada (`NFE-QRCODE`).

## 5. Resumo

1. Crie o núcleo offline (`Emissao` e `Contingencia`), chame `Preparar` e **guarde o XML**.
2. Imprima o DANFCE com o QR Code do XML.
3. Quando a rede voltar, transmita o XML guardado (síncrona ou lote), dentro de 24 horas.
4. Guarde o `nfeProc` do que foi autorizado (100 ou 150). Trate o 204 como recuperação, não como erro.
