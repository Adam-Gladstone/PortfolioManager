using CommunityToolkit.Mvvm.ComponentModel;

namespace PortfolioManager.Core.Models;

public partial class Transaction : ObservableObject
{
    public int PortfolioId { get; set; }

    public int TransactionId { get; set; }

    [ObservableProperty]
    private DateTime date;

    [ObservableProperty]
    private string ticker;

    [ObservableProperty]
    private string type;

    [ObservableProperty]
    private long shares;

    [ObservableProperty]
    private double price;

    public double Cost => Shares * Price;

    public string CostString => Cost.ToString("F3");
}
