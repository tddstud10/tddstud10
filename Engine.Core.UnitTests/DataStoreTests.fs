module R4nd0mApps.TddStud10.Engine.Core.DataStoreTests

open Xunit
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework
open System
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Common

let createDS slnPath = 
    let ds = DataStore() :> IDataStore
    RunStartParamsExtensions.create DateTime.Now (FilePath slnPath) |> ds.UpdateRunStartParams
    ds

let createDSWithPATC slnPath = 
    let ds = createDS slnPath
    let spy = CallSpy<PerAssemblyTestCases>(Throws(Exception()))
    ds.TestCasesUpdated.Add(spy.Func >> ignore)
    ds, spy

let createDSWithPDSP slnPath = 
    let ds = createDS slnPath
    let spy = CallSpy<PerDocumentSequencePoints>(Throws(Exception()))
    ds.SequencePointsUpdated.Add(spy.Func >> ignore)
    ds, spy

let createDSWithTRO slnPath = 
    let ds = createDS slnPath
    let spy1 = CallSpy<PerTestIdResults>(Throws(Exception()))
    ds.TestResultsUpdated.Add(spy1.Func >> ignore)
    let spy2 = CallSpy<PerDocumentLocationTestFailureInfo>(Throws(Exception()))
    ds.TestFailureInfoUpdated.Add(spy2.Func >> ignore)
    let spy3 = CallSpy<PerAssemblySequencePointsCoverage>(Throws(Exception()))
    ds.CoverageInfoUpdated.Add(spy3.Func >> ignore)
    ds, spy1, spy2, spy3

let createPATC (ts : (string * string * string * int) list) = 
    let patc = PerAssemblyTestCases()
    
    let addTestCase (acc : PerAssemblyTestCases) (s, f, d, l) = 
        let tc = TestCase(f, Uri("exec://utf"), s)
        tc.CodeFilePath <- d
        tc.LineNumber <- l
        let b = acc.GetOrAdd(FilePath s, fun _ -> ConcurrentBag<_>())
        b.Add(tc) |> ignore
        acc
    ts |> Seq.fold addTestCase patc

let createPDSP() = PerDocumentSequencePoints()
let createTRO() = PerTestIdResults(), PerDocumentLocationTestFailureInfo(), PerAssemblySequencePointsCoverage()

[<Fact>]
let ``UpdateData with PATV causes event to be fired and crash in handler is ignored``() = 
    let ds, spy = createDSWithPATC @"c:\a.sln"
    let patc = [] |> createPATC
    ds.UpdateData(patc |> TestCases)
    Assert.Equal(spy.CalledWith, Some patc)

[<Fact>]
let ``UpdateData with PDSP causes event to be fired and crash in handler is ignored``() = 
    let ds, spy = createDSWithPDSP @"c:\a.sln"
    let pdsp = () |> createPDSP
    ds.UpdateData(pdsp |> SequencePoints)
    Assert.Equal(spy.CalledWith, Some pdsp)

[<Fact>]
let ``UpdateData with TRO causes event to be fired and crash in handler is ignored``() = 
    let ds, spy1, spy2, spy3 = createDSWithTRO @"c:\a.sln"
    let ptir, pdtfi, paspc = () |> createTRO
    ds.UpdateData((ptir, pdtfi, paspc) |> TestRunOutput)
    Assert.Equal(spy1.CalledWith, Some ptir)
    Assert.Equal(spy2.CalledWith, Some pdtfi)
    Assert.Equal(spy3.CalledWith, Some paspc)

[<Fact>]
let ``FindTest2 returns None if it cannot find a match``() = 
    let ds, _ = createDSWithPATC @"c:\a.sln"
    let ts = ds.FindTest2 (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Empty(ts)

[<Fact>]
let ``FindTest2 returns TestCase if it can find a match``() = 
    let ds, _ = createDSWithPATC @"c:\a.sln"
    let patc = [ (@"c:\adll.dll", "FQN#1", @"c:\a.cs", 10) ] |> createPATC
    ds.UpdateData(patc |> TestCases)
    let ts = ds.FindTest2 (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Equal([| "FQN#1" |], ts |> Seq.map (fun t -> t.FullyQualifiedName))

[<Fact>]
let ``FindTest2 returns both test cases if matching testid exist in 2 assemblies``() = 
    let ds, _ = createDSWithPATC @"c:\a.sln"
    
    let patc = 
        [ (@"c:\1.dll", "FQN#1", @"c:\a.cs", 10)
          (@"c:\2.dll", "FQN#1", @"c:\a.cs", 10) ]
        |> createPATC
    ds.UpdateData(patc |> TestCases)
    let ts = ds.FindTest2 (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Equal([| @"c:\1.dll"; @"c:\2.dll" |], 
                 ts
                 |> Seq.map (fun t -> t.Source)
                 |> Seq.sort)
    Assert.Equal([| @"FQN#1"; @"FQN#1" |], 
                 ts
                 |> Seq.map (fun t -> t.FullyQualifiedName)
                 |> Seq.sort)

[<Fact>]
let ``FindTest2 returns TestCase if it an find a match after sln path normalization``() = 
    let ds, _ = createDSWithPATC @"c:\sln\a.sln"
    let patc = [ (@"c:\adll.dll", "FQN#2", PathBuilder.snapShotRoot + @"sln\proj\a.cs", 10) ] |> createPATC
    ds.UpdateData(patc |> TestCases)
    let ts = ds.FindTest2 (FilePath @"c:\sln\proj\a.cs") (DocumentCoordinate 10)
    Assert.Equal([| "FQN#2" |], ts |> Seq.map (fun t -> t.FullyQualifiedName))
