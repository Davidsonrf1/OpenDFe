using System.Globalization;
using OpenDFe.Impressao;
using static OpenDFe.NFSe.FormatadorDanfse;

namespace OpenDFe.NFSe;

/// <summary>
/// Monta o DANFSe (NT SE/CGNFS-e 008/2026 v1.02) da NFS-e lida: um <see cref="FormularioImpressao"/> com os quadros nas
/// posições do item 2.4.5 e na disposição do Anexo I, numa folha A4 só (docs/NFSE.md 12; a conformidade item a item está em
/// docs/IMPRESSAO.md 5). Só lê a nota: não confere o schema nem as regras, e não imprime o que o XML não tem (2.1).
/// </summary>
/// <remarks>
/// As medidas estão em centímetros a partir da borda do papel, como na NT, e viram milímetros a partir do canto da área útil
/// do <see cref="PapelImpressao.A4RetratoEstreito"/> (margem de 1,5 mm) na hora de criar o quadro.
/// </remarks>
internal static class MontagemDanfse
{
    private const string UrlConsulta = "https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=";
    private const string TextoQrCode = "A autenticidade desta NFS-e pode ser verificada pela leitura deste código QR ou pela consulta da chave de acesso no portal nacional da NFS-e";
    private const string FraseHomologacao = "NFS-e SEM VALIDADE JURÍDICA";
    private const string TomadorAusente = "TOMADOR/ADQUIRENTE DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e";
    private const string DestinatarioAusente = "DESTINATÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e";
    private const string DestinatarioTomador = "O DESTINATÁRIO É O PRÓPRIO TOMADOR/ADQUIRENTE DA OPERAÇÃO";
    private const string IntermediarioAusente = "INTERMEDIÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e";
    private const string IssqnAusente = "TRIBUTAÇÃO MUNICIPAL (ISSQN) - OPERAÇÃO NÃO SUJEITA AO ISSQN";
    private const string LeiTributos = "Totais Aproximados dos Tributos cfe. Lei nº 12.741/2012: ";

    // A margem do papel (1,5 mm), em cm: a origem das coordenadas dos quadros.
    private const double Margem = 0.15;

    // As quatro colunas do Anexo I e as larguras (2.4.5).
    private static readonly double[] Colunas = [0.30, 5.41, 10.51, 15.62];
    private const double Esquerda = 0.30;
    private const double LarguraColuna = 5.09;
    private const double LarguraDupla = 10.19;
    private const double LarguraTotal = 20.40;

    // As linhas dos blocos: 0,63 de altura, com o passo médio da tabela (de 4,34 a 12,74 são 13 linhas; de 14,43 a 20,90, 10),
    // que reproduz cada posição dela até 0,02 cm; as do valor total, 0,67 a cada 0,69; o bloco suprimido (notas 2 a 4), 0,32.
    private const double PassoLinha = 0.6465;
    private const double AlturaLinha = 0.63;
    private const double PassoTotal = 0.69;
    private const double AlturaTotal = 0.67;
    private const double AlturaAviso = 0.32;
    private const double PassoAviso = 0.34;

    // O começo dos blocos depois da identificação, o canhoto e o fim do formulário (2.4.5).
    private const double InicioBlocos = 4.34;
    private const double TopoCanhoto = 28.10;
    private const double FimFormulario = 28.77;

    // A estimativa da altura do texto livre (a descrição do serviço e as informações complementares), para dividir entre os
    // dois a altura que sobra: 7 pt de conteúdo (0,284 cm por linha), 6 pt de rótulo, e 150 caracteres numa linha de 20,40 cm
    // (a Lato de 7 pt põe uns 170; a folga cobre as palavras que não cabem no fim da linha). O PDF corta o que ainda passar.
    private const int CaracteresLinha = 150;
    private const double LinhaConteudo = 7 * 1.15 / 72 * 2.54;
    private const double LinhaRotulo = 6 * 1.15 / 72 * 2.54;
    private const double RespiroTexto = 0.06;
    private const double AlturaDescricaoMinima = 0.64;
    private const double AlturaInformacoesMinima = 0.39;

    private static readonly Lazy<byte[]> Logomarca = new(() => Recurso("Recursos/LogomarcaNFSe.png"));

    internal static DocumentoImpressao Montar(NFSeLida nota, OpcoesDanfse? opcoes)
    {
        ArgumentNullException.ThrowIfNull(nota);
        opcoes ??= new OpcoesDanfse();
        if (opcoes.Cancelada && opcoes.Substituida)
            throw new ArgumentException("A NFS-e não pode estar cancelada e substituída ao mesmo tempo.", nameof(opcoes));
        var lida = nota.Nota ?? throw new ArgumentException(
            "A leitura não achou a NFS-e (a DPS sozinha não tem DANFSe): não há o que imprimir.", nameof(nota));
        var chave = nota.Chave ?? throw new ArgumentException("A NFS-e não tem a chave de acesso (Id do infNFSe) válida.", nameof(nota));
        return Compor(lida, chave, opcoes);
    }

