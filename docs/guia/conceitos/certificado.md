# Certificado digital

[Documentação](../../index.md) › Conceitos › Certificado digital

A NF-e e a NFC-e são assinadas com o certificado digital do emitente, e o mesmo certificado autentica a conexão com a SEFAZ no TLS mútuo. Este artigo mostra como abrir o certificado, conferir a validade e o titular, e o que a biblioteca espera dele.

## O que a biblioteca espera

- Um `System.Security.Cryptography.X509Certificates.X509Certificate2` **com a chave privada RSA**.
- Carregar o certificado é seu: de um arquivo PFX numa pasta protegida, de um cofre de segredos, do repositório de certificados do Windows.
- A biblioteca não guarda nem copia o certificado: ele é seu, e você o descarta quando não for mais usado.

| Tipo de certificado | Suporte |
|---|---|
| A1 (arquivo PFX/P12) | Windows e Linux |
| A3 (cartão ou token) | Windows, pelo repositório de certificados do sistema (o driver do dispositivo expõe o certificado como um `X509Certificate2`). No Linux, depende de PKCS#11, que a biblioteca ainda não suporta. |

## Abrir um PFX

```csharp
using OpenDFe;

// De um arquivo.
using var certificado = CertificadoDigital.AbrirArquivoPfx(@"C:\certificados\emitente.pfx", senha);

// De bytes (um cofre de segredos, um banco de dados).
byte[] conteudo = await cofre.LerAsync("pfx-emitente");
using var doCofre = CertificadoDigital.AbrirPfx(conteudo, senha);
```

`AbrirPfx` e `AbrirArquivoPfx` lançam `CryptographicException` com a causa na mensagem: o conteúdo não é um PFX, a senha está errada, o PFX não tem a chave privada, a chave não é RSA. Se o PFX tem a cadeia junto, vale o primeiro certificado com a chave privada.

> **Observação**
> O certificado vencido abre normalmente, para poder ser lido. A validade é conferida à parte.

## Usar o repositório do Windows

```csharp
using System.Security.Cryptography.X509Certificates;

using var repositorio = new X509Store(StoreName.My, StoreLocation.CurrentUser);
repositorio.Open(OpenFlags.ReadOnly);
var certificado = repositorio.Certificates
    .Find(X509FindType.FindByThumbprint, impressaoDigital, validOnly: false)
    .Single();
```

O certificado A3 vem do mesmo jeito; o sistema pede o PIN do dispositivo na primeira assinatura.

## Conferir a validade

```csharp
var validade = CertificadoDigital.Validade(certificado, DateTimeOffset.Now);
switch (validade)
{
    case ValidadeCertificado.Vigente:
        break;
    case ValidadeCertificado.Vencido:
        Avisar($"O certificado venceu em {certificado.NotAfter:dd/MM/yyyy}.");
        break;
    case ValidadeCertificado.AindaNaoVigente:
        Avisar($"O certificado só vale a partir de {certificado.NotBefore:dd/MM/yyyy}.");
        break;
}
```

`DFe.Criar` não confere a validade, porque o núcleo pode viver mais que o certificado. Confira no seu aplicativo, por exemplo ao iniciar e uma vez por dia, e avise com antecedência. A SEFAZ rejeita a nota assinada com certificado vencido (291).

## Ler o titular

```csharp
var titular = CertificadoDigital.LerTitular(certificado);
Console.WriteLine(titular?.Nome);   // EMPRESA EXEMPLO LTDA
Console.WriteLine(titular?.Cnpj);   // 11222333000181 (nulo no e-CPF)
Console.WriteLine(titular?.Cpf);    // nulo no e-CNPJ
```

`LerTitular` lê o CNPJ ou o CPF dos campos que a ICP-Brasil usa (o `otherName` do nome alternativo e, nos certificados mais novos, o `serialNumber`). Devolve `null` quando o certificado não tem nenhum dos dois.

> **Dica**
> A SEFAZ compara o titular com o emitente pela **raiz do CNPJ** (8 posições): o certificado de um estabelecimento assina pelas filiais da mesma empresa. No e-CPF, pelo CPF inteiro. Confira isso antes de transmitir para dar uma mensagem melhor que a rejeição 213.

## A assinatura

O núcleo assina sozinho o que o serviço pede (a nota, o evento, a inutilização), com XMLDSig *enveloped*, C14N e RSA-SHA1, como a NF-e exige. Para assinar ou verificar um XML fora das operações, use `AssinaturaDigital`:

```csharp
// Assinar o elemento com Id="NFe3126...".
string assinado = AssinaturaDigital.Assinar(xml, "NFe" + chave, certificado);

// Verificar a assinatura de um XML recebido e o titular contra o emitente.
IReadOnlyList<Problema> problemas = AssinaturaDigital.Verificar(xmlRecebido, "NFe" + chave, cnpjEmitente);
if (problemas.Count > 0)
    Console.WriteLine(string.Join(Environment.NewLine, problemas));
```

`Verificar` confere a assinatura, a referência, o certificado do `KeyInfo` e o titular (a raiz do CNPJ ou o CPF). A cadeia ICP-Brasil, a revogação e a validade na data da emissão ainda não entram na verificação.

## No Linux

A assinatura RSA-SHA1 usa o OpenSSL do sistema. Nas distribuições que bloqueiam o SHA-1 por padrão (a família RHEL 9), libere com `update-crypto-policies --set DEFAULT:SHA1`. Veja [Instalação](../instalacao.md).

## Nos testes

A [SEFAZ emulada](../sefaz-emulada/index.md) aceita um certificado autoassinado com o CNPJ do emitente, gerado na hora. [Conectar a OpenDFe à SEFAZ emulada](../sefaz-emulada/conectar.md) mostra o código.

## Prompt para agentes de IA

Peça a um agente que escreva o carregamento do certificado. Use junto com o [prompt de contexto](../agentes-ia.md), que traz as regras gerais da biblioteca, e revise o código gerado.

```text
Escreva o carregamento do certificado do emitente para a OpenDFe, seguindo docs/guia/conceitos/certificado.md.
Origem do certificado: <arquivo PFX em pasta protegida / cofre de segredos / repositório do Windows por impressão digital>.
Use CertificadoDigital.AbrirArquivoPfx ou AbrirPfx (que lançam CryptographicException com a causa), confira a validade com
CertificadoDigital.Validade e o titular com CertificadoDigital.LerTitular: a raiz do CNPJ (8 posições) tem de ser a do emitente <CNPJ>.
Não registre a senha em log. Descarte o certificado só quando nenhum núcleo o usar mais.
```

## Próximas etapas

- [Conexões, TLS e tempos](conexoes-tls.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
