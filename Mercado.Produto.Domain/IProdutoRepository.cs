using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain;

public interface IProdutoRepository
{
    Task SalvarAsync(Produto produto);
    Task<Produto?> BuscarPorIdAsync(Guid id);
    Task<Produto?> BuscarPorSkuAsync(string sku);
    Task<IEnumerable<Produto>> ListarTodosAsync(); //IEnumerable é uma coleção "read-only"
    Task DeletarAsync(Guid id);
}