using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

namespace Mercado.Produto.Application;

/// Camada de Serviço - Orquestra as ações.
public class ProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    
    ///Criar um novo produto.
    public async Task<Guid> CriarNovoProdutoAsync(string sku, string nome, decimal preco, int estoque, CategoriaProduto categoria, DateOnly? validade)
    {
        // 1. Orquestração: Verifica regras
        if (await _produtoRepository.BuscarPorSkuAsync(sku) is not null)
        {
            throw new ValidacaoProdutoException($"SKU '{sku}' já cadastrado.");
        }

        // 2. Delegação: Usando o ALIAS 'ProdutoEntidade' para criar
        var produto = ProdutoEntidade.Create(sku, nome, preco, estoque, categoria, validade);

        // 3. Persistência
        await _produtoRepository.SalvarAsync(produto);
        return produto.Id;
    }

    ///Da baixa em um item do estoque
    public async Task DarBaixaEstoqueAsync(string sku, int quantidade)
    {
        // 1. Orquestração: Buscar a entidade (ou falhar)
        var produto = await _produtoRepository.BuscarPorSkuAsync(sku)
            ?? throw new ProdutoNaoEncontradoException($"Produto com SKU '{sku}' não encontrado.");

        // 2. Delegação: O domínio executa a lógica
        produto.DarBaixaEstoque(quantidade);

        // 3. Persistência
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