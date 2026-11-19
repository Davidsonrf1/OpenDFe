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

    // A substituição (E.8), no grupo subst da DPS (RN DPS_NFS-e).
    public const string ChaveSubstituidaInvalida = "E0042";
    public const string SubstituidaCancelada = "E0046";
    public const string SubstituicaoForaPrazo = "E0050";
    public const string SubstituidaAnalisePendente = "E0068";
    public const string MotivoSubstituicaoAusente = "E0078";

    // O pedido de registro de evento (E.8), do Anexo II (ANEXO_II-SEFIN_ADN-PEDREGEVT_EVT-SNNFSe-v1.01-20260122,
    // docs/NTS.md 12.1), aba RN_EVENTO_PED.REG.EVENTO.
    public const string EventoCnpjAutorCertificado = "E0812";
    public const string EventoCnpjAutorTipo = "E0813";
    public const string EventoCpfAutorCertificado = "E0815";
    public const string EventoCpfAutorTipo = "E0816";
    public const string CancelamentoForaPrazo = "E0822";
    public const string CancelamentoImpedido = "E0840";
    public const string SubstituicaoImpedida = "E0845";
    public const string AnaliseImpedida = "E0848";
    public const string DeferimentoImpedido = "E0852";
    public const string DeferimentoSemAnalise = "E0853";
    public const string IndeferimentoImpedido = "E0855";
    public const string IndeferimentoSemAnalise = "E0856";
    public const string EventoNFSeInexistente = "E1831";
    public const string IdPedidoDivergente = "E1827";
    public const string EventoEmissaoPosterior = "E1843";
    public const string EventoAmbienteDivergente = "E1845";
    public const string CancelamentoOficioImpedido = "E1960";
    public const string EventoAssinaturaInvalida = "E1980";
    public const string EventoAssinaturaAusente = "E1989";

    // Sem código no anexo: a nossa leitura.
    public const string DpsInexistente = "EMULADA-DPS";
    public const string ConsultaInvalida = "EMULADA-CONSULTA";
    public const string MarcaInvalida = "EMULADA-MARCA";
    public const string EventoChaveDivergente = "EMULADA-CHAVE";
    public const string EventoNaoAceito = "EMULADA-TIPO";
    public const string EventoInexistente = "EMULADA-EVENTO";

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
        [ChaveSubstituidaInvalida] = "Chave de NFS-e a ser substituída é inválida.",
        [SubstituidaCancelada] = "Uma NFS-e cancelada não pode ser substituída. Informe uma chave de NFS-e não cancelada anteriormente.",
        [SubstituicaoForaPrazo] = "Uma NFS-e não pode ser substituída fora do prazo estabelecido pelo município emissor da NFS-e.",
        [SubstituidaAnalisePendente] = "Não é possível a substituição desta NFS-e pois a mesma possui registro de Evento de Solicitação de Análise Fiscal para Cancelamento de NFS-e aguardando resposta. Para mais informações, consultar a Administração Tributária Municipal do município emissor da NFS-e.",
        [MotivoSubstituicaoAusente] = "Quando o campo cMotivo = 99, o campo xMotivo deve informado obrigatoriamente.",
        [EventoCnpjAutorCertificado] = "O CNPJ do autor do pedido de registro de evento não corresponde à base do CNPJ informada no certificado digital da assinatura do pedido.",
        [EventoCnpjAutorTipo] = "O CNPJ autor informado no pedido de registro de evento não corresponde ao \"AUTOR DO PEDIDO DE REGISTRO DE EVENTO\" indicado na planilha \"Tipo Eventos\".",
        [EventoCpfAutorCertificado] = "O CPF do autor do pedido de registro de evento não corresponde ao CPF informado no certificado digital da assinatura do pedido.",
        [EventoCpfAutorTipo] = "O CPF autor informado no pedido de registro de evento não corresponde ao \"AUTOR DO PEDIDO DE REGISTRO DE EVENTO\" indicado na planilha \"Tipo Eventos\".",
        [CancelamentoForaPrazo] = "O prazo para o cancelamento da NFS-e expirou, conforme parametrização do município emissor da NFS-e.",
        [CancelamentoImpedido] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [SubstituicaoImpedida] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e POR SUBSTITUIÇÃO pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [AnaliseImpedida] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE SOLICITAÇÃO DE ANÁLISE FISCAL PARA CANCELAMENTO DE NFS-e pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [DeferimentoImpedido] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e DEFERIDO POR ANÁLISE FISCAL pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [DeferimentoSemAnalise] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e DEFERIDO POR ANÁLISE FISCAL pois não existe um EVENTO DE SOLICITAÇÃO DE ANÁLISE FISCAL PARA CANCELAMENTO DE NFS-e pendente para deferimento ou indeferimento.",
        [IndeferimentoImpedido] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-E INDEFERIDO POR ANÁLISE FISCAL pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [IndeferimentoSemAnalise] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e INDEFERIDO POR ANÁLISE FISCAL pois não existe um EVENTO DE SOLICITAÇÃO DE ANÁLISE FISCAL PARA CANCELAMENTO DE NFS-e pendente para deferimento ou indeferimento.",
        [EventoNFSeInexistente] = "O pedido de registro de evento não pode ser validado pois a NFS-e indicada não existe no Ambiente de Dados Nacional NFS-e.",
        [IdPedidoDivergente] = "Conteúdo do identificador informado no identificador do Pedido de Registro de Evento difere da concatenação dos campos correspondentes.",
        [EventoEmissaoPosterior] = "A data de emissão do pedido do registro do evento não pode ser posterior à data de recebimento pelo Sistema Nacional NFS-e.",
        [EventoAmbienteDivergente] = "Ambiente informado diverge do ambiente de recebimento para o qual o emitente está enviando o evento.",
        [CancelamentoOficioImpedido] = "O Sistema Nacional NFS-e não pode recepecionar o EVENTO DE CANCELAMENTO DE NFS-e POR OFÍCIO pois, o evento de <nome_evento_vinculado_a_NFS-e> já está vinculado à NFS-e indicada no evento enviado, impedindo sua recepção.",
        [EventoAssinaturaInvalida] = "Arquivo enviado com erro na assinatura.",
        [EventoAssinaturaAusente] = "A assinatura é obrigatória quando for enviado para o Web Service.",
        [MarcaInvalida] = "Marca do emulador inválida no xInfComp.",
        [EventoChaveDivergente] = "A chave do caminho difere da chNFSe do pedido de registro de evento.",
        [EventoNaoAceito] = "O tipo de evento não é recebido pela Sefin Nacional do autor que emite (só o 101101 e o 101103).",
        [EventoInexistente] = "Não há evento da NFS-e com o tipo e o número sequencial informados.",
    };

    // A mensagem com o nome do evento que impede, no lugar de <nome_evento_vinculado_a_NFS-e>.
    public static string Texto(string codigo, string eventoVinculado) =>
        Textos[codigo].Replace("<nome_evento_vinculado_a_NFS-e>", eventoVinculado, StringComparison.Ordinal);

    public static string Texto(string codigo) => Textos[codigo];
}
