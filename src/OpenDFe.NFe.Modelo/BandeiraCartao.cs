namespace OpenDFe.NFe;

/// <summary>A bandeira da operadora do cartão. O valor de cada membro é o do XML (<c>tBand</c>).</summary>
/// <remarks>
/// A lista é a do MOC 7.0 (Anexo I). Desde a NT 2020.006 vale a tabela de códigos das operadoras publicada no Portal
/// Nacional, que pode ter mais bandeiras: a leitura de um código que não está aqui devolve o número, e a escrita
/// escreve o número que estiver.
/// </remarks>
public enum BandeiraCartao
{
    /// <summary>Visa (1).</summary>
    Visa = 1,

    /// <summary>Mastercard (2).</summary>
    Mastercard = 2,

    /// <summary>American Express (3).</summary>
    AmericanExpress = 3,

    /// <summary>Sorocred (4).</summary>
    Sorocred = 4,

    /// <summary>Diners Club (5).</summary>
    DinersClub = 5,

    /// <summary>Elo (6).</summary>
    Elo = 6,

    /// <summary>Hipercard (7).</summary>
    Hipercard = 7,

    /// <summary>Aura (8).</summary>
    Aura = 8,

    /// <summary>Cabal (9).</summary>
    Cabal = 9,

    /// <summary>Outros (99).</summary>
    Outros = 99,
}
