namespace Mercado.Produto.Tests.Application;

using FluentAssertions;
using Mercado.Produto.Application;
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Moq;
using Xunit;
using ProdutoEntidade = Mercado.Produto.Domain.Produto;

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _mockRepository;
    private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        _mockRepository = new Mock<IProdutoRepository>();

        _service = new ProdutoService(_mockRepository.Object);
    }

    [Fact]
    public async Task CriarNovoProdutoAsync_ComSkuUnico_DeveSalvar()
    {
        var sku = "123";

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync((ProdutoEntidade?)null);

        var id = await _service.CriarNovoProdutoAsync(sku, "Produto Novo", 10m, 10, CategoriaProduto.Padaria, null);


        id.Should().NotBe(Guid.Empty);

        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Once());
    }

    [Fact]
    public async Task CriarNovoProdutoAsync_ComSkuDuplicado_DeveLancarExcecao()
    {

        var skuDuplicado = "123";

        var produtoExistente = ProdutoEntidade.Create(skuDuplicado, "Produto Velho", 1m, 1, CategoriaProduto.Mercearia, null);


        _mockRepository.Setup(r => r.BuscarPorSkuAsync(skuDuplicado))
            .ReturnsAsync(produtoExistente);


        Func<Task> act = async () => await _service.CriarNovoProdutoAsync(skuDuplicado, "Outro Produto", 10m, 10, CategoriaProduto.Padaria, null);


        await act.Should().ThrowAsync<ValidacaoProdutoException>()
            .WithMessage($"SKU '{skuDuplicado}' já cadastrado.");


        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task DarBaixaEstoqueAsync_ProdutoNaoEncontrado_DeveLancarExcecao()
    {

        var skuInexistente = "404";

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(skuInexistente))
            .ReturnsAsync((ProdutoEntidade?)null);


        Func<Task> act = async () => await _service.DarBaixaEstoqueAsync(skuInexistente, 1);


        await act.Should().ThrowAsync<ProdutoNaoEncontradoException>();

        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task DarBaixaEstoqueAsync_ComQuantidadeValida_DeveAtualizarEstoque()
    {
        var sku = "SKU001";
        var produto = ProdutoEntidade.Create(sku, "Produto Teste", 50m, 100, CategoriaProduto.Mercearia, null);
        var quantidadeBaixa = 30;

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync(produto);

        await _service.DarBaixaEstoqueAsync(sku, quantidadeBaixa);

        _mockRepository.Verify(r => r.SalvarAsync(It.Is<ProdutoEntidade>(p => p.EstoqueAtual == 70)),
            Times.Once());
    }

    [Fact]
    public async Task DarBaixaEstoqueAsync_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        var sku = "SKU002";
        var produto = ProdutoEntidade.Create(sku, "Produto Teste", 50m, 10, CategoriaProduto.Mercearia, null);

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync(produto);

        Func<Task> act = async () => await _service.DarBaixaEstoqueAsync(sku, 15);

        await act.Should().ThrowAsync<EstoqueInsuficienteException>();

        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task ListarTodosProdutosAsync_DeveRetornarTodosProdutos()
    {
        var produtos = new List<ProdutoEntidade>
        {
            ProdutoEntidade.Create("SKU001", "Produto 1", 10m, 5, CategoriaProduto.Mercearia, null),
            ProdutoEntidade.Create("SKU002", "Produto 2", 20m, 10, CategoriaProduto.Padaria, null),
            ProdutoEntidade.Create("SKU003", "Produto 3", 30m, 15, CategoriaProduto.Hortifruti, null)
        };

        _mockRepository.Setup(r => r.ListarTodosAsync())
            .ReturnsAsync(produtos);

        var resultado = await _service.ListarTodosProdutosAsync();

        resultado.Should().HaveCount(3);
        resultado.Should().ContainEquivalentOf(produtos[0]);
    }

    [Fact]
    public async Task BuscarPorSkuAsync_ComSkuValido_DeveRetornarProduto()
    {
        var sku = "SKU123";
        var produto = ProdutoEntidade.Create(sku, "Produto Teste", 25m, 50, CategoriaProduto.Laticinios, null);

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync(produto);

        var resultado = await _service.BuscarPorSkuAsync(sku);

        resultado.Should().NotBeNull();
        resultado?.Sku.Should().Be(sku);
    }

    [Fact]
    public async Task BuscarPorSkuAsync_ComSkuInexistente_DeveRetornarNull()
    {
        var sku = "INEXISTENTE";

        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync((ProdutoEntidade?)null);

        var resultado = await _service.BuscarPorSkuAsync(sku);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task BuscarPorIdAsync_ComIdValido_DeveRetornarProduto()
    {
        var id = Guid.NewGuid();
        var produto = ProdutoEntidade.Create("SKU456", "Produto Teste", 15m, 20, CategoriaProduto.Acougue, null);

        _mockRepository.Setup(r => r.BuscarPorIdAsync(id))
            .ReturnsAsync(produto);

        var resultado = await _service.BuscarPorIdAsync(id);

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarPorIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        var idInexistente = Guid.NewGuid();

        _mockRepository.Setup(r => r.BuscarPorIdAsync(idInexistente))
            .ReturnsAsync((ProdutoEntidade?)null);

        var resultado = await _service.BuscarPorIdAsync(idInexistente);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarProdutoAsync_ComProdutoExistente_DeveAtualizar()
    {
        var id = Guid.NewGuid();
        var skuOriginal = "SKU789";
        var produtoOriginal = ProdutoEntidade.Create(skuOriginal, "Produto Original", 10m, 100, CategoriaProduto.Bebidas, null);

        _mockRepository.Setup(r => r.BuscarPorIdAsync(id))
            .ReturnsAsync(produtoOriginal);
        _mockRepository.Setup(r => r.BuscarPorSkuAsync(It.IsAny<string>()))
            .ReturnsAsync((ProdutoEntidade?)null);

        await _service.AtualizarProdutoAsync(id, "NOVOSKU", "Produto Atualizado", 20m, 50, CategoriaProduto.Padaria, null);

        _mockRepository.Verify(r => r.SalvarAsync(It.Is<ProdutoEntidade>(p => p.Nome == "Produto Atualizado")),
            Times.Once());
    }

    [Fact]
    public async Task AtualizarProdutoAsync_ComProdutoInexistente_DeveLancarExcecao()
    {
        var idInexistente = Guid.NewGuid();

        _mockRepository.Setup(r => r.BuscarPorIdAsync(idInexistente))
            .ReturnsAsync((ProdutoEntidade?)null);

        Func<Task> act = async () => await _service.AtualizarProdutoAsync(
            idInexistente, "SKU", "Produto", 10m, 10, CategoriaProduto.Mercearia, null);

        await act.Should().ThrowAsync<ProdutoNaoEncontradoException>()
            .WithMessage($"Produto com ID '{idInexistente}' não encontrado.");

        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task AtualizarProdutoAsync_ComSkuDuplicado_DeveLancarExcecao()
    {
        var id = Guid.NewGuid();
        var skuExistente = "DUPLICADO";
        var produtoOriginal = ProdutoEntidade.Create("SKU001", "Original", 10m, 10, CategoriaProduto.Mercearia, null);
        var produtoComSkuDuplicado = ProdutoEntidade.Create(skuExistente, "Outro", 10m, 10, CategoriaProduto.Mercearia, null);

        _mockRepository.Setup(r => r.BuscarPorIdAsync(id))
            .ReturnsAsync(produtoOriginal);
        _mockRepository.Setup(r => r.BuscarPorSkuAsync(skuExistente))
            .ReturnsAsync(produtoComSkuDuplicado);

        Func<Task> act = async () => await _service.AtualizarProdutoAsync(
            id, skuExistente, "Novo Nome", 10m, 10, CategoriaProduto.Mercearia, null);

        await act.Should().ThrowAsync<ValidacaoProdutoException>()
            .WithMessage($"SKU '{skuExistente}' já cadastrado.");

        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task ExcluirProdutoAsync_ComProdutoExistente_DeveDeletar()
    {
        var id = Guid.NewGuid();
        var produto = ProdutoEntidade.Create("SKU999", "Produto Teste", 10m, 10, CategoriaProduto.Limpeza, null);

        _mockRepository.Setup(r => r.BuscarPorIdAsync(id))
            .ReturnsAsync(produto);

        await _service.ExcluirProdutoAsync(id);

        _mockRepository.Verify(r => r.DeletarAsync(id),
            Times.Once());
    }

    [Fact]
    public async Task ExcluirProdutoAsync_ComProdutoInexistente_DeveLancarExcecao()
    {
        var idInexistente = Guid.NewGuid();

        _mockRepository.Setup(r => r.BuscarPorIdAsync(idInexistente))
            .ReturnsAsync((ProdutoEntidade?)null);

        Func<Task> act = async () => await _service.ExcluirProdutoAsync(idInexistente);

        await act.Should().ThrowAsync<ProdutoNaoEncontradoException>()
            .WithMessage($"Produto com ID '{idInexistente}' não encontrado.");

        _mockRepository.Verify(r => r.DeletarAsync(It.IsAny<Guid>()),
            Times.Never());
    }
}