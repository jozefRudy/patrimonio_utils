module JsonTests

open Xunit
open Portfolio.Json
open Swensen.Unquote

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
