using Microsoft.VisualBasic.FileIO;
using PortfolioManager.Core.Contracts.Services;
using PortfolioManager.Core.Models;

namespace PortfolioManager.Services;
public class TickerAttributesService : ITickerAttributesService
{
    public List<TickerAttribute> TickerAttributes { get; private set; } = [];

    public void Initialize(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            TickerAttributes = GetDefaultTickerAttributes();
        }
        else
        {
            TickerAttributes = GetTickerAttributes(filename);

            if (TickerAttributes is null || TickerAttributes.Count <= 0)
            {
                TickerAttributes = GetDefaultTickerAttributes();
            }
        }
    }

    private static List<TickerAttribute> GetDefaultTickerAttributes()
    {
        var attributes = new List<TickerAttribute>()
        {
            new() { Name = "industry", Type = "string", Selected = false },
            new() { Name = "sector", Selected = true },
            new() { Name = "fullTimeEmployees", Selected = false },
            new() { Name = "previousClose", Selected = true },
            new() { Name = "open", Selected = false },
            new() { Name = "dayLow", Selected = false },
            new() { Name = "dayHigh", Selected = false },
            new() { Name = "regularMarketPreviousClose", Selected = false },
            new() { Name = "regularMarketOpen", Selected = false },
            new() { Name = "regularMarketDayLow", Selected = false },
            new() { Name = "regularMarketDayHigh", Selected = false },
            new() { Name = "dividendRate", Selected = false },
            new() { Name = "dividendYield", Selected = false },
            new() { Name = "exDividendDate", Selected = false },
            new() { Name = "payoutRatio", Selected = false },
            new() { Name = "fiveYearAvgDividendYield", Selected = false },
            new() { Name = "beta", Selected = true },
            new() { Name = "trailingPE", Selected = true },
            new() { Name = "forwardPE", Selected = true },
            new() { Name = "volume", Selected = false },
            new() { Name = "regularMarketVolume", Selected = false },
            new() { Name = "averageVolume", Selected = false },
            new() { Name = "averageVolume10days", Selected = false },
            new() { Name = "averageDailyVolume10Day", Selected = false },
            new() { Name = "bid", Selected = false },
            new() { Name = "ask", Selected = false },
            new() { Name = "bidSize", Selected = false },
            new() { Name = "askSize", Selected = false },
            new() { Name = "marketCap", Selected = true },
            new() { Name = "fiftyTwoWeekLow", Selected = false },
            new() { Name = "fiftyTwoWeekHigh", Selected = false },
            new() { Name = "priceToSalesTrailing12Months", Selected = false },
            new() { Name = "fiftyDayAverage", Selected = false },
            new() { Name = "twoHundredDayAverage", Selected = false },
            new() { Name = "trailingAnnualDividendRate", Selected = false },
            new() { Name = "trailingAnnualDividendYield", Selected = false },
            new() { Name = "currency", Selected = false },
            new() { Name = "tradeable", Selected = false },
            new() { Name = "enterpriseValue", Selected = false },
            new() { Name = "profitMargins", Selected = false },
            new() { Name = "floatShares", Selected = false },
            new() { Name = "sharesOutstanding", Selected = false },
            new() { Name = "sharesShort", Selected = false },
            new() { Name = "sharesShortPriorMonth", Selected = false },
            new() { Name = "sharesShortPreviousMonthDate", Selected = false },
            new() { Name = "dateShortInterest", Selected = false },
            new() { Name = "sharesPercentSharesOut", Selected = false },
            new() { Name = "heldPercentInsiders", Selected = false },
            new() { Name = "heldPercentInstitutions", Selected = false },
            new() { Name = "shortRatio", Selected = false },
            new() { Name = "shortPercentOfFloat", Selected = false },
            new() { Name = "impliedSharesOutstanding", Selected = false },
            new() { Name = "bookValue", Selected = false },
            new() { Name = "priceToBook", Selected = false },
            new() { Name = "lastFiscalYearEnd", Selected = false },
            new() { Name = "nextFiscalYearEnd", Selected = false },
            new() { Name = "mostRecentQuarter", Selected = false },
            new() { Name = "earningsQuarterlyGrowth", Selected = false },
            new() { Name = "netIncomeToCommon", Selected = false },
            new() { Name = "trailingEps", Selected = false },
            new() { Name = "forwardEps", Selected = false },
            new() { Name = "lastSplitFactor", Selected = false },
            new() { Name = "lastSplitDate", Selected = false },
            new() { Name = "enterpriseToRevenue ", Selected = false },
            new() { Name = "enterpriseToEbitda", Selected = false },
            new() { Name = "52WeekChange", Selected = false },
            new() { Name = "SandP52WeekChange", Selected = false },
            new() { Name = "lastDividendValue", Selected = false },
            new() { Name = "lastDividendDate", Selected = false },
            new() { Name = "quoteType", Selected = false },
            new() { Name = "currentPrice", Selected = false },
            new() { Name = "targetHighPrice", Selected = false },
            new() { Name = "targetLowPrice", Selected = false },
            new() { Name = "targetMeanPrice", Selected = false },
            new() { Name = "targetMedianPrice", Selected = false },
            new() { Name = "recommendationMean", Selected = false },
            new() { Name = "recommendationKey", Selected = false },
            new() { Name = "numberOfAnalystOpinions", Selected = false },
            new() { Name = "totalCash", Selected = false },
            new() { Name = "totalCashPerShare", Selected = false },
            new() { Name = "ebitda", Selected = false },
            new() { Name = "totalDebt", Selected = false },
            new() { Name = "quickRatio", Selected = false },
            new() { Name = "currentRatio", Selected = false },
            new() { Name = "totalRevenue", Selected = false },
            new() { Name = "debtToEquity", Selected = false },
            new() { Name = "revenuePerShare", Selected = false },
            new() { Name = "returnOnAssets", Selected = false },
            new() { Name = "returnOnEquity", Selected = false },
            new() { Name = "grossProfits", Selected = false },
            new() { Name = "freeCashflow", Selected = false },
            new() { Name = "operatingCashflow", Selected = false },
            new() { Name = "earningsGrowth", Selected = false },
            new() { Name = "revenueGrowth", Selected = false },
            new() { Name = "grossMargins", Selected = false },
            new() { Name = "ebitdaMargins", Selected = false },
            new() { Name = "operatingMargins", Selected = false },
            new() { Name = "financialCurrency", Selected = false },
            new() { Name = "symbol", Selected = false },
            new() { Name = "language", Selected = false },
            new() { Name = "region", Selected = false },
            new() { Name = "typeDisp", Selected = false },
            new() { Name = "quoteSourceName", Selected = false },
            new() { Name = "triggerable", Selected = false },
            new() { Name = "customPriceAlertConfidence", Selected = false },
            new() { Name = "preMarketTime ", Selected = false },
            new() { Name = "regularMarketTime", Selected = false },
            new() { Name = "exchange ", Selected = false },
            new() { Name = "messageBoardId", Selected = false },
            new() { Name = "exchangeTimezoneName", Selected = false },
            new() { Name = "exchangeTimezoneShortName", Selected = false },
            new() { Name = "gmtOffSetMilliseconds", Selected = false },
            new() { Name = "market", Selected = false },
            new() { Name = "esgPopulated", Selected = false },
            new() { Name = "regularMarketChangePercent", Selected = false },
            new() { Name = "regularMarketPrice", Selected = false },
            new() { Name = "hasPrePostMarketData", Selected = false },
            new() { Name = "firstTradeDateMilliseconds", Selected = false },
            new() { Name = "preMarketChange", Selected = false },
            new() { Name = "preMarketChangePercent", Selected = false },
            new() { Name = "preMarketPrice", Selected = false },
            new() { Name = "regularMarketChange", Selected = false },
            new() { Name = "regularMarketDayRange", Selected = false },
            new() { Name = "fullExchangeName", Selected = false },
            new() { Name = "averageDailyVolume3Month", Selected = false },
            new() { Name = "fiftyTwoWeekLowChange", Selected = false },
            new() { Name = "fiftyTwoWeekLowChangePercent", Selected = false },
            new() { Name = "fiftyTwoWeekRange", Selected = false },
            new() { Name = "fiftyTwoWeekHighChange", Selected = false },
            new() { Name = "fiftyTwoWeekHighChangePercent", Selected = false },
            new() { Name = "fiftyTwoWeekChangePercent", Selected = false },
            new() { Name = "dividendDate", Selected = false },
            new() { Name = "earningsTimestamp", Selected = false },
            new() { Name = "earningsTimestampStart", Selected = false },
            new() { Name = "earningsTimestampEnd", Selected = false },
            new() { Name = "earningsCallTimestampStart", Selected = false },
            new() { Name = "earningsCallTimestampEnd", Selected = false },
            new() { Name = "isEarningsDateEstimate", Selected = false },
            new() { Name = "epsTrailingTwelveMonths", Selected = false },
            new() { Name = "epsForward", Selected = false },
            new() { Name = "epsCurrentYear", Selected = false },
            new() { Name = "priceEpsCurrentYear", Selected = false },
            new() { Name = "fiftyDayAverageChange", Selected = false },
            new() { Name = "fiftyDayAverageChangePercent", Selected = false },
            new() { Name = "twoHundredDayAverageChange", Selected = false },
            new() { Name = "twoHundredDayAverageChangePercent", Selected = false },
            new() { Name = "sourceInterval", Selected = false },
            new() { Name = "exchangeDataDelayedBy", Selected = false },
            new() { Name = "averageAnalystRating", Selected = false },
            new() { Name = "cryptoTradeable", Selected = false },
            new() { Name = "shortName", Selected = false },
            new() { Name = "longName", Selected = false },
            new() { Name = "marketState", Selected = false },
            new() { Name = "displayName", Selected = false },
            new() { Name = "trailingPegRatio", Selected = false },
        };

        return attributes;
    }

    private List<TickerAttribute> GetTickerAttributes(string filename)
    {
        var attributes = new List<TickerAttribute>();

        if (!string.IsNullOrEmpty(filename))
        {
            using TextFieldParser csvParser = new(filename);

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
                    var type = fields[1].ToString();
                    var selected = System.Convert.ToBoolean(fields[2].ToString());

                    var tickerAttribute = new TickerAttribute
                    {
                        Name = name,
                        Type = type,
                        Selected = selected
                    };

                    attributes.Add(tickerAttribute);
                }
            }
        }
        return attributes;
    }
}
