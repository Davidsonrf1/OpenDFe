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
| `DocumentoPedido` | um documento dentro da mensagem, para as regras do leiaute e os callbacks (3.1, `docs/REGRAS.md`) |

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

Vêm depois, como membros novos (versão menor): `Detectar` (estático, 2.5; feito, desenho e implementação em `docs/LEITURA.md`, com a `FormaLeitura` declarada na `DeclaracaoLeiaute`), `Preparar` (4.4, feito: seção 14) e a impressão (7.2).

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
    public ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler> fabrica);   // 4.1
    public TimeSpan TempoConexao { get; init; }   // 4.1; 15 s
    public TimeSpan TempoResposta { get; init; }  // 4.1; 60 s
    public TimeSpan TempoOcioso { get; init; }    // 4.1; 30 min
    public void Dispose();
}

/// O que a fábrica recebe (4.1); construtor interno.
public sealed class ConexaoSefaz
{
    public X509Certificate2 Certificado { get; }              // a cópia do pool
    public IReadOnlyList<TlsCipherSuite>? Cifras { get; }     // null = o padrão da máquina
    public SocketsHttpHandler CriarPadrao();
}
```

A fábrica de handler substituível (2.3), os tempos e o descarte dos handlers sem uso entraram na 4.1, como sobrecarga do construtor e propriedades `init` (versão menor); o desenho está no `PROJETO.md` 8.5.

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
    public void DeclararServico(ServicoSoap servico);                                  // 4.1: namespace do WSDL e ação; 8.1: e a operação do Body (ServicoSoap.Operacao, 23.1)
    public void DeclararCifras(Destino autorizador, IEnumerable<TlsCipherSuite> cifras); // 4.1: só no Linux

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

    /// Lê o retorno (o conteúdo do nfeResultMsg), com a mensagem como foi enviada, assinada (4.4: o nfeProc). XML que
    /// não se lê: lançar XmlException ou FormatException; o núcleo a embrulha numa FalhaTransmissaoException com o rastro.
    LeituraWS<TRetorno> Ler(TParametro parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto);
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
    public TipoFalhaTransmissao Tipo { get; }   // 4.1: Conexao, Tls, CertificadoRecusado, TempoEsgotado, Http,
}                                               // FalhaSoap, RespostaIlegivel (PROJETO.md 8.5)
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

Quem chama os callbacks e em que ponto do caminho (depois do schema e das regras do leiaute, 6) ficou para a 3.1, sem mudar `IOperacao`: o `PedidoWS` ganhou os `Documentos` (`DocumentoPedido`), a `DeclaracaoLeiaute` o `DeclararRegras` e a `ConfiguracaoDFe` o `RegrasComoAviso` (`docs/REGRAS.md` 2 e 3).

## 3. O caminho de `ExecutarAsync`

1. Acha o handler pelo **tipo exato** do parâmetro; sem ele, `NotSupportedException`.
2. `Montar` → `PedidoWS`. Problema `Erro` em `Problemas`: `RecusadoLocalmente`, com o `XmlPedido` no rastro.
3. Assina cada `Id` de `IdsAssinatura` com o certificado (`AssinaturaDigital.Assinar`, 1.4).
4. Valida cada documento que declara o seu (`DocumentoPedido.Schema`, 4.4) sozinho e, sem erro neles, o XML assinado pelo `Schema` (`SchemaXml.Validar`, 1.5). `Erro`: `RecusadoLocalmente`.
5. As regras do leiaute (`DeclararRegras`) em cada um dos `Documentos` do pedido, com os códigos de `RegrasComoAviso` rebaixados a aviso (3.1, `docs/REGRAS.md` 3). `Erro`: `RecusadoLocalmente`.
6. Os callbacks de `Validacao` em cada documento: o `Campo` em cada campo, depois o `Documento` (3.1). `Erro`: `RecusadoLocalmente`.
7. Escolhe a URL: destino = `parametro.Destino` ?? `pedido.Destino` ?? a UF do núcleo; tipo de emissão = `pedido.Emissao` ?? o do núcleo; se o destino é uma UF, o autorizador sai do `Atendimento`; a URL, do autorizador × ambiente × serviço, nas efetivas. Sem URL: `NotSupportedException`.
8. SOAP 1.2 pelas `Conexoes` (4.1). Falha: `FalhaTransmissaoException`.
9. `Ler` → `LeituraWS`. Exceção da leitura: `FalhaTransmissaoException` com `PodeTerSidoProcessado`.
10. `Processado`, com os problemas de 2, 4, 5, 6 e 9 (os avisos) e o rastro.

Na 1.6 os passos 1 a 4, 7, 9 e 10 são implementados; o 8 é uma costura interna do `ConexoesSefaz` (o teste injeta um transporte falso) até a 4.1. Os passos 5 e 6 entram na 3.1, e a numeração antiga (de 5 a 8) passou a ser de 7 a 10.

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
    Documento   = new ConfiguracaoNFCe(new CodigoSeguranca(1, csc)),   // a versão 2 do QR Code; sem a configuração, a 3 (5.1)
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

try { var a = await dfe.ExecutarAsync(new Autorizacao(nota), ct); }
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    // Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido), guardar; depois, RecuperacaoAutorizacao (seção 15, docs/QUANDO_CAI.md)
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
        d.DeclararConfiguracaoDocumento<ConfiguracaoNFCe>(obrigatoria: false);   // a versão 3 do QR Code não precisa de nada (5.1)

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
- **Os parâmetros e retornos** (`ConsultaStatus`, `Autorizacao`, ..., 4.3 a 8.2), no pacote do leiaute; se o status vai para o núcleo como "operação comum" (3.4), decide-se na 4.3. **Decidido na 4.3: vai ao núcleo** (seção 13).
- **`Preparar` e `DocumentoPreparado`** (4.4, feitos: seção 14), o **detector** `DFe.Detectar` (2.5), a **impressão** (7.2): membros novos no `DFe` e na `DeclaracaoLeiaute`.
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

## 13. O status do serviço (etapa 4.3)

**Aprovado em 2026-10-22**, todas as decisões pela recomendação. A primeira operação de verdade: o caminho do `ExecutarAsync` (seção 3) não mudou.

```csharp
// núcleo, namespace OpenDFe: operação comum aos documentos (PROJETO.md 3.4)
public sealed class ConsultaStatus : ParametroWS<RetornoStatus>
{
    public ConsultaStatus();          // sem membros próprios; o Destino herdado sobrepõe
}

public sealed class RetornoStatus
{
    public RetornoStatus(int status, string motivo);       // o leiaute cria; ArgumentNullException no motivo nulo
    public int Status { get; }                             // cStat
    public string Motivo { get; }                          // xMotivo
    public bool Operando { get; }                          // Status == 107
    public Ambiente? Ambiente { get; init; }               // tpAmb
    public string? VersaoAplicativo { get; init; }         // verAplic
    public UF? UF { get; init; }                           // cUF, a UF responsável pelo serviço
    public DateTimeOffset? Recebimento { get; init; }      // dhRecbto
    public TimeSpan? TempoMedio { get; init; }             // tMed, em segundos
    public DateTimeOffset? PrevisaoRetorno { get; init; }  // dhRetorno
    public string? Observacao { get; init; }               // xObs
}
```

No leiaute, o `OperacaoStatus` interno, registrado para a NF-e e para a NFC-e.

1. **No núcleo, não no `OpenDFe.NFe`:** os campos são os mesmos no CT-e e no MDF-e; o PDV e o ERP monitoram qualquer documento com o mesmo `new ConsultaStatus()`, e cada leiaute registra o seu handler.
2. **Imutável:** o construtor com o `cStat` e o `xMotivo`, o resto `init` e anulável, como o `LeituraWS`. O `Status` e o `Motivo` repetem os do `ResultadoWS`, para o retorno valer sozinho (como no `Protocolo`).
3. **`Operando`:** o que o PDV olha para decidir a contingência.
4. **Leitura branda**, sem o schema do retorno, como a da nota (`LEITURA.md`): outra raiz, ou sem o `cStat` (ou ele não numérico) ou o `xMotivo`, é resposta ilegível (`FormatException`, que o núcleo faz `FalhaTransmissaoException` do tipo `RespostaIlegivel`); o opcional que não se lê fica nulo com o aviso `NFE-VALOR` (o `cUF` que não é UF também); o campo desconhecido, em outro namespace ou com filhos é `NFE-DESCONHECIDO`, e o repetido, `NFE-REPETIDO` (vale o primeiro), todos aviso: a resposta já foi processada, e o aviso não pode virar recusa.
5. **O pedido:** o `cUF` é a UF do destino quando ele é uma UF; nos virtuais (SVRS, SVAN, SVC), a do núcleo. O tipo de emissão é o do núcleo (o núcleo em SVC-AN consulta a SVC-AN); para olhar a SVC com o núcleo normal, `Destino = Destino.SvcAN`. Sem assinatura. O AN não tem o status: `NotSupportedException`, sem transmitir.
6. **O PI nas duas SVC também no emulador**, que só emula a homologação (a divergência deixada pela 4.2).
7. **Sem o açúcar `dfe.ConsultarStatusAsync()`** por enquanto (seção 8, decisão 5).

## 14. A autorização (etapa 4.4)

**Aprovado em 2026-10-22**, as 14 decisões pela recomendação, e implementado na etapa 4.4 com os ajustes da 14.7. A autorização da NF-e (síncrona e por lote), a consulta do recibo, o `nfeProc`, o `Preparar` e a autorização do XML já assinado. A NFC-e entra na 5.1, com o QR Code (decisão 12).

### 14.1. No leiaute (`OpenDFe.NFe`, pacote `OpenDFe.NFe`)

```csharp
/// A autorização de 1 a 50 notas (NFeAutorizacao4). Uma nota vai na resposta síncrona; de 2 a 50, no lote com
/// recibo (NT 2025.001, GAP03a: o lote de uma nota é 452; a síncrona com mais de uma, 764).
public sealed class Autorizacao : ParametroWS<RetornoAutorizacao>
{
    /// As notas do modelo: o leiaute escreve, o núcleo assina, valida e transmite.
    /// ArgumentException: nenhuma, mais de 50, ou uma nula.
    public Autorizacao(params IEnumerable<NotaFiscal> notas);
    /// As notas já assinadas (o Preparar, a NFC-e offline, o reenvio): vão com os mesmos bytes.
    /// ArgumentException: nenhuma, mais de 50, ou uma nula ou vazia.
    public Autorizacao(params IEnumerable<string> xmls);

    public IReadOnlyList<NotaFiscal> Notas { get; }   // vazia quando vieram os XMLs
    public IReadOnlyList<string> Xmls { get; }        // vazia quando vieram as notas
    /// O idLote (1 a 15 dígitos); null = do relógio da configuração (AAMMDDhhmmssmmm).
    public long? Lote { get; init; }
}

/// A consulta do recibo do lote (NFeRetAutorizacao4).
public sealed class ConsultaRecibo : ParametroWS<RetornoAutorizacao>
{
    /// ArgumentException: o recibo não tem 15 dígitos.
    public ConsultaRecibo(string recibo);
    public string Recibo { get; }
    /// As notas assinadas do lote (ResultadoNota.Xml do retorno com o recibo): com elas, o retorno traz o nfeProc.
    public IReadOnlyList<string> Xmls { get; init; } = [];
}

/// O retorno da autorização e da consulta do recibo. Construtor interno; imutável.
public sealed class RetornoAutorizacao
{
    public int Status { get; }                       // cStat do lote: 103, 104, 105, 106 ou a rejeição do lote
    public string Motivo { get; }
    public Ambiente? Ambiente { get; }
    public string? VersaoAplicativo { get; }
    public UF? UF { get; }
    public DateTimeOffset? Recebimento { get; }
    public string? Recibo { get; }                   // nRec (103); também quando a síncrona devolve o recibo
    public TimeSpan? TempoMedio { get; }             // tMed
    /// Uma por nota enviada, na ordem do parâmetro (ou do Xmls da consulta); na consulta sem os XMLs, uma por
    /// protNFe, na ordem da resposta. Vazia na rejeição do lote.
    public IReadOnlyList<ResultadoNota> Notas { get; }
}

/// Uma nota do lote. Construtor interno; imutável.
public sealed class ResultadoNota
{
    public ChaveAcesso? Chave { get; }
    /// O NFe assinado, como foi: o que se guarda até ter o protocolo. Null na consulta sem os XMLs.
    public string? Xml { get; }
    /// O protNFe (o Protocolo do modelo, LEITURA.md 2.3); null enquanto o lote não foi processado.
    public Protocolo? Protocolo { get; }
    /// O nfeProc (a nota com os bytes assinados e o protNFe): com o protocolo de autorização ou de denegação
    /// (100, 150, 110, 301, 302) e o XML da nota; senão null. É o XML distribuível (PROJETO.md 8.7).
    public string? XmlProcessado { get; }
    /// Protocolo.Status é 100 ou 150.
    public bool Autorizada { get; }
}
```

### 14.2. No núcleo (`OpenDFe`)

```csharp
public sealed class DFe
{
    /// Gera, assina e valida um documento sem transmitir (PROJETO.md 8.3; a NFC-e offline, 9.2): montar → assinar →
    /// schema → regras do leiaute → callbacks, como o ExecutarAsync. Síncrono, sem I/O.
    /// NotSupportedException: o leiaute não registrou o preparo de TDocumento para o documento do núcleo.
    public DocumentoPreparado Preparar<TDocumento>(TDocumento documento) where TDocumento : class;
}

/// O preparo de um documento, no leiaute (a nota). Sem estado e seguro em paralelo.
public interface IPreparo<TDocumento> where TDocumento : class
{
    DocumentoMontado Montar(TDocumento documento, ContextoOperacao contexto);
}

/// O documento montado, sem assinar. Construtor público: o leiaute cria.
public sealed class DocumentoMontado
{
    /// documento: o Id que o núcleo assina, o modelo, a chave e a conversão (REGRAS.md 3).
    public DocumentoMontado(string xml, SchemaXml schema, DocumentoPedido documento);
    public string Xml { get; }
    public SchemaXml Schema { get; }
    public DocumentoPedido Documento { get; }
    public IReadOnlyList<Problema> Problemas { get; init; } = [];
}

/// O resultado do Preparar. Construtor interno.
public sealed class DocumentoPreparado
{
    /// Sem problema de severidade Erro: o Xml está assinado e válido.
    public bool Preparado { get; }
    /// O XML assinado (o que se transmite e se guarda); o montado, se parou antes de assinar; null se a montagem não o produziu.
    public string? Xml { get; }
    public ChaveAcesso? Chave { get; }
    public IReadOnlyList<Problema> Problemas { get; }
}

public sealed class DeclaracaoLeiaute
{
    /// ArgumentException: já há preparo para TDocumento.
    public void RegistrarPreparo<TDocumento>(IPreparo<TDocumento> preparo) where TDocumento : class;
}

public sealed class DocumentoPedido
{
    /// O schema do documento sozinho (a nota, 225), validado antes do schema da mensagem (o lote, 215).
    public SchemaXml? Schema { get; init; }
}

public interface IOperacao<TParametro, TRetorno>
{
    /// Muda: recebe também a mensagem como foi enviada (assinada), para montar o nfeProc com os mesmos bytes.
    LeituraWS<TRetorno> Ler(TParametro parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto);
}
```

### 14.3. Exemplo de uso

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, configuracao);

var r = await dfe.ExecutarAsync(new Autorizacao(nota), ct);             // síncrona
if (r.Processado && r.Retorno.Notas[0] is { Autorizada: true } n)
    guardar(n.Chave, n.XmlProcessado);                                  // o nfeProc
else if (!r.Processado)
    mostrar(r.Problemas);                                               // Propriedade: "Notas[0].Itens[2].Produto.Valor"

var lote = await dfe.ExecutarAsync(new Autorizacao(notas), ct);         // 2 a 50: 103 com o recibo
guardar(lote.Retorno!.Notas.Select(x => x.Xml));
await Task.Delay(lote.Retorno.TempoMedio ?? TimeSpan.FromSeconds(1), ct);
var consulta = await dfe.ExecutarAsync(new ConsultaRecibo(lote.Retorno.Recibo!) { Xmls = xmlsGuardados }, ct);

var preparado = dfe.Preparar(nota);                                     // sem transmitir
if (preparado.Preparado)
    await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), ct);      // os mesmos bytes
```

### 14.4. O caminho da autorização

1. **`Montar`:** cada nota do modelo é escrita (`EscritorNFe`, com os valores do núcleo); cada XML assinado é lido só até a chave do `Id`. O `enviNFe` (`idLote`, `indSinc` pela quantidade, as notas na ordem) com um `DocumentoPedido` por nota (o `Id`, o modelo, a chave, o schema da nota e a conversão com o prefixo `Notas[k].` ou `Xmls[k].`); `IdsAssinatura` só com as notas do modelo. Os XMLs entram com os bytes como vieram. Destino e tipo de emissão: os do núcleo nas notas do modelo; os da chave nos XMLs (8.4). As regras do lote (decisão 10) são problemas da montagem.
2. **O núcleo** assina, valida **cada nota pelo schema dela (225)** e, só sem erro nelas, **a mensagem pelo `enviNFe` (215)**; depois as regras e os callbacks por nota, a URL, o SOAP.
3. **`Ler`:** o `retEnviNFe` ou o `retConsReciNFe`, brando como o status (seção 13, decisão 4); cada `protNFe` pelo leitor do protocolo da 2.5, ligado à nota pelo `chNFe`. O `protNFe` de uma chave que não está no lote é aviso (`NFE-PROTOCOLO`) e fica fora; a nota sem protocolo num lote processado fica com o `Protocolo` nulo e um aviso. O `nfeProc` é escrito com a nota como foi enviada e o `protNFe` da resposta.

### 14.5. Decisões

