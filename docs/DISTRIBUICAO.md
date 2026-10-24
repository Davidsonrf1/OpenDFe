# Distribuição DF-e: a caixa de entrada de NF-e

Guia para quem usa a `DistribuicaoDFe` (o módulo fiscal do ErpCore e qualquer aplicativo que baixe as notas destinadas a um CNPJ ou CPF). O desenho está em `docs/CONTRATOS.md` 23; as regras são da NT 2014.002 v1.40 (`docs/NTS.md` 10). O código do laço da seção 7 está em `tests/OpenDFe.NFe.Testes/DistribuicaoGuiaTestes.cs` e roda contra a SEFAZ emulada: o que está aqui compila e faz o que o texto diz.

**A lib não repete a consulta, não espera e não guarda o NSU** (`PROJETO.md` 8.6). Ela monta o pedido, transmite ao Ambiente Nacional (AN), descompacta o que volta e diz o que a NT manda fazer em seguida (`TemMais`, `EsperaSugerida`). O laço, a hora de espera e a gravação do NSU e dos documentos são do aplicativo. **Só a NF-e** é distribuída: a NFC-e não existe neste serviço, e `dfe.Suporta<DistribuicaoDFe>()` é falso no núcleo de NFC-e.

## 1. O que cada um recebe

O AN gera um NSU (número sequencial único, sem lacunas) **por interessado**, na ordem em que recebeu cada documento, e entrega ao interessado o que a tabela do item 3 da NT diz:

| Quem é na NF-e | O que recebe |
|---|---|
| **Destinatário** (`dest`) | Na autorização, o **resumo** da NF-e (`resNFe`). O **cancelamento** sempre: como resumo do evento (`resEvento`) antes da manifestação e como `procEventoNFe` depois. A ciência, a confirmação ou a operação não realizada (o desconhecimento, não) **liberam o download**: o `nfeProc` (a nota com o protocolo) e as cartas de correção já registradas chegam em NSU novos, e os eventos seguintes chegam inteiros. **A manifestação é a etapa 8.2** da lib; sem ela, o destinatário só tem o resumo. |
| **Transportador** (`transporta`) e **autorizados** (`autXML`) | Na autorização, o `nfeProc` inteiro; o `procEventoNFe` do cancelamento e da carta de correção. Os autorizados recebem também os eventos de manifestação. Quem também é o destinatário da nota recebe só como destinatário. |
| **Emitente** | Os eventos de manifestação do destinatário (`procEventoNFe`). A própria nota, não: a consulta dela por chave é a 641. |

Cada `DocumentoDistribuido` traz o `Tipo` (`ResumoNFe`, `NFeProcessada`, `ResumoEvento`, `EventoProcessado`, ou `Outro` para o que a lib não conhece: eventos de CT-e e MDF-e, por exemplo), o `Nsu`, a `Chave` e o `Xml` descompactado. Os resumos vêm também tipados (`ResumoNFe`, `ResumoEvento`). **Para a nota inteira ou o evento inteiro, leia o `Xml` com `LeiauteNFe400.Ler`** (a `NotaLida`, `docs/LEITURA.md`): a lib não lê a nota duas vezes.

## 2. O primeiro acesso

- **NSU 0 traz o que o AN recebeu nos últimos 90 dias** (a NT diz "3 meses" no leiaute e 90 dias no texto: vale o prazo de 90 dias da autorização). Documento mais antigo não se recupera por nenhum método.
- **A geração do NSU começa no primeiro acesso** (NT 3.4, "Obs1"): quem nunca usou o serviço, ou parou por mais de 60 dias, recebe o **137** na primeira consulta (com o `UltimoNsu` zerado) e só nas seguintes, **depois de esperar a hora**, passa a receber documentos. Não é falha.
- Guarde sempre o maior `UltimoNsu` visto e **nunca volte** com ele (o laço da seção 7 faz isso): o 137 do primeiro acesso devolve zero.

## 3. O certificado e o interessado

- O certificado do núcleo é o do interessado: o AN confere a **raiz do CNPJ** (8 posições) do certificado com a do CNPJ consultado. **A matriz consulta as filiais** com o mesmo certificado (`DistribuicaoDFe.UltimoNsu("<CNPJ da filial>")`). Outro CNPJ base é a rejeição **593**; o CPF consultado que não é o do certificado, a **472**; certificado sem CNPJ nem CPF, a 473.
- **Cada estabelecimento (as 14 posições) tem a sua sequência de NSU.** A matriz não vê as notas da filial na própria sequência: consulte cada CNPJ que interessa. Várias aplicações do mesmo ator que consultam o mesmo CNPJ de 14 posições **têm de seguir a mesma sequência** (compartilhar o NSU): senão se bloqueiam uma à outra (656, seção 5).
- O `interessado` aceita a máscara e o CNPJ alfanumérico (2026). O `cUFAutor` vai com a UF do núcleo; `ufAutor:` nos três métodos muda.

