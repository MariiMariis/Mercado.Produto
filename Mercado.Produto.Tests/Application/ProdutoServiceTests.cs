using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercado.Produto.Tests.Application;

using Mercado.Produto.Application;
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Xunit;
using FluentAssertions;
using Moq;
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
}