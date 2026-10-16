# OpenDFe

Biblioteca .NET aberta para documentos fiscais eletrônicos brasileiros: NF-e e NFC-e primeiro, CT-e e MDF-e depois. Gera o XML, valida pelas regras da SEFAZ, assina, transmite, lê e trata os eventos. **Proposta de 2026-10-16, aguardando aprovação.** Nada implementado.

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
| NFS-e (padrão nacional) | — | a avaliar: é API REST com JSON/XML próprio, outro mundo |

## 2. Princípios

2.1. **Só .NET, distribuída pelo NuGet.** Sem fachada nativa nem bindings para outras linguagens (decidido em 2026-10-16).

2.2. **Alvo: `net10.0`.** O .NET 8 e o 9 saem de suporte em novembro de 2026; uma lib que nasce agora começa no LTS atual.

2.3. **Sem estado global e sem I/O escondido.** Toda dependência externa entra pelo construtor: o `HttpMessageHandler` (ou a fábrica dele), o relógio (`TimeProvider`), o certificado. Nada de configuração estática, `ServicePointManager` ou leitura de arquivo implícita. Os tipos são seguros para uso concorrente.

2.4. **Problema não é exceção.** Validação e rejeição da SEFAZ voltam como resultado (lista de problemas, retorno com `cStat`). Exceção é só para falha de infraestrutura (rede, certificado inválido, XML de resposta ilegível) e erro de programação.

2.5. **Determinística e invariável à cultura.** Valores em `decimal`, formatados pelas casas que o leiaute define para cada campo; datas em `DateTimeOffset` com o fuso; nada depende de `CultureInfo.CurrentCulture`.

2.6. **Tabelas oficiais versionadas com a lib.** Schemas XSD, URLs dos autorizadores, URLs de consulta da NFC-e, códigos da reforma (CST do IBS/CBS, `cClassTrib`): tudo embutido, com a versão de origem, e substituível por quem usa (o autorizador que muda de URL não pode esperar uma versão nova da lib).

2.7. **Nomes do domínio em português**, como o leiaute. README e documentação em português.

## 3. Pacotes

```
OpenDFe.Core     certificado, assinatura e verificação (XMLDSig), C14N, cliente SOAP 1.2,
                 TLS com a cadeia ICP-Brasil, UF e ambiente, documentos (CPF, CNPJ alfanumérico)
OpenDFe.NFe      leiaute 4.00 (55 e 65), leitura e escrita, XSD, regras, chave de acesso,
                 serviços (autorização, consulta, status, inutilização, eventos, distribuição DF-e)
OpenDFe.NFCe     QR Code e CSC, URLs de consulta por UF, contingência offline
OpenDFe.Danfe    modelo de impressão do DANFE e do DANFCE, e o PDF
OpenDFe.CTe, OpenDFe.MDFe   depois
```

`OpenDFe.NFCe` e `OpenDFe.Danfe` dependem de `OpenDFe.NFe`; tudo depende de `OpenDFe.Core`. Quem só emite NF-e não carrega o PDF.

## 4. Modelo do documento

4.1. **Classes que espelham o leiaute**, um grupo por classe (`Ide`, `Emit`, `Dest`, `Det`, `Prod`, `Imposto`, `ICMSTot`...), com os **nomes das tags** em PascalCase (`Ide.NatOp`, `Prod.VProd`, `ICMSTot.VNF`) e, no comentário XML, o identificador do campo no MOC (B04, I11...) e a descrição. Quem usa a lib conhece a nota pelos nomes do MOC; traduzir para nomes "amigáveis" criaria um segundo vocabulário. **A aprovar** (alternativa descartada: `NaturezaDaOperacao`, `ValorDoProduto`).

4.2. **Escolhas do leiaute viram tipos.** Os grupos de ICMS por CST/CSOSN são subclasses de um tipo base (o leiaute permite um só); as enumerações levam o valor do XML (`TipoEmissao.ContingenciaOffline` ↔ `9`).

