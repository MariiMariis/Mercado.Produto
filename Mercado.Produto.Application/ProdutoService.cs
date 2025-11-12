using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
//
// PORQUÊ: Esta é a correção.
// Damos o "apelido" de 'ProdutoEntidade' para a classe
// 'Mercado.Produto.Domain.Produto' para resolver a ambiguidade.
//
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

namespace Mercado.Produto.Application;

/// <summary>
/// Camada de Serviço (Casos de Uso). Orquestra as ações.
/// </summary>
public class ProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    /// <summary>
    /// Caso de Uso: Criar um novo produto.
    /// </summary>
    public async Task<Guid> CriarNovoProdutoAsync(string sku, string nome, decimal preco, int estoque, CategoriaProduto categoria, DateOnly? validade)
    {
        // 1. Orquestração: Verificar regras
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

    /// <summary>
    /// Caso de Uso: Dar baixa em um item do estoque.
    /// </summary>
    public async Task DarBaixaEstoqueAsync(string sku, int quantidade)
    {
        // 1. Orquestração: Buscar a entidade (ou falhar)
        // PORQUÊ: Note que 'ProdutoEntidade' não é necessário aqui,
        // pois o compilador não fica confuso com 'var'.
        var produto = await _produtoRepository.BuscarPorSkuAsync(sku)
            ?? throw new ProdutoNaoEncontradoException($"Produto com SKU '{sku}' não encontrado.");

        // 2. Delegação: O domínio executa a lógica
        produto.DarBaixaEstoque(quantidade);

        // 3. Persistência
        await _produtoRepository.SalvarAsync(produto);
    }

    public async Task<IEnumerable<ProdutoEntidade>> ListarTodosProdutosAsync()
    {
        // PORQUÊ: Aqui o alias é útil para clareza no tipo de retorno.
        return await _produtoRepository.ListarTodosAsync();
    }

    public async Task<ProdutoEntidade?> BuscarPorSkuAsync(string sku)
    {
        // PORQUÊ: E aqui também, para o tipo de retorno anulável.
        return await _produtoRepository.BuscarPorSkuAsync(sku);
    }
}