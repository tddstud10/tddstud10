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
