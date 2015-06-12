namespace R4nd0mApps.TddStud10.TestHost

open System
open R4nd0mApps.TddStud10.TestRuntime
open System.ServiceModel
open R4nd0mApps.TddStud10.TestHost.Diagnostics
open R4nd0mApps.TddStud10.Common.Domain
open System.Collections.Concurrent

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type CoverageDataCollector() = 
    let coverageData = new PerAssemblySequencePointsCoverage()
    let tempStore = new ConcurrentDictionary<string, ConcurrentBag<string * string * string>>()
    
    let enterSequencePoint testRunId assemblyId methodMdRid spId = 
        if testRunId = null || assemblyId = null || methodMdRid = null || spId = null then 
            Logger.logErrorf "CoverageDataCollector: EnterSequencePoint: Invalid payload: %s %s %s %s" testRunId 
                assemblyId methodMdRid spId
        else 
            let list = tempStore.GetOrAdd(testRunId, fun _ -> ConcurrentBag<_>())
            list.Add(assemblyId, methodMdRid, spId)
    
    let exitUnitTest testRunId source document line = 
        if testRunId = null || source = null || document = null || line = null then 
            Logger.logErrorf "CoverageDataCollector: ExitUnitTest: Unexpected payload in ExitUnitTest: %s %s %s %s" 
                testRunId source document line
        else 
            let exists, sps = tempStore.TryRemove(testRunId)
            if not exists then 
                Logger.logErrorf 
                    "CoverageDataCollector: ExitUnitTest: Did not have any sequence points in thread %s for %s,%s,%s." 
                    testRunId source document line
            else 
                let addSPC a m s = 
                    let asmId = AssemblyId(Guid.Parse(a))
                    let l = coverageData.GetOrAdd(asmId, fun _ -> ConcurrentBag<_>())
                    l.Add { methodId = 
                                { assemblyId = asmId
                                  mdTokenRid = MdTokenRid(UInt32.Parse(m)) }
                            sequencePointId = SequencePointId(Int32.Parse(s))
                            testRunId = 
                                { testId = 
                                      { source = source |> FilePath
                                        document = document |> FilePath
                                        line = DocumentCoordinate(Int32.Parse(line)) }
                                  testRunInstanceId = TestRunInstanceId(Int32.Parse(testRunId)) } }
                    |> ignore
                Async.Parallel [ for sp in sps -> async { return sp |||> addSPC } ]
                |> Async.RunSynchronously
                |> ignore
                Logger.logInfof "CoverageDataCollector: Servicing ExitUnitTest: %s,%s,%s. Sequence Points = %d" source 
                    document line sps.Count
    
    member __.CoverageData = coverageData
    interface ICoverageDataCollector with
        member __.EnterSequencePoint(testRunId : string, assemblyId : string, methodMdRid : string, spId : string) : unit = 
            enterSequencePoint testRunId assemblyId methodMdRid spId
        member __.ExitUnitTest(testRunId : string, source : string, document : string, line : string) : unit = 
            exitUnitTest testRunId source document line
        member x.Ping() : unit = Logger.logInfof "CoverageDataCollector - responding to ping."
