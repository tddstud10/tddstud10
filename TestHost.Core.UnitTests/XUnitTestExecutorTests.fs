module R4nd0mApps.TddStud10.TestExecution.Adapters.XUnitTestExecutorTests

open System.Collections.Concurrent
open Xunit
open System.IO
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open FSharp.Configuration
open System.Runtime.Serialization
open System.Xml
open System
open System.Reflection

type ResX = ResXProvider< file="Resources\Resources.resx" >

let testBinDir = 
    (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName

let expectedTests = 
    [ "XUnit20FSPortable.UnitTests.Fact Test 1", TestOutcome.Passed
      "XUnit20FSPortable.UnitTests.Fact Test 2", TestOutcome.Failed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 1)", TestOutcome.Passed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 2)", TestOutcome.Failed ]

let createExecutor() = 
    let te = new XUnitTestExecutor()
    let trs = new ConcurrentQueue<TestResult>()
    te.TestExecuted |> Observable.add trs.Enqueue
    te, trs

let rehydrateTestCases tcs = 
    let serializer = new DataContractSerializer(typeof<TestCase []>)
    use sr = new StringReader(tcs)
    use xtr = new XmlTextReader(sr)
    serializer.ReadObject(xtr) :?> TestCase []

[<Fact>]
let ``Can run re-hydrated tests``() = 
    let te, tos = createExecutor()
    let tests = rehydrateTestCases (ResX.Resources.XUnit20FSPortableTests.Replace(@"D:\src\r4nd0mapps\tddstud10\TestHost.Core.UnitTests\bin\Debug", testBinDir))
    tests |> te.ExecuteTests
    let actualTests = 
        tos
        |> Seq.map (fun t -> t.DisplayName, t.Outcome)
        |> Seq.sortBy fst
        |> Seq.toList
    Assert.Equal<list<_>>(expectedTests, actualTests)