    private static DocumentoImpressao Compor(NotaFiscalServico nota, ChaveNFSe chave, OpcoesDanfse opcoes)
    {
        var folha = new Folha();
        Cabecalho(folha, nota);
        Identificacao(folha, nota, chave);

        var secoes = new List<Secao>
        {
            Prestador(nota),
            Tomador(nota.Dps),
            Destinatario(nota.Dps),
            Intermediario(nota.Dps),
            Servico(nota),
        };
        var descricao = Cortar(Texto(nota.Dps.Servico.Codigo.Descricao), 1300);
        var depois = new List<Secao> { Issqn(nota), Federal(nota.Dps), IbsCbs(nota), Total(nota) };
        var (informacoes, tributos) = Informacoes(nota);

        // A altura que sobra vai à descrição do serviço e às informações complementares (2.3), pela necessidade estimada de
        // cada uma; sem caber as duas, na proporção dela.
        var fim = opcoes.Canhoto ? TopoCanhoto - 0.02 : FimFormulario;
        var fixo = secoes.Sum(s => s.Altura) + depois.Sum(s => s.Altura) + 0.41;
        var livre = fim - InicioBlocos - fixo;
        var precisaDescricao = Math.Max(AlturaDescricaoMinima, Estimar(descricao, rotulo: true) + 0.02);
        var alturaTributos = RespiroTexto + LinhaConteudo + 0.02;
        var precisaInformacoes = Math.Max(AlturaInformacoesMinima, Estimar(informacoes, rotulo: false)) + alturaTributos;
        var alturaDescricao = precisaDescricao + precisaInformacoes <= livre
            ? precisaDescricao
            : Math.Max(AlturaDescricaoMinima, livre * precisaDescricao / (precisaDescricao + precisaInformacoes));

        var y = InicioBlocos;
        foreach (var secao in secoes)
        {
            secao.Desenhar(folha, y);
            y += secao.Altura;
        }

        folha.Texto(Esquerda, y, LarguraTotal, alturaDescricao - 0.02, "Descrição do Serviço", descricao);
        y += alturaDescricao;
        foreach (var secao in depois)
        {
            secao.Desenhar(folha, y);
            y += secao.Altura;
        }

        // As informações complementares: o título, o texto e, embaixo, a linha dos totais aproximados, que nunca se corta (nota 10).
        folha.Titulo(y, 0.39, "INFORMAÇÕES COMPLEMENTARES");
        folha.Divisoria(y);
        y += 0.41;
        var alturaInformacoes = fim - y;
        if (informacoes.Length > 0)
            folha.Texto(Esquerda, y, LarguraTotal, Math.Max(0.1, alturaInformacoes - alturaTributos), null, informacoes);
        folha.Texto(Esquerda, fim - alturaTributos, LarguraTotal, alturaTributos, null, tributos);

        if (opcoes.Canhoto)
            Canhoto(folha, nota, chave);

        var marca = opcoes.Cancelada ? "CANCELADA" : opcoes.Substituida ? "SUBSTITUÍDA" : null;
        return new DocumentoImpressao(PapelImpressao.A4RetratoEstreito, [new FormularioImpressao(folha.Quadros) { BordaPt = 1, MarcaAgua = marca }])
        {
            Titulo = "DANFSe NFS-e " + Numero(nota.Numero ?? chave.Numero),
        };
    }

    // O cabeçalho (2.4.3): a logomarca à esquerda, "DANFSe v2.0" e "Documento Auxiliar da NFS-e" ao centro (com a frase da
    // produção restrita em vermelho embaixo), e o município, o ambiente gerador e o tipo de ambiente à direita.
    private static void Cabecalho(Folha folha, NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        folha.Adicionar(new TextoFormulario(Mm(Esquerda), Mm(0.30), LarguraTotal * 10, 11.6, null, null) { Sombreado = true, Bordas = BordaQuadro.Baixo });
        folha.Adicionar(new ImagemFormulario(Mm(0.49), Mm(0.44), 40, 8.5, Logomarca.Value));
        var homologacao = dps.Ambiente == Ambiente.Homologacao;
        folha.Adicionar(new TextoFormulario(Mm(5.41), Mm(homologacao ? 0.30 : 0.45), 101.9, 8.0, null, "DANFSe v2.0\nDocumento Auxiliar da NFS-e")
        {
            TamanhoValorPt = 9,
            ValorNegrito = true,
            Alinhamento = AlinhamentoImpressao.Centro,
        });
        if (homologacao)
        {
            folha.Adicionar(new TextoFormulario(Mm(5.41), Mm(1.08), 101.9, 3.8, null, FraseHomologacao)
            {
                TamanhoValorPt = 9,
                ValorNegrito = true,
                Alinhamento = AlinhamentoImpressao.Centro,
                Cor = CorImpressao.Vermelho,
            });
        }

        // O município não sai quando o item do código de tributação nacional é 99 (2.4.5).
        if (dps.Servico.Codigo.TributacaoNacional?.StartsWith("99", StringComparison.Ordinal) != true)
        {
            var uf = nota.Emitente.Endereco.UF ?? (dps.MunicipioEmissor is { } emissor ? MunicipiosIbge.Estado(emissor) : null);
            var municipio = Juntar(" / ", nota.NomeMunicipioEmissor, uf?.Sigla());
            folha.Adicionar(new TextoFormulario(Mm(15.62), Mm(0.30), 50.9, 5.8, null, "Município: " + Cortar(Texto(municipio), 37)) { TamanhoValorPt = 8 });
        }

        folha.Adicionar(new TextoFormulario(Mm(15.62), Mm(0.88), 50.9, 2.9, null, "Ambiente Gerador: " + TextosDanfse.AmbienteGerador(nota.AmbienteGerador)) { TamanhoValorPt = 6 });
        folha.Adicionar(new TextoFormulario(Mm(15.62), Mm(1.17), 50.9, 2.9, null, "Tipo de Ambiente: " + TextosDanfse.Ambiente(dps.Ambiente)) { TamanhoValorPt = 6 });
    }

