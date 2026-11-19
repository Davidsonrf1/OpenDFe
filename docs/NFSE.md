# NFS-e nacional: fontes e desenho (etapa 9.0)

Desenho da Fase 9 (`PROJETO.md` 18, `PLANO.md` Fase 9): o que as fontes oficiais dizem, o que isso muda no `PROJETO.md` 18, as decisões das questões do 18.5, os tipos e as assinaturas públicas, e a lista das regras fora do schema. **Aprovado pelo usuário em 2026-11-19**, inteiro, com a D2 pela recomendação; incorporado ao `PROJETO.md` 18. As fontes, com versão, data, endereço e SHA-256, estão em `docs/NTS.md` 12.

## 1. O que as fontes mudaram no `PROJETO.md` 18

Conferido em 2026-11-19 no gov.br/nfse (biblioteca, documentação atual, produção restrita, APIs) e nos documentos baixados. Seis achados mudam o que o 18 supunha:

1. **O DANFSe é de quem emite.** A NT SE/CGNFS-e 008/2026 (v1.02, 14/07/2026) dá o leiaute obrigatório do DANFSe para os sistemas emissores e **suspendeu a API de geração do DANFSe do ADN em 03/08/2026**. Não há "baixar o DANFSe" (18.4): a 9.5 gera o DANFSe no leiaute, pelo modelo de impressão neutro e o `OpenDFe.Impressao.Pdf`, como o DANFE. O emulador não precisa da rota do DANFSe (E.9).
2. **O cancelamento por substituição não é um evento que quem emite envia.** Quem substitui envia uma **DPS nova com o grupo `subst`** (a chave da substituída, o código e o motivo) ao `POST /nfse`; a Sefin Nacional gera sozinha o evento 105102, com autor "município emissor" (Anexo II, aba "TIPO EVENTOS"; manual da API, 1.3.2). Na lib, a substituição é a emissão com `Substituicao` preenchida, e o 105102 só aparece na leitura dos eventos.
3. **A manifestação (confirmação e rejeição do prestador, do tomador e do intermediário) vai ao ADN, e não à Sefin.** É o lado de quem toma o serviço, como a distribuição, que o 18.4 já deixou para depois: fica fora da Fase 9 (decisão D9).
4. **A Sefin Nacional só aceita a DPS do prestador** (`tpEmit` = 1; rejeição E9996 para 2 e 3).
5. **A assinatura é a da NF-e:** XMLDSig enveloped, C14N 2001-03-15, RSA-SHA1 e SHA-1, referência `#<Id>`, `Signature` sem prefixo, no fim do elemento pai (Manual Integrado 1.00.02, 6.1.4). Algumas bibliotecas abertas assinam em SHA-256 e relatam aceitação; a lib segue o manual, que é o que o `AssinaturaDigital` da 1.4 já faz (decisão D6).
6. **O destino não depende do município.** Todas as operações da Fase 9 vão a uma única Sefin Nacional (ou ao ADN), com o município **dentro do documento** (`cLocEmi`) ou no parâmetro da consulta. A configuração não precisa apontar o município (decisão D3), ao contrário do que o 18.2 supôs.

E um achado que pede decisão do usuário (D2): **a produção e a produção restrita usam a mesma versão `1.01` com schemas diferentes.** A produção roda o leiaute da NT 004 v2.00 com o domínio `tpRetPisConfins` da NT 007 (pacote `NFSe-ESQUEMAS_XSD-v1.01-20260209`). A produção restrita roda a NT 009 v1.01, "release 1" (pacote `NFSe-ESQUEMAS_XSD-PRODREST-v1.01-20261020`), que renomeia e reestrutura grupos (o `vDedRed` e o `gReeRepRes` viram o `vAjusteBC`; o `gLocBensMoveis` vira `bensMoveis`; o `dest` e o `indDest` sobem para o `infDPS`; entram as notas de ajuste, o `gPgtoVinc` e o CNPJ alfanumérico como texto), e o atributo `versao` continua `1.01` nos dois. Segundo a orientação do ADN (Fenacon, outubro de 2026), as releases 1 a 3 da NT 009 entram em produção **até 01/12/2026**, com cronograma formal ainda por publicar.

## 2. Decisões (aprovadas em 2026-11-19)

Todas aprovadas pela recomendação. As que mudam o `PROJETO.md` 18 já estão nele.

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

## 4. Regras fora do schema (2.9), aprovadas em 2026-11-19

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

## 7. O modelo da DPS, da NFS-e e dos eventos (etapa 9.1): para aprovação

Apresentado na abertura da 9.1 (2026-11-19), antes do código, pela D10. **Aprovado pelo usuário em 2026-11-19, tudo pela recomendação** ("aprovado, tudo pela recomendação"). O que mudou na implementação está em 8.

### 7.1. O pacote (D2), conferido na abertura

Em 2026-11-19 a página "Documentação atual" (atualizada em 04/11/2026) segue com o `NFSe-ESQUEMAS_XSD-v1.01-20260209` (NT 004 v2.00 e NT 007), baixado de novo com o **mesmo SHA-256** da 9.0 (`e7935cbd...a8072`); a NT 009 não entrou em produção. Pela D2, **a 9.1 embute esse pacote** (pastas `1.00` e `1.01`), e a NT 009 fica como pendência. O Anexo I foi baixado de novo, também com o mesmo SHA-256 (`de5bc492...7eb428e`).

A árvore abaixo saiu do XSD por script (`TCDPS`, `TCNFSe`, `TCPedRegEvt`, `TCEvento`), comparada com a aba "LEIAUTE DPS_NFS-e" do Anexo I.

### 7.2. Decisões do modelo

Todas seguem as regras de forma do `MODELO.md` 3 (classe selada mutável, todo campo anulável, grupo obrigatório inicializado, listas `IList<T>` só leitura, sem conferência no modelo). O que é próprio da NFS-e:

