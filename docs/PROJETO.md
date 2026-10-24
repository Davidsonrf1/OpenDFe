# OpenDFe

Biblioteca .NET aberta para documentos fiscais eletrônicos brasileiros: NF-e e NFC-e primeiro, CT-e e MDF-e depois. Gera o XML, valida pelas regras da SEFAZ, assina, transmite, lê e trata os eventos. **Proposta de 2026-10-16, com a arquitetura revisada em 2026-10-19 (seção 3), aguardando aprovação.** Nada implementado.

Projeto pessoal de Davidson Faria, open source. O ErpCore (módulo fiscal) e o PDV do ErpCore são consumidores como qualquer outro: nada do ErpCore entra aqui, nem dependência, nem convenção.

## 1. Escopo

1.1. **A lib cuida do documento, não da decisão.** Recebe os valores prontos (tributos, CST, CFOP, totais informados pelo emitente) e entrega o documento autorizado ou os problemas. **Não calcula tributo**: quem decide o imposto é o sistema que usa a lib (no ErpCore, o motor fiscal; no PDV, a carga). Ela confere a coerência que a SEFAZ confere (o total é a soma dos itens, o grupo de ICMS combina com o CST), porque esse é o mesmo trabalho da rejeição, feito antes de transmitir.

1.2. **Fora do escopo:** persistência (a lib não grava nada; quem usa guarda o XML e o protocolo), fila e reenvio automático, impressão em impressora (ESC/POS é do aplicativo; a lib entrega o modelo de impressão e o PDF, seção 10), escrituração (SPED), apuração e guias.

1.3. **Documentos:**

| Documento | Modelo | Quando |
|---|---|---|
| NF-e | 55 | primeira versão |
| NFC-e | 65 | primeira versão (mesmo leiaute da NF-e) |
| CT-e, CT-e OS | 57, 67 | depois |
| MDF-e | 58 | depois |
| NFS-e (padrão nacional) | — | v2: é API REST com JSON/XML próprio, outro mundo |

## 2. Princípios

2.1. **Só .NET, distribuída pelo NuGet.** Sem fachada nativa nem bindings para outras linguagens (decidido em 2026-10-16).

2.2. **Alvo: `net10.0`.** O .NET 8 e o 9 saem de suporte em novembro de 2026; uma lib que nasce agora começa no LTS atual.

2.3. **Sem estado global e sem I/O escondido.** Toda dependência externa entra pelo construtor: o `HttpMessageHandler` (ou a fábrica dele), o relógio (`TimeProvider`), o certificado. Nada de configuração estática, `ServicePointManager` ou leitura de arquivo implícita. Os tipos são seguros para uso concorrente.

2.4. **Problema não é exceção.** Validação e rejeição da SEFAZ voltam como resultado (lista de problemas, retorno com `cStat`). Exceção é só para falha de infraestrutura (rede, certificado inválido, XML de resposta ilegível) e erro de programação.

2.5. **Determinística e invariável à cultura.** Valores em `decimal`, formatados pelas casas que o leiaute define para cada campo; datas em `DateTimeOffset` com o fuso; nada depende de `CultureInfo.CurrentCulture`.

2.6. **Tabelas oficiais versionadas com a lib.** Schemas XSD, URLs dos autorizadores, URLs de consulta da NFC-e, códigos da reforma (CST do IBS/CBS, `cClassTrib`): tudo embutido no leiaute, com a versão de origem, e substituível por quem usa (o autorizador que muda de URL não pode esperar uma versão nova da lib). As URLs: seção 8.2.

2.7. **Nomes do domínio em português.** README e documentação em português.

2.8. **Plataforma:** Windows e Linux. No Linux, o **OpenSSL 3 é requisito** (decidido em 2026-10-19), com a política de criptografia da máquina permitindo a assinatura RSA-SHA1 que a NF-e exige; a distribuição que a bloqueia por padrão pede a liberação, documentada no README. A lib não traz criptografia própria.

2.9. **Validação: o schema, e só as regras necessárias** (decidido em 2026-10-22, regra do projeto). A base de toda validação é o XSD, devolvendo o máximo que o próprio schema informa (o campo, o caminho, a propriedade, o tipo, as restrições, o valor e o `xs:documentation` do campo e do tipo). Regra escrita à mão, do MOC ou de uma NT, só entra quando é **necessária**: pega um erro comum de quem emite, que o schema não vê, e quase não muda com as NTs (CPF e CNPJ, DV de chave, totais contra os itens, combinações simples como CFOP × `idDest`, datas). Não se implementa o MOC inteiro: a regra que depende de tabela oficial ou muda a cada NT fica para a SEFAZ, e o catálogo (`docs/REGRAS.md`) a registra como tal. **Toda regra fora do schema é listada e aprovada pelo usuário antes de implementada** (decidido em 2026-10-22): a etapa apresenta a lista (identificador, código, o que confere) e só implementa as aprovadas. Vale para a NF-e, a NFC-e, os eventos e todo documento novo.

## 3. Arquitetura

3.1. **Núcleo e leiaute.** O **núcleo** não conhece nenhum documento: tem os contratos, a assinatura, o SOAP, o TLS, a validação pelo schema e a escolha da URL. O **leiaute** implementa um documento numa versão do leiaute (`ILeiaute`): o mapeamento do modelo para as tags, os schemas, as regras do MOC, as operações, as URLs padrão e o modelo de impressão. NF-e e NFC-e são documentos distintos no mesmo leiaute, com o mesmo modelo de dados.

