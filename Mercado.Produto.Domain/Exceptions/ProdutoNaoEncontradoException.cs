using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain.Exceptions;

public class ProdutoNaoEncontradoException : Exception
{
    public ProdutoNaoEncontradoException(string message) : base(message)
    {
    }
}