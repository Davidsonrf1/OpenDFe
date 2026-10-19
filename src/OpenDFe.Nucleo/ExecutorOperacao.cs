namespace OpenDFe;

/// <summary>
/// O handler registrado, sem o tipo do parâmetro: o núcleo o acha pelo tipo exato do parâmetro e o chama pelo
/// tipo do retorno, sem reflexão.
/// </summary>
internal abstract class ExecutorOperacao
{
    public abstract OperacaoSuportada Operacao { get; }
}

/// <inheritdoc cref="ExecutorOperacao"/>
internal abstract class ExecutorOperacao<TRetorno> : ExecutorOperacao
    where TRetorno : class
{
    public abstract PedidoWS Montar(ParametroWS<TRetorno> parametro, ContextoOperacao contexto);

    public abstract LeituraWS<TRetorno> Ler(ParametroWS<TRetorno> parametro, string xmlRetorno, ContextoOperacao contexto);
}

/// <inheritdoc cref="ExecutorOperacao"/>
internal sealed class ExecutorOperacao<TParametro, TRetorno> : ExecutorOperacao<TRetorno>
    where TParametro : ParametroWS<TRetorno>
    where TRetorno : class
{
    private readonly IOperacao<TParametro, TRetorno> _operacao;

    public ExecutorOperacao(IOperacao<TParametro, TRetorno> operacao)
    {
        _operacao = operacao;
        Operacao = new OperacaoSuportada(typeof(TParametro), typeof(TRetorno));
    }

    public override OperacaoSuportada Operacao { get; }

    // O núcleo só chama com o parâmetro do tipo exato TParametro (o dicionário é por tipo).
    public override PedidoWS Montar(ParametroWS<TRetorno> parametro, ContextoOperacao contexto) =>
        _operacao.Montar((TParametro)parametro, contexto);

    public override LeituraWS<TRetorno> Ler(ParametroWS<TRetorno> parametro, string xmlRetorno, ContextoOperacao contexto) =>
        _operacao.Ler((TParametro)parametro, xmlRetorno, contexto);
}