1. **Onde ficam os tipos.** (a) **`Autorizacao`, `ConsultaRecibo`, `RetornoAutorizacao` e `ResultadoNota` no `OpenDFe.NFe` (o projeto do leiaute), porque falam da `NotaFiscal` e do `Protocolo`; o preparo no núcleo, comum a todo documento**; (b) tudo no núcleo, com `object` no lugar da nota. Recomendo (a): é o `PROJETO.md` 3.4 ("os parâmetros e retornos próprios da NF-e"), e o CT-e terá os seus.
2. **Síncrona ou lote.** (a) **sem a propriedade `Sincrona`: uma nota vai sempre na síncrona (`indSinc=1`) e de 2 a 50 sempre no lote com recibo**; (b) a `Sincrona` do `PROJETO.md` 8.3, conferida (`ArgumentException` na combinação que a SEFAZ rejeita). Recomendo (a): desde a NT 2025.001 a quantidade decide (452 e 764), e a propriedade só serviria para errar. **Diverge do `PROJETO.md` 8.3.**
3. **A entrada.** (a) **dois construtores, as notas do modelo ou os XMLs assinados, sem misturar no mesmo lote**; (b) uma lista de um tipo que embrulha os dois; (c) também o `DocumentoPreparado`. Recomendo (a): o `preparado.Xml` já é o XML; misturar no mesmo lote não tem uso conhecido e complica a ordem e a assinatura.
4. **O retorno.** (a) **`RetornoAutorizacao` com os campos do lote e uma `ResultadoNota` por nota (a chave, o XML assinado, o `Protocolo`, o `nfeProc`, `Autorizada`); o mesmo tipo para a síncrona, o lote e a consulta do recibo; construtores internos**; (b) um retorno para a síncrona e outro para o lote. Recomendo (a): é o `PROJETO.md` 8.3 (a consulta do recibo devolve `RetornoAutorizacao`), e a síncrona também pode devolver o recibo; o `Protocolo` é o do modelo, que a leitura da 2.5 já preenche.
5. **Quando sai o `nfeProc`.** (a) **com o protocolo de autorização (100, 150) e de denegação (110, 301, 302); `Autorizada` só no 100 e no 150**; (b) só no 100 e no 150. Recomendo (a): a nota denegada também é guardada com o protocolo (o `nfeProc` é a forma legal dela); a NT 2024.001 tirou o 301 e o 302 das regras novas, mas uma SEFAZ ainda pode devolvê-los.
6. **A mensagem enviada na leitura.** (a) **o `IOperacao.Ler` passa a receber a mensagem assinada (`xmlPedido`)**; (b) um membro de interface padrão, com o `Ler` antigo ficando; (c) o núcleo monta o `nfeProc`. Recomendo (a): sem ela, a leitura não tem os bytes assinados da nota do modelo (o núcleo assina depois do `Montar`). Nada foi publicado (os `PublicAPI.Shipped.txt` estão vazios), então mudar a interface não quebra ninguém; (b) deixaria dois `Ler` para sempre; (c) poria a NF-e no núcleo.
7. **A consulta do recibo e o `nfeProc`.** (a) **`ConsultaRecibo(recibo) { Xmls }`: com as notas do lote, o retorno liga o protocolo à nota e traz o `nfeProc`; sem elas, só os protocolos**; (b) só o recibo, e o `nfeProc` montado por quem usa. Recomendo (a): quem usa já guarda os XMLs do lote (o `ResultadoNota.Xml` do retorno com o recibo), e montar o `nfeProc` à mão é o erro que a lib existe para evitar.
8. **O `Preparar`.** (a) **no núcleo, genérico pelo tipo do documento: `DFe.Preparar<TDocumento>`, com o `IPreparo<TDocumento>` registrado pelo leiaute (`RegistrarPreparo`), o `DocumentoMontado` e o `DocumentoPreparado`; o caminho é o do `ExecutarAsync` até os callbacks**; (b) um método do leiaute (`LeiauteNFe400.Preparar(dfe, nota)`); (c) `Preparar(object)` sem tipo. Recomendo (a): o schema, as regras e os callbacks são do núcleo e não podem ser opcionais (6.1), como na decisão 2 da seção 8; o tipo é deduzido (`dfe.Preparar(nota)`), sem reflexão. É o "membro novo no `DFe` e na `DeclaracaoLeiaute`" previsto na seção 9.
9. **A nota do lote no problema** (a decisão 4 do `MODELO.md` 14 e a 12 do `REGRAS.md`). (a) **cada nota validada sozinha pelo schema da nota (225) e, sem erro nelas, a mensagem pelo `enviNFe` (215); o caminho é o da nota (`NFe/infNFe/det[3]/prod/vProd`) no schema, nas regras e nos callbacks, igual ao do `Preparar`; a propriedade leva o índice do parâmetro (`Notas[0].Itens[2].Produto.Valor`, `Xmls[1]...`)**; (b) só o `enviNFe` (215), com o caminho da mensagem (`enviNFe/NFe[2]/...`) e a propriedade sem o prefixo; (c) um membro novo `Problema.Documento`. Recomendo (a): o problema da nota sai igual no `Preparar` e na autorização, o código é o que a SEFAZ dá à nota (225), e o prefixo diz qual nota sem membro novo no `Problema`. Custa o `DocumentoPedido.Schema` (membro novo) e uma validação a mais por nota.
10. **As regras do lote** (`PROJETO.md` 2.9: listadas para aprovação). (a) **estas, como problemas da montagem**:

    | Regra | Código | Severidade | O que confere |
    |---|---|---|---|
    | B02-20 | 476 | Erro | as notas do lote com a mesma UF (só os XMLs podem divergir; as do modelo levam a do núcleo) |
    | B06-20 | 765 | Erro | as notas com o modelo do núcleo (o XML de NFC-e num núcleo de NF-e) |
    | C02-20 | 560 | Aviso (facultativa) | o CNPJ base ou o CPF do emitente igual ao da primeira nota |
    | F02 | 297 | Erro | a assinatura de cada XML recebido conferida (`AssinaturaDigital`, 1.4): o XML alterado depois de assinado |
    | (lib) | `NFE-LOTE` | Erro | a mesma chave duas vezes no lote; tipos de emissão diferentes no lote (iriam a autorizadores diferentes) |

    Ficam fora: o 214 (tamanho da mensagem, 500 KB), para a SEFAZ; o 452, o 764 e o 776, que a escolha da decisão 2 evita. (b) nenhuma, tudo para a SEFAZ. Recomendo (a).
11. **O `idLote`.** (a) **`Lote` opcional; sem ele, do relógio da configuração (`AAMMDDhhmmssmmm`, 15 dígitos)**; (b) sorteado; (c) obrigatório. Recomendo (a): legível no rastro e testável com o relógio falso; a SEFAZ não exige que seja único.
12. **A NFC-e.** (a) **a autorização, a consulta do recibo e o preparo registrados só para a NF-e nesta etapa; a NFC-e entra na 5.1, com o QR Code e o `infNFeSupl` escritos pela lib**; (b) registrados para os dois já, com o QR Code preenchido por quem usa até a 5.1. Recomendo (a): a NFC-e sem QR Code é rejeitada, e (b) deixaria quem usa montando o `infNFeSupl` à mão por uma fase.
13. **O `cNF` sorteado.** (a) **a escrita continua sem alterar a nota (`MODELO.md` 6): executar de novo a mesma nota sorteia outra chave; o reenvio depois de uma queda usa o XML (o `ResultadoNota.Xml`, o `Rastro.XmlPedido`, ou o `Preparar` antes), e o guia da 4.5 diz isso**; (b) a autorização grava o `cNF` sorteado na nota. Recomendo (a): a mesma numeração com outra chave a SEFAZ recusa (539), e o caminho seguro para o reenvio é o XML já assinado; (b) faria a lib mexer no objeto de quem usa.
14. **Sem consulta automática do recibo.** (a) **o 103 volta com o recibo e o `TempoMedio`; consultar é de quem usa**; (b) a lib espera e consulta até o 104. Recomendo (a): segue o 8.6 (a lib não repete chamadas sozinha) e não prende o `ExecutarAsync` por um tempo que quem usa não controla; um açúcar que espera entra depois, se pedido.

### 14.6. Testes previstos

- **Leiaute, com o handler que responde o retorno do caso:**
  - a montagem: uma nota síncrona, o lote de 2 e de 50, os XMLs assinados com os mesmos bytes, o `idLote` do relógio e o informado, o destino e a emissão da chave;
  - cada regra da decisão 10;
  - o schema da nota (225) e da mensagem (215), com o caminho e a propriedade com o prefixo;
  - a leitura da síncrona, do 103, do 104 com os protocolos, do 105, do 106, da rejeição do lote, do `protNFe` de outra chave e da nota sem protocolo;
  - o `nfeProc` válido no `procNFe` e com os bytes da nota;
  - o `Preparar`: o preparado, o recusado, o XML montado, as regras e os callbacks.
- **Contra a SEFAZ emulada:** a NF-e autorizada (100), síncrona e no lote de 2 com a consulta do recibo (104); a rejeição; o 105 pelo controle; a duplicidade (204 e 539); a SVC-AN com MG paralisado; o `Preparar` e a autorização do XML depois; a queda depois de autorizar e o reenvio do mesmo XML (204).
- **Homologação real:** a NF-e autorizada em MG, pulada sem o PFX.


### 14.7. Implementação (etapa 4.4, 2026-10-22)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **`DocumentoPedido.PrefixoPropriedade`** (membro novo, além do `Schema`): o prefixo da decisão 9 (`Notas[1].`) é posto pelo núcleo nos problemas do documento (o schema dele, as regras e os callbacks). A conversão do documento não podia carregá-lo: as regras do leiaute calculam a propriedade sozinhas, e o callback `Campo` passaria a ver `Notas[0].Emitente.Cnpj` na autorização e `Emitente.Cnpj` no `Preparar`. Com o prefixo à parte, o `CampoVerificado.Propriedade` é sempre o da nota.
- **`RetornoAutorizacao.Mensagem`** (membro novo): o `cMsg` e o `xMsg` do lote, que o `retConsReciNFe` pode trazer; sem ele, a mensagem da SEFAZ sumiria calada.
- **`Autorizacao.MaximoNotas`** (50), público, usado também pela `ConsultaRecibo`.
- **A assinatura da nota recebida** (regra F02 da decisão 10): o `AssinaturaDigital.Verificar` da 1.4, com as rejeições da assinatura e do certificado (297, 298, 290); o titular (213, 227, 292) não entra, por não estar na lista aprovada. A nota recebida que não é XML, não é um `NFe` ou não tem a chave no `Id` é `NFE-XML`, `NFE-RAIZ` ou `NFE-CHAVE` (os códigos da leitura), sem a nota no lote.
- **A consulta do recibo** vai ao autorizador da chave da primeira nota em `Xmls` (o lote foi para lá); sem as notas, ao do núcleo.
- **A leitura do retorno:** o `protNFe` é lido pelo leitor do protocolo da 2.5, e os problemas dele saem como aviso, com o caminho a partir da raiz do retorno (`retEnviNFe/protNFe[2]/infProt/...`) e a propriedade a partir do retorno (`Notas[0].Protocolo.Recebimento`). A nota sem protocolo só é aviso no 104; no 103 e no 105, é o esperado. O resultado das notas sai no 103, 104 e 105, ou quando veio algum `protNFe`.
- **O `nfeProc`** leva a nota com os bytes da mensagem enviada (as do modelo, tiradas do `enviNFe` assinado; as recebidas, como entraram no lote, sem o BOM e a declaração) e o `protNFe` como o núcleo o leu do envelope (equivalente, não byte a byte, como na 4.1).
- **O `idLote` do relógio** é em UTC.
- **O preparo sem chave** (a escrita com erro): o documento leva o `Id` `"NFe"` só para existir; nada é assinado, porque o erro da montagem para antes.
- **A nota repetida no lote** entra uma vez só nos documentos do pedido (o `PedidoWS` recusa dois com o mesmo `Id`); o `NFE-LOTE` já recusa o lote.

## 15. A queda no meio (etapa 4.5)

**Aprovado em 2026-10-22**, as 13 decisões pela recomendação (com a regra da decisão 3), e implementado na etapa 4.5 com os ajustes da 15.6. A consulta do protocolo pela chave, a recuperação da autorização cuja resposta se perdeu (`PROJETO.md` 8.6), a duplicidade identificada com a chave e o guia `docs/QUANDO_CAI.md`. O caminho do `ExecutarAsync` (seção 3) não muda; o núcleo não ganha membro novo.

### 15.1. No leiaute (`OpenDFe.NFe`)

```csharp
/// A consulta da situação de uma nota pela chave (NFeConsultaProtocolo4: consSitNFe, retConsSitNFe).
public sealed class ConsultaProtocolo : ParametroWS<RetornoProtocolo>
{
    /// ArgumentNullException na chave nula. O destino e o tipo de emissão saem da chave (PROJETO.md 8.4).
    public ConsultaProtocolo(ChaveAcesso chave);
    public ChaveAcesso Chave { get; }
}

/// O retorno da consulta do protocolo. Construtor interno; imutável.
public sealed class RetornoProtocolo
{
    public int Status { get; }                      // cStat: 100, 150, 101 (cancelada), 110/301/302, 217 (não consta), 562...
    public string Motivo { get; }
    public Ambiente? Ambiente { get; }
    public string? VersaoAplicativo { get; }
    public UF? UF { get; }
    public DateTimeOffset? Recebimento { get; }
    public ChaveAcesso? Chave { get; }              // chNFe; nulo quando não se lê
    /// O protNFe da autorização (ou da denegação), quando a nota consta.
    public Protocolo? Protocolo { get; }
    /// Cada procEventoNFe como veio (o cancelamento, a CC-e), na ordem da resposta. O evento tipado vem na fase 6.
    public IReadOnlyList<string> Eventos { get; }
    /// No 562 (outro cNF na base), a chave da nota da base, quando o motivo a traz ([chNFe:...]).
    public ChaveAcesso? ChaveRegistrada { get; }
    public bool Autorizada { get; }                 // Status 100 ou 150
    public bool Cancelada { get; }                  // Status 101, 151 ou 155
}

/// A autorização cuja resposta se perdeu (PROJETO.md 8.6): consulta a chave do XML assinado e, achado o protocolo
/// de autorização ou de denegação do mesmo XML, monta o nfeProc. Não reenvia nada.
public sealed class RecuperacaoAutorizacao : ParametroWS<RetornoAutorizacao>
{
    /// O NFe assinado, como foi enviado (ResultadoNota.Xml, DocumentoPreparado.Xml ou Autorizacao.NotasEnviadas).
    /// ArgumentException: nulo ou vazio.
    public RecuperacaoAutorizacao(string xml);
    public string Xml { get; }
}

public sealed class Autorizacao
{
    /// Novo: as notas assinadas, com os bytes como foram, tiradas da mensagem enviada (o Rastro.XmlPedido do
    /// resultado ou da FalhaTransmissaoException). FormatException: não é um enviNFe.
    public static IReadOnlyList<string> NotasEnviadas(string xmlPedido);
}

public sealed class ResultadoNota
{
    /// Novo: no 204 e no 539, a nota que já está na base. Nulo nos outros.
    public Duplicidade? Duplicidade { get; }
}

/// A nota já registrada que causou a rejeição por duplicidade. Construtor interno; imutável.
public sealed class Duplicidade
{
    /// 204: a chave da própria nota (é a mesma nota); 539: a do motivo ([chNFe:...]), ou nula se ele não a traz.
    public ChaveAcesso? Chave { get; }
    /// O recibo do lote em que ela foi autorizada ([nRec:...]), quando o motivo o traz.
    public string? Recibo { get; }
    /// 204: a mesma chave, e a RecuperacaoAutorizacao dá o nfeProc; 539: outra chave com a mesma numeração.
    public bool MesmaChave { get; }
}
```

No leiaute, os internos `OperacaoConsultaProtocolo` e `OperacaoRecuperacao`, com a leitura do `retConsSitNFe` compartilhada pelos dois.

### 15.2. Exemplo de uso

```csharp
ResultadoWS<RetornoAutorizacao> r;
try
{
    r = await dfe.ExecutarAsync(new Autorizacao(nota), ct);
}
catch (FalhaTransmissaoException ex) when (ex.PodeTerSidoProcessado)
{
    var xml = Autorizacao.NotasEnviadas(ex.Rastro.XmlPedido!)[0];         // guardar antes de tudo
    r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), ct);
    // Status 100/150: Notas[0].XmlProcessado é o nfeProc; 217: não chegou, reenviar o MESMO xml
}

if (r.Retorno?.Notas[0].Duplicidade is { MesmaChave: true })             // 204: já estava autorizada
    r = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(r.Retorno.Notas[0].Xml!), ct);

var situacao = await dfe.ExecutarAsync(new ConsultaProtocolo(chave), ct); // a situação e os eventos
```

### 15.3. O caminho

1. **`ConsultaProtocolo`:** o `consSitNFe` (`tpAmb` do núcleo, `xServ` `CONSULTAR`, `chNFe`), sem assinatura, validado pelo `consSitNFe_v4.00.xsd`. Destino e tipo de emissão da chave.
2. **`RecuperacaoAutorizacao`:** o XML lido só até a chave do `Id`, como as notas assinadas da 4.4 (`NFE-XML`, `NFE-RAIZ`, `NFE-CHAVE`); o mesmo `consSitNFe`, ao autorizador da chave. Na leitura, achado o `protNFe` com 100, 150, 110, 301 ou 302 e o `digVal` igual ao `DigestValue` do XML, o `nfeProc` (o mesmo `LoteNFe.NotaProcessada` da 4.4). O `RetornoAutorizacao` leva o `cStat` e o `xMotivo` da consulta, sem `Recibo` nem `TempoMedio`, e uma `ResultadoNota` com o XML, o `Protocolo` e o `nfeProc`.
3. **A leitura** do `retConsSitNFe` é branda, como a do status e a da autorização (seção 13, decisão 4): o `protNFe` pelo leitor da 2.5, cada `procEventoNFe` guardado como texto, o `retCancNFe` (o cancelamento de antes dos eventos) conhecido e ignorado.

### 15.4. Decisões

1. **Onde ficam os tipos.** (a) **no `OpenDFe.NFe`, como os da 4.4: falam do `Protocolo` e do `RetornoAutorizacao`**; (b) a consulta no núcleo. Recomendo (a): é o `PROJETO.md` 3.4.
2. **A entrada da consulta.** (a) **a `ChaveAcesso`, que já confere o tamanho, o DV, a UF, o modelo e o tipo de emissão (o 236 e o 614 a 619 da SEFAZ não chegam a sair)**; (b) o texto da chave. Recomendo (a): o texto vira chave com `ChaveAcesso.Ler`, e a chave errada não sai do computador.
3. **A chave de outro modelo** (regra fora do schema, para aprovação). (a) **a chave de NFC-e num núcleo de NF-e, e o contrário, é recusada na montagem (`NFE-CHAVE`, Erro), como o 765 no lote**; (b) transmitida, e a SEFAZ responde. Recomendo (a): o web service da NF-e e o da NFC-e são outros em várias UFs (MG), e a resposta seria enganosa.
4. **O destino da consulta.** (a) **a UF e o tipo de emissão da chave (8.4): a nota emitida em SVC-AN é consultada na SVC-AN; o `Destino` sobrepõe, para consultar no normal**; (b) sempre o autorizador normal da UF, que acha também as da SVC. Recomendo (a): é a regra que o `PROJETO.md` 8.4 já fixou, e a SVC pode estar no ar quando o normal não está; o guia explica o caso.
5. **O retorno da consulta.** (a) **`RetornoProtocolo` com os campos da resposta, o `Protocolo` do modelo, os eventos como texto e `Autorizada`/`Cancelada`**; (b) os eventos já tipados. Recomendo (a): o evento tipado é da fase 6 (o modelo dele ainda não existe); como nada foi publicado, a fase 6 pode trocar o tipo da lista sem quebrar ninguém.
6. **A recuperação.** (a) **um parâmetro próprio, `RecuperacaoAutorizacao(xml)`, com o mesmo `RetornoAutorizacao` da autorização: quem usa trata os dois resultados com o mesmo código**; (b) `ConsultaProtocolo { Xml }`, com o `nfeProc` no `RetornoProtocolo`. Recomendo (a): é o `PROJETO.md` 8.3, e a pergunta é "a autorização terminou?", não "qual a situação?".
7. **Uma nota por recuperação.** (a) **um XML por chamada (o `ExecutarAsync` faz uma transmissão, e a consulta é por chave); o lote perdido é recuperado nota a nota**; (b) até 50 XMLs, com várias consultas por dentro. Recomendo (a): (b) mudaria o núcleo (uma operação, um SOAP) e misturaria as falhas de várias transmissões num resultado.
8. **O `digVal`.** (a) **o `nfeProc` só sai se o `digVal` do protocolo é o `DigestValue` do XML; diferente, o `Protocolo` vem e o `XmlProcessado` não, com o aviso `NFE-PROTOCOLO` (a base tem outro XML com a mesma chave)**; (b) sem conferir. Recomendo (a): é a única prova de que o protocolo é daquele XML (a mesma chave com outro conteúdo existe: a nota gerada de novo com o mesmo `cNF`), e um `nfeProc` com o XML errado seria um documento falso.
9. **As notas da mensagem perdida.** (a) **`Autorizacao.NotasEnviadas(xmlPedido)`, público e estático: as notas assinadas do `enviNFe` do rastro, com os bytes como foram**; (b) quem usa lê o `enviNFe`. Recomendo (a): a nota do modelo só existe assinada dentro do rastro (decisão 13 da 4.4), e tirá-la à mão com os bytes certos é fácil de errar (o `XDocument` reescreve). Já existe por dentro (`LoteNFe.Enviadas`).
10. **A duplicidade identificada.** (a) **`ResultadoNota.Duplicidade` no 204 e no 539, com a chave e o recibo lidos do motivo (`[chNFe:...]`, `[nRec:...]`, com ou sem espaço depois dos dois-pontos) e `MesmaChave`; no 204 sem a chave no motivo, a da própria nota; o motivo sem o complemento não é aviso** (o MOC o faz opcional); (b) só o `cStat`, e quem usa lê o motivo. Recomendo (a): é o `PROJETO.md` 8.6, e o 204 e o 539 pedem ações opostas (recuperar; investigar a outra nota). O 562 da consulta ganha o mesmo, no `RetornoProtocolo.ChaveRegistrada`.
11. **Sem automatismo** (8.6). (a) **a lib não consulta sozinha depois da falha nem do 204; quem usa decide, seguindo o guia**; (b) a `Autorizacao` recupera sozinha no 204. Recomendo (a).
12. **A NFC-e.** (a) **a `ConsultaProtocolo` registrada para a NF-e e para a NFC-e (não depende do QR Code); a `RecuperacaoAutorizacao` só para a NF-e, e a NFC-e entra na 5.1, com a autorização dela**; (b) as duas só na NF-e. Recomendo (a): o PDV já pode consultar a situação das NFC-e emitidas por outro sistema.
13. **O guia `docs/QUANDO_CAI.md`**, escrito para quem usa a lib, com o código de cada caso: a falha que não chegou (reenviar); a que pode ter chegado (guardar o XML e recuperar; 217: reenviar o mesmo XML; a consulta também caiu: esperar e tentar de novo, nunca gerar outra nota); o 204 (recuperar) e o 539 (consultar a outra chave: a numeração está em uso); o 103 perdido (sem recibo, recuperar nota a nota); a resposta do recibo perdida (consultar o recibo de novo); a contingência com uma nota em dúvida (resolver antes de emitir a mesma numeração em SVC, que daria outra chave e o 539); por que não gerar a nota de novo (o `cNF` sorteado, decisão 13 da 4.4).

