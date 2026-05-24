module AccountTests

open Xunit
open Library.CurrentAccount
open System.IO
open System

type PrintTests(outputHelper: ITestOutputHelper) =

    [<Fact>]
    let ``printTransactions shows Type column`` () =
        let transactions = [|
            { Id = 1L
              Date = "2024-01-15"
              Amount = 123.45M
              Currency = "EUR"
              UserIdentification = ""
              Comment = "Grocery store"
              Type = "Card payment" }
            { Id = 2L
              Date = "2024-01-14"
              Amount = -50.00M
              Currency = "EUR"
              UserIdentification = ""
              Comment = "ATM withdrawal"
              Type = "ATM" }
        |]

        let writer = new StringWriter()
        let oldOut = Console.Out
        Console.SetOut writer
        Print.printTransactions transactions
        Console.SetOut oldOut

        let output = writer.ToString()
        outputHelper.WriteLine(output)
        Assert.Contains("Type", output)
        Assert.Contains("Card payment", output)
        Assert.Contains("ATM", output)
        let lines = output.Split('\n')
        let dataLine = lines |> Array.find (fun l -> l.Contains("Card payment"))
        Assert.Contains("Grocery store", dataLine)
        Assert.Contains("Card payment", dataLine)

type TransactionMappingTests() =

    [<Fact>]
    let ``toTransaction maps typ to Type`` () =
        let json : JsonTransaction =
            { id = { value = 42L; name = "ID"; id = 22 }
              date = { value = "2024-05-01"; name = "Date"; id = 0 }
              amount = { value = 100M; name = "Amount"; id = 1 }
              currency = { value = "EUR"; name = "Currency"; id = 14 }
              userIdentification = { value = "user123"; name = "User"; id = 7 }
              comment = { value = "test"; name = "Comment"; id = 25 }
              typ = { value = "Transfer"; name = "Type"; id = 12 } }

        let tx = JsonTransaction.toTransaction json
        Assert.Equal("Transfer", tx.Type)
