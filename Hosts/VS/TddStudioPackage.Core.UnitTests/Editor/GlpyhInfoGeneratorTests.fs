module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.GlpyhInfoGeneratorTests

open Xunit
open System.Windows
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.TestCommon
open System
open Microsoft.VisualStudio.Text.Tagging
open System.Windows.Media

let stubTC1 = 
    { fqn = "FQNTest#1"
      src = FilePath "testdll1.dll"
      file = FilePath "test1.cpp"
      ln = DocumentCoordinate 100 }

let stubTC2 = 
    { fqn = "FQNTest#2"
      src = FilePath "testdll2.dll"
      file = FilePath "test2.cpp"
      ln = DocumentCoordinate 200 }

let stubTR1p = 
    { name = "Test Result #1"
      outcome = DTestOutcome.TOPassed }

let stubTR2p = 
    { name = "Test Result #2"
      outcome = DTestOutcome.TOPassed }

let stubTR2f = 
    { name = "Test Result #2"
      outcome = DTestOutcome.TOFailed }

let newTST (tc : SimpleTestCase) = 
    let tst = FakeMappingTagSpan<IMarginGlyphTag>()
    tst.Tag <- { testCases = [ tc.toTC() ]
                 location = tc.toTID().location }
    tst :> IMappingTagSpan<_>

let newFPT (tr : SimpleTestResult) = 
    let fpt = FakeMappingTagSpan<IMarginGlyphTag>()
    fpt.Tag <- { tfis = 
                     [ { message = tr.name
                         stack = [| UnparsedFrame "Failure at line 100" |] } ] }
    fpt :> IMappingTagSpan<_>

let newCCT (tctrs : seq<SimpleTestCase * SimpleTestResult>) = 
    let cct = FakeMappingTagSpan<IMarginGlyphTag>()
    
    let spid = 
        { methodId = 
              { assemblyId = AssemblyId(Guid.NewGuid())
                mdTokenRid = MdTokenRid 101u }
          uid = 1 }
    
    let sp = 
        { id = spid
          document = FilePath "code.cs"
          startLine = DocumentCoordinate 10
          startColumn = DocumentCoordinate 1
          endLine = DocumentCoordinate 10
          endColumn = DocumentCoordinate 100 }
    
    let trs = tctrs |> Seq.fold (fun acc (tc, tr) -> (tc.toTC() |> tr.toTR) :: acc) []
    cct.Tag <- { CCTSeqPoint = sp
                 CCTTestResults = trs }
    cct :> IMappingTagSpan<_>

[<Fact>]
let ``For empty input return None``() = 
    let ts = Seq.empty
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(None, it)

[<Fact>]
let ``Color test - 1 TST 1 FPT 1 CCTp 1 CCTf - return Red and all GlyphTags``() = 
    let ts = 
        [ newTST stubTC1
          newFPT stubTR2f
          newCCT [ stubTC1, stubTR1p ]
          newCCT [ stubTC2, stubTR2f ] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(Colors.Red, it |> Option.map (fun (_, i) -> i.color))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``Color test - 1 TST 1 FPT 1 CCTp 1 CCTp - return Green and all GlyphTags``() = 
    let ts = 
        [ newTST stubTC1
          newFPT stubTR2f
          newCCT [ stubTC1, stubTR1p ]
          newCCT [ stubTC2, stubTR2p ] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(Colors.Green, it |> Option.map (fun (_, i) -> i.color))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``Color test - 1 TST 1 FPT 0 CCT - return White and all GlyphTags``() = 
    let ts = 
        [ newTST stubTC1
          newFPT stubTR2f ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(Colors.LightGray, it |> Option.map (fun (_, i) -> i.color))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``Color test - 1 TST 1 FPT 1 CCTx - return White and all GlyphTags``() = 
    let ts = 
        [ newTST stubTC1
          newFPT stubTR2f
          newCCT [] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(Colors.LightGray, it |> Option.map (fun (_, i) -> i.color))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``GlyphType test - 1 TST 1 FPT 1 CCTp 1 CCTf - return TS and all GlyphTags``() = 
    let ts = 
        [ newTST stubTC1
          newFPT stubTR2f
          newCCT [ stubTC1, stubTR1p ]
          newCCT [ stubTC2, stubTR2f ] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(TestStart, it |> Option.map (fun (_, i) -> i.glyphType))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``GlyphType test - 1 FPT 1 CCTp 1 CCTf - return FP and all GlyphTags``() = 
    let ts = 
        [ newFPT stubTR2f
          newCCT [ stubTC1, stubTR1p ]
          newCCT [ stubTC2, stubTR2f ] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(FailurePoint, it |> Option.map (fun (_, i) -> i.glyphType))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``GlyphType test - 1 CCTp 1 CCTf - return CCf and all GlyphTags``() = 
    let ts = 
        [ newCCT [ stubTC1, stubTR1p ]
          newCCT [ stubTC2, stubTR2f ] ]
    
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(CodeCoverage, it |> Option.map (fun (_, i) -> i.glyphType))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))

[<Fact>]
let ``GlyphType test - 1 CCTx - return CC and all GlyphTags``() = 
    let ts = [ newCCT [] ]
    let it = (Rect(), ts) |> GlyphInfoGenerator.generate
    Assert.Equal(Rect(), it |> Option.map (fun (r, _) -> r))
    Assert.Equal(CodeCoverage, it |> Option.map (fun (_, i) -> i.glyphType))
    Assert.Equal(ts |> List.map (fun t -> t.Tag), it |> Option.map (fun (_, i) -> i.glyphTags |> Seq.toList))
