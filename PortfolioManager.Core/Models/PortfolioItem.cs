﻿using CommunityToolkit.Mvvm.ComponentModel;
using Dapper.Contrib.Extensions;

namespace PortfolioManager.Core.Models;

public class TickerValue
{
    public string Ticker
    {
        get; set;
    }

    public double Value
    {
        get; set;
    }
}


public class TickerWeight
{
    public string Ticker 
    { 
        get; set; 
    }

    public double Weight 
    { 
        get; set; 
    }
}

public partial class PortfolioItem : ObservableObject
{
    ///////////////////////////////////////////////////////////////////////////
    // Static data describing a portfolio

    [Key]
    public int Id
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }

    public string Type
    {
        get; set;
    }

    public int SymbolCode
    {
        get; set;
    }

    public string SymbolName
    {
        get; set;
    }

    public char Symbol => (char)SymbolCode;

    public Dictionary<string, double> TickerValues { get; set; } = [];

    public string Components => string.Join(", ", TickerValues.Keys);

    ///////////////////////////////////////////////////////////////////////////
    // Parameters acquired for the selected portfolio just prior to the analysis

    public string Benchmark 
    { 
        get; set; 
    }
    
    public DateTime Start 
    { 
        get; set; 
    }
    
    public DateTime End 
    { 
        get; set; 
    }
    
    public double RiskFreeRate 
    { 
        get; set; 
    }


    ///////////////////////////////////////////////////////////////////////////
    // Data returned after performing the analysis

    public List<TickerWeight> TickerWeights 
    { 
        get; set; 
    }

    public Dictionary<string, List<Tuple<DateTime, double>>> IndividualCumulativeReturns 
    { 
        get; set; 
    }

    public Dictionary<string, Tuple<double, double>> IndividualRiskReward 
    { 
        get; set; 
    }

    public Dictionary<string, double> IndividualSharpeRatio 
    { 
        get; set; 
    }

    public Dictionary<string, List<Tuple<DateTime, double>>> ComparativeCumulativeReturns
    {
        get; set;
    }

    public Dictionary<string, Tuple<double, double>> PortfolioRiskReward
    {
        get; set;
    }

    public string ExceptionMessage 
    { 
        get; set; 
    }

    public override string ToString() => $"{Name}";
}
