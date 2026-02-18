namespace Mercado.Produto.Domain.Exceptions;

public class ProdutoNaoEncontradoException : Exception
{
    public ProdutoNaoEncontradoException(string message) : base(message)
    {
    }
}