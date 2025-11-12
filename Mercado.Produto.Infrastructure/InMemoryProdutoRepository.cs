using Mercado.Produto.Domain;
using System.Collections.Concurrent;


namespace Mercado.Produto.Infrastructure;

using Mercado.Produto.Domain; // Certifique-se que o using do Domain está aqui
using System.Collections.Concurrent;

/// <summary>
/// Implementação (Adaptador) do contrato do Repositório em memória.
/// </summary>
public class InMemoryProdutoRepository : IProdutoRepository
{
    //
    // CORREÇÃO: Os campos devem estar DENTRO da classe.
    // PORQUÊ: Eles representam o estado privado e interno
    // desta implementação específica do repositório.
    //
    private static readonly ConcurrentDictionary<Guid, Produto> _db = new();
    private static readonly ConcurrentDictionary<string, Guid> _skuIndex = new();

    public Task SalvarAsync(Produto produto)
    {
        // Verifica a unicidade do SKU
        if (_skuIndex.TryGetValue(produto.Sku, out Guid idExistente) && idExistente != produto.Id)
        {
            throw new InvalidOperationException($"SKU '{produto.Sku}' já está em uso por outro produto.");
        }

        _db[produto.Id] = produto;
        _skuIndex[produto.Sku] = produto.Id;

        return Task.CompletedTask;
    }

    public Task<Produto?> BuscarPorIdAsync(Guid id)
    {
        _db.TryGetValue(id, out Produto? produto);
        return Task.FromResult(produto);
    }

    public Task<Produto?> BuscarPorSkuAsync(string sku)
    {
        if (_skuIndex.TryGetValue(sku, out Guid id))
        {
            return BuscarPorIdAsync(id);
        }
        return Task.FromResult((Produto?)null);
    }

    public Task<IEnumerable<Produto>> ListarTodosAsync()
    {
        return Task.FromResult(_db.Values.AsEnumerable());
    }

    public Task DeletarAsync(Guid id)
    {
        if (_db.TryRemove(id, out Produto? removido))
        {
            _skuIndex.TryRemove(removido.Sku, out _);
        }
        return Task.CompletedTask;
    }
}