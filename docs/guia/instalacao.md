# Instalar a OpenDFe

[Documentação](../index.md) › Instalação

Este artigo mostra os requisitos da OpenDFe e as formas de adicioná-la a um projeto .NET.

## Pré-requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download) (a biblioteca tem como alvo `net10.0`).
- Windows ou Linux.
- No Linux, o **OpenSSL 3**, com a política de criptografia permitindo a assinatura **RSA-SHA1**, que a NF-e ainda exige. Veja [Linux e a assinatura RSA-SHA1](#linux-e-a-assinatura-rsa-sha1).
- Para emitir na SEFAZ real, um certificado digital **A1** (PFX) ICP-Brasil do emitente. Para desenvolver e testar, você não precisa dele: a [SEFAZ emulada](sefaz-emulada/index.md) aceita um certificado autoassinado.

## Escolher os pacotes

| Pacote | Quando usar |
|---|---|
| `OpenDFe.NFe` | Sempre que trabalhar com NF-e ou NFC-e. Traz o modelo, o leiaute 4.00 e depende do `OpenDFe.Nucleo`. |
| `OpenDFe.Nucleo` | Vem como dependência do `OpenDFe.NFe`. Referencie direto só se for escrever um leiaute próprio. |
| `OpenDFe.Impressao.Pdf` | Só se for gerar o DANFE ou o DANFCE em PDF. Depende do QuestPDF, que tem [licença própria](impressao/pdf.md). |

Quem imprime só em impressora térmica (ESC/POS) não precisa do pacote de PDF: o modelo de impressão já está no núcleo.

## Instalar pelo NuGet

> **Importante**
> Os pacotes ainda não estão publicados no NuGet.org. Enquanto isso, use os pacotes gerados a partir do código-fonte (abaixo). Quando forem publicados, a instalação será:
>
> ```
> dotnet add package OpenDFe.NFe
> dotnet add package OpenDFe.Impressao.Pdf
> ```

## Gerar os pacotes a partir do código-fonte

1. Clone o repositório e gere os pacotes numa pasta local:

   ```
   git clone https://github.com/Davidsonrf1/OpenDFe.git
   cd OpenDFe
   dotnet pack OpenDFe.slnx -c Release -o C:\pacotes\opendfe
   ```

   A pasta recebe `OpenDFe.Nucleo.0.1.0.nupkg`, `OpenDFe.NFe.0.1.0.nupkg` e `OpenDFe.Impressao.Pdf.0.1.0.nupkg`, com os símbolos (`.snupkg`).

2. No seu projeto, acrescente a pasta como origem de pacotes num `nuget.config`, ao lado do `.sln` ou do `.csproj`:

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="opendfe-local" value="C:\pacotes\opendfe" />
     </packageSources>
   </configuration>
   ```

3. Adicione os pacotes:

   ```
   dotnet add package OpenDFe.NFe --version 0.1.0
   dotnet add package OpenDFe.Impressao.Pdf --version 0.1.0
   ```

4. Confira a instalação com um programa mínimo:

   ```csharp
   using OpenDFe;
   using OpenDFe.NFe;

   Console.WriteLine(new LeiauteNFe400().Versao);        // 4.00
   Console.WriteLine(Cnpj.Valido("11.222.333/0001-81"));  // True
   ```

## Referenciar os projetos diretamente

Se você prefere depurar dentro da biblioteca, referencie os projetos em vez dos pacotes. Referencie também o projeto do modelo: no pacote `OpenDFe.NFe` ele vem junto com o leiaute, mas a referência de projeto não o repassa.

```xml
<ItemGroup>
  <ProjectReference Include="..\OpenDFe\src\OpenDFe.Leiaute.NFe.v400\OpenDFe.Leiaute.NFe.v400.csproj" />
  <ProjectReference Include="..\OpenDFe\src\OpenDFe.NFe.Modelo\OpenDFe.NFe.Modelo.csproj" />
  <ProjectReference Include="..\OpenDFe\src\OpenDFe.Impressao.Pdf\OpenDFe.Impressao.Pdf.csproj" />
</ItemGroup>
```

## Namespaces

| Namespace | O que tem |
|---|---|
| `OpenDFe` | O núcleo (`DFe`, `ConfiguracaoDFe`, `ConexoesSefaz`, `CertificadoDigital`), os resultados e os tipos comuns (`UF`, `Ambiente`, `ChaveAcesso`, `Cnpj`, `Cpf`) |
| `OpenDFe.NFe` | O leiaute (`LeiauteNFe400`), o modelo (`NotaFiscal` e os grupos) e as operações (`Autorizacao`, `EnvioEvento`...) |
| `OpenDFe.Impressao` | O modelo de impressão (`DocumentoImpressao` e os elementos) |
| `OpenDFe.Impressao.Pdf` | O renderizador PDF (`RenderizadorPdf`) |

## Linux e a assinatura RSA-SHA1

A NF-e é assinada com RSA e SHA-1, e a biblioteca usa a criptografia do sistema (no Linux, o OpenSSL 3). Algumas distribuições bloqueiam assinaturas SHA-1 por padrão, e a assinatura falha com `CryptographicException`.

- **Ubuntu e Debian:** a política padrão aceita a assinatura. Nada a fazer.
- **RHEL 9, AlmaLinux 9, Rocky Linux 9, Oracle Linux 9 e derivados:** a política `DEFAULT` bloqueia o SHA-1. Libere com o subperfil `SHA1`:

  ```
  sudo update-crypto-policies --set DEFAULT:SHA1
  ```

  Reinicie o aplicativo depois de mudar a política.

No contêiner, a política vale para a imagem: aplique o comando no `Dockerfile` da imagem baseada em RHEL.

## Próximas etapas

- [Autorizar a primeira NFC-e na SEFAZ emulada](inicio-rapido.md)
- [Arquitetura: núcleo, leiaute e pacotes](conceitos/arquitetura.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
