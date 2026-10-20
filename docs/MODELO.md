# Modelo da NF-e e da NFC-e: cabeçalho e partes (etapa 2.2)

Desenho dos tipos públicos do modelo de dados (`PROJETO.md` 4) para tudo o que não é item, tributo nem total: a raiz da nota, a identificação com os documentos referenciados, o emitente, a emissão avulsa, o destinatário, a retirada e a entrega, os autorizados a obter o XML, o transporte, a cobrança, o pagamento, o intermediador, as informações adicionais, a exportação, a compra, a cana, o responsável técnico, a solicitação da NFF, os produtos agropecuários, o provedor de assinatura e as informações suplementares da NFC-e. **Aprovado em 2026-10-19** (seção 8): todas as decisões pela recomendação, salvo a 12, que ficou com a classe compartilhada (`Local`, `Veiculo`, `Observacao`). **Implementado em 2026-10-19** (etapa 2.2), com os ajustes da seção 10.

O arquivo se chama `MODELO.md`, e não `CABECALHO.md`, porque o desenho da 2.3 (itens, tributos e totais) entra nele como seções novas: as regras de forma (seção 3) valem para as duas etapas, e o modelo é um só.

Tudo no projeto `OpenDFe.NFe.Modelo`, namespace `OpenDFe.NFe` (2.1), no pacote `OpenDFe.NFe`. O critério é o do `CONTRATOS.md`: a menor superfície pública que serve à escrita (2.4), à leitura (2.5) e ao caminho do problema até a propriedade (2.6), fácil de estender em versão menor (`PROJETO.md` 12.3), com a regra de nomes sem preposição.

**Conferido na fonte (2026-10-19):** a estrutura, as ocorrências, os tipos e as escolhas pelos XSDs embutidos do `PL_010f_v1.04` (`leiauteNFe_v4.00.xsd`, `tiposBasico_v4.00.xsd`, `DFeTiposBasicos_v1.00.xsd`); os identificadores do MOC (`B02`, `C06`...) no MOC 7.0, Anexo I (o PDF `moc7-anexo-i-leiaute-e-rv.pdf` do portal da SVRS), e os dos campos criados depois nas NTs do Portal Nacional: 2025.002 v1.52 (`B10a`, `B12a`, `B25.1`, `B25.2`, `B25d`, `BB01` a `BB05`, `BC01`, `BC02`, `C22`), 2023.004 v1.20 (`YA02a`, `YA03a` a `YA03d`, `YA07a`, `YA07b`, `Z13`), 2021.002 v1.12 (`ZE01`, `ZE02`), 2024.003 v1.10 (`ZF01` a `ZF08`) e 2026.001 v1.02b (`ZG01` a `ZG07`); o `BA02a` (`refNFeSig`) na regra BA02a-50 da NT 2023.002. As listas de `tPag` e `tBand`, que o XSD declara só pelo padrão, conferidas na implementação (seção 10 e `docs/NTS.md` 1.5): o `tPag` pela Tabela de Meios de Pagamento do Portal Nacional (04/03/2026); o `tBand` só pelo MOC 7.0, porque a tabela das operadoras do portal não foi achada.

## 1. Visão geral

```
NFe                           NotaFiscal
  infNFe (Id, versao)           (achatado na raiz: decisão 2)
    ide                         .Identificacao        Identificacao
      NFref*                      .Referencias          DocumentoReferenciado (abstrata) e 6 subclasses
      gCompraGov?                 .CompraGovernamental  CompraGovernamental
      gPagAntecipado?             .PagamentoAntecipado  PagamentoAntecipado
    emit                        .Emitente             Emitente, EnderecoEmitente
    avulsa?                     .Avulsa               FiscoEmitente
    dest?                       .Destinatario         Destinatario, EnderecoDestinatario
    retirada?, entrega?         .Retirada, .Entrega   Local (as duas: decisão 12)
    autXML*                     .AutorizadosXml       PessoaAutorizada
    det+                        .Itens                Item          (2.3)
    total                       .Total                Total         (2.3)
    transp                      .Transporte           Transporte e os grupos dele
    cobr?                       .Cobranca             Cobranca, Fatura, Duplicata
    pag                         .Pagamento            Pagamento, FormaPagamento, TransacaoEletronica
    infIntermed?                .Intermediador        Intermediador
    infAdic?                    .InformacoesAdicionais InformacoesAdicionais e os grupos dela
    exporta?, compra?, cana?    .Exportacao, .Compra, .Cana
    infRespTec?                 .ResponsavelTecnico   ResponsavelTecnico
    infSolicNFF?                .SolicitacaoNff       SolicitacaoNff
    agropecuario?               .Agropecuario         Agropecuario, Defensivo, GuiaTransito
    infPAA?                     .ProvedorAssinatura   ProvedorAssinatura, AssinaturaProvedor, ChavePublicaRsa
  infNFeSupl?                   .Suplementares        InformacoesSuplementares
  Signature                     (fora do modelo: decisão 20)
```

| Tipo | Papel |
|---|---|
| `NotaFiscal` | a raiz: uma NF-e ou uma NFC-e, o que se passa à autorização e o que a leitura devolve |
| `Identificacao` | o `ide`: número, série, datas, natureza, finalidade, indicadores |
| `DocumentoReferenciado` e as subclasses | cada `NFref`: NF-e, NF-e com sigilo, NF modelo 1/2, NF de produtor, CT-e, cupom fiscal |
| `Emitente`, `Destinatario`, `FiscoEmitente` | as partes; os endereços do emitente e do destinatário são tipos diferentes no XSD e ficam em classes diferentes |
| `Local`, `PessoaAutorizada` | os locais de retirada e de entrega (uma classe para os dois, decisão 12) e os autorizados a obter o XML |
| `Transporte` e os grupos | modalidade, transportador, retenção, veículo, reboques, volumes |
| `Cobranca`, `Pagamento` e os grupos | fatura e duplicatas; as formas de pagamento e a transação eletrônica |
| os outros grupos | intermediador, informações adicionais, exportação, compra, cana, responsável técnico, NFF, agropecuário, PAA, suplementares |
| as enumerações (2.12) | cada campo de domínio fechado, com o valor do XML |

## 2. Tipos públicos

Os blocos mostram só os membros públicos, sem corpo, com a documentação essencial; o comentário de cada propriedade é o `[Tag]` dela. **Todas as classes são `public sealed class` com construtor público sem parâmetros**, salvo as bases abstratas de escolha. "Opcional" no comentário é a ocorrência `0-1` do XSD.

### 2.1. `NotaFiscal`

```csharp
/// Uma NF-e (modelo 55) ou NFC-e (modelo 65), com tudo o que vai dentro do infNFe e do infNFeSupl.
/// Preenchida por quem emite e devolvida pela leitura. Mutável e sem conferência: o que está errado aparece
/// como Problema na montagem (schema, regras), nunca como exceção do modelo. Não é segura para alteração
/// concorrente; a lib nunca altera a nota que recebe.
public sealed class NotaFiscal
{
    [Tag("ide", "B01")]          public Identificacao Identificacao { get; set; } = new();
    [Tag("emit", "C01")]         public Emitente Emitente { get; set; } = new();
    [Tag("avulsa", "D01")]       public FiscoEmitente? Avulsa { get; set; }
    [Tag("dest", "E01")]         public Destinatario? Destinatario { get; set; }
    [Tag("retirada", "F01")]     public Local? Retirada { get; set; }
    [Tag("entrega", "G01")]      public Local? Entrega { get; set; }
    [Tag("autXML", "GA01")]      public IList<PessoaAutorizada> AutorizadosXml { get; } = new List<PessoaAutorizada>();
    [Tag("det", "H01")]          public IList<Item> Itens { get; } = new List<Item>();          // 2.3
    [Tag("total", "W01")]        public Total Total { get; set; } = new();                       // 2.3
    [Tag("transp", "X01")]       public Transporte Transporte { get; set; } = new();
    [Tag("cobr", "Y01")]         public Cobranca? Cobranca { get; set; }
    [Tag("pag", "YA01")]         public Pagamento Pagamento { get; set; } = new();
    [Tag("infIntermed", "YB01")] public Intermediador? Intermediador { get; set; }
    [Tag("infAdic", "Z01")]      public InformacoesAdicionais? InformacoesAdicionais { get; set; }
    [Tag("exporta", "ZA01")]     public Exportacao? Exportacao { get; set; }
    [Tag("compra", "ZB01")]      public Compra? Compra { get; set; }
    [Tag("cana", "ZC01")]        public AquisicaoCana? Cana { get; set; }
    [Tag("infRespTec", "ZD01")]  public ResponsavelTecnico? ResponsavelTecnico { get; set; }
    [Tag("infSolicNFF", "ZE01")] public SolicitacaoNff? SolicitacaoNff { get; set; }
    [Tag("agropecuario", "ZF01")] public Agropecuario? Agropecuario { get; set; }
    [Tag("infPAA", "ZG01")]      public ProvedorAssinatura? ProvedorAssinatura { get; set; }
    /// Irmão do infNFe no XML (NFe/infNFeSupl), não filho; a leitura preenche, a escrita da NFC-e o gera (decisão 15).
    [Tag("infNFeSupl", "ZX01")]  public InformacoesSuplementares? Suplementares { get; set; }
}

// 2.3: o conteúdo de cada item (prod, imposto, impostoDevol, infAdProd, obsItem, vItem, DFeReferenciado) e
// dos totais (ICMSTot, ISSQNtot, retTrib, ISTot, IBSCBSTot, vNFTot). Na 2.2, as duas classes entram vazias,
// para a raiz compilar; os nomes Item e Total são provisórios, decididos na 2.3. O nItem é a posição na
// lista (decisão 19).
public sealed class Item { }
public sealed class Total { }
```

### 2.2. `Identificacao` e o que fica dentro dela

