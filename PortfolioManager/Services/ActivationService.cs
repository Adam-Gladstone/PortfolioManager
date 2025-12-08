using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PortfolioManager.Activation;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Views;

namespace PortfolioManager.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IDatabaseFolderSelectorService _databaseFolderSelectorService;
    private readonly IScriptFolderSelectorService _scriptFolderSelectorService;
    private readonly IPythonFileSelectorService _pythonFileSelectorService;
    private readonly IDefaultRiskFreeRateService _defaultRiskFreeRateService;
    private readonly IDefaultBenchmarkIndexService _defaultBenchmarkIndexService;
    private readonly IDefaultStartDateOffsetService _defaultStartDateOffsetService;
    private readonly ITickerAttributesSelectorService _tickerAttributesSelectorService;

    private UIElement? _shell = null;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers, 
        IThemeSelectorService themeSelectorService,
        IDatabaseFolderSelectorService databaseFolderSelectorService,
        IScriptFolderSelectorService scriptFolderSelectorService,
        IPythonFileSelectorService pythonFileSelectorService,
        IDefaultRiskFreeRateService defaultRiskFreeRateService,
        IDefaultBenchmarkIndexService defaultBenchmarkIndexService,
        IDefaultStartDateOffsetService defaultStartDateOffsetService,
        ITickerAttributesSelectorService tickerAttributesSelectorService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _databaseFolderSelectorService = databaseFolderSelectorService;
        _scriptFolderSelectorService = scriptFolderSelectorService;
        _pythonFileSelectorService = pythonFileSelectorService;
        _defaultRiskFreeRateService = defaultRiskFreeRateService;
        _defaultBenchmarkIndexService = defaultBenchmarkIndexService;
        _defaultStartDateOffsetService = defaultStartDateOffsetService;
        _tickerAttributesSelectorService = tickerAttributesSelectorService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);

        await _databaseFolderSelectorService.InitializeAsync().ConfigureAwait(false);

        await _scriptFolderSelectorService.InitializeAsync().ConfigureAwait(false);

        await _pythonFileSelectorService.InitializeAsync().ConfigureAwait(false);

        await _defaultRiskFreeRateService.InitializeAsync().ConfigureAwait(false);

        await _defaultBenchmarkIndexService.InitializeAsync().ConfigureAwait(false);

        await _defaultStartDateOffsetService.InitializeAsync().ConfigureAwait(false);

        await _tickerAttributesSelectorService.InitializeAsync().ConfigureAwait(false);

        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();

        await Task.CompletedTask;
    }
}
