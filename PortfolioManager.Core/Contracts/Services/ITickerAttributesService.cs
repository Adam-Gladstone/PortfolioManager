using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Contracts.Services;

public interface ITickerAttributesService
{
    List<TickerAttribute> TickerAttributes { get; }

    void Initialize(string filename);

}
