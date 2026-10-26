namespace OpenDFe.SefazEmulada;

// Os CNPJs e CPFs em situação irregular perante o Fisco: o pedaço do cadastro da SEFAZ (e do CCC) que as regras
// do emulador consultam. Desde a NT 2024.001 v1.20 (Ajuste SINIEF 43/23) não há mais denegação: o emitente
// irregular é rejeitado com 781 na autorização (1C17-38) e com 240 no evento e na inutilização (1P10-20, I06); o
// destinatário irregular da NF-e, com 302 (5E17-40, que deixou de ser denegação). A rota /_controle/irregulares
// cadastra e tira; a marca EMULADA:irregular:<papel> no infCpl faz o mesmo para uma nota só. Vale para qualquer
// UF, pelo documento inteiro (o CNPJ do estabelecimento, não a raiz). Em memória, vazio a cada partida (a menos
// que o Armazenamento guarde); seguro para uso concorrente.
internal sealed class Irregulares
{
    private readonly Lock _trava = new();
    private readonly SortedSet<string> _documentos = new(StringComparer.Ordinal);
    private readonly Armazenamento _armazenamento;

    public Irregulares(Armazenamento armazenamento)
    {
        _armazenamento = armazenamento;
        armazenamento.Carregar(this);
    }

    // A partida recarrega o que o Armazenamento guardava, sem gravar de novo.
    public void Restaurar(string documento) => _documentos.Add(documento);

    // Confere o documento (ArgumentException se não é um CNPJ, também o alfanumérico, nem um CPF válido) e o
    // cadastra; falso se já estava.
    public bool Cadastrar(string? documento)
    {
        var conferido = Conferir(documento);
        lock (_trava)
        {
            var novo = _documentos.Add(conferido);
            if (novo)
                _armazenamento.GravarIrregular(conferido);
            return novo;
        }
    }

    public bool Remover(string documento)
    {
        lock (_trava)
        {
            var removido = _documentos.Remove(documento);
            if (removido)
                _armazenamento.RemoverIrregular(documento);
            return removido;
        }
    }

    public bool Contem(string? documento)
    {
        if (documento is null)
            return false;
        lock (_trava)
            return _documentos.Contains(documento);
    }

    public IReadOnlyList<string> Listar()
    {
        lock (_trava)
            return [.. _documentos];
    }

    public void Limpar()
    {
        lock (_trava)
        {
            _documentos.Clear();
            _armazenamento.LimparIrregulares();
        }
    }

    private static string Conferir(string? documento) =>
        documento is not null && (documento.Length == 14 ? Documentos.CnpjValido(documento) : Documentos.CpfValido(documento))
            ? documento
            : throw new ArgumentException("O documento é um CNPJ (14 posições, também o alfanumérico) ou um CPF (11 algarismos) válido, sem máscara.");
}

// O pedido de cadastro, como chega em JSON no POST /_controle/irregulares.
internal sealed record PedidoIrregular(string? Documento);
