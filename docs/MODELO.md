# Modelo da NF-e e da NFC-e: cabeçalho e partes (etapa 2.2), itens, tributos e totais (etapa 2.3)

Desenho dos tipos públicos do modelo de dados (`PROJETO.md` 4) para tudo o que não é item, tributo nem total: a raiz da nota, a identificação com os documentos referenciados, o emitente, a emissão avulsa, o destinatário, a retirada e a entrega, os autorizados a obter o XML, o transporte, a cobrança, o pagamento, o intermediador, as informações adicionais, a exportação, a compra, a cana, o responsável técnico, a solicitação da NFF, os produtos agropecuários, o provedor de assinatura e as informações suplementares da NFC-e. **Aprovado em 2026-10-19** (seção 8): todas as decisões pela recomendação, salvo a 12, que ficou com a classe compartilhada (`Local`, `Veiculo`, `Observacao`). **Implementado em 2026-10-19** (etapa 2.2), com os ajustes da seção 10. **Itens, tributos e totais (etapa 2.3): seção 11, aprovada em 2026-10-20 (todas as decisões pela recomendação) e implementada no mesmo dia (seção 12).**

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

- **2.3:** o conteúdo de `Item` e `Total` (produto, grupos específicos, ICMS por CST e CSOSN como subclasses, IPI, II, PIS, COFINS, partilha, IBS, CBS, IS, os totais), os nomes definitivos das duas classes e o `nItem` (decisão 19). As regras de forma da seção 3 valem lá. Desenho na seção 11.
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

## 11. Itens, tributos e totais (etapa 2.3): desenho aprovado em 2026-10-20

Desenho dos tipos públicos do que a 2.2 deixou vazio: o conteúdo de `Item` (`det`: o produto com os grupos específicos, os tributos, a devolução, as observações e a referência a outro documento) e de `Total` (`total`). **Aprovado em 2026-10-20**, com as 15 decisões da seção 11.12 pela recomendação; as regras de forma da seção 3 valem aqui, com as duas mudanças da 11.9. São 106 classes novas, as duas da 2.2 preenchidas, a `Observacao` reaproveitada e 27 enumerações (a conta da aprovação dizia 26; a lista da 11.8 tem 27), no mesmo projeto, namespace e pacote.

**Conferido na fonte (2026-10-20):** a estrutura, as ocorrências, os tipos e as escolhas pelo `PL_010f_v1.04` embutido (`leiauteNFe_v4.00.xsd`, `DFeTiposBasicos_v1.00.xsd`, `tiposBasico_v4.00.xsd`), percorrido por script a partir do `det` e do `total`: 739 campos e grupos, todos com propriedade, salvo o `@nItem` (decisão 21). Os identificadores do MOC, no MOC 7.0, Anexo I (novembro de 2020), e nas NTs que criaram os campos depois: 2025.002 v1.52 (IBS, CBS e IS: `I05k`, `I17c`, `UB01` a `UB134`, `VB01`, `VC01` a `VC03`, `W31` a `W60`), 2023.001 v1.60 (monofásico: `N02a`, `N03a`, `N07a`, `N08a`, `N37a` a `N48`, `LA18` a `LA21`, `W06b.1` a `W06e`), 2022.002 v1.30a (`N16a` a `N16d` e `N17.2` no ICMS90), 2023.004 v1.20 (`I23d1`, `N28b`), 2021.004 v1.35 (`VA01` a `VA07`), 2021.002 v1.12 (`I86`, `I87`), 2020.005 v1.21 (`I03a`, `I12a`, `N33a`, `N33b`, `N17d` a `N17f`, `R07`, `T07`) e 2019.001 v1.70 (`I05g` a `I05j`, `N14a`); as três últimas baixadas do Portal Nacional nesta etapa, as outras são as da 2.2. Os domínios das enumerações pelo XSD e, onde o XSD só traz o número, pelo MOC e pelas NTs (seção 11.8).

### 11.1. Visão geral

```
det+ (@nItem)                   Item                       (nItem = a posição: decisão 21)
  prod                            .Produto                 Produto
    gCred*, DI*, detExport*,        .CreditosPresumidos, .Importacoes, .Exportacoes, .Rastros
      rastro*                       CreditoPresumidoIcms, DeclaracaoImportacao (Adicao), DetalheExportacao (ExportacaoIndireta), Rastro
    infProdNFF?, infProdEmb?        .Nff, .Embalagem       ProdutoNff, EmbalagemProduto
    veicProd | med | arma+ |        .VeiculoNovo, .Medicamento, .Armas, .Combustivel, .Recopi   (exclusivas: decisão 27)
      comb | nRECOPI                VeiculoNovo, Medicamento, Arma, Combustivel (Cide, Encerrante, OrigemCombustivel)
  imposto                         .Imposto                 Imposto
    ICMS                            .Icms                  Icms (abstrata) e 21 subclasses: Icms00 ... IcmsSn900 (decisão 23)
    IPI, II, ISSQN                  .Ipi, .ImpostoImportacao, .Issqn          (ICMS ou ISSQN: decisão 26)
    PIS, PISST, COFINS, COFINSST    .Pis, .PisSt, .Cofins, .CofinsSt          Pis e Cofins abstratas, 4 subclasses cada (decisão 25)
    ICMSUFDest                      .IcmsUFDestino         IcmsUFDestino (a partilha da EC 87/2015)
    IS                              .ImpostoSeletivo       ImpostoSeletivo
    IBSCBS                          .IbsCbs                IbsCbs e os grupos (seção 11.5)
  impostoDevol?                   .ImpostoDevolvido        ImpostoDevolvido, IpiDevolvido
  infAdProd?, obsItem?            .InformacoesAdicionais, .Observacoes      ObservacoesItem (com a Observacao da 2.2)
  vItem?, DFeReferenciado?        .ValorTotal, .Referencia                  ItemReferenciado
total                           Total
  ICMSTot                         .Icms                    TotalIcms
  ISSQNtot?, retTrib?             .Issqn, .Retencao        TotalIssqn, RetencaoTributos
  ISTot?, IBSCBSTot?              .ImpostoSeletivo, .IbsCbs                 TotalImpostoSeletivo, TotalIbsCbs e os grupos
  vNFTot?                         .ValorTotalNota
```

| Tipo | Papel |
|---|---|
| `Item`, `Produto` e os grupos do produto | o item, o produto ou serviço, a importação e a exportação, a rastreabilidade, a NFF, a embalagem e os grupos específicos (veículo novo, medicamento, armamento, combustível, RECOPI) |
| `Imposto` | os tributos do item, cada um num grupo |
| `Icms` e as subclasses | o grupo de ICMS por CST (`Icms00` ... `Icms90`), a partilha (`IcmsPartilha`), o repasse do ST retido (`IcmsRepasse`) e o Simples Nacional por CSOSN (`IcmsSn101` ... `IcmsSn900`) |
| `Ipi`, `ImpostoImportacao`, `Issqn`, `Pis`, `Cofins` e os grupos | os tributos de antes da reforma |
| `IcmsUFDestino` | o ICMS de partilha para a UF do destinatário |
| `ImpostoSeletivo`, `IbsCbs` e os grupos | os tributos da reforma (NT 2025.002) |
| `Total` e os grupos | os totais da nota: ICMS, ISSQN, retenções, IS, IBS e CBS |
| as enumerações (11.8) | cada campo de domínio fechado, com o valor do XML |

### 11.2. `Item`, `Produto` e os grupos do produto

```csharp
public sealed class Item
{
    [Tag("prod", "I01")]           public Produto Produto { get; set; } = new();
    [Tag("imposto", "M01")]        public Imposto Imposto { get; set; } = new();
    [Tag("impostoDevol", "UA01")]  public ImpostoDevolvido? ImpostoDevolvido { get; set; }
    [Tag("infAdProd", "V01")]      public string? InformacoesAdicionais { get; set; }
    [Tag("obsItem", "VA01")]       public ObservacoesItem? Observacoes { get; set; }
    [Tag("vItem", "VB01")]         public decimal? ValorTotal { get; set; }
    [Tag("DFeReferenciado", "VC01")] public ItemReferenciado? Referencia { get; set; }
    // nItem (H02, o atributo): a posição na lista; a escrita numera de 1 a n, a leitura confere (decisão 21).
}

public sealed class Produto
{
    [Tag("cProd", "I02")]          public string? Codigo { get; set; }
    [Tag("cEAN", "I03")]           public string? Gtin { get; set; }
    [Tag("cBarra", "I03a")]        public string? CodigoBarras { get; set; }
    [Tag("xProd", "I04")]          public string? Descricao { get; set; }
    [Tag("NCM", "I05")]            public string? Ncm { get; set; }
    [Tag("NVE", "I05a")]           public IList<string> Nve { get; } = new List<string>();   // 0-8
    [Tag("CEST", "I05c")]          public string? Cest { get; set; }
    [Tag("indEscala", "I05d")]     public IndicadorEscala? Escala { get; set; }
    [Tag("CNPJFab", "I05e")]       public string? CnpjFabricante { get; set; }
    [Tag("cBenef", "I05f")]        public string? CodigoBeneficio { get; set; }
    [Tag("gCred", "I05g")]         public IList<CreditoPresumidoIcms> CreditosPresumidos { get; } = new List<CreditoPresumidoIcms>();   // 0-4
    [Tag("tpCredPresIBSZFM", "I05k")] public ClassificacaoCreditoPresumidoZfm? ClassificacaoZfm { get; set; }
    [Tag("EXTIPI", "I06")]         public string? ExTipi { get; set; }
    [Tag("CFOP", "I08")]           public int? Cfop { get; set; }
    [Tag("uCom", "I09")]           public string? UnidadeComercial { get; set; }
    [Tag("qCom", "I10")]           public decimal? QuantidadeComercial { get; set; }
    [Tag("vUnCom", "I10a")]        public decimal? ValorUnitarioComercial { get; set; }
    [Tag("vProd", "I11")]          public decimal? Valor { get; set; }
    [Tag("cEANTrib", "I12")]       public string? GtinTributavel { get; set; }
    [Tag("cBarraTrib", "I12a")]    public string? CodigoBarrasTributavel { get; set; }
    [Tag("uTrib", "I13")]          public string? UnidadeTributavel { get; set; }
    [Tag("qTrib", "I14")]          public decimal? QuantidadeTributavel { get; set; }
    [Tag("vUnTrib", "I14a")]       public decimal? ValorUnitarioTributavel { get; set; }
    [Tag("vFrete", "I15")]         public decimal? Frete { get; set; }
    [Tag("vSeg", "I16")]           public decimal? Seguro { get; set; }
    [Tag("vDesc", "I17")]          public decimal? Desconto { get; set; }
    [Tag("vOutro", "I17a")]        public decimal? OutrasDespesas { get; set; }
    [Tag("indTot", "I17b")]        public IndicadorTotal? Totalizacao { get; set; }
    [Tag("indBemMovelUsado", "I17c")] public IndicadorBemMovelUsado? BemMovelUsado { get; set; }
    [Tag("DI", "I18")]             public IList<DeclaracaoImportacao> Importacoes { get; } = new List<DeclaracaoImportacao>();   // 0-100
    [Tag("detExport", "I50")]      public IList<DetalheExportacao> Exportacoes { get; } = new List<DetalheExportacao>();   // 0-500
    [Tag("xPed", "I60")]           public string? Pedido { get; set; }
    [Tag("nItemPed", "I61")]       public string? ItemPedido { get; set; }
    [Tag("nFCI", "I70")]           public string? NumeroFci { get; set; }
    [Tag("rastro", "I80")]         public IList<Rastro> Rastros { get; } = new List<Rastro>();   // 0-500
    [Tag("infProdNFF", "I86")]     public ProdutoNff? Nff { get; set; }
    [Tag("infProdEmb", "I87")]     public EmbalagemProduto? Embalagem { get; set; }
    // veicProd, med, arma, comb ou nRECOPI: escolha do XSD, propriedades exclusivas (decisão 27).
    [Tag("veicProd", "J01")]       public VeiculoNovo? VeiculoNovo { get; set; }
    [Tag("med", "K01")]            public Medicamento? Medicamento { get; set; }
    [Tag("arma", "L01")]           public IList<Arma> Armas { get; } = new List<Arma>();   // 1-500
    [Tag("comb", "LA01")]          public Combustivel? Combustivel { get; set; }
    [Tag("nRECOPI", "LB01")]       public string? Recopi { get; set; }
}

public sealed class CreditoPresumidoIcms
{
    [Tag("cCredPresumido", "I05h")] public string? Codigo { get; set; }
    [Tag("pCredPresumido", "I05i")] public decimal? Percentual { get; set; }
    [Tag("vCredPresumido", "I05j")] public decimal? Valor { get; set; }
}

public sealed class DeclaracaoImportacao
{
    [Tag("nDI", "I19")]            public string? Numero { get; set; }
    [Tag("dDI", "I20")]            public DateOnly? Data { get; set; }
    [Tag("xLocDesemb", "I21")]     public string? LocalDesembaraco { get; set; }
    [Tag("UFDesemb", "I22")]       public UF? UFDesembaraco { get; set; }
    [Tag("dDesemb", "I23")]        public DateOnly? DataDesembaraco { get; set; }
    [Tag("tpViaTransp", "I23a")]   public ViaTransporteInternacional? ViaTransporte { get; set; }
    [Tag("vAFRMM", "I23b")]        public decimal? ValorAfrmm { get; set; }
    [Tag("tpIntermedio", "I23c")]  public FormaImportacao? FormaImportacao { get; set; }
    // CNPJ ou CPF do adquirente ou encomendante (decisão 10).
    [Tag("CNPJ", "I23d")]          public string? Cnpj { get; set; }
    [Tag("CPF", "I23d1")]          public string? Cpf { get; set; }
    [Tag("UFTerceiro", "I23e")]    public UF? UFTerceiro { get; set; }
    [Tag("cExportador", "I24")]    public string? CodigoExportador { get; set; }
    [Tag("adi", "I25")]            public IList<Adicao> Adicoes { get; } = new List<Adicao>();   // 1-999
}

public sealed class Adicao
{
    [Tag("nAdicao", "I26")]        public int? Numero { get; set; }
    [Tag("nSeqAdic", "I27")]       public int? Sequencial { get; set; }
    [Tag("cFabricante", "I28")]    public string? CodigoFabricante { get; set; }
    [Tag("vDescDI", "I29")]        public decimal? Desconto { get; set; }
    [Tag("nDraw", "I29a")]         public string? Drawback { get; set; }
}

public sealed class DetalheExportacao
{
    [Tag("nDraw", "I51")]          public string? Drawback { get; set; }
    [Tag("exportInd", "I52")]      public ExportacaoIndireta? Indireta { get; set; }
}

public sealed class ExportacaoIndireta
{
    [Tag("nRE", "I53")]            public string? RegistroExportacao { get; set; }
    [Tag("chNFe", "I54")]          public string? Chave { get; set; }
    [Tag("qExport", "I55")]        public decimal? Quantidade { get; set; }
}

public sealed class Rastro
{
    [Tag("nLote", "I81")]          public string? Lote { get; set; }
    [Tag("qLote", "I82")]          public decimal? Quantidade { get; set; }
    [Tag("dFab", "I83")]           public DateOnly? Fabricacao { get; set; }
    [Tag("dVal", "I84")]           public DateOnly? Validade { get; set; }
    [Tag("cAgreg", "I85")]         public string? CodigoAgregacao { get; set; }
}

public sealed class ProdutoNff
{
    [Tag("cProdFisco", "I86a")]    public string? CodigoFisco { get; set; }
    [Tag("cOperNFF", "I86b")]      public int? CodigoOperacao { get; set; }
}

public sealed class EmbalagemProduto
{
    [Tag("xEmb", "I87a")]          public string? Descricao { get; set; }
    [Tag("qVolEmb", "I87b")]       public decimal? Volume { get; set; }
    [Tag("uEmb", "I87c")]          public string? Unidade { get; set; }
}
```

