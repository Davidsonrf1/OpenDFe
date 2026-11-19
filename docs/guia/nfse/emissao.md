# Emitir e consultar a NFS-e

[Documentação](../../index.md) › NFS-e › Emissão

A NFS-e do padrão nacional é emitida pela **Sefin Nacional**: quem presta o serviço monta a DPS (Declaração de Prestação de Serviços), assina e envia; a Sefin devolve a NFS-e gerada, com a chave de 50 posições. A OpenDFe usa a mesma porta da NF-e: `DFe.Criar` com o leiaute da NFS-e. O transporte é REST, com o XML compactado no JSON, e a biblioteca cuida disso.

## Criar o núcleo

```csharp
using OpenDFe;
using OpenDFe.NFSe;

using var conexoes = new ConexoesSefaz();
var dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,   // a produção restrita
    UF = UF.MG,                        // a do prestador; a NFS-e não a usa para escolher o destino
    Certificado = certificado,
    Conexoes = conexoes,
});
```

- **Um núcleo serve a todos os municípios.** O município vai dentro da DPS (`MunicipioEmissor`), e todas as operações vão à mesma Sefin Nacional.
- **Não há contingência**: `Emissao` diferente de `Normal` é recusada no `Criar`.

## Montar a DPS

```csharp
var dps = new Dps
{
    Ambiente = Ambiente.Homologacao,
    Emissao = DateTimeOffset.Now,
    Serie = "1",
    Numero = 42,                                   // a sua numeração da DPS
    Competencia = DateOnly.FromDateTime(DateTime.Today),
    Emitente = EmitenteDps.Prestador,              // a Sefin só aceita o prestador
    MunicipioEmissor = 3106200,                    // o código IBGE
    Prestador =
    {
        Cnpj = "11222333000181",
        RegimeTributacao = { SimplesNacional = SituacaoSimplesNacional.NaoOptante, RegimeEspecial = RegimeEspecialTributacao.Nenhum },
    },
    Tomador = new Pessoa { Cpf = "52998224725", Nome = "CLIENTE" },
    Servico =
    {
        Local = { Municipio = 3106200 },
        Codigo = { TributacaoNacional = "010101", Descricao = "Desenvolvimento de software" },
    },
    Valores =
    {
        Servico = { Valor = 1000.00m },
        Tributacao =
        {
            Municipal = { Tributacao = TributacaoIssqn.Tributavel, Retencao = RetencaoIssqn.NaoRetido },
            Total = { Indicador = IndicadorTotalTributos.NaoInformado },
        },
    },
};
```

- **Não informe o nome nem o endereço do prestador** quando ele é o emitente: o ambiente nacional os tira do cadastro, e a regra `NFSE-PRESTADOR` recusa a DPS que os traz.
- A biblioteca **não calcula** base, ISSQN, IBS nem CBS: a Sefin calcula e devolve na NFS-e.
- Os campos seguem o schema 1.01 em produção; os nomes do modelo estão em `docs/NFSE.md` 7.

## Emitir

```csharp
var resultado = await dfe.ExecutarAsync(new EmissaoNFSe(dps));
var retorno = resultado.Retorno!;
if (retorno.Emitida)
    Guardar(retorno.Chave!.Value, retorno.Xml!);   // o XML da NFS-e é o documento
else
    foreach (var erro in retorno.Erros)
        Console.WriteLine(erro);                   // "E0014: <descrição> (<complemento>)"
```

| Situação | O que volta |
|---|---|
| `Emitida` | a `Chave`, o `Xml` e a `Nota` lida |
| Rejeitada | `Erros` com o código da Sefin (`E0014`: a DPS já foi usada, com a chave da primeira no complemento) |
| `RecusadoLocalmente` | o schema, uma regra (`NFSE-*`) ou um callback recusou antes de transmitir: nada saiu |

O `CStat` do resultado é o status HTTP (201 na emissão), e o `Motivo`, o primeiro erro.

## Consultar

```csharp
var nota = await dfe.ExecutarAsync(new ConsultaNFSe(chave));                    // a NFS-e pela chave
var porDps = await dfe.ExecutarAsync(new ConsultaDps(IdentificadorDps.Montar(3106200, "11222333000181", "1", 42)));
var existe = await dfe.ExecutarAsync(new ConsultaDps(id) { ApenasExistencia = true });   // o HEAD, sem a chave
```

A consulta pela DPS devolve a chave da NFS-e gerada a partir dela (`Existe`, `Chave`).

## Quando a conexão cai

Se a emissão lançar `FalhaTransmissaoException` com `PodeTerSidoProcessado`, **não reenvie às cegas**: a DPS pode ter gerado a NFS-e. Consulte pela DPS:

```csharp
var recuperacao = await RecuperacaoEmissao.ExecutarAsync(dfe, IdentificadorDps.Montar(3106200, "11222333000181", "1", 42));
if (recuperacao.Gerada)
    Guardar(recuperacao.Nota!.Retorno!.Chave!.Value, recuperacao.Nota.Retorno.Xml!);
else if (recuperacao.NaoGerada)
    await dfe.ExecutarAsync(new EmissaoNFSe(dps));   // seguro: a DPS não gerou nada
```

O reenvio da mesma DPS que já gerou a NFS-e é rejeitado (`E0014`), com a chave dela no complemento.

## O que guardar

- O XML da NFS-e (`retorno.Xml`) e a chave.
- A DPS como você a montou, com a série e o número, para a recuperação e para a substituição.

## Prompt para agentes de IA

Peça a um agente que escreva a emissão. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva a emissão da NFS-e nacional com a OpenDFe, seguindo docs/guia/nfse/emissao.md: DFe.Criar(LeiauteNFSe101.Instancia,
Documento.NFSe, configuração), a Dps com o MunicipioEmissor, o prestador como emitente (sem nome e endereço), o serviço e os
valores; EmissaoNFSe; trate Emitida, Erros e RecusadoLocalmente; guarde a chave e o XML. Na FalhaTransmissaoException com
PodeTerSidoProcessado, use RecuperacaoEmissao antes de reenviar. Os dados vêm de <descreva>.
```

## Próximas etapas

- [Cancelar e substituir a NFS-e](eventos.md)
- [Consultar a parametrização do município](parametrizacao.md)
- [Imprimir o DANFSe](../impressao/danfse.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
