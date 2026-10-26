# QR Code e CSC da NFC-e

[Documentação](../../index.md) › NFC-e › QR Code e CSC

Toda NFC-e leva, fora da parte assinada, o grupo `infNFeSupl`, com o texto do QR Code e a URL de consulta pela chave. A OpenDFe gera esse grupo sozinha, a partir da chave, do ambiente e da configuração da NFC-e. Este artigo mostra as versões do QR Code, como configurar o CSC e as URLs de consulta por UF.

## As versões

| Versão | Como a biblioteca a escolhe | Parâmetros (online) |
|---|---|---|
| **3** (padrão) | Sem configuração, ou `new ConfiguracaoNFCe()` | `chave\|3\|tpAmb` |
| **2** | `new ConfiguracaoNFCe(new CodigoSeguranca(id, csc))` | `chave\|2\|tpAmb\|idCSC\|hash`, com o hash SHA-1 dos parâmetros e do CSC |

A versão 3 (NT 2025.001) dispensa o CSC e é a única para o emitente com CPF (444) e para a NF-e com o DANFE Simplificado Tipo 2 (672). A versão 2 continua aceita para quem já opera com o CSC. A versão 100 (a primeira) só se lê: a SEFAZ a recusa (903).

O texto do QR Code é a URL de consulta da UF, seguida de `?p=` e dos parâmetros separados por `|`. Na contingência offline, as duas versões levam também o dia da emissão, o valor total e a prova de que a nota é aquela (veja [Contingência offline](contingencia-offline.md)).

## Sem configuração: a versão 3

```csharp
var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
});

var resultado = await nfce.ExecutarAsync(new Autorizacao(nota));
// Notas[0].XmlProcessado traz o infNFeSupl com o qrCode e a urlChave gerados
```

## Com o CSC: a versão 2

O CSC (Código de Segurança do Contribuinte) é o código que a SEFAZ da UF deu à empresa, com um identificador numérico. Você o obtém no portal da SEFAZ da UF, um para homologação e outro para produção.

```csharp
var csc = new CodigoSeguranca(identificador: 1, codigo: segredos["csc-producao"]);

var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Documento = new ConfiguracaoNFCe(csc),
});
```

`CodigoSeguranca` confere os valores no construtor:

| Valor | Regra | Exceção |
|---|---|---|
| `identificador` | De 1 a 999999 (vai no QR Code sem zeros à esquerda) | `ArgumentOutOfRangeException` |
| `codigo` | De 16 a 36 caracteres ASCII visíveis, sem a barra vertical | `ArgumentException` |

> **Importante**
> O CSC é segredo. `CodigoSeguranca.ToString()` mostra só o identificador (`"CSC 1"`), nunca o código: ele não cai num log por descuido. Guarde o código num cofre de segredos, não no código-fonte.

## As URLs de consulta

O QR Code e o `urlChave` usam as URLs de consulta da NFC-e da UF do emitente, em cada ambiente. A biblioteca traz uma tabela delas, e você troca ou completa na `ConfiguracaoNFCe`.

```csharp
foreach (var url in new LeiauteNFe400().UrlsConsulta)
    Console.WriteLine($"{url.UF} {url.Ambiente}: {url.QrCode} | {url.Chave}");
```

> **Importante**
> A tabela traz **só as UFs conferidas em fonte primária** (o portal da SEFAZ da UF): hoje, MG, MS e SC. Nas outras UFs, informe as duas URLs na configuração. Sem elas, a montagem da NFC-e é recusada com `NFE-QRCODE`, antes de transmitir. As URLs vigentes de cada UF estão no Portal da NFC-e da ENCAT e no portal da SEFAZ da UF.

```csharp
var configuracao = new ConfiguracaoNFCe
{
    UrlQrCode = new Uri("https://www.sefaz.uf.gov.br/nfce/qrcode"),     // sem a query: a biblioteca acrescenta o ?p=
    UrlChave = new Uri("https://www.sefaz.uf.gov.br/nfce/consulta"),    // como vai no urlChave
};
// Com o CSC: new ConfiguracaoNFCe(csc) { UrlQrCode = ..., UrlChave = ... }
```

- As URLs valem para a UF e o ambiente do núcleo, que são os da nota.
- A troca vale por campo: você pode trocar só uma das duas numa UF que está na tabela.
- As URLs são conferidas no `init`: relativa, fora de `http` e `https`, ou com query na do QR Code é `ArgumentException`.
- A SEFAZ rejeita a URL que diverge da tabela dela (395 no QR Code, 878 no `urlChave`).

## A NF-e com o DANFE Simplificado Tipo 2

A NF-e com `FormatoImpressao.DanfeSimplificadoTipo2` (`tpImp` 6, o DANFE Simplificado Tipo 2, NT 2026.002) também leva o QR Code, sempre na versão 3, com as URLs da NFC-e da UF. A `ConfiguracaoNFCe` é aceita no núcleo de NF-e só pelas URLs; o CSC nela é recusado (672).

## O grupo preenchido por você

Você não precisa preencher `NotaFiscal.Suplementares`: a biblioteca gera o grupo sempre. Se você o preencher igual ao gerado, passa; diferente, a montagem acusa `NFE-NUCLEO`. Na NF-e sem `tpImp` 6, o grupo preenchido é a rejeição 393.

## O que a biblioteca confere

| Código | Severidade | O que confere |
|---|---|---|
| 444 | Erro | A versão 2 (o CSC configurado) com o emitente CPF |
| 672 | Erro | A NF-e com `tpImp` 6 e o CSC configurado |
| 393 | Erro | A NF-e sem `tpImp` 6 com o `Suplementares` preenchido |
| `NFE-QRCODE` | Erro | A UF sem URL na tabela e sem a troca na configuração |
| `NFE-NUCLEO` | Erro | O `Suplementares` preenchido e diferente do gerado |

O CSC que a SEFAZ tem cadastrado (462, 463, 464) só a SEFAZ confere.

> **Dica**
> A [SEFAZ emulada](../sefaz-emulada/dados-teste.md) tem um CSC padrão para qualquer emitente: o identificador `1` e o código `OPENDFE-SEFAZ-EMULADA-CSC-PADRAO`.

## Próximas etapas

- [Contingência offline](contingencia-offline.md)
- [Modelo de impressão: DANFE e DANFCE](../impressao/modelo-impressao.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
