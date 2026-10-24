namespace OpenDFe.NFe;

/// <summary>Qual das três consultas do <c>distDFeInt</c> a <see cref="DistribuicaoDFe"/> faz (NT 2014.002, 3.4.1).</summary>
public enum TipoConsultaDistribuicao
{
    /// <summary>A distribuição a partir do último NSU (<c>distNSU</c>): até 50 documentos por resposta, em ordem de NSU.</summary>
    UltimoNsu = 0,

    /// <summary>A consulta pontual de um NSU (<c>consNSU</c>): conta no limite de 20 consultas por hora.</summary>
    Nsu = 1,

    /// <summary>A consulta pontual de uma NF-e pela chave (<c>consChNFe</c>): conta no limite de 20 consultas por hora.</summary>
    Chave = 2,
}
