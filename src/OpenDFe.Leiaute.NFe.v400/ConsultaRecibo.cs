namespace OpenDFe.NFe;

/// <summary>
/// A consulta do recibo do lote (<c>NFeRetAutorizacao4</c>, docs/CONTRATOS.md 14): os protocolos das notas do lote
/// que a <see cref="Autorizacao"/> devolveu com o recibo (103).
/// </summary>
/// <remarks>
/// Com as notas assinadas do lote em <see cref="Xmls"/> (as <see cref="ResultadoNota.Xml"/> do retorno com o recibo),
/// o retorno liga cada protocolo à nota e traz o <c>nfeProc</c>, e a consulta vai ao autorizador da chave da primeira
/// nota (PROJETO.md 8.4); sem elas, só os protocolos, e a consulta vai ao autorizador do núcleo. A lib não consulta
/// sozinha: esperar o tempo médio e repetir é de quem usa (decisão 14).
/// </remarks>
public sealed class ConsultaRecibo : ParametroWS<RetornoAutorizacao>
{
    private const int TamanhoRecibo = 15;

    private readonly IReadOnlyList<string> _xmls = [];

    /// <summary>A consulta do recibo.</summary>
    /// <param name="recibo">O <c>nRec</c>: 15 dígitos.</param>
    /// <exception cref="ArgumentNullException">O recibo é nulo.</exception>
    /// <exception cref="ArgumentException">O recibo não tem 15 dígitos.</exception>
    public ConsultaRecibo(string recibo)
    {
        ArgumentNullException.ThrowIfNull(recibo);
        if (recibo.Length != TamanhoRecibo || !recibo.All(char.IsAsciiDigit))
            throw new ArgumentException("O recibo tem 15 dígitos.", nameof(recibo));
        Recibo = recibo;
    }

    /// <summary>O número do recibo (<c>nRec</c>).</summary>
    public string Recibo { get; }

    /// <summary>As notas assinadas do lote, para o <c>nfeProc</c>; vazia: só os protocolos.</summary>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">Mais de 50 notas, ou uma nula ou vazia.</exception>
    public IReadOnlyList<string> Xmls
    {
        get => _xmls;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            string[] lista = [.. value];
            if (lista.Length > Autorizacao.MaximoNotas)
                throw new ArgumentException($"O lote vai até {Autorizacao.MaximoNotas} notas: vieram {lista.Length}.", nameof(value));
            if (Array.Exists(lista, string.IsNullOrWhiteSpace))
                throw new ArgumentException("Uma das notas assinadas é nula ou vazia.", nameof(value));
            _xmls = lista.AsReadOnly();
        }
    }
}
