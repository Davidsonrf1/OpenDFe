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
// Substituído na 9.5 (seção 12): LeiauteNFSe101.Danfse(NFSeLida nota, OpcoesDanfse? opcoes = null), com o FormularioImpressao no núcleo.
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

## 9. Transporte e emissão (etapa 9.2, 2026-11-19)

O que as seções 2 e 3 desenharam para o REST, a emissão e as consultas, implementado e testado contra a Sefin emulada (E.7). A situação da etapa, com os testes e o que foi conferido, está no `PLANO.md`. O caminho do `ExecutarAsync` no REST está no `CONTRATOS.md` 3.

### 9.1. O que entrou

- **No núcleo:** `Destino.SefinNacional = 120` (sigla `SEFIN`) e `Destino.ADN = 121`; `MetodoRest`; `IOperacaoRest<,>`, `PedidoRest`, `RespostaRest` e a sobrecarga `DeclaracaoLeiaute.RegistrarOperacao(IOperacaoRest<,>)`; `RastroWS.StatusHttp`. Por dentro: o caminho REST do `ExecutarAsync`, o corpo em JSON com o XML em GZip e Base64 (`CorpoRest`), e o transporte REST no mesmo pool do SOAP (`TransporteSoap.EnviarRestAsync`, com a leitura da D5). O `DFe.Criar` aceita o leiaute sem atendimento declarado (a UF da configuração não é conferida contra ele), recusa a NFS-e com o tipo de emissão diferente de `Normal`, e não pede o envelope SOAP das URLs da `SefinNacional` e do `ADN`.
- **No leiaute:** a `EmissaoNFSe` (`POST /nfse`, com o `RetornoEmissaoNFSe`), a `ConsultaNFSe` (`GET /nfse/{chave}`, com o `RetornoConsultaNFSe`), a `ConsultaDps` (`GET` ou `HEAD /dps/{id}`, com o `RetornoConsultaDps`), a `MensagemSefin` (os itens de `erros` e `alertas`) e a `RecuperacaoEmissao`, com o `RecuperacaoNFSe`. As bases da Sefin nos dois ambientes (`https://sefin.nfse.gov.br/SefinNacional` e `https://sefin.producaorestrita.nfse.gov.br/SefinNacional`). A substituição é a `EmissaoNFSe` com a `Dps.Substituicao`, sem nada a mais.

### 9.2. Ajustes ao desenho (a conferir pelo usuário)

1. **A `RecuperacaoEmissao` devolve o `RecuperacaoNFSe`, e não um `ResultadoWS<RetornoConsultaNFSe>`.** São duas chamadas, e a segunda só acontece se a DPS gerou a NFS-e; quando não gerou, não há consulta da NFS-e para devolver (e o `ResultadoWS` só o núcleo cria). O `RecuperacaoNFSe` traz os dois resultados (`Dps` e `Nota`, nulo sem a NFS-e) e o que eles dizem: `Gerada` (guardar a NFS-e) e `NaoGerada` (a Sefin respondeu 404: reenviar a mesma DPS).
2. **O `CStat` do REST é o status HTTP** (201 gerada, 200 encontrada, 400 rejeitada, 404 não encontrada), e o `Motivo` é o primeiro erro (`"E0014: ... (complemento)"`), ou o texto do sucesso, ou `"HTTP <status>"`. O desenho não dizia o que ia no `LeituraWS`, que exige um número.
3. **Membros a mais:** `ConsultaNFSe.Chave`, `ConsultaDps.Id`, `EmissaoNFSe.Dps` (o desenho só mostrava o deste) e `MensagemSefin.ToString()`. O `RetornoConsultaNFSe` e o `RetornoConsultaDps`, que a seção 3.3 só nomeava: `Encontrada`, `Chave`, `Xml`, `Nota`, `Erros` e `Alertas`; `Existe`, `Chave` e `Erros`. A `Nota` dos retornos é a `NotaFiscalServico` (a `NFSe` da seção 3.3 virou esse nome na 9.1).
4. **A base da produção restrita é a sem `/API/`** (a das implementações que emitem lá); a outra entra pela `ConfiguracaoDFe.Urls`. A seção 6, item 1, pedia que a 9.2 "registrasse as duas": a tabela de URLs tem uma por ambiente e serviço, e a escolha fica para o PFX.
5. **O pedido sem corpo (GET, HEAD) nunca "pode ter sido processado"**: a consulta não muda nada na Sefin. O 5xx com JSON também é falha (D5 fala de 5xx sem distinguir o corpo): é a Sefin fora do ar, e o leiaute não o lê.
6. **A leitura do JSON não distingue maiúsculas e ignora o que não conhece**, porque os nomes vêm de uma implementação aberta e não do Swagger (seção 6, item 1); o `codigo` numérico é lido como texto. O sucesso (2xx) sem a chave ou sem a NFS-e, a chave inválida, o Base64 ou o GZip que não abrem e o campo com o tipo errado são `FormatException` (a resposta ilegível, com `PodeTerSidoProcessado` no POST).
7. **A sobrecarga do `RegistrarOperacao`** torna ambígua a chamada com o literal `null` (`RegistrarOperacao<P, R>(null!)`): quem fizer isso precisa do cast. Nenhum leiaute faz; só um teste do núcleo fazia, e ganhou o cast.