## 4. As três consultas

| Consulta | Quando usar | Cuidado |
|---|---|---|
| `DistribuicaoDFe.UltimoNsu(interessado, ultimoNsu)` | **A sincronização.** Até 50 documentos por resposta, em ordem de NSU, a partir do NSU seguinte ao informado | Siga a sequência (seção 5) |
| `DistribuicaoDFe.Nsu(interessado, nsu)` | **Fechar uma lacuna**: um NSU que o aplicativo não recebeu. O 137 diz que o AN não gerou esse NSU (descarte-o) | Conta no limite de **20 consultas por hora** |
| `DistribuicaoDFe.Chave(interessado, chave)` | Baixar **uma NF-e** pela chave, sem esperar a sincronização (o destinatário ainda sem ciência recebe o resumo; os outros, o `nfeProc`). Não traz eventos. Vem **sem NSU** no `docZip` e com o `UltimoNsu` zerado | Conta nos mesmos 20 por hora; só o interessado (destinatário, transportador ou autorizado), e a nota até 90 dias, não cancelada (653) |

`TemMais` e `EsperaSugerida` só valem no `UltimoNsu`: a consulta pontual devolve um documento e pronto.

## 5. O uso indevido e a hora de espera

O AN bloqueia por **1 hora** o CNPJ ou CPF que abusa (rejeição **656**, `ConsumoIndevido`). Quem consulta **dentro do bloqueio zera o tempo**: a hora recomeça. Por isso a regra é não consultar, e não "consultar para ver se já liberou".

| Situação | Resposta | O que fazer |
|---|---|---|
| `distNSU` que traz documentos (138) com `UltimoNsu` < `MaximoNsu` | `TemMais` | chamar de novo **na hora**, com o `UltimoNsu` devolvido |
| `distNSU` sem documento novo (137), ou 138 com `UltimoNsu` = `MaximoNsu` | `EsperaSugerida` = 1 hora | **esperar 1 hora** antes de consultar de novo; consultar antes é o 656 |
| `distNSU` com outro `ultNSU` que não o último devolvido (inclusive o 0, recomeçando) | 656 "Deve ser utilizado o ultNSU nas solicitações subsequentes" | esperar 1 hora; o 656 devolve o `UltimoNsu` do servidor |
| 21ª consulta pontual (`Nsu` ou `Chave`) na hora | 656 "ultrapassou o limite de 20 consultas por hora" | esperar 1 hora |
| NSU acima do maior (589) | `Status` 589 | o `UltimoNsu` guardado está errado (outro ambiente, outro CNPJ): não repetir |

A lib devolve a `EsperaSugerida` e **não espera**: o aplicativo guarda "não consultar antes de" (o `NaoConsultarAte` do laço) e o respeita, inclusive depois de reiniciar. Um agendador que dispara a sincronização de hora em hora cumpre a regra sozinho.

## 6. Quando a transmissão falha (a resposta se perde)

`FalhaTransmissaoException.PodeTerSidoProcessado` é verdadeiro quando o pedido saiu e a resposta não chegou (tempo esgotado, conexão derrubada, resposta cortada). Na distribuição isso é perigoso, porque **o AN já considera entregue o lote que se perdeu**. A NT (3.11.4.1) manda sempre consultar a partir do `ultNSU` devolvido na consulta anterior; a repetição com o mesmo `ultNSU` que o aplicativo tinha é, para o AN, uma consulta **fora da sequência**: **656**. Conferido no texto da NT e contra a SEFAZ emulada (`DistribuicaoEmuladaTestes`, `DistribuicaoGuiaTestes`); a confirmação com o AN real fica para a homologação (pendente até o PFX).

**A recomendação, passo a passo:**

1. **Queda antes de o AN processar** (a conexão não chegou, `PodeTerSidoProcessado` falso): repita com o mesmo `UltimoNsu`, sem receio.
2. **Resposta perdida** (`PodeTerSidoProcessado` verdadeiro): repita **uma vez** com o mesmo `UltimoNsu`. Se vier o 138, nada se perdeu (o AN não tinha processado). Se vier o **656**, o AN já tinha entregue o lote: **o `UltimoNsu` do 656 é o do servidor.** Grave o 656 como `NaoConsultarAte` = agora + 1 hora (`EsperaSugerida`) e **não consulte antes**.
3. Entre o `UltimoNsu` que o aplicativo tinha (`a`) e o do 656 (`b`) está a **lacuna**: os NSU `a+1` a `b`, que o AN entregou e o aplicativo não viu. Passada a hora, **busque-os um a um pelo `DistribuicaoDFe.Nsu`** (`RecuperarLacunaAsync` da seção 7; o NSU que o AN não gerou volta 137 e se descarta) e só então avance o NSU até `b`. São no máximo 50 NSU por lote perdido, e **20 consultas pontuais por hora**: uma lacuna de mais de 20 NSU leva mais de uma hora, e o laço retoma de onde parou.
4. Não use o `UltimoNsu` do 656 para continuar a sincronização **sem fechar a lacuna**: os documentos entre `a` e `b` se perderiam. O NSU não tem lacunas no AN; se o aplicativo detectar uma, a causa é uma resposta perdida.

