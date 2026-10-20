namespace OpenDFe.SefazEmulada;

// Os códigos de resultado (cStat) que o emulador usa e o texto do xMotivo, como o MOC 7.0, Anexo I, 4.4.1
// e 4.4.2 os escreve (sem a referência à NT entre parênteses e sem os complementos entre colchetes, que
// quem monta o retorno acrescenta). O 764 não aparece na tabela 4.4.2 do PDF; o texto é o da regra GAP03a-1,
// em 4.1.3. Os códigos que as NTs criaram ou mudaram depois do MOC 7.0 levam o texto da NT: a 2025.001 v1.02
// (444, 452, 583) e a 2026.002 v1.11 (672, 704, 711).
internal static class Motivos
{
    public const string Autorizado = "100";
    public const string LoteRecebido = "103";
    public const string LoteProcessado = "104";
    public const string LoteProcessando = "105";
    public const string LoteNaoLocalizado = "106";
    public const string Operando = "107";
    public const string ParalisadoMomentaneamente = "108";
    public const string ParalisadoIndefinidamente = "109";
    public const string AutorizadoForaPrazo = "150";
    public const string Duplicidade = "204";
    public const string CnpjEmitenteInvalido = "207";
    public const string CnpjBaseDivergente = "213";
    public const string TamanhoExcedido = "214";
    public const string FalhaSchema = "215";
    public const string NotaInexistente = "217";
    public const string TransmissorDivergente = "223";
    public const string FalhaSchemaLote = "225";
    public const string UFEmitenteDivergente = "226";
    public const string CpfDivergente = "227";
    public const string EmissaoAtrasada = "228";
    public const string ChaveDigitoInvalido = "236";
    public const string VersaoNaoSuportada = "239";
    public const string UFReciboDivergente = "248";
    public const string AmbienteDivergente = "252";
    public const string DigitoChaveInvalido = "253";
    public const string UFDivergente = "289";
    public const string CertificadoAssinaturaInvalido = "290";
    public const string CertificadoDocumentoAusente = "292";
    public const string AssinaturaDiferente = "297";
    public const string AssinaturaForaPadrao = "298";
    public const string SuplementarNfe = "393";
    public const string QrCodeAusente = "394";
    public const string QrCodeDivergente = "397";
    public const string CpfEmitenteInvalido = "401";
    public const string PrefixoNamespace = "404";
    public const string QrCodePessoaFisica = "444";
    public const string AssincronoUmaNota = "452";
    public const string CscInexistente = "462";
    public const string CscRevogado = "463";
    public const string HashQrCodeDivergente = "464";
    public const string UFPrimeiraNotaDivergente = "476";
    public const string ChaveDivergenteCampos = "502";
    public const string RaizInexistente = "516";
    public const string VersaoInexistente = "517";
    public const string DuplicidadeChaveDiferente = "539";
    public const string MesChaveDivergente = "561";
    public const string CodigoNumericoDivergente = "562";
    public const string TipoAutorizadorDivergente = "553";
    public const string RaizLoteInexistente = "565";
    public const string VersaoLoteInexistente = "568";
    public const string TipoEmissaoContingencia = "570";
    public const string AssinaturaQrCodeDivergente = "583";
    public const string ChaveDivergenteBase = "613";
    public const string ChaveAnoInvalido = "615";
    public const string ChaveMesInvalido = "616";
    public const string ChaveDocumentoInvalido = "617";
    public const string ChaveModeloInvalido = "618";
    public const string ChaveNumeroInvalido = "619";
    public const string VersaoQrCodeSimplificado = "672";
    public const string EmissaoPosterior = "703";
    public const string NfceAtrasada = "704";
    public const string NfeOffline = "711";
    public const string TipoEmissaoSvc = "713";
    public const string SincronoVariasNotas = "764";
    public const string LoteMisturado = "765";
    public const string NfceSvc = "783";
    public const string VersaoQrCodeAntiga = "903";

