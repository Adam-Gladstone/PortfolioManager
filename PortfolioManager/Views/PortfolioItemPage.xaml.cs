using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Dialogs;
using PortfolioManager.ViewModels;

namespace PortfolioManager.Views;

public sealed partial class PortfolioItemPage : Page
{
    public PortfolioItemViewModel ViewModel
    {
        get;
    }

    public PortfolioItemPage()
    {
        ViewModel = App.GetService<PortfolioItemViewModel>();
        
        InitializeComponent();
    }

    private void TickerValueDeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            var item = ((FrameworkElement)sender).DataContext;
            if (item != null)
            {
                if (item is TickerValue tickerValue)
                {
                    ViewModel.TickerValues.Remove(tickerValue);
                }
            }
        }
    }

    private void PortfolioName_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateData();
    }

    private void PortfolioType_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateData();
    }

    private void UpdateData()
    {
        ButtonSave.IsEnabled = !string.IsNullOrEmpty(portfolioName.Text) &&
                                        !string.IsNullOrEmpty(portfolioType.Text);
    }

    private static ContentDialog CreateTickerValueDialog()
    {
        ContentDialog dialog = new()
        {
            XamlRoot = App.MainRoot?.XamlRoot,
            RequestedTheme = App.GetService<IThemeSelectorService>().Theme,
            Title = "Ticker Value",
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = false,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = new TickerValueDialog()
        };

        return dialog;
    }

    private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = CreateTickerValueDialog();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var tickerValueDialog = dialog.Content as TickerValueDialog;

                var tickerValue = new TickerValue()
                {
                    Ticker = tickerValueDialog?.Ticker,
                    Value = Convert.ToDouble(tickerValueDialog?.Value)
                };

                ViewModel.TickerValues.Add(tickerValue);
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }

    private async void TickerValues_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (App.MainRoot == null) { return; }

        if(ViewModel.Selected == null) { return; }

        try
        {
            var tickerValue = ViewModel.Selected;

            var dialog = CreateTickerValueDialog();

            var tickerValueDialog = dialog.Content as TickerValueDialog;
            if (tickerValueDialog != null)
            {
                tickerValueDialog.Ticker = tickerValue.Ticker;
                tickerValueDialog.Value = tickerValue.Value.ToString();
            }

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                tickerValue.Ticker = tickerValueDialog?.Ticker;
                tickerValue.Value = Convert.ToDouble(tickerValueDialog?.Value);

                ViewModel.TickerValues.Remove(tickerValue);
                ViewModel.TickerValues.Add(tickerValue);
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }

    private void ListViewTickerValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListViewTickerValues.SelectedItem is TickerValue tickerValue)
        {
            ViewModel.Selected = tickerValue;
        }
        else
        {
            ViewModel.Selected = null;
        }
    }
}
