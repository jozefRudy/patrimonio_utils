module Library.CurrentAccount

open System
open System.IO
open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open FsToolkit.ErrorHandling
open Portfolio.Format

let getUrl since until token =
    $"v1/rest/periods/{token}/{since}/{until}/transactions.json"

let formatDate (dt: DateOnly) = dt.ToString "yyyy-MM-dd"

type Info =
    { [<JsonPropertyName("iban")>]
      Iban: string
      [<JsonPropertyName("closingBalance")>]
      Balance: decimal
      [<JsonPropertyName("currency")>]
      Currency: string }

type Transaction =
    { Id: int64
      Date: string
      Amount: decimal
      Currency: string
      UserIdentification: string
      Comment: string }

type ColumnValue<'T> = { value: 'T; name: string; id: int }

type JsonTransaction =
    { [<JsonPropertyName("column22")>]
      id: ColumnValue<int64>
      [<JsonPropertyName("column0")>]
      date: ColumnValue<string>
      [<JsonPropertyName("column1")>]
      amount: ColumnValue<decimal>
      [<JsonPropertyName("column14")>]
      currency: ColumnValue<string>
      [<JsonPropertyName("column7")>]
      userIdentification: ColumnValue<string>
      [<JsonPropertyName("column25")>]
      comment: ColumnValue<string> }

module JsonTransaction =
    let safeExtractValue<'T> (column: ColumnValue<'T>) defaultVal =
        match Option.ofObj column with
        | None -> defaultVal
        | Some column -> column.value

    let toTransaction (json: JsonTransaction) : Transaction =
        { Id = json.id.value
          Date = json.date.value
          Amount = json.amount.value
          Currency = json.currency.value
          UserIdentification = safeExtractValue json.userIdentification ""
          Comment = safeExtractValue json.comment "" }

type FioData =
    { accountStatement:
        {| info: Info
           transactionList: {| transaction: JsonTransaction array |} |} }

type FioClient(client: HttpClient) =
    member this.Get(token: string) =
        taskResult {

            let until = DateOnly.FromDateTime DateTime.UtcNow
            let since = until.AddDays -2

            let url = getUrl (formatDate since) (formatDate until) token

            try
                let! response = client.GetStringAsync url
                let result = JsonSerializer.Deserialize<FioData> response

                return
                    result.accountStatement.info,
                    result.accountStatement.transactionList.transaction
                    |> Array.map JsonTransaction.toTransaction
            with ex ->
                return! ex |> Error
        }

module Print =
    let newLine () = printf "\n"

    let printCurrency (currency: string) balance =
        match currency.ToLowerInvariant() with
        | "eur" -> formatEur balance
        | "usd" -> formatUsd balance
        | _ -> "Error"

    let printInfo (account: Info) =
        printfn "%24s %12s" "Iban" "Balance"
        printfn "%24s %12s" account.Iban (printCurrency account.Currency account.Balance)

    let printTransactions (transactions: Transaction array) =
        if Seq.length transactions > 0 then
            printfn "%15s %12s %68s" "Date" "Amount" "Comment"

            transactions
            |> Array.sortByDescending _.Id
            |> Array.iter (fun x ->
                printfn
                    "%15s %12s %68s"
                    x.Date
                    (printCurrency x.Currency x.Amount)
                    (x.Comment.Substring(0, min 67 x.Comment.Length)))

            let currency = transactions |> Seq.head |> _.Currency
            printfn "%15s %12s" "Total" (transactions |> Seq.sumBy _.Amount |> (fun x -> printCurrency currency x))

type Token =
    { [<JsonPropertyName("token")>]
      Token: string }

type TokenFile = Token array

let loadJsonFromFile (filePath: string) =
    result {
        try
            let json = File.ReadAllText filePath
            return JsonSerializer.Deserialize<TokenFile> json
        with ex ->
            return! ex |> Error
    }