| # | Questão | Recomendação |
|---|---|---|
| M1 | O `[Tag]` | **Só a tag (`[Tag("cTribNac")]`), sem identificador.** O Anexo I não tem identificador de campo como o MOC (B01, C02): numera as linhas da planilha (1, 2, 3...), e a numeração muda a cada versão. O construtor de um parâmetro já existe (`MODELO.md`, decisão 12). |
| M2 | A raiz da DPS | **`Dps` achatada (o `DPS` e o `infDPS` juntos); `@Id`, `@versao` e a `Signature` ficam fora.** O Id sai de `IdentificadorDps.Montar` na escrita e vai para o resultado (`DpsEscrita`), como a chave da NF-e (`MODELO.md`, decisões 2 e 20). |
| M3 | O nome da NFS-e lida | **`NotaFiscalServico`, e não `NFSe`.** Uma classe `NFSe` no namespace `OpenDFe.NFSe` quebra o CA1724 e deixa o nome ambíguo para quem escreve `using OpenDFe.NFSe;`. Paralelo a `NotaFiscal`. Achatada (o `NFSe` e o `infNFSe`), com a DPS de dentro em `.Dps`; a chave fica no resultado da leitura (`NFSeLida.Chave`). Corrige o `NFSe` dos itens 3.2 e 3.3 deste documento (`RetornoEmissaoNFSe.Nota`, `DanfseNFSe.Montar`). |
| M4 | Os campos que o núcleo sabe | **`Ambiente` nulo = o da configuração; preenchido e diferente = `Erro`** (`MODELO.md`, decisão 6). **`VersaoAplicativo` nulo = "OpenDFe " + a versão do pacote**, o mesmo padrão dos eventos da NF-e (`CancelamentoSubstituicao`). Vale para a `Dps` e o `EventoNFSe`. |
| M5 | Os tipos | Os do `MODELO.md`, decisão 7: `decimal?` nos valores e percentuais; `DateTimeOffset?` nos `dh*`; `DateOnly?` em `dCompet`, `dtIni`, `dtFim`, `dtEmiDoc`, `dtCompDoc`; `long?` em `nDPS`, `nNFSe`, `nDFSe`; `int?` nos códigos IBGE, no `nSeqEvento` e nas contagens; `string?` onde o zero à esquerda ou a letra importam (CNPJ, CPF, NIF, CAEPF, IM, CEP, telefone, `cTribNac`, `cTribMun`, `cNBS`, `cPais`, `tpMoeda`, o CST e o `cClassTrib` do IBS/CBS como na NF-e, `cIndOp`, `cCredPres`, as chaves). **A `Serie` fica `string?`**, como a 9.0 aprovou (o XSD aceita `1` e `00001`). |
| M6 | Enumerações | **`enum` com o valor do XML, também nos 0/1 e nos de um valor só** (`finNFSe` = 0, `indTotTrib` = 0, que as NTs vão ampliar: a NT 009 já põe o 1 e o 2 no `finNFSe`). Os códigos com zero à esquerda (`01` a `99`) são o número, e o leiaute escreve os zeros. **O modelo da NFS-e não depende do da NF-e:** o CST do PIS/COFINS ganha enum próprio (`CstPisCofinsServico`). |
| M7 | Escolhas do XSD | **Propriedades exclusivas em todas as da DPS e da NFS-e** (CNPJ/CPF/NIF/`cNaoNIF`; `endNac`/`endExt`; `pDR`/`vDR`/`documentos`; os seis do `docDedRed`; os três do `gReeRepRes`; os quatro do `totTrib`; `vRedBCBM`/`pRedBCBM`; `cLocPrestacao`/`cPaisPrestacao`; `cObra`/`cCIB`/`end`): todas dividem o pai com outros campos (`MODELO.md`, decisão 10). As subclasses ficam só nos eventos (a 9.0, 3.2). Mais de uma preenchida = `Erro` na montagem. |
| M8 | O mesmo tipo do XSD em vários lugares | **Uma classe por tipo do XSD** (`MODELO.md`, decisão 12 (b)): `TCInfoPessoa` → `Pessoa` (tomador, intermediário, fornecedor da dedução); `TCEndereco` → `Endereco` (as quatro pessoas e o destinatário); `TCEnderObraEvento` → `EnderecoObra` (a obra e o imóvel). Tipos diferentes com o mesmo conteúdo ficam em classes diferentes, como os endereços da NF-e (`TCEnderecoSimples` → `EnderecoEvento`; `TCRTCInfoDest` → `Destinatario`; os três grupos de alíquota do IBS/CBS). |
| M9 | Grupo que só tem uma lista (`gItemPed`, `gRefNFSe`, `documentos` da dedução, `gReeRepRes`) | **Uma classe com a lista**, como o `gPagAntecipado` da NF-e (`PagamentoAntecipado.Chaves`): o caminho do problema (2.6) tem um passo para cada elemento. |
| M10 | A versão 1.00 (D8) | **O modelo é a união das duas.** Os grupos que só a 1.00 tem (`explRod`, pedágio; `lsadppu`, locação de faixa, postes e dutos; o `tpBM` dentro do `BM`) entram como propriedades marcadas "só na 1.00": a leitura preenche, a escrita da 1.01 não escreve (aviso de campo não suportado, `PROJETO.md` 4.4). O que mudou de nome ou de lugar vai para a mesma propriedade (`atvEvento/desc` da 1.00 → `AtividadeEvento.Nome`; `valores/xOutInf` da 1.00 → `NotaFiscalServico.InformacoesFisco`). O `valores/tpBM` da NFS-e 1.00 é texto livre (até 40) e na 1.01 é código (1 a 4): a leitura da 1.00 aproveita o código quando o texto for 1 a 4 e, senão, deixa nulo com aviso (o XML original fica preservado). |
| M11 | Os eventos | **As 16 subclasses do XSD 1.01 no modelo** (a leitura precisa de todas: D9 lê os do município e do fisco); o envio da 9.3 aceita só as duas de quem emite. O `xDesc` (texto fixo por tipo) fica fora: o leiaute escreve e a leitura confere. **Correção da 9.0:** o Id do pedido é `PRE` + chave (50) + tipo (6), 56 dígitos, **sem** número de sequência (`TSIdPedRegEvt`); o do evento registrado é `EVT` + chave + tipo + `nSeqEvento` (3). O evento registrado (o `evento` que a Sefin devolve) é `EventoNFSeRegistrado`, com o pedido dentro. |
| M12 | O `tribISSQN` muda de texto entre as versões | **Achado:** o XSD 1.00 documenta 1 Tributável, 2 Exportação, 3 Não incidência, 4 Imunidade; o 1.01 e o Anexo I 1.01 (linha 300 do leiaute e as regras E1301 e E1321), 1 Tributável, 2 Imunidade, 3 Exportação, 4 Não incidência. Não há como saber, pelas fontes baixadas, se a 1.00 usava mesmo outros códigos ou se o texto dela estava errado. **Recomendação:** o enum segue a 1.01, e a leitura da 1.00 guarda o número como veio, sem traduzir. A dúvida fica como pendência; quem lê NFS-e 1.00 é avisado no membro. |

### 7.3. A DPS (`OpenDFe.NFSe.Modelo`)

