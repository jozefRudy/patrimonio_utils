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
        asyncResult {
            let! items = File.commandlinePath () |> Result.map readFile |> Result.map parsePortfolio
            let! quotes = Quotes.getQuotes (items |> List.toArray)
            let! exchangeRate = Quotes.getExchangeRate ()

            let portfolio =
                Portfolio.Portfolio.fromItems quotes (fst exchangeRate) DateTimeOffset.UtcNow

            return quotes, portfolio
        }

    quotes
    |> AsyncResult.tee (fun (assets, portfolio) ->
        Print.printAssets assets
        Print.newLine ()
        Print.printPortfolio portfolio
        Print.newLine ())

    |> AsyncResult.mapError (fun x -> Logging.getLogger("Portfolio").LogError(x, "Failed to process portfolio"))
    |> Async.RunSynchronously
    |> ignore

    0
