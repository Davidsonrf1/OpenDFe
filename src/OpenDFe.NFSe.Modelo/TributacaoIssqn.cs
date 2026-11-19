namespace OpenDFe.NFSe;

/// <summary>A tributação do ISSQN (<c>tribISSQN</c>), pela versão 1.01 e o Anexo I. O XSD 1.00 dá outro texto para os códigos 2 a 4 (2 exportação, 3 não incidência, 4 imunidade): a leitura da 1.00 guarda o número como veio, sem traduzir (docs/NFSE.md 7, M12). O valor de cada membro é o do XML.</summary>
public enum TributacaoIssqn
{
    /// <summary>Operação tributável (1).</summary>
    Tributavel = 1,

    /// <summary>Imunidade (2).</summary>
    Imunidade = 2,

    /// <summary>Exportação de serviço (3).</summary>
    Exportacao = 3,

    /// <summary>Não incidência (4).</summary>
    NaoIncidencia = 4,
}
