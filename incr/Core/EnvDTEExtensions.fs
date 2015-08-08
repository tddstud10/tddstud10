[<AutoOpen>]
module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.EnvDTEExtensions

open Clide.Solution.Adapters
open FSharpVSPowerTools
open QuickGraph
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections
open System.Diagnostics
open VSLangProj

type DTEConstants = EnvDTE.Constants

type DTESolution = EnvDTE.Solution

type DTEProject = EnvDTE.Project

type DTEProjectItem = EnvDTE.ProjectItem

type DTEProjectItems = EnvDTE.ProjectItems

type DTEBuildDependency = EnvDTE.BuildDependency

type MSBuildProject = Microsoft.Build.Evaluation.Project

type EnvDTE.Project with
    
    member self.ProjectGuid = 
        let p = EnvDTE.AdapterFacade.Adapt(self).As<MSBuildProject>()
        let id = p.GetPropertyValue("ProjectGuid")
        Guid.Parse(id)
    
    member self.GetProjectItems() = 
        let rec loop (l0 : list<DTEProjectItem>) (is : list<DTEProjectItem>) : list<DTEProjectItem> = 
            match is with
            | [] -> l0
            | i :: is -> 
                let l2 = [ i ]
                let l1 = i.ProjectItems |> List.fromUntypedEnumerable<DTEProjectItem>
                loop (l2 @ l0) (l1 @ is)
        self.ProjectItems
        |> List.fromUntypedEnumerable<DTEProjectItem>
        |> loop []
    
    // TODO: Understand the maybe monad
    member self.GetProjectReferences() = 
        (self.Object :?> VSProject).References
        |> Seq.cast<Reference>
        |> Seq.choose (fun ref -> maybe { let! ref = Option.ofNull ref
                                          let! p = Option.attempt (fun _ -> ref.SourceProject)
                                          return! Option.ofNull p })
        |> Seq.map (fun p -> 
               { UniqueName = p.UniqueName
                 Id = p.ProjectGuid })
    
    // TODO: Understand the maybe monad
    member self.GetFileReferences() = 
        (self.Object :?> VSProject).References
        |> Seq.cast<Reference>
        |> Seq.choose (fun ref -> 
               maybe { 
                   let! ref = Option.ofNull ref
                   let! isCopyLocal = Option.attempt (fun _ -> ref.CopyLocal)
                   let! p = Option.attempt (fun _ -> ref.SourceProject)
                   let! path = Option.attempt (fun _ -> ref.Path)
                   return! if isCopyLocal && p = null then Option.ofNull path
                           else None
               })
        |> Seq.map (fun p -> p |> FilePath)

let private unsupportedProjectItemKinds = 
    [ DTEConstants.vsProjectItemKindPhysicalFolder.ToUpperInvariant() ] |> Set.ofSeq

type EnvDTE.ProjectItem with
    
    static member isSupported (pi : DTEProjectItem) = 
        pi.Kind.ToUpper()
        |> unsupportedProjectItemKinds.Contains
        |> not
    
    static member getFiles (pi : DTEProjectItem) = 
        seq { 
            for i in 1..(int) pi.FileCount do
                yield pi.FileNames((int16) i) |> FilePath
        }

type EnvDTE.Solution with
    
    member self.GetBuildDependencies() : seq<DTEBuildDependency> = 
        seq { 
            for i in 1..(int) self.SolutionBuild.BuildDependencies.Count do
                yield self.SolutionBuild.BuildDependencies.Item(i)
        }
    
    member self.GetProjects() : seq<DTEProject> = self.GetBuildDependencies() |> Seq.map (fun bd -> bd.Project)
