using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Core.Services;
using PortfolioManager.Dialogs;
using Windows.Storage;

namespace PortfolioManager.ViewModels;

public partial class PortfolioViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDataService _dataService;

    [ObservableProperty]
    private string filter = "";

    [ObservableProperty]
    private PortfolioItem? selected;

    public PortfolioViewModel(INavigationService navigationService, IDataService dataService)
    {
        _navigationService = navigationService;
        _dataService = dataService;

        var settings = App.GetService<SettingsViewModel>();

        if (!string.IsNullOrEmpty(settings.DatabaseFolder))
        {
            var sqliteDataService = (SqliteDataService)_dataService;

            if (Directory.Exists(settings.DatabaseFolder))
            {
                sqliteDataService.DbPath = Path.Combine(settings.DatabaseFolder, sqliteDataService.DbFilename);
            }
            else
            {
                sqliteDataService.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteDataService.DbFilename);
            }

            _dataService.InitializeDataAsync();
        }
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task<List<PortfolioItem>> GetDataAsync()
    {
        return (List<PortfolioItem>)await _dataService.GetItemsAsync();
    }

    public void AddPortfolioItem()
    {
        var portfolio = new PortfolioItem
        {
            Id = -1,
            SymbolCode = 57808,   // &#xE1D0
            SymbolName = "Calculator"
        };

        _navigationService.NavigateTo(typeof(PortfolioItemViewModel).FullName!, portfolio);
    }

    public void EditPortfolioItem()
    {
        if (Selected != null)
        {
            _navigationService.NavigateTo(typeof(PortfolioItemViewModel).FullName!, Selected);
        }
    }

    public void DeletePortfolioItem()
    {
        try
        {
            if (Selected != null)
            {
                var id = _dataService.DeleteItemAsync(Selected).Result;
                if (id != false)
                {
                    Debug.WriteLine($"Deleted item \'{Selected.Name}\' with id:{id}");
                }
                else
                {
                    throw new Exception($"Unable to delete the item \'{Selected.Name}\'.");
                }
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }

    public async void RunPortfolioAnalysis()
    {
        if (App.MainRoot == null) { return; }

        try
        {
            if (Selected != null)
            {
                var themeSelectorService = App.GetService<IThemeSelectorService>();

                PortfolioAnalysisParamsDialog portfolioAnalysisParamsDialog = new();
                ContentDialog dialog = new()
                {
                    XamlRoot = App.MainRoot.XamlRoot,
                    RequestedTheme = themeSelectorService.Theme,
                    Title = "Portfolio Analysis Parameters",
                    PrimaryButtonText = "OK",
                    IsPrimaryButtonEnabled = false,
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = portfolioAnalysisParamsDialog
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    Selected.Benchmark = portfolioAnalysisParamsDialog.Benchmark;
                    Selected.Start = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.StartDateOffset);
                    Selected.End = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.EndDateOffset);
                    Selected.RiskFreeRate = System.Convert.ToDouble(portfolioAnalysisParamsDialog.RiskFreeRate);

                    _navigationService.SetListDataItemForNextConnectedAnimation(Selected);
                    _navigationService.NavigateTo(typeof(PortfolioDetailViewModel).FullName!, Selected);
                }
            }
        }
        catch (Exception exc)
        {
            App.ReportException(exc);
        }
    }
}