```csharp
/// DPS + infDPS (M2). O Id, a versao e a Signature ficam fora.
public sealed class Dps
{
    [Tag("tpAmb")]         public Ambiente? Ambiente { get; set; }                 // nulo = o da configuração (M4)
    [Tag("dhEmi")]         public DateTimeOffset? Emissao { get; set; }
    [Tag("verAplic")]      public string? VersaoAplicativo { get; set; }           // nulo = "OpenDFe x.y.z" (M4)
    [Tag("serie")]         public string? Serie { get; set; }
    [Tag("nDPS")]          public long? Numero { get; set; }
    [Tag("dCompet")]       public DateOnly? Competencia { get; set; }
    [Tag("tpEmit")]        public EmitenteDps? Emitente { get; set; }              // 1 Prestador, 2 Tomador, 3 Intermediario
    [Tag("cMotivoEmisTI")] public MotivoEmissaoTerceiro? MotivoEmissaoTerceiro { get; set; } // 1 a 4
    [Tag("chNFSeRej")]     public string? ChaveRejeitada { get; set; }
    [Tag("cLocEmi")]       public int? MunicipioEmissor { get; set; }
    [Tag("subst")]         public Substituicao? Substituicao { get; set; }
    [Tag("prest")]         public Prestador Prestador { get; set; } = new();
    [Tag("toma")]          public Pessoa? Tomador { get; set; }
    [Tag("interm")]        public Pessoa? Intermediario { get; set; }
    [Tag("serv")]          public Servico Servico { get; set; } = new();
    [Tag("valores")]       public ValoresDps Valores { get; set; } = new();
    [Tag("IBSCBS")]        public IbsCbsDps? IbsCbs { get; set; }
}

public sealed class Substituicao                         // subst (como na 9.0)
{
    [Tag("chSubstda")] public string? Chave { get; set; }
    [Tag("cMotivo")]   public MotivoSubstituicao? Motivo { get; set; }      // 01 a 05, 99
    [Tag("xMotivo")]   public string? Descricao { get; set; }
}

public sealed class Prestador                            // TCInfoPrestador
{
    [Tag("CNPJ")]    public string? Cnpj { get; set; }                        // um dos quatro (M7)
    [Tag("CPF")]     public string? Cpf { get; set; }
    [Tag("NIF")]     public string? Nif { get; set; }
    [Tag("cNaoNIF")] public MotivoAusenciaNif? MotivoAusenciaNif { get; set; } // 0 a 2
    [Tag("CAEPF")]   public string? Caepf { get; set; }
    [Tag("IM")]      public string? InscricaoMunicipal { get; set; }
    [Tag("xNome")]   public string? Nome { get; set; }                       // com tpEmit 1, ausente (NFSE-PRESTADOR)
    [Tag("end")]     public Endereco? Endereco { get; set; }                 // idem
    [Tag("fone")]    public string? Telefone { get; set; }
    [Tag("email")]   public string? Email { get; set; }
    [Tag("regTrib")] public RegimeTributacao RegimeTributacao { get; set; } = new();
}

public sealed class RegimeTributacao                     // TCRegTrib
{
    [Tag("opSimpNac")]   public SituacaoSimplesNacional? SimplesNacional { get; set; }   // 1 NaoOptante, 2 Mei, 3 MeEpp
    [Tag("regApTribSN")] public RegimeApuracaoSimples? ApuracaoSimples { get; set; }     // 1 a 3
    [Tag("regEspTrib")]  public RegimeEspecialTributacao? RegimeEspecial { get; set; }   // 0 Nenhum a 6, 9
}

/// TCInfoPessoa: o tomador, o intermediário e o fornecedor da dedução (M8).
public sealed class Pessoa
{
    // Cnpj, Cpf, Nif, MotivoAusenciaNif, Caepf, InscricaoMunicipal, Nome, Endereco, Telefone, Email: as tags e os
    // nomes do Prestador, sem o regTrib.
}

/// TCEndereco (M8). Nacional ou Exterior (M7).
public sealed class Endereco
{
    [Tag("endNac")]  public EnderecoNacional? Nacional { get; set; }
    [Tag("endExt")]  public EnderecoExterior? Exterior { get; set; }
    [Tag("xLgr")]    public string? Logradouro { get; set; }
    [Tag("nro")]     public string? Numero { get; set; }
    [Tag("xCpl")]    public string? Complemento { get; set; }
    [Tag("xBairro")] public string? Bairro { get; set; }
}
public sealed class EnderecoNacional  { [Tag("cMun")] int? Municipio; [Tag("CEP")] string? Cep; }
public sealed class EnderecoExterior  { [Tag("cPais")] string? Pais; [Tag("cEndPost")] string? CodigoPostal;
                                        [Tag("xCidade")] string? Cidade; [Tag("xEstProvReg")] string? EstadoProvincia; }
/// TCEnderObraEvento (a obra e o imóvel) e TCEnderecoSimples (o evento): o mesmo conteúdo, tipos diferentes (M8).
public sealed class EnderecoObra   { [Tag("CEP")] string? Cep; [Tag("endExt")] EnderecoExteriorSimples? Exterior;
                                     Logradouro, Numero, Complemento, Bairro (as tags do Endereco) }
public sealed class EnderecoEvento { /* o mesmo conteúdo do EnderecoObra */ }
public sealed class EnderecoExteriorSimples { CodigoPostal, Cidade, EstadoProvincia (as tags do EnderecoExterior) }
```

O serviço:

