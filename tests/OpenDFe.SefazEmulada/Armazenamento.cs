using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace OpenDFe.SefazEmulada;

// O que o emulador guarda em disco com --persistir sim, para valer entre as partidas: os dados de controle num
// SQLite (<pasta>/persistencia/controle.db) e os XMLs em arquivos, por autorizador (<pasta>/persistencia/xml/<autorizador>/):
//   nfe/<chave>.xml                       a nota com o protNFe (o nfeProc)
//   eventos/<chave>-<tipo>-<seq>.xml      o procEventoNFe de cada evento registrado
//   lotes/<recibo>.xml                    os protNFe do lote assíncrono
//   distribuicao/<interessado>/<NSU>-<schema>.xml   o que o AN distribui (autorizador AN)
// Sem a persistência (o padrão e os testes), Nenhum não faz nada. Cada gravação é feita na hora, com a Trava do
// repositório (ou a do próprio cadastro) já tomada; a conexão é uma só, protegida por uma trava própria.
internal sealed class Armazenamento : IDisposable
{
    private const string Pasta = "persistencia";

    private static readonly UTF8Encoding Utf8 = new(false);

    private readonly SqliteConnection? _conexao;
    private readonly string _xml;
    private readonly Lock _trava = new();

    private Armazenamento(SqliteConnection? conexao, string xml)
    {
        _conexao = conexao;
        _xml = xml;
    }

    // O armazenamento desligado: nada é gravado nem lido.
    public static Armazenamento Nenhum { get; } = new(null, "");

    public bool Ativo => _conexao is not null;

