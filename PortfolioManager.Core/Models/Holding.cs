using CommunityToolkit.Mvvm.ComponentModel;

namespace PortfolioManager.Core.Models;
public partial class Holding : ObservableObject
{
    [ObservableProperty]
    private string ticker;

    [ObservableProperty]
    private long totalShares;

    [ObservableProperty]
    private double totalCost;

    [ObservableProperty]
    private double averageCost;

    [ObservableProperty]
    private double percent;

    public override string ToString() => $"{Ticker}";
}
