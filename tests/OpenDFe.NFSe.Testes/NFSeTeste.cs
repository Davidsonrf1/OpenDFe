using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// As NFS-e de exemplo (PROJETO.md 13.2): a 1.01 montada no teste a partir de uma DPS de teste, com o infNFSe completo, e
// assinada pelo certificado de teste no papel da Sefin (o schema exige a Signature); a 1.00, à mão, com os grupos que a
// 1.01 tirou (docs/NFSE.md 7, M10).
internal static class NFSeTeste
{
    internal const string Chave = "3106200" + "2" + "2" + DpsTeste.CnpjPrestador + "0000000000042" + "2611" + "123456789" + "0";

    internal static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = "00394460005887" }.Gerar();

    private static readonly XmlWriterSettings Configuracao = new() { OmitXmlDeclaration = true, Indent = false };

    internal static NotaFiscalServico Nota(Dps dps) => new()
    {
        NomeMunicipioEmissor = "Belo Horizonte",
        NomeLocalPrestacao = "Belo Horizonte",
        Numero = 42,
        MunicipioIncidencia = DpsTeste.Municipio,
        NomeMunicipioIncidencia = "Belo Horizonte",
        DescricaoTributacaoNacional = "Análise e desenvolvimento de sistemas.",
        DescricaoTributacaoMunicipal = "Desenvolvimento de software",
        DescricaoNbs = "Serviços de projeto e desenvolvimento de software",
        VersaoAplicativo = "SefinNac_1.0",
        AmbienteGerador = AmbienteGerador.SistemaNacional,
        TipoEmissao = TipoEmissaoNFSe.Normal,
        ProcessoEmissao = ProcessoEmissao.Contribuinte,
        Situacao = SituacaoNFSe.Gerada,
        Processamento = new DateTimeOffset(2026, 11, 19, 10, 31, 5, TimeSpan.FromHours(-3)),
        NumeroDfse = 1234,
        Emitente =
        {
            Cnpj = DpsTeste.CnpjPrestador,
            InscricaoMunicipal = "12345",
            Nome = "PRESTADOR DE TESTE LTDA",
            NomeFantasia = "TESTE",
            Endereco = { Logradouro = "RUA DOS TESTES", Numero = "100", Complemento = "SALA 2", Bairro = "CENTRO", Municipio = DpsTeste.Municipio, UF = UF.MG, Cep = "30110000" },
            Telefone = "31999990000",
            Email = "prestador@exemplo.com.br",
        },
        Valores =
        {
            DeducaoReducao = 100.00m,
            TipoBeneficio = TipoBeneficio.ReducaoValor,
            ReducaoBeneficio = 1.00m,
            BaseCalculo = 892.50m,
            AliquotaAplicada = 2.00m,
            Issqn = 17.85m,
            TotalRetencoes = 25.00m,
            Liquido = 967.50m,
        },
        InformacoesFisco = "Uso do município",
        IbsCbs = new IbsCbsNFSe
        {
            MunicipioIncidencia = DpsTeste.Municipio,
            NomeMunicipioIncidencia = "Belo Horizonte",
            PercentualRedutor = 10.00m,
            Valores =
            {
                BaseCalculo = 892.50m,
                ReembolsoRepasse = 50.00m,
                UF = { Aliquota = 0.10m, Reducao = 0.00m, Efetiva = 0.10m },
                Municipio = { Aliquota = 0.00m, Reducao = 0.00m, Efetiva = 0.00m },
                Cbs = { Aliquota = 0.90m, Reducao = 0.00m, Efetiva = 0.90m },
            },
            Totais =
            {
                TotalNota = 1000.00m,
                Ibs =
                {
                    Total = 0.89m,
                    CreditoPresumido = new CreditoPresumidoIbs { Aliquota = 1.00m, Valor = 0.01m },
                    UF = { Diferimento = 0.00m, Valor = 0.89m },
                    Municipio = { Diferimento = 0.00m, Valor = 0.00m },
                },
                Cbs = { CreditoPresumido = new CreditoPresumidoCbs { Aliquota = 1.00m, Valor = 0.08m }, Diferimento = 0.00m, Valor = 8.03m },
                TributacaoRegular = new TotalTributacaoRegular
                {
                    AliquotaIbsUF = 0.10m, ValorIbsUF = 0.89m, AliquotaIbsMunicipio = 0.00m, ValorIbsMunicipio = 0.00m, AliquotaCbs = 0.90m, ValorCbs = 8.03m,
                },
                CompraGovernamental = new TotalCompraGovernamental
                {
                    AliquotaIbsUF = 0.10m, ValorIbsUF = 0.89m, AliquotaIbsMunicipio = 0.00m, ValorIbsMunicipio = 0.00m, AliquotaCbs = 0.90m, ValorCbs = 8.03m,
                },
            },
        },
        Dps = dps,
    };

    // O NFSe 1.01: o infNFSe pelo mapa do leiaute (a lib não escreve a NFS-e; o teste usa o mapa para montar o exemplo), a
    // DPS escrita pela lib, e a assinatura no fim do NFSe.
    internal static string Xml(NotaFiscalServico nota)
    {
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, Configuracao))
        {
            var e = new EscritaNFSe(xml);
            e.Abrir("NFSe", null);
            e.Atributo("versao", "1.01");
            e.Abrir("infNFSe", null);
            e.Atributo("Id", "NFS" + Chave);
            EscritaMapa.Escrever(e, MapasNFSe.MapaNotaFiscalServico, nota);
            xml.WriteRaw(EscritorNFSe.Escrever(nota.Dps, null).Xml);
            e.Fechar();
            e.Fechar();
        }
        return AssinaturaDigital.Assinar(texto.ToString(), "NFS" + Chave, Certificado);
    }

    // A NFS-e 1.00, à mão: o xOutInf dentro dos valores e o tpBM em texto; na DPS, a locação, o pedágio, o evento com
    // desc e id, o benefício com o tpBM; o tribISSQN 2 (exportação, no texto da 1.00) e a situação 101, que a 1.01 tirou.
    internal static string Xml100()
    {
        var xml =
            "<NFSe versao=\"1.00\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><infNFSe Id=\"NFS" + Chave + "\">"
            + "<xLocEmi>Belo Horizonte</xLocEmi><xLocPrestacao>Belo Horizonte</xLocPrestacao><nNFSe>42</nNFSe>"
            + "<xTribNac>Análise e desenvolvimento de sistemas.</xTribNac><verAplic>SefinNac_0.9</verAplic><ambGer>2</ambGer><tpEmis>1</tpEmis>"
            + "<cStat>101</cStat><dhProc>2025-08-01T09:00:00-03:00</dhProc><nDFSe>77</nDFSe>"
            + "<emit><CNPJ>" + DpsTeste.CnpjPrestador + "</CNPJ><xNome>PRESTADOR DE TESTE LTDA</xNome>"
            + "<enderNac><xLgr>RUA DOS TESTES</xLgr><nro>100</nro><xBairro>CENTRO</xBairro><cMun>3106200</cMun><UF>MG</UF><CEP>30110000</CEP></enderNac></emit>"
            + "<valores><tpBM>Isenção parcial</tpBM><vLiq>1000.00</vLiq><xOutInf>Uso do município na 1.00</xOutInf></valores>"
            + "<DPS versao=\"1.00\"><infDPS Id=\"DPS310620021122233300018100001000000000000007\">"
            + "<tpAmb>2</tpAmb><dhEmi>2025-08-01T08:59:00-03:00</dhEmi><verAplic>ERP 0.9</verAplic><serie>1</serie><nDPS>7</nDPS>"
            + "<dCompet>2025-08-01</dCompet><tpEmit>1</tpEmit><cLocEmi>3106200</cLocEmi>"
            + "<prest><CNPJ>" + DpsTeste.CnpjPrestador + "</CNPJ><regTrib><opSimpNac>1</opSimpNac><regEspTrib>0</regEspTrib></regTrib></prest>"
            + "<serv><locPrest><cLocPrestacao>3106200</cLocPrestacao></locPrest><cServ><cTribNac>220101</cTribNac><xDescServ>Pedágio</xDescServ></cServ>"
            + "<lsadppu><categ>1</categ><objeto>3</objeto><extensao>120</extensao><nPostes>45</nPostes></lsadppu>"
            + "<atvEvento><desc>FESTIVAL</desc><dtIni>2025-07-01</dtIni><dtFim>2025-07-02</dtFim><id>EVT9</id></atvEvento>"
            + "<explRod><categVeic>01</categVeic><nEixos>2</nEixos><rodagem>1</rodagem><sentido>1</sentido><placa>ABC1234</placa>"
            + "<codAcessoPed>ABCDE12345</codAcessoPed><codContrato>AB12</codContrato></explRod></serv>"
            + "<valores><vServPrest><vServ>1000.00</vServ></vServPrest><trib><tribMun><tribISSQN>2</tribISSQN>"
            + "<BM><tpBM>3</tpBM><nBM>12345678901234</nBM><vRedBCBM>10.00</vRedBCBM></BM><tpRetISSQN>1</tpRetISSQN></tribMun>"
            + "<totTrib><indTotTrib>0</indTotTrib></totTrib></trib></valores></infDPS></DPS></infNFSe></NFSe>";
        return AssinaturaDigital.Assinar(xml, "NFS" + Chave, Certificado);
    }
}