Os grupos específicos (escolha do XSD, decisão 27):

```csharp
public sealed class VeiculoNovo
{
    [Tag("tpOp", "J02")]           public TipoOperacaoVeiculo? TipoOperacao { get; set; }
    [Tag("chassi", "J03")]         public string? Chassi { get; set; }
    [Tag("cCor", "J04")]           public string? CodigoCor { get; set; }
    [Tag("xCor", "J05")]           public string? Cor { get; set; }
    [Tag("pot", "J06")]            public string? Potencia { get; set; }
    [Tag("cilin", "J07")]          public string? Cilindrada { get; set; }
    [Tag("pesoL", "J08")]          public string? PesoLiquido { get; set; }
    [Tag("pesoB", "J09")]          public string? PesoBruto { get; set; }
    [Tag("nSerie", "J10")]         public string? Serie { get; set; }
    [Tag("tpComb", "J11")]         public string? TipoCombustivel { get; set; }
    [Tag("nMotor", "J12")]         public string? Motor { get; set; }
    [Tag("CMT", "J13")]            public string? CapacidadeTracao { get; set; }
    [Tag("dist", "J14")]           public string? DistanciaEixos { get; set; }
    [Tag("anoMod", "J16")]         public int? AnoModelo { get; set; }
    [Tag("anoFab", "J17")]         public int? AnoFabricacao { get; set; }
    [Tag("tpPint", "J18")]         public string? TipoPintura { get; set; }
    [Tag("tpVeic", "J19")]         public string? TipoVeiculo { get; set; }
    [Tag("espVeic", "J20")]        public int? EspecieVeiculo { get; set; }
    [Tag("VIN", "J21")]            public CondicaoVin? Vin { get; set; }
    [Tag("condVeic", "J22")]       public CondicaoVeiculo? Condicao { get; set; }
    [Tag("cMod", "J23")]           public string? MarcaModelo { get; set; }
    [Tag("cCorDENATRAN", "J24")]   public string? CorDenatran { get; set; }
    [Tag("lota", "J25")]           public int? Lotacao { get; set; }
    [Tag("tpRest", "J26")]         public RestricaoVeiculo? Restricao { get; set; }
}

public sealed class Medicamento
{
    [Tag("cProdANVISA", "K01a")]   public string? CodigoAnvisa { get; set; }
    [Tag("xMotivoIsencao", "K01b")] public string? MotivoIsencao { get; set; }
    [Tag("vPMC", "K06")]           public decimal? PrecoMaximoConsumidor { get; set; }
}

public sealed class Arma
{
    [Tag("tpArma", "L02")]         public TipoArma? Tipo { get; set; }
    [Tag("nSerie", "L03")]         public string? Serie { get; set; }
    [Tag("nCano", "L04")]          public string? SerieCano { get; set; }
    [Tag("descr", "L05")]          public string? Descricao { get; set; }
}

public sealed class Combustivel
{
    [Tag("cProdANP", "LA02")]      public string? CodigoAnp { get; set; }
    [Tag("descANP", "LA03")]       public string? DescricaoAnp { get; set; }
    [Tag("pGLP", "LA03a")]         public decimal? PercentualGlp { get; set; }
    [Tag("pGNn", "LA03b")]         public decimal? PercentualGasNacional { get; set; }
    [Tag("pGNi", "LA03c")]         public decimal? PercentualGasImportado { get; set; }
    [Tag("vPart", "LA03d")]        public decimal? ValorPartida { get; set; }
    [Tag("CODIF", "LA04")]         public string? Codif { get; set; }
    [Tag("qTemp", "LA05")]         public decimal? QuantidadeTemperaturaAmbiente { get; set; }
    [Tag("UFCons", "LA06")]        public UF? UFConsumo { get; set; }
                                  public bool Exterior { get; set; }
    [Tag("CIDE", "LA07")]          public Cide? Cide { get; set; }
    [Tag("encerrante", "LA11")]    public Encerrante? Encerrante { get; set; }
    [Tag("pBio", "LA17")]          public decimal? PercentualBiodiesel { get; set; }
    [Tag("origComb", "LA18")]      public IList<OrigemCombustivel> Origens { get; } = new List<OrigemCombustivel>();   // 0-30
}

public sealed class Cide
{
    [Tag("qBCProd", "LA08")]       public decimal? BaseCalculo { get; set; }
    [Tag("vAliqProd", "LA09")]     public decimal? Aliquota { get; set; }
    [Tag("vCIDE", "LA10")]         public decimal? Valor { get; set; }
}

public sealed class Encerrante
{
    [Tag("nBico", "LA12")]         public int? Bico { get; set; }
    [Tag("nBomba", "LA13")]        public int? Bomba { get; set; }
    [Tag("nTanque", "LA14")]       public int? Tanque { get; set; }
    [Tag("vEncIni", "LA15")]       public decimal? Inicial { get; set; }
    [Tag("vEncFin", "LA16")]       public decimal? Final { get; set; }
}

public sealed class OrigemCombustivel
{
    [Tag("indImport", "LA19")]     public IndicadorImportacao? Importacao { get; set; }
    [Tag("cUFOrig", "LA20")]       public UF? UF { get; set; }
    [Tag("pOrig", "LA21")]         public decimal? Percentual { get; set; }
}
```

### 11.3. `Imposto` e o ICMS

```csharp
public sealed class Imposto
{
    [Tag("vTotTrib", "M02")]       public decimal? ValorTotalTributos { get; set; }
    // ICMS (com IPI e II) ou ISSQN (com IPI): escolha do XSD, propriedades exclusivas; o IPI é um só nos dois ramos (decisão 26).
    [Tag("ICMS", "N01")]           public Icms? Icms { get; set; }
    [Tag("IPI", "O01")]            public Ipi? Ipi { get; set; }
    [Tag("II", "P01")]             public ImpostoImportacao? ImpostoImportacao { get; set; }
    [Tag("ISSQN", "U01")]          public Issqn? Issqn { get; set; }
    [Tag("PIS", "Q01")]            public Pis? Pis { get; set; }
    [Tag("PISST", "R01")]          public PisSt? PisSt { get; set; }
    [Tag("COFINS", "S01")]         public Cofins? Cofins { get; set; }
    [Tag("COFINSST", "T01")]       public CofinsSt? CofinsSt { get; set; }
    [Tag("ICMSUFDest", "NA01")]    public IcmsUFDestino? IcmsUFDestino { get; set; }
    [Tag("IS", "UB01")]            public ImpostoSeletivo? ImpostoSeletivo { get; set; }
    [Tag("IBSCBS", "UB12")]        public IbsCbs? IbsCbs { get; set; }
}

public abstract class Icms
{
    /// Um grupo de ICMS do item (o ICMS00 ... ICMSSN900): exatamente uma das 21 escolhas do XSD. Hierarquia fechada.
    private protected Icms();
    [Tag("orig", "N11")]           public OrigemMercadoria? Origem { get; set; }
}
```

As 21 subclasses, uma por grupo do XSD, cada uma com o `[Tag]` na classe (decisão 11). As propriedades repetem o nome em todas as subclasses em que o campo aparece (`BaseCalculo` é o `vBC` em todas); o identificador é o do MOC em cada grupo (decisão 30).

