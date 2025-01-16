using Microsoft.UI.Xaml.Controls;

using PortfolioManager.ViewModels;

namespace PortfolioManager.Views;

public sealed partial class PortfolioPage : Page
{
    public PortfolioViewModel ViewModel
    {
        get;
    }

    public PortfolioPage()
    {
        ViewModel = App.GetService<PortfolioViewModel>();
        InitializeComponent();
    }
}