### 9.3. Decisões tomadas sem perguntar

- O REST no mesmo `TransporteSoap` (o nome ficou, por ser interno): o pool, os tempos e a classificação das falhas de rede são os mesmos; um certificado tem um handler só para o SOAP e o REST.
- O `Accept: application/json` em todo pedido REST; o `Content-Type: application/json; charset=utf-8` no POST.
- O caminho do `PedidoRest` recusa a barra no começo, o esquema, a consulta (`?`) e o fragmento (`#`): os valores vão no caminho, como as rotas da Sefin.
- A `NFSE-*` das regras e o schema da DPS rodam antes de transmitir, como na NF-e; a recusa local leva o XML assinado no rastro, sem a URL.

### 9.4. O que fica para depois

- **A produção restrita** (o PFX, `PROJETO.md` 17): a base com ou sem `/API/`, os nomes do JSON e os status de verdade (o Swagger só abre com o ICP-Brasil), a DPS da lib aceita lá.
- **O `dotnet test`** não acha os testes nesta máquina desde a E.7 (código 5, "Zero testes executados", em todos os projetos; o SDK é o 10.0.401, e o `xunit.v3` é o 4.0.2 desde a 0.1). A suíte foi conferida pelos executáveis de teste. Fora do escopo da etapa.

## 10. Eventos (etapa 9.3, 2026-11-19)

O registro dos eventos de quem emite e a consulta dos eventos, que as seções 2 (D9) e 3.3 desenharam, implementados e testados contra a Sefin emulada (E.8). A situação da etapa, com os testes e o que foi conferido, está no `PLANO.md`.

### 10.1. O que entrou

- **No leiaute** (o núcleo não mudou):

```csharp
public sealed class EnvioEventoNFSe : ParametroWS<RetornoEventoNFSe>       // POST /nfse/{chave}/eventos
{
    public EnvioEventoNFSe(EventoNFSe evento);   // ArgumentException: o evento não é de quem emite (101101, 101103)
    public EventoNFSe Evento { get; }
}
public sealed class RetornoEventoNFSe
{
    public bool Registrado { get; }                                          // há o Evento
    public EventoNFSeRetornado? Evento { get; }
    public IReadOnlyList<MensagemSefin> Erros { get; }
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}
public sealed class ConsultaEventosNFSe : ParametroWS<RetornoEventosNFSe>  // GET /nfse/{chave}/eventos[/{tipo}[/{seq}]]
{
    public ConsultaEventosNFSe(ChaveNFSe chave);
    public ChaveNFSe Chave { get; }
    public string? Tipo { get; init; }           // 6 dígitos (ArgumentException)
    public int? Sequencia { get; init; }         // 1 a 999 (ArgumentOutOfRangeException); só com o Tipo
}
public sealed class RetornoEventosNFSe
{
    public bool Encontrada { get; }                                          // 2xx, mesmo com a lista vazia
    public IReadOnlyList<EventoNFSeRetornado> Eventos { get; }
    public IReadOnlyList<MensagemSefin> Erros { get; }
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}
/// O evento que a Sefin devolve, no registro e na consulta, de qualquer autor.
public sealed class EventoNFSeRetornado
{
    public string Tipo { get; }
    public int? Sequencia { get; }
    public DateTimeOffset? Processamento { get; }
    public string Xml { get; }                                               // o evento, descompactado: o que se guarda
    public EventoNFSeRegistrado? Registrado { get; }                         // lido, com o pedido na subclasse do tipo
}
```

- **O JSON:** o pedido vai no campo `pedidoRegistroEventoXmlGZipB64` (de implementações abertas, `NTS.md` 12.4); a resposta é lida em `tipoEvento`, `numSeqEvento`, `dataHoraProcessamento`, `eventoXmlGZipB64` e, na consulta, na lista `eventos` com os mesmos campos (a leitura do emulador, E.8). Como na 9.2, sem distinguir maiúsculas e ignorando o que não conhece; o 2xx sem o evento, ou um item da lista sem o XML, é `FormatException` (a resposta ilegível).
- **A substituição** continua sendo a `EmissaoNFSe` com a `Dps.Substituicao`; o 105102 que a Sefin gera sobre a substituída se lê na `ConsultaEventosNFSe` dela, com a `CancelamentoSubstituicao.ChaveSubstituta`.

### 10.2. Ajustes ao desenho (a conferir pelo usuário)

1. **O `EventoNFSeRetornado`**, um tipo só para o evento que volta do registro e de cada item da consulta. A seção 3.3 só nomeava os retornos.
2. **O `EnvioEventoNFSe` recusa no construtor** o evento que não é de quem emite (`ArgumentException`), em vez de deixar para a montagem: o tipo já se sabe ali, e mandar o 105102 ou a manifestação à Sefin é erro de programa, não de dado.
3. **O `dhEvento` nulo vale o relógio da configuração**, no horário de Brasília, como no evento da NF-e (`OperacaoEvento`). A 7.5 só dizia o que o `tpAmb` e o `verAplic` nulos valem.
4. **A `Sequencia` sem o `Tipo`** é `ArgumentException` no `ExecutarAsync` (a montagem confere: a ordem dos `init` não é fixa); o tipo e o número fora da forma são recusados no `init`.
5. **O `CStat` é o status HTTP** (201 registrado, 200 encontrada, 400 rejeitado, 404 não encontrada) e o `Motivo`, o primeiro erro, como na 9.2 (9.2, item 2).