3.2. **Um núcleo, um leiaute.** O núcleo é criado com um leiaute, um documento e uma configuração; a versão fica implícita no leiaute. Outro documento, outra versão ou outra configuração é outro núcleo:

```csharp
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente     = Ambiente.Homologacao,
    UF           = UF.MG,
    Certificado  = certificado,
    Conexoes     = conexoes,                       // o pool de handlers, de quem usa, compartilhado (8.5)
    Emissao      = TipoEmissao.ContingenciaOffline,
    Contingencia = new Contingencia(inicio, justificativa),   // dhCont e xJust; obrigatória fora do Normal
    Urls         = urlsTrocadas,                   // só as que mudam; o resto vem do padrão do leiaute
    Documento    = new ConfiguracaoNFCe(new CodigoSeguranca(1, csc))   // a parte que o leiaute declara; opcional (sem ela, o QR Code versão 3)
});
```

A configuração leva as **conexões** (`ConexoesSefaz`, obrigatórias: o pool de 8.5, criado e descartado por quem usa) e, com o tipo de emissão diferente de `Normal`, a **contingência** (início e justificativa), conforme o desenho aprovado da etapa 1.6 (`docs/CONTRATOS.md`, decisões 7 e 15).

- **A configuração é imutável.** Entrar em contingência é criar outro núcleo, o que é barato: o leiaute não tem estado e é compartilhado entre núcleos (no ERP multi-tenant, um núcleo por emitente e documento).
- **O leiaute é entregue pelo tipo, não pela DLL.** Quem usa referencia o pacote e passa a instância; o núcleo só enxerga `ILeiaute`. Carregar por caminho de DLL (`DFe.CarregarDe(caminho)`) fica como extra, para leiautes de terceiros, porque custa identidade de tipo entre `AssemblyLoadContext`s, quebra com single-file, trimming e AOT (o PDV é um pacote único e assinado) e abre um caminho para executar código vindo de configuração.
- **O leiaute declara** os destinos que atende (UFs, SVRS, SVAN, AN, SVC-AN, SVC-RS), os serviços e as URLs padrão de cada um, e a parte da configuração que é do documento (o CSC da NFC-e). Quem usa pode listar tudo, por exemplo para uma tela de configuração.

3.3. **Detector.** `DFe.Detectar(xml)` lê a raiz, o modelo e a versão de um XML recebido (`NFe`, `nfeProc`, evento) e diz qual leiaute, entre os registrados por quem usa, sabe lê-lo. É o caminho da caixa de entrada do ErpCore, onde a versão só aparece dentro do XML.

3.4. **Pacotes:**

```
OpenDFe.Nucleo           contratos (ILeiaute, ParametroWS<T>, ResultadoWS<T>, Problema), certificado,
                         assinatura e verificação (XMLDSig), C14N, cliente SOAP 1.2, TLS com a cadeia
                         ICP-Brasil, validação pelo XSD, detector, UF, ambiente, tipo de emissão,
                         documentos (CPF, CNPJ alfanumérico), chave de acesso, operações comuns
OpenDFe.NFe              o modelo NF-e/NFC-e (seção 4), os parâmetros e retornos próprios da NF-e e
                         o leiaute 4.00 (projeto OpenDFe.Leiaute.NFe.v400): mapeamento, XSD, regras
                         do MOC, operações, URLs, QR Code e CSC, modelo do DANFE e do DANFCE
OpenDFe.Impressao.Pdf    renderizador PDF do modelo de impressão
OpenDFe.CTe, OpenDFe.MDFe   depois
```

O modelo e os leiautes de um documento são projetos separados que saem **no mesmo pacote e na mesma versão**, num namespace por pacote (`OpenDFe` no núcleo, `OpenDFe.NFe` no modelo e no leiaute): o leiaute nunca roda contra um modelo de outra versão. Tudo depende de `OpenDFe.Nucleo`; quem só emite NF-e não carrega o PDF, e o PDV (ESC/POS) também não.

## 4. Modelo do documento

4.1. **Um modelo por documento, válido para todas as versões do leiaute**, com **nomes descritivos** (`Destinatario.Nome`, `Produto.Valor`), um grupo por classe. Cada propriedade leva o atributo com a tag e o identificador do campo no MOC (`[Tag("xNome", "E04")]`), que serve à documentação (o IntelliSense mostra a tag), ao problema (que aponta a tag e a propriedade, 6.4) e ao aviso de campo não suportado (4.4). Decidido em 2026-10-19 (alternativa descartada: os nomes das tags).

4.2. **Um campo nunca quebra o de uma versão anterior.** O nome não leva versão; o sufixo com a versão do leiaute (`NomeDestinatarioV5`) só aparece quando uma versão nova for incompatível com a anterior no mesmo campo. Na 4.00 não há sufixo.

4.3. **Escolhas do leiaute viram tipos.** Os grupos de ICMS por CST/CSOSN são subclasses de um tipo base (o leiaute permite um só); as enumerações levam o valor do XML (`TipoEmissao.ContingenciaOffline` ↔ `9`).

4.4. **Campo que o leiaute não suporta** (preenchido no modelo, de uma versão que o leiaute carregado não conhece) fica fora do XML e vira um problema de severidade aviso. Nunca some calado: a nota autorizada sem o campo seria um erro fiscal que ninguém vê.

