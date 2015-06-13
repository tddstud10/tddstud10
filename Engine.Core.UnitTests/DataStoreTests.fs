module R4nd0mApps.TddStud10.Engine.Core.DataStoreTests

open Xunit
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework
open System
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Concurrent

let createRSS slnPath tpa = 
    let rd = RunExecutor.makeRunData DateTime.Now (FilePath slnPath)
    { name = RunStepName "__"
      kind = Test
      subKind = DiscoverTests
      status = Succeeded
      addendum = FreeFormatData ""
      runData = { rd with testsPerAssembly = Some tpa } }

let createDS() = 
    let ds = new DataStore() :> IDataStore
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
    let ds, spy = createDS()
    let tpa = [] |> createTPA
    ds.UpdateData(tpa |> createRSS @"c:\a.sln")
    Assert.Equal(spy.CalledWith, Some tpa)

[<Fact>]
let ``FindTestByDocumentAndLineNumber returns None if it cannot find a match``() = 
    let ds, _ = createDS()
    let t = ds.FindTestByDocumentAndLineNumber (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Equal(t, None)

[<Fact>]
let ``FindTestByDocumentAndLineNumber returns TestCase if it an find a match``() = 
    let ds, _ = createDS()
    let tpa = [ (@"c:\adll.dll", "FQN#1", @"c:\a.cs", 10) ] |> createTPA
    ds.UpdateData(tpa |> createRSS @"c:\a.sln")
    let t = ds.FindTestByDocumentAndLineNumber (FilePath @"c:\a.cs") (DocumentCoordinate 10)
    Assert.Equal(t |> Option.map (fun t -> t.FullyQualifiedName), Some "FQN#1")

[<Fact>]
let ``FindTestByDocumentAndLineNumber returns TestCase if it an find a match after sln path normalization``() = 
    let ds, _ = createDS()
    let tpa = [ (@"c:\adll.dll", "FQN#2", PathBuilder.snapShotRoot + @"sln\proj\a.cs", 10) ] |> createTPA
    ds.UpdateData(tpa |> createRSS @"c:\sln\a.sln")
    let t = ds.FindTestByDocumentAndLineNumber (FilePath @"c:\sln\proj\a.cs") (DocumentCoordinate 10)
    Assert.Equal(t |> Option.map (fun t -> t.FullyQualifiedName), Some "FQN#2")
