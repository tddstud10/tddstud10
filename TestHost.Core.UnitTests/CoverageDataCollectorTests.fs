module R4nd0mApps.TddStud10.TestHost.CoverageDataCollectorTests

open Xunit
open R4nd0mApps.TddStud10.TestRuntime
open System
open R4nd0mApps.TddStud10.Common.Domain
open System.Collections.Generic
open Foq

let asmId1 = Guid.NewGuid()
let asmId2 = Guid.NewGuid()

type FakeCallContext() = 
    let mutable data = null
    member __.GetData n = data
    member __.SetData n o = data <- o

let createCoverageData (s, d, l) ss = 
    [ for (a, m, sp) in ss do
          yield { methodId = 
                      { assemblyId = a |> AssemblyId
                        mdTokenRid = m |> MdTokenRid }
                  sequencePointId = sp |> SequencePointId
                  testRunId = 
                      { testId = 
                            { source = s |> FilePath
                              document = d |> FilePath
                              line = l |> DocumentCoordinate }
                        testRunInstanceId = 0xbadf00d |> TestRunInstanceId } } ]

type Assert with
    static member MatchSPC (e : SequencePointCoverage seq) (a : SequencePointCoverage seq) = 
        let spcComp = 
            { new IEqualityComparer<SequencePointCoverage> with
                  member __.Equals(s1 : _, s2 : _) : bool = 
                      s1.methodId = s2.methodId && s1.sequencePointId = s2.sequencePointId 
                      && s1.testRunId.testId = s2.testRunId.testId 
                      && s1.testRunId.testRunInstanceId <> s2.testRunId.testRunInstanceId 
                      && s1.testRunId.testRunInstanceId <> (TestRunInstanceId 0) 
                      && s2.testRunId.testRunInstanceId <> (TestRunInstanceId 0)
                  member __.GetHashCode(obj : _) : int = failwith "Not implemented yet" }
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
            (Func<ICoverageDataCollector>(fun () -> upcast cdc), Func<bool>(fun () -> false), 
             Func<string, obj>(fcc.GetData), Action<string, obj>(fcc.SetData))
    cdc, m

let createCDC2 cdc db = 
    let fcc = FakeCallContext()
    let m = 
        Marker
            (Func<ICoverageDataCollector>(fun () -> cdc), Func<bool>(fun () -> db = Attached), 
             Func<string, obj>(fcc.GetData), Action<string, obj>(fcc.SetData))
    m

[<Fact>]
let ``Same test - Sequence points in same assembly``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterEnterSequencePoint(asmId1.ToString(), "101", "2")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Assert.Equal(1, cdc.CoverageData.Keys.Count)
    let expected = 
        createCoverageData ("a.dll", "a.cs", 100) [ (asmId1, 100u, 1)
                                                    (asmId1, 101u, 2) ]
    Assert.MatchSPC expected cdc.CoverageData.[AssemblyId asmId1]

[<Fact>]
let ``Same test - Sequence points in different assemblies``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "200", "11")
    m.RegisterEnterSequencePoint(asmId2.ToString(), "201", "21")
    m.RegisterExitUnitTest("b.dll", "b.cs", "500")
    Assert.Equal(2, cdc.CoverageData.Keys.Count)
    let expected1 = createCoverageData ("b.dll", "b.cs", 500) [ (asmId1, 200u, 11) ]
    Assert.MatchSPC expected1 cdc.CoverageData.[AssemblyId asmId1]
    let expected2 = createCoverageData ("b.dll", "b.cs", 500) [ (asmId2, 201u, 21) ]
    Assert.MatchSPC expected2 cdc.CoverageData.[AssemblyId asmId2]

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
    Assert.NotEqual(sps.[0].testRunId.testRunInstanceId, sps.[1].testRunId.testRunInstanceId)

[<Fact>]
let ``Different test - coverage data collected for both tests``() = 
    let cdc, m = createCDC()
    m.RegisterEnterSequencePoint(asmId1.ToString(), "500", "100")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    m.RegisterEnterSequencePoint(asmId2.ToString(), "600", "200")
    m.RegisterExitUnitTest("b.dll", "b.cs", "200")
    let expected1 = createCoverageData ("a.dll", "a.cs", 100) [ (asmId1, 500u, 100) ]
    Assert.MatchSPC expected1 cdc.CoverageData.[AssemblyId asmId1]
    let expected2 = createCoverageData ("b.dll", "b.cs", 200) [ (asmId2, 600u, 200) ]
    Assert.MatchSPC expected2 cdc.CoverageData.[AssemblyId asmId2]

[<Fact>]
let ``With debugger attached coverage info does not get registered``() = 
    let cdc = Mock.Of<ICoverageDataCollector>()
    let m = createCDC2 cdc Attached
    m.RegisterEnterSequencePoint(asmId1.ToString(), "100", "1")
    m.RegisterExitUnitTest("a.dll", "a.cs", "100")
    Mock.Verify(<@ cdc.EnterSequencePoint(any(), any(), any(), any()) @>, never)
    Mock.Verify(<@ cdc.ExitUnitTest(any(), any(), any(), any()) @>, never)

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
