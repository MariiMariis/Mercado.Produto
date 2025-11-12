using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Domain;

/// <summary>
/// Define o CONTRATO de persistência (A Porta).
/// O Domínio diz "eu preciso que alguém salve e busque produtos".
/// PORQUÊ: Usamos 'Task' (async/await) por padrão em .NET para I/O.
/// Isso torna a aplicação escalável e não-bloqueante.
/// PORQUÊ: Usamos 'Produto?' (nullable) para indicar que a busca pode não
/// retornar nada, evitando 'null' (Artefato 4).
/// </summary>
public interface IProdutoRepository
{
    Task SalvarAsync(Produto produto);
    Task<Produto?> BuscarPorIdAsync(Guid id);
    Task<Produto?> BuscarPorSkuAsync(string sku);
    Task<IEnumerable<Produto>> ListarTodosAsync(); // PORQUÊ: IEnumerable é uma coleção "read-only"
    Task DeletarAsync(Guid id);
}