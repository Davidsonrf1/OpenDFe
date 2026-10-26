# Passar para a homologação real e para a produção

[Documentação](../index.md) › Da homologação à produção

Depois de desenvolver e testar contra a [SEFAZ emulada](sefaz-emulada/index.md), o caminho é a homologação real da SEFAZ e, depois, a produção. Este artigo lista o que muda em cada passo e o que conferir.

## Da SEFAZ emulada à homologação real

| Item | Na SEFAZ emulada | Na homologação real |
|---|---|---|
| Certificado | Autoassinado, com o CNPJ do emitente | O A1 ICP-Brasil do emitente (`CertificadoDigital.AbrirArquivoPfx`) |
| Conexões | Com a fábrica que confia na AC de teste | `new ConexoesSefaz()`, sem fábrica |
| URLs | As do emulador, em `ConfiguracaoDFe.Urls` | Nenhuma: as do leiaute |
| CSC (NFC-e, versão 2) | O padrão do emulador | O CSC de homologação, do portal da SEFAZ da UF |
| URLs de consulta da NFC-e | Qualquer (o emulador não confere) | As da UF: a tabela da biblioteca ou a `ConfiguracaoNFCe` |
| Ambiente | `Homologacao` | `Homologacao` |

O resto do código é o mesmo.

### O que conferir na homologação real

- [ ] **O TLS mútuo** com o certificado ICP-Brasil, no Windows e no Linux. A cadeia do servidor de cada autorizador que você usa.
- [ ] **O status** (107) em cada autorizador: o da UF e o da SVC.
- [ ] **A autorização** de uma NF-e e de uma NFC-e (versão 3 e, se usar, versão 2).
- [ ] **As URLs de consulta da NFC-e** da sua UF: a SEFAZ rejeita a divergente (395 no QR Code, 878 no `urlChave`).
- [ ] **Os eventos**: o cancelamento, a carta de correção e, na NFC-e, a substituição.
- [ ] **A contingência**: a NFC-e offline transmitida depois, e a SVC na NF-e.
- [ ] **A distribuição DF-e** e a manifestação com o CNPJ do certificado.
- [ ] **As regras que só a SEFAZ confere**: a duplicidade, a numeração, a situação cadastral, as particularidades da UF.

Em homologação, as notas não têm valor fiscal. A SEFAZ exige que o nome do destinatário da NF-e de homologação seja o texto fixo do MOC; confira a NT vigente.

> **Observação**
> A homologação real da própria OpenDFe (os testes de `tests/OpenDFe.Homologacao.Testes`) ainda depende de um certificado A1 ICP-Brasil. As operações foram conferidas contra a SEFAZ emulada. Os testes de homologação estão escritos e são pulados sem o certificado; com ele, rodam pelas variáveis `OPENDFE_PFX_CAMINHO`, `OPENDFE_PFX_SENHA`, `OPENDFE_EMITENTE_IE` e, na NFC-e versão 2, `OPENDFE_CSC_ID` e `OPENDFE_CSC`.

## Da homologação à produção

| Item | O que muda |
|---|---|
| `ConfiguracaoDFe.Ambiente` | `Ambiente.Producao` |
| CSC (versão 2) | O CSC de produção |
| Numeração | A série e a numeração de produção, que não se misturam com as de homologação |

### Antes de emitir em produção

- [ ] **Guarde o XML assinado antes de transmitir** (`Preparar`) e o `nfeProc` depois. Veja [Autorizar notas](nfe/autorizacao.md).
- [ ] **Trate a queda no meio** com a recuperação, nunca com o reenvio às cegas. Veja [Quando a conexão cai](nfe/queda-recuperacao.md).
- [ ] **Confira a validade do certificado** todo dia e avise antes de vencer.
- [ ] **Defina a licença do QuestPDF**, se gerar PDF. Veja [Gerar o PDF](impressao/pdf.md).
- [ ] **Não registre em log** os XMLs do rastro nem os valores dos problemas de schema: têm dados pessoais.
- [ ] **Separe os segredos**: o PFX, a senha e o CSC num cofre, fora do código e da configuração em texto.
- [ ] **Configure os tempos** do `ConexoesSefaz` com folga: um `TempoResposta` curto transforma uma autorização lenta numa queda no meio.

## Acompanhar as Notas Técnicas

A SEFAZ muda o leiaute e as regras por Notas Técnicas, e de 2026 a 2033 elas mudam todo ano por causa da reforma tributária. Atualize a biblioteca quando sair uma versão para a NT que entrar em vigor. As NTs que cada versão implementa estão em [NTS.md](../NTS.md). Uma URL de autorizador que muda antes de sair a versão nova se troca na configuração (veja [Autorizadores e URLs](conceitos/autorizadores-urls.md)).

## Próximas etapas

- [Solução de problemas](solucao-problemas.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
