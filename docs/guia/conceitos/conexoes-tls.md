# Conexões, TLS e tempos

[Documentação](../../index.md) › Conceitos › Conexões, TLS e tempos

A comunicação com a SEFAZ é HTTPS com TLS mútuo: o cliente se autentica com o certificado do emitente. O `ConexoesSefaz` cuida disso. Este artigo mostra como criá-lo, como a biblioteca confia nos servidores da SEFAZ, os tempos de espera e como personalizar o handler HTTP.

## O ConexoesSefaz

O `ConexoesSefaz` é um pool de `HttpMessageHandler`, **um por certificado**. Ele é obrigatório na configuração do núcleo e é seu: crie um por processo, compartilhe entre todos os núcleos e descarte quando o aplicativo terminar.

```csharp
using var conexoes = new ConexoesSefaz();

var nfe = DFe.Criar(leiaute, Documento.NFe, new ConfiguracaoDFe { /* ... */ Conexoes = conexoes });
var nfce = DFe.Criar(leiaute, Documento.NFCe, new ConfiguracaoDFe { /* ... */ Conexoes = conexoes });
```

- O certificado do TLS mútuo fica no handler. Os núcleos do mesmo certificado reaproveitam as conexões abertas.
- O pool guarda uma cópia do certificado para o TLS, mas o núcleo assina com o seu (`ConfiguracaoDFe.Certificado`). Mantenha o certificado vivo enquanto houver núcleo que o use.
- O handler ocioso há mais de `TempoOcioso` (30 minutos) é descartado na transmissão seguinte, sem timer e nunca com o handler em uso.
- O `ConexoesSefaz` é seguro para uso concorrente.

## Os tempos

| Propriedade | Padrão | Descrição |
|---|---|---|
| `TempoConexao` | 15 segundos | O tempo para estabelecer a conexão (inclui o TLS) |
| `TempoResposta` | 60 segundos | Do envio ao fim da resposta, por transmissão |
| `TempoOcioso` | 30 minutos | Depois disso sem uso, o handler do certificado é descartado |

```csharp
using var conexoes = new ConexoesSefaz
{
    TempoConexao = TimeSpan.FromSeconds(10),
    TempoResposta = TimeSpan.FromSeconds(30),
};
```

> **Importante**
> Um `TempoResposta` curto demais faz a biblioteca desistir enquanto a SEFAZ ainda processa. Na autorização, isso é uma queda no meio: `FalhaTransmissaoException` com `PodeTerSidoProcessado` verdadeiro, e a nota pode ter sido autorizada. Veja [Quando a conexão cai](../nfe/queda-recuperacao.md).

## Como a biblioteca confia nos servidores

Nem todo autorizador usa certificado da ICP-Brasil no HTTPS: alguns usam ACs públicas (Sectigo, GlobalSign), que só o repositório da máquina tem; outros usam a raiz da ICP-Brasil, que o Windows tem e o Linux, em geral, não. Por isso o handler padrão:

1. aceita o que o repositório de certificados da máquina aceita;
2. quando o único erro é a cadeia, monta a cadeia de novo com as **ACs raiz da ICP-Brasil embutidas na biblioteca** (v5, v6, v7, v10 a v13);
3. nunca aceita nome de servidor divergente;
4. não confere a revogação do certificado do servidor, para que uma lista de revogação fora do ar não pare a emissão.

## Personalizar o handler

O construtor `ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler>)` recebe uma fábrica. A fábrica é chamada uma vez por certificado e recebe a `ConexaoSefaz`, com o certificado, as cifras do autorizador (quando o leiaute as declara) e o `CriarPadrao()`, que devolve o `SocketsHttpHandler` da biblioteca já configurado. Ajuste o handler padrão em vez de criar um do zero:

### Usar um proxy

```csharp
using System.Net;

using var conexoes = new ConexoesSefaz(conexao =>
{
    var handler = conexao.CriarPadrao();
    handler.Proxy = new WebProxy("http://proxy.empresa.local:3128");
    handler.UseProxy = true;
    return handler;
});
```

### Conferir a revogação

```csharp
using var conexoes = new ConexoesSefaz(conexao =>
{
    var handler = conexao.CriarPadrao();
    handler.SslOptions.CertificateRevocationCheckMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online;
    return handler;
});
```

### Confiar numa AC de teste

É o caso da [SEFAZ emulada](../sefaz-emulada/conectar.md): o validador padrão continua valendo, e a fábrica acrescenta a AC de teste só para as conexões da biblioteca.

> **Importante**
> Nunca troque a validação do servidor por `return true`. Isso aceitaria qualquer servidor no lugar da SEFAZ, inclusive um que capture as notas e o certificado do cliente.

## Cifras por autorizador

Um autorizador que só negocia cifras antigas pode ser atendido por uma política de cifras própria (`CipherSuitesPolicy`), declarada pelo leiaute para aquele autorizador, num handler separado, sem rebaixar o padrão da máquina inteira. O .NET só aplica a política no Linux; no Windows vale o padrão do sistema. A tabela do leiaute da NF-e está vazia hoje: nenhum autorizador conhecido exige.

## As falhas de transmissão

Uma falha de rede, TLS, HTTP ou SOAP vira `FalhaTransmissaoException`, com a espécie em `Tipo` e o `Rastro`:

| `TipoFalhaTransmissao` | Exemplos | `PodeTerSidoProcessado` |
|---|---|---|
| `Conexao` | DNS, conexão recusada | Não. Sim se a conexão caiu depois do envio |
| `Tls` | O servidor não é confiável, a negociação falhou | Não |
| `CertificadoRecusado` | O servidor recusou o certificado do cliente (o HTTP 403 sem envelope de alguns autorizadores) | Não |
| `TempoEsgotado` | O `TempoResposta` acabou | Sim |
| `Http` | HTTP diferente de 200 | Não no 4xx; sim no 5xx |
| `FalhaSoap` | A falha SOAP | Não no `Sender`; sim no `Receiver` |
| `RespostaIlegivel` | A resposta cortada ou que não se lê | Sim |

Veja [Resultados, problemas e exceções](resultados-problemas.md).

## Prompt para agentes de IA

Peça a um agente que configure as conexões. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Configure o ConexoesSefaz da OpenDFe para o meu ambiente, seguindo docs/guia/conceitos/conexoes-tls.md:
<proxy corporativo em ...? conferência de revogação? tempos de conexão e de resposta?>.
Use o construtor com a fábrica e parta sempre de conexao.CriarPadrao(), ajustando o SocketsHttpHandler devolvido.
Nunca troque a validação do certificado do servidor por "return true". Explique o risco de um TempoResposta curto na autorização
(a queda no meio, PodeTerSidoProcessado) e escolha valores com folga.
```

## Próximas etapas

- [Resultados, problemas e exceções](resultados-problemas.md)
- [Autorizadores, URLs e contingência SVC](autorizadores-urls.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
