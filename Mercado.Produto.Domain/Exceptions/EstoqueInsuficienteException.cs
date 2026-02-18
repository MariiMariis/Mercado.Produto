namespace Mercado.Produto.Domain.Exceptions;

public class EstoqueInsuficienteException : Exception
{
    public EstoqueInsuficienteException(string message) : base(message)
    {
    }
}