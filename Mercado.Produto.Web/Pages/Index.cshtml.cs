using Mercado.Produto.Application;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

namespace Mercado.Produto.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ProdutoService _produtoService;

    public IndexModel(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    public IList<ProdutoEntidade> Produtos { get; set; } = new List<ProdutoEntidade>();

    public async Task OnGetAsync()
    {
        Produtos = (await _produtoService.ListarTodosProdutosAsync()).ToList();
    }
}
