namespace OpenDFe.SefazEmulada;

// Os códigos e as mensagens da Sefin Nacional que o emulador usa (E.7), do Anexo I da NFS-e
// (ANEXO_I-SEFIN_ADN-DPS_NFSe-SNNFSe-v1.01-20260209, docs/NTS.md 12.1): a aba RN_RECEPCAO_DPS (E12xx) e a RN DPS_NFS-e.
// As mensagens como estão no anexo, sem o travessão. Os códigos sem fonte (a consulta que não acha, o pedido sem o JSON)
// são a nossa leitura e estão em docs/SEFAZ_EMULADA.md.
internal static class MotivosNFSe
{
    // Recepção (RN_RECEPCAO_DPS).
    public const string FalhaBase64 = "E1225";
    public const string EstruturaMalFormada = "E1226";
    public const string PrefixoNamespace = "E1228";
    public const string NaoUtf8 = "E1229";
    public const string FalhaSchema = "E1235";
    public const string TipoNaoTratado = "E1242";

    // A DPS (RN DPS_NFS-e).
    public const string VersaoExpirada = "E0001";
    public const string IdDivergente = "E0004";
    public const string AmbienteDivergente = "E0006";
    public const string EmissaoPosterior = "E0008";
    public const string DpsDuplicada = "E0014";
    public const string CompetenciaPosterior = "E0015";
    public const string EmitenteNaoPrestador = "E9996";
    public const string MunicipioInexistente = "E0037";
    public const string ConvenioInativo = "E0038";
    public const string EmissorProprio = "E0039";
    public const string CnpjPrestadorInvalido = "E0080";
    public const string CpfPrestadorInvalido = "E0096";
    public const string RegimeNaoAdmitido = "E0177";
    public const string BeneficioInexistente = "E0541";
    public const string AliquotaSuperior = "E0595";
    public const string RetencaoCpfTomador = "E0667";
    public const string RetencaoCpfIntermediario = "E0670";
    public const string AssinaturaInvalida = "E0714";
    public const string AssinaturaAusente = "E0717";
    public const string AssinaturaOutroTitular = "E0718";

    // A consulta (E0044 é o da substituição, "NFS-e não existe na base de dados", que serve à consulta pela chave).
    public const string NFSeInexistente = "E0044";

    // Sem código no anexo: a nossa leitura.
    public const string DpsInexistente = "EMULADA-DPS";
    public const string ConsultaInvalida = "EMULADA-CONSULTA";
    public const string SubstituicaoNaoEmulada = "EMULADA-SUBST";
    public const string MarcaInvalida = "EMULADA-MARCA";

    private static readonly Dictionary<string, string> Textos = new(StringComparer.Ordinal)
    {
        [FalhaBase64] = "Falha na decodificação da base 64 da área de dados",
        [EstruturaMalFormada] = "Estrutura descompactada mal formada.",
        [PrefixoNamespace] = "Uso de prefixo de namespace não permitido na área de dados descompactada.",
        [NaoUtf8] = "XML não está utilizando codificação UTF8.",
        [FalhaSchema] = "Falha no esquema XML do DF-e.",
        [TipoNaoTratado] = "Tipo DF-e não tratado pelo Sistema Nacional NFS-e.",
        [VersaoExpirada] = "O prazo de aceitação da versão do leiaute da DPS expirou.",
        [IdDivergente] = "Conteúdo do identificador informado na DPS difere da concatenação dos campos correspondentes.",
        [AmbienteDivergente] = "Ambiente informado diverge do ambiente de recebimento para o qual o emitente enviou a DPS.",
        [EmissaoPosterior] = "A data e hora de emissão da DPS deve ser anterior ou igual à data do seu processamento (dhProc) pelo Sistema Nacional NFS-e.",
        [DpsDuplicada] = "Conjunto de Série, Número, Código do Município Emissor e CNPJ/CPF informado nesta DPS já existe em uma NFS-e gerada a partir de uma DPS enviada anteriormente.",
        [CompetenciaPosterior] = "A data de competência informada na DPS não pode ser posterior à data de emissão (dhEmi) da DPS.",
        [EmitenteNaoPrestador] = "Nesta versão da aplicação, não é permitida a emissão de NFS-e pelo tomador ou intermediário.",
        [MunicipioInexistente] = "O código do município emissor informado na DPS é inexistente no cadastro de convênio municipal do sistema nacional.",
        [ConvenioInativo] = "A situação do convênio do município emissor informado na DPS deve ser \"ATIVO\" no cadastro de convênio municipal do sistema nacional.",
        [EmissorProprio] = "O município emissor informado na DPS deve estar parametrizado para utilizar os emissores públicos nacionais, conforme parametrização do município no Sistema Nacional NFS-e.",
        [CnpjPrestadorInvalido] = "CNPJ do prestador informado na DPS é inválido.",
        [CpfPrestadorInvalido] = "CPF do prestador informado na DPS é inválido.",
        [RegimeNaoAdmitido] = "Regime especial de tributação informado na DPS não é admitido na parametrização do município de incidência do ISSQN.",
        [BeneficioInexistente] = "Não existe o código de identificação do benefício municipal informado na DPS para o municipío de incidência do ISSQN.",
        [AliquotaSuperior] = "Não é permitido informar alíquota superior a 5%.",
        [RetencaoCpfTomador] = "Município da incidência do ISSQN não autoriza que o CPF do tomador informado na DPS seja indicado para retenção deste imposto.",
        [RetencaoCpfIntermediario] = "Município da incidência do ISSQN não autoriza que o CPF do intermediário informado na DPS seja indicado para retenção deste imposto.",
        [AssinaturaInvalida] = "Arquivo enviado com erro na assinatura.",
        [AssinaturaAusente] = "A assinatura é obrigatória quando for enviado para o Web Service.",
        [AssinaturaOutroTitular] = "A assinatura deve ser feita com o certificado digital do emitente da DPS.",
        [NFSeInexistente] = "NFS-e não existe na base de dados do autorizador de NFS-e nacional. Informe uma chave de NFS-e existente.",
        [DpsInexistente] = "Não há NFS-e gerada a partir da DPS informada.",
        [ConsultaInvalida] = "O parâmetro da consulta não tem o formato do leiaute.",
        [SubstituicaoNaoEmulada] = "A substituição de NFS-e ainda não é emulada (etapa E.8).",
        [MarcaInvalida] = "Marca do emulador inválida no xInfComp.",
    };

    public static string Texto(string codigo) => Textos[codigo];
}
