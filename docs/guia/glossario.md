# Glossário

[Documentação](../index.md) › Glossário

| Termo | Significado |
|---|---|
| **AN** | Ambiente Nacional. Recebe a manifestação do destinatário e faz a distribuição DF-e. |
| **Autorizador** | A SEFAZ (de uma UF ou virtual) que autoriza as notas de uma UF num tipo de emissão. |
| **Chave de acesso** | Os 44 dígitos que identificam a nota: UF, ano e mês, CNPJ ou CPF do emitente, modelo, série, número, tipo de emissão, código numérico e dígito verificador. Na biblioteca, `ChaveAcesso`. |
| **CC-e** | Carta de correção eletrônica (evento 110110). |
| **cNF** | O código numérico da chave de acesso. Sem ele na nota, a biblioteca sorteia um a cada escrita. |
| **Contingência** | A emissão fora do modo normal quando o autorizador está indisponível: a SVC na NF-e, a offline na NFC-e. |
| **cStat** | O código de situação que a SEFAZ devolve (107 em operação, 100 autorizada, 539 duplicidade...). |
| **CSC** | Código de Segurança do Contribuinte: o segredo que a SEFAZ dá ao emitente de NFC-e para o QR Code versão 2. Na biblioteca, `CodigoSeguranca`. |
| **DANFE** | Documento Auxiliar da NF-e: a representação impressa da NF-e. |
| **DANFCE** | Documento Auxiliar da NFC-e. |
| **digVal** | O resumo (`DigestValue`) da assinatura da nota, que o protocolo repete. Prova que o protocolo é daquele XML. |
| **Distribuição DF-e** | O serviço do Ambiente Nacional que entrega a um CNPJ ou CPF os documentos em que ele aparece. |
| **Evento** | Um fato registrado sobre uma nota autorizada: cancelamento, carta de correção, manifestação. |
| **Homologação** | O ambiente de testes da SEFAZ, sem valor fiscal (`tpAmb` 2). |
| **Inutilização** | O registro de uma faixa de números que não foram nem serão usados. |
| **Leiaute** | A estrutura do XML de um documento numa versão (a 4.00 da NF-e). Na biblioteca, a implementação dessa versão (`LeiauteNFe400`). |
| **Lote** | O conjunto de 2 a 50 notas enviado de uma vez, processado de forma assíncrona e consultado pelo recibo. |
| **Manifestação do destinatário** | Os eventos com que o destinatário diz o que sabe da nota: ciência, confirmação, desconhecimento, operação não realizada. |
| **MOC** | Manual de Orientação do Contribuinte da NF-e: a especificação do leiaute, dos serviços e das regras. |
| **nfeProc** | O XML da nota assinada junto com o protocolo de autorização: o XML fiscal que se guarda e se distribui. Na biblioteca, `ResultadoNota.XmlProcessado`. |
| **NSU** | Número Sequencial Único: a posição de cada documento na distribuição DF-e de um interessado. |
| **NT** | Nota Técnica: o documento com que a SEFAZ muda o leiaute e as regras. |
| **Núcleo** | Na biblioteca, o `DFe`: o objeto que executa as operações para um leiaute, um documento e uma configuração. |
| **Pacote de liberação** | O conjunto de schemas XSD publicado com cada NT. |
| **Produção** | O ambiente real da SEFAZ, com valor fiscal (`tpAmb` 1). |
| **Protocolo** | O registro da SEFAZ de que processou a nota (`protNFe`), com o número (`nProt`), o `cStat` e o momento. |
| **QR Code** | O código impresso na NFC-e (e no DANFE Simplificado Tipo 2) para o consumidor consultar a nota. |
| **Recibo** | O número que a SEFAZ devolve ao receber um lote, para a consulta do resultado. |
| **SEFAZ emulada** | O servidor local da OpenDFe que responde como os autorizadores em homologação, para desenvolvimento e testes. |
| **SVAN, SVRS** | As SEFAZ virtuais (do Ambiente Nacional e do Rio Grande do Sul) que autorizam as notas das UFs sem autorizador próprio. |
| **SVC-AN, SVC-RS** | As SEFAZ virtuais de contingência, que autorizam a NF-e quando o autorizador da UF está fora. |
| **Tipo de emissão (`tpEmis`)** | Como a nota foi emitida: normal (1), SVC-AN (6), SVC-RS (7), offline (9) e outras contingências. |
| **TLS mútuo** | A conexão HTTPS em que também o cliente apresenta certificado: a SEFAZ autentica o transmissor pelo certificado. |
| **urlChave** | A URL de consulta da NFC-e pela chave de acesso, impressa no DANFCE. |

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
