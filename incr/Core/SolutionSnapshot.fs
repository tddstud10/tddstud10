namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open Microsoft.Build.Execution
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Xml
open System.Xml.Linq
open System.Xml.XPath
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics

module QuickGraphExtensions = 

    let visitInDependencyOrder2 nodeProcessor (dg : AdjacencyGraph<_, _>) = 
        let rec dependencyLoop np (dg : AdjacencyGraph<_, _>) = 
            AlgorithmExtensions.Sinks<_, _>(dg)
            |> Seq.tryHead
            |> Option.iter (fun p -> 
                   p |> np
                   dg.RemoveVertex(p) |> ignore
                   dependencyLoop np dg)
        dependencyLoop nodeProcessor dg

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
module ProjectExtensionsTBM = 
    open Microsoft.Build.Utilities
    open Microsoft.Build.Framework

    type BuildLogger() =
        inherit Logger()
        let warnings = ConcurrentQueue<string>()
        let errors = ConcurrentQueue<string>()
        member __.Warnings = warnings
        member __.Errors = errors
        override __.Initialize(es : IEventSource) =
            es.WarningRaised.Add(fun w -> warnings.Enqueue(sprintf "%s(%d,%d): %s error %s: %s" w.File w.LineNumber w.ColumnNumber w.Subcategory w.Code w.Message))
            es.ErrorRaised.Add(fun e -> errors.Enqueue(sprintf "%s(%d,%d): %s error %s: %s" e.File e.LineNumber e.ColumnNumber e.Subcategory e.Code e.Message))

    type Project with
        
        static member createSnapshot (s : Solution2) (p : Project) = 
            let copyToSnapshotRoot root p = 
                let dst = (root, p |> Path.getPathWithoutRoot) ||> Path.combine
                dst
                |> Path.getDirectoryName
                |> Directory.createDirectory
                |> ignore
                let i = 
                    try 
                        (p, dst) ||> File.forceCopy
                        None
                    with :? FileNotFoundException as e -> e :> exn |> Some
                dst, i
            
            let root = sprintf @"d:\tddstud10\%d" (p.GetHashCode()) |> FilePath
            
            let is = 
                [ p.Items; p.FileReferences ]
                |> Seq.concat
                |> Seq.fold (fun acc e -> 
                       let _, ex = (root, e) ||> copyToSnapshotRoot
                       ex |> Option.fold (fun acc e -> e :: acc) acc) []
            
            let xdoc = XDocument.Load(p.Path.ToString())
            let xnm = XmlNamespaceManager(NameTable())
            xnm.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
            let is2 = 
                Extensions.XPathSelectElements
                    (xdoc, "/msb:Project/msb:ItemGroup/msb:None[@Include='packages.config']", xnm)
                |> Seq.map 
                       (fun _ -> 
                       let pconfig = (p.Path |> Path.getDirectoryName, "packages.config" |> FilePath) ||> Path.combine
                       let pconfig = XDocument.Load(pconfig.ToString())
                       Extensions.XPathSelectElements(pconfig, "/packages/package") 
                       |> Seq.map 
                              (fun pkg -> 
                              let id = pkg.Attribute(XName.Get("id")).Value
                              let ver = pkg.Attribute(XName.Get("version")).Value
                              (Path.getDirectoryName s.Path, (sprintf "packages\%s.%s" id ver) |> FilePath) 
                              ||> Path.combine))
                |> Seq.collect id
                |> Seq.map (fun p -> Directory.EnumerateFiles(p.ToString(), "*", SearchOption.AllDirectories))
                |> Seq.collect id
                |> Seq.map FilePath
                |> Seq.fold (fun acc e -> 
                       let _, ex = (root, e) ||> copyToSnapshotRoot
                       ex |> Option.fold (fun acc e -> e :: acc) acc) is
            
            let is3 = 
                Extensions.XPathSelectElements
                    (xdoc, "/msb:Project/msb:PropertyGroup/msb:AssemblyOriginatorKeyFile", xnm)
                |> Seq.map (fun x -> 
                    (p.Path |> Path.getDirectoryName, x.Value |> FilePath) ||> Path.combine)
                |> Seq.fold (fun acc e -> 
                       let _, ex = (root, e) ||> copyToSnapshotRoot
                       ex |> Option.fold (fun acc e -> e :: acc) acc) is2

            let dst, i = (root, p.Path) ||> copyToSnapshotRoot
            { ProjectSnapshot.Path = dst
              Issues = i |> Option.fold (fun acc e -> e :: acc) is3 }
        
        static member fixupProject (dpids : seq<ProjectId>) (rmap : Dictionary<ProjectId, ProjectBuildResult>) (psn : ProjectSnapshot) = 
            let createFileRefIGFragment inc (hp : string) = 
                XElement(XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003"), 
                    XElement(XName.Get("Reference", "http://schemas.microsoft.com/developer/msbuild/2003"), 
                        XAttribute(XName.Get("Include"), inc), 
                        XElement(XName.Get("HintPath", "http://schemas.microsoft.com/developer/msbuild/2003"), hp), 
                        XElement(XName.Get("Private", "http://schemas.microsoft.com/developer/msbuild/2003"), "True")))
            
            let xdoc = XDocument.Load(psn.Path.ToString())
            let xnm = XmlNamespaceManager(NameTable())
            xnm.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
            let ig1 = Extensions.XPathSelectElements(xdoc, "//msb:ItemGroup", xnm) |> Seq.nth 0

            dpids
            |> Seq.iter (fun dpid -> 
                         rmap.[dpid].Outputs 
                         |> Seq.iter (fun o -> 
                                      let oName = Path.GetFileNameWithoutExtension(o)
                                      (oName, o)
                                      ||> createFileRefIGFragment
                                      |> ig1.AddBeforeSelf))

            Extensions.XPathSelectElements(xdoc, "//msb:ProjectReference", xnm) 
            |> Seq.iter (fun x -> x.Remove())

            xdoc.Save(psn.Path.ToString())
            psn
        
        static member buildSnapshot (_ : Dictionary<ProjectId, ProjectBuildResult>) (psn : ProjectSnapshot) = 
            let wrapperProject = """<Project ToolsVersion="12.0" DefaultTargets="_TddStud10BuildProject" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Target Name="_TddStud10BuildProject">
    <MSBuild 
      Projects="$(_TddStud10Project)" 
      Targets="$(_TddStud10Target)"
    >
        <Output TaskParameter="TargetOutputs" ItemName="_TddStud10TargetOutputs" />
    </MSBuild>
  </Target>
</Project>"""
            let wrapperProjectPath = 
                (psn.Path |> Path.getDirectoryName, "tddstud10wrapper.proj" |> FilePath) ||> Path.combine
            File.WriteAllText(wrapperProjectPath.ToString(), wrapperProject)
            let properties = 
                Map.empty
                |> Map.add "_TddStud10Project" (psn.Path.ToString())
                |> Map.add "_TddStud10Target" "Build"
            
            let l = BuildLogger()
            let p = ProjectInstance(wrapperProjectPath.ToString(), properties :> IDictionary<_, _>, "12.0")
            let status = p.Build([| "_TddStud10BuildProject" |], [l :> ILogger])
            let outputs = p.GetItems("_TddStud10TargetOutputs") |> Seq.map (fun i -> i.EvaluatedInclude)
            { Status2 = status
              Warnings = l.Warnings
              Errors = l.Errors
              Outputs = outputs }