    // Os dados da NFS-e (2.1.2): a chave, as três linhas de identificação, o QR Code e o texto dele.
    private static void Identificacao(Folha folha, NotaFiscalServico nota, ChaveNFSe chave)
    {
        var dps = nota.Dps;
        folha.Identificacao(Esquerda, 1.48, 15.30, 0.77, "CHAVE DE ACESSO DA NFS-E", chave.Valor);
        folha.Identificacao(Colunas[0], 2.27, LarguraColuna, 0.67, "NÚMERO DA NFS-E", Numero(nota.Numero ?? chave.Numero));
        folha.Identificacao(Colunas[1], 2.27, LarguraColuna, 0.67, "COMPETÊNCIA DA NFS-E", Data(dps.Competencia));
        folha.Identificacao(Colunas[2], 2.27, LarguraColuna, 0.67, "DATA E HORA DA EMISSÃO DA NFS-E", DataHora(nota.Processamento));
        folha.Identificacao(Colunas[0], 2.96, LarguraColuna, 0.67, "NÚMERO DA DPS", Numero(dps.Numero));
        folha.Identificacao(Colunas[1], 2.96, LarguraColuna, 0.67, "SÉRIE DA DPS", Cortar(Texto(dps.Serie), 5));
        folha.Identificacao(Colunas[2], 2.96, LarguraColuna, 0.67, "DATA E HORA DA EMISSÃO DA DPS", DataHora(dps.Emissao));
        folha.Identificacao(Colunas[0], 3.65, LarguraColuna, 0.67, "EMITENTE DA NFS-E", Cortar(TextosDanfse.Emitente(dps.Emitente), 13), sombreado: true);
        folha.Identificacao(Colunas[1], 3.65, LarguraColuna, 0.67, "SITUAÇÃO DA NFS-E", Cortar(TextosDanfse.Situacao(nota.Situacao), 40));
        folha.Identificacao(Colunas[2], 3.65, LarguraColuna, 0.67, "FINALIDADE", Cortar(TextosDanfse.Finalidade(dps.IbsCbs?.Finalidade), 40));
        folha.Adicionar(new QrCodeFormulario(Mm(17.48), Mm(1.67), 15.2, UrlConsulta + chave.Valor));
        folha.Adicionar(new TextoFormulario(Mm(15.80), Mm(3.36), 47.2, 9.6, null, TextoQrCode) { TamanhoValorPt = 6 });
    }

    private static Secao Prestador(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var prest = dps.Prestador;
        var emit = nota.Emitente;
        // Com o prestador como emitente (tpEmit 1, o único que a Sefin aceita), o ambiente nacional tira do cadastro o nome e
        // o endereço, que a DPS não traz (NFSE-PRESTADOR): o DANFSe os lê do emit da NFS-e.
        var doEmit = dps.Emitente is null or EmitenteDps.Prestador;
        // O documento é o do prestador quando a DPS tem algum (o CNPJ, o CPF ou o NIF); o do emit só na falta dos três.
        var temDocumento = !string.IsNullOrEmpty(prest.Cnpj) || !string.IsNullOrEmpty(prest.Cpf) || !string.IsNullOrEmpty(prest.Nif);
        var documento = temDocumento || !doEmit ? Documento(prest.Cnpj, prest.Cpf, prest.Nif) : Documento(emit.Cnpj, emit.Cpf, null);
        var parte = new Parte(
            documento,
            prest.InscricaoMunicipal ?? (doEmit ? emit.InscricaoMunicipal : null),
            prest.Telefone ?? (doEmit ? emit.Telefone : null),
            prest.Nome ?? (doEmit ? emit.Nome : null),
            prest.Endereco is { } endereco ? Local(endereco) : doEmit ? Local(emit.Endereco) : null,
            prest.Email ?? (doEmit ? emit.Email : null),
            TemInscricao: true);
        var regime = prest.RegimeTributacao;
        var linhas = LinhasParte(parte);
        linhas.Add(
        [
            new Celula(0, 1, "Simples Nacional na Data de Competência", Cortar(TextosDanfse.Simples(regime.SimplesNacional), 40)),
            new Celula(1, 2, "Regime de Apuração Tributária pelo SN", Cortar(TextosDanfse.ApuracaoSimples(regime.ApuracaoSimples), 80)),
        ]);
        return Bloco("PRESTADOR / FORNECEDOR", linhas);
    }

