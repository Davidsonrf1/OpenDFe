# NFS-e nacional: fontes e desenho (etapa 9.0)

Desenho da Fase 9 (`PROJETO.md` 18, `PLANO.md` Fase 9): o que as fontes oficiais dizem, o que isso muda no `PROJETO.md` 18, as decisões das questões do 18.5, os tipos e as assinaturas públicas, e a lista das regras fora do schema. **Proposto em 2026-11-19, a aprovar pelo usuário**: nada de código antes disso, porque cria API pública (`PLANO.md`, "Como trabalhar"). As fontes, com versão, data, endereço e SHA-256, estão em `docs/NTS.md` 12.

## 1. O que as fontes mudaram no `PROJETO.md` 18

Conferido em 2026-11-19 no gov.br/nfse (biblioteca, documentação atual, produção restrita, APIs) e nos documentos baixados. Seis achados mudam o que o 18 supunha:

1. **O DANFSe é de quem emite.** A NT SE/CGNFS-e 008/2026 (v1.02, 14/07/2026) dá o leiaute obrigatório do DANFSe para os sistemas emissores e **suspendeu a API de geração do DANFSe do ADN em 03/08/2026**. Não há "baixar o DANFSe" (18.4): a 9.5 gera o DANFSe no leiaute, pelo modelo de impressão neutro e o `OpenDFe.Impressao.Pdf`, como o DANFE. O emulador não precisa da rota do DANFSe (E.9).
2. **O cancelamento por substituição não é um evento que quem emite envia.** Quem substitui envia uma **DPS nova com o grupo `subst`** (a chave da substituída, o código e o motivo) ao `POST /nfse`; a Sefin Nacional gera sozinha o evento 105102, com autor "município emissor" (Anexo II, aba "TIPO EVENTOS"; manual da API, 1.3.2). Na lib, a substituição é a emissão com `Substituicao` preenchida, e o 105102 só aparece na leitura dos eventos.
3. **A manifestação (confirmação e rejeição do prestador, do tomador e do intermediário) vai ao ADN, e não à Sefin.** É o lado de quem toma o serviço, como a distribuição, que o 18.4 já deixou para depois: fica fora da Fase 9 (decisão D9).
4. **A Sefin Nacional só aceita a DPS do prestador** (`tpEmit` = 1; rejeição E9996 para 2 e 3).
5. **A assinatura é a da NF-e:** XMLDSig enveloped, C14N 2001-03-15, RSA-SHA1 e SHA-1, referência `#<Id>`, `Signature` sem prefixo, no fim do elemento pai (Manual Integrado 1.00.02, 6.1.4). Algumas bibliotecas abertas assinam em SHA-256 e relatam aceitação; a lib segue o manual, que é o que o `AssinaturaDigital` da 1.4 já faz (decisão D6).
6. **O destino não depende do município.** Todas as operações da Fase 9 vão a uma única Sefin Nacional (ou ao ADN), com o município **dentro do documento** (`cLocEmi`) ou no parâmetro da consulta. A configuração não precisa apontar o município (decisão D3), ao contrário do que o 18.2 supôs.

E um achado que pede decisão do usuário (D2): **a produção e a produção restrita usam a mesma versão `1.01` com schemas diferentes.** A produção roda o leiaute da NT 004 v2.00 com o domínio `tpRetPisConfins` da NT 007 (pacote `NFSe-ESQUEMAS_XSD-v1.01-20260209`). A produção restrita roda a NT 009 v1.01, "release 1" (pacote `NFSe-ESQUEMAS_XSD-PRODREST-v1.01-20261020`), que renomeia e reestrutura grupos (o `vDedRed` e o `gReeRepRes` viram o `vAjusteBC`; o `gLocBensMoveis` vira `bensMoveis`; o `dest` e o `indDest` sobem para o `infDPS`; entram as notas de ajuste, o `gPgtoVinc` e o CNPJ alfanumérico como texto), e o atributo `versao` continua `1.01` nos dois. Segundo a orientação do ADN (Fenacon, outubro de 2026), as releases 1 a 3 da NT 009 entram em produção **até 01/12/2026**, com cronograma formal ainda por publicar.

## 2. Decisões a aprovar

Cada uma com a recomendação. As que mudam o `PROJETO.md` 18 entram nele na aprovação.

