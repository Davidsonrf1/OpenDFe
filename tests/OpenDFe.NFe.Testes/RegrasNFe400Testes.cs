using System.Reflection;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// As regras do leiaute (REGRAS.md 4): a estrutura da 3.1, com regras de teste; o lote da 3.2 está em RegrasLoteTestes.
public class RegrasNFe400Testes
{
    private static ContextoRegra Contexto(Documento documento = Documento.NFe) =>
        new(NotasTeste.Nucleo(documento), null, NotasTeste.Emissao);

    private static RegraNFe Regra(
        Func<NotaFiscal, ContextoRegra, IEnumerable<OcorrenciaRegra>> conferir,
        AplicacaoRegra aplicacao = AplicacaoRegra.Obrigatoria,
        Documento[]? modelos = null,
        string codigo = "564") =>
        new("T01-10", codigo, modelos ?? [Documento.NFe, Documento.NFCe], aplicacao, "Mensagem da regra", conferir);

    [Fact]
    public void OLeiauteDeclaraAsRegrasNosDoisDocumentos()
    {
        Assert.NotEmpty(RegrasNFe400.Lista);
        var regras = typeof(DeclaracaoLeiaute).GetProperty("Regras", BindingFlags.NonPublic | BindingFlags.Instance)!;
        foreach (var documento in (Documento[])[Documento.NFe, Documento.NFCe])
        {
            var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);
            Assert.Single((System.Collections.IEnumerable)regras.GetValue(declaracao)!);
        }
    }

    [Fact]
    public void OProblemaSaiComOCodigoAMensagemOCaminhoEAPropriedade()
    {
        var regra = Regra((_, _) => [new OcorrenciaRegra("NFe/infNFe/total/ICMSTot/vNF", "vNF 10,00; soma dos itens 9,00")]);

        var problema = Assert.Single(RegrasNFe400.Conferir([regra], NotasTeste.NFe(), "", Contexto()));

        Assert.Equal("564", problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("Mensagem da regra (vNF 10,00; soma dos itens 9,00)", problema.Mensagem);
        Assert.Equal("vNF", problema.Tag);
        Assert.Equal("NFe/infNFe/total/ICMSTot/vNF", problema.Caminho);
        Assert.Equal("Total.Icms.ValorNota", problema.Propriedade);
        Assert.Null(problema.Schema);
    }

    [Fact]
    public void ATagSaiDoUltimoPassoEAPropriedadeDaRegraPrevalece()
    {
        var regra = Regra((_, _) =>
        [
            new OcorrenciaRegra("NFe/infNFe/det[3]/prod/vProd"),
            new OcorrenciaRegra("NFe/infNFe/det[2]/@nItem", Propriedade: "Itens[1]"),
            new OcorrenciaRegra(null, "sem campo"),
        ]);

        var problemas = RegrasNFe400.Conferir([regra], NotasTeste.NFe(), "", Contexto());

        Assert.Equal(
            [("vProd", "Itens[2].Produto.Valor", "Mensagem da regra"), ("nItem", "Itens[1]", "Mensagem da regra"), (null, null, "Mensagem da regra (sem campo)")],
            problemas.Select(p => (p.Tag, p.Propriedade, p.Mensagem)));
        Assert.Null(problemas[2].Caminho);
    }

    [Fact]
    public void AFacultativaSaiComoAviso()
    {
        var regra = Regra((_, _) => [new OcorrenciaRegra(null)], AplicacaoRegra.Facultativa);

        Assert.Equal(Severidade.Aviso, Assert.Single(RegrasNFe400.Conferir([regra], NotasTeste.NFe(), "", Contexto())).Severidade);
    }

    [Fact]
    public void SoRodamAsRegrasDoModeloDoNucleoNaOrdemDaLista()
    {
        var chamadas = new List<string>();
        RegraNFe Marcar(string nome, params Documento[] modelos) =>
            Regra((_, _) => { chamadas.Add(nome); return []; }, modelos: modelos);
        RegraNFe[] regras = [Marcar("as duas", Documento.NFe, Documento.NFCe), Marcar("NFC-e", Documento.NFCe), Marcar("NF-e", Documento.NFe)];

        RegrasNFe400.Conferir(regras, NotasTeste.NFe(), "", Contexto(Documento.NFe));
        RegrasNFe400.Conferir(regras, NotasTeste.NFCe(), "", Contexto(Documento.NFCe));

        Assert.Equal(["as duas", "NF-e", "as duas", "NFC-e"], chamadas);
    }

    [Fact]
    public void ARegraRecebeANotaEOContexto()
    {
        var nota = NotasTeste.NFe();
        var contexto = Contexto();
        (NotaFiscal Nota, ContextoRegra Contexto)? recebido = null;

        RegrasNFe400.Conferir([Regra((n, c) => { recebido = (n, c); return []; })], nota, "", contexto);

        Assert.Same(nota, recebido!.Value.Nota);
        Assert.Same(contexto, recebido.Value.Contexto);
        Assert.Equal(NotasTeste.Emissao, contexto.Agora);
    }

    [Fact]
    public void ComOModeloNuloAsRegrasLeemOXml()
    {
        var xml = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFe()));
        NotaFiscal? lida = null;

        RegrasNFe400.Conferir([Regra((n, _) => { lida = n; return []; })], null, xml, Contexto());

        Assert.NotNull(lida);
        Assert.Equal(NotasTeste.CnpjEmitente, lida!.Emitente!.Cnpj);
        Assert.Empty(RegrasNFe400.Conferir([Regra((_, _) => [new OcorrenciaRegra(null)])], null, "<outro/>", Contexto()));
    }

    [Fact]
    public void OModeloDeOutroTipoEErroDeProgramacao()
    {
        Assert.Throws<InvalidOperationException>(() => RegrasNFe400.Conferir([], "nota", "", Contexto()));
    }
}
