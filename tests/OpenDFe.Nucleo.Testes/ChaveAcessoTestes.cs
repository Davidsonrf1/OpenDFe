namespace OpenDFe.Testes;

public class ChaveAcessoTestes
{
    private static readonly DateTimeOffset Outubro2026 = new(2026, 10, 19, 14, 30, 0, TimeSpan.FromHours(-3));

    // Chaves de exemplo dos manuais oficiais, com o DV que o manual mostra:
    // - MOC 7.0, Visão Geral, 2.2.10.3 (exemplo do hashCSRT): NF-e do PR, série 890 (NFA-e);
    // - MOC 7.0, Anexo I, campo ZX02 (exemplo da URL do QR Code): NFC-e do RS.
    public static TheoryData<string, UF, int, int, string, Documento, int, int, TipoEmissao, int, int> Oficiais => new()
    {
        {
            "41180678393592000146558900000006041028190697",
            UF.PR, 2018, 6, "78393592000146", Documento.NFe, 890, 604, TipoEmissao.Normal, 02819069, 7
        },
        {
            "43150108287693000157651010000000971000001251",
            UF.RS, 2015, 1, "08287693000157", Documento.NFCe, 101, 97, TipoEmissao.Normal, 125, 1
        },
    };

    [Theory]
    [MemberData(nameof(Oficiais))]
    public void LeAsPartesDasChavesDosManuais(string valor, UF uf, int ano, int mes, string emitente,
        Documento modelo, int serie, int numero, TipoEmissao tipoEmissao, int codigoNumerico, int digito)
    {
        var chave = ChaveAcesso.Ler(valor);

        Assert.Equal(uf, chave.UF);
        Assert.Equal(ano, chave.Ano);
        Assert.Equal(mes, chave.Mes);
        Assert.Equal(emitente, chave.Emitente);
        Assert.Equal(modelo, chave.Modelo);
        Assert.Equal(serie, chave.Serie);
        Assert.Equal(numero, chave.Numero);
        Assert.Equal(tipoEmissao, chave.TipoEmissao);
        Assert.Equal(codigoNumerico, chave.CodigoNumerico);
        Assert.Equal(digito, chave.DigitoVerificador);
        Assert.Equal(valor, chave.ToString());
    }

    [Theory]
    [MemberData(nameof(Oficiais))]
    public void MontaAsChavesDosManuais(string valor, UF uf, int ano, int mes, string emitente,
        Documento modelo, int serie, int numero, TipoEmissao tipoEmissao, int codigoNumerico, int digito)
    {
        _ = digito;
        var emissao = new DateTimeOffset(ano, mes, 1, 0, 0, 0, TimeSpan.FromHours(-3));
        var chave = ChaveAcesso.Montar(uf, emissao, emitente, modelo, serie, numero, tipoEmissao, codigoNumerico);
        Assert.Equal(valor, chave.ToString());
    }

    [Fact]
    public void DigitoDoExemploDoManual()
    {
        // MOC 7.0, Visão Geral, 2.2.6.2: as 43 posições do exemplo têm soma ponderada 644, resto 6, DV 5.
        // A chave é do leiaute 1.10 (cNF de 9 dígitos, sem tipo de emissão), por isso o teste é do cálculo.
        Assert.Equal(5, Modulo11.Calcular("5206043300991100250655012000000780026730161"));
    }

    [Fact]
    public void ChaveDoLeiauteAnteriorAo200EhRecusada()
    {
        // O mesmo exemplo, com o DV: a posição do tipo de emissão tem 0, que não é um tipo.
        Assert.False(ChaveAcesso.TentarLer("52060433009911002506550120000007800267301615", out _));
    }

    [Fact]
    public void CnpjAlfanumerico()
    {
        // CNPJ do exemplo da Receita Federal; DV conferido à parte, pela regra da NT 2025.001 (ASCII − 48).
        var emissao = new DateTimeOffset(2026, 7, 6, 9, 0, 0, TimeSpan.FromHours(-3));
        var chave = ChaveAcesso.Montar(UF.SP, emissao, "12.ABC.345/01DE-35", Documento.NFe, 1, 123,
            TipoEmissao.Normal, 87654321);

        Assert.Equal("35260712ABC34501DE35550010000001231876543210", chave.ToString());
        Assert.Equal("12ABC34501DE35", chave.Emitente);

        var lida = ChaveAcesso.Ler("35260712ABC34501DE35550010000001231876543210");
        Assert.Equal(chave, lida);
        Assert.Equal(0, lida.DigitoVerificador);
    }

