using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Contracts.Services;
public interface IPythonService
{
    Task InitializeAsync(string pythonDll, string scriptFolder);

    void Shutdown();

    bool RunPortfolioAnalysis(PortfolioItem portfolio);

    bool RetrievePortfolioData(PortfolioItem portfolio);

    bool RetrieveDividendCalendarData(PortfolioItem portfolio);
}
