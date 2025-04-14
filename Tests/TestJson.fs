module JsonTests

open Xunit
open Portfolio
open Portfolio.Json
open Swensen.Unquote
open System
open System
open Xunit

type Serialization(logger: ITestOutputHelper) =

    [<Fact>]
    let ``test json`` () =
        let item =
            """
    [
        {
            "Symbol": "USD",
            "Quantity": 2
        }
    ]       
        """

        let portfolio = parsePortfolio item
        test <@ portfolio.Length = 1 @>
        test <@ portfolio.Head.Symbol = "USD" @>
        test <@ portfolio.Head.Quantity = 2.0M @>

    [<Fact>]
    let ``serialize portfolio`` () =
        let portfolio =
            { Date = DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
              ValueEur = 100.1321M
              ValueUsd = 111.21312M }

        let json = Portfolio.toJson portfolio
        logger.WriteLine json
        Assert.True true
