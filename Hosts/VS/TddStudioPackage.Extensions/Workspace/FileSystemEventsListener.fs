namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Workspace

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.IO

(* NOTE: Very thin wrapper over FSW. Add unit tests before adding any more logic *)
type FileSystemEventsListener(path : FilePath) = 
    let mutable disposed = false
    let projectItemChanged = Event<_>()
    let fsw = 
        new FileSystemWatcher(path.ToString() |> Path.GetDirectoryName, Filter = (path.ToString() |> Path.GetFileName), 
                              NotifyFilter = NotifyFilters.LastWrite, EnableRaisingEvents = true)
    let pi = { ProjectItem.path = path }
    let fswSub = fsw.Changed.Subscribe(fun x -> projectItemChanged.Trigger(pi))
    
    interface IProjectItemEventsListener with
        member __.ProjectItemChanged : IObservable<ProjectItem> = projectItemChanged.Publish :> _
    
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                fswSub.Dispose()
                fsw.Dispose()
            disposed <- true
    
    override x.Finalize() = x.Dispose(false)
    interface IDisposable with
        member x.Dispose() : _ = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
