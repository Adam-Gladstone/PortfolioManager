using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

using PortfolioManager.ViewModels;
using Microsoft.UI.Xaml.Navigation;

namespace PortfolioManager.Views;

public enum Group
{
    None,
    Type,
    Name,
}

public partial class GroupedList : List<object>
{
    public GroupedList(IEnumerable<object> items) : base(items)
    {
    }
    public object? Key
    {
        get; set;
    }

    public override string ToString()
    {
        return $"Group {Key?.ToString()}";
    }
}


public sealed partial class PortfolioPage : Page
{
    private Group mGroup = Group.Name;

    public PortfolioViewModel ViewModel
    {
        get;
    }

    public PortfolioPage()
    {
        ViewModel = App.GetService<PortfolioViewModel>();

        InitializeComponent();

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme(ActualTheme);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ActualThemeChanged += Page_ActualThemeChanged;

        base.OnNavigatedTo(e);

        UpdateGroupSource(ViewModel.Filter);

        ListView.SelectedIndex = 0;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ActualThemeChanged -= Page_ActualThemeChanged;
        base.OnNavigatedFrom(e);
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        ApplyTheme(sender.ActualTheme);
    }

    #region Control Handlers

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        UpdateGroupSource(args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            UpdateGroupSource(sender.Text);
        }
    }

    private void ButtonAdd_Click(object? sender, RoutedEventArgs? e)
    {
        AddPortfolioItem();
    }

    private void ButtonEdit_Click(object? sender, RoutedEventArgs? e)
    {
        EditPortfolioItem();
    }

    private void ButtonDelete_Click(object? sender, RoutedEventArgs? e)
    {
        DeletePortfolioItem();

        ListView.SelectedIndex = 0;
    }

    private void ButtonRun_Click(object? sender, RoutedEventArgs? e)
    {
        RunPortfolioAnalysis();
    }

    private void ListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (ListView.SelectedItem is Core.Models.PortfolioItem)
        {
            ViewModel.EditPortfolioItem();
        }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListView.SelectedItem is Core.Models.PortfolioItem portfolio)
        {
            ViewModel.Selected = portfolio;
        }
        else
        {
            ViewModel.Selected = null;
        }
    }

    #endregion

    #region Implementation

    private async void UpdateGroupSource(string? filter)
    {
        switch (mGroup)
        {
            case Group.Name:
                PortfolioCVS.Source = await GetNameDataAsync(filter);
                break;
        }
    }

    private async Task<ObservableCollection<GroupedList>> GetNameDataAsync(string? filter)
    {
        var portfolios = await ViewModel.GetDataAsync();

        IEnumerable<GroupedList> groupedList;

        if (string.IsNullOrEmpty(filter))
        {
            groupedList = from portfolio in portfolios
                          group portfolio by portfolio.Name[..1] into g
                          orderby g.Key
                          select new GroupedList(g) { Key = g.Key };

        }
        else
        {
            groupedList = from portfolio in portfolios
                          where portfolio.Name.Contains(filter)
                          group portfolio by portfolio.Name[..1] into g
                          orderby g.Key
                          select new GroupedList(g) { Key = g.Key };
        }

        return new ObservableCollection<GroupedList>(groupedList);
    }

    private void AddPortfolioItem()
    {
        try
        {
            ViewModel.AddPortfolioItem();

            SearchBox.Text = string.Empty;
            ViewModel.Filter = "";

            UpdateGroupSource(ViewModel.Filter);
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private void EditPortfolioItem()
    {
        try
        {
            ViewModel.EditPortfolioItem();

            SearchBox.Text = string.Empty;
            ViewModel.Filter = "";

            UpdateGroupSource(ViewModel.Filter);
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private void RunPortfolioAnalysis()
    {
        try
        {
            ViewModel.RunPortfolioAnalysis();

            ApplyTheme(ActualTheme);
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private void DeletePortfolioItem()
    {
        try
        {
            ViewModel.DeletePortfolioItem();

            SearchBox.Text = string.Empty;
            ViewModel.Filter = "";

            UpdateGroupSource(ViewModel.Filter);
        }
        catch (Exception e)
        {
            App.ReportException(e);
        }
    }

    private void ApplyTheme(ElementTheme theme)
    {
    }

    #endregion

}
