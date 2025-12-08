namespace PortfolioManager.Contracts.Services;

public interface IScriptFolderSelectorService
{
    string Folder { get; }

    Task InitializeAsync();

    Task SetFolderAsync(string folder);

    Task SetRequestedFolderAsync();
}
