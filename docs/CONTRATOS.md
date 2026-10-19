# Contratos do núcleo (etapa 1.6)

Desenho dos tipos públicos que ligam quem usa, o núcleo e o leiaute: a criação do núcleo, a declaração do leiaute, as operações, o resultado e os callbacks de validação. **Aprovado em 2026-10-19** (as 17 recomendações da seção 8) e implementado na etapa 1.6, com as mudanças do usuário da seção 11 e os desvios da implementação da seção 12. A porta se chama `DFe` (`DFe.Criar`), não `Nucleo`, por decisão do usuário em 2026-10-19; "núcleo" continua sendo o pacote `OpenDFe.Nucleo`. O modelo é o `PROJETO.md` (3, 6.3, 8); onde este desenho diverge dele, a divergência está apontada numa das decisões (seção 8) e o `PROJETO.md` foi atualizado com ela na implementação.

Critério: a menor superfície pública que atende as fases 2 a 8, fácil de estender em versão menor (12.3). Tudo no namespace `OpenDFe`, no pacote `OpenDFe.Nucleo`, como os tipos das etapas 1.1 a 1.5.

## 1. Visão geral

```
quem usa                         núcleo                                   leiaute (OpenDFe.NFe)
--------                         ------                                   ---------------------
DFe.Criar(leiaute, doc, cfg) ─► DeclaracaoLeiaute.Criar(leiaute, doc) ► ILeiaute.Declarar(declaracao)
                                   confere a configuração                   RegistrarOperacao, DeclararAtendimento,
                                                                            DeclararEndereco, DeclararConfiguracaoDocumento
dfe.ExecutarAsync(parametro) ─► acha o IOperacao pelo tipo ──────────► Montar(parametro, contexto) → PedidoWS
                                   recusa local? assina, valida (XSD)
                                   escolhe a URL, SOAP 1.2 (4.1)
                                   ◄──────────────────────────────────── Ler(parametro, xmlRetorno, contexto) → LeituraWS
ResultadoWS<TRetorno> ◄───────── junta problemas, rastro
```

| Tipo | Papel |
|---|---|
| `DFe` | a porta: criado com um leiaute, um documento e uma configuração; executa operações e lista o que o leiaute declara |
| `ConfiguracaoDFe` | ambiente, UF, tipo de emissão, certificado, conexões, URLs trocadas, a parte do documento, validação, relógio |
| `ConfiguracaoDocumento` | base abstrata da parte da configuração que é do documento (o CSC da NFC-e, `ConfiguracaoNFCe` no leiaute) |
| `Contingencia` | início e justificativa da entrada em contingência (`dhCont`, `xJust`) |
| `ConexoesSefaz` | o pool de `HttpMessageHandler` por certificado (8.5), de quem usa, compartilhado entre núcleos |
| `ILeiaute` | o que um leiaute implementa: versão, documentos e a declaração |
| `DeclaracaoLeiaute` | onde o leiaute declara operações, atendimentos, URLs e a configuração do documento; depois, a listagem |
| `OperacaoSuportada`, `Atendimento`, `EnderecoServico` | as linhas da listagem: operação; UF × tipo de emissão → autorizador; autorizador × ambiente × serviço → URL |
| `Destino`, `Destinos` | as UFs, o AN e os autorizadores virtuais (SVRS, SVAN, SVC-AN, SVC-RS) |
| `ParametroWS`, `ParametroWS<TRetorno>` | a base dos parâmetros das operações; o genérico diz o tipo do retorno |
| `IOperacao<TParametro, TRetorno>` | o handler de uma operação no leiaute: monta o pedido e lê o retorno |
| `ContextoOperacao`, `PedidoWS`, `LeituraWS<TRetorno>` | o que o handler recebe e devolve |
| `ResultadoWS<TRetorno>`, `SituacaoResultado`, `RastroWS` | o resultado da operação, com o rastro (XMLs, URL, tempos) |
| `FalhaTransmissaoException` | a falha de infraestrutura na transmissão, com o rastro e se o pedido pode ter sido processado |
| `ValidacaoCallback`, `CampoVerificado`, `DocumentoVerificado` | os callbacks de validação de quem usa (6.3) |

## 2. Tipos públicos

Os blocos mostram só os membros públicos, sem corpo, com a documentação essencial. "Construtor interno" quer dizer que só o núcleo cria a instância.

### 2.1. `DFe`

O núcleo de um leiaute, um documento e uma configuração (3.2). Imutável e seguro para uso concorrente.

```csharp
public sealed class DFe
{
    /// Cria o núcleo. Chama ILeiaute.Declarar e confere a configuração contra a declaração.
    /// ArgumentException: o leiaute não atende o documento; a configuração do documento falta (quando o leiaute a
    ///   declara obrigatória) ou é de outro tipo; a UF não tem atendimento para o tipo de emissão; tipo de emissão
    ///   diferente de Normal sem Contingencia; URL trocada inválida.
    /// CryptographicException: o certificado não tem chave privada RSA (o ExigirChavePrivadaRsa da 1.3).
    /// Não confere a validade nem o titular do certificado (CertificadoDigital.Validade e LerTitular são de quem usa).
    public static DFe Criar(ILeiaute leiaute, Documento documento, ConfiguracaoDFe configuracao);

    public ILeiaute Leiaute { get; }
    public Documento Documento { get; }
    public ConfiguracaoDFe Configuracao { get; }

    /// As operações que o leiaute registrou para o documento.
    public IReadOnlyList<OperacaoSuportada> Operacoes { get; }

    /// Qual autorizador atende cada UF em cada tipo de emissão (a tabela do leiaute).
    public IReadOnlyList<Atendimento> Atendimentos { get; }

    /// As URLs efetivas: as do leiaute com as trocadas na configuração por cima.
    public IReadOnlyList<EnderecoServico> Enderecos { get; }

    /// O leiaute registrou um handler para este tipo de parâmetro?
    public bool Suporta<TParametro>() where TParametro : ParametroWS;

    /// Executa uma operação: montar → recusa local → assinar → schema → SOAP → ler (seção 3).
    /// O C# deduz TRetorno do parâmetro: ExecutarAsync(new ConsultaStatus()) é Task<ResultadoWS<RetornoStatus>>.
    /// NotSupportedException: o leiaute não registrou a operação, ou não há URL para o autorizador, o ambiente e o serviço.
    /// FalhaTransmissaoException: falha de rede, TLS, HTTP, SOAP ou resposta ilegível (2.4).
    /// OperationCanceledException: cancelado.
    public Task<ResultadoWS<TRetorno>> ExecutarAsync<TRetorno>(
        ParametroWS<TRetorno> parametro, CancellationToken cancellationToken = default)
        where TRetorno : class;
}
```