    private static Secao Tomador(Dps dps) => dps.Tomador is { } toma
        ? Bloco("TOMADOR / ADQUIRENTE", LinhasParte(Pessoa(toma, temInscricao: true)))
        : Aviso(TomadorAusente);

    // O destinatário (IBSCBS/dest): o bloco com os dados; a frase quando é o próprio tomador (indDest 0, 2.3.2); a frase de
    // ausência sem nenhum dos dois.
    private static Secao Destinatario(Dps dps)
    {
        var ibsCbs = dps.IbsCbs;
        if (ibsCbs?.Destinatario is { } dest)
        {
            var parte = new Parte(Documento(dest.Cnpj, dest.Cpf, dest.Nif), null, dest.Telefone, dest.Nome,
                dest.Endereco is { } endereco ? Local(endereco) : null, dest.Email, TemInscricao: false);
            return Bloco("DESTINATÁRIO DA OPERAÇÃO", LinhasParte(parte));
        }

        return ibsCbs?.IndicadorDestinatario == IndicadorDestinatario.Tomador && dps.Tomador is not null
            ? Aviso(DestinatarioTomador)
            : Aviso(DestinatarioAusente);
    }

    private static Secao Intermediario(Dps dps) => dps.Intermediario is { } interm
        ? Bloco("INTERMEDIÁRIO DA OPERAÇÃO", LinhasParte(Pessoa(interm, temInscricao: true)))
        : Aviso(IntermediarioAusente);

    // O serviço prestado (2.1.7): os códigos e o local; a descrição do código de tributação, numa linha sem rótulo. A
    // descrição do serviço é desenhada à parte, com a altura que sobrar.
    private static Secao Servico(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var codigo = dps.Servico.Codigo;
        var codigos = Juntar(" / ",
            codigo.TributacaoNacional is { Length: > 0 } nacional ? TributacaoNacional(nacional) : null,
            codigo.TributacaoMunicipal);
        var local = dps.Servico.Local;
        var lugar = local.Municipio is { } municipio
            ? Juntar(" / ", nota.NomeLocalPrestacao ?? MunicipiosIbge.Nome(municipio), MunicipiosIbge.Estado(municipio)?.Sigla(), "BR")
            : Juntar(" / ", nota.NomeLocalPrestacao, local.Pais);
        var descricaoCodigo = !string.IsNullOrWhiteSpace(nota.DescricaoTributacaoMunicipal)
            ? nota.DescricaoTributacaoMunicipal
            : nota.DescricaoTributacaoNacional;
        var bloco = Bloco("SERVIÇO PRESTADO",
        [
            [
                new Celula(1, 1, "Código de Tributação Nacional / Municipal", Cortar(Texto(codigos), 14)),
                new Celula(2, 1, "Código da NBS", Nbs(codigo.Nbs)),
                new Celula(3, 1, "Local da Prestação / Sigla UF / País", Cortar(Texto(lugar), 42)),
            ],
        ]);
        return new Secao(bloco.Altura + 0.40, (folha, y) =>
        {
            bloco.Desenhar(folha, y);
            folha.Texto(Esquerda, y + bloco.Altura, LarguraTotal, 0.38, null, Cortar(Texto(descricaoCodigo), 170));
        });
    }

    // A tributação municipal (2.1.8). Sem incidência do ISSQN (tribISSQN 4), a frase da nota 4. As linhas da nota 5 (o regime
    // especial e a suspensão; o benefício e as deduções) somem quando não têm dado nenhum; o regime "Nenhum" (0) não é dado.
    private static Secao Issqn(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var mun = dps.Valores.Tributacao.Municipal;
        if (mun.Tributacao == TributacaoIssqn.NaoIncidencia)
            return Aviso(IssqnAusente);
        var valores = nota.Valores;
        var regime = dps.Prestador.RegimeTributacao.RegimeEspecial;
        var incidencia = Juntar(" / ", nota.NomeMunicipioIncidencia,
            nota.MunicipioIncidencia is { } codigo ? MunicipiosIbge.Estado(codigo)?.Sigla() : null,
            mun.PaisResultado ?? (nota.MunicipioIncidencia is null ? null : "BR"));
        var linhas = new List<IReadOnlyList<Celula>>
        {
            new[]
            {
                new Celula(1, 1, "Tipo de Tributação do ISSQN", Cortar(TextosDanfse.TributacaoIssqn(mun.Tributacao), 21)),
                new Celula(2, 2, "Município / Sigla UF / País da Incidência do ISSQN", Cortar(Texto(incidencia), 42)),
            },
        };
        var suspensao = mun.ExigibilidadeSuspensa;
        if (regime is not (null or RegimeEspecialTributacao.Nenhum) || mun.Imunidade is not null || suspensao is not null)
        {
            linhas.Add(
            [
                new Celula(0, 1, "Regime Especial de Tributação do ISSQN", Cortar(TextosDanfse.RegimeEspecial(regime), 27)),
                new Celula(1, 1, "Tipo de Imunidade do ISSQN", Cortar(TextosDanfse.Imunidade(mun.Imunidade), 40)),
                new Celula(2, 1, "Suspensão da Exigibilidade do ISSQN", Cortar(TextosDanfse.Suspensao(suspensao?.Tipo), 40)),
                new Celula(3, 1, "Número Processo Suspensão", Cortar(Texto(suspensao?.Processo), 30)),
            ]);
        }

        var calculoBeneficio = valores.ReducaoBeneficio ?? mun.Beneficio?.ValorReducao;
        var deducoes = Somar(valores.DeducaoReducao ?? dps.Valores.DeducaoReducao?.Valor, nota.IbsCbs?.Valores.ReembolsoRepasse);
        var incondicionado = dps.Valores.Descontos?.Incondicionado;
        if (valores.TipoBeneficio is not null || calculoBeneficio is not null || deducoes is not null || incondicionado is not null)
        {
            linhas.Add(
            [
                new Celula(0, 1, "Benefício Municipal", Cortar(TextosDanfse.Beneficio(valores.TipoBeneficio), 40)),
                new Celula(1, 1, "Cálculo do BM", Dinheiro(calculoBeneficio)),
                new Celula(2, 1, "Total Deduções/Reduções", Dinheiro(deducoes)),
                new Celula(3, 1, "Desconto Incondicionado", Dinheiro(incondicionado)),
            ]);
        }

        linhas.Add(
        [
            new Celula(0, 1, "BC ISSQN", Dinheiro(valores.BaseCalculo)),
            new Celula(1, 1, "Alíquota Aplicada", Percentual(valores.AliquotaAplicada)),
            new Celula(2, 1, "Retenção do ISSQN", Cortar(TextosDanfse.RetencaoIssqn(mun.Retencao), 25)),
            new Celula(3, 1, "ISSQN Apurado", Dinheiro(valores.Issqn)),
        ]);
        return Bloco("TRIBUTAÇÃO MUNICIPAL (ISSQN)", linhas);
    }

