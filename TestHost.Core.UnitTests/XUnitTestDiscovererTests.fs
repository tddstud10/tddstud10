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
    |> fun (FilePath p) -> Path.Combine(p, "TestData\\bins\\XUnit20FSPortable\\XUnit20FSPortable.dll")
    |> FilePath

let createDiscoverer() = 
    let td = new XUnitTestDiscoverer()
    let tcs = new ConcurrentBag<DTestCase>()
    td.TestDiscovered 
    |> Observable.map TestPlatformExtensions.toDTestCase
    |> Observable.add tcs.Add
    td, tcs

[<Fact>]
let ``Can run successfully on assemblies with no tests``() = 
    let it, _ = createDiscoverer()
    it.DiscoverTests([ ], testBin, Array.empty<string>)


[<Fact>]
let ``Can discover theory and facts from test assembly``() = 
    let it, tcs = createDiscoverer()
    let td = 
        TestPlatformExtensions.getLocalPath() 
        |> TestPlatformExtensions.loadTestAdapter :?> ITestDiscoverer
    it.DiscoverTests([ td ], testBin, Array.empty<string>)
    let actualTests = 
        tcs
        |> Seq.map (fun t -> t.DisplayName)
        |> Seq.sort
        |> Seq.toList
    Assert.Equal<list<_>>(expectedTests, actualTests)

[<Fact>]
let ``Can ignore discover theory and facts from test assembly``() = 
    let it, tcs = createDiscoverer()
    let td = 
        TestPlatformExtensions.getLocalPath() 
        |> TestPlatformExtensions.loadTestAdapter :?> ITestDiscoverer
    let filteredTestName = "XUnit20FSPortable.UnitTests.Theory Tests"
   
    it.DiscoverTests([ td ], testBin, [|filteredTestName|])
    
    let filteredTests = expectedTests |> List.filter (fun f -> not(f.StartsWith(filteredTestName)))
    let actualTests = 
        tcs
        |> Seq.map (fun t -> t.DisplayName)
        |> Seq.sort
        |> Seq.toList
    Assert.Equal<list<_>>(filteredTests , actualTests)
