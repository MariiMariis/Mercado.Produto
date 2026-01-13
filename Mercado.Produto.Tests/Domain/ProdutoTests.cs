namespace Mercado.Produto.Tests.Domain;

using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Xunit;
using FluentAssertions;

public class ProdutoTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarProduto()
    {
        var sku = "123";
        var nome = "Produto Bom";
        var preco = 10.0m;
        var estoque = 10;
        var categoria = CategoriaProduto.Mercearia;

        var produto = Produto.Create(sku, nome, preco, estoque, categoria, null);

        produto.Should().NotBeNull();

        produto.Nome.Should().Be(nome);
        produto.EstoqueAtual.Should().Be(estoque);
        produto.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(-1.0)]  
    [InlineData(-0.01)] 
    public void Create_PrecoNegativo_DeveLancarExcecao(double precoDouble)
    {
        var preco = (decimal)precoDouble;

        Action act = () => Produto.Create("123", "Produto", preco, 10, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Preço não pode ser negativo.");
    }

    [Fact]
    public void Create_NomeCurto_DeveLancarExcecao()
    {
        Action act = () => Produto.Create("123", "Ab", 10m, 10, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Nome deve ter entre 3 e 100 caracteres.");
    }

    [Fact]
    public void Create_EstoqueNegativo_DeveLancarExcecao()
    {

        Action act = () => Produto.Create("123", "Produto", 10m, -1, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Estoque inicial não pode ser negativo.");
    }


    [Fact]
    public void DarBaixaEstoque_EstoqueSuficiente_DeveSubtrair()
    {

        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);


        produto.DarBaixaEstoque(7);


        produto.EstoqueAtual.Should().Be(3);
    }

    [Fact]
    public void DarBaixaEstoque_EstoqueInsuficiente_DeveLancarExcecao()
    {

        var produto = Produto.Create("123", "Produto", 10m, 5, CategoriaProduto.Mercearia, null);


        Action act = () => produto.DarBaixaEstoque(6); // Tenta tirar 6 de 5


        act.Should().Throw<EstoqueInsuficienteException>();


        produto.EstoqueAtual.Should().Be(5);
    }

    [Fact]
    public void DarBaixaEstoque_QuantidadeNegativa_DeveLancarExcecao()
    {

        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);


        Action act = () => produto.DarBaixaEstoque(-2);


        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da baixa deve ser positiva.");
        produto.EstoqueAtual.Should().Be(10);
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadePositiva_DeveSomarCorretamente()
    {

        var produto = Produto.Create("789", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);


        produto.AdicionarEstoque(5);


        produto.EstoqueAtual.Should().Be(15);
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadeNegativa_DeveLancarExcecao()
    {
        var produto = Produto.Create("789", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);


        Action act = () => produto.AdicionarEstoque(-5);


        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da adição deve ser positiva.");


        produto.EstoqueAtual.Should().Be(10);
    }
}