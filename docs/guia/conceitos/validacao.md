# Validação: schema, regras e regras próprias

[Documentação](../../index.md) › Conceitos › Validação

A OpenDFe valida todo documento antes de transmitir, em três etapas. Este artigo explica cada uma, como rebaixar uma regra a aviso e como acrescentar as regras de negócio do seu sistema.

| Etapa | O que confere | Sempre? |
|---|---|---|
| 1. **Schema** | O XML contra o XSD do pacote de liberação vigente, embutido na biblioteca | Sim |
| 2. **Regras do leiaute** | As regras do MOC que pegam os erros comuns que o schema não vê | Sim (pode rebaixar a aviso) |
| 3. **Callbacks** | As regras de negócio do seu sistema | Se você configurar |

Cada etapa só roda se a anterior não deu erro; dentro da etapa, todas as regras rodam. Com qualquer problema de severidade `Erro`, o resultado é `RecusadoLocalmente` e nada é enviado.

## O schema

O núcleo valida a mensagem assinada contra o XSD que o leiaute entrega para aquela operação (a nota, o lote, o evento, a inutilização). Os schemas vêm embutidos, sem disco nem rede, e são carregados uma vez.

O problema de schema leva o código que a SEFAZ daria (225 para a nota, 215 para as outras mensagens), a mensagem em português montada a partir da regra do schema e, em `Problema.Schema`, o detalhe estruturado: o tipo, as facetas violadas, os elementos esperados, a documentação do campo no XSD. Veja [Resultados, problemas e exceções](resultados-problemas.md).

## As regras do leiaute

O schema não vê relações entre campos. As regras do leiaute cobrem as que são erro comum de quem emite e quase não mudam com as Notas Técnicas:

- o CPF e o CNPJ (inclusive o alfanumérico), e o dígito das chaves referenciadas;
- os totais contra a soma dos itens (564, 610 e afins);
- combinações simples, como o CFOP contra o `idDest` e o `indIEDest` contra a inscrição estadual;
- as datas (emissão no futuro, a NFC-e atrasada);
- o QR Code e a contingência da NFC-e.

Cada regra leva o código da rejeição que ela antecipa e a mensagem do MOC. A regra obrigatória sai como `Erro`; a facultativa (a UF escolhe se aplica), como `Aviso`.

As regras que dependem de tabela oficial (CST e `cClassTrib` do IBS e da CBS, CFOP, combustíveis) ou que mudam a cada NT ficam para a SEFAZ. O [catálogo das regras](../../REGRAS.md) lista as que a biblioteca confere e as que só a SEFAZ confere.

### Rebaixar uma regra a aviso

Se a biblioteca aplicar uma regra que o seu autorizador não aplica (uma divergência de interpretação, uma NT que mudou), você não precisa parar de emitir até sair a versão corrigida. Rebaixe o código a aviso:

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.SP,
    Certificado = certificado,
    Conexoes = conexoes,
    RegrasComoAviso = ["610"],
});
```

O problema continua no resultado, com severidade `Aviso`, e a nota segue para a SEFAZ. `RegrasComoAviso` não vale para o schema nem para os seus callbacks.

## As suas regras: callbacks

`ConfiguracaoDFe.Validacao` recebe dois callbacks, ambos opcionais:

| Callback | Chamado | Recebe |
|---|---|---|
| `Campo` | Uma vez por campo do XML (elemento com texto ou atributo), fora da assinatura | `CampoVerificado`: o documento, a `Tag`, o `Caminho`, a `Propriedade` do modelo e o `Texto` como está no XML |
| `Documento` | Uma vez por documento (cada nota do lote) | `DocumentoVerificado`: o documento, o `Modelo` (a `NotaFiscal`, quando veio do modelo), o `Xml` assinado e a `Chave` |

Cada callback devolve os problemas que achou (vazio quando está tudo certo). Use um prefixo próprio no código, para distinguir das regras da SEFAZ.

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Validacao = new ValidacaoCallback
    {
        // Nenhum produto com "TESTE" na descrição.
        Campo = campo => campo.Tag == "xProd" && campo.Texto.Contains("TESTE", StringComparison.OrdinalIgnoreCase)
            ? [new Problema("PDV-01", Severidade.Erro, "Produto de teste na nota.") { Caminho = campo.Caminho, Propriedade = campo.Propriedade }]
            : [],

        // Venda acima de R$ 10.000,00 na NFC-e exige o consumidor identificado.
        Documento = documento => documento.Modelo is NotaFiscal { Destinatario: null } nota
                                 && nota.Total.Icms.ValorNota > 10_000m
            ? [new Problema("PDV-02", Severidade.Erro, "Identifique o consumidor em vendas acima de R$ 10.000,00.")]
            : [],
    },
});
```

O resultado da regra própria sai junto com os outros:

```
Erro PDV-01: Produto de teste na nota. (Notas[0].Itens[0].Produto.Descricao)
```

- Os callbacks rodam depois do schema e das regras do leiaute, sem erro neles.
- Eles podem ser chamados em paralelo (o núcleo é compartilhado): não guarde estado mutável sem sincronização.
- Exceção dentro de um callback sobe como veio: é tratada como erro de programação.
- O `Modelo` é `null` quando o documento veio já assinado (o XML de um `Preparar`, o reenvio): use o `Xml`.

## Validar sem transmitir

`dfe.Preparar(nota)` percorre o mesmo caminho da autorização até os callbacks (monta, assina, valida pelo schema, aplica as regras e os callbacks) sem enviar nada:

```csharp
var preparado = dfe.Preparar(nota);
if (!preparado.Preparado)
{
    foreach (var problema in preparado.Problemas)
        Console.WriteLine(problema);
    return;
}
Guardar(preparado.Chave, preparado.Xml);   // o XML assinado e válido, pronto para transmitir
```

É o caminho para mostrar os erros na tela antes de emitir, e o caminho seguro para emitir: guarde o XML e só depois transmita (veja [Autorizar notas](../nfe/autorizacao.md)).

## Próximas etapas

- [Montar a nota](../nfe/montar-nota.md)
- [Catálogo de regras e rejeições](../../REGRAS.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
