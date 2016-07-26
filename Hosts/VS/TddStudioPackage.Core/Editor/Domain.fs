namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain
open System.Windows.Media

type SequencePointTag = 
    { sp : SequencePoint }
    interface ITag

type IMarginGlyphTag = 
    inherit ITag

type TestStartTag = 
    { testCases : DTestCase seq
      location : DocumentLocation }
    interface IMarginGlyphTag

type FailurePointTag = 
    { tfis : TestFailureInfo seq }
    interface IMarginGlyphTag

type CodeCoverageTag = 
    { CCTSeqPoint : SequencePoint
      CCTTestResults : DTestResult seq }
    interface IMarginGlyphTag

(* NOTE: This should have an 1-1 mapping with the FrameworkElement being displayed in the Margin Canvas. 
         This is so that we dont need to test the GlyphGenerator code. *)
type MarginGlyphType = 
    | TestStart
    | FailurePoint
    | CodeCoverage

type MarginGlyphInfo = 
    { color : Color
      glyphType : MarginGlyphType
      glyphTags : IMarginGlyphTag seq }
