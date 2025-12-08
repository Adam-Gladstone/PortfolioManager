using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class DefaultBenchmarkIndexService : IDefaultBenchmarkIndexService
{
    private const string SettingsKey = "BenchmarkIndex";

    public string BenchmarkIndex { get; set; } = string.Empty;

    private readonly ILocalSettingsService localSettingsService;

    public DefaultBenchmarkIndexService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        BenchmarkIndex = await LoadBenchmarkIndexFromSettingsAsync();

        await Task.CompletedTask;
    }

    public async Task SetBenchmarkIndexAsync(string index)
    {
        BenchmarkIndex = index;

        await SaveBenchmarkIndexInSettingsAsync(BenchmarkIndex);
    }

    private async Task<string> LoadBenchmarkIndexFromSettingsAsync()
    {
        var index = await localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(index) ? string.Empty : index;
    }

    private async Task SaveBenchmarkIndexInSettingsAsync(string index)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, index);
    }
}
