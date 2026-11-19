using OpenDFe.NFSe;

namespace OpenDFe.Testes;

/// <summary>
/// Os casos do DANFSe (etapa 9.5): as NFS-e de exemplo (as DPS de teste da 9.1 dentro da NFS-e montada pelo mapa e assinada),
/// mais as variações que a NT 008/2026 trata à parte (a produção restrita, as supressões, a não incidência do ISSQN, a
/// competência de 2027, os textos no limite, a 1.00, a cancelada, a substituída, sem o canhoto). Os golden de texto e os
/// testes do PDF usam os mesmos casos.
/// </summary>
internal static class CatalogoDanfse
{
    internal sealed record Caso(string Nome, NFSeLida Lida, OpcoesDanfse? Opcoes = null);

    internal static IEnumerable<Caso> Todos()
    {
        yield return Novo("Minima", DpsTeste.Minima());
        yield return Novo("CompletaA", DpsTeste.CompletaA());
        yield return Novo("CompletaB", DpsTeste.CompletaB());
        yield return Novo("CompletaC", DpsTeste.CompletaC());
        yield return Novo("CompletaD", DpsTeste.CompletaD());
        // Todas as linhas de todos os blocos, e a descrição numa linha: as posições da tabela do 2.4.5, uma a uma.
        yield return Novo("Anexo", Ajustar(DpsTeste.CompletaA(), dps =>
        {
            dps.Intermediario!.Email = "intermediario@exemplo.com.br";
            dps.Servico.Codigo.Descricao = "Desenvolvimento de software sob encomenda";
        }));
        yield return Novo("Producao", Ajustar(DpsTeste.CompletaA(), dps => dps.Ambiente = Ambiente.Producao));
        yield return Novo("DestinatarioTomador", Ajustar(DpsTeste.CompletaA(), dps =>
        {
            dps.IbsCbs!.IndicadorDestinatario = IndicadorDestinatario.Tomador;
            dps.IbsCbs.Destinatario = null;
        }));
        yield return Novo("NaoIncidencia", Ajustar(DpsTeste.CompletaA(), dps => dps.Valores.Tributacao.Municipal.Tributacao = TributacaoIssqn.NaoIncidencia));
        yield return Novo("Competencia2027", Ajustar(DpsTeste.CompletaA(), dps => dps.Competencia = new DateOnly(2027, 1, 15)));
        yield return Novo("TextosLimite", Ajustar(DpsTeste.CompletaA(), dps =>
        {
            dps.Servico.Codigo.Descricao = Repetir("Descrição longa do serviço prestado, com acentuação e números 1234. ", 2000);
            dps.Servico.InformacoesComplementares!.Texto = Repetir("Informação complementar extensa do contribuinte. ", 2000);
            dps.Prestador.Nome = Repetir("NOME EMPRESARIAL MUITO LONGO ", 150);
        }));
        yield return new Caso("Versao100", LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100()));
        yield return Novo("Cancelada", DpsTeste.CompletaA(), new OpcoesDanfse { Cancelada = true });
        yield return Novo("Substituida", DpsTeste.CompletaA(), new OpcoesDanfse { Substituida = true });
        yield return Novo("CanhotoSuprimido", DpsTeste.CompletaA(), new OpcoesDanfse { Canhoto = false });
    }

    internal static Caso Achar(string nome) => Todos().Single(c => c.Nome == nome);

    internal static NFSeLida Ler(Dps dps) => LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml(NFSeTeste.Nota(dps)));

    private static Caso Novo(string nome, Dps dps, OpcoesDanfse? opcoes = null) => new(nome, Ler(dps), opcoes);

    private static Dps Ajustar(Dps dps, Action<Dps> ajuste)
    {
        ajuste(dps);
        return dps;
    }

    private static string Repetir(string trecho, int tamanho)
    {
        var texto = string.Concat(Enumerable.Repeat(trecho, tamanho / trecho.Length + 1));
        return texto[..tamanho].TrimEnd();
    }
}
