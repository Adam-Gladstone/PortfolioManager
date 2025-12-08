namespace PortfolioManager.Contracts.Services;

public interface IDefaultRiskFreeRateService
{
    string RiskFreeRate {  get;  }

    Task InitializeAsync();

    Task SetRiskFreeRateAsync(string rate);
}