```csharp
[Tag("ICMS00", "N02")]
public sealed class Icms00 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
}

[Tag("ICMS02", "N02a")]
public sealed class Icms02 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("qBCMono", "N37a")]       public decimal? QuantidadeBaseMonofasico { get; set; }
    [Tag("adRemICMS", "N38")]      public decimal? AliquotaAdRem { get; set; }
    [Tag("vICMSMono", "N39")]      public decimal? ValorMonofasico { get; set; }
}

[Tag("ICMS10", "N03")]
public sealed class Icms10 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("vBCFCP", "N17a")]        public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("vICMSSTDeson", "N33a")]  public decimal? ValorStDesonerado { get; set; }
    [Tag("motDesICMSST", "N33b")]  public MotivoDesoneracao? MotivoDesoneracaoSt { get; set; }
}

[Tag("ICMS15", "N03a")]
public sealed class Icms15 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("qBCMono", "N37a")]       public decimal? QuantidadeBaseMonofasico { get; set; }
    [Tag("adRemICMS", "N38")]      public decimal? AliquotaAdRem { get; set; }
    [Tag("vICMSMono", "N39")]      public decimal? ValorMonofasico { get; set; }
    [Tag("qBCMonoReten", "N39a")]  public decimal? QuantidadeBaseRetencao { get; set; }
    [Tag("adRemICMSReten", "N40")] public decimal? AliquotaAdRemRetencao { get; set; }
    [Tag("vICMSMonoReten", "N41")] public decimal? ValorMonofasicoRetencao { get; set; }
    [Tag("pRedAdRem", "N47")]      public decimal? PercentualReducaoAdRem { get; set; }
    [Tag("motRedAdRem", "N48")]    public MotivoReducaoAdRem? MotivoReducaoAdRem { get; set; }
}

[Tag("ICMS20", "N04")]
public sealed class Icms20 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("vBCFCP", "N17a")]        public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}

[Tag("ICMS30", "N05")]
public sealed class Icms30 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}

[Tag("ICMS40", "N06")]
public sealed class Icms40 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}

[Tag("ICMS51", "N07")]
public sealed class Icms51 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("cBenefRBC", "N14a")]     public string? CodigoBeneficioReducao { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMSOp", "N16a")]       public decimal? ValorOperacao { get; set; }
    [Tag("pDif", "N16b")]          public decimal? PercentualDiferimento { get; set; }
    [Tag("vICMSDif", "N16c")]      public decimal? ValorDiferido { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("vBCFCP", "N17a")]        public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
    [Tag("pFCPDif", "N17d")]       public decimal? PercentualDiferimentoFcp { get; set; }
    [Tag("vFCPDif", "N17e")]       public decimal? ValorFcpDiferido { get; set; }
    [Tag("vFCPEfet", "N17f")]      public decimal? ValorFcpEfetivo { get; set; }
}

[Tag("ICMS53", "N07a")]
public sealed class Icms53 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("qBCMono", "N37a")]       public decimal? QuantidadeBaseMonofasico { get; set; }
    [Tag("adRemICMS", "N38")]      public decimal? AliquotaAdRem { get; set; }
    // N41a e N42 aparecem duas vezes no grupo na NT 2023.001 v1.60 (decisão 30).
    [Tag("vICMSMonoOp", "N41a")]   public decimal? ValorMonofasicoOperacao { get; set; }
    [Tag("pDif", "N42")]           public decimal? PercentualDiferimento { get; set; }
    [Tag("vICMSMonoDif", "N43")]   public decimal? ValorMonofasicoDiferido { get; set; }
    [Tag("vICMSMono", "N39")]      public decimal? ValorMonofasico { get; set; }
    [Tag("qBCMonoDif", "N41a")]    public decimal? QuantidadeBaseDiferida { get; set; }
    [Tag("adRemICMSDif", "N42")]   public decimal? AliquotaAdRemDiferida { get; set; }
}

[Tag("ICMS60", "N08")]
public sealed class Icms60 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("vBCSTRet", "N26")]       public decimal? BaseCalculoStRetido { get; set; }
    [Tag("pST", "N26a")]           public decimal? AliquotaConsumidorFinal { get; set; }
    [Tag("vICMSSubstituto", "N26b")] public decimal? ValorSubstituto { get; set; }
    [Tag("vICMSSTRet", "N27")]     public decimal? ValorStRetido { get; set; }
    [Tag("vBCFCPSTRet", "N27a")]   public decimal? BaseCalculoFcpStRetido { get; set; }
    [Tag("pFCPSTRet", "N27b")]     public decimal? PercentualFcpStRetido { get; set; }
    [Tag("vFCPSTRet", "N27d")]     public decimal? ValorFcpStRetido { get; set; }
    [Tag("pRedBCEfet", "N34")]     public decimal? PercentualReducaoBaseEfetiva { get; set; }
    [Tag("vBCEfet", "N35")]        public decimal? BaseCalculoEfetiva { get; set; }
    [Tag("pICMSEfet", "N36")]      public decimal? AliquotaEfetiva { get; set; }
    [Tag("vICMSEfet", "N37")]      public decimal? ValorEfetivo { get; set; }
}

[Tag("ICMS61", "N08a")]
public sealed class Icms61 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("qBCMonoRet", "N43a")]    public decimal? QuantidadeBaseRetida { get; set; }
    [Tag("adRemICMSRet", "N44")]   public decimal? AliquotaAdRemRetida { get; set; }
    [Tag("vICMSMonoRet", "N45")]   public decimal? ValorMonofasicoRetido { get; set; }
}

[Tag("ICMS70", "N09")]
public sealed class Icms70 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("vBCFCP", "N17a")]        public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
    [Tag("vICMSSTDeson", "N33a")]  public decimal? ValorStDesonerado { get; set; }
    [Tag("motDesICMSST", "N33b")]  public MotivoDesoneracao? MotivoDesoneracaoSt { get; set; }
}

[Tag("ICMS90", "N10")]
public sealed class Icms90 : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("cBenefRBC", "N14a")]     public string? CodigoBeneficioReducao { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMSOp", "N16b")]       public decimal? ValorOperacao { get; set; }
    [Tag("pDif", "N16c")]          public decimal? PercentualDiferimento { get; set; }
    [Tag("vICMSDif", "N16d")]      public decimal? ValorDiferido { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("vBCFCP", "N17a")]        public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCP", "N17b")]          public decimal? PercentualFcp { get; set; }
    [Tag("vFCP", "N17c")]          public decimal? ValorFcp { get; set; }
    [Tag("pFCPDif", "N17d")]       public decimal? PercentualDiferimentoFcp { get; set; }
    [Tag("vFCPDif", "N17e")]       public decimal? ValorFcpDiferido { get; set; }
    [Tag("vFCPEfet", "N17f")]      public decimal? ValorFcpEfetivo { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
    [Tag("vICMSSTDeson", "N33a")]  public decimal? ValorStDesonerado { get; set; }
    [Tag("motDesICMSST", "N33b")]  public MotivoDesoneracao? MotivoDesoneracaoSt { get; set; }
}

[Tag("ICMSPart", "N10a")]
public sealed class IcmsPartilha : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("pBCOp", "N25")]          public decimal? PercentualBaseOperacao { get; set; }
    [Tag("UFST", "N24")]           public UF? UFSt { get; set; }
                                  public bool Exterior { get; set; }
    [Tag("vICMSDeson", "N28a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("motDesICMS", "N28")]     public MotivoDesoneracao? MotivoDesoneracao { get; set; }
    [Tag("indDeduzDeson", "N28b")] public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}

[Tag("ICMSST", "N10b")]
public sealed class IcmsRepasse : Icms
{
    [Tag("CST", "N12")]            public CstIcms? Cst { get; set; }
    [Tag("vBCSTRet", "N26")]       public decimal? BaseCalculoStRetido { get; set; }
    [Tag("pST", "N26a")]           public decimal? AliquotaConsumidorFinal { get; set; }
    [Tag("vICMSSubstituto", "N26b")] public decimal? ValorSubstituto { get; set; }
    [Tag("vICMSSTRet", "N27")]     public decimal? ValorStRetido { get; set; }
    [Tag("vBCFCPSTRet", "N27a")]   public decimal? BaseCalculoFcpStRetido { get; set; }
    [Tag("pFCPSTRet", "N27b")]     public decimal? PercentualFcpStRetido { get; set; }
    [Tag("vFCPSTRet", "N27d")]     public decimal? ValorFcpStRetido { get; set; }
    [Tag("vBCSTDest", "N31")]      public decimal? BaseCalculoStDestino { get; set; }
    [Tag("vICMSSTDest", "N32")]    public decimal? ValorStDestino { get; set; }
    [Tag("pRedBCEfet", "N34")]     public decimal? PercentualReducaoBaseEfetiva { get; set; }
    [Tag("vBCEfet", "N35")]        public decimal? BaseCalculoEfetiva { get; set; }
    [Tag("pICMSEfet", "N36")]      public decimal? AliquotaEfetiva { get; set; }
    [Tag("vICMSEfet", "N37")]      public decimal? ValorEfetivo { get; set; }
}

[Tag("ICMSSN101", "N10c")]
public sealed class IcmsSn101 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
    [Tag("pCredSN", "N29")]        public decimal? AliquotaCredito { get; set; }
    [Tag("vCredICMSSN", "N30")]    public decimal? ValorCredito { get; set; }
}

[Tag("ICMSSN102", "N10d")]
public sealed class IcmsSn102 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
}

[Tag("ICMSSN201", "N10e")]
public sealed class IcmsSn201 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("pCredSN", "N29")]        public decimal? AliquotaCredito { get; set; }
    [Tag("vCredICMSSN", "N30")]    public decimal? ValorCredito { get; set; }
}

[Tag("ICMSSN202", "N10f")]
public sealed class IcmsSn202 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
}

[Tag("ICMSSN500", "N10g")]
public sealed class IcmsSn500 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
    [Tag("vBCSTRet", "N26")]       public decimal? BaseCalculoStRetido { get; set; }
    [Tag("pST", "N26a")]           public decimal? AliquotaConsumidorFinal { get; set; }
    [Tag("vICMSSubstituto", "N26b")] public decimal? ValorSubstituto { get; set; }
    [Tag("vICMSSTRet", "N27")]     public decimal? ValorStRetido { get; set; }
    [Tag("vBCFCPSTRet", "N27a")]   public decimal? BaseCalculoFcpStRetido { get; set; }
    [Tag("pFCPSTRet", "N27b")]     public decimal? PercentualFcpStRetido { get; set; }
    [Tag("vFCPSTRet", "N27d")]     public decimal? ValorFcpStRetido { get; set; }
    [Tag("pRedBCEfet", "N34")]     public decimal? PercentualReducaoBaseEfetiva { get; set; }
    [Tag("vBCEfet", "N35")]        public decimal? BaseCalculoEfetiva { get; set; }
    [Tag("pICMSEfet", "N36")]      public decimal? AliquotaEfetiva { get; set; }
    [Tag("vICMSEfet", "N37")]      public decimal? ValorEfetivo { get; set; }
}

[Tag("ICMSSN900", "N10h")]
public sealed class IcmsSn900 : Icms
{
    [Tag("CSOSN", "N12a")]         public CsosnIcms? Csosn { get; set; }
    [Tag("modBC", "N13")]          public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }
    [Tag("vBC", "N15")]            public decimal? BaseCalculo { get; set; }
    [Tag("pRedBC", "N14")]         public decimal? PercentualReducaoBase { get; set; }
    [Tag("pICMS", "N16")]          public decimal? Aliquota { get; set; }
    [Tag("vICMS", "N17")]          public decimal? Valor { get; set; }
    [Tag("modBCST", "N18")]        public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }
    [Tag("pMVAST", "N19")]         public decimal? PercentualMargemSt { get; set; }
    [Tag("pRedBCST", "N20")]       public decimal? PercentualReducaoBaseSt { get; set; }
    [Tag("vBCST", "N21")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("pICMSST", "N22")]        public decimal? AliquotaSt { get; set; }
    [Tag("vICMSST", "N23")]        public decimal? ValorSt { get; set; }
    [Tag("vBCFCPST", "N23a")]      public decimal? BaseCalculoFcpSt { get; set; }
    [Tag("pFCPST", "N23b")]        public decimal? PercentualFcpSt { get; set; }
    [Tag("vFCPST", "N23d")]        public decimal? ValorFcpSt { get; set; }
    [Tag("pCredSN", "N29")]        public decimal? AliquotaCredito { get; set; }
    [Tag("vCredICMSSN", "N30")]    public decimal? ValorCredito { get; set; }
}
```

### 11.4. IPI, II, ISSQN, PIS, COFINS e a partilha

```csharp
public sealed class Ipi
{
    [Tag("CNPJProd", "O03")]       public string? CnpjProdutor { get; set; }
    [Tag("cSelo", "O04")]          public string? CodigoSelo { get; set; }
    [Tag("qSelo", "O05")]          public long? QuantidadeSelo { get; set; }
    [Tag("cEnq", "O06")]           public string? CodigoEnquadramento { get; set; }
    // IPITrib ou IPINT: escolha do XSD, propriedades exclusivas (decisão 25).
    [Tag("IPITrib", "O07")]        public IpiTributado? Tributado { get; set; }
    [Tag("IPINT", "O08")]          public IpiNaoTributado? NaoTributado { get; set; }
}

public sealed class IpiTributado
{
    [Tag("CST", "O09")]            public CstIpi? Cst { get; set; }
    // vBC e pIPI, ou qUnid e vUnid: escolha do XSD, propriedades exclusivas.
    [Tag("vBC", "O10")]            public decimal? BaseCalculo { get; set; }
    [Tag("pIPI", "O13")]           public decimal? Aliquota { get; set; }
    [Tag("qUnid", "O11")]          public decimal? Quantidade { get; set; }
    [Tag("vUnid", "O12")]          public decimal? ValorUnidade { get; set; }
    [Tag("vIPI", "O14")]           public decimal? Valor { get; set; }
}

public sealed class IpiNaoTributado
{
    [Tag("CST", "O09")]            public CstIpi? Cst { get; set; }
}

public sealed class ImpostoImportacao
{
    [Tag("vBC", "P02")]            public decimal? BaseCalculo { get; set; }
    [Tag("vDespAdu", "P03")]       public decimal? DespesasAduaneiras { get; set; }
    [Tag("vII", "P04")]            public decimal? Valor { get; set; }
    [Tag("vIOF", "P05")]           public decimal? Iof { get; set; }
}

public sealed class Issqn
{
    [Tag("vBC", "U02")]            public decimal? BaseCalculo { get; set; }
    [Tag("vAliq", "U03")]          public decimal? Aliquota { get; set; }
    [Tag("vISSQN", "U04")]         public decimal? Valor { get; set; }
    [Tag("cMunFG", "U05")]         public int? MunicipioFatoGerador { get; set; }
    [Tag("cListServ", "U06")]      public string? ItemListaServico { get; set; }
    [Tag("vDeducao", "U07")]       public decimal? Deducao { get; set; }
    [Tag("vOutro", "U08")]         public decimal? OutrasRetencoes { get; set; }
    [Tag("vDescIncond", "U09")]    public decimal? DescontoIncondicionado { get; set; }
    [Tag("vDescCond", "U10")]      public decimal? DescontoCondicionado { get; set; }
    [Tag("vISSRet", "U11")]        public decimal? Retencao { get; set; }
    [Tag("indISS", "U12")]         public ExigibilidadeIss? Exigibilidade { get; set; }
    [Tag("cServico", "U13")]       public string? CodigoServico { get; set; }
    [Tag("cMun", "U14")]           public int? MunicipioIncidencia { get; set; }
    [Tag("cPais", "U15")]          public int? CodigoPais { get; set; }
    [Tag("nProcesso", "U16")]      public string? Processo { get; set; }
    [Tag("indIncentivo", "U17")]   public IndicadorIncentivo? IncentivoFiscal { get; set; }
}
```

```csharp
public abstract class Pis
{
    /// Um grupo de PIS do item (PISAliq, PISQtde, PISNT, PISOutr). Hierarquia fechada; o CST está em todos, na base.
    private protected Pis();
    [Tag("CST", "Q06")]            public CstPisCofins? Cst { get; set; }
}

[Tag("PISAliq", "Q02")]
public sealed class PisAliquota : Pis
{
    [Tag("vBC", "Q07")]            public decimal? BaseCalculo { get; set; }
    [Tag("pPIS", "Q08")]           public decimal? Aliquota { get; set; }
    [Tag("vPIS", "Q09")]           public decimal? Valor { get; set; }
}

[Tag("PISQtde", "Q03")]
public sealed class PisQuantidade : Pis
{
    [Tag("qBCProd", "Q10")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "Q11")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vPIS", "Q09")]           public decimal? Valor { get; set; }
}

[Tag("PISNT", "Q04")]
public sealed class PisNaoTributado : Pis
{
}

[Tag("PISOutr", "Q05")]
public sealed class PisOutros : Pis
{
    // vBC e pPIS, ou qBCProd e vAliqProd: escolha do XSD, propriedades exclusivas.
    [Tag("vBC", "Q07")]            public decimal? BaseCalculo { get; set; }
    [Tag("pPIS", "Q08")]           public decimal? Aliquota { get; set; }
    [Tag("qBCProd", "Q10")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "Q11")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vPIS", "Q09")]           public decimal? Valor { get; set; }
}

public sealed class PisSt
{
    // vBC e pPIS, ou qBCProd e vAliqProd: escolha do XSD, propriedades exclusivas.
    [Tag("vBC", "R02")]            public decimal? BaseCalculo { get; set; }
    [Tag("pPIS", "R03")]           public decimal? Aliquota { get; set; }
    [Tag("qBCProd", "R04")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "R05")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vPIS", "R06")]           public decimal? Valor { get; set; }
    [Tag("indSomaPISST", "R07")]   public IndicadorTotal? Totalizacao { get; set; }
}

public abstract class Cofins
{
    /// Um grupo de COFINS do item (COFINSAliq, COFINSQtde, COFINSNT, COFINSOutr). Hierarquia fechada.
    private protected Cofins();
    [Tag("CST", "S06")]            public CstPisCofins? Cst { get; set; }
}

[Tag("COFINSAliq", "S02")]
public sealed class CofinsAliquota : Cofins
{
    [Tag("vBC", "S07")]            public decimal? BaseCalculo { get; set; }
    [Tag("pCOFINS", "S08")]        public decimal? Aliquota { get; set; }
    [Tag("vCOFINS", "S11")]        public decimal? Valor { get; set; }
}

[Tag("COFINSQtde", "S03")]
public sealed class CofinsQuantidade : Cofins
{
    [Tag("qBCProd", "S09")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "S10")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vCOFINS", "S11")]        public decimal? Valor { get; set; }
}

[Tag("COFINSNT", "S04")]
public sealed class CofinsNaoTributado : Cofins
{
}

[Tag("COFINSOutr", "S05")]
public sealed class CofinsOutros : Cofins
{
    // vBC e pCOFINS, ou qBCProd e vAliqProd: escolha do XSD, propriedades exclusivas.
    [Tag("vBC", "S07")]            public decimal? BaseCalculo { get; set; }
    [Tag("pCOFINS", "S08")]        public decimal? Aliquota { get; set; }
    [Tag("qBCProd", "S09")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "S10")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vCOFINS", "S11")]        public decimal? Valor { get; set; }
}

public sealed class CofinsSt
{
    // vBC e pCOFINS, ou qBCProd e vAliqProd: escolha do XSD, propriedades exclusivas.
    [Tag("vBC", "T02")]            public decimal? BaseCalculo { get; set; }
    [Tag("pCOFINS", "T03")]        public decimal? Aliquota { get; set; }
    [Tag("qBCProd", "T04")]        public decimal? Quantidade { get; set; }
    [Tag("vAliqProd", "T05")]      public decimal? AliquotaUnidade { get; set; }
    [Tag("vCOFINS", "T06")]        public decimal? Valor { get; set; }
    [Tag("indSomaCOFINSST", "T07")] public IndicadorTotal? Totalizacao { get; set; }
}
```

