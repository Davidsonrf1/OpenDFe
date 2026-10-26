# Consultar o status do serviço

[Documentação](../../index.md) › NF-e e NFC-e › Status do serviço

A consulta do status (`NFeStatusServico4`) diz se o autorizador está operando. É a pergunta que o PDV e o ERP fazem para decidir se emitem normalmente ou entram em contingência.

## Consultar

```csharp
var resultado = await dfe.ExecutarAsync(new ConsultaStatus());
if (resultado.Processado)
{
    var status = resultado.Retorno;
    Console.WriteLine($"{status.Status} {status.Motivo}");      // 107 Serviço em Operação
    Console.WriteLine(status.Operando);                         // true
    Console.WriteLine(status.TempoMedio);                       // 00:00:01
}
```

`ConsultaStatus` é do núcleo (namespace `OpenDFe`): a mesma operação vale para todo documento, e cada leiaute registra a sua.

## O retorno

| Membro de `RetornoStatus` | Tag | Descrição |
|---|---|---|
| `Status` | `cStat` | 107 em operação; 108 paralisado momentaneamente; 109 paralisado sem previsão |
| `Motivo` | `xMotivo` | O texto do `cStat` |
| `Operando` | | `Status == 107` |
| `Ambiente` | `tpAmb` | O ambiente que respondeu |
| `VersaoAplicativo` | `verAplic` | A versão do aplicativo do autorizador |
| `UF` | `cUF` | A UF responsável pelo serviço |
| `Recebimento` | `dhRecbto` | O momento da resposta |
| `TempoMedio` | `tMed` | O tempo médio de resposta do serviço |
| `PrevisaoRetorno` | `dhRetorno` | Na paralisação, a previsão de volta, quando informada |
| `Observacao` | `xObs` | Uma observação do autorizador |

## O destino

Sem `Destino`, a consulta vai ao autorizador da UF do núcleo, no tipo de emissão do núcleo: o núcleo em SVC-AN consulta a SVC-AN. Para consultar outro autorizador com o mesmo núcleo, informe o destino:

```csharp
var svc = await dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.SvcAN });
var svrs = await dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.SVRS });
```

O Ambiente Nacional (`Destino.AN`) não tem o serviço de status: a chamada lança `NotSupportedException` sem transmitir.

## Decidir a contingência

```csharp
async Task<bool> AutorizadorNoArAsync(DFe dfe, CancellationToken ct)
{
    try
    {
        var resultado = await dfe.ExecutarAsync(new ConsultaStatus(), ct);
        return resultado.Processado && resultado.Retorno.Operando;
    }
    catch (FalhaTransmissaoException)
    {
        return false;   // sem resposta: trate como fora do ar
    }
}
```

Quando o autorizador está fora: na NF-e, a contingência SVC (veja [Autorizadores, URLs e contingência SVC](../conceitos/autorizadores-urls.md)); na NFC-e, a [contingência offline](../nfce/contingencia-offline.md).

> **Dica**
> Na SEFAZ emulada, paralise um autorizador pela aba **Autorizadores** do painel, ou com uma falha `paralisacao`, para testar esse fluxo. Veja [Simular falhas](../sefaz-emulada/falhas-comandadas.md).

## Prompt para agentes de IA

Peça a um agente que escreva o monitor do serviço. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva um monitor do status da SEFAZ com a OpenDFe, seguindo docs/guia/nfe/status-servico.md: a cada <N> minutos, ConsultaStatus
no autorizador da UF e na SVC (Destino = Destino.SvcAN ou SvcRS); considere fora do ar o Retorno.Operando falso (108, 109) e a
FalhaTransmissaoException; mostre o Motivo, a Observacao e a PrevisaoRetorno quando vierem. Use o TimeProvider da configuração
para os testes e um teste contra a SEFAZ emulada com a falha "paralisacao".
```

## Próximas etapas

- [Autorizar notas](autorizacao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
