namespace OpenDFe.NFSe;

/// <summary>A categoria do serviço de locação (<c>categ</c>, só na versão 1.00). O valor de cada membro é o do XML.</summary>
public enum CategoriaLocacao
{
    /// <summary>Locação (1).</summary>
    Locacao = 1,

    /// <summary>Sublocação (2).</summary>
    Sublocacao = 2,

    /// <summary>Arrendamento (3).</summary>
    Arrendamento = 3,

    /// <summary>Direito de passagem (4).</summary>
    DireitoPassagem = 4,

    /// <summary>Permissão de uso (5).</summary>
    PermissaoUso = 5,
}