Vêm depois, como membros novos (versão menor): `Detectar` (estático, 2.5), `Preparar` (4.4 e 5.2) e a impressão (7.2).

### 2.2. `ConfiguracaoDFe`, `ConfiguracaoDocumento`, `Contingencia`

```csharp
/// A configuração de um núcleo. O Criar tira uma cópia das listas: mudar a lista depois não muda o núcleo.
public sealed class ConfiguracaoDFe
{
    public required Ambiente Ambiente { get; init; }
    /// A UF do emitente: o destino natural das operações sem documento nem chave (8.4).
    public required UF UF { get; init; }
    /// O certificado A1 ou A3 com a chave privada RSA. De quem chama: o núcleo não o descarta nem o copia.
    public required X509Certificate2 Certificado { get; init; }
    /// O pool de conexões, compartilhado entre os núcleos (2.3).
    public required ConexoesSefaz Conexoes { get; init; }

    /// O tipo de emissão da nota nova (8.4). Diferente de Normal exige Contingencia.
    public TipoEmissao Emissao { get; init; } = TipoEmissao.Normal;
    public Contingencia? Contingencia { get; init; }
    /// Só as URLs que mudam; substituem a do leiaute com o mesmo autorizador, ambiente e serviço, ou se acrescentam.
    public IReadOnlyList<EnderecoServico> Urls { get; init; } = [];
    /// A parte que é do documento, do tipo que o leiaute declara (ConfiguracaoNFCe, com o CSC).
    public ConfiguracaoDocumento? Documento { get; init; }
    /// Os callbacks de validação de quem usa (6.3).
    public ValidacaoCallback? Validacao { get; init; }
    /// O relógio: rastro, tempos e o instante que o leiaute usar (2.3).
    public TimeProvider Relogio { get; init; } = TimeProvider.System;
}

/// A base da configuração que é de um documento. O leiaute deriva (ConfiguracaoNFCe) e declara o tipo.
public abstract class ConfiguracaoDocumento
{
    protected ConfiguracaoDocumento();
}

/// A entrada em contingência: dhCont e xJust de toda nota emitida fora do tipo Normal.
public sealed class Contingencia
{
    /// ArgumentException: justificativa vazia (o tamanho, 15 a 256, é do schema).
    public Contingencia(DateTimeOffset inicio, string justificativa);
    public DateTimeOffset Inicio { get; }
    public string Justificativa { get; }
}
```

### 2.3. `ConexoesSefaz`

```csharp
/// O pool de HttpMessageHandler, um por certificado (8.5), com as ACs raiz da ICP-Brasil (CustomRootTrust).
/// Um por processo (ou por contêiner de DI), compartilhado por todos os núcleos; de quem cria, que o descarta
/// depois do último núcleo. Seguro para uso concorrente.
public sealed class ConexoesSefaz : IDisposable
{
    public ConexoesSefaz();
    public void Dispose();
}
```

A fábrica de handler substituível (2.3), o tempo limite e o descarte dos handlers sem uso entram na 4.1, como sobrecarga do construtor e propriedades `init` (versão menor).

### 2.4. `ILeiaute` e a declaração

```csharp
/// Um documento numa versão do leiaute (3.1). Sem estado e seguro para uso concorrente: a mesma instância
/// serve a vários núcleos.
public interface ILeiaute
{
    /// A versão do leiaute, como no atributo versao do XML ("4.00").
    string Versao { get; }
    /// Os documentos que o leiaute atende ([NFe, NFCe]).
    IReadOnlyList<Documento> Documentos { get; }
    /// Declara tudo o que o leiaute tem para o documento de declaracao.Documento. Chamado pelo Criar
    /// (e pelo DeclaracaoLeiaute.Criar); a mesma entrada dá sempre a mesma declaração.
    void Declarar(DeclaracaoLeiaute declaracao);
}

/// O que um leiaute declara para um documento. Os métodos Registrar/Declarar só valem durante o
/// ILeiaute.Declarar (depois, InvalidOperationException); as propriedades são a listagem. Construtor interno.
public sealed class DeclaracaoLeiaute
{
    /// A declaração de um leiaute para um documento, sem criar núcleo (a tela de configuração, antes do certificado).
    /// ArgumentException: o leiaute não atende o documento.
    public static DeclaracaoLeiaute Criar(ILeiaute leiaute, Documento documento);

    public Documento Documento { get; }

    /// ArgumentException: já há handler para TParametro.
    public void RegistrarOperacao<TParametro, TRetorno>(IOperacao<TParametro, TRetorno> operacao)
        where TParametro : ParametroWS<TRetorno>
        where TRetorno : class;

    /// A UF, no tipo de emissão, é atendida pelo autorizador (AC, Normal → SVRS; MG, ContingenciaSvcAN → SvcAN).
    public void DeclararAtendimento(UF uf, TipoEmissao emissao, Destino autorizador);

    /// A URL do serviço ("NFeStatusServico4") no autorizador e no ambiente.
    public void DeclararEndereco(EnderecoServico endereco);

    /// O tipo da configuração do documento; obrigatoria: o Criar recusa a configuração sem ela.
    public void DeclararConfiguracaoDocumento<TConfiguracao>(bool obrigatoria)
        where TConfiguracao : ConfiguracaoDocumento;

    public IReadOnlyList<OperacaoSuportada> Operacoes { get; }
    public IReadOnlyList<Atendimento> Atendimentos { get; }
    public IReadOnlyList<EnderecoServico> Enderecos { get; }
    public Type? TipoConfiguracaoDocumento { get; }
    public bool ConfiguracaoDocumentoObrigatoria { get; }
}

/// Uma operação registrada. Construtor interno.
public sealed class OperacaoSuportada
{
    public Type Parametro { get; }   // typeof(ConsultaStatus)
    public Type Retorno { get; }     // typeof(RetornoStatus)
}

/// UF × tipo de emissão → autorizador. Construtor interno.
public sealed class Atendimento
{
    public UF UF { get; }
    public TipoEmissao Emissao { get; }
    public Destino Autorizador { get; }
}

/// Autorizador × ambiente × serviço → URL. Construtor público: o leiaute declara, quem usa troca.
public sealed class EnderecoServico
{
    /// ArgumentException: serviço vazio; URL que não é absoluta e HTTPS.
    public EnderecoServico(Destino autorizador, Ambiente ambiente, string servico, Uri url);
    public Destino Autorizador { get; }
    public Ambiente Ambiente { get; }
    public string Servico { get; }
    public Uri Url { get; }
}
```

