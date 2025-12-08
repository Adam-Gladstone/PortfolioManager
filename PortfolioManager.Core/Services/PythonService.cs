using System.Collections.Generic;
using System.Diagnostics;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

using Python.Runtime;
using Python.Runtime.Codecs;

namespace PortfolioManager.Core.Services;

public class PythonService : IPythonService
{
    private dynamic module;

    public static string Version { get; private set; }
    
    public static string Build { get; private set; }
    
    public static string Copyright { get; private set; }
    
    public static string ProgramName { get; private set; }
    
    public static string PythonHome { get; private set; }
    
    public static string PythonPath { get; private set; }

    public bool Initialized { get; private set; }


    // https://github.com/LykosAI/StabilityMatrix
    private readonly PyIOStream StdOutStream = new();
    private readonly PyIOStream StdErrStream = new();

    public async Task InitializeAsync(string pythonDll, string scriptFolder)
    {
        if (!Initialized)
        {
            if (string.IsNullOrWhiteSpace(pythonDll))
            {
                throw new ArgumentNullException(nameof(pythonDll), "The path to the python dll is empty.");
            }

            if (!File.Exists(pythonDll))
            {
                throw new FileNotFoundException($"The python dll was not found at: {pythonDll}");
            }

            if (string.IsNullOrWhiteSpace(scriptFolder))
            {
                throw new ArgumentNullException(nameof(scriptFolder), "The script folder is empty.");
            }

            Runtime.PythonDLL = pythonDll;

            if (!PythonEngine.IsInitialized)
            {
                var args = new List<string>() { "--noconsole" };

                PythonEngine.Initialize(args);

                Version = PythonEngine.Version;
                Build = PythonEngine.BuildInfo;
                Copyright = PythonEngine.Copyright;
                ProgramName = PythonEngine.ProgramName;
                PythonHome = PythonEngine.PythonHome;
                PythonPath = PythonEngine.PythonPath;

                module = Py.CreateScope("PortfolioAnalysis");

                // Redirect stdout and stderr
                var sys = Py.Import("sys") as PyModule ?? throw new NullReferenceException("sys module not found.");
                sys.Set("stdout", StdOutStream);
                sys.Set("stderr", StdErrStream);

                Initialized = await InitializeScriptsAsync(scriptFolder);
            }
        }
    }

    private async Task<bool> InitializeScriptsAsync(string scriptFolder)
    {
        bool initialized;

        var script1 = await File.ReadAllTextAsync($@"{scriptFolder}\portfolio_analysis.py");
        var script2 = await File.ReadAllTextAsync($@"{scriptFolder}\portfolio_data.py");

        module.Exec(script1);
        module.Exec(script2);

        initialized = true;        

        return initialized;
    }

    public void Shutdown()
    {
        if (Initialized)
        {
            Initialized = false;

            // https://github.com/pythonnet/pythonnet/issues/2469
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            PythonEngine.Shutdown();
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);
        }
    }

    public bool RunPortfolioAnalysis(PortfolioItem portfolio)
    {
        var hasExceptions = false;

        try
        {
            using (Py.GIL())
            {
                var startDate = portfolio.Start.ToString("yyyy-MM-dd");
                var endDate = portfolio.End.ToString("yyyy-MM-dd");
                var benchmark = portfolio.Benchmark;
                var riskFreeRate = portfolio.RiskFreeRate;

                var tickers = Interop.Converters.ConvertToPythonDictionary(portfolio.Holdings);

                dynamic portfolioResults = module.portfolio_returns(tickers, startDate, endDate);

                var pyTickerWeights = new PyDict(portfolioResults["weights"]);

                dynamic portfolioData = portfolioResults["data"];

                PyDict pyAnalysisResults = module.perform_portfolio_analysis(portfolioData, pyTickerWeights, riskFreeRate);

                portfolio.IndividualCumulativeReturns = Interop.Converters.ConvertIndividualCumulativeReturns(pyTickerWeights, pyAnalysisResults);

                portfolio.IndividualRiskReward = Interop.Converters.ConvertIndividualVolatility(pyTickerWeights, pyAnalysisResults);

                portfolio.IndividualSharpeRatio = Interop.Converters.ConvertIndividualSharpeRatio(pyTickerWeights, pyAnalysisResults);

                dynamic benchmarkResults = module.benchmark_returns(benchmark, startDate, endDate);

                dynamic portfolioReturns = portfolioResults["portfolio returns"];
                dynamic benchmarkReturns = benchmarkResults["benchmark returns"];

                PyDict pyComparativeResults = module.portfolio_vs_benchmark(portfolioReturns, benchmarkReturns, riskFreeRate);

                portfolio.ComparativeCumulativeReturns = Interop.Converters.ConvertComparativeCumulativeReturns(pyComparativeResults);

                portfolio.PortfolioRiskReward = Interop.Converters.ConvertComparativeVolatility(benchmark, pyComparativeResults);
            }
        }
        catch (Exception exc)
        {
            hasExceptions = true;
            
            portfolio.ExceptionMessage = exc.Message;

            Debug.WriteLine(exc.Message);
        }

        return hasExceptions;
    }

    public bool RetrievePortfolioData(PortfolioItem portfolio)
    {
        var hasExceptions = false;

        try
        {
            using (Py.GIL())
            {
                var tickers = Interop.Converters.ConvertToPythonDictionary(portfolio.Holdings);

                var fields = Interop.Converters.ConvertToPythonList(portfolio.TickerAttributes);

                dynamic portfolio_data = module.ticker_data(tickers, fields);

                PyObject pyObject = portfolio_data;

                portfolio.PortfolioData = Interop.Converters.ConvertToDataTable(pyObject, portfolio);
            }
        }
        catch (Exception exc)
        {
            hasExceptions = true;

            portfolio.ExceptionMessage = exc.Message;

            Debug.WriteLine(exc.Message);
        }

        return hasExceptions;
    }

    public bool RetrieveDividendCalendarData(PortfolioItem portfolio)
    {
        var hasExceptions = false;

        try
        {
            using (Py.GIL())
            {
                var tickers = Interop.Converters.ConvertToPythonDictionary(portfolio.Holdings);

                var fields = Interop.Converters.ConvertToPythonList(portfolio.TickerAttributes);

                dynamic portfolio_data = module.ticker_data(tickers, fields);

                PyObject pyObject = portfolio_data;

                portfolio.DividendCalendarData = Interop.Converters.ConvertToDataTable(pyObject, portfolio);
            }
        }
        catch (Exception exc)
        {
            hasExceptions = true;

            portfolio.ExceptionMessage = exc.Message;

            Debug.WriteLine(exc.Message);
        }

        return hasExceptions;
    }
}