| # | Questão | Recomendação |
|---|---|---|
| D1 | Entrada: `DFe.Criar` com um `ILeiaute`, ou uma porta própria? (18.5) | **`DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, configuracao)`**, a mesma porta. Ganha de graça o certificado, o pool, o caminho montar → assinar → schema → regras → callbacks → transmitir → ler, o `RecusadoLocalmente`, o `ResultadoWS`, o rastro e os callbacks. O que é REST entra como operação REST (D4), sem uma segunda porta para quem usa aprender. |
| D2 | Qual pacote de schemas embutir (produção ou produção restrita, ambos `1.01`)? | **O que estiver em produção no dia da 9.1**, conferido no portal na abertura dela. Hoje é o da NT 004/007; se a NT 009 já tiver entrado em produção (o previsto é até 01/12/2026), é o dela. Um só pacote por DLL: os dois têm `versao="1.01"`, e o leiaute não tem como saber de qual se trata. Enquanto a produção restrita estiver à frente, a lib pode ser recusada lá com a DPS da produção; isso fica escrito como pendência, como a NT 2026.008 na NF-e. Nada está publicado no NuGet, então trocar o pacote depois (com renomeações no modelo) não custa versão maior. **Alternativa**, se o usuário preferir não modelar grupos que vão mudar em semanas: começar direto pelo da produção restrita (NT 009 release 1) e aceitar que a lib não emite em produção até a NT 009 chegar lá. |
| D3 | A configuração: `ConfiguracaoDFe` ou uma própria; o município? (18.5) | **`ConfiguracaoDFe` como está, sem `ConfiguracaoNFSe`.** O município vai no documento (`cLocEmi` da DPS) e no parâmetro das consultas: um prestador com estabelecimentos em vários municípios usa um núcleo só. A `UF` continua `required` (é a do prestador; a NFS-e não a usa para destino, e a documentação diz isso): torná-la opcional mudaria o tipo da propriedade. `Emissao` diferente de `Normal` é recusada no `Criar` (a NFS-e nacional não tem contingência). `Urls` troca as bases como hoje. |
| D4 | Como o REST entra no núcleo (18.5) | **Um segundo tipo de handler, `IOperacaoRest<TParametro, TRetorno>`, registrado por uma sobrecarga de `RegistrarOperacao`.** O núcleo continua sem conhecer o documento: o leiaute monta um `PedidoRest` (método, caminho relativo, o XML a assinar e validar, o nome do campo JSON que o leva), e o núcleo assina, valida, compacta (GZip + Base64), põe no JSON `{"<campo>": "..."}`, transmite pelo mesmo `ConexoesSefaz` (o pool por certificado, a fábrica do handler, os tempos e as raízes da 4.1 e 8.5) e entrega ao `Ler` a `RespostaRest` (o status HTTP e o corpo). O transporte REST é interno, como o SOAP. |
| D5 | O que é resposta e o que é falha de transmissão no REST | **Vai para o `Ler` (é `Processado`):** 2xx, e 4xx com corpo JSON (as rejeições da Sefin vêm em 400 com `erros`; o 404 da consulta é "não encontrada"). No `HEAD`, 200 e 404 sem corpo. **É `FalhaTransmissaoException`:** 5xx; 4xx sem JSON (o 403 da borda que recusa o certificado é `CertificadoRecusado`); corpo ilegível (`RespostaIlegivel`). `PodeTerSidoProcessado` segue a regra de hoje (o pedido saiu inteiro e a resposta não veio). |
| D6 | A assinatura (18.5) | **A da NF-e, sem mudar nada:** o `AssinaturaDigital` da 1.4 (RSA-SHA1, C14N, enveloped, `#Id`), sobre o `infDPS` e o `infPedReg`. Se a Sefin real recusar (E0714) quando houver o PFX, a etapa do PFX decide; a opção por SHA-256 não entra antes. |
| D7 | O detector (18.5) | **O mesmo `DFe.Detectar`, com uma forma de leitura sem modelo no XML:** a `FormaLeitura` ganha um construtor com o `Documento` fixo (a NFS-e não tem `mod`), e o leiaute declara quatro formas no namespace `http://www.sped.fazenda.gov.br/nfse`: `DPS`, `NFSe`, `pedRegEvento` e `evento`, todas com a versão em `@versao` da raiz. A `DeteccaoDFe` não muda: `Documento.NFSe`, a `Raiz` e a `Versao`. |
| D8 | Os pacotes (18.3) | **`OpenDFe.NFSe.Modelo` e `OpenDFe.Leiaute.NFSe.v101`, no pacote `OpenDFe.NFSe`**, como o par da NF-e. O leiaute escreve a `1.01`; lê a `1.00` e a `1.01` (a 1.00 vigorou de julho de 2022 a 28/09/2025, e há NFS-e emitidas nela). O DANFSe no leiaute, o PDF no `OpenDFe.Impressao.Pdf`. |
| D9 | As operações e os eventos da fase (18.4) | **Emissão (com a substituição dentro), consulta pela chave, consulta pela DPS (`GET` e `HEAD`), a recuperação depois da queda no meio, o cancelamento (101101), a solicitação de análise fiscal para cancelamento (101103), a consulta dos eventos e a parametrização municipal.** Fora: a manifestação (ao ADN, com a distribuição), e os eventos de autoria do município ou do fisco (105104, 105105, 202204, 205208, 305101 a 305103), que a lib **lê** na consulta dos eventos mas não envia. |
| D10 | O modelo da DPS | **As convenções da NF-e** (`MODELO.md`): nomes descritivos em português com `[Tag]`, listas, enumerações com o valor do XML, nulo = ausente, sem cálculo. **A lista de campos é apresentada na abertura da 9.1 para aprovação**, como foi na 2.2 e na 2.3: o leiaute da DPS tem 252 elementos em 71 tipos, e o pacote só se fixa pela D2. |
| D11 | O valor de `Documento.NFSe` | **`NFSe = 99`.** Os valores do enum são o modelo do XML (55, 65), e a NFS-e nacional não tem modelo; o 99 não é usado por nenhum DF-e e nunca é escrito no XML (documentado no membro). |
| D12 | O rastro do REST | `XmlPedido` é o XML assinado, legível (o que se guarda), e não o JSON compactado; `XmlRetorno` é o corpo da resposta como veio (o JSON); o XML da NFS-e ou do evento, já descompactado, fica no `Retorno` da operação. `RastroWS` ganha `StatusHttp` (`int?`, nulo no SOAP). |
| D13 | A parametrização (9.4) | **Retorno com os campos que se conseguir confirmar e o JSON inteiro ao lado** (`JsonElement`), porque o Swagger oficial (`adn.../parametrizacao/docs`) só abre com o certificado ICP-Brasil no TLS mútuo, e os manuais não dão o JSON. As rotas do manual dos contribuintes (`/parametros_municipais/{municipio}/convenio`, `/{municipio}/{codigoServico}` e as duas `/{municipio}/{CPF/CNPJ}`, de retenções e de benefícios, que o manual escreve iguais) não batem com a base da página de APIs (`adn.../parametrizacao`); a 9.4 confere, e o emulador segue a nossa leitura. |

