using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class DataSelectorService : IDataSelectorService
{
    private const string SettingsKey = "DataFilename";

    public string Filename { get; set; } = string.Empty;

    private readonly ILocalSettingsService _localSettingsService;

    public DataSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Filename = await LoadFilenameFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetFilenameAsync(string filename)
    {
        Filename = filename;

        await SetRequestedFilenameAsync();
        await SaveFilenameInSettingsAsync(Filename);
    }

    public async Task SetRequestedFilenameAsync()
    {
        // Nothing to do
        await Task.CompletedTask;
    }

    private async Task<string> LoadFilenameFromSettingsAsync()
    {
        var filename = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(filename) ? string.Empty : filename;
    }

    private async Task SaveFilenameInSettingsAsync(string filename)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, filename);
    }
}