4.3. **Uma versão de leiaute por modelo (4.00).** As Notas Técnicas acrescentam campos opcionais ao mesmo modelo; o campo de uma NT ainda não vigente só é escrito se preenchido. Versão nova de leiaute (5.00, se vier) é outro namespace de classes.

4.4. **Reforma tributária desde o início:** os grupos de IBS, CBS e IS entram no modelo da primeira versão, porque 2026 já é ano de teste e o leiaute os tem.

## 5. Leitura e escrita do XML

5.1. **Escrita e leitura à mão, com `XmlWriter` e `XmlReader`**, um método por grupo. O XML assinado precisa sair exatamente como o leiaute pede: ordem das tags, tag vazia omitida, decimais com as casas do campo, sem prefixo de namespace, sem espaços. O `XmlSerializer` exigiria propriedades-sombra de texto em cada valor formatado e não garante a forma. **A aprovar** (alternativa: `XmlSerializer`, mais rápido de escrever; ou um gerador de código a partir do XSD, mais tarde).

5.2. **A leitura aceita o que a SEFAZ aceita:** `NFe`, `nfeProc` (com o protocolo), versões de NT anteriores, e devolve problemas do que não reconhece, sem perder o XML original. O ErpCore já tem um leitor (`ErpCore.Fiscal`, `Nfe/LeitorDeNfe.cs`), que migra para cá.

5.3. **O XML original é preservado.** Ler e escrever de novo não reproduz os bytes da assinatura; quem precisa guardar a nota guarda o XML recebido ou autorizado, nunca o regerado. A lib devolve sempre os dois: o objeto e o XML.

## 6. Validação

Três camadas, todas antes da transmissão, todas com o mesmo resultado (`Problema`: código, severidade, mensagem, caminho do campo):

6.1. **Estrutural:** contra o XSD oficial do pacote de liberação vigente, embutido (`schemas/<pacote>/`).

6.2. **Regras do MOC e das NTs que dá para conferir sem a SEFAZ:** dígito da chave, CPF e CNPJ (inclusive o alfanumérico), totais contra a soma dos itens, combinações (CST × grupo, CFOP × `idDest`, `indIEDest` × IE), datas, `cClassTrib` contra a tabela. Cada regra leva o **código da rejeição** que ela antecipa (ex.: 564, "Total do Produto / Serviço difere do somatório dos itens") e a mensagem do MOC. O repositório mantém o catálogo das rejeições, com as implementadas e as que só a SEFAZ confere (`docs/REGRAS.md`).

6.3. **Só na SEFAZ** (duplicidade, situação cadastral, numeração): fora da lib; o retorno da autorização traz o `cStat`.

## 7. Chave de acesso, assinatura e certificado

7.1. **Chave:** geração (`cNF` aleatório, nunca igual ao número; DV módulo 11) e leitura das partes (UF, AAMM, CNPJ ou CPF, modelo, série, número, tipo de emissão).

7.2. **Assinatura XMLDSig enveloped**, canonicalização C14N, referência `#NFe<chave>`, RSA com SHA-1 (o que a NF-e ainda exige; SHA-256 quando a NT mudar). **Verificação** também: confere a assinatura e o certificado do emitente num XML recebido (o ErpCore hoje não confere, `PLANO.md` 15, etapa 10).

7.3. **Certificado:** a lib recebe um `X509Certificate2` com a chave privada; carregá-lo é de quem usa (PFX em pasta protegida, repositório do Windows). Utilitário para abrir o PFX e checar validade e CNPJ. **A1 na primeira versão.** O A3 no Windows funciona pelo repositório do sistema (o mesmo `X509Certificate2`); no Linux pede PKCS#11, que fica para depois.

## 8. Transmissão

8.1. **SOAP 1.2 sobre HTTPS com TLS mútuo**, pelo `HttpClient`. Serviços da NF-e: `NFeAutorizacao4` (síncrona, `indSinc=1`, e por lote), `NFeRetAutorizacao4`, `NFeConsultaProtocolo4`, `NFeStatusServico4`, `NFeInutilizacao4`, `NFeRecepcaoEvento4`, `NFeDistribuicaoDFe` e `CadConsultaCadastro4`.

