using System.Runtime.CompilerServices;
using System.Text;

namespace OpenDFe.Testes;

/// <summary>
/// Os golden files de texto do DANFCE (<c>Impressao/Esperado/*.txt</c>, versionados): compara o texto renderizado com o
/// arquivo e, quando difere, mostra o texto novo para a revisão. Para gerar ou regerar os arquivos, rode os testes com a
/// variável <c>OPENDFE_GERAR_GOLDEN=1</c> e <b>confira cada arquivo linha a linha contra o manual</b> antes de commitar.
/// </summary>
internal static class ArquivoEsperado
{
    internal static void Conferir(string nome, string atual, [CallerFilePath] string origem = "")
    {
        var arquivo = Path.Combine(Path.GetDirectoryName(origem)!, "Esperado", nome + ".txt");
        if (Environment.GetEnvironmentVariable("OPENDFE_GERAR_GOLDEN") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(arquivo)!);
            File.WriteAllText(arquivo, atual, new UTF8Encoding(false));
            return;
        }

        Assert.True(File.Exists(arquivo), $"O golden {nome}.txt não existe. O texto renderizado:{Environment.NewLine}{atual}");
        var esperado = File.ReadAllText(arquivo, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.True(esperado == atual,
            $"O DANFCE {nome} difere do golden. O texto novo (confira contra o manual antes de aceitar):{Environment.NewLine}{atual}");
    }
}
