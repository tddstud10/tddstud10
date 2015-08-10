namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.IO
open System.Threading

(* DELETE THIS ONCE WE MERGE BACK *)
module Common = 
    let safeExec (f : unit -> unit) = 
        try 
            f()
        with _ -> ()

[<AutoOpen>]
module EventExtensions = 
    type Event<'T> with
        member self.SafeTrigger(arg : 'T) = Common.safeExec (fun () -> self.Trigger(arg))

(*Logger.logErrorf "Exception thrown: %s." (ex.ToString())*)
(* DELETE THIS ONCE WE MERGE BACK  *)

[<AutoOpen>]
module SystemIOExtensions = 
    type File with
        static member forceCopy (FilePath src) (FilePath dst) = File.Copy(src, dst, true)
    
    type Path with
        
        static member getDirectoryName (FilePath p) = 
            p
            |> Path.GetDirectoryName
            |> FilePath
        
        static member getFileName (FilePath p) = 
            p
            |> Path.GetFileName
            |> FilePath
        
        static member getFileNameWithoutExtension (FilePath p) = 
            p
            |> Path.GetFileName
            |> FilePath
        
        static member getPathWithoutRoot (FilePath p) = p.Substring(Path.GetPathRoot(p).Length) |> FilePath
        static member combine (FilePath p1) (FilePath p2) = Path.Combine(p1, p2) |> FilePath
    
    type Directory with
        static member createDirectory (FilePath p) = p |> Directory.CreateDirectory
        static member combine (FilePath p1) (FilePath p2) = Path.Combine(p1, p2) |> FilePath

[<AutoOpen>]
module SynchronizationContextExtensions = 
    type SynchronizationContext with
        
        static member CaptureCurrent() = 
            match SynchronizationContext.Current with
            | null -> SynchronizationContext()
            | ctxt -> ctxt
        
        member self.Execute<'T>(f : unit -> 'T option) = 
            let ret : 'T option ref = ref None
            self.Send((fun _ -> ret := f()), null)
            !ret

[<AutoOpen>]
module DomainExtensions = 
    type Project with
        static member fromDTEProject (p : DTEProject) : Project = 
            { Id = 
                  { UniqueName = p.UniqueName
                    Id = p.ProjectGuid }
              Path = p.FullName |> FilePath
              Items = 
                  p.GetProjectItems()
                  |> Seq.filter DTEProjectItem.isSupported
                  |> Seq.map DTEProjectItem.getFiles
                  |> Seq.collect id
              FileReferences = p.GetFileReferences()
              ProjectReferences = p.GetProjectReferences()
              Watchers = Seq.empty }
    
    type Solution with
        member self.DGraph = 
            (self.DependencyMap |> Map.keys).ToAdjacencyGraph<_, _> 
                (Func<_, _>(fun t -> self.DependencyMap.[t] |> Seq.map (fun s -> SEquatableEdge<_>(s, t))))
