using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using PortfolioManager.Contracts.Services;
using PortfolioManager.ViewModels;
using PortfolioManager.Services.Theming;

namespace PortfolioManager.Views;

public sealed partial class PortfolioAnalysisPage : Page
{
    public PortfolioAnalysisViewModel ViewModel { get; }

    public PortfolioAnalysisPage()
    {
        ViewModel = App.GetService<PortfolioAnalysisViewModel>();
        
        InitializeComponent();

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme(ActualTheme);
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ActualThemeChanged += Page_ActualThemeChanged;

        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        ActualThemeChanged -= Page_ActualThemeChanged;

        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            var navigationService = App.GetService<INavigationService>();

            if (ViewModel.Item != null)
            {
                navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        ApplyTheme(sender.ActualTheme);
    }

    private void ApplyTheme(ElementTheme theme)
    {
        ViewModel.HistoricPerformanceAssetsModel.ApplyTheme(theme);
        ViewModel.RiskRewardModel.ApplyTheme(theme);
        ViewModel.SharpeRatioModel.ApplyTheme(theme);
        ViewModel.ComparativeReturnsModel.ApplyTheme(theme);
        ViewModel.ComparativeRiskRewardModel.ApplyTheme(theme);
    }
}
