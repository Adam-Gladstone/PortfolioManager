using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Contracts.Services;
public interface IPythonService
{
    void Initialize(string pythonDll);

    bool RunPortfolioAnalysis(PortfolioItem portfolio);

}
