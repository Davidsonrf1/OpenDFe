namespace OpenDFe.NFSe;

/// <summary>O motivo do cancelamento da NFS-e (<c>cMotivo</c> do evento 101101). O valor de cada membro é o do XML.</summary>
public enum MotivoCancelamento
{
    /// <summary>Erro na emissão (1).</summary>
    ErroEmissao = 1,

    /// <summary>Serviço não prestado (2).</summary>
    ServicoNaoPrestado = 2,

    /// <summary>Outros (9).</summary>
    Outros = 9,
}
