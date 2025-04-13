module Portfolio

open System
open System.IO
open System.Globalization
open System.Web
open FSharp.Data
open Microsoft.Extensions.Logging
open System.Text.Json

type Asset = { Symbol: string; Quantity: decimal }

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


module Quotes =
    [<Literal>]
    let sample = "https://query1.finance.yahoo.com/v8/finance/chart/EURUSD=X"

    type Data = JsonProvider<sample>

    let getQuote (symbol: string) =
        match symbol with
        | "USD" -> (1.0m, 0m) |> Ok
        | _ ->
            try
                let encodedSymbol = HttpUtility.UrlEncode symbol
                let url = $"https://query1.finance.yahoo.com/v8/finance/chart/{encodedSymbol}"
                let data = Data.Load url
                let d0 = data.Chart.Result[0].Meta.RegularMarketPrice
                let d1 = data.Chart.Result[0].Meta.PreviousClose
                (d0, 100.0m * (d0 / d1 - 1m)) |> Ok
            with ex ->
                ex |> Error


module Json =
    let readFile path = File.ReadAllText path

    let parsePortfolio (item: string) =
        JsonSerializer.Deserialize<Asset list> item


module File =
    let commandlinePath () =
        match Environment.GetCommandLineArgs() |> Array.tryItem 1 with
        | Some p -> Ok p
        | None -> Error(exn "Please provide path to json file")


module Logging =
    let factory = LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)
    let getLogger category = factory.CreateLogger category


module Print =
    let printAssets (assets: Asset list) =
        let gray = "\x1b[38;5;240m"
        let reset = "\u001b[0m"
        printfn "%24s  %12s  %12s  %12s  %12s" "Symbol" "$ Price" "% Change" "Quantity" "$ Value"

        assets
        |> List.fold
            (fun total asset ->
                match Quotes.getQuote asset.Symbol with
                | Ok(price, change) ->
                    let value = price * asset.Quantity
                    let color = if asset.Quantity = 0m then gray else reset

                    printfn
                        "%s%24s  %12s  %12.1f  %12.2f  %12s%s"
                        color
                        asset.Symbol
                        (Format.formatUsd price)
                        change
                        asset.Quantity
                        (Format.formatUsd value)
                        reset

                    total + value
                | Error _ ->
                    printfn "%24s  %12.2f  %12s" asset.Symbol asset.Quantity "ERROR"
                    total)
            0m

    let printPortfolio value =
        printfn "%24s  %12s" "$ Value" "€ Value"

        match Quotes.getQuote "EURUSD=X" with
        | Ok(exchangeRate, _) -> printfn "%24s  %12s" (Format.formatUsd value) (Format.formatEur (value / exchangeRate))
        | Error _ -> printfn "%24s  %12s" "ERROR" "ERROR"