## 3. Tipos e assinaturas públicas

Os blocos mostram só os membros públicos, sem corpo, como em `CONTRATOS.md`. Tudo no núcleo é versão menor sobre o que existe (membros e tipos novos); nada muda de assinatura.

### 3.1. No núcleo (`OpenDFe`, pacote `OpenDFe.Nucleo`)

```csharp
public enum Documento
{
    NFe = 55,
    NFCe = 65,
    /// NFS-e do padrão nacional. Não é modelo de documento: o valor nunca é escrito no XML (D11).
    NFSe = 99,
}

public enum Destino
{
    // ... as UFs, AN = 91, SVRS = 101, SVAN = 102, SvcAN = 103, SvcRS = 104 ...
    /// A Sefin Nacional da NFS-e (emissor público nacional: a emissão, as consultas da NFS-e e da DPS, os eventos). Sigla "SEFIN".
    SefinNacional = 120,
    /// O Ambiente de Dados Nacional da NFS-e (parametrização municipal, distribuição). Sigla "ADN".
    ADN = 121,
}

/// O método HTTP de uma operação REST.
public enum MetodoRest { Get = 1, Post = 2, Head = 3 }

/// O handler de uma operação REST, no leiaute. Sem estado e sem I/O, como o IOperacao.
public interface IOperacaoRest<TParametro, TRetorno>
    where TParametro : ParametroWS<TRetorno>
    where TRetorno : class
{
    /// Monta o pedido, sem assinar nem compactar.
    PedidoRest Montar(TParametro parametro, ContextoOperacao contexto);

    /// Lê a resposta. xmlPedido: o XML como foi enviado, assinado (null sem XML). JSON que não se lê: XmlException,
    /// FormatException ou JsonException, que o núcleo embrulha numa FalhaTransmissaoException com o rastro.
    LeituraWS<TRetorno> Ler(TParametro parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto);
}

/// O pedido REST montado. Construtor público: o leiaute cria.
public sealed class PedidoRest
{
    /// servico: o nome da base na tabela de URLs ("SefinNacional"). caminho: relativo à base, já com os valores
    /// ("nfse", "nfse/<chave>/eventos", "dps/<id>"). ArgumentException: caminho absoluto ou vazio.
    public PedidoRest(string servico, MetodoRest metodo, string caminho);
    public string Servico { get; }
    public MetodoRest Metodo { get; }
    public string Caminho { get; }

    /// O documento que vai no corpo (a DPS, o pedRegEvento) e o seu schema; null = sem corpo (GET, HEAD).
    public string? Xml { get; init; }
    public SchemaXml? Schema { get; init; }
    /// O campo do JSON que leva o XML compactado ("dpsXmlGZipB64", "pedidoRegistroEventoXmlGZipB64").
    public string? CampoXml { get; init; }
    /// Como no PedidoWS: os Id a assinar, o destino (obrigatório no REST: não há UF para cair), os problemas da
    /// montagem e os documentos para as regras e os callbacks.
    public IReadOnlyList<string> IdsAssinatura { get; init; } = [];
    public Destino? Destino { get; init; }
    public IReadOnlyList<Problema> Problemas { get; init; } = [];
    public IReadOnlyList<DocumentoPedido> Documentos { get; init; } = [];
}

/// A resposta HTTP que o núcleo entrega ao handler (D5). Construtor público, para os testes do leiaute.
public sealed class RespostaRest
{
    public RespostaRest(int status, string? corpo);
    public int Status { get; }
    /// O corpo como texto (JSON); null no HEAD e na resposta vazia.
    public string? Corpo { get; }
}

public sealed class DeclaracaoLeiaute
{
    // ... o que existe ...
    /// ArgumentException: já há handler (SOAP ou REST) para TParametro.
    public void RegistrarOperacao<TParametro, TRetorno>(IOperacaoRest<TParametro, TRetorno> operacao)
        where TParametro : ParametroWS<TRetorno>
        where TRetorno : class;
}

public sealed class FormaLeitura
{
    // ... o construtor de hoje, com o caminho do modelo ...
    /// Uma forma sem modelo no XML: o documento é sempre o informado (D7).
    public FormaLeitura(string espacoNomes, string raiz, string caminhoVersao, Documento documento);
    /// null na forma com Documento fixo.
    public string? CaminhoModelo { get; }
    /// O documento fixo; null na forma com o caminho do modelo.
    public Documento? Documento { get; }
}

public sealed class RastroWS
{
    // ... o que existe ...
    /// O status HTTP da resposta REST; null no SOAP e sem resposta (D12).
    public int? StatusHttp { get; }
}
```

