using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// O caminho do XML até a propriedade do modelo (MODELO.md 4, PROJETO.md 6.4), pelo mapa (<see cref="MapasNFSe"/>):
/// <c>DPS/infDPS/serv/cServ/xDescServ</c> vira <c>Servico.Codigo.Descricao</c>, a partir da raiz de cada documento (a
/// <see cref="Dps"/>, a <see cref="NotaFiscalServico"/>, o <see cref="EventoNFSe"/>, o <see cref="EventoNFSeRegistrado"/>).
/// </summary>
/// <remarks>
/// Os passos que o modelo achata (<c>infDPS</c>, <c>infNFSe</c>, <c>infPedReg</c>, <c>infEvento</c>, o detalhe do evento)
/// não geram propriedade. O índice do XML começa em 1; o do modelo, em 0; a lista sem índice é o primeiro elemento. O
/// atributo, a <c>Signature</c> e o passo que o mapa não conhece dão nulo.
/// </remarks>
internal static class CaminhoModeloNFSe
{
    /// <summary>A propriedade do caminho; nulo quando ele não chega a uma propriedade.</summary>
    internal static string? Propriedade(string caminho)
    {
        ArgumentNullException.ThrowIfNull(caminho);
        var passos = caminho.Split('/');
        if (passos.Length < 2)
            return null;
        MapaGrupo mapa;
        var raiz = Nome(passos[0]);
        var dentro = Nome(passos[1]);
        switch (raiz, dentro)
        {
            case ("DPS", "infDPS"):
                mapa = MapasNFSe.MapaDps;
                break;
            case ("NFSe", "infNFSe"):
                mapa = MapasNFSe.MapaNotaFiscalServico;
                break;
            case ("pedRegEvento", "infPedReg"):
                mapa = MapasNFSe.MapaEventoNFSe;
                break;
            case ("evento", "infEvento"):
                mapa = MapasNFSe.MapaEventoNFSeRegistrado;
                break;
            default:
                return null;
        }

        var partes = new List<string>();
        for (var i = 2; i < passos.Length; i++)
        {
            if (passos[i].StartsWith('@'))
                return null;
            if (!Ler(passos[i], out var nome, out var indice))
                return null;
            if (mapa == MapasNFSe.MapaEventoNFSe && MapasNFSe.Eventos.FirstOrDefault(t => t.Tag == nome) is { } tipo)
            {
                mapa = tipo.Mapa();   // o detalhe do evento: os campos são da subclasse, sem passo
                continue;
            }
            if (mapa.Campo(nome) is not { } campo || campo.Propriedade.Length == 0)
                return null;
            partes.Add(campo.Repete ? string.Create(CultureInfo.InvariantCulture, $"{campo.Propriedade}[{(indice ?? 1) - 1}]") : campo.Propriedade);
            // a DPS dentro da NFS-e e o pedido dentro do evento: o infDPS e o infPedReg não têm propriedade
            var sub = campo is CampoPedido<EventoNFSeRegistrado> ? MapasNFSe.MapaEventoNFSe : campo.Subgrupo;
            if (sub is null)
                return i == passos.Length - 1 ? string.Join('.', partes) : null;
            mapa = sub;
            if (i + 1 < passos.Length && Nome(passos[i + 1]) is "infDPS" or "infPedReg")
                i++;
        }
        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static string Nome(string passo)
    {
        var abre = passo.IndexOf('[', StringComparison.Ordinal);
        return abre < 0 ? passo : passo[..abre];
    }

    private static bool Ler(string passo, out string nome, out int? indice)
    {
        indice = null;
        var abre = passo.IndexOf('[', StringComparison.Ordinal);
        if (abre < 0)
        {
            nome = passo;
            return passo.Length > 0;
        }
        nome = passo[..abre];
        if (!passo.EndsWith(']')
            || !int.TryParse(passo.AsSpan(abre + 1, passo.Length - abre - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var numero)
            || numero < 1)
            return false;
        indice = numero;
        return true;
    }
}