```csharp
public sealed class Identificacao
{
    // null nos seis campos marcados com (N) = o valor do núcleo na escrita (decisão 6); a leitura sempre preenche.
    [Tag("cUF", "B02")]       public UF? UF { get; set; }                                  // (N) a UF da configuração
    [Tag("cNF", "B03")]       public int? CodigoNumerico { get; set; }                     // (N) sorteado na montagem
    [Tag("natOp", "B04")]     public string? NaturezaOperacao { get; set; }
    [Tag("mod", "B06")]       public Documento? Modelo { get; set; }                       // (N) o documento do núcleo
    [Tag("serie", "B07")]     public int? Serie { get; set; }
    [Tag("nNF", "B08")]       public int? Numero { get; set; }
    [Tag("dhEmi", "B09")]     public DateTimeOffset? Emissao { get; set; }
    [Tag("dhSaiEnt", "B10")]  public DateTimeOffset? SaidaEntrada { get; set; }
    [Tag("dPrevEntrega", "B10a")] public DateOnly? PrevisaoEntrega { get; set; }
    [Tag("tpNF", "B11")]      public TipoNota? Tipo { get; set; }
    [Tag("idDest", "B11a")]   public LocalDestino? LocalDestino { get; set; }
    [Tag("cMunFG", "B12")]    public int? MunicipioFatoGerador { get; set; }
    [Tag("cMunFGIBS", "B12a")] public int? MunicipioFatoGeradorIbsCbs { get; set; }
    [Tag("tpImp", "B21")]     public FormatoImpressao? FormatoImpressao { get; set; }
    [Tag("tpEmis", "B22")]    public TipoEmissao? TipoEmissao { get; set; }                // (N) o tipo de emissão da configuração
    // cDV (B23): fora do modelo; calculado na escrita, conferido na leitura.
    [Tag("tpAmb", "B24")]     public Ambiente? Ambiente { get; set; }                      // (N) o ambiente da configuração
    [Tag("finNFe", "B25")]    public FinalidadeEmissao? Finalidade { get; set; }
    [Tag("tpNFDebito", "B25.1")]  public TipoNotaDebito? TipoDebito { get; set; }
    [Tag("tpNFCredito", "B25.2")] public TipoNotaCredito? TipoCredito { get; set; }
    [Tag("indFinal", "B25a")] public IndicadorConsumidorFinal? ConsumidorFinal { get; set; }
    [Tag("indPres", "B25b")]  public PresencaComprador? Presenca { get; set; }
    [Tag("indIntermed", "B25c")] public IndicadorIntermediador? Intermediacao { get; set; }
    [Tag("cIndOp", "B25d")]   public string? CodigoLocalOperacao { get; set; }             // 6 dígitos (NT 2025.002)
    [Tag("procEmi", "B26")]   public ProcessoEmissao? Processo { get; set; }
    [Tag("verProc", "B27")]   public string? VersaoAplicativo { get; set; }
    // dhCont e xJust: a sequência opcional do XSD, sem elemento próprio (decisão 13).
    [Tag("dhCont", "B28")]    public DateTimeOffset? InicioContingencia { get; set; }      // (N) a Contingencia da configuração
    [Tag("xJust", "B29")]     public string? JustificativaContingencia { get; set; }       // (N) idem
    [Tag("NFref", "BA01")]    public IList<DocumentoReferenciado> Referencias { get; } = new List<DocumentoReferenciado>();
    [Tag("gCompraGov", "BB01")]     public CompraGovernamental? CompraGovernamental { get; set; }
    [Tag("gPagAntecipado", "BC01")] public PagamentoAntecipado? PagamentoAntecipado { get; set; }
}

/// Um NFref: exatamente uma das seis escolhas do XSD (decisão 10). Hierarquia fechada: só as seis.
public abstract class DocumentoReferenciado
{
    private protected DocumentoReferenciado();
}

public sealed class NFeReferenciada : DocumentoReferenciado
{
    [Tag("refNFe", "BA02")]     public string? Chave { get; set; }
}

/// A NF-e referenciada pela chave com o cNF zerado, para manter o sigilo dela.
public sealed class NFeSigilosaReferenciada : DocumentoReferenciado
{
    [Tag("refNFeSig", "BA02a")] public string? Chave { get; set; }
}

[Tag("refNF", "BA03")]                                      // [Tag] na classe: decisão 11
public sealed class NotaModelo1Referenciada : DocumentoReferenciado
{
    [Tag("cUF", "BA04")]   public UF? UF { get; set; }
    [Tag("AAMM", "BA05")]  public string? AnoMes { get; set; }                          // "AAMM", como no XML
    [Tag("CNPJ", "BA06")]  public string? Cnpj { get; set; }
    [Tag("mod", "BA07")]   public ModeloNotaReferenciada? Modelo { get; set; }
    [Tag("serie", "BA08")] public int? Serie { get; set; }
    [Tag("nNF", "BA09")]   public int? Numero { get; set; }
}

[Tag("refNFP", "BA10")]
public sealed class NotaProdutorReferenciada : DocumentoReferenciado
{
    [Tag("cUF", "BA11")]   public UF? UF { get; set; }
    [Tag("AAMM", "BA12")]  public string? AnoMes { get; set; }
    [Tag("CNPJ", "BA13")]  public string? Cnpj { get; set; }                            // CNPJ ou CPF (decisão 10)
    [Tag("CPF", "BA14")]   public string? Cpf { get; set; }
    [Tag("IE", "BA15")]    public string? InscricaoEstadual { get; set; }               // ou "ISENTO"
    [Tag("mod", "BA16")]   public ModeloNotaProdutor? Modelo { get; set; }
    [Tag("serie", "BA17")] public int? Serie { get; set; }
    [Tag("nNF", "BA18")]   public int? Numero { get; set; }
}

public sealed class CTeReferenciado : DocumentoReferenciado
{
    [Tag("refCTe", "BA19")] public string? Chave { get; set; }
}

[Tag("refECF", "BA20")]
public sealed class CupomFiscalReferenciado : DocumentoReferenciado
{
    [Tag("mod", "BA21")]  public ModeloCupomFiscal? Modelo { get; set; }
    [Tag("nECF", "BA22")] public int? NumeroEcf { get; set; }
    [Tag("nCOO", "BA23")] public int? Coo { get; set; }
}

/// Compra governamental (NT 2025.002, o TCompraGov do DFeTiposBasicos).
public sealed class CompraGovernamental
{
    [Tag("tpEnteGov", "BB02")] public EnteGovernamental? Ente { get; set; }
    [Tag("pRedutor", "BB03")]  public decimal? PercentualReducao { get; set; }
    [Tag("tpOperGov", "BB04")] public OperacaoGovernamental? Operacao { get; set; }
    [Tag("refDFeAnt", "BB05")] public IList<string> DocumentosAnteriores { get; } = new List<string>();   // 0-99 chaves
}

/// As NF-e de antecipação de pagamento que esta abate (NT 2025.002).
public sealed class PagamentoAntecipado
{
    [Tag("refNFe", "BC02")] public IList<string> Chaves { get; } = new List<string>();   // 1-99
}
```

### 2.3. `Emitente`, `FiscoEmitente`

```csharp
public sealed class Emitente
{
    [Tag("CNPJ", "C02")]      public string? Cnpj { get; set; }                         // CNPJ ou CPF (decisão 10)
    [Tag("CPF", "C02a")]      public string? Cpf { get; set; }
    [Tag("xNome", "C03")]     public string? Nome { get; set; }
    [Tag("xFant", "C04")]     public string? NomeFantasia { get; set; }
    [Tag("enderEmit", "C05")] public EnderecoEmitente Endereco { get; set; } = new();
    [Tag("IE", "C17")]        public string? InscricaoEstadual { get; set; }            // opcional desde a NT 2026.007; ou "ISENTO"
    [Tag("IEST", "C18")]      public string? InscricaoEstadualSubstituto { get; set; }
    [Tag("IM", "C19")]        public string? InscricaoMunicipal { get; set; }           // IM e CNAE: sequência opcional
    [Tag("CNAE", "C20")]      public string? Cnae { get; set; }
    [Tag("CRT", "C21")]       public RegimeTributario? RegimeTributario { get; set; }
    [Tag("ISUFEmit", "C22")]  public string? InscricaoSuframa { get; set; }
}

/// O TEnderEmi: só UF do Brasil, CEP obrigatório, país 1058.
public sealed class EnderecoEmitente
{
    [Tag("xLgr", "C06")]    public string? Logradouro { get; set; }
    [Tag("nro", "C07")]     public string? Numero { get; set; }
    [Tag("xCpl", "C08")]    public string? Complemento { get; set; }
    [Tag("xBairro", "C09")] public string? Bairro { get; set; }
    [Tag("cMun", "C10")]    public int? CodigoMunicipio { get; set; }
    [Tag("xMun", "C11")]    public string? Municipio { get; set; }
    [Tag("UF", "C12")]      public UF? UF { get; set; }
    [Tag("CEP", "C13")]     public string? Cep { get; set; }
    [Tag("cPais", "C14")]   public int? CodigoPais { get; set; }
    [Tag("xPais", "C15")]   public string? Pais { get; set; }
    [Tag("fone", "C16")]    public string? Telefone { get; set; }
}

/// O grupo avulsa: o fisco que emitiu a nota avulsa.
public sealed class FiscoEmitente
{
    [Tag("CNPJ", "D02")]    public string? Cnpj { get; set; }
    [Tag("xOrgao", "D03")]  public string? Orgao { get; set; }
    [Tag("matr", "D04")]    public string? MatriculaAgente { get; set; }
    [Tag("xAgente", "D05")] public string? NomeAgente { get; set; }
    [Tag("fone", "D06")]    public string? Telefone { get; set; }
    [Tag("UF", "D07")]      public UF? UF { get; set; }
    [Tag("nDAR", "D08")]    public string? NumeroDar { get; set; }
    [Tag("dEmi", "D09")]    public DateOnly? EmissaoDar { get; set; }
    [Tag("vDAR", "D10")]    public decimal? ValorDar { get; set; }
    [Tag("repEmi", "D11")]  public string? Reparticao { get; set; }
    [Tag("dPag", "D12")]    public DateOnly? PagamentoDar { get; set; }
}
```

### 2.4. `Destinatario`, locais e autorizados

