using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A chave de acesso da NFS-e (50 dígitos) e o identificador da DPS (45 posições), Anexo I, linhas 4 e 101 do leiaute.
public class ChaveIdentificadorTestes
{
    // A chave de exemplo do guia do emissor público nacional web 1.2.1 (produção restrita, 9.1).
    private const string ChaveGuia = "31062001251235800000112230000000173023019580208160";

    [Fact]
    public void AChaveDoGuiaOficialELidaComAsPartes()
    {
        var chave = ChaveNFSe.Ler(ChaveGuia);

        Assert.Equal(3106200, chave.Municipio);
        Assert.Equal(AmbienteGerador.Prefeitura, chave.AmbienteGerador);
        Assert.False(chave.InscricaoCpf);
        Assert.Equal("51235800000112", chave.Inscricao);
        Assert.Equal(2300000001730, chave.Numero);
        Assert.Equal((2023, 1), (chave.Ano, chave.Mes));
        Assert.Equal(958020816, chave.CodigoNumerico);
        Assert.Equal(0, chave.DigitoVerificador);
        Assert.Equal(ChaveGuia, chave.ToString());
    }

    [Fact]
    public void AChaveComCPFDevolveOCPFSemOsZeros()
    {
        var chave = ChaveNFSe.Ler("3106200" + "2" + "1" + "000" + DpsTeste.Cpf + "0000000000001" + "2611" + "123456789" + "5");

        Assert.True(chave.InscricaoCpf);
        Assert.Equal(DpsTeste.Cpf, chave.Inscricao);
    }

    // O DV não é conferido (docs/NFSE.md 4, a NFSE-CHAVE caiu): qualquer dígito no fim serve.
    [Theory]
    [InlineData('0')]
    [InlineData('9')]
    public void ODigitoVerificadorNaoEConferido(char digito)
    {
        Assert.True(ChaveNFSe.TentarLer(ChaveGuia[..49] + digito, out _));
    }

    [Theory]
    [InlineData("3106200125123580000011223000000017302301958020816")]      // 49
    [InlineData("310620012512358000001122300000001730230195802081600")]    // 51
    [InlineData("31062001251235800000112230000000173023019580208A60")]     // letra
    [InlineData("31062003251235800000112230000000173023019580208160")]     // ambiente 3
    [InlineData("31062001351235800000112230000000173023019580208160")]     // inscrição tipo 3
    [InlineData("31062001151235800000112230000000173023019580208160")]     // CPF sem 000
    [InlineData("31062001251235800000112230000000173023139580208160")]     // mês 13
    [InlineData("3106200" + "1" + "2" + "51235800000112" + "0000000000000" + "2301" + "958020816" + "0")] // número zero
    public void AChaveQueNaoServeLancaFormatException(string chave)
    {
        Assert.Throws<FormatException>(() => ChaveNFSe.Ler(chave));
        Assert.False(ChaveNFSe.TentarLer(chave, out var resultado));
        Assert.Equal("", resultado.Valor);
    }

    [Fact]
    public void AMensagemDaChaveInvalidaNaoTrazOValor()
    {
        var erro = Assert.Throws<FormatException>(() => ChaveNFSe.Ler("31062001151235800000112230000000173023019580208160"));

        Assert.DoesNotContain("5123580000011", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OIdentificadorEMontadoComOsZeros()
    {
        var id = IdentificadorDps.Montar(3106200, DpsTeste.CnpjPrestador, "1", 42);

        Assert.Equal("DPS3106200" + "2" + DpsTeste.CnpjPrestador + "00001" + "000000000000042", id.Valor);
        Assert.Equal((3106200, false, DpsTeste.CnpjPrestador, "00001", 42L), (id.Municipio, id.InscricaoCpf, id.Inscricao, id.Serie, id.Numero));
        Assert.Equal(id, IdentificadorDps.Ler(id.Valor));
    }

    [Fact]
    public void OIdentificadorComCPFLevaO000()
    {
        var id = IdentificadorDps.Montar(3106200, DpsTeste.Cpf, "49999", 999_999_999_999_999);

        Assert.Equal("DPS3106200" + "1" + "000" + DpsTeste.Cpf + "49999" + "999999999999999", id.Valor);
        Assert.Equal(DpsTeste.Cpf, id.Inscricao);
    }

    [Theory]
    [InlineData(999_999, DpsTeste.Cpf, "1", 1L, "municipioEmissor")]
    [InlineData(3106200, "123", "1", 1L, "cpfCnpj")]
    [InlineData(3106200, "1122233300018A", "1", 1L, "cpfCnpj")]
    [InlineData(3106200, DpsTeste.Cpf, "", 1L, "serie")]
    [InlineData(3106200, DpsTeste.Cpf, "123456", 1L, "serie")]
    [InlineData(3106200, DpsTeste.Cpf, "1", 0L, "numero")]
    [InlineData(3106200, DpsTeste.Cpf, "1", 1_000_000_000_000_000L, "numero")]
    public void OIdentificadorComCampoForaDaFaixaLanca(int municipio, string documento, string serie, long numero, string parametro)
    {
        var erro = Assert.ThrowsAny<ArgumentException>(() => IdentificadorDps.Montar(municipio, documento, serie, numero));

        Assert.Equal(parametro, erro.ParamName);
    }

    [Theory]
    [InlineData("DPS310620021122233300018100001000000000000042X")]
    [InlineData("XPS310620021122233300018100001000000000000042")]
    [InlineData("DPS310620031122233300018100001000000000000042")]
    [InlineData("DPS310620011122233300018100001000000000000042")]
    [InlineData("DPS310620021122233300018100001000000000000000")]
    public void OIdentificadorQueNaoServeLancaFormatException(string id)
    {
        Assert.Throws<FormatException>(() => IdentificadorDps.Ler(id));
        Assert.False(IdentificadorDps.TentarLer(id, out _));
    }
}
