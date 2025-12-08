using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PortfolioManager.ViewModels;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using PortfolioManager.Services.Theming;

namespace PortfolioManager.Views;


public sealed partial class PortfolioPage : Page
{
    public PortfolioViewModel ViewModel { get; set; }

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
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        ActualThemeChanged += Page_ActualThemeChanged;

        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        ActualThemeChanged -= Page_ActualThemeChanged;

        base.OnNavigatedFrom(e);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Current" && ViewModel.HasCurrent)
        {
            ListView.ScrollIntoView(ViewModel.Current);
        }
    }

    private void ListViewItem_PointerEntered(object? sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
        }
    }

    private void ListViewItem_PointerExited(object? sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && ViewModel.HasCurrent)
        {
            ViewModel.InitializePortfolioWeights();

            ApplyTheme(ActualTheme);
        }
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.Filter = args.QueryText;
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.Filter = sender.Text;
        }
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        ApplyTheme(sender.ActualTheme);
    }

    #region Control Handlers

    private void ListView_DoubleTapped(object? sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel?.EditPortfolioItem();
    }

    #endregion

    #region Implementation

    private void ApplyTheme(ElementTheme theme)
    {
        ViewModel.PortfolioWeightsModel?.ApplyTheme(theme);
    }

    #endregion
}
