module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Workspace.ProjectEventsListenerTests

open Foq
open Microsoft.VisualStudio.Shell.Interop
open R4nd0mApps.TddStud10.Hosts.Common.TestCode
open System
open Xunit

(*
v Subscribes to events during create, correctly unsubscribes on dispose
- OnAfterAddFilesEx raises ProjectItemAdded event
- OnAfterRemoveFiles raises ProjectItemRemoved event
- OnAfterRenameFiles raises ProjectItemRemoved and then ProjectItemAdded events
- Throws exception when subscribe fails
- Dont throw exception when unsubscribe fails

Question:
- Does OnAfterRenameDirectories result in rename file?
 *)

[<Fact>]
let ``Subscribes to events during create, correctly unsubscribes on dispose``() = 
    let tpd = SpyVsTrackProjectDocuments2()
    let pel = new ProjectEventsListener(tpd :> IVsTrackProjectDocuments2)
    Assert.Equal(pel :> IVsTrackProjectDocumentsEvents2, tpd.AdviseEventsCalledWith)
    (pel :> IDisposable).Dispose()
    Assert.True(tpd.VerifyUnadviseEvents())

[<Fact>]
let ``OnAfterAddFilesEx raises ProjectItemAdded event``() = 
    let tpd = SpyVsTrackProjectDocuments2()
    use pel = new ProjectEventsListener(tpd :> IVsTrackProjectDocuments2) 
    let tpde = pel :> IVsTrackProjectDocumentsEvents2 
    tpde.OnAfterAddFilesEx(1, 1, )