### 10.3. Decisões tomadas sem perguntar

- O tipo, o número e o processamento saem do XML lido (o que a Sefin assinou), e do JSON só sem ele; os problemas da leitura do XML vão nos problemas do resultado.
- A consulta pelo número sequencial aceita também o evento único no próprio objeto, fora da lista: nenhuma fonte dá a forma dessa resposta.
- O autor (`CnpjAutor`, `CpfAutor`) não é tirado do certificado: é de quem usa, como na DPS, e o schema aponta a falta.
- Sem uma `RecuperacaoEvento`: depois da queda no meio, a `ConsultaEventosNFSe` com o tipo diz se o evento foi registrado, e o reenvio do mesmo evento é recusado pela matriz (E0840 no cancelamento). Uma chamada só, que quem usa já tem.

### 10.4. O que fica para depois

- **A produção restrita** (o PFX, `PROJETO.md` 17): os eventos lá; o nome do campo da resposta e a forma da consulta, pelo Swagger.
- **A manifestação** (ao ADN, com a distribuição; D9) e o bloqueio e o desbloqueio por ofício, que a lib só lê.

## 11. A parametrização municipal (etapa 9.4)

Apresentado na abertura da 9.4 (2026-11-19), antes do código, porque cria API pública. **Aprovado pelo usuário em 2026-11-19, tudo pela recomendação** ("aprovado, tudo pela recomendação"); o que mudou na implementação está em 11.4. **O esboço da seção 3.3 não vale como está:** ele previa quatro consultas (convênio, parâmetros do serviço, retenções e benefícios, pelo código do serviço ou pelo CPF/CNPJ), e as rotas que uma implementação aberta usa de verdade no ADN (`NTS.md` 12.4, a nfse4j, que o emulador segue desde a E.8) são seis, com a competência e o número do benefício, e as retenções sem o CPF/CNPJ. As rotas `/parametros_municipais/...` do manual continuam sem bater com a base (D13).

### 11.1. Decisões

| # | Questão | Recomendação |
|---|---|---|
| P1 | Quais rotas | **As seis da nfse4j**, uma operação REST por rota, no destino `ADN`, serviço `"Parametrizacao"`, com as bases `https://adn.nfse.gov.br/parametrizacao` e `https://adn.producaorestrita.nfse.gov.br/parametrizacao` (`NTS.md` 12.4). A alíquota e o histórico são uma operação só (a competência nula é o histórico). Ficam **cinco parâmetros**. |
| P2 | O retorno (D13) | **Os campos que se conseguiu confirmar, tipados, e o JSON inteiro ao lado** (`Json`, um `JsonElement` clonado), em todos. A leitura não distingue maiúsculas e aceita os tipos misturados que a nfse4j mostra (`true`, `"1"`, `"S"`, `"Sim"`; o número em texto). O que não se lê **não** é resposta ilegível: o campo fica nulo e sai um aviso (`NFSE-PARAMETRO`), porque o JSON está ao lado. Só o corpo que não é JSON é ilegível. |
| P3 | O código do serviço | **O `cTribNac` (6 dígitos) e o complemento municipal opcional (3 dígitos, padrão `000`)**, que a lib formata como `XX.XX.XX.XXX`. Fora da forma, `ArgumentException` no construtor. |
| P4 | A competência | **`DateOnly`**, formatada `MM-DD-AAAA` no caminho (a forma da nfse4j). Obrigatória nas rotas que a têm; o dia vai como está. |
| P5 | O município | **`int`, o código IBGE** (7 dígitos, `ArgumentOutOfRangeException` fora disso), como o `Dps.MunicipioEmissor`. A UF dos dois primeiros não é conferida (a tabela é do núcleo da NF-e, e o ADN responde). |
| P6 | Encontrado e erro | **`Encontrado`** = 2xx; o 404 (município sem convênio, benefício que não há) e o 400 trazem a **`Mensagem`** (o campo `mensagem`). O `CStat` é o status HTTP, e o `Motivo`, a mensagem ou o texto do sucesso, como na 9.2. |
| P7 | As enumerações | **As do modelo da DPS**: o regime pelo `RegimeEspecialTributacao`, o tipo do benefício pelo `TipoBeneficio`, os tipos de retenção pelo `RetencaoIssqn`. O valor que a enumeração não tem fica nulo, com o aviso, e o JSON ao lado. |

### 11.2. Tipos e assinaturas (no leiaute, `OpenDFe.NFSe`)

