# Patrimonio Utils

Portfolio tracking and bank account monitoring utilities written in F#.

## Projects

### Tracker
Portfolio tracking tool that calculates the current value of your investment holdings.

**Features:**
- Reads portfolio holdings from JSON
- Fetches real-time quotes from Yahoo Finance
- Calculates portfolio value in USD and EUR
- Shows price changes and percentage movements

**Usage:**
```bash
dotnet run --project Tracker -- portfolio.json
```

**JSON Format:**
```json
[
  {
    "Symbol": "AAPL",
    "Quantity": 10.5
  },
  {
    "Symbol": "MSFT",
    "Quantity": 5.25
  },
  {
    "Symbol": "USD",
    "Quantity": 1000.00
  }
]
```

### Account
Bank account monitoring tool that retrieves recent transactions from Fio Bank using a read-only token.

**Features:**
- Fetches last 2 days of transactions from Fio Bank
- Displays account balance and IBAN
- Shows transaction details (date, amount, comment)
- Supports multiple accounts

**Usage:**
```bash
dotnet run --project Account -- accounts.json
```

**JSON Format:**
```json
[
  {
    "token": "your-fio-readonly-token-here"
  },
  {
    "token": "another-account-token"
  }
]
```

**Getting a Fio Bank Token:**
1. Log in to Fio internet banking
2. Navigate to Settings → API
3. Generate a read-only token
4. Copy the token to your JSON file

## Library
Shared library containing:
- `Portfolio.fs` - Portfolio data structures, quote fetching, formatting
- `CurrentAccount.fs` - Fio Bank API client and transaction handling

## Building
```bash
dotnet build
```

## Running Tests
```bash
dotnet test
```
