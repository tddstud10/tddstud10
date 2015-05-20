module R4nd0mApps.TddStud10.TestExecution.TestPlatformExtensions

open System.Reflection
open System
open System.IO
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Generic
open R4nd0mApps.TddStud10.TestHost.Diagnostics

let getLocalPath() = 
    (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName

let loadTestAdapter() = 
    let aPath = getLocalPath() |> fun lp -> Path.Combine(lp, "xunit20\\xunit.runner.visualstudio.testadapter.dll")
    Logger.logInfof "Loading Test Adapter from %s" aPath
    let ta = 
        Assembly.LoadFrom(aPath)
        |> fun a -> a.GetType("Xunit.Runner.VisualStudio.TestAdapter.VsTestRunner")
        |> fun t -> Activator.CreateInstance(t)
    Logger.logInfof "Test Adapter loaded"
    ta

let createDiscoveryContext() = 
    { new IDiscoveryContext with
          member x.RunSettings : IRunSettings = 
              Logger.logErrorf "TestPlatform: RunSettings call was unexpected"
              failwith "Not implemented yet" }

let createMessageLogger() = 
    { new IMessageLogger with
          member x.SendMessage(testMessageLevel : TestMessageLevel, message : string) : unit =
              Logger.logErrorf "TestPlatform: SendMessage call was unexpected : [%O] %s" testMessageLevel message }

let createDiscoverySink td = 
    { new ITestCaseDiscoverySink with
          member x.SendTestCase(discoveredTest : TestCase) : unit = td (discoveredTest) }

let createRunContext() = 
    { new IRunContext with
          
          member x.GetTestCaseFilter(supportedProperties : IEnumerable<string>, 
                                     propertyProvider : Func<string, TestProperty>) : ITestCaseFilterExpression = 
              Logger.logErrorf "TestPlatform: GetTestCaseFilter call was unexpected"
              null
          
          member x.InIsolation : bool = 
              Logger.logErrorf "TestPlatform: InIsolation call was unexpected"
              false
          
          member x.IsBeingDebugged : bool = 
              Logger.logErrorf "TestPlatform: IsBeingDebugged call was unexpected"
              false
          
          member x.IsDataCollectionEnabled : bool = 
              Logger.logErrorf "TestPlatform: IsDataCollectionEnabled call was unexpected"
              false
          
          member x.KeepAlive : bool = 
              Logger.logErrorf "TestPlatform: KeepAlive call was unexpected"
              false
          
          member x.RunSettings : IRunSettings = 
              Logger.logErrorf "TestPlatform: RunSettings call was unexpected"
              null
          
          member x.SolutionDirectory : string = 
              Logger.logErrorf "TestPlatform: SolutionDirectory call was unexpected"
              null
          
          member x.TestRunDirectory : string = 
              Logger.logErrorf "TestPlatform: TestRunDirectory call was unexpected"
              null }

let createFrameworkHandle te = 
    { new IFrameworkHandle with
          
          member x.EnableShutdownAfterTestRun 
              with get () = true : bool
              and set (v : bool) = () : unit
          
          member x.LaunchProcessWithDebuggerAttached(filePath : string, workingDirectory : string, arguments : string, 
                                                     environmentVariables : IDictionary<string, string>) : int = 
              Logger.logErrorf "TestPlatform: LaunchProcessWithDebuggerAttached call was unexpected"
              0
          
          member x.RecordAttachments(attachmentSets : IList<AttachmentSet>) : unit = 
              Logger.logErrorf "TestPlatform: RecordAttachments call was unexpected"
          member x.RecordEnd(testCase : TestCase, outcome : TestOutcome) : unit = ()
          member x.RecordResult(testResult : TestResult) : unit = te (testResult)
          member x.RecordStart(testCase : TestCase) : unit = ()
          member x.SendMessage(testMessageLevel : TestMessageLevel, message : string) : unit = 
              Logger.logErrorf "TestPlatform: SendMessage call was unexpected : [%O] %s" testMessageLevel message }
