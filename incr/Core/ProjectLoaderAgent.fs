module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.ProjectLoaderAgent

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Threading
open FSharpx.Collections
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

let private processProject (sc : SynchronizationContext) s p (rmap : ProjectLoadResultMap) = 
    let failedPrereqs = 
        rmap
        |> Map.valueList
        |> Seq.choose (function | None -> Some p | Some r -> match r with | LoadSuccess _ -> None | LoadFailure _ -> Some p)
    if (failedPrereqs |> Seq.length > 0) then 
        failedPrereqs 
        |> Seq.map (fun kv -> sprintf "Required project %s failed to load." kv.UniqueName)
        |> LoadFailure
    else 
        let proj = sc.Execute (fun () -> (s, p) ||> ProjectExtensions.loadProject)
        match proj with
        | Some proj -> proj |> LoadSuccess
        | None -> 
            sprintf "Project %s failed to load." p.UniqueName |> Seq.singleton |> LoadFailure
    
let rec private processor (sc : SynchronizationContext) nc (ple : Event<_>) (mbox : Agent<_>) = 
    async { 
        Logger.logInfof "PLA: #### Starting loop."
        let! msg = mbox.Receive()
        Logger.logInfof "PLA: #### Processing message %A." msg
        match msg with
        | LoadProject(s, rmap, pid) -> 
            Logger.logInfof "PLA: Loading project %A." pid
            let res = 
                try 
                    processProject sc s pid rmap
                with e ->
                    e.ToString() |> Seq.singleton |> LoadFailure
            ple.SafeTrigger(pid,res)
            Logger.logInfof "PLA: Done loading project %s: Result = %A." pid.UniqueName res
        return! processor sc nc ple mbox
    }

let create sc nc ple = (sc, nc, ple) |||> processor |> AutoCancelAgent.Start