    // A tributação federal (2.1.9). As contribuições sociais retidas somam o PIS e a COFINS com o tpRetPisCofins 1, e a linha
    // do PIS e da COFINS de apuração própria só sai com a competência até o fim de 2026 (2.4.5 e a nota 6).
    private static Secao Federal(Dps dps)
    {
        var federal = dps.Valores.Tributacao.Federal;
        var pisCofins = federal?.PisCofins;
        var retidos = pisCofins?.Retencao == RetencaoPisCofins.PisCofinsRetidos;
        var sociais = retidos ? Somar(federal?.RetencaoCsll, pisCofins?.ValorPis, pisCofins?.ValorCofins) : federal?.RetencaoCsll;
        var linhas = new List<IReadOnlyList<Celula>>
        {
            new[]
            {
                new Celula(1, 1, "IRRF", Dinheiro(federal?.RetencaoIrrf)),
                new Celula(2, 1, "Contribuição Previdenciária - Retida", Dinheiro(federal?.RetencaoCp)),
                new Celula(3, 1, "Contribuições Sociais - Retidas", Dinheiro(sociais)),
            },
        };
        if (dps.Competencia is not { Year: > 2026 })
        {
            linhas.Add(
            [
                new Celula(0, 1, "PIS - Débito Apuração Própria", Dinheiro(retidos ? 0m : pisCofins?.ValorPis)),
                new Celula(1, 1, "COFINS - Débito Apuração Própria", Dinheiro(retidos ? 0m : pisCofins?.ValorCofins)),
                new Celula(2, 2, "Descrição Contrib. Sociais - Retidas", Cortar(TextosDanfse.RetencaoPisCofins(pisCofins?.Retencao), 35)),
            ]);
        }

        return Bloco("TRIBUTAÇÃO FEDERAL (EXCETO CBS)", linhas);
    }

