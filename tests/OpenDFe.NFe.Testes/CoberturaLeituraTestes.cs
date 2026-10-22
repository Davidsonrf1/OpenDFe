using System.Reflection;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A cobertura da leitura (docs/LEITURA.md 8), espelho do OLeiauteEscreveTodoCampoDoModelo: as notas preenchidas por
// reflexão (um valor em cada campo, uma rodada por subclasse de escolha) escritas e lidas de volta, campo a campo; toda
// propriedade com [Tag] do modelo volta com o valor, salvo as que a escrita tira do núcleo.
public class CoberturaLeituraTestes
{
    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    // Os campos do núcleo: a nota preenchida por reflexão diverge da configuração (NFE-NUCLEO), e sai o valor do núcleo.
    private static readonly string[] Nucleo =
    [
        "Identificacao.UF", "Identificacao.Modelo", "Identificacao.TipoEmissao", "Identificacao.Ambiente",
        "Identificacao.InicioContingencia", "Identificacao.JustificativaContingencia",
    ];

    [Fact]
    public void OLeiauteLeTodoCampoDoModelo()
    {
        var vistas = new HashSet<(Type, string)>();
        var rodadas = Modelo.GetTypes().Where(t => t.IsAbstract && t.IsClass && t.Namespace == "OpenDFe.NFe")
            .Max(b => Modelo.GetTypes().Count(t => t.IsSubclassOf(b) && !t.IsAbstract));
        for (var rodada = 0; rodada < rodadas; rodada++)
        {
            var nota = (NotaFiscal)CoberturaEscritaTestes.Preencher(typeof(NotaFiscal), rodada);
            var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFe, TipoEmissao.ContingenciaSvcAN,
                new Contingencia(NotasTeste.Emissao, "SEFAZ FORA DO AR DESDE AS 14H")));

            var lida = new LeiauteNFe400().Ler(escrita.Xml);

            // O emitente "X" não dá chave; fora isso, a leitura não acha nada estranho no que a escrita escreveu.
            Assert.All(lida.Problemas, p => Assert.Equal("NFE-CHAVE", p.Codigo));
            var diferencas = ComparacaoModelo.Diferencas(nota, lida.Nota!, vistas, caminho => Nucleo.Contains(caminho));
            Assert.True(diferencas.Count == 0, $"Rodada {rodada}: " + string.Join(Environment.NewLine, diferencas));
        }

        var nucleo = Nucleo.Select(c => c.Split('.')[1]).ToHashSet();
        var faltam = Modelo.GetTypes()
            .Where(t => t.IsClass && t.IsPublic && t != typeof(Protocolo) && t != typeof(MensagemSefaz))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(p => p.GetCustomAttribute<TagAttribute>() is not null)
            .Where(p => !vistas.Contains((p.DeclaringType!, p.Name)))
            .Where(p => !(p.DeclaringType == typeof(Identificacao) && nucleo.Contains(p.Name)))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();
        Assert.True(faltam.Count == 0, "Campos do modelo que a leitura não traz de volta: " + string.Join(", ", faltam));
    }
}