### 2.5. `Destino`

```csharp
/// Para onde vai uma operação: uma UF (resolvida pelo Atendimento) ou um autorizador.
/// As UFs têm o código IBGE (o mesmo valor de UF); o AN, o 91 do cOrgao; os virtuais não têm código oficial,
/// e o valor deles nunca é escrito no XML.
public enum Destino
{
    RO = 11, AC = 12, /* ... as 27, com os valores de UF ... */ DF = 53,
    AN = 91,
    SVRS = 101, SVAN = 102, SvcAN = 103, SvcRS = 104,
}

public static class Destinos
{
    public static Destino ComoDestino(this UF uf);
    /// A UF do destino, se for uma UF.
    public static bool TentarUF(this Destino destino, out UF uf);
    /// "MG", "AN", "SVRS", "SVAN", "SVC-AN", "SVC-RS".
    public static string Sigla(this Destino destino);
    public static bool TentarLerSigla(string? sigla, out Destino destino);
}
```

### 2.6. `ParametroWS`

```csharp
/// A base de todo parâmetro de operação. Só ParametroWS<TRetorno> deriva dela (construtor private protected);
/// existe para o Suporta<T>() ter uma restrição.
public abstract class ParametroWS
{
    private protected ParametroWS();
    /// null = o destino natural da operação (8.3, 8.4). Sobrepõe o destino; o tipo de emissão continua o do pedido.
    public Destino? Destino { get; init; }
}

/// O parâmetro de uma operação que devolve TRetorno. Os leiautes derivam classes seladas (ConsultaStatus).
public abstract class ParametroWS<TRetorno> : ParametroWS where TRetorno : class
{
    protected ParametroWS();
}
```

### 2.7. `IOperacao` e o que ele recebe e devolve

```csharp
/// O handler de uma operação, no leiaute. Sem estado e seguro para uso concorrente. Não faz I/O: o núcleo
/// assina, valida pelo schema, escolhe a URL e transmite (8.2).
public interface IOperacao<TParametro, TRetorno>
    where TParametro : ParametroWS<TRetorno>
    where TRetorno : class
{
    /// Monta a mensagem (o conteúdo do nfeDadosMsg), sem assinar. Exceção aqui é erro de programação e sobe como veio.
    PedidoWS Montar(TParametro parametro, ContextoOperacao contexto);

    /// Lê o retorno (o conteúdo do nfeResultMsg). XML que não se lê: lançar XmlException ou FormatException;
    /// o núcleo a embrulha numa FalhaTransmissaoException com o rastro.
    LeituraWS<TRetorno> Ler(TParametro parametro, string xmlRetorno, ContextoOperacao contexto);
}

/// O que o handler enxerga do núcleo. Construtor interno; ganha membros nas fases 3 e 5 (o motor de regras,
/// os callbacks) sem quebrar ninguém.
public sealed class ContextoOperacao
{
    public Documento Documento { get; }
    public ConfiguracaoDFe Configuracao { get; }
}

/// O pedido montado. Construtor público: o leiaute cria.
public sealed class PedidoWS
{
    /// servico: o nome na tabela de URLs ("NFeStatusServico4"). xml: a mensagem; com problema de severidade
    /// Erro em Problemas, pode estar incompleta ou vazia. schema: o da mensagem (1.5), validado depois de assinar.
    public PedidoWS(string servico, string xml, SchemaXml schema);
    public string Servico { get; }
    public string Xml { get; }
    public SchemaXml Schema { get; }

    /// Os Id dos elementos que o núcleo assina, na ordem (uma NFe por nota do lote, um infEvento por evento);
    /// vazio quando a mensagem não é assinada ou já vem assinada (o XML preparado, 4.4).
    public IReadOnlyList<string> IdsAssinatura { get; init; } = [];
    /// O destino e o tipo de emissão que saem do documento ou da chave (8.4); null = os do núcleo.
    public Destino? Destino { get; init; }
    public TipoEmissao? Emissao { get; init; }
    /// Problemas da montagem (regras do leiaute, campo não suportado, 4.4). Com Erro, nada é assinado nem enviado.
    public IReadOnlyList<Problema> Problemas { get; init; } = [];
}

/// O retorno lido. Construtor público: o leiaute cria.
public sealed class LeituraWS<TRetorno> where TRetorno : class
{
    /// cStat e motivo: os da mensagem (no lote, o do lote; o de cada nota fica no retorno).
    public LeituraWS(TRetorno retorno, int cStat, string motivo);
    public TRetorno Retorno { get; }
    public int CStat { get; }
    public string Motivo { get; }
    public IReadOnlyList<Problema> Problemas { get; init; } = [];
}
```

