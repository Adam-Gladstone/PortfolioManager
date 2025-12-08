namespace PortfolioManager.Contracts.Services;

public interface IDefaultStartDateOffsetService
{
    string StartDateOffset { get; }

    Task InitializeAsync();

    Task SetStartDateOffsetAsync(string offset);
}