4.5. **Uma versão de leiaute por leiaute.** As Notas Técnicas acrescentam campos opcionais ao mesmo leiaute; o campo de uma NT ainda não vigente só é escrito se preenchido. Versão nova (5.00, se vier) é outro leiaute, sobre o mesmo modelo.

4.6. **Reforma tributária desde o início:** os grupos de IBS, CBS e IS entram no modelo da primeira versão, porque 2026 já é ano de teste e o leiaute os tem.

## 5. Leitura e escrita do XML

5.1. **O leiaute escreve e lê o XML à mão, com `XmlWriter` e `XmlReader`**, um método por grupo. O XML assinado precisa sair exatamente como o leiaute pede: ordem das tags, tag vazia omitida, decimais com as casas do campo, sem prefixo de namespace, sem espaços. O `XmlSerializer` exigiria propriedades-sombra de texto em cada valor formatado e não garante a forma. Decidido em 2026-10-19 (alternativas descartadas: `XmlSerializer` e um gerador de código a partir do XSD).

5.2. **A leitura aceita o que a SEFAZ aceita:** `NFe`, `nfeProc` (com o protocolo), versões de NT anteriores, e devolve problemas do que não reconhece, sem perder o XML original. O ErpCore já tem um leitor (`ErpCore.Fiscal`, `Nfe/LeitorDeNfe.cs`), que migra para cá.

5.3. **O XML original é preservado.** Ler e escrever de novo não reproduz os bytes da assinatura; quem precisa guardar a nota guarda o XML recebido ou autorizado, nunca o regerado. A lib devolve sempre os dois: o objeto e o XML.

## 6. Validação

Três etapas, nesta ordem, todas antes da transmissão, todas com o mesmo resultado (`Problema`: código, severidade, mensagem, tag, propriedade do modelo):

6.1. **Schema, sempre.** O núcleo valida o XML gerado com os schemas que o leiaute entrega: cada operação declara o schema da própria mensagem (a nota, o lote, o pedido de evento, a inutilização), do pacote de liberação vigente, embutido (`Schemas/<pacote>/`; desde a série 010 o pacote traz só a nota, e a pasta o completa com as mensagens que ele não republica, arquivo a arquivo, registradas em `docs/NTS.md`): o `SchemaXml` do núcleo, que carrega o `XmlSchemaSet` dos recursos do leiaute (sem disco nem rede) e o compila uma vez. Não é opcional.

6.2. **Regras do MOC e das NTs, no leiaute: só as necessárias** (2.9; decidido em 2026-10-22, `docs/REGRAS.md` 11): a base da validação é o schema; das regras, só as que pegam o erro comum de quem emite, que o schema não vê, e que quase não mudam com as NTs: CPF e CNPJ (inclusive o alfanumérico), o DV das chaves informadas, os totais contra a soma dos itens, CFOP × `idDest`, `indIEDest` × IE e as datas. As que dependem de tabela oficial (CST e `cClassTrib` do IBS e da CBS, CFOP, combustíveis) e as que mudam a cada NT ficam para a SEFAZ conferir, até pedido em contrário. Cada regra leva o **código da rejeição** que ela antecipa (ex.: 564, "Total do Produto / Serviço difere do somatório dos itens") e a mensagem do MOC. O repositório mantém o catálogo das rejeições, com as implementadas e as que só a SEFAZ confere (`docs/REGRAS.md`). A regra obrigatória do MOC sai como erro; a facultativa (a UF escolhe se aplica), como aviso. Quem usa pode rebaixar a aviso as regras da lib de um código (`ConfiguracaoDFe.RegrasComoAviso`), para seguir emitindo se a lib aplicar uma regra que o autorizador não aplica; o problema continua no resultado (decidido em 2026-10-22, `docs/REGRAS.md`, decisões 6 e 7).

6.3. **Regras de negócio de quem usa, opcionais:** dois callbacks na configuração do núcleo (`ConfiguracaoDFe.Validacao`, um `ValidacaoCallback`), um chamado **por campo** (`Campo`) e um pelo **documento inteiro** (`Documento`: a nota e o XML), para as regras do sistema, não da SEFAZ. Devolvem `Problema` como as outras etapas. O núcleo os chama depois do schema e das regras do leiaute, sem erro neles, em cada documento do pedido (etapa 3.1, `docs/REGRAS.md` 3). Cada etapa só roda se a anterior não deu erro; dentro da etapa, todas as regras rodam.

6.4. **O problema aponta a origem:** a tag e o caminho no XML (com o índice a partir de 1 no elemento que se repete: `NFe/infNFe/det[3]/prod/vProd`), a propriedade do modelo (pelo atributo `[Tag]`, 4.1) e, no erro de schema, as restrições do schema para o campo (tipo, tamanho, padrão, valores aceitos) e a documentação dele no schema (o `xs:documentation` do campo e do tipo, desde a etapa 3.2), em português: a mensagem é montada pelo núcleo a partir da regra, não traduzida da do .NET. As regras vêm também estruturadas (`Problema.Schema`: o que falhou, o tipo do schema, a documentação do campo e do tipo, as facetas violadas com o valor delas, os elementos esperados, o valor encontrado, a linha e a coluna), para quem quiser mostrar de outro jeito. O valor encontrado pode ser dado pessoal: fica no detalhe, nunca na mensagem.

