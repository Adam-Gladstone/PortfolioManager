using PortfolioManager.Contracts.Services;

namespace PortfolioManager.Services;

public class PythonFileSelectorService : IPythonFileSelectorService
{
    private const string SettingsKey = "PythonLibrary";

    public string Filename { get; set; } = string.Empty;

    private readonly ILocalSettingsService localSettingsService;

    public PythonFileSelectorService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
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
        var filename = await localSettingsService.ReadSettingAsync<string>(SettingsKey);

        return string.IsNullOrEmpty(filename) ? string.Empty : filename;
    }

    private async Task SaveFilenameInSettingsAsync(string filename)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, filename);
    }
}