```csharp
public sealed class IcmsUFDestino
{
    [Tag("vBCUFDest", "NA03")]     public decimal? BaseCalculo { get; set; }
    [Tag("vBCFCPUFDest", "NA04")]  public decimal? BaseCalculoFcp { get; set; }
    [Tag("pFCPUFDest", "NA05")]    public decimal? PercentualFcp { get; set; }
    [Tag("pICMSUFDest", "NA07")]   public decimal? AliquotaInterna { get; set; }
    [Tag("pICMSInter", "NA09")]    public decimal? AliquotaInterestadual { get; set; }
    [Tag("pICMSInterPart", "NA11")] public decimal? PercentualPartilha { get; set; }
    [Tag("vFCPUFDest", "NA13")]    public decimal? ValorFcp { get; set; }
    [Tag("vICMSUFDest", "NA15")]   public decimal? ValorDestino { get; set; }
    [Tag("vICMSUFRemet", "NA17")]  public decimal? ValorRemetente { get; set; }
}
```

### 11.5. IS, IBS e CBS

```csharp
public sealed class ImpostoSeletivo
{
    [Tag("CSTIS", "UB02")]         public string? Cst { get; set; }
    [Tag("cClassTribIS", "UB03")]  public string? ClassificacaoTributaria { get; set; }
    [Tag("vBCIS", "UB05")]         public decimal? BaseCalculo { get; set; }
    [Tag("pIS", "UB06")]           public decimal? Aliquota { get; set; }
    [Tag("adRemIS", "UB07")]       public decimal? AliquotaEspecifica { get; set; }
    [Tag("uTrib", "UB09")]         public string? UnidadeTributavel { get; set; }
    [Tag("qTrib", "UB10")]         public decimal? QuantidadeTributavel { get; set; }
    [Tag("vIS", "UB11")]           public decimal? Valor { get; set; }
}

public sealed class IbsCbs
{
    [Tag("CST", "UB13")]           public string? Cst { get; set; }
    [Tag("cClassTrib", "UB14")]    public string? ClassificacaoTributaria { get; set; }
    [Tag("indDoacao", "UB14a")]    public IndicadorDoacao? Doacao { get; set; }
    // gIBSCBS, gIBSCBSMono, gTransfCred ou gAjusteCompet: escolha do XSD, propriedades exclusivas.
    [Tag("gIBSCBS", "UB15")]       public TributacaoIbsCbs? Tributacao { get; set; }
    [Tag("gIBSCBSMono", "UB84")]   public MonofasiaIbsCbs? Monofasia { get; set; }
    [Tag("gTransfCred", "UB106")]  public TransferenciaCredito? TransferenciaCredito { get; set; }
    [Tag("gAjusteCompet", "UB112")] public AjusteCompetencia? AjusteCompetencia { get; set; }
    [Tag("gEstornoCred", "UB116")] public EstornoCredito? EstornoCredito { get; set; }
    // gCredPresOper ou gCredPresIBSZFM: escolha do XSD, propriedades exclusivas.
    [Tag("gCredPresOper", "UB120")] public CreditoPresumidoOperacao? CreditoPresumido { get; set; }
    [Tag("gCredPresIBSZFM", "UB131")] public CreditoPresumidoZfm? CreditoPresumidoZfm { get; set; }
}

public sealed class TributacaoIbsCbs
{
    [Tag("vBC", "UB16")]           public decimal? BaseCalculo { get; set; }
    [Tag("gIBSUF", "UB17")]        public IbsUF IbsUF { get; set; } = new();
    [Tag("gIBSMun", "UB36")]       public IbsMunicipio IbsMunicipio { get; set; } = new();
    [Tag("vIBS", "UB54a")]         public decimal? ValorIbs { get; set; }
    [Tag("gCBS", "UB55")]          public Cbs Cbs { get; set; } = new();
    [Tag("gTribRegular", "UB68")]  public TributacaoRegular? TributacaoRegular { get; set; }
    [Tag("gTribCompraGov", "UB82a")] public TributacaoCompraGovernamental? CompraGovernamental { get; set; }
}

public sealed class IbsUF
{
    [Tag("pIBSUF", "UB18")]        public decimal? Aliquota { get; set; }
    [Tag("gDif", "UB21")]          public Diferimento? Diferimento { get; set; }
    [Tag("gDevTrib", "UB24")]      public DevolucaoTributo? Devolucao { get; set; }
    [Tag("gRed", "UB26")]          public ReducaoAliquota? Reducao { get; set; }
    [Tag("vIBSUF", "UB35")]        public decimal? Valor { get; set; }
}

public sealed class IbsMunicipio
{
    [Tag("pIBSMun", "UB37")]       public decimal? Aliquota { get; set; }
    [Tag("gDif", "UB40")]          public Diferimento? Diferimento { get; set; }
    [Tag("gDevTrib", "UB43")]      public DevolucaoTributo? Devolucao { get; set; }
    [Tag("gRed", "UB45")]          public ReducaoAliquota? Reducao { get; set; }
    [Tag("vIBSMun", "UB54")]       public decimal? Valor { get; set; }
}

public sealed class Cbs
{
    [Tag("pCBS", "UB56")]          public decimal? Aliquota { get; set; }
    [Tag("gDif", "UB59")]          public Diferimento? Diferimento { get; set; }
    [Tag("gDevTrib", "UB62")]      public DevolucaoTributo? Devolucao { get; set; }
    [Tag("gRed", "UB64")]          public ReducaoAliquota? Reducao { get; set; }
    [Tag("gALCZFMCBS", "UB66a")]   public CbsAreaIncentivada? AreaIncentivada { get; set; }
    [Tag("vCBS", "UB67")]          public decimal? Valor { get; set; }
}

public sealed class Diferimento
{
    [Tag("pDif")]                  public decimal? Percentual { get; set; }
    [Tag("vDif")]                  public decimal? Valor { get; set; }
}

public sealed class DevolucaoTributo
{
    [Tag("pDevTrib")]              public decimal? Percentual { get; set; }
    [Tag("vDevTrib")]              public decimal? Valor { get; set; }
}

public sealed class ReducaoAliquota
{
    [Tag("pRedAliq")]              public decimal? Percentual { get; set; }
    [Tag("pAliqEfet")]             public decimal? AliquotaEfetiva { get; set; }
}

public sealed class CbsAreaIncentivada
{
    [Tag("tpALCZFMCBS", "UB66b")]  public AplicacaoAliquotaZeroCbs? Tipo { get; set; }
    [Tag("nProcSuframa", "UB66c")] public string? ProcessoSuframa { get; set; }
    [Tag("pAliqEfetRegCBS", "UB66d")] public decimal? AliquotaEfetiva { get; set; }
    [Tag("vTribRegCBS", "UB66e")]  public decimal? Valor { get; set; }
}

public sealed class TributacaoRegular
{
    [Tag("CSTReg", "UB69")]        public string? Cst { get; set; }
    [Tag("cClassTribReg", "UB70")] public string? ClassificacaoTributaria { get; set; }
    [Tag("pAliqEfetRegIBSUF", "UB71")] public decimal? AliquotaIbsUF { get; set; }
    [Tag("vTribRegIBSUF", "UB72")] public decimal? ValorIbsUF { get; set; }
    [Tag("pAliqEfetRegIBSMun", "UB72a")] public decimal? AliquotaIbsMunicipio { get; set; }
    [Tag("vTribRegIBSMun", "UB72b")] public decimal? ValorIbsMunicipio { get; set; }
    [Tag("pAliqEfetRegCBS", "UB72c")] public decimal? AliquotaCbs { get; set; }
    [Tag("vTribRegCBS", "UB72d")]  public decimal? ValorCbs { get; set; }
}

public sealed class TributacaoCompraGovernamental
{
    [Tag("pAliqIBSUF", "UB82b")]   public decimal? AliquotaIbsUF { get; set; }
    [Tag("vTribIBSUF", "UB82c")]   public decimal? ValorIbsUF { get; set; }
    [Tag("pAliqIBSMun", "UB82d")]  public decimal? AliquotaIbsMunicipio { get; set; }
    [Tag("vTribIBSMun", "UB82e")]  public decimal? ValorIbsMunicipio { get; set; }
    [Tag("pAliqCBS", "UB82f")]     public decimal? AliquotaCbs { get; set; }
    [Tag("vTribCBS", "UB82g")]     public decimal? ValorCbs { get; set; }
}
```

A tributação monofásica de combustíveis no IBS e na CBS, na forma da NT 2025.002 v1.50 (produção em 03/11/2026): quatro grupos (IBS e CBS, ad rem e ad valorem), cada um com até quatro subgrupos. Os subgrupos de mesmo conteúdo nos dois grupos de um tributo são uma classe só (decisão 29).