6.5. **Só na SEFAZ** (duplicidade, situação cadastral, numeração): fora da lib; o retorno da autorização traz o `cStat`.

## 7. Chave de acesso, assinatura e certificado

7.1. **Chave:** geração (`cNF` aleatório, nunca igual ao número; DV módulo 11) e leitura das partes (UF, AAMM, CNPJ ou CPF, modelo, série, número, tipo de emissão).

7.2. **Assinatura XMLDSig enveloped**, canonicalização C14N, referência `#NFe<chave>`, RSA com SHA-1 (o que a NF-e ainda exige; SHA-256 quando a NT mudar). **Verificação** também: confere a assinatura e o certificado do emitente num XML recebido (o ErpCore hoje não confere, `PLANO.md` 15, etapa 10). O titular do certificado é comparado como a SEFAZ compara (MOC 7.0, Anexo I, F03 e F03A): a raiz do CNPJ (o certificado de um estabelecimento assina pelos outros da empresa) ou o CPF inteiro. A cadeia ICP-Brasil, a revogação e a validade na data da emissão ainda não entram na verificação (`PLANO.md`, 1.4).

7.3. **Certificado:** a lib recebe um `X509Certificate2` com a chave privada; carregá-lo é de quem usa (PFX em pasta protegida, repositório do Windows). Utilitário para abrir o PFX e checar validade e CNPJ. **A1 na primeira versão.** O A3 no Windows funciona pelo repositório do sistema (o mesmo `X509Certificate2`); no Linux pede PKCS#11, que fica para depois.

## 8. Operações e transmissão

8.1. **Uma porta só.** Toda operação é `dfe.ExecutarAsync(parametro, ct)`. O parâmetro deriva de `ParametroWS<TRetorno>` e diz o tipo do retorno, que o C# deduz (`ExecutarAsync(new ConsultaStatus())` devolve `ResultadoWS<RetornoStatus>`). O leiaute registra um handler por operação (`IOperacao<TParametro, TRetorno>`); `dfe.Suporta<T>()` e `dfe.Operacoes` dizem o que ele implementa. Operação nova = parâmetro novo + handler, sem mexer no núcleo.

```csharp
public abstract class ParametroWS                 // a base não genérica, só para a restrição de Suporta<T>()
{
    public Destino? Destino { get; init; }        // null = o destino natural (8.3)
}

public abstract class ParametroWS<TRetorno> : ParametroWS where TRetorno : class { }

public sealed class ResultadoWS<TRetorno> where TRetorno : class
{
    public SituacaoResultado Situacao { get; }    // Processado | RecusadoLocalmente
    public bool Processado { get; }               // com ele, Retorno e Motivo não são nulos
    public int? CStat { get; }
    public string? Motivo { get; }
    public TRetorno? Retorno { get; }
    public IReadOnlyList<Problema> Problemas { get; }
    public RastroWS Rastro { get; }               // XmlPedido, XmlRetorno, Autorizador, Url, Inicio, Duracao,
}                                                 // DuracaoTransmissao; o mesmo na FalhaTransmissaoException
```

O rastro é um objeto (`RastroWS`), o mesmo no resultado e na `FalhaTransmissaoException` (a falha de infraestrutura, com `PodeTerSidoProcessado`), e guarda a mensagem (o conteúdo do `nfeDadosMsg` e do `nfeResultMsg`), não o envelope SOAP (`docs/CONTRATOS.md`, decisões 5, 10 e 11).

8.2. **O caminho de toda operação:** o leiaute monta o XML do pedido (com problema bloqueante na montagem, `RecusadoLocalmente`) → o núcleo assina, se o serviço pede → valida pelo schema a mensagem assinada (seção 6; o XSD exige a `Signature`; com problema bloqueante, `RecusadoLocalmente`, nada é enviado) → envelope SOAP 1.2 (na base, escrito à mão, sem importar WSDL) → HTTPS com TLS mútuo → o leiaute lê o retorno. A rejeição da SEFAZ é `Processado` com o `cStat` (2.4).

8.3. **As operações da NF-e e da NFC-e:**

| Operação | Parâmetro | Retorno | Destino natural |
|---|---|---|---|
| Status do serviço | `ConsultaStatus` (no núcleo, comum aos documentos: `CONTRATOS.md` 13) | `RetornoStatus` | UF do núcleo, no tipo de emissão do núcleo |
| Autorização | `Autorizacao(notas)` ou `Autorizacao(xmlsAssinados)`, de 1 a 50 (uma vai na síncrona; de 2 a 50, no lote com recibo: NT 2025.001, `CONTRATOS.md` 14) | `RetornoAutorizacao`: protocolo e `nfeProc` por nota (`ResultadoNota`), ou o recibo | UF do núcleo; a da chave nas assinadas |
| Consulta do recibo | `ConsultaRecibo(recibo) { Xmls }` (com as notas do lote, o `nfeProc`) | `RetornoAutorizacao` | UF do núcleo; a da chave da primeira nota |
| Consulta do protocolo | `ConsultaProtocolo(chave)` (NF-e e NFC-e, `CONTRATOS.md` 15) | `RetornoProtocolo`: a situação, o protocolo e os eventos | UF e tipo de emissão da chave; com o `Destino`, o autorizador normal dele |
| Inutilização | `Inutilizacao(cnpj, serie, inicial, final, justificativa)` (`CONTRATOS.md` 18) | `RetornoInutilizacao` | UF do núcleo |
| Evento | `EnvioEvento(1 a 20 eventos)` (`CONTRATOS.md` 18), o detalhe é subclasse: `Cancelamento`, `CancelamentoSubstituicao`, `CartaCorrecao`, `Manifestacao`, `Epec`... | `RetornoEvento`, com o `procEventoNFe` por evento | o do documento; AN para manifestação e EPEC |
| Distribuição DF-e | `DistribuicaoDFe(ultimoNsu / nsu / chave)` | `RetornoDistribuicao` | AN |
| Consulta cadastro | `ConsultaCadastro(uf, documento)` | `RetornoCadastro` | UF consultada |
| Recuperação | `RecuperacaoAutorizacao(xmlAssinado)` (uma nota; `Autorizacao.NotasEnviadas` tira as notas do rastro) | `RetornoAutorizacao`, com o `nfeProc` se o `digVal` confere | UF e tipo de emissão da chave (8.6) |

