using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

namespace Mercado.Produto.Application;

public class ProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<Guid> CriarNovoProdutoAsync(string sku, string nome, decimal preco, int estoque, CategoriaProduto categoria, DateOnly? validade)
    {

        if (await _produtoRepository.BuscarPorSkuAsync(sku) is not null)
        {
            throw new ValidacaoProdutoException($"SKU '{sku}' já cadastrado.");
        }

        var produto = ProdutoEntidade.Create(sku, nome, preco, estoque, categoria, validade);

        await _produtoRepository.SalvarAsync(produto);
        return produto.Id;
    }


    public async Task DarBaixaEstoqueAsync(string sku, int quantidade)
    {
        var produto = await _produtoRepository.BuscarPorSkuAsync(sku)
            ?? throw new ProdutoNaoEncontradoException($"Produto com SKU '{sku}' não encontrado.");

        produto.DarBaixaEstoque(quantidade);

        await _produtoRepository.SalvarAsync(produto);
    }

    public async Task<IEnumerable<ProdutoEntidade>> ListarTodosProdutosAsync()
    {
       
        return await _produtoRepository.ListarTodosAsync();
    }

    public async Task<ProdutoEntidade?> BuscarPorSkuAsync(string sku)
    {

        return await _produtoRepository.BuscarPorSkuAsync(sku);
    }
}