```csharp
public sealed class Servico                              // serv
{
    [Tag("locPrest")]  public LocalPrestacao Local { get; set; } = new();
    [Tag("cServ")]     public CodigoServico Codigo { get; set; } = new();
    [Tag("comExt")]    public ComercioExterior? ComercioExterior { get; set; }
    [Tag("lsadppu")]   public LocacaoSublocacao? Locacao { get; set; }               // só na 1.00 (M10)
    [Tag("obra")]      public Obra? Obra { get; set; }
    [Tag("atvEvento")] public AtividadeEvento? Evento { get; set; }
    [Tag("explRod")]   public ExploracaoRodoviaria? ExploracaoRodoviaria { get; set; } // só na 1.00 (M10)
    [Tag("infoCompl")] public InformacoesComplementares? InformacoesComplementares { get; set; }
}

public sealed class LocalPrestacao   { [Tag("cLocPrestacao")] int? Municipio; [Tag("cPaisPrestacao")] string? Pais; } // M7

public sealed class CodigoServico                        // cServ
{
    [Tag("cTribNac")]    public string? TributacaoNacional { get; set; }    // 6 dígitos
    [Tag("cTribMun")]    public string? TributacaoMunicipal { get; set; }   // 3 dígitos
    [Tag("xDescServ")]   public string? Descricao { get; set; }
    [Tag("cNBS")]        public string? Nbs { get; set; }
    [Tag("cIntContrib")] public string? CodigoInterno { get; set; }
}

public sealed class ComercioExterior                     // comExt
{
    [Tag("mdPrestacao")] public ModoPrestacao? ModoPrestacao { get; set; }               // 0 a 4
    [Tag("vincPrest")]   public VinculoPartes? Vinculo { get; set; }                     // 0 a 6, 9
    [Tag("tpMoeda")]     public string? Moeda { get; set; }
    [Tag("vServMoeda")]  public decimal? ValorMoeda { get; set; }
    [Tag("mecAFComexP")] public MecanismoFomentoPrestador? MecanismoPrestador { get; set; } // 00 a 08
    [Tag("mecAFComexT")] public MecanismoFomentoTomador? MecanismoTomador { get; set; }     // 00 a 26
    [Tag("movTempBens")] public MovimentacaoTemporariaBens? MovimentacaoBens { get; set; }  // 0 a 3
    [Tag("nDI")]         public string? DeclaracaoImportacao { get; set; }
    [Tag("nRE")]         public string? RegistroExportacao { get; set; }
    [Tag("mdic")]        public CompartilhamentoMdic? CompartilharMdic { get; set; }        // 0 Nao, 1 Sim
}

public sealed class LocacaoSublocacao                    // lsadppu, só na 1.00
{
    [Tag("categ")]    public CategoriaLocacao? Categoria { get; set; }     // 1 a 5
    [Tag("objeto")]   public ObjetoLocacao? Objeto { get; set; }           // 1 a 6
    [Tag("extensao")] public int? Extensao { get; set; }
    [Tag("nPostes")]  public int? Postes { get; set; }
}

public sealed class Obra                                 // obra
{
    [Tag("inscImobFisc")] public string? InscricaoImobiliaria { get; set; }
    [Tag("cObra")]        public string? Codigo { get; set; }               // um dos três (M7)
    [Tag("cCIB")]         public string? Cib { get; set; }
    [Tag("end")]          public EnderecoObra? Endereco { get; set; }
}

public sealed class AtividadeEvento                      // atvEvento
{
    [Tag("xNome")]    public string? Nome { get; set; }                     // desc na 1.00 (M10)
    [Tag("dtIni")]    public DateOnly? Inicio { get; set; }
    [Tag("dtFim")]    public DateOnly? Fim { get; set; }
    [Tag("idAtvEvt")] public string? Identificacao { get; set; }            // ou o Endereco (M7)
    [Tag("end")]      public EnderecoEvento? Endereco { get; set; }
}

public sealed class ExploracaoRodoviaria                 // explRod, só na 1.00
{
    [Tag("categVeic")]    public CategoriaVeiculo? Categoria { get; set; }  // 00 a 10
    [Tag("nEixos")]       public int? Eixos { get; set; }
    [Tag("rodagem")]      public TipoRodagem? Rodagem { get; set; }         // 1 Simples, 2 Dupla
    [Tag("sentido")]      public int? Sentido { get; set; }
    [Tag("placa")]        public string? Placa { get; set; }
    [Tag("codAcessoPed")] public string? CodigoAcesso { get; set; }
    [Tag("codContrato")]  public string? CodigoContrato { get; set; }
}

public sealed class InformacoesComplementares            // infoCompl
{
    [Tag("idDocTec")] public string? ResponsabilidadeTecnica { get; set; }  // ART, RRT, DRT
    [Tag("docRef")]   public string? DocumentoReferencia { get; set; }
    [Tag("xPed")]     public string? Pedido { get; set; }
    [Tag("gItemPed")] public ItensPedido? ItensPedido { get; set; }         // M9
    [Tag("xInfComp")] public string? Texto { get; set; }                    // a marca EMULADA: do emulador (5)
}
public sealed class ItensPedido { [Tag("xItemPed")] IList<string> Itens; }  // 1 a 99
```

Os valores e os tributos da DPS:

```csharp
public sealed class ValoresDps                           // valores (TCInfoValores)
{
    [Tag("vServPrest")]      public ValorServico Servico { get; set; } = new();
    [Tag("vDescCondIncond")] public Descontos? Descontos { get; set; }
    [Tag("vDedRed")]         public DeducaoReducao? DeducaoReducao { get; set; }
    [Tag("trib")]            public Tributacao Tributacao { get; set; } = new();
}
public sealed class ValorServico { [Tag("vReceb")] decimal? ValorRecebido; [Tag("vServ")] decimal? Valor; }
public sealed class Descontos    { [Tag("vDescIncond")] decimal? Incondicionado; [Tag("vDescCond")] decimal? Condicionado; }

public sealed class DeducaoReducao                       // vDedRed: um dos três (M7)
{
    [Tag("pDR")]        public decimal? Percentual { get; set; }
    [Tag("vDR")]        public decimal? Valor { get; set; }
    [Tag("documentos")] public DocumentosDeducao? Documentos { get; set; }
}
public sealed class DocumentosDeducao { [Tag("docDedRed")] IList<DocumentoDeducao> Itens; } // 1 a 1000 (M9)

public sealed class DocumentoDeducao                     // docDedRed
{
    [Tag("chNFSe")]   public string? ChaveNFSe { get; set; }                // um dos seis (M7)
    [Tag("chNFe")]    public string? ChaveNFe { get; set; }
    [Tag("NFSeMun")]  public NFSeMunicipal? NFSeMunicipal { get; set; }
    [Tag("NFNFS")]    public NotaPapel? NotaPapel { get; set; }
    [Tag("nDocFisc")] public string? NumeroDocumentoFiscal { get; set; }
    [Tag("nDoc")]     public string? NumeroDocumento { get; set; }
    [Tag("tpDedRed")] public TipoDeducao? Tipo { get; set; }                // 1 a 9, 99
    [Tag("xDescOutDed")]         public string? DescricaoOutra { get; set; }
    [Tag("dtEmiDoc")]            public DateOnly? Emissao { get; set; }
    [Tag("vDedutivelRedutivel")] public decimal? ValorDedutivel { get; set; }
    [Tag("vDeducaoReducao")]     public decimal? ValorUtilizado { get; set; }
    [Tag("fornec")]              public Pessoa? Fornecedor { get; set; }
}
public sealed class NFSeMunicipal { [Tag("cMunNFSeMun")] int? Municipio; [Tag("nNFSeMun")] string? Numero;
                                    [Tag("cVerifNFSeMun")] string? CodigoVerificacao; }
public sealed class NotaPapel     { [Tag("nNFS")] string? Numero; [Tag("modNFS")] string? Modelo; [Tag("serieNFS")] string? Serie; }

public sealed class Tributacao                           // trib
{
    [Tag("tribMun")] public TributacaoMunicipal Municipal { get; set; } = new();
    [Tag("tribFed")] public TributacaoFederal? Federal { get; set; }
    [Tag("totTrib")] public TotalTributos Total { get; set; } = new();
}

public sealed class TributacaoMunicipal                  // tribMun (o ISSQN)
{
    [Tag("tribISSQN")]   public TributacaoIssqn? Tributacao { get; set; }       // 1 Tributavel, 2 Imunidade, 3 Exportacao, 4 NaoIncidencia (M12)
    [Tag("cPaisResult")] public string? PaisResultado { get; set; }
    [Tag("tpImunidade")] public TipoImunidade? Imunidade { get; set; }          // 0 a 5
    [Tag("exigSusp")]    public ExigibilidadeSuspensa? ExigibilidadeSuspensa { get; set; }
    [Tag("BM")]          public BeneficioMunicipal? Beneficio { get; set; }
    [Tag("tpRetISSQN")]  public RetencaoIssqn? Retencao { get; set; }           // 1 NaoRetido, 2 Tomador, 3 Intermediario
    [Tag("pAliq")]       public decimal? Aliquota { get; set; }
}
public sealed class ExigibilidadeSuspensa { [Tag("tpSusp")] TipoSuspensao? Tipo; [Tag("nProcesso")] string? Processo; } // 1 Judicial, 2 Administrativa
public sealed class BeneficioMunicipal
{
    [Tag("tpBM")]     public TipoBeneficioLei? Tipo { get; set; }               // só na 1.00 (M10)
    [Tag("nBM")]      public string? Identificador { get; set; }
    [Tag("vRedBCBM")] public decimal? ValorReducao { get; set; }                // um dos dois (M7)
    [Tag("pRedBCBM")] public decimal? PercentualReducao { get; set; }
}

public sealed class TributacaoFederal                    // tribFed
{
    [Tag("piscofins")] public PisCofins? PisCofins { get; set; }
    [Tag("vRetCP")]    public decimal? RetencaoCp { get; set; }
    [Tag("vRetIRRF")]  public decimal? RetencaoIrrf { get; set; }
    [Tag("vRetCSLL")]  public decimal? RetencaoCsll { get; set; }
}
public sealed class PisCofins
{
    [Tag("CST")]            public CstPisCofinsServico? Cst { get; set; }       // 00 a 09, 49 a 56, 60 a 67, 70 a 75, 98, 99
    [Tag("vBCPisCofins")]   public decimal? BaseCalculo { get; set; }
    [Tag("pAliqPis")]       public decimal? AliquotaPis { get; set; }
    [Tag("pAliqCofins")]    public decimal? AliquotaCofins { get; set; }
    [Tag("vPis")]           public decimal? ValorPis { get; set; }
    [Tag("vCofins")]        public decimal? ValorCofins { get; set; }
    [Tag("tpRetPisCofins")] public RetencaoPisCofins? Retencao { get; set; }    // 0 a 9 (NT 007)
}

public sealed class TotalTributos                        // totTrib: um dos quatro (M7)
{
    [Tag("vTotTrib")]   public TotalTributosValor? Valores { get; set; }
    [Tag("pTotTrib")]   public TotalTributosPercentual? Percentuais { get; set; }
    [Tag("indTotTrib")] public IndicadorTotalTributos? Indicador { get; set; }  // 0 NaoInformado (M6)
    [Tag("pTotTribSN")] public decimal? PercentualSimples { get; set; }
}
public sealed class TotalTributosValor      { [Tag("vTotTribFed")] decimal? Federal; [Tag("vTotTribEst")] decimal? Estadual; [Tag("vTotTribMun")] decimal? Municipal; }
public sealed class TotalTributosPercentual { [Tag("pTotTribFed")] decimal? Federal; [Tag("pTotTribEst")] decimal? Estadual; [Tag("pTotTribMun")] decimal? Municipal; }
```

