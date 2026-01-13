using Mercado.Produto.Application;
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Mercado.Produto.Infrastructure;
using System.Globalization;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

IProdutoRepository repository = new ArquivoProdutoRepository();
ProdutoService service = new ProdutoService(repository);

Console.WriteLine("=== Sistema de Cadastro de Produtos do Mercado (.NET) ===");
Console.WriteLine("Bem-vindo, Engenheiro.");

await MainLoopAsync();

async Task MainLoopAsync()
{
    while (true)
    {
        ExibirMenu();
        string? escolha = Console.ReadLine();

        switch (escolha)
        {
            case "1":
                await ListarTodosProdutosAsync();
                break;
            case "2":
                await CadastrarNovoProdutoAsync();
                break;
            case "3":
                await DarBaixaEstoqueAsync();
                break;
            case "0":
                Console.WriteLine("Desligando sistemas. Até logo.");
                return;
            default:
                ImprimirErro("Opção inválida. Tente novamente.");
                break;
        }
    }
}

void ExibirMenu()
{
    Console.WriteLine("\n--- MENU DE OPERAÇÕES ---");
    Console.WriteLine("1: Listar todos os produtos");
    Console.WriteLine("2: Cadastrar novo produto");
    Console.WriteLine("3: Dar baixa no estoque");
    Console.WriteLine("0: Sair");
    Console.Write("Escolha uma opção: ");
}

async Task ListarTodosProdutosAsync()
{
    Console.WriteLine("\n[OPERAÇÃO: Listar Produtos]");
    var produtos = await service.ListarTodosProdutosAsync();

    if (!produtos.Any())
    {
        Console.WriteLine("Nenhum produto cadastrado.");
        return;
    }

    foreach (var p in produtos)
    {
        
        Console.WriteLine(p.ToString());
    }
}

async Task CadastrarNovoProdutoAsync()
{
    Console.WriteLine("\n[OPERAÇÃO: Cadastrar Novo Produto]");
    try
    {
        string sku = LerStringObrigatoria("SKU (Código de Barras): ");
        string nome = LerStringObrigatoria("Nome (3-100 caracteres): ");
        decimal preco = LerDecimal("Preço de Venda (ex: 29.99): ");
        int estoque = LerInt("Estoque Inicial (ex: 50): ");
        CategoriaProduto categoria = LerCategoria("Categoria: ");

        var id = await service.CriarNovoProdutoAsync(sku, nome, preco, estoque, categoria, null);

        ImprimirSucesso($"Produto '{nome}' (ID: {id}) cadastrado com sucesso.");
    }
    catch (Exception ex)
    {
        ImprimirErro(ex.Message);
    }
}

async Task DarBaixaEstoqueAsync()
{
    Console.WriteLine("\n[OPERAÇÃO: Dar Baixa de Estoque]");
    try
    {
        string sku = LerStringObrigatoria("SKU do produto: ");
        int quantidade = LerInt("Quantidade a dar baixa (ex: 5): ");

        await service.DarBaixaEstoqueAsync(sku, quantidade);

        var produto = await service.BuscarPorSkuAsync(sku);
        ImprimirSucesso($"Baixa realizada. Novo estoque: {produto?.EstoqueAtual}");
    }
    catch (Exception ex)
    {
        ImprimirErro(ex.Message);
    }
}

string LerStringObrigatoria(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input;
        }
        ImprimirErro("Este campo é obrigatório.");
    }
}

decimal LerDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
        {
            return valor;
        }
        ImprimirErro("Valor decimal inválido. Use '.' como separador (ex: 10.99).");
    }
}

int LerInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int valor))
        {
            return valor;
        }
        ImprimirErro("Valor inteiro inválido. (ex: 10).");
    }
}

CategoriaProduto LerCategoria(string prompt)
{
    Console.WriteLine(prompt);
    var categorias = Enum.GetNames<CategoriaProduto>();
    for (int i = 0; i < categorias.Length; i++)
    {
        Console.WriteLine($"  {i + 1}: {categorias[i]}");
    }

    while (true)
    {
        Console.Write("Escolha um número: ");
        if (int.TryParse(Console.ReadLine(), out int escolha) && escolha > 0 && escolha <= categorias.Length)
        {
            return (CategoriaProduto)(escolha - 1);
        }
        ImprimirErro("Opção de categoria inválida.");
    }
}

void ImprimirErro(string mensagem)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERRO] {mensagem}");
    Console.ResetColor();
}

void ImprimirSucesso(string mensagem)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[SUCESSO] {mensagem}");
    Console.ResetColor();
}