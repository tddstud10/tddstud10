module R4nd0mApps.TddStud10.Engine.Core.DataStoreTests

open Xunit
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework
open System
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Concurrent

let createDS slnPath = 
    let ds = new DataStore() :> IDataStore
    RunExecutor.createRunStartParams DateTime.Now (FilePath slnPath)
    |> ds.UpdateRunStartParams
    let spy = new CallSpy<PerAssemblyTestCases>(Throws(new Exception()))
    ds.TestCasesUpdated.Add(spy.Func >> ignore)
    ds, spy

let createTPA (ts : (string * string * string * int) list) = 
    let tpa = new PerAssemblyTestCases()
    
    let addTestCase (acc : PerAssemblyTestCases) (s, f, d, l) = 
        let tc = new TestCase(f, new Uri("exec://utf"), s)
        tc.CodeFilePath <- d
        tc.LineNumber <- l
        let b = acc.GetOrAdd(FilePath s, fun _ -> new ConcurrentBag<_>())
        b.Add(tc) |> ignore
        acc
    ts |> Seq.fold addTestCase tpa

[<Fact>]
let ``UpdateData causes event to be fired and crash in handler is ignored``() = 
    let ds, spy = createDS @"c:\a.sln"
    let tpa = [] |> createTPA
    ds.UpdateData(tpa |> TestCases)
    Assert.Equal(spy.CalledWith, Some tpa)

[<Fact>]
let ``FindTest2 returns None if it cannot find a match``() = 
    let ds, _ = createDS @"c:\a.sln"
    let ts = ds.FindTest2 (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Empty(ts)

[<Fact>]
let ``FindTest2 returns TestCase if it can find a match``() = 
    let ds, _ = createDS @"c:\a.sln"
    let tpa = [ (@"c:\adll.dll", "FQN#1", @"c:\a.cs", 10) ] |> createTPA
    ds.UpdateData(tpa |> TestCases)
    let ts = ds.FindTest2 (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Equal([| "FQN#1" |], ts |> Seq.map (fun t -> t.FullyQualifiedName))

[<Fact>]
let ``FindTest2 returns TestCase if it an find a match after sln path normalization``() = 
    let ds, _ = createDS @"c:\sln\a.sln"
    let tpa = [ (@"c:\adll.dll", "FQN#2", PathBuilder.snapShotRoot + @"sln\proj\a.cs", 10) ] |> createTPA
    ds.UpdateData(tpa |> TestCases)
    let ts = ds.FindTest2 (FilePath @"c:\sln\proj\a.cs") (DocumentCoordinate 10)
    Assert.Equal([| "FQN#2" |], ts |> Seq.map (fun t -> t.FullyQualifiedName))
