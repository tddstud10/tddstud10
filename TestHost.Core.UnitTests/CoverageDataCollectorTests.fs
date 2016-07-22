module R4nd0mApps.TddStud10.TestHost.CoverageDataCollectorTests

open Xunit
open R4nd0mApps.TddStud10.TestRuntime
open System
open R4nd0mApps.TddStud10.Common.Domain
open System.Collections.Generic

let asmId1 = Guid.NewGuid()
let asmId2 = Guid.NewGuid()

type FakeCallContext() = 
    let mutable data = null
    member __.GetData _ = data
    member __.SetData _ o = data <- o

let createTRIDs ts = 
    [ for (s, d, l) in ts do
          yield { testId = 
                      { source = s |> FilePath
                        location = { document = d |> FilePath
                                     line = l |> DocumentCoordinate } }
                  testRunInstanceId = obj().GetHashCode() |> TestRunInstanceId } ]

let createSPID a m s =
    { methodId = { assemblyId = a |> AssemblyId; mdTokenRid = m |> MdTokenRid }; uid = s }

type Assert with
    static member MatchTRIDs (e : TestRunId seq) (a : TestRunId seq) = 
        let spcComp = 
            { new IEqualityComparer<TestRunId> with
                  member __.Equals(tr1 : _, tr2 : _) : bool = 
                      tr1.testId = tr2.testId 
                      && tr1.testRunInstanceId <> tr2.testRunInstanceId 
                      && tr1.testRunInstanceId <> (TestRunInstanceId 0) 
                      && tr2.testRunInstanceId <> (TestRunInstanceId 0)
                  member __.GetHashCode(_ : _) : int = failwith "Not implemented yet" }
        Assert.Equal(e |> Seq.length, a |> Seq.length)
        e |> Seq.iter (fun e -> Assert.Contains(e, a, spcComp))

type DebuggerBehavior = 
    | NotAttached
    | Attached

let createCDC() = 
    let cdc = CoverageDataCollector()
    let fcc = FakeCallContext()
    let m = 
        Marker
            (Func<ICoverageDataCollector>(fun () -> upcast cdc), false, 
             Func<string, obj>(fcc.GetData), Action<string, obj>(fcc.SetData))
    cdc, m

let createCDC2 cdc db = 
    let fcc = FakeCallContext()
    Marker
        (Func<ICoverageDataCollector>(fun () -> cdc), (db = Attached), 
            Func<string, obj>(fcc.GetData), Action<string, obj>(fcc.SetData))

[<Fact>]
let ``Same test - Sequence points in same assembly``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "101", "2")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Assert.Equal(2, cdc.CoverageData.Keys.Count)
    let expected = createTRIDs [ ("a.dll", "a.cs", 100) ]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId1, 100u, 1) |||> createSPID]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId1, 101u, 2) |||> createSPID]

[<Fact>]
let ``Same test - Sequence points in different assemblies``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "200", "11")
    m.RegisterEnterSequencePoint(asmId2.ToString(), "201", "21")
    m.RegisterExitUnitTest("b.dll", "b.cs", "500")
    Assert.Equal(2, cdc.CoverageData.Keys.Count)
    let expected = createTRIDs [ ("b.dll", "b.cs", 500) ]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId1, 200u, 11) |||> createSPID]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId2, 201u, 21) |||> createSPID]

[<Fact>]
let ``Same test - different runs should created unique runinstanceids``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    let sps = 
        (cdc.CoverageData.Values
         |> Seq.collect id
         |> Array.ofSeq)
    Assert.NotEqual(sps.[0].testRunInstanceId, sps.[1].testRunInstanceId)

[<Fact>]
let ``Different test - coverage data collected for both tests``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "500", "100")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    m.RegisterEnterSequencePoint(asmId2.ToString(), "600", "200")
    m.RegisterExitUnitTest("b.dll", "b.cs", "200")
    Assert.Equal(2, cdc.CoverageData.Keys.Count)
    let expected = createTRIDs [ ("a.dll", "a.cs", 100) ]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId1, 500u, 100) |||> createSPID]
    let expected = createTRIDs [ ("b.dll", "b.cs", 200) ]
    Assert.MatchTRIDs expected cdc.CoverageData.[(asmId2, 600u, 200) |||> createSPID]

type CdcMock() =
    interface ICoverageDataCollector with
        member x.EnterSequencePoint(_: string, _: string, _: string, _: string): unit = 
            x.EnterSPCalled <- true
        member x.ExitUnitTest(_: string, _: string, _: string, _: string): unit = 
            x.ExitUTCalled <- true
        member __.Ping(): unit = 
            failwith "Not implemented yet"
    member val EnterSPCalled = false with get, set
    member val ExitUTCalled = false with get, set


[<Fact>]
let ``With debugger attached coverage info does not get registered``() = 
    let cdc = CdcMock()
    let m = createCDC2 cdc Attached
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Assert.False(cdc.EnterSPCalled)
    Assert.False(cdc.ExitUTCalled)

[<Fact>]
let ``If sequence points not registered, exit unit test is ignored``() = 
    let cdc, m = createCDC()
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Assert.Equal(0, cdc.CoverageData.Keys.Count)

[<Fact>]
let ``If any of EnterSequencePoint parameters are null - no coverage data gets collected``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(null, "100", "1")
    m.RegisterEnterSequencePoint(asmId1.ToString(), null, "1")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", null)
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Assert.Equal(0, cdc.CoverageData.Keys.Count)

[<Fact>]
let ``If any of ExitUnitTest parameters are null - no coverage data gets collected``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest(null, "a.cs", "100")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", null, "100")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", "a.cs", null)
    Assert.Equal(0, cdc.CoverageData.Keys.Count)
