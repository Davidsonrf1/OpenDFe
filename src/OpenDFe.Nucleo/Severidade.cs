namespace OpenDFe;

/// <summary>Severidade de um <see cref="Problema"/>.</summary>
public enum Severidade
{
    /// <summary>Não impede o envio, mas precisa ser visto (por exemplo, um campo que o leiaute não suporta).</summary>
    Aviso = 1,

    /// <summary>Impede o envio: o documento seria rejeitado.</summary>
    Erro = 2,
}
