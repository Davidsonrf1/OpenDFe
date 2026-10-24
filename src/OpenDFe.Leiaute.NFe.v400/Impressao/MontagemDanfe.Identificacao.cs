using System.Globalization;
using OpenDFe.Impressao;

namespace OpenDFe.NFe;

// O canhoto e o bloco de identificação: o emitente, o "DANFE", a chave com os dois códigos de barras, o protocolo e os
// campos do emitente (Anexo II 3.1.1 a 3.1.3, 3.5 e 3.9; NT 2026.010 4.2).
internal static partial class MontagemDanfe
{
    private const string Consulta = "Consulte a autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora";
    private const string ConsultaEpec = "Consulta de autenticidade no portal da NF-e www.nfe.fazenda.gov.br/portal";
    private const string LegendaFormularioSeguranca = "DANFE em Contingência - impresso em decorrência de problemas técnicos";
    private const string LegendaEpec = "DANFE impresso em contingência - EPEC regularmente recebida pela Receita Federal do Brasil";

    // O recibo de entrega (Anexo II 3.8.1): o texto, a data de recebimento, a assinatura e o número da nota; a linha
    // tracejada o separa do resto, que é o que se destaca.
    private static List<ElementoImpressao> Canhoto(NotaFiscal nota, ChaveAcesso chave) =>
    [
        new TextoImpressao(
            $"RECEBEMOS DE {Vazio(nota.Emitente.Nome)} OS PRODUTOS E/OU SERVIÇOS CONSTANTES DA NOTA FISCAL ELETRÔNICA INDICADA AO LADO")
        {
            Tamanho = TamanhoTexto.Pequeno,
        },
        Secao(null,
            Linha(
                Campo("DATA DE RECEBIMENTO", string.Empty, 4.10),
                Campo("IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR", string.Empty, 12.10),
                Campo("NF-e", $"Nº {FormatadorImpressao.NumeroNota(Numero(nota, chave))} SÉRIE {FormatadorImpressao.Zeros(Serie(nota, chave), 3)}", 4.50, negrito: true))),
        new SeparadorImpressao(),
    ];