## 7. O laço completo

A caixa de entrada é do aplicativo (banco de dados, arquivo): a interface diz o que o laço precisa dela.

```csharp
/// A caixa de entrada do aplicativo: guarda o NSU e os documentos. Quem usa implementa (banco, arquivo).
public interface ICaixaEntrada
{
    /// O último NSU recebido e gravado; 0 na primeira vez.
    long UltimoNsu { get; }

    /// Até quando não consultar (a hora de espera da NT); nulo se pode consultar.
    DateTimeOffset? NaoConsultarAte { get; set; }

    /// Grava o documento se ainda não está (a chave de idempotência é o NSU; sem NSU, a chave). Devolve se era novo.
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

            // Sem mais lotes (ou uma rejeição, que o chamador examina pelo resultado): para.
            if (!retorno.TemMais)
                return novos;
        }
    }

    /// Busca um a um os NSU da lacuna (a resposta perdida), no limite de 20 consultas por hora.
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
            caixa.AvancarNsu(nsu); // o 137 do NSU que o Ambiente Nacional não gerou também fecha a lacuna
        }
        return novos;
    }
}
```

O uso, com o núcleo do interessado (o certificado dele) e uma caixa de entrada:

```csharp
using var conexoes = new ConexoesSefaz();
var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
{
    Ambiente = Ambiente.Producao,
    UF = UF.MG,
    Certificado = certificadoDoInteressado,
    Conexoes = conexoes,
});

// De hora em hora (o agendador do aplicativo); a espera de 1 hora que a NT manda fica na caixa.
var novos = await SincronizadorDistribuicao.SincronizarAsync(dfe, "11444777000161", caixa, TimeProvider.System, ct);

// Depois de uma resposta perdida e do 656, passada a hora (seção 6): o ultNSU do 656 é o "ateNsu".
// await SincronizadorDistribuicao.RecuperarLacunaAsync(dfe, "11444777000161", caixa, ultNsuDoServidor, ct);
```

E, para cada documento gravado, o que o aplicativo faz com ele:

```csharp
switch (documento.Tipo)
{
    case TipoDocumentoDistribuido.ResumoNFe:
        var resumo = documento.ResumoNFe;              // chave, emitente, valor, situação: para manifestar (8.2)
        break;
    case TipoDocumentoDistribuido.NFeProcessada:
        var nota = new LeiauteNFe400().Ler(documento.Xml);   // a NotaLida: o modelo, o protocolo, o XML
        break;
    case TipoDocumentoDistribuido.ResumoEvento:
        var evento = documento.ResumoEvento;           // tipo, autor, chave, protocolo
        break;
    case TipoDocumentoDistribuido.EventoProcessado:
        // o procEventoNFe inteiro, em documento.Xml: guardar com a nota
        break;
}
```

## 8. Idempotência e avisos

- **O mesmo documento pode chegar de novo** (a repetição depois de uma queda, a recuperação da lacuna, a consulta pontual de um NSU que já se tem). Grave pelo **NSU** (e, na consulta por chave, que não traz NSU, pela **chave** e pelo tipo do documento) e trate a repetição como "já tenho".
- O `docZip` que a lib não conseguiu ler (base64 ou gzip inválido, mais de 16 MB descompactado, XML mal formado) **sai da lista com o aviso `NFE-DISTRIBUICAO`** (`resultado.Problemas`, com o NSU na mensagem): o NSU dele foi consumido. **Registre o aviso**; se o documento importa, busque-o de novo pelo `DistribuicaoDFe.Nsu` (conta nos 20 por hora) ou pela chave, e se a falha persistir, abra o caso com o AN com o `Rastro.XmlRetorno`.
- O resumo que não se lê nos tipos fica com o `Xml` e o `ResumoNFe`/`ResumoEvento` nulo, com o aviso: o documento é seu.
- Aviso nunca vira recusa: a consulta foi processada, e o `UltimoNsu` vale.

## 9. O que a lib não faz

O armazenamento do NSU e dos documentos, a fila, o agendamento, o laço automático e a espera (são do aplicativo, como acima); a **manifestação do destinatário** e a consulta cadastro (etapa 8.2); a distribuição de NFC-e, CT-e e MDF-e (não existem ou são de outras libs). O limite do pedido do AN é de 10 KB, que o pedido da lib nunca alcança.
