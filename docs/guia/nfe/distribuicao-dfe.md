# Baixar as notas destinadas: distribuição DF-e

[Documentação](../../index.md) › NF-e e NFC-e › Distribuição DF-e

A distribuição DF-e (`NFeDistribuicaoDFe`, no Ambiente Nacional) entrega a um CNPJ ou CPF os documentos em que ele aparece: as NF-e destinadas a ele, as de que ele é transportador ou autorizado, e os eventos dessas notas. É a caixa de entrada de NF-e de uma empresa. Só a NF-e é distribuída: num núcleo de NFC-e, `dfe.Suporta<DistribuicaoDFe>()` é falso.

> **Importante**
> A biblioteca **não repete a consulta, não espera e não guarda o NSU**. Ela monta o pedido, transmite, descompacta o que volta e diz o que fazer em seguida (`TemMais`, `EsperaSugerida`). O laço, a hora de espera e a gravação do NSU e dos documentos são do seu aplicativo. As regras de uso são da NT 2014.002, e consultar fora delas bloqueia o CNPJ por uma hora.

## O que cada um recebe

O Ambiente Nacional gera um NSU (número sequencial único, sem lacunas) **por interessado**, na ordem em que recebeu cada documento:

| Quem é na NF-e | O que recebe |
|---|---|
| **Destinatário** | Na autorização, o **resumo** da NF-e (`resNFe`). O cancelamento, como resumo do evento antes da manifestação e inteiro depois. A ciência, a confirmação ou a operação não realizada [liberam o download](manifestacao.md): o `nfeProc` e as cartas de correção chegam em NSU novos |
| **Transportador** e **autorizados** (`autXML`) | Na autorização, o `nfeProc` inteiro; os eventos de cancelamento e de carta de correção |
| **Emitente** | Os eventos de manifestação do destinatário. A própria nota, não |

## O certificado e o interessado

- O certificado do núcleo é o do interessado: o Ambiente Nacional confere a **raiz do CNPJ** do certificado com a do CNPJ consultado. A matriz consulta as filiais com o mesmo certificado. Outro CNPJ base é a rejeição 593; o CPF que não é o do certificado, a 472.
- **Cada estabelecimento (as 14 posições do CNPJ) tem a sua sequência de NSU.** Consulte cada CNPJ que interessa.
- Vários aplicativos que consultam o mesmo CNPJ têm de compartilhar o NSU: senão um bloqueia o outro (656).

## As três consultas

| Consulta | Quando usar |
|---|---|
| `DistribuicaoDFe.UltimoNsu(interessado, ultimoNsu)` | **A sincronização.** Até 50 documentos por resposta, a partir do NSU seguinte ao informado |
| `DistribuicaoDFe.Nsu(interessado, nsu)` | **Fechar uma lacuna**: buscar um NSU que o aplicativo não recebeu. Conta no limite de 20 consultas por hora |
| `DistribuicaoDFe.Chave(interessado, chave)` | Baixar **uma NF-e** pela chave, sem esperar a sincronização. Não traz eventos. Conta no mesmo limite de 20 por hora |

```csharp
var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu("11444777000161", ultimoNsu));
var retorno = resultado.Retorno!;

Console.WriteLine($"{retorno.Status} {retorno.Motivo}");     // 138 Documento(s) localizado(s)
foreach (var documento in retorno.Documentos)
    Console.WriteLine($"{documento.Nsu} {documento.Tipo} {documento.Chave}");
// retorno.UltimoNsu: o NSU para a próxima consulta; retorno.MaximoNsu: o maior do interessado
```

## O retorno

| Membro de `RetornoDistribuicao` | Descrição |
|---|---|
| `Status`, `Motivo` | 138 (documentos localizados), 137 (nenhum documento), 656 (uso indevido), 589 (NSU acima do maior), ou a rejeição |
| `Documentos` | Os documentos, já descompactados |
| `UltimoNsu`, `MaximoNsu` | O último NSU devolvido e o maior do interessado |
| `TemMais` | Há mais documentos: chame de novo **na hora**, com o `UltimoNsu` devolvido |
| `EsperaSugerida` | Quanto esperar antes da próxima consulta (1 hora no 137, no 138 que alcançou o `MaximoNsu` e no 656); nula quando pode seguir |
| `NenhumDocumento`, `ConsumoIndevido` | Atalhos para o 137 e o 656 |

