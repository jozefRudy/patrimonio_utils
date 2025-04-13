open FsToolkit.ErrorHandling.Operator.Result
open Library
open Portfolio
open Portfolio.Json
open Microsoft.Extensions.Logging
open FsToolkit.ErrorHandling
open CurrentAccount

[<EntryPoint>]
let main argv =
    File.commandlinePath ()
    |> Result.map loadPortfolioFromFile
    |> Result.map Print.printAssets
    |> Result.tee (fun _ -> Print.newLine ())
    |> Result.tee Print.printPortfolio
    |> Result.mapError (fun x -> Logging.getLogger("Portfolio").LogError(x, "Failed to process portfolio"))
    |> ignore

    0