using System.Configuration;
using Python.Runtime;

namespace PortfolioAnalysisApp;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            Runtime.PythonDLL = ConfigurationManager.AppSettings["PythonDll"]?.ToString();

            var cmd = Environment.GetCommandLineArgs();
            PythonEngine.Initialize();

            using (Py.GIL())
            {
                var script1 = File.ReadAllText(@"D:\Development\Projects\C#\PortfolioManager\Scriptlets\portfolio_analysis.py");
                var script2 = File.ReadAllText(@"D:\Development\Projects\C#\PortfolioManager\Scriptlets\portfolio_data.py");

                dynamic module = Py.CreateScope("PortfolioAnalysis");

                module.Exec(script1);
                module.Exec(script2);

                dynamic tickers = PythonEngine.Eval("{" +
                    "\'SAB.MC\' : 1000.0, " +
                    "\'FER.MC\' : 1000.0, " +
                    "\'ITX.MC\' : 1000.0, " +
                    "\'MEL.MC\' : 1000.0, " +
                    "}");

                dynamic fields = PythonEngine.Eval("[" +
                    "\'trailingPegRatio\', " +
                    "\'trailingPE\', " +
                    "\'forwardPE\', " +
                    "\'marketCap\', " +
                    "\'beta\', " +
                    "]");

                dynamic portfolio_data = module.ticker_data(tickers, fields);

                var start_date = "2019-11-28";
                var end_date = "2024-11-28";
                var benchmark = "^IBEX";
                var risk_free_rate = 0.01;

                dynamic portfolio_results = module.portfolio_returns(tickers, start_date, end_date);
                dynamic benchmark_results = module.benchmark_returns(benchmark, start_date, end_date);

                dynamic port_returns = portfolio_results["portfolio returns"];
                dynamic benchmark_returns = benchmark_results["benchmark returns"];

                dynamic comparative_results = module.portfolio_vs_benchmark(port_returns, benchmark_returns, risk_free_rate);

                dynamic df = portfolio_results["data"];
                dynamic ticker_weights = portfolio_results["weights"];
                dynamic analysis_results = module.perform_portfolio_analysis(df, ticker_weights, risk_free_rate);

                Console.ReadKey();
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
        }
        finally
        {
            PythonEngine.Shutdown();
        }
    }
}
