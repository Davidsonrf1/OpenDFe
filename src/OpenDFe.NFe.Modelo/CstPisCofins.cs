namespace OpenDFe.NFe;

/// <summary>O CST do PIS e da COFINS (os mesmos códigos). O valor de cada membro é o do XML (<c>CST</c>).</summary>
/// <remarks>
/// Cada grupo do PIS e da COFINS aceita só uma parte dos códigos, e o schema aponta o código que o grupo não aceita. O
/// leiaute escreve o valor com os dois dígitos.
/// </remarks>
public enum CstPisCofins
{
    /// <summary>Operação tributável com alíquota básica (<c>01</c>).</summary>
    AliquotaBasica = 1,

    /// <summary>Operação tributável com alíquota diferenciada (<c>02</c>).</summary>
    AliquotaDiferenciada = 2,

    /// <summary>Operação tributável com alíquota por unidade de medida de produto (<c>03</c>).</summary>
    AliquotaUnidade = 3,

    /// <summary>Operação tributável monofásica, revenda a alíquota zero (<c>04</c>).</summary>
    MonofasicaRevendaAliquotaZero = 4,

    /// <summary>Operação tributável por substituição tributária (<c>05</c>).</summary>
    SubstituicaoTributaria = 5,

    /// <summary>Operação tributável a alíquota zero (<c>06</c>).</summary>
    AliquotaZero = 6,

    /// <summary>Operação isenta da contribuição (<c>07</c>).</summary>
    Isenta = 7,

    /// <summary>Operação sem incidência da contribuição (<c>08</c>).</summary>
    NaoIncidencia = 8,

    /// <summary>Operação com suspensão da contribuição (<c>09</c>).</summary>
    Suspensao = 9,

    /// <summary>Outras operações de saída (<c>49</c>).</summary>
    OutrasSaidas = 49,

    /// <summary>
    /// Com direito a crédito, vinculada exclusivamente a receita tributada no mercado interno (<c>50</c>).
    /// </summary>
    CreditoReceitaTributada = 50,

    /// <summary>
    /// Com direito a crédito, vinculada exclusivamente a receita não tributada no mercado interno (<c>51</c>).
    /// </summary>
    CreditoReceitaNaoTributada = 51,

    /// <summary>Com direito a crédito, vinculada exclusivamente a receita de exportação (<c>52</c>).</summary>
    CreditoReceitaExportacao = 52,

    /// <summary>
    /// Com direito a crédito, vinculada a receitas tributadas e não tributadas no mercado interno (<c>53</c>).
    /// </summary>
    CreditoReceitasMercadoInterno = 53,

    /// <summary>
    /// Com direito a crédito, vinculada a receitas tributadas no mercado interno e de exportação (<c>54</c>).
    /// </summary>
    CreditoReceitasTributadaExportacao = 54,

    /// <summary>
    /// Com direito a crédito, vinculada a receitas não tributadas no mercado interno e de exportação (<c>55</c>).
    /// </summary>
    CreditoReceitasNaoTributadaExportacao = 55,

    /// <summary>
    /// Com direito a crédito, vinculada a receitas tributadas e não tributadas no mercado interno e de exportação
    /// (<c>56</c>).
    /// </summary>
    CreditoReceitasTodas = 56,

    /// <summary>
    /// Crédito presumido, aquisição vinculada exclusivamente a receita tributada no mercado interno (<c>60</c>).
    /// </summary>
    CreditoPresumidoReceitaTributada = 60,

    /// <summary>
    /// Crédito presumido, aquisição vinculada exclusivamente a receita não tributada no mercado interno (<c>61</c>).
    /// </summary>
    CreditoPresumidoReceitaNaoTributada = 61,

    /// <summary>Crédito presumido, aquisição vinculada exclusivamente a receita de exportação (<c>62</c>).</summary>
    CreditoPresumidoReceitaExportacao = 62,

    /// <summary>
    /// Crédito presumido, aquisição vinculada a receitas tributadas e não tributadas no mercado interno (<c>63</c>).
    /// </summary>
    CreditoPresumidoReceitasMercadoInterno = 63,

    /// <summary>
    /// Crédito presumido, aquisição vinculada a receitas tributadas no mercado interno e de exportação (<c>64</c>).
    /// </summary>
    CreditoPresumidoReceitasTributadaExportacao = 64,

    /// <summary>
    /// Crédito presumido, aquisição vinculada a receitas não tributadas no mercado interno e de exportação (<c>65</c>).
    /// </summary>
    CreditoPresumidoReceitasNaoTributadaExportacao = 65,

    /// <summary>
    /// Crédito presumido, aquisição vinculada a receitas tributadas e não tributadas no mercado interno e de exportação
    /// (<c>66</c>).
    /// </summary>
    CreditoPresumidoReceitasTodas = 66,

    /// <summary>Crédito presumido, outras operações (<c>67</c>).</summary>
    CreditoPresumidoOutras = 67,

    /// <summary>Aquisição sem direito a crédito (<c>70</c>).</summary>
    AquisicaoCreditoVedado = 70,

    /// <summary>Aquisição com isenção (<c>71</c>).</summary>
    AquisicaoIsencao = 71,

    /// <summary>Aquisição com suspensão (<c>72</c>).</summary>
    AquisicaoSuspensao = 72,

    /// <summary>Aquisição a alíquota zero (<c>73</c>).</summary>
    AquisicaoAliquotaZero = 73,

    /// <summary>Aquisição sem incidência da contribuição (<c>74</c>).</summary>
    AquisicaoNaoIncidencia = 74,

    /// <summary>Aquisição por substituição tributária (<c>75</c>).</summary>
    AquisicaoSubstituicao = 75,

    /// <summary>Outras operações de entrada (<c>98</c>).</summary>
    OutrasEntradas = 98,

    /// <summary>Outras operações (<c>99</c>).</summary>
    Outras = 99,
}
