open System
open Portfolio
open Library.CurrentAccount
open Microsoft.Extensions.DependencyInjection
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Logging

#nowarn "20"


[<EntryPoint>]
let main argv =
    let services = ServiceCollection()

    services.AddLogging(fun logging -> logging.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Warning) |> ignore)
    services.AddHttpClient<FioClient>(fun client -> client.BaseAddress <- Uri("https://fioapi.fio.cz"))

    use sp = services.BuildServiceProvider()
    let loggerFactory = sp.GetRequiredService<ILoggerFactory>()
    let logger = loggerFactory.CreateLogger("FioApp")
    let client = sp.GetRequiredService<FioClient>()


    taskResult {
        let! path = File.commandlinePath ()
        let! accounts = path |> loadJsonFromFile

        for t in accounts do
            let! info, transactions = client.Get t.Token
            Print.printInfo info
            Print.newLine ()
            Print.printTransactions transactions
            Print.newLine ()
            Print.newLine ()
    }
    |> TaskResult.teeError (fun ex -> logger.LogError(ex, "download failed"))
    |> Async.AwaitTask
    |> Async.RunSynchronously

    0
