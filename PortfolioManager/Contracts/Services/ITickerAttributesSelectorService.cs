namespace PortfolioManager.Contracts.Services;

public interface ITickerAttributesSelectorService
{
    string Filename
    {
        get;
    }

    Task InitializeAsync();

    Task SetFilenameAsync(string filename);

    Task SetRequestedFilenameAsync();
}