8.2. **Tabela de autorizadores** (UF × modelo × ambiente × serviço → URL e versão), embutida, com as contingências SVC-AN e SVC-RS, e substituível (2.6).

8.3. **TLS no Linux, que é requisito do PDV:**

- a cadeia ICP-Brasil não está nos repositórios das distribuições: a lib embute as ACs raiz e valida o servidor com `X509ChainTrustMode.CustomRootTrust`, sem depender da máquina;
- autorizador que só negocia cifra antiga: `CipherSuitesPolicy` por autorizador (o .NET só a aplica no Linux), declarado na tabela, nunca rebaixando o padrão da máquina inteira.

8.4. **Sem reenvio automático.** A lib não repete a autorização sozinha: quem chama decide. Para a queda no meio (a SEFAZ autorizou e a resposta se perdeu), a lib oferece a recuperação: consultar pela chave e montar o `nfeProc` com o protocolo encontrado; a duplicidade (rejeição 204 e afins) vem identificada no retorno, com a chave. O guia "o que fazer quando cai" faz parte da documentação.

8.5. **Resultado da autorização:** o XML distribuível (`nfeProc`, nota e protocolo), o `cStat` e o motivo, e o XML de envio e de retorno, para quem quiser guardar o rastro.

## 9. NFC-e

9.1. **QR Code** com o CSC (identificador e token, de quem usa), nas versões que a NT vigente aceita, e a URL de consulta por UF (tabela embutida, 2.6).

9.2. **Contingência offline** (`tpEmis=9`): a nota é emitida e assinada sem a SEFAZ, com a justificativa e a data de entrada em contingência, e o QR Code do modo offline; a transmissão posterior é a autorização normal, dentro do prazo da legislação. A lib não guarda a fila: devolve o XML pronto, e o aplicativo transmite quando a rede volta.

9.3. **Cancelamento por substituição** (evento 110112) e o cancelamento comum, como os outros eventos (seção 11).

## 10. DANFE e DANFCE

10.1. **Modelo de impressão neutro:** a lib monta o DANFCE como blocos de texto com alinhamento, destaque e o QR Code, e o DANFE como seções. Um renderizador transforma o modelo em saída. O modelo aceita também **imagem de 1 bit** (o logo do emitente no topo e a assinatura do software emissor no rodapé, como o PDV do ErpCore pede): a térmica só imprime preto, e o renderizador ESC/POS a manda como raster.

10.2. **Renderizador PDF no pacote.** A biblioteca de PDF precisa ter licença compatível com a da OpenDFe (seção 14). **A decidir** (PdfSharp/MigraDoc, MIT; o QuestPDF tem licença comunitária com limite de receita, o que complica uma lib aberta).

10.3. **ESC/POS fica com o aplicativo** (o PDV), a partir do mesmo modelo. Pode virar pacote depois, se houver demanda.

## 11. Eventos

Cancelamento (110111), cancelamento por substituição (110112, NFC-e), carta de correção (110110), manifestação do destinatário (210200, 210210, 210220, 210240), EPEC (110140) e os eventos da reforma tributária conforme as NTs. Mesmo padrão da autorização: monta, valida, assina, transmite e devolve o `procEventoNFe`.

## 12. Manutenção pelas Notas Técnicas

12.1. A lib só está certa enquanto acompanha as NTs, e de 2026 a 2033 elas mudam todo ano. **O trabalho é permanente e faz parte do projeto.**

12.2. `docs/NTS.md` registra cada NT: o que mudou, a vigência em homologação e em produção, a versão da OpenDFe que a implementou. O pacote de schemas acompanha.

12.3. **SemVer.** NT nova sem quebra = versão menor. Mudança que obriga quem usa a alterar código = versão maior, com o guia de migração.

## 13. Testes