    // A tributação do IBS e da CBS (2.1.10); na NFS-e 1.00 (sem o grupo), os campos saem com o traço.
    private static Secao IbsCbs(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var grupo = nota.IbsCbs;
        var valores = grupo?.Valores;
        var totais = grupo?.Totais;
        var classificacao = dps.IbsCbs?.Valores.Tributos.Classificacao;
        var cst = Juntar(" / ", classificacao?.Cst, classificacao?.ClassificacaoTributaria);
        var incidencia = Juntar(" / ", dps.IbsCbs?.CodigoOperacao,
            grupo?.MunicipioIncidencia?.ToString(CultureInfo.InvariantCulture), grupo?.NomeMunicipioIncidencia,
            grupo?.MunicipioIncidencia is { } codigo ? MunicipiosIbge.Estado(codigo)?.Sigla() : null);
        var federal = dps.Valores.Tributacao.Federal?.PisCofins;
        var exclusoes = Somar(dps.Valores.Descontos?.Incondicionado, valores?.ReembolsoRepasse, nota.Valores.Issqn,
            federal?.ValorPis, federal?.ValorCofins);
        return Bloco("TRIBUTAÇÃO IBS / CBS",
        [
            [
                new Celula(1, 1, "CST / cClassTrib", Cortar(Texto(cst), 12)),
                new Celula(2, 2, "Indicador de Operação / Código IBGE Incidência / Município Incidência / Sigla UF", Cortar(Texto(incidencia), 56)),
            ],
            [
                new Celula(0, 1, "Exclusões e Reduções da Base de Cálculo", Dinheiro(exclusoes)),
                new Celula(1, 1, "Base de Cálculo Após Exclusões e Reduções", Dinheiro(valores?.BaseCalculo)),
                new Celula(2, 1, "Red. Alíquota IBS / Red. Alíquota CBS",
                    $"{Percentual(valores?.UF.Reducao)} / {Percentual(valores?.Municipio.Reducao)} / {Percentual(valores?.Cbs.Reducao)}"),
                new Celula(3, 1, "Alíquota - IBS UF / IBS Mun", $"{Percentual(valores?.UF.Aliquota)} / {Percentual(valores?.Municipio.Aliquota)}"),
            ],
            [
                new Celula(0, 1, "Alíq. Efetiva Municipal - IBS", Percentual(valores?.Municipio.Efetiva)),
                new Celula(1, 1, "Valor Apurado Municipal - IBS", Dinheiro(totais?.Ibs.Municipio.Valor)),
                new Celula(2, 1, "Alíq. Efetiva Estadual - IBS", Percentual(valores?.UF.Efetiva)),
                new Celula(3, 1, "Valor Apurado Estadual - IBS", Dinheiro(totais?.Ibs.UF.Valor)),
            ],
            [
                new Celula(0, 1, "Valor Total Apurado - IBS", Dinheiro(totais?.Ibs.Total)),
                new Celula(1, 1, "Alíquota - CBS", Percentual(valores?.Cbs.Aliquota)),
                new Celula(2, 1, "Alíquota Efetiva - CBS", Percentual(valores?.Cbs.Efetiva)),
                new Celula(3, 1, "Valor Total Apurado - CBS", Dinheiro(totais?.Cbs.Valor)),
            ],
        ]);
    }

    // O valor total (2.1.11): os rótulos em caixa alta e 7 pt nos três valores que o Anexo I destaca, e o líquido com o
    // IBS/CBS sombreado (2.2.3).
    private static Secao Total(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var totais = nota.IbsCbs?.Totais;
        var ibsCbs = Somar(totais?.Ibs.Total, totais?.Cbs.Valor);
        return Bloco("VALOR TOTAL DA NFS-E",
        [
            [
                new Celula(1, 1, "VALOR DA OPERAÇÃO / SERVIÇO", Dinheiro(dps.Valores.Servico.Valor), Destaque: true),
                new Celula(2, 1, "Desconto Incondicionado", Dinheiro(dps.Valores.Descontos?.Incondicionado)),
                new Celula(3, 1, "Desconto Condicionado", Dinheiro(dps.Valores.Descontos?.Condicionado)),
            ],
            [
                new Celula(0, 1, "Total das Retenções (ISSQN / Federais)", Dinheiro(nota.Valores.TotalRetencoes)),
                new Celula(1, 1, "VALOR LÍQUIDO DA NFS-e", Dinheiro(nota.Valores.Liquido), Destaque: true),
                new Celula(2, 1, "Total do IBS/CBS", Dinheiro(ibsCbs)),
                new Celula(3, 1, "VALOR LÍQUIDO DA NFS-e + IBS/CBS", Dinheiro(totais?.TotalNota), Destaque: true, Sombreado: true),
            ],
        ], PassoTotal, AlturaTotal);
    }

    // As informações complementares (2.1.12 e o 2.4.5): os campos na ordem da NT, separados por pipes e cortados em 1.997
    // caracteres; e a linha dos totais aproximados dos tributos à parte, que é fixa (nota 10).
    private static (string Informacoes, string Tributos) Informacoes(NotaFiscalServico nota)
    {
        var dps = nota.Dps;
        var servico = dps.Servico;
        var compl = servico.InformacoesComplementares;
        var partes = new List<string>();
        void Parte(string rotulo, string? valor)
        {
            if (!string.IsNullOrWhiteSpace(valor))
                partes.Add(rotulo + " " + valor.Trim());
        }

        Parte("Inf. Cont.:", compl?.Texto);
        Parte("NFS-e Subst.:", dps.Substituicao?.Chave);
        Parte("Doc. Ref.:", compl?.DocumentoReferencia);
        Parte("Cod. Obra:", servico.Obra?.Codigo);
        Parte("Insc. Imob.:", servico.Obra?.InscricaoImobiliaria ?? dps.IbsCbs?.Imovel?.InscricaoImobiliaria);
        Parte("Cod. Evt.:", servico.Evento?.Identificacao);
        Parte("Doc. Tec.:", compl?.ResponsabilidadeTecnica);
        Parte("Núm. Ped.:", compl?.Pedido);
        Parte("Item Ped.:", compl?.ItensPedido is { Itens.Count: > 0 } itens ? string.Join("; ", itens.Itens) : null);
        Parte("Inf. A. T. Mun.:", nota.InformacoesFisco);
        var informacoes = Cortar(string.Join(" | ", partes), 1997);

        var total = dps.Valores.Tributacao.Total;
        string tributos;
        if (total.Valores is { } valores)
            tributos = $"Federais: {Dinheiro(valores.Federal)} ; Estaduais: {Dinheiro(valores.Estadual)} ; Municipais: {Dinheiro(valores.Municipal)}";
        else if (total.Percentuais is { } percentuais)
            tributos = $"Federais: {Percentual(percentuais.Federal)} ; Estaduais: {Percentual(percentuais.Estadual)} ; Municipais: {Percentual(percentuais.Municipal)}";
        else
            tributos = $"Federais: {Ausente} ; Estaduais: {Ausente} ; Municipais: {Ausente}";
        return (informacoes, LeiTributos + tributos);
    }