    private static List<ElementoImpressao> Identificacao(NotaFiscal nota, ChaveAcesso chave, Protocolo? protocolo, bool autorizada,
        OpcoesDanfe opcoes)
    {
        var ide = nota.Identificacao;
        var emitente = nota.Emitente;
        var endereco = emitente.Endereco;
        var formularioSeguranca = ide.TipoEmissao is TipoEmissao.ContingenciaFSIA or TipoEmissao.ContingenciaFSDA && !autorizada;
        var epec = ide.TipoEmissao == TipoEmissao.ContingenciaEpec && !autorizada;

        var elementos = new List<ElementoImpressao>();
        if (opcoes.Logotipo is { } logotipo)
            elementos.Add(new ImagemImpressao(logotipo) { Alinhamento = AlinhamentoImpressao.Esquerda });

        // O quadro do emitente (3.1.3): nome ou razão social, endereço completo e telefone, em negrito.
        elementos.Add(new TextoImpressao(Vazio(emitente.Nome)) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        var logradouro = Juntar(", ", endereco.Logradouro, endereco.Numero, endereco.Complemento);
        var primeira = Juntar(" - ", logradouro, endereco.Bairro);
        if (primeira.Length > 0)
            elementos.Add(new TextoImpressao(primeira) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        var cidade = Juntar(" - ", endereco.Municipio, endereco.UF?.Sigla(),
            string.IsNullOrEmpty(endereco.Cep) ? null : "CEP: " + FormatadorImpressao.CepDanfe(endereco.Cep));
        if (cidade.Length > 0)
            elementos.Add(new TextoImpressao(cidade) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        if (!string.IsNullOrEmpty(endereco.Telefone))
            elementos.Add(new TextoImpressao("Fone: " + FormatadorImpressao.Telefone(endereco.Telefone)) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });

        // O quadro "DANFE" (3.7.4): o tipo de operação com o dígito do tpNF, o número, a série e a folha (as fichas de página).
        elementos.Add(new TextoImpressao("DANFE") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true, Tamanho = TamanhoTexto.Duplo });
        elementos.Add(new TextoImpressao("DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        elementos.Add(Secao(null,
            Linha(Campo("0 - ENTRADA / 1 - SAÍDA", ide.Tipo is { } tipo ? ((int)tipo).ToString(CultureInfo.InvariantCulture) : string.Empty, negrito: true)),
            Linha(
                Campo("Nº", FormatadorImpressao.NumeroNota(Numero(nota, chave)), negrito: true),
                Campo("SÉRIE", FormatadorImpressao.Zeros(Serie(nota, chave), 3), negrito: true),
                Campo("FOLHA", TextoPaginado.Pagina + "/" + TextoPaginado.Paginas, negrito: true))));

        // A contingência com o formulário de segurança e o EPEC trazem a expressão em destaque (MOC Anexo III 2.1 e 2.2).
        if (formularioSeguranca)
        {
            elementos.Add(new TextoImpressao(LegendaFormularioSeguranca) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
            var motivo = Juntar(" - ", ide.JustificativaContingencia,
                ide.InicioContingencia is { } inicio ? "desde " + FormatadorImpressao.DataHora(inicio) : null);
            if (motivo.Length > 0)
                elementos.Add(new TextoImpressao("Motivo da contingência: " + motivo) { Alinhamento = AlinhamentoImpressao.Centro });
        }
        else if (epec)
        {
            elementos.Add(new TextoImpressao(LegendaEpec) { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true });
        }

        // O código de barras da chave e a chave em onze blocos de quatro (2 e 3.1.1).
        elementos.Add(new CodigoBarrasImpressao(chave.ToString()));
        elementos.Add(Secao(null, Linha(Campo("CHAVE DE ACESSO", FormatadorImpressao.Chave(chave.ToString()), negrito: true))));

        // Os campos 1 e 2 de conteúdo variável (3.9): o formulário de segurança traz o código de barras dos "Dados da NF-e";
        // nos demais casos, a consulta de autenticidade e o protocolo (do EPEC, se for o caso).
        if (formularioSeguranca)
        {
            var dados = DadosNota(nota, chave);
            elementos.Add(new CodigoBarrasImpressao(dados));
            elementos.Add(Secao(null, Linha(Campo("DADOS DA NF-e", BlocosQuatro(dados), negrito: true))));
        }
        else
        {
            elementos.Add(new TextoImpressao(epec ? ConsultaEpec : Consulta) { Alinhamento = AlinhamentoImpressao.Centro, Tamanho = TamanhoTexto.Pequeno });
            var rotulo = epec ? "PROTOCOLO DE AUTORIZAÇÃO DO EPEC" : "PROTOCOLO DE AUTORIZAÇÃO DE USO";
            var valor = autorizada && protocolo is not null
                ? Juntar(" ", protocolo.Numero, protocolo.Recebimento is { } recebimento ? FormatadorImpressao.DataHora(recebimento) : null)
                : string.Empty;
            elementos.Add(Secao(null, Linha(Campo(rotulo, valor, negrito: true))));
        }

        elementos.Add(Secao(null, Linha(Campo("NATUREZA DA OPERAÇÃO", ide.NaturezaOperacao, negrito: true))));

        var linhas = new List<LinhaCamposImpressao>
        {
            Linha(
                Campo("INSCRIÇÃO ESTADUAL", emitente.InscricaoEstadual, 6.86),
                Campo("INSCRIÇÃO ESTADUAL DO SUBST. TRIBUTÁRIO", emitente.InscricaoEstadualSubstituto, 6.86),
                Campo("CNPJ / CPF", DocumentoPessoa(emitente.Cnpj, emitente.Cpf), 6.86)),
        };
        if (emitente.RegimeTributario is { } regime)
            linhas.Add(Linha(Campo("CÓDIGO DO REGIME TRIBUTÁRIO", RegimeTexto(regime))));
        elementos.Add(new SecaoImpressao(null, linhas));
        return elementos;
    }

    // O número e a série da nota: os do ide, e os da chave na falta deles.
    private static int Numero(NotaFiscal nota, ChaveAcesso chave) => nota.Identificacao.Numero ?? chave.Numero;

    private static int Serie(NotaFiscal nota, ChaveAcesso chave) => nota.Identificacao.Serie ?? chave.Serie;

    // O texto do CRT (NT 2026.010 4.2): o código e o nome do regime.
    private static string RegimeTexto(RegimeTributario regime) => regime switch
    {
        RegimeTributario.SimplesNacional => "1 - SIMPLES NACIONAL",
        RegimeTributario.SimplesExcessoSublimite => "2 - SIMPLES NACIONAL - EXCESSO DE SUBLIMITE DE RECEITA BRUTA",
        RegimeTributario.Normal => "3 - REGIME NORMAL",
        RegimeTributario.Mei => "4 - SIMPLES NACIONAL - MEI",
        _ => ((int)regime).ToString(CultureInfo.InvariantCulture),
    };

    private static string BlocosQuatro(string digitos)
    {
        var blocos = new List<string>();
        for (var i = 0; i < digitos.Length; i += 4)
            blocos.Add(digitos.Substring(i, Math.Min(4, digitos.Length - i)));
        return string.Join(' ', blocos);
    }

    // O código de barras adicional "Dados da NF-e" do formulário de segurança (Anexo II 3.9.2): 36 dígitos, cada campo com
    // zeros à esquerda e sem formatação: cUF (99 no exterior), tpEmis, CNPJ ou CPF do destinatário (zeros no exterior), vNF
    // em centavos, ICMS próprio (1 há destaque, 2 não), ICMS ST (1 há, 2 não), o dia da emissão e o DV pelo módulo 11.
    private static string DadosNota(NotaFiscal nota, ChaveAcesso chave)
    {
        var ide = nota.Identificacao;
        var destinatario = nota.Destinatario;
        var exterior = ide.LocalDestino == LocalDestino.Exterior || destinatario?.Endereco?.Exterior == true
            || !string.IsNullOrEmpty(destinatario?.IdEstrangeiro);
        var uf = exterior ? 99 : (destinatario?.Endereco?.UF ?? chave.UF).Codigo();
        var documento = exterior ? string.Empty : destinatario?.Cnpj ?? destinatario?.Cpf ?? string.Empty;
        if (!documento.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                "O código de barras dos Dados da NF-e (Anexo II 3.9.2) só tem dígitos, e o CNPJ do destinatário não é numérico.", nameof(nota));
        }

        var valor = (long)Math.Round((nota.Total.Icms.ValorNota ?? 0m) * 100m, 0, MidpointRounding.AwayFromZero);
        var dia = (ide.Emissao ?? DateTimeOffset.MinValue).Day;

        var corpo = string.Concat(
            uf.ToString("00", CultureInfo.InvariantCulture),
            ((int)(ide.TipoEmissao ?? chave.TipoEmissao)).ToString(CultureInfo.InvariantCulture),
            documento.PadLeft(14, '0'),
            valor.ToString("00000000000000", CultureInfo.InvariantCulture),
            nota.Total.Icms.Valor is > 0m ? "1" : "2",
            nota.Total.Icms.ValorSt is > 0m ? "1" : "2",
            dia.ToString("00", CultureInfo.InvariantCulture));
        return corpo + Modulo11(corpo).ToString(CultureInfo.InvariantCulture);
    }

    // O dígito verificador do módulo 11, com pesos de 2 a 9 da direita para a esquerda, como o da chave (MOC, 5.4).
    private static int Modulo11(string digitos)
    {
        var soma = 0;
        var peso = 2;
        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
