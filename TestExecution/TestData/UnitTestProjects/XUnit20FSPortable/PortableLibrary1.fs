module XUnit20FSPortable.UnitTests

open Xunit

[<Theory>]
[<InlineData(1)>]
[<InlineData(2)>]
let ``Theory Tests`` input =
    Assert.Equal(1, input)

[<Fact>]
let ``Fact Test 1`` () =
    Assert.Equal(1, 1)

[<Fact>]
let ``Fact Test 2`` () =
    Assert.Equal(1, 2)
