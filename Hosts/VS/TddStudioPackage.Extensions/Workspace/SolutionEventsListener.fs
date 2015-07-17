namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Workspace

open Microsoft.VisualStudio
open Microsoft.VisualStudio.Shell.Interop
open R4nd0mApps.TddStud10.Common.Domain
open System

type SolutionEventsListener(sln : IVsSolution) = 
    
    interface ISolutionEventsListener with
        member __.Dispose() : unit = failwith "Not implemented yet"
        member __.ProjectAdded : IObservable<Project> = failwith "Not implemented yet"
        member __.ProjectRemoved : IObservable<Project> = failwith "Not implemented yet"
        member __.SolutionClosed : IObservable<Solution> = failwith "Not implemented yet"
        member __.SolutionOpened : IObservable<Solution> = failwith "Not implemented yet"
    
    interface IVsSolutionEvents with
        member __.OnAfterCloseSolution(pUnkReserved : obj) : int = VSConstants.S_OK
        member __.OnAfterLoadProject(pStubHierarchy : IVsHierarchy, pRealHierarchy : IVsHierarchy) : int = 
            VSConstants.S_OK
        member __.OnAfterOpenProject(pHierarchy : IVsHierarchy, fAdded : int) : int = VSConstants.S_OK
        member __.OnAfterOpenSolution(pUnkReserved : obj, fNewSolution : int) : int = VSConstants.S_OK
        member __.OnBeforeCloseProject(pHierarchy : IVsHierarchy, fRemoved : int) : int = VSConstants.S_OK
        member __.OnBeforeCloseSolution(pUnkReserved : obj) : int = VSConstants.S_OK
        member __.OnBeforeUnloadProject(pRealHierarchy : IVsHierarchy, pStubHierarchy : IVsHierarchy) : int = 
            VSConstants.S_OK
        member __.OnQueryCloseProject(pHierarchy : IVsHierarchy, fRemoving : int, pfCancel : byref<int>) : int = 
            VSConstants.S_OK
        member __.OnQueryCloseSolution(pUnkReserved : obj, pfCancel : byref<int>) : int = VSConstants.S_OK
        member __.OnQueryUnloadProject(pRealHierarchy : IVsHierarchy, pfCancel : byref<int>) : int = VSConstants.S_OK
