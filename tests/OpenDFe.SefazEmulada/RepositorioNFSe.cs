using System.Globalization;
using System.Security.Cryptography;

namespace OpenDFe.SefazEmulada;

// Uma NFS-e gerada pela Sefin emulada: a chave, o Id da DPS que a originou, o número, o processamento e o XML assinado
// (o NFSe, com a DPS dentro).
internal sealed record NFSeRegistrada(
    string Chave, string IdDps, long Numero, long NumeroDfse, int Municipio, string Prestador, DateTimeOffset Processamento, string Xml);

// As NFS-e geradas, pela chave e pelo Id da DPS, e as numerações: o nNFSe por município e inscrição, o nDFSe do
// ambiente e o código numérico da chave. Em memória, vazio a cada partida (a persistência é da E.9); seguro para uso
// concorrente. O Registrar devolve a NFS-e que já existia para a DPS, se outra chegou antes (a duplicidade, E0014).
internal sealed class RepositorioNFSe
{
    private readonly Lock _trava = new();
    private readonly Dictionary<string, NFSeRegistrada> _chaves = new(StringComparer.Ordinal);
    private readonly Dictionary<string, NFSeRegistrada> _dps = new(StringComparer.Ordinal);
    private readonly List<NFSeRegistrada> _ordem = [];
    private readonly Dictionary<(int, string), long> _numeros = [];
    private long _documentos;

    public NFSeRegistrada? Chave(string chave)
    {
        lock (_trava)
            return _chaves.GetValueOrDefault(chave);
    }

    public NFSeRegistrada? Dps(string id)
    {
        lock (_trava)
            return _dps.GetValueOrDefault(id);
    }

    // Gera a NFS-e com os números reservados aqui, debaixo da trava, para duas DPS iguais ao mesmo tempo não gerarem duas
    // notas: a segunda recebe a primeira, como Existente.
    public (NFSeRegistrada? Gerada, NFSeRegistrada? Existente) Gerar(string idDps, int municipio, string prestador,
        Func<long, long, int, NFSeRegistrada> gerar)
    {
        ArgumentNullException.ThrowIfNull(gerar);
        lock (_trava)
        {
            if (_dps.TryGetValue(idDps, out var existente))
                return (null, existente);
            var numero = _numeros.GetValueOrDefault((municipio, prestador)) + 1;
            var nota = gerar(numero, _documentos + 1, RandomNumberGenerator.GetInt32(1, 1_000_000_000));
            _numeros[(municipio, prestador)] = numero;
            _documentos++;
            _chaves[nota.Chave] = nota;
            _dps[idDps] = nota;
            _ordem.Add(nota);
            return (nota, null);
        }
    }

    public IReadOnlyList<NFSeRegistrada> Listar()
    {
        lock (_trava)
            return [.. _ordem];
    }

    // Esquece as notas; as numerações continuam, como a sequência dos protocolos da NF-e.
    public void Limpar()
    {
        lock (_trava)
        {
            _chaves.Clear();
            _dps.Clear();
            _ordem.Clear();
        }
    }

    // O código numérico de 9 posições, com zeros à esquerda.
    public static string Codigo(int codigo) => codigo.ToString("D9", CultureInfo.InvariantCulture);
}
