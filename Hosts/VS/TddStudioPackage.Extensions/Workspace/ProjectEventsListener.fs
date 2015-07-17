namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Workspace

open Microsoft.VisualStudio
open Microsoft.VisualStudio.Shell.Interop
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions
open System

type ProjectEventsListener(trackPD : IVsTrackProjectDocuments2) as self = 
    let mutable disposed = false
    let pdwCookie = ref 0u
    let hr = trackPD.AdviseTrackProjectDocumentsEvents(self, pdwCookie)
    do ErrorHandlerExtensions.ThrowOnFailure(hr)
    
    interface IProjectEventsListener with
        member __.ProjectItemAdded : IObservable<ProjectItem> = failwith "Not implemented yet"
        member __.ProjectItemRemoved : IObservable<ProjectItem> = failwith "Not implemented yet"
    
    interface IVsTrackProjectDocumentsEvents2 with
        member __.OnAfterAddDirectoriesEx(cProjects : int, cDirectories : int, rgpProjects : IVsProject [], 
                                          rgFirstIndices : int [], rgpszMkDocuments : string [], 
                                          rgFlags : VSADDDIRECTORYFLAGS []) : int = VSConstants.S_OK
        member __.OnAfterAddFilesEx(cProjects : int, cFiles : int, rgpProjects : IVsProject [], rgFirstIndices : int [], 
                                    rgpszMkDocuments : string [], rgFlags : VSADDFILEFLAGS []) : int = VSConstants.S_OK
        member __.OnAfterRemoveDirectories(cProjects : int, cDirectories : int, rgpProjects : IVsProject [], 
                                           rgFirstIndices : int [], rgpszMkDocuments : string [], 
                                           rgFlags : VSREMOVEDIRECTORYFLAGS []) : int = VSConstants.S_OK
        member __.OnAfterRemoveFiles(cProjects : int, cFiles : int, rgpProjects : IVsProject [], rgFirstIndices : int [], 
                                     rgpszMkDocuments : string [], rgFlags : VSREMOVEFILEFLAGS []) : int = 
            VSConstants.S_OK
        member __.OnAfterRenameDirectories(cProjects : int, cDirs : int, rgpProjects : IVsProject [], 
                                           rgFirstIndices : int [], rgszMkOldNames : string [], 
                                           rgszMkNewNames : string [], rgFlags : VSRENAMEDIRECTORYFLAGS []) : int = 
            VSConstants.S_OK
        member __.OnAfterRenameFiles(cProjects : int, cFiles : int, rgpProjects : IVsProject [], rgFirstIndices : int [], 
                                     rgszMkOldNames : string [], rgszMkNewNames : string [], 
                                     rgFlags : VSRENAMEFILEFLAGS []) : int = VSConstants.S_OK
        member __.OnAfterSccStatusChanged(cProjects : int, cFiles : int, rgpProjects : IVsProject [], 
                                          rgFirstIndices : int [], rgpszMkDocuments : string [], 
                                          rgdwSccStatus : uint32 []) : int = VSConstants.S_OK
        member __.OnQueryAddDirectories(pProject : IVsProject, cDirectories : int, rgpszMkDocuments : string [], 
                                        rgFlags : VSQUERYADDDIRECTORYFLAGS [], 
                                        pSummaryResult : VSQUERYADDDIRECTORYRESULTS [], 
                                        rgResults : VSQUERYADDDIRECTORYRESULTS []) : int = VSConstants.S_OK
        member __.OnQueryAddFiles(pProject : IVsProject, cFiles : int, rgpszMkDocuments : string [], 
                                  rgFlags : VSQUERYADDFILEFLAGS [], pSummaryResult : VSQUERYADDFILERESULTS [], 
                                  rgResults : VSQUERYADDFILERESULTS []) : int = VSConstants.S_OK
        member __.OnQueryRemoveDirectories(pProject : IVsProject, cDirectories : int, rgpszMkDocuments : string [], 
                                           rgFlags : VSQUERYREMOVEDIRECTORYFLAGS [], 
                                           pSummaryResult : VSQUERYREMOVEDIRECTORYRESULTS [], 
                                           rgResults : VSQUERYREMOVEDIRECTORYRESULTS []) : int = VSConstants.S_OK
        member __.OnQueryRemoveFiles(pProject : IVsProject, cFiles : int, rgpszMkDocuments : string [], 
                                     rgFlags : VSQUERYREMOVEFILEFLAGS [], pSummaryResult : VSQUERYREMOVEFILERESULTS [], 
                                     rgResults : VSQUERYREMOVEFILERESULTS []) : int = VSConstants.S_OK
        member __.OnQueryRenameDirectories(pProject : IVsProject, cDirs : int, rgszMkOldNames : string [], 
                                           rgszMkNewNames : string [], rgFlags : VSQUERYRENAMEDIRECTORYFLAGS [], 
                                           pSummaryResult : VSQUERYRENAMEDIRECTORYRESULTS [], 
                                           rgResults : VSQUERYRENAMEDIRECTORYRESULTS []) : int = VSConstants.S_OK
        member __.OnQueryRenameFiles(pProject : IVsProject, cFiles : int, rgszMkOldNames : string [], 
                                     rgszMkNewNames : string [], rgFlags : VSQUERYRENAMEFILEFLAGS [], 
                                     pSummaryResult : VSQUERYRENAMEFILERESULTS [], 
                                     rgResults : VSQUERYRENAMEFILERESULTS []) : int = VSConstants.S_OK
    
    member private __.Dispose(disposing : _) = 
        if not disposed then 
            if (disposing) then 
                let hr = trackPD.UnadviseTrackProjectDocumentsEvents(!pdwCookie)
                do ErrorHandlerExtensions.ThrowOnFailure(hr)
            disposed <- true
    
    override x.Finalize() = x.Dispose(false)
    interface IDisposable with
        member x.Dispose() : _ = 
            x.Dispose(true)
            GC.SuppressFinalize(x)