| Membro de `DocumentoDistribuido` | Descrição |
|---|---|
| `Tipo` | `ResumoNFe`, `NFeProcessada`, `ResumoEvento`, `EventoProcessado` ou `Outro` (eventos de CT-e e MDF-e, por exemplo) |
| `Nsu` | O NSU (nulo na consulta por chave) |
| `Chave` | A chave da nota |
| `Xml` | O documento descompactado |
| `ResumoNFe`, `ResumoEvento` | Os resumos, tipados |
| `Schema` | O schema informado pelo Ambiente Nacional (`resNFe_v1.01.xsd`...) |

Para a nota inteira, leia o `Xml` com `LeiauteNFe400.Ler` (veja [Ler um XML recebido](leitura-xml.md)).

## O uso indevido e a hora de espera

O Ambiente Nacional bloqueia por **1 hora** o CNPJ ou CPF que abusa (656). Quem consulta **dentro do bloqueio** recomeça a hora. Por isso a regra é não consultar, e não "consultar para ver se já liberou".

| Situação | Resposta | O que fazer |
|---|---|---|
| 138 com `UltimoNsu` menor que `MaximoNsu` | `TemMais` | Chamar de novo na hora, com o `UltimoNsu` devolvido |
| 137, ou 138 com `UltimoNsu` igual ao `MaximoNsu` | `EsperaSugerida` de 1 hora | Esperar 1 hora |
| Consulta com outro `ultNSU` que não o último devolvido (inclusive o 0, recomeçando) | 656 | Esperar 1 hora; o 656 devolve o `UltimoNsu` do servidor |
| 21ª consulta pontual na hora | 656 | Esperar 1 hora |

O primeiro acesso traz os documentos dos **últimos 90 dias**. Quem nunca usou o serviço (ou parou por mais de 60 dias) pode receber o 137 na primeira consulta, e só passa a receber documentos depois de esperar a hora. Não é falha.

## O laço de sincronização

A caixa de entrada é do aplicativo (banco de dados, arquivo). A interface abaixo diz o que o laço precisa dela:

```csharp
/// A caixa de entrada do aplicativo: guarda o NSU e os documentos.
public interface ICaixaEntrada
{
    /// O último NSU recebido e gravado; 0 na primeira vez.
    long UltimoNsu { get; }

    /// Até quando não consultar (a hora de espera); nulo se pode consultar.
    DateTimeOffset? NaoConsultarAte { get; set; }

    /// Grava o documento se ainda não está (pelo NSU; sem NSU, pela chave). Devolve se era novo.
    bool Gravar(DocumentoDistribuido documento);

    /// Avança o NSU, depois de gravar os documentos até ele. Nunca volta.
    void AvancarNsu(long ultimoNsu);
}

public static class SincronizadorDistribuicao
{
    /// Busca tudo o que há de novo e respeita a hora de espera. Devolve quantos documentos novos gravou.
    public static async Task<int> SincronizarAsync(DFe dfe, string interessado, ICaixaEntrada caixa, TimeProvider relogio, CancellationToken ct)
    {
        if (caixa.NaoConsultarAte is { } ate && relogio.GetUtcNow() < ate)
            return 0;

        var novos = 0;
        while (true)
        {
            var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(interessado, caixa.UltimoNsu), ct);
            if (!resultado.Processado)
                throw new InvalidOperationException("A consulta foi recusada antes de transmitir: " + string.Join("; ", resultado.Problemas));

            var retorno = resultado.Retorno;
            foreach (var documento in retorno.Documentos)
            {
                if (caixa.Gravar(documento))
                    novos++;
            }

            // Só avança no 137 e no 138, e nunca para trás (o 137 do primeiro acesso traz o ultNSU zerado).
            if (retorno.Status is 137 or 138 && retorno.UltimoNsu is { } ultimo && ultimo > caixa.UltimoNsu)
                caixa.AvancarNsu(ultimo);

            // 137, 138 que alcançou o maxNSU e 656: esperar 1 hora, sem consultar de novo antes.
            if (retorno.EsperaSugerida is { } espera)
            {
                caixa.NaoConsultarAte = relogio.GetUtcNow() + espera;
                return novos;
            }

            if (!retorno.TemMais)
                return novos;
        }
    }

    /// Busca um a um os NSU da lacuna (depois de uma resposta perdida), no limite de 20 consultas por hora.
    public static async Task<int> RecuperarLacunaAsync(DFe dfe, string interessado, ICaixaEntrada caixa, long ateNsu, CancellationToken ct)
    {
        var novos = 0;
        for (var nsu = caixa.UltimoNsu + 1; nsu <= ateNsu; nsu++)
        {
            var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.Nsu(interessado, nsu), ct);
            if (!resultado.Processado || resultado.Retorno.ConsumoIndevido)
                break; // o limite de 20 por hora: recomeça da lacuna que ficou, depois da espera

            foreach (var documento in resultado.Retorno.Documentos)
            {
                if (caixa.Gravar(documento))
                    novos++;
            }
            caixa.AvancarNsu(nsu); // o 137 do NSU que não foi gerado também fecha a lacuna
        }
        return novos;
    }
}
```