```csharp
public sealed class Destinatario
{
    [Tag("CNPJ", "E02")]          public string? Cnpj { get; set; }                     // um dos três (decisão 10)
    [Tag("CPF", "E03")]           public string? Cpf { get; set; }
    [Tag("idEstrangeiro", "E03a")] public string? IdEstrangeiro { get; set; }           // "" = a tag vazia (decisão 5)
    [Tag("xNome", "E04")]         public string? Nome { get; set; }
    [Tag("enderDest", "E05")]     public EnderecoDestinatario? Endereco { get; set; }
    [Tag("indIEDest", "E16a")]    public IndicadorIEDestinatario? IndicadorIE { get; set; }
    [Tag("IE", "E17")]            public string? InscricaoEstadual { get; set; }
    [Tag("ISUF", "E18")]          public string? InscricaoSuframa { get; set; }
    [Tag("IM", "E18a")]           public string? InscricaoMunicipal { get; set; }
    [Tag("email", "E19")]         public string? Email { get; set; }
}

/// O TEndereco: aceita o exterior (UF "EX", decisão 9).
public sealed class EnderecoDestinatario
{
    [Tag("xLgr", "E06")]    public string? Logradouro { get; set; }
    [Tag("nro", "E07")]     public string? Numero { get; set; }
    [Tag("xCpl", "E08")]    public string? Complemento { get; set; }
    [Tag("xBairro", "E09")] public string? Bairro { get; set; }
    [Tag("cMun", "E10")]    public int? CodigoMunicipio { get; set; }                   // 9999999 no exterior
    [Tag("xMun", "E11")]    public string? Municipio { get; set; }                      // "EXTERIOR" no exterior
    [Tag("UF", "E12")]      public UF? UF { get; set; }
    /// O UF do XML é "EX". Com ele, UF fica null. Sem [Tag]: é outro valor do mesmo campo (decisão 9).
    public bool Exterior { get; set; }
    [Tag("CEP", "E13")]     public string? Cep { get; set; }
    [Tag("cPais", "E14")]   public int? CodigoPais { get; set; }
    [Tag("xPais", "E15")]   public string? Pais { get; set; }
    [Tag("fone", "E16")]    public string? Telefone { get; set; }
}

/// O TLocal: o local de retirada (F) e o de entrega (G). Uma classe para os dois lugares (decisão 12): o [Tag]
/// dos campos leva só a tag, sem o identificador do MOC, que muda com o lugar (xLgr é F03 na retirada e G03
/// na entrega); o identificador do grupo fica na propriedade da NotaFiscal (F01, G01).
public sealed class Local
{
    [Tag("CNPJ")]    public string? Cnpj { get; set; }                                  // CNPJ ou CPF
    [Tag("CPF")]     public string? Cpf { get; set; }
    [Tag("xNome")]   public string? Nome { get; set; }
    [Tag("xLgr")]    public string? Logradouro { get; set; }
    [Tag("nro")]     public string? Numero { get; set; }
    [Tag("xCpl")]    public string? Complemento { get; set; }
    [Tag("xBairro")] public string? Bairro { get; set; }
    [Tag("cMun")]    public int? CodigoMunicipio { get; set; }
    [Tag("xMun")]    public string? Municipio { get; set; }
    [Tag("UF")]      public UF? UF { get; set; }
    public bool Exterior { get; set; }
    [Tag("CEP")]     public string? Cep { get; set; }
    [Tag("cPais")]   public int? CodigoPais { get; set; }
    [Tag("xPais")]   public string? Pais { get; set; }
    [Tag("fone")]    public string? Telefone { get; set; }
    [Tag("email")]   public string? Email { get; set; }
    [Tag("IE")]      public string? InscricaoEstadual { get; set; }
}

public sealed class PessoaAutorizada
{
    [Tag("CNPJ", "GA02")] public string? Cnpj { get; set; }                             // CNPJ ou CPF
    [Tag("CPF", "GA03")]  public string? Cpf { get; set; }
}
```

### 2.5. `Transporte`

```csharp
public sealed class Transporte
{
    [Tag("modFrete", "X02")]   public ModalidadeFrete? Modalidade { get; set; }
    [Tag("transporta", "X03")] public Transportador? Transportador { get; set; }
    [Tag("retTransp", "X11")]  public RetencaoIcmsTransporte? Retencao { get; set; }
    // veicTransp + reboque, vagao ou balsa: escolha do XSD, propriedades exclusivas (decisão 10).
    [Tag("veicTransp", "X18")] public Veiculo? Veiculo { get; set; }
    [Tag("reboque", "X22")]    public IList<Veiculo> Reboques { get; } = new List<Veiculo>();       // 0-5
    [Tag("vagao", "X25a")]     public string? Vagao { get; set; }
    [Tag("balsa", "X25b")]     public string? Balsa { get; set; }
    [Tag("vol", "X26")]        public IList<Volume> Volumes { get; } = new List<Volume>();         // 0-5000
}

public sealed class Transportador
{
    [Tag("CNPJ", "X04")]   public string? Cnpj { get; set; }                            // CNPJ, CPF ou nenhum
    [Tag("CPF", "X05")]    public string? Cpf { get; set; }
    [Tag("xNome", "X06")]  public string? Nome { get; set; }
    [Tag("IE", "X07")]     public string? InscricaoEstadual { get; set; }
    [Tag("xEnder", "X08")] public string? Endereco { get; set; }                        // o endereço em texto
    [Tag("xMun", "X09")]   public string? Municipio { get; set; }
    [Tag("UF", "X10")]     public UF? UF { get; set; }
    public bool Exterior { get; set; }
}

public sealed class RetencaoIcmsTransporte
{
    [Tag("vServ", "X12")]    public decimal? ValorServico { get; set; }
    [Tag("vBCRet", "X13")]   public decimal? BaseCalculo { get; set; }
    [Tag("pICMSRet", "X14")] public decimal? Aliquota { get; set; }
    [Tag("vICMSRet", "X15")] public decimal? Valor { get; set; }
    [Tag("CFOP", "X16")]     public int? Cfop { get; set; }
    [Tag("cMunFG", "X17")]   public int? MunicipioFatoGerador { get; set; }
}

/// O TVeiculo: o veículo de tração (veicTransp, X19 a X21) e cada reboque (X23 a X25). Uma classe para os dois
/// (decisão 12), com o [Tag] só com a tag.
public sealed class Veiculo
{
    [Tag("placa")] public string? Placa { get; set; }
    [Tag("UF")]    public UF? UF { get; set; }
    public bool Exterior { get; set; }
    [Tag("RNTC")]  public string? Rntc { get; set; }
}

public sealed class Volume
{
    [Tag("qVol", "X27")]   public long? Quantidade { get; set; }                        // até 15 dígitos
    [Tag("esp", "X28")]    public string? Especie { get; set; }
    [Tag("marca", "X29")]  public string? Marca { get; set; }
    [Tag("nVol", "X30")]   public string? Numeracao { get; set; }
    [Tag("pesoL", "X31")]  public decimal? PesoLiquido { get; set; }
    [Tag("pesoB", "X32")]  public decimal? PesoBruto { get; set; }
    [Tag("lacres", "X33")] public IList<Lacre> Lacres { get; } = new List<Lacre>();
}

public sealed class Lacre
{
    [Tag("nLacre", "X34")] public string? Numero { get; set; }
}
```

### 2.6. `Cobranca`

```csharp
public sealed class Cobranca
{
    [Tag("fat", "Y02")] public Fatura? Fatura { get; set; }
    [Tag("dup", "Y07")] public IList<Duplicata> Duplicatas { get; } = new List<Duplicata>();   // 0-120
}

public sealed class Fatura
{
    [Tag("nFat", "Y03")]  public string? Numero { get; set; }
    [Tag("vOrig", "Y04")] public decimal? ValorOriginal { get; set; }
    [Tag("vDesc", "Y05")] public decimal? Desconto { get; set; }
    [Tag("vLiq", "Y06")]  public decimal? ValorLiquido { get; set; }
}

public sealed class Duplicata
{
    [Tag("nDup", "Y08")]  public string? Numero { get; set; }
    [Tag("dVenc", "Y09")] public DateOnly? Vencimento { get; set; }
    [Tag("vDup", "Y10")]  public decimal? Valor { get; set; }
}
```

### 2.7. `Pagamento`

```csharp
public sealed class Pagamento
{
    [Tag("detPag", "YA01a")] public IList<FormaPagamento> Formas { get; } = new List<FormaPagamento>();   // 1-100
    [Tag("vTroco", "YA09")]  public decimal? Troco { get; set; }
}

public sealed class FormaPagamento
{
    [Tag("indPag", "YA01b")] public IndicadorPagamento? Indicador { get; set; }
    [Tag("tPag", "YA02")]    public MeioPagamento? Meio { get; set; }
    [Tag("xPag", "YA02a")]   public string? Descricao { get; set; }
    [Tag("vPag", "YA03")]    public decimal? Valor { get; set; }
    [Tag("dPag", "YA03a")]   public DateOnly? Data { get; set; }
    // CNPJPag e UFPag: sequência opcional (decisão 13).
    [Tag("CNPJPag", "YA03c")] public string? CnpjTransacao { get; set; }
    [Tag("UFPag", "YA03d")]   public UF? UFTransacao { get; set; }
    [Tag("card", "YA04")]    public TransacaoEletronica? Transacao { get; set; }
}

/// O grupo card: cartões, PIX, boletos e outros pagamentos eletrônicos.
public sealed class TransacaoEletronica
{
    [Tag("tpIntegra", "YA04a")] public TipoIntegracao? Integracao { get; set; }
    [Tag("CNPJ", "YA05")]       public string? CnpjInstituicao { get; set; }
    [Tag("tBand", "YA06")]      public BandeiraCartao? Bandeira { get; set; }
    [Tag("cAut", "YA07")]       public string? Autorizacao { get; set; }
    [Tag("CNPJReceb", "YA07a")] public string? CnpjBeneficiario { get; set; }
    [Tag("idTermPag", "YA07b")] public string? Terminal { get; set; }
}
```

### 2.8. `Intermediador`, `InformacoesAdicionais`, `Exportacao`, `Compra`

