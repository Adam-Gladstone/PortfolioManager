using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class DefaultRiskFreeRateService : IDefaultRiskFreeRateService
{
    private const string SettingsKey = "RiskFreeRate";

    public string RiskFreeRate { get; set; } = string.Empty;

    private readonly ILocalSettingsService localSettingsService;

    public DefaultRiskFreeRateService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        RiskFreeRate = await LoadRiskFreeRateFromSettingsAsync();

        await Task.CompletedTask;
    }

    public async Task SetRiskFreeRateAsync(string rate)
    {
        RiskFreeRate = rate;

        await SaveRiskFreeRateInSettingsAsync(RiskFreeRate);
    }

    private async Task<string> LoadRiskFreeRateFromSettingsAsync()
    {
        var rate = await localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(rate) ? string.Empty : rate;
    }

    private async Task SaveRiskFreeRateInSettingsAsync(string rate)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, rate);
    }
}
