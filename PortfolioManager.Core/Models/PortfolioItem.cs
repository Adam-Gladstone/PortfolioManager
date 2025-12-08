using System.Data;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper.Contrib.Extensions;

namespace PortfolioManager.Core.Models;


public partial class PortfolioItem : ObservableObject
{
    ///////////////////////////////////////////////////////////////////////////
    // Static data describing a portfolio

    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public List<Holding> Holdings { get; set; } = [];

    public double TotalHoldings { get; set; }

    public List<Transaction> Transactions { get; set; } = [];

    public string Components
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var holding in Holdings)
            {
                sb.AppendLine($"{holding.Ticker}\t({holding.Percent.ToString("F2")}%)");
            }
            return sb.ToString();
        }
    }

    public bool ApplyFilter(string filter)
    {
        return Name.Contains(
                filter, StringComparison.InvariantCultureIgnoreCase)
            || (Type is not null && Type.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            ;
    }


    ///////////////////////////////////////////////////////////////////////////
    // Parameters acquired for the selected portfolio just prior to the analysis

    public string Benchmark { get; set; }
    
    public DateTime Start { get; set; }
    
    public DateTime End { get; set; }
    
    public double RiskFreeRate { get; set; }

    public List<TickerAttribute> TickerAttributes { get; set; } = [];

    ///////////////////////////////////////////////////////////////////////////
    // Additional portfolio data retrieved from script (ticker_data)

    public DataTable PortfolioData { get; set; }

    public DataTable DividendCalendarData { get; set; }

    ///////////////////////////////////////////////////////////////////////////
    // Data returned after performing the analysis

    public Dictionary<string, List<Tuple<DateTime, double>>> IndividualCumulativeReturns { get; set; }

    public Dictionary<string, Tuple<double, double>> IndividualRiskReward { get; set; }

    public Dictionary<string, double> IndividualSharpeRatio { get; set; }

    public Dictionary<string, List<Tuple<DateTime, double>>> ComparativeCumulativeReturns { get; set; }

    public Dictionary<string, Tuple<double, double>> PortfolioRiskReward { get; set; }

    public string ExceptionMessage { get; set; }
    
    public override string ToString() => $"{Name}";
}
