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
    public static string DbFilename => dbFilename;

    private string dbPath = "";

    public string DbPath
    {
        get => dbPath;
        set => dbPath = value;
    }

    public async Task InitializeDataAsync()
    {
        using var db = GetOpenConnectionAsync();

        await CreateTransactionsTableAsync(db);

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

    private static async Task CreateTransactionsTableAsync(SqliteConnection db)
    {
        var tableCommand = @"CREATE TABLE IF NOT 
                EXISTS Transactions (Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                PortfolioId INTEGER NOT NULL,
                Date DATE NOT NULL,
                Ticker TEXT NOT NULL,
                Type TEXT NOT NULL,
                Shares INTEGER NOT NULL,
                Price REAL NOT NULL,
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
                Type TEXT
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
                                    [PortfolioItems].[Type]
                                FROM
                                    [PortfolioItems]"
                        );

        var portfolioItems = new List<PortfolioItem>();
        foreach (var result in results) 
        {
            var portfolioItem = portfolioItems.Find(x => x.Name == result.Name);

            if (portfolioItem != null)
            {
                portfolioItem.Transactions = GetTransactions(db, portfolioItem.Id);

                portfolioItem.Holdings = GetHoldings(db, portfolioItem.Id);

                portfolioItem.TotalHoldings = portfolioItem.Holdings.Sum(x => x.TotalCost);
            }
            else
            {
                portfolioItem = new PortfolioItem
                {
                    Id = result.Id,
                    Name = result.Name,
                    Type = result.Type,
                };

                portfolioItem.Transactions = GetTransactions(db, portfolioItem.Id);

                portfolioItem.Holdings = GetHoldings(db, portfolioItem.Id);

                portfolioItem.TotalHoldings = portfolioItem.Holdings.Sum(x => x.TotalCost);

                // Update the percentage
                foreach (var holding in portfolioItem.Holdings)
                {
                    holding.Percent = holding.TotalCost / portfolioItem.TotalHoldings * 100.0;
                }

                portfolioItems.Add(portfolioItem);
            }
        }

        return portfolioItems;
    }

    private static List<Transaction> GetTransactions(SqliteConnection db, int portfolioId)
    {
        var query = @$"SELECT
                        [Transactions].[Id],
                        [Transactions].[Date],
                        [Transactions].[Ticker],
                        [Transactions].[Type],
                        [Transactions].[Shares],
                        [Transactions].[Price]
                    FROM
	                    [Transactions]
                    WHERE
	                    [Transactions].[PortfolioId] = {portfolioId}
                        ";

        using var command = new SqliteCommand(query, db);

        var transactions = new List<Transaction>();

        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var transaction = new Transaction()
                {
                    TransactionId = reader.GetInt32(0),
                    Date = reader.GetDateTime(1),
                    Ticker = reader.GetString(2),
                    Type = reader.GetString(3),
                    Shares = reader.GetInt64(4),
                    Price = reader.GetDouble(5),
                };

                transactions.Add(transaction);
            }
        }

        return transactions;
    }

    private static List<Holding> GetHoldings(SqliteConnection db, int portfolioId)
    {
        var query = @$"SELECT 
                        [Transactions].[Id],
                        [Transactions].[Date],
                        [Transactions].[Ticker],
                        [Transactions].[Type],
                        [Transactions].[Shares],
                        [Transactions].[Price]
                    FROM
	                    [Transactions]
                    WHERE
	                    [Transactions].[PortfolioId] = {portfolioId}
                        ";

        using var command = new SqliteCommand(query, db);

        var holdings = new Dictionary<string, Holding>();

        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var ticker = reader.GetString(2);
                var type = reader.GetString(3);
                long multiple = (type == "BUY") ? 1 : -1;
                var shares = reader.GetInt64(4);
                var price = reader.GetDouble(5);

                var holding = holdings.TryGetValue(ticker, out var value) ? value : new Holding();

                holding.Ticker = ticker;
                holding.TotalShares += shares * multiple;
                holding.TotalCost += shares * price;

                holding.AverageCost = (holding.TotalShares != 0) ? (holding.TotalCost / holding.TotalShares) : 0;

                holdings[ticker] = holding;
            }
        }

        return [.. holdings.Values.Where(x => x.TotalShares > 0)];
    }

    private static async Task<int> InsertPortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        var newIds = await db.QueryAsync<long>(
            @"INSERT INTO PortfolioItems
                    (Name, Type)
                    VALUES
                    (@Name, @Type);
                SELECT last_insert_rowid()", item);

        var portfolioId = (int)newIds.First();

        var transactions = item.Transactions;
        if (transactions.Count > 0)
        {
            await InsertTransactionsAsync(db, portfolioId, transactions);
        }

        return portfolioId;
    }

    private static async Task UpdatePortfolioItemAsync(SqliteConnection db, PortfolioItem item)
    {
        await db.QueryAsync(
            @"UPDATE PortfolioItems
                  SET Name = @Name,
                    Type = @Type
                  WHERE Id = @Id;", item);

        var id = item.Id;

        var transactions = item.Transactions;
        if (transactions.Count > 0)
        {
            await InsertTransactionsAsync(db, id, transactions);
        }
    }

    private static async Task<bool> DeletePortfolioItemAsync(SqliteConnection db, int id)
    {
        var sql = "DELETE FROM Transactions WHERE PortfolioId = @id";

        using var command = new SqliteCommand(sql, db);
        command.Parameters.AddWithValue("@id", id);

        var rowDeleted = command.ExecuteNonQuery();

        return await db.DeleteAsync<PortfolioItem>(new PortfolioItem { Id = id });
    }

    private static async Task<int> InsertTransactionsAsync(SqliteConnection db, int portfolioId, List<Models.Transaction> transactions)
    {
        var idx = 0;

        var sb = new StringBuilder();

        sb.Append("INSERT INTO Transactions (PortfolioId, Date, Ticker, Type, Shares, Price) VALUES ");

        foreach (var transaction in transactions)
        {
            if (idx > 0 && idx < transactions.Count)
            {
                sb.Append(",\n");
            }

            var date = transaction.Date.ToString("yyyy-MM-dd");

            sb.Append($"({portfolioId}, \"{date}\", \"{transaction.Ticker}\", \"{transaction.Type}\", {transaction.Shares}, {transaction.Price})");
            ++idx;
        }

        sb.AppendLine(";");

        _ = await db.QueryAsync<long>(sb.ToString());

        return portfolioId;
    }
}
