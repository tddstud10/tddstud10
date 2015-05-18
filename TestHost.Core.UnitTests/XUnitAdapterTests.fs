module R4nd0mApps.TddStud10.TestExecution.Adapters.XUnitAdapterTests

open System.Collections.Generic
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.TestExecution.Adapters.Execution
open Xunit
open System.IO
open System
open System.Reflection
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.TestExecution.Adapters.Discovery

let expectedTests = 
    [ "XUnit20FSPortable.UnitTests.Fact Test 1", TestOutcome.Passed
      "XUnit20FSPortable.UnitTests.Fact Test 2", TestOutcome.Failed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 1)", TestOutcome.Passed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 2)", TestOutcome.Failed ]

let testBin = 
    (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName
    |> fun p -> Path.Combine(p, "TestData\\bins\\XUnit20FSPortable\\XUnit20FSPortable.dll")

let createDiscoverer() = 
    let td = new XUnitTestDiscoverer()
    let tcs = new List<TestCase>()
    td.TestDiscovered |> Observable.add tcs.Add
    td, tcs

let createExecutor() = 
    let te = new XUnitTestExecutor()
    let trs = new ConcurrentQueue<TestResult>()
    te.TestExecuted |> Observable.add trs.Enqueue
    te, trs

[<Fact>]
let ``Can discover and run theory and facts from test assembly``() = 
    let td, tcs = createDiscoverer()
    testBin |> td.DiscoverTests
    let te, tos = createExecutor()
    tcs |> te.ExecuteTests
    let actualTests = 
        tos
        |> Seq.map (fun t -> t.DisplayName, t.Outcome)
        |> Seq.sortBy fst
        |> Seq.toList
    Assert.Equal<list<string * TestOutcome>>(expectedTests, actualTests)
