using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.TestExecution.Adapters;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.TestRuntime;

namespace R4nd0mApps.TddStud10.TestHost
{
    public static class Program
    {
        private static bool _debuggerAttached = Debugger.IsAttached;
        private static void LogInfo(string format, params object[] args)
        {
            Logger.I.LogInfo(format, args);
        }

        private static void LogError(string format, params object[] args)
        {
            Logger.I.LogError(format, args);
        }

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] args)
        {
            LogInfo("TestHost: Entering Main.");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            var rsp = RunStartParamsExtensions.create(new DateTime(0), FilePath.NewFilePath(args[0]));
            var command = args[1];
            var codeCoverageStore = args[2];
            var testResultsStore = args[3];
            var discoveredUnitTestsStore = args[4];
            var testFailureInfoStore = args[5];

            var allTestsPassed = _debuggerAttached
                ? RunTests(rsp, testResultsStore, discoveredUnitTestsStore, testFailureInfoStore)
                : ExecuteTestWithCoverageDataCollection(() => RunTests(rsp, testResultsStore, discoveredUnitTestsStore, testFailureInfoStore), codeCoverageStore);

            LogInfo("TestHost: Exiting Main.");
            return allTestsPassed ? 0 : 1;
        }

        private static bool ExecuteTestWithCoverageDataCollection(Func<bool> runTests, string codeCoverageStore)
        {
            bool allTestsPassed = true;
            var ccServer = new CoverageDataCollector();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                LogInfo("TestHost: Created Service Host.");
                string address = Marker.CreateCodeCoverageDataCollectorEndpointAddress();
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICoverageDataCollector), binding, address);
                serviceHost.Open();
                LogInfo("TestHost: Opened _channel.");

                allTestsPassed = runTests();
                LogInfo("TestHost: Finished running test cases.");
            }
            ccServer.CoverageData.Serialize(FilePath.NewFilePath(codeCoverageStore));
            return allTestsPassed;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError("Exception thrown in InvokeEngine: {0}.", e.ExceptionObject);
        }

        private static bool RunTests(RunStartParams rsp, string testResultsStore, string discoveredUnitTestsStore, string testFailureInfoStore)
        {
            Stopwatch stopWatch = new Stopwatch();

            LogInfo("TestHost executing tests...");
            stopWatch.Start();
            var testResults = new PerTestIdResults();
            var testFailureInfo = new PerDocumentLocationTestFailureInfo();
            var perAssemblyTestIds = PerAssemblyTestCases.Deserialize(FilePath.NewFilePath(discoveredUnitTestsStore));
            Parallel.ForEach(
                perAssemblyTestIds.Keys,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                asm =>
                {
                    if (perAssemblyTestIds[asm].Count == 0)
                    {
                        return;
                    }

                    LogInfo("Executing tests in {0}: Start.", asm);
                    var exec = new XUnitTestExecutor();
                    exec.TestExecuted.AddHandler(
                        new FSharpHandler<TestResult>(
                            (o, ea) =>
                            {
                                NoteTestResults(testResults, ea);
                                NoteTestFailureInfo(rsp, testFailureInfo, ea);
                            }));
                    exec.ExecuteTests(perAssemblyTestIds[asm]);
                    LogInfo("Executing tests in {0}: Done.", asm);
                });

            if (!_debuggerAttached)
            {
                testResults.Serialize(FilePath.NewFilePath(testResultsStore));
                //testFailureInfo.Serialize(FilePath.NewFilePath(testFailureInfoStore));
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            LogInfo("Done TestHost executing tests! [" + elapsedTime + "]");
            LogInfo("");

            var rrs =
                from tr in testResults
                from rr in tr.Value
                where rr.result.Outcome == TestOutcome.Failed
                select rr;

            return !rrs.Any();
        }

        private static void NoteTestFailureInfo(RunStartParams rsp, PerDocumentLocationTestFailureInfo pdtfi, TestResult tr)
        {
            LogInfo("Noting Test Failure Info: {0} - {1}", tr.DisplayName, tr.Outcome);

            TestFailureInfoExtensions.create(rsp, tr)
            .Aggregate(
                pdtfi,
                (acc, e) =>
                {
                    acc
                    .GetOrAdd(e.Item1, _ => new ConcurrentBag<TestFailureInfo>())
                    .Add(e.Item2);
                    return acc;
                });
        }

        private static void NoteTestResults(PerTestIdResults testResults, TestResult tr)
        {
            LogInfo("Noting Test Result: {0} - {1}", tr.DisplayName, tr.Outcome);

            var testId = new TestId(
                FilePath.NewFilePath(tr.TestCase.Source),
                FilePath.NewFilePath(tr.TestCase.CodeFilePath),
                DocumentCoordinate.NewDocumentCoordinate(tr.TestCase.LineNumber));

            var results = testResults.GetOrAdd(testId, _ => new ConcurrentBag<TestRunResult>());
            results.Add(new TestRunResult(tr));
        }
    }
}