```csharp
public sealed class MonofasiaIbsCbs
{
    // ad rem ou ad valorem, no IBS e na CBS: duas escolhas do XSD, propriedades exclusivas aos pares.
    [Tag("gIBSMonoAdRem", "UB85a")] public MonofasiaIbsAdRem? IbsAdRem { get; set; }
    [Tag("gIBSMonoAdValorem", "UB90")] public MonofasiaIbsAdValorem? IbsAdValorem { get; set; }
    [Tag("gCBSMonoAdRem", "UB95a")] public MonofasiaCbsAdRem? CbsAdRem { get; set; }
    [Tag("gCBSMonoAdValorem", "UB100")] public MonofasiaCbsAdValorem? CbsAdValorem { get; set; }
    [Tag("vTotIBSMonoItem", "UB105a")] public decimal? TotalIbs { get; set; }
    [Tag("vTotCBSMonoItem", "UB105b")] public decimal? TotalCbs { get; set; }
}

public sealed class MonofasiaIbsAdRem
{
    [Tag("gMonoPadrao", "UB86")]   public MonofasiaIbsAdRemPadrao? Padrao { get; set; }
    [Tag("gMonoReten", "UB87")]    public MonofasiaIbsAdRemRetencao? Retencao { get; set; }
    [Tag("gMonoRet", "UB88")]      public MonofasiaIbsRetido? Retido { get; set; }
    [Tag("gpBioDiferenca", "UB89")] public MonofasiaIbsBiocombustivel? Biocombustivel { get; set; }
}

public sealed class MonofasiaIbsAdRemPadrao
{
    [Tag("qBCMono", "UB86a")]      public decimal? Quantidade { get; set; }
    [Tag("adRemIBS", "UB86b")]     public decimal? AliquotaAdRem { get; set; }
    [Tag("vIBSMono", "UB86c")]     public decimal? Valor { get; set; }
}

public sealed class MonofasiaIbsAdRemRetencao
{
    [Tag("qBCMonoReten", "UB87a")] public decimal? Quantidade { get; set; }
    [Tag("adRemIBSReten", "UB87b")] public decimal? AliquotaAdRem { get; set; }
    [Tag("vIBSMonoReten", "UB87c")] public decimal? Valor { get; set; }
}

public sealed class MonofasiaIbsAdValorem
{
    [Tag("gMonoPadrao", "UB91")]   public MonofasiaIbsAdValoremPadrao? Padrao { get; set; }
    [Tag("gMonoReten", "UB92")]    public MonofasiaIbsAdValoremRetencao? Retencao { get; set; }
    [Tag("gMonoRet", "UB93")]      public MonofasiaIbsRetido? Retido { get; set; }
    [Tag("gpBioDiferenca", "UB94")] public MonofasiaIbsBiocombustivel? Biocombustivel { get; set; }
}

public sealed class MonofasiaIbsAdValoremPadrao
{
    [Tag("vBCMono", "UB91a")]      public decimal? BaseCalculo { get; set; }
    [Tag("pAliqMonoUF", "UB91b")]  public decimal? AliquotaUF { get; set; }
    [Tag("vIBSMonoUF", "UB91c")]   public decimal? ValorUF { get; set; }
    [Tag("pAliqMonoMun", "UB91d")] public decimal? AliquotaMunicipio { get; set; }
    [Tag("vIBSMonoMun", "UB91e")]  public decimal? ValorMunicipio { get; set; }
    [Tag("vIBSMono", "UB91f")]     public decimal? Valor { get; set; }
}

public sealed class MonofasiaIbsAdValoremRetencao
{
    [Tag("vBCMonoReten", "UB92a")] public decimal? BaseCalculo { get; set; }
    [Tag("pAliqMonoReten", "UB92b")] public decimal? Aliquota { get; set; }
    [Tag("vIBSMonoReten", "UB92c")] public decimal? Valor { get; set; }
}

public sealed class MonofasiaIbsRetido
{
    [Tag("vIBSMonoRet")]           public decimal? Valor { get; set; }
}

public sealed class MonofasiaIbsBiocombustivel
{
    [Tag("qBCBioComb")]            public decimal? Quantidade { get; set; }
    [Tag("vIBSDiferenca")]         public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsAdRem
{
    [Tag("gMonoPadrao", "UB96")]   public MonofasiaCbsAdRemPadrao? Padrao { get; set; }
    [Tag("gMonoReten", "UB97")]    public MonofasiaCbsAdRemRetencao? Retencao { get; set; }
    [Tag("gMonoRet", "UB98")]      public MonofasiaCbsRetido? Retido { get; set; }
    [Tag("gpBioDiferenca", "UB99")] public MonofasiaCbsBiocombustivel? Biocombustivel { get; set; }
}

public sealed class MonofasiaCbsAdRemPadrao
{
    [Tag("qBCMono", "UB96a")]      public decimal? Quantidade { get; set; }
    [Tag("adRemCBS", "UB96b")]     public decimal? AliquotaAdRem { get; set; }
    [Tag("vCBSMono", "UB96c")]     public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsAdRemRetencao
{
    [Tag("qBCMonoReten", "UB97a")] public decimal? Quantidade { get; set; }
    [Tag("adRemCBSReten", "UB97b")] public decimal? AliquotaAdRem { get; set; }
    [Tag("vCBSMonoReten", "UB97c")] public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsAdValorem
{
    [Tag("gMonoPadrao", "UB101")]  public MonofasiaCbsAdValoremPadrao? Padrao { get; set; }
    [Tag("gMonoReten", "UB102")]   public MonofasiaCbsAdValoremRetencao? Retencao { get; set; }
    [Tag("gMonoRet", "UB103")]     public MonofasiaCbsRetido? Retido { get; set; }
    [Tag("gpBioDiferenca", "UB104")] public MonofasiaCbsBiocombustivel? Biocombustivel { get; set; }
}

public sealed class MonofasiaCbsAdValoremPadrao
{
    [Tag("vBCMono", "UB101a")]     public decimal? BaseCalculo { get; set; }
    [Tag("pAliqMonoCBS", "UB101b")] public decimal? Aliquota { get; set; }
    [Tag("vCBSMono", "UB101c")]    public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsAdValoremRetencao
{
    [Tag("vBCMonoReten", "UB102a")] public decimal? BaseCalculo { get; set; }
    [Tag("pAliqMonoReten", "UB102b")] public decimal? Aliquota { get; set; }
    [Tag("vCBSMonoReten", "UB102c")] public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsRetido
{
    [Tag("vCBSMonoRet")]           public decimal? Valor { get; set; }
}

public sealed class MonofasiaCbsBiocombustivel
{
    [Tag("qBCBioComb")]            public decimal? Quantidade { get; set; }
    [Tag("vCBSDiferenca")]         public decimal? Valor { get; set; }
}
```

```csharp
public sealed class TransferenciaCredito
{
    [Tag("vIBS", "UB107")]         public decimal? ValorIbs { get; set; }
    [Tag("vCBS", "UB108")]         public decimal? ValorCbs { get; set; }
}

public sealed class AjusteCompetencia
{
    [Tag("competApur", "UB113")]   public string? Competencia { get; set; }
    [Tag("vIBS", "UB114")]         public decimal? ValorIbs { get; set; }
    [Tag("vCBS", "UB115")]         public decimal? ValorCbs { get; set; }
}

public sealed class EstornoCredito
{
    [Tag("vIBSEstCred")]           public decimal? ValorIbs { get; set; }
    [Tag("vCBSEstCred")]           public decimal? ValorCbs { get; set; }
}

public sealed class CreditoPresumidoOperacao
{
    [Tag("vBCCredPres", "UB121")]  public decimal? BaseCalculo { get; set; }
    [Tag("cCredPres", "UB122")]    public string? Codigo { get; set; }
    [Tag("gIBSCredPres", "UB123")] public CreditoPresumidoIbsCbs? Ibs { get; set; }
    [Tag("gCBSCredPres", "UB127")] public CreditoPresumidoIbsCbs? Cbs { get; set; }
}

public sealed class CreditoPresumidoIbsCbs
{
    [Tag("pCredPres")]             public decimal? Percentual { get; set; }
    // vCredPres ou vCredPresCondSus: escolha do XSD, propriedades exclusivas.
    [Tag("vCredPres")]             public decimal? Valor { get; set; }
    [Tag("vCredPresCondSus")]      public decimal? ValorCondicaoSuspensiva { get; set; }
}

public sealed class CreditoPresumidoZfm
{
    [Tag("competApur", "UB132")]   public string? Competencia { get; set; }
    [Tag("tpCredPresIBSZFM", "UB133")] public ClassificacaoCreditoPresumidoZfm? Classificacao { get; set; }
    [Tag("vCredPresIBSZFM", "UB134")] public decimal? Valor { get; set; }
}
```

### 11.6. Devolução, observações e referência do item

```csharp
public sealed class ImpostoDevolvido
{
    [Tag("pDevol", "UA02")]        public decimal? Percentual { get; set; }
    [Tag("IPI", "UA03")]           public IpiDevolvido Ipi { get; set; } = new();
}

public sealed class IpiDevolvido
{
    [Tag("vIPIDevol", "UA04")]     public decimal? Valor { get; set; }
}

public sealed class ObservacoesItem
{
    [Tag("obsCont", "VA02")]       public Observacao? Contribuinte { get; set; }
    [Tag("obsFisco", "VA05")]      public Observacao? Fisco { get; set; }
}

public sealed class ItemReferenciado
{
    [Tag("chaveAcesso", "VC02")]   public string? Chave { get; set; }
    [Tag("nItem", "VC03")]         public int? NumeroItem { get; set; }
}
```

O `obsCont` e o `obsFisco` do item têm o mesmo conteúdo dos da nota (`Z04` a `Z09`): a `Observacao` da 2.2, que já leva o `[Tag]` só com a tag, serve aos quatro lugares (decisão 29). No item eles não se repetem (`0-1`), por isso são propriedades e não listas.

### 11.7. `Total`

```csharp
public sealed class Total
{
    [Tag("ICMSTot", "W02")]        public TotalIcms Icms { get; set; } = new();
    [Tag("ISSQNtot", "W17")]       public TotalIssqn? Issqn { get; set; }
    [Tag("retTrib", "W23")]        public RetencaoTributos? Retencao { get; set; }
    [Tag("ISTot", "W31")]          public TotalImpostoSeletivo? ImpostoSeletivo { get; set; }
    [Tag("IBSCBSTot", "W34")]      public TotalIbsCbs? IbsCbs { get; set; }
    [Tag("vNFTot", "W60")]         public decimal? ValorTotalNota { get; set; }
}

public sealed class TotalIcms
{
    [Tag("vBC", "W03")]            public decimal? BaseCalculo { get; set; }
    [Tag("vICMS", "W04")]          public decimal? Valor { get; set; }
    [Tag("vICMSDeson", "W04a")]    public decimal? ValorDesonerado { get; set; }
    [Tag("vFCPUFDest", "W04c")]    public decimal? ValorFcpUFDestino { get; set; }
    [Tag("vICMSUFDest", "W04e")]   public decimal? ValorIcmsUFDestino { get; set; }
    [Tag("vICMSUFRemet", "W04g")]  public decimal? ValorIcmsUFRemetente { get; set; }
    [Tag("vFCP", "W04h")]          public decimal? ValorFcp { get; set; }
    [Tag("vBCST", "W05")]          public decimal? BaseCalculoSt { get; set; }
    [Tag("vST", "W06")]            public decimal? ValorSt { get; set; }
    [Tag("vFCPST", "W06a")]        public decimal? ValorFcpSt { get; set; }
    [Tag("vFCPSTRet", "W06b")]     public decimal? ValorFcpStRetido { get; set; }
    [Tag("qBCMono", "W06b.1")]     public decimal? QuantidadeBaseMonofasico { get; set; }
    [Tag("vICMSMono", "W06c")]     public decimal? ValorMonofasico { get; set; }
    [Tag("qBCMonoReten", "W06c.1")] public decimal? QuantidadeBaseRetencao { get; set; }
    [Tag("vICMSMonoReten", "W06d")] public decimal? ValorMonofasicoRetencao { get; set; }
    [Tag("qBCMonoRet", "W06d.1")]  public decimal? QuantidadeBaseRetida { get; set; }
    [Tag("vICMSMonoRet", "W06e")]  public decimal? ValorMonofasicoRetido { get; set; }
    [Tag("vProd", "W07")]          public decimal? ValorProdutos { get; set; }
    [Tag("vFrete", "W08")]         public decimal? Frete { get; set; }
    [Tag("vSeg", "W09")]           public decimal? Seguro { get; set; }
    [Tag("vDesc", "W10")]          public decimal? Desconto { get; set; }
    [Tag("vII", "W11")]            public decimal? ValorImpostoImportacao { get; set; }
    [Tag("vIPI", "W12")]           public decimal? ValorIpi { get; set; }
    [Tag("vIPIDevol", "W12a")]     public decimal? ValorIpiDevolvido { get; set; }
    [Tag("vPIS", "W13")]           public decimal? ValorPis { get; set; }
    [Tag("vCOFINS", "W14")]        public decimal? ValorCofins { get; set; }
    [Tag("vOutro", "W15")]         public decimal? OutrasDespesas { get; set; }
    [Tag("vNF", "W16")]            public decimal? ValorNota { get; set; }
    [Tag("vTotTrib", "W16a")]      public decimal? ValorTotalTributos { get; set; }
}

public sealed class TotalIssqn
{
    [Tag("vServ", "W18")]          public decimal? ValorServicos { get; set; }
    [Tag("vBC", "W19")]            public decimal? BaseCalculo { get; set; }
    [Tag("vISS", "W20")]           public decimal? Valor { get; set; }
    [Tag("vPIS", "W21")]           public decimal? ValorPis { get; set; }
    [Tag("vCOFINS", "W22")]        public decimal? ValorCofins { get; set; }
    [Tag("dCompet", "W22a")]       public DateOnly? Competencia { get; set; }
    [Tag("vDeducao", "W22b")]      public decimal? Deducao { get; set; }
    [Tag("vOutro", "W22c")]        public decimal? OutrasRetencoes { get; set; }
    [Tag("vDescIncond", "W22d")]   public decimal? DescontoIncondicionado { get; set; }
    [Tag("vDescCond", "W22e")]     public decimal? DescontoCondicionado { get; set; }
    [Tag("vISSRet", "W22f")]       public decimal? Retencao { get; set; }
    [Tag("cRegTrib", "W22g")]      public RegimeEspecialTributacao? RegimeEspecial { get; set; }
}

public sealed class RetencaoTributos
{
    [Tag("vRetPIS", "W24")]        public decimal? Pis { get; set; }
    [Tag("vRetCOFINS", "W25")]     public decimal? Cofins { get; set; }
    [Tag("vRetCSLL", "W26")]       public decimal? Csll { get; set; }
    [Tag("vBCIRRF", "W27")]        public decimal? BaseCalculoIrrf { get; set; }
    [Tag("vIRRF", "W28")]          public decimal? Irrf { get; set; }
    [Tag("vBCRetPrev", "W29")]     public decimal? BaseCalculoPrevidencia { get; set; }
    [Tag("vRetPrev", "W30")]       public decimal? Previdencia { get; set; }
}

public sealed class TotalImpostoSeletivo
{
    [Tag("vIS", "W33")]            public decimal? Valor { get; set; }
}

public sealed class TotalIbsCbs
{
    [Tag("vBCIBSCBS", "W35")]      public decimal? BaseCalculo { get; set; }
    [Tag("gIBS", "W36")]           public TotalIbs? Ibs { get; set; }
    [Tag("gCBS", "W50")]           public TotalCbs? Cbs { get; set; }
    [Tag("gMono", "W57")]          public TotalMonofasia? Monofasia { get; set; }
    [Tag("gEstornoCred", "W59e")]  public EstornoCredito? EstornoCredito { get; set; }
}

public sealed class TotalIbs
{
    [Tag("gIBSUF", "W37")]         public TotalIbsUF UF { get; set; } = new();
    [Tag("gIBSMun", "W42")]        public TotalIbsMunicipio Municipio { get; set; } = new();
    [Tag("vIBS", "W47")]           public decimal? Valor { get; set; }
    [Tag("vCredPres", "W48")]      public decimal? CreditoPresumido { get; set; }
    [Tag("vCredPresCondSus", "W49")] public decimal? CreditoPresumidoCondicaoSuspensiva { get; set; }
}

public sealed class TotalIbsUF
{
    [Tag("vDif", "W38")]           public decimal? Diferimento { get; set; }
    [Tag("vDevTrib", "W39")]       public decimal? Devolucao { get; set; }
    [Tag("vIBSUF", "W41")]         public decimal? Valor { get; set; }
}

public sealed class TotalIbsMunicipio
{
    [Tag("vDif", "W43")]           public decimal? Diferimento { get; set; }
    [Tag("vDevTrib", "W44")]       public decimal? Devolucao { get; set; }
    [Tag("vIBSMun", "W46")]        public decimal? Valor { get; set; }
}

public sealed class TotalCbs
{
    [Tag("vDif", "W53")]           public decimal? Diferimento { get; set; }
    [Tag("vDevTrib", "W54")]       public decimal? Devolucao { get; set; }
    [Tag("vCBS", "W56")]           public decimal? Valor { get; set; }
    [Tag("vCredPres", "W56a")]     public decimal? CreditoPresumido { get; set; }
    [Tag("vCredPresCondSus", "W56b")] public decimal? CreditoPresumidoCondicaoSuspensiva { get; set; }
}

public sealed class TotalMonofasia
{
    [Tag("vIBSMono", "W58")]       public decimal? Ibs { get; set; }
    [Tag("vCBSMono", "W59")]       public decimal? Cbs { get; set; }
    [Tag("vIBSMonoReten", "W59a")] public decimal? IbsRetencao { get; set; }
    [Tag("vCBSMonoReten", "W59b")] public decimal? CbsRetencao { get; set; }
    [Tag("vIBSMonoRet", "W59c")]   public decimal? IbsRetido { get; set; }
    [Tag("vCBSMonoRet", "W59d")]   public decimal? CbsRetido { get; set; }
}
```