```csharp
// GET /{municipio}/convenio
public sealed class ConsultaConvenio : ParametroWS<RetornoConvenio>
{
    public ConsultaConvenio(int municipio);
    public int Municipio { get; }
}
public sealed class RetornoConvenio
{
    public bool Encontrado { get; }
    public bool? AderenteAmbienteNacional { get; }
    public bool? AderenteEmissorNacional { get; }
    public string? Mensagem { get; }
    public JsonElement? Json { get; }
}

// GET /{municipio}/{servico}/{competencia}/aliquota, ou /{municipio}/{servico}/historicoaliquotas sem a competência
public sealed class ConsultaAliquota : ParametroWS<RetornoAliquota>
{
    public ConsultaAliquota(int municipio, string tributacaoNacional, string complemento = "000");
    public int Municipio { get; }
    public string Servico { get; }                       // "XX.XX.XX.XXX"
    public DateOnly? Competencia { get; init; }          // nula: o histórico
}
public sealed class RetornoAliquota
{
    public bool Encontrado { get; }
    public IReadOnlyList<VigenciaAliquota> Aliquotas { get; }   // as do serviço consultado, na ordem
    public string? Mensagem { get; }
    public JsonElement? Json { get; }
}
public sealed class VigenciaAliquota { public decimal? Aliquota { get; } public DateTime? Inicio { get; } public DateTime? Fim { get; } }

// GET /{municipio}/{servico}/{competencia}/regimes_especiais
public sealed class ConsultaRegimesEspeciais : ParametroWS<RetornoRegimesEspeciais>
{
    public ConsultaRegimesEspeciais(int municipio, string tributacaoNacional, DateOnly competencia, string complemento = "000");
}
public sealed class RetornoRegimesEspeciais
{
    public bool Encontrado { get; }
    public IReadOnlyList<RegimeEspecialMunicipal> Regimes { get; }
    public string? Mensagem { get; }
    public JsonElement? Json { get; }
}
public sealed class RegimeEspecialMunicipal
{
    public RegimeEspecialTributacao? Regime { get; }
    public int? Situacao { get; }                        // "sit"; o significado não tem fonte (o emulador manda 1, permitido)
    public DateTime? Inicio { get; }
    public DateTime? Fim { get; }
}

// GET /{municipio}/{competencia}/retencoes
public sealed class ConsultaRetencoes : ParametroWS<RetornoRetencoes>
{
    public ConsultaRetencoes(int municipio, DateOnly competencia);
}
public sealed class RetornoRetencoes
{
    public bool Encontrado { get; }
    public bool? Artigo6Habilitado { get; }              // "art6.habilitado": a retenção da LC 116, art. 6º
    public IReadOnlyList<RetencaoMunicipal> Retencoes { get; }   // "retMun"
    public string? Mensagem { get; }
    public JsonElement? Json { get; }
}
public sealed class RetencaoMunicipal
{
    public string? Descricao { get; }
    public IReadOnlyList<RetencaoIssqn> Tipos { get; }   // "tpRet"
    public IReadOnlyList<string> Responsaveis { get; }   // "respTrib[].insc": o CPF ou o CNPJ
}

// GET /{municipio}/{beneficio}/{competencia}/beneficio
public sealed class ConsultaBeneficio : ParametroWS<RetornoBeneficio>
{
    public ConsultaBeneficio(int municipio, string numero, DateOnly competencia);   // numero: 14 dígitos
}
public sealed class RetornoBeneficio
{
    public bool Encontrado { get; }
    public string? Numero { get; }
    public string? Descricao { get; }
    public TipoBeneficio? Tipo { get; }                  // "tpoBenef"
    public decimal? AliquotaDiferenciada { get; }        // "aliqDiferenc", no tipo 4
    public DateTime? Inicio { get; }
    public DateTime? Fim { get; }
    public string? Mensagem { get; }
    public JsonElement? Json { get; }
}
```

As datas vêm sem fuso (`2022-07-01T00:00:00`): `DateTime` com `Kind` `Unspecified`, e não `DateTimeOffset`. Os membros de cada parâmetro repetem os argumentos do construtor (`Municipio`, `Servico`, `Competencia`, `Numero`), como a `ConsultaNFSe.Chave`. Os construtores dos retornos e dos itens são internos.

### 11.3. O que a 9.4 faz depois da aprovação

O desenho acima no leiaute (o núcleo não muda: o `Destino.ADN` existe desde a 9.2); as bases do ADN declaradas; a leitura tolerante (P2) com o código `NFSE-PARAMETRO` no `REGRAS.md` 20; os testes sem rede (a montagem de cada caminho, a leitura com os tipos misturados, o que não se lê) e contra o ADN emulado (as seis rotas com a configuração dos municípios, o 404 do convênio inexistente e do benefício, o 400, a paralisação). Pendente até o PFX: as rotas, os nomes e os tipos pelo Swagger.

### 11.4. Implementação (2026-11-19)

O desenho de 11.2 como aprovado, nos arquivos `ConsultasParametrizacao.cs` (os parâmetros, os retornos e os handlers) e `LeituraParametro.cs` (as bases, a forma dos argumentos e a leitura tolerante). A situação, com os testes, está no `PLANO.md`. Sem mudança de assinatura; o que o desenho não dizia:

