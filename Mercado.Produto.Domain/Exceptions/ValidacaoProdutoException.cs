namespace Mercado.Produto.Domain.Exceptions;

public class ValidacaoProdutoException : Exception
{
    public ValidacaoProdutoException(string message) : base(message)
    {
    }
}