O `gEstornoCred` do total é a `EstornoCredito` do item (decisão 29).

### 11.8. Enumerações

O valor de cada membro é o do XML, como na 2.2; os CSTs saem com os dígitos do campo (`CstIcms.Tributada` = 0 sai `00`, `CstPisCofins.AliquotaBasica` = 1 sai `01`). Os dois de valor não numérico (`IndicadorEscala`, `CondicaoVin`) têm membros 1 e 2 e o leiaute traduz, como o `ModeloCupomFiscal` (decisão 31). Os nomes evitam "Sem", como na 2.2 (`ClassificacaoCreditoPresumidoZfm.Nenhum`, `RestricaoVeiculo.Nenhuma`).

```csharp
public enum OrigemMercadoria                                                                       // orig: o Torig
{
    Nacional = 0, EstrangeiraImportacaoDireta = 1, EstrangeiraMercadoInterno = 2, NacionalConteudoImportacaoMedio = 3,
    NacionalProcessoProdutivoBasico = 4, NacionalConteudoImportacaoBaixo = 5, EstrangeiraImportacaoDiretaCamex = 6,
    EstrangeiraMercadoInternoCamex = 7, NacionalConteudoImportacaoAlto = 8,
}
public enum CstIcms                                                                                // CST do ICMS (decisão 24)
{
    Tributada = 0, MonofasicaPropria = 2, TributadaSubstituicao = 10, MonofasicaPropriaRetencao = 15, ReducaoBase = 20,
    IsentaSubstituicao = 30, Isenta = 40, NaoTributada = 41, Suspensao = 50, Diferimento = 51, MonofasicaDiferida = 53,
    CobradoAnteriormente = 60, MonofasicaCobradaAnteriormente = 61, ReducaoBaseSubstituicao = 70, Outras = 90,
}
public enum CsosnIcms                                                                              // CSOSN
{
    PermiteCredito = 101, NaoPermiteCredito = 102, IsencaoFaixaReceita = 103, PermiteCreditoSubstituicao = 201,
    NaoPermiteCreditoSubstituicao = 202, IsencaoFaixaReceitaSubstituicao = 203, Imune = 300, NaoTributada = 400,
    CobradoAnteriormente = 500, Outros = 900,
}
public enum ModalidadeBaseCalculo { MargemValorAgregado = 0, Pauta = 1, PrecoTabeladoMaximo = 2, ValorOperacao = 3 }   // modBC
public enum ModalidadeBaseCalculoSt                                                                // modBCST
{
    PrecoTabelado = 0, ListaNegativa = 1, ListaPositiva = 2, ListaNeutra = 3, MargemValorAgregado = 4, Pauta = 5, ValorOperacao = 6,
}
public enum MotivoDesoneracao                                                                      // motDesICMS e motDesICMSST: a união dos grupos
{
    Taxi = 1, Agropecuaria = 3, FrotistaLocadora = 4, DiplomaticoConsular = 5, UtilitariosMotocicletasAmazonia = 6, Suframa = 7,
    OrgaoPublico = 8, Outros = 9, DeficienteCondutor = 10, DeficienteNaoCondutor = 11, FomentoAgropecuario = 12,
    OlimpiadasRio2016 = 16, SolicitadoFisco = 90,
}
public enum IndicadorDeducaoDesoneracao { NaoDeduz = 0, Deduz = 1 }                                // indDeduzDeson
public enum MotivoReducaoAdRem { TransporteColetivo = 1, Outros = 9 }                              // motRedAdRem
public enum CstIpi                                                                                 // CST do IPI
{
    EntradaRecuperacaoCredito = 0, EntradaAliquotaZero = 1, EntradaIsenta = 2, EntradaNaoTributada = 3, EntradaImune = 4,
    EntradaSuspensao = 5, OutrasEntradas = 49, SaidaTributada = 50, SaidaAliquotaZero = 51, SaidaIsenta = 52,
    SaidaNaoTributada = 53, SaidaImune = 54, SaidaSuspensao = 55, OutrasSaidas = 99,
}
public enum CstPisCofins                                                                           // CST do PIS e da COFINS (os mesmos códigos)
{
    AliquotaBasica = 1, AliquotaDiferenciada = 2, AliquotaUnidade = 3, MonofasicaRevendaAliquotaZero = 4,
    SubstituicaoTributaria = 5, AliquotaZero = 6, Isenta = 7, NaoIncidencia = 8, Suspensao = 9, OutrasSaidas = 49,
    CreditoReceitaTributada = 50, CreditoReceitaNaoTributada = 51, CreditoReceitaExportacao = 52,
    CreditoReceitasMercadoInterno = 53, CreditoReceitasTributadaExportacao = 54, CreditoReceitasNaoTributadaExportacao = 55,
    CreditoReceitasTodas = 56, CreditoPresumidoReceitaTributada = 60, CreditoPresumidoReceitaNaoTributada = 61,
    CreditoPresumidoReceitaExportacao = 62, CreditoPresumidoReceitasMercadoInterno = 63,
    CreditoPresumidoReceitasTributadaExportacao = 64, CreditoPresumidoReceitasNaoTributadaExportacao = 65,
    CreditoPresumidoReceitasTodas = 66, CreditoPresumidoOutras = 67, AquisicaoCreditoVedado = 70, AquisicaoIsencao = 71,
    AquisicaoSuspensao = 72, AquisicaoAliquotaZero = 73, AquisicaoNaoIncidencia = 74, AquisicaoSubstituicao = 75,
    OutrasEntradas = 98, Outras = 99,
}
public enum IndicadorEscala { Relevante = 1, NaoRelevante = 2 }                                    // indEscala: "S", "N"
public enum IndicadorTotal { NaoCompoe = 0, Compoe = 1 }                                           // indTot, indSomaPISST, indSomaCOFINSST
public enum IndicadorBemMovelUsado { Usado = 1 }                                                   // indBemMovelUsado (decisão 31)
public enum ClassificacaoCreditoPresumidoZfm                                                       // tpCredPresIBSZFM (o TTpCredPresIBSZFM)
{
    Nenhum = 0, BensConsumoFinal = 1, BensCapital = 2, BensIntermediarios = 3, BensInformatica = 4,
}
public enum ViaTransporteInternacional                                                             // tpViaTransp
{
    Maritima = 1, Fluvial = 2, Lacustre = 3, Aerea = 4, Postal = 5, Ferroviaria = 6, Rodoviaria = 7, Conduto = 8,
    MeiosProprios = 9, Ficta = 10, Courier = 11, EntregaMaos = 12, Reboque = 13,
}
public enum FormaImportacao { ContaPropria = 1, ContaOrdem = 2, Encomenda = 3 }                    // tpIntermedio
public enum TipoOperacaoVeiculo { Outros = 0, VendaConcessionaria = 1, FaturamentoDireto = 2, VendaDireta = 3 }   // tpOp
public enum CondicaoVin { Remarcado = 1, Normal = 2 }                                              // VIN: "R", "N"
public enum CondicaoVeiculo { Acabado = 1, Inacabado = 2, SemiAcabado = 3 }                        // condVeic
public enum RestricaoVeiculo                                                                       // tpRest
{
    Nenhuma = 0, AlienacaoFiduciaria = 1, ArrendamentoMercantil = 2, ReservaDominio = 3, PenhorVeiculos = 4, Outras = 9,
}
public enum TipoArma { UsoPermitido = 0, UsoRestrito = 1 }                                         // tpArma
public enum IndicadorImportacao { Nacional = 0, Importado = 1 }                                    // indImport
public enum ExigibilidadeIss                                                                       // indISS
{
    Exigivel = 1, NaoIncidencia = 2, Isencao = 3, Exportacao = 4, Imunidade = 5, SuspensaJudicial = 6, SuspensaAdministrativa = 7,
}
public enum IndicadorIncentivo { Sim = 1, Nao = 2 }                                                // indIncentivo
public enum RegimeEspecialTributacao                                                               // cRegTrib
{
    MicroempresaMunicipal = 1, Estimativa = 2, SociedadeProfissionais = 3, Cooperativa = 4, Mei = 5, MicroempresaPequenoPorte = 6,
}
public enum AplicacaoAliquotaZeroCbs { AreaIncentivada = 1, ProcessoSuframa = 2 }                  // tpALCZFMCBS
public enum IndicadorDoacao { Doacao = 1 }                                                         // indDoacao (decisão 31)
```

O `MotivoDesoneracao` é um só para o `motDesICMS` e o `motDesICMSST` de todos os grupos, com a união dos valores; cada grupo do XSD aceita só uma parte (o ICMS20 aceita 3, 9, 10, 11 e 12; o ICMS40, 1, 3 a 11, 16 e 90), e o schema aponta o valor que o grupo não aceita, como no CST (decisão 24). O `3` é "Uso na agropecuária" em quase todos os grupos e "Produtor Agropecuário" no ICMS40: `Agropecuaria`.

### 11.9. Regras de forma: o que muda

A seção 3 vale inteira, com duas mudanças, as duas abertas pela decisão aprovada na 2.2 e generalizadas aqui:

- **Regra 1 (classe compartilhada):** "o mesmo tipo do XSD em mais de um lugar" passa a ser **o mesmo conteúdo** (as mesmas tags, com os mesmos tipos e ocorrências) em mais de um lugar, declarado no XSD como tipo nomeado (`TDif`, `TCredPres`) ou como grupo local (o `obsCont`, o `gMonoRet`), como a 2.2 já fez com o `obsCont` e o `obsFisco` (decisão 29).
- **Regra 5 (escolha):** a escolha que é todo o conteúdo de um elemento vira subclasses, **repetido ou não** (o `NFref` se repete; o `ICMS`, o `PIS` e o `COFINS`, não) (decisões 23 e 25). As outras escolhas continuam como propriedades exclusivas.

### 11.10. Mapeamento tag → propriedade

Como na seção 4: o caminho do XML (o do `Problema`) e a propriedade (o `Problema.Propriedade`, 2.6); o índice do XML começa em 1, o do modelo em 0. Nas escolhas por subclasse, o passo do grupo (`ICMS00`, `PISAliq`) é o `[Tag]` da classe, e a propriedade é a da subclasse: `Imposto.Icms.BaseCalculo` com `Icms` sendo um `Icms00`.

