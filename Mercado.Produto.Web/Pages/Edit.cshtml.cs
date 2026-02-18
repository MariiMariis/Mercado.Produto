using Mercado.Produto.Application;
using Mercado.Produto.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Mercado.Produto.Web.Pages;

public class EditModel : PageModel
{
    private readonly ProdutoService _produtoService;
    public EditModel(ProdutoService produtoService)
    {
        _produtoService = produtoService;
        Categorias = new SelectList(System.Enum.GetValues(typeof(CategoriaProduto)));
    }

    [BindProperty]
    public ProdutoInput Produto { get; set; } = new ProdutoInput();
    public SelectList Categorias { get; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var produto = await _produtoService.BuscarPorIdAsync(id);
        if (produto == null)
            return RedirectToPage("Index");
        Produto = new ProdutoInput
        {
            Id = produto.Id,
            Sku = produto.Sku,
            Nome = produto.Nome,
            PrecoVenda = produto.PrecoVenda,
            Categoria = produto.Categoria,
            DataValidade = produto.DataValidade,
            EstoqueAtual = produto.EstoqueAtual
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        try
        {
            await _produtoService.AtualizarProdutoAsync(
                Produto.Id,
                Produto.Sku,
                Produto.Nome,
                Produto.PrecoVenda,
                Produto.EstoqueAtual,
                Produto.Categoria,
                Produto.DataValidade);
            return RedirectToPage("Index");
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public class ProdutoInput
    {
        public Guid Id { get; set; }
        [Required]
        public string Sku { get; set; } = string.Empty;
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Display(Name = "Preço de Venda")]
        [Range(0, double.MaxValue)]
        public decimal PrecoVenda { get; set; }
        [Required]
        public CategoriaProduto Categoria { get; set; }
        [Display(Name = "Data de Validade")]
        [DataType(DataType.Date)]
        public DateOnly? DataValidade { get; set; }
        [Display(Name = "Estoque Atual")]
        [Range(0, int.MaxValue)]
        public int EstoqueAtual { get; set; }
    }
}
