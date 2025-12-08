using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using WinUI.TableView;

namespace PortfolioManager.Views;

public static class TableViewExtensions
{
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.RegisterAttached(
            "Columns",
            typeof(ObservableCollection<Column>),
            typeof(TableViewExtensions),
            new PropertyMetadata(
                new ObservableCollection<Column>(),
                OnColumnsPropertyChanged));

    public static ObservableCollection<Column> GetColumns(DependencyObject obj)
    {
        return (ObservableCollection<Column>)obj.GetValue(ColumnsProperty);
    }

    public static void SetColumns(DependencyObject obj, ObservableCollection<Column> value)
    {
        obj.SetValue(ColumnsProperty, value);
    }

    private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TableView tableView ||
            e.NewValue is not ObservableCollection<Column> columns)
        {
            return;
        }

        tableView.Columns.Clear();

        foreach (var column in columns)
        {
            Binding binding = new()
            {
                Path = new PropertyPath(column.Binding)
            };

            tableView.Columns.Add(
                new TableViewTextColumn
                {
                    Header = column.Header ?? string.Empty,
                    Binding = binding,
                });
        }
    }

    public class Column
    {
        public string? Header
        {
            get; set;
        }

        public string? Binding
        {
            get; set;
        }
    }
}