### 15.5. Testes previstos

- **Leiaute, com o handler que responde o retorno do caso:** o `consSitNFe` no schema e o destino da chave (também a SVC-AN); a chave de outro modelo; a leitura do 100 com o protocolo e os eventos, do 101, do 217, do 562 com e sem a chave no motivo, e as respostas ilegíveis; a recuperação do 100 com o `nfeProc` válido no `procNFe` e com os bytes, do 110, do 217, do `digVal` diferente e do XML que não é nota; a duplicidade do 204 e do 539, com e sem o complemento, com e sem o espaço; o `NotasEnviadas` com os mesmos bytes e com o que não é `enviNFe`.
- **Contra a SEFAZ emulada:** a consulta da autorizada, a da chave que não consta (217), a do `cNF` diferente (562) e a da SVC-AN (o cancelamento é da fase 6); a queda depois de autorizar (`conexaoDerrubada` com `depois`), a recuperação com o `nfeProc` e o reenvio dando 204 com a `Duplicidade`; a queda antes de processar, a recuperação com o 217 e o reenvio autorizado; o 539 com a chave da outra nota, consultada depois.
- **Homologação real:** a consulta da nota autorizada pelo `AutorizacaoHomologacaoTestes`, pulada sem o PFX.

### 15.6. Implementação (etapa 4.5, 2026-10-22)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **O `Destino` do parâmetro vai ao autorizador normal** (decisão 4). O núcleo escolhe o autorizador da UF pelo tipo de emissão do pedido; com o da chave, `Destino = Destino.MG` numa chave de SVC-AN continuaria na SVC-AN. Com o `Destino` informado, o pedido leva o tipo de emissão normal; sem ele, o da chave. Vale para a consulta e para a recuperação.
- **A recuperação confere a assinatura do XML** (297, 298, 290), pela mesma leitura das notas assinadas da 4.4: o XML vai para o `nfeProc`. O problema do XML leva a propriedade `Xml` (o parâmetro tem um XML só, sem índice).
- **O protocolo sem o `digVal`** também não dá o `nfeProc` (decisão 8), com o aviso `NFE-PROTOCOLO`: sem ele, não há como provar que o protocolo é daquele XML. O protocolo de outra chave fica fora, com o mesmo aviso, como na 4.4.
- **A nota cancelada (101) recuperada** traz o `nfeProc` com o protocolo da autorização: a nota existiu; o cancelamento está nos eventos da consulta.
- **O `chNFe` zerado** das rejeições da mensagem (o emulador e a SEFAZ o mandam assim) fica nulo sem aviso; o ilegível, nulo com o `NFE-VALOR`. O `protNFe` repetido é `NFE-REPETIDO`, e vale o primeiro.
- **O complemento do motivo** (`[chNFe:...]`, `[nRec:...]`) é lido por um apoio interno (`MotivoSefaz`), sem expressão regular: os dígitos entre os dois-pontos e o colchete, com os espaços em volta, no tamanho exato (44 e 15); fora disso, nulo.
- **Os eventos** são o `procEventoNFe` escrito de novo pelo `XElement`, com a declaração do namespace (`<procEventoNFe versao="1.00" xmlns="...">`): equivalentes, não byte a byte, como o `protNFe` do `nfeProc`.

## 16. A NFC-e: QR Code e CSC (etapa 5.1)

**Aprovado em 2026-10-24, as 12 decisões pela recomendação**, com as regras fora do schema da decisão 10 (393, 444, 672, `NFE-QRCODE` e `NFE-NUCLEO`), e implementado na etapa 5.1 com os ajustes da 16.7. Desenho de 2026-10-23: a parte da configuração que é da NFC-e (`PROJETO.md` 3.2 e 9.1), o QR Code e o `infNFeSupl` escritos pela lib, a tabela das URLs de consulta por UF, e a autorização, a consulta do recibo, o preparo e a recuperação registrados também para a NFC-e (a decisão 12 da 4.4 e a 12 da 4.5). O núcleo não muda: a configuração do documento (`ConfiguracaoDocumento`, `DeclararConfiguracaoDocumento`) existe desde a 1.6. A contingência offline (o QR Code com o `digVal` ou com a assinatura) é da 5.2.

**Conferido na fonte (2026-10-23):**
- a **NT 2025.001** (v1.02, a cópia da 3.1, SHA-256 `44b71f16…`; o catálogo cita a v1.03, `333b69f7…`, que só muda regras da autorização): a **versão 3** do QR Code, sem CSC (a online só com a chave, a versão e o ambiente; a offline assinada em RSA-SHA1 com o certificado da nota), ao lado da **versão 2**, com o CSC; o emitente CPF só na versão 3 (444); o 407 (a UF que não aceita a versão 3) saiu na v1.01, "considerando que todas as UF irão disponibilizar o layout do qrCode v3"; o fim do CSC está "previsto, sem data definida";
- o **Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0** (março de 2025, do Portal Nacional, SHA-256 `bf906cc2…`, o mesmo da E.3): os parâmetros de cada forma (4.3.1, 4.3.2, 4.4.1 e 4.4.2), o hash da versão 2 (4.3.4 e 4.3.5), o CSC (4.6: de 16 a 36 caracteres, o identificador com até 6 dígitos, escrito sem os zeros à esquerda), e as URLs por UF no Portal da NFC-e da ENCAT (`nfce.encat.org/desenvolvedor/qrcode/`). Os dois exemplos de hash do manual conferem (`DC6AE2C2…` e `4615A93B…`) com o CSC `SEU-CODIGO-CSC-CONTRIBUINTE-36-CARACTERES`, que tem 41 caracteres (o manual o lista sem o `SEU-`, que não dá o hash);
- a **NT 2026.002 v1.11** (`72c033b2…`): a NF-e com o DANFE Simplificado Tipo 2 (`tpImp` 6) leva o QR Code, só na versão 3 (672), e "a URL de consulta do qrCode será a mesma URL da NFC-e" (ZX02-20, observação 3);
- o **schema** do `PL_010f_v1.04`: o `infNFeSupl` com o `qrCode` (60 a 1000 caracteres, um padrão por forma: a 100, a 2 e a 3, online e offline) e o `urlChave` (21 a 85), os dois obrigatórios no grupo;
- **as URLs por UF:** o Portal da ENCAT está fora do ar hoje (a 4.2 já o achou assim); a página do QR Code foi salva em 2026-10-19 (as duas tabelas, produção e homologação, com datas de troca em GO, MG, RJ, RN e SP no próprio texto), e a da consulta pela chave só existe no Wayback Machine (de 2019). A implementação confere as duas de novo (a ENCAT, senão o portal de cada UF) e registra em `docs/NTS.md` 4, que nasce nela.

### 16.1. No leiaute (`OpenDFe.NFe`)

```csharp
/// A parte da configuração que é da NFC-e (PROJETO.md 3.2, 9.1). Opcional: sem ela, o QR Code sai na versão 3
/// com as URLs do leiaute. Imutável.
public sealed class ConfiguracaoNFCe : ConfiguracaoDocumento
{
    /// O QR Code na versão 3, sem CSC.
    public ConfiguracaoNFCe();
    /// O QR Code na versão 2, com o CSC da UF. ArgumentNullException no CSC nulo.
    public ConfiguracaoNFCe(CodigoSeguranca csc);

    public VersaoQrCode QrCode { get; }          // V3 no construtor vazio; V2 com o CSC
    public CodigoSeguranca? Csc { get; }         // null na versão 3

    /// Trocam as da tabela do leiaute (LeiauteNFe400.UrlsConsulta) na UF e no ambiente do núcleo.
    /// UrlQrCode: o endereço sem a query (a lib acrescenta o "?p="); UrlChave: como vai no urlChave.
    /// ArgumentException: URL relativa, fora de http e https, ou com query no UrlQrCode.
    public Uri? UrlQrCode { get; init; }
    public Uri? UrlChave { get; init; }
}

/// A versão do QR Code que a lib escreve. A 100 (a primeira) só se lê: a SEFAZ a recusa (903).
public enum VersaoQrCode
{
    V2 = 2,
    V3 = 3,
}

/// O CSC (Código de Segurança do Contribuinte) que a SEFAZ da UF deu à empresa (Manual 4.6). O ToString não mostra
/// o código, que é segredo.
public sealed class CodigoSeguranca
{
    /// ArgumentOutOfRangeException: identificador fora de 1 a 999999.
    /// ArgumentException: código com menos de 16 ou mais de 36 caracteres, ou com caractere fora do ASCII visível,
    /// ou com a barra vertical.
    public CodigoSeguranca(int identificador, string codigo);
    public int Identificador { get; }            // escrito sem os zeros à esquerda
    public string Codigo { get; }
    public override string ToString();           // "CSC 1"
}

/// A URL de consulta da NFC-e de uma UF num ambiente: uma linha da tabela do leiaute. Construtor interno.
public sealed class UrlConsultaNFCe
{
    public UF UF { get; }
    public Ambiente Ambiente { get; }
    public Uri QrCode { get; }                   // sem a query
    public Uri Chave { get; }                    // o urlChave
}

public sealed class LeiauteNFe400
{
    /// Novo: a tabela das URLs de consulta (PROJETO.md 2.6), as 27 UFs nos dois ambientes, para listar (a tela de
    /// configuração). A NF-e com tpImp 6 usa a mesma (NT 2026.002).
    public IReadOnlyList<UrlConsultaNFCe> UrlsConsulta { get; }
}
```

**O que muda na declaração** (o `Declarar` do leiaute): para a NFC-e, a `Autorizacao`, a `ConsultaRecibo`, a `RecuperacaoAutorizacao` e o preparo passam a ser registrados (eram só da NF-e), e a `ConfiguracaoNFCe` é declarada opcional (`obrigatoria: false`); para a NF-e, também opcional, só pelas URLs (decisão 8). A `ConsultaProtocolo` já estava nos dois.

**A escrita** (o `EscritorNFe`, que o `Montar` da autorização e o do preparo usam) passa a gerar o `infNFeSupl` depois do `infNFe`, na NFC-e e na NF-e com `tpImp` 6, com a chave da nota, o ambiente do núcleo e a configuração. O `infNFeSupl` fica fora do `infNFe`, que é o que a assinatura cobre (a referência `#NFe<chave>`): escrito antes de assinar, ele não muda o `DigestValue`.

### 16.2. Exemplo de uso

```csharp
// A NFC-e com o QR Code na versão 3: nada a configurar.
var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes,
});
var r = await nfce.ExecutarAsync(new Autorizacao(nota), ct);
// r.Retorno.Notas[0].XmlProcessado: o nfeProc com o infNFeSupl gerado (qrCode e urlChave)

// Com o CSC (versão 2) e a URL do QR Code trocada:
var comCsc = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes,
    Documento = new ConfiguracaoNFCe(new CodigoSeguranca(1, csc)) { UrlQrCode = new Uri("https://nova.exemplo.gov.br/qrcode") },
});

var preparado = nfce.Preparar(nota);                              // o XML assinado, já com o QR Code
var mg = new LeiauteNFe400().UrlsConsulta.First(u => u.UF == UF.MG && u.Ambiente == Ambiente.Producao);
```

### 16.3. O QR Code (Manual 6.0, 4.3 e 4.4)

O texto é a URL de consulta da UF, o `?p=` e os parâmetros separados por `|`:

| Forma | Parâmetros |
|---|---|
| Versão 2, online | `chave\|2\|tpAmb\|idCSC\|hash`; o hash é o SHA-1 de `chave\|2\|tpAmb\|idCSC` com o CSC colado no fim, em hexadecimal (40 caracteres) |
| Versão 3, online | `chave\|3\|tpAmb` |
| Versão 2, offline (5.2) | `chave\|2\|tpAmb\|dia\|vNF\|digVal em hexadecimal\|idCSC\|hash` |
| Versão 3, offline (5.2) | `chave\|3\|tpAmb\|dia\|vNF\|tipo do destinatário\|documento\|assinatura`; a assinatura é RSA-SHA1 dos sete primeiros, em Base64, com o certificado da nota |

O `idCSC` sem zeros à esquerda; o hash em maiúsculas, como nos exemplos do manual (a SEFAZ não distingue); o texto em UTF-8 (4.5.3), sem CDATA (a NT 2025.001, nota 4, dispensa nas versões 2 e 3). O `urlChave` é a URL de consulta pela chave da tabela, como está. A imagem do QR Code (nível M, 25 mm) é do DANFCE, na fase 7.

### 16.4. Decisões

1. **As versões.** (a) **a versão 3 por padrão e a 2 só com o CSC configurado**; (b) só a versão 3, sem CSC na lib; (c) a versão 2 por padrão, com o CSC obrigatório (o exemplo do `PROJETO.md` 3.2). Recomendo (a): a 3 é o rumo da NT (o CSC vai acabar), é a única do emitente CPF e dispensa segredo na configuração; a 2 custa pouco (um SHA-1), o emulador já a confere, e quem já tem o CSC cadastrado continua com ele. (b) seria mais simples, mas tiraria a escolha de quem já opera com a 2. **Diverge do `PROJETO.md` 3.2 e 9.1**, que a implementação atualiza.
2. **A forma da configuração.** (a) **dois construtores, a versão sai de qual foi usado (`new ConfiguracaoNFCe()` é a 3; `new ConfiguracaoNFCe(csc)`, a 2); imutável**; (b) propriedades `init` (`QrCode`, `IdCsc`, `Csc`), com o problema na montagem quando não combinam (a 2 sem o CSC). Recomendo (a): a combinação errada não se escreve, e o erro sai na hora de configurar, não na nota. **Diverge do exemplo do `PROJETO.md` 3.2** (`{ IdCsc = "1", Csc = csc }`) **e do `CONTRATOS.md` 4 e 5**.
3. **Obrigatória?** (a) **opcional: sem a configuração, a versão 3 com as URLs da tabela**; (b) obrigatória, como o exemplo do `CONTRATOS.md` 5 (`obrigatoria: true`). Recomendo (a): com a versão 3 não há nada a configurar, e o PDV cria o núcleo sem pedir nada ao usuário.
4. **O CSC.** (a) **um tipo próprio, `CodigoSeguranca(identificador, codigo)`, com o identificador numérico (1 a 999999, escrito sem zeros) e o código conferido (16 a 36 caracteres ASCII visíveis, sem a barra vertical, que quebraria os parâmetros); o `ToString` não mostra o código**; (b) duas propriedades de texto na configuração. Recomendo (a): o identificador como texto deixa passar `"01"` (o manual manda sem os zeros, e o hash sai diferente); o código é segredo e não pode cair num log pelo `ToString`. O manual diz "alfanuméricos", mas o exemplo dele tem hífen: a conferência fica no que quebra o QR Code, não no alfabeto.
5. **As URLs.** (a) **uma tabela interna do leiaute, UF × ambiente → URL do QR Code e URL da consulta pela chave, listada pelo `LeiauteNFe400.UrlsConsulta`, com a troca na `ConfiguracaoNFCe` (só a da UF e do ambiente do núcleo, que são os da nota); a UF sem URL na tabela e sem a troca é `Erro` na montagem (`NFE-QRCODE`)**; (b) a troca pelas `ConfiguracaoDFe.Urls`, como as dos web services. Recomendo (a): o `EnderecoServico` é de autorizador e serviço SOAP, só aceita HTTPS (várias consultas são `http`) e não tem UF; a troca na configuração do documento é o lugar que o `PROJETO.md` 3.2 já dá a essa parte. URL trocada na tabela é versão menor (`PROJETO.md` 12.3).
6. **Onde o QR Code é escrito.** (a) **na escrita da nota (a montagem), depois do `infNFe`: as formas online só dependem da chave, do ambiente e da configuração, e o `infNFeSupl` está fora do que a assinatura cobre**; (b) no núcleo, depois de assinar, por um passo novo. Recomendo (a) para a 5.1: nada muda no núcleo. As formas offline precisam do `DigestValue` (a 2) ou de assinar com o certificado (a 3); como o `DigestValue` do `infNFe` já se calcula antes da assinatura (a transformação é *enveloped*, e a `Signature` fica fora do `infNFe`), a 5.2 decide se o leiaute o calcula ou se o núcleo ganha um passo, sem mudar o que a 5.1 publica.
7. **O `Suplementares` preenchido por quem usa** (a decisão 15 do `MODELO.md`, confirmada). (a) **a lib gera sempre; preenchido igual ao gerado passa, e diferente é `Erro` (`NFE-NUCLEO`, como o campo que o núcleo já sabe, `MODELO.md` decisão 6)**; (b) o preenchido vence. Recomendo (a): o QR Code de outra chave ou de outro CSC daria rejeição (397, 464) ou um DANFCE que não consulta. Para reenviar, o caminho seguro continua sendo o XML assinado (4.4, decisão 13).
8. **A NF-e com o DANFE Simplificado Tipo 2** (`tpImp` 6, NT 2026.002). (a) **a lib gera o QR Code também nela, sempre na versão 3 (a única aceita, 672), com a URL da NFC-e da UF; a `ConfiguracaoNFCe` passa a ser aceita (opcional) no núcleo de NF-e só pelas URLs, e a versão 2 nela é `Erro` (a regra 672 da decisão 10)**; (b) gera com a tabela, sem troca de URL na NF-e; (c) fica para depois, e quem usa preenche. Recomendo (a): sem o QR Code a nota é rejeitada (394), e (c) poria quem usa montando o QR Code à mão, o que a decisão 7 recusa. O nome `ConfiguracaoNFCe` num núcleo de NF-e é estranho, mas é a configuração da consulta da NFC-e, que a NT manda usar.
9. **A contingência offline antes da 5.2.** (a) **a nota com `tpEmis` 9 (NFC-e, ou NF-e com `tpImp` 6) é `Erro` na montagem (`NFE-QRCODE`: o QR Code da contingência offline entra na etapa 5.2)**; (b) escrita sem o `infNFeSupl`. Recomendo (a): (b) passaria pelo schema (o grupo é opcional) e seria rejeitada (394) só na transmissão, que pode ser horas depois.
10. **As regras fora do schema** (`PROJETO.md` 2.9: listadas para aprovação). (a) **estas, como regras do leiaute (`RegraNFe`, depois do schema) e como problemas da montagem**:

    | Regra | Código | Severidade | O que confere |
    |---|---|---|---|
    | ZX01-10 | 393 | Erro | a NF-e sem `tpImp` 6 com o `Suplementares` preenchido (a lib não gera nela; o grupo veio de quem usa) |
    | ZX02-222 | 444 | Erro | a versão 2 (o CSC configurado) com o emitente CPF |
    | ZX02-220 | 672 | Erro | a NF-e com `tpImp` 6 e a versão 2 configurada |
    | (lib) | `NFE-QRCODE` | Erro | na montagem: a UF sem URL na tabela e sem a troca (decisão 5); a contingência offline antes da 5.2 (decisão 9) |
    | (lib) | `NFE-NUCLEO` | Erro | na montagem: o `Suplementares` preenchido e diferente do gerado (decisão 7) |

    As outras do grupo ZX ficam como estão no catálogo, com a situação revista: os parâmetros, o hash e a versão (396, 397, 398, 400, 464, 903, 813) a montagem evita ("escrita"); o CSC não cadastrado e o revogado (462, 463) e a URL divergente da tabela da UF (395, 878) ficam para a SEFAZ: a lib usa a tabela, mas não tem como saber o CSC que a SEFAZ tem; as da offline (445, 474, 583 e as de dia, valor, `digVal` e destinatário) vão para a 5.2. (b) nenhuma, tudo para a SEFAZ. Recomendo (a).
