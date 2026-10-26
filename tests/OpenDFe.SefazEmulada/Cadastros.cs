using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenDFe.SefazEmulada;

// O cadastro de contribuintes do ICMS que a consulta cadastro acha (MOC 7.0, Visão Geral, 5.6), carregado pela
// rota /_controle/cadastros. Um contribuinte por UF e IE; cadastrar de novo a mesma IE na mesma UF troca o que
// estava. A situação que a consulta devolve (cSit) é a do cadastro e também a dos Irregulares: o CNPJ ou o CPF
// irregular sai como não habilitado (0), como a autorização e o evento o tratam (781, 240). Em memória, vazio a
// cada partida (a menos que o Armazenamento guarde); seguro para uso concorrente.
internal sealed partial class Cadastros
{
    private readonly Lock _trava = new();
    private readonly SortedDictionary<(string Uf, string Ie), Contribuinte> _contribuintes = [];
    private readonly Armazenamento _armazenamento;

    public Cadastros(Armazenamento armazenamento)
    {
        _armazenamento = armazenamento;
        armazenamento.Carregar(this);
    }

    // A partida recarrega o que o Armazenamento guardava, sem gravar de novo.
    public void Restaurar(Contribuinte contribuinte) => _contribuintes[(contribuinte.Uf!, contribuinte.Ie!)] = contribuinte;

    // Confere o contribuinte (ArgumentException com o que está errado) e o cadastra; verdadeiro se é novo, falso
    // se trocou o que havia na mesma UF e IE.
    public bool Cadastrar(Contribuinte? contribuinte)
    {
        var conferido = Conferir(contribuinte);
        lock (_trava)
        {
            var novo = !_contribuintes.ContainsKey((conferido.Uf!, conferido.Ie!));
            _contribuintes[(conferido.Uf!, conferido.Ie!)] = conferido;
            _armazenamento.GravarCadastro(conferido);
            return novo;
        }
    }

    public bool Remover(string uf, string ie)
    {
        lock (_trava)
        {
            var removido = _contribuintes.Remove((uf.ToUpperInvariant(), ie));
            if (removido)
                _armazenamento.RemoverCadastro(uf, ie);
            return removido;
        }
    }

    public IReadOnlyList<Contribuinte> Listar()
    {
        lock (_trava)
            return [.. _contribuintes.Values];
    }

    public void Limpar()
    {
        lock (_trava)
        {
            _contribuintes.Clear();
            _armazenamento.LimparCadastros();
        }
    }

    // Os contribuintes da UF pelo argumento da consulta: pelo CNPJ ou pelo CPF, todos os estabelecimentos com
    // ele; pela IE, o da IE e os que a têm como IE única (GR07: a IE única devolve todos os estabelecimentos).
    public IReadOnlyList<Contribuinte> Procurar(string uf, string? ie, string? cnpj, string? cpf)
    {
        lock (_trava)
        {
            return [.. _contribuintes.Values.Where(c => c.Uf == uf
                && (ie is not null ? c.Ie == ie || c.IeUnica == ie : cnpj is not null ? c.Cnpj == cnpj : c.Cpf == cpf))];
        }
    }

