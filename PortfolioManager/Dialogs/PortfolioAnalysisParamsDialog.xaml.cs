using Microsoft.UI.Xaml.Controls;

namespace PortfolioManager.Dialogs;
public sealed partial class PortfolioAnalysisParamsDialog : Page
{
    public string Benchmark { get; set; }
    public DateTimeOffset StartDateOffset { get; set; }
    public DateTimeOffset EndDateOffset { get; set; }
    public string RiskFreeRate { get; set; }

    public PortfolioAnalysisParamsDialog()
    {
        InitializeComponent();

        Benchmark = "^IBEX";
        EndDateOffset = DateTime.Now;
        StartDateOffset = new DateTimeOffset(DateTime.Now).AddYears(-5);
        RiskFreeRate = 0.0125.ToString();
    }

    private void Benchmark_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void RiskFreeRate_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParent();
    }

    private void UpdateParent()
    {
        var parent = Parent as ContentDialog;
        if (parent != null)
        {
            parent.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(benchmark.Text) &&
                                            !string.IsNullOrEmpty(riskFreeRate.Text) &&
                                            System.Convert.ToDouble(riskFreeRate.Text) != 0;
        }
    }


    // https://learn.microsoft.com/en-us/dotnet/standard/datetime/converting-between-datetime-and-offset
    public static DateTime ConvertFromDateTimeOffset(DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
            return dateTime.UtcDateTime;
        else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
        else
            return dateTime.DateTime;
    }
}
