using Microsoft.UI.Xaml.Controls;
using PortfolioManager.ViewModels;

namespace PortfolioManager.Views;


public sealed partial class PortfolioDetailPage : Page
{
    public PortfolioDetailViewModel ViewModel { get; set; }


    public PortfolioDetailPage()
    {
        ViewModel = App.GetService<PortfolioDetailViewModel>();

        InitializeComponent();
    }
}