O IBS e a CBS declarados na DPS:

```csharp
public sealed class IbsCbsDps                            // IBSCBS (TCRTCInfoIBSCBS)
{
    [Tag("finNFSe")]   public FinalidadeNFSe? Finalidade { get; set; }          // 0 Regular (M6)
    [Tag("indFinal")]  public ConsumoPessoal? ConsumoPessoal { get; set; }      // 0 Nao, 1 Sim
    [Tag("cIndOp")]    public string? CodigoOperacao { get; set; }              // 6 dígitos, Anexo C
    [Tag("tpOper")]    public TipoOperacaoGoverno? TipoOperacao { get; set; }   // 1 a 5
    [Tag("gRefNFSe")]  public NFSeReferenciadas? Referencias { get; set; }      // M9
    [Tag("tpEnteGov")] public EnteGovernamental? EnteGovernamental { get; set; }// 1 Uniao, 2 Estado, 3 DistritoFederal, 4 Municipio
    [Tag("indDest")]   public IndicadorDestinatario? IndicadorDestinatario { get; set; } // 0 Tomador, 1 Outro
    [Tag("dest")]      public Destinatario? Destinatario { get; set; }
    [Tag("imovel")]    public Imovel? Imovel { get; set; }
    [Tag("valores")]   public ValoresIbsCbs Valores { get; set; } = new();
}
public sealed class NFSeReferenciadas { [Tag("refNFSe")] IList<string> Chaves; }   // 1 a 99
/// TCRTCInfoDest: o conteúdo da Pessoa, com xNome de 150 e sem CAEPF e IM (M8).
public sealed class Destinatario { Cnpj, Cpf, Nif, MotivoAusenciaNif, Nome, Endereco (Endereco), Telefone, Email }
public sealed class Imovel { [Tag("inscImobFisc")] string? InscricaoImobiliaria; [Tag("cCIB")] string? Cib; [Tag("end")] EnderecoObra? Endereco; } // Cib ou Endereco

public sealed class ValoresIbsCbs                        // IBSCBS/valores
{
    [Tag("gReeRepRes")] public ReembolsoRepasse? ReembolsoRepasse { get; set; } // M9
    [Tag("trib")]       public TributosIbsCbs Tributos { get; set; } = new();
}
public sealed class ReembolsoRepasse { [Tag("documentos")] IList<DocumentoReembolso> Documentos; } // 1 a 1000

public sealed class DocumentoReembolso                   // gReeRepRes/documentos
{
    [Tag("dFeNacional")]    public DFeNacional? DFeNacional { get; set; }        // um dos três (M7)
    [Tag("docFiscalOutro")] public DocumentoFiscalOutro? DocumentoFiscal { get; set; }
    [Tag("docOutro")]       public DocumentoOutro? DocumentoNaoFiscal { get; set; }
    [Tag("fornec")]         public FornecedorDocumento? Fornecedor { get; set; }
    [Tag("dtEmiDoc")]       public DateOnly? Emissao { get; set; }
    [Tag("dtCompDoc")]      public DateOnly? Competencia { get; set; }
    [Tag("tpReeRepRes")]    public TipoReembolso? Tipo { get; set; }             // 01 a 04, 99
    [Tag("xTpReeRepRes")]   public string? DescricaoTipo { get; set; }
    [Tag("vlrReeRepRes")]   public decimal? Valor { get; set; }
}
public sealed class DFeNacional          { [Tag("tipoChaveDFe")] TipoDFe? Tipo; [Tag("xTipoChaveDFe")] string? DescricaoTipo; [Tag("chaveDFe")] string? Chave; } // 1 NFSe, 2 NFe, 3 CTe, 9 Outro
public sealed class DocumentoFiscalOutro { [Tag("cMunDocFiscal")] int? Municipio; [Tag("nDocFiscal")] string? Numero; [Tag("xDocFiscal")] string? Descricao; }
public sealed class DocumentoOutro       { [Tag("nDoc")] string? Numero; [Tag("xDoc")] string? Descricao; }
public sealed class FornecedorDocumento  { Cnpj, Cpf, Nif, MotivoAusenciaNif, [Tag("xNome")] Nome }

public sealed class TributosIbsCbs { [Tag("gIBSCBS")] public ClassificacaoIbsCbs Classificacao { get; set; } = new(); }
public sealed class ClassificacaoIbsCbs                  // gIBSCBS
{
    [Tag("CST")]          public string? Cst { get; set; }
    [Tag("cClassTrib")]   public string? ClassificacaoTributaria { get; set; }
    [Tag("cCredPres")]    public string? CreditoPresumido { get; set; }
    [Tag("gTribRegular")] public TributacaoRegularIbsCbs? TributacaoRegular { get; set; }
    [Tag("gDif")]         public DiferimentoIbsCbs? Diferimento { get; set; }
}
public sealed class TributacaoRegularIbsCbs { [Tag("CSTReg")] string? Cst; [Tag("cClassTribReg")] string? ClassificacaoTributaria; }
public sealed class DiferimentoIbsCbs       { [Tag("pDifUF")] decimal? PercentualUF; [Tag("pDifMun")] decimal? PercentualMunicipio; [Tag("pDifCBS")] decimal? PercentualCbs; }
```