**No caminho do `ExecutarAsync`** (seção 3 de `CONTRATOS.md`): o passo 1 acha o handler SOAP ou REST pelo tipo; os passos 2 a 6 são os mesmos sobre o `Xml` do `PedidoRest` (sem `Xml`, pulam a assinatura e o schema); o 7 escolhe a URL = a base do `Destino` × ambiente × `Servico`, mais o `Caminho`; o 8 é o transporte REST (o JSON com o campo compactado, `Content-Type: application/json`); o 9 é o `Ler` com a `RespostaRest`, ou a falha da D5.

**No `Criar`:** a recusa "a UF não tem atendimento para o tipo de emissão" só vale quando o leiaute declarou algum atendimento (o da NFS-e não declara); com `Documento.NFSe`, `Emissao` diferente de `Normal` é `ArgumentException` (D3).

### 3.2. No modelo (`OpenDFe.NFSe`, DLL `OpenDFe.NFSe.Modelo`)

Só os tipos de topo e os que as operações usam; os campos da DPS saem na 9.1 (D10).

```csharp
/// A Declaração de Prestação de Serviço: o que quem emite assina e envia. Os grupos e campos, na 9.1.
public sealed class Dps
{
    [Tag("tpAmb")]   public Ambiente? Ambiente { get; set; }
    [Tag("dhEmi")]   public DateTimeOffset? Emissao { get; set; }
    [Tag("serie")]   public string? Serie { get; set; }
    [Tag("nDPS")]    public long? Numero { get; set; }
    [Tag("dCompet")] public DateOnly? Competencia { get; set; }
    [Tag("tpEmit")]  public EmitenteDps? Emitente { get; set; }          // só Prestador na Sefin Nacional
    [Tag("cLocEmi")] public int? MunicipioEmissor { get; set; }          // o código IBGE
    [Tag("subst")]   public Substituicao? Substituicao { get; set; }     // preenchida = substitui a NFS-e da chave
    [Tag("prest")]   public Prestador? Prestador { get; set; }
    [Tag("toma")]    public Pessoa? Tomador { get; set; }
    [Tag("interm")]  public Pessoa? Intermediario { get; set; }
    [Tag("serv")]    public Servico? Servico { get; set; }
    [Tag("valores")] public ValoresDps? Valores { get; set; }
    [Tag("IBSCBS")]  public IbsCbsDps? IbsCbs { get; set; }
    // ...
}

public sealed class Substituicao
{
    [Tag("chSubstda")] public string? Chave { get; set; }                // texto, como as chaves referenciadas da NF-e
    [Tag("cMotivo")]   public MotivoSubstituicao? Motivo { get; set; }   // 01 a 05, 99
    [Tag("xMotivo")]   public string? Descricao { get; set; }
}

/// A chave de acesso da NFS-e (50 dígitos): município (7), ambiente gerador (1), tipo de inscrição (1),
/// inscrição (14), número (13), ano e mês (4), código numérico (9), DV (1). Gerada pelo ambiente nacional.
public readonly record struct ChaveNFSe
{
    /// FormatException: tamanho, caracteres ou DV.
    public static ChaveNFSe Ler(string chave);
    public static bool TentarLer(string? chave, out ChaveNFSe resultado);
    public int Municipio { get; }
    public string Inscricao { get; }
    public long Numero { get; }
    public string Valor { get; }
}

/// O identificador da DPS (45): "DPS" + município (7) + tipo de inscrição (1) + inscrição (14) + série (5) + número (15).
/// É o Id do infDPS e o parâmetro da consulta pela DPS; sai dos campos da DPS.
public readonly record struct IdentificadorDps
{
    public static IdentificadorDps Montar(int municipioEmissor, string cpfCnpj, string serie, long numero);
    public static IdentificadorDps Ler(string id);
    public string Valor { get; }
}

/// A NFS-e lida: o infNFSe (número, emitente, valores calculados, o IBSCBS, a situação) e a DPS de dentro.
public sealed class NFSe { /* na 9.1 */ }

/// A base dos eventos que quem emite envia (D9). O detalhe é a subclasse; o Id (PRE + chave + tipo + número) sai dela.
public abstract class EventoNFSe
{
    private protected EventoNFSe();
    public string? Chave { get; set; }
    public DateTimeOffset? Data { get; set; }                            // dhEvento
    public string? Autor { get; set; }                                   // CNPJAutor ou CPFAutor
    public abstract string Tipo { get; }                                 // "101101"
}
public sealed class CancelamentoNFSe : EventoNFSe                       // e101101
{
    public MotivoCancelamento? Motivo { get; set; }                      // 1 erro na emissão, 2 serviço não prestado, 9 outros
    public string? Descricao { get; set; }
}
public sealed class AnaliseFiscalCancelamento : EventoNFSe              // e101103
{
    public MotivoAnaliseFiscal? Motivo { get; set; }
    public string? Descricao { get; set; }
}
```