### 2.8. `ResultadoWS` e o rastro

```csharp
public enum SituacaoResultado
{
    /// A SEFAZ respondeu, inclusive com rejeição: o cStat diz qual (2.4).
    Processado = 1,
    /// Problema de severidade Erro antes de transmitir (montagem ou schema): nada foi enviado.
    RecusadoLocalmente = 2,
}

/// O resultado de uma operação. Construtor interno.
public sealed class ResultadoWS<TRetorno> where TRetorno : class
{
    public SituacaoResultado Situacao { get; }
    /// Situacao == Processado; com ele, Retorno, CStat e Motivo não são nulos.
    [MemberNotNullWhen(true, nameof(Retorno), nameof(Motivo))]
    public bool Processado { get; }
    public int? CStat { get; }
    public string? Motivo { get; }
    public TRetorno? Retorno { get; }
    /// Todos: os da montagem, os do schema e os da leitura; os avisos também no Processado.
    public IReadOnlyList<Problema> Problemas { get; }
    public RastroWS Rastro { get; }
}

/// O que foi enviado e recebido, para quem quiser guardar (8.7). Construtor interno.
/// Os XMLs têm dados pessoais: guardar com o cuidado do documento, nunca em log.
public sealed class RastroWS
{
    /// A mensagem como foi (ou seria) enviada, assinada; null se a montagem não a produziu.
    public string? XmlPedido { get; }
    /// A resposta (o conteúdo do nfeResultMsg); null se não houve resposta.
    public string? XmlRetorno { get; }
    public Destino? Autorizador { get; }
    public Uri? Url { get; }
    /// Pelo Relogio da configuração.
    public DateTimeOffset Inicio { get; }
    /// Do início do ExecutarAsync ao resultado (ou à falha).
    public TimeSpan Duracao { get; }
    /// Só a ida e volta HTTPS; null se não chegou a transmitir.
    public TimeSpan? DuracaoTransmissao { get; }
}
```

### 2.9. `FalhaTransmissaoException`

```csharp
/// Falha de infraestrutura depois de montado e validado o pedido: rede, TLS, tempo esgotado, HTTP diferente de
/// 200, falha SOAP, resposta ilegível (2.4). A causa fica em InnerException.
public sealed class FalhaTransmissaoException : Exception
{
    public RastroWS Rastro { get; }
    /// O pedido saiu inteiro e a resposta não veio (ou não se leu): a SEFAZ pode ter processado. Na autorização,
    /// é o caso da RecuperacaoAutorizacao (8.6). false: com certeza não chegou (DNS, conexão recusada, TLS).
    public bool PodeTerSidoProcessado { get; }
}
```

### 2.10. Callbacks de validação (6.3)

```csharp
/// As regras de negócio de quem usa, na configuração. Cada callback devolve os problemas (vazio = nada).
/// Podem ser chamados em paralelo (núcleo compartilhado). Exceção num callback sobe como veio (erro de programação).
public sealed class ValidacaoCallback
{
    /// Chamado por campo escrito.
    public Func<CampoVerificado, IEnumerable<Problema>>? Campo { get; init; }
    /// Chamado por documento inteiro (cada nota do lote).
    public Func<DocumentoVerificado, IEnumerable<Problema>>? Documento { get; init; }
}

/// Um campo, como foi escrito no XML. Construtor interno.
public sealed class CampoVerificado
{
    public Documento Documento { get; }
    public string Tag { get; }               // "xProd"
    public string Caminho { get; }           // "NFe/infNFe/det[3]/prod/xProd", como o do Problema (1.5)
    public string? Propriedade { get; }      // "Itens[2].Produto.Descricao", quando o leiaute a conhece (2.6)
    public string Texto { get; }             // o valor como está no XML
}

/// Um documento inteiro. Construtor interno.
public sealed class DocumentoVerificado
{
    public Documento Documento { get; }
    /// O modelo do leiaute (no OpenDFe.NFe, a nota); null quando o documento veio só como XML.
    public object? Modelo { get; }
    public string Xml { get; }
    public ChaveAcesso? Chave { get; }
}
```

Quem chama os callbacks e em que ponto do caminho (depois do schema e das regras do leiaute, 6) é da 3.1: entra como membros novos de `ContextoOperacao` e `PedidoWS`, classes seladas do núcleo, sem mudar `IOperacao`.

## 3. O caminho de `ExecutarAsync`

1. Acha o handler pelo **tipo exato** do parâmetro; sem ele, `NotSupportedException`.
2. `Montar` → `PedidoWS`. Problema `Erro` em `Problemas`: `RecusadoLocalmente`, com o `XmlPedido` no rastro.
3. Assina cada `Id` de `IdsAssinatura` com o certificado (`AssinaturaDigital.Assinar`, 1.4).
4. Valida o XML assinado pelo `Schema` (`SchemaXml.Validar`, 1.5). `Erro`: `RecusadoLocalmente`.
5. Escolhe a URL: destino = `parametro.Destino` ?? `pedido.Destino` ?? a UF do núcleo; tipo de emissão = `pedido.Emissao` ?? o do núcleo; se o destino é uma UF, o autorizador sai do `Atendimento`; a URL, do autorizador × ambiente × serviço, nas efetivas. Sem URL: `NotSupportedException`.
6. SOAP 1.2 pelas `Conexoes` (4.1). Falha: `FalhaTransmissaoException`.
7. `Ler` → `LeituraWS`. Exceção da leitura: `FalhaTransmissaoException` com `PodeTerSidoProcessado`.
8. `Processado`, com os problemas de 2, 4 e 7 (os avisos) e o rastro.