13.1. **Sem certificado real na CI pública.** Os testes de assinatura geram um certificado de teste na hora. A verificação usa XMLs autorizados públicos (exemplos dos manuais) e XMLs reais com os dados trocados.

13.2. **Todo XML gerado passa pelo XSD** nos testes, e a leitura de volta reproduz o objeto.

13.3. **Homologação da SEFAZ:** testes de integração opcionais, ligados por variável de ambiente (caminho e senha do PFX, CSC), rodados localmente ou num pipeline com os segredos, nunca em PR de terceiros.

13.4. **CI em Linux e Windows**, porque o TLS e o certificado se comportam diferente nos dois.

## 14. Distribuição e governança

14.1. **GitHub público** e **NuGet.org**, com a reserva do prefixo `OpenDFe.*` antes da primeira publicação.

14.2. **Licença MIT** (a confirmar; a Apache-2.0 acrescenta a concessão de patentes). Permissiva para que produtos fechados, como o ErpCore e o PDV, usem a lib distribuída dentro do executável; LGPL atrapalharia o pacote único e assinado do PDV.

14.3. **Propriedade:** o projeto é pessoal desde o primeiro commit, e o ErpCore o usa sob a licença. Vale deixar isso escrito (README e o `CLAUDE.md` do ErpCore) para não haver dúvida se o ErpCore ganhar sócios ou for vendido.

14.4. **Nome:** "DFe" também aparece em projetos sem relação (o DFE-Digital, do governo britânico); o README deixa claro, já na primeira linha, que é sobre documento fiscal eletrônico brasileiro.

## 15. Consumidores conhecidos

- **ErpCore, módulo fiscal:** o leitor da caixa de entrada (migra para a lib), a verificação da assinatura, o futuro aditivo de NF-e e a distribuição DF-e (as notas destinadas). A lib entra na lista de pacotes permitidos do backend (`PLANO.md` 1.2).
- **PDV (ErpCore.FrenteCaixa):** NFC-e com contingência offline, cancelamento e DANFCE, no host CEF com o CoreCLR, em Windows e Linux.

## 16. Fases

| Fase | Conteúdo | Pronto quando |
|---|---|---|
| 0 | Repositório, licença, README, CI Linux e Windows, estrutura dos pacotes | a CI roda vazia nos dois |
| 1 | Core: documentos (CPF, CNPJ alfanumérico), UF, chave de acesso, certificado, assinatura e verificação | um XML de exemplo assina e verifica |
| 2 | Modelo NF-e/NFC-e 4.00 com IBS/CBS, escrita, leitura, XSD | gerar e ler de volta notas de teste válidas no XSD |
| 3 | Regras de validação, primeiro lote (totais, chave, documentos, combinações principais) e `docs/REGRAS.md` | as rejeições mais comuns barradas antes |
| 4 | Transmissão: status, autorização síncrona e por lote, consulta, `nfeProc`, autorizadores, TLS ICP-Brasil | NF-e autorizada em homologação, no Windows e no Linux |
| 5 | NFC-e: QR Code, CSC, contingência offline | NFC-e online e em contingência autorizadas em homologação |
| 6 | Eventos: cancelamento, substituição, CC-e, inutilização | os quatro em homologação |
| 7 | DANFCE e DANFE: modelo de impressão e PDF | PDFs conferidos contra o manual |
| 8 | Distribuição DF-e e manifestação | notas destinadas baixadas em homologação |
| depois | CT-e, MDF-e, A3 no Linux, NFS-e nacional (a avaliar) | — |

As fases 1 a 5 atendem o PDV; a 8, a caixa de entrada do ErpCore.

## 17. Em aberto

1. Nomes das propriedades: as tags do leiaute (4.1, recomendado) ou nomes descritivos.
2. Escrita do XML à mão (5.1, recomendado) ou `XmlSerializer`.
3. Licença: MIT ou Apache-2.0 (14.2).
4. Biblioteca de PDF (10.2).
5. NFS-e nacional no roteiro ou fora.
