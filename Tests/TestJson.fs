module JsonTests

open Xunit
open Portfolio.Json

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
    Assert.True(portfolio.Length = 1)
    Assert.True(portfolio.Head.Symbol = "USD")
    Assert.True(portfolio.Head.Quantity = 2.0M)