11. **A NFC-e nas operações da 4.4 e da 4.5.** (a) **a autorização (síncrona com uma nota, lote de 2 a 50, como na NF-e: a NT 2025.001 obriga a síncrona com uma nota nos dois modelos), a consulta do recibo, o preparo e a recuperação registrados para a NFC-e, com o mesmo código da NF-e**; (b) a NFC-e só na síncrona. Recomendo (a): a NFC-e emitida offline vai em lote depois (5.2), e as regras do lote da 4.4 (476, 765, 560, 297, `NFE-LOTE`) já valem para os dois modelos.
12. **Sem gerar a imagem.** (a) **a lib escreve só o texto do QR Code; a imagem (nível M, UTF-8) é do DANFCE (fase 7)**; (b) um método que desenha o QR Code agora. Recomendo (a): é a ordem do `PLANO.md`, e o texto já está no `nfeProc`.

### 16.5. Testes previstos

- **Leiaute:**
  - a configuração: os dois construtores, cada recusa do `CodigoSeguranca` (o identificador 0 e 1000000; o código com 15 e 37 caracteres, com espaço, com a barra vertical, com acento), o `ToString` sem o código, as URLs trocadas e as recusadas;
  - o hash contra os dois exemplos do manual (pelo cálculo interno, porque o CSC do exemplo tem 41 caracteres) e contra o emulador;
  - a escrita: a versão 3 e a 2 online na NFC-e, nos dois ambientes, com a URL da tabela e a trocada; o `infNFeSupl` válido no schema depois de assinado; a assinatura que não muda com ele; a NF-e com `tpImp` 6 (versão 3) e sem ele (sem o grupo); o `Suplementares` igual (passa) e diferente (`NFE-NUCLEO`); a UF sem URL; o `tpEmis` 9;
  - cada regra da decisão 10;
  - a tabela: as 27 UFs nos dois ambientes, as URLs absolutas, a do QR Code sem query, o `urlChave` de 21 a 85 caracteres;
  - o registro das operações na NFC-e (`LeiauteNFe400Testes`) e a configuração aceita nos dois documentos;
  - o `Preparar` da NFC-e e a autorização do XML preparado.
- **Contra a SEFAZ emulada:** a NFC-e autorizada (100) na versão 3 e na 2 com o CSC padrão do emulador (`1`, `OPENDFE-SEFAZ-EMULADA-CSC-PADRAO`); o CSC não cadastrado (462) e o revogado (463) pela rota `/_controle/csc`; o CSC errado (464); o emitente CPF na versão 3; a NF-e com `tpImp` 6; o lote de duas NFC-e com a consulta do recibo; o `Preparar` e a autorização do XML; a recuperação depois da queda.
- **Homologação real:** a NFC-e autorizada em MG, pulada sem o PFX (e, na versão 2, sem o CSC de homologação: `PROJETO.md` 17).

### 16.6. Fora desta etapa

- **A contingência offline** (5.2, feita: seção 17): as duas formas offline do QR Code, o `dhCont` e o `xJust`, o `Preparar` em `tpEmis` 9 e a transmissão depois.
- **A imagem do QR Code e o DANFCE** (fase 7).
- **O CSC por web service** (Manual 4.6, "a critério da UF"): não há especificação nacional publicada; quem usa cadastra o CSC no portal da UF.

### 16.7. Implementação (etapa 5.1, 2026-10-24)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **A tabela de URLs tem só MG.** A ENCAT e o Wayback Machine estavam fora do ar, e o portal das outras UFs não deu as duas URLs de forma que se possa tomar como fonte primária (`docs/NTS.md` 4). Pela regra "nunca inventar URL", as 26 UFs restantes ficam fora, e o `NFE-QRCODE` da decisão 5 acusa na montagem: quem emite fora de MG informa `UrlQrCode` e `UrlChave` na `ConfiguracaoNFCe`. O teste "as 27 UFs nos dois ambientes" virou "as UFs conferidas", com a lista das que faltam (`UFsPendentes`) no teste. A linha de MG segue a página da SEF-MG, que lista o mesmo QR Code nos dois ambientes (a conferir com o PFX).
- **`UrlQrCode` e `UrlChave` são validadas no `init`** (`ArgumentException`: relativa, fora de http e https, ou, no QR Code, com query ou fragmento), e a lib usa a URL como foi dada (`OriginalString`), sem normalizar.
- **A troca de URL vale por campo:** a `UrlQrCode` trocada e a `UrlChave` da tabela (ou o contrário) é aceito; na UF sem linha na tabela, as duas têm de estar na configuração, e o `NFE-QRCODE` diz qual falta.
- **A versão 2 só vale na NFC-e.** Na NF-e com `tpImp` 6 a lib escreve sempre a versão 3, mesmo com o CSC na configuração; a configuração com o CSC é o erro da 672 (regra do leiaute depois do schema), não da escrita.
- **O 393 (ZX01-10) é só da NF-e sem `tpImp` 6.** A escrita nesse caso não gera nem recusa o grupo: escreve o que veio, e a regra o acusa. Na NFC-e e na NF-e com `tpImp` 6, o grupo preenchido que difere do gerado é `NFE-NUCLEO`, campo a campo (`qrCode`, `urlChave`), com o caminho `NFe/infNFeSupl/<campo>` e a propriedade `Suplementares.<campo>`; o campo nulo ou vazio no que veio não diverge.
- **O 444 (ZX02-222) é só da NFC-e** (modelo 65 no catálogo) e confere o emitente com CPF contra a versão 2 da configuração; o 672 (ZX02-220), só da NF-e.
- **O 407 do catálogo** (ZX02-220, modelo 65, "NFC-e com qrCode na versão 3") vai para "excluída": a própria NT 2025.001 o tirou na v1.01. Não está no código.
- **Catálogo (`REGRAS.md` 15):** 3 linhas "lib (5.1)" (393, 444, 672); as parâmetro/hash/versão (394, 396, 397, 398, 399, 400, 464, 813, 903) "escrita"; 395, 462, 463 e 878 "SEFAZ"; 445, 474, 583 e os 397 de dia, valor, `digVal` e destinatário "a fazer (5.2)".
- **O `NFE-QRCODE` da contingência offline** (decisão 9) vale para a NFC-e e para a NF-e com `tpImp` 6; a nota com `tpEmis` 9 sem o `tpImp` 6 na NF-e não leva QR Code e passa.
- **Testes antigos mudados por consequência da etapa** (a lib passa a gerar o grupo): o suporte da NFC-e nas operações e a declaração (`LeiauteNFe400Testes`, `AutorizacaoTestes`), o final do XML da NFC-e escrita (`EscritaFormaTestes.OXmlSaiSemDeclaracao...` e `AsSuplementaresSaemDepoisDoInfNFe`), a contingência da NFC-e (`AContingenciaIgualANucleoNaoEProblema` agora também vê o `NFE-QRCODE`) e a ida e volta da leitura (`LeitorNFeTestes.AssertIdaVolta` copia o grupo gerado para a nota de origem). Nenhum teste foi enfraquecido para passar.

## 17. A contingência offline da NFC-e (etapa 5.2)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/5.2-contingencia-offline.md`). Sem API pública nova: o `Preparar` e a `Autorizacao` já existiam; muda o que a escrita faz com `tpEmis` 9. Fecha a fase 5 no emulador.

### 17.1. As decisões

1. **O cálculo está no leiaute, sem mudar o núcleo.** O `infNFe` é escrito, e só então se calcula o que a forma offline precisa; o `infNFeSupl` entra no texto antes do `</NFe>`, sem mexer num byte do `infNFe`, e o núcleo assina depois (a `Signature` vem depois do grupo, como na online). O `digVal` é o resumo da assinatura: calculado por `SignedXml` com a referência *enveloped* e C14N sobre o `Id`, a mesma da `AssinaturaDigital.Assinar`, com uma chave HMAC descartável no lugar do certificado (o resumo do `infNFe` não depende da chave). A assinatura da versão 3 é `RSA.SignData(UTF-8 dos sete primeiros parâmetros, SHA-1, PKCS#1 v1.5)`, em Base64, com a chave do certificado da configuração.
2. **A versão segue a configuração:** `ConfiguracaoNFCe()` ou nenhuma, versão 3; `ConfiguracaoNFCe(csc)`, versão 2 (a NF-e com `tpImp` 6 é sempre a 3, e a 672 acusa o CSC configurado).
3. **As quatro formas** (Manual 4.3 e 4.4): versão 2 offline `chave|2|tpAmb|dia|vNF|digVal|idCSC|hash`; versão 3 offline `chave|3|tpAmb|dia|vNF|tipo|documento|assinatura`. O `dia` é o dia do mês de `dhEmi`, 2 dígitos, no fuso da nota; o `vNF` é o texto do `vNF` da nota; o destinatário é 1 e o CNPJ, 2 e o CPF, 3 e vazio (estrangeiro), os dois vazios sem destinatário.
4. **`tpEmis` 9 deixa de ser erro de montagem** (era o `NFE-QRCODE` da 5.1, decisão 9): a NFC-e, e a NF-e com `tpImp` 6, geram o `infNFeSupl` offline.
5. **As regras fora do schema** (`REGRAS.md` 16): B22-10 (711), B28-10 (556) e B28-20 (557). Não entram a 558 e a 569 (facultativas, só da SEFAZ).
6. **Sem fila e sem relógio de prazo na lib** (`PROJETO.md` 9.2).
7. **A transmissão depois é a `Autorizacao` normal**, síncrona ou em lote, com a consulta do recibo: o destino sai da chave (`tpEmis` 9 vai ao autorizador normal da UF). Nenhum código novo.
8. **O guia `docs/CONTINGENCIA.md`.**

### 17.2. Implementação (2026-10-24)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **O `digVal` é o texto do `DigestValue` em Base64, cada caractere em hexadecimal** (56 caracteres), e não o resumo bruto de 20 bytes em hexadecimal (40): é o que o Manual 4.3.5, passo 1 ("Converter o valor do DigestValue da NFC-e para HEXADECIMAL") manda, como o emulador o lê desde a E.3. O arquivo da etapa dizia só "em hexadecimal". **O manual não foi relido por inteiro nesta etapa** (o portal respondeu com redirecionamentos em laço ao download direto, e a segunda tentativa, com cookie e agente de navegador, foi barrada pelo controle de permissão, então o arquivo foi descartado); a decisão repousa na leitura da E.3 e da 5.1, e fica a conferir.
- **O certificado entra na escrita pelo `ValoresNucleo.Certificado`** (interno, preenchido pelo `Criar` a partir de `ConfiguracaoDFe.Certificado`); nulo fora do núcleo (os testes da escrita). A versão 3 offline sem ele, ou sem chave privada RSA, é `NFE-QRCODE`; a versão 2 não precisa dele.
- **O `NFE-QRCODE` da contingência offline deixou de existir** como tal: a offline continua dando `NFE-QRCODE` na UF sem URL e na versão 3 sem certificado.
- **As regras 556 e 557 usam o valor que a escrita grava:** o `TipoEmissao`, o `InicioContingencia` e a `JustificativaContingencia` da nota ou, nulos, os do núcleo. Como o `Criar` exige a `Contingencia` com emissão de contingência e a recusa com emissão normal, as duas só acusam a nota do modelo que contradiz o núcleo (e o próprio `Preparar` acusa também o `NFE-NUCLEO`).
- **Testes antigos mudados por consequência da etapa:** `EscritaQrCodeTestes.AContingenciaOfflineEErroNFEQrCodeAteA52` e `ANFeSimplificadaEmContingenciaOfflineTambemEErro` (a offline era erro até a 5.2) saíram, cobertas por `EscritaOfflineTestes`; o `CadaRegraTemOSeuCaso` soma `RegrasContingenciaTestes`.
- **Testes novos:** `EscritaOfflineTestes` (as duas formas, os dois ambientes, o dia, o `vNF`, os destinatários, o `digVal` igual ao `DigestValue` da assinatura, a assinatura da versão 3 com `RSA.VerifyData`, o schema depois de assinado, os problemas), `PreparoOfflineTestes` (o `Preparar` offline, o `Criar` sem a `Contingencia`), `RegrasContingenciaTestes` e `NFCeContingenciaEmuladaTestes` (a lib inteira contra o emulador).

## 18. Os eventos e a inutilização (etapa 6.1)

**Aprovado em 2026-10-24, todas as decisões pela recomendação, com as duas regras da decisão 9** (`docs/etapas/6.1-eventos-base-cancelamento-inutilizacao.md`). API pública nova só no leiaute (`OpenDFe.NFe`); o núcleo não mudou. A 6.2 (substituição e carta de correção) e a 8.2 (manifestação) acrescentam subclasses de `Evento` **sem mudar** o `EnvioEvento` nem a `OperacaoEvento`.

### 18.1. A API

```csharp
public abstract class Evento                         // base fechada: construtor private protected e membros internos
{
    public ChaveAcesso Chave { get; }                // chNFe
    public TipoEvento Tipo { get; }                  // tpEvento
    public int Sequencia { get; }                    // nSeqEvento (1 no cancelamento)
    public DateTimeOffset? Momento { get; init; }    // dhEvento; nulo = o relógio da configuração, em -03:00
}

public enum TipoEvento { CartaCorrecao = 110110, Cancelamento = 110111, CancelamentoSubstituicao = 110112,
    ConfirmacaoOperacao = 210200, CienciaOperacao = 210210, DesconhecimentoOperacao = 210220, OperacaoNaoRealizada = 210240 }

public sealed class Cancelamento : Evento
{
    public Cancelamento(ChaveAcesso chave, string protocolo, string justificativa);   // nProt de 15 dígitos
    public string Protocolo { get; }
    public string Justificativa { get; }
}

public sealed class EnvioEvento : ParametroWS<RetornoEvento>
{
    public const int MaximoEventos = 20;
    public EnvioEvento(params IEnumerable<Evento> eventos);
    public IReadOnlyList<Evento> Eventos { get; }
    public long? Lote { get; init; }                 // idLote; nulo = do relógio, em UTC (AAMMDDhhmmssmmm)
}

public sealed class RetornoEvento       { Status, Motivo, Ambiente?, VersaoAplicativo?, Orgao?, Lote?, IReadOnlyList<ResultadoEvento> Eventos }
public sealed class ResultadoEvento     { Evento?, Chave?, Status, Motivo, Descricao?, Protocolo?, Registro?, XmlEvento?, XmlProcessado?, Registrado }

public sealed class Inutilizacao : ParametroWS<RetornoInutilizacao>
{
    public Inutilizacao(string cnpj, int serie, int numeroInicial, int numeroFinal, string justificativa);
    public int? Ano { get; init; }                   // 2 dígitos; nulo = o ano do relógio, no fuso de Brasília
}
public sealed class RetornoInutilizacao { Status, Motivo, Ambiente?, VersaoAplicativo?, UF?, Ano?, Cnpj?, Modelo?, Serie?, NumeroInicial?, NumeroFinal?, Recebimento?, Protocolo?, XmlProcessado?, Homologada }
```

Registrados na NF-e e na NFC-e. O tipo da substituição é **`CancelamentoSubstituicao`** (o `PROJETO.md` escrevia `CancelamentoPorSubstituicao`, anterior à regra de nomes; o texto foi atualizado).

### 18.2. Exemplo de uso

```csharp
var nota = resultadoAutorizacao.Retorno.Notas[0];
var evento = new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, "Cancelamento por erro de digitacao") { Momento = agora };
var resultado = await dfe.ExecutarAsync(new EnvioEvento(evento));
var cancelado = resultado.Retorno!.Eventos[0];       // Registrado: 135 (136 e 155 também)
var distribuivel = cancelado.XmlProcessado;          // o procEventoNFe

var inutilizacao = await dfe.ExecutarAsync(new Inutilizacao("11.222.333/0001-81", 1, 10, 15, "Salto de numeracao"));
var proc = inutilizacao.Retorno!.XmlProcessado;      // o ProcInutNFe (102)
```

### 18.3. Decisões

1. **Onde ficam os tipos:** no leiaute. O núcleo já bastava (assinatura por `Id`, `IdsAssinatura`, `Documentos`, `Destino` e `Emissao` do `PedidoWS`).
2. **A hierarquia, aberta só para a lib:** o `Evento` tem membros internos que cada tipo implementa (a descrição, o autor, o schema do `detEvento`, os textos livres, a escrita do detalhe, a propriedade de cada campo, o órgão, o destino, `AceitaContingencia`); a `OperacaoEvento` não conhece o tipo.
3. **O envio, o retorno e o resultado por evento** como na API acima. `Registrado` é 135, 136 ou 155 (MOC 7.0, Visão Geral, 5.8.5 e 5.9.4, conferidos).
4. **A montagem:** `envEvento` 1.00, cada `evento` com `infEvento Id = "ID" + tpEvento + chave + nSeqEvento(2)`, `cOrgao` (a UF da chave), `tpAmb`, `CNPJ` ou `CPF` do autor (o emitente da chave; o CPF sem os três zeros), `chNFe`, `dhEvento`, `tpEvento`, `nSeqEvento`, `verEvento` 1.00 e o `detEvento`. Cada `infEvento` é assinado (a `Signature` é filha do `evento`, depois do `infEvento`). O `detEvento` é conferido na montagem pelo schema do tipo (`e110111`), com o código **493** e a propriedade (`Eventos[0].Justificativa`).
5. **O destino e o tipo de emissão** saem da chave (a UF dela e, só no cancelamento, o `tpEmis`: a SVC registra só o 110111, das notas que ela autorizou). Os outros tipos vão ao autorizador normal com o tipo de emissão normal. O `Destino` do parâmetro sobrepõe, no normal.
6. **O `procEventoNFe`:** o `evento` com os bytes enviados e o `retEvento` relido pelo `XElement` (equivalente, não byte a byte), válido no `procEventoNFe_v1.00.xsd`.
7. **A leitura do retorno é branda:** os `retEvento` ligados aos eventos pela chave, o tipo e a sequência; sem esses campos, pela posição quando a quantidade é a mesma; o que sobra vira `ResultadoEvento` com `Evento` nulo e o aviso `NFE-EVENTO`. Lote rejeitado (`cStat` diferente de 128) devolve `Eventos` vazio.
8. **A inutilização:** `inutNFe` 4.00 com `infInut Id = "ID" + cUF + ano + CNPJ + mod + série(3) + início(9) + fim(9)`, `xServ` `INUTILIZAR`, o modelo do núcleo; sempre ao autorizador normal da UF do núcleo, no tipo de emissão normal. O emitente com CPF não tem inutilização neste schema (`infInut` só tem CNPJ): pendente.
9. **Regras fora do schema (aprovadas, `REGRAS.md` 17):** `NFE-CHAVE` (chave de outro modelo que o do núcleo) e `NFE-LOTE` (destinos ou tipos de emissão diferentes no mesmo envio, ou o mesmo evento repetido). Nenhuma outra: prazo (501), sequência, titular (213, 227, 574) e situação da nota ficam para a SEFAZ.
10. **NF-e e NFC-e:** as duas operações são registradas nos dois documentos, com o mesmo código.
11. **Sem automatismo** (`PROJETO.md` 8.6): a queda no meio de um cancelamento se resolve pela `ConsultaProtocolo`; o guia está em `docs/QUANDO_CAI.md` 9.
12. **`RetornoProtocolo.Eventos` continua texto:** a leitura tipada do evento registrado não está no escopo da fase 6; quem usa lê o `procEventoNFe` pelo XML (versão menor futura).

### 18.4. Implementação (etapa 6.1, 2026-10-24)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **O construtor do `Evento` recebe a chave, o tipo e a sequência** (`private protected Evento(ChaveAcesso, TipoEvento, int)`), em vez de sem parâmetros: as propriedades são só de leitura e a base valida a chave nula.
- **`Evento.Textos`** (interno): os textos livres do detalhe, para a lib conferir os caracteres que o XML não aceita (`NFE-CARACTERE`, o código que a escrita da nota já usa) antes de escrever, sem que o `XmlWriter` lance exceção.
- **A `RegrasNFe400.Conferir` ignora o `Evento`** (o modelo do `DocumentoPedido` do evento): as regras da nota não se aplicam, e os callbacks de quem usa recebem o evento (`DocumentoVerificado.Modelo`).
- **O mapa tag→propriedade do `inutNFe`** passou a valer no `SchemasNFe400.Inutilizacao` (o problema do schema da justificativa sai com `Justificativa`); o `Id` de dois eventos iguais entra uma vez só nos documentos do pedido (como a nota repetida da 4.4).
- **`SituacaoNFe.ConferirModelo`** ganhou o parâmetro `operacao` (o texto da mensagem: "a consulta" ou "o evento").
- **O `ProcInutNFe`** tem o P maiúsculo (o nome do elemento no `procInutNFe_v4.00.xsd`).
- **Teste antigo mudado por consequência da etapa:** `LeiauteNFe400Testes.ADeclaracaoTemAsFormasDeLeituraEAsOperacoesDoDocumento` (a lista das operações ganha o `EnvioEvento` e a `Inutilizacao`).
- **Testes novos:** `AutorizacaoTestes.Evento` (montagem, recusas, destino e leitura, com o handler falso; um evento de teste derivado da base prova a extensão sem mudar o envio nem a operação), `EventoEmuladoTestes` e `InutilizacaoEmuladaTestes` (a lib inteira contra o emulador, com o relógio avançado nas 24 horas).

