namespace OpenDFe;

/// <summary>O método HTTP de uma operação REST (<see cref="PedidoRest.Metodo"/>; docs/NFSE.md, D4).</summary>
public enum MetodoRest
{
    /// <summary>GET: a consulta, sem corpo.</summary>
    Get = 1,

    /// <summary>POST: o documento compactado no JSON (a emissão, o evento).</summary>
    Post = 2,

    /// <summary>HEAD: só o status, sem corpo na ida nem na volta.</summary>
    Head = 3,
}
