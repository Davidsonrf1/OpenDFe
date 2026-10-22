using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O NFeInutilizacao4 (MOC 7.0, Visão Geral, 5.3): o inutNFe e o retInutNFe. As regras, na ordem: a raiz (516,
// 404, 517, 239), o schema (215), a assinatura e o titular (grupos E e F), e as da Tabela 5-12: o ambiente (I01,
// 252), a UF (I02, 250), a série do emitente CPF (I02a) e a reservada ao PAA (I02d, NT 2026.001), as duas 266, o
// ano (I02b, 453; I02c, 454), a faixa invertida (I03, 224), o limite de 10.000 números (I04, 201) e o Id (I04.a,
// 502); a paralisação (108, 109); e, na base, o emitente irregular (I06, 240), a mesma faixa (I07, 563, com o
// protocolo da anterior), o número já inutilizado (I07a, 256) e o número já usado por uma nota (I08, 241).
// Passando tudo, 102 com o protocolo. A SVC não oferece a inutilização (Anexo III, 2.1.3, e).
internal sealed class Inutilizacao : Servico
{
    private const string Versao = "4.00";

    // I04: o máximo de números num pedido.
    private const int LimiteNumeros = 10_000;

    public override string Nome => NomeInutilizacao;

    public override bool Atende(Autorizador autorizador) => autorizador.TipoEmissao is null;

    public override XElement Processar(XElement dados, Pedido pedido)
    {
        if (ConferirRaiz(dados, pedido, "inutNFe", Versao) is { } rejeicao)
            return Rejeitar(rejeicao, pedido);
        if (SchemasLeiaute.Inutilizacao.Validar(Texto(dados)).Count > 0)
            return Rejeitar(Motivos.FalhaSchema, pedido);

        var informacao = dados.Element(NamespaceNFe + "infInut")!;
        string Campo(string nome) => informacao.Element(NamespaceNFe + nome)!.Value;
        var id = informacao.Attribute("Id")!.Value;
        var (uf, ano, cnpj, modelo) = (Campo("cUF"), Campo("ano"), Campo("CNPJ"), Campo("mod"));
        var serie = int.Parse(Campo("serie"), CultureInfo.InvariantCulture);
        var inicio = long.Parse(Campo("nNFIni"), CultureInfo.InvariantCulture);
        var fim = long.Parse(Campo("nNFFin"), CultureInfo.InvariantCulture);

        // O CNPJ com o DV errado não tem regra própria na Tabela 5-12: não está no cadastro (I05, 203).
        if (!Documentos.CnpjValido(cnpj))
            return Rejeitar(Motivos.EmissorInabilitado, pedido);
        var assinatura = AssinaturaDigital.Verificar(Texto(dados), id, cnpj);
        if (assinatura.Count > 0)
            return Rejeitar(assinatura[0].Codigo, pedido);

        if (Campo("tpAmb") != "2")
            return Rejeitar(Motivos.AmbienteDivergente, pedido);
        if (!pedido.Autorizador.Atende(uf))
            return Rejeitar(Motivos.OrgaoDivergente, pedido);
        if (serie is >= 910 and <= 989)
            return Rejeitar(Motivos.SerieNaoPermitida, pedido);
        var anoAtual = pedido.Recebimento.ToOffset(TimeSpan.FromHours(-3)).Year % 100;
        var anoPedido = int.Parse(ano, CultureInfo.InvariantCulture);
        if (anoPedido > anoAtual)
            return Rejeitar(Motivos.AnoPosterior, pedido);
        if (anoPedido < 6)
            return Rejeitar(Motivos.AnoAnterior, pedido);
        if (inicio > fim)
            return Rejeitar(Motivos.FaixaInvertida, pedido);
        if (fim - inicio + 1 > LimiteNumeros)
            return Rejeitar(Motivos.InutilizacaoLimite, pedido);
        if (id != "ID" + uf + ano + cnpj + modelo + serie.ToString("D3", CultureInfo.InvariantCulture)
            + inicio.ToString("D9", CultureInfo.InvariantCulture) + fim.ToString("D9", CultureInfo.InvariantCulture))
            return Rejeitar(Motivos.ChaveDivergenteCampos, pedido);
        if (pedido.Paralisacao is { } paralisacao)
            return Rejeitar(paralisacao.Codigo, pedido);

        var faixa = new FaixaInutilizada(uf, cnpj, modelo, serie, inicio, fim, "", pedido.Recebimento, pedido.Autorizador.Sigla);
        if (pedido.Irregulares!.Contem(cnpj))
            return Rejeitar(Motivos.EmitenteIrregular, pedido);
        var repositorio = pedido.Repositorio!;
        lock (repositorio.Trava)
        {
            if (repositorio.Faixa(uf, cnpj, modelo, serie, inicio, fim) is { } anterior)
                return Retorno(Motivos.InutilizacaoRepetida, pedido, ano, faixa with { Protocolo = anterior.Protocolo });
            if (repositorio.Inutilizado(uf, cnpj, modelo, serie, inicio, fim))
                return Rejeitar(Motivos.FaixaInutilizada, pedido);
            if (repositorio.Utilizado(uf, cnpj, modelo, serie, inicio, fim))
                return Rejeitar(Motivos.NumeroUtilizado, pedido);
            faixa = faixa with { Protocolo = repositorio.NovoProtocolo(pedido.Autorizador, uf, pedido.Recebimento) };
            repositorio.Registrar(faixa);
            return Retorno(Motivos.Inutilizado, pedido, ano, faixa);
        }
    }

    public override XElement Rejeitar(string codigo, Pedido pedido) => Retorno(codigo, pedido, null, null);

    // O retInutNFe (Tabela 5-10), com o cUF da casa do autorizador e sem o Id (a resposta não é assinada). A
    // faixa e o protocolo vão no 102 e no 563 (com o protocolo da faixa já inutilizada, 5.3.5); o dhRecbto,
    // sempre.
    private static XElement Retorno(string codigo, Pedido pedido, string? ano, FaixaInutilizada? faixa) =>
        new(NamespaceNFe + "retInutNFe",
            new XAttribute("versao", Versao),
            Elemento("infInut", new object?[]
            {
                Elemento("tpAmb", "2"),
                Elemento("verAplic", VersaoAplicativo(pedido)),
                Elemento("cStat", codigo),
                Elemento("xMotivo", Motivos.Texto(codigo)),
                Elemento("cUF", pedido.Autorizador.CodigoCasa),
                faixa is null ? null : new object[]
                {
                    Elemento("ano", ano),
                    Elemento("CNPJ", faixa.Cnpj),
                    Elemento("mod", faixa.Modelo),
                    Elemento("serie", faixa.Serie.ToString(CultureInfo.InvariantCulture)),
                    Elemento("nNFIni", faixa.Inicio.ToString(CultureInfo.InvariantCulture)),
                    Elemento("nNFFin", faixa.Fim.ToString(CultureInfo.InvariantCulture)),
                },
                Elemento("dhRecbto", DataHora(pedido.Recebimento)),
                faixa is null ? null : Elemento("nProt", faixa.Protocolo),
            }));
}
