namespace PortfolioManager.Contracts.Services;

public interface IFileSelectorService
{
    string Filename
    {
        get;
    }

    Task InitializeAsync();

    Task SetFilenameAsync(string filename);

    Task SetRequestedFilenameAsync();
}