O uso, com o núcleo do interessado e um agendador que roda de hora em hora:

```csharp
using var conexoes = new ConexoesSefaz();
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificadoDoInteressado,
    Conexoes = conexoes,
});

var novos = await SincronizadorDistribuicao.SincronizarAsync(dfe, "11444777000161", caixa, TimeProvider.System, ct);
```

E o que fazer com cada documento gravado:

```csharp
switch (documento.Tipo)
{
    case TipoDocumentoDistribuido.ResumoNFe:
        var resumo = documento.ResumoNFe;                 // chave, emitente, valor, situação: manifeste a ciência
        break;
    case TipoDocumentoDistribuido.NFeProcessada:
        var nota = new LeiauteNFe400().Ler(documento.Xml);   // a nota inteira, com o protocolo
        break;
    case TipoDocumentoDistribuido.ResumoEvento:
        var evento = documento.ResumoEvento;              // tipo, autor, chave, protocolo
        break;
    case TipoDocumentoDistribuido.EventoProcessado:
        // o procEventoNFe inteiro, em documento.Xml: guarde com a nota
        break;
}
```

## Quando a resposta se perde

Na distribuição, a queda no meio é perigosa: **o Ambiente Nacional já considera entregue o lote cuja resposta se perdeu.** Repetir com o mesmo `UltimoNsu` é, para ele, uma consulta fora da sequência: 656.

1. **Queda antes de processar** (`PodeTerSidoProcessado` falso): repita com o mesmo `UltimoNsu`, sem receio.
2. **Resposta perdida** (`PodeTerSidoProcessado` verdadeiro): repita **uma vez** com o mesmo `UltimoNsu`. Se vier o 138, nada se perdeu. Se vier o 656, o lote já tinha sido entregue: **o `UltimoNsu` do 656 é o do servidor.** Grave a espera de 1 hora e não consulte antes.
3. Entre o NSU que o aplicativo tinha (`a`) e o do 656 (`b`) está a **lacuna**. Passada a hora, busque os NSU `a+1` a `b` um a um com `DistribuicaoDFe.Nsu` (o `RecuperarLacunaAsync` acima) e só então avance o NSU até `b`. São no máximo 50 NSU por lote perdido, e 20 consultas pontuais por hora.
4. Não avance para o `UltimoNsu` do 656 **sem fechar a lacuna**: os documentos entre `a` e `b` se perderiam.

## Idempotência e avisos

- O mesmo documento pode chegar de novo (a repetição depois de uma queda, a recuperação da lacuna). Grave pelo NSU (e, na consulta por chave, que não traz NSU, pela chave e pelo tipo) e trate a repetição como "já tenho".
- O documento que a biblioteca não conseguiu ler (base64 ou gzip inválido, mais de 16 MB descompactado, XML mal formado) sai da lista com o aviso `NFE-DISTRIBUICAO`, com o NSU na mensagem. Registre o aviso e, se o documento importa, busque-o de novo pelo NSU ou pela chave.

> **Dica**
> Teste o laço inteiro na [SEFAZ emulada](../sefaz-emulada/index.md): ela gera o NSU por interessado, distribui o resumo e o `nfeProc` conforme a manifestação e aplica o uso indevido (656) com o bloqueio de uma hora. Use um núcleo com o certificado do emitente para autorizar as notas e outro com o do destinatário para baixá-las.

## Próximas etapas

- [Manifestação do destinatário](manifestacao.md)
- [Ler um XML recebido](leitura-xml.md)

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.
