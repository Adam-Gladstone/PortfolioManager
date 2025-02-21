using System.Diagnostics;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

using Python.Runtime;

namespace PortfolioManager.Core.Services;

public class PythonService : IPythonService
{
    public static string Version 
    { 
        get; private set; 
    }
    
    public static string Build 
    { 
        get; private set; 
    }
    
    public static string Copyright 
    { 
        get; private set; 
    }
    
    public static string ProgramName 
    { 
        get; private set; 
    }
    
    public static string PythonHome 
    { 
        get; private set; 
    }
    
    public static string PythonPath 
    { 
        get; private set; 
    }

    // https://github.com/LykosAI/StabilityMatrix
    private readonly PyIOStream StdOutStream = new();
    private readonly PyIOStream StdErrStream = new();

    public void Initialize(string pythonDll)
    {
        if (string.IsNullOrWhiteSpace(pythonDll))
        {
            throw new ArgumentNullException("The path to the python dll is empty.");
        }

        if (!File.Exists(pythonDll))
        {
            throw new FileNotFoundException($"The python dll was not found at: {pythonDll}");
        }

        Runtime.PythonDLL = pythonDll;

        if (!PythonEngine.IsInitialized)
        {
            var args = new List<string>() { "--noconsole" };

            PythonEngine.Initialize(args);
        }

        Version = PythonEngine.Version;
        Build = PythonEngine.BuildInfo;
        Copyright = PythonEngine.Copyright;
        ProgramName = PythonEngine.ProgramName;
        PythonHome = PythonEngine.PythonHome;
        PythonPath = PythonEngine.PythonPath;

        // Redirect stdout and stderr
        var sys = Py.Import("sys") as PyModule ?? throw new NullReferenceException("sys module not found.");
        sys.Set("stdout", StdOutStream);
        sys.Set("stderr", StdErrStream);
    }

    public bool RunPortfolioAnalysis(PortfolioItem portfolio)
    {
        var hasExceptions = false;

        try
        {
            using (Py.GIL())
            {
                var script = File.ReadAllText(@"D:\Development\Projects\C#\PortfolioManager\Scriptlets\portfolio_analysis.py");
                dynamic module = Py.CreateScope("PortfolioAnalysis");
                module.Exec(script);

                var startDate = portfolio.Start.ToString("yyyy-MM-dd");
                var endDate = portfolio.End.ToString("yyyy-MM-dd");
                var benchmark = portfolio.Benchmark;
                var riskFreeRate = portfolio.RiskFreeRate;

                var tickers = Interop.Converters.ConvertToPythonDictionary(portfolio.TickerValues);

                dynamic portfolioResults = module.portfolio_returns(tickers, startDate, endDate);

                var pyTickerWeights = new PyDict(portfolioResults["weights"]);

                portfolio.TickerWeights = Interop.Converters.ConvertTickerWeights(pyTickerWeights);

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
        finally
        {
            // https://github.com/pythonnet/pythonnet/issues/2469
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            PythonEngine.Shutdown();
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);
        }

        return hasExceptions;
    }
}
