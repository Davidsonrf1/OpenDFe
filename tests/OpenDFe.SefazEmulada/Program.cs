using OpenDFe.SefazEmulada;

// dotnet run --project tests/OpenDFe.SefazEmulada [-- --porta 5443 --pasta <pasta> --endereco 127.0.0.1 --nome 127.0.0.1 --painel 5444 --pedidos 100 --controle local --persistir nao]
Console.OutputEncoding = System.Text.Encoding.UTF8;
var opcoes = OpcoesEmulador.Ler(args) with { Registrar = true };
await using var emulador = await Emulador.IniciarAsync(opcoes);

Console.WriteLine($"SEFAZ emulada em {emulador.Endereco} (só homologação, só para testes).");
Console.WriteLine($"A AC de teste, para confiar no servidor: {emulador.ArquivoAutoridade}");
Console.WriteLine($"As URLs: GET {emulador.Endereco}. O controle das falhas: {emulador.Endereco}_controle/falhas.");
if (emulador.Painel is { } painel)
    Console.WriteLine($"O painel, no navegador: {painel}");
Console.WriteLine("Ctrl+C para parar.");
await emulador.AguardarAsync();