Serviços SOAP por trás: `NFeAutorizacao4` (síncrona, `indSinc=1`, e por lote), `NFeRetAutorizacao4`, `NFeConsultaProtocolo4`, `NFeStatusServico4`, `NFeInutilizacao4`, `NFeRecepcaoEvento4`, `NFeDistribuicaoDFe` e `CadConsultaCadastro4`.

**Locais, sem web service:** `dfe.Preparar(nota)` → `DocumentoPreparado { Preparado, Xml assinado, Chave, Problemas }` (gera, assina e valida sem enviar: a NFC-e offline, 9.2; no núcleo, pelo `IPreparo` que o leiaute registra, `CONTRATOS.md` 14), e a `Autorizacao` aceita o XML já assinado (o `preparado.Xml`), para transmitir os mesmos bytes. Desde a 4.4, na NF-e; na NFC-e, com o QR Code (5.1).

8.4. **Escolha da URL:** destino × ambiente × tipo de emissão × serviço, a partir da tabela que o leiaute declara (3.2), com as URLs que quem usa trocou. São dois eixos independentes: o **ambiente** (homologação, produção) e o **tipo de emissão** (normal, SVC-AN, SVC-RS, EPEC, FS-DA, offline da NFC-e), porque a contingência também se testa em homologação, e cada tipo de emissão muda a URL ou o XML de um jeito. **Quando a operação tem um documento ou uma chave, o destino e o tipo de emissão saem dele**, não da configuração do núcleo: a NFC-e emitida offline é transmitida depois, com o núcleo já normal, e a consulta e os eventos de uma nota emitida em SVC seguem o `tpEmis` da chave. A configuração do núcleo vale para a nota nova. O `Destino` do parâmetro sobrepõe tudo.

8.5. **TLS** (decidido na 4.1, 2026-10-22):

- **confiança no servidor:** o que o repositório da máquina aceita e, quando o único erro é a cadeia, a cadeia montada de novo com as ACs raiz da ICP-Brasil embutidas no núcleo (v5, v6, v7, v10 a v13), com `X509ChainTrustMode.CustomRootTrust`; nome divergente nunca passa. É o achado da prova (`PLANO.md` 1.7): nem todo autorizador está na ICP-Brasil. MG (`hnfe`, `hnfce`), o Ambiente Nacional e a SVAN usam ACs públicas (Sectigo/USERTrust, GlobalSign), que só o repositório da máquina tem; SVRS, SP, PR, GO, MS, MT e PE, a Raiz v10 da ICP-Brasil, que o Windows tem e o Linux não. Só as raízes ICP-Brasil recusariam MG; só o sistema recusaria a SVRS no Linux. **Sem conferir a revogação** do certificado do servidor (a LCR das ACs fora do ar não pode parar a emissão); quem quiser conferir troca na fábrica;
- autorizador que só negocia cifra antiga: `CipherSuitesPolicy` por autorizador, declarada pelo leiaute (`DeclaracaoLeiaute.DeclararCifras`), num handler próprio, nunca rebaixando o padrão da máquina inteira. O .NET só a aplica no Linux; no Windows vale o padrão do sistema. A tabela do leiaute da NF-e está vazia (nenhum autorizador conhecido exige; sem o PFX, a 4.2 não pôde conferir o que cada um negocia, e fica para a homologação real, 17);
- **um `HttpMessageHandler` por certificado** (e política de cifras), num pool compartilhado entre os núcleos do mesmo emitente: o certificado do TLS mútuo está no handler, e o ERP multi-tenant não pode abrir conexões novas a cada núcleo. O pool é do pacote do núcleo, mas a instância é de quem usa (`ConexoesSefaz`, `IDisposable`, uma por processo, obrigatória na configuração; `docs/CONTRATOS.md`, decisão 7), não estado global nem do núcleo. O pool guarda uma cópia do certificado e descarta o handler ocioso há mais de `TempoOcioso` (30 minutos), conferido a cada transmissão, sem timer e nunca com o handler em uso;
- **a fábrica do handler é trocável** (2.3): `new ConexoesSefaz(Func<ConexaoSefaz, HttpMessageHandler>)`; a `ConexaoSefaz` traz o certificado, as cifras e o `CriarPadrao()`, o handler da lib, para quem só quer ajustá-lo (outra validação do servidor, um proxy, as intermediárias do certificado no handshake);
- **os tempos:** `TempoConexao` (15 s) e `TempoResposta` (60 s, do envio ao fim da resposta, por transmissão), propriedades `init` do `ConexoesSefaz`;
- **as falhas:** a `FalhaTransmissaoException` diz a espécie (`TipoFalhaTransmissao`: conexão, TLS, certificado recusado, tempo esgotado, HTTP, falha SOAP, resposta ilegível) e se pode ter sido processada. Não foi processado: DNS, conexão recusada, TLS, o HTTP 403 sem envelope (o IIS da SVRS e de SP recusando o certificado do cliente, achado da 1.7), a falha SOAP do remetente (`Sender` e afins) e o HTTP 4xx. Pode ter sido: o tempo esgotado e a conexão caída depois do envio, a falha `Receiver`, o HTTP 5xx e a resposta ilegível.