```csharp
public sealed class Intermediador
{
    [Tag("CNPJ", "YB02")]         public string? Cnpj { get; set; }
    [Tag("idCadIntTran", "YB03")] public string? Identificador { get; set; }
}

public sealed class InformacoesAdicionais
{
    [Tag("infAdFisco", "Z02")] public string? Fisco { get; set; }
    [Tag("infCpl", "Z03")]     public string? Complementares { get; set; }
    [Tag("obsCont", "Z04")]    public IList<Observacao> ObservacoesContribuinte { get; } = new List<Observacao>();
    [Tag("obsFisco", "Z07")]   public IList<Observacao> ObservacoesFisco { get; } = new List<Observacao>();
    [Tag("procRef", "Z10")]    public IList<ProcessoReferenciado> Processos { get; } = new List<ProcessoReferenciado>();
}

/// O obsCont (Z05, Z06) e o obsFisco (Z08, Z09). Uma classe para os dois (decisão 12), com o [Tag] só com a tag.
public sealed class Observacao
{
    [Tag("xCampo")] public string? Campo { get; set; }                                  // o atributo
    [Tag("xTexto")] public string? Texto { get; set; }
}

public sealed class ProcessoReferenciado
{
    [Tag("nProc", "Z11")]   public string? Numero { get; set; }
    [Tag("indProc", "Z12")] public OrigemProcesso? Origem { get; set; }
    [Tag("tpAto", "Z13")]   public TipoAtoConcessorio? TipoAto { get; set; }
}

public sealed class Exportacao
{
    [Tag("UFSaidaPais", "ZA02")]  public UF? UFSaida { get; set; }
    [Tag("xLocExporta", "ZA03")]  public string? LocalEmbarque { get; set; }
    [Tag("xLocDespacho", "ZA04")] public string? LocalDespacho { get; set; }
}

public sealed class Compra
{
    [Tag("xNEmp", "ZB02")] public string? NotaEmpenho { get; set; }
    [Tag("xPed", "ZB03")]  public string? Pedido { get; set; }
    [Tag("xCont", "ZB04")] public string? Contrato { get; set; }
}
```

### 2.9. `AquisicaoCana`

```csharp
public sealed class AquisicaoCana
{
    [Tag("safra", "ZC02")]   public string? Safra { get; set; }                         // "AAAA" ou "AAAA/AAAA"
    [Tag("ref", "ZC03")]     public string? Referencia { get; set; }                    // "MM/AAAA"
    [Tag("forDia", "ZC04")]  public IList<FornecimentoDiario> Fornecimentos { get; } = new List<FornecimentoDiario>();   // 1-31
    [Tag("qTotMes", "ZC07")] public decimal? QuantidadeMes { get; set; }
    [Tag("qTotAnt", "ZC08")] public decimal? QuantidadeAnterior { get; set; }
    [Tag("qTotGer", "ZC09")] public decimal? QuantidadeGeral { get; set; }
    [Tag("deduc", "ZC10")]   public IList<Deducao> Deducoes { get; } = new List<Deducao>();   // 0-10
    [Tag("vFor", "ZC13")]    public decimal? ValorFornecimentos { get; set; }
    [Tag("vTotDed", "ZC14")] public decimal? ValorDeducoes { get; set; }
    [Tag("vLiqFor", "ZC15")] public decimal? ValorLiquido { get; set; }
}

public sealed class FornecimentoDiario
{
    [Tag("dia", "ZC05")]  public int? Dia { get; set; }                                 // o atributo
    [Tag("qtde", "ZC06")] public decimal? Quantidade { get; set; }
}

public sealed class Deducao
{
    [Tag("xDed", "ZC11")] public string? Descricao { get; set; }
    [Tag("vDed", "ZC12")] public decimal? Valor { get; set; }
}
```

### 2.10. Responsável técnico, NFF, agropecuário, PAA

```csharp
public sealed class ResponsavelTecnico
{
    [Tag("CNPJ", "ZD02")]     public string? Cnpj { get; set; }
    [Tag("xContato", "ZD04")] public string? Contato { get; set; }
    [Tag("email", "ZD05")]    public string? Email { get; set; }
    [Tag("fone", "ZD06")]     public string? Telefone { get; set; }
    // idCSRT e hashCSRT: sequência opcional (decisões 13 e 16).
    [Tag("idCSRT", "ZD08")]   public int? IdCsrt { get; set; }
    [Tag("hashCSRT", "ZD09")] public string? HashCsrt { get; set; }                     // base64, como no XML
}

public sealed class SolicitacaoNff
{
    [Tag("xSolic", "ZE02")] public string? Texto { get; set; }
}

public sealed class Agropecuario
{
    // defensivo ou guiaTransito: escolha do XSD, propriedades exclusivas (decisão 10).
    [Tag("defensivo", "ZF02")]    public IList<Defensivo> Defensivos { get; } = new List<Defensivo>();   // 1-20
    [Tag("guiaTransito", "ZF04")] public GuiaTransito? GuiaTransito { get; set; }
}

public sealed class Defensivo
{
    [Tag("nReceituario", "ZF03")] public string? Receituario { get; set; }
    [Tag("CPFRespTec", "ZF03a")]  public string? CpfResponsavel { get; set; }
}

public sealed class GuiaTransito
{
    [Tag("tpGuia", "ZF05")]    public TipoGuiaTransito? Tipo { get; set; }
    [Tag("UFGuia", "ZF06")]    public UF? UF { get; set; }
    [Tag("serieGuia", "ZF07")] public string? Serie { get; set; }
    [Tag("nGuia", "ZF08")]     public int? Numero { get; set; }
}

/// infPAA: a NF-e gerada por Provedor de Assinatura e Autorização (procEmi 4).
public sealed class ProvedorAssinatura
{
    [Tag("CNPJPAA", "ZG02")]      public string? Cnpj { get; set; }
    [Tag("PAASignature", "ZG03")] public AssinaturaProvedor Assinatura { get; set; } = new();   // obrigatório no infPAA (seção 10)
}

public sealed class AssinaturaProvedor
{
    [Tag("SignatureValue", "ZG04")] public string? Valor { get; set; }                  // base64
    [Tag("RSAKeyValue", "ZG05")]    public ChavePublicaRsa ChavePublica { get; set; } = new();    // idem
}

public sealed class ChavePublicaRsa
{
    [Tag("Modulus", "ZG06")]  public string? Modulo { get; set; }                       // base64
    [Tag("Exponent", "ZG07")] public string? Expoente { get; set; }                     // base64, "AQAB"
}

public sealed class InformacoesSuplementares
{
    [Tag("qrCode", "ZX02")]   public string? QrCode { get; set; }
    [Tag("urlChave", "ZX03")] public string? UrlChave { get; set; }
}
```

### 2.11. Os membros do núcleo que o modelo usa

`UF`, `Documento`, `Ambiente` e `TipoEmissao` (1.1), no namespace `OpenDFe`. O `TipoEmissao` não tem o `3` (NFF, decisão da 1.1): a leitura de uma NFF devolve `(TipoEmissao)3` (decisão 8).

### 2.12. Enumerações

O valor de cada membro é o do XML (`PROJETO.md` 4.3), escrito pelo leiaute com os dígitos do campo (`TipoNotaDebito.MultaJuros` = 4 sai `04`). Só o `ModeloCupomFiscal` tem valor do XML que não é número (decisão 8).

