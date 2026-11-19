# Consultar a parametrização do município

[Documentação](../../index.md) › NFS-e › Parametrização municipal

O Ambiente de Dados Nacional (ADN) publica o que cada município configurou no Sistema Nacional: o convênio, as alíquotas do ISSQN, os regimes especiais, as retenções e os benefícios. Antes de emitir num município novo, vale conferir se ele é conveniado e usa o emissor nacional (as rejeições E0037 a E0039 vêm daí).

| Consulta | Parâmetros | Retorno |
|---|---|---|
| `ConsultaConvenio` | município | `RetornoConvenio` (aderente ao ambiente e ao emissor nacional) |
| `ConsultaAliquota` | município, código de tributação nacional, competência (nula: o histórico) | `RetornoAliquota` (as vigências) |
| `ConsultaRegimesEspeciais` | município, código de tributação nacional, competência | `RetornoRegimesEspeciais` |
| `ConsultaRetencoes` | município, competência | `RetornoRetencoes` |
| `ConsultaBeneficio` | município, número do benefício, competência | `RetornoBeneficio` |

## Consultar

```csharp
var convenio = await dfe.ExecutarAsync(new ConsultaConvenio(3106200));
if (convenio.Retorno!.Encontrado && convenio.Retorno.AderenteEmissorNacional == true)
{
    var aliquota = await dfe.ExecutarAsync(new ConsultaAliquota(3106200, "010101") { Competencia = new DateOnly(2026, 11, 1) });
    foreach (var vigencia in aliquota.Retorno!.Aliquotas)
        Console.WriteLine($"{vigencia.Aliquota}% de {vigencia.Inicio:d}");
}
else
{
    Console.WriteLine(convenio.Retorno.Mensagem);   // o 404 do município sem convênio
}
```

- O município é o código IBGE de 7 dígitos; o código de serviço, o `cTribNac` de 6 dígitos (com o complemento municipal opcional, `"000"` por padrão).
- **Os campos são os que se conseguiu confirmar.** O Swagger oficial só abre com o certificado ICP-Brasil, e os manuais não dão o JSON: todo retorno traz o `Json` inteiro ao lado, para o que a biblioteca ainda não lê. O campo que não se lê fica nulo, com o aviso `NFSE-PARAMETRO` nos problemas.

> **Atenção**
> As rotas da parametrização vêm de implementações abertas que emitem na produção restrita; o manual dos contribuintes as escreve de outro jeito. A conferência com o PFX real está pendente (`docs/NFSE.md` D13).

## Prompt para agentes de IA

Peça a um agente que consulte a parametrização. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Consulte a parametrização municipal da NFS-e com a OpenDFe, seguindo docs/guia/nfse/parametrizacao.md: ConsultaConvenio(municipio)
antes de emitir, e ConsultaAliquota(municipio, cTribNac) com a competência. Trate Encontrado e Mensagem; use o Json para o que
o retorno não tipa. O município e o serviço vêm de <descreva>.
```

## Próximas etapas

- [Emitir a NFS-e](emissao.md)
- [Imprimir o DANFSe](../impressao/danfse.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
