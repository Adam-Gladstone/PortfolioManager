using System.Text;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;


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
        
        await CreateTickerValuesTableAsync(db);

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

    private static async Task CreateTickerValuesTableAsync(SqliteConnection db)
    {
        var tableCommand = @"CREATE TABLE IF NOT 
                EXISTS TickerValues (Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                PortfolioId INTEGER NOT NULL,
                Ticker TEXT NOT NULL,
                Value REAL NOT NULL,
                CONSTRAINT fk_portfolioitems 
                FOREIGN KEY(PortfolioId) 
                REFERENCES PortfolioItems(Id)
            )";

        var createTable = new SqliteCommand(tableCommand, db);

        await createTable.ExecuteNonQueryAsync();
    }

    private static async Task CreatePortfolioItemTableAsync(SqliteConnection db)
    {
        var tableCommand = @"CREATE TABLE IF NOT 
                EXISTS PortfolioItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Name TEXT NOT NULL,
                Type TEXT,
                SymbolCode INTEGER NOT NULL,
                SymbolName TEXT NOT NULL
            )";

        var createTable = new SqliteCommand(tableCommand, db);

        await createTable.ExecuteNonQueryAsync();
    }

    private static async Task<List<PortfolioItem>> GetAllPortfolioItemsAsync(SqliteConnection db)
    {
        var results = await db.QueryAsync<PortfolioItem>
                        (
                            @"SELECT
                                    [PortfolioItems].[Id],
                                    [PortfolioItems].[Name],
                                    [PortfolioItems].[Type],
                                    [PortfolioItems].[SymbolCode],
                                    [PortfolioItems].[SymbolName]
                                FROM
                                    [PortfolioItems]"
                        );

        var portfolioItems = new List<PortfolioItem>();
        foreach (var result in results) 
        {
            var portfolioItem = portfolioItems.Find(x => x.Name == result.Name);

            if (portfolioItem != null)
            {
                portfolioItem.TickerValues = GetTickerValues(db, portfolioItem.Id);
            }
            else
            {
                portfolioItem = new PortfolioItem
                {
                    Id = result.Id,
                    Name = result.Name,
                    Type = result.Type,
                    SymbolCode = result.SymbolCode,
                    SymbolName = result.SymbolName
                };
                portfolioItem.TickerValues = GetTickerValues(db, portfolioItem.Id);

                portfolioItems.Add(portfolioItem);
            }
        }

        return portfolioItems;
    }

    private static Dictionary<string, double> GetTickerValues(SqliteConnection db, int portfolioId)
    {
        var query = @$"SELECT 
	                    [TickerValues].[Ticker],
	                    [TickerValues].[Value]
                    FROM
	                    [TickerValues]
                    WHERE
	                    [TickerValues].[PortfolioId] = {portfolioId}";

        using var command = new SqliteCommand(query, db);

        var tickerValues = new Dictionary<string, double>();

        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tickerValues[reader.GetString(0)] = reader.GetDouble(1);
            }
        }

        return tickerValues;
    }

    private static async Task<int> InsertPortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        var newIds = await db.QueryAsync<long>(
            @"INSERT INTO PortfolioItems
                    (Name, Type, SymbolCode, SymbolName)
                    VALUES
                    (@Name, @Type, @SymbolCode, @SymbolName);
                SELECT last_insert_rowid()", item);

        var portfolioId = (int)newIds.First();

        var tickerValues = item.TickerValues;
        if (tickerValues.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO TickerValues (PortfolioId, Ticker, Value) VALUES ");

            var keys = tickerValues.Keys;
            var idx = 0;
            foreach (var key in keys)
            {
                if (idx > 0 && idx < item.TickerValues.Count)
                {
                    sb.Append(",\n");
                }

                sb.Append($"({portfolioId}, \"{key}\", {tickerValues[key]})");
                ++idx;
            }

            sb.AppendLine(";");

            _ = await db.QueryAsync<long>(sb.ToString());
        }

        return portfolioId;
    }

    private static async Task UpdatePortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        await db.QueryAsync(
            @"UPDATE PortfolioItems
                  SET Name = @Name,
                    Type = @Type,
                    SymbolCode = @SymbolCode, 
                    SymbolName = @SymbolName
                  WHERE Id = @Id;", item);

        var id = item.Id;

        var sql = "DELETE FROM TickerValues WHERE PortfolioId = @id";

        using var command = new SqliteCommand(sql, db);
        command.Parameters.AddWithValue("@id", id);
        var rowDeleted = command.ExecuteNonQuery();

        var tickerValues = item.TickerValues;
        if (tickerValues.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO TickerValues (PortfolioId, Ticker, Value) VALUES ");

            var keys = tickerValues.Keys;
            var idx = 0;
            foreach (var key in keys)
            {
                if (idx > 0 && idx < item.TickerValues.Count)
                {
                    sb.Append(",\n");
                }

                sb.Append($"({id}, \"{key}\", {tickerValues[key]})");
                ++idx;
            }

            sb.AppendLine(";");

            _ = await db.QueryAsync<long>(sb.ToString());
        }
    }

    private static async Task<bool> DeletePortfolioItemAsync(SqliteConnection db, int id)
    {
        var sql = "DELETE FROM TickerValues WHERE PortfolioId = @id";

        using var command = new SqliteCommand(sql, db);
        command.Parameters.AddWithValue("@id", id);

        var rowDeleted = command.ExecuteNonQuery();

        return await db.DeleteAsync<PortfolioItem>(new PortfolioItem { Id = id });
    }
}