```csharp
public enum TipoNota { Entrada = 0, Saida = 1 }                                                    // tpNF
public enum LocalDestino { Interna = 1, Interestadual = 2, Exterior = 3 }                          // idDest
public enum FormatoImpressao                                                                       // tpImp
{
    Nenhum = 0, DanfeRetrato = 1, DanfePaisagem = 2, DanfeSimplificado = 3, DanfeNFCe = 4,
    DanfeNFCeMensagem = 5, DanfeSimplificadoTipo2 = 6,
}
public enum FinalidadeEmissao                                                                      // finNFe
{
    Normal = 1, Complementar = 2, Ajuste = 3, Devolucao = 4, NotaCredito = 5, NotaDebito = 6,
}
public enum TipoNotaDebito                                                                         // tpNFDebito
{
    TransferenciaCreditoCooperativa = 1, AnulacaoCreditoSaidaImune = 2, DebitoNaoProcessado = 3, MultaJuros = 4,
    TransferenciaCreditoSucessao = 5, PagamentoAntecipado = 6, PerdaEstoque = 7, DesenquadramentoSimples = 8,
}
public enum TipoNotaCredito                                                                        // tpNFCredito
{
    MultaJuros = 1, CreditoPresumidoZfm = 2, RetornoRecusaEntrega = 3, ReducaoValores = 4,
    TransferenciaCreditoSucessao = 5, RetornoRecusaParcial = 6,
}
public enum IndicadorConsumidorFinal { Normal = 0, ConsumidorFinal = 1 }                           // indFinal
public enum PresencaComprador                                                                      // indPres
{
    NaoAplicavel = 0, Presencial = 1, NaoPresencialInternet = 2, NaoPresencialTeleatendimento = 3,
    NaoPresencialEntrega = 4, PresencialExterno = 5, NaoPresencialOutros = 9,
}
public enum IndicadorIntermediador { PlataformaPropria = 0, PlataformaTerceiros = 1 }              // indIntermed
public enum ProcessoEmissao                                                                        // procEmi
{
    AplicativoContribuinte = 0, AvulsaFisco = 1, AvulsaContribuinteSite = 2, AplicativoFisco = 3, ProvedorAssinatura = 4,
}
public enum RegimeTributario { SimplesNacional = 1, SimplesExcessoSublimite = 2, Normal = 3, Mei = 4 }   // CRT
public enum IndicadorIEDestinatario { Contribuinte = 1, ContribuinteIsento = 2, NaoContribuinte = 9 }   // indIEDest
public enum ModalidadeFrete                                                                        // modFrete
{
    Remetente = 0, Destinatario = 1, Terceiros = 2, ProprioRemetente = 3, ProprioDestinatario = 4, NenhumTransporte = 9,
}
public enum IndicadorPagamento { Vista = 0, Prazo = 1 }                                            // indPag
public enum MeioPagamento                                                                          // tPag: a tabela do portal de 04/03/2026 (seção 10)
{
    Dinheiro = 1, Cheque = 2, CartaoCredito = 3, CartaoDebito = 4, CartaoLoja = 5,
    ValeAlimentacao = 10, ValeRefeicao = 11, ValePresente = 12, ValeCombustivel = 13, DuplicataMercantil = 14,
    BoletoBancario = 15, DepositoBancario = 16, PixDinamico = 17, Ted = 18, ProgramaFidelidade = 19,
    PixEstatico = 20, CreditoLoja = 21, PagamentoEletronicoNaoInformado = 22, PixAutomatico = 23, TefBookTransfer = 24,
    Nenhum = 90, PagamentoPosterior = 91, Outros = 99,
}
public enum TipoIntegracao { Integrado = 1, NaoIntegrado = 2 }                                     // tpIntegra
public enum BandeiraCartao                                                                         // tBand: a lista do MOC 7.0 (seção 10)
{
    Visa = 1, Mastercard = 2, AmericanExpress = 3, Sorocred = 4, DinersClub = 5, Elo = 6, Hipercard = 7,
    Aura = 8, Cabal = 9, Outros = 99,
}
public enum OrigemProcesso { Sefaz = 0, JusticaFederal = 1, JusticaEstadual = 2, SecexRfb = 3, Confaz = 4, Outros = 9 }   // indProc
public enum TipoAtoConcessorio                                                                     // tpAto
{
    TermoAcordo = 8, RegimeEspecial = 10, AutorizacaoEspecifica = 12, AjusteSinief = 14, ConvenioIcms = 15,
}
public enum EnteGovernamental                                                                      // tpEnteGov
{
    Uniao = 1, Estado = 2, DistritoFederal = 3, Municipio = 4, ConsorcioPublico = 5, ComiteGestorIbs = 6,
}
public enum OperacaoGovernamental                                                                  // tpOperGov
{
    FornecimentoPagamentoPosterior = 1, RecebimentoFornecimentoRealizado = 2,
    FornecimentoPagamentoRealizado = 3, RecebimentoFornecimentoPosterior = 4,
}
public enum TipoGuiaTransito { Gta = 1, Tta = 2, Dta = 3, Atv = 4, Ptv = 5, Gtv = 6, GuiaFlorestal = 7 }   // tpGuia
public enum ModeloNotaReferenciada { Modelo1 = 1, Modelo2 = 2 }                                    // refNF/mod: 01, 02
public enum ModeloNotaProdutor { NotaAvulsa = 1, NotaProdutor = 4 }                                // refNFP/mod: 01, 04
public enum ModeloCupomFiscal { MaquinaRegistradora = 1, Pdv = 2, Ecf = 3 }                        // refECF/mod: "2B", "2C", "2D"
```

## 3. Regras de forma (valem também para a 2.3)

1. **Um grupo do XML, uma classe; um campo, uma propriedade com `[Tag]`** (tag e identificador do MOC). O mesmo tipo do XSD em mais de um lugar é uma classe só, e o `[Tag]` dos campos dela leva só a tag (decisão 12); o identificador do grupo fica na propriedade que o guarda. Lista do XML, `IList<T>` com o `[Tag]` do elemento que se repete. Atributo do XML, propriedade com a tag sem `@` (`xCampo`, `dia`), como diz o `TagAttribute`.
2. **Classe selada, mutável (`get; set;`), construtor público sem parâmetros**; sem `record`, sem `struct`, sem igualdade de valor, sem `required`.
3. **Todo campo é anulável**; grupo opcional `null` = ausente; grupo obrigatório inicializado (`= new()`); lista nunca nula, inicializada, só leitura na propriedade.
4. **O modelo não confere nem normaliza nada:** sem exceção em setter, sem tirar máscara, sem aparar espaços. O valor vai ao XML como está (formatado pelo leiaute: decimais com as casas, datas com o fuso) e o schema e as regras apontam o erro, com a propriedade (2.6).
5. **Escolha do XSD:** valores simples alternativos ou grupos que dividem o pai com outros campos = propriedades exclusivas; grupos alternativos que são todo o conteúdo de um elemento repetido = subclasses de uma base abstrata fechada (`private protected`), com o `[Tag]` na classe quando a escolha é um grupo.
6. **Nomes:** descritivos, em português, sem preposição (`PLANO.md`, "Como trabalhar"); sem o nome do grupo repetido na propriedade (`Fatura.Numero`, não `Fatura.NumeroFatura`); siglas como palavra (`Cnpj`, `Cfop`, `Rntc`), salvo `UF` (o tipo do núcleo) e `NFe`/`NFCe` (como no `Documento`).

## 4. Mapeamento tag → propriedade

O caminho do XML (o do `Problema`, 1.5) e a propriedade (o `Problema.Propriedade`, 2.6). Os índices do XML começam em 1; os do modelo, em 0. O passo `infNFe` é transparente: a raiz `NotaFiscal` é o `NFe` e o `infNFe` juntos.

