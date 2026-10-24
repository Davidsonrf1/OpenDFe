using OpenDFe.Impressao;

namespace OpenDFe.NFe;

// Os dados adicionais (Anexo II 3.1.8 e 3.1.9): as informações complementares e o quadro reservado ao Fisco.
internal static partial class MontagemDanfe
{
    // As informações complementares trazem tudo o que consta do infAdFisco e do infCpl (3.1.8); as observações do contribuinte
    // (obsCont) são facultativas e não saem. O ambiente de homologação leva a frase "SEM VALOR FISCAL" aqui e na marca
    // d'água (3). O quadro "Reservado ao Fisco" fica em branco: o contribuinte não o preenche (3.1.9).
    private static SecaoImpressao Adicionais(NotaFiscal nota)
    {
        var adicionais = nota.InformacoesAdicionais;
        var linhas = new List<string>();
        if (nota.Identificacao.Ambiente == Ambiente.Homologacao)
            linhas.Add(TextoHomologacao);
        if (!string.IsNullOrWhiteSpace(adicionais?.Fisco))
            linhas.Add(adicionais.Fisco);
        if (!string.IsNullOrWhiteSpace(adicionais?.Complementares))
            linhas.Add(adicionais.Complementares);

        return Secao("DADOS ADICIONAIS",
            Linha(
                Campo("INFORMAÇÕES COMPLEMENTARES", string.Join(Quebra, linhas), 12.95),
                Campo("RESERVADO AO FISCO", string.Empty, 7.62)));
    }
}
