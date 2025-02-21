using System;
using System.Diagnostics;
using PortfolioManager.Core.Models;
using Python.Runtime;

namespace PortfolioManager.Core.Services.Interop;
internal class Converters
{
    public static PyDict ConvertToPythonDictionary<TKey, TValue>(Dictionary<TKey, TValue> d) where TKey : notnull
    {
        var pyDict = new PyDict();
        var keys = d.Keys;
        foreach (var key in keys)
        {
            pyDict[key.ToString()] = d[key].ToPython();
        }

        return pyDict;
    }

    public static PyDict ConvertToPythonDictionary(List<TickerValue> tickerValues)
    {
        var pyDict = new PyDict();

        foreach (var item in tickerValues)
        {
            pyDict[item.Ticker.ToString()] = item.Value.ToPython();
        }

        return pyDict;
    }

    public static List<TickerWeight> ConvertTickerWeights(PyDict pyTickerWeights)
    {
        var tickerWeights = new List<TickerWeight>();

        var keys = pyTickerWeights.Keys();
        foreach (var key in keys)
        {
            var tickerWeight = new TickerWeight
            {
                Ticker = key.ToString(),
                Weight = System.Convert.ToDouble(pyTickerWeights[key])
            };

            tickerWeights.Add(tickerWeight);
        }

        return tickerWeights;
    }

    public static Dictionary<string, List<Tuple<DateTime, double>>> ConvertIndividualCumulativeReturns(PyDict pyTickerWeights, PyDict pyAnalysisResults)
    {
        var individualCumulativeReturns = new Dictionary<string, List<Tuple<DateTime, double>>>();

        dynamic pyCumulativeReturns = pyAnalysisResults["individual cumulative returns"];

        var keys = pyTickerWeights.Keys();
        foreach (var key in keys)
        {
            PySequence index = pyCumulativeReturns[key.ToString()].index;
            PySequence values = pyCumulativeReturns[key.ToString()].values;

            List<Tuple<DateTime, double>> observations = [];

            for (var i = 0; i < index.Length(); i++)
            {
                var x = DateTime.Parse(index[i].ToString());
                var y = (double)values[i].AsManagedObject(typeof(double));

                observations.Add(new Tuple<DateTime, double>(x, y));
            }

            individualCumulativeReturns[key.ToString()] = observations;
        }

        return individualCumulativeReturns;
    }

    public static Dictionary<string, Tuple<double, double>> ConvertIndividualVolatility(PyDict pyTickerWeights, PyDict pyAnalysisResults)
    {
        var individualRiskReward = new Dictionary<string, Tuple<double, double>>();

        dynamic pyIndividualVolatility = pyAnalysisResults["individual volatility"];
        dynamic pyCumulativeReturns = pyAnalysisResults["individual cumulative returns"];

        var keys = pyTickerWeights.Keys();
        foreach (var key in keys)
        {
            PySequence index = pyCumulativeReturns[key.ToString()].index;
            PySequence returns = pyCumulativeReturns[key.ToString()].values;

            var i = (int)index.Length() - 1;

            var x = (double)pyIndividualVolatility[key.ToString()].AsManagedObject(typeof(double));
            var y = (double)returns[i].AsManagedObject(typeof(double));

            var observation = new Tuple<double, double>(x, y);

            individualRiskReward[key.ToString()] = observation;
        }

        return individualRiskReward;
    }

    public static Dictionary<string, double> ConvertIndividualSharpeRatio(PyDict pyTickerWeights, PyDict pyAnalysisResults)
    {
        var individualSharpeRatio = new Dictionary<string, double>();

        dynamic pyIndividualSharpeRatio = pyAnalysisResults["individual Sharpe ratio"];

        var keys = pyTickerWeights.Keys();
        foreach (var key in keys)
        {
            var sharpeRatio = (double)pyIndividualSharpeRatio[key.ToString()].AsManagedObject(typeof(double));

            individualSharpeRatio[key.ToString()] = sharpeRatio;
        }

        return individualSharpeRatio;
    }

    public static Dictionary<string, List<Tuple<DateTime, double>>> ConvertComparativeCumulativeReturns(PyDict pyComparativeResults)
    {
        var comparativeCumulativeReturns = new Dictionary<string, List<Tuple<DateTime, double>>>();

        dynamic pyPortfolioCumulativeReturns = pyComparativeResults["portfolio cumulative returns"];
        {
            PySequence index = pyPortfolioCumulativeReturns.index;
            PySequence values = pyPortfolioCumulativeReturns.values;

            List<Tuple<DateTime, double>> observations = [];

            for (var i = 0; i < index.Length(); i++)
            {
                var x = DateTime.Parse(index[i].ToString());
                var y = (double)values[i].AsManagedObject(typeof(double));

                observations.Add(new Tuple<DateTime, double>(x, y));
            }

            comparativeCumulativeReturns["Portfolio"] = observations;
        }

        dynamic pyBenchmarkCumulativeReturns = pyComparativeResults["benchmark cumulative returns"];
        {
            PySequence index = pyBenchmarkCumulativeReturns.index;
            PySequence values = pyBenchmarkCumulativeReturns.values;

            List<Tuple<DateTime, double>> observations = [];

            for (var i = 0; i < index.Length(); i++)
            {
                var x = DateTime.Parse(index[i].ToString());

                var y = (double)values[i][0].AsManagedObject(typeof(double));

                observations.Add(new Tuple<DateTime, double>(x, y));
            }

            comparativeCumulativeReturns["Benchmark"] = observations;
        }

        return comparativeCumulativeReturns;
    }

    public static Dictionary<string, Tuple<double, double>> ConvertComparativeVolatility(string benchmark, PyDict pyComparativeResults)
    {
        var comparativeRiskReward = new Dictionary<string, Tuple<double, double>>();

        dynamic pyPortfolioAnnualizedVolatility = pyComparativeResults["portfolio volatility"];
        dynamic pyPortfolioCumulativeReturns = pyComparativeResults["portfolio cumulative returns"];

        {
            PySequence index = pyPortfolioCumulativeReturns.index;
            PySequence returns = pyPortfolioCumulativeReturns.values;

            var i = (int)index.Length() - 1;

            var x = (double)pyPortfolioAnnualizedVolatility.AsManagedObject(typeof(double));
            var y = (double)returns[i].AsManagedObject(typeof(double));

            var observation = new Tuple<double, double>(x, y);

            comparativeRiskReward["Portfolio"] = observation;
        }

        dynamic pyBenchmarkAnnualizedVolatility = pyComparativeResults["benchmark volatility"];
        dynamic pyBenchmarkCumulativeReturns = pyComparativeResults["benchmark cumulative returns"];

        {
            PySequence index = pyBenchmarkCumulativeReturns.index;
            PySequence returns = pyBenchmarkCumulativeReturns.values;

            var i = (int)index.Length() - 1;

            var x = (double)pyBenchmarkAnnualizedVolatility[benchmark].AsManagedObject(typeof(double));

            var y = (double)returns[i][0].AsManagedObject(typeof(double));

            var observation = new Tuple<double, double>(x, y);

            comparativeRiskReward["Benchmark"] = observation;
        }

        return comparativeRiskReward;
    }
}
