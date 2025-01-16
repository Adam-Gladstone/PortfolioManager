using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;


namespace PortfolioManager.Core.Services;
public class SqliteDataService : IDataService
{
    private const string dbFilename = "PortfolioData.db";
    public string DbFilename => dbFilename;

    private string dbPath = "";

    public string DbPath
    {
        get => dbPath;
        set => dbPath = value;
    }

    public async Task InitializeDataAsync()
    {
        using var db = GetOpenConnectionAsync();
        await CreatePortfolioItemTableAsync(db);
    }

    public async Task<int> AddItemAsync(PortfolioItem item)
    {
        using var db = GetOpenConnectionAsync();
        return await InsertPortfolioItemAsync(db, item);
    }

    public async Task<PortfolioItem> GetItemAsync(int id)
    {
        IList<PortfolioItem> portfolioItems;

        using (var db = GetOpenConnectionAsync())
        {
            portfolioItems = await GetAllPortfolioItemsAsync(db);
        }

        // Filter the list to get the item for our Id.
        return portfolioItems.FirstOrDefault(i => i.Id == id);
    }

    public async Task<IList<PortfolioItem>> GetItemsAsync()
    {
        using var db = GetOpenConnectionAsync();
        return await GetAllPortfolioItemsAsync(db);
    }

    public async Task<bool> DeleteItemAsync(PortfolioItem item)
    {
        using var db = GetOpenConnectionAsync();
        return await DeletePortfolioItemAsync(db, item.Id);
    }

    public async Task UpdateItemAsync(PortfolioItem item)
    {
        using var db = GetOpenConnectionAsync();
        await UpdatePortfolioItemAsync(db, item);
    }

    private SqliteConnection GetOpenConnectionAsync()
    {
        var connection = new SqliteConnection($"Filename={DbPath}");
        connection.Open();

        return connection;
    }

    private static async Task CreatePortfolioItemTableAsync(SqliteConnection db)
    {
        var tableCommand = @"CREATE TABLE IF NOT 
                EXISTS PortfolioItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                NAME TEXT NOT NULL
                )";

        var createTable = new SqliteCommand(tableCommand, db);

        await createTable.ExecuteNonQueryAsync();
    }

    private static async Task<List<PortfolioItem>> GetAllPortfolioItemsAsync(SqliteConnection db)
    {
        var itemsResult = await db.QueryAsync<PortfolioItem>
                        (
                            @"SELECT
                                    [PortfolioItems].[Id],
                                    [PortfolioItems].[Name]
                                FROM
                                    [PortfolioItems]"
                        );

        return itemsResult.ToList();
    }

    private static async Task<int> InsertPortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        var newIds = await db.QueryAsync<long>(
            @"INSERT INTO PortfolioItems
                    (Name)
                    VALUES
                    (@Name);
                SELECT last_insert_rowid()", item);

        return (int)newIds.First();
    }

    private static async Task UpdatePortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        await db.QueryAsync(
            @"UPDATE PortfolioItems
                  SET Name = @Name
                  WHERE Id = @Id;", item);
    }

    private static async Task<bool> DeletePortfolioItemAsync(SqliteConnection db, int id)
    {
        // This will look in the table PortfolioItems
        return await db.DeleteAsync<PortfolioItem>(new PortfolioItem { Id = id });
    }
}
