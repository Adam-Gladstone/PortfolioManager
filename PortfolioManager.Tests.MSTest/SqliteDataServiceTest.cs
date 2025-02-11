using PortfolioManager.Core.Models;
using PortfolioManager.Core.Services;

namespace PortfolioManager.Tests.MSTest;

[TestClass]
public class SqliteDataServiceTest
{
    private const string dbPath = @"D:\Development\Projects\C#\PortfolioManager\PortfolioManager.Tests.MSTest\Data\";

    private static readonly List<PortfolioItem> entries = new()
    {
        new PortfolioItem
        {
            Id = 1,
            Name = "Test1",
            Type = "Equity",
            SymbolCode = 57808,
            SymbolName = "Calculator",
            TickerValues = new Dictionary<string, double>() 
            { 
                { "SAB.MC", 1000 }, 
                { "FER.MC", 750.0 }, 
                { "ITX.MC", 250 }, 
                { "MEL.MC", 2000.0 } 
            }
        }
    };

    [TestMethod]
    public void TestAddItem()
    {
        // Arrange
        var dataService = new SqliteDataService
        {
            DbPath = $"{dbPath}{Path.GetRandomFileName()}"
        };
        _ = dataService.InitializeDataAsync();

        var portfolioItem = entries[0];

        // Act
        var id = dataService.AddItemAsync(portfolioItem).Result;

        // Assert
        Assert.AreEqual(id, 1);
    }

    [TestMethod]
    public void TestGetItem()
    {
        var dataService = new SqliteDataService
        {
            DbPath = $"{dbPath}{Path.GetRandomFileName()}"
        };
        _ = dataService.InitializeDataAsync();

        var portfolioItem = entries[0];

        var id = dataService.AddItemAsync(portfolioItem).Result;

        var target = dataService.GetItemAsync(id).Result;

        Assert.AreEqual(portfolioItem.Id, target.Id);
        Assert.AreEqual(portfolioItem.Type, target.Type);

        var tickerValues = portfolioItem.TickerValues;
        var targetTickerValues = target.TickerValues;
        foreach (var key in tickerValues.Keys)
        {
            Assert.AreEqual(tickerValues[key], targetTickerValues[key]);
        }
    }

    [TestMethod]
    public void TestGetItems()
    {
        var dataService = new SqliteDataService
        {
            DbPath = $"{dbPath}{Path.GetRandomFileName()}"
        };
        _ = dataService.InitializeDataAsync();

        foreach (var entry in entries)
        {
            _ = dataService.AddItemAsync(entry).Result;
        }

        var targets = dataService.GetItemsAsync().Result;

        foreach (var target in targets)
        {
            Assert.AreEqual(entries[target.Id - 1].Id, target.Id);
            Assert.AreEqual(entries[target.Id - 1].Name, target.Name);
            Assert.AreEqual(entries[target.Id - 1].Type, target.Type);
        }
    }

    [TestMethod]
    public void TestDeleteItem()
    {
        // Arrange
        var dataService = new SqliteDataService
        {
            DbPath = $"{dbPath}{Path.GetRandomFileName()}"
        };
        _ = dataService.InitializeDataAsync();

        foreach (var entry in entries)
        {
            _ = dataService.AddItemAsync(entry).Result;
        }

        // Act
        var targets = dataService.GetItemsAsync().Result;
        Assert.AreEqual(entries.Count, targets.Count);

        var portfolioItem = entries[0];

        var result = dataService.DeleteItemAsync(portfolioItem).Result;
        Assert.IsTrue(result);

        targets = dataService.GetItemsAsync().Result;

        Assert.AreEqual(entries.Count - 1, targets.Count);

        var found = false;
        foreach (var target in targets)
        {
            if (target.Name == portfolioItem.Name)
            {
                found = true; break;
            }
        }

        Assert.IsFalse(found);
    }

    [TestMethod]
    public void TestUpdateItem()
    {
        // Arrange
        var dataService = new SqliteDataService
        {
            DbPath = $"{dbPath}{Path.GetRandomFileName()}"
        };
        _ = dataService.InitializeDataAsync();

        foreach (var entry in entries)
        {
            _ = dataService.AddItemAsync(entry).Result;
        }

        // Act
        foreach (var entry in entries)
        {
            entry.Type = "Market Equity";
            _ = dataService.UpdateItemAsync(entry);
        }

        var targets = dataService.GetItemsAsync().Result;

        // Assert
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var entry = entries[i];
            Assert.AreEqual(entry.Type, target.Type);
        }
    }
}
