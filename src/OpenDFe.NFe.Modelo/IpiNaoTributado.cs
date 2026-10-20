namespace OpenDFe.NFe;

/// <summary>O IPI não tributado (<c>IPINT</c>).</summary>
public sealed class IpiNaoTributado
{
    /// <summary>
    /// O CST: <c>01</c>, <c>02</c>, <c>03</c>, <c>04</c>, <c>05</c>, <c>51</c>, <c>52</c>, <c>53</c>, <c>54</c> ou
    /// <c>55</c>.
    /// </summary>
    [Tag("CST", "O09")]
    public CstIpi? Cst { get; set; }
}
