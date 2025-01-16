using Microsoft.VisualBasic.FileIO;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

namespace PortfolioManager.Core.Services;

public class PortfolioItemService : IPortfolioItemService
{
    private List<PortfolioItem> _portfolios = [];

    private bool _initialized = false;

    public string Filename
    {
        get; set;
    }

    public void InitializeData()
    {
        _portfolios.Clear();

        _portfolios = RetrievePortfolioData();

        _initialized = true;
    }

    public List<PortfolioItem> GetData()
    {
        if (!_initialized)
        {
            InitializeData();
        }

        return _portfolios;
    }

    private List<PortfolioItem> RetrievePortfolioData()
    {
        List<PortfolioItem> portfolios = [];

        if (!string.IsNullOrEmpty(Filename))
        {
            using TextFieldParser csvParser = new(Filename);

            csvParser.CommentTokens = ["#"];
            csvParser.SetDelimiters([",", "\t"]);
            csvParser.HasFieldsEnclosedInQuotes = true;

            // Skip the row with the column names
            csvParser.ReadLine();

            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                var fields = csvParser.ReadFields();

                if (fields != null && fields.Length == 3)
                {
                    var name = fields[0].ToString();
                    var ticker = fields[1].ToString();
                    var value = System.Convert.ToDouble(fields[2].ToString());

                    var portfolio = portfolios.Find(x => x.Name == name);

                    if (portfolio != null)
                    {
                        var values = portfolio.TickerValues;
                        values[ticker] = value;
                    }
                    else
                    {
                        portfolio = new PortfolioItem
                        {
                            Name = name,
                            Type = "Equity",
                            // https://adamdawes.com/windows8/win8_segoeuisymbol.html
                            SymbolCode = 57808,   // &#xE1D0
                            SymbolName = "Calculator"
                        };

                        var values = new Dictionary<string, double>
                        {
                            [ticker] = value
                        };

                        portfolio.TickerValues = values;

                        portfolios.Add(portfolio);
                    }
                }
            }
        }

        return portfolios;
    }
}
