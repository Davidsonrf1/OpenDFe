namespace OpenDFe.NFe;

/// <summary>O formato de impressão do DANFE. O valor de cada membro é o do XML (<c>tpImp</c>).</summary>
public enum FormatoImpressao
{
    /// <summary>Sem geração de DANFE (0).</summary>
    Nenhum = 0,

    /// <summary>DANFE normal, retrato (1).</summary>
    DanfeRetrato = 1,

    /// <summary>DANFE normal, paisagem (2).</summary>
    DanfePaisagem = 2,

    /// <summary>DANFE simplificado (3).</summary>
    DanfeSimplificado = 3,

    /// <summary>DANFE NFC-e (4).</summary>
    DanfeNFCe = 4,

    /// <summary>DANFE NFC-e em mensagem eletrônica (5).</summary>
    DanfeNFCeMensagem = 5,

    /// <summary>DANFE simplificado, tipo 2 (6).</summary>
    DanfeSimplificadoTipo2 = 6,
}
