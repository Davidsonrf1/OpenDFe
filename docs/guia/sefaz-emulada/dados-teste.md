# Preparar dados de teste na SEFAZ emulada

[Documentação](../../index.md) › [SEFAZ emulada](index.md) › Dados de teste

Algumas regras da SEFAZ dependem de dados que ela tem e você não: o CSC cadastrado da NFC-e, a situação cadastral dos contribuintes, o cadastro da consulta cadastro. Na SEFAZ emulada, você cadastra esses dados pela API de controle ou pela aba **Cadastros** do painel. Este artigo mostra cada um e como zerar o estado entre os testes.

Todas as rotas abaixo ficam no painel (`http://127.0.0.1:5444/`) e só são aceitas do próprio computador.

## O CSC da NFC-e

A versão 2 do QR Code da NFC-e usa o CSC (Código de Segurança do Contribuinte) que a SEFAZ da UF deu ao emitente. O emulador confere o identificador e o hash.

Sem cadastrar nada, vale o **CSC padrão** para qualquer emitente:

| Identificador | Código |
|---|---|
| `1` | `OPENDFE-SEFAZ-EMULADA-CSC-PADRAO` |

```csharp
var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Homologacao,
    UF = UF.MG,
    Certificado = certificado,
    Conexoes = conexoes,
    Urls = urls,
    Documento = new ConfiguracaoNFCe(new CodigoSeguranca(1, "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO")),
});
```

Para testar outros identificadores, a revogação ou o CSC errado, cadastre um CSC por UF e raiz do CNPJ:

```http
POST http://127.0.0.1:5444/_controle/csc
Content-Type: application/json

{ "uf": "MG", "cnpj": "11222333", "id": "2", "csc": "MEU-CSC-DE-TESTE-0123456789" }
```

- `cnpj` é a raiz do CNPJ do emitente (8 posições, também a alfanumérica).
- `id` é o identificador sem zeros à esquerda, até 6 algarismos.
- `csc` tem de 16 a 36 caracteres ASCII, sem espaço nem barra vertical.

| Teste | Como provocar | `cStat` |
|---|---|---|
| Identificador não cadastrado | Use um `CodigoSeguranca` com um identificador que não existe para a UF e o CNPJ | 462 |
| CSC revogado | `DELETE /_controle/csc/MG/11222333/2` e emita depois | 463 (a nota emitida antes da revogação ainda passa) |
| Hash que não confere | Use o identificador certo com outro código | 464 |

`GET /_controle/csc` lista os cadastrados e revogados; `DELETE /_controle/csc` volta só ao padrão.

## Contribuintes irregulares

Para testar as rejeições de situação cadastral, marque um CNPJ ou CPF como irregular. Ele vale pelo documento inteiro (o estabelecimento, não a raiz) e em qualquer UF.

```http
POST http://127.0.0.1:5444/_controle/irregulares
Content-Type: application/json

{ "documento": "11222333000181" }
```

| O irregular é... | Operação | `cStat` |
|---|---|---|
| O emitente | Autorização da NF-e ou da NFC-e | 781 |
| O emitente | Evento, inutilização | 240 |
| O destinatário | Autorização da NF-e | 302 |
| O transmissor (o certificado do TLS) | Consulta cadastro | 257 |

`GET /_controle/irregulares` lista; `DELETE /_controle/irregulares/{documento}` tira um; `DELETE /_controle/irregulares` tira todos.

Para marcar só uma nota, sem cadastrar, use a marca `EMULADA:irregular:emitente` ou `EMULADA:irregular:destinatario` no `infCpl` (veja [Simular falhas](falhas-comandadas.md)).

## Contribuintes da consulta cadastro

A consulta cadastro (`CadConsultaCadastro4`) responde com os contribuintes que você cadastra, um por UF e inscrição estadual:

```http
POST http://127.0.0.1:5444/_controle/cadastros
Content-Type: application/json

{
  "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "CONTRIBUINTE DE TESTE LTDA",
  "fantasia": "TESTE", "regimeApuracao": "NORMAL", "cnae": "4751201", "inicioAtividade": "2010-03-01",
  "endereco": { "logradouro": "RUA DE TESTE", "numero": "100", "bairro": "CENTRO",
                "codigoMunicipio": "3106200", "municipio": "BELO HORIZONTE", "cep": "30110000" },
  "habilitado": true, "credenciadoNFe": "1", "credenciadoCTe": "4"
}
```

- Obrigatórios: `uf` (a sigla), `ie` (de 2 a 14 algarismos, não toda zero), `cnpj` ou `cpf` (com o dígito certo) e `nome` (até 60 caracteres).
- Opcionais: os outros. Omitidos, `habilitado` é `true`, `credenciadoNFe` é `1` e `credenciadoCTe` é `4`.
- A resposta é `201` com o contribuinte (`200` quando trocou o da mesma UF e IE) ou `400` com o erro.

Depois, a consulta pela OpenDFe:

```csharp
var r = await nfe.ExecutarAsync(new ConsultaCadastro(UF.MG, "98765432000198"));
// r.Retorno.Status == 111; r.Retorno.Contribuintes[0].Nome == "CONTRIBUINTE DE TESTE LTDA"
```

O mesmo CNPJ com duas inscrições na UF dá o 112; o CNPJ, a IE ou o CPF que não estão no cadastro, o 259, o 261 ou o 264. Veja [Consulta cadastro](../nfe/consulta-cadastro.md).

## Inspecionar o estado

| Rota | O que devolve |
|---|---|
| `GET /_controle/notas` | As notas autorizadas, na ordem: chave, protocolo, recibo, autorizador, situação (100 ou 150) e se está cancelada |
| `GET /_controle/notas/{chave}` | Uma nota, com o `nfeProc` e os eventos |
| `GET /_controle/eventos` | Os eventos registrados |
| `GET /_controle/inutilizacoes` | As faixas inutilizadas |
| `GET /_controle/distribuicao` | O que o Ambiente Nacional distribui a cada interessado, com o NSU |

## Zerar o estado

| Rota | O que esquece |
|---|---|
| `DELETE /_controle/notas` | As notas, os lotes, os eventos, as faixas inutilizadas e a distribuição (com o consumo de cada interessado). A sequência dos recibos e protocolos continua. |
| `DELETE /_controle/falhas` | As falhas comandadas |
| `DELETE /_controle/csc` | Os CSCs cadastrados e revogados (fica o padrão) |
| `DELETE /_controle/irregulares` | Os irregulares |
| `DELETE /_controle/cadastros` | Os contribuintes |
| `DELETE /_controle` | Tudo isso e os pedidos recentes (o botão **Zerar tudo** do painel) |

Com a persistência ligada (`--persistir sim`), o `DELETE` esquece também o que está em disco.

> **Dica**
> Nos testes automatizados, zere o estado no começo de cada teste (ou de cada classe) com `DELETE /_controle`. Assim um teste não herda as notas nem as falhas de outro. Veja [Usar em testes automatizados](testes-automatizados.md).

## Próximas etapas

- [Usar em testes automatizados](testes-automatizados.md)
- [Referência das rotas de controle](api-controle.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
