module R4nd0mApps.TddStud10.TestExecution.Adapters.XUnitTestDiscovererTests

open System.Collections.Concurrent
open Xunit
open System.IO
open System
open System.Reflection
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain

let expectedTests = 
    [ "XUnit20FSPortable.UnitTests.Fact Test 1"; "XUnit20FSPortable.UnitTests.Fact Test 2"; 
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 1)"; "XUnit20FSPortable.UnitTests.Theory Tests(input: 2)" ]

let testBin = 
    (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName
    |> fun p -> Path.Combine(p, "TestData\\bins\\XUnit20FSPortable\\XUnit20FSPortable.dll")
    |> FilePath

let createDiscoverer() = 
    let td = new XUnitTestDiscoverer()
    let tcs = new ConcurrentBag<DTestCase>()
    td.TestDiscovered |> Observable.add tcs.Add
    td, tcs

[<Fact>]
let ``Can discover theory and facts from test assembly``() = 
    let td, tcs = createDiscoverer()
    testBin |> td.DiscoverTests
    let actualTests = 
        tcs
        |> Seq.map (fun t -> t.DisplayName)
        |> Seq.sort
        |> Seq.toList
    Assert.Equal<list<_>>(expectedTests, actualTests)
