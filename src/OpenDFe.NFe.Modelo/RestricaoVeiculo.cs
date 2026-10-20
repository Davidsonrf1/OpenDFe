namespace OpenDFe.NFe;

/// <summary>A restrição do veículo. O valor de cada membro é o do XML (<c>tpRest</c>).</summary>
public enum RestricaoVeiculo
{
    /// <summary>Não há (0).</summary>
    Nenhuma = 0,

    /// <summary>Alienação fiduciária (1).</summary>
    AlienacaoFiduciaria = 1,

    /// <summary>Arrendamento mercantil (2).</summary>
    ArrendamentoMercantil = 2,

    /// <summary>Reserva de domínio (3).</summary>
    ReservaDominio = 3,

    /// <summary>Penhor de veículos (4).</summary>
    PenhorVeiculos = 4,

    /// <summary>Outras (9).</summary>
    Outras = 9,
}
