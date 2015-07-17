module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Workspace.SolutionEventsListenerTests

(*
- Subscribes to events during create, unsbscribes from events on dispose
- Throws exception when subscribe fails
- Throws exception when unsubscribe fails
- OnAfterOpenProject raises ProjectAdded
- OnAfterLoadProject raises ProjectAdded
- OnBeforeCloseProject raises ProjectRemoved
- OnBeforeUnloadProject raises ProjectRemoved
- OnAfterOpenSolution raises SolutionOpened
- OnAfterCloseSolution raises SolutionClosed
- Events not fired if solution is not fully loaded
 *)
