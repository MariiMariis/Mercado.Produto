using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//O namespace espelha a localização.
namespace Mercado.Produto.Tests.Application;

// Usei o Moq para 'dublar' o repositório.
// Importei o Alias para resolver a ambiguidade do nome 'Produto'.
using Mercado.Produto.Application;
using Mercado.Produto.Domain;
using Mercado.Produto.Domain.Exceptions;
using Xunit;
using FluentAssertions;
using Moq;
using ProdutoEntidade = Mercado.Produto.Domain.Produto; // O Alias

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _mockRepository;
    private readonly ProdutoService _service; // O SUT 

    public ProdutoServiceTests()
    {
        // Arrange (Setup)
        // CRIAÇÃO DO MOCK DO REPOSITÓRIO
        _mockRepository = new Mock<IProdutoRepository>();

        // Criamos o Serviço, injetando o 'dublê' (Mock) nele.
        // _mockRepository.Object é o objeto 'dublê' que implementa IProdutoRepository
        _service = new ProdutoService(_mockRepository.Object);
    }

    [Fact]
    public async Task CriarNovoProdutoAsync_ComSkuUnico_DeveSalvar()
    {
        // Arrange (Preparar)
        var sku = "123";
        // 1. Ensinamos o roteiro ao Mock:
        // "Quando 'BuscarPorSkuAsync' for chamado com '123',
        // retorne 'null' (Task.FromResult((ProdutoEntidade?)null))."
        _mockRepository.Setup(r => r.BuscarPorSkuAsync(sku))
            .ReturnsAsync((ProdutoEntidade?)null); // Retorna null (não encontrado)

        // Act (Agir)
        // Chamamos o método real do serviço.
        var id = await _service.CriarNovoProdutoAsync(sku, "Produto Novo", 10m, 10, CategoriaProduto.Padaria, null);

        // Assert (Verificar)
        id.Should().NotBe(Guid.Empty); // Verificamos se um ID foi retornado.

        // 2. Verificamos se o Mock foi chamado como esperado:
        // "Verifique se o método 'SalvarAsync' foi chamado
        // EXATAMENTE UMA VEZ com qualquer objeto Produto."
        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Once());
    }

    [Fact]
    public async Task CriarNovoProdutoAsync_ComSkuDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var skuDuplicado = "123";
        // Criamos um produto falso para o Mock retornar.
        var produtoExistente = ProdutoEntidade.Create(skuDuplicado, "Produto Velho", 1m, 1, CategoriaProduto.Mercearia, null);

        // 1. Ensinamos o Mock:
        // "Quando 'BuscarPorSkuAsync' for chamado com '123',
        // retorne o 'produtoExistente'."
        _mockRepository.Setup(r => r.BuscarPorSkuAsync(skuDuplicado))
            .ReturnsAsync(produtoExistente);

        // Act
        // Envolvemos a chamada assíncrona em uma 'Func<Task>'
        Func<Task> act = async () => await _service.CriarNovoProdutoAsync(skuDuplicado, "Outro Produto", 10m, 10, CategoriaProduto.Padaria, null);

        // Assert
        // "A ação DEVE lançar 'ValidacaoProdutoException'"
        await act.Should().ThrowAsync<ValidacaoProdutoException>()
            .WithMessage($"SKU '{skuDuplicado}' já cadastrado.");

        // 2. Verificação CRÍTICA:
        // "Verifique se o método 'SalvarAsync' NUNCA foi chamado."
        // Isso prova que nossa validação parou a execução.
        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }

    [Fact]
    public async Task DarBaixaEstoqueAsync_ProdutoNaoEncontrado_DeveLancarExcecao()
    {
        // Arrange
        var skuInexistente = "404";
        // 1. Ensinamos o Mock:
        // "Quando 'BuscarPorSkuAsync' for chamado com '404', retorne null."
        _mockRepository.Setup(r => r.BuscarPorSkuAsync(skuInexistente))
            .ReturnsAsync((ProdutoEntidade?)null);

        // Act
        Func<Task> act = async () => await _service.DarBaixaEstoqueAsync(skuInexistente, 1);

        // Assert
        // "A ação DEVE lançar 'ProdutoNaoEncontradoException'"
        await act.Should().ThrowAsync<ProdutoNaoEncontradoException>();

        // Verificamos que 'Salvar' também não foi chamado.
        _mockRepository.Verify(r => r.SalvarAsync(It.IsAny<ProdutoEntidade>()),
            Times.Never());
    }
}