## 18.5. A substituição e a carta de correção (etapa 6.2)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/6.2-substituicao-carta-correcao.md`). Duas subclasses de `Evento`, **sem mudar** o `EnvioEvento`, a `OperacaoEvento`, o `RetornoEvento` nem o `ResultadoEvento`. O guia para quem usa é `docs/EVENTOS.md`.

```csharp
/// O cancelamento por substituição (110112) de uma NFC-e: cancela a NFC-e e aponta a que a substitui.
public sealed class CancelamentoSubstituicao : Evento
{
    public CancelamentoSubstituicao(ChaveAcesso chave, string protocolo, string justificativa, ChaveAcesso chaveSubstituta);
    public string Protocolo { get; }
    public string Justificativa { get; }
    public ChaveAcesso ChaveSubstituta { get; }          // chNFeRef
    public string VersaoAplicativo { get; init; }        // verAplic; padrão "OpenDFe " + a versão do pacote; cortado em 20
}

/// A carta de correção (110110) de uma NF-e; a NFC-e não tem.
public sealed class CartaCorrecao : Evento
{
    public CartaCorrecao(ChaveAcesso chave, int sequencia, string correcao);   // sequência de 1 a 20
    public string Correcao { get; }                      // xCorrecao
}
```

**O `detEvento`:**

- **Substituição:** `descEvento` "Cancelamento por substituicao" (como o schema enumera), `cOrgaoAutor` (a UF da chave), `tpAutor` 1 (empresa emitente), `verAplic`, `nProt`, `xJust`, `chNFeRef`. Sequência sempre 1; autor, o emitente da chave. Vai ao autorizador normal da UF da chave (o tipo não aceita a SVC: só o cancelamento comum aceita). O `cOrgaoAutor` que a SEFAZ devolve no `retEvento` fica no `XmlProcessado`; o `ResultadoEvento` não ganhou o campo.
- **Carta:** `descEvento` "Carta de Correcao", `xCorrecao` e `xCondUso` com o texto exato do `e110110_v1.00.xsd` (a forma sem acento; o schema aceita as duas). Um teste lê o XSD embutido e confere o texto com a enumeração (se uma NT mudar o texto, o teste acusa). Vai ao autorizador normal da UF (a SVC não registra CC-e), mesmo para a chave de uma nota da SVC.
- A `Sequencia` da carta é de quem usa (a lib não sabe qual é a próxima): o guia explica (conte as cartas da consulta). O tamanho da correção (15 a 1000) e o da justificativa (15 a 255) são do schema (493, com a propriedade).
- **Sem regra nova fora do schema** (`REGRAS.md` 18). As recusas dos construtores são validação de argumento: o modelo da chave (65 na substituição, as duas chaves; 55 na carta), as chaves iguais, os 15 dígitos do protocolo, a justificativa e a correção vazias, a sequência fora de 1 a 20.
- **O `procEventoNFe`** da substituição e o da carta passam no `procEventoNFe_v1.00.xsd`. O `retConsSitNFe_v4.00` do PL_010d diverge no caso da substituição (o `TProcEvento` dele não tem o `cOrgaoAutor`: `SEFAZ_EMULADA.md`, "Divergência entre os schemas do Portal"): a lib não valida a resposta da consulta pelo schema e a documentação o registra.

### 18.5.1. Implementação (etapa 6.2, 2026-10-24)

Como no desenho, com estes ajustes (a conferir pelo usuário):

- **A substituta é a NFC-e emitida offline (`tpEmis` 9), não a da emissão normal.** O arquivo da etapa dizia que o 920 e o 921 "dizem que a substituição é da emissão normal"; o MOC 7.0, Visão Geral, 5.9.3 (Tabela 5-38, conferido e registrado em `NTS.md` 5) diz outra coisa: o **920** rejeita a **cancelada** que não é da emissão normal (P12-48) e o **921** rejeita a **substituta** que é da emissão normal (5P31-44: "Tipo de emissão da NF-e substituta deve ser de contingência"). O emulador já fazia assim, a lib não confere nenhum dos dois (é da SEFAZ), e os testes e o guia usam a substituta offline. O destino da substituição é o autorizador normal da UF da chave da cancelada (que é da emissão normal).
- **`Evento.Autor` deixou de ser abstrato:** o autor do cancelamento, da substituição e da carta é o mesmo (o emitente da chave), então a implementação passou à base (`internal virtual`), e o `Cancelamento` a perdeu. A manifestação (8.2) a sobrescreve (o destinatário). O `EventoFalso` do teste da 6.1 continua a sobrescrever.
- **`CancelamentoSubstituicao.VersaoAplicativo`:** o nulo volta ao padrão e o que passa de 20 caracteres é cortado (o schema pede de 1 a 20); a versão padrão vem do `AssemblyInformationalVersion` sem o "+commit".
- **A base da 6.1 bastou:** a `OperacaoEvento`, o `EnvioEvento` e os resultados não mudaram (provado pelas duas subclasses, contra o handler falso e contra o emulador).
- **Testes novos:** `AutorizacaoTestes.EventoSubstituicaoCorrecao` (20), `SubstituicaoEmuladaTestes` (13) e `CartaCorrecaoEmuladaTestes` (7); `ApoioEmulado.Autorizar` (a nota do caso, também offline); dois casos pulados em `AutorizacaoHomologacaoTestes`.

## 19. O pacote de PDF: QuestPDF, a página de teste e os blocos gráficos (etapa 7.1)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/7.1-questpdf.md`). O pacote `OpenDFe.Impressao.Pdf` ganha o QuestPDF e o QRCoder, três blocos gráficos internos e **um único membro público**, o diagnóstico. O modelo de impressão (7.2), o DANFE (7.3) e o renderizador do modelo (7.4) vêm depois.

```csharp
namespace OpenDFe.Impressao.Pdf;

/// O renderizador PDF do modelo de impressão (7.4). Nesta versão, só o diagnóstico.
public static class RenderizadorPdf
{
    /// Uma página A4 com texto acentuado, um Code 128, um QR Code e uma imagem de 1 bit.
    /// InvalidOperationException: a licença do QuestPDF não foi definida (a mensagem aponta o README do pacote).
    public static byte[] GerarPaginaTeste();
}
```

**Licença.** O pacote **não define** `QuestPDF.Settings.License` (estado global, `PROJETO.md` 2.3 e 10.2). A classe interna `LicencaQuestPdf.Exigir()` confere `Settings.License` e, se for nula, lança `InvalidOperationException` com o motivo, o código (`QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;`) e o endereço do README do pacote no GitHub. Ela é o ponto único que os outros pontos de entrada da 7.4 chamam. O README (`src/OpenDFe.Impressao.Pdf/README.md`, empacotado) tem a licença no primeiro bloco, com o texto conferido em 2026-10-24 (`NTS.md` 6).

**Os blocos gráficos** (internos; `InternalsVisibleTo` do projeto de teste). Todos entregam **SVG vetorial** ao QuestPDF por `.Svg(string)`, em unidades de módulo ou pixel (`viewBox` de uma unidade por módulo), com `shape-rendering="crispEdges"` e **sem borda**: a zona de silêncio (4 módulos no QR Code, 10 no Code 128) é desenhada por quem usa.

- `CodigoBarras128`: Code 128 conjunto C, escrito pela lib. A tabela dos 107 padrões (larguras de barra e espaço), o início C (105), os pares de dígitos, o verificador `(105 + Σ valor × posição) mod 103` e o fim (106, 7 elementos). Recusa (`ArgumentException`) texto vazio, caractere que não é `0` a `9` e quantidade ímpar. A chave de 44 dígitos dá 25 símbolos e 24 × 11 + 13 = 277 módulos. `Simbolos`, `Larguras` (a sequência de barra, espaço, barra...) e `Gerar` (um `path` com um retângulo por barra).
- `QrCodeSvg`: o QRCoder gera a matriz (`ECCLevel.M`, `forceUtf8: true`: nível M e UTF-8, como o manual do DANFE NFC-e manda); a lib tira a zona de silêncio de 4 módulos que o QRCoder acrescenta e desenha o SVG.
- `ImagemBit` e `ImagemBitSvg`: a representação interna da imagem de 1 bit (largura, altura, bits empacotados por linha, o mais significativo primeiro, 1 é preto; os bits que sobram no fim da linha são ignorados), que a 7.2 expõe no modelo neutro. O SVG é um `path` com uma corrida horizontal de pixels pretos por segmento (`M x y h n v1 h-n z`); a imagem toda branca não tem `path`. O `QrCodeSvg` usa o mesmo desenho.

**A página de teste.** `Document.Create` com uma página A4 (margem de 2 cm), a fonte embutida padrão do QuestPDF (Lato, que cobre o português; **nenhuma fonte é registrada**: o `FontManager` é global), o texto acentuado, o Code 128 de uma chave de formato válido (8 x 2 cm), o QR Code de uma URL de NFC-e fictícia (4 x 4 cm) e uma imagem de 20 x 20 pixels (20 não é múltiplo de 8). O `Svg` do QuestPDF 2026.9.1 renderiza esses SVGs como vetor (conferido com o PdfPig: três caminhos, um por elemento, o do código de barras com 76 subcaminhos, nenhum bitmap) e **mantém a proporção** do SVG dentro da caixa dada (o código de barras de 277 x 50 numa caixa de 8 x 2 cm sai com 8 x 1,44 cm): quem usar a 7.4 dimensiona pela proporção.

**Pacotes.** `QuestPDF` 2026.9.1 e `QRCoder` 1.8.0 em `Directory.Packages.props` (a última estável de cada uma com pelo menos 14 dias, `NTS.md` 6); `PdfPig` 0.1.16 só no projeto de teste. O `eng/ConferirPacotes.ps1` passa a esperar `OpenDFe.Nucleo`, `QRCoder` e `QuestPDF` como dependências do `OpenDFe.Impressao.Pdf`, e o `OpenDFe.Nucleo` e o `OpenDFe.NFe` continuam sem nenhuma delas.

**Implementação (ajustes ao desenho).**

- **A licença do QuestPDF não é MIT.** O arquivo da etapa e o `PROJETO.md` 10.2 diziam "comunitária (MIT)"; a licença vigente (versão 3.0, de 6 de julho de 2026) é "source-available". O README, o `NTS.md` 6 e o `PROJETO.md` 10.2 trazem o texto conferido (limite de USD 1.000.000 de receita bruta anual, projetos open source, e os que ficam de fora: órgãos públicos e empresas de capital aberto). Ponto para o usuário conferir: a categoria de **dependência transitiva** não vale se a biblioteca intermediária for basicamente um invólucro do QuestPDF; o `OpenDFe.Impressao.Pdf` tem função própria (o DANFE e o DANFCE), mas a fronteira é do texto da licença, e o README manda cada um ler.
- `LicencaQuestPdf.Exigir` tem uma sobrecarga com o `LicenseType?` informado (a de sem argumento lê `Settings.License`), para provar a mensagem **sem mexer no estado global**; o teste do `GerarPaginaTeste` sem licença mexe nele numa coleção sem paralelismo e restaura no `finally`.
- Nomes: `Gerar` e `Criar` no lugar de `ParaSvg` e `De` (a regra de nomes sem preposição).
- O `README.md` do pacote substitui o da raiz no pacote (o `src/Directory.Build.props` empacota o da raiz; o csproj dele o remove).
- Os testes do projeto novo: `CodigoBarras128Testes`, `QrCodeSvgTestes`, `ImagemBitSvgTestes`, `LicencaQuestPdfTestes`, `RenderizadorPdfTestes` (com o PdfPig).


## 20. O modelo de impressão neutro e o DANFCE (etapa 7.2)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/7.2-modelo-impressao-danfce.md`; o número sugerido no arquivo era o 21, o próximo livre era o 20). O modelo de impressão mora no **núcleo** (`src/OpenDFe.Nucleo/Impressao/`, namespace `OpenDFe.Impressao`) e **não conhece NF-e**; o DANFCE mora no **leiaute** (`src/OpenDFe.Leiaute.NFe.v400/Impressao/`, namespace `OpenDFe.NFe`). A conformidade contra o Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0 está em `docs/IMPRESSAO.md` 1. Esta etapa não gera PDF nem ESC/POS: o PDV e a 7.4 consomem o mesmo `DocumentoImpressao`.

### 20.1 O modelo (núcleo)

Tudo imutável, público, sem lógica de renderização, seguro em paralelo; os construtores validam (`ArgumentException`, e `ArgumentNullException`/`ArgumentOutOfRangeException`, que derivam dela) e **copiam as listas** passadas.

```csharp
namespace OpenDFe.Impressao;

public sealed class DocumentoImpressao                  // o papel e os elementos, de cima para baixo
{
    public DocumentoImpressao(PapelImpressao papel, IReadOnlyList<ElementoImpressao> elementos);
    public PapelImpressao Papel { get; }  public IReadOnlyList<ElementoImpressao> Elementos { get; }
    public string? Titulo { get; init; }                 // metadado (título do PDF)
}

public sealed class PapelImpressao                       // só as fábricas; construtor privado
{
    public static PapelImpressao Bobina80 { get; }       // 80 mm, 48 colunas, margem de 4 mm (72 mm impressos = 576 pontos a 203 dpi)
    public static PapelImpressao Bobina58 { get; }       // 58 mm, 32 colunas, margem de 5 mm (48 mm impressos)
    public static PapelImpressao A4Retrato { get; }  public static PapelImpressao A4Paisagem { get; }   // margem de 8 mm (era 10 mm até a 7.2; o Anexo II 3.6.2 do DANFE permite de 2 a 8 mm, CONTRATOS.md 21)
    public TipoPapel Tipo { get; }  public double LarguraMm { get; }  public double? AlturaMm { get; }
    public int Colunas { get; }  public double MargemMm { get; }
    public int ColunasTamanho(TamanhoTexto tamanho);     // normal = Colunas; pequeno = Colunas * 4 / 3 (fonte B); duplo = Colunas / 2
}

public abstract class ElementoImpressao { private protected ElementoImpressao(); }   // conjunto fechado

public sealed class TextoImpressao : ElementoImpressao   // Texto, Alinhamento, Negrito, Tamanho (Pequeno, Normal, Duplo), Invertido
{
    public TextoImpressao(string texto);
    public IReadOnlyList<string> Quebrar(int colunas);   // em espaço; a palavra maior que a linha é cortada; \n e linha vazia mantidos
}
public sealed class LinhaImpressao : ElementoImpressao   // células com peso
{
    public LinhaImpressao(IReadOnlyList<CelulaImpressao> celulas);
    public IReadOnlyList<int> Larguras(int colunas);     // soma exatamente `colunas`
}
public sealed class CelulaImpressao { public CelulaImpressao(string texto, double peso = 1); /* Texto, Peso, Alinhamento, Negrito, Quebrar(colunas) */ }
public sealed class SeparadorImpressao : ElementoImpressao { /* Estilo: Tracejado (padrão), Cheio */ }
public sealed class EspacoImpressao : ElementoImpressao { public EspacoImpressao(int linhas); }
public sealed class QrCodeImpressao : ElementoImpressao       // Texto, LadoMm (25 mm padrão, o total com a margem de silêncio), Alinhamento (centro)
public sealed class CodigoBarrasImpressao : ElementoImpressao // Code 128 C: só dígitos, quantidade par; Digitos, AlturaMm (10)
public sealed class Imagem1Bit                                // Largura, Altura, Bits (MSB primeiro, linha em bytes inteiros, 1 = preto), Preto(x, y), Limiarizar(cinza, largura, altura, limiar = 128)
public sealed class ImagemImpressao : ElementoImpressao       // Imagem, Alinhamento (centro), LarguraMm (nula = natural a 203 dpi)
public sealed class SecaoImpressao : ElementoImpressao        // DANFE (7.3): Titulo e LinhaCamposImpressao[], cada uma com CampoImpressao (Rotulo, Valor, Peso, Alinhamento, Negrito)
public sealed class TabelaImpressao : ElementoImpressao       // DANFE (7.3): Titulo, ColunaImpressao[] (Titulo, Peso, Alinhamento) e as linhas (um texto por coluna)
public sealed class QuebraPaginaImpressao : ElementoImpressao
```

Decisões do arquivo da etapa, cumpridas: (1) o modelo é do núcleo e não conhece NF-e; (2) o texto é sempre `string` já formatada; (3) é estrutural, sem fonte nem coordenada em pixel; (4) o conjunto de elementos é fechado por `private protected` (testado por reflexão); (5) `LinhaImpressao.Larguras` e `TextoImpressao.Quebrar` existem para o PDV não reescrevê-los; (6) `Imagem1Bit` confere o tamanho dos bits (`ceil(largura / 8) * altura`) e **copia** os bits.

**`Larguras(colunas)`**: o método do maior resto (cada célula recebe a parte inteira da sua cota; as colunas que sobram vão às maiores frações, a célula da esquerda nos empates). Com `colunas` maior ou igual ao número de células, **nenhuma fica com zero** (a coluna que falta sai da célula mais larga); com menos, as de menor peso podem ficar com zero. A soma é sempre `colunas` (propriedade provada com pesos aleatórios de semente fixa, de 1 a 100 colunas).

### 20.2 O DANFCE (leiaute)

```csharp
namespace OpenDFe.NFe;

public enum ViaDanfce { Consumidor, Estabelecimento }

public sealed class OpcoesDanfce
{
    public PapelImpressao Papel { get; init; } = PapelImpressao.Bobina80;   // Bobina80 ou Bobina58; outro papel → ArgumentException
    public Imagem1Bit? Logotipo { get; init; }            // no topo
    public Imagem1Bit? AssinaturaSoftware { get; init; }  // no fim, depois do rodapé
    public string? TextoRodape { get; init; }             // uma linha livre depois do conteúdo da NFC-e
    public ViaDanfce Via { get; init; } = ViaDanfce.Consumidor;
}

public sealed class LeiauteNFe400
{
    public DocumentoImpressao Danfce(NotaLida nota, OpcoesDanfce? opcoes = null);
}
```

`Danfce` lê a `NotaLida` (o `nfeProc` autorizado ou o `NFe` assinado em contingência, sem protocolo) e **não** confere o schema nem as regras do MOC. `ArgumentException` (a mensagem cita o campo, nunca o valor): a leitura sem a nota; a nota que não é NFC-e (modelo 65, pelo `ide/mod` ou, na falta, pela chave); sem a chave; sem o `qrCode` ou sem a `urlChave` das informações suplementares; o protocolo de situação que não é 100 nem 150; o papel que não é a bobina de 80 ou de 58 mm; a via fora do enum.

**A ordem do documento** (a da figura 1B do manual, QR Code centralizado): o logotipo; o cabeçalho (CNPJ ou CPF e nome do emitente, o endereço sem o país, "Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica"); a legenda da contingência; os itens; os totais; as formas de pagamento e o troco; a consulta pela chave (o texto, a URL da nota e a chave em 11 blocos de 4); o consumidor; a identificação (número, série, data e hora de emissão e a via); o protocolo e a data da autorização; a legenda da contingência de novo; o QR Code; as mensagens (o `infAdFisco`, o texto de homologação, o `infCpl`, a Lei 12.741, o rodapé); a assinatura do software. As exigências, uma a uma, estão em `docs/IMPRESSAO.md` 1.

**O QR Code do documento é o `Suplementares.QrCode` da nota, igual byte a byte** (provado nos testes e contra a NFC-e produzida pela lib e aceita pelo emulador). O lado é de 30 mm (o manual pede ao menos 25 mm no total, com a margem de silêncio dentro dele).

