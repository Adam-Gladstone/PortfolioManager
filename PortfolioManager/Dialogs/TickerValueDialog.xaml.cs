using Microsoft.UI.Xaml.Controls;

namespace PortfolioManager.Dialogs;
public sealed partial class TickerValueDialog : Page
{
    public string Ticker { get; set; }

    public string Value { get; set; }

    public TickerValueDialog()
    {
        InitializeComponent();

        Ticker = string.Empty;
        Value = string.Empty;
    }

    private void Ticker_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void Value_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void UpdateParent()
    {
        var parent = Parent as ContentDialog;
        if (parent != null)
        {
            parent.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(ticker.Text) &&
                                            !string.IsNullOrEmpty(value.Text);
        }
    }
}