    // Abre (e cria, se preciso) o banco e a pasta dos XMLs dentro da pasta do emulador.
    public static Armazenamento Abrir(string pasta)
    {
        var raiz = Path.Combine(pasta, Pasta);
        Directory.CreateDirectory(raiz);
        var xml = Path.Combine(raiz, "xml");
        Directory.CreateDirectory(xml);
        var conexao = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(raiz, "controle.db"),
            Pooling = false,
        }.ToString());
        conexao.Open();
        var armazenamento = new Armazenamento(conexao, xml);
        armazenamento.Criar();
        return armazenamento;
    }

    public void Dispose() => _conexao?.Dispose();

    // ----- Repositório -----

    public void GravarSequencia(long sequencia) =>
        Executar("INSERT INTO meta (chave, valor) VALUES ('sequencia', $v) ON CONFLICT(chave) DO UPDATE SET valor = $v",
            ("$v", sequencia));

    public void GravarNota(NotaRegistrada nota)
    {
        if (!Ativo)
            return;
        var arquivo = Gravar($"{nota.Autorizador}/nfe/{nota.Chave}.xml", Interessados.Processada(nota));
        Executar("INSERT INTO notas (chave, numeracao, protocolo, recibo, recebimento, autorizador, situacao, cancelada, arquivo) "
            + "VALUES ($chave, $numeracao, $protocolo, $recibo, $recebimento, $autorizador, $situacao, $cancelada, $arquivo)",
            ("$chave", nota.Chave), ("$numeracao", nota.Numeracao), ("$protocolo", nota.Protocolo), ("$recibo", nota.Recibo),
            ("$recebimento", Texto(nota.Recebimento)), ("$autorizador", nota.Autorizador), ("$situacao", nota.Situacao),
            ("$cancelada", nota.Cancelada ? 1 : 0), ("$arquivo", arquivo));
    }

    public void GravarCancelada(string chave) =>
        Executar("UPDATE notas SET cancelada = 1 WHERE chave = $chave", ("$chave", chave));

    public void GravarEvento(EventoRegistrado evento)
    {
        if (!Ativo)
            return;
        var arquivo = Gravar($"{evento.Autorizador}/eventos/{evento.Chave}-{evento.Tipo}-{evento.Sequencia.ToString("D2", CultureInfo.InvariantCulture)}.xml",
            evento.Processado);
        Executar("INSERT INTO eventos (chave, tipo, sequencia, protocolo, registro, autorizador, arquivo) "
            + "VALUES ($chave, $tipo, $sequencia, $protocolo, $registro, $autorizador, $arquivo)",
            ("$chave", evento.Chave), ("$tipo", evento.Tipo), ("$sequencia", evento.Sequencia), ("$protocolo", evento.Protocolo),
            ("$registro", Texto(evento.Registro)), ("$autorizador", evento.Autorizador), ("$arquivo", arquivo));
    }

    public void GravarFaixa(FaixaInutilizada faixa) =>
        Executar("INSERT INTO faixas (uf, cnpj, modelo, serie, inicio, fim, protocolo, registro, autorizador) "
            + "VALUES ($uf, $cnpj, $modelo, $serie, $inicio, $fim, $protocolo, $registro, $autorizador)",
            ("$uf", faixa.Uf), ("$cnpj", faixa.Cnpj), ("$modelo", faixa.Modelo), ("$serie", faixa.Serie), ("$inicio", faixa.Inicio),
            ("$fim", faixa.Fim), ("$protocolo", faixa.Protocolo), ("$registro", Texto(faixa.Registro)), ("$autorizador", faixa.Autorizador));

    // Grava o lote, ou só as consultas que ainda respondem 105, se ele já estava.
    public void GravarLote(LoteRecebido lote)
    {
        if (!Ativo)
            return;
        var arquivo = $"{lote.Autorizador}/lotes/{lote.Recibo}.xml";
        if (!File.Exists(Path.Combine(_xml, arquivo)))
            Gravar(arquivo, new XElement(Servico.NamespaceNFe + "protocolos", lote.Protocolos.Select(p => new XElement(p))));
        Executar("INSERT INTO lotes (autorizador, recibo, transmissor, pendentes, arquivo) VALUES ($autorizador, $recibo, $transmissor, $pendentes, $arquivo) "
            + "ON CONFLICT(autorizador, recibo) DO UPDATE SET pendentes = $pendentes",
            ("$autorizador", lote.Autorizador), ("$recibo", lote.Recibo), ("$transmissor", lote.Transmissor),
            ("$pendentes", lote.Pendentes), ("$arquivo", arquivo));
    }

    public void GravarDocumento(string interessado, DocumentoDistribuido documento)
    {
        if (!Ativo)
            return;
        var arquivo = Gravar(string.Create(CultureInfo.InvariantCulture,
            $"AN/distribuicao/{interessado}/{documento.Nsu:D9}-{Path.GetFileNameWithoutExtension(documento.Schema)}.xml"), documento.Xml);
        Executar("INSERT INTO documentos (interessado, nsu, chave, schema, recebimento, arquivo) VALUES ($interessado, $nsu, $chave, $schema, $recebimento, $arquivo)",
            ("$interessado", interessado), ("$nsu", documento.Nsu), ("$chave", documento.Chave), ("$schema", documento.Schema),
            ("$recebimento", Texto(documento.Recebimento)), ("$arquivo", arquivo));
    }

    public void GravarLiberada(string chave) =>
        Executar("INSERT OR IGNORE INTO liberadas (chave) VALUES ($chave)", ("$chave", chave));

    public void GravarConsumo(string documento, Consumo consumo) =>
        Executar("INSERT INTO consumos (documento, ultimo_nsu, nenhum, bloqueio, motivo, pontuais) VALUES ($documento, $ultimo, $nenhum, $bloqueio, $motivo, $pontuais) "
            + "ON CONFLICT(documento) DO UPDATE SET ultimo_nsu = $ultimo, nenhum = $nenhum, bloqueio = $bloqueio, motivo = $motivo, pontuais = $pontuais",
            ("$documento", documento), ("$ultimo", consumo.UltimoNsu), ("$nenhum", consumo.Nenhum is { } n ? Texto(n) : null),
            ("$bloqueio", consumo.Bloqueio is { } b ? Texto(b) : null), ("$motivo", consumo.Motivo),
            ("$pontuais", string.Join(';', consumo.Pontuais.Select(Texto))));

    // Esquece o que o repositório guarda (as notas, os lotes, os eventos, as faixas e a distribuição), com os XMLs.
    // A sequência dos recibos e dos protocolos continua.
    public void LimparRepositorio()
    {
        if (!Ativo)
            return;
        Executar("DELETE FROM notas; DELETE FROM eventos; DELETE FROM faixas; DELETE FROM lotes; DELETE FROM documentos; DELETE FROM liberadas; DELETE FROM consumos");
        foreach (var autorizador in Directory.EnumerateDirectories(_xml))
            Directory.Delete(autorizador, recursive: true);
    }

    // ----- CSCs, irregulares e cadastros -----

    public void GravarCsc(CscCadastrado csc) =>
        Executar("INSERT INTO csc (uf, cnpj, id, csc, revogacao) VALUES ($uf, $cnpj, $id, $csc, $revogacao) "
            + "ON CONFLICT(uf, cnpj, id) DO UPDATE SET csc = $csc, revogacao = $revogacao",
            ("$uf", csc.Uf), ("$cnpj", csc.Cnpj), ("$id", csc.Id), ("$csc", csc.Csc),
            ("$revogacao", csc.Revogacao is { } r ? Texto(r) : null));

    public void LimparCsc() => Executar("DELETE FROM csc");

    public void GravarIrregular(string documento) =>
        Executar("INSERT OR IGNORE INTO irregulares (documento) VALUES ($documento)", ("$documento", documento));

    public void RemoverIrregular(string documento) =>
        Executar("DELETE FROM irregulares WHERE documento = $documento", ("$documento", documento));

    public void LimparIrregulares() => Executar("DELETE FROM irregulares");

    public void GravarCadastro(Contribuinte contribuinte) =>
        Executar("INSERT INTO cadastros (uf, ie, json) VALUES ($uf, $ie, $json) ON CONFLICT(uf, ie) DO UPDATE SET json = $json",
            ("$uf", contribuinte.Uf), ("$ie", contribuinte.Ie), ("$json", JsonSerializer.Serialize(contribuinte)));

    public void RemoverCadastro(string uf, string ie) =>
        Executar("DELETE FROM cadastros WHERE uf = $uf AND ie = $ie", ("$uf", uf.ToUpperInvariant()), ("$ie", ie));

    public void LimparCadastros() => Executar("DELETE FROM cadastros");

    // ----- Leitura, na partida -----

    public void Carregar(Repositorio repositorio)
    {
        if (!Ativo)
            return;
        Consultar("SELECT valor FROM meta WHERE chave = 'sequencia'", l => repositorio.Restaurar(sequencia: l.GetInt64(0)));
        Consultar("SELECT chave, numeracao, protocolo, recibo, recebimento, autorizador, situacao, cancelada, arquivo FROM notas ORDER BY rowid", l =>
        {
            var processada = Ler(l.GetString(8));
            repositorio.Restaurar(new NotaRegistrada(l.GetString(0), l.GetString(1), l.GetString(2), l.IsDBNull(3) ? null : l.GetString(3),
                Data(l.GetString(4)), l.GetString(5), l.GetString(6),
                processada.Element(Servico.NamespaceNFe + "protNFe")!, processada.Element(Servico.NamespaceNFe + "NFe")!, l.GetInt32(7) != 0));
        });
        Consultar("SELECT chave, tipo, sequencia, protocolo, registro, autorizador, arquivo FROM eventos ORDER BY rowid", l =>
            repositorio.Restaurar(new EventoRegistrado(l.GetString(0), l.GetString(1), l.GetInt32(2), l.GetString(3), Data(l.GetString(4)),
                l.GetString(5), Ler(l.GetString(6)))));
        Consultar("SELECT uf, cnpj, modelo, serie, inicio, fim, protocolo, registro, autorizador FROM faixas ORDER BY rowid", l =>
            repositorio.Restaurar(new FaixaInutilizada(l.GetString(0), l.GetString(1), l.GetString(2), l.GetInt32(3), l.GetInt64(4), l.GetInt64(5),
                l.GetString(6), Data(l.GetString(7)), l.GetString(8))));
        Consultar("SELECT autorizador, recibo, transmissor, pendentes, arquivo FROM lotes ORDER BY rowid", l =>
            repositorio.Restaurar(new LoteRecebido(l.GetString(0), l.GetString(1), l.GetString(2), [.. Ler(l.GetString(4)).Elements()], l.GetInt32(3))));
        Consultar("SELECT interessado, nsu, chave, schema, recebimento, arquivo FROM documentos ORDER BY interessado, nsu", l =>
            repositorio.Interessados.Restaurar(l.GetString(0),
                new DocumentoDistribuido(l.GetInt64(1), l.GetString(2), l.GetString(3), Ler(l.GetString(5)), Data(l.GetString(4)))));
        Consultar("SELECT chave FROM liberadas", l => repositorio.Interessados.RestaurarLiberada(l.GetString(0)));
        Consultar("SELECT documento, ultimo_nsu, nenhum, bloqueio, motivo, pontuais FROM consumos", l =>
        {
            var consumo = new Consumo
            {
                UltimoNsu = l.IsDBNull(1) ? null : l.GetInt64(1),
                Nenhum = l.IsDBNull(2) ? null : Data(l.GetString(2)),
                Bloqueio = l.IsDBNull(3) ? null : Data(l.GetString(3)),
                Motivo = l.GetString(4),
            };
            consumo.Pontuais.AddRange(l.GetString(5).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(Data));
            repositorio.Interessados.Restaurar(l.GetString(0), consumo);
        });
    }

    public void Carregar(CodigosSeguranca codigos) =>
        Consultar("SELECT uf, cnpj, id, csc, revogacao FROM csc", l =>
            codigos.Restaurar(new CscCadastrado(l.GetString(0), l.GetString(1), l.GetString(2), l.GetString(3),
                l.IsDBNull(4) ? null : Data(l.GetString(4)))));

    public void Carregar(Irregulares irregulares) =>
        Consultar("SELECT documento FROM irregulares", l => irregulares.Restaurar(l.GetString(0)));

    public void Carregar(Cadastros cadastros) =>
        Consultar("SELECT json FROM cadastros", l => cadastros.Restaurar(JsonSerializer.Deserialize<Contribuinte>(l.GetString(0))!));

    // ----- Apoio -----

    private void Criar() =>
        Executar("""
            CREATE TABLE IF NOT EXISTS meta (chave TEXT PRIMARY KEY, valor);
            CREATE TABLE IF NOT EXISTS notas (chave TEXT PRIMARY KEY, numeracao TEXT NOT NULL, protocolo TEXT NOT NULL, recibo TEXT,
                recebimento TEXT NOT NULL, autorizador TEXT NOT NULL, situacao TEXT NOT NULL, cancelada INTEGER NOT NULL, arquivo TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS eventos (chave TEXT NOT NULL, tipo TEXT NOT NULL, sequencia INTEGER NOT NULL, protocolo TEXT NOT NULL,
                registro TEXT NOT NULL, autorizador TEXT NOT NULL, arquivo TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS faixas (uf TEXT NOT NULL, cnpj TEXT NOT NULL, modelo TEXT NOT NULL, serie INTEGER NOT NULL,
                inicio INTEGER NOT NULL, fim INTEGER NOT NULL, protocolo TEXT NOT NULL, registro TEXT NOT NULL, autorizador TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS lotes (autorizador TEXT NOT NULL, recibo TEXT NOT NULL, transmissor TEXT NOT NULL, pendentes INTEGER NOT NULL,
                arquivo TEXT NOT NULL, PRIMARY KEY (autorizador, recibo));
            CREATE TABLE IF NOT EXISTS documentos (interessado TEXT NOT NULL, nsu INTEGER NOT NULL, chave TEXT NOT NULL, schema TEXT NOT NULL,
                recebimento TEXT NOT NULL, arquivo TEXT NOT NULL, PRIMARY KEY (interessado, nsu));
            CREATE TABLE IF NOT EXISTS liberadas (chave TEXT PRIMARY KEY);
            CREATE TABLE IF NOT EXISTS consumos (documento TEXT PRIMARY KEY, ultimo_nsu INTEGER, nenhum TEXT, bloqueio TEXT, motivo TEXT NOT NULL,
                pontuais TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS csc (uf TEXT NOT NULL, cnpj TEXT NOT NULL, id TEXT NOT NULL, csc TEXT NOT NULL, revogacao TEXT,
                PRIMARY KEY (uf, cnpj, id));
            CREATE TABLE IF NOT EXISTS irregulares (documento TEXT PRIMARY KEY);
            CREATE TABLE IF NOT EXISTS cadastros (uf TEXT NOT NULL, ie TEXT NOT NULL, json TEXT NOT NULL, PRIMARY KEY (uf, ie));
            """);

    // O comando é sempre um texto fixo deste arquivo, com os valores em parâmetros.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Texto fixo; valores em parâmetros.")]
    private void Executar(string comando, params (string Nome, object? Valor)[] parametros)
    {
        if (_conexao is null)
            return;
        lock (_trava)
        {
            using var sql = _conexao.CreateCommand();
            sql.CommandText = comando;
            foreach (var (nome, valor) in parametros)
                sql.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
            sql.ExecuteNonQuery();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Texto fixo.")]
    private void Consultar(string comando, Action<SqliteDataReader> linha)
    {
        if (_conexao is null)
            return;
        lock (_trava)
        {
            using var sql = _conexao.CreateCommand();
            sql.CommandText = comando;
            using var leitor = sql.ExecuteReader();
            while (leitor.Read())
                linha(leitor);
        }
    }

    // Grava o XML como está (o espaço em branco faz parte do que a nota assina) e devolve o caminho relativo.
    private string Gravar(string relativo, XElement xml)
    {
        var caminho = Path.Combine(_xml, relativo);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        File.WriteAllText(caminho, xml.ToString(SaveOptions.DisableFormatting), Utf8);
        return relativo;
    }

    private XElement Ler(string relativo) =>
        XElement.Parse(File.ReadAllText(Path.Combine(_xml, relativo), Utf8), LoadOptions.PreserveWhitespace);

    private static string Texto(DateTimeOffset data) => data.ToString("o", CultureInfo.InvariantCulture);

    private static DateTimeOffset Data(string texto) => DateTimeOffset.Parse(texto, CultureInfo.InvariantCulture);
}
