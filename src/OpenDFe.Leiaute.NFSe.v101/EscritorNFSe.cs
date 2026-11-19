using System.Reflection;
using System.Text;
using System.Xml;

namespace OpenDFe.NFSe;

/// <summary>
/// A escrita da DPS e do pedido de registro de evento no leiaute 1.01 (docs/NFSE.md 7), pelo mapa do modelo
/// (<see cref="MapasNFSe"/>), na ordem do XSD.
/// </summary>
/// <remarks>
/// <para>
/// O XML sai sem prefixo, sem declaração, sem espaço entre as tags e sem a <c>Signature</c> (Manual Integrado 6.1.1); a tag
/// de valor nulo (e de texto vazio) não sai. O modelo não é conferido aqui: o valor errado sai como está, e o schema aponta
/// (MODELO.md 3, regra 4).
/// </para>
/// <para>
/// A escrita não altera o objeto: o ambiente nulo vale o do núcleo, a versão do aplicativo nula vale a padrão (M4), e o Id
/// vai para o resultado. Sem estado e seguro em paralelo.
/// </para>
/// </remarks>
internal static class EscritorNFSe
{
    /// <summary>A versão que o leiaute escreve.</summary>
    internal const string Versao = "1.01";

    private const int TamanhoVersaoAplicativo = 20;

    private static readonly XmlWriterSettings Configuracao = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    /// <summary>A versão do aplicativo quando o objeto não a traz: "OpenDFe " e a versão do pacote (M4).</summary>
    internal static string VersaoPadrao { get; } = Cortar("OpenDFe " + VersaoPacote());

    /// <summary>
    /// Escreve a DPS. <paramref name="ambienteNucleo"/>: o ambiente da configuração; nulo (a escrita sem núcleo,
    /// <see cref="LeiauteNFSe101.Escrever"/>), vale o da DPS.
    /// </summary>
    internal static DpsEscrita Escrever(Dps dps, Ambiente? ambienteNucleo)
    {
        ArgumentNullException.ThrowIfNull(dps);
        var texto = new StringBuilder();
        EscritaNFSe e;
        IdentificadorDps? id;
        using (var xml = XmlWriter.Create(texto, Configuracao))
        {
            e = new EscritaNFSe(xml);
            e.Abrir("DPS", null);
            e.Atributo("versao", Versao);
            e.Abrir("infDPS", null);
            id = MontarId(dps, e);
            if (id is { } identificador)
                e.Atributo("Id", identificador.Valor);
            EscritaMapa.Escrever(e, MapasNFSe.MapaDps, dps, c => Nucleo(e, c, dps.Ambiente, dps.VersaoAplicativo, ambienteNucleo));
            e.Fechar();
            e.Fechar();
        }
        return new DpsEscrita(texto.ToString(), id, e.Problemas.ToArray());
    }

    /// <summary>Escreve o pedido de registro do evento (<c>pedRegEvento</c>), com o Id e o texto fixo do tipo.</summary>
    internal static PedidoEscrito EscreverPedido(EventoNFSe evento, Ambiente? ambienteNucleo)
    {
        ArgumentNullException.ThrowIfNull(evento);
        var tipo = MapasNFSe.Eventos.First(t => t.Classe == evento.GetType());
        var texto = new StringBuilder();
        EscritaNFSe e;
        string? id = null;
        using (var xml = XmlWriter.Create(texto, Configuracao))
        {
            e = new EscritaNFSe(xml);
            e.Abrir("pedRegEvento", null);
            e.Atributo("versao", Versao);
            e.Abrir("infPedReg", null);
            if (ChaveNFSe.TentarLer(evento.Chave, out var chave))
            {
                id = "PRE" + chave.Valor + tipo.Tipo;
                e.Atributo("Id", id);
            }
            else
            {
                e.Registrar(CodigosNFSe.Id, Severidade.Erro,
                    "A chave da NFS-e do evento não é válida (50 dígitos). Sem ela, o Id do pedido não é montado.", "chNFSe", "Chave");
            }
            EscritaMapa.Escrever(e, MapasNFSe.MapaEventoNFSe, evento, c => Nucleo(e, c, evento.Ambiente, evento.VersaoAplicativo, ambienteNucleo));
            e.Abrir(tipo.Tag, null);
            e.Valor("xDesc", tipo.Descricao101);
            EscritaMapa.Escrever(e, tipo.Mapa(), evento);
            e.Fechar();
            e.Fechar();
            e.Fechar();
        }
        return new PedidoEscrito(texto.ToString(), id, e.Problemas.ToArray());
    }

