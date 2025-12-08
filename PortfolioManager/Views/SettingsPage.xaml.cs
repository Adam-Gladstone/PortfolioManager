using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using PortfolioManager.Contracts.Services;
using PortfolioManager.ViewModels;

namespace PortfolioManager.Views;


public sealed partial class SettingsPage : Page
{
#pragma warning disable CA1822 // Mark members as static
    public string Version
#pragma warning restore CA1822 // Mark members as static
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

#pragma warning disable CA1822 // Mark members as static
    public string WinAppSdkRuntimeDetails => App.WinAppSdkRuntimeDetails;
#pragma warning restore CA1822 // Mark members as static

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

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        var defaultRiskFreeRateService = App.GetService<IDefaultRiskFreeRateService>();
        var currentRiskFreeRate = defaultRiskFreeRateService.RiskFreeRate;

        if (currentRiskFreeRate != ViewModel.RiskFreeRate)
        {
            defaultRiskFreeRateService.SetRiskFreeRateAsync(ViewModel.RiskFreeRate);
        }

        var defaultBenchmarkIndexService = App.GetService<IDefaultBenchmarkIndexService>();
        var currentBenchmarkIndex = defaultBenchmarkIndexService.BenchmarkIndex;

        if (currentBenchmarkIndex != ViewModel.BenchmarkIndex)
        {
            defaultBenchmarkIndexService.SetBenchmarkIndexAsync(ViewModel.BenchmarkIndex);
        }

        base.OnNavigatedFrom(e);
    }


    private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
    {
        var themeSelectorService = App.GetService<IThemeSelectorService>();
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

        var defaultStartDateOffsetService = App.GetService<IDefaultStartDateOffsetService>();

        defaultStartDateOffset.SelectedIndex = 
            string.IsNullOrEmpty(defaultStartDateOffsetService.StartDateOffset) ? -1 :
            Convert.ToInt16(defaultStartDateOffsetService.StartDateOffset) - 1;
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

    private void StartDateOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedOffset = ((ComboBoxItem)defaultStartDateOffset.SelectedItem)?.Tag?.ToString();

        if (!string.IsNullOrEmpty(selectedOffset))
        {
            var defaultStartDateOffsetService = App.GetService<IDefaultStartDateOffsetService>();
            var currentOffset = defaultStartDateOffsetService.StartDateOffset;

            if (currentOffset != selectedOffset)
            {
                defaultStartDateOffsetService.SetStartDateOffsetAsync(selectedOffset);
            }
        }
    }
}
