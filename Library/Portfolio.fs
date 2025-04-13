module Portfolio

open System
open System.IO
open System.Globalization
open System.Web
open FSharp.Data
open Microsoft.Extensions.Logging
open System.Text.Json
open System
open FsToolkit.ErrorHandling.Operator.Result
open FsToolkit.ErrorHandling

type Asset = { Symbol: string; Quantity: decimal }

type QuotedAsset =
    { Symbol: string
      Quantity: decimal
      Price: decimal
      PriceYesterday: decimal
      PctChange: decimal
      AbsChange: decimal
      Value: decimal }

type Portfolio =
    { Value: decimal
      EurValue: decimal
      Date: DateTimeOffset }

module Portfolio =
    let fromItems (items: QuotedAsset list) (exchangeRate: decimal) (date: DateTimeOffset) =
        let value = items |> List.sumBy _.Value
        let eurValue = value / exchangeRate

        { Value = value
          EurValue = value / exchangeRate
          Date = date }

    let toJsonFile file (portfolio: Portfolio) =
        let content = JsonSerializer.Serialize portfolio
        File.AppendAllText(file, content + Environment.NewLine)

module Format =
    let formatUsd (price: decimal) =
        let culture = CultureInfo.GetCultureInfo("en-US").Clone() :?> CultureInfo
        culture.NumberFormat.CurrencyDecimalDigits <- 3
        price.ToString("C", culture)

    let formatEur (price: decimal) =
        let culture = CultureInfo.GetCultureInfo("en-US").Clone() :?> CultureInfo
        culture.NumberFormat.CurrencySymbol <- "€"
        culture.NumberFormat.CurrencyDecimalDigits <- 3
        price.ToString("C", culture)

module Json =
    let readFile path = File.ReadAllText path

    let parsePortfolio (item: string) =
        JsonSerializer.Deserialize<Asset list> item

module File =
    let commandlinePath () =
        let commandLineArgs = Environment.GetCommandLineArgs()

        let inputJson =
            match commandLineArgs |> Array.tryItem 1 with
            | Some p -> Ok p
            | None -> Error(exn "Please provide path to json file")

        inputJson


module Logging =
    let factory = LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)
    let getLogger category = factory.CreateLogger category

module Quotes =
    [<Literal>]
    let sample = "https://query1.finance.yahoo.com/v8/finance/chart/EURUSD=X"

    type Data = JsonProvider<sample>

    let getQuote (symbol: string) =
        match symbol with
        | "USD" -> (1.0m, 1.0m) |> Ok
        | _ ->
            try
                let encodedSymbol = HttpUtility.UrlEncode symbol
                let url = $"https://query1.finance.yahoo.com/v8/finance/chart/{encodedSymbol}"
                let data = Data.Load url
                let d0 = data.Chart.Result[0].Meta.RegularMarketPrice
                let d1 = data.Chart.Result[0].Meta.PreviousClose
                (d0, d1) |> Ok
            with ex ->
                ex |> Error


    let getExchangeRate () = getQuote "EURUSD=X"

    let getQuotes (items: Asset list) =
        result {
            let! quotes =
                items
                |> List.map (fun asset -> getQuote asset.Symbol |> Result.map (fun q -> q, asset))
                |> List.sequenceResultM

            return
                quotes
                |> List.map (fun ((q0, q1), asset) ->
                    { Symbol = asset.Symbol
                      Quantity = asset.Quantity
                      Price = q0
                      PriceYesterday = q1
                      PctChange = if q1 = 0m then -1m else q0 / q1 - 1m
                      AbsChange = q0 - q1
                      Value = q0 * asset.Quantity })
        }

module Print =
    let printAssets (assets: QuotedAsset list) =
        let gray = "\x1b[38;5;240m"
        let reset = "\u001b[0m"
        printfn "%24s  %12s  %12s  %12s  %12s" "Symbol" "$ Price" "% Change" "Quantity" "$ Value"

        assets
        |> List.iter (fun asset ->
            let color = if asset.Quantity = 0m then gray else reset

            printfn
                "%s%24s  %12s  %12.1f  %12.2f  %12s%s"
                color
                asset.Symbol
                (Format.formatUsd asset.Price)
                asset.PctChange
                asset.Quantity
                (Format.formatUsd (asset.Price * asset.Quantity))
                reset)

    let printPortfolio (portfolio: Portfolio) =
        printfn "%24s  %12s" "$ Value" "€ Value"
        printfn "%24s  %12s" (Format.formatUsd portfolio.Value) (Format.formatEur portfolio.EurValue)
