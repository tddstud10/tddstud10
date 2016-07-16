module R4nd0mApps.TddStud10.TestExecution.Adapters.XUnitTestDiscovererTests

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.TestExecution
open System.Collections.Concurrent
open System.IO
open Xunit
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

let expectedTests = 
    [ "XUnit20FSPortable.UnitTests.Fact Test 1"; "XUnit20FSPortable.UnitTests.Fact Test 2"; 
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 1)"; "XUnit20FSPortable.UnitTests.Theory Tests(input: 2)" ]

let testBin = 
    ()
    |> TestPlatformExtensions.getLocalPath
    |> fun p -> Path.Combine(p, "TestData\\bins\\XUnit20FSPortable\\XUnit20FSPortable.dll")
    |> FilePath

let createDiscoverer() = 
    let td = new XUnitTestDiscoverer()
    let tcs = new ConcurrentBag<DTestCase>()
    td.TestDiscovered 
    |> Observable.map TestPlatformExtensions.toDTestCase
    |> Observable.add tcs.Add
    td, tcs

[<Fact>]
let ``Can discover theory and facts from test assembly``() = 
    let it, tcs = createDiscoverer()
    let td = 
        TestPlatformExtensions.getLocalPath() 
        |> TestPlatformExtensions.loadTestAdapter :?> ITestDiscoverer
    it.DiscoverTests([ td ], testBin)
    let actualTests = 
        tcs
        |> Seq.map (fun t -> t.DisplayName)
        |> Seq.sort
        |> Seq.toList
    Assert.Equal<list<_>>(expectedTests, actualTests)
