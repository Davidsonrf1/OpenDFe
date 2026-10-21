using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O CadConsultaCadastro4 (MOC 7.0, Visão Geral, 5.6): o ConsCad e o retConsCad, sobre os contribuintes da rota
// /_controle/cadastros. As regras, na ordem: a raiz (516, 404, 517, 239), o schema (215), a UF da consulta contra
// a do Web Service (K01, 265; o "SU" da SUFRAMA também, que nenhum autorizador do emulador consulta), a
// paralisação (108, 109), o solicitante (K02, 257: o transmissor irregular) e, pelo argumento, o CNPJ (K03, 258;
// K04, 259), a IE (K05, 260; K06, 261) ou o CPF (K08, 263; K09, 264). Achado, 111 com um infCad ou 112 com mais
// de um (5.6.5). Não há tpAmb no pedido: a consulta não tem o 252.
internal sealed class ConsultaCadastro : Servico
{
    private const string Versao = "2.00";

    public override string Nome => NomeConsultaCadastro;

    public override bool Atende(Autorizador autorizador) => autorizador.Cadastro.Count > 0;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "ConsCad", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Cadastro.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);
        var consulta = dados.Element(NamespaceNFe + "infCons")!;
        var uf = consulta.Element(NamespaceNFe + "UF")!.Value;
        var argumento = consulta.Elements().Last();
        var valor = argumento.Value;
        var codigo = Conferir(uf, argumento.Name.LocalName, valor, pedido, out var achados);
        return Retorno(codigo, pedido, uf, argumento.Name.LocalName, valor, achados);
    }

    // Antes de ler a consulta, a UF é a da casa do autorizador e o argumento vai como um CNPJ zerado (o schema
    // pede um dos três).
    public override XElement Rejeitar(string codigo, Pedido pedido) =>
        Retorno(codigo, pedido, UnidadesFederativas.Sigla(pedido.Autorizador.CodigoCasa)!, "CNPJ", "00000000000000", []);

    private static string Conferir(string uf, string tipo, string valor, Pedido pedido, out IReadOnlyList<Contribuinte> achados)
    {
        achados = [];
        if (UnidadesFederativas.CodigoOuNulo(uf) is not { } codigoUF || !pedido.Autorizador.Cadastro.Contains(codigoUF, StringComparer.Ordinal))
            return Motivos.UFConsultaDivergente;
        if (pedido.Paralisacao is { } paralisacao)
            return paralisacao.Codigo;
        if (pedido.Irregulares!.Contem(pedido.Transmissor))
            return Motivos.SolicitanteInabilitado;
        switch (tipo)
        {
            case "CNPJ":
                if (!Documentos.CnpjValido(valor))
                    return Motivos.CnpjConsultaInvalido;
                achados = pedido.Cadastros!.Procurar(uf, null, valor, null);
                if (achados.Count == 0)
                    return Motivos.CnpjNaoCadastrado;
                break;
            case "IE":
                if (!Cadastros.InscricaoValida(valor))
                    return Motivos.IeConsultaInvalida;
                achados = pedido.Cadastros!.Procurar(uf, valor, null, null);
                if (achados.Count == 0)
                    return Motivos.IeNaoCadastrada;
                break;
            default:
                if (!Documentos.CpfValido(valor))
                    return Motivos.CpfConsultaInvalido;
                achados = pedido.Cadastros!.Procurar(uf, null, null, valor);
                if (achados.Count == 0)
                    return Motivos.CpfNaoCadastrado;
                break;
        }
        return achados.Count == 1 ? Motivos.CadastroUmaOcorrencia : Motivos.CadastroOcorrencias;
    }

    // O retConsCad (Tabela 5-22), com o argumento da consulta como veio, o dhCons no horário de Brasília e o cUF da
    // casa do autorizador (GR06f: a UF que atendeu).
    private static XElement Retorno(string codigo, Pedido pedido, string uf, string tipo, string valor, IReadOnlyList<Contribuinte> achados) =>
        new(NamespaceNFe + "retConsCad",
            new XAttribute("versao", Versao),
            Elemento("infCons", new object?[]
            {
                Elemento("verAplic", VersaoAplicativo(pedido)),
                Elemento("cStat", codigo),
                Elemento("xMotivo", Motivos.Texto(codigo)),
                Elemento("UF", uf),
                Elemento(tipo, valor),
                Elemento("dhCons", DataHora(pedido.Recebimento)),
                Elemento("cUF", pedido.Autorizador.CodigoCasa),
                achados.Select(c => Informacoes(c, pedido.Irregulares!)),
            }));

    // O infCad (GR07 a GR29), na ordem do schema. O cSit é 0 (não habilitado) no contribuinte assim cadastrado e
    // no CNPJ ou CPF irregular.
    private static XElement Informacoes(Contribuinte contribuinte, Irregulares irregulares)
    {
        static string Data(DateOnly data) => data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var habilitado = contribuinte.Habilitado && !irregulares.Contem(contribuinte.Cnpj ?? contribuinte.Cpf);
        var endereco = contribuinte.Endereco;
        return Elemento("infCad", new object?[]
        {
            Elemento("IE", contribuinte.Ie),
            contribuinte.Cnpj is not null ? Elemento("CNPJ", contribuinte.Cnpj) : Elemento("CPF", contribuinte.Cpf),
            Elemento("UF", contribuinte.Uf),
            Elemento("cSit", habilitado ? "1" : "0"),
            Elemento("indCredNFe", contribuinte.CredenciadoNFe),
            Elemento("indCredCTe", contribuinte.CredenciadoCTe),
            Elemento("xNome", contribuinte.Nome),
            Opcional("xFant", contribuinte.Fantasia),
            Opcional("xRegApur", contribuinte.RegimeApuracao),
            Opcional("CNAE", contribuinte.Cnae),
            contribuinte.InicioAtividade is { } inicio ? Elemento("dIniAtiv", Data(inicio)) : null,
            contribuinte.UltimaSituacao is { } ultima ? Elemento("dUltSit", Data(ultima)) : null,
            contribuinte.Baixa is { } baixa ? Elemento("dBaixa", Data(baixa)) : null,
            Opcional("IEUnica", contribuinte.IeUnica),
            Opcional("IEAtual", contribuinte.IeAtual),
            endereco is null ? null : Elemento("ender", new object?[]
            {
                Opcional("xLgr", endereco.Logradouro),
                Opcional("nro", endereco.Numero),
                Opcional("xCpl", endereco.Complemento),
                Opcional("xBairro", endereco.Bairro),
                Opcional("cMun", endereco.CodigoMunicipio),
                Opcional("xMun", endereco.Municipio),
                Opcional("CEP", endereco.Cep),
            }),
        });
    }

    private static XElement? Opcional(string nome, string? valor) => valor is null ? null : Elemento(nome, valor);
}
