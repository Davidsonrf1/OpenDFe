# Autorizadores, URLs e contingência SVC

[Documentação](../../index.md) › Conceitos › Autorizadores e URLs

Cada UF tem um autorizador para cada tipo de emissão, e cada autorizador tem uma URL por serviço e ambiente. Este artigo explica como a biblioteca escolhe a URL, como trocar uma URL que mudou e como emitir em contingência SVC quando o autorizador da UF está fora.

## Destinos e autorizadores

O enum `Destino` representa para onde uma operação vai: uma UF (com o código IBGE, como em `UF`) ou um autorizador virtual.

| Destino | Sigla | O que é |
|---|---|---|
| `Destino.MG`, `Destino.SP`... | `MG`, `SP` | Uma UF, resolvida pelo atendimento (a UF com autorizador próprio é o próprio autorizador) |
| `Destino.SVRS` | `SVRS` | A SEFAZ Virtual do Rio Grande do Sul |
| `Destino.SVAN` | `SVAN` | A SEFAZ Virtual do Ambiente Nacional |
| `Destino.SvcAN` | `SVC-AN` | A SEFAZ Virtual de Contingência do Ambiente Nacional |
| `Destino.SvcRS` | `SVC-RS` | A SEFAZ Virtual de Contingência do Rio Grande do Sul |
| `Destino.AN` | `AN` | O Ambiente Nacional (distribuição DF-e e manifestação do destinatário) |

`Destinos` tem os utilitários: `UF.MG.ComoDestino()`, `Destino.SvcAN.Sigla()` (`"SVC-AN"`), `Destinos.TentarLerSigla("SVRS", out var destino)` e `destino.TentarUF(out var uf)`.

## Como a URL é escolhida

A URL sai de quatro coisas: o **destino**, o **ambiente**, o **tipo de emissão** e o **serviço**.

1. **O destino.** O `Destino` do parâmetro, se você informou; senão o do documento ou da chave (a UF e o `tpEmis` da nota); senão a UF do núcleo.
2. **O autorizador.** Se o destino é uma UF, o atendimento do leiaute diz qual autorizador a atende naquele tipo de emissão (AC em emissão normal → SVRS; MG em SVC-AN → SVC-AN).
3. **A URL.** A do autorizador, no ambiente do núcleo, para o serviço da operação.

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, configuracao);   // UF = SC

var status = await dfe.ExecutarAsync(new ConsultaStatus());
Console.WriteLine(status.Rastro.Autorizador?.Sigla());   // SVRS
Console.WriteLine(status.Rastro.Url);                    // a URL do NFeStatusServico4 da SVRS
```

> **Observação**
> Quando a operação tem um documento ou uma chave, **o destino e o tipo de emissão saem dele**, e não da configuração do núcleo. A NFC-e emitida offline é transmitida depois pelo autorizador normal; a consulta e o cancelamento de uma nota emitida em SVC-AN vão à SVC-AN, porque o `tpEmis` 6 está na chave.

## Trocar uma URL

As URLs dos autorizadores vêm embutidas no leiaute. Quando um autorizador muda de endereço, você troca só a URL que mudou, sem esperar uma versão nova da biblioteca:

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Urls =
    [
        new EnderecoServico(Destino.MG, Ambiente.Producao, "NFeAutorizacao4",
            new Uri("https://novo-endereco.exemplo.gov.br/nfe/services/NFeAutorizacao4")),   // o endereço novo publicado pela UF
    ],
});
```

Cada `EnderecoServico` substitui a URL do leiaute com o mesmo autorizador, ambiente e serviço, ou se acrescenta à lista. A URL tem de ser absoluta e HTTPS. O resultado (as URLs do leiaute com as trocadas por cima) está em `dfe.Enderecos`.

Os nomes dos serviços são os do WSDL: `NFeAutorizacao4`, `NFeRetAutorizacao4`, `NFeConsultaProtocolo4`, `NFeStatusServico4`, `NFeInutilizacao4`, `NFeRecepcaoEvento4`, `NFeDistribuicaoDFe` e `CadConsultaCadastro4`.

> **Observação**
> As URLs de **consulta da NFC-e** (as do QR Code e da consulta pela chave) não são Web Services e não ficam em `ConfiguracaoDFe.Urls`: elas ficam na `ConfiguracaoNFCe`. Veja [QR Code e CSC](../nfce/qrcode-csc.md).

