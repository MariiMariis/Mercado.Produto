using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain.Exceptions;

public class EstoqueInsuficienteException : Exception
{
    public EstoqueInsuficienteException(string message) : base(message)
    {
    }
}