1. **O serviço que a resposta não traz pela chave consultada:** as alíquotas e os regimes vêm num objeto por serviço (`{"01.01.01.000": [...]}`). Se a chave consultada não está lá e há uma só, ela é lida, com o aviso; com várias, nenhuma, com o aviso.
2. **O 2xx sem o grupo** (`parametrosConvenio`, `aliquotas`...) é o aviso, com o `Encontrado` verdadeiro e os campos nulos; o corpo vazio no erro deixa o `Json` nulo e o `Motivo` em `"HTTP <status>"`.
3. **Os regimes:** um item por vigência de cada regime; o código da chave que não é número, ou que a enumeração não tem, fica com o `Regime` nulo e o aviso.
4. **As retenções:** o tipo fora do `RetencaoIssqn` sai da lista, com o aviso; o responsável sem o `insc` não entra.
5. **As datas com fuso**, se vierem, valem a hora local do fuso (`DateTimeOffset.DateTime`); as sem fuso, `Kind` `Unspecified`.
6. **O número em texto** aceita a vírgula decimal (`"3,00"`); o lógico aceita `true`/`false`, `1`/`0` e, em texto, `1`, `S`, `Sim`, `true`, `verdadeiro` e os contrários, sem distinguir maiúsculas.

## 12. O DANFSe (etapa 9.5): para aprovação

Apresentado na abertura da 9.5 (2026-11-19), antes do código, porque cria API pública no núcleo, no leiaute e no PDF. **Aprovado pelo usuário em 2026-11-19, tudo pela recomendação** ("aprovado pela recomendação"); o que mudou na implementação está em 12.5. A fonte é a NT SE/CGNFS-e 008/2026 v1.02 (14/07/2026), **conferida de novo na cópia do gov.br** (`NTS.md` 12.3): é a versão vigente, com o mesmo texto da cópia da Fenacon lida na 9.0.

**O esboço da seção 3.3 não vale como está:** ele previa `DanfseNFSe.Montar(NFSe nota, ...)`, mas não há tipo `NFSe` (a nota é a `NotaFiscalServico`, e a chave e o XML estão na `NFSeLida`), e o DANFE da 7.3 é um método do leiaute (`LeiauteNFe400.Danfe(NotaLida, ...)`). E a NT pede o que o modelo de impressão da 7.2 não expressa (12.1).

### 12.1. O que a NT pede e o modelo de impressão não tem

A NT 008 não é uma lista de campos com ordem livre, como o Anexo II do DANFE: é um **formulário de posição fixa**. "Deverá ser utilizado o modelo de DANFSe conforme Anexo I, devendo a disposição de campos obrigatoriamente obedecer ao disposto no respectivo anexo" (2.2.4), e o item 2.4.5 dá a posição e o tamanho de cada campo em centímetros. E ainda:

- **página única obrigatória** (2.2), A4 retrato, margem de 0,15 a 0,20 cm (2.2.2) e os campos a partir de 0,30 cm da borda;
- **fonte em pontos por papel**: título de bloco de 7 pt negrito em caixa alta, rótulo de 6 pt negrito, conteúdo de 7 pt, os rótulos da identificação de 7 pt, o "DANFSe v2.0" de 9 pt, o município de 8 pt (2.4); o DANFE da 7.4 dá 10 pt ao conteúdo;
- **sombreamento** cinza de 5% no cabeçalho, nos títulos dos blocos e em dois campos ("Emitente da NFS-e" e "Valor Líquido da NFS-e + IBS/CBS"), linhas de 0,5 pt e **borda da página de 1 pt** (2.2.3);
- **o título do bloco na primeira célula da primeira linha**, ao lado dos campos (Anexo I), e não numa linha própria como o `SecaoImpressao`;
- **o QR Code à direita dos campos de identificação**, em X 17,48 cm e Y 1,67 cm, com 1,52 cm no mínimo, e o texto de 3 linhas embaixo (2.4.3); o modelo empilha e não põe nada lado a lado (a lacuna que a 7.4 já registrou no DANFE, `IMPRESSAO.md` 3.4);
- **"NFS-e SEM VALIDADE JURÍDICA" em vermelho** sólido, 9 pt negrito, abaixo do título, na produção restrita (2.4.3); o modelo não tem cor;
- **a logomarca da NFS-e** colorida no canto esquerdo do cabeçalho (2.4.3); o modelo só tem a imagem de 1 bit;
- **a marca d'água** "CANCELADA" ou "SUBSTITUÍDA" em diagonal, normal (sem negrito), 50 pt no mínimo, cinza K35 (2.5.1 e 2.5.2); a do renderizador é negrito, `#E6E6E6`, até 64 pt.

### 12.2. Decisões

