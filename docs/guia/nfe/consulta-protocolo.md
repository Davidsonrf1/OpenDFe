# Consultar a situação de uma nota

[Documentação](../../index.md) › NF-e e NFC-e › Consultar a situação

A consulta do protocolo (`NFeConsultaProtocolo4`) responde "como está esta nota agora?": autorizada, cancelada, desconhecida. Ela traz o protocolo da autorização e os eventos registrados (o cancelamento, as cartas de correção).

## Consultar pela chave

```csharp
var chave = ChaveAcesso.Ler("31261011222333000181550010000045671123456789");

var resultado = await dfe.ExecutarAsync(new ConsultaProtocolo(chave));
var situacao = resultado.Retorno!;

Console.WriteLine($"{situacao.Status} {situacao.Motivo}");
if (situacao.Autorizada)          // 100 ou 150
    Console.WriteLine($"Protocolo {situacao.Protocolo!.Numero} de {situacao.Protocolo.Recebimento}");
if (situacao.Cancelada)           // 101, 151 ou 155
    Console.WriteLine("Cancelada");

foreach (var evento in situacao.Eventos)
    Console.WriteLine(evento);    // cada procEventoNFe, como texto
```

`ChaveAcesso.Ler` lança `ArgumentException` com a chave inválida (o tamanho, o dígito, a UF, o modelo); `ChaveAcesso.TentarLer` devolve `false` no lugar. A chave errada não sai do computador. A chave também traz as partes: `chave.UF`, `chave.Ano`, `chave.Mes`, `chave.Emitente`, `chave.Modelo`, `chave.Serie`, `chave.Numero`, `chave.TipoEmissao`, `chave.CodigoNumerico`.

## O retorno

| Membro de `RetornoProtocolo` | Descrição |
|---|---|
| `Status`, `Motivo` | 100, 150 (autorizada), 101 (cancelada), 217 (não consta), 562 (outro `cNF` na base), ou a rejeição |
| `Autorizada` | `Status` 100 ou 150 |
| `Cancelada` | `Status` 101, 151 ou 155 |
| `Protocolo` | O protocolo da autorização, quando a nota consta |
| `Eventos` | Cada `procEventoNFe` registrado, como texto XML, na ordem da resposta |
| `ChaveRegistrada` | No 562, a chave da nota da base, quando a SEFAZ a informa |
| `Chave`, `Ambiente`, `UF`, `Recebimento`, `VersaoAplicativo` | Os outros campos da resposta |

## O destino

A consulta vai ao autorizador da UF e do tipo de emissão **da chave**: a nota emitida em SVC-AN é consultada na SVC-AN. A SVC só acha as notas que ela autorizou; o autorizador normal acha também as da SVC. Para consultar no normal, informe o destino:

```csharp
var noNormal = await dfe.ExecutarAsync(new ConsultaProtocolo(chave) { Destino = Destino.MG });
```

A chave de NFC-e num núcleo de NF-e (ou o contrário) é recusada antes de transmitir (`NFE-CHAVE`): em várias UFs, os Web Services da NF-e e da NFC-e são outros.

## Contar as cartas de correção

A sequência da próxima carta de correção é sua. Conte as que a consulta traz:

```csharp
var cartas = situacao.Eventos.Count(xml => xml.Contains("<tpEvento>110110</tpEvento>", StringComparison.Ordinal));
var proxima = new CartaCorrecao(chave, sequencia: cartas + 1, "Correcao do endereco de entrega: Rua B, 2");
```

Para ler o evento inteiro, carregue o texto num `XDocument`.

## Próximas etapas

- [Quando a conexão cai: recuperação e duplicidade](queda-recuperacao.md)
- [Cancelar e corrigir: eventos](eventos.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
