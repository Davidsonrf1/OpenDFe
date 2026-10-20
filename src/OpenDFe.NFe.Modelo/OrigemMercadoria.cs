namespace OpenDFe.NFe;

/// <summary>A origem da mercadoria. O valor de cada membro é o do XML (<c>orig</c>).</summary>
public enum OrigemMercadoria
{
    /// <summary>Nacional, exceto as dos códigos 3, 4, 5 e 8 (0).</summary>
    Nacional = 0,

    /// <summary>Estrangeira, importação direta, exceto a do código 6 (1).</summary>
    EstrangeiraImportacaoDireta = 1,

    /// <summary>Estrangeira, adquirida no mercado interno, exceto a do código 7 (2).</summary>
    EstrangeiraMercadoInterno = 2,

    /// <summary>Nacional, com conteúdo de importação superior a 40% e inferior ou igual a 70% (3).</summary>
    NacionalConteudoImportacaoMedio = 3,

    /// <summary>Nacional, produzida conforme os processos produtivos básicos (4).</summary>
    NacionalProcessoProdutivoBasico = 4,

    /// <summary>Nacional, com conteúdo de importação inferior ou igual a 40% (5).</summary>
    NacionalConteudoImportacaoBaixo = 5,

    /// <summary>Estrangeira, importação direta, sem similar nacional, constante em lista da CAMEX (6).</summary>
    EstrangeiraImportacaoDiretaCamex = 6,

    /// <summary>
    /// Estrangeira, adquirida no mercado interno, sem similar nacional, constante em lista da CAMEX (7).
    /// </summary>
    EstrangeiraMercadoInternoCamex = 7,

    /// <summary>Nacional, com conteúdo de importação superior a 70% (8).</summary>
    NacionalConteudoImportacaoAlto = 8,
}
