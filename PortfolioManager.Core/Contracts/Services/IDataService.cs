using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Contracts.Services;
public interface IDataService
{
    Task InitializeDataAsync();

    Task<int> AddItemAsync(PortfolioItem item);

    Task<PortfolioItem> GetItemAsync(int id);

    Task<IList<PortfolioItem>> GetItemsAsync();

    Task<bool> DeleteItemAsync(PortfolioItem item);

    Task UpdateItemAsync(PortfolioItem item);
}