    // O canhoto (2.1.13), opcional, no pé da folha.
    private static void Canhoto(Folha folha, NotaFiscalServico nota, ChaveNFSe chave)
    {
        folha.Divisoria(TopoCanhoto);
        folha.Adicionar(new TextoFormulario(Mm(Colunas[0]), Mm(TopoCanhoto), LarguraColuna * 10, AlturaTotal * 10, "DATA CIENTIFICAÇÃO:", null) { TamanhoRotuloPt = 7 });
        folha.Adicionar(new TextoFormulario(Mm(Colunas[1]), Mm(TopoCanhoto), LarguraColuna * 10, AlturaTotal * 10, "IDENTIFICAÇÃO E ASSINATURA", null)
        {
            TamanhoRotuloPt = 7,
            Bordas = BordaQuadro.Esquerda,
        });
        folha.Adicionar(new TextoFormulario(Mm(Colunas[2]), Mm(TopoCanhoto), LarguraDupla * 10, AlturaTotal * 10, "Nº NFS-e / CHAVE NFS-e",
            Numero(nota.Numero ?? chave.Numero) + " / " + chave.Valor)
        {
            TamanhoRotuloPt = 7,
            Bordas = BordaQuadro.Todas,
        });
    }

    // As três primeiras linhas de uma parte (o prestador, o tomador, o destinatário, o intermediário): os documentos, o
    // nome e o município, o endereço e o e-mail (este, nota 1, só sai quando tem dado).
    private static List<IReadOnlyList<Celula>> LinhasParte(Parte parte)
    {
        var primeira = new List<Celula> { new(1, 1, "CNPJ / CPF / NIF", Cortar(parte.Documento, 40)) };
        if (parte.TemInscricao)
            primeira.Add(new Celula(2, 1, "Indicador Municipal (Inscrição)", Cortar(Texto(parte.Inscricao), 15)));
        primeira.Add(new Celula(3, 1, "Telefone", Cortar(Telefone(parte.Telefone), 20)));
        var local = parte.Local;
        var linhas = new List<IReadOnlyList<Celula>>
        {
            primeira,
            new[]
            {
                new Celula(0, 2, "Nome / Nome Empresarial", Cortar(Texto(parte.Nome), 80)),
                new Celula(2, 1, "Município / Sigla UF", Cortar(Texto(local?.Municipio), 37)),
                new Celula(3, 1, "Código IBGE / CEP", Cortar(Texto(local?.Codigo), 21)),
            },
        };
        if (!string.IsNullOrWhiteSpace(local?.Endereco) || !string.IsNullOrWhiteSpace(parte.Email))
        {
            linhas.Add(
            [
                new Celula(0, 2, "Endereço", Cortar(Texto(local?.Endereco), 80)),
                new Celula(2, 2, "E-mail", Cortar(Texto(parte.Email), 80)),
            ]);
        }

        return linhas;
    }

    private static Parte Pessoa(Pessoa pessoa, bool temInscricao) => new(
        Documento(pessoa.Cnpj, pessoa.Cpf, pessoa.Nif), pessoa.InscricaoMunicipal, pessoa.Telefone, pessoa.Nome,
        pessoa.Endereco is { } endereco ? Local(endereco) : null, pessoa.Email, temInscricao);

    // O endereço da DPS: o nacional pelo código do IBGE (o nome da tabela e a UF dos dois primeiros dígitos, decisão G4) e o
    // exterior pela cidade e o código postal.
    private static LocalParte Local(Endereco endereco)
    {
        var linha = Juntar(", ", endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro);
        if (endereco.Nacional is { } nacional)
        {
            var municipio = nacional.Municipio is { } codigo
                ? Juntar(" / ", MunicipiosIbge.Nome(codigo) ?? codigo.ToString(CultureInfo.InvariantCulture), MunicipiosIbge.Estado(codigo)?.Sigla())
                : null;
            var ibge = Juntar(" / ", nacional.Municipio?.ToString(CultureInfo.InvariantCulture), nacional.Cep is { } cep ? Cep(cep) : null);
            return new LocalParte(municipio, ibge, linha);
        }

        if (endereco.Exterior is { } exterior)
        {
            return new LocalParte(Juntar(" / ", exterior.Cidade, exterior.EstadoProvincia),
                exterior.CodigoPostal is { Length: > 0 } postal ? postal + " (ext)" : null, linha);
        }

        return new LocalParte(null, null, linha);
    }

    // O endereço do emitente da NFS-e (enderNac do emit).
    private static LocalParte Local(EnderecoEmitente endereco)
    {
        var linha = Juntar(", ", endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro);
        var uf = endereco.UF ?? (endereco.Municipio is { } m ? MunicipiosIbge.Estado(m) : null);
        var municipio = endereco.Municipio is { } codigo
            ? Juntar(" / ", MunicipiosIbge.Nome(codigo) ?? codigo.ToString(CultureInfo.InvariantCulture), uf?.Sigla())
            : null;
        var ibge = Juntar(" / ", endereco.Municipio?.ToString(CultureInfo.InvariantCulture), endereco.Cep is { } cep ? Cep(cep) : null);
        return new LocalParte(municipio, ibge, linha);
    }

