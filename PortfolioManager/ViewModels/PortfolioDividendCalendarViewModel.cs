using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Views;

namespace PortfolioManager.ViewModels;


public partial class PortfolioDividendCalendarViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPythonService pythonService;

    private readonly ITickerAttributesService tickerAttributesService;

    [ObservableProperty]
    private bool isWorking;

    [ObservableProperty]
    private PortfolioItem? item;

    [ObservableProperty]
    private ObservableCollection<object>? calendarData;

    [ObservableProperty]
    private ObservableCollection<TableViewExtensions.Column>? columns;

    public PortfolioDividendCalendarViewModel(IPythonService pythonService, ITickerAttributesService tickerAttributesService)
    {
        IsWorking = false;

        this.pythonService = pythonService;

        var pythonLibraryService = App.GetService<IPythonFileSelectorService>();
        var scriptFolderSelectorService = App.GetService<IScriptFolderSelectorService>();
        var tickerAttributesSelectorService = App.GetService<ITickerAttributesSelectorService>();

        this.pythonService.InitializeAsync(pythonLibraryService.Filename, scriptFolderSelectorService.Folder);
        
        this.tickerAttributesService = tickerAttributesService;
        this.tickerAttributesService.Initialize(tickerAttributesSelectorService.Filename);
    }

    public void OnNavigatedTo(object parameter)
    {
        IsWorking = true;

        if (parameter is PortfolioItem item)
        {
            List<TickerAttribute> attributes = new List<TickerAttribute>()
            {
                new() { Name = "exDividendDate", Type = "Date", Selected = true },
                new() { Name = "lastDividendDate", Type = "Date", Selected = true },
            };

            item.TickerAttributes = attributes;

            if (pythonService.RetrieveDividendCalendarData(item))
            {
                var message = $"Retrieve Portfolio Data Exception\n{item.ExceptionMessage}";
                App.ReportException(new Exception(message));
            }
            else
            {
                InitializePortfolioCalendarData(item.DividendCalendarData);
            }
        }

        IsWorking = false;
    }

    public void OnNavigatedFrom()
    {
    }
    
    private void InitializePortfolioCalendarData(DataTable dataTable)
    {
        ObservableCollection<TableViewExtensions.Column> columns = [];
        ObservableCollection<object> items = [];

        var index = 0;

        foreach (DataColumn column in dataTable.Columns)
        {
            columns.Add(
                new TableViewExtensions.Column
                {
                    Header = column.ColumnName,
                    Binding = $"[{index++}]"
                });
        }

        foreach (DataRow row in dataTable.Rows)
        {
            items.Add(row.ItemArray);
        }

        Columns = columns;
        CalendarData = items;
    }
}
