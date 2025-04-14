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
open System.Text.Json.Serialization

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
    { ValueUsd: decimal
      ValueEur: decimal
      Date: DateTimeOffset }

module Portfolio =

    let fromItems (items: QuotedAsset seq) (exchangeRate: decimal) (date: DateTimeOffset) =
        let value = items |> Seq.sumBy _.Value
        let eurValue = value / exchangeRate

        { ValueUsd = value
          ValueEur = value / exchangeRate
          Date = date }

    let toJson (portfolio: Portfolio) =
        let options = JsonSerializerOptions()
        options.WriteIndented <- true
        options.IndentSize <- 4
        JsonSerializer.Serialize(portfolio, options)

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
    open FsToolkit.ErrorHandling
    open System.Threading.Tasks
    open FsToolkit.ErrorHandling

    [<Literal>]
    let sample = "https://query1.finance.yahoo.com/v8/finance/chart/EURUSD=X"

    type Data = JsonProvider<sample>

    let getQuote (symbol: string) =
        asyncResult {
            match symbol with
            | "USD" -> return 1.0m, 1.0m
            | _ ->
                try
                    let encodedSymbol = HttpUtility.UrlEncode symbol
                    let url = $"https://query1.finance.yahoo.com/v8/finance/chart/{encodedSymbol}"
                    let! data = Data.AsyncLoad url
                    let d0 = data.Chart.Result[0].Meta.RegularMarketPrice
                    let d1 = data.Chart.Result[0].Meta.PreviousClose
                    return d0, d1
                with ex ->
                    return! ex |> Error

        }


    let getExchangeRate () = getQuote "EURUSD=X"


    let getQuotes (items: Asset array) =
        taskResult {
            let! quotes =
                items
                |> Seq.traverseAsyncResultM (fun asset -> getQuote asset.Symbol |> AsyncResult.map (fun q -> q, asset))

            return
                quotes
                |> Seq.map (fun ((q0, q1), asset) ->
                    { Symbol = asset.Symbol
                      Quantity = asset.Quantity
                      Price = q0
                      PriceYesterday = q1
                      PctChange = if q1 = 0m then -1m else q0 / q1 - 1m
                      AbsChange = q0 - q1
                      Value = q0 * asset.Quantity })
        }

module Print =
    let printAssets (assets: QuotedAsset seq) =
        let gray = "\x1b[38;5;240m"
        let reset = "\u001b[0m"
        printfn "%24s  %12s  %12s  %12s  %12s" "Symbol" "$ Price" "% Change" "Quantity" "$ Value"

        assets
        |> Seq.iter (fun asset ->
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
        printfn "%24s  %12s" (Format.formatUsd portfolio.ValueUsd) (Format.formatEur portfolio.ValueEur)
