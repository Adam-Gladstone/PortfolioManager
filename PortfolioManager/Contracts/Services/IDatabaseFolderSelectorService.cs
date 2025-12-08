namespace PortfolioManager.Contracts.Services;

public interface IDatabaseFolderSelectorService
{
    string Folder { get; }

    Task InitializeAsync();

    Task SetFolderAsync(string folder);

    Task SetRequestedFolderAsync();
}
