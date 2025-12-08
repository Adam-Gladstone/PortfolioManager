using System.Windows.Input;
using CodeManager.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OxyPlot;
using OxyPlot.Series;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Core.Services;
using PortfolioManager.Dialogs;
using PortfolioManager.Helpers;
using PortfolioManager.Services;
using Windows.Storage;

namespace PortfolioManager.ViewModels;

public partial class PortfolioViewModel : MasterDetailViewModel<PortfolioItem>, INavigationAware
{
    private readonly INavigationService navigationService;
    private readonly IDataService dataService;
    private readonly IPythonService pythonService;

    [ObservableProperty]
    private bool isWorking;

    [ObservableProperty]
    private PlotModel? portfolioWeightsModel;

    public ICommand AddCommand { get; set; }

    public ICommand EditCommand { get; set; }

    public ICommand DeleteCommand { get; set; }

    public ICommand RunCommand { get; set; }

    public ICommand PortfolioDataCommand { get; set; }

    public ICommand PortfolioDividendCalendarCommand { get; set; }


    public PortfolioViewModel(INavigationService navigationService, IDataService dataService, IPythonService pythonService)
    {
        IsWorking = false;

        this.navigationService = navigationService;
        this.dataService = dataService;
        this.pythonService = pythonService;

        AddCommand = new RelayCommand(Add);

        EditCommand = new RelayCommand(Edit);

        DeleteCommand = new AsyncRelayCommand(DeleteAsync);

        RunCommand = new AsyncRelayCommand(RunAsync);

        PortfolioDataCommand = new RelayCommand(RetrievePortfolioData);

        PortfolioDividendCalendarCommand = new RelayCommand(RetrieveDividendCalendarData);

        //
        // Database service
        //
        var databaseLocationService = App.GetService<IDatabaseFolderSelectorService>();
        var sqliteDataService = (SqliteDataService)this.dataService;

        if (Directory.Exists(databaseLocationService.Folder))
        {
            sqliteDataService.DbPath = Path.Combine(databaseLocationService.Folder, SqliteDataService.DbFilename);
        }
        else
        {
            if (RuntimeHelper.IsMSIX)
            {
                sqliteDataService.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, SqliteDataService.DbFilename);
            }
            else
            {
                sqliteDataService.DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SqliteDataService.DbFilename);
            }
        }

        this.dataService.InitializeDataAsync();

        //
        // Python service and script locations
        //
        var pythonLibraryService = App.GetService<IPythonFileSelectorService>();
        var scriptFolderSelectorService = App.GetService<IScriptFolderSelectorService>();

        if (!string.IsNullOrWhiteSpace(pythonLibraryService.Filename) &&
            !string.IsNullOrWhiteSpace(scriptFolderSelectorService.Folder))
        {
            this.pythonService.InitializeAsync(pythonLibraryService.Filename, scriptFolderSelectorService.Folder);
        }

        Items.CollectionChanged += Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
    }

    public void OnNavigatedTo(object? parameter)
    {
        RefreshPortfolio();
    }

    public void OnNavigatedFrom()
    {
    }

    public override bool ApplyFilter(PortfolioItem item, string filter)
    {
        return item.ApplyFilter(filter);
    }

    private void RefreshPortfolio()
    {
        IsWorking = true;

        Items.Clear();

        var portfolios = GetDataAsync().Result;

        if (portfolios.Count > 0)
        {
            portfolios.OrderBy(c => c.Name).ToList().ForEach(c => Items.Add(c));
        }

        if (Items.Count > 0)
        {
            Current = Items[0];
        }

        IsWorking = false;
    }

    private void Add()
    {
        try
        {
            AddPortfolioItem();
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private void Edit()
    {
        try
        {
            EditPortfolioItem();
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private async Task DeleteAsync()
    {
        try
        {
            var message = $"Are you sure you want to delete the portfolio ({Current?.Name})?";

            var result = await App.MainRoot.ConfirmationDialogAsync("Delete Portfolio", message, "Delete", "Cancel");
            if (result)
            {
                DeletePortfolioItem();
            }
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private async Task RunAsync()
    {
        IsWorking = true;

        try
        {
            await RunPortfolioAnalysisAsync();
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }

        IsWorking = false;
    }

    private void RetrievePortfolioData()
    {
        navigationService.NavigateTo(typeof(PortfolioDetailViewModel).FullName!, Current);
    }

    private void RetrieveDividendCalendarData()
    {
        navigationService.NavigateTo(typeof(PortfolioDividendCalendarViewModel).FullName!, Current);
    }

    public async Task<List<PortfolioItem>> GetDataAsync()
    {
        return (List<PortfolioItem>)await dataService.GetItemsAsync();
    }

    private void AddPortfolioItem()
    {
        var portfolio = new PortfolioItem
        {
            Id = -1,
        };

        navigationService.NavigateTo(typeof(PortfolioItemViewModel).FullName!, portfolio);
    }

    // Called via ListView_DoubleTapped
    public void EditPortfolioItem()
    {
        if (Current != null)
        {
            navigationService.NavigateTo(typeof(PortfolioItemViewModel).FullName!, Current);
        }
    }

    private void DeletePortfolioItem()
    {
        if (Current != null)
        {
            var id = dataService.DeleteItemAsync(Current).Result;
            if (id == false)
            {
                throw new Exception($"Unable to delete the item \'{Current.Name}\'.");
            }

            DeleteItem(Current);
            
            RefreshPortfolio();
        }
    }

    private async Task RunPortfolioAnalysisAsync()
    {
        if (Current != null)
        {
            PortfolioAnalysisParamsDialog portfolioAnalysisParamsDialog = new();
            ContentDialog dialog = new()
            {
                XamlRoot = App.MainRoot?.XamlRoot,
                RequestedTheme = App.GetService<IThemeSelectorService>().Theme,
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
                Current.Benchmark = portfolioAnalysisParamsDialog.Benchmark;
                Current.Start = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.StartDateOffset);
                Current.End = PortfolioAnalysisParamsDialog.ConvertFromDateTimeOffset(portfolioAnalysisParamsDialog.EndDateOffset);
                Current.RiskFreeRate = System.Convert.ToDouble(portfolioAnalysisParamsDialog.RiskFreeRate);

                navigationService.SetListDataItemForNextConnectedAnimation(Current);
                navigationService.NavigateTo(typeof(PortfolioAnalysisViewModel).FullName!, Current);
            }
        }
    }

    public void InitializePortfolioWeights()
    {
        PortfolioWeightsModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        var holdings = Current?.Holdings;
        if (holdings != null)
        {
            dynamic seriesP1 = new PieSeries
            {
                StrokeThickness = 2.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            foreach (var holding in holdings)
            {
                var item = new PieSlice(holding.Ticker, holding.Percent)
                {
                    IsExploded = true
                };

                seriesP1.Slices.Add(item);
            }

            PortfolioWeightsModel.Series.Add(seriesP1);
        }
    }
}