Na 1.6 os passos 1 a 5, 7 e 8 são implementados; o 6 é uma costura interna do `ConexoesSefaz` (o teste injeta um transporte falso) até a 4.1.

## 4. Exemplo de uso

```csharp
using var conexoes = new ConexoesSefaz();                         // um por processo
using var certificado = CertificadoDigital.AbrirArquivoPfx(caminho, senha);

var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente    = Ambiente.Homologacao,
    UF          = UF.MG,
    Certificado = certificado,
    Conexoes    = conexoes,
    Documento   = new ConfiguracaoNFCe { IdCsc = "1", Csc = csc },
    Urls        = [new EnderecoServico(Destino.MG, Ambiente.Homologacao, "NFeStatusServico4", urlNova)],
    Validacao   = new ValidacaoCallback
    {
        Campo = c => c.Tag == "xProd" && c.Texto.Contains("TESTE")
            ? [new Problema("PDV-01", Severidade.Erro, "Produto de teste na nota.") { Caminho = c.Caminho }]
            : [],
    },
});

if (dfe.Suporta<ConsultaStatus>())
{
    var r = await dfe.ExecutarAsync(new ConsultaStatus(), ct);     // ResultadoWS<RetornoStatus>
    if (r.Processado)
        Console.WriteLine($"{r.CStat} {r.Motivo}");                     // 107 Servico em Operacao
    else
        foreach (var p in r.Problemas) Console.WriteLine(p);
    Console.WriteLine($"{r.Rastro.Url} em {r.Rastro.Duracao}");
}

foreach (var a in dfe.Atendimentos) Console.WriteLine($"{a.UF} {a.Emissao} → {a.Autorizador.Sigla()}");
foreach (var e in dfe.Enderecos)    Console.WriteLine($"{e.Autorizador.Sigla()} {e.Ambiente} {e.Servico} {e.Url}");

try { var a = await dfe.ExecutarAsync(new Autorizacao([nota]) { Sincrona = true }, ct); }
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    // guardar ex.Rastro.XmlPedido; depois, RecuperacaoAutorizacao (8.6)
}
```

## 5. Exemplo de leiaute (fase 2)

```csharp
public sealed class LeiauteNFe400 : ILeiaute
{
    public string Versao => "4.00";
    public IReadOnlyList<Documento> Documentos { get; } = [Documento.NFe, Documento.NFCe];

    public void Declarar(DeclaracaoLeiaute d)
    {
        d.RegistrarOperacao(new OperacaoStatus());
        d.RegistrarOperacao(new OperacaoAutorizacao());
        if (d.Documento == Documento.NFCe)
            d.DeclararConfiguracaoDocumento<ConfiguracaoNFCe>(obrigatoria: true);

        foreach (var a in Tabelas.Atendimentos(d.Documento))     // tabela embutida, conferida na 4.2
            d.DeclararAtendimento(a.UF, a.Emissao, a.Autorizador);
        foreach (var e in Tabelas.Enderecos(d.Documento))
            d.DeclararEndereco(e);
    }
}

internal sealed class OperacaoStatus : IOperacao<ConsultaStatus, RetornoStatus>
{
    private static readonly Lazy<SchemaXml> Schema = new(() => SchemaXml.Carregar(
        typeof(OperacaoStatus).Assembly, "Schemas/PL_009/consStatServ_v4.00.xsd", "215"));

    public PedidoWS Montar(ConsultaStatus p, ContextoOperacao c) =>
        new("NFeStatusServico4", EscritorStatus.Escrever(c.Configuracao.Ambiente, p.Destino, c.Configuracao.UF),
            Schema.Value);

    public LeituraWS<RetornoStatus> Ler(ConsultaStatus p, string xml, ContextoOperacao c)
    {
        var r = LeitorStatus.Ler(xml);
        return new(r, r.CStat, r.Motivo);
    }
}
// Na autorização: IdsAssinatura = ["NFe" + chave, ...]; Destino e Emissao saem da chave na consulta e nos eventos.
```

## 6. Como as decisões das etapas 1.1 a 1.5 entram

- **Problemas como `IReadOnlyList<Problema>`** em todo lugar (pedido, leitura, resultado, callbacks devolvendo `IEnumerable<Problema>`), com o `Codigo` texto de quem cria (1.1) e o `Schema` do 1.5.
- **Exceção só para infraestrutura e erro de programação** (2.4): rejeição é `Processado` com `cStat`; recusa local é `RecusadoLocalmente`; rede, TLS, SOAP e resposta ilegível são `FalhaTransmissaoException`; parâmetro errado é `ArgumentException`; operação não registrada, `NotSupportedException`.
- **`SchemaXml` por mensagem, com o código** (1.5): cada `PedidoWS` traz o seu; o leiaute o guarda (estático e preguiçoso).
- **Assinatura pelo `Id`** (1.4): o pedido diz os `Id`; o núcleo assina na ordem, no texto, preservando os bytes do que já veio assinado.
- **Certificado `X509Certificate2` de quem chama** (1.3): a configuração o referencia; o núcleo não o descarta. O `Criar` usa o `ExigirChavePrivadaRsa` interno.
- **Enumerações com o valor do XML** (1.1): `Destino` segue o padrão, com o valor da UF e o 91 do AN.
- **Classes seladas, sem `record`, construtor interno onde só o núcleo cria** (1.2, 1.5).

## 7. Concorrência, ciclo de vida, cancelamento e extensibilidade

