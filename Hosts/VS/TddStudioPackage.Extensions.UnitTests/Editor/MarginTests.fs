module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.Margin

open Xunit
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open Microsoft.VisualStudio.Text.Editor
open System
open System.Windows
open R4nd0mApps.TddStud10.Common.TestFramework

let createMargin2 p ls = 
    let tv = new StubWpfTextView(p, ls)
    let ta = new StubTagAggregator<_>()
    let s = new CallSpy2<_, _>()
    let m = new Margin(tv, ta, s.Func, (fun () -> Double.MaxValue), (fun () -> null))
    m, tv, ta, s

let createMargin() = createMargin2 (new Point(0.0, 0.0)) (new StubWpfTextViewLineCollection())

let createPainterArgs() = 
    let lines = new StubWpfTextViewLineCollection() :> IWpfTextViewLineCollection
    let loc = new Point(Double.MinValue, Double.MaxValue)
    lines, loc

[<Fact>]
let ``Basic properties and simple methods work as expected``() = 
    let m, _, _, _ = createMargin()
    Assert.Equal(true, (m :> ITextViewMargin).Enabled)
    Assert.Equal(m :> ITextViewMargin, (m :> ITextViewMargin).GetTextViewMargin(MarginConstants.Name))
    Assert.Equal(null, (m :> ITextViewMargin).GetTextViewMargin("Random Margin Name"))
    Assert.Equal(Double.MaxValue, (m :> ITextViewMargin).MarginSize)
    Assert.Equal(null, (m :> IWpfTextViewMargin).VisualElement)

[<Fact>]
let ``Basic properties and simple methods throw if object is disposed``() = 
    let m, _, _, _ = createMargin()
    (m :> IDisposable).Dispose()
    Assert.Throws<ObjectDisposedException>(fun () -> (m :> ITextViewMargin).Enabled |> ignore) |> ignore
    Assert.Throws<ObjectDisposedException>
        (fun () -> (m :> ITextViewMargin).GetTextViewMargin(MarginConstants.Name) |> ignore) |> ignore
    Assert.Throws<ObjectDisposedException>
        (fun () -> (m :> ITextViewMargin).GetTextViewMargin("Random Margin Name") |> ignore) |> ignore
    Assert.Throws<ObjectDisposedException>(fun () -> (m :> ITextViewMargin).MarginSize |> ignore) |> ignore
    Assert.Throws<ObjectDisposedException>(fun () -> (m :> IWpfTextViewMargin).VisualElement |> ignore) |> ignore

[<Fact>]
let ``Painter is called on LayoutChanged event``() = 
    let lines, loc = createPainterArgs()
    let _, tv, _, s = createMargin2 loc lines
    tv.FireLayoutChangedEvent()
    Assert.True(s.CalledWith |> Option.exists (fun (p, ls) -> p.Equals(box loc) && ls = lines))

[<Fact>]
let ``Painter is called on TagsChanged event``() = 
    let lines, loc = createPainterArgs()
    let _, _, ta, s = createMargin2 loc lines
    ta.FireTagsChangedEvent()
    Assert.True(s.CalledWith |> Option.exists (fun (p, ls) -> p.Equals(box loc) && ls = lines))

[<Fact>]
let ``Painter is not called if object is disposed``() = 
    let lines, loc = createPainterArgs()
    let m, tv, ta, s = createMargin2 loc lines
    (m :> IDisposable).Dispose()
    tv.FireLayoutChangedEvent()
    ta.FireTagsChangedEvent()
    Assert.Equal(None, s.CalledWith)
