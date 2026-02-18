namespace Mercado.Produto.Tests.Domain;

using FluentAssertions;
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Xunit;

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

    [Fact]
    public void Create_DataValidadeNoPassado_DeveLancarExcecao()
    {
        var dataPassada = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        Action act = () => Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, dataPassada);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Data de validade não pode estar no passado.");
    }

    [Fact]
    public void Create_ComNomeNoTamanhoMaximo_DeveCriar()
    {
        var nomeLongo = new string('A', 100);

        var produto = Produto.Create("123", nomeLongo, 10m, 10, CategoriaProduto.Mercearia, null);

        produto.Should().NotBeNull();
        produto.Nome.Should().HaveLength(100);
    }

    [Fact]
    public void Create_ComNomeAcimaTamanhoMaximo_DeveLancarExcecao()
    {
        var nomeMuitoLongo = new string('A', 101);

        Action act = () => Produto.Create("123", nomeMuitoLongo, 10m, 10, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>();
    }

    [Fact]
    public void Create_SkuNulo_DeveLancarExcecao()
    {
        Action act = () => Produto.Create(null!, "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("SKU é obrigatório.");
    }

    [Fact]
    public void Create_NomeNulo_DeveLancarExcecao()
    {
        Action act = () => Produto.Create("123", null!, 10m, 10, CategoriaProduto.Mercearia, null);

        act.Should().Throw<ValidacaoProdutoException>();
    }

    [Fact]
    public void DarBaixaEstoque_ComQuantidadeZero_DeveLancarExcecao()
    {
        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        Action act = () => produto.DarBaixaEstoque(0);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da baixa deve ser positiva.");
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadeZero_DeveLancarExcecao()
    {
        var produto = Produto.Create("789", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        Action act = () => produto.AdicionarEstoque(0);

        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da adição deve ser positiva.");
    }

    [Fact]
    public void Create_PrecoZero_DeveCriar()
    {
        var produto = Produto.Create("123", "Produto", 0m, 10, CategoriaProduto.Mercearia, null);

        produto.Should().NotBeNull();
        produto.PrecoVenda.Should().Be(0m);
    }

    [Fact]
    public void Create_EstoqueZero_DeveCriar()
    {
        var produto = Produto.Create("123", "Produto", 10m, 0, CategoriaProduto.Mercearia, null);

        produto.Should().NotBeNull();
        produto.EstoqueAtual.Should().Be(0);
    }

    [Fact]
    public void Create_ComDataValidadeValida_DeveCriar()
    {
        var dataFutura = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, dataFutura);

        produto.Should().NotBeNull();
        produto.DataValidade.Should().Be(dataFutura);
    }

    [Fact]
    public void Equals_ComProdutosComMesmoId_DeveRetornarTrue()
    {
        var id = Guid.NewGuid();
        var produto1 = new Produto(id, "SKU1", "Produto 1", 10m, CategoriaProduto.Mercearia, null, 10);
        var produto2 = new Produto(id, "SKU2", "Produto 2", 20m, CategoriaProduto.Padaria, null, 20);

        (produto1 == produto2).Should().BeFalse();
        produto1.Equals(produto2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ComProdutosComIdDiferente_DeveRetornarFalse()
    {
        var produto1 = Produto.Create("SKU1", "Produto 1", 10m, 10, CategoriaProduto.Mercearia, null);
        var produto2 = Produto.Create("SKU2", "Produto 2", 20m, 20, CategoriaProduto.Padaria, null);

        produto1.Equals(produto2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ComObjNulo_DeveRetornarFalse()
    {
        var produto = Produto.Create("SKU1", "Produto 1", 10m, 10, CategoriaProduto.Mercearia, null);

        produto.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ComObjOutroTipo_DeveRetornarFalse()
    {
        var produto = Produto.Create("SKU1", "Produto 1", 10m, 10, CategoriaProduto.Mercearia, null);

        produto.Equals("string").Should().BeFalse();
    }

    [Fact]
    public void ToString_DeveRetornarFormatoEsperado()
    {
        var produto = Produto.Create("SKU123", "Produto Teste", 10.50m, 100, CategoriaProduto.Mercearia, null);

        var resultado = produto.ToString();

        resultado.Should().Contain("SKU123");
        resultado.Should().Contain("Produto Teste");
        resultado.Should().Contain("100");
    }

    [Fact]
    public void DarBaixaEstoque_Multiplas_DeveSubtrairCorretamente()
    {
        var produto = Produto.Create("123", "Produto", 10m, 100, CategoriaProduto.Mercearia, null);

        produto.DarBaixaEstoque(25);
        produto.DarBaixaEstoque(25);
        produto.DarBaixaEstoque(25);

        produto.EstoqueAtual.Should().Be(25);
    }

    [Fact]
    public void Create_ComTodasAsCategorias_DeveCriarSucesso()
    {
        var categorias = System.Enum.GetValues(typeof(CategoriaProduto));

        foreach (CategoriaProduto categoria in categorias)
        {
            var produto = Produto.Create("SKU", "Produto", 10m, 10, categoria, null);
            produto.Categoria.Should().Be(categoria);
        }
    }
}