- **Imutável e concorrente:** `DFe`, `DeclaracaoLeiaute` (depois do `Declarar`), `ResultadoWS` e os tipos da listagem são imutáveis. Um núcleo atende chamadas em paralelo. `ILeiaute`, `IOperacao` e os callbacks precisam ser seguros para uso concorrente (documentado).
- **Posse:** o certificado e o `ConexoesSefaz` são de quem os cria; o núcleo só os referencia e **não é `IDisposable`**. Descartar o certificado ou as conexões com um núcleo em uso é erro de quem chama (`ObjectDisposedException` ou `CryptographicException` na chamada seguinte). O `HttpMessageHandler` é do `ConexoesSefaz`.
- **Custo:** criar um núcleo é barato (o `Declarar` monta dicionários; os schemas são do leiaute, carregados uma vez). Trocar de contingência é criar outro núcleo com o mesmo `ConexoesSefaz`.
- **Cancelamento:** o `CancellationToken` vai em cada `ExecutarAsync`, último parâmetro, opcional. `Montar` e `Ler` são síncronos e sem I/O, sem token. O cancelamento sai como `OperationCanceledException`; cancelar depois de o pedido sair é, para a SEFAZ, a mesma queda no meio (8.6), e quem cancela não recebe o rastro.
- **Relógio:** `ConfiguracaoDFe.Relogio`; nada lê `DateTimeOffset.Now`.
- **Extensibilidade sem versão maior:**
  - selados: todos os tipos concretos do núcleo; membros novos neles são versão menor;
  - abertos para o leiaute: as interfaces `ILeiaute` e `IOperacao` (mínimas: o que cresce, cresce na `DeclaracaoLeiaute`, no `ContextoOperacao` e no `PedidoWS`) e as bases abstratas `ParametroWS<TRetorno>` e `ConfiguracaoDocumento`;
  - construtor interno: `DeclaracaoLeiaute`, `ContextoOperacao`, `ResultadoWS`, `RastroWS`, `OperacaoSuportada`, `Atendimento`, `CampoVerificado`, `DocumentoVerificado`, `FalhaTransmissaoException`;
  - construtor público: o que o leiaute ou quem usa cria (`PedidoWS`, `LeituraWS`, `EnderecoServico`, `ConfiguracaoDFe`, `Contingencia`, `ValidacaoCallback`, `ConexoesSefaz`);
  - `required` só nos quatro membros da configuração que todo núcleo precisa: acrescentar outro `required` depois seria versão maior.

## 8. Decisões (aprovadas em 2026-10-19)

