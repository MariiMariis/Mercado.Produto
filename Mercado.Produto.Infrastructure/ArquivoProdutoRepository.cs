namespace Mercado.Produto.Infrastructure;

using Mercado.Produto.Domain;
using System.Text.Json;
using System.Text.Json.Serialization;

/// Implementação de repositório persistente que salva os dados
/// em um arquivo JSON local.
public class ArquivoProdutoRepository : IProdutoRepository
{
    // O nome do  arquivo de banco de dados
    private const string DB_PATH = "mercado.db.json";

    // CRÍTICO! Um "semáforo" (lock) para garantir que
    // apenas uma operação aceda ao arquivo de cada vez.
    // Previne corrupção de dados.
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    // Estas são as regras explícitas para o serializador JSON.
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        // 1. Torna o arquivo JSON legível
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    //Métodos de Leitura e Escrita (Helpers)
    /// Lê e desserializa todos os produtos do arquivo de banco de dados.
    private async Task<Dictionary<Guid, Produto>> LerDatabaseAsync()
    {
        if (!File.Exists(DB_PATH))
        {
            return new Dictionary<Guid, Produto>();
        }

        string json = await File.ReadAllTextAsync(DB_PATH);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<Guid, Produto>();
        }

        // '_jsonOptions' para ler o arquivo
        // corretamente, entendendo os Enums como strings
        return JsonSerializer.Deserialize<Dictionary<Guid, Produto>>(json, _jsonOptions)
            ?? new Dictionary<Guid, Produto>();
    }

    /// Serializa e escreve o banco de dados completo no arquivo.
    private async Task EscreverDatabaseAsync(Dictionary<Guid, Produto> database)
    {
        // '_jsonOptions' para escrever o arquivo
        // corretamente, salvando Enums como strings.
        string json = JsonSerializer.Serialize(database, _jsonOptions);
        await File.WriteAllTextAsync(DB_PATH, json);
    }

    // Implementação da Interface IProdutoRepository

    public async Task SalvarAsync(Produto produto)
    {
        // Espera a sua vez de aceder ao arquivo
        await _lock.WaitAsync();
        try
        {
            // 2Lê TODOS os dados do disco
            var db = await LerDatabaseAsync();

            // 3. Modifica os dados na memória (Adiciona ou Atualiza)
            db[produto.Id] = produto;

            // 4. Escreve TODOS os dados de volta ao disco
            await EscreverDatabaseAsync(db);
        }
        finally
        {
            // 5. Liberta o "semáforo" para a próxima operação.
            _lock.Release();
        }
    }

    public async Task<Produto?> BuscarPorIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var db = await LerDatabaseAsync();
            db.TryGetValue(id, out var produto);
            return produto;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Produto?> BuscarPorSkuAsync(string sku)
    {
        await _lock.WaitAsync();
        try
        {
            var db = await LerDatabaseAsync();
            // PORQUÊ: LINQ. Encontra o primeiro produto (ou null)
            // cujo SKU corresponde.
            return db.Values.FirstOrDefault(p => p.Sku == sku);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<Produto>> ListarTodosAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var db = await LerDatabaseAsync();
            return db.Values.ToList(); // Retorna uma nova lista
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeletarAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var db = await LerDatabaseAsync();
            if (db.Remove(id)) // Se a remoção foi bem-sucedida...
            {
                await EscreverDatabaseAsync(db); // ...salva as mudanças.
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}