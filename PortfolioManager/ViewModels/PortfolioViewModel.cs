using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Core.Services;
using PortfolioManager.Dialogs;

namespace PortfolioManager.ViewModels;

public partial class PortfolioViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IPortfolioItemService _portfolioItemService;

    public ObservableCollection<PortfolioItem> Source { get; } = [];

    public PortfolioViewModel(INavigationService navigationService, IPortfolioItemService portfolioItemService)
    {
        _navigationService = navigationService;
        _portfolioItemService = portfolioItemService;

        var settings = App.GetService<SettingsViewModel>();

        if (!string.IsNullOrEmpty(settings.DataFilename))
        {
            if (File.Exists(settings.DataFilename))
            {
                ((PortfolioItemService)_portfolioItemService).Filename = settings.DataFilename;
                _portfolioItemService.InitializeData();
            }
            else
            {
                throw new Exception($"The portfolio file {settings.DataFilename} does not exist.\nPlease check the settings.");
            }
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        var data = _portfolioItemService.GetData();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private async Task OnItemClick(PortfolioItem? clickedItem)
    {
        if (App.MainRoot == null) { return; }

        if (clickedItem != null)
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
                clickedItem.Benchmark = portfolioAnalysisParamsDialog.Benchmark;
                clickedItem.Start = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.StartDateOffset);
                clickedItem.End = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.EndDateOffset);
                clickedItem.RiskFreeRate = System.Convert.ToDouble(portfolioAnalysisParamsDialog.RiskFreeRate);

                _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.NavigateTo(typeof(PortfolioDetailViewModel).FullName!, clickedItem);
            }
        }
    }
}
