namespace R4nd0mApps.TddStud10.Common.Domain

open System

type Solution = 
    { path : FilePath }

type ProjectItem = 
    { path : FilePath }

type Project = 
    { path : FilePath
      items : ProjectItem seq }

type ISolutionEventsListener = 
    inherit IDisposable
    abstract SolutionOpened : IObservable<Solution>
    abstract SolutionClosed : IObservable<Solution>
    abstract ProjectAdded : IObservable<Project>
    abstract ProjectRemoved : IObservable<Project>

type IProjectEventsListener = 
    inherit IDisposable
    abstract ProjectItemAdded : IObservable<ProjectItem>
    abstract ProjectItemRemoved : IObservable<ProjectItem>

type IProjectItemEventsListener = 
    inherit IDisposable
    abstract ProjectItemChanged : IObservable<ProjectItem>

#if DONT_COMPILE

SolutionEventListener -> Workspace -> BatchEvents -> Scheduler -> Engine [Snapshoting -> Build -> Test]

Worspace
- Does:
  - StartMonitoring
  - StopMonitoring
    - Disposes all collaboratros
    - Clears workspace
  - *Configure
    - *Project Filters
  - AddProject
  - RemoveProject
  - AddProjectItem
  - RemoveProjectItem
  - Fires Event For every ProjectItem changes (Build Order of changed Project, Items Changed per project)
- Contains
  - ISolutionEventsListener
  - IProjectEventsListener
  - map<Project,seq<ProjectItem * IProjectItemEventsListener>>

-------- Monitoring --------

Start: Sources of Raw Events
- ISolutionEventsListener 
  - SolutionEventsListener [IVsSolutionEvents4 - Project Add/Remove/Rename]
- IProjectEventsListener 
  - ProjectEventsListener [IVsTrackProjectDocumentsEvents2 - ProjectItem Add/Remove/Rename]
- IProjectItemEventsListener
  - FileSystemEventsListener [FileSystemWatcher - ProjectItem Changed]
  - *TextBufferEventsListener [TextBuffer.TextChanged - ProjectItem Changed]

Operations:
- Start Monitoring
- Stop Monitoring
- Configure Monitoring [i.e. project to ignore]

Finally: Stream of events
Batched
Source Path of modified entity
Type of entity
Type of change
Build Order
[opt] Contents of item changed

-------- Snapshoting --------
- Copy to workspace
- ProjectItem path fix up

-------- Per assembly pipeline --------
- Cancellation on new event [cancel build, cancel unit test]



TODO:
- Incremental Pipeline
  - Core [FS/VS saves only]
    - In - Solution
    - Out 
      - [a] Build order 
      - [b] Interesting Change notifications
        - Solution
          - Add - Create & Initialize Session
          - Remove - Destroy Session
          - Rename = Add + Remove 
          - Update - n/a
        - Project
          - Add - Update BuildOrder, Add project to engine, FireItemChange
          - Remove - Update BuilderOrder, Remove project from engine, FireItemChange
          - Rename = Add + Remove
          - Update - Update BuildOrder, FireItemChange
        - ProjectItem
          - Add - FireItemChange
          - Remove - FireItemChange
          - Rename - FireItemChange
          - Update - FireItemChange
      - Notes
        - Package load Workspace + Engine
    - VisualStudioItemsSource [hosted in vs, writes in output window which items changed + projects to build]
    - Modes
      - First time: all changes 
      - Subsquently: incremental
      - Reset goes back to first time
  - DisableProject via per project config
  - Snapshot projects
    - Copy to tddstud10
    - Fixup paths: project dependencies, reference assemblies
    - Additional items via per project config
  - Batch Events
  - Per assembly pipeline
    - Adapt pipeline to make to per assembly
    - Init session
    - Resync session
    - Merge Datastore
  - Enable parallelism
    - Adapt notification icon
  - Realtime edits without save
  - Per test updates

#endif