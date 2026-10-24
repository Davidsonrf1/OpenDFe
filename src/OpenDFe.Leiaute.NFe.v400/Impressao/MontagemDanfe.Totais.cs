using System.Globalization;
using OpenDFe.Impressao;

namespace OpenDFe.NFe;

// O cálculo do imposto, o total do IBS/CBS/IS (NT 2026.010 4.1), o cálculo do ISSQN e o quadro do transportador (Anexo II
// 3.1.10, 3.3.3 e 3.8.1).
internal static partial class MontagemDanfe
{
    // O "Cálculo do imposto": os dois blocos de linhas do Anexo II e, depois deles, o que o XML tem e o leiaute não prevê
    // (o valor aproximado dos tributos, 3; o ICMS desonerado, 3.10.5; o FCP e o DIFAL, o ICMS monofásico, NT 2026.010 4.4),
    // cada um só quando o valor é maior que zero.
    private static SecaoImpressao Imposto(NotaFiscal nota)
    {
        var total = nota.Total.Icms;
        var linhas = new List<LinhaCamposImpressao>
        {
            Linha(
                Valor("BASE DE CÁLCULO DO ICMS", total.BaseCalculo, 4.06),
                Valor("VALOR DO ICMS", total.Valor, 4.06),
                Valor("BASE DE CÁLCULO DO ICMS ST", total.BaseCalculoSt, 4.06),
                Valor("VALOR DO ICMS ST", total.ValorSt, 4.06),
                Valor("VALOR TOTAL DOS PRODUTOS", total.ValorProdutos, 4.32)),
            Linha(
                Valor("VALOR DO FRETE", total.Frete, 3.30),
                Valor("VALOR DO SEGURO", total.Seguro, 3.30),
                Valor("DESCONTO", total.Desconto, 3.30),
                Valor("OUTRAS DESPESAS ACESSÓRIAS", total.OutrasDespesas, 3.30),
                Valor("VALOR DO IPI", total.ValorIpi, 3.30),
                Valor("VALOR TOTAL DA NOTA", total.ValorNota, 4.06, negrito: true)),
        };

        AcrescentarSeHouver(linhas,
            Valor("VALOR APROXIMADO DOS TRIBUTOS", Positivo(total.ValorTotalTributos)),
            Valor("VALOR DO ICMS DESONERADO", Positivo(total.ValorDesonerado)));
        AcrescentarSeHouver(linhas,
            Valor("VALOR DO FCP", Positivo(total.ValorFcp)),
            Valor("VALOR DO FCP RETIDO POR ST", Positivo(total.ValorFcpStRetido)),
            Valor("VALOR DO DIFAL NA UF DE DESTINO", Positivo(total.ValorIcmsUFDestino)),
            Valor("VALOR DO FCP NA UF DE DESTINO", Positivo(total.ValorFcpUFDestino)));
        AcrescentarSeHouver(linhas,
            Campo("BC DO ICMS MONOFÁSICO", Quantidade(total.QuantidadeBaseMonofasico), alinhamento: AlinhamentoImpressao.Direita),
            Valor("VALOR DO ICMS MONOFÁSICO", Positivo(total.ValorMonofasico)),
            Campo("BC DO ICMS MONOFÁSICO POR RETENÇÃO", Quantidade(total.QuantidadeBaseRetencao), alinhamento: AlinhamentoImpressao.Direita),
            Valor("VALOR DO ICMS MONOFÁSICO POR RETENÇÃO", Positivo(total.ValorMonofasicoRetencao)));
        return new SecaoImpressao("CÁLCULO DO IMPOSTO", linhas);
    }

    // O bloco "Total do IBS/CBS/IS" (NT 2026.010 4.1), depois do bloco do ICMS e do IPI: só os campos que o XML tem; o bloco
    // some quando a nota não tem nenhum dos tributos da reforma.
    private static SecaoImpressao? TotalReforma(NotaFiscal nota)
    {
        var total = nota.Total;
        var linhas = new List<LinhaCamposImpressao>();
        AcrescentarSeHouver(linhas,
            ExisteValor("VALOR DO IBS UF", total.IbsCbs?.Ibs?.UF.Valor),
            ExisteValor("VALOR DO IBS MUNICÍPIO", total.IbsCbs?.Ibs?.Municipio.Valor),
            ExisteValor("VALOR DA CBS", total.IbsCbs?.Cbs?.Valor),
            ExisteValor("VALOR DO IMPOSTO SELETIVO", total.ImpostoSeletivo?.Valor));
        AcrescentarSeHouver(linhas,
            Valor("VALOR DO IBS MONOFÁSICO", Positivo(total.IbsCbs?.Monofasia?.Ibs)),
            Valor("VALOR DA CBS MONOFÁSICA", Positivo(total.IbsCbs?.Monofasia?.Cbs)),
            Valor("VALOR DO IBS MONOFÁSICO POR RETENÇÃO", Positivo(total.IbsCbs?.Monofasia?.IbsRetencao)),
            Valor("VALOR DA CBS MONOFÁSICA POR RETENÇÃO", Positivo(total.IbsCbs?.Monofasia?.CbsRetencao)));
        return linhas.Count == 0 ? null : new SecaoImpressao("TOTAL DO IBS / CBS / IS", linhas);
    }

