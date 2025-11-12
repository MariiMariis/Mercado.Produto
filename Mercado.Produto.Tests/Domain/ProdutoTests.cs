using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using System.Drawing;
using Xunit; // Pacote xUnit
using FluentAssertions; // Pacote FluentAssertions

// PORQUÊ: O namespace espelha a localização do arquivo.
namespace Mercado.Produto.Tests.Domain;

// PORQUÊ: Importamos as ferramentas de teste (Xunit, FluentAssertions)
// e as classes que estamos testando (Domínio e Exceções do Domínio).
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Xunit;
using FluentAssertions;
using System.Security.Cryptography;

// PORQUÊ: O nome da classe de teste termina com 'Tests'
public class ProdutoTests
{
    // PORQUÊ: [Fact] marca um teste simples, sem parâmetros.
    [Fact]
    public void Create_ComDadosValidos_DeveCriarProduto()
    {
        // Arrange (Preparar)
        var sku = "123";
        var nome = "Produto Bom";
        var preco = 10.0m; // 'm' para decimal
        var estoque = 10;
        var categoria = CategoriaProduto.Mercearia;

        // Act (Agir)
        // Usamos o método de fábrica estático 'Create' que definimos.
        var produto = Produto.Create(sku, nome, preco, estoque, categoria, null);

        // Assert (Verificar)
        // PORQUÊ: Usamos FluentAssertions para legibilidade.
        // "O produto NÃO DEVE ser nulo"
        produto.Should().NotBeNull();
        // "O nome do produto DEVE SER 'Produto Bom'"
        produto.Nome.Should().Be(nome);
        produto.EstoqueAtual.Should().Be(estoque);
        produto.Id.Should().NotBe(Guid.Empty); // Deve ter gerado um ID
    }

    // PORQUÊ: [Theory] permite um teste que roda com múltiplos dados.
    // [InlineData] fornece esses dados.
    [Theory]
    [InlineData(-1.0)]  // Teste de limite
    [InlineData(-0.01)] // Teste de limite
    public void Create_PrecoNegativo_DeveLancarExcecao(double precoDouble)
    {
        // Arrange
        var preco = (decimal)precoDouble;
        // PORQUÊ: Testamos a ação (Act) de criar.
        // Envolvemos a chamada de criação em uma 'Action'.
        Action act = () => Produto.Create("123", "Produto", preco, 10, CategoriaProduto.Mercearia, null);

        // Assert
        // "A ação DEVE lançar 'ValidacaoProdutoException'
        // E a mensagem DEVE SER 'Preço não pode ser negativo.'"
        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Preço não pode ser negativo.");
    }

    [Fact]
    public void Create_NomeCurto_DeveLancarExcecao()
    {
        // Arrange
        Action act = () => Produto.Create("123", "Ab", 10m, 10, CategoriaProduto.Mercearia, null);

        // Assert
        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Nome deve ter entre 3 e 100 caracteres."); // A mensagem exata importa!
    }

    [Fact]
    public void Create_EstoqueNegativo_DeveLancarExcecao()
    {
        // Arrange
        Action act = () => Produto.Create("123", "Produto", 10m, -1, CategoriaProduto.Mercearia, null);

        // Assert
        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Estoque inicial não pode ser negativo.");
    }


    [Fact]
    public void DarBaixaEstoque_EstoqueSuficiente_DeveSubtrair()
    {
        // Arrange
        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        // Act
        produto.DarBaixaEstoque(7);

        // Assert
        produto.EstoqueAtual.Should().Be(3);
    }

    [Fact]
    public void DarBaixaEstoque_EstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var produto = Produto.Create("123", "Produto", 10m, 5, CategoriaProduto.Mercearia, null);

        // Act
        Action act = () => produto.DarBaixaEstoque(6); // Tenta tirar 6 de 5

        // Assert
        act.Should().Throw<EstoqueInsuficienteException>();

        // PORQUÊ: Fundamental! Verificamos que o estado não foi corrompido.
        // O estoque DEVE permanecer 5.
        produto.EstoqueAtual.Should().Be(5);
    }

    [Fact]
    public void DarBaixaEstoque_QuantidadeNegativa_DeveLancarExcecao()
    {
        // Arrange
        var produto = Produto.Create("123", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        // Act
        Action act = () => produto.DarBaixaEstoque(-2);

        // Assert
        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da baixa deve ser positiva.");
        produto.EstoqueAtual.Should().Be(10); // Estado não deve mudar.
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadePositiva_DeveSomarCorretamente()
    {
        // Arrange (Preparar)
        var produto = Produto.Create("789", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        // Act (Agir)
        produto.AdicionarEstoque(5);

        // Assert (Verificar)
        produto.EstoqueAtual.Should().Be(15);
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadeNegativa_DeveLancarExcecao()
    {
        // Arrange
        var produto = Produto.Create("789", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        // Act
        // Verifica o caminho de falha (a linha 'if (quantidade <= 0)')
        Action act = () => produto.AdicionarEstoque(-5);

        // Assert
        act.Should().Throw<ValidacaoProdutoException>()
            .WithMessage("Quantidade da adição deve ser positiva.");

        // Garante que o estado não foi corrompido
        produto.EstoqueAtual.Should().Be(10);
    }
}