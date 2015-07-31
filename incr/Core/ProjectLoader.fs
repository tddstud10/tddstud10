namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Threading

type ProjectLoader() = 
    let mutable disposed = false
    let onProjectLoaded = new Event<_>()
    let syncContext = SynchronizationContext.CaptureCurrent()
    
    let processProject s p (rmap : ProjectLoadResultMap) = 
        let failedPrereqs = 
            rmap
            |> Seq.filter (fun kv -> not kv.Value.Status)
        if (failedPrereqs |> Seq.length > 0) then 
            failedPrereqs 
            |> Seq.map (fun kv -> sprintf "Required project %s failed to build." kv.Key.UniqueName)
            |> ProjectLoadResult.createFailedResult  
        else 
            let proj : Project option ref = ref None
            syncContext.Send((fun _ -> proj := (s, p) ||> ProjectExtensions.loadProject), null)
            match !proj with
            | Some proj -> 
                proj
                |> ProjectExtensions.createSnapshot s
                |> ProjectExtensions.fixupProject rmap
                |> ProjectExtensions.buildSnapshot
            | None -> [ sprintf "Required project %s failed to load." p.UniqueName ] |> ProjectLoadResult.createFailedResult 
    
    let rec processor (inbox : MailboxProcessor<_>) = 
        async { 
            let! msg = inbox.Receive()
            match msg with
            | LoadProject(s, p, rmap) -> 
                let res = 
                    try 
                        processProject s p rmap
                    with e ->
                        [ e.ToString() ] |> ProjectLoadResult.createFailedResult 
                Common.safeExec (fun () -> onProjectLoaded.Trigger(p, res))
                return! processor inbox
        }
    
    let agent = AutoCancelAgent.Start(processor)
    
    let dispose disposing = 
        if not disposed then 
            if (disposing) then (agent :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.LoadProject s p rmap = agent.Post((s, p, rmap) |> LoadProject)
    member __.OnProjectLoaded = onProjectLoaded.Publish