| # | Questão | Recomendação |
|---|---|---|
| G1 | A porta de entrada | **`LeiauteNFSe101.Danfse(NFSeLida nota, OpcoesDanfse? opcoes = null)`**, que devolve o `DocumentoImpressao`, como o `Danfe` da NF-e: lê a NFS-e lida (a 1.00 e a 1.01), não confere o schema nem as regras, sem estado e seguro em paralelo. O PDF sai do `RenderizadorPdf.Gerar`, como o DANFE. |
| G2 | Como cumprir o formulário de posição fixa | **Um elemento novo no núcleo, `FormularioImpressao`: quadros em posição absoluta (em mm) com o tamanho da fonte em pontos, o sombreamento, as bordas e a cor**, e o QR Code e a imagem também em posição. O DANFSe monta um documento com um só formulário, com as coordenadas do 2.4.5 da NT; o PDF o desenha como está. Cumpre o Anexo I à risca e a página única por construção. **Custo:** o elemento contraria a decisão 3 da 7.2 ("estrutural, sem fonte nem coordenada"), só nele; o ESC/POS não o imprime (o DANFSe é só A4). **Alternativa:** estender o modelo estrutural (título na célula, sombreamento no campo, cor no texto, uma linha de blocos lado a lado, tamanhos de fonte por documento) e o PDF empilhar, como no DANFE: mais tipos novos, posição aproximada ("Parcial", como o 3.8 do DANFE) e a página única só por corte de texto. |
| G3 | A logomarca da NFS-e | **Embutida no leiaute**, como recurso: o PNG oficial do gov.br (`Logo - NFS-e - Horizontal.png`, 1920 x 389, RGBA, 99 KB; SHA-256 em `NTS.md` 12.3), porque a NT a manda imprimir em todo DANFSe e a publica para isso. O formulário ganha a imagem em PNG (`ImagemFormulario`, os bytes como estão), que o PDF desenha colorida. **Alternativa:** quem usa passa o PNG nas opções e, sem ele, o quadro fica vazio (se o usuário preferir não redistribuir a marca do governo num repositório MIT). |
| G4 | O nome do município do prestador, do tomador, do destinatário e do intermediário | O XML só traz o código IBGE (`cMun`) no endereço nacional, e a NT manda "utilizar a descrição destes códigos". **Uma tabela interna dos municípios do IBGE no leiaute** (os 5.571, código e nome, de `servicodados.ibge.gov.br/api/v1/localidades/municipios`, com a data e o SHA-256 em `NTS.md`), sem API pública; o código fora da tabela sai como o código. A UF sai dos dois primeiros dígitos. O município do emitente, da prestação e da incidência vêm do próprio XML (`xLocEmi`, `xLocPrestacao`, `xLocIncid`). **Alternativa:** um `Func<int, string?>` nas opções, por conta de quem usa. |
| G5 | Cancelada e substituída | **`OpcoesDanfse.Cancelada` e `OpcoesDanfse.Substituida`**, como o `OpcoesDanfe.Cancelada`: a situação vem de quem usa (a `ConsultaEventosNFSe` traz o 101101 e o 105102); as duas juntas são `ArgumentException`. A marca d'água fica dentro do formulário, com o estilo da NT (G2), e não mexe na do DANFE. |
| G6 | A produção restrita | **"NFS-e SEM VALIDADE JURÍDICA" em vermelho, abaixo do título**, pelo `tpAmb` 2, e **sem marca d'água** (a NT não pede; o DANFE põe "SEM VALOR FISCAL" porque o Anexo II pede). |
| G7 | O canhoto | **`OpcoesDanfse.Canhoto`, verdadeiro por padrão** (o Anexo I o traz; "caso o emitente opte por não utilizar", 2.3.3), como o do DANFE. Sem ele, a altura vai às informações complementares. |
| G8 | As supressões (2.3 e as notas 1 a 5) | **Sempre que se aplicam, sem opção:** o bloco sem dados sai com a frase da NT ("TOMADOR/ADQUIRENTE DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e" e as outras três), com 0,32 cm; o destinatário igual ao tomador (`indDest`), com "O DESTINATÁRIO É O PRÓPRIO TOMADOR/ADQUIRENTE DA OPERAÇÃO"; as linhas da nota 5 (regime especial, benefício) somem quando todos os campos delas estão vazios; as da nota 1 (endereço e e-mail) ficam quando há dado. Os blocos de baixo sobem, e a altura que sobra vai à descrição do serviço e às informações complementares, na proporção das alturas do Anexo I. A linha do PIS e da COFINS (nota 6) só sai com a competência até 31/12/2026. |
| G9 | O texto que não cabe | **O limite de caracteres da NT, com reticências** (37, 77, 167, 1.297 e 1.997, o "Tam. do Campo" do 2.4.5; a linha dos totais aproximados dos tributos nunca se corta, nota 10), aplicado na montagem; e o PDF corta com reticências o que ainda passar da altura do quadro (a Lato não é de largura fixa). Nada passa para uma segunda página. |
| G10 | Os textos das enumerações | **As descrições do `xs:documentation` do XSD 1.01** (o `tpEmit`, o `cStat`, o `finNFSe`, o `opSimpNac`, o `regApTribSN`, o `tribISSQN`, o `regEspTrib`, o `tpImunidade`, o `tpSusp`, o `tpBM`, o `tpRetISSQN`, o `tpRetPisCofins`, o `ambGer` e o `tpAmb`), fixas no leiaute e cortadas pelo G9; o valor fora da lista sai como o código. O campo sem dado no XML sai com um traço (nota 12). |
| G11 | As fontes | **Como no DANFE:** a Lato por padrão, e uma família só pelo `OpcoesPdf.Fonte`. A NT pede a Arial nos rótulos e a Microsoft Sans Serif nos conteúdos, que não se embutem (licença); segue como a pendência 3 da seção 6, a mesma do DANFE. |
| G12 | O formato | Dinheiro `1.234,56` (o "1-15V2"), percentual com 2 casas, data `DD/MM/AAAA` e data e hora `DD/MM/AAAA hh:mm:ss` como o XML traz, sem converter o fuso; CNPJ, CPF, CEP, `cTribNac` `nn.nn.nn`, NBS `n.nnnn.nn.nn`: o `FormatadorImpressao` da NF-e, copiado para o leiaute da NFS-e (os leiautes não se referenciam). |

