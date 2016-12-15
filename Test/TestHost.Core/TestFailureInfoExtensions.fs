namespace R4nd0mApps.TddStud10.TestHost

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Text.RegularExpressions

module TestFailureInfoExtensions = 
    let private stackFrameCracker = 
        Regex("""^at (?<atMethod>(.*)) in (?<document>(.*))\:line (?<line>(\d+))$""", RegexOptions.Compiled)
    
    let create (tr : DTestResult) : seq<DocumentLocation * TestFailureInfo> = 
        let parseSF input = 
            let m = input |> stackFrameCracker.Match
            if m.Success then 
                (m.Groups.["atMethod"].Value, 
                 { document = m.Groups.["document"].Value |> FilePath
                   line = 
                       m.Groups.["line"].Value
                       |> Int32.Parse
                       |> DocumentCoordinate })
                |> ParsedFrame
            else input |> UnparsedFrame
        if tr.Outcome <> TOFailed || tr.ErrorStackTrace = null then Seq.empty
        else 
            let stack = 
                tr.ErrorStackTrace.Split([| "\r\n"; "\r"; "\n" |], StringSplitOptions.RemoveEmptyEntries)
                |> Seq.map (fun s -> s.Trim() |> parseSF)
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
