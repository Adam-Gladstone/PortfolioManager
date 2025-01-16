namespace PortfolioManager.Contracts.Services;

public interface IDataSelectorService
{
    string Filename
    {
        get;
    }

    Task InitializeAsync();

    Task SetFilenameAsync(string filename);

    Task SetRequestedFilenameAsync();
}
