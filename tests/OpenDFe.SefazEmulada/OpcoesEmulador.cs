using System.Globalization;
using System.Net;

namespace OpenDFe.SefazEmulada;

// Como o emulador sobe. Pela linha de comando: --porta, --pasta, --endereco e --nome.
internal sealed record OpcoesEmulador
{
    public const int PortaPadrao = 5443;

    // Onde a AC de teste fica (ac.pfx e ac.crt). O padrão é fora do repositório, no perfil do usuário.
    public string Pasta { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenDFe.SefazEmulada");

    // O endereço em que o Kestrel escuta: só o próprio computador, por padrão.
    public IPAddress Endereco { get; init; } = IPAddress.Loopback;

    // Zero: uma porta livre (a dos testes).
    public int Porta { get; init; } = PortaPadrao;

    // O nome do servidor nas URLs listadas, e também no certificado. O padrão é o IP, e não "localhost":
    // no Windows, o "localhost" tenta o ::1 antes, onde o emulador não escuta.
    public string Nome { get; init; } = "127.0.0.1";

    public TimeProvider Relogio { get; init; } = TimeProvider.System;

    // Registra cada pedido no console (o dotnet run); os testes sobem calados.
    public bool Registrar { get; init; }

    public static OpcoesEmulador Ler(IReadOnlyList<string> argumentos)
    {
        ArgumentNullException.ThrowIfNull(argumentos);
        var opcoes = new OpcoesEmulador();
        for (var i = 0; i < argumentos.Count; i++)
        {
            var valor = i + 1 < argumentos.Count ? argumentos[i + 1] : throw new ArgumentException($"Falta o valor de {argumentos[i]}.");
            opcoes = argumentos[i] switch
            {
                "--porta" => opcoes with { Porta = int.Parse(valor, CultureInfo.InvariantCulture) },
                "--pasta" => opcoes with { Pasta = Path.GetFullPath(valor) },
                "--endereco" => opcoes with { Endereco = IPAddress.Parse(valor) },
                "--nome" => opcoes with { Nome = valor },
                _ => throw new ArgumentException($"Opção desconhecida: {argumentos[i]}. As opções: --porta, --pasta, --endereco, --nome."),
            };
            i++;
        }
        return opcoes;
    }
}
