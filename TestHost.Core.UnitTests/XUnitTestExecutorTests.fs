module R4nd0mApps.TddStud10.TestExecution.Adapters.XUnitTestExecutorTests

open FSharp.Configuration
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.TestExecution
open System.Collections.Concurrent
open System.IO
open System.Runtime.Serialization
open System.Xml
open Xunit
open R4nd0mApps.TddStud10.Common.Domain
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type ResX = ResXProvider< file="Resources\Resources.resx" >

let expectedTests = 
    [ "XUnit20FSPortable.UnitTests.Fact Test 1", TOPassed
      "XUnit20FSPortable.UnitTests.Fact Test 2", TOFailed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 1)", TOPassed
      "XUnit20FSPortable.UnitTests.Theory Tests(input: 2)", TOFailed ]

let createExecutor() = 
    let te = new XUnitTestExecutor()
    let trs = new ConcurrentQueue<DTestResult>()
    te.TestExecuted |> Observable.add trs.Enqueue
    te, trs

let rehydrateTestCases tcs = 
    let serializer = new DataContractSerializer(typeof<TestCase []>)
    use sr = new StringReader(tcs)
    use xtr = new XmlTextReader(sr)
    serializer.ReadObject(xtr) :?> TestCase []

[<Fact>]
let ``Can run successfully on assemblies with no tests``() = 
    let it, _ = createExecutor()
    it.ExecuteTests([ ], [||])

[<Fact>]
let ``Can run re-hydrated tests``() = 
    let it, tos = createExecutor()
    let tests = 
        rehydrateTestCases 
            (ResX.Resources.XUnit20FSPortableTests.Replace
                 (@"D:\src\r4nd0mapps\tddstud10\TestHost.Core.UnitTests\bin\Debug", 
                  TestPlatformExtensions.getLocalPath().ToString()))
    let te = 
        TestPlatformExtensions.getLocalPath() 
        |> TestPlatformExtensions.loadTestAdapter :?> ITestExecutor
    it.ExecuteTests([ te ], tests)
    let actualTests = 
        tos
        |> Seq.map (fun t -> t.DisplayName, t.Outcome)
        |> Seq.sortBy fst
        |> Seq.toList
    Assert.Equal<list<_>>(expectedTests, actualTests)