8.6. **Sem reenvio automático.** A lib não repete a autorização sozinha: quem chama decide. Para a queda no meio (a SEFAZ autorizou e a resposta se perdeu), a `RecuperacaoAutorizacao` consulta pela chave e monta o `nfeProc` com o protocolo encontrado; a duplicidade (rejeição 204 e afins) vem identificada no retorno, com a chave (`ResultadoNota.Duplicidade`). O guia "o que fazer quando cai" faz parte da documentação: `docs/QUANDO_CAI.md` (4.5).

8.7. **Resultado da autorização:** o XML distribuível (`nfeProc`, nota e protocolo), o `cStat` e o motivo, e o XML de envio e de retorno, para quem quiser guardar o rastro.

## 9. NFC-e

9.1. **QR Code** gerado pela lib (texto do `qrCode` e `urlChave` do `infNFeSupl`; a imagem é do DANFCE, fase 7): a **versão 3** por padrão (sem CSC, a única do emitente CPF e da NF-e com `tpImp` 6) e a **versão 2** só com o `CodigoSeguranca` (identificador numérico e código, na `ConfiguracaoNFCe`, 3.2); a URL de consulta por UF vem da tabela do leiaute (2.6; só as UFs conferidas, as outras na `ConfiguracaoNFCe`). As formas offline entram na etapa 5.2. Decidido na 5.1 (`CONTRATOS.md` 16).

9.2. **Contingência offline** (`tpEmis=9`): num núcleo com esse tipo de emissão, `Preparar` emite e assina a nota sem a SEFAZ, com a justificativa e a data de entrada em contingência, e o QR Code do modo offline (a versão 3, assinada com o certificado da nota, ou a 2, com o `digVal` e o CSC, conforme a `ConfiguracaoNFCe`; `CONTRATOS.md` 17); a transmissão posterior é a autorização normal do XML preparado, dentro do prazo da legislação. A lib não guarda a fila nem impõe o prazo das 24 horas (depois dele a SEFAZ autoriza com o 150): devolve o XML pronto, e o aplicativo transmite quando a rede volta (`docs/CONTINGENCIA.md`).

9.3. **Cancelamento por substituição** (evento 110112) e o cancelamento comum, como os outros eventos (seção 11).

## 10. DANFE e DANFCE

10.1. **O leiaute monta o modelo de impressão**, porque ele depende do leiaute (grupos, QR Code): o DANFCE como blocos de texto com alinhamento, destaque e o QR Code, e o DANFE como seções. O modelo é neutro, definido no núcleo, e aceita também **imagem de 1 bit** (o logo do emitente no topo e a assinatura do software emissor no rodapé, como o PDV do ErpCore pede): a térmica só imprime preto, e o renderizador ESC/POS a manda como raster.

10.2. **Renderizador PDF em pacote à parte** (`OpenDFe.Impressao.Pdf`), para que nem o núcleo nem o leiaute carreguem a biblioteca de PDF. **QuestPDF** (decidido em 2026-10-19; descartado o PdfSharp/MigraDoc). A licença do QuestPDF é dupla e **não é MIT** (conferido na 7.1, em 2026-10-24: a versão 3.0 de 6 de julho de 2026 é "source-available", não aprovada pela OSI): a Community, gratuita, vale para projetos open source (OSI), pessoas físicas, entidades de caridade, instituições acadêmicas e organizações com receita bruta anual abaixo de USD 1.000.000 (órgãos públicos e empresas de capital aberto ficam de fora, qualquer que seja a receita); fora disso, quem usa compra a licença (Professional ou Enterprise). Quem só usa uma biblioteca que depende do QuestPDF pode se enquadrar como dependência transitiva, mas não se a biblioteca for basicamente um invólucro dele (o texto está no README do pacote, com a data de conferência). Por isso:

- a obrigação é de **quem usa o `OpenDFe.Impressao.Pdf`**, não da OpenDFe, e o README do pacote diz isso logo no início;
- o pacote **não define** `QuestPDF.Settings.License` (é estado global, 2.3, e a escolha da licença é do aplicativo); sem ela definida, o QuestPDF recusa gerar, e o erro aponta o README;
- o núcleo e o leiaute não dependem do QuestPDF; quem não gera PDF (o PDV, com ESC/POS) não o carrega.

10.3. **ESC/POS fica com o aplicativo** (o PDV), a partir do mesmo modelo. Pode virar pacote depois, se houver demanda.

## 11. Eventos

Cancelamento (110111), cancelamento por substituição (110112, NFC-e), carta de correção (110110), manifestação do destinatário (210200, 210210, 210220, 210240), EPEC (110140) e os eventos da reforma tributária conforme as NTs. Mesmo caminho das outras operações (8.2), pela `EnvioEvento`: monta, valida, assina, transmite e devolve o `procEventoNFe`.