### 12.3. Tipos e assinaturas

**No núcleo** (`OpenDFe.Impressao`, versão menor: tipos novos, nada muda de assinatura):

```csharp
/// Um formulário de posição fixa: quadros com posição e tamanho em milímetros, a partir do canto superior esquerdo da
/// área útil do papel, e a fonte em pontos (o DANFSe, NT 008/2026). O único elemento do modelo com coordenada e fonte;
/// ocupa uma página inteira e nunca se divide.
public sealed class FormularioImpressao : ElementoImpressao
{
    public FormularioImpressao(IReadOnlyList<QuadroFormulario> quadros);   // a lista é copiada
    public IReadOnlyList<QuadroFormulario> Quadros { get; }
    public double BordaPt { get; init; }          // a borda da área útil (1 pt no DANFSe); 0 sem borda
    public string? MarcaAgua { get; init; }       // em diagonal, normal, 50 pt no mínimo, cinza K35, atrás dos quadros
}

[Flags] public enum BordaQuadro { Nenhuma = 0, Cima = 1, Baixo = 2, Esquerda = 4, Direita = 8, Todas = 15 }
public enum CorImpressao { Preto, Vermelho }

/// A base dos quadros: posição e tamanho em mm; o conjunto é fechado (private protected).
public abstract class QuadroFormulario
{
    public double XMm { get; }  public double YMm { get; }  public double LarguraMm { get; }  public double AlturaMm { get; }
    public bool Sombreado { get; init; }          // fundo cinza claro (5%)
    public BordaQuadro Bordas { get; init; }      // as linhas de 0,5 pt
}
public sealed class TextoFormulario : QuadroFormulario       // o rótulo em cima e o valor embaixo, ou só um dos dois
{
    public TextoFormulario(double xMm, double yMm, double larguraMm, double alturaMm, string? rotulo, string? valor);
    public string? Rotulo { get; }  public string? Valor { get; }
    public double TamanhoRotuloPt { get; init; } = 6;   public bool RotuloNegrito { get; init; } = true;
    public double TamanhoValorPt { get; init; } = 7;    public bool ValorNegrito { get; init; }
    public AlinhamentoImpressao Alinhamento { get; init; }
    public CorImpressao Cor { get; init; }
}
public sealed class QrCodeFormulario : QuadroFormulario      // o texto do QR Code (nível M, UTF-8), no lado menor do quadro
{
    public QrCodeFormulario(double xMm, double yMm, double ladoMm, string texto);
    public string Texto { get; }
}
public sealed class ImagemFormulario : QuadroFormulario      // PNG, encaixado no quadro sem distorcer
{
    public ImagemFormulario(double xMm, double yMm, double larguraMm, double alturaMm, ReadOnlyMemory<byte> png);
    public ReadOnlyMemory<byte> Png { get; }
}

public sealed class PapelImpressao
{
    public static PapelImpressao A4RetratoEstreito { get; }  // A4 em pé com margem de 1,5 mm (o mínimo do 2.2.2 da NT 008)
}
```

Os construtores recusam as medidas negativas ou zero, o quadro fora da área útil do papel não é conferido no núcleo (o formulário não conhece o papel) e sim no PDF, que lança `ArgumentException`. O renderizador de texto de teste (`TextoPlano`) e o PDF passam a desenhar o formulário; o PDF o põe na página com o QuestPDF (`Layers` e o deslocamento de cada quadro), com o texto cortado por linhas com reticências (G9).

**No leiaute** (`OpenDFe.NFSe`):

```csharp
public sealed class OpcoesDanfse
{
    public bool Canhoto { get; init; } = true;    // G7
    public bool Cancelada { get; init; }          // marca d'água "CANCELADA" (G5)
    public bool Substituida { get; init; }        // marca d'água "SUBSTITUÍDA" (G5)
}

public sealed class LeiauteNFSe101
{
    public DocumentoImpressao Danfse(NFSeLida nota, OpcoesDanfse? opcoes = null);
}
```

`ArgumentException` (a mensagem cita o campo, nunca o valor): a leitura sem a NFS-e (a DPS sozinha não tem DANFSe) ou sem a chave; `Cancelada` e `Substituida` juntas. O documento leva o `Titulo` `DANFSe NFS-e <número>` e o papel `A4RetratoEstreito`.

**No PDF** (`OpenDFe.Impressao.Pdf`): nada público novo; o `FormularioImpressao` entra no mapeamento do `CONTRATOS.md` 22.2.

### 12.4. O que a 9.5 faz depois da aprovação

