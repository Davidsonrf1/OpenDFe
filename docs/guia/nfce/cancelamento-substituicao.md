# Cancelamento por substituição da NFC-e

[Documentação](../../index.md) › NFC-e › Cancelamento por substituição

O cancelamento por substituição (evento 110112) cancela uma NFC-e e aponta a NFC-e que a substitui. Ele serve à venda que precisa ser refeita depois do prazo do cancelamento comum (24 horas): vale **até 168 horas** (7 dias) depois da autorização da cancelada.

## As regras

A SEFAZ confere (MOC 7.0, Visão Geral, 5.9):

- a NFC-e cancelada é da **emissão normal** (`tpEmis` 1), senão 920;
- a substituta é uma NFC-e **emitida em contingência offline** (`tpEmis` 9), já **autorizada**: senão 921 (se for da emissão normal), 912 (não existe) ou 913 (cancelada);
- a substituta é do mesmo emitente e da mesma UF, e do mesmo mês da cancelada ou do anterior (911), com a emissão até **2 horas depois** da emissão da cancelada (914);
- o mesmo valor total (915), o mesmo ICMS (916), o mesmo destinatário (917), a mesma quantidade de itens (918) e os mesmos itens (919: código, EAN, descrição, NCM, CFOP, unidade, quantidade, valor unitário, valor do item e `indTot`);
- o pedido sai até 168 horas depois da autorização da cancelada (501).

A biblioteca confere o que vai no evento (a chave de NFC-e, o protocolo de 15 dígitos, a justificativa); as regras acima ficam para a SEFAZ.

## Cancelar por substituição

```csharp
var evento = new CancelamentoSubstituicao(
    cancelada.Chave!,
    cancelada.Protocolo!.Numero!,
    "Substituicao por erro de digitacao no item",
    substituta.Chave!)
{
    VersaoAplicativo = "MeuPDV 1.0",   // o verAplic: a versão do seu sistema (até 20 caracteres)
    Momento = DateTimeOffset.Now,
};

var envio = await nfce.ExecutarAsync(new EnvioEvento(evento));
var registrado = envio.Retorno!.Eventos[0];
if (registrado.Registrado)              // 135
    Guardar(registrado.XmlProcessado!);
```

- Sem `VersaoAplicativo`, o `verAplic` sai "OpenDFe " seguido da versão do pacote.
- O evento vai ao autorizador normal da UF da chave.
- Depois do 135, a consulta da cancelada devolve 101 com o evento, e a substituta segue autorizada.

## O fluxo completo

1. Emita a substituta em contingência offline, com o mesmo valor, o mesmo destinatário e os mesmos itens da que será cancelada, até 2 horas depois da emissão dela (veja [Contingência offline](contingencia-offline.md)).
2. Transmita a substituta e espere o 100.
3. Envie o `CancelamentoSubstituicao` da original, apontando a substituta.

## Próximas etapas

- [Cancelar e corrigir: eventos](../nfe/eventos.md)
- [Contingência offline](contingencia-offline.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
