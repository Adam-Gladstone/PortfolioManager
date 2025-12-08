using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using PortfolioManager.Contracts.Services;
using PortfolioManager.Helpers;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using Windows.System;

namespace PortfolioManager.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ElementTheme elementTheme;

    [ObservableProperty]
    private string versionDescription;

    [ObservableProperty]
    private string databaseFolder;

    [ObservableProperty]
    private string pythonLibrary;

    [ObservableProperty]
    private string tickerAttributes;

    [ObservableProperty]
    private string scriptFolder;

    [ObservableProperty]
    private string riskFreeRate;

    [ObservableProperty]
    private string benchmarkIndex;

    [ObservableProperty]
    private string startDateOffset;

    public ICommand SetDatabaseFolderCommand { get; set; }

    public ICommand SetScriptFolderCommand { get; set; }

    public ICommand SetPythonLibraryCommand { get; set; }

    public ICommand SwitchThemeCommand { get; set; }

    public ICommand BugRequestCommand { get; set; }

    public ICommand SetTickerAttributesCommand { get; set; }

    public SettingsViewModel()
    {
        // Binding
        var themeSelectorService = App.GetService<IThemeSelectorService>();
        ElementTheme = themeSelectorService.Theme;

        var databaseFolderSelectorService = App.GetService<IDatabaseFolderSelectorService>();
        DatabaseFolder = databaseFolderSelectorService.Folder;

        var scriptFolderSelectorService = App.GetService<IScriptFolderSelectorService>();
        ScriptFolder = scriptFolderSelectorService.Folder;

        var pythonLibrarySelectorService = App.GetService<IPythonFileSelectorService>();
        PythonLibrary = pythonLibrarySelectorService.Filename;

        var tickerAttributesSelectorService = App.GetService<ITickerAttributesSelectorService>();
        TickerAttributes = tickerAttributesSelectorService.Filename;

        var defaultRiskFreeRateService = App.GetService<IDefaultRiskFreeRateService>();
        RiskFreeRate = defaultRiskFreeRateService.RiskFreeRate;

        var defaultBenchmarkIndexService = App.GetService<IDefaultBenchmarkIndexService>();
        BenchmarkIndex = defaultBenchmarkIndexService.BenchmarkIndex;

        var defaultStartDateOffsetService = App.GetService<IDefaultStartDateOffsetService>();
        StartDateOffset = defaultStartDateOffsetService.StartDateOffset;

        VersionDescription = GetVersionDescription();

        // Command routing
        SwitchThemeCommand = new RelayCommand<ElementTheme>(SetThemeAsync);

        SetDatabaseFolderCommand = new RelayCommand(SetDatabaseFolderAsync);

        SetPythonLibraryCommand = new RelayCommand(SetPythonLibraryAsync);

        SetScriptFolderCommand = new RelayCommand(SetScriptFolderAsync);

        BugRequestCommand = new RelayCommand(BugRequestAsync);

        SetTickerAttributesCommand = new RelayCommand(SetTickerAttributesAsync);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    private async void SetPythonLibraryAsync()
    {
        try
        {
            FileOpenPicker picker = new();

            App.InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".dll");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var pythonLibrarySelectorService = App.GetService<IPythonFileSelectorService>();

                await pythonLibrarySelectorService.SetFilenameAsync(file.Path);

                PythonLibrary = file.Path;
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }

    private async void SetThemeAsync(ElementTheme theme)
    {
        if (ElementTheme != theme)
        {
            var themeSelectorService = App.GetService<IThemeSelectorService>();

            await themeSelectorService.SetThemeAsync(theme);
            
            ElementTheme = theme;
        }
    }

    private async void SetDatabaseFolderAsync()
    {
        try
        {
            FolderPicker picker = new();

            App.InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.CommitButtonText = "Select this folder";

            var folderItem = await picker.PickSingleFolderAsync();
            if (folderItem != null)
            {
                var folderSelectorService = App.GetService<IDatabaseFolderSelectorService>();
                await folderSelectorService.SetFolderAsync(folderItem.Path);

                DatabaseFolder = folderItem.Path;
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }

    private async void SetScriptFolderAsync()
    {
        try
        {
            FolderPicker picker = new();

            App.InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.CommitButtonText = "Select this folder";

            var folderItem = await picker.PickSingleFolderAsync();
            if (folderItem != null)
            {
                var folderSelectorService = App.GetService<IScriptFolderSelectorService>();
                await folderSelectorService.SetFolderAsync(folderItem.Path);

                ScriptFolder = folderItem.Path;
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }

    private async void BugRequestAsync()
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/Adam-Gladstone/PortfolioManager/issues"));
    }

    private async void SetTickerAttributesAsync()
    {
        try
        {
            FileOpenPicker picker = new();

            App.InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".csv");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var tickerAttributesSelectorService = App.GetService<ITickerAttributesSelectorService>();

                await tickerAttributesSelectorService.SetFilenameAsync(file.Path);

                TickerAttributes = file.Path;
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }
}