| Grupo | Caminho no XML | Propriedade | Tipo |
|---|---|---|---|
| B01 | `NFe/infNFe/ide` | `Identificacao` | `Identificacao` |
| B02-B29 | `.../ide/cUF`, `cNF`, `natOp`, `mod`, `serie`, `nNF`, `dhEmi`, `dhSaiEnt`, `dPrevEntrega`, `tpNF`, `idDest`, `cMunFG`, `cMunFGIBS`, `tpImp`, `tpEmis`, `tpAmb`, `finNFe`, `tpNFDebito`, `tpNFCredito`, `indFinal`, `indPres`, `indIntermed`, `cIndOp`, `procEmi`, `verProc`, `dhCont`, `xJust` | `Identificacao.UF`, `CodigoNumerico`, `NaturezaOperacao`, `Modelo`, `Serie`, `Numero`, `Emissao`, `SaidaEntrada`, `PrevisaoEntrega`, `Tipo`, `LocalDestino`, `MunicipioFatoGerador`, `MunicipioFatoGeradorIbsCbs`, `FormatoImpressao`, `TipoEmissao`, `Ambiente`, `Finalidade`, `TipoDebito`, `TipoCredito`, `ConsumidorFinal`, `Presenca`, `Intermediacao`, `CodigoLocalOperacao`, `Processo`, `VersaoAplicativo`, `InicioContingencia`, `JustificativaContingencia` | 2.2 |
| B23 | `.../ide/cDV` | (nenhuma: calculado) | |
| BA01 | `.../ide/NFref[n]` | `Identificacao.Referencias[n-1]` | `DocumentoReferenciado` |
| BA02, BA02a, BA19 | `.../NFref[n]/refNFe`, `refNFeSig`, `refCTe` | `Referencias[n-1].Chave` | `NFeReferenciada`, `NFeSigilosaReferenciada`, `CTeReferenciado` |
| BA03-BA09 | `.../NFref[n]/refNF/cUF` ... `nNF` | `Referencias[n-1].UF` ... `Numero` | `NotaModelo1Referenciada` |
| BA10-BA18 | `.../NFref[n]/refNFP/...` | `Referencias[n-1]...` | `NotaProdutorReferenciada` |
| BA20-BA23 | `.../NFref[n]/refECF/mod`, `nECF`, `nCOO` | `Referencias[n-1].Modelo`, `NumeroEcf`, `Coo` | `CupomFiscalReferenciado` |
| BB01-BB05 | `.../ide/gCompraGov/tpEnteGov`, `pRedutor`, `tpOperGov`, `refDFeAnt[n]` | `Identificacao.CompraGovernamental.Ente`, `PercentualReducao`, `Operacao`, `DocumentosAnteriores[n-1]` | `CompraGovernamental` |
| BC01-BC02 | `.../ide/gPagAntecipado/refNFe[n]` | `Identificacao.PagamentoAntecipado.Chaves[n-1]` | `PagamentoAntecipado` |
| C01-C22 | `NFe/infNFe/emit/CNPJ`, `CPF`, `xNome`, `xFant`, `IE`, `IEST`, `IM`, `CNAE`, `CRT`, `ISUFEmit` | `Emitente.Cnpj`, `Cpf`, `Nome`, `NomeFantasia`, `InscricaoEstadual`, `InscricaoEstadualSubstituto`, `InscricaoMunicipal`, `Cnae`, `RegimeTributario`, `InscricaoSuframa` | `Emitente` |
| C05-C16 | `.../emit/enderEmit/xLgr` ... `fone` | `Emitente.Endereco.Logradouro` ... `Telefone` | `EnderecoEmitente` |
| D01-D12 | `NFe/infNFe/avulsa/CNPJ` ... `dPag` | `Avulsa.Cnpj` ... `PagamentoDar` | `FiscoEmitente` |
| E01-E19 | `NFe/infNFe/dest/CNPJ`, `CPF`, `idEstrangeiro`, `xNome`, `indIEDest`, `IE`, `ISUF`, `IM`, `email` | `Destinatario.Cnpj`, `Cpf`, `IdEstrangeiro`, `Nome`, `IndicadorIE`, `InscricaoEstadual`, `InscricaoSuframa`, `InscricaoMunicipal`, `Email` | `Destinatario` |
| E05-E16 | `.../dest/enderDest/xLgr` ... `fone` (`UF` = `EX`: `Exterior`) | `Destinatario.Endereco.Logradouro` ... `Telefone` | `EnderecoDestinatario` |
| F01-F15 | `NFe/infNFe/retirada/CNPJ` ... `IE` | `Retirada.Cnpj` ... `InscricaoEstadual` | `Local` |
| G01-G15 | `NFe/infNFe/entrega/...` | `Entrega...` | `Local` |
| GA01-GA03 | `NFe/infNFe/autXML[n]/CNPJ`, `CPF` | `AutorizadosXml[n-1].Cnpj`, `Cpf` | `PessoaAutorizada` |
| H01 | `NFe/infNFe/det[n]` (e `@nItem`) | `Itens[n-1]` | `Item` (2.3) |
| W01 | `NFe/infNFe/total` | `Total` | `Total` (2.3) |
| X01-X02 | `NFe/infNFe/transp/modFrete` | `Transporte.Modalidade` | `Transporte` |
| X03-X10 | `.../transp/transporta/CNPJ` ... `UF` | `Transporte.Transportador.Cnpj` ... `UF` | `Transportador` |
| X11-X17 | `.../transp/retTransp/vServ` ... `cMunFG` | `Transporte.Retencao.ValorServico` ... `MunicipioFatoGerador` | `RetencaoIcmsTransporte` |
| X18-X21 | `.../transp/veicTransp/placa`, `UF`, `RNTC` | `Transporte.Veiculo.Placa`, `UF`, `Rntc` | `Veiculo` |
| X22-X25 | `.../transp/reboque[n]/placa`, `UF`, `RNTC` | `Transporte.Reboques[n-1].Placa`, `UF`, `Rntc` | `Veiculo` |
| X25a, X25b | `.../transp/vagao`, `balsa` | `Transporte.Vagao`, `Balsa` | `string` |
| X26-X32 | `.../transp/vol[n]/qVol` ... `pesoB` | `Transporte.Volumes[n-1].Quantidade` ... `PesoBruto` | `Volume` |
| X33-X34 | `.../transp/vol[n]/lacres[m]/nLacre` | `Transporte.Volumes[n-1].Lacres[m-1].Numero` | `Lacre` |
| Y01-Y06 | `NFe/infNFe/cobr/fat/nFat` ... `vLiq` | `Cobranca.Fatura.Numero` ... `ValorLiquido` | `Fatura` |
| Y07-Y10 | `.../cobr/dup[n]/nDup`, `dVenc`, `vDup` | `Cobranca.Duplicatas[n-1].Numero`, `Vencimento`, `Valor` | `Duplicata` |
| YA01, YA09 | `NFe/infNFe/pag`, `pag/vTroco` | `Pagamento`, `Pagamento.Troco` | `Pagamento` |
| YA01a-YA03d | `.../pag/detPag[n]/indPag`, `tPag`, `xPag`, `vPag`, `dPag`, `CNPJPag`, `UFPag` | `Pagamento.Formas[n-1].Indicador`, `Meio`, `Descricao`, `Valor`, `Data`, `CnpjTransacao`, `UFTransacao` | `FormaPagamento` |
| YA04-YA07b | `.../detPag[n]/card/tpIntegra`, `CNPJ`, `tBand`, `cAut`, `CNPJReceb`, `idTermPag` | `Pagamento.Formas[n-1].Transacao.Integracao`, `CnpjInstituicao`, `Bandeira`, `Autorizacao`, `CnpjBeneficiario`, `Terminal` | `TransacaoEletronica` |
| YB01-YB03 | `NFe/infNFe/infIntermed/CNPJ`, `idCadIntTran` | `Intermediador.Cnpj`, `Identificador` | `Intermediador` |
| Z01-Z03 | `NFe/infNFe/infAdic/infAdFisco`, `infCpl` | `InformacoesAdicionais.Fisco`, `Complementares` | `InformacoesAdicionais` |
| Z04-Z09 | `.../infAdic/obsCont[n]/@xCampo`, `xTexto` (e `obsFisco`) | `InformacoesAdicionais.ObservacoesContribuinte[n-1].Campo`, `Texto` (e `ObservacoesFisco`) | `Observacao` |
| Z10-Z13 | `.../infAdic/procRef[n]/nProc`, `indProc`, `tpAto` | `InformacoesAdicionais.Processos[n-1].Numero`, `Origem`, `TipoAto` | `ProcessoReferenciado` |
| ZA01-ZA04 | `NFe/infNFe/exporta/UFSaidaPais`, `xLocExporta`, `xLocDespacho` | `Exportacao.UFSaida`, `LocalEmbarque`, `LocalDespacho` | `Exportacao` |
| ZB01-ZB04 | `NFe/infNFe/compra/xNEmp`, `xPed`, `xCont` | `Compra.NotaEmpenho`, `Pedido`, `Contrato` | `Compra` |
| ZC01-ZC15 | `NFe/infNFe/cana/...`, `forDia[n]/@dia`, `qtde`, `deduc[n]/xDed`, `vDed` | `Cana...`, `Fornecimentos[n-1].Dia`, `Quantidade`, `Deducoes[n-1].Descricao`, `Valor` | `AquisicaoCana` e os grupos |
| ZD01-ZD09 | `NFe/infNFe/infRespTec/CNPJ`, `xContato`, `email`, `fone`, `idCSRT`, `hashCSRT` | `ResponsavelTecnico.Cnpj`, `Contato`, `Email`, `Telefone`, `IdCsrt`, `HashCsrt` | `ResponsavelTecnico` |
| ZE01-ZE02 | `NFe/infNFe/infSolicNFF/xSolic` | `SolicitacaoNff.Texto` | `SolicitacaoNff` |
| ZF01-ZF08 | `NFe/infNFe/agropecuario/defensivo[n]/nReceituario`, `CPFRespTec`; `guiaTransito/tpGuia`, `UFGuia`, `serieGuia`, `nGuia` | `Agropecuario.Defensivos[n-1].Receituario`, `CpfResponsavel`; `Agropecuario.GuiaTransito.Tipo`, `UF`, `Serie`, `Numero` | `Agropecuario` e os grupos |
| ZG01-ZG07 | `NFe/infNFe/infPAA/CNPJPAA`, `PAASignature/SignatureValue`, `PAASignature/RSAKeyValue/Modulus`, `Exponent` | `ProvedorAssinatura.Cnpj`, `Assinatura.Valor`, `Assinatura.ChavePublica.Modulo`, `Expoente` | `ProvedorAssinatura` e os grupos |
| ZX01-ZX03 | `NFe/infNFeSupl/qrCode`, `urlChave` | `Suplementares.QrCode`, `UrlChave` | `InformacoesSuplementares` |
| A02, A01 | `NFe/infNFe/@Id`, `@versao` | (nenhuma: a chave e a versão do leiaute) | |
| ZZ01 | `NFe/Signature` e o que vem dentro | (nenhuma: decisão 20) | |

**Como a 2.6 converte:** a partir de `NotaFiscal`, cada passo do caminho acha a propriedade do tipo atual com aquela tag (`[Tag]` em propriedade); `[n]` vira `[n-1]` na lista, e sem índice é o primeiro (1.5). Nos `NFref`, o passo da escolha (`refNF`, `refNFP`, `refECF`) é o `[Tag]` da subclasse (decisão 11): o caminho segue nas propriedades dela; nas escolhas de um valor só (`refNFe`, `refNFeSig`, `refCTe`), a propriedade `Chave` da subclasse é achada pela tag. `infNFe` não gera passo; `Signature`, `@Id`, `@versao` e `cDV` não têm propriedade (`Problema.Propriedade` nulo). `UF` com `EX` aponta a propriedade `UF`. Nas classes compartilhadas (`Local`, `Veiculo`, `Observacao`), o passo acha a propriedade pela tag, como nas outras; o identificador do MOC do campo (F03 ou G03) não vem do `[Tag]`, e o `Problema` aponta a propriedade pelo caminho (`Entrega.Logradouro`).

## 5. Exemplo de uso

```csharp
var nota = new NotaFiscal
{
    Identificacao =
    {
        NaturezaOperacao = "VENDA", Serie = 1, Numero = 123,
        Emissao = relogio.GetLocalNow(), Tipo = TipoNota.Saida, LocalDestino = LocalDestino.Interna,
        MunicipioFatoGerador = 3106200, FormatoImpressao = FormatoImpressao.DanfeNFCe,
        Finalidade = FinalidadeEmissao.Normal, ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal,
        Presenca = PresencaComprador.Presencial, Processo = ProcessoEmissao.AplicativoContribuinte,
        VersaoAplicativo = "PDV 1.0",
        // UF, Modelo, Ambiente, TipoEmissao, CodigoNumerico e contingência: do núcleo (decisão 6)
    },
    Emitente =
    {
        Cnpj = "12ABC34501DE35", Nome = "LOJA EXEMPLO LTDA", InscricaoEstadual = "0010000000000",
        RegimeTributario = RegimeTributario.SimplesNacional,
        Endereco = { Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200,
                     Municipio = "BELO HORIZONTE", UF = UF.MG, Cep = "30110000" },
    },
    Destinatario = new Destinatario { Cpf = "52998224725", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte },
    Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
    Pagamento =
    {
        Formas =
        {
            new FormaPagamento { Meio = MeioPagamento.PixDinamico, Valor = 50.00m,
                                 Transacao = new TransacaoEletronica { Integracao = TipoIntegracao.NaoIntegrado } },
        },
    },
};
nota.Itens.Add(item);                                     // 2.3
nota.Identificacao.Referencias.Add(new NFeReferenciada { Chave = chave });

// leitura (2.5): as escolhas pelo tipo
foreach (var r in lida.Identificacao.Referencias)
{
    var texto = r switch
    {
        NFeReferenciada n => n.Chave,
        CupomFiscalReferenciado c => $"ECF {c.NumeroEcf} COO {c.Coo}",
        _ => "outro",                                     // escolha nova em versão menor (decisão 17)
    };
}
```

## 6. Escrita, leitura e o caminho do problema (o que o desenho deixa pronto)

- **Escrita (2.4):** um método por classe, na ordem do XSD; `null` e texto vazio omitem a tag (salvo `IdEstrangeiro = ""`); os decimais com as casas do campo (`FormatadorInvariavel`, sem arredondar); `DateTimeOffset` com o deslocamento do valor; `DateOnly` como `AAAA-MM-DD`; enumerações pelo valor, com os zeros do campo. A escrita **não altera** a nota: o `cNF` sorteado, a chave e o `cDV` vão para o resultado (`PedidoWS`, `DocumentoPreparado`), não para o objeto. A exclusividade (CNPJ e CPF juntos) e a divergência com o núcleo (decisão 6) são `Erro` na montagem, com a propriedade. O aviso do campo não suportado (4.4) é o levantamento das propriedades `[Tag]` que o leiaute escreve contra as preenchidas.
- **Leitura (2.5):** preenche tudo o que reconhece, inclusive os seis campos do núcleo; valor de enumeração fora da lista entra como o número (decisão 8), com aviso; elemento desconhecido é problema, sem perder o XML original (5.3). A chave e o protocolo vêm no resultado da leitura, ao lado da nota.
- **Problema até a propriedade (2.6):** a seção 4, pela reflexão sobre o `[Tag]` (uma vez por tipo, guardada), com as regras de lá.

