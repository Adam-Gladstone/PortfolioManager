using CommunityToolkit.Mvvm.ComponentModel;

namespace PortfolioManager.Core.Models;
public partial class TickerAttribute : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string type;

    [ObservableProperty]
    private bool selected;
}