### 3.3. No leiaute (`OpenDFe.NFSe`, DLL `OpenDFe.Leiaute.NFSe.v101`)

```csharp
public sealed class LeiauteNFSe101 : ILeiaute
{
    public static LeiauteNFSe101 Instancia { get; }
    public string Versao { get; }                                        // "1.01"
    public IReadOnlyList<Documento> Documentos { get; }                  // [NFSe]
    public void Declarar(DeclaracaoLeiaute declaracao);

    /// A escrita da DPS sem transmitir (o XML para guardar ou o emulador), com os problemas da montagem.
    public DpsEscrita Escrever(Dps dps);
    /// A leitura da NFS-e, da DPS e dos eventos, 1.00 e 1.01, com o XML original preservado.
    public NFSeLida Ler(string xml);
    public EventoLido LerEvento(string xml);
}

// As operações (o parâmetro → o retorno). Nomes de trabalho; os retornos detalhados saem na etapa de cada uma.
public sealed class EmissaoNFSe : ParametroWS<RetornoEmissaoNFSe>       // POST /nfse         (9.2)
{
    public EmissaoNFSe(Dps dps);
    public Dps Dps { get; }
}
public sealed class RetornoEmissaoNFSe
{
    public bool Emitida { get; }
    public ChaveNFSe? Chave { get; }
    public string? Xml { get; }                                          // a NFS-e, descompactada
    public NFSe? Nota { get; }
    public IReadOnlyList<MensagemSefin> Erros { get; }                   // codigo, descricao, complemento
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}
public sealed class ConsultaNFSe : ParametroWS<RetornoConsultaNFSe>     // GET /nfse/{chave}  (9.2)
{
    public ConsultaNFSe(ChaveNFSe chave);
}
public sealed class ConsultaDps : ParametroWS<RetornoConsultaDps>       // GET ou HEAD /dps/{id} (9.2)
{
    public ConsultaDps(IdentificadorDps id);
    /// true: HEAD, que só diz se a NFS-e foi gerada (qualquer certificado); false: GET, que devolve a chave
    /// (só ao prestador, ao tomador ou ao intermediário da nota).
    public bool ApenasExistencia { get; init; }
}
/// A queda no meio da emissão (como a 8.6): consulta a DPS e, gerada, a NFS-e pela chave. Duas chamadas.
public static class RecuperacaoEmissao
{
    public static Task<ResultadoWS<RetornoConsultaNFSe>> ExecutarAsync(
        DFe dfe, IdentificadorDps id, CancellationToken cancellationToken = default);
}
public sealed class EnvioEventoNFSe : ParametroWS<RetornoEventoNFSe>    // POST /nfse/{chave}/eventos (9.3)
{
    public EnvioEventoNFSe(EventoNFSe evento);
}
public sealed class ConsultaEventosNFSe : ParametroWS<RetornoEventosNFSe> // GET /nfse/{chave}/eventos[/{tipo}[/{seq}]] (9.3)
{
    public ConsultaEventosNFSe(ChaveNFSe chave);
    public string? Tipo { get; init; }
    public int? Sequencia { get; init; }
}
// A parametrização (9.4): ConsultaConvenio, ConsultaParametrosServico, ConsultaRetencoes, ConsultaBeneficios,
// com o município e o código de tributação nacional ou o CPF/CNPJ; o retorno pela D13.

// O DANFSe (9.5): o modelo de impressão do leiaute pela NT 008/2026, como o DANFE da 7.3.
public static class DanfseNFSe { public static DocumentoImpressao Montar(NFSe nota, OpcoesDanfse? opcoes = null); }
```