    // O tpAmb e o verAplic (M4): nulo, o do núcleo e o padrão; o ambiente diferente do núcleo é erro, e sai o do núcleo.
    private static bool Nucleo(EscritaNFSe e, CampoMapa campo, Ambiente? ambiente, string? versao, Ambiente? ambienteNucleo)
    {
        switch (campo.Tag)
        {
            case "tpAmb":
                if (ambienteNucleo is { } nucleo && ambiente is { } proprio && proprio != nucleo)
                    e.Registrar(CodigosNFSe.Nucleo, Severidade.Erro,
                        "O campo tpAmb difere do ambiente da configuração do núcleo: deixe-o nulo para usar o do núcleo.", "tpAmb", "Ambiente");
                if ((ambienteNucleo ?? ambiente) is { } valor)
                    e.Valor("tpAmb", ((int)valor).ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            case "verAplic":
                e.Texto("verAplic", "VersaoAplicativo", string.IsNullOrEmpty(versao) ? VersaoPadrao : versao);
                return true;
            default:
                return false;
        }
    }

    // O Id (Anexo I, linha 101): o município, a inscrição de quem emite a DPS (o prestador, o tomador ou o intermediário,
    // pelo tpEmit), a série e o número. O dado que falta é problema, e a DPS sai sem Id.
    private static IdentificadorDps? MontarId(Dps dps, EscritaNFSe e)
    {
        var (documento, propriedade) = dps.Emitente switch
        {
            EmitenteDps.Tomador => (Documento(dps.Tomador?.Cnpj, dps.Tomador?.Cpf), "Tomador"),
            EmitenteDps.Intermediario => (Documento(dps.Intermediario?.Cnpj, dps.Intermediario?.Cpf), "Intermediario"),
            _ => (Documento(dps.Prestador?.Cnpj, dps.Prestador?.Cpf), "Prestador"),
        };
        string? falta = null;
        string? tag = null;
        string? onde = null;
        if (dps.MunicipioEmissor is null)
            (falta, tag, onde) = ("o município emissor", "cLocEmi", "MunicipioEmissor");
        else if (documento is null)
            (falta, tag, onde) = ($"o CNPJ ou o CPF de quem emite a DPS ({propriedade})", null, propriedade + ".Cnpj");
        else if (string.IsNullOrEmpty(dps.Serie))
            (falta, tag, onde) = ("a série", "serie", "Serie");
        else if (dps.Numero is null)
            (falta, tag, onde) = ("o número", "nDPS", "Numero");
        if (falta is not null)
        {
            e.Registrar(CodigosNFSe.Id, Severidade.Erro, $"Falta {falta}. Sem ele, o Id da DPS não é montado.", tag, onde);
            return null;
        }
        try
        {
            return IdentificadorDps.Montar(dps.MunicipioEmissor!.Value, documento!, dps.Serie!, dps.Numero!.Value);
        }
        catch (ArgumentException erro)
        {
            (var mensagem, tag, onde) = erro.ParamName switch
            {
                "municipioEmissor" => ("O município emissor tem 7 dígitos.", "cLocEmi", "MunicipioEmissor"),
                "serie" => ("A série tem de 1 a 5 dígitos.", "serie", "Serie"),
                "numero" => ("O número vai de 1 a 999.999.999.999.999.", "nDPS", "Numero"),
                _ => ($"O CNPJ ou o CPF de quem emite a DPS ({propriedade}) não tem 14 nem 11 dígitos.", null, propriedade + ".Cnpj"),
            };
            e.Registrar(CodigosNFSe.Id, Severidade.Erro, mensagem + " Sem isso, o Id da DPS não é montado.", tag, onde);
            return null;
        }
    }

    private static string? Documento(string? cnpj, string? cpf) =>
        !string.IsNullOrEmpty(cnpj) ? cnpj : !string.IsNullOrEmpty(cpf) ? cpf : null;

    private static string Cortar(string valor) => valor.Length > TamanhoVersaoAplicativo ? valor[..TamanhoVersaoAplicativo] : valor;

    // A versão do pacote (a informacional sem o "+<commit>"; a do assembly, se não houver).
    private static string VersaoPacote()
    {
        var assembly = typeof(EscritorNFSe).Assembly;
        var informacional = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrEmpty(informacional))
            return informacional.Split('+')[0];
        return assembly.GetName().Version?.ToString(3) ?? "0";
    }
}

/// <summary>O pedido de registro de evento escrito: o XML sem a assinatura, o Id e os problemas da montagem.</summary>
internal sealed record PedidoEscrito(string Xml, string? Id, IReadOnlyList<Problema> Problemas);

/// <summary>Um tipo de evento do XSD: o código, a tag do detalhe, a classe do modelo, o mapa e o texto fixo do <c>xDesc</c>.</summary>
internal sealed record TipoEventoNFSe(string Tipo, string Tag, Type Classe, Func<EventoNFSe> Criar, Func<MapaGrupo> Mapa,
    string Descricao101, string Descricao100, bool Emitente);
