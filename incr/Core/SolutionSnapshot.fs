namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage

open FSharpx.Collections
open QuickGraph
open QuickGraph.Algorithms
open R4nd0mApps.TddStud10.Common.Domain
open System

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
        // TODO: Move this to QuickGraphExtensions
        let cloneGraph g = 
            let cg = AdjacencyGraph<_, _>()
            AlgorithmExtensions.Clone(g, Func<_, _>(fun v -> v), Func<_, _, _, _>(fun e _ _ -> e), cg)
            cg
        
        let processProject p = 
            Common.safeExec (fun () -> beginCreateProjectSnapshot.Trigger(p))
            Threading.Thread.Sleep(500)
            Common.safeExec (fun () -> endCreateProjectSnapshot.Trigger(p))
        
        let rec dependencyLoop (dg : AdjacencyGraph<_, _>) = 
            AlgorithmExtensions.Sinks<ProjectId, _>(dg)
            |> Seq.tryHead
            |> Option.iter (fun p -> 
                   p |> processProject
                   dg.RemoveVertex(p) |> ignore
                   dependencyLoop dg)
        
        async { 
            Common.safeExec (fun () -> beginCreateSnapshot.Trigger(sln))
            sln.DependencyGraph
            |> cloneGraph
            |> dependencyLoop
            Common.safeExec (fun () -> endCreateSnapshot.Trigger(sln))
        }
