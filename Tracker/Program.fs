open FsToolkit.ErrorHandling.Operator.Result
open Library
open Portfolio
open Portfolio.Json
open Microsoft.Extensions.Logging
open FsToolkit.ErrorHandling
open CurrentAccount
open System

[<EntryPoint>]
let main argv =
    let quotes =
        result {
            let! items = File.commandlinePath () |> Result.map readFile |> Result.map parsePortfolio
            let! quotes = Quotes.getQuotes items
            let! exchangeRate = Quotes.getExchangeRate ()

            let portfolio =
                Portfolio.Portfolio.fromItems quotes (fst exchangeRate) DateTimeOffset.UtcNow

            return quotes, fst exchangeRate, portfolio
        }

    quotes
    |> Result.tee (fun (a, exchRate, portfolio) ->
        Print.printAssets a
        Print.newLine ()
        Print.printPortfolio portfolio)
    |> Result.mapError (fun x -> Logging.getLogger("Portfolio").LogError(x, "Failed to process portfolio"))
    |> ignore

    0
