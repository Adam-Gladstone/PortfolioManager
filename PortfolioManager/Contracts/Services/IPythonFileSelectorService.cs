namespace PortfolioManager.Contracts.Services;

public interface IPythonFileSelectorService
{
    string Filename
    {
        get;
    }

    Task InitializeAsync();

    Task SetFilenameAsync(string filename);

    Task SetRequestedFilenameAsync();
}
