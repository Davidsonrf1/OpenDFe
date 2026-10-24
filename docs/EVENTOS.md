# Eventos e inutilização: o guia de quem usa

Para quem emite NF-e e NFC-e com a lib. O desenho está em `CONTRATOS.md` 18; a queda no meio de um envio, em `QUANDO_CAI.md` 9. Os trechos abaixo são os mesmos dos testes contra a SEFAZ emulada (`EventoEmuladoTestes`, `SubstituicaoEmuladaTestes`, `CartaCorrecaoEmuladaTestes`, `InutilizacaoEmuladaTestes`). Quem decide é a SEFAZ: a lib confere o que o schema confere e o que ela mesma recusa antes de transmitir (o modelo da chave, os 15 dígitos do protocolo, a sequência de 1 a 20); prazos, situação da nota e a conferência da substituta ficam para a resposta.

Todos os eventos vão por um só `EnvioEvento` (de 1 a 20 eventos, cada um assinado) e voltam em `RetornoEvento.Eventos`, um `ResultadoEvento` por evento.

## 1. Cancelar

O cancelamento (110111) vale para a NF-e e para a NFC-e, **até 24 horas depois da autorização** (501 depois disso; o cancelamento fora do prazo, quando a UF o aceita, volta como 155). Use o protocolo da autorização (`nProt`, 15 dígitos) e uma justificativa de 15 a 255 caracteres.

```csharp
var evento = new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, "Cancelamento por erro de digitacao");
var resultado = await dfe.ExecutarAsync(new EnvioEvento(evento), ct);
var cancelado = resultado.Retorno!.Eventos[0];   // Registrado: 135 (ou 136, 155)
var distribuivel = cancelado.XmlProcessado;      // o procEventoNFe
```

## 2. Cancelar por substituição (só NFC-e)

O cancelamento por substituição (110112) cancela uma NFC-e e aponta a NFC-e que a substitui (`chNFeRef`). As regras que a SEFAZ confere (MOC 7.0, Visão Geral, 5.9, Tabela 5-38):

- a NFC-e cancelada é da **emissão normal** (`tpEmis` 1), senão 920;
- a substituta é a NFC-e **emitida em contingência offline** (`tpEmis` 9, o QR Code offline), já **autorizada**, senão 921 (se for da emissão normal), 912 (não existe) ou 913 (cancelada ou denegada);
- a substituta é do mesmo emitente e da mesma UF, e do mesmo mês da cancelada ou do anterior (911), com a emissão até **2 horas depois** da emissão da cancelada (914);
- o mesmo valor total (915), o mesmo ICMS (916), o mesmo destinatário (917), a mesma quantidade de itens (918) e os mesmos itens (919: código, EAN, descrição, NCM, CFOP, unidade, quantidade, valor unitário, valor do item e `indTot`);
- o pedido sai **até 168 horas** (7 dias) depois da autorização da cancelada (501).

```csharp
var evento = new CancelamentoSubstituicao(cancelada.Chave!, cancelada.Protocolo!.Numero!,
    "Substituicao por erro de digitacao", substituta.Chave!)
{
    VersaoAplicativo = "PDV 1.0",   // verAplic: a versão do seu sistema (até 20 caracteres)
};
var resultado = await dfe.ExecutarAsync(new EnvioEvento(evento), ct);
var registrado = resultado.Retorno!.Eventos[0];  // Registrado: 135
```

Sem `VersaoAplicativo`, o `verAplic` sai "OpenDFe " e a versão do pacote. O evento vai ao autorizador da UF da chave; a SEFAZ devolve o `cOrgaoAutor` no `retEvento`, que fica no `XmlProcessado` (o `ResultadoEvento` não tem o campo). Depois do 135, a consulta da cancelada devolve 101 com o evento, e a substituta segue autorizada.

## 3. Corrigir (só NF-e)

A carta de correção (110110) **não existe para a NFC-e** (784, e o construtor recusa a chave de NFC-e). O texto da correção tem de 15 a 1000 caracteres; a lib acrescenta sozinha a condição de uso, que é o texto fixo do schema.

```csharp
var carta = new CartaCorrecao(nota.Chave!, sequencia: 1, "Correcao do endereco de entrega: Rua B, 2");
var resultado = await dfe.ExecutarAsync(new EnvioEvento(carta), ct);
```

- **A sequência é sua.** A lib não sabe qual é a próxima: a primeira carta é 1, a seguinte 2, até 20. Conte as cartas que a consulta da nota traz (`ConsultaProtocolo`, `RetornoProtocolo.Eventos`, um texto por evento registrado). A mesma sequência de novo é 573.
- **A carta mais recente substitui as anteriores** (MOC 7.0, 5.10): cada carta nova deve repetir o que continua valendo das anteriores.
- **O que a carta não pode corrigir** (a condição de uso, MOC 7.0, 5.10): as variáveis que determinam o valor do imposto (base de cálculo, alíquota, diferença de preço, quantidade, valor da operação ou da prestação); dados cadastrais que mudem o remetente ou o destinatário; a data de emissão ou de saída. A SEFAZ não confere o conteúdo: o erro é de quem emite.
- Não cabe em nota cancelada (580). Pode ir no mesmo envio que o cancelamento da mesma nota: a carta antes do cancelamento é registrada; depois dele, 580.

## 4. Inutilizar

A inutilização retira da numeração uma faixa de números que **não foram usados** (salto de numeração, nota que não saiu): CNPJ do emitente, série, número inicial e final, e uma justificativa de 15 a 255 caracteres. Um número da faixa que já foi usado é 241; um número que já está inutilizado, 256; o mesmo pedido de novo, 563.

```csharp
var resultado = await dfe.ExecutarAsync(new Inutilizacao("11.222.333/0001-81", 1, 10, 15, "Salto de numeracao"), ct);
var proc = resultado.Retorno!.XmlProcessado;     // o ProcInutNFe (102)
```

O emitente com CPF não tem inutilização neste schema (`CONTRATOS.md` 18.3, decisão 8).

## 5. O resultado, e o que guardar

- `ResultadoEvento.Registrado` é verdadeiro no 135, 136 e 155. Outro código é rejeição: o `Status` e o `Motivo` dizem qual.
- **Guarde o `XmlProcessado`** (o `procEventoNFe`, o evento assinado com o registro da SEFAZ): é o que se distribui e o que prova o evento. Guarde também o evento como você o montou, com o `Momento`, para o reenvio (os mesmos bytes e o mesmo `Id`).
- Na inutilização, guarde o `ProcInutNFe` (102); no 563, o protocolo da primeira.
- A lib não grava nada: tudo o que ela devolve fica com você.

## 6. Quando a conexão cai no meio

O evento pode ter sido registrado sem a resposta chegar. Não reenvie às cegas: consulte a chave (`ConsultaProtocolo`), que diz o que a SEFAZ registrou, e reenvie o **mesmo** evento só se ele não consta (a SEFAZ responde 573 ao repetido). O passo a passo, com o código, está em `QUANDO_CAI.md` 9.