## 7. Concorrência, posse e extensibilidade

- A nota é de quem a cria; a lib só a lê, na montagem, e não guarda referência depois (o `DocumentoVerificado.Modelo` dos callbacks é a mesma instância, durante a chamada). Alterar a nota durante um `ExecutarAsync` é erro de quem chama.
- **Versão menor:** propriedade anulável nova (campo de NT), grupo opcional novo, classe nova, subclasse nova de uma escolha, membro novo de enumeração. **Versão maior:** renomear, mudar o tipo, tornar obrigatório (por isso nenhum `required`), tirar campo.
- As bases de escolha são fechadas (`private protected`): só a lib deriva, e o leiaute conhece todas.

## 8. Decisões (aprovadas em 2026-10-19)

Todas pela recomendação (a), salvo a 12, aprovada com a (b).

1. **Nome da raiz.** (a) **`NotaFiscal`**; (b) `Nota`; (c) `NotaFiscalEletronica`; (d) `DocumentoNFe` (colide no uso com o enum `Documento` do núcleo). Recomendo (a): descritivo, vale para a NF-e e a NFC-e (que também é nota fiscal), não colide com o namespace `OpenDFe.NFe` (2.1) nem com o núcleo; (b) é curto demais para um tipo público ao lado de `Item` e `Total`.
2. **Raiz achatada.** (a) **`NotaFiscal` é o `NFe` e o `infNFe` juntos; o `infNFeSupl` é uma propriedade dela; `Id`, `versao` e `Signature` ficam fora**; (b) espelho do XML (`nota.Informacoes.Identificacao`). Recomendo (a): o `infNFe` não tem campo próprio (só o `Id`, que é a chave, e a `versao`, que é do leiaute), e o caminho da 2.6 fica `Itens[2].Produto.Valor`, como o `PLANO.md` pede.
3. **Classe, mutabilidade e forma.** (a) **classe selada, `get; set;`, construtor sem parâmetros, sem `record` nem `required`**; (b) `init` (imutável depois do inicializador); (c) `record`. Recomendo (a): quem emite monta a nota aos poucos (o PDV acrescenta itens, o ERP mapeia do pedido) e a leitura preenche campo a campo; (b) impede o `nota.Identificacao.Numero = proximo` depois de criada; (c) publica `with`, `Deconstruct` e igualdade de valor numa classe mutável, o que a 1.2 já recusou. Sem `required` porque o XSD muda a obrigatoriedade por NT (a IE do emitente ficou opcional na NT 2026.007) e tornar `required` depois é versão maior.
4. **Coleções.** (a) **`IList<T> { get; } = new List<T>()`**; (b) `List<T> { get; set; } = []`; (c) `IReadOnlyList<T> { get; init; }`. Recomendo (a): nunca nula, aceita o inicializador (`Formas = { ... }`) e o `Add`, não esbarra no CA2227 nem no CA1002; (b) permite trocar a lista inteira, inclusive por `null`; (c) não serve a quem monta aos poucos. Os máximos do XSD (990 itens, 10 autorizados) não são impostos pela lista: o schema aponta.
5. **Nulidade e texto vazio.** (a) **todo campo simples anulável; grupo opcional `null` = ausente; grupo obrigatório inicializado; `null` e `""` omitem a tag, salvo `IdEstrangeiro = ""`, que escreve a tag vazia (o XSD aceita e o MOC a usa para o estrangeiro sem documento)**; (b) campos obrigatórios não anuláveis com valor padrão (`string Nome = ""`, `int Numero`); (c) `""` escrito como tag vazia sempre. Recomendo (a): a leitura de um XML incompleto devolve o que leu com problemas, sem inventar zero; o modelo vale para todas as versões (`PROJETO.md` 4.1); `PROJETO.md` 5.1 diz "tag vazia omitida". O custo é o `?` em toda leitura de quem consome.
6. **Os campos que o núcleo já sabe** (`cUF`, `mod`, `tpAmb`, `tpEmis`, `dhCont`, `xJust`) e o `cNF`. (a) **no modelo, anuláveis: `null` na escrita = o valor do núcleo (a configuração, `PROJETO.md` 8.4) e, no `cNF`, o sorteio da `ChaveAcesso.Gerar`; preenchido e diferente do núcleo = `Erro` na montagem; a leitura sempre preenche; o `cDV` fica fora (calculado na escrita, conferido na leitura)**; (b) fora do modelo, sempre do núcleo (a leitura os devolveria à parte); (c) obrigatórios no modelo, conferidos contra o núcleo. Recomendo (a): quem emite não repete a configuração, a leitura tem onde pôr o que leu, e o preenchido divergente vira problema em vez de ser ignorado calado (2.4). O `cNF` preenchido é respeitado (quem já gerou a chave, como o PDV offline, mantém a mesma).
7. **Tipos dos valores.** (a) **`decimal?` em valores, quantidades e percentuais; `DateTimeOffset?` nos campos `dh*`; `DateOnly?` nos `d*`; `int?` (ou `long?`) nos números sem zero à esquerda significativo (`nNF`, `serie`, `cMun`, `cPais`, `CFOP`, `idCSRT`, `nECF`, `nCOO`, `qVol`); `string?` onde o zero à esquerda ou a letra importam (CNPJ, CPF, IE, CEP, telefone, CNAE, `cIndOp`, placa) e nas chaves referenciadas (não `ChaveAcesso`); base64 como `string`; `AAMM`, `safra` e `ref` da cana como texto**; (b) tudo texto, como no XML; (c) chaves como `ChaveAcesso?`. Recomendo (a): segue o `PROJETO.md` 2.5 e a 1.1 (CPF e CNPJ como texto); a chave referenciada fica texto porque a leitura não pode falhar numa chave de terceiro com DV errado (a regra BA02-20 é "facultativa") e a `ChaveAcesso` recusa a inválida; o base64 como texto evita o CA1819 (`byte[]` em propriedade) e é o que está no XML.
8. **Enumerações.** (a) **sempre `enum` com o valor do XML, nunca `bool`, inclusive nos indicadores 0/1 (`indFinal`, `indIntermed`); a lista é aberta na leitura: o valor desconhecido entra como o número (`(MeioPagamento)77`), com aviso, e a escrita escreve o número que estiver; o único valor não numérico (`2B`, `2C`, `2D` do cupom fiscal) tem membros 1 a 3 e o leiaute traduz**; (b) `bool` nos indicadores 0/1; (c) texto nos códigos que as NTs ampliam (`tPag`, `tBand`, `indPres`). Recomendo (a): `bool` não ganha um terceiro valor depois sem versão maior (o `indPres` ganhou o 4 e o 5, o `tpImp` o 6); o enum ganha membro em versão menor. Os nomes evitam "Sem" (regra de nomes): `MeioPagamento.Nenhum` (90), `FormatoImpressao.Nenhum` (0), `ModalidadeFrete.NenhumTransporte` (9), `IndicadorIntermediador.PlataformaPropria` (0).
9. **UF com "EX"** (endereço do destinatário, retirada, entrega, transportador, veículos: o `TUf` aceita `EX`; o emitente, a exportação, o pagamento e a guia usam o `TUfEmi`, sem ele). (a) **`UF? UF` mais `bool Exterior` (sem `[Tag]`) nas classes do `TUf`**; (b) `string` com a sigla nessas classes; (c) um `EX` no `UF` do núcleo. Recomendo (a): continua tipado; (c) poria no núcleo uma "UF" que não é UF, e (b) perde o tipo nos casos comuns. O problema no `UF` aponta a propriedade `UF`.
10. **Escolhas do XSD (`xs:choice`).** (a) **valores alternativos e grupos que dividem o pai com outros campos viram propriedades exclusivas (CNPJ/CPF/`idEstrangeiro`; `veicTransp`+`reboque`/`vagao`/`balsa`; `defensivo`/`guiaTransito`), com `Erro` na montagem se mais de uma vier preenchida; a escolha que é todo o conteúdo de um elemento repetido (`NFref`) vira subclasses de uma base fechada; o pagamento não tem escolha no XSD (o `card` é opcional) e não ganha subclasse por meio**; (b) subclasses em todas (`DestinatarioPessoaFisica`...); (c) propriedades exclusivas em todas, também no `NFref`. Recomendo (a): CNPJ e CPF são o mesmo dado em duas tags, e a subclasse obrigaria a trocar o objeto para mudar de CPF para CNPJ; o `NFref` é uma lista de coisas diferentes, como os grupos de ICMS (`PROJETO.md` 4.3), e o `switch` por tipo é o natural; subclasse por meio de pagamento não acompanha o `tPag`, que muda por NT.
11. **`[Tag]` na classe.** (a) **o `TagAttribute` passa a valer também em classe (`AttributeTargets.Property | AttributeTargets.Class`), para a subclasse de escolha que é um grupo (`refNF`, `refNFP`, `refECF`; na 2.3, os grupos de ICMS)**; (b) só em propriedade, e o leiaute guarda à parte qual tag é qual subclasse. Recomendo (a): a 2.6 e a documentação acham a tag no próprio tipo, sem tabela paralela. Muda a decisão da 2.1 ("só propriedade; abrir depois é versão menor") sem custo: nada foi publicado e o `AttributeUsage` não está no `PublicAPI`.
12. **O mesmo tipo do XSD em dois lugares** (`TLocal` na retirada e na entrega, `TVeiculo` no `veicTransp` e no `reboque`, `obsCont` e `obsFisco`; o endereço do emitente e o do destinatário já são tipos diferentes no XSD). (a) **uma classe por lugar (`LocalRetirada`/`LocalEntrega`, `VeiculoTracao`/`Reboque`, `ObservacaoContribuinte`/`ObservacaoFisco`), cada uma com os identificadores do MOC do seu lugar**; (b) uma classe compartilhada (`Local`, `Veiculo`, `Observacao`), com um construtor novo do `[Tag]` só com a tag, sem identificador, nos campos dela; (c) classe compartilhada com o identificador do primeiro lugar. Recomendo (a): o identificador do MOC é diferente em cada lugar (`xLgr` é F03 na retirada e G03 na entrega), o `[Tag]` o exige, e um XSD futuro pode separar os dois. Custo: três pares de classes iguais e, para quem mapeia de um endereço próprio, duas atribuições. (c) é errado; (b) perde o identificador e é a alternativa se (a) parecer repetitiva demais. **Aprovada a (b):** `Local`, `Veiculo` e `Observacao`; o `TagAttribute` ganha o construtor `(string tag)`, e o `Identificador` passa a `string?` (nulo nesses campos). Se um XSD futuro separar os dois lugares, a classe nova entra em versão maior.
13. **Sequências sem elemento** (`dhCont`+`xJust`, `IM`+`CNAE`, `CNPJPag`+`UFPag`, `idCSRT`+`hashCSRT`). (a) **propriedades soltas no pai; o schema aponta o par incompleto**; (b) uma classe para o par (`Identificacao.Contingencia`, usando a `Contingencia` do núcleo). Recomendo (a): não há tag no XML para o grupo, e a 2.6 não teria passo para ele; a `Contingencia` do núcleo não tem `[Tag]` nem aceita o par incompleto que a leitura pode encontrar.
14. **A NFC-e.** (a) **o mesmo modelo; o que a NFC-e não usa (avulsa, retirada, cobrança, `NFref`, exportação, compra, cana, `dPrevEntrega`...) é regra da 3.3, com o código da rejeição; a escrita escreve o que estiver preenchido**; (b) um tipo próprio (`NotaFiscalConsumidor`) sem esses grupos; (c) a escrita recusa já na 2.4. Recomendo (a): `PROJETO.md` 3.1 ("o mesmo modelo de dados"), e as regras da NFC-e mudam por NT e por UF, o que um tipo não acompanha.
15. **`infNFeSupl` (QR Code e URL da consulta).** (a) **no modelo (`Suplementares`), preenchido pela leitura; na escrita, escrito como está até a 5.1, quando o leiaute passa a gerá-lo a partir do CSC da configuração; com o gerado, o preenchido e diferente é `Erro`**; (b) fora do modelo, só no resultado. Recomendo (a): a leitura precisa de onde pô-lo (o DANFCE de uma nota lida o imprime), e não decidir hoje o que a 5.1 vai precisar.
16. **CSRT do responsável técnico.** (a) **o modelo guarda o que está no XML (`IdCsrt`, `HashCsrt`); a conta do hash (SHA-1 do CSRT com a chave), que precisa da chave já montada e do CSRT secreto, fica para a fase 4, na configuração do documento**; (b) o CSRT secreto como propriedade do modelo. Recomendo (a): o segredo não pertence a um objeto que vai para callbacks e logs de quem usa; até a fase 4, quem informa o hash precisa informar também o `cNF` (decisão 6), para a chave ser a mesma.
17. **Extensibilidade.** (a) **campo de NT nova = propriedade anulável nova; grupo novo = classe nova; alternativa nova numa escolha = subclasse nova (quem faz `switch` precisa de `default`, documentado); valor novo = membro novo: tudo versão menor; as NTs 2026.006 (`gPgtoVinc`, no `pag`) e 2026.008 (item e totais) entram quando o pacote de schemas sair (`docs/NTS.md` 1.3), numa etapa própria**; (b) acrescentar já as propriedades das duas NTs sem schema. Recomendo (a): sem o XSD não há como validar nem conferir a ordem das tags; o campo preenchido de NT que o leiaute não escreve vira aviso (4.4).
18. **Sem conferência no modelo.** (a) **nenhuma exceção em setter, nenhuma normalização (máscara, espaços, maiúsculas); quem quer tira a máscara com `Cnpj.RemoverMascara`**; (b) setters que conferem e lançam; (c) a escrita tira a máscara. Recomendo (a): `PROJETO.md` 2.4 (problema não é exceção) e a 1.1 (o valor não muda calado); o schema aponta a máscara, com a propriedade (2.6).
19. **O encaixe da 2.3.** (a) **`NotaFiscal.Itens` (`IList<Item>`, `det`) e `NotaFiscal.Total` (`Total`, inicializado), com as classes vazias na 2.2 e os nomes provisórios; o `nItem` é a posição na lista (a escrita numera de 1 a n, a leitura confere a sequência)**; (b) propriedade `Numero` no item. Recomendo (a) para o encaixe; o `nItem` fica para a aprovação da 2.3, com (a) como recomendação: o MOC exige a sequência, e dois números para a mesma coisa só abririam espaço para divergir.
20. **Fora do modelo.** (a) **a `Signature`, o `protNFe` e o `nfeProc`: a nota autorizada é o XML (5.3) mais o protocolo, num tipo do resultado (2.5 e 4.4), não propriedades da `NotaFiscal`; a chave idem (ela sai do resultado, não do objeto)**; (b) `NotaFiscal.Protocolo` e `NotaFiscal.Chave` no modelo. Recomendo (a): o modelo é o que quem emite preenche; o protocolo e a assinatura são da SEFAZ e do certificado, e o objeto regerado não reproduz os bytes assinados (5.3).