### 3.4. Exemplo de uso

```csharp
using var conexoes = new ConexoesSefaz();
var dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,          // a produção restrita
    UF = UF.MG,                               // a do prestador; a NFS-e não a usa para destino
    Certificado = certificado,
    Conexoes = conexoes,
});

var dps = new Dps { /* ... MunicipioEmissor = 3106200, Serie = "1", Numero = 42 ... */ };
var resultado = await dfe.ExecutarAsync(new EmissaoNFSe(dps));
if (resultado.Processado && resultado.Retorno.Emitida)
    Guardar(resultado.Retorno.Chave, resultado.Retorno.Xml);
// FalhaTransmissaoException com PodeTerSidoProcessado: RecuperacaoEmissao.ExecutarAsync(dfe, IdentificadorDps.Montar(...))
```

## 4. Regras fora do schema (2.9), para aprovar

O schema da DPS é a base, com o `xs:documentation` de cada campo como já faz a NF-e. Quase todas as 655 linhas do "RN DPS_NFS-e" do Anexo I dependem do que só o ambiente nacional tem (o convênio, o CNC, o cadastro CNPJ/CPF, o Simples Nacional, a calculadora do IBS e da CBS, os valores que ele calcula). Ficam com a Sefin. A lista abaixo pega o erro comum que o schema não vê e quase não muda:

| Código local | O que confere | Rejeição que evita | Severidade |
|---|---|---|---|
| `NFSE-DOCUMENTO` | o DV do CPF e do CNPJ (o alfanumérico também) do prestador, do tomador e do intermediário, pelo `Cpf` e o `Cnpj` do núcleo | E0082, E0098, E0190, E0207, E0250 (não encontrado no cadastro) | Erro |
| `NFSE-COMPETENCIA` | `dCompet` não é posterior à data do `dhEmi` | E0015 | Erro |
| `NFSE-EMISSAO` | `dhEmi` não é posterior ao relógio da configuração, com a mesma tolerância da regra de datas da NF-e (`REGRAS.md`) | E0008 | Erro |
| `NFSE-EMITENTE` | `tpEmit` = 1 (a Sefin Nacional só aceita o prestador) | E9996 | Erro |
| `NFSE-PRESTADOR` | com `tpEmit` = 1, o prestador sem `xNome`, sem `end`, sem `NIF` e sem `cNaoNIF` (o ambiente nacional os tira do cadastro) | E0121, E0128, E0112, E0114 | Erro |
| `NFSE-CHAVE` | o DV da chave da NFS-e substituída (`chSubstda`) e da chave do evento; **se a 9.1 não conseguir confirmar o algoritmo do DV com chaves reais, a regra cai** | E0042 | Erro |

Não entram: o titular do certificado contra o prestador (E0718: é de quem usa, pelo `CertificadoDigital.LerTitular`, como na NF-e), o município conveniado e com o emissor nacional (E0037 a E0039: é a parametrização, que quem usa consulta pela 9.4), e os valores calculados (base, ISS, líquido, IBS e CBS: a lib não calcula, 1.1).

## 5. O que isso muda no emulador (E.7 a E.9)

- **Rotas** com os caminhos da real, sob um prefixo: `POST /nfse`, `GET /nfse/{chave}`, `GET` e `HEAD /dps/{id}`, `POST` e `GET /nfse/{chave}/eventos[...]` (Sefin) e as da parametrização (ADN), na leitura da D13.
- **A marca `EMULADA:`** vai no `infDPS/serv/infoCompl/xInfComp` (informações complementares, texto livre de até 2.000 caracteres).
- **A substituição** é a DPS com `subst`: o emulador gera o 105102 sobre a substituída, como a Sefin.
- **Sem a rota do DANFSe** (suspensa no ADN): a E.9 fica com o painel e a persistência.
- **A assinatura da NFS-e** pelo certificado da AC de teste no papel do município ou da Sefin (E1638), e a resposta no JSON da Sefin (`chaveAcesso`, `nfseXmlGZipB64`, `alertas`; e `erros` com `codigo`, `descricao` e `complemento`).

## 6. O que fica pendente desta etapa

1. **O Swagger da Sefin e do ADN** está atrás do TLS mútuo com ICP-Brasil (403 e handshake recusado em 2026-11-19): os nomes do JSON (`dpsXmlGZipB64`, `pedidoRegistroEventoXmlGZipB64`, `chaveAcesso`, `nfseXmlGZipB64`, `erros`, `alertas`) vêm do manual (que diz "GZip em base64 no JSON" sem dar os nomes) e de implementações abertas que emitem na produção restrita. A base da Sefin na produção restrita aparece com e sem `/API/` (`.../API/SefinNacional/docs` na página de APIs, `.../SefinNacional` nas implementações): a 9.2 registra as duas e o PFX decide.
2. **A NT 009** (D2) e o cronograma de produção dela; a nota técnica específica do DANFSe para os novos fatos geradores do IBS e da CBS (anunciada na NT 008, não publicada).
3. **As fontes do DANFSe:** a NT 008 pede Arial e Microsoft Sans Serif; a lib usa a Lato (a mesma pendência do DANFE, `PLANO.md`, pendência 7).
4. **A homologação real** (a produção restrita) de tudo, quando houver o PFX (`PROJETO.md` 17).