1. **`ILeiaute`: forma.** (a) interface com um membro por coisa declarada (operações, URLs, schemas, detector, impressão); (b) classe abstrata `Leiaute` com membros virtuais; (c) **interface mínima (`Versao`, `Documentos`, `Declarar`) e uma `DeclaracaoLeiaute` selada do núcleo**. Recomendo (c): o que vier (detector na 2.5, preparo na 4.4, cifras na 4.1, impressão na 7.2) é método novo na declaração, versão menor, sem quebrar leiaute de terceiro; mantém o nome `ILeiaute` do `PROJETO.md`.
2. **`IOperacao`: quem conduz o caminho.** (a) **interface com `Montar` e `Ler` síncronos, o núcleo dono do caminho** (assina, valida, transmite); (b) classe abstrata com os mesmos métodos; (c) o handler conduz tudo (`ExecutarAsync` próprio, com serviços do contexto). Recomendo (a): o schema e a assinatura não ficam a critério do handler (6.1, "não é opcional"), o leiaute não faz I/O nem precisa de token, e o que crescer vai no `ContextoOperacao` e no `PedidoWS`.
3. **Registro das operações.** (a) **instância pela declaração** (`d.RegistrarOperacao(new OperacaoStatus())`, tipos deduzidos); (b) tipo genérico (`RegistrarOperacao<OperacaoStatus>()`, com `new()`); (c) dicionário `Type → object` no leiaute. Recomendo (a): tipado, sem reflexão (AOT e trimming, que o PDV usa), e o handler pode receber o que precisa no construtor.
4. **Despacho pelo tipo do parâmetro.** (a) **tipo exato** (os parâmetros são selados); (b) o mais próximo na hierarquia. Recomendo (a): sem ambiguidade; o detalhe do evento é subclasse do detalhe (8.3), não do parâmetro.
5. **`Operacoes` e `Suporta<T>()`.** (a) **propriedade `IReadOnlyList<OperacaoSuportada>`** e `Suporta<TParametro>() where TParametro : ParametroWS`; (b) método `ListarOperacoes()`; (c) acesso tipado (`dfe.Operacoes.Status.ExecutarAsync()`). Recomendo (a): (c) obrigaria o núcleo a conhecer as operações da NF-e; o açúcar tipado, se fizer falta, vem como métodos de extensão no pacote do leiaute (`dfe.ConsultarStatusAsync()`), sem mexer no núcleo. A base não genérica `ParametroWS` (construtor `private protected`) é acréscimo ao 8.1 do `PROJETO.md`, só para a restrição.
6. **`CancellationToken` e `TimeProvider`.** Token: (a) **em cada `ExecutarAsync`**; (b) na configuração. Relógio: (a) **na `ConfiguracaoDFe`, padrão `TimeProvider.System`**; (b) parâmetro do `Criar`; (c) por chamada. Recomendo os dois (a): o token é da chamada (padrão do .NET, CA2016); o relógio é dependência do núcleo (2.3) e a configuração já é o "construtor" dele.
7. **Conexões (o pool de handlers, 8.5).** (a) **objeto explícito `ConexoesSefaz`, `IDisposable`, de quem usa, obrigatório na configuração**; (b) pool estático interno, sem nada na configuração; (c) núcleo `IDisposable` dono do handler. Recomendo (a): (b) é estado global (2.3) e, no ERP com muitos certificados, cresce sem dono para descartar; (c) impede o compartilhamento entre núcleos que o 8.5 pede. **Diverge do exemplo do 3.2** (que não tem `Conexoes`) e do "pool do núcleo" do 8.5 (o pool continua do pacote do núcleo, mas a instância é de quem usa). Precisa ser decidido agora: tornar obrigatório depois é versão maior.
8. **Configuração do documento sem o núcleo conhecer a NFC-e.** (a) **base abstrata `ConfiguracaoDocumento`, o leiaute declara o tipo e se é obrigatória, o `Criar` confere**; (b) `object?`, conferido pelo leiaute; (c) genérico (`ConfiguracaoDFe<T>`, `DFe<T>`). Recomendo (a): tipado no uso (`Documento = new ConfiguracaoNFCe {...}`, como o 3.2), erro cedo no `Criar`, e (c) espalharia o genérico por todos os tipos.
9. **Callbacks de regras.** Forma: (a) **dois delegados (`Func<CampoVerificado, IEnumerable<Problema>>` e `Func<DocumentoVerificado, IEnumerable<Problema>>`) numa `ValidacaoCallback` selada, na configuração**; (b) interface `IValidacaoCallback` com dois métodos; (c) classe abstrata com dois virtuais vazios. Recomendo (a): lambda sem classe para quem usa; um callback novo (por evento, por item) é propriedade nova, sem quebrar. O modelo vai como `object?` (o núcleo não conhece a nota; quem usa faz o *cast*) e o campo como texto do XML, com a tag, o caminho e a propriedade. Registrados na configuração (o 6.3 diz "no núcleo", e a configuração é dele), não por chamada.
10. **Como o `ResultadoWS` representa os quatro casos.** Sucesso e rejeição: `Processado` com `CStat` (2.4). Recusa local: `RecusadoLocalmente`, com os problemas e o XML montado no rastro. Infraestrutura: (a) **`FalhaTransmissaoException` própria, com o `Rastro` e `PodeTerSidoProcessado`**; (b) terceira situação no resultado (`FalhaInfraestrutura`), sem exceção; (c) as exceções do .NET como vêm (`HttpRequestException`). Recomendo (a): segue o 2.4, e a queda no meio (8.6) precisa saber se o pedido saiu e qual XML foi, o que (c) não dá. `Processado` também como `bool` com `MemberNotNullWhen`, para o `Retorno` sem `!`.
11. **Forma do rastro.** (a) **objeto `RastroWS`** (`XmlPedido`, `XmlRetorno`, `Autorizador`, `Url`, `Inicio`, `Duracao`, `DuracaoTransmissao`), o mesmo no resultado e na exceção; (b) propriedades soltas no `ResultadoWS`, como no 8.1. Conteúdo: (a) **a mensagem (o conteúdo do `nfeDadosMsg` e do `nfeResultMsg`)**; (b) o envelope SOAP inteiro. Recomendo os dois (a): a exceção precisa do rastro, e a mensagem é o que se guarda e se reenvia; o envelope não tem nada que quem usa precise. **Diverge do 8.1** (as propriedades `XmlEnviado`, `XmlRetorno`, `Url`, `Duracao` passam para o `Rastro`).
12. **Ordem do caminho: assinar antes do schema.** (a) **montar → recusa local da montagem → assinar → schema → SOAP**; (b) validar antes de assinar, como no 8.2 e no `PLANO.md` 4.3, descartando o "falta `Signature`". Recomendo (a): o XSD da NF-e exige a `Signature` (o `TNFe` a declara obrigatória), e descartar um problema pelo nome é frágil; assinar antes custa uma assinatura RSA, nada é enviado com erro do mesmo jeito. **Diverge do 8.2 e do `PLANO.md` 4.3** na ordem. A ordem do 6 (schema, regras do leiaute, callbacks) fica para a 3.1.
13. **`Destino`: enum ou classe.** (a) **enum com o valor da UF, o 91 do AN e valores sem significado oficial para os virtuais**, mais a classe estática `Destinos`; (b) classe selada com instâncias estáticas (`Destino.Da(UF.MG)`, `Destino.SVRS`). Recomendo (a): coerente com `UF` e `TipoEmissao` (1.1), `switch` completo, `Destino?` sem alocação.
14. **Tabela de destinos em dois níveis.** (a) **UF × tipo de emissão → autorizador (`Atendimento`) e autorizador × ambiente × serviço → URL (`EnderecoServico`); quem usa troca só as URLs**; (b) uma tabela só, UF × tipo de emissão × ambiente × serviço → URL; (c) dois níveis com o atendimento também trocável. Recomendo (a): é como os autorizadores funcionam (a URL do SVRS muda num lugar só), e a UF que migra de autorizador se resolve trocando as URLs dela ou pelo `Destino` do parâmetro; trocar o atendimento entra depois como propriedade nova, se fizer falta.
15. **Contingência na configuração agora.** (a) **`Contingencia` (início e justificativa) na 1.6, obrigatória com `Emissao` diferente de `Normal`**; (b) só na 5.2. Recomendo (a): o `dhCont` e o `xJust` são de toda nota em contingência (SVC na fase 4, offline na 5), e passar a exigir no `Criar` depois quebraria quem cria núcleo em contingência sem ela.
16. **Listagem sem núcleo.** (a) **`DeclaracaoLeiaute.Criar(leiaute, documento)`, além das listas do núcleo**; (b) só pelo núcleo. Recomendo (a): a tela de configuração (3.2) lista antes de haver certificado; o mesmo objeto serve ao `Criar`.
17. **O `Criar` não confere a validade do certificado.** (a) **não confere** (quem usa chama `CertificadoDigital.Validade`; o PDV mostra "vencido em"); (b) recusa o vencido. Recomendo (a): conferir num instante fixo erraria no núcleo de vida longa (o certificado vence com o núcleo criado), e a SEFAZ rejeita (291). Conferir depois seria mudança de comportamento.

### Divergências com o `PROJETO.md`

- 3.2 (exemplo do `Criar`): a configuração ganha `Conexoes` (obrigatória) e `Contingencia` (decisões 7 e 15).
- 8.1: o rastro sai das propriedades soltas do `ResultadoWS` para `RastroWS` (11); `ResultadoWS` ganha `Processado` (10); `ParametroWS<T>` ganha a base não genérica `ParametroWS` (5).
- 8.2 e `PLANO.md` 4.3: assinar antes de validar pelo schema (12).
- 8.5: o pool é uma instância de quem usa, não do núcleo (7).