    // O valor que existe no XML sai, mesmo zero (o destaque do IBS e da CBS da nota); o que não existe, não sai.
    private static CampoImpressao? ExisteValor(string rotulo, decimal? valor) => valor is null ? null : Valor(rotulo, valor);

    private static decimal? Positivo(decimal? valor) => valor is > 0m ? valor : null;

    private static string Quantidade(decimal? valor) => valor is > 0m and var v ? FormatadorImpressao.Fixo(v, 4) : string.Empty;

    // Acrescenta uma linha com os campos que têm valor: sem nenhum, nada se acrescenta. Os campos vazios (sem valor) caem.
    private static void AcrescentarSeHouver(List<LinhaCamposImpressao> linhas, params CampoImpressao?[] campos)
    {
        var existentes = campos.Where(c => c is not null && c.Valor.Length > 0).Select(c => c!).ToArray();
        if (existentes.Length > 0)
            linhas.Add(new LinhaCamposImpressao(existentes));
    }

    // O cálculo do ISSQN: só quando a nota tem o total do ISSQN (3.3.3).
    private static SecaoImpressao? Issqn(NotaFiscal nota)
    {
        if (nota.Total.Issqn is not { } issqn)
            return null;
        return Secao("CÁLCULO DO ISSQN",
            Linha(
                Campo("INSCRIÇÃO MUNICIPAL", nota.Emitente.InscricaoMunicipal, 5.08),
                Valor("VALOR TOTAL DOS SERVIÇOS", issqn.ValorServicos, 5.08),
                Valor("BASE DE CÁLCULO DO ISSQN", issqn.BaseCalculo, 5.08),
                Valor("VALOR DO ISSQN", issqn.Valor, 5.33)));
    }

    // O transportador e os volumes (X): sempre os campos do modelo, em branco quando o XML não os traz; um volume por linha.
    private static SecaoImpressao Transporte(NotaFiscal nota)
    {
        var transporte = nota.Transporte;
        var transportador = transporte.Transportador;
        var veiculo = transporte.Veiculo;
        var linhas = new List<LinhaCamposImpressao>
        {
            Linha(
                Campo("NOME / RAZÃO SOCIAL", transportador?.Nome, 9.02),
                Campo("FRETE POR CONTA", transporte.Modalidade is { } modalidade ? Modalidade(modalidade) : null, 3.5),
                Campo("CÓDIGO ANTT", veiculo?.Rntc, 2.5),
                Campo("PLACA DO VEÍCULO", veiculo?.Placa, 2.79),
                Campo("UF", Sigla(veiculo?.UF, exterior: false), 1.78),
                Campo("CNPJ / CPF", DocumentoPessoa(transportador?.Cnpj, transportador?.Cpf), 4.0)),
            Linha(
                Campo("ENDEREÇO", transportador?.Endereco, 9.02),
                Campo("MUNICÍPIO", transportador?.Municipio, 6.86),
                Campo("UF", Sigla(transportador?.UF, transportador?.Exterior == true), 0.76),
                Campo("INSCRIÇÃO ESTADUAL", transportador?.InscricaoEstadual, 3.94)),
        };

        if (transporte.Volumes.Count == 0)
            linhas.Add(Volume(null));
        foreach (var volume in transporte.Volumes)
            linhas.Add(Volume(volume));
        return new SecaoImpressao("TRANSPORTADOR / VOLUMES TRANSPORTADOS", linhas);
    }

    private static LinhaCamposImpressao Volume(Volume? volume) =>
        Linha(
            Campo("QUANTIDADE", volume?.Quantidade?.ToString(CultureInfo.InvariantCulture), 2.92, alinhamento: AlinhamentoImpressao.Direita),
            Campo("ESPÉCIE", volume?.Especie, 3.05),
            Campo("MARCA", volume?.Marca, 3.05),
            Campo("NUMERAÇÃO", volume?.Numeracao, 4.83),
            Campo("PESO BRUTO", volume?.PesoBruto is { } bruto ? FormatadorImpressao.Fixo(bruto, 3) : null, 3.43, alinhamento: AlinhamentoImpressao.Direita),
            Campo("PESO LÍQUIDO", volume?.PesoLiquido is { } liquido ? FormatadorImpressao.Fixo(liquido, 3) : null, 3.30, alinhamento: AlinhamentoImpressao.Direita));

    // A modalidade do frete (X02) com o código do Anexo II 3.1.10 (NT 2016.002 e 2018.005).
    private static string Modalidade(ModalidadeFrete modalidade) => modalidade switch
    {
        ModalidadeFrete.Remetente => "0 - Remetente (CIF)",
        ModalidadeFrete.Destinatario => "1 - Destinatário (FOB)",
        ModalidadeFrete.Terceiros => "2 - Terceiros",
        ModalidadeFrete.ProprioRemetente => "3 - Próprio do Remetente",
        ModalidadeFrete.ProprioDestinatario => "4 - Próprio do Destinatário",
        ModalidadeFrete.NenhumTransporte => "9 - Sem Ocorrência de Transporte",
        _ => ((int)modalidade).ToString(CultureInfo.InvariantCulture),
    };
}
