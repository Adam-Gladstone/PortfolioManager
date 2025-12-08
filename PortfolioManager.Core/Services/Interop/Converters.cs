using System.Data;
using System.Diagnostics;
using PortfolioManager.Core.Models;
using Python.Runtime;
using Python.Runtime.Codecs;

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

    public static PyDict ConvertToPythonDictionary(List<Holding> holdings)
    {
        var pyDict = new PyDict();

        foreach (var item in holdings)
        {
            pyDict[item.Ticker.ToString()] = item.TotalCost.ToPython();
        }

        return pyDict;
    }

    public static PyList ConvertToPythonList(List<TickerAttribute> attributes)
    {
        var pyList = new PyList();

        foreach (var attribute in attributes)
        {
            var name = attribute.Name;
            pyList.Append(name.ToPython());
        }

        return pyList;
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

    public static DateTime ConvertToDateTime(object pyIntVal)
    {
        var val = Convert.ToInt64(pyIntVal);        // 1745884800
        val /= 60;                                  //   29098080
        val /= 60;                                  //     484968
        val /= 24;                                  //      20207
        val += 25569;                               // += 01/01/1970 = 45776
        return DateTime.FromOADate((double)val);
    }

    public static object ConvertData(TickerAttribute attribute, object pyData)
    {
        object val = null;

        try
        {
            val = attribute.Type switch
            {
                "double" => Convert.ToDouble(pyData),
                "Date" => ConvertToDateTime(pyData),
                "long" => Convert.ToInt64(pyData),
                _ => pyData.ToString(),
            };
        }
        catch (Exception e)
        {
            var message = $"Unable to convert {attribute.Name} to {attribute.Type} with value {pyData}\n. " +
                $"The exception was:\n";
            message += e.Message;
            Debug.WriteLine(message);
        }

        return val;
    }

    public static DataTable ConvertToDataTable(PyObject pyObject, PortfolioItem portfolio)
    {
        var dt = new DataTable();

        var codec = IterableDecoder.Instance;
        if (codec.TryDecode(pyObject, out System.Collections.IEnumerable enumerable))
        {
            var c = 0;

            foreach (var column in enumerable)
            {
                var columnName = column.ToString();

                dt.Columns.Add(columnName);

                var columnData = pyObject[columnName];

                if (codec.TryDecode(columnData, out System.Collections.IEnumerable columnEnumerable))
                {
                    var r = 0;
                    
                    foreach (var row in columnEnumerable)
                    {
                        DataRow dr;

                        if (c == 0)
                        {
                            dr = dt.NewRow();
                        } 
                        else
                        {
                            dr = dt.Rows[r];
                        }

                        var attribute = portfolio.TickerAttributes.Find(x => x.Name == columnName);

                        dr[columnName] = (attribute != null) ? ConvertData(attribute, row) : row.ToString();

                        if (c == 0)
                        {
                            dt.Rows.Add(dr);
                        }

                        r++;
                    }
                }
                c++;
            }
        }

        return dt;
    }

    public static DataTable ConvertPortfolioData(List<List<object>> table, int rows, int cols)
    {
        var dataTable = new DataTable();

        for (var r = 0; r < rows; r++)
        {
            DataRow dataRow = null;
            if(r != 0)
            {
                dataRow = dataTable.NewRow();
            }

            for (var c = 0; c < cols; c++)
            {
                if (r == 0)
                {
                    dataTable.Columns.Add(new DataColumn(table[c][r].ToString()));
                }
                else
                {
                    dataRow[c] = table[c][r].ToString();
                }
            }

            if(r != 0)
            {
                dataTable.Rows.Add(dataRow);
            }
        }

        return dataTable;
    }
}