    [Fact]
    public void EmitenteCpfLevaTresZerosAEsquerda()
    {
        // MOC 7.0, Visão Geral, 2.2.6.1, e NT 2023.002, 2.1: o CPF precedido de zeros, completando 14 posições.
        var chave = ChaveAcesso.Montar(UF.MG, Outubro2026, "529.982.247-25", Documento.NFCe, 920, 1,
            TipoEmissao.Normal, 10203040);

        Assert.Equal("31261000052998224725659200000000011102030408", chave.ToString());
        Assert.Equal("00052998224725", chave.Emitente);
        Assert.True(ChaveAcesso.TentarLer(chave.ToString(), out var lida));
        Assert.Equal(chave, lida);
    }

    [Theory]
    [InlineData("12ABC34501DE35", Documento.NFe, 1, 1, TipoEmissao.Normal, 0)]
    [InlineData("11222333000181", Documento.NFCe, 999, 999_999_999, TipoEmissao.ContingenciaOffline, 99_999_999)]
    [InlineData("52998224725", Documento.NFe, 910, 42, TipoEmissao.ContingenciaSvcRS, 4242)]
    [InlineData("A1B2C3D4E5F668", Documento.NFCe, 0, 7, TipoEmissao.ContingenciaEpec, 13)]
    public void IdaEVolta(string emitente, Documento modelo, int serie, int numero, TipoEmissao tipoEmissao,
        int codigoNumerico)
    {
        foreach (var uf in Enum.GetValues<UF>())
        {
            var montada = ChaveAcesso.Montar(uf, Outubro2026, emitente, modelo, serie, numero, tipoEmissao,
                codigoNumerico);
            var lida = ChaveAcesso.Ler(montada.ToString());

            Assert.Equal(ChaveAcesso.Tamanho, montada.ToString().Length);
            Assert.Equal(montada, lida);
            Assert.Equal(uf, lida.UF);
            Assert.Equal(2026, lida.Ano);
            Assert.Equal(10, lida.Mes);
            Assert.Equal(modelo, lida.Modelo);
            Assert.Equal(serie, lida.Serie);
            Assert.Equal(numero, lida.Numero);
            Assert.Equal(tipoEmissao, lida.TipoEmissao);
            Assert.Equal(codigoNumerico, lida.CodigoNumerico);
            Assert.Equal(montada.DigitoVerificador, lida.DigitoVerificador);
        }
    }

    [Fact]
    public void AnoEMesSaemDoDeslocamentoDaEmissao()
    {
        // 31/10 às 23h em Brasília já é novembro em UTC: vale outubro, o mês de quem emitiu.
        var emissao = new DateTimeOffset(2026, 10, 31, 23, 0, 0, TimeSpan.FromHours(-3));
        var chave = ChaveAcesso.Montar(UF.MG, emissao, "11222333000181", Documento.NFe, 1, 1, TipoEmissao.Normal, 1);
        Assert.Equal("2610", chave.ToString()[2..6]);
    }

