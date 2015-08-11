namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open Microsoft.Build.Execution
open Microsoft.Build.Framework
open Microsoft.Build.Utilities
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Xml
open System.Xml.Linq
open System.Xml.XPath

type BuildLogger() = 
    inherit Microsoft.Build.Utilities.Logger()
    let warnings = ConcurrentQueue<string>()
    let errors = ConcurrentQueue<string>()
    member __.Warnings = warnings
    member __.Errors = errors
    override __.Initialize(es : IEventSource) = 
        es.WarningRaised.Add
            (fun w -> 
            warnings.Enqueue
                (sprintf "%s(%d,%d): %s error %s: %s" w.File w.LineNumber w.ColumnNumber w.Subcategory w.Code w.Message))
        es.ErrorRaised.Add
            (fun e -> 
            errors.Enqueue
                (sprintf "%s(%d,%d): %s error %s: %s" e.File e.LineNumber e.ColumnNumber e.Subcategory e.Code e.Message))

module ProjectExtensions = 
    let subscribeToChangeNotifications (proj : Project) res (dce : Event<_>) = 
        let createFSW path = 
            let fsw = new FileSystemWatcher()
            fsw.Path <- (Path.getDirectoryName path).ToString()
            fsw.Filter <- (Path.getFileName path).ToString()
            fsw.Changed.Add(fun e -> ((e.FullPath |> FilePath), proj) |> dce.Trigger)
            // TODO: This is temporary till we wire up the Editor changed event handlers
            fsw.Renamed.Add(fun e -> ((e.FullPath |> FilePath), proj) |> dce.Trigger)
            fsw.EnableRaisingEvents <- true
            fsw
        
        let iws = 
            proj.Items
            |> Seq.map createFSW
            |> Seq.toArray
        
        (*{ res with ItemWatchers = iws }*)
        res
    
    let loadProject (s : Solution) (pid : ProjectId) = 
        let proj = s.Solution.GetProjects() |> Seq.tryFind (fun p -> p.UniqueName = pid.UniqueName)
        proj |> Option.map Project.fromDTEProject
    
    let createSnapshot (s : Solution) (p : Project) = 
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
            Extensions.XPathSelectElements(xdoc, "/msb:Project/msb:ItemGroup/msb:None[@Include='packages.config']", xnm)
            |> Seq.map 
                   (fun _ -> 
                   let pconfig = (p.Path |> Path.getDirectoryName, "packages.config" |> FilePath) ||> Path.combine
                   let pconfig = XDocument.Load(pconfig.ToString())
                   Extensions.XPathSelectElements(pconfig, "/packages/package") 
                   |> Seq.map 
                          (fun pkg -> 
                          let id = pkg.Attribute(XName.Get("id")).Value
                          let ver = pkg.Attribute(XName.Get("version")).Value
                          (Path.getDirectoryName s.Path, (sprintf "packages\%s.%s" id ver) |> FilePath) ||> Path.combine))
            |> Seq.collect id
            |> Seq.map (fun p -> Directory.EnumerateFiles(p.ToString(), "*", SearchOption.AllDirectories))
            |> Seq.collect id
            |> Seq.map FilePath
            |> Seq.fold (fun acc e -> 
                   let _, ex = (root, e) ||> copyToSnapshotRoot
                   ex |> Option.fold (fun acc e -> e :: acc) acc) is
        
        let is3 = 
            Extensions.XPathSelectElements(xdoc, "/msb:Project/msb:PropertyGroup/msb:AssemblyOriginatorKeyFile", xnm)
            |> Seq.map (fun x -> (p.Path |> Path.getDirectoryName, x.Value |> FilePath) ||> Path.combine)
            |> Seq.fold (fun acc e -> 
                   let _, ex = (root, e) ||> copyToSnapshotRoot
                   ex |> Option.fold (fun acc e -> e :: acc) acc) is2
        
        let dst, i = (root, p.Path) ||> copyToSnapshotRoot
        let is4 = i |> Option.fold (fun acc e -> e :: acc) is3

        SnapshotSuccess(dst, p, is4 |> Seq.map (fun e -> e.ToString()))
    
    let fixupProject bos (psnPath : FilePath) = 
        let createFileRefIGFragment inc (hp : FilePath) = 
            XElement
                (XName.Get("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003"), 
                 XElement
                     (XName.Get("Reference", "http://schemas.microsoft.com/developer/msbuild/2003"), 
                      XAttribute(XName.Get("Include"), inc), 
                      XElement(XName.Get("HintPath", "http://schemas.microsoft.com/developer/msbuild/2003"), hp.ToString()), 
                      XElement(XName.Get("Private", "http://schemas.microsoft.com/developer/msbuild/2003"), "True")))
        let xdoc = XDocument.Load(psnPath.ToString())
        let xnm = XmlNamespaceManager(NameTable())
        xnm.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
        let ig1 = Extensions.XPathSelectElements(xdoc, "//msb:ItemGroup", xnm) |> Seq.nth 0
        bos
        |> Seq.iter (fun o -> 
               let oName = o |> Path.getFileNameWithoutExtension
               (oName, o)
               ||> createFileRefIGFragment
               |> ig1.AddBeforeSelf)
        Extensions.XPathSelectElements(xdoc, "//msb:ProjectReference", xnm) |> Seq.iter (fun x -> x.Remove())
        xdoc.Save(psnPath.ToString())
        psnPath
    
    let buildSnapshot proj (psnPath : FilePath) = 
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
            (psnPath |> Path.getDirectoryName, "tddstud10wrapper.proj" |> FilePath) ||> Path.combine
        File.WriteAllText(wrapperProjectPath.ToString(), wrapperProject)
        let properties = 
            Map.empty
            |> Map.add "_TddStud10Project" (psnPath.ToString())
            |> Map.add "_TddStud10Target" "Build"
        
        let l = BuildLogger()
        let p = ProjectInstance(wrapperProjectPath.ToString(), properties :> IDictionary<_, _>, "12.0")
        let status = p.Build([| "_TddStud10BuildProject" |], [ l :> ILogger ])
        let outputs = p.GetItems("_TddStud10TargetOutputs") |> Seq.map (fun i -> i.EvaluatedInclude |> FilePath)
        let is = l.Warnings |> Seq.append l.Errors
        if (status) then
            BuildSuccess(outputs, is)
        else
            BuildFailure(is)
