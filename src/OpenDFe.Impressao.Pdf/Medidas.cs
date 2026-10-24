namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// As medidas do PDF, cada uma com o item do manual que a fixa (docs/CONTRATOS.md 22 e docs/IMPRESSAO.md 3). Pontos
/// tipográficos nas fontes, milímetros nas dimensões.
/// </summary>
internal static class Medidas
{
    /// <summary>Pontos por milímetro (1 pt = 1/72 de polegada; 1 polegada = 25,4 mm).</summary>
    public const float PontosMilimetro = 72f / 25.4f;

    // ---- A bobina (DANFE NFC-e, Manual de Padrões Técnicos 6.0). O manual não fixa fonte nem posição (3.1.2: "não são
    // reguladas"): a escala segue a térmica de 203 dpi, em que a fonte A de 12 x 24 pontos põe 1,5 mm por coluna (48 colunas
    // em 72 mm, as do modelo) e a fonte B de 9 x 17 é 3/4 dela. O modelo conta as colunas assim (PapelImpressao.ColunasTamanho).

    /// <summary>Texto normal da bobina: o equivalente da fonte A da térmica.</summary>
    public const float BobinaNormal = 8f;

    /// <summary>Texto pequeno da bobina: 3/4 do normal (a fonte B da térmica, 4/3 das colunas).</summary>
    public const float BobinaPequeno = 6f;

    /// <summary>
    /// Texto duplo da bobina: 12 pt. A térmica dobra a largura e a altura (24 colunas de 48); a fonte proporcional é mais larga que
    /// a da térmica, e 12 pt é o maior tamanho em que a frase de 23 letras da contingência cabe numa linha de 72 mm.
    /// </summary>
    public const float BobinaDuplo = 12f;

    /// <summary>Margem vertical da bobina (a lateral é a do papel: o manual pede no mínimo 2 mm, 3.3).</summary>
    public const float BobinaMargemVerticalMm = 3f;

    /// <summary>Altura do papel da bobina que o QuestPDF deixa crescer (a altura é contínua, sem corte de página).</summary>
    public const float BobinaAlturaMinimaMm = 10f;

    // ---- A folha A4 (DANFE, Anexo II do MOC 7.0, 3.7 "Padrões de Caracteres": tamanhos mínimos em pontos).

    /// <summary>Descritivo dos blocos de campos: 5 pt, negrito (3.7.1).</summary>
    public const float FolhaTituloQuadro = 5f;

    /// <summary>Descritivo dos campos do quadro dos produtos: 5 pt (3.7.2).</summary>
    public const float FolhaCabecalhoTabela = 5f;

    /// <summary>Conteúdo dos campos do quadro dos produtos e das informações complementares: 6 pt (3.7.7 e 3.7.8).</summary>
    public const float FolhaCelulaTabela = 6f;

    /// <summary>Descritivo dos demais campos: 6 pt (3.7.3).</summary>
    public const float FolhaRotuloCampo = 6f;

    /// <summary>Conteúdo dos demais campos: 10 pt (3.7.9).</summary>
    public const float FolhaValorCampo = 10f;

    /// <summary>Conteúdo do campo das informações complementares: 6 pt (3.7.8).</summary>
    public const float FolhaTextoLivre = 6f;

    /// <summary>
    /// O rótulo do campo das informações complementares (Anexo II 3.1.8), o único cujo conteúdo é de 6 pt: o modelo não
    /// distingue o campo de texto livre dos demais, e o rótulo é o do Anexo II.
    /// </summary>
    public const string RotuloTextoLivre = "INFORMAÇÕES COMPLEMENTARES";

    /// <summary>O espaço que o quadro precisa ter no pé da página para começar nela: o título e o começo da primeira linha.</summary>
    public const float QuadroEspacoMinimoPt = 40f;

    /// <summary>Texto pequeno da folha (a consulta de autenticidade, o rodapé): 6 pt, o mínimo de 3.7.3.</summary>
    public const float FolhaPequeno = 6f;

    /// <summary>
    /// Texto normal da folha: 8 pt, o mínimo de "DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA" e dos dados do emitente
    /// (3.7.4 e 3.7.6).
    /// </summary>
    public const float FolhaNormal = 8f;

    /// <summary>Texto duplo da folha: 12 pt, o mínimo da descrição "DANFE" (3.7.4).</summary>
    public const float FolhaDuplo = 12f;

    /// <summary>Espessura das bordas dos quadros (linha fina).</summary>
    public const float BordaPt = 0.5f;

    /// <summary>O respiro interno do quadro, em milímetros.</summary>
    public const float QuadroRespiroMm = 0.8f;

    /// <summary>O respiro horizontal das células da tabela dos itens (colunas estreitas: cada décimo de milímetro conta).</summary>
    public const float CelulaRespiroHorizontalMm = 0.25f;

    /// <summary>A altura da linha, em múltiplos do tamanho da fonte (a Lato já traz o entrelinha dela).</summary>
    public const float Entrelinha = 1.15f;

    // ---- O QR Code (Manual do DANFE NFC-e 3.1.5 e 3.4).

    /// <summary>O lado mínimo do QR Code com a margem de silêncio: 25 mm (3.4).</summary>
    public const double QrCodeLadoMinimoMm = 25;

    /// <summary>A margem de silêncio até 25 mm: 3 mm (22 mm de conteúdo e 3 mm de margem, 3.4).</summary>
    public const double QrCodeSilencioMm = 3;

    /// <summary>A margem de silêncio acima de 25 mm: 10% do lado total (3.4).</summary>
    public const double QrCodeSilencioProporcao = 0.10;

    /// <summary>A zona de silêncio do ISO/IEC 18004: 4 módulos, que vale se a margem do manual for menor.</summary>
    public const int QrCodeSilencioModulos = 4;

    // ---- O código de barras (Anexo II 2 e 3.8.1).

    /// <summary>A largura máxima do código de barras da chave: 11,5 cm (3.8.1, "código de barras da chave").</summary>
    public const double CodigoBarrasLarguraMaximaMm = 115;

    /// <summary>Os 10 módulos de margem clara de cada lado do código de barras (Anexo II 2).</summary>
    public const int CodigoBarrasMargemModulos = 10;

    // ---- A imagem de 1 bit sem largura: tamanho natural a 203 dpi (a térmica).

    /// <summary>Os pontos por polegada da imagem de 1 bit de tamanho natural.</summary>
    public const double ImagemDpi = 203;

    // ---- A marca d'água (Anexo II 3.10.1: desde que não prejudique a legibilidade).

    /// <summary>O ângulo da marca d'água (graus, no sentido anti-horário), em diagonal.</summary>
    public const float MarcaAnguloGraus = -45f;

    /// <summary>O maior tamanho da marca d'água, em pontos.</summary>
    public const float MarcaTamanhoMaximo = 64f;

    /// <summary>A largura de cada letra da marca d'água, em múltiplos do tamanho (maiúsculas em negrito da Lato, com folga).</summary>
    public const float MarcaLarguraLetra = 0.8f;

    /// <summary>O cinza muito claro da marca d'água (não prejudica a leitura nem o código de barras).</summary>
    public const string MarcaCor = "#E6E6E6";
}
