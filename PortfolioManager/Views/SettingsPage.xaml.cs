using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using PortfolioManager.Contracts.Services;
using PortfolioManager.ViewModels;

using Windows.System;

namespace PortfolioManager.Views;


public sealed partial class SettingsPage : Page
{
    public string Version
    {
        get
        {
            var versionString = string.Empty;

            System.Version? version;
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                version = assembly.GetName().Version;
                if (version != null)
                {
                    versionString = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                }
            }
            return versionString;
        }
    }

    public string WinAppSdkRuntimeDetails => App.WinAppSdkRuntimeDetails;

    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();

        InitializeComponent();

        Loaded += OnSettingsPageLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
    {
        var themeSelectorService = App.GetService<IThemeSelectorService>();
        if (themeSelectorService != null)
        {
            var currentTheme = themeSelectorService.Theme;
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    themeMode.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    themeMode.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                    themeMode.SelectedIndex = 2;
                    break;
            }
        }
    }

    private void ThemeMode_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();

        var themeSelectorService = App.GetService<IThemeSelectorService>();
        var currentTheme = themeSelectorService.Theme.ToString();

        if (currentTheme != selectedTheme)
        {
            _ = Enum.TryParse(selectedTheme, out ElementTheme param);

            themeSelectorService.SetThemeAsync(param);
        }
    }

    private async void BugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/Adam-Gladstone/PortfolioManager/issues"));
    }
}