| Grupo | Caminho no XML | Propriedade | Tipo |
|---|---|---|---|
| H01, H02 | `NFe/infNFe/det[n]` (e `@nItem`) | `Itens[n-1]` (o `@nItem` não tem: decisão 21) | `Item` |
| I01-I17c | `.../det[n]/prod/cProd` ... `indBemMovelUsado` | `Itens[n-1].Produto.Codigo` ... `BemMovelUsado` | `Produto` |
| I05a | `.../prod/NVE[m]` | `Produto.Nve[m-1]` | `string` |
| I05g-I05j | `.../prod/gCred[m]/cCredPresumido` ... | `Produto.CreditosPresumidos[m-1].Codigo` ... | `CreditoPresumidoIcms` |
| I18-I29a | `.../prod/DI[m]/nDI` ..., `DI[m]/adi[k]/nAdicao` ... | `Produto.Importacoes[m-1].Numero` ..., `.Adicoes[k-1].Numero` ... | `DeclaracaoImportacao`, `Adicao` |
| I50-I55 | `.../prod/detExport[m]/nDraw`, `exportInd/nRE` ... | `Produto.Exportacoes[m-1].Drawback`, `.Indireta.RegistroExportacao` ... | `DetalheExportacao`, `ExportacaoIndireta` |
| I60-I70 | `.../prod/xPed`, `nItemPed`, `nFCI` | `Produto.Pedido`, `ItemPedido`, `NumeroFci` | `Produto` |
| I80-I85 | `.../prod/rastro[m]/nLote` ... `cAgreg` | `Produto.Rastros[m-1].Lote` ... `CodigoAgregacao` | `Rastro` |
| I86-I87c | `.../prod/infProdNFF/...`, `infProdEmb/...` | `Produto.Nff...`, `Produto.Embalagem...` | `ProdutoNff`, `EmbalagemProduto` |
| J01-J26 | `.../prod/veicProd/tpOp` ... `tpRest` | `Produto.VeiculoNovo.TipoOperacao` ... `Restricao` | `VeiculoNovo` |
| K01-K06 | `.../prod/med/cProdANVISA`, `xMotivoIsencao`, `vPMC` | `Produto.Medicamento.CodigoAnvisa`, `MotivoIsencao`, `PrecoMaximoConsumidor` | `Medicamento` |
| L01-L05 | `.../prod/arma[m]/tpArma` ... `descr` | `Produto.Armas[m-1].Tipo` ... `Descricao` | `Arma` |
| LA01-LA21 | `.../prod/comb/cProdANP` ..., `CIDE/...`, `encerrante/...`, `origComb[m]/...` | `Produto.Combustivel.CodigoAnp` ..., `.Cide...`, `.Encerrante...`, `.Origens[m-1]...` | `Combustivel` e os grupos |
| LB01 | `.../prod/nRECOPI` | `Produto.Recopi` | `string` |
| M01, M02 | `.../det[n]/imposto`, `imposto/vTotTrib` | `Itens[n-1].Imposto`, `Imposto.ValorTotalTributos` | `Imposto` |
| N01-N48 | `.../imposto/ICMS/ICMS00/vBC` (e os outros 20 grupos) | `Imposto.Icms.BaseCalculo` (o `Icms` é um `Icms00`) | `Icms` e as subclasses |
| NA01-NA17 | `.../imposto/ICMSUFDest/vBCUFDest` ... | `Imposto.IcmsUFDestino.BaseCalculo` ... | `IcmsUFDestino` |
| O01-O14 | `.../imposto/IPI/cEnq`, `IPI/IPITrib/vBC`, `IPI/IPINT/CST` ... | `Imposto.Ipi.CodigoEnquadramento`, `.Tributado.BaseCalculo`, `.NaoTributado.Cst` ... | `Ipi` e os grupos |
| P01-P05 | `.../imposto/II/vBC` ... `vIOF` | `Imposto.ImpostoImportacao.BaseCalculo` ... `Iof` | `ImpostoImportacao` |
| Q01-Q11 | `.../imposto/PIS/PISAliq/vBC` (e `PISQtde`, `PISNT`, `PISOutr`) | `Imposto.Pis.BaseCalculo` (o `Pis` é um `PisAliquota`) | `Pis` e as subclasses |
| R01-R07 | `.../imposto/PISST/vBC` ... `indSomaPISST` | `Imposto.PisSt.BaseCalculo` ... `Totalizacao` | `PisSt` |
| S01-S11 | `.../imposto/COFINS/COFINSAliq/vBC` ... | `Imposto.Cofins.BaseCalculo` ... | `Cofins` e as subclasses |
| T01-T07 | `.../imposto/COFINSST/vBC` ... `indSomaCOFINSST` | `Imposto.CofinsSt.BaseCalculo` ... `Totalizacao` | `CofinsSt` |
| U01-U17 | `.../imposto/ISSQN/vBC` ... `indIncentivo` | `Imposto.Issqn.BaseCalculo` ... `IncentivoFiscal` | `Issqn` |
| UB01-UB11 | `.../imposto/IS/CSTIS` ... `vIS` | `Imposto.ImpostoSeletivo.Cst` ... `Valor` | `ImpostoSeletivo` |
| UB12-UB14a | `.../imposto/IBSCBS/CST`, `cClassTrib`, `indDoacao` | `Imposto.IbsCbs.Cst`, `ClassificacaoTributaria`, `Doacao` | `IbsCbs` |
| UB15-UB82g | `.../IBSCBS/gIBSCBS/vBC`, `gIBSUF/gDif/pDif` ... | `IbsCbs.Tributacao.BaseCalculo`, `.IbsUF.Diferimento.Percentual` ... | `TributacaoIbsCbs` e os grupos |
| UB84-UB105b | `.../IBSCBS/gIBSCBSMono/gIBSMonoAdRem/gMonoPadrao/qBCMono` ... | `IbsCbs.Monofasia.IbsAdRem.Padrao.Quantidade` ... | `MonofasiaIbsCbs` e os grupos |
| UB106-UB134 | `.../IBSCBS/gTransfCred/...`, `gAjusteCompet/...`, `gEstornoCred/...`, `gCredPresOper/...`, `gCredPresIBSZFM/...` | `IbsCbs.TransferenciaCredito...`, `.AjusteCompetencia...`, `.EstornoCredito...`, `.CreditoPresumido...`, `.CreditoPresumidoZfm...` | os grupos |
| UA01-UA04 | `.../det[n]/impostoDevol/pDevol`, `IPI/vIPIDevol` | `Itens[n-1].ImpostoDevolvido.Percentual`, `.Ipi.Valor` | `ImpostoDevolvido`, `IpiDevolvido` |
| V01 | `.../det[n]/infAdProd` | `Itens[n-1].InformacoesAdicionais` | `string` |
| VA01-VA07 | `.../det[n]/obsItem/obsCont/@xCampo`, `xTexto` (e `obsFisco`) | `Itens[n-1].Observacoes.Contribuinte.Campo`, `Texto` (e `Fisco`) | `ObservacoesItem`, `Observacao` |
| VB01 | `.../det[n]/vItem` | `Itens[n-1].ValorTotal` | `decimal` |
| VC01-VC03 | `.../det[n]/DFeReferenciado/chaveAcesso`, `nItem` | `Itens[n-1].Referencia.Chave`, `NumeroItem` | `ItemReferenciado` |
| W02-W16a | `NFe/infNFe/total/ICMSTot/vBC` ... `vTotTrib` | `Total.Icms.BaseCalculo` ... `ValorTotalTributos` | `TotalIcms` |
| W17-W22g | `.../total/ISSQNtot/vServ` ... `cRegTrib` | `Total.Issqn.ValorServicos` ... `RegimeEspecial` | `TotalIssqn` |
| W23-W30 | `.../total/retTrib/vRetPIS` ... `vRetPrev` | `Total.Retencao.Pis` ... `Previdencia` | `RetencaoTributos` |
| W31-W33 | `.../total/ISTot/vIS` | `Total.ImpostoSeletivo.Valor` | `TotalImpostoSeletivo` |
| W34-W59g | `.../total/IBSCBSTot/vBCIBSCBS`, `gIBS/gIBSUF/vDif` ..., `gEstornoCred/...` | `Total.IbsCbs.BaseCalculo`, `.Ibs.UF.Diferimento` ..., `.EstornoCredito...` | `TotalIbsCbs` e os grupos |
| W60 | `.../total/vNFTot` | `Total.ValorTotalNota` | `decimal` |

**Na 2.6:** além das regras da seção 4, o passo de uma escolha por subclasse (`ICMS00`, `PISQtde`) acha, entre as subclasses do tipo da propriedade (`Icms`), a que tem aquele `[Tag]` na classe, e o caminho segue nas propriedades dela (inclusive as herdadas: `orig` é a `Icms.Origem`, o `CST` do PIS é a `Pis.Cst`). A conversão só olha os tipos, sem a instância.

### 11.11. Exemplo de uso

```csharp
var item = new Item
{
    Produto =
    {
        Codigo = "123", Gtin = "SEM GTIN", Descricao = "CAFE TORRADO 500G", Ncm = "09012100", Cfop = 5102,
        UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m, Valor = 50.00m,
        GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m, ValorUnitarioTributavel = 25.00m,
        Totalizacao = IndicadorTotal.Compoe,
    },
    Imposto =
    {
        Icms = new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito },
        Pis = new PisOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
        Cofins = new CofinsOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
        IbsCbs = new IbsCbs
        {
            Cst = "000", ClassificacaoTributaria = "000001",
            Tributacao = new TributacaoIbsCbs
            {
                BaseCalculo = 50.00m, ValorIbs = 0.05m,
                IbsUF = { Aliquota = 0.10m, Valor = 0.05m },
                IbsMunicipio = { Aliquota = 0m, Valor = 0m },
                Cbs = { Aliquota = 0.90m, Valor = 0.45m },
            },
        },
    },
};
nota.Itens.Add(item);                                     // nItem = 1: a posição (decisão 21)
nota.Total.Icms.ValorProdutos = 50.00m;                   // o total é de quem emite (decisão 33)
nota.Total.Icms.ValorNota = 50.00m;

// leitura (2.5): os grupos de ICMS pelo tipo; a origem e o CST do PIS sem switch
foreach (var lido in lida.Itens)
{
    var origem = lido.Imposto.Icms?.Origem;
    var cstPis = lido.Imposto.Pis?.Cst;
    var baseIcms = lido.Imposto.Icms switch
    {
        Icms00 i => i.BaseCalculo,
        Icms20 i => i.BaseCalculo,
        IcmsSn900 i => i.BaseCalculo,
        _ => null,                                        // grupo novo em versão menor (decisão 17)
    };
}
```

### 11.12. Decisões (aprovadas em 2026-10-20)

Todas pela recomendação (a). A numeração continua a da seção 8.

