using Mercado.Produto.Domain.Exceptions;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Mercado.Produto.Domain;

/// Representa a entidade Produto, guardando as regras de negócio
public class Produto
{
    //Constantes para evitar "Valores Mágicos"
    private const int TAMANHO_MINIMO_NOME = 3;
    private const int TAMANHO_MAXIMO_NOME = 100;

    //Propriedades com 'private set' (Imutabilidade Parcial).
    //Só podem ser alteradas por métodos dentro desta classe.
    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public CategoriaProduto Categoria { get; private set; }
    public DateOnly? DataValidade { get; private set; }
    public int EstoqueAtual { get; private set; }

    //Um construtor privado para forçar o uso
    //do método de fábrica 'Create' e garantir a inicialização correta
    private Produto()
    {
        // Garante que strings nunca sejam nulas
        Sku = string.Empty;
        Nome = string.Empty;
    }

    //O [JsonConstructor] diz ao Serializador
    //para usar este construtor ao ler o objeto
    [JsonConstructor]
    public Produto(Guid id, string sku, string nome, decimal precoVenda, CategoriaProduto categoria, DateOnly? dataValidade, int estoqueAtual)
    {
        //Este construtor "confia" nos dados do banco.
        //Ele simplesmente atribui os valores lidos.
        Id = id;
        Sku = sku;
        Nome = nome;
        PrecoVenda = precoVenda;
        Categoria = categoria;
        DataValidade = dataValidade;
        EstoqueAtual = estoqueAtual;
    }

    ///Método que cria um novo porduto
    ///Garante que todas as regras de negócio sejam validadas.
    public static Produto Create(string sku, string nome, decimal precoVenda, int estoqueAtual, CategoriaProduto categoria, DateOnly? dataValidade)
    {
        // validador
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


        //Usa o construtor privado e gera um NOVO Guid
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

    public override string ToString()
    {
        // Formata o preço para a cultura pt-BR (R$)
        return $"Produto [Id={Id}, Sku={Sku}, Nome={Nome}, Preco={PrecoVenda.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}, Estoque={EstoqueAtual}]";
    }

    //Entidades são comparadas por ID.
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