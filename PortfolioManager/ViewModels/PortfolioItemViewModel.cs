using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using Windows.Storage.Pickers;

namespace PortfolioManager.ViewModels;

public partial class PortfolioItemViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDataService _dataService;

    public ObservableCollection<Transaction> Transactions { get; set; } = [];

    [ObservableProperty]
    private Transaction? selected;

    public ICommand CancelCommand { get; set; }

    public ICommand SaveCommand { get; set; }

    public ICommand ImportCommand { get; set; }


    private PortfolioItem? portfolioItem;

    public PortfolioItem? PortfolioItem
    {
        get => portfolioItem;
        set
        {
            portfolioItem = value;

            if (portfolioItem != null)
            {
                var transactions = portfolioItem.Transactions;
                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
                }
            }
        }
    }

    public PortfolioItemViewModel(INavigationService navigationService, IDataService dataService)
    {
        _navigationService = navigationService;
        _dataService = dataService;

        CancelCommand = new RelayCommand(Cancel);

        SaveCommand = new AsyncRelayCommand(SaveItemAndReturnAsync);

        ImportCommand = new AsyncRelayCommand(ImportTransactionsAsync);
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is PortfolioItem item)
        {
            PortfolioItem = item;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void Cancel()
    {
        _navigationService.GoBack();
    }

    private async Task SaveItemAndReturnAsync()
    {
        await SaveItemAsync();

        _navigationService.GoBack();
    }

    private async Task SaveItemAsync()
    {
        if (PortfolioItem != null)
        {
            PortfolioItem.Transactions.Clear();

            foreach (var transaction in Transactions)
            {
                if (transaction.TransactionId == -1)
                {
                    PortfolioItem.Transactions.Add(transaction);
                }
            }

            if (PortfolioItem.Id == -1)
            {
                _ = await _dataService.AddItemAsync(PortfolioItem);
            }
            else
            {
                await _dataService.UpdateItemAsync(PortfolioItem);
            }
        }
    }

    public async Task ImportTransactionsAsync()
    {
        if (PortfolioItem != null)
        {
            FileOpenPicker picker = new();

            App.InitializePicker(picker);

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".csv");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ImportTransactions(file.Path);
            }
        }
    }

    private void ImportTransactions(string filename)
    {
        Transactions.Clear();

        if (!string.IsNullOrEmpty(filename))
        {
            using TextFieldParser csvParser = new(filename);

            csvParser.CommentTokens = ["#"];
            csvParser.SetDelimiters([",", "\t"]);
            csvParser.HasFieldsEnclosedInQuotes = true;

            // Skip the row with the column names
            csvParser.ReadLine();

            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                var fields = csvParser.ReadFields();

                if (fields != null && fields.Length == 6)
                {
                    var transaction = new Transaction
                    {
                        TransactionId = -1,
                        Date = Convert.ToDateTime(fields[0].ToString()),
                        Ticker = fields[1].ToString(),
                        Type = fields[2].ToString(),
                        Shares = Convert.ToInt64(fields[3].ToString()),
                        Price = Convert.ToDouble(fields[4].ToString())
                    };

                    Transactions.Add(transaction);
                }
            }
        }
    }
}
