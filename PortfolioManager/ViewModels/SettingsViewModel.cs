using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using PortfolioManager.Contracts.Services;
using PortfolioManager.Helpers;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;

namespace PortfolioManager.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    private readonly IFolderSelectorService _folderSelectorService;

    private readonly IFileSelectorService _pythonLibrarySelectorService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string _databaseFolder;

    [ObservableProperty]
    private string _pythonLibrary;

    public RelayCommand SetFolderCommand
    {
        get; set;
    }

    public RelayCommand SetPythonLibraryCommand
    {
        get; set;
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService, 
        IFileSelectorService fileSelectorService,
        IFolderSelectorService folderSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _pythonLibrarySelectorService = fileSelectorService;
        _folderSelectorService = folderSelectorService;

        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        SetFolderCommand = new RelayCommand(SetFolder);

        SetPythonLibraryCommand = new RelayCommand(SetPythonLibrary);

        _databaseFolder = _folderSelectorService.Folder;
        _pythonLibrary = _pythonLibrarySelectorService.Filename;
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

    private async void SetPythonLibrary()
    {
        try
        {
            FileOpenPicker picker = new();

            InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".dll");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                PythonLibrary = file.Path;

                await _pythonLibrarySelectorService.SetFilenameAsync(file.Path);
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }

    private async void SetFolder()
    {
        try
        {
            FolderPicker picker = new();

            InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.CommitButtonText = "Select this folder";

            var folderItem = await picker.PickSingleFolderAsync();
            if (folderItem != null)
            {
                DatabaseFolder = folderItem.Path;

                var folderSelectorService = App.GetService<IFolderSelectorService>();
                await folderSelectorService.SetFolderAsync(folderItem.Path);
            }
        }
        catch (Exception ex)
        {
            App.ReportException(ex);
        }
    }

    private static void InitializePicker(object picker)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
    }
}
