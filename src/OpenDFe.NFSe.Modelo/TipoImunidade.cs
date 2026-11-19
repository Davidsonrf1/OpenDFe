namespace OpenDFe.NFSe;

/// <summary>O tipo da imunidade do ISSQN (<c>tpImunidade</c>). O valor de cada membro é o do XML.</summary>
public enum TipoImunidade
{
    /// <summary>Imunidade, tipo não informado na nota de origem (0).</summary>
    NaoInformada = 0,

    /// <summary>Patrimônio, renda ou serviços, uns dos outros (CF, art. 150, VI, a) (1).</summary>
    Reciproca = 1,

    /// <summary>Templos de qualquer culto (CF, art. 150, VI, b) (2).</summary>
    Templos = 2,

    /// <summary>Partidos, sindicatos de trabalhadores, educação e assistência social sem fins lucrativos (CF, art. 150, VI, c) (3).</summary>
    PartidosEntidades = 3,

    /// <summary>Livros, jornais, periódicos e o papel da impressão (CF, art. 150, VI, d) (4).</summary>
    LivrosJornais = 4,

    /// <summary>Fonogramas e videofonogramas musicais brasileiros (CF, art. 150, VI, e) (5).</summary>
    Fonogramas = 5,
}
