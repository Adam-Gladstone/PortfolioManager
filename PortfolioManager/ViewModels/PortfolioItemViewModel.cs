using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

namespace PortfolioManager.ViewModels;

public partial class PortfolioItemViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDataService _dataService;

    public ObservableCollection<TickerValue> TickerValues { get; set; } = [];

    [ObservableProperty]
    private TickerValue? selected;

    public ICommand CancelCommand
    {
        get; set;
    }

    private PortfolioItem? portfolioItem;

    public PortfolioItem? PortfolioItem
    {
        get => portfolioItem;
        set
        {
            portfolioItem = value;

            if (portfolioItem != null)
            {
                var tickerValues = portfolioItem.TickerValues;
                var keys = tickerValues.Keys;
                foreach (var key in keys)
                {
                    TickerValues.Add(new TickerValue { Ticker = key, Value = tickerValues[key] });
                }
            }
        }
    }

    public PortfolioItemViewModel(INavigationService navigationService, IDataService dataService)
    {
        _navigationService = navigationService;
        _dataService = dataService;

        CancelCommand = new RelayCommand(Cancel);
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

    public async Task SaveItemAndReturnAsync()
    {
        await SaveItemAsync();

        _navigationService.GoBack();
    }

    private async Task SaveItemAsync()
    {
        if (PortfolioItem != null)
        {
            foreach (var tickerValue in TickerValues)
            {
                PortfolioItem.TickerValues[tickerValue.Ticker] = tickerValue.Value;
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
}