    [Fact]
    public void QualquerDigitoTrocadoEhRecusado()
    {
        const string valor = "41180678393592000146558900000006041028190697";
        for (var digito = 0; digito <= 9; digito++)
        {
            if (digito == 7)
                continue;
            Assert.False(ChaveAcesso.TentarLer(valor[..43] + digito, out _));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("4118067839359200014655890000000604102819069")]     // 43 posições
    [InlineData("411806783935920001465589000000060410281906970")]   // 45 posições
    [InlineData(" 41180678393592000146558900000006041028190697")]
    [InlineData("NFe41180678393592000146558900000006041028190697")] // com o prefixo do Id
    [InlineData("4118 0678 3935 9200 0146 5589 0000 0060 4102 8190 6970")]
    [InlineData("99180678393592000146558900000006041028190697")]    // UF 99 (com o DV recalculado, como as seguintes)
    [InlineData("41181378393592000146558900000006041028190699")]    // mês 13
    [InlineData("41180078393592000146558900000006041028190695")]    // mês 0
    [InlineData("41180678393592000147558900000006041028190699")]    // CNPJ com DV errado
    [InlineData("41180611111111111111558900000006041028190694")]    // CNPJ com as posições iguais
    [InlineData("35260712abc34501de35550010000001231876543215")]    // letras minúsculas
    [InlineData("A5260712ABC34501DE35550010000001231876543210")]    // letra fora do CNPJ
    [InlineData("35260712ABC34501DE3A550010000001231876543212")]    // letra no DV do CNPJ
    [InlineData("41180678393592000146578900000006041028190694")]    // modelo 57
    [InlineData("41180678393592000146558900000000001028190695")]    // número zero
    [InlineData("41180678393592000146558900000006043028190693")]    // tipo de emissão 3
    [InlineData("41180678393592000146558900000006048028190694")]    // tipo de emissão 8
    [InlineData("41180678393592000146558900000006040028190699")]    // tipo de emissão 0
    public void RecusaChaveInvalida(string? valor)
    {
        Assert.False(ChaveAcesso.TentarLer(valor, out var chave));
        Assert.Null(chave);
        if (valor is not null)
            Assert.Throws<ArgumentException>(() => ChaveAcesso.Ler(valor));
    }

    [Fact]
    public void CodigoNumericoGeradoNuncaIgualAoNumeroNemSequenciaRecusada()
    {
        int[] recusados =
        [
            00000000, 11111111, 22222222, 33333333, 44444444, 55555555, 66666666, 77777777, 88888888, 99999999,
            12345678, 23456789, 34567890, 45678901, 56789012, 67890123, 78901234, 89012345, 90123456, 01234567,
        ];
        for (var numero = 1; numero <= 10_000; numero++)
        {
            var chave = ChaveAcesso.Gerar(UF.MG, Outubro2026, "11222333000181", Documento.NFe, 1, numero,
                TipoEmissao.Normal);
            Assert.NotEqual(numero, chave.CodigoNumerico);
            Assert.DoesNotContain(chave.CodigoNumerico, recusados);
            Assert.InRange(chave.CodigoNumerico, 0, 99_999_999);
            Assert.Equal(numero, chave.Numero);
            Assert.Equal(chave, ChaveAcesso.Ler(chave.ToString()));
        }
    }

    [Fact]
    public void GerarSorteiaDeNovoQuandoSaiONumeroOuUmaSequenciaRecusada()
    {
        var sorteios = new Queue<int>([42, 00000000, 12345678, 01234567, 99999999, 100_000_000, -1, 42, 7_654_321]);
        var chave = ChaveAcesso.Gerar(UF.MG, Outubro2026, "11222333000181", Documento.NFCe, 1, 42,
            TipoEmissao.Normal, sorteios.Dequeue);

        Assert.Equal(7_654_321, chave.CodigoNumerico);
        Assert.Empty(sorteios);
    }

    [Fact]
    public void GerarProduzCodigosDiferentes()
    {
        var codigos = new HashSet<int>();
        for (var i = 0; i < 1_000; i++)
        {
            codigos.Add(ChaveAcesso.Gerar(UF.MG, Outubro2026, "11222333000181", Documento.NFe, 1, 1,
                TipoEmissao.Normal).CodigoNumerico);
        }

        // Em 10^8 valores, 1.000 sorteios quase nunca repetem; a margem evita um teste instável.
        Assert.True(codigos.Count > 990);
    }

    [Fact]
    public void MontarRecusaArgumentosInvalidos()
    {
        var e = Outubro2026;
        const string c = "11222333000181";
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar((UF)99, e, c, Documento.NFe, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, new DateTimeOffset(1999, 12, 31, 0, 0, 0, TimeSpan.Zero), c, Documento.NFe, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero), c, Documento.NFe, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, (Documento)57, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, -1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1000, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1, 0, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1, 1_000_000_000, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1, 1, (TipoEmissao)3, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1, 1, TipoEmissao.Normal, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Montar(UF.MG, e, c, Documento.NFe, 1, 1, TipoEmissao.Normal, 100_000_000));
        Assert.Throws<ArgumentException>(() => ChaveAcesso.Montar(UF.MG, e, "11222333000182", Documento.NFe, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentException>(() => ChaveAcesso.Montar(UF.MG, e, "00052998224725", Documento.NFe, 1, 1, TipoEmissao.Normal, 1));
        Assert.Throws<ArgumentException>(() => ChaveAcesso.Gerar(UF.MG, e, "", Documento.NFe, 1, 1, TipoEmissao.Normal));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChaveAcesso.Gerar(UF.MG, e, c, Documento.NFe, 1, 0, TipoEmissao.Normal));
    }

    [Fact]
    public void IgualdadePelasQuarentaEQuatroPosicoes()
    {
        var a = ChaveAcesso.Ler("41180678393592000146558900000006041028190697");
        var b = ChaveAcesso.Ler("41180678393592000146558900000006041028190697");
        var outra = ChaveAcesso.Ler("43150108287693000157651010000000971000001251");

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(a.Equals((object)b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a != outra);
        Assert.False(a == null);
        Assert.False(a.Equals(null));
    }
}