    private static Contribuinte Conferir(Contribuinte? contribuinte)
    {
        if (contribuinte is null)
            throw new ArgumentException("O contribuinte é obrigatório.");
        var uf = contribuinte.Uf?.ToUpperInvariant();
        if (uf is null || UnidadesFederativas.CodigoOuNulo(uf) is null)
            throw new ArgumentException("A uf é a sigla de uma UF.");
        if (!InscricaoValida(contribuinte.Ie))
            throw new ArgumentException("A ie tem de 2 a 14 algarismos, sem máscara, e não é toda zero.");
        if ((contribuinte.Cnpj is null) == (contribuinte.Cpf is null))
            throw new ArgumentException("O contribuinte tem o cnpj ou o cpf, um dos dois.");
        if (contribuinte.Cnpj is not null && !Documentos.CnpjValido(contribuinte.Cnpj))
            throw new ArgumentException("O cnpj tem 14 posições (também o alfanumérico), sem máscara, com o DV certo.");
        if (contribuinte.Cpf is not null && !Documentos.CpfValido(contribuinte.Cpf))
            throw new ArgumentException("O cpf tem 11 algarismos, sem máscara, com o DV certo.");
        Texto(contribuinte.Nome, "nome", 60, obrigatorio: true);
        Texto(contribuinte.Fantasia, "fantasia", 60);
        Texto(contribuinte.RegimeApuracao, "regimeApuracao", 60);
        if (contribuinte.Cnae is not null && !Cnae().IsMatch(contribuinte.Cnae))
            throw new ArgumentException("O cnae tem 6 ou 7 algarismos.");
        if (contribuinte.IeUnica is not null && !InscricaoValida(contribuinte.IeUnica))
            throw new ArgumentException("A ieUnica tem de 2 a 14 algarismos e não é toda zero.");
        if (contribuinte.IeAtual is not null && !InscricaoValida(contribuinte.IeAtual))
            throw new ArgumentException("A ieAtual tem de 2 a 14 algarismos e não é toda zero.");
        if (contribuinte.CredenciadoNFe is not ("0" or "1" or "2" or "3" or "4"))
            throw new ArgumentException("O credenciadoNFe é de 0 a 4 (GR12a).");
        if (contribuinte.CredenciadoCTe is not ("0" or "1" or "2" or "3" or "4"))
            throw new ArgumentException("O credenciadoCTe é de 0 a 4 (GR12b).");
        if (contribuinte.Endereco is { } endereco)
        {
            Texto(endereco.Logradouro, "endereco.logradouro", 255);
            Texto(endereco.Numero, "endereco.numero", 60);
            Texto(endereco.Complemento, "endereco.complemento", 60);
            Texto(endereco.Bairro, "endereco.bairro", 60);
            Texto(endereco.Municipio, "endereco.municipio", 60);
            if (endereco.CodigoMunicipio is not null && !Municipio().IsMatch(endereco.CodigoMunicipio))
                throw new ArgumentException("O endereco.codigoMunicipio tem os 7 algarismos do IBGE.");
            if (endereco.Cep is not null && !Cep().IsMatch(endereco.Cep))
                throw new ArgumentException("O endereco.cep tem 7 ou 8 algarismos.");
        }
        return contribuinte with { Uf = uf };
    }

    // A IE da consulta e do cadastro: de 2 a 14 algarismos (TIe, sem o ISENTO), e não toda zero. O DV, que cada UF
    // calcula do seu jeito, não é conferido.
    public static bool InscricaoValida(string? ie) =>
        ie is not null && Inscricao().IsMatch(ie) && ie.Any(c => c != '0');

    // O texto do schema (TString): sem espaço no começo nem no fim, até o tamanho.
    private static void Texto(string? valor, string campo, int tamanho, bool obrigatorio = false)
    {
        if (valor is null)
        {
            if (obrigatorio)
                throw new ArgumentException($"O {campo} é obrigatório.");
            return;
        }
        if (valor.Length == 0 || valor.Length > tamanho || valor.Trim() != valor)
            throw new ArgumentException(string.Create(CultureInfo.InvariantCulture,
                $"O {campo} tem de 1 a {tamanho} caracteres, sem espaço no começo nem no fim."));
    }

    [GeneratedRegex("^[0-9]{2,14}$", RegexOptions.CultureInvariant)]
    private static partial Regex Inscricao();

    [GeneratedRegex("^[0-9]{6,7}$", RegexOptions.CultureInvariant)]
    private static partial Regex Cnae();

    [GeneratedRegex("^[0-9]{7}$", RegexOptions.CultureInvariant)]
    private static partial Regex Municipio();

    [GeneratedRegex("^[0-9]{7,8}$", RegexOptions.CultureInvariant)]
    private static partial Regex Cep();
}

// Um contribuinte do cadastro (Tabela 5-22, GR07 a GR29), como chega em JSON no POST /_controle/cadastros e sai
// no GET. Habilitado é o cSit (GR12); CredenciadoNFe e CredenciadoCTe, o indCredNFe e o indCredCTe (GR12a e GR12b).
internal sealed record Contribuinte(
    string? Uf,
    string? Ie,
    string? Cnpj = null,
    string? Cpf = null,
    string? Nome = null,
    string? Fantasia = null,
    string? RegimeApuracao = null,
    string? Cnae = null,
    DateOnly? InicioAtividade = null,
    DateOnly? UltimaSituacao = null,
    DateOnly? Baixa = null,
    string? IeUnica = null,
    string? IeAtual = null,
    EnderecoContribuinte? Endereco = null,
    bool Habilitado = true,
    string CredenciadoNFe = "1",
    string CredenciadoCTe = "4");

// O endereço do contribuinte (GR22 a GR29).
internal sealed record EnderecoContribuinte(
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? CodigoMunicipio = null,
    string? Municipio = null,
    string? Cep = null);
