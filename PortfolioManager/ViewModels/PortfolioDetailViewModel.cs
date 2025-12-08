using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using PortfolioManager.Contracts.Services;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;
using PortfolioManager.Views;

namespace PortfolioManager.ViewModels;


public partial class PortfolioDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPythonService pythonService;

    private readonly ITickerAttributesService tickerAttributesService;

    [ObservableProperty]
    private bool isWorking;

    [ObservableProperty]
    private PortfolioItem? item;

    [ObservableProperty]
    private ObservableCollection<object>? portfolioData;

    [ObservableProperty]
    private ObservableCollection<TableViewExtensions.Column>? columns;

    public PortfolioDetailViewModel(IPythonService pythonService, ITickerAttributesService tickerAttributesService)
    {
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
            var attributes = tickerAttributesService.TickerAttributes;
            var filtered = attributes.Where(attribute => attribute.Selected == true);

            // collection expression: A collection expression contains a sequence of elements between [ and ] brackets.
            // You use a spread element .. to inline collection values in a collection expression.
            item.TickerAttributes = [.. filtered];

            if (item.TickerAttributes.Count == 0)
            {
                var message = "The list of attributes to retrieve data for is empty.";
                App.ReportException(new Exception(message));
            }
            else
            {
                if (pythonService.RetrievePortfolioData(item))
                {
                    var message = $"Retrieve Portfolio Data Exception\n{item.ExceptionMessage}";
                    App.ReportException(new Exception(message));
                }
                else
                {
                    InitializePortfolioData(item.PortfolioData);
                }
            }
        }

        IsWorking = false;
    }

    public void OnNavigatedFrom()
    {
    }
    
    private void InitializePortfolioData(DataTable dataTable)
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
        PortfolioData = items;
    }
}