### Divergências com o `PROJETO.md` e a 2.1

- `PLANO.md` 2.1, decisão do `[Tag]` "só propriedade": passa a valer também em classe (decisão 11).
- `PLANO.md` 2.1, `TagAttribute` com o identificador obrigatório: ganha o construtor só com a tag, e o `Identificador` fica anulável (decisão 12). Nada foi publicado; muda só o `PublicAPI.Unshipped.txt`.
- Nenhuma com o `PROJETO.md`: o 4.1 a 4.6 e o 5.1 continuam valendo; o 4.3 ("escolhas viram tipos") ganha o critério da decisão 10.

## 9. O que fica para depois

- **2.3:** o conteúdo de `Item` e `Total` (produto, grupos específicos, ICMS por CST e CSOSN como subclasses, IPI, II, PIS, COFINS, partilha, IBS, CBS, IS, os totais), os nomes definitivos das duas classes e o `nItem` (decisão 19). As regras de forma da seção 3 valem lá.
- **2.4:** a escrita (seção 6), com os testes contra o XSD; a numeração do `cNF` e a chave no resultado; o aviso do campo não suportado.
- **2.5:** a leitura e o detector; o tipo do resultado com a nota, a chave, o protocolo e o XML original; a migração do leitor do ErpCore (que usa `ide/mod`, `serie`, `nNF`, `dhEmi`, `tpNF`; `emit` e `dest` com `CNPJ`, `CPF`, `xNome`, `IE`; os totais e os itens), todos cobertos aqui ou na 2.3.
- **2.6:** a conversão do caminho (seção 4).
- **Fase 3:** as exclusividades e as regras da NFC-e como regras do MOC com o código da rejeição (a 2.4 já recusa as exclusividades na montagem).
- **Fase 4 e 5:** o hash do CSRT (decisão 16), o QR Code e a URL de consulta (decisão 15).
- **NTs sem pacote:** 2026.006 e 2026.008 (decisão 17).

## 10. Implementação (etapa 2.2, 2026-10-19)

- **Onde:** `src/OpenDFe.NFe.Modelo/`, um arquivo por tipo (50 classes e 25 enumerações), com a documentação XML e os 771 símbolos no `PublicAPI.Unshipped.txt`. O `TagAttribute` do núcleo ganhou o `AttributeTargets.Class`, o construtor `(string tag)` e o `Identificador` anulável (decisões 11 e 12).
- **Conferido contra o XSD pelo teste** (`tests/OpenDFe.NFe.Testes/ModeloNFeTestes.cs`), que percorre o `PL_010f_v1.04` compilado a partir do `NFe`: cada `[Tag]` existe no grupo certo; todo campo do XSD tem propriedade, salvo os da seção 4 que ficam fora (`Id`, `versao`, `Signature`, `cDV`) e o conteúdo de `det` e `total` (2.3); lista no modelo onde o XSD repete; grupo obrigatório inicializado e opcional nulo (a escolha conta como opcional); o `Exterior` exatamente onde a UF aceita `EX`; os valores de cada enumeração iguais aos do XSD; toda classe pública na árvore da nota; os identificadores do MOC sem repetição; e as regras de forma da seção 3. As mutações (uma tag trocada, um grupo opcional inicializado, um valor de enumeração mudado) foram conferidas: o teste acusa cada uma.
- **Ajustes ao desenho** (pelas regras da seção 3, sem decisão nova):
  - `ProvedorAssinatura.Assinatura` e `AssinaturaProvedor.ChavePublica` são grupos **obrigatórios** dentro do `infPAA` (o `PAASignature` e o `RSAKeyValue` não têm `minOccurs="0"`): inicializados (`= new()`) e não anuláveis, pela regra 3, e não `?` como no bloco da 2.10 da aprovação.
  - **`MeioPagamento`** pela Tabela de Meios de Pagamento do Portal Nacional (IT 2024.002 v1.11, 04/03/2026), e não pela do MOC 7.0: o 5 virou `CartaoLoja` ("Cartão da Loja (Private Label), Crediário Digital, Outros Crediários") e o `CreditoLoja` é o 21 ("Crédito em Loja"); o 17 virou `PixDinamico` e o 18 `Ted` (a tabela passou a dizer "TED"; o 24 é a transferência no mesmo banco, `TefBookTransfer`); entraram o 14 (`DuplicataMercantil`), 20 (`PixEstatico`), 22 (`PagamentoEletronicoNaoInformado`), 23 (`PixAutomatico`), 24 e 91 (`PagamentoPosterior`).
  - **`BandeiraCartao`** fica com a lista do MOC 7.0 (01 a 09 e 99): a tabela das operadoras que a NT 2020.006 cita não foi achada no portal (`docs/NTS.md` 1.5, pendente). O enum é aberto na leitura (decisão 8), e o código que falta entra em versão menor.

