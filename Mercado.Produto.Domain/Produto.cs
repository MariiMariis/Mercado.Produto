using Mercado.Produto.Domain.Exceptions;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Mercado.Produto.Domain;


public class Produto
{
   
    private const int TAMANHO_MINIMO_NOME = 3;
    private const int TAMANHO_MAXIMO_NOME = 100;

    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public CategoriaProduto Categoria { get; private set; }
    public DateOnly? DataValidade { get; private set; }
    public int EstoqueAtual { get; private set; }


    private Produto()
    {

        Sku = string.Empty;
        Nome = string.Empty;
    }

    [JsonConstructor]
    public Produto(Guid id, string sku, string nome, decimal precoVenda, CategoriaProduto categoria, DateOnly? dataValidade, int estoqueAtual)
    {
        Id = id;
        Sku = sku;
        Nome = nome;
        PrecoVenda = precoVenda;
        Categoria = categoria;
        DataValidade = dataValidade;
        EstoqueAtual = estoqueAtual;
    }

    public static Produto Create(string sku, string nome, decimal precoVenda, int estoqueAtual, CategoriaProduto categoria, DateOnly? dataValidade)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ValidacaoProdutoException("SKU é obrigatório.");

        if (string.IsNullOrWhiteSpace(nome) || nome.Length < TAMANHO_MINIMO_NOME || nome.Length > TAMANHO_MAXIMO_NOME)
            throw new ValidacaoProdutoException($"Nome deve ter entre {TAMANHO_MINIMO_NOME} e {TAMANHO_MAXIMO_NOME} caracteres.");

        if (precoVenda < 0)
            throw new ValidacaoProdutoException("Preço não pode ser negativo.");

        if (estoqueAtual < 0)
            throw new ValidacaoProdutoException("Estoque inicial não pode ser negativo.");

        if (dataValidade.HasValue && dataValidade.Value < DateOnly.FromDateTime(DateTime.Now))
            throw new ValidacaoProdutoException("Data de validade não pode estar no passado.");
     
        return new Produto
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Nome = nome,
            PrecoVenda = precoVenda,
            EstoqueAtual = estoqueAtual,
            Categoria = categoria,
            DataValidade = dataValidade
        };
    }

    public void DarBaixaEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ValidacaoProdutoException("Quantidade da baixa deve ser positiva.");

        if (EstoqueAtual < quantidade)
            throw new EstoqueInsuficienteException($"Estoque: {EstoqueAtual}, Tentativa de baixa: {quantidade}");

        EstoqueAtual -= quantidade;
    }

    public void AdicionarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ValidacaoProdutoException("Quantidade da adição deve ser positiva.");

        EstoqueAtual += quantidade;
    }

    public override string ToString()
    {
        return $"Produto [Id={Id}, Sku={Sku}, Nome={Nome}, Preco={PrecoVenda.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}, Estoque={EstoqueAtual}]";
    }


    public override bool Equals(object? obj)
    {
        if (obj is not Produto outroProduto) return false;
        return Id.Equals(outroProduto.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}