## 12. Manutenção pelas Notas Técnicas

12.1. A lib só está certa enquanto acompanha as NTs, e de 2026 a 2033 elas mudam todo ano. **O trabalho é permanente e faz parte do projeto.**

12.2. `docs/NTS.md` registra cada NT: o que mudou, a vigência em homologação e em produção, a versão da OpenDFe que a implementou. O pacote de schemas acompanha.

12.3. **SemVer.** NT nova sem quebra = versão menor. Mudança que obriga quem usa a alterar código = versão maior, com o guia de migração.

## 13. Testes

13.1. **Sem certificado real na CI pública.** Os testes de assinatura geram um certificado de teste na hora: **autoassinado** (RSA 2048, com os campos de titular que o e-CNPJ e o e-CPF da ICP-Brasil levam, DOC-ICP-04), pelo código compartilhado dos testes (`tests/Compartilhado`), fora da API pública. Abrir o PFX e ler o titular não exigem a cadeia ICP-Brasil, para funcionar com ele; o certificado real entra com o PFX de homologação (13.3). A verificação usa XMLs autorizados públicos (exemplos dos manuais) e XMLs reais com os dados trocados.

13.2. **Todo XML gerado passa pelo XSD** nos testes, e a leitura de volta reproduz o objeto.

13.3. **Homologação da SEFAZ:** testes de integração opcionais, ligados por variável de ambiente (caminho e senha do PFX, `OPENDFE_EMITENTE_IE`, e o CSC de homologação em `OPENDFE_CSC_ID` e `OPENDFE_CSC`, só para a NFC-e na versão 2), rodados localmente ou num pipeline com os segredos, nunca em PR de terceiros.

13.4. **CI em Linux e Windows**, porque o TLS e o certificado se comportam diferente nos dois.

13.5. **SEFAZ emulada** (aprovada em 2026-10-19). Um servidor local que responde como os autorizadores, **só para testes**, porque a homologação real recusa o certificado autoassinado no TLS mútuo (só aceita ICP-Brasil) e não reproduz falhas.

- **Para que serve:** desenvolver e testar as fases 4 a 8 sem o PFX real; provocar o que a homologação real não deixa (serviço paralisado, tempo esgotado, autorizar e derrubar a conexão antes da resposta, que é o caso da `RecuperacaoAutorizacao` e do `PodeTerSidoProcessado`, 8.6; contingência SVC com o autorizador normal fora); e servir de ambiente de desenvolvimento ao PDV e ao módulo fiscal do ErpCore.
- **Forma:** aplicação ASP.NET Core (Kestrel, sem pacote novo) em `tests/OpenDFe.SefazEmulada`, fora dos pacotes publicados. Roda sozinha (`dotnet run` ou a imagem Docker, `tests/OpenDFe.SefazEmulada/compose.yaml`) ou dentro dos testes, numa porta livre.
- **TLS:** o certificado do servidor é emitido por uma AC de teste que o emulador gera na primeira partida e exporta (`.crt`). O emulador exige o certificado do cliente e aceita qualquer um, inclusive o autoassinado da 13.1, sem conferir a cadeia. O certificado do TLS identifica o transmissor, que não precisa ser o emitente (MOC 7.0, Visão Geral, 4.3.3): só ele consulta o recibo do lote. O titular comparado ao emitente, como a SEFAZ faz (raiz do CNPJ ou CPF, 7.2), é o do certificado da assinatura.
- **Só homologação:** pedido com `tpAmb=1` é rejeitado (252, ambiente divergente). Do lado de quem usa, a raiz do emulador entra pela fábrica do handler (8.5, 2.3), **sem API pública própria**: a fábrica de teste fica no código compartilhado dos testes, e o ErpCore e o PDV montam a sua só no build de teste. As URLs chegam pelas URLs trocadas (8.4). O emulador atende todos os destinos e serviços no mesmo servidor, com o caminho `/<destino>/<serviço>` (`/MG/NFeStatusServico4`), e lista as URLs prontas para trocar. Confirmado na 4.1: a fábrica de teste (`tests/Compartilhado/ConexaoTeste.cs`) pega o handler padrão e aceita também a cadeia que sobe à AC do emulador.
- **O que confere:** o envelope SOAP, o schema da mensagem, a assinatura e o titular, e um conjunto pequeno de regras que só a SEFAZ confere: duplicidade, numeração, situação da nota para o evento, prazo do cancelamento, sequência da CC-e e faixa da inutilização. O estado (chaves autorizadas, protocolos, eventos, faixas inutilizadas, NSU) fica em memória e volta vazio a cada partida. As regras do MOC que a lib já confere antes (6.2) não são repetidas, salvo as que o teste de uma rejeição precisar.
- **Falhas comandadas:** por uma rota de controle aceita só do próprio computador (`/_controle`), usada pelos testes, e por uma marca no `infCpl` da nota, para provocar a falha manualmente a partir do PDV ou do ERP. As falhas: código de serviço paralisado (108, 109), atraso, tempo esgotado, conexão derrubada antes ou depois de processar, resposta ilegível, falha SOAP e lote preso em processamento (105).
- **Independência da lib:** o emulador lê o pedido e monta o retorno à mão, a partir dos exemplos e dos leiautes dos manuais (MOC, NTs, WSDL), **sem o leitor nem as regras da lib**: se reaproveitasse o código, um erro da lib apareceria igual dos dois lados e passaria. Da lib usa só o que é neutro: o `SchemaXml` (1.5) com os XSDs do leiaute e a verificação da assinatura (1.4).
- **Limite:** o emulador é a nossa leitura do MOC, não a SEFAZ. Não substitui a homologação real (13.3): as SEFAZes divergem entre si (cifras, mensagens, peculiaridades de cada UF), e só a real prova a cadeia ICP-Brasil e as cifras. `docs/SEFAZ_EMULADA.md` registra os serviços, as regras e as falhas que ele implementa, com os códigos do MOC.
- **Em paralelo à lib:** é uma trilha própria do plano, com branch e sessão próprias (`PLANO.md`, "Trilha paralela").

