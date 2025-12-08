namespace PortfolioManager.Contracts.Services;

public interface IDefaultBenchmarkIndexService
{
    string BenchmarkIndex { get; }

    Task InitializeAsync();

    Task SetBenchmarkIndexAsync(string index);
}
