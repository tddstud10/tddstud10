module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.TestMarkerTagger

open Xunit
open R4nd0mApps.TddStud10.Engine.Core
open R4nd0mApps.TddStud10.Common.Domain
open System
open R4nd0mApps.TddStud10.Common.TestFramework
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Tagging

let createRSS slnPath tpa = 
    let rd = RunExecutor.makeRunData DateTime.Now (FilePath slnPath)
    { name = RunStepName "Discover Unit Tests"
      kind = Test
      status = Succeeded
      addendum = FreeFormatData ""
      runData = { rd with testsPerAssembly = Some tpa } }

let createTMT p t = 
    let ds = DataStore() :> IDataStore
    let tb = FakeTextBuffer(p, t) :> ITextBuffer
    let tmt = TestMarkerTagger(tb, ds) :> ITagger<_>
    let spy = CallSpy1<SnapshotSpanEventArgs>(Throws(new Exception()))
    tmt.TagsChanged.Add(spy.Func >> ignore)
    ds, tb, tmt, spy

let createTPA (ts : (string * string * int) seq) = 
    let tpa = PerAssemblyTestCases()
    
    let addTestCase (acc : PerAssemblyTestCases) (f, d, l) = 
        let tc = TestCase(f, Uri("exec://utf"), "src")
        tc.CodeFilePath <- d
        tc.LineNumber <- l
        let b = acc.GetOrAdd(FilePath "src", fun _ -> ConcurrentBag<_>())
        b.Add(tc) |> ignore
        acc
    ts |> Seq.fold addTestCase tpa

[<Fact>]
let ``Datastore TestCasesUpdated event fires TagsChanged event``() = 
    let ds, tb, _, s = createTMT "" ""
    []
    |> createTPA
    |> createRSS @"c:\a.sln"
    |> ds.UpdateData
    Assert.True(s.CalledWith |> Option.exists (fun ssea -> ssea.Span.Snapshot.Equals(tb.CurrentSnapshot)))

[<Fact>]
let ``GetTags returns right tag for 1 empty and 1 each found and not found in datastore``() = 
    let ds, tb, tmt, _ = createTMT @"c:\sln\proj\a.cs" """
2nd line (first is empty)
3rd line
"""
    let ss = tb.CurrentSnapshot.Lines |> Seq.map (fun l -> l.Extent)
    [ ("FQN:2nd line", @"c:\sln\proj\a.cs", 2) ]
    |> createTPA
    |> createRSS @"c:\sln\sln.sln"
    |> ds.UpdateData
    let ts = tmt.GetTags(NormalizedSnapshotSpanCollection(ss))
    Assert.Equal([| "FQN:2nd line" |], ts |> Seq.map (fun t -> t.Tag.testCase.FullyQualifiedName))
