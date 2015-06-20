namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Tagging
open System.ComponentModel.Design
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain
open System.Windows.Media

(*
Simple Design:

painter =
    (Point * seq<ITextViewLine>) -> seq<Rect * ITextViewLine> 
    >> Seq.map ((Rect * ITextViewLine) -> (Rect * seq<MarginGlyphTag>))
    >> Seq.map ((Rect * seq<MarginGlyphTag>) -> (Rect * GlyphInfo))
    >> Seq.map ((Rect * GlyphInfo) -> (Rect * Glyph)) 
    >> seq<Rect * Glyph> -> ()

MarginGlyphTag:
- TestStartTag { testCase }
- SequencePointTag { sequencePoint }
- CodeCoverageTag { sequencePoint, seq<testRunId> }
- FailurePointTag { exception }

GlyphInfo
- Color 
- SequencePoint | TestStart | FailurePoint
- Tooltip
- ContextMenu
- Tag

Order of lightup
- Test Start
- Sequence Points
- Failure Points
- Code Coverage

Open Questions:
- CodeCoverageTag will consume the SequencePointTag
  - Will the SequencePointTagger get called twice?
- Will we call all taggers for every ZF/LC events? Probably yes
- Can call only selective taggers depending on events from pipeline?
- Should we do async tagging?
- Optimize for empty lines, comments, etc.

 *)

type IMarginGlyphTag = 
    inherit ITag

type TestStartTag = 
    { testCase : TestCase
      textHash : int }
    interface IMarginGlyphTag

type SequencePointTag = 
    { sp : SequencePoint }
    interface IMarginGlyphTag

type FailurePointTag = 
    { testCase : TestResult }
    interface IMarginGlyphTag

type CodeCoverageTag = 
    { testCase : TestCase }
    interface IMarginGlyphTag

(* NOTE: This should have an 1-1 mapping with the FrameworkElement being displayed in the Margin Canvas. 
         This is so that we dont need to test the GlyphGenerator code. *)
type GlyphType = 
    | TestStart
    | FailurePoint
    | SequencePoint

type MarginGlyphInfo = 
    { color : Color
      glyphType : GlyphType
      glyphTags : IMarginGlyphTag seq
      toolTipText : string
      contextMenu : CommandID }