    // Um bloco do Anexo I: a linha divisória em cima, o título na primeira célula da primeira linha (sombreado, 2.2.3) e as
    // células nas quatro colunas.
    private static Secao Bloco(string titulo, IReadOnlyList<IReadOnlyList<Celula>> linhas, double passo = PassoLinha, double altura = AlturaLinha) =>
        new(linhas.Count * passo, (folha, y) =>
        {
            folha.Titulo(y, altura, titulo);
            for (var i = 0; i < linhas.Count; i++)
            {
                foreach (var celula in linhas[i])
                {
                    var largura = celula.Colunas == 1 ? LarguraColuna : celula.Colunas == 2 ? LarguraDupla : LarguraColuna * 3 + 0.22;
                    folha.Adicionar(new TextoFormulario(Mm(Colunas[celula.Coluna]), Mm(y + i * passo), largura * 10, altura * 10, celula.Rotulo, celula.Valor)
                    {
                        TamanhoRotuloPt = celula.Destaque ? 7 : 6,
                        Sombreado = celula.Sombreado,
                    });
                }
            }

            // A divisória por último, para o sombreado do título não a cobrir.
            folha.Divisoria(y);
        });

    // O bloco suprimido (notas 2 a 4): só a frase, com a altura mínima de 0,32 cm.
    private static Secao Aviso(string frase) => new(PassoAviso, (folha, y) =>
    {
        folha.Adicionar(new TextoFormulario(Mm(Esquerda), Mm(y), LarguraTotal * 10, AlturaAviso * 10, null, frase) { ValorNegrito = true });
        folha.Divisoria(y);
    });

    private static double Estimar(string texto, bool rotulo)
    {
        var linhas = 0;
        foreach (var paragrafo in texto.Split('\n'))
            linhas += Math.Max(1, (paragrafo.Length + CaracteresLinha - 1) / CaracteresLinha);
        return RespiroTexto + (rotulo ? LinhaRotulo : 0) + linhas * LinhaConteudo;
    }

    // A soma dos valores que existem; nulo sem nenhum.
    private static decimal? Somar(params decimal?[] valores) =>
        valores.Any(v => v is not null) ? valores.Sum(v => v ?? 0) : null;

    private static double Mm(double cm) => Math.Round((cm - Margem) * 10, 3);

    private static byte[] Recurso(string nome)
    {
        using var fluxo = typeof(MontagemDanfse).Assembly.GetManifestResourceStream(nome)
            ?? throw new InvalidOperationException($"O recurso {nome} não está no assembly.");
        using var memoria = new MemoryStream();
        fluxo.CopyTo(memoria);
        return memoria.ToArray();
    }

    private sealed record Celula(int Coluna, int Colunas, string Rotulo, string Valor, bool Destaque = false, bool Sombreado = false);

    private sealed record Parte(string Documento, string? Inscricao, string? Telefone, string? Nome, LocalParte? Local, string? Email, bool TemInscricao);

    private sealed record LocalParte(string? Municipio, string? Codigo, string Endereco);

    private sealed record Secao(double Altura, Action<Folha, double> Desenhar);

    // Os quadros do formulário, na ordem em que se desenham.
    private sealed class Folha
    {
        public List<QuadroFormulario> Quadros { get; } = [];

        public void Adicionar(QuadroFormulario quadro) => Quadros.Add(quadro);

        // A linha divisória dos blocos (0,5 pt, 2.2.3): a borda de cima de um quadro vazio na largura toda.
        public void Divisoria(double y) =>
            Quadros.Add(new TextoFormulario(Mm(Esquerda), Mm(y), LarguraTotal * 10, 1, null, null) { Bordas = BordaQuadro.Cima });

        // O título do bloco: 7 pt, negrito, caixa alta, sombreado (2.4.1 e 2.2.3).
        public void Titulo(double y, double altura, string titulo) =>
            Quadros.Add(new TextoFormulario(Mm(Esquerda), Mm(y), LarguraColuna * 10, altura * 10, null, titulo)
            {
                TamanhoValorPt = 7,
                ValorNegrito = true,
                Sombreado = true,
            });

        // Um campo dos dados da NFS-e: o rótulo de 7 pt em caixa alta (2.4.2) e o conteúdo de 7 pt (2.4.3).
        public void Identificacao(double x, double y, double largura, double altura, string rotulo, string valor, bool sombreado = false) =>
            Quadros.Add(new TextoFormulario(Mm(x), Mm(y), largura * 10, altura * 10, rotulo, valor) { TamanhoRotuloPt = 7, Sombreado = sombreado });

        public void Texto(double x, double y, double largura, double altura, string? rotulo, string? valor) =>
            Quadros.Add(new TextoFormulario(Mm(x), Mm(y), largura * 10, altura * 10, rotulo, valor));
    }
}