## 14. Distribuição e governança

14.1. **GitHub público** e **NuGet.org**, com a reserva do prefixo `OpenDFe.*` antes da primeira publicação.

14.2. **Licença MIT** (decidido em 2026-10-19; descartada a Apache-2.0, que acrescentaria a concessão de patentes). Permissiva para que produtos fechados, como o ErpCore e o PDV, usem a lib distribuída dentro do executável; LGPL atrapalharia o pacote único e assinado do PDV.

14.3. **Propriedade:** o projeto é pessoal desde o primeiro commit, e o ErpCore o usa sob a licença. Vale deixar isso escrito (README e o `CLAUDE.md` do ErpCore) para não haver dúvida se o ErpCore ganhar sócios ou for vendido.

14.4. **Nome:** "DFe" também aparece em projetos sem relação (o DFE-Digital, do governo britânico); o README deixa claro, já na primeira linha, que é sobre documento fiscal eletrônico brasileiro.

## 15. Consumidores conhecidos

- **ErpCore, módulo fiscal:** o leitor da caixa de entrada (migra para a lib, pelo detector), a verificação da assinatura, o futuro aditivo de NF-e e a distribuição DF-e (as notas destinadas). Um núcleo por emitente e documento, com o pool de handlers por certificado (8.5). A lib entra na lista de pacotes permitidos do backend (`PLANO.md` 1.2).
- **PDV (ErpCore.FrenteCaixa):** NFC-e com contingência offline, cancelamento e DANFCE, no host CEF com o CoreCLR, em Windows e Linux. Leiaute referenciado pelo tipo (3.2), compatível com o pacote único e assinado.

## 16. Fases

| Fase | Conteúdo | Pronto quando |
|---|---|---|
| 0 | Repositório, licença, README, CI Linux e Windows, estrutura dos pacotes | a CI roda vazia nos dois |
| 1 | Núcleo: contratos (`ILeiaute`, `ParametroWS<T>`, `ResultadoWS<T>`, `Problema`), documentos (CPF, CNPJ alfanumérico), UF, chave de acesso, certificado, assinatura e verificação, validação pelo XSD com os problemas em português | um XML de exemplo assina, verifica e valida |
| 2 | Modelo NF-e/NFC-e com IBS/CBS e o atributo `[Tag]`; leiaute 4.00: escrita, leitura, schemas, aviso de campo não suportado; detector | gerar e ler de volta notas de teste válidas no XSD |
| 3 | Regras do MOC, primeiro lote (totais, chave, documentos, combinações principais), callbacks de quem usa e `docs/REGRAS.md` | as rejeições mais comuns barradas antes |
| 4 | Operações: SOAP, tabela de destinos e URLs, escolha da URL, pool de handlers, TLS ICP-Brasil; status, autorização síncrona e por lote, recibo, protocolo, recuperação, `nfeProc` | NF-e autorizada em homologação, no Windows e no Linux |
| 5 | NFC-e: QR Code, CSC, `Preparar` e contingência offline | NFC-e online e em contingência autorizadas em homologação |
| 6 | Eventos: cancelamento, substituição, CC-e, inutilização | os quatro em homologação |
| 7 | Modelo do DANFCE e do DANFE no leiaute, `OpenDFe.Impressao.Pdf` | PDFs conferidos contra o manual |
| 8 | Distribuição DF-e e manifestação | notas destinadas baixadas em homologação |
| paralela | SEFAZ emulada (13.5): status, autorização, consulta e NFC-e, eventos, distribuição, com as falhas comandadas | cada serviço das fases 4 a 8 respondido pelo emulador |
| depois | CT-e, MDF-e, A3 no Linux, carregamento de leiaute por DLL (3.2) | — |
| v2 | NFS-e nacional | — |

As fases 1 a 5 atendem o PDV; a 8, a caixa de entrada do ErpCore.

## 17. Em aberto

Nada em aberto agora.

**Decidido em 2026-10-21:** o "pronto quando" das fases 4 a 8 vale **contra a SEFAZ emulada (13.5)** enquanto não houver o PFX real: o usuário não terá o PFX A1 ICP-Brasil de homologação por enquanto. Cada etapa fecha no emulador, nos dois sistemas, e registra na situação dela (`PLANO.md`) a homologação real como pendente, com o que fica por conferir (a cadeia ICP-Brasil no TLS mútuo, o CSC de homologação na NFC-e, as regras que só a SEFAZ real confere). Quando o PFX chegar, uma etapa própria roda o que ficou pendente contra a homologação de MG, junto com o fechamento da 1.7.
