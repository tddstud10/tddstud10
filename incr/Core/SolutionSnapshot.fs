namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open System

module QuickGraphExtensions = 
    let clone g = 
        let g' = AdjacencyGraph<_, _>()
        AlgorithmExtensions.Clone(g, Func<_, _>(fun v -> v), Func<_, _, _, _>(fun e _ _ -> e), g')
        g'
    
    let visitInDependencyOrder nodeProcessor (dg : AdjacencyGraph<_, _>) = 
        let rec dependencyLoop np (dg : AdjacencyGraph<_, _>) = 
            AlgorithmExtensions.Sinks<ProjectId, _>(dg)
            |> Seq.tryHead
            |> Option.iter (fun p -> 
                   p |> np
                   dg.RemoveVertex(p) |> ignore
                   dependencyLoop np dg)
        dependencyLoop nodeProcessor dg

type SolutionSnapshot() = 
    let beginCreateSnapshot = new Event<_>()
    let beginCreateProjectSnapshot = new Event<_>()
    let endCreateProjectSnapshot = new Event<_>()
    let endCreateSnapshot = new Event<_>()
    member public __.BeginCreateSnapshot = beginCreateSnapshot.Publish
    member public __.BeginCreateProjectSnapshot = beginCreateProjectSnapshot.Publish
    member public __.EndCreateProjectSnapshot = endCreateProjectSnapshot.Publish
    member public __.EndCreateSnapshot = endCreateSnapshot.Publish
    member __.Load(sln : Solution) : Async<unit> = 
        let processProject (_ : Map<_, _>) pid = 
            Common.safeExec (fun () -> beginCreateProjectSnapshot.Trigger(pid))
            Threading.Thread.Sleep(500)
            Common.safeExec (fun () -> endCreateProjectSnapshot.Trigger(pid))
        async { 
            Common.safeExec (fun () -> beginCreateSnapshot.Trigger(sln))
            sln.DependencyGraph
            |> QuickGraphExtensions.clone
            |> QuickGraphExtensions.visitInDependencyOrder (processProject sln.Projects)
            Common.safeExec (fun () -> endCreateSnapshot.Trigger(sln))
        }