### 7.4. A NFS-e lida

```csharp
/// NFSe + infNFSe (M3). O Id (a chave), a versao e as duas Signature ficam fora; a chave está no NFSeLida.
public sealed class NotaFiscalServico
{
    [Tag("xLocEmi")]       public string? NomeMunicipioEmissor { get; set; }
    [Tag("xLocPrestacao")] public string? NomeLocalPrestacao { get; set; }
    [Tag("nNFSe")]         public long? Numero { get; set; }
    [Tag("cLocIncid")]     public int? MunicipioIncidencia { get; set; }
    [Tag("xLocIncid")]     public string? NomeMunicipioIncidencia { get; set; }
    [Tag("xTribNac")]      public string? DescricaoTributacaoNacional { get; set; }
    [Tag("xTribMun")]      public string? DescricaoTributacaoMunicipal { get; set; }
    [Tag("xNBS")]          public string? DescricaoNbs { get; set; }
    [Tag("verAplic")]      public string? VersaoAplicativo { get; set; }
    [Tag("ambGer")]        public AmbienteGerador? AmbienteGerador { get; set; }      // 1 Prefeitura, 2 SistemaNacional
    [Tag("tpEmis")]        public TipoEmissaoNFSe? TipoEmissao { get; set; }         // 1 Normal, 2 Transcricao
    [Tag("procEmi")]       public ProcessoEmissao? ProcessoEmissao { get; set; }     // 1 Contribuinte, 2 FiscoWeb, 3 FiscoApp
    [Tag("cStat")]         public SituacaoNFSe? Situacao { get; set; }               // 100 Gerada, 102 DecisaoJudicial, 103 Avulsa, 107 Mei
    [Tag("dhProc")]        public DateTimeOffset? Processamento { get; set; }
    [Tag("nDFSe")]         public long? NumeroDfse { get; set; }
    [Tag("emit")]          public EmitenteNFSe Emitente { get; set; } = new();
    [Tag("valores")]       public ValoresNFSe Valores { get; set; } = new();
    [Tag("xOutInf")]       public string? InformacoesFisco { get; set; }             // valores/xOutInf na 1.00 (M10)
    [Tag("IBSCBS")]        public IbsCbsNFSe? IbsCbs { get; set; }
    [Tag("DPS")]           public Dps Dps { get; set; } = new();
}

public sealed class EmitenteNFSe                         // emit (TCEmitente)
{
    [Tag("CNPJ")]     public string? Cnpj { get; set; }
    [Tag("CPF")]      public string? Cpf { get; set; }
    [Tag("IM")]       public string? InscricaoMunicipal { get; set; }
    [Tag("xNome")]    public string? Nome { get; set; }
    [Tag("xFant")]    public string? NomeFantasia { get; set; }
    [Tag("enderNac")] public EnderecoEmitente Endereco { get; set; } = new();
    [Tag("fone")]     public string? Telefone { get; set; }
    [Tag("email")]    public string? Email { get; set; }
}
public sealed class EnderecoEmitente { Logradouro, Numero, Complemento, Bairro, [Tag("cMun")] int? Municipio, [Tag("UF")] UF? UF, [Tag("CEP")] string? Cep }

public sealed class ValoresNFSe                          // valores (TCValoresNFSe): o que a Sefin calculou
{
    [Tag("vCalcDR")]    public decimal? DeducaoReducao { get; set; }
    [Tag("tpBM")]       public TipoBeneficio? TipoBeneficio { get; set; }      // 1 Isencao, 2 ReducaoPercentual, 3 ReducaoValor, 4 AliquotaDiferenciada (M10)
    [Tag("vCalcBM")]    public decimal? ReducaoBeneficio { get; set; }
    [Tag("vBC")]        public decimal? BaseCalculo { get; set; }
    [Tag("pAliqAplic")] public decimal? AliquotaAplicada { get; set; }
    [Tag("vISSQN")]     public decimal? Issqn { get; set; }
    [Tag("vTotalRet")]  public decimal? TotalRetencoes { get; set; }
    [Tag("vLiq")]       public decimal? Liquido { get; set; }
}

public sealed class IbsCbsNFSe                           // IBSCBS (TCRTCIBSCBS): o que a Sefin calculou
{
    [Tag("cLocalidadeIncid")] public int? MunicipioIncidencia { get; set; }
    [Tag("xLocalidadeIncid")] public string? NomeMunicipioIncidencia { get; set; }
    [Tag("pRedutor")]         public decimal? PercentualRedutor { get; set; }
    [Tag("valores")]          public ValoresIbsCbsNFSe Valores { get; set; } = new();
    [Tag("totCIBS")]          public TotaisIbsCbs Totais { get; set; } = new();
}
public sealed class ValoresIbsCbsNFSe
{
    [Tag("vBC")]            public decimal? BaseCalculo { get; set; }
    [Tag("vCalcReeRepRes")] public decimal? ReembolsoRepasse { get; set; }
    [Tag("uf")]             public AliquotaIbsUF UF { get; set; } = new();
    [Tag("mun")]            public AliquotaIbsMunicipio Municipio { get; set; } = new();
    [Tag("fed")]            public AliquotaCbs Cbs { get; set; } = new();
}
public sealed class AliquotaIbsUF        { [Tag("pIBSUF")] decimal? Aliquota;  [Tag("pRedAliqUF")] decimal? Reducao;  [Tag("pAliqEfetUF")] decimal? Efetiva; }
public sealed class AliquotaIbsMunicipio { [Tag("pIBSMun")] decimal? Aliquota; [Tag("pRedAliqMun")] decimal? Reducao; [Tag("pAliqEfetMun")] decimal? Efetiva; }
public sealed class AliquotaCbs          { [Tag("pCBS")] decimal? Aliquota;    [Tag("pRedAliqCBS")] decimal? Reducao; [Tag("pAliqEfetCBS")] decimal? Efetiva; }

public sealed class TotaisIbsCbs                         // totCIBS
{
    [Tag("vTotNF")]         public decimal? TotalNota { get; set; }
    [Tag("gIBS")]           public TotalIbs Ibs { get; set; } = new();
    [Tag("gCBS")]           public TotalCbs Cbs { get; set; } = new();
    [Tag("gTribRegular")]   public TotalTributacaoRegular? TributacaoRegular { get; set; }
    [Tag("gTribCompraGov")] public TotalCompraGovernamental? CompraGovernamental { get; set; }
}
public sealed class TotalIbs
{
    [Tag("vIBSTot")]      public decimal? Total { get; set; }
    [Tag("gIBSCredPres")] public CreditoPresumidoIbs? CreditoPresumido { get; set; }
    [Tag("gIBSUFTot")]    public TotalIbsUF UF { get; set; } = new();
    [Tag("gIBSMunTot")]   public TotalIbsMunicipio Municipio { get; set; } = new();
}
public sealed class CreditoPresumidoIbs { [Tag("pCredPresIBS")] decimal? Aliquota; [Tag("vCredPresIBS")] decimal? Valor; }
public sealed class TotalIbsUF          { [Tag("vDifUF")] decimal? Diferimento;  [Tag("vIBSUF")] decimal? Valor; }
public sealed class TotalIbsMunicipio   { [Tag("vDifMun")] decimal? Diferimento; [Tag("vIBSMun")] decimal? Valor; }
public sealed class TotalCbs
{
    [Tag("gCBSCredPres")] public CreditoPresumidoCbs? CreditoPresumido { get; set; }
    [Tag("vDifCBS")]      public decimal? Diferimento { get; set; }
    [Tag("vCBS")]         public decimal? Valor { get; set; }
}
public sealed class CreditoPresumidoCbs { [Tag("pCredPresCBS")] decimal? Aliquota; [Tag("vCredPresCBS")] decimal? Valor; }
public sealed class TotalTributacaoRegular
{
    [Tag("pAliqEfeRegIBSUF")]  decimal? AliquotaIbsUF;        [Tag("vTribRegIBSUF")]  decimal? ValorIbsUF;
    [Tag("pAliqEfeRegIBSMun")] decimal? AliquotaIbsMunicipio; [Tag("vTribRegIBSMun")] decimal? ValorIbsMunicipio;
    [Tag("pAliqEfeRegCBS")]    decimal? AliquotaCbs;          [Tag("vTribRegCBS")]    decimal? ValorCbs;
}
public sealed class TotalCompraGovernamental
{
    [Tag("pIBSUF")] decimal? AliquotaIbsUF; [Tag("vIBSUF")] decimal? ValorIbsUF; [Tag("pIBSMun")] decimal? AliquotaIbsMunicipio;
    [Tag("vIBSMun")] decimal? ValorIbsMunicipio; [Tag("pCBS")] decimal? AliquotaCbs; [Tag("vCBS")] decimal? ValorCbs;
}
```

