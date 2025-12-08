"""
portfolio_data.py

[X] flake8  - passed
[X] black   - passed
"""

# Pandas
import pandas as pd

# yfinance to retrieve financial data
import yfinance as yf

import warnings

warnings.filterwarnings("ignore")


def get_value(info: dict, key: str):
    """
    Retrieve a value from a ticker info dictionary
    or None if the key doesn't exist
    """

    if info is not None:
        if key in info.keys():
            return info[key]

    return None


def get_items(symbol: str, fields: list) -> list:
    """
    Retrieve data from the corresponding fields for the specified ticker
    """

    items: list = [symbol]

    try:
        tk: dict = yf.Ticker(symbol)
        info: dict = tk.info

        for field in fields:
            items.append(get_value(info, field))
    except Exception:
        pass

    return items


def ticker_data(symbols: list, fields: list) -> pd.DataFrame:
    """
    Retrieve the specified fields from the input tickers
    """

    headers: list = ["Ticker"]

    for field in fields:
        headers.append(field)

    rows: list = []

    for symbol in symbols:
        rows.append(get_items(symbol, fields))

    df = pd.DataFrame(rows, columns=headers)
    return df


"""
tickers = {
    'SAB.MC': (1000.0),
    'FER.MC': (750.0),
    'ITX.MC': (250.0),
    'MEL.MC': (2000.0)
}

fields = ['trailingPegRatio', 'trailingPE', 'forwardPE', 'marketCap', 'beta']

data = ticker_data(tickers, fields)
print(data)
"""
