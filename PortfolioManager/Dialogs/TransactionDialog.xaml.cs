using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace PortfolioManager.Dialogs;


public sealed partial class TransactionDialog : Page
{
    public string Ticker { get; set; }

    public string TransactionType { get; set; }

    public string Shares { get; set; }

    public string Price { get; set; }

    public TransactionDialog()
    {
        InitializeComponent();
       
        Ticker = string.Empty;
        TransactionType = string.Empty;
        Shares = string.Empty;
        Price = string.Empty;

        UpdateParent();
    }

    private void Ticker_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void Shares_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void Price_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void TransactionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateParent();
    }

    private void UpdateParent()
    {
        var parent = Parent as ContentDialog;
        if (parent != null)
        {
            parent.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(ticker.Text) &&
                                            !string.IsNullOrEmpty(TransactionType) && 
                                            !string.IsNullOrEmpty(shares.Text) &&
                                            !string.IsNullOrEmpty(price.Text);
        }
    }
}
