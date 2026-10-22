using System.Reflection;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A escrita da nota (2.4): toda nota gerada passa pelo XSD depois de assinada (PROJETO.md 13.2).
public class EscritorNFeTestes
{
    // As subclasses das escolhas do imposto, cada uma com todos os campos preenchidos.
    public static TheoryData<string> GruposImposto() =>
        [.. typeof(NotaFiscal).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && (t.IsSubclassOf(typeof(Icms)) || t.IsSubclassOf(typeof(Pis)) || t.IsSubclassOf(typeof(Cofins))))
            .Select(t => t.Name).Order(StringComparer.Ordinal)];

    // O CST ou o CSOSN dos grupos que aceitam mais de um (os de um só ficam nulos: a escrita põe o do grupo).
    private static readonly Dictionary<string, object> Codigos = new()
    {
        [nameof(Icms40)] = CstIcms.Isenta,
        [nameof(IcmsPartilha)] = CstIcms.TributadaSubstituicao,
        [nameof(IcmsRepasse)] = CstIcms.NaoTributada,
        [nameof(IcmsSn102)] = CsosnIcms.NaoPermiteCredito,
        [nameof(IcmsSn202)] = CsosnIcms.NaoPermiteCreditoSubstituicao,
        [nameof(PisAliquota)] = CstPisCofins.AliquotaBasica,
        [nameof(PisNaoTributado)] = CstPisCofins.AliquotaZero,
        [nameof(PisOutros)] = CstPisCofins.OutrasSaidas,
        [nameof(CofinsAliquota)] = CstPisCofins.AliquotaBasica,
        [nameof(CofinsNaoTributado)] = CstPisCofins.AliquotaZero,
        [nameof(CofinsOutros)] = CstPisCofins.OutrasSaidas,
    };

    [Fact]
    public void ANFCeMinimaPassaNoSchema() => NotasTeste.AssertValida(NotasTeste.NFCe());

    [Fact]
    public void ANFeComOsGruposDoCabecalhoPassaNoSchema() => NotasTeste.AssertValida(NotasTeste.NFe());

    [Theory]
    [MemberData(nameof(GruposImposto))]
    public void CadaGrupoDoImpostoPassaNoSchema(string grupo) => NotasTeste.AssertValida(NotaGrupoImposto(grupo));

    // A NFC-e mínima com o grupo do imposto no item, com todos os campos preenchidos (a ida e volta da leitura usa as mesmas).
    internal static NotaFiscal NotaGrupoImposto(string grupo)
    {
        var tipo = typeof(NotaFiscal).Assembly.GetTypes().Single(t => t.Name == grupo);
        var preenchido = Activator.CreateInstance(tipo)!;
        foreach (var p in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            if (p.Name is "Cst" or "Csosn")
                p.SetValue(preenchido, Codigos.GetValueOrDefault(grupo));
            else if (t == typeof(decimal))
                p.SetValue(preenchido, 10m);
            else if (t == typeof(UF))
                p.SetValue(preenchido, UF.SP);
            else if (t == typeof(string))
                p.SetValue(preenchido, "ABC12345");
            else if (t == typeof(MotivoDesoneracao))
                p.SetValue(preenchido, MotivoDesoneracao.Outros);   // o 9 está na lista de todos os grupos
            else if (t.IsEnum)
                p.SetValue(preenchido, Enum.GetValues(t).GetValue(0));
        }
        // Os excludentes dentro do PIS e da COFINS (base e alíquota, ou quantidade e valor por unidade): fica a base.
        if (tipo.GetProperty("BaseCalculo") is not null)
            foreach (var nome in new[] { "Quantidade", "AliquotaUnidade" })
                tipo.GetProperty(nome)?.SetValue(preenchido, null);

        var nota = NotasTeste.NFCe();
        var imposto = nota.Itens[0].Imposto;
        switch (preenchido)
        {
            case Icms icms: imposto.Icms = icms; break;
            case Pis pis: imposto.Pis = pis; break;
            case Cofins cofins: imposto.Cofins = cofins; break;
        }
        return nota;
    }
}
