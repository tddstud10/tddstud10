namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System

type IncrementalBuildPipeline(bcs : Solution -> unit, bcps : ProjectId -> unit, ecps : ProjectId * ProjectLoadResult -> unit, ecs : Solution -> unit) = 
    do Logger.logInfof "In IncrementalBuildPipeline - Initializing..."
    let mutable disposed = false
    let pl = new ProjectLoader()
    let w = new Workspace(pl.LoadProject)
    let ploplss = pl.OnProjectLoaded.Subscribe(w.ProjectLoaded)
    let wolgss = w.OnLoading.Subscribe(bcs)
    let woplgss = w.OnProjectLoading.Subscribe(bcps)
    let wopldss = w.OnProjectLoaded.Subscribe(ecps)
    let woldss = w.OnLoaded.Subscribe(ecs)
    
    let dispose disposing = 
        if not disposed then 
            if (disposing) then 
                woldss.Dispose()
                wopldss.Dispose()
                woplgss.Dispose()
                wolgss.Dispose()
                ploplss.Dispose()
                (pl :> IDisposable).Dispose()
                (w :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.Trigger(sln : DTESolution) : unit = w.Load(sln)