### 7.5. Os eventos (M11)

```csharp
/// O infPedReg. A base da 9.0, com o ambiente e a versão do aplicativo (M4). O Id (PRE + chave + tipo) e o xDesc ficam fora.
public abstract class EventoNFSe
{
    private protected EventoNFSe();
    [Tag("tpAmb")]     public Ambiente? Ambiente { get; set; }               // nulo = o da configuração
    [Tag("verAplic")]  public string? VersaoAplicativo { get; set; }         // nulo = "OpenDFe x.y.z"
    [Tag("dhEvento")]  public DateTimeOffset? Data { get; set; }
    [Tag("CNPJAutor")] public string? CnpjAutor { get; set; }                // um dos dois (M7)
    [Tag("CPFAutor")]  public string? CpfAutor { get; set; }
    [Tag("chNFSe")]    public string? Chave { get; set; }
    public abstract string Tipo { get; }                                     // "101101"
}

// Os de quem emite (enviados na 9.3):
[Tag("e101101")] CancelamentoNFSe            { MotivoCancelamento? Motivo (1 ErroEmissao, 2 ServicoNaoPrestado, 9 Outros); string? Descricao }
[Tag("e101103")] AnaliseFiscalCancelamento   { MotivoAnaliseFiscal? Motivo (1, 2, 9); string? Descricao }
// Os que a lib só lê (D9):
[Tag("e105102")] CancelamentoSubstituicao    { MotivoSubstituicao? Motivo; string? Descricao; string? ChaveSubstituta }
[Tag("e105104")] CancelamentoDeferido        { string? CpfAgente; string? ProcessoAdministrativo; MotivoDeferimento? Motivo (1); string? Descricao }
[Tag("e105105")] CancelamentoIndeferido      { string? CpfAgente; string? ProcessoAdministrativo; MotivoIndeferimento? Motivo (1, 2); string? Descricao }
[Tag("e202201")] ConfirmacaoPrestador        { }
[Tag("e203202")] ConfirmacaoTomador          { }
[Tag("e204203")] ConfirmacaoIntermediario    { }
[Tag("e205204")] ConfirmacaoTacita           { }
[Tag("e202205")] RejeicaoPrestador           { MotivoRejeicao? Motivo (1 a 5, 9); string? Descricao }
[Tag("e203206")] RejeicaoTomador             { idem }
[Tag("e204207")] RejeicaoIntermediario       { idem }
[Tag("e205208")] AnulacaoRejeicao            { string? CpfAgente; string? EventoRejeicao (idEvManifRej); string? Descricao }
[Tag("e305101")] CancelamentoOficio          { string? CpfAgente; string? ProcessoAdministrativo; string? DescricaoProcesso }
[Tag("e305102")] BloqueioOficio              { string? CpfAgente; EventoBloqueado? Evento (codEvento: e101101, e105102, e105104, e105105, e305101); string? Descricao }
[Tag("e305103")] DesbloqueioOficio           { string? CpfAgente; string? Bloqueio (idBloqOfic) }

/// O evento que a Sefin (ou o município, ou o ADN) registrou: o infEvento com o pedido dentro. O Id (EVT ...) e a Signature ficam fora.
public sealed class EventoNFSeRegistrado
{
    [Tag("verAplic")]     public string? VersaoAplicativo { get; set; }
    [Tag("ambGer")]       public AmbienteGeradorEvento? AmbienteGerador { get; set; } // 1 Municipio, 2 SefinNacional, 3 Adn
    [Tag("nSeqEvento")]   public int? Sequencia { get; set; }
    [Tag("dhProc")]       public DateTimeOffset? Processamento { get; set; }
    [Tag("nDFSe")]        public long? NumeroDfse { get; set; }
    [Tag("pedRegEvento")] public EventoNFSe? Pedido { get; set; }
}
```

As enumerações não listadas à parte têm o nome no comentário de cada campo; os membros seguem o texto do XSD (`MODELO.md`, decisão 8: sem "Sem" no nome, `Nenhum` onde o XSD diz "nenhum").

### 7.6. O que a 9.1 faz depois da aprovação

