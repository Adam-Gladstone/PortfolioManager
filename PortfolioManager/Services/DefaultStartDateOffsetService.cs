using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class DefaultStartDateOffsetService : IDefaultStartDateOffsetService
{
    private const string SettingsKey = "StartDateOffset";

    public string StartDateOffset { get; set; } = string.Empty;

    private readonly ILocalSettingsService localSettingsService;

    public DefaultStartDateOffsetService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        StartDateOffset = await LoadStartDateOffsetFromSettingsAsync();

        await Task.CompletedTask;
    }

    public async Task SetStartDateOffsetAsync(string offset)
    {
        StartDateOffset = offset;

        await SaveStartDateOffsetInSettingsAsync(StartDateOffset);
    }

    private async Task<string> LoadStartDateOffsetFromSettingsAsync()
    {
        var offset = await localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(offset) ? string.Empty : offset;
    }

    private async Task SaveStartDateOffsetInSettingsAsync(string offset)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, offset);
    }
}
