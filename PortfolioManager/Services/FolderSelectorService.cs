using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class FolderSelectorService : IFolderSelectorService
{
    private const string SettingsKey = "DatabaseFolder";

    public string Folder { get; set; } = string.Empty;

    private readonly ILocalSettingsService _localSettingsService;

    public FolderSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Folder = await LoadFolderFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetFolderAsync(string folder)
    {
        Folder = folder;

        await SetRequestedFolderAsync();
        await SaveFolderInSettingsAsync(Folder);
    }

    public async Task SetRequestedFolderAsync()
    {
        // Nothing to do
        await Task.CompletedTask;
    }

    private async Task<string> LoadFolderFromSettingsAsync()
    {
        var folder = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(folder) ? string.Empty : folder;
    }

    private async Task SaveFolderInSettingsAsync(string folder)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, folder);
    }
}