O modelo acima; o leiaute (`OpenDFe.Leiaute.NFSe.v101`) com os schemas 1.00 e 1.01 embutidos, a escrita da DPS (com o Id) e a leitura da NFS-e, da DPS, do pedido de evento e do evento, nas duas versões; as quatro formas do detector (D7) e o `Documento.NFSe` (D11); a `ChaveNFSe` e o `IdentificadorDps`; as seis regras da seção 4 (a `NFSE-CHAVE` só se o DV for confirmado com chaves reais da documentação ou de NFS-e publicadas; senão cai, como a 9.0 previu). Pronto quando: DPS de teste válidas no XSD e lidas de volta iguais; NFS-e de exemplo lidas.

## 8. Implementação (etapa 9.1, 2026-11-19)

O modelo da seção 7, como aprovado, mais o que a seção 3 deixou para a 9.1. A situação da etapa, com os testes e o que foi conferido, está no `PLANO.md`.

### 8.1. O que entrou

- **No núcleo:** `Documento.NFSe = 99` (D11); a `FormaLeitura` com o documento fixo (D7: o construtor novo, `CaminhoModelo` anulável e `Documento`); e a `FormaLeitura.OutrasVersoes` (8.2); o `DFe.Detectar` com as duas.
- **No modelo** (`OpenDFe.NFSe.Modelo`): as 71 classes e as 44 enumerações da seção 7, a base `EventoNFSe` com as 16 subclasses, o `EventoNFSeRegistrado`, a `ChaveNFSe` e o `IdentificadorDps`.
- **No leiaute** (`OpenDFe.Leiaute.NFSe.v101`, pacote `OpenDFe.NFSe`): o `LeiauteNFSe101` (`Instancia`, `Versao`, `Documentos`, `Declarar`, `Escrever`, `Ler`, `LerEvento`), a `DpsEscrita`, a `NFSeLida` e o `EventoLido`; os schemas 1.00 e 1.01 do pacote de produção (D2), com o código E1235; as regras da seção 4 (5 de 6, 8.2); e, por dentro, a escrita do pedido de evento (para a 9.3 e os testes).
- **Um mapa por classe** (`MapasNFSe`): os campos na ordem do XSD, com a tag, a propriedade, o tipo e as escolhas. A escrita, a leitura das duas versões e o caminho do problema até a propriedade andam pelo mesmo mapa, em vez de um método escrito à mão por grupo, como na NF-e. O mapa e as classes do modelo saem de uma especificação só, pelo gerador em `eng/GerarModeloNFSe` (Python, `python eng/GerarModeloNFSe/gerar.py .`); o `LeiauteNFSe101Testes` confere que o modelo e o mapa têm as mesmas tags. Trocar o pacote de schemas (a NT 009) é editar a especificação e gerar de novo.

### 8.2. Ajustes ao desenho (a conferir pelo usuário)

1. **A `NFSE-CHAVE` caiu, e a `ChaveNFSe` não confere o DV.** A documentação só diz "dígito verificador do código numérico" (guia do emissor público nacional web 1.2.1, 9.1, `NTS.md` 12.5). A chave de exemplo do guia (`3106200125123580000011223000000017302301958020816` + `0`) não bate com o módulo 11 da NF-e sobre as 49 posições (dá 7), nem sobre o código numérico (dá 1); a chave de exemplo de uma implementação aberta (a `nfelib`) bate com o da NF-e. Sem duas fontes que concordem, a leitura confere o tamanho, os dígitos, o ambiente gerador, o tipo de inscrição, os zeros do CPF, o mês e o número, e não o DV. A seção 3.2 dizia "`FormatException`: tamanho, caracteres ou DV".
2. **O `xmldsig-core-schema.xsd` da 1.01 vai embutido sem o bloco `DOCTYPE`.** O arquivo do pacote traz o DOCTYPE do W3C, com entidades que ele não usa, e o núcleo proíbe DTD na carga dos schemas (etapa 1.5, `SchemaXmlTestes.DtdNoSchemaEhProibido`). O resto do arquivo é byte a byte o do pacote; os dois SHA-256 estão no `NTS.md` 12.5. O da 1.00 não tem DTD e vai como está.
3. **`FormaLeitura.OutrasVersoes`** (membro novo do núcleo, não previsto na 3.1): o detector compara a versão do XML com a do leiaute, e o 1.01 lê também a 1.00 (D8). Sem isso, a NFS-e 1.00 não seria detectada.
4. **Achados da versão 1.00** (a comparação da abertura pulava os filhos das escolhas do XSD): na 1.00, o evento da atividade usa `id` onde a 1.01 usa `idAtvEvt` (vai para a mesma propriedade, como o `desc`); a rejeição e a anulação da rejeição (202205, 203206, 204207, 205208) têm os campos dentro de um grupo `infRej` / `infAnRej`, que a leitura atravessa; a obra põe a `inscImobFisc` dentro da escolha; o `cStat` aceita o 101, que a 1.01 tirou (entra como número, com aviso). Também o texto fixo `xDesc` dos eventos muda entre as versões (sem acento e sem o "Manifestação de NFS-e -" na 1.00); a leitura confere o da versão do XML.
5. **O `EventoNFSe` da seção 3.2** ganhou o `Ambiente`, a `VersaoAplicativo` e o `CnpjAutor`/`CpfAutor` no lugar de `Autor`, como a 7.5 aprovou; o Id do pedido é `PRE` + chave + tipo, sem número (M11).
6. **Os resultados:** `DpsEscrita` (`Xml`, `Id`, `Problemas`); `NFSeLida` (`Nota`, `Dps`, `Chave`, `IdentificadorDps`, `Versao`, `Xml`, `Problemas`), que lê também a DPS sozinha; `EventoLido` (`Evento`, `Registrado`, `Id`, `Versao`, `Xml`, `Problemas`). A seção 3.3 só dava os nomes.
7. **Os códigos dos problemas** da escrita e da leitura: `NFSE-EXCLUSIVO`, `NFSE-NUCLEO`, `NFSE-ID`, `NFSE-CARACTERE`, `NFSE-NAO-SUPORTADO`, `NFSE-XML`, `NFSE-RAIZ`, `NFSE-VERSAO`, `NFSE-CHAVE-ID`, `NFSE-VALOR`, `NFSE-ENUMERACAO`, `NFSE-DESCONHECIDO`, `NFSE-REPETIDO` e `NFSE-EVENTO` (`REGRAS.md` 20). O `NFSE-CHAVE-ID` (o Id ausente ou inválido na leitura) tem outro nome para não se confundir com a regra que caiu.

### 8.3. O que fica para depois

- A NT 009 (D2), quando entrar em produção: a especificação do gerador muda, e o modelo com ela.
- A `NFSE-CHAVE` e o DV da `ChaveNFSe`, quando houver chaves reais para conferir (o PFX, `PROJETO.md` 17).
- As operações: a emissão e as consultas (9.2), os eventos (9.3), a parametrização (9.4); o DANFSe (9.5). O `DFe.Criar` com `Documento.NFSe` ainda recusa (o atendimento por UF e o `Emissao`, D3): entra na 9.2.