    private static readonly Dictionary<string, string> Textos = new(StringComparer.Ordinal)
    {
        [Autorizado] = "Autorizado o uso da NF-e",
        [LoteRecebido] = "Lote recebido com sucesso",
        [LoteProcessado] = "Lote processado",
        [LoteProcessando] = "Lote em processamento",
        [LoteNaoLocalizado] = "Lote não localizado",
        [Operando] = "Serviço em Operação",
        [ParalisadoMomentaneamente] = "Serviço Paralisado Momentaneamente (curto prazo)",
        [ParalisadoIndefinidamente] = "Serviço Paralisado sem Previsão",
        [AutorizadoForaPrazo] = "Autorizado o uso da NF-e, autorização fora de prazo",
        [Duplicidade] = "Rejeição: Duplicidade de NF-e",
        [CnpjEmitenteInvalido] = "Rejeição: CNPJ do emitente inválido",
        [CnpjBaseDivergente] = "Rejeição: CNPJ-Base do Emitente difere do CNPJ-Base do Certificado Digital",
        [TamanhoExcedido] = "Rejeição: Tamanho da mensagem excedeu o limite estabelecido",
        [FalhaSchema] = "Rejeição: Falha no schema XML",
        [NotaInexistente] = "Rejeição: NF-e não consta na base de dados da SEFAZ",
        [TransmissorDivergente] = "Rejeição: CNPJ/CPF do transmissor do lote difere do CNPJ/CPF do transmissor da consulta",
        [FalhaSchemaLote] = "Rejeição: Falha no Schema XML do lote de NFe",
        [UFEmitenteDivergente] = "Rejeição: Código da UF do Emitente diverge da UF autorizadora",
        [CpfDivergente] = "Rejeição: CPF do Emitente difere do CPF do Certificado Digital",
        [EmissaoAtrasada] = "Rejeição: Data de Emissão muito atrasada",
        [ChaveDigitoInvalido] = "Rejeição: Chave de Acesso com dígito verificador inválido",
        [VersaoNaoSuportada] = "Rejeição: Versão do arquivo XML não suportada",
        [UFReciboDivergente] = "Rejeição: UF do Recibo diverge da UF autorizadora",
        [AmbienteDivergente] = "Rejeição: Ambiente informado diverge do Ambiente de recebimento",
        [DigitoChaveInvalido] = "Rejeição: Digito Verificador da chave de acesso composta inválida",
        [UFDivergente] = "Rejeição: Código da UF informada diverge da UF solicitada",
        [CertificadoAssinaturaInvalido] = "Rejeição: Certificado Assinatura inválido",
        [CertificadoDocumentoAusente] = "Rejeição: Certificado de Assinatura sem CNPJ/CPF",
        [AssinaturaDiferente] = "Rejeição: Assinatura difere do calculado",
        [AssinaturaForaPadrao] = "Rejeição: Assinatura difere do padrão do Sistema",
        [SuplementarNfe] = "Rejeição: NF-e com o grupo de Informações Suplementares",
        [QrCodeAusente] = "Rejeição: Nota Fiscal sem a informação do QR-Code",
        [QrCodeDivergente] = "Rejeição: Parâmetro do QR-Code divergente da Nota Fiscal",
        [CpfEmitenteInvalido] = "Rejeição: CPF do emitente inválido",
        [PrefixoNamespace] = "Rejeição: Uso de prefixo de namespace não permitido",
        [QrCodePessoaFisica] = "Rejeição: NFC-e com qrCode na versão 2 para Pessoa Física",
        [AssincronoUmaNota] = "Rejeição: Solicitada resposta assíncrona para Lote com somente 1 (uma) NF-e",
        [CscInexistente] = "Rejeição: Código Identificador do CSC no QR-Code não cadastrado na SEFAZ",
        [CscRevogado] = "Rejeição: Código Identificador do CSC no QR-Code foi revogado pela empresa",
        [HashQrCodeDivergente] = "Rejeição: Código de Hash no QR-Code difere do calculado",
        [UFPrimeiraNotaDivergente] = "Rejeição: Código da UF diverge da UF da primeira NF-e do Lote",
        [ChaveDivergenteCampos] = "Rejeição: Erro na Chave de Acesso - Campo Id não corresponde à concatenação dos campos correspondentes",
        [RaizInexistente] = "Rejeição: Falha no schema XML – inexiste a tag raiz esperada para a mensagem",
        [VersaoInexistente] = "Rejeição: Falha no schema XML – inexiste atributo versao na tag raiz da mensagem",
        [DuplicidadeChaveDiferente] = "Rejeição: Duplicidade de NF-e com diferença na Chave de Acesso",
        [MesChaveDivergente] = "Rejeição: Mês de Emissão informado na Chave de Acesso difere do Mês de Emissão da NF-e",
        [CodigoNumericoDivergente] = "Rejeição: Código Numérico informado na Chave de Acesso difere do Código Numérico da NF-e",
        [TipoAutorizadorDivergente] = "Rejeição: Tipo autorizador do recibo diverge do Órgão Autorizador.",
        [RaizLoteInexistente] = "Rejeição: Falha no schema XML – inexiste a tag raiz esperada para o lote de NF-e",
        [VersaoLoteInexistente] = "Rejeição: Falha no schema XML – inexiste atributo versao na tag raiz do lote de NF-e",
        [TipoEmissaoContingencia] = "Rejeição: Tipo de Emissão 3, 6 ou 7 só é válido nas contingências SCAN/SVC",
        [AssinaturaQrCodeDivergente] = "Rejeição: Valor da assinatura do qrCode difere do valor calculado",
        [ChaveDivergenteBase] = "Rejeição: Chave de Acesso difere da existente em BD",
        [ChaveAnoInvalido] = "Rejeição: Chave de Acesso inválida (Ano < 06 ou Ano maior que Ano Corrente)",
        [ChaveMesInvalido] = "Rejeição: Chave de Acesso inválida (Mês < 1 ou Mês > 12)",
        [ChaveDocumentoInvalido] = "Rejeição: Chave de Acesso inválida (CNPJ/CPF zerado ou dígito inválido)",
        [ChaveModeloInvalido] = "Rejeição: Chave de Acesso inválida (modelo diferente de 55 e 65)",
        [ChaveNumeroInvalido] = "Rejeição: Chave de Acesso inválida (número NF = 0)",
        [VersaoQrCodeSimplificado] = "Rejeição: NF-e com DANFE Simplificado Tipo 2 com versão de qrCode não permitida",
        [EmissaoPosterior] = "Rejeição: Data-Hora de Emissão posterior ao horário de recebimento",
        [NfceAtrasada] = "Rejeição: NFC-e ou NF-e com DANFE Simplificado Tipo 2 com Data-Hora de emissão atrasada",
        [NfeOffline] = "Rejeição: NF-e com contingência off-line em operação não permitida",
        [TipoEmissaoSvc] = "Rejeição: Tipo de Emissão diferente de 6 ou 7 para contingência da SVC acessada",
        [SincronoVariasNotas] = "Rejeição: Solicitada resposta síncrona para Lote com mais de uma NF-e (indSinc=1)",
        [LoteMisturado] = "Rejeição: Lote só poderá conter NF-e ou NFC-e",
        [NfceSvc] = "Rejeição: NFC-e não é autorizada pela SVC",
        [VersaoQrCodeAntiga] = "Rejeição Versão informada no QR-Code (“100”) não é mais válida para a data de emissão",
    };

    public static string Texto(string codigo) => Textos[codigo];
}
