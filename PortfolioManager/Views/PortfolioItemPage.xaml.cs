using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Dialogs;
using PortfolioManager.ViewModels;

using Transaction = PortfolioManager.Core.Models.Transaction;

namespace PortfolioManager.Views;

public sealed partial class PortfolioItemPage : Page
{
    public PortfolioItemViewModel ViewModel { get; set; }

    public PortfolioItemPage()
    {
        ViewModel = App.GetService<PortfolioItemViewModel>();
        
        InitializeComponent();

        UpdateData();
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

    private static ContentDialog CreateTransactionDialog()
    {
        ContentDialog dialog = new()
        {
            XamlRoot = App.MainRoot?.XamlRoot,
            RequestedTheme = App.GetService<IThemeSelectorService>().Theme,
            Title = "Add Transaction",
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = false,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = new TransactionDialog()
        };

        return dialog;
    }

    private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = CreateTransactionDialog();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var transactionDialog = dialog.Content as TransactionDialog;

                if (transactionDialog != null)
                {
                    var transaction = new Transaction()
                    {
                        TransactionId = -1,
                        Ticker = transactionDialog.Ticker,
                        Type = transactionDialog.TransactionType,
                        Date = DateTime.Now,
                        Shares = Convert.ToInt64(transactionDialog.Shares),
                        Price = Convert.ToDouble(transactionDialog.Price)
                    };

                    ViewModel.Transactions.Add(transaction);
                }
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }

    private async void ListViewTransactions_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (App.MainRoot == null) { return; }

        if(ViewModel.Selected == null) { return; }

        try
        {
            var transaction = ViewModel.Selected;
            if (transaction.TransactionId == -1)
            {
                var dialog = CreateTransactionDialog();

                var transactionDialog = dialog.Content as TransactionDialog;

                if (transactionDialog != null)
                {
                    transactionDialog.Ticker = transaction.Ticker;
                    transactionDialog.TransactionType = transaction.Type;
                    transactionDialog.Shares = transaction.Shares.ToString();
                    transactionDialog.Price = transaction.Price.ToString();

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        transaction.Ticker = transactionDialog.Ticker;
                        transaction.Type = transactionDialog.TransactionType;
                        transaction.Date = DateTime.Now;
                        transaction.Shares = Convert.ToInt64(transactionDialog.Shares);
                        transaction.Price = Convert.ToDouble(transactionDialog.Price);

                        ViewModel.Transactions.Remove(transaction);
                        ViewModel.Transactions.Add(transaction);
                    }
                }
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }

    private void ListViewTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListViewTransactions.SelectedItem is Transaction transaction)
        {
            ViewModel.Selected = transaction;
        }
        else
        {
            ViewModel.Selected = null;
        }
    }
}
