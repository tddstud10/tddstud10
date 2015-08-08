namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Generic

type ProjectSnapshoterAgent() = 
    let mutable disposed = false
    
    let agent = 
        AutoCancelAgent.Start(fun inbox -> 
            let rec loop() = 
                async { 
                    let! msg = inbox.Receive()
                    match msg with
                    | CreateSnapshot p -> return ()
                }
            loop())
    
    let dispose disposing = 
        if not disposed then 
            if (disposing) then (agent :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.SnapshotProject(p) = agent.Post(CreateSnapshot p)
