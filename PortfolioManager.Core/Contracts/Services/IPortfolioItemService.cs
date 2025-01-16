using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Contracts.Services;
public interface IPortfolioItemService
{
    void InitializeData();

    List<PortfolioItem> GetData();
}
