# OpenDFe

Biblioteca .NET aberta para **documentos fiscais eletrônicos brasileiros** (NF-e e NFC-e da SEFAZ; CT-e e MDF-e depois): geração do XML, validação pelas regras da SEFAZ, assinatura, transmissão, leitura e eventos. Não tem relação com outros projetos chamados "DFe".

**Em desenvolvimento.** Nada publicado ainda. A arquitetura está em [`docs/PROJETO.md`](docs/PROJETO.md) e o andamento em [`PLANO.md`](PLANO.md).

## Pacotes

| Pacote | Conteúdo |
|---|---|
| `OpenDFe.Nucleo` | contratos, certificado, assinatura XMLDSig, SOAP, TLS ICP-Brasil, validação pelo XSD |
| `OpenDFe.NFe` | modelo da NF-e e da NFC-e e o leiaute 4.00 |
| `OpenDFe.Impressao.Pdf` | DANFE e DANFCE em PDF |

## Requisitos

- .NET 10.
- Windows ou Linux. No Linux, OpenSSL 3, com a política de criptografia permitindo a assinatura RSA-SHA1 que a NF-e exige (`docs/PROJETO.md` 2.8).

## Licença e propriedade

[MIT](LICENSE). A OpenDFe é projeto pessoal de Davidson Faria desde o primeiro commit. Produtos que a usam, inclusive os comerciais, o fazem sob a licença MIT, sem transferência de propriedade.
