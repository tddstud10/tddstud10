namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Threading

type IncrementalBuildPipeline(bcs : Solution -> unit, bcps : ProjectId -> unit, ecps : ProjectId * ProjectLoadResult -> unit, ecs : Solution -> unit) = 
    do Logger.logInfof "In IncrementalBuildPipeline - Initializing..."
    let mutable disposed = false
    let sc = SynchronizationContext.CaptureCurrent()
    
    let normalizeDeltaStream (e : Event<_>) = e.Publish |> Event.map ((function 
                                                                      | ProcessDelta(fp, p) -> fp, p) |> Seq.map)

    // Agents and their events
    //
    // SolutionSynchronizer Agent & Events
    let ssaes = 
        { LoadStarting = Event<_>()
          ProjectLoadStarting = Event<_>()
          ProjectLoadNeeded = Event<_>()
          ProjectLoadFinished = Event<_>()
          LoadFailed = Event<_>()
          LoadFinished = Event<_>()
          SyncAndBuildStarting = Event<_>()
          ProjectSyncAndBuildStarting = Event<_>()
          ProjectSyncAndBuildNeeded = Event<_>()
          ProjectSyncAndBuildFinished = Event<_>()
          SyncAndBuildFailed = Event<_>()
          SyncAndBuildFinished = Event<_>() }
    let ssa = SolutionSyncAgent.create ssaes
    // ProjectLoader Agent & Events
    let plaple = Event<_>()
    let dce = Event<_>()
    let pla = ProjectLoaderAgent.create sc dce plaple
    // DeltaBatching Agent & Events
    let dbabce = Event<_>()
    let ndse = dbabce |> normalizeDeltaStream
    let bdpa = DeltaBatchingAgent.create 100 500 dbabce
    // ProjectSynchronizer Agent & Events
    let psapsce = Event<_>()
    let psa = ProjectSyncAgent.create dce psapsce
    // ProjectBuilder Agent & Events
    let pbapbce = Event<_>()
    let pbapbse = Event<_>()
    let pba = ProjectBuilderAgent.create pbapbce pbapbse


    // Wire up Agents to form the agent grid
    //
    let ssaolges = ssaes.LoadStarting.Publish.Subscribe(bcs)
    let ssaoplges = ssaes.ProjectLoadStarting.Publish.Subscribe(bcps)
    let ssaopldes = ssaes.ProjectLoadFinished.Publish.Subscribe(ecps)
    let ssaoldes = ssaes.LoadFinished.Publish.Subscribe(ecs)
    
    let plaples = plaple.Publish.Subscribe(ProcessLoadedProject >> ssa.Post)
    let ssaplnes = ssaes.ProjectLoadNeeded.Publish.Subscribe(LoadProject >> pla.Post)


    let ssapsabnes = ssaes.ProjectSyncAndBuildNeeded.Publish.Subscribe(SyncProject >> psa.Post)
    let pbapbces = pbapbce.Publish.Subscribe(ProcessSyncedAndBuiltProject >> ssa.Post)
    let psapsapsces = psapsce.Publish.Subscribe(BuildProject >> pba.Post)

    let dispose disposing = 
        if not disposed then 
            if (disposing) then 
                psapsapsces.Dispose()
                pbapbces.Dispose()
                ssapsabnes.Dispose()
                ssaplnes.Dispose()
                plaples.Dispose()
                ssaoldes.Dispose()
                ssaopldes.Dispose()
                ssaoplges.Dispose()
                ssaolges.Dispose()
                (psa :> IDisposable).Dispose()
                (psa :> IDisposable).Dispose()
                (bdpa :> IDisposable).Dispose()
                (pla :> IDisposable).Dispose()
                (ssa :> IDisposable).Dispose()
            disposed <- true
    
    override __.Finalize() = dispose false
    
    interface IDisposable with
        member self.Dispose() : _ = 
            dispose true
            GC.SuppressFinalize(self)
    
    member __.Trigger(sln : DTESolution) : unit = SolutionSyncAgent.load ssa sln
    member __.Unload() : unit = 
        Logger.logInfof "Starting unload."
        UnloadSolution |> ssa.Post
