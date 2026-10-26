# Consulta cadastro

[Documentação](../../index.md) › NF-e e NFC-e › Consulta cadastro

A consulta cadastro (`CadConsultaCadastro4`) pergunta à SEFAZ de uma UF a situação cadastral de um contribuinte do ICMS: se ele está habilitado, a inscrição estadual, o nome, o endereço e o credenciamento para a NF-e. É útil antes de emitir uma NF-e para um cliente novo. A operação está registrada só no núcleo de NF-e.

## Consultar

Por CNPJ ou CPF (com ou sem máscara):

```csharp
var resultado = await dfe.ExecutarAsync(new ConsultaCadastro(UF.MG, "11.444.777/0001-61"));
var retorno = resultado.Retorno!;

if (retorno.Encontrado)                                   // 111 ou 112
{
    foreach (var contribuinte in retorno.Contribuintes)
    {
        Console.WriteLine($"{contribuinte.InscricaoEstadual} {contribuinte.Nome}");
        Console.WriteLine(contribuinte.Situacao);         // Habilitado ou NaoHabilitado
        Console.WriteLine(contribuinte.CredenciamentoNFe);
        Console.WriteLine($"{contribuinte.Endereco?.Logradouro}, {contribuinte.Endereco?.Numero} - {contribuinte.Endereco?.Municipio}");
    }
}
else
{
    Console.WriteLine($"{retorno.Status} {retorno.Motivo}");
}
```

Pela inscrição estadual (só os algarismos, sem máscara):

```csharp
var porInscricao = await dfe.ExecutarAsync(ConsultaCadastro.Inscricao(UF.MG, "0620000000000"));
```

## O retorno

| Membro de `RetornoCadastro` | Descrição |
|---|---|
| `Status`, `Motivo` | 111 (uma ocorrência), 112 (mais de uma), ou a rejeição |
| `Encontrado` | `Status` 111 ou 112 |
| `Contribuintes` | Um `ContribuinteCadastro` por inscrição achada |
| `UF`, `UFAtendimento`, `Consulta`, `VersaoAplicativo` | A UF consultada, a que atendeu, o momento da consulta e a versão do aplicativo |

| Membro de `ContribuinteCadastro` | Descrição |
|---|---|
| `InscricaoEstadual`, `Cnpj`, `Cpf`, `UF` | A identificação |
| `Situacao` | `SituacaoCadastral.Habilitado` ou `NaoHabilitado` |
| `CredenciamentoNFe`, `CredenciamentoCTe` | `CredenciamentoDFe`: `NaoCredenciado`, `Credenciado`, `ObrigatorioTotal`, `ObrigatorioParcial`, `NaoInformado` |
| `Nome`, `Fantasia`, `RegimeApuracao`, `Cnae` | Os dados cadastrais |
| `InicioAtividade`, `UltimaSituacao`, `Baixa` | As datas |
| `InscricaoUnica`, `InscricaoAtual` | A IE única e a atual |
| `Endereco` | `EnderecoCadastro`: logradouro, número, complemento, bairro, município (com o código) e CEP |

| Rejeição | Quando |
|---|---|
| 259, 261, 264 | O CNPJ, a IE ou o CPF não estão no cadastro da UF |
| 258, 260, 263 | O CNPJ, a IE ou o CPF inválidos |
| 265 | A UF consultada não é atendida por esse Web Service |
| 257 | O solicitante não está habilitado |

## Que UFs oferecem o serviço

A consulta vai ao autorizador da UF consultada. Têm o serviço as UFs com autorizador próprio (AM, BA, GO, MG, MS, MT, PE, PR, RS, SP) e, pela SVRS, AC, ES, RN, PB e SC. Nas outras, a biblioteca recusa antes de transmitir (`NFE-CADASTRO`). Se uma UF passar a oferecer o serviço, informe o destino para pular essa conferência: `new ConsultaCadastro(UF.RJ, cnpj) { Destino = Destino.SVRS }`.

> **Dica**
> Na [SEFAZ emulada](../sefaz-emulada/dados-teste.md), cadastre os contribuintes pela rota `/_controle/cadastros` para testar cada resposta.

## Prompt para agentes de IA

Peça a um agente que escreva a conferência do cadastro do cliente. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva a conferência do cadastro de um cliente antes de emitir a NF-e, com a ConsultaCadastro da OpenDFe, seguindo
docs/guia/nfe/consulta-cadastro.md: por CNPJ (new ConsultaCadastro(uf, cnpj)) ou por IE (ConsultaCadastro.Inscricao, só algarismos);
Encontrado (111, 112), a Situacao (Habilitado) e a InscricaoEstadual; 259/261/264 como não cadastrado. Lembre que só as UFs que oferecem
o serviço aceitam (NFE-CADASTRO nas outras) e que a operação só existe no núcleo de NF-e. Atualize o meu cadastro <descreva>.
```

## Próximas etapas

- [Montar a nota](montar-nota.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
