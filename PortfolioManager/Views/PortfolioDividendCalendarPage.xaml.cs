using Microsoft.UI.Xaml.Controls;
using PortfolioManager.ViewModels;

namespace PortfolioManager.Views;


public sealed partial class PortfolioDividendCalendarPage : Page
{
    public PortfolioDividendCalendarViewModel ViewModel { get; set; }


    public PortfolioDividendCalendarPage()
    {
        ViewModel = App.GetService<PortfolioDividendCalendarViewModel>();

        InitializeComponent();
    }
}
