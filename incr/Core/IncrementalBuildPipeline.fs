namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Threading

[<CLIMutable>]
type IncrementalBuildPipelineEventsHandlers =
    { LoadStarting : Solution -> unit 
      ProjectLoadStarting : ProjectId -> unit 
      ProjectLoadFinished : ProjectId * ProjectLoadResult -> unit 
      LoadFinished : Solution * SolutionOperationResult -> unit
      SyncAndBuildStarting : Solution -> unit
      ProjectSyncStarting : Project -> unit
      ProjectSyncFinished : Project * ProjectSyncResult -> unit
      ProjectBuildStarting : Project -> unit
      ProjectBuildFinished : Project * ProjectBuildResult -> unit
      SyncAndBuildFinished : Solution * SolutionOperationResult -> unit }

type IncrementalBuildPipeline(plehs : IncrementalBuildPipelineEventsHandlers) = 
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
          ProjectLoadNeeded = Event<_>()
          LoadFinished = Event<_>()
          SyncAndBuildStarting = Event<_>()
          ProjectSyncAndBuildNeeded = Event<_>()
          SyncAndBuildFinished = Event<_>() }
    let ssa = SolutionSyncAgent.create ssaes
    // ProjectLoader Agent & Events
    let ples = 
        { ProjectLoadEvents.LoadStarting = Event<_>()
          LoadFinished = Event<_>() }
    let dce = Event<_>()
    let pla = ProjectLoaderAgent.create sc dce ples
    // DeltaBatching Agent & Events
    let dbabce = Event<_>()
    let ndse = dbabce |> normalizeDeltaStream
    let bdpa = DeltaBatchingAgent.create 100 500 dbabce
    // ProjectSynchronizer Agent & Events
    let pses = 
        { ProjectSyncEvents.SyncStarting = Event<_>()
          SyncFinished = Event<_>() }
    let psapsce = Event<_>()
    let psa = ProjectSyncAgent.create dce pses
    // ProjectBuilder Agent & Events
    let pbes = 
        { ProjectBuildEvents.BuildStarting = Event<_>()
          BuildFinished = Event<_>() }
    let pba = ProjectBuilderAgent.create pbes

    // Wire up Agents to form the agent grid
    //
    let subs = [
        ssaes.LoadStarting.Publish.Subscribe(plehs.LoadStarting)
        ssaes.LoadFinished.Publish.Subscribe(plehs.LoadFinished)
        ssaes.SyncAndBuildStarting.Publish.Subscribe(plehs.SyncAndBuildStarting)
        ssaes.SyncAndBuildFinished.Publish.Subscribe(plehs.SyncAndBuildFinished)
    
        ssaes.ProjectLoadNeeded.Publish.Subscribe(LoadProject >> pla.Post)
        ples.LoadFinished.Publish.Subscribe(ProcessLoadedProject >> ssa.Post)
        ples.LoadStarting.Publish.Subscribe(plehs.ProjectLoadStarting)
        ples.LoadFinished.Publish.Subscribe(plehs.ProjectLoadFinished)

        ssaes.ProjectSyncAndBuildNeeded.Publish.Subscribe(SyncProject >> psa.Post)
        pbes.BuildFinished.Publish.Subscribe(ProcessSyncedAndBuiltProject >> ssa.Post)
        pbes.BuildStarting.Publish.Subscribe(plehs.ProjectBuildStarting)
        pbes.BuildFinished.Publish.Subscribe(plehs.ProjectBuildFinished)

        pses.SyncFinished.Publish.Subscribe(BuildProject >> pba.Post)
        pses.SyncStarting.Publish.Subscribe(plehs.ProjectSyncStarting)
        pses.SyncFinished.Publish.Subscribe(plehs.ProjectSyncFinished)
    ]

    let dispose disposing = 
        if not disposed then 
            if (disposing) then 
                subs |> List.fold (fun () e -> e.Dispose()) ()
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