type SolutionSnapshot() = 
    let beginCreateSnapshot = new Event<_>()
    let beginCreateProjectSnapshot = new Event<_>()
    let endCreateProjectSnapshot = new Event<_>()
    let endCreateSnapshot = new Event<_>()
    member public __.BeginCreateSnapshot = beginCreateSnapshot.Publish
    member public __.BeginCreateProjectSnapshot = beginCreateProjectSnapshot.Publish
    member public __.EndCreateProjectSnapshot = endCreateProjectSnapshot.Publish
    member public __.EndCreateSnapshot = endCreateSnapshot.Publish
    member __.Load(sln : Solution2) : Async<unit> = 
        let processProject (rmap : Dictionary<ProjectId, ProjectBuildResult>) (sln : Solution2) pid = 
            Common.safeExec (fun () -> beginCreateProjectSnapshot.Trigger(pid))
            let failedDeps = 
                sln.DependencyMap2.[pid]
                |> Seq.filter (fun p -> not rmap.[p].Status2)

            let res = 
                if (failedDeps |> Seq.length = 0) then
                    sln.Projects.[pid]
                    |> Project.createSnapshot sln
                    |> Project.fixupProject sln.DependencyMap2.[pid] rmap
                    |> Project.buildSnapshot rmap
                else
                    { Status2 = false
                      Warnings = Seq.empty
                      Errors = failedDeps |> Seq.map (fun p -> sprintf "Required project %s failed to build." p.UniqueName)
                      Outputs = Seq.empty }
            rmap.Add(pid, res)
                
            // What is the immutable-functional approach to rmap?
            Common.safeExec (fun () -> endCreateProjectSnapshot.Trigger(pid, res))
        async { 
            Common.safeExec (fun () -> beginCreateSnapshot.Trigger(sln))
            let rmap = Dictionary<ProjectId, ProjectBuildResult>()
            sln.DependencyMap2.Keys.ToAdjacencyGraph<_, _>
                (Func<_, _>(fun s -> sln.DependencyMap2.[s] |> Seq.map (fun t -> SEquatableEdge<_>(s, t)))) 
            |> QuickGraphExtensions.visitInDependencyOrder2 (processProject rmap sln)
            Common.safeExec (fun () -> endCreateSnapshot.Trigger(sln))
        }
