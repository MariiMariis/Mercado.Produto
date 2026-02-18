using Mercado.Produto.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

namespace Mercado.Produto.Web.Pages;

public class DeleteModel : PageModel
{
    private readonly ProdutoService _produtoService;
    public DeleteModel(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [BindProperty]
    public ProdutoEntidade? Produto { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Produto = await _produtoService.BuscarPorIdAsync(id);
        if (Produto is null)
            return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Produto is null)
            return RedirectToPage("Index");
        await _produtoService.ExcluirProdutoAsync(Produto.Id);
        return RedirectToPage("Index");
    }
}
