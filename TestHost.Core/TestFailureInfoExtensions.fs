namespace R4nd0mApps.TddStud10.TestHost

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Text.RegularExpressions
open R4nd0mApps.TddStud10.Common

module TestFailureInfoExtensions = 
    let private StackFrameCracker = 
        Regex("""^at (?<atMethod>(.*)) in (?<document>(.*))\:line (?<line>(\d+))$""", RegexOptions.Compiled)
    
    let create rsp (tr : TestResult) : seq<DocumentLocation * TestFailureInfo> = 
        let parseSF input = 
            let m = input |> StackFrameCracker.Match
            if m.Success then 
                (m.Groups.["atMethod"].Value, 
                 { document = 
                       m.Groups.["document"].Value
                       |> FilePath
                       |> PathBuilder.rebaseCodeFilePath rsp
                   line = 
                       m.Groups.["line"].Value
                       |> Int32.Parse
                       |> DocumentCoordinate })
                |> ParsedFrame
            else input |> UnparsedFrame
        if tr.Outcome <> TestOutcome.Failed || tr.ErrorStackTrace = null then Seq.empty
        else 
            let stack = 
                tr.ErrorStackTrace.Trim().Split([| "\r\n" |], StringSplitOptions.RemoveEmptyEntries)
                |> Seq.map parseSF
                |> Seq.toArray
            
            let tfi = 
                { message = 
                      if tr.ErrorMessage = null then ""
                      else tr.ErrorMessage
                  stack = stack }
            
            stack |> Seq.choose (fun sf -> 
                         match sf with
                         | ParsedFrame(_, dl) -> Some(dl, tfi)
                         | _ -> None)
