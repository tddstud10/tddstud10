module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.HostIdeApiTests

open Xunit
open System
open R4nd0mApps.TddStud10.Common.Domain

let createMGT n =
    [ for _ in 1 .. n do yield { new IMarginGlyphTag } ]

let createSP () =
    { id = 
        { methodId = 
            { assemblyId = AssemblyId (Guid.NewGuid())
              mdTokenRid = MdTokenRid 100u }
          uid = 100 }
      document = FilePath ""
      startLine = DocumentCoordinate 1
      startColumn = DocumentCoordinate 1
      endLine = DocumentCoordinate 2
      endColumn = DocumentCoordinate 2 }

let createCCT sp trs =
        { CctSeqPoint = sp
          CctTestResults = trs } :> IMarginGlyphTag

let createTRs =
        List.map (fun (dnr, dnc, dto) -> 
                 { DisplayName = dnr
                   TestCase =     
                     { DtcId = Guid.NewGuid()
                       FullyQualifiedName = "Fully Qualified Name"
                       DisplayName = dnc
                       Source = FilePath "ut.dll"
                       CodeFilePath = FilePath "ut.cs"
                       LineNumber = DocumentCoordinate 1 }
                   Outcome = dto
                   ErrorStackTrace = ""
                   ErrorMessage = "" })

[<Fact>]
let ``If none of the tags are CCTs return, empty results`` () =
    let res = HostIdeApiExtensions.getCoveringTestResults (createMGT 2)
    Assert.Empty(res())

[<Fact>]
let ``If some non-CCT and some CCT, return CCT results`` () =
    let sp = createSP () 
    let trs = createTRs [("TR DN", "TC DN", TOPassed)]
    let cct = createCCT sp trs
    let res = HostIdeApiExtensions.getCoveringTestResults (cct :: (createMGT 2)) ()
    Assert.Equal<seq<SequencePoint * DTestResult>>(res, trs |> Seq.map (fun it -> sp, it))

[<Fact>]
let ``Some non-CCTs, a failed CCT and a success CCT, return both CCT results with failures first`` () =
    let sp = createSP () 
    let trs = createTRs [("TR-DN1", "TC-DN1", TOPassed); ("TR-DN2", "TC-DN2", TOFailed)]
    let cct = createCCT sp trs
    let res = HostIdeApiExtensions.getCoveringTestResults (cct :: (createMGT 2)) ()
    Assert.Equal(Seq.length res, 2)
    Assert.Equal(res |> Seq.nth 0, (sp, trs |> Seq.find (fun it -> it.Outcome = TOFailed)))
    Assert.Equal(res |> Seq.nth 1, (sp, trs |> Seq.find (fun it -> it.Outcome = TOPassed)))

[<Fact>]
let ``Two CCTs, two TRs each, 3 distinct TRs, return only distinct TRs`` () =
    let sp1 = createSP () 
    let trs1 = createTRs [("TR-DN1", "TC-DN", TOPassed); ("TR-DN2", "TC-DN", TOPassed)]
    let sp2 = createSP () 
    let trs2 = createTRs [("TR-DN1", "TC-DN", TOPassed); ("TR-DN3", "TC-DN", TOFailed)]
    let ccts = [createCCT sp1 trs1; createCCT sp2 trs2]
    let res = HostIdeApiExtensions.getCoveringTestResults (ccts @ (createMGT 2)) ()
    let trs = trs1 @ trs2
    Assert.Equal(Seq.length res, 3)
    Assert.Equal(res |> Seq.nth 0, (sp2, trs |> Seq.find (fun it -> it.Outcome = TOFailed)))
    let trdns = res |> Seq.filter (fun (_, it) -> it.Outcome = TOPassed) |> Seq.map (fun (_, it) -> it.DisplayName) |> Seq.sort
    Assert.Equal<string>(trdns, ["TR-DN1"; "TR-DN2"])

