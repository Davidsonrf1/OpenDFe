using System.Text.RegularExpressions;

namespace OpenDFe.SefazEmulada;

// Os CSCs (Código de Segurança do Contribuinte) que o hash do QR Code versão 2 da NFC-e usa (Manual do DANFE
// NFC-e e QR Code 6.0, 4.3.3 e 4.6): um por UF, raiz do CNPJ e identificador. O CSC padrão (id 1, o valor de
// ValorPadrao) vale para qualquer emitente, para os testes e o PDV não precisarem cadastrar nada; a rota
// /_controle/csc cadastra outros e revoga, também o padrão. Em memória, vazio a cada partida; seguro para uso
// concorrente.
internal sealed partial class CodigosSeguranca
{
    public const string IdPadrao = "1";
    public const string ValorPadrao = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";

    private readonly Lock _trava = new();
    private readonly Dictionary<(string Uf, string Cnpj, string Id), CscCadastrado> _cadastro = [];

    // Confere o pedido (ArgumentException com o que está errado) e cadastra o CSC; o mesmo identificador
    // cadastrado de novo troca o valor e desfaz a revogação.
    public CscCadastrado Cadastrar(PedidoCsc pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        var (uf, cnpj, id) = Conferir(pedido.Uf, pedido.Cnpj, pedido.Id);
        if (pedido.Csc is not { Length: >= 16 and <= 36 } csc || csc.Any(c => char.IsWhiteSpace(c) || c == '|' || c > '~'))
            throw new ArgumentException("O CSC tem de 16 a 36 caracteres ASCII, sem espaço nem \"|\".", nameof(pedido));
        var cadastrado = new CscCadastrado(uf, cnpj, id, csc, null);
        lock (_trava)
            _cadastro[(uf, cnpj, id)] = cadastrado;
        return cadastrado;
    }

    // Revoga o CSC no instante dado; falso se ele não existe (o padrão sempre existe).
    public bool Revogar(string uf, string cnpj, string id, DateTimeOffset instante)
    {
        (uf, cnpj, id) = Conferir(uf, cnpj, id);
        lock (_trava)
        {
            var atual = _cadastro.GetValueOrDefault((uf, cnpj, id)) ?? Padrao(uf, cnpj, id);
            if (atual is null)
                return false;
            _cadastro[(uf, cnpj, id)] = atual with { Revogacao = atual.Revogacao ?? instante };
            return true;
        }
    }

    // O CSC do emitente (a sigla da UF, a raiz do CNPJ) com o identificador do QR Code; nulo se não há (462).
    public CscCadastrado? Procurar(string uf, string cnpj, string id)
    {
        lock (_trava)
            return _cadastro.GetValueOrDefault((uf, cnpj, id)) ?? Padrao(uf, cnpj, id);
    }

    // Os cadastrados e revogados pela rota, sem o padrão que nunca foi mexido.
    public IReadOnlyList<CscCadastrado> Listar()
    {
        lock (_trava)
            return [.. _cadastro.Values.OrderBy(c => (c.Uf, c.Cnpj, c.Id))];
    }

    public void Limpar()
    {
        lock (_trava)
            _cadastro.Clear();
    }

    private static CscCadastrado? Padrao(string uf, string cnpj, string id) =>
        id == IdPadrao ? new CscCadastrado(uf, cnpj, id, ValorPadrao, null) : null;

    // A sigla da UF, a raiz do CNPJ (8 posições, também a alfanumérica) e o identificador sem os zeros não
    // significativos, como o QR Code o traz (Manual, 4.3.1, 4º parâmetro).
    private static (string Uf, string Cnpj, string Id) Conferir(string? uf, string? cnpj, string? id)
    {
        if (uf is null || UnidadesFederativas.CodigoOuNulo(uf.ToUpperInvariant()) is null)
            throw new ArgumentException($"UF desconhecida: {uf}.");
        if (cnpj is null || !Raiz().IsMatch(cnpj))
            throw new ArgumentException("O CNPJ é a raiz, com 8 posições (letras maiúsculas ou algarismos).");
        if (id is null || !Identificador().IsMatch(id))
            throw new ArgumentException("O identificador do CSC tem até 6 algarismos, sem zeros à esquerda.");
        return (uf.ToUpperInvariant(), cnpj, id);
    }

    [GeneratedRegex("^[0-9A-Z]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex Raiz();

    [GeneratedRegex("^(0|[1-9][0-9]{0,5})$", RegexOptions.CultureInvariant)]
    private static partial Regex Identificador();
}

// O pedido de cadastro, como chega em JSON no POST /_controle/csc: a sigla da UF, a raiz do CNPJ, o
// identificador e o CSC.
internal sealed record PedidoCsc(string? Uf, string? Cnpj, string? Id, string? Csc);

// Um CSC do emitente. Revogacao: o instante em que foi revogado; vale para as notas emitidas antes (463).
internal sealed record CscCadastrado(string Uf, string Cnpj, string Id, string Csc, DateTimeOffset? Revogacao);
