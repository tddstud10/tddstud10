namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open System

type IncrementalBuildPipeline(bcs : Solution -> unit, bcps : ProjectId -> unit, ecps : ProjectId -> unit, ecs : Solution -> unit) = 
    let mutable disposed = false
    let w = Workspace()
    let ss = SolutionSnapshot()
    let wlcs = w.LoadComplete.Subscribe(ss.Load >> Async.Start)
    let ssbcss = ss.BeginCreateSnapshot.Subscribe(bcs)
    let ssbcpss = ss.BeginCreateProjectSnapshot.Subscribe(bcps)
    let ssecpss = ss.EndCreateProjectSnapshot.Subscribe(ecps)
    let ssecss = ss.EndCreateSnapshot.Subscribe(ecs)
    
    let dispose disposing = 
        if not disposed then 
            if (disposing) then 
                ssecss.Dispose()
                ssecpss.Dispose()
                ssbcpss.Dispose()
                ssbcss.Dispose()
                wlcs.Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.Trigger(sln : DTESolution) : unit = w.Load(sln)
