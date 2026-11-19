namespace OpenDFe.NFSe;

/// <summary>O indicador de não informar o total aproximado dos tributos (<c>indTotTrib</c>). O XSD só aceita o 0; o enum fica aberto às NTs (docs/NFSE.md 7, M6). O valor de cada membro é o do XML.</summary>
public enum IndicadorTotalTributos
{
    /// <summary>Não informar nenhum valor estimado (Decreto 8.264/2014) (0).</summary>
    NaoInformado = 0,
}
