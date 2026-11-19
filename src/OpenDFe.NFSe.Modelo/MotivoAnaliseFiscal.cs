namespace OpenDFe.NFSe;

/// <summary>O motivo da solicitação de análise fiscal para cancelamento (<c>cMotivo</c> do evento 101103). O valor de cada membro é o do XML.</summary>
public enum MotivoAnaliseFiscal
{
    /// <summary>Erro na emissão (1).</summary>
    ErroEmissao = 1,

    /// <summary>Serviço não prestado (2).</summary>
    ServicoNaoPrestado = 2,

    /// <summary>Outros (9).</summary>
    Outros = 9,
}
