using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain.Exceptions;

// PORQUÊ: Uma exceção específica do domínio. Herda de Exception,
// pois é uma condição de falha conhecida (violação de regra de negócio).
public class ValidacaoProdutoException : Exception
{
    public ValidacaoProdutoException(string message) : base(message)
    {
    }
}