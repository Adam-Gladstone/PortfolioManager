using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PortfolioManager.Contracts.ViewModels;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

namespace PortfolioManager.ViewModels;

public partial class PortfolioDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPythonService _pythonService;

    [ObservableProperty]
    private PortfolioItem? item;

    [ObservableProperty]
    private PlotModel portfolioWeightsModel;

    [ObservableProperty]
    private PlotModel historicPerformanceAssetsModel;

    [ObservableProperty]
    private PlotModel riskRewardModel;

    [ObservableProperty]
    private PlotModel sharpeRatioModel;

    [ObservableProperty]
    private PlotModel comparativeReturnsModel;

    [ObservableProperty]
    private PlotModel comparativeRiskRewardModel;


    public PortfolioDetailViewModel(IPythonService pythonService)
    {
        _pythonService = pythonService;

        var settings = App.GetService<SettingsViewModel>();

        var pythonDll = settings.PythonLibrary;
        _pythonService.Initialize(pythonDll);

        PortfolioWeightsModel = InitializePortfolioWeights([]);
        HistoricPerformanceAssetsModel = InitializeIndividualCumulativeReturns([]);
        RiskRewardModel = InitializeIndividualRiskReward([]);
        SharpeRatioModel = InitializeIndividualSharpeRatio([]);
        ComparativeReturnsModel = InitializeComparativeCumulativeReturns([]);
        ComparativeRiskRewardModel = InitializeComparativeRiskReward([]);
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is PortfolioItem item)
        {
            if(_pythonService.RunPortfolioAnalysis(item))
            {
                var message = $"Portfolio Analysis Exception\n{item.ExceptionMessage}";
                App.ReportException(new Exception(message));
            }
            else 
            {
                PortfolioWeightsModel = InitializePortfolioWeights(item.TickerWeights);

                HistoricPerformanceAssetsModel = InitializeIndividualCumulativeReturns(item.IndividualCumulativeReturns);

                RiskRewardModel = InitializeIndividualRiskReward(item.IndividualRiskReward);

                SharpeRatioModel = InitializeIndividualSharpeRatio(item.IndividualSharpeRatio);

                ComparativeReturnsModel = InitializeComparativeCumulativeReturns(item.ComparativeCumulativeReturns);

                ComparativeRiskRewardModel = InitializeComparativeRiskReward(item.PortfolioRiskReward);
            }
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private static PlotModel InitializePortfolioWeights(List<TickerWeight> tickerWeights)
    {
        var piePlot = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (tickerWeights != null)
        {
            dynamic seriesP1 = new PieSeries
            {
                StrokeThickness = 2.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            foreach (var tickerWeight in tickerWeights)
            {
                var item = new PieSlice(tickerWeight.Ticker, tickerWeight.Weight)
                {
                    IsExploded = true
                };

                seriesP1.Slices.Add(item);
            }

            piePlot.Series.Add(seriesP1);
        }

        return piePlot;
    }

    private static PlotModel InitializeIndividualCumulativeReturns(Dictionary<string, List<Tuple<DateTime, double>>> individualCumulativeReturns)
    {
        var lineSeriesModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (individualCumulativeReturns != null)
        {
            lineSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Returns (%)",
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside
            });

            lineSeriesModel.Axes.Add(new DateTimeAxis
            {
                Title = "Date",
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.Inside
            });

            foreach (var key in individualCumulativeReturns.Keys)
            {
                var lineSeries = new LineSeries { Title = key };

                var list = individualCumulativeReturns[key];

                foreach (var i in list)
                {
                    lineSeries.Points.Add(new DataPoint((double)i.Item1.ToOADate(), i.Item2));
                }

                lineSeriesModel.Series.Add(lineSeries);
            }
        }

        return lineSeriesModel;
    }

    private static PlotModel InitializeIndividualRiskReward(Dictionary<string, Tuple<double, double>> individualVolatility)
    {
        var stemSeriesModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (individualVolatility != null)
        {
            stemSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Returns (%)",
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside
            });

            stemSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Annualized Volatility (%)",
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.Inside,
                Minimum = 0,
                Maximum = 100
            });

            foreach (var key in individualVolatility.Keys)
            {
                var stemSeries = new StemSeries
                {
                    Title = key,
                    MarkerStroke = OxyColors.Green,
                    MarkerType = MarkerType.Circle
                };

                var obs = individualVolatility[key];

                stemSeries.Points.Add(new DataPoint(obs.Item1, obs.Item2));

                stemSeriesModel.Series.Add(stemSeries);
            }
        }

        return stemSeriesModel;
    }
    private static PlotModel InitializeIndividualSharpeRatio(Dictionary<string, double> individualSharpeRatio)
    {
        var barSeriesModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (individualSharpeRatio != null)
        {
            var count = 0;
            var categories = new string[individualSharpeRatio.Count];
            var bars = new List<BarItem>(); 
            foreach (var key in individualSharpeRatio.Keys)
            {
                bars.Add(new BarItem { Value = individualSharpeRatio[key] });
                categories[count++] = key;
            }

            var barSeries = new BarSeries
            {
                ItemsSource = bars,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}%",
                TextColor = OxyColors.WhiteSmoke
            };

            barSeriesModel.Series.Add(barSeries);
            barSeriesModel.Axes.Add(new CategoryAxis 
            {
                Position = AxisPosition.Left,
                Key = "Sharpe Ratio",
                ItemsSource = categories 
            });
        }

        return barSeriesModel;
    }

    private static PlotModel InitializeComparativeCumulativeReturns(Dictionary<string, List<Tuple<DateTime, double>>> comparativeCumulativeReturns)
    {
        var lineSeriesModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (comparativeCumulativeReturns != null)
        {
            lineSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Cumulative Returns (%)",
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside
            });

            lineSeriesModel.Axes.Add(new DateTimeAxis
            {
                Title = "Date",
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.Inside
            });

            foreach (var key in comparativeCumulativeReturns.Keys)
            {
                var lineSeries = new LineSeries { Title = key };

                var list = comparativeCumulativeReturns[key];

                foreach (var i in list)
                {
                    lineSeries.Points.Add(new DataPoint((double)i.Item1.ToOADate(), i.Item2));
                }

                lineSeriesModel.Series.Add(lineSeries);
            }
        }

        return lineSeriesModel;
    }

    private static PlotModel InitializeComparativeRiskReward(Dictionary<string, Tuple<double, double>> comparativeVolatility)
    {
        var stemSeriesModel = new PlotModel
        {
            PlotAreaBorderColor = OxyColors.Transparent
        };

        if (comparativeVolatility != null)
        {
            stemSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Returns (%)",
                Position = AxisPosition.Left,
                TickStyle = TickStyle.Inside
            });

            stemSeriesModel.Axes.Add(new LinearAxis
            {
                Title = "Annualized Volatility (%)",
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.Inside,
                Minimum = 0,
                Maximum = 100
            });

            foreach (var key in comparativeVolatility.Keys)
            {
                var stemSeries = new StemSeries
                {
                    Title = key,
                    MarkerStroke = OxyColors.Green,
                    MarkerType = MarkerType.Circle
                };

                var obs = comparativeVolatility[key];

                stemSeries.Points.Add(new DataPoint(obs.Item1, obs.Item2));

                stemSeriesModel.Series.Add(stemSeries);
            }
        }

        return stemSeriesModel;
    }
}