**A contingência** (`tpEmis` 9 **sem** protocolo): "EMITIDA EM CONTINGÊNCIA" (em negrito, em tamanho duplo quando cabe na bobina) e "Pendente de autorização", centralizados, **nos dois lugares** do manual (abaixo do cabeçalho e abaixo da identificação), sem o protocolo, e a linha "Via do Consumidor" ou "Via do Estabelecimento". **A lib não decide quantas vias imprimir:** quem imprime chama `Danfce` uma vez por via (`OpcoesDanfce.Via`). Com o protocolo (a offline já autorizada), a legenda some e o protocolo sai.

**Formatação** (`FormatadorImpressao`, interno; não `FormatoImpressao`, que é o enum do `tpImp`): `NumberFormatInfo` explícito, nada de `CultureInfo("pt-BR")` nem da cultura do sistema (provado trocando a cultura da thread). Dinheiro `1.234,56`; a quantidade sem zeros sobrando à direita (`2`, `2,5`, `1,235`, `1.000`), com até 10 casas e sem arredondar; o valor unitário com ao menos 2 casas e até 10; a data `dd/MM/yyyy HH:mm:ss` **como a nota traz, sem converter o fuso**; CNPJ (também o alfanumérico) `00.000.000/0000-00`, CPF `000.000.000-00`, CEP `00.000-000` (o do manual, figura 2).

### 20.3 Implementação (ajustes ao desenho)

- **O manual vence o arquivo da etapa**, nos pontos em que diferem: o texto do cabeçalho é só "Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica" (o 6.0 não traz "DANFE NFC-e - ..." nem "Não permite aproveitamento de crédito de ICMS"); o nome fantasia não é pedido (não sai); o "Valor a Pagar" só sai quando há acréscimo ou desconto (o `vNF` difere do `vProd`); o troco só quando maior que zero; os acréscimos saem um por linha ("Desconto R$", "Frete R$", "Seguro R$", "Outras despesas R$"), como a figura 2; a "Via do Estabelecimento" só na contingência ou quando a via pedida é a do estabelecimento (a emissão normal não imprime via). A lista de partida do arquivo cedeu em todos eles.
- **A data e a hora não são convertidas.** O manual diz "convertida para o horário local" porque, na época, o `dhEmi` era UTC; no leiaute 4.00 o `dhEmi` e o `dhRecbto` já trazem o horário local do emitente e da SEFAZ, com o deslocamento (`-03:00`), e o arquivo da etapa manda não converter. O DANFCE imprime os campos de relógio do próprio valor (provado com `-03:00`, `+09:00` e `Z`).
- **`PapelImpressao.ColunasTamanho(TamanhoTexto)`** (novo; o arquivo não o previa): o texto pequeno e o duplo têm outras colunas, e o renderizador de texto precisa delas para quebrar. **`CelulaImpressao.Quebrar`** (novo): a mesma quebra do texto, para a célula.
- As margens do papel (4 mm na bobina de 80 e 5 mm na de 58) saem da área impressa da térmica (72 e 48 mm, 8 pontos por mm a 203 dpi); o manual pede no mínimo 2 mm.
- O logotipo sai **centralizado, acima** do cabeçalho: o modelo empilha elementos e não compõe imagem ao lado de texto (o manual o põe no canto esquerdo, "a critério do emissor"). A 7.4 pode compor os dois elementos consecutivos como quiser.
- Os itens saem em **duas linhas** (o código e a descrição; depois `quantidade unidade x unitário` com o total à direita), com o cabeçalho "Código Descrição" e "Qtde UN x Vl Unit / Vl Total": o manual não regula a posição (3.1.2) e as cinco colunas não cabem na bobina de 58 mm (32 colunas).
- A homologação sai depois do `infAdFisco` e antes do `infCpl` (a divisão VIII do manual), em negrito, centralizada, em tamanho pequeno (cabe em uma linha na bobina de 80 mm); o hífen é o ASCII (`-`): o PDV deve manter assim, porque a tabela de caracteres de várias térmicas não tem o travessão.
- **Os testes do modelo** estão em `Nucleo.Testes` (`Impressao/`) e os do DANFCE em `NFe.Testes` (`Impressao/`): o renderizador de texto (`TextoPlano`) é **só de teste** e marca cada linha com `B` (negrito), `P` ou `D` (pequeno, duplo) e `I` (invertido).

## 21. O DANFE da NF-e e os acréscimos ao modelo de impressão (etapa 7.3)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/7.3-danfe.md`; o número sugerido no arquivo era o 22 e o próximo livre era o 21). O DANFE mora no **leiaute** (`src/OpenDFe.Leiaute.NFe.v400/Impressao/`, namespace `OpenDFe.NFe`) e monta um `DocumentoImpressao` do modelo neutro da 7.2; os acréscimos ao modelo moram no **núcleo** (`src/OpenDFe.Nucleo/Impressao/`, `OpenDFe.Impressao`). A conformidade contra o Anexo II do MOC 7.0 e as NTs está em `docs/IMPRESSAO.md` 2 (o checklist, quadro a quadro). A etapa não gera PDF (7.4).

### 21.1 Os acréscimos ao modelo neutro (núcleo)

Todos opcionais para quem renderiza: um renderizador que os ignora ainda gera um documento válido. Documentados e testados em `Nucleo.Testes` (`PaginacaoImpressaoTestes`).

```csharp
namespace OpenDFe.Impressao;

public sealed class DocumentoImpressao        // + 2 propriedades init
{
    public IReadOnlyList<ElementoImpressao> CabecalhoContinuacao { get; init; }   // o que se repete no topo das páginas seguintes (copiada; vazia por padrão)
    public string? MarcaAgua { get; init; }                                       // o texto em diagonal em todas as páginas (null sem marca; vazio é recusado)
}

public static class TextoPaginado             // as fichas de paginação e a troca
{
    public const string Pagina = "{pagina}";  public const string Paginas = "{paginas}";
    public static string Resolver(string texto, int pagina, int paginas);         // Pagina e Paginas trocadas, em algarismos sem cultura; ArgumentOutOfRange fora da faixa
}

public sealed class TabelaImpressao           // + 2 propriedades init
{
    public bool LinhasIndivisiveis { get; init; }   // a linha nunca se divide entre duas páginas (o item do DANFE, Anexo II 3.1.7)
    public bool DivisorLinhas { get; init; }        // o "destaque divisório" entre uma linha e a seguinte (3.1.7)
}
```

- **As fichas** `{pagina}` e `{paginas}` valem no texto de um `TextoImpressao`, no valor de um `CampoImpressao` e no texto de uma célula, incluindo o `CabecalhoContinuacao`: o DANFE escreve `FOLHA {pagina}/{paginas}` e quem renderiza chama `TextoPaginado.Resolver` com os números de cada página, depois de cortar o documento. `Resolver` não depende da cultura (provado com `ar-SA` e `fa-IR`, que trocam os algarismos).
- **A quebra de linha** (o caractere de nova linha) no texto de uma célula da tabela ou no valor de um campo é uma quebra de linha dentro da célula ou do campo (o DANFE põe a descrição, as informações adicionais e os dados do grupo H do item em linhas da mesma célula).
- **A tabela já repetia o cabeçalho** a cada página (7.2); `LinhasIndivisiveis` e `DivisorLinhas` são o que o Anexo II 3.1.7 pede a mais, e o DANFE liga os dois.
- **A margem das folhas A4 passou de 10 para 8 mm** (`PapelImpressao.A4Retrato` e `A4Paisagem`): o Anexo II 3.6.2 permite de 0,2 a 0,8 cm em cada lado do DANFE, e 10 mm excedia o máximo. Mudança de valor, sem mudança de assinatura; nenhum teste dependia dos 10 mm.

### 21.2 O DANFE (leiaute)

```csharp
namespace OpenDFe.NFe;

public enum OrientacaoDanfe { Retrato, Paisagem }

public sealed class OpcoesDanfe
{
    public Imagem1Bit? Logotipo { get; init; }            // no quadro do emitente
    public bool Canhoto { get; init; } = true;            // só o retrato o suprime (Anexo II 3.3.1); na paisagem sai sempre
    public OrientacaoDanfe? Orientacao { get; init; }     // null = pelo tpImp da nota (1 retrato, 2 paisagem)
    public bool Cancelada { get; init; }                  // marca d'água "CANCELADA" (a situação vem de quem usa)
    public string? TextoRodape { get; init; }             // uma linha livre ao fim, ex.: "Impresso por ERP Core"
    public PapelImpressao? Papel { get; init; }           // só do Tipo 2: Bobina80 (padrão), Bobina58 ou A4Retrato
    public ViaDanfce Via { get; init; }                   // só do Tipo 2 em contingência (a via do DANFCE)
}

public sealed class LeiauteNFe400
{
    public DocumentoImpressao Danfe(NotaLida nota, OpcoesDanfe? opcoes = null);
}
```

`Danfe` lê a `NotaLida` (o `nfeProc` autorizado, ou o `NFe` assinado em contingência, sem protocolo) e **não** confere o schema nem as regras do MOC; é sem estado e seguro em paralelo. `ArgumentException` (a mensagem cita o campo, nunca o valor): a leitura sem a nota ou sem a chave; a nota que não é NF-e (modelo 55); o protocolo de situação que não é 100 nem 150; o `tpImp` 0, 3 (**não implementado**, `IMPRESSAO.md` 2.3), 4 ou 5, ou ausente sem a orientação nas opções; a orientação fora do enum; o papel nas opções no retrato e na paisagem; a orientação no Tipo 2; o papel do Tipo 2 que não é a bobina ou o A4 em pé; a via fora do enum; o Tipo 2 sem o `qrCode` e a `urlChave`; o CNPJ não numérico do destinatário na contingência com formulário de segurança (o código de barras dos Dados da NF-e só tem dígitos).

**O `tpImp` da nota escolhe o documento:** 1 (retrato) e 2 (paisagem) são o A4 do Anexo II, com a orientação das opções por cima do `tpImp`; 6 é o DANFE Simplificado Tipo 2, no leiaute do DANFCE (`MontagemDanfce`, parametrizado: o cabeçalho "DANFE Simplificado Tipo 2", o "NF-e nº" e a divisão III-A dos novos tributos), em bobina.

### 21.3 A ordem do documento (retrato e paisagem)

A do Anexo II, com os quadros opcionais só quando a nota os tem: o **canhoto** (o texto "RECEBEMOS DE ...", a data de recebimento, a assinatura e o número da nota; o primeiro elemento; só o retrato o suprime); a **identificação** (o logotipo; o emitente com endereço e telefone; "DANFE" e "DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA"; o tipo de operação com o dígito do `tpNF`, o número, a série e a folha; a legenda da contingência; o código de barras da chave e a chave em 11 blocos; o código de barras dos Dados da NF-e na contingência com formulário de segurança, ou a consulta de autenticidade e o protocolo; a natureza da operação; a IE, a IE do substituto e o CNPJ ou CPF; o CRT) que é também o **`CabecalhoContinuacao`**; o destinatário ou remetente; os locais de retirada e de entrega; a fatura e as duplicatas; o cálculo do imposto; o total do IBS/CBS/IS; o transportador e os volumes; a **tabela** dos produtos e serviços (17 colunas do Anexo II e, havendo IBS, CBS ou IS em algum item, mais 3 da NT 2026.010); o cálculo do ISSQN; os dados adicionais (informações complementares e reservado ao Fisco); o QR Code, se a nota o tem; o rodapé. O documento leva o `Titulo` `DANFE NF-e 000000123 série 001` e a `MarcaAgua` (`CANCELADA`, ou `SEM VALOR FISCAL` em homologação).

**A tabela dos itens:** uma linha por item, uma célula por coluna, a descrição com as informações do item em linhas da mesma célula (o nome, o EAN, a unidade tributável quando difere da comercial, o `infAdProd`, os lotes, o medicamento, o combustível, o veículo, a arma, a classificação tributária); o `O/CST` é a origem seguida do CST de 2 dígitos ou do CSOSN de 3 (`000`, `0102`); quantidade e valor unitário com ao menos 2 casas e até 10, os valores com 2, os pontos de milhar e a vírgula decimal sem a cultura do sistema.

### 21.4 Implementação (ajustes ao desenho)

- **Escopo do `tpImp`:** o arquivo da etapa mandava o 3 e o 6 "se o leiaute estiver nas fontes". O **6 está** (a NT 2026.003, com as figuras e os exemplos) e foi implementado; o **3 (e a Etiqueta, que também é `tpImp` 3) não está**: o Anexo II e a NT 2020.004 só dão os campos mínimos, sem ordem nem dimensões (`IMPRESSAO.md` 2.3). Recusa com `ArgumentException`.
- **`OpcoesDanfe.Papel` e `OpcoesDanfe.Via`** (novos, o arquivo não os previa): o Tipo 2 é bobina e tem duas vias na contingência (NT 2026.003, 3.1.9), como o DANFCE.
- **O Anexo II venceu a NT 2026.010 onde as duas divergem** (a ordem dos quadros e as colunas dos itens); os acréscimos da NT entram quando o XML tem o dado (`NTS.md` 8, `IMPRESSAO.md` 2.1).
- **Margem do A4:** 10 mm passou a 8 mm (21.1).
- **`Montagem` do Tipo 2:** o `MontagemDanfce` ganhou o parâmetro do Tipo 2 e `Largura(papel)` (o A4 não tem colunas e usa as da bobina de 80 mm nas divisões); o DANFCE da NFC-e não mudou (os 22 golden da 7.2 passam sem alteração).
- **O renderizador de texto de teste** (`TextoPlano`) foi estendido para as seções, as tabelas, o código de barras, a `MarcaAgua` e o `CabecalhoContinuacao` (com as fichas trocadas por `1/1` no corpo e `2/2` no cabeçalho de continuação), sem tocar nos 22 golden do DANFCE.
- **O número do `CONTRATOS.md` é 21** (o arquivo da etapa dizia 22; o 20 é o da 7.2 e o 21 era o próximo livre).

## 22. O renderizador PDF (etapa 7.4)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/7.4-pdf.md`; o número sugerido no arquivo era o 23, mas o próximo livre era o 22 (a 7.3 usou o 21 e não houve 22)). O `OpenDFe.Impressao.Pdf` passa a **renderizar o modelo de impressão neutro** (qualquer `DocumentoImpressao`: o DANFCE da 7.2 e o DANFE da 7.3) em PDF com o QuestPDF 2026.9.1. O renderizador só **desenha**: não toma decisão fiscal e não mexe no conteúdo. A conformidade contra o Manual do DANFE NFC-e 6.0 e o Anexo II do MOC 7.0 está em `docs/IMPRESSAO.md` 3, e o guia do ESC/POS para o PDV, em `docs/IMPRESSAO.md` 4.

### 22.1 A API pública

```csharp
namespace OpenDFe.Impressao.Pdf;

public static partial class RenderizadorPdf
{
    public static byte[] Gerar(DocumentoImpressao documento, OpcoesPdf? opcoes = null);
    public static void Gerar(DocumentoImpressao documento, Stream destino, OpcoesPdf? opcoes = null);
    public static byte[] GerarPaginaTeste();                       // a 7.1
}

public sealed class OpcoesPdf
{
    public string? Titulo { get; init; }       // padrão: DocumentoImpressao.Titulo
    public string? Autor { get; init; }
    public string Criador { get; init; } = "OpenDFe";
    public string? Fonte { get; init; }        // a família registrada pelo aplicativo; null = a Lato embutida (22.8)
}
```

Síncrona, sem estado e **segura em paralelo** (provado com 16 gerações simultâneas de documentos diferentes, texto igual ao da sequencial); nenhuma configuração global do QuestPDF é tocada (a licença é do aplicativo, README). `ArgumentNullException` com o documento (ou o fluxo) nulo; `InvalidOperationException` com a licença não definida, pela mesma `LicencaQuestPdf.Exigir` da página de teste, com o endereço do README (as duas sobrecargas). O fluxo não é fechado nem reposicionado. As duas sobrecargas de `Gerar` têm o parâmetro opcional, como a decisão 1 manda; o analisador RS0026 reclama de sobrecargas opcionais, e a supressão está comentada no código (elas diferem no tipo do segundo argumento e a API ainda é `Unshipped`).

### 22.2 O mapeamento do modelo para o PDF

Determinístico, um arquivo por família (`RenderizadorPdf.Texto.cs`, `.Quadros.cs`, `.Tabela.cs`, `.Graficos.cs`, `.Documento.cs`).

| Elemento do modelo | O PDF |
|---|---|
| **Papel** `Bobina` | página com a **largura do papel** (80 ou 58 mm) e **altura contínua** (`ContinuousSize`: cresce com o conteúdo), margem lateral do papel (4 e 5 mm) e vertical de 3 mm |
| **Papel** `Folha` | a página com as dimensões do papel (A4 em pé, 210 x 297 mm, ou deitado) e a margem dele (8 mm), paginada |
| `TextoImpressao` | um bloco de texto: alinhamento, negrito, tamanho (22.3) e **invertido** (fundo preto, letra branca); a quebra de linha do texto vira linha; o texto longo **quebra**, nunca é cortado; a linha vazia é uma linha em branco |
| `LinhaImpressao` | uma fileira: cada célula com a largura proporcional ao **peso** (a do `Larguras` em colunas, aqui em proporção direta); **a última célula alinhada à direita toma a largura do conteúdo** e a de texto cede o resto: a fonte do PDF não é de largura fixa como a da térmica, e o peso 13/48 de "VALOR PAGO R$" não comportaria o texto (22.8) |
| `SeparadorImpressao` | linha horizontal na largura útil: `Cheio` contínua de 1 pt, `Tracejado` de 0,6 pt em traços de 2,5 pt |
| `EspacoImpressao` | altura de `linhas` linhas do texto normal (tamanho x 1,15) |
| `QrCodeImpressao` | vetor (SVG) do QR Code do QRCoder, **nível M, UTF-8**, no `Alinhamento`; o `LadoMm` é o **lado total, com a zona de silêncio dentro** (22.5) |
| `CodigoBarrasImpressao` | vetor do Code 128 conjunto C, as barras com a largura do contêiner **até 11,5 cm** e a altura `AlturaMm`, e a margem clara de 10 módulos de cada lado (22.5) |
| `ImagemImpressao` | vetor da imagem de 1 bit, na `LarguraMm` ou no tamanho natural a 203 dpi (nunca maior que a linha), no `Alinhamento` |
| `SecaoImpressao` | o título do bloco (5 pt, negrito) e, por `LinhaCamposImpressao`, uma **tabela de uma fileira** de quadros de borda fina com o rótulo pequeno em cima e o valor embaixo; os quadros de uma fileira têm **a mesma altura** (o que quebra em linhas estica os vizinhos); o quadro maior que a página continua na seguinte |
| `TabelaImpressao` | o título do bloco e a tabela do QuestPDF com as colunas na proporção dos pesos, o **cabeçalho repetido** a cada página, a linha com o texto das células (quebra de linha da célula = linha), e, com `LinhasIndivisiveis`, a linha que **não se divide** entre duas páginas; `DivisorLinhas` escurece a linha entre os itens |
| `QuebraPaginaImpressao` | na folha, uma quebra de página; na bobina, o **corte**: cada trecho é um bilhete (uma página de altura contínua) |
| `DocumentoImpressao.CabecalhoContinuacao` | no topo de cada página **depois da primeira** (`ShowIf(PageNumber > 1)`); a primeira não reserva o espaço dele |
| `DocumentoImpressao.MarcaAgua` | texto em negrito, cinza `#E6E6E6`, a 45 graus, centrado, atrás do conteúdo, **em todas as páginas**; o tamanho (até 64 pt) é o que põe o texto na largura útil |
| fichas `{pagina}` e `{paginas}` | o número da página e o total do QuestPDF (`CurrentPageNumber` e `TotalPages`), em qualquer texto, valor de campo ou célula, inclusive no cabeçalho de continuação (`FOLHA 2/3`) |
| `DocumentoImpressao.Titulo` e `OpcoesPdf` | título, autor e criador do PDF; o idioma é `pt-BR` |

Documento sem elemento sai com uma página em branco (o PDF precisa de ao menos uma).

### 22.3 As fontes e as medidas (constantes em `Medidas.cs`, cada uma com o item do manual)

**Bobina** (o Manual do DANFE NFC-e 6.0 não fixa fonte nem posição, 3.1.2): texto normal de **8 pt**, pequeno de **6 pt** (3/4, como a fonte B da térmica) e duplo de **12 pt** (a térmica dobra a 16 pt, mas a Lato em negrito não põe a frase de 23 letras da contingência numa linha de 72 mm a 16 pt; a 12 pt põe).

