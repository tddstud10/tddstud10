module R4nd0mApps.TddStud10.TestExecution.TestPlatformExtensions

open System.Reflection
open System
open System.IO
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging
open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System.Collections.Generic
open R4nd0mApps.TddStud10.Common.Domain

let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger

let getLocalPath() = 
    Assembly.GetExecutingAssembly().CodeBase
    |> fun cb -> (new Uri(cb)).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName
    |> FilePath

let loadTestAdapter (FilePath binDir) = 
    let aPath = Path.Combine(binDir, "xunit.runner.visualstudio.testadapter.dll")
    logger.logInfof "Loading Test Adapter from %s" aPath
    let ta = 
        Assembly.LoadFrom(aPath)
        |> fun a -> a.GetType("Xunit.Runner.VisualStudio.TestAdapter.VsTestRunner")
        |> fun t -> Activator.CreateInstance(t)
    logger.logInfof "Test Adapter loaded"
    ta

let toDTestCase (tc : TestCase) =
    { DtcId = tc.Id
      FullyQualifiedName = tc.FullyQualifiedName
      DisplayName = tc.DisplayName
      Source = FilePath tc.Source
      CodeFilePath = FilePath tc.CodeFilePath
      LineNumber = DocumentCoordinate tc.LineNumber }

let toDTestOutcome = function
    | TestOutcome.None -> TONone
    | TestOutcome.Passed -> TOPassed
    | TestOutcome.Failed -> TOFailed
    | TestOutcome.Skipped -> TOSkipped
    | TestOutcome.NotFound -> TONotFound
    | o -> failwithf "Unknown TestOutcome: %O" o

let toDTestResult (tr : TestResult) =
    { DisplayName = tr.DisplayName
      TestCase = toDTestCase tr.TestCase
      Outcome = toDTestOutcome tr.Outcome
      ErrorStackTrace = tr.ErrorStackTrace
      ErrorMessage = tr.ErrorMessage }

let createRunSettings() =
    { new IRunSettings with
          member __.GetSettings(_: string): ISettingsProvider = 
              failwith "Not implemented yet"
          member __.SettingsXml: string = 
              "<RunSettings/>" }

let createDiscoveryContext() = 
    { new IDiscoveryContext with
          member __.RunSettings : IRunSettings = 
              createRunSettings() }

let createMessageLogger() = 
    { new IMessageLogger with
          member __.SendMessage(testMessageLevel : TestMessageLevel, message : string) : unit =
              logger.logWarnf "TestPlatform: SendMessage call was unexpected : [%O] %s" testMessageLevel message }

let createDiscoverySink td = 
    { new ITestCaseDiscoverySink with
          member __.SendTestCase(discoveredTest : TestCase) : unit = discoveredTest |> td }

let createRunContext() = 
    { new IRunContext with
          
          member __.GetTestCaseFilter(_ : IEnumerable<string>, 
                                      _ : Func<string, TestProperty>) : ITestCaseFilterExpression = 
              logger.logWarnf "TestPlatform: GetTestCaseFilter call was unexpected"
              null
          
          member __.InIsolation : bool = 
              logger.logWarnf "TestPlatform: InIsolation call was unexpected"
              false
          
          member __.IsBeingDebugged : bool = 
              logger.logWarnf "TestPlatform: IsBeingDebugged call was unexpected"
              false
          
          member __.IsDataCollectionEnabled : bool = 
              logger.logWarnf "TestPlatform: IsDataCollectionEnabled call was unexpected"
              false
          
          member __.KeepAlive : bool = 
              logger.logWarnf "TestPlatform: KeepAlive call was unexpected"
              false
          
          member __.RunSettings : IRunSettings = 
              createRunSettings()   
          
          member __.SolutionDirectory : string = 
              logger.logWarnf "TestPlatform: SolutionDirectory call was unexpected"
              null
          
          member __.TestRunDirectory : string = 
              logger.logWarnf "TestPlatform: TestRunDirectory call was unexpected"
              null }

let createFrameworkHandle te = 
    { new IFrameworkHandle with
          
          member __.EnableShutdownAfterTestRun 
              with get () = true : bool
              and set (_ : bool) = () : unit
          
          member __.LaunchProcessWithDebuggerAttached(_ : string, _ : string, _ : string, 
                                                      _ : IDictionary<string, string>) : int = 
              logger.logWarnf "TestPlatform: LaunchProcessWithDebuggerAttached call was unexpected"
              0
          
          member __.RecordAttachments(_ : IList<AttachmentSet>) : unit = 
              logger.logWarnf "TestPlatform: RecordAttachments call was unexpected"
          member __.RecordEnd(_ : TestCase, _ : TestOutcome) : unit = ()
          member __.RecordResult(testResult : TestResult) : unit = testResult |> (toDTestResult >> te) 
          member __.RecordStart(_ : TestCase) : unit = ()
          member __.SendMessage(testMessageLevel : TestMessageLevel, message : string) : unit = 
              logger.logWarnf "TestPlatform: SendMessage call was unexpected : [%O] %s" testMessageLevel message }