## Listar sem criar o núcleo

Para uma tela de configuração, antes de haver certificado, use a declaração do leiaute:

```csharp
var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFe);

foreach (var atendimento in declaracao.Atendimentos.Where(a => a.UF == UF.SC))
    Console.WriteLine($"{atendimento.Emissao}: {atendimento.Autorizador.Sigla()}");

foreach (var endereco in declaracao.Enderecos.Where(e => e.Autorizador == Destino.SVRS && e.Ambiente == Ambiente.Producao))
    Console.WriteLine($"{endereco.Servico}: {endereco.Url}");
```

## Contingência SVC

Quando o autorizador da UF está fora do ar, a NF-e pode ser emitida na SEFAZ Virtual de Contingência (SVC-AN ou SVC-RS, conforme a UF), com o tipo de emissão 6 ou 7. Na NFC-e, a contingência é a offline (veja [Contingência offline](../nfce/contingencia-offline.md)).

### 1. Detectar a indisponibilidade

```csharp
var status = await dfe.ExecutarAsync(new ConsultaStatus());
if (status.Processado && !status.Retorno.Operando)
{
    // 108 ou 109: o autorizador da UF está paralisado. Confira a SVC.
    var svc = await dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.SvcAN });
}
```

Uma `FalhaTransmissaoException` repetida também indica o autorizador fora. A decisão de entrar em contingência é do seu sistema (e, em geral, do usuário).

### 2. Criar o núcleo em contingência

A configuração é imutável: a contingência é outro núcleo, com o mesmo `ConexoesSefaz`. O tipo de emissão diferente de `Normal` exige a `Contingencia`, com o início e a justificativa (de 15 a 256 caracteres), que vão no `dhCont` e no `xJust` de cada nota.

```csharp
var emContingencia = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Emissao = TipoEmissao.ContingenciaSvcAN,
    Contingencia = new Contingencia(DateTimeOffset.Now, "SEFAZ MG indisponivel, emissao em SVC-AN"),
});
```

### 3. Emitir as notas novas

```csharp
var resultado = await emContingencia.ExecutarAsync(new Autorizacao(notaNova));
// resultado.Rastro.Autorizador == Destino.SvcAN
```

A chave da nota leva o `tpEmis` 6: é outra chave que a da emissão normal, mesmo com a mesma numeração.

> **Importante**
> Não reemita em SVC uma nota que ficou **em dúvida** no autorizador normal (a conexão caiu e você não sabe se ela foi autorizada). Ela pode estar autorizada lá, e a mesma numeração com outra chave seria rejeitada com 539. Emita as notas novas em SVC com números novos, e resolva a nota em dúvida quando o autorizador normal voltar. Veja [Quando a conexão cai](../nfe/queda-recuperacao.md).

### 4. Depois

- Volte ao núcleo normal quando o status do autorizador da UF responder 107.
- A consulta e o cancelamento de uma nota emitida em SVC vão à SVC, pela chave. A SVC só registra o cancelamento; a carta de correção vai ao autorizador normal.
- O autorizador normal também conhece as notas da SVC. Para consultar lá, informe o destino: `new ConsultaProtocolo(chave) { Destino = Destino.MG }`.

## Prompt para agentes de IA

Peça a um agente que implemente a entrada em contingência SVC. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Implemente a contingência SVC da NF-e com a OpenDFe para a UF <...>, seguindo docs/guia/conceitos/autorizadores-urls.md:
detectar o autorizador fora (ConsultaStatus com Retorno.Operando falso, ou FalhaTransmissaoException repetida); conferir a SVC
(ConsultaStatus com Destino = Destino.SvcAN ou SvcRS); criar outro DFe com Emissao = ContingenciaSvcAN (ou SvcRS) e Contingencia
(início e justificativa de 15 a 256 caracteres); emitir só notas novas com números novos; não reemitir na SVC a nota que ficou em dúvida
no autorizador normal; voltar ao núcleo normal quando o status responder 107. A decisão de entrar em contingência é <automática / do usuário>.
```

## Próximas etapas

- [Autorizar notas](../nfe/autorizacao.md)
- [Simular falhas na SEFAZ emulada](../sefaz-emulada/falhas-comandadas.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