O formulário no núcleo, com os testes (as validações, a cópia das listas, o conjunto fechado); o DANFSe no leiaute, quadro a quadro com as coordenadas do 2.4.5, a tabela dos municípios e a logomarca embutidas (G3, G4); o formulário no PDF. A conformidade item a item contra a NT, como o `IMPRESSAO.md` 2 e 3 fizeram com o Anexo II, numa seção nova do `IMPRESSAO.md`. Os testes: os golden de texto (`TextoPlano`) de um catálogo de NFS-e (a completa, a mínima sem tomador, a com destinatário igual ao tomador, o intermediário, a não sujeita ao ISSQN, a substituta, a com IBS/CBS, a 1.00, a produção restrita, a cancelada, a substituída, os textos no limite); o PDF lido de volta pelo PdfPig (a página única, a posição e o tamanho de cada quadro contra a NT, as fontes em pontos, o sombreamento, a borda, o vermelho, o QR Code na posição com a matriz do QRCoder e a URL `https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=<chave>`, a marca d'água); a ponta a ponta com a NFS-e emitida pela Sefin emulada; e a inspeção visual dos PDFs. Fecha a fase: o README, o guia em `docs/guia/` e a situação.

### 12.5. Implementação (2026-11-19)

O desenho de 12.3 como aprovado, sem mudança de assinatura: no núcleo, `FormularioImpressao.cs` e o papel novo em `PapelImpressao.cs`; no PDF, `RenderizadorPdf.Formulario.cs` e as medidas em `Medidas.cs`; no leiaute, a pasta `Impressao/` (`MontagemDanfse`, `OpcoesDanfse`, `FormatadorDanfse`, `TextosDanfse`, `MunicipiosIbge`) e a pasta `Recursos/` (a logomarca e a tabela dos municípios, embutidas). A conformidade item a item está em `IMPRESSAO.md` 5; a situação, com os testes, no `PLANO.md`. O que o desenho não dizia:

1. **As coordenadas.** A tabela do 2.4.5 mede a partir da borda do papel: os campos começam a 0,30 cm e vão até 20,70 cm, com a borda de 1 pt na margem de 0,15 cm (a folga de 0,15 cm dos dois lados). O papel é o A4 com margem de 1,5 mm, e os quadros saem em mm a partir da área útil (o cm da NT menos 0,15, vezes 10).
2. **As linhas.** Os blocos andam 0,6465 cm por linha, o passo médio da tabela (13 linhas de 4,34 a 12,74; 10 de 14,43 a 20,90), com 0,63 cm de quadro; o valor total, 0,69 com 0,67; a descrição tem no mínimo 0,64 (de 13,79 a 14,43). Cada posição da tabela sai com até 0,02 cm de diferença.
3. **O Anexo I vence a tabela onde diverge** (2.2.4): o regime de apuração do SN na coluna 2, e não em 10,51; o tipo de tributação do ISSQN na coluna 2 e o município da incidência nas colunas 3 e 4 (a tabela os põe em 0,30 e 5,41, a posição do título do bloco).
4. **A altura que sobra** (os blocos suprimidos, o canhoto ausente) vai à descrição do serviço e às informações complementares pela necessidade estimada de cada uma (150 caracteres por linha de 20,40 cm a 7 pt); sem caber as duas, na proporção da necessidade. A linha dos totais aproximados dos tributos é um quadro à parte, no pé, e nunca se corta (nota 10).
5. **O prestador** usa o documento da DPS quando ela tem algum (CNPJ, CPF ou NIF), e o nome, o endereço, o telefone, o e-mail e a inscrição do `emit` da NFS-e quando a DPS não os traz e o emitente é o prestador (NFSE-PRESTADOR: a Sefin os tira do cadastro). A inspeção visual achou o caso: a DPS mínima, de prestador CPF, saía com o CNPJ do `emit`.
6. **As supressões:** a linha do endereço e do e-mail (nota 1) some quando os dois estão vazios; a linha da nota 5 do regime especial some sem regime (o "Nenhum", 0, não conta como dado), imunidade e suspensão; a do benefício, sem o tipo, o cálculo, as deduções e o desconto. A não incidência do ISSQN (nota 4) é o `tribISSQN` 4. O destinatário igual ao tomador (2.3.2) é o `indDest` 0 com o tomador.
7. **O 101 da versão 1.00** ("NFS-e de Substituição Gerada", que a 1.01 tirou do `TStat`) sai com o texto dele, e não com o código.
8. **O PDF não encolhe a fonte** no quadro justo: o respiro vertical encolhe antes; a redução ao caber só age se nem a primeira linha couber (o quadro pequeno demais de quem monta). Achado no teste: o texto de 9 pt num quadro de 4 mm saía com 8,62 pt.
9. **A linha divisória** é um quadro de 1 mm com a borda de cima, desenhado depois do bloco (o sombreado do título a cobriria); o conteúdo do quadro ocupa o quadro inteiro (`Extend`), porque a borda do QuestPDF acompanha o tamanho do conteúdo, e o quadro vazio não mostrava a linha.
10. **O título do documento** é `DANFSe NFS-e <número>`.
