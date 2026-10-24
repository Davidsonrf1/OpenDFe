using System.Reflection;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// A escrita da nota no leiaute 4.00 (PROJETO.md 5.1, MODELO.md 6): o <see cref="NotaFiscal"/> vira o <c>NFe</c>, com
/// <see cref="XmlWriter"/> à mão, um método por grupo, na ordem do XSD do pacote embutido.
/// </summary>
/// <remarks>
/// <para>
/// O XML sai sem prefixo, sem declaração, sem espaço entre as tags e sem a <c>Signature</c>; a tag de valor nulo (e de
/// texto vazio) não sai; os decimais com as casas do campo, sem arredondar; as datas com o deslocamento do valor; as
/// enumerações pelo valor do XML, com os zeros do campo. O modelo não é conferido aqui: o valor errado sai como está,
/// e o schema aponta (MODELO.md 3, regra 4).
/// </para>
/// <para>
/// A escrita não altera a nota. Os campos que o núcleo já sabe vêm de <see cref="ValoresNucleo"/>; o <c>cNF</c> nulo é
/// sorteado; a chave, o <c>Id</c> e o <c>cDV</c> vão para o <see cref="NotaEscrita"/>. Sem estado e seguro em paralelo.
/// </para>
/// <para>
/// As casas decimais de cada campo vêm do tipo do XSD: as fixas, como estão (<c>TDec_1302</c>: 2); nas variáveis, a
/// menor da faixa, mas não menos de 2 (<c>TDec_1110v</c>, de 1 a 10: 2; <c>TDec_0803v</c>, que pede 3 no zero: 3). O
/// valor com mais casas significativas sai com todas (<see cref="FormatadorInvariavel.Numero"/>).
/// </para>
/// </remarks>
internal static partial class EscritorNFe
{
    private const int MaiorSerie = 999;
    private const int MaiorNumero = 999_999_999;
    private const int LimiteCodigoNumerico = 100_000_000;

    private static readonly XmlWriterSettings Configuracao = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    /// <summary>Escreve a nota com os valores do núcleo.</summary>
    internal static NotaEscrita Escrever(NotaFiscal nota, ValoresNucleo nucleo) =>
        Escrever(nota, nucleo, CamposNaoSuportados.Lista);

    /// <summary>Escreve a nota, com os avisos dos campos de <paramref name="naoSuportados"/> preenchidos.</summary>
    internal static NotaEscrita Escrever(NotaFiscal nota, ValoresNucleo nucleo, IReadOnlyCollection<PropertyInfo> naoSuportados)
    {
        ArgumentNullException.ThrowIfNull(nota);
        ArgumentNullException.ThrowIfNull(nucleo);
        ArgumentNullException.ThrowIfNull(naoSuportados);

        var texto = new StringBuilder();
        EscritaNFe e;
        using (var xml = XmlWriter.Create(texto, Configuracao))
        {
            e = new EscritaNFe(xml, nucleo);
            e.Chave = MontarChave(nota, nucleo, e);
            xml.WriteStartElement("NFe", EscritaNFe.Namespace);
            e.Abrir("infNFe", null);
            xml.WriteAttributeString("versao", "4.00");
            if (e.Chave is { } chave)
                xml.WriteAttributeString("Id", "NFe" + chave);
            Escrever(e, nota);
            e.Fechar();
            if (Suplementares(e, nota, nucleo) is { } suplementares)
            {
                e.Abrir("infNFeSupl", "Suplementares");
                Escrever(e, suplementares);
                e.Fechar();
            }
            xml.WriteEndElement();
        }
        CamposNaoSuportados.Conferir(nota, naoSuportados, e.Problemas);
        return new NotaEscrita(texto.ToString(), e.Chave, e.Problemas.ToArray());
    }

    // A chave (7.1) com o que vai no XML: a UF, o modelo e o tipo de emissão do núcleo; a data da emissão, o emitente,
    // a série e o número da nota; o cNF da nota ou sorteado. O dado que falta ou não serve é problema (sem a chave, a
    // nota não tem Id nem cDV), e a escrita segue para apontar o resto.
    private static ChaveAcesso? MontarChave(NotaFiscal nota, ValoresNucleo nucleo, EscritaNFe e)
    {
        var ide = nota.Identificacao ?? new Identificacao();
        var emitente = nota.Emitente ?? new Emitente();
        var falta = false;

        void Faltar(string caminho, string propriedade, string mensagem)
        {
            e.RegistrarInfNFe(CodigosEscrita.Chave, mensagem + " Sem ele, a chave de acesso não é montada.", caminho, propriedade);
            falta = true;
        }

        var emissao = ide.Emissao;
        if (emissao is null)
            Faltar("ide/dhEmi", "Identificacao.Emissao", "Falta a data da emissão.");
        else if (emissao.Value.Year is < 2000 or > 2099)
            Faltar("ide/dhEmi", "Identificacao.Emissao", "O ano da emissão vai de 2000 a 2099.");

        string? documento = null;
        if (!string.IsNullOrEmpty(emitente.Cnpj))
        {
            if (Cnpj.Valido(emitente.Cnpj))
                documento = emitente.Cnpj;
            else
                Faltar("emit/CNPJ", "Emitente.Cnpj", "O CNPJ do emitente não é válido.");
        }
        else if (!string.IsNullOrEmpty(emitente.Cpf))
        {
            if (Cpf.Valido(emitente.Cpf))
                documento = emitente.Cpf;
            else
                Faltar("emit/CPF", "Emitente.Cpf", "O CPF do emitente não é válido.");
        }
        else
            Faltar("emit/CNPJ", "Emitente.Cnpj", "Falta o CNPJ ou o CPF do emitente.");

        if (ide.Serie is not (>= 0 and <= MaiorSerie))
            Faltar("ide/serie", "Identificacao.Serie", ide.Serie is null ? "Falta a série." : "A série vai de 0 a 999.");
        if (ide.Numero is not (>= 1 and <= MaiorNumero))
            Faltar("ide/nNF", "Identificacao.Numero", ide.Numero is null ? "Falta o número." : "O número vai de 1 a 999.999.999.");
        if (ide.CodigoNumerico is { } codigo && codigo is < 0 or >= LimiteCodigoNumerico)
            Faltar("ide/cNF", "Identificacao.CodigoNumerico", "O código numérico vai de 0 a 99.999.999.");

        if (falta)
            return null;
        return ide.CodigoNumerico is { } cnf
            ? ChaveAcesso.Montar(nucleo.UF, emissao!.Value, documento!, nucleo.Documento, ide.Serie!.Value, ide.Numero!.Value,
                nucleo.Emissao, cnf)
            : ChaveAcesso.Gerar(nucleo.UF, emissao!.Value, documento!, nucleo.Documento, ide.Serie!.Value, ide.Numero!.Value,
                nucleo.Emissao);
    }
}
