# O núcleo e a configuração

[Documentação](../../index.md) › Conceitos › O núcleo e a configuração

O núcleo (`DFe`) é o objeto pelo qual você executa as operações. Este artigo mostra como criá-lo, o que cada item da configuração faz e como organizar os núcleos num aplicativo real.

## Criar o núcleo

```csharp
using OpenDFe;
using OpenDFe.NFe;

using var certificado = CertificadoDigital.AbrirArquivoPfx("emitente.pfx", senha);
using var conexoes = new ConexoesSefaz();   // um por processo

var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
});
```

`DFe.Criar` chama a declaração do leiaute e confere a configuração contra ela. Ele lança:

| Exceção | Quando |
|---|---|
| `ArgumentException` | O leiaute não atende o documento; a configuração do documento é de outro tipo; a UF não tem autorizador para o tipo de emissão; tipo de emissão diferente de `Normal` sem `Contingencia` (ou `Normal` com ela); URL trocada de um serviço que o documento não tem; código vazio em `RegrasComoAviso` |
| `CryptographicException` | O certificado não tem a chave privada RSA |

`DFe.Criar` não confere a validade do certificado: um núcleo de vida longa pode ver o certificado vencer. Confira com `CertificadoDigital.Validade` (veja [Certificado digital](certificado.md)).

## A configuração

| Propriedade | Obrigatória | Descrição |
|---|---|---|
| `Ambiente` | sim | `Ambiente.Homologacao` ou `Ambiente.Producao` |
| `UF` | sim | A UF do emitente: o destino das operações sem documento nem chave |
| `Certificado` | sim | O `X509Certificate2` com a chave privada RSA. É seu: o núcleo não o descarta nem o copia |
| `Conexoes` | sim | O pool de conexões (`ConexoesSefaz`), compartilhado entre os núcleos |
| `Emissao` | não | O tipo de emissão da nota nova; padrão `TipoEmissao.Normal` |
| `Contingencia` | com `Emissao` diferente de `Normal` | O início e a justificativa da contingência (`dhCont` e `xJust` de toda nota nova) |
| `Urls` | não | Só as URLs que mudam; cada uma substitui a do leiaute com o mesmo autorizador, ambiente e serviço |
| `Documento` | não | A configuração que é do documento: na NFC-e, `ConfiguracaoNFCe` (o CSC e as URLs de consulta) |
| `Validacao` | não | Os seus callbacks de validação (`ValidacaoCallback`) |
| `RegrasComoAviso` | não | Códigos de regra da biblioteca rebaixados a aviso |
| `Relogio` | não | O `TimeProvider`; padrão `TimeProvider.System` |

`DFe.Criar` tira uma cópia das listas: mudar a lista depois não muda o núcleo.

## O que o núcleo lista

| Membro | O que devolve |
|---|---|
| `dfe.Operacoes` | As operações registradas para o documento (o tipo do parâmetro e do retorno) |
| `dfe.Suporta<TParametro>()` | Se o leiaute registrou a operação |
| `dfe.Atendimentos` | Qual autorizador atende cada UF em cada tipo de emissão |
| `dfe.Enderecos` | As URLs efetivas: as do leiaute com as suas trocadas por cima |

```csharp
foreach (var atendimento in dfe.Atendimentos.Where(a => a.UF == UF.MG))
    Console.WriteLine($"{atendimento.Emissao} → {atendimento.Autorizador.Sigla()}");
// Normal → MG, ContingenciaFSIA → MG, ContingenciaEpec → MG, ContingenciaFSDA → MG,
// ContingenciaOffline → MG, ContingenciaSvcAN → SVC-AN

foreach (var endereco in dfe.Enderecos.Where(e => e.Autorizador == Destino.MG && e.Ambiente == Ambiente.Homologacao))
    Console.WriteLine($"{endereco.Servico}: {endereco.Url}");
```

## Ciclo de vida e concorrência

- **O núcleo é imutável e seguro para uso concorrente.** Um núcleo atende chamadas em paralelo.
- **O núcleo não é `IDisposable`.** O certificado e as conexões são de quem os cria; descarte-os depois do último uso. Descartá-los com um núcleo em uso é erro de quem chama.
- **Criar um núcleo é barato.** O leiaute carrega os schemas uma vez; o `Criar` só monta dicionários. Entrar em contingência é criar outro núcleo com o mesmo `ConexoesSefaz`.
- **O cancelamento** vai no `CancellationToken` de cada `ExecutarAsync`. Cancelar depois de o pedido sair é, para a SEFAZ, a mesma queda no meio: veja [Quando a conexão cai](../nfe/queda-recuperacao.md).

## Vários emitentes

Num ERP com muitas empresas, mantenha um `ConexoesSefaz` por processo e um núcleo por emitente e documento:

```csharp
public sealed class Emissores(ConexoesSefaz conexoes, LeiauteNFe400 leiaute)
{
    private readonly ConcurrentDictionary<(string Cnpj, Documento Documento), DFe> _nucleos = new();

    public DFe Obter(Empresa empresa, Documento documento) =>
        _nucleos.GetOrAdd((empresa.Cnpj, documento), _ => DFe.Criar(leiaute, documento, new ConfiguracaoDFe
        {
            Ambiente = empresa.Ambiente,
            UF = empresa.UF,
            Certificado = empresa.Certificado,
            Conexoes = conexoes,
        }));
}
```

O `ConexoesSefaz` guarda um handler HTTP por certificado: as empresas não abrem conexões novas a cada núcleo.

## Com injeção de dependência

O `ConexoesSefaz` e o leiaute são singletons; o núcleo depende do emitente e é criado quando se sabe qual é.

```csharp
builder.Services.AddSingleton(_ => new ConexoesSefaz());
builder.Services.AddSingleton<LeiauteNFe400>();
builder.Services.AddSingleton<Emissores>();
```

O contêiner descarta o `ConexoesSefaz` quando o aplicativo termina.

## O relógio

O núcleo nunca lê `DateTimeOffset.Now` direto: o rastro, o `idLote`, o `dhEvento` e as regras de data usam o `Relogio` da configuração. Nos testes, passe um `TimeProvider` falso (por exemplo o `FakeTimeProvider` do pacote `Microsoft.Extensions.TimeProvider.Testing`) para controlar o tempo.

## Prompt para agentes de IA

Peça a um agente que crie os núcleos para várias empresas. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva um serviço C# que fornece o DFe da OpenDFe para cada empresa e documento do meu sistema, seguindo
docs/guia/conceitos/nucleo-configuracao.md. Os dados da empresa vêm de <repositório/banco>: CNPJ, UF, ambiente, PFX e senha <ou cofre>.
Use um ConexoesSefaz único (singleton, descartado no fim do aplicativo), um ConcurrentDictionary por (CNPJ, Documento) e
DFe.Criar com ConfiguracaoDFe imutável. Confira a validade do certificado com CertificadoDigital.Validade ao criar o núcleo e
avise quando faltar menos de <30> dias. Não descarte o certificado enquanto houver núcleo que o use.
```

## Próximas etapas

- [Certificado digital](certificado.md)
- [Conexões, TLS e tempos](conexoes-tls.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
