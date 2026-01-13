using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain.Exceptions;

public class ValidacaoProdutoException : Exception
{
    public ValidacaoProdutoException(string message) : base(message)
    {
    }
}