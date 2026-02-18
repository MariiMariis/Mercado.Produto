namespace Mercado.Produto.Domain;

public interface IProdutoRepository
{
    Task SalvarAsync(Produto produto);
    Task<Produto?> BuscarPorIdAsync(Guid id);
    Task<Produto?> BuscarPorSkuAsync(string sku);
    Task<IEnumerable<Produto>> ListarTodosAsync();
    Task DeletarAsync(Guid id);
}