21. **O `nItem`** (a decisão 19 deixou para cá). (a) **a posição na lista: a escrita numera de 1 a n, a leitura confere a sequência (o item fora de ordem ou com número repetido é problema, e o item fica na posição em que veio)**; (b) uma propriedade `Item.Numero`. Recomendo (a): o MOC exige a sequência a partir de 1 (regra H02-10, "nItem fora de ordem"), e dois números para a mesma coisa só abririam espaço para divergir; quem precisa do número usa o índice + 1.
22. **Nomes de `Item`, `Total` e dos grupos principais.** (a) **`Item` e `Total` ficam (os nomes provisórios da 2.2 viram definitivos); o `prod` é `Item.Produto` (`Produto`, como no `PROJETO.md` 4.1 e no `PLANO.md` 2.6: `Itens[2].Produto.Valor`); o `imposto` é `Item.Imposto` (`Imposto`, no singular, como no XML); o `impostoDevol`, `ImpostoDevolvido`; o `DFeReferenciado`, `Item.Referencia` (`ItemReferenciado`, para não confundir com a `DocumentoReferenciado` do `NFref`); o `veicProd`, `VeiculoNovo` (a `Veiculo` é a do transporte)**; (b) `ItemNota`, `TotalNota`, `Tributos`. Recomendo (a): `Item` e `Total` não colidem com nada no namespace e são o que o XML e o MOC chamam assim; o prefixo "Nota" não acrescenta.
23. **O ICMS.** (a) **base abstrata fechada `Icms` (`private protected`, como a `DocumentoReferenciado`) com a `Origem`, que está nos 21 grupos com a mesma tag e o mesmo identificador; uma subclasse selada por grupo, com o `[Tag]` na classe e o nome do grupo do XML: `Icms00` ... `Icms90`, `IcmsSn101` ... `IcmsSn900`, e `IcmsPartilha` (`ICMSPart`) e `IcmsRepasse` (`ICMSST`, o repasse do ST retido anteriormente), que o XML não nomeia pelo CST**; (b) nomes descritivos (`IcmsTributadoIntegralmente`, `IcmsSimplesNacionalPermiteCredito`); (c) uma classe só com todos os campos e o CST. Recomendo (a): é o que o `PROJETO.md` 4.3 manda e a decisão 11 preparou; o nome com o número é o que quem conhece a NF-e procura, e o nome descritivo não diz qual CST o grupo aceita (o `Icms40` aceita 40, 41 e 50); (c) aceitaria qualquer combinação de campos e perderia a ordem do XSD de cada grupo. Intermediárias (`IcmsComSt`) não entram: os identificadores do MOC mudam entre os grupos no mesmo campo (o `vICMSOp` é `N16a` no ICMS51 e `N16b` no ICMS90), e a herança obrigaria um só.
24. **O CST e o CSOSN.** (a) **propriedade em todos os grupos, inclusive nos que o XSD só aceita um valor (o `Icms00` só aceita `00`): `null` na escrita = o valor único do grupo; preenchido com um valor que o grupo não aceita, o schema aponta (a enumeração do XSD), com a propriedade; a leitura sempre preenche. Na base quando a tag e o identificador são os mesmos em todas as subclasses (`Pis.Cst`, `Cofins.Cst`); no ICMS, o `Cst` (`N12`) e o `Csosn` (`N12a`) ficam nas subclasses. Uma enumeração por tributo (`CstIcms`, `CsosnIcms`, `CstIpi`, `CstPisCofins`), com todos os códigos dele**; (b) propriedade só nos grupos de mais de um valor (o `Icms40`, o `IcmsSn102`); (c) uma enumeração por grupo (`CstIcms40 { Isenta, NaoTributada, Suspensao }`). Recomendo (a): quem lê uma nota de compra pega o CST sem `switch` no PIS e na COFINS e com o valor certo em todos os grupos; quem emite a partir do cadastro (que guarda o CST) atribui o mesmo valor que usou para escolher o grupo; e o teste de que todo campo do XSD tem propriedade não ganha exceção. Com (b), o `null` não existiria, mas a leitura do CST dependeria do tipo; (c) multiplica as enumerações (cinco só no ICMS) e o cadastro não guarda o CST por grupo.
25. **PIS, COFINS e IPI.** (a) **o `PIS` e o `COFINS` têm como conteúdo só a escolha de quatro grupos: subclasses de uma base fechada (`Pis`: `PisAliquota`, `PisQuantidade`, `PisNaoTributado`, `PisOutros`; idem `Cofins`), como o ICMS; o `IPITrib` e o `IPINT` dividem o `IPI` com o `cEnq` e outros campos: propriedades exclusivas `Ipi.Tributado` e `Ipi.NaoTributado`**, pela regra 5 com a mudança da 11.9; (b) no PIS e na COFINS, uma classe com quatro propriedades exclusivas (`Imposto.Pis.Aliquota.BaseCalculo`). Recomendo (a): é a mesma forma do ICMS para a mesma forma do XSD, e o caminho fica `Imposto.Pis.BaseCalculo`; (b) acrescentaria um passo que não tem dado.
26. **ICMS ou ISSQN.** O `imposto` tem a escolha (`ICMS`, `IPI`?, `II`?) ou (`IPI`?, `ISSQN`), com o IPI nos dois ramos. (a) **propriedades exclusivas no `Imposto` (`Icms`, `Ipi`, `ImpostoImportacao`, `Issqn`), com um `Ipi` só; o `Icms` e o `Issqn` juntos, ou o `ImpostoImportacao` com o `Issqn`, é `Erro` na montagem, como as outras exclusivas (decisão 10); a escrita escolhe o ramo pelo que está preenchido**; (b) duas classes (`TributacaoMercadoria`, `TributacaoServico`) com uma escolha entre elas. Recomendo (a): é a regra 5 (a escolha divide o pai com o PIS, a COFINS, o IBS...), e (b) criaria um passo no caminho que o XML não tem.
27. **O produto específico** (`veicProd`, `med`, `arma`, `comb`, `nRECOPI`). (a) **propriedades exclusivas no `Produto` (`VeiculoNovo`, `Medicamento`, `Armas`, `Combustivel`, `Recopi`), com `Erro` na montagem se mais de uma vier preenchida; o `arma` é lista (1 a 500 dentro da escolha), e a lista não vazia conta como escolhida**; (b) uma base `ProdutoEspecifico` com subclasses. Recomendo (a): a escolha divide o `prod` com todos os outros campos (regra 5), e o `nRECOPI` é um valor, não um grupo.
28. **Códigos de tabelas fora do XSD.** (a) **texto: o CST e a classificação tributária do IBS, da CBS e do IS (`CST`, `cClassTrib`, `CSTIS`, `cClassTribIS`, `CSTReg`, `cClassTribReg`), o `cCredPres`, o `cBenef`, o `cBenefRBC`, o `cCredPresumido`, o `cEnq`, o `cListServ`, o NCM, o CEST, o `EXTIPI`, o `cProdANP`, o `cProdANVISA` e os códigos do RENAVAM e do DENATRAN (`tpComb`, `tpVeic`, `cMod`, `cCorDENATRAN`); enumeração só onde o XSD lista os valores (e o `tPag` e o `tBand` da 2.2, tabelas pequenas e estáveis)**; (b) enumeração no CST do IBS e da CBS (uns vinte códigos). Recomendo (a): são tabelas publicadas à parte, grandes (a do `cClassTrib` tem centenas de linhas) e que mudam por informe técnico sem pacote de schemas; com zero à esquerda significativo (`000`, `000001`), o que a decisão 7 já manda para texto; a conferência contra a tabela é regra da 3.2 (`PROJETO.md` 6.2). O CST do IBS e da CBS ainda muda a cada versão da NT 2025.002; enumeração agora seria versão maior a cada código retirado.
29. **Classe compartilhada pelo conteúdo** (estende a 12). (a) **o mesmo conteúdo em mais de um lugar é uma classe só, com o `[Tag]` só com a tag: a `Observacao` da 2.2 também no `obsItem`; `Diferimento`, `DevolucaoTributo` e `ReducaoAliquota` (no IBS da UF, no do município e na CBS); `CreditoPresumidoIbsCbs` (o `gIBSCredPres` e o `gCBSCredPres`); `EstornoCredito` (no item e no total); `MonofasiaIbsRetido`, `MonofasiaCbsRetido`, `MonofasiaIbsBiocombustivel` e `MonofasiaCbsBiocombustivel` (no ad rem e no ad valorem de cada tributo)**; (b) uma classe por lugar, com os identificadores. Recomendo (a): é a decisão 12 aprovada, aplicada igual; são 10 classes a menos (das 23 ocorrências), e o identificador do campo continua achável pelo caminho. Os grupos de mesmo nome e conteúdo diferente (o `gMonoPadrao` do ad rem e o do ad valorem; o `gIBSUF` do item e o do total) são classes diferentes.
30. **Identificadores do MOC repetidos.** O MOC numera o mesmo campo com o mesmo identificador nas alternativas de uma escolha (`N15` é o `vBC` dos 13 grupos de ICMS que o têm; `Q06` é o CST dos quatro grupos de PIS; `O09`, o do `IPITrib` e o do `IPINT`). (a) **usar o identificador da fonte em cada propriedade, repetido onde a fonte repete; o teste `IdentificadoresDoMocNaoSeRepetem` passa a aceitar a repetição só entre as alternativas de uma mesma escolha (as subclasses de uma base, ou as propriedades exclusivas de um pai); as duas repetições dentro de um grupo que a NT 2023.001 v1.60 traz no ICMS53 (`N41a` no `vICMSMonoOp` e no `qBCMonoDif`; `N42` no `pDif` e no `adRemICMSDif`) ficam como na NT, numa lista de exceções do teste; os erros de digitação da fonte (`N17.a`, `N17.b`, `N17.c` no ICMS10 do MOC; `N23.a`, `N23.b`, `N23.d` no ICMS90 da NT 2020.005) são normalizados para a forma dos outros grupos (`N17a`, `N23a`)**; (b) identificadores próprios onde a fonte repete. Recomendo (a): o identificador serve para achar o campo no MOC, e um inventado não se acha; a duplicidade do ICMS53 é da NT e se resolve quando ela for corrigida (versão menor: só o atributo muda).
31. **Valores de enumeração que não são inteiros, e enumerações de um valor.** (a) **`indEscala` (`S`, `N`) e `VIN` (`R`, `N`): enumeração com membros 1 e 2 e a tradução no leiaute, como o `ModeloCupomFiscal` (decisão 8); `pICMSInter` (`4.00`, `7.00`, `12.00`): `decimal?`, porque é percentual e o schema confere a lista; `indBemMovelUsado` e `indDoacao`, que o XSD só aceita com `1`: enumeração de um membro (`IndicadorBemMovelUsado.Usado`, `IndicadorDoacao.Doacao`), não `bool`**; (b) `bool` nos indicadores de um valor (`true` = a tag com `1`). Recomendo (a): a decisão 8 recusou `bool` porque o indicador ganha valores por NT (o `indPres` ganhou o 4 e o 5), e um indicador de um valor é o caso mais provável de ganhar o segundo; o percentual como enumeração obrigaria a converter para calcular.
32. **Tipos dos valores** (aplica a decisão 7). (a) **ficam texto os campos que o XSD tipa como texto mesmo parecendo número (no `veicProd`, a potência, a cilindrada, os pesos, a capacidade de tração e a distância entre eixos; o `xPed` e o `nItemPed`), os códigos de tabela de largura fixa (decisão 28) e o `nFCI` (o GUID em maiúsculas, como o XSD pede; o `Guid` do .NET formata em minúsculas); `competApur` (o `gYearMonth`, `AAAA-MM`) é texto, como o `AAMM` da 2.2; `int?` nos números sem zero à esquerda (`CFOP`, `nAdicao`, `nSeqAdic`, `cOperNFF`, `anoMod`, `anoFab`, `espVeic`, `lota`, `nBico`, `nBomba`, `nTanque`, `cMun`, `cMunFG`, `cPais`, o `nItem` do `DFeReferenciado`); `long?` no `qSelo` (até 12 dígitos); o `cUFOrig` (o código IBGE) é `UF?`, como o `cUF` da identificação; a `UF` que aceita `EX` (`UFCons`, `UFST`) ganha o `Exterior` (decisão 9)**; (b) número onde o conteúdo é numérico (`int?` na potência, `decimal?` nos pesos). Recomendo (a): o modelo não converte (regra 4), e o XSD aceita nesses campos o que um número não guarda (zeros à esquerda, texto livre de até 9 posições).
33. **Os totais.** (a) **o modelo não calcula: o `Total` é preenchido por quem emite (ou pela leitura), e a conferência contra a soma dos itens é regra da 3.2 (rejeições 531 a 535, 564 e as da reforma), com o código da rejeição; um totalizador que preenche o `Total` a partir dos itens, se pedido, entra depois em versão menor, fora do modelo**; (b) `null` no total = calculado na escrita. Recomendo (a): o `PROJETO.md` 1.1 ("os totais informados pelo emitente"; a lib não calcula tributo) e a escrita não altera a nota (seção 6); (b) esconderia na escrita uma regra de soma que muda por NT (o `vNF` com e sem o desonerado, o `indSomaPISST`, o `vNFTot` da reforma).
34. **Nomes repetidos entre os tributos.** (a) **a mesma tag tem o mesmo nome em todos os grupos em que aparece (`vBC` é `BaseCalculo`, `pICMS`/`pPIS`/`pIPI` é `Aliquota`, o valor do tributo do grupo é `Valor`), e o nome do tributo só entra quando o grupo tem mais de um (`ValorIbs`, `ValorCbs`, `AliquotaIbsUF`); nos totais, `Valor` + o tributo (`ValorPis`, `ValorIpi`), salvo os que o `Produto` já nomeia sem `Valor` (`Frete`, `Seguro`, `Desconto`, `OutrasDespesas`); o `vNFTot` é `Total.ValorTotalNota`, ao lado do `Total.Icms.ValorNota` (o `vNF`)**; (b) o nome do tributo em todas (`Icms00.ValorIcms`, `PisAliquota.ValorPis`). Recomendo (a): é a regra 6 (sem o nome do grupo repetido), e o `switch` da leitura usa o mesmo nome em todos os grupos.
35. **NTs sem pacote de schemas.** (a) **o `vUnComLiq`, o `vProdLiq`, o `ICMSPrevistoPagtoAntecip` e o `vProdLiqTot` da NT 2026.008 entram quando o pacote sair (`docs/NTS.md` 1.3), como a decisão 17 já diz**; (b) acrescentar já. Recomendo (a), pela decisão 17.

### 11.13. Divergências e o que fica para depois

- **Com a seção 3:** as duas mudanças da 11.9 (regras 1 e 5).
- **Com o teste da 2.2:** o `IdentificadoresDoMocNaoSeRepetem` muda (decisão 30); o `SoAsClassesCompartilhadasTemTagSemIdentificador` passa a listar as 13 compartilhadas (as três da 2.2 e as dez da decisão 29); o `ADocumentoReferenciadoEFechada` vale também para `Icms`, `Pis` e `Cofins`.
- **Com o `PROJETO.md`:** nenhuma. O 4.3 ("os grupos de ICMS por CST/CSOSN são subclasses de um tipo base") é a decisão 23; o 4.6 (IBS, CBS e IS desde o início), a seção 11.5.
- **Para a 2.4:** escrever o CST do grupo de valor único quando a propriedade está nula (decisão 24); as exclusivas novas (decisões 25 a 27) como `Erro` na montagem; os `S`/`N`/`R` do `indEscala` e do `VIN` (decisão 31).
- **Para a 2.5:** a conferência da sequência do `nItem` (decisão 21); o valor fora das enumerações de CST lido como o número (decisão 8).
- **Para a 3.2:** os totais contra os itens (decisão 33), o CST contra o grupo (o CST que o grupo não aceita já é erro de schema; a regra é a do CST × CRT e afins), o `cClassTrib` contra a tabela (decisão 28).
- **NT 2026.008:** decisão 35.

## 12. Implementação (etapa 2.3, 2026-10-20)

- **Onde:** `src/OpenDFe.NFe.Modelo/`, um arquivo por tipo: 106 classes novas, `Item` e `Total` preenchidas e 27 enumerações, como na seção 11; a `Observacao` com a documentação estendida ao `obsItem` (VA03 a VA07). São 1.820 símbolos novos no `PublicAPI.Unshipped.txt` (2.591 no modelo). Nenhum tipo, nome ou `[Tag]` mudou em relação ao desenho.
- **Como os arquivos foram feitos:** gerados por um script (fora do repositório) a partir dos blocos de código da seção 11, que conferiu no XSD cada tag e o máximo de cada lista; a documentação XML foi escrita no script, com o "opcional" e os valores do CST de cada grupo tirados do XSD (o `Icms40` documenta 40, 41 e 50; o `Icms00`, o 00 como o valor da escrita com a propriedade nula). Daqui em diante, os arquivos são editados à mão.
- **O teste do modelo** (`tests/OpenDFe.NFe.Testes/ModeloNFeTestes.cs`), com as mudanças da 11.13:
  - o `det` e o `total` entram na conferência contra o XSD, e o `@nItem` fica fora (decisão 21); a subclasse é conferida com as propriedades herdadas da base (a `Icms.Origem`, o `Pis.Cst`, o `Cofins.Cst`); o `IPI`, que o XSD repete nos dois ramos da escolha do `imposto`, conta uma vez, como opcional;
  - a enumeração usada em mais de um lugar (os CSTs, o `MotivoDesoneracao`, o `IndicadorTotal`) é conferida pela soma: os valores de cada lugar do XSD precisam estar nela, e o valor dela que não está em nenhum lugar é falha; o `IndicadorEscala` (`S`, `N`) e a `CondicaoVin` (`R`, `N`) são traduzidas como o `ModeloCupomFiscal`. Todas as 27 enumerações novas têm a lista no XSD: o `MeioPagamento` e a `BandeiraCartao` continuam as únicas sem ela;
  - `IdentificadoresDoMocNaoSeRepetem` passa a `IdentificadoresDoMocSoSeRepetemEntreAlternativas` (decisão 30): a repetição vale entre as subclasses de uma base, no par `IpiTributado` e `IpiNaoTributado` e nas duas do ICMS53 (`N41a`, `N42`);
  - `SoAsClassesCompartilhadasTemTagSemIdentificador` lista **12** classes, e não as 13 da 11.13: a `Observacao` estava nas duas contas (as três da 2.2 e as nove novas da decisão 29);
  - `ADocumentoReferenciadoEFechada` passa a `ABaseDeEscolhaEFechada` (a `DocumentoReferenciado` com 6 subclasses, a `Icms` com 21, a `Pis` e a `Cofins` com 4), mais `SoAsEscolhasPorSubclasseSaoAbstratas`; novos `OItemNovoTemSoOsGruposObrigatorios` e `MontaOItemDoExemploDoDesenho` (o exemplo da 11.11).
  - **Mutações conferidas:** uma tag trocada numa subclasse do ICMS, a tag do CST trocada na base do PIS, um valor do XSD tirado do `CstIcms`, um valor inventado no `CstIcms`, um grupo obrigatório (`TotalIbs.UF`) anulável, um identificador repetido, um valor do `IndicadorEscala` fora da tradução e o `Exterior` tirado do `Combustivel`: cada uma acusada.
