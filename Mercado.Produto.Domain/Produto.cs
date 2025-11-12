using Mercado.Produto.Domain.Exceptions;
using System.Globalization;
//
// --- A CORREÇÃO CRÍTICA (PASSO 1) ---
//
// PORQUÊ: Esta é a 'chave' (o 'using') que faltava.
// Ela diz ao compilador onde encontrar a ferramenta [JsonConstructor].
//
using System.Text.Json.Serialization;

namespace Mercado.Produto.Domain;

/// <summary>
/// Representa a entidade Produto.
/// É o "cofre" que guarda as regras de negócio.
/// </summary>
public class Produto
{
    // PORQUÊ: Constantes para evitar "Valores Mágicos" (Artefato 5)
    private const int TAMANHO_MINIMO_NOME = 3;
    private const int TAMANHO_MAXIMO_NOME = 100;

    // PORQUÊ: Propriedades com 'private set' (Imutabilidade Parcial).
    // Só podem ser alteradas por métodos DENTRO desta classe.
    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public CategoriaProduto Categoria { get; private set; }
    public DateOnly? DataValidade { get; private set; }
    public int EstoqueAtual { get; private set; }

    // PORQUÊ: Um construtor privado para forçar o uso
    // do método de fábrica 'Create' e garantir a inicialização correta.
    private Produto()
    {
        // Garante que strings nunca sejam nulas
        Sku = string.Empty;
        Nome = string.Empty;
    }

    //
    // --- A "PORTA DE SERVIÇO" (CORREÇÃO PASSO 2) ---
    //
    // PORQUÊ: O [JsonConstructor] diz ao Serializador
    // para usar este construtor ao "re-hidratar" (ler) o objeto
    // do arquivo JSON.
    [JsonConstructor]
    public Produto(Guid id, string sku, string nome, decimal precoVenda, CategoriaProduto categoria, DateOnly? dataValidade, int estoqueAtual)
    {
        // PORQUÊ: Este construtor "confia" nos dados do banco.
        // Ele simplesmente atribui os valores lidos.
        Id = id;
        Sku = sku;
        Nome = nome;
        PrecoVenda = precoVenda;
        Categoria = categoria;
        DataValidade = dataValidade;
        EstoqueAtual = estoqueAtual;
    }

    /// <summary>
    /// O "PORTÃO DA FRENTE": Método de fábrica para criar um NOVO Produto.
    /// Garante que todas as regras de negócio sejam validadas.
    /// </summary>
    public static Produto Create(string sku, string nome, decimal precoVenda, int estoqueAtual, CategoriaProduto categoria, DateOnly? dataValidade)
    {
        // --- O Portão de Validação (O Guardião) ---
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

        // --- Fim do Portão ---

        // PORQUÊ: Usa o construtor privado e gera um NOVO Guid.
        return new Produto
        {
            Id = Guid.NewGuid(), // Gera um NOVO Id
            Sku = sku,
            Nome = nome,
            PrecoVenda = precoVenda,
            EstoqueAtual = estoqueAtual,
            Categoria = categoria,
            DataValidade = dataValidade
        };
    }

    // --- Comandos (CQS - Artefato 5) ---
    // Métodos que alteram o estado de forma controlada.

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

    // --- Métodos de Infraestrutura ---

    public override string ToString()
    {
        // Formata o preço para a cultura pt-BR (R$)
        return $"Produto [Id={Id}, Sku={Sku}, Nome={Nome}, Preco={PrecoVenda.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}, Estoque={EstoqueAtual}]";
    }

    // PORQUÊ: Entidades são comparadas por ID.
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