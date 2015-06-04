module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.TestMarkerTagger

open System
open System.Runtime.CompilerServices
open Xunit

#if DONT_COMPILE
DataStore.TestCasesUpdated 
- fires tagsChanged

GetTags
- If span is empty, return empty
- two spans (one in datstore and one not in datastore) - returns 1 tagspan with testcase, snapshotspan of start/length
- if filepath not found return empty
 
#endif