## 9. Fora desta etapa

Só declarado, ou nem isso, e vem depois:

- **SOAP real, TLS ICP-Brasil, `CipherSuitesPolicy` por destino, fábrica do handler, tempo limite** (4.1): na 1.6, a transmissão é uma costura interna do `ConexoesSefaz`, usada pelos testes. A ação SOAP e o namespace do WSDL por serviço entram como membros novos do `PedidoWS` ou da declaração.
- **As URLs e os atendimentos reais** (4.2), conferidos nos portais.
- **Os parâmetros e retornos** (`ConsultaStatus`, `Autorizacao`, ..., 4.3 a 8.2), no pacote do leiaute; se o status vai para o núcleo como "operação comum" (3.4), decide-se na 4.3.
- **`Preparar` e `DocumentoPreparado`** (4.4, 5.2), o **detector** `DFe.Detectar` (2.5), a **impressão** (7.2): membros novos no `DFe` e na `DeclaracaoLeiaute`.
- **O motor de regras e a chamada dos callbacks** (3.1).
- **A troca dos schemas por quem usa** (2.6 do `PROJETO.md`): sobrecarga do `SchemaXml` e propriedade da configuração.
- Fora por especulativo: interface `INucleo` para *mock* (quem usa testa com um leiaute falso e um transporte falso); carregar leiaute por DLL (3.2); exceção própria de certificado (1.3).

## 10. Testes da 1.6

Um leiaute falso no `Nucleo.Testes` (uma operação de eco com um XSD de exemplo da 1.5, um atendimento e uma URL): o `Criar` com cada recusa da seção 2.1; `Suporta`, `Operacoes`, `Atendimentos` e `Enderecos` (com a URL trocada por cima e a acrescentada); `DeclaracaoLeiaute.Criar` e a recusa do registro depois do `Declarar`; a escolha da URL (parâmetro, pedido, núcleo, atendimento); `RecusadoLocalmente` pela montagem e pelo schema, sem transmitir; a assinatura dos `Id` do pedido; `Processado` e `FalhaTransmissaoException` pelo transporte falso, com o rastro e um relógio falso; o cancelamento; chamadas em paralelo.

## 11. Mudanças do usuário (2026-10-19, na aprovação)

- **`RegrasDeQuemUsa` passa a ser `ValidacaoCallback`**, com os delegados `Campo` e `Documento` (antes `PorCampo` e `PorDocumento`) e a propriedade `ConfiguracaoDFe.Validacao` (antes `Regras`).
- **Regra de nomes, para sempre no repositório: identificadores sem preposição nem contração** (`De`, `Da`, `Do`, `Das`, `Dos`, `Para`, `Por`, `Pelo`, `Pela`, `Com`, `Sem`, `No`, `Na`, `Em`, `Ao` e afins como palavra de ligação: nunca `RegrasDaNFe`, e sim `RegraNFe`). Vale para tipos, membros, parâmetros, variáveis, campos, constantes, alvos e propriedades MSBuild e nomes de arquivo, em `src/`, `tests/` (o código auxiliar) e `eng/`; não vale para os nomes dos métodos de teste (frases) nem para o texto em português. No desenho: `DeclaracaoLeiaute.De` → `DeclaracaoLeiaute.Criar`, `PedidoWS.IdsParaAssinar` → `IdsAssinatura`, `Destinos.TentarPelaSigla` → `TentarLerSigla`. As renomeações do código das etapas anteriores estão no `PLANO.md` (Situação, 1.6). `Destinos.ComoDestino` fica: "como" não é preposição.

## 12. Desvios da implementação

O mínimo, sem mudar a forma dos tipos:

- **A transmissão (passo 6)** é a interface interna `ITransporteSefaz`, com o envio (`EnvioSefaz`: certificado, autorizador, URL, serviço, mensagem), recebida por um construtor interno do `ConexoesSefaz`. O transporte marca o envio quando a mensagem saiu inteira, e é essa marca que decide o `PodeTerSidoProcessado`. O construtor público usa um transporte pendente, que falha toda transmissão (`FalhaTransmissaoException` com `PodeTerSidoProcessado` falso e uma `NotSupportedException` dentro) até a 4.1.
- **Recusas a mais no `Criar`:** a `Contingencia` com o tipo de emissão `Normal` (incoerente; aceitar depois é versão menor), a URL trocada nula ou repetida (mesmo autorizador, ambiente e serviço), os membros obrigatórios nulos e os valores de enumeração desconhecidos (`ArgumentException`), e as conexões já descartadas (`ObjectDisposedException`).
- **Recusas a mais na declaração:** o parâmetro abstrato no `RegistrarOperacao` (o despacho é pelo tipo exato; nunca seria achado), o atendimento repetido para a mesma UF e tipo de emissão, a URL repetida e a configuração do documento declarada duas vezes. As exceções do `ILeiaute.Declarar` sobem do `Criar` como vieram.
- **Cancelamento e tempo:** o token é conferido no início (antes do `Montar`) e antes de transmitir. A `OperationCanceledException` que não é do token de quem chama (o tempo esgotado do `HttpClient`) é falha de transmissão.
- **Handler e transporte que devolvem nulo:** o pedido ou a leitura nulos são `InvalidOperationException` (erro de programação do leiaute); a resposta nula do transporte é falha de transmissão.
- **Analisadores:** a `FalhaTransmissaoException` fica sem os construtores padrão de exceção, como no desenho: o CA1032 não está ligado no nível `latest-recommended` do repositório (conferido: só no `latest-all`) e o CA1710 é atendido pelo sufixo. O construtor protegido de `ParametroWS<TRetorno>` dispara o RS0022 (torna herdável a base com construtor `private protected`), que é o desenho; suprimido ali, com o motivo.
