namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

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
              ProjectReferences = p.GetProjectReferences() }
    
    type ProjectLoadResult with
        static member createFailedResult e = 
            { Status = false
              Warnings = Seq.empty
              Errors = e
              Outputs = Seq.empty }
