namespace OpenDFe.NFSe;

/// <summary>O motivo da rejeição da NFS-e na manifestação (<c>cMotivo</c> dos eventos 202205, 203206 e 204207). O valor de cada membro é o do XML.</summary>
public enum MotivoRejeicao
{
    /// <summary>NFS-e em duplicidade (1).</summary>
    Duplicidade = 1,

    /// <summary>NFS-e já emitida pelo tomador (2).</summary>
    EmitidaTomador = 2,

    /// <summary>Não ocorrência do fato gerador (3).</summary>
    FatoGeradorInexistente = 3,

    /// <summary>Erro quanto à responsabilidade tributária (4).</summary>
    ResponsabilidadeTributaria = 4,

    /// <summary>Erro no valor do serviço, das deduções, no serviço ou na data do fato gerador (5).</summary>
    ErroValores = 5,

    /// <summary>Outros (9).</summary>
    Outros = 9,
}