**Folha** (Anexo II do MOC 7.0, 3.7): o título do bloco de **5 pt** em negrito (3.7.1); o título das colunas dos itens de **5 pt** (3.7.2); o rótulo do campo de **6 pt** (3.7.3); o conteúdo dos itens e das informações complementares de **6 pt** (3.7.7 e 3.7.8); o conteúdo dos demais campos de **10 pt** (3.7.9); o texto normal de **8 pt** ("DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA" e os dados do emitente, 3.7.4 e 3.7.6); o duplo de **12 pt** ("DANFE", 3.7.4); o pequeno de **6 pt**. As **informações complementares** são o único campo de texto livre: o modelo não o distingue dos demais, e o renderizador o reconhece pelo rótulo do Anexo II (`INFORMAÇÕES COMPLEMENTARES`) para dar 6 pt (os 5.000 caracteres a 10 pt ocupariam três folhas).

A entrelinha é 1,15 do tamanho; a borda dos quadros, de 0,5 pt; o respiro do quadro, 0,8 mm (0,25 mm nas células das colunas estreitas da tabela).

### 22.4 Os caracteres

O QuestPDF lança se faltar o glifo (`CheckIfAllTextGlyphsAreAvailable` fica no padrão, sem mexer em configuração global). A emissão **nunca** pode quebrar por causa de um caractere no nome do produto, então todo texto passa por `TextoSeguro.Limpar` antes de ir ao PDF (o texto, os campos, as células, os títulos, os metadados e a marca d'água):

- a Lato embutida **desenha** (conferido caractere a caractere, desenhando cada um em normal e em negrito): o ASCII (U+0020 a U+007E), o **Latin-1 inteiro** (U+00A0 a U+00FF: o `TString` do schema da NF-e só admite até U+00FF) e o Latin Extended-A e B (até U+024F), mais U+2010 a U+2022 (hifens, travessões, aspas, adagas, marcador), a reticência, o por mil, as aspas angulares simples, o euro, a marca registrada e o sinal de menos: **553 caracteres**, que saem sem troca (provado com `ç ã õ é ê í ó ú â ô à º ª § ½ ² ³ – — “ ” € ° ' " & < >` e os demais);
- o que a fonte **não** tem e tem equivalente ASCII é trocado: o marcador triangular e o hífen de marcador por `-`, o ponto de hifenização por `.`, a barra de fração por `/`, os espaços especiais (de U+2000 a U+200A, U+202F, U+205F e U+3000) por espaço, os separadores de linha e de parágrafo por quebra de linha;
- os caracteres de formatação invisíveis (espaço de largura zero, marcas de direção, a marca de ordem de bytes) são **tirados**, e os controles viram espaço;
- o resto (ideogramas, emoji, grego, setas, surrogado solto) vira `?` (os fora do plano básico, um `?` por caractere).

### 22.5 O QR Code e o código de barras

**O lado do QR Code é o total, com a zona de silêncio dentro** (Manual do DANFE NFC-e 3.1.5 e 3.4: "no mínimo 25 mm x 25 mm, sendo 22 mm de conteúdo para 3 mm de margem segura (quiet zone); acima de 25 mm, a margem de 10% da dimensão total"). O DANFCE pede 30 mm (7.2): o símbolo ocupa o lado menos as duas margens. A margem é **a maior** entre a do manual (3 mm até 25 mm; 10% do lado acima) e os **4 módulos** do ISO/IEC 18004 (o `MargemQrCode`): para a versão 3 (29 módulos) em 30 mm, o manual dá 3,0 mm e os 4 módulos dão 3,24 mm, que valem. Nenhum dos dois pode ser menor.

**O código de barras:** as barras com a largura do contêiner, **limitada a 11,5 cm** (a largura do código de barras da chave no Anexo II 3.8.1; a decisão da etapa dizia "a do contêiner", que num A4 em paisagem daria 28 cm de barras), a altura pedida (`AlturaMm`, 10 mm: o Anexo II manda no mínimo 8 mm) e a **margem clara de 10 módulos** de cada lado, fora das barras (Anexo II 2). O módulo (a barra mais fina) sai de 115 mm / 277 = 0,415 mm, acima dos 0,2 mm do Anexo II. Na bobina, o conjunto cabe nos 72 mm: o módulo é 67,1 / 277 = 0,24 mm. As **277 posições** são 22 pares de 11 módulos, o início C (11), o verificador (11) e o fim (13). O SVG sai na proporção exata da caixa (o QuestPDF mantém a proporção do SVG; o `CodigoBarras128.Gerar` passou a aceitar a altura em unidades fracionárias).

### 22.6 A paginação

A **folha** pagina pelo QuestPDF: a `Column` do conteúdo divide entre os elementos, e dentro deles a tabela divide entre as linhas (com o cabeçalho repetido) e o quadro grande divide dentro da célula. O quadro de seção usa `EnsureSpace` (o título não fica sozinho no pé de uma página) em vez de `ShowEntire`, que lança erro com o quadro maior que a página (as informações complementares de 5.000 caracteres). A continuação das informações complementares é **na folha seguinte**, nunca "no verso" (Anexo II 3.4 e 3.5). A **bobina** é de altura contínua, sem paginar.

### 22.7 Onde estão os testes

- `OpenDFe.Impressao.Pdf.Testes` (PdfPig): `RenderizadorPdfElementosTestes` (cada elemento, o papel, a paginação, os metadados, a concorrência e a licença) e `TextoSeguroTestes` (os caracteres), mais os `PdfLeitor` e `PdfVetores` (a leitura do PDF e as matrizes do QR Code e do código de barras).
- `OpenDFe.NFe.Testes`: `PdfDocumentosTestes` (os DANFCE e os DANFE do `CatalogoImpressao`, os mesmos casos dos golden de texto, mais 300 itens, descrição de 500 caracteres e as informações complementares de 5.000) e `PdfEmuladoTestes` (a nota autorizada pelo emulador, lida e impressa). **Os documentos completos e a ponta a ponta moram no `NFe.Testes`**, onde as notas dos golden e a infraestrutura do emulador já estão, sem duplicar: o projeto referencia o `OpenDFe.Impressao.Pdf` (o PdfPig e o QRCoder só nos testes) e **compartilha dois arquivos** de teste do `Impressao.Pdf.Testes` (`PdfLeitor.cs` e `PdfVetores.cs`, `Compile Include` por link); o `Impressao.Pdf` ganhou o `InternalsVisibleTo` do `OpenDFe.NFe.Testes` para o teste do código de barras usar as larguras do Code 128 como referência.
- Para ver o PDF: `OPENDFE_AMOSTRAS=<pasta>` grava o PDF de cada caso na pasta (fora do repositório), para abrir e conferir a olho; sem a variável não grava nada.

### 22.8 Implementação (ajustes ao desenho)

- **`OpcoesPdf.Fonte`** (novo; a decisão 1 só tinha título, autor e criador): o Anexo II 3.7 pede Times New Roman ou Courier New, que a lib não embute, e o QuestPDF só desenha em família registrada. O aplicativo que quer uma delas (ou a Liberation Serif, de mesmas métricas) a registra e passa o nome. **Família não registrada cai na Lato**, para a emissão não quebrar (o QuestPDF lança com uma família que não existe).
- **O código de barras até 11,5 cm** (22.5), e não a largura do contêiner.
- **O QR Code com a margem máxima** entre o manual e os 4 módulos (22.5): a decisão deixava a escolha ("pelo manual").
- **O texto duplo da bobina em 12 pt**, e não 16 (22.3).
- **A última célula à direita de uma `LinhaImpressao` toma a largura do conteúdo** (22.2): a fonte não é de largura fixa.
- **`QuebraPaginaImpressao` na bobina corta em dois bilhetes** (um por trecho), em vez de ignorar: é o corte da térmica, e o `Danfce` nunca o usa.
- **As informações complementares em 6 pt**, reconhecidas pelo rótulo (22.3).
- **Os campos da seção em tabela de uma fileira** (22.2): a `Row` do QuestPDF não iguala a altura das células; a `Table` iguala.
- **`CodigoBarras128.Gerar`** (interno) passou a aceitar a altura `double`; os testes da 7.1 não mudaram.
- **`InternalsVisibleTo`** do `OpenDFe.NFe.Testes` e a supressão do RS0026 (22.1).
- **Lacunas do renderizador** (o que o Anexo II pede e o modelo ou o renderizador não faz) estão em `docs/IMPRESSAO.md` 3.4: o quadro de identificação não é composto lado a lado, o nome do emitente sai em 8 pt (não em 12 pt), o canhoto da paisagem sai na horizontal, e a fonte padrão é a Lato.

## 23. A distribuição DF-e (etapa 8.1)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/8.1-distribuicao-dfe.md`; o número sugerido no arquivo era o 24, mas o próximo livre era o 23, porque a 7.4 usou o 22). O `LeiauteNFe400` passa a consultar o `NFeDistribuicaoDFe` do Ambiente Nacional (NT 2014.002 v1.40, MOC 7.0, Visão Geral, 5.7) por último NSU, por NSU e por chave, e devolve os documentos já descompactados. A NFC-e não é distribuída: a operação só é registrada para a NF-e. O guia de uso, com o laço de sincronização, é o `docs/DISTRIBUICAO.md`.

### 23.1 O núcleo: `ServicoSoap.Operacao`

O `nfeDadosMsg` da distribuição vai **dentro** de um elemento da operação, e a resposta vem em `<operação>Response/<operação>Result`; os outros serviços põem o `nfeDadosMsg` direto no `Body` e leem o `nfeResultMsg` (`docs/NTS.md` 3.2, decisão da 4.2). A mudança no núcleo é uma propriedade só:

```csharp
public sealed class ServicoSoap
{
    // ...Nome, NamespaceWsdl e Acao como estavam (2.4)
    /// O nome da operação, nos serviços cujo Body a tem como elemento ("nfeDistDFeInteresse"). Quando não nulo, o
    /// nfeDadosMsg vai dentro de <Operacao xmlns="namespaceWsdl">, e a resposta é lida em <Operacao>Response/<Operacao>Result,
    /// em vez do nfeResultMsg direto no Body. Nulo (o padrão): o envelope dos outros serviços, sem mudança.
    /// ArgumentException: vazio, só espaços ou fora de [A-Za-z0-9_] (e não começa por algarismo).
    public string? Operacao { get; init; }
}
```

- **`EnvelopeSoap.Montar(servico, mensagem)`** escreve `<Operacao xmlns="..."><nfeDadosMsg xmlns="...">mensagem</nfeDadosMsg></Operacao>` quando há operação; sem ela, o texto é o mesmo de antes, **byte a byte** (teste de regressão, `EnvelopeSoapTestes.SemOperacaoOEnvelopeNaoMudaUmByte`, com o texto completo do envelope do status). O `xmlns` do elemento da operação é o do WSDL, e o `nfeDadosMsg` o repete (como no exemplo da NT, item 5).
- **`EnvelopeSoap.Ler(resposta, servico)`** procura o `<Operacao>Result` em qualquer profundidade e em qualquer namespace do `Body` (a mesma tolerância do `nfeResultMsg`); com operação, o `nfeResultMsg` direto não serve, e sem operação o `<Operacao>Result` não serve (`InvalidDataException`, resposta ilegível). A falha SOAP sai como antes. O parâmetro é opcional (`ServicoSoap? servico = null`): quem não passa lê o `nfeResultMsg`, como antes (o `EnvelopeSoap` é interno; o `TransporteSoap` passa o `envio.Soap`).
- Entra no `PublicAPI.Unshipped.txt` do núcleo (`ServicoSoap.Operacao.get` e `.init`) e é coberto por `EnvelopeSoapTestes` (o envelope com e sem operação, a leitura em três formas de resposta, a recusa do `nfeResultMsg` direto, o resultado vazio, a falha SOAP e a validação do nome) e por `TransporteSoapTestes` (o Kestrel local com TLS mútuo, com um serviço de operação declarado pelo `Acrescimo` do leiaute falso: o corpo, a ação e a leitura do `Result`; e a resposta no `nfeResultMsg` direto, que é ilegível).

### 23.2 A declaração no leiaute

Só na NF-e (`declaracao.Documento == Documento.NFe`), em `AutorizadoresNFe400`:

| O quê | Valor |
|---|---|
| serviço | `NFeDistribuicaoDFe`, namespace `http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe` |
| ação | `http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe/nfeDistDFeInteresse` |
| `Operacao` | `nfeDistDFeInteresse` |
| URL de produção (AN) | `https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx` |
| URL de homologação (AN) | `https://hom1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx` |

As duas URLs foram **conferidas na "Relação de Serviços Web" do Portal Nacional, em 2026-10-24** (produção e homologação, seção "Ambiente Nacional", serviço `NFeDistribuicaoDFe`, versão 1.00 do serviço; os SHA-256 das páginas baixadas estão em `docs/NTS.md` 3). O `distDFeInt_v1.01.xsd` entra em `SchemasNFe400.DistribuicaoPedido` (código 215), do pacote `PL_010f_v1.04` já embutido (NTS.md 1): nada novo no pacote de schemas. O `AN` só tinha o `NFeRecepcaoEvento4`; ganha a distribuição, e o `Atendimento` não muda (o pedido leva o `Destino.AN`, que não é UF).

### 23.3 O parâmetro

```csharp
namespace OpenDFe.NFe;

/// A consulta à distribuição de DF-e do Ambiente Nacional. Só na NF-e. Sempre ao AN.
public sealed class DistribuicaoDFe : ParametroWS<RetornoDistribuicao>
{
    public static DistribuicaoDFe UltimoNsu(string interessado, long ultimoNsu = 0, UF? ufAutor = null);
    public static DistribuicaoDFe Nsu(string interessado, long nsu, UF? ufAutor = null);
    public static DistribuicaoDFe Chave(string interessado, ChaveAcesso chave, UF? ufAutor = null);

    public string Interessado { get; }                    // o CNPJ ou o CPF, sem máscara
    public TipoConsultaDistribuicao Consulta { get; }      // UltimoNsu, Nsu, Chave
    public long? NsuConsultado { get; }                    // o ultNSU ou o NSU; nulo na chave
    public ChaveAcesso? ChaveConsultada { get; }
    public UF? UFAutor { get; }                            // cUFAutor; nulo = a UF do núcleo
}
public enum TipoConsultaDistribuicao { UltimoNsu = 0, Nsu = 1, Chave = 2 }
```

- **O `interessado`** aceita a máscara e é validado com `Cnpj.Valido` (numérico ou alfanumérico) e, se não for, com `Cpf.Valido`; vai ao XML sem máscara, como `<CNPJ>` ou `<CPF>`. Nem um nem outro: `ArgumentException` (a mensagem não traz o valor). O CNPJ alfanumérico entra como veio (maiúsculas): a NT 2014.002 v1.40 e o pacote 1.04 aceitam.
- **O NSU** de 0 a 999999999999999 (15 algarismos): negativo ou acima, `ArgumentOutOfRangeException` (subclasse de `ArgumentException`); vai ao XML com 15 algarismos e zeros à esquerda (`000000000000123`), como o `TNSU` do schema exige.
- **O pedido:** `distDFeInt versao="1.01"` com `tpAmb` do núcleo, `cUFAutor` (a `UFAutor`, ou a UF do núcleo), o `CNPJ` ou o `CPF`, e a consulta (`distNSU/ultNSU`, `consNSU/NSU` ou `consChNFe/chNFe`). **Sem assinatura**, destino `Destino.AN`, tipo de emissão normal, schema `distDFeInt_v1.01.xsd` (215). A chave de outro modelo que o do núcleo (a de NFC-e) é recusada antes de transmitir (`NFE-CHAVE`, `RecusadoLocalmente`, o `SituacaoNFe.ConferirModelo` da consulta do protocolo).
- **Sem regra fora do schema** (decisão 6): os argumentos são validados nos factories, e o que o Ambiente Nacional rejeita sobre o interessado e a chave (489, 490, 593, 472, 236, 614 a 619, 640, 641) é da SEFAZ.

### 23.4 O retorno

```csharp
public sealed class RetornoDistribuicao                  // construtor interno; imutável
{
    public int Status { get; }                           // 137, 138, 656 e as rejeições
    public string Motivo { get; }
    public Ambiente? Ambiente { get; }   public string? VersaoAplicativo { get; }
    public DateTimeOffset? Resposta { get; }             // dhResp
    public long? UltimoNsu { get; }      public long? MaximoNsu { get; }
    public IReadOnlyList<DocumentoDistribuido> Documentos { get; }   // vazia sem documento
    public bool NenhumDocumento { get; }                 // 137
    public bool ConsumoIndevido { get; }                 // 656
    public bool TemMais { get; }                         // distNSU, 138, UltimoNsu < MaximoNsu (nunca no consNSU nem no consChNFe)
    public TimeSpan? EsperaSugerida { get; }             // 1 hora no distNSU sem mais o que entregar (137, ou 138 com UltimoNsu >= MaximoNsu) e no 656
}

public sealed class DocumentoDistribuido                 // construtor interno; imutável
{
    public long? Nsu { get; }                            // nulo na consulta por chave
    public string Schema { get; }                        // "resNFe_v1.01.xsd", "procNFe_v4.00.xsd", ...
    public TipoDocumentoDistribuido Tipo { get; }        // ResumoNFe, NFeProcessada, ResumoEvento, EventoProcessado, Outro
    public string Xml { get; }                           // descompactado (gzip), UTF-8, como veio, sem o BOM
    public ChaveAcesso? Chave { get; }
    public ResumoNFe? ResumoNFe { get; }                 // só no ResumoNFe
    public ResumoEvento? ResumoEvento { get; }           // só no ResumoEvento
}

public sealed class ResumoNFe      // Chave, Emitente, NomeEmitente, InscricaoEstadual, Emissao, TipoNota, ValorTotal, Digest, Recebimento, Protocolo, Situacao
public sealed class ResumoEvento   // Orgao, Autor, Chave, Momento, Tipo, Sequencia, Descricao, Recebimento, Protocolo
public enum SituacaoResumo { Autorizada = 1, Denegada = 2, Cancelada = 3 }   // cSitNFe, aberta
```

- **Os resumos nos tipos do modelo:** `dhEmi`, `dhRecbto` e `dhEvento` em `DateTimeOffset` com o fuso que veio; `vNF` em `decimal`; `tpNF` em `TipoNota`; `tpEvento` em `TipoEvento` (aberta: `(TipoEvento)999999` para o que a enumeração não nomeia); `cSitNFe` em `SituacaoResumo` (aberta). A `IE` vem como está (algarismos, `ISENTO` ou vazia: a NT, C07, aceita vazia, e o schema, `TIe`, não). `Emitente` e `Autor` são o CNPJ ou o CPF, sem máscara. O `digVal`, opcional no schema, é `Digest` (nulo se não veio).
- **Para a NF-e processada e o evento processado**, quem usa lê o `Xml` com `LeiauteNFe400.Ler` (a `NotaLida`) ou direto: **a leitura da nota não é duplicada aqui.** O `Chave` do `NFeProcessada` sai do `Id` do `infNFe` (ou do `chNFe` do protocolo); o do `EventoProcessado`, do `chNFe` do `evento` (ou do `retEvento`).
- **`TemMais` e `EsperaSugerida` só valem na distribuição por NSU** (ajuste ao desenho, 23.7): a consulta pontual responde um documento só, e o `UltimoNsu` dela é o NSU pedido, menor que o máximo; chamá-la de "há mais" levaria a um laço errado. O 656 vale sempre a hora. **A espera de 1 hora vale também no 138 cujo `UltimoNsu` alcançou o `MaximoNsu`** (NT 3.11.4.1, 2: "quando ultNSU for igual ao maxNSU, não existem mais documentos; para não haver bloqueio, deve-se aguardar 1 hora"): quem chama de novo na hora só recebe o 137, e o 137 já bloqueia a consulta seguinte.
- **A lib não repete a consulta, não espera e não guarda o NSU** (decisão 6, PROJETO.md 8.6).

### 23.5 A leitura branda

`LeituraRetorno` (decisão 4 do CONTRATOS.md 13): a raiz tem de ser `retDistDFeInt` no namespace da NF-e; sem `cStat` numérico ou sem `xMotivo`, a resposta é ilegível (`FormatException`, o handler do `Executar` a trata como `RespostaIlegivel`). O que não se lê é aviso, nunca recusa (a resposta já foi processada):

| Aviso | Quando | O que acontece |
|---|---|---|
| `NFE-VALOR` | o `ultNSU`, o `maxNSU`, o `dhResp`, o `tpAmb` ou o `NSU` do `docZip` não se leem | o campo fica nulo |
| `NFE-DISTRIBUICAO` | o `docZip` não é base64, não é gzip, passa de **16 MB** descompactado (bomba de descompressão), não é UTF-8 ou o XML não é bem formado (também com DTD, que é proibido); ou o `resNFe`/`resEvento` não se lê nos tipos | o documento sai da lista, **com o NSU na mensagem**, e os outros seguem; no resumo que não se lê, o documento fica com o `Xml` e o resumo nulo |
| `NFE-DESCONHECIDO`, `NFE-REPETIDO` | elemento fora do leiaute, ou repetido (`cStat`, `loteDistDFeInt`) | ignorado; vale o primeiro |

O `schema` desconhecido vira `Tipo.Outro` (sem aviso, com o `Xml`); o `docZip` sem `schema` também (com `NFE-DISTRIBUICAO`). Mais de 50 `docZip` não é erro (a NT limita a 50: aceita-se o que vier). `NFE-DISTRIBUICAO` está em `docs/LEITURA.md` 3.

### 23.6 Testes

- **`OperacaoDistribuicaoTestes`** (NFe.Testes, 81, handler falso): a NF-e tem a distribuição e a NFC-e não; o serviço, a ação, a operação e as duas URLs do AN; o pedido no schema nos três modos, com CNPJ (numérico, mascarado, alfanumérico) e CPF, com `cUFAutor` do núcleo e informada, com o `ultNSU` de 15 algarismos; o envelope com a operação; as recusas dos factories e da chave de NFC-e; a leitura de 137, 138 com os quatro tipos (gzip montado no teste), 656, 589, 593, 215 e 632; `TemMais` e `EsperaSugerida`; os resumos com todos os campos (`ISENTO`, IE vazia, CPF, sem `digVal`, situação aberta, evento aberto); a chave de cada tipo; o NSU ausente; o BOM e a declaração; o base64 com quebras de linha como na NT; o `docZip` com base64 inválido, não gzip, gzip cortado e corrompido, XML mal formado, vazio, DTD e não UTF-8, a bomba de 17 MB, o documento de 4 MB que passa, o resumo que não se lê, o NSU ilegível e o `schema` ausente; 55 documentos; a resposta ilegível e a sem o elemento da operação. Os XML de exemplo passam nos schemas do pacote (`resNFe`, `resEvento`, `retDistDFeInt`).
- **`DistribuicaoEmuladaTestes`** (NFe.Testes, 22, contra a SEFAZ emulada, com núcleos de CNPJ diferente, cada um com o seu certificado autoassinado): o destinatário B recebe o `resNFe` (NSU 1) lido nos tipos; o 137 na seguinte (com a espera de 1 hora) e o 656 na terceira; o `ultNSU` fora da sequência (656, com o `UltimoNsu` do servidor); o emitente não recebe a própria nota; o autorizado no `autXML` (que a lib escreve) recebe o `nfeProc`, lido com `LeiauteNFe400.Ler` e igual à nota autorizada; o cancelamento (6.1) como `resEvento` ao destinatário e como `procEventoNFe` ao autorizado; o `distNSU` acima do máximo (589, com o máximo dele); `consNSU` e `consChNFe` (138, 589, 217, 641, 640, 653, 632 com o relógio avançado 91 dias); o certificado de outro CNPJ base (593) e de CNPJ para CPF (472); a matriz que consulta a filial; o destinatário pessoa física pelo CPF; a 21ª consulta pontual (656); o serviço paralisado (108 e 109); a conexão derrubada depois de processar, a repetição que cai no 656 e a recuperação pelo `consNSU` depois da hora; a derrubada antes de processar e a repetição; 55 notas em dois lotes (50 e 5, com `TemMais`).
- **`DistribuicaoGuiaTestes`** (NFe.Testes, 2): o laço do `docs/DISTRIBUICAO.md` 7, exatamente como está no guia, contra a SEFAZ emulada: a sincronização de 55 notas em dois lotes, a hora de espera sem consultar (o último pedido do emulador não muda) e a retomada depois dela; a resposta perdida, o 656 da repetição e a lacuna fechada pelo `consNSU`.
- **Núcleo:** 16 testes novos (23.1). **Homologação real:** `DistribuicaoHomologacaoTestes`, a distribuição do CNPJ do PFX com o `ultNSU` 0, pulado sem o PFX.
- **Testes que mudaram** por consequência direta da etapa: `AutorizadoresNFe400Testes` (a lista de serviços da NF-e ganha o `NFeDistribuicaoDFe`; as URLs da NF-e passam de 190 para 192; o AN da NF-e tem o evento e a distribuição, que a asserção "a distribuição fica fora" proibia) e `LeiauteNFe400Testes` (as operações da NF-e ganham o `DistribuicaoDFe`).

### 23.7 Implementação

**Ajustes ao desenho** (a conferir pelo usuário):

- **O `UFAutor` é parâmetro opcional dos factories**, e não `init`: com construtor privado e factories estáticos, um `init` não tem como ser usado (não há `new DistribuicaoDFe { ... }` nem `with`). `UltimoNsu(interessado, ultimoNsu = 0, ufAutor = null)` e os outros dois; a propriedade `UFAutor` é só de leitura.
- **`TemMais` e `EsperaSugerida` só na distribuição por NSU** (23.4), e **a espera também no 138 que alcança o máximo**: o desenho os dava pelo status (a espera só no 137 e no 656); o 137 do `consNSU` (o NSU que o Ambiente Nacional não gerou) e do `consChNFe` (a NF-e que ele não tem) não bloqueiam o `distNSU` seguinte (NT 3.11.4.1: o bloqueio de 1 hora é do `distNSU`), e o 138 do `consNSU` com o `UltimoNsu` menor que o máximo não é "há mais"; e o último 138 do `distNSU` (o `UltimoNsu` igual ao `MaximoNsu`) já manda esperar a hora (NT 3.11.4.1, 2), em vez de deixar a chamada seguinte descobrir o 137. O 656 vale a hora em qualquer consulta (NT 3.11.4.2, observação).
- **`EnvelopeSoap.Ler(resposta, servico = null)`**: o desenho dava `Ler(resposta, servico)`; o parâmetro é opcional para os serviços sem operação e os testes de antes não mudarem.
- **`ServicoSoap.Operacao` recusa o nome que não é um nome XML simples** (letras, algarismos e sublinhado): ele é escrito no envelope sem escape.
- **O número da seção é o 23**, e não o 24 do arquivo da etapa.

**Decisões tomadas sem perguntar:**

- **O `docZip` sem `schema` fica na lista como `Outro`** (com `NFE-DISTRIBUICAO`) em vez de ser omitido: o conteúdo está lá, e omitir perderia um documento que o NSU já consumiu; o `schema` é obrigatório no XSD, então a ausência é do servidor.
- **O `resNFe` ou o `resEvento` que não se lê nos tipos** fica na lista, com o resumo nulo e o `Xml`, e `NFE-DISTRIBUICAO`: quem usa ainda tem o documento e o NSU.
- **O XML descompactado sai como texto UTF-8 sem o BOM** (o BOM, se vier, é tirado); a declaração XML dele fica como veio. Bytes que não são UTF-8 são `docZip` ilegível.
- **A `InscricaoEstadual` vazia no `resNFe` é aceita** (C07 da NT diz que vale vazio; o schema `TIe` não): a leitura segue a norma e o servidor real.
- **O `xNome` do `resNFe` com 2 posições** (o schema aceita de 2 a 60; a NT diz 3 a 60) é aceito.
- **O `dhEmi`/`dhRecbto` são lidos com o formato do `TDateTimeUTC`** (`LeituraRetorno.LerDataHora`): o que não casa deixa o resumo nulo.
- **A recomendação para a queda no meio** (o item 7 do arquivo da etapa pedia "confirme na NT e escreva a recomendação exata") está em `docs/DISTRIBUICAO.md` 6, conferida na NT 3.11.4.1 (o `ultNSU` do 656) e contra o emulador: a repetição com o mesmo `ultNSU` depois de uma resposta perdida **cai no 656** (o servidor já considera entregue o lote que se perdeu); o 656 devolve o `ultNSU` do servidor, e os NSU perdidos se recuperam um a um pelo `consNSU` depois da hora de espera. O emulador segue a NT; a confirmação com o Ambiente Nacional real fica para a homologação (pendente até o PFX).
- **Não se registra a distribuição na NFC-e** (a NT só trata a NF-e).

## 24. A manifestação do destinatário e a consulta cadastro (etapa 8.2)

**Aprovado em 2026-10-24, todas as decisões pela recomendação** (`docs/etapas/8.2-manifestacao-consulta-cadastro.md`; o número sugerido no arquivo era o 25, mas o próximo livre era o 24, porque a 8.1 usou o 23). O `LeiauteNFe400` passa a enviar os quatro eventos de manifestação do destinatário ao Ambiente Nacional, sobre a base de eventos da 6.1, e a consultar o `CadConsultaCadastro4` da UF. As duas coisas valem só para a NF-e.

### 24.1 A manifestação (NT 2020.001 v1.60, MOC 7.0, Visão Geral, 5.11)

```csharp
public abstract class Manifestacao : Evento          // construtor private protected
{
    public new string Autor { get; }                 // CNPJ (numérico ou alfanumérico) ou CPF do destinatário, só os algarismos e as letras
}
public sealed class CienciaOperacao : Manifestacao        { public CienciaOperacao(ChaveAcesso chave, string autor); }                                  // 210210, sequência 1
public sealed class ConfirmacaoOperacao : Manifestacao    { public ConfirmacaoOperacao(ChaveAcesso chave, string autor, int sequencia = 1); }           // 210200, 1 ou 2
public sealed class DesconhecimentoOperacao : Manifestacao{ public DesconhecimentoOperacao(ChaveAcesso chave, string autor, int sequencia = 1); }       // 210220, 1 ou 2
public sealed class OperacaoNaoRealizada : Manifestacao   { public OperacaoNaoRealizada(ChaveAcesso chave, string autor, string justificativa, int sequencia = 1); public string Justificativa { get; } } // 210240
```

- **Destino:** sempre o Ambiente Nacional (`Destino.AN`), `cOrgao` 91, tipo de emissão normal, qualquer que seja a chave (UF, SVC). É a mesma extensão interna que a 6.1 criou para o `AceitaContingencia` (`Orgao` e `Destino` sobrescritos em `Manifestacao`); `EnvioEvento` e `OperacaoEvento` **não mudaram**, e o `NFE-LOTE` da 6.1 já proíbe misturar manifestação (AN) com cancelamento (UF) no lote.
- **O autor** entra no `infEvento` como `CNPJ` ou `CPF`; o `Id` é `ID` + tipo + chave + sequência de 2 dígitos. O certificado do núcleo assina; a raiz do CNPJ ou o CPF do autor contra o certificado é conferência da SEFAZ.
- **O `detEvento`** leva o `descEvento` como o schema do tipo enumera (`Ciencia da Operacao`, `Confirmacao da Operacao`, `Desconhecimento da Operacao`, `Operacao nao Realizada`) e, na operação não realizada, o `xJust` (15 a 255, pelo schema do tipo, com o 493 e a propriedade `Eventos[k].Justificativa`). Os quatro schemas `e2102xx_v1.00.xsd` já estavam embutidos desde a E.5.
- **Construtores:** `ArgumentNullException` na chave nula; `ArgumentException` no autor que não é CNPJ nem CPF válido (a máscara é aceita e tirada), na chave que não é de NF-e (a NFC-e não tem manifestação) e na justificativa nula ou vazia; `ArgumentOutOfRangeException` na sequência fora de 1 a 2.
- **Resultado:** o `ResultadoEvento` da 6.1 (135 ou 136; `xEvento`; `XmlProcessado`, o `procEventoNFe`), sem campo novo.
- **Regra fora do schema (aprovada):** nenhuma nova. O `NFE-CHAVE` da 6.1 recusa a manifestação num núcleo de NFC-e. O prazo (596), a sequência (594), a ordem das manifestações (655), a nota cancelada (650, 651) e o destinatário (575) ficam para a SEFAZ.

### 24.2 A consulta cadastro (MOC 7.0, Visão Geral, 5.6, Tabela 5-22)

```csharp
public sealed class ConsultaCadastro : ParametroWS<RetornoCadastro>
{
    public ConsultaCadastro(UF uf, string documento);                          // CNPJ ou CPF, com ou sem máscara, DV conferido
    public static ConsultaCadastro Inscricao(UF uf, string inscricaoEstadual); // 2 a 14 algarismos, nem toda zero, sem máscara
    public UF UF { get; }  public string? Documento { get; }  public string? InscricaoEstadual { get; }
}
public sealed class RetornoCadastro { Status, Motivo, VersaoAplicativo, UF, UFAtendimento (cUF), Consulta (dhCons), Contribuintes, Encontrado /* 111 ou 112 */ }
public sealed class ContribuinteCadastro { InscricaoEstadual, Cnpj, Cpf, UF, Situacao, CredenciamentoNFe, CredenciamentoCTe, Nome, Fantasia, RegimeApuracao, Cnae,
                                           InicioAtividade, UltimaSituacao, Baixa (DateOnly?), InscricaoUnica, InscricaoAtual, Endereco }
public sealed class EnderecoCadastro { Logradouro, Numero, Complemento, Bairro, CodigoMunicipio, Municipio, Cep }
public enum SituacaoCadastral { NaoHabilitado = 0, Habilitado = 1 }
public enum CredenciamentoDFe { NaoCredenciado = 0, Credenciado = 1, ObrigatorioTotal = 2, ObrigatorioParcial = 3, NaoInformado = 4 }
```

- **O pedido:** `ConsCad` 2.00 com `infCons` (`xServ` `CONS-CAD`, `UF`, `IE` ou `CNPJ` ou `CPF`), sem assinatura e sem `tpAmb`, no `CadConsultaCadastro/consCad_v2.00.xsd` (215). Destino: o autorizador da UF consultada (`uf.ComoDestino()`, atendimento normal, qualquer que seja a UF ou a emissão do núcleo). A SUFRAMA (`SU`) não existe aqui.
- **A regra `NFE-CADASTRO` (aprovada, Erro):** a UF sem o serviço é recusada na montagem, sem `Destino` informado: só atendem as 10 UFs com autorizador próprio (AM, BA, GO, MG, MS, MT, PE, PR, RS, SP) e, na SVRS, AC, ES, RN, PB e SC (`AutorizadoresNFe400.TemCadastro`). O MA (SVAN) e as outras da SVRS são recusados (campo `UF`, caminho `ConsCad/infCons/UF`). Com o `Destino` informado a regra não vale. A lista é a de `docs/NTS.md` 3.2, conferida em 2026-10-21/22; **a "Relação de Serviços Web" não pôde ser reconferida em 2026-10-24** (o portal responde com redirecionamentos sem fim ao cliente da etapa), e a lista continua a anterior.
- **A leitura é branda** (`LeituraRetorno`): sem o `infCons`, o `cStat` ou o `xMotivo`, a resposta é ilegível (`FormatException`, vira `FalhaTransmissaoException`); campo que não se lê fica nulo com `NFE-VALOR`, campo que o leiaute não conhece é `NFE-DESCONHECIDO`, ambos aviso. A situação e o credenciamento são **enumerações abertas**: um número que o MOC não define entra como o número, com aviso `NFE-VALOR`. `IE` e `xNome` ausentes do `infCad` ficam vazios, com aviso.
- **Só NF-e:** a 4.2 listou a consulta cadastro na NFC-e em GO, MS e PR, mas a lib registra a operação só na NF-e (o PDV não consulta cadastro): na NFC-e, **não registrada nesta versão**.

### 24.3 Testes

- **`AutorizacaoTestes.Manifestacao`** (NFe.Testes, handler falso): as recusas dos quatro construtores; cada evento no `envEvento` do schema, assinado, `cOrgao` 91, `Id`, `descEvento`, `xJust`; autor CNPJ, CPF e mascarado; a retificação (sequência 2); o `Momento`; a justificativa de 14 e 256 (493) e de 15 e 255; o caractere inválido; o AN mesmo com a chave de SVC-AN e SVC-RS; o lote de 2 manifestações; a mistura com o cancelamento e o evento repetido (`NFE-LOTE`); a NF-e num núcleo de NFC-e (`NFE-CHAVE`); o `procEventoNFe` no schema com a assinatura; 135, 136 e as rejeições.
- **`AutorizacaoTestes.Cadastro`**: o `ConsCad` por CNPJ, CPF, inscrição e CNPJ alfanumérico (byte a byte, no schema, sem assinatura e sem `tpAmb`); as recusas do construtor; o destino de MG, SP, AM, ES, AC, RN, PB e SC; o `NFE-CADASTRO` em MA, RJ, CE, DF, PA e TO e a regra desligada com o `Destino`; a leitura de 111 (todos os campos e o endereço), 112, as rejeições 257 a 265, 108 e 109; campos opcionais ausentes; valores abertos e ilegíveis; a resposta ilegível. Os retornos de exemplo passam no `retConsCad_v2.00.xsd`.
- **`ManifestacaoEmuladaTestes`** (14, contra a SEFAZ emulada, núcleos de CNPJ diferente): a ciência (135) libera o `nfeProc` na distribuição do destinatário (por NSU e por chave); a confirmação chega ao emitente como `procEventoNFe`; a confirmação impede o cancelamento (221), também depois da retificação (sequência 2), e a terceira nem sai do construtor; o desconhecimento deixa cancelar; a operação não realizada libera o download; o autor CPF; 655, 575, 494, 650 e 651 (e a confirmação e a operação não realizada de nota cancelada, registradas); os prazos de 10 e 90 dias (596, com o relógio compartilhado); a consulta da nota com o `procEventoNFe`; o lote de 2; a conexão derrubada depois de processar e o reenvio (573). O 594 da ciência com sequência 2 não sai da lib: o emulador o prova no `SefazEmulada.Testes` (`ManifestacaoTestes`).
- **`CadastroEmuladoTestes`** (12, contra o emulador, com os contribuintes pela rota `POST /_controle/cadastros`): por CNPJ (todos os campos), inscrição e CPF; o CNPJ com duas inscrições (112); não cadastrado (259, 261, 264); o cadastro de outra UF; 265 pelo `Destino`; o solicitante irregular (257); 108 e 109; o ES pela SVRS; o `NFE-CADASTRO` sem pedido ao emulador.
- **Homologação real:** `ManifestacaoHomologacaoTestes` (a ciência de uma nota destinada ao CNPJ do PFX, achada pela distribuição) e `CadastroHomologacaoTestes` (o CNPJ do PFX em MG), pulados sem o PFX.
- **Testes que mudaram** por consequência direta da etapa: `LeiauteNFe400Testes` (as operações da NF-e ganham a `ConsultaCadastro`).

### 24.4 Implementação

**Ajustes ao desenho** (a conferir pelo usuário):

- **`Evento` ganhou o membro interno `AutorInformado`** (nulo por padrão): o `Manifestacao.Autor` público não pode sobrescrever o `Evento.Autor` interno (mesmo nome, outro acesso), então a propriedade pública esconde a interna (`new`) e a base passa a usar o `AutorInformado` quando ele existe. `EnvioEvento` e `OperacaoEvento` não mudaram; o teste `EventoFalso` da 6.1, que sobrescreve `Autor`, segue igual.
- **O autor é normalizado** (a máscara é aceita e tirada) e o CNPJ alfanumérico vale, como na distribuição; o desenho dizia "só os dígitos".
- **O número da seção é o 24**, e não o 25 do arquivo da etapa.
- **`RetornoCadastro` ganhou `UFAtendimento`** (o `cUF`).

**Decisões tomadas sem perguntar:**

- A inscrição estadual da consulta **não aceita máscara** (2 a 14 algarismos, só dígitos): a máscara varia por UF e o schema só aceita algarismos; a lib recusa em vez de adivinhar.
- A justificativa vazia da operação não realizada é `ArgumentException` (como o cancelamento); o tamanho é do schema (493).
- O `infCad` com `IE` ou `xNome` ausentes (obrigatórios no schema) **não é descartado**: o contribuinte sai com o campo vazio e um aviso, para não perder o resultado de uma consulta já feita.
- A lista de UFs do `NFE-CADASTRO` não foi reconferida no Portal (fora do alcance): vale a de `NTS.md` 3.2.
