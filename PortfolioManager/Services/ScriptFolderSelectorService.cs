using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class ScriptFolderSelectorService : IScriptFolderSelectorService
{
    private const string SettingsKey = "ScriptFolder";

    public string Folder { get; set; } = string.Empty;

    private readonly ILocalSettingsService localSettingsService;

    public ScriptFolderSelectorService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
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
        var folder = await localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(folder) ? string.Empty : folder;
    }

    private async Task SaveFolderInSettingsAsync(string folder)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, folder);
    }
}
