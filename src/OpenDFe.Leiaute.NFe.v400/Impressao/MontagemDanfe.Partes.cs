using OpenDFe.Impressao;

namespace OpenDFe.NFe;

// O destinatário ou remetente, os locais de retirada e de entrega (NT 2018.005) e a fatura e as duplicatas (Anexo II
// 3.1.4 a 3.1.6 e 3.8.1).
internal static partial class MontagemDanfe
{
    private static SecaoImpressao Destinatario(NotaFiscal nota)
    {
        var ide = nota.Identificacao;
        var destinatario = nota.Destinatario;
        var endereco = destinatario?.Endereco;
        var documento = !string.IsNullOrEmpty(destinatario?.IdEstrangeiro)
            ? destinatario.IdEstrangeiro
            : DocumentoPessoa(destinatario?.Cnpj, destinatario?.Cpf);

        return Secao("DESTINATÁRIO / REMETENTE",
            Linha(
                Campo("NOME / RAZÃO SOCIAL", destinatario?.Nome, 12.32),
                Campo("CNPJ / CPF", documento, 5.33),
                Campo("DATA DA EMISSÃO", ide.Emissao is { } emissao ? FormatadorImpressao.Data(emissao) : null, 2.92)),
            Linha(
                Campo("ENDEREÇO", Juntar(", ", endereco?.Logradouro, endereco?.Numero, endereco?.Complemento), 10.16),
                Campo("BAIRRO / DISTRITO", endereco?.Bairro, 4.83),
                Campo("CEP", Cep(endereco?.Cep), 2.67),
                Campo("DATA DA ENTRADA / SAÍDA", ide.SaidaEntrada is { } saida ? FormatadorImpressao.Data(saida) : null, 2.92)),
            Linha(
                Campo("MUNICÍPIO", endereco?.Municipio, 7.11),
                Campo("FONE / FAX", Telefone(endereco?.Telefone), 4.06),
                Campo("UF", Sigla(endereco?.UF, endereco?.Exterior == true), 1.14),
                Campo("INSCRIÇÃO ESTADUAL", destinatario?.InscricaoEstadual, 5.33),
                Campo("HORA DA ENTRADA / SAÍDA", ide.SaidaEntrada is { } hora ? FormatadorImpressao.Hora(hora) : null, 2.92)));
    }

    // O quadro específico do local de retirada ou de entrega (NT 2018.005, 5.2 e 5.3), só quando o grupo F ou G existe.
    private static SecaoImpressao Local(string titulo, Local local) =>
        Secao(titulo,
            Linha(
                Campo("NOME / RAZÃO SOCIAL", local.Nome, 12.32),
                Campo("CNPJ / CPF", DocumentoPessoa(local.Cnpj, local.Cpf), 5.33),
                Campo("INSCRIÇÃO ESTADUAL", local.InscricaoEstadual, 4.20)),
            Linha(
                Campo("ENDEREÇO", Juntar(", ", local.Logradouro, local.Numero, local.Complemento), 10.16),
                Campo("BAIRRO / DISTRITO", local.Bairro, 4.83),
                Campo("CEP", Cep(local.Cep), 2.67)),
            Linha(
                Campo("MUNICÍPIO", local.Municipio, 10.16),
                Campo("UF", Sigla(local.UF, local.Exterior), 1.14),
                Campo("FONE / FAX", Telefone(local.Telefone), 4.06)));

    // A fatura e as duplicatas (Y02 e Y07 a Y10): o quadro some quando não há cobrança (3.3.2).
    private static SecaoImpressao? Cobranca(NotaFiscal nota)
    {
        var cobranca = nota.Cobranca;
        if (cobranca is null || (cobranca.Fatura is null && cobranca.Duplicatas.Count == 0))
            return null;

        var linhas = new List<LinhaCamposImpressao>();
        if (cobranca.Fatura is { } fatura)
        {
            linhas.Add(Linha(
                Campo("FATURA", fatura.Numero),
                Valor("VALOR ORIGINAL", fatura.ValorOriginal),
                Valor("DESCONTO", fatura.Desconto),
                Valor("VALOR LÍQUIDO", fatura.ValorLiquido)));
        }

        // Três duplicatas por linha, como no modelo da NT 2026.010.
        for (var i = 0; i < cobranca.Duplicatas.Count; i += 3)
        {
            var campos = new List<CampoImpressao>();
            foreach (var duplicata in cobranca.Duplicatas.Skip(i).Take(3))
            {
                campos.Add(Campo("DUPLICATA", duplicata.Numero));
                campos.Add(Campo("VENCIMENTO", duplicata.Vencimento is { } vencimento ? FormatadorImpressao.Data(vencimento) : null));
                campos.Add(Valor("VALOR", duplicata.Valor));
            }

            linhas.Add(new LinhaCamposImpressao(campos));
        }

        return new SecaoImpressao("FATURA / DUPLICATAS", linhas);
    }

    private static string? Cep(string? valor) => string.IsNullOrEmpty(valor) ? null : FormatadorImpressao.CepDanfe(valor);

    private static string? Telefone(string? valor) => string.IsNullOrEmpty(valor) ? null : FormatadorImpressao.Telefone(valor);
}
