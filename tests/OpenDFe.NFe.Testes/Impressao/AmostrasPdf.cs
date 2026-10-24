namespace OpenDFe.Testes;

/// <summary>
/// As amostras de inspeção visual do PDF (etapa 7.4): com <c>OPENDFE_AMOSTRAS=&lt;pasta&gt;</c> os testes gravam o PDF de cada
/// caso na pasta (fora do repositório), para abrir e conferir a olho contra o manual. Sem a variável, não grava nada.
/// </summary>
internal static class AmostrasPdf
{
    internal static void Gravar(string nome, byte[] pdf)
    {
        if (Environment.GetEnvironmentVariable("OPENDFE_AMOSTRAS") is not { Length: > 0 } pasta)
            return;
        Directory.CreateDirectory(pasta);
        File.WriteAllBytes(Path.Combine(pasta, nome + ".pdf"), pdf);
    }
}
